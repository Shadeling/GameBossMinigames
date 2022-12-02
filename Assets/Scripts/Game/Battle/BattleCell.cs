using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using System;

namespace MyGame
{
    [RequireComponent(typeof(BattleCellView))]
    public class BattleCell : MonoBehaviour, IBattleCell//, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private BattleCellView view;

        private CellType cellType;
        public CellType CellType { get => cellType; set => cellType = value; }


        private IUnit unit;
        public IUnit MyUnit { get => unit; set => unit = value; }

        public bool HasUnit => MyUnit != null;

        [SerializeField] private Vector2Int position;
        public Vector2Int Position { get => position; set => position = value; }

        private CellHighlightState state;
        public CellHighlightState State { get => state; }

        private bool selected = false;
        public bool Selected { get => selected; }

        public void Init(Vector2Int position)
        {
            this.position = position;
        }

        public Transform PivotPoint => transform;

        public string Name => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public string SpriteName => throw new NotImplementedException();

        private void OnValidate()
        {
            if(view == null)
            {
                view = GetComponent<BattleCellView>();
            }
        }

        public void ChangeState(CellHighlightState cellState)
        {
            state = cellState;

            view.UpdateUI();
        }

        public void Select(bool select)
        {
            if(selected == select) return;

            selected = select;
            view.UpdateUI();
        }


        public bool AddUnit(IUnit unit)
        {
            if(HasUnit)
                return false;


            this.unit = unit;
            view.UpdateUI();

            return true;
        }

        public void RemoveUnit()
        {
            unit = null;
            view.UpdateUI();
        }

        public void ApplyEffect(IUnit unit)
        {
            throw new System.NotImplementedException();
        }
    }
}
