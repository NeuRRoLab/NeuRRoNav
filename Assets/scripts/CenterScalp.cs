using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Makes the center of this gameobject the center of all of the points.
// Must be run for scalp placement to work
public class CenterScalp : MonoBehaviour {

	public List<Transform> points;

	[ContextMenu("Center")]
	public void Center() {
		// Unparent the children to move this accordingly
		Transform[] children = this.transform.GetComponentsInChildren<Transform>();
		foreach (Transform child in children) {
			child.parent = null;
		}

		Vector3 center = Vector3.zero;
		foreach (Transform point in points) {
			center += point.transform.position;
		}
		center /= points.Count;

		this.transform.position = center;
		this.transform.rotation = Utility.ThreePointLocalSpaceConversion(
			points[0].transform.position,
			points[2].transform.position,
			points[1].transform.position);

		foreach (Transform child in children) {
			child.parent = this.transform;
		}
	}
}