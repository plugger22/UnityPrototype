﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using modalAPI;
using gameAPI;
using System.Text;
using System;
using packageAPI;

/// <summary>
/// handles Modal Generic UI (picker with 2 to 3 choices that can be used for a wide range of purposes)
/// </summary>
public class ModalGenericPicker : MonoBehaviour
{

    [Tooltip("Max number of options allowed in Modal Generic Picker")]
    [Range(3, 3)] public int maxOptions = 3;

    public Canvas modalPickerCanvas;
    public GameObject modalGenericObject;
    public GameObject modalPanelObject;
    public Image modalPanel;
    public Image modalHeader;

    public TextMeshProUGUI topText;
    public TextMeshProUGUI middleText;
    public TextMeshProUGUI bottomText;
    public TextMeshProUGUI headerText;

    public Button buttonCancel;
    public Button buttonBack;
    public Button buttonConfirm;
    public Button buttonHelp;

    public GameObject[] arrayOfGenericOptions;                      //place generic option UI elements here (3 options)

    //private CanvasGroup canvasGroup;
    private ButtonInteraction buttonConfirmInteraction;
    private ButtonInteraction buttonCancelInteraction;
    private ButtonInteraction buttonBackInteraction;                //specifically for the Back button as it can be dynamically updated
    private GenericHelpTooltipUI help;
    private static ModalGenericPicker modalGenericPicker;

    private int optionIDSelected;                                   //slot ID (eg arrayOfGenericOptions [index] of selected option [EDIT: Not sure about this, I think it is teamID, targetID, actorID etc)
    private string optionTextSelected;                              //used for nested Generic Picker windows, ignore otherwise
    private string optionNameSelected;                              //used instead of optionID for objects with a name key, eg. gear
    private int nodeIDSelected;                                     //used for MANAGE
    private int actorSlotIDSelected;                                //used for MANAGE
    private int datapoint;                                          //generic data point passed to picker by initialising method
    private EventType defaultReturnEvent;                           //event to trigger once confirmation button is clicked
    private EventType backReturnEvent;                              //event triggered when back button clicked (dynamic -> SetBackButton)
    private ModalActionDetails nestedDetails;                       //used only if there are multiple, nested, option windows (dynamic -> InitialiseNestedOptions)

