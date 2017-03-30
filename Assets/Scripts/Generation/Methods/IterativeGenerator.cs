using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

/** Generates the cave by iterative creating a full tunnel. Then, the holes extrusion order is decided
 *  from the data structured used as a subclass **/
abstract public class IterativeGenerator : AbstractGenerator {

	/** Creates the data structure used for save holes. Must be one for polylines 
	 * and another for the corresponding BB which can intersect with **/
	abstract protected void createDataStructure (Polyline iniP);
	/**Checks if the data structure has more holes to extrude **/
	abstract protected bool isDataStructureEmpty ();

	/**Function to call at the start of each tunnel to obtain the corresponding hole gate **/
	abstract protected void initializeDataStructure (ref int canIntersect, ref Polyline p);
	/** Add hole gate to the data structure **/
	abstract protected void addElementToDataStructure (Polyline p, int canIntersect);

	public override void generate() {
		createDataStructure (gatePolyline);
		generate (gatePolyline, initialTunelHoleProb);
	}

	protected void generate(Polyline originPoly, float holeProb) {
		Polyline newPoly;
		float actualDistance;
		Vector3 actualDirection;
		int actualExtrusionTimes, extrusionsSinceOperation, noIntersection;
		noIntersection = -1;
		while (isDataStructureEmpty()) {
			//new tunnel(hole) will be done, update the counter and all the data
			--maxHoles;
			actualDistance = DecisionGenerator.Instance.generateDistance(true);
			actualDirection = originPoly.calculateNormal ();
			actualExtrusionTimes = 0;
			extrusionsSinceOperation = 0;
			ExtrusionOperation operation = new ExtrusionOperation();
			initializeDataStructure(ref noIntersection, ref originPoly);

			//Extrude the tunnel
			while (maxHoles >= 0 && actualExtrusionTimes <= maxExtrudeTimes) {
				IntersectionsController.Instance.addPolyline (originPoly);
				++actualExtrusionTimes;
				//Generate the new polyline applying the operation
				newPoly = extrude (operation, originPoly, ref actualDirection, ref actualDistance, ref noIntersection);
				if (newPoly == null)
					continue;
				//Make hole?
				if (operation.holeOperation()) {
					noIntersection = -1;
					Polyline polyHole = makeHole (originPoly, newPoly);
					addElementToDataStructure (polyHole, IntersectionsController.Instance.getLastBB () + 1);
				}

				//Triangulate from origin to new polyline as a tube/cave shape
				proceduralMesh.triangulatePolylines (originPoly, newPoly);
				//Set next operation and extrude
				DecisionGenerator.Instance.generateNextOperation(ref operation, ref extrusionsSinceOperation,actualExtrusionTimes,holeProb);
				originPoly = newPoly;
			}
			IntersectionsController.Instance.addPolyline (originPoly);
			IntersectionsController.Instance.addActualBox ();
			proceduralMesh.closePolyline(originPoly);
			holeProb -= 0.01f;
		}
	}
}
