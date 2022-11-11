using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using XD;
using Zenject;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PSYView))]
[Serializable]
public class PSYController : MonoBehaviour, IPSYEventHandler
{
    private bool updateEnabled = false;
	private bool receiveChildEvents = false;    

	private Transform _transform;
    private bool hasTransform;
    public new Transform transform
    {
        get
        {
            if (!hasTransform)
            {
                _transform = gameObject.transform;
                hasTransform = true;
            }

            return _transform;
        }
    }

    public RectTransform Rect
    {
        get
        {            
            return view.Rect;
        }
    }

	/// <summary>
	/// Number of controller's children
	/// </summary>
	public int count { get { return children.Count; } }

    /// <summary>
    /// List of all child controllers
    /// </summary>
	private List<PSYController> children = new List<PSYController>();	

	public override string ToString()
	{		
		return name;
	}
	
	private PSYView _view = null;
    private bool hasView;
    public PSYView view
    {
        get
        {
            if (!hasView)
            {
                _view = GetComponent<PSYView>();
                hasView = true;
            }

            return _view;
        }
    }

    public List<PSYController> Children
    {
        get
        {
            return children;
        }

        set
        {
            children = value;
        }
    }

    public List<PSYViewEvent> ignoreEvents = new List<PSYViewEvent>();

    [SerializeField]
    [HideInInspector]
    private PSYController parent = null;

    public string ID
    {
        get;
        set;
    }

    public PSYController Parent
    {
        get
        {
            return parent;
        }

        set
        {
            parent = value;
        }
    }
    protected bool started = false;
    public bool IsStarted { get { return started; } }
	protected bool setup = false;
	private PSYParams setupParams = null;

    [Inject] private SceneContextRegistry m_sceneContextRegistry;

    protected DiContainer Container {
        get {
            var scene = SceneManager.GetActiveScene();
            SceneContext sceneContext = m_sceneContextRegistry.TryGetSceneContextForScene(scene);
            if (sceneContext != null)
            {
                return sceneContext.Container;
            }
            return ProjectContext.Instance.Container;
        }
    }

    private Dictionary<float, bool> hoverTimes = null;
    private Dictionary<float, bool> HoverTimes
    {
        get
        {
            if(hoverTimes == null)
            {
                hoverTimes = new Dictionary<float, bool>();
            }

            return hoverTimes;
        }
    }

	private void ResetHoverTimes()
	{
        if(ignoreEvents.Contains(PSYViewEvent.MouseHover))
        {
            return;
        }

		List<float> hoverTimesList = new List<float>();
        foreach (var pair in HoverTimes)
        {
            hoverTimesList.Add(pair.Key);
        }

        for (int i = 0; i < hoverTimesList.Count; i++)
        {
            HoverTimes[hoverTimesList[i]] = true;
        }
	}

	private void Awake()
	{
        //Items and windows in the resource folder aren't parented yet. Parent them now.                
        if (Parent == null)
        {
            Parent = PSYTools.GetParentObject<PSYController>(transform);
        }

        Parent.RegisterChild(this);

        OnAwake();

        ignoreEvents.Add(PSYViewEvent.MouseHover);
        ignoreEvents.Add(PSYViewEvent.MouseIn);
        ignoreEvents.Add(PSYViewEvent.MouseOut);
	}

	private void Start()
	{
        if (view.type == PSYViewType.Window)
        {
            EnableEvents(true);
        }
        
        //All controllers will receive own view's events
        EnableViewEvents();

        //All controllers will receive child events
        EnableChildEvents();

        OnStart();

		//We don't want to call Setup method before Awake, because in Awake we registering children that may be used 
		//in OnSetup overriden method. So if Setup was called, we delay Setup until script is started. After Start any Setup call will be performed immediately
		started = true;
        if (setupParams != null)
        {
            try
            {
                Setup(setupParams);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat(this, "{0}: some exception on Setup: {1}!\n{2}", name, e.Message, e.StackTrace);
            }

            setupParams = null;
        }

        try
        {
            Init();
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat(this, "{0}: some exception on Init: {1}!\n{2}", name, e.Message, e.StackTrace);        
        }

		OnResolutionChange(new PSYResolutionParams(new Vector2(Screen.width, Screen.height), PSYGUI.ReferencedResolution));

        if (view.type != PSYViewType.Window && view.puppet && parent != null && parent.view.isOpened && !view.isOpened)
        {
            view.Open();
        }
        else if (view.type == PSYViewType.Window)
        {
            view.Open();
        }

        if (view.type == PSYViewType.Window)
        {
            PSYGUI.Event(PSYEvent.WindowInited, view.window.ToParam());
        }
	}


