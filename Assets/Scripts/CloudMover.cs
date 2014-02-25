using UnityEngine;
using System.Collections;

public class CloudMover : MonoBehaviour {
	public Vector3 velocity;

	void FixedUpdate() {
		transform.Translate(velocity * Time.fixedDeltaTime);
	}
}