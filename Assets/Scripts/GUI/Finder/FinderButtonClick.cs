using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// FinderUI -> when user clicks on a button it highlights the button
/// </summary>
public class FinderButtonClick : MonoBehaviour, IPointerClickHandler
{
    private EventType eventType = EventType.None;               //The  event that is triggered when the button is clicked (get component in code and call SetButton)
    private int data = -1;                                      //list button index


    /// <summary>
    /// set Event Type and optional data to pass as a parameter once button clicked
    /// </summary>
    /// <param name="type"></param>
    public void SetButton(EventType type, int returnData = -1)
    {
        eventType = type;
        data = returnData;
    }

    /// <summary>
    /// Generate event/s when button clicked
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        //event 1
        if (eventType != EventType.None)
        {
            EventManager.i.PostNotification(eventType, this, data, "FinderButtonClick.cs -> OnPointerClick");
        }
    }

}
