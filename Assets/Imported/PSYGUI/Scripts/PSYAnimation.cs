using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
 
[Serializable]
public class PSYAnimation
{
	public enum Type
	{
		Alpha = 0,
		Scale = 1,
		Move = 2,
		Spin = 3,
		Color = 4,
		Size = 5,
	}

    public static int AnimationTypeLength
    {
        get
        {
            if(_animationTypeLength == 0)
            {
                _animationTypeLength = Enum.GetValues(typeof(Type)).Length;
            }

            return _animationTypeLength;
        }
    }

    private static int _animationTypeLength = 0;

    [Serializable]
	public class Item
	{
		public static Vector4 ParseV4(float value) { return new Vector4(value, 0, 0, 0); }
		public static Vector4 ParseV4(Vector3 value) { return new Vector4(value.x, value.y, value.z, 0); }
		public static Vector4 ParseV4(Color value) { return new Vector4(value.r, value.g, value.b, value.a); }
		public static Vector3 ParseVector(Vector4 value) { return new Vector3(value.x, value.y, value.z); }
		public static Vector3 ParseScale(Vector4 value) { return new Vector3(value.x, value.y, 1); }
		public static Vector3 ParseSpin(Vector4 value) { return new Vector3(value.x, value.y, value.z); }
		public static Vector2 ParseSize(Vector4 value) { return new Vector2(value.x, value.y); }
		public static Color ParseColor(Vector4 value) { return new Color(value.x, value.y, value.z, value.w); }

		public Vector4 startValue;
		public Vector4 newValue;
		public Vector4 resValue;
		public Type type;
		public string target;
		public Ease ease = Ease.Linear;
		public float time = 0;
		public float delay = 0;
		public int loops = 1;
		public bool enabled = true;
		public bool relative = false;
		public bool random = false;
		public bool randomTime = false;
		public float addTime = 0;
		public bool rNegative = false;
		public bool bIndexBasedDelay = false;
		public float indexBasedDelay = 0.00f;
		public int IBDIndex = 0;

		/// <summary>
		/// Use with Move animation
		/// </summary>
		public bool useDepth = false;

		/// <summary>
		/// Use with Color animation
		/// </summary>
		public bool useAlpha = false;

		public Item(Type type, string target, Vector4 newValue, Ease ease, float time, float delay, int loops, bool relative, bool random, bool randomTime, bool rNegative, float addTime, bool bIBD, float IBD)
		{
			this.type = type;
			this.ease = ease;
			this.time = time;
			this.delay = delay;
			this.target = target;
			this.loops = loops;
			this.relative = relative;
			this.newValue = newValue;
			this.random = random;
			this.randomTime = randomTime;
			this.rNegative = rNegative;
			this.addTime = addTime;
			this.bIndexBasedDelay = bIBD;
			this.indexBasedDelay = IBD;
		}

		public Item Clone(string newTarget = "")
		{
			Item newA = new Item(type, newTarget != "" ? newTarget : target, newValue, ease, time, delay, loops, relative, random, randomTime, rNegative, addTime, bIndexBasedDelay, indexBasedDelay);
			newA.startValue = startValue;
			return newA;
		}

		public override string ToString()
		{
			return String.Format("TYPE={0} TARGET={1} V={2} T={3} D={4} L={5}", type, target, newValue, time, delay, loops);
		}

		public bool CheckErrors(PSYView view = null)
		{
			if (view != null && target == view.name) target = PSYGUI.WholeViewAnimationTarget;
			if (String.IsNullOrEmpty(target))
				target = PSYGUI.WholeViewAnimationTarget;
			return true;
		}
	}

