using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using modalAPI;
using gameAPI;
using System.Text;
using System;

/// <summary>
/// handles Modal Generic UI (picker with 2 to 3 choices that can be used for a wide range of purposes)
/// </summary>
public class ModalGenericPicker : MonoBehaviour
{

    public GameObject modalGenericObject;
    public GameObject modalPanelObject;
    public Image modalPanel;

    public TextMeshProUGUI topText;
    public TextMeshProUGUI middleText;
    public TextMeshProUGUI bottomText;

    public Button buttonCancel;
    public Button buttonBack;
    public Button buttonConfirm;

    public GameObject[] arrayOfGenericOptions;                //place generic option UI elements here (3 options)

    //private CanvasGroup canvasGroup;
    private ButtonInteraction buttonInteraction;
    private ButtonInteraction buttonBackInteraction;        //specifically for the Back button as it can be dynamically updated
    private static ModalGenericPicker modalGenericPicker;

    private int optionIDSelected;                             //slot ID (eg arrayOfGenericOptions [index] of selected option
    private string optionTextSelected;                        //used for nested Generic Picker windows, ignore otherwise
    private int nodeIDSelected;
    private int actorSlotIDSelected; 
    private EventType defaultReturnEvent;                          //event to trigger once confirmation button is clicked
    private EventType backReturnEvent;                  //event triggered when back button clicked (dynamic -> SetBackButton)
    private ModalActionDetails nestedDetails;           //used only if there are multiple, nested, option windows (dynamic -> InitialiseNestedOptions)

    private string colourEffect;
    private string colourDefault;
    private string colourNormal;
    private string colourEnd;


    /// <summary>
    /// Static instance so the Modal Generic Picker can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalGenericPicker Instance()
    {
        if (!modalGenericPicker)
        {
            modalGenericPicker = FindObjectOfType(typeof(ModalGenericPicker)) as ModalGenericPicker;
            if (!modalGenericPicker)
            { Debug.LogError("There needs to be one active ModalGenericPicker script on a GameObject in your scene"); }
        }
        return modalGenericPicker;
    }

    private void Awake()
    {
        //confirm button event
        buttonInteraction = buttonConfirm.GetComponent<ButtonInteraction>();
        if (buttonInteraction != null)
        { buttonInteraction.SetEvent(EventType.ConfirmGenericChoice); }
        else { Debug.LogError("Invalid buttonInteraction Confirm (Null)"); }
        //Back button event (default -> can be set dynamically using 'SetBackButton' method
        buttonBackInteraction = buttonBack.GetComponent<ButtonInteraction>();
        if (buttonBackInteraction != null)
        { buttonBackInteraction.SetEvent(EventType.BackButtonGeneric); }
        else { Debug.LogError("Invalid buttonBackInteraction (Null)"); }
        backReturnEvent = EventType.None;
        //cancel button event
        buttonInteraction = buttonCancel.GetComponent<ButtonInteraction>();
        if (buttonInteraction != null)
        { buttonInteraction.SetEvent(EventType.CloseGenericPicker); }
        else { Debug.LogError("Invalid buttonInteraction Cancel (Null)"); }
    }

    private void Start()
    {
        //register listener
        EventManager.instance.AddListener(EventType.OpenGenericPicker, OnEvent, "ModalGenericPicker");
        EventManager.instance.AddListener(EventType.CloseGenericPicker, OnEvent, "ModalGenericPicker");
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "ModalGenericPicker");
        EventManager.instance.AddListener(EventType.ConfirmGenericActivate, OnEvent, "ModalGenericPicker");
        EventManager.instance.AddListener(EventType.ConfirmGenericChoice, OnEvent, "ModalGenericPicker");
        EventManager.instance.AddListener(EventType.ConfirmGenericDeactivate, OnEvent, "ModalGenericPicker");
        EventManager.instance.AddListener(EventType.BackButtonGeneric, OnEvent, "ModalGenericPicker");
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
            case EventType.OpenGenericPicker:
                GenericPickerDetails details = Param as GenericPickerDetails;
                SetGenericPicker(details);
                break;
            case EventType.CloseGenericPicker:
                CloseGenericPicker();
                break;
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.ConfirmGenericActivate:
                GenericReturnData data = Param as GenericReturnData;
                SetConfirmButton(true, data);
                break;
            case EventType.ConfirmGenericDeactivate:
                SetConfirmButton(false);
                break;
            case EventType.ConfirmGenericChoice:
                ProcessGenericChoice();
                break;
            case EventType.BackButtonGeneric:
                ProcessBackButton();
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
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// If using multiple nested option windows you need to call this prior to using a back button to ensure it has the details needed to pass through
    /// to the previous window
    /// </summary>
    /// <param name="details"></param>
    public void InitialiseNestedOptions(ModalActionDetails details)
    {
        if (details != null)
        { nestedDetails = details; }
        else { Debug.LogError("Invalid ModalActionDetails (null)"); }
    }

