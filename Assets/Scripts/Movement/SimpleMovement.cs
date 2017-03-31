using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovement : MonoBehaviour {

	public float speedY = 15.0f;
	public float speedX = 15.0f;
	public float speedZ = 15.0f;

	// Update is called once per frame
	void Update () {

		transform.Translate (0.0f, speedY * Time.deltaTime*Input.mouseScrollDelta.y, 0.0f,Space.Self);

		if (Input.GetKey(KeyCode.W))
			transform.Translate (0.0f, 0.0f, speedZ*Time.deltaTime,Space.Self);
		if (Input.GetKey(KeyCode.S))
			transform.Translate (0.0f, 0.0f, -speedZ*Time.deltaTime,Space.Self);
		
		if (Input.GetKey(KeyCode.D))
			transform.Translate (speedX*Time.deltaTime, 0.0f, 0.0f,Space.Self);
		if (Input.GetKey(KeyCode.A))
			transform.Translate (-speedX*Time.deltaTime,0.0f, 0.0f,Space.Self);

		transform.Rotate (0.0f, Input.GetAxis("Mouse X") , 0.0f, Space.Self);
	}
}
