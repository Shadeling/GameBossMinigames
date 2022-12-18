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
        EffectOnAll = 3,

        TransformTiles = 4,
    }

    /// <summary>
    /// Также нужны множители Урона, Дистанции, перезарядки и прочего от базовых характеристик(сила, ловкость, инт)
    /// </summary>
    public interface ISpell : IVisualizable
    {
        public SpellType Type { get; }

        public DamageType DamageType { get; set; }

        public SpellStatWithMultiplier MinDistance { get;  }

        public SpellStatWithMultiplier MaxDistance { get;  }

        public ZoneSO Zone { get;  }

        public SpellStatWithMultiplier Cooldown { get; }

        public int CurrentCooldown { get; }

        public abstract void OnTurnEnd();
        public abstract void OnCast(IUnit caster);
        public abstract void OnTargetAlly(IUnit target);

        public abstract void OnTargetEnemy(IUnit target);

        public abstract void OnTargetTiles(IBattleCell tile);
    }


    [Serializable]
    public struct SpellStatWithMultiplier
    {
        [SerializeField]
        public int StartValue;

        [SerializeField]
        public List<StatMultiplier> multipliers;

    }
}
