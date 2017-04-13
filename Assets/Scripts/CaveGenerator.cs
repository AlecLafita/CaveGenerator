using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;


/** Main class. Waits the user to select the cave gate and then generates it **/
public class CaveGenerator : MonoBehaviour {

	public Camera cam;
	public int gateSize = 3; //Number of points the cave'gate will have
	public float caveDistance = 10.0f; //Depth(z) where the cave will start on, from the camera
	private int pointsSelected; //Number of points the user has selected
	private bool generatorCalled; //In order to generate the cave just once
	InitialPolyline initialPoints;

	public int maxHoles = 50; //How many times a hole can be extruded and behave like a tunnel
	public int maxExtrudeTimes = 100; // How many times an extrusion can be applied from a hole initially

	public enum generationMethod
	{
		Recursive, IterativeStack, IterativeQueue
	}
	public generationMethod method = generationMethod.Recursive;
	public bool debugBB = false;
	public bool debugTriangles = false;

	public GameObject player;

	private Geometry.Mesh proceduralMesh;

	void Start () {
		initialPoints = new InitialPolyline(gateSize);
		pointsSelected = 0;
		generatorCalled = false;
	}

	/** Draw a line between start and end **/
	void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
	{
		GameObject myLine = new GameObject();
		myLine.transform.position = start;
		myLine.AddComponent<LineRenderer>();
		LineRenderer lr = myLine.GetComponent<LineRenderer>();
		lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
		lr.SetColors(color, color);
		lr.SetWidth(0.05f, 0.05f);
		lr.SetPosition(0, start);
		lr.SetPosition(1, end);
		//GameObject.Destroy(myLine, duration);
	}

	void Update () {
		/** Waits the user to select the points that will form the cave gate (initial polyline). 
		 * Once all the points have been selected thorugh clicks, it calls the function that generates
		 * the cave. The initial points are all on the XY plane and must be CLOCKWISE **/
		if (Input.GetMouseButtonDown (0) && pointsSelected < gateSize) { //left click
			Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f);
			pos = cam.ScreenToWorldPoint(pos);
			if (pointsSelected > 0) {
				DrawLine (initialPoints.getVertex (pointsSelected-1).getPosition(), pos, Color.green, 20.0f);
			}
			initialPoints.addPosition (pos);
			Debug.Log (pos);
			++pointsSelected;
			if (pointsSelected == gateSize) {
				DrawLine (pos, initialPoints.getVertex (0).getPosition(), Color.green, 20.0f);

				Debug.Log ("All points selected, press right mouse button");
			}
		}

		if (Input.GetMouseButtonDown (1) && pointsSelected==gateSize && !generatorCalled) {//right click
			//Generate the cave when the user has selected all the points
			cam.ResetProjectionMatrix();
			Debug.Log("Starting generation");
			//TODO:check it's clockwise. Otherwise, transform it
			initialPoints.initializeIndices();
			initialPoints.generateUVs ();
			startGeneration(initialPoints);
			generatorCalled = true;
			Debug.Log ("Cave generated");
		}
	}

	/** Function to be called in order to start generating the cave **/
	public void startGeneration (InitialPolyline iniPol) {
		AbstractGenerator generator;
		//Start the generation
		switch (method) {
		case(generationMethod.Recursive): {
				//generator = new CaveGenerator ();
				//generateRecursive (iniPol, tunnelHoleProb,-1);
				generator = new RecursiveGenerator ();

				break;
			}
		case(generationMethod.IterativeStack): {
				//generateIterativeStack (iniPol, tunnelHoleProb);
				generator = new QueueGenerator ();

				break;
			}
		case(generationMethod.IterativeQueue): {
				//generateIterativeQueue (iniPol, tunnelHoleProb);
				generator = new QueueGenerator ();
				break;
			}
		default:
			return;
		}

		float tunnelHoleProb = 0.8f;
		generator.initialize (iniPol, tunnelHoleProb, maxHoles, maxExtrudeTimes);
		generator.generate ();
		proceduralMesh = generator.getMesh ();

		Debug.Log ("Vertices generated: " + proceduralMesh.getNumVertices ());
		Debug.Log ("Triangles generated: " + proceduralMesh.getNumTriangles ());

		//Generation finished, assign the vertices and triangles created to a Unity mesh
		UnityEngine.Mesh mesh = new UnityEngine.Mesh ();
		//mesh.vertices = mVertices.ToArray(); //Slower
		mesh.SetVertices (proceduralMesh.getVertices());
		//mesh.triangles = mTriangles.ToArray ();
		mesh.SetTriangles (proceduralMesh.getTriangles(),0);
		mesh.SetUVs (0, proceduralMesh.getUVs ());
		//http://schemingdeveloper.com/2014/10/17/better-method-recalculate-normals-unity/
		mesh.RecalculateNormals ();
		mesh.RecalculateBounds();

		//Assign the created mesh to the one we are storing and visualizing
		GetComponent<MeshFilter> ().mesh = mesh;

		//Assign the mesh to the collider
		GetComponent<MeshCollider>().sharedMesh = mesh;

		preparePlayer (iniPol);

	}

	/** Gets all the player related stuff ready **/
	void preparePlayer(Polyline iniPol) {
		//Instantiate the player at the cave entrance
		GameObject pl = Instantiate(player);
		pl.transform.position = iniPol.calculateBaricenter () + new Vector3 (0.0f, 0.0f, caveDistance);

		cam.enabled = false;
		cam.GetComponent<AudioListener> ().enabled = false;
	}

	/** For debug purposes **/
	void OnDrawGizmos() { 
		//Avoid error messages after stopping
		if (!Application.isPlaying) return; 

		if (debugTriangles) {
			//Draw triangles vertices
			Vector3[] vertices = proceduralMesh.getVertices ().ToArray ();
			for (int i = 0; i < vertices.Length; ++i) {
				Gizmos.DrawWireSphere (vertices [i], 0.1f);
			}

			//Draw triangles edges
			int[] triangles = proceduralMesh.getTriangles ().ToArray ();
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
