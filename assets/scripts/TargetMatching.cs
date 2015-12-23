using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Diagnostics;


public class TargetMatching : MonoBehaviour
{
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

    TargetPoint scalpHotSpot;

    IList<Text> xStatus = new List<Text>();
    IList<Text> yStatus = new List<Text>();
    IList<Text> zStatus = new List<Text>();
    IList<Text> yawStatus = new List<Text>();
    IList<Text> rollStatus = new List<Text>();
    IList<Text> pitchStatus = new List<Text>();

    string[] loggingString;

    Text setHotSpot;
    Text errorToggleText;

    int numPoints;
    int numGrids;

    bool matching;
    bool settingGrid;
    bool settingHotSpot;
    bool usingGrid;
    bool logging;

    // Use this for initialization
    void Start()
    {

        coilTracker = GameObject.Find("CoilTracker").GetComponent<Coil>();
        camController = GameObject.Find("Camera Controller").GetComponent<CameraController>();
        matching = false;
        usingGrid = false;
        settingHotSpot = false;
        numGrids = 0;
        scalpHotSpot.pos = null;
        scalpHotSpot.rot = null;
        watch = new Stopwatch();
        logging = false;
        loggingString = new string[6];

        CreateTextArray();

        currentGrid = new Grid("null", new List<TargetPoint>());


        string path = Application.dataPath + @"\Grids\Load";

        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }

