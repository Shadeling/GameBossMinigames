using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame {

    public class BattleCellView : MonoBehaviour
    {
        [SerializeField] private BattleCell cell;

        [SerializeField] Image selectedFrame;
        [SerializeField] Image locationImage;

        [Space(15), SerializeField] Image stateGraphics;
        [SerializeField] Image subStateGraphics;

        [Space(15), Header("UnitData"), SerializeField]
        RectTransform unitRoot;

        [SerializeField] Image unitImage;

        [SerializeField] Text UnitHP;

        

        private void OnValidate()
        {
            if (cell == null)
            {
                cell = GetComponent<BattleCell>();
            }
        }

        public void UpdateUI()
        {
            if (cell.Selected)
                selectedFrame.color = selectedFrame.color.SetAlpha(1);
            else
                selectedFrame.color = selectedFrame.color.SetAlpha(0);

            // Unit UI
            if (cell.HasUnit)
            {
                unitRoot.gameObject.SetActive(true);
                unitImage.color = Color.cyan;
                UnitHP.text = cell.MyUnit.GetStat(UnitStat.HP).ToString();
            }
            else
            {
                unitRoot.gameObject.SetActive(false);
            }
        }

        public void UpdateStates()
        {
            stateGraphics.color = CellData.GetStateColor(cell.State);

            subStateGraphics.color = CellData.GetStateColor(cell.SubState);
        }
    }

}