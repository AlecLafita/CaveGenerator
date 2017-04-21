using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry {

	/** Class that represent a Vertex object **/
	public class Vertex {
		
		private Vector3 position; //Position of the vertex in world coords.
		private int index; //Index of the vertex in the mesh
		private bool inHole; //Does this vertex belongs to a hole?
		private Vector2 UV; //Texture coordinates of this vertex

		//******** Constructors ********//
		public Vertex() {
			inHole = false;
			index = -1;
			position = new Vector3 ();
		}

		public Vertex (Vertex original) {
			position = original.getPosition();
			index = original.getIndex ();
			inHole = original.getInHole ();
			UV = original.getUV ();
		}

		//******** Setters ********//
		public void setPosition(Vector3 position) {
			this.position = new Vector3(position.x, position.y, position.z);
		}

		public void setIndex(int index) {
			this.index = index;
		}

		public void setInHole(bool hole) {
			inHole = hole;
		}

		public void setUV(Vector2 UV) {
			this.UV = UV;
		}

		//******** Getters ********//
		public Vector3 getPosition() {
			return position;
		}

		public int getIndex() {
			return index;
		}

		public bool getInHole() {
			return inHole;
		}

		public Vector2 getUV() {
			return UV;
		}

		/**Sets the vertex position and UV as the interpolation of two other vertices **/
		public void Lerp(Vertex a, Vertex b, float t) {
			position = Vector3.Lerp (a.position, b.position, t);
			UV = Vector2.Lerp (a.UV, b.UV, t);
		}
	}
}