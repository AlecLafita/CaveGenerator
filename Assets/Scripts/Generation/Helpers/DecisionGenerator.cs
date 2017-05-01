using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

/** Class that contains the random functions to decide which operations apply when generating and how **/

//TODO: pass a parameter with previous operations, and decide next taking those one into account
public class DecisionGenerator : MonoBehaviour {

	//******** Singleton stuff ********//
	private static DecisionGenerator mInstace; 
	public void Awake() {
		mInstace = this;
		//Random.seed = 5; //With this setted the result will always be the same
	}

	public static DecisionGenerator Instance {
		get {
			return mInstace;
		}
	}

	//******** Next operation decision********//
	//Duration of each operation
	public int directionExtrBase = 20;
	public int directionExtrDesv = 5;
	public int scaleExtrBase = 20;
	public int scaleExtrDesv = 3;
	public int rotationExtrBase = 30;
	public int rotationExtrDesv = 5;
	//How many extrusions to wait from last operation application
	public int directionKBase = 5;
	public int directionKDesv = 1;
	public int scaleKBase = 5;
	public int scaleKDesv = 1;
	public int rotationKBase = 15;
	public int rotationKDesv = 2;
	public int stalgmKBase = 5;
	public int stalgmKDesv = 4;
	public int pointLightKBase = 12;
	public int pointLightKDesv = 5;
	/** Generates a new operation instance, as it was the beggining of a tunnel **/
	public ExtrusionOperations generateNewOperation(Polyline p) {
		ExtrusionOperations op = new ExtrusionOperations();
		op.forceDistanceOperation (1,DecisionGenerator.Instance.generateDistance (false));
		op.forceDirectionOperation (0, p.calculateNormal (), p.calculateNormal ());
		op.setCanIntersect(IntersectionsController.Instance.getLastBB());
		//op.forceScaleOperation (operationK,Mathf.Pow(2.0f,1/(float)operationK));
		//Generate extrusion wait for each operation
		op.setDirectionWait(generateFromRange(directionKBase,directionKDesv));
		op.setScaleWait (generateFromRange (scaleKBase, scaleKDesv));
		op.setRotateWait (generateFromRange (rotationKBase, rotationKDesv));
		op.setStalagmWait (generateFromRange (stalgmKBase, stalgmKDesv));
		op.setPointLightWait (generateFromRange (pointLightKBase, pointLightKDesv));
		return op;
	}

	/**Decide which operations apply to the next extrusion **/
	public void generateNextOperation (Polyline p, ExtrusionOperations op, int numExtrude, float tunnelProb, int holesCountdown) {
		//Change the distance always, in order to introduce more irregularity
		op.forceDistanceOperation (1,generateDistance (false));
		//Decide to make hole or not
		if (!op.holeOperation())
			op.forceHoleOperation(makeHole (numExtrude, tunnelProb, holesCountdown));

		//Decide which operations generate and apply on next extrusions
		generateNoHoleOperation (p, op);

		//Distance for hole case
		if (op.holeOperation ()) {
			op.forceDistanceOperation (1, generateDistance (true));
		}
		//TODO: Distance for stalagmite case?

		//Update the wait counter
		op.decreaseWait();
	}
		
	/**Decide which operations except from hole apply **/
	private void generateNoHoleOperation(Polyline p, ExtrusionOperations op) {
		//Check each different operation one by one
		int duration;
		if (op.generateDirection()) {
			Vector3 newDirection = generateDirection (p);
			if (newDirection != Vector3.zero) { //Valid direction found
				duration = generateFromRange(directionExtrBase,directionExtrDesv);
				op.forceDirectionOperation(duration, newDirection);
				op.setDirectionWait(duration + generateFromRange(directionKBase,directionKDesv));
				IntersectionsController.Instance.addActualBox ();
				IntersectionsController.Instance.addPolyline (p);
				//op.setCanIntersect (-1);
				op.setCanIntersect (IntersectionsController.Instance.getLastBB ());
			}
		}

		if (op.generateScale ()) {
			duration = generateFromRange (scaleExtrBase, scaleExtrDesv);
			op.forceScaleOperation (duration,Mathf.Pow(generateScale(),1/(float)duration));
			op.setScaleWait (duration + generateFromRange (scaleKBase, scaleKDesv));
		}

		if (op.generateRotation ()) {
			duration = generateFromRange(rotationExtrBase,rotationExtrDesv);
			op.forceRotationOperation(duration,generateRotation ()/duration);
			op.setRotateWait (duration + generateFromRange (rotationKBase, rotationKDesv));
		}

		if (!op.holeOperation() && op.generateStalagmite()) {
			//TODO:Generate all types of stalagmites, add more than one stalagmite at a time
			int type = Random.Range(0,10);
			if (type < 6) 
				op.forceStalagmiteOperation (ExtrusionOperations.stalgmOp.Stalagmite);
			else 
				op.forceStalagmiteOperation (ExtrusionOperations.stalgmOp.Stalactite);
			op.setStalagmWait (1 + generateFromRange(stalgmKBase,stalgmKDesv));
		}
		if (op.generatePointLight ()) {
			op.forcePointLightOperation (true);
			op.setPointLightWait (1 + generateFromRange (pointLightKBase, pointLightKDesv));
		}
		//TODO: add stones, grass,...
	}

