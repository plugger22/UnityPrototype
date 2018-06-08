using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// attached to shadow (invisible) side tab image that intercepts all mouse clicks 
/// </summary>
public class AIDisplaySideTabMouseUI : MonoBehaviour, IPointerClickHandler
{


    /// <summary>
    /// Mouse click -> Right: Actor Action Menu
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
            case PointerEventData.InputButton.Right:
                //closes AI Display (auto opens side tab)
                EventManager.instance.PostNotification(EventType.AIDisplayClose, this, null, "AIDisplaySideTabMouseUI.cs -> OnPointerClick");
                break;
            default:
                Debug.LogError("Unknown InputButton");
                break;
        }
    }

}
