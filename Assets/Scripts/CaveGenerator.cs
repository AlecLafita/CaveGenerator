using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

public class CaveGenerator : MonoBehaviour {

	List<int> mTriangles;
	List<Vector3> mVertices;

	public int extrudeTimes = 100;

	bool hole = true; //TODO: change this!

	/** Makes the triangulation from the triangle vertices**/
	void triangulateTriangle(int v1, int v2, int v3) {
		mTriangles.Add (v1); mTriangles.Add (v2);mTriangles.Add (v3);
	}

	/** Triangulates the quad iff is not part of a hole**/
	void triangulateQuad(Vertex bl, Vertex br, Vertex tl, Vertex tr) {
		/**		The quad vertices seen from outside the cave:
		 * 		tl___tr
		 * 		|	 |
		 * 		bl___br
		 **/
		if (!((bl.getInHole() && tr.getInHole()) || (tl.getInHole() && br.getInHole()))) {
			//Left-hand!
			mTriangles.Add (bl.getIndex()); mTriangles.Add (tr.getIndex()); mTriangles.Add (tl.getIndex());
			mTriangles.Add (bl.getIndex()); mTriangles.Add (br.getIndex()); mTriangles.Add (tr.getIndex());
		}
	}

	/** Makes the triangulation between two polylines, checking they
	 * have the same size **/
	void triangulatePolylines(Polyline pol1, Polyline pol2) {
		if (pol1.getSize () != pol2.getSize ()) {//TODO : throw exception
			Debug.Log ("The two polylines do not have the same length!");
			return;
		}
		for (int i = 0; i < pol1.getSize(); ++i) {
			triangulateQuad(pol1.getVertex(i), pol1.getVertex(i+1), pol2.getVertex(i), pol2.getVertex(i+1));
		}
	}

	/** Closes a polyline by triangulating all it's vertices with it's baricenter**/
	void closePolyline(Polyline poly) {
		Vector3 baricenter = poly.calculateBaricenter ();
		int baricenterIndex = mVertices.Count;
		mVertices.Add (baricenter);
		Debug.Log (baricenter); 
		for (int i = 0; i < poly.getSize(); ++i) {
			triangulateTriangle (poly.getVertex (i).getIndex(), poly.getVertex (i + 1).getIndex(), baricenterIndex);
		}
	}


	/** From the vertices of an existing polyline, it creates a new new one
	 * with the same number of vertices and following some direction and at some distance**/
	void extrude(Polyline originPoly,  Vector3 direction, int distance) {
		if (extrudeTimes < 0) {
			closePolyline(originPoly);
			return;
		}
		
		//Create the new polyline
		Polyline newPoly = new Polyline(originPoly.getSize());
		for (int i = 0; i < originPoly.getSize(); ++i) {//Generate the new vertices
			//Add vertex to polyline
			newPoly.extrudeVertex(i, originPoly.getVertex(i).getPosition(), direction,distance);
			//Add index vertex to polyline
			newPoly.getVertex(i).setIndex(mVertices.Count);
			//Add the new vertex to the mesh
			mVertices.Add(newPoly.getVertex(i).getPosition());
		}

		//Make holes: mark some vertices (from old and new polyline) and form a new polyline
		if (!hole) {
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

			hole = true;
			direction = new Vector3 (0.0f, 1.0f, 0.0f);
			extrude (polyHole, direction, distance);
		}

		//Triangulate from origin to new polyline as a tube/cave shape
		triangulatePolylines (originPoly, newPoly);
		--extrudeTimes;
		extrude(newPoly,direction,distance);
	}

	public void startGeneration (InitialPolyline iniPol) {
		mVertices = new List<Vector3>();
		mTriangles = new List<int>();

		//Add the first polyline vertices to the mesh
		for (int i = 0; i < iniPol.getSize (); ++i) {
			mVertices.Add (iniPol.getVertex (i).getPosition ());
		}

		//Start the generation
		extrude (iniPol,  new Vector3 (0.0f, 0.0f, 1.0f), 10);


		//Assign the vertices and triangles created to a mesh
		Mesh mesh = new Mesh ();
		//mesh.vertices = mVertices.ToArray(); //Slower
		mesh.SetVertices (mVertices);
		//mesh.triangles = mTriangles.ToArray ();
		mesh.SetTriangles (mTriangles,0);
		mesh.RecalculateNormals ();
		mesh.RecalculateBounds();

		//Assign the created mesh to the one we are storing and visualizing
		GetComponent<MeshFilter> ().mesh = mesh;
	}
		
	void OnDrawGizmos() { //For debug purposes
		if (!Application.isPlaying) return; //Avoid error messages after stopping
		Vector3[] vertices = mVertices.ToArray ();
		for (int i = 0; i < vertices.Length; ++i) {
			Gizmos.DrawWireSphere (center: vertices [i], radius: 0.2f);
		}
	}
}
