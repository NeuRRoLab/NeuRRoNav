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

	Texture2D texttex;
	Texture2D newtex;

	public DICOMImageScr fronttoback;
	public DICOMImageScr righttoleft;
	public DICOMImageScr bottomtotop;

	public UnityEngine.UI.Slider sliderfrontback;
	public UnityEngine.UI.Slider sliderrightleft;
	public UnityEngine.UI.Slider sliderbottomtop;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if ((imgspecs != null) && (imgspecs.initialized)) {
			// Pass the correct textures to the three DicomImageScrs.
			float frontsliderval = sliderfrontback.value;
			float rightsliderval = sliderrightleft.value;
			float bottomsliderval = sliderbottomtop.value;

			var newtexfront = new Texture2D(imgspecs.dicomspace_voxeldim.x, imgspecs.dicomspace_voxeldim.z);
			var newtexright = new Texture2D(imgspecs.dicomspace_voxeldim.y, imgspecs.dicomspace_voxeldim.z);
			var newtexbottom = new Texture2D(imgspecs.dicomspace_voxeldim.x, imgspecs.dicomspace_voxeldim.y);

			int xcoord = Mathf.FloorToInt (rightsliderval * imgspecs.dicomspace_voxeldim.x);
			int ycoord = Mathf.FloorToInt (frontsliderval * imgspecs.dicomspace_voxeldim.y);
			int zcoord = Mathf.FloorToInt (bottomsliderval * imgspecs.dicomspace_voxeldim.z);
				
			// frontback
			for(int z=0;z<imgspecs.dicomspace_voxeldim.z;z++){
				for(int x=0;x<imgspecs.dicomspace_voxeldim.x;x++){
					Vector3Int coord = new Vector3Int (x, ycoord, z);
					float pixlval = imgspecs.dicomspacevoxelarr[(imgspecs.dicomspace_voxeldim.x * imgspecs.dicomspace_voxeldim.y) * coord.z + (coord.y * imgspecs.dicomspace_voxeldim.x) + coord.x]/imgspecs.maxval;
					newtexfront.SetPixel (newtexfront.width-1-x, z, new Color (pixlval, pixlval, pixlval, 1));
				}
			}
			newtexfront.Apply();
			fronttoback.img.texture = newtexfront;

			// rightleft
			for(int z=0;z<imgspecs.dicomspace_voxeldim.z;z++){
				for(int y=0;y<imgspecs.dicomspace_voxeldim.y;y++){
					Vector3Int coord = new Vector3Int (xcoord, y, z);
					float pixlval = imgspecs.dicomspacevoxelarr[(imgspecs.dicomspace_voxeldim.x * imgspecs.dicomspace_voxeldim.y) * coord.z + (coord.y * imgspecs.dicomspace_voxeldim.x) + coord.x]/imgspecs.maxval;
					newtexright.SetPixel (newtexright.width-1-y, z, new Color (pixlval, pixlval, pixlval, 1));
				}
			}
			newtexright.Apply();
			righttoleft.img.texture = newtexright;

			// bottomtop
			for(int y=0;y<imgspecs.dicomspace_voxeldim.y;y++){
				for(int x=0;x<imgspecs.dicomspace_voxeldim.x;x++){
					Vector3Int coord = new Vector3Int (x, y, zcoord);
					float pixlval = imgspecs.dicomspacevoxelarr[(imgspecs.dicomspace_voxeldim.x * imgspecs.dicomspace_voxeldim.y) * coord.z + (coord.y * imgspecs.dicomspace_voxeldim.x) + coord.x]/imgspecs.maxval;
					newtexbottom.SetPixel (newtexfront.width-1-x, y, new Color (pixlval, pixlval, pixlval, 1));
				}
			}
			newtexbottom.Apply();
			bottomtotop.img.texture = newtexbottom;
		}
	}

	public void LoadDICOMFromFolder(){
		imgspecs = new DICOMImgSpecs ();
		imgspecs.InitFromDir(folderloc.text);
		if ((imgspecs != null) && (imgspecs.initialized)) {
			fronttoback.widthscaler = Mathf.Abs (imgspecs.dicomspace_dims.x / imgspecs.dicomspace_dims.z);
			righttoleft.widthscaler = Mathf.Abs (imgspecs.dicomspace_dims.y / imgspecs.dicomspace_dims.z);
			bottomtotop.widthscaler = Mathf.Abs (imgspecs.dicomspace_dims.x / imgspecs.dicomspace_dims.y);
		}
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

	public Vector3Int(Vector3 input){
		x = Mathf.FloorToInt(input.x);
		y = Mathf.FloorToInt(input.y);
		z = Mathf.FloorToInt(input.z);
	}

	public string ToString(){
		return "("+x.ToString()+","+y.ToString()+","+z.ToString()+")";
	}
}

