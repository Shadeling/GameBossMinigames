using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using XD;

public class PSYStatusInfo : EditorWindow
{
	private static PSYStatusInfo instance = null;
	private static string outText = "";
	List<GameObject> assets = new List<GameObject>();
	List<PSYView> views = new List<PSYView>();
	List<PSYController> conts = new List<PSYController>();
	List<PSYView> allV = new List<PSYView>();
	List<PSYController> allC = new List<PSYController>();
	List<PSYView> windows = new List<PSYView>();
	[MenuItem ("XD/PSYGUI/Status Info")]
    static void Init () 
	{
		if (instance != null) return;
		PSYStatusInfo window = (PSYStatusInfo)EditorWindow.CreateInstance(typeof(PSYStatusInfo));
		window.Show();
		window.titleContent = new GUIContent("PSYStatusInfo");
		instance = window;
    }

	public void OnDestroy()
	{
		instance = null;
	}

	public void OnGUI() 
	{
		GUILayout.BeginHorizontal();
		bool refresh = GUILayout.Button("Refresh assets", GUILayout.MaxWidth(120));
		GUILayout.EndHorizontal();
		IEnumerable<string> assetFolderPaths = null;

		List<PSYItem> types = new List<PSYItem>();		
		if (refresh)
		{
			outText = "";
			assetFolderPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".prefab") && path.Contains("UI"));
			assets = assetFolderPaths.Select(item => AssetDatabase.LoadAssetAtPath(item, typeof(GameObject))).Cast<GameObject>().Where(obj => obj != null).ToList();
			views = assets.Where(item => item.GetComponent<PSYView>() != null).Select(item => item.GetComponent<PSYView>()).Cast<PSYView>().ToList();
			conts = assets.Where(item => item.GetComponent<PSYController>() != null).Select(item => item.GetComponent<PSYController>()).Cast<PSYController>().ToList();
            allV = new List<PSYView>();
            allV.AddRange(views);
            allC = new List<PSYController>();
            allC.AddRange(conts);

            foreach (PSYView v in views)
            {
                PSYTools.GetChildObjects(v.transform, allV, true);
            }

            foreach (PSYController c in conts)
            {
                PSYTools.GetChildObjects(c.transform, allC, true);
            }

			outText += "\nTotal PSYView components: " + allV.Count.ToString();
			outText += "\nTotal PSYController components: " + allC.Count.ToString();

			//Not presented controllers types
			outText += "\n\nControllers types, not presented in prefabs:";
			types = new List<PSYItem>(Enum.GetValues(typeof(PSYItem)).Cast<PSYItem>().ToList());			
			foreach (PSYController cont in allC)
				if(types.Contains(cont.view.item))
					types.Remove(cont.view.item);
			foreach (PSYItem type in types)
				outText += "\n" + type.ToString();
			outText += "\n\n";

			int n = 0;
            foreach (PSYView v in allV)
            {
                for (int i = 0; i < v.Widgets.Count; i++)
                {
                    bool auto = false;
                    if (!v.Widgets[i].isImage)
                    {
                        continue;
                    }

                    if (v.Widgets[i].Image.sprite == null)
                    {
                        string parent = "";
                        if (v.transform.parent != null)
                        {
                            parent = v.transform.parent.name;

                            if (v.transform.parent.parent != null)
                            {
                                parent = v.transform.parent.parent.name + "/" + v.transform.parent.name;

                                if (v.transform.parent.parent.parent != null)
                                {
                                    parent = v.transform.parent.parent.parent.name + "/" + v.transform.parent.parent.name + "/" + v.transform.parent.name;

                                    if (v.transform.parent.parent.parent.parent != null)
                                    {
                                        parent = v.transform.parent.parent.parent.parent.name + "/" + v.transform.parent.parent.parent.name + "/" + v.transform.parent.parent.name + "/" + v.transform.parent.name;
                                    }
                                }
                            }
                        }
                        outText += "\n   " + parent + "/" + v.name + " " + v.Widgets[i].name + (auto ? "   AUTO" : "");
                        continue;
                    }

                    string path = AssetDatabase.GetAssetPath(v.Widgets[i].Image.sprite);
                    
                    if (path != null && path.Contains("Assets/_AssetBundles/UI/Sprites"))
                    {                        
                        string parent = "";
                        if(v.transform.parent != null)
                        {
                            parent = v.transform.parent.name;

                            if (v.transform.parent.parent != null)
                            {
                                parent = v.transform.parent.parent.name + "/" + v.transform.parent.name;

                                if (v.transform.parent.parent.parent != null)
                                {
                                    parent = v.transform.parent.parent.parent.name + "/" + v.transform.parent.parent.name + "/" + v.transform.parent.name;

                                    if (v.transform.parent.parent.parent.parent != null)
                                    {
                                        parent = v.transform.parent.parent.parent.parent.name + "/" + v.transform.parent.parent.parent.name + "/" + v.transform.parent.parent.name + "/" + v.transform.parent.name;
                                    }
                                }
                            }
                        }
                        outText += "\n " + path + "   " + parent + "/" + v.name + " " + v.Widgets[i].name + (auto ? "   AUTO" : "");
                    }
                }

                EditorUtility.SetDirty(v);
            }

            /*foreach(var pair in full)
            {
                outText += "\n " + pair.Key + " " + pair.Value;
            }*/

			outText += n.ToString() + "\n\n";

			//List of windows
			windows.Clear();
			for (int i = 0; i < allV.Count; i++)
			{
				if (allV[i].type == PSYViewType.Window)
					windows.Add(allV[i]);
			}
		}

		if (outText != "") GUILayout.TextArea(outText, GUILayout.Height(500));
		this.Repaint();
	}

	private bool BoolField(string s, ref bool value, bool newLine = false)
	{
		if (newLine) GUILayout.BeginHorizontal();
		value = EditorGUILayout.Toggle("", value, GUILayout.Width(10));
		if (s != "") GUILayout.Label(s);
		if (newLine) GUILayout.EndHorizontal();
		return value;
	}

	private static int CompareTransform(PSYView A, PSYView B)
	{
		return A.name.CompareTo(B.name);
	}
}
