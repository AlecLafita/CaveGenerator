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
		int actualExtrusionTimes, extrusionsSinceOperation, noIntersection;
		noIntersection = -1;
		while (isDataStructureEmpty()) {
			//new tunnel(hole) will be done, initialize all the data
			//Case base is implicit, as the operation generation takes into account the maxHoles variables in order to stop generating holes
			initializeDataStructure(ref noIntersection, ref originPoly);
			actualExtrusionTimes = 0;
			extrusionsSinceOperation = 0;
			ExtrusionOperations operation = DecisionGenerator.Instance.generateNewOperation (originPoly);
			operation.setCanIntersect (noIntersection);

			//Extrude the tunnel
			while (actualExtrusionTimes <= maxExtrudeTimes) {
				IntersectionsController.Instance.addPolyline (originPoly);
				++actualExtrusionTimes;
				//Generate the new polyline applying the operation
				newPoly = extrude (operation, originPoly);
				if (newPoly == null) {
					//DecisionGenerator.Instance.generateNextOperation (ref operation, ref extrusionsSinceOperation, actualExtrusionTimes, holeProb);
					operation = DecisionGenerator.Instance.generateNewOperation (originPoly);
					continue;
				}
				//Make hole?
				if (operation.holeOperation()) {
					noIntersection = -1;
					operation.setCanIntersect (noIntersection);
					Polyline polyHole = makeHole (originPoly, newPoly);
					if (polyHole != null) {//Check the hole was done without problems
						addElementToDataStructure (polyHole, IntersectionsController.Instance.getLastBB () + 1);
						--maxHoles;
					}
					//TODO: ELSE, reextrude without hole (this is due to it is generated with big distance)

				}

				//Triangulate from origin to new polyline as a tube/cave shape
				proceduralMesh.triangulatePolylines (originPoly, newPoly);
				//Set next operation and extrude
				originPoly = newPoly;
				DecisionGenerator.Instance.generateNextOperation(originPoly, ref operation, ref extrusionsSinceOperation,actualExtrusionTimes,holeProb, maxHoles);
			}
			IntersectionsController.Instance.addPolyline (originPoly);
			IntersectionsController.Instance.addActualBox ();
			proceduralMesh.closePolyline(originPoly);
			holeProb -= 0.001f;
		}
	}
}
