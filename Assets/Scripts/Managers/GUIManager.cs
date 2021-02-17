using gameAPI;
using modalAPI;
using packageAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Customises and Manages the main GUI
/// </summary>
public class GUIManager : MonoBehaviour
{
    [Header("Message Pipeline")]
    [Tooltip("Time in seconds to pause between messages, eg. between outcomes -> decision -> main info App")]
    [Range(0f, 1f)] public float pipelineWait = 0.2f;

    [Header("Alpha")]
    [Tooltip("Alpha of Actor portraits when ActorStatus is 'Active'")]
    [Range(0f, 1f)] public float alphaActive = 1.0f;
    [Tooltip("Alpha of Actor portraits when ActorStatus is 'InActive'")]
    [Range(0f, 1f)] public float alphaInactive = 0.45f;
    [Tooltip("Alpha of Base Panel city and country text")]
    [Range(0, 1f)] public float alphaBaseText = 0.65f;

    [Header("Modal")]
    [Tooltip("How many blocking modal levels are there? eg. the number of stackable UI levels?")]
    [Range(1, 2)] public int numOfModalLevels = 2;               //NOTE: change this > 2 you'll have to tweak a few switch/case structures, search on 'modalLevel'

    [Header("Flashing")]
    [Tooltip("How long it takes, in seconds, for the flashing red security alert (WidgetTopUI) to go from zero to full opacity")]
    [Range(0.5f, 2.0f)] public float flashRedTime = 1.0f;
    [Tooltip("How long it takes, in seconds, for the flashing white alert to go from zero to full opacity")]
    [Range(0.5f, 2.0f)] public float flashAlertTime = 1.0f;
    [Tooltip("Flash interval (in real seconds) for nodes")]
    [Range(0.1f, 1.0f)] public float flashNodeTime = 0.4f;
    [Tooltip("Flash interval (in real seconds) for connections")]
    [Range(0.1f, 1.0f)] public float flashConnectiontTime = 0.4f;
    [Tooltip("Flash interval (in real seconds) for InfoApp alerts over the top of Request and Meeting tabs")]
    [Range(0.1f, 1.0f)] public float flashInfoTabTime = 0.4f;
 
    [Header("PopUp Texts")]
    [Tooltip("How long the full animation cycle lasts (seconds)")]
    [Range(1f, 3f)] public float timerMax = 2.0f;
    [Tooltip("y_axis move speed (upwards) -> code in place but currently not used")]
    [Range(0, 3f)] public float moveSpeed = 0;
    [Tooltip("factor to increase size of text (first half of animation cycle")]
    [Range(0, 3f)] public float increaseScale = 1.0f;
    [Tooltip("factor to decrease size of text (second half of animation cycle")]
    [Range(0, 3f)] public float decreaseScale = 1.5f;
    [Tooltip("factor to fade in text during first half of animation cycle and fade out in the second")]
    [Range(0, 3f)] public float fadeSpeed = 1.0f;

    [Header("Alerts")]
    [Tooltip("Default time that Alerts stay on screen before disappearing")]
    [Range(1f, 5f)] public float alertDefaultTime = 3.0f;

    [Header("Tooltips")]
    [Tooltip("Time in seconds before tooltip activated (commences fade in)")]
    [Range(0.0f, 2.0f)] public float tooltipDelay = 0.5f;
    [Tooltip("Time in seconds for tooltip to fade in")]
    [Range(0.0f, 2.0f)] public float tooltipFade = 0.5f;
    [Tooltip("How many pixels above object that tooltip will be offset by")]
    [Range(0, 100)] public int tooltipOffset = 30;

    [Header("Top Widget Icon coroutines")]
    [Tooltip("Min font size for icon (default normal size)")]
    [Range(10, 20)] public int iconMinFontSize = 12;
    [Tooltip("Max font size for icon")]
    [Range(10, 20)] public int iconMaxFontSize = 18;
    [Tooltip("Default normal size for icons")]
    [Range(10, 20)] public int iconDefaultFontSize = 14;
    [Tooltip("Time interval (seconds) for coroutine step change in font size")]
    [Range(0, 1.0f)] public float iconFlashInterval = 0.05f;
    [Tooltip("Amount icon font size changes each time interval")]
    [Range(0, 1.0f)] public float iconFontIncrement = 0.25f;

    [Header("TopicUI")]
    [Tooltip("Coroutine base speed for Comms topic line interference movement down panel (higher is faster)")]
    [Range(0, 3.0f)] public float commsInterval = 1.5f;

    [Header("ModalReviewUI Review Button Flash")]
    [Tooltip("Max button size (y_axis)")]
    [Range(10, 30)] public int reviewMaxButtonSize = 20;
    [Tooltip("Button size increase increments per time interval")]
    [Range(1, 5)] public int reviewButtonIncrement = 2;
    [Tooltip("Button size change time interval (per increment step) in seconds")]
    [Range(0, 1.0f)] public float reviewTimeInterval = 0.10f;

    [Header("ActorPanelUI")]
    [Tooltip("Font size for Compatibility and Player Moods stars")]
    [Range(14, 24)] public int actorFontSize = 18;

    [Header("ModalInventoryUI")]
    [Tooltip("Max number of options available in UI")]
    [Range(4, 4)] public int maxInventoryOptions = 4;

    [Header("ModalOutcomeUI")]
    [Tooltip("Time for blackBars to Grow for Outcome Special mode (seconds)")]
    [Range(0.5f, 1.5f)] public float outcomeBarTimerGrow = 0.75f;
    [Tooltip("Time for blackBars to Shrink for Outcome Special mode (seconds)")]
    [Range(0.5f, 1.5f)] public float outcomeBarTimerShrink = 0.50f;
    [Tooltip("Time for highlight line to expand/contract for Outcome Special mode (seconds)")]
    [Range(0.5f, 1.5f)] public float outcomeHighlightTimer = 1.00f;
    [Tooltip("Pause time for highlight when it has shrunk and before it starts growing again for Outcome Special mode (seconds)")]
    [Range(0.5f, 1.5f)] public float outcomeHighlightPause = 1.0f;
    [Tooltip("Max width for highlight to expand to for Outcome Special mode (pixels)")]
    [Range(350, 1000)] public float outcomeHighlightMax = 700f;
    [Tooltip("Timer for fading out Highlights upon closing (seconds)")]
    [Range(0.25f, 1.00f)] public float outcomeHighlightFade = 0.50f;

