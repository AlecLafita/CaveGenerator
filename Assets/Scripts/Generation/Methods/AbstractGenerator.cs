using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;


/** Abstract class that manages the cave generation. The differents subclasses differ on the why the holes/tunnels are extruded **/
abstract public class AbstractGenerator : MonoBehaviour{

	protected List<Geometry.Mesh> proceduralMeshes; //Mesh that will be modified during the cave generation
	protected Geometry.Mesh actualMesh; //Mesh coresponding to the actual tunnel being generated
	protected List<Geometry.Mesh> stalagmitesMeshes; //Mesh corresponding to the stalagmites and so TODO:MAke more than one mesh in case there are lot of estalamgites!
	protected float initialTunelHoleProb; //holes can be created depending on this probability [0-1]
	protected int maxHoles; //How many times a hole can be extruded and behave like a tunnel, acts as a countdown
	protected int maxExtrudeTimes; //TODO: consider to deccrement this value as holes are created, or some random function that handles this
	protected InitialPolyline gatePolyline; //Polyline where the cave starts from
	protected int entranceSize; //Number of vertices of a tunnel entrance
	protected GameObject lights; // GameObject that contains all the generated lights
	public bool finished; //Check if the generation has beeen completed

	protected bool showGeneration; //Use yield or not to show the generation
	protected const float extrusionTime = 0.1f;
	protected const float holeTime = 1.0f;

	public void Awake() {
		proceduralMeshes = new List<Geometry.Mesh> ();
		stalagmitesMeshes = new List<Geometry.Mesh> ();
		stalagmitesMeshes.Add (new Geometry.Mesh ());
		lights = new GameObject ("Lights");
		finished = false;
	}

	private int smoothIterations = 3;
	/**Initialize, being the arguments the needed parameters for the generator **/
	public void initialize(int gateSize, InitialPolyline iniPol, float initialTunelHoleProb, int maxHoles, int maxExtrudeTimes) {
		showGeneration = gameObject.GetComponent<CaveGenerator> ().showGeneration;
		for (int i = 0; i < smoothIterations;++i)
			iniPol.smoothMean ();
		iniPol.generateUVs ();
		iniPol.duplicateFirstVertex ();
		gatePolyline = iniPol;
		this.initialTunelHoleProb = initialTunelHoleProb;
		this.maxHoles = maxHoles;
		this.maxExtrudeTimes = maxExtrudeTimes;
		entranceSize = gateSize;
		if (entranceSize % 2 != 0) //Force it to be pair
			++entranceSize;
	}

	/**Initializes the tunnel initial polyline, returning the corresponding mesh and setting it as the actual one**/
	protected Geometry.Mesh initializeTunnel(ref Polyline iniPol) {
		((InitialPolyline)iniPol).initializeIndices();
		//Create the new mesh with the hole polyline
		Geometry.Mesh m = new Geometry.Mesh (iniPol);
		proceduralMeshes.Add (m);
		actualMesh = m;
		return m;
	}

	/**Generates the cave **/
	abstract public IEnumerator generate (Polyline originPoly, float holeProb);

	/**Returns the generated list of meshes **/
	public List<Geometry.Mesh> getMesh() {
		return proceduralMeshes;
	}
	/** Returns the meshes od the stalagmites **/
	public List<Geometry.Mesh> getStalagmitesMesh() {
		return stalagmitesMeshes;
	}

	public static float UVfactor = 25.0f;
	/**It creates a new polyline from an exsiting one, applying the corresponding operations**/
	protected Polyline extrude(ExtrusionOperations operation, Polyline originPoly) {
		//Create the new polyline from the actual one
		Polyline newPoly = new Polyline(originPoly.getSize());
		Vector3 direction = operation.directionOperation().apply();
		float distance = operation.distanceOperation().apply();
		//Generate the UVS of the new polyline from the coordinates of the original and on the same
		//same direction that the extrusion, as if it was a projection to XZ plane
		//Vector2 UVincr = new Vector2(direction.x,direction.z);
		Vector2 UVincr = new Vector2(0.0f,1.0f);
		UVincr.Normalize ();
		UVincr *= (distance / UVfactor);
		for (int i = 0; i < originPoly.getSize(); ++i) { //Generate the new vertices
			//Add vertex to polyline
			newPoly.extrudeVertex(i, originPoly.getVertex(i).getPosition(), direction, distance);
			//Add the index to vertex
			newPoly.getVertex(i).setIndex(actualMesh.getNumVertices() + i);
			//Add UV
			newPoly.getVertex(i).setUV(originPoly.getVertex(i).getUV() + UVincr);
		}

		//Apply operations, if any
		if (operation.scaleOperation().needApply()) {
			newPoly.scale(operation.scaleOperation().apply());
		}
		if (operation.rotateOperation().needApply() ) {
			newPoly.rotate(operation.rotateOperation().apply());
		}
			
		//Check there is no intersection
		if (IntersectionsController.Instance.doIntersect (originPoly, newPoly, operation.getCanIntersect())) {
			return null;
		}

		return newPoly;
	}

