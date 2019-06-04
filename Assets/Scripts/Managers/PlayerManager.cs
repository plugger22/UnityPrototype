using gameAPI;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// handles all player related data and methods
/// </summary>
public class PlayerManager : MonoBehaviour
{
    public Sprite sprite;

    //[HideInInspector] public int numOfRecruits;
    //[HideInInspector] public int Invisibility;

    #region save data compatibile
    [HideInInspector] public int actorID;
    [HideInInspector] public ActorStatus status;
    [HideInInspector] public ActorTooltip tooltipStatus;                            //Actor sprite shows a relevant tooltip if tooltipStatus > None (Breakdown, etc)
    [HideInInspector] public ActorInactive inactiveStatus;                          //reason actor is inactive
    [HideInInspector] public bool isBreakdown;                                      //enforces a minimum one turn gap between successive breakdowns
    [HideInInspector] public bool isEndOfTurnGearCheck;                             //set true by UpdateGear (as a result of Compromised gear check)
    [HideInInspector] public bool isLieLowFirstturn;                                //set true when lie low action, prevents invis incrementing on first turn
    [HideInInspector] public bool isStressLeave;                                    //set true to ensure player spends one turn inactive on stress leave

    private List<string> listOfGear = new List<string>();                           //gear names of all gear items in inventory
    private List<Condition> listOfConditionsResistance = new List<Condition>();     //list of all conditions currently affecting the Resistance player
    private List<Condition> listOfConditionsAuthority = new List<Condition>();      //list of all conditions currently affecting the Authority player
    private List<Secret> listOfSecrets = new List<Secret>();                        //list of all secrets (skeletons in the closet)
    #endregion

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
                SubInitialiseLevelAll();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

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
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register event listeners
        EventManager.instance.AddListener(EventType.EndTurnLate, OnEvent, "PlayerManager.cs");
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
        //remove any conditions, Player starts followOn level with a clean slate
        RemoveAllConditions(GameManager.instance.sideScript.PlayerSide);
        //empty out gear list
        listOfGear.Clear();
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
    /// returns gearID of best piece of gear that matches type, '-1' if none
    /// </summary>
    /// <param name="gearType"></param>
    /// <returns></returns>
    public int CheckGearTypePresent(GearType gearType)
    {
        int gearID = -1;
        int rarity = -1;
        if (listOfGear.Count > 0)
        {
            //loop through looking for best piece of gear that matches the type
            for (int i = 0; i < listOfGear.Count; i++)
            {
                Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
                if (gear != null)
                {
                    if (gear.type.name.Equals(gearType.name) == true)
                    {
                        //is it a better piece of gear (higher rarity) than already found?
                        if (gear.rarity.level > rarity)
                        {
                            rarity = gear.rarity.level;
                            gearID = gear.gearID;
                        }
                    }
                }
                else
                { Debug.LogWarning(string.Format("Invalid gear (Null) for gearID {0}", listOfGear[i])); }
            }
            return gearID;
        }
        else { return -1; }
    }


