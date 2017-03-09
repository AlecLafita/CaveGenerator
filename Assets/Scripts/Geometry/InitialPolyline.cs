using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry {

	/**Class to help to create the initial polyline **/
	public class InitialPolyline : Polyline { 

		private int mActualPos = 0; //Actual vertex position

		//Constructors
		public InitialPolyline() : base() {}
		public InitialPolyline(int numV) : base(numV){}

		//Setters
		public void initializeIndices() {
			for (int i = 0; i < mNumV; ++i) {
				mVertices [i].setIndex (i);
			}
		}
		public void addPosition(Vector3 newPos) {
			if (mActualPos >= mNumV) { //TODO:exception
				Debug.Log ("Number of index bigger than size");
				return;
			}
			mVertices[mActualPos].setPosition(newPos);
			++mActualPos;
		}
	}
}