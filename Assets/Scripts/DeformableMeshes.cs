using UnityEngine;
using System.Collections;

public class DeformableMeshes : MonoBehaviour {
	private static DeformableMeshes instance = null;
	public static DeformableMeshes Instance {
		get {
			if (instance == null) {
				instance = GameObject.FindGameObjectWithTag("CloudGen").GetComponent<DeformableMeshes>();
			}
			return instance;
		}
	}
	public Mesh[] meshes;
}