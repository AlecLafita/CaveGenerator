using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;


/** Abstract class that manages the cave generation. The differents subclasses differ on the why the holes/tunnels are extruded **/
abstract public class AbstractGenerator {

	protected List<Geometry.Mesh> proceduralMesh; //Mesh that will be modified during the cave generation
	protected Geometry.Mesh actualMesh; //Mesh coresponding to the actual tunnel being generated
	protected float initialTunelHoleProb; //holes can be created depending on this probability [0-1]
	protected int maxHoles; //How many times a hole can be extruded and behave like a tunnel, acts as a countdown
	protected int maxExtrudeTimes; //TODO: consider to deccrement this value as holes are created, or some random function that handles this
	protected InitialPolyline gatePolyline; //Polyline where the cave starts from

	/**Creates the instance without initializing anything **/
	public AbstractGenerator() {
		proceduralMesh = new List<Geometry.Mesh> ();
	}

	private int smoothIterations = 3;
	/**Initialize, being the arguments the needed parameters for the generator **/
	public void initialize(InitialPolyline iniPol, float initialTunelHoleProb, int maxHoles, int maxExtrudeTimes) {
		for (int i = 0; i < smoothIterations;++i)
			((InitialPolyline)iniPol).smoothMean ();
		((InitialPolyline)iniPol).generateUVs ();
		//initializeTunnel (ref iniPol);

		gatePolyline = iniPol;
		this.initialTunelHoleProb = initialTunelHoleProb;
		this.maxHoles = maxHoles;
		this.maxExtrudeTimes = maxExtrudeTimes;
	}

	/**Initializes the tunnel initial polyline, returning the corresponding mesh and setting it as the actual one**/
	protected Geometry.Mesh initializeTunnel(ref Polyline iniPol) {
		/*
		for (int i = 0; i < smoothIterations;++i)
			((InitialPolyline)iniPol).smoothMean ();
		//((InitialPolyline)iniPol).generateUVs ();
*/
		//Smoth the hole polyline? (but should be smoothed too on the tunnel where the hole is done)

		//This piece of code is valid either for the projection and the no projection version
		((InitialPolyline)iniPol).initializeIndices();
		//Create the new mesh with the hole polyline
		Geometry.Mesh m = new Geometry.Mesh (iniPol);
		proceduralMesh.Add (m);
		actualMesh = m;


		return m;
	}

	/**Generates the cave **/
	abstract public void generate ();

	/**Returns the generated mesh **/
	public List<Geometry.Mesh> getMesh() {
		return proceduralMesh;
	}

	public float UVfactor = 50.0f;
	/**It creates a new polyline from an exsiting one, applying the corresponding operations**/
	protected Polyline extrude(ExtrusionOperations operation, Polyline originPoly) {

		//Create the new polyline from the actual one
		Polyline newPoly = new Polyline(originPoly.getSize());
		Vector3 direction = operation.applyDirection ();
		float distance = operation.applyDistance ();
		//Generate the UVS of the new polyline from the coordinates of the original and on the same
		//same direction that the extrusion, as if it was a projection to XZ plane
		//Vector2 UVincr = new Vector2(direction.x,direction.z);
		Vector2 UVincr = new Vector2(0.0f,1.0f);
		UVincr.Normalize ();
		UVincr *= (distance / UVfactor);
		for (int i = 0; i < originPoly.getSize(); ++i) { //Generate the new vertices
			//Add vertex to polyline
			newPoly.extrudeVertex(i, originPoly.getVertex(i).getPosition(), direction, distance);
			//Add the index to vertex
			newPoly.getVertex(i).setIndex(actualMesh.getNumVertices() + i);
			//Add UV
			newPoly.getVertex(i).setUV(originPoly.getVertex(i).getUV() + UVincr);
		}

		//Apply operations, if any
		if (operation.scaleOperation()) {
			newPoly.scale(operation.applyScale ());
		}
		if (operation.rotationOperation ()) {
			newPoly.rotate(operation.applyRotate ());
		}
			
		//Check there is no intersection
		if (IntersectionsController.Instance.doIntersect (originPoly, newPoly, operation.getCanIntersect())) {
			return null;
		}

		return newPoly;
	}