class DICOMImgSpecs{
	public bool initialized = false;
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
	public Vector3Int dicomvoxelspace_bottombackleft = new Vector3Int(0,0,0);
	public Vector3Int dicomvoxelspace_frontforwardright = new Vector3Int(0,0,0);

	public float[] voxelarr;
	public int rows;
	public int cols;
	public int slices;

	public float[] dicomspacevoxelarr;
	public Vector3Int dicomspace_voxeldim;

	public float[] worldspacevoxelarr;
	public int worldspace_xvoxels;
	public int worldspace_yvoxels;
	public int worldspace_zvoxels;

	public float maxval;

	Vector3 dimensionscaling;

	FileInfo[] dicom_files;

	AffineTransformer_IndexOnly affinetransformer;

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
		var lastimage = new DicomImage (dirname + dicom_files [dicom_files.Length - 1].Name);

		string[] imagepospatient = image.Dataset.Get<string[]>(Dicom.DicomTag.ImagePositionPatient);
		string[] imageorientationpatient = image.Dataset.Get<string[]> (Dicom.DicomTag.ImageOrientationPatient);
		string[] nextpospatient = nextimage.Dataset.Get<string[]>(Dicom.DicomTag.ImagePositionPatient);
		string[] lastpospatient = lastimage.Dataset.Get<string[]>(Dicom.DicomTag.ImagePositionPatient);
		string[] pixelspacing = nextimage.Dataset.Get<string[]>(Dicom.DicomTag.PixelSpacing);

		deltar = System.Convert.ToSingle (pixelspacing [0]);
		deltac = System.Convert.ToSingle (pixelspacing [1]);

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

		Vector3 lastpos = new Vector3 (
			System.Convert.ToSingle(lastpospatient[0]),
			System.Convert.ToSingle(lastpospatient[1]),
			System.Convert.ToSingle(lastpospatient[2]));

		slice_dir_cosine = newpos - originpos;

		rows = image.Dataset.Get<int>(Dicom.DicomTag.Rows);
		cols = image.Dataset.Get<int>(Dicom.DicomTag.Columns);
		slices = dicom_files.Length;

		dicomspace_dims = (
			row_dir_cosine * rows+
			col_dir_cosine * cols+
			slice_dir_cosine * slices);	

		Vector3 voxeldim = row_dir_cosine.normalized * rows +
		                   col_dir_cosine.normalized * cols +
		                   slice_dir_cosine.normalized * slices;
		
		voxeldim = new Vector3 (Mathf.Abs(Mathf.Round (voxeldim [0])), 
			Mathf.Abs(Mathf.Round (voxeldim [1])),
			Mathf.Abs(Mathf.Round (voxeldim [2])));

		dimensionscaling = 
			new Vector3 (Mathf.Abs(dicomspace_dims.x/voxeldim.x),
				Mathf.Abs(dicomspace_dims.y/voxeldim.y),
				Mathf.Abs(dicomspace_dims.z/voxeldim.z));

		dicomspace_voxeldim = new Vector3Int (voxeldim);

		InitDelimiterVectors ();

