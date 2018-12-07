using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class ScalpMeshMenuController : MonoBehaviour {
    bool activeMesh = false;
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
    public Button activationkey;
    // Use this for initialization
    void Start () {
        activationkey = GameObject.Find("Scalp Mesh").GetComponent<Button>();
        inactivepos = new Vector3(680,98,0);
        activepos = GameObject.Find("CenterScreen").GetComponent<RectTransform>().position;//new Vector3(-934,90,0);
        myrect = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
        if (isactive) {
            if (!activationkey.interactable) {
                activationKey();
            }
        }
        if (STATE == "GENHEAD")
        {
            GameObject.Find("CalibrationInstructions").GetComponent<Text>().text = "PRESS RMB AND DRAG, then Space";
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
                    try
                    {
                        int index = (verticeslong * closestx) + closesty;

                        vertices[index] = new Vector3(vertices[index].x, vertices[index].y, localpos.z);
                        filter.mesh.vertices = vertices;
                        filter.mesh.RecalculateNormals();
                    }
                    catch {
                        // Means minor rounding error that causes out of bounds issue 
                    }
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
                bool changedany = false;
                for (int x = 0; x < verticeswide; x += 1)
                {
                    for (int y = 0; y < verticeslong; y += 1)
                    {
                        if (vertices[(verticeslong * x) + y] == new Vector3(base_x + x * gridspacing, base_y + y * gridspacing, -0.5f)) {
                            vertices[(verticeslong * x) + y] = Vector3.zero;
                            changedany = true;
                        }
                        
                    }
                }
                
                activeMesh = true;
                filter.mesh.vertices = vertices;
                filter.mesh.RecalculateNormals();
                planeObj.GetComponent<MeshCollider>().sharedMesh = filter.mesh;

                if (GameObject.Find("Toggle_SavePrompts").GetComponent<Toggle>().isOn)
                {
                    if (AskIfToSave())
                    {
                        saveHeadMeshToFile();
                    }
                }
                GameObject.Find("Save Scalp").GetComponent<Button>().interactable = true;

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
            activepos = GameObject.Find("CenterScreen").GetComponent<RectTransform>().position;
            myrect.position = activepos;
        }
    }
    public void ClearHeadMesh() {
        // Clear the stuff
        if ((filter != null) && (filter.mesh!=null))
        {
            filter.mesh.triangles = new int[0];
            filter.mesh.normals = new Vector3[0];
            filter.mesh.vertices = new Vector3[0];
            
            

            filter.mesh.RecalculateNormals();
            planeObj.GetComponent<MeshCollider>().sharedMesh = filter.mesh;
        }
        // Disable Saving
        GameObject.Find("Save Scalp").GetComponent<Button>().interactable = false;
    }

    public void loadHeadMeshFromFile() {
        // Don't save if we are currently making one
        if (STATE != "INACTIVE")
        {
            return;
        }
        Vector3[] vertices = new Vector3[0];
        Vector3[] loadedvertices = new Vector3[0];
        int totalverts;
        try
        {
            // Load the data we want
            string path = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.scalpMeshLoadPath);
            string fileName = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.scalpMeshLoadName);

            path += fileName;

            System.IO.FileStream filestream = new System.IO.FileStream(path,
                                                 System.IO.FileMode.Open,
                                                 System.IO.FileAccess.Read,
                                                 System.IO.FileShare.Read);
            System.IO.StreamReader file = new System.IO.StreamReader(filestream);

            // Load planePoint pos/rot
            string data = file.ReadLine();

            // planepoint pos
            string[] dims = data.Split('\t');
            planePoint.localPosition = new Vector3((float)System.Convert.ToDouble(dims[0]), (float)System.Convert.ToDouble(dims[1]), (float)System.Convert.ToDouble(dims[2]));
            // planepoint rot
            data = file.ReadLine();
            dims = data.Split('\t');
            planePoint.localRotation = new Quaternion((float)System.Convert.ToDouble(dims[1]), (float)System.Convert.ToDouble(dims[2]), (float)System.Convert.ToDouble(dims[3]), (float)System.Convert.ToDouble(dims[0]));

            // Width and length
            width = (float)System.Convert.ToDouble(file.ReadLine());
            length = (float)System.Convert.ToDouble(file.ReadLine());

            // Generate verts
            verticeswide = System.Convert.ToInt32(width / gridspacing);
            verticeslong = System.Convert.ToInt32(length / gridspacing);
            totalverts = verticeslong * verticeswide;
            vertices = new Vector3[totalverts];
            loadedvertices = new Vector3[totalverts];

            for (int i = 0; i < totalverts; ++i) {
                data = file.ReadLine();
                dims = data.Split('\t');
                loadedvertices[i] = new Vector3((float)System.Convert.ToDouble(dims[0]), (float)System.Convert.ToDouble(dims[1]), (float)System.Convert.ToDouble(dims[2]));

            }

            file.Close();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return;
        }
        // Now initialize as before
        // Now to create our mesh
        // start from back left
        filter = planeObj.GetComponent<MeshFilter>();
        filter.mesh.Clear();

        
        
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

        filter.mesh.vertices = loadedvertices;
        filter.mesh.RecalculateNormals();
        planeObj.GetComponent<MeshCollider>().sharedMesh = filter.mesh;
        GameObject.Find("Save Scalp").GetComponent<Button>().interactable = true;

    }
    public void saveHeadMeshToFile(){



        // Don't save if there is no mesh, or if we are currently making one
        if ((!activeMesh) || (STATE != "INACTIVE")) {
            return;
        }
        try
        {
            string path = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.scalpMeshSavePath);
            string fileName = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.scalpMeshSaveName);

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
                // Write planePoint position,rotation in subsequent lines
                file.WriteLine(planePoint.localPosition.x + "\t" + planePoint.localPosition.y + "\t" + planePoint.localPosition.z);
                file.WriteLine(planePoint.localRotation.w + "\t" + planePoint.localRotation.x + "\t" + planePoint.localRotation.y + "\t" + planePoint.localRotation.z);

                // Then write width and height
                file.WriteLine(width);
                file.WriteLine(length);

                // Now store the vertices vector
                filter = planeObj.GetComponent<MeshFilter>();

                Vector3[] vertices = filter.mesh.vertices;
                for (int i = 0; i < vertices.Length; ++i) {
                    file.WriteLine(vertices[i].x + "\t" + vertices[i].y + "\t" + vertices[i].z);
                }
            }

            //GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().incrementField((int)SettingsMenu.settings.scalpMeshSaveName);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
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
        activeMesh = false;
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

        width = Vector3.Distance(lefthead.position, midpoint) * 6f;
        length = Vector3.Distance(nose.position, midpoint) * 6f;

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

    bool PromptOverwrite()
    {
        using (var form1 = new System.Windows.Forms.Form())
        {
            System.Windows.Forms.Label text = new System.Windows.Forms.Label();
            System.Windows.Forms.Button button1 = new System.Windows.Forms.Button();
            System.Windows.Forms.Button button3 = new System.Windows.Forms.Button();
            System.Windows.Forms.Button buttondefault = new System.Windows.Forms.Button();
            buttondefault.Location = new System.Drawing.Point(-2000, -2000);


            text.Text = text.Text = "A file exists at the Scalp Mesh Save location specified! \nDo you want to overwrite?\n\nIf not: Cancel, then edit the Save Scalp Mesh Field, \nthen Save Manually.";
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

            text.Text = "Successfully calibrated Scalp Mesh! Do you want to\n save to Scalp Mesh Save location?";
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
