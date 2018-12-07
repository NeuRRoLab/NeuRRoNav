using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Diagnostics;


public class TargetMatching : MonoBehaviour
{
    Text calibrationInstruct;

    Stopwatch watch;
    GameObject coil;
    GameObject tCoil;
    GameObject coilHotSpot;
    GameObject tHotSpot;
    public TargetPoint tPoint;
    Coil coilTracker;
    MeshRenderer[] renderers;
    CameraController camController;

    public Grid currentGrid;
    public string gridName;

    IList<Text> xStatus = new List<Text>();
    IList<Text> yStatus = new List<Text>();
    IList<Text> zStatus = new List<Text>();
    IList<Text> yawStatus = new List<Text>();
    IList<Text> rollStatus = new List<Text>();
    IList<Text> pitchStatus = new List<Text>();

    //string[] loggingString;

    Text setHotSpot;

    int numPoints;
    int numGrids;

    int numHotSpots;

    bool matching;
    bool settingGrid;
    bool settingHotSpot;
    bool usingGrid;
    bool logging;
    bool initalized = false;

    public float mThresh = 0.25F;
    public float rThresh = 2.5F;


    FileIO logger;
    SettingsMenu settingsMenu;

    public LayerMask mask;

    public void setMThresh()
    {
        float value = float.Parse(GameObject.Find("mThresh").GetComponent<InputField>().text);
        print(value);
        mThresh = value;
    }

    public void setRThresh()
    {
        float value = float.Parse(GameObject.Find("rThresh").GetComponent<InputField>().text);
        print(value);
        rThresh = value;
    }

    // Use this for initialization
    void Start()
    {
        coilTracker = GameObject.Find("CoilTracker").GetComponent<Coil>();
        camController = GameObject.Find("Camera Controller").GetComponent<CameraController>();
        calibrationInstruct = GameObject.Find("CalibrationInstructions").GetComponent<Text>();
        settingsMenu = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>();
        matching = false;
        usingGrid = false;
        settingHotSpot = false;
        numGrids = 0;
        //scalpHotSpots.pos = null;
        //scalpHotSpots.rot = null;
        watch = new Stopwatch();

        //loggingString = new string[6];
        numHotSpots = 0;
        
        logger = new FileIO(7, settingsMenu.getField((int)SettingsMenu.settings.loggingPath), settingsMenu.getField((int)SettingsMenu.settings.loggingName), 200, new string[] { "X", "Y", "Z", "Pitch", "Yaw", "Roll", "TimeStamp" });
        logger.toggleLogging(false);

        CreateTextArray();

        initalized = true;
    }

    private void CreateTextArray()
    {
        xStatus.Add(GameObject.Find("Xl").GetComponent<Text>());
        xStatus.Add(GameObject.Find("Xr").GetComponent<Text>());
        yStatus.Add(GameObject.Find("Yl").GetComponent<Text>());
        yStatus.Add(GameObject.Find("Yr").GetComponent<Text>());
        zStatus.Add(GameObject.Find("Zl").GetComponent<Text>());
        zStatus.Add(GameObject.Find("Zr").GetComponent<Text>());

        yawStatus.Add(GameObject.Find("Yawl").GetComponent<Text>());
        yawStatus.Add(GameObject.Find("Yawr").GetComponent<Text>());
        rollStatus.Add(GameObject.Find("Rotationl").GetComponent<Text>());
        rollStatus.Add(GameObject.Find("Rotationr").GetComponent<Text>());
        pitchStatus.Add(GameObject.Find("Pitchl").GetComponent<Text>());
        pitchStatus.Add(GameObject.Find("Pitchr").GetComponent<Text>());
    }

    // Update is called once per frame
    void Update()
    {
        coilTracker.setStylusSensitiveTrackingState(matching);
        
        //if (settingGrid && (Utility.AnyInputDown()))
       // {
        //    currentGrid.gridPoints.Add(CreateGridPoint());
       // }
        if (usingGrid && Input.GetKeyDown(KeyCode.Mouse1) && !matching)
        {
            MouseSelectHotSpot();
        }
        if (settingHotSpot && (Input.anyKeyDown) &&(!Input.GetMouseButton(0)))
        {
            prepareHotSpot();
        }
        if (!matching && logging)
        {
            LogToggle();
        }

        if (matching)
        {
            CalculateOffsets();
            if (logging)
            {
                Log();
            }
        }
    }

    private void prepareHotSpot()
    {
        if (currentGrid == null)
        {
            CreateNewGrid();
        }

        TargetPoint newHotSpot = new TargetPoint(false, currentGrid.hotSpots);
        GameObject hs = new GameObject();
        hs.transform.position = GameObject.Find(coilTracker.coilName).transform.FindChild("container").FindChild("hotspot").transform.position;
        hs.transform.rotation = GameObject.Find(coilTracker.coilName).transform.FindChild("container").FindChild("hotspot").transform.rotation;
        // Yuck
        CreateScalpHotSpot(GameObject.Find(coilTracker.coilName).transform.FindChild("container").FindChild("hotspot").transform.position, GameObject.Find(coilTracker.coilName).transform.FindChild("container").FindChild("hotspot").transform.rotation);
        if (GameObject.Find("Toggle_SavePrompts").GetComponent<Toggle>().isOn)
        {
            if (AskIfToSave())
            {
                ExportGrid(0);
            }
        }

        setHotSpot.text = "New Hot Spot";
        calibrationInstruct.text = "";
        settingHotSpot = false;
    }