    [Header("BillboardUI")]
    [Tooltip("Offset distance to get side panels offscreen during development (pixels")]
    [Range(0f, 200f)] public float billboardOffset = 135;
    [Tooltip("Billboard border flash speed, strobing in and out")]
    [Range(0.5f, 2.0f)] public float billboardFlash = 1.0f;
    [Tooltip("If billboard is switch 'ON' gives the % chance (less than) of a billboard being shown at the end of a turn (eg. determines frequency)")]
    [Range(0, 100)] public int billboardChance = 20;
    [Tooltip("Speed at which billboard blinds open and shut (speed x Time.deltaTime)")]
    [Range(0f, 200f)] public float billboardSpeed = 200.0f;
    [Tooltip("Strobing of playerName font size. How will it pause at full size? (Time.deltaTime)")]
    [Range(0f, 3.00f)] public float billboardFontPause = 1.25f;
    [Tooltip("Speed of font growing or shrinking (Time.deltaTime")]
    [Range(1.0f, 3.00f)] public float billboardFontSpeed = 1.25f;
    [Tooltip("Extra boost (multiplier) to font Growing (so it's this much times faster than when it's shrinking)")]
    [Range(1.0f, 3.00f)] public float billboardFontBoost = 1.75f;
    [Tooltip("Minimum font size that playerName will shrink to before growing")]
    [Range(10.0f, 20.0f)] public float billboardFontMin = 12.0f;
    [Tooltip("Max number of characters in playerName, over which it'll be swapped for a default text")]
    [Range(0, 50)] public int billboardNameMax = 20;
    [Tooltip("Light beams (top lights) -> how long they'll flicker off for")]
    [Range(0f, 1.0f)] public float billboardLightOff = 0.10f;
    [Tooltip("% chance of a light beam flickering each frame")]
    [Range(0, 100)] public int billboardLightChance = 1;

    [Header("Advertisements")]
    [Tooltip("Chance of an advert occuring per turn (excludes turns where a Review occurs)")]
    [Range(0, 100)] public int advertChance = 50;
    [Tooltip("Time for bars to grow (seconds)")]
    [Range(0.5f, 1.5f)] public float advertGrowTime = 0.75f;
    [Tooltip("Time for bars to shrink (seconds")]
    [Range(0.25f, 1.5f)] public float advertShrinkTime = 0.50f;



    //font awesome icons (characters)
    [HideInInspector] public char bulletChar;
    [HideInInspector] public char starChar;
    [HideInInspector] public char airportChar;
    [HideInInspector] public char harbourChar;
    [HideInInspector] public char cityHallChar;
    [HideInInspector] public char corporateChar;
    [HideInInspector] public char gatedChar;
    [HideInInspector] public char industrialChar;
    [HideInInspector] public char governmentChar;
    [HideInInspector] public char sprawlChar;
    [HideInInspector] public char utilityChar;
    [HideInInspector] public char researchChar;
    [HideInInspector] public char iconChar;
    [HideInInspector] public char positiveChar;
    [HideInInspector] public char neutralChar;
    [HideInInspector] public char negativeChar;
    [HideInInspector] public char commendationChar;
    [HideInInspector] public char blackmarkChar;
    [HideInInspector] public char investigateChar;
    [HideInInspector] public char innocenceChar;
    [HideInInspector] public char blackmailChar;
    [HideInInspector] public char unhappyChar;
    [HideInInspector] public char doomChar;
    [HideInInspector] public char conflictChar;
    [HideInInspector] public char stabilityChar;
    [HideInInspector] public char supportChar;
    [HideInInspector] public char securityChar;
    [HideInInspector] public char invisibilityChar;
    [HideInInspector] public char opinionChar;
    [HideInInspector] public char connectionsChar;
    [HideInInspector] public char cityChar;
    [HideInInspector] public char compatibilityChar;
    [HideInInspector] public char alertChar;
    //UI
    [HideInInspector] public char arrowRight;
    [HideInInspector] public char arrowLeft;


    //predefined data icons (colour formatted characters)
    [HideInInspector] public string stabilityIcon;
    [HideInInspector] public string supportIcon;
    [HideInInspector] public string securityIcon;
    [HideInInspector] public string invisibilityIcon;
    [HideInInspector] public string opinionIcon;       //also for Player MOOD
    [HideInInspector] public string connectionsIcon;
    [HideInInspector] public string innocenceIcon;
    [HideInInspector] public string cityIcon;
    [HideInInspector] public string cityIconGood;
    [HideInInspector] public string cityIconBad;
    [HideInInspector] public string hqIcon;
    [HideInInspector] public string hqIconBad;
    [HideInInspector] public string compatibilityIcon;
    [HideInInspector] public string starIconGood;
    [HideInInspector] public string starIconBad;
    [HideInInspector] public string alertIconGood;
    [HideInInspector] public string alertIconBad;
    //UI
    [HideInInspector] public string arrowIconRight;
    [HideInInspector] public string arrowIconLeft;



    private bool[] arrayIsBlocked;                                    //set True to selectively block raycasts onto game scene, eg. mouseover tooltips, etc.
                                                                      //to block use -> 'if (isBlocked == false)' in OnMouseDown/Over/Exit etc.
                                                                      //array corresponds to modalLevel, one block setting for each level, level 1 is isBlocked[1]
    private ShowMeData showMeData;                                    //data package that controls highlighting of node/connection and callback event to originating UI element
    [HideInInspector] public bool waitUntilDone;                        //flag to ensure nothing happens until current UI element completed, eg. allows sequential execution of message pipeline
    private Dictionary<MsgPipelineType, ModalOutcomeDetails> dictOfPipeline = new Dictionary<MsgPipelineType, ModalOutcomeDetails>();           //handles message queue for start of turn information pipeline
    //colour palette 
    private string colourAlert;
    private string colourGood;
    private string colourDataGood;
    private string colourDataTerrible;
    private string colourNeutral;
    private string colourGrey;
    private string colourCancel;
    private string colourBad;
    //private string colourNormal;
    private string colourEnd;

    /*//tooltip coroutines
    public Coroutine myNodeTooltip;*/


    private string alpha = "<alpha=#66>";   //alpha transparency, used for stars (FF is 100%, CC / AA / 88 / 66 / 44 / 22)



    /// <summary>
    /// Initialises GUI with all relevant data
    /// </summary>
    /// <param name="arrayOfActors"></param>
    public void Initialise(GameState state)
    {
        SetColours();
        SetFontChars();
        SetIcons();
        //make sure blocking layers are all set to false
        arrayIsBlocked = new bool[numOfModalLevels + 1];
        for (int i = 0; i < arrayIsBlocked.Length; i++)
        { arrayIsBlocked[i] = false; }
        //event listener
        /*EventManager.instance.AddListener(EventType.ChangeSide, OnEvent, "GUIManager");*/
        EventManager.i.AddListener(EventType.ChangeColour, OnEvent, "GUIManager");
        EventManager.i.AddListener(EventType.ShowMeRestore, OnEvent, "GUIManager");
    }

