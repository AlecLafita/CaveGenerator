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
		
	private const int distanceGenerationTries = 3;
	/**It creates a new polyline from an exsiting one, applying the corresponding operations**/
	protected Polyline extrude(ExtrusionOperation operation, Polyline originPoly, ref Vector3 direction, ref float distance, ref int canIntersect) {
		//Check if distance/ direction needs to be changed
		Vector3 newDirection = direction;
		float newDistance = distance;
		if (operation.distanceOperation()) {
			newDistance = DecisionGenerator.Instance.generateDistance (operation.holeOperation());
		}
		if (operation.directionOperation()) {
			//This does not change the normal! The normal is always the same as all the points of a polyline are generated at 
			//the same distance that it's predecessor polyline (at the moment at least)
			bool goodDirection = false;
			Vector3 auxiliarDirection =  new Vector3();
			Vector3 polylineNormal = originPoly.calculateNormal ();
			for (int i = 0; i < distanceGenerationTries && !goodDirection; ++i) {
				//auxiliarDirection = DecisionGenerator.Instance.changeDirection(direction);
				auxiliarDirection = DecisionGenerator.Instance.generateDirection();
				//auxiliarDirection = DecisionGenerator.Instance.generateDirection(polylineNormal);
				//Avoid intersection and narrow halways between the old and new polylines by setting an angle limit
				//(90 would produce a plane and greater than 90 would produce an intersection)
				if (Vector3.Angle (auxiliarDirection, polylineNormal) < DecisionGenerator.Instance.directionMaxAngle) {
					goodDirection = true;
					newDirection = auxiliarDirection;
				}
			}
			//if (!goodDirection)
				//Debug.Log ("BAD DIRECITON");
		}
		//Create the new polyline from the actual one
		Polyline newPoly = new Polyline(originPoly.getSize());
		for (int i = 0; i < originPoly.getSize(); ++i) { //Generate the new vertices
			//Add vertex to polyline
			newPoly.extrudeVertex(i, originPoly.getVertex(i).getPosition(), newDirection, newDistance);
			//Add the index to vertex
			newPoly.getVertex(i).setIndex(proceduralMesh.getNumVertices() + i);

		}

		//Apply operations, if any
		if (operation.scaleOperation()) {
			newPoly.scale (DecisionGenerator.Instance.generateScale());
		}
		if (operation.rotationOperation ()) {
			newPoly.rotate (DecisionGenerator.Instance.generateRotation());
		}
			
		//Check there is no intersection
		if (IntersectionsController.Instance.doIntersect (originPoly, newPoly, canIntersect)) {
			return null;
		} else {
			distance = newDistance;
			if (direction != newDirection) { //Direction changed
				IntersectionsController.Instance.addActualBox ();
				IntersectionsController.Instance.addPolyline (originPoly);
				canIntersect = IntersectionsController.Instance.getLastBB ();
				direction = newDirection;
			}
		}

		//Add new polyline to the mesh
		for (int i = 0; i < originPoly.getSize (); ++i) {
			//Add the new vertex to the mesh
			proceduralMesh.addVertex(newPoly.getVertex(i).getPosition());
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

		//In the walking case, check if the hole is not too upwards or downwards(y component)
		//TODO: Improve this to be less do and test and more test and do
		/*bool invalidWalkHole = checkInvalidWalk(polyHole);
		//Undo hole if invalid
		if (invalidWalkHole) {
			for (int j = 0; j < sizeHole/2; ++j) {
				originPoly.getVertex (firstIndex + j).setInHole (false);
				destinyPoly.getVertex (firstIndex + j).setInHole (false);
			}
			return null;
		}*/

		return polyHole;
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
		
}
