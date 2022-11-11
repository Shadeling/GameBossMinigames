using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PSYViewType
{
	Root = 0,	
	Window = 1,
	Item = 2
}

public enum PSYViewEvent
{
	Open,
	Close,
	Select,
	Deselect,
	Enable,
	Disable,
	MouseIn,
	MouseOut,
	MouseHover,
	MouseDown,
	MouseUp,
	Click,
	Hold,
	Kill,
	Show,
	Hide,
}

public enum PSYSuperposition
{    
    /// <summary>
    /// All animations will be performed one after another
    /// </summary>
	Stack,

    /// <summary>
    /// All currently playing animations of this view with conflicting types will be interrupted before new animation starts, others will continue
    /// </summary>
	Merge,

    /// <summary>
    /// All currently playing animations of this view will be interrupted before new animation starts
    /// </summary>
	Break
}

public interface IPSYEventHandler
{
    void PSYEventHandler(PSYEvent e, PSYParams param);
}

// public interface PSYLocalizer
// {
//     bool CanLocalize(string text);
//     string Localize(string text);
//     string Localize(string text, object value);
//     string LocalizeDefault(string text);
// }

[System.Serializable]
public class PSYWindowMapper
{
#pragma warning disable 414 // used as human readable name in editor
    [SerializeField]
    private string      name = "";
#pragma warning restore 414 
    [SerializeField]
    private PSYWindow   window = PSYWindow.None;
    [SerializeField]
    private string      path = "";

    public PSYWindow Window
    {
        get
        {
            return window;
        }

        set
        {
            window = value;
        }
    }

    public string Path
    {
        get
        {
            return path;
        }

        set
        {
            path = value;
        }
    }

    public PSYWindowMapper(PSYWindow window, string path)
    {
        this.window = window;
        this.path = path;
        name = window.ToString(); 
    }
}

[System.Serializable]
public class PSYItemMapper
{
#pragma warning disable 414 // used as human readable name in editor
    [SerializeField]
    private string      name = "";
#pragma warning restore 414
    [SerializeField]
    private PSYItem     item = PSYItem.None;
    [SerializeField]
    private string      path = "";

    public PSYItem Item
    {
        get
        {
            return item;
        }

        set
        {
            item = value;
        }
    }

    public string Path
    {
        get
        {
            return path;
        }

        set
        {
            path = value;
        }
    }

    public PSYItemMapper(PSYItem item, string path)
    {
        this.item = item;
        this.path = path;
        name = item.ToString();
    }
}

[Serializable]
public class PSYMeasure
{
	public string ID { get; private set; }
	public float minimum;
	public float maximum;
	public float current;

	public delegate void ChangeHandler(int difference);
	public event ChangeHandler OnChanged;

	public float percent { get { return (float)current / maximum; } }
	public float percent100 { get { return current * 100 / maximum; } }

    public PSYMeasure(float minimum = 0, float maximum = 0, float current = 0, string id = "")
	{
		this.minimum = minimum;
		this.maximum = maximum;
		this.current = current;
		this.ID = id;
	}

    public PSYMeasure Add(float val)
    {
        this.current = Mathf.Clamp(this.current + val, minimum, maximum);
        return this;
    }

    public PSYMeasure Set(float val)
    {
        this.current = Mathf.Clamp(val, minimum, maximum);
        return this;
    }

    public float Rand()
    {
        return minimum + PSYTools.GetRandomF(maximum - minimum);
    }

	public static PSYMeasure operator +(PSYMeasure m, int value)
	{
        float old = m.current;
		m.current = Mathf.Clamp(m.current + value, m.minimum, m.maximum);
		if (m.current != old && m.OnChanged != null)
			m.OnChanged(value);
		//PSYDebug.Log(PSYDebug.Source.Model, "<color=red>{0}</color> {1} + {2} = {3}; [{4} {5}]", m.ID, old, value, m.current, m.minimum, m.maximum);
		return m;
	}

	public static PSYMeasure operator -(PSYMeasure m, int value)
	{
        float old = m.current;
		m.current = Mathf.Clamp(m.current - value, m.minimum, m.maximum);
		if (m.current != old && m.OnChanged != null)
			m.OnChanged(-value);
		//PSYDebug.Log(PSYDebug.Source.Model, "<color=red>{0}</color> {1} - {2} = {3}; [{4} {5}]", m.ID, old, value, m.current, m.minimum, m.maximum);
		return m;
	}    
}

public class PSYDelayedEvent
{
	public PSYEvent e { get; private set; }
	public PSYParams param { get; private set; }
	public float delay { get; private set; }
	public float timeLeft;
	public PSYDelayedEvent(PSYEvent e, PSYParams param, float delay)
	{
		this.e = e;
		this.param = param;
		this.delay = delay;
		timeLeft = delay;
	}
}

