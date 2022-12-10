using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MyGame
{


    /*[CustomPropertyDrawer(typeof(SpellStatChange))]
    public class SpellStatDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;


            if((UnitStat) property.FindPropertyRelative("statToChange").intValue != UnitStat.Resistances) {
                var statToChangeRect = new Rect(position.x, position.y, 150, position.height);

                EditorGUI.PropertyField(statToChangeRect, property.FindPropertyRelative("statToChange"), GUIContent.none);
            }
            else
            {
                var statToChangeRect = new Rect(position.x, position.y, 150, position.height);
                var resistanceTypeRect = new Rect(position.x + 200, position.y, position.width - 200, position.height);

                EditorGUI.PropertyField(statToChangeRect, property.FindPropertyRelative("statToChange"), GUIContent.none);
                EditorGUI.PropertyField(resistanceTypeRect, property.FindPropertyRelative("resistanceType"), GUIContent.none);
         
            }

            var changeRect = new Rect(position.x, position.y + position.height, position.width, position.height);
            EditorGUI.ObjectField(changeRect, property.FindPropertyRelative("change"), GUIContent.none);

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
            position = new Rect(position.x, position.y + position.height, position.width, position.height);

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.standardVerticalSpacing;

            return totalHeight * 4;
        }
    }*/


    [CustomPropertyDrawer(typeof(StatValue))]
    public class StatValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 1;

            var statToChangeRect = new Rect(position.x, position.y, 190, position.height);
            var changeRect = new Rect(position.x + 200, position.y, position.width - 200, position.height);

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(statToChangeRect, property.FindPropertyRelative("stat"), GUIContent.none);
            EditorGUI.IntField(changeRect, property.FindPropertyRelative("value").intValue);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(ResistanceValue))]
    public class ResistanceValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var statToChangeRect = new Rect(position.x, position.y, 190, position.height);
            var changeRect = new Rect(position.x + 200, position.y, position.width - 200, position.height);

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(statToChangeRect, property.FindPropertyRelative("resistanceType"), GUIContent.none);
            EditorGUI.IntField(changeRect, property.FindPropertyRelative("change").intValue);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(StatMultiplier))]
    public class StatMultiplierDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 1;

            var statToChangeRect = new Rect(position.x, position.y, 190, position.height);
            var changeRect = new Rect(position.x + 200, position.y, position.width - 200, position.height);

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(statToChangeRect, property.FindPropertyRelative("stat"), GUIContent.none);
            EditorGUI.FloatField(changeRect, property.FindPropertyRelative("value").floatValue);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}