	/// <summary>
	/// Will be called every time parent window is shown
	/// </summary>
	protected virtual void Init()
	{

	}

    protected virtual void InitSteam()
    {

    }
     
    protected virtual void InitNonSteam()
    {

    }

    public virtual void Set(PSYParams param)
    {
        
    }

    public virtual PSYParams ParamSetup()
    {
	    return null;
    }

    
    public virtual void Setup(PSYParams param)
	{		
		if (!started)
		{
			PSYDebug.Log(PSYDebug.Source.Setup, view, "early setup: {0}", param); 
			if (setupParams == null) setupParams = PSYParams.New();
			setupParams.Merge(param);
			return;
		}
		
		PSYDebug.Log(PSYDebug.Source.Setup, view, "setup: {0}", param); 
		OnSetup(param);
		setup = true;
	}	

	protected virtual void OnDestroy()
	{
        //If view is disabled
        if (parent != null)
        {
            parent.UnregisterChild(this);
        }

		EnableInput(false);
		EnableMouse(false);
		EnableTouch(false);
		EnableUpdate(false);
		EnableEvents(false);
		EnableGUIEvents(false);
		EnableChildEvents(false);		
	}

    public string Description
    {
        get
        {
            return view.FullName;
        }
    }
	
	/// <summary>
	/// Registering new child object
	/// </summary>	
	protected void RegisterChild(PSYController child, bool onAwake = false)
	{
        children.Add(child);
        child.Parent = this;
        PSYDebug.Log(PSYDebug.Source.Registration, view, "controller registered {0} (child N{1})", child.name, children.Count - 1);
    }

	/// <summary>
	/// Unregistering new child object
	/// </summary>	
	protected void UnregisterChild(PSYController child)
	{
		if (!children.Contains(child))
		{
			//PSYDebug.Log(PSYDebug.Source.Registration, view, "you are trying to unregister controller {0}, but it isn't registered as a child", child.name);
			return;
		}
        children.Remove(child);
        //PSYDebug.Log(PSYDebug.Source.Registration, view, "controller unregistered: {0} (children left: {1})", child.name, children.Count);
    }

	/// <summary>
	/// Instantiates given controller to target transform and registers it as a child
	/// </summary>
	/// <param name="type">Source item type</param>
	/// <param name="parent">Transform to parent item to</param>
	/// <param name="name">Custom name for new item</param>
	public virtual PSYController AddChild(PSYController controller, Transform parent = null, string name = "")
	{
		if (controller == null) return null;
        //PSYDebug.Log(PSYDebug.Source.Controller, view, "instantiating new child {0}", controller);
		PSYController child = (Container.InstantiatePrefab(controller.gameObject, parent == null ? transform : parent)).GetComponent<PSYController>();
        child.name = name == "" ? child.name : name;
		return child;
	}

    /// <summary>
    /// Adopt child
    /// </summary>
    public virtual void AdoptChild(PSYController child)
    {
        if (child == null)
        {
            return;
        }

        child.parent.UnregisterChild(child);
        //child.name = name == "" ? child.name : name;
        RegisterChild(child);
        return;
    }

	/// <summary>
	/// Instantiates given source and parents it to target transform
	/// </summary>
	/// <param name="type">Source item type</param>
	/// <param name="parent">Transform to parent item to</param>
	/// <param name="name">Custom name for new item</param>
	public virtual PSYController AddChild(PSYItem type, Transform parent = null, string name = "")
	{
		//PSYDebug.Log(PSYDebug.Source.Controller, view, "instantiating new child {0}", type);
		PSYController child = Container.InstantiatePrefab(PSYGUI.GetPrefab(type).gameObject, parent == null ? transform : parent).GetComponent<PSYController>();
        child.name = name == "" ? child.name : name;        
		return child;
	}

    public void AddChildren(int count, PSYItem type, Action<PSYController> doWithChild, Transform parent = null)
    {
        StartCoroutine (AddChildrenCoroutine (count, type, doWithChild, parent));
    }

    private IEnumerator AddChildrenCoroutine(int count, PSYItem type, Action<PSYController> doWithChild, Transform parent = null)
    {
        int cur = 0;
        PSYController child = AddChild(type, parent);
        if(doWithChild!=null)
        {
            doWithChild(child);
        }
        yield return new WaitForSeconds(0.001f);
        if(++cur == count)
        {
            yield break;
        }
    }

