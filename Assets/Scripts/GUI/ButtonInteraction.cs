using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using UnityEngine.EventSystems;
using modalAPI;
using UnityEngine.UI;

/// <summary>
/// Generic class to attach to UI buttons such as 'Cancel' or 'Confirm'. You need to set the event type and whether the event returns a Parameter (int)
/// </summary>
public class ButtonInteraction : MonoBehaviour, IPointerClickHandler
{

    private EventType eventType = EventType.None;               //The  event that is triggered when the button is clicked (get component in code and call SetButton)
    private int returnData = -1;                                      //optional int data parameter that you can supply (SetButton) which is passed back as a parameter when button clicked


    /// <summary>
    /// set Event Type and optional data to pass as a parameter once button clicked
    /// </summary>
    /// <param name="type"></param>
    public void SetButton(EventType type, int returnData = -1)
    {
        eventType = type;
        this.returnData = returnData;
    }

    public EventType GetEvent()
    { return eventType; }

    /// <summary>
    /// Generate event/s when button clicked
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        //event 1
        if (eventType != EventType.None)
        {
            EventManager.i.PostNotification(eventType, this, returnData, "ButtonInteraction.cs -> OnPointerClick");
        }
    }
}