	public PSYAnimationEvent                                evnt;
	public PSYView                                          view;
	public List<Item>                                       items = new List<Item>();
	public Dictionary<string, Dictionary<int, Sequence>>    sequences = null;
	public bool                                             enabled = true;	
	public bool                                             cyclic = false;
    public bool                                             disableOnComplete = true;
    public bool                                             killOnComplete = true;
    public float                                            childrenAnimationInterval = 0.0f;
	public float                                            delay = 0.0f;
	private float                                           maxDuration = -1;
    public bool                                             isPlaying = false;
    private Dictionary<string, Vector4>                     curTargetsValues = null;

    public PSYAnimation(PSYView view, PSYAnimationEvent e)
	{		
		this.view = view;
		this.evnt = e;
	}

	public string owner { get { return this.view != null ? this.view.name : "null"; } }

	[System.Runtime.CompilerServices.IndexerName("AnimationItem")]
	public Item this[int index] { get { return items[index]; } set { items[index] = value; } }

	public int Count {
        get
        {
            return items.Count;
        }
    }

	public void Clear()
	{
		PSYDebug.Log(PSYDebug.Source.Animation, view, "clear {0} set: {1} fields", evnt, items.Count);		
		ClearSequences();
		items.Clear();
		enabled = true;
	}

	private void ClearSequences()
	{		
		if (sequences != null)
		{
			foreach (var val1 in sequences.Values)
			{
				foreach (var val2 in val1.Values)
					val2.Kill();
			}
			sequences.Clear();
		}
	}

	public PSYAnimation Add(Item anim)
	{
		if (anim.target == null) return this;		
		items.Add(anim);		
		return this;
	}

	public List<Item> Remove(Item anim)
	{
		items.Remove(anim);		
		return items;
	}
	
