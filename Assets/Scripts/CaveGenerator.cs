using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator : MonoBehaviour {

	List<int> triangles;
	List<Vector3[]> polylines;


	void triangulateQuad (Vector3[] originPoly, Vector3[]newPoly, int i) {
		int next = (i + 1) % originPoly.Length;

		//Left-hand!
		//TODO: don't hardcode this!
		triangles.Add (0 + i);
		triangles.Add (0 + next);
		triangles.Add (4 * 1 + i);

		triangles.Add (4 * 1 + next);
		triangles.Add (4 * 1 + i);
		triangles.Add (0 + next);

	}


	/** From the vertices of an existing polyline, it creates a new new one
	 * with the same number of vertices and following some direction **/
	Vector3[] extrude(Vector3[] originPoly,  Vector3 direction) {
		//Create the new polyline
		Vector3[] newPoly= new Vector3[originPoly.Length];
		for (int i = 0; i < originPoly.Length; ++i) {
			newPoly [i] = originPoly [i] + direction;
		}

		//Triangulate from origin to new polyline as a tube/cave shape
		for (int i = 0; i < originPoly.Length; ++i) {
			triangulateQuad (originPoly, newPoly, i);
		}

		return newPoly;
	}

	/** Transforms the polylines list to a vertex array**/
	Vector3[] polylinesToVertices (List<Vector3[]> polylines,int verticesNum) {
		Vector3[] vertices = new Vector3[verticesNum];
		int index = 0;
		foreach (Vector3[] polyline in polylines) {
			foreach (Vector3 vertex in polyline) {
				vertices[index] = vertex;
				++index;
			}
		}
		return vertices;
	}

	void Start () {
		Mesh mesh = new Mesh ();
		polylines = new List<Vector3[]>(); //Contains the vertices grouped by polylines groups
		int numV = 0;
		triangles = new List<int>();

		Vector3[] exPoly = new Vector3[4];
		exPoly [0] = new Vector3 (0.0f, 0.0f, 0.0f);
		exPoly [1] = new Vector3 (0.0f, 1.0f, 0.0f);
		exPoly [2] = new Vector3 (1.0f, 1.0f, 0.0f);
		exPoly [3] = new Vector3 (1.0f, 0.0f, 0.0f);
		polylines.Add (exPoly);
		numV += 4;



		Vector3 dir = new Vector3 (0.0f,0.0f,10.0f);
		Vector3[] newPoly = extrude (exPoly, dir);
		polylines.Add (newPoly);
		numV += 4;



		mesh.vertices = polylinesToVertices (polylines,numV);
		mesh.triangles = triangles.ToArray ();

		//mesh.RecalculateNormals ();
		//mesh.RecalculateBounds();

		//Assign the created mesh to the one we are storing and visualizing
		GetComponent<MeshFilter> ().mesh = mesh;
	}

}
