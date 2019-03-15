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

	// Note: when I say z axis, I mean from back to front of head
	// 		 when I say x axis, I mean from left to right of head
	// 		 when I say y axis, I mean from bottom to top of head.

	public InputField folderloc;

	DICOMImgSpecs imgspecs;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void LoadDICOMFromFolder(){
		imgspecs = new DICOMImgSpecs ();
		imgspecs.InitFromDir(folderloc.text);
	}
}

struct Vector3Int{
	public int x;
	public int y;
	public int z;

	public Vector3Int(int _x,int _y, int _z){
		x = _x;
		y = _y;
		z = _z;
	}
}

class DICOMImgSpecs{
	public Vector3 originpos;

	public Vector3 row_dir_cosine;
	public Vector3 col_dir_cosine;
	public Vector3 slice_dir_cosine;

	public Vector3 dicomspace_dims; // Length width height in millimeters, essentially the 
									// rectangular prism the thing occupies.

	public float[] voxelarr;

	public float maxval;

	public int rows;
	public int cols;
	public int slices;

	FileInfo[] dicom_files;

	public void InitFromDir(string dirname){
		if (dirname[dirname.Length - 1] != '/') {
			dirname = dirname + "/";
		}

		if (!Directory.Exists (dirname)) {
			Debug.LogError ("Could not find directory at specified location");
			return;
		}
			
		// Need to get initial stuff. 
		DirectoryInfo d = new DirectoryInfo(dirname);
		dicom_files = d.GetFiles();

		var image = new DicomImage(dirname+dicom_files[0].Name);
		var nextimage = new DicomImage(dirname+dicom_files[1].Name);

		string[] imagepospatient = image.Dataset.Get<string[]>(Dicom.DicomTag.ImagePositionPatient);
		string[] imageorientationpatient = image.Dataset.Get<string[]> (Dicom.DicomTag.ImageOrientationPatient);
		string[] nextpospatient = nextimage.Dataset.Get<string[]>(Dicom.DicomTag.ImagePositionPatient);

		// Assign Basic Image Vars
		originpos = new Vector3 (
			System.Convert.ToSingle(imagepospatient[0]),
			System.Convert.ToSingle(imagepospatient[1]),
			System.Convert.ToSingle(imagepospatient[2]));

		row_dir_cosine = new Vector3 (
			System.Convert.ToSingle(imageorientationpatient[0]),
			System.Convert.ToSingle(imageorientationpatient[1]),
			System.Convert.ToSingle(imageorientationpatient[2]));

		col_dir_cosine = new Vector3 (
			System.Convert.ToSingle(imageorientationpatient[3]),
			System.Convert.ToSingle(imageorientationpatient[4]),
			System.Convert.ToSingle(imageorientationpatient[5]));

		Vector3 newpos = new Vector3 (
			System.Convert.ToSingle(nextpospatient[0]),
			System.Convert.ToSingle(nextpospatient[1]),
			System.Convert.ToSingle(nextpospatient[2]));

		slice_dir_cosine = newpos - originpos;

		rows = image.Dataset.Get<int>(Dicom.DicomTag.Rows);
		cols = image.Dataset.Get<int>(Dicom.DicomTag.Columns);
		slices = dicom_files.Length;

		dicomspace_dims = (
			row_dir_cosine * rows+
			col_dir_cosine * cols+
			slice_dir_cosine * slices);	

		LoadInPixelData ();

		printSpecs ();
	
	}

	void LoadInPixelData(){
		voxelarr = new float[rows*cols*slices];

		IPixelData pixelData;

		int fileindex = 0;
		foreach (FileInfo file in dicom_files)
		{
			Texture2D newtex = new Texture2D(rows, cols);

			var image = new DicomImage(file.FullName);
			pixelData = PixelDataFactory.Create(image.PixelData, 0);

			for (int y = 0; y < cols; ++y)
			{
				for (int x = 0; x < rows; ++x)
				{
					float cur_pixel = (float)pixelData.GetPixel(x, y);
					voxelarr [(rows * cols) * fileindex + (y * rows) + x] = cur_pixel;
					if (cur_pixel > maxval) {
						maxval = cur_pixel;
					}

				}
			}
			fileindex += 1;
			//Debug.Log ("Maxval: "+maxval.ToString ());
		}

	}

	public void printSpecs(){
		Debug.Log ("Origin:" + originpos.ToString ());
		Debug.Log ("Row:" + row_dir_cosine.ToString ());
		Debug.Log ("Col:" + col_dir_cosine.ToString ());
		Debug.Log ("Slice:" + slice_dir_cosine.ToString ());
		Debug.Log ("Rows:" + rows.ToString ());
		Debug.Log ("Cols:" + cols.ToString ());
		Debug.Log ("Slices:" + slices.ToString ());
		Debug.Log ("DicomSpaceDims:" + dicomspace_dims.ToString ());
	}
}