    /// <summary>
    /// Set AwesomeFont char symbols
    /// </summary>
    private void SetFontChars()
    {
        bulletChar = '\u2022';
        starChar = '\uf005';
        airportChar = '\uf072';
        harbourChar = '\uf13d';
        cityHallChar = '\uf19c';
        corporateChar = '\uf1ad';
        gatedChar = '\uf015';
        industrialChar = '\uf0ad';
        governmentChar = '\uf0e3';
        sprawlChar = '\uf554';
        utilityChar = '\uf0eb';
        researchChar = '\uf0c3';
        iconChar = '\uf5a6';
        positiveChar = '\uf058';
        neutralChar = '\uf056';
        negativeChar = '\uf057';
        commendationChar = '\uf559';
        blackmarkChar = '\uf0a3';
        investigateChar = '\uf002';
        innocenceChar = '\uf515';
        blackmailChar = '\uf7b9';
        unhappyChar = '\uf119';
        doomChar = '\uf714';
        conflictChar = '\uf57f';
        stabilityChar = '\uf67c';
        supportChar = '\uf2b5';
        securityChar = '\uf3ed';
        invisibilityChar = '\uf06e';
        opinionChar = '\uf118';
        connectionsChar = '\uf500';
        cityChar = '\uf64f';
        compatibilityChar = '\uf6ad';
        alertChar = '\uf06a';
        arrowRight = '\uf0a9';
        arrowLeft = '\uf0a8';
    }

    /// <summary>
    /// Set colour formatted predefined icon strings for data values. Used throughout code base, central repositry here
    /// </summary>
    private void SetIcons()
    {
        //icon colour should be different from topBarUI icon colours in order to differentiate between datapoint icons and alert icons
        string colourIcon = colourNeutral;
        stabilityIcon = string.Format("{0}{1}{2}", colourIcon, stabilityChar, colourEnd);
        supportIcon = string.Format("{0}{1}{2}", colourIcon, supportChar, colourEnd);
        securityIcon = string.Format("{0}{1}{2}", colourIcon, securityChar, colourEnd);
        invisibilityIcon = string.Format("{0}<font=\"fontAwesomeReg\">{1}</font>{2}", colourIcon, invisibilityChar, colourEnd);
        opinionIcon = string.Format("{0}<font=\"fontAwesomeReg\">{1}</font>{2}", colourIcon, opinionChar, colourEnd);
        connectionsIcon = string.Format("{0}{1}{2}", colourIcon, connectionsChar, colourEnd);
        innocenceIcon = string.Format("{0}{1}{2}", colourIcon, innocenceChar, colourEnd);
        compatibilityIcon = string.Format("{0}{1}{2}", colourIcon, compatibilityChar, colourEnd);
        cityIcon = string.Format("{0}{1}{2}", colourIcon, cityChar, colourEnd);
        cityIconGood = string.Format("{0}{1}{2}", colourDataGood, cityChar, colourEnd);
        cityIconBad = string.Format("{0}{1}{2}", colourDataTerrible, cityChar, colourEnd);
        hqIcon = string.Format("{0}HQ{1}", colourIcon, colourEnd);
        hqIconBad = string.Format("{0}HQ{1}", colourDataTerrible, colourEnd);
        starIconGood = string.Format("{0}{1}{2}", colourDataGood, starChar, colourEnd);
        starIconBad = string.Format("{0}{1}{2}", colourDataTerrible, starChar, colourEnd);
        alertIconGood = string.Format("{0}{1}{2}", colourDataGood, alertChar, colourEnd);
        alertIconBad = string.Format("{0}{1}{2}", colourDataTerrible, alertChar, colourEnd);
        //UI
        arrowIconRight = string.Format("{0}<size=120%>{1}</size>{2}", colourIcon, arrowRight, colourEnd);
        arrowIconLeft = string.Format("{0}<size=120%>{1}</size>{2}", colourIcon, arrowLeft, colourEnd);
    }


    /// <summary>
    /// handles events
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
            case EventType.ShowMeRestore:
                ShowMeRestore();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// set colour palette for Generic Tool tip
    /// </summary>
    public void SetColours()
    {
        colourGood = GameManager.i.colourScript.GetColour(ColourType.goodText);
        colourDataGood = GameManager.i.colourScript.GetColour(ColourType.dataGood);
        colourDataTerrible = GameManager.i.colourScript.GetColour(ColourType.dataTerrible);
        colourNeutral = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourBad = GameManager.i.colourScript.GetColour(ColourType.badText);
        colourGrey = GameManager.i.colourScript.GetColour(ColourType.greyText);
        colourCancel = GameManager.i.colourScript.GetColour(ColourType.moccasinText);
        //colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourAlert = GameManager.i.colourScript.GetColour(ColourType.salmonText);
        colourEnd = GameManager.i.colourScript.GetEndTag();
    }


    //
    // - - - GUI - - -
    //

    /// <summary>
    /// set True to selectively block raycasts onto game scene, eg. mouseover tooltips, etc.
    /// NOTE: UI elements are blocked through modal panels masks (ModalGUI.cs) and node gameobjects through code (gamestate)
    /// level is default 1, only use 2 if you require a UI element to open/close ontop of another, retained, UI element. Eg menu ontop of InventoryUI
    /// </summary>
    /// <param name="isBlocked"></param>
    public void SetIsBlocked(bool isBlocked, int level = 1)
    {
        Debug.Assert(level <= numOfModalLevels, string.Format("Invalid level {0}, max is numOfModalLevels {1}", level, numOfModalLevels));
        arrayIsBlocked[level] = isBlocked;
        Debug.Log(string.Format("GUIManager: Blocked -> {0}, level {1}{2}", isBlocked, level, "\n"));
        GameManager.i.modalGUIScript.SetModalMasks(isBlocked, level);
    }

    /// <summary>
    /// checks if modal layer blocked for a particular level
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public bool CheckIsBlocked(int level = 1)
    {
        Debug.Assert(level <= numOfModalLevels, string.Format("Invalid level {0}, max is numOfModalLevels {1}", level, numOfModalLevels));
        return arrayIsBlocked[level];
    }

    //
    // - - - Alert Messages
    //

