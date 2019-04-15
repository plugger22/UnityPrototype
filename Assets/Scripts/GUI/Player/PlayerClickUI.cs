﻿using System.Collections;
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
        menuHeaderRightClick = GameManager.instance.actionScript.GetPlayerActionMenuHeader();
        Debug.Assert(String.IsNullOrEmpty(menuHeaderRightClick) == false, "Invalid menuHeaderRightClick (Null or empty)");
    }

    /// <summary>
    /// Mouse click -> Right: Actor Action Menu
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        bool proceedFlag = true;
        AlertType alertType = AlertType.None;

        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                break;
            case PointerEventData.InputButton.Right:
                if (GameManager.instance.guiScript.CheckIsBlocked() == false)
                {
                    //Action Menu -> not valid if AI is active for side
                    if (GameManager.instance.sideScript.CheckInteraction() == false)
                    { proceedFlag = false; alertType = AlertType.SideStatus; }
                    if (GameManager.instance.playerScript.status != ActorStatus.Active)
                    { proceedFlag = false; alertType = AlertType.PlayerStatus; }
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
                                itemName = GameManager.instance.playerScript.PlayerName,
                                itemDetails = menuHeaderRightClick,
                                menuPos = position,
                                listOfButtonDetails = GameManager.instance.actorScript.GetPlayerActions(),
                                menuType = ActionMenuType.Player
                            };
                            //activate menu
                            GameManager.instance.actionMenuScript.SetActionMenu(details);
                    }
                    else
                    {
                        //explanatory message
                        if (alertType != AlertType.None)
                        { GameManager.instance.guiScript.SetAlertMessage(alertType); }
                    }
                }
                break;
            default:
                Debug.LogWarningFormat("Unknown InputButton \"{0}\"", eventData.button);
                break;
        }
    }

}