public class PSYMouseInput
{
	public Vector3 start;
	public Vector3 current;
	public int button;

	public PSYMouseInput(int button, Vector3 start, Vector3 current)
	{
		this.button = button;
		this.start = start;
		this.current = current;
	}

	public override string ToString()
	{
		return "{Mouse: " + button.ToString() + " " + current.ToString() + " " + start.ToString();
	}
}

[Serializable]
public class PSYViewState
{
    public string                           id = "";
    private bool                            animating;
    private bool                            selected;
    private bool                            active;
    private bool                            opened;
    private bool                            hovered;
    private bool                            pressed;
    private bool                            holded;
    private bool                            dying;
    public List<PSYViewEvent>               states = new List<PSYViewEvent>() { PSYViewEvent.Deselect, PSYViewEvent.Close, PSYViewEvent.Disable };

    public List<PSYViewEvent> States
    {
        get
        {
            return states;
        }
    }
    

    public PSYViewState(string id, bool opened, bool enabled, bool selected)
    {
        this.id = id;
        Opened = opened;
        Enabled = enabled;
        Selected = selected;        
    }

    public bool Animating
    {
        get
        {
            return animating;
        }

        set
        {
            if (animating == value)
            {
                return;
            }

            animating = value;
            //PSYDebug.Log(PSYDebug.Source.ViewState, "{0} {1}", value ? "animating" : "animation end", id);
        }
    }

    public bool Selected
    {
        get
        {
            return selected;
        }

        set
        {
            if (selected == value)
            {
                return;
            }

            selected = value;

            if(value)
            {
                states.Add(PSYViewEvent.Select);
                states.Remove(PSYViewEvent.Deselect);
            }
            else
            {
                states.Remove(PSYViewEvent.Select);
                states.Add(PSYViewEvent.Deselect);
            }
            //PSYDebug.Log(PSYDebug.Source.ViewState, "{0} {1}", value ? "select" : "deselect", id);
        }
    }

    public bool Enabled
    {
        get
        {
            return active;
        }

        set
        {
            if (active == value)
            {
                return;
            }

            active = value;
            if (value)
            {
                states.Add(PSYViewEvent.Enable);
                states.Remove(PSYViewEvent.Disable);
            }
            else
            {
                states.Remove(PSYViewEvent.Enable);
                states.Add(PSYViewEvent.Disable);
            }
            //PSYDebug.Log(PSYDebug.Source.ViewState, "{0} {1}", value ? "activate" : "deactivate", id);
        }
    }

    public bool Opened
    {
        get
        {
            return opened;
        }

        set
        {
            if (opened == value)
            {
                return;
            }

            opened = value;
            if (value)
            {
                states.Add(PSYViewEvent.Open);
                states.Remove(PSYViewEvent.Close);
            }
            else
            {
                states.Remove(PSYViewEvent.Open);
                states.Add(PSYViewEvent.Close);
            }
            //PSYDebug.Log(PSYDebug.Source.ViewState, "{0} {1}", value ? "open" : "close", id);
        }
    }

    public bool Hovered
    {
        get
        {
            return hovered;
        }

        set
        {
            if (hovered == value)
            {
                return;
            }

            hovered = value;
            //PSYDebug.Log(PSYDebug.Source.ViewState, "{0} {1}", value ? "MouseIn" : "MouseOut", id);
        }
    }

    public bool Pressed
    {
        get
        {
            return pressed;
        }

        set
        {
            if (pressed == value)
            {
                return;
            }

            pressed = value;
            //PSYDebug.Log(PSYDebug.Source.ViewState, "{0} {1}", value ? "press" : "release", id);
        }
    }

    public bool Holded
    {
        get
        {
            return holded;
        }

        set
        {
            if (holded == value)
            {
                return;
            }

            holded = value;
            //PSYDebug.Log(PSYDebug.Source.ViewState, "{0} {1}", value ? "hold" : "hold end", id);
        }
    }

    public bool Closing
    {
        get;
        set;
    }

    public bool Dying
    {
        get
        {
            return dying;
        }

        set
        {
            if (dying == value)
            {
                return;
            }

            dying = value;
            //PSYDebug.Log(PSYDebug.Source.ViewState, "{0} {1}", value ? "dying" : "cancel death", id);
        }
    }
}

[Serializable]
public class PSYHoldMode
{
	public float threshold;
	public int intervalSteps;
	public int curPhase;
	public int curStep;
	public float lastTime;
	public float incrementTime = 1.0f;
	public int increment = 1;
	public bool ignoreNextClick;
	public float firstThreshold = 0.8f;//Activate press events only when that much time is passed since button was pressed
	public float baseThreshold = 0.4f;//After first threshold is passed, this one becomes active and will decrease over time
	public float acceleration = 0.2f;//Speed of threshold decreasing (threshold -= threshold * thisValue)
    public bool enabled = false;

