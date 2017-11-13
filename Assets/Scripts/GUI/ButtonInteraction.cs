using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Generic class to attach to UI buttons such as 'Cancel' or 'Confirm'. You need to set the event type and whether the event returns a Parameter (int)
/// </summary>
public class ButtonInteraction : MonoBehaviour, IPointerClickHandler
{

    [Tooltip("Select the event that is triggered when the button is clicked")]
    public EventType eventType = EventType.None;
    [Tooltip("Does the button return an <int> value when clicked?")]
    public bool isReturn = false;


    private int returnData;



    /// <summary>
    /// Data to be returned if isReturn = true
    /// </summary>
    /// <param name="data"></param>
    public void SetReturnData(int data)
    { returnData = data; }


    /// <summary>
    /// Generate an event when button clicked
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick (PointerEventData eventData)
    {
        if (eventType != EventType.None)
        {
            if (isReturn == true)
            { EventManager.instance.PostNotification(eventType, this, returnData); }
            else
            { EventManager.instance.PostNotification(eventType, this); }

        }

    }
}
