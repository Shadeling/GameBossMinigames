using UnityEngine;
using System.Collections.Generic;
using System;
using DG.Tweening;
using XD;

public static class PSYGUI
{
    public static Vector2 ReferencedResolution
    {
        get
        {
            return _manager.referencedResolution;
        }

        set
        {
            _manager.referencedResolution = value;
        }
    }

    private static float resolutionScale = 1.0f;
    public static float ResolutionScale
    {
        get
        {
            return resolutionScale;
        }

        set
        {
            resolutionScale = value;
        }
    }

    public static Vector2 ReferencedScaledResolution
    {
        get
        {
            return _manager.referencedResolution * ResolutionScale;
        }        
    }

    public static PSYResolutionParams ResolutionParameters
    {
        get
        {
            return manager.ResolutionParameters;
        }
    }
    public static string texturesPath { get { return _manager.texturesPath; } }
	public static string WholeViewAnimationTarget = "Entire";
	public static float lastClickTime;
	
	public static float globalAnimationTime { get { return Time.timeScale * _manager.globalAnimationTime; } }
	public static float minimumDelayBetweenClicks { get { return _manager.minimumDelayBetweenClicks; } }	
	
	public static bool clickPermission
	{
		get	
		{
			bool may = Time.realtimeSinceStartup - lastClickTime > minimumDelayBetweenClicks;
            if (!may)
            {
                PSYDebug.Log(PSYDebug.Source.Core, "click aborted: global click interval restriction");
            }
			return may;
		}
	}
	
	private static PSYManager _manager;
	private static PSYManager manager
	{
		get
		{
			if (_manager == null)
			{
				PSYDebug.Error(PSYDebug.Source.Core, "manager is null");
				return null;
			}
			return _manager;
		}
		set { _manager = value; }
	}

	public static void SetManager(PSYManager _manager)
	{
		manager = _manager;
	}

    private static ARGUETools _agent;
    private static ARGUETools agent
    {
        get
        {
            if (_agent == null)
            {
                PSYDebug.Error(PSYDebug.Source.Core, "agent is null");
                return null;
            }
            return _agent;
        }
        set { _agent = value; }
    }

    public static void SetAgent(ARGUETools _agent)
    {
        agent = _agent;
    }

    // ::::::::::::::::::::::::::::::::::::::::::::::::::::::
    // ::::::::::::::::::::::::COMMON::::::::::::::::::::::::
    // ::::::::::::::::::::::::::::::::::::::::::::::::::::::

    public static bool BlockingAnimationExists
    {
        get
        {            
            List<DG.Tweening.Tween> list = DOTween.PlayingTweens();
            if (list != null)
                for (int i = 0; i < list.Count; i++)
                    if (list[i].id != null && ((string)list[i].id).Contains("_B"))
                    {
                        PSYDebug.Log(PSYDebug.Source.Input, "click aborted: there are incomplete animations with global block ({0})", list[i].id);
                        return true;
                    }
            return false;
        }
    }


    public static void Input(PSYKeys key, bool down = true)
	{
		if (manager != null)
			manager.ManagerInputHandler(key, down);
	}
    
    public static void Event(PSYEvent e, PSYParams param)
	{
		Event(e, param, 0f);
	}

	public static void Event(PSYEvent e)
	{
		Event(e, PSYParams.Empty);
	}

	public static void Event(PSYEvent e, PSYParams param, float delay)
	{
		if (manager != null)
			manager.Event(e, param, delay); 
	}

	private static int warnings = 0;
	// public static string Localize(object str)
	// {
 //        if (str == null) return "NULL";
 //        if (localizer == null)
	// 	{			
	// 		if (++warnings > 109)
	// 		{
	// 			PSYDebug.Warning(PSYDebug.Source.Core, "localizer is NULL");
	// 			warnings = 0;
	// 		}
	// 		return str.ToString();
	// 	}
	// 	return localizer.Localize(str.ToString());
	// }

	// public static string Localize(string str, object value = null)
	// {
	// 	if (str == null) return "NULL";
	// 	if (localizer == null)
	// 	{
	// 		if (++warnings > 109)
	// 		{
	// 			PSYDebug.Warning(PSYDebug.Source.Core, "localizer is NULL");
	// 			warnings = 0;
	// 		}
	// 		return String.Format(str, value);
 //        }
 //        return localizer.Localize(str, value);
	// }

	// public static string LocalizeDefault(object str)
	// {
	// 	if (str == null) return "NULL";
	// 	if (localizer == null)
	// 	{
	// 		if (++warnings > 109)
	// 		{
	// 			PSYDebug.Warning(PSYDebug.Source.Core, "localizer is NULL");
	// 			warnings = 0;
	// 		}
	// 		return str.ToString();
	// 	}
	// 	return localizer.LocalizeDefault(str.ToString());
	// }

	public static bool managerExists
    {
        get
        {
            return _manager != null;
        }
    }

	public static void ClearDebug()
	{
        if (manager != null)
        {
            manager.ClearScreenDebug();
        }
	}

	public static void Debug(string key, object value, bool force = false)
	{
        if (Application.isEditor || UnityEngine.Debug.isDebugBuild || force)
        {
            if (manager != null)
            {
                manager.ScreenDebug(key, value);
            }
        }
	}

    public static void ClearDebugKey(string key)
    {
        if (manager != null)
        {
            manager.ScreenDebugClearKey(key);
        }
    }

	public static PSYController GetPrefab(PSYItem type)
	{
		return manager.GetItemPrefab(type);
	}

	public static bool WindowExists(PSYWindow type)
	{
		return manager.WindowExists(type);
	}

	public static Vector3 GetWindowPosition(PSYWindow type)
	{
		return manager.GetWindowPosition(type);
	}

    public static int GetWindowSiblingIndex(PSYWindow wnd)
    {
        return manager.GetWindowSiblingIndex(wnd);
    }

    public static void SetWindowSiblingIndex(PSYWindow wnd, int ind)
    {
        manager.SetWindowSiblingIndex(wnd, ind);
    }

    /// <summary>
    /// Сортировка окон в соответствии со списком listWindow, 0 - самый нижний слой 
    /// </summary>
    /// <param name="listWindow"></param>
    public static void SortWindowSibling(List<PSYWindow> listWindow)
    {
	    manager.SortWindowSibling(listWindow);
    }
    
    public static Camera mainCamera { get { return manager.UICamera; } }

    public static PSYController GetWindowController(PSYWindow window)
    {
        if (manager != null)
            return manager.GetWindowController(window);
        return null;
    }

	public static PSYWindow GetWindow(PSYController controller)
	{
		if (manager != null)
			return manager.GetParentWindowType(controller);
		return default(PSYWindow);
	}	

	public static PSYController GetController(PSYItem type, int index = -1)
	{
		return manager.GetController(type, index);
	}


    public static void Show(PSYWindow wnd, PSYParams param)
	{
		if (manager != null)
			manager.Show(wnd, param);
	}

	public static void Show(params PSYWindow[] targets)
	{
		if (manager != null)
			manager.Show(targets);
	}

	public static void Hide(params PSYWindow[] targets)
	{
		if (manager != null)
			manager.Hide(targets);
	}

	public static void Kill(params PSYWindow[] targets)
	{
		if (manager != null)
			manager.Kill(targets);
	}
}