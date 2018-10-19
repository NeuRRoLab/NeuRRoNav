using UnityEngine;
using System.Collections;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System.Windows.Forms;
#endif


public class FileExplorerButton : MonoBehaviour {

	public SettingsMenu.settings field;
	SettingsMenu settingsMenu;

	void Awake() {
		settingsMenu = FindObjectOfType<SettingsMenu>();
	}

	public void OnClickButton() {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
		// Folder
		if ((int)field % 2 == 0) {
            /*
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			string startPath = settingsMenu.getField((int)field);
			fbd.RootFolder = System.Environment.SpecialFolder.DesktopDirectory;
			fbd.SelectedPath = startPath;
			if (fbd.ShowDialog() == DialogResult.OK) {
				string correctedPath = fbd.SelectedPath.Replace('\\', '/');
				correctedPath += '/';
				settingsMenu.setField((int)field, correctedPath);
			}*/
            string startPath = settingsMenu.getField((int)field );
            
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = startPath;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    settingsMenu.setField((int)field, dialog.SelectedPath);
                }
            }
        }
		// File
		else {
			OpenFileDialog ofd = new OpenFileDialog();
			string startPath = settingsMenu.getField((int)field - 1);
			//ofd.InitialDirectory = startPath;
			if (ofd.ShowDialog() == DialogResult.OK) {
				string path = System.IO.Path.GetDirectoryName(ofd.FileName);
				string fileName = System.IO.Path.GetFileName(ofd.FileName);
				settingsMenu.setField((int)field - 1, path);
				settingsMenu.setField((int)field, fileName);

			}
		}
#endif
	}
}
