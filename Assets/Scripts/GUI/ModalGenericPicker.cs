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
    public Image modalPanel;

    public TextMeshProUGUI topText;
    public TextMeshProUGUI middleText;
    public TextMeshProUGUI bottomText;

    public Button buttonCancel;
    public Button buttonConfirm;

    public Sprite errorSprite;                              //sprite to display in event of an error in the outcome dialogue

    public GameObject[] arrayOfGenericOptions;                //place generic image UI elements here (3 options)

    //private CanvasGroup canvasGroup;
    private ButtonInteraction buttonInteraction;

    private static ModalGenericPicker modalGenericPicker;

    private int optionIDSelected;                             //slot ID (eg arrayOfGenericOptions [index] of selected option
    private int nodeIDSelected;
    private int actorSlotIDSelected; 
    private EventType returnEvent;                          //event to trigger once confirmation button is clicked

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
        buttonInteraction = buttonConfirm.GetComponent<ButtonInteraction>();
    }

    private void Start()
    {
        //register listener
        EventManager.instance.AddListener(EventType.OpenGenericPicker, OnEvent);
        EventManager.instance.AddListener(EventType.CloseGenericPicker, OnEvent);
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
        EventManager.instance.AddListener(EventType.ConfirmGenericActivate, OnEvent);
        EventManager.instance.AddListener(EventType.ConfirmGenericChoice, OnEvent);
        EventManager.instance.AddListener(EventType.ConfirmGenericDeactivate, OnEvent);
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
                SetConfirmButton(true, (int)Param);
                break;
            case EventType.ConfirmGenericDeactivate:
                SetConfirmButton(false);
                break;
            case EventType.ConfirmGenericChoice:
                ProcessGenericChoice();
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
    /// Sets up Generic picker window
    /// </summary>
    private void SetGenericPicker(GenericPickerDetails details)
    {
        bool errorFlag = false;
        //set modal status
        GameManager.instance.Blocked(true);
        //activate dialogue window
        modalGenericObject.SetActive(true);
        //confirm button should be switched off at the start
        buttonConfirm.gameObject.SetActive(false);
        
        //canvasGroup.alpha = 100;

        //populate dialogue
        if (details != null)
        {
            //set up modal panel & buttons to be side appropriate
            switch(details.side)
            {
                case Side.Authority:
                    modalPanel.sprite = GameManager.instance.sideScript.picker_background_Authority;
                    //set button sprites
                    buttonCancel.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Authority;
                    buttonConfirm.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Authority;
                    //set sprite transitions
                    SpriteState spriteStateAuthority = new SpriteState();
                    spriteStateAuthority.highlightedSprite = GameManager.instance.sideScript.button_highlight_Authority;
                    spriteStateAuthority.pressedSprite = GameManager.instance.sideScript.button_Click;
                    buttonCancel.spriteState = spriteStateAuthority;
                    buttonConfirm.spriteState = spriteStateAuthority;
                    break;
                case Side.Resistance:
                    modalPanel.sprite = GameManager.instance.sideScript.picker_background_Rebel;
                    //set button sprites
                    buttonCancel.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Rebel;
                    buttonConfirm.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Rebel;
                    //set sprite transitions
                    SpriteState spriteStateRebel = new SpriteState();
                    spriteStateRebel.highlightedSprite = GameManager.instance.sideScript.button_highlight_Rebel;
                    spriteStateRebel.pressedSprite = GameManager.instance.sideScript.button_Click;
                    buttonCancel.spriteState = spriteStateRebel;
                    buttonConfirm.spriteState = spriteStateRebel;
                    break;
                default:
                    Debug.LogError(string.Format("Invalid side \"{0}\"", details.side));
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
                        GenericInteraction genericData = arrayOfGenericOptions[i].GetComponent<GenericInteraction>();
                        if (genericData != null)
                        {
                            //there are 3 options but not all of them may be used
                            if (details.arrayOfOptions[i] != null)
                            {
                                //activate option
                                arrayOfGenericOptions[i].SetActive(true);
                                //populate data
                                genericData.optionImage.sprite = details.arrayOfOptions[i].sprite;
                                genericData.optionText.text = details.arrayOfOptions[i].text;
                                genericData.optionID = details.arrayOfOptions[i].optionID;
                                //activate option (in Generic picker assumed all options are active)
                                genericData.isActive = true;

                                //tooltips
                                GenericTooltipUI tooltipUI = arrayOfGenericOptions[i].GetComponent<GenericTooltipUI>();
                                if (tooltipUI != null)
                                {
                                    GenericTooltipDetails tooltipDetails = details.arrayOfTooltips[i];
                                    if (tooltipDetails != null)
                                    {
                                        tooltipUI.ToolTipHeader = details.arrayOfTooltips[i].textHeader;
                                        tooltipUI.ToolTipMain = details.arrayOfTooltips[i].textMain;
                                        tooltipUI.ToolTipEffect = details.arrayOfTooltips[i].textDetails;
                                    }
                                    else { Debug.LogError(string.Format("Invalid tooltipDetails (Null) for arrayOfOptions[\"{0}\"]", i)); }
                                }
                                else
                                { Debug.LogError(string.Format("Invalid tooltipUI (Null) for arrayOfOptions[\"{0}\"]", i)); }
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
                returnEvent = details.returnEvent;
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
            outcomeDetails.textTop = "There has been a SNAFU and mo teams can be recalled";
            outcomeDetails.textBottom = "Heads, toes and other limbswill be removed";
            outcomeDetails.side = details.side;
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
        }
        //all good, generate
        else
        {
            //texts
            topText.text = details.textTop;
            middleText.text = details.textMiddle;
            bottomText.text = details.textBottom;
            //set game state
            GameManager.instance.inputScript.GameState = GameState.ModalPicker;
            Debug.Log("UI: Open -> ModalGenericPicker" + "\n");
        }
    }



    /// <summary>
    /// close Action Menu
    /// </summary>
    private void CloseGenericPicker()
    {
        modalGenericObject.SetActive(false);
        GameManager.instance.Blocked(false);
        //deselect all generic options to prevent picker opening next time with a preselected team
        EventManager.instance.PostNotification(EventType.DeselectOtherGenerics, this);
        SetConfirmButton(false);
        //set game state
        GameManager.instance.inputScript.GameState = GameState.Normal;
        Debug.Log("UI: Close -> ModalGenericPicker" + "\n");
    }


    /// <summary>
    /// Confirm button switched on/off. Only ON and visible if a generic optin has been selected
    /// </summary>
    /// <param name="activate"></param>
    public void SetConfirmButton(bool isActive, int optionID = -1)
    {
        string text = "Unknown"; ;
        //An option is selected
        if (isActive == true)
        {
            buttonConfirm.gameObject.SetActive(true);
            if (optionID > -1)
            {
                //update currently selected option
                optionIDSelected = optionID;
                //change top text to show which option selected
                switch(returnEvent)
                {
                    case EventType.GenericTeamRecall:
                        Team teamRecall = GameManager.instance.dataScript.GetTeam(optionID);
                        if (teamRecall != null)
                        {
                            text = string.Format("{0}{1} Team {2}{3}selected{4}", colourEffect, teamRecall.Arc.name.ToUpper(), colourEnd, colourDefault, colourEnd);
                            Debug.Log(string.Format("TeamPicker: teamArcID {0} selected{1}", optionID, "\n"));
                        }
                        else { Debug.LogError(string.Format("Invalid team (Null) for teamID {0}", optionID)); }
                        break;
                    case EventType.GenericNeutraliseTeam:
                        Team teamNeutralise = GameManager.instance.dataScript.GetTeam(optionID);
                        if (teamNeutralise != null)
                        {
                            text = string.Format("{0}{1} Team {2}{3}selected{4}", colourEffect, teamNeutralise.Arc.name.ToUpper(), colourEnd, colourDefault, colourEnd);
                            Debug.Log(string.Format("TeamPicker: teamArcID {0} selected{1}", optionID, "\n"));
                        }
                        else { Debug.LogError(string.Format("Invalid team (Null) for teamID {0}", optionID)); }
                        break;
                    case EventType.GenericGearChoice:
                        Gear gear = GameManager.instance.dataScript.GetGear(optionID);
                        if (gear != null)
                        {
                            text = string.Format("{0}{1}{2} {3}selected{4}", colourEffect, gear.name.ToUpper(), colourEnd, colourDefault, colourEnd);
                            Debug.Log(string.Format("GearPicker: gearID {0} selected{1}", optionID, "\n"));
                        }
                        else { Debug.LogError(string.Format("Invalid gear (Null) for gearID {0}", optionID)); }
                        break;
                    case EventType.GenericRecruitActor:
                        Actor actor = GameManager.instance.dataScript.GetActor(optionID);
                        if (actor != null)
                        {
                            text = string.Format("{0}{1}{2} {3}selected{4}", colourEffect, actor.arc.name, colourEnd, colourDefault, colourEnd);
                            Debug.Log(string.Format("RecruitPicker: actorID {0} selected{1}", optionID, "\n"));
                        }
                        else { Debug.LogError(string.Format("Invalid actor (Null) for gearID {0}", optionID)); }
                        break;
                }
            }
        }
        else
        {
            //Nothing currently selected, show generic message
            buttonConfirm.gameObject.SetActive(false);
            switch (returnEvent)
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
                case EventType.GenericRecruitActor:
                    text = string.Format("{0}Recruit{1} {2}selection{3}", colourEffect, colourEnd, colourNormal, colourEnd);
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
        returnData.nodeID = nodeIDSelected;
        returnData.actorSlotID = actorSlotIDSelected;
        //close picker window regardless
        EventManager.instance.PostNotification(EventType.CloseGenericPicker, this);
        //trigger the appropriate return Event and pass selected optionID back to the originating class
        switch (returnEvent)
        {
            case EventType.GenericTeamRecall:
            case EventType.GenericNeutraliseTeam:
            case EventType.GenericGearChoice:
            case EventType.GenericRecruitActor:
                EventManager.instance.PostNotification(returnEvent, this, returnData);
                break;
            default:
                Debug.LogError(string.Format("Invalid returnEvent \"{0}\"", returnEvent));
                break;
        }
    }


    //place methods above here
}
