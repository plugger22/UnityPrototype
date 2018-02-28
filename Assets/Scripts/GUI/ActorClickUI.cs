﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using gameAPI;
using modalAPI;


/// <summary>
/// handles mouse clicks on actor images
/// </summary>
public class ActorClickUI : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public int actorSlotID = 0;

    public void Awake()
    {
        //actorSlotID = GetComponentInParent<ActorHighlightUI>().actorSlotID;
        Debug.Log(string.Format("ActorClickUI: actorSlotID {0}{1}", actorSlotID, "\n"));
    }

    /// <summary>
    /// Mouse click -> Right: Actor Action Menu
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log(string.Format("ActorClickUI: Button Clicked slotID {0}{1}", actorSlotID, "\n"));
        bool proceedFlag = true;
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                break;
            case PointerEventData.InputButton.Right:
                if (GameManager.instance.guiScript.CheckIsBlocked() == false)
                {
                    //Action Menu -> not valid if Resistance Plyr and player captured, etc.
                    if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
                    {
                        if (GameManager.instance.turnScript.resistanceState != ResistanceState.Normal)
                        { proceedFlag = false; }
                    }
                    if (proceedFlag == true)
                    {
                        Actor actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, GameManager.instance.sideScript.PlayerSide);
                        if (actor != null)
                        {
                            //adjust position prior to sending
                            Vector3 position = transform.position;
                            position.x += 25;
                            position.y -= 50;
                            position = Camera.main.ScreenToWorldPoint(position);
                            //actor
                            ModalPanelDetails details = new ModalPanelDetails()
                            {
                                itemID = actor.actorSlotID,
                                itemName = actor.actorName,
                                itemDetails = string.Format("{0} ID {1}", actor.arc.name, actor.actorID),
                                itemPos = position,
                                listOfButtonDetails = GameManager.instance.actorScript.GetActorActions(actorSlotID),
                                menuType = ActionMenuType.Actor
                            };
                            //activate menu
                            GameManager.instance.actionMenuScript.SetActionMenu(details);
                        }
                        else { Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", actorSlotID)); }
                    }
                }
                break;
            default:
                Debug.LogError("Unknown InputButton");
                break;
        }
    }


}
