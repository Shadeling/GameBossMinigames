using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{

    public enum IngredientType
    {
        None = -1,

        Wood = 0,
        Stone = 1,
        Iron = 2,
        DragonStone = 3,

    }


    [Serializable]
    public class IngredientInfo
    {
        [SerializeField]
        public IngredientType Type;

        [SerializeField]
        public string Name;

        [SerializeField]
        public string Description;

        [SerializeField]
        public string SpriteName;

        [SerializeField]
        public int ID;

        [SerializeField]
        public Rarity Rarity;
    }

    [CreateAssetMenu(fileName = "IngredientHolderSO", menuName = "ScriptableObjects/IngredientHolder", order = 2)]
    public class IngredientHolder : ScriptableObject
    {
        [SerializeField]
        public List<IngredientInfo> Infos = new List<IngredientInfo>();

        private Dictionary<IngredientType, IngredientInfo> InfoDict = null;

        public bool TryGetInfoByType(IngredientType type, out IngredientInfo info)
        {
            info = null;

            if (InfoDict == null)
                CreateDict();

            if (InfoDict.TryGetValue(type, out var i))
            {
                info = i;
                return true;
            }

            return false;
        }

        private void CreateDict()
        {
            InfoDict = new Dictionary<IngredientType, IngredientInfo>();
            foreach (var info in Infos)
            {
                if (!InfoDict.ContainsKey(info.Type))
                {
                    InfoDict.Add(info.Type, info);
                }
            }
        }
    }
}
