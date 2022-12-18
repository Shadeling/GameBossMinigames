using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UnityEngine.UI;
using MyGame.Utils;
using System;

namespace MyGame.UI
{

    public class WndUnitStats : ARController
    {
        [Inject] StateHolder state;

        protected override void Init()
        {
            state.SelectedItem.OnNewValue += OnCellClicked;
        }

        private void OnCellClicked(ISelectable selected)
        {
            if (selected as BattleCell != null)
            {
                BattleCell cell = (BattleCell)selected;

                if (!cell.HasUnit)
                {
                    view.Disable();
                }
                else
                {
                    view.Enable();
                    DrawUnitInfo(cell.MyUnit);
                }
            }
        }

        private void DrawUnitInfo(IUnit myUnit)
        {
            
        }
    }
}
