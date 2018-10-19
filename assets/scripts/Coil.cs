using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Xml;
using System;
//=============================================================================----
// Copyright © NaturalPoint, Inc. All Rights Reserved.
// 
// This software is provided by the copyright holders and contributors "as is" and
// any express or implied warranties, including, but not limited to, the implied
// warranties of merchantability and fitness for a particular purpose are disclaimed.
// In no event shall NaturalPoint, Inc. or contributors be liable for any direct,
// indirect, incidental, special, exemplary, or consequential damages
// (including, but not limited to, procurement of substitute goods or services;
// loss of use, data, or profits; or business interruption) however caused
// and on any theory of liability, whether in contract, strict liability,
// or tort (including negligence or otherwise) arising in any way out of
// the use of this software, even if advised of the possibility of such damage.
//=============================================================================----

// Attach Body.cs to an empty Game Object and it will parse and create visual
// game objects based on bone data.  Body.cs is meant to be a simple example 
// of how to parse and display skeletal data in Unity.

// In order to work properly, this class is expecting that you also have instantiated
// another game object and attached the Slip Stream script to it.  Alternatively
// they could be attached to the same object.

public class Coil : MonoBehaviour
{
    public string coilName;
    CameraController camController;
    Text coilTrackStatus;
    GameObject stylusPoint;
    GameObject coil;
    GameObject hotspot;
    GameObject calibrateHotSpot;
    GameObject forward;
    GameObject right;
    GameObject calibrateForward;
    GameObject calibrateRight;
	TransformSmoother transformSmoother = new TransformSmoother();

	Text calibrationInstruct;

    AudioSource trackingWarning;

    bool initialized;
    bool tracked;
    bool calibrating;
    bool coilTrackingIsSensitive;

    int point;

    void Start()
    {
        initialized = false;
        tracked = false;
        calibrationInstruct = GameObject.Find("CalibrationInstructions").GetComponent<Text>();
        camController = GameObject.Find("Camera Controller").GetComponent<CameraController>();
        coilTrackStatus = GameObject.Find("CoilTrackStatus").GetComponent<Text>();
        FindObjectOfType<SlipStream>().PacketNotification += new PacketReceivedHandler(OnPacketReceived);

        coilTrackingIsSensitive = false;
        trackingWarning = GameObject.Find("Alert").GetComponent<AudioSource>();
    }

    // packet received
    void OnPacketReceived(object sender, string Packet)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(Packet);

        XmlNodeList rigidBodyList = xmlDoc.GetElementsByTagName("Body");
        //Debug.Log(System.Environment.Version.ToString());

