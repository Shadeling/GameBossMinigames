using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{

    public class BattleCell : MonoBehaviour, IBattleCell
    {
        private CellType cellType;
        public CellType CellType { get => cellType; set => cellType = value; }


        private IUnit unit;
        public IUnit MyUnit { get => unit; set => unit = value; }

        public void ApplyEffect(IUnit unit)
        {
            throw new System.NotImplementedException();
        }


    }
}
