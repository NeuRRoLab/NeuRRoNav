using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TransformSmoother {

	public static int maxSaves = 20;
	List<Vector3> savedPositions = new List<Vector3>();
	List<Quaternion> savedRotations = new List<Quaternion>();

	public void AddTransform(Vector3 point, Quaternion rotation) {
		savedPositions.Add(point);
		while (savedPositions.Count > maxSaves)
			savedPositions.RemoveAt(0);

		savedRotations.Add(rotation);
		while (savedRotations.Count > maxSaves)
			savedRotations.RemoveAt(0);
	}

	public Vector3 GetAveragePosition() {
		//return savedPositions[0];

		if (savedPositions.Count == 0) {
			return Vector3.zero;
		}
		Vector3 average = Vector3.zero;
		foreach (Vector3 position in savedPositions) {
			average += position;
		}
		average /= savedPositions.Count;
		return average;
	}

	public Quaternion GetAverageRotation() {
		//return savedRotations[0];

		if (savedRotations.Count == 0)
			return Quaternion.identity;
		else if (savedRotations.Count == 1)
			return savedRotations[0];
		return AverageQuaternion(savedRotations);
	}

	Quaternion AverageQuaternion(List<Quaternion> quaternions) {
		Quaternion qAvg = quaternions[0];
		float weight;
		for (int i = 1; i < quaternions.Count; i++) {
			weight = 1.0f / (float)(i + 1);
			qAvg = Quaternion.Slerp(qAvg, quaternions[i], weight);
		}
		return qAvg;
	}
}
