using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XD;

namespace MyGame
{

    public class UnitBase : MonoBehaviour, IUnit
    {

        private string name;

        private string description;

        private string spriteName;

        private bool controlledByPlayer;

        private bool myTurn;

        private List<BaseEquipment> equipment;

        private UnitClass unitClass;

        private Dictionary<IBuff, int> buffsDur;

        private List<SpellBase> spells;

        private List<PassiveBase> passives;

        #region IUnit

        public string Name => name;

        public string Description => description;

        public string SpriteName => spriteName;

        public bool ControlledByPlayer { get => controlledByPlayer; set => controlledByPlayer = value; }

        public List<BaseEquipment> Equipment => equipment;

        public UnitClass UnitClass => unitClass;

        public Transform PivotPoint => transform;

        public List<SpellBase> Spells => spells;

        public List<PassiveBase> Passives => passives;

        public bool IsAlive => HP.Minimum;

        public bool MyTurn { get => myTurn; set => myTurn = value; }

        #endregion

        private Clamper HP;

        private Dictionary<UnitStat, int> statsDict = new Dictionary<UnitStat, int>();

        private Dictionary<DamageType, int> resistanceDict = new Dictionary<DamageType, int>();

        public UnitBase(UnitTemplate template)
        {
            CreateFromTemplate(template);
        }

        public void ChangeResistance(DamageType damageType, int value)
        {
            if (resistanceDict.ContainsKey(damageType)){
                resistanceDict[damageType] += value;
            }
        }

        public void ChangeStats(UnitStat stat, int value, DamageType damageType = DamageType.None)
        {
            if (stat == UnitStat.HealUnit)
            {
                Heal(value);
            }
            else if (stat == UnitStat.DealDamage)
            {
                TakeHit(value, damageType);
            }
            else if (stat == UnitStat.HP)
            {
                HP = HP.AddToMax(value);
            }
            else
            {
                if (statsDict.ContainsKey(stat))
                {
                    statsDict[stat] += value;
                }
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
            this.buffsDur =  new Dictionary<IBuff, int>();

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
                        statsDict.Add(baseStat.stat, (int)baseStat.value);
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
                    ChangeStats(stat.stat, (int)stat.value);
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
                    ChangeStats(stat.stat, -1 * (int)stat.value);
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
                float reduction = resistanceDict[type] == 0 ? 1 : (100 - resistanceDict[type]) / 100;

                HP.Plus(-1 * damage * reduction);
            }
            else
            {
                HP.Plus(-1 * damage);
            }
        }

        public void Heal(float heal)
        {
            HP.Plus(heal);
        }

        public void OnTurnEnd()
        {
            foreach(var buff in buffsDur.Keys)
            {
                buff.EveryTurnUpdate(this);
                buffsDur[buff] = buffsDur[buff] - 1;

                if(buffsDur[buff] <= 0)
                {
                    buff.OnRemove(this);
                    buffsDur.Remove(buff);
                }
            }

            foreach(var spell in spells)
            {
                spell.OnTurnEnd();
            }
        }

        public void AddBuff(IBuff buff, int duration)
        {
            if (buffsDur.ContainsKey(buff)){
                buffsDur[buff] = buffsDur[buff] + duration;
            }
            else
            {
                buff.OnApply(this);
                buffsDur.Add(buff, duration);
            }
        }

        public void ClearBuffs(bool onlyDebuffs = false)
        {

            foreach(IBuff buff in buffsDur.Keys)
            {
                if(buff.IsNegative || !onlyDebuffs)
                {
                    buff.OnRemove(this);
                    buffsDur.Remove(buff);
                }
            }
        }
    }

}
