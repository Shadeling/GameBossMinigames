using UnityEngine;
using UnityEditor;
using System.Linq;

public class PSYDebugEditor : EditorWindow
{
    private PSYDebug debug = null;
    private string newPath = "";
    private static PSYDebugEditor window = null;

    [MenuItem ("XD/PSYGUI/Debug Editor")]
    static void Init () 
	{
        window = GetWindow<PSYDebugEditor>("PSYDebugEditor");
        window.position = new Rect(200, 300, 274, 508);
        window.minSize = new Vector2(274, 508);
        window.maxSize = new Vector2(275, 509);
        window.CheckPath(PlayerPrefs.GetString("PSYDebugPath", "Entities/UI/PSYDebug"));
    }

    public void CheckPath(string path)
    {
        PSYDebug debug = (PSYDebug)Resources.Load(path, typeof(PSYDebug));
        if (debug != null)
        {
            PSYDebug.Log(PSYDebug.Source.Temp, "New PSYDebug {0}", debug);
            this.debug = debug;
            newPath = path;
            PlayerPrefs.SetString("PSYDebugPath", path);
        }
    }

	public void OnGUI()
	{
        GUILayout.BeginHorizontal();
        newPath = GUILayout.TextField(newPath, GUILayout.MaxWidth(200));

        if (GUI.changed)
        {
            CheckPath(newPath);
        }

        if (debug == null)
        {
            return;
        }

        if(GUILayout.Button("Select", GUILayout.MaxWidth(65)))
        {
            Selection.activeObject = debug;
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
		GUILayout.Label("Enabled: ", GUILayout.MaxWidth(60));
        debug.enabled = GUILayout.Toggle(debug.enabled, "", GUILayout.MaxWidth(100));

        GUILayout.Label("Warnings: ", GUILayout.MaxWidth(60));
        debug.alwaysShowWarnings = GUILayout.Toggle(debug.alwaysShowWarnings, "", GUILayout.MaxWidth(100));

        GUILayout.Label("Errors: ", GUILayout.MaxWidth(60));
        debug.alwaysShowErrors = GUILayout.Toggle(debug.alwaysShowErrors, "", GUILayout.MaxWidth(100));
        GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Filter: ", GUILayout.MaxWidth(60));
        for (int i = 0; i < debug.filter.Length; i++)
        {
            debug.filter[i] = GUILayout.TextField(debug.filter[i], GUILayout.MaxWidth(100));
        }
		GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(); 
        GUILayout.Label("Ignore: ", GUILayout.MaxWidth(60));
        for (int i = 0; i < debug.ignore.Length; i++)
        {
            debug.ignore[i] = GUILayout.TextField(debug.ignore[i], GUILayout.MaxWidth(100));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Force: ", GUILayout.MaxWidth(60));
        debug.force = EditorGUILayout.TextField(debug.force, GUILayout.MaxWidth(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Color: ", GUILayout.MaxWidth(60));
        debug.mainColor = EditorGUILayout.ColorField(debug.mainColor, GUILayout.MaxWidth(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Context: ", GUILayout.MaxWidth(60));
        debug.contextColor = EditorGUILayout.ColorField(debug.contextColor, GUILayout.MaxWidth(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
		GUILayout.Label("-----------------------------------------------------");
		GUILayout.EndHorizontal();

		for (int i = 0; i < debug.items.Count; i++)
		{
			GUILayout.BeginHorizontal();
			PSYDebug.Source type = (PSYDebug.Source)i;
            debug.items[i].enabled = EditorGUILayout.Toggle(debug.items[i].enabled, GUILayout.MaxWidth(20));
            debug.items[i].color = EditorGUILayout.ColorField(debug.items[i].color, GUILayout.MaxWidth(40));
            GUILayout.Label(debug.items[i].type.ToString(), GUILayout.MaxWidth(100));
			GUILayout.Label(PSYDebug.debugCalls.ContainsKey((int)type) ? PSYDebug.debugCalls[(int)type].ToString() : "0", GUILayout.MaxWidth(60));
			GUILayout.EndHorizontal();
		}

		int total = 0;
		foreach (var pair in PSYDebug.debugCalls)
			total += pair.Value;
		GUILayout.BeginHorizontal();
		GUILayout.Space(72);
		GUILayout.Label("Total: ", GUILayout.MaxWidth(100));
        GUILayout.Label(total.ToString(), GUILayout.MaxWidth(60));
        GUILayout.EndHorizontal();
		this.Repaint();
        EditorUtility.SetDirty(debug);
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
