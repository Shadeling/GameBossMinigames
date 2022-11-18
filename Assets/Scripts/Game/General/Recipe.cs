using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{

    [CreateAssetMenu(fileName = "Recipe", menuName = "ScriptableObjects/Recipe", order = 4)]
    public class Recipe : ScriptableObject
    {
        [SerializeField] public string RecipeName;
        [SerializeField] public List<IngredientType> Ingredients;
        [SerializeField] public string Description;
        [SerializeField] public Rarity Rarity;

        [SerializeField] public BaseEquipment Result;

        [SerializeField] public UnlockCondition Unlock;
    }

}
