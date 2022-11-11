using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;
using UnityEngine.UI;
using XD;
using Zenject;

public abstract class PSYManager : PSYController
{
    private static List<IPSYEventHandler> subscribers = new List<IPSYEventHandler>();
    public static void AddSubscriber(IPSYEventHandler handler)
    {
        if (!subscribers.Contains(handler))
        {
            subscribers.Add(handler);
        }
    }
    public static void RemoveSubscriber(IPSYEventHandler handler)
    {
        if (subscribers.Contains(handler))
        {            
            subscribers.Remove(handler);
        }
    }

    public class PSYUpdateEvent : UnityEvent<PSYUpdateType> { }
	public static PSYUpdateEvent OnUpdateEvent;
	public class PSYUIEvent : UnityEvent<PSYViewEventArgs> { }
	public static PSYUIEvent OnUIEvent;
	public class PSYTouchEvent : UnityEvent<Touch[]> { }
	public static PSYTouchEvent OnTouchEvent;
	public class PSYMouseEvent : UnityEvent<List<PSYMouseInput>> { }
	public static PSYMouseEvent OnMouseEvent;
	public class PSYInputEvent : UnityEvent<PSYKeys>{}
	public static PSYInputEvent OnInputEvent;

    private static bool CheckInstance(bool subscribing)
	{
		if (instance == null)
		{
			if (subscribing)
				PSYDebug.Error(PSYDebug.Source.Core, "There is no any PSYManager instances in the scene");
			return false;
		}
		return true;
	}
	
	protected List<PSYDelayedEvent>         delayedEvents = new List<PSYDelayedEvent>();
	
	public string                           windowsPath = "";
	public string                           itemsPath = "";
	public string                           texturesPath = "";

    public Camera                           UICamera = null;

	/// <summary>
	/// Coefficient to slow down or speed up animations
	/// </summary>
	public float                            globalAnimationTime = 1.0f;
	public float                            minimumDelayBetweenClicks = 0.2f;

    public Vector2                          referencedResolution = new Vector2(1920, 1080);

    public List<PSYWindow>                  persistentWindows = new List<PSYWindow>();
	
	protected PSYWindowStack                windowStack = new PSYWindowStack();

    [SerializeField]
	protected Text                          debugLabel = null;

	protected Dictionary<string, string>    debugDict = new Dictionary<string, string>();

    [SerializeField]
    private List<PSYWindowMapper>           sourcesListWnd = null;
    private Dictionary<PSYWindow, string>   sourcesDictWnd = null;

    [SerializeField]
    private List<PSYItemMapper>             sourcesListItm = null;
    private Dictionary<PSYItem, string>     sourcesDictItm = null;

    protected int                           windowsCount;	
	private static PSYManager               instance = null;

	private bool                            mouse0;
	private bool                            mouse1;
	private Vector3                         mouseStart0;
	private Vector3                         mouseStart1;	

	private Dictionary<PSYKeys, bool>       ignoreInput = new Dictionary<PSYKeys, bool>();

    private PSYResolutionParams             resolutionParam = null;

    public PSYResolutionParams ResolutionParameters
    {
        get
        {
            return resolutionParam;
        }
    }

    public List<PSYWindowMapper> SourcesListWnd
    {
        get
        {
            return sourcesListWnd;
        }

        set
        {
            sourcesListWnd = value;
        }
    }

    public Dictionary<PSYWindow, string> SourcesDictWnd
    {
        get
        {
            if(sourcesDictWnd == null)
            {
                sourcesDictWnd = new Dictionary<PSYWindow, string>();

                for (int i = 0; i < SourcesListWnd.Count; i++)
                {
                    sourcesDictWnd[SourcesListWnd[i].Window] = SourcesListWnd[i].Path;
                }
            }

            return sourcesDictWnd;
        }

        set
        {
            sourcesDictWnd = value;
        }
    }

    public List<PSYItemMapper> SourcesListItm
    {
        get
        {
            return sourcesListItm;
        }

        set
        {
            sourcesListItm = value;
        }
    }

    public Dictionary<PSYItem, string> SourcesDictItm
    {
        get
        {
            if (sourcesDictItm == null)
            {
                sourcesDictItm = new Dictionary<PSYItem, string>();

                for (int i = 0; i < SourcesListItm.Count; i++)
                {
                    sourcesDictItm[SourcesListItm[i].Item] = SourcesListItm[i].Path;
                }
            }

            return sourcesDictItm;
        }

        set
        {
            sourcesDictItm = value;
        }
    }