	/** Generates a random value between k-d and k+d, both inclusive **/
	private int generateFromRange(int k, int d) {
		return Random.Range(k-d,k+d+1);
	}
		
	//******** Distance to extrude ********//
	public float distanceSmallMin = 2.0f;
	public float distanceSmallMax = 3.0f;
	public float distanceBigMin = 8.0f;
	public float distanceBigMax = 10.0f;

	public float generateDistance(bool big) {
		if (big)
			return Random.Range (distanceBigMin, distanceBigMax);
		else
			return Random.Range (distanceSmallMin, distanceSmallMax);
	}

	//******** Direction ********//
	public float directionMinChange = 0.2f;
	public float directionMaxChange = 0.5f;
	[Range (0.0f,1.0f)] public float directionYWalkLimit = 0.35f;
	[Range (0.0f,40.0f)] public float directionMaxAngle = 40.0f;

	private const int directionGenerationTries = 3;
	/** Generates a new extrusion direction for a polylilne. It check it's not too far from it's normal **/
	public Vector3 generateDirection(Polyline p) {
		//This does not change the normal! The normal is always the same as all the points of a polyline are generated at 
		//the same distance that it's predecessor polyline (at the moment at least)
		bool goodDirection = false;
		Vector3 auxiliarDirection = new Vector3();
		Vector3 result = Vector3.zero;
		Vector3 polylineNormal = p.calculateNormal ();
		for (int i = 0; i < directionGenerationTries && !goodDirection; ++i) {
			//auxiliarDirection = changeDirection(polylineNormal);
			auxiliarDirection = generateDirection();
			//Avoid intersection and narrow halways between the old and new polylines by setting an angle limit
			//(90 would produce a plane and greater than 90 would produce an intersection)
			if (Vector3.Angle (auxiliarDirection, polylineNormal) < directionMaxAngle) {
				goodDirection = true;
				result = auxiliarDirection;
			}
		}
		return result;
	}

	/** Generates a new direction by changing a bit an existing one **/
	private Vector3 changeDirection(Vector3 dir) {
		int xChange = Random.Range (-1, 2);
		int yChange = Random.Range (-1, 2);
		int zChange = Random.Range (-1, 2);
		dir += new Vector3 (xChange *Random.Range(directionMinChange,directionMaxChange), 
			yChange*Random.Range(directionMinChange,directionMaxChange),
			zChange*Random.Range(directionMinChange,directionMaxChange));
		if (dir.y < 0)
			dir.y = Mathf.Max (dir.y, -directionYWalkLimit);
		else if (dir.y > 0)
			dir.y = Mathf.Min (dir.y, directionYWalkLimit);
		return dir.normalized;
	}

	/** Generates a raw new direction **/
	private Vector3 generateDirection() {
		float xDir = Random.Range (-1.0f, 1.0f);
		float yDir = Random.Range (-directionYWalkLimit, directionYWalkLimit);
		float zDir = Random.Range (-1.0f, 1.0f);
		return new Vector3(xDir, yDir, zDir).normalized;
	}
		
	//******** Scale ********//
	[Range (0.0f,0.99f)] public float scaleLimit = 0.5f;
	public float generateScale() {
		return Random.Range (1.0f-scaleLimit, 1.0f + scaleLimit);
	}

