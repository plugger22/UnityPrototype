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

    [Tooltip("Select the  event that is triggered when the button is clicked (REQUIRED)")]
    private EventType eventType = EventType.None;

    /*[Tooltip("Does the  button event return <int> data when clicked? (Optional)")]
    public bool isReturn = false;*/

    /*[Tooltip("Select the Second event that is triggered when the button is clicked (Optional)")]
    public EventType eventType2 = EventType.None;
    [Tooltip("Does the Second button event return GenericReturnData value when clicked? (Optional)")]
    public bool isReturn2 = false;*/

    /*private GenericReturnData returnData;           //there is only one set of return data, it can be returned by either the first or second event

    /// <summary>
    /// internal initialisation
    /// </summary>
    private void Awake()
    {
        //returnData = new GenericReturnData();
    }


    /// <summary>
    /// Data to be returned if isReturn = true
    /// </summary>
    /// <param name="data"></param>
    public void SetReturnDataOption(int data)
    { returnData.optionID = data; }

    public void SetReturnDataNode(int data)
    { returnData.nodeID = data; }
    */

    /// <summary>
    /// set Event Type
    /// </summary>
    /// <param name="type"></param>
    public void SetEvent(EventType type)
    { eventType = type; }

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
            EventManager.instance.PostNotification(eventType, this);

            /*if (isReturn == true)
            { EventManager.instance.PostNotification(eventType, this, returnData); }
            else
            { EventManager.instance.PostNotification(eventType, this); }*/
        }
    }
}
