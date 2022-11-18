using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{

    public class LocationBase : ScriptableObject, ILocation
    {
        [SerializeField]
        private LocationType type;

        [SerializeField]
        private string name;

        [SerializeField]
        private string description;

        [SerializeField]
        private string spriteName;

        [SerializeField]
        private int locationLevel = 1;

        [SerializeField]
        private float locationFrequency = 1;

        

        public virtual LocationType Type => type;

        public int LocationLevel => locationLevel;

        public float LocationFrequency => locationFrequency;

        public string Name => name;

        public string Description => description;

        public string SpriteName => spriteName;
    }
}
