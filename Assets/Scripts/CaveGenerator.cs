using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

public class CaveGenerator : MonoBehaviour {

	List<int> mTriangles;
	List<Vector3> mVertices;
	int extrudeTimes = 100;


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

	/** Makes the triangulation between two polylines, checking they
	 * have the same size **/
	void triangulatePolylines(Polyline pol1, Polyline pol2) {
		if (pol1.getSize() != pol2.getSize()) //TODO : throw exception
			Debug.Log ("The two polylines do not have the same length!");
		for (int i = 0; i < pol1.getSize(); ++i) {
			triangulateQuad (pol1.getIndex(i), pol1.getIndex(i + 1), pol2.getIndex(i), pol2.getIndex(i + 1));
		}
	}



	/** From the vertices of an existing polyline, it creates a new new one
	 * with the same number of vertices and following some direction and at some distance**/
	void extrude(Polyline originPoly,  Vector3 direction, int distance) {
		if (extrudeTimes < 0)
			return;
		
		//Create the new polyline
		Polyline newPoly = new Polyline(originPoly.getSize());
		for (int i = 0; i < originPoly.getSize(); ++i) {//Generate the new vertices
			//Add vertex to polyline
			newPoly.extrudeVertex(i, originPoly.getPosition(i),direction,distance);
			//Add index vertex to polyline
			newPoly.setIndex(i, mVertices.Count);
			//Add the new vertex to the mesh
			mVertices.Add(newPoly.getPosition(i));
		}

		//Makes holes: mark some vertices (from old and new polyline) as a new polyline

		//Triangulate from origin to new polyline as a tube/cave shape
		triangulatePolylines (originPoly, newPoly);
		--extrudeTimes;
		extrude(newPoly,direction,distance);
	}

	void Start () {
		mVertices = new List<Vector3>();
		mTriangles = new List<int>();

		InitialPolyline iniPol = new InitialPolyline (5);
		iniPol.addPosition (new Vector3 (0.0f, 0.0f, 0.0f));
		iniPol.addPosition (new Vector3 (0.0f, 2.0f, 0.0f));
		iniPol.addPosition (new Vector3 (0.0f, 5.0f, 0.0f));
		iniPol.addPosition (new Vector3 (2.0f, 2.0f, 0.0f));
		iniPol.addPosition (new Vector3 (2.0f, 0.0f, 0.0f));
		iniPol.initializeIndices();

		//Add the first polyline vertices to the mesh
		Vector3[] poss = iniPol.getPositions ();
		foreach (Vector3 v in poss) {
			mVertices.Add (v);
		}

		extrude (iniPol,  new Vector3 (0.0f, 0.0f, 1.0f), 10);

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
		
	/*void OnDrawGizmos() { //For debug purposes
		Vector3[] vertices = mVertices.ToArray ();
		for (int i = 0; i < vertices.Length; ++i) {
			Gizmos.DrawWireSphere (center: vertices [i], radius: 0.2f);
		}
	}*/
}
