using UnityEngine;

namespace MyGame.Utils
{
    [CreateAssetMenu(fileName = nameof(SelectableValue), menuName = "Something/" + nameof(SelectableValue), order = 0)]
    public class SelectableValue : StatefulScriptableObjectValueBase<ISelectable>
    {

    }
}
