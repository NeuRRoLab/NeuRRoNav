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
    CameraController camController;
    Text stylusTrackStatus;
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
        stylusTrackStatus = GameObject.Find("StylusTrackStatus").GetComponent<Text>();
        FindObjectOfType<SlipStream>().PacketNotification += new PacketReceivedHandler(OnPacketReceived);
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
                            initialized = true;
                        }

                        this.transform.position = position;
                        this.transform.rotation = orientation;
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

	public void setPoint() {
		pauseTracking = true;
		if (point != null) {
			DestroyImmediate(point);
		}

		GameObject calibrationTool = GameObject.Find("CalibrationTool");
		GameObject pivot = this.transform.FindChild("Pivot").gameObject;
		GameObject connectorLine = pivot.transform.FindChild("ConnectorLine").gameObject;

		point = new GameObject();
		point.name = "Point";
		point.transform.position = calibrationTool.transform.position;
		point.transform.parent = this.transform;

		pivot.transform.LookAt(point.transform.position);


		GameObject psphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		psphere.name = "Point Sphere";
		psphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
		psphere.transform.parent = point.transform;
		psphere.transform.localPosition = new Vector3(0, 0, 0);

		connectorLine.transform.localScale = new Vector3(1, Vector3.Distance(pivot.transform.position, point.transform.position), 1);

		camController.putCamOnStylus(1);

		GameObject.Find("Calibrate Coil").GetComponent<Button>().interactable = true;
		GameObject.Find("Landmarks").GetComponent<Button>().interactable = true;
		foreach (Button b in GameObject.Find("LandmarksList").GetComponentsInChildren<Button>()) {
			b.interactable = true;
		}
		pauseTracking = false;
	}


	public void setStylusSensitiveTrackingState(bool state)
    {
        stylusTrackingIsSensitive = state;
    }

}
