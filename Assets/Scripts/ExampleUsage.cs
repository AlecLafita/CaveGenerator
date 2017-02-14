using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleUsage : MonoBehaviour {

	//https://www.youtube.com/watch?v=8LTDFwWMlqQ&feature=youtu.be
	//http://wiki.unity3d.com/index.php/ProceduralPrimitives
	//http://kobolds-keep.net/?p=33

	private Mesh mesh;

	//Components needed
		//Mesh filter-> stores mesh information
		//Mesh renderer-> renders mesh filter
		//Script-> generates mesh,etc

	// Use this for initialization
	void Start () {
		mesh = new Mesh ();

		/*****Vertices coords***/
		//vertices the mesh contains
		Vector3[] vertices =  new Vector3[3];
		vertices[0] = new Vector3 (0.0f, 0.0f, 0.0f);
		vertices [1] = new Vector3 (0.0f, 2.0f, 0.0f);
		vertices [2] = new Vector3 (3.0f, 0.0f, 0.0f);
		mesh.vertices = vertices;

		/******TRIANGLES****/
		//assign triangles indices, each three numbers indicates the vertices forming a triangle (thus needs to be multiple of 3)
		//The indices correspond to mesh.vertices
		int[] triangles = new int[3] {0,1,2};
		mesh.triangles = triangles;

		/*****NORMALS*****/
		//Normals per vertex(indices must correspond)
		Vector3[] normals = new Vector3[3];
		normals [0] = Vector3.back;	normals [1] = Vector3.back;	normals [2] = Vector3.back;

		mesh.normals = normals;//Empty by default
		//There won't have any effect setting the normals if the cross-product of the vertices while creating the triangle is not facing
		//the camera. The normals are only taken on account for illumantion/shaders stuff


		/******UVS********/
		//Texture coordinate per vertex [0,1]
		//Must be set the texture as the mesh renderer material
		Vector2[] uvs = new Vector2[3];
		uvs [0] = new Vector2 (0.0f, 0.0f);
		uvs [1] = new Vector2 (0.0f, 1.0f);
		uvs [2] = new Vector2 (1.0f, 0.0f);
		mesh.uv = uvs;

		//Assign the created mesh to the one we are storing and visualizing
		GetComponent<MeshFilter> ().mesh = mesh;
	}
	
	// Update is called once per frame
	void Update () {
		/*****COLORS*****/
		/*
		//We can also change the color per vertex instead of using a material (usefull for particles systems) -> reccomended use Color32
		float speedColor = 500.0f;
		Color[] colors = new Color[3];
		float offset = Time.deltaTime * speedColor;
		colors [0] = new Color (offset, 0.0f, 0.0f);
		colors [1] = new Color (0.0f, offset, 0.0f);
		colors [2] = new Color ( 0.0f, 0.0f, offset);

		mesh.colors = colors; //Needs to use a shader that uses vertex color(material)

		GetComponent<MeshFilter> ().mesh = mesh;*/

	}
}
