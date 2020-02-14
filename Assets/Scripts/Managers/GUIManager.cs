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
    [Tooltip("Alpha of Base Panel city and country text. Uses a 'byte' due to TextMeshPro script interface which is different to C#")]
    [Range(0, 255)] public byte alphaBaseText = 100;

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

    [Header("Alerts")]
    [Tooltip("Default time that Alerts stay on screen before disappearing")]
    [Range(1f, 5f)] public float alertDefaultTime = 3.0f;

    [Header("Sprites")]
    [Tooltip("Sprite to use for ActorGUI to show that the position is vacant")]
    public Sprite vacantAuthorityActor;
    [Tooltip("Sprite to use for ActorGUI to show that the position is vacant")]
    public Sprite vacantResistanceActor;
    [Tooltip("Universal Error sprite")]
    public Sprite errorSprite;
    [Tooltip("Universal Info sprite")]
    public Sprite infoSprite;
    [Tooltip("Alarm (spotted) sprite")]
    public Sprite alarmSprite;
    [Tooltip("Used for Target attempts that succeed")]
    public Sprite targetSuccessSprite;
    [Tooltip("Used for Target attempts that fail")]
    public Sprite targetFailSprite;
    [Tooltip("Used for Generic Picker with Planner action to select target (maybe there could be three colour variations of the same sprite to indicate how much intel you will gain?")]
    public Sprite targetInfoSprite;
    [Tooltip("Used for Player or Actor having been captured (152 x 160 png)")]
    public Sprite capturedSprite;
    [Tooltip("Used for Player or Actor being released from captivity (152 x 160 png)")]
    public Sprite releasedSprite;
    [Tooltip("Used for Player being Fired by their Faction")]
    public Sprite firedSprite;
    [Tooltip("Default City Arc sprite (512 x 150 png)")]
    public Sprite cityArcDefaultSprite;
    [Tooltip("Used for message Information Alerts (152 x 160 png)")]
    public Sprite alertInformationSprite;
    [Tooltip("Used for message Warning Alerts (152 x 160 png)")]
    public Sprite alertWarningSprite;
    [Tooltip("Used for message Random (152 x 160 png)")]
    public Sprite alertRandomSprite;
    [Tooltip("Used for ai reboot messages (152 x 160 png)")]
    public Sprite aiRebootSprite;
    [Tooltip("Used for ai countermeasures (152 x 160 png)")]
    public Sprite aiCountermeasureSprite;
    [Tooltip("Used for ai alert status changes (152 x 160 png)")]
    public Sprite aiAlertSprite;
    [Tooltip("Used for Investigations")]
    public Sprite investigationSprite;
    [Tooltip("Used for ongoing effects (152 x 160 png")]
    public Sprite ongoingEffectSprite;
    [Tooltip("Used for node Crisis (152 x 160 png")]
    public Sprite nodeCrisisSprite;
    [Tooltip("Used for city loyalty changes (152 x 160 png)")]
    public Sprite cityLoyaltySprite;
    [Tooltip("Used for itemData priority High in MainInfoUI (20 x 20 artboard with icon being 15 x 15 png)")]
    public Sprite priorityHighSprite;
    [Tooltip("Used for itemData priority Medium in MainInfoUI (20 x 20 artboard with icon being 15 x 15 png)")]
    public Sprite priorityMediumSprite;
    [Tooltip("Used for itemData priority Low in MainInfoUI (20 x 20 artboard with icon being 15 x 15 png)")]
    public Sprite priorityLowSprite;
    [Tooltip("Used for action adjustment infoAPP effect notifications")]
    public Sprite actionSprite;
    [Tooltip("Used for Objectives")]
    public Sprite objectiveSprite;
    [Tooltip("Default topic sprite")]
    public Sprite topicDefaultSprite;
    [Tooltip("Default sprite used for Topic UI (valid option) if none specified")]
    public Sprite topicOptionValidSprite;
    [Tooltip("Sprite used for an invalid topic option")]
    public Sprite topicOptionInvalidSprite;
    [Tooltip("Sprite for a Friendly relationship")]
    public Sprite friendSprite;
    [Tooltip("Sprite for an Enemy relationship")]
    public Sprite enemySprite;
    //moods
    [Tooltip("Player mood 0 star")]
    public Sprite moodStar0;
    [Tooltip("Player mood 1 star")]
    public Sprite moodStar1;
    [Tooltip("Player mood 2 star")]
    public Sprite moodStar2;
    [Tooltip("Player mood 3 star")]
    public Sprite moodStar3;

    [Header("Background Colours")]
    [Tooltip("Topic UI, main background, for all normal topics")]
    public Color colourTopicNormal;
    [Tooltip("Topic UI, main background, for Capture topics")]
    public Color colourTopicCapture;
    [Tooltip("Main Info UI left and right backgrounds as well as colour of Active LHS tab")]
    public Color colourMainBackground;

    //font awesome icons
    [HideInInspector] public char starChar = '\uf005';
    [HideInInspector] public char airportChar = '\uf072';
    [HideInInspector] public char harbourChar = '\uf13d';
    [HideInInspector] public char cityHallChar = '\uf66f';
    [HideInInspector] public char iconChar = '\uf5a6';


    private bool[] arrayIsBlocked;                                    //set True to selectively block raycasts onto game scene, eg. mouseover tooltips, etc.
                                                                      //to block use -> 'if (isBlocked == false)' in OnMouseDown/Over/Exit etc.
                                                                      //array corresponds to modalLevel, one block setting for each level, level 1 is isBlocked[1]
    private ShowMeData showMeData;                                    //data package that controls highlighting of node/connection and callback event to originating UI element
    [HideInInspector] public bool waitUntilDone;                        //flag to ensure nothing happens until
    private Dictionary<MsgPipelineType, ModalOutcomeDetails> dictOfPipeline = new Dictionary<MsgPipelineType, ModalOutcomeDetails>();           //handles message queue for start of turn information pipeline
    //colour palette 
    private string colourAlert;
    private string colourGood;
    private string colourNeutral;
    private string colourGrey;
    private string colourCancel;
    private string colourBad;
    //private string colourNormal;
    private string colourEnd;


    private string alpha = "<alpha=#66>";   //alpha transparency, used for stars (FF is 100%, CC / AA / 88 / 66 / 44 / 22)

    public void Awake()
    {
        //Check sprites are present
        Debug.Assert(vacantAuthorityActor != null, "Invalid vacantAuthorityActor (Null)");
        Debug.Assert(vacantResistanceActor != null, "Invalid vacantResistanceActor (Null)");
        Debug.Assert(errorSprite != null, "Invalid errorSprite (Null)");
        Debug.Assert(infoSprite != null, "Invalid infoSprite (Null)");
        Debug.Assert(alarmSprite != null, "Invalid alarmSprite (Null)");
        Debug.Assert(targetSuccessSprite != null, "Invalid targetSuccessSprite (Null)");
        Debug.Assert(targetFailSprite != null, "Invalid targetFailSprite (Null)");
        Debug.Assert(capturedSprite != null, "Invalid capturedSprite (Null)");
        Debug.Assert(releasedSprite != null, "Invalid releasedSprite (Null)");
        Debug.Assert(firedSprite != null, "Invalid firedSprite (Null)");
        Debug.Assert(cityArcDefaultSprite != null, "Invalid cityArcDefaultSprite (Null)");
        Debug.Assert(alertInformationSprite != null, "Invalid alertInformationSprite (Null)");
        Debug.Assert(alertWarningSprite != null, "Invalid alertWarningSprite (Null)");
        Debug.Assert(alertRandomSprite != null, "Invalid alertRandomSprite (Null)");
        Debug.Assert(aiRebootSprite != null, "Invalid aiRebootSpirte (Null)");
        Debug.Assert(aiCountermeasureSprite != null, "Invalid aiCountermeasureSprite (Null)");
        Debug.Assert(aiAlertSprite != null, "Invalid aiAlertSprite (Null)");
        Debug.Assert(investigationSprite != null, "Invalid investigationSprite (Null)");
        Debug.Assert(ongoingEffectSprite != null, "Invalid ongoingEffectSprite (Null)");
        Debug.Assert(nodeCrisisSprite != null, "Invalid nodeCrisisSprite (Null)");
        Debug.Assert(cityLoyaltySprite != null, "Invalid cityLoyaltySprite (Null)");
        Debug.Assert(priorityHighSprite != null, "Invalid priorityHighSprite (Null)");
        Debug.Assert(priorityMediumSprite != null, "Invalid priorityMediumSprite (Null)");
        Debug.Assert(priorityLowSprite != null, "Invalid priorityLowSprite (Null)");
        Debug.Assert(actionSprite != null, "Invalid actionSprite (Null)");
        Debug.Assert(objectiveSprite != null, "Invalid objectiveSprite (Null)");
        Debug.Assert(topicDefaultSprite != null, "Invalid topicDefaultSprite (Null)");
        Debug.Assert(topicOptionValidSprite != null, "Invalid topicValidSprite (Null)");
        Debug.Assert(topicOptionInvalidSprite != null, "Invalid topicInvalidSprite (Null)");
        Debug.Assert(friendSprite != null, "Invalid friendSprite (Null)");
        Debug.Assert(enemySprite != null, "Invalid enemySprite (Null)");
        Debug.Assert(moodStar0 != null, "Invalid moodStar0 (Null)");
        Debug.Assert(moodStar1 != null, "Invalid moodStar1 (Null)");
        Debug.Assert(moodStar2 != null, "Invalid moodStar2 (Null)");
        Debug.Assert(moodStar3 != null, "Invalid moodStar3 (Null)");
    }

    /// <summary>
    /// Initialises GUI with all relevant data
    /// </summary>
    /// <param name="arrayOfActors"></param>
    public void Initialise(GameState state)
    {
        //make sure blocking layers are all set to false
        arrayIsBlocked = new bool[numOfModalLevels + 1];
        for (int i = 0; i < arrayIsBlocked.Length; i++)
        { arrayIsBlocked[i] = false; }
        //event listener
        /*EventManager.instance.AddListener(EventType.ChangeSide, OnEvent, "GUIManager");*/
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "GUIManager");
        EventManager.instance.AddListener(EventType.ShowMeRestore, OnEvent, "GUIManager");
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
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodText);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralText);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badText);
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourCancel = GameManager.instance.colourScript.GetColour(ColourType.moccasinText);
        //colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.salmonText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
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
        GameManager.instance.modalGUIScript.SetModalMasks(isBlocked, level);
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


    /// <summary>
    /// generates an Alert messsage (generic UI) of a particular type (extend by adding to enum and providing relevant code to handle below).
    /// Multipurpose data point, eg. actorID, ignore otherwise.
    /// </summary>
    /// <param name="type"></param>
    public void SetAlertMessage(AlertType type, int data = -1)
    {
        ModalOutcomeDetails details = new ModalOutcomeDetails();
        details.sprite = infoSprite;
        details.side = GameManager.instance.sideScript.PlayerSide;
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
                switch (GameManager.instance.playerScript.status)
                {
                    case ActorStatus.Captured:
                        details.textTop = string.Format("This action can't be taken because you have been {0}Captured{1}", colourBad, colourEnd);
                        break;
                    case ActorStatus.Inactive:
                        switch (GameManager.instance.playerScript.inactiveStatus)
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
                Actor actor = GameManager.instance.dataScript.GetActor(data);
                if (actor != null)
                {
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
                switch (GameManager.instance.sideScript.PlayerSide.level)
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
            case AlertType.HackingInsufficientRenown:
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
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "GUIManager.cs -> SetAlertMessage");
    }


    /// <summary>
    /// Sets event type to call when a 'Show Me' event is restored, eg. UI element hidden, map showing and user presses any key to exit map and restore UI element (eg. MainInfoApp)
    /// </summary>
    /// <param name="restoreEvent"></param>
    public void SetShowMe(ShowMeData data)
    {
        if (data != null)
        {
            showMeData = data;
            if (data.nodeID > -1 || data.connID > -1)
            {
                //set game state
                ModalStateData package = new ModalStateData();
                package.mainState = ModalSubState.ShowMe;
                GameManager.instance.inputScript.SetModalState(package);
                //alert message
                GameManager.instance.nodeScript.NodeShowFlag = 1;
                GameManager.instance.alertScript.SetAlertUI("Press any KEY or BUTTON to Return");
                //highlight
                if (data.nodeID > -1)
                { EventManager.instance.PostNotification(EventType.FlashNodeStart, this, showMeData.nodeID, "GUIManager.cs -> SetShowMe"); }
                if (data.connID > -1)
                { EventManager.instance.PostNotification(EventType.FlashSingleConnectionStart, this, showMeData.connID, "GUIManager.cs -> SetShowMe"); }
            }
            else { Debug.LogWarning("GUIManager.cs -> SetShowMe: There are no node or connections to show"); }
        }
        else { Debug.LogError("Invalid ShowMeData package (Null)"); }
    }

    /// <summary>
    /// Restore map back to calling UI Element after a ShowMe event
    /// </summary>
    private void ShowMeRestore()
    {
        //reset node back to normal, if required
        if (showMeData.nodeID > -1)
        { EventManager.instance.PostNotification(EventType.FlashNodeStop, this, showMeData.nodeID, "GUIManager.cs -> ExecuteShowMeRestore"); }
        //reset connection back to normal, if required
        if (showMeData.connID > -1)
        { EventManager.instance.PostNotification(EventType.FlashSingleConnectionStop, this, showMeData.connID, "GUIManager.cs -> ExecuteShowMeRestore"); }
        //reactivate calling UI element
        EventManager.instance.PostNotification(showMeData.restoreEvent, this, null, "GUIManager.cs -> ShowMeRestore");
    }

    //
    // - - - Start of Turn Information Pipeline
    //

    public Dictionary<MsgPipelineType, ModalOutcomeDetails> GetDictOfPipeline()
    { return dictOfPipeline; }

    /// <summary>
    /// Empties out dictOfPipeline. Called by TurnManager.cs NewTurn prior to any endOfTurn/NewTurn processing
    /// </summary>
    public void InfoPipelineDictClear()
    {
        Debug.LogFormat("[Tst] GUIManager.cs -> InfoPipelineClear: Empty dictionary{0}", "\n");
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
            if (GameManager.instance.turnScript.CheckIsAutoRun() == false)
            {
                if (details.type != MsgPipelineType.None)
                {
                    //add to dictionary
                    try
                    {
                        dictOfPipeline.Add(details.type, details);
                        Debug.LogFormat("[Tst] GUIManager.cs -> InfoPipelineAdd: \"{0}\" added{1}", details.type, "\n");
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
            Debug.LogFormat("[Tst] GUIManager.cs -> InfoPipelineStart: start Pipeline - - - {0}", "\n");
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
        //loop through each message type and display in enum order, if present, one at a time.
        foreach (var msgType in Enum.GetValues(typeof(MsgPipelineType)))
        {
            if ((MsgPipelineType)msgType != MsgPipelineType.None)
            { yield return StartCoroutine("InfoPipelineMessage", (MsgPipelineType)msgType); }
            yield return new WaitForSecondsRealtime(pipelineWait);
        }
        InfoPipelineDictClear();
        //only do topic if level or campaign win state is 'None' (level win state incorporates campaign win state where appropriate)
        if (GameManager.instance.turnScript.winStateLevel == WinStateLevel.None)
        {
            yield return StartCoroutine("Topic");
            yield return new WaitForSecondsRealtime(pipelineWait);
        }
        yield return StartCoroutine("MainInfoApp", playerSide);
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
            Debug.LogFormat("[Tst] GUIManager.cs -> InfoPipelineProcess: dictionary contains key {0}{1}", type, "\n");
            ModalOutcomeDetails details = dictOfPipeline[type];
            if (details != null)
            {
                waitUntilDone = true;
                EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "GUIManager.cs -> InfoPipelineProcess");
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
        if (GameManager.instance.topicDisplayScript.CheckIsTopic() == true)
        {
            //switch of all modal 0 tooltips
            SetTooltipsOff();
            waitUntilDone = true;
            InitialiseTopic();
        }
        else { waitUntilDone = false; }
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
    }

    /// <summary>
    /// sets up and runs topic prior to info app at start of turn
    /// </summary>
    private void InitialiseTopic()
    {
        GameManager.instance.topicDisplayScript.ActivateTopicDisplay();
    }

    /// <summary>
    /// sets up and runs info app at start of turn
    /// </summary>
    /// <returns></returns>
    private void InitialiseInfoApp(GlobalSide playerSide)
    {
        MainInfoData data = GameManager.instance.dataScript.UpdateCurrentItemData();
        //only display InfoApp if player is Active (out of contact otherwise but data is collected and can be accessed when player returns to active status)
        ActorStatus playerStatus = GameManager.instance.playerScript.status;
        if (playerStatus == ActorStatus.Active)
        { EventManager.instance.PostNotification(EventType.MainInfoOpen, this, data, "TurnManager.cs -> ProcessNewTurn"); }
        else
        {
            Sprite sprite = GameManager.instance.guiScript.errorSprite;
            string text = ""; //empty
            switch (playerStatus)
            {
                case ActorStatus.Captured:
                    //special case of player losing campaign by being locked up permanently
                    if (GameManager.instance.turnScript.winReasonCampaign != WinReasonCampaign.Innocence)
                    {
                        text = string.Format("You have been {0}CAPTURED{1}", colourBad, colourEnd);
                        sprite = GameManager.instance.guiScript.capturedSprite;
                    }
                    break;
                case ActorStatus.Inactive:
                    switch (GameManager.instance.playerScript.inactiveStatus)
                    {
                        case ActorInactive.Breakdown:
                            text = string.Format("You are undergoing a {0}STRESS BREAKDOWN{1}", colourBad, colourEnd);
                            sprite = GameManager.instance.guiScript.infoSprite;
                            break;
                        case ActorInactive.LieLow:
                            text = string.Format("You are {0}LYING LOW{1}", colourNeutral, colourEnd);
                            sprite = GameManager.instance.guiScript.infoSprite;
                            break;
                        case ActorInactive.StressLeave:
                            text = string.Format("You are on {0}STRESS LEAVE{1}", colourNeutral, colourEnd);
                            sprite = GameManager.instance.guiScript.infoSprite;
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
                EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details);
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
                    { details.side = GameManager.instance.dataScript.GetGlobalSide(pipe.sideName); }
                    if (string.IsNullOrEmpty(pipe.spriteName) == false)
                    { details.sprite = GameManager.instance.dataScript.GetSprite(pipe.spriteName); }
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

    //set all modal 0 tooltips off
    public void SetTooltipsOff()
    {
        //exit any generic or node tooltips
        GameManager.instance.tooltipGenericScript.CloseTooltip("GUIManager.cs -> SetTooltipsOff");
        GameManager.instance.tooltipNodeScript.CloseTooltip("GUIManager.cs -> SetTooltipsOff");
        GameManager.instance.tooltipHelpScript.CloseTooltip("GUIManager.cs -> SetTooltipsOff");
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
    /// returns TMP Pro self contained string for the required number of stars (always returns 3 stars, colourNeutral for active, grey, low opacity, for inactive). Returns "Unknown"
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public string GetStars(int num)
    {
        string stars = "Unknown";
        switch (num)
        {
            case 3: stars = string.Format("<font=\"fontAwesomeSolid\">{0}{1} {2} {3}{4}</font>", colourNeutral, starChar, starChar, starChar, colourEnd); break;
            case 2: stars = string.Format("<font=\"fontAwesomeSolid\">{0}{1} {2} {3}{4}{5}{6}{7}</font>", colourNeutral, starChar, starChar, colourEnd, colourGrey, alpha, starChar, colourEnd); break;
            case 1: stars = string.Format("<font=\"fontAwesomeSolid\">{0}{1}{2} {3}{4}{5} {6}{7}</font>", colourNeutral, starChar, colourEnd, colourGrey, alpha, starChar, starChar, colourEnd); break;
            case 0: stars = string.Format("<font=\"fontAwesomeSolid\">{0}{1}{2} {3} {4}{5}</font>", colourGrey, alpha, starChar, starChar, starChar, colourEnd); break;
            default: Debug.LogWarningFormat("Unrecognised num \"{0}\"", num); break;
        }
        return stars;
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
            foreach(var record in dictOfPipeline)
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
