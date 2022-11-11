using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using DG.Tweening;

public class PSYGUIAnimation : EditorWindow
{
	public PSYAnimation set = null;
	public PSYView view = null;		
	private List<PSYWidget> widgets
    {
        get
        {
            return view.Widgets;
        }
    }

	private static PSYGUIAnimation instance;
	public const float boolField = 15;
	public const float floatField = 30;
	public static bool zoneWidgets = false;
	public static PSYAnimation setToCopy;
	
	static void Init()
	{
		instance = (PSYGUIAnimation)EditorWindow.CreateInstance(typeof(PSYGUIAnimation));
		instance.titleContent = new GUIContent("PSY Editor");
		instance.Show();
	}

	public static void Load(PSYAnimation _set)
	{
		if (instance == null)
			instance = EditorWindow.GetWindow<PSYGUIAnimation>();
		instance.titleContent = new GUIContent("PSY Editor");
		instance.set = _set;
		instance.view = _set.view;
		instance.Show();
		instance.Repaint();		
	}

	public static void DrawSeparator(string text = "", bool shrt = false)
	{
		GUILayout.Space(12f);

		if (Event.current.type == EventType.Repaint)
		{
			Rect rect = GUILayoutUtility.GetLastRect();			
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			GUI.DrawTexture(new Rect(shrt ? (Screen.width * 4f / 10f) : (0), rect.yMin + 6f, shrt ? (Screen.width * 1 / 5f) : (Screen.width), 1f), EditorGUIUtility.whiteTexture);
			GUI.color = Color.white;
		}
	}

	private void OnFocus()
	{
		instance = this;
	}

	public static string Section(string text)
	{
		return "  • " + text;
	}

	public void OnGUI() 
	{
		if (Selection.activeGameObject == null) return;
		if (Selection.activeGameObject.GetComponent<PSYView>() != null && Selection.activeGameObject.GetComponent<PSYView>() != view)
			if (Selection.activeGameObject.GetComponent<PSYView>().GetSet(set.evnt) != null)
				Load(Selection.activeGameObject.GetComponent<PSYView>().GetSet(set.evnt));
		if (set == null || set.view == null) return;

		GUILayout.BeginHorizontal();
		GUILayout.Label(String.Format("{0} - {1}", set.view.name, set.evnt));

		GUILayout.Space(30);
		GUILayout.Label(set.enabled ? "Total time: " + set.TotalTime.ToString() : "(disabled)", GUILayout.MaxWidth(100));
		if(set.enabled)
			GUILayout.Label("Tweens: " + set.items.Count, GUILayout.MaxWidth(100));
		GUILayout.FlexibleSpace();
		if (setToCopy != null && setToCopy.view != null)
		{
			GUILayout.Label(String.Format("Buffer: {0} {1}", setToCopy.view.name, setToCopy.evnt));
			if(GUILayout.Button("Paste", GUILayout.Width(50)))
				set.Set(setToCopy.Clone(set.view));
		}
		if(GUILayout.Button("Copy", GUILayout.Width(40)))
			setToCopy = set;
		
		GUILayout.EndHorizontal();
		DrawSeparator();
		GUILayout.BeginHorizontal();
		BoolField("Enable", ref set.enabled, false, 70);				
		BoolField("Cyclic", ref set.cyclic, false, 70);
		FloatField("Delay", ref set.delay, false, 70);
        if (set.evnt == PSYAnimationEvent.Disable)
            BoolField("Disable on complete", ref set.disableOnComplete, false, 200);
        if (set.evnt == PSYAnimationEvent.Close)
            BoolField("Kill on complete", ref set.killOnComplete, false, 200);
        GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();

		GUILayout.EndHorizontal();
		DrawSeparator();

		if (set != null)
			ASetField(set);
		this.Repaint();
	}

	public PSYAnimation ASetField(PSYAnimation aSet)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(80);
		GUILayout.Label("Type", GUILayout.Width(55));
		GUILayout.Label("Target", GUILayout.Width(75));
		GUILayout.Label("Ease", GUILayout.Width(53));
		GUILayout.Label("Rel", GUILayout.Width(24));		
		GUILayout.Label("Loop", GUILayout.Width(30));
		GUILayout.Label("RndT", GUILayout.Width(34));
		GUILayout.Label("AddT", GUILayout.Width(35));
		GUILayout.Label("Time", GUILayout.Width(34));
		GUILayout.Label("IBD", GUILayout.Width(28));
		GUILayout.Label("Delay", GUILayout.Width(36));
		GUILayout.Label("RndV", GUILayout.Width(34));
		GUILayout.Label("-Rnd", GUILayout.Width(32));
		GUILayout.Label("Z/A", GUILayout.Width(26));				
		GUILayout.Label("Value", GUILayout.Width(102));		
		GUILayout.EndHorizontal();

