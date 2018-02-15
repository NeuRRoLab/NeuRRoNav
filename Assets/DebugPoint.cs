using UnityEngine;
using System.Collections;

public class DebugPoint : MonoBehaviour {

	public PrimitiveType p;
	public Color c;

	void Start() {
		GameObject g = GameObject.CreatePrimitive(p);
		g.name = "Debug Point";
		g.transform.parent = this.transform;
		g.transform.localPosition = Vector3.zero;
		g.transform.localRotation = Quaternion.identity;
		g.transform.localScale = Vector3.one * .01f;
		if (p == PrimitiveType.Sphere)
			g.transform.localScale = Vector3.one * .005f;
		g.GetComponent<MeshRenderer>().material.color = c;
		Destroy(g.GetComponent<Collider>());
	}
}