    private void Awake()
	{
		if (instance != null && instance != this)
		{
			Destroy(gameObject);
			return;
		}

		instance = this;
		DontDestroyOnLoad(this);
		/*if (OnPSYEvent == null)
			OnPSYEvent = new PSYEventEvent();*/
		if (OnUIEvent == null)
			OnUIEvent = new PSYUIEvent();
		if (OnUpdateEvent == null)
			OnUpdateEvent = new PSYUpdateEvent();
		if (OnMouseEvent == null)
			OnMouseEvent = new PSYMouseEvent();
		if (OnTouchEvent == null)
			OnTouchEvent = new PSYTouchEvent();
		if (OnInputEvent == null)
			OnInputEvent = new PSYInputEvent();
		PSYGUI.SetManager(this);

		//Subscribe to update event
		OnUpdateEvent.AddListener(UpdateEventHandler);

		for (int i = 0; i < Enum.GetValues(typeof(PSYDebug.Source)).Length; i++)
			PSYDebug.debugCalls[i] = 0;
		
		foreach(var pair in PSYCommon.updateIntervals)
			StartCoroutine(UpdateCoroutine(PSYParams.New(pair.Key, pair.Value)));

        PSYDebug.GetInstance();

		ManagerAwake();
	}

	private IEnumerator UpdateCoroutine(PSYParams param)
	{
		while (1 == 1)
		{
			yield return new WaitForSeconds(param.Float);
			if (OnUpdateEvent != null)
				OnUpdateEvent.Invoke(param.Get<PSYUpdateType>());
		}
	}

 	protected virtual void ManagerAwake()
	{

	}

	protected override sealed void OnStart()
	{
        resolutionParam = new PSYResolutionParams(new Vector2(Screen.width, Screen.height), referencedResolution, GetComponent<CanvasScaler>());
        UICamera.orthographicSize *= resolutionParam.clampedX ? resolutionParam.matchWidthDifHeight : 1;
        view.Open();
		ManagerStart();        
}

	protected virtual void ManagerStart()
	{

	}

	/// Returns PSYController of item prefab by given controller type
	/// </summary>
	/// <param name="scene">Controller type</param>
	public PSYController GetItemPrefab(PSYItem type)
	{
        GameObject obj = (GameObject)Resources.Load(SourcesDictItm[type]);
        if (obj == null)
        {
            PSYDebug.Error(PSYDebug.Source.Manager, "item prefab with controller of type '{0}' not found", type);
            return null;
        }
        else
        {
            return obj.GetComponent<PSYController>();
        }
	}

    /// <summary>
    /// Returns PSYController of window's source of given type
    /// </summary>
    /// <param name="scene">Window type</param>
    protected PSYController GetWindowPrefab(PSYWindow type)
    {
        //PSYDebug.Log(PSYDebug.Source.Temp, "Loading {0}", SourcesDictWnd[type]);
        if (!SourcesDictWnd.ContainsKey(type))
        {
	        PSYDebug.Warning(PSYDebug.Source.Manager, "window '{0}' not found in dictionary", type);
	        return null;
        }

        GameObject obj = (GameObject)Resources.Load(SourcesDictWnd[type]);
        if (obj == null)
        {
            PSYDebug.Error(PSYDebug.Source.Manager, "window '{0}' not found in resources", type);
            return null;
        }
        else
        {
            return obj.GetComponent<PSYController>();
        }
    }

    public void ScreenDebugClearKey(string key)
    {
        debugDict.Remove(key);
        UpdateScreenDebug();
    }

	public void ScreenDebug(string key, object value)
	{
        //PSYDebug.Log(PSYDebug.Source.PlayMode, "screen debug: {0} {1}", key, value);

        if (debugDict.ContainsKey(key ?? ""))
        {
            debugDict[key] = value.ToString();
        }
        else
        {
            debugDict.Add(key, value.ToString());
        }

        UpdateScreenDebug();
	}

    private void UpdateScreenDebug()
    {
        if (debugLabel == null)
        {
            return;
        }

        StringBuilder strBuilder = new StringBuilder();
        foreach (var pair in debugDict)
            strBuilder.Append($"{pair.Key}: {pair.Value}\n");

        int strLength = strBuilder.Length;
        bool isActive = strLength > 0;
        if (isActive)   // Убираем последний '\n'.
            strBuilder.Remove(strLength - 1, 1);
        debugLabel.text = strBuilder.ToString();
        debugLabel.gameObject.SetActive(isActive);
    }

