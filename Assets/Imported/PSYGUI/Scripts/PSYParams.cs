using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using System.Collections;

public class PSYParamsEmpty : PSYParams
{
	public PSYParamsEmpty() : base(true)
	{
		
	}
}

public class PSYParams
{
	private bool                        empty = false;
	private object                      id = null;
	private List<object>                param = null;

    private Dictionary<object, object>  namedParam = null;

    private Dictionary<object, object> NamedParam
    {
        get
        {
            if (namedParam == null)
            {
                namedParam = new Dictionary<object, object>();
            }
            return namedParam;
        }
    }


	public int Count
    {
        get
        {
            return param.Count;
        }
    }

    public object[] GetParam()
    {
        if (param == null)
        {
            param = new List<object>();
        }

        return param.ToArray();
    }

	public override string ToString()
	{
		return "Param: {id='" + id + "'; " + (param != null ? param.ToStringF() : "param is NULL") + "}";
	}

	public PSYParams ID(object id)
	{
		this.id = id;
		return this;
	}

	public PSYParams(bool empty = false)
	{
		this.empty = empty;
		if (!empty)
		{
			param = new List<object>();			
		}
	}	
	
	public static PSYParams New(params object[] param)
	{
		return new PSYParams().Add(param);
	}

	public static PSYParams Empty
	{
		get
        {
            return new PSYParamsEmpty();
        }
	}
	
	/// <summary>
	/// Checks if parameters class contains any value of given type
	/// </summary>
	/// <typeparam name="T">Type to search</typeparam>
	/// <param name="searchBaseTypes">If all base types of types presented in parameters should be searched too</param>	
	public bool Contains<T>(bool searchBaseTypes = true)
	{
		return GetList<T>(searchBaseTypes).Count > 0;
	}

	/// <summary>
	/// Checks if parameters class contains given value of given type
	/// </summary>
	/// <typeparam name="T">Type to search</typeparam>
	/// <param name="value">Value to search</param>
	/// <param name="searchBaseTypes">If all base types of types presented in parameters should be searched too</param>	
	public bool Contains<T>(T value, bool searchBaseTypes = false)
	{
		return GetList<T>(searchBaseTypes).Contains(value);
	}

	public bool ContainsByID(object id)
	{
        if (empty)
        {
            return false;
        }

		foreach (object key in NamedParam.Keys.ToList<object>())
		{
            if (key.Equals(id))
            {
                return true;
            }
		}
		return false;
	}

	public bool Bool
    {
        get
        {
            return Get<bool>();
        }
    }

	public string String
    {
        get
        {
            return Get<string>();
        }
    }

	public int Int
    {
        get
        {
            return Get<int>();
        }
    }

	public float Float
    {
        get
        {
            return Get<float>();
        }
    }

	public Vector2 Vector2
    {
        get
        {
            return Get<Vector2>();
        }
    }

	public Vector3 Vector3
    {
        get
        {
            return Get<Vector3>();
        }
    }

	public Vector4 Vector4
    {
        get
        {
            return Get<Vector4>();
        }
    }

	/// <summary>
	/// Returns value of given type by value's index in the list of all values of that type
	/// </summary>
	/// <typeparam name="T">Target type</typeparam>
	/// <param name="index">Index of value</param>
	/// <param name="searchBaseTypes">If all base types of types presented in parameters should be searched too</param>		
	public T Get<T>(int index = 0, object def = null, bool searchBaseTypes = true)
	{
        int found = -1;
        if (empty)
        {
            return default(T);
        }

        for (int i = 0; i < param.Count; i++)
        {
            Type checkKey = param[i].GetType();
            do
            {
                Type type = typeof(T);
                if (type == checkKey || type.IsAssignableFrom(checkKey))
                {
                    if (++found == index)
                    {
                        return (T)param[i];
                    }                    
                }
#if NETFX_CORE
            checkKey = checkKey.GetTypeInfo().BaseType;
#else
                checkKey = checkKey.BaseType;
#endif
            }
            while (checkKey != typeof(System.Object) && searchBaseTypes);
        }
		
        return def != null && def.GetType() == typeof(T) ? (T)def : default(T);
	}