		// bottom back left is important so we have consistent reference frame later.
		affinetransformer = new AffineTransformer_IndexOnly(new Vector3Int(row_dir_cosine.normalized),
			new Vector3Int(col_dir_cosine.normalized),
			new Vector3Int(slice_dir_cosine.normalized));
		
		LoadInPixelData ();

		CreateDicomSpaceVoxelArr ();

		printSpecs ();
		initialized = true;



		Debug.Log("Transform: "+affinetransformer.TransformPoint (new Vector3Int(0,0,0)).ToString());
	
	}

	void InitDelimiterVectors(){
		// min x
		if (dicomspace_dims.x < 0) {
			dicomspace_bottombackleft.x = originpos.x + dicomspace_dims.x;
			dicomvoxelspace_bottombackleft.x = (dicomspace_voxeldim.x-1) * (Mathf.FloorToInt(Mathf.Sign (dicomspace_dims.x)));
		} else {
			dicomspace_bottombackleft.x = originpos.x;
		}

		// min y
		if (dicomspace_dims.y < 0) {
			dicomspace_bottombackleft.y = originpos.y + dicomspace_dims.y;
			dicomvoxelspace_bottombackleft.y = (dicomspace_voxeldim.y-1) * (Mathf.FloorToInt(Mathf.Sign (dicomspace_dims.y)));
		} else {
			dicomspace_bottombackleft.y = originpos.y;
		}

		// min z
		if (dicomspace_dims.z < 0) {
			dicomspace_bottombackleft.z = originpos.z + dicomspace_dims.z;
			dicomvoxelspace_bottombackleft.z = (dicomspace_voxeldim.z-1) * (Mathf.FloorToInt(Mathf.Sign (dicomspace_dims.z)));
		} else {
			dicomspace_bottombackleft.z = originpos.z;
		}


		// max x
		if (dicomspace_dims.x > 0) {
			dicomspace_frontforwardright.x = originpos.x + dicomspace_dims.x;
			dicomvoxelspace_frontforwardright.x = (dicomspace_voxeldim.x-1) * (Mathf.FloorToInt(Mathf.Sign (dicomspace_dims.x)));
		} else {
			dicomspace_frontforwardright.x = originpos.x;
		}

		// max y
		if (dicomspace_dims.y > 0) {
			dicomspace_frontforwardright.y = originpos.y + dicomspace_dims.y;
			dicomvoxelspace_frontforwardright.y = (dicomspace_voxeldim.y-1) * (Mathf.FloorToInt(Mathf.Sign (dicomspace_dims.y)));
		} else {
			dicomspace_frontforwardright.y = originpos.y;
		}

		// max z
		if (dicomspace_dims.z > 0) {
			dicomspace_frontforwardright.z = originpos.z + dicomspace_dims.z;
			dicomvoxelspace_frontforwardright.z = (dicomspace_voxeldim.z-1) * (Mathf.FloorToInt(Mathf.Sign (dicomspace_dims.z)));
		} else {
			dicomspace_frontforwardright.z = originpos.z;
		}
		Debug.Log ("Dicom back:" + dicomvoxelspace_bottombackleft.ToString ());
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
		int failures = 0;
		// What will dimensions of alignedvoxelarr be?
		dicomspacevoxelarr = new float[Mathf.Abs(dicomspace_voxeldim.x * dicomspace_voxeldim.y * dicomspace_voxeldim.z)];

		// Iterate through all points in voxelarr:
		for(int x = 0; x<rows;++x){
			for (int y = 0; y < cols; ++y) {
				for (int z = 0; z < slices; ++z) {
					Vector3Int cur_img_coord = new Vector3Int (x, y, z);
					float cur_img_value = voxelarr [(rows * cols) * z + (y * rows) + x];

					// Which point of the dicomspace arr should be written to?
					Vector3Int transformedpoint = affinetransformer.TransformPoint(cur_img_coord);

					Vector3Int dicomspacecoord = new Vector3Int(
						transformedpoint.x - dicomvoxelspace_bottombackleft.x,
						transformedpoint.y - dicomvoxelspace_bottombackleft.y,
						transformedpoint.z - dicomvoxelspace_bottombackleft.z
					);
						
					try{
						var dicomfloored = dicomspacecoord;
						dicomspacevoxelarr [(dicomspace_voxeldim.x * dicomspace_voxeldim.y) * dicomfloored.z + (dicomfloored.y * dicomspace_voxeldim.x) + dicomfloored.x] = cur_img_value;
						
					}
					catch{
						failures++;

						return;
					}

				}
			}
		}
		if (failures > 0) {
			Debug.LogError ("Hey!!! This many points got mapped out of bounds: "+failures.ToString());
		}
	}

	public bool CheckInBounds(Vector3 transformedpoint){
		// Is it in bounds?
		bool isinbounds = true;

		if (transformedpoint.x < dicomspace_bottombackleft.x) {
			isinbounds = false;
		}

		if (transformedpoint.x > dicomspace_frontforwardright.x) {
			isinbounds = false;
		}

		if (transformedpoint.y < dicomspace_bottombackleft.y) {
			isinbounds = false;
		}

		if (transformedpoint.y > dicomspace_frontforwardright.y) {
			isinbounds = false;
		}

		if (transformedpoint.z < dicomspace_bottombackleft.z) {
			isinbounds = false;
		}

		if (transformedpoint.z > dicomspace_frontforwardright.z) {
			isinbounds = false;
		}

		return isinbounds;
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
		Debug.Log ("DicomSpaceVoxelDims:" + dicomspace_voxeldim.x.ToString() + ","+dicomspace_voxeldim.y.ToString()+","+dicomspace_voxeldim.z.ToString());
	}
}

