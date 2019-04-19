﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Render;
using System.IO;
using System.Collections.Generic;  

public class DICOM_Manager : MonoBehaviour {
	// Handles the loading and representation of DICOM files.

	int kernelfrontback; 
	int kernelrightleft; 
	int kernelbottomtop; 

	public ComputeShader dicomslicecreator;

	public static ComputeBuffer voxelbuffer;
	public static ComputeBuffer todicombuffer;
	public static ComputeBuffer toimgbuffer;
	public static ComputeBuffer bottombackbuffer;
	public static ComputeBuffer frontforwardbuffer;
	public static ComputeBuffer dicomspacedims;

	int imagedim = 512;

	public InputField folderloc;

	public DICOMImgSpecs imgspecs;

	Texture2D texttex;
	Texture2D newtex;

	public DICOMImageScr fronttoback;
	public DICOMImageScr righttoleft;
	public DICOMImageScr bottomtotop;

	public Text fronttext;
	public Text righttext;
	public Text bottomtext;

	public UnityEngine.UI.Slider sliderfrontback;
	public UnityEngine.UI.Slider sliderrightleft;
	public UnityEngine.UI.Slider sliderbottomtop;

	float prevfronval = -100;
	float prevrightval = -100;
	float prevbottomval = -100;

	Texture2D newtexfront;
	Texture2D newtexright;
	Texture2D newtexbottom;

	public bool supportsComputeShaders;

	RenderTexture fronttex;
	RenderTexture righttex;
	RenderTexture bottomtex;

	// Use this for initialization
	void Start () {
		supportsComputeShaders = SystemInfo.supportsComputeShaders;

		fronttex = new RenderTexture(imagedim, imagedim, 32);
		fronttex.enableRandomWrite = true;					
		fronttex.Create();								
		fronttex.filterMode = FilterMode.Point;			
		fronttoback.img.texture = fronttex;

		righttex = new RenderTexture(imagedim, imagedim, 32);
		righttex.enableRandomWrite = true;			
		righttex.Create();						
		righttex.filterMode = FilterMode.Point;		
		righttoleft.img.texture = righttex;

		bottomtex = new RenderTexture(imagedim, imagedim, 32);
		bottomtex.enableRandomWrite = true;			
		bottomtex.Create();						
		bottomtex.filterMode = FilterMode.Point;	
		bottomtotop.img.texture = bottomtex;

	}
	
