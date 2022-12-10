using MyGame.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    public class StateHolder : MonoBehaviour
    {
        [SerializeField]  SelectableValue currentSelected;
        [SerializeField]  SelectableValue currentClicked;
        [SerializeField]  SelectableValue currentHover;

        [SerializeField]  SpellValue selectedSpellValue;

        [SerializeField]  CurrentStateValue stateValue;

        public SelectableValue SelectedItem { get { return currentSelected; } }
        public SelectableValue ClickedItem { get { return currentClicked; } }
        public SelectableValue CurrentHover { get { return currentHover; } }
        public SpellValue SelectedSpellValue { get { return selectedSpellValue; } }
        public CurrentStateValue StateValue { get { return stateValue; } }
    }
}