	/// <summary>
	/// Searches for child controller with given item type and index
	/// </summary>
	public PSYController GetChild(PSYItem type, int index = -1)
	{
        for (int i = 0; i < children.Count; i++)
        {            
            if (children[i].view.item == type && (index == -1 || children[i].view.index == index))
            {
                return children[i];
            }
        }

		for (int i = 0; i < children.Count; i++)
		{
			PSYController found = children[i].GetChild(type, index);
			if (found != null) return found;
		}
		return null;
	}

	/// <summary>
	/// Searches for child controller with given view index
	/// </summary>
	public PSYController GetChild(int index)
	{
		for (int i = 0; i < children.Count; i++)
			if (children[i].view.index == index)
				return children[i];
		for (int i = 0; i < children.Count; i++)
		{
			PSYController found = children[i].GetChild(index);
			if (found != null) return found;
		}
		return null;
	}

    /*public void SetPosition(Vector3 pos, bool initial = false)
    {
        Vector3 result = pos;
        if (initial)
        {
            RectTransform parent = transform.parent.GetComponent<RectTransform>();
            result += new Vector3((0.5f - parent.pivot.x) * parent.sizeDelta.x, (0.5f - parent.pivot.y) * parent.sizeDelta.y, 0);
        }
        transform.localPosition = result;
    }*/
    
    public void SetPosition(Vector3 pos, bool initial = false)
    {
        Vector3 result = pos;
        RectTransform rect = transform.GetComponent<RectTransform>();
        RectTransform rectParent = transform.parent.GetComponent<RectTransform>();
        if (!initial)
        {
            rect.anchoredPosition3D = result;
            return;
        }

        if (rect.anchorMin.x == rect.anchorMax.x)
        {
            result -= new Vector3((0.5f - rect.anchorMin.x) * rectParent.sizeDelta.x, 0, 0);
        }

        if (rect.anchorMin.y == rect.anchorMax.y)
        {
            result -= new Vector3(0, (0.5f - rect.anchorMin.y) * rectParent.sizeDelta.y, 0);
            //PSYDebug.Log(PSYDebug.Source.Temp, "result {0}", (0.5f - rect.anchorMin.y) * rectParent.sizeDelta.y);
        }

        rect.anchoredPosition3D = result;
    }

    protected bool ViewIsOwn(PSYView view)
	{
		return view == this.view;
	}

	/// <summary>
	/// Handler of event from this object's view or from child view
	/// </summary>	
	private void OnViewEvent(PSYViewEventArgs e)
	{        
        //PSYDebug.Log(PSYDebug.Source.ViewEvent, "OnViewEvent Sender {0} Receiver {1} {2}", e.sender, view, e.viewEvent);
        if (!ViewIsOwn(e.sender))
			ChildEventMethod(e);
		else
			OwnEventMethod(e);
        if (view.type != PSYViewType.Root && (parent.view.type != PSYViewType.Root || e.viewEvent == PSYViewEvent.Click) && parent.receiveChildEvents)
            parent.OnViewEvent(e);
	}

	private void OnViewAnimationCompletedHandler(PSYController cont, PSYAnimationEvent e)
	{
		OnViewAnimationCompleted(cont, e);
		if (view.type != PSYViewType.Root && parent.receiveChildEvents)
			parent.OnViewAnimationCompletedHandler(cont, e);
	}

	protected virtual void OwnEventMethod(PSYViewEventArgs e)
	{
		//PSYDebug.Log(PSYDebug.Source.ViewEvent, view, "OwnEventMethod {0}", e.viewEvent);
		switch (e.viewEvent)
		{
			case PSYViewEvent.Open: OnViewOpen(); break;
			case PSYViewEvent.Close: OnViewClose(); break;
			case PSYViewEvent.Select: OnViewSelect(e); break;
			case PSYViewEvent.Deselect: OnViewDeselect(); break;
			case PSYViewEvent.Enable: OnViewActivate(); break;
			case PSYViewEvent.Disable: OnViewDeactivate(); break;
			case PSYViewEvent.Hold: OnViewHold(); break;
			case PSYViewEvent.Click: OnViewClick(e); break;
			case PSYViewEvent.MouseUp: OnViewMouseUp(); break;
			case PSYViewEvent.MouseIn: OnViewMouseIn(); break;
			case PSYViewEvent.MouseOut: OnViewMouseOut(); ResetHoverTimes(); break;
			case PSYViewEvent.MouseHover:
				foreach (var pair in HoverTimes)
					if (e.time >= pair.Key && pair.Value)
					{
						HoverTimes[pair.Key] = false;
						OnViewHover(pair.Key);
						break;
					}
				break;
			case PSYViewEvent.MouseDown: OnViewMouseDown(e); break;
			case PSYViewEvent.Kill:
				parent.UnregisterChild(this);
				OnViewDestroy();
				break;
		}
	}

