using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles mouse interaction for tutorial widget Master Help (Question) -> open master help
/// </summary>
public class TutorialWidgetQuestion : MonoBehaviour, IPointerClickHandler
{

    #region OnPointerClick
    /// <summary>
    /// Mouse click -> Left / Right 
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
            case PointerEventData.InputButton.Right:
                EventManager.i.PostNotification(EventType.MasterHelpOpen, this, null, "TutorialWidgetQuestion.cs -> OnPointerClick");
                break;
            default:
                Debug.LogError("Unknown InputButton");
                break;
        }
    }
    #endregion
}
