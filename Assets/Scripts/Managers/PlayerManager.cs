using gameAPI;
using modalAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// handles all player related data and methods
/// </summary>
public class PlayerManager : MonoBehaviour
{
    [Header("Mood")]
    [Tooltip("Maximum value possible for Mood")]
    [Range(3, 3)] public int moodMax = 3;
    [Tooltip("Starting value of Player Mood at beginning of a level")]
    [Range(1, 3)] public int moodStart = 2;
    [Tooltip("Mood resets to this value once Player loses their Stressed Condition or completes Lying Low")]
    [Range(1, 3)] public int moodReset = 3;
    [Tooltip("Maximum amount per turn that Mood can improve by not spending actions")]
    [Range(1, 3)] public int moodImprove = 1;

    [Header("Investigations")]
    [Tooltip("Maximum number of investigations that can be active at a time")]
    [Range(3, 3)] public int maxInvestigations = 3;
    [Tooltip("Percentage chance of a revealed Secret resulting in an Investigation")]
    [Range(0, 100)] public int chanceInvestigation = 50;
    [Tooltip("Percentage chance of an investigation progressing (more evidence revealed) per turn")]
    [Range(0, 100)] public int chanceEvidence = 5;
    [Tooltip("Once an investigation reaches a boundary limit (> 3 or < 0) the timer kicks in and once it counts down to Zero the investigation is resolved (if evidence still at 0 or 3)")]
    [Range(1, 20)] public int timerInvestigationBase = 5;
    [Tooltip("Gain in HQ Approval if found Innocent")]
    [Range(1, 3)] public int investHQApproval = 2;


    public Sprite sprite;

    #region save data compatibile
    [HideInInspector] public int actorID;
    [HideInInspector] public ActorTooltip tooltipStatus;                            //Actor sprite shows a relevant tooltip if tooltipStatus > None (Breakdown, etc)
    [HideInInspector] public ActorSex sex;
    [HideInInspector] public bool isBreakdown;                                      //enforces a minimum one turn gap between successive breakdowns
    [HideInInspector] public bool isEndOfTurnGearCheck;                             //set true by UpdateGear (as a result of Compromised gear check)
    [HideInInspector] public bool isLieLowFirstturn;                                //set true when lie low action, prevents invis incrementing on first turn
    [HideInInspector] public bool isStressLeave;                                    //set true to ensure player spends one turn inactive on stress leave
    [HideInInspector] public bool isStressed;                                       //true if player stressed
    [HideInInspector] public bool isSpecialMoveGear;                                //player has/doesn't have special gear in inventory that allows move x 2 districts with no security issues
    [HideInInspector] public int numOfSuperStress;                                  //increments whenever the player gets stressed when already stressed
    [HideInInspector] public bool isAddicted;                                       //true if player addicted
    [HideInInspector] public int stressImmunityStart;                               //starting value of stressImmunityCurrent (decreases each time drug used)
    [HideInInspector] public int stressImmunityCurrent;                             //dynamic number of turns player is immune from stress (due to drug)
    [HideInInspector] public int addictedTally;                                     //starts from 0 whenever player becomes addicted (needed to provide a buffer before feed the need kicks in)
    [HideInInspector] public int maxNumOfDevices;                                   //max number of interrogation devices (captureTools) player can have in inventory at any one time (UI restriction)
    [HideInInspector] public string backstory0;
    [HideInInspector] public string backstory1;
    [HideInInspector] public string previousJob;
    [HideInInspector] public string pet;
    [HideInInspector] public string petName;
    [HideInInspector] public string reasonJoined;                                    //reason joined the Resistance 'I joined because [...]'
    [HideInInspector] public string initialSecret;                                   //starting secret
    //collections
    private bool[] arrayOfCaptureTools = new bool[4];                               //if true Player has CaptureTool.SO corresponding to array index (0 to 3) level of Innocence
    private List<string> listOfGear = new List<string>();                           //gear names of all gear items in inventory
    private List<Condition> listOfConditionsResistance = new List<Condition>();     //list of all conditions currently affecting the Resistance player
    private List<Condition> listOfConditionsAuthority = new List<Condition>();      //list of all conditions currently affecting the Authority player
    private List<Secret> listOfSecrets = new List<Secret>();                        //list of all secrets (skeletons in the closet)
    private List<Investigation> listOfInvestigations = new List<Investigation>();   //list of all active investigations into Player's behaviour (both sides)
    private List<HistoryMood> listOfMoodHistory = new List<HistoryMood>();          //tracks all changes to player mood
    //personality
    private int mood;
    private Personality personality = new Personality();
    private List<NodeActionData> listOfNodeActions = new List<NodeActionData>();
    #endregion

    private int[] arrayOfFactors = new int[5];                  //personality factors (initial), no need to save, could be default values or could be from player choice in tutorial

    /*[HideInInspector] public bool isOrgActivatedCurePresent;*/

    #region Private fields...
    //private backing fields, need to track separately to handle AI playing both sides
    private int _powerResistance;
    private int _powerAuthority;
    private string _playerNameResistance;
    private string _playerNameAuthority;
    private string _firstName;
    private int _invisibility;
    private int _innocence;
    private ActorStatus _status;
    private ActorInactive _inactiveStatus;
    #endregion

    #region Fast Access fields...
    //for fast access
    private GlobalSide globalAuthority;
    private GlobalSide globalResistance;
    private string hackingGear;
    private int maxNumOfSecrets = -1;
    private SecretStatus secretStatusActive;
    private Condition conditionCorrupt;
    private Condition conditionIncompetent;
    private Condition conditionQuestionable;
    private Condition conditionDoomed;
    private Condition conditionTagged;
    private Condition conditionWounded;
    private Condition conditionImaged;
    private Condition conditionStressed;
    private Condition conditionAddicted;
    private Gear gearSpecialMove;
    #endregion


    //Note: There is no ActorStatus for the player as the 'ResistanceState' handles this -> EDIT: Nope, status does

    #region Properties...

    #region PlayerName
    //Returns name of human controlled player side (even if temporarily AI controlled during an autoRun)
    //Use GetPlayerNameResistance/Authority for a specific side name
    //NOTE: _playerNameResistance/Authority are set by SideManager.cs -> Initialise
    public string PlayerName
    {
        get
        {
            if (GameManager.i.sideScript.PlayerSide.level == globalResistance.level)
            { return _playerNameResistance; }
            else if (GameManager.i.sideScript.PlayerSide.level == globalAuthority.level)
            { return _playerNameAuthority; }
            else
            {
                Debug.LogError("Invalid PlayerSide.level");
                return "Unknown Name";
            }
        }
    }
    #endregion

    #region FirstName
    public string FirstName
    { get { return _firstName; } }
    #endregion

    #region Power
    public int Power
    {
        get
        {
            if (GameManager.i.sideScript.PlayerSide.level == globalResistance.level) { return _powerResistance; }
            else if (GameManager.i.sideScript.PlayerSide.level == globalAuthority.level) { return _powerAuthority; }
            else
            {
                //AI control of both side
                if (GameManager.i.turnScript.currentSide.level == globalResistance.level)
                { return _powerResistance; }
                else { return _powerAuthority; }
            }
        }
        set
        {
            if (GameManager.i.sideScript.PlayerSide.level == globalResistance.level)
            {
                Debug.LogFormat("[Sta] -> PlayerManager.cs: Player (Resistance) Power changed from {0} to {1}{2}", _powerResistance, value, "\n");
                _powerResistance = value;

                //update AI side tab (not using an Event here) -> no updates for the first turn
                if (GameManager.i.turnScript.Turn > 0 && GameManager.i.sideScript.resistanceOverall == SideState.Human)
                { GameManager.i.aiScript.UpdateSideTabData(_powerResistance); }
                //update Power UI (regardless of whether on or off
                GameManager.i.actorPanelScript.UpdatePlayerPowerUI(_powerResistance);
            }
            else if (GameManager.i.sideScript.PlayerSide.level == globalAuthority.level)
            {

                /*value = Mathf.Clamp(value, 0, GameManager.instance.actorScript.maxStatValue);*/

                Debug.LogFormat("[Sta] -> PlayerManager.cs: Player (Authority) Power changed from {0} to {1}{2}", _powerAuthority, value, "\n");
                _powerAuthority = value;
                //update Power UI (regardless of whether on or off
                GameManager.i.actorPanelScript.UpdatePlayerPowerUI(_powerAuthority);
            }
            else
            {
                //AI control of both side
                if (GameManager.i.turnScript.currentSide.level == globalResistance.level) { _powerResistance = value; }
                else { _powerAuthority = value; }
            }
        }
    }
    #endregion

    #region Invisibility
    public int Invisibility
    {
        get { return _invisibility; }
        set
        {
            //stats (if losing invisibility), tick up for each level of invisibility lost
            if (value < _invisibility)
            { GameManager.i.dataScript.StatisticIncrement(StatType.PlayerInvisibilityLost, _invisibility - value); }
            else if (value <= 0 && _invisibility == 0)
            { GameManager.i.dataScript.StatisticIncrement(StatType.PlayerInvisibilityLost); }
            //change value
            value = Mathf.Clamp(value, 0, GameManager.i.actorScript.maxStatValue);
            Debug.LogFormat("[Sta] -> PlayerManager.cs:  Player (Resistance) Invisibility changed from {0} to {1}{2}", _invisibility, value, "\n");
            _invisibility = value;
        }
    }
    #endregion

    #region Innocence
    /// <summary>
    /// How innocent the Authority see the resistance human/AI player when captured (3 is innocent, eg. low level operative, 0 is guilty, eg. high level commander)
    /// </summary>
    public int Innocence
    {
        get { return _innocence; }
        set
        {
            if (value < 0)
            {
                //fail state for campaign
                string text = "Unknown";
                switch (GameManager.i.sideScript.PlayerSide.level)
                {
                    case 1: text = GameManager.Formatt("You have identified and incarcerated the leader of the Resistance in the City", ColourType.goodText); break;
                    case 2: text = GameManager.Formatt("You have been identified and incarcerated permanently", ColourType.badText); break;
                    default: Debug.LogWarningFormat("Unrecognised playerSide {0}", GameManager.i.sideScript.PlayerSide.name); break;
                }
                GameManager.i.turnScript.SetWinStateCampaign(WinStateCampaign.Authority, WinReasonCampaign.Innocence, "Authority Locks up Rebel Leader", text);
            }
            value = Mathf.Clamp(value, 0, GameManager.i.actorScript.maxStatValue);

            Debug.LogFormat("[Sta] -> PlayerManager.cs: Player (Resistance) Innocence changed from {0} to {1}{2}", _innocence, value, "\n");
            _innocence = value;
            if (GameManager.i.inputScript.CheckNormalMode() == true)
            {
                //update topBar
                GameManager.i.topBarScript.UpdateInnocence(_innocence);
            }
        }
    }
    #endregion

    #region Status
    public ActorStatus Status
    {
        get { return _status; }
        set
        {
            Debug.LogFormat("[Sta] -> PlayerManager.cs: Player Status changed from {0} to {1}{2}", _status, value, "\n");
            _status = value;
        }
    }
    #endregion

    #region InactiveStatus
    public ActorInactive InactiveStatus
    {
        get { return _inactiveStatus; }
        set
        {
            Debug.LogFormat("[Sta] -> PlayerManager.cs: Player InactiveStatus changed from {0} to {1}{2}", _inactiveStatus, value, "\n");
            _inactiveStatus = value;
        }
    }
    #endregion

    #endregion

    #region Initialisation...

