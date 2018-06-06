﻿using gameAPI;
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
    //mouse elements
    public Image tabSideMouse;
    public Image tabTopMouse;
    public Image tabBottomMouse;
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

    private int rebootTimer;                                //data passed in from AIManager.cs. Tab will only open if timer is 0

    private GenericTooltipUI topTabTooltip;
    private GenericTooltipUI bottomTabTooltip;
    private GenericTooltipUI sideTabTooltip;

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
        topTabTooltip = tabTopMouse.GetComponent<GenericTooltipUI>();
        bottomTabTooltip = tabBottomMouse.GetComponent<GenericTooltipUI>();
        sideTabTooltip = tabSideMouse.GetComponent<GenericTooltipUI>();
        Debug.Assert(topTabTooltip != null, "Invalid topTabTooltip (Null)");
        Debug.Assert(bottomTabTooltip != null, "Invalid bottomTabPanelTooltip (Null)");
        Debug.Assert(sideTabTooltip != null, "Invalid sideTabTooltip (Null)");
        
    }



    public void Initialise()
    {
        InitialiseTooltips();
        //set all sub compoponents to Active
        SetAllToActive();
        
    }

    public void Start()
    {
        
        //event listener
        EventManager.instance.AddListener(EventType.AIDisplayOpen, OnEvent, "AIDisplayUI");
        EventManager.instance.AddListener(EventType.AIDisplayClose, OnEvent, "AIDisplayUI");
        EventManager.instance.AddListener(EventType.AISendDisplayData, OnEvent, "AIDisplayUI");
        EventManager.instance.AddListener(EventType.AISendHackingData, OnEvent, "AIDisplayUI");
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
    }

    /// <summary>
    /// initialise data for fixed generic tooltips
    /// </summary>
    public void InitialiseTooltips()
    {
        //faction top tab tooltip
        topTabTooltip.tooltipHeader = GameManager.instance.factionScript.GetFactionName();
        topTabTooltip.tooltipMain = GameManager.instance.factionScript.GetFactionDescription();
        topTabTooltip.tooltipDetails = GameManager.instance.factionScript.GetFactionDetails();
        topTabTooltip.x_offset = 175;
        //hacking bottom tab tooltip
        bottomTabTooltip.tooltipHeader = GameManager.instance.factionScript.GetFactionName();
        bottomTabTooltip.tooltipMain = GameManager.instance.factionScript.GetFactionDescription();
        bottomTabTooltip.tooltipDetails = GameManager.instance.factionScript.GetFactionDetails();
        bottomTabTooltip.x_offset = 175;
        //side tab tooltip
        sideTabTooltip.tooltipHeader = "Unknown";
        sideTabTooltip.tooltipMain = "Unknown";
        sideTabTooltip.x_offset = 30;
    }

    /// <summary>
    /// populates all text fields with data package sent from AIManager.cs -> ProcessTasksDataPackage
    /// </summary>
    /// <param name="data"></param>
    private void ProcessDisplayData(AIDisplayData data)
    {
        if (data != null)
        {
            //timer
            rebootTimer = data.rebootTimer;
            //1st Task
            if (String.IsNullOrEmpty(data.task_1_textUpper) == false)
            { subTopUpper.text = data.task_1_textUpper; }
            else { subTopUpper.text = ""; }
            if (String.IsNullOrEmpty(data.task_1_textLower) == false)
            { subTopLower.text = data.task_1_textLower; }
            else { subTopLower.text = ""; }
            if (String.IsNullOrEmpty(data.task_1_chance) == false)
            { subTopChance.text = data.task_1_chance; }
            else { subTopChance.text = ""; }
            //2nd Task
            if (String.IsNullOrEmpty(data.task_2_textUpper) == false)
            { subMiddleUpper.text = data.task_2_textUpper; }
            else { subMiddleUpper.text = ""; }
            if (String.IsNullOrEmpty(data.task_2_textLower) == false)
            { subMiddleLower.text = data.task_2_textLower; }
            else { subMiddleLower.text = ""; }
            if (String.IsNullOrEmpty(data.task_2_chance) == false)
            { subMiddleChance.text = data.task_2_chance; }
            else { subMiddleChance.text = ""; }
            //3rd Task
            if (String.IsNullOrEmpty(data.task_3_textUpper) == false)
            { subBottomUpper.text = data.task_3_textUpper; }
            else { subBottomUpper.text = ""; }
            if (String.IsNullOrEmpty(data.task_3_textLower) == false)
            { subBottomLower.text = data.task_3_textLower; }
            else { subBottomLower.text = ""; }
            if (String.IsNullOrEmpty(data.task_3_chance) == false)
            { subBottomChance.text = data.task_3_chance; }
            else { subBottomChance.text = ""; }
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
            if (String.IsNullOrEmpty(data.hackingStatus) == false)
            { tabBottomText.text = data.hackingStatus; }
            else { tabBottomText.text = "Unknown Data"; }
        }
        else { Debug.LogWarning("Invalid AIHackingData package (Null)"); }
    }

    /// <summary>
    /// show AI display
    /// </summary>
    public void SetAIDisplay()
    {
        //close side tab
        EventManager.instance.PostNotification(EventType.AISideTabClose, this);
        //switch on display
        aiDisplayObject.gameObject.SetActive(true);
        //set modal status
        GameManager.instance.guiScript.SetIsBlocked(true);
        //set game state
        GameManager.instance.inputScript.SetModalState(ModalState.InfoDisplay);
    }

    /// <summary>
    /// close AIDisplayUI
    /// </summary>
    private void CloseAIDisplay()
    {
        aiDisplayObject.gameObject.SetActive(false);
        GameManager.instance.guiScript.SetIsBlocked(false);
        //set game state
        GameManager.instance.inputScript.ResetStates();
        Debug.Log("UI: Close -> AIDisplayUI" + "\n");
        //open side tab
        EventManager.instance.PostNotification(EventType.AISideTabOpen, this);
    }
}
