using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Represent all the operations to apply to a extrusion **/
public class ExtrusionOperations  {

	public enum stalgmOp {
		Stalagmite, Stalactite, Pillar
	}

	//Different operations
	private Operation<float> distance;
	private LerpOperation direction;
	private Operation<float> scale;
	private Operation<float> rotate;
	private bool hole; //To make a hole on this extrusion or not
	private Operation<stalgmOp> stalagmite;	//To make an stalgmite on this extrusion or not
	//private Operation<bool> pointLight;	//To create a point light on this extrusion or not
	private int canIntersect;//Index of the BB this extrusion can intersect with

	/** Creator, makes a just extrusion operation**/
	public ExtrusionOperations() {
		distance = new Operation<float> ();
		direction = new LerpOperation ();
		scale = new Operation<float> ();
		rotate = new Operation<float> ();
		stalagmite = new Operation<stalgmOp> ();
		//pointLight = new Operation<bool> ();
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
		//pointLight = new Operation<bool> (original.pointLight);
		canIntersect = original.canIntersect;
	}

	//*********Getters**********//
	/** Returns if no operations need to be done, just the extrusion **/
	public bool justExtrude() {
		return (distance.needApply() || direction.needApply() || scale.needApply() ||
			 rotate.needApply() || stalagmite.needApply() || holeOperation() /*|| pointLight.needApply()*/);
	}

	/** Returns the distance operation **/
	public Operation<float> distanceOperation() {
		return distance;
	}

	/** Returns the direction operation **/
	public LerpOperation directionOperation() {
		return direction;
	}

	/** Returns the scale operation **/
	public Operation<float> scaleOperation() {
		return scale;
	}

	/** Returns the rotate operation **/
	public Operation<float> rotateOperation() {
		return rotate;
	}

	/** Returns the stalagmite operation **/
	public Operation<stalgmOp> stalagmiteOperation() {
		return stalagmite;
	}

	/** Returns the point light operation **/
	/*public Operation<bool> pointLightOperation() {
		return pointLight;
	}*/

	/** Returns if a hole needs to be done on this extrusion**/
	public bool holeOperation() {
		return hole;
	}

	/** Returns the bounding box the next extrusions can intersect with **/
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
		//pointLight.decreaseWait ();
	}

	/** Forces to make a hole or cancel it **/
	public void forceHoleOperation(bool value) {
		hole = value;
	}

	/** Sets new value for the BB the actual extrusion can intersect with **/
	public void setCanIntersect(int newValue) {
		canIntersect = newValue;
	}

}