	// Update is called once per frame
	void Update () {
		if ((imgspecs != null) && (imgspecs.initialized)) {
			// Pass the correct textures to the three DicomImageScrs
			float frontsliderval = sliderfrontback.value;
			float rightsliderval = sliderrightleft.value;
			float bottomsliderval = sliderbottomtop.value;

			float xcoord = imgspecs.dicomspace_bottombackleft.x + rightsliderval * imgspecs.dicomspace_dims.x;
			float ycoord = imgspecs.dicomspace_bottombackleft.y + frontsliderval * imgspecs.dicomspace_dims.y;
			float zcoord = imgspecs.dicomspace_bottombackleft.z + bottomsliderval * imgspecs.dicomspace_dims.z;

			if (supportsComputeShaders) {

				// GPU-Offloaded texture updates
				if (frontsliderval != prevfronval) {
	
					dicomslicecreator.SetFloat ("ycoord", ycoord);
					dicomslicecreator.Dispatch (kernelfrontback, imagedim / 16, imagedim / 16, 1);
					fronttext.text = ycoord.ToString () + "mm";
				}

				if (rightsliderval != prevrightval) {
					dicomslicecreator.SetFloat ("xcoord", xcoord);
					dicomslicecreator.Dispatch (kernelrightleft, imagedim / 16, imagedim / 16, 1);
					righttext.text = xcoord.ToString () + "mm";
				}

				if (bottomsliderval != prevbottomval) {
					dicomslicecreator.SetFloat ("zcoord", zcoord);
					dicomslicecreator.Dispatch (kernelbottomtop, imagedim / 16, imagedim / 16, 1);
					bottomtext.text = zcoord.ToString () + "mm";
				}
			}

			// NOTE: Below is legacy functionality for drawing the DICOM textures,
			// which operated on the CPU instead of the GPU, it was very slow due to
			// the extensive matrix multiplications required (one for each pixel)
			/*
			// frontback
			if (frontsliderval != prevfronval) {
				for (int z = 0; z < imagedim; ++z) {
					for (int x = 0; x < imagedim; ++x) {
						float xfrac = (float)x / imagedim; 
						float zfrac = (float)z / imagedim; 
						Vector3 dicomspacecoord = new Vector3 (
							                          imgspecs.dicomspace_bottombackleft.x + xfrac * imgspecs.dicomspace_dims.x,
							                          ycoord,
							                          imgspecs.dicomspace_bottombackleft.z + zfrac * imgspecs.dicomspace_dims.z);

						Vector3Int imgcoord = imgspecs.affinetransformer.TransformPointDICOMToImg (dicomspacecoord);
						float pixlval = imgspecs.GetValAtImgCoord(imgcoord);

						newtexfront.SetPixel (x, z, new Color (pixlval, pixlval, pixlval, 1));
					}
				}
			
				newtexfront.Apply ();
				fronttext.text = ycoord.ToString()+"mm";
			}
				
			// rightleft
			if (rightsliderval != prevrightval) {

				for (int z = 0; z < imagedim; ++z) {
					for (int y = 0; y < imagedim; ++y) {
						float yfrac = (float)y / imagedim; 
						float zfrac = (float)z / imagedim; 
						Vector3 dicomspacecoord = new Vector3 (
							xcoord,
							imgspecs.dicomspace_bottombackleft.y + yfrac * imgspecs.dicomspace_dims.y,
							imgspecs.dicomspace_bottombackleft.z + zfrac * imgspecs.dicomspace_dims.z);

						Vector3Int imgcoord = imgspecs.affinetransformer.TransformPointDICOMToImg (dicomspacecoord);
						float pixlval = imgspecs.GetValAtImgCoord(imgcoord);

						newtexright.SetPixel (y, z, new Color (pixlval, pixlval, pixlval, 1));
					}
				}
	
				newtexright.Apply ();
				righttext.text = xcoord.ToString()+"mm";
			}


			// bottomtop
			if (bottomsliderval != prevbottomval) {
				for (int x = 0; x < imagedim; ++x) {
					for (int y = 0; y < imagedim; ++y) {
						float yfrac = (float)y / imagedim; 
						float xfrac = (float)x / imagedim; 
						Vector3 dicomspacecoord = new Vector3 (
							imgspecs.dicomspace_bottombackleft.x + xfrac * imgspecs.dicomspace_dims.x,
							imgspecs.dicomspace_bottombackleft.y + yfrac * imgspecs.dicomspace_dims.y,
							zcoord);

						Vector3Int imgcoord = imgspecs.affinetransformer.TransformPointDICOMToImg (dicomspacecoord);
						float pixlval = imgspecs.GetValAtImgCoord(imgcoord);

						newtexbottom.SetPixel (x,newtexbottom.height - 1 - y, new Color (pixlval, pixlval, pixlval, 1));
					}
				}


				newtexbottom.Apply ();
				bottomtext.text = zcoord.ToString()+"mm";
			}*/

			prevfronval = frontsliderval;
			prevrightval = rightsliderval;
			prevbottomval = bottomsliderval;
		}
	}

