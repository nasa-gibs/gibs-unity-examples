using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This script should be attached to the sphere being used as the Earth
public class DataCurtain : MonoBehaviour {

	[Tooltip("This value is multiplied by the radius to determine the height of the data curtain")]
	public float heightFactor = 0.2f; // height of curtain = radius * height factor
	private const int X_VERTICES = 20; // number of vertices in the horizontal component of the mesh
	private const int Y_VERTICES = 20; // number of vertices in the vertical component of the mesh
	private float radius, height;
	private List<GameObject> curtains = new List<GameObject>(); // list of all our data curtain segments
	private List<Material> materials = new List<Material>(); // list of all materials we have created to ensure we destroy them all at the end.

	private void Awake()
	{
		radius = transform.localScale.x / 2;
		height = radius * heightFactor;
	}

	public void Generate (float startLat, float startLong, float endLat, float endLong, Texture profileImg) {

		// Create new gameObject, make it a child of the sphere
		GameObject profile = new GameObject("Vertical Profile " + curtains.Count);
		profile.transform.parent = gameObject.transform;

		// Add a MeshFilter and MeshRenderer to our new vertical profile GameObject
		profile.AddComponent(typeof(MeshFilter));
		profile.AddComponent(typeof(MeshRenderer));

		Mesh mesh = new Mesh();
		profile.GetComponent<MeshFilter>().mesh = mesh;

		mesh.name = "Data Curtain " + curtains.Count;

		Vector3[] vertices = new Vector3[(X_VERTICES + 1) * (Y_VERTICES + 1)];
		Vector2[] uv = new Vector2[vertices.Length];

		// Calculate surface start coords and end coords
		Vector3 startCoords = Coordinates.LatLongToXYZ(startLat, startLong, radius);
		Vector3 endCoords = Coordinates.LatLongToXYZ(endLat, endLong, radius);

		// In XYZ world space, this is the amount we need to increment X_VERTICES times
		// to get from the start coordinates to the end coordinates.
		Vector3 xIncrement = (endCoords - startCoords) / (float) X_VERTICES;
		
		// Same thing but going from the surface of the Earth to the top of the curtain
		float yIncrement = height / (float) Y_VERTICES;

		// Add all the vertices
		for (int i = 0, x = 0; x <= X_VERTICES; x++) {
			// Convert to lat and long to find where we are in geo coordinates.
			// Need this because we're gonna be underground as it is now without accounting for the Earth's curve.
			Vector3 currLatLong = Coordinates.XYZToLatLong(startCoords + x * xIncrement);
			for (int y = 0; y <= Y_VERTICES; y++, i++) {
				// Convert back to XYZ coordinates, but with the correct radius.
				// This should put us on or above the Earth's surface.
				vertices[i] = Coordinates.LatLongToXYZ(currLatLong.x, currLatLong.y, radius + y * yIncrement);
				// Assign the mesh's UVs
				uv[i] = new Vector2((float)x / X_VERTICES, (float)y / Y_VERTICES);
			}
		}

		mesh.vertices = vertices;
		mesh.uv = uv;

		// Add the triangles to the mesh
		int[] triangles = new int[X_VERTICES * Y_VERTICES * 6];
		for (int ti = 0, vi = 0, y = 0; y < Y_VERTICES; y++, vi++) {
			for (int x = 0; x < X_VERTICES; x++, ti += 6, vi++) {
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + X_VERTICES + 1;
				triangles[ti + 5] = vi + X_VERTICES + 2;
			}
		}
		mesh.triangles = triangles;
		mesh.RecalculateNormals();

		// "Mobile/Particles/Alpha Blended" allows the image to be viewed from both sides
		Material profileMat = new Material(Shader.Find("Mobile/Particles/Alpha Blended"));
		
		// Set our profile image as the texture
		profileMat.mainTexture = profileImg;

		// Assign our new material to our vertical profile
		profile.GetComponent<MeshRenderer>().material = profileMat;

		Debug.Log(profile.GetComponent<MeshRenderer>().material);

		// Add our new material to our list of materials we need to destroy at the end
		materials.Add(profileMat);

		// Add our new vertical profile gameObject to our list of curtains
		curtains.Add(profile);
	}

	// Clean up our materials
	void OnDisable()
	{
		int matCount = materials.Count;
		// Destroy all the materials we created
		for (int i = 0; i < matCount; i ++)
		{
			Material condemned = materials[0];
			materials.RemoveAt(0);
			Destroy(condemned);
		}
	}
}
