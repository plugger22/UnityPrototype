﻿using gameAPI;
using packageAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AIDisplayUI : MonoBehaviour
{
    //structural elements
    public Canvas aiCanvas;
    public GameObject aiDisplayObject;
    public Image mainPanel;
    public Image backgroundImage;
    public Image innerPanel;
    public Image subTopPanel;
    public Image subMiddlePanel;
    public Image subBottomPanel;
    public Image powerPanel;
    public Image textPanel;
    public Image buttonPanel;
    public Button cancelButton;
    public Button proceedButton;
    //mouse elements
    public Image tabSideMouse;
    public Image tabTopMouse;
    public Image tabBottomMouse;
    //flashers (if detected)
    public Image detectedFlasher;
    //text elements
    public TextMeshProUGUI tabTopText;
    public TextMeshProUGUI tabBottomText;
    public TextMeshProUGUI tabCloseText;
    public TextMeshProUGUI subTopUpper;
    public TextMeshProUGUI subTopLower;
    public TextMeshProUGUI subTopChance;
    public TextMeshProUGUI subMiddleUpper;
    public TextMeshProUGUI subMiddleLower;
    public TextMeshProUGUI subMiddleChance;
    public TextMeshProUGUI subBottomUpper;
    public TextMeshProUGUI subBottomLower;
    public TextMeshProUGUI subBottomChance;
    public TextMeshProUGUI decisionText;
    public TextMeshProUGUI gearText;
    //is Active
    [HideInInspector] public bool isActive;                 //true if in use (Resistance Player, Authority AI), false otherwise

    private int rebootTimer;                                //data passed in from AIManager.cs. Tab will only open if timer is 0
    private GlobalSide aiSide;                             //side the AI controls (opposite to player)
    private bool isFree;                                    //true once Power cost paid. Reset to false each turn.
    private string tabBottomTextCache;                      //needed for hacking detected text swap
    private string hackingDetected;
    private Coroutine myCoroutine;                          //used for flashing red alert if hacking attempt detected
    private bool isFading;
    private float flashRedTime;
    

    private GenericTooltipUI topTabTooltip;
    private GenericTooltipUI bottomTabTooltip;
    private GenericTooltipUI sideTabTooltip;
    private GenericTooltipUI topTaskTooltip;
    private GenericTooltipUI middleTaskTooltip;
    private GenericTooltipUI bottomTaskTooltip;
    private ButtonInteraction cancelInteraction;
    private ButtonInteraction proceedInteraction;

    #region Static instance...
    private static AIDisplayUI aiDisplayUI;
    
    /// <summary>
    /// provide a static reference to AIDisplayUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static AIDisplayUI Instance()
    {
        if (!aiDisplayUI)
        {
            aiDisplayUI = FindObjectOfType(typeof(AIDisplayUI)) as AIDisplayUI;
            if (!aiDisplayUI)
            { Debug.LogError("There needs to be one active aiDisplayUI script on a GameObject in your scene"); }
        }
        return aiDisplayUI;
    }
    #endregion

    #region Initialise
    /// <summary>
    /// Conditional activation based on player side for GameState.LoadGame
    /// </summary>
    /// <param name="state"></param>
    public void Initialise(GameState state)
    {
        //Resistance Player only
        if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideResistance.level)
        {
            switch (state)
            {
                case GameState.TutorialOptions:
                case GameState.NewInitialisation:
                    SubInitialiseAsserts();
                    SubInitialiseSessionStart();
                    SubInitialiseFastAccess();
                    SubInitialiseEvents();
                    SubInitialiseResistance();
                    break;
                case GameState.LoadAtStart:
                    SubInitialiseAsserts();
                    SubInitialiseSessionStart();
                    SubInitialiseFastAccess();
                    SubInitialiseEvents();
                    SubInitialiseResistance();
                    break;
                case GameState.LoadGame:
                    SubInitialiseSessionStart();
                    SubInitialiseFastAccess();
                    SubInitialiseEvents();
                    SubInitialiseResistance();
                    break;
                case GameState.FollowOnInitialisation:
                    break;
                default:
                    Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                    break;
            }
        }
    }
    #endregion

    #region Initialise SubMethods...

    #region SubInitialiseAsserts
    private void SubInitialiseAsserts()
    {
        Debug.Assert(aiCanvas != null, "Invalid aiCanvas (Null)");
        Debug.Assert(aiDisplayObject != null, "Invalid aiDisplayObject (Null)");
        Debug.Assert(mainPanel != null, "Invalid mainPanel (Null)");
        Debug.Assert(backgroundImage != null, "Invalid backgroundImage (Null)");
        Debug.Assert(innerPanel != null, "Invalid innerPanel (Null)");
        Debug.Assert(subTopPanel != null, "Invalid subTopPanel (Null)");
        Debug.Assert(subMiddlePanel != null, "Invalid subMiddlePanel (Null)");
        Debug.Assert(subBottomPanel != null, "Invalid subBottomPanel (Null)");
        Debug.Assert(powerPanel != null, "Invalid powerPanel (Null)");
        Debug.Assert(textPanel != null, "Invalid textPanel (Null)");
        Debug.Assert(buttonPanel != null, "Invalid buttonPanel (Null)");
        Debug.Assert(cancelButton != null, "Invalid cancelButton (Null)");
        Debug.Assert(proceedButton != null, "Invalid proceedButton (Null)");
        Debug.Assert(tabSideMouse != null, "Invalid tabSideMouse (Null)");
        Debug.Assert(tabTopMouse != null, "Invalid tabTopMouse (Null)");
        Debug.Assert(tabBottomMouse != null, "Invalid tabBottomMouse (Null)");
        Debug.Assert(detectedFlasher != null, "Invalid detectedFlasher (Null)");
        Debug.Assert(tabTopText != null, "Invalid tabTopText (Null)");
        Debug.Assert(tabBottomText != null, "Invalid tabBottomText (Null)");
        Debug.Assert(tabCloseText != null, "Invalid tabCloseText (Null)");
        Debug.Assert(subTopUpper != null, "Invalid subTopUpper (Null)");
        Debug.Assert(subTopLower != null, "Invalid subTopLower (Null)");
        Debug.Assert(subTopChance != null, "Invalid subTopChance (Null)");
        Debug.Assert(subMiddleUpper != null, "Invalid subMidleUpper (Null)");
        Debug.Assert(subMiddleLower != null, "Invalid subMidleLower (Null)");
        Debug.Assert(subMiddleChance != null, "Invalid subMidleChance (Null)");
        Debug.Assert(subBottomUpper != null, "Invalid subBottomUpper (Null)");
        Debug.Assert(subBottomLower != null, "Invalid subBottomLower (Null)");
        Debug.Assert(subBottomChance != null, "Invalid subBottomChance (Null)");
        Debug.Assert(decisionText != null, "Invalid decisionText (Null)");
        Debug.Assert(gearText != null, "Invalid gearText (Null)");
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        aiSide = GameManager.i.sideScript.GetAISide();      
        flashRedTime = GameManager.i.guiScript.flashRedTime;
        Debug.Assert(aiSide != null, "Invalid AI side (Null)");
        Debug.Assert(flashRedTime > 0, "Invalid flashRedTime ( <= 0 )");
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event listener
        EventManager.i.AddListener(EventType.AIDisplayOpen, OnEvent, "AIDisplayUI");
        EventManager.i.AddListener(EventType.AIDisplayClose, OnEvent, "AIDisplayUI");
        EventManager.i.AddListener(EventType.AISendDisplayData, OnEvent, "AIDisplayUI");
        EventManager.i.AddListener(EventType.AISendHackingData, OnEvent, "AIDisplayUI");
        EventManager.i.AddListener(EventType.AIDisplayPanelOpen, OnEvent, "AIDisplayUI");
    }
    #endregion

    #region SubInitialiseResistance
    private void SubInitialiseResistance()
    {
        //tooltip data
        InitialiseTooltips();
        Debug.Assert(string.IsNullOrEmpty(hackingDetected) == false, "Invalid hackingDetected (Null or Empty)");

        /*
        //set button sprites -> Not needed -> Aug '21
        cancelButton.GetComponent<Image>().sprite = GameManager.i.sideScript.button_Resistance;
        proceedButton.GetComponent<Image>().sprite = GameManager.i.sideScript.button_Resistance;
        */
        
        //top text
        tabTopText.text = string.Format("{0}{1}Authority AI", GameManager.i.globalScript.tagGlobalAIName, "\n");
        //active
        isActive = true;
        //set all sub compoponents
        SetAllStatus(isActive);
    }
    #endregion

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        hackingDetected = string.Format("Hacking Attempt{0}<b>DETECTED</b>", "\n");
        tabBottomText.text = string.Format("Hacking Status{0}<b>UNKNOWN</b>", "\n");
        //tabs
        topTabTooltip = tabTopMouse.GetComponent<GenericTooltipUI>();
        bottomTabTooltip = tabBottomMouse.GetComponent<GenericTooltipUI>();
        sideTabTooltip = tabSideMouse.GetComponent<GenericTooltipUI>();
        topTaskTooltip = subTopPanel.GetComponent<GenericTooltipUI>();
        middleTaskTooltip = subMiddlePanel.GetComponent<GenericTooltipUI>();
        bottomTaskTooltip = subBottomPanel.GetComponent<GenericTooltipUI>();
        Debug.Assert(topTabTooltip != null, "Invalid topTabTooltip (Null)");
        Debug.Assert(bottomTabTooltip != null, "Invalid bottomTabPanelTooltip (Null)");
        Debug.Assert(sideTabTooltip != null, "Invalid sideTabTooltip (Null)");
        Debug.Assert(topTaskTooltip != null, "Invalid topTaskTooltip (Null)");
        Debug.Assert(middleTaskTooltip != null, "Invalid middleTaskTooltip (Null)");
        Debug.Assert(bottomTaskTooltip != null, "Invalid bottomTaskTooltip (Null)");
        //initialise tooltips
        topTaskTooltip.Initialise();
        middleTaskTooltip.Initialise();
        bottomTabTooltip.Initialise();
        //buttons
        cancelInteraction = cancelButton.GetComponent<ButtonInteraction>();
        proceedInteraction = proceedButton.GetComponent<ButtonInteraction>();
        Debug.Assert(cancelInteraction != null, "Invalid cancelInteraction (Null)");
        Debug.Assert(proceedInteraction != null, "Invalid proceedInteraction (Null)");
        cancelInteraction.SetButton(EventType.AIDisplayClose);
        proceedInteraction.SetButton(EventType.AIDisplayPanelOpen);
    }
    #endregion

    #endregion

    #region OnEvent
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
            case EventType.AIDisplayOpen:
                if (rebootTimer == 0)
                { SetAIDisplay(); }
                break;
            case EventType.AIDisplayPanelOpen:
                OpenAIDisplayPanel();
                break;
            case EventType.AIDisplayClose:
                CloseAIDisplay();
                break;
            case EventType.AISendDisplayData:
                AIDisplayData dataDisplay = Param as AIDisplayData;
                ProcessDisplayData(dataDisplay);
                break;
            case EventType.AISendHackingData:
                AIHackingData dataHacking = Param as AIHackingData;
                ProcessHackingData(dataHacking);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion

    #region SetAllStatus
    /// <summary>
    /// set all UI components (apart from main) to active. Run at level start to ensure no problems (something hasn't been switched off in the editor)
    /// </summary>
    public void SetAllStatus(bool status)
    {
        //switch gui elements on/off
        aiCanvas.gameObject.SetActive(false);
        aiDisplayObject.SetActive(false);
        mainPanel.gameObject.SetActive(status);
        backgroundImage.gameObject.SetActive(status);
        innerPanel.gameObject.SetActive(status);
        subTopPanel.gameObject.SetActive(status);
        subMiddlePanel.gameObject.SetActive(status);
        subBottomPanel.gameObject.SetActive(status);
        tabCloseText.gameObject.SetActive(status);
        tabSideMouse.gameObject.SetActive(status);
        tabTopMouse.gameObject.SetActive(status);
        tabBottomMouse.gameObject.SetActive(status);
        powerPanel.gameObject.SetActive(status);
        decisionText.gameObject.SetActive(status);
        gearText.gameObject.SetActive(status);
        cancelButton.gameObject.SetActive(status);
        proceedButton.gameObject.SetActive(status);
        buttonPanel.gameObject.SetActive(status);
        //switch off flashers
        detectedFlasher.gameObject.SetActive(false);
    }
    #endregion

    #region InitialiseTooltips
    /// <summary>
    /// initialise data for fixed generic tooltips
    /// </summary>
    public void InitialiseTooltips()
    {
        //faction top tab tooltip
        topTabTooltip.tooltipHeader = GameManager.i.hqScript.GetHqName(aiSide);
        topTabTooltip.tooltipMain = GameManager.i.hqScript.GetHqDescription(aiSide);
        topTabTooltip.tooltipDetails = GameManager.i.hqScript.GetHqDetails(aiSide);
        topTabTooltip.x_offset = 175;
        //hacking bottom tab tooltip
        bottomTabTooltip.tooltipHeader = "";
        bottomTabTooltip.tooltipMain = string.Format("Hacking attempts <i>may</i> be{0}detected by the AI", "\n");
        bottomTabTooltip.tooltipDetails = string.Format("Check back for detailed information{0}once you've{1}<b>HACKED the AI</b>", "\n", "\n");
        bottomTabTooltip.x_offset = 175;
        //side tab tooltip
        sideTabTooltip.tooltipMain = GameManager.i.aiScript.GetCloseAITabTooltip();
        sideTabTooltip.x_offset = 25;
        //sideTabTooltip.x_offset = 30;
        //top Task tooltip
        topTaskTooltip.tooltipMain = "Unknown";
        topTaskTooltip.x_offset = 330;
        topTaskTooltip.y_offset = 180;
        //middle Task tooltip
        middleTaskTooltip.tooltipMain = "Unknown";
        middleTaskTooltip.x_offset = 330;
        middleTaskTooltip.y_offset = 250;
        //bottom Task tooltip
        bottomTaskTooltip.tooltipMain = "Unknown";
        bottomTaskTooltip.x_offset = 330;
        bottomTaskTooltip.y_offset = 310;
    }
    #endregion

    #region ProcessDisplayData
    /// <summary>
    /// populates all text fields with data package sent from AIManager.cs -> ProcessTasksDataPackage
    /// </summary>
    /// <param name="data"></param>
    private void ProcessDisplayData(AIDisplayData data)
    {
        if (data != null)
        {
            //timer & Power details
            rebootTimer = data.rebootTimer;
            decisionText.text = data.powerDecision;
            //reset isFree at beginning of each turn
            isFree = false;
            //
            // - - - 1st Task - - -
            //
            if (string.IsNullOrEmpty(data.task_1_textUpper) == false)
            { subTopUpper.text = data.task_1_textUpper; }
            else { subTopUpper.text = ""; }
            if (string.IsNullOrEmpty(data.task_1_textLower) == false)
            { subTopLower.text = data.task_1_textLower; }
            else { subTopLower.text = ""; }
            if (string.IsNullOrEmpty(data.task_1_chance) == false)
            { subTopChance.text = data.task_1_chance; }
            else { subTopChance.text = ""; }
            //1st task -> tooltip
            if (string.IsNullOrEmpty(data.task_1_tooltipMain) == false)
            {
                topTaskTooltip.tooltipMain = data.task_1_tooltipMain;
                if (string.IsNullOrEmpty(data.task_1_tooltipDetails) == false)
                { topTaskTooltip.tooltipDetails = data.task_1_tooltipDetails; }
            }
            else { topTaskTooltip.tooltipMain = ""; }
            topTaskTooltip.nodeID = data.nodeID_1;
            topTaskTooltip.connID = data.connID_1;
            topTaskTooltip.testTag = "Top Task";
            //
            // - - - 2nd Task - - -
            //
            if (string.IsNullOrEmpty(data.task_2_textUpper) == false)
            { subMiddleUpper.text = data.task_2_textUpper; }
            else { subMiddleUpper.text = ""; }
            if (string.IsNullOrEmpty(data.task_2_textLower) == false)
            { subMiddleLower.text = data.task_2_textLower; }
            else { subMiddleLower.text = ""; }
            if (string.IsNullOrEmpty(data.task_2_chance) == false)
            { subMiddleChance.text = data.task_2_chance; }
            else { subMiddleChance.text = ""; }
            //2nd Task -> tooltip
            if (string.IsNullOrEmpty(data.task_2_tooltipMain) == false)
            {
                middleTaskTooltip.tooltipMain = data.task_2_tooltipMain;
                if (string.IsNullOrEmpty(data.task_2_tooltipDetails) == false)
                { middleTaskTooltip.tooltipDetails = data.task_2_tooltipDetails; }
            }
            else { middleTaskTooltip.tooltipMain = ""; }
            middleTaskTooltip.nodeID = data.nodeID_2;
            middleTaskTooltip.connID = data.connID_2;
            middleTaskTooltip.testTag = "Middle Task";
            //
            // - - - 3rd Task - - -
            //
            if (string.IsNullOrEmpty(data.task_3_textUpper) == false)
            { subBottomUpper.text = data.task_3_textUpper; }
            else { subBottomUpper.text = ""; }
            if (string.IsNullOrEmpty(data.task_3_textLower) == false)
            { subBottomLower.text = data.task_3_textLower; }
            else { subBottomLower.text = ""; }
            if (string.IsNullOrEmpty(data.task_3_chance) == false)
            { subBottomChance.text = data.task_3_chance; }
            else { subBottomChance.text = ""; }
            //3rd Task -> tooltip
            if (string.IsNullOrEmpty(data.task_3_tooltipMain) == false)
            {
                bottomTaskTooltip.tooltipMain = data.task_3_tooltipMain;
                if (string.IsNullOrEmpty(data.task_3_tooltipDetails) == false)
                { bottomTaskTooltip.tooltipDetails = data.task_3_tooltipDetails; }
            }
            else { bottomTaskTooltip.tooltipMain = ""; }
            bottomTaskTooltip.nodeID = data.nodeID_3;
            bottomTaskTooltip.connID = data.connID_3;
            bottomTaskTooltip.testTag = "Bottom Task";
            //Faction
            if (string.IsNullOrEmpty(data.aiDetails) == false)
            { tabTopText.text = data.aiDetails; }
            else { tabTopText.text = ""; }
        }
        else { Debug.LogWarning("Invalid AIDisplayData package (Null)"); }
    }
    #endregion

    #region ProcessHackingData
    /// <summary>
    /// populates Hacking data (bottom tab in display) with data from AIManager.cs -> UpdateHackingStatus
    /// </summary>
    /// <param name="data"></param>
    private void ProcessHackingData(AIHackingData data)
    {
        if (data != null)
        {
            //bottom tab data
            if (string.IsNullOrEmpty(data.hackingStatus) == false)
            {
                tabBottomText.text = data.hackingStatus;
                tabBottomTextCache = data.hackingStatus;
            }
            else { tabBottomText.text = "Unknown Data"; }
            //bottom tab hacking tooltip
            if (string.IsNullOrEmpty(data.tooltipHeader) == false)
            { bottomTabTooltip.tooltipHeader = data.tooltipHeader; }
            else { bottomTabTooltip.tooltipHeader = ""; }
            if (string.IsNullOrEmpty(data.tooltipMain) == false)
            { bottomTabTooltip.tooltipMain = data.tooltipMain; }
            else { bottomTabTooltip.tooltipMain = ""; }
            if (string.IsNullOrEmpty(data.tooltipDetails) == false)
            { bottomTabTooltip.tooltipDetails = data.tooltipDetails; }
            else { bottomTabTooltip.tooltipDetails = ""; }
        }
        else { Debug.LogWarning("Invalid AIHackingData package (Null)"); }
    }
    #endregion

    #region SetAIDisplay
    /// <summary>
    /// show AI display
    /// </summary>
    public void SetAIDisplay()
    {
        //close side tab
        EventManager.i.PostNotification(EventType.AISideTabClose, this, null, "AIDisplayUI.cs -> SetAIDisplay");
        //Power panel on/off
        if (isFree == true)
        {
            powerPanel.gameObject.SetActive(false);
            tabBottomText.text = tabBottomTextCache;
            SetDetectedFlasher(false);
        }
        else
        {
            //get any relevant player modifiers
            GameManager.i.aiScript.UpdatePlayerHackingLists();
            //gear text
            gearText.text = GameManager.i.aiScript.UpdateGearText();
            //open UI
            powerPanel.gameObject.SetActive(true);
        }
        //switch on display
        aiCanvas.gameObject.SetActive(true);
        aiDisplayObject.SetActive(true);
        //set modal status
        GameManager.i.guiScript.SetIsBlocked(true);
        //turn off any alert message
        GameManager.i.alertScript.CloseAlertUI(true);
        //set game state
        ModalStateData package = new ModalStateData();
        package.mainState = ModalSubState.InfoDisplay;
        package.infoState = ModalInfoSubState.AIInfo;
        GameManager.i.inputScript.SetModalState(package);
    }
    #endregion

    #region OpenAIDisplayPanel
    /// <summary>
    /// 'Proceed' clicked on Power panel. Panel 'opened' to reveal AI data underneath
    /// </summary>
    public void OpenAIDisplayPanel()
    {
        powerPanel.gameObject.SetActive(false);
        isFree = true;

        /*//deduct cost [Edit] built into UpdateHackingStatus, made more sense [/edit]
        GameManager.instance.aiScript.UpdateHackingCost();*/

        //update hacking status
        if (GameManager.i.aiScript.UpdateHackingStatus() == true)
        {
            //switch on flashers
            detectedFlasher.gameObject.SetActive(true);
            //change bottom tab text
            tabBottomText.text = hackingDetected;
            SetDetectedFlasher(true);
        }
        else
        {
            //switch off flashers
            detectedFlasher.gameObject.SetActive(false);
        }
    }
    #endregion

    #region CloseAIDisplay
    /// <summary>
    /// close AIDisplayUI
    /// </summary>
    private void CloseAIDisplay()
    {
        GameManager.i.tooltipGenericScript.CloseTooltip("AIDisplayUI.cs -> CloseAIDisplay");
        aiCanvas.gameObject.SetActive(false);
        aiDisplayObject.SetActive(false);
        GameManager.i.guiScript.SetIsBlocked(false);
        //switch off flashers
        detectedFlasher.gameObject.SetActive(false);
        SetDetectedFlasher(false);
        //set game state
        GameManager.i.inputScript.ResetStates();
        Debug.LogFormat("[UI] AIDisplay.cs -> CloseAIDisplay{0}", "\n");
        //open side tab
        EventManager.i.PostNotification(EventType.AISideTabOpen, this, null, "AIDisplayUI.cs -> CloseAIDisplay");
    }
    #endregion

    #region SetDetectedFlasher
    /// <summary>
    /// Start (true) or Stop  (false) the flashing red indicator for a detected hacking attempt
    /// </summary>
    /// <param name="isStart"></param>
    private void SetDetectedFlasher(bool isStart)
    {
        switch (isStart)
        {
            case true:
                if (myCoroutine == null)
                { myCoroutine = StartCoroutine("ShowDetected"); }
                break;
            case false:
                if (myCoroutine != null)
                {
                    StopCoroutine(myCoroutine);
                    myCoroutine = null;
                    isFading = false;
                    //reset opacity back to zero
                    Color tempColor = detectedFlasher.color;
                    tempColor.a = 0.0f;
                    detectedFlasher.color = tempColor;
                }
                break;
        }
    }
    #endregion

    #region ShowDetected
    /// <summary>
    /// Coroutine for flasher
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowDetected()
    {
        //infinite loop
        while (true)
        {
            Color tempColor = detectedFlasher.color;
            if (isFading == false)
            {
                tempColor.a += Time.deltaTime / flashRedTime;
                if (tempColor.a >= 1.0f)
                { isFading = true; }
            }
            else
            {
                tempColor.a -= Time.deltaTime / flashRedTime;
                if (tempColor.a <= 0.0f)
                { isFading = false; }
            }
            detectedFlasher.color = tempColor;
            yield return null;
        }
    }
    #endregion

    //new methods above here
}
