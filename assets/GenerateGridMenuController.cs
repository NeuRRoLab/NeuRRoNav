using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class GenerateGridMenuController : MonoBehaviour
{
    bool isactive = false;
    Vector3 inactivepos;
    Vector3 activepos;
    RectTransform myrect;
    public LayerMask mask;
    public Button activationkey;

    // Use this for initialization
    void Start()
    {
        activationkey = GameObject.Find("Generate Grid").GetComponent<Button>();
        inactivepos = new Vector3(680, 98, 0);
        activepos = new Vector3(-934, -269, 0);
        myrect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isactive)
        {
            if (!activationkey.interactable)
            {
                activationKey();
            }
        }
    }
    public void generateGridofHotSpots() {
        GameObject TPoint = GameObject.Find("TargetMatching").GetComponent<TargetMatching>().tPoint.pos.gameObject;
        // Need to get n and spacing
        int n = System.Convert.ToInt32(GameObject.Find("InputFieldGridDim").GetComponent<InputField>().text);
        float gridspacing = float.Parse(GameObject.Find("InputFieldGridSpacing").GetComponent<InputField>().text);
        // Plot out arr of vectors where needed
        //      - Get Central Point
        //GameObject coil = GameObject.Find(GameObject.Find("CoilTracker").GetComponent<Coil>().coilName);
        GameObject coilHotSpot = TPoint; //coil.transform.FindChild("container").FindChild("hotspot").gameObject;
        Vector3 hotSpotPos = coilHotSpot.transform.position;
        
        int totalwidth = 1 + 2 * n;
        float startposx = -(((float)n) * gridspacing);
        float startposz = -(((float)n) * gridspacing);
        float yheight = 0.5f;
        List<Vector3> list = new List<Vector3>();
        List<Vector3> dirs = new List<Vector3>();
        /*
        // Need to find dist of hotspot off of ground
        float dist = 0;
        Vector3 gpos1 = coilHotSpot.transform.TransformPoint(new Vector3(0, 0.5f, 0));
        RaycastHit hitinfo1;
        // Raycast down, if hit, then make a hotspot there
        if (Physics.Raycast(new Ray(gpos1, coilHotSpot.transform.TransformDirection(Vector3.down)), out hitinfo1, 100f, mask))
        {
            dist = Vector3.Distance(hitinfo1.point,coil.transform.position);
        }
        */
        // All of the following will be in localspace of given coil
        for (float x = startposx; x <= -startposx; x += gridspacing) {
            for (float z = startposz; z <= -startposz; z += gridspacing)
            {
                if (x == 0 && z == 0) {
                    continue;
                }
                Vector3 gpos = coilHotSpot.transform.TransformPoint(new Vector3(x, yheight, z));
               // GameObject pl = GameObject.CreatePrimitive(PrimitiveType.Cube);
               // pl.transform.position = gpos;
               // pl.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                RaycastHit hitinfo;
                // Raycast down, if hit, then make a hotspot there
                if (Physics.Raycast(new Ray(gpos, coilHotSpot.transform.TransformDirection(Vector3.down)), out hitinfo, 100f, mask))
                {
                   // list.Add(hitinfo.point);
                  //  dirs.Add(hitinfo.normal);
                    GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    plane.transform.position = hitinfo.point; // + hitinfo.normal.normalized * dist;
                    // Orient the cube such that its up is always consistent with the normal, then align 
                    // as best as possible with the forward of the coil

                    plane.transform.rotation = Quaternion.LookRotation(hitinfo.normal,coilHotSpot.transform.forward);
                    plane.transform.RotateAround(plane.transform.position,plane.transform.forward, -180);
                    plane.transform.RotateAround(plane.transform.position, plane.transform.right, 90);

                    GameObject.Find("TargetMatching").GetComponent<TargetMatching>().CreateScalpHotSpot(plane.transform.position, plane.transform.rotation);
                    Destroy(plane);
                    // plane.transform.up = hitinfo.normal;
                    plane.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                    // Now make a hotSpot here!
                   // GameObject.Find("TargetMatching").GetComponent<TargetMatching>().CreateScalpHotSpot(hitinfo.point + hitinfo.normal.normalized * dist, Quaternion.LookRotation(-hitinfo.normal, coilHotSpot.transform.right));



                    
                }
            }
        }

        //      
        // Generate suitable hotspots
    }
    public void activationKey()
    {
        if (isactive)
        {
            isactive = false;
            myrect.localPosition = inactivepos;
        }
        else
        {
            isactive = true;
            myrect.localPosition = activepos;
        }
    }
   
}

