using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;


/** Class that manages the cave generation **/
public class CaveGenerator : MonoBehaviour {

	Geometry.Mesh proceduralMesh;
	public int totalHoles = 200; //How many times an extrusion can be applied, acts as a countdown
	public int maxExtrudeTimes = 40; // How many times an extrusion can be applied from a hole
									//TODO: consider to deccrement this value as holes are created, or some random function that handles this


	/** Function to be called in order to start generating the cave **/
	public void startGeneration (InitialPolyline iniPol) {
		//Create the mesh that will be modified during the cave generation
		proceduralMesh = new Geometry.Mesh (iniPol);

		//Start the generation
		//generateRecursive (iniPol, 0.8f);
		//generateIterativeStack (iniPol, new Vector3 (0.0f, 0.0f, 0.5f), DecisionGenerator.Instance.generateDistance ());
		generateIterativeQueue (iniPol, new Vector3 (0.0f, 0.0f, 0.5f), DecisionGenerator.Instance.generateDistance ());
		Debug.Log ("Triangles generated: " + proceduralMesh.getNumTriangles ());

		//Generation finished, assign the vertices and triangles created to a Unity mesh
		UnityEngine.Mesh mesh = new UnityEngine.Mesh ();
		//mesh.vertices = mVertices.ToArray(); //Slower
		mesh.SetVertices (proceduralMesh.getVertices());
		//mesh.triangles = mTriangles.ToArray ();
		mesh.SetTriangles (proceduralMesh.getTriangles(),0);
		mesh.RecalculateNormals ();
		mesh.RecalculateBounds();

		//Assign the created mesh to the one we are storing and visualizing
		GetComponent<MeshFilter> ().mesh = mesh;
	}

	/** Generates a hallway/tunnel of the cave. It can create holes depending on the second parameter probability **/
	void generateRecursive(Polyline originPoly, float holeProb) {
		//Hole make will be done, update the counter
		--totalHoles;

		//Base case, triangulate the actual polyline as a polygon to close the cave
		if (totalHoles < 0 ) { 
			proceduralMesh.closePolyline(originPoly);
			return;
		}
		//TODO: change maxExtrudeTimes as holes are done (eg, random number between a rank)

		//Generate the actual hallway/tunnel
		DecisionGenerator.ExtrusionOperation actualOperation = DecisionGenerator.ExtrusionOperation.ExtrudeOnly;
		float actualDistance = DecisionGenerator.Instance.generateDistance();
		Vector3 actualDirection = originPoly.calculateNormal ();
		for (int i = 0; i < maxExtrudeTimes; ++i) {
			//Generate the new polyline applying the corresponding operation
			Polyline newPoly = extrude (actualOperation, originPoly, ref actualDirection, ref actualDistance);
			//Make hole?
			if (DecisionGenerator.Instance.makeHole(i,holeProb)) {
				Polyline polyHole = makeHole (originPoly, newPoly);
				generateRecursive (polyHole, holeProb);
			}
			//Triangulate from origin to new polyline as a tube/cave shape
			proceduralMesh.triangulatePolylines (originPoly, newPoly);
			//Set next operation and continue from the new polyline
			actualOperation = DecisionGenerator.Instance.generateNextOperation();
			originPoly = newPoly;
		}
		//Finally, close the actual hallway/tunnel
		proceduralMesh.closePolyline(originPoly);
	}
		
	//The following two generation methods WON'T MADE the same effect that the recursive way
	/** Generate the cave creating the holes by LIFO **/
	void generateIterativeStack(Polyline originPoly, Vector3 direction, float distance) {
		//Stacks for saving the hole information
		//This with generating holes with MoreExtrMoreProb is a bad combination, as it will made the impression of
		//only one path being followed (no bifurcations)
		Stack<Polyline> polylinesStack = new Stack<Polyline> ();
		polylinesStack.Push(originPoly);
		Polyline newPoly;
		while (polylinesStack.Count > 0) {
			//new tunnel(hole) will be done, update the counter and all the data
			--totalHoles;
			originPoly = polylinesStack.Pop ();
			direction = originPoly.calculateNormal ();
			int actualExtrusionTimes = 0;
			DecisionGenerator.ExtrusionOperation operation = DecisionGenerator.ExtrusionOperation.ExtrudeOnly;

			while (totalHoles >= 0 && actualExtrusionTimes <= maxExtrudeTimes) {
				++actualExtrusionTimes;
				//Generate the new polyline applying the operation
				newPoly = extrude (operation, originPoly, ref direction, ref distance);
				//Make holes: mark some vertices (from old and new polyline) and form a new polyline
				if (DecisionGenerator.Instance.makeHole(actualExtrusionTimes)) {
					Polyline polyHole = makeHole (originPoly, newPoly);
					polylinesStack.Push (polyHole);
				}

				//Triangulate from origin to new polyline as a tube/cave shape
				proceduralMesh.triangulatePolylines (originPoly, newPoly);
				//Set next operation and extrude
				operation = DecisionGenerator.Instance.generateNextOperation();
				originPoly = newPoly;
			}
			proceduralMesh.closePolyline(originPoly);
		}
	}

