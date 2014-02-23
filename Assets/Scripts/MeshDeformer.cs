using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshDeformer : MonoBehaviour {
	public int meshIndex;
	public MapShaperWrapper[] heightMapShapers = null;
	private static List<MeshSharedVertices> storedSharedVertices;

	void Start () {
		Vector2 arraySize = new Vector2(129, 129);
		float[,] heightMap = new float[(int)arraySize.x, (int)arraySize.y];
		heightMapShapers[0].shaper.SeedRandom((int)Time.time);//heightMapShapers[0].seed);
		heightMapShapers[0].shaper.InitShaper();
		//heightMap = heightMapShapers[0].shaper.ShapeHeightMap(new MapShaperInfo(heightMap, arraySize, heightMapShapers[0].seed, 0, 0, new Vector3(1, 5, 1)));

		// Temporary
		if (storedSharedVertices == null) {
			storedSharedVertices = new List<MeshSharedVertices>();
		}
		heightMap = heightMapShapers[0].shaper.ShapeHeightMap(new MapShaperInfo(heightMap, arraySize, storedSharedVertices.Count, 0, 0, new Vector3(1, 5, 1)));

		Mesh mesh = (Mesh)Instantiate(DeformableMeshes.Instance.meshes[meshIndex]);

		Vector3[] vertices = mesh.vertices;
		if (mesh != null) {

			List<List<int>> sharedVertices = FindSharedVertices(meshIndex);

			for (int i = 0; i < mesh.vertices.Length; i++) {
				vertices[i] += mesh.normals[i] * heightMap[(int)(mesh.uv[i].x * (arraySize.x - 1)), (int)(mesh.uv[i].y * (arraySize.y - 1))];
			}

			for (int i = 0; i < sharedVertices.Count; i++) {
				Vector3 sharedPos = Vector3.zero;
				for (int j = 0; j < sharedVertices[i].Count; j++) {
					sharedPos += vertices[sharedVertices[i][j]];
				}
				sharedPos /= sharedVertices[i].Count;
				for (int j = 0; j < sharedVertices[i].Count; j++) {
					vertices[sharedVertices[i][j]] = sharedPos;
				}
			}

			mesh.vertices = vertices;
			
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();

			MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
			if (meshFilter != null) {
				meshFilter.mesh = mesh;
			}

			MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
			if (meshCollider != null) {
				meshCollider.sharedMesh = mesh;
			}
		}
	}

	static public List<List<int>> FindSharedVertices(int meshIndex) {
		if (storedSharedVertices == null) {
			storedSharedVertices = new List<MeshSharedVertices>();
		}
		List<List<int>> sharedVertices = null;
		Mesh mesh = DeformableMeshes.Instance.meshes[meshIndex];
		for (int i = 0; i < storedSharedVertices.Count && sharedVertices == null; i++) {
			if (meshIndex == storedSharedVertices[i].meshIndex) {
				sharedVertices = storedSharedVertices[i].sharedVertices;
			}
		}
		if (sharedVertices == null) {
			sharedVertices = new List<List<int>>();
			for (int i = 0; i < mesh.vertices.Length; i++) {
				List<int> sharedList = null;
				for (int j = i + 1; j < mesh.vertices.Length; j++) {
					if (mesh.vertices[i] == mesh.vertices[j]) {
						if (sharedList == null) {
							sharedList = new List<int>();
							sharedVertices.Add(sharedList);
							sharedList.Add(i);
							sharedList.Add(j);
						} else {
							sharedList.Add(j);
						}
					}
				}
			}
			storedSharedVertices.Add(new MeshSharedVertices(meshIndex, sharedVertices));
		}
		return sharedVertices;
	}

	static public void FindSharedVerticesOnAll() {
		for (int i = 0; i < DeformableMeshes.Instance.meshes.Length; i++) {
			FindSharedVertices(i);
		}
	}

	private class MeshSharedVertices {
		public MeshSharedVertices(int meshIndex, List<List<int>> sharedVertices) {
			this.meshIndex = meshIndex;
			this.sharedVertices = sharedVertices;
		}
		public int meshIndex;
		public List<List<int>> sharedVertices;
	}
}