	public void InitComputeShader() {
		if (!supportsComputeShaders) {
			return;
		}

		ReleaseBuffers ();
		
		kernelfrontback = dicomslicecreator.FindKernel("FrontBackTextureCalc");
		kernelrightleft = dicomslicecreator.FindKernel("RightLeftTextureCalc");
		kernelbottomtop = dicomslicecreator.FindKernel("BottomTopTextureCalc");

		// Gotta transfer main float[]
		/*ComputeBuffer */voxelbuffer = new ComputeBuffer(imgspecs.voxelarr.Length, 4);
		voxelbuffer.SetData(imgspecs.voxelarr);
		dicomslicecreator.SetBuffer(kernelfrontback, "voxelarr", voxelbuffer);
		dicomslicecreator.SetBuffer(kernelrightleft, "voxelarr", voxelbuffer);
		dicomslicecreator.SetBuffer(kernelbottomtop, "voxelarr", voxelbuffer);

		// Then both the transformation and inverse transformation matrix
		/*ComputeBuffer */ todicombuffer = new ComputeBuffer(imgspecs.affinetransformer.todicom_flattened.Length, 4);
		todicombuffer.SetData(imgspecs.affinetransformer.todicom_flattened);
		dicomslicecreator.SetBuffer(kernelfrontback, "todicommatrix", todicombuffer);
		dicomslicecreator.SetBuffer(kernelrightleft, "todicommatrix", todicombuffer);
		dicomslicecreator.SetBuffer(kernelbottomtop, "todicommatrix", todicombuffer);

		/*ComputeBuffer */ toimgbuffer = new ComputeBuffer(imgspecs.affinetransformer.toimg_flattened.Length, 4);
		toimgbuffer.SetData(imgspecs.affinetransformer.toimg_flattened);
		dicomslicecreator.SetBuffer(kernelfrontback, "toimgmatrix", toimgbuffer);
		dicomslicecreator.SetBuffer(kernelrightleft, "toimgmatrix", toimgbuffer);
		dicomslicecreator.SetBuffer(kernelbottomtop, "toimgmatrix", toimgbuffer);

		// Then frontback and bottomleft
		float[] bottombackarr = new float[3]{
			imgspecs.dicomspace_bottombackleft.x,
			imgspecs.dicomspace_bottombackleft.y,
			imgspecs.dicomspace_bottombackleft.z,
		};

		/*ComputeBuffer */ bottombackbuffer = new ComputeBuffer(bottombackarr.Length,4);
		bottombackbuffer.SetData (bottombackarr);
		dicomslicecreator.SetBuffer(kernelfrontback, "backbottomleft", bottombackbuffer);
		dicomslicecreator.SetBuffer(kernelrightleft, "backbottomleft", bottombackbuffer);
		dicomslicecreator.SetBuffer(kernelbottomtop, "backbottomleft", bottombackbuffer);

		float[] frontforwardarr = new float[3]{
			imgspecs.dicomspace_frontforwardright.x,
			imgspecs.dicomspace_frontforwardright.y,
			imgspecs.dicomspace_frontforwardright.z,
		};

		/*ComputeBuffer */ frontforwardbuffer = new ComputeBuffer(frontforwardarr.Length,4);
		frontforwardbuffer.SetData (frontforwardarr);
		dicomslicecreator.SetBuffer(kernelfrontback, "frontforwardright", frontforwardbuffer);
		dicomslicecreator.SetBuffer(kernelrightleft, "frontforwardright", frontforwardbuffer);
		dicomslicecreator.SetBuffer(kernelbottomtop, "frontforwardright", frontforwardbuffer);

		// dicomspace_dims
		float[] dicomspacedims_arr = new float[3]{
			imgspecs.dicomspace_dims.x,
			imgspecs.dicomspace_dims.y,
			imgspecs.dicomspace_dims.z,
		};

		/*ComputeBuffer */ dicomspacedims = new ComputeBuffer(dicomspacedims_arr.Length,4);
		dicomspacedims.SetData (dicomspacedims_arr);
		dicomslicecreator.SetBuffer(kernelfrontback, "dicomspace_dims", dicomspacedims);
		dicomslicecreator.SetBuffer(kernelrightleft, "dicomspace_dims", dicomspacedims);
		dicomslicecreator.SetBuffer(kernelbottomtop, "dicomspace_dims", dicomspacedims);

		// Then link the texture
		dicomslicecreator.SetTexture(kernelfrontback, "textureOutFrontBack", fronttex);
		dicomslicecreator.SetTexture(kernelrightleft, "textureOutRightLeft", righttex);
		dicomslicecreator.SetTexture(kernelbottomtop, "textureOutBottomTop", bottomtex);

		// Dim info of voxelarr
		dicomslicecreator.SetInt("rows",imgspecs.rows);
		dicomslicecreator.SetInt("slices",imgspecs.slices);
		dicomslicecreator.SetInt("cols",imgspecs.cols);
		dicomslicecreator.SetFloat("maxval",imgspecs.maxval);


	}

