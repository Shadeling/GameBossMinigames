using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
#if NETFX_CORE
using System.Reflection;
#endif

namespace XD
{
    using UniRx;
    
    public static class UtilsExtensionMethods
    {
        public static string Serialize<T>(this T obj)
        {
            return MiniJSON.Json.Serialize(obj);
        }

        public static T GetRandom<T>(this T[] array)
        {
            return array[UnityEngine.Random.Range(0, array.Length)];
        }

        public static string GetName(this UnityEngine.Object obj)
        {
            return obj == null ? "Null" : obj.name;
        }

        public static string ToFullString(this object[] objects)
        {
            string result = "Objects:\n";
            for (int i = 0; i < objects.Length; i++)
            {
                result += (objects[i] == null ? "null type" : objects[i].GetType().ToString()) + ": " + (objects[i] == null ? "null" : objects[i].ToString()) + "\n";
            }
            return result;
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static string DebugString<T, M>(this Dictionary<T, M> dictionary, string separator = "\n")
        {
            string result = "";

            foreach (KeyValuePair<T, M> pair in dictionary)
            {
                result += pair.Key + ": " + pair.Value + separator;
            }
            return result;
        }

        public static bool IsDefined<T>(this T value) where T: Enum
        {
            return Enum.IsDefined(typeof(T), value);
        }

        private static Dictionary<Type, IFromJsonFactory> JsonFactories = new Dictionary<Type, IFromJsonFactory>();
        private static Dictionary<Type, Type> JsonFactoryTypes = new Dictionary<Type, Type>();

        public static bool RegisterJsonFactory<C, F>()
            where C: class              // класс для которого регистрируетяс фабрика
            where F: IFromJsonFactory   // класс фабрики
        {
            Type classType = typeof(C);
            if (!JsonFactoryTypes.ContainsKey(classType))
            {
                JsonFactoryTypes.Add(classType, typeof(F));
                return true;
            }
            return false;
        }


        public static bool ExtractList<T>(this Dictionary<string, object> dict, string key, ref List<T> output)
        {
            if (dict != null && 
                dict.TryGetValue(key, out object obj))
            {
                ICollection<object> collection;
                if (obj is Dictionary<string, object> objAsDict)
                    collection = objAsDict.Values;
                else
                if (obj is List<object> objAsList)
                    collection = objAsList;
                else
                    return false;

                List<T> result = null;
                foreach (object itemData in collection)
                {
                    object item = default(T);
                    if (itemData.Extract(typeof(T), ref item))
                    {
                        if (result == null)
                            result = new List<T>(collection.Count);
                        result.Add((T)item);
                    }
                    else
                    {
                        return false;
                    }
                }
                output = result;
                return true;
            }
            return false;
        }

        private static bool Extract(this object obj, Type resultType, ref object output)
        {
            try
            {
                // from factory
                if (JsonFactoryTypes.TryGetValue(resultType, out Type factoryType))
                {
                    if (!JsonFactories.TryGetValue(resultType, out IFromJsonFactory factory))
                    {
                        factory = (IFromJsonFactory)Activator.CreateInstance(factoryType);
                        JsonFactories.Add(resultType, factory);
                    }

                    object result = null;
                    JsonParseResult parseResult = factory.FromJson(obj, ref result);
                    switch (parseResult)
                    {
                        case JsonParseResult.Success:
                            if (resultType.IsInstanceOfType(result))
                            {
                                output = result;
                                return true;
                            }
                            Debug.LogErrorFormat("Parsing with factory error. Illegal result type [{0}].", result.GetType());
                            break;

                        case JsonParseResult.FactoryNotFound:
                            Debug.LogErrorFormat("Parsing with factory error. Factory not found for type [{0}].", resultType);
                            break;

                        case JsonParseResult.InvalidDataType:
                            Debug.LogErrorFormat("Parsing with factory error. Invalid data type [{0}].", obj.GetType());
                            break;

                        case JsonParseResult.NotEnoughData: 
                            Debug.LogErrorFormat("Parsing with factory error. Not enough data for type [{0}].", resultType);
                            break;
                    }
                }

                if (output is Enum)
                {
                    string objString = obj as string;
                    object result = Enum.Parse(resultType, objString ?? Convert.ToString(obj), true);
                    if (Enum.IsDefined(resultType, result))
                        output = result;
                    else
                    {
                        Debug.LogErrorFormat("Enum parsing error. Undefined enum value [{0}] of type [{1}] .", result, resultType);
                        return false;
                    }
                }
                else
                    output = Convert.ChangeType(obj, resultType);

                return true;
            }
            catch
            {
            }

            return false;
        }

        public static bool Extract<T>(this Dictionary<string, object> dict, string key, ref T output, bool necessary = true)
        {
            if (dict != null && 
                dict.TryGetValue(key, out object obj))
            {
                object result = output;
                if (obj.Extract(typeof(T), ref result))
                {
                    output = (T)result;
                    return true;
                }
            }
            return !necessary;
        }

        public static T ExtractOrDefault<T>(this Dictionary<string, object> dict, string key, T defaultValue = default(T))
        {
            T output = defaultValue;
            Extract(dict, key, ref output, false);

            return output;
        }

        public static string ToTime(this float value)
        {
            return ((int)value).ToTime();
        }

        public static string ToTime(this int value)
        {
            int hours = value / 3600;
            int minutes = (value - hours * 60) / 60;
            int seconds = value % 60;
            return (hours > 0 ? (hours.TimeFormat() + ":") : "") + minutes.TimeFormat() + ":" + seconds.TimeFormat();
        }       

        public static string TimeFormat(this int value)
        {
            return value >= 10 ? value.ToString() : "0" + value;
        }

        public static float Round(this float value, int power)
        {
            float pow = Mathf.Pow(10, power);
            return Mathf.Round(value * pow) / pow;
        }

        public static string ToHex(this Color color)
        {
            string hex = "#" + color.r.ToColor().ToString("X2") + color.g.ToColor().ToString("X2") + color.b.ToColor().ToString("X2");
            return hex;
        }

        public static int ToColor(this float val)
        {
            return (int)(255 * val);
        }

        public static string GetTimeString(this System.DateTime dateTime)
        {
            string time = (dateTime.Hour < 10 ? "0" : "") + dateTime.Hour + ":" + (dateTime.Minute < 10 ? "0" : "") + dateTime.Minute + ":" + (dateTime.Second < 10 ? "0" : "") + dateTime.Second;
            return time;
        }

        public static string Concat(this string[] value, int startIndex = 5, int maxCount = -1)
        {
            if (value == null)
            {
                return "";
            }
            string result = "";
            if (maxCount == -1)
            {
                maxCount = value.Length;
            }

            for (int i = startIndex; i < startIndex + Mathf.Min(maxCount, value.Length - startIndex); i++)
            {
                result += "\n" + value[i];
            }
            return result;
        }

        public static string GetName<T>(this T target) where T : Component
        {
            if (target == null)
            {
                return "NULL";
            }
            return target.name;
        }

        public static float AngleSigned(this Vector2 vector)
        {
            return -Mathf.Atan2(vector.x, vector.y) * Mathf.Rad2Deg;
        }

        public static float AngleSigned(this Vector3 vector1, Vector3 vector2, Vector3 normal)
        {
            return Mathf.Atan2(
                Vector3.Dot(normal, Vector3.Cross(vector1, vector2)),
                Vector3.Dot(vector1, vector2)) * Mathf.Rad2Deg;
        }

        public static List<Transform> FindRecursivelyArray(this Transform transform, string name)
        {
            if (transform == null)
            {
                return new List<Transform>();
            }
            List<Transform> result = new List<Transform>();
            Transform founded = transform.Find(name);
            if (founded != null)
            {
                result.Add(founded);
            }
            else
            {
                foreach (Transform child in transform)
                {
                    result.AddRange(child.FindRecursivelyArray(name));                    
                }
            }
            return result;
        }

        public static List<Transform> FindMatchRecursivelyArray(this Transform transform, string[] partNames)
        {
            if (transform == null)
            {
                return new List<Transform>();
            }
            List<Transform> result = new List<Transform>();

            foreach (Transform child in transform)
            {
                string childName = child.name.ToLower();

                foreach (var partName in partNames)
                {
                    if (childName.Contains(partName))
                        result.Add(child);
                }
            }

            foreach (Transform child in transform)
            {
                result.AddRange(child.FindMatchRecursivelyArray(partNames));
            }

            return result;
        }

        public static Transform FindRecursively(this Transform transform, string name)
        {
            Transform result = transform.Find(name);
            if (result == null)
            {
                foreach (Transform child in transform)
                {
                    result = child.FindRecursively(name);
                    if (result != null)
                    {
                        break;
                    }
                }
            }
            return result;
        }

        public static T GetOrAdd<T>(this GameObject gameObject) where T : MonoBehaviour
        {
            T comp = gameObject.GetComponent<T>();
            if (comp == null)
            {
                comp = gameObject.AddComponent<T>();
            }
            return comp;
        }

        public static T RandomElement<T>(this object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                return default(T);
            }
            return (T)parameters[UnityEngine.Random.Range(0, parameters.Length)];
        }

        

