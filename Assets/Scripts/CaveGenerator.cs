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

	public GameObject player;

	private GameObject lines; //In order to have the lines classified on same group
	private List<Geometry.Mesh> proceduralMesh;
	public Material caveMaterial;
	private GameObject[] tunnelsArray;
	public bool showGeneration = false;

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
		}
	}

	/** Function to be called in order to start generating the cave **/
	public void startGeneration (InitialPolyline iniPol) {
		AbstractGenerator generator; 
		//Start the generation
		switch (method) {
		case(generationMethod.Recursive): {
				//generator = (RecursiveGenerator)generator;// new RecursiveGenerator ();
				generator= gameObject.AddComponent<RecursiveGenerator> ();
				break;
			}
		case(generationMethod.IterativeStack): {
				//generator = (StackGenerator)generator;//new StackGenerator ();
				generator= gameObject.AddComponent<StackGenerator> ();

				break;
			}
		case(generationMethod.IterativeQueue): {
				//generator = (QueueGenerator)generator;//new QueueGenerator ();
				generator= gameObject.AddComponent<QueueGenerator> ();

				break;
			}
		default:
			return;
		}

		float tunnelHoleProb = 0.8f;
		generator.initialize (gateSize, iniPol, tunnelHoleProb, maxHoles, maxExtrudeTimes);
		//First create as many game objects as the number of tunnels and initialize them 
		GameObject tunnels = new GameObject ("Tunnels");
		tunnelsArray = new GameObject[maxHoles + 1];
		for (int i = 0; i < maxHoles + 1; ++i) {
			//Add the new game object(tunnel)
			GameObject tunnel = new GameObject ("Tunnel " + i);
			if (i == 0)
				tunnel.name = "Stalagmites";
			tunnel.transform.parent = tunnels.transform;
			//Debug script
			tunnel.AddComponent<DebugTunnel> ();
			//Generate the needed components for the rendering and collision, and attach them to the tunnel
			MeshFilter filter = tunnel.AddComponent <MeshFilter> ();
			MeshCollider collider = tunnel.AddComponent <MeshCollider> ();
			MeshRenderer renderer = tunnel.AddComponent<MeshRenderer> ();
			renderer.material = new Material (caveMaterial);
			renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			tunnelsArray [i] = tunnel;
		}
			
		//Use coroutines to show the generation steps
		StartCoroutine (generator.generate (iniPol, tunnelHoleProb));

	}

	/**Updates the tunnel meshes with the generated ones **/
	public void updateMeshes(AbstractGenerator generator) {
		proceduralMesh = generator.getMesh ();
		long verticesNum = 0, trianglesNum = 0;
		int actTunel = 0;
		foreach (Geometry.Mesh m in proceduralMesh) { //Attach the generated mesh to Unity stuff
			verticesNum += m.getNumVertices ();
			trianglesNum += m.getNumTriangles ();

			//Smooth the mesh
			if (generator.finished) 
				m.smooth (smoothIterations);

			//Generation finished, assign the vertices and triangles created to a Unity mesh
			UnityEngine.Mesh mesh = new UnityEngine.Mesh ();
			mesh.SetVertices (m.getVertices ());
			mesh.SetTriangles (m.getTriangles (), 0);
			mesh.SetUVs (0, m.getUVs ());
			//http://schemingdeveloper.com/2014/10/17/better-method-recalculate-normals-unity/
			mesh.RecalculateNormals ();
			mesh.RecalculateBounds ();
			//Assing it to the corresponding game object
			GameObject tunnel = tunnelsArray [actTunel];
			tunnel.GetComponent<MeshFilter> ().mesh = mesh;
			tunnel.GetComponent<MeshCollider>().sharedMesh = mesh;
			++actTunel;
		}

		//Mesh size
		if (generator.finished) {
			Debug.Log ("Vertices generated: " + verticesNum);
			Debug.Log ("Triangles generated: " + trianglesNum);

			//Put the player on scene
			preparePlayer (initialPoints);
			Debug.Log ("Cave generated");
		}
	}

	/** Gets all the player related stuff ready **/
	void preparePlayer(Polyline iniPol) {
		//Instantiate the player at the cave entrance
		GameObject pl = Instantiate(player);
		pl.transform.position = iniPol.calculateBaricenter () + new Vector3 (0.0f, 0.0f, caveDistance);

		cam.enabled = false;
		cam.GetComponent<AudioListener> ().enabled = false;
	}

}
