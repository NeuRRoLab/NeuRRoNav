using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ScalpGenerator : MonoBehaviour {

    CameraController CamController;
    SurfaceGen surfaceGen;
	
	bool waitingToDraw;
    bool drawing;
    bool landmarksFound;

    int splines;
	int splinePoints;

    GameObject[] landmarks;
    enum landmarkNames { forehead = 0, leftEar = 1, rightEar = 2, backOfHead = 4, topOfHead = 3 };
    int landmarkIndex;

    IList<IList<Vector3>> splineCage;

    LineRenderer splineRenderer;
    GameObject stylusPoint;
    GameObject head;
    GameObject scalp;
    GameObject scalpSpline;
    Text stylusTracking;
    Text headTracking;

	void Start () 
	{
        surfaceGen = GameObject.Find("ScalpSurface").GetComponent<SurfaceGen>();
        CamController = GameObject.Find("CameraController").GetComponent<CameraController>();

        Text stylusTracking = GameObject.Find("StylusTrackStatus").GetComponent<Text>();
        Text headTracking = GameObject.Find("HeadTrackStatus").GetComponent<Text>();

        splines = 0;
		splinePoints = 0;
		waitingToDraw = false;
        landmarksFound = true;
        drawing = false;
        splineCage = new List<IList<Vector3>>();
	}
	
	// Update is called once per frame
	void Update () 
	{
        if (!landmarksFound)
        {
            FindLandmarks();
        }
        else
        {
            if (waitingToDraw && Input.GetKeyDown(KeyCode.Space))
            {
                StartDraw();
            }
            else if(drawing && Input.GetKey(KeyCode.Space))
            {
                //if (stylusTracking.color == Color.green && stylusTracking.color == Color.green)
               // {
                    splinePoints = DrawNewVert(splinePoints);
                //}
            }
            else if(drawing && !Input.GetKey(KeyCode.Space))
            {
                drawing = false;
                waitingToDraw = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            ExportXYZ();
        }
    }



    void FindLandmarks()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            stylusPoint = GameObject.Find("Point");
            head = GameObject.Find("Head");

            landmarks[landmarkIndex].transform.position = stylusPoint.transform.position;
            landmarks[landmarkIndex].gameObject.name = landmarkIndex.ToString();
            landmarkIndex++;
            if (landmarkIndex == 5)
            {
                waitingToDraw = true;
                landmarksFound = true;
                CenterHead();
            }
        }
    }

    void StartDraw()
    {
            waitingToDraw = false;
            splines++;
            Debug.Log("Space pressed, drawing spline");

            scalpSpline = new GameObject();
            scalpSpline.name = "spline_" + splines.ToString();
            scalpSpline.transform.position = scalp.transform.position;
            scalpSpline.transform.parent = scalp.transform;

            splineRenderer = scalpSpline.AddComponent<LineRenderer>();
            splineRenderer.useWorldSpace = false;
            splineRenderer.material = new Material(Shader.Find("Diffuse"));
            splineRenderer.receiveShadows = true;
            splineRenderer.SetWidth((float)0.001, (float)0.001);

            splineCage.Add(new List<Vector3>());

            splinePoints = 0;

            drawing = true;
            
    }

	int DrawNewVert(int point)
	{
        stylusPoint = GameObject.Find("Point");
        Debug.Log("Drawing");
		int points = point + 1;
		splineRenderer.SetVertexCount(points);
		splineRenderer.SetPosition (points - 1, splineRenderer.transform.InverseTransformVector(stylusPoint.transform.position - splineRenderer.transform.position));

        splineCage[splines - 1].Add(stylusPoint.transform.position);

		return points;
	}

    void CenterHead()
    {
        head = GameObject.Find("Head");
        scalp = GameObject.Find("Scalp");
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
            GameObject nasion = GameObject.Find("Nasion");
            GameObject inion = GameObject.Find("Inion");
            GameObject lTragus = GameObject.Find("LTragus");
            GameObject rTragus = GameObject.Find("RTragus");
            GameObject vertex = GameObject.Find("Vertex");

            //Vector3 centeredX = Vector3.Lerp(landmarks[(int)landmarkNames.leftEar].transform.position, landmarks[(int)landmarkNames.rightEar].transform.position, (float)0.5);
            //Vector3 centeredZ = Vector3.Lerp(landmarks[(int)landmarkNames.forehead].transform.position, landmarks[(int)landmarkNames.backOfHead].transform.position, (float)0.5);

            //scalp.transform.position = new Vector3(centeredX.x, scalp.transform.position.y, centeredZ.z);

            scalp.transform.rotation = head.transform.rotation;
            nasion.transform.parent = head.transform;
            scalp.transform.parent = nasion.transform;
            nasion.transform.position = landmarks[(int)landmarkNames.forehead].transform.position;
            scalp.transform.parent = head.transform;
            nasion.transform.parent = scalp.transform;

            foreach (GameObject obj in landmarks)
            {
                obj.transform.parent = scalp.transform;
            }

            float scaleZ = (Vector3.Distance(new Vector3(0, 0, landmarks[(int)landmarkNames.backOfHead].transform.localPosition.z), new Vector3(0, 0, landmarks[(int)landmarkNames.forehead].transform.localPosition.z)) 
                / Vector3.Distance(new Vector3(0, 0, inion.transform.localPosition.z), new Vector3(0, 0, nasion.transform.localPosition.z)));
            
            float scaleX = (Vector3.Distance(new Vector3(landmarks[(int)landmarkNames.leftEar].transform.localPosition.x, 0, 0), new Vector3(landmarks[(int)landmarkNames.rightEar].transform.localPosition.x, 0, 0)) 
                / Vector3.Distance(new Vector3(lTragus.transform.localPosition.x, 0, 0), new Vector3(rTragus.transform.localPosition.x, 0, 0)));
            
            float scaleY = (Vector3.Distance(new Vector3(0, landmarks[(int)landmarkNames.forehead].transform.localPosition.y, 0), new Vector3(0, landmarks[(int)landmarkNames.topOfHead].transform.localPosition.y, 0)) 
                / Vector3.Distance(new Vector3(0, nasion.transform.localPosition.y, 0), new Vector3(0, vertex.transform.localPosition.y, 0)));

            Debug.Log(scaleX.ToString() + " " + scaleY.ToString() + " " + scaleZ.ToString());
            
            foreach (GameObject obj in landmarks)
            {
                obj.transform.parent = head.transform;
            }

            scalp.transform.localScale = new Vector3((scalp.transform.localScale.x * scaleX), (scalp.transform.localScale.y * scaleY),  (scalp.transform.localScale.z * scaleZ));

            nasion.transform.parent = head.transform;
            scalp.transform.parent = nasion.transform;
            nasion.transform.position = landmarks[(int)landmarkNames.forehead].transform.position;
            scalp.transform.parent = head.transform;
            nasion.transform.parent = scalp.transform;

            
        }

        CamController.centerMainOnObject("Scalp", 0.6F);
    }

    void ExportXYZ()
    {
        string fileName = Application.dataPath + "scalpDump.txt";
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, true))
        {
            foreach (List<Vector3> line in splineCage)
            {
                foreach (Vector3 point in line)
                {
                    file.WriteLine(point.x + "\t" + point.y + "\t" + point.z);
                }
            }
        }
    }

    public void LandmarksButtonPress()
    {
        head = GameObject.Find("Head");
        landmarks = new GameObject[5];
        for (int i = 0; i < 5; i++)
        {
            landmarks[i] = new GameObject();
            landmarks[i].transform.position = head.transform.position;
            landmarks[i].transform.rotation = head.transform.rotation;
            landmarks[i].transform.parent = head.transform;
        }
        landmarkIndex = 0;
        landmarksFound = false;
    }

    public void GenScalpButtonPress()
    {
        createGrid();
    }

    void createGrid()
    {
        int highestResolution = 0;
        int j = 0;
        foreach (List<Vector3> line in splineCage)
        {
            foreach (Vector3 point in line)
            {
                j++;
                if (j > highestResolution)
                {
                    highestResolution = j;
                }
            }
            j = 0;
        }

        Vector3[,] scalpGrid = new Vector3[highestResolution, highestResolution];

        foreach (List<Vector3> line in splineCage)
        {
            while (line.Count < highestResolution)
            {
                float distance = 0;
                Vector3 v1, v2, f1, f2;
                v1 = line[0];
                v2 = line[1];
                f1 = line[0];
                f2 = line[1];
                bool first = true;
                int k = 0;
                int l = 0;
                foreach (Vector3 point in line)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        v2 = point;
                        float d = Vector3.Distance(v1, v2);
                        if (d > distance)
                        {
                            f1 = v1;
                            f2 = v2;
                            distance = d;
                            l = k;
                        }
                        v1 = point;
                    }
                    k++;
                }
                line.Insert(l, Vector3.Lerp(f1, f2, 0.5F));
            }
        }

        int i = 0;
        for (i = 0; i < splineCage.Count; i++)
        {
            for (j = 0; j < highestResolution; j++)
            {
                if (j < splineCage[i].Count)
                {
                    Debug.Log("Filling at " + i.ToString() + " " + j.ToString());
                    scalpGrid[i, j] = splineCage[i][j];
                }
            }
        }
        
        Debug.Log("Setting control grid to " + splineCage.Count.ToString() + " x " + highestResolution.ToString());

        surfaceGen.CreateScalp(scalpGrid, splineCage.Count, highestResolution);
    }
}
