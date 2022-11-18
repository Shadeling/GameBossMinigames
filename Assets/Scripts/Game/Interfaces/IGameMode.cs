using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    public interface IGameMode
    {
        public bool IsGameStarted { get; set; }
    }

}
