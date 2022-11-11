using System;
using System.Collections.Generic;
using UnityEngine;

namespace XD
{
    public enum JsonParseResult
    {
        Success,
        InvalidDataType,
        NotEnoughData,
        FactoryNotFound
    }
    public interface IFromJsonFactory
    {
        JsonParseResult FromJson(object data, ref object output);
    }
    public interface IFromJsonDictFactory
    {
        JsonParseResult FromJson(Dictionary<string, object> data, ref object output);
    }
}
