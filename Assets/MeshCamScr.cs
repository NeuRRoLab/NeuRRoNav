using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MeshCamScr : MonoBehaviour {
	public BottomRightScr imagescr;
	public Transform center;
	public Slider slider;
	public Slider slider2;
	public Slider slider3;
	Vector3 origpos;
	public Text deg;
	float initdist;
	Vector3 initvec;
	Vector3 lastMousePos;
	bool dragging = false;
	public float minangle = 5f;
	public float rotspeed = 5f;
	// Use this for initialization
	void Start () {
		origpos = transform.position;
		initdist = Vector3.Distance (transform.position, center.position);
		initvec = transform.position - center.position;
		lastMousePos = Input.mousePosition;
	}
	
	// Update is called once per frame
	void Update () {
		if (imagescr.isOver) {
			if (Input.GetKey (KeyCode.Mouse0)) {
				dragging = true;
			}
			float mouseScroll = Input.mouseScrollDelta.y;

			if (Mathf.Abs(mouseScroll) > 0)
			{
				//cam.transform.LookAt(target.transform.position);
				Vector3 newPos = Vector3.MoveTowards(transform.position, center.position, Mathf.Sign(mouseScroll) * 0.3F);
				if (Vector3.Distance(newPos, center.position) > 0.1F)
				{
					transform.position = newPos;
				}
			}


		}

		if (!Input.GetKey (KeyCode.Mouse0)) {
			dragging = false;
		}

		if (dragging) {
			transform.RotateAround(center.position, Vector3.up, Time.deltaTime*rotspeed*(Input.mousePosition.x - lastMousePos.x));
			Quaternion prevrot = transform.rotation;
			Vector3 prevpos = transform.position;
			transform.RotateAround(center.position, transform.right, -Time.deltaTime*rotspeed*(Input.mousePosition.y - lastMousePos.y));

			if ((Vector3.Angle(Vector3.up, transform.forward) < minangle) || (Vector3.Angle(Vector3.up, transform.forward) >180f-minangle))
			{
				transform.rotation = prevrot;
				transform.position = prevpos;
			}
			transform.LookAt (center.position);



		}

		lastMousePos = Input.mousePosition;

		// Old functionality which used sliders
		/*
		transform.position = center.position;
		transform.position += initvec * slider3.value;

		transform.RotateAround (center.position, Vector3.up, slider.value);
		transform.LookAt (center.position);
		transform.RotateAround (center.position, transform.right, slider2.value);
		transform.LookAt (center.position);
	
		deg.text = "";//slider.value.ToString ().Substring(0,3)+","+slider2.value.ToString().Substring(0,3) + " deg";
		*/
	}


}