    /// <summary>
    /// Sets the 'Back' button for cases where you want to step back in a nested window situation. Set to EventType.None otherwise (prevents Back button displaying)
    /// </summary>
    /// <param name="eventType"></param>
    public void SetBackButton(EventType eventType)
    { backReturnEvent = eventType; }

    /// <summary>
    /// Sets up Generic picker window
    /// </summary>
    private void SetGenericPicker(GenericPickerDetails details)
    {
        bool errorFlag = false;
        CanvasGroup genericCanvasGroup;
        GenericInteraction genericData;
        //set modal status
        GameManager.instance.guiScript.SetIsBlocked(true);
        //activate main panel
        modalPanelObject.SetActive(true);
        //activate dialogue window
        modalGenericObject.SetActive(true);
        //confirm button should be switched off at the start
        buttonConfirm.gameObject.SetActive(false);
        //back button only switched on if it has a valid underlying eventType
        if (backReturnEvent == EventType.None)
        { buttonBack.gameObject.SetActive(false); }
        else { buttonBack.gameObject.SetActive(true); }

        //canvasGroup.alpha = 100;

        //populate dialogue
        if (details != null)
        {
            //set up modal panel & buttons to be side appropriate
            switch (details.side.name)
            {
                case "Authority":
                    modalPanel.sprite = GameManager.instance.sideScript.picker_background_Authority;
                    //set button sprites
                    buttonCancel.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Authority;
                    buttonConfirm.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Authority;
                    buttonBack.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Authority;
                    //set sprite transitions
                    SpriteState spriteStateAuthority = new SpriteState();
                    spriteStateAuthority.highlightedSprite = GameManager.instance.sideScript.button_highlight_Authority;
                    spriteStateAuthority.pressedSprite = GameManager.instance.sideScript.button_Click;
                    buttonCancel.spriteState = spriteStateAuthority;
                    buttonConfirm.spriteState = spriteStateAuthority;
                    buttonBack.spriteState = spriteStateAuthority;
                    break;
                case "Resistance":
                    modalPanel.sprite = GameManager.instance.sideScript.picker_background_Rebel;
                    //set button sprites
                    buttonCancel.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Resistance;
                    buttonConfirm.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Resistance;
                    buttonBack.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Resistance;
                    //set sprite transitions
                    SpriteState spriteStateRebel = new SpriteState();
                    spriteStateRebel.highlightedSprite = GameManager.instance.sideScript.button_highlight_Resistance;
                    spriteStateRebel.pressedSprite = GameManager.instance.sideScript.button_Click;
                    buttonCancel.spriteState = spriteStateRebel;
                    buttonConfirm.spriteState = spriteStateRebel;
                    buttonBack.spriteState = spriteStateRebel;
                    break;
                default:
                    Debug.LogError(string.Format("Invalid side \"{0}\"", details.side.name));
                    break;
            }
            if (details.arrayOfOptions.Length > 0)
            {
                nodeIDSelected = details.nodeID;
                actorSlotIDSelected = details.actorSlotID;
                //assign sprites, texts, optionID's and tooltips
                for (int i = 0; i < details.arrayOfOptions.Length; i++)
                {
                    if (arrayOfGenericOptions[i] != null)
                    {
                        genericData = arrayOfGenericOptions[i].GetComponent<GenericInteraction>();
                        if (genericData != null)
                        {
                            //there are 3 options but not all of them may be used
                            if (details.arrayOfOptions[i] != null)
                            {
                                //get option canvas
                                genericCanvasGroup = arrayOfGenericOptions[i].GetComponent<CanvasGroup>();
                                if (genericCanvasGroup != null)
                                {
                                    //activate option
                                    arrayOfGenericOptions[i].SetActive(true);
                                    //populate data
                                    genericData.optionImage.sprite = details.arrayOfOptions[i].sprite;
                                    genericData.displayText.text = details.arrayOfOptions[i].text;
                                    genericData.data.optionID = details.arrayOfOptions[i].optionID;
                                    genericData.data.optionText = details.arrayOfOptions[i].optionText;
                                    genericData.data.actorSlotID = details.actorSlotID;
                                    //option Active or Not?
                                    if (details.arrayOfOptions[i].isOptionActive == true)
                                    {
                                        //activate option 
                                        genericCanvasGroup.alpha = 1.0f;
                                        genericCanvasGroup.interactable = true;
                                        genericData.isActive = true;
                                    }
                                    else
                                    {
                                        //deactivate option
                                        genericCanvasGroup.alpha = 0.25f;
                                        genericCanvasGroup.interactable = false;
                                        genericData.isActive = false;
                                    }
                                    //tooltips
                                    GenericTooltipUI tooltipUI = arrayOfGenericOptions[i].GetComponent<GenericTooltipUI>();
                                    if (tooltipUI != null)
                                    {
                                        GenericTooltipDetails tooltipDetails = details.arrayOfTooltips[i];
                                        if (tooltipDetails != null)
                                        {
                                            tooltipUI.tooltipHeader = details.arrayOfTooltips[i].textHeader;
                                            tooltipUI.tooltipMain = details.arrayOfTooltips[i].textMain;
                                            tooltipUI.tooltipDetails = details.arrayOfTooltips[i].textDetails;
                                        }
                                        else { Debug.LogError(string.Format("Invalid tooltipDetails (Null) for arrayOfOptions[\"{0}\"]", i)); }
                                    }
                                    else
                                    { Debug.LogError(string.Format("Invalid tooltipUI (Null) for arrayOfOptions[\"{0}\"]", i)); }
                                }
                                else { Debug.LogError(string.Format("Invalid genericCanvasGroup for arrayOfGenericOptions[{0}]", i)); }

                            }
                            else
                            { arrayOfGenericOptions[i].SetActive(false); }
                        }
                        else
                        {
                            //error -> Null Interaction data
                            Debug.LogError(string.Format("Invalid arrayOfGenericOptions[\"{0}\"] genericData (Null)", i));
                            errorFlag = true;
                            break;
                        }
                    }
                    else
                    {
                        //error -> Null array
                        Debug.LogError(string.Format("Invalid arrayOfGenericOptions[\"{0}\"] (Null)", i));
                        errorFlag = true;
                        break;
                    }
                }
                //register return event for reference once user confirms a choice
                defaultReturnEvent = details.returnEvent;
            }
        }
        else
        {
            //error -> null parameter
            Debug.LogError("Invalid GenericPickerDetails (Null)");
            errorFlag = true;
        }
        //if a problem then generate an outcome window instead
        if (errorFlag == true)
        {
            modalGenericObject.SetActive(false);
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.textTop = "There has been a SNAFU";
            outcomeDetails.textBottom = "Heads, toes and other limbswill be removed";
            outcomeDetails.side = details.side;
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ModalGenericPicker.cs -> SetGenericPicker");
        }
        //all good, generate
        else
        {
            //texts
            topText.text = details.textTop;
            middleText.text = details.textMiddle;
            bottomText.text = details.textBottom;
            //set game state
            GameManager.instance.inputScript.SetModalState(ModalState.GenericPicker);
            Debug.Log("UI: Open -> ModalGenericPicker" + "\n");
        }
    }



