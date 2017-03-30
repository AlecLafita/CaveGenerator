using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;


/** Abstract class that manages the cave generation. The differents subclasses differ on the why the holes/tunnels are extruded **/
abstract public class AbstractGenerator {

	protected Geometry.Mesh proceduralMesh;	//Mesh that will be modified during the cave generation
	protected float initialTunelHoleProb; //holes can be created depending on this probability [0-1]
	protected int maxHoles; //How many times a hole can be extruded and behave like a tunnel, acts as a countdown
	protected int maxExtrudeTimes;//TODO: consider to deccrement this value as holes are created, or some random function that handles this
	protected Polyline gatePolyline; //Polyline where the cave starts from

	/**Creates the instance without initializing anything **/
	public AbstractGenerator() {
	}

	/**Initialize, being the arguments are the needed parameters for the generator **/
	public void initialize(Polyline iniPol, float initialTunelHoleProb, int maxHoles, int maxExtrudeTimes) {
		proceduralMesh = new Geometry.Mesh (iniPol);
		gatePolyline = iniPol;
		this.initialTunelHoleProb = initialTunelHoleProb;
		this.maxHoles = maxHoles;
		this.maxExtrudeTimes = maxExtrudeTimes;
	}

	/**Generates the cave **/
	abstract public void generate ();

	/**Returns the generated mesh **/
	public Geometry.Mesh getMesh() {
		return proceduralMesh;
	}
		
	private const float maxNormalDirectionAngle = 40.0f;
	private const int distanceGenerationTries = 3;
	/**It creates a new polyline from an exsiting one, applying the corresponding operations**/
	protected Polyline extrude(ExtrusionOperation operation, Polyline originPoly, ref Vector3 direction, ref float distance, ref int canIntersect) {
		//Check if distance/ direction needs to be changed
		Vector3 oldDirection = direction;
		float oldDistance = distance;
		int oldCanIntersect = canIntersect;

		if (operation.distanceOperation()) {
			distance = DecisionGenerator.Instance.generateDistance (operation.holeOperation());
		}
		if (operation.directionOperation()) {
			//This does not change the normal! The normal is always the same as all the points of a polyline are generated at 
			//the same distance that it's predecessor polyline (at the moment at least)
			bool goodDirection = false;
			Vector3 newDirection =  new Vector3();
			Vector3 polylineNormal = originPoly.calculateNormal ();
			for (int i = 0; i < distanceGenerationTries && !goodDirection; ++i) {
				//Vector3 newDirection = DecisionGenerator.Instance.changeDirection(direction);
				newDirection = DecisionGenerator.Instance.generateDirection();
				//Avoid intersection and narrow halways between the old and new polylines by setting an angle limit
				//(90 would produce a plane and greater than 90 would produce an intersection)
				if (Vector3.Angle (newDirection, polylineNormal) < maxNormalDirectionAngle) {
					goodDirection = true;
					direction = newDirection;
					IntersectionsController.Instance.addActualBox ();
					IntersectionsController.Instance.addPolyline (originPoly);
					canIntersect = IntersectionsController.Instance.getLastBB ();
				}
			}
		}

		//Create the new polyline from the actual one
		Polyline newPoly = new Polyline(originPoly.getSize());
		for (int i = 0; i < originPoly.getSize(); ++i) { //Generate the new vertices
			//Add vertex to polyline
			newPoly.extrudeVertex(i, originPoly.getVertex(i).getPosition(), direction,distance);
			//Add the index to vertex
			newPoly.getVertex(i).setIndex(proceduralMesh.getNumVertices() + i);

		}
		//Check there is no intersection
		if (IntersectionsController.Instance.doIntersect (originPoly, newPoly, canIntersect)) {
			//Undo changes
			distance = oldDistance;
			direction = oldDirection;
			canIntersect = oldCanIntersect;
			return null;
		}

		//Add new polyline to the mesh
		for (int i = 0; i < originPoly.getSize (); ++i) {
			//Add the new vertex to the mesh
			proceduralMesh.addVertex(newPoly.getVertex(i).getPosition());
		}

		//Apply operations, if any
		if (operation.scaleOperation()) {
			newPoly.scale (DecisionGenerator.Instance.generateScale());
		}
		if (operation.rotationOperation ()) {
			newPoly.rotate (DecisionGenerator.Instance.generateRotation());
		}

		return newPoly;
	}

	/** Makes a hole betwen two polylines and return this hole as a new polyline **/
	protected Polyline makeHole(Polyline originPoly, Polyline destinyPoly) {
		//TODO: more than one hole, Make two holes on same polylines pairs can cause intersections!

		// Decide how and where the hole will be done, take advantatge indices
		// on the two polylines are at the same order (the new is kind of a projection of the old)
		int sizeHole; int firstIndex;
		DecisionGenerator.Instance.whereToDig (originPoly.getSize(), out sizeHole, out firstIndex);

		//Create the hole polyline by marking and adding the hole vertices (from old a new polylines)
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

		return polyHole;
	}
		
}
