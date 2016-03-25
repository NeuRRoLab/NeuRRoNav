using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SettingsMenu : MonoBehaviour {

    bool hidden;
    Vector3 defaultPos;
    GameObject menu;
    CameraController camController;

    string[] fields;
    InputField[] inputs;
    public enum settings { loggingPath, loggingName, coilSavePath, coilSaveName, coilLoadPath, coilLoadName, gridSavePath, gridSaveName, gridLoadPath, gridLoadName };


	// Use this for initialization
	void Start ()
    {
        fields = new string[10];
        inputs = GameObject.Find("Panels").GetComponentsInChildren<InputField>();

        menu = GameObject.Find("SettingMenu");
        camController = GameObject.Find("Camera Controller").GetComponent<CameraController>();
        
        hidden = true;
        defaultPos = menu.transform.localPosition;

        Initialize();
        GameObject.Find("TargetMatching").GetComponent<TargetMatching>().setGridName(fields[(int)settings.gridSaveName]);

        Hide();
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void Hide()
    {
        hidden = !hidden;
        if(hidden)
        {
            menu.transform.localPosition = defaultPos;
            camController.SetListenToMouse(true);
        }
        else
        {
            menu.transform.localPosition = Vector3.zero;
            camController.SetListenToMouse(false);
        }
    }

    public void setField(int field, string input)
    {
        fields[field] = input;
        if(field == 0 || field %2 == 0)
        {
            makeDir(field, field+1);
        }
        else
        {
            makeDir(field - 1, field);
        }
        if(field == (int)settings.loggingName)
        {
            GameObject.Find("TargetMatching").GetComponent<TargetMatching>().setLoggingName(fields[field]);
        }
        else if(field == (int)settings.loggingPath)
        {
            GameObject.Find("TargetMatching").GetComponent<TargetMatching>().setLoggingPath(fields[field]);
        }
        else if(field == (int)settings.gridSaveName)
        {
            GameObject.Find("TargetMatching").GetComponent<TargetMatching>().setGridName(fields[field]);
        }
    }

    public void setField(int field)
    {
        setField(field, inputs[field].text);
    }

    public string getField(int field)
    {
        return fields[field];
    }

    private void Initialize()
    {
        fields[(int)settings.loggingPath] = Application.dataPath + @"/Logs/";
        fields[(int)settings.loggingName] = "Grid1_" + string.Format("session-{0:yyyy-MM-dd_hh-mm-ss-tt}", System.DateTime.Now) + ".txt";

        fields[(int)settings.coilSavePath] = Application.dataPath + @"/Coils/Saved/";
        fields[(int)settings.coilSaveName] = "Coil1_" + string.Format("session-{0:yyyy-MM-dd_hh-mm-ss-tt}", System.DateTime.Now) + ".txt";

        fields[(int)settings.coilLoadPath] = Application.dataPath + @"/Coils/Load/";
        fields[(int)settings.coilLoadName] = fields[(int)settings.coilSaveName];

        fields[(int)settings.gridSavePath] = Application.dataPath + @"/Grids/Saved/";
        fields[(int)settings.gridSaveName] = fields[(int)settings.loggingName];

        fields[(int)settings.gridLoadPath] = Application.dataPath + @"/Grids/Load/";
        fields[(int)settings.gridLoadName] = fields[(int)settings.loggingName];

        int i = 0;
        foreach (InputField input in GameObject.Find("Panels").GetComponentsInChildren<InputField>())
        {
            input.text = fields[i];
            i++;
        }

        for(int j = 0; j < fields.Length; j+=2)
        {
            makeDir(j, j + 1);
        }
    }

    private void makeDir(int path, int name)
    {
        if(fields[name].Length > 4 && fields[name].Substring(fields[name].Length-4,4) == ".txt")
        {
            fields[name] = fields[name].Remove(fields[name].Length - 4, 4);
        }
        if (!System.IO.Directory.Exists(fields[path]))
        {
            System.IO.Directory.CreateDirectory(fields[path]);
        }
        if(System.IO.Directory.Exists(fields[path] + fields[name] + ".txt"))
        {
            int appendNum = 1;
            string newName = fields[name] + "("+ appendNum +")";
            while(System.IO.Directory.Exists(path + newName + ".txt"))
            {
                appendNum++;
                newName = fields[name] + "(" + appendNum + ")";
            }
            fields[name] = newName + ".txt";
        }
        else
        {
            fields[name] = fields[name] + ".txt";
        }
        inputs[name].text = fields[name];
    }

    public void incrementField(int field)
    {
        if(fields[field].Length > 4 && fields[field].Substring(fields[field].Length - 4,4) == ".txt")
        {
            string subName = fields[field].Remove(fields[field].Length - 4, 4);
            int result = 1;
            if(subName.Length > 3 && subName.Substring(subName.Length-3, 3) == "(" + subName.Substring(subName.Length - 2,1) + ")" && int.TryParse(subName.Substring(subName.Length - 2, 1), out result))
            {
                ++result;
                subName = subName.Remove(subName.Length - 3, 3);
            }
            inputs[field].text = fields[field] = subName + "(" + result + ").txt";
        }
    }
}
