using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Xml;
using System;
using UnityEngine.Audio;
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

public class Stylus : MonoBehaviour
{

    public GameObject SlipStreamObject;
    CameraController camController;
    Text stylusTrackStatus;
    GameObject stylus;
    GameObject point;
    AudioSource trackingWarning;


    bool stylusTrackingIsSensitive;

    bool initialized;
    bool tracked;
    bool lost;
    bool pauseTracking;

    void Start()
    {
        initialized = false;
        tracked = false;
        camController = GameObject.Find("Camera Controller").GetComponent<CameraController>();
        SlipStreamObject = GameObject.Find("Optitrack");
        stylusTrackStatus = GameObject.Find("StylusTrackStatus").GetComponent<Text>();
        SlipStreamObject.GetComponent<SlipStream>().PacketNotification += new PacketReceivedHandler(OnPacketReceived);
        stylusTrackingIsSensitive = false;

        trackingWarning = GameObject.Find("Alert").GetComponent<AudioSource>();
    }

    // packet received
    void OnPacketReceived(object sender, string Packet)
    {
        if (!pauseTracking)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(Packet);

            XmlNodeList rigidBodyList = xmlDoc.GetElementsByTagName("Body");

            for (int index = 0; index < rigidBodyList.Count; index++)
            {
                string name = System.Convert.ToString(rigidBodyList[index].Attributes["Name"].InnerText);
                name = name.Replace(" ", string.Empty);
                name = name.ToLower();
                if (name.Equals("stylus", StringComparison.OrdinalIgnoreCase))
                {
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
                            string objectName = "Stylus";
                            stylus = GameObject.Find(objectName);
                            initialized = true;
                        }

                        stylus.transform.position = position;
                        stylus.transform.rotation = orientation;
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
                stylusTrackStatus.color = Color.red;
                if (stylusTrackingIsSensitive)
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
                stylusTrackStatus.color = Color.green;
            }
        }
    }

    public void setPoint()
    {

        pauseTracking = true;
        if (point != null)
        {
            DestroyImmediate(point.transform.FindChild("Point Sphere").gameObject);
            DestroyImmediate(point);
        }
        GameObject calibrationTool = GameObject.Find("CalibrationTool");
        GameObject model = stylus.transform.FindChild("model").gameObject;
        GameObject tip = model.transform.FindChild("Tip").gameObject;

        tip.transform.rotation = calibrationTool.transform.rotation;
        model.transform.parent = null;
        model.transform.rotation = new Quaternion(0, 0, 0, 0);
        tip.transform.parent = null;
        model.transform.parent = tip.transform;
        tip.transform.position = new Vector3(calibrationTool.transform.position.x, tip.transform.position.y, calibrationTool.transform.position.z);
        model.transform.parent = stylus.transform;
        tip.transform.parent = model.transform;
        

        point = new GameObject();
        GameObject psphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        psphere.name = "Point Sphere";
        Vector3 scale = new Vector3(0.01f, 0.01f, 0.01f);
        psphere.transform.localScale = scale;
        point.transform.position = calibrationTool.transform.position;
        point.transform.rotation = calibrationTool.transform.rotation;
        psphere.transform.parent = point.transform;
        psphere.transform.localPosition = new Vector3(0, 0.005F, 0);
        point.name = "Point";

        point.transform.parent = stylus.transform;

        camController.putCamOnStylus(1);
        //stylus.transform.LookAt(point.transform);

        Debug.Log(point.transform.position.ToString());
        Debug.Log(calibrationTool.transform.position.ToString());
        pauseTracking = false;

        GameObject.Find("Calibrate Coil").GetComponent<Button>().interactable = true;
        GameObject.Find("Landmarks").GetComponent<Button>().interactable = true;
        foreach(Button b in GameObject.Find("Landmarks").GetComponentsInChildren<Button>())
        {
            b.interactable = true;
        }
    }

    public void setStylusSensitiveTrackingState(bool state)
    {
        stylusTrackingIsSensitive = state;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
