﻿using gameAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles all topic decision UI matters
/// </summary>
public class TopicUI : MonoBehaviour
{
    [Header("Main")]
    public Canvas topicCanvas;
    public GameObject topicObject;

    [Header("Backgrounds")]
    public Image outerBackground;
    public Image innerBackground;

    [Header("Panels")]
    public Image panelBoss;

    [Header("Buttons")]
    public Button buttonOption0;
    public Button buttonOption1;
    public Button buttonOption2;
    public Button buttonOption3;
    public Button buttonIgnore;
    public Button buttonShowMe;
    public Button buttonHelp_generic;
    public Button buttonHelp_specific;
    public Button buttonStoryHelp0;
    public Button buttonStoryHelp1;

    [Header("Texts")]
    public TextMeshProUGUI textHeader;
    public TextMeshProUGUI textMain;
    public TextMeshProUGUI letterText;
    public TextMeshProUGUI textOption0;
    public TextMeshProUGUI textOption1;
    public TextMeshProUGUI textOption2;
    public TextMeshProUGUI textOption3;

    [Header("Sprite")]
    public Image imageTopic;
    public Image imageBoss;

    //background images
    private Image outerBackgroundImage;
    private Image innerBackgroundImage;
    //button script handlers
    private ButtonInteraction buttonInteractiveOption0;
    private ButtonInteraction buttonInteractiveOption1;
    private ButtonInteraction buttonInteractiveOption2;
    private ButtonInteraction buttonInteractiveOption3;
    private ButtonInteraction buttonInteractiveIgnore;
    private ButtonInteraction buttonInteractiveShowMe;
    //tooltips
    private GenericTooltipUI tooltipOption0;
    private GenericTooltipUI tooltipOption1;
    private GenericTooltipUI tooltipOption2;
    private GenericTooltipUI tooltipOption3;
    private GenericTooltipUI tooltipIgnore;
    private GenericTooltipUI tooltipShowMe;
    private GenericTooltipUI tooltipImage;
    private GenericTooltipUI tooltipBoss;
    //help
    private GenericHelpTooltipUI helpSpecific;
    private StoryHelpTooltipUI helpStory0;
    private StoryHelpTooltipUI helpStory1;
    //options
    private TopicOption[] arrayOfOptions;
    private Button[] arrayOfButtons;
    private TextMeshProUGUI[] arrayOfOptionTexts;
    private ButtonInteraction[] arrayOfButtonInteractions;
    private GenericTooltipUI[] arrayOfTooltips;

    //data package
    private TopicUIData dataPackage;

    //fast access
    private Sprite topicDefault;
    private Sprite topicOptionValid;
    private Sprite topicOptionInvalid;
    private Sprite topicOptionNormalValid;
    private Sprite topicOptionNormalInvalid;
    private Sprite topicOptionLetterValid;
    private Sprite topicOptionLetterInvalid;

    //static reference
    private static TopicUI topicUI;


