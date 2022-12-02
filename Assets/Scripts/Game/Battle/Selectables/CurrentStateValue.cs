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
        SelectCellWithUnit = 1,
        SelectCellWithoutUnit = 2,
        SelectSpell = 3,
    }
}