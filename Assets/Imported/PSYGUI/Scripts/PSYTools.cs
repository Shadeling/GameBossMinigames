//author: Dolan D.

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine.UI;
using XD;

public static class PSYTools
{
    public static PSYViewEvent GetInverseViewEvent(PSYViewEvent e)
    {
        switch (e)
        {
            case PSYViewEvent.Open:
                return PSYViewEvent.Close;
            case PSYViewEvent.Close:
                return PSYViewEvent.Open;
            case PSYViewEvent.Select:
                return PSYViewEvent.Deselect;
            case PSYViewEvent.Deselect:
                return PSYViewEvent.Select;
            case PSYViewEvent.Enable:
                return PSYViewEvent.Disable;
            case PSYViewEvent.Disable:
                return PSYViewEvent.Enable;
            case PSYViewEvent.MouseIn:
                return PSYViewEvent.MouseOut;
            case PSYViewEvent.MouseOut:
                return PSYViewEvent.MouseIn;
            default:
                return e;
        }
    }

    public static PSYAnimationEvent GetInverseAnimationEvent(PSYAnimationEvent e)
    {
        switch (e)
        {
            case PSYAnimationEvent.Open:
                return PSYAnimationEvent.Close;
            case PSYAnimationEvent.Close:
                return PSYAnimationEvent.Open;
            case PSYAnimationEvent.Select:
                return PSYAnimationEvent.Deselect;
            case PSYAnimationEvent.Deselect:
                return PSYAnimationEvent.Select;
            case PSYAnimationEvent.Enable:
                return PSYAnimationEvent.Disable;
            case PSYAnimationEvent.Disable:
                return PSYAnimationEvent.Enable;
            case PSYAnimationEvent.MouseIn:
                return PSYAnimationEvent.MouseOut;
            case PSYAnimationEvent.MouseOut:
                return PSYAnimationEvent.MouseIn;
            default:
                return e;
        }
    }

    

    /// <summary>
    /// Getting list of all widgets within target transform that are not within another view
    /// </summary>
    /// <param name="transform">Parent transform to do search in</param>
    public static List<PSYWidget> GetFreeWidgets(Transform transform, List<PSYWidget> list)
    {
        if (list == null)
        {
            list = new List<PSYWidget>();
        }

        foreach (Transform child in transform)
        {
            if (child.gameObject.GetComponent<PSYView>() != null)
            {
                continue;
            }

            Image image = child.gameObject.GetComponent<Image>();
            if (image != null)
            {
                list.Add(new PSYWidget(image));
            }
            else
            {
                Text text = child.gameObject.GetComponent<Text>();
                if (text != null)
                {
                    list.Add(new PSYWidget(text));
                }
            }

            GetFreeWidgets(child, list);
        }

        return list;
    }

    //Обновление списков view.Widgets и view.Child у всех виджетов с компонентом PSYView
    //Перезачитывание view.Widgets - списка дочерних виджетов с компонентом Image или Text и view.Children - списка дочерних виджетов с компонентом PSYView
    //Для всех дочерних виджетов view с компонентом PSYView выполняется аналогичная процедура
    public static void UpdateLitsWidgetAndChild(PSYView view)
    {

        if (view.GetComponent<RectTransform>() == null)
        {
            view.gameObject.AddComponent<RectTransform>();
        }


        if (view.state == null)
        {
            view.state = new PSYViewState(view.ID, false, view.isEnabledOnStart, false);
        }

        if (view.Widgets == null)
        {
            view.Widgets = new List<PSYWidget>();
        }
        else
        {
            view.Widgets.Clear();
            view.Children.Clear();
        }

        UpdateLitsWidgetAndChild(view.transform, view.Widgets, view);

        if (view.type != PSYViewType.Window)
        {
            view.Parent = PSYTools.GetParentObject<PSYView>(view.transform);
            //PSYDebug.Warning(PSYDebug.Source.Editor, "Updated view parent of {0} {1}", view.name, view.Parent.gameObject.name);
        }

        PSYController cont = view.GetComponent<PSYController>();
        if (cont != null && cont.view.type != PSYViewType.Window)
        {
            cont.Parent = PSYTools.GetParentObject<PSYController>(view.transform);
            //PSYDebug.Warning(PSYDebug.Source.Editor, "Updated cont parent of {0} {1}", view.name, cont.Parent.gameObject.name);
        }

    }

