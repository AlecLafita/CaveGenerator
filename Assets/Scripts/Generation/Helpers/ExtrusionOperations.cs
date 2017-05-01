using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Represent all the operations to apply to a extrusion **/
public class ExtrusionOperations  {

	//TODO: do all the getvalue, apply operation, etc functions to Operaton and LerpOperation class better?

	public enum stalgmOp {
		Stalagmite, Stalactite, Pillar
	}

	//Different operations

	private Operation<float> distance;

	private LerpOperation direction;

	private Operation<float> scale;

	private Operation<float> rotate;

	//To make a hole on this extrusion or not
	private bool hole; 

	//To make an stalgmite on this extrusion or not
	private Operation<stalgmOp> stalagmite;

	//To create a point light on this extrusion or not
	private Operation<bool> pointLight;

	//Index of the BB this extrusion can intersect with
	private int canIntersect;

	/** Creator, makes a just extrusion operation**/
	public ExtrusionOperations() {
		distance = new Operation<float> ();
		direction = new LerpOperation ();
		scale = new Operation<float> ();
		rotate = new Operation<float> ();
		stalagmite = new Operation<stalgmOp> ();
		pointLight = new Operation<bool> ();
		canIntersect = -1;
	}

	/**Clone creator **/
	public ExtrusionOperations(ExtrusionOperations original) {
		distance = new Operation<float> (original.distance);
		direction = new LerpOperation (original.direction);
		scale = new Operation<float> (original.scale);
		rotate = new Operation<float> (original.rotate);
		hole = original.hole;
		stalagmite = new Operation<stalgmOp>(original.stalagmite);
		pointLight = new Operation<bool> (original.pointLight);
		canIntersect = original.canIntersect;
	}

	//*********Getters**********//

	/** Returns if no operations need to be done, just the extrusion **/
	public bool justExtrude() {
		return (distanceOperation() || directionOperation() || scaleOperation() || rotationOperation() || holeOperation());
	}

	/**Returns if a distance value has to be generated**/
	public bool generateDistance() {
		return !distanceOperation() && (distance.getWait () <= 0);
	} 
	/** Returns if a distance change needs to be done on this extrusion**/
	public bool distanceOperation() {
		return distance.getCountdown()>0;
	}

	/**Returns if a direction value has to be generated**/
	public bool generateDirection() {
		return !directionOperation() && (direction.getWait () <= 0);
	}
	/** Returns if a direction change needs to be done on this extrusion**/
	public bool directionOperation() {
		return direction.getCountdown()>0;
	}

	/**Returns if a scale value has to be generated**/
	public bool generateScale() {
		return !scaleOperation() && (scale.getWait () <= 0);
	}
	/** Returns if a scale needs to be done on this extrusion**/
	public bool scaleOperation() {
		return scale.getCountdown()>0;
	}

	/**Returns if a rotation value has to be generated**/
	public bool generateRotation() {
		return !rotationOperation() && (rotate.getWait () <= 0);
	}
	/** Returns if a rotation needs to be done on this extrusion**/
	public bool rotationOperation() {
		return rotate.getCountdown()>0;
	}

	/** Returns if a hole needs to be done on this extrusion**/
	public bool holeOperation() {
		return hole;
	}

	/**Returns if a stalagmite has to be generated**/
	public bool generateStalagmite() {
		return !stalagmiteOperation() && (stalagmite.getWait () <= 0);
	}
	/**Returns if a stalagmite needs to be done on this extrusion**/
	public bool stalagmiteOperation() {
		return stalagmite.getCountdown()>0;
	}

	/**Returns if a point light has to be generated  **/
	public bool generatePointLight() {
		return !pointLightOperation () && (pointLight.getWait () <= 0);
	}
	/**Returns if a point light nees to be done on this extrusion **/
	public bool pointLightOperation() {
		return pointLight.getCountdown () > 0;
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
		stalagmite.decreaseWait ();
		pointLight.decreaseWait ();
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

	/**Sets extrusion waits for stalagmite operation **/
	public void setStalagmWait(int waitExtr) {
		stalagmite.setWait (waitExtr);
	}
	/** Forces to make an stalagmite **/
	public void forceStalagmiteOperation(stalgmOp value) {
		stalagmite.setCountdown (1);
		stalagmite.setValue(value);
	}

	/**Sets extrusion waits for point light operation **/
	public void setPointLightWait(int waitExtr) {
		pointLight.setWait (waitExtr);
	}
	/**Forces to make an point light operation **/
	public void forcePointLightOperation(bool value) {
		pointLight.setCountdown (1);
		pointLight.setValue (value);
	}

	/** Sets new value for the BB the actual extrusion can intersect with **/
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

	public stalgmOp applyStalagmite() {
		stalagmite.decreaseCountdown ();
		return stalagmite.getValue ();
	}

	public bool applyPointLight() {
		pointLight.decreaseCountdown ();
		return pointLight.getValue();
	}

}
