using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FinderSideTabMouseUI : MonoBehaviour, IPointerClickHandler
{

    private bool onMouseFlag;                           //flag indicates that onMouseOver is true (used for tooltip coroutine)
    private float mouseOverDelay;                       //tooltip



    /// <summary>
    /// Mouse click -> Right: Actor Action Menu
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
            case PointerEventData.InputButton.Right:
                if (GameManager.i.guiScript.CheckIsBlocked() == false)
                {
                    EventManager.i.PostNotification(EventType.FinderOpen, this, null, "FinderSideTabMouseUI.cs -> OnPointerClick");
                }
                break;
            default:
                Debug.LogError("Unknown InputButton");
                break;
        }
    }
}