    public void ClearScreenDebug()
    {
        debugDict.Clear();
        debugLabel.text = "";
    }

    protected virtual void EnableOrShow(PSYWindow wnd, PSYParams param)
    {
        if (WindowExists(wnd))
            windowStack[wnd].view.Enable();
        else
            Show(wnd, param);
    }

    protected virtual void EnableOrShow(PSYWindow wnd)
    {
        if (WindowExists(wnd))
            windowStack[wnd].view.Enable();
        else
            Show(wnd);
    }

    public void Show(params PSYWindow[] targets)
	{
		for (int i = 0; i < targets.Length; i++)
			if (!WindowExists(targets[i]))
				Register(targets[i]);
	}

    public void ShowOrUpdate(PSYWindow wnd, PSYParams param)
    {
	    if (WindowExists(wnd))
	    {
		    windowStack[wnd].Setup(param);
		    if (!windowStack[wnd].view.enabled)
		    {
			    windowStack[wnd].view.Enable();
		    }
		    return;
	    }

	    Register(wnd).Setup(param);
    }
    
    public bool Show(PSYWindow wnd, PSYParams param)
	{
        if (WindowExists(wnd))
        {
            return false;
        }

        Register(wnd).Setup(param);
        return true;
	}

    public void Show(PSYParams param, params PSYWindow[] targets)
    {
	    for (int i = 0; i < targets.Length; i++)
	    {
		    Show(targets[i], param);
	    }
    }
    
	private PSYController Register(PSYWindow type)
	{		
		PSYController source = GetWindowPrefab(type);		
		if (source == null) return null;
		OnWindowOpening(type);
		source = AddChild(source);
        //source.view.Open();
        windowStack.Add(source);
	    PSYDebug.Log(PSYDebug.Source.Registration, "reg: {0}, stack: {1}", source.view.window, windowStack.Count);
		OnWindowOpened(type);
		PSYGUI.Event(PSYEvent.WindowOpened, PSYParams.New(type, source));
		return source;
	}

	private void Unregister(PSYWindow type)
	{
		windowStack.Remove(type);
		PSYDebug.Log(PSYDebug.Source.Registration, "unreg: {0}, stack: {1}", type, windowStack.Count);
	}

	public void Hide(params PSYWindow[] targets)
	{
		HideOrKill(true, targets);
	}

	public void Kill(params PSYWindow[] targets)
	{
		HideOrKill(false, targets);
	}

	private void HideOrKill(bool hide, params PSYWindow[] targets)
	{		
		PSYController target = null;
		for (int i = 0; i < targets.Length; i++)
			if (windowStack.Contains(targets[i]))
			{
				PSYDebug.Log(PSYDebug.Source.Manager, "hiding {0}", windowStack[targets[i]].view.window);				
				target = windowStack[targets[i]];
				OnWindowClosing(targets[i]);
				Unregister(target.view.window);
				target.view.Perform(hide ? PSYViewEvent.Close : PSYViewEvent.Kill);
				OnWindowClosed(targets[i]);                
                PSYGUI.Event(PSYEvent.WindowClosed, PSYParams.New(targets[i], target));
			}
	}

	protected virtual void OnWindowOpening(PSYWindow window) { }	
	protected virtual void OnWindowClosing(PSYWindow window) { }
    protected virtual void OnWindowAnimated(PSYWindow window, PSYAnimationEvent e) { }

    protected override void OnViewAnimationCompleted(PSYController cont, PSYAnimationEvent e)
    {
        if (cont.view.type == PSYViewType.Window)
            OnWindowAnimated(cont.view.window, e);
    }

    protected void KillAllBut(params PSYWindow[] exceptions)
	{
		List<PSYWindow> toClose = new List<PSYWindow>();
		foreach(var pair in windowStack)
			if (!exceptions.Contains(pair.Value.view.window))
				toClose.Add(pair.Value.view.window);
		Kill(toClose.ToArray<PSYWindow>());
	}

