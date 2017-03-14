using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Class that contains the random functions to decide which operations apply when generating and how **/

public class DecisionGenerator : MonoBehaviour {

	//******** Singleton stuff ********//
	private static DecisionGenerator mInstace; 
	public void Awake() {
		mInstace = this;
	}

	public static DecisionGenerator Instance {
		get {
			return mInstace;
		}
	}

	//******** Different operations ********//
	private int numOperations = 5;
	public enum ExtrusionOperation
	{
		ExtrudeOnly, ChangeDistance, ChangeDirection, Scale, Rotate
	}

	//******** General decision********//

	public ExtrusionOperation getNextOperation() {
		//TODO: pass a parameter with previous operations, and decide next taking those one into account
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
	//public int minExtrusionsForHole = 10;
	public int extrudeForHole = 30;
	public bool makeHole(int numExtrude) {
		if (numExtrude < minExtrusionsForHole) //Wait at least minExtrusionsForHole to make a hole
			return false; 
		int r = Random.Range (1, extrudeForHole);
		if (r == 4)
			return true;
		return false;
	}

	//******** Direction ********//



	//******** Scale ********//
	//TODO:Take into account previous scale operations

	//******** Rotation ********//
}