    void FixedUpdate()
    {
		//if (matching)
		//{
		//	CalculateOffsets();
		//	if (logging)
		//	{
		//		Log();
		//	}
		//}
    }

    private void Log()
    {
        //string path = Application.dataPath + @"\Logs\";
        //using (System.IO.StreamWriter file =
        //   new System.IO.StreamWriter(path + currentGrid.name + tPoint.ID.ToString() + ".txt", true))
        //{
        //    string pstring = "";
        //    foreach(string s in loggingString)
        //    {
        //        pstring+= s;
        //        pstring+= "\t";
        //    }
        //    pstring += watch.ElapsedMilliseconds.ToString();
        //    file.WriteLine(pstring);
        //}
        logger.setColumn(6, watch.ElapsedMilliseconds.ToString());
        logger.Log();
    }
    /*
    public void SetPointOrientation()
    {
        tPoint.rot.transform.rotation = coilHotSpot.transform.rotation;
        if (tCoil != null)
        {
            Destroy(tCoil);
        }
        if (tHotSpot != null)
        {
            Destroy(tHotSpot);
        }
        DestroyImmediate(tPoint.pos.transform.FindChild("point").gameObject);
        InstantiateTargetCoil();
        VisualizePoint(tPoint.pos, tPoint.rot);
    }*/

    private void MouseSelectHotSpot()
    {
        Ray ray = camController.cameras[camController.activeCamera].GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        //int result;
        if (Physics.Raycast(ray,out hit,100000,mask))
        {
            GameObject hitObj = hit.collider.gameObject.transform.parent.gameObject;
            TargetPoint newTPoint = null;
            // UnityEngine.Debug.Log("Hit an object:");
            // UnityEngine.Debug.Log(hitObj.name.ToString());
            /*
            foreach (TargetPoint t in currentGrid.gridPoints)
            {  
                if (t.pos.Equals(hitObj))
                {
                    newTPoint = t;
                    break;
                }
            }*/
            if(newTPoint==null)
            {
                foreach (TargetPoint t in currentGrid.hotSpots)
                {
                    
                    if (t.pos.Equals(hitObj))
                    {
                        newTPoint = t;
                        break;
                    }
                }
            }
            if(newTPoint!=null && newTPoint.fired == false)
            {
               // UnityEngine.Debug.Log("Case 3");
                tPoint = newTPoint;
                target();
            }
            else
            {
                UnityEngine.Debug.Log("Point Not Found");
            }
        }
    }
    /*
    private TargetPoint CreateGridPoint()
    {
        TargetPoint point = new TargetPoint(false, currentGrid.gridPoints);
        GameObject pos = new GameObject();
        pos.transform.position = GameObject.Find("Stylus").transform.FindChild("Point").position;
        pos.transform.parent = GameObject.Find("Head").transform;
        point.pos = pos;
        point.rot = new GameObject();
        pos.name = currentGrid.gridPoints.Count.ToString();
        point.ID = "GridPoint" + pos.name;

        VisualizePoint(pos);

        return point;
    }*/

    private static void VisualizePoint(GameObject pos)
    {
        //GameObject pshere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject pshere = Instantiate(GameObject.Find("Arrow"));
        pshere.name = "point";
        //pshere.transform.localScale = new Vector3(0.005F, 0.005F, 0.005F);
        pshere.transform.position = pos.transform.position;
        pshere.transform.parent = pos.transform;
    }
    private static void VisualizePoint(GameObject pos, GameObject rot)
    {
        //GameObject pshere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject pshere = Instantiate(GameObject.Find("Arrow"));
        pshere.name = "point";
        //pshere.transform.localScale = new Vector3(0.005F, 0.005F, 0.005F);
        pshere.transform.position = pos.transform.position;
        pshere.transform.rotation = rot.transform.rotation;
        pshere.transform.parent = pos.transform;
    }

    private void CalculateOffsets()
    {
        bool rotOK = false;
        bool posOK = false;

        if (tPoint.rot != null)
        {
            rotOK = CalculateRotation();
        }
        else
        {
            //clear fields
        }

        posOK = CalculateDistance();

        if(rotOK && posOK)
        {
            GameObject.Find("CalibrationInstructions").GetComponent<Text>().text = "FIRE";
            setTargetColor(1);
        }
        else
        {
            GameObject.Find("CalibrationInstructions").GetComponent<Text>().text = "Match";
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            tmsFire();
        }
    }

