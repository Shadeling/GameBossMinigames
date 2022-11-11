#if !DISABLE_PSY
#define PSY_ENABLED
#endif

using UnityEngine;
using System;
using System.Collections.Generic;
using XD;

[CreateAssetMenu (menuName = "XD/PSYDebug")]
public class PSYDebug: ScriptableObject
{
    public enum Source
	{
		Animation,		
		Controller,				
		Core,
		Editor,
		Events,
		Input,
		Manager,				
		Model,
		Params,
		Registration,
		Setup,
		Sound,		
		Temp,
		View,
		ViewAnimation,
		ViewEvent,
		ViewState,
		Widget,

        ARGUEInput,
        Logic,
	}

    [Serializable]
    public class Item
    {
        public bool enabled = false;
        public Source type = Source.Core;
        public Color color = new Color(0.565f, 0.565f, 0.565f);

        public Item(Source type, bool enabled, Color color)
        {
            this.type = type;
            this.enabled = enabled;
            this.color = color;
        }

        public Item(Source type, bool enabled = false)
        {
            this.type = type;
            this.enabled = enabled;
        }
    }

    public static PSYDebug          instance = null;
	public bool                     enabled = true;
	public string                   force = ">>>";
    public string[]                 filter = new string[2] { "", ""};
    public string[]                 ignore = new string[2] { "", ""};
    public Color                    mainColor = new Color(0.0f, 0.5f, 0.5f);
    public Color                    contextColor = new Color(0.565f, 0.565f, 0.565f);
    public bool                     alwaysShowWarnings = false;
    public bool                     alwaysShowErrors = true;

    private bool HasFilter
    {
        get
        {
            for (int i = 0; i < filter.Length; i++)
            {
                if (filter[i] != "")
                {
                    return true;
                }
            }
            return false;
        }
    }

    public List<Item> items = new List<Item>()
	{
		
	};

    public Dictionary<Source, Item> itemsDict = new Dictionary<Source, Item>()
    {

    };

    public static Dictionary<int, int> debugCalls = new Dictionary<int, int>()
	{

	};

    public static void GetInstance()
    {
        string path = PlayerPrefs.GetString("PSYDebugPath", "Entities/UI/PSYDebug");        
        instance = (PSYDebug)Resources.Load(path, typeof(PSYDebug));
        if(instance == null)
        {
            PSYDebug.Error(Source.Core, "You need to setup PSYDebug.asset");
            return;
        }

        Debug.Log("PSYDebug instance loaded at path: " + path);
        Log(Source.Core, "PSYDebug instance loaded at path: {0}", path);
        if(instance.items == null || instance.items.Count == 0)
        {
            instance.items = new List<Item>() { };
            for (int i = 0; i < PSYTools.EnumSize(typeof(Source)); i++)
            {
                instance.items.Add(new Item(PSYTools.ListFromEnum<Source>()[i]));
            }
        }

        for (int i = 0; i < instance.items.Count; i++)
        {
            instance.itemsDict.Add(instance.items[i].type, instance.items[i]);
        }
    }

    [System.Diagnostics.Conditional("PSY_ENABLED")]
	public static void Log(Source type, string formatedString, params object[] msgs)
	{
        if (instance == null)
        {
            if(!Application.isPlaying)
            {
                Debug.Log("PSYDEBUGEDITOR: " + String.Format(formatedString, msgs));
            }
            return;
        }

        instance.Out(type, LogType.Log, formatedString, msgs);
	}

	[System.Diagnostics.Conditional("PSY_ENABLED")]
	public static void Log(Source type, PSYView context, string formatedString, params object[] msgs)
	{
        if (instance == null)
        {
            if (!Application.isPlaying)
            {
                Debug.Log("PSYDEBUGEDITOR: " + String.Format(formatedString, msgs));
            }
            return;
        }

        instance.Out(type, context, LogType.Log, formatedString, msgs);
	}
	
	[System.Diagnostics.Conditional("PSY_ENABLED")]
	public static void Warning(Source type, string formatedString, params object[] msgs)
	{
        if (instance == null)
        {
            if (!Application.isPlaying)
            {
                Debug.Log("PSYDEBUGEDITOR: " + String.Format(formatedString, msgs));
            }
            return;
        }

        instance.Out(type, LogType.Warning, formatedString, msgs);
	}

	[System.Diagnostics.Conditional("PSY_ENABLED")]
	public static void Warning(Source type, PSYView context, string formatedString, params object[] msgs)
	{
        if (instance == null)
        {
            if (!Application.isPlaying)
            {
                Debug.Log("PSYDEBUGEDITOR: " + String.Format(formatedString, msgs));
            }
            return;
        }

        instance.Out(type, context, LogType.Warning, formatedString, msgs);
	}

	[System.Diagnostics.Conditional("PSY_ENABLED")]
	public static void Error(Source type, string formatedString, params object[] msgs)
	{
        if (instance == null)
        {
            if (!Application.isPlaying)
            {
                Debug.Log("PSYDEBUGEDITOR: " + String.Format(formatedString, msgs));
            }
            return;
        }

        instance.Out(type, LogType.Error, formatedString, msgs);
	}

