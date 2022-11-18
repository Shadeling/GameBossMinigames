using System;
using System.Collections.Generic;
using UnityEngine;


namespace MyGame
{
    public enum SpellType
    {
        SelfBuff = 0,

        AllyBuffZone = 1,
        EnemyEffectZone = 2,

    }

    /// <summary>
    /// Также нужны множители Урона, Дистанции, перезарядки и прочего от базовых характеристик(сила, ловкость, инт)
    /// </summary>
    public interface ISpell
    {
        public SpellType Type { get; }

        public DamageType DamageType { get; set; }

        public SpellStatWithMultiplier MinDistance { get;  }

        public SpellStatWithMultiplier MaxDistance { get;  }

        public ZoneSO Zone { get;  }

        public SpellStatWithMultiplier Cooldown { get; }


        public abstract void OnCast(IUnit caster);
        public abstract void OnTargetAlly(List<IUnit> targets);

        public abstract void OnTargetEnemy(List<IUnit> targets);

        public abstract void OnTargetTiles(List<IBattleCell> tiles);
    }


    [Serializable]
    public struct SpellStatWithMultiplier
    {
        [SerializeField]
        public float StartValue;

        [SerializeField]
        public List<SpellMult> multipliers;

    }

    [Serializable]
    public struct SpellMult
    {
        public UnitStat stat;
        public float multiplier;
    }
}