    /// <summary>
    /// Generates an Alert message of a particular type. Modal Two, shown above a GUI element. Specify what GUI element should take control once message accepted
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    public void SetAlertMessageModalTwo(AlertType type, ModalSubState subState, int data = -1)
    {
        ModalOutcomeDetails details = new ModalOutcomeDetails();
        //sprite can be override in case statements below
        details.sprite = GameManager.i.spriteScript.infoSprite;
        details.side = GameManager.i.sideScript.PlayerSide;
        details.modalState = subState;
        CreateAlertMessage(type, details, data);
    }

    /// <summary>
    /// generates an Alert messsage (generic UI) of a particular type (extend by adding to enum and providing relevant code to handle below). Modal One, eg. no underlying GUI element
    /// Multipurpose data point, eg. actorID, ignore otherwise.
    /// </summary>
    /// <param name="type"></param>
    public void SetAlertMessageModalOne(AlertType type, int data = -1)
    {
        ModalOutcomeDetails details = new ModalOutcomeDetails();
        //sprite can be override in case statements below
        details.sprite = GameManager.i.spriteScript.infoSprite;
        details.side = GameManager.i.sideScript.PlayerSide;
        CreateAlertMessage(type, details, data);
    }

    #region CreateAlertMessage
    /// <summary>
    /// SubMethod for generating an alert Message. Called by SetAlertMessageModalOne\Two
    /// </summary>
    /// <param name="type"></param>
    /// <param name="details"></param>
    /// <param name="data"></param>
    private void CreateAlertMessage(AlertType type, ModalOutcomeDetails details, int data)
    {
        if (details != null)
        {
            switch (type)
            {
                case AlertType.SomethingWrong:
                    //generic fault message
                    details.textTop = string.Format("{0}Something has gone horribly wrong{1}", colourBad, colourEnd);
                    details.textBottom = "We aren't sure what but we've got our best man on it";
                    break;
                case AlertType.MainMenuUnavailable:
                    //generic fault message
                    details.textTop = string.Format("{0}Main Menu is unavailable{1}", colourBad, colourEnd);
                    details.textBottom = string.Format("If you are {0}Lying Low{1} or have been {2}Captured{3} your options are limited", colourAlert, colourEnd, colourAlert, colourEnd);
                    break;
                case AlertType.PlayerStatus:
                    switch (GameManager.i.playerScript.status)
                    {
                        case ActorStatus.Captured:
                            details.textTop = string.Format("This action can't be taken because you have been {0}Captured{1}", colourBad, colourEnd);
                            break;
                        case ActorStatus.Inactive:
                            details.sprite = GameManager.i.playerScript.sprite;
                            switch (GameManager.i.playerScript.inactiveStatus)
                            {
                                case ActorInactive.Breakdown:
                                    details.textTop = string.Format("This action can't be taken because you{0}are undergoing a{1}{2}{3}STRESS BREAKDOWN{4}", "\n", "\n", "\n",
                                        colourBad, colourEnd);
                                    break;
                                case ActorInactive.LieLow:
                                    details.textTop = string.Format("This action can't be taken because you are {0}{1}{2}Lying Low{3}", "\n", "\n", colourNeutral, colourEnd);
                                    break;
                                case ActorInactive.StressLeave:
                                    details.textTop = string.Format("This action can't be taken because you are on{0}{1}{2}Stress Leave{3}", "\n", "\n", colourNeutral, colourEnd);
                                    break;
                            }
                            break;
                        default:
                            details.textTop = "This action can't be taken because the Player is indisposed";
                            break;
                    }
                    break;
                case AlertType.ActorStatus:
                    //data is actorID
                    Actor actor = GameManager.i.dataScript.GetActor(data);
                    if (actor != null)
                    {
                        details.sprite = actor.sprite;
                        switch (actor.Status)
                        {
                            case ActorStatus.Captured:
                                details.textTop = string.Format("This action can't be taken because {0}, {1}, has been {2}Captured{3}", actor.actorName, actor.arc.name, colourBad, colourEnd);
                                break;
                            case ActorStatus.Inactive:
                                switch (actor.inactiveStatus)
                                {
                                    case ActorInactive.Breakdown:
                                        details.textTop = string.Format("This action can't be taken because {0}, {1},{2}is undergoing a{3}{4}{5}STRESS BREAKDOWN{6}", actor.actorName, actor.arc.name, "\n",
                                            "\n", "\n", colourBad, colourEnd);
                                        break;
                                    case ActorInactive.LieLow:
                                        details.textTop = string.Format("This action can't be taken because {0}, {1}, is {2}{3}{4}Lying Low{5}", actor.actorName, actor.arc.name,
                                            "\n", "\n", colourNeutral, colourEnd);
                                        break;
                                    case ActorInactive.StressLeave:
                                        details.textTop = string.Format("This action can't be taken because {0}, {1}, is on{2}{3}{4}Stress Leave{5}", actor.actorName, actor.arc.name,
                                            "\n", "\n", colourNeutral, colourEnd);
                                        break;
                                }
                                break;
                            default:
                                details.textTop = string.Format("This action can't be taken because {0}, {1}, is indisposed", actor.actorName, actor.arc.name);
                                break;
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", data); }
                    break;
                case AlertType.SideStatus:
                    details.textTop = string.Format("{0}This action is unavailable as the AI controls this side{1}", colourAlert, colourEnd);
                    break;
                case AlertType.DebugAI:
                    details.textTop = "The AI has been switched OFF";
                    details.textBottom = string.Format("The Player now has {0}<b>Manual control</b>{1} of both sides", colourNeutral, colourEnd);
                    break;
                case AlertType.DebugPlayer:
                    switch (GameManager.i.sideScript.PlayerSide.level)
                    {
                        case 1:
                            //authority
                            details.textTop = "The AI has been switched back ON (Resistance)";
                            details.textBottom = string.Format("The Player has {0}<b>Manual control</b>{1} of the AUTHORITY side only", colourNeutral, colourEnd);
                            break;
                        case 2:
                            //resistance
                            details.textTop = "The AI has been switched back ON (Authority)";
                            details.textBottom = string.Format("The Player has {0}<b>Manual control</b>{1} of the RESISTANCE side only", colourNeutral, colourEnd);
                            break;
                    }
                    break;
                case AlertType.HackingInitialising:
                    details.textTop = "Jacking into Authority AI. Initialising Icebreakers.";
                    details.textBottom = string.Format("{0}Wait one...{1}", colourNeutral, colourEnd);
                    break;
                case AlertType.HackingInsufficientPower:
                    details.textTop = string.Format("You have {0}{1}Insufficient Renown{2}{3}for a Hacking attempt", "\n", colourBad, colourEnd, "\n");
                    details.textBottom = string.Format("Check the colour of the Renown cost. If {0}Yellow{1} then just enough, {2}Green{3} then more than enough",
                        colourNeutral, colourEnd, colourGood, colourEnd);
                    break;
                case AlertType.HackingRebootInProgress:
                    details.textTop = string.Format("The AI is {0}Rebooting{1} it's Security Systems", colourBad, colourEnd);
                    details.textBottom = string.Format("Hacking attempts {0}aren't possible{1} until the Reboot is complete", colourNeutral, colourEnd);
                    break;
                case AlertType.HackingOffline:
                    details.textTop = string.Format("The AI has been {0}Isolated{1} from all external access", colourBad, colourEnd);
                    details.textBottom = string.Format("Until the AI comes back online hacking attempts {0}aren't possible{1}", colourNeutral, colourEnd);
                    break;
                case AlertType.HackingIndisposed:
                    details.textTop = string.Format("The AI is currently {0}inaccessible{1}", colourBad, colourEnd);
                    details.textBottom = string.Format("This is a {0}temporary{1} state and is due to the Player being {2}indisposed{3}", colourNeutral, colourEnd,
                        colourNeutral, colourEnd);
                    break;
                default:
                    details.textTop = "This action is unavailable";
                    break;
            }
            EventManager.i.PostNotification(EventType.OutcomeOpen, this, details, "GUIManager.cs -> SetAlertMessage");
        }
        else { Debug.LogError("Invalid ModalOutcomeDetails (Null)"); }
    }
    #endregion

    //
    // - - - Show Me
    //

    /// <summary>
    /// Sets event type to call when a 'Show Me' event is restored, eg. UI element hidden, map showing and user presses any key to exit map and restore UI element (eg. MainInfoApp)
    /// </summary>
    /// <param name="restoreEvent"></param>
    public void SetShowMe(ShowMeData data)
    {
        if (data != null)
        {
            //hide any underlying UI
            if (data.hideOtherEvent != EventType.None)
            { EventManager.i.PostNotification(data.hideOtherEvent, this, null, "GUIManager.cs -> SetShowMe"); }
            showMeData = data;
            //multiple nodes has priority
            if (data.listOfNodes.Count > 0)
            {
                InitialiseShowMe();
                EventManager.i.PostNotification(EventType.FlashNodesStart, this, data.listOfNodes, "GUIManager.cs -> SetShowMe");
            }
            else if (data.nodeID > -1 || data.connID > -1)
            {
                InitialiseShowMe();
                //highlight
                if (data.nodeID > -1)
                {
                    //add to a list (parameter changed to be a list, not a single node)
                    Node node = GameManager.i.dataScript.GetNode(data.nodeID);
                    if (node != null)
                    {
                        List<Node> listOfNodes = new List<Node>() { node };
                        EventManager.i.PostNotification(EventType.FlashNodesStart, this, listOfNodes, "GUIManager.cs -> SetShowMe");
                    }
                    else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", data.nodeID); }
                }
                if (data.connID > -1)
                { EventManager.i.PostNotification(EventType.FlashSingleConnectionStart, this, showMeData.connID, "GUIManager.cs -> SetShowMe"); }
            }
            else { Debug.LogWarning("GUIManager.cs -> SetShowMe: There are no nodes or connections to show"); }
        }
        else { Debug.LogError("Invalid ShowMeData package (Null)"); }
    }


    private void InitialiseShowMe()
    {
        //set game state
        ModalStateData package = new ModalStateData();
        package.mainState = ModalSubState.ShowMe;
        GameManager.i.inputScript.SetModalState(package);
        //alert message
        GameManager.i.nodeScript.NodeShowFlag = 1;
        GameManager.i.alertScript.SetAlertUI("Press any KEY or BUTTON to Return", 999f);
    }

    /// <summary>
    /// Restore map back to calling UI Element after a ShowMe event
    /// </summary>
    private void ShowMeRestore()
    {
        //reset nodes, or node, back to normal, if required
        if (showMeData.listOfNodes.Count > 0)
        { EventManager.i.PostNotification(EventType.FlashNodesStop, this, showMeData.nodeID, "GUIManager.cs -> ShowMeRestore"); }
        else if (showMeData.nodeID > -1)
        { EventManager.i.PostNotification(EventType.FlashNodesStop, this, showMeData.nodeID, "GUIManager.cs -> ShowMeRestore"); }
        //reset connection back to normal, if required
        if (showMeData.connID > -1)
        { EventManager.i.PostNotification(EventType.FlashSingleConnectionStop, this, showMeData.connID, "GUIManager.cs -> ShowMeRestore"); }
        //restore any underlying UI
        if (showMeData.restoreOtherEvent != EventType.None)
        { EventManager.i.PostNotification(showMeData.restoreOtherEvent, this, null, "GUIManager.cs -> ShowMeRestore"); }
        //restore calling UI element
        EventManager.i.PostNotification(showMeData.restoreEvent, this, null, "GUIManager.cs -> ShowMeRestore");
    }

    //
    // - - - Information Pipeline
    //

    public Dictionary<MsgPipelineType, ModalOutcomeDetails> GetDictOfPipeline()
    { return dictOfPipeline; }

    /// <summary>
    /// Empties out dictOfPipeline. Called by TurnManager.cs NewTurn prior to any endOfTurn/NewTurn processing
    /// </summary>
    public void InfoPipelineDictClear()
    {
        /*Debug.LogFormat("[Tst] GUIManager.cs -> InfoPipelineClear: Finish Messages, clear dictionary{0}", "\n");*/
        dictOfPipeline.Clear();
    }

    /// <summary>
    /// returns true if the specified pipeline type currently has a message sitting in the infopipeline (dictOfPipeline), false otherwise
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool CheckInfoPipeline(MsgPipelineType type)
    {
        if (dictOfPipeline.ContainsKey(type) == true)
        { return true; }
        return false;
    }

    /// <summary>
    /// add a message to the pipeline. Note that only one message per MsgPipeLineType can be added to dict. Returns true if successful, false otherwise (true if failed because of AutoRun)
    /// </summary>
    /// <param name="details"></param>
    public bool InfoPipelineAdd(ModalOutcomeDetails details)
    {
        if (details != null)
        {
            //don't add anything to the pipeline during an autorun
            if (GameManager.i.turnScript.CheckIsAutoRun() == false)
            {
                if (details.type != MsgPipelineType.None)
                {
                    //add to dictionary
                    try
                    {
                        dictOfPipeline.Add(details.type, details);
                        Debug.LogFormat("[Pip] GUIManager.cs -> InfoPipelineAdd: \"{0}\" message added{1}", details.type, "\n");
                        return true;
                    }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Invalid details (Duplicate) for \"{0}\"", details.type); }
                }
                else { Debug.LogWarning("Invalid ModalOutcomeDetails (type of None)"); }
            }
            else { return true; }
        }
        else { Debug.LogError("Invalid ModalOutcomeDetails (Null)"); }
        return false;
    }

