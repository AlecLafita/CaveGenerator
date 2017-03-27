using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

/**Responsible for avoid the intersections through the cave. It will work the following way:
 * accumulate polylines until a hole is done or the direction is changed, then add this set
 * of polylines to a axis aligned bounding box. Each time a extrusion is done, must check if 
 * it intersects with ALL the other BBs **/

public class IntersectionsController : MonoBehaviour {

	private static List<Bounds> boundingBoxes;
	private static List<Polyline> actualPolylines;

	//******** Singleton stuff ********//
	private static IntersectionsController mInstace; 
	public void Awake() {
		mInstace = this;
		boundingBoxes = new List<Bounds> ();
		actualPolylines = new List<Polyline> ();
	}

	public static IntersectionsController Instance {
		get {
			return mInstace;
		}
	}
		
	//******** Getters ********//
	public List<Bounds> getBBs() {
		return boundingBoxes;
	}

	//******** Other functions ********//
	/**Adds a new polyline to the actual set **/
	public void addPolyline(Polyline p ) {
		actualPolylines.Add (p);
	}

	/**Empties the actual set of polylines**/
	private void resetActual() {
		actualPolylines.Clear ();
	}

	/**Uses the actual set of polylines to create a new bounding box **/
	public void addActualBox() {
		if (actualPolylines.Count > 1) {
			Bounds newBB = BBfromPolylines (actualPolylines);
			//Add the new BB and reset the set of polylines
			boundingBoxes.Add (newBB);
		}
		resetActual ();
	}

	/**Check if the received extrusion do intersect with the previous ones**/
	public bool doIntersect(Polyline orig, Polyline dest) {
		List<Polyline> extr = new List<Polyline> ();
		extr.Add (orig); extr.Add (dest);
		Bounds extrusionBox = BBfromPolylines (extr);
		for (int i = 0; i < boundingBoxes.Count; ++i) {
			if (extrusionBox.Intersects (boundingBoxes [i]))
				return true;
		}
		return false;
	}

	/**Creates the bounding box from a set of polylines **/
	private Bounds BBfromPolylines(List<Polyline> ps) {
		//Obtain the BB from the set of the polylines' points
		Vector3 min = new Vector3 (float.MaxValue,float.MaxValue,float.MaxValue);
		Vector3 max = new Vector3 (float.MinValue, float.MinValue, float.MinValue);
		Vector3 actualPoint;
		for (int i = 0; i < ps.Count; ++i) {
			for (int j = 0; j < ps [i].getSize(); ++j) {
				actualPoint = ps [i].getVertex (j).getPosition ();
				if (actualPoint.x > max.x) max.x = actualPoint.x;
				if (actualPoint.y > max.y) max.y = actualPoint.y;
				if (actualPoint.z > max.z) max.z = actualPoint.z;
				if (actualPoint.x < min.x) min.x = actualPoint.x;
				if (actualPoint.y < min.y) min.y = actualPoint.y;
				if (actualPoint.z < min.z) min.z = actualPoint.z;
			}
		}
		Bounds newBB = new Bounds();
		newBB.SetMinMax (min, max);
		return newBB;
	}
}