    /// <summary>
    /// provide a static reference to TopicUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TopicUI Instance()
    {
        if (!topicUI)
        {
            topicUI = FindObjectOfType(typeof(TopicUI)) as TopicUI;
            if (!topicUI)
            { Debug.LogError("There needs to be one active topicUI script on a GameObject in your scene"); }
        }
        return topicUI;
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
    private void SubInitialiseSessionStart()
    {
        //initialise arrays
        int maxNumOfOptions = GameManager.i.topicScript.maxOptions;
        //hard coded max num of options based on how many fields in class (need to change if maxOptions increases)
        if (maxNumOfOptions > 0 && maxNumOfOptions < 5)
        {
            arrayOfOptions = new TopicOption[maxNumOfOptions];
            arrayOfButtons = new Button[maxNumOfOptions];
            arrayOfOptionTexts = new TextMeshProUGUI[maxNumOfOptions];
            arrayOfButtonInteractions = new ButtonInteraction[maxNumOfOptions];
            arrayOfTooltips = new GenericTooltipUI[maxNumOfOptions];
        }
        else { Debug.LogErrorFormat("Invalid maxOptions \"{0}\", arrays not initialised", maxNumOfOptions); }
        /*//background
        innerBackground = innerBackground.GetComponent<Image>();*/
        //UI elements
        Debug.Assert(topicCanvas != null, "Invalid topicCanvas (Null)");
        Debug.Assert(topicObject != null, "Invalid topicObject (Null)");
        Debug.Assert(panelBoss != null, "Invalid panelBoss (Null)");
        Debug.Assert(buttonOption0 != null, "Invalid buttonOption0 (Null)");
        Debug.Assert(buttonOption1 != null, "Invalid buttonOption1 (Null)");
        Debug.Assert(buttonOption2 != null, "Invalid buttonOption2 (Null)");
        Debug.Assert(buttonOption3 != null, "Invalid buttonOption3 (Null)");
        Debug.Assert(buttonIgnore != null, "Invalid buttonIgnore (Null)");
        Debug.Assert(buttonShowMe != null, "Invalid buttonShowMe (Null)");
        Debug.Assert(buttonHelp_generic != null, "Invalid buttonHelp_generic (Null)");
        Debug.Assert(buttonHelp_specific != null, "Invalid buttonHelp_specific (Null)");
        Debug.Assert(buttonStoryHelp0 != null, "Invalid buttonStoryHelp0 (Null)");
        Debug.Assert(buttonStoryHelp1 != null, "Invalid buttonStoryHelp1 (Null)");
        Debug.Assert(textHeader != null, "Invalid textHeader (Null)");
        Debug.Assert(textMain != null, "Invalid textMain (Null)");
        Debug.Assert(letterText != null, "Invalid letterText (Null)");
        Debug.Assert(textOption0 != null, "Invalid textOption0 (Null)");
        Debug.Assert(textOption1 != null, "Invalid textOption1 (Null)");
        Debug.Assert(textOption2 != null, "Invalid textOption2 (Null)");
        Debug.Assert(textOption3 != null, "Invalid textOption3 (Null)");
        Debug.Assert(imageTopic != null, "Invalid imageTopic (Null)");
        Debug.Assert(imageBoss != null, "Invalid imageBoss (Null)");
        //images -> outer background
        if (outerBackground != null)
        {
            outerBackgroundImage = outerBackground.GetComponent<Image>();
            Debug.Assert(outerBackgroundImage != null, "Invalid outerBackgroundImage (Null)");
        }
        else { Debug.LogError("Invalid outerBackground (Null)"); }
        //images -> inner background
        if (innerBackground != null)
        {
            innerBackgroundImage = innerBackground.GetComponent<Image>();
            Debug.Assert(innerBackgroundImage != null, "Invalid innerBackgroundImage (Null)");
        }
        else { Debug.LogError("Invalid innerBackground (Null)"); }
        //Button Interactive
        buttonInteractiveOption0 = buttonOption0.GetComponent<ButtonInteraction>();
        buttonInteractiveOption1 = buttonOption1.GetComponent<ButtonInteraction>();
        buttonInteractiveOption2 = buttonOption2.GetComponent<ButtonInteraction>();
        buttonInteractiveOption3 = buttonOption3.GetComponent<ButtonInteraction>();
        buttonInteractiveIgnore = buttonIgnore.GetComponent<ButtonInteraction>();
        buttonInteractiveShowMe = buttonShowMe.GetComponent<ButtonInteraction>();
        Debug.Assert(buttonInteractiveOption0 != null, "Invalid buttonInteractiveOption0 (Null)");
        Debug.Assert(buttonInteractiveOption1 != null, "Invalid buttonInteractiveOption1 (Null)");
        Debug.Assert(buttonInteractiveOption2 != null, "Invalid buttonInteractiveOption2 (Null)");
        Debug.Assert(buttonInteractiveOption3 != null, "Invalid buttonInteractiveOption3 (Null)");
        Debug.Assert(buttonInteractiveIgnore != null, "Invalid buttonInteractiveIgnore (Null)");
        Debug.Assert(buttonInteractiveShowMe != null, "Invalid buttonInteractiveShowMe (Null)");
        //Button events
        buttonInteractiveOption0?.SetButton(EventType.TopicDisplayOption);
        buttonInteractiveOption1?.SetButton(EventType.TopicDisplayOption);
        buttonInteractiveOption2?.SetButton(EventType.TopicDisplayOption);
        buttonInteractiveOption3?.SetButton(EventType.TopicDisplayOption);
        buttonInteractiveIgnore?.SetButton(EventType.TopicDisplayIgnore);
        //Tooltips
        tooltipOption0 = buttonOption0.GetComponent<GenericTooltipUI>();
        tooltipOption1 = buttonOption1.GetComponent<GenericTooltipUI>();
        tooltipOption2 = buttonOption2.GetComponent<GenericTooltipUI>();
        tooltipOption3 = buttonOption3.GetComponent<GenericTooltipUI>();
        tooltipIgnore = buttonIgnore.GetComponent<GenericTooltipUI>();
        tooltipShowMe = buttonShowMe.GetComponent<GenericTooltipUI>();
        tooltipImage = imageTopic.GetComponent<GenericTooltipUI>();
        tooltipBoss = imageBoss.GetComponent<GenericTooltipUI>();
        Debug.Assert(tooltipOption0 != null, "Invalid tooltipOption0 (Null)");
        Debug.Assert(tooltipOption1 != null, "Invalid tooltipOption1 (Null)");
        Debug.Assert(tooltipOption2 != null, "Invalid tooltipOption2 (Null)");
        Debug.Assert(tooltipOption3 != null, "Invalid tooltipOption3 (Null)");
        Debug.Assert(tooltipIgnore != null, "Invalid tooltipIgnore (Null)");
        Debug.Assert(tooltipShowMe != null, "Invalid tooltipShowMe (Null)");
        Debug.Assert(tooltipImage != null, "Invalid tooltipImage (Null)");
        Debug.Assert(tooltipBoss != null, "Invalid tooltipBoss (Null)");
        //populate arrayOfButtons
        arrayOfButtons[0] = buttonOption0;
        arrayOfButtons[1] = buttonOption1;
        arrayOfButtons[2] = buttonOption2;
        arrayOfButtons[3] = buttonOption3;
        //populate arrayOfButtonInteractions
        arrayOfButtonInteractions[0] = buttonInteractiveOption0;
        arrayOfButtonInteractions[1] = buttonInteractiveOption1;
        arrayOfButtonInteractions[2] = buttonInteractiveOption2;
        arrayOfButtonInteractions[3] = buttonInteractiveOption3;
        //populate arrayOfOptionTexts
        arrayOfOptionTexts[0] = textOption0;
        arrayOfOptionTexts[1] = textOption1;
        arrayOfOptionTexts[2] = textOption2;
        arrayOfOptionTexts[3] = textOption3;
        //populate arrayOfTooltips
        arrayOfTooltips[0] = tooltipOption0;
        arrayOfTooltips[1] = tooltipOption1;
        arrayOfTooltips[2] = tooltipOption2;
        arrayOfTooltips[3] = tooltipOption3;
        //fixed tooltips
        InitialiseTooltips();
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        topicDefault = GameManager.i.guiScript.topicDefaultSprite;
        topicOptionNormalValid = GameManager.i.guiScript.topicOptionNormalValidSprite;
        topicOptionNormalInvalid = GameManager.i.guiScript.topicOptionNormalInvalidSprite;
        topicOptionLetterValid = GameManager.i.guiScript.topicOptionLetterValidSprite;
        topicOptionLetterInvalid = GameManager.i.guiScript.topicOptionLetterInvalidSprite;
        Debug.Assert(topicDefault != null, "Invalid topicDefault sprite (Null)");
        Debug.Assert(topicOptionNormalValid != null, "Invalid topicOptionNormalValid sprite (Null)");
        Debug.Assert(topicOptionNormalInvalid != null, "Invalid topicOptionNormalInvalid sprite (Null)");
        Debug.Assert(topicOptionLetterValid != null, "Invalid topicOptionLetterValid sprite (Null)");
        Debug.Assert(topicOptionLetterInvalid != null, "Invalid topicOptionLetterInvalid sprite (Null)");
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event listener
        EventManager.i.AddListener(EventType.TopicDisplayOpen, OnEvent, "TopicUI");
        EventManager.i.AddListener(EventType.TopicDisplayClose, OnEvent, "TopicUI");
        EventManager.i.AddListener(EventType.TopicDisplayRestore, OnEvent, "TopicUI");
        EventManager.i.AddListener(EventType.TopicDisplayShowMe, OnEvent, "TopicUI");
        EventManager.i.AddListener(EventType.TopicDisplayIgnore, OnEvent, "TopicUI");
        EventManager.i.AddListener(EventType.StartTurnEarly, OnEvent, "TopicUI");
        EventManager.i.AddListener(EventType.TopicDisplayOption, OnEvent, "TopicUI");
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
            case EventType.TopicDisplayOpen:
                TopicUIData data = Param as TopicUIData;
                SetTopicDisplay(data);
                break;
            case EventType.TopicDisplayOption:
                ProcessTopicOption((int)Param);
                break;
            case EventType.TopicDisplayClose:
                CloseTopicUI((int)Param);
                break;
            case EventType.TopicDisplayRestore:
                RestoreTopicUI();
                break;
            case EventType.TopicDisplayShowMe:
                ProcessShowMe();
                break;
            case EventType.TopicDisplayIgnore:
                ProcessTopicIgnore();
                /*CloseTopicUI((int)Param);*/
                break;
            case EventType.StartTurnEarly:
                StartTurnEarly();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Pre processing admin
    /// </summary>
    private void StartTurnEarly()
    {
        //reset TopicUI.cs data package prior to end of turn topic processing
        dataPackage = null;
    }


    /// <summary>
    /// TopicManager.cs -> InitialiseTopicUI calls this to pass the data across. This is done rather than a direct call to SetTopicDisplay as it is activated conditionally (if data present) in the Msg Pipeline
    /// </summary>
    /// <param name="data"></param>
    public void InitialiseData(TopicUIData data)
    {
        if (data != null)
        { dataPackage = data; }
        else { Debug.LogError("Invalid TopicUIData (Null)"); }
    }

    /// <summary>
    /// returns true if a topic is available for display, false otherwise
    /// </summary>
    /// <returns></returns>
    public bool CheckIsTopic()
    { return dataPackage == null ? false : true; }


    /// <summary>
    /// Used by message pipeline to activate the TopicUI Display
    /// </summary>
    public void ActivateTopicDisplay()
    {
        if (dataPackage != null)
        { SetTopicDisplay(dataPackage); }
        else { Debug.LogError("Invalid dataPackage (Null)"); }
    }

    /// <summary>
    /// Initialise fixed tooltips
    /// </summary>
    private void InitialiseTooltips()
    {
        //guiManager.cs provides a standard ShowMe tooltip implementation
        Tuple<string, string, string> texts = GameManager.i.guiScript.GetShowMeTooltip();
        tooltipShowMe.tooltipHeader = texts.Item1;
        tooltipShowMe.tooltipMain = texts.Item2;
        tooltipShowMe.tooltipDetails = texts.Item3;
        //needs to be offset to prevent button being covered and causing 'blinking tooltip' syndrome (ShowMe and Ignore tooltips)
        tooltipShowMe.x_offset = 60;
        tooltipIgnore.x_offset = -402;
        tooltipImage.x_offset = -180;
        tooltipImage.y_offset = 10;
        tooltipBoss.x_offset = -275;
        //help
        List<HelpData> listOfHelp = GameManager.i.helpScript.GetHelpData("topicUI_0", "topicUI_1", "topicUI_2", "topicUI_3");
        if (listOfHelp != null && listOfHelp.Count > 0)
        {
            //generic help
            GenericHelpTooltipUI helpGeneric = buttonHelp_generic.GetComponent<GenericHelpTooltipUI>();
            if (helpGeneric != null)
            { helpGeneric.SetHelpTooltip(listOfHelp, 150, 200); }
            else { Debug.LogWarning("Invalid GenericHelpTooltipUI for helpGeneric (Null)"); }
            //specific help (don't populate help, just get component)
            helpSpecific = buttonHelp_specific.GetComponent<GenericHelpTooltipUI>();
            if (helpSpecific == null)
            { Debug.LogWarning("Invalid GenericHelpTooltipUI for helpSpecific (Null)"); }
            //story Help
            helpStory0 = buttonStoryHelp0.GetComponent<StoryHelpTooltipUI>();
            helpStory1 = buttonStoryHelp1.GetComponent<StoryHelpTooltipUI>();
            if (helpStory0 == null) { Debug.LogWarning("Invalid StoryHelpTooltipUI for helpStory0 (Null)"); }
            if (helpStory1 == null) { Debug.LogWarning("Invalid StoryHelpTooltipUI for helpStory1 (Null)"); }
        }
        else { Debug.LogWarning("Invalid listOfHelp (Null or Empty)"); }
    }

    /*/// <summary>
    /// Get fixed help data
    /// </summary>
    /// <returns></returns>
    private List<HelpData> GetInfoHelpList()
    { return GetHelpData("info_app_0", "info_app_1", "info_app_2", "info_app_3"); }*/

    /// <summary>
    /// Set up TopicUI for the specific TopicType required
    /// </summary>
    /// <param name="type"></param>
    private void SetTopicType(TopicUIData data)
    {
        Color color = outerBackgroundImage.color;
        switch (data.type)
        {
            case TopicDecisionType.Normal:
                //hide letter background
                outerBackgroundImage.color = new Color(color.r, color.g, color.b, 0.0f);
                //show inner image
                innerBackgroundImage.gameObject.SetActive(true);
                //texts
                textHeader.gameObject.SetActive(true);
                textMain.gameObject.SetActive(true);
                letterText.gameObject.SetActive(false);
                //option sprites
                topicOptionValid = topicOptionNormalValid;
                topicOptionInvalid = topicOptionNormalInvalid;
                //topic header
                if (string.IsNullOrEmpty(data.header) == false)
                { textHeader.text = data.header; }
                else
                {
                    Debug.LogWarningFormat("Invalid data.header (Null or Empty) for topic \"{0}\"", data.topicName);
                    textHeader.text = "Unknown";
                }
                //topic text
                if (string.IsNullOrEmpty(data.text) == false)
                { textMain.text = data.text; }
                else
                {
                    Debug.LogWarningFormat("Invalid data.text (Null or Empty) for topic \"{0}\"", data.topicName);
                    textHeader.text = "Unknown";
                }
                break;
            case TopicDecisionType.Letter:
                //100% alpha to display letter background
                outerBackgroundImage.color = new Color(color.r, color.g, color.b, 1.0f);
                //hide inner image
                innerBackgroundImage.gameObject.SetActive(false);
                //texts
                textHeader.gameObject.SetActive(false);
                textMain.gameObject.SetActive(false);
                letterText.gameObject.SetActive(true);
                //option sprites
                topicOptionValid = topicOptionLetterValid;
                topicOptionInvalid = topicOptionLetterInvalid;
                //letter text
                letterText.text = data.text;
                break;
            default: Debug.LogWarningFormat("Unrecognised topicDecisionType \"{0}\"", data.type); break;
        }
    }

    #region SetTopicDisplay
    /// <summary>
    /// Initialise topicUI display
    /// </summary>
    /// <param name="data"></param>
    private void SetTopicDisplay(TopicUIData data)
    {
        if (data != null)
        {
            SetTopicType(data);
            //set colour of background
            innerBackground.color = new Color(data.colour.r, data.colour.g, data.colour.b);
            //deactivate all options
            for (int i = 0; i < arrayOfButtons.Length; i++)
            { arrayOfButtons[i].gameObject.SetActive(false); }

            //topic sprite
            if (data.spriteMain != null)
            { imageTopic.sprite = data.spriteMain; }
            else
            {
                Debug.LogWarningFormat("Invalid data.sprite (Null or Empty) for topic \"{0}\"", data.topicName);
                //use default sprite
                imageTopic.sprite = GameManager.i.guiScript.topicDefaultSprite;
            }
            //story Help
            if (data.listOfStoryHelp != null)
            {
                switch (data.listOfStoryHelp.Count)
                {
                    case 0: buttonStoryHelp0.gameObject.SetActive(false); buttonStoryHelp1.gameObject.SetActive(false); break;
                    case 1:
                        buttonStoryHelp0.gameObject.SetActive(true);
                        buttonStoryHelp1.gameObject.SetActive(false);
                        helpStory0.SetHelpTooltip(data.listOfStoryHelp[0]);
                        break;
                    case 2:
                        buttonStoryHelp0.gameObject.SetActive(true);
                        buttonStoryHelp1.gameObject.SetActive(true);
                        helpStory0.SetHelpTooltip(data.listOfStoryHelp[0]);
                        helpStory1.SetHelpTooltip(data.listOfStoryHelp[1]);
                        break;
                }
            }
            else { buttonStoryHelp0.gameObject.SetActive(false); buttonStoryHelp1.gameObject.SetActive(false); }
            //HQ boss
            if (data.isBoss == true)
            {
                //boss present, toggle on
                panelBoss.gameObject.SetActive(true);
                //sprite
                if (data.spriteBoss != null)
                { imageBoss.sprite = data.spriteBoss; }
                else { Debug.LogWarning("Invalid spriteBoss (Null)"); }
                //initialise boss image tooltip
                if (string.IsNullOrEmpty(data.bossTooltipHeader) == false)
                {
                    tooltipBoss.gameObject.SetActive(true);
                    tooltipBoss.tooltipHeader = data.bossTooltipHeader;
                }
                else { tooltipBoss.tooltipHeader = ""; }
                if (string.IsNullOrEmpty(data.bossTooltipMain) == false)
                { tooltipBoss.tooltipMain = data.bossTooltipMain; }
                else { tooltipBoss.tooltipMain = ""; }
                if (string.IsNullOrEmpty(data.bossTooltipDetails) == false)
                { tooltipBoss.tooltipDetails = data.bossTooltipDetails; }
                else { tooltipBoss.tooltipDetails = ""; }
            }
            else { panelBoss.gameObject.SetActive(false); }
            //options
            if (data.listOfOptions != null)
            {
                int count = data.listOfOptions.Count;
                if (count > 0)
                {
                    Debug.AssertFormat(count <= arrayOfOptions.Length, "Mismatch on option count (should be {0}, is {1})", arrayOfOptions.Length, count);
                    //populate arrayOfOptions
                    for (int i = 0; i < count; i++)
                    {
                        TopicOption option = data.listOfOptions[i];
                        if (option != null)
                        {
                            arrayOfOptions[i] = option;
                            //initialise option text
                            if (string.IsNullOrEmpty(option.textToDisplay) == false)
                            { arrayOfOptionTexts[i].text = option.textToDisplay; }
                            else
                            {
                                arrayOfOptionTexts[i].text = "Unknown";
                                Debug.LogWarningFormat("Invalid optionText (Null or Empty) for arrayOfOptionTexts[{0}], topic \"{1}\"", i, data.topicName);
                            }
                            //initialise option tooltip
                            if (string.IsNullOrEmpty(option.tooltipHeader) == false)
                            {
                                arrayOfTooltips[i].gameObject.SetActive(true);
                                arrayOfTooltips[i].tooltipHeader = option.tooltipHeader;
                            }
                            if (string.IsNullOrEmpty(option.tooltipMain) == false)
                            { arrayOfTooltips[i].tooltipMain = option.tooltipMain; }
                            if (string.IsNullOrEmpty(option.tooltipDetails) == false)
                            { arrayOfTooltips[i].tooltipDetails = option.tooltipDetails; }
                            //need to offset to prevent obscuring part of button and suffering 'blinking tooltip' syndrome
                            arrayOfTooltips[i].x_offset = -550;
                            //button Interaction
                            arrayOfButtonInteractions[i].SetButton(EventType.TopicDisplayOption, i);
                            //button sprite (yellow bar to match yellow text for valid, grey all for invalid)
                            if (option.isValid == true) { arrayOfButtons[i].image.sprite = topicOptionValid; }
                            else { arrayOfButtons[i].image.sprite = topicOptionInvalid; }
                            //initialise option
                            arrayOfButtons[i].gameObject.SetActive(true);
                        }
                        else
                        { Debug.LogWarningFormat("Invalid option (Null) in listOfOptions[{0}] for topic \"{1}\"", i, data.topicName); }
                    }
                }
                else { Debug.LogWarningFormat("Invalid listOfOptions (Empty) for topic \"{0}\"", data.topicName); }
            }
            else { Debug.LogWarningFormat("Invalid listOfOptions (Null) for topic \"{0}\"", data.topicName); }
            //ignore button
            buttonInteractiveIgnore.SetButton(EventType.TopicDisplayIgnore, -1);
            //show Me button
            if (dataPackage.nodeID > -1)
            {
                buttonShowMe.gameObject.SetActive(true);
                buttonInteractiveShowMe.SetButton(EventType.TopicDisplayShowMe, -1);
            }
            else { buttonShowMe.gameObject.SetActive(false); }
            //optional second help icon
            if (data.listOfHelp != null && data.listOfHelp.Count > 0)
            {
                buttonHelp_specific.gameObject.SetActive(true);
                if (helpSpecific != null)
                {
                    List<HelpData> listOfHelpData = GameManager.i.helpScript.GetHelpData(data.listOfHelp);
                    if (listOfHelpData != null)
                    { helpSpecific.SetHelpTooltip(listOfHelpData, 150, 180); }
                    else { buttonHelp_specific.gameObject.SetActive(false); }
                }
                else { Debug.LogWarning("Invalid GenericHelpTooltipUI for helpSpecific (Null)"); }
            }
            else { buttonHelp_specific.gameObject.SetActive(false); }
            //initialise Image tooltip
            if (string.IsNullOrEmpty(data.imageTooltipHeader) == false)
            {
                tooltipImage.gameObject.SetActive(true);
                tooltipImage.tooltipHeader = data.imageTooltipHeader;
            }
            else { tooltipImage.tooltipHeader = ""; }
            if (string.IsNullOrEmpty(data.imageTooltipMain) == false)
            { tooltipImage.tooltipMain = data.imageTooltipMain; }
            else { tooltipImage.tooltipMain = ""; }
            if (string.IsNullOrEmpty(data.imageTooltipDetails) == false)
            { tooltipImage.tooltipDetails = data.imageTooltipDetails; }
            else { tooltipImage.tooltipDetails = ""; }
            //initialise ignore Button tooltip
            if (string.IsNullOrEmpty(data.ignoreTooltipHeader) == false)
            {
                tooltipIgnore.gameObject.SetActive(true);
                tooltipIgnore.tooltipHeader = data.ignoreTooltipHeader;
            }
            else { tooltipIgnore.tooltipHeader = ""; }
            if (string.IsNullOrEmpty(data.ignoreTooltipMain) == false)
            { tooltipIgnore.tooltipMain = data.ignoreTooltipMain; }
            else { tooltipIgnore.tooltipMain = ""; }
            if (string.IsNullOrEmpty(data.ignoreTooltipDetails) == false)
            { tooltipIgnore.tooltipDetails = data.ignoreTooltipDetails; }
            else { tooltipIgnore.tooltipDetails = ""; }
            //Fixed position at screen centre
            Vector3 screenPos = new Vector3();
            screenPos.x = Screen.width / 2;
            screenPos.y = Screen.height / 2;
            //set position
            topicObject.transform.position = screenPos;
            //set states
            ModalStateData package = new ModalStateData() { mainState = ModalSubState.Topic };
            GameManager.i.inputScript.SetModalState(package);
            GameManager.i.guiScript.SetIsBlocked(true);
            Debug.LogFormat("[UI] TopicUI.cs -> SetTopicDisplay{0}", "\n");
            //initialise Canvas (switches one everything once all ready to go)
            topicCanvas.gameObject.SetActive(true);
        }
        else { Debug.LogError("Invalid TopicUIData (Null)"); }
    }
    #endregion


    /// <summary>
    /// close TopicUI display. Outcome if parameter >= 0, none if otherwise
    /// </summary>
    private void CloseTopicUI(int isOutcome)
    {
        GameManager.i.tooltipGenericScript.CloseTooltip("TopicUI.cs -> CloseTopicUI");
        GameManager.i.tooltipHelpScript.CloseTooltip("TopicUI.cs -> CloseTopicUI");
        GameManager.i.tooltipStoryScript.CloseTooltip("TopicUI.cs -> CloseTopicUI");
        topicCanvas.gameObject.SetActive(false);
        Debug.LogFormat("[UI] TopicUI.cs -> CloseTopicDisplay{0}", "\n");
        //switch of AlertUI 
        GameManager.i.alertScript.CloseAlertUI();

        /*//set game state -> EDIT: Don't do this as you are going straight to the maininfoApp on every occasion and resetting the state allows clicks to node in the interim.
        GameManager.instance.inputScript.ResetStates();
        GameManager.instance.guiScript.SetIsBlocked(false);*/

        //set waitUntilDone false to continue with pipeline only if there is no outcome from the topic option selected
        if (isOutcome < 0)
        {
            //No outcome -> close topicUI and go straight to MainInfoApp
            GameManager.i.guiScript.waitUntilDone = false;
        }
    }

    /// <summary>
    /// Restore topicUI after a 'ShowMe'
    /// </summary>
    private void RestoreTopicUI()
    {
        //set game state
        ModalStateData package = new ModalStateData();
        package.mainState = ModalSubState.Topic;
        GameManager.i.inputScript.SetModalState(package);
        //restore topicUI
        topicCanvas.gameObject.SetActive(true);
        Debug.LogFormat("[UI] TopicUI.cs -> RestoreTopicUI after 'ShowMe'{0}", "\n");
    }

    /// <summary>
    /// Ignore button pressed, no option selected
    /// </summary>
    private void ProcessTopicIgnore()
    {
        if (dataPackage != null)
        {
            //normal
            if (dataPackage.listOfIgnoreEffects != null && dataPackage.listOfIgnoreEffects.Count > 0)
            {
                //close TopicUI (with an arbitrary parameter > -1 to indicate outcome window required)
                CloseTopicUI(1);
                //actual processing of selected option is handled by topicManager.cs
                GameManager.i.topicScript.ProcessIgnore();
            }
            else
            {
                //close TopicUI without an Outcome dialogue
                CloseTopicUI(-1);
                GameManager.i.topicScript.ProcessIgnoreBossOpinion();
                GameManager.i.topicScript.ProcessTopicAdmin();
            }
        }
        else
        {
            Debug.LogWarning("Invalid TopicUIData package (Null)");
            //close TopicUI without an Outcome dialogue
            CloseTopicUI(-1);
        }
    }

    /// <summary>
    /// Show Me button pressed, no option selected
    /// </summary>
    private void ProcessShowMe()
    {
        if (dataPackage.nodeID > -1)
        {
            //debug
            topicCanvas.gameObject.SetActive(false);
            ShowMeData data = new ShowMeData()
            {
                restoreEvent = EventType.TopicDisplayRestore,
                nodeID = dataPackage.nodeID
            };
            GameManager.i.guiScript.SetShowMe(data);
        }
        /*else { Debug.LogWarningFormat("Invalid nodeID \"{0}\" (expected to be > -1)", dataPackage.nodeID); } -> Edit: commented out as you could press SPACE and there might not be a showMe button*/
    }

    /// <summary>
    /// Process selected Topic Option (index range 0 -> 3 (maxOptions -1)
    /// </summary>
    /// <param name="optionIndex"></param>
    private void ProcessTopicOption(int optionIndex)
    {
        if (optionIndex >= 0 && optionIndex < GameManager.i.topicScript.maxOptions)
        {
            //check option is valid
            if (dataPackage.listOfOptions[optionIndex].isValid == true)
            {
                //close TopicUI (with parameter > -1 to indicate outcome window required)
                CloseTopicUI(optionIndex);
                //actual processing of selected option is handled by topicManager.cs
                GameManager.i.topicScript.ProcessOption(optionIndex);
            }
        }
        else
        {
            Debug.LogWarningFormat("Invalid optionIndex \"{0}\" (should be >= 0 and < maxOptions). Outcome cancelled", optionIndex);
            //close TopicUI without an Outcome dialogue
            CloseTopicUI(-1);
        }
    }




    //new methods above here
}
