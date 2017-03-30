using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

/** Generates the cave by extruding the holes by FIFO **/
public class StackGenerator : IterativeGenerator {

	Stack<Polyline> polylinesStack;
	Stack<int> noIntersectionsQueue;

	protected override void createDataStructure (Polyline iniP){
		polylinesStack = new Stack<Polyline> ();
		noIntersectionsQueue = new Stack<int> ();
		polylinesStack.Push(iniP);
		noIntersectionsQueue.Push (-1);
	}

	protected override bool isDataStructureEmpty (){
		return polylinesStack.Count > 0;
	}

	protected override void initializeDataStructure (ref int canIntersect, ref Polyline p){
		canIntersect = noIntersectionsQueue.Pop ();
		p = polylinesStack.Pop ();
	}

	protected override void addElementToDataStructure (Polyline p, int canIntersect) {
		polylinesStack.Push (p);
		noIntersectionsQueue.Push (canIntersect);
	}
}