    private bool CalculateDistance()
    {
        GameObject center = GameObject.Find("Center");
        float distance = Math.Abs(Vector3.Distance(coilHotSpot.transform.position, tPoint.pos.transform.position));
        Vector3 p = center.transform.InverseTransformVector(coilHotSpot.transform.position);
        Vector3 t = center.transform.InverseTransformVector(tPoint.pos.transform.position);
        float deltX = p.x - t.x;
        float deltY = p.y - t.y;
        float deltZ = p.z - t.z;

        if (distance > 0.5) // 0.05 is what this should be
        {
            distance = 1;
            deltX *= 100;
            deltY *= 100;
            deltZ *= 100;
            xStatus[0].text = xStatus[1].text = "X: Get Closer";
            yStatus[0].text = yStatus[1].text = "Y: Get Closer";
            zStatus[0].text = zStatus[1].text = "Z: Get Closer";
            //loggingString[0] = deltX.ToString("0.00");
            //loggingString[1] = deltY.ToString("0.00");
            //loggingString[2] = deltZ.ToString("0.00");
            logger.setColumn(0,deltX.ToString("0.00"));
            logger.setColumn(1, deltY.ToString("0.00"));
            logger.setColumn(2, deltZ.ToString("0.00"));
        }

        else
        {
            //Color calcuations
            float distX = Math.Abs(deltX) / 0.0288F;
            float distY = Math.Abs(deltY) / 0.0288F;
            float distZ = Math.Abs(deltZ) / 0.0288F;

            xStatus[0].color = xStatus[1].color = new Color(distX, 1 - distX, 0, 1);
            yStatus[0].color = yStatus[1].color = new Color(distY, 1 - distY, 0, 1);
            zStatus[0].color = zStatus[1].color = new Color(distZ, 1 - distZ, 0, 1);

            //Convert to CM
            deltX *= 100;
            deltY *= 100;
            deltZ *= 100;

            //Calculate needed corrections
            string rl = setMoveInstruction("Right ", "Left ", -deltX, mThresh);
            string ud = setMoveInstruction("Up ", "Down ", -deltY, mThresh);
            string fb = setMoveInstruction("Back ", "Fward ", deltZ, mThresh);

            //Update the status of each dimension
            xStatus[0].text = xStatus[1].text = "X: " + rl + deltX.ToString("0.0");
            yStatus[0].text = yStatus[1].text = "Y: " + ud + deltY.ToString("0.0");
            zStatus[0].text = zStatus[1].text = "Z: " + fb + deltZ.ToString("0.0");

            distance = distance / 0.05F;

            //loggingString[0] = deltX.ToString("0.00");
            //loggingString[1] = deltY.ToString("0.00");
            //loggingString[2] = deltZ.ToString("0.00");
            logger.setColumn(0, deltX.ToString("0.00"));
            logger.setColumn(1, deltY.ToString("0.00"));
            logger.setColumn(2, deltZ.ToString("0.00"));

            if (rl == ud && ud == fb && fb == "OK")
            {
                tPoint.pos.transform.FindChild("point").GetComponent<MeshRenderer>().material.color = new Color(distance, 1 - distance, 0, 0.2F);
                return true;
            }
        }
        tPoint.pos.transform.FindChild("point").GetComponent<MeshRenderer>().material.color = new Color(distance, 1 - distance, 0, 0.2F);
        return false;
    }

    private Quaternion quaternionDifference(Quaternion fromRotation, Quaternion toRotation)
    {
        //return fromRotation * Quaternion.Inverse(toRotation);
        return Quaternion.Inverse(fromRotation) * toRotation;
    }

    private float[] AngleDecomposition(Quaternion angle)//Returns Roll Pitch Yaw
    {
        // 
        float w = angle.w;
        float y = angle.y;
        float z = angle.z;
        float x = angle.x;

        float[] rpy = new float[3];
        rpy[0] = Mathf.Atan2(2 * y * w - 2 * x * z, 1 - 2 * y * y - 2 * z * z) * 180 / Mathf.PI;
        rpy[1] = Mathf.Atan2(2 * x * w - 2 * y * z, 1 - 2 * x * x - 2 * z * z) * 180 / Mathf.PI;
        rpy[2] = Mathf.Asin(2 * x * y + 2 * z * w) * 180 / Mathf.PI;

        return rpy;
    }

