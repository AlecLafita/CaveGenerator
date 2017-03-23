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

	//******** Different operations ********//
	private int numOperations = 5;
	//TODO: As more than one operation can be applied at a time, should be a good idea
	//implement the different operations as a binary positions
	public enum ExtrusionOperation
	{
		ExtrudeOnly, ChangeDistance, ChangeDirection, Scale, Rotate
	}

	//******** General decision********//
	public ExtrusionOperation generateNextOperation() {
		int nextOperation = Random.Range(0,numOperations);
		return (ExtrusionOperation)nextOperation;
	}
		
	//******** Distance to extrude ********//
	public float minDistance = 1.0f;
	public float maxDistance = 10.0f;

	public float generateDistance() {
		return Random.Range (minDistance, maxDistance);
	}

	//******** Direction ********//
	public float minDirectionToChange = 0.2f;
	public float maxDirectionToChange = 0.5f;
	public Vector3 changeDirection(Vector3 dir) {
		int xChange = Random.Range (-1, 2);
		int yChange = Random.Range (-1, 2);
		int zChange = Random.Range (-1, 2);
		dir += new Vector3 (xChange *Random.Range(minDirectionToChange,maxDirectionToChange), 
			yChange*Random.Range(minDirectionToChange,maxDirectionToChange),
			zChange*Random.Range(minDirectionToChange,maxDirectionToChange));

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
	[Range (0.0f,1.0f)] public float probForHole = 0.4f; //Initial probability to do a hole
	public int holeK = 5; //For the k conditions
	public float lambdaHole = 0.02f; //How each extrusion weights to the to final decision


	public enum holeCondition {
		EachK, EachKProb, MoreExtrMoreProb, MoreExtrLessProb
	}
	public holeCondition mHoleCondition;

	public bool makeHole(int numExtrude, float probHole = 1.0f) {
		//Wait at least minExtrusionsForHole to make a hole
		if (numExtrude < minExtrusionsForHole) 
			return false; 
		
		//Check if this tunnel can make a hole
		float r = Random.value;
		if (r > probHole)
			return false;

		//Then apply differents decisions to make holes (or not)
		r = Random.value;
		switch (mHoleCondition) {
		case (holeCondition.EachK) :{
				if (numExtrude % holeK == 0)
					return true;
				break;
			}
		case (holeCondition.EachKProb): {
				if ((numExtrude % holeK == 0) && r <= probForHole)
					return true;
				break;
			}
		case (holeCondition.MoreExtrMoreProb): {
				Debug.Log (probForHole + numExtrude * lambdaHole);
				if (r <= probForHole + numExtrude * lambdaHole)
					return true;
				break;
			}
		case (holeCondition.MoreExtrLessProb): {
				if (r <= probForHole - numExtrude * lambdaHole)
					return true;
				break;
			}
		default:
			break;
		}
		return false;
	}

	public int maxVerticesHole = 10;
	public void whereToDig(int numV, out int sizeHole, out int firstIndex) {
		//TODO: improve this to avoid intersections
		sizeHole = Random.Range(2,numV);
		sizeHole *= 2; //Must be a pair number!
		sizeHole = Mathf.Min (sizeHole, maxVerticesHole);
		firstIndex = Random.Range (0, numV);
	}

}