    protected virtual void ChildEventMethod(PSYViewEventArgs e)
    {
        //PSYDebug.Log(PSYDebug.Source.ViewEvent, view, "ChildEventMethod {0}", e.viewEvent);
        switch (e.viewEvent)
        {
            case PSYViewEvent.Open:
                OnChildOpen(e);
                break;
            case PSYViewEvent.Close:
                OnChildClose(e);
                break;
            case PSYViewEvent.Select:
                OnChildSelect(e, true);
                break;
            case PSYViewEvent.Deselect:
                OnChildSelect(e, false);
                OnChildDeselect(e);
                break;
            case PSYViewEvent.Enable:
                OnChildActivate(e);
                break;
            case PSYViewEvent.Disable:
                OnChildDeactivate(e);
                break;
            case PSYViewEvent.Hold:
                OnChildMouseHold(e);
                break;
            case PSYViewEvent.Click:
                /*XD.StaticContainer.UI.AddDebug(string.Format("{0}:{1}->{2}", name, e.controller.name, "click"));
                PSYDebug.Log(PSYDebug.Source.Temp, string.Format("{0}:{1}->{2}", name, e.controller.name, "click"));*/
                OnChildClick(e);
                break;
            case PSYViewEvent.MouseUp:
                /*XD.StaticContainer.UI.AddDebug(string.Format("{0}:{1}->{2}", name, e.controller.name, "mouseUp"));
                PSYDebug.Log(PSYDebug.Source.Temp, string.Format("{0}:{1}->{2}", name, e.controller.name, "mouseUp"));*/
                OnChildMouseUp(e);
                break;
            case PSYViewEvent.MouseIn:
                OnChildMouseIn(e);
                break;
            case PSYViewEvent.MouseOut:
                OnChildMouseOut(e);
                ResetHoverTimes();
                break;
            case PSYViewEvent.MouseHover:
                foreach (var pair in HoverTimes)
                    if (e.time >= pair.Key && pair.Value)
                    {
                        HoverTimes[pair.Key] = false;
                        OnChildMouseHover(e);
                        break;
                    }
                break;
            case PSYViewEvent.MouseDown:
                /*XD.StaticContainer.UI.AddDebug(string.Format("{0}:{1}->{2}", name, e.controller.name, "mouseDown"));
                PSYDebug.Log(PSYDebug.Source.Temp, string.Format("{0}:{1}->{2}", name, e.controller.name, "mouseDown"));*/
                OnChildMouseDown(e);
                break;
            case PSYViewEvent.Kill:
                UnregisterChild(e.controller);
                OnChildDestroy(e);
                break;
        }
    }

	public bool IsParentOf(PSYController controller, bool includeNested = true)
	{
		if (controller == this) return false;
		for (int i = 0; i < children.Count; i++)
			if (children[i].Equals(controller)) return true;
		if (!includeNested) return false;
		for (int i = 0; i < children.Count; i++)
			if(children[i].IsParentOf(controller))
				return true;
		return false;
	}

    public List<PSYController> GetChildren()
    {
        List<PSYController> res = new List<PSYController>();
        res.AddRange(children);
        for (int i = 0; i < children.Count; i++)
        {
            res.AddRange(children[i].GetChildren());
        }

        return res;
    }
	
	public List<PSYController> FilterChildren(PSYItem type, int index = -1, bool includeNested = true)
	{
        List<PSYController> conts = GetChildren();
        for (int i = conts.Count - 1; i >= 0; i--)
		{
            if (conts[i].view.item != type && type != PSYItem.None)
				conts.RemoveAt(i);
		}
		if (index != -1)
			for (int i = conts.Count - 1; i >= 0; i--)
				if (conts[i].view.index != index)
					conts.RemoveAt(i);
		return conts;
	}

	protected void EnableMouse(bool enable = true)
	{		
		if (enable)
			PSYManager.OnMouseEvent.AddListener(MouseEventHandler);
		else
			PSYManager.OnMouseEvent?.RemoveListener(MouseEventHandler);
	}

	protected void EnableTouch(bool enable = true)
	{
		if (enable)
			PSYManager.OnTouchEvent.AddListener(TouchEventHandler);
		else
			PSYManager.OnTouchEvent?.RemoveListener(TouchEventHandler);
	}

    protected void EnableUpdate(bool enable = true, bool toggle = false)
	{
        if (toggle)
        {
            updateEnabled = !updateEnabled;
        }
        else
        {
            if(enable == updateEnabled)
            {
                return;
            }

            updateEnabled = enable;
        }

        if (updateEnabled)
			PSYManager.OnUpdateEvent.AddListener(UpdateEventHandler);
		else
			PSYManager.OnUpdateEvent?.RemoveListener(UpdateEventHandler);
	}	