    private bool CalculateRotation()
    {
        //float angle = Quaternion.Angle(coilHotSpot.transform.rotation, tPoint.rot.transform.rotation);
        //float pitch = coilHotSpot.transform.rotation.eulerAngles.x - tPoint.rot.transform.rotation.eulerAngles.x;
        //float yaw = coilHotSpot.transform.rotation.eulerAngles.y - tPoint.rot.transform.rotation.eulerAngles.y;
        //float roll = coilHotSpot.transform.rotation.eulerAngles.z - tPoint.rot.transform.rotation.eulerAngles.z;

        // This gives the angle that when applied to coilHotSpot, will result in tPoint.rotation
        Quaternion angle =  quaternionDifference(coilHotSpot.transform.rotation, tPoint.rot.transform.rotation);

        float[] rpy = AngleDecomposition(angle);

        float distRoll = Math.Abs(rpy[0]) / 180F;
        float distPitch = Math.Abs(rpy[1]) / 180F;
        float distYaw = Math.Abs(rpy[2]) / 180F;

        pitchStatus[0].color = pitchStatus[1].color = new Color(distPitch, 1 - distPitch, 0, 1);
        yawStatus[0].color = yawStatus[1].color = new Color(distYaw, 1 - distYaw, 0, 1);
        rollStatus[0].color = rollStatus[1].color = new Color(distRoll, 1 - distRoll, 0, 1);

        float angleDif = Quaternion.Angle(coilHotSpot.transform.rotation, tPoint.rot.transform.rotation) / 60;
        if (angleDif > 1)
        {
            angleDif = 1;
        }

        setTargetColor(angleDif * 0.9f);

        string r = setRotateInstruction("Clk ", "CtrCl ", rpy[0], rThresh);
        string p = setRotateInstruction("Up ", "Dwn ", -rpy[1], rThresh);
        string y = setRotateInstruction("Lef ", "Rigt ", rpy[2], rThresh);

        pitchStatus[0].text = pitchStatus[1].text = "P: " + p + rpy[1].ToString("0.00");
        yawStatus[0].text = yawStatus[1].text = "Y: " + y + rpy[2].ToString("0.00");
        rollStatus[0].text = rollStatus[1].text = "R: " + r + rpy[0].ToString("0.00");

        //loggingString[3] = "Pitch: " + rpy[1].ToString("0.00");
        //loggingString[4] = "Yaw: " + rpy[2].ToString("0.00");
        //loggingString[5] = "Roll: " + rpy[0].ToString("0.00");
        logger.setColumn(3, rpy[1].ToString("0.00"));
        logger.setColumn(4, rpy[2].ToString("0.00"));
        logger.setColumn(5, rpy[0].ToString("0.00"));

        if (r == p && p == y && y == "OK")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void setTargetColor(float decPercent)
    {
        foreach (MeshRenderer renderer in renderers)
        {
            if (!renderer.gameObject.name.Equals("Point"))
            {
                Material transMat = renderer.material;
                Color transColor = new Color(decPercent, 1 - decPercent, 0, 0.2F);
                transMat.color = transColor;
                renderer.material = transMat;
            }
        }
    }

    public class TargetPoint
    {
        public GameObject pos;
        public GameObject rot;
        public string ID;
        public bool fired;
        public IList<TargetPoint> containedIn;
        public int index;

        public TargetPoint(bool fired, IList<TargetPoint> containedIn)
        {
            this.fired = fired;
            pos = null;
            rot = null;
            ID = null;
            index = -1;
            this.containedIn = containedIn;
        }
    }

    public class Grid
    {
        //public IList<TargetPoint> gridPoints;
        public IList<TargetPoint> hotSpots; 
        public string name;

        public Grid(string name, /*IList<TargetPoint> points,*/ IList<TargetPoint> hotSpots)
        {
            //this.gridPoints = points;
            this.hotSpots = hotSpots;
            this.name = name;
        }
    }

    public void AddPoints()
    {














        // Old stuff I don't get, ignore for now
        /*
        if (!settingGrid)
        {
            if(currentGrid == null)
            {
                CreateNewGrid();
            }
            GameObject.Find("StylusTracker").GetComponent<Stylus>().setStylusSensitiveTrackingState(true);
            GameObject.Find("Add Points").transform.FindChild("Text").GetComponent<Text>().text = "Confirm Grid";
            settingGrid = true;
        }
        else
        {
            GameObject.Find("StylusTracker").GetComponent<Stylus>().setStylusSensitiveTrackingState(false);

            if(currentGrid.gridPoints.Count >= 5)
            {
                CalculateEstimatedTangentsForGridPoints();
            }
            else
            {
                //not enough points to calculate tangents, warn user
            }

            settingGrid = false;
            usingGrid = true;

            GameObject.Find("Add Points").transform.FindChild("Text").GetComponent<Text>().text = "Add Points";
            GameObject.Find("ScalpGenerator").GetComponent<ScalpGenerator>().waitingToDraw = true;
            GameObject.Find("Save Grids").GetComponent<Button>().interactable = true;

        }*/
    }
    /*
    private void CalculateEstimatedTangentsForGridPoints()
    {
        foreach (TargetPoint point in currentGrid.gridPoints)
        {
            TargetPoint a = new TargetPoint(false, currentGrid.gridPoints);
            TargetPoint b = new TargetPoint(false, currentGrid.gridPoints);
            TargetPoint c = new TargetPoint(false, currentGrid.gridPoints);
            float distanceA = -1;
            float distanceB = -1;
            float distanceC = -1;
            int count = 0;
            foreach (TargetPoint p in currentGrid.gridPoints)
            {
                if (!p.Equals(point))
                {
                    if (count == 0)
                    {
                        distanceA = Vector3.Distance(point.pos.transform.position, p.pos.transform.position);
                        a = p;
                    }
                    else if (count == 1)
                    {
                        distanceB = Vector3.Distance(point.pos.transform.position, p.pos.transform.position);
                        if (distanceB > distanceA)
                        {
                            b = p;
                        }
                        else
                        {
                            float switchF = distanceA;
                            distanceA = distanceB;
                            distanceB = switchF;
                            b = a;
                            a = p;
                        }
                    }
                    else if (count == 3)
                    {
                        distanceC = Vector3.Distance(point.pos.transform.position, p.pos.transform.position);
                        if (distanceC < distanceA && distanceC < distanceB)
                        {
                            float switchF = distanceA;
                            distanceA = distanceC;
                            distanceC = distanceB;
                            distanceB = switchF;

                            c = b;
                            b = a;
                            a = p;
                        }
                        else if (distanceC > distanceA && distanceC < distanceB)
                        {
                            float switchF = distanceB;
                            distanceB = distanceC;
                            distanceB = switchF;
                            c = b;
                            b = p;
                        }
                        else
                        {
                            c = p;
                        }
                    }
                    else
                    {
                        float distance = Vector3.Distance(point.pos.transform.position, p.pos.transform.position);
                        if (distance < distanceA && distance < distanceB && distance < distanceC)
                        {
                            distanceC = distanceB;
                            distanceB = distanceA;
                            distanceA = distance;

                            c = b;
                            b = a;
                            a = p;
                        }
                        else if (distance > distanceA && distance < distanceB && distance < distanceC)
                        {
                            distanceC = distanceB;
                            distanceB = distance;
                            c = b;
                            b = p;
                        }
                        else if (distance > distanceA && distance > distanceB && distance < distanceC)
                        {
                            distanceC = distance;
                            c = p;
                        }

                    }
                    count++;
                }
            }

            Vector3 side1 = a.pos.transform.position - point.pos.transform.position;
            Vector3 side2b = b.pos.transform.position - point.pos.transform.position;
            Vector3 side2c = c.pos.transform.position - point.pos.transform.position;

            Vector3 cross;

            if (Math.Abs(Vector3.Dot(side1.normalized, side2b.normalized)) < Math.Abs(Vector3.Dot(side1.normalized, side2c.normalized)))
            {
                cross = Vector3.Cross(side2b, side1);
            }
            else
            {
                cross = Vector3.Cross(side2c, side1);
            }

            GameObject center = GameObject.Find("Center").gameObject;
            Vector3 toCenter = center.transform.position - point.pos.transform.position;

            point.rot.transform.rotation = Quaternion.LookRotation(cross);

            if (Vector3.Dot(point.rot.transform.forward.normalized, toCenter.normalized) > 0)
            {
                point.rot.transform.rotation = Quaternion.LookRotation(-cross);
            }

            point.rot.transform.LookAt(point.rot.transform.up, point.rot.transform.forward);

            point.rot.transform.parent = GameObject.Find("Head").transform;

            point.pos.transform.FindChild("point").transform.rotation = point.rot.transform.rotation;
        }
    }*/

    public void setScalpHotspotButton(GameObject button)
    {
        if (settingHotSpot == false)
        {
            calibrationInstruct.text = "Press Space";

            settingHotSpot = true;
            setHotSpot = button.GetComponentInChildren<Text>();
            setHotSpot.text = "Cancel";
        }
        else
        {
            calibrationInstruct.text = "";
            settingHotSpot = false;
            setHotSpot.text = "New Hot Spot";
        }
    }

    public void setGridManualButtonPress()
    {
        if (!settingGrid)
        {
            newGrid();
        }
    }

    public void newGrid()
    {
        DestroyAllHotSpots();
        CreateNewGrid();
    }

    private void CreateNewGrid()
    {
        numPoints = 0;
        int numg = ++numGrids;
        //IList<TargetPoint> gp = new List<TargetPoint>();
        IList<TargetPoint> hs = new List<TargetPoint>();
        currentGrid = new Grid(gridName, /*gp,*/ hs);
        GameObject.Find("Set Grid").GetComponent<Button>().interactable = true;
        //GameObject.Find("ScalpGenerator").GetComponent<ScalpGenerator>().waitingToDraw = false;
    }

    public void LogToggle()
    {
        UnityEngine.Debug.Log("Clicked");
        if (!logger.toggleLogging())
        {
            logging = false;
            UnityEngine.Debug.Log("Log Off");
            watch.Stop();
            watch.Reset();
            GameObject.Find("Logging").GetComponentInChildren<Text>().text = "Toggle Logging";
        }
        else
        {
            //string path = Application.dataPath + @"\Logs\";
            //using (System.IO.StreamWriter file =
            //   new System.IO.StreamWriter(path + currentGrid.name + "_" + tPoint.ID.ToString() + ".txt", true))
            //{
            //    file.WriteLine(tPoint.ID.ToString());
            //    file.WriteLine(System.DateTime.Now.ToString() + " " + System.DateTime.Now.Millisecond.ToString());
            //}
            logging = true;
            UnityEngine.Debug.Log("Log On");
            logger.setColumn(0, currentGrid.name + "_" + tPoint.ID.ToString());
            logger.Log();
            GameObject.Find("Logging").GetComponentInChildren<Text>().text = "Stop Logging";
            watch.Start();
        }
    }

    private void DestroyAllHotSpots()
    {
        // New behavior: Put all three cameras on head, with similar orientation as when points are being lined up.
        camController.putMainCam1FacingBackOfHead();
        camController.putTargetCam1OnHeadXZ();
        camController.putTargetCam2OnHeadZY();

        if (currentGrid.hotSpots.Count > 0)
        {
            foreach (TargetPoint point in currentGrid.hotSpots)
            {
                Destroy(point.pos.transform.FindChild("point").gameObject);
                Destroy(point.pos.gameObject);
                Destroy(point.rot.gameObject);

            }
            currentGrid.hotSpots = null;
        }
        GameObject.Find("Set Grid").GetComponent<Button>().interactable = false;
    }

    public void DestroySelectedPoint()
    {
        if (matching)
        {
            /* OLD BUGGY BEHAVIOR: would put both bottom cameras on stylus, and break the top one
            int i = 0;
            foreach(GameObject t in camController.targets)
            {
                if (t.Equals(tPoint.pos))
                {
                    camController.putCamOnStylus(i);
                }
                i++;
            }*/

            // New behavior: Put all three cameras on head, with similar orientation as when points are being lined up.
            camController.putMainCam1FacingBackOfHead();
            camController.putTargetCam1OnHeadXZ();
            camController.putTargetCam2OnHeadZY();


            Destroy(tPoint.pos.transform.FindChild("point").gameObject);
            Destroy(tPoint.pos.gameObject);
            Destroy(tPoint.rot.gameObject);
            tPoint.containedIn.Remove(tPoint);

            if (tCoil != null)
            {
                Destroy(tCoil);
            }
            if (tHotSpot != null)
            {
                Destroy(tHotSpot);
            }

            GameObject.Find("CalibrationInstructions").GetComponent<Text>().text = "";
            GameObject.Find("Set Grid").GetComponent<Button>().interactable = true;
            GameObject.Find("Delete Point").GetComponent<Button>().interactable = false;
            //GameObject.Find("Set Point Orientation").GetComponent<Button>().interactable = false;
            GameObject.Find("Logging").GetComponent<Button>().interactable = false;
            GameObject.Find("Generate Grid").GetComponent<Button>().interactable = false;
            GameObject.Find("Scalp Mesh").GetComponent<Button>().interactable = true;
            GameObject.Find("Set Hot Spot").GetComponent<Button>().interactable = true;
            matching = false;

        }
    }

    string setMoveInstruction(string greaterThan, string lessThan, float dimension, float threshold)
    {
        if (Math.Abs(Math.Round(dimension, 1)) > threshold)
        {
            if (Math.Sign(dimension) == 1)
            {
                return greaterThan;
            }
            else
            {
                return lessThan;
            }
        }
        else
        {
            return "OK";
        }
    }

    string setRotateInstruction(string greaterThan, string lessThan, float angle, float threshold)
    {
        if (Math.Abs(Math.Round(angle, 1)) > threshold)
        {
            if (Math.Sign(angle) == 1)
            {
                return greaterThan;
            }
            else
            {
                return lessThan;
            }
        }
        else
        {
            return "OK";
        }
    }

    public void target()
    {
        GameObject head = GameObject.Find("Head");
        coil = GameObject.Find(GameObject.Find("CoilTracker").GetComponent<Coil>().coilName);
        coilHotSpot = coil.transform.FindChild("container").FindChild("hotspot").gameObject;

        InstantiateTargetCoil();
        // Disable adding new points during target mode
        

        // LEGACY
        /*
        camController.putMainCamOnTargetXY(tPoint.pos);
        camController.putTargetCam1OnTargetXZ(tPoint.pos);
        camController.putTargetCam2OnTargetZY(tPoint.pos);
        */

        //camController.putMainCamOnTargetXY(coil);
        camController.putMainCam1FacingBackOfCoil(tPoint.pos);
        camController.putTargetCam1OnTargetXZ(tPoint.pos);
        camController.putTargetCam2OnTargetZY(tPoint.pos);

        //tCoil.transform.FindChild("model").transform.localScale = Vector3.Scale(tCoil.transform.FindChild("model").transform.localScale, new Vector3(1.1F,1.1F,1.1F)); position/stretch problems

        matching = true;
        GameObject.Find("Set Hot Spot").GetComponent<Button>().interactable = false;
        GameObject.Find("Delete Point").GetComponent<Button>().interactable = true;
        //GameObject.Find("Set Point Orientation").GetComponent<Button>().interactable = true;
        GameObject.Find("Generate Grid").GetComponent<Button>().interactable = true;
        GameObject.Find("Logging").GetComponent<Button>().interactable = true;
        GameObject.Find("Set Grid").GetComponent<Button>().interactable = false;
        GameObject.Find("Scalp Mesh").GetComponent<Button>().interactable = false;
        //GameObject.Find("CalibrationInstructions").GetComponent<Text>().text = "Match";
    }

    private void InstantiateTargetCoil()
    {

        GameObject head = GameObject.Find("Head");
        tCoil = (GameObject)Instantiate(coil);
        tHotSpot = tCoil.transform.FindChild("container").FindChild("hotspot").gameObject;
        tHotSpot.transform.parent = null;
        tCoil.transform.parent = tHotSpot.transform;
        tHotSpot.transform.position = tPoint.pos.transform.position;

        if (tPoint.rot == null)
        {
            UnityEngine.Debug.Log("setting rotation on tms fire");
            Destroy(tCoil.transform.FindChild("container").FindChild("model").gameObject);
            tHotSpot.transform.parent = head.transform;
            renderers = tHotSpot.GetComponentsInChildren<MeshRenderer>();
        }
        else
        {
            UnityEngine.Debug.Log("rotation not null");
            tHotSpot.transform.rotation = tPoint.rot.transform.rotation;
            tCoil.transform.parent = head.transform;
            tHotSpot.transform.parent = tCoil.transform;
            renderers = tCoil.GetComponentsInChildren<MeshRenderer>();
        }

        foreach (MeshRenderer renderer in renderers)
        {
            Material transMat = renderer.material;
            Color transColor = Color.red;
            transColor.a = 0.2F;
            transMat.color = transColor;
            transMat.SetFloat("_Mode", 3);
            transMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            transMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            transMat.SetInt("_ZWrite", 0);
            transMat.DisableKeyword("_ALPHATEST_ON");
            transMat.DisableKeyword("_ALPHABLEND_ON");
            transMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            transMat.renderQueue = 3000;
            renderer.material = transMat;
        }
    }

    public void ExportGrid(int index)
    {
        GameObject center = GameObject.Find("Center");

        string path = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.gridSavePath);
        string fileName = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.gridSaveName);

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
            Grid grid = currentGrid;
            file.WriteLine(grid.name);
            //file.WriteLine(grid.gridPoints.Count.ToString());
            file.WriteLine(grid.hotSpots.Count.ToString());
            /*
            foreach (TargetPoint point in grid.gridPoints)
            {
                //Vector3 p = center.transform.InverseTransformPoint(GameObject.Find("Head").transform.TransformPoint(point.pos.transform.position));
                Vector3 p = center.transform.InverseTransformPoint(point.pos.transform.position);
                if (point.rot != null)
                {
                    Quaternion r = Quaternion.Inverse(center.transform.rotation) * point.rot.transform.rotation;
                    file.WriteLine("0" + "\t" + p.x + "\t" + p.y + "\t" + p.z + "\t" + r.x + "\t" + r.y + "\t" + r.z + "\t" + r.w);
                }
                else
                {
                    file.WriteLine("1" + "\t" + p.x + "\t" + p.y + "\t" + p.z);
                }
            }*/
            //if (scalpHotSpots.pos != null)
            foreach (TargetPoint point in grid.hotSpots)
            {
                Vector3 p = center.transform.InverseTransformPoint(point.pos.transform.position);
                Quaternion r = Quaternion.Inverse(center.transform.rotation) * point.rot.transform.rotation;
                file.WriteLine("0" + "\t" + p.x + "\t" + p.y + "\t" + p.z + "\t" + r.x + "\t" + r.y + "\t" + r.z + "\t" + r.w);
                //CreateScalpHotSpot(p, r);
            }

