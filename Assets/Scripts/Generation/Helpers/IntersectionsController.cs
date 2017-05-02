using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

/**Responsible for avoid the intersections through the cave. It will work the following way:
 * accumulate polylines until a hole is done or the direction is changed, then add this set
 * of polylines to a axis aligned bounding box. Each time a extrusion is done, must check if 
 * it intersects with ALL the other BBs **/

public class IntersectionsController {

	private static List<Bounds> boundingBoxes;
	private static List<Polyline> actualPolylines;
	private static Polyline lastPoly;//Last BB created
	private static float epsilon = 0.1f;
	//******** Singleton stuff ********//
	private static IntersectionsController mInstace; 

	public static IntersectionsController Instance {
		get {
			if (mInstace == null)
				mInstace = new IntersectionsController ();
			return mInstace;
		}
	}

	//******** Creator ********//
	public IntersectionsController() {
		boundingBoxes = new List<Bounds> ();
		actualPolylines = new List<Polyline> ();
		lastPoly = null;
	}

	//******** Getters ********//
	public List<Bounds> getBBs() {
		return boundingBoxes;
	}

	public int getLastBB() {
		return boundingBoxes.Count - 1;
	}

	//******** Other functions ********//
	/**Adds a new polyline to the actual set **/
	public void addPolyline(Polyline p ) {
		if (lastPoly != p) { //Avoid adding repeated polylines
			actualPolylines.Add (p);
			lastPoly = p;
		}
	}

	/**Empties the actual set of polylines**/
	private void resetActual() {
		actualPolylines.Clear ();
		lastPoly = null;
	}

	/**Uses the actual set of polylines to create a new bounding box **/
	public void addActualBox() {
		if (actualPolylines.Count > 1) {
			Bounds newBB = BBfromPolylines (actualPolylines);
			//Add the new BB
			boundingBoxes.Add (newBB);
		}
		// Reset the set of polylines
		resetActual ();
	}

	/**Check if the received extrusion do intersect with the previous ones**/
	public bool doIntersect(Polyline orig, Polyline dest, int canIntersect) {
		//canIntersect = -1;
		List<Polyline> extr = new List<Polyline> ();
		extr.Add (orig); extr.Add (dest);
		Bounds extrusionBox = BBfromPolylines (extr);
		int index = 0; 
		foreach (Bounds BB in boundingBoxes) {
			if (canIntersect != index && extrusionBox.Intersects (BB))
				return true;
			++index;
		}
		return false;
	}

	/**Creates the bounding box from a set of polylines **/
	private Bounds BBfromPolylines(List<Polyline> ps) {
		//Obtain the BB bounds from the set of the polylines' points
		Vector3 min = new Vector3 (float.MaxValue,float.MaxValue,float.MaxValue);
		Vector3 max = new Vector3 (float.MinValue, float.MinValue, float.MinValue);
		Vector3 actualPoint;
		foreach (Polyline p in ps) {
			for (int j = 0; j < p.getSize(); ++j) {
				actualPoint = p.getVertex (j).getPosition ();
				if (actualPoint.x > max.x) max.x = actualPoint.x;
				if (actualPoint.y > max.y) max.y = actualPoint.y;
				if (actualPoint.z > max.z) max.z = actualPoint.z;
				if (actualPoint.x < min.x) min.x = actualPoint.x;
				if (actualPoint.y < min.y) min.y = actualPoint.y;
				if (actualPoint.z < min.z) min.z = actualPoint.z;
			}
		}
		//Accurate a little the box size in order to not block the extrusion
		min += new Vector3 (epsilon, epsilon, epsilon);
		max -= new Vector3 (epsilon, epsilon, epsilon);
		//Finally create the bounding box from the min and max point
		Bounds newBB = new Bounds();
		newBB.SetMinMax (min, max);
		return newBB;
	}
}