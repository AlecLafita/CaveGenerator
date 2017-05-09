using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Geometry {

	/**Represent a set of vertices forming a simple closed polyline (polygonal chain). 
	*  They need to be sorted! (either clockwise or counterclockwise) **/
	public class Polyline {

		protected Vertex[] mVertices; //Vertices that form the polyline
		protected float minRadius = 2.5f; //Minimum radius when the scale is applied
		protected float maxRadius = 40.0f;//Maximum radius when the scale is applied
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
			for (int i = 0; i < getSize(); ++i) {
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
				i = getSize() + i;
			return mVertices [i % getSize()];
		}

		public int getSize() {
			return mVertices.Length;
		}

		public float getMinRadius() {
			return minRadius;
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
			for(int i = 0; i < getSize()-1;++i) {
				Vertex v = getVertex(i);
				float distanceAux = Vector3.Distance (b, v.getPosition());
				if (distanceAux > radius)
					radius = distanceAux;
			}
			return radius;
		}

		/**Gets the minimum distance between baricenter and some vertex, in 3D**/
		public float computeMinimumRadius() {
			Vector3 b = calculateBaricenter ();
			float radius = float.MaxValue;
			for(int i = 0; i < getSize()-1;++i) {
				Vertex v = getVertex(i);
				float distanceAux = Vector3.Distance (b, v.getPosition());
				if (distanceAux < radius)
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
			for(int i = 0; i < getSize()-1;++i) {
				Vertex v = getVertex(i);
				baricenter += v.getPosition ();
			}
			return baricenter/(float)(getSize()-1);
		}

		public Vector2 calculateBaricenterUV() {
			Vector2 baricenter = new Vector2 (0.5f,getVertex(0).getUV().y);
			/* Vector2 baricenter = Vector2.zero;
			 * for(int i = 0; i < getSize()-1;++i) {
				Vertex v = getVertex(i);
				baricenter += v.getUV ();
			}
			baricenter = baricenter/(float)(getSize()-1);*/

			/*Vector2 UVincr = new Vector2(0.0f,1.0f);
			UVincr.Normalize ();
			UVincr *= (computeRadius()/AbstractGenerator.UVfactor);
			baricenter += UVincr;*/
			baricenter.y += (computeRadius () / AbstractGenerator.UVfactor);
			return baricenter;
		}

		/** Computes the normal of the plane formed by the polyline's vertices. Each component is calculated from the 
		 * area of the projection on the coordinate plane corresponding for the actual component.
		 * Pre: The polygon formed by the polyline must be simple **/
		public Vector3 calculateNormal() {
			Vector3 normal = new Vector3 (0.0f,0.0f,0.0f);
			for (int i = 0; i < getSize(); ++i) {
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

		/** Checks the polyline is simple (no intersections between vertices are produced). Pre: All polyline points must on same plane  **/
		public bool isSimple() {
			//TODO http://geomalgorithms.com/a09-_intersect-3.html
			// Bentley-Ottmann algorithm -> says where are intersection points ->no need for this O((N + L)log N), N intersection point, l lines
			// Shamos-Hoey -> just says if intersection is produced or not -> this is what we want! O(n) space, O(nlogn) time, n points

			//O(n^2)
			Plane p = new Plane(getVertex(0).getPosition(),getVertex(1).getPosition(),getVertex(2).getPosition());
			for (int i = 0; i < getSize();++i) {
				for (int j = i+2; j < getSize (); ++j) {
					if (segmentsIntersect (p, getVertex (i).getPosition (), getVertex (i + 1).getPosition (), 
						    getVertex (j).getPosition (), getVertex (j + 1).getPosition ())) {
						return false;
					}
				}
			}
			return true;
		}

		private bool segmentsIntersect(Plane p, Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2) {
			//Just simple intersection (cross)
			Vector3 p1First = a1 + Vector3.Cross(a2 - a1, b1 - a1);
			Vector3 p2First = a1 + Vector3.Cross (a2 - a1, b2 - a1);
			bool firstC = !p.SameSide (p1First, p2First);
			Vector3 p1Second = b1 + Vector3.Cross (b2 - b1, a1 - b1);
			Vector3 p2Second = b1 + Vector3.Cross (b2 - b1, a2 - b1);
			bool secondC = !p.SameSide (p1Second, p2Second);
			return firstC && secondC;
		}

		/** Checks the polyline is convex. Pre: All polyline points must on same plane **/
		public bool isConvex() { //Is convex => is simple
			//http://stackoverflow.com/questions/471962/how-do-determine-if-a-polygon-is-complex-convex-nonconvex
			//O(n), check all three consecutive vertices are clockwise. If some are counter-clock, is not convex
			Plane p = new Plane(getVertex(0).getPosition(),getVertex(1).getPosition(),getVertex(2).getPosition());
			for (int i = 0; i < getSize (); ++i) {
				Vector3 first = getVertex (i + 1).getPosition() - getVertex (i).getPosition();
				Vector3 second = getVertex (i + 2).getPosition() - getVertex (i + 1).getPosition ();
				if (Vector3.Angle (first, second) <= 3.0f) //Very small angle for diferent segments that almost form a rect and are problematic
					continue;
				Vector3 cross = Vector3.Cross (first.normalized, second.normalized);
				cross.Normalize();
				Vector3 planePoint = getVertex (i).getPosition () + cross;
				if (cross != Vector3.zero && !p.GetSide (planePoint)) {
					//Debug.Log (i);
					//Debug.Log (getVertex (i).getPosition () + "   " + planePoint);
					return false;
				}
			}
			return true;
		}

	}
}