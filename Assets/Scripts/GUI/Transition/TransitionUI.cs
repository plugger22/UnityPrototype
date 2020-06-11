using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using gameAPI;
using packageAPI;
using modalAPI;
using System;

/// <summary>
/// handles TransitionUI which is a single master UI with three togglable panels ->  end of level / HQ changes / Player status
/// </summary>
public class TransitionUI : MonoBehaviour
{

    #region Main

    [Header("Main")]
    public GameObject transitionObject;
    public Canvas transitionCanvas;
    public Image transitionBackground;
    public TextMeshProUGUI transitionHeader; 

    [Header("Main Buttons")]
    public Button buttonBack;
    public Button buttonContinue;
    public Button buttonExit;
    public Button buttonHelpMain;

    //button interactions
    private ButtonInteraction buttonInteractionBack;
    private ButtonInteraction buttonInteractionContinue;
    private ButtonInteraction buttonInteractionExit;

    //data package required to populate UI
    private TransitionInfoData transitionInfoData = new TransitionInfoData();

    private ModalTransitionSubState state;
   
    //colours
    string colourDefault;
    string colourNeutral;
    string colourGrey;
    string colourAlert;
    /*string colourGood;
    string colourBlue;
    string colourNormal;
    string colourError;
    string colourInvalid;*/
    string colourCancel;
    string colourEnd;

    //static reference
    private static TransitionUI transitionUI;

    #endregion

    #region End Level

    [Header("End Level")]
    public Canvas endLevelCanvas;
    public Image endLevelBackground;

    #endregion

    #region HQ Status

    [Header("HQ Status")]
    public Canvas hqCanvas;
    public Image hqBackground;
    public Image hierarchyBackground;
    public Image workerBackground;
    public Image textBackground;

    public HqInteraction optionBoss;
    public HqInteraction optionSubBoss1;
    public HqInteraction optionSubBoss2;
    public HqInteraction optionSubBoss3;

    public WorkerInteraction worker0;
    public WorkerInteraction worker1;
    public WorkerInteraction worker2;
    public WorkerInteraction worker3;
    public WorkerInteraction worker4;
    public WorkerInteraction worker5;
    public WorkerInteraction worker6;
    public WorkerInteraction worker7;

    public TextMeshProUGUI hierarchyLeft;
    public TextMeshProUGUI hierarchyRight;
    public TextMeshProUGUI workersLeft;
    public TextMeshProUGUI workersRight;

    //collections
    private HqInteraction[] arrayOfHqOptions;
    private WorkerInteraction[] arrayOfWorkerOptions;

    //assorted
    private float vacantActorAlpha = 0.50f;

    //fast access
    private Sprite vacantActorSprite;
    private string vacantActorCompatibility;

    #endregion

    #region Player Status

    [Header("Player Status")]
    public Canvas playerStatusCanvas;
    public Image playerStatusBackground;

    #endregion

    #region Briefing One

    [Header("BriefingOne")]
    public Canvas briefingOneCanvas;
    public Image briefingOneBackground;

    #endregion

    #region Briefing Two

    [Header("BriefingTwo")]
    public Canvas briefingTwoCanvas;
    public Image briefingTwoBackground;
    #endregion


