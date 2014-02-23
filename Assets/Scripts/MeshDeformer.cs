using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshDeformer : MonoBehaviour {
	public Mesh mesh = null;
	public MapShaperWrapper[] heightMapShapers = null;
	//private static MeshSharedVertices storedSharedVertices;

	void Start () {
		if (mesh == null) {
			mesh = gameObject.GetComponent<MeshFilter>().mesh;
		}

		Vector2 arraySize = new Vector2(129, 129);
		float[,] heightMap = new float[(int)arraySize.x, (int)arraySize.y];
		heightMapShapers[0].shaper.SeedRandom(heightMapShapers[0].seed);
		heightMapShapers[0].shaper.InitShaper();
		heightMap = heightMapShapers[0].shaper.ShapeHeightMap(new MapShaperInfo(heightMap, arraySize, heightMapShapers[0].seed, 0, 0, new Vector3(1, 5, 1)));

		Vector3[] vertices = mesh.vertices;
		if (mesh != null) {
			Debug.Log(mesh.vertexCount);
			int checkCount = 0;
			List<List<int>> sharedVertices = new List<List<int>>();
			for (int i = 0; i < mesh.vertices.Length; i++) {
				List<int> sharedList = null;
				for (int j = i + 1; j < mesh.vertices.Length; j++) {
					checkCount++;
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

			for (int i = 0; i < mesh.vertices.Length; i++) {
				if (i < 100)
					Debug.Log (mesh.normals[i]);
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

			MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
			if (meshCollider != null) {
				meshCollider.sharedMesh = mesh;
			}
		}

	}

	/*private class MeshSharedVertices {
		public MeshSharedVertices(Mesh mesh, List<List<int>> sharedVertices) {
			this.mesh = mesh;
			this.sharedVertices = sharedVertices;
		}
		public Mesh mesh;
		public List<List<int>> sharedVertices;
	}*/
}