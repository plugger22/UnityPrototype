using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using modalAPI;
using gameAPI;



/// <summary>
/// handles modal Action Menu for nodes and other purposes, eg. Reserve action menu
/// </summary>
public class ModalActionMenu : MonoBehaviour
{
    //public GameObject modalActionObject;
    public Canvas menuCanvas;
    public GameObject modalMenuObject;
    public Image modalPanel;
    public Image background;
    public Image divider;
    public TextMeshProUGUI itemDetails;
    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;
    public Button button5;
    public Button button6;
    public TextMeshProUGUI button1Text;
    public TextMeshProUGUI button2Text;
    public TextMeshProUGUI button3Text;
    public TextMeshProUGUI button4Text;
    public TextMeshProUGUI button5Text;
    public TextMeshProUGUI button6Text;

    
    [HideInInspector] public string tooltip1;
    [HideInInspector] public string tooltip2;
    [HideInInspector] public string tooltip3;
    [HideInInspector] public string tooltip4;
    [HideInInspector] public string tooltip5;


    private RectTransform rectTransform;
    private int offset;
    private int modalLevel;                                 //modal level of menu, passed in by ModalPanelDetails in SetActionMenu
    private ModalSubState modalState;                       //modal state to return to if action panel closed

    //colour palette
    /*private string colourEffects;
    private string colourEnd;*/

    #region Static...

    private static ModalActionMenu modalActionMenu;

    /// <summary>
    /// Static instance so the Modal Menu can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalActionMenu Instance()
    {
        if (!modalActionMenu)
        {
            modalActionMenu = FindObjectOfType(typeof(ModalActionMenu)) as ModalActionMenu;
            if (!modalActionMenu)
            { Debug.LogError("There needs to be one active ModalActionMenu script on a GameObject in your scene"); }
        }
        return modalActionMenu;
    }
    #endregion