    /// <summary>
    /// provide a static reference to TransitionUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TransitionUI Instance()
    {
        if (!transitionUI)
        {
            transitionUI = FindObjectOfType(typeof(TransitionUI)) as TransitionUI;
            if (!transitionUI)
            { Debug.LogError("There needs to be one active TransitionUI script on a GameObject in your scene"); }
        }
        return transitionUI;
    }



    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadGame:
            case GameState.FollowOnInitialisation:
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseSessionStart
    /// <summary>
    /// Session start
    /// </summary>
    private void SubInitialiseSessionStart()
    {
        #region Main
        //
        // - - - Main
        //
        Debug.Assert(transitionObject != null, "Invalid transitionObject (Null)");
        Debug.Assert(transitionCanvas != null, "Invalid transitionCanvas (Null)");
        Debug.Assert(transitionBackground != null, "Invalid transitionBackground (Null)");
        Debug.Assert(transitionHeader != null, "Invalid transitionHeader (Null)");
        Debug.Assert(buttonBack != null, "Invalid buttonBack (Null)");
        Debug.Assert(buttonContinue != null, "Invalid buttonContinue (Null)");
        Debug.Assert(buttonExit != null, "Invalid buttonExit (Null)");
        Debug.Assert(buttonHelpMain != null, "Invalid buttonHelpMain (Null)");
        //end level
        Debug.Assert(endLevelCanvas != null, "Invalid endLevelCanvas (Null)");
        Debug.Assert(endLevelBackground != null, "Invalid endLevelBackground (Null)");
        //hq
        Debug.Assert(hqCanvas != null, "Invalid hqCanvas (Null)");
        Debug.Assert(hqBackground != null, "Invalid hqBackground (Null)");
        //playerStatus
        Debug.Assert(playerStatusCanvas != null, "Invalid playerStatusCanvas (Null)");
        Debug.Assert(playerStatusBackground != null, "Invalid playerStatusBackground (Null)");
        //BriefingOne
        Debug.Assert(briefingOneCanvas != null, "Invalid briefingOneCanvas (Null)");
        Debug.Assert(briefingOneBackground != null, "Invalid briefingOneBackground (Null)");
        //BriefingTwo
        Debug.Assert(briefingTwoCanvas != null, "Invalid briefingTwoCanvas (Null)");
        Debug.Assert(briefingTwoBackground != null, "Invalid briefingTwoBackground (Null)");
        //Button events
        buttonInteractionBack = buttonBack.GetComponent<ButtonInteraction>();
        buttonInteractionContinue = buttonContinue.GetComponent<ButtonInteraction>();
        buttonInteractionExit = buttonExit.GetComponent<ButtonInteraction>();
        Debug.Assert(buttonInteractionBack != null, "Invalid buttonInteractionBack (Null)");
        Debug.Assert(buttonInteractionContinue != null, "Invalid buttonInteractionContinue (Null)");
        Debug.Assert(buttonInteractionExit != null, "Invalid buttonInteractionExit (Null)");
        buttonInteractionBack.SetButton(EventType.TransitionBack);
        buttonInteractionContinue.SetButton(EventType.TransitionContinue);
        buttonInteractionExit.SetButton(EventType.TransitionClose);
        //colours
        Color color = GameManager.i.guiScript.colourTransitionHeader;
        transitionHeader.color = color;
        //Set starting Initialisation states
        InitialiseTooltips();
        #endregion

        #region EndLevel

        #endregion

        #region HQ Status
        //
        // - - - HQ Status
        //
        Debug.Assert(hierarchyBackground != null, "Invalid hierarchyBackground (Null)");
        Debug.Assert(workerBackground != null, "Invalid workerBackground (Null)");
        Debug.Assert(textBackground != null, "Invalid textBackground (Null)");
        Debug.Assert(optionBoss != null, "Invalid optionBoss (Null)");
        Debug.Assert(optionSubBoss1 != null, "Invalid optionSubBoss1 (Null)");
        Debug.Assert(optionSubBoss2 != null, "Invalid optionSubBoss2 (Null)");
        Debug.Assert(optionSubBoss3 != null, "Invalid optionSubBoss3 (Null)");
        Debug.Assert(worker0 != null, "Invalid worker0 (Null)");
        Debug.Assert(worker1 != null, "Invalid worker1 (Null)");
        Debug.Assert(worker2 != null, "Invalid worker2 (Null)");
        Debug.Assert(worker3 != null, "Invalid worker3 (Null)");
        Debug.Assert(worker4 != null, "Invalid worker4 (Null)");
        Debug.Assert(worker5 != null, "Invalid worker5 (Null)");
        Debug.Assert(worker6 != null, "Invalid worker6 (Null)");
        Debug.Assert(worker7 != null, "Invalid worker7 (Null)");
        Debug.Assert(hierarchyLeft != null, "Invalid hierarchyLeft (Null)");
        Debug.Assert(hierarchyRight != null, "Invalid hierarchyRight (Null)");
        Debug.Assert(workersLeft != null, "Invalid workersLeft (Null)");
        Debug.Assert(workersRight != null, "Invalid workersRight (Null)");
        //collections
        arrayOfHqOptions = new HqInteraction[GameManager.i.hqScript.numOfActorsHQ];
        arrayOfWorkerOptions = new WorkerInteraction[GameManager.i.hqScript.maxNumOfWorkers];
        //populate
        if (arrayOfHqOptions.Length == 4)
        {
            arrayOfHqOptions[0] = optionBoss;
            arrayOfHqOptions[1] = optionSubBoss1;
            arrayOfHqOptions[2] = optionSubBoss2;
            arrayOfHqOptions[3] = optionSubBoss3;
        }
        else { Debug.LogErrorFormat("Invalid arrayOfHqOptions.Length \"{0}\"", arrayOfHqOptions.Length); }
        if (arrayOfWorkerOptions.Length == 8)
        {
            arrayOfWorkerOptions[0] = worker0;
            arrayOfWorkerOptions[1] = worker1;
            arrayOfWorkerOptions[2] = worker2;
            arrayOfWorkerOptions[3] = worker3;
            arrayOfWorkerOptions[4] = worker4;
            arrayOfWorkerOptions[5] = worker5;
            arrayOfWorkerOptions[6] = worker6;
            arrayOfWorkerOptions[7] = worker7;
        }
        else { Debug.LogErrorFormat("Invalid arrayOfWorkerOptions.Length \"{0}\"", arrayOfWorkerOptions.Length); }
        //initialise main screen components
        hierarchyLeft.text = "HIERARCHY";
        hierarchyRight.text = "HIERARCHY";
        workersLeft.text = "WORKERS";
        workersRight.text = "WORKERS";
        color = GameManager.i.guiScript.colourTransitionHqBackground;
        hierarchyLeft.color = color;
        hierarchyRight.color = color;
        workersLeft.color = color;
        workersRight.color = color;
        //lower alpha
        color.a = 0.50f;
        hierarchyBackground.color = color;
        hierarchyBackground.gameObject.SetActive(true);
        workerBackground.color = color;
        workerBackground.gameObject.SetActive(true);
        textBackground.gameObject.SetActive(true);
        #endregion

        #region Player Status

        #endregion

        #region Briefing One

        #endregion

        #region Briefing Two

        #endregion
    }
    #endregion

