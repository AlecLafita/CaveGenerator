using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	public void generateNextOperation (ref ExtrusionOperation op, ref int extrusionsSinceLastOperation, int numExtrude, float tunnelProb = 1.0f) {
		op = new ExtrusionOperation();
		//Change the distance as the first one is always bigger
		if (numExtrude == 0) 
			op.forceDistanceOperation ();
		//Decide which operations to apply
		generateNoHoleOperation (ref op, extrusionsSinceLastOperation);

		//Decide to make hole or not
		makeHole (ref op, numExtrude, tunnelProb);

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
	private void generateNoHoleOperation(ref ExtrusionOperation op, int extrusionsSinceLastOperation) {
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
			op.forceOperation (opPos);
		}
	}
		
	//******** Distance to extrude ********//
	public float distanceMin = 2.0f;
	public float distanceMax = 3.0f;
	public float distanceHoleMin = 8.0f;
	public float distanceHoleMax = 10.0f;

	public float generateDistance() {
		return Random.Range (distanceMin, distanceMax);
	}

	public float generateDistance(bool doHole) {
		if (doHole)
			return Random.Range (distanceHoleMin, distanceHoleMax);
		else
			return Random.Range (distanceMin, distanceMax);
	}

	//******** Direction ********//
	public float directionMinChange = 0.2f;
	public float directionMaxChange = 0.5f;
	public bool directionJustWalk = false;
	public float directionYWalkLimit = 0.35f;
	public Vector3 changeDirection(Vector3 dir) {
		int xChange = Random.Range (-1, 2);
		int yChange = Random.Range (-1, 2);
		int zChange = Random.Range (-1, 2);
		dir += new Vector3 (xChange *Random.Range(directionMinChange,directionMaxChange), 
			yChange*Random.Range(directionMinChange,directionMaxChange),
			zChange*Random.Range(directionMinChange,directionMaxChange));
		if (dir.y < 0)
			dir.y = Mathf.Max (dir.y, -directionYWalkLimit);
		else if(dir.y >0)
			dir.y = Mathf.Min (dir.y, directionYWalkLimit);
		return dir.normalized;
	}

	public Vector3 generateDirection() {
		float xDir = Random.Range (-1.0f, 1.0f);
		float yDir;
		if (directionJustWalk)
			yDir = Random.Range (-directionYWalkLimit, directionYWalkLimit);
		else
			yDir = Random.Range (-1.0f, 1.0f);
		float zDir = Random.Range (-1.0f, 1.0f);
		return new Vector3(xDir, yDir, zDir);
	}


	//******** Scale ********//
	public float generateScale() {
		return Random.Range (0.5f, 1.5f);
	}

	//******** Rotation ********//
	private int rotationLimit = 30;
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

	public void makeHole(ref ExtrusionOperation op, int numExtrude, float tunnelProb = 1.0f) {
		
		//Wait at least minExtrusionsForHole to make a hole
		if (numExtrude < minExtrusionsForHole)
			return; 
		//Check if this tunnel can make a hole
		float r = Random.value;
		if (r > tunnelProb)
			return;

		//Then apply differents decisions to make holes (or not)
		r = Random.value;
		switch (holeCondition) {
		case (holeConditions.EachK) :{
				if (numExtrude % holeK == 0) {
					op.forceHoleOperation ();
					return;
				}
				break;
			}
		case (holeConditions.EachKProb): {
				if ((numExtrude % holeK == 0) && r <= holeProb){
					op.forceHoleOperation ();
					return;
				}
				break;
			}
		case (holeConditions.MoreExtrMoreProb): {
				if (r <= holeProb + numExtrude * holeLambda){
					op.forceHoleOperation ();
					return;
				}
				break;
			}
		case (holeConditions.MoreExtrLessProb): {
				if (r <= holeProb - numExtrude * holeLambda){
					op.forceHoleOperation ();
					return;
				}
				break;
			}
		default:
			break;
		}
		return;
	}

	public int holeMaxVertices = 10;
	public void whereToDig(int numV, out int sizeHole, out int firstIndex) {
		//TODO: improve this to avoid intersections (artifacts)
		sizeHole = Random.Range(3,numV/2);
		sizeHole *= 2; //Must be a pair number!
		sizeHole = Mathf.Min (sizeHole, holeMaxVertices);
		firstIndex = Random.Range (0, numV);
	}

}