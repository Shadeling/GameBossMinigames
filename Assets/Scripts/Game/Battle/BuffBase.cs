using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    [CreateAssetMenu(fileName = "BuffBase", menuName = "ScriptableObjects/Buffs+Debuffs/BuffBase", order = 1)]
    public class BuffBase : ScriptableObject, IBuff
    {
        [SerializeField] private string name;

        [SerializeField] private string description;

        [SerializeField] private string spriteName;


        [Space(20),Header("Stats")]
        [SerializeField] private BuffType buffType;

        [SerializeField] private bool isNegative;

        [SerializeField] private List<StatValue> statChanges;

        [SerializeField] private List<ResistanceValue> resistChanges;

        [SerializeField] private DamageType damageType;

        #region IBuff
        public BuffType BuffType => buffType;

        public bool IsNegative => isNegative;

        public string Name => name;

        public string Description => description;

        public string SpriteName => spriteName;

        public DamageType DamageType => damageType;

        #endregion

        public virtual void EveryTurnUpdate(IUnit unit)
        {
            if(BuffType == BuffType.EveryTurn)
            {
                ApplyEffectTo(unit);
            }
        }

        public virtual void OnApply(IUnit unit)
        {
            if(BuffType == BuffType.DurationEffect)
            {
                ApplyEffectTo(unit);
            }
        }

        public virtual void OnRemove(IUnit unit)
        {
            if(BuffType == BuffType.DurationEffect)
            {
                ResetEffectTo(unit);
            }
            else if(BuffType == BuffType.OnEndEffect)
            {
                ApplyEffectTo(unit);
            }
        }

        protected void ApplyEffectTo(IUnit unit)
        {
            foreach (StatValue stat in statChanges)
            {
                unit.ChangeStats(stat.stat, stat.value, DamageType);
            }
            foreach (ResistanceValue resistance in resistChanges)
            {
                unit.ChangeResistance(resistance.resistanceType, resistance.change);
            }
        }

        protected void ResetEffectTo(IUnit unit)
        {
            foreach (StatValue stat in statChanges)
            {
                unit.ChangeStats(stat.stat, -1 * stat.value, DamageType);
            }
            foreach (ResistanceValue resistance in resistChanges)
            {
                unit.ChangeResistance(resistance.resistanceType, -1 * resistance.change);
            }
        }
    }

    [Serializable]
    public struct BuffWithDuration
    {
        [SerializeField]
        public BuffBase buff;

        [SerializeField]
        public int duration;
    }
}
