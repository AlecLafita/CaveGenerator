using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry {
	public class Utils  {
		
		/**Gets the projection position of some point to some plane, on the plane direction **/
		public static Vector3 getPlaneProjection(Plane p, Vector3 originalPoint) {
			Ray ray = new Ray (originalPoint, -p.normal);
			float pointPlane;
			p.Raycast (ray, out pointPlane);
			return ray.GetPoint (pointPlane);
		}
	}
}