    /// <summary>
    /// Commence processing info Pipeline (compromised gear has already been taken care off in TurnManager.cs -> ProcessNewTurn)
    /// Process the information pipeline in order -> Message Queue -> Topic -> MainInfoApp
    /// </summary>
    /// <param name="playerSide"></param>
    public void InfoPipelineStart(GlobalSide playerSide)
    {
        if (playerSide != null)
        {
            //switch of all modal 0 tooltips
            SetTooltipsOff();
            waitUntilDone = false;
            //process all messages in pipeline (waits until each message done)
            Debug.LogFormat("[Pip] GUIManager.cs -> InfoPipelineStart: start Pipeline - - - (dict has {0} entr{1}){2}", dictOfPipeline.Count, dictOfPipeline.Count != 1 ? "ies" : "y", "\n");
            StartCoroutine("InfoPipeline", playerSide);
        }
        else { Debug.LogError("Invalid playerSide (Null)"); }
    }

    /// <summary>
    /// Master coroutine that controls movement through the pipeline
    /// </summary>
    /// <param name="playerSide"></param>
    /// <returns></returns>
    IEnumerator InfoPipeline(GlobalSide playerSide)
    {
        //Billboard
        if (GameManager.i.optionScript.billboard == true)
        {
            int rnd = UnityEngine.Random.Range(0, 100);
            Debug.LogFormat("[Rnd] -> GUIManager.cs -> InfoPipeline: Billboard {0}, need < {1}, rolled {2}{3}", rnd < billboardChance ? "True" : "False", billboardChance, rnd, "\n");
            if (rnd < billboardChance)
            { GameManager.i.billboardScript.RunBillboard(true); }
            else { GameManager.i.billboardScript.RunBillboard(false); }
            yield return new WaitUntil(() => waitUntilDone == true);
            waitUntilDone = false;
            GameManager.i.billboardScript.ResetBillboard();
        }
        //loop through each message type and display in enum order, if present, one at a time.
        foreach (var msgType in Enum.GetValues(typeof(MsgPipelineType)))
        {
            if ((MsgPipelineType)msgType != MsgPipelineType.None)
            {
                yield return StartCoroutine("InfoPipelineMessage", (MsgPipelineType)msgType);
                yield return new WaitForSecondsRealtime(pipelineWait);
            }
        }
        InfoPipelineDictClear();
        //only do topic if level or campaign win state is 'None' (level win state incorporates campaign win state where appropriate)
        if (GameManager.i.turnScript.winStateLevel == WinStateLevel.None)
        {
            if (GameManager.i.optionScript.isDecisions == true)
            {
                yield return StartCoroutine("Topic");
                yield return new WaitForSecondsRealtime(pipelineWait);
            }
        }
        if (GameManager.i.optionScript.isMainInfoApp == true)
        { yield return StartCoroutine("MainInfoApp", playerSide); }
        else { GameManager.i.turnScript.AllowNewTurn(); }
    }

