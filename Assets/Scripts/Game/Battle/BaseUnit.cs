using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XD;

namespace MyGame
{

    public class BaseUnit : MonoBehaviour, IUnit
    {

        private string name;

        private string description;

        private string spriteName;

        private bool controlledByPlayer;

        private List<BaseEquipment> equipment;

        private UnitClass unitClass;

        private List<IBuff> buffs = new List<IBuff>();

        private List<SpellBase> spells;

        private List<PassiveBase> passives;

        #region IUnit

        public string Name => name;

        public string Description => description;

        public string SpriteName => spriteName;

        public bool ControlledByPlayer { get => controlledByPlayer; set => controlledByPlayer = value; }

        public List<BaseEquipment> Equipment => equipment;

        public UnitClass UnitClass => unitClass;

        public List<IBuff> Buffs => buffs;

        public Transform PivotPoint => transform;

        public List<SpellBase> Spells => spells;

        public List<PassiveBase> Passives => passives;

        public bool IsAlive => HP.Minimum;

        #endregion

        private Clamper HP;

        private Dictionary<UnitStat, float> statsDict = new Dictionary<UnitStat, float>();

        private Dictionary<DamageType, float> resistanceDict = new Dictionary<DamageType, float>();

        public BaseUnit(UnitTemplate template)
        {
            CreateFromTemplate(template);
        }

        public void ChangeResistance(DamageType damageType, float value)
        {
            if (resistanceDict.ContainsKey(damageType)){
                resistanceDict[damageType] += value;
            }
        }

        public void ChangeStats(UnitStat stat, float value)
        {
            if(stat != UnitStat.HP)
            {
                if (statsDict.ContainsKey(stat))
                {
                    statsDict[stat] += value;
                }
            }
            else
            {
                HP = HP.AddToMax(value);
            }
        }

        public void CreateFromTemplate(UnitTemplate template)
        {
            this.name = template.Name;
            this.description = template.Description;
            this.spriteName = template.SpriteName;
            this.unitClass = template.UnitClass; 
            this.controlledByPlayer = template.ControlledByPlayer;
            this.spells = new List<SpellBase>(template.Spells);
            this.passives = new List<PassiveBase>(template.Passives);

            statsDict.Clear();
            foreach(var baseStat in template.stats)
            {
                if (baseStat.stat == UnitStat.HP)
                {
                    HP = new Clamper(baseStat.value);
                }
                else
                {
                    if(!statsDict.ContainsKey(baseStat.stat))
                        statsDict.Add(baseStat.stat, baseStat.value);
                }
            }

            resistanceDict.Clear();
            foreach (var resistance in template.resistances)
            {
                if (!resistanceDict.ContainsKey(resistance.resistanceType))
                    resistanceDict.Add(resistance.resistanceType, resistance.change);
            }

            foreach(var item in template.Equipment)
            {
                EquipItem(item);
            }

        }

        public bool EquipItem(BaseEquipment item)
        {
            if (!Equipment.Contains(item))
            {

                foreach (var stat in item.StatChanges)
                {
                    ChangeStats(stat.stat, stat.value);
                }
                foreach (var res in item.ResistanceChanges)
                {
                    ChangeResistance(res.resistanceType, res.change);
                }
                foreach (var spell in item.GiveSpells)
                {
                    if(!Spells.Contains(spell))
                        Spells.Add(spell);
                }
                foreach (var passive in item.GivePassives)
                {
                    if (!Passives.Contains(passive))
                        Passives.Add(passive);
                }

                Equipment.Add(item);
                return true;
            }
            else return false;
        }

        public bool RemoveItem(BaseEquipment item)
        {
            if (Equipment.Contains(item))
            {

                foreach (var stat in item.StatChanges)
                {
                    ChangeStats(stat.stat, -1 * stat.value);
                }
                foreach (var res in item.ResistanceChanges)
                {
                    ChangeResistance(res.resistanceType, -1 * res.change);
                }
                foreach (var spell in item.GiveSpells)
                {
                    if (Spells.Contains(spell))
                        Spells.Remove(spell);
                }
                foreach (var passive in item.GivePassives)
                {
                    if (Passives.Contains(passive))
                        Passives.Remove(passive);
                }

                Equipment.Remove(item);
                return true;
            }
            else return false;
        }

        public float GetStat(UnitStat stat)
        {
            if (stat != UnitStat.HP)
            {
                if (statsDict.ContainsKey(stat))
                {
                    return statsDict[stat];
                }
            }
            else
            {
                return HP.Current;
            }

            return 0;
        }

        public void TakeHit(float damage, DamageType type = DamageType.Pure)
        {
            if (resistanceDict.ContainsKey(type))
            {
                HP.Plus(damage * resistanceDict[type]);
            }
            else
            {
                HP.Plus(damage);
            }
        }

        public void Heal(float heal)
        {
            HP.Plus(heal);
        }
    }

}