    /// <summary>
    /// returns list of AI(Hacking) gearID's of all gear capable of doing so in Player's inventory. Returns null if none
    /// </summary>
    /// <returns></returns>
    public List<int> CheckAIGearPresent()
    {
        List<int> tempList = new List<int>();
        //loop through looking for all AI hacking capable gear
        for (int i = 0; i < listOfGear.Count; i++)
        {
            Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
            if (gear != null)
            {
                //hacking gear
                if (gear.type.name.Equals(hackingGear) == true)
                {
                    //has AI effects
                    if (gear.aiHackingEffect != null)
                    { tempList.Add(gear.gearID); }
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
                if (gear.type.name.Equals(hackingGear) == true)
                {
                    //has AI effects -> return first instance found
                    if (gear.aiHackingEffect != null)
                    {
                        if (gear.aiHackingEffect.name.Equals(effectName) == true)
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
                    if (gear.type.name.Equals(gearType.name) == true)
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
                    Debug.LogFormat("[Gea] PlayerManager.cs -> AddGear: {0}, ID {1}, added to Player inventory{2}", gear.tag, gear.gearID, "\n");
                    CheckForAIUpdate(gear);
                    //add to listOfCurrentGear (if not already present)
                    GameManager.instance.dataScript.AddGearNew(gear);
                    return true;
                }
                else
                { Debug.LogWarningFormat("Gear \'{0}\", gearID {1} is already present in Player inventory", gear.tag, gear.gearID);  }
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
                    Debug.LogError(string.Format("Gear \"{0}\", gearID {1} NOT removed from inventory", gear.tag, gear.gearID));
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
        Debug.Log(string.Format("[Gea] PlayerManager.cs -> RemoveGear: {0}, ID {1}, removed from inventory{2}", gear.tag, gear.gearID, "\n"));
        CheckForAIUpdate(gear);
        //lost gear
        if (isLost == true)
        {
            if (GameManager.instance.dataScript.RemoveGearLost(gear) == false)
            {
                gear.statTurnLost = GameManager.instance.turnScript.Turn;
                Debug.LogWarningFormat("Invalid gear Remove Lost for \"{0}\", gearID {1}", gear.tag, gear.gearID);
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
        if (gear.type.name.Equals(hackingGear) == true)
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
    /// called by GearManager.cs -> ProcessCompromisedGear. Tidies up gear at end of turn. savedGearID is the compromised gear that the player chose to save. Resets all gear
    /// returns name of gear saved (UPPER) [assumes only one piece of gear can be saved per turn], if any. Otherwise returns Empty string
    /// </summary>
    public string UpdateGear(int renownUsed = -1, int savedGearID = -1)
    {
        string gearSavedName = "";
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
                        //if not saved gear then remove
                        if (gear.gearID != savedGearID)
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
            }
        }
        return gearSavedName;
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
                    }               
                    Debug.LogFormat("[Con] PlayerManager.cs -> AddCondition: {0} Player, gains {1} condition{2}", side.name, condition.tag, "\n");
                    if (GameManager.instance.sideScript.PlayerSide.level == side.level)
                    {
                        //message
                        string msgText = string.Format("{0} Player, {1}, gains condition \"{2}\"", side.name, GetPlayerName(side), condition.tag);
                        GameManager.instance.messageScript.ActorCondition(msgText, actorID, true, condition, reason, isResistance);
                    }
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
                    foreach (Condition checkCondition in listOfConditions)
                    {
                        if (checkCondition.tag.Equals(condition.tag) == true)
                        { return true; }
                    }
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
                        if (listOfConditions[i].name.Equals(condition.tag) == true)
                        {
                            listOfConditions.RemoveAt(i);
                            Debug.LogFormat("[Con] PlayerManager.cs -> RemoveCondition: {0} Player, lost {1} condition{2}", side.name, condition.tag, "\n");
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
                    if (checkCorrupt == true && condition.tag.Equals(conditionCorrupt.name) == true)
                    { tempList.Add(conditionCorrupt); checkCorrupt = false; }
                    if (checkIncompetent == true && condition.tag.Equals(conditionIncompetent.name) == true)
                    { tempList.Add(conditionIncompetent); checkIncompetent = false; }
                    if (checkQuestionable == true && condition.tag.Equals(conditionQuestionable.name) == true)
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

    //
    // - - - Secrets - - -
    //

    public List<Secret> GetListOfSecrets()
    { return listOfSecrets; }

    public int CheckNumOfSecrets()
    { return listOfSecrets.Count; }


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
    /// Add a new secret, checks for duplicates and won't add if one found (warning msg)
    /// </summary>
    /// <param name="secret"></param>
    public void AddSecret(Secret secret)
    {
        if (secret != null)
        {
            //check same secret doesn't already exist
            if (listOfSecrets.Exists(x => x.secretID == secret.secretID) == false)
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
                    Debug.LogFormat("[Sec] PlayerManager.cs -> AddSecret: Player learns {0} secret, ID {1}{2}", secret.tag, secret.secretID, "\n");
                }
                else { Debug.LogWarning("Secret NOT added as not enough space"); }
            }
            else { Debug.LogWarningFormat("Duplicate secret already in list, secretID {0}", secret.secretID); }
        }
    }

    /// <summary>
    /// Remove a secret from listOfSecrets. Returns true if successful, false if not found
    /// </summary>
    /// <param name="secretID"></param>
    /// <returns></returns>
    public bool RemoveSecret(int secretID)
    {
        bool isSuccess = false;
        if (listOfSecrets.Exists(x => x.secretID == secretID) == true)
        {
            //reverse loop through and remove secret
            for (int i = listOfSecrets.Count - 1; i >= 0; i--)
            {
                if (listOfSecrets[i].secretID == secretID)
                {
                    Secret secret = listOfSecrets[i];
                    //reset secret known
                    secret.ResetSecret();
                    //add to correct list
                    switch(secret.status)
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
                    Debug.LogFormat("[Sec] PlayerManager.cs -> RemoveSecret: Player REMOVES {0} secret, ID {1}{2}", secret.tag, secret.secretID, "\n");
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
            foreach(Secret secret in listOfPlayerSecrets)
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
        if ( numOfSecrets > 0)
        { secret = listOfSecrets[Random.Range(0, numOfSecrets)]; }
        return secret;
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
                { builder.AppendFormat("{0} ID {1}, {2} ({3}), {4}, Known: {5}", "\n", secret.secretID, secret.name, secret.tag, secret.status, secret.CheckNumOfActorsWhoKnow()); }
        }
        else { builder.AppendFormat("{0} No records", "\n"); }
        return builder.ToString();
    }

    /// <summary>
    /// Debug function to display all player related stats
    /// </summary>
    /// <returns></returns>
    public string DisplayPlayerStats()
    {
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //use correct list for the player side
        List<Condition> listOfConditions = GetListOfConditionForSide(playerSide);
        StringBuilder builder = new StringBuilder();
        builder.Append(string.Format("- {0} Player Stats{1}{2}", playerSide.name, "\n", "\n"));
        builder.Append(string.Format("- Stats{0}", "\n"));
        if (playerSide.level == globalResistance.level)
        { builder.Append(string.Format(" Invisibility {0}{1}", Invisibility, "\n")); }
        builder.Append(string.Format(" Renown {0}{1}", Renown, "\n"));
        if (GameManager.instance.actorScript.doomTimer > 0) { builder.AppendFormat(" Doom Timer {0}{1}", GameManager.instance.actorScript.doomTimer, "\n"); }
        //Conditions
        if (listOfConditions != null)
        {
            builder.Append(string.Format("{0}- Conditions{1}", "\n", "\n"));
            if (listOfConditions.Count > 0)
            {
                for (int i = 0; i < listOfConditions.Count; i++)
                { builder.Append(string.Format(" {0}{1}", listOfConditions[i].name, "\n")); }
            }
            else { builder.AppendFormat(" None{0}", "\n"); }
        }
        else { Debug.LogError("Invalid listOfConditions (Null)"); }
        //Cures
        List<Node> listOfCures = GameManager.instance.dataScript.GetListOfCureNodes();
        if (listOfCures != null)
        {
            builder.Append(string.Format("{0}- Cures{1}", "\n", "\n"));
            if (listOfCures.Count > 0)
            {
                for (int i = 0; i < listOfCures.Count; i++)
                { builder.AppendFormat(" {0} at {1}, {2}, ID {3}{4}", listOfCures[i].cure.cureName, listOfCures[i].nodeName, listOfCures[i].Arc.name, listOfCures[i].nodeID, "\n"); }
            }
            else { builder.AppendFormat(" None{0}", "\n"); }
        }
        else { Debug.LogError("Invalid listOfCures (Null)"); }
        builder.Append(string.Format("{0}- States{1}", "\n", "\n"));
        builder.Append(string.Format(" Status {0}{1}", status, "\n"));
        builder.Append(string.Format(" InactiveStatus {0}{1}", inactiveStatus, "\n"));
        builder.Append(string.Format(" TooltipStatus {0}{1}", tooltipStatus, "\n"));
        builder.Append(string.Format(" isBreakdown {0}{1}", isBreakdown, "\n"));
        builder.Append(string.Format("{0} -Global{1}", "\n", "\n"));
        /*builder.Append(string.Format(" Resistance Cause  {0} of {1}{2}", GameManager.instance.rebelScript.resistanceCause,
            GameManager.instance.rebelScript.resistanceCauseMax, "\n"));
        builder.Append(string.Format(" resistanceState {0}{1}", GameManager.instance.turnScript.resistanceState, "\n"));*/
        builder.Append(string.Format(" authorityState {0}{1}", GameManager.instance.turnScript.authoritySecurityState, "\n"));
        builder.Append(string.Format("{0} -Reserve Pool{1}", "\n", "\n"));
        builder.Append(string.Format(" NumOfRecruits {0} + {1}{2}", GameManager.instance.dataScript.CheckNumOfOnMapActors(playerSide), GameManager.instance.dataScript.CheckNumOfActorsInReserve(), "\n"));
        if (playerSide.level == globalResistance.level)
        {
            //gear
            builder.Append(string.Format("{0}- Gear{1}", "\n", "\n"));
            if (listOfGear.Count > 0)
            {
                for (int i = 0; i < listOfGear.Count; i++)
                {
                    Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
                    if (gear != null)
                    { builder.Append(string.Format(" {0}, ID {1}, {2}{3}", gear.tag, gear.gearID, gear.type.name, "\n")); }
                }
            }
            else { builder.Append(" No gear in inventory"); }
        }
        //stats
        builder.AppendFormat("{0}{1} -Stats{2}", "\n", "\n", "\n");
        builder.AppendFormat(" breakdowns: {0}{1}", GameManager.instance.dataScript.StatisticGetLevel(StatType.PlayerBreakdown), "\n");
        builder.AppendFormat(" lie low: {0}{1}", GameManager.instance.dataScript.StatisticGetLevel(StatType.PlayerLieLow), "\n");
        return builder.ToString();
    }


    /// <summary>
    /// DEBUG method to show players gear in lieu of a working UI element
    /// </summary>
    /// <returns></returns>
    public string DisplayGear()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(" Player's Gear");
        builder.AppendLine();
        foreach (int gearID in listOfGear)
        {
            Gear gear = GameManager.instance.dataScript.GetGear(gearID);
            if (gear != null)
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.Append(string.Format(" {0}", gear.tag.ToUpper()));
                builder.AppendLine();
                /*if (gear.metaLevel != null)
                { builder.Append(string.Format(" Metalevel \"{0}\"", gear.metaLevel.name)); }
                else { builder.Append(" MetaLevel \"None\""); }*/
                builder.AppendLine();
                builder.Append(string.Format(" gearID {0}", gear.gearID));
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
        int gearID = -1;
        if (listOfGear.Count < GameManager.instance.gearScript.maxNumOfGear)
        {
            if (string.IsNullOrEmpty(gearName) == false)
            {
                //find gear in dictionary
                Dictionary<int, Gear> dictOfGear = GameManager.instance.dataScript.GetDictOfGear();
                if (dictOfGear != null)
                {
                    //loop dictionary looking for gear
                    foreach (var gear in dictOfGear)
                    {
                        if (gear.Value.name.Equals(gearName) == true)
                        {
                            gearID = gear.Value.gearID;
                            break;
                        }
                    }
                    if (gearID > -1)
                    {
                        //add gear to player's inventory
                        if (AddGear(gearID) == true)
                        {
                            //remove from pool
                            Gear gear = GameManager.instance.dataScript.GetGear(gearID);
                            if (GameManager.instance.dataScript.RemoveGearFromPool(gear) == false)
                            { Debug.LogWarning("Gear not removed from Pool (Null or other problem)"); }
                            text = string.Format("{0} has been added to the Player's inventory{1}Press ESC to exit", gearName, "\n");
                            //message
                            Node nodePlayer = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
                            GameManager.instance.messageScript.GearObtained(string.Format("{0} added (DEBUG)", gearName), nodePlayer, gear);
                        }
                    }
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
            foreach(var secret in listOfSecrets)
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

    //place new methods above here
}