    #region Start
    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        //asserts
        Debug.Assert(menuCanvas != null, "Invalid menuCanvas (Null)");
        Debug.Assert(modalMenuObject != null, "Invalid modalMenuObject (Null)");
        Debug.Assert(modalPanel != null, "Invalid modalPanel (Null)");
        Debug.Assert(background != null, "Invalid background (Null)");
        Debug.Assert(divider != null, "Invalid divider (Null)");
        Debug.Assert(itemDetails != null, "Invalid itemDetails (Null)");
        Debug.Assert(button1 != null, "Invalid button1 (Null)");
        Debug.Assert(button2 != null, "Invalid button2 (Null)");
        Debug.Assert(button3 != null, "Invalid button3 (Null)");
        Debug.Assert(button4 != null, "Invalid button4 (Null)");
        Debug.Assert(button5 != null, "Invalid button5 (Null)");
        Debug.Assert(button6 != null, "Invalid button6 (Null)");
        Debug.Assert(button1Text != null, "Invalid button1Text (Null)");
        Debug.Assert(button2Text != null, "Invalid button2Text (Null)");
        Debug.Assert(button3Text != null, "Invalid button3Text (Null)");
        Debug.Assert(button4Text != null, "Invalid button4Text (Null)");
        Debug.Assert(button5Text != null, "Invalid button5Text (Null)");
        Debug.Assert(button6Text != null, "Invalid button6Text (Null)");
        //offset
        offset = GameManager.i.guiScript.tooltipOffset * 2;
        //activate object
        modalMenuObject.SetActive(true);
        //deactivate canvas
        menuCanvas.gameObject.SetActive(false);
        //register listener
        EventManager.i.AddListener(EventType.CloseActionMenu, OnEvent, "ModalActionMenu");
    }
    #endregion

    #region OnEvent
    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //select event type
        switch(eventType)
        {
            case EventType.CloseActionMenu:
                CloseActionMenu();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion

    #region SetActionMenu
    /// <summary>
    /// Initialise and activate modal Action Menu
    /// </summary>
    /// <param name="details"></param>
    public void SetActionMenu(ModalGenericMenuDetails details)
    {
        //set states (done up front to prevent node tooltip reoccuring during menu display)
        ModalStateData package = new ModalStateData() { mainState = ModalSubState.ActionMenu };
        GameManager.i.inputScript.SetModalState(package);
        //close all tooltips
        GameManager.i.guiScript.SetTooltipsOff();
        //check enough actions, or tutorial mode with isActions switched off
        if (GameManager.i.turnScript.CheckRemainingActions() == true || (GameManager.i.optionScript.isActions == false && GameManager.i.inputScript.GameState == GameState.Tutorial))
        {
            //modalActionObject.SetActive(true);
            menuCanvas.gameObject.SetActive(true);

            //set all states to off
            button1.gameObject.SetActive(false);
            button2.gameObject.SetActive(false);
            button3.gameObject.SetActive(false);
            button4.gameObject.SetActive(false);
            button5.gameObject.SetActive(false);
            button6.gameObject.SetActive(false);

            //set up ModalActionObject
            itemDetails.text = string.Format("{0}{1}{2}", details.itemName, "\n", details.itemDetails);
            //tooltip at top of menu -> pass through data
            ModalMenuUI modal = itemDetails.GetComponent<ModalMenuUI>();
            modal.menuType = details.menuType;
            switch (details.menuType)
            {
                case ActionMenuType.Node:
                case ActionMenuType.NodeGear:
                    modal.nodeID = details.itemID;
                    break;
                case ActionMenuType.Actor:
                    modal.actorSlotID = details.itemID;
                    break;
                case ActionMenuType.Player:
                    break;
                case ActionMenuType.Gear:
                    modal.gearName = details.itemKey;
                    break;
            }
            //There can be a max of 6 buttons (5 plus 1 x Cancel)
            int counter = 0;
            Button tempButton;
            TextMeshProUGUI title;
            foreach (EventButtonDetails buttonDetails in details.listOfButtonDetails)
            {
                tempButton = null;
                title = null;
                counter++;
                //get the relevent UI elements
                switch (counter)
                {
                    case 1:
                        tempButton = button1;
                        title = button1Text;
                        break;
                    case 2:
                        tempButton = button2;
                        title = button2Text;
                        break;
                    case 3:
                        tempButton = button3;
                        title = button3Text;
                        break;
                    case 4:
                        tempButton = button4;
                        title = button4Text;
                        break;
                    case 5:
                        tempButton = button5;
                        title = button5Text;
                        break;
                    case 6:
                        tempButton = button6;
                        title = button6Text;
                        break;
                    default:
                        Debug.LogWarning("To many EventButtonDetails in list!\n");
                        break;
                }
                //set up the UI elements
                if (tempButton != null && title != null)
                {
                    tempButton.onClick.RemoveAllListeners();
                    tempButton.onClick.AddListener(CloseActionMenu);
                    tempButton.onClick.AddListener(buttonDetails.action);
                    title.text = buttonDetails.buttonTitle;
                    tempButton.gameObject.SetActive(true);
                    GenericTooltipUI generic = tempButton.GetComponent<GenericTooltipUI>();
                    generic.tooltipHeader = buttonDetails.buttonTooltipHeader;
                    generic.tooltipMain = buttonDetails.buttonTooltipMain;
                    generic.tooltipDetails = buttonDetails.buttonTooltipDetail;
                    generic.x_offset = 40;
                }
            }

            //convert coordinates
            Vector3 screenPos = Camera.main.WorldToScreenPoint(details.menuPos);
            //update rectTransform to get a correct height as it changes every time with the dynamic menu resizing depending on number of buttons
            Canvas.ForceUpdateCanvases();
            rectTransform = modalMenuObject.GetComponent<RectTransform>();
            //get dimensions of dynamic menu
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;
            //calculate offset - height (default above)
            if (screenPos.y + height + offset < Screen.height)
            { screenPos.y += height + offset; }
            else { screenPos.y -= offset; }
            //width - default right
            if (screenPos.x + offset >= Screen.width)
            { screenPos.x -= offset + screenPos.x - Screen.width; }
            //go left if needed
            else if (screenPos.x - offset - width <= 0)
            { screenPos.x += offset - width; }
            else
            { screenPos.x += offset; }
            //set new position
            modalMenuObject.transform.position = screenPos;
            //block raycasts to gameobjects
            GameManager.i.guiScript.SetIsBlocked(true, details.modalLevel);
            modalLevel = details.modalLevel;
            modalState = details.modalState;
            Debug.LogFormat("[UI] ModalActionMenu.cs -> SetActionMenu{0}", "\n");
        }
        else
        {
            //insufficient actions remaining -> create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = GameManager.i.sideScript.PlayerSide;
            outcomeDetails.textTop = "You have used up all your Actions for this turn";
            //extra text if player is wounded
            if (GameManager.i.turnScript.CheckPlayerWounded() == true)
            { outcomeDetails.textBottom = "Maximum ONE Action allowed while WOUNDED"; }
            outcomeDetails.sprite = GameManager.i.spriteScript.infoSprite;
            outcomeDetails.modalLevel = details.modalLevel;
            outcomeDetails.modalState = details.modalState;
            EventManager.i.PostNotification(EventType.OutcomeOpen, this, outcomeDetails, "ModalActionMenu.cs -> SetActionMenu");
        }
    }
    #endregion

    #region CloseActionMenu
    /// <summary>
    /// close Action Menu
    /// </summary>
    public void CloseActionMenu()
    {
        //modalActionObject.SetActive(false);
        menuCanvas.gameObject.SetActive(false);
        GameManager.i.guiScript.SetIsBlocked(false, modalLevel);
        //remove highlight from node
        GameManager.i.nodeScript.ToggleNodeHighlight();
        //close Alert UI safety check (ignored if not active)
        GameManager.i.alertScript.CloseAlertUI();
        //close toolips (eg. from button)
        GameManager.i.guiScript.SetTooltipsOff();

        /*GameManager.i.tooltipGenericScript.CloseTooltip("ModalActionMenu -> CloseActionMenu");*/

        //set game state
        GameManager.i.inputScript.ResetStates(modalState);
        Debug.LogFormat("[UI] ModalActionMenu.cs -> CloseActionMenu{0}", "\n");
    }
    #endregion

    #region CheckActionMenu
    /// <summary>
    /// returns true if action menu active, false otherwise
    /// </summary>
    /// <returns></returns>
    public bool CheckActionMenu()
    { return menuCanvas.gameObject.activeSelf; }
    #endregion

}


