using gameAPI;
using modalAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Handles Inventory related UI's, eg. Reserve Actor pool (both sides) and Player Gear (Resistance)
/// </summary>
public class ModalInventoryUI : MonoBehaviour
{

    public GameObject modalInventoryObject;
    public GameObject modalPanelObject;
    public GameObject modalHeaderObject;
    public Image modalPanel;
    public Image headerPanel;

    public TextMeshProUGUI topText;
    public TextMeshProUGUI bottomText;
    public TextMeshProUGUI headerText;

    public Button buttonCancel;

    public GameObject[] arrayOfInventoryOptions;                //place Inventory option UI elements here (up to 4 options)
    private InventoryInteraction[] arrayOfInteractions;         //used for fast access to interaction components
    private GenericTooltipUI[] arrayOfTooltips;                 //used for fast access to tooltip components

    private static ModalInventoryUI modalInventoryUI;
    private ButtonInteraction buttonInteraction;

    /*private int optionIDSelected;                             //slot ID (eg arrayOfGenericOptions [index] of selected option
    private string optionTextSelected;                        //used for nested Generic Picker windows, ignore otherwise
    private int nodeIDSelected;
    private int actorSlotIDSelected;
    private EventType defaultReturnEvent;                          //event to trigger once confirmation button is clicked
    private EventType backReturnEvent;                  //event triggered when back button clicked (dynamic -> SetBackButton)
    private ModalActionDetails nestedDetails;           //used only if there are multiple, nested, option windows (dynamic -> InitialiseNestedOptions)*/

    private string colourEffect;
    private string colourSide;
    private string colourTeam;
    private string colourDefault;
    private string colourNormal;
    private string colourGood;
    private string colourBad;
    private string colourEnd;

    /// <summary>
    /// Static instance so the Modal Generic Picker can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalInventoryUI Instance()
    {
        if (!modalInventoryUI)
        {
            modalInventoryUI = FindObjectOfType(typeof(ModalInventoryUI)) as ModalInventoryUI;
            if (!modalInventoryUI)
            { Debug.LogError("There needs to be one active ModalInventoryUI script on a GameObject in your scene"); }
        }
        return modalInventoryUI;
    }


    private void Awake()
    {
        //cancel button event
        buttonInteraction = buttonCancel.GetComponent<ButtonInteraction>();
        if (buttonInteraction != null)
        { buttonInteraction.SetEvent(EventType.InventoryCloseUI); }
        else { Debug.LogError("Invalid buttonInteraction Cancel (Null)"); }
        //inventory interaction & tooltip arrays set up
        int numOfOptions = arrayOfInventoryOptions.Length;
        arrayOfInteractions = new InventoryInteraction[numOfOptions];
        arrayOfTooltips = new GenericTooltipUI[numOfOptions];
        for (int i = 0; i < numOfOptions; i++)
        {
            if (arrayOfInventoryOptions[i] != null)
            {
                //interaction
                InventoryInteraction interaction = arrayOfInventoryOptions[i].GetComponent<InventoryInteraction>();
                if (interaction != null)
                { arrayOfInteractions[i] = interaction; }
                else { Debug.LogError(string.Format("Invalid InventoryInteraction for arrayOfInventoryOptions[{0}] (Null)", i)); }
                //tooltip
                GenericTooltipUI tooltip = arrayOfInventoryOptions[i].GetComponent<GenericTooltipUI>();
                if (tooltip != null)
                { arrayOfTooltips[i] = tooltip; }
                else { Debug.LogError(string.Format("Invalid GenericTooltipUI for arrayOfInventoryOptions[{0}] (Null)", i)); }
            }
            else { Debug.LogError(string.Format("Invalid arrayOfInventoryOptions[{0}] (Null)", i)); }
        }

    }


    private void Start()
    {        
        //register listener
        EventManager.instance.AddListener(EventType.InventoryOpenUI, OnEvent);
        EventManager.instance.AddListener(EventType.InventoryCloseUI, OnEvent);
    }

    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //select event type
        switch (eventType)
        {
            case EventType.InventoryOpenUI:
                InventoryInputData details = Param as InventoryInputData;
                SetInventoryUI(details);
                break;
            case EventType.InventoryCloseUI:
                CloseInventoryUI();
                break;
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        colourEffect = GameManager.instance.colourScript.GetColour(ColourType.actionEffect);
        colourSide = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourTeam = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }


    /// <summary>
    /// Open up Inventory UI
    /// </summary>
    /// <param name="details"></param>
    private void SetInventoryUI(InventoryInputData details)
    {
        bool errorFlag = false;
        //CanvasGroup inventoryCanvasGroup;
        //InventoryInteraction optionInteraction;
        //set modal status
        GameManager.instance.guiScript.SetIsBlocked(true);
        //activate main panel
        modalPanelObject.SetActive(true);
        modalInventoryObject.SetActive(true);
        modalHeaderObject.SetActive(true);

        /*//close node tooltip (safety check)
        GameManager.instance.tooltipNodeScript.CloseTooltip();*/
        //populate dialogue
        if (details != null)
        {
            //set up modal panel & buttons to be side appropriate
            switch (details.side.name)
            {
                case "Authority":
                    modalPanel.sprite = GameManager.instance.sideScript.inventory_background_Authority;
                    headerPanel.sprite = GameManager.instance.sideScript.header_background_Authority;
                    //set button sprites
                    buttonCancel.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Authority;
                    //set sprite transitions
                    SpriteState spriteStateAuthority = new SpriteState();
                    spriteStateAuthority.highlightedSprite = GameManager.instance.sideScript.button_highlight_Authority;
                    spriteStateAuthority.pressedSprite = GameManager.instance.sideScript.button_Click;
                    buttonCancel.spriteState = spriteStateAuthority;
                    break;
                case "Resistance":
                    modalPanel.sprite = GameManager.instance.sideScript.inventory_background_Rebel;
                    headerPanel.sprite = GameManager.instance.sideScript.header_background_Rebel;
                    //set button sprites
                    buttonCancel.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Rebel;
                    //set sprite transitions
                    SpriteState spriteStateRebel = new SpriteState();
                    spriteStateRebel.highlightedSprite = GameManager.instance.sideScript.button_highlight_Rebel;
                    spriteStateRebel.pressedSprite = GameManager.instance.sideScript.button_Click;
                    buttonCancel.spriteState = spriteStateRebel;
                    break;
                default:
                    Debug.LogError(string.Format("Invalid side \"{0}\"", details.side.name));
                    break;
            }
            //set texts
            headerText.text = details.textHeader;
            topText.text = details.textTop;
            bottomText.text = details.textBottom;
            //loop array and set options
            for (int i = 0; i < details.arrayOfOptions.Length; i++)
            {
                //valid option?
                if (arrayOfInventoryOptions[i] != null)
                {
                    //optionInteraction = arrayOfInventoryOptions[i].GetComponent<InventoryInteraction>();
                    if (arrayOfInteractions[i] != null)
                    {
                        if (details.arrayOfOptions[i] != null)
                        {
                            //activate option
                            arrayOfInventoryOptions[i].SetActive(true);
                            //populate option data
                            arrayOfInteractions[i].optionImage.sprite = details.arrayOfOptions[i].sprite;
                            arrayOfInteractions[i].textUpper.text = details.arrayOfOptions[i].textUpper;
                            arrayOfInteractions[i].textLower.text = details.arrayOfOptions[i].textLower;
                            //tooltip data
                            if (arrayOfTooltips[i] != null)
                            {
                                if (details.arrayOfTooltips[i] != null)
                                {
                                    arrayOfTooltips[i].toolTipHeader = details.arrayOfTooltips[i].textHeader;
                                    arrayOfTooltips[i].toolTipMain = details.arrayOfTooltips[i].textMain;
                                    arrayOfTooltips[i].toolTipEffect = details.arrayOfTooltips[i].textDetails;
                                }
                                else { Debug.LogWarning(string.Format("Invalid tooltipDetails (Null) for arrayOfOptions[\"{0}\"]", i)); }
                            }
                            else
                            {
                                Debug.LogError(string.Format("Invalid GenericTooltipUI (Null) in arrayOfTooltips[{0}]", i));
                            }
                        }
                        else
                        {
                            //invalid option, switch off
                            arrayOfInventoryOptions[i].SetActive(false);
                        }
                    }
                    else
                    {
                        //error -> Null Interaction data
                        Debug.LogError(string.Format("Invalid arrayOfInventoryOptions[\"{0}\"] optionInteraction (Null)", i));
                        errorFlag = true;
                        break;
                    }
                }
                else
                {
                    //error -> Null array
                    Debug.LogError(string.Format("Invalid arrayOfInventoryOptions[\"{0}\"] (Null)", i));
                    errorFlag = true;
                    break;
                }
            }
        }
        else
        {
            Debug.LogError("Invalid InventoryInputData (Null)");
            errorFlag = true;
        }
        //error outcome message if there is a problem
        if (errorFlag == true)
        {
            modalInventoryObject.SetActive(false);
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.textTop = "There has been a hiccup and the information isn't available";
            outcomeDetails.textBottom = "The WolfMan has been called";
            outcomeDetails.side = details.side;
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
        }
        else
        {
            //all good, inventory window displayed
            GameManager.instance.inputScript.SetModalState(ModalState.Inventory);
            Debug.Log("UI: Open -> ModalInventoryUI" + "\n");
        }
    }


    /// <summary>
    /// Close Inventory UI
    /// </summary>
    private void CloseInventoryUI()
    {
        modalInventoryObject.SetActive(false);
        GameManager.instance.guiScript.SetIsBlocked(false);
        //close generic tooltip (safety check)
        GameManager.instance.tooltipGenericScript.CloseTooltip();

        /*//deselect all generic options to prevent picker opening next time with a preselected team
        EventManager.instance.PostNotification(EventType.DeselectOtherGenerics, this);
        SetConfirmButton(false);*/

        //set game state
        GameManager.instance.inputScript.ResetStates();
        Debug.Log(string.Format("UI: Close -> ModalInventoryUI{0}", "\n"));
    }

    //place new methods above here
}