	/// <summary>
	/// Returns last value of given type in the list of all values of that type
	/// </summary>
	/// <param name="searchBaseTypes">If all base types of types presented in parameters should be searched too</param>		
	public T GetLast<T>(bool searchBaseTypes = true)
	{
        if (empty)
        {
            return default(T);
        }

		List<T> list = GetList<T>(searchBaseTypes);
		return Get<T>(list.Count - 1, searchBaseTypes);
	}

	/// <summary>
	/// Returns value with given index
	/// </summary>
	/// <param name="index">Index of value</param>	
	public object GetAt(int index = 0)
	{
        if (empty)
        {
            return null;
        }

        if (param.Count > index)
        {
            return param[index];
        }
		/*else if (param.Count > 0)
			PSYDebug.Log(PSYDebug.Source.Params, "param index {0} is out of range", index);
		else
			PSYDebug.Log(PSYDebug.Source.Params, "missing {0} parameter in PSYParams");*/
		return null;
	}

    /// <summary>
    /// Returns parameter with ID by given ID
    /// </summary>
    /// <typeparam name="T">Type expected</typeparam>
    /// <param name="name">ID of the parameter</param>
    /// <returns>Default value of T if type mismatched</returns>
    public T GetByID<T>(object id, object def = null, bool searchBaseTypes = true)
    {
        if (empty)
        {
            return def != null && def.GetType() == typeof(T) ? (T)def : default(T);
        }
        object value = null;
        foreach (object key in NamedParam.Keys.ToList<object>())
        {
            if (key.Equals(id))
            {
                value = NamedParam[key];
            }
        }
        if (value == null)
        {
            PSYDebug.Warning(PSYDebug.Source.Params, "missing parameter of type {0} with ID '{1}'", typeof(T), id);
            return def != null && def.GetType() == typeof(T) ? (T)def : default(T);
        }

        bool foundType = false;
        Type checkType = value.GetType();
        do
        {
            if (typeof(T) == checkType)
                foundType = true;
#if NETFX_CORE
            checkType = checkType.GetTypeInfo().BaseType;
#else
            checkType = checkType.BaseType;
#endif
        }
        while (checkType != typeof(System.Object) && searchBaseTypes);

        if (!foundType)
        {
            PSYDebug.Warning(PSYDebug.Source.Params, "named parameter type mismatch: {0} {1} (should be {2})", typeof(T), id, value.GetType());
            return def.GetType() == typeof(T) ? (T)def : default(T);
        }
        return (T)value;
    }

	public bool HasID()
	{
		return id != null;
	}

	public T GetID<T>()
	{
		if (id == null) return default(T);
		if (typeof(T) != id.GetType())
		{
			PSYDebug.Error(PSYDebug.Source.Params, "ID type mismatch: {0} (should be {1})", typeof(T), id.GetType());
			return default(T);
		}
		return (T)id;
	}

	/// <summary>
	/// Returns list of values of given type
	/// </summary>
	/// <typeparam name="T">Target type</typeparam>
	/// <param name="searchBaseTypes">If all base types of types presented in parameters should be searched too</param>		
	public List<T> GetList<T>(bool searchBaseTypes = true)
	{
		List<T> res = new List<T>();
        if (empty)
        {
            return res;
        }

		for (int i = 0; i < param.Count; i++)
		{
			Type checkKey = param[i].GetType();
			do
			{
                Type type = typeof(T);
                if (type == checkKey || type.IsAssignableFrom(checkKey))
                {
                    res.Add((T)param[i]);
                }

#if NETFX_CORE
            checkKey = checkKey.GetTypeInfo().BaseType;
#else
                checkKey = checkKey.BaseType;
#endif                
			}
			while (checkKey != typeof(System.Object) && searchBaseTypes);
		}
		
		return res;
	}

	/// <summary>
	/// Returns list of all types, presented in parameters
	/// </summary>	
	public List<Type> GetTypes()
	{		
		List<Type> types = new List<Type>();
        if (empty)
        {
            return types;
        }

		for (int i = 0; i < param.Count; i++)
		{
			if (!types.Contains(param[i].GetType()))
				types.Add(param[i].GetType());
		}
		return types;
	}

