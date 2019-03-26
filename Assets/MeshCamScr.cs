using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MeshCamScr : MonoBehaviour {
	public Transform center;
	public Slider slider;
	public Slider slider2;
	public Slider slider3;
	Vector3 origpos;
	public Text deg;
	float initdist;
	Vector3 initvec;
	// Use this for initialization
	void Start () {
		origpos = transform.position;
		initdist = Vector3.Distance (transform.position, center.position);
		initvec = transform.position - center.position;
	}
	
	// Update is called once per frame
	void Update () {

		transform.position = center.position;
		transform.position += initvec * slider3.value;

		transform.RotateAround (center.position, Vector3.up, slider.value);
		transform.LookAt (center.position);
		transform.RotateAround (center.position, transform.right, slider2.value);
		transform.LookAt (center.position);


	
		deg.text = "";//slider.value.ToString ().Substring(0,3)+","+slider2.value.ToString().Substring(0,3) + " deg";
	}
}
