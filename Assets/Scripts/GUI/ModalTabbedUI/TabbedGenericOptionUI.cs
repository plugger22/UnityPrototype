using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Combined Interaction and UI script for various page (gear/history/orgs) multi-Option 'buttons' -> ModalTabbedUI.cs
/// </summary>
public class TabbedGenericOptionUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Components")]
    public Image image;
    public TextMeshProUGUI title;

    private EventType eventType = EventType.None;               //The  event that is triggered when the image is clicked (get component in code and call SetEvent)

    /// <summary>
    /// Awake
    /// </summary>
    public void Awake()
    {
        Debug.Assert(image != null, "Invalid image (Null)");
        Debug.Assert(title != null, "Invalid title (Null)");
    }


    /// <summary>
    /// set Event Type and optional data to pass as a parameter once image clicked
    /// </summary>
    /// <param name="type"></param>
    public void SetEvent(EventType type)
    {
        eventType = type;
    }


    /// <summary>
    /// Mouse click -> Display History data set (left or right click)
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            //same for left or right click
            case PointerEventData.InputButton.Left:
            case PointerEventData.InputButton.Right:
                EventManager.i.PostNotification(eventType, this, null, "TabbedHistoryOptionUI.cs -> OnPointClick");
                break;
        }
    }

}
