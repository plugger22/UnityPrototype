using gameAPI;
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
    [HideInInspector] public ActorStatus status;
    [HideInInspector] public ActorTooltip tooltipStatus;                            //Actor sprite shows a relevant tooltip if tooltipStatus > None (Breakdown, etc)
    [HideInInspector] public ActorInactive inactiveStatus;                          //reason actor is inactive
    [HideInInspector] public ActorSex sex;
    [HideInInspector] public bool isBreakdown;                                      //enforces a minimum one turn gap between successive breakdowns
    [HideInInspector] public bool isEndOfTurnGearCheck;                             //set true by UpdateGear (as a result of Compromised gear check)
    [HideInInspector] public bool isLieLowFirstturn;                                //set true when lie low action, prevents invis incrementing on first turn
    [HideInInspector] public bool isStressLeave;                                    //set true to ensure player spends one turn inactive on stress leave
    [HideInInspector] public bool isStressed;                                       //true if player stressed
    [HideInInspector] public int numOfSuperStress;                                  //increments whenever the player gets stressed when already stressed
    [HideInInspector] public bool isAddicted;                                       //true if player addicted
    [HideInInspector] public int stressImmunityStart;                               //starting value of stressImmunityCurrent (decreases each time drug used)
    [HideInInspector] public int stressImmunityCurrent;                             //dynamic number of turns player is immune from stress (due to drug)
    [HideInInspector] public int addictedTally;                                     //starts from 0 whenever player becomes addicted (needed to provide a buffer before feed the need kicks in)
    //collections
    private List<string> listOfGear = new List<string>();                           //gear names of all gear items in inventory
    private List<Condition> listOfConditionsResistance = new List<Condition>();     //list of all conditions currently affecting the Resistance player
    private List<Condition> listOfConditionsAuthority = new List<Condition>();      //list of all conditions currently affecting the Authority player
    private List<Secret> listOfSecrets = new List<Secret>();                        //list of all secrets (skeletons in the closet)
    private List<Investigation> listOfInvestigations = new List<Investigation>();   //list of all active investigations into Player's behaviour (both sides)
    private List<HistoryMood> listOfMoodHistory = new List<HistoryMood>();          //tracks all changes to player mood
    //personality
    private int mood;
    private Personality personality = new Personality();
    #endregion

    private List<NodeActionData> listOfNodeActions = new List<NodeActionData>();

    /*[HideInInspector] public bool isOrgActivatedCurePresent;*/

    //private backing fields, need to track separately to handle AI playing both sides
    private int _renownResistance;
    private int _renownAuthority;
    private string _playerNameResistance;
    private string _playerNameAuthority;
    private int _invisibility;


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



    //Note: There is no ActorStatus for the player as the 'ResistanceState' handles this -> EDIT: Nope, status does

    //Returns name of human controlled player side (even if temporarily AI controlled during an autoRun)
    //Use GetPlayerNameResistance/Authority for a specific side name
    //NOTE: _playerNameResistance/Authority are set by SideManager.cs -> Initialise
    public string PlayerName
    {
        get
        {
            if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level)
            { return _playerNameResistance; }
            else if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level)
            { return _playerNameAuthority; }
            else
            {
                Debug.LogError("Invalid PlayerSide.level");
                return "Unknown Name";
            }
        }
    }

    public int Renown
    {
        get
        {
            if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level) { return _renownResistance; }
            else if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level) { return _renownAuthority; }
            else
            {
                //AI control of both side
                if (GameManager.instance.turnScript.currentSide.level == globalResistance.level)
                { return _renownResistance; }
                else { return _renownAuthority; }
            }
        }
        set
        {
            if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level)
            {
                Debug.LogFormat("[Sta] -> PlayerManager.cs: Player (Resistance) Renown changed from {0} to {1}{2}", _renownResistance, value, "\n");
                _renownResistance = value;

                //update AI side tab (not using an Event here) -> no updates for the first turn
                if (GameManager.instance.turnScript.Turn > 0 && GameManager.instance.sideScript.resistanceOverall == SideState.Human)
                { GameManager.instance.aiScript.UpdateSideTabData(_renownResistance); }
                //update renown UI (regardless of whether on or off
                GameManager.instance.actorPanelScript.UpdatePlayerRenownUI(_renownResistance);
            }
            else if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level)
            {

                /*value = Mathf.Clamp(value, 0, GameManager.instance.actorScript.maxStatValue);*/

                Debug.LogFormat("[Sta] -> PlayerManager.cs: Player (Authority) Renown changed from {0} to {1}{2}", _renownAuthority, value, "\n");
                _renownAuthority = value;
                //update renown UI (regardless of whether on or off
                GameManager.instance.actorPanelScript.UpdatePlayerRenownUI(_renownAuthority);
            }
            else
            {
                //AI control of both side
                if (GameManager.instance.turnScript.currentSide.level == globalResistance.level) { _renownResistance = value; }
                else { _renownAuthority = value; }
            }
        }
    }

    public int Invisibility
    {
        get { return _invisibility; }
        set
        {
            value = Mathf.Clamp(value, 0, GameManager.instance.actorScript.maxStatValue);
            Debug.LogFormat("[Sta] -> PlayerManager.cs:  Player (Resistance) Invisibility changed from {0} to {1}{2}", _invisibility, value, "\n");
            _invisibility = value;
        }
    }


    /// <summary>
    /// Not for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
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
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }


    public void InitialiseLate(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                //debug purposes only 
                SubInitialiseSessionStartLate();
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStartEarly()
    {
        int[] arrayOfFactors = new int[5];
        if (GameManager.instance.testScript.testPersonality == null)
        {
            //Debug assign random personality
            arrayOfFactors = GameManager.instance.personScript.SetPersonalityFactors(new int[] { 99, 99, 99, 99, 99 });
        }
        else
        {
            //use a test personality
            arrayOfFactors = GameManager.instance.testScript.testPersonality.GetFactors();
        }
        //set personality factors
        personality.SetFactors(arrayOfFactors);
        //sex -> DEBUG (placeholder)
        sex = ActorSex.Male;
        //set initial default value for drug stress immunity
        stressImmunityStart = GameManager.instance.actorScript.playerAddictedImmuneStart;
    }
    #endregion

    #region SubInitialiseSessionStartLate
    private void SubInitialiseSessionStartLate()
    {
        GameManager.instance.personScript.SetDescriptors(personality);
        GameManager.instance.personScript.CheckPersonalityProfile(GameManager.instance.dataScript.GetDictOfProfiles(), personality);
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //fast access fields (BEFORE set stats below)
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
        hackingGear = GameManager.instance.gearScript.typeHacking.name;
        maxNumOfSecrets = GameManager.instance.secretScript.secretMaxNum;
        conditionCorrupt = GameManager.instance.dataScript.GetCondition("CORRUPT");
        conditionIncompetent = GameManager.instance.dataScript.GetCondition("INCOMPETENT");
        conditionQuestionable = GameManager.instance.dataScript.GetCondition("QUESTIONABLE");
        conditionDoomed = GameManager.instance.dataScript.GetCondition("DOOMED");
        conditionWounded = GameManager.instance.dataScript.GetCondition("WOUNDED");
        conditionTagged = GameManager.instance.dataScript.GetCondition("TAGGED");
        conditionImaged = GameManager.instance.dataScript.GetCondition("IMAGED");
        conditionStressed = GameManager.instance.dataScript.GetCondition("STRESSED");
        conditionAddicted = GameManager.instance.dataScript.GetCondition("ADDICTED");
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
    }
    #endregion

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
        actorID = GameManager.instance.preloadScript.playerActorID;
        //gear check
        isEndOfTurnGearCheck = false;
        //set stats      
        Invisibility = 3;
        mood = moodStart;
        //Update player mood
        GameManager.instance.actorPanelScript.SetPlayerMoodUI(mood);
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register event listeners
        EventManager.instance.AddListener(EventType.EndTurnLate, OnEvent, "PlayerManager.cs");
        EventManager.instance.AddListener(EventType.StartTurnLate, OnEvent, "PlayerManager.cs");
    }

    #endregion

    #region SubInitialiseLevelAll
    private void SubInitialiseLevelAll()
    {
        //place Player in a random start location (Sprawl node) -> AFTER Level and Session Initialisation
        InitialisePlayerStartNode();
        //Comes at end AFTER all other initialisations
        Renown = 0;
        //give the player a random secret (PLACEHOLDER -> should be player choice)
        GetRandomStartSecret();
    }
    #endregion

    #region SubInitialiseFollowOn
    private void SubInitialiseFollowOn()
    {
        //place Player in a random start location (Sprawl node) -> AFTER Level and Session Initialisation
        InitialisePlayerStartNode();
        //set player alpha to active (may have been lying low at end of previous level)
        GameManager.instance.actorPanelScript.UpdatePlayerAlpha(GameManager.instance.guiScript.alphaActive);
        //set default status
        status = ActorStatus.Active;
        inactiveStatus = ActorInactive.None;
        tooltipStatus = ActorTooltip.None;

        /*//remove any conditions, Player starts followOn level with a clean slate
        RemoveAllConditions(GameManager.instance.sideScript.PlayerSide);*/

        //reset mood to default
        SetMood(moodStart);
        //clear out mood history
        listOfMoodHistory.Clear();
        //if immunity to stress > 0, set to max allowed
        if (stressImmunityCurrent > 0)
        { stressImmunityCurrent = stressImmunityStart; }
        //empty out gear list
        listOfGear.Clear();
        //add cures for any existing conditions
        SetCures();
    }
    #endregion

    #endregion

    /// <summary>
    /// starts player at a random SPRAWL node, error if not or if City Hall district (player can't start here as nemesis always starts from there)
    /// </summary>
    private void InitialisePlayerStartNode()
    {
        //assign player to a starting node (Sprawl)
        int nodeID = 0;
        int nodeArcID = GameManager.instance.dataScript.GetNodeArcID("SPRAWL");
        if (nodeArcID > -1)
        {
            Node node = GameManager.instance.dataScript.GetRandomNode(nodeArcID);
            if (node != null)
            { nodeID = node.nodeID; }
            else
            { Debug.LogWarning("PlayerManager: Invalid Player starting node (Null). Player placed in node '0' by default"); }
            //can't be at City Hall (Nemesis starting location) -> not possible as not a SPRAWL district but check anyway
            if (nodeID != GameManager.instance.cityScript.cityHallDistrictID)
            {
                Node nodeTemp = GameManager.instance.dataScript.GetNode(nodeID);
                if (nodeTemp != null)
                {
                    //initialise move list
                    nodeTemp.SetPlayerMoveNodes();
                    //set player node
                    GameManager.instance.nodeScript.nodePlayer = nodeID;
                    Debug.LogFormat("[Ply] PlayerManager.cs -> Initialise: Player starts at node {0}, {1}, id {2}{3}", nodeTemp.nodeName, nodeTemp.Arc.name, nodeTemp.nodeID, "\n");
                    //message
                    string text = string.Format("Player commences at \"{0}\", {1}, ID {2}", node.nodeName, node.Arc.name, node.nodeID);
                    GameManager.instance.messageScript.PlayerMove(text, node, 0, 0, true);
                }
                else { Debug.LogErrorFormat("Invalid playerNode (Null) for nodeID {0}", nodeID); }
            }
            else { Debug.LogError("Invalid player start node (Not a SPRAWL district)"); }
        }
        else { Debug.LogError("Invalid player start node (City Hall location)"); }
    }

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
                if (status == ActorStatus.Active)
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

    /// <summary>
    /// end of turn final
    /// </summary>
    private void EndTurnFinal()
    {
        //handles situation of no compromised gear checks and resets isEndOfTurnGearCheck
        ResetAllGear();
    }


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
                Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
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
            Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
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
            Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
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
                Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
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
        Gear gear = GameManager.instance.dataScript.GetGear(gearName);
        if (listOfGear.Count < GameManager.instance.gearScript.maxNumOfGear)
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
                    GameManager.instance.dataScript.AddGearNew(gear);
                    //statistics
                    GameManager.instance.dataScript.StatisticIncrement(StatType.GearTotal);
                    return true;
                }
                else
                { Debug.LogWarningFormat("Gear \'{0}\", is already present in Player inventory", gear.tag); }
            }
            else
            { Debug.LogError(string.Format("Invalid gear (Null) for gear {0}", gearName)); }
        }
        /*else { Debug.LogWarning("You cannot exceed the maxium number of Gear items -> Gear NOT added"); }*/
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
            Gear gear = GameManager.instance.dataScript.GetGear(gearName);
            if (gear != null)
            {
                //check gear not already in inventory
                if (CheckGearPresent(gearName) == true)
                {
                    RemoveGearItem(gear, isLost);
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
            if (GameManager.instance.dataScript.RemoveGearLost(gear) == false)
            {
                gear.statTurnLost = GameManager.instance.turnScript.Turn;
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
                    Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
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
                GameManager.instance.aiScript.UpdateAIGearStatus();
            }
        }
    }

    /// <summary>
    /// called by GearManager.cs -> ProcessCompromisedGear. Tidies up gear at end of turn. gearSaveName is the compromised gear that the player chose to save. Resets all gear
    /// returns name of gear saved (UPPER) [assumes only one piece of gear can be saved per turn], if any. Otherwise returns Empty string
    /// </summary>
    public void UpdateGear(int renownUsed = -1, string gearSaveName = null)
    {
        if (listOfGear.Count > 0)
        {
            //reverse loop through looking for compromised gear
            for (int i = listOfGear.Count - 1; i >= 0; i--)
            {
                Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
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
                                GameManager.instance.messageScript.GearCompromised(msgText, gear, -1);
                                RemoveGearItem(gear, true);
                            }
                            else
                            {
                                //gear saved
                                ResetGearItem(gear);
                                string msgText = string.Format("{0} ({1}), has been COMPROMISED and SAVED", gear.tag, gear.type.name);
                                GameManager.instance.messageScript.GearCompromised(msgText, gear, renownUsed);
                            }
                        }
                        else
                        {
                            //gear lost
                            string msgText = string.Format("{0} ({1}), has been COMPROMISED and LOST", gear.tag, gear.type.name);
                            GameManager.instance.messageScript.GearCompromised(msgText, gear, -1);
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
                            GameManager.instance.messageScript.GearCompromised(msgText, gear, renownUsed);
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

    //
    // - - - Conditions - - -
    //

    /// <summary>
    /// Initialise the listOfConditions (overwrites existing list). Used for load save game
    /// </summary>
    /// <param name="listOfSecrets"></param>
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
                                GameManager.instance.actorScript.SetDoomTimer();
                                GameManager.instance.nodeScript.AddCureNode(conditionDoomed.cure);
                                break;
                            case "TAGGED":
                                GameManager.instance.nodeScript.AddCureNode(conditionTagged.cure);
                                break;
                            case "WOUNDED":
                                GameManager.instance.nodeScript.AddCureNode(conditionWounded.cure);
                                break;
                            case "IMAGED":
                                GameManager.instance.nodeScript.AddCureNode(conditionImaged.cure);
                                break;
                            case "ADDICTED":
                                isAddicted = true;
                                addictedTally = 0;
                                GameManager.instance.nodeScript.AddCureNode(conditionAddicted.cure);
                                break;
                            case "STRESSED":
                                mood = 0;
                                isStressed = true;
                                //update UI
                                GameManager.instance.actorPanelScript.SetPlayerMoodUI(0, isStressed);
                                //stats
                                GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerTimesStressed);
                                break;
                        }
                        Debug.LogFormat("[Cnd] PlayerManager.cs -> AddCondition: {0} Player, gains {1} condition{2}", side.name, condition.tag, "\n");
                        if (GameManager.instance.sideScript.PlayerSide.level == side.level)
                        {
                            //message
                            string msgText = string.Format("{0} Player, {1}, gains condition \"{2}\"", side.name, GetPlayerName(side), condition.tag);
                            GameManager.instance.messageScript.ActorCondition(msgText, actorID, true, condition, reason, isResistance);
                        }
                    }
                }
                else
                    //condition already exists
                    switch (condition.tag)
                    {
                        case "STRESSED":
                            //stats
                            GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerSuperStressed);
                            //used to force a breakdown at the next opportunity
                            numOfSuperStress++;
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
                            if (GameManager.instance.sideScript.PlayerSide.level == side.level)
                            {
                                //message
                                string msgText = string.Format("{0} Player, {1}, condition \"{2}\" removed", side.name, GetPlayerName(side), condition.tag);
                                GameManager.instance.messageScript.ActorCondition(msgText, actorID, false, condition, reason, isResistance);
                            }
                            //special conditions
                            switch (condition.tag)
                            {
                                case "DOOMED":
                                    GameManager.instance.actorScript.StopDoomTimer();
                                    GameManager.instance.nodeScript.RemoveCureNode(conditionDoomed.cure);
                                    break;
                                case "TAGGED":
                                    GameManager.instance.nodeScript.RemoveCureNode(conditionTagged.cure);
                                    break;
                                case "WOUNDED":
                                    GameManager.instance.nodeScript.RemoveCureNode(conditionWounded.cure);
                                    break;
                                case "IMAGED":
                                    GameManager.instance.nodeScript.RemoveCureNode(conditionImaged.cure);
                                    break;
                                case "STRESSED":
                                    ChangeMood(moodReset, reason, "n.a");
                                    isStressed = false;
                                    numOfSuperStress = 0;
                                    //change UI mood sprite
                                    GameManager.instance.actorPanelScript.SetPlayerMoodUI(mood);
                                    break;
                                case "ADDICTED":
                                    //reset drug immunity period back to default value
                                    stressImmunityStart = GameManager.instance.actorScript.playerAddictedImmuneStart;
                                    stressImmunityCurrent = 0;
                                    isAddicted = false;
                                    addictedTally = 0;
                                    GameManager.instance.nodeScript.RemoveCureNode(conditionAddicted.cure);
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
            GameManager.instance.messageScript.PlayerImmuneStart(text, stressImmunityCurrent, stressImmunityStart, isAddicted);
            GameManager.instance.messageScript.PlayerImmuneEffect(text, stressImmunityCurrent, stressImmunityStart, isAddicted);
        }

        /*//effect tab only if right at start of addiction period (messages double up otherwise) -> not possible as there is now an exempt period
        if (addictedTally == 0)
        { GameManager.instance.messageScript.PlayerImmuneEffect(text, stressImmunityCurrent, stressImmunityStart, isAddicted); }*/

        //decrease immunity period after every addiction episode
        stressImmunityStart--;
        //minCap
        stressImmunityStart = Mathf.Max(GameManager.instance.actorScript.playerAddictedImmuneMin, stressImmunityStart);
        //remove stress if present
        RemoveCondition(conditionStressed, GameManager.instance.sideScript.PlayerSide, "Took Drugs");
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

    /*/// <summary>
    /// returns true if any current condition has a cure activated by an Org
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public bool CheckForOrgCure(GlobalSide side)
    {
        if (side != null)
        {
            List<Condition> listOfConditions = GetListOfConditionForSide(side);
            if (listOfConditions != null)
            {
                if (listOfConditions.Count > 0)
                {
                    foreach (Condition condition in listOfConditions)
                    {
                        //only check conditions with valid cures
                        if (condition.cure != null)
                        {
                            if (condition.cure.isOrgActivated == true)
                            { return true; }
                        }
                    }
                }
            }
            else { Debug.LogError("Invalid listOfConditions (Null)"); }
        }
        else { Debug.LogError("Invalid side (Null)"); }
        return false;
    }*/

    /// <summary>
    /// Follow on level -> if player has any existing conditions, new cures are created
    /// </summary>
    private void SetCures()
    {
        //if player has any conditions that require a cure you need to set up nodes for the cures
        List<Condition> listOfConditions = GetListOfConditions(GameManager.instance.sideScript.PlayerSide);
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
                        { GameManager.instance.actorScript.SetDoomTimer(); }
                        //add cure to a node (cure isn't activated yet)
                        GameManager.instance.nodeScript.AddCureNode(condition.cure);
                    }
                }
            }
        }
        else { Debug.LogError("Invalid listOfConditions (Null)"); }
    }

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
                    secret.status = gameAPI.SecretStatus.Active;
                    secret.gainedWhen = GameManager.instance.turnScript.Turn;
                    //Msg
                    GameManager.instance.messageScript.PlayerSecret(string.Format("Player gains new secret ({0})", secret.tag), secret);
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
    /// Remove a secret from listOfSecrets. Returns true if successful, false if not found
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
                        case gameAPI.SecretStatus.Revealed:
                            //revealed secret
                            GameManager.instance.dataScript.AddRevealedSecret(secret);
                            break;
                        case gameAPI.SecretStatus.Deleted:
                            //deleted secret
                            GameManager.instance.dataScript.AddDeletedSecret(secret);
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
        List<Secret> listOfPlayerSecrets = GameManager.instance.dataScript.GetListOfPlayerSecrets();
        if (listOfPlayerSecrets != null)
        {
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
    public void SetInvestigations(List<Investigation> listOfInvestigations)
    {
        if (listOfInvestigations != null)
        {
            this.listOfInvestigations.Clear();
            this.listOfInvestigations.AddRange(listOfInvestigations);
        }
        else { Debug.LogError("Invalid listOfInvestigations (Null)"); }
    }

    /// <summary>
    /// Investigation dropped, handles all admin
    /// </summary>
    /// <param name="invest"></param>
    public void DropInvestigation(Investigation invest)
    {
        if (invest != null)
        {
            invest.status = InvestStatus.Completed;
            invest.outcome = InvestOutcome.Dropped;
            invest.turnFinish = GameManager.instance.turnScript.Turn;
            GameManager.instance.dataScript.StatisticIncrement(StatType.InvestigationsCompleted);
            //remove from player
            RemoveInvestigation(invest.reference);
            //add to listOfCompleted
            GameManager.instance.dataScript.AddInvestigationCompleted(invest);
        }
        else { Debug.LogError("Invalid investigation (Null)"); }
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
            int motivation;
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
                            GameManager.instance.dataScript.StatisticIncrement(StatType.InvestigationsCompleted);
                            switch (invest.evidence)
                            {
                                case 3:
                                    //player found innocent
                                    invest.outcome = InvestOutcome.Innocent;
                                    //gain HQ approval
                                    int approvalGain = GameManager.instance.playerScript.investHQApproval;
                                    GameManager.instance.factionScript.ChangeFactionApproval(approvalGain, GameManager.instance.sideScript.PlayerSide, string.Format("{0} Investigation", invest.tag));
                                    Debug.LogFormat("[Inv] PlayerManager.cs -> ProcessInvestigation: Investigation \"{0}\" completed. Player found INNOCENT{1}", invest.tag, "\n");
                                    break;
                                case 0:
                                    //player found guilty
                                    invest.outcome = InvestOutcome.Guilty;
                                    Debug.LogFormat("[Inv] PlayerManager.cs -> ProcessInvestigation: Investigation \"{0}\" completed. Player found GUILTY{1}", invest.tag, "\n");
                                    WinState winner = WinState.None;
                                    switch (GameManager.instance.sideScript.PlayerSide.level)
                                    {
                                        case 1: winner = WinState.Resistance; break;
                                        case 2: winner = WinState.Authority; break;
                                        default: Debug.LogWarningFormat("Unrecognised Player side {0}", GameManager.instance.sideScript.PlayerSide.name); break;
                                    }
                                    GameManager.instance.turnScript.SetWinState(winner, WinReason.Investigation, "Player found Guilty", string.Format("{0} Investigation", invest.tag));
                                    break;
                                default: Debug.LogWarningFormat("Unrecognised invest.evidence \"{0}\"", invest.evidence); break;
                            }
                            //end investigation
                            invest.turnFinish = GameManager.instance.turnScript.Turn;
                            RemoveInvestigation(invest.reference);
                            //msg
                            text = string.Format("{0} Investigation completed. {1} Verdict", invest.tag, invest.evidence == 3 ? "Innocent" : "Guilty");
                            GameManager.instance.messageScript.InvestigationCompleted(text, invest);
                            //TO DO -> place in completed investigation list in DataManager.cs
                        }
                        else
                        {
                            Debug.LogFormat("[Inv] PlayerManager.cs -> ProcessInvestigations: Investigation \"{0}\" is {1} day{2} away from a Player {3} conclusion (evidence {4}){5}", invest.tag, invest.timer,
                                invest.timer != 1 ? "s" : "", invest.evidence == 3 ? "INNOCENT" : "GUILTY", invest.evidence, "\n");
                            text = string.Format("{0} Investigation counting down to a Resolution", invest.tag);
                            GameManager.instance.messageScript.InvestigationResolution(text, invest);
                        }
                    }
                    else
                    {
                        //
                        // - - - No timer -> check for new evidence (not when investigation first launched)
                        //
                        if (invest.turnStart + 1 < GameManager.instance.turnScript.Turn)
                        {
                            rnd = Random.Range(0, 100);
                            isGood = false;
                            if (rnd < chanceEvidence)
                            {
                                Debug.LogFormat("[Rnd] PlayerManager.cs -> ProcessInvestigations: {0} Investigation new EVIDENCE, need {1}, rolled {2}{3}", invest.tag, chanceEvidence, rnd, "\n");
                                //previous
                                invest.previousEvidence = invest.evidence;
                                //good or bad evidence -> depends on HQ actor opinion of you
                                Actor actor = GameManager.instance.dataScript.GetHQHierarchyActor(invest.lead);
                                if (actor != null)
                                {
                                    rnd = Random.Range(0, 100);
                                    motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
                                    switch (motivation)
                                    {
                                        case 3: chance = 80; break;
                                        case 2: chance = 60; break;
                                        case 1: chance = 40; break;
                                        case 0: chance = 20; break;
                                        default: Debug.LogWarningFormat("Unrecognised actor Motivation \"{0}\" for {1}", invest.lead); chance = 0; break;
                                    }
                                    if (rnd < chance)
                                    {
                                        isGood = true;
                                        Debug.LogFormat("[Inv] PlayerManager.cs -> ProcessInvestigation: Investigation \"{0}\", new evidence, Good (needed {1}, rolled {2}), {3} Mot {4}, Ev {5}{6}", 
                                            invest.tag, chance, rnd, invest.lead, motivation, invest.evidence + 1, "\n");
                                    }
                                    else
                                    {
                                        Debug.LogFormat("[Inv] PlayerManager.cs -> ProcessInvestigation: Investigation \"{0}\", new evidence, Bad (needed {1}, rolled {2}), {3} Mot {4}, Ev {5}{6}", 
                                            invest.tag, chance, rnd, invest.lead, motivation, invest.evidence , "\n");
                                    }
                                    text = string.Format("{0} Evidence Uncovered", invest.tag);
                                    GameManager.instance.messageScript.GeneralRandom(text, "Type of Evidence", chance, rnd, false, "rand_5");
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
                                GameManager.instance.messageScript.InvestigationEvidence(text, invest, "your Lead Investigator");
                            }
                            //effects tab msg
                            text = string.Format("Ongoing investigation into your {0}", invest.tag);
                            GameManager.instance.messageScript.InvestigationOngoing(text, invest);
                        }
                    }
                }
                else { Debug.LogErrorFormat("Invalid investigation (Null) for listOfInvestigations[{0}]", i); }
            }
        }
    }


    //
    // - - - Debug - - -
    //

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
            Gear gear = GameManager.instance.dataScript.GetGear(gearName);
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
        if (listOfGear.Count < GameManager.instance.gearScript.maxNumOfGear)
        {
            if (string.IsNullOrEmpty(gearName) == false)
            {
                //find gear in dictionary
                Gear gear = GameManager.instance.dataScript.GetGear(gearName);
                if (gear != null)
                {
                    //check you aren't adding gear that has already been lost
                    List<string> listOfLostGear = GameManager.instance.dataScript.GetListOfLostGear();
                    if (listOfLostGear.Exists(x => x == gear.name) == false)
                    {
                        //add gear to player's inventory
                        if (AddGear(gearName) == true)
                        {
                            //remove from pool
                            if (GameManager.instance.dataScript.RemoveGearFromPool(gear) == false)
                            { Debug.LogWarning("Gear not removed from Pool (Null or other problem)"); }
                            text = string.Format("{0} has been added to the Player's inventory{1}Press ESC to exit", gearName, "\n");
                            //message
                            Node nodePlayer = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
                            GameManager.instance.messageScript.GearObtained(string.Format("{0} added (DEBUG)", gearName), nodePlayer, gear);
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

    public void DebugAddRandomSecret()
    {
        //give the player a random secret
        List<Secret> listOfSecrets = GameManager.instance.dataScript.GetListOfPlayerSecrets();
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
                secret.status = gameAPI.SecretStatus.Deleted;
                secret.deletedWhen = GameManager.instance.turnScript.Turn;
                GameManager.instance.secretScript.RemoveSecretFromAll(secret.name);
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
        List<Investigation> listOfCompletedInvestigations = GameManager.instance.dataScript.GetListOfCompletedInvestigations();
        if (listOfCompletedInvestigations != null)
        { builder.Append(DebugDisplayInvestigationList(listOfCompletedInvestigations)); }
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


    //
    // - - - Personality & mood
    //

    public Personality GetPersonality()
    { return personality; }

    public int GetMood()
    { return mood; }

    /// <summary>
    /// Main method to change mood. Give a reason (keep short, eg. "Dismissed FIXER") and the name of the Factor that applied). Auto adds a history record
    /// NOTE: You only want to call this method if there is definite change (eg. not Zero)
    /// NOTE: No changes are possible while player is STRESSED
    /// </summary>
    /// <param name="change"></param>
    /// <param name="reason"></param>
    /// <param name="factor"></param>
    public void ChangeMood(int change, string reason, string factor)
    {
        string text = "Unknown";
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //check player isn't currently stressed
        if (isStressed == false)
        {
            Debug.Assert(change != 0, "Invalid change (no point of it's Zero)");
            if (string.IsNullOrEmpty(reason) == true)
            {
                reason = "Unknown";
                Debug.LogWarning("Invalid reason (Null or Empty)");
            }
            if (string.IsNullOrEmpty(factor) == true)
            {
                factor = "Unknown";
                Debug.LogWarning("Invalid factor (Null or Empty)");
            }
            //update mood
            bool isStressed = false;
            mood += change;
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
                GameManager.instance.messageScript.PlayerImmuneStress(text, stressImmunityCurrent, stressImmunityStart, isAddicted);
            }
            mood = Mathf.Clamp(mood, 0, moodMax);
            //change sprite
            GameManager.instance.actorPanelScript.SetPlayerMoodUI(mood, isStressed);
            //add a record
            HistoryMood record = new HistoryMood()
            {
                change = change,
                turn = GameManager.instance.turnScript.Turn,
                mood = mood,
                factor = factor,
                isStressed = isStressed
            };
            //colour coded descriptor
            if (isAddicted == true)
            { text = string.Format("{0} {1}{2} {3}", reason, change > 0 ? "+" : "", change, isStressed == true ? ", IMMUNE" : ""); }
            else { text = string.Format("{0} {1}{2} {3}", reason, change > 0 ? "+" : "", change, isStressed == true ? ", STRESSED" : ""); }
            if (change > 0) { record.descriptor = GameManager.instance.colourScript.GetFormattedString(text, ColourType.goodText); }
            else if (change < 0) { { record.descriptor = GameManager.instance.colourScript.GetFormattedString(text, ColourType.badText); } }
            else { record.descriptor = GameManager.instance.colourScript.GetFormattedString(text, ColourType.neutralText); }
            //add to list
            listOfMoodHistory.Add(record);
            //message
            GameManager.instance.messageScript.PlayerMoodChange(reason, change, mood, isStressed);
        }
        else
        {
            //player already stressed
            if ((mood + change) < 0)
            {
                GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerSuperStressed);
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
            if (status == ActorStatus.Active)
            {
                //stats
                GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerDoNothing, unusedActions);
                //only improve mood if there is room for improvement
                if (mood < moodMax && isStressed == false)
                { ChangeMood(unusedActions, "Watching SerialFlix", "n.a"); }
            }
        }
        else { Debug.LogWarning("Invalid unused Actions (Zero)"); }
    }

    /// <summary>
    /// Player captured and ransomed off, eg. Bounty Hunter nemesis. All gear and all renown lost
    /// </summary>
    public void RansomPlayer()
    {
        int numOfGear;
        string gearName;
        switch (GameManager.instance.sideScript.resistanceOverall)
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
                //renown -> remove all
                Renown = 0;
                break;
            case SideState.AI:
                //renown
                GameManager.instance.dataScript.SetAIResources(GameManager.instance.globalScript.sideResistance, 0);
                //gear
                GameManager.instance.aiRebelScript.ResetGearPool();
                break;
            default: Debug.LogWarningFormat("Unrecognised resistanceOverall sideState \"{0}\"", GameManager.instance.sideScript.resistanceOverall); break;
        }
    }


    //
    // - - - Debug 
    //

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
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //use correct list for the player side
        List<Condition> listOfConditions = GetListOfConditionForSide(playerSide);
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- {0} Player Stats{1}{2}", playerSide.name, "\n", "\n");
        builder.AppendFormat("- Stats{0}", "\n");
        if (playerSide.level == globalResistance.level)
        { builder.AppendFormat(" Invisibility {0}{1}", Invisibility, "\n"); }
        builder.AppendFormat(" Renown {0}{1}", Renown, "\n");
        builder.AppendFormat(" Mood {0}{1}", mood, "\n");
        if (GameManager.instance.actorScript.doomTimer > 0) { builder.AppendFormat(" Doom Timer {0}{1}", GameManager.instance.actorScript.doomTimer, "\n"); }
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
        List<Node> listOfCures = GameManager.instance.dataScript.GetListOfCureNodes();
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
        builder.AppendFormat(" Status {0}{1}", status, "\n");
        builder.AppendFormat(" InactiveStatus {0}{1}", inactiveStatus, "\n");
        builder.AppendFormat(" TooltipStatus {0}{1}", tooltipStatus, "\n");
        builder.AppendFormat(" isBreakdown {0}{1}", isBreakdown, "\n");
        builder.AppendFormat(" isEndOfTurnGearCheck {0}{1}", isEndOfTurnGearCheck, "\n");
        builder.AppendFormat(" isLieLowFirstTurn {0}{1}", isLieLowFirstturn, "\n");
        builder.AppendFormat(" isStressLeave {0}{1}", isStressLeave, "\n");
        builder.AppendFormat(" isStressed {0}{1}", isStressed, "\n");
        builder.AppendFormat(" numOfSuperStressed {0}{1}", numOfSuperStress, "\n");
        builder.AppendFormat(" stressImmunityStart {0}{1}", stressImmunityStart, "\n");
        builder.AppendFormat(" stressImmunityCurrent {0}{1}", stressImmunityCurrent, "\n");
        builder.AppendFormat(" addictedTally {0}{1}", addictedTally, "\n");
        builder.AppendFormat(" Sex {0}{1}", sex, "\n");
        builder.AppendFormat("{0} -Global{1}", "\n", "\n");
        builder.AppendFormat(" authorityState {0}{1}", GameManager.instance.turnScript.authoritySecurityState, "\n");
        builder.AppendFormat("{0} -Reserve Pool{1}", "\n", "\n");
        builder.AppendFormat(" NumOfRecruits {0} + {1}{2}", GameManager.instance.dataScript.CheckNumOfOnMapActors(playerSide), GameManager.instance.dataScript.CheckNumOfActorsInReserve(), "\n");
        if (playerSide.level == globalResistance.level)
        {
            //gear
            builder.AppendFormat("{0}- Gear{1}", "\n", "\n");
            if (listOfGear.Count > 0)
            {
                for (int i = 0; i < listOfGear.Count; i++)
                {
                    Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
                    if (gear != null)
                    { builder.AppendFormat(" {0}, {1}{2}", gear.tag, gear.type.name, "\n"); }
                }
            }
            else { builder.Append(" No gear in inventory"); }
        }
        //stats
        builder.AppendFormat("{0}{1} -Stats{2}", "\n", "\n", "\n");
        builder.AppendFormat(" breakdowns: {0}{1}", GameManager.instance.dataScript.StatisticGetLevel(StatType.PlayerBreakdown), "\n");
        builder.AppendFormat(" lie low: {0}{1}", GameManager.instance.dataScript.StatisticGetLevel(StatType.PlayerLieLowTimes), "\n");
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
        reason = reason + GameManager.instance.dataScript.GetRandomActorArcs(1, GameManager.instance.sideScript.PlayerSide)[0].name;
        factor = GameManager.instance.dataScript.DebugGetRandomFactor();
        ChangeMood(change, reason, factor);
    }

    //
    // - - - Node Actions
    //

    /// <summary>
    /// add a nodeActionData package to list
    /// </summary>
    /// <param name="data"></param>
    public void AddNodeAction(NodeActionData data)
    {
        if (data != null)
        {
            //validate data
            if (data.turn < 0 || data.turn > GameManager.instance.turnScript.Turn)
            { Debug.LogWarningFormat("Invalid NodeActionData turn \"{0}\" for {1}, {2}", data.turn, PlayerName, "Player"); }
            if (data.actorID != 999)
            { Debug.LogWarningFormat("Invalid NodeActionData actorID \"{0}\" (should be 999) for {1}, {2}", data.actorID, PlayerName, "Player"); }
            if (data.nodeID < 0 || data.nodeID > GameManager.instance.nodeScript.nodeIDCounter)
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
    public void RemoveMostRecentNodeAction()
    { listOfNodeActions.RemoveAt(listOfNodeActions.Count - 1); }

    /// <summary>
    /// Empty out listOfNodeActions
    /// </summary>
    public void ClearAllNodeActions()
    { listOfNodeActions.Clear(); }


    /// <summary>
    /// returns true for the player if active and if meets the special enum criteria, false otherwise
    /// </summary>
    /// <param name="check"></param>
    /// <returns></returns>
    public bool CheckPlayerSpecial(PlayerCheck check)
    {
        bool isResult = false;
        if (status == ActorStatus.Active)
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
                            TopicSubSubType subSubType = GameManager.instance.topicScript.GetTopicSubSubType(data.nodeAction);
                            if (subSubType != null)
                            {
                                List<Topic> listOfTopics = GameManager.instance.dataScript.GetListOfTopics(subSubType.subType);
                                //check that Players most recent node Action has at least one Live topic of correct subSubType on the list
                                if (listOfTopics.Exists(x => x.subSubType.name.Equals(subSubType.name, StringComparison.Ordinal) && x.status == Status.Live))
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

    //place new methods above here
}
