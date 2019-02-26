using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Render;
using System.IO;
using System.Collections.Generic;  

public class DICOM_Manager : MonoBehaviour {
	// Handles the loading and representation of DICOM files.
	public InputField folderloc;

	DicomImage image;
	IPixelData pixelData;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void LoadDICOMFromFolder(){
		string filename = folderloc.text;
		if (!Directory.Exists (filename)) {
			Debug.LogError ("Could not find directory at specified location");
			return;
		}

	}
}
