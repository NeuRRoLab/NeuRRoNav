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
		/*
		string dirname = folderloc.text;
		if (!Directory.Exists (dirname)) {
			Debug.LogError ("Could not find directory at specified location");
			return;
		}

		if (dirname[dirname.Length - 1] != '/') {
			dirname = dirname + "/";
		}

		DirectoryInfo d = new DirectoryInfo(dirname);
		FileInfo[] Files = d.GetFiles();

		Debug.Log(dirname+Files[0].Name);
		image = new DicomImage(dirname+Files[0].Name);*/

		//var PatientName= image.Dataset.Get<string>(Dicom.DicomTag.PatientName);

		//Debug.Log(PatientName);
		      
		string thispath = "C:/Users/Daniel/Downloads/MR_Head_DICOM/ScalarVolume_10/";
		DirectoryInfo d = new DirectoryInfo(thispath);//Assuming Test is your Folder
		FileInfo[] Files = d.GetFiles(); //Getting Text files

		image = new DicomImage(thispath + Files[0].Name);

	}
}
