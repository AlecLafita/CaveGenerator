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

		/** From existing polyline, generates a new one by projecting the original to a plane on it's normal direction.
	 	* It is also smoothed and scaled.  **/
		public static InitialPolyline generateProjection(InitialPolyline polyHole, int projectionSize, int smoothIterations) {
			//projectionSize must be pair!
			//Get the plane to project to
			Plane tunnelEntrance = polyHole.generateNormalPlane ();
			//Generate the polyline by projecting to the plane
			InitialPolyline planePoly = new InitialPolyline (projectionSize);
			int holePos = 0;
			int incr = ((polyHole.getSize ()/2)-1)/ ((projectionSize / 2)-1);
			for (int i = 0; i < projectionSize / 2; ++i) {
				planePoly.addPosition (Geometry.Utils.getPlaneProjection (tunnelEntrance, polyHole.getVertex (holePos).getPosition ()));
				holePos += incr;
			}
			holePos = polyHole.getSize () / 2;
			for (int i = 0; i < projectionSize / 2; ++i) {
				planePoly.addPosition (Geometry.Utils.getPlaneProjection (tunnelEntrance, polyHole.getVertex (holePos).getPosition ()));
				holePos += incr;
			}

			//Smooth it
			for (int j = 0; j < smoothIterations;++j)
				planePoly.smoothMean ();

			//Scale to an approximate size of the real size of the original
			float maxActualRadius = planePoly.computeRadius();
			float destinyRadius = polyHole.computeProjectionRadius ();
			planePoly.scale (destinyRadius / maxActualRadius);

			return planePoly;
		}
			
		/** Checks if a generated hole has it's extrusion direction too high (from the parameters limits) **/
		public static bool checkInvalidWalk(Polyline tunelStartPoly) {
			bool invalidHole = false;
			Vector3 normal = tunelStartPoly.calculateNormal ();
			if (normal.y < 0) {
				invalidHole = normal.y < -DecisionGenerator.Instance.directionYWalkLimit;
			} else {
				invalidHole = normal.y > DecisionGenerator.Instance.directionYWalkLimit;
			}
			return invalidHole;
		}

		/**Check if projecting/extruding the passed polyline, an artifact will be generated**/
		public static bool checkArtifacts (Polyline polyHole) {
			//TODO: IMprove this, maybe check directly with y coord is not good enough
			Plane projection = ((InitialPolyline)polyHole).generateNormalPlane ();
			//As its a hole polyline, first and second half are symmetric, there is need to just check one
			//If one half projected does not has all vertices on descendant or ascendant order, it will sure generate an aritfact
			if (Geometry.Utils.getPlaneProjection(projection, polyHole.getVertex (0).getPosition ()).y < 
				Geometry.Utils.getPlaneProjection(projection, polyHole.getVertex (1).getPosition ()).y) { //ascendant
				for (int i = 1; i < polyHole.getSize () / 2; ++i) {
					if (Geometry.Utils.getPlaneProjection (projection, polyHole.getVertex (i).getPosition ()).y >
						Geometry.Utils.getPlaneProjection (projection, polyHole.getVertex (i + 1).getPosition ()).y)
						return true;
				}
			} else { //descendent
				for (int i = 1; i < polyHole.getSize () / 2; ++i) {
					if (Geometry.Utils.getPlaneProjection (projection, polyHole.getVertex (i).getPosition ()).y <
						Geometry.Utils.getPlaneProjection (projection, polyHole.getVertex (i + 1).getPosition ()).y)
						return true;
				}
			}
			return false;
		}
	}
}