using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// handles mouse overs and clicks on ModalTabbedUI side tabs
/// </summary>
public class TabbedSideTabUI : MonoBehaviour, IPointerClickHandler
{

    private int tabIndex = -1;          //assigned at start by ModalTabbedUI, corresponds to tabItems[index]
    private int maxTabIndex = -1;       //assigned at start by ModalTabbedUI, max for Side tabs only


    /// <summary>
    /// sets tab index
    /// </summary>
    /// <param name="index"></param>
    public void SetTabIndex(int index, int max)
    {
        Debug.AssertFormat(max > -1 && max >= index, "Invalid max \"{0}\", (must be > -1 and > index(\"{1}\"))", max, index);
        maxTabIndex = max;
        Debug.AssertFormat(index > -1 && index <= maxTabIndex, "Invalid index (must be > -1 && < {0})", maxTabIndex);
        tabIndex = index;
        /*Debug.LogFormat("[Tst] TabbedSideTabUI.cs -> SetTabIndex: tabIndex {0}{1}", tabIndex, "\n");*/
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
                EventManager.i.PostNotification(EventType.TabbedSideTabOpen, this, tabIndex, "TabbedSideTabUI.cs -> OnPointClick");
                break;
        }
    }
}