    #region SubInitialiseFastAccess
    /// <summary>
    /// fast access
    /// </summary>
    private void SubInitialiseFastAccess()
    {
        vacantActorSprite = GameManager.i.guiScript.vacantActorSprite;
        vacantActorCompatibility = GameManager.i.guiScript.GetCompatibilityStars(0);
        Debug.Assert(vacantActorSprite != null, "Invalid vacantActorSprite (Null)");
        Debug.Assert(string.IsNullOrEmpty(vacantActorCompatibility) == false, "Invalid vacantActorCompatibiity (Null or Empty)");
    }
    #endregion

    #region SubInitialiseEvents
    /// <summary>
    /// events
    /// </summary>
    private void SubInitialiseEvents()
    {
        //listeners
        EventManager.i.AddListener(EventType.ChangeColour, OnEvent, "TransitionUI");
        EventManager.i.AddListener(EventType.TransitionOpen, OnEvent, "TransitionUI");
        EventManager.i.AddListener(EventType.TransitionClose, OnEvent, "TransitionUI");
        EventManager.i.AddListener(EventType.TransitionContinue, OnEvent, "TransitionUI");
        EventManager.i.AddListener(EventType.TransitionBack, OnEvent, "TransitionUI");
    }
    #endregion

    #endregion

    /// <summary>
    /// Event handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.TransitionOpen:
                SetTransitionUI();
                break;
            case EventType.TransitionClose:
                ExecuteClose();
                break;
            case EventType.TransitionContinue:
                ExecuteContinue();
                break;
            case EventType.TransitionBack:
                ExecuteBack();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// Set colour palette for tooltip
    /// </summary>
    public void SetColours()
    {
        colourDefault = GameManager.i.colourScript.GetColour(ColourType.whiteText);
        colourNeutral = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourGrey = GameManager.i.colourScript.GetColour(ColourType.greyText);
        colourAlert = GameManager.i.colourScript.GetColour(ColourType.salmonText);
        /*colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodText);
        colourBlue = GameManager.instance.colourScript.GetColour(ColourType.blueText)
        colourNormal = GameManager.i.colourScript.GetColour(ColourType.normalText);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badText);
        colourError = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourInvalid = GameManager.instance.colourScript.GetColour(ColourType.salmonText);*/
        colourCancel = GameManager.i.colourScript.GetColour(ColourType.moccasinText);
        colourEnd = GameManager.i.colourScript.GetEndTag();
    }

