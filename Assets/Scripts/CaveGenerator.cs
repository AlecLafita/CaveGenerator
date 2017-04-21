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

	public int smoothIterations = 1;

	public enum generationMethod
	{
		Recursive, IterativeStack, IterativeQueue
	}
	public generationMethod method = generationMethod.Recursive;
	public bool debugBB = false;
	public bool debugTriangles = false;

	public GameObject player;

	private GameObject lines; //In order to have the lines classified on same group
	private List<Geometry.Mesh> proceduralMesh;
	public Material caveMaterial;

	void Start () {
		initialPoints = new InitialPolyline(gateSize);
		pointsSelected = 0;
		generatorCalled = false;
		lines = new GameObject ("Start Lines");
	}

	/** Draw a line between start and end **/
	void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
	{
		GameObject myLine = new GameObject();
		myLine.transform.parent = lines.transform;
		myLine.transform.position = start;
		LineRenderer lr = myLine.AddComponent<LineRenderer>();
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
			startGeneration(initialPoints);
			generatorCalled = true;
			Debug.Log ("Cave generated");
		}
	}

	/** Function to be called in order to start generating the cave **/
	public void startGeneration (InitialPolyline iniPol) {
		for (int i = 0; i < 3;++i)
			iniPol.smoothMean ();

		//DEBUG smooth line
		for (int i = 0; i < iniPol.getSize (); ++i) {
			DrawLine (iniPol.getVertex (i).getPosition ()-new Vector3(0.0f,0.0f,1.0f), 
				iniPol.getVertex (i + 1).getPosition () -new Vector3(0.0f,0.0f,1.0f), Color.black, 30.0f);
		}

		AbstractGenerator generator;
		//Start the generation
		switch (method) {
		case(generationMethod.Recursive): {
				generator = new RecursiveGenerator ();
				break;
			}
		case(generationMethod.IterativeStack): {
				generator = new QueueGenerator ();
				break;
			}
		case(generationMethod.IterativeQueue): {
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

		long verticesNum = 0;
		long trianglesNum = 0;
		GameObject tunnels = new GameObject ("Tunnels");
		int actTunel = 0;
		foreach (Geometry.Mesh m in proceduralMesh) { //Attach the generated mesh to Unity stuff
			verticesNum += m.getNumVertices();
			trianglesNum += m.getNumTriangles ();
			//Smooth the mesh
			//m.smooth (smoothIterations);

			//Generation finished, assign the vertices and triangles created to a Unity mesh
			UnityEngine.Mesh mesh = new UnityEngine.Mesh ();
			mesh.SetVertices (m.getVertices ());
			mesh.SetTriangles (m.getTriangles (), 0);
			mesh.SetUVs (0, m.getUVs ());
			//http://schemingdeveloper.com/2014/10/17/better-method-recalculate-normals-unity/
			mesh.RecalculateNormals ();
			mesh.RecalculateBounds ();

			//Add the new game object(tunnel)
			GameObject tunnel = new GameObject ("Tunnel " + actTunel);
			tunnel.transform.parent = tunnels.transform;
			//Generate the needed components for the rendering and collision, and attach them to the tunnel
			MeshFilter filter = tunnel.AddComponent <MeshFilter>();
			filter.mesh = mesh;
			MeshCollider collider = tunnel.AddComponent <MeshCollider>();
			collider.sharedMesh = mesh;
			MeshRenderer renderer =  tunnel.AddComponent<MeshRenderer> ();
			renderer.material = new Material (caveMaterial);
			renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			++actTunel;
		}

		//MEsh size
		Debug.Log ("Vertices generated: " + verticesNum);
		Debug.Log ("Triangles generated: " + trianglesNum);

		//Put the player on scene
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
			foreach (Geometry.Mesh m in proceduralMesh) {
				//Draw triangles vertices
				Vector3[] vertices = m.getVertices ().ToArray ();
				for (int i = 0; i < vertices.Length; ++i) {
					Gizmos.DrawWireSphere (vertices [i], 0.1f);
				}
				//Draw triangles edges
				int[] triangles = m.getTriangles ().ToArray ();
				Gizmos.color = Color.blue;
				for (int i = 0; i < triangles.Length; i += 3) {
					Gizmos.DrawLine (vertices [triangles [i]], vertices [triangles [i + 1]]);
					Gizmos.DrawLine (vertices [triangles [i + 1]], vertices [triangles [i + 2]]);
					Gizmos.DrawLine (vertices [triangles [i + 2]], vertices [triangles [i]]);
				}
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
