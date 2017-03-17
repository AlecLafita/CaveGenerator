using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;


/** Class that manages the cave generation **/
public class CaveGenerator : MonoBehaviour {

	Geometry.Mesh proceduralMesh;
	public int totalExtrudeTimes = 200; //How many times an extrusion can be applied, acts as a countdown
	public int maxExtrudeTimes = 40; // How many times an extrusion can be applied from a hole
									//TODO: consider to deccrement this value as holes are created


	/** Function to be called in order to start generating the cave **/
	public void startGeneration (InitialPolyline iniPol) {
		//Create the mesh that will be modified during the cave generation
		proceduralMesh = new Geometry.Mesh (iniPol);

		//Start the generation
		//generateRecursive (DecisionGenerator.ExtrusionOperation.ExtrudeOnly, iniPol, new Vector3 (0.0f, 0.0f, 0.5f), DecisionGenerator.Instance.generateDistance(), 0);
		generateIterativeStack (iniPol, new Vector3 (0.0f, 0.0f, 0.5f), DecisionGenerator.Instance.generateDistance ());

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


		
	void generateRecursive(DecisionGenerator.ExtrusionOperation operation, Polyline originPoly,  Vector3 direction, float distance, int actualExtrusionTimes) {
		//TODO: 6422 recursive calls gives stack overflow error, check this!

		//Extrusion will be done, update the counter
		--totalExtrudeTimes;
		++actualExtrusionTimes;

		//Base case, triangulate the actual polyline as a polygon to close the cave
		if (totalExtrudeTimes < 0 || actualExtrusionTimes > maxExtrudeTimes) { 
			proceduralMesh.closePolyline(originPoly);
			return;
		}

		//Generate the new polyline applying the operation
		Polyline newPoly = extrude (operation, originPoly, ref direction, ref distance);
		//Make hole
		if (DecisionGenerator.Instance.makeHole(actualExtrusionTimes)) {
			Polyline polyHole = makeHole (originPoly, newPoly);
			Vector3 directionHole = polyHole.calculateNormal();
			generateRecursive (DecisionGenerator.ExtrusionOperation.ExtrudeOnly, polyHole, directionHole, DecisionGenerator.Instance.generateDistance(), 0);
		}

		//Triangulate from origin to new polyline as a tube/cave shape
		proceduralMesh.triangulatePolylines (originPoly, newPoly);
		//Set next operation and extrude
		operation = DecisionGenerator.Instance.generateNextOperation();
		generateRecursive(operation,newPoly,direction,distance,actualExtrusionTimes);
	}
		
	//EXTRUSION ALTERNATIVE: make it iterative and each time a hole need to be done, push it:
	//(more holes at the end). This WON'T MADE the same effect than the recursive function
	void generateIterativeStack(Polyline originPoly, Vector3 direction, float distance) {
		//Stacks for saving the hole information
		//This with generating holes with MoreExtrMoreProb is a bad combination, as it will made the impression of
		//only one path being followed (no interections)
		Stack<Polyline> polylinesStack = new Stack<Polyline> ();
		Stack<Vector3> holesDirectionStack = new Stack<Vector3> ();
		polylinesStack.Push(originPoly);
		holesDirectionStack.Push (direction);
		Polyline newPoly;
		while (polylinesStack.Count > 0) {
			originPoly = polylinesStack.Pop ();
			direction = holesDirectionStack.Pop ();
			int actualExtrusionTimes = 0;
			DecisionGenerator.ExtrusionOperation operation = DecisionGenerator.ExtrusionOperation.ExtrudeOnly;

			while (totalExtrudeTimes >= 0 && actualExtrusionTimes <= maxExtrudeTimes) {
				//Extrusion will be done, update the counter
				--totalExtrudeTimes;
				++actualExtrusionTimes;
				//Generate the new polyline applying the operation
				newPoly = extrude (operation, originPoly, ref direction, ref distance);
				//Make holes: mark some vertices (from old and new polyline) and form a new polyline
				if (DecisionGenerator.Instance.makeHole(actualExtrusionTimes)) {
					Polyline polyHole = makeHole (originPoly, newPoly);
					Vector3 directionHole = polyHole.calculateNormal();
					polylinesStack.Push (polyHole);
					holesDirectionStack.Push (directionHole);
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
	//to a queue will made the inverse recursion effect (more holes at the beggining)

	/**It creates a new polyline from an exsiting one, applying the corresponding operation and with the direction and distance passed **/
	Polyline extrude(DecisionGenerator.ExtrusionOperation operation, Polyline originPoly, ref Vector3 direction, ref float distance) {
		//Check if distance/ direction needs to be changed
		if (operation == DecisionGenerator.ExtrusionOperation.ChangeDistance) {
			distance = DecisionGenerator.Instance.generateDistance ();
		}
		if (operation == DecisionGenerator.ExtrusionOperation.ChangeDirection) {
			direction = DecisionGenerator.Instance.generateDirection(direction);
			//This does not change the normal! The normal is always the same as all the points of a polyline are generated at 
			//the same distance that it's predecessor polyline (at the moment at least)
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
				//newPoly.scale (DecisionGenerator.Instance.generateScale());
				break;
			}
		case (DecisionGenerator.ExtrusionOperation.Rotate): {
				//newPoly.rotate (DecisionGenerator.Instance.generateRotation());
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
		// on the two polylines are at the same order (there are kind of a projection)
		int sizeHole; int firstIndex;
		DecisionGenerator.Instance.whereToDig (out sizeHole, out firstIndex);

		//Create the hole polyline by marking and adding the hole vertices(from old a new polylines)
		InitialPolyline polyHole = new InitialPolyline (sizeHole);
		//Increasing order for the old and decreasing for the new polyline in order to 
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
	/*void OnDrawGizmos() { 
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
	}*/
}
