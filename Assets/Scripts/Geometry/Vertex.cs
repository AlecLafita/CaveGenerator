using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry {

	/** Class that represent a Vertex object **/
	public class Vertex {
		
		private Vector3 position; //Position of the vertex in world coords.
		private int index; //Index of the vertex in the mesh
		private bool inHole; //Does this vertex belongs to a hole?
	
		//Constructor
		public Vertex() {
			inHole = false;
			index = -1;
			position = new Vector3 ();
		}

		//Setters
		public void setPosition(Vector3 position) {
			this.position = position;
		}

		public void setIndex(int index) {
			this.index = index;
		}

		public void setInHole(bool hole) {
			inHole = hole;
		}

		//Getters
		public Vector3 getPosition() {
			return position;
		}

		public int getIndex() {
			return index;
		}

		public bool getInHole() {
			return inHole;
		}

	}
}