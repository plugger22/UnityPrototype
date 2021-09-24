using UnityEngine;
using UnityEngine.EventSystems;

public class FinderTabMouseUI : MonoBehaviour, IPointerClickHandler
{

    private bool onMouseFlag;                           //flag indicates that onMouseOver is true (used for tooltip coroutine)
    private float mouseOverDelay;                       //tooltip



    /// <summary>
    /// Mouse click -> Left / Right: close finderUI
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
            case PointerEventData.InputButton.Right:
                EventManager.i.PostNotification(EventType.FinderClose, this, null, "FinderTabMouseUI.cs -> OnPointerClick");
                break;
            default:
                Debug.LogError("Unknown InputButton");
                break;
        }
    }



}
