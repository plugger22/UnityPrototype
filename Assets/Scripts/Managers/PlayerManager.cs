using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using modalAPI;
using System.Text;

/// <summary>
/// handles all player related data and methods
/// </summary>
public class PlayerManager : MonoBehaviour
{
    public Sprite sprite;

    [HideInInspector] public int numOfRecruits;
    //[HideInInspector] public int Invisibility;
    [HideInInspector] public int actorID = 999;
    [HideInInspector] public ActorStatus status;
    [HideInInspector] public ActorTooltip tooltipStatus;                            //Actor sprite shows a relevant tooltip if tooltipStatus > None (Breakdown, etc)
    [HideInInspector] public ActorInactive inactiveStatus;                          //reason actor is inactive
    [HideInInspector] public bool isBreakdown;                                      //enforces a minimum one turn gap between successive breakdowns
    [HideInInspector] public bool isEndOfTurnGearCheck;                             //set true by UpdateGear (as a result of Compromised gear check)
    [HideInInspector] public bool isLieLowFirstturn;                                //set true when lie low action, prevents invis incrementing on first turn


    private List<int> listOfGear = new List<int>();                                 //gearID's of all gear items in inventory
    private List<Condition> listOfConditionsResistance = new List<Condition>();     //list of all conditions currently affecting the Resistance player
    private List<Condition> listOfConditionsAuthority = new List<Condition>();      //list of all conditions currently affecting the Authority player
    private List<Secret> listOfSecrets = new List<Secret>();                        //list of all secrets (skeletons in the closet)

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
    

    

    //Note: There is no ActorStatus for the player as the 'ResistanceState' handles this -> EDIT: Nope, status does

