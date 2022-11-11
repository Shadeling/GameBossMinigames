using UnityEngine;
using System;
using XD;
using System.Collections.Generic;
using System.Linq;
using Zenject;

public class ARGUETools : MonoBehaviour, IPSYEventHandler
{
    private static ARGUETools               instance = null;
    private string                          lastResourcesPath = "";

    [SerializeField]
    private string                          spritesCommonPath = "";


    [SerializeField]
    private List<Sprite>                    dynamicResourcesSprites = new List<Sprite>();

    private Dictionary<string, Sprite>      resourcesDict = new Dictionary<string, Sprite>();

    /*[Inject] private IInput m_input;
    [Inject] private IUI m_ui;*/


    private void Awake()
    {
        instance = this;

        FillSpritesDict(spritesCommonPath, false);

        PSYGUI.SetAgent(this);
        PSYManager.AddSubscriber(this);
    }

    private void FillSpritesDict(string path, bool saveDynamicList)
    {
        if (path == lastResourcesPath)
        {
            PSYDebug.Log(PSYDebug.Source.Core, "<color=magenta> Loading sprites canceled, the same path: {0}</color>", path);
            return;
        }

        List<Sprite> list = Resources.Load<ARResourceSprites>("Entities/UI/" + path).ResourcesSprites;

        PSYDebug.Log(PSYDebug.Source.Core, "<color=magenta> Loading sprites from Entities/UI/{0}: {1}</color>", path, list.Count);
        
        if (saveDynamicList && path != lastResourcesPath)
        {
            lastResourcesPath = path;
            int was = resourcesDict.Count;
            for (int i = 0; i < dynamicResourcesSprites.Count; i++)
            {
                if (dynamicResourcesSprites[i] == null)
                {
                    PSYDebug.Error(PSYDebug.Source.Temp, "{0} {1} {2}", path, i, i > 0 ? dynamicResourcesSprites[i - 1].name : "");
                }
                resourcesDict.Remove(dynamicResourcesSprites[i].name);
            }
            PSYDebug.Log(PSYDebug.Source.Core, "Changing dynamic list: {0} removed, {1} will be added", was - resourcesDict.Count, list.Count);
            dynamicResourcesSprites = list;
        }

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null)
            {
                PSYDebug.Error(PSYDebug.Source.Core, "SPRITE MISSING AT {0}", i);
                continue;
            }

            if (resourcesDict.ContainsKey(list[i].name))
            {
                PSYDebug.Error(PSYDebug.Source.Core, "SPRITE DUPLICATE {0} Number In Array {1}", list[i].name, i);
                continue;
            }

