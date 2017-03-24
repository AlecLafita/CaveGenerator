using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Represent all the operations to apply to a extrusion **/
public class ExtrusionOperation  {

	//Number of different operations
	private int numOperations = 4;

	//Position of each operation
	private int distance = 0; 
	private int direction = 1;
	private int scale = 2;
	private int rotation = 3;

	//Which operations to apply
	private bool[] operation;


	/** Creator, makes a just extrusion operation**/
	public ExtrusionOperation() {
		operation = new bool[numOperations];
		reset ();
	}

	/*********Getters**********/
	/** Returns true if no operations needs to be done, just the extrusion **/
	public bool justExtrude() {
		for (int i = 0; i < numOperations; ++i) {
			if (operation[i])
				return false;
		}
		return true;
	}

	/** Returns if a distance change needs to be done **/
	public bool distanceOperation() {
		return operation [distance];
	}

	/** Returns if a direction change needs to be done **/
	public bool directionOperation() {
		return operation [direction];
	}

	/** Returns if a scale needs to be done **/
	public bool scaleOperation() {
		return operation [scale];
	}

	/** Returns if a rotation needs to be done **/
	public bool rotationOperation() {
		return operation [rotation];
	}

	/*********Setters**********/
	/** Generate one random operation to be applied **/
	public void generateRandomOperation() {
		int i = Random.Range(0,numOperations);
		operation [i] = true;
	}

	/** Resets the operation, making it as a simpe extrusion **/
	public void reset() {
		for (int i = 0; i < numOperations; ++i) {
			operation [i] = false;
		}
	}

	/** Forces to make a distance change**/
	public void doDistanceOperation() {
		operation [distance]= true;
	}

	/** Forces to make a direction change**/
	public void doDirectionOperation() {
		operation [direction]= true;
	}

	/** Forces to make a scale **/
	public void doScaleOperation() {
		operation [scale]= true;
	}

	/** Forces to make a rotation **/
	public void doRotationOperation() {
		operation [rotation] = true;
	}
		
}
