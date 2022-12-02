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
        public List<IBuff> Buffs { get; }

        public bool IsAlive { get; }

        public bool EquipItem(BaseEquipment item);
        public bool RemoveItem(BaseEquipment item);

        public void TakeHit(float damage, DamageType type = DamageType.Pure);

        public void Heal(float heal);

        public void ChangeStats(UnitStat stat, float value);

        public float GetStat(UnitStat stat);

        public void ChangeResistance(DamageType damageType, float value);

        public void CreateFromTemplate(UnitTemplate template);
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
