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

	//******** General decision********//
	public int operationK = 3; // Every k extrusion, operation to do
	public int operationDeviation = 2; // Add more range to make extrusions, each [k-deviation,k+deviation]
	public ExtrusionOperation generateNextOperation (int extrusionSinceLastOperation) {
		ExtrusionOperation op = new ExtrusionOperation();
		//Check if a new operation can be done
		int extrusionsNeeded = Random.Range(-operationDeviation, operationDeviation+1);
		//If it not satisfies the condition of generating an operation, return a just extrusion operation
		if ((extrusionSinceLastOperation % operationK + extrusionsNeeded) != 0)
			return op;

		int numOperations = op.getNumOperations ();
		int i = Random.Range (0, numOperations);
		op.forceOperation (i);
		return op;
	}
		
	//******** Distance to extrude ********//
	public float distanceMin = 1.0f;
	public float distanceMax = 10.0f;

	public float generateDistance() {
		return Random.Range (distanceMin, distanceMax);
	}

	//******** Direction ********//
	public float directionMinChange = 0.2f;
	public float directionMaxChange = 0.5f;
	public Vector3 changeDirection(Vector3 dir) {
		int xChange = Random.Range (-1, 2);
		int yChange = Random.Range (-1, 2);
		int zChange = Random.Range (-1, 2);
		dir += new Vector3 (xChange *Random.Range(directionMinChange,directionMaxChange), 
			yChange*Random.Range(directionMinChange,directionMaxChange),
			zChange*Random.Range(directionMinChange,directionMaxChange));

		return dir.normalized;
	}

	public Vector3 generateDirection() {
		float xDir = Random.Range (-1.0f, 1.0f);
		float yDir = Random.Range (-1.0f, 1.0f);
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

	public bool makeHole(int numExtrude, float tunnelProb = 1.0f) {
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
				if (numExtrude % holeK == 0)
					return true;
				break;
			}
		case (holeConditions.EachKProb): {
				if ((numExtrude % holeK == 0) && r <= holeProb)
					return true;
				break;
			}
		case (holeConditions.MoreExtrMoreProb): {
				Debug.Log (holeProb + numExtrude * holeLambda);
				if (r <= holeProb + numExtrude * holeLambda)
					return true;
				break;
			}
		case (holeConditions.MoreExtrLessProb): {
				if (r <= holeProb - numExtrude * holeLambda)
					return true;
				break;
			}
		default:
			break;
		}
		return false;
	}

	public int holeMaxVertices = 10;
	public void whereToDig(int numV, out int sizeHole, out int firstIndex) {
		//TODO: improve this to avoid intersections (artifacts)
		sizeHole = Random.Range(2,numV);
		sizeHole *= 2; //Must be a pair number!
		sizeHole = Mathf.Min (sizeHole, holeMaxVertices);
		firstIndex = Random.Range (0, numV);
	}

}