	public void Activate()
	{
		threshold = baseThreshold;
		intervalSteps = 1;
		curStep = 0;
		curPhase = 0;
		lastTime = 0;
		incrementTime = 0;
        increment = 1;
		lastTime = Time.time;
	}

	public bool clickPermission { get { return curStep == 0 || !ignoreNextClick; } }
}

public class PSYResolutionParams
{
	public Vector2 current;
	public Vector2 previous;
	public Vector2 dif;
	public int proportionalWidth;

    /// <summary>
    /// Needed to handle situation when height of the screen is lesser than referencedResolution.y for clampedY = true
    /// </summary>
    public int proportionalWidthForWidthScreen;

    /// <summary>
    /// Difference between current width of the screen and expected proportional width of new resolution height
    /// </summary>
    public int difference;

	/// <summary>
	/// Difference between current width of the screen and expected proportional width of new resolution height in percentage
	/// </summary>
	public float widthPercent;

	/// <summary>
	/// Difference between difference and expected proportional width of new resolution height in percentage
	/// </summary>
	public float differencePercent;

	public float correctionX;

	/// <summary>
	/// If current width of the screen is less than expected proportional width of new resolution height
	/// </summary>
	public bool clampedX = false;

    /// <summary>
	/// If current height of the screen is less than expected proportional height of new resolution
	/// </summary>
	public bool clampedY = false;

    /// <summary>
    /// If current width is less than expected proportional, and you need to fit both: width and height, this returns proper height to fit them
    /// </summary>
    public int fitInHeight = 0;

    public float fitInHeightPercent = 0;

    public float matchHeightDifHeight = 1.0f;
    public float matchWidthDifWidth = 1.0f;
    public float matchHeightDifWidth = 0.0f;
    public float matchWidthDifHeight = 0.0f;

    public CanvasScaler canvasScaler = null;

    public float difWidth
    {
        get
        {
            return canvasScaler.matchWidthOrHeight == 0 ? previous.x / current.x : matchHeightDifWidth;
        }
    }
    public float difHeight
    {
        get
        {
            return canvasScaler.matchWidthOrHeight == 1 ? previous.y / current.y : matchWidthDifHeight;
        }
    }

    public int ActiveWidth
    {
        get
        {
            if (clampedX)
            {
                return (int)(PSYGUI.ReferencedResolution.x * PSYGUI.ResolutionScale);
            }
            else if (clampedY)
            {
                return (int)(current.x / current.y * previous.y * PSYGUI.ResolutionScale);
            }
            else
            {
                return (int)(PSYGUI.ReferencedResolution.x * PSYGUI.ResolutionScale);
            }
        }
    }

    public int ActiveHeight
    {
        get
        {
            if (clampedX)
            {
                return (int)(PSYGUI.ReferencedResolution.x / current.x * current.y * PSYGUI.ResolutionScale);
            }
            else
            {
                return (int)(PSYGUI.ReferencedResolution.y * PSYGUI.ResolutionScale);
            }
        }
    }

    public PSYResolutionParams(Vector2 current, Vector2 previous, CanvasScaler cs = null, bool log = false)
	{        
        this.current = current;
		this.previous = previous;
		this.dif = current - previous;
		this.proportionalWidth = (int)(current.y / previous.y * previous.x);
        this.difference = (int)((current.x - proportionalWidth));
		this.widthPercent = current.x / proportionalWidth;
		this.differencePercent = (float)difference / proportionalWidth;		
		this.correctionX = previous.x * differencePercent;
		this.clampedX = current.x < proportionalWidth;
        this.clampedY = current.x > proportionalWidth;
        this.fitInHeight = (int)(this.clampedX ? current.y * (1 + differencePercent) : current.y);
		this.fitInHeightPercent = fitInHeight/ current.y;

        if (cs != null)
        {
            this.canvasScaler = cs;
            cs.matchWidthOrHeight = clampedY ? 1.0f : 0.0f;

            PSYDebug.Log(PSYDebug.Source.Temp, "cs.matchWidthOrHeight {0}", cs.matchWidthOrHeight);

            if (cs.matchWidthOrHeight == 0)
            {
                matchWidthDifHeight = previous.x / current.x;
            }

            else if (cs.matchWidthOrHeight == 1)
            {
                matchHeightDifWidth = previous.y / current.y;
            }
        }

        if (log)
        {
            PSYDebug.Log(PSYDebug.Source.Core, "<color=brown> Resolution {0}, expectedWidth: {1}({2}%), clampedX: {3}, height to fit: {4} </color>", current, proportionalWidth, differencePercent, clampedX, fitInHeight);
        }
	}

	public override string ToString()
	{
		return string.Format("<color=brown> Resolution {0}, expectedWidth: {1}({2}%), clampedX {3}, height to fit: {4} </color>", current, proportionalWidth, differencePercent, clampedX, fitInHeight);
	}	
}

