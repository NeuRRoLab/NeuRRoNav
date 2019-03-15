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

	public float deltar;
	public float deltac;

	public Vector3 row_dir_cosine;
	public Vector3 col_dir_cosine;
	public Vector3 slice_dir_cosine;

	public Vector3 dicomspace_dims; // Length width height in millimeters, essentially the 
									// rectangular prism the thing occupies.\

	public Vector3 dicomspace_bottombackleft;
	public Vector3 dicomspace_frontforwardright;

	public float[] voxelarr;
	public int rows;
	public int cols;
	public int slices;

	public float[] dicomspacevoxelarr;
	public int dicomspace_xvoxels;
	public int dicomspace_yvoxels;
	public int dicomspace_zvoxels;

	public float[] worldspacevoxelarr;
	public int worldspace_xvoxels;
	public int worldspace_yvoxels;
	public int worldspace_zvoxels;

	public float maxval;

	FileInfo[] dicom_files;

	AffineTransformer affinetransformer;

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
		string[] pixelspacing = nextimage.Dataset.Get<string[]>(Dicom.DicomTag.PixelSpacing);

		deltar = System.Convert.ToSingle (pixelspacing [0]);
		deltac = System.Convert.ToSingle (pixelspacing [1]);

		Debug.Log ("PixelSpacing:");
		foreach (string s in pixelspacing){
			Debug.Log (s);
		}
		Debug.Log ("End PixelSpacing");

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

		// max x
		if (dicomspace_dims.x > 0) {
			dicomspace_bottombackleft.x = originpos.x + dicomspace_dims.x;
		} else {
			dicomspace_bottombackleft.x = originpos.x;
		}

		// max y
		if (dicomspace_dims.y > 0) {
			dicomspace_bottombackleft.y = originpos.y + dicomspace_dims.y;
		} else {
			dicomspace_bottombackleft.y = originpos.y;
		}

		// min z
		if (dicomspace_dims.z < 0) {
			dicomspace_bottombackleft.z = originpos.z + dicomspace_dims.z;
		} else {
			dicomspace_bottombackleft.z = originpos.z;
		}


		// min x
		if (dicomspace_dims.x < 0) {
			dicomspace_frontforwardright.x = originpos.x + dicomspace_dims.x;
		} else {
			dicomspace_frontforwardright.x = originpos.x;
		}

		// min y
		if (dicomspace_dims.y < 0) {
			dicomspace_frontforwardright.y = originpos.y + dicomspace_dims.y;
		} else {
			dicomspace_frontforwardright.y = originpos.y;
		}

		// max z
		if (dicomspace_dims.z > 0) {
			dicomspace_frontforwardright.z = originpos.z + dicomspace_dims.z;
		} else {
			dicomspace_frontforwardright.z = originpos.z;
		}

		// bottom back left is important so we have consistent reference frame later.
		affinetransformer = new AffineTransformer(row_dir_cosine, col_dir_cosine, originpos, 
								newpos, deltar, deltac, 2);
		
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

	public void CreateDicomSpaceVoxelArr(){


		// What will dimensions of alignedvoxelarr be?

		// Iterate through all points in voxelarr:
		for(int x = 0; x<rows;++x){
			for (int y = 0; y < rows; ++y) {
				for (int z = 0; z < rows; ++z) {
					Vector3Int cur_img_coord = new Vector3Int (x, y, z);
					float cur_img_value = voxelarr [(rows * cols) * z + (y * rows) + x];

					// Which point of the dicomspace arr should be written to?
					Vector3 transformedpoint = affinetransformer.TransformPoint(cur_img_coord);


				}
			}
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

class AffineTransformer{
	// Source for affine transformation matrix: https://nipy.org/nibabel/dicom/dicom_orientation.html
	float[,] affinematrix; 

	public AffineTransformer(Vector3 rowdircos, Vector3 coldircos, Vector3 firstpos, 
							 Vector3 lastpos, float delta_r, float delta_c, int slices){
		// Init the transformer matrix
		Vector3 f1r = delta_r * coldircos;
		Vector3 f2c = delta_c * rowdircos;
		Vector3 t1 = firstpos;
		Vector3 tn = lastpos;
		float n = slices;
		Vector3 tnscaled = (firstpos - lastpos);
		tnscaled = tnscaled * (1.0f / (1.0f - n));

		affinematrix = new float[,] { 	{ f1r.x, f2c.x, tnscaled.x, t1.x},
										{ f1r.y, f2c.y, tnscaled.y, t1.y}, 
										{ f1r.z, f2c.z, tnscaled.z, t1.z},
										{ 0, 0, 0, 1 } };;
	}

	public Vector3 TransformPoint(Vector3Int imgcoord){
		// Takes an imgcoord and spits out coord in space of 
		float[,] coordmatrix = new float[,] {{(float)imgcoord.x},{(float)imgcoord.y},{(float)imgcoord.z},{1}};
		float[,] outputmatrix = new float[,] {{0},{0},{0},{0}};

		// foreach row of matrix
		for (int i = 0; i < 4; ++i) {
			float cur_sum = 0;
			for (int j = 0; j < 4; ++j) {
				cur_sum += affinematrix [i, j] * coordmatrix [j, 0];
			}
			outputmatrix [i,0] = cur_sum;
		}

		return new Vector3(outputmatrix[0,0],outputmatrix[1,0],outputmatrix[2,0]);
	}
}