    /// <summary>
    /// copy data package required to populate UI
    /// </summary>
    /// <param name="data"></param>
    public void SetTransitionInfoData(TransitionInfoData data)
    {
        if (data != null)
        {
            //clear out existing data and repopulate with new
            transitionInfoData.Reset();
            transitionInfoData = data;
        }
        else { Debug.LogError("Invalid TransitionInfoData (Null)"); }
    }

    /// <summary>
    /// Initialise Transition UI prior to use. Called from within SetTransitionUI
    /// NOTE: data checked for Null by parent method
    /// </summary>
    /// <param name="data"></param>
    public void InitialiseTransitionUI()
    {
        if (transitionInfoData != null)
        {
            //shorten name for ease of coding
            TransitionInfoData data = transitionInfoData;

            #region End Level

            #endregion

            #region HQ Status
            //
            // - - - HQ Status
            //
            //clear out hq arrays
            for (int i = 0; i < arrayOfHqOptions.Length; i++)
            {
                arrayOfHqOptions[i].optionImage.sprite = null;
                arrayOfHqOptions[i].textUpper.text = "";
                arrayOfHqOptions[i].textLower.text = "";
                arrayOfHqOptions[i].optionTooltip.tooltipHeader = "";
                arrayOfHqOptions[i].optionTooltip.tooltipMain = "";
                arrayOfHqOptions[i].optionTooltip.tooltipDetails = "";
            }
            //clear out worker arrays
            for (int i = 0; i < arrayOfWorkerOptions.Length; i++)
            {
                arrayOfWorkerOptions[i].optionImage.sprite = null;
                arrayOfWorkerOptions[i].textUpper.text = "";
                arrayOfWorkerOptions[i].textLower.text = "";
                arrayOfWorkerOptions[i].optionTooltip.tooltipHeader = "";
                arrayOfWorkerOptions[i].optionTooltip.tooltipMain = "";
                arrayOfWorkerOptions[i].optionTooltip.tooltipDetails = "";
            }
            //hq options -> Populate
            Debug.AssertFormat(data.listOfHqSprites.Count == data.listOfHqRenown.Count, "Mismatch on count for listOfHqSprites ({0} records) and listOfHqCompatibility ({1} records)",
                data.listOfHqSprites.Count, data.listOfHqRenown.Count);
            Debug.AssertFormat(data.listOfHqSprites.Count == data.listOfHqTitles.Count, "Mismatch on count for listOfHqSprites ({0} records) and listOfHqTitles ({1} records)",
                data.listOfHqSprites.Count, data.listOfHqTitles.Count);
            Debug.AssertFormat(data.listOfHqSprites.Count == data.listOfHqTooltips.Count, "Mismatch on count for listOfHqSprites ({0} records) and listOfHqTooltips ({1} records)",
                data.listOfHqSprites.Count, data.listOfHqTooltips.Count);
            for (int i = 0; i < data.listOfHqSprites.Count; i++)
            {
                arrayOfHqOptions[i].optionImage.sprite = data.listOfHqSprites[i];
                arrayOfHqOptions[i].textUpper.text = data.listOfHqRenown[i];
                arrayOfHqOptions[i].textLower.text = data.listOfHqTitles[i];
                //tooltip
                if (data.listOfHqTooltips[i] != null)
                {
                    arrayOfHqOptions[i].optionTooltip.tooltipHeader = data.listOfHqTooltips[i].header;
                    arrayOfHqOptions[i].optionTooltip.tooltipMain = data.listOfHqTooltips[i].main;
                    arrayOfHqOptions[i].optionTooltip.tooltipDetails = data.listOfHqTooltips[i].details;
                }
            }
            //worker options -> populate
            Debug.AssertFormat(data.listOfWorkerSprites.Count == data.listOfWorkerArcs.Count, "Mismatch -> listOfWorkersSprites has {0} records, listOfWorkerNames has {1} records",
                data.listOfWorkerSprites.Count, data.listOfWorkerArcs.Count);

            for (int i = 0; i < data.listOfWorkerSprites.Count; i++)
            {
                arrayOfWorkerOptions[i].optionImage.sprite = data.listOfWorkerSprites[i];
                arrayOfWorkerOptions[i].textUpper.text = data.listOfWorkerRenown[i];
                arrayOfWorkerOptions[i].textLower.text = data.listOfWorkerArcs[i];
                //tooltip
                if (data.listOfWorkerTooltips[i] != null)
                {
                    arrayOfWorkerOptions[i].optionTooltip.tooltipHeader = data.listOfWorkerTooltips[i].header;
                    arrayOfWorkerOptions[i].optionTooltip.tooltipMain = data.listOfWorkerTooltips[i].main;
                    arrayOfWorkerOptions[i].optionTooltip.tooltipDetails = data.listOfWorkerTooltips[i].details;
                }
                else { Debug.LogWarningFormat("Invalid tooltip (Null) for arrayOfWorkerOptions[{0}].optionTooltip", i); }
            }

            //toggle worker options on/alpha low
            for (int i = 0; i < arrayOfWorkerOptions.Length; i++)
            {
                if (arrayOfWorkerOptions[i].optionImage.sprite == null)
                {
                    arrayOfWorkerOptions[i].optionImage.sprite = vacantActorSprite;
                }
                else
                {
                    //viable option
                }
            }

            #endregion

            #region Player Status

            #endregion

            #region Briefing One

            #endregion

            #region Briefing Two

            #endregion
        }
        else { Debug.LogError("Invalid transitionInfoData (Null)"); }
    }

