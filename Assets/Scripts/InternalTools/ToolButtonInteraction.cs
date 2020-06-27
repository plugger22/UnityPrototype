using UnityEngine;
using UnityEngine.EventSystems;

#if (UNITY_EDITOR)
/// <summary>
/// buttonInteraction equivalent for internal tools
/// </summary>
public class ToolButtonInteraction : MonoBehaviour
{

    private ToolEventType eventType = ToolEventType.None;               //The  event that is triggered when the button is clicked (get component in code and call SetButton)
    private int returnData = -1;                                        //optional int data parameter that you can supply (SetButton) which is passed back as a parameter when button clicked


    /// <summary>
    /// set Event Type and optional data to pass as a parameter once button clicked
    /// </summary>
    /// <param name="type"></param>
    public void SetButton(ToolEventType type, int returnData = -1)
    {
        eventType = type;
        this.returnData = returnData;
    }

    public ToolEventType GetEvent()
    { return eventType; }

    /// <summary>
    /// Generate event/s when button clicked
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        //event 1
        if (eventType != ToolEventType.None)
        {
            ToolEvents.i.PostNotification(eventType, this, returnData, "ToolButtonInteraction.cs -> OnPointerClick");
        }
    }
}

#endif
