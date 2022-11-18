using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MyGame
{
    public interface IVisualizable
    {
        public string Name { get; }
        public string Description { get; }
        public string SpriteName { get; }
    }
}