	/** Makes a hole betwen two polylines and return this hole as a new polyline **/
	protected Polyline makeHole(Polyline originPoly, Polyline destinyPoly) {
		//TODO: more than one hole, Make two holes on same polylines pairs can cause intersections!
			//could take the negate direction of the already make hole and try to do a new one

		// FIRST: Decide where the hole will be done, take advantatge indices
		// on the two polylines are at the same order (the new is kind of a projection of the old)
		int sizeHole; int firstIndex;
		//DecisionGenerator.Instance.whereToDig (originPoly.getSize(), out sizeHole, out firstIndex);
		DecisionGenerator.Instance.whereToDig (originPoly, out sizeHole, out firstIndex);
		if (sizeHole < DecisionGenerator.Instance.holeMinVertices)
			return null;
		
		//SECOND: Create the hole polyline by marking and adding the hole vertices (from old a new polylines)
		InitialPolyline polyHole = new InitialPolyline (sizeHole);
		//Increasing order for the origin and decreasing for the destiny polyline in order to 
		//make a correct triangulation
		int i = 0;
		while (i < sizeHole / 2) {
			originPoly.getVertex (firstIndex +i).setInHole (true);
			polyHole.addVertex (originPoly.getVertex (firstIndex +i));
			++i;
		}
		//at this point i = sizeHole / 2;
		while (i > 0) {
			--i;
			destinyPoly.getVertex (firstIndex+i).setInHole (true);
			polyHole.addVertex (destinyPoly.getVertex (firstIndex+i));
		}

		//THIRD: Check is a valid hole: no artifacts will be produced, 
		bool invalidHole = false;
		//invalidHole = Geometry.Utils.checkArtifacts (polyHole);
		//and in the walking case, check if the hole is not too upwards or downwards(y component)
		invalidHole = invalidHole || Geometry.Utils.checkInvalidWalk(polyHole);
		//Undo hole if invalid
		if (invalidHole) {
			for (int j = 0; j < sizeHole/2; ++j) {
				originPoly.getVertex (firstIndex + j).setInHole (false);
				destinyPoly.getVertex (firstIndex + j).setInHole (false);
			}
			return null;
		}

		//FOURTH: Do the hole smooth: Project the polyline(3D) into a plane(2D) on the polyline normal direction, just n (not very big) vertices
		InitialPolyline planePoly = Geometry.Utils.generateProjection(polyHole, entranceSize,smoothIterations);

		//FIFTH: Last check if hole is really valid (intersection stuff and simple polyline check)
		if (!planePoly.isConvex () || !planePoly.isSimple () || IntersectionsController.Instance.doIntersect(polyHole,planePoly,-1)) {
			for (int j = 0; j < sizeHole/2; ++j) {
				originPoly.getVertex (firstIndex + j).setInHole (false);
				destinyPoly.getVertex (firstIndex + j).setInHole (false);
			}
			return null;
		}
		//In case the hole can really be done, add the extruded polyline to the mesh
		// (needed for triangulate correctly between the hole and the projection)
		actualMesh.addPolyline (destinyPoly);
		//And the corresponding hole vertices
		for (int j = 0; j < polyHole.getSize (); ++j) {
			actualMesh.addHoleIndex (polyHole.getVertex (j).getIndex ());
		}

		//SIXTH: Final propoerties of the projection
		//Generate new UVs coordinates of the projection, from y coord of the hole
		float yCoord = (polyHole.getVertex(0).getUV().y + polyHole.getVertex(-1).getUV().y)/2;
		//float projDistance = Vector3.Distance (polyHole.calculateBaricenter (), planePoly.calculateBaricenter ());
		//yCoord += projDistance / UVfactor;

		planePoly.generateUVs (yCoord);
		//Duplicate the first vertex in order to have good texturization between last and first vertex
		planePoly.duplicateFirstVertex ();
		//And put the corresponding indices
		for (int j = 0; j < planePoly.getSize (); ++j) {
			planePoly.getVertex(j).setIndex(actualMesh.getNumVertices()+j);
		}
		//Add the new polyline information to the mesh
		actualMesh.addPolyline (planePoly);

		///FINALLY: Triangulate between the hole and the projection and add the projection to the B intersections
		actualMesh.triangulateTunnelStart(polyHole,planePoly);
		IntersectionsController.Instance.addPolyline (planePoly);

		return planePoly;
	}


