using UnityEngine;
using System.Collections;

public class CloudMover : MonoBehaviour {
	public Vector3 velocity;

	void Update() {
		transform.Translate(velocity * Time.deltaTime);
	}
}
