using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecisionGenerator : MonoBehaviour
{

	/** Singleton stuff **/
	private static DecisionGenerator mInstace; 
	public void Awake() {
		mInstace = this;
	}

	public static DecisionGenerator Instance {
		get {
			return mInstace;
		}
	}
		
	/** Distance to extrude **/
	public float minDistance = 1.0f;
	public float maxDistance = 10.0f;

	public float generateDistance() {
		return Random.Range (minDistance, maxDistance);
	}

	/** Holes **/


	/**Direction **/






}