	protected void EnableGUIEvents(bool enable = true)
	{		
		if (enable)
			PSYManager.OnUIEvent.AddListener(UIEventHandler);
		else
			PSYManager.OnUIEvent?.RemoveListener(UIEventHandler);
	}

	public void EnableViewEvents(bool enable = true)
	{        
		if (enable)
		{
			view.OnState.AddListener(OnViewEvent);
			view.OnAnimationCompleted.AddListener(OnViewAnimationCompletedHandler);
		}
		else
		{
			view.OnState?.RemoveListener(OnViewEvent);
			view.OnAnimationCompleted?.RemoveListener(OnViewAnimationCompletedHandler);
		}
	}

	public void EnableChildEvents(bool enable = true)
	{
		receiveChildEvents = enable;		
	}

    public void EnableEvents(bool enable = true)
	{
        if (enable)
            PSYManager.AddSubscriber(this);
        else
            PSYManager.RemoveSubscriber(this);
	}

	protected void EnableInput(bool enable = true)
	{
		if (enable)
			PSYManager.OnInputEvent.AddListener(InputHandler);
		else
			PSYManager.OnInputEvent?.RemoveListener(InputHandler);
	}

	public virtual void PSYEventHandler(PSYEvent e, PSYParams param)
	{
		switch (e)
		{
			case PSYEvent.ResolutionChanged: OnResolutionChange(param.Get<PSYResolutionParams>()); break;
			case PSYEvent.WindowOpened: OnWindowOpened(param.Get<PSYWindow>()); break;
			case PSYEvent.WindowClosed: OnWindowClosed(param.Get<PSYWindow>()); break;
		}		
	}

	/// <summary>
	/// Do not refer to outside PSY-components in OnAwake() because some connections and relationships may not yet be established. Use OnStart() for that purpose instead.
	/// </summary>
	protected virtual void OnAwake() { }
	protected virtual void OnSetup(PSYParams param) { }
	protected virtual void OnStart() { }
	protected virtual void OnViewHover(float time) { }
	protected virtual void OnViewOpen() { }
	protected virtual void OnViewClose() { }
	protected virtual void OnViewSelect(PSYViewEventArgs e) { }
    protected virtual void OnViewDeselect() { }
	protected virtual void OnViewActivate() { }
	protected virtual void OnViewDeactivate() { }
	protected virtual void OnViewMouseDown(PSYViewEventArgs e) { }
	protected virtual void OnViewHold() { }
	protected virtual void OnViewMouseUp() { }
	protected virtual void OnViewClick(PSYViewEventArgs e) { }
    protected virtual void OnViewDestroy() { }
	protected virtual void OnViewMouseIn() { }
	protected virtual void OnViewMouseOut() { }

	protected virtual void OnChildOpen(PSYViewEventArgs e) { }
	protected virtual void OnChildClose(PSYViewEventArgs e) { }
	protected virtual void OnChildSelect(PSYViewEventArgs e, bool selected) { }
	protected virtual void OnChildDeselect(PSYViewEventArgs e) { }
	protected virtual void OnChildActivate(PSYViewEventArgs e) { }
	protected virtual void OnChildDeactivate(PSYViewEventArgs e) { }
    protected virtual void OnChildMouseIn(PSYViewEventArgs e) { }
    protected virtual void OnChildMouseOut(PSYViewEventArgs e) { }
	protected virtual void OnChildMouseDown(PSYViewEventArgs e) { }
    protected virtual void OnChildMouseUp(PSYViewEventArgs e) { }
    protected virtual void OnChildMouseHover(PSYViewEventArgs e) { }
	protected virtual void OnChildMouseHold(PSYViewEventArgs e) { }
	protected virtual void OnChildClick(PSYViewEventArgs e) { }
	protected virtual void OnChildDestroy(PSYViewEventArgs e) { }

	protected virtual void InputHandler(PSYKeys key) { }
	protected virtual void UIEventHandler(PSYViewEventArgs e) { }	
	protected virtual void UpdateEventHandler(PSYUpdateType type) { }
	protected virtual void MouseEventHandler(List<PSYMouseInput> input) { }
	protected virtual void TouchEventHandler(Touch[] touches) { }
	protected virtual void OnViewAnimationCompleted(PSYController cont, PSYAnimationEvent e) { }
	protected virtual void OnResolutionChange(PSYResolutionParams param) { }
	protected virtual void OnWindowOpened(PSYWindow wnd) { }
	protected virtual void OnWindowClosed(PSYWindow wnd) { }	
}
