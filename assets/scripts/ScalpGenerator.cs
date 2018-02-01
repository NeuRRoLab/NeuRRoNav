using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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
            if (waitingToDraw && (Input.GetKeyDown(KeyCode.Space)) && !releaseSpace)
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



    void FindLandmarks()
    {
        if (Input.GetKeyDown(KeyCode.Space) || (Input.GetKeyDown(KeyCode.Mouse1) && stylusTracking.color.Equals(Color.green)))
        {
            setLandmark(landmarkIndex);
            landmarkIndex++;
            if (landmarkIndex == 5)
            {
                waitingToDraw = true;
                settingLandmarks = false;
                calibrationInstruct.text = "";
                CenterHead();
                FindObjectOfType<Stylus>().setStylusSensitiveTrackingState(false);
                return;
            }
                
            calibrationInstruct.text = "Select " + LandmarkIndexToName(landmarkIndex);
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
        else
        {
            if (scalp.transform.localScale != scalpStartScale)
            {
                scalp.transform.localScale = scalpStartScale;
            }

            //Vector3 centeredX = Vector3.Lerp(landmarks[(int)landmarkNames.leftEar].transform.position, landmarks[(int)landmarkNames.rightEar].transform.position, (float)0.5);
            //Vector3 centeredZ = Vector3.Lerp(landmarks[(int)landmarkNames.forehead].transform.position, landmarks[(int)landmarkNames.backOfHead].transform.position, (float)0.5);

            //scalp.transform.position = new Vector3(centeredX.x, scalp.transform.position.y, centeredZ.z);

            scalp.transform.rotation = head.transform.rotation;
            nasion.transform.parent = head.transform;
            scalp.transform.parent = nasion.transform;
            nasion.transform.position = landmarks[(int)landmarkNames.nasion].transform.position;
            scalp.transform.parent = head.transform;
            nasion.transform.parent = scalp.transform;

            //foreach (GameObject obj in landmarks)
            //{
            //    obj.transform.parent = scalp.transform;
            //}

            float scaleZ = (Vector3.Distance(new Vector3(0, 0, landmarks[(int)landmarkNames.inion].transform.localPosition.z), new Vector3(0, 0, landmarks[(int)landmarkNames.nasion].transform.localPosition.z))
                / Vector3.Distance(new Vector3(0, 0, inion.transform.localPosition.z), new Vector3(0, 0, nasion.transform.localPosition.z)));

            float scaleX = (Vector3.Distance(new Vector3(landmarks[(int)landmarkNames.leftTragus].transform.localPosition.x, 0, 0), new Vector3(landmarks[(int)landmarkNames.rightTragus].transform.localPosition.x, 0, 0))
                / Vector3.Distance(new Vector3(lTragus.transform.localPosition.x, 0, 0), new Vector3(rTragus.transform.localPosition.x, 0, 0)));

            float scaleY = (Vector3.Distance(new Vector3(0, landmarks[(int)landmarkNames.nasion].transform.localPosition.y, 0), new Vector3(0, landmarks[(int)landmarkNames.aproxVertex].transform.localPosition.y, 0))
                / Vector3.Distance(new Vector3(0, nasion.transform.localPosition.y, 0), new Vector3(0, vertex.transform.localPosition.y, 0)));

            Debug.Log(scaleX.ToString() + " " + scaleY.ToString() + " " + scaleZ.ToString());

            foreach (GameObject obj in landmarks)
            {
                obj.transform.parent = head.transform;
            }

            scalp.transform.localScale = new Vector3((scalp.transform.localScale.x * scaleX), (scalp.transform.localScale.y * scaleY), (scalp.transform.localScale.z * scaleZ));

            nasion.transform.parent = head.transform;
            scalp.transform.parent = nasion.transform;
            nasion.transform.position = landmarks[(int)landmarkNames.nasion].transform.position;
            scalp.transform.parent = head.transform;
            nasion.transform.parent = scalp.transform;

            if (center != null)
            {
                Destroy(center);
            }

            Vector3 lerpCenter = Vector3.Lerp(Vector3.Lerp(landmarks[(int)landmarkNames.leftTragus].transform.position, landmarks[(int)landmarkNames.rightTragus].transform.position, 0.5F), landmarks[(int)landmarkNames.nasion].transform.position, 0.5f);

            center = new GameObject();
            center.name = "Center";
            center.transform.position = lerpCenter;
            //center.transform.rotation = head.transform.rotation;
            center.transform.rotation = Quaternion.LookRotation(
                Vector3.Normalize(landmarks[(int)landmarkNames.nasion].transform.position - lerpCenter), 
                Vector3.Normalize(
                    Vector3.Cross(Vector3.Normalize(landmarks[(int)landmarkNames.nasion].transform.position - lerpCenter), 
                        Vector3.Normalize(Vector3.Normalize(landmarks[(int)landmarkNames.leftTragus].transform.position - landmarks[(int)landmarkNames.rightTragus].transform.position)))));
            center.transform.parent = head.transform;

            //}

            camController.centerMainOnObject(head, 0.5F);

            GameObject.Find("Set Hot Spot").GetComponent<Button>().interactable = true;
            GameObject.Find("Add Points").GetComponent<Button>().interactable = true;
            GameObject.Find("Load Grids").GetComponent<Button>().interactable = true;
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
        if(landmarks != null)
        {
            for (int i = 0; i < 5; i++)
            {
                if (landmarks[i] != null)
                {
                    DestroyImmediate(landmarks[i]);
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

        calibrationInstruct.text = "Select Nasion";
    }

    public void LandmarkButtonPress(int landmark)
    {
        if(landmarks[landmark] != null)
        {
            DestroyImmediate(landmarks[landmark]);
            landmarks[landmark] = new GameObject();
            landmarks[landmark].transform.position = head.transform.position;
            landmarks[landmark].transform.rotation = head.transform.rotation;
            landmarks[landmark].transform.parent = head.transform;
        }
        setLandmark(landmark);
        if(landmarkIndex >= 5)
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
}
