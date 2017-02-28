using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator : MonoBehaviour {

	List<int> mTriangles;
	List<Vector3> mVertices;


	void triangulateQuad (int bl, int br, int tl, int tr) {
		/**		The quad vertices seen from outside the cave:
		 * 		tl___tr
		 * 		|	 |
		 * 		bl___br
		 **/
		//Left-hand!
		mTriangles.Add (bl); mTriangles.Add (tr); mTriangles.Add (tl);

		mTriangles.Add (bl); mTriangles.Add (br); mTriangles.Add (tr);
	}

	/** Makes the triangulation between two polyline, checking they
	 * have the same size **/
	void triangulatePolylines(int[] pol1, int[] pol2) {
		if (pol1.Length != pol2.Length)
			Debug.Log ("The two polylines do not have the same length!");

		for (int i = 0; i < pol1.Length-1; ++i) {
			triangulateQuad (pol1 [i], pol1 [i + 1], pol2 [i], pol2 [i + 1]);
		}
		triangulateQuad (pol1 [pol1.Length - 1], pol1 [0], pol2 [pol2.Length - 1], pol2 [0]);	
	}


	/** From the vertices of an existing polyline, it creates a new new one
	 * with the same number of vertices and following some direction and at some distance**/
	Vector3[] extrude(Vector3[] originPolyPos, int[] originPolyIndex,  Vector3 direction, int distance) {
		//TODO: create polyline data structure

		//Create the new polyline
		Vector3[] newPolyPos = new Vector3[originPolyPos.Length];
		int[] newPolyIndex = new int[originPolyIndex.Length];
		for (int i = 0; i < originPolyPos.Length; ++i) {//Generate the new vertices
			//Add vertex to polyline
			newPolyPos [i] = originPolyPos [i] + direction*distance;
			//Add index vertex to polyline
			newPolyIndex [i] = mVertices.Count;//assign new index
			Debug.Log(mVertices.Count);
			//Add new vertex
			mVertices.Add(newPolyPos[i]);
		}

		//Triangulate from origin to new polyline as a tube/cave shape
		triangulatePolylines (originPolyIndex, newPolyIndex);

		return newPolyPos;//TODO: return poly position + indices
	}

	/** Transforms the polylines list to a vertex array**/
	/*Vector3[] polylinesToVertices (List<Vector3[]> polylines,int verticesNum) {
		Vector3[] vertices = new Vector3[verticesNum];
		int index = 0;
		foreach (Vector3[] polyline in polylines) {
			foreach (Vector3 vertex in polyline) {
				vertices[index] = vertex;
				++index;
			}
		}
		return vertices;
	}*/

	void Start () {
		Mesh mesh = new Mesh ();
		mVertices = new List<Vector3>();
		mTriangles = new List<int>();

		Vector3[] iniPolyPos = new Vector3[4] {new Vector3 (0.0f, 0.0f, 0.0f),new Vector3 (0.0f, 1.0f, 0.0f),
								new Vector3 (1.0f, 1.0f, 0.0f), new Vector3 (1.0f, 0.0f, 0.0f)};
		int[] iniPolyIndex = new int[4] { 0, 1, 2, 3 };

		foreach (Vector3 v in iniPolyPos) {
			mVertices.Add (v);
		}

		/*mVertices.Add(new Vector3 (0.0f, 0.0f, 0.0f));
		mVertices.Add(new Vector3 (0.0f, 1.0f, 0.0f));
		mVertices.Add(new Vector3 (1.0f, 1.0f, 0.0f));
		mVertices.Add(new Vector3 (1.0f, 0.0f, 0.0f));*/

		Vector3 dir = new Vector3 (0.0f, 0.0f, 1.0f);
		Vector3[] newPoly = extrude (iniPolyPos, iniPolyIndex, dir, 10);

		mesh.vertices = mVertices.ToArray();
		mesh.triangles = mTriangles.ToArray ();

		//mesh.RecalculateNormals ();
		//mesh.RecalculateBounds();

		//Assign the created mesh to the one we are storing and visualizing
		GetComponent<MeshFilter> ().mesh = mesh;

	}

}
