using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    [CreateAssetMenu(fileName = "SpellBase", menuName = "ScriptableObjects/Spells/SpellBase", order = 1)]
    public class SpellBase : ScriptableObject, ISpell
    {
        public SpellType Type => throw new System.NotImplementedException();

        public DamageType DamageType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public SpellStatWithMultiplier MinDistance => throw new System.NotImplementedException();

        public SpellStatWithMultiplier MaxDistance => throw new System.NotImplementedException();

        public ZoneSO Zone => throw new System.NotImplementedException();

        public SpellStatWithMultiplier Cooldown => throw new System.NotImplementedException();

        public virtual void OnCast(IUnit caster)
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnTargetAlly(List<IUnit> targets)
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnTargetEnemy(List<IUnit> targets)
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnTargetTiles(List<IBattleCell> tiles)
        {
            throw new System.NotImplementedException();
        }
    }
}
