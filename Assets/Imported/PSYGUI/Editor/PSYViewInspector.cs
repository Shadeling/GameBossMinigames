using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(PSYView))]
public class PSYViewInspector : Editor
{
	PSYView view = null;
    private bool updateDone = false;
    private bool events = false;
    private List<PSYViewEvent> allEvents = PSYTools.EnumToObjectList<PSYViewEvent>(typeof(PSYViewEvent));
    private List<PSYAnimationEvent> allAnimationEvents = PSYTools.EnumToObjectList<PSYAnimationEvent>(typeof(PSYAnimationEvent));

    private int indexPopupWidgets=0;
    private int indexPopupChild = 0;
    //Принудительно, по кнопке update, перезачитать список дочерних виджетов -> view.Widgets, если !Application.isPlaying && view.type != PSYViewType.Root
    private bool forcedUpdateWidgets = false;

    public override void OnInspectorGUI()
    {
        GUI.changed = false;

        if (target as PSYView != view)
        {
            updateDone = false;
        }

        view = target as PSYView;
        if (view.GetComponent<RectTransform>() == null)
        {
            view.gameObject.AddComponent<RectTransform>();
        }


        if (((forcedUpdateWidgets) || (!updateDone)) && !Application.isPlaying && view.type != PSYViewType.Root)
        {
            //Debug.LogError("Reset forcedUpdateWidgets=" + forcedUpdateWidgets);
            updateDone = true;

            PSYTools.UpdateLitsWidgetAndChild(view);

            if (forcedUpdateWidgets)
            {
                forcedUpdateWidgets = false;
                
                string strMessage = "Выполнено обновление списков view.Widgets и view.Child у всех виджетов с компонентом PSYView";
                strMessage += "\n";
                strMessage += "\n";
                strMessage += "Состояние текущего(выделенного) виджета";
                strMessage += "\n";
                strMessage += getChildrenWidgetStr(5);
                strMessage += "\n";
                strMessage += getWidgetsStr(40);
                strMessage += "\n";
                strMessage += "Сохраните изменения префаба Ctrl+s";

                EditorUtility.DisplayDialog("Обновление списка дочерних widget-ов", strMessage, "Ok");
            }
            forcedUpdateWidgets = false;

        }
        else if (forcedUpdateWidgets)
        {
            forcedUpdateWidgets = false;
            string strTmp2 = "Список не перезачитан, т.к.";
            if (view.type == PSYViewType.Root)
            {
                strTmp2 += "\n view.type == PSYViewType.Root";
            }

            if (Application.isPlaying)
            {
                strTmp2 += "\n Application.isPlaying=true";
            }

            EditorUtility.DisplayDialog("Принудительное обновление списка дочерних widget-ов", strTmp2, "Ok");
        }

        GUIStyle helpStyle = new GUIStyle(GUI.skin.button);
        helpStyle.wordWrap = true;
        helpStyle.alignment = TextAnchor.UpperLeft;
        Color c = Color.white;
        c.a = 0.75f;
        helpStyle.normal.textColor = c;

        //EditorUtility.SetDirty(target);

        PSYGUIAnimation.DrawSeparator();
        GUILayout.BeginHorizontal();

        PSYViewType type = (PSYViewType)EditorGUILayout.EnumPopup("", view.type, GUILayout.Width(60));
        switch (type)
        {
            case PSYViewType.Window:
                view.window = (PSYWindow)EditorGUILayout.EnumPopup("", view.window, GUILayout.Width(80));
                break;

            case PSYViewType.Item:
                view.item = (PSYItem)EditorGUILayout.EnumPopup("", view.item, GUILayout.Width(80));
                break;
        }
        PSYGUIAnimation.IntField("", ref view.index, false);
        if (Application.isPlaying)
        {
            GUILayout.Label(view.IntID.ToString(), GUILayout.MaxWidth(24));

            if (view.state.Opened)
            {
                GUILayout.Label("O", GUILayout.MaxWidth(12));
            }
            if (view.state.Enabled)
            {
                GUILayout.Label("E", GUILayout.MaxWidth(12));
            }
            if (view.isSelectable && view.state.Selected)
            {
                GUILayout.Label("S", GUILayout.MaxWidth(12));
            }
        }
        GUILayout.EndHorizontal();

        if (view.type != type)
        {
            view.type = type;
        }
        //Constants
        switch (type)
        {
            case PSYViewType.Root: view.childrenClickInterval = 0.0f; return;
            case PSYViewType.Window:
                view.isSelectable = false;
                break;
            case PSYViewType.Item:
                PSYGUIAnimation.BoolField("Radio group", ref view.isRadioGroup, true);
                GUILayout.BeginHorizontal();
                PSYGUIAnimation.BoolField(view.isSelectable ? "" : "Selectable", ref view.isSelectable, false, 100);
                if (view.isSelectable)
                    PSYGUIAnimation.BoolField("Toggleable", ref view.isSwitch, false, 100);
                GUILayout.EndHorizontal();
                break;
        }

        //Alpha animation and UIPanel
        bool foundA = false;
        PSYAnimationEvent evnt = PSYAnimationEvent.Open;
        for (int i = 0; i < view.sets.Count; i++)
            for (int j = 0; j < view.sets[i].items.Count; j++)
                if (view.sets[i].items[j].type == PSYAnimation.Type.Alpha && view.sets[i].items[j].target == PSYGUI.WholeViewAnimationTarget)
                {
                    evnt = view.sets[i].evnt;
                    foundA = true;
                    break;
                }
        if (view.GetComponent<CanvasGroup>() == null && foundA)
        {
            EditorUtility.DisplayDialog("PSYGUI Notification", "CanvasGroup was added to game object because it is required to perform alpha animation of entire view on " + evnt.ToString() + " event.", "Ok");
            view.gameObject.AddComponent<CanvasGroup>().alpha = 1;
        }

        if (type == PSYViewType.Item)
        {
            GUILayout.BeginHorizontal();
            PSYGUIAnimation.BoolField("", ref view.hold.enabled, false);
            if (!view.hold.enabled)
                GUILayout.Label("Enable hold mode");
            else
            {
                PSYGUIAnimation.BoolField("", ref view.hold.ignoreNextClick);
                PSYGUIAnimation.FloatField("", ref view.hold.firstThreshold, false, 40);
                PSYGUIAnimation.FloatField("", ref view.hold.baseThreshold, false, 40);
                PSYGUIAnimation.FloatField("", ref view.hold.acceleration, false, 40);
                PSYGUIAnimation.FloatField("", ref view.hold.incrementTime, false, 40);
            }
            GUILayout.EndHorizontal();
        }

        PSYGUIAnimation.BoolField("Enabled on start", ref view.isEnabledOnStart, true);
        if (!view.isEnabledOnStart)
        {
            PSYGUIAnimation.BoolField("Enable on start", ref view.enableOnStart, true);
        }
        else
        {
            PSYGUIAnimation.BoolField("Disable on start", ref view.disableOnStart, true);
        }
        if (view.isSelectable)
            PSYGUIAnimation.BoolField("Selected on start", ref view.isSelectedOnStart, true);

        GUILayout.BeginHorizontal();
        if (view.type != PSYViewType.Window)
        {
            PSYGUIAnimation.BoolField("Puppet", ref view.puppet, false, 60);
        }
        else
        {
            GUILayout.Label("Puppet: yes", GUILayout.MaxWidth(80));
            view.puppet = true;
        }

        PSYController psyController = view.GetComponent<PSYController>();
        int childredCount = psyController == null ? 0 : psyController.GetChildren().Count;

        GUILayout.Label("Child: " + view.Children.Count + "/" + childredCount, GUILayout.MaxWidth(60));
        //string strTmp;
        List<string> strChild = new List<string>();
        //Debug.LogError("view.Children.Count="+ view.Children.Count);
        for (int i = 0; i < view.Children.Count; i++)
        {
            strChild.Add(view.Children[i].name);
        }

        if (indexPopupChild >= view.Children.Count)
        {
            indexPopupChild = view.Children.Count == 0 ? 0 : view.Children.Count - 1;
        }

        indexPopupChild = EditorGUILayout.Popup(indexPopupChild, strChild.ToArray(), GUILayout.Width(60));
        if (GUILayout.Button("<", GUILayout.MaxWidth(20)))
        {
            //Debug.LogError("indexPopupChild=" + indexPopupChild);
            if (indexPopupChild < view.Children.Count)
            {
                Selection.activeObject = view.Children[indexPopupChild].Rect.gameObject;
            }
        }
        GUILayout.Label("Widgets: " + view.Widgets.Count, GUILayout.MaxWidth(90));

        //Отображение выпадающего списка с дочерними виджетами, которые без PSYView
        List<string> strWidgets = getWidgets();


        if (indexPopupWidgets >= view.Widgets.Count)
        {
            indexPopupWidgets = view.Widgets.Count == 0 ? 0 : view.Widgets.Count - 1;
        }

        indexPopupWidgets = EditorGUILayout.Popup(indexPopupWidgets, strWidgets.ToArray(), GUILayout.Width(60));
        if (GUILayout.Button("<", GUILayout.MaxWidth(20)))
        {
            if (indexPopupWidgets < view.Widgets.Count)
            {
                Selection.activeObject = view.Widgets[indexPopupWidgets].Rect.gameObject;
            }
        }

        bool GUI_enabled_tmp = GUI.enabled;
        //if (((forcedUpdateWidgets) || (!updateDone)) && !Application.isPlaying && view.type != PSYViewType.Root)
        if (!Application.isPlaying && view.type != PSYViewType.Root)
        {
            GUI.enabled = true;
        }else
        {
            GUI.enabled = false;
        }
        //    if (Application.isPlaying && view.type != PSYViewType.Root)
        //{
        //    GUI.enabled = false;
        //}
            if (GUILayout.Button("Update", GUILayout.MaxWidth(60)))
            {
                forcedUpdateWidgets = true;
                //Debug.LogError("PushButton forcedUpdatePopupWidgets=" + forcedUpdateWidgets);
            }

            GUI.enabled = GUI_enabled_tmp;
        

        GUILayout.EndHorizontal();


        view.animDoubling = PSYGUIAnimation.BoolField("Queue identical animations", view.animDoubling, true);
        GUILayout.BeginHorizontal();
        bool animationsBlockInputGlobal = view.animationBlocksInputGlobal;
    bool animationsBlockInput = PSYGUIAnimation.BoolField(view.animationBlocksInputLocal ? "" : "Animation blocks input", view.animationBlocksInputLocal, false);
        if (animationsBlockInput)
            animationsBlockInputGlobal = PSYGUIAnimation.BoolField("Animation blocks global input", view.animationBlocksInputGlobal, false);
    GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Animation superposition");
        view.superposition = (PSYSuperposition) EditorGUILayout.EnumPopup("", view.superposition, GUILayout.Width(73));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if (animationsBlockInput != view.animationBlocksInputLocal)
        {
            view.animationBlocksInputLocal = animationsBlockInput;
            List<PSYView> list = new List<PSYView>();
    PSYTools.GetChildObjects(view.transform, list, true);

            foreach (PSYView sio in list)
            {
                sio.animationBlocksInputLocal = view.animationBlocksInputLocal;
            }
        }
        if (animationsBlockInputGlobal != view.animationBlocksInputGlobal)
{
    view.animationBlocksInputGlobal = animationsBlockInputGlobal;
    /*foreach (PSYView sio in PSYTools.GetChildObjects<PSYView>(view.transform, true))
sio.animationBlocksInputGlobal = view.animationBlocksInputGlobal;*/
}

List<PSYAnimationEvent> freeToChoose = new List<PSYAnimationEvent>();
List<string> strFTC = new List<string>();
foreach (PSYAnimation set in view.sets)
{
    freeToChoose = new List<PSYAnimationEvent>() { set.evnt };
    freeToChoose = freeToChoose.AddEnumValues<PSYAnimationEvent>();
    foreach (PSYAnimation set2 in view.sets)
        if (set2.evnt != set.evnt)
            freeToChoose.Remove(set2.evnt);

    strFTC = new List<string>();
    for (int i = 0; i < freeToChoose.Count; i++)
        strFTC.Add(freeToChoose[i].ToString());
    GUILayout.BeginHorizontal();
    int res = EditorGUILayout.Popup(freeToChoose.IndexOf(set.evnt), strFTC.ToArray(), GUILayout.Width(140));
    if (res != 0)
        set.evnt = freeToChoose[res];

    if (GUILayout.Button("Edit", GUILayout.Width(52), GUILayout.MaxHeight(14)))
    {
        PSYGUIAnimation.Load(set);
    }

    if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.MaxHeight(14)))
    {
        view.sets.Remove(set);
        break;
    }

