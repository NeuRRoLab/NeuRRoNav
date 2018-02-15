using UnityEngine;
using System.Collections;
using System;

public class CameraController : MonoBehaviour
{

    GameObject mainCamera;
    GameObject targetCamera1;
    GameObject targetCamera2;
    enum targetNames : int { mainTarget = 0, targetCamera1Target = 1, targetCamera2Target = 2 };

    public GameObject[] cameras = new GameObject[3];
    public GameObject[] targets = new GameObject[3];

    public int activeCamera = 0;

    Vector3 lastMousePos;

    GameObject scalp;
    GameObject stylusPoint;
    GameObject coilHotSpot;

    bool stylusCam;
    bool coilCam;
    bool listenToMouse;

    int mouseFlip = -1;

    void Start()
    {
        targetCamera1 = GameObject.Find("TargetCam1");
        targetCamera2 = GameObject.Find("TargetCam2");
        mainCamera = Camera.main.gameObject;

        listenToMouse = false;

        cameras[0] = mainCamera;
        cameras[1] = targetCamera1;
        cameras[2] = targetCamera2;
        //targets[0] = mainTarget;
        //targets[1] = targetCamera1Target;
        //targets[2] = targetCamera2Target;

        GameObject obj = new GameObject();
        obj.transform.rotation = Quaternion.identity;
        obj.transform.position = Vector3.zero;
        putTargetCam1OnTargetXZ(obj);
        putTargetCam2OnTargetZY(obj);
        centerMainOnObject(obj, Vector3.forward, 1);
    }

    void Update()
    {
        if (stylusCam)
        {

            if (stylusPoint != null)
            {
                if (targets[(int)targetNames.mainTarget].Equals(stylusPoint))
                {
                    mainCamera.transform.LookAt(stylusPoint.transform.position, GameObject.Find("Head").transform.up);
                }
                if (targets[(int)targetNames.targetCamera1Target].Equals(stylusPoint))
                {
                    targetCamera1.transform.LookAt(stylusPoint.transform.position, GameObject.Find("Head").transform.up);
                }
                if (targets[(int)targetNames.targetCamera2Target].Equals(stylusPoint))
                {
                    targetCamera2.transform.LookAt(stylusPoint.transform.position, GameObject.Find("Head").transform.up);
                }
            }
        }
        if (coilCam)
        {
            if (targets[(int)targetNames.mainTarget].Equals(coilCam))
            {
                mainCamera.transform.LookAt(coilHotSpot.transform.position, GameObject.Find("Head").transform.up);
            }
            if (targets[(int)targetNames.targetCamera1Target].Equals(coilCam))
            {
                targetCamera1.transform.LookAt(coilHotSpot.transform.position, GameObject.Find("Head").transform.up);
            }
            if (targets[(int)targetNames.targetCamera2Target].Equals(coilCam))
            {
                targetCamera2.transform.LookAt(coilHotSpot.transform.position, GameObject.Find("Head").transform.up);
            }
        }
        bool mouse0 = false;
        float mouseScroll = Input.mouseScrollDelta.y;
        if (Input.GetKey(KeyCode.Mouse0))
        {
            mouse0 = true;
        }
        if (listenToMouse)
        {
            if (mouse0)
            {
                rotateCamera(targets[activeCamera], cameras[activeCamera]);
            }
            if (Math.Abs(mouseScroll) > 0)
            {
                zoomCamera(targets[activeCamera], cameras[activeCamera], mouseScroll);
            }
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            mouseFlip = -1;
        }
        lastMousePos = Input.mousePosition;
    }

    public void centerMainOnObject(GameObject target, Vector3 forward, float distance)
    {
        targets[0] = target;
        mainCamera.transform.parent = targets[(int)targetNames.mainTarget].transform;
		mainCamera.transform.position = targets[(int)targetNames.mainTarget].transform.position - forward * distance;

		mainCamera.transform.LookAt(target.transform);
    }

    public void putCamOnStylus(int camera)
    {
        GameObject cam = cameras[camera];
		GameObject stylus = GameObject.Find("Stylus");

		stylusPoint = stylus.transform.FindChild("Point").gameObject;
        targets[camera] = stylusPoint;
        cam.transform.position = Vector3.Lerp(stylusPoint.transform.position, stylus.transform.position, 0.95F);
        cam.transform.LookAt(stylusPoint.transform.position);
        cam.transform.parent = stylus.transform;
        stylusCam = true;
    }

    public void putCamOnCoil(int camera)
    {
        try
        {
            GameObject coil = GameObject.Find(GameObject.Find("CoilTracker").GetComponent<Coil>().coilName);
            GameObject cam = cameras[camera];
            coilHotSpot = coil.transform.FindChild("container").FindChild("hotspot").gameObject;
            targets[camera] = coilHotSpot;
            cam.transform.position = Vector3.Lerp(coilHotSpot.transform.position, coil.transform.position, 0.95F);
            cam.transform.LookAt(coilHotSpot.transform.position, coilHotSpot.transform.forward);
            cam.transform.parent = coil.transform;
            coilCam = true;
        }
        catch (NullReferenceException e)
        {

        }
    }