    /// <summary>
    /// generates individual message and waits until it is closed (ModalOutcome.cs -> CloseModalOutcome sets waitUntilDone to false)
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    IEnumerator InfoPipelineMessage(MsgPipelineType type)
    {
        InfoPipelineProcess(type);
        yield return new WaitUntil(() => waitUntilDone == false);
    }

    /// <summary>
    /// Process  message
    /// Each waits until it's closed before the next one starts
    /// </summary>
    /// <param name="type"></param>
    private void InfoPipelineProcess(MsgPipelineType type)
    {
        //find entry (if any) in dictionary
        if (dictOfPipeline.ContainsKey(type) == true)
        {
            Debug.LogFormat("[Pip] GUIManager.cs -> InfoPipelineProcess: show {0} message{1}", type, "\n");
            ModalOutcomeDetails details = dictOfPipeline[type];
            if (details != null)
            {
                waitUntilDone = true;
                EventManager.i.PostNotification(EventType.OutcomeOpen, this, details, "GUIManager.cs -> InfoPipelineProcess");
            }
            else { Debug.LogWarningFormat("Invalid details (Null) for dictOfPipeline[{0}]", type); }
        }
    }


    /// <summary>
    /// Sets up and displays Topic 
    /// </summary>
    /// <returns></returns>
    IEnumerator Topic()
    {
        TopicGlobal topicGlobal = GameManager.i.topicScript.GetTopicGlobal();
        Debug.LogFormat("[Pip] GUIManager.cs -> show {0} topic{1}", topicGlobal, "\n");

        //topic type
        switch (topicGlobal)
        {
            case TopicGlobal.Decision:
                //tooltips (modal 0) off
                SetTooltipsOff();
                if (GameManager.i.topicDisplayScript.CheckIsTopic() == true)
                {
                    waitUntilDone = true;
                    InitialiseTopic();
                }
                else { waitUntilDone = false; }
                break;
            case TopicGlobal.Review:
                //tooltips (modal 0) off
                SetTooltipsOff();
                waitUntilDone = true;
                GameManager.i.actorScript.InitialiseReview();
                break;
            case TopicGlobal.Advert:
                //tooltips (modal 0) off
                SetTooltipsOff();
                waitUntilDone = true;
                GameManager.i.advertScript.RunAdvert();
                break;
            case TopicGlobal.None:
            default:
                waitUntilDone = false;
                break;
        }
        yield return new WaitUntil(() => waitUntilDone == false);
    }

    /// <summary>
    /// sets up and displays mainInfoApp
    /// </summary>
    /// <returns></returns>
    IEnumerator MainInfoApp(GlobalSide playerSide)
    {
        yield return new WaitUntil(() => waitUntilDone == false);
        InitialiseInfoApp(playerSide);
        yield return null;
        GameManager.i.turnScript.AllowNewTurn();
    }

    /// <summary>
    /// sets up and runs topic prior to info app at start of turn
    /// </summary>
    private void InitialiseTopic()
    {
        GameManager.i.topicDisplayScript.ActivateTopicDisplay();
    }

