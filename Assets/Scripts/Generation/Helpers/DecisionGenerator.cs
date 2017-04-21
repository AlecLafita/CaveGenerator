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
	/**Decide which operations apply to the next extrusion **/

	public ExtrusionOperations generateNewOperation(Polyline p) {
		ExtrusionOperations op = new ExtrusionOperations();
		op.forceDistanceOperation (1,DecisionGenerator.Instance.generateDistance (false));
		op.forceDirectionOperation (0, p.calculateNormal (), p.calculateNormal ());
		op.setCanIntersect(IntersectionsController.Instance.getLastBB());
		//op.forceScaleOperation (operationK,Mathf.Pow(2.0f,1/(float)operationK));
		return op;
	}
		
	public void generateNextOperation (Polyline p, ExtrusionOperations op, ref int extrusionsSinceLastOperation, int numExtrude, float tunnelProb, int holesCountdown) {
		//op = new ExtrusionOperation();
		//Change the distance as the first one is always bigger
		//if (numExtrude == 0) 
			op.forceDistanceOperation (1,generateDistance (false));
		//Decide to make hole or not
		if (!op.holeOperation())
			op.forceHoleOperation(makeHole (numExtrude, tunnelProb, holesCountdown));

		//Decide which operations to apply
		generateNoHoleOperation (p, op, extrusionsSinceLastOperation);

		//Distance for hole case
		if (op.holeOperation()) {
			op.forceDistanceOperation (1,generateDistance (true));
		}

		//Update the counter
		if (op.justExtrude ())
			++extrusionsSinceLastOperation;
		else
			extrusionsSinceLastOperation = 0;
	}

	public int operationK = 4; // Every k extrusions, operation can be done
	public int operationDeviation = 0; // Add more range to make extrusions, each random value between [k-deviation,k+deviation]
	//Changes each time the function is called!
	private int operationMax = 2; //How many operations can be applied at a time
	/**Decide which operations except from hole apply **/
	private void generateNoHoleOperation(Polyline p, ExtrusionOperations op, int extrusionsSinceLastOperation) {
		//Check if a new operation can be done
		//If it not satisfies the condition of generating an operation, return a just extrusion operation
		int extrusionsNeeded = Random.Range(-operationDeviation, operationDeviation+1);
		if ((extrusionsSinceLastOperation % operationK + extrusionsNeeded) != 0) {
			return;
		}
		int numOperations = op.getNumOperations ();
		int operationsToDo = Random.Range (1, operationMax + 1);
		for (int i = 0; i < operationsToDo;++i) {
			int opPos = Random.Range (0, numOperations);
			switch (opPos) {
			case(0): //Distance
				{
					op.forceDistanceOperation (1,generateDistance (false));
					break;
				}
			case(1): //Direction
				{
					Vector3 newDirection = generateDirection (p);
					if (newDirection != Vector3.zero) { //Valid direction found
						op.forceDirectionOperation(operationK, newDirection);
						IntersectionsController.Instance.addActualBox ();
						IntersectionsController.Instance.addPolyline (p);
						op.setCanIntersect (-1);
						//op.setCanIntersect (IntersectionsController.Instance.getLastBB ());
					}
					break;
				}
			case(2): //Scale
				{
					op.forceScaleOperation (operationK,Mathf.Pow(generateScale(),1/(float)operationK));
					break;
				}
			case(3): //Rotation
				{
					op.forceRotationOperation(operationK,generateRotation ()/operationK);
					break;
				}
			}
		}

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
	public bool directionJustWalk = false;
	public float directionYWalkLimit = 0.35f;

	private const int directionGenerationTries = 3;
	public Vector3 generateDirection(Polyline p) {
		//This does not change the normal! The normal is always the same as all the points of a polyline are generated at 
		//the same distance that it's predecessor polyline (at the moment at least)
		bool goodDirection = false;
		Vector3 auxiliarDirection = new Vector3();
		Vector3 result = Vector3.zero;
		Vector3 polylineNormal = p.calculateNormal ();
		for (int i = 0; i < directionGenerationTries && !goodDirection; ++i) {
			//auxiliarDirection = DecisionGenerator.Instance.changeDirection(direction);
			auxiliarDirection = generateDirection();
			//auxiliarDirection = DecisionGenerator.Instance.generateDirection(polylineNormal);
			//Avoid intersection and narrow halways between the old and new polylines by setting an angle limit
			//(90 would produce a plane and greater than 90 would produce an intersection)
			if (Vector3.Angle (auxiliarDirection, polylineNormal) < DecisionGenerator.Instance.directionMaxAngle) {
				goodDirection = true;
				result = auxiliarDirection;
			}
		}
		//if (!goodDirection)
		//Debug.Log ("BAD DIRECITON");

		return result;

	}

	private Vector3 changeDirection(Vector3 dir) {
		int xChange = Random.Range (-1, 2);
		int yChange = Random.Range (-1, 2);
		int zChange = Random.Range (-1, 2);
		dir += new Vector3 (xChange *Random.Range(directionMinChange,directionMaxChange), 
			yChange*Random.Range(directionMinChange,directionMaxChange),
			zChange*Random.Range(directionMinChange,directionMaxChange));
		if (directionJustWalk) {
			if (dir.y < 0)
				dir.y = Mathf.Max (dir.y, -directionYWalkLimit);
			else if (dir.y > 0)
				dir.y = Mathf.Min (dir.y, directionYWalkLimit);
		}
		return dir.normalized;
	}

	private Vector3 generateDirection() {
		float xDir = Random.Range (-1.0f, 1.0f);
		float yDir;
		if (directionJustWalk)
			yDir = Random.Range (-directionYWalkLimit, directionYWalkLimit);
		else
			yDir = Random.Range (-1.0f, 1.0f);
		float zDir = Random.Range (-1.0f, 1.0f);
		return new Vector3(xDir, yDir, zDir).normalized;
	}

	[Range (0.0f,40.0f)] public float directionMaxAngle = 40.0f;
	private Vector3 generateDirection(Vector3 normal) {
		float xValue = Random.Range (normal.x - directionMaxAngle / 90.0f, normal.x + directionMaxAngle / 90.0f);
		float yValue = Random.Range (normal.y - directionMaxAngle / 90.0f, normal.y + directionMaxAngle / 90.0f);
		float zValue = Random.Range (normal.z - directionMaxAngle / 90.0f, normal.z + directionMaxAngle / 90.0f);

		Vector3 result =  new Vector3(xValue,yValue,zValue);
		if (directionJustWalk) {
			//TODO
		}
		return result.normalized;

	}


	//******** Scale ********//
	[Range (0.0f,0.99f)] public float scaleLimit = 0.5f;
	public float generateScale() {
		return Random.Range (1.0f-scaleLimit, 1.0f + scaleLimit);
	}

	//******** Rotation ********//
	public int rotationLimit = 30;
	public float generateRotation() {
		return (float)Random.Range (-rotationLimit, rotationLimit);
	}

	//******** Holes ********//
	private int minExtrusionsForHole = 3; //Number of extrusions to wait to make hole
	[Range (0.0f,1.0f)] public float holeProb = 0.4f; //Initial probability to do a hole
	public int holeK = 5; //For the k conditions
	public float holeLambda = 0.02f; //How each extrusion weights to the to final decision

	public enum holeConditions {
		EachK, EachKProb, MoreExtrMoreProb, MoreExtrLessProb
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
		case (holeConditions.EachK) :{
				if (numExtrude % holeK == 0) {
					return true;
				}
				break;
			}
		case (holeConditions.EachKProb): {
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
	public void whereToDig(int numV, out int sizeHole, out int firstIndex) {
		//TODO: improve this to avoid intersections (artifacts)
		sizeHole = Random.Range(3,numV/2);
		sizeHole *= 2; //Must be a pair number!
		sizeHole = Mathf.Min (sizeHole, holeMaxVertices);
		firstIndex = Random.Range (0, numV);
	}


	public float holesMaxAngleDirection = 30.0f;
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
		sizeHole *= 2;
		sizeHole = Mathf.Min (sizeHole, holeMaxVertices);
	}

}