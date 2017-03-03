using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Geometry {
	public class Polyline {

		protected Vector3[] mPositions; //Contains the position for each vertex of the polyline
		protected int[] mIndices; //Containts the index from the mesh for each vertex of the polyline
		protected int mNumV; //Number of vertices this polyline contains

		//Constructors
		public Polyline() {
			mPositions = new Vector3[0];
			mIndices = new int[0];
			mNumV = 0;
		}

		public Polyline(int mNumV) {
			mPositions = new Vector3[mNumV];
			mIndices = new int[mNumV];
			this.mNumV = mNumV;
		}

		//Setters
		public void setPosition(int v, Vector3 position) {
			mPositions [v] = position;
		}

		public void setIndex(int v, int index) {
			mIndices [v] = index;
		}

		//Getters
		public Vector3 getPosition(int v) {
			return mPositions [v];
		}
		public Vector3[] getPositions() {
			return mPositions;
		}

		public int getIndex(int v) {
			return mIndices [v];
		}
		public int[] getIndices() {
			return mIndices;
		}

		public int getSize() {
			return mNumV;
		}

		//Other functions
		//Generates the position of some vertex from other position, following a direction and distance
		public void extrudeVertex(int v, Vector3 originPos, Vector3 direction, int distance) {
			mPositions [v] = originPos + direction * distance;
		}
	}
}