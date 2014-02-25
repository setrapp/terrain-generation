using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerateClouds : MonoBehaviour {
	public bool preFindSharedVertices = true;
	public GameObject starterCloud = null;
	public GameObject player = null;
	public int seed;
	public int generatedEverCount;
	private List<GameObject> clouds;
	public float generatorHeight;
	public float generatorHeightRange;
	public int cloudsPerCycleMax = 1;
	public int cloudsTotalMax;
	public float minDegenerateDistance;
	public Vector3 scaleMin = new Vector3(1, 1, 1);
	public Vector3 scaleMax = new Vector3(1, 1, 1);
	public Vector3 cloudVelocity;
	public MapShaperWrapper[] heightMapShapers = null;
	public float minDelaySec;
	private System.Random random;
	private float timeSinceTry = 0;
	public float generatorDiameterMin;
	public float generatorDiameterMax;
	public int firstGenerateCount; 

	void Start() {
		random = new System.Random(seed);
		clouds = new List<GameObject>();

		if (preFindSharedVertices) {
			MeshDeformer.FindSharedVerticesOnAll();
		}

		for (int i = 0; i < heightMapShapers.Length; i++) {
				heightMapShapers [i].shaper.SeedRandom(heightMapShapers [i].seed);
				heightMapShapers [i].shaper.InitShaper();
		}

		// Populate the space with some clouds before the player sees anything.
		for (int i = 0; i < firstGenerateCount; i++) {
			GenerateCloud(true);
		}
	}

	void Update() {
		float now = Time.time;
		if (now - timeSinceTry > minDelaySec) {
			int numClouds = (int)(random.NextDouble() * cloudsPerCycleMax) + 1;
			for (int i = 0; i < numClouds; i++) {
				if (clouds.Count >= cloudsTotalMax) {
					DegenerateCloud();
				}
				GenerateCloud();
			}
			timeSinceTry = now;
		}
	}

	private void GenerateCloud(bool firstSpawn = false) {
		// Ensure that max scales are not less that min scales.
		CheckScaleLimits();

		float minDiameter = generatorDiameterMin;
		if (firstSpawn) {
			minDiameter = 0;

		}

		Vector3 newCloudPos = player.transform.position;
		newCloudPos.y = generatorHeight;
		newCloudPos += new Vector3((float)((random.NextDouble() * generatorDiameterMax) - (0.5f * generatorDiameterMax)), 
		                           (float)((random.NextDouble() * generatorHeightRange) - (0.5f * generatorHeightRange)), 
		                           (float)((random.NextDouble() * generatorDiameterMax) - (0.5f * generatorDiameterMax)));

		if ((newCloudPos - player.transform.position).sqrMagnitude < minDiameter) {
			newCloudPos.x += minDiameter;
			newCloudPos.z += minDiameter;
		}

		GameObject newCloud = (GameObject)GameObject.Instantiate(starterCloud, newCloudPos, Quaternion.identity);
		
		newCloud.transform.localScale = new Vector3(scaleMin.x + (float)(random.NextDouble() * (scaleMax.x - scaleMin.x)), 
		                                            scaleMin.y + (float)(random.NextDouble() * (scaleMax.y - scaleMin.y)), 
		                                            scaleMin.z + (float)(random.NextDouble() * (scaleMax.z - scaleMin.z)));
		
		newCloud.GetComponent<MeshDeformer>().DeformMesh(heightMapShapers, seed - generatedEverCount, ((int)random.NextDouble() * (DeformableMeshes.Instance.meshes.Length - 1)));
		newCloud.GetComponent<CloudMover>().velocity = cloudVelocity;

		clouds.Add(newCloud);
		generatedEverCount++;
	}

	private void DegenerateCloud() {
		// Find the furthest cloud.
		float furthestDistance = minDegenerateDistance;
		GameObject furthestCloud = null;
		int furthestIndex = -1;
		for (int i = 0; i < clouds.Count; i++) {	
			float distance = (clouds[i].transform.position - player.transform.position).sqrMagnitude;
			if (distance > furthestDistance) {
				furthestDistance = distance;
				furthestCloud = clouds[i];
				furthestIndex = i;
			}
		}

		// If no cloud is beyond the minimum distance, pick the oldest.
		if (furthestCloud == null) {
			furthestCloud = clouds[0];
			furthestIndex = 0;
		}

		clouds.RemoveAt(furthestIndex);
		GameObject.Destroy(furthestCloud);
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
