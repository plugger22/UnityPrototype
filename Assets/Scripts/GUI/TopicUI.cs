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
    public Image innerBackgroundLetter;

    [Header("Panels")]
    public Image panelBoss;

    [Header("Buttons")]
    public Button optionNormal0;
    public Button optionNormal1;
    public Button optionNormal2;
    public Button optionNormal3;
    public Button optionLetter0;
    public Button optionLetter1;
    public Button optionLetter2;
    public Button optionLetter3;
    public Button buttonNormalIgnore;
    public Button buttonNormalShowMe;
    public Button buttonNormalHelp_generic;
    public Button buttonNormalHelp_specific;
    public Button buttonNormalStoryHelp0;
    public Button buttonNormalStoryHelp1;
    public Button buttonLetterIgnore;
    public Button buttonLetterShowMe;
    public Button buttonLetterHelp_generic;
    public Button buttonLetterHelp_specific;
    public Button buttonLetterStoryHelp0;
    public Button buttonLetterStoryHelp1;

    [Header("Texts")]
    public TextMeshProUGUI textHeader;
    public TextMeshProUGUI textMain;
    public TextMeshProUGUI letterText;
    public TextMeshProUGUI textOptionNormal0;
    public TextMeshProUGUI textOptionNormal1;
    public TextMeshProUGUI textOptionNormal2;
    public TextMeshProUGUI textOptionNormal3;
    public TextMeshProUGUI textOptionLetter0;
    public TextMeshProUGUI textOptionLetter1;
    public TextMeshProUGUI textOptionLetter2;
    public TextMeshProUGUI textOptionLetter3;

    [Header("Sprite")]
    public Image imageTopic;
    public Image imageBoss;

    //button script handlers
    private ButtonInteraction normalInteractiveOption0;
    private ButtonInteraction normalInteractiveOption1;
    private ButtonInteraction normalInteractiveOption2;
    private ButtonInteraction normalInteractiveOption3;
    private ButtonInteraction normalInteractiveIgnore;
    private ButtonInteraction normalInteractiveShowMe;
    private ButtonInteraction letterInteractiveOption0;
    private ButtonInteraction letterInteractiveOption1;
    private ButtonInteraction letterInteractiveOption2;
    private ButtonInteraction letterInteractiveOption3;
    private ButtonInteraction letterInteractiveIgnore;
    private ButtonInteraction letterInteractiveShowMe;
    //tooltips
    private GenericTooltipUI tooltipNormalOption0;
    private GenericTooltipUI tooltipNormalOption1;
    private GenericTooltipUI tooltipNormalOption2;
    private GenericTooltipUI tooltipNormalOption3;
    private GenericTooltipUI tooltipNormalIgnore;
    private GenericTooltipUI tooltipNormalShowMe;
    private GenericTooltipUI tooltipNormalImage;
    private GenericTooltipUI tooltipNormalBoss;
    private GenericTooltipUI tooltipLetterOption0;
    private GenericTooltipUI tooltipLetterOption1;
    private GenericTooltipUI tooltipLetterOption2;
    private GenericTooltipUI tooltipLetterOption3;
    private GenericTooltipUI tooltipLetterIgnore;
    private GenericTooltipUI tooltipLetterShowMe;
    private GenericTooltipUI tooltipLetterImage;
    private GenericTooltipUI tooltipLetterBoss;
    //help
    private GenericHelpTooltipUI helpNormalSpecific;
    private StoryHelpTooltipUI helpNormalStory0;
    private StoryHelpTooltipUI helpNormalStory1;
    private GenericHelpTooltipUI helpLetterSpecific;
    private StoryHelpTooltipUI helpLetterStory0;
    private StoryHelpTooltipUI helpLetterStory1;
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
        Debug.Assert(optionNormal0 != null, "Invalid optionNormal0 (Null)");
        Debug.Assert(optionNormal1 != null, "Invalid optionNormal1 (Null)");
        Debug.Assert(optionNormal2 != null, "Invalid optionNormal2 (Null)");
        Debug.Assert(optionNormal3 != null, "Invalid optionNormal3 (Null)");
        Debug.Assert(optionLetter0 != null, "Invalid optionLetter0 (Null)");
        Debug.Assert(optionLetter1 != null, "Invalid optionLetter1 (Null)");
        Debug.Assert(optionLetter2 != null, "Invalid optionLetter2 (Null)");
        Debug.Assert(optionLetter3 != null, "Invalid optionLetter3 (Null)");
        Debug.Assert(buttonNormalIgnore != null, "Invalid buttonNormalIgnore (Null)");
        Debug.Assert(buttonNormalShowMe != null, "Invalid buttonNormalShowMe (Null)");
        Debug.Assert(buttonNormalHelp_generic != null, "Invalid buttonNormalHelp_generic (Null)");
        Debug.Assert(buttonNormalHelp_specific != null, "Invalid buttonNormalHelp_specific (Null)");
        Debug.Assert(buttonNormalStoryHelp0 != null, "Invalid buttonNormalStoryHelp0 (Null)");
        Debug.Assert(buttonNormalStoryHelp1 != null, "Invalid buttonNormalStoryHelp1 (Null)");
        Debug.Assert(buttonLetterIgnore != null, "Invalid buttonLetterIgnore (Null)");
        Debug.Assert(buttonLetterShowMe != null, "Invalid buttonLetterShowMe (Null)");
        Debug.Assert(buttonLetterHelp_generic != null, "Invalid buttonLetterHelp_generic (Null)");
        Debug.Assert(buttonLetterHelp_specific != null, "Invalid buttonLetterHelp_specific (Null)");
        Debug.Assert(buttonLetterStoryHelp0 != null, "Invalid buttonLetterStoryHelp0 (Null)");
        Debug.Assert(buttonLetterStoryHelp1 != null, "Invalid buttonLetterStoryHelp1 (Null)");
        Debug.Assert(textHeader != null, "Invalid textHeader (Null)");
        Debug.Assert(textMain != null, "Invalid textMain (Null)");
        Debug.Assert(letterText != null, "Invalid letterText (Null)");
        Debug.Assert(textOptionNormal0 != null, "Invalid textOptionNormal0 (Null)");
        Debug.Assert(textOptionNormal1 != null, "Invalid textOptionNormal1 (Null)");
        Debug.Assert(textOptionNormal2 != null, "Invalid textOptionNormal2 (Null)");
        Debug.Assert(textOptionNormal3 != null, "Invalid textOptionNormal3 (Null)");
        Debug.Assert(textOptionLetter0 != null, "Invalid textOptionLetter0 (Null)");
        Debug.Assert(textOptionLetter1 != null, "Invalid textOptionLetter1 (Null)");
        Debug.Assert(textOptionLetter2 != null, "Invalid textOptionLetter2 (Null)");
        Debug.Assert(textOptionLetter3 != null, "Invalid textOptionLetter3 (Null)");
        Debug.Assert(imageTopic != null, "Invalid imageTopic (Null)");
        Debug.Assert(imageBoss != null, "Invalid imageBoss (Null)");
        Debug.Assert(outerBackground != null, "Invalid outerBackgroundImage (Null)");
        Debug.Assert(innerBackgroundNormal != null, "Invalid innerBackgroundNormal (Null)");
        Debug.Assert(innerBackgroundLetter != null, "Invalid innerBackgroundLetter (Null)");
        //Button Interactive
        normalInteractiveOption0 = optionNormal0.GetComponent<ButtonInteraction>();
        normalInteractiveOption1 = optionNormal1.GetComponent<ButtonInteraction>();
        normalInteractiveOption2 = optionNormal2.GetComponent<ButtonInteraction>();
        normalInteractiveOption3 = optionNormal3.GetComponent<ButtonInteraction>();
        normalInteractiveIgnore = buttonNormalIgnore.GetComponent<ButtonInteraction>();
        normalInteractiveShowMe = buttonNormalShowMe.GetComponent<ButtonInteraction>();
        letterInteractiveOption0 = optionLetter0.GetComponent<ButtonInteraction>();
        letterInteractiveOption1 = optionLetter1.GetComponent<ButtonInteraction>();
        letterInteractiveOption2 = optionLetter2.GetComponent<ButtonInteraction>();
        letterInteractiveOption3 = optionLetter3.GetComponent<ButtonInteraction>();
        letterInteractiveIgnore = buttonNormalIgnore.GetComponent<ButtonInteraction>();
        letterInteractiveShowMe = buttonNormalShowMe.GetComponent<ButtonInteraction>();
        Debug.Assert(normalInteractiveOption0 != null, "Invalid normalInteractiveOption0 (Null)");
        Debug.Assert(normalInteractiveOption1 != null, "Invalid normalInteractiveOption1 (Null)");
        Debug.Assert(normalInteractiveOption2 != null, "Invalid normalInteractiveOption2 (Null)");
        Debug.Assert(normalInteractiveOption3 != null, "Invalid normalInteractiveOption3 (Null)");
        Debug.Assert(normalInteractiveIgnore != null, "Invalid normalInteractiveIgnore (Null)");
        Debug.Assert(normalInteractiveShowMe != null, "Invalid normalInteractiveShowMe (Null)");
        Debug.Assert(letterInteractiveOption0 != null, "Invalid letterInteractiveOption0 (Null)");
        Debug.Assert(letterInteractiveOption1 != null, "Invalid letterInteractiveOption1 (Null)");
        Debug.Assert(letterInteractiveOption2 != null, "Invalid letterInteractiveOption2 (Null)");
        Debug.Assert(letterInteractiveOption3 != null, "Invalid letterInteractiveOption3 (Null)");
        Debug.Assert(letterInteractiveIgnore != null, "Invalid letterInteractiveIgnore (Null)");
        Debug.Assert(letterInteractiveShowMe != null, "Invalid letterInteractiveShowMe (Null)");
        //Button events
        normalInteractiveOption0?.SetButton(EventType.TopicDisplayOption);
        normalInteractiveOption1?.SetButton(EventType.TopicDisplayOption);
        normalInteractiveOption2?.SetButton(EventType.TopicDisplayOption);
        normalInteractiveOption3?.SetButton(EventType.TopicDisplayOption);
        normalInteractiveIgnore?.SetButton(EventType.TopicDisplayIgnore);
        letterInteractiveOption0?.SetButton(EventType.TopicDisplayOption);
        letterInteractiveOption1?.SetButton(EventType.TopicDisplayOption);
        letterInteractiveOption2?.SetButton(EventType.TopicDisplayOption);
        letterInteractiveOption3?.SetButton(EventType.TopicDisplayOption);
        letterInteractiveIgnore?.SetButton(EventType.TopicDisplayIgnore);
        //Tooltips
        tooltipNormalOption0 = optionNormal0.GetComponent<GenericTooltipUI>();
        tooltipNormalOption1 = optionNormal1.GetComponent<GenericTooltipUI>();
        tooltipNormalOption2 = optionNormal2.GetComponent<GenericTooltipUI>();
        tooltipNormalOption3 = optionNormal3.GetComponent<GenericTooltipUI>();
        tooltipNormalIgnore = buttonNormalIgnore.GetComponent<GenericTooltipUI>();
        tooltipNormalShowMe = buttonNormalShowMe.GetComponent<GenericTooltipUI>();
        tooltipNormalImage = imageTopic.GetComponent<GenericTooltipUI>();
        tooltipNormalBoss = imageBoss.GetComponent<GenericTooltipUI>();
        tooltipLetterOption0 = optionLetter0.GetComponent<GenericTooltipUI>();
        tooltipLetterOption1 = optionLetter1.GetComponent<GenericTooltipUI>();
        tooltipLetterOption2 = optionLetter2.GetComponent<GenericTooltipUI>();
        tooltipLetterOption3 = optionLetter3.GetComponent<GenericTooltipUI>();
        tooltipLetterIgnore = buttonLetterIgnore.GetComponent<GenericTooltipUI>();
        tooltipLetterShowMe = buttonLetterShowMe.GetComponent<GenericTooltipUI>();
        tooltipLetterImage = imageTopic.GetComponent<GenericTooltipUI>();
        tooltipLetterBoss = imageBoss.GetComponent<GenericTooltipUI>();
        Debug.Assert(tooltipNormalOption0 != null, "Invalid tooltipNormalOption0 (Null)");
        Debug.Assert(tooltipNormalOption1 != null, "Invalid tooltipNormalOption1 (Null)");
        Debug.Assert(tooltipNormalOption2 != null, "Invalid tooltipNormalOption2 (Null)");
        Debug.Assert(tooltipNormalOption3 != null, "Invalid tooltipNormalOption3 (Null)");
        Debug.Assert(tooltipNormalIgnore != null, "Invalid tooltipNormalIgnore (Null)");
        Debug.Assert(tooltipNormalShowMe != null, "Invalid tooltipNormalShowMe (Null)");
        Debug.Assert(tooltipNormalImage != null, "Invalid tooltipNormalImage (Null)");
        Debug.Assert(tooltipNormalBoss != null, "Invalid tooltipNormalBoss (Null)");
        Debug.Assert(tooltipLetterOption0 != null, "Invalid tooltipLetterOption0 (Null)");
        Debug.Assert(tooltipLetterOption1 != null, "Invalid tooltipLetterOption1 (Null)");
        Debug.Assert(tooltipLetterOption2 != null, "Invalid tooltipLetterOption2 (Null)");
        Debug.Assert(tooltipLetterOption3 != null, "Invalid tooltipLetterOption3 (Null)");
        Debug.Assert(tooltipLetterIgnore != null, "Invalid tooltipLetterIgnore (Null)");
        Debug.Assert(tooltipLetterShowMe != null, "Invalid tooltipLetterShowMe (Null)");
        Debug.Assert(tooltipLetterImage != null, "Invalid tooltipLetterImage (Null)");
        Debug.Assert(tooltipLetterBoss != null, "Invalid tooltipLetterBoss (Null)");
        //populate arrayOfButtons
        int indexNormal = (int)TopicDecisionType.Normal;
        int indexLetter = (int)TopicDecisionType.Letter;
        arrayOfButtons[indexNormal, 0] = optionNormal0;
        arrayOfButtons[indexNormal, 1] = optionNormal1;
        arrayOfButtons[indexNormal, 2] = optionNormal2;
        arrayOfButtons[indexNormal, 3] = optionNormal3;
        arrayOfButtons[indexLetter, 0] = optionLetter0;
        arrayOfButtons[indexLetter, 1] = optionLetter1;
        arrayOfButtons[indexLetter, 2] = optionLetter2;
        arrayOfButtons[indexLetter, 3] = optionLetter3;
        //populate arrayOfButtonInteractions
        arrayOfButtonInteractions[indexNormal, 0] = normalInteractiveOption0;
        arrayOfButtonInteractions[indexNormal, 1] = normalInteractiveOption1;
        arrayOfButtonInteractions[indexNormal, 2] = normalInteractiveOption2;
        arrayOfButtonInteractions[indexNormal, 3] = normalInteractiveOption3;
        arrayOfButtonInteractions[indexLetter, 0] = letterInteractiveOption0;
        arrayOfButtonInteractions[indexLetter, 1] = letterInteractiveOption1;
        arrayOfButtonInteractions[indexLetter, 2] = letterInteractiveOption2;
        arrayOfButtonInteractions[indexLetter, 3] = letterInteractiveOption3;
        //populate arrayOfOptionTexts
        arrayOfOptionTexts[indexNormal, 0] = textOptionNormal0;
        arrayOfOptionTexts[indexNormal, 1] = textOptionNormal1;
        arrayOfOptionTexts[indexNormal, 2] = textOptionNormal2;
        arrayOfOptionTexts[indexNormal, 3] = textOptionNormal3;
        arrayOfOptionTexts[indexLetter, 0] = textOptionLetter0;
        arrayOfOptionTexts[indexLetter, 1] = textOptionLetter1;
        arrayOfOptionTexts[indexLetter, 2] = textOptionLetter2;
        arrayOfOptionTexts[indexLetter, 3] = textOptionLetter3;
        //populate arrayOfTooltips
        arrayOfTooltips[indexNormal, 0] = tooltipNormalOption0;
        arrayOfTooltips[indexNormal, 1] = tooltipNormalOption1;
        arrayOfTooltips[indexNormal, 2] = tooltipNormalOption2;
        arrayOfTooltips[indexNormal, 3] = tooltipNormalOption3;
        arrayOfTooltips[indexLetter, 0] = tooltipNormalOption0;
        arrayOfTooltips[indexLetter, 1] = tooltipNormalOption1;
        arrayOfTooltips[indexLetter, 2] = tooltipNormalOption2;
        arrayOfTooltips[indexLetter, 3] = tooltipNormalOption3;

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
        //help
        List<HelpData> listOfHelp = GameManager.i.helpScript.GetHelpData("topicUI_0", "topicUI_1", "topicUI_2", "topicUI_3");
        if (listOfHelp != null && listOfHelp.Count > 0)
        {
            //generic help
            GenericHelpTooltipUI helpGeneric = buttonNormalHelp_generic.GetComponent<GenericHelpTooltipUI>();
            if (helpGeneric != null)
            { helpGeneric.SetHelpTooltip(listOfHelp, 150, 200); }
            else { Debug.LogWarning("Invalid GenericHelpTooltipUI for helpGeneric (Null)"); }
            //specific help (don't populate help, just get component)
            helpNormalSpecific = buttonNormalHelp_specific.GetComponent<GenericHelpTooltipUI>();
            if (helpNormalSpecific == null)
            { Debug.LogWarning("Invalid GenericHelpTooltipUI for helpSpecific (Null)"); }
            //story Help
            helpNormalStory0 = buttonNormalStoryHelp0.GetComponent<StoryHelpTooltipUI>();
            helpNormalStory1 = buttonNormalStoryHelp1.GetComponent<StoryHelpTooltipUI>();
            if (helpNormalStory0 == null) { Debug.LogWarning("Invalid StoryHelpTooltipUI for helpStory0 (Null)"); }
            if (helpNormalStory1 == null) { Debug.LogWarning("Invalid StoryHelpTooltipUI for helpStory1 (Null)"); }
        }
        else { Debug.LogWarning("Invalid listOfHelp (Null or Empty)"); }
    }

    /*/// <summary>
    /// Get fixed help data
    /// </summary>
    /// <returns></returns>
    private List<HelpData> GetInfoHelpList()
    { return GetHelpData("info_app_0", "info_app_1", "info_app_2", "info_app_3"); }*/

    /*/// <summary>
    /// Set up TopicUI for the specific TopicType required
    /// </summary>
    /// <param name="type"></param>
    private void SetTopicType(TopicUIData data)
    {
        Color color = outerBackground.color;
        switch (data.type)
        {
            case TopicDecisionType.Normal:
                //hide letter background
                outerBackground.color = new Color(color.r, color.g, color.b, 0.0f);
                //set colour of background
                innerBackgroundNormal.color = new Color(data.colour.r, data.colour.g, data.colour.b, 1.0f);
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
                outerBackground.color = new Color(color.r, color.g, color.b, 1.0f);
                //toggle off inner background using alpha (not gameObject as it switches off the whole shebang)
                innerBackgroundNormal.color = new Color(data.colour.r, data.colour.g, data.colour.b, 0.0f);
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
    }*/

    /// <summary>
    /// Displays relevant topic type, normal or letter style
    /// </summary>
    /// <param name="data"></param>
    private void DisplayTopic(TopicUIData data)
    {
        switch (data.type)
        {
            case TopicDecisionType.Normal:
                innerBackgroundNormal.gameObject.SetActive(true);
                innerBackgroundLetter.gameObject.SetActive(false);
                SetTopicDisplayNormal(data);
                break;
            case TopicDecisionType.Letter:
                innerBackgroundNormal.gameObject.SetActive(false);
                innerBackgroundLetter.gameObject.SetActive(true);
                SetTopicDisplayLetter(data);
                break;
            default: Debug.LogWarningFormat("Unrecognised data.type \"{0}\"", data.type); break;
        }
    }

    #region SetTopicDisplay
    /// <summary>
    /// Initialise topicUI display
    /// </summary>
    /// <param name="data"></param>
    private void SetTopicDisplayNormal(TopicUIData data)
    {
        if (data != null)
        {
            int index = (int)TopicDecisionType.Normal;
            //deactivate all options
            for (int i = 0; i < arrayOfButtons.Length; i++)
            { arrayOfButtons[index, i].gameObject.SetActive(false); }

            //set colour of background
            innerBackgroundNormal.color = new Color(data.colour.r, data.colour.g, data.colour.b, 1.0f);
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
            //ignore button
            normalInteractiveIgnore.SetButton(EventType.TopicDisplayIgnore, -1);
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

    #region SetTopicDisplayLetter
    /// <summary>
    /// display topicUI in letter format (story Bravo topics -> family)
    /// </summary>
    /// <param name="data"></param>
    private void SetTopicDisplayLetter(TopicUIData data)
    {
        if (data != null)
        {
            int index = (int)TopicDecisionType.Letter;
            //deactivate all options
            for (int i = 0; i < arrayOfButtons.Length; i++)
            { arrayOfButtons[index, i].gameObject.SetActive(false); }
            //texts
            letterText.gameObject.SetActive(true);
            //topic text
            if (string.IsNullOrEmpty(data.text) == false)
            { letterText.text = data.text; }
            else
            {
                Debug.LogWarningFormat("Invalid data.text (Null or Empty) for topic \"{0}\"", data.topicName);
                textHeader.text = "Unknown";
            }
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
                            if (option.isValid == true) { arrayOfButtons[index, i].image.sprite = topicOptionLetterValid; }
                            else { arrayOfButtons[index, i].image.sprite = topicOptionLetterInvalid; }
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
            //ignore button
            letterInteractiveIgnore.SetButton(EventType.TopicDisplayIgnore, -1);
            //show Me button
            if (dataPackage.nodeID > -1)
            {
                buttonLetterShowMe.gameObject.SetActive(true);
                letterInteractiveShowMe.SetButton(EventType.TopicDisplayShowMe, -1);
            }
            else { buttonLetterShowMe.gameObject.SetActive(false); }
            //optional second help icon
            if (data.listOfHelp != null && data.listOfHelp.Count > 0)
            {
                buttonLetterHelp_specific.gameObject.SetActive(true);
                if (helpLetterSpecific != null)
                {
                    List<HelpData> listOfHelpData = GameManager.i.helpScript.GetHelpData(data.listOfHelp);
                    if (listOfHelpData != null)
                    { helpLetterSpecific.SetHelpTooltip(listOfHelpData, 150, 180); }
                    else { buttonLetterHelp_specific.gameObject.SetActive(false); }
                }
                else { Debug.LogWarning("Invalid GenericHelpTooltipUI for helpSpecific (Null)"); }
            }
            else { buttonLetterHelp_specific.gameObject.SetActive(false); }
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
            //initialise ignore Button tooltip
            if (string.IsNullOrEmpty(data.ignoreTooltipHeader) == false)
            {
                tooltipLetterIgnore.gameObject.SetActive(true);
                tooltipLetterIgnore.tooltipHeader = data.ignoreTooltipHeader;
            }
            else { tooltipLetterIgnore.tooltipHeader = ""; }
            if (string.IsNullOrEmpty(data.ignoreTooltipMain) == false)
            { tooltipLetterIgnore.tooltipMain = data.ignoreTooltipMain; }
            else { tooltipLetterIgnore.tooltipMain = ""; }
            if (string.IsNullOrEmpty(data.ignoreTooltipDetails) == false)
            { tooltipLetterIgnore.tooltipDetails = data.ignoreTooltipDetails; }
            else { tooltipLetterIgnore.tooltipDetails = ""; }
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