    /// <summary>
    /// Initialise fixed tooltips
    /// </summary>
    private void InitialiseTooltips()
    {
        List<HelpData> listOfHelp;
        /*//main help button
        listOfHelp = GameManager.i.helpScript.GetHelpData("metaGameUI_0", "metaGameUI_1", "metaGameUI_2", "metaGameUI_3");
        if (listOfHelp != null)
        { helpMain.SetHelpTooltip(listOfHelp, x_offset, y_offset); }
        else { Debug.LogWarning("Invalid listOfHelp for helpMain (Null)"); }*/
        
    }

    /// <summary>
    /// toggles on state canvas and turns all others off
    /// </summary>
    /// <param name="state"></param>
    private void SetCanvas(ModalTransitionSubState state)
    {
        switch (state)
        {
            case ModalTransitionSubState.EndLevel:
                endLevelCanvas.gameObject.SetActive(true);
                hqCanvas.gameObject.SetActive(false);
                playerStatusCanvas.gameObject.SetActive(false);
                briefingOneCanvas.gameObject.SetActive(false);
                briefingTwoCanvas.gameObject.SetActive(false);
                break;
            case ModalTransitionSubState.HQ:
                endLevelCanvas.gameObject.SetActive(false);
                hqCanvas.gameObject.SetActive(true);
                playerStatusCanvas.gameObject.SetActive(false);
                briefingOneCanvas.gameObject.SetActive(false);
                briefingTwoCanvas.gameObject.SetActive(false);
                break;
            case ModalTransitionSubState.PlayerStatus:
                endLevelCanvas.gameObject.SetActive(false);
                hqCanvas.gameObject.SetActive(false);
                playerStatusCanvas.gameObject.SetActive(true);
                briefingOneCanvas.gameObject.SetActive(false);
                briefingTwoCanvas.gameObject.SetActive(false);
                break;
            case ModalTransitionSubState.BriefingOne:
                endLevelCanvas.gameObject.SetActive(false);
                hqCanvas.gameObject.SetActive(false);
                playerStatusCanvas.gameObject.SetActive(false);
                briefingOneCanvas.gameObject.SetActive(true);
                briefingTwoCanvas.gameObject.SetActive(false);
                break;
            case ModalTransitionSubState.BriefingTwo:
                endLevelCanvas.gameObject.SetActive(false);
                hqCanvas.gameObject.SetActive(false);
                playerStatusCanvas.gameObject.SetActive(false);
                briefingOneCanvas.gameObject.SetActive(false);
                briefingTwoCanvas.gameObject.SetActive(true);
                break;
            default: Debug.LogWarningFormat("Unrecognised ModalTransitionState \"{0}\"", GameManager.i.inputScript.ModalTransitionState); break;
        }
    }

