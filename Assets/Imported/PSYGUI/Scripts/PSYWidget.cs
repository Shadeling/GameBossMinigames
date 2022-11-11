using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PSYWidget
{
    public Text                 Text = null;

    public Image                Image = null;

    public RectTransform        Rect = null;

    public string               name = "";
    private object              value = null;
    private string              key = "";
    public string Key
    {
        get
        {
            if (string.IsNullOrEmpty(key))
            {
                key = Text.text;
            }
            return key;
        }

        private set
        {
            key = value;
        }
    }

    public float fill
	{
		get
		{
            return !isImage || Image.type != Image.Type.Filled ? 0 : Image.fillAmount;
		}
	}
    
	public string text
    {
        get
        {
            return isText ? Text.text : "component is not of type Text";
        }
        set
        {
            this.value = value;
            SetText();
        }
    }

	public float alpha
    {
        get
        {
            return isText ? Text.color.a : Image.color.a;
        }
        set
        {
            SetAlpha(value);
        }
    }

	public Color color
    {
        get
        {
            return isText ? Text.color : Image.color;
        }
        set
        {
            SetColor(value);
        }
    }

	public Vector3 position
    {
        get
        {
            return Rect.anchoredPosition3D;
        }
        set
        {
            SetPos(value);
        }
    }

	public Vector3 scale
    {
        get
        {
            return Rect.localScale;
        }
        set
        {
            SetScale(value);
        }
    }

	public Vector2 size
    {
        get
        {
            return Rect.sizeDelta;
        }
        set
        {
            SetSize(value);
        }
    }

	public Vector3 rotation
    {
        get
        {
            return Transform.eulerAngles;
        }
        set
        {
            SetRot(value);
        }
    }



	public Graphic Widget
    {
        get
        {
            return isText ? (Graphic)Text : (Graphic)Image;
        }
    }

    private Transform transform = null;
    public Transform Transform
    {
        get
        {
            if(transform == null)
            {
                transform = Widget.transform;
            }
            return transform;
        }
    }    	

    public bool isImage 
	{ 
		get 
		{ 
			return Image != null; 
		} 
	}
	
	public bool isText
    {
        get
        {
            return Text != null;
        }
    }

    public PSYWidget(Image image)
    {
        Image = image;
        name = Widget.name;
        Rect = image.rectTransform;
    }

    public PSYWidget(Text text)
    {
        Text = text;
        name = Widget.name;        
        Rect = text.rectTransform;
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
    public void Set(object value, bool setNativeSize = false)
    {        
        if (value == null)
        {
            return;
        }
        if (value is Color)
        {
            SetColor((Color)value);
        }
        else if (value is Vector2)
        {
            SetSize((Vector2)value);
        }
        else
        {
            this.value = value;

            if (isText)
            {
                SetText();
            }
            else
            {
                if (Image.type == Image.Type.Filled && !(value is string))
                {
                    SetFill();
                }
                else
                {
                    SetImage(setNativeSize);
                }
            }
        }

        return;
    }
    
    public void SetLocalisedText(string text)
    {
        if (!isText)
        {
            PSYDebug.Error(PSYDebug.Source.Widget, "SetText: component should be of type Text ({0})", Transform.name);
            return;
        }

        Text.text = text;
    }


    private void SetImage(bool setNativeSize)
    {
        if (!isImage)
        {
            PSYDebug.Log(PSYDebug.Source.Widget, "SetImage: component should be of type Image({0})", Transform.name);
            return;
        }

        if (value is Sprite)
        {
            if (Application.platform == RuntimePlatform.WSAPlayerARM && ((Sprite)value).name == "Btn_Back")
            {
                PSYGUI.Debug("BACKBUTTON2", "+");
            }
            Image.sprite = (Sprite)value;
        }
        else if (value is Texture2D)
        {
            Image.sprite = Sprite.Create((Texture2D)value, new Rect(0, 0, ((Texture2D)value).width, ((Texture2D)value).height), Vector2.zero);
        }
        else if (value is byte[])
        {
            Texture2D tex = new Texture2D(10, 10);
            tex.LoadImage((byte[])value);
            Image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }
        else if (value is string)
        {
            Image.sprite = Resources.Load<Sprite>(PSYGUI.texturesPath + "/" + value);
        }

        if (setNativeSize)
        {
            Image.SetNativeSize();
        }
    }

    public void Hide()
	{
        alpha = 0;
    }

	public void Show()
	{
        alpha = 1;
	}	

	private void SetText()
	{
		if (!isText)
		{
			PSYDebug.Error(PSYDebug.Source.Widget, "SetText: component should be of type Text ({0})", Transform.name);
			return;
		}

        string before = "";
        string after = "";
        string activeVal = (value != null ? value : "").ToString();

        Key = (activeVal != null ? activeVal : "").ToString();
        //Text.text = before + PSYGUI.localizer.Localize(activeVal != null ? activeVal : "").Trim() + after;
	}	

    private string GetBordered(string val, string sep, out string before, out string after)
    {
        if(!val.Contains(sep))
        {
            before = "";
            after = "";
            return val;
        }

        int index = val.IndexOf(sep);
        before = val.Substring(0, index);
        val = val.Substring(index + 1);
        after = val.Substring(val.IndexOf(sep) + 1);
        return val.Substring(0, val.IndexOf(sep));
    }

	private void SetFill()
	{
		if (!isImage || Image.type != Image.Type.Filled)
		{
			PSYDebug.Error(PSYDebug.Source.Widget, "SetFill: no component of type 'Image' found, or it's type is not 'Filled' ({0})", Transform.name);
			return;
		}

		Image.fillAmount = value is float ? (float)value : (float)(Convert.ToDouble(value) / 100);
	}				

	public void SetFont(Font font)
	{
		if (isText)
			Text.font = font;
		else
			PSYDebug.Error(PSYDebug.Source.Widget, "SetFont: component should be of type Text ({0})", Transform.name);		
	}

	public void SetColor(Color color, bool withAlpha = true)
	{
		Color col = isText ? Text.color : Image.color;
		if (withAlpha)
			col = color;
		else
			col = new Color(color.r, color.g, color.b, col.a);
		if (isText)
            Text.color = col;
		else
            Image.color = col;
	}	

	public void SetAlpha(float alpha)
	{
		Color color = isText ? Text.color : Image.color;
		color.a = alpha;
		if (isText)
            Text.color = color;
		else
            Image.color = color;
	}

	public void SetScale(Vector2 scale, bool relative = false)
	{
		Vector2 newScale = relative ? ((Vector2)Rect.localScale + scale) : scale;
		newScale = new Vector2(Mathf.Max(0, newScale.x), Mathf.Max(0, newScale.y));
		Rect.localScale = new Vector3(newScale.x, newScale.y, 1);
	}

	public void SetSize(Vector2 size, bool relative = false)
	{
		Vector2 newSize = relative ? ((Vector2)Rect.sizeDelta + size) : size;
		newSize = new Vector2(Mathf.Max(0, newSize.x), Mathf.Max(0, newSize.y));
        Rect.sizeDelta = newSize;
	}

	public void SetPos(Vector3 pos, bool relative = false)
	{
		Vector2 newPos = relative ? Rect.anchoredPosition3D + pos : pos;
        Rect.anchoredPosition3D = newPos;		
	}

    public void SetPosX(float newX, bool relative = false)
    {
        Vector2 pos = Rect.anchoredPosition;
        pos.x = relative ? pos.x + newX : newX;
        Rect.anchoredPosition = pos;
    }

    public void SetPosY(float newY, bool relative = false)
    {
        Vector2 pos = Rect.anchoredPosition;
        pos.y = relative ? pos.y + newY : newY;
        Rect.anchoredPosition = pos;
    }

	public void SetRot(Vector3 rot, bool relative = false)
	{
		Vector3 newRot = relative ? Transform.eulerAngles + rot : rot;
		Transform.localRotation = Quaternion.Euler(newRot);
	}

	public object GetFont(Component c)
	{
		if (!isText)
		{
			PSYDebug.Error(PSYDebug.Source.Widget, "GetFont: component should be of type Text ({0})", c.transform.name);
			return null;
		}

		return Text.font;
	}

    public void SetActiveChildren(bool val)
    {
        for (int j = 0; j < Transform.childCount; j++)
        {
            Transform.GetChild(j).gameObject.SetActive(val);
        }
    }
}