class AffineTransformer{
	// This is more general purpose than AffineTransformer_IndexOnly. Beware of floating point errors though
	// Source for affine transformation matrix: https://nipy.org/nibabel/dicom/dicom_orientation.html
	float[,] affinematrix; 

	public AffineTransformer(Vector3 rowdircos, Vector3 coldircos, Vector3 firstpos, 
							 Vector3 lastpos, float delta_r, float delta_c, int slices){
		// Init the transformer matrix
		Vector3 f1r = delta_c * rowdircos;//delta_r * coldircos;
		Vector3 f2c = delta_r * coldircos;//delta_c * rowdircos;
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

class AffineTransformer_IndexOnly{
	// Source for affine transformation matrix: https://nipy.org/nibabel/dicom/dicom_orientation.html
	// This should only be used when transforming voxels into different arrangements. Has no concept of scale, only directionality
	int[,] affinematrix; 

	public AffineTransformer_IndexOnly(Vector3Int rowdircos, Vector3Int coldircos, Vector3Int slicedircos){
		// Init the transformer matrix
		Vector3Int f1r = rowdircos;//delta_r * coldircos;
		Vector3Int f2c = coldircos;//delta_c * rowdircos;

		affinematrix = new int[,] { 	{ f1r.x, f2c.x, slicedircos.x, 0},
										{ f1r.y, f2c.y, slicedircos.y, 0}, 
										{ f1r.z, f2c.z, slicedircos.z, 0},
										{ 0, 0, 0, 1 } };;
	}

	public Vector3Int TransformPoint(Vector3Int imgcoord){
		// Takes an imgcoord and spits out coord in space of 
		int[,] coordmatrix = new int[,] {{imgcoord.x},{imgcoord.y},{imgcoord.z},{1}};
		int[,] outputmatrix = new int[,] {{0},{0},{0},{0}};

		// foreach row of matrix
		for (int i = 0; i < 4; ++i) {
			int cur_sum = 0;
			for (int j = 0; j < 4; ++j) {
				cur_sum += affinematrix [i, j] * coordmatrix [j, 0];
			}
			outputmatrix [i,0] = cur_sum;
		}

		return new Vector3Int(outputmatrix[0,0],outputmatrix[1,0],outputmatrix[2,0]);
	}
}