            //GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().incrementField((int)SettingsMenu.settings.gridSaveName);
        }
    }

    public void CreateScalpHotSpot(Vector3 p, Quaternion r)
    {
        //if (scalpHotSpot.pos != null && scalpHotSpot.pos.transform.FindChild("point") != null)
        //{
        //    DestroyImmediate(scalpHotSpot.pos.transform.FindChild("point").gameObject);
        //    DestroyImmediate(scalpHotSpot.pos.gameObject);
        //    DestroyImmediate(scalpHotSpot.rot.gameObject);
        //}

        TargetPoint newHotSpot = new TargetPoint(false, currentGrid.hotSpots);
        GameObject hs = new GameObject();
        hs.transform.position = p;
        hs.transform.rotation = r;
        hs.transform.parent = GameObject.Find("Head").transform;
        //GameObject pshere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject psphere = Instantiate(GameObject.Find("HotSpot Arrow"));
        //pshere.transform.localScale = new Vector3(0.01F, 0.01F, 0.01F);
        psphere.transform.position = hs.transform.position;
        psphere.transform.rotation = hs.transform.rotation;
        psphere.transform.parent = hs.transform;
        psphere.name = "point";
        hs.name = "ScalpHotSpot " + (++numHotSpots);

        newHotSpot.pos = hs;
        newHotSpot.rot = hs;
        newHotSpot.ID = hs.name;
        newHotSpot.rot.name = hs.name + " rot";
        currentGrid.hotSpots.Add(newHotSpot);
        newHotSpot.index = currentGrid.hotSpots.IndexOf(newHotSpot);

        usingGrid = true;

        GameObject.Find("Save Grids").GetComponent<Button>().interactable = true;
        
    }

    public void ImportGrid()
    {
        string path = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.gridLoadPath);
        print(path);
        string fileName = GameObject.Find("SettingMenu").GetComponent<SettingsMenu>().getField((int)SettingsMenu.settings.gridLoadName);

       

        try {
            System.IO.FileStream filestream = new System.IO.FileStream(path + "/" + fileName,
                                              System.IO.FileMode.Open,
                                              System.IO.FileAccess.Read,
                                              System.IO.FileShare.Read);
            System.IO.StreamReader file = new System.IO.StreamReader(filestream);

            if (currentGrid != null)
            {
                DestroyAllHotSpots();
            }

            currentGrid = new Grid(file.ReadLine(), /*new List<TargetPoint>(),*/ new List<TargetPoint>());
            //int points = System.Convert.ToInt32(file.ReadLine());
            int hotSpots = System.Convert.ToInt32(file.ReadLine());
            /*
            for (int i = 0; i < points; i++)
            {
                TargetPoint t = new TargetPoint(false, currentGrid.gridPoints);
                string data = file.ReadLine();
                char[] d = new char[1];
                d[0] = '\t';
                string[] dims = data.Split(d);
                t.pos = new GameObject();
                //t.pos.transform.position = GameObject.Find("Head").transform.InverseTransformPoint(GameObject.Find("Center").transform.TransformPoint(new Vector3((float)System.Convert.ToDouble(dims[1]), (float)System.Convert.ToDouble(dims[2]), (float)System.Convert.ToDouble(dims[3]))));
                t.pos.transform.position = GameObject.Find("Center").transform.TransformPoint(new Vector3((float)System.Convert.ToDouble(dims[1]), (float)System.Convert.ToDouble(dims[2]), (float)System.Convert.ToDouble(dims[3])));
                if (dims[0].Equals("0"))
                {
                    t.rot = new GameObject();
                    t.rot.transform.rotation = GameObject.Find("Center").transform.rotation * new Quaternion((float)System.Convert.ToDouble(dims[4]), (float)System.Convert.ToDouble(dims[5]), (float)System.Convert.ToDouble(dims[6]), (float)System.Convert.ToDouble(dims[7]));
                }
                else
                {
                    t.rot = null;
                }
                currentGrid.gridPoints.Add(t);
                t.index = currentGrid.gridPoints.IndexOf(t);
            }*/
            for (int i = 0; i < hotSpots; i++)
            {
                string datahs = file.ReadLine();
                char[] dhs = new char[1];
                dhs[0] = '\t';
                string[] dimshs = datahs.Split(dhs);
                if (dimshs[0].Equals("0"))
                {
                    Vector3 p = GameObject.Find("Center").transform.TransformPoint(new Vector3((float)System.Convert.ToDouble(dimshs[1]), (float)System.Convert.ToDouble(dimshs[2]), (float)System.Convert.ToDouble(dimshs[3])));
                    Quaternion r = GameObject.Find("Center").transform.rotation * new Quaternion((float)System.Convert.ToDouble(dimshs[4]), (float)System.Convert.ToDouble(dimshs[5]), (float)System.Convert.ToDouble(dimshs[6]), (float)System.Convert.ToDouble(dimshs[7]));
                    CreateScalpHotSpot(p, r);
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
        /*
        foreach (TargetPoint point in currentGrid.gridPoints)
        {
            point.pos.transform.parent = GameObject.Find("Head").transform;
            point.pos.name = currentGrid.gridPoints.IndexOf(point).ToString();
            point.rot.name = currentGrid.gridPoints.IndexOf(point).ToString() + "rot";

            //GameObject pshere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //pshere.name = "point";
            //pshere.transform.localScale = new Vector3(0.005F, 0.005F, 0.005F);
            //pshere.transform.position = point.pos.transform.position;
            //pshere.transform.parent = point.pos.transform;

            VisualizePoint(point.pos, point.rot);
        }*/

        //GameObject.Find("Add Points").GetComponent<Button>().interactable = true;
        // Re-enables adding new points during target mode
        GameObject.Find("Set Hot Spot").GetComponent<Button>().interactable = true;
        GameObject.Find("Set Grid").GetComponent<Button>().interactable = true;
        GameObject.Find("Reset Grid").GetComponent<Button>().interactable = true;
        settingGrid = false;
        usingGrid = true;
        matching = false;
    }

    public void resetGrid()
    {
        Material pmat = GameObject.Find("Arrow").GetComponent<Renderer>().material;
        Material hmat = GameObject.Find("HotSpot Arrow").GetComponent<Renderer>().material;
        /*
        for (int i = 0; i < currentGrid.gridPoints.Count; i++)
        {
            TargetPoint t = currentGrid.gridPoints[i];
            t.fired = false;
            Renderer renderer = t.pos.GetComponentInChildren<Renderer>();
            renderer.material = pmat;
            CapsuleCollider collider = t.pos.GetComponentInChildren<CapsuleCollider>();
            collider.enabled = true;
            currentGrid.gridPoints[i] = t;
        }*/
        for (int i = 0; i < currentGrid.hotSpots.Count; i++)
        {
            TargetPoint t = currentGrid.hotSpots[i];
            t.fired = false;
            Renderer renderer = t.pos.GetComponentInChildren<Renderer>();
            CapsuleCollider collider = t.pos.GetComponentInChildren<CapsuleCollider>();
            collider.enabled = true;
            renderer.material = hmat;
            currentGrid.hotSpots[i] = t;
        }
    }

    void tmsFire()
    {
        
        if (tPoint.rot == null)
        {
            UnityEngine.Debug.Log("rotation set on fire");
            tPoint.rot = new GameObject();
            tPoint.rot.transform.rotation = coil.transform.FindChild("hotspot").transform.rotation;
            tPoint.rot.transform.parent = GameObject.Find("Head").transform;
            DestroyImmediate(tPoint.pos.transform.FindChild("point"));
            VisualizePoint(tPoint.pos, tPoint.rot);
        }
        /*
        // New mode: destroy tPoint after firing, need to realign cameras too
        EDIT: Nevermind!
        camController.putMainCam1FacingBackOfHead();
        camController.putTargetCam1OnHeadXZ();
        camController.putTargetCam2OnHeadZY();

        DestroyImmediate(tPoint.pos.transform.FindChild("point").gameObject);
        DestroyImmediate(tPoint.pos.gameObject);
        DestroyImmediate(tPoint.rot.gameObject);
        tPoint.containedIn.Remove(tPoint);
        */
        if (tCoil != null)
        {
            Destroy(tCoil);
        }
        if (tHotSpot != null)
        {
            Destroy(tHotSpot);
        }
        
        tPoint.fired = true;
        matching = false;
        Renderer renderer = tPoint.pos.GetComponentInChildren<Renderer>();
        CapsuleCollider collider = tPoint.pos.GetComponentInChildren<CapsuleCollider>();
        collider.enabled = false;
        Material transMat = renderer.material;
        Color transColor = Color.red;
        transColor.a = 0.2F;
        transMat.color = transColor;
        transMat.SetFloat("_Mode", 3);
        transMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        transMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        transMat.SetInt("_ZWrite", 0);
        transMat.DisableKeyword("_ALPHATEST_ON");
        transMat.DisableKeyword("_ALPHABLEND_ON");
        transMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        transMat.renderQueue = 3000;
        renderer.material = transMat;
        tPoint.containedIn[tPoint.containedIn.IndexOf(tPoint)] = tPoint;

        GameObject.Find("Set Hot Spot").GetComponent<Button>().interactable = true;
        GameObject.Find("Reset Grid").GetComponent<Button>().interactable = true;
        //GameObject.Find("Set Point Orientation").GetComponent<Button>().interactable = false;
        GameObject.Find("Delete Point").GetComponent<Button>().interactable = false;
        GameObject.Find("Set Grid").GetComponent<Button>().interactable = true;
        GameObject.Find("Generate Grid").GetComponent<Button>().interactable = false;
        GameObject.Find("CalibrationInstructions").GetComponent<Text>().text = "";
        GameObject.Find("Scalp Mesh").GetComponent<Button>().interactable = true;

        if (logging)
        {
            LogToggle();
        }
        matching = false;
    }

    public void setGridName(string name)
    {
        gridName = name;
    }

    public void OnApplicationQuit()
    {
        logger.Close();
    }

    public void setLoggingPath(string path)
    {
        if(initalized)
        logger.SetFilePath(path);
    }

    public void setLoggingName(string name)
    {
        if (initalized)
        logger.SetFileName(name);
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

            text.Text = text.Text = "A file exists at the Grid Save location specified! \nDo you want to overwrite?\n\nIf not: Cancel, then edit the Save Grid Field, \nthen Save Manually.";
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

            text.Text = "Successfully added Hotspot! Do you want to save the \nentire Grid to Grid Save location?";
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
