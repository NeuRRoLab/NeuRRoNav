using UnityEngine;
using System.Collections;

public class DICOM_Menu : MonoBehaviour {
	bool hidden;
	Vector3 defaultPos;
	GameObject menu;
	CameraController camController;
	// Use this for initialization
	void Start () {
		menu = GameObject.Find("DICOM_Menu");
		camController = GameObject.Find("Camera Controller").GetComponent<CameraController>();

		hidden = true;
		defaultPos = menu.transform.localPosition;

		Hide();
		Hide();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Hide()
	{
		hidden = !hidden;
		if (hidden)
		{
			menu.transform.localPosition = defaultPos;
			camController.SetListenToMouse(true);
		}
		else
		{
			menu.transform.localPosition = Vector3.zero;
			camController.SetListenToMouse(false);
		}
	}
}
