using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpOperation {

	//What doesn't allow this class to begeneric the Lerp, as it is only for Vector3 and float(Mathf)
	private Vector3 iniValue;
	private Vector3 fiValue;

	private int countdown;
	private int numSteps;

	public LerpOperation() {
		iniValue = Vector3.zero;
		fiValue = Vector3.zero;
		countdown = numSteps = 0;
	}

	public LerpOperation(LerpOperation original) {
		iniValue = original.iniValue;
		fiValue = original.fiValue;
		countdown = original.countdown;
		numSteps = original.numSteps;
	}

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

	public Vector3 applyLerp() {
		float weight = ((float)numSteps-(float)countdown)/(float)numSteps;//Between 0 and 1, interpolation weigth
		Vector3 value =  Vector3.Slerp (iniValue, fiValue, weight);
		return value.normalized;
	}

	public Vector3 getInitialValue() {
		return iniValue;
	}

	public Vector3 getFiValue() {
		return fiValue;
	}

	public int getCountdown() {
		return countdown;
	}
	public void decreaseCountdown() {
		countdown--;
	}
	public void restartCountdown() {
		countdown = numSteps;
	}
}
