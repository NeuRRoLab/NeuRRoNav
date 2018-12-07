using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MappingController : MonoBehaviour
{
    /*
    public Text consoleoutput;
    public Transform rightCont;
    public Transform leftCont;

    private SteamVR_TrackedObject trackedObjectRight;
    private SteamVR_Controller.Device deviceRight;
    private SteamVR_TrackedObject trackedObjectLeft;
    private SteamVR_Controller.Device deviceLeft;
    private SteamVR_Controller.Device deviceStylus;
    private SteamVR_Controller.Device deviceOther;

    public string state = "UNINITIALIZED";
    public bool stylus_is_left = false;
    public Transform stylusPoint;
    public Transform planePoint;
    public Transform planeObj;
    // Planeobj properties
    float gridspacing = 0.02f;
    float width = 0;
    float length = 0;

    public Material green;
    public Material red;

    Vector3[] vertices;
    int verticeslong;
    int verticeswide;
    Mesh mesh;
    //HEADEXTREMITYmapping
    public Transform nose;
    public Transform backhead;
    public Transform lefthead;
    public Transform righthead;
    public int extremityindex = 0;
    // Use this for initialization
    void Start()
    {
        consoleoutput.text = "Press CALIBRATE STYLUS to begin calibration";
        trackedObjectRight = rightCont.GetComponent<SteamVR_TrackedObject>();
        trackedObjectLeft = leftCont.GetComponent<SteamVR_TrackedObject>();
    }

    // Update is called once per frame
    void Update()
    {

        deviceRight = SteamVR_Controller.Input((int)trackedObjectRight.index);
        deviceLeft = SteamVR_Controller.Input((int)trackedObjectLeft.index);
        if (stylus_is_left)
        {
            deviceStylus = deviceLeft;
            deviceOther = deviceRight;
        }
        else
        {
            deviceStylus = deviceRight;
            deviceOther = deviceLeft;
        }

        if (state == "CALIBRATESTYLUS")
        {
            if (deviceLeft.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                //Debug.Log("LT");
                stylus_is_left = true;
                stylusPoint.parent = leftCont;
                stylusPoint.position = GameObject.Find("Device" + deviceRight.index).transform.Find("trigger").Find("attach").position;
                consoleoutput.text = "When satisfied, continue to MAP HEAD EXTREMITIES";
            }
            if (deviceRight.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                //Debug.Log("RT");
                stylus_is_left = false;
                stylusPoint.parent = rightCont;
                stylusPoint.position = GameObject.Find("Device" + deviceLeft.index).transform.Find("trigger").Find("attach").position;
                consoleoutput.text = "When satisfied, continue to MAP HEAD EXTREMITIES";
            }
        }
        if (state == "MAPHEADEXTREMITIES")
        {
            if (deviceStylus.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                if (extremityindex == 0)
                {
                    nose.position = stylusPoint.position;
                    consoleoutput.text = "Place stylus on back of head, then pull trigger";
                    extremityindex = 1;
                }
                else if (extremityindex == 1)
                {
                    backhead.position = stylusPoint.position;
                    consoleoutput.text = "Place stylus on left of head, then pull trigger";
                    extremityindex = 2;
                }
                else if (extremityindex == 2)
                {
                    lefthead.position = stylusPoint.position;
                    consoleoutput.text = "Place stylus on right of head, then pull trigger";
                    extremityindex = 3;
                }
                else if (extremityindex == 3)
                {
                    righthead.position = stylusPoint.position;
                    consoleoutput.text = "EXTREMITY MAPPING COMPLETE, CREATING BLANK HEAD MESH";
                    extremityindex = 10000;
                    createHeadMesh();
                }

            }

        }
        if (state == "GENHEADMAP")
        {
            if (deviceStylus.GetPress(SteamVR_Controller.ButtonMask.Trigger))
            {
                // Get current stylus point in local coordinates
                Vector3 localpos = planePoint.InverseTransformPoint(stylusPoint.position);

                // Check if within bounds
                if (Mathf.Abs(localpos.x) < width / 2 && Mathf.Abs(localpos.y) < length / 2)
                {
                    stylusPoint.GetComponent<Renderer>().material = green;
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
                    mesh.vertices = vertices;
                    mesh.RecalculateNormals();
                }
                else
                {
                    stylusPoint.GetComponent<Renderer>().material = red;
                }
            }
        }
    }
    public void setStylusCalibrate()
    {
        consoleoutput.text = "Place trigger of non-stylus controller on tip of stylus, then pull trigger of stylus.";
        state = "CALIBRATESTYLUS";
    }
    public void setMapHeadExtremities()
    {
        consoleoutput.text = "Place stylus on nose, then pull trigger";
        state = "MAPHEADEXTREMITIES";
    }
    public void generateTopologicalMesh()
    {
        consoleoutput.text = "Hold down trigger while dragging over subject's head";
        state = "GENHEADMAP";
    }
    Vector3 midpoint_calc(Vector3 josh, Vector3 mark)
    {
        Vector3 newvec;
        newvec.x = josh.x + (mark.x - josh.x) / 2f;
        newvec.y = josh.y + (mark.y - josh.y) / 2f;
        newvec.z = josh.z + (mark.z - josh.z) / 2f;
        return newvec;
    }
    void createHeadMesh()
    {
        // Must first find center point
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
        MeshFilter filter = planeObj.GetComponent<MeshFilter>();
        mesh = filter.mesh;


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

        mesh.vertices = vertices;

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

        mesh.triangles = tri;

        // Now normals
        Vector3[] normals = new Vector3[totalverts];
        for (int i = 0; i < totalverts; ++i)
        {
            normals[i] = -Vector3.forward;
        }

        mesh.normals = normals;

        // Testing, delete later

        for (int x = 0; x < verticeswide; x += 1)
        {
            for (int y = 0; y < verticeslong; y += 1)
            {
                vertices[(verticeslong * x) + y] = new Vector3(base_x + x * gridspacing, base_y + y * gridspacing, -0.5f);
            }
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();



    }*/
}

