﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// handles item interaction (click) for RHS. Click on an item and display details in LHS
/// </summary>
public class MainInfoRightItemUI : MonoBehaviour, IPointerClickHandler
{

    private int itemIndex = -1;          //assigned at start by MainInfoUI, corresponds to listOfCurrentPageData[index]

    /// <summary>
    /// sets item index
    /// </summary>
    /// <param name="index"></param>
    public void SetItemIndex(int index)
    {
        Debug.Assert(index > -1 && index < 20, "Invalid index (must be > -1 && < 20)");
        itemIndex = index;
        /*Debug.LogFormat("[Tst] MainInfoRightItemUI.cs -> SetItemIndex: itemIndex {0}{1}", itemIndex, "\n");*/
    }

    public int GetItemIndex()
    { return itemIndex; }

    /// <summary>
    /// Mouse click -> Show details, if any, in LHS
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            //same for left or right click
            case PointerEventData.InputButton.Left:
            case PointerEventData.InputButton.Right:
                EventManager.instance.PostNotification(EventType.MainInfoShowDetails, this, itemIndex, "MainInfoRightItemUI.cs -> OnPointClick");
                break;
        }
    }

}
