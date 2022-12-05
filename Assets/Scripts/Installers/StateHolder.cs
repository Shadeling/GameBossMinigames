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
        [SerializeField]  CurrentStateValue stateValue;

        public SelectableValue CurrentSelected { get { return currentSelected; } }
        public SelectableValue CurrentClicked { get { return currentClicked; } }
        public CurrentStateValue StateValue { get { return stateValue; } }
    }
}