	//******** Rotation ********//
	[Range (0.0f,35.0f)] public int rotationLimit = 30;
	public float generateRotation() {
		return (float)Random.Range (-rotationLimit, rotationLimit);
	}

	//******** Holes ********//
	public int holeK = 3;
	private int minExtrusionsForHole = 3; //Number of extrusions to wait to make hole
	[Range (0.0f,1.0f)] public float holeProb = 0.4f; //Initial probability to do a hole
	public float holeLambda = 0.02f; //How each extrusion weights to the to final decision

	public enum holeConditions {
		EachKDesv, EachKDesvProb, MoreExtrMoreProb, MoreExtrLessProb
	}
	public holeConditions holeCondition;

	private bool makeHole(int numExtrude, float tunnelProb, int holesCountdown) {
		//Check that the holes limit has not arrived
		if (holesCountdown <= 0 ) 
			return false;

		//Wait at least minExtrusionsForHole to make a hole
		if (numExtrude < minExtrusionsForHole)
			return false; 
		//Check if this tunnel can make a hole
		float r = Random.value;
		if (r > tunnelProb)
			return false;

		//Then apply differents decisions to make holes (or not)
		r = Random.value;
		switch (holeCondition) {
		case (holeConditions.EachKDesv) :{
				if (numExtrude % holeK == 0) { //TODO:Change this
					return true;
				}
				break;
			}
		case (holeConditions.EachKDesvProb): {
				if ((numExtrude % holeK == 0) && r <= holeProb){
					return true;
				}
				break;
			}
		case (holeConditions.MoreExtrMoreProb): {
				if (r <= holeProb + numExtrude * holeLambda){
					return true;
				}
				break;
			}
		case (holeConditions.MoreExtrLessProb): {
				if (r <= holeProb - numExtrude * holeLambda){
					return true;
				}
				break;
			}
		default:
			break;
		}
		return false;
	}

	public int holeMinVertices = 4;
	public int holeMaxVertices = 10;
	/*
	public void whereToDig(int numV, out int sizeHole, out int firstIndex) {
		//TODO: improve this to avoid intersections (artifacts)
		sizeHole = Random.Range(3,numV/2);
		sizeHole *= 2; //Must be a pair number!
		sizeHole = Mathf.Min (sizeHole, holeMaxVertices);
		firstIndex = Random.Range (0, numV);
	}*/


	public float holesMaxAngleDirection = 30.0f;
	/** Generates the first index of a vertex and how many vertices from it use to make a hole **/
	public void whereToDig(Polyline p, out int sizeHole, out int firstIndex) {
		sizeHole = 0;
		firstIndex = 0;
		//Generate the approximate direction of the hole
		Vector3 apprDir = generateDirection();
		if (apprDir == Vector3.zero) //No random direction could be found
			return;

		Vector3 baricenter = p.calculateBaricenter ();

		//valid <=> close to the approximate direction)
		//Auxiliar variables
		bool found = false;
		int auxIndex = 0;
		Vector3 auxDirection;

		while (!found && auxIndex < p.getSize()) { //Get any vertex that is valid
			auxDirection = p.getVertex (auxIndex).getPosition () - baricenter;
			if (Vector3.Angle (auxDirection, apprDir) < holesMaxAngleDirection)
				found = true;
			else
				++auxIndex;
		}
		if (!found) //None of the vertex are valid
			return;

		found = false;
		while (!found) { //Get the first vertex to be valid to make the hole (clockwise!)
			--auxIndex;
			auxDirection = p.getVertex (auxIndex).getPosition () - baricenter;
			if (Vector3.Angle (auxDirection, apprDir) >= holesMaxAngleDirection) {
				auxIndex++;
				firstIndex = auxIndex;
				found = true;
			}
		}//This should not be an infinite loop as there will be always some vertex direction not too close to the approximate one

		//Now check check all the vertices from the first until some no valid found, this will mark the end of the hole
		found = false;
		sizeHole = 1;
		while (!found) {
			auxDirection = p.getVertex (auxIndex).getPosition () - baricenter;
			if (Vector3.Angle (auxDirection, apprDir) < holesMaxAngleDirection)
				++sizeHole;
			else
				found = true;
			++auxIndex;
		}//This should not be an infinite loop as there will be always some vertex direction not too close to the approximate one
		sizeHole *= 2; //Same vertices on the two polylines
		sizeHole = Mathf.Min (sizeHole, holeMaxVertices);
	}

}