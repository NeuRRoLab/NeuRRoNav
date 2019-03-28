using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DICOMImageScr : MonoBehaviour {
	public RawImage img;
	RectTransform rt;
	public float widthscaler = 1;

	// Use this for initialization
	void Start () {
		rt = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
		rt.sizeDelta = new Vector2(rt.sizeDelta.y*widthscaler, rt.sizeDelta.y);
	}
}
