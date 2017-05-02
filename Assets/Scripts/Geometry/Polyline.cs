using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Geometry {

	/**Represent a set of vertices forming a simple closed polyline (polygonal chain). 
	*  They need to be sorted! (either clockwise or counterclockwise) **/
	public class Polyline {

		protected Vertex[] mVertices; //Vertices that form the polyline
		protected float minRadius = 30.0f; //Minimum radius when the scale is applied
		protected float maxRadius = 100.0f;//Maximum radius when the scale is applied
		//******** Constructors ********//
		public Polyline() {
			mVertices = new Vertex[0];
		}

		public Polyline(int numV) {
			mVertices = new Vertex[numV];
			for (int i = 0; i < numV; ++i) { //Instantiate Vertex!
				mVertices [i] = new Vertex ();
			}
		}

		public Polyline(Polyline original) {
			mVertices = new Vertex[original.getSize ()];
			for (int i = 0; i < mVertices.Length; ++i) {
				mVertices [i] = new Vertex (original.getVertex (i));
			}
			minRadius = original.minRadius;
			maxRadius = original.maxRadius;
		}

		//******** Setters ********//
		public void setVertex(int i, Vertex v) {
			mVertices [i] = v; //This does not create a new instance! Sharing same vertex
		}

		public void setMinRadius(float newValue) {
			minRadius = newValue;
		}

		//******** Getters ********//
		public Vertex getVertex(int i) {
			if (i < 0)
				i = mVertices.Length + i;
			return mVertices [i % mVertices.Length];
		}

		public int getSize() {
			return mVertices.Length;
		}

		//******** Other functions ********//
		/** Generates the position of some vertex at the direction and distance from some other position **/
		public void extrudeVertex(int v, Vector3 originPos, Vector3 direction, float distance) {
			mVertices [v].setPosition (originPos + direction * distance);
		}

		/**Gets the maximum distance between baricenter and some vertex, in 3D**/
		public float computeRadius() {
			Vector3 b = calculateBaricenter ();
			float radius = 0.0f;
			foreach (Vertex v in mVertices) {
				float distanceAux = Vector3.Distance (b, v.getPosition());
				if (distanceAux > radius)
					radius = distanceAux;
			}
			return radius;
		}

		/** Scales all the polyline vertices taking the baricenter as origin **/
		public void scale(float scaleValue) {
			Vector3 b = calculateBaricenter ();
			//First check the scale result is not to small nor big
			foreach (Vertex v in mVertices) {
				Vector3 scaledPos = v.getPosition ();
				scaleFromCenter (b, ref scaledPos, scaleValue);
				float radius = Vector3.Distance (b, scaledPos);
				if ( (radius < minRadius && scaleValue < 1.0f) || ( radius > maxRadius && scaleValue > 1.0f)) 
					return;
			}
			//If condition accomplished, then scale
			foreach (Vertex v in mVertices) {
				Vector3 scaledPos = v.getPosition ();
				/*scaledPos -= b; //Translate it to the origin with baricenter as pivot
				scaledPos *= scaleValue; //Apply the scale
				scaledPos += b; //Return it to the real position*/
				scaleFromCenter (b, ref scaledPos, scaleValue);
				v.setPosition (scaledPos);
			}
		}
		private void scaleFromCenter(Vector3 center, ref Vector3 point2Scale, float scaleValue) {
			point2Scale -= center; //Translate the point to the origin with the center as pivot
			point2Scale *= scaleValue;//Apply the scale
			point2Scale += center; //Return it to the real position
		}

		/** Rotates all the polyline vertices through the polyline's normal vector(degrees) **/
		public void rotate(float angle) {
			//TODO:Rotate towards extrusion direction instead of polyline's normal?
			Vector3 normal = calculateNormal ();
			/*Quaternion rot = new Quaternion ();
			rot.SetFromToRotation (Vector3.forward, normal);*/

			Vector3 baricenter = calculateBaricenter ();

			foreach (Vertex v in mVertices) {
				//Create a new space with center at the actual vertex and rotate the normal vector
				GameObject obj = new GameObject ();
				//obj.transform.rotation = rot;
				obj.transform.position = v.getPosition ();
				obj.transform.RotateAround (baricenter, normal, angle);
				v.setPosition (obj.transform.position);
				UnityEngine.Object.Destroy (obj);
			}
		}

		/** Calculates the polyline center, which is the vertices mean position **/
		public Vector3 calculateBaricenter() {
			Vector3 baricenter = new Vector3 (0.0f,0.0f,0.0f);
			foreach (Vertex v in mVertices) {
				baricenter += v.getPosition ();
			}
			return baricenter/(float)mVertices.Length;
		}

		public Vector2 calculateBaricenterUV() {
			Vector2 baricenter = new Vector2 (0.0f,0.0f);
			foreach (Vertex v in mVertices) {
				baricenter += v.getUV ();
			}
			baricenter.y += 10.0f;
			return baricenter/(float)mVertices.Length;
		}

		/** Computes the normal of the plane formed by the polyline's vertices. Each component is calculated from the 
		 * area of the projection on the coordinate plane corresponding for the actual component.
		 * Pre: The polygon formed by the polyline must be simple **/
		public Vector3 calculateNormal() {
			Vector3 normal = new Vector3 (0.0f,0.0f,0.0f);
			for (int i = 0; i < mVertices.Length; ++i) {
				normal.x += ((getVertex (i).getPosition ().z + getVertex (i + 1).getPosition ().z)* 
							(getVertex (i).getPosition ().y - getVertex (i + 1).getPosition ().y));
				normal.y += ((getVertex (i).getPosition ().x + getVertex (i + 1).getPosition ().x)* 
							(getVertex (i).getPosition ().z - getVertex (i + 1).getPosition ().z));
				normal.z += ((getVertex (i).getPosition ().y + getVertex (i + 1).getPosition ().y)* 
							(getVertex (i).getPosition ().x - getVertex (i + 1).getPosition ().x));
			}
			//normal *= 0.5f; //counter-clockwise  (left-hand!)
			normal *= -0.5f; //clockwise (left-hand!)
			normal.Normalize ();
			return normal;
		}

		/** Checks the polyline is simple (no intersections are produced) **/
		/*public bool isSimple() {
			//TODO
		}*/

		/** Checks the polyline is convex **/
		/*public bool isConvex() {
			//TODO
		}*/

	}
}