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
				Vector2 UV = mVertices [i - 1].getUV() + new Vector2 (distAux / distance, 0.0f);
				mVertices [i].setUV (UV);
				//mVertices[i].setUV(new Vector2((float)i/(float)(mVertices.Length-1),0.0f));
			}
		}

		//******** Smooth ********//


		public void smoothMean() {
			InitialPolyline newPolyline = new InitialPolyline (mVertices.Length * 2);
			//Add new mid-points
			for (int i = 0; i < mVertices.Length; ++i) {
				//newPolyline.setVertex (i, mVertices [i / 2]);
				newPolyline.addPosition (mVertices [i].getPosition ());
				//Vertex newVertex = new Vertex ();
				Vector3 newPosition = getVertex(i).getPosition () + getVertex(i+ 1).getPosition ();
				newPosition /= 2;
				//newVertex.setPosition (newPosition);
				//newPolyline.setVertex(i+1,newVertex);
				newPolyline.addPosition (newPosition);
			}

			//Take the mean of the vertices previously existing with the new vertices
			for (int i = 0; i < newPolyline.getSize(); i+= 2) {
				//Vertex newVertex = new Vertex ();
				Vector3 newPosition = newPolyline.getVertex(i-1).getPosition () + newPolyline.getVertex(i + 1).getPosition ();
				newPosition /= 2;
				//newVertex.setPosition (newPosition);
				//newPolyline.setVertex (i, newVertex);
				newPolyline.getVertex(i).setPosition(newPosition);
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


		}
	}
}