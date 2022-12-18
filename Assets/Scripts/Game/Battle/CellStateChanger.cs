using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{

    /*public static class CellStateChanger
    {
        #region ApplyStates

        public static void ClearHighlight(CellHighlightState state = CellHighlightState.None, bool chechSubstate = true)
        {
            if (state == CellHighlightState.None)
            {
                foreach (var cell in CellsGrid.Values)
                {
                    cell.ChangeState(CellHighlightState.None);
                }
            }
            else
            {
                foreach (var cell in CellsGrid.Values)
                {
                    if (cell.State == state || (chechSubstate && cell.SubState == state))
                        cell.ChangeState(CellHighlightState.None);
                }
            }
        }

        public static void ApplyInFullRadius(BattleCell target, int radius, CellHighlightState cellState, bool isSubstate = false)
        {
            for (int x = target.Position.x - radius; x <= target.Position.x + radius; x++)
            {
                for (int y = target.Position.y - radius; y <= target.Position.y + radius; y++)
                {
                    if (CellsGrid.TryGetValue(new Vector2Int(x, y), out var battleCell))
                    {
                        battleCell.ChangeState(cellState, isSubstate);
                    }
                }
            }
        }

        public static void ApplyInMoveRadius(BattleCell target, int radius, CellHighlightState cellState, bool isSubstate = false)
        {
            for (int x = target.Position.x - radius; x <= target.Position.x + radius; x++)
            {
                for (int y = target.Position.y - radius; y <= target.Position.y + radius; y++)
                {
                    if (CellsGrid.TryGetValue(new Vector2Int(x, y), out var battleCell))
                    {
                        if (CanReach(target, battleCell, radius))
                            battleCell.ChangeState(cellState, isSubstate);
                    }
                }
            }
        }

        public static void ApplyOnZone(BattleCell target, ZoneSO zone, CellHighlightState cellState, bool isSubstate = false)
        {
            Vector2Int offset = new Vector2Int();
            for (int x = 0; x < zone.sizeX; x++)
            {
                for (int y = 0; y < zone.sizeY; y++)
                {
                    if (!zone.fieldStateLinear[x + y * zone.sizeX])
                        continue;

                    offset.Set(target.Position.x + zone.startPoint.x - x, target.Position.y + zone.startPoint.y - y);
                    if (CellsGrid.TryGetValue(offset, out var battleCell))
                    {
                        battleCell.ChangeState(cellState, isSubstate);
                    }
                }
            }
        }

        public static void ApplyInAllDirections(BattleCell target, int minRange, int maxRange, CellHighlightState cellState, bool isSubstate = false)
        {
            Vector2Int offset = new Vector2Int();
            for (int x = target.Position.x - maxRange; x <= target.Position.x + maxRange; x++)
            {
                offset.Set(x, target.Position.y);
                int delta = Mathf.Abs(target.Position.x - x);
                if (delta >= minRange && CellsGrid.TryGetValue(offset, out var battleCell))
                {
                    battleCell.ChangeState(cellState, isSubstate);
                }
            }
        }
        #endregion
    }*/
}
