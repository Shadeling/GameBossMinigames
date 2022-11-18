using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    public enum ItemType
    {
        None = -1,
        Ingredient = 0,
        Equipment = 1,
        Character = 2,

    }

    public enum Rarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
        Ultra = 5,
    }

    public interface IItem : IVisualizable
    {
        public ItemType ItemType { get; }

        public int ID { get; }

        public Rarity Rarity { get; }

    }
}
