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

    public TextMeshProUGUI textTop;                                 //text above image, eg. compatibility stars
    public TextMeshProUGUI textUpper;
    public TextMeshProUGUI textLower;
    //public GenericTooltipUI tooltipSprite;                        //EDIT -> tooltip component attached to gameobject rather than sprite as can't get mouseover and click detection from sprite without code change
    public GenericTooltipUI tooltipStars;
    public GenericTooltipUI tooltipCompatibility;

    [HideInInspector] public int optionData;                                            //multipurpose field to hold ID of actor, etc.
    [HideInInspector] public string optionName;                                         //multipurpose field for key name fields, eg. gear
    [HideInInspector] public ModalInventorySubState type;


    public void Awake()
    {
        Debug.Assert(optionImage != null, "Invalid optionImage (Null)");
        Debug.Assert(textTop != null, "Invalid textTop (Null)");
        Debug.Assert(textUpper != null, "Invalid textUpper (Null)");
        Debug.Assert(textLower != null, "Invalid textLower (Null)");
        Debug.Assert(tooltipStars != null, "Invalid tooltipStars (Null)");
    }
    /// <summary>
    /// Mouse click -> Right: Actor Action Menu
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        GlobalSide side = GameManager.i.sideScript.PlayerSide;
        bool proceedFlag = true;
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:

                //display gear info -> TO DO
                Debug.LogFormat("[Tst] InventoryInteraction.cs -> OnPointerClick: LEFT BUTTON CLICKED{0}", "\n");

                break;
            case PointerEventData.InputButton.Right:
                if (GameManager.i.guiScript.CheckIsBlocked(2) == false)
                {
                    //Action Menu -> not valid if Resistance Plyr and player captured, etc.
                    if (GameManager.i.playerScript.status != ActorStatus.Active)
                    { proceedFlag = false; }
                    if (proceedFlag == true)
                    {
                        switch (type)
                        {
                            case ModalInventorySubState.Gear:
                                Gear gear = GameManager.i.dataScript.GetGear(optionName);
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
                                        /*itemID = gear.gearID,*/
                                        itemName = gear.tag,
                                        modalLevel = 2,
                                        modalState = ModalSubState.Inventory,
                                        itemDetails = string.Format("{0}", gear.type.name),
                                        menuPos = position,
                                        listOfButtonDetails = GameManager.i.actorScript.GetGearInventoryActions(gear.name),
                                        menuType = ActionMenuType.Gear
                                    };
                                    //activate menu
                                    GameManager.i.actionMenuScript.SetActionMenu(details);

                                }
                                else
                                { Debug.LogError(string.Format("Invalid Gear (Null) for gearID / optionData {0}", optionData)); }
                                break;
                            case ModalInventorySubState.ReservePool:
                                Actor actor = GameManager.i.dataScript.GetActor(optionData);
                                if (actor != null)
                                {
                                    //adjust position prior to sending
                                    Vector3 position = transform.position;
                                    position.x += 25;
                                    position.y -= 50;
                                    position = Camera.main.ScreenToWorldPoint(position);
                                    //reserve actions menu
                                    ModalGenericMenuDetails details = new ModalGenericMenuDetails()
                                    {
                                        itemID = actor.actorID,
                                        itemName = actor.actorName,
                                        modalLevel = 2,
                                        modalState = ModalSubState.Inventory,
                                        itemDetails = string.Format("{0} ID {1}", actor.arc.name, actor.actorID),
                                        menuPos = position,
                                        listOfButtonDetails = GameManager.i.actorScript.GetReservePoolActions(actor.actorID),
                                        menuType = ActionMenuType.Reserve
                                    };
                                    //activate menu
                                    GameManager.i.actionMenuScript.SetActionMenu(details);

                                }
                                else
                                { Debug.LogError(string.Format("Invalid Actor (Null) for actorID / optionData {0}", optionData));  }
                                break;
                            default:
                                Debug.LogError(string.Format("Invalid InventoryType \"{0}\"", type));
                                break;
                        }
                    }
                    else
                    {
                        //player not active
                        GameManager.i.guiScript.SetAlertMessage(AlertType.PlayerStatus);
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
