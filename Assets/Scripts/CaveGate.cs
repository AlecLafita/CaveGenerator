using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

/** This scripts waits the user to select the points that will form the cave gate (initial polyline). 
	 * Once all the points have been selected thorugh clicks, it calls the script that generates
	 * the cave. The initial points are all on the XY plane and must be CLOCKWISE **/
public class CaveGate : MonoBehaviour {

	public Camera cam;
	public int gateSize = 3; //Number of points the cave'gate will have
	public float caveDistance = 10.0f; //Depth(z) where the cave will start on, from the camera
	private int pointsSelected; //Number of points the user has selected
	private bool generatorCalled; //In order to generate the cave just once
	InitialPolyline initialPoints;

	void Start () {
		initialPoints = new InitialPolyline(gateSize);
		pointsSelected = 0;
		generatorCalled = false;
	}
	
	void Update () {
		if (Input.GetMouseButtonDown (0) && pointsSelected < gateSize) { //left click
			Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f);
			pos = cam.ScreenToWorldPoint(pos);
			initialPoints.addPosition (pos);
			Debug.Log (pos);
			++pointsSelected;
			if (pointsSelected == gateSize) {
				Debug.Log ("All points selected, press right mouse button");
			}
		}

		if (Input.GetMouseButtonDown (1) && pointsSelected==gateSize && !generatorCalled) {//right click
			//Generate the cave when the user has selected all the points
			cam.ResetProjectionMatrix();
			cam.enabled = false;
			cam.GetComponent<AudioListener> ().enabled = false;
			Debug.Log("Starting generation");
			//TODO:check it's clockwise. In case it's not, transform it
			initialPoints.initializeIndices();
			GetComponent<CaveGenerator>().startGeneration(initialPoints);
			generatorCalled = true;
			Debug.Log ("Cave generated");
		}
		
	}
}
