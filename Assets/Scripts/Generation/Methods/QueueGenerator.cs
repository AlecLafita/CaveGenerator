using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

/** Generates the cave by extruding the holes by FIFO **/
public class QueueGenerator : IterativeGenerator {

	Queue<Polyline> polylinesStack;
	Queue<int> noIntersectionsQueue;

	protected override void createDataStructure (Polyline iniP){
		polylinesStack = new Queue<Polyline> ();
		noIntersectionsQueue = new Queue<int> ();
		polylinesStack.Enqueue(iniP);
		noIntersectionsQueue.Enqueue (-1);
	}

	protected override bool isDataStructureEmpty (){
		return polylinesStack.Count > 0;
	}

	protected override void initializeDataStructure (ref int canIntersect, ref Polyline p){
		canIntersect = noIntersectionsQueue.Dequeue ();
		p = polylinesStack.Dequeue ();
	}

	protected override void addElementToDataStructure (Polyline p, int canIntersect) {
		polylinesStack.Enqueue (p);
		noIntersectionsQueue.Enqueue (canIntersect);
	}
}
