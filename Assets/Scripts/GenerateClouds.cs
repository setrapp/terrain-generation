using UnityEngine;
using System.Collections;

public class GenerateClouds : MonoBehaviour {
	public bool preFindSharedVertices = true;
	public GameObject starterCloud = null;
	public int seed;
	public int generatedCount = 0;
	public Vector3 generatorPos;
	public Vector3 generatorRange;
	public Vector3 scaleMin = new Vector3(1, 1, 1);
	public Vector3 scaleMax = new Vector3(1, 1, 1);
	public Vector3 cloudVelocity;
	public MapShaperWrapper[] heightMapShapers = null;
	public float minDelayMilli;
	public float generateChance;
	private System.Random random;

	/*TODO Figure out how to limit the number of clouds, maybe destroy after a certain distance if the player can't see and only create new ones when there is room*/

	void Start() {
		random = new System.Random(seed);

		if (preFindSharedVertices) {
			MeshDeformer.FindSharedVerticesOnAll();
		}

		for (int i = 0; i < heightMapShapers.Length; i++) {
				heightMapShapers [i].shaper.SeedRandom(heightMapShapers [i].seed);
				heightMapShapers [i].shaper.InitShaper();
		}
	}

	void Update() {
		// Ensure that max scales are not less that min scales.
		CheckScaleLimits();

		if (Input.GetButtonDown("Fire2")) {
			Vector3 newCloudPos = new Vector3(generatorPos.x + (float)((random.NextDouble() * generatorRange.x) - (0.5f * generatorRange.x)), 
			                                  generatorPos.y + (float)((random.NextDouble() * generatorRange.y) - (0.5f * generatorRange.y)), 
			                                  generatorPos.z + (float)((random.NextDouble() * generatorRange.z) - (0.5f * generatorRange.z)));
			GameObject newCloud = (GameObject)GameObject.Instantiate(starterCloud, newCloudPos, Quaternion.identity);

			newCloud.transform.localScale = new Vector3(scaleMin.x + (float)(random.NextDouble() * (scaleMax.x - scaleMin.x)), 
			                                            scaleMin.y + (float)(random.NextDouble() * (scaleMax.y - scaleMin.y)), 
			                                            scaleMin.z + (float)(random.NextDouble() * (scaleMax.z - scaleMin.z)));

			newCloud.GetComponent<MeshDeformer>().DeformMesh(heightMapShapers, seed - generatedCount, ((int)random.NextDouble() * (DeformableMeshes.Instance.meshes.Length - 1)));
			newCloud.GetComponent<CloudMover>().velocity = cloudVelocity;
		}
	}

	private void CheckScaleLimits() {
		if (scaleMax.x < scaleMin.x) {
			float tempScale = scaleMax.x;
			scaleMax.x = scaleMin.x;
			scaleMax.x = tempScale;
		}
		if (scaleMax.y < scaleMin.y) {
			float tempScale = scaleMax.y;
			scaleMax.y = scaleMin.y;
			scaleMax.y = tempScale;
		}
		if (scaleMax.z < scaleMin.z) {
			float tempScale = scaleMax.z;
			scaleMax.z = scaleMin.z;
			scaleMax.z = tempScale;
		}
	}
}
