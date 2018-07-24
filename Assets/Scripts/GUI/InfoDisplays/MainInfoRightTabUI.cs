using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// handles Passive Tab interactions (click) for RHS of Main Info App
/// </summary>
public class MainInfoRightTabUI : MonoBehaviour, IPointerClickHandler
{

    private int tabIndex = -1;          //assigned at start by MainInfoUI, corresponds to tabActiveArray[index]


    /// <summary>
    /// sets tab index
    /// </summary>
    /// <param name="index"></param>
    public void SetTabIndex(int index)
    {
        Debug.Assert(index > -1 && index < 6, "Invalid index (must be > -1 && < 6)");
        tabIndex = index;
        /*Debug.LogFormat("[Tst] MainInofRightTabUI.cs -> SetTabIndex: tabIndex {0}{1}", tabIndex, "\n");*/
    }

    public int GetTabIndex()
    { return tabIndex; }

    /// <summary>
    /// Mouse click -> Open tab
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            //same for left or right click
            case PointerEventData.InputButton.Left:
            case PointerEventData.InputButton.Right:
                EventManager.instance.PostNotification(EventType.MainInfoTabOpen, this, tabIndex, "MainInfoRightTabUI.cs -> OnPointClick");
                break;
        }
    }

}
