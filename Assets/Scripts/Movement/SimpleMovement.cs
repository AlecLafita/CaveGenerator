using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovement : MonoBehaviour {

	public float speedY = 15.0f;
	public float speedX = 15.0f;
	public float speedZ = 15.0f;

	// Update is called once per frame
	void Update () {

		GetComponent<Rigidbody>().AddRelativeForce (0.0f, speedY * Time.deltaTime*Input.mouseScrollDelta.y, 0.0f,ForceMode.VelocityChange);

		if (Input.GetKey(KeyCode.W))
			GetComponent<Rigidbody>().AddRelativeForce(0.0f, 0.0f, speedZ*Time.deltaTime,ForceMode.Impulse);
		if (Input.GetKey(KeyCode.S))
			GetComponent<Rigidbody>().AddRelativeForce (0.0f, 0.0f, -speedZ*Time.deltaTime,ForceMode.Impulse);
		if (Input.GetKey(KeyCode.D))
			GetComponent<Rigidbody>().AddRelativeForce(speedX*Time.deltaTime, 0.0f, 0.0f,ForceMode.Impulse);
		if (Input.GetKey(KeyCode.A))
			GetComponent<Rigidbody>().AddRelativeForce (-speedX*Time.deltaTime,0.0f, 0.0f,ForceMode.Impulse);

		transform.Rotate (0.0f, Input.GetAxis("Mouse X") , 0.0f, Space.Self);
	}
}
