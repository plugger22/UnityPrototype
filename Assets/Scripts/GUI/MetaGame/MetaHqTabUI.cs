using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// handles mouse overs and clicks on Hq tabs
/// </summary>
public class MetaHqTabUI : MonoBehaviour, IPointerClickHandler
{

    private int tabIndex = -1;          //assigned at start by MetaGameUI, corresponds to tabItems[index]


    /// <summary>
    /// sets tab index
    /// </summary>
    /// <param name="index"></param>
    public void SetTabIndex(int index)
    {
        Debug.Assert(index > -1 && index < 4, "Invalid index (must be > -1 && < 4)");
        tabIndex = index;
        /*Debug.LogFormat("[Tst] MetaHqTabUI.cs -> SetTabIndex: tabIndex {0}{1}", tabIndex, "\n");*/
    }

    public int GetTabIndex()
    { return tabIndex; }

    /// <summary>
    /// Mouse click -> Open tab (left or right click)
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            //same for left or right click
            case PointerEventData.InputButton.Left:
            case PointerEventData.InputButton.Right:
                EventManager.instance.PostNotification(EventType.MetaTabOpen, this, tabIndex, "MetaHqTabUI.cs -> OnPointClick");
                break;
        }
    }
}
