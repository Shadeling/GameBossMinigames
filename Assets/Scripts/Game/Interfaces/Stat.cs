using System;

namespace MyGame
{

    public enum UnitStat
    {
        HP = 1,
        Damage = 2,

        Strength = 3,       //Способности и здоровье
        Agility = 4,        //Способности и очередность хода
        Intelligence = 5,   //Способности и 

        MovePoints = 6,
        Resistances = 7,
    }

    public enum DamageType
    {
        Pure = -1,

        Fire = 1,
        Frost = 2,
        Physical = 3,
        Electrical = 4,
        Poison = 5,
    }


    [Serializable]
    public struct StatValue
    {
        public string Name;
        public UnitStat stat;
        public float value;
    }

    [Serializable]
    public struct ResistanceValue
    {
        public string Name;
        public DamageType resistanceType;
        public float change;
    }

}
