using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DICOM_MeshGenerator : MonoBehaviour {
	MeshFilter filter;
	public Transform DICOMMeshObj;
	public DICOM_Manager dmanager;

	public int faces_per_side = 10;
	public int verts_per_side = 0;
	public float scalefactor = 100f;

	public Slider threshslider;

	public float stepdist = 0.5f;
	//public float thresh = 0.5f;

	// Use this for initialization
	void Start () {
		verts_per_side = faces_per_side + 1;
	}
	
	// Update is called once per frame
	void Update () {

	}
	Vector3 ConvertDICOMToUnity(Vector3 dicom, Vector3 unity_dims, Vector3 dicombottom, Vector3 dicomtop){
		dicom = dicom - new Vector3 (dicomtop.x, dicomtop.y, dicombottom.z);
		dicom = dicom / scalefactor;
		var res = new Vector3 (-dicom.x,dicom.z,-dicom.y);
		return new Vector3 (unity_dims.x - res.x, res.y, res.z);


		/*dicom = dicom - new Vector3 (dicombottom.x, dicomtop.y, dicombottom.z);
		dicom = dicom / scalefactor;
		return new Vector3 (-dicom.x,dicom.z,-dicom.y);*/
	}

	Vector3 ConvertUnityToDICOM(Vector3 unity, Vector3 unity_dims, Vector3 dicombottom, Vector3 dicomtop){
		unity = new Vector3 (unity_dims.x - unity.x, unity.y, unity.z);
		unity = new Vector3 (-unity.x, -unity.z, unity.y);
		unity = unity * scalefactor;
		return unity + new Vector3 (dicomtop.x, dicomtop.y, dicombottom.z);

		/*unity = new Vector3 (-unity.x, -unity.z, unity.y);
		unity = unity * scalefactor;
		return unity + new Vector3 (dicombottom.x, dicomtop.y, dicombottom.z);*/

	}

	Vector3 CastRayIntoDICOM(Vector3 startpos_unityspace, Vector3 unity_dims, Vector3 center_dicomspace, Vector3 dicombottom, Vector3 dicomtop,
		DICOMImgSpecs imgspecs, float thresh){

		//Vector3 pos_dicomspace = ConvertUnityToDICOM (startpos_unityspace, dicombottom, dicomtop);
		//return ConvertDICOMToUnity (pos_dicomspace, dicombottom, dicomtop);

		Vector3 pos_dicomspace = ConvertUnityToDICOM (startpos_unityspace, unity_dims, dicombottom, dicomtop);
		Vector3 forward_dicomspace = (center_dicomspace - pos_dicomspace).normalized * stepdist;

		int num_of_steps = Mathf.CeilToInt(Vector3.Distance (pos_dicomspace, center_dicomspace) / stepdist);
		//print (num_of_steps);
		for (int i = 0; i < num_of_steps; ++i) {
			// Get value in dicomspace
			Vector3Int imgcoord = imgspecs.affinetransformer.TransformPointDICOMToImg (pos_dicomspace);
			//print (pos_dicomspace.ToString());
			float pixlval = imgspecs.GetValAtImgCoord(imgcoord);
			// Does it beat threshold
			//print(pixlval);
			if(pixlval>thresh){
				//var converted = ConvertDICOMToUnity (pos_dicomspace, dicombottom, dicomtop);
				//return new Vector3 (unity_dims.x - converted.x, converted.y, converted.z);
				return ConvertDICOMToUnity (pos_dicomspace,unity_dims, dicombottom, dicomtop);
			}
			pos_dicomspace += forward_dicomspace;
		}
		return ConvertDICOMToUnity (pos_dicomspace, unity_dims, dicombottom, dicomtop);

	}

	public void GenerateDICOMHeadMesh(){
		filter = DICOMMeshObj.GetComponent<MeshFilter>();
		filter.mesh.Clear();

		DICOMImgSpecs imgspecs = dmanager.imgspecs;
		Vector3 dicombackleft = imgspecs.dicomspace_bottombackleft;
		Vector3 dicomfrontright = imgspecs.dicomspace_frontforwardright;

		Vector3 dims_dicom = imgspecs.dicomspace_dims/scalefactor;
		Vector3 dims_unity = new Vector3 (dims_dicom.x,dims_dicom.z,dims_dicom.y);
		dims_unity = new Vector3 (Mathf.Abs(dims_unity.x), Mathf.Abs(dims_unity.y), Mathf.Abs(dims_unity.z));

		Vector3 unityup = new Vector3 (0,dims_unity.y,0)/(faces_per_side);
		Vector3 unityforward = new Vector3 (0, 0, dims_unity.z)/(faces_per_side);
		Vector3 unityright = new Vector3 (dims_unity.x, 0, 0)/(faces_per_side);

		int numfaces = 6;

		Vector3[] vertices = new Vector3[(verts_per_side*verts_per_side)*numfaces];
		int[] tris = new int[numfaces * (2 * (verts_per_side - 1) * (verts_per_side - 1)) * 3];
		Vector3[] norms = new Vector3[(verts_per_side*verts_per_side)*numfaces];
			
		int vertincrement = (verts_per_side * verts_per_side);
		int triinc = (2 * (verts_per_side - 1) * (verts_per_side - 1)) * 3;
		int index_verts = 0;
		int index_tris = 0;

		// Create the primitive shape
		// Back face
		FillFace(vertices, tris, norms, index_verts,index_tris,index_verts, verts_per_side, Vector3.zero, 
			unityup, unityright, Vector3.back, false);
		index_verts += vertincrement;
		index_tris += triinc;

		// Left Face
		FillFace(vertices, tris, norms, index_verts,index_tris,index_verts, verts_per_side, Vector3.zero, 
			unityforward, unityup,-Vector3.right, false);
		index_verts += vertincrement;
		index_tris += triinc;

		// Right
		FillFace(vertices, tris, norms, index_verts,index_tris,index_verts, verts_per_side, unityright*(faces_per_side), 
			unityforward, unityup, Vector3.right, true);
		index_verts += vertincrement;
		index_tris += triinc;

		// Forward face
		FillFace(vertices, tris, norms, index_verts,index_tris,index_verts, verts_per_side, unityforward*(faces_per_side), 
			unityup, unityright, Vector3.forward, true);
		index_verts += vertincrement;
		index_tris += triinc;

		// Top face
		FillFace(vertices, tris, norms, index_verts,index_tris,index_verts, verts_per_side, unityup*(faces_per_side), 
			unityforward, unityright, Vector3.up, false);
		index_verts += vertincrement;
		index_tris += triinc;

		// Bottom face
		FillFace(vertices, tris, norms, index_verts,index_tris,index_verts, verts_per_side, Vector3.zero, 
			unityforward, unityright, Vector3.up, true);
		index_verts += vertincrement;
		index_tris += triinc;




		Vector3 center_dicomspace = imgspecs.dicomspace_bottombackleft + (0.5f * imgspecs.dicomspace_dims);
		//Debug.LogError (center_dicomspace.ToString());

		//Debug.Log (ConvertDICOMToUnity (dicomfrontright,dicombackleft, dicomfrontright));
		//Debug.Log (ConvertDICOMToUnity (new Vector3(dicomfrontright.x,dicomfrontright.y,dicombackleft.z),dicombackleft, dicomfrontright));
		//Debug.Log (ConvertDICOMToUnity (dicombackleft,dicombackleft, dicomfrontright));


		float thresh = threshslider.value;

		for (int i=0;i<vertices.Length;++i){
			
			vertices[i] = CastRayIntoDICOM(vertices[i], dims_unity, center_dicomspace, imgspecs.dicomspace_bottombackleft, 
				imgspecs.dicomspace_frontforwardright,imgspecs, thresh);
			
		}

		filter.mesh.vertices = vertices;
		filter.mesh.triangles = tris;
		filter.mesh.normals = norms;

		filter.mesh.RecalculateNormals();
		DICOMMeshObj.transform.localPosition = Vector3.zero;
		DICOMMeshObj.transform.localPosition = transform.localPosition - dims_unity / 2f;

		//DICOMMeshObj.GetComponent<MeshCollider>().sharedMesh = filter.mesh;
	}

	public void FillFace(Vector3[] vertices, int[] tri, Vector3[] norms, 
		int startindex_verts, int startindex_tris, int startindex_norms,
		int verts_per_side, Vector3 startpos, Vector3 dircos1, Vector3 dircos2, Vector3 normvector, bool triangleflipped){

		// Verts!
		for (int xcount = 0; xcount < verts_per_side; xcount += 1)
		{
			for (int ycount = 0; ycount < verts_per_side; ycount += 1)
			{
				Vector3 currentpos = startpos + xcount * dircos1 + ycount * dircos2;
				vertices[startindex_verts + (verts_per_side * ycount) + xcount] = currentpos;
			}
		}

		// Tris!
		// Now tris
		int cur_tri_index = startindex_tris;
		for (int x = 0; x < verts_per_side - 1; x += 1)
		{
			for (int y = 0; y < verts_per_side - 1; y += 1)
			{
				if (triangleflipped) {

					//  Lower left triangle.
					tri [cur_tri_index] = startindex_verts + (verts_per_side * x) + y;
					tri [cur_tri_index + 1] = startindex_verts + (verts_per_side * (x + 1)) + y + 1;
					tri [cur_tri_index + 2] = startindex_verts + (verts_per_side * x) + y + 1;
					//  Upper right triangle.   
					tri [cur_tri_index + 3] = startindex_verts + (verts_per_side * x) + y;
					tri [cur_tri_index + 4] = startindex_verts + (verts_per_side * (x + 1)) + y;
					tri [cur_tri_index + 5] = startindex_verts + (verts_per_side * (x + 1)) + y + 1;
				} else {
					//  Lower left triangle.
					tri [cur_tri_index] = startindex_verts + (verts_per_side * x) + y;
					tri [cur_tri_index + 1] = startindex_verts + (verts_per_side * x) + y + 1;
					tri [cur_tri_index + 2] = startindex_verts + (verts_per_side * (x + 1)) + y + 1;

					//  Upper right triangle.   
					tri [cur_tri_index + 3] = startindex_verts + (verts_per_side * x) + y;
					tri [cur_tri_index + 4] = startindex_verts + (verts_per_side * (x + 1)) + y + 1;
					tri [cur_tri_index + 5] = startindex_verts + (verts_per_side * (x + 1)) + y;
				
				}
				//vertices[(verticeslong * x) + y]
				cur_tri_index += 6;
			}
		}

		// Norms
		for (int i = startindex_norms; i < startindex_norms+(verts_per_side*verts_per_side); ++i)
		{
			norms [i] = normvector;
		}



	}
}
