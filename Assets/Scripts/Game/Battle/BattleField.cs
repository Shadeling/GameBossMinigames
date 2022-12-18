using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame
{
    [RequireComponent(typeof(BattleStateController))]  
    public class BattleField : MonoBehaviour
    {

        [SerializeField] private Vector2Int size = new Vector2Int (10, 10);
        [SerializeField] private Vector2Int spacing = new Vector2Int(5, 5);
        [SerializeField] private RectTransform rootTr;
        [SerializeField] private GridLayoutGroup grid;

        [SerializeField] private BattleCell cellPrefab;

        [SerializeField, HideInInspector] private List<BattleCell> cellList = new List<BattleCell>();

        [SerializeField] private BattleStateController controller;

        private void OnValidate()
        {
            if (rootTr == null)
            {
                rootTr = GetComponent<RectTransform>();
            }

            if(controller == null)
            {
                controller = GetComponent<BattleStateController>();
            }
        }


        private void Awake()
        {
            Clear();

            if (cellList.Count == 0)
            {
                CreateGrid();
            }

            controller.Init(cellList);
        }

        

        [ContextMenu("Create")]
        public void CreateGrid()
        {
            Clear();

            var fieldWidth = (int)rootTr.rect.size.x;
            var fieldHeigth = (int)rootTr.rect.size.y;
            Vector2 cellSize = Vector2.one;
            cellSize.x = fieldWidth / size.x - spacing.x;
            cellSize.y = fieldHeigth / size.y - spacing.y;
            grid.cellSize = cellSize;
            grid.spacing = spacing;
            grid.constraintCount = size.x;


            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    BattleCell newCell = Instantiate(cellPrefab, rootTr);
                    newCell.Init(new Vector2Int(x, y));

                    cellList.Add(newCell);
                }
            }
        }

        [ContextMenu("Clear")]
        public void Clear()
        {
            foreach(BattleCell cell in cellList)
            {
                DestroyImmediate(cell.gameObject);
            }
            cellList.Clear();
        }
    }
}
