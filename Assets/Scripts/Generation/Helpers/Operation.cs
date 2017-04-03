using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Operation<T> {

	private int countdown;
	private T value;

	public Operation() {
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


	public void setValue(T newValue) {
		value = newValue;
	}

	public void setCountdown(int newCountdown) {
		countdown = newCountdown;
	}
	public void decreaseCountdown() {
		countdown--;
	}
}


