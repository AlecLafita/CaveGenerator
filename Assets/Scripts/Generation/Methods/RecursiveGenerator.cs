using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

/** Generates the cave by recursive calls each time a hole is done **/
public class RecursiveGenerator : AbstractGenerator {

	public override void generate() {
		generate (gatePolyline, initialTunelHoleProb,-1);
	}


	private void generate(Polyline originPoly, float holeProb, int canIntersect) {
		//Hole is done, update the counter
		--maxHoles;

		//Base case, triangulate the actual tunnel last polyline as a polygon to close the hole
		if (maxHoles < 0 ) { 
			proceduralMesh.closePolyline(originPoly);
			return;
		}
		//TODO: change maxExtrudeTimes as holes are done (eg, random number between a rank)

		//Generate the actual hallway/tunnel
		ExtrusionOperation actualOperation = new ExtrusionOperation();
		float actualDistance = DecisionGenerator.Instance.generateDistance(true);
		Vector3 actualDirection = originPoly.calculateNormal ();
		int extrusionsSinceOperation = 0;
		for (int i = 0; i < maxExtrudeTimes; ++i) {
			//Add actual polyline to the next intersection BB
			IntersectionsController.Instance.addPolyline(originPoly);
			//Generate the new polyline applying the corresponding operation
			Polyline newPoly = extrude (actualOperation, originPoly, ref actualDirection, ref actualDistance, ref canIntersect);
			if (newPoly == null) { //Intersection produced
				//TODO: improve this
				continue;
			}
			//Make hole?
			if (actualOperation.holeOperation()) {
				if (maxHoles >= 0 )
					IntersectionsController.Instance.addActualBox ();
				canIntersect = IntersectionsController.Instance.getLastBB()+1; //Avoid intersection check with hole first BB
				Polyline polyHole = makeHole (originPoly, newPoly);
				generate(polyHole, holeProb-0.01f, IntersectionsController.Instance.getLastBB());
				//if (maxHoles > 0 ) before the recursive call. This comrobation won't be done as it is redundant 
				// (it was the last polyline to be added IC, so it won't be added again)
				IntersectionsController.Instance.addPolyline(originPoly);
			}
			//Triangulate from origin to new polyline as a tube/cave shape
			proceduralMesh.triangulatePolylines (originPoly, newPoly);
			//Set next operation and continue from the new polyline
			DecisionGenerator.Instance.generateNextOperation(ref actualOperation, ref extrusionsSinceOperation,i,holeProb);
			originPoly = newPoly;
		}
		//Finally, close the actual hallway/tunnel
		IntersectionsController.Instance.addPolyline(originPoly);
		IntersectionsController.Instance.addActualBox ();
		proceduralMesh.closePolyline(originPoly);
	}

}