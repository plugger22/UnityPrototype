using gameAPI;
using modalAPI;
using packageAPI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public Button buttonSpecial;
    public Button buttonBack;
    public Button buttonContinue;
    public Button buttonExit;
    public Button buttonHelpMain;
    public Button buttonHelpPage;

    //button interactions
    private ButtonInteraction buttonInteractionSpecial;
    private ButtonInteraction buttonInteractionBack;
    private ButtonInteraction buttonInteractionContinue;
    private ButtonInteraction buttonInteractionExit;

    //button tooltips
    public GenericTooltipUI tooltipSpecial;

    //button texts
    public TextMeshProUGUI buttonSpecialText;

    //button help
    public GenericHelpTooltipUI helpMain;
    public GenericHelpTooltipUI helpPage;


    //Lists of Help
    List<HelpData> listOfMainHelp = new List<HelpData>();
    List<HelpData> listOfEndLevelHelp = new List<HelpData>();
    List<HelpData> listOfHqHelp = new List<HelpData>();
    List<HelpData> listOfPlayerStatusHelp = new List<HelpData>();
    List<HelpData> listOfBriefingOneHelp = new List<HelpData>();
    List<HelpData> listOfBriefingTwoHelp = new List<HelpData>();

    private int buttonHelpOffset_x = 125;
    private int buttonHelpOffset_y = 100;

    //data package required to populate UI
    private TransitionInfoData transitionInfoData = new TransitionInfoData();

    private ModalTransitionSubState state;

    //colours
    string colourDefault;
    string colourNeutral;
    string colourGrey;
    string colourAlert;
    string colourNormal;
    /*string colourGood;
    string colourBlue;
    string colourError;
    string colourInvalid;*/
    string colourCancel;
    string colourEnd;

    //special colours
    string colourHeader;

    //predefined colours (NOTE: spelt 'color')
    Color colorHeader;
    Color colorBackground;
    Color colorText;

    //static reference
    private static TransitionUI transitionUI;

    #endregion

    #region End Level

    [Header("End Level")]
    public Canvas endLevelCanvas;
    public Image endLevelBackground;
    public Image endLevelPanel;
    public Image endLevelSprite;
    public Image endLevelBar;
    public TextMeshProUGUI endLevelTextLeft;
    public TextMeshProUGUI endLevelTextRight;
    public TextMeshProUGUI renownText;

    public EndLevelInteraction endLevelBoss;
    public EndLevelInteraction endLevelSubBoss1;
    public EndLevelInteraction endLevelSubBoss2;
    public EndLevelInteraction endLevelSubBoss3;

    public GenericTooltipUI tooltipEndRenown;

    //collection
    private EndLevelInteraction[] arrayOfEndLevelOptions;

    #endregion

    #region HQ Status

    [Header("HQ Status")]
    public Canvas hqCanvas;
    public Image hqBackground;
    public Image hierarchyBackground;
    public Image workerBackground;
    public Image hqTextBackground;
    public Image hierarchySprite;
    public Image workerSprite;
    public Image hierarchyBar;
    public Image workerBar;

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
    public TextMeshProUGUI hqBottomText;

    //collections
    private HqInteraction[] arrayOfHqOptions;
    private WorkerInteraction[] arrayOfWorkerOptions;

    //assorted
    private TooltipData renownTooltip;
    private TooltipData specialTooltip;

    //fast access
    private Sprite vacantActorSprite;
    private string vacantActorCompatibility;

    #endregion

    #region Player Status

    [Header("Player Status")]
    public Canvas playerStatusCanvas;
    public Image playerStatusBackground;
    public Image playerImage;
    public Image playerTextBackground;
    public TextMeshProUGUI playerText;
    public TextMeshProUGUI playerImageText;

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
        Debug.Assert(buttonSpecial != null, "Invalid buttonSpecial (Null)");
        Debug.Assert(buttonBack != null, "Invalid buttonBack (Null)");
        Debug.Assert(buttonContinue != null, "Invalid buttonContinue (Null)");
        Debug.Assert(buttonExit != null, "Invalid buttonExit (Null)");
        Debug.Assert(buttonHelpMain != null, "Invalid buttonHelpMain (Null)");
        Debug.Assert(buttonHelpPage != null, "Invalid buttonHelpPage (Null)");
        Debug.Assert(tooltipSpecial != null, "Invalid tooltipSpecial (Null)");
        Debug.Assert(helpMain != null, "Invalid helpMain (Null)");
        Debug.Assert(helpPage != null, "Invalid helpPage (Null)");
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
        buttonInteractionSpecial = buttonSpecial.GetComponent<ButtonInteraction>();
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
        transitionHeader.color = colorHeader;
        //Set starting Initialisation states
        SetColours();
        InitialiseTooltips();
        InitialiseHelp();
        InitialiseMainTexts();
        #endregion

        #region EndLevel
        Debug.Assert(endLevelCanvas != null, "Invalid endlLevelCanvas (Null)");
        Debug.Assert(endLevelBackground != null, "Invalid endlLevelBackground (Null)");
        Debug.Assert(endLevelPanel != null, "Invalid endLevelPanel (Null)");
        Debug.Assert(endLevelBar != null, "Invalid endLevelBar (Null)");
        Debug.Assert(endLevelSprite != null, "Invalid endLevelSprite (Null)");
        Debug.Assert(endLevelTextLeft != null, "Invalid endLevelTextLeft (Null)");
        Debug.Assert(endLevelTextRight != null, "Invalid endLevelTextRight (Null)");
        Debug.Assert(endLevelBoss != null, "Invalid endLevelBoss (Null)");
        Debug.Assert(endLevelSubBoss1 != null, "Invalid endLevelSubBoss1 (Null)");
        Debug.Assert(endLevelSubBoss2 != null, "Invalid endLevelSubBoss2 (Null)");
        Debug.Assert(endLevelSubBoss3 != null, "Invalid endLevelSubBoss3 (Null)");
        Debug.Assert(tooltipEndRenown != null, "Invalid tooltipEndRenown (Null)");
        //collections
        arrayOfEndLevelOptions = new EndLevelInteraction[GameManager.i.hqScript.numOfActorsHQ];
        //populate
        if (arrayOfEndLevelOptions.Length == 4)
        {
            arrayOfEndLevelOptions[0] = endLevelBoss;
            arrayOfEndLevelOptions[1] = endLevelSubBoss1;
            arrayOfEndLevelOptions[2] = endLevelSubBoss2;
            arrayOfEndLevelOptions[3] = endLevelSubBoss3;
        }
        else { Debug.LogErrorFormat("Invalid arrayOfEndLevelOptions.Length \"[0}\"", arrayOfEndLevelOptions.Length); }
        //bar
        colorBackground.a = 1.00f;
        endLevelSprite.color = colorBackground;
        colorBackground.a = 0.75f;
        endLevelBar.color = colorBackground;
        colorBackground.a = 0.50f;
        endLevelPanel.color = colorBackground;
        //bar texts
        endLevelTextLeft.gameObject.SetActive(true);
        endLevelTextRight.gameObject.SetActive(true);
        colorText.a = 0.25f;
        endLevelTextLeft.color = colorText;
        endLevelTextRight.color = colorText;
        //set up options
        colorBackground.a = 0.75f;
        for (int i = 0; i < arrayOfEndLevelOptions.Length; i++)
        {
            EndLevelInteraction option = arrayOfEndLevelOptions[i];
            if (option != null)
            {
                //background colours
                option.assessmentBackground.color = colorBackground;
            }
            else { Debug.LogWarningFormat("Invalid endLevelOption (Null) for arrayOfEndLevelOptions[{0}]", i); }
        }
        #endregion

        #region HQ Status
        //
        // - - - HQ Status
        //
        Debug.Assert(hqBackground != null, "Invalid hqBackground (Null)");
        Debug.Assert(hierarchyBackground != null, "Invalid hierarchyBackground (Null)");
        Debug.Assert(workerBackground != null, "Invalid workerBackground (Null)");
        Debug.Assert(hqTextBackground != null, "Invalid textBackground (Null)");
        Debug.Assert(hierarchySprite != null, "Invalid hierarchySprite (Null)");
        Debug.Assert(workerSprite != null, "Invalid workerSprite (Null)");
        Debug.Assert(hierarchyBar != null, "Invalid hierarchyBar (Null)");
        Debug.Assert(workerBar != null, "Invalid workerBar (Null)");
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
        Debug.Assert(hqBottomText != null, "Invalid mainBottomText (Null)");
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
        //texts and colours
        hierarchyLeft.text = "HIERARCHY";
        hierarchyRight.text = "HIERARCHY";
        workersLeft.text = "WORKERS";
        workersRight.text = "WORKERS";
        colorText.a = 0.25f;
        hierarchyLeft.color = colorText;
        hierarchyRight.color = colorText;
        workersLeft.color = colorText;
        workersRight.color = colorText;
        //lower alpha
        colorBackground.a = 0.50f;
        hierarchyBackground.color = colorBackground;
        hierarchyBackground.gameObject.SetActive(true);
        workerBackground.color = colorBackground;
        workerBackground.gameObject.SetActive(true);
        hqTextBackground.gameObject.SetActive(true);
        colorBackground.a = 0.75f;
        hierarchySprite.color = colorBackground;
        workerSprite.color = colorBackground;
        hierarchyBar.color = colorBackground;
        workerBar.color = colorBackground;
        //renown tooltip
        renownTooltip = new TooltipData()
        {
            header = string.Format("{0}RENOWN{1}", colourCancel, colourEnd),
            main = string.Format("{0}Hierarchy positions are determined by <b>who has the most Renown</b>{1}", colourNormal, colourEnd),
            details = string.Format("{0}If a <b>Worker</b> accumulates more Renown than a member of the Hierarchy they will <b>take their spot</b>{1}", colourNormal, colourEnd)
        };
        //background
        hqBackground.gameObject.SetActive(true);
        #endregion

        #region Player Status
        Debug.Assert(playerStatusCanvas != null, "Invalid playerStatusCanvas (Null)");
        Debug.Assert(playerStatusBackground != null, "Invalid playerStatusBackground (Null)");
        Debug.Assert(playerImage != null, "Invalid playerImage (Null)");
        Debug.Assert(playerTextBackground != null, "Invalid playerTextBackground (Null)");
        Debug.Assert(playerText != null, "Invalid playerText (Null)");
        //background color (transitionHqBackground / 100% alpha
        playerStatusBackground.gameObject.SetActive(true);
        colorBackground.a = 0.50f;
        playerTextBackground.color = colorBackground;
        playerImageText.text = GameManager.i.playerScript.PlayerName;
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
        //colors
        colorHeader = GameManager.i.guiScript.colourTransitionHeader;
        colorBackground = GameManager.i.guiScript.colourTransitionBackground;
        colorText = GameManager.i.guiScript.colourTransitionText;
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
        EventManager.i.AddListener(EventType.TransitionHqEvents, OnEvent, "TransitionUI");
        EventManager.i.AddListener(EventType.TransitionObjectives, OnEvent, "TransitionUI");
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
            case EventType.TransitionHqEvents:
                ExecuteHqEvents();
                break;
            case EventType.TransitionObjectives:
                ExecuteObjectives();
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
        colourNormal = GameManager.i.colourScript.GetColour(ColourType.normalText);
        /*colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodText);
        colourBlue = GameManager.instance.colourScript.GetColour(ColourType.blueText)
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badText);
        colourError = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourInvalid = GameManager.instance.colourScript.GetColour(ColourType.salmonText);*/
        colourCancel = GameManager.i.colourScript.GetColour(ColourType.moccasinText);
        colourEnd = GameManager.i.colourScript.GetEndTag();

        //special colours
        colourHeader = string.Format("<color=#{0}>", ColorUtility.ToHtmlStringRGB(colorHeader));
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
            transitionInfoData = new TransitionInfoData(data);
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
            //
            // - - - End Level
            //
            //clear out options
            for (int i = 0; i < arrayOfEndLevelOptions.Length; i++)
            {
                arrayOfEndLevelOptions[i].hqPortrait.sprite = null;
                arrayOfEndLevelOptions[i].hqTitle.text = "";
                arrayOfEndLevelOptions[i].hqRenown.text = "";
                arrayOfEndLevelOptions[i].medal.sprite = null;
                arrayOfEndLevelOptions[i].assessmentText.text = "";
                arrayOfEndLevelOptions[i].tooltipPortrait.tooltipHeader = "";
                arrayOfEndLevelOptions[i].tooltipPortrait.tooltipMain = "";
                arrayOfEndLevelOptions[i].tooltipPortrait.tooltipDetails = "";
                arrayOfEndLevelOptions[i].tooltipMedal.tooltipHeader = "";
                arrayOfEndLevelOptions[i].tooltipMedal.tooltipMain = "";
                arrayOfEndLevelOptions[i].tooltipMedal.tooltipDetails = "";
            }
            endLevelBackground.gameObject.SetActive(true);
            for (int i = 0; i < arrayOfEndLevelOptions.Length; i++)
            {
                EndLevelInteraction option = arrayOfEndLevelOptions[i];
                if (option != null)
                {
                    //hq Portrait
                    option.hqPortrait.sprite = data.listOfHqSprites[i];
                    option.hqTitle.text = string.Format("{0}{1}{2}", colourHeader, data.listOfHqTitles[i], colourEnd);
                    //EndLevelData
                    EndLevelData endData = data.listOfEndLevelData[i];
                    if (endData != null)
                    {
                        //renown
                        option.hqRenown.text = string.Format("{0}{1}{2}{3}", colourHeader, endData.renown > 0 ? "+" : "", endData.renown, colourEnd);
                        //medal
                        switch (endData.medal)
                        {
                            case EndlLevelMedal.Gold:
                            case EndlLevelMedal.Silver:
                            case EndlLevelMedal.Bronze:
                                option.medal.sprite = GameManager.i.guiScript.medalSprite;
                                break;
                            case EndlLevelMedal.DeadDuck:
                                option.medal.sprite = GameManager.i.guiScript.failureSprite;
                                break;
                            default:
                                Debug.LogWarningFormat("Unrecognised endData.medal \"{0}\"", endData.medal);
                                option.medal.sprite = GameManager.i.guiScript.failureSprite;
                                break;
                        }
                        //stat text
                        option.assessmentText.text = endData.assessmentText;
                        //tooltip Portrait
                        option.tooltipPortrait.tooltipHeader = endData.tooltipPortrait.header;
                        option.tooltipPortrait.tooltipMain = endData.tooltipPortrait.main;
                        option.tooltipPortrait.tooltipDetails = endData.tooltipPortrait.details;
                        option.tooltipPortrait.x_offset = 50;
                        //tooltip Medal
                        option.tooltipMedal.tooltipHeader = endData.tooltipMedal.header;
                        option.tooltipMedal.tooltipMain = endData.tooltipMedal.main;
                        option.tooltipMedal.tooltipDetails = endData.tooltipMedal.details;
                        option.tooltipMedal.x_offset = 50;
                    }
                    else { Debug.LogWarningFormat("Invalid EndLevelData (Null) for listOfEndLevelData[{0}]", i); }
                }
                else { Debug.LogWarningFormat("Invalid arrayOfEndLevelInteraction (Null) for arrayOfEndLevelOptions[{0}]", i); }
            }

            //renown tooltip
            string posRenown = "<pos=70%>";
            int playerRenown = GameManager.i.playerScript.Renown;
            int renownTotal = playerRenown + data.listOfEndLevelData.Select(x => x.renown).Sum();
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0}<align=\"left\">Carried Over</align>{1}{2}{3}{4}{5}{6}", colourNormal, colourEnd, posRenown, colourNeutral, playerRenown, colourEnd, "\n");
            builder.AppendFormat("{0}<align=\"left\">{1}</align>{2}{3}{4}{5}{6}{7}", colourNormal, GameManager.i.hqScript.GetHqTitle(ActorHQ.Boss), colourEnd, posRenown, colourNeutral, 
                data.listOfEndLevelData[(int)ActorHQ.Boss - 1].renown, colourEnd, "\n");
            builder.AppendFormat("{0}<align=\"left\">{1}</align>{2}{3}{4}{5}{6}{7}", colourNormal, GameManager.i.hqScript.GetHqTitle(ActorHQ.SubBoss1), colourEnd, posRenown, colourNeutral,
                data.listOfEndLevelData[(int)ActorHQ.SubBoss1 - 1].renown, colourEnd, "\n");
            builder.AppendFormat("{0}<align=\"left\">{1}</align>{2}{3}{4}{5}{6}{7}", colourNormal, GameManager.i.hqScript.GetHqTitle(ActorHQ.SubBoss2), colourEnd, posRenown, colourNeutral,
                data.listOfEndLevelData[(int)ActorHQ.SubBoss2 - 1].renown, colourEnd, "\n");
            builder.AppendFormat("{0}<align=\"left\">{1}</align>{2}{3}{4}{5}{6}{7}", colourNormal, GameManager.i.hqScript.GetHqTitle(ActorHQ.SubBoss3), colourEnd, posRenown, colourNeutral,
                data.listOfEndLevelData[(int)ActorHQ.SubBoss3 - 1].renown, colourEnd, "\n");
            tooltipEndRenown.tooltipHeader = string.Format("{0}Player Renown{1}", colourAlert, colourEnd);
            tooltipEndRenown.tooltipMain = builder.ToString();
            tooltipEndRenown.tooltipDetails = string.Format("{0}Total Renown{1}{2}{3}{4}{5}", colourNormal, colourEnd, posRenown, colourNeutral, renownTotal, colourEnd);
            tooltipEndRenown.x_offset = -350;
            tooltipEndRenown.y_offset = -50;
            //renown -> update player for renown granted by HQ and update renown display
            renownText.text = renownTotal.ToString();
            GameManager.i.playerScript.Renown = renownTotal;
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
            //tooltip offsets
            int x_offset = 50;
            int y_offset = 100;
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
                arrayOfHqOptions[i].textLower.text = string.Format("{0}{1}{2}", colourHeader, data.listOfHqTitles[i], colourEnd);
                //sprite tooltip
                if (data.listOfHqTooltips[i] != null)
                {
                    arrayOfHqOptions[i].optionTooltip.tooltipHeader = data.listOfHqTooltips[i].header;
                    arrayOfHqOptions[i].optionTooltip.tooltipMain = data.listOfHqTooltips[i].main;
                    arrayOfHqOptions[i].optionTooltip.tooltipDetails = data.listOfHqTooltips[i].details;
                    arrayOfHqOptions[i].optionTooltip.x_offset = x_offset;
                    arrayOfHqOptions[i].optionTooltip.y_offset = y_offset;

                }
                else { Debug.LogWarningFormat("Invalid tooltip (Null) for arrayOfHqOptions[{0}].optionTooltip", i); }
                //renown tooltip
                arrayOfHqOptions[i].renownTooltip.tooltipHeader = renownTooltip.header;
                arrayOfHqOptions[i].renownTooltip.tooltipMain = renownTooltip.main;
                arrayOfHqOptions[i].renownTooltip.tooltipDetails = renownTooltip.details;
                arrayOfHqOptions[i].renownTooltip.x_offset = x_offset;
                arrayOfHqOptions[i].renownTooltip.y_offset = x_offset;     //NOTE: I want x_offset for both here
            }
            //worker options -> populate
            Debug.AssertFormat(data.listOfWorkerSprites.Count == data.listOfWorkerArcs.Count, "Mismatch -> listOfWorkersSprites has {0} records, listOfWorkerNames has {1} records",
                data.listOfWorkerSprites.Count, data.listOfWorkerArcs.Count);

            for (int i = 0; i < data.listOfWorkerSprites.Count; i++)
            {
                arrayOfWorkerOptions[i].optionImage.sprite = data.listOfWorkerSprites[i];
                arrayOfWorkerOptions[i].textUpper.text = data.listOfWorkerRenown[i];
                arrayOfWorkerOptions[i].textLower.text = data.listOfWorkerArcs[i];
                //sprite tooltip
                if (data.listOfWorkerTooltips[i] != null)
                {
                    arrayOfWorkerOptions[i].optionTooltip.tooltipHeader = data.listOfWorkerTooltips[i].header;
                    arrayOfWorkerOptions[i].optionTooltip.tooltipMain = data.listOfWorkerTooltips[i].main;
                    arrayOfWorkerOptions[i].optionTooltip.tooltipDetails = data.listOfWorkerTooltips[i].details;
                    arrayOfWorkerOptions[i].optionTooltip.x_offset = x_offset;
                    arrayOfWorkerOptions[i].optionTooltip.y_offset = y_offset;
                }
                else { Debug.LogWarningFormat("Invalid tooltip (Null) for arrayOfWorkerOptions[{0}].optionTooltip", i); }
                //renown tooltip
                arrayOfWorkerOptions[i].renownTooltip.tooltipHeader = renownTooltip.header;
                arrayOfWorkerOptions[i].renownTooltip.tooltipMain = renownTooltip.main;
                arrayOfWorkerOptions[i].renownTooltip.tooltipDetails = renownTooltip.details;
                arrayOfWorkerOptions[i].renownTooltip.x_offset = x_offset;
                arrayOfWorkerOptions[i].renownTooltip.y_offset = x_offset;   //NOTE: I want x_offset for both here
            }

            //special button tooltip
            int count = transitionInfoData.listOfHqEvents.Count;
            specialTooltip = new TooltipData()
            {
                header = string.Format("{0}HQ Events{1}", colourCancel, colourEnd),
                main = string.Format("There {0} {1}{2}{3} event{4}", count != 1 ? "are" : "is only", colourNeutral, count, colourEnd, count != 1 ? "s" : ""),
                details = string.Format("{0}Click to View{1}", colourAlert, colourEnd)
            };

            //toggle worker options on (EDIT: was for alpha
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
            playerTextBackground.gameObject.SetActive(true);
            playerImage.sprite = GameManager.i.playerScript.sprite;
            playerText.gameObject.SetActive(true);
            //assign status text
            playerText.text = transitionInfoData.playerStatus;
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
        int x_offset = -50;
        int y_offset = 50;
        //Special Button
        tooltipSpecial.x_offset = x_offset;
        tooltipSpecial.y_offset = y_offset;
    }

    /// <summary>
    /// Initialise Help
    /// </summary>
    private void InitialiseHelp()
    {
        listOfEndLevelHelp = GameManager.i.helpScript.GetHelpData("transitionEnd_0", "transitionEnd_1", "transitionEnd_2", "transitionEnd_3");
        listOfHqHelp = GameManager.i.helpScript.GetHelpData("transitionHq_3", "transitionHq_4", "transitionHq_5", "transitionHq_6");
        listOfPlayerStatusHelp = GameManager.i.helpScript.GetHelpData("transitionPlayer_0", "transitionPlayer_1");
        listOfBriefingOneHelp = GameManager.i.helpScript.GetHelpData("test0");
        listOfBriefingTwoHelp = GameManager.i.helpScript.GetHelpData("test0");
        //main help button -> default
        listOfMainHelp = GameManager.i.helpScript.GetHelpData("transitionMain_0");
        if (listOfMainHelp != null)
        { helpMain.SetHelpTooltip(listOfMainHelp, buttonHelpOffset_x, buttonHelpOffset_y); }
        else { Debug.LogWarning("Invalid listOfMainHelp for helpMain (Null)"); }
    }

    /// <summary>
    /// Initialise Main texts on the various screens (Instructional texts)
    /// </summary>
    private void InitialiseMainTexts()
    {
        StringBuilder builder = new StringBuilder();
        //HQ Status
        builder.Clear();
        builder.AppendFormat("<pos=35%>{0}Mouse Over{1}{2} Portraits for details{3}{4}", colourAlert, colourEnd, colourNormal, colourEnd, "\n");
        builder.AppendFormat("<pos=35%>{0}Left Click{1}{2} Portraits for more information{3}{4}", colourAlert, colourEnd, colourNormal, colourEnd, "\n");
        builder.AppendFormat("<pos=35%>{0}SPECIAL{1}{2} button for a Summary of HQ Events{3}", colourAlert, colourEnd, colourNormal, colourEnd);
        hqBottomText.text = builder.ToString();
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
                buttonInteractionSpecial.SetButton(EventType.TransitionObjectives);
                buttonSpecialText.text = "OBJECTIVES";
                buttonSpecial.gameObject.SetActive(true);
                buttonBack.gameObject.SetActive(false);
                buttonContinue.gameObject.SetActive(true);
                buttonExit.gameObject.SetActive(false);
                break;
            case ModalTransitionSubState.HQ:
                buttonInteractionSpecial.SetButton(EventType.TransitionHqEvents);
                buttonSpecialText.text = "HQ Events";
                buttonSpecial.gameObject.SetActive(true);
                buttonBack.gameObject.SetActive(true);
                buttonContinue.gameObject.SetActive(true);
                buttonExit.gameObject.SetActive(false);
                break;
            case ModalTransitionSubState.PlayerStatus:
                buttonInteractionSpecial.SetButton(EventType.None);
                buttonSpecial.gameObject.SetActive(false);
                buttonBack.gameObject.SetActive(true);
                buttonContinue.gameObject.SetActive(true);
                buttonExit.gameObject.SetActive(false);
                break;
            case ModalTransitionSubState.BriefingOne:
                buttonInteractionSpecial.SetButton(EventType.None);
                buttonSpecial.gameObject.SetActive(false);
                buttonBack.gameObject.SetActive(true);
                buttonContinue.gameObject.SetActive(true);
                buttonExit.gameObject.SetActive(false);
                break;
            case ModalTransitionSubState.BriefingTwo:
                buttonInteractionSpecial.SetButton(EventType.None);
                buttonSpecial.gameObject.SetActive(false);
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
            case ModalTransitionSubState.EndLevel: transitionHeader.text = string.Format("{0}Mission Review{1}", colourHeader, colourEnd); break;
            case ModalTransitionSubState.HQ: transitionHeader.text = string.Format("{0}HQ Status{1}", colourHeader, colourEnd); break;
            case ModalTransitionSubState.PlayerStatus: transitionHeader.text = string.Format("{0}Current Status{1}", colourHeader, colourEnd); break;
            case ModalTransitionSubState.BriefingOne: transitionHeader.text = string.Format("{0}Briefing One{1}", colourHeader, colourEnd); break;
            case ModalTransitionSubState.BriefingTwo: transitionHeader.text = string.Format("{0}Briefing Two{1}", colourHeader, colourEnd); break;
            default: Debug.LogWarningFormat("Unrecognised ModalTransitionState \"{0}\"", GameManager.i.inputScript.ModalTransitionState); break;
        }
    }

    /// <summary>
    /// Sets Help button texts (down at main button bar)
    /// </summary>
    /// <param name="state"></param>
    private void SetHelp(ModalTransitionSubState state)
    {
        List<HelpData> listOfHelp = null;
        int x_offset = buttonHelpOffset_x;
        int y_offset = buttonHelpOffset_y;
        switch (state)
        {
            case ModalTransitionSubState.EndLevel:
                listOfHelp = listOfEndLevelHelp;
                break;
            case ModalTransitionSubState.HQ:
                listOfHelp = listOfHqHelp;
                break;
            case ModalTransitionSubState.PlayerStatus:
                listOfHelp = listOfPlayerStatusHelp;
                break;
            case ModalTransitionSubState.BriefingOne:
                listOfHelp = listOfBriefingOneHelp;
                break;
            case ModalTransitionSubState.BriefingTwo:
                listOfHelp = listOfBriefingTwoHelp;
                break;
            default:
                Debug.LogWarningFormat("Unrecognised ModalTransitionState \"{0}\"", GameManager.i.inputScript.ModalTransitionState);
                listOfHelp = GameManager.i.helpScript.GetHelpData("test0");
                break;
        }
        //page help button -> default
        if (listOfHelp != null)
        { helpPage.SetHelpTooltip(listOfHelp, x_offset, y_offset); }
        else { Debug.LogWarning("Invalid listOfHelp for helpPage (Null)"); }
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
            buttonHelpPage.gameObject.SetActive(true);
            SetStates(startState);
            Debug.LogFormat("[UI] TransitionUI.cs -> TransitionUI{0}", "\n");
        }
        else { Debug.LogWarning("Invalid TranistionInfoData (Null)"); }
    }

    /// <summary>
    /// Sets different states at start up and during page changes
    /// </summary>
    /// <param name="state"></param>
    private void SetStates(ModalTransitionSubState state)
    {
        SetButtons(state);
        SetCanvas(state);
        SetHeader(state);
        SetHelp(state);
    }

    /// <summary>
    /// Close TransitionUI
    /// </summary>
    private void ExecuteClose()
    {
        transitionCanvas.gameObject.SetActive(false);
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
            case ModalTransitionSubState.EndLevel:
                newState = ModalTransitionSubState.HQ;
                //special button tooltip (HQ events)
                tooltipSpecial.tooltipHeader = specialTooltip.header;
                tooltipSpecial.tooltipMain = specialTooltip.main;
                tooltipSpecial.tooltipDetails = specialTooltip.details;
                break;
            case ModalTransitionSubState.HQ:
                newState = ModalTransitionSubState.PlayerStatus;
                ClearSpecialButtonTooltip();
                break;
            case ModalTransitionSubState.PlayerStatus:
                newState = ModalTransitionSubState.BriefingOne;
                ClearSpecialButtonTooltip();
                break;
            case ModalTransitionSubState.BriefingOne:
                newState = ModalTransitionSubState.BriefingTwo;
                ClearSpecialButtonTooltip();
                break;
            case ModalTransitionSubState.BriefingTwo: break; //do nothing
            default: Debug.LogWarningFormat("Unrecognised ModalTransitionState \"{0}\"", GameManager.i.inputScript.ModalTransitionState); break;
        }
        //close tooltips
        GameManager.i.tooltipGenericScript.CloseTooltip("GUIManager.cs -> SetTooltipsOff");
        GameManager.i.tooltipHelpScript.CloseTooltip("GUIManager.cs -> SetTooltipsOff");
        //set new state
        if (newState != ModalTransitionSubState.None)
        {
            GameManager.i.inputScript.SetModalState(new ModalStateData() { mainState = ModalSubState.Transition, transitionState = newState });
            //Adjust UI
            SetStates(newState);
        }
    }

    /// <summary>
    /// Zero out special button tooltip to prevent data carrying over between pages
    /// </summary>
    private void ClearSpecialButtonTooltip()
    {
        tooltipSpecial.tooltipHeader = "";
        tooltipSpecial.tooltipMain = "";
        tooltipSpecial.tooltipDetails = "";
    }

    /// <summary>
    /// Back button pressed
    /// </summary>
    private void ExecuteBack()
    {
        ModalTransitionSubState newState = ModalTransitionSubState.None;
        switch (GameManager.i.inputScript.ModalTransitionState)
        {
            case ModalTransitionSubState.EndLevel: break; //do nothing
            case ModalTransitionSubState.HQ:
                ClearSpecialButtonTooltip();
                newState = ModalTransitionSubState.EndLevel;
                break;
            case ModalTransitionSubState.PlayerStatus:
                newState = ModalTransitionSubState.HQ;
                //special button tooltip (HQ events)
                tooltipSpecial.tooltipHeader = specialTooltip.header;
                tooltipSpecial.tooltipMain = specialTooltip.main;
                tooltipSpecial.tooltipDetails = specialTooltip.details;
                break;
            case ModalTransitionSubState.BriefingOne:
                newState = ModalTransitionSubState.PlayerStatus;
                ClearSpecialButtonTooltip();
                break;
            case ModalTransitionSubState.BriefingTwo:
                newState = ModalTransitionSubState.BriefingOne;
                ClearSpecialButtonTooltip();
                break;
            default: Debug.LogWarningFormat("Unrecognised ModalTransitionState \"{0}\"", GameManager.i.inputScript.ModalTransitionState); break;
        }
        //close tooltips
        GameManager.i.tooltipGenericScript.CloseTooltip("GUIManager.cs -> SetTooltipsOff");
        GameManager.i.tooltipHelpScript.CloseTooltip("GUIManager.cs -> SetTooltipsOff");
        //set new state
        if (newState != ModalTransitionSubState.None)
        {
            GameManager.i.inputScript.SetModalState(new ModalStateData() { mainState = ModalSubState.Transition, transitionState = newState });
            //Adjust UI
            SetStates(newState);
        }
    }

    /// <summary>
    /// Special button pressed in HQ status screen
    /// </summary>
    private void ExecuteHqEvents()
    {
        StringBuilder builder = new StringBuilder();
        int count = transitionInfoData.listOfHqEvents.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                if (builder.Length > 0) { builder.AppendLine(); builder.AppendLine(); }
                builder.Append(transitionInfoData.listOfHqEvents[i]);
            }
        }
        else { builder.Append("No events have occurred"); }

        //outcome
        ModalOutcomeDetails details = new ModalOutcomeDetails();
        details.side = GameManager.i.sideScript.PlayerSide;
        details.textTop = string.Format("{0}Recent HQ Events{1}", colourAlert, colourEnd);
        details.textBottom = builder.ToString();
        details.sprite = GameManager.i.guiScript.infoSprite;
        details.modalLevel = 2;
        details.modalState = ModalSubState.Transition;
        details.help0 = "transitionHq_0";
        details.help1 = "transitionHq_1";
        details.help2 = "transitionHq_2";
        //open outcome windown (will open MetaGameUI via triggerEvent once closed
        EventManager.i.PostNotification(EventType.OutcomeOpen, this, details, "TransitionUI.cs -> ExecuteHqEvents");

    }

    /// <summary>
    /// Special button pressed in EndLevel status Screen -> displays objective outcomes (current text is placeholder)
    /// </summary>
    private void ExecuteObjectives()
    {
        /*StringBuilder builder = new StringBuilder();
        int count = transitionInfoData.listOfObjectives.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                if (builder.Length > 0) { builder.AppendLine(); builder.AppendLine(); }
                builder.Append(transitionInfoData.listOfObjectives[i]);
            }
        }
        else { builder.Append("The mission had no objectives"); }*/


        //outcome
        ModalOutcomeDetails details = new ModalOutcomeDetails();
        details.side = GameManager.i.sideScript.PlayerSide;
        details.textTop = string.Format("{0}Mission Objectives{1}", colourAlert, colourEnd);
        details.textBottom = transitionInfoData.objectiveStatus;
        details.sprite = GameManager.i.guiScript.infoSprite;
        details.modalLevel = 2;
        details.modalState = ModalSubState.Transition;
        details.help0 = "transitionEnd_4";
        details.help1 = "transitionEnd_5";
        //open outcome windown (will open MetaGameUI via triggerEvent once closed
        EventManager.i.PostNotification(EventType.OutcomeOpen, this, details, "TransitionUI.cs -> ExecuteObjectives");
    }





    //new methods above here
}
