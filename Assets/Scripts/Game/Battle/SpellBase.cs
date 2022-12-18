using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    [Serializable]
    public class SpellStatChange
    {

        [SerializeField]
        public UnitStat statToChange;

        [SerializeField]
        public DamageType resistanceType;

        [SerializeField]
        public SpellStatWithMultiplier change;
    }



    [CreateAssetMenu(fileName = "SpellBase", menuName = "ScriptableObjects/Spells/SpellBase", order = 1)]
    public class SpellBase : ScriptableObject, ISpell
    {
        [SerializeField]
        private string name;

        [SerializeField]
        private string description;

        [SerializeField]
        private string spriteName;


        [Space(30),SerializeField]
        private SpellType type;

        [SerializeField]
        private SpellStatWithMultiplier cooldown;

        [SerializeField]
        private ZoneSO zone;


        [Space(20), Header("Stats"),SerializeField]
        private SpellStatWithMultiplier minDistance;

        [SerializeField]
        private SpellStatWithMultiplier maxDistance;

        [Space(20),SerializeField]
        private DamageType damageType;

        [SerializeField]
        private List<SpellStatChange> changes;

        [SerializeField]
        private List<BuffWithDuration> buffs;

        [SerializeField]
        private CellType switchTilesTo = CellType.None;


        private int currentCooldown = 0;

        private IUnit caster;

        #region ISpell
        public SpellType Type => type;

        public DamageType DamageType { get => damageType; set => damageType = value; }

        public SpellStatWithMultiplier MinDistance => minDistance;

        public SpellStatWithMultiplier MaxDistance => maxDistance;

        public ZoneSO Zone => zone;

        public SpellStatWithMultiplier Cooldown => cooldown;

        public int CurrentCooldown => currentCooldown;

        public string Name => name;

        public string Description => description;

        public string SpriteName => spriteName;

        #endregion

        public virtual void OnCast(IUnit caster)
        {
            this.caster = caster;

            currentCooldown = Mathf.CeilToInt(FindFinalStatChange(cooldown, caster));

            if (type == SpellType.SelfBuff)
            {
                ApplySpellEffectToUnit(caster);
            }
        }

        public virtual void OnTargetAlly(IUnit target)
        {
            if(type == SpellType.AllyBuffZone || type == SpellType.EffectOnAll)
            {
                ApplySpellEffectToUnit(target);
            }
        }

        public virtual void OnTargetEnemy(IUnit target)
        {
            if (type == SpellType.EnemyEffectZone || type == SpellType.EffectOnAll)
            {
                ApplySpellEffectToUnit(target);
            }
        }

        public virtual void OnTargetTiles(IBattleCell tile)
        {
            if(switchTilesTo != CellType.None)
            {
                tile.CellType = switchTilesTo;
            }
        }

        public virtual void OnTurnEnd()
        {
            currentCooldown--;
        }

        protected void ApplySpellEffectToUnit(IUnit unit)
        {
            foreach(var statChange in changes)
            {
                int finalChange = FindFinalStatChange(statChange.change, caster);

                unit.ChangeStats(statChange.statToChange, finalChange, damageType);
            }

            foreach(var buff in buffs)
            {
                unit.AddBuff(buff.buff, buff.duration);
            }

        }

        public static int FindFinalStatChange(SpellStatWithMultiplier spellStat, IUnit unit)
        {
            float finalValue = spellStat.StartValue;

            if(unit != null)
            {
                foreach (var multiplier in spellStat.multipliers)
                {
                    finalValue += multiplier.value * unit.GetStat(multiplier.stat);
                }
            }

            return Mathf.FloorToInt(finalValue);
        }
    }
}
