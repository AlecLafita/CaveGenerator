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
	public ExtrusionOperation getNextOperation() {
		int nextOperation = Random.Range(0,numOperations);
		return (ExtrusionOperation)nextOperation;
	}
		
	//******** Distance to extrude ********//
	public float minDistance = 1.0f;
	public float maxDistance = 10.0f;

	public float generateDistance() {
		return Random.Range (minDistance, maxDistance);
	}

	//******** Holes ********//
	private int minExtrusionsForHole = 3;
	public float probForHole = 0.4f;
	public int kHole = 4;
	private float lambdaHole = 0.2f;

	public enum holeCondition{
		EachK, EachKProb, MoreExtrMoreProb, MoreExtrLessProb
	}
	public holeCondition mHoleCondition;

	public bool makeHole(int numExtrude) {
		if (numExtrude < minExtrusionsForHole) //Wait at least minExtrusionsForHole to make a hole
			return false; 
		//Different decisions to make holes
		float r = Random.value;

		switch (mHoleCondition) {
			case (holeCondition.EachK) :{
				if (numExtrude % kHole == 0)
					return true;
				break;
			}
			case (holeCondition.EachKProb): {
				if ((numExtrude % kHole == 0) && r <= probForHole)
					return true;
				break;
			}
			case (holeCondition.MoreExtrMoreProb): {
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

	//******** Direction ********//



	//******** Scale ********//


	//******** Rotation ********//
}