//Unity MVC GUI Tools v2.0; Author: Dolan D.
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Events;
using XD;

[Serializable]
/// <summary>
/// View component of MVC pattern
/// </summary>
public sealed class PSYView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    /// <summary>
    /// TODP: Убрать, статику храним в одном месте
    /// </summary>
    public static int       numViewTotal = 0;
    public static int       numViewCurrent = 0;

    //TODO:Паблик переменные вперемешку с приватными

    /// <summary>
    /// State change event
    /// </summary>
    /// TODO:Перенести в отдельный скрипт с классами
    public class StateEvent : UnityEvent<PSYViewEventArgs>
    {
    }

    /// <summary>
    /// Animation callback
    /// </summary>
    /// TODO:Перенести в отдельный скрипт с классами
    public class AnimationCompletedEvent : UnityEvent<PSYController, PSYAnimationEvent>
    {
    }

    [SerializeField]
    private PSYController           controller = null;

    /// <summary>
    /// Structural type of this view
    /// </summary>
    public PSYViewType              type = PSYViewType.Root;

    /// <summary>
    /// Type of window, if it is a window
    /// </summary>
    public PSYWindow                window;

    /// <summary>
    /// Type of item, if it is an item
    /// </summary>
    public PSYItem                  item;

    /// <summary>
    /// Index of item, if it is an item
    /// </summary>
    public int                      index = 0;

    public PSYViewState             state = null;

    public StateEvent               OnState = new StateEvent();

    public AnimationCompletedEvent  OnAnimationCompleted = new AnimationCompletedEvent(); 

    /// <summary>
    /// If view should be selected upon opening
    /// </summary>
    public bool                     isSelectedOnStart = false;

    /// <summary>
    /// If view should be instantiated in 'enabled' state
    /// </summary>
    public bool isEnabledOnStart = true;

    /// <summary>
    /// If view should be selected upon opening
    /// </summary>
    public bool clickOnStart = false;

    /// <summary>
    /// If view should be enabled upon opening
    /// </summary>
    public bool enableOnStart = false;

    /// <summary>
    /// If view should be disabled upon opening
    /// </summary>
    public bool disableOnStart = false;

    /// <summary>
    /// Animation superposition type
    /// </summary>
    public PSYSuperposition superposition = PSYSuperposition.Stack;

    /// <summary>
    /// If true, this item will change its state to selected and selection animation will be played upon click
    /// </summary>
    public bool isSelectable = true;

    /// <summary>
    /// If true, second selection attempt will deselect this item
    /// </summary>
    public bool isSwitch = false;

    /// <summary>
    /// If true, only one child item of this item can be selected at a time. Others will be deselected automatically.
    /// </summary>
    public bool isRadioGroup = false;

    /// <summary>
    /// If true, two or more identical animations can be queued (like open animation after open animation)
    /// </summary>
    public bool animDoubling = false;    

    /// <summary>
    /// Whether state should be changed and animation played according to parent view's state
    /// </summary>
    public bool                             puppet = true;
    private List<PSYViewEvent>              puppetEvents = new List<PSYViewEvent>() { PSYViewEvent.Open, PSYViewEvent.Close, PSYViewEvent.Enable, PSYViewEvent.Disable, PSYViewEvent.Kill };

    /// <summary>
    /// List of active cyclic animations
    /// </summary>
    private List<PSYAnimationEvent>         cyclicStarted = new List<PSYAnimationEvent>();

    /// <summary>
    /// If true, you can not interact with this view until all its animations are completed
    /// </summary>
    public bool                             animationBlocksInputLocal = true;

    /// <summary>
    /// If true, you can not interact with any view until all animations of this view are completed
    /// </summary>
    public bool                             animationBlocksInputGlobal = false;

    /// <summary>
    /// Minimum interval at which children can be clicked
    /// </summary>
    public float                            childrenClickInterval = 0.0f;

    /// <summary>
    /// Full list of animations, added to this view
    /// </summary>
	public List<PSYAnimation>               sets = new List<PSYAnimation>();

	public List<PSYViewEvent> events = new List<PSYViewEvent>() { PSYViewEvent.Click};
    public List<PSYAnimationEvent> animationEvents = new List<PSYAnimationEvent>() { };

    /// <summary>
    /// Animation queue, that resolves in different ways according to animation superposition mode
    /// </summary>
	private List<PSYAnimationEvent>         animationsQueue = new List<PSYAnimationEvent>();

    [SerializeField]
    /// <summary>
    /// List of all widgets, owned by this view
    /// </summary>
    private List<PSYWidget>                 widgets = null;

    /// <summary>
    /// Dictionary that will be filled on first request
    /// </summary>
    private Dictionary<string, PSYWidget>   widgetsDict = null;
    private Dictionary<string, PSYWidget>   WidgetsDict
    {
        get
        {
            if(widgetsDict == null)
            {
                FillWidgetsDict();
            }
            return widgetsDict;
        }
        set
        {
            widgetsDict = value;
        }
    }

    /// <summary>
    /// List of all "PSYView" children inside this one
    /// </summary>
    private List<PSYView>                   children = new List<PSYView>();

    [SerializeField]
    [HideInInspector]
    private PSYView                         parent;

    /// <summary>
    /// Holding left button pressed
    /// </summary>
    public PSYHoldMode                      hold = new PSYHoldMode();

    private float                           lastChildClickTime = 0;
    private float                           hoverTime = 0;
    private float                           hoverStartTime = 0;
    private string                          fullName = "";
    private Transform                       _transform = null;

    public List<PSYWidget> Widgets
    {
        get
        {
            return widgets;
        }

        set
        {
            widgets = value;
        }
    }

    public List<PSYView> Children
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

    private RectTransform                   rect = null;
    public RectTransform                    Rect
    {
        get
        {
            if (rect == null)
                rect = GetComponent<RectTransform>();

            return rect;
        }
    }

    public string FullName
    {
        get
        {
            return fullName;
        }

        private set
        {
            fullName = value;
            ID = String.Format("(V-{0}: {1})", IntID, value);
            controller.ID = ID;
        }
    }

    public int IntID
    {
        get;
        private set;
    }

    private string _id = "";

    public string ID
    {
        get
        {
            return _id;
        }
        private set
        {
            _id = value;
        }
    }

    public override string ToString()
    {
        return ID;
    }

    public bool isSelected
    {
        get
        {
            return state.Selected;
        }
    }

    public bool isOpened
    {
        get
        {
            return state.Opened;
        }
    }

    public bool isEnabled
    {
        get
        {
            return state.Enabled;
        }
    }

    public bool isAnimating
    {
        get
        {
            return state.Animating;
        }
    }
    public bool isPressed
    {
        get
        {
            return state.Pressed;
        }
    }

    public bool isHolded
    {
        get
        {
            return state.Holded;
        }
    }

    public bool isHovered
    {
        get
        {
            return state.Hovered;
        }
    }

    public bool isDying
    {
        get
        {
            return state.Dying;
        }
    }

    public bool isEnabledSet
    {
        get
        {
            return state.Enabled;
        }
        set
        {
            state.Enabled = value;
        }
    }

    public new Transform transform
    {
        get
        {
            if (_transform == null)
            {
                _transform = gameObject.transform;
            }
            return _transform;
        }
    }

    public PSYView Parent
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

    //Convinient properties
    public bool isWindow
    {
        get
        {
            return type == PSYViewType.Window;
        }
    }

    public bool isItem
    {
        get
        {
            return type == PSYViewType.Item;
        }
    }


    private bool ParentClickPermission
    {
        get
        {
            if (parent == null)
                return true;
            return parent.ChildrenClickIntervalPermission;
        }
    }

    public bool ChildrenClickIntervalPermission
    {
        get
        {
            if (Time.realtimeSinceStartup - lastChildClickTime > childrenClickInterval)
            {
                lastChildClickTime = Time.realtimeSinceStartup;
                return ParentClickPermission;
            }
            else
            {
                PSYDebug.Log(PSYDebug.Source.Input, this, "click aborted: children click interval restriction ({0})", childrenClickInterval);
                return false;
            }
        }
    }

    private bool BlocksAnimation
    {
        get
        {
            if (animationBlocksInputLocal && state.Animating)
            {
                PSYDebug.Log(PSYDebug.Source.Input, this, "click aborted: object is animating, local block");
                return true;
            }

            List<DG.Tweening.Tween> list = DOTween.PlayingTweens();
            if (list != null)
                for (int i = 0; i < list.Count; i++)
                    if (list[i].id != null && ((string)list[i].id).Contains("_B"))
                    {
                        PSYDebug.Log(PSYDebug.Source.Input, this, "click aborted: there are incomplete animations with global block ({0})", list[i].id);
                        return true;
                    }
            return false;
        }
    }

    private void SetParent(PSYView parent)
    {
        if (Parent == parent)
        {
            return;
        }

        Parent = parent;
    }


    public void SetFont(Font font, float size)
    {
        for (int i = 0; i < widgets.Count; i++)
        {
            if (widgets[i].isText)
            {
                widgets[i].Text.font = font;
                widgets[i].Text.fontSize = Mathf.RoundToInt(widgets[i].Text.fontSize * size);
            }
        }

        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetFont(font, size);
        }
    }

    private void Awake()
    {        
        if (controller == null)
        {
            controller = GetComponent<PSYController>();
            if (controller == null)
            {
                controller = gameObject.AddComponent<PSYController>();
            }
        }

        _transform = transform;
        name = name.Replace("(Clone)", "");
        numViewCurrent++;
        IntID = ++numViewTotal;

        if (widgets == null)
        {
            PSYTools.GetFreeWidgets(transform, widgets);
        }

        switch (type)
        {
            case PSYViewType.Root:
                FullName = name;
                break;

            default:
                //Items and windows in the resource folder aren't parented yet. Parent them now.                
                if (Parent == null)
                {
                    Parent = PSYTools.GetParentObject<PSYView>(transform);
                }

                FullName = Parent.FullName + "/" + name;
                Parent.RegisterChild(this);
                break;
        }

        state = new PSYViewState(ID, false, isEnabledOnStart, false);
    }

    private void FillWidgetsDict()
    {
        WidgetsDict = new Dictionary<string, PSYWidget>();        
        for (int i = 0; i < widgets.Count; i++)
        {
            WidgetsDict[widgets[i].name] = widgets[i];
        }
    }

    private void Start()
    {
        //Localize();
        //SetFont(ARGUEManager.currentFont, ARGUEManager.bEastFont ? ARGUEManager.EastFontSizeModificator : 1.0f);
    }

    public void RegisterChildTest(PSYView child)
    {
        if (children.Contains(child))
        {
            children.Remove(child);
            //children.Add(child);
        }
        children.Add(child);
        child.SetParent(this);
        PSYDebug.Log(PSYDebug.Source.Registration, this, "child №{0} registered: {1}", children.Count - 1, child.ID);
    }

    public void RegisterChild(PSYView child)
    {
        children.Add(child);
        child.SetParent(this);
        PSYDebug.Log(PSYDebug.Source.Registration, this, "child №{0} registered: {1}", children.Count - 1, child.ID);
    }

    private void UnregisterChild(PSYView child)
    {
        if (!children.Contains(child))
        {
            PSYDebug.Warning(PSYDebug.Source.Registration, this, "You are trying to unregister view {0}, but it is not a child", child.ID);
            return;
        }

        children.Remove(child);
        PSYDebug.Log(PSYDebug.Source.Registration, this, "view unregistered: {0} (children left: {1})", child.ID, children.Count);
    }

    //Shortcuts
    public void Open(bool toggle = false)
    {
        Perform(PSYViewEvent.Open, PSYParams.Empty, toggle);
    }
    public void Close(bool toggle = false)
    {
        Perform(PSYViewEvent.Close, PSYParams.Empty, toggle);
    }
    public void Select(bool toggle = false)
    {
        Perform(PSYViewEvent.Select, PSYParams.New(true), toggle);
    }
    public void Deselect(bool toggle = false)
    {
        Perform(PSYViewEvent.Deselect, PSYParams.New(true), toggle);
    }
    public void Enable(bool toggle = false)
    {
        Perform(PSYViewEvent.Enable, PSYParams.Empty, toggle);
    }
    public void Disable(bool toggle = false)
    {
        Perform(PSYViewEvent.Disable, PSYParams.Empty, toggle);
    }
    public void Kill()
    {
        Perform(PSYViewEvent.Kill, PSYParams.Empty, false);
    }

    public void OnPointerEnter(PointerEventData data)
    {
        if (type == PSYViewType.Window || type == PSYViewType.Root || state.Hovered)
            return;

        //PSYDebug.Log(PSYDebug.Source.Input, this, "OnPointerEnter" + (!state.hovered ? "" : " ignored"));

        //If it is possible, performing MouseIn
        if (isEnabled && isOpened)
            Perform(PSYViewEvent.MouseIn);
        else//Otherwise we should do "MouseIn" right after opening or enabling, not now. 
            state.Hovered = true;
    }

    public void OnPointerExit(PointerEventData data)
    {
        if (type == PSYViewType.Window || type == PSYViewType.Root || !state.Hovered)
            return;

        //PSYDebug.Log(PSYDebug.Source.Input, this, "OnPointerExit" + (state.hovered ? "" : " ignored"));	

        //If it is possible, performing MouseOut
        if (isEnabled && isOpened)
            Perform(PSYViewEvent.MouseOut);
        else//Otherwise we should just remember that the mouse is out
            state.Hovered = false;
    }

    /// <summary>
    /// Simulates user's click
    /// </summary>
    public void Click()
    {
        if (!isOpened)
        {
            PSYDebug.Log(PSYDebug.Source.Input, this, "SimulateClick delayed until view is opened");
            clickOnStart = true;
            return;
        }
        if (!isEnabled)
        {
            PSYDebug.Log(PSYDebug.Source.Input, this, "SimulateClick ignored, view isn't active");
            return;
        }

        PSYDebug.Log(PSYDebug.Source.Input, this, "SimulateClick");

        Perform(PSYViewEvent.MouseDown, false, true);
        Perform(PSYViewEvent.Click, false, true);
        Perform(PSYViewEvent.MouseUp, false, true);
    }

    public void OnPointerDown(PointerEventData data)
    {
        bool block = BlocksAnimation || !ParentClickPermission;

        PSYDebug.Log(PSYDebug.Source.Input, this, "OnPointerDown" + (block ? " ignored" : ""));

        if (block)
            return;

        //If disabled, will fire Press event anyway. If Press shouldn't be called, uncomment
        if (!isOpened || !isEnabled)
            return;

        Perform(PSYViewEvent.MouseDown, PSYParams.New(data.position));
    }

    public void OnPointerUp(PointerEventData data)
    {
        PSYDebug.Log(PSYDebug.Source.Input, this, "OnPointerUp" + (isEnabled ? "" : " ignored"));
        if (isOpened && isEnabled)
            Perform(PSYViewEvent.MouseUp);
    }

    public void OnPointerClick(PointerEventData data)
    {
        bool block = BlocksAnimation || !PSYGUI.clickPermission || !ParentClickPermission;

        PSYDebug.Log(PSYDebug.Source.Input, this, "OnPointerClick" + (block ? " ignored" : ""));

        if (block)
            return;
        
        if (!isOpened || !isEnabled)
            return;

        //If "hold mode" occurs and it's option to ignore following click is checked, that click shall not count as click
        if (!hold.clickPermission)
        {
            PSYDebug.Log(PSYDebug.Source.Input, this, "ignoring click event after hold mode");
            return;
        }

        lastChildClickTime = Time.realtimeSinceStartup;
        PSYGUI.lastClickTime = Time.realtimeSinceStartup;

        Perform(PSYViewEvent.Click);
    }

    /// <summary>
    /// Occurs if "hold mode" is enabled and user holds mouse button long enough.
    /// Event "PSYViewEvent.Hold" fires more and more rapidly after time, allowing to perform some increment/decrement actions.
    /// </summary>
    private void Hold()
    {
        hold.curStep++;
        hold.lastTime = Time.time;
        Perform(PSYViewEvent.Hold, PSYParams.New(hold.increment));
        if (hold.curStep >= hold.intervalSteps)
        {
            hold.curPhase++;
            hold.intervalSteps = hold.curPhase * 3;
            hold.threshold -= hold.threshold * hold.acceleration;
            if (hold.baseThreshold / hold.threshold > 1000)
            {
                if (hold.incrementTime == 0)
                    hold.incrementTime = Time.time;
                hold.increment = Mathf.Clamp((int)((Time.time - hold.incrementTime) / hold.incrementTime), 1, 1000000);
                hold.increment *= hold.increment;
            }
        }
    }

    private void OnGUIUpdate(PSYUpdateType interval)
    {
        //Firing hover event
        if (state.Hovered && !controller.ignoreEvents.Contains(PSYViewEvent.MouseHover))
        {
            hoverTime = Time.realtimeSinceStartup - hoverStartTime;
            if (OnState != null)
            {
                OnState.Invoke(new PSYViewEventArgs(controller, PSYViewEvent.MouseHover, PSYParams.New(hoverTime)));
            }
        }

        //Firing hold event
        if (hold.enabled && state.Pressed && Time.time - hold.lastTime >= (isHolded ? hold.threshold : hold.firstThreshold))
        {
            Hold();
        }
    }

    public void Perform(PSYViewEvent e, bool toggle = false, bool simulation = false)
    {
        Perform(e, PSYParams.Empty, toggle, simulation);
    }

    public void Perform(PSYViewEvent e, PSYParams param, bool toggle = false, bool simulation = false)
    {
        if (state.Dying || (isWindow && state.Closing))
        {
            return;
        }

        switch (e)
        {
            case PSYViewEvent.Enable:
                gameObject.SetActive(true);
                break;

            case PSYViewEvent.Kill:
                break;

            default:
                if(!gameObject.activeSelf)
                {
                    return;
                }
                break;
        }

        //bool val = false;
        //If view is already in target state, checking toggle
        if (state.states.Contains(e) && e != PSYViewEvent.Close)
        {
            if (toggle)//Changing animation to opposite
            {
                //PSYDebug.Log(PSYDebug.Source.ViewEvent, this, "toggling {0}", e);
                Perform(PSYTools.GetInverseViewEvent(e), param, toggle);
            }
            // else if (!parent.isRadioGroup)//Do not spam at radioGroup  !!! причина Null Reference Exception в определенных ситуациях
            // {
            //     //PSYDebug.Warning(PSYDebug.Source.ViewEvent, this, "ignoring {0}, already in that state", e);
            // }
            return;
        }

        if (simulation)
        {
            param.Add(true);
        }

        //PSYDebug.Log(PSYDebug.Source.ViewEvent, this, "{0} {1} {2}", e, param, transform.localPosition);

        switch (e)
        {
            case PSYViewEvent.Open:
                state.Opened = true;
                break;

            case PSYViewEvent.Close:
                state.Opened = false;
                state.Closing = true;
                break;

            case PSYViewEvent.Select:
                state.Selected = true;
                break;

            case PSYViewEvent.Deselect:
                state.Selected = false;
                break;

            case PSYViewEvent.Enable:
                gameObject.SetActive(true);
                state.Enabled = true;
                break;

            case PSYViewEvent.Disable:
                state.Enabled = false;
                break;

            case PSYViewEvent.MouseIn:
                state.Hovered = true;
                hoverTime = 0;
                hoverStartTime = Time.realtimeSinceStartup;
                if (!controller.ignoreEvents.Contains(PSYViewEvent.MouseHover) || hold.enabled)
                {
                    PSYManager.OnUpdateEvent.AddListener(OnGUIUpdate);
                }
                break;

            case PSYViewEvent.MouseOut:
                state.Hovered = false;
                PSYManager.OnUpdateEvent.RemoveListener(OnGUIUpdate);
                break;

            case PSYViewEvent.Click:
                state.Holded = false;
                break;

            case PSYViewEvent.MouseDown:
                state.Pressed = true;
                hold.Activate();
                break;

            case PSYViewEvent.MouseUp:
                state.Pressed = false;
                state.Holded = false;
                break;

            case PSYViewEvent.Hold:
                state.Holded = true;
                break;

            case PSYViewEvent.Kill:
                state.Opened = false;
                state.Dying = true;
                break;
        }

        //Performing animation
        PerformAnimation(e);

        //Firing event
        if (OnState != null && controller != null && !controller.ignoreEvents.Contains(e))
        {
            PSYDebug.Log(PSYDebug.Source.View, this, "Invoke {0}", e);
            OnState.Invoke(new PSYViewEventArgs(controller, e, param));
        }

        //Performing the same event on puppets
        for (int i = children.Count - 1; i >= 0; i--)
        {
            if (children[i] != null && children[i].puppet && puppetEvents.Contains(e) && (children[i].gameObject.activeSelf || e != PSYViewEvent.Open))
            {
                if (children[i].controller.IsStarted)
                {
                    children[i].Perform(e, param);
                }
                else if(e == PSYViewEvent.Disable)
                {
                    //children[i].disableOnStart = true;
                }
            }
        }

        //After all that we are ready to perform some post-events
        switch (e)
        {
            case PSYViewEvent.Open:
                if (enableOnStart)
                    Perform(PSYViewEvent.Enable);
                if (disableOnStart)
                    Perform(PSYViewEvent.Disable);
                if (isSelectedOnStart)
                    Perform(PSYViewEvent.Select, false, true);
                if (clickOnStart)
                {
                    //PSYDebug.Log(PSYDebug.Source.Temp, this, "Executing clickOnStart");
                    Perform(PSYViewEvent.MouseDown, false, true);
                    Perform(PSYViewEvent.Click, false, true);
                }
                break;

            //Checking whether this view should be selected or not when pressed
            case PSYViewEvent.Click:
                if (!isSelectable)
                    break;
                if (!state.Selected || isSwitch)
                    Perform(PSYViewEvent.Select, true, simulation);
                break;

            //If parent is radiogroup, we should notify it about selection
            case PSYViewEvent.Select:
                if (parent != null && parent.isRadioGroup)
                    parent.ChildrenSelected(this);
                break;

            case PSYViewEvent.Kill:
                //If gameObject is inactive, destroyint it immediately because the kill animation won't be played
                if (!gameObject.activeSelf)
                {
                    Destroy();
                }
                break;
        }
    }

    /// <summary>
    /// For radiogroup only. When child is selected, all others sould be deselected
    /// </summary>
    /// <param name="child"></param>
	private void ChildrenSelected(PSYView child)
    {
        for (int i = 0; i < children.Count; i++)
            if (child != children[i])
                children[i].Perform(PSYViewEvent.Deselect);
    }

    private void PerformAnimation(PSYViewEvent e)
    {
        switch (e)
        {
            case PSYViewEvent.Open:
                PlayAnimation(PSYAnimationEvent.Open);
                break;
            case PSYViewEvent.Close:
                PlayAnimation(PSYAnimationEvent.Close);
                break;
            case PSYViewEvent.Select:
                PlayAnimation(PSYAnimationEvent.Select);
                break;
            case PSYViewEvent.Deselect:
                PlayAnimation(PSYAnimationEvent.Deselect);
                break;
            case PSYViewEvent.Enable:
                PlayAnimation(PSYAnimationEvent.Enable);
                break;
            case PSYViewEvent.Disable:
                PlayAnimation(PSYAnimationEvent.Disable);
                break;
            case PSYViewEvent.Click:
                PlayAnimation(PSYAnimationEvent.Click);
                break;
            case PSYViewEvent.MouseDown:
                PlayAnimation(PSYAnimationEvent.MouseDown);
                break;
            case PSYViewEvent.MouseUp:
                PlayAnimation(PSYAnimationEvent.MouseUp);
                break;
            case PSYViewEvent.Hold:
                PlayAnimation(PSYAnimationEvent.Hold);
                break;
            case PSYViewEvent.Kill:
                PlayAnimation(PSYAnimationEvent.Kill);
                break;
            case PSYViewEvent.MouseIn:
                PlayAnimation(PSYAnimationEvent.MouseIn);
                break;
            case PSYViewEvent.MouseOut:
                PlayAnimation(PSYAnimationEvent.MouseOut);
                break;
        }
    }

    public void PlayAnimation(PSYAnimationEvent anim)
    {
        //PSYDebug.Log(PSYDebug.Source.ViewAnimation, this, "{0}", anim);

        PSYAnimation set = GetSet(anim);
        if (set == null && anim != PSYAnimationEvent.Close && anim != PSYAnimationEvent.Kill)
        {
            return;
        }

        switch (superposition)
        {
            case PSYSuperposition.Stack:
                break;
            case PSYSuperposition.Merge:
                animationsQueue = new List<PSYAnimationEvent>();
                break;
            case PSYSuperposition.Break:
                animationsQueue = new List<PSYAnimationEvent>();
                DOTween.Kill(ID);
                DOTween.Kill(ID + "_B");
                //PSYDebug.Log(PSYDebug.Source.Temp, this, "KILLED {0}", DOTween.Kill(ID));
                break;
        }

        AddAnimationToQueue(anim);

        if (superposition != PSYSuperposition.Stack || !isAnimating)
        {
            LaunchAnimationFromQueue();
        }
    }

    private void AddAnimationToQueue(PSYAnimationEvent anim)
    {
        //Don't add the second identical animation unless specified
        if (!animDoubling)
            for (int i = 0; i < animationsQueue.Count; i++)
                if (animationsQueue[i] == anim)
                {
                    //PSYDebug.Log(PSYDebug.Source.ViewAnimation, this, "Ignoring {0} animation because similar one is already queued. If doubling is necessary, set 'animDoubles' to true", anim);
                    return;
                }

        //Removing reversable pairs
        for (int i = 0; i < animationsQueue.Count; i++)
            if (animationsQueue[i] == PSYTools.GetInverseAnimationEvent(anim) && PSYTools.GetInverseAnimationEvent(anim) != anim)
            {
                animationsQueue.RemoveAt(i);
                return;
            }

        animationsQueue.Add(anim);

        PSYDebug.Log(PSYDebug.Source.ViewAnimation, this, "Added to queue: {0}", anim);
    }

    private void LaunchAnimationFromQueue()
    {
        if (animationsQueue.Count == 0)
        {
            LaunchCyclicAnimation();
            return;
        }
        PSYAnimationEvent e = animationsQueue[0];
        animationsQueue.RemoveAt(0);//Removing completed animation from queue
        PSYAnimation set = GetSet(e);
        if (set == null)
        {
            AnimationEmptyCallBack(e);
            return;
        }
        PSYDebug.Log(PSYDebug.Source.ViewAnimation, this, "Playing queued: {0}", e);
        if (!set.cyclic)
        {
            state.Animating = true;
        }
        if (set.cyclic && !cyclicStarted.Contains(set.evnt))
            cyclicStarted.Add(set.evnt);
        //StartCoroutine(PlayAnimationCoroutine(set));
        set.Play(1.0f, index);
    }

    private IEnumerator PlayAnimationCoroutine(PSYAnimation set)
    {
        yield return new WaitForSeconds(0);
        set.Play(1.0f, index);
        yield break;
    }

    public void StopCyclicAnimations()
    {
        cyclicStarted.Clear();
    }
    
    public void StopCyclicAnimation(PSYAnimationEvent e)
    {
        PSYAnimation set = GetSet(e);
        if (set != null && cyclicStarted.Contains(set.evnt))
            cyclicStarted.Remove(set.evnt);
    }

    private void LaunchCyclicAnimation()
    {
        for (int i = 0; i < sets.Count; i++)
            if (sets[i].cyclic && cyclicStarted.Contains(sets[i].evnt) && !sets[i].isPlaying)
                PlayAnimation(sets[i].evnt);
    }

    private void AnimationEmptyCallBack(PSYAnimationEvent e)
    {
        //PSYDebug.Log(PSYDebug.Source.ViewAnimation, this, "{0} animation not found", e);
        state.Animating = false;
        PostAnimationEffects(e);
        if (state.Hovered && e == PSYAnimationEvent.Open)
            PlayAnimation(PSYAnimationEvent.MouseIn);
        else
            LaunchAnimationFromQueue();
    }

    public void AnimationSetCallBack(PSYAnimation set)
    {
        //PSYDebug.Log(PSYDebug.Source.ViewAnimation, this, "done {0}", set.evnt);
        state.Animating = false;
        if (OnAnimationCompleted != null)
            OnAnimationCompleted.Invoke(controller, set.evnt);

        if (!PostAnimationEffects(set.evnt) && animationsQueue.Count > 0 && animationsQueue[0] != PSYAnimationEvent.Enable)
        {
            animationsQueue.Clear();
            return;
        }
        if (state.Hovered && set.evnt == PSYAnimationEvent.Open && GetSet(PSYAnimationEvent.MouseIn) != null)
            PlayAnimation(PSYAnimationEvent.MouseIn);
        else if (state.Hovered && set.evnt == PSYAnimationEvent.Enable && GetSet(PSYAnimationEvent.MouseIn) != null)
            PlayAnimation(PSYAnimationEvent.MouseIn);
        else
            LaunchAnimationFromQueue();
    }

    /// <summary>
    /// Some post animation effects
    /// </summary>
    /// <returns>False if gameObject is going to be destroyed or disabled and view shouldn't perform animations/actions anymore</returns>
    private bool PostAnimationEffects(PSYAnimationEvent e)
    {
        switch (e)
        {
            case PSYAnimationEvent.Enable:
                gameObject.SetActive(true);
                break;

            case PSYAnimationEvent.Disable:
                //state.enabled = false;
                if (GetSet(PSYAnimationEvent.Disable).enabled && GetSet(PSYAnimationEvent.Disable).disableOnComplete)
                    gameObject.SetActive(false);
                return !GetSet(PSYAnimationEvent.Disable).disableOnComplete;

            case PSYAnimationEvent.Kill:
                Destroy();
                return false;

            case PSYAnimationEvent.Close:
                state.Closing = false;
                if (type == PSYViewType.Window && (GetSet(PSYAnimationEvent.Close) == null || GetSet(PSYAnimationEvent.Close).killOnComplete))
                    Perform(PSYViewEvent.Kill);
                return true;
        }

        return true;
    }

    private void DestroyAnimations()
    {
        foreach (var set in sets)
        {
            set.Clear();
        }

        cyclicStarted.Clear();

        for (int i = children.Count - 1; i >= 0; i--)
        {
            children[i].DestroyAnimations();
        }
    }

    private void DestroySubscriptions()
    {
        PSYManager.OnUpdateEvent.RemoveListener(OnGUIUpdate);
        for (int i = children.Count - 1; i >= 0; i--)
            children[i].DestroySubscriptions();
    }

    private void DestroyObjects()
    {
        for (int i = children.Count - 1; i >= 0; i--)
            children[i].Destroy();

        //PSYDebug.Log(PSYDebug.Source.View, this, "destroying now");

        //КОСТЫЛЬ
        if (this)
            GameObject.Destroy(gameObject);

        numViewCurrent--;
    }

    private void Destroy()
    {
        //PSYDebug.Log(PSYDebug.Source.View, this, "Destroy(); children: {0} ", children.Count);
        if (state != null)
        {
            state.Dying = true;
        }

        if (parent != null)
        {
            parent.UnregisterChild(this);
        }

        DestroySubscriptions();
        DestroyAnimations();
        DestroyObjects();
    }

    public PSYAnimation GetSet(PSYAnimationEvent e)
    {
        for (int i = 0; i < sets.Count; i++)
            if (sets[i].evnt == e)
                return sets[i];
        return null;
    }

    /// <summary>
    /// Returns the size of the visual presentation (from min coordinate to max coordinate for both axes)
    /// </summary>	
    public Vector2 GetSize()
    {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        if (widgets.Count == 0)
        {
            return new Vector3(0, 0, 1);
        }

        for (int i = 0; i < widgets.Count; i++)
        {
            float x1 = widgets[i].Rect.anchoredPosition3D.x - widgets[i].Rect.sizeDelta.x * widgets[i].Rect.pivot.x;
            float x2 = widgets[i].Rect.anchoredPosition3D.x + widgets[i].Rect.sizeDelta.x * (1 - widgets[i].Rect.pivot.x);
            float y1 = widgets[i].Rect.anchoredPosition3D.y - widgets[i].Rect.sizeDelta.y * widgets[i].Rect.pivot.y;
            float y2 = widgets[i].Rect.anchoredPosition3D.y + widgets[i].Rect.sizeDelta.y * (1 - widgets[i].Rect.pivot.y);
            if (x1 < minX)
                minX = x1;
            if (x2 > maxX)
                maxX = x2;
            if (y1 < minY)
                minY = y1;
            if (y2 > maxY)
                maxY = y2;
            //PSYDebug.Log(PSYDebug.Source.Temp, "Widget {0} {1} {2}", widgets[i].name, minX, maxX);
        }

        return new Vector3(maxX - minX, maxY - minY);
    }

    /// <summary>
    /// Returns PSYWidget by it's name
    /// </summary>
    /// <param name="type">Key name</param>
    public PSYWidget GetWidget(string name)
    {
        PSYWidget wdgt;
        WidgetsDict.TryGetValue(name, out wdgt);

        //if (wdgt == null)
        // Debug.LogError("GetWidget: " + name + " == NULL, view: " + this.name, gameObject);

        return wdgt;
    }

    /// <summary>
    /// <para>Sets some value based on widget's type</para>
    /// <para>If Color is passed, changes Image's or Text's color</para>
    /// <para>If Vector2 is passed, changes widget RectTransform's size</para>
    /// <para>If Sprite is passed, changes Image's sprite</para>
    /// <para>If Texture2D is passed, creates Sprite from Texture2D and uploads it to Image component</para>
    /// <para>If string is passed, changes Text's text</para>
    /// <para>If float is passed, changes Filled Image's percent</para>
    /// </summary>	
    public void SetWidget(string name, object value, bool setNativeSize = false)
    {
        PSYWidget widget = null;
        if (!WidgetsDict.TryGetValue(name, out widget))
        {
            PSYDebug.Warning(PSYDebug.Source.Widget, this, "Can't find widget with name {0}", name);
            return;
        }

        widget.Set(value, setNativeSize);
    }
    
    /// <summary>
    /// Set text without using PSYGUI.Localize
    /// </summary>
    /// <param name="name"> Widget GameObject name </param>
    /// <param name="text"> Already localised string </param>
    public void SetLocalisedText(string name, string text)
    {
        PSYWidget widget = null;
        if (!WidgetsDict.TryGetValue(name, out widget))
        {
            PSYDebug.Warning(PSYDebug.Source.Widget, this, "Can't find widget with name {0}", name);
            return;
        }

        widget.SetLocalisedText(text);
    }

    /// <summary>
    /// Quick way to show widget without specifying any animation
    /// </summary>
    /// <param name="name"></param>
	public void ShowWidget(string name, bool enableChildren = false)
    {
        for (int i = 0; i < widgets.Count; i++)
            if (name == "" || widgets[i].name == name)
            {
                widgets[i].Show();
                if (enableChildren)
                {
                    widgets[i].SetActiveChildren(true);
                }
                return;
            }
    }

    public void EnableWidget(string name, bool enable = true)
    {
        PSYWidget widget = null;
        if (!WidgetsDict.TryGetValue(name, out widget))
        {
            PSYDebug.Warning(PSYDebug.Source.Widget, this, "Can't find widget with name {0}", name);
            return;
        }

        widget.Transform.gameObject.SetActive(enable);
    }

    /// <summary>
    /// Quick way to hide widget without specifying any animation
    /// </summary>
    /// <param name="name"></param>
    public void HideWidget(string name, bool disableChildren = false)
    {
        for (int i = 0; i < widgets.Count; i++)
        {
            if (name == "" || widgets[i].name == name)
            {
                widgets[i].Hide();
                if (disableChildren)
                {
                    widgets[i].SetActiveChildren(false);
                }
                return;
            }
        }
    }

    public void SetActiveChildren(string name, bool val)
    {
        for (int i = 0; i < widgets.Count; i++)
        {
            if (name == "" || widgets[i].name == name)
            {
                widgets[i].SetActiveChildren(val);
                break;
            }
        }
    }

    public Vector3 GetScreenPosition()
    {
        Transform t = transform;
        Vector3 res = Vector3.zero;
        do
        {
            if (t.parent.GetComponent<PSYView>() != null && t.parent.GetComponent<PSYView>().type == PSYViewType.Root)
                return res + t.localPosition;
            else
                res += t.localPosition;
            t = t.parent;
        } while (1 == 1);
    }

    public void SetAnimationTime(PSYAnimationEvent e, float time)
    {
        for (int i = 0; i < sets.Count; i++)
            if (sets[i].evnt == e)
                for (int k = 0; k < sets[i].items.Count; k++)
                    sets[i].items[k].time = time;
    }

    public void SetAnimationGlobalDelay(PSYAnimationEvent e, float delay)
    {
        for (int i = 0; i < sets.Count; i++)
            if (sets[i].evnt == e)
                sets[i].delay = delay;
    }

    public void SetAnimationDelay(PSYAnimationEvent e, float delay)
    {
        for (int i = 0; i < sets.Count; i++)
            if (sets[i].evnt == e)
                for (int k = 0; k < sets[i].items.Count; k++)
                    sets[i].items[k].delay = delay;
    }

    public void SetAnimationEase(PSYAnimationEvent e, Ease ease)
    {
        for (int i = 0; i < sets.Count; i++)
            if (sets[i].evnt == e)
                for (int k = 0; k < sets[i].items.Count; k++)
                    sets[i].items[k].ease = ease;
    }

    public void SetAnimationIBDDelay(PSYAnimationEvent e, float delay)
    {
        for (int i = 0; i < sets.Count; i++)
            if (sets[i].evnt == e)
                for (int k = 0; k < sets[i].items.Count; k++)
                    sets[i].items[k].indexBasedDelay = delay;
    }

    public void SetAnimationValue(PSYAnimationEvent e, PSYAnimation.Type type, Vector4 val)
    {
        for (int i = 0; i < sets.Count; i++)
            if (sets[i].evnt == e)
            {
                for (int j = 0; j < sets[i].items.Count; j++)
                    if (sets[i].items[j].type == type)
                    {
                        sets[i].items[j].newValue = val;
                        return;
                    }
            }
    }

    public void SetAnimationValue(PSYAnimationEvent e, float itemN, Vector4 val)
    {
        for (int i = 0; i < sets.Count; i++)
            if (sets[i].evnt == e)
                sets[i].items[(int)itemN].newValue = val;
    }

    public void KillTweens()
    {
        List<Tween> tweens = DOTween.TweensById(ID);
        if (tweens != null)
        {
            for (int k = 0; k < tweens.Count; k++)
            {
                tweens[k].Kill();
            }
        }
    }
}