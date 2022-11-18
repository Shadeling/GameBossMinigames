using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{

    [Serializable]
    public struct IngredientDistribution
    {
        [SerializeField]
        public IngredientType Type;
        [SerializeField]
        public float Probability;
    }



    [CreateAssetMenu(fileName = "ResourceLocation", menuName = "ScriptableObjects/Locations/ResourceLocation", order = 1)]
    public class ResourceLocation : LocationBase
    {

        [SerializeField]
        private List<IngredientDistribution> locationIngredients;

        public List<IngredientDistribution> LocationIngredients { get { return locationIngredients; } }
    }

}
