using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Geometry {

	/**Represent a set of vertices forming a simple closed polyline (polygonal chain). 
	*  They need to be sorted! (either clockwise or counterclockwise) **/
	public class Polyline {

		protected Vertex[] mVertices; //Vertices that form the polyline
		protected int mNumV; //Number of vertices this polyline should have

		//******** Constructors ********//
		public Polyline() {
			mVertices = new Vertex[0];
			mNumV = 0;
		}

		public Polyline(int numV) {
			mVertices = new Vertex[numV];
			for (int i = 0; i < numV; ++i) { //Instantiate Vertex!
				mVertices [i] = new Vertex ();
			}
			mNumV = numV;
		}

		//******** Setters ********//
		public void setVertex(int i, Vertex v) {
			mVertices [i] = v; //This does not create a new instance! Sharing same vertex
		}

		//******** Getters ********//
		public Vertex getVertex(int i) {
			return mVertices [i % mNumV];
		}

		public int getSize() {
			return mNumV;
		}

		//******** Other functions ********//
		/** Generates the position of some vertex at the direction and distance from some position **/
		public void extrudeVertex(int v, Vector3 originPos, Vector3 direction, float distance) {
			mVertices [v].setPosition (originPos + direction * distance);
		}

		/** Calculates the polyline center, which is the vertices mean position **/
		public Vector3 calculateBaricenter() {
			Vector3 baricenter = new Vector3 (0.0f,0.0f,0.0f);
			foreach (Vertex v in mVertices) {
				baricenter += v.getPosition ();
			}
			return baricenter/mNumV;
		}

		/** Scales all the polyline vertices taking the baricentre as origin **/
		public void scale(float scaleValue) {
			Vector3 b = calculateBaricenter ();
			foreach (Vertex v in mVertices) {
				Vector3 scaledPos = v.getPosition ();
				scaledPos -= b; //Translate it to the origin with baricenter as pivot
				scaledPos *= scaleValue; //Apply the scale
				scaledPos += b; //Return it to the real position
				v.setPosition (scaledPos);
			}
		}

		/** Generates the normal of the plane formed by the polyline's vertices **/
		/*public Vector3 calculateNormal() {
			//TODO
		}

		/** Checks the polyline is simple (no intersections are produced) **/
		/*public bool isSimple() {
			//TODO
		}*/
		

	}
}