	void ReleaseBuffers(){
		if (voxelbuffer != null) {
			voxelbuffer.Release ();
		}
		if (todicombuffer != null) {
			todicombuffer.Release ();
		}
		if (toimgbuffer != null) {
			toimgbuffer.Release ();
		}
		if (bottombackbuffer != null) {
			bottombackbuffer.Release ();
		}
		if (frontforwardbuffer != null) {
			frontforwardbuffer.Release ();
		}
		if (dicomspacedims != null) {
			dicomspacedims.Release ();
		}
	}

	void OnDestroy(){	
		if (supportsComputeShaders) {
			ReleaseBuffers ();
		}
	}

	public void LoadDICOMFromFolder(){
		imgspecs = new DICOMImgSpecs ();
		imgspecs.InitFromDir(folderloc.text);
		if ((imgspecs != null) && (imgspecs.initialized)) {
			//imagedim = Mathf.CeilToInt(Vector3.Magnitude (new Vector3(imgspecs.rows,imgspecs.cols, imgspecs.slices)));

			fronttoback.widthscaler = Mathf.Abs (imgspecs.dicomspace_dims.x / imgspecs.dicomspace_dims.z);
			righttoleft.widthscaler = Mathf.Abs (imgspecs.dicomspace_dims.y / imgspecs.dicomspace_dims.z);
			bottomtotop.widthscaler = Mathf.Abs (imgspecs.dicomspace_dims.x / imgspecs.dicomspace_dims.y);

			newtexfront = new Texture2D(imagedim, imagedim);
			newtexright = new Texture2D(imagedim, imagedim);
			newtexbottom = new Texture2D(imagedim, imagedim);

			fronttoback.img.material.mainTexture = fronttex;
			righttoleft.img.material.mainTexture = righttex;
			bottomtotop.img.material.mainTexture = bottomtex;

			//fronttoback.img.type = UnityEngine.UI.Image.Type.Simple;

			//fronttoback.img.texture = newtexfront;
			//righttoleft.img.texture = newtexright;
			//bottomtotop.img.texture = newtexbottom;

			prevbottomval = -100;
			prevfronval = -100;
			prevrightval = -100;

			InitComputeShader ();
		}
	}
}

public struct Vector3Int{
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

	public static Vector3Int operator + (Vector3Int v1,  
		Vector3Int v2) { 
		return new Vector3Int (v1.x+v2.x, v1.y+v2.y, v1.z+v2.z);
	} 
}

public class DICOMImgSpecs{
	public bool initialized = false;
	public Vector3 originpos;

	public float deltar;
	public float deltac;

	public Vector3 row_dir_cosine;
	public Vector3 col_dir_cosine;
	public Vector3 slice_dir_cosine;

	public Vector3 dicomspace_dims; // Length width height in millimeters, essentially the 
									// rectangular prism the thing occupies.

	public Vector3 dicomspace_bottombackleft;
	public Vector3 dicomspace_frontforwardright;
	public Vector3Int dicomvoxelspace_bottombackleft = new Vector3Int(0,0,0);
	public Vector3Int dicomvoxelspace_frontforwardright = new Vector3Int(0,0,0);

	public float[] voxelarr;
	public int rows;
	public int cols;
	public int slices;

	//public float[] dicomspacevoxelarr;
	//public Vector3Int dicomspace_voxeldim;

