using UnityEngine;
using System.Collections;

public class camList : MonoBehaviour {

    CamPosDropDown parentScript;

    // Use this for initialization
    void Start()
    {
        parentScript = gameObject.transform.parent.FindChild("PosButton").GetComponent<CamPosDropDown>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Hover()
    {
        parentScript.SetChildHover(true);
    }

    public void Exit()
    {
        parentScript.SetChildHover(false);
    }
}