        public static bool Contains(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }
        

        public static void SetLayer(this GameObject gameObject, int layer, LayerMask ignore)
        {
            if (gameObject == null)
            {
                return;
            }

            if (!ignore.Contains(gameObject.layer))
            {
                gameObject.layer = layer;
            }

            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetLayer(layer, ignore);
            }
        }

        public static void SetEffectsAlpha(this ParticleSystem[] effects, float alpha)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                SetEffectAlpha(effects[i], alpha);
            }
        }

        public static void SetEffectAlpha(this ParticleSystem particleSystem, float alpha)
        {
            var main = particleSystem.main;
            Color color = main.startColor.color;
            color.a = alpha;
            main.startColor = color;

        }

        public static bool Set<T>(this object[] parameters, T value, int index = 0)
        {
            int currentIndex = -1;
            Type targetType = typeof(T);
#if NETFX_CORE
            TypeInfo targetTypeInfo = targetType.GetTypeInfo();
#else
            Type targetTypeInfo = targetType;
#endif
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] == null)
                {
                    continue;
                }

#if NETFX_CORE
                TypeInfo type = parameters[i].GetType().GetTypeInfo();
#else
                Type type = parameters[i].GetType();
#endif

                if (targetTypeInfo.IsAssignableFrom(type) || type.IsAssignableFrom(targetTypeInfo) || type.IsSubclassOf(targetType))
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        parameters[i] = value;
                        return true;
                    }
                }
            }
            return false;
        }

        public static T Get<T>(this object[] parameters, int index = 0)
        {
            T result = default(T);
            int currentIndex = -1;
            Type targetType = typeof(T);
#if NETFX_CORE
            TypeInfo targetTypeInfo = targetType.GetTypeInfo();
#else            
            Type targetTypeInfo = targetType;
#endif
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] == null)
                {
                    continue;
                }                

