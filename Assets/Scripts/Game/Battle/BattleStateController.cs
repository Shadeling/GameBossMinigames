using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGame.Utils;
using Zenject;
using MyGame.UI;
using System.Numerics;
using System;
using System.Linq;


namespace MyGame
{

    public class BattleStateController : MonoBehaviour
    {
        [Inject] StateHolder State;

        [SerializeField] UnitTemplate unitTemplate;

        private CurrentState currentState { get { return State.StateValue.CurrentValue; } }

        private ISpell currentSpell { get { return State.SelectedSpellValue.CurrentValue; } }



        private Dictionary<Vector2Int, BattleCell> CellsGrid = new Dictionary<Vector2Int, BattleCell>();

        private List<IUnit> unitsInGame = new List<IUnit>();
        private int currentUnitNum;

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
            unitsInGame.Add(baseUnit);

            var r = UnityEngine.Random.Range(0, newCells.Count);
            Vector2Int newPos = newCells[r].Position;
            CellsGrid[newPos].AddUnit(baseUnit);

            currentUnitNum = unitsInGame.Count;
        }

        private void GameTurnsLoop()
        {
            EndGameCheck();

            if (currentUnitNum >= unitsInGame.Count)
            {
                unitsInGame.RemoveAll(x => !x.IsAlive);
                foreach (var unit in unitsInGame)
                {
                    unit.OnTurnEnd();
                }
                unitsInGame.OrderByDescending(x => x.GetStat(UnitStat.Agility));
                currentUnitNum = 0;
            }

            unitsInGame[currentUnitNum].MyTurn = true;
            if (!unitsInGame[currentUnitNum].ControlledByPlayer)
            {
                //Call bot logic
            }
            currentUnitNum++;
        }

        private void EndGameCheck()
        {

        }



        private void OnSelected(ISelectable selectable)
        {
            BattleCell battleCell = selectable as BattleCell;
            if (battleCell)
            {
                switch (currentState)
                {
                    case CurrentState.SpellSelected:
                        if(currentSpell != null && battleCell.State == CellHighlightState.RangeHighlight)
                        {
                            currentSpell.OnCast(selectedCell.MyUnit);
                            ApplyOnZoneWithRotation(battleCell, currentSpell.Zone, (pos) => ApplySpellEffect(pos, selectedCell.MyUnit, currentSpell));
                            ClearHighlight();
                        }
                        else
                        {
                            OnCellSelected(battleCell);
                        }
                        break;

                    default:
                        OnCellSelected(battleCell);
                        break;
                }
            }
            else
            {
                SelectedCell = null;
            }
        }

        private void OnCellSelected(BattleCell cell)
        {
            SelectedCell = cell;
            DrawMoveColors();
        }

        private void ApplySpellEffect(Vector2Int cellPos, IUnit caster, ISpell spell)
        {
            if (caster == null || spell == null)
                return;

            if(CellsGrid.TryGetValue(cellPos, out var cell))
            {
                if (cell.HasUnit)
                {
                    if (cell.MyUnit.ControlledByPlayer == caster.ControlledByPlayer)
                    {
                        spell.OnTargetAlly(cell.MyUnit);
                    }
                    else
                    {
                        spell.OnTargetEnemy(cell.MyUnit);
                    }
                }
                spell.OnTargetTiles(cell);
            }

        }

        private void ChangeState(Vector2Int position, CellHighlightState cellState, bool isSubstate = false)
        {
            if (CellsGrid.TryGetValue(position, out var battleCell))
            {
                battleCell.ChangeState(cellState, isSubstate);
            }
        }

