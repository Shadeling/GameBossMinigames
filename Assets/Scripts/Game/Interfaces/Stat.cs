using System;

namespace MyGame
{

    public enum UnitStat
    {
        HP = 1,

        Strength = 3,       //Способности и здоровье
        Agility = 4,        //Способности и очередность хода
        Intelligence = 5,   //Способности и 

        MovePoints = 6,
        Resistances = 7,


        // Stats for spells
        HealUnit = 100,
        DealDamage = 101,
    }

    public enum DamageType
    {
        None = -1000,
        Pure = 0,

        Fire = 1,
        Frost = 2,
        Physical = 3,
        Electrical = 4,
        Poison = 5,
    }


    [Serializable]
    public struct StatValue
    {
        public UnitStat stat;
        public int value;
    }

    [Serializable]
    public struct ResistanceValue
    {
        public DamageType resistanceType;
        public int change;
    }

    [Serializable]
    public struct StatMultiplier
    {
        public UnitStat stat;
        public float value;
    }

}