    /// <summary>
    /// close Action Menu
    /// </summary>
    private void CloseGenericPicker()
    {
        modalGenericObject.SetActive(false);
        GameManager.instance.guiScript.SetIsBlocked(false);
        //close generic tooltip (safety check)
        GameManager.instance.tooltipGenericScript.CloseTooltip();
        //deselect all generic options to prevent picker opening next time with a preselected team
        EventManager.instance.PostNotification(EventType.DeselectOtherGenerics, this, null, "ModalGenericPicker.cs -> CloseGenericPicker");
        SetConfirmButton(false);
        //set game state
        GameManager.instance.inputScript.ResetStates();
        Debug.Log("UI: Close -> ModalGenericPicker" + "\n");
    }

    /// <summary>
    /// Back button selected -> current instance of Picker window closed and new event generated (opens new instance with previous window)
    /// </summary>
    private void ProcessBackButton()
    {
        //close current GenericPicker Window
        CloseGenericPicker();
        //trigger event that calls previous window
        EventManager.instance.PostNotification(backReturnEvent, this, nestedDetails, "ModalGenericPicker.cs -> ProcessBackButton");
    }


    /// <summary>
    /// Confirm button switched on/off. Only ON and visible if a generic option has been selected and a data package returned
    /// </summary>
    /// <param name="activate"></param>
    public void SetConfirmButton(bool isActive, GenericReturnData data = null)
    {
        string text = "Unknown"; ;
        //An option is selected
        if (isActive == true)
        {
            buttonConfirm.gameObject.SetActive(true);
            if (data != null)
            {
                //update currently selected option
                optionIDSelected = data.optionID;
                optionTextSelected = data.optionText;
                //change top text to show which option selected
                switch (defaultReturnEvent)
                {
                    case EventType.GenericTeamRecall:
                        if (data.optionID > -1)
                        {
                            Team teamRecall = GameManager.instance.dataScript.GetTeam(data.optionID);
                            if (teamRecall != null)
                            {
                                text = string.Format("{0}{1} Team {2}{3}selected{4}", colourEffect, teamRecall.arc.name, colourEnd, colourDefault, colourEnd);
                                Debug.Log(string.Format("TeamPicker: teamArcID {0} selected{1}", data.optionID, "\n"));
                            }
                            else { Debug.LogError(string.Format("Invalid team (Null) for teamID {0}", data.optionID)); }
                        }
                        else { Debug.LogError("Invalid data.optionID (< 0)"); }
                        break;
                    case EventType.GenericNeutraliseTeam:
                        if (data.optionID > -1)
                        {
                            Team teamNeutralise = GameManager.instance.dataScript.GetTeam(data.optionID);
                            if (teamNeutralise != null)
                            {
                                text = string.Format("{0}{1} Team {2}{3}selected{4}", colourEffect, teamNeutralise.arc.name, colourEnd, colourDefault, colourEnd);
                                Debug.Log(string.Format("TeamPicker: teamArcID {0} selected{1}", data.optionID, "\n"));
                            }
                            else { Debug.LogError(string.Format("Invalid team (Null) for teamID {0}", data.optionID)); }
                        }
                        else { Debug.LogError("Invalid data.optionID (< 0)"); }
                        break;
                    case EventType.GenericGearChoice:
                        if (data.optionID > -1)
                        {
                            Gear gear = GameManager.instance.dataScript.GetGear(data.optionID);
                            if (gear != null)
                            {
                                text = string.Format("{0}{1}{2} {3}selected{4}", colourEffect, gear.name.ToUpper(), colourEnd, colourDefault, colourEnd);
                                Debug.Log(string.Format("GearPicker: gearID {0} selected{1}", data.optionID, "\n"));
                            }
                            else { Debug.LogError(string.Format("Invalid gear (Null) for gearID {0}", data.optionID)); }
                        }
                        else { Debug.LogError("Invalid data.optionID (< 0)"); }
                        break;
                    case EventType.GenericRecruitActorResistance:
                    case EventType.GenericRecruitActorAuthority:
                        if (data.optionID > -1)
                        {
                            Actor actor = GameManager.instance.dataScript.GetActor(data.optionID);
                            if (actor != null)
                            {
                                text = string.Format("{0}{1}{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                Debug.Log(string.Format("RecruitPicker: actorID {0} selected{1}", data.optionID, "\n"));
                            }
                            else { Debug.LogError(string.Format("Invalid actor (Null) for actorID {0}", data.optionID)); }
                        }
                        else { Debug.LogError("Invalid data.optionID (< 0)"); }
                        break;
                    case EventType.GenericHandleActor:
                        if (data.actorSlotID > -1)
                        {
                            if (string.IsNullOrEmpty(data.optionText) == false)
                            {
                                Actor actor = GameManager.instance.dataScript.GetCurrentActor(data.actorSlotID, GameManager.instance.sideScript.PlayerSide);
                                if (actor != null)
                                {
                                    switch (data.optionText)
                                    {
                                        case "HandleReserve":
                                            text = string.Format("{0}Send {1} to Reserve Pool{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.Log(string.Format("ManagePicker: {0}, ID {1}, to RESERVE pool selected{2}", actor.actorName, data.actorSlotID, "\n"));
                                            break;
                                        case "HandleDismiss":
                                            text = string.Format("{0}Fire {1}{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.Log(string.Format("ManagePicker: {0}, ID {1}, FIRE selected{2}", actor.actorName, data.actorSlotID, "\n"));
                                            break;
                                        case "HandleDispose":
                                            text = string.Format("{0}Dispose of {1}{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.Log(string.Format("ManagePicker: {0}, ID {1}, DISPOSE selected{2}", actor.actorName, data.actorSlotID, "\n"));
                                            break;
                                        default:
                                            Debug.LogError(string.Format("Invalid data.optionText \"{0}\"", data.optionText));
                                            break;
                                    }

                                }
                                else { Debug.LogError(string.Format("Invalid actor (Null) for actorID {0}", data.actorSlotID)); }
                            }
                            else { Debug.LogError("Invalid data.optionText (Null or Empty)"); }
                        }
                        else { Debug.LogError("Invalid data.optionID (< 0)"); }
                        break;
                    case EventType.GenericReserveActor:
                        if (data.actorSlotID > -1)
                        {
                            if (string.IsNullOrEmpty(data.optionText) == false)
                            {
                                Actor actor = GameManager.instance.dataScript.GetCurrentActor(data.actorSlotID, GameManager.instance.sideScript.PlayerSide);
                                if (actor != null)
                                {
                                    switch (data.optionText)
                                    {
                                        case "ReserveRest":
                                            text = string.Format("{0}Send {1} to Rest{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.Log(string.Format("ManagePicker: {0}, ID {1}, REST selected{2}", actor.actorName, data.actorSlotID, "\n"));
                                            break;
                                        case "ReservePromise":
                                            text = string.Format("{0}You Promise {1}{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.Log(string.Format("ManagePicker: {0}, ID {1}, PROMISE selected{2}", actor.actorName, data.actorSlotID, "\n"));
                                            break;
                                        case "ReserveNoPromise":
                                            text = string.Format("{0}NO promises to {1}{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.Log(string.Format("ManagePicker: {0}, ID {1}, NO PROMISE selected{2}", actor.actorName, data.actorSlotID, "\n"));
                                            break;
                                        default:
                                            Debug.LogError(string.Format("Invalid data.optionText \"{0}\"", data.optionText));
                                            break;
                                    }

                                }
                                else { Debug.LogError(string.Format("Invalid actor (Null) for actorID {0}", data.actorSlotID)); }
                            }
                            else { Debug.LogError("Invalid data.optionText (Null or Empty)"); }
                        }
                        else { Debug.LogError("Invalid data.actorSlotID (< 0)"); }
                        break;
                    case EventType.GenericDismissActor:
                        if (data.actorSlotID > -1)
                        {
                            if (string.IsNullOrEmpty(data.optionText) == false)
                            {
                                Actor actor = GameManager.instance.dataScript.GetCurrentActor(data.actorSlotID, GameManager.instance.sideScript.PlayerSide);
                                if (actor != null)
                                {
                                    switch (data.optionText)
                                    {
                                        case "DismissPromote":
                                            text = string.Format("{0}Promote {1}{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.Log(string.Format("ManagePicker: {0}, ID {1}, PROMOTE selected{2}", actor.actorName, data.actorSlotID, "\n"));
                                            break;
                                        case "DismissIncompetent":
                                            text = string.Format("{0}Dismiss {1} for Incompetence{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.Log(string.Format("ManagePicker: {0}, ID {1}, INCOMPETENT selected{2}", actor.actorName, data.actorSlotID, "\n"));
                                            break;
                                        case "DismissUnsuited":
                                            text = string.Format("{0}Dismiss {1} for Unsuitability{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.Log(string.Format("ManagePicker: {0}, ID {1}, UNSUITED selected{2}", actor.actorName, data.actorSlotID, "\n"));
                                            break;
                                        default:
                                            Debug.LogError(string.Format("Invalid data.optionText \"{0}\"", data.optionText));
                                            break;
                                    }

                                }
                                else { Debug.LogError(string.Format("Invalid actor (Null) for actorID {0}", data.actorSlotID)); }
                            }
                            else { Debug.LogError("Invalid data.optionText (Null or Empty)"); }
                        }
                        else { Debug.LogError("Invalid data.actorSlotID (< 0)"); }
                        break;
                    case EventType.GenericDisposeActor:
                        if (data.actorSlotID > -1)
                        {
                            if (string.IsNullOrEmpty(data.optionText) == false)
                            {
                                Actor actor = GameManager.instance.dataScript.GetCurrentActor(data.actorSlotID, GameManager.instance.sideScript.PlayerSide);
                                if (actor != null)
                                {
                                    switch (data.optionText)
                                    {
                                        case "DisposeLoyalty":
                                            text = string.Format("{0}Dispose of {1} because they are Disloyal{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.Log(string.Format("ManagePicker: {0}, ID {1}, DISPOSE due to LOYALTY selected{2}", actor.actorName, data.actorSlotID, "\n"));
                                            break;
                                        case "DisposeCorrupt":
                                            text = string.Format("{0}Dispose of {1} because they are Corrupt{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.Log(string.Format("ManagePicker: {0}, ID {1}, DISPOSE due to CORRUPTION{2}", actor.actorName, data.actorSlotID, "\n"));
                                            break;
                                        case "DisposeHabit":
                                            text = string.Format("{0}Dispose of {1} because you can{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.Log(string.Format("ManagePicker: {0}, ID {1}, DISPOSE due to HABIT{2}", actor.actorName, data.actorSlotID, "\n"));
                                            break;
                                        default:
                                            Debug.LogError(string.Format("Invalid data.optionText \"{0}\"", data.optionText));
                                            break;
                                    }

                                }
                                else { Debug.LogError(string.Format("Invalid actor (Null) for actorID {0}", data.actorSlotID)); }
                            }
                            else { Debug.LogError("Invalid data.optionText (Null or Empty)"); }
                        }
                        else { Debug.LogError("Invalid data.actorSlotID (< 0)"); }
                        break;
                }
            }
            else { Debug.LogError("Invalid GenericReturnData (Null)"); }
        }
        else
        {
            //Nothing currently selected, show generic message
            buttonConfirm.gameObject.SetActive(false);
            switch (defaultReturnEvent)
            {
                case EventType.GenericTeamRecall:
                    text = string.Format("{0}Recall{1} {2}team{3}", colourEffect, colourEnd, colourNormal, colourEnd);
                    break;
                case EventType.GenericNeutraliseTeam:
                    text = string.Format("{0}Neutralise{1} {2}team{3}", colourEffect, colourEnd, colourNormal, colourEnd);
                    break;
                case EventType.GenericGearChoice:
                    text = string.Format("{0}Gear{1} {2}selection{3}", colourEffect, colourEnd, colourNormal, colourEnd);
                    break;
                case EventType.GenericRecruitActorResistance:
                case EventType.GenericRecruitActorAuthority:
                    text = string.Format("{0}Recruit{1} {2}selection{3}", colourEffect, colourEnd, colourNormal, colourEnd);
                    break;
                case EventType.GenericHandleActor:
                    text = string.Format("{0}Managerial{1} {2}selection{3}", colourEffect, colourEnd, colourNormal, colourEnd);
                    break;
                case EventType.GenericReserveActor:
                    text = string.Format("{0}Reserve Pool{1} {2}selection{3}", colourEffect, colourEnd, colourNormal, colourEnd);
                    break;
                case EventType.GenericDismissActor:
                    text = string.Format("{0}Dismiss{1} {2}selection{3}", colourEffect, colourEnd, colourNormal, colourEnd);
                    break;
                case EventType.GenericDisposeActor:
                    text = string.Format("{0}Dispose off{1} {2}selection{3}", colourEffect, colourEnd, colourNormal, colourEnd);
                    break;
                default:
                    text = string.Format("{0}Select {1}{2}ANY{3}{4} Option{5}", colourDefault, colourEnd, colourEffect, colourEnd, colourDefault, colourEnd);
                    break;
            }
        }
        //update top text
        topText.text = text;
    }

    /// <summary>
    /// click confirm, process generic option and return data to calling class
    /// </summary>
    private void ProcessGenericChoice()
    {
        GenericReturnData returnData = new GenericReturnData();
        returnData.optionID = optionIDSelected;
        returnData.optionText = optionTextSelected;
        returnData.nodeID = nodeIDSelected;
        returnData.actorSlotID = actorSlotIDSelected;
        //close picker window regardless
        EventManager.instance.PostNotification(EventType.CloseGenericPicker, this, null, "ModalGenericPicker.cs -> ProcessGenericChoice");
        //trigger the appropriate return Event and pass selected optionID back to the originating class
        switch (defaultReturnEvent)
        {
            case EventType.GenericTeamRecall:
            case EventType.GenericNeutraliseTeam:
            case EventType.GenericGearChoice:
            case EventType.GenericRecruitActorResistance:
            case EventType.GenericRecruitActorAuthority:
            case EventType.GenericHandleActor:
            case EventType.GenericReserveActor:
            case EventType.GenericDismissActor:
            case EventType.GenericDisposeActor:
                EventManager.instance.PostNotification(defaultReturnEvent, this, returnData, "ModalGenericPicker.cs -> ProcessGenericChoice");
                break;
            default:
                Debug.LogError(string.Format("Invalid returnEvent \"{0}\"", defaultReturnEvent));
                break;
        }
    }





    //place methods above here
}
