using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;


/** Abstract class that manages the cave generation. The differents subclasses differ on the why the holes/tunnels are extruded **/
abstract public class AbstractGenerator {

	protected List<Geometry.Mesh> proceduralMesh; //Mesh that will be modified during the cave generation
	protected Geometry.Mesh actualMesh; //Mesh coresponding to the actual tunnel being generated
	protected Geometry.Mesh stalagmitesMesh; //Mesh corresponding to the stalagmites and so 
	protected float initialTunelHoleProb; //holes can be created depending on this probability [0-1]
	protected int maxHoles; //How many times a hole can be extruded and behave like a tunnel, acts as a countdown
	protected int maxExtrudeTimes; //TODO: consider to deccrement this value as holes are created, or some random function that handles this
	protected InitialPolyline gatePolyline; //Polyline where the cave starts from
	protected int entranceSize; //Number of vertices of a tunnel entrance
	protected GameObject lights; // GameObject that contains all the generated lights

	/**Creates the instance without initializing anything **/
	public AbstractGenerator() {
		proceduralMesh = new List<Geometry.Mesh> ();
		stalagmitesMesh = new Geometry.Mesh ();
		proceduralMesh.Add (stalagmitesMesh);
		lights = new GameObject ("Lights");
	}

	private int smoothIterations = 3;
	/**Initialize, being the arguments the needed parameters for the generator **/
	public void initialize(int gateSize, InitialPolyline iniPol, float initialTunelHoleProb, int maxHoles, int maxExtrudeTimes) {
		for (int i = 0; i < smoothIterations;++i)
			((InitialPolyline)iniPol).smoothMean ();
		((InitialPolyline)iniPol).generateUVs ();

		gatePolyline = iniPol;
		this.initialTunelHoleProb = initialTunelHoleProb;
		this.maxHoles = maxHoles;
		this.maxExtrudeTimes = maxExtrudeTimes;
		entranceSize = gateSize;
		if (entranceSize % 2 != 0) //Force it to be pair
			++entranceSize;
	}