	/// <summary>
	/// Returns type by it's index if the list of all types presented in parameters
	/// </summary>
	public Type GetTypeAt(int index = 0)
	{
        if (empty)
        {
            return default(Type);
        }

        if (GetTypes().Count > index)
        {
            return GetTypes()[index];
        }
        else
        {
            //PSYDebug.Log(PSYDebug.Source.Params, "index of type {0} is out of range", index);
            return default(Type);
        }
	}

	/// <summary>
	/// Adds given params to lists of appropriate types, but only if condition is true
	/// </summary>
	/// <param name="condition">Condition to check</param>
	public PSYParams AddIf(bool condition, params object[] param)
	{
        if (condition)
        {
            Add(param);
        }

		return this;
	}
	
	/// <summary>
	/// Adds given params to lists of appropriate types.
	/// Type will be determined automatically basing on given value.
	/// </summary>	
	public PSYParams Add(params object[] param)
	{
		if (empty)
		{
			empty = false;
			this.param = new List<object>();
		}

		for (int i = 0; i < param.Length; i++)
		{
            //object par = PSYUtils.CloneObject(param[i]);
            if (param[i] == null)
            {
                continue;
            }

            if (param[i] is PSYParams)
            {
                this.NamedParam.MergeDict(((PSYParams)param[i]).NamedParam);
                this.param.AddRange(((PSYParams)param[i]).param);
            }
            else
            {
                this.param.Add(param[i]);
            }
		}
		return this;
	}

	/// <summary>
	/// Changes the value of a parameter of given type.
	/// Type will be determined automatically basing on given value.
	/// </summary>
	/// <param name="value">Значение для установки</param>	
	public PSYParams Set<T>(object value)
	{
		if (empty) return this;
        for (int i = param.Count - 1; i >= 0; i--)
        {
            if (param[i].GetType() == typeof(T))
            {
                param.RemoveAt(i);
            }
        }

		Add(value);
		return this;
	}

	/// <summary>
	/// Saves given param and it's ID in parameters
	/// </summary>
	/// <param name="id">ID of parameter</param>
	/// <param name="value">Value of parameter</param>
	/// <returns></returns>
	public PSYParams AddByID(object id, object value)
	{
        if (empty)
        {
            return this;
        }

		NamedParam[id] = value;
		return this;
	}

    public bool TryGetByID<T>(object id, out T res, bool searchBaseTypes = true)
    {
        res = default(T);
        object value = null;
        foreach (object key in NamedParam.Keys.ToList<object>())
        {
            if (key.Equals(id))
            {
                value = NamedParam[key];
            }
        }
        if (value == null)
        {
            PSYDebug.Warning(PSYDebug.Source.Params, "missing parameter of type {0} with ID '{1}'", typeof(T), id);
            return false;
        }

        bool foundType = false;
        Type checkType = value.GetType();
        do
        {
            if (typeof(T) == checkType)
            {
                foundType = true;
            }
#if NETFX_CORE
                checkType = checkType.GetTypeInfo().BaseType;
#else
            checkType = checkType.BaseType;
#endif
        }
        while (checkType != typeof(System.Object) && searchBaseTypes);

        if (!foundType)
        {
            PSYDebug.Warning(PSYDebug.Source.Params, "named parameter type mismatch: {0} {1} (should be {2})", typeof(T), id, value.GetType());
            return false;
        }

        res = (T)value;
        return true;
    }

    public PSYParams MergeNamed(PSYParams param)
	{
        if (empty || param.NamedParam == null)
        {
            return this;
        }

        foreach (var pair in param.NamedParam)
        {
            NamedParam[pair.Key] = pair.Value;
        }

		return this;
	}

	public PSYParams MergeID(PSYParams param)
	{
        if (empty)
        {
            return this;
        }

		this.id = param.id;
		return this;
	}

	public PSYParams Merge(PSYParams param)
	{
        if (empty)
        {
            return this;
        }

		MergeID(param);
		MergeNamed(param);
        if (param.param != null)
        {
            this.param.AddRange(param.param);
        }
		return this;
	}

	public PSYParams Clone()
	{
		return PSYParams.New().Merge(this);
	}
}