using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGame.Utils;


namespace MyGame
{

    public enum UnitClass
    {
        None = 0,
        Melee = 1,
        Range = 2,
        Mage = 3,
        Mover = 4,
    }

    public interface IUnit : IUnitStats
    { 
        public bool IsAlive { get; }

        public bool MyTurn { get; set; }

        public abstract void OnTurnEnd();
        public abstract bool EquipItem(BaseEquipment item);
        public abstract bool RemoveItem(BaseEquipment item);

        public abstract void TakeHit(float damage, DamageType type = DamageType.Pure);

        public abstract void Heal(float heal);

        public abstract void ChangeStats(UnitStat stat, int value, DamageType damageType = DamageType.None);

        public abstract void AddBuff(IBuff buff, int duration);

        public abstract void ClearBuffs(bool onlyDebuffs = false);

        public abstract float GetStat(UnitStat stat);

        public abstract void ChangeResistance(DamageType damageType, int value);

        public abstract void CreateFromTemplate(UnitTemplate template);
    }


    public interface IUnitStats : IVisualizable
    {
        public List<SpellBase> Spells { get; }

        public List<PassiveBase> Passives { get; }

        public bool ControlledByPlayer { get; set; }

        public UnitClass UnitClass { get; }

        public List<BaseEquipment> Equipment { get; }

    }
}
