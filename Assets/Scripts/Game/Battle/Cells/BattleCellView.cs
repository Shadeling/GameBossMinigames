using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame {

    public class BattleCellView : MonoBehaviour
    {
        [SerializeField] Image highlightImage;

        [SerializeField] Image selectedFrame;

        [SerializeField] Image unitImage;

        [SerializeField] SpriteRenderer locationImage;

        [SerializeField] private BattleCell cell;


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


            highlightImage.color = CellData.GetStateColor(cell.State);

            // Unit UI
            if (cell.HasUnit)
            {
                unitImage.color = Color.cyan;
            }
            else
            {
                unitImage.color = Color.clear;
            }
        }
    }

}