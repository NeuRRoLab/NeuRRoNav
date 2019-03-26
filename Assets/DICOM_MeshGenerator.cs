using UnityEngine;
using System.Collections;

public class DICOM_MeshGenerator : MonoBehaviour {
	MeshFilter filter;
	public Transform DICOMMeshObj;
	public DICOM_Manager dmanager;

	public int faces_per_side = 10;
	public int verts_per_side = 0;

	// Use this for initialization
	void Start () {
		verts_per_side = faces_per_side + 1;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	Vector3 ConvertDICOMToUnity(Vector3 dicom){
		return new Vector3 (-dicom.x,dicom.z,-dicom.y);
	}

	Vector3 ConvertUnityToDICOM(Vector3 unity){
		return new Vector3 (-unity.x,-unity.y,unity.z);
	}

	public void GenerateDICOMHeadMesh(){
		filter = DICOMMeshObj.GetComponent<MeshFilter>();
		filter.mesh.Clear();

		DICOMImgSpecs imgspecs = dmanager.imgspecs;

		Vector3 dims_dicom = imgspecs.dicomspace_dims/100f;
		Vector3 dims_unity = ConvertDICOMToUnity (dims_dicom);
		dims_unity = new Vector3 (Mathf.Abs(dims_unity.x), Mathf.Abs(dims_unity.y), Mathf.Abs(dims_unity.z));

		Vector3 unityup = new Vector3 (0,dims_unity.y,0)/(faces_per_side);
		Vector3 unityforward = new Vector3 (0, 0, dims_unity.z)/(faces_per_side);
		Vector3 unityright = new Vector3 (dims_unity.x, 0, 0)/(faces_per_side);

		int numfaces = 5;

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


		for (int i=0;i<vertices.Length;++i){
			vertices [i] = vertices [i] + 0.06f*new Vector3 (Random.value, Random.value, Random.value);

		}

		filter.mesh.vertices = vertices;
		filter.mesh.triangles = tris;
		filter.mesh.normals = norms;



		filter.mesh.RecalculateNormals();
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
