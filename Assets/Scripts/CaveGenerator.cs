using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;


/** Class that manages the cave generation **/
public class CaveGenerator : MonoBehaviour {

	Geometry.Mesh proceduralMesh;
	public int totalExtrudeTimes = 200; //How many times an extrusion can be applied, acts as a countdown
	public int maxExtrudeTimes = 40; // How many times an extrusion can be applied from a hole
									//TODO: consider to increment this value as holes are created


	/** Function to be called in order to start generating the cave **/
	public void startGeneration (InitialPolyline iniPol) {
		//Create the mesh that will be modified during the cave generation
		proceduralMesh = new Geometry.Mesh (iniPol);

		//Start the generation
		extrude (DecisionGenerator.ExtrusionOperation.ExtrudeOnly, iniPol, new Vector3 (0.0f, 0.0f, 0.5f), DecisionGenerator.Instance.generateDistance(), 0);

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
		
	/**From the vertices of an existing polyline, it creates a new new one
	 * with the same number of vertices and following some direction and at some distance **/
	void extrude(DecisionGenerator.ExtrusionOperation operation, Polyline originPoly,  Vector3 direction, float distance, int actualExtrusionTimes) {
		//Extrusion will be done, update the counter
		--totalExtrudeTimes;
		++actualExtrusionTimes;

		//Base case, triangulate the actual polyline as a polygon to close the cave
		if (totalExtrudeTimes < 0 || actualExtrusionTimes > maxExtrudeTimes) { 
			proceduralMesh.closePolyline(originPoly);
			return;
		}

		//Check here if distance/ direction needs to be changed
		/*if (operation == DecisionGenerator.ExtrusionOperation.ChangeDistance) {
			distance = DecisionGenerator.Instance.generateDistance ();
		}*/

		//Create the new polyline from the actual one
		Polyline newPoly = new Polyline(originPoly.getSize());
		for (int i = 0; i < originPoly.getSize(); ++i) { //Generate the new vertices
			//Add vertex to polyline
			newPoly.extrudeVertex(i, originPoly.getVertex(i).getPosition(), direction,distance);
			//Add index vertex to polyline
			newPoly.getVertex(i).setIndex(proceduralMesh.getNumVertices());
			//Add the new vertex to the mesh
			proceduralMesh.addVertex(newPoly.getVertex(i).getPosition());
		}
		newPoly.rotate (20.0f);
		switch (operation) {
		case (DecisionGenerator.ExtrusionOperation.Scale) : {
				//newPoly.scale (1.1f);
				break;
		}
		default:
			break;
		}

		//Make holes: mark some vertices (from old and new polyline) and form a new polyline
		if (DecisionGenerator.Instance.makeHole(actualExtrusionTimes)) {
			//Debug.Log ("Making hole");
			Polyline polyHole = new Polyline (4);

			//TODO: not hardcode this
			originPoly.getVertex(0).setInHole(true);
			originPoly.getVertex(1).setInHole(true);
			newPoly.getVertex(0).setInHole(true);
			newPoly.getVertex(1).setInHole(true);

			polyHole.setVertex (0, originPoly.getVertex (0));
			polyHole.setVertex (1, originPoly.getVertex (1));
			polyHole.setVertex (2, newPoly.getVertex (1));
			polyHole.setVertex (3, newPoly.getVertex (0));

			Vector3 directionHole = polyHole.calculateNormal();
			extrude (DecisionGenerator.ExtrusionOperation.ExtrudeOnly,polyHole, directionHole, DecisionGenerator.Instance.generateDistance(), 0);
		}

		//Triangulate from origin to new polyline as a tube/cave shape
		proceduralMesh.triangulatePolylines (originPoly, newPoly);
		//Set next operation and extrude
		operation = DecisionGenerator.Instance.getNextOperation();
		extrude(operation,newPoly,direction,DecisionGenerator.Instance.generateDistance(),actualExtrusionTimes);
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