    public static void UpdateLitsWidgetAndChild(Transform transform, List<PSYWidget> list, PSYView viewValue)
    {
        if (list == null)
        {
            list = new List<PSYWidget>();
        }

        foreach (Transform child in transform)
        {
            if (child.gameObject.GetComponent<PSYView>() != null)
            {

                PSYView viewChild = child.gameObject.GetComponent<PSYView>();

                viewValue.RegisterChildTest(viewChild);
                //UpdateLitsWidgetAndChild(child, list, viewTmp);
                UpdateLitsWidgetAndChild(viewChild);
                continue;
            }

            Image image = child.gameObject.GetComponent<Image>();
            if (image != null)
            {
                list.Add(new PSYWidget(image));
            }
            else
            {
                Text text = child.gameObject.GetComponent<Text>();
                if (text != null)
                {
                    list.Add(new PSYWidget(text));
                }
            }

            UpdateLitsWidgetAndChild(child, list, viewValue);
        }

        //return list;
    }

    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        var result = aParent.Find(aName);
        if (result != null)
            return result;
        foreach (Transform child in aParent)
        {
            result = child.FindDeepChild(aName);
            if (result != null)
                return result;
        }
        return null;
    }

    /// <summary> 
    /// Getting list of all child objects within target transform
    /// </summary>
    /// <param name="transform">Target transform to search in</param>
    /// <param name="allOfThem">Whether include children's children</param>	
    public static void GetChildObjects<T>(Transform transform, List<T> list, bool allOfThem = false) where T : Component
    {
        //PSYDebug.Log(PSYDebug.Source.Temp, "search in {0}", t.name);
        foreach (Transform child in transform)
        {
            //PSYDebug.Log(PSYDebug.Source.Temp, "found {0}", tt.name);
            T component = child.gameObject.GetComponent<T>();
            if (component != null)
            {
                list.Add(component);
                if (allOfThem)
                {
                    GetChildObjects(child, list, allOfThem);
                }
            }
            else
            {
                GetChildObjects(child, list, allOfThem);
            }
        }
    }

    /// <summary> 
    /// Getting list of all child objects within target transform
    /// </summary>
    /// <param name="transform">Target transform to search in</param>
    /// <param name="allOfThem">Whether include children's children</param>	
    public static T GetParentObject<T>(Transform transform) where T : Component
    {
        Transform parent = transform;
        T res = null;
        while (parent.parent != null)
        {
            parent = parent.parent;
            res = parent.gameObject.GetComponent<T>();
            if (res != null)
            {
                return res;
            }
        }

        return null;
    }

    public static string ToStringF(this IDictionary origin)
    {
        string res = "";
        foreach (var v in origin.Keys)
            res += v.ToString() + " ";
        foreach (var v in origin.Values)
            res += v.ToString() + " ";
        return res;
    }

    public static string ToStringF(this object obj, bool includeProperties)
    {
        if (obj == null)
            return "Object is null";
#if NETFX_CORE
        if (obj.GetType().GetTypeInfo().IsGenericType)
#else
        if (obj.GetType().IsGenericType)
#endif
            return ((IList)obj).ToStringF();
        if (obj.GetType().GetFields().Length == 0)
            return obj.ToString();
        string str = obj.GetType().ToString() + "\n{";
        foreach (FieldInfo member in obj.GetType().GetFields())
            if (member.GetValue(obj) != null)
            {
#if NETFX_CORE
                if (member.FieldType.GetTypeInfo().IsGenericType)
#else
                if (member.FieldType.IsGenericType)
#endif
                    str += "\n   " + member.Name.ToString() + ": " + member.GetValue(obj).ToStringF(false);
                else
                    str += "\n   " + member.Name.ToString() + ": " + member.GetValue(obj);
            }
        if (includeProperties)
            foreach (PropertyInfo member in obj.GetType().GetProperties())
                if (member.GetValue(obj, null) != null)
                {
#if NETFX_CORE
                if (member.PropertyType.GetTypeInfo().IsGenericType)
#else
                    if (member.PropertyType.IsGenericType)
#endif
                        str += "\n    " + member.Name.ToString() + ": " + member.GetValue(obj, null).ToStringF(false);
                    else
                        str += "\n    " + member.Name.ToString() + ": " + member.GetValue(obj, null);
                }
        return str + "\n}\n";
    }

    public static string ToStringF(this IList list)
    {
        string str = "List(" + list.Count + "): {";
        foreach (object item in list)
            str += " " + (item == null ? "NULL" : item.ToString());
        return str + "}";
    }
    
    public static string FirstLetterToUpper(this string input)
    {
        switch (input)
        {
            case null: throw new ArgumentNullException(nameof(input));
            case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
            default: return input.First().ToString().ToUpper() + input.Substring(1);
        }
    }
    
    public static void Swap<T>(this List<T> list, int first, int second)
    {
        var item = list[second];
        list[second] = list[first];
        list[first] = item;
    }

    // public static T1 GetKeyByValue<T1, T2>(this Dictionary<T1, T2> dict, T2 value)
    // {
    //     foreach (var pair in dict)
    //         if (pair.Value.Equals(value))
    //             return pair.Key;
    //     return default(T1);
    // }

    /// <summary>
    /// Returns one of the coordinates of Vector2
    /// </summary>
    /// <param name="?">Target vector</param>
    /// <param name="x">If true, 'x' value of vector 2 will be returned. </param>
    /// <returns></returns>
    public static float GetVector2Part(this Vector2 v, bool x)
    {
        return x ? v.x : v.y;
    }

    public static string[] EnumToStringArray(Type enumType)
    {
        if (enumType == null)
            return new string[0];
        string[] arr = new string[Enum.GetValues(enumType).Length];
        for (int i = 0; i < Enum.GetValues(enumType).Length; i++)
            arr[i] = Enum.GetValues(enumType).GetValue(i).ToString();
        return arr;
    }

    public static List<string> EnumToStringList(Type enumType)
    {
        List<string> list = new List<string>();
        if (enumType != null)
            for (int i = 0; i < Enum.GetValues(enumType).Length; i++)
                list.Add(Enum.GetValues(enumType).GetValue(i).ToString());
        return list;
    }

    public static List<T> EnumToObjectList<T>(Type enumType)
    {
        List<T> list = new List<T>();
        if (enumType != null)
            for (int i = 0; i < Enum.GetValues(enumType).Length; i++)
                list.Add((T)Enum.GetValues(enumType).GetValue(i));
        return list;
    }

    public static void EnumToObjectList(ref List<object> list, Type enumType)
    {
        list.Clear();
        if (enumType != null)
            for (int i = 0; i < Enum.GetValues(enumType).Length; i++)
                list.Add(Enum.GetValues(enumType).GetValue(i).ToString());
    }

    public static string GetMD5Hash(string input)
    {
        System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
        bs = x.ComputeHash(bs);
        System.Text.StringBuilder s = new System.Text.StringBuilder();
        foreach (byte b in bs)
        {
            s.Append(b.ToString("x2").ToLower());
        }
        string password = s.ToString();
        return password;
    }

    public static List<T> PSYSort<T>(this List<T> source, string fieldName)
    {
        if (typeof(T).GetField(fieldName) == null)
        {
            //PSYDebug.Log(PSYDebug.Source.Filter, "type {0} doesn't contain field with name {1}", typeof(T), fieldName);
            return source;
        }
        if (typeof(T).GetField(fieldName).GetType() != typeof(int))
        {
            //PSYDebug.Log(PSYDebug.Source.Filter, "field with name {1} isn't of type int", fieldName);
            return source;
        }
        List<T> clone = new List<T>(source);
        List<T> list = new List<T>();
        while (clone.Count > 0)
        {
            int min = int.MinValue;
            int index = -1;
            for (int i = 0; i < clone.Count; i++)
            {
                FieldInfo info = clone[i].GetType().GetField(fieldName);
                if (info != null && (int)info.GetValue(clone[i]) < min)
                {
                    min = (int)info.GetValue(clone[i]);
                    index = i;
                }
            }
            if (index == -1)
            {
                list.AddRange(clone);
                return list;
            }
            else
            {
                list.Add(clone[index]);
                clone.RemoveAt(index);
            }
        }
        return source;
    }

    /// <summary>
    /// Merges all keys from addHash into the target. Adds new keys and updates the values of existing keys in target.
    /// </summary>
    /// <param name="target">The IDictionary to update.</param>
    /// <param name="addHash">The IDictionary containing data to merge into target.</param>
    public static void MergeDict(this IDictionary target, IDictionary addHash)
    {
        if (addHash == null || target.Equals(addHash))
        {
            return;
        }

        foreach (object key in addHash.Keys)
        {
            target[key] = addHash[key];
        }
    }

    public static bool RayCastHit(Vector3 input, GameObject go)
    {
        RaycastHit hit;
        return Physics.Raycast(Camera.main.ScreenPointToRay(input), out hit) && hit.collider.gameObject == go;
    }

    public static RaycastHit RayCastHit(Vector3 input, bool ignore = true)
    {
        RaycastHit hit;
        if (!ignore)
            Physics.Raycast(Camera.main.ScreenPointToRay(input), out hit, Mathf.Infinity, -1);
        else
            Physics.Raycast(Camera.main.ScreenPointToRay(input), out hit, Mathf.Infinity);
        return hit;
    }

    public static string ColorToNGUIColor(Color color)
    {
        return string.Format("[{0:x2}{1:x2}{2:x2}]", Mathf.FloorToInt(color.r * 255f), Mathf.FloorToInt(color.g * 255f), Mathf.FloorToInt(color.b * 255f));
    }

    public static Color GetColor(this Color32 color)
    {
        return new Color(color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f);
    }

    public static T[] ToArray<T>(this List<T> source)
    {
        if (source == null)
            return new T[0];
        T[] arr = new T[source.Count];
        for (int i = 0; i < source.Count; i++)
            arr[i] = source[i];
        return arr;
    }

    public static T[] PSYClone<T>(this T[] source)
    {
        if (source == null)
            return new T[0];
        T[] arr = new T[source.Length];
        for (int i = 0; i < source.Length; i++)
            arr[i] = source[i];
        return arr;
    }

    public static List<T> ToList<T>(this T[] source)
    {
        if (source == null)
            return new List<T>();
        List<T> list = new List<T>();
        for (int i = 0; i < source.Length; i++)
            list.Add(source[i]);
        return list;
    }

    public static List<T> PSYClone<T>(this List<T> source)
    {
        if (source == null)
            return new List<T>();
        List<T> list = new List<T>();
        for (int i = 0; i < source.Count; i++)
            list.Add(source[i]);
        return list;
    }

    public static List<T> SubList<T>(this List<T> source, int fromIndex = 0, int count = 0)
    {
        if (source == null)
            return new List<T>();
        List<T> list = new List<T>();
        for (int i = 0; i < source.Count; i++)
        {
            if (i >= fromIndex && (count == 0 || i - fromIndex <= count - 1))
                list.Add(source[i]);
        }
        return list;
    }

    public static IList SubList<T>(this IList source, int fromIndex = 0, int count = 0)
    {
        if (source == null)
            return new List<T>();
        IList list = new List<T>();
        for (int i = 0; i < source.Count; i++)
        {
            if (i >= fromIndex && (count == 0 || i - fromIndex <= count - 1))
                list.Add(source[i]);
        }
        return list;
    }

    public static int Value10Degree(float value)
    {
        int res = 0;
        while (value > 10)
        {
            res++;
            value = value / 10;
        }
        return res;
    }

    public static bool Enamable<T>(this object value)
    {
        try
        {
            Enum.Parse(typeof(T), value.ToString());
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static T ToEnum<T>(this object value)
    {
        return (T)Enum.Parse(typeof(T), value.ToString());
    }

    public static List<T> ListFromEnum<T>()
    {
        List<T> list = new List<T>();
        foreach (T t in Enum.GetValues(typeof(T)))
            list.Add(t);
        return list;
    }

    public static List<T> AddEnumValues<T>(this List<T> source)
    {
        foreach (T t in Enum.GetValues(typeof(T)))
            if (!source.Contains(t))
                source.Add(t);
        return source;
    }

    public static T Rnd<T>(this List<T> list)
    {
        InitRandom();
        return list[rnd.Next(list.Count)];
    }

    public static int Rnd(this int val, bool bothSignes = false)
    {
        InitRandom();
        return rnd.Next(val) * (bothSignes ? (rnd.Next(2) == 0 ? -1 : 1) : 1);
    }

    public static int GetRandom(int from, int to)
    {
        return from + GetRandom(to - from);
    }

    public static int GetRandom(int val, bool bothSignes = false)
    {
        InitRandom();
        return rnd.Next(val) * (bothSignes ? (rnd.Next(2) == 0 ? -1 : 1) : 1);
    }

    public static int GetNegative_50x50(int val)
    {
        if (_50x50())
            return -val;
        else
            return val;
    }

    public static float GetRandomF(float val, bool bothSignes = false)
    {
        InitRandom();
        return (float)rnd.NextDouble() * val * (bothSignes ? (rnd.Next(2) == 0 ? -1 : 1) : 1);
    }

    public static T GetAtRandom<T>(this List<T> list, int minIndex = 0, int maxIndex = int.MaxValue, bool remove = false)
    {
        if (list.Count == 0)
            return default(T);
        if (minIndex > list.Count || maxIndex < 0)
            return default(T);
        int realMax = Mathf.Min(maxIndex, list.Count);
        int realMin = Mathf.Max(minIndex, 0);
        realMax = realMax - realMin;
        int rnd = GetRandom(realMax);
        T val = list[realMin + rnd];
        if (remove)
            list.Remove(val);
        return val;
    }

    public static System.Random rnd = null;
    public static void InitRandom()
    {
        if (rnd == null)
            rnd = new System.Random(DateTime.Now.Millisecond);
    }

    /// <summary>
    /// Translates "01001" to List { false, true, false, false, true }
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static List<bool> StringToListOfBool(string source)
    {
        List<bool> list = new List<bool>();
        for (int i = 0; i < source.Length; i++)
            list.Add(source[i] == '1');
        return list;
    }

    public static int EnumSize(Type type)
    {
        return Enum.GetNames(type).Length;
    }

    public static int EnumMath(Type type, int enumValue, int shiftValue)
    {
        int length = Enum.GetValues(type).Length;
        int newValue = enumValue + shiftValue;
        if (newValue < 0)
        {
            return length - 1;
        }
        else if (newValue >= length)
        {
            return 0;
        }
        else
        {
            return newValue;
        }
    }

    public static T EnumMath<T>(int enumValue, int shiftValue)
    {
        int length = Enum.GetValues(typeof(T)).Length;
        int newValue = enumValue + shiftValue;
        if (newValue < 0)
        {
            return (T)Enum.GetValues(typeof(T)).GetValue(length - 1);
        }
        else if (newValue >= length)
        {
            return (T)Enum.GetValues(typeof(T)).GetValue(0);
        }
        else
        {
            return (T)Enum.GetValues(typeof(T)).GetValue(newValue);
        }
    }

    public static Vector3 GetV3(this Vector2 v, int z = 0)
    {
        return new Vector3(v.x, v.y, z);
    }

    public static string ToDottedString(this int value)
    {
        string res = "";
        string valueS = value.ToString();
        if (valueS.Length < 4)
            return value.ToString();
        int first = valueS.Length % 3;
        for (int i = 0; i < first; i++)
            res += valueS[i];
        if (first != 0)
            res += ".";
        for (int i = 0; i < valueS.Length - first; i++)
            res += valueS[first + i] + (i + 1 == 3 && i != valueS.Length - first - 1 ? "." : "");
        return res;
    }

    public static bool _50x50()
    {
        return GetRandom(2) == 0;
    }

    public static List<PSYParams> GetPSYParamsList<T>(List<T> source)
    {
        List<PSYParams> list = new List<PSYParams>();
        for (int i = 0; i < source.Count; i++)
            list.Add(PSYParams.New(source[i]));
        return list;
    }

    public static Vector2 Round(this Vector2 v2)
    {
        return new Vector2(Mathf.RoundToInt(v2.x), Mathf.RoundToInt(v2.y));
    }

    public static Vector3 Round(this Vector3 v3)
    {
        return new Vector3(Mathf.RoundToInt(v3.x), Mathf.RoundToInt(v3.y), Mathf.RoundToInt(v3.z));
    }

    public static Vector2 Clamp(this Vector2 v2)
    {
        PSYDebug.Log(PSYDebug.Source.Temp, "Clamp {0} {1}", v2.x, (int)v2.x);
        return new Vector2((int)v2.x, (int)v2.y);
    }

    public static Vector3 Clamp(this Vector3 v3)
    {
        return new Vector3((int)v3.x, (int)v3.y, (int)v3.z);
    }

    /// <summary>
    /// Adds object to PSYParams
    /// </summary>
    public static PSYParams ToParam(this object obj)
    {
        return PSYParams.New(obj);
    }

    /// <summary>
    /// Returns n senior time periods, which can be substracted from incoming value
    /// </summary>
    /// <param name="value">Incoming seconds value</param>
    /// <param name="periodsCount">What to return from 0 to 4. 2 = "2h 45m", 3 = 3d 2h 12m</param>
    /// <param name="shortening">"days" or "d"</param>
    /// <param name="showZeros">"0h 34m 12s" or "34m 12s"</param>
    /// <param name="secondsOnly"> Display value as seconds</param>
    /// <returns></returns>
    

    
    public static float GetCenteredItemsPosition(int itemsCount, int itemIndex, float itemSize, float itemSpacing)
    {
        float res = 0;
        float startPos = 0;
        
        float itemWidth = itemSize + itemSpacing;
        if (itemsCount % 2 == 0)
        {
            startPos = -itemWidth / 2 - itemWidth * (itemsCount / 2 - 1);
            res = startPos + itemWidth * itemIndex;
        }
        else
        {
            startPos = -itemWidth * (itemsCount / 2);
            res = startPos + itemWidth * itemIndex;
        }
        return res;
    }
    
    public static float GetCenteredItemsPosition(int itemsCount, int itemIndex, float itemSize, float itemSpacing, float startPos, int offsetDir = 0)
    {
        float itemWidth = itemSize + itemSpacing;
        float res = 0;
        float offsetDelta = offsetDir != 0 ? 1 * Math.Sign(offsetDir) * (itemsCount - 1) * itemWidth / 2 : 0f;

        if (itemIndex == 0)
        {
            return startPos;
        }

        //if (itemsCount%2 == 0)
        //{
        //    startPos -= itemWidth / 2 - itemWidth * (itemsCount / 2 - 1);
        //}
        //else
        //{
        //    startPos -= itemWidth * (itemsCount / 2);
        //}

        res = startPos + itemWidth * itemIndex;
        return res /*+ offsetDelta*/;
    }

    public static string InsertSeparator(this string price, string separator = " ")
    {
        List<string> threes = new List<string>();
            
        GetThrees(price, ref threes);
        price = "";
            
        for (int i = 0; i < threes.Count; i++)
        {            
            price = (i == threes.Count - 1 ? "" : separator) + threes[i] + price;
        }
        return price;
    }
    
    /// <summary>
    /// Дробит число на классы (единицы, тысячи, миллионы) и возвращает их в обратном порядке
    /// </summary>
    public static void GetThrees(this string value, ref List<string> threes)
    {
        threes.Clear();
        do
        {
            threes.Add(value.Substring(Mathf.Max(0, value.Length - 3), Mathf.Min(3, value.Length)));
            value = value.Substring(0, Mathf.Max(0, value.Length - 3));
        }
        while (value.Length > 0);
    }

    public static string CurrencyFormat(decimal val, bool _short = false)
    {
        //PSYDebug.Log(PSYDebug.Source.Temp, "CurrencyFormat {0}", val);
        return CurrencyFormat((long)val, _short);
    }

    public static string CurrencyFormat(long val, bool _short = false)
    {
        //PSYDebug.Log(PSYDebug.Source.Temp, "CurrencyFormat {0}", val);

        string str = val.ToString();

        if (_short)
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

    public static string GetRomanNumber(int val)
    {
        string res = "";

        if (val < 4)
        {
            for (int i = 0; i < val; i++)
                res += "I";
        }

        else if (val == 4)
            return "IV";

        else if (val == 5)
            return "V";

        else if (val < 9)
        {
            res = "V";
            for (int i = 5; i < val; i++)
                res += "I";
        }

        else if (val < 10)
            res = "IX";

        else if (val < 14)
        {
            res = "X";
            for (int i = 10; i < val; i++)
                res += "I";
        }

        return res;
    }

    public static string ColorToString(Color color)
    {
        return string.Format("<color=#{0:x2}{1:x2}{2:x2}>", Mathf.FloorToInt(color.r * 255f), Mathf.FloorToInt(color.g * 255f), Mathf.FloorToInt(color.b * 255f));
    }

    /// <summary>
    /// Coloring numeral characters
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string ColorText(string text, Color color)
    {
        string newText = "";
        bool numeral = false;
        int insideTag = 0;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            switch (c)
            {
                case '<':
                    insideTag++;
                    break;

                case '>':
                    insideTag--;
                    break;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    if (!numeral && insideTag == 0)
                    {
                        newText += ColorToString(color);
                        numeral = true;
                    }
                    break;

                default:
                    if (numeral && insideTag == 0 && c != ',' && c != '.')
                    {
                        newText += "</color>";
                        numeral = false;
                    }
                    break;
            }

            newText += c;
        }

        if (numeral)
        {
            newText += "</color>";
        }

        return newText;
    }
}