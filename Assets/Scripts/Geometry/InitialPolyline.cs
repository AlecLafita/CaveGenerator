using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry {

	/**Class that helps to create the initial polyline **/
	public class InitialPolyline : Polyline { 

		private int mActualPos = 0; //Actual vertex position

		//******** Constructors ********//
		public InitialPolyline() : base() {}
		public InitialPolyline(int numV) : base(numV){}

		//******** Setters ********//
		public void initializeIndices() {
			for (int i = 0; i < mVertices.Length; ++i) {
				mVertices [i].setIndex (i);
			}
		}

		public void addPosition(Vector3 newPos) {
			if (mActualPos >= mVertices.Length) { //TODO:exception
				Debug.Log ("Number of index bigger than size");
				return;
			}
			mVertices[mActualPos].setPosition(newPos);
			++mActualPos;
		}

		public void addVertex(Vertex newV) {
			if (mActualPos >= mVertices.Length) { //TODO:exception
				Debug.Log ("Number of index bigger than size");
				return;
			}
			mVertices [mActualPos] = new Vertex(newV);
			++mActualPos;
		}

		public void generateUVs () {
			//Get the accumulate distance
			float distance= 0.0f;
			for (int i = 0; i < mVertices.Length; ++i) {
				distance += Vector3.Distance (getVertex (i).getPosition (), getVertex (i + 1).getPosition ());
			}
			//Set the UV proportional to the distance, as if the polyline was being mapped to x axis proportionally
			//and between 0 and 1
			mVertices [0].setUV (new Vector2 (0.0f, 0.0f));
			for (int i = 1; i < mVertices.Length; ++i) {
				float distAux = Vector3.Distance (getVertex (i-1).getPosition (), getVertex (i).getPosition ());
				Vector2 UV = getVertex(i-1).getUV() + new Vector2 (distAux / distance, 0.0f);
				getVertex(i).setUV (UV);
			}
			mVertices [0].setUV (new Vector2 (0.0f, 1.0f));

			/*for (int i = 0; i < mVertices.Length-1;++i) {
				mVertices[i].setUV(new Vector2((float)i/(float)(mVertices.Length-1),0.0f));
			}*/
		}
		public void generateUVs (float yCoord) {
			//Get the accumulate distance
			float distance = 0.0f;
			for (int i = 0; i < mVertices.Length; ++i) {
				distance += Vector3.Distance (getVertex (i).getPosition (), getVertex (i + 1).getPosition ());
			}
			//Set the UV proportional to the distance, as if the polyline was being mapped to x axis proportionally
			//and between 0 and 1
			mVertices [0].setUV (new Vector2 (0.0f, yCoord));
			for (int i = 1; i < mVertices.Length; ++i) {
				float distAux = Vector3.Distance (getVertex (i-1).getPosition (), getVertex (i).getPosition ());
				Vector2 UV = getVertex(i-1).getUV() + new Vector2 (distAux / distance, 0.0f);
				getVertex(i).setUV (UV);
			}
		}

		/** Gets the plane generated on the normal direction of the polyline and to a TODO distance **/
		public Plane generateNormalPlane() {
			//First get maximum "radius" on normal direction to know the distance to project
			//As it's polyline formed from an extrusion, they will always form two symmetric semicircles. Its enough then
			//to check the first (or last) half of vertice
			/*float radius = 0.0f;

					//TODO GET RADIUS
			*/

			float distance = 5.0f;
			Vector3 planeNormal = calculateNormal ();
			Vector3 b = calculateBaricenter ();
			Plane result = new Plane (-planeNormal, b + planeNormal * distance);
			return result;
		}

		/**Gets the maximum distance between baricenter and some vertex, in 2D projection on normal direction**/
		public float computeProjectionRadius() {
			Plane p = generateNormalPlane ();
			Vector3 b = calculateBaricenter ();
			b = Geometry.Utils.getPlaneProjection (p, b);
			float radius = 0.0f;
			foreach (Vertex v in mVertices) {
				float distanceAux = Vector3.Distance (b, Geometry.Utils.getPlaneProjection(p,v.getPosition()));
				if (distanceAux > radius)
					radius = distanceAux;
			}
			return radius;
		}

		//******** Smooth ********//

		public void smoothMean() {
			InitialPolyline newPolyline = new InitialPolyline (mVertices.Length * 2);
			//Add new mid-points
			for (int i = 0; i < mVertices.Length; ++i) {
				newPolyline.addPosition (getVertex(i).getPosition ());
				newPolyline.addPosition(new Vector3());
				newPolyline.getVertex (i * 2+1).Lerp (getVertex(i), getVertex(i + 1), 0.5f);
			}

			//Take the mean of the vertices previously existing with the new vertices
			for (int i = 0; i < newPolyline.getSize(); i+= 2) {
				newPolyline.getVertex (i).Lerp (newPolyline.getVertex (i - 1), newPolyline.getVertex (i + 1), 0.5f);
			}
			this.mVertices = newPolyline.mVertices;

		}

		/** m = n + p + 1, m size of knot vector, n size of control points, p curve(polynomial) degree
		 * 
		 * Close curve -> repeat the degree + 1 first control points at the end
		 *  De Bor: O(p^2) + O(p + n)
		 **/
		private const int smoothDegree = 3;

		/** Smoothes the polyline by applying a B-spline, and multiplying the number of vertices as the parameter says **/
		public void smoothBspline() {
			int controlLength = mVertices.Length;
			int knotLength = controlLength + smoothDegree + 1;
			//Generate the knot vector

			//Vector3[] controlPoints = mVertices;
			//TODO

		}
	}
}