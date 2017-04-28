using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Operation<T> {

	private int countdown;
	private T value;
	private int waitExtrusions;

	public Operation() {
	}

	public Operation(Operation<T> original) {
		countdown = original.countdown;
		value = original.value;
		waitExtrusions = original.waitExtrusions;
	}

	public void reset() {
		countdown = 0;
	}

	public T getValue() {
		return value;
	}

	public int getCountdown() {
		return countdown;
	}

	public int getWait() {
		return waitExtrusions;
	}


	public void setValue(T newValue) {
		value = newValue;
	}

	public void setCountdown(int newCountdown) {
		countdown = newCountdown;
	}
	public void decreaseCountdown() {
		countdown--;
	}

	public void setWait(int newWait) {
		waitExtrusions = newWait;
	}
	public void decreaseWait() {
		waitExtrusions--;
	}
}


