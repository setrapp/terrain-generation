using UnityEngine;
using System.Collections;

public class SkyFilters : MonoBehaviour {
	public float startingScale;
	public Material material;

	void Start() {
		for (int i = 0; i < transform.childCount; i++) {
			Transform child = transform.GetChild(i);
			Vector3 childScale = new Vector3 (child.localScale.x + startingScale, child.localScale.y + startingScale, child.localScale.z + startingScale);
			child.localScale = childScale;
			child.gameObject.renderer.material = material;
		}
	}
}