    /// <summary>
    /// Not for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.TutorialOptions:
            case GameState.NewInitialisation:
                SubInitialiseTutorialDefaults(); //needs to be first
                SubInitialiseSessionStartEarly();
                SubInitialiseLevelStart();
                SubInitialiseFastAccess();
                SubInitialiseEvents();
                SubInitialiseLevelAll();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseFollowOn();
                break;
            case GameState.LoadAtStart:
                SubInitialiseLevelStart();
                SubInitialiseFastAccess();
                SubInitialiseEvents();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }


    public void InitialiseLate(GameState state)
    {
        switch (state)
        {
            case GameState.TutorialOptions:
            case GameState.NewInitialisation:
                //debug purposes only 
                SubInitialiseSessionStartLate();
                break;
        }
    }

    #endregion

    #region Initialise SubMethods...

    #region SubInitialiseSessionStartEarly
    private void SubInitialiseSessionStartEarly()
    {
        if (GameManager.i.inputScript.GameState != GameState.TutorialOptions)
        {
            if (GameManager.i.testScript.testPersonality == null)
            {
                //may have already been set in tutorial
                if (personality.CheckIfPersonalityDone() == false)
                {
                    //assign random / semi-random personality
                    arrayOfFactors = GameManager.i.personScript.SetPersonalityFactors(arrayOfFactors);
                    //set personality factors
                    personality.SetFactors(arrayOfFactors);
                }
            }
            else
            {
                //use a test personality
                arrayOfFactors = GameManager.i.testScript.testPersonality.GetFactors();
                //set personality factors
                personality.SetFactors(arrayOfFactors);
            }
        }
        //maximum innocence at start of campaign
        Innocence = 3;
        //set initial default value for drug stress immunity
        stressImmunityStart = GameManager.i.actorScript.playerAddictedImmuneStart;
    }
    #endregion

    #region SubInitialiseTutorialDefaults
    /// <summary>
    /// Sets default values, if needed, for any player related tutorial questions that haven't been done based on PreloadManager.cs defaults
    /// NOTE: Needs to be BEFORE SubInitialiseLevelAll
    /// </summary>
    private void SubInitialiseTutorialDefaults()
    {
        //Defaults -> sex
        if (sex == ActorSex.None)
        {
            switch (GameManager.i.preloadScript.playerSex.name)
            {
                case "Male": sex = ActorSex.Male; break;
                case "Female": sex = ActorSex.Female; break;
                default: Debug.LogWarningFormat("Unrecognised preloadManager.sex \"{0}\"", GameManager.i.preloadScript.playerSex.name); break;
            }
        }
        //Defaults -> previous job
        if (string.IsNullOrEmpty(previousJob) == true)
        { previousJob = GameManager.i.preloadScript.playerJob; }
        //Defaults -> pet
        if (string.IsNullOrEmpty(pet) == true)
        { pet = GameManager.i.preloadScript.playerPet; }
        //Defaults -> pet name
        if (string.IsNullOrEmpty(petName) == true)
        { petName = GameManager.i.preloadScript.playerPetName; }
        //Defaults -> player Secret
        if (string.IsNullOrEmpty(initialSecret) == true)
        { initialSecret = GameManager.i.preloadScript.playerSecret; }
        //Defaults -> personality factors (full random)
        arrayOfFactors = new int[] { 99, 99, 99, 99, 99 };
        //Defaults -> reason
        if (string.IsNullOrEmpty(reasonJoined) == true)
        { reasonJoined = GameManager.i.preloadScript.playerReason; }
    }
    #endregion

    #region SubInitialiseSessionStartLate
    private void SubInitialiseSessionStartLate()
    {
        GameManager.i.personScript.SetDescriptors(personality);
        GameManager.i.personScript.CheckPersonalityProfile(GameManager.i.dataScript.GetDictOfProfiles(), personality);

        //Debug -> testing purposes only
        /*Investigation invest = new Investigation()
        {
            reference = "test",
            city = GameManager.instance.cityScript.GetCity().name,
            tag = "Bad Ted Collaboration",
            turnStart = 1,
            lead = ActorHQ.SubBoss1,
            status = InvestStatus.Completed,
            outcome = InvestOutcome.Innocent
        };
        GameManager.instance.dataScript.AddInvestigationCompleted(invest);*/

        //tutorial
        if (GameManager.i.inputScript.GameState == GameState.TutorialOptions)
        {
            //Tutorial -> configure player
            if (GameManager.i.tutorialScript.set.playerConfig != null)
            { ConfigureTutorialPlayer(GameManager.i.tutorialScript.set.playerConfig); }
        }
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //fast access fields (BEFORE set stats below)
        globalAuthority = GameManager.i.globalScript.sideAuthority;
        globalResistance = GameManager.i.globalScript.sideResistance;
        hackingGear = GameManager.i.gearScript.typeHacking.name;
        maxNumOfSecrets = GameManager.i.secretScript.secretMaxNum;
        gearSpecialMove = GameManager.i.gearScript.gearSpecialMove;
        conditionCorrupt = GameManager.i.dataScript.GetCondition("CORRUPT");
        conditionIncompetent = GameManager.i.dataScript.GetCondition("INCOMPETENT");
        conditionQuestionable = GameManager.i.dataScript.GetCondition("QUESTIONABLE");
        conditionDoomed = GameManager.i.dataScript.GetCondition("DOOMED");
        conditionWounded = GameManager.i.dataScript.GetCondition("WOUNDED");
        conditionTagged = GameManager.i.dataScript.GetCondition("TAGGED");
        conditionImaged = GameManager.i.dataScript.GetCondition("IMAGED");
        conditionStressed = GameManager.i.dataScript.GetCondition("STRESSED");
        conditionAddicted = GameManager.i.dataScript.GetCondition("ADDICTED");
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(hackingGear != null, "Invalid hackingGear (Null)");
        Debug.Assert(maxNumOfSecrets > -1, "Invalid maxNumOfSecrets (-1)");
        Debug.Assert(conditionCorrupt != null, "Invalid conditionCorrupt (Null)");
        Debug.Assert(conditionIncompetent != null, "Invalid conditionIncompetent (Null)");
        Debug.Assert(conditionQuestionable != null, "Invalid conditionQuestionable (Null)");
        Debug.Assert(conditionDoomed != null, "Invalid conditionDoomed (Null)");
        Debug.Assert(conditionWounded != null, "Invalid conditionWounded (Null)");
        Debug.Assert(conditionTagged != null, "Invalid conditionTagged (Null)");
        Debug.Assert(conditionImaged != null, "Invalid conditionImaged (Null)");
        Debug.Assert(conditionStressed != null, "Invalid conditionStressed (Null)");
        Debug.Assert(conditionAddicted != null, "Invalid conditionAddicted (Null)");
        Debug.Assert(gearSpecialMove != null, "Invalid gearSpecialMove (Null)");
    }
    #endregion

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
        actorID = GameManager.i.preloadScript.playerActorID;
        //gear check
        isEndOfTurnGearCheck = false;
        //set stats      
        Invisibility = 3;
        mood = moodStart;
        //Update player mood
        GameManager.i.actorPanelScript.SetPlayerMoodUI(mood);
        //History
        string cityName = GameManager.i.cityScript.GetCityName();
        GameManager.i.dataScript.AddHistoryPlayer(new HistoryActor() { text = $"Arrive and take charge at <b>{cityName}</b>", isHighlight = true });
        //Mood
        HistoryMood record = new HistoryMood()
        {
            change = 0,
            descriptor = string.Format("Assume Command at <b>{0}</b>", cityName),
            mood = mood,
            factor = "",
            isStressed = false,
            isHighlight = true
        };
        listOfMoodHistory.Add(record);
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register event listeners
        EventManager.i.AddListener(EventType.EndTurnLate, OnEvent, "PlayerManager.cs");
        EventManager.i.AddListener(EventType.StartTurnLate, OnEvent, "PlayerManager.cs");
    }

    #endregion

    #region SubInitialiseLevelAll
    private void SubInitialiseLevelAll()
    {
        //place Player in a random start location (Sprawl node) -> AFTER Level and Session Initialisation
        InitialisePlayerStartNode();
        //empty out gear list
        listOfGear.Clear();
        //empty out secret list
        listOfSecrets.Clear();
        //Comes at end AFTER all other initialisations
        Power = 0;
        //assign chosen secret (not at start of tutorial as player will choose a secret during tutorial)
        if (GameManager.i.inputScript.GameState != GameState.TutorialOptions)
        { GetStartingSecret(); }
    }
    #endregion

    #region SubInitialiseFollowOn
    private void SubInitialiseFollowOn()
    {
        //place Player in a random start location (Sprawl node) -> AFTER Level and Session Initialisation
        InitialisePlayerStartNode();
        //set player alpha to active (may have been lying low at end of previous level)
        GameManager.i.actorPanelScript.UpdatePlayerAlpha(GameManager.i.guiScript.alphaActive);
        //set default status
        Status = ActorStatus.Active;
        InactiveStatus = ActorInactive.None;
        tooltipStatus = ActorTooltip.None;
        //reset mood to default
        SetMood(moodStart);
        //clear out required collections
        listOfMoodHistory.Clear();
        listOfNodeActions.Clear();
        //if immunity to stress > 0, set to max allowed
        if (stressImmunityCurrent > 0)
        { stressImmunityCurrent = stressImmunityStart; }
        //add cures for any existing conditions
        SetCures();
        //empty out gear list
        listOfGear.Clear();
        //Check listOfSpecial gear (populated during MetaGame) Place any gear into Player inventory
        List<string> listOfSpecialGear = GameManager.i.dataScript.GetListOfSpecialGear();
        if (listOfSpecialGear != null)
        {
            int count = listOfSpecialGear.Count;
            if (count > 0)
            {
                //add to Player's inventory
                for (int i = 0; i < count; i++)
                { AddGear(listOfSpecialGear[i]); }
                //empty out listOfSpecials
                listOfSpecialGear.Clear();
            }
        }
        else { Debug.LogError("Invalid listOfSpecialGear (Null)"); }
        //Check listOfCaptureTools (populated during MetaGame) Place any devicews into Player inventory
        List<CaptureTool> listOfCaptureTools = GameManager.i.dataScript.GetListOfCaptureTools();
        if (listOfCaptureTools != null)
        {
            int count = listOfCaptureTools.Count;
            if (count > 0)
            {
                //add to Player's inventory
                for (int i = 0; i < count; i++)
                { AddCaptureTool(listOfCaptureTools[i].innocenceLevel); }
                //empty out listOfCaptureTools
                listOfCaptureTools.Clear();
            }
        }
        else { Debug.LogError("Invalid listOfCaptureTools (Null)"); }
    }
    #endregion

    #endregion

    #region InitialisePlayerStartNode
    /// <summary>
    /// starts player at a random SPRAWL node (if startNodeID = -1, otherwise at specified)s, error if not or if City Hall district (player can't start here as nemesis always starts from there)
    /// </summary>
    private void InitialisePlayerStartNode(int startNodeID = -1)
    {
        //assign player to a starting node (Sprawl)
        int nodeID = 0;
        int nodeArcID = GameManager.i.dataScript.GetNodeArcID("SPRAWL");
        if (nodeArcID > -1)
        {
            Node node;
            if (startNodeID == -1)
            { node = GameManager.i.dataScript.GetRandomNode(nodeArcID); }
            else { node = GameManager.i.dataScript.GetNode(startNodeID); }
            if (node != null)
            { nodeID = node.nodeID; }
            else
            { Debug.LogWarning("PlayerManager: Invalid Player starting node (Null). Player placed in node '0' by default"); }
            //can't be at City Hall (Nemesis starting location) -> not possible as not a SPRAWL district but check anyway
            if (nodeID != GameManager.i.cityScript.cityHallDistrictID)
            {
                Node nodeTemp = GameManager.i.dataScript.GetNode(nodeID);
                if (nodeTemp != null)
                {
                    //initialise move list
                    nodeTemp.SetPlayerMoveNodes();
                    //set player node
                    GameManager.i.nodeScript.nodePlayer = nodeID;
                    Debug.LogFormat("[Ply] PlayerManager.cs -> Initialise: Player starts at node {0}, {1}, id {2}{3}", nodeTemp.nodeName, nodeTemp.Arc.name, nodeTemp.nodeID, "\n");
                    //message
                    string text = string.Format("Player commences at \"{0}\", {1}, ID {2}", node.nodeName, node.Arc.name, node.nodeID);
                    GameManager.i.messageScript.PlayerMove(text, node, 0, 0, true);
                    //history
                    GameManager.i.dataScript.AddHistoryRebelMove(new HistoryRebelMove()
                    { turn = 0, playerNodeID = nodeTemp.nodeID, invisibility = Invisibility, nemesisNodeID = -1, isHighlight = true, nodeName = nodeTemp.nodeName });
                }
                else { Debug.LogErrorFormat("Invalid playerNode (Null) for nodeID {0}", nodeID); }
            }
            else { Debug.LogError("Invalid player start node (Not a SPRAWL district)"); }
        }
        else { Debug.LogError("Invalid player start node (City Hall location)"); }
    }
    #endregion

    #region OnEvent
    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //select event type
        switch (eventType)
        {
            case EventType.StartTurnLate:
                if (Status == ActorStatus.Active)
                { ProcessInvestigations(); }
                break;
            case EventType.EndTurnLate:
                EndTurnFinal();
                break;
            default:
                Debug.LogErrorFormat("Invalid eventType {0}{1}", eventType, "\n");
                break;
        }
    }
    #endregion

    #region EndTurnFinal
    /// <summary>
    /// end of turn final
    /// </summary>
    private void EndTurnFinal()
    {
        //handles situation of no compromised gear checks and resets isEndOfTurnGearCheck
        ResetAllGear();
    }
    #endregion

    #region Tutorial...

    #region ResetTutorialPlayer
    /// <summary>
    /// Do this at start of every new tutorial set regardless of whether a tutorialPlayerConfig file is present or not
    /// if playerStartNodeID is -1 (first attempt at sandbox) then it's random, otherwise use that node (for retries)
    /// </summary>
    public void ResetTutorialPlayer(int playerStartNodeID = -1)
    {
        //status to active (may have been captured)
        Status = ActorStatus.Active;
        InactiveStatus = ActorInactive.None;
        tooltipStatus = ActorTooltip.None;
        //automatically clear out gear and secrets
        listOfSecrets.Clear();
        listOfGear.Clear();
        //remove any conditions
        RemoveAllConditions(globalResistance);
        //reset player position
        InitialisePlayerStartNode(playerStartNodeID);
    }
    #endregion

    #region ConfigureTutorialPlayer
    /// <summary>
    /// Initialises player state at the start of a set
    /// </summary>
    public void ConfigureTutorialPlayer(TutorialPlayerConfig config)
    {
        //player config present
        if (config != null)
        {
            int count;
            GlobalSide side = GameManager.i.sideScript.PlayerSide;
            //invisibility
            Invisibility = config.invisibility;
            //power
            Power = config.power;
            //mood
            mood = config.mood;
            //innocence
            Innocence = config.innocence;
            //conditions
            count = config.listOfConditions.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    Condition condition = config.listOfConditions[i];
                    if (condition != null)
                    {  AddCondition(condition, side, "tutorial config"); }
                    else { Debug.LogWarningFormat("Invalid condition (Null) in config.listOfConditions[{0}]", i); }
                }
            }
            //secrets
            count = config.listOfSecrets.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    Secret secret = config.listOfSecrets[i];
                    if (secret != null)
                    { AddSecret(secret); }
                    else { Debug.LogWarningFormat("Invalid secret (Null) in config.listOfSecrets[{0}]", i); }
                }
            }
            //gear
            count = config.listOfGear.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    Gear gear = config.listOfGear[i];
                    if (gear != null)
                    { AddGear(gear.name); }
                    else { Debug.LogWarningFormat("Invalid gear (Null) in config.listOfGear[{0}]", i); }
                }
            }
        }
        else { Debug.LogError("Invalid tutorialPlayerConfig (Null)"); }
    }
    #endregion

    #endregion

    #region Gear...
    //
    /// - - - Gear - - -
    /// 

    /// <summary>
    /// returns true if Gear name present in player's inventory
    /// </summary>
    /// <param name="gearName"></param>
    /// <returns></returns>
    public bool CheckGearPresent(string gearName)
    { return listOfGear.Exists(x => x == gearName); }

    /// <summary>
    /// returns gearName of best piece of gear that matches type, Null if none
    /// </summary>
    /// <param name="gearType"></param>
    /// <returns></returns>
    public string CheckGearTypePresent(GearType gearType)
    {
        string gearName = null;
        int rarity = -1;
        if (listOfGear.Count > 0)
        {
            //loop through looking for best piece of gear that matches the type
            for (int i = 0; i < listOfGear.Count; i++)
            {
                Gear gear = GameManager.i.dataScript.GetGear(listOfGear[i]);
                if (gear != null)
                {
                    if (gear.type.name.Equals(gearType.name, System.StringComparison.Ordinal) == true)
                    {
                        //is it a better piece of gear (higher rarity) than already found?
                        if (gear.rarity.level > rarity)
                        {
                            rarity = gear.rarity.level;
                            gearName = gear.name;
                        }
                    }
                }
                else
                { Debug.LogWarning(string.Format("Invalid gear (Null) for gearID {0}", listOfGear[i])); }
            }
        }
        return gearName;
    }


    /// <summary>
    /// returns list of AI(Hacking) gearNames of all gear capable of doing so in Player's inventory. Returns null if none
    /// </summary>
    /// <returns></returns>
    public List<string> CheckAIGearPresent()
    {
        List<string> tempList = new List<string>();
        //loop through looking for all AI hacking capable gear
        for (int i = 0; i < listOfGear.Count; i++)
        {
            Gear gear = GameManager.i.dataScript.GetGear(listOfGear[i]);
            if (gear != null)
            {
                //hacking gear
                if (gear.type.name.Equals(hackingGear, System.StringComparison.Ordinal) == true)
                {
                    //has AI effects
                    if (gear.aiHackingEffect != null)
                    { tempList.Add(gear.name); }
                }
            }
            else
            { Debug.LogWarning(string.Format("Invalid gear (Null) for gearID {0}", listOfGear[i])); }
        }
        return tempList;
    }

    /// <summary>
    /// returns Gear that has the specified aiHackingEffect.name, eg. "TraceBack Mask". Returns null if not found
    /// </summary>
    /// <param name="effectName"></param>
    /// <returns></returns>
    public Gear GetAIGear(string effectName)
    {
        //loop through looking for ai Hacking gear
        for (int i = 0; i < listOfGear.Count; i++)
        {
            Gear gear = GameManager.i.dataScript.GetGear(listOfGear[i]);
            if (gear != null)
            {
                //hacking gear
                if (gear.type.name.Equals(hackingGear, System.StringComparison.Ordinal) == true)
                {
                    //has AI effects -> return first instance found
                    if (gear.aiHackingEffect != null)
                    {
                        if (gear.aiHackingEffect.name.Equals(effectName, System.StringComparison.Ordinal) == true)
                        { return gear; }
                    }
                }
            }
            else
            { Debug.LogWarning(string.Format("Invalid gear (Null) for gearID {0}", listOfGear[i])); }
        }
        return null;
    }

    /// <summary>
    /// returns the amount of gear the player has in their inventory
    /// </summary>
    /// <returns></returns>
    public int CheckNumOfGear()
    { return listOfGear.Count; }


    public List<string> GetListOfGear()
    { return listOfGear; }

    /// <summary>
    /// update listOfGear with loaded save game data. Existing data is cleared out prior to updating.
    /// </summary>
    /// <param name="listOfGear"></param>
    public void SetListOfGear(List<string> listOfSavedGear)
    {
        if (listOfSavedGear != null)
        {
            listOfGear.Clear();
            listOfGear.AddRange(listOfSavedGear);
        }
        else { Debug.LogError("Invalid listOfSavedGear (Null)"); }
    }

    /// <summary>
    /// returns a list of all gear that matches the specified type, null if none
    /// </summary>
    /// <param name="gearType"></param>
    /// <returns></returns>
    public List<Gear> GetListOfGearType(GearType gearType)
    {
        Debug.Assert(gearType != null, "Invalid gearType (Null)");
        List<Gear> listOfSpecificGear = null;
        if (listOfGear.Count > 0)
        {
            //loop through looking for best piece of gear that matches the type
            for (int i = 0; i < listOfGear.Count; i++)
            {
                Gear gear = GameManager.i.dataScript.GetGear(listOfGear[i]);
                if (gear != null)
                {
                    if (gear.type.name.Equals(gearType.name, System.StringComparison.Ordinal) == true)
                    {
                        //initialise list if haven't already done so
                        if (listOfSpecificGear == null)
                        { listOfSpecificGear = new List<Gear>(); }
                        //add gear to list
                        listOfSpecificGear.Add(gear);
                    }
                }
            }
        }
        return listOfSpecificGear;
    }

    /// <summary>
    /// add gear to player's inventory. Checks for duplicates and null Gear. Returns true if successful.
    /// </summary>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public bool AddGear(string gearName)
    {
        if (GameManager.i.optionScript.isGear == true)
        {
            Gear gear = GameManager.i.dataScript.GetGear(gearName);
            if (listOfGear.Count < GameManager.i.gearScript.maxNumOfGear)
            {
                if (gear != null)
                {
                    //check gear not already in inventory
                    if (CheckGearPresent(gearName) == false)
                    {
                        ResetGearItem(gear);
                        listOfGear.Add(gearName);
                        Debug.LogFormat("[Gea] PlayerManager.cs -> AddGear: {0}, added to Player inventory{1}", gear.tag, "\n");
                        CheckForAIUpdate(gear);
                        //add to listOfCurrentGear (if not already present)
                        GameManager.i.dataScript.AddGearNew(gear);
                        //special Move gear
                        if (gearName.Equals(gearSpecialMove.name, StringComparison.Ordinal) == true)
                        {
                            isSpecialMoveGear = true;
                            UpdateMoveNodes();
                        }
                        //statistics
                        GameManager.i.dataScript.StatisticIncrement(StatType.GearTotal);
                        return true;
                    }
                    else
                    { Debug.LogWarningFormat("Gear \'{0}\", is already present in Player inventory", gear.tag); }
                }
                else
                { Debug.LogError(string.Format("Invalid gear (Null) for gear {0}", gearName)); }
            }
            /*else { Debug.LogWarning("You cannot exceed the maxium number of Gear items -> Gear NOT added"); }*/
        }
        else { Debug.LogWarningFormat("[Gea] PlayerManager.cs -> AddGear: Gear \"{0}\" can't be added as gear disabled (OptionManager.isGear {1}){2}", gearName, GameManager.i.optionScript.isGear, "\n"); }
        return false;
    }

    /// <summary>
    /// remove gear from player's inventory. Set isLost to true if the gear is gone forever so that the relevant lists can be updated
    /// </summary>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public bool RemoveGear(string gearName, bool isLost = false)
    {
        if (string.IsNullOrEmpty(gearName) == false)
        {
            Gear gear = GameManager.i.dataScript.GetGear(gearName);
            if (gear != null)
            {
                //check gear not already in inventory
                if (CheckGearPresent(gearName) == true)
                {
                    RemoveGearItem(gear, isLost);
                    //special Move gear
                    if (gearName.Equals(gearSpecialMove.name, StringComparison.Ordinal) == true)
                    {
                        isSpecialMoveGear = false;
                        UpdateMoveNodes();
                    }
                    return true;
                }
                else
                {
                    Debug.LogError(string.Format("Gear \"{0}\" NOT removed from inventory", gear.tag));
                    return false;
                }
            }
            else
            {
                Debug.LogError(string.Format("Invalid gear (Null) for gear {0}", gearName));
                return false;
            }
        }
        else { Debug.LogError("Invalid gearName (Null or Empty)"); }
        return false;
    }

    /// <summary>
    /// subMethod to update player's possible move nodes when special move gear is added or removed
    /// </summary>
    private void UpdateMoveNodes()
    {
        //update nodes where player can move (now only immediate neighbours)
        Node node = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.GetPlayerNodeID());
        if (node != null)
        { node.SetPlayerMoveNodes(); }
        else { Debug.LogErrorFormat("Invalid node (Null) for playerID {0}", GameManager.i.nodeScript.GetPlayerNodeID()); }
    }

    /// <summary>
    /// subMethod to remove a specific gear item and handle admin
    /// NOTE: gear checked for Null by calling method
    /// </summary>
    /// <param name="gear"></param>
    private void RemoveGearItem(Gear gear, bool isLost)
    {
        ResetGearItem(gear);
        listOfGear.Remove(gear.name);
        Debug.Log(string.Format("[Gea] PlayerManager.cs -> RemoveGear: {0}, removed from inventory{1}", gear.tag, "\n"));
        CheckForAIUpdate(gear);
        //lost gear
        if (isLost == true)
        {
            if (GameManager.i.dataScript.RemoveGearLost(gear) == false)
            {
                gear.statTurnLost = GameManager.i.turnScript.Turn;
                Debug.LogWarningFormat("Invalid gear Remove Lost for \"{0}\"", gear.tag);
            }
        }
    }

    /// <summary>
    /// called at beginning of every turn in order to reset fields based around gear use ready for the next turn. Only done if there where no Compromised Gear checks
    /// </summary>
    public void ResetAllGear()
    {
        if (isEndOfTurnGearCheck == false)
        {
            if (listOfGear.Count > 0)
            {
                //loop through looking for best piece of gear that matches the type
                for (int i = 0; i < listOfGear.Count; i++)
                {
                    Gear gear = GameManager.i.dataScript.GetGear(listOfGear[i]);
                    if (gear != null)
                    { ResetGearItem(gear); }
                }
            }
        }
        else
        {
            //checks have already been done elsewhere (PlayerManager.cs -> UpdateGear). Reset Flag
            isEndOfTurnGearCheck = false;
        }
    }

    /// <summary>
    /// subMethod to reset a single item of gear. Called by Add/Remove gear and ResetAllGear
    /// NOTE: Gear checked for Null by calling method
    /// </summary>
    /// <param name="gear"></param>
    private void ResetGearItem(Gear gear)
    {
        gear.timesUsed = 0;
        gear.reasonUsed = "";
        gear.isCompromised = false;
        gear.chanceOfCompromise = 0;
    }

    /// <summary>
    /// subMethod for Add / Remove gear that checks and updates AIManager.cs when required for presence, or absence, of AI Hacking gear
    /// NOTE: Gear checked for Null by calling methods
    /// </summary>
    /// <param name="gear"></param>
    private void CheckForAIUpdate(Gear gear)
    {
        //check for AI hacking gear
        if (gear.type.name.Equals(hackingGear, System.StringComparison.Ordinal) == true)
        {
            //has AI effects
            if (gear.aiHackingEffect != null)
            {
                //update AI to reflect this
                GameManager.i.aiScript.UpdateAIGearStatus();
            }
        }
    }

    /// <summary>
    /// called by GearManager.cs -> ProcessCompromisedGear. Tidies up gear at end of turn. gearSaveName is the compromised gear that the player chose to save. Resets all gear
    /// returns name of gear saved (UPPER) [assumes only one piece of gear can be saved per turn], if any. Otherwise returns Empty string
    /// </summary>
    public void UpdateGear(int powerUsed = -1, string gearSaveName = null)
    {
        if (listOfGear.Count > 0)
        {
            //reverse loop through looking for compromised gear
            for (int i = listOfGear.Count - 1; i >= 0; i--)
            {
                Gear gear = GameManager.i.dataScript.GetGear(listOfGear[i]);
                if (gear != null)
                {
                    if (gear.isCompromised == true)
                    {
                        if (gearSaveName != null)
                        {
                            //if not saved gear then remove
                            if (gear.name.Equals(gearSaveName, System.StringComparison.Ordinal) == false)
                            {
                                //gear lost
                                string msgText = string.Format("{0} ({1}), has been COMPROMISED and LOST", gear.tag, gear.type.name);
                                GameManager.i.messageScript.GearCompromised(msgText, gear, -1);
                                RemoveGearItem(gear, true);
                            }
                            else
                            {
                                //gear saved
                                ResetGearItem(gear);
                                string msgText = string.Format("{0} ({1}), has been COMPROMISED and SAVED", gear.tag, gear.type.name);
                                GameManager.i.messageScript.GearCompromised(msgText, gear, powerUsed);
                            }
                        }
                        else
                        {
                            //gear lost
                            string msgText = string.Format("{0} ({1}), has been COMPROMISED and LOST", gear.tag, gear.type.name);
                            GameManager.i.messageScript.GearCompromised(msgText, gear, -1);
                            RemoveGearItem(gear, true);
                        }
                    }
                    else
                    {
                        //no compromised gear
                        ResetGearItem(gear);
                    }
                }
                else { Debug.LogWarningFormat("Invalid gear (Null) for gear ID {0}", listOfGear[i]); }
            }
            #region archived code
            /*// reverse loop through looking for compromised gear
            for (int i = listOfGear.Count - 1; i >= 0; i--)
            {
                Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
                if (gear != null)
                {
                    if (gear.isCompromised == true)
                    {
                        //if not saved gear then remove
                        if (gear.name.Equals(gearSaveName) == false)
                        {
                            //message
                            string msgText = string.Format("{0} ({1}), has been COMPROMISED and LOST", gear.tag, gear.type.name);
                            GameManager.instance.messageScript.GearCompromised(msgText, gear, -1);
                            RemoveGearItem(gear, true);
                        }
                        else
                        {
                            //gear saved
                            ResetGearItem(gear);
                            gearSavedName = gear.tag.ToUpper();
                            string msgText = string.Format("{0} ({1}), has been COMPROMISED and SAVED", gear.tag, gear.type.name);
                            GameManager.instance.messageScript.GearCompromised(msgText, gear, powerUsed);
                        }
                    }
                    else
                    {
                        //no compromised gear
                        ResetGearItem(gear);
                    }
                }
                else { Debug.LogWarningFormat("Invalid gear (Null) for gear ID {0}", listOfGear[i]); }
            }*/
            #endregion
        }
    }

    /// <summary>
    /// returns a random gear item in Player inventory, null if none
    /// </summary>
    /// <returns></returns>
    public string GetRandomGear()
    {
        string gearName = null;
        int count = listOfGear.Count;
        if (count > 0)
        { gearName = listOfGear[Random.Range(0, count)]; }
        return gearName;
    }
    #endregion

    #region Conditions...
    //
    // - - - Conditions - - -
    //

    /// <summary>
    /// Initialise the listOfConditions (overwrites existing list). Used for load save game
    /// </summary>
    /// <param name="listOfConditions"></param>
    public void SetConditions(List<Condition> listOfConditions, GlobalSide side)
    {
        if (listOfConditions != null)
        {
            switch (side.level)
            {
                case 1:
                    listOfConditionsAuthority.Clear();
                    listOfConditionsAuthority.AddRange(listOfConditions);
                    break;
                case 2:
                    listOfConditionsResistance.Clear();
                    listOfConditionsResistance.AddRange(listOfConditions);
                    break;
                default:
                    Debug.LogError("Invalid side \"{0}\"", side);
                    break;
            }
        }
        else { Debug.LogError("Invalid listOfConditions (Null)"); }
    }

    /// <summary>
    /// Add a new condition to list provided it isn't already present, ignored if it is. Reason is a short self-contained sentence
    /// </summary>
    /// <param name="condition"></param>
    public void AddCondition(Condition condition, GlobalSide side, string reason)
    {
        bool isResistance = true;
        bool isProceed = true;
        if (side.level == 1) { isResistance = false; }
        if (condition != null)
        {
            List<Condition> listOfConditions = GetListOfConditionForSide(side);
            //keep going if reason not provided
            if (string.IsNullOrEmpty(reason) == true)
            {
                reason = "Unknown";
                Debug.LogWarning("Invalid reason (Null or Empty)");
            }
            if (listOfConditions != null)
            {
                //node details
                string nodeName = "Unknown";
                Node node = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.GetPlayerNodeID());
                if (node != null) { nodeName = node.nodeName; }
                else { Debug.LogWarningFormat("Invalid playerNode (Null) for nodeID {0}", GameManager.i.nodeScript.nodePlayer); }
                //check that condition isn't already present
                if (CheckConditionPresent(condition, side) == false)
                {
                    //special case -> Stressed, player may have immunity
                    if (condition.tag.Equals("STRESSED", StringComparison.Ordinal) == true)
                    {
                        if (stressImmunityCurrent > 0)
                        { isProceed = false; }
                    }
                    if (isProceed == true)
                    {
                        listOfConditions.Add(condition);
                        //special conditions
                        switch (condition.tag)
                        {
                            case "DOOMED":
                                GameManager.i.actorScript.SetDoomTimer();
                                GameManager.i.nodeScript.AddCureNode(conditionDoomed.cure);
                                break;
                            case "TAGGED":
                                GameManager.i.nodeScript.AddCureNode(conditionTagged.cure);
                                break;
                            case "WOUNDED":
                                GameManager.i.nodeScript.AddCureNode(conditionWounded.cure);
                                break;
                            case "IMAGED":
                                GameManager.i.nodeScript.AddCureNode(conditionImaged.cure);
                                break;
                            case "QUESTIONABLE":
                                GameManager.i.nodeScript.AddCureNode(conditionQuestionable.cure);
                                break;
                            case "ADDICTED":
                                isAddicted = true;
                                addictedTally = 0;
                                GameManager.i.nodeScript.AddCureNode(conditionAddicted.cure);
                                break;
                            case "STRESSED":
                                mood = 0;
                                isStressed = true;
                                //update UI
                                GameManager.i.actorPanelScript.SetPlayerMoodUI(0, isStressed);
                                //stats
                                GameManager.i.dataScript.StatisticIncrement(StatType.PlayerTimesStressed);
                                break;
                        }
                        Debug.LogFormat("[Cnd] PlayerManager.cs -> AddCondition: {0} Player, gains {1} condition{2}", side.name, condition.tag, "\n");
                        if (GameManager.i.sideScript.PlayerSide.level == side.level)
                        {
                            //message
                            string msgText = string.Format("{0} Player, {1}, gains condition \"{2}\"", side.name, GetPlayerName(side), condition.tag);
                            GameManager.i.messageScript.ActorCondition(msgText, actorID, true, condition, reason, isResistance);
                            //history
                            string textHistory = "Uknown";
                            switch (condition.tag)
                            {
                                case "DOOMED":
                                    textHistory = "Received a Lethal Injection (DOOMED)";
                                    break;
                                case "TAGGED":
                                    textHistory = "You've been TAGGED";
                                    break;
                                case "WOUNDED":
                                    textHistory = "You're WOUNDED";
                                    break;
                                case "IMAGED":
                                    textHistory = "You've been IMAGED";
                                    break;
                                case "QUESTIONABLE":
                                    textHistory = "Your loyalty is in doubt (QUESTIONABLE)";
                                    break;
                                case "ADDICTED":
                                    textHistory = "You've become ADDICTED";
                                    break;
                                case "STRESSED":
                                    textHistory = "You're STRESSED";
                                    break;
                                default: textHistory = string.Format("Is {0}", condition.tag); break;
                            }
                            GameManager.i.dataScript.AddHistoryPlayer(new HistoryActor() { text = textHistory, district = nodeName });
                        }
                    }
                }
                else
                    //condition already exists
                    switch (condition.tag)
                    {
                        case "STRESSED":
                            //stats
                            GameManager.i.dataScript.StatisticIncrement(StatType.PlayerSuperStressed);
                            //used to force a breakdown at the next opportunity
                            numOfSuperStress++;
                            //history
                            GameManager.i.dataScript.AddHistoryPlayer(new HistoryActor() { text = "Has become SUPER STRESSED", district = nodeName });
                            break;
                    }
            }
            else { Debug.LogError("Invalid listOfConditions (Null)"); }
        }
        else { Debug.LogError("Invalid condition (Null)"); }
    }

    /// <summary>
    /// Checks if actor has a specified condition 
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public bool CheckConditionPresent(Condition condition, GlobalSide side)
    {
        if (condition != null)
        {
            //use correct list for the player side
            List<Condition> listOfConditions = GetListOfConditionForSide(side);
            if (listOfConditions != null)
            {
                if (listOfConditions.Count > 0)
                {
                    /*foreach (Condition checkCondition in listOfConditions)
                    {
                        if (checkCondition.tag.Equals(condition.tag, System.StringComparison.Ordinal) == true)
                        { return true; }
                    }*/

                    if (listOfConditions.Exists(x => x.tag.Equals(condition.tag, System.StringComparison.Ordinal)) == true)
                    { return true; }
                }

            }
            else { Debug.LogError("Invalid listOfConditions (Null)"); }
        }
        else { Debug.LogError("Invalid condition (Null)"); }
        return false;
    }

    /// <summary>
    /// Removes a specified condition if present, ignored if it isn't
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public bool RemoveCondition(Condition condition, GlobalSide side, string reason)
    {
        bool isResistance = true;
        if (side.level == 1) { isResistance = false; }
        if (condition != null)
        {
            //keep going if reason not provided
            if (string.IsNullOrEmpty(reason) == true)
            {
                reason = "Unknown";
                Debug.LogWarning("Invalid reason (Null or Empty)");
            }
            //use correct list for the player side
            List<Condition> listOfConditions = GetListOfConditionForSide(side);
            if (listOfConditions != null)
            {
                if (listOfConditions.Count > 0)
                {
                    //reverse loop -> delete and return if found
                    for (int i = listOfConditions.Count - 1; i >= 0; i--)
                    {
                        if (listOfConditions[i].name.Equals(condition.tag, System.StringComparison.Ordinal) == true)
                        {
                            listOfConditions.RemoveAt(i);
                            Debug.LogFormat("[Cnd] PlayerManager.cs -> RemoveCondition: {0} Player, lost {1} condition{2}", side.name, condition.tag, "\n");
                            if (GameManager.i.sideScript.PlayerSide.level == side.level)
                            {
                                //message
                                string msgText = string.Format("{0} Player, {1}, condition \"{2}\" removed", side.name, GetPlayerName(side), condition.tag);
                                GameManager.i.messageScript.ActorCondition(msgText, actorID, false, condition, reason, isResistance);
                            }
                            //special conditions
                            switch (condition.tag)
                            {
                                case "DOOMED":
                                    GameManager.i.actorScript.StopDoomTimer();
                                    GameManager.i.nodeScript.RemoveCureNode(conditionDoomed.cure);
                                    break;
                                case "TAGGED":
                                    GameManager.i.nodeScript.RemoveCureNode(conditionTagged.cure);
                                    break;
                                case "WOUNDED":
                                    GameManager.i.nodeScript.RemoveCureNode(conditionWounded.cure);
                                    break;
                                case "IMAGED":
                                    GameManager.i.nodeScript.RemoveCureNode(conditionImaged.cure);
                                    break;
                                case "QUESTIONABLE":
                                    GameManager.i.nodeScript.RemoveCureNode(conditionQuestionable.cure);
                                    break;
                                case "STRESSED":
                                    ChangeMood(moodReset, reason, "");
                                    isStressed = false;
                                    numOfSuperStress = 0;
                                    //change UI mood sprite
                                    GameManager.i.actorPanelScript.SetPlayerMoodUI(mood);
                                    break;
                                case "ADDICTED":
                                    //reset drug immunity period back to default value
                                    stressImmunityStart = GameManager.i.actorScript.playerAddictedImmuneStart;
                                    stressImmunityCurrent = 0;
                                    isAddicted = false;
                                    addictedTally = 0;
                                    GameManager.i.nodeScript.RemoveCureNode(conditionAddicted.cure);
                                    break;
                            }
                            return true;
                        }
                    }
                }
            }
            else { Debug.LogError("Invalid listOfConditions (Null)"); }
        }
        else { Debug.LogError("Invalid condition (Null)"); }
        return false;
    }

    /// <summary>
    /// Player takes illegal drugs (GlobalManager.cs -> tagGlobalDrug) and gains immunity from Stress for a number of turns/days
    /// </summary>
    public void TakeDrugs()
    {
        stressImmunityCurrent = stressImmunityStart;
        string text = string.Format("[Msg] Player takes drugs, current immunity {0} days, start {1} days, addicted {2}{3}", stressImmunityCurrent, stressImmunityStart, isAddicted, "\n");

        //message (only if not Addicted)
        if (isAddicted == false)
        {
            GameManager.i.messageScript.PlayerImmuneStart(text, stressImmunityCurrent, stressImmunityStart, isAddicted);
            GameManager.i.messageScript.PlayerImmuneEffect(text, stressImmunityCurrent, stressImmunityStart, isAddicted);
        }

        /*//effect tab only if right at start of addiction period (messages double up otherwise) -> not possible as there is now an exempt period
        if (addictedTally == 0)
        { GameManager.instance.messageScript.PlayerImmuneEffect(text, stressImmunityCurrent, stressImmunityStart, isAddicted); }*/

        //decrease immunity period after every addiction episode
        stressImmunityStart--;
        //minCap
        stressImmunityStart = Mathf.Max(GameManager.i.actorScript.playerAddictedImmuneMin, stressImmunityStart);
        //remove stress if present
        RemoveCondition(conditionStressed, GameManager.i.sideScript.PlayerSide, "Took Drugs");
    }

    /// <summary>
    /// Used for a followOn level, Player starts with no conditions
    /// </summary>
    /// <param name="side"></param>
    private void RemoveAllConditions(GlobalSide side)
    {
        //use correct list for the player side
        List<Condition> listOfConditions = GetListOfConditionForSide(side);
        if (listOfConditions != null)
        {
            if (listOfConditions.Count > 0)
            { listOfConditions.Clear(); }
        }
        else { Debug.LogErrorFormat("Invalid listOfConditions (Null) for {0}", side); }
    }

    /// <summary>
    /// Number of conditions present
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public int CheckNumOfConditions(GlobalSide side)
    {
        //use correct list for the player side
        List<Condition> listOfConditions = GetListOfConditionForSide(side);
        return listOfConditions.Count;
    }

    /// <summary>
    /// Returns a list of all bad conditions (Corrupt/Incompetent/Questionable) present, empty if none
    /// </summary>
    /// <returns></returns>
    public List<Condition> GetNumOfBadConditionPresent(GlobalSide side)
    {
        List<Condition> tempList = new List<Condition>();
        bool checkCorrupt = true;
        bool checkIncompetent = true;
        bool checkQuestionable = true;
        //use correct list for the player side
        List<Condition> listOfConditions = GetListOfConditionForSide(side);
        if (listOfConditions != null)
        {
            if (listOfConditions.Count > 0)
            {
                foreach (Condition condition in listOfConditions)
                {
                    if (checkCorrupt == true && condition.tag.Equals(conditionCorrupt.name, System.StringComparison.Ordinal) == true)
                    { tempList.Add(conditionCorrupt); checkCorrupt = false; }
                    if (checkIncompetent == true && condition.tag.Equals(conditionIncompetent.name, System.StringComparison.Ordinal) == true)
                    { tempList.Add(conditionIncompetent); checkIncompetent = false; }
                    if (checkQuestionable == true && condition.tag.Equals(conditionQuestionable.name, System.StringComparison.Ordinal) == true)
                    { tempList.Add(conditionQuestionable); checkQuestionable = false; }
                }
            }
        }
        else { Debug.LogError("Invalid listOfConditions (Null)"); }
        return tempList;
    }

    /// <summary>
    /// returns Condition list for the specified side, Null if a problem
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public List<Condition> GetListOfConditions(GlobalSide side)
    {
        return GetListOfConditionForSide(side);
    }

    /// <summary>
    /// Sub method to get correct list of Conditions, returns null if a problem. Use 'GetListOfConditions' for public access
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    private List<Condition> GetListOfConditionForSide(GlobalSide side)
    {
        //use correct list for the player side
        List<Condition> listOfConditions = null;
        switch (side.level)
        {
            case 1: listOfConditions = listOfConditionsAuthority; break;
            case 2: listOfConditions = listOfConditionsResistance; break;
            default: Debug.LogErrorFormat("Invalid side \"{0}\" (must be Authority or Resistance regardless of whether AI or not)", side.name); break;
        }
        return listOfConditions;
    }


    /// <summary>
    /// Follow on level -> if player has any existing conditions, new cures are created
    /// </summary>
    private void SetCures()
    {
        //if player has any conditions that require a cure you need to set up nodes for the cures
        List<Condition> listOfConditions = GetListOfConditions(GameManager.i.sideScript.PlayerSide);
        if (listOfConditions != null)
        {
            if (listOfConditions.Count > 0)
            {
                foreach (Condition condition in listOfConditions)
                {
                    if (condition.cure != null)
                    {
                        //reset doom timer to starting value, if required
                        if (condition.tag.Equals("DOOMED", StringComparison.Ordinal) == true)
                        { GameManager.i.actorScript.SetDoomTimer(); }
                        //add cure to a node (cure isn't activated yet)
                        GameManager.i.nodeScript.AddCureNode(condition.cure);
                    }
                }
            }
        }
        else { Debug.LogError("Invalid listOfConditions (Null)"); }
    }
    #endregion

    #region Secrets...
    //
    // - - - Secrets - - -
    //

    public List<Secret> GetListOfSecrets()
    { return listOfSecrets; }

    public int CheckNumOfSecrets()
    { return listOfSecrets.Count; }

    /// <summary>
    /// returns true if secret present in Player's list of secrets, false otherwise
    /// </summary>
    /// <param name="secret"></param>
    /// <returns></returns>
    public bool CheckSecretPresent(Secret secret)
    { return listOfSecrets.Exists(x => x.name.Equals(secret.name, StringComparison.Ordinal) == true); }

    /// <summary>
    /// Initialise the listOfSecrets (overwrites existing list). Used for load save game
    /// </summary>
    /// <param name="listOfSecrets"></param>
    public void SetSecrets(List<Secret> listOfSecrets)
    {
        if (listOfSecrets != null)
        {
            this.listOfSecrets.Clear();
            this.listOfSecrets.AddRange(listOfSecrets);
        }
        else { Debug.LogError("Invalid listOfSecrets (Null)"); }
    }

    /// <summary>
    /// returns secret based on name, null if not found
    /// </summary>
    /// <param name="secretName"></param>
    /// <returns></returns>
    public Secret GetSecret(string secretName)
    { return listOfSecrets.Find(x => x.name.Equals(secretName, StringComparison.Ordinal)); }

    /// <summary>
    /// Add a new secret, checks for duplicates and won't add if one found (warning msg). Returns true if successful, false otherwise
    /// </summary>
    /// <param name="secret"></param>
    public bool AddSecret(Secret secret)
    {
        if (secret != null)
        {
            //check same secret doesn't already exist
            /*if (listOfSecrets.Exists(x => x.name == secret.name) == false)*/
            if (listOfSecrets.Exists(x => x.name.Equals(secret.name, StringComparison.Ordinal)) == false)
            {
                //check space for a new secret
                if (listOfSecrets.Count < maxNumOfSecrets)
                {
                    //add secret & make active
                    listOfSecrets.Add(secret);
                    secret.status = SecretStatus.Active;
                    secret.gainedWhen = GameManager.SetTimeStamp();
                    //Msg
                    GameManager.i.messageScript.PlayerSecret(string.Format("Player gains new secret ({0})", secret.tag), secret);
                    Debug.LogFormat("[Sec] PlayerManager.cs -> AddSecret: Player learns {0} secret, ID {1}{2}", secret.tag, secret.name, "\n");
                    return true;
                }
                else { Debug.LogWarning("Secret NOT added as not enough space"); }
            }
            else { Debug.LogWarningFormat("Duplicate secret already in list, secretID {0}", secret.name); }
        }
        return false;
    }

    /// <summary>
    /// Remove a secret from listOfSecrets. You need to set status and turnRemoved of secret prior to being removed, eg. Revealed / Deleted. Returns true if successful, false if not found
    /// </summary>
    /// <param name="secretID"></param>
    /// <returns></returns>
    public bool RemoveSecret(string secretName)
    {
        bool isSuccess = false;
        if (listOfSecrets.Exists(x => x.name.Equals(secretName, StringComparison.Ordinal)) == true)
        {
            //reverse loop through and remove secret
            for (int i = listOfSecrets.Count - 1; i >= 0; i--)
            {
                if (listOfSecrets[i].name.Equals(secretName, StringComparison.Ordinal) == true)
                {
                    Secret secret = listOfSecrets[i];
                    //reset secret known
                    secret.ResetSecret();
                    //add to correct list
                    switch (secret.status)
                    {
                        case SecretStatus.Revealed:
                            //revealed secret
                            GameManager.i.dataScript.AddRevealedSecret(secret);
                            break;
                        case SecretStatus.Deleted:
                            //deleted secret
                            secret.deletedWhen = GameManager.SetTimeStamp();
                            GameManager.i.dataScript.AddDeletedSecret(secret);
                            break;
                    }
                    //remove secret
                    listOfSecrets.RemoveAt(i);
                    isSuccess = true;
                    //admin
                    Debug.LogFormat("[Sec] PlayerManager.cs -> RemoveSecret: Player REMOVES {0} secret {1}{2}", secret.tag, secret.name, "\n");
                    break;
                }
            }
        }
        return isSuccess;
    }

    /// <summary>
    /// placeholder method to automatically assign a random secret to the player at game start
    /// </summary>
    private void GetRandomStartSecret()
    {
        List<Secret> listOfPlayerSecrets = GameManager.i.dataScript.GetListOfPlayerSecrets();
        if (listOfPlayerSecrets != null)
        {
            //clear out listOfSecrets (in case of multiple new game starts)
            listOfSecrets.Clear();
            //copy all Inactive secrets across to another list
            List<Secret> tempList = new List<Secret>();
            foreach (Secret secret in listOfPlayerSecrets)
            {
                if (secret != null)
                { tempList.Add(secret); }
                else { Debug.LogWarning("Invalid secret (Null) for listOfPlayerSecrets"); }
            }
            //find a random secret
            int numOfSecrets = tempList.Count;
            if (numOfSecrets > 0)
            {
                int index = Random.Range(0, numOfSecrets);
                Secret secret = tempList[index];
                //secret already checked for null above
                AddSecret(secret);
            }
            else { Debug.LogWarning("Invalid listOfPlayerSecrets (No records)"); }
        }
        else { Debug.LogWarning("Invalid listOfPlayerSecrets (Null)"); }
    }

    /// <summary>
    /// Uses initialSecret field to populate player secretList with a starting secret from dBase
    /// </summary>
    public void GetStartingSecret()
    {
        //get name of initial secret (if already present)
        string startSecret = "Unknown";
        if (listOfSecrets.Count > 0)
        { startSecret = listOfSecrets[0].name; }
        //clear out listOfSecrets (in case of multiple new game starts)
        listOfSecrets.Clear();
        Secret secret = GameManager.i.dataScript.GetSecret(initialSecret);
        if (secret != null)
        {
            AddSecret(secret);
            //check to prevent duplicate messages (eg. player did tutorial then started a game)
            if (startSecret.Equals(initialSecret, StringComparison.Ordinal) == false)
            { Debug.LogFormat("[Sec] PlayerManager.cs -> GetStartingSecret: player choses \"{0}\" secret{1}", secret.tag, "\n"); }
        }
        else { Debug.LogErrorFormat("Invalid secret (Null) for initialSecret \"{0}\"", initialSecret); }
    }

    /// <summary>
    /// returns a random secret from the Player's current list of secrets. Returns null if none present or a problem
    /// </summary>
    /// <returns></returns>
    public Secret GetRandomCurrentSecret()
    {
        Secret secret = null;
        int numOfSecrets = listOfSecrets.Count;
        if (numOfSecrets > 0)
        { secret = listOfSecrets[Random.Range(0, numOfSecrets)]; }
        return secret;
    }
    #endregion

    #region Investigations...
    //
    // - - - Investigations - - -
    //

    public List<Investigation> GetListOfInvestigations()
    { return listOfInvestigations; }

    /// <summary>
    /// Adds an investigation to listOfInvestigations. Checks that list isn't already maxxed out. Returns true if successful, false otherwise
    /// </summary>
    /// <param name="invest"></param>
    public bool AddInvestigation(Investigation invest)
    {
        if (invest != null)
        {
            if (listOfInvestigations.Count < maxInvestigations)
            {
                listOfInvestigations.Add(invest);
                Debug.LogFormat("[Inv] PlayerManager.cs -> AddInvestigation: Investigation \"{0}\" commenced, lead {1}, evidence {2}, ref \"{3}\"{4}", invest.tag, invest.lead, invest.evidence,
                    invest.reference, "\n");
                //update topBar
                GameManager.i.topBarScript.UpdateInvestigations(listOfInvestigations.Count);
                return true;
            }
            else { Debug.LogWarning("ListOfInvestigations is Full (Maxxed out). Record not added"); }
        }
        else { Debug.LogError("Invalid investigation (Null)"); }
        return false;
    }

    /// <summary>
    /// Removes an investigation from listOfInvestigation based on Investigation.reference (turn # + secret.name)
    /// </summary>
    /// <param name="reference"></param>
    /// <returns></returns>
    public bool RemoveInvestigation(string reference)
    {
        if (string.IsNullOrEmpty(reference) == false)
        {
            int index = listOfInvestigations.FindIndex(x => x.reference.Equals(reference, StringComparison.Ordinal));
            if (index > -1)
            {
                Debug.LogFormat("[Inv] PlayerManager.cs -> RemoveInvestigation: Investigation \"{0}\", removed{1}", reference, "\n");
                listOfInvestigations.RemoveAt(index);
                //update topBar
                GameManager.i.topBarScript.UpdateInvestigations(listOfInvestigations.Count);
                return true;
            }
            else { Debug.LogWarningFormat("Investigation reference \"{0}\" not found in listOfInvestigations", reference); }
        }
        else { Debug.LogError("Invalid reference (Null or Empty)"); }
        return false;
    }

    public int CheckNumOfInvestigations()
    { return listOfInvestigations.Count; }

    /// <summary>
    /// returns true if there is space in listOfInvestigations for another, false otherwise (eg. maxInvestigations already)
    /// </summary>
    /// <returns></returns>
    public bool CheckInvestigationPossible()
    {
        if (listOfInvestigations.Count < maxInvestigations)
        { return true; }
        return false;
    }

    /// <summary>
    /// Initialise the listOfInvestigations (overwrites existing list). Used for load save game
    /// </summary>
    /// <param name="listOfInvestigations"></param>
    public void SetListOfInvestigations(List<Investigation> listOfInvestigations)
    {
        if (listOfInvestigations != null)
        {
            this.listOfInvestigations.Clear();
            this.listOfInvestigations.AddRange(listOfInvestigations);
        }
        else { Debug.LogError("Invalid listOfInvestigations (Null)"); }
    }

    /// <summary>
    /// Investigation dropped, handles all admin, returns text string for outcome msg
    /// </summary>
    /// <param name="invest"></param>
    public string DropInvestigation(string investReference)
    {
        Investigation invest = listOfInvestigations.Find(x => x.reference.Equals(investReference, StringComparison.Ordinal));
        if (invest != null)
        {
            invest.status = InvestStatus.Completed;
            invest.outcome = InvestOutcome.Dropped;
            invest.turnFinish = GameManager.i.turnScript.Turn;
            GameManager.i.dataScript.StatisticIncrement(StatType.InvestigationsCompleted);
            GameManager.i.dataScript.StatisticIncrement(StatType.OrgHQDropped);
            //msg
            string text = string.Format("{0} investigation Dropped due to intervention by {1}{2}", invest.tag, GameManager.i.campaignScript.campaign.details.orgHQ.tag, "\n");
            GameManager.i.messageScript.InvestigationDropped(text, invest);
            //remove from player
            RemoveInvestigation(invest.reference);
            //add to listOfCompleted
            GameManager.i.dataScript.AddInvestigationCompleted(invest);
            //Add to list of services provided by orgHQ
            GameManager.i.dataScript.AddOrgData(new OrgData() { text = invest.tag, turn = GameManager.i.turnScript.Turn }, OrganisationType.HQ);
            //history
            GameManager.i.dataScript.AddHistoryPlayer(new HistoryActor() { text = string.Format("Investigation Dropped ({0})", invest.tag) });
            //return
            return GameManager.Formatt(string.Format("{0} investigation DROPPED", invest.tag), ColourType.goodText);
        }
        else { Debug.LogError("Invalid investigation (Null), not found in listOfInvestigations"); }
        return "Unknown";
    }

    /// <summary>
    /// handles all turn based investigation matters. Only does so if Player Active
    /// </summary>
    public void ProcessInvestigations()
    {
        //any active investigations
        int count = listOfInvestigations.Count;
        if (count > 0)
        {
            int rnd;
            int chance;
            int opinion;
            bool isGood;
            string text;
            //loop investigations
            for (int i = 0; i < count; i++)
            {
                Investigation invest = listOfInvestigations[i];
                if (invest != null)
                {
                    //
                    // - - - TIMER active
                    //
                    if (invest.timer > 0)
                    {
                        //decrement timer
                        invest.timer--;
                        //investigation complete
                        if (invest.timer <= 0)
                        {
                            invest.status = InvestStatus.Completed;
                            GameManager.i.dataScript.StatisticIncrement(StatType.InvestigationsCompleted);
                            string bottomText = "Unknown";
                            switch (invest.evidence)
                            {
                                case 3:
                                    //player found innocent
                                    invest.outcome = InvestOutcome.Innocent;
                                    //history
                                    GameManager.i.dataScript.AddHistoryPlayer(new HistoryActor() { text = string.Format("Investigation Resolved - INNOCENT ({0})", invest.tag) });
                                    //gain HQ approval
                                    int approvalGain = GameManager.i.playerScript.investHQApproval;
                                    GameManager.i.hqScript.ChangeHqApproval(approvalGain, GameManager.i.sideScript.PlayerSide, string.Format("{0} Investigation", invest.tag));
                                    Debug.LogFormat("[Inv] PlayerManager.cs -> ProcessInvestigation: Investigation \"{0}\" completed. Player found INNOCENT{1}", invest.tag, "\n");
                                    bottomText = string.Format("You are{0}<size=120%>{1}</size>{2}of all charges", "\n", GameManager.Formatt("INNOCENT", ColourType.goodText), "\n");
                                    break;
                                case 0:
                                    //player found guilty
                                    invest.outcome = InvestOutcome.Guilty;
                                    Debug.LogFormat("[Inv] PlayerManager.cs -> ProcessInvestigation: Investigation \"{0}\" completed. Player found GUILTY{1}", invest.tag, "\n");
                                    WinStateLevel winner = WinStateLevel.None;
                                    switch (GameManager.i.sideScript.PlayerSide.level)
                                    {
                                        case 1: winner = WinStateLevel.Resistance; break;
                                        case 2: winner = WinStateLevel.Authority; break;
                                        default: Debug.LogWarningFormat("Unrecognised Player side {0}", GameManager.i.sideScript.PlayerSide.name); break;
                                    }
                                    GameManager.i.turnScript.SetWinStateLevel(winner, WinReasonLevel.Investigation, "Player found Guilty", string.Format("{0} Investigation", invest.tag));
                                    //black marks
                                    GameManager.i.campaignScript.ChangeBlackmarks(GameManager.i.campaignScript.GetInvestigationBlackmarks(), string.Format("{0} Investigation", invest.tag));
                                    //history
                                    GameManager.i.dataScript.AddHistoryPlayer(new HistoryActor() { text = string.Format("Investigation Resolved - GUILTY ({0})", invest.tag) });

                                    /*//increase cost in blackMarks for future investigations EDIT -> fixed cost of 1 blackmark per investigation
                                    GameManager.instance.campaignScript.IncrementInvestigationBlackmarks();*/

                                    bottomText = string.Format("You have been found{0}<size=120%>{1}</size>{2}on all charges", "\n",
                                        GameManager.Formatt("GUILTY", ColourType.badText), "\n");
                                    break;
                                default: Debug.LogWarningFormat("Unrecognised invest.evidence \"{0}\"", invest.evidence); break;
                            }
                            //outcome (message pipeline)
                            text = string.Format("<size=120%>INVESTIGATION</size>{0}Completed into your{1}{2}", "\n", "\n", GameManager.Formatt(invest.tag, ColourType.neutralText));
                            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
                            {
                                textTop = text,
                                textBottom = bottomText,
                                sprite = GameManager.i.spriteScript.investigationSprite,
                                isAction = false,
                                side = GameManager.i.sideScript.PlayerSide,
                                type = MsgPipelineType.InvestigationCompleted,
                                help0 = "invest_8",
                                help1 = "invest_9"
                            };
                            if (GameManager.i.guiScript.InfoPipelineAdd(outcomeDetails) == false)
                            { Debug.LogWarningFormat("Investigation Completed InfoPipeline message FAILED to be added to dictOfPipeline"); }

                            //end investigation
                            invest.turnFinish = GameManager.i.turnScript.Turn;
                            RemoveInvestigation(invest.reference);

                            //msg
                            text = string.Format("{0} Investigation completed. {1} Verdict", invest.tag, invest.evidence == 3 ? "Innocent" : "Guilty");
                            GameManager.i.messageScript.InvestigationCompleted(text, invest);
                            //add to listOfCompleted
                            GameManager.i.dataScript.AddInvestigationCompleted(invest);
                        }
                        else
                        {
                            Debug.LogFormat("[Inv] PlayerManager.cs -> ProcessInvestigations: Investigation \"{0}\" is {1} day{2} away from a Player {3} conclusion (evidence {4}){5}", invest.tag, invest.timer,
                                invest.timer != 1 ? "s" : "", invest.evidence == 3 ? "INNOCENT" : "GUILTY", invest.evidence, "\n");
                            text = string.Format("{0} Investigation counting down to a Resolution", invest.tag);
                            GameManager.i.messageScript.InvestigationResolution(text, invest);
                            //update topBarUI tooltip
                            GameManager.i.topBarScript.UpdateInvestigations(count);
                        }
                    }
                    else
                    {
                        //
                        // - - - No timer -> check for new evidence (not when investigation first launched)
                        //
                        if (invest.turnStart + 1 < GameManager.i.turnScript.Turn)
                        {
                            rnd = Random.Range(0, 100);
                            isGood = false;
                            if (rnd < chanceEvidence)
                            {
                                Debug.LogFormat("[Rnd] PlayerManager.cs -> ProcessInvestigations: {0} Investigation new EVIDENCE, need {1}, rolled {2}{3}", invest.tag, chanceEvidence, rnd, "\n");
                                //previous
                                invest.previousEvidence = invest.evidence;
                                //good or bad evidence -> depends on HQ actor opinion of you
                                Actor actor = GameManager.i.dataScript.GetHqHierarchyActor(invest.lead);
                                if (actor != null)
                                {
                                    rnd = Random.Range(0, 100);
                                    opinion = actor.GetDatapoint(ActorDatapoint.Opinion1);
                                    switch (opinion)
                                    {
                                        case 3: chance = 80; break;
                                        case 2: chance = 60; break;
                                        case 1: chance = 40; break;
                                        case 0: chance = 20; break;
                                        default: Debug.LogWarningFormat("Unrecognised actor Opinion \"{0}\" for {1}", invest.lead); chance = 0; break;
                                    }
                                    if (rnd < chance)
                                    {
                                        isGood = true;
                                        Debug.LogFormat("[Inv] PlayerManager.cs -> ProcessInvestigation: Investigation \"{0}\", new evidence, Good (needed {1}, rolled {2}), {3} Mot {4}, Ev {5}{6}",
                                            invest.tag, chance, rnd, invest.lead, opinion, invest.evidence + 1, "\n");
                                    }
                                    else
                                    {
                                        Debug.LogFormat("[Inv] PlayerManager.cs -> ProcessInvestigation: Investigation \"{0}\", new evidence, Bad (needed {1}, rolled {2}), {3} Mot {4}, Ev {5}{6}",
                                            invest.tag, chance, rnd, invest.lead, opinion, invest.evidence, "\n");
                                    }
                                    text = string.Format("{0} Evidence Uncovered", invest.tag);
                                    GameManager.i.messageScript.GeneralRandom(text, "Type of Evidence", chance, rnd, false, "rand_5");
                                }
                                else
                                { Debug.LogWarningFormat("Invalid HQ Actor (Null) for investigation.lead {0}", invest.lead); }
                                //apply evidence
                                if (isGood == true)
                                {
                                    //Xonerating Evidence (good)
                                    invest.evidence++;
                                    //exceed max
                                    if (invest.evidence > 3)
                                    {
                                        //start timer
                                        invest.timer = timerInvestigationBase;
                                        invest.status = InvestStatus.Resolution;
                                        Debug.LogFormat("[Inv] PlayerManager.cs -> ProcessInvestigation: Investigation \"{0}\" is reaching a conclusion. Player will be found INNOCENT in {1} turn{2}",
                                            invest.tag, invest.timer, "\n");
                                    }
                                }
                                else
                                {
                                    //Incriminating Evidence (bad)
                                    invest.evidence--;
                                    //exceeds min
                                    if (invest.evidence < 0)
                                    {
                                        //start timer
                                        invest.timer = timerInvestigationBase;
                                        invest.status = InvestStatus.Resolution;
                                        Debug.LogFormat("[Inv] PlayerManager.cs -> ProcessInvestigation: Investigation \"{0}\" is reaching a conclusion. Player will be found GUILTY in {1} turn{2}",
                                            invest.tag, invest.timer, "\n");
                                    }
                                }
                                //ensure value doesn't exceed boundary
                                invest.evidence = Mathf.Clamp(invest.evidence, 0, 3);
                                //evidence message
                                text = string.Format("{0} Investigation uncovers new Evidence (was {1}, now {2})", invest.tag, invest.previousEvidence, invest.evidence);
                                GameManager.i.messageScript.InvestigationEvidence(text, invest, "your Lead Investigator");
                                //update topBarUI (tooltip needs to change to reflect new evidence)
                                GameManager.i.topBarScript.UpdateInvestigations(count);
                            }
                        }
                        //effects tab msg
                        text = string.Format("Ongoing investigation into your {0}", invest.tag);
                        GameManager.i.messageScript.InvestigationOngoing(text, invest);
                    }
                }
                else { Debug.LogErrorFormat("Invalid investigation (Null) for listOfInvestigations[{0}]", i); }
            }
        }
    }

    /// <summary>
    /// returns true if there is at least one current investigation in the normal phase (resolution timer hasn't commenced, eg. status.Ongoing) and orgHQ hasn't yet intervened (isOrgNormal false)
    /// </summary>
    public bool CheckInvestigationNormal()
    {
        //any active investigations
        int count = listOfInvestigations.Count;
        if (count > 0)
        {
            //loop investigations looking for the first positive match
            for (int i = 0; i < count; i++)
            {
                Investigation invest = listOfInvestigations[i];
                if (invest != null)
                {
                    if (invest.status == InvestStatus.Ongoing)
                    {
                        if (invest.isOrgHQNormal == false)
                        { return true; }
                    }
                }
                else { Debug.LogWarningFormat("Invalid investigation Normal (Null) for listOfInvestigations[{0}]", i); }
            }
        }
        return false;
    }

    /// <summary>
    /// returns true if there is at least one current investigation in the resolution phase (eg. status Resolution), with an imminent GUILTY verdict, where orgHQ hasn't yet intervented (isOrgTimer false)
    /// </summary>
    public bool CheckInvestigationTimer()
    {
        //any active investigations
        int count = listOfInvestigations.Count;
        if (count > 0)
        {
            //loop investigations looking for the first positive match
            for (int i = 0; i < count; i++)
            {
                Investigation invest = listOfInvestigations[i];
                if (invest != null)
                {
                    if (invest.status == InvestStatus.Resolution)
                    {
                        if (invest.isOrgHQTimer == false)
                        {
                            if (invest.outcome == InvestOutcome.Guilty)
                            { return true; }
                        }
                    }
                }
                else { Debug.LogWarningFormat("Invalid investigation Timer (Null) for listOfInvestigations[{0}]", i); }
            }
        }
        return false;
    }

    /// <summary>
    /// returns investigation for a current investigation, status.Ongoing, isOrgHQNormal false, returns null if none
    /// </summary>
    /// <returns></returns>
    public Investigation GetInvestigationNormal()
    {
        //any active investigations
        int count = listOfInvestigations.Count;
        if (count > 0)
        {
            //loop investigations looking for the first positive match
            for (int i = 0; i < count; i++)
            {
                Investigation invest = listOfInvestigations[i];
                if (invest != null)
                {
                    if (invest.status == InvestStatus.Ongoing)
                    {
                        if (invest.isOrgHQNormal == false)
                        { return invest; }
                    }
                }
                else { Debug.LogWarningFormat("Invalid investigation Normal (Null) for listOfInvestigations[{0}]", i); }
            }
        }
        return null;
    }

    /// <summary>
    /// returns investigation for a current investigation, status.Resolution, guilty verdict imminent, isOrgHQTimer false. Returns null if none
    /// </summary>
    /// <returns></returns>
    public Investigation GetInvestigationTimer()
    {
        //any active investigations
        int count = listOfInvestigations.Count;
        if (count > 0)
        {
            //loop investigations looking for the first positive match
            for (int i = 0; i < count; i++)
            {
                Investigation invest = listOfInvestigations[i];
                if (invest != null)
                {
                    if (invest.status == InvestStatus.Resolution)
                    {
                        if (invest.isOrgHQTimer == false)
                        {
                            if (invest.outcome == InvestOutcome.Guilty)
                            { return invest; }
                        }
                    }
                }
                else { Debug.LogWarningFormat("Invalid investigation Timer (Null) for listOfInvestigations[{0}]", i); }
            }
        }
        return null;
    }

    /// <summary>
    /// Sets 'isOrgHQNormal' flag true to signify that orgHQ asked player if they wanted investigation dropped (OrgHQ topics 0/1), regardless of outcome, to prevent repeat question
    /// </summary>
    /// <param name="invest"></param>
    public string SetInvestigationFlagNormal(string investigationReference)
    {
        Investigation investigation = listOfInvestigations.Find(x => x.reference.Equals(investigationReference, StringComparison.Ordinal));
        if (investigation != null)
        { investigation.isOrgHQNormal = true; }
        else { Debug.LogWarningFormat("Investigation not found in listOfInvestigations (invest.reference \"{0}\")", investigationReference); }
        return "Offer has been made";
    }

    /// <summary>
    /// Sets 'isOrgHQTimer' flag true to signify that orgHQ asked player if they wanted investigation dropped (OrgHQ topics 7/8), regardless of outcome, to prevent repeat question
    /// </summary>
    /// <param name="invest"></param>
    public string SetInvestigationFlagTimer(string investigationReference)
    {
        Investigation investigation = listOfInvestigations.Find(x => x.reference.Equals(investigationReference, StringComparison.Ordinal));
        if (investigation != null)
        { investigation.isOrgHQTimer = true; }
        else { Debug.LogWarningFormat("Investigation not found in listOfInvestigations (invest.reference \"{0}\")", investigationReference); }
        return "Offer has been made";
    }

    /// <summary>
    /// returns colour formatted tooltip for current investigations (if any) in format 'Investigation.tag'/'Evidence + GetStars()'/lead Investigator/Verdict timer for each, or, if none, "No current investigations"
    /// </summary>
    /// <returns></returns>
    public string GetInvestigationTooltip()
    {
        string text = "No current Investigations";

        if (listOfInvestigations.Count > 0)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < listOfInvestigations.Count; i++)
            {
                Investigation investigation = listOfInvestigations[i];
                if (investigation != null)
                {
                    if (builder.Length > 0) { builder.AppendLine(); }
                    builder.AppendFormat("{0}{1}", GameManager.Formatt(investigation.tag, ColourType.neutralText), "\n");
                    builder.AppendFormat("Evidence  {0}{1}", GameManager.i.guiScript.GetNormalStars(investigation.evidence), "\n");
                    builder.AppendFormat("{0} is lead", GameManager.Formatt(GameManager.i.hqScript.GetHqTitle(investigation.lead).ToUpper(), ColourType.salmonText));
                    if (investigation.timer > 0)
                    {
                        string textVerdict = string.Format("{0}Verdict in {1} day{2}", "\n", investigation.timer, investigation.timer != 1 ? "s" : "");
                        builder.AppendFormat(GameManager.Formatt(textVerdict, investigation.evidence == 3 ? ColourType.goodText : ColourType.badText));
                    }
                }
                else { Debug.LogWarningFormat("Invalid investigation (Null) for listOfInvestigations[i]", i); }
            }
            text = builder.ToString();
        }
        return text;
    }
    #endregion

    #region Debug...
    //
    // - - - Debug - - -
    //

    /// <summary>
    /// Debug method to add a made up investigation to the player
    /// </summary>
    public void DebugAddInvestigation()
    {
        int turn = GameManager.i.turnScript.Turn;
        Investigation investigation = new Investigation()
        {
            reference = $"{turn}Debug",
            city = GameManager.i.cityScript.GetCityName(),
            turnStart = turn
        };
        switch (Random.Range(0, 3))
        {
            case 0: investigation.tag = "Bedwetting Drama"; break;
            case 1: investigation.tag = "Rats in the Ranks"; break;
            case 2: investigation.tag = "Born a Bastard"; break;
        }
        investigation.lead = (ActorHQ)Random.Range(1, 5);
        if (Random.Range(0, 100) < 50)
        {
            investigation.status = InvestStatus.Ongoing;
            investigation.evidence = Random.Range(0, 4);
        }
        else
        {
            investigation.status = InvestStatus.Resolution;
            if (Random.Range(0, 100) < 50)
            {
                investigation.evidence = 3;
                investigation.outcome = InvestOutcome.Innocent;
            }
            else
            {
                investigation.evidence = 0;
                investigation.outcome = InvestOutcome.Guilty;
            }
            investigation.timer = Random.Range(1, 10);
        }
        AddInvestigation(investigation);
    }

    /// <summary>
    /// DEBUG method to show players gear in lieu of a working UI element
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayGear()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(" Player's Gear");
        builder.AppendLine();
        foreach (string gearName in listOfGear)
        {
            Gear gear = GameManager.i.dataScript.GetGear(gearName);
            if (gear != null)
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.Append(string.Format(" {0}", gear.tag.ToUpper()));
                builder.AppendLine();
                builder.AppendLine();
                builder.Append(string.Format(" rarity \"{0}\"", gear.rarity.name));
                builder.AppendLine();
                builder.Append(string.Format(" gearType \"{0}\"", gear.type.name));
                builder.AppendLine();
                builder.Append(string.Format(" data {0}", gear.data));
            }
        }
        return builder.ToString();
    }

    /// <summary>
    /// Debug function to manually add gear to the player's inventory.Returns a message to display
    /// </summary>
    /// <param name="gearname"></param>
    public string DebugAddGear(string gearName)
    {
        string text = string.Format("{0} has NOT been added to the Player's inventory{1}Press ESC to exit", gearName, "\n");
        if (listOfGear.Count < GameManager.i.gearScript.maxNumOfGear)
        {
            if (string.IsNullOrEmpty(gearName) == false)
            {
                //find gear in dictionary
                Gear gear = GameManager.i.dataScript.GetGear(gearName);
                if (gear != null)
                {
                    //check you aren't adding gear that has already been lost
                    List<string> listOfLostGear = GameManager.i.dataScript.GetListOfLostGear();
                    if (listOfLostGear.Exists(x => x == gear.name) == false)
                    {
                        //add gear to player's inventory
                        if (AddGear(gearName) == true)
                        {
                            //remove from pool
                            if (GameManager.i.dataScript.RemoveGearFromPool(gear) == false)
                            { Debug.LogWarning("Gear not removed from Pool (Null or other problem)"); }
                            text = string.Format("{0} has been added to the Player's inventory{1}Press ESC to exit", gearName, "\n");
                            //message
                            Node nodePlayer = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.GetPlayerNodeID());
                            GameManager.i.messageScript.GearObtained(string.Format("{0} added (DEBUG)", gearName), nodePlayer, gear);
                        }
                    }
                    else
                    { text = string.Format("{0} NOT added as is LOST GEAR{1}Press ESC to exit", gearName, "\n"); }
                }
            }
        }
        else
        {
            //gear inventory already full
            text = string.Format("{0} NOT added as Gear Allowance MAXXED{1}Press ESC to exit", gearName, "\n");
            /*Debug.LogWarning("You cannot exceed the maximum num of Gear -> gear not added");*/
        }
        return text;
    }

    /// <summary>
    /// Assigns a random player secret to Player
    /// </summary>
    public void DebugAddRandomSecret()
    {
        //give the player a random secret
        List<Secret> listOfSecrets = GameManager.i.dataScript.GetListOfPlayerSecrets();
        if (listOfSecrets != null)
        {
            List<Secret> tempList = new List<Secret>();
            //put all inactive player secrets into the list to enable a random pick
            foreach (var secret in listOfSecrets)
            {
                if (secret.status == gameAPI.SecretStatus.Inactive)
                { tempList.Add(secret); }
            }
            //make a random choice
            if (tempList.Count > 0)
            {
                Secret secret = tempList[Random.Range(0, tempList.Count)];
                if (secret != null)
                {
                    //add to player (AddSecret method will change secret status)
                    AddSecret(secret);
                }
            }
            else { Debug.LogWarning("No entries in tempList of Secrets"); }
        }
        else { Debug.LogWarning("Invalid dictOfSecrets (Null)"); }
    }

    /// <summary>
    /// Removes a random secret from player, and anyone else who knows it. Handles all admin. Secret treated as deleted but effects are ignored
    /// </summary>
    public void DebugRemoveRandomSecret()
    {
        int count = listOfSecrets.Count;
        if (count > 0)
        {
            Secret secret = listOfSecrets[Random.Range(0, count)];
            if (secret != null)
            {
                secret.status = SecretStatus.Deleted;
                secret.deletedWhen = GameManager.SetTimeStamp();
                GameManager.i.secretScript.RemoveSecretFromAll(secret.name);
            }
            else { Debug.LogError("Invalid secret (Null)"); }
        }
    }

    /// <summary>
    /// Display contents of listOfInvestigations
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayInvestigations()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("-Investigations{0}", "\n");
        builder.Append(DebugDisplayInvestigationList(listOfInvestigations));
        //completed investigations
        builder.AppendFormat("{0}{1}-ListOfCompletedInvestigations{2}", "\n", "\n", "\n");
        List<Investigation> listOfCompletedInvestigations = GameManager.i.dataScript.GetListOfCompletedInvestigations();
        if (listOfCompletedInvestigations != null)
        { builder.Append(DebugDisplayInvestigationList(listOfCompletedInvestigations)); }
        //Commendations
        List<AwardData> listOfCommendations = GameManager.i.dataScript.GetListOfCommendations();
        if (listOfCommendations != null)
        {
            builder.AppendFormat("{0}{1}-ListOfCommendations{2}", "\n", "\n", "\n");
            if (listOfCommendations.Count > 0)
            {
                for (int i = 0; i < listOfCommendations.Count; i++)
                {
                    AwardData data = listOfCommendations[i];
                    builder.AppendFormat(" t{0}: {1}{2}", data.turn, data.reason, "\n");
                }
            }
            else { builder.AppendFormat(" No Commendations have been awarded{0}", "\n"); }
        }
        else { Debug.LogError("Invalid listOfCommendations (Null)"); }
        //Blackmarks
        List<AwardData> listOfBlackmarks = GameManager.i.dataScript.GetListOfBlackmarks();
        if (listOfBlackmarks != null)
        {
            builder.AppendFormat("{0}-ListOfBlackmarks{1}", "\n", "\n");
            if (listOfBlackmarks.Count > 0)
            {
                for (int i = 0; i < listOfBlackmarks.Count; i++)
                {
                    AwardData data = listOfBlackmarks[i];
                    builder.AppendFormat(" t{0}: {1}{2}", data.turn, data.reason, "\n");
                }
            }
            else { builder.AppendFormat(" No Blackmarks have been awarded{0}", "\n"); }
        }
        else { Debug.LogError("Invalid listOfBlackmarks (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// Submethod for DebugDisplayInvestigations
    /// </summary>
    /// <param name="listOfInvest"></param>
    /// <returns></returns>
    private string DebugDisplayInvestigationList(List<Investigation> listOfInvest)
    {
        StringBuilder builder = new StringBuilder();
        int count = listOfInvest.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                Investigation invest = listOfInvest[i];
                if (invest != null)
                {
                    if (builder.Length > 0) { builder.AppendLine(); }
                    builder.AppendFormat(" Investigating {0} at {1}{2}", invest.tag, invest.city, "\n");
                    builder.AppendFormat("  reference: {0}{1}", invest.reference, "\n");
                    builder.AppendFormat("  evidence: {0}{1}", invest.evidence, "\n");
                    builder.AppendFormat("  lead: {0}{1}", invest.lead, "\n");
                    builder.AppendFormat("  turns: start {0}, finish {1}{2}", invest.turnStart, invest.turnFinish, "\n");
                    builder.AppendFormat("  timer: {0}{1}", invest.timer, "\n");
                    builder.AppendFormat("  status: {0}, outcome {1}{2}", invest.status, invest.outcome, "\n");
                }
                else { builder.Append(" Invalid investigation (Null)"); }
            }
        }
        else { builder.Append(" No Investigations present"); }
        return builder.ToString();
    }


    /// <summary>
    /// return a list of all secretsto SecretManager.cs -> DisplaySecretData for Debug display
    /// </summary>
    /// <returns></returns>
    public string DebugDisplaySecrets()
    {
        StringBuilder builder = new StringBuilder();
        if (listOfSecrets.Count > 0)
        {
            foreach (Secret secret in listOfSecrets)
            { builder.AppendFormat("{0} ID {1}, {2} ({3}), {4}, Known: {5}", "\n", secret.name, secret.name, secret.tag, secret.status, secret.CheckNumOfActorsWhoKnow()); }
        }
        else { builder.AppendFormat("{0} No records", "\n"); }
        return builder.ToString();
    }

    /// <summary>
    /// Debug function to display all player related stats
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayPlayerStats()
    {
        GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
        //use correct list for the player side
        List<Condition> listOfConditions = GetListOfConditionForSide(playerSide);
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- {0} Player Stats{1}{2}", playerSide.name, "\n", "\n");
        builder.AppendFormat(" Sex {0}{1}", sex, "\n");
        builder.AppendFormat(" {0}, first name \"{1}\"{2}{3}", PlayerName, FirstName, "\n", "\n");
        builder.AppendFormat("- Stats{0}", "\n");
        if (playerSide.level == globalResistance.level)
        { builder.AppendFormat(" Invisibility {0}{1}", Invisibility, "\n"); }
        builder.AppendFormat(" Power {0}{1}", Power, "\n");
        builder.AppendFormat(" Mood {0}{1}", mood, "\n");
        builder.AppendFormat(" Innocence {0}{1}", Innocence, "\n");
        if (GameManager.i.actorScript.doomTimer > 0) { builder.AppendFormat(" Doom Timer {0}{1}", GameManager.i.actorScript.doomTimer, "\n"); }
        //Backstory
        builder.AppendFormat("{0}- Backstory{1}", "\n", "\n");
        builder.AppendFormat(" Previous Job: {0}{1}", previousJob, "\n");
        builder.AppendFormat(" Favourite Pet: {0}{1}", pet, "\n");
        builder.AppendFormat(" Pet Name: {0}{1}", petName, "\n");
        builder.AppendFormat(" Initial Secret: {0}{1}", initialSecret, "\n");
        //Conditions
        if (listOfConditions != null)
        {
            builder.AppendFormat("{0}- Conditions{1}", "\n", "\n");
            if (listOfConditions.Count > 0)
            {
                for (int i = 0; i < listOfConditions.Count; i++)
                { builder.AppendFormat(" {0}{1}", listOfConditions[i].name, "\n"); }
            }
            else { builder.AppendFormat(" None{0}", "\n"); }
        }
        else { Debug.LogError("Invalid listOfConditions (Null)"); }
        //Cures
        List<Node> listOfCures = GameManager.i.dataScript.GetListOfCureNodes();
        if (listOfCures != null)
        {
            builder.AppendFormat("{0}- Cures{1}", "\n", "\n");
            if (listOfCures.Count > 0)
            {
                for (int i = 0; i < listOfCures.Count; i++)
                {
                    builder.AppendFormat(" {0} at {1}, {2}, ID {3}{4}", listOfCures[i].cure.cureName, listOfCures[i].nodeName, listOfCures[i].Arc.name, listOfCures[i].nodeID, "\n");
                    builder.AppendFormat("  isActive {0}, isOrgActivated {1}{2}", listOfCures[i].cure.isActive, listOfCures[i].cure.isOrgActivated, "\n");
                }
            }
            else { builder.AppendFormat(" None{0}", "\n"); }
        }
        else { Debug.LogError("Invalid listOfCures (Null)"); }
        builder.AppendFormat("{0}- States{1}", "\n", "\n");
        builder.AppendFormat(" Status {0}{1}", Status, "\n");
        builder.AppendFormat(" InactiveStatus {0}{1}", InactiveStatus, "\n");
        builder.AppendFormat(" showPlayerNode {0}{1}", GameManager.i.nodeScript.GetShowPlayerNode(), "\n");
        builder.AppendFormat(" TooltipStatus {0}{1}", tooltipStatus, "\n");
        builder.AppendFormat(" isBreakdown {0}{1}", isBreakdown, "\n");
        builder.AppendFormat(" isEndOfTurnGearCheck {0}{1}", isEndOfTurnGearCheck, "\n");
        builder.AppendFormat(" isLieLowFirstTurn {0}{1}", isLieLowFirstturn, "\n");
        builder.AppendFormat(" isStressLeave {0}{1}", isStressLeave, "\n");
        builder.AppendFormat(" isStressed {0}{1}", isStressed, "\n");
        builder.AppendFormat(" isSpecialMoveGear {0}{1}", isSpecialMoveGear, "\n");
        builder.AppendFormat(" numOfSuperStressed {0}{1}", numOfSuperStress, "\n");
        builder.AppendFormat(" stressImmunityStart {0}{1}", stressImmunityStart, "\n");
        builder.AppendFormat(" stressImmunityCurrent {0}{1}", stressImmunityCurrent, "\n");
        builder.AppendFormat(" addictedTally {0}{1}", addictedTally, "\n");
        builder.AppendFormat("{0} -Global{1}", "\n", "\n");
        builder.AppendFormat(" authorityState {0}{1}", GameManager.i.turnScript.authoritySecurityState, "\n");
        builder.AppendFormat("{0} -Reserve Pool{1}", "\n", "\n");
        builder.AppendFormat(" NumOfRecruits {0} + {1}{2}", GameManager.i.dataScript.CheckNumOfOnMapActors(playerSide), GameManager.i.dataScript.CheckNumOfActorsInReserve(), "\n");
        if (playerSide.level == globalResistance.level)
        {
            //gear
            builder.AppendFormat("{0}-Gear{1}", "\n", "\n");
            if (listOfGear.Count > 0)
            {
                for (int i = 0; i < listOfGear.Count; i++)
                {
                    Gear gear = GameManager.i.dataScript.GetGear(listOfGear[i]);
                    if (gear != null)
                    { builder.AppendFormat(" {0}, {1}{2}", gear.tag, gear.type.name, "\n"); }
                }
            }
            else { builder.Append(" No gear in inventory"); }
        }
        //stats
        builder.AppendFormat("{0}{1} -Stats{2}", "\n", "\n", "\n");
        builder.AppendFormat(" breakdowns: {0}{1}", GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerBreakdown), "\n");
        builder.AppendFormat(" lie low: {0}{1}", GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerLieLowTimes), "\n");
        //Capture Tools
        builder.AppendFormat("{0}-Capture Tools{1}", "\n", "\n");
        int count = 0;
        for (int index = 0; index < arrayOfCaptureTools.Length; index++)
        {
            if (arrayOfCaptureTools[index] == true)
            {
                CaptureTool tool = GameManager.i.captureScript.GetCaptureTool(index);
                if (tool != null)
                { builder.AppendFormat(" {0}, level {1}{2}", tool.tag, tool.innocenceLevel, "\n"); }
                else { builder.AppendFormat(" Invalid captureTool (NULL){0}", "\n"); }
                count++;
            }
        }
        if (count == 0) { builder.AppendFormat(" None present{0}", "\n"); }
        return builder.ToString();
    }


    /// <summary>
    /// display mood history
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayMoodHistory()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- Mood history{0}", "\n");
        foreach (HistoryMood history in listOfMoodHistory)
        { builder.AppendFormat(" {0}{1}", history.descriptor, "\n"); }
        return builder.ToString();
    }

    /// <summary>
    /// Debug method to create sample mood history data set
    /// </summary>
    public void DebugCreateMoodHistory()
    {
        int change = 1;
        string reason = "Promote ";
        string factor = "Unknown";
        //create one random mood history per turn
        if (Random.Range(0, 100) < 50)
        {
            change = -1;
            reason = "Dismiss ";
        }
        reason = reason + GameManager.i.dataScript.GetRandomActorArcs(1, GameManager.i.sideScript.PlayerSide)[0].name;
        factor = GameManager.i.dataScript.DebugGetRandomFactor();
        ChangeMood(change, reason, factor);
    }

    /// <summary>
    /// Adds a CaptureTool to the Player's inventory for the specified innocence Level
    /// </summary>
    /// <param name="innocenceString"></param>
    /// <returns></returns>
    public string DebugAddCaptureTool(string innocenceString)
    {
        string reply = "Error";
        int innocenceLevel = -1;
        if (string.IsNullOrEmpty(innocenceString) == false)
        {
            //convert string to int
            try { innocenceLevel = System.Convert.ToInt32(innocenceString); }
            catch (System.OverflowException)
            { Debug.LogErrorFormat("Invalid conversion for innocenceString \"{0}\"", innocenceString); }
            //add Capture tool (called method will handle invalid innocentLevel)
            if (AddCaptureTool(innocenceLevel) == true)
            {
                CaptureTool tool = GameManager.i.captureScript.GetCaptureTool(innocenceLevel);
                if (tool != null)
                { reply = $"{tool.tag} added"; }
                else { Debug.LogErrorFormat("Invalid CaptureTool (Null) for innocenceLevel \"{0}\"", innocenceLevel); }
            }
        }
        else { Debug.LogError("Invalid innoceneString (Null or Empty)"); }
        return reply;
    }

    /// <summary>
    /// Sets player innocence level
    /// </summary>
    /// <param name="innocenceString"></param>
    /// <returns></returns>
    public string DebugSetInnocence(string innocenceString)
    {
        string reply = "Error";
        int innocenceLevel = -1;
        if (string.IsNullOrEmpty(innocenceString) == false)
        {
            //convert string to int
            try { innocenceLevel = System.Convert.ToInt32(innocenceString); }
            catch (System.OverflowException)
            { Debug.LogErrorFormat("Invalid conversion for innocenceString \"{0}\"", innocenceString); }
            //Set Innocence (called method will handle invalid innocentLevel)
            Innocence = innocenceLevel;
            reply = $"Player Innocence now {innocenceLevel}";
        }
        else { Debug.LogError("Invalid innocenceString (Null or Empty)"); }
        return reply;
    }


    /// <summary>
    /// Sets player Mood
    /// </summary>
    /// <param name="moodString"></param>
    /// <returns></returns>
    public string DebugSetMood(string moodString)
    {
        string reply = "Error";
        int moodLevel = -1;
        if (string.IsNullOrEmpty(moodString) == false)
        {
            //convert string to int
            try { moodLevel = System.Convert.ToInt32(moodString); }
            catch (System.OverflowException)
            { Debug.LogErrorFormat("Invalid conversion for moodString \"{0}\"", moodString); }
            //Set Mood -> method will handle invalid input
            int change = moodLevel - GetMood();
            ChangeMood(change, "Debug", "Debug");
            reply = $"Player Mood now {GetMood()}";
            //Popup
            GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, string.Format("Mood {0}{1}", change > 0 ? "+" : "", change));
            GameManager.i.popUpFixedScript.ExecuteFixed();
        }
        else { Debug.LogError("Invalid moodString (Null or Empty)"); }
        return reply;
    }

    /// <summary>
    /// Gives player +10 Power
    /// </summary>
    public void DebugGivePower()
    {
        Power += 10;
        GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, "Power +10");
        GameManager.i.popUpFixedScript.ExecuteFixed();
    }
    #endregion
    
    #region Names...
    //
    // - - - Names - - -
    //


    public void SetPlayerNameAuthority(string playerName)
    {
        if (string.IsNullOrEmpty(playerName) == false)
        { _playerNameAuthority = playerName; }
        else { Debug.LogError("Invalid Authority playerName (Null or Empty)"); }
    }

    public void SetPlayerNameResistance(string playerName)
    {
        if (string.IsNullOrEmpty(playerName) == false)
        { _playerNameResistance = playerName; }
        else { Debug.LogError("Invalid Resistance playerName (Null or Empty)"); }
    }

    public void SetPlayerFirstName(string firstName)
    {
        if (string.IsNullOrEmpty(firstName) == false)
        { _firstName = firstName; }
        else { Debug.LogError("Invalid Player First Name (Null or Empty)"); }
    }

    /// <summary>
    /// returns name of Authority Player regardless of whether it is AI or Human controlled
    /// </summary>
    /// <returns></returns>
    public string GetPlayerNameAuthority()
    { return _playerNameAuthority; }

    /// <summary>
    /// returns name of Resistance Player regardless of whether it is AI or Human controlled
    /// </summary>
    /// <returns></returns>
    public string GetPlayerNameResistance()
    { return _playerNameResistance; }

    /// <summary>
    /// returns name of specific side player regardless of whether it is AI or Human controlled
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public string GetPlayerName(GlobalSide side)
    {
        if (side != null)
        {
            if (side.level == 1)
            {
                //Authority
                return _playerNameAuthority;
            }
            else if (side.level == 2)
            {
                //Resistance
                return _playerNameResistance;
            }
            else { Debug.LogErrorFormat("Invalid side \"{0}\"", side.name); }
        }
        else { Debug.LogError("Invalid side (Null)"); }
        return "Unknown Name";
    }

    #endregion

    #region Personality and Mood...
    //
    // - - - Personality & mood
    //

    public Personality GetPersonality()
    { return personality; }

    public int GetMood()
    { return mood; }

    /// <summary>
    /// Main method to change mood. Give a reason (keep short, eg. "Dismissed FIXER") and the name of the Factor that applied). Auto adds a history record. Auto displays popUp unless isPopUp set to false 
    /// NOTE: You only want to call this method if there is definite change (eg. not Zero)
    /// NOTE: No changes are possible while player is STRESSED
    /// </summary>
    /// <param name="change"></param>
    /// <param name="reason"></param>
    /// <param name="factor"></param>
    public void ChangeMood(int change, string reason, string factor, bool isPopUp = true)
    {
        string text = "Unknown";
        GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
        //check player isn't currently stressed
        if (isStressed == false)
        {
            Debug.Assert(change != 0, "Invalid change (no point of it's Zero)");
            if (string.IsNullOrEmpty(reason) == true)
            {
                reason = "Unknown";
                Debug.LogWarning("Invalid reason (Null or Empty)");
            }
            //factor can be EMPTY but not Null
            if (factor == null)
            {
                factor = "Unknown";
                Debug.LogWarning("Invalid factor (Null or Empty)");
            }
            //update mood
            bool isStressed = false;
            mood += change;
            if (isPopUp == true)
            { GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, string.Format("Mood {0}{1}", change > 0 ? "+" : "", change)); }
            if (mood < 0)
            {
                //player immune to stress
                if (stressImmunityCurrent <= 0)
                {
                    //player gains stressed condition
                    AddCondition(conditionStressed, playerSide, "Mood drops below Zero");
                    isStressed = true;
                }
                text = string.Format("[Msg] Player did NOT become STRESSED due to drugs, currentImmunity {0}, startImmunity {1}, isAddicted {2}{3}", stressImmunityCurrent, stressImmunityStart, isAddicted, "\n");
                GameManager.i.messageScript.PlayerImmuneStress(text, stressImmunityCurrent, stressImmunityStart, isAddicted);
            }
            mood = Mathf.Clamp(mood, 0, moodMax);
            Debug.LogFormat("[Sta] PlayerManager.cs -> ChangeMood: Mood {0}{1} (now {2}){3}", change > 0 ? "+" : "", change, mood, "\n");
            //change sprite
            GameManager.i.actorPanelScript.SetPlayerMoodUI(mood, isStressed);
            //add a record
            HistoryMood record = new HistoryMood()
            {
                change = change,
                mood = mood,
                factor = factor,
                isStressed = isStressed
            };
            //colour coded descriptor
            if (isAddicted == true)
            { text = string.Format("{0} {1}{2} {3}", reason, change > 0 ? "+" : "", change, isStressed == true ? ", IMMUNE" : ""); }
            else { text = string.Format("{0} {1}{2} {3}", reason, change > 0 ? "+" : "", change, isStressed == true ? ", STRESSED" : ""); }
            if (change > 0) { record.descriptor = GameManager.Formatt(text, ColourType.goodText); }
            else if (change < 0) { { record.descriptor = GameManager.Formatt(text, ColourType.badText); } }
            else { record.descriptor = GameManager.Formatt(text, ColourType.neutralText); }
            //add to list
            listOfMoodHistory.Add(record);
            //message
            GameManager.i.messageScript.PlayerMoodChange(reason, change, mood, isStressed);
        }
        else
        {
            //player already stressed
            if ((mood + change) < 0)
            {
                GameManager.i.dataScript.StatisticIncrement(StatType.PlayerSuperStressed);
                //used to force a breakdown at the next opportunity
                numOfSuperStress++;
            }
        }
    }

    /// <summary>
    /// Set mood to a particular value based on loaded save game data
    /// </summary>
    /// <param name="mood"></param>
    public void SetMood(int mood)
    {
        Debug.AssertFormat(mood >= 0 && mood <= moodMax, "Invalid mood \"{0}\", (should be between 0 & 3)");
        this.mood = mood;
        this.mood = Mathf.Clamp(this.mood, 0, moodMax);
    }

    public List<HistoryMood> GetListOfMoodHistory()
    { return listOfMoodHistory; }

    /// <summary>
    /// Set listOfMoodHistory with loaded save game data. Clears out any existing data prior to updating.
    /// </summary>
    /// <param name="listOfHistory"></param>
    public void SetMoodHistory(List<HistoryMood> listOfHistory)
    {
        if (listOfHistory != null)
        {
            listOfMoodHistory.Clear();
            listOfMoodHistory.AddRange(listOfHistory);
        }
        else { Debug.LogError("Invalid listOfHistory (Null)"); }
    }

    /// <summary>
    /// Any unused actions at the end of the turn are used to improve the Players mood, +1 / action. Player must be active for this to occur
    /// </summary>
    /// <param name="unusedActions"></param>
    public void ProcessDoNothing(int unusedActions)
    {
        if (unusedActions > 0)
        {
            if (Status == ActorStatus.Active)
            {
                //stats
                GameManager.i.dataScript.StatisticIncrement(StatType.PlayerDoNothing, unusedActions);
                //only improve mood if there is room for improvement
                if (mood < moodMax && isStressed == false)
                {
                    //there's a cap on mood improvements from doing nothing
                    int moodIncrease = Mathf.Min(unusedActions, moodImprove);
                    //improve mood
                    ChangeMood(moodIncrease, "Watching SerialFlix", "", false);
                }
            }
        }
        else { Debug.LogWarning("Invalid unused Actions (Zero)"); }
    }
    #endregion

    #region RansomPlayer
    /// <summary>
    /// Player captured and ransomed off, eg. Bounty Hunter nemesis. All gear and all power lost
    /// </summary>
    public void RansomPlayer()
    {
        int numOfGear;
        string gearName;
        switch (GameManager.i.sideScript.resistanceOverall)
        {
            case SideState.Human:
                //gear -> remove all, reverse loop
                numOfGear = listOfGear.Count;
                if (numOfGear > 0)
                {
                    for (int i = numOfGear - 1; i >= 0; i--)
                    {
                        gearName = listOfGear[i];
                        if (string.IsNullOrEmpty(gearName) == false)
                        { RemoveGear(gearName, true); }
                        else { Debug.LogWarningFormat("Invalid gearName (Null or Empty) for listOfGear[{0}]", i); }
                    }
                }
                //power -> remove all
                Power = 0;
                break;
            case SideState.AI:
                //Power
                GameManager.i.dataScript.SetAIResources(GameManager.i.globalScript.sideResistance, 0);
                //gear
                GameManager.i.aiRebelScript.ResetGearPool();
                break;
            default: Debug.LogWarningFormat("Unrecognised resistanceOverall sideState \"{0}\"", GameManager.i.sideScript.resistanceOverall); break;
        }
    }
    #endregion

    #region Node Actions...
    //
    // - - - Node Actions
    //

    public List<NodeActionData> GetListOfNodeActions()
    { return listOfNodeActions; }

    /// <summary>
    /// add a nodeActionData package to list
    /// </summary>
    /// <param name="data"></param>
    public void AddNodeAction(NodeActionData data)
    {
        if (data != null)
        {
            //validate data
            if (data.turn < 0 || data.turn > GameManager.i.turnScript.Turn)
            { Debug.LogWarningFormat("Invalid NodeActionData turn \"{0}\" for {1}, {2}", data.turn, PlayerName, "Player"); }
            if (data.actorID != 999)
            { Debug.LogWarningFormat("Invalid NodeActionData actorID \"{0}\" (should be 999) for {1}, {2}", data.actorID, PlayerName, "Player"); }
            if (data.nodeID < 0 || data.nodeID > GameManager.i.nodeScript.nodeIDCounter)
            { Debug.LogWarningFormat("Invalid NodeActionData nodeID \"{0}\" for {1}, {2}", data.nodeID, PlayerName, "Player"); }
            if (data.nodeAction == NodeAction.None)
            { Debug.LogWarningFormat("Invalid NodeActionData nodeAction \"{0}\" for {1}, {2}", data.nodeAction, PlayerName, "Player"); }
            //add to list
            listOfNodeActions.Add(data);
            /*Debug.LogFormat("[Tst] PlayerManager.cs -> AddNodeAction: t {0}, actorID {1}, nodeID {2}, act {3}, data {4}{5}", data.turn, data.actorID, data.nodeID, data.nodeAction, data.dataName, "\n");*/
        }
        else { Debug.LogError("Invalid nodeDataAction (Null)"); }
    }

    /// <summary>
    /// get number of nodeActionData records in list
    /// </summary>
    /// <returns></returns>
    public int CheckNumOFNodeActions()
    { return listOfNodeActions.Count; }

    /// <summary>
    /// Get most recent nodeActionData package (record at end of list)
    /// </summary>
    /// <returns></returns>
    public NodeActionData GetMostRecentNodeAction()
    { return listOfNodeActions[listOfNodeActions.Count - 1]; }

    /// <summary>
    /// Delete most recent nodeActionData package (record at end of list)
    /// </summary>
    public void RemoveLastUsedNodeAction(int turn, int nodeID, NodeAction nodeAction)
    {
        int index = listOfNodeActions.FindIndex(x => x.turn == turn && x.nodeID == nodeID && x.nodeAction == nodeAction);
        if (index > -1)
        {
            listOfNodeActions.RemoveAt(index);
            /*Debug.LogFormat("[Tst] PlayerManager.cs -> NodeAction turn {0}, nodeID {1}, nodeAction {2} REMOVED", turn, nodeID, nodeAction);*/
        }
        else { Debug.LogWarningFormat("NodeAction not found in listOfNodeActions for turn {0}, nodeID {1} and nodeAction {2}", turn, nodeID, nodeAction); }
    }

    /// <summary>
    /// Empty out listOfNodeActions
    /// </summary>
    public void ClearAllNodeActions()
    { listOfNodeActions.Clear(); }

    /// <summary>
    /// used for updating with load/save data
    /// </summary>
    /// <param name="tempList"></param>
    public void SetListOfNodeActions(List<NodeActionData> tempList)
    {

        if (tempList != null)
        {
            listOfNodeActions.Clear();
            if (tempList.Count > 0)
            { listOfNodeActions.AddRange(tempList); }
        }
        else { Debug.LogWarning("Invalid tempList parameter (Null)"); }
    }

    /// <summary>
    /// Display Player Node actions
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayPlayerNodeActions()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(string.Format("-Player NodeActionData{0}{1}", "\n", "\n"));
        if (listOfNodeActions != null && listOfNodeActions.Count > 0)
        {
            for (int j = 0; j < listOfNodeActions.Count; j++)
            {
                NodeActionData data = listOfNodeActions[j];
                if (data != null)
                { builder.AppendFormat("  t{0}: {1}, A {2}, N {3}, T {4}, S {5}{6}", data.turn, data.nodeAction, data.actorID, data.nodeID, data.teamID, data.dataName, "\n"); }
                else { Debug.LogWarningFormat("Invalid nodeActionData for listOfData[{0}]{1}", j, "\n"); }
            }
        }
        else { builder.AppendFormat("  no records present{0}", "\n"); }
        builder.AppendLine();
        return builder.ToString();
    }
    #endregion

    #region CheckPlayerSpecial
    /// <summary>
    /// returns true for the player if active and if meets the special enum criteria, false otherwise
    /// </summary>
    /// <param name="check"></param>
    /// <returns></returns>
    public bool CheckPlayerSpecial(PlayerCheck check)
    {
        bool isResult = false;
        if (Status == ActorStatus.Active)
        {
            switch (check)
            {
                case PlayerCheck.NodeActionsNOTZero:
                    //Player with listOfNodeActions.Count > 0 && the player's most recent NodeActivity has valid topics present
                    if (CheckNumOFNodeActions() != 0)
                    {
                        NodeActionData data = GetMostRecentNodeAction();
                        if (data != null)
                        {
                            //get relevant subType topic list
                            TopicSubSubType subSubType = GameManager.i.topicScript.GetTopicSubSubType(data.nodeAction);
                            if (subSubType != null)
                            {
                                List<Topic> listOfTopics = GameManager.i.dataScript.GetListOfTopics(subSubType.subType);
                                //check that Players most recent node Action has at least one Live topic of correct subSubType on the list
                                if (listOfTopics.Exists(x => x.subSubType.name.Equals(subSubType.name, StringComparison.Ordinal) && x.status == GlobalStatus.Live))
                                { isResult = true; }
                            }
                            else { Debug.LogErrorFormat("Invalid subSubType (Null) for nodeAction \"{0}\"", data.nodeAction); }
                        }
                        else { Debug.LogError("Invalid NodeActionData (Null)"); }
                    }
                    break;
                default: Debug.LogWarningFormat("Unrecognised PlayerCheck \"{0}\"", check); break;
            }
        }
        return isResult;
    }
    #endregion

    #region Capture Tools...
    //
    // - - - Capture Tools
    //

    public bool[] GetArrayOfCaptureTools()
    { return arrayOfCaptureTools; }

    /// <summary>
    /// used for updated arrayOfCaptureTools with Save/Load data
    /// </summary>
    /// <param name="arrayOfTools"></param>
    public void SetArrayOfCaptureTools(bool[] arrayOfTools)
    {
        if (arrayOfTools != null)
        {
            int count = arrayOfCaptureTools.Length;
            Debug.AssertFormat(arrayOfTools.Length == count, "Mismatch on array lengths (input array has {0} records, arrayOfCaptureTools has {1})", arrayOfTools.Length, count);
            for (int i = 0; i < count; i++)
            { arrayOfCaptureTools[i] = arrayOfTools[i]; }
        }
        else { Debug.LogError("Invalid arrayOfTools (Null)"); }
    }

    /// <summary>
    /// Add capture tool, for the specified innocence level, to player's inventory. Returns true if successful, false otherwise, eg. may not be a CaptureTool present for that particular level
    /// Or may already be present, or inventory may be maxxed out (limit of 3)
    /// </summary>
    /// <param name="innocenceLevel"></param>
    /// <returns></returns>
    public bool AddCaptureTool(int innocenceLevel)
    {
        Debug.AssertFormat(innocenceLevel > -1 && innocenceLevel < 4, "Invalid innocence level \"{0}\" (should be within range 0 to 3)", innocenceLevel);
        //capture tool exist for that innocence level
        if (GameManager.i.captureScript.CheckIfCaptureToolPresent(innocenceLevel) == true)
        {
            //check if already present
            if (arrayOfCaptureTools[innocenceLevel] == false)
            {
                arrayOfCaptureTools[innocenceLevel] = true;
                return true;
            }
            else
            { Debug.LogWarningFormat("Capture tool already present for InnocenceLevel {0}", innocenceLevel); }
        }
        return false;
    }

    /// <summary>
    /// Remove capture Tool, for specified innocence leve, from Player's inventory. Returns true if successful and false if no capture tool was present for that level
    /// </summary>
    /// <param name="innocenceLevel"></param>
    /// <returns></returns>
    public bool RemoveCaptureTool(int innocenceLevel)
    {
        Debug.AssertFormat(innocenceLevel > -1 && innocenceLevel < 4, "Invalid innocence level \"{0}\" (should be within range 0 to 3)", innocenceLevel);
        if (arrayOfCaptureTools[innocenceLevel] == true)
        {
            arrayOfCaptureTools[innocenceLevel] = false;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if player has a capture tool in their inventory for the specified level of innocence, false otherwise
    /// </summary>
    /// <param name="innnocenceLevel"></param>
    /// <returns></returns>
    public bool CheckCaptureToolPresent(int innocenceLevel)
    {
        Debug.AssertFormat(innocenceLevel > -1 && innocenceLevel < 4, "Invalid innocence level \"{0}\" (should be within range 0 to 3)", innocenceLevel);
        return arrayOfCaptureTools[innocenceLevel];
    }
    #endregion

    #region MetaGame...
    //
    // - - - MetaGame
    //

    /// <summary>
    /// take care of all MetaGame matters
    /// </summary>
    public void ProcessMetaPlayer()
    {
        //Send lists to MetaGame
        if (listOfSecrets.Count > 0)
        { GameManager.i.metaScript.SetMetaSecrets(listOfSecrets); }
        if (listOfInvestigations.Count > 0)
        { GameManager.i.metaScript.SetMetaInvestigations(listOfInvestigations); }
        //zero out arrayOfCaptureTools at end of level
        for (int i = 0; i < arrayOfCaptureTools.Length; i++)
        { arrayOfCaptureTools[i] = false; }

    }
    #endregion

    #region GetInnocenceDescriptor
    /// <summary>
    /// Returns a colour formatted, and Bolded, string 'The authority views you as a [...]'. Used for CheckText as well as TopBarUI tooltip. Standardises innocence descriptors across code base.
    /// NOTE: Can specify colour if required, default  ColourType.neutralText)
    /// </summary>
    /// <returns></returns>
    public string GetInnocenceDescriptor(ColourType color = ColourType.neutralText)
    {
        string replaceText = "Unknown";
        switch (Innocence)
        {
            case 3: replaceText = GameManager.Formatt("<b>Low level street Operative</b>", color); break;
            case 2: replaceText = GameManager.Formatt("<b>Mid level Organiser</b>", color); break;
            case 1: replaceText = GameManager.Formatt("<b>High level Operative</b>", color); break;
            case 0: replaceText = GameManager.Formatt("<b>City Commander</b>", color); break;
            default: Debug.LogWarningFormat("Unrecognised Innocence {0}", Innocence); break;
        }
        return replaceText;
    }
    #endregion

    //place new methods above here
}