    if (!set.enabled)
    {
        GUILayout.Label("(disabled)", GUILayout.MaxHeight(14));
    }

    GUILayout.EndHorizontal();
}

if (view.sets.Count < Enum.GetValues(typeof(PSYAnimationEvent)).Length)
{
    if (GUILayout.Button("New animation entry...", GUILayout.Width(218), GUILayout.MaxHeight(14)))
    {
        freeToChoose = PSYTools.ListFromEnum<PSYAnimationEvent>();
        foreach (PSYAnimation set in view.sets)
            freeToChoose.Remove(set.evnt);
        PSYAnimation anim = new PSYAnimation(view, freeToChoose[0]);
        view.sets.Add(anim);
    }
}

GUILayout.BeginHorizontal();
GUILayout.Label("");
GUILayout.EndHorizontal();
GUILayout.BeginHorizontal();
if (GUILayout.Button("EVENTS / ANIMATION EVENTS", GUILayout.Width(250), GUILayout.MaxHeight(16)))
{
    events = !events;
}
GUILayout.EndHorizontal();

if (events)
{
    for (int i = 0; i < Mathf.Max(view.events.Count, view.animationEvents.Count); i++)
    {
        GUILayout.BeginHorizontal();

        if (view.events.Count > i)
        {
            List<PSYViewEvent> available1 = allEvents;

            foreach (PSYViewEvent e in view.events)
            {
                available1.Remove(e);
            }

            available1.Insert(0, view.events[i]);
            List<string> availableS1 = new List<string>();
            foreach (PSYViewEvent e in available1)
            {
                availableS1.Add(e.ToString());
            }

            view.events[i] = available1[EditorGUILayout.Popup(available1.IndexOf(view.events[i]), availableS1.ToArray(), GUILayout.Width(90))];

            if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.MaxHeight(14)))
            {
                view.events.RemoveAt(i);
                break;
            }
        }
        else
        {
            GUILayout.Space(116);
        }

        GUILayout.Space(22);

        if (view.animationEvents.Count > i)
        {
            List<PSYAnimationEvent> available2 = allAnimationEvents;

            foreach (PSYAnimationEvent e in view.animationEvents)
            {
                available2.Remove(e);
            }

            available2.Insert(0, view.animationEvents[i]);
            List<string> availableS2 = new List<string>();
            foreach (PSYAnimationEvent e in available2)
            {
                availableS2.Add(e.ToString());
            }

            view.animationEvents[i] = available2[EditorGUILayout.Popup(available2.IndexOf(view.animationEvents[i]), availableS2.ToArray(), GUILayout.Width(90))];

            if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.MaxHeight(14)))
            {
                view.animationEvents.RemoveAt(i);
                break;
            }
        }

        GUILayout.EndHorizontal();
    }

    GUILayout.BeginHorizontal();
    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.MaxHeight(14)))
    {
        List<PSYViewEvent> available = allEvents;

        foreach (PSYViewEvent e in view.events)
        {
            available.Remove(e);
        }

        view.events.Add(available[0]);
    }

    GUILayout.Space(114);
    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.MaxHeight(14)))
    {
        List<PSYAnimationEvent> available = allAnimationEvents;

        foreach (PSYAnimationEvent e in view.animationEvents)
        {
            available.Remove(e);
        }

        view.animationEvents.Add(available[0]);
    }
    GUILayout.EndHorizontal();
}

