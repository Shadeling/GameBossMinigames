using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{

    public enum CellType
    {
        None = 0,
        Forest = 1,
        Mountain = 2,
        River = 3,
    }

    public interface IBattleCell
    {
        CellType CellType { get; set; }

        IUnit MyUnit { get; set; }


        public void ApplyEffect(IUnit unit);
    }

}