	public float[] worldspacevoxelarr;
	public int worldspace_xvoxels;
	public int worldspace_yvoxels;
	public int worldspace_zvoxels;

	string anatomicalorientationtype;

	public float maxval;

	Vector3 dimensionscaling;

	FileInfo[] dicom_files;

	public AffineTransformer affinetransformer;

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
		try{
			anatomicalorientationtype = nextimage.Dataset.Get<string>(Dicom.DicomTag.AnatomicalOrientationType);
		}
		catch{
			anatomicalorientationtype = "UNSPECIFIED";
		}

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


		// bottom back left is important so we have consistent reference frame later.
		affinetransformer = new AffineTransformer(row_dir_cosine, col_dir_cosine, originpos, 
			lastpos, deltar, deltac, slices);

		InitDelimiterVectors ();
		
		LoadInPixelData ();

		printSpecs ();
		initialized = true;
	
	}

	public float GetValAtImgCoord(Vector3Int imgcoord){
		//  Check if in bounds
		bool inbounds = true;
		if((imgcoord.x<0)||(imgcoord.x>=cols)){
			inbounds = false;
		}
		if((imgcoord.y<0)||(imgcoord.y>=rows)){
			inbounds = false;
		}
		if((imgcoord.z<0)||(imgcoord.z>=slices)){
			inbounds = false;
		}
			
		if (!inbounds) {
			return 0;
		} else {
			return voxelarr [(rows * cols) * imgcoord.z + (imgcoord.y * cols) + imgcoord.x]/maxval;
		}

	}

	public float GetSmoothedValAtDICOMCoord(Vector3 dicomcoord){
		var transformedpoint = affinetransformer.TransformPointDICOMToImg_Unfloored (dicomcoord);
		//print (pos_dicomspace.ToString());
		//transformedpoint = new Vector3(Mathf.Floor(transformedpoint.x),Mathf.Floor(transformedpoint.y),transformedpoint.z);

		float frac = transformedpoint.z - Mathf.Floor (transformedpoint.z); // weight to give higher 

		float invfrac = 1f - frac; // weight to give lower

		Vector3Int flatvec = new Vector3Int (transformedpoint);

		return invfrac * GetValAtImgCoord(flatvec) + 
			frac*GetValAtImgCoord(flatvec + new Vector3Int(0,0,1));
	}

	void InitDelimiterVectors(){

		dicomspace_bottombackleft = affinetransformer.TransformPointImgToDICOM (new Vector3Int (0, 0, 0));
		dicomspace_frontforwardright = affinetransformer.TransformPointImgToDICOM (new Vector3Int (0, 0, 0));

		Vector3 slicevec = slice_dir_cosine * slices;
		Vector3 rowvec = col_dir_cosine * rows;
		Vector3 colvec = row_dir_cosine * cols;

		List<Vector3> extremitypoints = new List<Vector3> ();

		extremitypoints.Add (affinetransformer.TransformPointImgToDICOM(new Vector3Int(0,0,0)));
		extremitypoints.Add (affinetransformer.TransformPointImgToDICOM(new Vector3Int(cols,0,0)));
		extremitypoints.Add (affinetransformer.TransformPointImgToDICOM(new Vector3Int(0,rows,0)));
		extremitypoints.Add (affinetransformer.TransformPointImgToDICOM(new Vector3Int(cols,rows,0)));

		extremitypoints.Add (affinetransformer.TransformPointImgToDICOM(new Vector3Int(0,0,slices)));
		extremitypoints.Add (affinetransformer.TransformPointImgToDICOM(new Vector3Int(cols,0,slices)));
		extremitypoints.Add (affinetransformer.TransformPointImgToDICOM(new Vector3Int(0,rows,slices)));
		extremitypoints.Add (affinetransformer.TransformPointImgToDICOM(new Vector3Int(cols,rows,slices)));

		// Now find max x and y and z in each.
		foreach (Vector3 vec in extremitypoints){
			// Bottombackleft
			if(vec.x<dicomspace_bottombackleft.x){
				dicomspace_bottombackleft.x = vec.x;
			}
			if(vec.y<dicomspace_bottombackleft.y){
				dicomspace_bottombackleft.y = vec.y;
			}
			if(vec.z<dicomspace_bottombackleft.z){
				dicomspace_bottombackleft.z = vec.z;
			}

			// frontforwardright
			if(vec.x>dicomspace_frontforwardright.x){
				dicomspace_frontforwardright.x = vec.x;
			}
			if(vec.y>dicomspace_frontforwardright.y){
				dicomspace_frontforwardright.y = vec.y;
			}
			if(vec.z>dicomspace_frontforwardright.z){
				dicomspace_frontforwardright.z = vec.z;
			}
		}

		dicomspace_dims = dicomspace_frontforwardright - dicomspace_bottombackleft;
	}

	void LoadInPixelData(){
		voxelarr = new float[rows*cols*slices];

		IPixelData pixelData;

		int fileindex = 0;
		foreach (FileInfo file in dicom_files)
		{
			Texture2D newtex = new Texture2D(cols, rows);

			var image = new DicomImage(file.FullName);
			pixelData = PixelDataFactory.Create(image.PixelData, 0);

			for (int y = 0; y < rows; ++y)
			{
				for (int x = 0; x < cols; ++x)
				{
					float cur_pixel = (float)pixelData.GetPixel(x, y);
					voxelarr [(rows * cols) * fileindex + (y * cols) + x] = cur_pixel;
					if (cur_pixel > maxval) {
						maxval = cur_pixel;
					}
				}
			}
			fileindex += 1;
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
		Debug.Log ("Bottombackleft" + dicomspace_bottombackleft.ToString ());
		Debug.Log ("Frontforwardright: " + dicomspace_frontforwardright.ToString ());
		Debug.Log ("DicomSpaceDims:" + dicomspace_dims.ToString ());
		Debug.Log ("AnatomicalOrientationType:" + anatomicalorientationtype);

	}
}