    /// <summary>
    /// toggles on/off main buttons
    /// </summary>
    /// <param name="state"></param>
    private void SetButtons(ModalTransitionSubState state)
    {
        switch (state)
        {
            case ModalTransitionSubState.EndLevel:
                buttonBack.gameObject.SetActive(false);
                buttonContinue.gameObject.SetActive(true);
                buttonExit.gameObject.SetActive(false);
                break;
            case ModalTransitionSubState.HQ:
                buttonBack.gameObject.SetActive(true);
                buttonContinue.gameObject.SetActive(true);
                buttonExit.gameObject.SetActive(false);
                break;
            case ModalTransitionSubState.PlayerStatus:
                buttonBack.gameObject.SetActive(true);
                buttonContinue.gameObject.SetActive(true);
                buttonExit.gameObject.SetActive(false);
                break;
            case ModalTransitionSubState.BriefingOne:
                buttonBack.gameObject.SetActive(true);
                buttonContinue.gameObject.SetActive(true);
                buttonExit.gameObject.SetActive(false);
                break;
            case ModalTransitionSubState.BriefingTwo:
                buttonBack.gameObject.SetActive(true);
                buttonContinue.gameObject.SetActive(false);
                buttonExit.gameObject.SetActive(true);
                break;
            default: Debug.LogWarningFormat("Unrecognised ModalTransitionState \"{0}\"", GameManager.i.inputScript.ModalTransitionState); break;
        }
    }

    /// <summary>
    /// Sets Header text correct for state
    /// </summary>
    /// <param name="state"></param>
    private void SetHeader(ModalTransitionSubState state)
    {
        switch (state)
        {
            case ModalTransitionSubState.EndLevel: transitionHeader.text = "Mission Review"; break;
            case ModalTransitionSubState.HQ: transitionHeader.text = "HQ Status"; break;
            case ModalTransitionSubState.PlayerStatus: transitionHeader.text = "Your Status"; break;
            case ModalTransitionSubState.BriefingOne: transitionHeader.text = "Briefing Part A"; break;
            case ModalTransitionSubState.BriefingTwo: transitionHeader.text = "Briefing Part B"; break;
            default: Debug.LogWarningFormat("Unrecognised ModalTransitionState \"{0}\"", GameManager.i.inputScript.ModalTransitionState); break;
        }
    }




    /// <summary>
    /// Display TransitionUI
    /// </summary>
    public void SetTransitionUI()
    {
        if (transitionInfoData != null)
        {
            //set blocked (fail safe)
            GameManager.i.guiScript.SetIsBlocked(true);
            //start in End Level
            ModalTransitionSubState startState = ModalTransitionSubState.EndLevel;
            GameManager.i.inputScript.SetModalState(new ModalStateData() { mainState = ModalSubState.Transition, transitionState = startState });
            InitialiseTransitionUI();
            transitionCanvas.gameObject.SetActive(true);



            //Main UI elements
            buttonHelpMain.gameObject.SetActive(true);
            SetButtons(startState);
            SetCanvas(startState);
            SetHeader(startState);

            Debug.LogFormat("[UI] TransitionUI.cs -> TransitionUI{0}", "\n");
        }
        else { Debug.LogWarning("Invalid TranistionInfoData (Null)"); }
    }


