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

    Text stylusTracking;

    void Start()
    {
        initialized = false;
        tracked = false;
        calibrationInstruct = GameObject.Find("CalibrationInstructions").GetComponent<Text>();
        camController = GameObject.Find("Camera Controller").GetComponent<CameraController>();
        coilTrackStatus = GameObject.Find("CoilTrackStatus").GetComponent<Text>();
        stylusTracking = GameObject.Find("StylusTrackStatus").GetComponent<Text>();
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
        if (calibrating && Input.anyKeyDown && stylusTracking.color.Equals(Color.green) &&(!Input.GetMouseButton(0)))
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
        if (GameObject.Find("Toggle_SavePrompts").GetComponent<Toggle>().isOn)
        {
            if (AskIfToSave())
            {
                ExportCoil();
            }
        }
        GameObject.Find("Save Coil").GetComponent<UnityEngine.UI.Button>().interactable = true;
        GameObject.Find("ScalpGenerator").GetComponent<ScalpGenerator>().waitingToDraw = true;

    }

    public void ExportCoil()
    {

        string path = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.coilSavePath);
        string fileName = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.coilSaveName);

        path += fileName;

        if (System.IO.File.Exists(path))
        {
            bool val = PromptOverwrite();
            if (val == false)
            {
                //Debug.Log("Quitting Save");
                return;
            }
            else
            {
               // Debug.Log("Overwriting!!!");
            }
        }

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

       // GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().incrementField((int)SettingsMenu.settings.coilSaveName);
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

            //MatchRotation();

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

            GameObject.Find("ScalpGenerator").GetComponent<ScalpGenerator>().waitingToDraw = true;
            GameObject.Find("Save Coil").GetComponent<UnityEngine.UI.Button>().interactable = true;
        }
    }

    public void setStylusSensitiveTrackingState(bool state)
    {
        coilTrackingIsSensitive = state;
    }

    bool PromptOverwrite()
    {
        using (var form1 = new System.Windows.Forms.Form())
        {
            System.Windows.Forms.Label text = new System.Windows.Forms.Label();
            System.Windows.Forms.Button button1 = new System.Windows.Forms.Button();
            System.Windows.Forms.Button button3 = new System.Windows.Forms.Button();
            System.Windows.Forms.Button buttondefault = new System.Windows.Forms.Button();
            buttondefault.Location = new System.Drawing.Point(-2000, -2000);

            text.Text = text.Text = "A file exists at the Coil Save location specified! \nDo you want to overwrite?\n\nIf not: Cancel, then edit the Save Coil Field, \nthen Save Manually.";
            text.Width = 280;
            text.Height = 70;
            text.Location
               = new System.Drawing.Point(10, 10);

            // Set the text of button1 to "OK".
            button3.Text = "Cancel Save";
            // Set the position of the button on the form.
            button3.Location = new System.Drawing.Point(text.Left, text.Height + text.Top + 10);
            button3.BackColor = System.Drawing.Color.LightGreen;
            button3.Width = 100;

            // Set the text of button1 to "OK".
            button1.Text = "Overwrite!";
            // Set the position of the button on the form.
            button1.Location = new System.Drawing.Point(button3.Left, button3.Height + button3.Top + 15);
            button1.BackColor = System.Drawing.Color.LightYellow;
            button1.Width = 100;
            form1.Text = "CAUTION";
            // Define the border style of the form to a dialog box.
            form1.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            // Set the MaximizeBox to false to remove the maximize box.
            form1.MaximizeBox = false;
            // Set the MinimizeBox to false to remove the minimize box.
            form1.MinimizeBox = false;
            // Set the accept button of the form to button1.
            form1.AcceptButton = button1;
            form1.CancelButton = button3;
            // Set the cancel button of the form to button2.
            // Set the start position of the form to the center of the screen.
            form1.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            form1.Height = 200;
            form1.Width = 300;

            button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            button3.DialogResult = System.Windows.Forms.DialogResult.Cancel;

            //Add button1 to the form.
            form1.Controls.Add(buttondefault);
            form1.Controls.Add(button1);
            //Add button2 to the form.
            form1.Controls.Add(button3);

            form1.Controls.Add(text);
            System.Windows.Forms.DialogResult retval = form1.ShowDialog();
            // Display the form as a modal dialog box.
            if (retval == System.Windows.Forms.DialogResult.Cancel)
            {
                //Debug.Log("Canceled save");
                return false;
            }
            if (retval == System.Windows.Forms.DialogResult.OK)
            {
                //Debug.Log("accepted");
                return true;
            }


        }
        return false;
    }

    bool AskIfToSave()
    {
        using (var form1 = new System.Windows.Forms.Form())
        {
            System.Windows.Forms.Label text = new System.Windows.Forms.Label();
            System.Windows.Forms.Button button1 = new System.Windows.Forms.Button();
            System.Windows.Forms.Button button3 = new System.Windows.Forms.Button();
            System.Windows.Forms.Button buttondefault = new System.Windows.Forms.Button();
            buttondefault.Location = new System.Drawing.Point(-2000, -2000);

            text.Text = "Successfully calibrated Coil! Do you want to save to \nCoil Save location?";
            text.Width = 280;
            text.Height = 50;
            text.Location
               = new System.Drawing.Point(10, 10);

            // Set the text of button1 to "OK".
            button3.Text = "Yes";
            // Set the position of the button on the form.
            button3.Location = new System.Drawing.Point(text.Left, text.Height + text.Top + 10);
            button3.BackColor = System.Drawing.Color.LightGreen;
            button3.Width = 100;

            // Set the text of button1 to "OK".
            button1.Text = "No";
            // Set the position of the button on the form.
            button1.Location = new System.Drawing.Point(button3.Left, button3.Height + button3.Top + 15);
            button1.BackColor = System.Drawing.Color.Pink;
            button1.Width = 100;
            form1.Text = "";
            // Define the border style of the form to a dialog box.
            form1.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            // Set the MaximizeBox to false to remove the maximize box.
            form1.MaximizeBox = false;
            // Set the MinimizeBox to false to remove the minimize box.
            form1.MinimizeBox = false;
            // Set the accept button of the form to button1.
            form1.AcceptButton = button1;
            form1.CancelButton = button3;
            // Set the cancel button of the form to button2.
            // Set the start position of the form to the center of the screen.
            form1.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            form1.Height = 200;
            form1.Width = 300;

            button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            button3.DialogResult = System.Windows.Forms.DialogResult.OK;

            //Add button1 to the form.
            form1.Controls.Add(buttondefault);
            form1.Controls.Add(button1);
            //Add button2 to the form.
            form1.Controls.Add(button3);

            form1.Controls.Add(text);
            System.Windows.Forms.DialogResult retval = form1.ShowDialog();
            // Display the form as a modal dialog box.
            if (retval == System.Windows.Forms.DialogResult.Cancel)
            {
                //Debug.Log("Canceled save");
                return false;
            }
            if (retval == System.Windows.Forms.DialogResult.OK)
            {
                //Debug.Log("accepted");
                return true;
            }


        }
        return false;
    }

}
