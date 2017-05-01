using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Operation<T> {

	private int countdown; //How many extrusions to still apply this operation
	private T value; //Value realted with this operation
	private int waitExtrusions; //How many extrusions to wait until apply this operation

	//*********Creators**********//

	public Operation() {
	}

	/**Clone Creator **/
	public Operation(Operation<T> original) {
		countdown = original.countdown;
		value = original.value;
		waitExtrusions = original.waitExtrusions;
	}

	//*********Getters**********//

	public T getValue() {
		return value;
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

	public void reset() {
		countdown = 0;
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

	/** Forces to make this operation on next extrusions*/
	public void forceOperation(int times, T value) {
		setCountdown (times);
		setValue (value);
	}

	//*********Operation application**********//

	/**Returns this operation value and decreases the countdown**/
	public T apply() {
		decreaseCountdown ();
		return getValue ();
	}
}


