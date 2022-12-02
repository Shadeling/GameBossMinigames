
using MyGame.Utils;
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

    public interface IBattleCell : ISelectable
    {
        CellType CellType { get; set; }

        IUnit MyUnit { get; set; }

        Vector2Int Position { get; set; }

        public void ApplyEffect(IUnit unit);
    }

}