    private string colourGood;
    private string colourEffect;
    private string colourDefault;
    private string colourNormal;
    private string colourNeutral;
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
        //asserts
        Debug.Assert(modalPickerCanvas != null, "Invalid modalPickerCanvas (Null)");
        Debug.Assert(modalGenericObject != null, "Invalid modalGenericObject (Null)");
        Debug.Assert(modalPanelObject != null, "Invalid modalPanelObject (Null)");
        Debug.Assert(modalPanel != null, "Invalid modalPanel (Null)");
        Debug.Assert(modalHeader != null, "Invalid modalHeader (Null)");
        Debug.Assert(topText != null, "Invalid topText (Null)");
        Debug.Assert(middleText != null, "Invalid middleText (Null)");
        Debug.Assert(bottomText != null, "Invalid bottomText (Null)");
        Debug.Assert(headerText != null, "Invalid headerText (Null)");
        Debug.Assert(buttonCancel != null, "Invalid buttonCancel (Null)");
        Debug.Assert(buttonBack != null, "Invalid buttonBack (Null)");
        Debug.Assert(buttonConfirm != null, "Invalid buttonConfirm (Null)");
        Debug.Assert(buttonHelp != null, "Invalid GenericHelpTooltipUI (Null)");
        //confirm button event
        buttonConfirmInteraction = buttonConfirm.GetComponent<ButtonInteraction>();
        if (buttonConfirmInteraction != null)
        { buttonConfirmInteraction.SetButton(EventType.ConfirmGenericChoice); }
        else { Debug.LogError("Invalid buttonInteraction Confirm (Null)"); }
        //cancel button event
        buttonCancelInteraction = buttonCancel.GetComponent<ButtonInteraction>();
        if (buttonCancelInteraction != null)
        { buttonCancelInteraction.SetButton(EventType.CancelButtonGeneric); }
        else { Debug.LogError("Invalid buttonInteraction Confirm (Null)"); }
        //help button
        help = buttonHelp.GetComponent<GenericHelpTooltipUI>();
        if (help == null) { Debug.LogError("Invalid help script (Null)"); }
        //Back button event (default -> can be set dynamically using 'SetBackButton' method
        buttonBackInteraction = buttonBack.GetComponent<ButtonInteraction>();
        if (buttonBackInteraction != null)
        { buttonBackInteraction.SetButton(EventType.BackButtonGeneric); }
        else { Debug.LogError("Invalid buttonBackInteraction (Null)"); }
        backReturnEvent = EventType.None;
    }

    private void Start()
    {
        Debug.Assert(maxOptions == 3, "Invalid maxOptions (must be 3)");
        //register listener
        EventManager.i.AddListener(EventType.OpenGenericPicker, OnEvent, "ModalGenericPicker");
        EventManager.i.AddListener(EventType.CloseGenericPicker, OnEvent, "ModalGenericPicker");
        EventManager.i.AddListener(EventType.CancelButtonGeneric, OnEvent, "ModalGenericPicker");
        EventManager.i.AddListener(EventType.ChangeColour, OnEvent, "ModalGenericPicker");
        EventManager.i.AddListener(EventType.ChangeSide, OnEvent, "ModalGenericPicker");
        EventManager.i.AddListener(EventType.ConfirmGenericActivate, OnEvent, "ModalGenericPicker");
        EventManager.i.AddListener(EventType.ConfirmGenericChoice, OnEvent, "ModalGenericPicker");
        EventManager.i.AddListener(EventType.ConfirmGenericDeactivate, OnEvent, "ModalGenericPicker");
        EventManager.i.AddListener(EventType.BackButtonGeneric, OnEvent, "ModalGenericPicker");
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
            case EventType.CancelButtonGeneric:
                ProcessCancelButton();
                break;
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.ChangeSide:
                SetSide();
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
        colourGood = GameManager.i.colourScript.GetColour(ColourType.goodText);
        colourEffect = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourDefault = GameManager.i.colourScript.GetColour(ColourType.whiteText);
        colourNormal = GameManager.i.colourScript.GetColour(ColourType.normalText);
        colourNeutral = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourEnd = GameManager.i.colourScript.GetEndTag();
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
        //close Node tooltip safety check
        GameManager.i.tooltipNodeScript.CloseTooltip("ModalGenericPicker.cs -> SetGenericPicker");
        //open Generic picker
        bool errorFlag = false;
        CanvasGroup genericCanvasGroup;
        GenericInteraction genericData;
        //set modal status
        GameManager.i.guiScript.SetIsBlocked(true);
        //activate main panel
        modalPanelObject.SetActive(true);
        //header activated only if text provided
        if (string.IsNullOrEmpty(details.textHeader) == false)
        {
            modalHeader.gameObject.SetActive(true);
            headerText.text = details.textHeader;
        }
        else
        { modalHeader.gameObject.SetActive(false); }
        //activate dialogue window
        modalPickerCanvas.gameObject.SetActive(true);
        modalGenericObject.SetActive(true);
        //confirm button should be switched off at the start
        buttonConfirm.gameObject.SetActive(false);
        //back button only switched on if it has a valid underlying eventType
        if (backReturnEvent == EventType.None)
        { buttonBack.gameObject.SetActive(false); }
        else { buttonBack.gameObject.SetActive(true); }
        //halt execution, until picker is processed, if indicated
        if (details.isHaltExecution == true)
        { GameManager.i.turnScript.haltExecution = true; }
        //canvasGroup.alpha = 100;

        //populate dialogue
        if (details != null)
        {
            if (details.arrayOfOptions.Length > 0)
            {
                //initialise data
                nodeIDSelected = details.nodeID;
                actorSlotIDSelected = details.actorSlotID;
                datapoint = details.data;
                optionIDSelected = -1;
                optionNameSelected = "";
                //set help
                List<HelpData> listOfHelpData = GameManager.i.helpScript.GetHelpData(details.help0, details.help1, details.help2, details.help3);
                if (listOfHelpData != null && listOfHelpData.Count > 0)
                {
                    buttonHelp.gameObject.SetActive(true);
                    help.SetHelpTooltip(listOfHelpData, 150, 200);
                }
                else { buttonHelp.gameObject.SetActive(false); }
                //assign sprites, texts, optionID's and tooltips
                for (int i = 0; i < details.arrayOfOptions.Length; i++)
                {
                    if (arrayOfGenericOptions[i] != null)
                    {
                        genericData = arrayOfGenericOptions[i].GetComponent<GenericInteraction>();
                        if (genericData != null)
                        {
                            //there are 'maxOptions' options but not all of them may be used
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
                                    genericData.data.optionName = details.arrayOfOptions[i].optionName;
                                    genericData.data.optionNested = details.arrayOfOptions[i].optionText;
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
                                            tooltipUI.x_offset = 55;
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
            modalPickerCanvas.gameObject.SetActive(false);

            /*modalGenericObject.SetActive(false);*/

            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.textTop = "There has been a SNAFU";
            outcomeDetails.textBottom = "Heads, toes and other limbswill be removed";
            outcomeDetails.side = details.side;
            EventManager.i.PostNotification(EventType.OutcomeOpen, this, outcomeDetails, "ModalGenericPicker.cs -> SetGenericPicker");
        }
        //all good, generate
        else
        {
            //texts
            topText.text = details.textTop;
            middleText.text = details.textMiddle;
            bottomText.text = details.textBottom;
            //set game state
            ModalStateData package = new ModalStateData();
            package.mainState = ModalSubState.GenericPicker;
            package.pickerState = details.subState;
            GameManager.i.inputScript.SetModalState(package);
            Debug.LogFormat("[UI] ModalGenericPicker.cs -> SetGenericPicker{0}", "\n");
        }
    }



    /// <summary>
    /// close Gemeric picker
    /// </summary>
    private void CloseGenericPicker()
    {
        /*modalGenericObject.SetActive(false);*/

        modalPickerCanvas.gameObject.SetActive(false);
        GameManager.i.guiScript.SetIsBlocked(false);
        //close generic tooltip (safety check)
        GameManager.i.tooltipGenericScript.CloseTooltip("ModalGenericPicker.cs -> CloseGenericPicker");
        //close help tooltip
        GameManager.i.tooltipHelpScript.CloseTooltip("ModalGenericPickerUI.cs -> CloseGenericPicker");
        //deselect all generic options to prevent picker opening next time with a preselected team
        EventManager.i.PostNotification(EventType.DeselectOtherGenerics, this, null, "ModalGenericPicker.cs -> CloseGenericPicker");
        //reset GUI elements to default
        SetConfirmButton(false);

        /*SetBackButton(EventType.None); //Edit: 4Mar19 -> if these are live then the back button won't work
        nestedDetails = null;*/

        //set game state
        GameManager.i.inputScript.ResetStates();
        Debug.LogFormat("[UI] ModalGenericPicker.cs -> CloseGenericPicker", "\n");
    }

    /// <summary>
    /// Back button selected -> current instance of Picker window closed and new event generated (opens new instance with previous window)
    /// </summary>
    private void ProcessBackButton()
    {
        //close current GenericPicker Window
        CloseGenericPicker();
        //trigger event that calls previous window
        EventManager.i.PostNotification(backReturnEvent, this, nestedDetails, "ModalGenericPicker.cs -> ProcessBackButton");
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
                optionNameSelected = data.optionName;
                optionTextSelected = data.optionNested;
                //change top text to show which option selected
                switch (defaultReturnEvent)
                {
                    case EventType.GenericTeamRecall:
                        if (data.optionID > -1)
                        {
                            Team teamRecall = GameManager.i.dataScript.GetTeam(data.optionID);
                            if (teamRecall != null)
                            {
                                text = string.Format("{0}{1} Team {2}{3}selected{4}", colourEffect, teamRecall.arc.name, colourEnd, colourDefault, colourEnd);
                                Debug.LogFormat("[UI] -> ModalGenericPicker: teamArcID {0} selected{1}", data.optionID, "\n");
                            }
                            else { Debug.LogError(string.Format("Invalid team (Null) for teamID {0}", data.optionID)); }
                        }
                        else { Debug.LogError("Invalid data.optionID (< 0)"); }
                        break;
                    case EventType.GenericNeutraliseTeam:
                        if (data.optionID > -1)
                        {
                            Team teamNeutralise = GameManager.i.dataScript.GetTeam(data.optionID);
                            if (teamNeutralise != null)
                            {
                                text = string.Format("{0}{1} Team {2}{3}selected{4}", colourEffect, teamNeutralise.arc.name, colourEnd, colourDefault, colourEnd);
                                Debug.LogFormat("[UI] -> ModalGenericPicker: teamArcID {0} selected{1}", data.optionID, "\n");
                            }
                            else { Debug.LogError(string.Format("Invalid team (Null) for teamID {0}", data.optionID)); }
                        }
                        else { Debug.LogError("Invalid data.optionID (< 0)"); }
                        break;
                    case EventType.GenericGearChoice:
                        if (string.IsNullOrEmpty(data.optionName) == false)
                        {
                            Gear gear = GameManager.i.dataScript.GetGear(data.optionName);
                            if (gear != null)
                            {
                                text = string.Format("{0}{1}{2} {3}selected{4}", colourEffect, gear.tag.ToUpper(), colourEnd, colourDefault, colourEnd);
                                Debug.LogFormat("[UI] -> ModalGenericPicker: gear {0} selected{1}", data.optionName, "\n");
                            }
                            else { Debug.LogError(string.Format("Invalid gear (Null) for gear {0}", data.optionName)); }
                        }
                        else { Debug.LogError("Invalid data.optionName (Null or Empty)"); }
                        break;
                    case EventType.GenericTargetInfo:
                        if (string.IsNullOrEmpty(data.optionName) == false)
                        {
                            Target target = GameManager.i.dataScript.GetTarget(data.optionName);
                            if (target != null)
                            {
                                text = string.Format("{0}{1}{2} {3}selected{4}", colourEffect, target.targetName.ToUpper(), colourEnd, colourDefault, colourEnd);
                                Debug.LogFormat("[UI] -> ModalGenericPicker: target {0} selected{1}", data.optionName, "\n");
                            }
                            else { Debug.LogError(string.Format("Invalid target (Null) for target {0}", data.optionName)); }
                        }
                        else { Debug.LogError("Invalid data.optionID (< 0)"); }
                        break;
                    case EventType.GenericCompromisedGear:
                        if (string.IsNullOrEmpty(data.optionName) == false)
                        {
                            Gear gear = GameManager.i.dataScript.GetGear(data.optionName);
                            if (gear != null)
                            {
                                text = string.Format("Save {0}{1}{2} for {3}{4} Renown{5} (have {6}{7}{8})", colourEffect, gear.tag.ToUpper(), colourEnd, 
                                    colourNeutral, datapoint, colourEnd, colourGood, GameManager.i.playerScript.Power, colourEnd);
                                Debug.LogFormat("[UI] -> ModalGenericPicker: gear {0} selected{1}", data.optionName, "\n");
                            }
                            else { Debug.LogErrorFormat("Invalid gear (Null) for gear {0}", data.optionName); }
                        }
                        else { Debug.LogError("Invalid data.optionName (Null or Empty)"); }
                        break;
                    case EventType.GenericRecruitActorResistance:
                    case EventType.GenericRecruitActorAuthority:
                        if (data.optionID > -1)
                        {
                            Actor actor = GameManager.i.dataScript.GetActor(data.optionID);
                            if (actor != null)
                            {
                                text = string.Format("{0}{1}{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                Debug.LogFormat("[UI] -> ModalGenericPicker: actorID {0} selected{1}", data.optionID, "\n");
                            }
                            else { Debug.LogError(string.Format("Invalid actor (Null) for actorID {0}", data.optionID)); }
                        }
                        else { Debug.LogError("Invalid data.optionID (< 0)"); }
                        break;
                    case EventType.GenericHandleActor:
                        if (data.actorSlotID > -1)
                        {
                            if (string.IsNullOrEmpty(data.optionNested) == false)
                            {
                                Actor actor = GameManager.i.dataScript.GetCurrentActor(data.actorSlotID, GameManager.i.sideScript.PlayerSide);
                                if (actor != null)
                                {
                                    switch (data.optionNested)
                                    {
                                        case "HandleReserve":
                                            text = string.Format("{0}Send {1} to Reserve Pool{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.LogFormat("[UI] -> ModalGenericPicker: {0}, ID {1}, to RESERVE pool selected{2}", actor.actorName, data.actorSlotID, "\n");
                                            break;
                                        case "HandleDismiss":
                                            text = string.Format("{0}Move On {1}{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.LogFormat("[UI] -> ModalGenericPicker: {0}, ID {1}, DISMISS selected{2}", actor.actorName, data.actorSlotID, "\n");
                                            break;
                                        case "HandleDispose":
                                            text = string.Format("{0}Dispose of {1}{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.LogFormat("[UI] -> ModalGenericPicker: {0}, ID {1}, DISPOSE selected{2}", actor.actorName, data.actorSlotID, "\n");
                                            break;
                                        default:
                                            Debug.LogErrorFormat("Invalid data.optionText \"{0}\"", data.optionNested);
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
                            if (string.IsNullOrEmpty(data.optionNested) == false)
                            {
                                Actor actor = GameManager.i.dataScript.GetCurrentActor(data.actorSlotID, GameManager.i.sideScript.PlayerSide);
                                if (actor != null)
                                {
                                    switch (data.optionNested)
                                    {
                                        case "ReserveRest":
                                            text = string.Format("{0}Send {1} to Rest{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.LogFormat("[UI] -> ModalGenericPicker: {0}, ID {1}, REST selected{2}", actor.actorName, data.actorSlotID, "\n");
                                            break;
                                        case "ReservePromise":
                                            text = string.Format("{0}You Promise {1}{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.LogFormat("[UI] -> ModalGenericPicker: {0}, ID {1}, PROMISE selected{2}", actor.actorName, data.actorSlotID, "\n");
                                            break;
                                        case "ReserveNoPromise":
                                            text = string.Format("{0}NO promises to {1}{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.LogFormat("[UI] -> ModalGenericPicker: {0}, ID {1}, NO PROMISE selected{2}", actor.actorName, data.actorSlotID, "\n");
                                            break;
                                        default:
                                            Debug.LogError(string.Format("Invalid data.optionText \"{0}\"", data.optionNested));
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
                            if (string.IsNullOrEmpty(data.optionNested) == false)
                            {
                                Actor actor = GameManager.i.dataScript.GetCurrentActor(data.actorSlotID, GameManager.i.sideScript.PlayerSide);
                                if (actor != null)
                                {
                                    switch (data.optionNested)
                                    {
                                        case "DismissPromote":
                                            text = string.Format("{0}Promote {1}{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.LogFormat("[UI] -> ModalGenericPicker: {0}, ID {1}, PROMOTE selected{2}", actor.actorName, data.actorSlotID, "\n");
                                            break;
                                        case "DismissIncompetent":
                                            text = string.Format("{0}Dismiss {1} for Incompetence{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.LogFormat("[UI] -> ModalGenericPicker: {0}, ID {1}, INCOMPETENT selected{2}", actor.actorName, data.actorSlotID, "\n");
                                            break;
                                        case "DismissUnsuited":
                                            text = string.Format("{0}Dismiss {1} for Unsuitability{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.LogFormat("[UI] -> ModalGenericPicker: {0}, ID {1}, UNSUITED selected{2}", actor.actorName, data.actorSlotID, "\n");
                                            break;
                                        default:
                                            Debug.LogErrorFormat("Invalid data.optionText \"{0}\"", data.optionNested);
                                            break;
                                    }

                                }
                                else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", data.actorSlotID); }
                            }
                            else { Debug.LogError("Invalid data.optionText (Null or Empty)"); }
                        }
                        else { Debug.LogError("Invalid data.actorSlotID (< 0)"); }
                        break;
                    case EventType.GenericDisposeActor:
                        if (data.actorSlotID > -1)
                        {
                            if (string.IsNullOrEmpty(data.optionNested) == false)
                            {
                                Actor actor = GameManager.i.dataScript.GetCurrentActor(data.actorSlotID, GameManager.i.sideScript.PlayerSide);
                                if (actor != null)
                                {
                                    switch (data.optionNested)
                                    {
                                        case "DisposeLoyalty":
                                            text = string.Format("{0}Dispose of {1} because they are Disloyal{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.LogFormat("[UI] -> ModalGenericPicker: {0}, ID {1}, DISPOSE due to LOYALTY selected{2}", actor.actorName, data.actorSlotID, "\n");
                                            break;
                                        case "DisposeCorrupt":
                                            text = string.Format("{0}Dispose of {1} because they are Corrupt{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.LogFormat("[UI] -> ModalGenericPicker: {0}, ID {1}, DISPOSE due to CORRUPTION{2}", actor.actorName, data.actorSlotID, "\n");
                                            break;
                                        case "DisposeHabit":
                                            text = string.Format("{0}Dispose of {1} because you can{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                                            Debug.LogFormat("[UI] -> ModalGenericPicker: {0}, ID {1}, DISPOSE due to HABIT{2}", actor.actorName, data.actorSlotID, "\n");
                                            break;
                                        default:
                                            Debug.LogErrorFormat("Invalid data.optionText \"{0}\"", data.optionNested);
                                            break;
                                    }

                                }
                                else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", data.actorSlotID); }
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
                case EventType.GenericTargetInfo:
                    text = string.Format("{0}Target{1} {2}selection{3}", colourEffect, colourEnd, colourNormal, colourEnd);
                    break;
                case EventType.GenericCompromisedGear:
                    text = string.Format("{0}Use Renown to {1}{2}SAVE ONE{3}{4} item{5} <size=70%>(optional)</size>", colourNormal, colourEnd, colourNeutral, colourEnd, 
                        colourNormal, colourEnd);
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
        returnData.optionName = optionNameSelected;
        returnData.optionNested = optionTextSelected;
        returnData.nodeID = nodeIDSelected;
        returnData.actorSlotID = actorSlotIDSelected;

        //close picker window regardless
        EventManager.i.PostNotification(EventType.CloseGenericPicker, this, null, "ModalGenericPicker.cs -> ProcessGenericChoice");
        //trigger the appropriate return Event and pass selected optionID back to the originating class
        switch (defaultReturnEvent)
        {
            case EventType.GenericTeamRecall:
            case EventType.GenericNeutraliseTeam:
            case EventType.GenericGearChoice:
            case EventType.GenericTargetInfo:
            case EventType.GenericCompromisedGear:
            case EventType.GenericRecruitActorResistance:
            case EventType.GenericRecruitActorAuthority:
            case EventType.GenericHandleActor:
            case EventType.GenericReserveActor:
            case EventType.GenericDismissActor:
            case EventType.GenericDisposeActor:
                EventManager.i.PostNotification(defaultReturnEvent, this, returnData, "ModalGenericPicker.cs -> ProcessGenericChoice");
                break;
            default:
                Debug.LogError(string.Format("Invalid returnEvent \"{0}\"", defaultReturnEvent));
                break;
        }
    }

    /// <summary>
    /// Closes generic picker and returns an event, if needed (sometimes you want to know if the user cancelled the dialogue)
    /// </summary>
    private void ProcessCancelButton()
    {
        //close picker window regardless
        EventManager.i.PostNotification(EventType.CloseGenericPicker, this, null, "ModalGenericPicker.cs -> ProcessGenericChoice");
        //trigger the appropriate return Event and pass selected optionID back to the originating class
        switch (defaultReturnEvent)
        {
            case EventType.GenericCompromisedGear:
                EventManager.i.PostNotification(defaultReturnEvent, this, null, "ModalGenericPicker.cs -> ProcessCancelButton");
                break;
            default:
                //No default needed
                break;
        }
    }


    /// <summary>
    /// Sets all sprites to the Player's current side
    /// </summary>
    private void SetSide()
    {
        switch(GameManager.i.sideScript.PlayerSide.level)
        {
            case 1:
                //Authority
                modalPanel.sprite = GameManager.i.sideScript.picker_background_Authority;
                modalHeader.sprite = GameManager.i.sideScript.header_background_Authority;
                //set button sprites
                buttonCancel.GetComponent<Image>().sprite = GameManager.i.sideScript.button_Authority;
                buttonConfirm.GetComponent<Image>().sprite = GameManager.i.sideScript.button_Authority;
                buttonBack.GetComponent<Image>().sprite = GameManager.i.sideScript.button_Authority;
                //set sprite transitions
                SpriteState spriteStateAuthority = new SpriteState();
                spriteStateAuthority.highlightedSprite = GameManager.i.sideScript.button_highlight_Authority;
                spriteStateAuthority.pressedSprite = GameManager.i.sideScript.button_Click;
                buttonCancel.spriteState = spriteStateAuthority;
                buttonConfirm.spriteState = spriteStateAuthority;
                buttonBack.spriteState = spriteStateAuthority;
                break;
            case 2:
                //Resistance
                modalPanel.sprite = GameManager.i.sideScript.picker_background_Rebel;
                modalHeader.sprite = GameManager.i.sideScript.header_background_Resistance;
                //set button sprites
                buttonCancel.GetComponent<Image>().sprite = GameManager.i.sideScript.button_Resistance;
                buttonConfirm.GetComponent<Image>().sprite = GameManager.i.sideScript.button_Resistance;
                buttonBack.GetComponent<Image>().sprite = GameManager.i.sideScript.button_Resistance;
                //set sprite transitions
                SpriteState spriteStateRebel = new SpriteState();
                spriteStateRebel.highlightedSprite = GameManager.i.sideScript.button_highlight_Resistance;
                spriteStateRebel.pressedSprite = GameManager.i.sideScript.button_Click;
                buttonCancel.spriteState = spriteStateRebel;
                buttonConfirm.spriteState = spriteStateRebel;
                buttonBack.spriteState = spriteStateRebel;
                break;
            default:
                Debug.LogErrorFormat("Invalid side \"{0}\"", GameManager.i.sideScript.PlayerSide.name);
                break;
        }
    }



    //place methods above here
}
