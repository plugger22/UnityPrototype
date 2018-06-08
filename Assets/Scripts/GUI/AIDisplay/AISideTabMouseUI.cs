using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using gameAPI;


/// <summary>
/// attached to AI side tab to handle all mouse interactions
/// </summary>
public class AISideTabMouseUI : MonoBehaviour, IPointerClickHandler/*, IPointerEnterHandler, IPointerExitHandler*/
{

    private bool onMouseFlag;                           //flag indicates that onMouseOver is true (used for tooltip coroutine)
    private float mouseOverDelay;                       //tooltip



    /// <summary>
    /// Mouse click -> Right: Actor Action Menu
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
            case PointerEventData.InputButton.Right:
                if (GameManager.instance.guiScript.CheckIsBlocked() == false)
                {
                    switch (GameManager.instance.aiSideTabScript.hackingStatus)
                    {
                        case HackingStatus.Initialising:
                            GameManager.instance.guiScript.SetAlertMessage(AlertType.HackingInitialising);
                            break;
                        case HackingStatus.Rebooting:
                            GameManager.instance.guiScript.SetAlertMessage(AlertType.HackingRebootInProgress);
                            break;
                        case HackingStatus.InsufficientRenown:
                            GameManager.instance.guiScript.SetAlertMessage(AlertType.HackingInsufficientRenown);
                            break;
                        case HackingStatus.Indisposed:
                            GameManager.instance.guiScript.SetAlertMessage(AlertType.HackingIndisposed);
                            break;
                        case HackingStatus.Possible:
                            //update hacking status
                            GameManager.instance.aiScript.UpdateHackingStatus();
                            //activate AI Display (auto closes side tab)
                            EventManager.instance.PostNotification(EventType.AIDisplayOpen, this, null, "AISideTabMouseUI.cs -> OnPointerClick");
                            break;
                        default:
                            Debug.LogWarningFormat("Invalid aiSideTabUI.cs hackingStatus {0}", GameManager.instance.aiSideTabScript.hackingStatus);
                            GameManager.instance.guiScript.SetAlertMessage(AlertType.SomethingWrong);
                            break;
                    }
                }
                break;
            default:
                Debug.LogError("Unknown InputButton");
                break;
        }
    }

}
