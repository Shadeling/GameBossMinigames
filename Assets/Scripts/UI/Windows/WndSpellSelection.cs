using MyGame.Utils;
using System.Collections.Generic;
using Zenject;
using UnityEngine;

namespace MyGame.UI
{

    public class WndSpellSelection : ARController
    {
        [SerializeField] RectTransform itemsParent;

        [Inject] StateHolder state;

        private List<ItemSpell> currentSpells = new List<ItemSpell>();

        private IUnit currentUnit;


        protected override void Init()
        {
            state.SelectedItem.OnNewValue += OnCellClicked;

            currentSpells.Clear();
        }

        private void OnCellClicked(ISelectable selected)
        {
            if (selected as BattleCell != null)
            {
                BattleCell cell = (BattleCell)selected;
                currentUnit = cell.MyUnit;

                if (!cell.HasUnit)
                    view.Disable();
                else
                    DrawUnitInfo(cell.MyUnit);
            }
        }

        private void DrawUnitInfo(IUnit unit)
        {
            while(unit.Spells.Count > currentSpells.Count)
            {
                var spell = (ItemSpell)this.AddChild(PSYItem.ItemSpell);
                spell.view.index = currentSpells.Count;
                spell.transform.SetParent(itemsParent);
                currentSpells.Add(spell);
            }

            for(int i = 0; i < currentSpells.Count; i++)
            {
                if (unit.Spells.Count > i)
                    currentSpells[i].Setup(new PSYParams(unit.Spells[i]));
                else
                    currentSpells[i].Setup(new PSYParamsEmpty());
            }
        }

        private void SetupSpellStats(ISpell spell)
        {

        }

        public override void PSYEventHandler(PSYEvent e, PSYParams param)
        {
            switch (e)
            {
                case PSYEvent.SpellClicked:
                    SetupSpellStats(param.Get<ISpell>());
                    break;
            }
        }

        protected override void OnChildClick(PSYViewEventArgs e)
        {
            switch (e.item)
            {
                case PSYItem.ItemSpell:
                    SetupSpellStats(currentUnit.Spells[e.index]);
                    break;
            }
        }

    }
}