	[System.Diagnostics.Conditional("PSY_ENABLED")]
    private void Out(Source type, LogType messageType, string formatedString, params object[] msgs)
    {
#if UNITY_EDITOR
        if (!debugCalls.ContainsKey((int)type))
        {
            debugCalls[(int)type] = 0;
        }

        debugCalls[(int)type]++;
#endif

        bool alwaysShowDebug = messageType == LogType.Warning? alwaysShowWarnings: (messageType == LogType.Error? alwaysShowErrors: false);

        if (!enabled && !alwaysShowDebug)
        {
            return;
        }

        Item item = null;
        bool itemExists = itemsDict.TryGetValue(type, out item);
        
        if ((!itemExists || !item.enabled) && !alwaysShowDebug)
        {
            return;
        }

        string res = GetStringFormat(formatedString, msgs);

        bool pass = !HasFilter;

        if (HasFilter)
        {
            for (int i = 0; i < filter.Length; i++)
            {
                if (filter[i] == "")
                {
                    continue;
                }
                if(res.Contains(filter[i]))
                {
                    pass = true;
                    break;
                }
            }
        }

        for (int i = 0; i < ignore.Length; i++)
        {
            if (ignore[i] != "" && res.Contains(ignore[i]))
            {
                pass = false;
                break;
            }
        }

        if (res.Contains(force))
        {
            pass = true;
        }

        if (!pass && !alwaysShowDebug)
        {
            return;
        }

        string outStr = "<color=" + mainColor.ToHex() + ">PSY" + type.ToString() + ": </color>" + "<color=" + item.color.ToHex() + ">" + res + "</color>";
        outStr = outStr.Replace(force, "");

		switch (messageType)
		{
			case LogType.Warning:
                Debug.LogWarning(outStr);
                break;

			case LogType.Error:
                Debug.LogError(outStr);
                break;

			case LogType.Log:
                Debug.Log(outStr);
                break;
		}
	}

    [System.Diagnostics.Conditional("PSY_ENABLED")]
	private void Out(Source type, PSYView context, LogType messageType, string formatedString, params object[] msgs)
	{
#if UNITY_EDITOR
        if (!debugCalls.ContainsKey((int)type))
        {
            debugCalls[(int)type] = 0;
        }

        debugCalls[(int)type]++;
#endif

        if (!enabled)
        {
            return;
        }

        Item item = null;
        bool itemExists = itemsDict.TryGetValue(type, out item);

        bool alwaysShowDebug = messageType == LogType.Warning? alwaysShowWarnings: (messageType == LogType.Error? alwaysShowErrors: false);
        if ((!itemExists || !item.enabled) && !alwaysShowDebug)
        {
            return;
        }

		string res = GetStringFormat(formatedString, msgs);

        bool pass = !HasFilter;

        if (HasFilter)
        {
            for (int i = 0; i < filter.Length; i++)
            {
                if (filter[i] == "")
                {
                    continue;
                }
                if (res.Contains(filter[i]) || context.ID.Contains(filter[i]))
                {
                    pass = true;
                    break;
                }
            }
        }

        for (int i = 0; i < ignore.Length; i++)
        {
            if (ignore[i] != "" && res.Contains(ignore[i]))
            {
                pass = false;
                break;
            }
        }

        for (int i = 0; i < ignore.Length; i++)
        {
            if (ignore[i] != "" && context.ID.Contains(ignore[i]))
            {
                pass = false;
                break;
            }
        }

        if (res.Contains(force) || context.ID.Contains(force))
        {
            pass = true;
        }

        if (!pass && !alwaysShowDebug)
        {
            return;
        }

        string outStr = "<color=" + mainColor.ToHex() + ">PSY" + type.ToString() + ": </color> <color=" + item.color.ToHex() + ">" + res + "</color> <color=" + contextColor.ToHex() + ">" + context.ID + "</color>";
        outStr = outStr.Replace(force, "");

		switch (messageType)
		{
			case LogType.Warning:
                Debug.LogWarning(outStr);
                break;

			case LogType.Error:
                Debug.LogError(outStr);
                break;

			case LogType.Log:
                Debug.Log(outStr);
                break;
		}
	}

	static string GetStringFormat(string s, object[] msgs)
	{
#if DISABLE_PSY
		return string.Empty;
#endif
		string res;
		try
		{
			switch (msgs.Length)
			{
				case 0:
					res = s;
					break;
				case 1:
					res = String.Format(s, msgs[0]);
					break;
				case 2:
					res = String.Format(s, msgs[0], msgs[1]);
					break;
				case 3:
					res = String.Format(s, msgs[0], msgs[1], msgs[2]);
					break;
				case 4:
					res = String.Format(s, msgs[0], msgs[1], msgs[2], msgs[3]);
					break;
				case 5:
					res = String.Format(s, msgs[0], msgs[1], msgs[2], msgs[3], msgs[4]);
					break;
				case 6:
					res = String.Format(s, msgs[0], msgs[1], msgs[2], msgs[3], msgs[4], msgs[5]);
					break;
				case 7:
					res = String.Format(s, msgs[0], msgs[1], msgs[2], msgs[3], msgs[4], msgs[5], msgs[6]);
					break;
				case 8:
					res = String.Format(s, msgs[0], msgs[1], msgs[2], msgs[3], msgs[4], msgs[5], msgs[6], msgs[7]);
					break;
				case 9:
					res = String.Format(s, msgs[0], msgs[1], msgs[2], msgs[3], msgs[4], msgs[5], msgs[6], msgs[7], msgs[8]);
					break;
				case 10:
					res = String.Format(s, msgs[0], msgs[1], msgs[2], msgs[3], msgs[4], msgs[5], msgs[6], msgs[7], msgs[8], msgs[9]);
					break;
				default:
					Debug.LogError(String.Format("Unsupported number of params: {0}", msgs.Length));
					res = "";
					break;
			}
		}
		catch (Exception)
		{
            res = "PSYDebug format exception";
            Error(Source.Core, res);
		}
		return res;
	}
}