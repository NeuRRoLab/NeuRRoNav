using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class ScalpGenerator : MonoBehaviour
{

    CameraController camController;
    SurfaceGen surfaceGen;

    public bool waitingToDraw;
    bool drawing;
    bool settingLandmarks;
    bool releaseSpace;

    int splines;
    int splinePoints;

   
    GameObject[] landmarks;
    enum landmarkNames { nasion = 0, leftTragus = 1, rightTragus = 2, inion = 4, aproxVertex = 3 };
    int landmarkIndex;

    Vector3 lastPoint;

    //IList<IList<Vector3>> splineCage;

    LineRenderer splineRenderer;
    GameObject stylusPoint;
    GameObject head;
    GameObject scalp;
    GameObject scalpSpline;
    GameObject center;
    Text stylusTracking;
    Text headTracking;
    Text calibrationInstruct;

    GameObject nasion;
    GameObject inion;
    GameObject lTragus;
    GameObject rTragus;
    GameObject vertex;

    Vector3 scalpStartScale;

    // If this is enabled, then an average of 5 clicks for each landmark are used. 
    bool averageMode = false;
    int clicks_per_landmark = 1;
    int clicks_per_landmark_placehold = 0;
    Vector3 currentlandmark_runningaverage = Vector3.zero;

    void Start()
    {

        surfaceGen = GameObject.Find("ScalpSurface").GetComponent<SurfaceGen>();
        camController = GameObject.Find("Camera Controller").GetComponent<CameraController>();
        stylusTracking = GameObject.Find("StylusTrackStatus").GetComponent<Text>();
        headTracking = GameObject.Find("HeadTrackStatus").GetComponent<Text>();
        calibrationInstruct = GameObject.Find("CalibrationInstructions").GetComponent<Text>();

        nasion = GameObject.Find("Nasion");
        inion = GameObject.Find("Inion");
        lTragus = GameObject.Find("Left Tragus");
        rTragus = GameObject.Find("Right Tragus");
        vertex = GameObject.Find("Aprox Vertex");

        scalpStartScale = new Vector3();

        scalp = GameObject.Find("Scalp");
        scalpStartScale = scalp.transform.localScale;
        //splines = 0;
        //splinePoints = 0;
        //waitingToDraw = false;
        settingLandmarks = false;
        //drawing = false;
        //splineCage = new List<IList<Vector3>>();
    }

    // Update is called once per frame
    void Update()
    {
        if (settingLandmarks)
        {
            FindLandmarks();
        }
        else
        {
            if (waitingToDraw && Utility.AnyInputDown() && !releaseSpace)
            {
                //StartDraw();
            }
            //if (drawing)
            //{
            //    if (Input.GetKey(KeyCode.Space))
            //    {
            //        if (stylusTracking.color == Color.red)
            //        {
            //            splines--;
            //            Destroy(scalpSpline);
            //            splineCage.RemoveAt(splines);
            //            drawing = false;
            //            waitingToDraw = true;
            //            releaseSpace = true;
            //            stylusTracker.GetComponent<Stylus>().setStylusSensitiveTrackingState(false);
            //        }
            //        else if (Vector3.Distance(lastPoint, stylusPoint.transform.position) > 0.005)
            //        {
            //            //splinePoints = DrawNewVert(splinePoints);
            //            lastPoint = stylusPoint.transform.position;
            //        }
            //    }

            //    else if (!Input.GetKey(KeyCode.Space) || !Input.GetKey(KeyCode.Mouse1))
            //    {
            //        drawing = false;
            //        waitingToDraw = true;
            //        stylusTracker.GetComponent<Stylus>().setStylusSensitiveTrackingState(false);
            //    }
            //}
            //else if (releaseSpace && (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Mouse1)))
            //{
            //    releaseSpace = false;
            //}

        }

        //if (Input.GetKeyDown(KeyCode.D))
        //{
        //    ExportScalpSurfaceXYZ();
        //}
    }
    public void ToggleAverageMode() {
        if (GameObject.Find("AverageLandmarksToggle").GetComponent<Toggle>().isOn)
        {
            averageMode = true;
            clicks_per_landmark = System.Convert.ToInt32(GameObject.Find("NumberOfClicksField").GetComponent<InputField>().text);
        }
        else {
            averageMode = false;
            clicks_per_landmark = 1;
        }
    }
    void FindLandmarks()
    {
		if (Input.anyKeyDown && stylusTracking.color.Equals(Color.green) && (!Input.GetMouseButton(0)))
        {
            stylusPoint = GameObject.Find("Stylus").transform.Find("Point").gameObject;
            head = GameObject.Find("Head");
            // Keep a running average of all points
            currentlandmark_runningaverage += stylusPoint.transform.position;

            clicks_per_landmark_placehold++;
            
            if (clicks_per_landmark_placehold >= clicks_per_landmark)
            {
                // Have collected all 5 points, get their average, keep on rolling
                setLandmarkFromVector(landmarkIndex,currentlandmark_runningaverage*((float)1/(float)clicks_per_landmark));

                // reset used variables
                currentlandmark_runningaverage = Vector3.zero;
                clicks_per_landmark_placehold = 0;

                landmarkIndex++;
                if (landmarkIndex == 5)
                {
                    // We have set all landmarks, and are done.
                    ExportLandmarks();
                    waitingToDraw = true;
                    settingLandmarks = false;
                    calibrationInstruct.text = "";
                    CenterHead();
                    FindObjectOfType<Stylus>().setStylusSensitiveTrackingState(false);
                    GameObject.Find("AverageLandmarksToggle").GetComponent<Toggle>().interactable = true;
                    GameObject.Find("NumberOfClicksField").GetComponent<InputField>().interactable = true;
                    return;
                }
            }
            if (!averageMode)
            {
                calibrationInstruct.text = "Select " + LandmarkIndexToName(landmarkIndex);
            }
            else {
                calibrationInstruct.text = "Select " + LandmarkIndexToName(landmarkIndex) + ", Iteration: "+clicks_per_landmark_placehold.ToString();
            }
        }
    }

	string LandmarkIndexToName(int landmarkIndex) {
		switch (landmarkIndex) {
			case 0:
				return "Nasion";
			case 1:
                return "Right Tragus";
            case 2:
                return "Left Tragus";
            case 3:
                return "Aprox Vertex";
            case 4:
                return "Inion";
		}
		return "Index Error";
	}

    void setLandmarkFromVector(int index, Vector3 pos) {
        head = GameObject.Find("Head");
        Debug.Log(landmarks[index].name);
        landmarks[index].transform.position = pos;
        landmarks[index].transform.parent = head.transform;
    }

    void setLandmark(int index)
    {
        stylusPoint = GameObject.Find("Stylus").transform.Find("Point").gameObject;
        head = GameObject.Find("Head");

        landmarks[index].transform.position = stylusPoint.transform.position;
		landmarks[index].transform.parent = head.transform;
    }

    //void StartDraw()
    //{
    //    stylusTracker.GetComponent<Stylus>().setStylusSensitiveTrackingState(true);
    //    waitingToDraw = false;
    //    splines++;
    //    Debug.Log("Space pressed, drawing spline");

    //    scalpSpline = new GameObject();
    //    scalpSpline.name = "spline_" + splines.ToString();
    //    scalpSpline.tag = "Spline";
    //    scalpSpline.transform.position = scalp.transform.position;
    //    scalpSpline.transform.parent = scalp.transform;

    //    splineRenderer = scalpSpline.AddComponent<LineRenderer>();
    //    splineRenderer.useWorldSpace = false;
    //    splineRenderer.material = new Material(Shader.Find("Diffuse"));
    //    splineRenderer.material.color = Color.green;
    //    splineRenderer.receiveShadows = false;
    //    splineRenderer.SetWidth((float)0.001, (float)0.001);

    //    splineCage.Add(new List<Vector3>());

    //    splinePoints = 0;

    //    drawing = true;

    //    lastPoint = stylusPoint.transform.position;

    //}

    //int DrawNewVert(int point)
    //{
    //    stylusPoint = GameObject.Find("Stylus").transform.FindChild("Point").gameObject;
    //    Debug.Log("Drawing");
    //    int points = point + 1;
    //    splineRenderer.SetVertexCount(points);
    //    splineRenderer.SetPosition(points - 1, splineRenderer.transform.InverseTransformVector(stylusPoint.transform.position - splineRenderer.transform.position));

    //    splineCage[splines - 1].Add(stylusPoint.transform.position);

    //    return points;
    //}

    void CenterHead()
    {
        head = GameObject.Find("Head");
        if (scalp == null)
        {
            scalp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Vector3 scale = new Vector3(0.1f, 0.1f, 0.1f);
            scalp.transform.localScale = scale;
            scalp.name = "Scalp";

            //Vector3 centeredPos = new Vector3();
            //centeredPos.x = (landmarks[(int)landmarkNames.leftEar].x + (landmarks[(int)landmarkNames.rightEar].x - landmarks[(int)landmarkNames.leftEar].x) / 2);
            //centeredPos.y = (landmarks[(int)landmarkNames.forehead].y + (landmarks[(int)landmarkNames.topOfHead].y - landmarks[(int)landmarkNames.forehead].y) / 2);
            //centeredPos.z = (landmarks[(int)landmarkNames.forehead].z + (landmarks[(int)landmarkNames.backOfHead].z - landmarks[(int)landmarkNames.forehead].z) / 2);
            //scalp.transform.position = centeredPos;
            //scalp.transform.parent = head.transform;
        }
        else {
            // If we are recreating the center, ie it already existed, undo the existing center object. 
            // First, do any cameras currently point here?
            for (int i = 0; i < 3; ++i) {
                if (camController.targets[i] == center) {
                    camController.putCamOnStylus(i);
                }
            }
			if (center != null) {
				Transform[] children = center.GetComponentsInChildren<Transform>();
				foreach (Transform child in children) {
					if (child.transform.parent == this.transform)
						child.transform.parent = center.transform.parent;
				}
                center.name = "junk";
				Destroy(center);
                
			}

			// Find the center of the three most important points
			// The nasion, right tragus, and left tragus
			Vector3 threePointCenter = 
				landmarks[(int)landmarkNames.nasion].transform.position +
				landmarks[(int)landmarkNames.leftTragus].transform.position +
				landmarks[(int)landmarkNames.rightTragus].transform.position;
			threePointCenter /= 3;

			// Create the center at the landmark important center
			center = new GameObject();
			center.name = "Center";
			center.transform.position = threePointCenter;

			// Set the rotation such that the forward vector points to the nasion and the right vector points to the right tragus
			center.transform.rotation = Utility.ThreePointLocalSpaceConversion(
				landmarks[(int)landmarkNames.nasion].transform.position,
				landmarks[(int)landmarkNames.rightTragus].transform.position,
				landmarks[(int)landmarkNames.leftTragus].transform.position);

			// Parent the center object to the head tracker
			center.transform.parent = head.transform;

			// Parent the landmarks to the center to make calculations easier
			foreach (GameObject landmark in landmarks) {
				landmark.transform.parent = center.transform;
			}

			// Setup the scalp to use similair local coordinates to the center
			scalp.transform.localScale = scalpStartScale;
			scalp.GetComponent<CenterScalp>().Center();

			// Find the new scalp scale
			Vector3 newScalpScale = Vector3.one;
			newScalpScale.x =
				Mathf.Abs(landmarks[(int)landmarkNames.leftTragus].transform.localPosition.x - landmarks[(int)landmarkNames.rightTragus].transform.localPosition.x) /
				Mathf.Abs(lTragus.transform.localPosition.x - rTragus.transform.localPosition.x);
			newScalpScale.z =
				Mathf.Abs(landmarks[(int)landmarkNames.nasion].transform.localPosition.z - landmarks[(int)landmarkNames.inion].transform.localPosition.z) /
				Mathf.Abs(nasion.transform.localPosition.z - inion.transform.localPosition.z);

            //newScalpScale.y = (newScalpScale.x + newScalpScale.z) / 2;
            newScalpScale.y =
                Mathf.Abs(landmarks[(int)landmarkNames.aproxVertex].transform.localPosition.y ) /
                Mathf.Abs(vertex.transform.localPosition.y);


            // Move the scalp under center and setup the transform properties
            scalp.transform.parent = center.transform;
			scalp.transform.localPosition = Vector3.zero;
			scalp.transform.localRotation = Quaternion.identity;
			scalp.transform.localScale = newScalpScale;

			// Set the camera to view the front of the head
			camController.centerMainOnObject(head, -center.transform.forward, 0.5F);

			GameObject.Find("Set Hot Spot").GetComponent<Button>().interactable = true;
			//GameObject.Find("Add Points").GetComponent<Button>().interactable = true;
			GameObject.Find("Load Grids").GetComponent<Button>().interactable = true;
            GameObject.Find("Scalp Mesh").GetComponent<Button>().interactable = true;
            foreach (Button b in GameObject.Find("LandmarksList").GetComponentsInChildren<Button>())
            {
                b.interactable = true;
            }
            // camController.putMainCam1FacingBackOfCoil(tPoint.pos);
            //  camController.putTargetCam1OnTargetXZ(tPoint.pos);
            // camController.putTargetCam2OnTargetZY(tPoint.pos);
        }
    }

    //public void ExportScalpSurfaceXYZ()
    //{
    //    GameObject scalp = GameObject.Find("Scalp");
    //    string path = Application.dataPath + @"\Scalps";
    //    if (!System.IO.File.Exists(path))
    //    {
    //        System.IO.Directory.CreateDirectory(path);
    //    }

    //    using (System.IO.StreamWriter file =
    //        new System.IO.StreamWriter(path + @"\Scalp.txt", true))
    //    {
    //        file.WriteLine(splineCage.Count.ToString());
    //        file.WriteLine(splineCage[0].Count.ToString());
    //        foreach (List<Vector3> line in splineCage)
    //        {
    //            foreach (Vector3 point in line)
    //            {
    //                Vector3 v = center.transform.InverseTransformPoint(GameObject.Find("ScalpSurface").transform.TransformPoint(point));
    //                file.WriteLine(v.x + "\t" + v.y + "\t" + v.z);
    //            }
    //        }
    //        //file.WriteLine(
    //    }
    //}

    //public void ImportXYZ()
    //{
    //    List<IList<Vector3>> newSplineCage = new List<IList<Vector3>>();

    //    System.IO.FileStream filestream = new System.IO.FileStream(Application.dataPath + @"\Scalps\Scalp.txt",
    //                                      System.IO.FileMode.Open,
    //                                      System.IO.FileAccess.Read,
    //                                      System.IO.FileShare.Read);
    //    System.IO.StreamReader file = new System.IO.StreamReader(filestream);


    //    int splines = System.Convert.ToInt32(file.ReadLine());
    //    int points = System.Convert.ToInt32(file.ReadLine());

    //    for (int i = 0; i < splines; i++)
    //    {
    //        newSplineCage.Add(new List<Vector3>());
    //        for (int j = 0; j < points; j++)
    //        {
    //            Vector3 v = new Vector3();
    //            string vertex = file.ReadLine();
    //            char[] d = new char[1];
    //            d[0] = '\t';
    //            string[] dims = vertex.Split(d);
    //            v.Set((float)System.Convert.ToDouble(dims[0]), (float)System.Convert.ToDouble(dims[1]), (float)System.Convert.ToDouble(dims[2]));
    //            newSplineCage[i].Add(center.transform.TransformPoint(v));
    //        }
    //    }
    //    file.Close();
    //    splineCage = newSplineCage;
    //    createGrid();
    //}

    public void LandmarksButtonPress()
    {
        head = GameObject.Find("Head");
        FindObjectOfType<Stylus>().setStylusSensitiveTrackingState(true);
        if (averageMode) {
            clicks_per_landmark = System.Convert.ToInt32(GameObject.Find("NumberOfClicksField").GetComponent<InputField>().text);
        }
        GameObject.Find("AverageLandmarksToggle").GetComponent<Toggle>().interactable = false;
        GameObject.Find("NumberOfClicksField").GetComponent<InputField>().interactable = false;
        if (landmarks != null)
        {
            for (int i = 0; i < 5; i++)
            {
                if (landmarks[i] != null)
                {
                    Destroy(landmarks[i]);
                }
            }
        }
        landmarks = new GameObject[5];
        for (int i = 0; i < 5; i++)
        {
            landmarks[i] = new GameObject();
            landmarks[i].transform.position = head.transform.position;
            landmarks[i].transform.rotation = head.transform.rotation;
            landmarks[i].transform.parent = head.transform;
			landmarks[i].name = LandmarkIndexToName(i);

			DebugPoint debugPoint = landmarks[i].AddComponent<DebugPoint>();
			debugPoint.p = PrimitiveType.Cube;
			if (i == 0)
				debugPoint.c = Color.red;
			else if (i == 1)
				debugPoint.c = Color.yellow;
			else if (i == 2)
				debugPoint.c = Color.green;
			else if (i == 3)
				debugPoint.c = Color.cyan;
			else if (i == 4)
				debugPoint.c = Color.magenta;


		}
        landmarkIndex = 0;
        settingLandmarks = true;

        if (!averageMode)
        {
            calibrationInstruct.text = "Select Nasion";
        }
        else
        {
            calibrationInstruct.text = "Select Nasion, Iteration 0"; 
        }

    }

    public void LandmarkButtonPress(int landmark)
    {
        setLandmark(landmark);
        CenterHead();
    }

    //public void GenScalpButtonPress()
    //{
    //    createGrid();
    //}

    //void createGrid()
    //{
    //    int splines = 0;
    //    int highestResolution = 0;
    //    int j = 0;
    //    foreach (List<Vector3> line in splineCage)
    //    {
    //        splines++;
    //        foreach (Vector3 point in line)
    //        {
    //            j++;
    //            if (j > highestResolution)
    //            {
    //                highestResolution = j;
    //            }
    //        }
    //        j = 0;
    //    }

    //    Vector3[,] scalpGrid = new Vector3[splines, highestResolution];
    //    //IList<Vector3> scalpGrid = new List<Vector3>();

    //    foreach (List<Vector3> line in splineCage)
    //    {
    //        while (line.Count < highestResolution)
    //        {
    //            float distance = 0;
    //            Vector3 v1, v2, f1, f2;
    //            v1 = line[0];
    //            v2 = line[1];
    //            f1 = line[0];
    //            f2 = line[1];
    //            bool first = true;
    //            int k = 0;
    //            int l = 0;
    //            foreach (Vector3 point in line)
    //            {
    //                if (first)
    //                {
    //                    first = false;
    //                }
    //                else
    //                {
    //                    v2 = point;
    //                    float d = Vector3.Distance(v1, v2);
    //                    if (d > distance)
    //                    {
    //                        f1 = v1;
    //                        f2 = v2;
    //                        distance = d;
    //                        l = k;
    //                    }
    //                    v1 = point;
    //                }
    //                k++;
    //            }
    //            line.Insert(l, Vector3.Lerp(f1, f2, 0.5F));
    //        }
    //    }

    //    int i = 0;
    //    for (i = 0; i < splineCage.Count; i++)
    //    {
    //        //float nextSpline = landmarks[(int)landmarkNames.inion].transform.position.z;
    //        //int index = 0;
    //        //int nextIndex = 0;
    //        //foreach (IList<Vector3> list in splineCage)
    //        //{
    //        //    if (scalp.transform.TransformPoint(list[0]).z < nextSpline)
    //        //    {
    //        //        nextSpline = scalp.transform.TransformPoint(list[0]).z;
    //        //        nextIndex = index;
    //        //        index++;
    //        //    }
    //        //}

    //        for (j = 0; j < highestResolution; j++)
    //        {
    //            if (j < splineCage[i].Count)
    //            {
    //                Debug.Log("Filling at " + i.ToString() + " " + j.ToString());
    //                scalpGrid[i, j] = splineCage[i][j];
    //                //scalpGrid.Add(splineCage[i][j]);
    //            }
    //        }
    //        //splineCage.RemoveAt(nextIndex);
    //    }

    //    Vector3[,] orderedScalpGrid = new Vector3[splines, highestResolution];

    //    Debug.Log("Setting control grid to " + splineCage.Count.ToString() + " x " + highestResolution.ToString());

    //    surfaceGen.CreateScalp(scalpGrid, splineCage.Count, highestResolution);

    //    GameObject[] allSplines = GameObject.FindGameObjectsWithTag("Spline");

    //    foreach (GameObject s in allSplines)
    //    {
    //        Destroy(s);
    //    }
    //}

    public void ImportLandmarks()
    {
        head = GameObject.Find("Head");
        FindObjectOfType<Stylus>().setStylusSensitiveTrackingState(true);
        if (averageMode)
        {
            clicks_per_landmark = System.Convert.ToInt32(GameObject.Find("NumberOfClicksField").GetComponent<InputField>().text);
        }
        GameObject.Find("AverageLandmarksToggle").GetComponent<Toggle>().interactable = false;
        GameObject.Find("NumberOfClicksField").GetComponent<InputField>().interactable = false;
        if (landmarks != null)
        {
            for (int i = 0; i < 5; i++)
            {
                if (landmarks[i] != null)
                {
                    Destroy(landmarks[i]);
                }
            }
        }
        landmarks = new GameObject[5];
        for (int i = 0; i < 5; i++)
        {
            landmarks[i] = new GameObject();
            landmarks[i].transform.position = head.transform.position;
            landmarks[i].transform.rotation = head.transform.rotation;
            landmarks[i].transform.parent = head.transform;
            landmarks[i].name = LandmarkIndexToName(i);

            DebugPoint debugPoint = landmarks[i].AddComponent<DebugPoint>();
            debugPoint.p = PrimitiveType.Cube;
            if (i == 0)
                debugPoint.c = Color.red;
            else if (i == 1)
                debugPoint.c = Color.yellow;
            else if (i == 2)
                debugPoint.c = Color.green;
            else if (i == 3)
                debugPoint.c = Color.cyan;
            else if (i == 4)
                debugPoint.c = Color.magenta;


        }

        string path = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.landmarkLoadPath);
        print(path);
        string fileName = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.landmarkLoadName);

        try
        {
            System.IO.FileStream filestream = new System.IO.FileStream(path + "/" + fileName,
                                              System.IO.FileMode.Open,
                                              System.IO.FileAccess.Read,
                                              System.IO.FileShare.Read);
            System.IO.StreamReader file = new System.IO.StreamReader(filestream);

            for (int i = 0; i < 5; ++i) {
                string line = file.ReadLine();
                var elements = line.Split('\t');
                Vector3 pos = head.transform.position + new Vector3(float.Parse(elements[1]), float.Parse(elements[2]), float.Parse(elements[3]));
                switch (i) {
                    case 0:
                        setLandmarkFromVector((int)landmarkNames.nasion, pos);
                        break;
                    case 1:
                        setLandmarkFromVector((int)landmarkNames.rightTragus, pos);
                        break;
                    case 2:
                        setLandmarkFromVector((int)landmarkNames.leftTragus, pos);
                        break;
                    case 3:
                        setLandmarkFromVector((int)landmarkNames.aproxVertex, pos);
                        break;
                    case 4:
                        setLandmarkFromVector((int)landmarkNames.inion, pos);
                        break;
                }
            }

            

            file.Close();
        }
        catch (Exception e)
        {
            print(e);
            //tell user something went wrong
            return;
        }
        // We have set all landmarks, and are done.
        waitingToDraw = true;
        settingLandmarks = false;
        calibrationInstruct.text = "";
        CenterHead();
        FindObjectOfType<Stylus>().setStylusSensitiveTrackingState(false);
        GameObject.Find("AverageLandmarksToggle").GetComponent<Toggle>().interactable = true;
        GameObject.Find("NumberOfClicksField").GetComponent<InputField>().interactable = true;
    }

    public void ExportLandmarks()
    {
        head = GameObject.Find("Head");
        GameObject center = GameObject.Find("Center");

        string path = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.landmarkSavePath);
        string fileName = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.landmarkSaveName);

        path += fileName;

        using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(path, false))
        {
            Vector3 pos = landmarks[(int)landmarkNames.nasion].transform.position - head.transform.position;
            file.WriteLine("Nasion:\t"+pos.x.ToString()+'\t' + pos.y.ToString() + '\t' + pos.z.ToString() + '\t');
            pos = landmarks[(int)landmarkNames.rightTragus].transform.position - head.transform.position;
            file.WriteLine("rTragus:\t" + pos.x.ToString() + '\t' + pos.y.ToString() + '\t' + pos.z.ToString() + '\t');
            pos = landmarks[(int)landmarkNames.leftTragus].transform.position - head.transform.position;
            file.WriteLine("lTragus:\t" + pos.x.ToString() + '\t' + pos.y.ToString() + '\t' + pos.z.ToString() + '\t');
            pos = landmarks[(int)landmarkNames.aproxVertex].transform.position - head.transform.position;
            file.WriteLine("Vertex:\t" + pos.x.ToString() + '\t' + pos.y.ToString() + '\t' + pos.z.ToString() + '\t');
            pos = landmarks[(int)landmarkNames.inion].transform.position - head.transform.position;
            file.WriteLine("Inion:\t" + pos.x.ToString() + '\t' + pos.y.ToString() + '\t' + pos.z.ToString() + '\t');
        }

        GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().incrementField((int)SettingsMenu.settings.landmarkSaveName);
    }
}
