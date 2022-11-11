using UnityEngine;
using System.Collections.Generic;

public static class PSYCommon
{
    public static Vector2 DeveloperResolution = new Vector2(1920, 1080);

    public static Dictionary<PSYUpdateType, float> updateIntervals = new Dictionary<PSYUpdateType, float>()
    {
        {PSYUpdateType.Micro50, 0.05f},
        {PSYUpdateType.Micro100, 0.1f},
        {PSYUpdateType.Micro200, 0.2f},
        {PSYUpdateType.Micro500, 0.5f},
        {PSYUpdateType.Sec15, 15.0f},
        {PSYUpdateType.Sec5, 5.0f},
        {PSYUpdateType.Sec1, 1.0f},
    };
}