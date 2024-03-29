﻿using gameAPI;
using modalAPI;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// handles mouse clicks on actor images
/// </summary>
public class ActorClickUI : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public int actorSlotID = 0;

    public void Awake()
    {
        /*Debug.Log(string.Format("ActorClickUI: actorSlotID {0}{1}", actorSlotID, "\n"));*/
    }

    /// <summary>
    /// Mouse click -> Left: Dossier, Right: Actor Action Menu
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.i.optionScript.isSubordinates == true)
        {
            GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
            bool proceedFlag = true;
            int data = -1;
            AlertType alertType = AlertType.None;
            //is there an actor in this slot?
            if (GameManager.i.dataScript.CheckActorSlotStatus(actorSlotID, playerSide) == true)
            {
                //close actor tooltip
                GameManager.i.tooltipActorScript.CloseTooltip("ActorClickUI.cs -> OnPointerClick");
                //which button
                switch (eventData.button)
                {
                    case PointerEventData.InputButton.Left:
                        if (GameManager.i.optionScript.isActorLeftMenu == true)
                        {
                            //actor review
                            TabbedUIData tabbedDetails = new TabbedUIData()
                            {
                                side = playerSide,
                                who = TabbedUIWho.Subordinates,
                                slotID = GameManager.i.dataScript.GetActorPosition(actorSlotID, playerSide),
                            };
                            EventManager.i.PostNotification(EventType.TabbedOpen, this, tabbedDetails, "ActorClickUI.cs -> OnPointerClick");
                        }
                        else { GameManager.i.guiScript.SetAlertMessageModalOne(AlertType.TutorialDossierUnavailable); }
                        break;
                    case PointerEventData.InputButton.Right:
                        if (GameManager.i.guiScript.CheckIsBlocked() == false)
                        {
                            //Action Menu -> not valid if AI is active for side
                            if (GameManager.i.sideScript.CheckInteraction() == false)
                            { proceedFlag = false; alertType = AlertType.SideStatus; }
                            //Action Menu -> not valid if  Player inactive
                            else if (GameManager.i.playerScript.Status != ActorStatus.Active)
                            { proceedFlag = false; alertType = AlertType.PlayerStatus; }
                            else if (GameManager.i.optionScript.isActorRightMenu == false)
                            { proceedFlag = false; alertType = AlertType.TutorialMenuUnavailable; }
                            Actor actor = GameManager.i.dataScript.GetCurrentActor(actorSlotID, playerSide);
                            if (actor != null)
                            {
                                if (actor.Status != ActorStatus.Active)
                                { proceedFlag = false; alertType = AlertType.ActorStatus; data = actor.actorID; }
                                //proceed
                                if (proceedFlag == true)
                                {
                                    //adjust position prior to sending
                                    Vector3 position = transform.position;
                                    position.x += 25;
                                    position.y -= 50;
                                    position = Camera.main.ScreenToWorldPoint(position);
                                    //actor
                                    ModalGenericMenuDetails genericDetails = new ModalGenericMenuDetails()
                                    {
                                        itemID = actor.slotID,
                                        itemName = actor.actorName,
                                        itemDetails = string.Format("{0} ID {1}", actor.arc.name, actor.actorID),
                                        menuPos = position,
                                        listOfButtonDetails = GameManager.i.actorScript.GetActorActions(actorSlotID),
                                        menuType = ActionMenuType.Actor
                                    };
                                    //activate menu
                                    GameManager.i.actionMenuScript.SetActionMenu(genericDetails);
                                }
                                else
                                {
                                    //explanatory message
                                    if (alertType != AlertType.None)
                                    { GameManager.i.guiScript.SetAlertMessageModalOne(alertType, data); }
                                }
                            }
                            else { Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", actorSlotID)); }
                        }
                        break;
                }
            }
        }
    }


}
