using UnityEngine;
using System.Collections;

public class CamPosDropDown : MonoBehaviour {

    bool hidden;
    bool hoverSelf;
    bool hoverChild;
    GameObject menu;
    Vector3 defaultPos;
    Vector3 hide;

	// Use this for initialization
	void Start () {
        hoverSelf = false;
        hoverChild = false;
        hidden = true;
        menu = transform.parent.FindChild("List").gameObject;
        defaultPos = menu.transform.position;
        hide = new Vector3(Screen.width*2, Screen.height*2, 0);
        Hide();

	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Mouse0) && !hoverSelf && !hoverChild && !hidden)
        {
            Hide();
        }

	}

    void OnMouseEnter()
    {
        hoverSelf = true;
    }

    void OnMouseExit()
    {
        hoverSelf = false;
    }

    public void Click()
    {
        if(hidden)
        {
            Show();
        }
    }

    public void ChildClick()
    {
        Hide();
        hoverSelf = false;
        hoverChild = false;
    }

    private void Hide()
    {
        menu.transform.position = hide;
        hidden = true;
    }
    private void Show()
    {
        menu.transform.position = defaultPos;
        hidden = false;
    }

    public void SetChildHover(bool setHover)
    {
        hoverChild = setHover;
    }
}