    /// <summary>
    /// sets up and runs info app at start of turn
    /// </summary>
    /// <returns></returns>
    private void InitialiseInfoApp(GlobalSide playerSide)
    {
        MainInfoData data = GameManager.i.dataScript.UpdateCurrentItemData();
        //only display InfoApp if player is Active (out of contact otherwise but data is collected and can be accessed when player returns to active status)
        ActorStatus playerStatus = GameManager.i.playerScript.status;
        if (playerStatus == ActorStatus.Active)
        { EventManager.i.PostNotification(EventType.MainInfoOpen, this, data, "TurnManager.cs -> ProcessNewTurn"); }
        else
        {
            Sprite sprite = GameManager.i.spriteScript.errorSprite;
            string text = ""; //empty
            switch (playerStatus)
            {
                case ActorStatus.Captured:
                    //special case of player losing campaign by being locked up permanently
                    if (GameManager.i.turnScript.winReasonCampaign != WinReasonCampaign.Innocence)
                    {
                        text = string.Format("You have been {0}CAPTURED{1}", colourBad, colourEnd);
                        sprite = GameManager.i.spriteScript.capturedSprite;
                    }
                    break;
                case ActorStatus.Inactive:
                    switch (GameManager.i.playerScript.inactiveStatus)
                    {
                        case ActorInactive.Breakdown:
                            text = string.Format("You are undergoing a {0}STRESS BREAKDOWN{1}", colourBad, colourEnd);
                            sprite = GameManager.i.spriteScript.infoSprite;
                            break;
                        case ActorInactive.LieLow:
                            text = string.Format("You are {0}LYING LOW{1}", colourNeutral, colourEnd);
                            sprite = GameManager.i.spriteScript.infoSprite;
                            break;
                        case ActorInactive.StressLeave:
                            text = string.Format("You are on {0}STRESS LEAVE{1}", colourNeutral, colourEnd);
                            sprite = GameManager.i.spriteScript.infoSprite;
                            break;
                    }
                    break;
            }
            //Non-active status -> generate a message
            if (string.IsNullOrEmpty(text) == false)
            {
                ModalOutcomeDetails details = new ModalOutcomeDetails();
                details.textTop = text;
                details.textBottom = string.Format("{0}You are out of contact{1}{2}{3}Messages will be available for review once you return", colourAlert, colourEnd, "\n", "\n");
                details.sprite = sprite;
                EventManager.i.PostNotification(EventType.OutcomeOpen, this, details);
            }
        }
    }

    /// <summary>
    /// Sets saved pipeline msg details (ModalOutcomeDetails) in dictOfPipeline for save/load data (needed 'cause Player can create some msgs during their turn, eg. move into same node as Nemesis
    /// </summary>
    /// <param name="listOfPipeLineDetails"></param>
    public void SetDictOfPipeline(List<SaveInfoPipeLineInfo> listOfPipeLineDetails)
    {
        if (listOfPipeLineDetails != null)
        {
            //clear dictionary
            InfoPipelineDictClear();
            for (int i = 0; i < listOfPipeLineDetails.Count; i++)
            {
                SaveInfoPipeLineInfo pipe = listOfPipeLineDetails[i];
                if (pipe != null)
                {
                    ModalOutcomeDetails details = new ModalOutcomeDetails();
                    if (string.IsNullOrEmpty(pipe.sideName) == false)
                    { details.side = GameManager.i.dataScript.GetGlobalSide(pipe.sideName); }
                    if (string.IsNullOrEmpty(pipe.spriteName) == false)
                    { details.sprite = GameManager.i.dataScript.GetSprite(pipe.spriteName); }
                    details.textTop = pipe.textTop;
                    details.textBottom = pipe.textBottom;
                    details.modalLevel = pipe.modalLevel;
                    details.modalState = pipe.modalState;
                    details.isAction = pipe.isAction;
                    details.reason = pipe.reason;
                    details.type = pipe.type;
                    details.help0 = pipe.help0;
                    details.help1 = pipe.help1;
                    details.help2 = pipe.help2;
                    details.help3 = pipe.help3;
                    //add to dictionary
                    InfoPipelineAdd(details);
                }
                else { Debug.LogErrorFormat("Invalid listOfPipeLineDetails[{0}] (Null)", i); }
            }
        }
        else { Debug.LogError("Invalid listOfPipeLineDetails (Null)"); }
    }

    //
    // - - - Tooltips - - -
    //

    /// <summary>
    /// turn off all modal 0 tooltips
    /// </summary>
    public void SetTooltipsOff()
    {
        /*if (myNodeTooltip != null)
        { StopCoroutine(myNodeTooltip); }*/

        //exit any generic or node tooltips
        GameManager.i.tooltipGenericScript.CloseTooltip("GUIManager.cs -> SetTooltipsOff");
        GameManager.i.tooltipNodeScript.CloseTooltip("GUIManager.cs -> SetTooltipsOff");
        GameManager.i.tooltipHelpScript.CloseTooltip("GUIManager.cs -> SetTooltipsOff");
    }

    /// <summary>
    /// Returns a 3 string colour Formatted Tuple for the 'Show Me' tooltip. Used to standardize tooltip for all instances of 'Show Me'
    /// </summary>
    public Tuple<string, string, string> GetShowMeTooltip()
    {
        string tooltipHeader = string.Format("{0}Show Me{1}", colourCancel, colourEnd);
        string tooltipMain = string.Format("Press to display the {0}District{1} and/or {2}Connection{3} referred to", colourAlert, colourEnd, colourAlert, colourEnd);
        string tooltipDetails = string.Format("{0}Keyboard Shortcut{1}{2}{3}SPACE{4}", colourGrey, colourEnd, "\n", colourNeutral, colourEnd);
        return new Tuple<string, string, string>(tooltipHeader, tooltipMain, tooltipDetails);
    }


    //
    // - - - Stars - - -
    //

    /// <summary>
    /// returns TMP Pro self contained string for the required number of stars (always returns 3 stars, colourNeutral for active, grey, low opacity, for inactive). Returns "Unknown" if an issue
    /// </summary>
    /// <param name="stars"></param>
    /// <returns></returns>
    public string GetNormalStars(int stars)
    {
        string starString = "Unknown";
        switch (stars)
        {
            case 3: starString = string.Format("<font=\"fontAwesomeSolid\">{0}{1} {2} {3}{4}</font>", colourNeutral, starChar, starChar, starChar, colourEnd); break;
            case 2: starString = string.Format("<font=\"fontAwesomeSolid\">{0}{1} {2} {3}{4}{5}{6}{7}</font>", colourNeutral, starChar, starChar, colourEnd, colourGrey, alpha, starChar, colourEnd); break;
            case 1: starString = string.Format("<font=\"fontAwesomeSolid\">{0}{1}{2} {3}{4}{5} {6}{7}</font>", colourNeutral, starChar, colourEnd, colourGrey, alpha, starChar, starChar, colourEnd); break;
            case 0: starString = string.Format("<font=\"fontAwesomeSolid\">{0}{1}{2} {3} {4}{5}</font>", colourGrey, alpha, starChar, starChar, starChar, colourEnd); break;
            default: Debug.LogWarningFormat("Unrecognised num \"{0}\"", stars); break;
        }
        return starString;
    }

