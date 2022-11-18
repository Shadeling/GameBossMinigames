using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public static class MiscTools
{
    public static double unixOrigin;
    public static Random random; 

    static MiscTools()
    {
        unixOrigin = ConvertToUnixTimestamp(new DateTime(1971, 1, 1, 0, 0, 0, 0));
        random = new Random();
    }

    public static void QuitGame()
    {
        Debug.LogWarning ("MiscTools.QuitGame()");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit ();
    }

    public static void OpenDirectURL(string url)
    {

#if UNITY_WEBPLAYER || UNITY_WEBGL
        WebTools.OpenURL(url);
#else
        Application.OpenURL (url);
#endif
    }

    public static bool Approximately(float a, float b, float tolerance = 0.000001f)
    {
        return (Mathf.Abs(a - b) < tolerance);
    }

    public static float RandomNext(float min, float max)
    {
	    return (float)(random.NextDouble() * (max - min) + min);
    }
    
    public static int RandomNext(int min, int max)
    {
	    return random.Next(min, max);
    }

    public static long Round (decimal value, int round)
    {
        long frac = (long)value % round;

        return (frac <= round / 2) 
            ? (long)value - frac 
            : (long)value - frac + round;
    }


    public static int GetRandomIndex(params int[] _probabilities)
	{
		int[] probabilities = new int[_probabilities.Length];

		_probabilities.CopyTo(probabilities, 0);
        
		int sum = 0;

		for (int i = 0; i < probabilities.Length; i++)
		{
			sum += probabilities[i];
			probabilities[i] = sum;
		}

	    if (sum == 0)
	        return probabilities.GetLowerBound(0);

        int rnd = random.Next(1, sum);

		for (int i = 0; i < probabilities.Length; i++)
            if (rnd <= probabilities[i])
                return i;

		return probabilities.GetUpperBound(0);
	}

	public static T[] GetRandomFromSeveral<T>(int[] _probabilities, T[] _values, int count = 1)
	{
		int sum = 0;

		int[] probabilities = new int[_probabilities.Length];

		_probabilities.CopyTo(probabilities, 0);

		T[] values = new T[_values.Length];

		_values.CopyTo(values, 0);

		for (int i = 0; i < probabilities.Length; i++)
		{
			sum += probabilities[i];
			probabilities[i] = sum;
		}

		T[] result = new T[count];

		for (int counter = 0; counter < count; counter++)
		{
            int rnd = random.Next(0, sum);

			int i;

			for (i = 0; i < probabilities.Length; i++)
			{
			    if (rnd >= probabilities[i])
                    continue;

			    result[counter] = values[i];

			    break;
			}

		    if (i == probabilities.Length)
		    {
		        int maxProbability = 0;

		        for (int j = 0; j < _probabilities.Length; j++)
                    if (_probabilities[j] > maxProbability)
                        maxProbability = _probabilities[j];

                for (int j = 0; j < _probabilities.Length; j++)
                    if (_probabilities[j] == maxProbability)
                        result[counter] = values[j];
            }
        }

		return result;
	}

	public static void SetObjectsActivity(IEnumerable<GameObject> objects, bool active)
	{
        if (objects == null)
            return;
		foreach (var go in objects)
		{
		    if (go != null && go.activeSelf != active)
                go.SetActive(active);
		}
	}
	
	public static void SetButtonsActivity(bool active, IEnumerable<Button> buttons)
	{
		foreach (Button button in buttons) 
			button.interactable = active;
	}

    public static DateTime TimestampToDate(double timestamp, bool toUTC = false)
    {
        var dtDateTime = new DateTime(1971, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
	    dtDateTime = dtDateTime.AddSeconds(timestamp);
		return toUTC ? dtDateTime : dtDateTime.ToLocalTime();
    }

	public static DateTime ConvertFromUnixTimestamp(double timestamp)
	{
		DateTime origin = new DateTime(1971, 1, 1, 0, 0, 0, 0);
		return origin.AddSeconds(timestamp);
	}

	public static double ConvertToUnixTimestamp(DateTime date)
	{
		DateTime origin = new DateTime(1971, 1, 1, 0, 0, 0, 0);
		TimeSpan diff = date.ToUniversalTime() - origin;
		return Math.Floor(diff.TotalSeconds);
	}

	public static string CheckIfNull(object obj, string name)
	{
		return String.Format("{0} {1}", name, obj == null ? "is NULL" : "is NOT NULL");
	}

	public static bool isErrorImage(Texture tex) {
		//The "?" image that Unity returns for an invalid www.texture has these consistent properties:
		//(we also reject null.)
		return (tex && tex.name == "" && tex.height == 8 && tex.width == 8 && tex.filterMode == FilterMode.Bilinear && tex.anisoLevel == 1 && tex.wrapMode == TextureWrapMode.Repeat && tex.mipMapBias == 0);
	}

    public static Texture2D GetScreenshot()
    {
        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture.Apply();
        return texture;
    }

    public static void SetScreenAutoOrientation(bool activate)
    {
        Screen.autorotateToLandscapeLeft = activate;
        Screen.autorotateToLandscapeRight = activate;
    }

    public static string GetGameObjectPath(GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }

    /// <summary>
    /// Получение текущего значения прозрачности от ф-ции,с заданным интервалом изменения от заданного минимального значения до 1
    /// </summary>
    /// <param name="minValue">должно быть от 0 до 1</param>
    /// <returns></returns>
    private static float GetFadingAlpha(float interval, float minValue, float initialTime)
    {
        return 0.5f*Mathf.Cos((Mathf.PI*((Time.time/interval) - initialTime))/(Mathf.PI*0.5f) + 1)*(1 - minValue) + (1 - (1 - minValue)*0.5f);
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Full transform name (with path)
    /// </summary>
    public static string GetFullTransformName(Transform tr)
    {
        if (tr == null)
            return "(null)";

        string trName = tr.name;
        Transform trParent = tr.parent;
        while (trParent != null)
        {
            trName = trParent.name + "/" + trName;
            trParent = trParent.parent;
        }
        return trName;
    }


    public static Transform GetTopParentTransform(Transform tr)
    {
        if (tr == null)
            return null;
        while (tr.parent != null)
            tr = tr.parent;
        return tr;
    }

    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }

    public static void ChangeLayersRecursively(Transform trans, string name)
    {
        trans.gameObject.layer = LayerMask.NameToLayer(name);
        foreach (Transform child in trans)
        {
            ChangeLayersRecursively(child.transform, name);
        }
    }

    public static int FindFirstInString(string str, Predicate<char> predicate)
    {
        for (int i = 0; i < str.Length; i++)
        {
            if (predicate(str[i]))
                return i;
        }

        return -1;
    }

    public static bool CheckIfLayerInMask(int mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }

    public static int GetLayerMask(params string[] layerNames)
    {
        int mask = 0;

        foreach (string layerName in layerNames)
            mask |= 1 << LayerMask.NameToLayer(layerName);

        return mask;
    }

    public static int ExcludeLayersFromMask(int layerMask, params string[] layersToExclude)
    {
        int excludeMask = GetLayerMask(layersToExclude);
        return layerMask & ~excludeMask;
    }

    public static int ExcludeLayerFromMask(int layerMask, int layer)
    {
        return layerMask & ~(1 << layer);
    }

    public static void ShowMaskLayers(string maskName, int mask)
    {
        Debug.LogFormat("Layers for mask '{0} ({1})':", maskName, mask);
        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) != 0)
                Debug.LogFormat("{0} ({1})", LayerMask.LayerToName(i), i);
        }
    }

    /// <summary>
	/// Parses string to TEnum without try/catch and .NET 4.5 TryParse()
	/// </summary>
	public static bool TryParseToEnum<TEnum>(string probablyEnumAsString_, out TEnum enumValue_, TEnum defaultEnumValue_, bool showWarning = false) where TEnum : struct
    {
        enumValue_ = defaultEnumValue_;
        if (!Enum.TryParse(probablyEnumAsString_, true, out enumValue_))
        {
            if (showWarning)
                Debug.LogErrorFormat("Can't parse value {0} to enum of type {1}", probablyEnumAsString_, enumValue_.GetType());
            return false;
        }
        return true;
    }

    /// <summary>
    /// Parses string to TEnum without try/catch and .NET 4.5 TryParse(). Short ver.
    /// </summary>
    public static bool TryParseToEnum<TEnum>(string probablyEnumAsString_, out TEnum enumValue_) where TEnum : struct
    {
        TEnum en = (TEnum)Enum.GetValues(typeof(TEnum)).GetValue(0);
        return TryParseToEnum<TEnum>(probablyEnumAsString_, out enumValue_, en, false);
    }

    public static TEnum GetParsedEnumValue<TEnum>(string probablyEnumAsString_, TEnum defaultEnumValue, bool showWarning = false) where TEnum : struct
    {
        TEnum enumValue = defaultEnumValue;
        TryParseToEnum(probablyEnumAsString_, out enumValue, defaultEnumValue, showWarning);
        return enumValue;
    }
}
