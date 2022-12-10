using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGame.Utils;
using Zenject;
using MyGame.UI;

namespace MyGame
{

    public class BattleStateController : MonoBehaviour
    {
        [Inject] StateHolder State;

        [SerializeField] UnitTemplate unitTemplate;



        private Dictionary<Vector2Int, BattleCell> CellsGrid = new Dictionary<Vector2Int, BattleCell>();

        private List<IUnit> activeUnits = new List<IUnit>();

        private BattleCell selectedCell;
        private BattleCell SelectedCell {
            get { return selectedCell; } 
            set
            {
                if (selectedCell == value)
                    return;

                if (selectedCell != null)
                    selectedCell.Select(false);

                selectedCell = value;

                if(selectedCell == null)
                {
                    State.StateValue.SetValue(CurrentState.None);
                }
                if (selectedCell != null)
                {
                    selectedCell.Select(true);

                    if (selectedCell.HasUnit)
                        State.StateValue.SetValue(CurrentState.CellWithUnitSelected);
                    else
                        State.StateValue.SetValue(CurrentState.CellWithoutUnitSelected);
                }
            }
        }

        public void Init(List<BattleCell> newCells)
        {
            //MainUIManager.AddSubscriber(this);

            CellsGrid.Clear();
            foreach (BattleCell cell in newCells)
            {
                CellsGrid.Add(cell.Position, cell);
            }

            State.StateValue.SetValue(CurrentState.None);
            State.SelectedItem.OnNewValue += OnSelected;
            State.ClickedItem.OnNewValue += OnClickedRMB;
            State.CurrentHover.OnNewValue += OnCellWithSpellHover;

            State.SelectedSpellValue.OnNewValue += OnSpellSelect;


            UnitBase baseUnit = new UnitBase(unitTemplate); 
            activeUnits.Add(baseUnit);

            var r = Random.Range(0, newCells.Count);
            Vector2Int newPos = newCells[r].Position;
            CellsGrid[newPos].AddUnit(baseUnit);
        }



        private void OnSelected(ISelectable selectable)
        {
            BattleCell battleCell = selectable as BattleCell;
            if (battleCell)
            {
                SelectedCell = battleCell;

                DrawMoveColors();
                //Draw area, update UI
            }
            else
            {
                SelectedCell = null;
            }
        }

        private void OnClickedRMB(ISelectable selectable)
        {
            BattleCell clickedCell = selectable as BattleCell;
            if (clickedCell)
            {
                switch (State.StateValue.CurrentValue)
                {

                    case CurrentState.CellWithUnitSelected:
                        int moves = (int)SelectedCell.MyUnit.GetStat(UnitStat.MovePoints);
                        if (!CanReach(clickedCell, SelectedCell, moves))
                            return;

                        if (clickedCell.AddUnit(SelectedCell.MyUnit))
                        {
                            SelectedCell.RemoveUnit();
                            SelectedCell = clickedCell;
                            DrawMoveColors();
                        }
                        break;


                    default:
                        break;
                }
            }

        }

        private void OnCellWithSpellHover(ISelectable selectable)
        {
            BattleCell cell = selectable as BattleCell;
            var spell = State.SelectedSpellValue.CurrentValue;
            if (cell && spell != null)
            {
                Debug.LogWarning($"Hover on cell x={cell.Position.x}, y={cell.Position.y}");
                ClearHighlight(CellHighlightState.SpellZoneHighlight);
                ApplyOnZone(cell, spell.Zone, CellHighlightState.SpellZoneHighlight);
            }
        }

        private void OnSpellSelect(ISpell spell)
        {
            ClearHighlight();
            if (spell != null)
            {
                State.StateValue.SetValue(CurrentState.SpellSelected);
            }
        }

        public void ClearHighlight(CellHighlightState state = CellHighlightState.None)
        {
            if(state == CellHighlightState.None)
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
                    if(cell.State == state)
                        cell.ChangeState(CellHighlightState.None);
                }
            }
        }

        private void DrawMoveColors()
        {
            ClearHighlight();


            if (SelectedCell && SelectedCell.HasUnit)
            {
                var evType = SelectedCell.MyUnit.ControlledByPlayer ? CellHighlightState.MoveHighlight : CellHighlightState.EnemyMoveHighlight;
                ApplyInMoveRadius(SelectedCell, (int)SelectedCell.MyUnit.GetStat(UnitStat.MovePoints), evType);
            }
        }

        private int DistanceBetween(BattleCell first, BattleCell second)
        {
            var x = Mathf.Abs(first.Position.x - second.Position.x);
            var y = Mathf.Abs(first.Position.y - second.Position.y);
            return x + y;
        }

        private bool CanReach(BattleCell first, BattleCell second, int movePoints)
        {
            return DistanceBetween(first, second) <= movePoints;
        }


        public void ApplyInFullRadius(BattleCell target, int radius, CellHighlightState cellState)
        {
                for(int x = target.Position.x - radius; x <= target.Position.x + radius; x++)
                {
                    for (int y = target.Position.y - radius; y <= target.Position.y + radius; y++)
                    {
                        if(CellsGrid.TryGetValue(new Vector2Int(x, y), out var battleCell))
                        {
                            battleCell.ChangeState(cellState);
                        }
                    }
                }
        }

        public void ApplyInMoveRadius(BattleCell target, int radius, CellHighlightState cellState)
        {
            for (int x = target.Position.x - radius; x <= target.Position.x + radius; x++)
            {
                for (int y = target.Position.y - radius; y <= target.Position.y + radius; y++)
                {
                    if (CellsGrid.TryGetValue(new Vector2Int(x, y), out var battleCell))
                    {
                        if(CanReach(target, battleCell, radius))
                            battleCell.ChangeState(cellState);
                    }
                }
            }
        }

        public void ApplyOnZone(BattleCell target, ZoneSO zone, CellHighlightState cellState)
        {
            Vector2Int offset = new Vector2Int();
            for(int x =0; x<zone.sizeX; x++)
            {
                for (int y = 0; y < zone.sizeY; y++)
                {
                    if (!zone.fieldStateLinear[x+y*zone.sizeX])
                        continue;

                    offset.Set(target.Position.x + zone.startPoint.x - x, target.Position.y + zone.startPoint.y - y);
                    if (CellsGrid.TryGetValue(offset, out var battleCell))
                    {
                        battleCell.ChangeState(cellState);
                    }
                }
            }
        }

        /*public void PSYEventHandler(PSYEvent e, PSYParams param)
        {
            switch (e)
            {
                case PSYEvent.SpellTargetCellSelected:
                    var cell = param.Get<BattleCell>();
                    if(cell != null && selectedSpell!=null)
                    {
                        Debug.LogWarning($"Hover on cell x={cell.Position.x}, y={cell.Position.y}");
                        ClearHighlight(CellHighlightState.SpellZoneHighlight);
                        ApplyOnZone(cell, selectedSpell.Zone, CellHighlightState.SpellZoneHighlight);
                    }
                    break;

                case PSYEvent.SpellClicked:
                    ClearHighlight();
                    selectedSpell = param.Get<ISpell>();
                    if(selectedSpell != null)
                    {
                        State.StateValue.SetValue(CurrentState.SelectSpell);
                    }
                    break;
            }
        }*/

        private void OnDestroy()
        {
            //MainUIManager.RemoveSubscriber(this);
        }
    }

}
