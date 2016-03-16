using UnityEngine;
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

public class RigidBody : MonoBehaviour {
	
	public GameObject SlipStreamObject;
	
	// Use this for initialization
	void Start () 
	{
		Debug.Log (Application.dataPath);
		//System.Diagnostics.Process.Start(Application.dataPath + @"\Scripts\OptitrackUnityPlugin\Source\Unity3D\bin\UnitySample.exe");
		SlipStreamObject = GameObject.Find("Optitrack");
		SlipStreamObject.GetComponent<SlipStream>().PacketNotification += new PacketReceivedHandler(OnPacketReceived);
	}
	
	// packet received
	void OnPacketReceived(object sender, string Packet)
	{
		XmlDocument xmlDoc= new XmlDocument();
		xmlDoc.LoadXml(Packet);
        
		XmlNodeList rigidBodyList = xmlDoc.GetElementsByTagName("Body");
        //Debug.Log(System.Environment.Version.ToString());
        Debug.Log("Looking for Rigid Bodies");

		for(int index=0; index<rigidBodyList.Count; index++)
		{

            Debug.Log("Getting Rigid Body");
			int id = System.Convert.ToInt32(rigidBodyList[index].Attributes["ID"].InnerText);
			
			float x = (float) System.Convert.ToDouble(rigidBodyList[index].Attributes["x"].InnerText);
			float y = (float) System.Convert.ToDouble(rigidBodyList[index].Attributes["y"].InnerText);
			float z = (float) System.Convert.ToDouble(rigidBodyList[index].Attributes["z"].InnerText);
			
			float qx = (float) System.Convert.ToDouble(rigidBodyList[index].Attributes["qx"].InnerText);
			float qy = (float) System.Convert.ToDouble(rigidBodyList[index].Attributes["qy"].InnerText);
			float qz = (float) System.Convert.ToDouble(rigidBodyList[index].Attributes["qz"].InnerText);
			float qw = (float) System.Convert.ToDouble(rigidBodyList[index].Attributes["qw"].InnerText);
			
			//== coordinate system conversion (right to left handed) ==--
			
			z = -z;
			qz = -qz;
			qw = -qw;
			
			//== bone pose ==--
			
			Vector3    position    = new Vector3(x,y,z);
			Quaternion orientation = new Quaternion(qx,qy,qz,qw);
			
			//== locate or create bone object ==--
			
			string objectName = "Body"+id.ToString();
				
			GameObject body;
			
			body = GameObject.Find(objectName);
			
			if(body==null)
			{
				body = GameObject.CreatePrimitive(PrimitiveType.Cube);
				Vector3 scale = new Vector3(0.1f,0.1f,0.1f);
				body.transform.localScale = scale;
				body.name = objectName;
			}		
			
			//== set bone's pose ==--
			
			body.transform.position = position;
			body.transform.rotation = orientation;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