public class AffineTransformer{
	// This is more general purpose than AffineTransformer_IndexOnly.
	// Source for affine transformation matrix: https://nipy.org/nibabel/dicom/dicom_orientation.html
	public float[,] affinematrix_todicom; 
	public float[,] affinematrix_toimg; 

	public float[] todicom_flattened;
	public float[] toimg_flattened;

	public AffineTransformer(Vector3 rowdircos, Vector3 coldircos, Vector3 firstpos, 
							 Vector3 lastpos, float delta_r, float delta_c, int slices){
		// Init the transformer matrix
		Vector3 f1r = delta_r * rowdircos;
		Vector3 f2c = delta_c * coldircos;
		Vector3 t1 = firstpos;
		Vector3 tn = lastpos;
		float n = slices;
		Vector3 tnscaled = (firstpos - lastpos);
		tnscaled = tnscaled * (1.0f / (1.0f - n));

		affinematrix_todicom = new float[,] { 	{ f1r.x, f2c.x, tnscaled.x, t1.x},
												{ f1r.y, f2c.y, tnscaled.y, t1.y}, 
												{ f1r.z, f2c.z, tnscaled.z, t1.z},
												{ 0, 0, 0, 1 } };

		// Behold the horror - manually calculated inverse of affinematrix_todicom
		var a = f1r.x;
		var b = f2c.x;
		var z = tnscaled.x;
		var d = t1.x;
		var e = f1r.y;
		var f = f2c.y;
		var g = tnscaled.y;
		var h = t1.y;
		var i = f1r.z;
		var j = f2c.z;
		var k = tnscaled.z;
		var y = t1.z;

		float denom = (b * g * i) - (f * z * i) - (a * g * j) + (a * f * k) - (b * e * k) + (e * j * z);

		affinematrix_toimg = new float[,] { 	
			{ ((f*k) - (g*j)), ((j*z) - (b*k)), ((b*g) - (f*z)), ((d*g*j)-(h*z*j)-(d*f*k)+(b*h*k)-(b*g*y)+(f*y*z))},
			{ ((g*i) - (e*k)), ((a*k) - (i*z)), ((e*z) - (a*g)), ((-d*g*i)+(h*z*i)-(a*h*k)+(d*e*k)+(a*g*y)-(e*y*z))}, 
			{ ((e*j) - (f*i)), ((b*i) - (a*j)), ((a*f) - (b*e)), ((d*f*i)-(b*h*i)+(a*h*j)-(d*e*j)-(a*f*y)+(b*e*y))},
			{ 0, 0, 0, 1 } };;

		// Need to divide by some nasty denom
		for(int row=0;row<3;++row){
			for (int col = 0; col < 4; ++col) {
				affinematrix_toimg [row,col] = affinematrix_toimg [row,col] / denom;
			}
		}

		// Create flattened arr, rows are fastest
		todicom_flattened = new float[16];

		for (int row=0;row<4;++row){
			for(int col=0;col<4;col++){
				todicom_flattened [(4 * col) + row] = affinematrix_todicom [row, col];
			}
		}

		// Create flattened arr, rows are fastest
		toimg_flattened = new float[16];

		for (int row=0;row<4;++row){
			for(int col=0;col<4;++col){
				toimg_flattened [(4 * row) + col] = affinematrix_toimg [row, col];
			}
		}
	}

