using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpOperation {

	//What doesn't allow this class to begeneric the Lerp, as it is only for Vector3 and float(Mathf)
	private Vector3 iniValue; //origin value to interpolate with
	private Vector3 fiValue; //final value to interpolate with

	private int countdown;//How many extrusions to still apply this operation
	private int numSteps; //Num extrusions to go from ini to fi value interpolating
	private int waitExtrusions;//How many extrusions to wait until apply this operation

	//*********Creators**********//
	public LerpOperation() {
		iniValue = Vector3.zero;
		fiValue = Vector3.zero;
		countdown = numSteps = 0;
	}

	/**Clone Creator **/
	public LerpOperation(LerpOperation original) {
		iniValue = original.iniValue;
		fiValue = original.fiValue;
		countdown = original.countdown;
		numSteps = original.numSteps;
		waitExtrusions = original.waitExtrusions;
	}

	//*********Getters**********//

	public Vector3 getInitialValue() {
		return iniValue;
	}

	public Vector3 getFiValue() {
		return fiValue;
	}

	public int getCountdown() {
		return countdown;
	}

	public int getWait() {
		return waitExtrusions;
	}

	/**Returns a value for this operation has to be generated**/
	public bool needGenerate() {
		return !needApply() && (getWait () <= 0);
	} 

	/** Returns if this operation needs to be done for actual extrusion**/
	public bool needApply() {
		return getCountdown()>0;
	}


	//*********Setters**********//

	public void setIniValue(Vector3 newValue) {
		iniValue = newValue;
	}

	public void setFiValue(Vector3 newValue) {
		fiValue = newValue;
	}

	public void setCountdown(int newCountdown) {
		countdown = newCountdown;
		numSteps = newCountdown;
	}
	public void decreaseCountdown() {
		countdown--;
	}
	public void restartCountdown() {
		countdown = numSteps;
	}

	public void setWait(int newWait) {
		waitExtrusions = newWait;
	}
	public void decreaseWait() {
		waitExtrusions--;
	}

	public void forceOperation(int times, Vector3 valueIni, Vector3 valueFi) {
		setCountdown (times);
		setIniValue (valueIni);
		setFiValue (valueFi);
	}
	/**Forces to make a direction change, only final direction is needed **/
	public void forceOperation(int times, Vector3 valueFi) {
		setCountdown (times);
		//Set as the initial one the previous final one, this way it will start the interpolation from it
		setIniValue (getFiValue()); 
		setFiValue (valueFi);
	}

	//*********Operation application**********//

	/** Returns the corresponding interpolation value taking into account the extrusion step **/
	public Vector3 apply() {
		//If a direction change is needed, it will be interpolated between the two directions,
		if (needApply ()) {
			decreaseCountdown ();
			return applyLerp ();
		} 
		//Otherwise it will take the last direction applied
		else
			return getFiValue ();
	}
		
	private Vector3 applyLerp() {
		float weight = ((float)numSteps-(float)countdown)/(float)numSteps;//Between 0 and 1, interpolation weigth
		Vector3 value =  Vector3.Slerp (iniValue, fiValue, weight);
		return value.normalized;
	}
}
