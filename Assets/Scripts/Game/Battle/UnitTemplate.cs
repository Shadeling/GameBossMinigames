using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    [CreateAssetMenu(fileName = "UnitTemplate", menuName = "ScriptableObjects/Units/UnitTemplate", order = 1)]
    public class UnitTemplate : ScriptableObject, IUnitStats
    {
        [SerializeField]
        private string unitName;
        public string Name => unitName;

        [SerializeField]
        private string unitDescription;
        public string Description => unitDescription;


        [SerializeField]
        private string unitSpriteName;
        public string SpriteName => unitSpriteName;

        [SerializeField]
        private bool controlledByPlayer;
        public bool ControlledByPlayer { get => controlledByPlayer; set => controlledByPlayer = value; }


        [Header("Stats and spells")]

        [SerializeField]
        private UnitClass unitClass;
        public UnitClass UnitClass => unitClass;

        [SerializeField]
        private List<ISpell> baseSpells;
        public List<ISpell> Spells => baseSpells;

        [SerializeField]
        private List<IPassive> basePassives;
        public List<IPassive> Passives => basePassives;

        [SerializeField]
        private List<BaseEquipment> startEquipment;
        public List<BaseEquipment> Equipment => startEquipment;

        [SerializeField]
        private List<StatValue> stats;

        [SerializeField]
        private List<ResistanceValue> resistances;

        public List<IBuff> Buffs => new List<IBuff>();

    }

}
