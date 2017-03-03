using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Geometry {
	public class Polyline {

		protected Vertex[] mVertices; //Vertices that form the polyline
		protected int mNumV; //Number of vertices this polyline should have

		//Constructors
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

		//Setters
		public void setPosition(int v, Vector3 position) {
			mVertices [v].setPosition (position);
		}

		public void setIndex(int v, int index) {
			mVertices[v].setIndex(index);
		}

		//Getters
		public Vector3 getPosition(int v) {
			return mVertices[v % mNumV].getPosition();
		}
		public Vector3[] getPositions() {
			Vector3[] result = new Vector3[mNumV];
			for (int i = 0; i < mNumV; ++i) {
				result [i] = mVertices [i].getPosition ();
			}
			return result;
		}

		public int getIndex(int v) {
			return mVertices[v % mNumV].getIndex();
		}
		public int[] getIndices() {
			//return mIndices;
			int[] result = new int[mNumV];
			for (int i = 0; i < mNumV; ++i) {
				result [i] = mVertices [i].getIndex ();
			}
			return result;
		}

		public int getSize() {
			return mNumV;
		}

		//Other functions
		//Generates the position of some vertex from other position, following a direction and distance
		public void extrudeVertex(int v, Vector3 originPos, Vector3 direction, int distance) {
			mVertices [v].setPosition (originPos + direction * distance);
		}
	}
}