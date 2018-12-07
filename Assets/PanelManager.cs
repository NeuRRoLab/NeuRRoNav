using UnityEngine;
using System.Collections;

public class PanelManager : MonoBehaviour {

    public RectTransform activePanel;
    public RectTransform goalPoint;

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void activatePanel(RectTransform cur_panel) {
        if (activePanel)
        {
            activePanel.localPosition = Vector2.up * 10000;
        }
        activePanel = cur_panel;
        activePanel.localPosition = goalPoint.localPosition;


    }
}