	/** Generate the cave creating the holes by FIFO **/
	void generateIterativeQueue(Polyline originPoly, Vector3 direction, float distance) {
		//Queues for saving the hole information
		Queue<Polyline> polylinesStack = new Queue<Polyline> ();
		polylinesStack.Enqueue(originPoly);
		Polyline newPoly;
		while (polylinesStack.Count > 0) {
			//new tunnel(hole) will be done, update the counter and all the data
			--totalHoles;
			originPoly = polylinesStack.Dequeue ();
			direction = originPoly.calculateNormal ();
			int actualExtrusionTimes = 0;
			DecisionGenerator.ExtrusionOperation operation = DecisionGenerator.ExtrusionOperation.ExtrudeOnly;

			while (totalHoles >= 0 && actualExtrusionTimes <= maxExtrudeTimes) {
				++actualExtrusionTimes;
				//Generate the new polyline applying the operation
				newPoly = extrude (operation, originPoly, ref direction, ref distance);
				//Make holes: mark some vertices (from old and new polyline) and form a new polyline
				if (DecisionGenerator.Instance.makeHole(actualExtrusionTimes)) {
					Polyline polyHole = makeHole (originPoly, newPoly);
					polylinesStack.Enqueue (polyHole);
				}

				//Triangulate from origin to new polyline as a tube/cave shape
				proceduralMesh.triangulatePolylines (originPoly, newPoly);
				//Set next operation and extrude
				operation = DecisionGenerator.Instance.generateNextOperation();
				originPoly = newPoly;
			}
			proceduralMesh.closePolyline(originPoly);
		}
	}

	private float maxNormalDirectionAngle = 40.0f;
	/**It creates a new polyline from an exsiting one, applying the corresponding operation and with the direction and distance passed **/
	Polyline extrude(DecisionGenerator.ExtrusionOperation operation, Polyline originPoly, ref Vector3 direction, ref float distance) {
		//Check if distance/ direction needs to be changed
		if (operation == DecisionGenerator.ExtrusionOperation.ChangeDistance) {
			distance = DecisionGenerator.Instance.generateDistance ();
		}
		if (operation == DecisionGenerator.ExtrusionOperation.ChangeDirection) {
			//This does not change the normal! The normal is always the same as all the points of a polyline are generated at 
			//the same distance that it's predecessor polyline (at the moment at least)

			//Vector3 newDirection = DecisionGenerator.Instance.changeDirection(direction);
			Vector3 newDirection = DecisionGenerator.Instance.generateDirection();
			//Avoid intersection and narrow halways between the old and new polylines by setting an angle limit
			//(90 would produce a plane and greater than 90 would produce an intersection)
			if (Vector3.Angle(newDirection,originPoly.calculateNormal()) < maxNormalDirectionAngle) {
				direction = newDirection; 
			}
		}

		//Create the new polyline from the actual one
		Polyline newPoly = new Polyline(originPoly.getSize());
		for (int i = 0; i < originPoly.getSize(); ++i) { //Generate the new vertices
			//Add vertex to polyline
			newPoly.extrudeVertex(i, originPoly.getVertex(i).getPosition(), direction,distance);
			//Add the index to vertex
			newPoly.getVertex(i).setIndex(proceduralMesh.getNumVertices());
			//Add the new vertex to the mesh
			proceduralMesh.addVertex(newPoly.getVertex(i).getPosition());
		}
		//Apply operations, if any
		switch (operation) {
		case (DecisionGenerator.ExtrusionOperation.Scale) : {
				newPoly.scale (DecisionGenerator.Instance.generateScale());
				break;
			}
		case (DecisionGenerator.ExtrusionOperation.Rotate): {
				newPoly.rotate (DecisionGenerator.Instance.generateRotation());
				break;
			}
		default:
			break;
		}

		return newPoly;
	}

	/** Makes a hole betwen two polylines and return this hole as a new polyline **/
	Polyline makeHole(Polyline originPoly, Polyline destinyPoly) {
		//TODO: more than one hole, Make two holes on same polylines pairs can cause intersections!

		// Decide how and where the hole will be done, take advantatge indices
		// on the two polylines are at the same order (the new is kind of a projection of the old)
		int sizeHole; int firstIndex;
		DecisionGenerator.Instance.whereToDig (originPoly.getSize(), out sizeHole, out firstIndex);

		//Create the hole polyline by marking and adding the hole vertices (from old a new polylines)
		InitialPolyline polyHole = new InitialPolyline (sizeHole);
		//Increasing order for the origin and decreasing for the destiny polyline in order to 
		//make a correct triangulation
		int i = 0;
		while (i < sizeHole / 2) {
			originPoly.getVertex (firstIndex +i).setInHole (true);
			polyHole.addVertex (originPoly.getVertex (firstIndex +i));
			++i;
		}
		//at this point i = sizeHole / 2;
		while (i > 0) {
			--i;
			destinyPoly.getVertex (firstIndex+i).setInHole (true);
			polyHole.addVertex (destinyPoly.getVertex (firstIndex+i));
		}

		return polyHole;
	}

	/** For debug purposes **/
	void OnDrawGizmos() { 
		//Avoid error messages after stopping
		if (!Application.isPlaying) return; 

		//Draw triangles vertices
		Vector3[] vertices = proceduralMesh.getVertices().ToArray ();
		for (int i = 0; i < vertices.Length; ++i) {
			Gizmos.DrawWireSphere (vertices [i], 0.1f);
		}

		//Draw triangles edges
		int[] triangles = proceduralMesh.getTriangles().ToArray();
		Gizmos.color = Color.blue;
		for (int i = 0; i < triangles.Length; i += 3) {
			Gizmos.DrawLine (vertices [triangles[i]], vertices [triangles[i + 1]]);
			Gizmos.DrawLine (vertices [triangles[i+1]], vertices [triangles[i + 2]]);
			Gizmos.DrawLine (vertices [triangles[i+2]], vertices [triangles[i]]);
		}
	}
}
