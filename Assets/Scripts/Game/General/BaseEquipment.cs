using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{

    [CreateAssetMenu(fileName = "BaseEquipment", menuName = "ScriptableObjects/Equipment/BaseEquipment", order = 1)]
    public class BaseEquipment : ScriptableObject, IItem
    {
        public ItemType ItemType => ItemType.Equipment;

        [SerializeField]
        private string name;
        public string Name => name;

        [SerializeField]
        private string description;
        public string Description => description;


        [SerializeField]
        private string spriteName;
        public string SpriteName => spriteName;


        [SerializeField]
        private int id;
        public int ID => id;

        [SerializeField]
        private Rarity rarity;
        public Rarity Rarity => rarity;

        [Header("Stats and bonuses")]

        [SerializeField]
        private List<StatValue> statChanges;
        public List<StatValue> StatChanges { get { return statChanges; } }

        [SerializeField]
        private List<ResistanceValue> resistanceChanges;
        public List<ResistanceValue> ResistanceChanges { get { return resistanceChanges; } }

        [SerializeField]
        private List<PassiveBase> givePassives;
        public List<PassiveBase> GivePassives { get { return givePassives; } }

        [SerializeField]
        private List<SpellBase> giveSpells;
        public List<SpellBase> GiveSpells { get { return giveSpells; } }

    }

}