GUILayout.BeginHorizontal();
GUILayout.Label("");
GUILayout.EndHorizontal();

if (GUI.changed)
{
    EditorUtility.SetDirty(target);
}
    }
    //Выдает список дочерних виджетов
    List<string> getWidgets(int lineLimit = 500)
    {
        List<string> strWidgets = new List<string>();
        string strTmp;

        for (int i = 0; i < view.Widgets.Count; i++)
        {
            if (i>=lineLimit)
            {
                break;
            }

            strTmp = i + 1 + ") ";

            if (view.Widgets[i].isText)
            {
                strTmp += "Text| " + view.Widgets[i].Rect.gameObject.name + " = " + view.Widgets[i].Text.text;
            }
            else if (view.Widgets[i].isImage)
            {
                strTmp += "Image| " + view.Widgets[i].Image.name + " = ";
                if (view.Widgets[i].Image.sprite == null)
                {
                    strTmp += "null";
                }else
                {
                    strTmp += view.Widgets[i].Image.sprite.name;
                }
                //Debug.LogError(strTmp);

            }
            else
            {
                strTmp += "_________Unknown_________";
            }
            //strWidgets.Add(i + 1 + ") " + view.Widgets[i].Rect.gameObject.name + " = " + strTmp);
            strWidgets.Add(strTmp);


        }

        return strWidgets;
    }


    string getChildrenWidgetStr(int lineLimit = 10)
    {
        List<string> strWidgets = new List<string>();
        string strRes = "Количество children: " + view.Children.Count + "\n";
        if (view.Children.Count >= lineLimit)
        {
            strRes += "\n";
            strRes += "В списке показаны первые " + lineLimit + "\n";
            strRes += "\n";
        }

        for (int i = 0; i < view.Children.Count; i++)
        {
            if (i >= lineLimit)
            {
                break;
            }
            strRes += i + 1 + ") " + view.Children[i].Rect.gameObject.name;

            strRes += "\n";
        }

        return strRes;
    }

    //Выдает список дочерних виджетов в виде строки
    string getWidgetsStr(int lineLimit=1000)
    {
        List<string> strWidgets = new List<string>();
        string strRes="Количество widgets: "+ view.Widgets.Count+"\n";
        if (view.Widgets.Count >= lineLimit)
        {
            strRes += "\n";
            strRes += "В списке показаны первые "+lineLimit+"\n";
            strRes += "\n";
        }

        for (int i = 0; i < view.Widgets.Count; i++)
        {
            if (i>=lineLimit)
            {
                break;
            }
            //strRes += i + 1 + ") " + view.Widgets[i].Rect.gameObject.name + "=";
            strRes += i + 1 + ") ";

            if (view.Widgets[i].isText)
            {
                strRes += "Text| "+ view.Widgets[i].Rect.gameObject.name + "=" + view.Widgets[i].Text.text;
            }
            else if (view.Widgets[i].isImage)
            {
                strRes += "Image| " + view.Widgets[i].Image.name + " = ";
                if (view.Widgets[i].Image.sprite == null)
                {
                    strRes += "null";
                }
                else
                {
                    strRes += view.Widgets[i].Image.sprite.name;
                }

            }
            else
            {
                strRes += "_________Unknown_________";
            }

            strRes += "\n";
        }

        return strRes;
    }


    //public override void OnInspectorGUI()
    //{
    //    GUI.changed = false;

    //    if (target as PSYView != view)
    //    {
    //        updateDone = false;
    //    }

    //    view = target as PSYView;
    //    if (view.GetComponent<RectTransform>() == null)
    //    {
    //        view.gameObject.AddComponent<RectTransform>();
    //    }



    //    if (!updateDone && !Application.isPlaying && view.type != PSYViewType.Root)
    //    {

    //        Debug.LogError("Reset forcedUpdateWidgets=" + forcedUpdateWidgets);
    //        updateDone = true;
    //        if (view.state == null)
    //        {
    //            view.state = new PSYViewState(view.ID, false, view.isEnabledOnStart, false);
    //        }

    //        if (view.Widgets == null)
    //        {
    //            view.Widgets = new List<PSYWidget>();
    //        }
    //        else
    //        {
    //            view.Widgets.Clear();
    //        }
    //        PSYTools.GetFreeWidgets(view.transform, view.Widgets);

    //        if (view.type != PSYViewType.Window)
    //        {
    //            view.Parent = PSYTools.GetParentObject<PSYView>(view.transform);
    //            //PSYDebug.Warning(PSYDebug.Source.Editor, "Updated view parent of {0} {1}", view.name, view.Parent.gameObject.name);
    //        }

    //        PSYController cont = view.GetComponent<PSYController>();
    //        if (cont != null && cont.view.type != PSYViewType.Window)
    //        {
    //            cont.Parent = PSYTools.GetParentObject<PSYController>(view.transform);
    //            //PSYDebug.Warning(PSYDebug.Source.Editor, "Updated cont parent of {0} {1}", view.name, cont.Parent.gameObject.name);
    //        }

    //        if (forcedUpdateWidgets)
    //        {
    //            forcedUpdateWidgets = false;
    //            string strMessage = "Список перезачитан";
    //            strMessage += "\n";
    //            strMessage += getWidgetsStr(40);
    //            strMessage += "\n";
    //            strMessage += "Сохраните изменения префаба Ctrl+s";
    //            //AssetDatabase.SaveAssets();
    //            //EditorUtility.Sa

    //            EditorUtility.DisplayDialog("Принудительное обновление списка дочерних widget-ов", strMessage, "Ok");
    //        }
    //        forcedUpdateWidgets = false;

    //    }
    //    else if (forcedUpdateWidgets)
    //    {
    //        string strTmp2 = "Список не перезачитан, т.к.";
    //        if (view.type == PSYViewType.Root)
    //        {
    //            strTmp2 += "\n view.type == PSYViewType.Root";
    //        }

    //        if (Application.isPlaying)
    //        {
    //            strTmp2 += "\n Application.isPlaying=true";
    //        }

    //        EditorUtility.DisplayDialog("Принудительное обновление списка дочерних widget-ов", strTmp2, "Ok");
    //    }

    //    GUIStyle helpStyle = new GUIStyle(GUI.skin.button);
    //    helpStyle.wordWrap = true;
    //    helpStyle.alignment = TextAnchor.UpperLeft;
    //    Color c = Color.white;
    //    c.a = 0.75f;
    //    helpStyle.normal.textColor = c;

    //    //EditorUtility.SetDirty(target);

    //    PSYGUIAnimation.DrawSeparator();
    //    GUILayout.BeginHorizontal();

    //    PSYViewType type = (PSYViewType)EditorGUILayout.EnumPopup("", view.type, GUILayout.Width(60));
    //    switch (type)
    //    {
    //        case PSYViewType.Window:
    //            view.window = (PSYWindow)EditorGUILayout.EnumPopup("", view.window, GUILayout.Width(80));
    //            break;

    //        case PSYViewType.Item:
    //            view.item = (PSYItem)EditorGUILayout.EnumPopup("", view.item, GUILayout.Width(80));
    //            break;
    //    }
    //    PSYGUIAnimation.IntField("", ref view.index, false);
    //    if (Application.isPlaying)
    //    {
    //        GUILayout.Label(view.IntID.ToString(), GUILayout.MaxWidth(24));

    //        if (view.state.Opened)
    //        {
    //            GUILayout.Label("O", GUILayout.MaxWidth(12));
    //        }
    //        if (view.state.Enabled)
    //        {
    //            GUILayout.Label("E", GUILayout.MaxWidth(12));
    //        }
    //        if (view.isSelectable && view.state.Selected)
    //        {
    //            GUILayout.Label("S", GUILayout.MaxWidth(12));
    //        }
    //    }
    //    GUILayout.EndHorizontal();

    //    if (view.type != type)
    //    {
    //        view.type = type;
    //    }
    //    //Constants
    //    switch (type)
    //    {
    //        case PSYViewType.Root: view.childrenClickInterval = 0.0f; return;
    //        case PSYViewType.Window:
    //            view.isSelectable = false;
    //            break;
    //        case PSYViewType.Item:
    //            PSYGUIAnimation.BoolField("Radio group", ref view.isRadioGroup, true);
    //            GUILayout.BeginHorizontal();
    //            PSYGUIAnimation.BoolField(view.isSelectable ? "" : "Selectable", ref view.isSelectable, false, 100);
    //            if (view.isSelectable)
    //                PSYGUIAnimation.BoolField("Toggleable", ref view.isSwitch, false, 100);
    //            GUILayout.EndHorizontal();
    //            break;
    //    }

    //    //Alpha animation and UIPanel
    //    bool foundA = false;
    //    PSYAnimationEvent evnt = PSYAnimationEvent.Open;
    //    for (int i = 0; i < view.sets.Count; i++)
    //        for (int j = 0; j < view.sets[i].items.Count; j++)
    //            if (view.sets[i].items[j].type == PSYAnimation.Type.Alpha && view.sets[i].items[j].target == PSYGUI.WholeViewAnimationTarget)
    //            {
    //                evnt = view.sets[i].evnt;
    //                foundA = true;
    //                break;
    //            }
    //    if (view.GetComponent<CanvasGroup>() == null && foundA)
    //    {
    //        EditorUtility.DisplayDialog("PSYGUI Notification", "CanvasGroup was added to game object because it is required to perform alpha animation of entire view on " + evnt.ToString() + " event.", "Ok");
    //        view.gameObject.AddComponent<CanvasGroup>().alpha = 1;
    //    }

    //    if (type == PSYViewType.Item)
    //    {
    //        GUILayout.BeginHorizontal();
    //        PSYGUIAnimation.BoolField("", ref view.hold.enabled, false);
    //        if (!view.hold.enabled)
    //            GUILayout.Label("Enable hold mode");
    //        else
    //        {
    //            PSYGUIAnimation.BoolField("", ref view.hold.ignoreNextClick);
    //            PSYGUIAnimation.FloatField("", ref view.hold.firstThreshold, false, 40);
    //            PSYGUIAnimation.FloatField("", ref view.hold.baseThreshold, false, 40);
    //            PSYGUIAnimation.FloatField("", ref view.hold.acceleration, false, 40);
    //            PSYGUIAnimation.FloatField("", ref view.hold.incrementTime, false, 40);
    //        }
    //        GUILayout.EndHorizontal();
    //    }

    //    PSYGUIAnimation.BoolField("Enabled on start", ref view.isEnabledOnStart, true);
    //    if (!view.isEnabledOnStart)
    //    {
    //        PSYGUIAnimation.BoolField("Enable on start", ref view.enableOnStart, true);
    //    }
    //    else
    //    {
    //        PSYGUIAnimation.BoolField("Disable on start", ref view.disableOnStart, true);
    //    }
    //    if (view.isSelectable)
    //        PSYGUIAnimation.BoolField("Selected on start", ref view.isSelectedOnStart, true);

    //    GUILayout.BeginHorizontal();
    //    if (view.type != PSYViewType.Window)
    //    {
    //        PSYGUIAnimation.BoolField("Puppet", ref view.puppet, false, 60);
    //    }
    //    else
    //    {
    //        GUILayout.Label("Puppet: yes", GUILayout.MaxWidth(80));
    //        view.puppet = true;
    //    }

    //    PSYController psyController = view.GetComponent<PSYController>();
    //    int childredCount = psyController == null ? 0 : psyController.GetChildren().Count;

    //    GUILayout.Label("Child: " + view.Children.Count + "/" + childredCount, GUILayout.MaxWidth(60));
    //    //string strTmp;
    //    List<string> strChild = new List<string>();
    //    for (int i = 0; i < view.Children.Count; i++)
    //    {
    //        strChild.Add(view.Children[i].name);
    //    }
    //    EditorGUILayout.Popup(0, strChild.ToArray(), GUILayout.Width(90));

    //    GUILayout.Label("Widgets: " + view.Widgets.Count, GUILayout.MaxWidth(90));

    //    List<string> strWidgets = getWidgets();

    //    if (indexPopupWidgets >= view.Widgets.Count)
    //    {
    //        indexPopupWidgets = view.Widgets.Count == 0 ? 0 : view.Widgets.Count - 1;
    //    }

    //    indexPopupWidgets = EditorGUILayout.Popup(indexPopupWidgets, strWidgets.ToArray(), GUILayout.Width(90));
    //    if (GUILayout.Button("Update", GUILayout.MaxWidth(60)))
    //    {
    //        forcedUpdateWidgets = true;
    //        Debug.LogError("PushButton forcedUpdatePopupWidgets=" + forcedUpdateWidgets);

    //    }
    //    GUILayout.EndHorizontal();


    //    view.animDoubling = PSYGUIAnimation.BoolField("Queue identical animations", view.animDoubling, true);
    //    GUILayout.BeginHorizontal();
    //    bool animationsBlockInputGlobal = view.animationBlocksInputGlobal;
    //    bool animationsBlockInput = PSYGUIAnimation.BoolField(view.animationBlocksInputLocal ? "" : "Animation blocks input", view.animationBlocksInputLocal, false);
    //    if (animationsBlockInput)
    //        animationsBlockInputGlobal = PSYGUIAnimation.BoolField("Animation blocks global input", view.animationBlocksInputGlobal, false);
    //    GUILayout.EndHorizontal();

    //    GUILayout.BeginHorizontal();
    //    GUILayout.Label("Animation superposition");
    //    view.superposition = (PSYSuperposition)EditorGUILayout.EnumPopup("", view.superposition, GUILayout.Width(73));
    //    GUILayout.FlexibleSpace();
    //    GUILayout.EndHorizontal();

    //    if (animationsBlockInput != view.animationBlocksInputLocal)
    //    {
    //        view.animationBlocksInputLocal = animationsBlockInput;
    //        List<PSYView> list = new List<PSYView>();
    //        PSYTools.GetChildObjects(view.transform, list, true);

    //        foreach (PSYView sio in list)
    //        {
    //            sio.animationBlocksInputLocal = view.animationBlocksInputLocal;
    //        }
    //    }
    //    if (animationsBlockInputGlobal != view.animationBlocksInputGlobal)
    //    {
    //        view.animationBlocksInputGlobal = animationsBlockInputGlobal;
    //        /*foreach (PSYView sio in PSYTools.GetChildObjects<PSYView>(view.transform, true))
    //sio.animationBlocksInputGlobal = view.animationBlocksInputGlobal;*/
    //    }

    //    List<PSYAnimationEvent> freeToChoose = new List<PSYAnimationEvent>();
    //    List<string> strFTC = new List<string>();
    //    foreach (PSYAnimation set in view.sets)
    //    {
    //        freeToChoose = new List<PSYAnimationEvent>() { set.evnt };
    //        freeToChoose = freeToChoose.AddEnumValues<PSYAnimationEvent>();
    //        foreach (PSYAnimation set2 in view.sets)
    //            if (set2.evnt != set.evnt)
    //                freeToChoose.Remove(set2.evnt);

    //        strFTC = new List<string>();
    //        for (int i = 0; i < freeToChoose.Count; i++)
    //            strFTC.Add(freeToChoose[i].ToString());
    //        GUILayout.BeginHorizontal();
    //        int res = EditorGUILayout.Popup(freeToChoose.IndexOf(set.evnt), strFTC.ToArray(), GUILayout.Width(140));
    //        if (res != 0)
    //            set.evnt = freeToChoose[res];

    //        if (GUILayout.Button("Edit", GUILayout.Width(52), GUILayout.MaxHeight(14)))
    //        {
    //            PSYGUIAnimation.Load(set);
    //        }

    //        if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.MaxHeight(14)))
    //        {
    //            view.sets.Remove(set);
    //            break;
    //        }

    //        if (!set.enabled)
    //        {
    //            GUILayout.Label("(disabled)", GUILayout.MaxHeight(14));
    //        }

    //        GUILayout.EndHorizontal();
    //    }

    //    if (view.sets.Count < Enum.GetValues(typeof(PSYAnimationEvent)).Length)
    //    {
    //        if (GUILayout.Button("New animation entry...", GUILayout.Width(218), GUILayout.MaxHeight(14)))
    //        {
    //            freeToChoose = PSYTools.ListFromEnum<PSYAnimationEvent>();
    //            foreach (PSYAnimation set in view.sets)
    //                freeToChoose.Remove(set.evnt);
    //            PSYAnimation anim = new PSYAnimation(view, freeToChoose[0]);
    //            view.sets.Add(anim);
    //        }
    //    }

    //    GUILayout.BeginHorizontal();
    //    GUILayout.Label("");
    //    GUILayout.EndHorizontal();
    //    GUILayout.BeginHorizontal();
    //    if (GUILayout.Button("EVENTS / ANIMATION EVENTS", GUILayout.Width(250), GUILayout.MaxHeight(16)))
    //    {
    //        events = !events;
    //    }
    //    GUILayout.EndHorizontal();

    //    if (events)
    //    {
    //        for (int i = 0; i < Mathf.Max(view.events.Count, view.animationEvents.Count); i++)
    //        {
    //            GUILayout.BeginHorizontal();

    //            if (view.events.Count > i)
    //            {
    //                List<PSYViewEvent> available1 = allEvents;

    //                foreach (PSYViewEvent e in view.events)
    //                {
    //                    available1.Remove(e);
    //                }

    //                available1.Insert(0, view.events[i]);
    //                List<string> availableS1 = new List<string>();
    //                foreach (PSYViewEvent e in available1)
    //                {
    //                    availableS1.Add(e.ToString());
    //                }

    //                view.events[i] = available1[EditorGUILayout.Popup(available1.IndexOf(view.events[i]), availableS1.ToArray(), GUILayout.Width(90))];

    //                if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.MaxHeight(14)))
    //                {
    //                    view.events.RemoveAt(i);
    //                    break;
    //                }
    //            }
    //            else
    //            {
    //                GUILayout.Space(116);
    //            }

    //            GUILayout.Space(22);

    //            if (view.animationEvents.Count > i)
    //            {
    //                List<PSYAnimationEvent> available2 = allAnimationEvents;

    //                foreach (PSYAnimationEvent e in view.animationEvents)
    //                {
    //                    available2.Remove(e);
    //                }

    //                available2.Insert(0, view.animationEvents[i]);
    //                List<string> availableS2 = new List<string>();
    //                foreach (PSYAnimationEvent e in available2)
    //                {
    //                    availableS2.Add(e.ToString());
    //                }

    //                view.animationEvents[i] = available2[EditorGUILayout.Popup(available2.IndexOf(view.animationEvents[i]), availableS2.ToArray(), GUILayout.Width(90))];

    //                if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.MaxHeight(14)))
    //                {
    //                    view.animationEvents.RemoveAt(i);
    //                    break;
    //                }
    //            }

    //            GUILayout.EndHorizontal();
    //        }

    //        GUILayout.BeginHorizontal();
    //        if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.MaxHeight(14)))
    //        {
    //            List<PSYViewEvent> available = allEvents;

    //            foreach (PSYViewEvent e in view.events)
    //            {
    //                available.Remove(e);
    //            }

    //            view.events.Add(available[0]);
    //        }

    //        GUILayout.Space(114);
    //        if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.MaxHeight(14)))
    //        {
    //            List<PSYAnimationEvent> available = allAnimationEvents;

    //            foreach (PSYAnimationEvent e in view.animationEvents)
    //            {
    //                available.Remove(e);
    //            }

    //            view.animationEvents.Add(available[0]);
    //        }
    //        GUILayout.EndHorizontal();
    //    }

    //    GUILayout.BeginHorizontal();
    //    GUILayout.Label("");
    //    GUILayout.EndHorizontal();

    //    if (GUI.changed)
    //    {
    //        EditorUtility.SetDirty(target);
    //    }
    //}


}