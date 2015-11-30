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

public class CalibrationTool : MonoBehaviour
{

    public GameObject SlipStreamObject;
    CameraController camController;
    Text headTrackStatus;
    bool tracked;
    GameObject tracker;

    // Use this for initialization
    void Start()
    {
        camController = GameObject.Find("Camera Controller").GetComponent<CameraController>();
        tracked = false;
        SlipStreamObject = GameObject.Find("Optitrack");
        SlipStreamObject.GetComponent<SlipStream>().PacketNotification += new PacketReceivedHandler(OnPacketReceived);
    }

    // packet received
    void OnPacketReceived(object sender, string Packet)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(Packet);

        XmlNodeList rigidBodyList = xmlDoc.GetElementsByTagName("Body");

        for (int index = 0; index < rigidBodyList.Count; index++)
        {

            string name = System.Convert.ToString(rigidBodyList[index].Attributes["Name"].InnerText);
            name = name.Replace(" ", string.Empty);
            if (name.Equals("calibrationtool", StringComparison.OrdinalIgnoreCase))
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

                    //== bone pose ==--

                    Vector3 position = new Vector3(x, y, z);
                    Quaternion orientation = new Quaternion(qx, qy, qz, qw);

                    //== locate or create bone object ==--

                    string objectName = name;

                    tracker = GameObject.Find(objectName);

                    if (tracker == null)
                    {
                        tracker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        Vector3 scale = new Vector3(0.01f, 0.01f, 0.01f);
                        tracker.transform.localScale = scale;
                        tracker.name = objectName;

                        GameObject text = Instantiate(GameObject.Find("CalibrationToolText"));
                        text.transform.position = tracker.transform.position;
                        text.transform.rotation = Quaternion.identity;
                        text.transform.parent = tracker.transform;

                    }

                    //== set bone's pose ==--

                    tracker.transform.position = position;
                    tracker.transform.rotation = orientation;
                    break;
                }
                else
                {
                    tracked = false;
                    break;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
