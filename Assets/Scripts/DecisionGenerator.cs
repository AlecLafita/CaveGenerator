using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Class that contains the random functions to decide which operations to apply and how **/
public class DecisionGenerator : MonoBehaviour
{

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
		
	//******** Distance to extrude ********//
	public float minDistance = 1.0f;
	public float maxDistance = 10.0f;

	public float generateDistance() {
		return Random.Range (minDistance, maxDistance);
	}

	//******** Holes ********//
	public int extrudeForHole = 10;
	public bool makeHole() {
		int r = Random.Range (1, 10);
		if (r == 4)
			return true;
		return false;
	}

	//******** Direction ********//






}