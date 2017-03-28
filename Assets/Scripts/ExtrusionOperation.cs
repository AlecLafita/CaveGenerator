using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Represent all the operations to apply to a extrusion **/
public class ExtrusionOperation  {

	//Number of different operations
	private const int numOperations = 4;

	//Position of each operation
	private const int distance = 0; 
	private const int direction = 1;
	private const int scale = 2;
	private const int rotation = 3;
	private bool hole; 

	//Which operations to apply
	private bool[] operation;


	/** Creator, makes a just extrusion operation**/
	public ExtrusionOperation() {
		operation = new bool[numOperations];
		reset ();
	}

	//*********Getters**********//
	/**Returns how many different operations are **/
	public int getNumOperations() {
		return numOperations;
	}

	/** Returns if no operations need to be done, just the extrusion **/
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

	/** Returns if a hole needs to be done **/
	public bool holeOperation() {
		return hole;
	}

	//*********Setters**********//
	/** Resets the operation, making it as a simpe extrusion **/
	public void reset() {
		for (int i = 0; i < numOperations; ++i) {
			operation [i] = false;
		}
		hole = false;
	}
	/** Force one specific operation to be done, from it's position **/
	public void forceOperation(int i) {
		operation [i] = true;
	}

	/** Forces to make a distance change**/
	public void forceDistanceOperation() {
		operation [distance]= true;
	}

	/** Forces to make a direction change**/
	public void forceDirectionOperation() {
		operation [direction]= true;
	}

	/** Forces to make a scale **/
	public void forceScaleOperation() {
		operation [scale]= true;
	}

	/** Forces to make a rotation **/
	public void forceRotationOperation() {
		operation [rotation] = true;
	}

	/** Forces to make a hole **/
	public void forceHoleOperation() {
		hole = true;
		operation [distance] = true;
	}	
}
