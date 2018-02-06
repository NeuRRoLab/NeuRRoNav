using UnityEngine;
using System.Collections;

public class Utility {

	// Returns the rotation created by a plane of three points
	// Where the first point is forward and the three points plane normal is upwards
	public static Quaternion ThreePointLocalSpaceConversion(Vector3 a, Vector3 b, Vector3 c) {
		Vector3 center = (a + b + c) / 3;
		Plane plane = new Plane(a, b, c);
		Vector3 planeNormal = plane.normal;
		if (planeNormal.y < 0)
			planeNormal = -planeNormal;

		return Quaternion.LookRotation(
			(a - center).normalized,
			planeNormal
			);

	}
}
