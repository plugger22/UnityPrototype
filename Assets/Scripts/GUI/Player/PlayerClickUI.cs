using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using gameAPI;
using modalAPI;
using System;

/// <summary>
/// handles clicks on Player portrait in ActorUI
/// </summary>
public class PlayerClickUI : MonoBehaviour, IPointerClickHandler
{

    string menuHeaderRightClick;

    public void Start()
    {
        menuHeaderRightClick = GameManager.i.actionScript.GetPlayerActionMenuHeader();
        Debug.Assert(string.IsNullOrEmpty(menuHeaderRightClick) == false, "Invalid menuHeaderRightClick (Null or empty)");
    }

    /// <summary>
    /// Mouse click -> Right: Actor Action Menu
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
        bool proceedFlag = true;
        AlertType alertType = AlertType.None;

        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                if (GameManager.i.optionScript.isActorLeftMenu == true)
                {
                    //player review
                    TabbedUIData tabbedDetails = new TabbedUIData()
                    {
                        side = playerSide,
                        who = TabbedUIWho.Player,
                        slotID = 0,
                    };
                    EventManager.i.PostNotification(EventType.TabbedOpen, this, tabbedDetails, "PlayerClickUI.cs -> OnPointerClick");
                }
                else { GameManager.i.guiScript.SetAlertMessageModalOne(AlertType.TutorialMenuUnavailable); }
                break;
            case PointerEventData.InputButton.Right:
                if (GameManager.i.guiScript.CheckIsBlocked() == false)
                {
                    //Action Menu -> not valid if AI is active for side
                    if (GameManager.i.sideScript.CheckInteraction() == false)
                    { proceedFlag = false; alertType = AlertType.SideStatus; }
                    if (GameManager.i.playerScript.status != ActorStatus.Active)
                    { proceedFlag = false; alertType = AlertType.PlayerStatus; }
                    if (GameManager.i.optionScript.isActorRightMenu == false)
                    { proceedFlag = false; alertType = AlertType.TutorialMenuUnavailable; }
                    /*//Action Menu -> not valid if  Player inactive
                    else if (GameManager.instance.playerScript.status != ActorStatus.Active)
                    { proceedFlag = false; alertType = AlertType.PlayerStatus; }*/
                    //proceed
                    if (proceedFlag == true)
                    {
                            //adjust position prior to sending
                            Vector3 position = transform.position;
                            position.x += 25;
                            position.y -= 50;
                            position = Camera.main.ScreenToWorldPoint(position);
                            //actor
                            ModalGenericMenuDetails details = new ModalGenericMenuDetails()
                            {
                                itemID = 1,
                                itemName = GameManager.i.playerScript.PlayerName,
                                itemDetails = menuHeaderRightClick,
                                menuPos = position,
                                listOfButtonDetails = GameManager.i.actorScript.GetPlayerActions(),
                                menuType = ActionMenuType.Player
                            };
                        //close Player tooltip
                        GameManager.i.tooltipPlayerScript.CloseTooltip();
                            //activate menu
                            GameManager.i.actionMenuScript.SetActionMenu(details);
                    }
                    else
                    {
                        //explanatory message
                        if (alertType != AlertType.None)
                        { GameManager.i.guiScript.SetAlertMessageModalOne(alertType); }
                    }
                }
                break;
            default:
                Debug.LogWarningFormat("Unknown InputButton \"{0}\"", eventData.button);
                break;
        }
    }

}
