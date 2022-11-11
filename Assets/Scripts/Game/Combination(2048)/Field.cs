using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class Field : MonoBehaviour
{


    [SerializeField]
    private CustomFieldSO startfield;

    [SerializeField]
    private int startCellsNum;

    private int fieldWidth;

    private int fieldHeigth;

    [SerializeField]
    private int spacing;

    [SerializeField]
    private ItemCell cellPrefab;

    [SerializeField]
    private RectTransform rt;

   

    private ItemCell[,] field;
    private bool anyCellMoved;

    private void Start()
    {
        var rect = GetComponent<RectTransform>();
        fieldWidth = (int)rect.rect.size.x;
        fieldHeigth = (int)rect.rect.size.y;
        GenerateField();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.A))
            OnInput(Vector2.left);
        if (Input.GetKeyDown(KeyCode.D))
            OnInput(Vector2.right);
        if (Input.GetKeyDown(KeyCode.W))
            OnInput(Vector2.up);
        if (Input.GetKeyDown(KeyCode.S))
            OnInput(Vector2.down);
#endif
    }

    private void OnInput(Vector2 direction)
    {
        anyCellMoved = false;

        ResetCellFlags();

        Move(direction);

        if (anyCellMoved)
        {
            GenerateRandomCell();
            CheckGameResult();
        }
    }

    private void Move(Vector2 direction)
    {
        int startXY = direction.x > 0 || direction.y< 0 ? -1 : 0;
        int dir = direction.x != 0 ? (int)direction.x : -(int)direction.y;
        int sizeI = direction.x != 0 ? startfield.sizeX : startfield.sizeY;
        int sizeJ = direction.x != 0 ? startfield.sizeY : startfield.sizeX;

        for (int i = 0; i < sizeI; i++)
        {
            for (int j = startXY; j >=0 && j< sizeJ; j-= dir)
            {
                var index = direction.x != 0 ? new Vector2(j,i) : new Vector2(i, j);

                if (field[(int)index.x, (int)index.y] == null)
                    continue;

                var cell = field[(int)index.x, (int)index.y];

                if (cell.IsEmpty)
                    continue;

                var cellToMerge = FindCellToMerge(cell, direction);
                if(cellToMerge != null)
                {
                    cell.MergeWithCell(cellToMerge);
                    anyCellMoved = true;

                    continue;
                }

                var emptyCell = FindEmptyCell(cell, direction);
                if(emptyCell!= null)
                {
                    cell.MoveToCell(emptyCell);
                    anyCellMoved = true;

                    continue;
                }
            }
        }
    }

    private ItemCell FindCellToMerge(ItemCell cell, Vector2 direction)
    {
        int startX = cell.X + (int)direction.x;
        int startY = cell.Y - (int)direction.y;

        for(int x = startX, y = startY;
            x>= 0 && x<startfield.sizeX && y>=0 && y< startfield.sizeY;
            x+=(int)direction.x, y -= (int)direction.y)
        {
            if (field[x, y] == null)
                break;

            if (field[x, y].IsEmpty)
                continue;

            if (field[x, y].CurrentPower == cell.CurrentPower && !field[x,y].HasMerged)
                return field[x, y];

            break;
        }

        return null;
    }

    private ItemCell FindEmptyCell(ItemCell cell, Vector2 direction)
    {
        ItemCell emptyCell = null;
        int startX = cell.X + (int)direction.x;
        int startY = cell.Y - (int)direction.y;

        for (int x = startX, y = startY;
            x >= 0 && x < startfield.sizeX && y >= 0 && y < startfield.sizeY;
            x += (int)direction.x, y -= (int)direction.y)
        {
            if (field[x, y]!=null && field[x, y].IsEmpty)
                emptyCell = field[x, y];
            else
                break;
        }

        return emptyCell;
    }

    private void CheckGameResult()
    {

    }

    private void GenerateField(bool force = false)
    {
        if (field == null || force)
            CreateField();

        for(int x =0; x<startfield.sizeX; x++)
        {
            for (int y = 0; y < startfield.sizeY; y++)
            {
                field[x, y]?.SetValue(x, y, 0);
            }
        }

        for (int i = 0; i < startCellsNum; i++)
            GenerateRandomCell();
    }

    private void CreateField()
    {
        field = new ItemCell[startfield.sizeX, startfield.sizeY];

        float cellSizeX = fieldWidth / startfield.sizeX - spacing;
        float cellSizeY = fieldHeigth / startfield.sizeY - spacing;
        float cellSize = Mathf.Min(cellSizeX, cellSizeY);

        float startX = -(fieldWidth / 4);
        float startY = (fieldHeigth /2) - cellSize/2 + spacing;

        for(int x=0; x < startfield.sizeX; x++)
        {
            for (int y = 0; y < startfield.sizeY; y++)
            {
                if (startfield.fieldStateLinear[x + y* startfield.sizeX] == false)
                    continue;

                var newCell = Instantiate(cellPrefab, transform, false);
                var pos = new Vector2(startX + (x * (cellSize + spacing)), startY - (y * (cellSize + spacing)));
                newCell.transform.localPosition = pos;
                newCell.RT.sizeDelta = new Vector2(cellSize, cellSize);

                newCell.SetValue(x, y, 0);

                field[x, y] = newCell;
            }
        }
    }

    private void GenerateRandomCell()
    {
        var emptyCells = new List<ItemCell>();

        for (int x = 0; x < startfield.sizeX; x++)
        {
            for (int y = 0; y < startfield.sizeY; y++)
            {
                if (field[x,y]!=null &&  field[x,y].IsEmpty)
                    emptyCells.Add(field[x,y]);
            }
        }

        if(emptyCells.Count == 0)
        {
            throw new System.Exception("No empty cells");
        }

        int value = Random.Range(0, 10) == 0 ? 2 : 1;

        var cell = emptyCells[Random.Range(0, emptyCells.Count)];
        cell.SetValue(cell.X, cell.Y, value, false);

        CellAnimationController.Instance.SmoothAppear(cell);
    }

    private void ResetCellFlags()
    {
        for (int x = 0; x < startfield.sizeX; x++)
        {
            for (int y = 0; y < startfield.sizeY; y++)
            {
                field[x, y]?.ResetFlags();
            }
        }
    }
}