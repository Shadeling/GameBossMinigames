using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    public class Ingredient : IItem
    {
        public IngredientInfo Info { get; private set; }

        public int Level { get; private set; }

        public ItemType ItemType { get => ItemType.Ingredient; }

        public string Name => Info.Name;

        public string Description => Info.Description;

        public int ID => Info.ID;

        public Rarity Rarity => Info.Rarity;

        public string SpriteName => Info.SpriteName;

        public Ingredient(IngredientInfo info, int level)
        {
            Info = info;
            Level = level;
        }
    }
}
