using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    [CreateAssetMenu(fileName = "BuffBase", menuName = "ScriptableObjects/Buffs+Debuffs/BuffBase", order = 1)]
    public class BuffBase : ScriptableObject, IBuff
    {
        public BuffType BuffType => throw new System.NotImplementedException();

        public bool isNegative => throw new System.NotImplementedException();

        public UnitStat AffectStat => throw new System.NotImplementedException();

        public int duration => throw new System.NotImplementedException();

        public string Name => throw new System.NotImplementedException();

        public string Description => throw new System.NotImplementedException();

        public string SpriteName => throw new System.NotImplementedException();

        public void EveryTurnUpdate(IUnit unit)
        {
            throw new System.NotImplementedException();
        }

        public void OnApply(IUnit unit)
        {
            throw new System.NotImplementedException();
        }

        public void OnRemove(IUnit unit)
        {
            throw new System.NotImplementedException();
        }
    }
}