		int i = 0;
		while (i < aSet.Count)
		{
			aSet[i] = AnimationField(aSet[i++], aSet);
		}

		for (i = aSet.Count - 1; i >= 0; i--)
			if (aSet[i] == null)
				aSet.items.RemoveAt(i);
		
		for (i = aSet.Count - 1; i >= 0; i--)
		{
			bool found = false;
			if(aSet[i].target == PSYGUI.WholeViewAnimationTarget)
				found = true;
			for (int j = widgets.Count - 1; j >= 0; j--)
				if (widgets[j].Transform.name == aSet[i].target)
					found = true;
			if (!found) aSet.Remove(aSet[i]);
		}

		PSYAnimation.Item newOne = new PSYAnimation.Item(PSYAnimation.Type.Scale, PSYGUI.WholeViewAnimationTarget, Vector4.zero, Ease.Linear, 0, 0, 1, false, false, false, false, 0, false, 0);
		if(GUILayout.Button("+", GUILayout.MaxWidth(20)))
			aSet.Add(newOne);
		return aSet;
	}

	public PSYAnimation.Item AnimationField(PSYAnimation.Item anim, PSYAnimation sourceSet)
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
			return null;
		if (GUILayout.Button("Copy", GUILayout.MaxWidth(40)))
			sourceSet.items.Insert(sourceSet.items.IndexOf(anim) + 1, anim.Clone());		
		PSYAnimation.Type newType = (PSYAnimation.Type)EditorGUILayout.EnumPopup(anim.type, GUILayout.Width(50));
		if (anim.type != newType)
			anim.type = newType;
		
		anim.CheckErrors();

		//Filling target list
		List<string> list = new List<string>();
		if (newType != PSYAnimation.Type.Color && newType != PSYAnimation.Type.Size)
			list.Add(PSYGUI.WholeViewAnimationTarget);
	    foreach (PSYWidget w in widgets)
			list.Add(w.name);		

		if (!anim.CheckErrors())
		{
			GUILayout.EndHorizontal();
			return anim;
		}

		string target = anim.target;
		if (newType == PSYAnimation.Type.Color && target == PSYGUI.WholeViewAnimationTarget)
		{
			if (list.Count > 0)
				target = list[0];
			else
				newType = PSYAnimation.Type.Alpha;
		}

		if (newType == PSYAnimation.Type.Size && target == PSYGUI.WholeViewAnimationTarget)
		{
			if (list.Count > 0)
				target = list[0];
			else
				newType = PSYAnimation.Type.Alpha;
		}

		int ind = EditorGUILayout.Popup(list.IndexOf(target), list.ToArray(), GUILayout.Width(70));
		if (ind < 0)
		{
			GUILayout.EndHorizontal();
			return anim;
		}
		anim.target = ind == 0 && newType != PSYAnimation.Type.Color && newType!= PSYAnimation.Type.Size ? PSYGUI.WholeViewAnimationTarget : list[ind];
		anim.ease = (Ease)EditorGUILayout.EnumPopup("", anim.ease, GUILayout.Width(70));		
		GUILayout.Space(5);
		anim.relative = BoolField("", anim.relative);		
		GUILayout.Space(10);
		anim.loops = EditorGUILayout.IntField("", anim.loops, GUILayout.Width(20));
		GUILayout.Space(14);
		anim.randomTime = BoolField("", anim.randomTime);
		GUILayout.Space(11);
		if (anim.randomTime)
			anim.addTime = EditorGUILayout.FloatField("", anim.addTime, GUILayout.Width(floatField));
		else GUILayout.Space(34);
		GUILayout.Space(4);
		anim.time = EditorGUILayout.FloatField("", anim.time, GUILayout.Width(floatField));
		anim.indexBasedDelay = EditorGUILayout.FloatField("", anim.indexBasedDelay, GUILayout.Width(floatField));
		GUILayout.Space(3);
		anim.delay = EditorGUILayout.FloatField("", anim.delay, GUILayout.Width(floatField));
		GUILayout.Space(12);
		anim.random = BoolField("", anim.random);
		GUILayout.Space(20);
		if (anim.random)
			anim.rNegative = BoolField("", anim.rNegative);
		else GUILayout.Space(19);
		GUILayout.Space(12);
		switch (anim.type)
		{
			case PSYAnimation.Type.Color:
			case PSYAnimation.Type.Move: BoolField("", ref anim.useDepth); break;
			default: GUILayout.Space(19); break;
		}
		GUILayout.Space(6);
		switch (anim.type)
		{
			case PSYAnimation.Type.Spin: anim.newValue = PSYAnimation.Item.ParseV4(Vector3Field("", anim.newValue, true)); break;
			case PSYAnimation.Type.Move: anim.newValue = PSYAnimation.Item.ParseV4(Vector3Field("", anim.newValue, true)); break;
			case PSYAnimation.Type.Scale: anim.newValue = PSYAnimation.Item.ParseV4(Vector3Field("", anim.newValue)); break;
			case PSYAnimation.Type.Alpha: anim.newValue = PSYAnimation.Item.ParseV4(Mathf.Clamp01(FloatField("", anim.newValue.x))); break;
			case PSYAnimation.Type.Color: anim.newValue = EditorGUILayout.ColorField(PSYAnimation.Item.ParseColor(anim.newValue), GUILayout.Width(2.1f * floatField)); break;
			case PSYAnimation.Type.Size: anim.newValue = PSYAnimation.Item.ParseV4(Vector2Field("", anim.newValue)); break;
		}
		GUILayout.EndHorizontal();
		return anim;
	}

	public static bool BoolField(string s, ref bool value, bool newLine = false, float width = 0)
	{
		if (newLine) GUILayout.BeginHorizontal();
		value = EditorGUILayout.Toggle("", value, GUILayout.Width(boolField));
		if (s != "" && width != 0) GUILayout.Label(s, GUILayout.Width(width));
		else if (s != "") GUILayout.Label(s);
		if (newLine) GUILayout.EndHorizontal();
		return value;
	}

	public static bool BoolField(string s, bool value, bool newLine = false, float width = 0)
	{
		if (newLine) GUILayout.BeginHorizontal();
		value = EditorGUILayout.Toggle("", value, GUILayout.Width(boolField));
		if (s != "" && width != 0) GUILayout.Label(s, GUILayout.Width(width));
		else if (s != "") GUILayout.Label(s);
		if (newLine) GUILayout.EndHorizontal();
		return value;
	}

	public static Vector3 Vector3Field(string s, ref Vector3 v3, bool _3 = false)
	{
		float x, y, z = 0;
		x = EditorGUILayout.FloatField("", v3.x, GUILayout.Width(30));
		y = EditorGUILayout.FloatField("", v3.y, GUILayout.Width(30));
		if (_3) z = EditorGUILayout.FloatField("", v3.z, GUILayout.Width(30));
		v3 = new Vector3(x, y, z);
		return v3;
	}

	public static Vector3 Vector2Field(string s, Vector2 v2)
	{
		float x, y, z = 0;
		x = EditorGUILayout.FloatField("", v2.x, GUILayout.Width(30));
		y = EditorGUILayout.FloatField("", v2.y, GUILayout.Width(30));
		v2 = new Vector3(x, y, z);
		return v2;
	}

	public static Vector3 Vector3Field(string s, Vector3 v3, bool _3 = false)
	{
		float x, y, z = 0;
		x = EditorGUILayout.FloatField("", v3.x, GUILayout.Width(30));
		y = EditorGUILayout.FloatField("", v3.y, GUILayout.Width(30));
		if (_3) z = EditorGUILayout.FloatField("", v3.z, GUILayout.Width(30));
		v3 = new Vector3(x, y, z);
		return v3;
	}

	public static float FloatField(string s, ref float value, bool newLine = false, float width = 0)
	{
		if (newLine) GUILayout.BeginHorizontal();
		value = EditorGUILayout.FloatField("", value, GUILayout.Width(floatField));
		if (s != "") GUILayout.Label(s, GUILayout.Width(width));
		if (newLine) GUILayout.EndHorizontal();
		return value;
	}

	public static float IntField(string s, ref int value, bool newLine = false, float width = floatField)
	{
		if (newLine) GUILayout.BeginHorizontal();
		value = EditorGUILayout.IntField("", value, GUILayout.Width(width));
		if (s != "") GUILayout.Label(s);
		if (newLine) GUILayout.EndHorizontal();
		return value;
	}

	public static float FloatField(string s, float value, bool newLine = false, float width = floatField)
	{
		if (newLine) GUILayout.BeginHorizontal();
		value = EditorGUILayout.FloatField("", value, GUILayout.Width(width));
		if (s != "") GUILayout.Label(s);
		if (newLine) GUILayout.EndHorizontal();
		return value;
	}
}
