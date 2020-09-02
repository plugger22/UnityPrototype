using gameAPI;
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
    public Image innerBackgroundNormal;
    public Image innerBackgroundOther;

    [Header("Panels")]
    public Image panelBoss;
    public Image panelLetter;

    [Header("Buttons")]
    public Button optionNormal0;
    public Button optionNormal1;
    public Button optionNormal2;
    public Button optionNormal3;
    public Button optionOther0;
    public Button optionOther1;
    public Button optionOther2;
    public Button optionOther3;
    public Button buttonNormalIgnore;
    public Button buttonNormalShowMe;
    public Button buttonNormalHelp_generic;
    public Button buttonNormalHelp_specific;
    public Button buttonNormalStoryHelp0;
    public Button buttonNormalStoryHelp1;
    public Button buttonOtherIgnore;
    public Button buttonOtherShowMe;
    public Button buttonOtherHelp_generic;
    public Button buttonOtherHelp_specific;
    public Button buttonOtherStoryHelp0;
    public Button buttonOtherStoryHelp1;
    public Button buttonLetterStoryHelp0;
    public Button buttonLetterStoryHelp1;

    [Header("Texts")]
    public TextMeshProUGUI textHeader;
    public TextMeshProUGUI textMain;
    public TextMeshProUGUI otherText;
    public TextMeshProUGUI letterText;
    public TextMeshProUGUI textOptionNormal0;
    public TextMeshProUGUI textOptionNormal1;
    public TextMeshProUGUI textOptionNormal2;
    public TextMeshProUGUI textOptionNormal3;
    public TextMeshProUGUI textOptionOther0;
    public TextMeshProUGUI textOptionOther1;
    public TextMeshProUGUI textOptionOther2;
    public TextMeshProUGUI textOptionOther3;

    [Header("Images")]
    public Image imageTopicNormal;
    public Image imageTopicOther;
    public Image imageTopicLetter;
    public Image imageBoss;

    //button script handlers
    private ButtonInteraction normalInteractiveOption0;
    private ButtonInteraction normalInteractiveOption1;
    private ButtonInteraction normalInteractiveOption2;
    private ButtonInteraction normalInteractiveOption3;
    private ButtonInteraction normalInteractiveIgnore;
    private ButtonInteraction normalInteractiveShowMe;
    private ButtonInteraction otherInteractiveOption0;
    private ButtonInteraction otherInteractiveOption1;
    private ButtonInteraction otherInteractiveOption2;
    private ButtonInteraction otherInteractiveOption3;
    private ButtonInteraction otherInteractiveIgnore;
    private ButtonInteraction otherInteractiveShowMe;
    //tooltips
    private GenericTooltipUI tooltipNormalOption0;
    private GenericTooltipUI tooltipNormalOption1;
    private GenericTooltipUI tooltipNormalOption2;
    private GenericTooltipUI tooltipNormalOption3;
    private GenericTooltipUI tooltipNormalIgnore;
    private GenericTooltipUI tooltipNormalShowMe;
    private GenericTooltipUI tooltipNormalImage;
    private GenericTooltipUI tooltipLetterImage;
    private GenericTooltipUI tooltipNormalBoss;
    private GenericTooltipUI tooltipOtherOption0;
    private GenericTooltipUI tooltipOtherOption1;
    private GenericTooltipUI tooltipOtherOption2;
    private GenericTooltipUI tooltipOtherOption3;
    private GenericTooltipUI tooltipOtherIgnore;
    private GenericTooltipUI tooltipOtherShowMe;
    private GenericTooltipUI tooltipOtherImage;
    private GenericTooltipUI tooltipOtherBoss;
    //help
    private GenericHelpTooltipUI helpNormalSpecific;
    private StoryHelpTooltipUI helpNormalStory0;
    private StoryHelpTooltipUI helpNormalStory1;
    private StoryHelpTooltipUI helpLetterStory0;
    private StoryHelpTooltipUI helpLetterStory1;
    private GenericHelpTooltipUI helpOtherSpecific;
    private StoryHelpTooltipUI helpOtherStory0;
    private StoryHelpTooltipUI helpOtherStory1;
    //options
    private TopicOption[,] arrayOfOptions;
    private Button[,] arrayOfButtons;
    private TextMeshProUGUI[,] arrayOfOptionTexts;
    private ButtonInteraction[,] arrayOfButtonInteractions;
    private GenericTooltipUI[,] arrayOfTooltips;

    //data package
    private TopicUIData dataPackage;

    //fast access
    private Sprite topicDefault;
    private Sprite topicOptionValid;
    private Sprite topicOptionInvalid;
    private Sprite topicOptionNormalValid;
    private Sprite topicOptionNormalInvalid;
    private Sprite topicOptionOtherValid;
    private Sprite topicOptionOtherInvalid;

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
            arrayOfOptions = new TopicOption[(int)TopicDecisionType.Count, maxNumOfOptions];
            arrayOfButtons = new Button[(int)TopicDecisionType.Count, maxNumOfOptions];
            arrayOfOptionTexts = new TextMeshProUGUI[(int)TopicDecisionType.Count, maxNumOfOptions];
            arrayOfButtonInteractions = new ButtonInteraction[(int)TopicDecisionType.Count, maxNumOfOptions];
            arrayOfTooltips = new GenericTooltipUI[(int)TopicDecisionType.Count, maxNumOfOptions];
        }
        else { Debug.LogErrorFormat("Invalid maxOptions \"{0}\", arrays not initialised", maxNumOfOptions); }
        /*//background
        innerBackground = innerBackground.GetComponent<Image>();*/
        //UI elements
        Debug.Assert(topicCanvas != null, "Invalid topicCanvas (Null)");
        Debug.Assert(topicObject != null, "Invalid topicObject (Null)");
        Debug.Assert(panelBoss != null, "Invalid panelBoss (Null)");
        Debug.Assert(panelLetter != null, "Invalid panelLetter (Null)");
        Debug.Assert(optionNormal0 != null, "Invalid optionNormal0 (Null)");
        Debug.Assert(optionNormal1 != null, "Invalid optionNormal1 (Null)");
        Debug.Assert(optionNormal2 != null, "Invalid optionNormal2 (Null)");
        Debug.Assert(optionNormal3 != null, "Invalid optionNormal3 (Null)");
        Debug.Assert(optionOther0 != null, "Invalid optionOther0 (Null)");
        Debug.Assert(optionOther1 != null, "Invalid optionOther1 (Null)");
        Debug.Assert(optionOther2 != null, "Invalid optionOther2 (Null)");
        Debug.Assert(optionOther3 != null, "Invalid optionOther3 (Null)");
        Debug.Assert(buttonNormalIgnore != null, "Invalid buttonNormalIgnore (Null)");
        Debug.Assert(buttonNormalShowMe != null, "Invalid buttonNormalShowMe (Null)");
        Debug.Assert(buttonNormalHelp_generic != null, "Invalid buttonNormalHelp_generic (Null)");
        Debug.Assert(buttonNormalHelp_specific != null, "Invalid buttonNormalHelp_specific (Null)");
        Debug.Assert(buttonNormalStoryHelp0 != null, "Invalid buttonNormalStoryHelp0 (Null)");
        Debug.Assert(buttonNormalStoryHelp1 != null, "Invalid buttonNormalStoryHelp1 (Null)");
        Debug.Assert(buttonOtherIgnore != null, "Invalid buttonOtherIgnore (Null)");
        Debug.Assert(buttonOtherShowMe != null, "Invalid buttonOtherShowMe (Null)");
        Debug.Assert(buttonOtherHelp_generic != null, "Invalid buttonOtherHelp_generic (Null)");
        Debug.Assert(buttonOtherHelp_specific != null, "Invalid buttonOtherHelp_specific (Null)");
        Debug.Assert(buttonOtherStoryHelp0 != null, "Invalid buttonOtherStoryHelp0 (Null)");
        Debug.Assert(buttonOtherStoryHelp1 != null, "Invalid buttonOtherStoryHelp1 (Null)");
        Debug.Assert(buttonLetterStoryHelp0 != null, "Invalid buttonLetterStoryHelp0 (Null)");
        Debug.Assert(buttonLetterStoryHelp1 != null, "Invalid buttonLetterStoryHelp1 (Null)");
        Debug.Assert(textHeader != null, "Invalid textHeader (Null)");
        Debug.Assert(textMain != null, "Invalid textMain (Null)");
        Debug.Assert(otherText != null, "Invalid otherText (Null)");
        Debug.Assert(letterText != null, "Invalid letterText (Null)");
        Debug.Assert(textOptionNormal0 != null, "Invalid textOptionNormal0 (Null)");
        Debug.Assert(textOptionNormal1 != null, "Invalid textOptionNormal1 (Null)");
        Debug.Assert(textOptionNormal2 != null, "Invalid textOptionNormal2 (Null)");
        Debug.Assert(textOptionNormal3 != null, "Invalid textOptionNormal3 (Null)");
        Debug.Assert(textOptionOther0 != null, "Invalid textOptionOther0 (Null)");
        Debug.Assert(textOptionOther1 != null, "Invalid textOptionOther1 (Null)");
        Debug.Assert(textOptionOther2 != null, "Invalid textOptionOther2 (Null)");
        Debug.Assert(textOptionOther3 != null, "Invalid textOptionOther3 (Null)");
        Debug.Assert(imageTopicNormal != null, "Invalid imageTopicNormal (Null)");
        Debug.Assert(imageTopicOther != null, "Invalid imageTopicOther (Null)");
        Debug.Assert(imageTopicLetter != null, "Invalid imageTopicLetter (Null)");
        Debug.Assert(imageBoss != null, "Invalid imageBoss (Null)");
        Debug.Assert(outerBackground != null, "Invalid outerBackgroundImage (Null)");
        Debug.Assert(innerBackgroundNormal != null, "Invalid innerBackgroundNormal (Null)");
        Debug.Assert(innerBackgroundOther != null, "Invalid innerBackgroundOther (Null)");
        //Button Interactive
        normalInteractiveOption0 = optionNormal0.GetComponent<ButtonInteraction>();
        normalInteractiveOption1 = optionNormal1.GetComponent<ButtonInteraction>();
        normalInteractiveOption2 = optionNormal2.GetComponent<ButtonInteraction>();
        normalInteractiveOption3 = optionNormal3.GetComponent<ButtonInteraction>();
        normalInteractiveIgnore = buttonNormalIgnore.GetComponent<ButtonInteraction>();
        normalInteractiveShowMe = buttonNormalShowMe.GetComponent<ButtonInteraction>();
        otherInteractiveOption0 = optionOther0.GetComponent<ButtonInteraction>();
        otherInteractiveOption1 = optionOther1.GetComponent<ButtonInteraction>();
        otherInteractiveOption2 = optionOther2.GetComponent<ButtonInteraction>();
        otherInteractiveOption3 = optionOther3.GetComponent<ButtonInteraction>();
        otherInteractiveIgnore = buttonOtherIgnore.GetComponent<ButtonInteraction>();
        otherInteractiveShowMe = buttonOtherShowMe.GetComponent<ButtonInteraction>();
        Debug.Assert(normalInteractiveOption0 != null, "Invalid normalInteractiveOption0 (Null)");
        Debug.Assert(normalInteractiveOption1 != null, "Invalid normalInteractiveOption1 (Null)");
        Debug.Assert(normalInteractiveOption2 != null, "Invalid normalInteractiveOption2 (Null)");
        Debug.Assert(normalInteractiveOption3 != null, "Invalid normalInteractiveOption3 (Null)");
        Debug.Assert(normalInteractiveIgnore != null, "Invalid normalInteractiveIgnore (Null)");
        Debug.Assert(normalInteractiveShowMe != null, "Invalid normalInteractiveShowMe (Null)");
        Debug.Assert(otherInteractiveOption0 != null, "Invalid letterInteractiveOption0 (Null)");
        Debug.Assert(otherInteractiveOption1 != null, "Invalid letterInteractiveOption1 (Null)");
        Debug.Assert(otherInteractiveOption2 != null, "Invalid letterInteractiveOption2 (Null)");
        Debug.Assert(otherInteractiveOption3 != null, "Invalid letterInteractiveOption3 (Null)");
        Debug.Assert(otherInteractiveIgnore != null, "Invalid letterInteractiveIgnore (Null)");
        Debug.Assert(otherInteractiveShowMe != null, "Invalid letterInteractiveShowMe (Null)");
        //Button events
        normalInteractiveOption0?.SetButton(EventType.TopicDisplayOption);
        normalInteractiveOption1?.SetButton(EventType.TopicDisplayOption);
        normalInteractiveOption2?.SetButton(EventType.TopicDisplayOption);
        normalInteractiveOption3?.SetButton(EventType.TopicDisplayOption);
        normalInteractiveIgnore?.SetButton(EventType.TopicDisplayIgnore);
        otherInteractiveOption0?.SetButton(EventType.TopicDisplayOption);
        otherInteractiveOption1?.SetButton(EventType.TopicDisplayOption);
        otherInteractiveOption2?.SetButton(EventType.TopicDisplayOption);
        otherInteractiveOption3?.SetButton(EventType.TopicDisplayOption);
        otherInteractiveIgnore?.SetButton(EventType.TopicDisplayIgnore);
        //Tooltips
        tooltipNormalOption0 = optionNormal0.GetComponent<GenericTooltipUI>();
        tooltipNormalOption1 = optionNormal1.GetComponent<GenericTooltipUI>();
        tooltipNormalOption2 = optionNormal2.GetComponent<GenericTooltipUI>();
        tooltipNormalOption3 = optionNormal3.GetComponent<GenericTooltipUI>();
        tooltipNormalIgnore = buttonNormalIgnore.GetComponent<GenericTooltipUI>();
        tooltipNormalShowMe = buttonNormalShowMe.GetComponent<GenericTooltipUI>();
        tooltipNormalImage = imageTopicNormal.GetComponent<GenericTooltipUI>();
        tooltipNormalBoss = imageBoss.GetComponent<GenericTooltipUI>();
        tooltipOtherOption0 = optionOther0.GetComponent<GenericTooltipUI>();
        tooltipOtherOption1 = optionOther1.GetComponent<GenericTooltipUI>();
        tooltipOtherOption2 = optionOther2.GetComponent<GenericTooltipUI>();
        tooltipOtherOption3 = optionOther3.GetComponent<GenericTooltipUI>();
        tooltipOtherIgnore = buttonOtherIgnore.GetComponent<GenericTooltipUI>();
        tooltipOtherShowMe = buttonOtherShowMe.GetComponent<GenericTooltipUI>();
        tooltipOtherImage = imageTopicOther.GetComponent<GenericTooltipUI>();
        tooltipOtherBoss = imageBoss.GetComponent<GenericTooltipUI>();
        tooltipLetterImage = imageTopicLetter.GetComponent<GenericTooltipUI>();
        Debug.Assert(tooltipNormalOption0 != null, "Invalid tooltipNormalOption0 (Null)");
        Debug.Assert(tooltipNormalOption1 != null, "Invalid tooltipNormalOption1 (Null)");
        Debug.Assert(tooltipNormalOption2 != null, "Invalid tooltipNormalOption2 (Null)");
        Debug.Assert(tooltipNormalOption3 != null, "Invalid tooltipNormalOption3 (Null)");
        Debug.Assert(tooltipNormalIgnore != null, "Invalid tooltipNormalIgnore (Null)");
        Debug.Assert(tooltipNormalShowMe != null, "Invalid tooltipNormalShowMe (Null)");
        Debug.Assert(tooltipNormalImage != null, "Invalid tooltipNormalImage (Null)");
        Debug.Assert(tooltipNormalBoss != null, "Invalid tooltipNormalBoss (Null)");
        Debug.Assert(tooltipOtherOption0 != null, "Invalid tooltipOtherOption0 (Null)");
        Debug.Assert(tooltipOtherOption1 != null, "Invalid tooltipOtherOption1 (Null)");
        Debug.Assert(tooltipOtherOption2 != null, "Invalid tooltipOtherOption2 (Null)");
        Debug.Assert(tooltipOtherOption3 != null, "Invalid tooltipOtherOption3 (Null)");
        Debug.Assert(tooltipOtherIgnore != null, "Invalid tooltipOtherIgnore (Null)");
        Debug.Assert(tooltipOtherShowMe != null, "Invalid tooltipOtherShowMe (Null)");
        Debug.Assert(tooltipOtherImage != null, "Invalid tooltipOtherImage (Null)");
        Debug.Assert(tooltipOtherBoss != null, "Invalid tooltipOtherBoss (Null)");
        Debug.Assert(tooltipLetterImage != null, "Invalid tooltipLetterImage (Null)");
        //populate arrayOfButtons
        int indexNormal = (int)TopicBase.Normal;
        int indexOther = (int)TopicBase.Other;
        arrayOfButtons[indexNormal, 0] = optionNormal0;
        arrayOfButtons[indexNormal, 1] = optionNormal1;
        arrayOfButtons[indexNormal, 2] = optionNormal2;
        arrayOfButtons[indexNormal, 3] = optionNormal3;
        arrayOfButtons[indexOther, 0] = optionOther0;
        arrayOfButtons[indexOther, 1] = optionOther1;
        arrayOfButtons[indexOther, 2] = optionOther2;
        arrayOfButtons[indexOther, 3] = optionOther3;
        //populate arrayOfButtonInteractions
        arrayOfButtonInteractions[indexNormal, 0] = normalInteractiveOption0;
        arrayOfButtonInteractions[indexNormal, 1] = normalInteractiveOption1;
        arrayOfButtonInteractions[indexNormal, 2] = normalInteractiveOption2;
        arrayOfButtonInteractions[indexNormal, 3] = normalInteractiveOption3;
        arrayOfButtonInteractions[indexOther, 0] = otherInteractiveOption0;
        arrayOfButtonInteractions[indexOther, 1] = otherInteractiveOption1;
        arrayOfButtonInteractions[indexOther, 2] = otherInteractiveOption2;
        arrayOfButtonInteractions[indexOther, 3] = otherInteractiveOption3;
        //populate arrayOfOptionTexts
        arrayOfOptionTexts[indexNormal, 0] = textOptionNormal0;
        arrayOfOptionTexts[indexNormal, 1] = textOptionNormal1;
        arrayOfOptionTexts[indexNormal, 2] = textOptionNormal2;
        arrayOfOptionTexts[indexNormal, 3] = textOptionNormal3;
        arrayOfOptionTexts[indexOther, 0] = textOptionOther0;
        arrayOfOptionTexts[indexOther, 1] = textOptionOther1;
        arrayOfOptionTexts[indexOther, 2] = textOptionOther2;
        arrayOfOptionTexts[indexOther, 3] = textOptionOther3;
        //populate arrayOfTooltips
        arrayOfTooltips[indexNormal, 0] = tooltipNormalOption0;
        arrayOfTooltips[indexNormal, 1] = tooltipNormalOption1;
        arrayOfTooltips[indexNormal, 2] = tooltipNormalOption2;
        arrayOfTooltips[indexNormal, 3] = tooltipNormalOption3;
        arrayOfTooltips[indexOther, 0] = tooltipOtherOption0;
        arrayOfTooltips[indexOther, 1] = tooltipOtherOption1;
        arrayOfTooltips[indexOther, 2] = tooltipOtherOption2;
        arrayOfTooltips[indexOther, 3] = tooltipOtherOption3;

        //set gameObject to active
        topicObject.SetActive(true);
        outerBackground.gameObject.SetActive(true);
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
        topicOptionOtherValid = GameManager.i.guiScript.topicOptionOtherValidSprite;
        topicOptionOtherInvalid = GameManager.i.guiScript.topicOptionOtherInvalidSprite;
        Debug.Assert(topicDefault != null, "Invalid topicDefault sprite (Null)");
        Debug.Assert(topicOptionNormalValid != null, "Invalid topicOptionNormalValid sprite (Null)");
        Debug.Assert(topicOptionNormalInvalid != null, "Invalid topicOptionNormalInvalid sprite (Null)");
        Debug.Assert(topicOptionOtherValid != null, "Invalid topicOptionOtherValid sprite (Null)");
        Debug.Assert(topicOptionOtherInvalid != null, "Invalid topicOptionOtherInvalid sprite (Null)");
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
                SetTopicDisplayNormal(data);
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
        { DisplayTopic(dataPackage); }
        else { Debug.LogError("Invalid dataPackage (Null)"); }
    }

    #region InitialiseTooltips
    /// <summary>
    /// Initialise fixed tooltips
    /// </summary>
    private void InitialiseTooltips()
    {
        //guiManager.cs provides a standard ShowMe tooltip implementation
        Tuple<string, string, string> texts = GameManager.i.guiScript.GetShowMeTooltip();
        tooltipNormalShowMe.tooltipHeader = texts.Item1;
        tooltipNormalShowMe.tooltipMain = texts.Item2;
        tooltipNormalShowMe.tooltipDetails = texts.Item3;
        //needs to be offset to prevent button being covered and causing 'blinking tooltip' syndrome (ShowMe and Ignore tooltips)
        tooltipNormalShowMe.x_offset = 60;
        tooltipNormalIgnore.x_offset = -402;
        tooltipNormalImage.x_offset = -180;
        tooltipNormalImage.y_offset = 10;
        tooltipNormalBoss.x_offset = -275;
        tooltipLetterImage.x_offset = -180;
        tooltipLetterImage.y_offset = 10;
        //Other
        tooltipOtherShowMe.tooltipHeader = texts.Item1;
        tooltipOtherShowMe.tooltipMain = texts.Item2;
        tooltipOtherShowMe.tooltipDetails = texts.Item3;
        //needs to be offset to prevent button being covered and causing 'blinking tooltip' syndrome (ShowMe and Ignore tooltips)
        tooltipOtherShowMe.x_offset = 60;
        tooltipOtherIgnore.x_offset = -402;
        tooltipOtherImage.x_offset = -180;
        tooltipOtherImage.y_offset = 10;
        tooltipOtherBoss.x_offset = -275;
        //help
        List<HelpData> listOfHelp = GameManager.i.helpScript.GetHelpData("topicUI_0", "topicUI_1", "topicUI_2", "topicUI_3");
        if (listOfHelp != null && listOfHelp.Count > 0)
        {
            //generic help -> normal
            GenericHelpTooltipUI helpGenericNormal = buttonNormalHelp_generic.GetComponent<GenericHelpTooltipUI>();
            if (helpGenericNormal != null)
            { helpGenericNormal.SetHelpTooltip(listOfHelp, 150, 200); }
            else { Debug.LogWarning("Invalid GenericHelpTooltipUI for helpGenericNormal (Null)"); }
            GenericHelpTooltipUI helpGenericOther = buttonOtherHelp_generic.GetComponent<GenericHelpTooltipUI>();
            //generic help -> letters
            if (helpGenericOther != null)
            { helpGenericOther.SetHelpTooltip(listOfHelp, 150, 200); }
            else { Debug.LogWarning("Invalid GenericHelpTooltipUI for helpGenericOther (Null)"); }
            //specific help (don't populate help, just get component)
            helpNormalSpecific = buttonNormalHelp_specific.GetComponent<GenericHelpTooltipUI>();
            helpOtherSpecific = buttonOtherHelp_specific.GetComponent<GenericHelpTooltipUI>();
            if (helpNormalSpecific == null) { Debug.LogWarning("Invalid GenericHelpTooltipUI for helpNormalSpecific (Null)"); }
            if (helpOtherSpecific == null) { Debug.LogWarning("Invalid GenericHelpTooltipUI for helpOtherSpecific (Null)"); }
            //story Help
            helpNormalStory0 = buttonNormalStoryHelp0.GetComponent<StoryHelpTooltipUI>();
            helpNormalStory1 = buttonNormalStoryHelp1.GetComponent<StoryHelpTooltipUI>();
            helpOtherStory0 = buttonOtherStoryHelp0.GetComponent<StoryHelpTooltipUI>();
            helpOtherStory1 = buttonOtherStoryHelp1.GetComponent<StoryHelpTooltipUI>();
            helpLetterStory0 = buttonLetterStoryHelp0.GetComponent<StoryHelpTooltipUI>();
            helpLetterStory1 = buttonLetterStoryHelp1.GetComponent<StoryHelpTooltipUI>();
            if (helpNormalStory0 == null) { Debug.LogWarning("Invalid StoryHelpTooltipUI for helpNormalStory0 (Null)"); }
            if (helpNormalStory1 == null) { Debug.LogWarning("Invalid StoryHelpTooltipUI for helpNormalStory1 (Null)"); }
            if (helpOtherStory0 == null) { Debug.LogWarning("Invalid StoryHelpTooltipUI for helpOtherStory0 (Null)"); }
            if (helpOtherStory1 == null) { Debug.LogWarning("Invalid StoryHelpTooltipUI for helpOtherStory1 (Null)"); }
            if (helpLetterStory0 == null) { Debug.LogWarning("Invalid StoryHelpTooltipUI for helpLetterStory0 (Null)"); }
            if (helpLetterStory1 == null) { Debug.LogWarning("Invalid StoryHelpTooltipUI for helpLetterStory1 (Null)"); }
        }
        else { Debug.LogWarning("Invalid listOfHelp (Null or Empty)"); }
    }
    #endregion


    /// <summary>
    /// Displays relevant topic base UI type
    /// </summary>
    /// <param name="data"></param>
    private void DisplayTopic(TopicUIData data)
    {
        switch (data.baseType)
        {
            case TopicBase.Normal:
                innerBackgroundNormal.gameObject.SetActive(true);
                innerBackgroundOther.gameObject.SetActive(false);
                SetTopicDisplayNormal(data);
                break;
            case TopicBase.Other:
                innerBackgroundNormal.gameObject.SetActive(false);
                innerBackgroundOther.gameObject.SetActive(true);
                SetTopicDisplayOther(data);
                break;
            default: Debug.LogWarningFormat("Unrecognised data.type \"{0}\"", data.uiType); break;
        }
    }


    #region SetTopicDisplayNormal
    /// <summary>
    /// Initialise topicUI display
    /// </summary>
    /// <param name="data"></param>
    private void SetTopicDisplayNormal(TopicUIData data)
    {
        if (data != null)
        {
            int index = (int)TopicBase.Normal;
            //deactivate all options
            for (int i = 0; i < arrayOfButtons.GetUpperBound(1) + 1; i++)
            { arrayOfButtons[index, i].gameObject.SetActive(false); }

            //set colour of background
            innerBackgroundNormal.color = new Color(data.colour.r, data.colour.g, data.colour.b, 1.0f);
            //toggle UI elements depending on type of normal Display
            switch (data.uiType)
            {
                case TopicDecisionType.Normal:
                    //toggle off letter
                    panelLetter.gameObject.SetActive(false);
                    //texts
                    textHeader.gameObject.SetActive(true);
                    textMain.gameObject.SetActive(true);
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
                    //topic sprite
                    imageTopicNormal.gameObject.SetActive(true);
                    if (data.spriteMain != null)
                    { imageTopicNormal.sprite = data.spriteMain; }
                    else
                    {
                        Debug.LogWarningFormat("Invalid data.sprite (Null or Empty) for topic \"{0}\"", data.topicName);
                        //use default sprite
                        imageTopicNormal.sprite = GameManager.i.guiScript.topicDefaultSprite;
                    }
                    //story Help
                    if (data.listOfStoryHelp != null)
                    {
                        switch (data.listOfStoryHelp.Count)
                        {
                            case 0: buttonNormalStoryHelp0.gameObject.SetActive(false); buttonNormalStoryHelp1.gameObject.SetActive(false); break;
                            case 1:
                                buttonNormalStoryHelp0.gameObject.SetActive(true);
                                buttonNormalStoryHelp1.gameObject.SetActive(false);
                                helpNormalStory0.SetHelpTooltip(data.listOfStoryHelp[0]);
                                break;
                            case 2:
                                buttonNormalStoryHelp0.gameObject.SetActive(true);
                                buttonNormalStoryHelp1.gameObject.SetActive(true);
                                helpNormalStory0.SetHelpTooltip(data.listOfStoryHelp[0]);
                                helpNormalStory1.SetHelpTooltip(data.listOfStoryHelp[1]);
                                break;
                        }
                    }
                    else { buttonNormalStoryHelp0.gameObject.SetActive(false); buttonNormalStoryHelp1.gameObject.SetActive(false); }
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
                            tooltipNormalBoss.gameObject.SetActive(true);
                            tooltipNormalBoss.tooltipHeader = data.bossTooltipHeader;
                        }
                        else { tooltipNormalBoss.tooltipHeader = ""; }
                        if (string.IsNullOrEmpty(data.bossTooltipMain) == false)
                        { tooltipNormalBoss.tooltipMain = data.bossTooltipMain; }
                        else { tooltipNormalBoss.tooltipMain = ""; }
                        if (string.IsNullOrEmpty(data.bossTooltipDetails) == false)
                        { tooltipNormalBoss.tooltipDetails = data.bossTooltipDetails; }
                        else { tooltipNormalBoss.tooltipDetails = ""; }
                    }
                    else { panelBoss.gameObject.SetActive(false); }
                    //initialise Image tooltip
                    if (string.IsNullOrEmpty(data.imageTooltipHeader) == false)
                    {
                        tooltipNormalImage.gameObject.SetActive(true);
                        tooltipNormalImage.tooltipHeader = data.imageTooltipHeader;
                    }
                    else { tooltipNormalImage.tooltipHeader = ""; }
                    if (string.IsNullOrEmpty(data.imageTooltipMain) == false)
                    { tooltipNormalImage.tooltipMain = data.imageTooltipMain; }
                    else { tooltipNormalImage.tooltipMain = ""; }
                    if (string.IsNullOrEmpty(data.imageTooltipDetails) == false)
                    { tooltipNormalImage.tooltipDetails = data.imageTooltipDetails; }
                    else { tooltipNormalImage.tooltipDetails = ""; }
                    break;
                case TopicDecisionType.Letter:
                    //toggle on letter background
                    panelLetter.gameObject.SetActive(true);
                    //text
                    letterText.gameObject.SetActive(true);
                    if (string.IsNullOrEmpty(data.text) == false)
                    { letterText.text = data.text; }
                    else
                    { Debug.LogWarningFormat("Invalid data.text (Null or Empty) for topic \"{0}\"", data.topicName); }
                    //sprite
                    imageTopicNormal.gameObject.SetActive(false);
                    imageTopicLetter.gameObject.SetActive(true);
                    if (data.spriteMain != null)
                    { imageTopicLetter.sprite = data.spriteMain; }
                    else
                    {
                        Debug.LogWarningFormat("Invalid data.sprite (Null or Empty) for topic \"{0}\"", data.topicName);
                        //use default sprite
                        imageTopicLetter.sprite = GameManager.i.guiScript.topicDefaultSprite;
                    }
                    //story Help
                    if (data.listOfStoryHelp != null)
                    {
                        switch (data.listOfStoryHelp.Count)
                        {
                            case 0: buttonLetterStoryHelp0.gameObject.SetActive(false); buttonLetterStoryHelp1.gameObject.SetActive(false); break;
                            case 1:
                                buttonLetterStoryHelp0.gameObject.SetActive(true);
                                buttonLetterStoryHelp1.gameObject.SetActive(false);
                                helpLetterStory0.SetHelpTooltip(data.listOfStoryHelp[0]);
                                break;
                            case 2:
                                buttonLetterStoryHelp0.gameObject.SetActive(true);
                                buttonLetterStoryHelp1.gameObject.SetActive(true);
                                helpLetterStory0.SetHelpTooltip(data.listOfStoryHelp[0]);
                                helpLetterStory1.SetHelpTooltip(data.listOfStoryHelp[1]);
                                break;
                        }
                    }
                    else { buttonLetterStoryHelp0.gameObject.SetActive(false); buttonLetterStoryHelp1.gameObject.SetActive(false); }
                    //HQ Boss off
                    panelBoss.gameObject.SetActive(false);
                    //initialise Image tooltip
                    if (string.IsNullOrEmpty(data.imageTooltipHeader) == false)
                    {
                        tooltipLetterImage.gameObject.SetActive(true);
                        tooltipLetterImage.tooltipHeader = data.imageTooltipHeader;
                    }
                    else { tooltipLetterImage.tooltipHeader = ""; }
                    if (string.IsNullOrEmpty(data.imageTooltipMain) == false)
                    { tooltipLetterImage.tooltipMain = data.imageTooltipMain; }
                    else { tooltipLetterImage.tooltipMain = ""; }
                    if (string.IsNullOrEmpty(data.imageTooltipDetails) == false)
                    { tooltipLetterImage.tooltipDetails = data.imageTooltipDetails; }
                    else { tooltipLetterImage.tooltipDetails = ""; }
                    break;
                default: Debug.LogWarningFormat("Invalid data.uiType \"{0}\"", data.uiType); break;
            }
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
                            arrayOfOptions[index, i] = option;
                            //initialise option text
                            if (string.IsNullOrEmpty(option.textToDisplay) == false)
                            { arrayOfOptionTexts[index, i].text = option.textToDisplay; }
                            else
                            {
                                arrayOfOptionTexts[index, i].text = "Unknown";
                                Debug.LogWarningFormat("Invalid optionText (Null or Empty) for arrayOfOptionTexts[{0}], topic \"{1}\"", i, data.topicName);
                            }
                            //initialise option tooltip
                            if (string.IsNullOrEmpty(option.tooltipHeader) == false)
                            {
                                arrayOfTooltips[index, i].gameObject.SetActive(true);
                                arrayOfTooltips[index, i].tooltipHeader = option.tooltipHeader;
                            }
                            if (string.IsNullOrEmpty(option.tooltipMain) == false)
                            { arrayOfTooltips[index, i].tooltipMain = option.tooltipMain; }
                            if (string.IsNullOrEmpty(option.tooltipDetails) == false)
                            { arrayOfTooltips[index, i].tooltipDetails = option.tooltipDetails; }
                            //need to offset to prevent obscuring part of button and suffering 'blinking tooltip' syndrome
                            arrayOfTooltips[index, i].x_offset = -550;
                            //button Interaction
                            arrayOfButtonInteractions[index, i].SetButton(EventType.TopicDisplayOption, i);
                            //button sprite (yellow bar to match yellow text for valid, grey all for invalid)
                            if (option.isValid == true) { arrayOfButtons[index, i].image.sprite = topicOptionNormalValid; }
                            else { arrayOfButtons[index, i].image.sprite = topicOptionNormalInvalid; }
                            //initialise option
                            arrayOfButtons[index, i].gameObject.SetActive(true);
                        }
                        else
                        { Debug.LogWarningFormat("Invalid option (Null) in listOfOptions[{0}] for topic \"{1}\"", i, data.topicName); }
                    }
                }
                else { Debug.LogWarningFormat("Invalid listOfOptions (Empty) for topic \"{0}\"", data.topicName); }
            }
            else { Debug.LogWarningFormat("Invalid listOfOptions (Null) for topic \"{0}\"", data.topicName); }
            //show Me button
            if (dataPackage.nodeID > -1)
            {
                buttonNormalShowMe.gameObject.SetActive(true);
                normalInteractiveShowMe.SetButton(EventType.TopicDisplayShowMe, -1);
            }
            else { buttonNormalShowMe.gameObject.SetActive(false); }
            //optional second help icon
            if (data.listOfHelp != null && data.listOfHelp.Count > 0)
            {
                buttonNormalHelp_specific.gameObject.SetActive(true);
                if (helpNormalSpecific != null)
                {
                    List<HelpData> listOfHelpData = GameManager.i.helpScript.GetHelpData(data.listOfHelp);
                    if (listOfHelpData != null)
                    { helpNormalSpecific.SetHelpTooltip(listOfHelpData, 150, 180); }
                    else { buttonNormalHelp_specific.gameObject.SetActive(false); }
                }
                else { Debug.LogWarning("Invalid GenericHelpTooltipUI for helpSpecific (Null)"); }
            }
            else { buttonNormalHelp_specific.gameObject.SetActive(false); }
            //initialise ignore Button tooltip
            if (string.IsNullOrEmpty(data.ignoreTooltipHeader) == false)
            {
                tooltipNormalIgnore.gameObject.SetActive(true);
                tooltipNormalIgnore.tooltipHeader = data.ignoreTooltipHeader;
            }
            else { tooltipNormalIgnore.tooltipHeader = ""; }
            if (string.IsNullOrEmpty(data.ignoreTooltipMain) == false)
            { tooltipNormalIgnore.tooltipMain = data.ignoreTooltipMain; }
            else { tooltipNormalIgnore.tooltipMain = ""; }
            if (string.IsNullOrEmpty(data.ignoreTooltipDetails) == false)
            { tooltipNormalIgnore.tooltipDetails = data.ignoreTooltipDetails; }
            else { tooltipNormalIgnore.tooltipDetails = ""; }
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

    #region SetTopicDisplayOther
    /// <summary>
    /// display topicUI in letter format (story Bravo topics -> family)
    /// </summary>
    /// <param name="data"></param>
    private void SetTopicDisplayOther(TopicUIData data)
    {
        if (data != null)
        {
            int index = (int)TopicBase.Other;
            //deactivate all options
            for (int i = 0; i < arrayOfButtons.GetUpperBound(1) + 1; i++)
            { arrayOfButtons[index, i].gameObject.SetActive(false); }
            //texts
            otherText.gameObject.SetActive(true);
            //topic text
            if (string.IsNullOrEmpty(data.text) == false)
            { otherText.text = data.text; }
            else
            {
                Debug.LogWarningFormat("Invalid data.text (Null or Empty) for topic \"{0}\"", data.topicName);
                textHeader.text = "Unknown";
            }
            //topic sprite
            if (data.spriteMain != null)
            { imageTopicOther.sprite = data.spriteMain; }
            else
            {
                Debug.LogWarningFormat("Invalid data.sprite (Null or Empty) for topic \"{0}\"", data.topicName);
                //use default sprite
                imageTopicOther.sprite = GameManager.i.guiScript.topicDefaultSprite;
            }
            //story Help
            if (data.listOfStoryHelp != null)
            {
                switch (data.listOfStoryHelp.Count)
                {
                    case 0: buttonOtherStoryHelp0.gameObject.SetActive(false); buttonOtherStoryHelp1.gameObject.SetActive(false); break;
                    case 1:
                        buttonOtherStoryHelp0.gameObject.SetActive(true);
                        buttonOtherStoryHelp1.gameObject.SetActive(false);
                        helpOtherStory0.SetHelpTooltip(data.listOfStoryHelp[0]);
                        break;
                    case 2:
                        buttonOtherStoryHelp0.gameObject.SetActive(true);
                        buttonOtherStoryHelp1.gameObject.SetActive(true);
                        helpOtherStory0.SetHelpTooltip(data.listOfStoryHelp[0]);
                        helpOtherStory1.SetHelpTooltip(data.listOfStoryHelp[1]);
                        break;
                }
            }
            else { buttonOtherStoryHelp0.gameObject.SetActive(false); buttonOtherStoryHelp1.gameObject.SetActive(false); }
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
                            arrayOfOptions[index, i] = option;
                            //initialise option text
                            if (string.IsNullOrEmpty(option.textToDisplay) == false)
                            { arrayOfOptionTexts[index, i].text = option.textToDisplay; }
                            else
                            {
                                arrayOfOptionTexts[index, i].text = "Unknown";
                                Debug.LogWarningFormat("Invalid optionText (Null or Empty) for arrayOfOptionTexts[{0}], topic \"{1}\"", i, data.topicName);
                            }
                            //initialise option tooltip
                            if (string.IsNullOrEmpty(option.tooltipHeader) == false)
                            {
                                arrayOfTooltips[index, i].gameObject.SetActive(true);
                                arrayOfTooltips[index, i].tooltipHeader = option.tooltipHeader;
                            }
                            if (string.IsNullOrEmpty(option.tooltipMain) == false)
                            { arrayOfTooltips[index, i].tooltipMain = option.tooltipMain; }
                            if (string.IsNullOrEmpty(option.tooltipDetails) == false)
                            { arrayOfTooltips[index, i].tooltipDetails = option.tooltipDetails; }
                            //need to offset to prevent obscuring part of button and suffering 'blinking tooltip' syndrome
                            arrayOfTooltips[index, i].x_offset = -550;
                            //button Interaction
                            arrayOfButtonInteractions[index, i].SetButton(EventType.TopicDisplayOption, i);
                            //button sprite (yellow bar to match yellow text for valid, grey all for invalid)
                            if (option.isValid == true) { arrayOfButtons[index, i].image.sprite = topicOptionOtherValid; }
                            else { arrayOfButtons[index, i].image.sprite = topicOptionOtherInvalid; }
                            //initialise option
                            arrayOfButtons[index, i].gameObject.SetActive(true);
                        }
                        else
                        { Debug.LogWarningFormat("Invalid option (Null) in listOfOptions[{0}] for topic \"{1}\"", i, data.topicName); }
                    }
                }
                else { Debug.LogWarningFormat("Invalid listOfOptions (Empty) for topic \"{0}\"", data.topicName); }
            }
            else { Debug.LogWarningFormat("Invalid listOfOptions (Null) for topic \"{0}\"", data.topicName); }
            //show Me button
            if (dataPackage.nodeID > -1)
            {
                buttonOtherShowMe.gameObject.SetActive(true);
                otherInteractiveShowMe.SetButton(EventType.TopicDisplayShowMe, -1);
            }
            else { buttonOtherShowMe.gameObject.SetActive(false); }
            //optional second help icon
            if (data.listOfHelp != null && data.listOfHelp.Count > 0)
            {
                buttonOtherHelp_specific.gameObject.SetActive(true);
                if (helpOtherSpecific != null)
                {
                    List<HelpData> listOfHelpData = GameManager.i.helpScript.GetHelpData(data.listOfHelp);
                    if (listOfHelpData != null)
                    { helpOtherSpecific.SetHelpTooltip(listOfHelpData, 150, 180); }
                    else { buttonOtherHelp_specific.gameObject.SetActive(false); }
                }
                else { Debug.LogWarning("Invalid GenericHelpTooltipUI for helpSpecific (Null)"); }
            }
            else { buttonOtherHelp_specific.gameObject.SetActive(false); }
            //initialise Image tooltip
            if (string.IsNullOrEmpty(data.imageTooltipHeader) == false)
            {
                tooltipOtherImage.gameObject.SetActive(true);
                tooltipOtherImage.tooltipHeader = data.imageTooltipHeader;
            }
            else { tooltipOtherImage.tooltipHeader = ""; }
            if (string.IsNullOrEmpty(data.imageTooltipMain) == false)
            { tooltipOtherImage.tooltipMain = data.imageTooltipMain; }
            else { tooltipOtherImage.tooltipMain = ""; }
            if (string.IsNullOrEmpty(data.imageTooltipDetails) == false)
            { tooltipOtherImage.tooltipDetails = data.imageTooltipDetails; }
            else { tooltipOtherImage.tooltipDetails = ""; }
            //initialise ignore Button tooltip
            if (string.IsNullOrEmpty(data.ignoreTooltipHeader) == false)
            {
                tooltipOtherIgnore.gameObject.SetActive(true);
                tooltipOtherIgnore.tooltipHeader = data.ignoreTooltipHeader;
            }
            else { tooltipOtherIgnore.tooltipHeader = ""; }
            if (string.IsNullOrEmpty(data.ignoreTooltipMain) == false)
            { tooltipOtherIgnore.tooltipMain = data.ignoreTooltipMain; }
            else { tooltipOtherIgnore.tooltipMain = ""; }
            if (string.IsNullOrEmpty(data.ignoreTooltipDetails) == false)
            { tooltipOtherIgnore.tooltipDetails = data.ignoreTooltipDetails; }
            else { tooltipOtherIgnore.tooltipDetails = ""; }
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
