using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using gameAPI;
using modalAPI;
using TMPro;

/// <summary>
/// Attached to each option in InventoryUI
/// </summary>
public class InventoryInteraction : MonoBehaviour, IPointerClickHandler
{

    public Image optionImage;
    public TextMeshProUGUI textUpper;
    public TextMeshProUGUI textLower;

    [HideInInspector] public int optionData;                                            //multipurpose field to hold ID of actor, etc.
    [HideInInspector] public string optionName;                                         //multipurpose field for key name fields, eg. gear
    [HideInInspector] public InventoryState type;

    /// <summary>
    /// Mouse click -> Right: Actor Action Menu
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        bool proceedFlag = true;
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                
                //display gear info -> TO DO

                break;
            case PointerEventData.InputButton.Right:
                if (GameManager.instance.guiScript.CheckIsBlocked(2) == false)
                {
                    //Action Menu -> not valid if Resistance Plyr and player captured, etc.
                    if (GameManager.instance.playerScript.status != ActorStatus.Active)
                    { proceedFlag = false; }
                    if (proceedFlag == true)
                    {
                        switch (type)
                        {
                            case InventoryState.Gear:
                                Gear gear = GameManager.instance.dataScript.GetGear(optionName);
                                if (gear != null)
                                {
                                    //adjust position prior to sending
                                    Vector3 position = transform.position;
                                    position.x += 25;
                                    position.y -= 50;
                                    position = Camera.main.ScreenToWorldPoint(position);
                                    //gear
                                    ModalGenericMenuDetails details = new ModalGenericMenuDetails()
                                    {
                                        itemID = gear.gearID,
                                        itemName = gear.tag,
                                        modalLevel = 2,
                                        modalState = ModalSubState.Inventory,
                                        itemDetails = string.Format("{0} ID {1}", gear.type.name, gear.gearID),
                                        menuPos = position,
                                        listOfButtonDetails = GameManager.instance.actorScript.GetGearInventoryActions(gear.name),
                                        menuType = ActionMenuType.Gear
                                    };
                                    //activate menu
                                    GameManager.instance.actionMenuScript.SetActionMenu(details);

                                }
                                else
                                {
                                    Debug.LogError(string.Format("Invalid Gear (Null) for gearID / optionData {0}", optionData));
                                }
                                break;
                            case InventoryState.ReservePool:
                                Actor actor = GameManager.instance.dataScript.GetActor(optionData);
                                if (actor != null)
                                {
                                    //adjust position prior to sending
                                    Vector3 position = transform.position;
                                    position.x += 25;
                                    position.y -= 50;
                                    position = Camera.main.ScreenToWorldPoint(position);
                                    //gear
                                    ModalGenericMenuDetails details = new ModalGenericMenuDetails()
                                    {
                                        itemID = actor.actorID,
                                        itemName = actor.actorName,
                                        modalLevel = 2,
                                        modalState = ModalSubState.Inventory,
                                        itemDetails = string.Format("{0} ID {1}", actor.arc.name, actor.actorID),
                                        menuPos = position,
                                        listOfButtonDetails = GameManager.instance.actorScript.GetReservePoolActions(actor.actorID),
                                        menuType = ActionMenuType.Reserve
                                    };
                                    //activate menu
                                    GameManager.instance.actionMenuScript.SetActionMenu(details);

                                }
                                else
                                {
                                    Debug.LogError(string.Format("Invalid Actor (Null) for actorID / optionData {0}", optionData));
                                }
                                break;
                            default:
                                Debug.LogError(string.Format("Invalid InventoryType \"{0}\"", type));
                                break;
                        }
                    }
                }
                break;
            default:
                Debug.LogError("Unknown InputButton");
                break;
        }
    }



    //new methods above here
}
