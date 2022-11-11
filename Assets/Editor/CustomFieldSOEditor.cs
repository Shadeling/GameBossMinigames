using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(CustomFieldSO))]
[CanEditMultipleObjects]
public class CustomFieldSOEditor : Editor
{
    CustomFieldSO field = null;

    void OnEnable()
    {
        field = (CustomFieldSO)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();
        field.sizeX = EditorGUILayout.IntField(field.sizeX, GUILayout.Width(30));
        field.sizeY = EditorGUILayout.IntField(field.sizeY, GUILayout.Width(30));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField(" X         Y");

        if (field.fieldStateLinear == null || field.fieldStateLinear.Count!= field.sizeX*field.sizeY )
        {
            ResetField();
        }

        EditorGUILayout.Space(20);

        for (int x = 0; x < field.sizeX; x++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int y = 0; y < field.sizeY; y++)
            {
                //field.fieldState[x][y] = EditorGUILayout.Toggle(field.fieldState[x][y], GUILayout.Width(15));
                field.fieldStateLinear[x+y*field.sizeX] = EditorGUILayout.Toggle(field.fieldStateLinear[x + y * field.sizeX], GUILayout.Width(15));
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Reset to 1", GUILayout.Width(100)))
        {
            ResetField(true);
        }

        if (GUILayout.Button("Reset to 0", GUILayout.Width(100)))
        {
            ResetField(true, false);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(field);
            Undo.RecordObject(field, "Changed field");
        }

        serializedObject.ApplyModifiedProperties();
    }


    private void ResetField(bool force = false, bool value = true)
    {
        var old = field.fieldStateLinear;

        field.fieldStateLinear = new List<bool>();

        for (int x = 0; x < field.sizeX; x++)
        {
            for (int y = 0; y < field.sizeY; y++)
            {

                if(!force && TryGetOldValue(old, x, y, out var oldBool))
                {
                    field.fieldStateLinear.Add(oldBool);
                }
                else
                {
                    field.fieldStateLinear.Add(value);
                }
            }
        }
    }

    private bool TryGetOldValue(List<bool> oldList, int x, int y, out bool result)
    {
        result = false;

        if (oldList == null)
        {
            return false;
        }

        if(oldList.Count ==0 || oldList.Count != field.sizeY * field.sizeX)
        {
            return false;
        }

        result = oldList[x+y*field.sizeX];

        return true;
    }
}
