using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;


/** Class that manages the cave generation **/
public class CaveGenerator : MonoBehaviour {

	Geometry.Mesh proceduralMesh;
	public int totalExtrudeTimes = 200; //How many times an extrusion can be applied, acts as a countdown
	public int maxExtrudeTimes = 40; // How many times an extrusion can be applied from a hole


	/** Function to be called in order to start generating the cave **/
	public void startGeneration (InitialPolyline iniPol) {
		//Create the mesh that will be modified during the cave generation
		proceduralMesh = new Geometry.Mesh (iniPol);

		//Start the generation
		extrude (iniPol, new Vector3 (0.0f, 0.0f, 1.0f), DecisionGenerator.Instance.generateDistance(), 0);

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
	void extrude(Polyline originPoly,  Vector3 direction, float distance, int actualExtrusionTimes) {
		//Extrusion will be done, reduce the counter
		--totalExtrudeTimes;
		++actualExtrusionTimes;
		//Base case, triangulate the polyline as a polygon
		if (totalExtrudeTimes < 0 || actualExtrusionTimes > maxExtrudeTimes) { 
			proceduralMesh.closePolyline(originPoly);
			return;
		}
		
		//Create the new polyline
		Polyline newPoly = new Polyline(originPoly.getSize());
		for (int i = 0; i < originPoly.getSize(); ++i) { //Generate the new vertices
			//Add vertex to polyline
			newPoly.extrudeVertex(i, originPoly.getVertex(i).getPosition(), direction,distance);
			//Add index vertex to polyline
			newPoly.getVertex(i).setIndex(proceduralMesh.getNumVertices());
			//Add the new vertex to the mesh
			proceduralMesh.addVertex(newPoly.getVertex(i).getPosition());
		}

		//Make holes: mark some vertices (from old and new polyline) and form a new polyline
		if (DecisionGenerator.Instance.makeHole()) {
			Polyline polyHole = new Polyline (4);

			//TODO: not hardcode this
			originPoly.getVertex(2).setInHole(true);
			originPoly.getVertex(3).setInHole(true);
			newPoly.getVertex(2).setInHole(true);
			newPoly.getVertex(3).setInHole(true);

			polyHole.setVertex (0, originPoly.getVertex (2));
			polyHole.setVertex (1, originPoly.getVertex (3));
			polyHole.setVertex (2, newPoly.getVertex (3));
			polyHole.setVertex (3, newPoly.getVertex (2));

			direction = new Vector3 (0.0f, 1.0f, 0.0f);
			extrude (polyHole, direction, DecisionGenerator.Instance.generateDistance(), 0);
		}

		//Triangulate from origin to new polyline as a tube/cave shape
		proceduralMesh.triangulatePolylines (originPoly, newPoly);
		extrude(newPoly,direction,DecisionGenerator.Instance.generateDistance(),actualExtrusionTimes);
	}
		
		
	/** For debug purposes **/
	void OnDrawGizmos() { 
		if (!Application.isPlaying) return; //Avoid error messages after stopping
		Vector3[] vertices = proceduralMesh.getVertices().ToArray ();
		for (int i = 0; i < vertices.Length; ++i) {
			Gizmos.DrawWireSphere (center: vertices [i], radius: 0.2f);
		}
	}
}