        for (int index = 0; index < rigidBodyList.Count; index++)
        {
            String name = System.Convert.ToString(rigidBodyList[index].Attributes["Name"].InnerText);
            name = name.Replace(" ", string.Empty);
            name = name.ToLower();
            if (name.Equals("coil") || name.Equals("doublecone") || name.Equals("figureeight"))
            {
                coilName = name;
                if (System.Convert.ToInt32(rigidBodyList[index].Attributes["Tracked"].InnerText) == 1)
                {
                    tracked = true;
                    int id = System.Convert.ToInt32(rigidBodyList[index].Attributes["ID"].InnerText);

                    float x = (float)System.Convert.ToDouble(rigidBodyList[index].Attributes["x"].InnerText);
                    float y = (float)System.Convert.ToDouble(rigidBodyList[index].Attributes["y"].InnerText);
                    float z = (float)System.Convert.ToDouble(rigidBodyList[index].Attributes["z"].InnerText);

                    float qx = (float)System.Convert.ToDouble(rigidBodyList[index].Attributes["qx"].InnerText);
                    float qy = (float)System.Convert.ToDouble(rigidBodyList[index].Attributes["qy"].InnerText);
                    float qz = (float)System.Convert.ToDouble(rigidBodyList[index].Attributes["qz"].InnerText);
                    float qw = (float)System.Convert.ToDouble(rigidBodyList[index].Attributes["qw"].InnerText);


                    //== coordinate system conversion (right to left handed) ==--

                    z = -z;
                    qz = -qz;
                    qw = -qw;


                    Vector3 position = new Vector3(x, y, z);
                    Quaternion orientation = new Quaternion(qx, qy, qz, qw);

                    if (initialized == false)
                    {
                        string objectName = name;
                        coil = GameObject.Find(objectName);
                        initialized = true;
                    }

					transformSmoother.AddTransform(position, orientation);
					coil.transform.position = transformSmoother.GetAveragePosition();
					coil.transform.rotation = transformSmoother.GetAverageRotation();
					break;
                }

                else
                {
                    tracked = false;
                    break;
                }
            }
        }
        if (tracked == false)
        {
            coilTrackStatus.color = Color.red;
            if (coilTrackingIsSensitive)
            {
                if (!trackingWarning.isPlaying)
                {
                    trackingWarning.Play();
                }
            }
        }
        else
        {
            if (trackingWarning.isPlaying)
            {
                trackingWarning.Stop();
            }
            coilTrackStatus.color = Color.green;
        }
    }

    public void calibrate()
    {
        GameObject.Find("ScalpGenerator").GetComponent<ScalpGenerator>().waitingToDraw = false;
        GameObject container = coil.transform.FindChild("container").gameObject;
        stylusPoint = GameObject.Find("Stylus").transform.FindChild("Point").gameObject;
        forward = container.transform.FindChild("forward").gameObject;
        hotspot = container.transform.FindChild("hotspot").gameObject;
        right = container.transform.FindChild("right").gameObject;

        point = 0;

        calibrationInstruct.text = "Hotspot";

        calibrating = true;
    }

    void Update()
    {
        if (calibrating && Utility.AnyInputDown())
        {
            if (point == 0)
            {
                calibrateHotSpot = new GameObject();
                calibrateHotSpot.name = "calibrateHotSpot";
                calibrateHotSpot.transform.position = stylusPoint.transform.position;
                calibrateHotSpot.transform.rotation = stylusPoint.transform.rotation;
                calibrateHotSpot.transform.parent = coil.transform;
                point++;
                calibrationInstruct.text = "Top/Forward";
            }
            else if (point == 1)
            {
                calibrateForward = new GameObject();

                calibrateForward.name = "calibrateForward";
                calibrateForward.transform.position = stylusPoint.transform.position;
                calibrateForward.transform.parent = coil.transform;
                point++;
                calibrationInstruct.text = "Right";
            }
            else if (point == 2)
            {
                calibrationInstruct.text = "Calibrating";
                calibrateRight = new GameObject();
                calibrateRight.name = "calibrateRight";
                calibrateRight.transform.position = stylusPoint.transform.position;

                MatchRotation();
            }

            //camController.putTargetCamOnStylus();
            //stylusPoint.transform.LookAt(hotspot.transform);
        }
    }

    private void MatchRotation()
    {
        GameObject container = coil.transform.FindChild("container").gameObject;

        hotspot.transform.parent = null;
        calibrateHotSpot.transform.parent = null;
        calibrateForward.transform.parent = null;
        calibrateRight.transform.parent = null;

        container.transform.parent = hotspot.transform;
        hotspot.transform.position = calibrateHotSpot.transform.position;
        container.transform.parent = null;

        hotspot.transform.LookAt(forward.transform);
        container.transform.parent = hotspot.transform;
        hotspot.transform.LookAt(calibrateForward.transform);

        container.transform.parent = null;
        hotspot.transform.LookAt(right.transform, calibrateForward.transform.position - calibrateHotSpot.transform.position);
        container.transform.parent = hotspot.transform;
        hotspot.transform.LookAt(calibrateRight.transform, calibrateForward.transform.position - calibrateHotSpot.transform.position);

        container.transform.parent = coil.transform;
        hotspot.transform.rotation = container.transform.rotation;
        hotspot.transform.parent = container.transform;

        calibrateForward.transform.parent = coil.transform;
        calibrateHotSpot.transform.parent = coil.transform;
        calibrateRight.transform.parent = coil.transform;

        calibrating = false;
        point = 0;
        calibrationInstruct.text = "";

        ExportCoil();

        GameObject.Find("ScalpGenerator").GetComponent<ScalpGenerator>().waitingToDraw = true;

    }

    public void ExportCoil()
    {

        string path = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.coilSavePath);
        string fileName = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.coilSaveName);

        path += fileName;

        using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(path, false))
        {
            Vector3 hotspotLoc = coil.transform.InverseTransformPoint(calibrateHotSpot.transform.position);
            Vector3 rightLoc = coil.transform.InverseTransformPoint(calibrateRight.transform.position);
            Vector3 forwarLoc = coil.transform.InverseTransformPoint(calibrateForward.transform.position);

            file.WriteLine(hotspotLoc.x + "\t" + hotspotLoc.y + "\t" + hotspotLoc.z);
            file.WriteLine(rightLoc.x + "\t" + rightLoc.y + "\t" + rightLoc.z);
            file.WriteLine(forwarLoc.x + "\t" + forwarLoc.y + "\t" + forwarLoc.z);
        }

        GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().incrementField((int)SettingsMenu.settings.coilSaveName);
    }

    public void ImportCoil()
    {
        if (initialized)
        {
            GameObject container = coil.transform.FindChild("container").gameObject;
            forward = container.transform.FindChild("forward").gameObject;
            hotspot = container.transform.FindChild("hotspot").gameObject;
            right = container.transform.FindChild("right").gameObject;

            string path = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.coilLoadPath);
            string fileName = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.coilLoadName);

            path += fileName;

            try
            {

                System.IO.FileStream filestream = new System.IO.FileStream(path,
                                                  System.IO.FileMode.Open,
                                                  System.IO.FileAccess.Read,
                                                  System.IO.FileShare.Read);
                System.IO.StreamReader file = new System.IO.StreamReader(filestream);

                GameObject[] points = { calibrateHotSpot, calibrateRight, calibrateForward };
                string[] names = { "calibrateHotSpot", "calibrateRight", "calibrateForward" };
                for (int i = 0; i < 3; i++)
                {
                    string data = file.ReadLine();
                    char[] d = new char[1];
                    d[0] = '\t';
                    string[] dims = data.Split(d);
                    points[i] = new GameObject(names[i]);
                    points[i].transform.position = coil.transform.TransformPoint(new Vector3((float)System.Convert.ToDouble(dims[0]), (float)System.Convert.ToDouble(dims[1]), (float)System.Convert.ToDouble(dims[2])));
                    points[i].transform.parent = coil.transform;
                }
                file.Close();


                calibrateHotSpot = points[0];
                calibrateRight = points[1];
                calibrateForward = points[2];
        }
            catch (Exception e)
        {
            //something went wrong, warn user
            return;
        }

        MatchRotation();
        }
    }

    public void setStylusSensitiveTrackingState(bool state)
    {
        coilTrackingIsSensitive = state;
    }
}
