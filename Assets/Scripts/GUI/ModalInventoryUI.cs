using delegateAPI;
using gameAPI;
using modalAPI;
using packageAPI;
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
    public Button buttonHelp;

    public GameObject[] arrayOfInventoryOptions;                //place Inventory option UI elements here (up to 4 options)
    private InventoryInteraction[] arrayOfInteractions;         //used for fast access to interaction components
    private GenericTooltipUI[] arrayOfTooltipsSprites;          //used for fast access to tooltip components (Sprites)
    private GenericTooltipUI[] arrayOfTooltipsStars;            //used for fast access to tooltip components for Stars (bottomText)
    private GenericTooltipUI[] arrayOfTooltipsCompatibility;    //used for fast access to tooltip components for Compatibility (topText)

    private static ModalInventoryUI modalInventoryUI;
    private ButtonInteraction buttonInteraction;
    private GenericHelpTooltipUI help;

    private InventoryDelegate handler;                          //method to be called for an option refresh (passed into SetInventoryUI)



    /// <summary>
    /// Static instance so the ModalInventoryUI can be accessed from any script
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
        //Asserts
        Debug.Assert(buttonHelp != null, "Invalid GenericHelpTooltipUI (Null)");
        Debug.Assert(buttonCancel != null, "Invalid buttonCancel (Null)");
        //cancel button event
        buttonInteraction = buttonCancel.GetComponent<ButtonInteraction>();
        if (buttonInteraction != null)
        { buttonInteraction.SetButton(EventType.InventoryCloseUI); }
        else { Debug.LogError("Invalid buttonInteraction Cancel (Null)"); }
        //help button
        help = buttonHelp.GetComponent<GenericHelpTooltipUI>();
        if (help == null) { Debug.LogError("Invalid help script (Null)"); }
        //inventory interaction & tooltip arrays set up
        int numOfOptions = arrayOfInventoryOptions.Length;
        arrayOfInteractions = new InventoryInteraction[numOfOptions];
        arrayOfTooltipsSprites = new GenericTooltipUI[numOfOptions];
        arrayOfTooltipsStars = new GenericTooltipUI[numOfOptions];
        arrayOfTooltipsCompatibility = new GenericTooltipUI[numOfOptions];
        for (int i = 0; i < numOfOptions; i++)
        {
            if (arrayOfInventoryOptions[i] != null)
            {
                //interaction
                InventoryInteraction interaction = arrayOfInventoryOptions[i].GetComponent<InventoryInteraction>();
                if (interaction != null)
                {
                    arrayOfInteractions[i] = interaction;
                    //tooltip -> sprite (attached to game object to prevent tooltip component masking gameobject interaction component which is needed for click detection for menu's)
                    GenericTooltipUI tooltipSprite = arrayOfInventoryOptions[i].GetComponent<GenericTooltipUI>();
                    if (tooltipSprite != null)
                    { arrayOfTooltipsSprites[i] = tooltipSprite; }
                    else { Debug.LogError(string.Format("Invalid GenericTooltipUI for arrayOfInventoryOptions[{0}] (Null)", i)); }
                    //tooltip -> stars (bottomText, optional)
                    GenericTooltipUI tooltipStars = interaction.tooltipStars.GetComponent<GenericTooltipUI>();
                    if (tooltipStars != null)
                    { arrayOfTooltipsStars[i] = tooltipStars; }
                    else { Debug.LogError(string.Format("Invalid GenericTooltipUI for interaction.tooltipStars \"{0}\" (Null)", i)); }
                    //tooltip -> compatibility (topText, optional)
                    GenericTooltipUI tooltipCompatibility = interaction.tooltipCompatibility.GetComponent<GenericTooltipUI>();
                    if (tooltipCompatibility != null)
                    { arrayOfTooltipsCompatibility[i] = tooltipCompatibility; }
                    else { Debug.LogError(string.Format("Invalid GenericTooltipUI for interaction.tooltipCompatibility \"{0}\" (Null)", i)); }
                }
                else { Debug.LogError(string.Format("Invalid InventoryInteraction for arrayOfInventoryOptions[{0}] (Null)", i)); }
            }
            else { Debug.LogError(string.Format("Invalid arrayOfInventoryOptions[{0}] (Null)", i)); }
        }

    }


    private void Start()
    {
        //register listener
        EventManager.instance.AddListener(EventType.InventoryOpenUI, OnEvent, "ModalInventoryUI");
        EventManager.instance.AddListener(EventType.InventoryCloseUI, OnEvent, "ModalInventoryUI");
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
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// Open Inventory UI
    /// </summary>
    /// <param name="details"></param>
    private void SetInventoryUI(InventoryInputData details)
    {
        bool errorFlag = false;
        //set modal status
        GameManager.i.guiScript.SetIsBlocked(true);
        //tooltips off
        GameManager.i.guiScript.SetTooltipsOff();
        //activate main panel
        modalPanelObject.SetActive(true);
        modalInventoryObject.SetActive(true);
        modalHeaderObject.SetActive(true);
        //delegate method to be called in the event of refresh
        handler = details.handler;
        //populate dialogue
        if (details != null)
        {
            //set up modal panel & buttons to be side appropriate
            switch (details.side.name)
            {
                case "Authority":
                    modalPanel.sprite = GameManager.i.sideScript.inventory_background_Authority;
                    headerPanel.sprite = GameManager.i.sideScript.header_background_Authority;
                    //set button sprites
                    buttonCancel.GetComponent<Image>().sprite = GameManager.i.sideScript.button_Authority;
                    //set sprite transitions
                    SpriteState spriteStateAuthority = new SpriteState();
                    spriteStateAuthority.highlightedSprite = GameManager.i.sideScript.button_highlight_Authority;
                    spriteStateAuthority.pressedSprite = GameManager.i.sideScript.button_Click;
                    buttonCancel.spriteState = spriteStateAuthority;
                    break;
                case "Resistance":
                    modalPanel.sprite = GameManager.i.sideScript.inventory_background_Resistance;
                    headerPanel.sprite = GameManager.i.sideScript.header_background_Resistance;
                    //set button sprites
                    buttonCancel.GetComponent<Image>().sprite = GameManager.i.sideScript.button_Resistance;
                    //set sprite transitions
                    SpriteState spriteStateRebel = new SpriteState();
                    spriteStateRebel.highlightedSprite = GameManager.i.sideScript.button_highlight_Resistance;
                    spriteStateRebel.pressedSprite = GameManager.i.sideScript.button_Click;
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
            //set help
            List<HelpData> listOfHelpData = GameManager.i.helpScript.GetHelpData(details.help0, details.help1, details.help2, details.help3);
            if (listOfHelpData != null && listOfHelpData.Count > 0)
            {
                buttonHelp.gameObject.SetActive(true);
                help.SetHelpTooltip(listOfHelpData, 150, 200);
            }
            else { buttonHelp.gameObject.SetActive(false); }
            //loop array and set options
            for (int i = 0; i < details.arrayOfOptions.Length; i++)
            {
                //valid option?
                if (arrayOfInventoryOptions[i] != null)
                {
                    if (arrayOfInteractions[i] != null)
                    {
                        if (details.arrayOfOptions[i] != null)
                        {
                            //activate option
                            arrayOfInventoryOptions[i].SetActive(true);
                            //populate option data
                            arrayOfInteractions[i].optionImage.sprite = details.arrayOfOptions[i].sprite;
                            arrayOfInteractions[i].textTop.text = details.arrayOfOptions[i].textTop;
                            arrayOfInteractions[i].textUpper.text = details.arrayOfOptions[i].textUpper;
                            arrayOfInteractions[i].textLower.text = details.arrayOfOptions[i].textLower;
                            arrayOfInteractions[i].optionData = details.arrayOfOptions[i].optionID;
                            arrayOfInteractions[i].optionName = details.arrayOfOptions[i].optionName;
                            arrayOfInteractions[i].type = details.state;
                            //tooltip data -> sprites
                            if (arrayOfTooltipsSprites[i] != null)
                            {
                                if (details.arrayOfTooltipsSprite[i] != null)
                                {
                                    arrayOfTooltipsSprites[i].tooltipHeader = details.arrayOfTooltipsSprite[i].textHeader;
                                    arrayOfTooltipsSprites[i].tooltipMain = details.arrayOfTooltipsSprite[i].textMain;
                                    arrayOfTooltipsSprites[i].tooltipDetails = details.arrayOfTooltipsSprite[i].textDetails;
                                    arrayOfTooltipsSprites[i].x_offset = 55;
                                }
                                else { Debug.LogWarningFormat("Invalid tooltipDetailsSprite (Null) for arrayOfOptions[{0}]", i); }
                            }
                            else
                            {
                                Debug.LogError(string.Format("Invalid GenericTooltipUI (Null) in arrayOfTooltips[{0}]", i));
                            }
                            //tooltip data -> stars
                            if (arrayOfTooltipsStars[i] != null)
                            {
                                if (details.arrayOfTooltipsStars[i] != null)
                                {
                                    arrayOfTooltipsStars[i].tooltipHeader = details.arrayOfTooltipsStars[i].textHeader;
                                    arrayOfTooltipsStars[i].tooltipMain = details.arrayOfTooltipsStars[i].textMain;
                                    arrayOfTooltipsStars[i].tooltipDetails = details.arrayOfTooltipsStars[i].textDetails;
                                    arrayOfTooltipsStars[i].x_offset = 55;
                                    arrayOfTooltipsStars[i].y_offset = 15;
                                }
                                else
                                {
                                    //this tooltip is optional, fill with blank data otherwise previously used data will be used
                                    arrayOfTooltipsStars[i].tooltipHeader = "";
                                    arrayOfTooltipsStars[i].tooltipMain = "";
                                    arrayOfTooltipsStars[i].tooltipDetails = "";
                                }
                            }
                            //tooltip data -> compatibility
                            if (arrayOfTooltipsCompatibility[i] != null)
                            {
                                if (details.arrayOfTooltipsCompatibility[i] != null)
                                {
                                    arrayOfTooltipsCompatibility[i].tooltipHeader = details.arrayOfTooltipsCompatibility[i].textHeader;
                                    arrayOfTooltipsCompatibility[i].tooltipMain = details.arrayOfTooltipsCompatibility[i].textMain;
                                    arrayOfTooltipsCompatibility[i].tooltipDetails = details.arrayOfTooltipsCompatibility[i].textDetails;
                                    arrayOfTooltipsCompatibility[i].x_offset = 55;
                                    arrayOfTooltipsCompatibility[i].y_offset = 15;
                                }
                                else
                                {
                                    //this tooltip is optional, fill with blank data otherwise previously used data will be used
                                    arrayOfTooltipsCompatibility[i].tooltipHeader = "";
                                    arrayOfTooltipsCompatibility[i].tooltipMain = "";
                                    arrayOfTooltipsCompatibility[i].tooltipDetails = "";
                                }
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
                        Debug.LogErrorFormat("Invalid arrayOfInventoryOptions[\"{0}\"] optionInteraction (Null)", i);
                        errorFlag = true;
                        break;
                    }
                }
                else
                {
                    //error -> Null array
                    Debug.LogErrorFormat("Invalid arrayOfInventoryOptions[{0}] (Null)", i);
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
            outcomeDetails.textBottom = "We've called the WolfMan. He's on his way";
            outcomeDetails.side = details.side;
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ModalInventoryUI.cs -> SetInventoryUI");
        }
        else
        {
            //all good, inventory window displayed
            ModalStateData package = new ModalStateData() { mainState = ModalSubState.Inventory };
            GameManager.i.inputScript.SetModalState(package);
            Debug.LogFormat("[UI] ModalInventoryUI.cs -> SetInventoryUI{0}", "\n");
        }
    }


    /// <summary>
    /// Close Inventory UI
    /// </summary>
    private void CloseInventoryUI()
    {
        modalInventoryObject.SetActive(false);
        GameManager.i.guiScript.SetIsBlocked(false);
        //close generic tooltip (safety check)
        GameManager.i.tooltipGenericScript.CloseTooltip("ModalInventoryUI.cs -> CloseInventory");
        //close help tooltip
        GameManager.i.tooltipHelpScript.CloseTooltip("ModalInventoryUI.cs -> CloseInventory");
        //set game state
        GameManager.i.inputScript.ResetStates();
        Debug.LogFormat("[UI] ModalInventoryUI.cs -> CloseInventoryUI{0}", "\n");
    }


    /// <summary>
    /// Updates options and redraws options after an action has been taken
    /// </summary>
    public void RefreshInventoryUI()
    {
        if (handler != null)
        {
            Debug.LogFormat("[UI] ModalInventoryUI.cs -> RefreshInventoryUI{0}", "\n");
            //call specific method to refresh data
            InventoryInputData details = handler();
            if (details != null)
            {
                //process data
                topText.text = details.textTop;
                bottomText.text = details.textBottom;
                //loop array and set options
                for (int i = 0; i < details.arrayOfOptions.Length; i++)
                {
                    //valid option?
                    if (arrayOfInventoryOptions[i] != null)
                    {
                        if (arrayOfInteractions[i] != null)
                        {
                            if (details.arrayOfOptions[i] != null)
                            {
                                //activate option
                                arrayOfInventoryOptions[i].SetActive(true);
                                //populate option data
                                arrayOfInteractions[i].optionImage.sprite = details.arrayOfOptions[i].sprite;
                                arrayOfInteractions[i].textTop.text = details.arrayOfOptions[i].textTop;
                                arrayOfInteractions[i].textUpper.text = details.arrayOfOptions[i].textUpper;
                                arrayOfInteractions[i].textLower.text = details.arrayOfOptions[i].textLower;
                                arrayOfInteractions[i].optionData = details.arrayOfOptions[i].optionID;
                                arrayOfInteractions[i].optionName = details.arrayOfOptions[i].optionName;
                                //tooltip data
                                if (arrayOfTooltipsSprites[i] != null)
                                {
                                    if (details.arrayOfTooltipsSprite[i] != null)
                                    {
                                        arrayOfTooltipsSprites[i].tooltipHeader = details.arrayOfTooltipsSprite[i].textHeader;
                                        arrayOfTooltipsSprites[i].tooltipMain = details.arrayOfTooltipsSprite[i].textMain;
                                        arrayOfTooltipsSprites[i].tooltipDetails = details.arrayOfTooltipsSprite[i].textDetails;
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
                        }
                    }
                    else
                    {
                        //error -> Null array
                        Debug.LogError(string.Format("Invalid arrayOfInventoryOptions[\"{0}\"] (Null)", i));
                    }
                }
            }
            else
            {
                Debug.LogError("Invalid InventoryInputData (Null)");
            }
        }
        else
        {
            Debug.LogError("Invalid handler (null)");
        }
    }


    /// <summary>
    /// Method to run when a gear option is right clicked (info display)
    /// </summary>
    /// <param name="optionID"></param>
    public void GearRightClicked(string optionName)
    {
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
        {
            Debug.LogError(string.Format("Invalid Gear (Null) for gear / optionData {0}", optionName));
        }
    }

    /// <summary>
    /// Method to run when a gear option is left clicked (action menu)
    /// </summary>
    /// <param name="optionData"></param>
    public void GearLeftClicked(int optionData)
    {

    }

    //place new methods above here
}
