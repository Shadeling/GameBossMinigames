using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{

    public class BaseUnit : IUnit
    {







        public string Name => throw new System.NotImplementedException();

        public string Description => throw new System.NotImplementedException();

        public string SpriteName => throw new System.NotImplementedException();

        public List<ISpell> Spells => throw new System.NotImplementedException();

        public List<IPassive> Passives => throw new System.NotImplementedException();

        public bool ControlledByPlayer { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public List<BaseEquipment> Equipment => throw new System.NotImplementedException();

        public UnitClass UnitClass => throw new System.NotImplementedException();

        public List<IBuff> Buffs => throw new System.NotImplementedException();

        public void ChangeResistance(DamageType damageType, float value)
        {
            throw new System.NotImplementedException();
        }

        public void ChangeStats(UnitStat stat, float value)
        {
            throw new System.NotImplementedException();
        }

        public void CreateFromTemplate(UnitTemplate template)
        {
            throw new System.NotImplementedException();
        }

        public bool EquipItem(BaseEquipment item)
        {
            throw new System.NotImplementedException();
        }

        public bool RemoveItem(BaseEquipment item)
        {
            throw new System.NotImplementedException();
        }

        public void TakeHit(float damage, DamageType type = DamageType.Pure)
        {
            throw new System.NotImplementedException();
        }
    }

}