        private void OnClickedRMB(ISelectable selectable)
        {
            BattleCell clickedCell = selectable as BattleCell;
            if (clickedCell)
            {
                switch (currentState)
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
            var spell = currentSpell;
            ClearHighlight(CellHighlightState.SpellZoneHighlight, true);
            if (cell && spell != null && cell.State == CellHighlightState.RangeHighlight)
            {
                ApplyOnZoneWithRotation(cell, spell.Zone, (pos) => { ChangeState(pos, CellHighlightState.SpellZoneHighlight, true); });
            }
        }

        private void OnSpellSelect(ISpell spell)
        {
            ClearHighlight();

            if (spell != null)
            {
                int minRange = SpellBase.FindFinalStatChange(spell.MinDistance, selectedCell.MyUnit);
                int maxRange = SpellBase.FindFinalStatChange(spell.MaxDistance, selectedCell.MyUnit);
                ApplyInAllDirections(selectedCell, minRange, maxRange, (pos) => { ChangeState(pos, CellHighlightState.RangeHighlight); });
                State.StateValue.SetValue(CurrentState.SpellSelected);
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

        #region ApplyStates

        public delegate void CellAction(Vector2Int position);

        public void ClearHighlight(CellHighlightState state = CellHighlightState.None, bool substate = false)
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
                    if (cell.State == state || (substate && cell.SubState == state))
                        cell.ChangeState(CellHighlightState.None, substate);
                }
            }
        }

        public void ApplyInFullRadius(BattleCell target, int radius, CellAction action)
        {
            Vector2Int offset = new Vector2Int();
            for (int x = target.Position.x - radius; x <= target.Position.x + radius; x++)
            {
                for (int y = target.Position.y - radius; y <= target.Position.y + radius; y++)
                {
                    offset.Set(x, y);
                    action?.Invoke(offset);
                }
            }
        }

        public void ApplyInMoveRadius(BattleCell target, int radius, CellHighlightState cellState, bool isSubstate = false)
        {

            for (int x = target.Position.x - radius; x <= target.Position.x + radius; x++)
            {
                for (int y = target.Position.y - radius; y <= target.Position.y + radius; y++)
                {
                    if (CellsGrid.TryGetValue(new Vector2Int(x, y), out var battleCell))
                    {
                        if(CanReach(target, battleCell, radius))
                            battleCell.ChangeState(cellState, isSubstate);
                    }
                }
            }
        }

        public void ApplyOnZone(BattleCell target, ZoneSO zone, CellAction action)
        {
            Vector2Int offset = new Vector2Int();
            for(int x =0; x<zone.sizeX; x++)
            {
                for (int y = 0; y < zone.sizeY; y++)
                {
                    if (!zone.fieldStateLinear[x+y*zone.sizeX])
                        continue;

                    offset.Set(target.Position.x + zone.startPoint.x - x, target.Position.y + zone.startPoint.y - y);
                    action?.Invoke(offset);
                }
            }
        }


        public void ApplyOnZoneWithRotation(BattleCell target, ZoneSO zone, CellAction action)
        {
            Vector2Int direction = selectedCell.Position - target.Position;
            var rotationAngle = direction.x == 0 ? 0 : direction.x > 0 ? 270 : 90;
            rotationAngle += direction.y <= 0 ?  0 : 180;
            var rot = Matrix3x2.CreateRotation(-rotationAngle * (float)(Math.PI / 180f));

            Vector2Int offset = new Vector2Int();
            for (int x = 0; x < zone.sizeX; x++)
            {
                for (int y = 0; y < zone.sizeY; y++)
                {
                    if (!zone.fieldStateLinear[x + y * zone.sizeX])
                        continue;

                    var vector = new System.Numerics.Vector2(zone.startPoint.x - x, zone.startPoint.y - y);
                    var vectorRotated = System.Numerics.Vector2.Transform(vector, rot);

                    offset.Set(target.Position.x + (int)vectorRotated.X, target.Position.y + (int)vectorRotated.Y);
                    action?.Invoke(offset);
                }
            }
        }

        public void ApplyInAllDirections(BattleCell target, int minRange, int maxRange, CellAction action)
        {
            Vector2Int offset = new Vector2Int();
            for (int x = target.Position.x - maxRange; x <= target.Position.x + maxRange; x++)
            {
                offset.Set(x, target.Position.y);
                int delta = Mathf.Abs(target.Position.x - x);
                if (delta >= minRange)
                {
                    action?.Invoke(offset);
                }
            }

            for (int y = target.Position.y - maxRange; y <= target.Position.y + maxRange; y++)
            {
                offset.Set(target.Position.x, y);
                int delta = Mathf.Abs(target.Position.y - y);
                if (delta >= minRange)
                {
                    action?.Invoke(offset);
                }
            }
        }
        #endregion

        /*public void PSYEventHandler(PSYEvent e, PSYParams param)
        {
            switch (e)
            {
                
            }
        }*/

    }

}