    /// <summary>
    /// Close TransitionUI
    /// </summary>
    private void ExecuteClose()
    {
        transitionCanvas.gameObject.SetActive(false);
        /*
        //outcome
        ModalOutcomeDetails details = new ModalOutcomeDetails();
        details.side = GameManager.i.sideScript.PlayerSide;
        details.textTop = GameManager.Formatt("HQ Assistance", ColourType.neutralText);
        details.textBottom = string.Format("HQ are willing to offer you {0} prior to your deployment", GameManager.Formatt("assistance", ColourType.moccasinText));
        details.sprite = GameManager.i.guiScript.infoSprite;
        details.modalLevel = 2;
        details.modalState = ModalSubState.MetaGame;
        details.triggerEvent = EventType.MetaGameOpen;
        //open outcome windown (will open MetaGameUI via triggerEvent once closed
        EventManager.i.PostNotification(EventType.OutcomeOpen, this, details, "TransitionUI.cs -> ExecuteClose");
        */

        //confirm window that will open metaOptions on closing
        ModalConfirmDetails details = new ModalConfirmDetails();
        details.topText = string.Format("HQ are willing to offer you {0} prior to your deployment", GameManager.Formatt("assistance", ColourType.moccasinText));
        details.bottomText = "Talk to HQ?";
        details.buttonFalse = "SAVE and EXIT";
        details.buttonTrue = "CONTINUE";
        details.eventFalse = EventType.SaveAndExit;
        details.eventTrue = EventType.MetaGameOpen;
        details.modalState = ModalSubState.MetaGame;
        details.restorePoint = RestorePoint.MetaOptions;
        //open confirm
        EventManager.i.PostNotification(EventType.ConfirmOpen, this, details, "TransitionUI.cs -> ExecuteClose");
    }

    /// <summary>
    /// Continue button pressed
    /// </summary>
    private void ExecuteContinue()
    {
        ModalTransitionSubState newState = ModalTransitionSubState.None;
        switch (GameManager.i.inputScript.ModalTransitionState)
        {
            case ModalTransitionSubState.EndLevel: newState = ModalTransitionSubState.HQ; break;
            case ModalTransitionSubState.HQ: newState = ModalTransitionSubState.PlayerStatus; break;
            case ModalTransitionSubState.PlayerStatus: newState = ModalTransitionSubState.BriefingOne; break;
            case ModalTransitionSubState.BriefingOne: newState = ModalTransitionSubState.BriefingTwo; break;
            case ModalTransitionSubState.BriefingTwo:  break; //do nothing
            default: Debug.LogWarningFormat("Unrecognised ModalTransitionState \"{0}\"", GameManager.i.inputScript.ModalTransitionState); break;
        }
        //set new state
        if (newState != ModalTransitionSubState.None)
        {
            GameManager.i.inputScript.SetModalState(new ModalStateData() { mainState = ModalSubState.Transition, transitionState = newState });
            //Adjust UI
            SetButtons(newState);
            SetHeader(newState);
            SetCanvas(newState);
        }
    }

    /// <summary>
    /// Back button pressed
    /// </summary>
    private void ExecuteBack()
    {
        ModalTransitionSubState newState = ModalTransitionSubState.None;
        switch (GameManager.i.inputScript.ModalTransitionState)
        {
            case ModalTransitionSubState.EndLevel:  break; //do nothing
            case ModalTransitionSubState.HQ: newState = ModalTransitionSubState.EndLevel; break;
            case ModalTransitionSubState.PlayerStatus: newState = ModalTransitionSubState.HQ; break;
            case ModalTransitionSubState.BriefingOne: newState = ModalTransitionSubState.PlayerStatus; break;
            case ModalTransitionSubState.BriefingTwo: newState = ModalTransitionSubState.BriefingOne; break;
            default: Debug.LogWarningFormat("Unrecognised ModalTransitionState \"{0}\"", GameManager.i.inputScript.ModalTransitionState); break;
        }
        //set new state
        if (newState != ModalTransitionSubState.None)
        {
            GameManager.i.inputScript.SetModalState(new ModalStateData() { mainState = ModalSubState.Transition, transitionState = newState });
            //Adjust UI
            SetButtons(newState);
            SetHeader(newState);
            SetCanvas(newState);
        }
    }

}
