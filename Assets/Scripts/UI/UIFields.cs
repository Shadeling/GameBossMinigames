using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIFields : MonoBehaviour
{
    [SerializeField] private GraphicRaycaster graphicRaycaster;
    [SerializeField] private EventSystem eventSystem;

    public GraphicRaycaster GraphicRaycaster { get { return graphicRaycaster; } }
    public EventSystem EventSystem { get { return eventSystem; } }

}