    /// <summary>
    /// returns TMP Pro self containted string for the required number of stars (red for negative, green for positive, grey for neutral). Returns 'Unknown' if an issue
    /// Compatibility -3 to +3
    /// </summary>
    /// <param name="compatibility"></param>
    /// <returns></returns>
    public string GetCompatibilityStars(int compatibility)
    {
        string stars = "Unknown";
        switch (compatibility)
        {
            case 3: stars = string.Format("<font=\"fontAwesomeSolid\">{0}{1} {2} {3}{4}</font>", colourDataGood, starChar, starChar, starChar, colourEnd); break;
            case 2: stars = string.Format("<font=\"fontAwesomeSolid\">{0}{1} {2} {3}{4}{5}{6}{7}</font>", colourDataGood, starChar, starChar, colourEnd, colourGrey, alpha, starChar, colourEnd); break;
            case 1: stars = string.Format("<font=\"fontAwesomeSolid\">{0}{1}{2} {3}{4}{5} {6}{7}</font>", colourDataGood, starChar, colourEnd, colourGrey, alpha, starChar, starChar, colourEnd); break;
            case 0: stars = string.Format("<font=\"fontAwesomeSolid\">{0}{1}{2} {3} {4}{5}</font>", colourGrey, alpha, starChar, starChar, starChar, colourEnd); break;
            case -1: stars = string.Format("<font=\"fontAwesomeSolid\">{0}{1}{2} {3}{4}{5} {6}{7}</font>", colourDataTerrible, starChar, colourEnd, colourGrey, alpha, starChar, starChar, colourEnd); break;
            case -2: stars = string.Format("<font=\"fontAwesomeSolid\">{0}{1} {2} {3}{4}{5}{6}{7}</font>", colourDataTerrible, starChar, starChar, colourEnd, colourGrey, alpha, starChar, colourEnd); break;
            case -3: stars = string.Format("<font=\"fontAwesomeSolid\">{0}{1} {2} {3}{4}</font>", colourDataTerrible, starChar, starChar, starChar, colourEnd); break;
            default: Debug.LogWarningFormat("Unrecognised compatibility \"{0}\"", compatibility); break;
        }
        return stars;
    }

    //
    // - - - Global tooltips
    //

    /// <summary>
    /// global Compatibility tooltip for consistency, returns only header, main and details
    /// </summary>
    /// <returns></returns>
    public GenericTooltipData GetCompatibilityTooltip()
    {
        string tooltipHeader = string.Format("{0} <size=120%>{1}</size>{2}with Player",
            GameManager.i.guiScript.compatibilityIcon,
            GameManager.Formatt("Compatibility", ColourType.moccasinText), "\n");
        string tooltipMain = string.Format("<align=\"left\">Due to Personalities{0}   {1} Good relations{2}   {3} Bad relations {4}{5} of Stars shows {6} of relationship.{7}{8}, doesn't change", "\n",
            GameManager.Formatt(starIconGood, ColourType.goodText), "\n",
            GameManager.Formatt(starIconBad, ColourType.badText), "\n",
            GameManager.Formatt("Number", ColourType.salmonText),
            GameManager.Formatt("Intensity", ColourType.salmonText), "\n",
            GameManager.Formatt("Constant", ColourType.salmonText)
            );
        string tooltipDetails = string.Format("<align=\"left\">A character {0} ignore {1} ({2}) or {3} ({4}){5}{6} Opinion outcomes",
            GameManager.Formatt("may", ColourType.salmonText),
            GameManager.Formatt("GOOD", ColourType.salmonText),
            GameManager.Formatt(starIconGood, ColourType.badText),
            GameManager.Formatt("BAD", ColourType.salmonText),
            GameManager.Formatt(starIconBad, ColourType.goodText), "\n", opinionIcon);
        GenericTooltipData tooltip = new GenericTooltipData()
        {
            header = tooltipHeader,
            main = tooltipMain,
            details = tooltipDetails
        };
        return tooltip;
    }

    //
    // - - - Debug - - -
    //

    /// <summary>
    /// display details of dictOfPipeline
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayInfoPipeLine()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("-dictOfPipeLine (infoPipeLine){0}", "\n");
        int count = dictOfPipeline.Count;
        if (count > 0)
        {
            foreach (var record in dictOfPipeline)
            {
                if (record.Value != null)
                {
                    builder.AppendFormat(" {0}{1}{2}", "\n", record.Key, "\n");
                    if (record.Value.side != null)
                    { builder.AppendFormat("  side: {0}{1}", record.Value.side.name, "\n"); }
                    else { builder.AppendFormat("  side: Null{0}", "\n"); }
                    builder.AppendFormat("  textTop: {0}{1}", record.Value.textTop, "\n");
                    builder.AppendFormat("  textBottom: {0}{1}", record.Value.textBottom, "\n");
                    if (record.Value.sprite != null)
                    { builder.AppendFormat("  sprite: {0}{1}", record.Value.sprite.name, "\n"); }
                    else { builder.AppendFormat("  sprite: Null{0}", "\n"); }
                    builder.AppendFormat("  modalLevel: {0}{1}", record.Value.modalLevel, "\n");
                    builder.AppendFormat("  modalState: {0}{1}", record.Value.modalState, "\n");
                    builder.AppendFormat("  isAction: {0}{1}", record.Value.isAction, "\n");
                    builder.AppendFormat("  reason: {0}{1}", record.Value.reason, "\n");
                    builder.AppendFormat("  type: {0}{1}", record.Value.type, "\n");
                    builder.AppendFormat("  help0: {0}{1}", record.Value.help0, "\n");
                    builder.AppendFormat("  help1: {0}{1}", record.Value.help1, "\n");
                    builder.AppendFormat("  help2: {0}{1}", record.Value.help2, "\n");
                    builder.AppendFormat("  help3: {0}{1}", record.Value.help3, "\n");
                }
                else { builder.Append("Invalid record (Null)"); }
            }
        }
        else { builder.Append(" No records"); }

        return builder.ToString();
    }

}
