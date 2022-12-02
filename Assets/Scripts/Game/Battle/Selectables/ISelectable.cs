using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Utils
{

    public interface ISelectable : IVisualizable
    {
        Transform PivotPoint { get; }
    }
}
