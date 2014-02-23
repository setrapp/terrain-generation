using UnityEngine;
using System.Collections;

public class GenerateClouds : MonoBehaviour {
	public bool preFindSharedVertices = true;
	public GameObject starterCloud = null;

	void Start() {
		if (preFindSharedVertices) {
			MeshDeformer.FindSharedVerticesOnAll();
		}
	}

	void Update() {
		if (Input.GetButtonDown("Jump")) {
			GameObject.Instantiate(starterCloud);
		}
	}
}
