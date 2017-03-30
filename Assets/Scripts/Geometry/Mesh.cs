using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry {
	
	/** This class contains the mesh that will be dynamically generated. It's important then,
	 * that the operations that modify it are done in constant time.
	 * The triangles need to have the normal with the direction pointing to the inside 
	 * of the cave, in order to see the triangulation from inside **/
	public class Mesh {

		private List<Vector3> mVertices; //The vertices position. There's no need to have them as a Vertex instances
		private List<int> mTriangles; //The triangles that form the mesh. It has a size multiple of three as 
									  //uses the vertices indices
		private List<Vector2> mUVs; //UVs coordinates. Position corresponding for each vertex

		//******** Constructors ********//
		public Mesh() {
			mVertices = new List<Vector3>();
			mTriangles = new List<int>();
			mUVs = new List<Vector2> ();
		}

		/** Creates the mesh intializating it with a polyline's vertices **/
		public Mesh (Polyline iniPol) {
			mVertices = new List<Vector3>();
			mTriangles = new List<int>();
			mUVs = new List<Vector2> ();
			for (int i = 0; i < iniPol.getSize (); ++i) {
				addVertex (iniPol.getVertex (i).getPosition ());
			}
		}

		//******** Getters ********//
		public List<Vector3> getVertices() {
			return mVertices;
		}
		public int getNumVertices() {
			return mVertices.Count;
		}

		public List<int> getTriangles() {
			return mTriangles;
		}
		public int getNumTriangles() {
			return mTriangles.Count/3;
		}

		public List<Vector2> getUVs() {
			return mUVs;
		}

		//******** Setters ********//
		public void setVertices(List<Vector3> vertices) {
			mVertices = vertices;
		}

		public void setTriangles(List<int> triangles) {
			mTriangles = triangles;
		}

		public void setUVs(List<Vector2> uvs) {
			mUVs = uvs;
		}

		//******** Other functions ********//
		/** Adds a new vertex to the mesh **/
		public void addVertex(Vector3 v) {
			mVertices.Add (v);
			//Generate random texture position TODO:Improve this
			Vector2 uv = new Vector2 (Random.Range (0.0f, 10.0f), Random.Range (0.0f, 10.0f));
			mUVs.Add (uv);
		}

		/** Adds a new triangle to the mesh**/
		private void addTriangle(int v1, int v2, int v3) {
			mTriangles.Add (v1); mTriangles.Add (v2); mTriangles.Add (v3);
		}

		/** Triangulates the quad iff is not part of a hole **/
		private void triangulateQuad(Vertex bl, Vertex br, Vertex tl, Vertex tr) {
			/**		The quad vertices seen from outside the cave:
			 * 		tl___tr
			 * 		|	 |
			 * 		bl___br
			 **/
			if (!(bl.getInHole() && tr.getInHole() && tl.getInHole() && br.getInHole())) { //Avoid triangulating holes
				//Left-hand!
				addTriangle(bl.getIndex(), tr.getIndex(), tl.getIndex());
				addTriangle(bl.getIndex(), br.getIndex(), tr.getIndex());
			}
		}

		/** Makes the triangulation between two polylines with same size. The order of
		 * the vertices has to be the same. This is used then for neighbour polylines **/
		public void triangulatePolylines(Polyline pol1, Polyline pol2) {
			if (pol1.getSize () != pol2.getSize ()) { //TODO : throw exception
				Debug.Log ("The two polylines do not have the same length!");
				return;
			}
			for (int i = 0; i < pol1.getSize(); ++i) {
				triangulateQuad(pol1.getVertex(i), pol1.getVertex(i+1), pol2.getVertex(i), pol2.getVertex(i+1));//Clockwise
				//triangulateQuad(pol1.getVertex(i+1), pol1.getVertex(i), pol2.getVertex(i+1),pol2.getVertex(i));//Counter-Clockwise
			}
		}

		/** Closes a polyline by triangulating all it's vertices with it's baricenter, visualizing it as a polygon**/
		public void closePolyline(Polyline poly) {
			Vector3 baricenter = poly.calculateBaricenter ();
			int baricenterIndex = getNumVertices();
			addVertex (baricenter);
			for (int i = 0; i < poly.getSize(); ++i) {
				//Left-hand!!
				addTriangle(poly.getVertex (i).getIndex(), poly.getVertex (i + 1).getIndex(), baricenterIndex);
			}
		}

	}
}