	public Vector3 TransformPointImgToDICOM(Vector3Int imgcoord){
		// Takes an imgcoord and spits out coord in space of 
		float[,] coordmatrix = new float[,] {{(float)imgcoord.x},{(float)imgcoord.y},{(float)imgcoord.z},{1}};
		float[,] outputmatrix = new float[,] {{0},{0},{0},{0}};

		// foreach row of matrix
		for (int i = 0; i < 4; ++i) {
			float cur_sum = 0;
			for (int j = 0; j < 4; ++j) {
				cur_sum += affinematrix_todicom [i, j] * coordmatrix [j, 0];
			}
			outputmatrix [i,0] = cur_sum;
		}

		return new Vector3(outputmatrix[0,0],outputmatrix[1,0],outputmatrix[2,0]);
	}

	public Vector3Int TransformPointDICOMToImg(Vector3 dicomcoord){
		// Takes an imgcoord and spits out coord in space of 
		float[,] coordmatrix = new float[,] {{(float)dicomcoord.x},{(float)dicomcoord.y},{(float)dicomcoord.z},{1}};
		float[,] outputmatrix = new float[,] {{0},{0},{0},{0}};

		// foreach row of matrix
		for (int i = 0; i < 4; ++i) {
			float cur_sum = 0;
			for (int j = 0; j < 4; ++j) {
				cur_sum += affinematrix_toimg [i, j] * coordmatrix [j, 0];
			}
			outputmatrix [i,0] = cur_sum;
		}

		return new Vector3Int(new Vector3(outputmatrix[0,0],outputmatrix[1,0],outputmatrix[2,0]));
	}

	public Vector3 TransformPointDICOMToImg_Unfloored(Vector3 dicomcoord){

		// This is useful for when interpolating...

		// Takes an imgcoord and spits out coord in space of 
		float[,] coordmatrix = new float[,] {{(float)dicomcoord.x},{(float)dicomcoord.y},{(float)dicomcoord.z},{1}};
		float[,] outputmatrix = new float[,] {{0},{0},{0},{0}};

		// foreach row of matrix
		for (int i = 0; i < 4; ++i) {
			float cur_sum = 0;
			for (int j = 0; j < 4; ++j) {
				cur_sum += affinematrix_toimg [i, j] * coordmatrix [j, 0];
			}
			outputmatrix [i,0] = cur_sum;
		}

		return new Vector3(outputmatrix[0,0],outputmatrix[1,0],outputmatrix[2,0]);
	}
}