    public string PlayerName
    {
        get
        {
            if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level) { return _playerNameResistance; }
            else if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level) { return _playerNameAuthority; }
            else
            {
                //AI control of both side
                if (GameManager.instance.turnScript.currentSide.level == globalResistance.level) { return _playerNameResistance; }
                else { return _playerNameAuthority; }
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
                if (GameManager.instance.turnScript.currentSide.level == globalResistance.level) { return _renownResistance; }
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
                if (GameManager.instance.turnScript.Turn > 0)
                { GameManager.instance.aiScript.UpdateSideTabData(_renownResistance); }
                //update renown UI (regardless of whether on or off
                GameManager.instance.actorPanelScript.UpdatePlayerRenownUI(_renownResistance);
            }
            else if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level)
            {
                value = Mathf.Clamp(value, 0, GameManager.instance.actorScript.maxStatValue);
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
            Debug.LogFormat("[Sta] -> PlayerManager.cs:  Player Invisibility changed from {0} to {1}{2}", _invisibility, value, "\n");
            _invisibility = value;
        }
    }


    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        //gear check
        isEndOfTurnGearCheck = false;
        //fast access fields (BEFORE set stats below)
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
        hackingGear = GameManager.instance.gearScript.typeHacking.name;
        maxNumOfSecrets = GameManager.instance.secretScript.secretMaxNum;
        secretStatusActive = GameManager.instance.secretScript.secretStatusActive;
        conditionCorrupt = GameManager.instance.dataScript.GetCondition("CORRUPT");
        conditionIncompetent = GameManager.instance.dataScript.GetCondition("INCOMPETENT");
        conditionQuestionable = GameManager.instance.dataScript.GetCondition("QUESTIONABLE");
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(hackingGear != null, "Invalid hackingGear (Null)");
        Debug.Assert(maxNumOfSecrets > -1, "Invalid maxNumOfSecrets (-1)");
        Debug.Assert(conditionCorrupt != null, "Invalid conditionCorrupt (Null)");
        Debug.Assert(conditionIncompetent != null, "Invalid conditionIncompetent (Null)");
        Debug.Assert(conditionQuestionable != null, "Invalid conditionQuestionable (Null)");
        //Debug
        _playerNameAuthority = "Evil Eddy";
        _playerNameResistance = "Cameron";
        //place Player in a random start location (Sprawl node)
        InitialisePlayerStartNode();
        //set stats
        Renown = 0;
        Invisibility = 3;
        numOfRecruits = GameManager.instance.actorScript.maxNumOfOnMapActors;
        Debug.Assert(numOfRecruits > -1, "Invalid numOfRecruits (-1)");
        //give the player a random secret (PLACEHOLDER -> should be player choice)
        GetRandomStartSecret();
        //register event listeners
        EventManager.instance.AddListener(EventType.EndTurnLate, OnEvent, "PlayerManager.cs");
    }

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
    /// returns true if GearID present in player's inventory
    /// </summary>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public bool CheckGearPresent(int gearID)
    { return listOfGear.Exists(id => id == gearID); }

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


    public List<int> GetListOfGear()
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
    public bool AddGear(int gearID)
    {
        Gear gear = GameManager.instance.dataScript.GetGear(gearID);
        if (listOfGear.Count < GameManager.instance.gearScript.maxNumOfGear)
        {
            if (gear != null)
            {
                //check gear not already in inventory
                if (CheckGearPresent(gearID) == false)
                {
                    ResetGearItem(gear);
                    listOfGear.Add(gearID);
                    Debug.LogFormat("[Gea] PlayerManager.cs -> AddGear: {0}, ID {1}, added to inventory{2}", gear.name, gearID, "\n");
                    CheckForAIUpdate(gear);
                    //add to listOfCurrentGear (if not already present)
                    GameManager.instance.dataScript.AddGearNew(gear);
                    return true;
                }
                else
                { Debug.LogWarningFormat("Gear |'{0}\", gearID {1} is already present in inventory", gear.name, gearID);  }
            }
            else
            { Debug.LogError(string.Format("Invalid gear (Null) for gearID {0}", gearID)); }
        }
        else { Debug.LogWarning("You cannot exceed the maxium number of Gear items -> Gear NOT added"); }
        return false;
    }

    /// <summary>
    /// remove gear from player's inventory. Set isLost to true if the gear is gone forever so that the relevant lists can be updated
    /// </summary>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public bool RemoveGear(int gearID, bool isLost = false)
    {
        Gear gear = GameManager.instance.dataScript.GetGear(gearID);
        if (gear != null)
        {
            //check gear not already in inventory
            if (CheckGearPresent(gearID) == true)
            {
                RemoveGearItem(gear, isLost);
                return true;
            }
            else
            {
                Debug.LogError(string.Format("Gear \"{0}\", gearID {1} NOT removed from inventory", gear.name, gearID));
                return false;
            }
        }
        else
        {
            Debug.LogError(string.Format("Invalid gear (Null) for gearID {0}", gearID));
            return false;
        }
    }

    /// <summary>
    /// subMethod to remove a specific gear item and handle admin
    /// NOTE: gear checked for Null by calling method
    /// </summary>
    /// <param name="gear"></param>
    private void RemoveGearItem(Gear gear, bool isLost)
    {
        ResetGearItem(gear);
        listOfGear.Remove(gear.gearID);
        Debug.Log(string.Format("[Gea] PlayerManager.cs -> RemoveGear: {0}, ID {1}, removed from inventory{2}", gear.name, gear.gearID, "\n"));
        CheckForAIUpdate(gear);
        //lost gear
        if (isLost == true)
        {
            if (GameManager.instance.dataScript.RemoveGearLost(gear) == false)
            {
                gear.statTurnLost = GameManager.instance.turnScript.Turn;
                Debug.LogWarningFormat("Invalid gear Remove Lost for \"{0}\", gearID {1}", gear.name, gear.gearID);
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
                            string msgText = string.Format("{0} ({1}), has been COMPROMISED and LOST", gear.name, gear.type.name);
                            GameManager.instance.messageScript.GearCompromised(msgText, gear, -1);
                            RemoveGearItem(gear, true);
                        }
                        else
                        {
                            //gear saved
                            ResetGearItem(gear);
                            gearSavedName = gear.name.ToUpper();
                            string msgText = string.Format("{0} ({1}), has been COMPROMISED and SAVED", gear.name, gear.type.name);
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
    /// Add a new condition to list provided it isn't already present, ignored if it is. Reason is a short self-contained sentence
    /// </summary>
    /// <param name="condition"></param>
    public void AddCondition(Condition condition, string reason)
    {
        if (condition != null)
        {
            //keep going if reason not provided
            if (string.IsNullOrEmpty(reason) == true)
            {
                reason = "Unknown";
                Debug.LogWarning("Invalid reason (Null or Empty)");
            }
            //use correct list for the player side
            List<Condition> listOfConditions;
            if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level) { listOfConditions = listOfConditionsResistance; }
            else if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level) { listOfConditions = listOfConditionsAuthority; }
            else
            {
                //AI control of both side
                if (GameManager.instance.turnScript.currentSide.level == globalResistance.level) { listOfConditions = listOfConditionsResistance; }
                else { listOfConditions = listOfConditionsAuthority; }
            }

            //check that condition isn't already present
            if (CheckConditionPresent(condition) == false)
            {
                listOfConditions.Add(condition);
                Debug.LogFormat("[Con] PlayerManager.cs -> AddCondition: {0}, Player, gains {1} condition{2}", PlayerName, condition.name, "\n");
                //message
                string msgText = string.Format("Player gains condition \"{0}\"", condition.name);
                GameManager.instance.messageScript.ActorCondition(msgText, actorID, true, condition, reason);
            }
        }
        else { Debug.LogError("Invalid condition (Null)"); }
    }

    /// <summary>
    /// Checks if actor has a specified condition 
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public bool CheckConditionPresent(Condition condition)
    {
        if (condition != null)
        {
            //use correct list for the player side
            List<Condition> listOfConditions;
            if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level) { listOfConditions = listOfConditionsResistance; }
            else if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level) { listOfConditions = listOfConditionsAuthority; }
            else
            {
                //AI control of both side
                if (GameManager.instance.turnScript.currentSide.level == globalResistance.level) { listOfConditions = listOfConditionsResistance; }
                else { listOfConditions = listOfConditionsAuthority; }
            }
            if (listOfConditions.Count > 0)
            {
                foreach (Condition checkCondition in listOfConditions)
                {
                    if (checkCondition.name.Equals(condition.name) == true)
                    { return true; }
                }
            }
        }
        else { Debug.LogError("Invalid condition (Null)"); }
        return false;
    }

    /// <summary>
    /// Removes a specified condition if present, ignored if it isn't
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public bool RemoveCondition(Condition condition, string reason)
    {
        if (condition != null)
        {
            //keep going if reason not provided
            if (string.IsNullOrEmpty(reason) == true)
            {
                reason = "Unknown";
                Debug.LogWarning("Invalid reason (Null or Empty)");
            }
            //use correct list for the player side
            List<Condition> listOfConditions;
            if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level) { listOfConditions = listOfConditionsResistance; }
            else if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level) { listOfConditions = listOfConditionsAuthority; }
            else
            {
                //AI control of both side
                if (GameManager.instance.turnScript.currentSide.level == globalResistance.level) { listOfConditions = listOfConditionsResistance; }
                else { listOfConditions = listOfConditionsAuthority; }
            }
            if (listOfConditions.Count > 0)
            {
                //reverse loop -> delete and return if found
                for (int i = listOfConditions.Count - 1; i >= 0; i--)
                {
                    if (listOfConditions[i].name.Equals(condition.name) == true)
                    {
                        listOfConditions.RemoveAt(i);
                        Debug.LogFormat("[Con] PlayerManager.cs -> RemoveCondition: {0}, Player, lost {1} condition{2}", PlayerName, condition.name, "\n");
                        //message
                        string msgText = string.Format("Player condition \"{0}\" removed", condition.name);
                        GameManager.instance.messageScript.ActorCondition(msgText, actorID, false, condition, reason);
                        return true;
                    }
                }
            }
        }
        else { Debug.LogError("Invalid condition (Null)"); }
        return false;
    }

    public List<Condition> GetListOfConditions()
    {
        //use correct list for the player side
        List<Condition> listOfConditions;
        if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level) { listOfConditions = listOfConditionsResistance; }
        else if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level) { listOfConditions = listOfConditionsAuthority; }
        else
        {
            //AI control of both side
            if (GameManager.instance.turnScript.currentSide.level == globalResistance.level) { listOfConditions = listOfConditionsResistance; }
            else { listOfConditions = listOfConditionsAuthority; }
        }
        return listOfConditions;
    }

    public int CheckNumOfConditions()
    {
        //use correct list for the player side
        List<Condition> listOfConditions;
        if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level) { listOfConditions = listOfConditionsResistance; }
        else if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level) { listOfConditions = listOfConditionsAuthority; }
        else
        {
            //AI control of both side
            if (GameManager.instance.turnScript.currentSide.level == globalResistance.level) { listOfConditions = listOfConditionsResistance; }
            else { listOfConditions = listOfConditionsAuthority; }
        }
        return listOfConditions.Count;
    }

    /// <summary>
    /// Returns a list of all bad conditions (Corrupt/Incompetent/Questionable) present, empty if none
    /// </summary>
    /// <returns></returns>
    public List<Condition> GetNumOfBadConditionPresent()
    {
        List<Condition> tempList = new List<Condition>();
        bool checkCorrupt = true;
        bool checkIncompetent = true;
        bool checkQuestionable = true;
        bool checkDiscredited = true;
        //use correct list for the player side
        List<Condition> listOfConditions;
        if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level) { listOfConditions = listOfConditionsResistance; }
        else if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level) { listOfConditions = listOfConditionsAuthority; }
        else
        {
            //AI control of both side
            if (GameManager.instance.turnScript.currentSide.level == globalResistance.level) { listOfConditions = listOfConditionsResistance; }
            else { listOfConditions = listOfConditionsAuthority; }
        }
        if (listOfConditions.Count > 0)
        {
            foreach (Condition condition in listOfConditions)
            {
                if (checkCorrupt == true && condition.name.Equals(conditionCorrupt.name) == true)
                { tempList.Add(conditionCorrupt); checkCorrupt = false; }
                if (checkIncompetent == true && condition.name.Equals(conditionIncompetent.name) == true)
                { tempList.Add(conditionIncompetent); checkIncompetent = false; }
                if (checkQuestionable == true && condition.name.Equals(conditionQuestionable.name) == true)
                { tempList.Add(conditionQuestionable); checkQuestionable = false; }
            }
        }
        return tempList;
    }

    //
    // - - - Secrets - - -
    //

    public List<Secret> GetListOfSecrets()
    { return listOfSecrets; }

    public int CheckNumOfSecrets()
    { return listOfSecrets.Count; }

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
                    secret.status = secretStatusActive;
                    secret.gainedWhen = GameManager.instance.turnScript.Turn;
                    //Msg
                    GameManager.instance.messageScript.PlayerSecret(string.Format("Player gains new secret ({0})", secret.tag), secret);
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
                    switch(secret.status.level)
                    {
                        case 2:
                            //revealed secret
                            GameManager.instance.dataScript.AddRevealedSecret(secret);
                            break;
                        case 3:
                            //deleted secret
                            GameManager.instance.dataScript.AddDeletedSecret(secret);
                            break;
                    }
                    //remove secret
                    listOfSecrets.RemoveAt(i);
                    isSuccess = true;
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
                { builder.AppendFormat("{0} ID {1}, {2} ({3}), {4}, Known: {5}", "\n", secret.secretID, secret.name, secret.tag, secret.status.name, secret.CheckNumOfActorsWhoKnow()); }
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
        //use correct list for the player side
        List<Condition> listOfConditions;
        if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level) { listOfConditions = listOfConditionsResistance; }
        else if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level) { listOfConditions = listOfConditionsAuthority; }
        else
        {
            //AI control of both side
            if (GameManager.instance.turnScript.currentSide.level == globalResistance.level) { listOfConditions = listOfConditionsResistance; }
            else { listOfConditions = listOfConditionsAuthority; }
        }
        StringBuilder builder = new StringBuilder();
        builder.Append(string.Format(" Player Stats{0}{1}", "\n", "\n"));
        builder.Append(string.Format("- Stats{0}", "\n"));
        if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level)
        { builder.Append(string.Format(" Invisibility {0}{1}", Invisibility, "\n")); }
        builder.Append(string.Format(" Renown {0}{1}", Renown, "\n"));
        builder.Append(string.Format("{0}- Conditions{1}", "\n", "\n"));
        if (listOfConditions.Count > 0)
        {
            for (int i = 0; i < listOfConditions.Count; i++)
            { builder.Append(string.Format(" {0}{1}", listOfConditions[i].name, "\n")); }
        }
        builder.Append(string.Format("{0}- States{1}", "\n", "\n"));
        builder.Append(string.Format(" Status {0}{1}", status, "\n"));
        builder.Append(string.Format(" InactiveStatus {0}{1}", inactiveStatus, "\n"));
        builder.Append(string.Format(" TooltipStatus {0}{1}", tooltipStatus, "\n"));
        builder.Append(string.Format(" isBreakdown {0}{1}", isBreakdown, "\n"));
        builder.Append(string.Format("{0}- Global{1}", "\n", "\n"));
        /*builder.Append(string.Format(" Resistance Cause  {0} of {1}{2}", GameManager.instance.rebelScript.resistanceCause,
            GameManager.instance.rebelScript.resistanceCauseMax, "\n"));*/
        builder.Append(string.Format(" resistanceState {0}{1}", GameManager.instance.turnScript.resistanceState, "\n"));
        builder.Append(string.Format(" authorityState {0}{1}", GameManager.instance.turnScript.authoritySecurityState, "\n"));
        builder.Append(string.Format("{0}- Reserve Pool{1}", "\n", "\n"));
        builder.Append(string.Format(" NumOfRecruits {0} + {1}{2}", numOfRecruits, GameManager.instance.dataScript.CheckNumOfActorsInReserve(), "\n"));
        if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level)
        {
            builder.Append(string.Format("{0}- Gear{1}", "\n", "\n"));
            if (listOfGear.Count > 0)
            {
                for (int i = 0; i < listOfGear.Count; i++)
                {
                    Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
                    if (gear != null)
                    { builder.Append(string.Format(" {0}, ID {1}, {2}{3}", gear.name, gear.gearID, gear.type.name, "\n")); }
                }
            }
            else { builder.Append(" No gear in inventory"); }
        }
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
                builder.Append(string.Format(" {0}", gear.name.ToUpper()));
                builder.AppendLine();
                if (gear.metaLevel != null)
                { builder.Append(string.Format(" Metalevel \"{0}\"", gear.metaLevel.name)); }
                else { builder.Append(" MetaLevel \"None\""); }
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
            Debug.LogWarning("You cannot exceed the maximum num of Gear -> gear not added");
            text = string.Format("{0} NOT added as Gear Allowance MAXXED{1}Press ESC to exit", gearName, "\n");
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
                if (secret.status.level == 0)
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

    //place new methods above here
}