#if NETFX_CORE
                TypeInfo type = parameters[i].GetType().GetTypeInfo();
#else
                Type type = parameters[i].GetType();
#endif                

                if (targetTypeInfo.IsAssignableFrom(type) || type.IsAssignableFrom(targetTypeInfo) || type.IsSubclassOf(targetType))
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        result = (T)parameters[i];
                        break;
                    }
                }
            }
            return result;
        }

        public static string FirstLetterToUpper(this string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        public static void Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }

        public static void Do<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            if (sequence == null)
                return;
            IEnumerator<T> enumerator = sequence.GetEnumerator();
            while (enumerator.MoveNext())
                action(enumerator.Current);
        }

        public static void DoIf<T>(
            this IEnumerable<T> sequence,
            Func<T, bool> condition,
            Action<T> action)
        {
            sequence.Where<T>(condition).Do<T>(action);
        }

        /// <summary>
        /// Get all immediate children in hierarchy
        /// </summary>
        public static Transform[] GetImmediateChildren(this Transform transform, bool sortByHierarchy = false)
        {
            List<Transform> children = new List<Transform>();

            foreach (Transform child in transform)
            {
                children.Add(child);
            }

            if (sortByHierarchy)
                return children.OrderBy(x => x.GetSiblingIndex()).ToArray();

            return children.ToArray();
        }

        public static string ToStringFull(this object[] data)
        {
            if (data == null) return "null";

            string[] sb = new string[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                object o = data[i];
                sb[i] = (o != null) ? o.ToString() : "null";
            }

            return string.Join(", ", sb);
        }
    }
}
