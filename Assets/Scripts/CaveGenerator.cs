using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

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
	void triangulatePolylines(Polyline pol1, Polyline pol2) {
		if (pol1.getSize() != pol2.getSize())
			Debug.Log ("The two polylines do not have the same length!");

		for (int i = 0; i < pol1.getSize()-1; ++i) {
			triangulateQuad (pol1.getIndex(i), pol1.getIndex(i + 1), pol2.getIndex(i), pol2.getIndex(i + 1));
		}
		triangulateQuad (pol1.getIndex(pol1.getSize() - 1), pol1.getIndex(0), 
			pol2.getIndex(pol2.getSize() - 1), pol2.getIndex(0));	
	}


	/** From the vertices of an existing polyline, it creates a new new one
	 * with the same number of vertices and following some direction and at some distance**/
	Polyline extrude(Polyline originPoly,  Vector3 direction, int distance) {
		//Create the new polyline
		Polyline newPoly = new Polyline(originPoly.getSize());
		for (int i = 0; i < originPoly.getSize(); ++i) {//Generate the new vertices
			//Add vertex to polyline
			newPoly.extrudeVertex(i, originPoly.getPosition(i),direction,distance);
			//Add index vertex to polyline
			newPoly.setIndex(i,mVertices.Count);
			//Add the new vertex to the set of vertices
			mVertices.Add(newPoly.getPosition(i));
		}

		//Triangulate from origin to new polyline as a tube/cave shape
		triangulatePolylines (originPoly, newPoly);

		return newPoly;
	}

	void Start () {
		Mesh mesh = new Mesh ();
		mVertices = new List<Vector3>();
		mTriangles = new List<int>();

		Polyline iniPol = new Polyline (4); //Initial polyline
		iniPol.setPosition (0,new Vector3 (0.0f, 0.0f, 0.0f));
		iniPol.setPosition (1,new Vector3 (0.0f, 1.0f, 0.0f));
		iniPol.setPosition (2,new Vector3 (1.0f, 1.0f, 0.0f));
		iniPol.setPosition (3,new Vector3 (1.0f, 0.0f, 0.0f));
		iniPol.setIndex (0, 0); iniPol.setIndex (1, 1); iniPol.setIndex (2, 2); iniPol.setIndex (3, 3);

		Vector3[] poss = iniPol.getPositions ();
		foreach (Vector3 v in poss) {
			mVertices.Add (v);
		}

		Vector3 dir = new Vector3 (0.0f, 0.0f, 1.0f);
		Polyline newPoly = extrude (iniPol, dir, 10);

		for (int i = 0; i < 1000; ++i) { //Example
			newPoly = extrude(newPoly,dir,10);
		}

		mesh.vertices = mVertices.ToArray();
		mesh.triangles = mTriangles.ToArray ();

		//mesh.RecalculateNormals ();
		//mesh.RecalculateBounds();

		//Assign the created mesh to the one we are storing and visualizing
		GetComponent<MeshFilter> ().mesh = mesh;

	}

}