	protected void HideAllBut(params PSYWindow[] exceptions)
	{
		List<PSYWindow> toClose = new List<PSYWindow>();
		foreach (var pair in windowStack)
			if (!exceptions.Contains(pair.Value.view.window) && !persistentWindows.Contains(pair.Value.view.window))
				toClose.Add(pair.Value.view.window);
		Hide(toClose.ToArray<PSYWindow>());
	}

    protected void HidePersistent()
    {
        List<PSYWindow> toClose = new List<PSYWindow>();
        foreach (var pair in windowStack)
            if (persistentWindows.Contains(pair.Value.view.window))
                toClose.Add(pair.Value.view.window);
        Hide(toClose.ToArray<PSYWindow>());
    }

	/// <summary>
	/// Hides all windows except given targets, that will be shown instead
	/// </summary>
	/// <param name="targets"></param>
	protected void HideAllButShow(params PSYWindow[] targets)
	{        
        List<PSYWindow> toClose = new List<PSYWindow>();
		foreach (var pair in windowStack)
			for (int i = windowStack.Count - 1; i >= 0; i--)
				if (!targets.Contains(pair.Value.view.window) && !persistentWindows.Contains(pair.Value.view.window))
					toClose.Add(pair.Value.view.window);
		Hide(toClose.ToArray<PSYWindow>());
		for (int i = 0; i < targets.Length; i++)
			Show(targets[i]);
	}

    public PSYController GetWindowController(PSYWindow window)
    {
        return windowStack[window];
    }

	/// <summary>
	/// Gets the parent window type of target controller
	/// </summary>
	/// <param name="controller">Target controller</param>
	public PSYWindow GetParentWindowType(PSYController controller)
	{
		if (controller.view.type == PSYViewType.Window) return controller.view.window;//controller is window itself
		Transform t = controller.transform.parent;
        if(t == null)
        {
            return PSYWindow.None;
        }
		while (t.GetComponent<PSYController>() == null)
		{
			t = t.parent;
            if (t == null)
            {
                return PSYWindow.None;
            }
        }
		return GetParentWindowType(t.GetComponent<PSYController>());
	}	

	public bool WindowExists(PSYWindow type)
	{
		return windowStack.Contains(type);
	}

	public Vector3 GetWindowPosition(PSYWindow type)
	{
		if (!windowStack.Contains(type)) return Vector3.zero;
		return windowStack[type].transform.localPosition;
	}

	/// <summary>
	/// Сортировка окон в соответствии со списком listWindow, 0 - самый нижний слой
	/// </summary>
	/// <param name="listWindow"></param>
	public void SortWindowSibling(List<PSYWindow> listWindow)
	{
		SortedDictionary<int, PSYWindow> windowSibling = GetWindowsSibling(listWindow);

		if (windowSibling.Keys.Count < 2)
		{
			return;
		}

		int indexStart= windowSibling.Keys.Min();
		foreach (PSYWindow windowValue in listWindow)
		{
			if (!windowSibling.Values.Contains(windowValue))
			{
				continue;
			}
			SetWindowSiblingIndex( windowValue , indexStart);
			indexStart++;
		}
	}
	
	public SortedDictionary<int, PSYWindow> GetWindowsSibling( List<PSYWindow> listWindow = null)
	{
		SortedDictionary<int, PSYWindow> dicRes = new SortedDictionary<int, PSYWindow>();
		foreach( PSYWindow typeWindow in windowStack.Keys )
		{

			if ((listWindow != null) && (!listWindow.Contains(typeWindow)))
			{
				continue;
			}
			
			dicRes[GetWindowSiblingIndex(typeWindow)] = typeWindow;
		}

		return dicRes;
	}
	
    public int GetWindowSiblingIndex(PSYWindow wnd)
    {
        if (!windowStack.Contains(wnd)) return -1;
        return windowStack[wnd].transform.GetSiblingIndex();
    }

    public void SetWindowSiblingIndex(PSYWindow wnd, int ind)
    {
        if (!windowStack.Contains(wnd))
        {
            return;
        }

        windowStack[wnd].transform.SetSiblingIndex(ind);
    }