	/** Makes a hole betwen two polylines and return this hole as a new polyline **/
	protected Polyline makeHole(Polyline originPoly, Polyline destinyPoly) {

		//TODO: more than one hole, Make two holes on same polylines pairs can cause intersections!
			//could take the negate direction of the already make hole and try to do a new one

		// FIRST: Decide where the hole will be done, take advantatge indices
		// on the two polylines are at the same order (the new is kind of a projection of the old)
		int sizeHole; int firstIndex;
		//DecisionGenerator.Instance.whereToDig (originPoly.getSize(), out sizeHole, out firstIndex);
		DecisionGenerator.Instance.whereToDig (originPoly, out sizeHole, out firstIndex);
		if (sizeHole < DecisionGenerator.Instance.holeMinVertices)
			return null;
		
		//SECOND: Create the hole polyline by marking and adding the hole vertices (from old a new polylines)
		InitialPolyline polyHole = new InitialPolyline (sizeHole);
		//Increasing order for the origin and decreasing for the destiny polyline in order to 
		//make a correct triangulation
		int i = 0;
		while (i < sizeHole / 2) {
			originPoly.getVertex (firstIndex +i).setInHole (true);
			polyHole.addVertex (originPoly.getVertex (firstIndex +i));
			++i;
		}
		//at this point i = sizeHole / 2;
		while (i > 0) {
			--i;
			destinyPoly.getVertex (firstIndex+i).setInHole (true);
			polyHole.addVertex (destinyPoly.getVertex (firstIndex+i));
		}
		//THIRD: Check is a valid hole: no artifacts will be produced, 
		bool invalidHole = false;
		//invalidHole = checkArtifacts (polyHole);
		//and in the walking case, check if the hole is not too upwards or downwards(y component)
		invalidHole = invalidHole || checkInvalidWalk(polyHole);
		//Undo hole if invalid
		if (invalidHole) {
			for (int j = 0; j < sizeHole/2; ++j) {
				originPoly.getVertex (firstIndex + j).setInHole (false);
				destinyPoly.getVertex (firstIndex + j).setInHole (false);
			}
			return null;
		}

		//FOURTH: Do the hole smooth: Project the polyline(3D) into a plane(2D) on the polyline normal direction, just n (not very big) vertices
		InitialPolyline planePoly = generateProjection(polyHole, 4);

		//FIFTH: Last check if hole is really valid (intersection stuff)
		if (IntersectionsController.Instance.doIntersect(polyHole,planePoly,-1)) {
			for (int j = 0; j < sizeHole/2; ++j) {
				originPoly.getVertex (firstIndex + j).setInHole (false);
				destinyPoly.getVertex (firstIndex + j).setInHole (false);
			}
			return null;
		}
		//In case the hole can really be done, add the extruded polyline to the mesh
		// (needed for triangulate correctly between the hole and the projection)
		actualMesh.addPolyline (destinyPoly);

		//SIXTH: Final propoerties of the projection
		//Generate new UVs coordinates of the projection, from y coord of the hole
		float yCoord = (polyHole.getVertex(0).getUV().y + polyHole.getVertex(-1).getUV().y)/2;
		planePoly.generateUVs (yCoord);
		//And put the corresponding indices
		for (int j = 0; j < planePoly.getSize (); ++j) {
			planePoly.getVertex(j).setIndex(actualMesh.getNumVertices()+j);
		}
		//Add the new polyline information to the mesh
		actualMesh.addPolyline (planePoly);

		///FINALLY: Triangulate between the hole and the projection and add the projection to the B intersections
		actualMesh.triangulateTunnelStart(polyHole,planePoly);
		IntersectionsController.Instance.addPolyline (planePoly);

		return planePoly;
	}

	/** From existing polyline, generates a new one by projecting the original to a plane on it's normal direction.
	 * It is also smoothed and scaled **/
	protected InitialPolyline generateProjection(InitialPolyline polyHole, int projectionSize = 4) {
		//Get the plane to project to
		Plane tunnelEntrance = polyHole.generateNormalPlane ();
		//Generate the polyline by projecting to the plane
		InitialPolyline planePoly = new InitialPolyline (4); //n =4, TODO change this as a parameter(must be pair?)
		planePoly.addPosition (Geometry.Utils.getPlaneProjection (tunnelEntrance, polyHole.getVertex (0).getPosition ()));
		planePoly.addPosition (Geometry.Utils.getPlaneProjection (tunnelEntrance, polyHole.getVertex (polyHole.getSize()/2-1).getPosition ()));
		planePoly.addPosition (Geometry.Utils.getPlaneProjection (tunnelEntrance, polyHole.getVertex (polyHole.getSize()/2).getPosition ()));
		planePoly.addPosition (Geometry.Utils.getPlaneProjection (tunnelEntrance, polyHole.getVertex (polyHole.getSize()-1).getPosition ()));

		//Smooth it
		for (int j = 0; j < smoothIterations;++j)
			planePoly.smoothMean ();
		
		//Scale to an approximate size of the real size of the original
		float maxActualRadius = planePoly.computeRadius();
		float destinyRadius = polyHole.computeProjectionRadius ();
		planePoly.scale (destinyRadius / maxActualRadius);

		return planePoly;
	}


	protected bool checkInvalidWalk(Polyline tunelStartPoly) {
		if (!DecisionGenerator.Instance.directionJustWalk)
			return false;

		bool invalidHole = false;
		Vector3 normal = tunelStartPoly.calculateNormal ();
		if (normal.y < 0) {
			invalidHole = normal.y < -DecisionGenerator.Instance.directionYWalkLimit;
		} else {
			invalidHole = normal.y > DecisionGenerator.Instance.directionYWalkLimit;
		}

		return invalidHole;
	}

	protected bool checkArtifacts (Polyline polyHole) {
		//TODO: IMprove this, maybe check directly with y coord is not good enough
		Plane projection = ((InitialPolyline)polyHole).generateNormalPlane ();
		//As its a hole polyline, first and second half are symmetric, there is need to just check one
		//If one half projected does not has all vertices on descendant or ascendant order, it will sure generate an aritfact
		if (Geometry.Utils.getPlaneProjection(projection, polyHole.getVertex (0).getPosition ()).y < 
			Geometry.Utils.getPlaneProjection(projection, polyHole.getVertex (1).getPosition ()).y) { //ascendant
			for (int i = 1; i < polyHole.getSize () / 2; ++i) {
				if (Geometry.Utils.getPlaneProjection (projection, polyHole.getVertex (i).getPosition ()).y >
				    Geometry.Utils.getPlaneProjection (projection, polyHole.getVertex (i + 1).getPosition ()).y)
					return true;
			}
		} else { //descendent
			for (int i = 1; i < polyHole.getSize () / 2; ++i) {
				if (Geometry.Utils.getPlaneProjection (projection, polyHole.getVertex (i).getPosition ()).y <
					Geometry.Utils.getPlaneProjection (projection, polyHole.getVertex (i + 1).getPosition ()).y)
					return true;
			}
		}
		return false;
	}
}