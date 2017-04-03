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

		//Base case, triangulate the actual tunnel last polyline as a polygon to close the hole
		if (maxHoles < 0 || checkInvalidWalk(originPoly)) { 
			proceduralMesh.closePolyline(originPoly);
			return;
		}
		//TODO: change maxExtrudeTimes as holes are done (eg, random number between a rank)

		//Generate the actual hallway/tunnel
		ExtrusionOperations actualOperation = DecisionGenerator.Instance.generateNewOperation (originPoly);
		int extrusionsSinceOperation = 0;
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
				if (maxHoles >= 0 )
					IntersectionsController.Instance.addActualBox ();
				actualOperation.setCanIntersect(IntersectionsController.Instance.getLastBB()+1); //Avoid intersection check with hole first BB
				Polyline polyHole = makeHole (originPoly, newPoly);
				//if (polyHole != null) //Check the hole was done without problems
					generate(polyHole, holeProb-0.001f);
				//if (maxHoles > 0 ) before the recursive call. This comrobation won't be done as it is redundant 
				// (it was the last polyline to be added IC, so it won't be added again)
				IntersectionsController.Instance.addPolyline(originPoly);

				//Provisional, TODO: change this
				actualOperation.forceHoleOperation (false);
				actualOperation.forceDistanceOperation (DecisionGenerator.Instance.generateDistance (false));
			}
			//Triangulate from origin to new polyline as a tube/cave shape
			proceduralMesh.triangulatePolylines (originPoly, newPoly);
			//Set next operation and continue from the new polyline
			originPoly = newPoly;
			DecisionGenerator.Instance.generateNextOperation(originPoly, ref actualOperation, ref extrusionsSinceOperation,i,holeProb);
		}
		//Finally, close the actual hallway/tunnel
		IntersectionsController.Instance.addPolyline(originPoly);
		IntersectionsController.Instance.addActualBox ();
		proceduralMesh.closePolyline(originPoly);
	}

}