        path = Application.dataPath + @"\Grids\Saved";

        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }

        path = Application.dataPath + @"\Logs";

        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }
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
        if (matching)
        {
            CalculateOffsets();
            if (logging)
            {
                Log();
            }
        }
        if (settingGrid && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse1)))
        {
            currentGrid.points.Add(CreateGridPoint());
        }
        if (usingGrid && Input.GetKeyDown(KeyCode.Mouse1) && !matching)
        {
            MouseSelectGridPoint();
        }

        if (settingHotSpot && (Input.GetKeyDown(KeyCode.Space)))
        {
            TargetPoint newHotSpot = new TargetPoint(false);
            GameObject hs = new GameObject();
            hs.transform.position = GameObject.Find(coilTracker.coilName).transform.FindChild("container").FindChild("hotspot").transform.position;
            hs.transform.rotation = GameObject.Find(coilTracker.coilName).transform.FindChild("container").FindChild("hotspot").transform.rotation;
            CreateScalpHotSpot(GameObject.Find(coilTracker.coilName).transform.FindChild("container").FindChild("hotspot").transform.position, GameObject.Find(coilTracker.coilName).transform.FindChild("container").FindChild("hotspot").transform.rotation);

            setHotSpot.text = "Set Hot Spot";
            settingHotSpot = false;
        }

        if (!matching && logging)
        {
            LogErrorToggle(errorToggleText);
        }
    }

    void fixedUpdate()
    {
       
    }

    private void Log()
    {
        string path = Application.dataPath + @"\Logs\";
        using (System.IO.StreamWriter file =
           new System.IO.StreamWriter(path + currentGrid.name + tPoint.ID.ToString() + ".txt", true))
        {
            string pstring = "";
            foreach(string s in loggingString)
            {
                pstring+= s;
                pstring+= "\t";
            }
            pstring += watch.ElapsedMilliseconds.ToString();
            file.WriteLine(pstring);
        }
    }

    public void SetPointOrientation()
    {
        tPoint.rot.transform.rotation = coilHotSpot.transform.rotation;
    }

    private void MouseSelectGridPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            int result;
            if (Int32.TryParse(hit.collider.gameObject.transform.parent.gameObject.name, out result))
            {
                tPoint = currentGrid.points[result];
                if (tPoint.fired == false)
                {
                    tPoint.ID = result.ToString();
                    target();
                }
            }
            else if (hit.collider.gameObject.transform.parent.gameObject.name.Equals("Scalp Hot Spot"))
            {
                tPoint = scalpHotSpot;
                if (tPoint.fired == false)
                {
                    target();
                }
            }
        }
    }

    private TargetPoint CreateGridPoint()
    {
        TargetPoint point = new TargetPoint(false);
        GameObject pos = new GameObject();
        pos.transform.position = GameObject.Find("Stylus").transform.FindChild("Point").position;
        pos.transform.parent = GameObject.Find("Head").transform;
        point.pos = pos;
        point.rot = new GameObject();
        pos.name = currentGrid.points.Count.ToString();

        VisualizePoint(pos);
        return point;
    }

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

        if (tPoint.rot != null)
        {
            CalculateRotation();
        }

        CalculateDistance();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            tmsFire();
        }
    }

    private void CalculateDistance()
    {
        GameObject center = GameObject.Find("Center");
        float distance = Math.Abs(Vector3.Distance(coilHotSpot.transform.position, tPoint.pos.transform.position));
        Vector3 p = center.transform.InverseTransformVector(coilHotSpot.transform.position);
        Vector3 t = center.transform.InverseTransformVector(tPoint.pos.transform.position);
        float deltX = p.x - t.x;
        float deltY = p.y - t.y;
        float deltZ = p.z - t.z;



        if (distance > 0.05)
        {
            distance = 1;
            deltX *= 100;
            deltY *= 100;
            deltZ *= 100;
            xStatus[0].text = xStatus[1].text = "X: Get Closer";
            yStatus[0].text = yStatus[1].text = "Y: Get Closer";
            zStatus[0].text = zStatus[1].text = "Z: Get Closer";
            loggingString[0] = deltX.ToString("0.00");
            loggingString[1] = deltY.ToString("0.00");
            loggingString[2] = deltZ.ToString("0.00");
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
            float mThresh = 0.1F;

            string rl = setMoveInstruction("Left ", "Right ", deltX, mThresh);
            string ud = setMoveInstruction("Down ", "Up ", deltY, mThresh);
            string fb = setMoveInstruction("Back ", "Forward ", deltZ, mThresh);

            //Update the status of each dimension
            xStatus[0].text = xStatus[1].text = "X: " + rl + deltX.ToString("0.0");
            yStatus[0].text = yStatus[1].text = "Y: " + ud + deltY.ToString("0.0");
            zStatus[0].text = zStatus[1].text = "Z: " + fb + deltZ.ToString("0.0");

            distance = distance / 0.05F;

            loggingString[0] = deltX.ToString("0.00");
            loggingString[1] = deltY.ToString("0.00");
            loggingString[2] = deltZ.ToString("0.00");

        }
        tPoint.pos.transform.FindChild("point").GetComponent<MeshRenderer>().material.color = new Color(distance, 1 - distance, 0, 0.2F);
    }
    private Quaternion quaternionDifference(Quaternion fromRotation, Quaternion toRotation)
    {
        return fromRotation * Quaternion.Inverse(toRotation);
    }
    private float[] AngleDecomposition(Quaternion angle)//Returns Roll Pitch Yaw
    {
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

    private void CalculateRotation()
    {
        //float angle = Quaternion.Angle(coilHotSpot.transform.rotation, tPoint.rot.transform.rotation);
        //float pitch = coilHotSpot.transform.rotation.eulerAngles.x - tPoint.rot.transform.rotation.eulerAngles.x;
        //float yaw = coilHotSpot.transform.rotation.eulerAngles.y - tPoint.rot.transform.rotation.eulerAngles.y;
        //float roll = coilHotSpot.transform.rotation.eulerAngles.z - tPoint.rot.transform.rotation.eulerAngles.z;
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

        foreach (MeshRenderer renderer in renderers)
        {
            if (!renderer.gameObject.name.Equals("Point"))
            {
                Material transMat = renderer.material;
                Color transColor = new Color(angleDif, 1 - angleDif, 0, 0.2F);
                transMat.color = transColor;
                renderer.material = transMat;
            }
        }

        float rThresh = 1F;

        string r = setRotateInstruction("Cntr Clockwise ", "Clockwise ", rpy[0], rThresh);
        string p = setRotateInstruction("Pitch Up ", "Pitch Down ", rpy[1], rThresh);
        string y = setRotateInstruction("Yaw Right ", "Yaw Left ", rpy[2], rThresh);

        pitchStatus[0].text = pitchStatus[1].text = "Pitch: " + p + rpy[1].ToString("0.00");
        yawStatus[0].text = yawStatus[1].text = "Yaw: " + y + rpy[2].ToString("0.00");
        rollStatus[0].text = rollStatus[1].text = "Roll: " + r + rpy[0].ToString("0.00");

        loggingString[3] = "Pitch: " + rpy[1].ToString("0.00");
        loggingString[4] = "Yaw: " + rpy[2].ToString("0.00");
        loggingString[5] = "Roll: " + rpy[0].ToString("0.00");
    }

    public struct TargetPoint
    {
        public GameObject pos;
        public GameObject rot;
        public string ID;
        public bool fired;

        public TargetPoint(bool fired)
        {
            this.fired = fired;
            pos = null;
            rot = null;
            ID = null;
        }
    }

    public struct Grid
    {
        public IList<TargetPoint> points;
        public string name;

        public Grid(string name, IList<TargetPoint> points)
        {
            this.points = points;
            this.name = name;
        }
    }

    public void AddPoints()
    {
        if (!settingGrid)
        {
            GameObject.Find("StylusTracker").GetComponent<Stylus>().setStylusSensitiveTrackingState(true);
            GameObject.Find("Set Grid").transform.FindChild("Text").GetComponent<Text>().text = "Confirm Grid";
            settingGrid = true;
        }
    }

    public void setScalpHotspotButton(GameObject button)
    {
        if (settingHotSpot == false)
        {
            settingHotSpot = true;
            setHotSpot = button.GetComponentInChildren<Text>();
            setHotSpot.text = "Cancel";
        }
        else
        {
            settingHotSpot = false;
            setHotSpot.text = "Set Hot Spot";
        }
    }
    public void setGridManualButtonPress()
    {
        if (!settingGrid)
        {
            GameObject.Find("StylusTracker").GetComponent<Stylus>().setStylusSensitiveTrackingState(true);
            GameObject.Find("Set Grid").transform.FindChild("Text").GetComponent<Text>().text = "Confirm Grid";
            setGridManual();
        }
        else
        {
            GameObject.Find("StylusTracker").GetComponent<Stylus>().setStylusSensitiveTrackingState(false);
            UnityEngine.Debug.Log("ready to select");
            GameObject.Find("Set Grid").transform.FindChild("Text").GetComponent<Text>().text = "New Manual Grid";

            foreach (TargetPoint point in currentGrid.points)
            {
                TargetPoint a = new TargetPoint();
                TargetPoint b = new TargetPoint();
                TargetPoint c = new TargetPoint();
                float distanceA = -1;
                float distanceB = -1;
                float distanceC = -1;
                int count = 0;
                foreach (TargetPoint p in currentGrid.points)
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

                point.pos.transform.FindChild("point").transform.rotation = point.rot.transform.rotation;
            }

            settingGrid = false;
            usingGrid = true;

            GameObject.Find("ScalpGenerator").GetComponent<ScalpGenerator>().waitingToDraw = true;
        }
    }

    public void setGridManual()
    {
        DestroyCurrentGrid();

        numPoints = 0;
        int numg = numGrids + 1;
        numGrids++;
        IList<TargetPoint> g = new List<TargetPoint>();
        currentGrid = new Grid("Grid" + numg.ToString(), g);
        settingGrid = true;
        GameObject.Find("ScalpGenerator").GetComponent<ScalpGenerator>().waitingToDraw = false;
    }

    public void LogErrorToggle(Text toggleText)
    {
        if (logging)
        {
            logging = false;
            watch.Stop();
            watch.Reset();
            toggleText.text = "Toggle Error Logging";
        }
        else if (!logging)
        {
            logging = true;
            string path = Application.dataPath + @"\Logs\";
            using (System.IO.StreamWriter file =
               new System.IO.StreamWriter(path + currentGrid.name + "_" + tPoint.ID.ToString() + ".txt", true))
            {
                file.WriteLine(tPoint.ID.ToString());
                file.WriteLine(System.DateTime.Now.ToString() + " " + System.DateTime.Now.Millisecond.ToString());
            }
            toggleText.text = "Stop Logging";
            watch.Start();
        }
    }

    private void DestroyCurrentGrid()
    {
        if (currentGrid.points.Count > 0)
        {
            foreach (TargetPoint point in currentGrid.points)
            {
                Destroy(point.pos.transform.FindChild("point").gameObject);
                Destroy(point.pos.gameObject);
                Destroy(point.rot.gameObject);

            }
            currentGrid.points = null;
        }
    }

    public void DestroySelectedPoint()
    {
        if (matching)
        {
            currentGrid.points.RemoveAt(currentGrid.points.IndexOf(tPoint));
            DestroyImmediate(tPoint.pos.transform.FindChild("point").gameObject);
            DestroyImmediate(tPoint.pos.gameObject);
            DestroyImmediate(tPoint.rot.gameObject);
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

        camController.putTargetCam1OnTargetXY(tPoint.pos, head);
        camController.putTargetCam2OnTargetZY(tPoint.pos, head);

        //tCoil.transform.FindChild("model").transform.localScale = Vector3.Scale(tCoil.transform.FindChild("model").transform.localScale, new Vector3(1.1F,1.1F,1.1F)); position/stretch problems

        matching = true;
    }

    public void ExportGrid(int index)
    {
        GameObject center = GameObject.Find("Center");
        string path = Application.dataPath + @"\Grids\Saved";

        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }

        path += @"\" + currentGrid.name + ".txt";

        using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(path, true))
        {
            Grid grid = currentGrid;
            file.WriteLine(grid.name);
            file.WriteLine(grid.points.Count.ToString());
            foreach (TargetPoint point in grid.points)
            {
                //Vector3 p = center.transform.InverseTransformPoint(GameObject.Find("Head").transform.TransformPoint(point.pos.transform.position));
                Vector3 p = center.transform.InverseTransformPoint(point.pos.transform.position);
                if (point.rot != null)
                {
                    Quaternion r = point.rot.transform.rotation;
                    file.WriteLine("0" + "\t" + p.x + "\t" + p.y + "\t" + p.z + "\t" + r.x + "\t" + r.y + "\t" + r.z + "\t" + r.w);
                }
                else
                {
                    file.WriteLine("1" + "\t" + p.x + "\t" + p.y + "\t" + p.z);
                }
            }
            if (scalpHotSpot.pos != null)
            {
                Vector3 p = center.transform.InverseTransformPoint(scalpHotSpot.pos.transform.position);
                Quaternion r = scalpHotSpot.rot.transform.rotation;
                file.WriteLine("0" + "\t" + p.x + "\t" + p.y + "\t" + p.z + "\t" + r.x + "\t" + r.y + "\t" + r.z + "\t" + r.w);

                CreateScalpHotSpot(p, r);
            }
            else
            {
                file.WriteLine("1");
            }
        }
    }

    private void CreateScalpHotSpot(Vector3 p, Quaternion r)
    {
        if (scalpHotSpot.pos != null && scalpHotSpot.pos.transform.FindChild("point") != null)
        {
            DestroyImmediate(scalpHotSpot.pos.transform.FindChild("point").gameObject);
            DestroyImmediate(scalpHotSpot.pos.gameObject);
            DestroyImmediate(scalpHotSpot.rot.gameObject);
        }
        TargetPoint newHotSpot = new TargetPoint(false);
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
        hs.name = "Scalp Hot Spot";

        newHotSpot.pos = hs;
        newHotSpot.rot = hs;
        newHotSpot.ID = hs.name;
        scalpHotSpot = newHotSpot;
    }

    public void ImportGrid()
    {

        DestroyCurrentGrid();

        //String path = EditorUtility.OpenFilePanel("Select Grid", Application.dataPath + @"\Grids", "txt");
        string path = Application.dataPath + @"\Grids\Load";

        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }

        string[] fileNames = System.IO.Directory.GetFiles(path);


        System.IO.FileStream filestream = new System.IO.FileStream(fileNames[0],
                                          System.IO.FileMode.Open,
                                          System.IO.FileAccess.Read,
                                          System.IO.FileShare.Read);
        System.IO.StreamReader file = new System.IO.StreamReader(filestream);


        Grid newGrid = new Grid(file.ReadLine(), new List<TargetPoint>());
        int points = System.Convert.ToInt32(file.ReadLine());

        for (int i = 0; i < points; i++)
        {
            TargetPoint t = new TargetPoint(false);
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
                t.rot.transform.rotation = new Quaternion((float)System.Convert.ToDouble(dims[4]), (float)System.Convert.ToDouble(dims[5]), (float)System.Convert.ToDouble(dims[6]), (float)System.Convert.ToDouble(dims[7]));
            }
            else
            {
                t.rot = null;
            }
            newGrid.points.Add(t);
        }
        string datahs = file.ReadLine();
        char[] dhs = new char[1];
        dhs[0] = '\t';
        string[] dimshs = datahs.Split(dhs);
        if (dimshs[0].Equals("0"))
        {
            Vector3 p = GameObject.Find("Center").transform.TransformPoint(new Vector3((float)System.Convert.ToDouble(dimshs[1]), (float)System.Convert.ToDouble(dimshs[2]), (float)System.Convert.ToDouble(dimshs[3])));
            Quaternion r = new Quaternion((float)System.Convert.ToDouble(dimshs[4]), (float)System.Convert.ToDouble(dimshs[5]), (float)System.Convert.ToDouble(dimshs[6]), (float)System.Convert.ToDouble(dimshs[7]));
            CreateScalpHotSpot(p, r);
        }
        file.Close();

        currentGrid = newGrid;

        foreach (TargetPoint point in currentGrid.points)
        {
            point.pos.transform.parent = GameObject.Find("Head").transform;
            point.pos.name = currentGrid.points.IndexOf(point).ToString();

            //GameObject pshere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //pshere.name = "point";
            //pshere.transform.localScale = new Vector3(0.005F, 0.005F, 0.005F);
            //pshere.transform.position = point.pos.transform.position;
            //pshere.transform.parent = point.pos.transform;

            VisualizePoint(point.pos, point.rot);
        }
        settingGrid = false;
        usingGrid = true;
        matching = false;
    }

    public void resetGrid()
    {
        for (int i = 0; i < currentGrid.points.Count; i++)
        {
            TargetPoint t = currentGrid.points[i];
            t.fired = false;
            Renderer renderer = t.pos.GetComponentInChildren<Renderer>();
            Material mat = renderer.material;
            mat = new Material(Shader.Find("Diffuse"));
            renderer.material = mat;
            currentGrid.points[i] = t;
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
        }
        if (tCoil != null)
        {
            Destroy(tCoil);
        }
        if (tHotSpot != null)
        {
            Destroy(tHotSpot);
        }

        //output firing error

        tPoint.fired = true;
        matching = false;
        Renderer renderer = tPoint.pos.GetComponentInChildren<Renderer>();
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

        currentGrid.points[Int32.Parse(tPoint.ID)] = tPoint;
    }
    public void setGridName(string name)
    {
        currentGrid.name = name;
    }
}
