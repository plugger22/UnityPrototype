using gameAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AIDisplayUI : MonoBehaviour
{
    //structural elements
    public GameObject aiDisplayObject;
    public Image mainPanel;
    public Image backgroundImage;
    public Image innerPanel;
    public Image subTopPanel;
    public Image subMiddlePanel;
    public Image subBottomPanel;
    public Image renownPanel;
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

    private int rebootTimer;                                //data passed in from AIManager.cs. Tab will only open if timer is 0
    private GlobalSide aiSide;                             //side the AI controls (opposite to player)
    private bool isFree;                                    //true once renown cost paid. Reset to false each turn.
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

    public void Awake()
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
        //buttons
        cancelInteraction = cancelButton.GetComponent<ButtonInteraction>();
        proceedInteraction = proceedButton.GetComponent<ButtonInteraction>();
        Debug.Assert(cancelInteraction != null, "Invalid cancelInteraction (Null)");
        Debug.Assert(proceedInteraction != null, "Invalid proceedInteraction (Null)");
        cancelInteraction.SetEvent(EventType.AIDisplayClose);
        proceedInteraction.SetEvent(EventType.AIDisplayPanelOpen);
    }



    public void Initialise()
    {
        //ai controlled side
        aiSide = GameManager.instance.sideScript.GetAISide();
        Debug.Assert(aiSide != null, "Invalid AI side (Null)");
        //tooltip data
        InitialiseTooltips();
        flashRedTime = GameManager.instance.guiScript.flashRedTime;
        Debug.Assert(flashRedTime > 0, "Invalid flashRedTime ( <= 0 )");
        //set all sub compoponents to Active
        SetAllToActive();
        Debug.Assert(string.IsNullOrEmpty(hackingDetected) == false, "Invalid hackingDetected (Null or Empty)");
        //set button sprites
        cancelButton.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Resistance;
        proceedButton.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Resistance;
    }

    public void Start()
    {
        
        //event listener
        EventManager.instance.AddListener(EventType.AIDisplayOpen, OnEvent, "AIDisplayUI");
        EventManager.instance.AddListener(EventType.AIDisplayClose, OnEvent, "AIDisplayUI");
        EventManager.instance.AddListener(EventType.AISendDisplayData, OnEvent, "AIDisplayUI");
        EventManager.instance.AddListener(EventType.AISendHackingData, OnEvent, "AIDisplayUI");
        EventManager.instance.AddListener(EventType.AIDisplayPanelOpen, OnEvent, "AIDisplayUI");
    }


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

    /// <summary>
    /// set all UI components (apart from main) to active. Run at level start to ensure no problems (something hasn't been switched off in the editor)
    /// </summary>
    private void SetAllToActive()
    {
        aiDisplayObject.gameObject.SetActive(true);
        mainPanel.gameObject.SetActive(true);
        backgroundImage.gameObject.SetActive(true);
        innerPanel.gameObject.SetActive(true);
        subTopPanel.gameObject.SetActive(true);
        subMiddlePanel.gameObject.SetActive(true);
        subBottomPanel.gameObject.SetActive(true);
        tabCloseText.gameObject.SetActive(true);
        tabSideMouse.gameObject.SetActive(true);
        tabTopMouse.gameObject.SetActive(true);
        tabBottomMouse.gameObject.SetActive(true);
        renownPanel.gameObject.SetActive(true);
        decisionText.gameObject.SetActive(true);
        gearText.gameObject.SetActive(true);
        cancelButton.gameObject.SetActive(true);
        proceedButton.gameObject.SetActive(true);
        buttonPanel.gameObject.SetActive(true);
        //switch off flashers
        detectedFlasher.gameObject.SetActive(false);
    }

    /// <summary>
    /// initialise data for fixed generic tooltips
    /// </summary>
    public void InitialiseTooltips()
    {
        //faction top tab tooltip
        topTabTooltip.tooltipHeader = GameManager.instance.factionScript.GetFactionName(aiSide);
        topTabTooltip.tooltipMain = GameManager.instance.factionScript.GetFactionDescription(aiSide);
        topTabTooltip.tooltipDetails = GameManager.instance.factionScript.GetFactionDetails(aiSide);
        topTabTooltip.x_offset = 175;
        //hacking bottom tab tooltip
        bottomTabTooltip.tooltipHeader = "";
        bottomTabTooltip.tooltipMain = string.Format("Hacking attempts <i>may</i> be{0}detected by the AI", "\n");
        bottomTabTooltip.tooltipDetails = string.Format("Check back for detailed information{0}once you've{1}<b>HACKED the AI</b>", "\n", "\n");
        bottomTabTooltip.x_offset = 175;
        //side tab tooltip
        sideTabTooltip.tooltipMain = GameManager.instance.aiScript.GetCloseAITabTooltip();
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

    /// <summary>
    /// populates all text fields with data package sent from AIManager.cs -> ProcessTasksDataPackage
    /// </summary>
    /// <param name="data"></param>
    private void ProcessDisplayData(AIDisplayData data)
    {
        if (data != null)
        {
            //timer & renown details
            rebootTimer = data.rebootTimer;
            decisionText.text = data.renownDecision;
            //reset isFree at beginning of each turn
            isFree = false;
            //
            // - - - 1st Task - - -
            //
            if (String.IsNullOrEmpty(data.task_1_textUpper) == false)
            { subTopUpper.text = data.task_1_textUpper; }
            else { subTopUpper.text = ""; }
            if (String.IsNullOrEmpty(data.task_1_textLower) == false)
            { subTopLower.text = data.task_1_textLower; }
            else { subTopLower.text = ""; }
            if (String.IsNullOrEmpty(data.task_1_chance) == false)
            { subTopChance.text = data.task_1_chance; }
            else { subTopChance.text = ""; }
            //1st task -> tooltip
            if (String.IsNullOrEmpty(data.task_1_tooltipMain) == false)
            {
                topTaskTooltip.tooltipMain = data.task_1_tooltipMain;
                if (String.IsNullOrEmpty(data.task_1_tooltipDetails) == false)
                { topTaskTooltip.tooltipDetails = data.task_1_tooltipDetails; }
            }
            else { topTaskTooltip.tooltipMain = ""; }
            topTaskTooltip.nodeID = data.nodeID_1;
            topTaskTooltip.connID = data.connID_1;
            topTaskTooltip.testTag = "Top Task";
            //
            // - - - 2nd Task - - -
            //
            if (String.IsNullOrEmpty(data.task_2_textUpper) == false)
            { subMiddleUpper.text = data.task_2_textUpper; }
            else { subMiddleUpper.text = ""; }
            if (String.IsNullOrEmpty(data.task_2_textLower) == false)
            { subMiddleLower.text = data.task_2_textLower; }
            else { subMiddleLower.text = ""; }
            if (String.IsNullOrEmpty(data.task_2_chance) == false)
            { subMiddleChance.text = data.task_2_chance; }
            else { subMiddleChance.text = ""; }
            //2nd Task -> tooltip
            if (String.IsNullOrEmpty(data.task_2_tooltipMain) == false)
            {
                middleTaskTooltip.tooltipMain = data.task_2_tooltipMain;
                if (String.IsNullOrEmpty(data.task_2_tooltipDetails) == false)
                { middleTaskTooltip.tooltipDetails = data.task_2_tooltipDetails; }
            }
            else { middleTaskTooltip.tooltipMain = ""; }
            middleTaskTooltip.nodeID = data.nodeID_2;
            middleTaskTooltip.connID = data.connID_2;
            middleTaskTooltip.testTag = "Middle Task";
            //
            // - - - 3rd Task - - -
            //
            if (String.IsNullOrEmpty(data.task_3_textUpper) == false)
            { subBottomUpper.text = data.task_3_textUpper; }
            else { subBottomUpper.text = ""; }
            if (String.IsNullOrEmpty(data.task_3_textLower) == false)
            { subBottomLower.text = data.task_3_textLower; }
            else { subBottomLower.text = ""; }
            if (String.IsNullOrEmpty(data.task_3_chance) == false)
            { subBottomChance.text = data.task_3_chance; }
            else { subBottomChance.text = ""; }
            //3rd Task -> tooltip
            if (String.IsNullOrEmpty(data.task_3_tooltipMain) == false)
            {
                bottomTaskTooltip.tooltipMain = data.task_3_tooltipMain;
                if (String.IsNullOrEmpty(data.task_3_tooltipDetails) == false)
                { bottomTaskTooltip.tooltipDetails = data.task_3_tooltipDetails; }
            }
            else { bottomTaskTooltip.tooltipMain = ""; }
            bottomTaskTooltip.nodeID = data.nodeID_3;
            bottomTaskTooltip.connID = data.connID_3;
            bottomTaskTooltip.testTag = "Bottom Task";
            //Faction
            if (String.IsNullOrEmpty(data.factionDetails) == false)
            { tabTopText.text = data.factionDetails; }
            else { tabTopText.text = ""; }
        }
        else { Debug.LogWarning("Invalid AIDisplayData package (Null)"); }
    }

    /// <summary>
    /// populates Hacking data (bottom tab in display) with data from AIManager.cs -> UpdateHackingStatus
    /// </summary>
    /// <param name="data"></param>
    private void ProcessHackingData(AIHackingData data)
    {
        if (data != null)
        {
            //bottom tab data
            if (String.IsNullOrEmpty(data.hackingStatus) == false)
            {
                tabBottomText.text = data.hackingStatus;
                tabBottomTextCache = data.hackingStatus;
            }
            else { tabBottomText.text = "Unknown Data"; }
            //bottom tab hacking tooltip
            if (String.IsNullOrEmpty(data.tooltipHeader) == false)
            { bottomTabTooltip.tooltipHeader = data.tooltipHeader; }
            else { bottomTabTooltip.tooltipHeader = ""; }
            if (String.IsNullOrEmpty(data.tooltipMain) == false)
            { bottomTabTooltip.tooltipMain = data.tooltipMain; }
            else { bottomTabTooltip.tooltipMain = ""; }
            if (String.IsNullOrEmpty(data.tooltipDetails) == false)
            { bottomTabTooltip.tooltipDetails = data.tooltipDetails; }
            else { bottomTabTooltip.tooltipDetails = ""; }
        }
        else { Debug.LogWarning("Invalid AIHackingData package (Null)"); }
    }

    /// <summary>
    /// show AI display
    /// </summary>
    public void SetAIDisplay()
    {
        //close side tab
        EventManager.instance.PostNotification(EventType.AISideTabClose, this, null, "AIDisplayUI.cs -> SetAIDisplay");
        //renown panel on/off
        if (isFree == true)
        {
            renownPanel.gameObject.SetActive(false);
            tabBottomText.text = tabBottomTextCache;
            SetDetectedFlasher(false);
        }
        else
        {
            //get any relevant player modifiers
            GameManager.instance.aiScript.UpdatePlayerHackingModifiers();
            //gear text
            gearText.text = GameManager.instance.aiScript.UpdateGearText();
            //open UI
            renownPanel.gameObject.SetActive(true);
        }
        //switch on display
        aiDisplayObject.gameObject.SetActive(true);
        //set modal status
        GameManager.instance.guiScript.SetIsBlocked(true);
        //turn off any alert message
        GameManager.instance.alertScript.CloseAlertUI(true);
        //set game state
        GameManager.instance.inputScript.SetModalState(ModalState.InfoDisplay, ModalInfo.AIInfo);
    }

    /// <summary>
    /// 'Proceed' clicked on renown panel. Panel 'opened' to reveal AI data underneath
    /// </summary>
    public void OpenAIDisplayPanel()
    {
        renownPanel.gameObject.SetActive(false);
        isFree = true;
        //deduct cost
        GameManager.instance.aiScript.UpdateHackingCost();
        //update hacking status
        if (GameManager.instance.aiScript.UpdateHackingStatus() == true)
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

    /// <summary>
    /// close AIDisplayUI
    /// </summary>
    private void CloseAIDisplay()
    {
        GameManager.instance.tooltipGenericScript.CloseTooltip("AIDisplayUI.cs -> CloseAIDisplay");
        aiDisplayObject.gameObject.SetActive(false);
        GameManager.instance.guiScript.SetIsBlocked(false);
        //switch off flashers
        detectedFlasher.gameObject.SetActive(false);
        SetDetectedFlasher(false);
        //set game state
        GameManager.instance.inputScript.ResetStates();
        Debug.LogFormat("[UI] AIDisplay.cs -> CloseAIDisplay{0}", "\n");
        //open side tab
        EventManager.instance.PostNotification(EventType.AISideTabOpen, this, null, "AIDisplayUI.cs -> CloseAIDisplay");
    }

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

    //new methods above here
}