	/**Initializes the tunnel initial polyline, returning the corresponding mesh and setting it as the actual one**/
	protected Geometry.Mesh initializeTunnel(ref Polyline iniPol) {
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

	public float UVfactor = 70.0f;
	/**It creates a new polyline from an exsiting one, applying the corresponding operations**/
	protected Polyline extrude(ExtrusionOperations operation, Polyline originPoly) {
		//Create the new polyline from the actual one
		Polyline newPoly = new Polyline(originPoly.getSize());
		Vector3 direction = operation.directionOperation().apply();
		float distance = operation.distanceOperation().apply();
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
		if (operation.scaleOperation().needApply()) {
			newPoly.scale(operation.scaleOperation().apply());
		}
		if (operation.rotateOperation().needApply() ) {
			newPoly.rotate(operation.rotateOperation().apply());
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
		//invalidHole = Geometry.Utils.checkArtifacts (polyHole);
		//and in the walking case, check if the hole is not too upwards or downwards(y component)
		invalidHole = invalidHole || Geometry.Utils.checkInvalidWalk(polyHole);
		//Undo hole if invalid
		if (invalidHole) {
			for (int j = 0; j < sizeHole/2; ++j) {
				originPoly.getVertex (firstIndex + j).setInHole (false);
				destinyPoly.getVertex (firstIndex + j).setInHole (false);
			}
			return null;
		}

		//FOURTH: Do the hole smooth: Project the polyline(3D) into a plane(2D) on the polyline normal direction, just n (not very big) vertices
		InitialPolyline planePoly =Geometry.Utils.generateProjection(polyHole, entranceSize,smoothIterations);

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
		//And the corresponding hole vertices
		for (int j = 0; j < polyHole.getSize (); ++j) {
			actualMesh.addHoleIndex (polyHole.getVertex (j).getIndex ());
		}

		//SIXTH: Final propoerties of the projection
		//Generate new UVs coordinates of the projection, from y coord of the hole
		float yCoord = (polyHole.getVertex(0).getUV().y + polyHole.getVertex(-1).getUV().y)/2;
		//float projDistance = Vector3.Distance (polyHole.calculateBaricenter (), planePoly.calculateBaricenter ());
		//yCoord += projDistance / UVfactor;
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

	private float maxDiffAngle = 30.0f;
	/** Makes a stalagmite between the two polylilnes (through the extrusion) **/
	protected void makeStalagmite (ExtrusionOperations.stalgmOp stalgmType, Polyline originPoly, Polyline newPoly){
		//TODO: other types of stalagmites, add more than one stalagmite at a time
		const int stalgmSize = 4;
		if (stalgmType == ExtrusionOperations.stalgmOp.Stalactite || stalgmType == ExtrusionOperations.stalgmOp.Stalagmite) {
			Vector3 objective;
			if (stalgmType == ExtrusionOperations.stalgmOp.Stalactite)
				objective = Vector3.up;
			else
				objective = Vector3.down;
			//FIRST: Check each group of 4 adjacent vertices and get the best candidate (normal distance nearer to -up)
			InitialPolyline stalgmPoly = new InitialPolyline (stalgmSize);
			float stalgmAngle = float.MaxValue;
			float auxAngle;
			for (int i = 0; i < originPoly.getSize (); ++i) {
				InitialPolyline auxPoly = new InitialPolyline (stalgmSize);
				auxPoly.addVertex (originPoly.getVertex (i));
				auxPoly.addVertex (newPoly.getVertex (i));
				auxPoly.addVertex (newPoly.getVertex (i + 1));
				auxPoly.addVertex (originPoly.getVertex (i + 1)); 
				auxAngle = Vector3.Angle (auxPoly.calculateNormal (), objective);
				if (auxAngle < stalgmAngle) {
					stalgmPoly = auxPoly;
					stalgmAngle = auxAngle;
				}
			}
			//SECOND: If angle with up/down is very high, cancel stalgmite creation
			if (stalgmAngle > maxDiffAngle)
				return;

			//THIRD: Add the first stalagmite polyline to the special stalagmite mesh, with the correct indices and UV coords
			InitialPolyline actualStalagmiteIni = new InitialPolyline (stalgmPoly);
			actualStalagmiteIni.generateUVs (); //TODO: this not produce a nice visual results
			for (int i = 0; i < stalgmSize; ++i) {
				actualStalagmiteIni.getVertex (i).setIndex (stalagmitesMesh.getNumVertices () + i);
			}
			Polyline actualStalagmite = actualStalagmiteIni;
			stalagmitesMesh.addPolyline (actualStalagmite);
			//FOURTH: Prepare generation variables: #extrusion, scale value,.. We need to find this values in
			//order the stalagmite does not intersect with the extrusion(cross it) and don't look too ugly
			Vector3 stalgmBaricenter = actualStalagmite.calculateBaricenter ();
			Vector3 stalgmDirection = actualStalagmite.calculateNormal ();
			//Find the "counterpart vertex", this is the vertex nearer to the intersection between the from baricenter
			//ray intersection on the stalgmite direction. Will be usefull to avoid corssings by knowing the max size of the stalgmite
			float minAngle = float.MaxValue;
			int counterpartVertex = 0;
			for (int i = 0; i < originPoly.getSize (); ++i) { //TODO: do some dicotomic search?
				auxAngle = Vector3.Angle (stalgmDirection, originPoly.getVertex (i).getPosition () - stalgmBaricenter);
				if (auxAngle < minAngle) {
					minAngle = auxAngle;
					counterpartVertex = i;
				}
			}
			float maxStalgmSize = (originPoly.getVertex (counterpartVertex).getPosition () - stalgmBaricenter).magnitude;
			//TODO: generate this random before, save it to operation and read it 
			maxStalgmSize *= 0.75f; //final stalgmite size
			float stalgmExtrusionDistance = 0.4f;
			int numExtrusions = (int) (maxStalgmSize / stalgmExtrusionDistance);
			//Scale value from stalgmite size and #extrusions
			float stalgmDiam = actualStalagmite.computeRadius()*2;
			float finalStalgmRadius = 0.1f;
			float scaleValue = Mathf.Pow (finalStalgmRadius/stalgmDiam, 1/(float)numExtrusions);
			Vector2 UVincr;
			if (stalgmType == ExtrusionOperations.stalgmOp.Stalactite)
				UVincr = new Vector2 (0.0f, 1.0f);
			else UVincr = new Vector2 (0.0f, -1.0f);
			UVincr.Normalize ();
			UVincr *= (stalgmExtrusionDistance / UVfactor);
			//FIFTH: Now we have the start of the stalgmite and all it's generation paramaeters
			//It needs to be extruded, scaled, extruded,scaled... until very small polyline is produced
			for (int j = 0; j < numExtrusions;++j) {
				Polyline newStalgPoly = new Polyline (stalgmSize);
				for (int i = 0; i < stalgmSize; ++i) {
					//Add vertex to polyline
					newStalgPoly.extrudeVertex (i, actualStalagmite.getVertex (i).getPosition (), stalgmDirection, stalgmExtrusionDistance);
					//Add the index to vertex
					newStalgPoly.getVertex (i).setIndex (actualStalagmite.getVertex (i).getIndex () + stalgmSize);
					//TODO: stalgmite UV -> maybe use another mesh for all the stalgmites, with another material
					newStalgPoly.getVertex (i).setUV (actualStalagmite.getVertex (i).getUV () + UVincr);
				}
				newStalgPoly.setMinRadius (0.0f);
				newStalgPoly.scale (scaleValue);
				//Triangulate the new stalgmite part
				stalagmitesMesh.addPolyline (newStalgPoly);
				stalagmitesMesh.triangulatePolylinesOutside (actualStalagmite, newStalgPoly);

				actualStalagmite = newStalgPoly;
			}

			stalagmitesMesh.closePolylineOutside (actualStalagmite);
		}
	}

	/**Creates a point light between the extrusion, on it's center **/
	protected void makePointLight(Polyline originPoly, Polyline destinyPoly) {
		GameObject newLight = new GameObject ();
		Vector3 position = (originPoly.calculateBaricenter() + destinyPoly.calculateBaricenter())/2;
		newLight.transform.position = position;
		Light l = newLight.AddComponent<Light> ();
		l.type = LightType.Point;
		//TODO: generate this random before, save it to operation and read it 
		l.intensity = 1.8f;
		l.range = 30.0f;
		newLight.transform.parent =  lights.transform;

	}
}