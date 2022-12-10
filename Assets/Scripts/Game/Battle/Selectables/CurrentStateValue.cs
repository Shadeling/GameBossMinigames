using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Utils
{
    [CreateAssetMenu(fileName = nameof(CurrentStateValue), menuName = "Something/" + nameof(CurrentStateValue), order = 0)]
    public class CurrentStateValue : StatefulScriptableObjectValueBase<CurrentState>
    {

    }


    public enum CurrentState
    {
        None = -1,
        CellWithUnitSelected = 1,
        CellWithoutUnitSelected = 2,
        SpellSelected = 3,
    }
}