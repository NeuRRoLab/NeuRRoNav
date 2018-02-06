using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Makes the center of this gameobject the center of all of the points.
// Must be run for scalp placement to work
public class CenterScalp : MonoBehaviour {

	public List<Transform> points;

	[ContextMenu("Center")]
	public void Center() {
		Vector3 center = Vector3.zero;
		foreach (Transform point in points) {
			center += point.transform.position;
		}
		center /= points.Count;

		Transform[] children = this.transform.GetComponentsInChildren<Transform>();
		foreach (Transform child in children) {
			child.parent = null;
		}

		this.transform.position = center;

		foreach (Transform child in children) {
			child.parent = this.transform;
		}
	}
}