public class PSYViewEventArgs : EventArgs
{
	public PSYController controller { get; private set; }
	public PSYView sender { get { return controller.view; } }
	public PSYParams param { get; private set;}	
	public PSYViewEvent viewEvent { get; private set; }

	public int index { get { return sender.index; } }
	public PSYItem item { get { return controller == null ? PSYItem.None : sender.item; } }

	public float time { get; private set; }
	public bool simulated { get { return param.Get<bool>(0); } }
	public bool nested { get { return param.Get<bool>(1); } }
	public bool click { get { return viewEvent == PSYViewEvent.Click; } }
	public bool open { get { return viewEvent == PSYViewEvent.Open; } }
	public bool close { get { return viewEvent == PSYViewEvent.Close; } }
	public bool activate { get { return viewEvent == PSYViewEvent.Enable; } }
	public bool deactivate { get { return viewEvent == PSYViewEvent.Disable; } }
	public bool select { get { return viewEvent == PSYViewEvent.Select; } }
	public bool deselect { get { return viewEvent == PSYViewEvent.Deselect; } }

	public PSYViewEventArgs(PSYController controller, PSYViewEvent viewEvent, PSYParams param)
	{
		this.controller = controller;
		this.viewEvent = viewEvent;
		this.param = param;
		this.time = param.Get<float>();
	}
}

public class PSYWindowStack : Dictionary<PSYWindow, PSYController>
{
	public override string ToString()
	{
		string str = "{ Stack (" + this.Count.ToString() + "): ";
		foreach (var pair in this)
			str += pair.Key.ToString() + " ";
		return str + "}";
	}

	public bool Contains(PSYWindow type)
	{
		return ContainsKey(type);
	}

	public void Add(PSYController cont)
	{
		if (ContainsKey(cont.view.window))
		{
			PSYDebug.Warning(PSYDebug.Source.Manager, "{0} already exists", cont.view.window);
			return;
		}
		this.Add(cont.view.window, cont);
	}

	public new PSYController this[PSYWindow type]
	{
		get
		{
            PSYController result = null;
            if (!base.TryGetValue(type, out result))
            {
                PSYDebug.Warning(PSYDebug.Source.Manager, $"PSYController of type '{type}' not found!");
                return null;
            }

			return result;
		}
	}
}

[System.Serializable]
public class VectorI2
{
    public int x = 0;
    public int y = 0;

    public VectorI2()
    {
    }

    public VectorI2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static VectorI2 zero
    {
        get
        {
            return new VectorI2();
        }
    }

    public override string ToString()
    {
        return string.Format("<{0} {1}>", x, y);
    }
}

public interface IPSYTweenable
{
    Image Image
    {
        get;
    }

    Text Text
    {
        get;
    }

    Graphic Graphic
    {
        get;
    }

    RectTransform Rect
    {
        get;
    }

    CanvasGroup Group
    {
        get;
    }

    bool IsText
    {
        get;
    }

    bool IsGraphic
    {
        get;
    }

    string Name
    {
        get;
    }

    string DebugName
    {
        get;
    }
}

public class PSYTweenable: IPSYTweenable
{
    private Image image = null;
    private Text text = null;
    private RectTransform rect = null;
    private Graphic graphic = null;
    private CanvasGroup group = null;
    private string debugName = "";
        
    public PSYTweenable(RectTransform t)
    {
        rect = t;
        group = t.GetComponent<CanvasGroup>();

        debugName = "PSYTweenable: " + rect.name + "/" + (rect.parent != null ? (rect.parent.name + "/" + (rect.parent.parent != null ? rect.parent.parent.name : "Null")) : "Null");
    }

    public PSYTweenable(Graphic gr)
    {
        rect = gr.GetComponent<RectTransform>();        
        graphic = gr;
        if (gr is Image)
        {
            image = (Image)gr;
        }
        else
        {
            text = (Text)gr;
        }

        debugName = "PSYTweenable: " + rect.name + "/" + (rect.parent != null ? (rect.parent.name + "/" + (rect.parent.parent != null ? rect.parent.parent.name : "Null")) : "Null");
    }

    public Graphic Graphic
    {
        get
        {
            return graphic;
        }
    }

    public Image Image
    {
        get
        {
            return image;
        }
    }

    public Text Text
    {
        get { return text; }
    }

    public RectTransform Rect
    {
        get
        {
            return rect;
        }
    }

    public CanvasGroup Group
    {
        get
        {
            return group;
        }
    }

    public string Name
    {
        get
        {
            return rect.name;
        }
    }

    public string DebugName
    {
        get
        {
            return debugName;
        }
    }

    public bool IsText
    {
        get
        {
            return text != null;
        }
    }

    public bool IsGraphic
    {
        get
        {
            return graphic != null;
        }
    }    
}