	/// <summary>
	/// Clones this animation set for target PSYView component
	/// </summary>
	/// <param name="targetView">Target PSYView component to put clone in</param>
	/// <param name="oldSet">Some parameters of cloned set can be set from existing set</param>	
	public PSYAnimation Clone(PSYView targetView, PSYAnimation oldSet = null)
	{
		PSYAnimation newSet = new PSYAnimation(targetView, evnt);
		PSYAnimation paramSet = null;
		if (oldSet != null)
			paramSet = oldSet;
		else 
			paramSet = this;		
		newSet.cyclic = paramSet.cyclic;		
		for (int i = 0; i < items.Count; i++)
		{			
			string newTarget = "";
			//However, if there is a child widget with the same name as original target name, we use it as a target of cloned animation
			if (items[i].target == "Entire")
				newTarget = items[i].target;
			else
			{
				if (targetView.transform.FindDeepChild(items[i].target) != null)
					newTarget = items[i].target;
				else
					newTarget = "Entire";
			}
			Item newA = items[i].Clone(newTarget);
			newA.relative = items[i].relative;
            newSet.delay = paramSet.delay;
			newSet.Add(newA);
		}
		return newSet;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="fromSet"></param>
	public void Set(PSYAnimation fromSet)
	{
		this.cyclic = fromSet.cyclic;	
		this.childrenAnimationInterval = fromSet.childrenAnimationInterval;
		this.view = fromSet.view;
		this.items = fromSet.items;
		this.enabled = fromSet.enabled;
        this.delay = fromSet.delay;
	}

	/// <summary>
	/// Returns total animation set time all things considered
	/// </summary>
	public float TotalTime
	{
		get
		{
			Dictionary<Type, Dictionary<string, float>> dict = new Dictionary<Type, Dictionary<string, float>>();
			for (int i = 0; i < items.Count; i++)
			{
				float time = Mathf.Abs(items[i].delay) + Mathf.Abs(items[i].time);
				if (!dict.ContainsKey(items[i].type))
					dict[items[i].type] = new Dictionary<string, float>();
				if (!dict[items[i].type].ContainsKey(items[i].target))
					dict[items[i].type][items[i].target] = 0;
				dict[items[i].type][items[i].target] += time;
			}
			float maxTime = 0;
			foreach (var pair in dict)
			{
				foreach(var pair2 in pair.Value)
					if (pair2.Value > maxTime)
						maxTime = pair2.Value;
			}
			return maxTime;
		}
	}

	public void Pause()
	{
		if (sequences == null) return;
		foreach (var val1 in sequences.Values)
			foreach (var val2 in val1.Values)
				val2.Complete();
	}

	/// <summary>
	/// Start animation sequence
	/// </summary>
	public void Play(float timeScale = 1.0f, int IBDIndex = 0)
	{
		if (!enabled)
		{
			PSYDebug.Warning(PSYDebug.Source.Animation, view, "{0} set is disabled", evnt);
            CallBack();
            return;
		}

		CreateSequences(IBDIndex);

		if (sequences == null || sequences.Count == 0)
		{
			CallBack();
			return;
		}

		//PSYDebug.Log(PSYDebug.Source.Animation, view, "playing {0} set", evnt);
        isPlaying = true;
		maxDuration = -1;
		Sequence maxDurable = null;
        //Sequence withZeroTime = null;
        foreach (var val1 in sequences.Values)
			foreach (var val2 in val1.Values)
				if (val2.Duration() >= maxDuration)//>= because we should take the last one of all possible
				{
					maxDurable = val2;
					maxDuration = val2.Duration();
				}

		foreach (var val1 in sequences.Values)
			foreach (var val2 in val1.Values)
			{
				if (val2.target == null)
				{
					PSYDebug.Warning(PSYDebug.Source.Animation, view, "sequence targets is NULL");
					continue;
				}

				val2.timeScale = 1 / timeScale;
                if (val2 == maxDurable)
                {                    
                    val2.OnComplete(() => CallBack());
                }
				val2.Goto(0);
				val2.Restart();		
			}
	}

	private void CallBack()
	{        
        isPlaying = false;
		view.AnimationSetCallBack(this);
	}

	public Vector4 GetCurrentValue(IPSYTweenable target, Type type)
	{
		switch (type)
		{
			case Type.Color:
                return (Item.ParseV4(target.Graphic.color));

			case Type.Alpha:
                CanvasGroup gr = target.IsGraphic ? target.Graphic.GetComponent<CanvasGroup>() : null;
                return Item.ParseV4(target.IsGraphic ? (gr != null ? gr.alpha : target.Graphic.color.a) : target.Group.alpha);

			case Type.Move:
                return Item.ParseV4(target.Rect.anchoredPosition3D);

			case Type.Spin:
                return Item.ParseV4(target.Rect.eulerAngles);

			case Type.Scale:
                return Item.ParseV4(target.Rect.localScale);

			case Type.Size:
                return Item.ParseSize(target.Rect.sizeDelta);

			default:
                return Vector4.zero;
		}
	}

    /// <summary>
    /// Calculating animations' start values and end values, considering they might play one after another
    /// </summary>
    /// <param name="IBDIndex"></param>
    private void CalculateValues(int IBDIndex)
    {
        IPSYTweenable target = null;
        if (curTargetsValues == null)
        {
            curTargetsValues = new Dictionary<string, Vector4>();
        }     
        
        for (int i = 0; i < AnimationTypeLength; i++)
        {
            curTargetsValues.Clear();
            for (int j = 0; j < items.Count; j++)
            {
                Item anim = items[j];
                anim.IBDIndex = IBDIndex;
                if (anim.type != (Type)i)
                {
                    continue;//Searching for animation of current type
                }

                target = CalcTarget(anim);
                if(target == null)
                {
                    PSYDebug.Error(PSYDebug.Source.Animation, "Animation item error at {0}; {1}; Line №{2}", view.FullName, evnt, items.IndexOf(anim));
                    continue;
                }

                //If there is a value in dictionary for this target, we use it as a startValue
                //Otherwise it is a first animation of this type for this object, so we just use current value of the object
                string name = target.Name;
                Vector4 v4 = Vector4.zero;
                anim.startValue = curTargetsValues.TryGetValue(name, out v4) ? v4 : GetCurrentValue(target, anim.type);
                anim.resValue = CalculateResValue(anim, target);

                //Result value of this animation becomes a start value for the next animation of the same type for the same target
                curTargetsValues[name] = anim.resValue;
            }
        }
    }

    private PSYTweenable CalcTarget(Item anim)
    {
        if (anim.target == PSYGUI.WholeViewAnimationTarget)
        {
            return new PSYTweenable(view.Rect);
        }
        else
        {
            PSYWidget widgetTarget = view.GetWidget(anim.target);
            if (widgetTarget == null)
            {
                return null;
            }

            if (widgetTarget.isText)
            {
                return new PSYTweenable(widgetTarget.Text);
            }
            else
            {
                return new PSYTweenable(widgetTarget.Image);
            }
        }
    }

    private object ValueToObject(Vector4 value, Item anim, IPSYTweenable target)
    {
        switch (anim.type)
        {
            case Type.Spin:
                return Item.ParseSpin(value);

            case Type.Move:
                float z = anim.useDepth ? value.z : (anim.relative ? 0 : target.Rect.anchoredPosition3D.z);
                return Item.ParseVector(new Vector4(value.x, value.y, z, value.w));

            case Type.Scale:
                return Item.ParseScale(value);

            case Type.Alpha:
                return value.x;

            case Type.Color:
                return Item.ParseColor(value);

            case Type.Size:
                return Item.ParseSize(value);

            default:
                return null;
        }
    }

    /// <summary>
    /// Creates single sequence for each tween target from this set
    /// </summary>
    private void CreateSequences(int IBDIndex)
	{
		//PSYDebug.Log(PSYDebug.Source.Animation, view, "creating sequences for {0} set", evnt);

        if (sequences != null)
        {
            ClearSequences();
        }

        CalculateValues(IBDIndex);

        IPSYTweenable target = null;

		for (int i = 0; i < items.Count; i++)
		{
            //Getting proper target
            target = CalcTarget(items[i]);
            if (target == null)
            {
                PSYDebug.Error(PSYDebug.Source.Animation, "Animation item error at {0}; {1}; Line №{2}", view.FullName, evnt, i);
                continue;
            }

            //Adjusting time
            float time = items[i].time;
            if (items[i].randomTime)
            {
                time = items[i].addTime + PSYTools.GetRandomF(time);
            }

            if (time == 0)
            {
                time = 0.001f;
            }

            //float prevTime = 0;
			object startValue = null;
            Sequence seq = null;
            Dictionary<int, Sequence> seqDict = null;

            //storing new value to 'value', randomizing it if needed
            object value = items[i].newValue;
			if (items[i].random)
			{
                value = CalcRandowValue(items[i]);

                if (items[i].rNegative)
                {
                    value = (Vector4)value * (PSYTools.GetRandom(2) == 0 ? -1 : 1);
                }
			}

            value = ValueToObject((Vector4)value, items[i], target);

            //Storing start value in case of loops
            if (items[i].loops > 1)
            {
                startValue = ValueToObject(items[i].startValue, items[i], target);                
            }

            //Storing type
			Type type = items[i].type;

            if (sequences == null)
            {
                sequences = new Dictionary<string, Dictionary<int, Sequence>>();
            }

            if (!sequences.TryGetValue(target.Name, out seqDict))
            {
                seqDict = new Dictionary<int, Sequence>();
                sequences[target.Name] = seqDict;
            }
            
            if (!seqDict.TryGetValue((int)type, out seq))
			{				
                seq = DOTween.Sequence();
                seq.id = view.ID;
                seq.target = target;
                if (view.animationBlocksInputGlobal)
                {
                    seq.id += "_B";
                }
                seq.AppendInterval(delay * PSYGUI.globalAnimationTime);
                seqDict[(int)type] = seq;

                //PSYDebug.Log(PSYDebug.Source.Animation, view, "new {0} sequence: '{1}' {2}; ID = '{3}'", type, target != null ? target.Name : target.ToString(), value, seq.id);
            }

            float space = (items[i].delay + items[i].IBDIndex * items[i].indexBasedDelay) * PSYGUI.globalAnimationTime;
            if (space > 0)
            {
                seq.AppendInterval(space);
            }
			time = time / items[i].loops * PSYGUI.globalAnimationTime;
            seq.Append(CreateTween(target, value, type, time).SetRelative(items[i].relative).SetEase(items[i].ease));

            if (items[i].loops > 1)
            {
                for (int k = 1; k < items[i].loops; k += 2)
                {
                    seq.Append(CreateTween(target, startValue, type, time).SetRelative(false).SetEase(items[i].ease));
                    if (k + 1 == items[i].loops) break;
                    seq.Append(CreateTween(target, value, type, time).SetRelative(items[i].relative).SetEase(items[i].ease));
                }
            }

            //If there will be some sequence of the same type for the same target, we need to wait a little after zero time before starting it
            if (time == 0)
            {
                for (int k = i + 1; k < items.Count; k++)
                {
                    if (target == CalcTarget(items[k]) && items[i].type == items[k].type)
                    {
                        seq.AppendInterval(0.0001f);
                        break;
                    }
                }
            }
        }
	}

    private Vector4 CalcRandowValue(Item anim)
    {
        switch (anim.type)
        {
            case Type.Color:
                float r = (anim.relative ? 0 : anim.startValue.x) + PSYTools.GetRandomF(anim.newValue.x - (anim.relative ? 0 : anim.startValue.x));
                float g = (anim.relative ? 0 : anim.startValue.y) + PSYTools.GetRandomF(anim.newValue.y - (anim.relative ? 0 : anim.startValue.y));
                float b = (anim.relative ? 0 : anim.startValue.z) + PSYTools.GetRandomF(anim.newValue.z - (anim.relative ? 0 : anim.startValue.z));
                float a = 0;
                if (anim.useDepth)
                {
                    a = (anim.relative ? 0 : anim.startValue.w) + PSYTools.GetRandomF(anim.newValue.w - (anim.relative ? 0 : anim.startValue.w));
                }
                else
                {
                    a = anim.relative ? 0 : anim.startValue.w;
                }
                return new Vector4(r, g, b, a);

            case Type.Alpha:
                a = (anim.relative ? 0 : anim.startValue.x) + PSYTools.GetRandomF(anim.newValue.x - (anim.relative ? 0 : anim.startValue.x));
                return new Vector4(a, anim.newValue.y, anim.newValue.z, anim.newValue.w);

            case Type.Spin:
                float x = (anim.relative ? 0 : anim.startValue.x) + PSYTools.GetRandomF(anim.newValue.x - (anim.relative ? 0 : anim.startValue.x), anim.relative ? anim.rNegative : false);
                float y = (anim.relative ? 0 : anim.startValue.y) + PSYTools.GetRandomF(anim.newValue.y - (anim.relative ? 0 : anim.startValue.y), anim.relative ? anim.rNegative : false);
                float z = (anim.relative ? 0 : anim.startValue.z) + PSYTools.GetRandomF(anim.newValue.z - (anim.relative ? 0 : anim.startValue.z), anim.relative ? anim.rNegative : false);
                return new Vector4(x, y, z, 0);

            case Type.Move:
            case Type.Scale:
                x = (anim.relative ? 0 : anim.startValue.x) + PSYTools.GetRandomF(anim.newValue.x - (anim.relative ? 0 : anim.startValue.x), anim.relative ? anim.rNegative : false);
                y = (anim.relative ? 0 : anim.startValue.y) + PSYTools.GetRandomF(anim.newValue.y - (anim.relative ? 0 : anim.startValue.y), anim.relative ? anim.rNegative : false);
                return new Vector4(x, y, 0, 0);

            case Type.Size:
                z = (anim.relative ? 0 : anim.startValue.z) + PSYTools.GetRandomF(anim.newValue.z - (anim.relative ? 0 : anim.startValue.z), anim.relative ? anim.rNegative : false);
                return new Vector4(anim.newValue.x, anim.newValue.y, z, anim.newValue.w);

            default:
                return Vector4.zero;
        }
    }

    private Vector4 CalculateResValue(Item anim, IPSYTweenable target)
    {
        switch (anim.type)
        {
            case Type.Color:
                return Item.ParseV4(Item.ParseColor(anim.newValue));

            case Type.Scale:
                return Item.ParseV4(Item.ParseScale(anim.newValue + (anim.relative ? anim.startValue : Vector4.zero)));                

            case Type.Move:
                return new Vector4(anim.newValue.x, anim.newValue.y, anim.useDepth ? anim.newValue.z : GetCurrentValue(target, anim.type).z, 0) + (anim.relative ? anim.startValue : Vector4.zero);

            case Type.Spin:
                return Item.ParseV4(Item.ParseSpin(anim.newValue)) + (anim.relative ? anim.startValue : Vector4.zero);

            case Type.Alpha:
                return Item.ParseV4(anim.newValue.x);

            case Type.Size:
                return anim.newValue;

            default:
                return Vector4.zero;
        }
    }

    private Tweener CreateTween(IPSYTweenable target, object value, PSYAnimation.Type type, float time)
    {
        if (target.IsGraphic)
        {
            switch (type)
            {
                case Type.Color:
                    return DOTween.To(() => target.Graphic.color, x => target.Graphic.color = x, (Color)value, time);

                case Type.Alpha:
                    CanvasGroup gr = target.Graphic.GetComponent<CanvasGroup>();
                    if (gr != null)
                    {
                        return DOTween.To(() => gr.alpha, x => gr.alpha = x, (float)value, time);
                    }
                    else
                    {
                        return DOTween.To(() => target.Graphic.color.a, x => target.Graphic.color = target.Graphic.color.SetAlpha(x), (float)value, time);
                    }

                case Type.Size:
                    return DOTween.To(() => target.Rect.sizeDelta, x => target.Rect.sizeDelta = x, (Vector2)value, time);

                case Type.Move:
                    return DOTween.To(() => target.Rect.anchoredPosition3D, x => target.Rect.anchoredPosition3D = x, (Vector3)value, time);

                case Type.Scale:
                    return DOTween.To(() => target.Rect.localScale, x => target.Rect.localScale = x, (Vector3)value, time);

                case Type.Spin:
                    return target.Rect.DOLocalRotate((Vector3)value, time, RotateMode.FastBeyond360);
            }
        }
        else try
            {
                switch (type)
                {
                    case Type.Alpha:
                        return DOTween.To(() => target.Group.alpha, x => target.Group.alpha = x, (float)value, time).OnStepComplete(()=> { if(target.Group == null) PSYDebug.Error(PSYDebug.Source.Animation, "Error creating animation of type {0} at {1} {2}", type, view != null ? view.FullName : "VIEW == NULL", target.DebugName); });

                    case Type.Move:
                        return DOTween.To(() => target.Rect.anchoredPosition3D, x => target.Rect.anchoredPosition3D = x, (Vector3)value, time);

                    case Type.Scale:
                        return DOTween.To(() => target.Rect.localScale, x => target.Rect.localScale = x, (Vector3)value, time);

                    case Type.Spin:
                        return target.Rect.DOLocalRotate((Vector3)value, time, RotateMode.FastBeyond360);
                }
            }
            catch (Exception)
            {
                PSYDebug.Error(PSYDebug.Source.Animation, "Error creating animation of type {0} at {1} {2}", type, view != null ? view.FullName : "VIEW == NULL", target.DebugName);
            }
        return null;
    }
}