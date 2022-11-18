using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CustomField", menuName = "ScriptableObjects/CustomField2048", order = 1)]
public class ZoneSO : ScriptableObject
{
    [SerializeField]
    public int sizeX = 1;

    [SerializeField]
    public int sizeY = 1;

    /*[SerializeField]
    public List<List<bool>> fieldState;*/

    [SerializeField]
    public List<bool> fieldStateLinear;
}
