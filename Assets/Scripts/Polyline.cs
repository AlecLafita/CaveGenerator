using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//http://answers.unity3d.com/questions/501330/creating-custom-class.html

namespace Geometry {
	public class Polyline {

		private Vector3[] positions; //Contains the position for each vertex of the polyline
		private int[] indices; //Containts the index from the mesh for each vertex of the polyline
		private int numV; //Number of vertices this polyline contains

		//Constructors
		public Polyline() {
			positions = new Vector3[0];
			indices = new int[0];
			numV = 0;
		}

		public Polyline(int numV) {
			positions = new Vector3[numV];
			indices = new int[numV];
			this.numV = numV;
		}

		//Setters
		public void setPosition(int v, Vector3 position) {
			positions [v] = position;
		}

		public void setIndex(int v, int index) {
			indices [v] = index;
		}

		//Getters
		public Vector3 getPosition(int v) {
			return positions [v];
		}
		public Vector3[] getPositions() {
			return positions;
		}

		public int getIndex(int v) {
			return indices [v];
		}
		public int[] getIndices() {
			return indices;
		}

		public int getSize() {
			return numV;
		}

		//Other functions

		public void extrudeVertex(int v, Vector3 originPos, Vector3 direction, int distance) {
			positions [v] = originPos + direction * distance;
		}
	}
}