            resourcesDict.Add(list[i].name, list[i]);
        }

        PSYDebug.Log(PSYDebug.Source.Core, "TOTAL DYNAMIC SPRITES: {0}", resourcesDict.Count);
    }

    public string Description
    {
        get
        {
            return "ARGUETools";
        }
    }


    public void PSYEventHandler(PSYEvent e, PSYParams param)
    {
        switch (e)
        {

            case PSYEvent.WindowOpened:
                /*switch(param.Get<PSYWindow>())
                {
                    case PSYWindow.None:
                        EnableCursor();
                        break;
                }*/
                break;

            case PSYEvent.WindowClosed:
                /*switch (param.Get<PSYWindow>())
                {
                    case PSYWindow.None:
                        EnableCursor(false);
                        break;
                }*/
                break;
        }
    }

    public static void EnableCursor(bool enable = true)
    {
        /*if (instance.m_input.IsMobile && !Application.isEditor)
        {
            return;
        }

        if (ARGUEManager.IsGamepad)
        {
            enable = false;
        }*/

            

        PSYDebug.Log(PSYDebug.Source.Temp, "EnableCursor {0}", enable);

        //instance.m_input.LockCursor(!enable);
        //PSYGUI.Event(PSYEvent.CursorEnabled, PSYParams.New(Cursor.visible, Cursor.lockState));
    }

    public static Sprite LoadSprite(string subfolder, string name, out bool success)
    {        
        Sprite sprite = LoadSprite(name);
        success = sprite != null;

        return sprite;
    }

    public static Sprite LoadResourceSprite(string subfolder, string name)
    {
        if (name == null)
        {
            Debug.LogError("LoadSprite :: Name is null!");
            return null;
        }

        //PSYDebug.Log(PSYDebug.Source.Temp, "LoadResourceSprite {0}", "UI/Textures/" + name);
        Sprite sprite = Resources.Load<Sprite>("UI/Textures/" + name);
        if (sprite == null)
        {
            PSYDebug.Warning(PSYDebug.Source.Temp, "Missing sprite {0}", name);
            return null;
        }

        return sprite;
    }

    public static Sprite LoadSprite(string subfolder, string name)
    {
        return LoadSprite(name);
    }    

    public static Sprite LoadSprite(string name, bool showMissing = true)
    {
        if (name == null)
        {
            PSYDebug.Warning(PSYDebug.Source.Temp, "Sprite is null");
            return null;
        }

        Sprite sprite = null;
        if(!instance.resourcesDict.TryGetValue(name, out sprite)) 
        {
            PSYDebug.Warning(PSYDebug.Source.Temp, "Missing sprite {0}", name);
            instance.resourcesDict.TryGetValue(showMissing ? "Missing" : "Empty", out sprite);
        }

        return sprite;
    }

    public static bool TryGetSprite(string name, out Sprite sprite, bool showMissing = true)
    {
        sprite = null;

        if (name == null)
        {
            Debug.LogError("LoadSprite :: Name is null!");
            return false;
        }

        if (!instance.resourcesDict.TryGetValue(name, out sprite))
        {
            PSYDebug.Warning(PSYDebug.Source.Temp, "Missing sprite {0}", name);
            instance.resourcesDict.TryGetValue(showMissing ? "Missing" : "Empty", out sprite);
            return false;
        }

        return true;
    }

    public static Sprite EmptySprite
    {
        get { return LoadSprite("Common", "Empty"); }
    }


    public static string CurrencyFormat(decimal val, bool _short = false)
    {
        //PSYDebug.Log(PSYDebug.Source.Temp, "CurrencyFormat {0} {1}", val, _short);
        return CurrencyFormat((long)val, _short);
    }

    public static string CurrencyFormat(long val, bool _short = false)
    {
        //PSYDebug.Log(PSYDebug.Source.Temp, "CurrencyFormat {0} {1}", val, _short);
        string str = val.ToString();

        if(_short)
        {
            if (str.Length < 4)
                return str;

            long thousands = val * 10 / 1000;
            string ths = thousands.ToString();

            if (ths[ths.Length - 1] == '0')
                return ths.Substring(0, ths.Length - 1) + "k";

            return ths.Insert(ths.Length - 1, ".") + "k";
        }

        int before = str.Length % 3;
        string res = str.Substring(0, before);
        for (int i = 0; i < str.Length / 3; i++)
            res += (before == 0 && i == 0 ? "" : " ") + str.Substring(before + 3 * i, 3);
        return res;
    }


    public static void ShowPriceInfo(PSYWidget icon, PSYWidget value, CurrencyValue cv, string prefix = "")
    {
        if (cv.Amount == 0)
        {
            value.SetPosX(0);
        }

        icon.Set(cv.Amount != 0 ? GetPriceIcon(cv.Type) : EmptySprite);
        value.Set(cv.Amount != 0 ? prefix + CurrencyFormat(cv.Amount) : "UI_Free");
    }

    public static void ShowPriceInfo(PSYWidget icon, PSYWidget value, Sprite sprite, string text)
    {
        icon.Set(sprite);
        value.Set(text);
    }

    public static Sprite GetPriceIcon(CurrencyType type)
    {
        return LoadSprite("Icon_" + type);
    }

    public static string GetPriceValue(CurrencyValue val)
    {
        Sprite sprite = null;
        bool found = TryGetSprite("Icon_" + val.StringCurrency, out sprite, false);
        return PSYTools.CurrencyFormat(val.Amount) + (found ? "" : " " + val.StringCurrency);
    }

    public static void PlaySound(int sound)
    {
        if (sound != -1)
        {
            PSYGUI.Event(PSYEvent.SoundRequestUI, sound.ToParam());
        }
    }

    public static string UnixToDate(int seconds)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified).AddSeconds(seconds).ToString("dd/MM/yy");
    }

    /// <summary>
    /// Returns int value rounded to specific int value
    /// </summary>
    public static int GetRoundedValueTo(int value, int roundTo)
    {
        if (roundTo == 0 || value % roundTo == 0)
        {
            return value;
        }

        int result = 0;
        int multiplier = (value / roundTo) + 1;
        result = (value < 0) ? -(roundTo * multiplier) : (roundTo * multiplier);
        return result;
    }


    public static decimal RoundHalfUp(decimal d, int decimals)
    {//округление 1.349999992 до 1.35 при decimals=2

        if (decimals < 0)
        {
            throw new ArgumentException("The decimals must be non-negative",
                "decimals");
        }

        decimal multiplier = (decimal)Math.Pow(10, decimals);
        decimal number = d * multiplier;

        if (decimal.Truncate(number) < number)
        {
            number += 0.5m;
        }
        return decimal.Round(number) / multiplier;
    }
}