    List<PSYMouseInput> input = new List<PSYMouseInput>();
	protected virtual void Update()
	{		
		if (Application.platform != RuntimePlatform.IPhonePlayer)
		{
			if(!mouse0 && Input.GetMouseButton(0))
				mouseStart0 = Input.mousePosition;			
			mouse0 = Input.GetMouseButton(0);
			if (mouse0)
				input.Add(new PSYMouseInput(0, mouseStart0, Input.mousePosition));

			if (!mouse1 && Input.GetMouseButton(1))
				mouseStart1 = Input.mousePosition;
			mouse1 = Input.GetMouseButton(1);
			if (mouse1)
				input.Add(new PSYMouseInput(1, mouseStart1, Input.mousePosition));

            if (input.Count > 0 && OnMouseEvent != null)
            {
                OnMouseEvent.Invoke(input);
                input.Clear();
            }
		}

		if (Input.touchCount > 0 && OnTouchEvent != null)
			OnTouchEvent.Invoke(Input.touches);

		for (int i = delayedEvents.Count - 1; i >= 0; i--)
		{			
			delayedEvents[i].timeLeft -= Time.deltaTime;
			if (delayedEvents[i].timeLeft <= 0)
			{
				PSYDelayedEvent e = delayedEvents[i];
				Event(e.e, e.param.AddByID("delay", e.delay));
				delayedEvents.Remove(delayedEvents[i]);
			}
		}
	}
	
	protected override void ChildEventMethod(PSYViewEventArgs e)
	{
		base.ChildEventMethod(e);
		if (OnUIEvent != null)
			OnUIEvent.Invoke(e);
	}

	protected override void OwnEventMethod(PSYViewEventArgs e)
	{
		base.OwnEventMethod(e);
		if (OnUIEvent != null)
			OnUIEvent.Invoke(e);
	}	

	protected PSYController FindController(PSYItem type, int index = -1)
	{
		PSYController target = null;
		foreach (var pair in windowStack)
		{
			if (pair.Value.view.item == type)
				target = pair.Value;
			else
				target = pair.Value.GetChild(type, index);
			if (target == null) continue;
			return target;
		}
		return null;
	}

	public PSYController GetController(PSYItem type, int index = -1)
	{		
		List<PSYController> conts;
		foreach(var pair in windowStack)		
		{
			conts = pair.Value.FilterChildren(type, index);
			if (conts.Count == 0) continue;
			return conts[0];
		}
		PSYDebug.Log(PSYDebug.Source.Manager, "no controller found {0} {1}", type, index);
		return null;
	}

    public void ManagerInputHandler(PSYKeys key, bool down)
	{
		if (!down)//Кнопка отпущена, меняем состояние на false, реагируем на следующее нажатие
		{
			ignoreInput[key] = false;
			return;
		}

		if (key == PSYKeys.Center)//Джойстик пришел в 0, реагируем на следующее изменение
		{
			ignoreInput[PSYKeys.Up] = false;
			ignoreInput[PSYKeys.Down] = false;
			ignoreInput[PSYKeys.Left] = false;
			ignoreInput[PSYKeys.Right] = false;
			return;
		}

		//Если нажатие уже зафиксировано, игнорируем эту клавишу до прихода false
		if (ignoreInput.ContainsKey(key) && ignoreInput[key]) return;

		if (OnInputEvent != null)
			OnInputEvent.Invoke(key);
		ignoreInput[key] = true;
	}

	public void Event(PSYEvent eventName, PSYParams param, float delay = 0.0f)
	{
		if (delay != 0)
		{
			PSYDebug.Log(PSYDebug.Source.Events, "delayed <color=yellow>{0}</color> {1} {2}", eventName, param, delay);
			delayedEvents.Add(new PSYDelayedEvent(eventName, param, delay));
			return;
		}

        switch(eventName)
        {

            default:
                PSYDebug.Log(PSYDebug.Source.Events, "<color=yellow>{0}</color> {1} {2}", eventName, param, param.ContainsByID("delay") ? "was delayed for " + param.GetByID<float>("delay").ToString() : "");
                break;
        }
		
		PSYEventHandler(eventName, param);        

        //While firing event, some subscribers may unsubscribe, messing with list indexes. So we store receivers in a separate list.
        List<IPSYEventHandler> receivers = new List<IPSYEventHandler>(subscribers);
        for (int i = 0; i < receivers.Count; i++)
        {
            receivers[i].PSYEventHandler(eventName, param);
        }

        PSYEventPostHandler(eventName, param);
    }

    protected virtual void PSYEventPostHandler(PSYEvent e, PSYParams param)
    {

    }

    protected override void OnDestroy()
	{
		base.OnDestroy();
		if (instance == this)
			instance = null;
	}
}