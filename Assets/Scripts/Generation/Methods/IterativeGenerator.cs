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
			Geometry.Mesh m = initializeTunnel(originPoly);
			actualExtrusionTimes = 0;
			extrusionsSinceOperation = -1; //Make sure the first two polylines are added as BB
			ExtrusionOperations operation = DecisionGenerator.Instance.generateNewOperation (originPoly);
			operation.setCanIntersect (noIntersection);
			//Add first polyline to the intersection BB
			IntersectionsController.Instance.addPolyline (originPoly);
			//Generate the tunnel
			while (actualExtrusionTimes <= maxExtrudeTimes) {
				++actualExtrusionTimes;
				//In case the hole is finally not done, same operation will need to be applied
				ExtrusionOperations actualOpBackTrack = new ExtrusionOperations(operation); 
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
					} else { //No hole could be done, reextrude
						//Force to have little extrusion distance
						actualOpBackTrack.forceDistanceOperation (1, DecisionGenerator.Instance.generateDistance (false));
						//It can't be null if with bigger extrusion distance it wasn't already: if
						//with bigger distance it didn't intersect, it can't intersect with a smaller one
						newPoly = extrude (actualOpBackTrack, originPoly);
						operation = actualOpBackTrack;
					}
					operation.forceHoleOperation (false);
				}
				//Adds the new polyline to the mesh, after all the changes previously done
				actualMesh.addPolyline (newPoly);
				//Triangulate from origin to new polyline as a tube/cave shape
				actualMesh.triangulatePolylines (originPoly, newPoly);
				//Set next operation and continue from the new polyline
				originPoly = newPoly;
				//Add actual polyline to the next intersection BB ang get nexxt operation
				IntersectionsController.Instance.addPolyline(originPoly);
				DecisionGenerator.Instance.generateNextOperation(originPoly, operation, ref extrusionsSinceOperation,actualExtrusionTimes,holeProb, maxHoles);
			}
			IntersectionsController.Instance.addActualBox ();
			actualMesh.closePolyline(originPoly);
			holeProb -= 0.001f;
		}
	}
}
