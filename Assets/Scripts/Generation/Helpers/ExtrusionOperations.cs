using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Represent all the operations to apply to a extrusion **/
public class ExtrusionOperations  {

	//Number of different operations
	private const int numOperations = 4;

	//Different operations

	private Operation<float> distance;

	private LerpOperation direction;

	private Operation<float> scale;

	private Operation<float> rotate;

	//To make a hole on this extrusion or not
	private bool hole; 

	//To make an stalgmite on this extrusion or not
	private bool stalagmite;

	//Index of the BB this extrusion can intersect with
	private int canIntersect;

	/** Creator, makes a just extrusion operation**/
	public ExtrusionOperations() {
		distance = new Operation<float> ();
		direction = new LerpOperation ();
		scale = new Operation<float> ();
		rotate = new Operation<float> ();
		canIntersect = -1;
	}

	/**Clone creator **/
	public ExtrusionOperations(ExtrusionOperations original) {
		distance = new Operation<float> (original.distance);
		direction = new LerpOperation (original.direction);
		scale = new Operation<float> (original.scale);
		rotate = new Operation<float> (original.rotate);
		hole = original.hole;
		stalagmite = original.stalagmite;
		canIntersect = original.canIntersect;
	}

	//*********Getters**********//
	/**Returns how many different operations are **/
	public int getNumOperations() {
		return numOperations;
	}

	/** Returns if no operations need to be done, just the extrusion **/
	public bool justExtrude() {
		return (distanceOperation() || directionOperation() || scaleOperation() || rotationOperation() || holeOperation());
	}

	/**Returns if a distance value has to be generated**/
	public bool generateDistance() {
		return !distanceOperation() && (distance.getWait () <= 0);
	} 
	/** Returns if a distance change needs to be done **/
	public bool distanceOperation() {
		return distance.getCountdown()>0;
	}

	/**Returns if a direction value has to be generated**/
	public bool generateDirection() {
		return !directionOperation() && (direction.getWait () <= 0);
	}
	/** Returns if a direction change needs to be done **/
	public bool directionOperation() {
		return direction.getCountdown()>0;
	}

	/**Returns if a scale value has to be generated**/
	public bool generateScale() {
		return !scaleOperation() && (scale.getWait () <= 0);
	}
	/** Returns if a scale needs to be done **/
	public bool scaleOperation() {
		return scale.getCountdown()>0;
	}

	/**Returns if a rotation value has to be generated**/
	public bool generateRotation() {
		return !rotationOperation() && (rotate.getWait () <= 0);
	}
	/** Returns if a rotation needs to be done **/
	public bool rotationOperation() {
		return rotate.getCountdown()>0;
	}

	/** Returns if a hole needs to be done **/
	public bool holeOperation() {
		return hole;
	}

	/**Returns if a stalagmite needs to be done **/
	public bool stalagmiteOperation() {
		return stalagmite;
	}

	public int getCanIntersect() {
		return canIntersect;
	}

	//*********Setters**********//
	/**Decreases the extrusion wait counter for all operations **/
	public void decreaseWait() {
		distance.decreaseWait ();
		direction.decreaseWait ();
		scale.decreaseWait ();
		rotate.decreaseWait ();
	}

	/**Sets extrusion waits for distance operation **/
	public void setDistanceWait(int waitExtr) {
		distance.setWait (waitExtr);
	}
	/** Forces to make a distance change**/
	public void forceDistanceOperation(int times, float value) {
		distance.setCountdown (times);
		distance.setValue (value);
	}

	/**Sets extrusion waits for direction operation **/
	public void setDirectionWait(int waitExtr) {
		direction.setWait (waitExtr);
	}
	/** Forces to make a direction change**/
	public void forceDirectionOperation(int times, Vector3 valueIni, Vector3 valueFi) {
		direction.setCountdown (times);
		direction.setIniValue (valueIni);
		direction.setFiValue (valueFi);
	}
	/**Forces to make a direction change, only final direction is needed **/
	public void forceDirectionOperation(int times, Vector3 valueFi) {
		direction.setCountdown (times);
		//Set as the initial one the previous final one, this way it will start the interpolation from it
		direction.setIniValue (direction.getFiValue()); 
		direction.setFiValue (valueFi);
	}

	/**Sets extrusion waits for scale operation **/
	public void setScaleWait(int waitExtr) {
		scale.setWait (waitExtr);
	}
	/** Forces to make a scale **/
	public void forceScaleOperation(int times, float value) {
		scale.setCountdown (times);
		scale.setValue (value);
	}

	/**Sets extrusion waits for rotate operation **/
	public void setRotateWait(int waitExtr) {
		rotate.setWait (waitExtr);
	}
	/** Forces to make a rotation **/
	public void forceRotationOperation(int times, float value) {
		rotate.setCountdown (times);
		rotate.setValue (value);
	}

	/** Forces to make a hole **/
	public void forceHoleOperation(bool value) {
		hole = value;
	}

	/** Forces to make an stalagmite **/
	public void forceStalagmiteOperation(bool value) {
		stalagmite = value;
	}

	public void setCanIntersect(int newValue) {
		canIntersect = newValue;
	}

	//*********Operations application**********//

	/**Returns the distance value. **/
	public float applyDistance() {
		distance.decreaseCountdown ();
		return distance.getValue ();
	}

	/**Returns the direction value. **/
	public Vector3 applyDirection() {
		//If a direction change is needed, it will be interpolated between the two directions,
		if (directionOperation ()) {
			direction.decreaseCountdown ();
			return direction.applyLerp ();
		} 
		//Otherwise it will take the last direction applied
		else
			return direction.getFiValue ();
	}

	/**Returns the scale value. Pre: need to check if a scale operation can be done **/
	public float applyScale() {
		scale.decreaseCountdown ();
		return scale.getValue ();
	}

	/**Returns the rotate value. Pre: need to check if a rotate operation can be done **/
	public float applyRotate() {
		rotate.decreaseCountdown ();
		return rotate.getValue ();
	}

}
