using UnityEditor;
using UnityEngine;

namespace MyGame {

    [CustomEditor(typeof(BattleField))]
    public class BattleFieldEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var field = target as BattleField;

            DrawDefaultInspector();

            if (GUILayout.Button("Create"))
            {
                field.CreateGrid();
            }

            if (GUILayout.Button("Clear"))
            {
                field.Clear();
            }
        }
    }
}
