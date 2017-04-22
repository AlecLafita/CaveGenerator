﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

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
				addVertex (iniPol.getVertex (i));
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
		/** Adds a polyline to the mesh **/
		public void addPolyline (Polyline p) {
			for (int i = 0; i < p.getSize (); ++i) {
				addVertex(p.getVertex(i));
			}
		}

		/** Adds a new vertex to the mesh **/
		private void addVertex(Vertex v) {
			mVertices.Add (v.getPosition());
			//Generate random texture position TODO:Improve this
			//Vector2 uv = new Vector2 (Random.Range (0.0f, 1.0f), Random.Range (0.0f, 1.0f));
			mUVs.Add (v.getUV());
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

		/** Triangulate between two polylines as if they where convex hulls, very similar to D&C merge 
		 * (Gift wrapping idea) from 3D Convex hull theory. Used for a tunnel start **/
		public void triangulateTunnelStart(Polyline originPoly, Polyline destinyPoly) {
			//TODO
			//Start from a line between the first one of each polyline, by construction one is generated from the projection of the other
			Vertex a = originPoly.getVertex(0);
			Vertex b = destinyPoly.getVertex (0);
			int aIndex = 1;
			int bIndex = 1;
			while (aIndex <= originPoly.getSize () && bIndex <= destinyPoly.getSize ()) {
				//This line ab will be triangulated with a point c either from one polyliline or the other, depending the one that has 
				//smallest angle with ab line(A-winner or B-winner).c It's always an adjacent vertex, we can use they are clockwise sorted(lucky!)
				Vector3 ab = b.getPosition() - a.getPosition();
				//Check the B polylilne candidate
				Vertex bWinner = destinyPoly.getVertex(bIndex);
				float bAngle = Vector3.Angle (-ab, bWinner.getPosition()-b.getPosition());
				//Check the A polylilne candidate
				Vertex aWinner = originPoly.getVertex(aIndex);
				float aAngle = Vector3.Angle (ab, aWinner.getPosition()-a.getPosition());
				//If it's A-winner(from first poly) a=c, if it's from b-Winner b = c
				if (aAngle < bAngle) { //A wins!
					addTriangle(a.getIndex(), aWinner.getIndex(), b.getIndex());
					a = aWinner;
					++aIndex;
				}
				else { //B wins!
					addTriangle(a.getIndex(), bWinner.getIndex(), b.getIndex());
					b = bWinner;
					++bIndex;
				}
			}//Repeat until ab we arrive to some of the polylines start, then triangulate with a or b constant(depends on polyline)

			if (aIndex > originPoly.getSize ()) {
				while (bIndex <= destinyPoly.getSize ()) {
					Vertex bWinner = destinyPoly.getVertex(bIndex);
					addTriangle(a.getIndex(), bWinner.getIndex(), b.getIndex());
					b = bWinner;
					++bIndex;
				}
			} else if (bIndex > destinyPoly.getSize ()) {
				while (aIndex <= originPoly.getSize()){
					Vertex aWinner = originPoly.getVertex(aIndex);
					addTriangle(a.getIndex(), aWinner.getIndex(), b.getIndex());
					a = aWinner;
					++aIndex;
				}
			} 
		}

		/** Closes a polyline by triangulating all it's vertices with it's baricenter, visualizing it as a polygon**/
		public void closePolyline(Polyline poly) {
			Vertex baricenter = new Vertex ();
			baricenter.setPosition (poly.calculateBaricenter ());
			baricenter.setUV (poly.calculateBaricenterUV());
			int baricenterIndex = getNumVertices();
			addVertex (baricenter);
			for (int i = 0; i < poly.getSize(); ++i) {
				//Left-hand!!
				addTriangle(poly.getVertex (i).getIndex(), poly.getVertex (i + 1).getIndex(), baricenterIndex);
			}
		}

		/** Smooths the mesh by using the selected techique or filter. It must be called
		 * after all the mesh is completely generated **/
		public void smooth(int it) {

			//Transform to array in order to have a direct index (const. time)
			Vector3[] verticesArray = mVertices.ToArray (); //O(V)

			//Get the adjacent vertices set list
			HashSet<int>[] adjacentList = computeAdjacents();

			//Apply the corresponding smooth techniques as many times as required
			for (int i = 0; i < it; ++i) {
				//smoothLaplacian (verticesArray, adjacentList);
				smoothLaplacianIncrement (verticesArray, adjacentList);
			}

			//Set the new vertices
			mVertices = verticesArray.ToList();//O(V)
		}

		/**Get the list of the adjacent vertices(by index) of each vertex. O(V+T)**/
		private HashSet<int>[] computeAdjacents() {
			//Create the adjacent list
			System.Collections.Generic.HashSet<int>[] finalList = new HashSet<int>[mVertices.Count];
			for (int i = 0; i < finalList.Length; ++i) { //O(V)
				finalList [i] = new HashSet<int> ();
			}
			//Transform to array in order to have a direct index (const. time)
			int[] trianglesArray = mTriangles.ToArray(); //O(T)
			//Generate the adjacent list
			for (int i = 0; i < trianglesArray.Length; i += 3) { //O(T)
				addAdjacents (finalList, trianglesArray[i],trianglesArray[i+1],trianglesArray[i+2]);
				addAdjacents (finalList, trianglesArray[i+1],trianglesArray[i+2],trianglesArray[i]);
				addAdjacents (finalList, trianglesArray[i+2],trianglesArray[i],trianglesArray[i+1]);
			}
			return finalList;
		}

		private void addAdjacents (HashSet<int>[] list, int position, int index1, int index2) {
			list [position].Add (index1);
			list [position].Add (index2);
		}

		/**Sets the new position of each vertex by taking the mean of its adjacent vertices. O(V*Adj) **/
		private void smoothLaplacian(Vector3[] v, HashSet<int>[] adjacentV) {
			for (int i = 0; i < v.Length; ++i) {
				//Calculate the adjacents mean and set as the new vertex position
				Vector3 newV = Vector3.zero;
				foreach(int adj in adjacentV[i] ) {
					newV += v [adj];
				}
				v [i] = newV / adjacentV [i].Count;
			}
		}

		/**Sets the new position of each vertex by taking the difference with the weighted 
		 * increment of the vertex and its adjacent vertices. O(V*Adj) **/
		private float lambdaLaplacian = 0.1f;
		private void smoothLaplacianIncrement(Vector3[] v, HashSet<int>[] adjacentV) {
			for (int i = 0; i < v.Length; ++i) {
				//Calculate the adjacents mean 
				Vector3 newV = Vector3.zero;
				foreach(int adj in adjacentV[i] ) {
					newV += v [adj];
				}
				newV /= adjacentV [i].Count;
				v [i] = v[i] + lambdaLaplacian * (newV - v[i]);
			}
		}

		//TODO: cotangent weights
	}
}
