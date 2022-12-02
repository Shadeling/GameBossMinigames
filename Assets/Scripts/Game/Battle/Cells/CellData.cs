using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame {

    public enum CellHighlightState
    {
        None = -1,
        MoveHighlight = 0,
        EnemyMoveHighlight = 1,
        RangeHighlight = 2,
        SpellZoneHighlight = 3,
    }

    [Serializable]
    public struct EventColor
    {
        public CellHighlightState CellEvent;
        public Color Color;
    }

    public class CellData : MonoBehaviour
    {
        public static CellData Instance { get; private set; }

        [SerializeField] List<EventColor> eventColors;
        private static Dictionary<CellHighlightState, Color> eventColorsDict = new Dictionary<CellHighlightState, Color>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            eventColorsDict.Clear();
            foreach (var ev in eventColors)
            {
                eventColorsDict.Add(ev.CellEvent, ev.Color);
            }
        }

        public static Color GetStateColor(CellHighlightState state)
        {
            if(eventColorsDict.TryGetValue(state, out var color)){
                return color;
            }
            else
            {
                return Color.clear;
            }
        }

    }


}