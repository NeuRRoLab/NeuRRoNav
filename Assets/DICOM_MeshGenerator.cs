using UnityEngine;
using System.Collections;

public class DICOM_MeshGenerator : MonoBehaviour {
	MeshFilter filter;
	public Transform DICOMMeshObj;
	public DICOM_Manager dmanager;

	public int verts_per_side = 10;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void GenerateDICOMHeadMesh(){
		filter = DICOMMeshObj.GetComponent<MeshFilter>();
		filter.mesh.Clear();

		DICOMImgSpecs imgspecs = dmanager.imgspecs;

		int numfaces = 5;

		Vector3[] vertices = new Vector3[(verts_per_side*verts_per_side)*numfaces];
		int[] tris = new int[numfaces * (2 * (verts_per_side - 1) * (verts_per_side - 1)) * 3];
		Vector3[] norms = new Vector3[(verts_per_side*verts_per_side)*numfaces];
			
		int vertincrement = (verts_per_side * verts_per_side);
		int triinc = (2 * (verts_per_side - 1) * (verts_per_side - 1)) * 3;
		int index_verts = 0;
		int index_tris = 0;

		// Back face
		FillFace(vertices, tris, norms, index_verts,index_tris,index_verts, 10, Vector3.zero, 
			Vector3.up*0.1f, Vector3.right*0.1f, Vector3.back, false);
		index_verts += vertincrement;
		index_tris += triinc;

		// Left Face
		FillFace(vertices, tris, norms, index_verts,index_tris,index_verts, 10, Vector3.zero, 
			Vector3.forward*0.1f, Vector3.up*0.1f,-Vector3.right, false);
		index_verts += vertincrement;
		index_tris += triinc;

		// Right
		FillFace(vertices, tris, norms, index_verts,index_tris,index_verts, 10, Vector3.right*0.9f, 
			Vector3.forward*0.1f, Vector3.up*0.1f, Vector3.right, true);
		index_verts += vertincrement;
		index_tris += triinc;

		// Forward face
		FillFace(vertices, tris, norms, index_verts,index_tris,index_verts, 10, Vector3.forward*0.9f, 
			Vector3.up*0.1f, Vector3.right*0.1f, Vector3.forward, true);
		index_verts += vertincrement;
		index_tris += triinc;

		// Top face
		FillFace(vertices, tris, norms, index_verts,index_tris,index_verts, 10, Vector3.up*0.9f, 
			Vector3.forward*0.1f, Vector3.right*0.1f, Vector3.up, false);
		index_verts += vertincrement;
		index_tris += triinc;

		filter.mesh.vertices = vertices;
		filter.mesh.triangles = tris;
		filter.mesh.normals = norms;

		//filter.mesh.RecalculateNormals();
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
