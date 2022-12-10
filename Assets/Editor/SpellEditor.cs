using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MyGame
{

    [CustomEditor(typeof(SpellBase))]
    [CanEditMultipleObjects]
    public class SpellEditor : Editor
    {

        SpellBase spell;

        void OnEnable()
        {
            spell = (SpellBase)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }

}
