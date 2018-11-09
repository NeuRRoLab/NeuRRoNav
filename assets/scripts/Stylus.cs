using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Xml;
using System;
using UnityEngine.Audio;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System.Windows.Forms;
using System.Drawing;
#endif

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
	TransformSmoother transformSmoother = new TransformSmoother();

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

						transformSmoother.AddTransform(position, orientation);
                        this.transform.position = transformSmoother.GetAveragePosition();
                        this.transform.rotation = transformSmoother.GetAverageRotation();
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
                stylusTrackStatus.color = UnityEngine.Color.red;
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
                stylusTrackStatus.color = UnityEngine.Color.green;
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

		GameObject.Find("Calibrate Coil").GetComponent<UnityEngine.UI.Button>().interactable = true;
		GameObject.Find("Landmarks").GetComponent<UnityEngine.UI.Button>().interactable = true;
		//foreach (Button b in GameObject.Find("LandmarksList").GetComponentsInChildren<Button>()) {
		//	b.interactable = true;
		//}
		pauseTracking = false;
        ExportStylus();
	}

    public void setPointFromVector3(Vector3 vec)
    {
        pauseTracking = true;
        if (point != null)
        {
            DestroyImmediate(point);
        }

        GameObject calibrationTool = GameObject.Find("CalibrationTool");
        GameObject pivot = this.transform.FindChild("Pivot").gameObject;
        GameObject connectorLine = pivot.transform.FindChild("ConnectorLine").gameObject;

        point = new GameObject();
        point.name = "Point";
        //point.transform.position = calibrationTool.transform.position;
        point.transform.parent = this.transform;
        point.transform.localPosition = vec;

        pivot.transform.LookAt(point.transform.position);


        GameObject psphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        psphere.name = "Point Sphere";
        psphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        psphere.transform.parent = point.transform;
        psphere.transform.localPosition = new Vector3(0, 0, 0);

        connectorLine.transform.localScale = new Vector3(1, Vector3.Distance(pivot.transform.position, point.transform.position), 1);

        camController.putCamOnStylus(1);

        GameObject.Find("Calibrate Coil").GetComponent<UnityEngine.UI.Button>().interactable = true;
        GameObject.Find("Landmarks").GetComponent<UnityEngine.UI.Button>().interactable = true;
        //foreach (Button b in GameObject.Find("LandmarksList").GetComponentsInChildren<Button>()) {
        //	b.interactable = true;
        //}
        pauseTracking = false;
    }

    public void ImportStylus()
    {
        if (initialized)
        {
            //Debug.Log("Input stylus!!!");
            Vector3 pos = Vector3.zero;
            try
            {
                string path = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.stylusLoadPath);
                string fileName = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.stylusLoadName);

                path += fileName;

                System.IO.FileStream filestream = new System.IO.FileStream(path,
                                                     System.IO.FileMode.Open,
                                                     System.IO.FileAccess.Read,
                                                     System.IO.FileShare.Read);
                System.IO.StreamReader file = new System.IO.StreamReader(filestream);

                string data = file.ReadLine();

                string[] dims = data.Split('\t');
                pos = new Vector3((float)System.Convert.ToDouble(dims[0]), (float)System.Convert.ToDouble(dims[1]), (float)System.Convert.ToDouble(dims[2]));
                file.Close();

                setPointFromVector3(pos);
            }
            catch (Exception e) {
                Debug.Log(e.Message);
            }
        }

    }

    public void ExportStylus() {
        using (var form1 = new Form())
        {
            System.Windows.Forms.Button button1 = new System.Windows.Forms.Button();
            System.Windows.Forms.Button button2 = new System.Windows.Forms.Button();

            // Set the text of button1 to "OK".
            button1.Text = "OK";
            // Set the position of the button on the form.
            button1.Location = new Point(10, 10);
            // Set the text of button2 to "Cancel".
            button2.Text = "Cancel";
            // Set the position of the button based on the location of button1.
            button2.Location
               = new Point(button1.Left, button1.Height + button1.Top + 10);
            // Set the caption bar text of the form.   
            form1.Text = "My Dialog Box";
            // Display a help button on the form.
            form1.HelpButton = true;

            // Define the border style of the form to a dialog box.
            form1.FormBorderStyle = FormBorderStyle.FixedDialog;
            // Set the MaximizeBox to false to remove the maximize box.
            form1.MaximizeBox = false;
            // Set the MinimizeBox to false to remove the minimize box.
            form1.MinimizeBox = false;
            // Set the accept button of the form to button1.
            form1.AcceptButton = button1;
            // Set the cancel button of the form to button2.
            form1.CancelButton = button2;
            // Set the start position of the form to the center of the screen.
            form1.StartPosition = FormStartPosition.CenterScreen;

            //Add button1 to the form.
            form1.Controls.Add(button1);
            //Add button2 to the form.
            form1.Controls.Add(button2);

            // Display the form as a modal dialog box.
            form1.ShowDialog();
        }
        try
        {
            Vector3 lpos = transform.Find("Point").localPosition; // only proceed if such a point exists i.e. there is a point
            string path = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.stylusSavePath);
            string fileName = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.stylusSaveName);

            path += fileName;

            

            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(path, false))
            {
                file.WriteLine(lpos.x + "\t" + lpos.y + "\t" + lpos.z);
            }

            GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().incrementField((int)SettingsMenu.settings.stylusSaveName);
        }
        catch (Exception e) {
            Debug.Log(e.Message);
        }
    }


	public void setStylusSensitiveTrackingState(bool state)
    {
        stylusTrackingIsSensitive = state;
    }

}
