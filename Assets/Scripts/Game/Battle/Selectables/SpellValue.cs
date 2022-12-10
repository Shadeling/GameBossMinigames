using UnityEngine;

namespace MyGame.Utils
{
    [CreateAssetMenu(fileName = nameof(SpellValue), menuName = "Something/" + nameof(SpellValue), order = 0)]
    public class SpellValue : StatefulScriptableObjectValueBase<ISpell>
    {

    }
}