    public void putMainCamOnTargetXY(GameObject tPoint) {
		try {
			GameObject cam = cameras[0];
            cam.transform.position = tPoint.transform.position;
            cam.transform.parent = coilHotSpot.transform;
            cam.transform.Translate((-coilHotSpot.transform.forward / 2));
            //cam.transform.localPosition = new Vector3(targetCamera1.transform.localPosition.x, targetCamera1.transform.localPosition.y, targetCamera1.transform.localPosition.z - 0.5F);
            cam.transform.LookAt(tPoint.transform.position, GameObject.Find("Scalp").transform.up);
            cam.transform.parent = tPoint.transform;
            targets[0] = tPoint;
		}
		catch (NullReferenceException e) {

		}
	}

    public void putTargetCam1OnTargetXZ(GameObject tPoint)
    {
        stylusCam = false;
        targetCamera1 = GameObject.Find("TargetCam1");
        targetCamera1.transform.position = tPoint.transform.position;
        targetCamera1.transform.parent = tPoint.transform;
        targetCamera1.transform.localPosition = new Vector3(targetCamera1.transform.localPosition.x, targetCamera1.transform.localPosition.y + 0.5F, targetCamera1.transform.localPosition.z);
        targetCamera1.transform.LookAt(tPoint.transform.position, GameObject.Find("Scalp").transform.forward);
        targets[1] = tPoint;
    }

    public void putTargetCam2OnTargetZY(GameObject tPoint)
    {
        targetCamera2 = GameObject.Find("TargetCam2");
        targetCamera2.transform.position = tPoint.transform.position;
        targetCamera2.transform.parent = tPoint.transform;
        targetCamera2.transform.localPosition = new Vector3(targetCamera2.transform.localPosition.x - 0.5F, targetCamera2.transform.localPosition.y, targetCamera2.transform.localPosition.z);
        targetCamera2.transform.LookAt(tPoint.transform.position, GameObject.Find("Scalp").transform.up);
        targets[2] = tPoint;
    }

    public void putMainCamOnTarget(string target)
    {
        try
        {
            if (target.Equals("tPoint"))
            {
                positionCamera(GameObject.Find("TargetMatching").GetComponent<TargetMatching>().tPoint.pos, 0);
            }
            else
            {
                positionCamera(GameObject.Find(target), 0);
            }
        }
        catch (NullReferenceException e)
        {

        }
    }
    public void putT1CamOnTarget(string target)
    {
        try
        {
            if (target.Equals("tPoint"))
            {
                positionCamera(GameObject.Find("TargetMatching").GetComponent<TargetMatching>().tPoint.pos, 1);
            }
            else
            {
                positionCamera(GameObject.Find(target), 1);
            }
        }
        catch (NullReferenceException e)
        {

        }
    }
    public void putT2CamOnTarget(string target)
    {
        try
        {
            if (target.Equals("tPoint"))
            {
                positionCamera(GameObject.Find("TargetMatching").GetComponent<TargetMatching>().tPoint.pos, 2);
            }
            else
            {
                positionCamera(GameObject.Find(target), 2);
            }
        }
        catch (NullReferenceException e)
        {

        }
    }
    private void positionCamera(GameObject target, int cam)
    {
        cameras[cam].transform.position = target.transform.position;
        cameras[cam].transform.parent = target.transform;
        cameras[cam].transform.position = Vector3.MoveTowards(cameras[cam].transform.position, cameras[cam].transform.forward, -1);
        targets[cam] = target;
    }

    //public void addGridToViews(GameObject targetCenter)
    //{
    //    GridOverlay overlay = mainCamera.GetComponent<GridOverlay>();
    //    //overlay.startX = (
    //}

    private void rotateCamera(GameObject target, GameObject cam)
    {
        cam.transform.LookAt(target.transform.position);
        cam.transform.RotateAround(target.transform.position, Vector3.up, (Input.mousePosition.x - lastMousePos.x));
        cam.transform.RotateAround(target.transform.position, cam.transform.right, mouseFlip * (Input.mousePosition.y - lastMousePos.y));
        if (Vector3.Dot(Vector3.up, cam.transform.up) <= 0)
        {
            mouseFlip *= -1;
        }
    }

    private void zoomCamera(GameObject target, GameObject cam, float mouseDelta)
    {
        //cam.transform.LookAt(target.transform.position);
        Vector3 newPos = Vector3.MoveTowards(cam.transform.position, target.transform.position, Math.Sign(mouseDelta) * 0.05F);
        if (Vector3.Distance(newPos, target.transform.position) > 0.1F)
        {
            cam.transform.position = newPos;
        }
    }

    public void ToggleSkybox()
    {
        foreach (GameObject c in cameras)
        {
            Camera cam = c.GetComponent<Camera>();
            if (cam.clearFlags == CameraClearFlags.SolidColor)
            {
                c.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
            }
            else
            {
                c.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            }
        }
    }

    public void SetActiveCamera(int panel)
    {
        activeCamera = panel;
    }

    public void SetListenToMouse(bool b)
    {
        listenToMouse = b;
    }
}
