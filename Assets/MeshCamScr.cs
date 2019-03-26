using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MeshCamScr : MonoBehaviour {
	public Transform center;
	public Slider slider;
	public Slider slider2;
	Vector3 origpos;
	public Text deg;
	// Use this for initialization
	void Start () {
		origpos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		
		transform.position = origpos;
		transform.RotateAround (center.position, Vector3.up, slider.value);
		transform.LookAt (center.position);
		transform.RotateAround (center.position, transform.right, slider2.value);
		transform.LookAt (center.position);
	
		deg.text = "";//slider.value.ToString ().Substring(0,3)+","+slider2.value.ToString().Substring(0,3) + " deg";
	}
}