	private const float maxDiffAngle = 35.0f;
	private const float maxDiffAnglePillar = 10.0f;
	/** Makes a stalagmite between the two polylilnes (through the extrusion) **/
	protected void makeStalagmite (ExtrusionOperations.stalgmOp stalgmType, Polyline originPoly, Polyline newPoly){
		//Create a new stalgmite mesh if needed
		if (stalagmitesMeshes [stalagmitesMeshes.Count - 1].getNumVertices () > 65000)
			stalagmitesMeshes.Add (new Geometry.Mesh ());
		int numStalgm = 2;
		//Common part for stalgmites/stalgitates and pillars
		//Get the mean distance between vertices to know how many vertices take per stalgmite
		float dist = 0;
		for (int i = 0; i < originPoly.getSize (); ++i) {
			dist += Vector3.Distance (originPoly.getVertex (i).getPosition (), originPoly.getVertex (i + 1).getPosition ());
		}
		dist /= originPoly.getSize ();
		dist = Vector3.Distance(newPoly.calculateBaricenter(),originPoly.calculateBaricenter())/dist;
		int stalgmSize = 2*(int)dist;
		stalgmSize = Mathf.Min(Mathf.Max (stalgmSize, 4),10);
		//Find first vertices candidates
		//FIRST: Check each group of stalgmSize adjacent vertices and get the best candidate (normal distance nearer to +-up)
		Polyline[] stalgmPolys;

		if (stalgmType == ExtrusionOperations.stalgmOp.Stalactite)
			stalgmPolys = getStalagmiteIni (numStalgm,stalgmSize, originPoly, newPoly, Vector3.up);
		else
			stalgmPolys = getStalagmiteIni (numStalgm,stalgmSize, originPoly, newPoly, Vector3.down);
		if (stalgmPolys == null)
			return;
		for(int p = 0; p <numStalgm;++p) {
			int numExtrusions = Random.Range(4,10);//For last part
			Polyline stalgmPoly = stalgmPolys [p];
			if (stalgmPoly == null)
				continue;
			if (stalgmType == ExtrusionOperations.stalgmOp.Stalactite || stalgmType == ExtrusionOperations.stalgmOp.Stalagmite) {
				initializeStalagmiteIni (ref stalgmPoly);
				//THIRD: Prepare generation variables: #extrusion, scale value,.. We need to find this values in
				//order the stalagmite does not intersect with the extrusion(cross it) and don't look too ugly
				Vector3 stalgmBaricenter = Vector3.zero;
				for (int i = 0; i < stalgmSize / 2; ++i) {
					stalgmBaricenter += stalgmPoly.getVertex (i).getPosition ();
				}
				stalgmBaricenter /= (stalgmSize / 2);
				Vector3 stalgmDirection = stalgmPoly.calculateNormal ();
				if (stalgmType == ExtrusionOperations.stalgmOp.Stalactite)
					stalgmDirection = Vector3.up;
				else
					stalgmDirection = Vector3.down;
				//Find the "counterpart vertex", this is the vertex nearer to the intersection between the from baricenter
				//ray intersection on the stalgmite direction. Will be usefull to avoid corssings by knowing the max size of the stalgmite
				float minAngle = float.MaxValue;
				int counterpartVertex = 0;
				float auxAngle;
				for (int i = 0; i < originPoly.getSize () - 1; ++i) { //TODO: do some dicotomic search?
					auxAngle = Vector3.Angle (stalgmDirection, originPoly.getVertex (i).getPosition () - stalgmBaricenter);
					if (auxAngle < minAngle) {
						minAngle = auxAngle;
						counterpartVertex = i;
					}
				}
				float maxStalgmSize = (originPoly.getVertex (counterpartVertex).getPosition () - stalgmBaricenter).magnitude;
				maxStalgmSize *= Random.Range (0.30f, 0.75f); //final stalgmite size
				float stalgmExtrusionDistance = maxStalgmSize / (float)numExtrusions;
				//Scale value from stalgmite size and #extrusions
				float stalgmDiam = stalgmPoly.computeRadius () * 2;
				float finalStalgmRadius = 0.1f;
				float scaleValue = Mathf.Pow (finalStalgmRadius / stalgmDiam, 1 / (float)numExtrusions);
				Vector2 UVincr;
				if (stalgmType == ExtrusionOperations.stalgmOp.Stalactite)
					UVincr = new Vector2 (1.0f, -1.0f);
				else
					UVincr = new Vector2 (1.0f, 1.0f);
				UVincr.Normalize ();
				UVincr *= (stalgmExtrusionDistance / (UVfactor / 2));
				//FOURTH: Now we have the start of the stalgmite and all it's generation parameters
				extrudeStalagmite (numExtrusions, stalgmDirection, stalgmExtrusionDistance, scaleValue, UVincr, stalgmPoly);
			} else if (stalgmType == ExtrusionOperations.stalgmOp.Pillar) {
				//We have the stalagmite beggining, we now need to find the stalagtite one
				Polyline stalagitePoly = (getStalagmiteIni (1,stalgmSize, originPoly, newPoly, -stalgmPoly.calculateNormal ()))[0];
				if (stalagitePoly == null)
					return;
				Vector3 extrusionVector = stalagitePoly.calculateBaricenter () - stalgmPoly.calculateBaricenter ();
				if (Vector3.Angle (extrusionVector.normalized, Vector3.down) > maxDiffAnglePillar) //Check is not too horizontal
				return;
				initializeStalagmiteIni (ref stalgmPoly);
				//Common extrusion parameters
				float maxStalgmSize = extrusionVector.magnitude / 2;
				float stalgmExtrusionDistance = maxStalgmSize / (float)numExtrusions;
				//Scale value from stalgmite size and #extrusions
				float stalgmDiam = stalgmPoly.computeRadius () * 2;
				float finalStalgmRadius = 0.3f;
				float scaleValue = Mathf.Pow (finalStalgmRadius / stalgmDiam, 1 / (float)numExtrusions);
				Vector2 UVincr = new Vector2 (1.0f, -1.0f);
				UVincr.Normalize ();
				UVincr *= (stalgmExtrusionDistance / (UVfactor / 2));
				//Generate stalagmite part
				extrudeStalagmite (numExtrusions, extrusionVector.normalized, stalgmExtrusionDistance, scaleValue, UVincr, stalgmPoly);
				//Generate stalagtite part
				initializeStalagmiteIni (ref stalagitePoly);
				UVincr.y *= -1.0f;
				extrudeStalagmite (numExtrusions, -extrusionVector.normalized, stalgmExtrusionDistance, scaleValue, UVincr, stalagitePoly);
			}
		}
	}
	/**From two extrusion polylines, get the polylines that are the start of a stalgmite, with the nearest objective direction **/
	protected Polyline[] getStalagmiteIni(int numStalgm, int stalgmSize, Polyline originPoly, Polyline newPoly, Vector3 objective) {
		InitialPolyline[] result = new InitialPolyline[numStalgm];
		List<int> startIndex = new List<int>(); //Indices of the first vertices used for each polyline
		for (int p = 0; p < numStalgm; ++p) {
			InitialPolyline stalgmPoly = new InitialPolyline (stalgmSize);
			float stalgmAngle = float.MaxValue;
			float auxAngle;
			int finalIndex = -1;
			for (int i = 0; i < originPoly.getSize () - 1; ++i) {
				bool invalidIndex = false;
				int aux = 0;
				//Check it's vertices don't intersect with any of the stalgmites of the same extrusion
				for (int j = 0; j < startIndex.Count && !invalidIndex; ++j) { 
					aux = startIndex [j];
					//Check i is not any of the vertices corresponding to jth estalgmite
					invalidIndex = ( i >= aux && i < aux+stalgmSize/2-1) || ( aux >=i && aux < i + stalgmSize/2-1);
				}
				if (invalidIndex) { 
					i = aux + stalgmSize/2-1; //Jump the next index not corresponding to the intersected stalagmite
					continue;
				}
				//Construct the stalgmite polyline
				InitialPolyline auxPoly = new InitialPolyline (stalgmSize);
				for (int j = (stalgmSize / 2) - 1; j >= 0; --j) {
					auxPoly.addVertex (originPoly.getVertex (i + j));
				}
				for (int j = 0; j < (stalgmSize / 2); ++j) {
					auxPoly.addVertex (newPoly.getVertex (i + j));
				}
				auxAngle = Vector3.Angle (auxPoly.calculateNormal (), objective);
				//Get the one with minimum angle
				if (auxAngle < stalgmAngle) {
					stalgmPoly = auxPoly;
					stalgmAngle = auxAngle;
					finalIndex = i;
				}
			}
			//Add obtained polyline avoid intersection with another one on same extrusion
			startIndex.Add(finalIndex);
			//If angle with objective direction is very high, cancel its creation
			if (stalgmAngle > maxDiffAngle)
				stalgmPoly = null;
			//Add stalgmite initial polyline to result
			result[p] = stalgmPoly;
		}
		return result;
	}
	/**Initializes the first stalagmite polyline and adds it to the mesh **/
	protected void initializeStalagmiteIni(ref Polyline stalgmPoly) {
		InitialPolyline actualStalagmiteIni = new InitialPolyline (stalgmPoly);
		for (int i = 0; i < stalgmPoly.getSize(); ++i) {
			actualStalagmiteIni.getVertex (i).setIndex (stalagmitesMeshes[stalagmitesMeshes.Count-1].getNumVertices () + i);
		}
		stalgmPoly = actualStalagmiteIni;
		stalagmitesMeshes[stalagmitesMeshes.Count-1].addPolyline (stalgmPoly);
	}
	/**Creates an stalagmite/stalagite by the parameters value **/
	protected void extrudeStalagmite(int numExtrusions, Vector3 stalgmDirection, float stalgmExtrusionDistance, float scaleValue, Vector2 UVincr, Polyline actualStalagmite) {
		//It needs to be extruded, scaled, extruded,scaled... until very small polyline is produced
		for (int j = 0; j < numExtrusions; ++j) {
			Polyline newStalgPoly = new Polyline (actualStalagmite.getSize());
			for (int i = 0; i < actualStalagmite.getSize(); ++i) {
				//Add vertex to polyline
				newStalgPoly.extrudeVertex (i, actualStalagmite.getVertex (i).getPosition (), stalgmDirection, stalgmExtrusionDistance);
				//Add the index to vertex
				newStalgPoly.getVertex (i).setIndex (actualStalagmite.getVertex (i).getIndex () + actualStalagmite.getSize());
				//stalgmite UV -> maybe use another mesh for all the stalgmites, with another material
				newStalgPoly.getVertex (i).setUV (actualStalagmite.getVertex (i).getUV () + UVincr);
			}
			newStalgPoly.setMinRadius (0.0f);
			newStalgPoly.scale (scaleValue);
			//Triangulate the new stalgmite part
			stalagmitesMeshes[stalagmitesMeshes.Count-1].addPolyline (newStalgPoly);
			stalagmitesMeshes[stalagmitesMeshes.Count-1].triangulatePolylinesOutside (actualStalagmite, newStalgPoly);
			actualStalagmite = newStalgPoly;
		}
		stalagmitesMeshes[stalagmitesMeshes.Count-1].closePolylineOutside (actualStalagmite);
	}

	/**Creates a point light between the extrusion, on it's center **/
	protected void makePointLight(Polyline originPoly, Polyline destinyPoly) {
		//TODO: Avoid creating and checking light operations
		/*
		GameObject newLight = new GameObject ();
		Vector3 position = (originPoly.calculateBaricenter() + destinyPoly.calculateBaricenter())/2;
		newLight.transform.position = position;
		Light l = newLight.AddComponent<Light> ();
		l.type = LightType.Point;
		//TODO: generate this random before, save it to operation and read it 
		l.intensity = 2.3f;
		l.range = 10.0f;
		newLight.transform.parent =  lights.transform;
		*/
	}
}