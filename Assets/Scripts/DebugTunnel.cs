using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

public class DebugTunnel : MonoBehaviour {

	public bool debugBB = false;
	public bool debugTriangles = false;

	void OnDrawGizmos() { 
		//Avoid error messages after stopping
		if (!Application.isPlaying) return; 

		if (debugTriangles) {
			UnityEngine.Mesh m = GetComponent<MeshFilter> ().mesh;
			//Draw triangles vertices
			Vector3[] vertices = m.vertices;
			Gizmos.color = Color.yellow;//For the first vertex
			for (int i = 0; i < vertices.Length; ++i) {
				if (i > 0)
					Gizmos.color = Color.green;
				Gizmos.DrawWireSphere (vertices [i], 0.1f);
			}
			//Draw triangles edges
			int[] triangles = m.triangles;
			Gizmos.color = Color.blue;
			for (int i = 0; i < triangles.Length; i += 3) {
				Gizmos.DrawLine (vertices [triangles [i]], vertices [triangles [i + 1]]);
				Gizmos.DrawLine (vertices [triangles [i + 1]], vertices [triangles [i + 2]]);
				Gizmos.DrawLine (vertices [triangles [i + 2]], vertices [triangles [i]]);
			}
		}
		if (debugBB) {
			//Draw intersection BBs
			List<Bounds> BBs = IntersectionsController.Instance.getBBs ();
			foreach (Bounds BB in BBs) {
				Gizmos.DrawCube (BB.center, BB.size);
			}
		}
	}
}
