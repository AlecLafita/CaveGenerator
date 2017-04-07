using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

/** Generates the cave by recursive calls each time a hole is done **/
public class RecursiveGenerator : AbstractGenerator {

	public override void generate() {
		generate (gatePolyline, initialTunelHoleProb);
	}


	private void generate(Polyline originPoly, float holeProb) {
		//Hole is done, update the counter
		--maxHoles;

		//Case base is implicit, as the operation generation takes into account the maxHoles variables in order to stop generating holes

		//TODO: change maxExtrudeTimes as holes are done (eg, random number between a rank)

		//Generate the actual hallway/tunnel
		ExtrusionOperations actualOperation = DecisionGenerator.Instance.generateNewOperation (originPoly);
		int extrusionsSinceOperation = -1; //Make sure the first two polylines are added as BB
		for (int i = 0; i < maxExtrudeTimes; ++i) {
			//Add actual polyline to the next intersection BB
			IntersectionsController.Instance.addPolyline(originPoly);
			//Generate the new polyline applying the corresponding operation
			Polyline newPoly = extrude (actualOperation, originPoly);
			if (newPoly == null) { //Intersection produced
				//TODO: improve this
				//DecisionGenerator.Instance.generateNextOperation(originPoly, ref actualOperation, ref extrusionsSinceOperation,i,holeProb);
				actualOperation = DecisionGenerator.Instance.generateNewOperation (originPoly);
				continue;
			}
			//Make hole?
			if (actualOperation.holeOperation()) {
				Polyline polyHole = makeHole (originPoly, newPoly);
				if (polyHole != null) { //Check the hole was done without problems
					IntersectionsController.Instance.addActualBox ();
					actualOperation.setCanIntersect(IntersectionsController.Instance.getLastBB()+1); //Avoid intersection check with hole first BB
					generate (polyHole, holeProb - 0.001f);
					IntersectionsController.Instance.addPolyline(originPoly);
				}
				//TODO: ELSE, reextrude without hole (this is due to it is generated with big distance)
				//if (maxHoles > 0 ) before the recursive call. This comrobation won't be done as it is redundant 
				// (it was the last polyline to be added IC, so it won't be added again)
			}
			//Triangulate from origin to new polyline as a tube/cave shape
			proceduralMesh.triangulatePolylines (originPoly, newPoly);
			//Set next operation and continue from the new polyline
			originPoly = newPoly;
			DecisionGenerator.Instance.generateNextOperation(originPoly, ref actualOperation, ref extrusionsSinceOperation,i,holeProb, maxHoles);
		}
		//Finally, close the actual hallway/tunnel
		IntersectionsController.Instance.addPolyline(originPoly);
		IntersectionsController.Instance.addActualBox ();
		proceduralMesh.closePolyline(originPoly);
	}

}