using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles mouse interaction for tutorial widget Right (Next) arrow -> go to next set
/// </summary>
public class TutorialWidgetRight : MonoBehaviour, IPointerClickHandler
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
                EventManager.i.PostNotification(EventType.TutorialNextSet, this, null, "TutorialWidgetRight.cs -> OnPointerClick");
                break;
            default:
                Debug.LogError("Unknown InputButton");
                break;
        }
    }
    #endregion
}
