using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry {

	/**Class that helps to create the initial polyline **/
	public class InitialPolyline : Polyline { 

		private int mActualPos = 0; //Actual vertex position

		//******** Constructors ********//
		public InitialPolyline() : base() {}
		public InitialPolyline(int numV) : base(numV){}

		//******** Setters ********//
		public void initializeIndices() {
			for (int i = 0; i < mVertices.Length; ++i) {
				mVertices [i].setIndex (i);
			}
		}

		public void addPosition(Vector3 newPos) {
			if (mActualPos >= mVertices.Length) { //TODO:exception
				Debug.Log ("Number of index bigger than size");
				return;
			}
			mVertices[mActualPos].setPosition(newPos);
			++mActualPos;
		}
	}
}