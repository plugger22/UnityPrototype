using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles mouse interaction for tutorial widget Left (Back) arrow -> go to previous set
/// </summary>
public class TutorialWidgetLeft : MonoBehaviour, IPointerClickHandler
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
                EventManager.i.PostNotification(EventType.TutorialPreviousSet, this, null, "TutorialWidgetLeft.cs -> OnPointerClick");
                break;
            default:
                Debug.LogError("Unknown InputButton");
                break;
        }
    }
    #endregion
}
