using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScalpMeshMenuController : MonoBehaviour {
    bool isactive = false;
    Vector3 inactivepos;
    Vector3 activepos;
    RectTransform myrect;
    Vector3[] vertices;
    int verticeslong;
    int verticeswide;
    public string STATE = "INACTIVE";
    Mesh mesh;
    public Transform planePoint;
    public Transform planeObj;
    Transform stylusPoint;
    // Planeobj properties
    float gridspacing = 0.02f;
    float width = 0;
    float length = 0;
    MeshFilter filter;

    // Use this for initialization
    void Start () {
        inactivepos = new Vector3(680,98,0);
        activepos = new Vector3(-934,90,0);
        myrect = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
        if (STATE == "GENHEAD")
        {
            GameObject.Find("CalibrationInstructions").GetComponent<Text>().text = "PRESS RMB AND DRAG";
            if (Input.GetKey(KeyCode.Mouse1))
            {
                // Get current stylus point in local coordinates
                Vector3 localpos = planePoint.InverseTransformPoint(stylusPoint.position);

                // Check if within bounds
                if (Mathf.Abs(localpos.x) < width / 2 && Mathf.Abs(localpos.y) < length / 2)
                {
                    
                    // Ok, we are within the zone, time to find out which vertex to update
                    // Should be as simple as rounding to the nearest gridspacing interval,
                    // then finding the proper index, then updating that vertex's z position to be localpos.z
                    float xcoord = (localpos.x + (width / 2)) / gridspacing;
                    float ycoord = (localpos.y + (length / 2)) / gridspacing;
                    // Debug.Log(System.Convert.ToString(xcoord) + "  " + System.Convert.ToString(ycoord));
                    int closestx = Mathf.RoundToInt(xcoord);
                    int closesty = Mathf.RoundToInt(ycoord);
                    int index = (verticeslong * closestx) + closesty;
                    vertices[index] = new Vector3(vertices[index].x, vertices[index].y, localpos.z);
                    filter.mesh.vertices = vertices;
                    filter.mesh.RecalculateNormals();
                }
                else
                {
                    GameObject.Find("CalibrationInstructions").GetComponent<Text>().text = "STYLUS OUTSIDE BOUNDS";
                    //stylusPoint.GetComponent<Renderer>().material = red;
                }
            }
            if (Input.GetKeyDown(KeyCode.Space)) {
                STATE = "INACTIVE";
                GameObject.Find("CalibrationInstructions").GetComponent<Text>().text = "";
                float base_x = -width / 2;
                float base_y = -length / 2;

                vertices = filter.mesh.vertices;
                for (int x = 0; x < verticeswide; x += 1)
                {
                    for (int y = 0; y < verticeslong; y += 1)
                    {
                        if (vertices[(verticeslong * x) + y] == new Vector3(base_x + x * gridspacing, base_y + y * gridspacing, -0.5f)) {
                            vertices[(verticeslong * x) + y] = Vector3.zero;
                        }
                        
                    }
                }
                filter.mesh.vertices = vertices;
                filter.mesh.RecalculateNormals();

            }
        }
	}
    public void activationKey() {
        if (isactive)
        {
            isactive = false;
            myrect.localPosition = inactivepos;
        }
        else {
            isactive = true;
            myrect.localPosition = activepos;
        }
    }
    public void loadHeadMeshFromFile() {
        // TODO
    }
    public void saveHeadMeshToFile(){
        // TODO
    }
    Vector3 midpoint_calc(Vector3 josh, Vector3 mark)
    {
        Vector3 newvec;
        newvec.x = josh.x + (mark.x - josh.x) / 2f;
        newvec.y = josh.y + (mark.y - josh.y) / 2f;
        newvec.z = josh.z + (mark.z - josh.z) / 2f;
        return newvec;
    }
    public void generateNewHeadMesh() {
        activationKey(); // Get this out of the way
        stylusPoint = GameObject.Find("Stylus").transform.Find("Point");
        // Need to make the new plane
        // First must find center point
        ScalpGenerator g = GameObject.Find("ScalpGenerator").GetComponent<ScalpGenerator>();
        // Loot the scalpgenerator for the reqd. points
        Transform righthead = GameObject.Find("Right Tragus").transform;
        Transform lefthead = GameObject.Find("Left Tragus").transform;
        Transform nose = GameObject.Find("Nasion").transform;
        Transform backhead = GameObject.Find("Inion").transform;

        Vector3 vlefttoright = righthead.position - lefthead.position;
        Vector3 vbacktonose = nose.position - backhead.position;
        Vector3 planedir = Vector3.Cross(vlefttoright, vbacktonose);
        Vector3 midpoint = midpoint_calc(midpoint_calc(backhead.position, nose.position), midpoint_calc(righthead.position, lefthead.position));

        planePoint.position = midpoint;
        planePoint.rotation = Quaternion.LookRotation(-planedir, vbacktonose);

        width = Vector3.Distance(lefthead.position, midpoint) * 3f;
        length = Vector3.Distance(nose.position, midpoint) * 3f;

        // Now to create our mesh
        // start from back left
        filter = planeObj.GetComponent<MeshFilter>();
        filter.mesh.Clear(); 


        // Generate verts
        verticeswide = System.Convert.ToInt32(width / gridspacing);
        verticeslong = System.Convert.ToInt32(length / gridspacing);
        int totalverts = verticeslong * verticeswide;
        vertices = new Vector3[totalverts];
        float base_x = -width / 2;
        float base_y = -length / 2;
        for (int x = 0; x < verticeswide; x += 1)
        {
            for (int y = 0; y < verticeslong; y += 1)
            {
                vertices[(verticeslong * x) + y] = new Vector3(base_y + y * gridspacing, base_x + x * gridspacing, 0);
            }
        }

        filter.mesh.vertices = vertices;

        // Now tris
        int[] tri = new int[(2 * (verticeswide - 1) * (verticeslong - 1)) * 3];
        ulong cur_tri_index = 0;
        for (int x = 0; x < verticeswide - 1; x += 1)
        {
            for (int y = 0; y < verticeslong - 1; y += 1)
            {
                //  Lower left triangle.
                tri[cur_tri_index] = (verticeslong * x) + y;
                tri[cur_tri_index + 1] = (verticeslong * (x + 1)) + y + 1;
                tri[cur_tri_index + 2] = (verticeslong * x) + y + 1;
                //  Upper right triangle.   
                tri[cur_tri_index + 3] = (verticeslong * x) + y;
                tri[cur_tri_index + 4] = (verticeslong * (x + 1)) + y;
                tri[cur_tri_index + 5] = (verticeslong * (x + 1)) + y + 1;
                //vertices[(verticeslong * x) + y]
                cur_tri_index += 6;
            }
        }

        filter.mesh.triangles = tri;

        // Now normals
        Vector3[] normals = new Vector3[totalverts];
        for (int i = 0; i < totalverts; ++i)
        {
            normals[i] = -Vector3.forward;
        }

        filter.mesh.normals = normals;

        // Testing, delete later

        for (int x = 0; x < verticeswide; x += 1)
        {
            for (int y = 0; y < verticeslong; y += 1)
            {
                vertices[(verticeslong * x) + y] = new Vector3(base_x + x * gridspacing, base_y + y * gridspacing, -0.5f);
            }
        }
        filter.mesh.vertices = vertices;
        filter.mesh.RecalculateNormals();
        
        STATE = "GENHEAD";
    }
    public void HeadVisibilityToggle() {
        if (GameObject.Find("headTest"))
        {
            GameObject head = GameObject.Find("headTest");
            if (head.GetComponent<MeshRenderer>().enabled)
            {
                head.GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                head.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }
}
