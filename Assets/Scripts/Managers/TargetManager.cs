using gameAPI;
using modalAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Handles all node target related matters
/// </summary>
public class TargetManager : MonoBehaviour
{

    /*[Tooltip("The % of the total Nodes on the level which commence with a Live target")]
    [Range(0, 20)] public int startPercentTargets = 10;
    [Tooltip("The % of the total Nodes on the level that can have a Live target at any one time")]
    [Range(20, 50)] public int maxPercentTargets = 25;*/

    [Header("Target Activation Chances")]
    [Tooltip("% Chance of target going Live each turn with LOW activation")]
    [Range(1, 50)] public int activateLowChance = 5;
    [Tooltip("% Chance of target going Live each turn with MED activation")]
    [Range(1, 50)] public int activateMedChance = 10;
    [Tooltip("% Chance of target going Live each turn with HIGH activation")]
    [Range(1, 50)] public int activateHighChance = 20;
    [Tooltip("% Chance of target going Live each turn with EXTREME activation")]
    [Range(1, 50)] public int activateExtremeChance = 50;

    [Header("Target Hard Limits")]
    [Tooltip("Number of turns with LOW activation at which target automatically activates if it hasn't already done so via start of turn checks")]
    [Range(1, 50)] public int activateLowLimit = 20;
    [Tooltip("Number of turns with MED activation at which target automatically activates if it hasn't already done so via start of turn checks")]
    [Range(1, 50)] public int activateMedLimit = 10;
    [Tooltip("Number of turns with HIGH activation at which target automatically activates if it hasn't already done so via start of turn checks")]
    [Range(1, 50)] public int activateHighLimit = 5;
    [Tooltip("Number of turns with EXTREME activation at which target automatically activates if it hasn't already done so via start of turn checks")]
    [Range(1, 50)] public int activateExtremeLimit = 2;

    [Header("Target Resolution")]
    [Tooltip("The base chance of a target attempt being successful with no other factors in play")]
    [Range(0, 100)] public int baseTargetChance = 50;
    [Tooltip("How much effect having the right Gear for a target will have on the chance of success")]
    [Range(1, 3)] public int gearEffect = 2;
    [Tooltip("How much effect having the right Actor for a target will have on the chance of success")]
    [Range(1, 3)] public int actorEffect = 2;
    [Tooltip("Maximum amount of target info that can be acquired on a specific target")]
    [Range(1, 3)] public int maxTargetInfo = 3;

    [Header("Warnings")]
    [Tooltip("A Live target will generate a warning message to Resistance player this number of turns before it expires")]
    [Range(1, 3)] public int targetWarning = 2;

    [Header("Default Profile")]
    [Tooltip("Each target should have a TargetProfile. If not and there is no Mission SO profile, this profile is used as the default")]
    public TargetProfile defaultProfile;

    #region Save Compatible Data
    [HideInInspector] public int StartTargets;
    [HideInInspector] public int ActiveTargets;
    [HideInInspector] public int LiveTargets;
    [HideInInspector] public int MaxTargets;
    #endregion

    private List<TargetFactors> listOfFactors = new List<TargetFactors>();              //used to ensure target calculations are consistent across methods

    //fast access
    private GearType infiltrationGear;
    private GlobalSide globalResistance;
    private GlobalSide globalAuthority;
    private int maxGenericOptions = -1;

    //colour Palette
    private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourGear;
    private string colourNormal;
    private string colourDefault;
    private string colourGrey;
    //private string colourAlert;
    //private string colourRebel;
    private string colourTarget;
    private string colourEnd;

    /// <summary>
    /// Initial setup called by MissionManager.cs -> Initialise and higher up by CampaignManager.cs -> Initialise
    /// </summary>
    public void Initialise()
    {
        switch (GameManager.instance.inputScript.GameState)
        {
            case GameState.NewInitialisation:
            case GameState.FollowOnInitialisation:
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseEvents();
                SubInitialiseLevelStart();
                break;
            case GameState.LoadGame:
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
        //fast access
        infiltrationGear = GameManager.instance.dataScript.GetGearType("Infiltration");
        globalResistance = GameManager.instance.globalScript.sideResistance;
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        maxGenericOptions = GameManager.instance.genericPickerScript.maxOptions;
        Debug.Assert(infiltrationGear != null, "Invalid infiltrationGear (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(maxGenericOptions != -1, "Invalid maxGenericOptions (-1)");
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "TargetManager");
        EventManager.instance.AddListener(EventType.StartTurnEarly, OnEvent, "TargetManager");
        EventManager.instance.AddListener(EventType.TargetInfoAction, OnEvent, "TargetManager");
        EventManager.instance.AddListener(EventType.GenericTargetInfo, OnEvent, "TargetManager");
    }
    #endregion

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
        Debug.Assert(activateLowChance < activateMedChance, "invalid activateLowChance (should be less than MED)");
        Debug.Assert(activateMedChance < activateHighChance, "invalid activateMedChance (should be less than HIGH)");
        Debug.Assert(activateHighChance < activateExtremeChance, "invalid activateHighChance (should be less than EXTREME)");
        Debug.Assert(activateLowChance > 0, "Invalid activateLowChance (Zero or Less)");
        Debug.Assert(activateMedChance > 0, "Invalid activateMedChance (Zero or Less)");
        Debug.Assert(activateHighChance > 0, "Invalid activateHighChance (Zero or Less)");
        Debug.Assert(activateExtremeChance > 0, "Invalid activateExtremeChance (Zero or Less)");
        Debug.Assert(activateLowLimit > activateMedLimit, "invalid activateLowLimit (should be more than MED)");
        Debug.Assert(activateMedLimit > activateHighLimit, "invalid activateMedLimit (should be more than HIGH)");
        Debug.Assert(activateHighLimit > activateExtremeLimit, "invalid activateHighLimit (should be more than EXTREME)");
        Debug.Assert(activateLowLimit > 0, "Invalid activateLowLimit (Zero or Less");
        Debug.Assert(activateMedLimit > 0, "Invalid activateMedimit (Zero or Less");
        Debug.Assert(activateHighLimit > 0, "Invalid activateHighLimit (Zero or Less");
        Debug.Assert(activateExtremeLimit > 0, "Invalid activateExtremeLimit (Zero or Less");
        //reset all targets (caters for followOn levels)
        ResetAllTargets();
        //set up generic target array
        InitialiseGenericTargetArray();
        //set up listOfTargetFactors. Note -> Sequence matters and is the order that the factors will be displayed
        foreach (var factor in Enum.GetValues(typeof(TargetFactors)))
        { listOfFactors.Add((TargetFactors)factor); }
    }
    #endregion

    #endregion

    /// <summary>
    /// initialise Generic target array
    /// </summary>
    private void InitialiseGenericTargetArray()
    {
        int index;
        Dictionary<string, Target> dictOfTargets = GameManager.instance.dataScript.GetDictOfTargets();
        if (dictOfTargets != null)
        {
            GameManager.instance.dataScript.InitialiseArrayOfGenericTargets();
            List<string>[] arrayOfGenericTargets = GameManager.instance.dataScript.GetArrayOfGenericTargets();
            if (arrayOfGenericTargets != null)
            {
                //assign targets to pools
                foreach (var target in dictOfTargets)
                {
                    if (target.Value.targetType != null)
                    {
                        switch (target.Value.targetType.name)
                        {
                            case "Generic":
                                if (target.Value.nodeArc != null)
                                {
                                    //only level one targets placed in array
                                    if (target.Value.targetLevel == 1)
                                    {
                                        index = target.Value.nodeArc.nodeArcID;
                                        arrayOfGenericTargets[index].Add(target.Value.name);
                                        /*Debug.LogFormat("[Tst] LoadManager.cs -> InitialiseEarly: Target \"{0}\", nodeArcID {1}, added to arrayOfGenericTargets[{2}]{3}", target.Value.name,
                                            target.Value.nodeArc.nodeArcID, index, "\n");*/
                                    }
                                }
                                else { Debug.LogWarningFormat("Invalid nodeArc for Generic target {0}", target.Value.name); }
                                break;
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid TargetType (Null) for target \"{0}\"", target.Value.name); }
                }
            }
            else { Debug.LogError("Invalid arrayOfGenericTargets (Null)"); }
        }
        else { Debug.LogError("Invalid dictOfTargets (Null)"); }
    }

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
            case EventType.StartTurnEarly:
                StartTurnEarly();
                break;
            case EventType.TargetInfoAction:
                ModalActionDetails details = Param as ModalActionDetails;
                InitialiseGenericPickerTargetInfo(details);
                break;
            case EventType.GenericTargetInfo:
                GenericReturnData returnDataTarget = Param as GenericReturnData;
                ProcessTargetInfo(returnDataTarget);
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
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodText);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralText);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badText);
        colourGear = GameManager.instance.colourScript.GetColour(ColourType.blueText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.whiteText);
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        //colourRebel = GameManager.instance.colourScript.GetColour(ColourType.blueText);
        colourTarget = GameManager.instance.colourScript.GetColour(ColourType.neutralText);
        //colourAlert = GameManager.instance.colourScript.GetColour(ColourType.salmonText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// loops nodes, checks all targets and updates isTargetKnown status
    /// </summary>
    private void StartTurnEarly()
    {
        CheckTargets();
    }

    /// <summary>
    /// Set all targets back to default values
    /// </summary>
    private void ResetAllTargets()
    {
        Dictionary<string, Target> dictOfTargets = GameManager.instance.dataScript.GetDictOfTargets();
        if (dictOfTargets != null)
        {
            foreach (var target in dictOfTargets)
            {
                target.Value.Reset();
                target.Value.nodeID = -1;
                target.Value.targetStatus = Status.Dormant;
            }
        }
        else { Debug.LogError("Invalid dictOfTargets (Null)"); }
    }

    /// <summary>
    /// Checks all targets on map and handles admin and status changes
    /// </summary>
    private void CheckTargets()
    {
        string targetName;
        int rndNum;
        bool isLive;
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        if (listOfNodes != null)
        {
            //loop nodes
            foreach (Node node in listOfNodes)
            {
                targetName = node.targetName;
                //Target present
                if (string.IsNullOrEmpty(targetName) == false)
                {
                    Target target = GameManager.instance.dataScript.GetTarget(targetName);
                    if (target != null)
                    {
                        //
                        // - - - Probe Team - - -
                        //
                        if (node.isTargetKnown == false)
                        {
                            //probe team present -> target known
                            if (node.isProbeTeam == true)
                            { node.isTargetKnown = true; }
                            else
                            {
                                //target automatically known regardless even if completed & not yet contained
                                switch (target.targetStatus)
                                {
                                    case Status.Outstanding:
                                        node.isTargetKnown = true;
                                        break;
                                }
                            }
                        }
                        //
                        // - - - Target status - - -
                        //
                        switch (target.targetStatus)
                        {
                            case Status.Live:
                                if (target.timerWindow == 0)
                                {
                                    Debug.LogFormat("[Tar] TargetManager.cs -> CheckTargets: Target {0} Expired", target.targetName);
                                    string text = string.Format("Target {0} at {1}, {2}, has Expired", target.targetName, node.nodeName, node.Arc.name);
                                    GameManager.instance.messageScript.TargetExpired(text, node, target);
                                    SetTargetDone(target, node);
                                }
                                else
                                {
                                    //warning message -> Resistance player only
                                    if (target.timerWindow == targetWarning && GameManager.instance.sideScript.PlayerSide.level == globalResistance.level)
                                    {
                                        string text = string.Format("Target {0} at {1}, {2}, about to Expire", target.targetName, node.nodeName, node.Arc.name);
                                        GameManager.instance.messageScript.TargetExpiredWarning(text, node, target);
                                    }
                                    target.timerWindow--;
                                }
                                break;
                            case Status.Active:
                                if (target.timerDelay <= 0)
                                {
                                    //activation roll
                                    isLive = false;
                                    target.timerHardLimit++;
                                    rndNum = Random.Range(0, 100);
                                    switch(target.profile.activation.level)
                                    {
                                        case 3:
                                            //Extreme
                                            if (rndNum < activateExtremeChance) { isLive = true; /*Debug.LogFormat("[Tst] Extreme Roll {0} < {1}", rndNum, activateExtremeChance);*/ }
                                            else if (target.timerHardLimit >= activateExtremeLimit) { isLive = true; }
                                            break;
                                        case 2:
                                            //High
                                            if (rndNum < activateHighChance) { isLive = true; /*Debug.LogFormat("[Tst] High Roll {0} < {1}", rndNum, activateHighChance);*/ }
                                            else if (target.timerHardLimit >= activateHighLimit) { isLive = true;  }
                                            break;
                                        case 1:
                                            //Medium
                                            /*Debug.LogFormat("[Tst] target  {0}, ID {1}, Med Roll (need {2}, roll {3}", target.targetName, target.targetID, activateMedChance, rndNum);*/
                                            if (rndNum < activateMedChance) { isLive = true;}
                                            else if (target.timerHardLimit >= activateMedLimit) { isLive = true;  }
                                            break;
                                        case 0:
                                            //Low
                                            if (rndNum < activateLowChance) { isLive = true; /*Debug.LogFormat("[Tst] Low Roll {0} < {1}", rndNum, activateLowChance);*/ }
                                            else if (target.timerHardLimit >= activateLowLimit) { isLive = true; }
                                            break;
                                        default:
                                            Debug.LogWarningFormat("Invalid activation GlobalChance.level {0}", target.profile.activation.level);
                                            break;
                                    }
                                    //Target goes Live
                                    if (isLive == true)
                                    {
                                        target.targetStatus = Status.Live;
                                        GameManager.instance.dataScript.AddTargetToPool(target, Status.Live);
                                        GameManager.instance.dataScript.RemoveTargetFromPool(target, Status.Active);
                                        string text = string.Format("New target {0} at {1}, {2}, id {3}", target.targetName, node.nodeName, node.Arc.name, node.nodeID);
                                        GameManager.instance.messageScript.TargetNew(text, node, target);
                                        Debug.LogFormat("[Tar] TargetManager.cs -> CheckTargets: Target {0} goes LIVE", target.targetName);
                                    }
                                }
                                else
                                { target.timerDelay--; }
                                break;
                        }
                    }
                    /*else { Debug.LogWarning(string.Format("Invalid target (Null) for target {0}", targetName)); } EDIT: Null is acceptable value for a node without a target*/
                }
                else
                {
                    //No target -> reset target known flag if true
                    if (node.isTargetKnown == true)
                    { node.isTargetKnown = false; }
                }
            }
        }
        else { Debug.LogError("Invalid listOfNodes (Null)"); }
    }

    /// <summary>
    /// master method to assign targets at level start
    /// </summary>
    /// <param name="mission"></param>
    public void AssignTargets(Mission mission)
    {
        if (mission != null)
        {
            AssignCityTargets(mission);
            AssignGenericTargets(mission);
            AssignVIPTarget(mission);
            AssignStoryTarget(mission);
            AssignGoalTarget(mission);
        }
        else { Debug.LogError("Invalid mission (Null)"); }
    }

    /// <summary>
    /// Assign city targets to appropriate nodes
    /// </summary>
    private void AssignCityTargets(Mission mission)
    {
        int nodeID;
        Target target;
        //city hall
        if (mission.targetBaseCityHall != null)
        {
            target = mission.targetBaseCityHall;
            nodeID = GameManager.instance.cityScript.cityHallDistrictID;
            Node node = GameManager.instance.dataScript.GetNode(nodeID);
            if (node != null)
            {
                SetTargetDetails(target, node);
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignCityTarget: CityHall node \"{0}\", {1}, id {2}, assigned target \"{3}\"", node.nodeName, node.Arc.name, nodeID,
                    target.targetName);
            }
            else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0} for iconTarget", nodeID); }
        }
        //icon
        if (mission.targetBaseIcon != null)
        {
            target = mission.targetBaseIcon;
            nodeID = GameManager.instance.cityScript.iconDistrictID;
            Node node = GameManager.instance.dataScript.GetNode(nodeID);
            if (node != null)
            {
                SetTargetDetails(target, node);
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignCityTarget: Icon node \"{0}\", {1}, id {2}, assigned target \"{3}\"", node.nodeName, node.Arc.name, nodeID,
                    target.targetName);
            }
            else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0} for iconTarget", nodeID); }
        }
        //airport
        if (mission.targetBaseAirport != null)
        {
            target = mission.targetBaseAirport;
            nodeID = GameManager.instance.cityScript.airportDistrictID;
            Node node = GameManager.instance.dataScript.GetNode(nodeID);
            if (node != null)
            {
                SetTargetDetails(target, node);
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignCityTarget: Airport node \"{0}\", {1}, id {2}, assigned target \"{3}\"", node.nodeName, node.Arc.name, nodeID,
                    target.targetName);
            }
            else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0} for airportTarget", nodeID); }
        }
        //harbour
        if (mission.targetBaseHarbour != null)
        {
            target = mission.targetBaseHarbour;
            nodeID = GameManager.instance.cityScript.harbourDistrictID;
            Node node = GameManager.instance.dataScript.GetNode(nodeID);
            if (node != null)
            {
                SetTargetDetails(target, node);
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignCityTarget: Harbour node \"{0}\", {1}, id {2}, assigned target \"{3}\"", node.nodeName, node.Arc.name, nodeID,
                    target.targetName);
            }
        }
    }

    /// <summary>
    /// Assign the specified number of active and Live Generic targets to specific NodeArc types
    /// </summary>
    /// <param name="mission"></param>
    private void AssignGenericTargets(Mission mission)
    {
        int index, counter, numOfNodes, attempts;
        List<NodeArc> listOfNodeArcs = new List<NodeArc>(GameManager.instance.dataScript.GetDictOfNodeArcs().Values);
        int numActive = mission.targetsGenericLive;
        int numLive = mission.targetsGenericActive;
        int numNodeArcs = listOfNodeArcs.Count;
        //check there are enough nodeArcs to cover the required targets
        if ((numActive + numLive) > numNodeArcs)
        {
            Debug.LogWarningFormat("TargetManager.cs -> AssignGenericTargets: Excess Targets Warning: numActive {0}, numLive {1} vs. numNodeArcs {2}", numActive, numLive, numNodeArcs);
            //downsize target numbers to fit nodeArcs (cut back on Active first)
            if (numLive < numNodeArcs)
            {
                numActive = numNodeArcs - numLive;
                Debug.LogWarningFormat("TargetManager.cs -> AssignGenericTargets: numActive targets now {0} (numLive still {1})", numActive, numLive);
            }
            else
            {
                numLive = numNodeArcs;
                numActive = 0;
                Debug.LogWarningFormat("TargetManager.cs -> AssignGenericTargets: numLive targets now {0} (numActive Zero)", numLive);
            }
            
        }
        List<Node> listOfNodesByType = new List<Node>();
        Node node = null;
        Target target = null;
        //
        // - - - Live Targets first - - -
        //
        counter = 0; attempts = 0;
        do
        {
            if (listOfNodeArcs.Count > 0)
            {
                //get random nodeArc
                index = Random.Range(0, listOfNodeArcs.Count);
                NodeArc nodeArc = listOfNodeArcs[index];
                if (nodeArc != null)
                {
                    //get a random node of that type
                    listOfNodesByType = GameManager.instance.dataScript.GetListOfNodesByType(nodeArc.nodeArcID);
                    numOfNodes = listOfNodesByType.Count;
                    if (numOfNodes > 0)
                    {
                        node = null;
                        //loop through list (can't randomly pick one as the node could already have a target)
                        for (int i = 0; i < numOfNodes; i++)
                        {
                            node = listOfNodesByType[i];
                            if (node != null)
                            {
                                //check node doesn't already have a target
                                if (string.IsNullOrEmpty(node.targetName) == true)
                                { break; }
                                else { node = null; }
                            }
                            else { Debug.LogWarningFormat("Invalid node (Null) in listOfNodesByType for nodeArc {0}, id {1}", nodeArc.name, nodeArc.nodeArcID); }
                        }
                        if (node != null)
                        {
                            //valid node
                            target = GameManager.instance.dataScript.GetRandomGenericTarget(nodeArc.nodeArcID);
                            if (target != null)
                            {
                                //assign target to node
                                SetTargetDetails(target, node, mission.profileGenericLive);
                                Debug.LogFormat("[Tar] MissionManager.cs -> AssignGenericTarget LIVE: node \"{0}\", {1}, id {2}, assigned target \"{3}\"", node.nodeName, node.Arc.name, node.nodeID,
                                    target.targetName);
                                counter++;
                                //delete target to prevent dupes
                                if (GameManager.instance.dataScript.RemoveTargetFromGenericList(target.name, nodeArc.nodeArcID) == false)
                                { Debug.LogErrorFormat("Target not removed from GenericList, target {0}, nodeArc {1}", target.targetName, nodeArc.nodeArcID); }
                            }
                            else { Debug.LogError("Invalid target (Null)"); }
                        }
                    }
                    //delete nodeArc to prevent dupes
                    listOfNodeArcs.RemoveAt(index);
                }
                else { Debug.LogError("Invalid nodeArc (Null) in listOfNodeArcs -> No Target assigned"); }
            }
            else { Debug.LogWarning("No more NodeArcs available"); break; }
            //endless loop prevention
            attempts++;
            if (attempts == 20)
            { Debug.LogFormat("[Tst] TargetManager.cs -> AssignGenericTargets: {0} out of {1} targets  timed out on {2} attempts", counter, numLive,  attempts); }
        }
        while (counter < numLive && attempts < 20);
        //
        // - - - Active Targets - - - 
        //
        counter = 0; attempts = 0;
        do
        {
            if (listOfNodeArcs.Count > 0)
            {
                //get random nodeArc
                index = Random.Range(0, listOfNodeArcs.Count);
                NodeArc nodeArc = listOfNodeArcs[index];
                if (nodeArc != null)
                {
                    //get a random node of that type
                    listOfNodesByType = GameManager.instance.dataScript.GetListOfNodesByType(nodeArc.nodeArcID);
                    numOfNodes = listOfNodesByType.Count;
                    if (numOfNodes > 0)
                    {
                        node = null;
                        //loop through list (can't randomly pick one as the node could already have a target)
                        for (int i = 0; i < numOfNodes; i++)
                        {
                            node = listOfNodesByType[i];
                            if (node != null)
                            {
                                //check node doesn't already have a target
                                if (string.IsNullOrEmpty(node.targetName) == true)
                                { break; }
                                else { node = null; }
                            }
                            else { Debug.LogWarningFormat("Invalid node (Null) in listOfNodesByType for nodeArc {0}, id {1}", nodeArc.name, nodeArc.nodeArcID); }
                        }
                        if (node != null)
                        {
                            //valid node
                            target = GameManager.instance.dataScript.GetRandomGenericTarget(nodeArc.nodeArcID);
                            if (target != null)
                            {
                                //assign target to node
                                SetTargetDetails(target, node, mission.profileGenericActive);
                                Debug.LogFormat("[Tar] MissionManager.cs -> AssignGenericTarget ACTIVE: node \"{0}\", {1}, id {2}, assigned target \"{3}\"", node.nodeName, node.Arc.name, node.nodeID,
                                    target.targetName);
                                counter++;
                                //delete target to prevent dupes
                                if (GameManager.instance.dataScript.RemoveTargetFromGenericList(target.name, nodeArc.nodeArcID) == false)
                                { Debug.LogErrorFormat("Target not removed from GenericList, target {0}, nodeArc {1}", target.targetName, nodeArc.nodeArcID); }
                            }
                            else { Debug.LogError("Invalid target (Null)"); }
                        }
                    }
                    //delete nodeArc to prevent dupes
                    listOfNodeArcs.RemoveAt(index);
                }
                else { Debug.LogError("Invalid nodeArc (Null) in listOfNodeArcs -> No Target assigned"); }
            }
            else { Debug.LogWarning("No more NodeArcs available -> Generic Target allocation cut short"); break; }
            //endless loop prevention
            attempts++;
            if (attempts == 20)
            { Debug.LogFormat("[Tst] TargetManager.cs -> AssignGenericTargets: {0} out of {1} targets  timed out on {2} attempts", counter, numActive, attempts); }
        }
        while (counter < numActive && attempts < 20);
    }

    /// <summary>
    /// Assign a VIP target (typically it's a repeating target) to a random node
    /// </summary>
    /// <param name="mission"></param>
    private void AssignVIPTarget(Mission mission)
    {
        Node node = GameManager.instance.dataScript.GetRandomTargetNode();
        if (node != null)
        {
            if (mission.targetBaseVIP != null)
            {
                Target target = mission.targetBaseVIP;
                //VIP targets don't have follow-on targets (use repeating random node targets instead)
                SetTargetDetails(target, node);
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignVIPTarget: VIP node \"{0}\", {1}, id {2}, assigned target \"{3}\"", node.nodeName, node.Arc.name, node.nodeID,
                    target.targetName);
            }
        }
        else { Debug.LogWarning("Invalid node (Null) for VIPTarget"); }
    }

    /// <summary>
    /// Assign a Story target to a random node
    /// </summary>
    /// <param name="mission"></param>
    private void AssignStoryTarget(Mission mission)
    {
        Node node = GameManager.instance.dataScript.GetRandomTargetNode();
        if (node != null)
        {
            if (mission.targetBaseHarbour != null)
            {
                Target target = mission.targetBaseStory;
                SetTargetDetails(target, node);
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignStoryTarget: Story node \"{0}\", {1}, id {2}, assigned target \"{3}\"", node.nodeName, node.Arc.name, node.nodeID,
                    target.targetName);
            }
        }
        else { Debug.LogWarning("Invalid node (Null) for StoryTarget"); }
    }

    /// <summary>
    /// Assign a Goal target to a random node
    /// </summary>
    /// <param name="mission"></param>
    private void AssignGoalTarget(Mission mission)
    {
        Node node = GameManager.instance.dataScript.GetRandomTargetNode();
        if (node != null)
        {
            if (mission.targetBaseHarbour != null)
            {
                Target target = mission.targetBaseGoal;
                SetTargetDetails(target, node);
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignGoalTarget: Goal node \"{0}\", {1}, id {2}, assigned target \"{3}\"", node.nodeName, node.Arc.name, node.nodeID,
                    target.targetName);
            }
        }
        else { Debug.LogWarning("Invalid node (Null) for GoalTarget"); }
    }

    /// <summary>
    /// Sets target activation, status and adds to relevant pools. Returns true if successful
    /// NOTE: Target and Node checked for null by calling methods
    /// </summary>
    /// <param name="target"></param>
    private bool SetTargetDetails(Target target, Node node, TargetProfile profileOverride = null)
    {
        bool isSuccess = true;
        //check if node doesn't already has a target
        if (string.IsNullOrEmpty(node.targetName) == true)
        {
            //check if target isn't already assigned to a node
            if (target.nodeID == -1)
            {
                //only proceed to assign target if successfully added to list
                if (GameManager.instance.dataScript.AddNodeToTargetList(node.nodeID) == true)
                {
                    //override profile or assign default profile to generic target (generics are override or default) and to all other targets (use existing profile or default if null) if they don't have a profile 
                    if (profileOverride != null)
                    { target.profile = profileOverride; }
                    else if (target.profile == null)
                    { target.profile = defaultProfile; }
                    else if (target.targetType.name.Equals("Generic", StringComparison.Ordinal) == true)
                    { target.profile = defaultProfile; }
                    //profile must be valid
                    if (target.profile != null)
                    {
                        //ID's
                        node.targetName = target.name;
                        target.nodeID = node.nodeID;
                        //timers
                        target.timerDelay = target.profile.delay;
                        target.timerHardLimit = 0;
                        target.timerWindow = target.profile.window;
                        target.turnsWindow = target.profile.window;
                        //defaults (need to set as Target SO could be carrying over data from a previous level)
                        target.Reset();
                        //status and message
                        switch (target.profile.trigger.name)
                        {
                            case "Live":
                                target.targetStatus = Status.Live;
                                string text = string.Format("New target {0}, at {1}, {2}, id {3}", target.targetName, node.nodeName, node.Arc.name, node.nodeID);
                                GameManager.instance.messageScript.TargetNew(text, node, target);
                                break;
                            case "Custom":
                                target.targetStatus = Status.Active;
                                break;
                            default:
                                Debug.LogErrorFormat("Invalid profile.Trigger \"{0}\" for target {1}", target.profile.trigger.name, target.targetName);
                                isSuccess = false;
                                break;
                        }
                        //add to pool
                        if (isSuccess == true)
                        { GameManager.instance.dataScript.AddTargetToPool(target, target.targetStatus); }
                    }
                    else { Debug.LogWarningFormat("Invalid profile (Null) for target {0}", target.targetName); isSuccess = false; }
                }
                else { Debug.LogWarningFormat("Node {0}, {1}, id {2} NOT assigned target {3}", node.nodeName, node.Arc.name, node.nodeID, target.targetName); }
            }
            else
            {
                Debug.LogWarningFormat("Node {0}, {1}, id {2} NOT assigned target {3} (Target already in use at nodeID {4})", node.nodeName, node.Arc.name, node.nodeID, target.targetName, target.nodeID);
                isSuccess = false;
            }
        }
        else { Debug.LogWarningFormat("Node {0}, {1}, id {2} NOT assigned target {3} (Node already has target)", node.nodeName, node.Arc.name, node.nodeID, target.targetName); isSuccess = false;}
        return isSuccess;
    }


    /// <summary>
    /// returns a list of formatted and coloured strings ready for a node Tooltip (Resistance), abbreviate for Authority (if target known), returns an empty list if none
    /// </summary>
    /// <param name="targetID"></param>
    /// <returns></returns>
    public List<string> GetTargetTooltip(string targetName, bool isTargetKnown)
    {
        List<string> tempList = new List<string>();
        if (string.IsNullOrEmpty(targetName) == false)
        {
            //find target
            Target target = GameManager.instance.dataScript.GetTarget(targetName);
            if (target != null)
            {
                switch (GameManager.instance.sideScript.PlayerSide.level)
                {
                    case 1:
                        //Authority
                        if (GameManager.instance.optionScript.fogOfWar == true)
                        {
                            //only show if FOW on and target is known
                            if (isTargetKnown == true)
                            {
                                switch (target.targetStatus)
                                {
                                    case Status.Active:
                                    case Status.Live:
                                        tempList.Add(string.Format("<b>{0} Target</b>", target.targetStatus));
                                        tempList.Add(string.Format("{0}<size=110%>{1}</size>{2}", colourTarget, target.targetName, colourEnd));
                                        tempList.Add(string.Format("Level {0}", target.targetLevel));
                                        break;
                                }
                            }
                            else
                            {
                                switch (target.targetStatus)
                                {
                                    case Status.Outstanding:
                                        tempList.Add(string.Format("<b>{0} Target</b>", target.targetStatus));
                                        tempList.Add(string.Format("{0}<size=110%>{1}</size>{2}", colourTarget, target.targetName, colourEnd));
                                        tempList.Add(string.Format("Level {0}", target.targetLevel));
                                        break;
                                }
                            }
                        }
                        else
                        {
                            //show target details regardless, FOW is Off (Active and Completed only)
                            tempList.AddRange(GetTargetDetails(target));
                        }
                        break;
                    case 2:
                        //Resistance -> target LIVE & Completed
                        if (GameManager.instance.optionScript.fogOfWar == true)
                        {
                            //FOW On, only show Live and Completed
                            switch (target.targetStatus)
                            {
                                case Status.Live:
                                case Status.Outstanding:
                                    tempList.AddRange(GetTargetDetails(target));
                                    break;
                            }
                        }
                        else
                        {
                            //FOW Off -> Show all, even Active ones
                            switch (target.targetStatus)
                            {
                                case Status.Active:
                                case Status.Live:
                                case Status.Outstanding:
                                    tempList.AddRange(GetTargetDetails(target));
                                    break;
                            }
                        }
                        break;
                }
            }
            else
            { Debug.LogError(string.Format("Invalid Target (null) for {0}{1}", targetName, "\n")); }
        }
        return tempList;
    }

    /// <summary>
    /// private subMethod for GetTargetTooltip to collate full target details.
    /// Target tested for null by parent method
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private List<string> GetTargetDetails(Target target)
    {
        List<string> tempList = new List<string>();
        switch (target.targetStatus)
        {
            case Status.Active:
                tempList.Add(string.Format("{0}<b>{1} Target</b>{2}", colourNormal, target.targetStatus, colourEnd));
                tempList.Add(string.Format("{0}<size=110%><b>{1}</b></size>{2}", colourTarget, target.targetName, colourEnd));
                tempList.Add(string.Format("{0}<b>Level {1}</b>{2}", colourDefault, target.targetLevel, colourEnd));
                if (GameManager.instance.optionScript.debugData == true)
                {
                    tempList.Add(string.Format("{0} \"{1}\"", target.targetStatus, target.profile.activation.name));
                    tempList.Add(string.Format("timerDelay {0}", target.timerDelay));
                    tempList.Add(string.Format("timerCountdown {0}", target.timerHardLimit));
                    tempList.Add(string.Format("timerWindow {0}", target.timerWindow));
                }
                break;
            case Status.Live:
                tempList.Add(string.Format("{0}<size=110%><b>{1}</b></size>{2}", colourTarget, target.targetName, colourEnd));

                /*//good effects
                Effect effect = null;
                for (int i = 0; i < target.listOfGoodEffects.Count; i++)
                {
                    effect = target.listOfGoodEffects[i];
                    if (effect != null)
                    { tempList.Add(string.Format("{0}{1}{2}", colourGood, effect.description, colourEnd)); }
                    else { Debug.LogError(string.Format("Invalid effect (null) for \"{0}\", ID {1}{2}", target.targetName, target.targetID, "\n")); }
                }
                //bad effects
                for (int i = 0; i < target.listOfBadEffects.Count; i++)
                {
                    effect = target.listOfBadEffects[i];
                    if (effect != null)
                    { tempList.Add(string.Format("{0}{1}{2}", colourBad, effect.description, colourEnd)); }
                    else { Debug.LogError(string.Format("Invalid effect (null) for \"{0}\", ID {1}{2}", target.targetName, target.targetID, "\n")); }
                }
                //ongoing effects
                if (target.OngoingEffect != null)
                { tempList.Add(string.Format("{0}{1}{2}", colourGood, target.OngoingEffect.description, colourEnd)); }*/

                if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level)
                {
                    //resistance player
                    tempList.Add(string.Format("{0}<b>{1}</b>{2}", colourDefault, target.descriptorResistance, colourEnd));
                    tempList.Add(string.Format("{0}Intel{1}  {2}<b>{3}</b>{4}", colourDefault, colourEnd,
                        GameManager.instance.colourScript.GetValueColour(target.intel), target.intel, colourEnd));
                    tempList.Add(string.Format("{0}<b>{1} gear</b>{2}", colourGear, target.gear.name, colourEnd));
                    tempList.Add(string.Format("{0}<b>{1}</b>{2}", colourGear, target.actorArc.name, colourEnd));
                    //target has an objective?
                    string objectiveInfo = GameManager.instance.objectiveScript.CheckObjectiveInfo(target.name);
                    if (objectiveInfo != null)
                    { tempList.Add(string.Format("{0}<b>{1}</b>{2}", colourTarget, objectiveInfo, colourEnd)); }
                    tempList.Add(string.Format("Available for {0}<b>{1}</b>{2} day{3}", colourNeutral, target.timerWindow, colourEnd, target.timerWindow != 1 ? "s" : ""));
                }
                else
                {
                    //authority player
                    tempList.Add(string.Format("{0}<b>{1}</b>{2}", colourNeutral, target.descriptorAuthority, colourEnd));
                    tempList.Add(string.Format("Exposed for {0}<b>{1}</b>{2} day{3}", colourNeutral, target.timerWindow, colourEnd, target.timerWindow != 1 ? "s" : ""));
                }
                if (GameManager.instance.optionScript.debugData == true)
                {
                    tempList.Add(string.Format("{0} \"{1}\"", target.targetStatus, target.profile.activation.name));
                    tempList.Add(string.Format("timerDelay {0}", target.timerDelay));
                    tempList.Add(string.Format("timerCountdown {0}", target.timerHardLimit));
                    tempList.Add(string.Format("timerWindow {0}", target.timerWindow));
                }
                break;
            case Status.Outstanding:
                //put tooltip together
                tempList.Add(string.Format("{0}Target \"{1}\" has been Completed{2}", colourTarget, target.targetName, colourEnd));
                //ongoing effects
                if (target.ongoingEffect != null)
                {
                    tempList.Add(string.Format("{0}Ongoing effects until contained{1}", colourDefault, colourEnd));
                    tempList.Add(string.Format("{0}{1}{2}", colourGood, target.ongoingEffect.description, colourEnd));
                }
                break;
        }
        return tempList;
    }

    /// <summary>
    /// returns formatted string of all good and bad target effects. Returns 'None' if no effects present
    /// </summary>
    /// <param name="targetID"></param>
    /// <returns></returns>
    public string GetTargetEffects(string targetName, bool isPlayerAction = false)
    {
        List<string> tempList = new List<string>();
        //find target
        Target target = GameManager.instance.dataScript.GetTarget(targetName);
        if (target != null)
        {
            //good effects
            Effect effect = null;
            if (target.listOfGoodEffects.Count > 0)
            {
                //add header
                for (int i = 0; i < target.listOfGoodEffects.Count; i++)
                {
                    effect = target.listOfGoodEffects[i];
                    if (effect != null)
                    { tempList.Add(string.Format("{0}{1}{2}", colourGood, effect.description, colourEnd)); }
                    else { Debug.LogError(string.Format("Invalid Good effect (null) for \"{0}\"{1}", target.targetName, "\n")); }
                }
            }
            //target has an objective?
            string objectiveInfo = GameManager.instance.objectiveScript.CheckObjectiveInfo(target.name);
            if (objectiveInfo != null)
            { tempList.Add(string.Format("{0}{1}{2}", colourGood, objectiveInfo, colourEnd)); }
            //bad effects
            if (target.listOfBadEffects.Count > 0)
            {
                //add header
                for (int i = 0; i < target.listOfBadEffects.Count; i++)
                {
                    effect = target.listOfBadEffects[i];
                    if (effect != null)
                    { tempList.Add(string.Format("{0}{1}{2}", colourBad, effect.description, colourEnd)); }
                    else { Debug.LogError(string.Format("Invalid Bad effect (null) for \"{0}\"{1}", target.targetName, "\n")); }
                }
            }
            //Ongoing effects -> add header
            if (target.ongoingEffect != null)
            { tempList.Add(string.Format("{0}{1} (Ongoing){2}", colourGood, target.ongoingEffect.description, colourEnd)); }
            //Mood effects (player action only)
            if (target.moodEffect != null && isPlayerAction == true)
            { tempList.Add(GameManager.instance.personScript.GetMoodTooltip(target.moodEffect.belief, "Player")); }
        }
        else
        {
            Debug.LogErrorFormat("Invalid Target (null) for {0}{1}", targetName, "\n");
            return null;
        }
        //convert to a string
        StringBuilder builder = new StringBuilder();
        //add header
        builder.AppendFormat("{0}Target Effects{1}", colourTarget, colourEnd);
        if (tempList.Count > 0)
        {
            //Effects
            foreach (string text in tempList)
            {
                if (builder.Length > 0)
                { builder.AppendLine(); }
                builder.Append(text);
            }
        }
        else
        {
            //No effects present
            builder.AppendFormat("{0}None", "\n");
        }
        return builder.ToString();
    }

    /// <summary>
    /// Returns all factors involved in a particular targets resolution (eg. effects chance of success). 
    /// Used by ActorManager.cs -> GetNodeActions for action button tooltip
    /// NOTE: Tweak listOfFactors in Initialise() if you want to change any factors in the calculations
    /// </summary>
    /// <param name="targetID"></param>
    /// <returns></returns>
    public string GetTargetFactors(string targetName)
    {
        List<string> tempList = new List<string>();
        //get target
        Target target = GameManager.instance.dataScript.GetTarget(targetName);
        if (target != null)
        {
            //get node
            Node node = GameManager.instance.dataScript.GetNode(target.nodeID);
            if (node != null)
            {
                //
                // - - - Factors affecting Resolution - - -
                //
                tempList.Add(string.Format("{0}Target Success Chance{1}", colourTarget, colourEnd));
                //base chance
                tempList.Add(string.Format("{0}<size=95%> Base Chance {1}</size>{2}", colourNeutral, baseTargetChance * 0.1, colourEnd));
                //Loop listOfFactors to ensure consistency of calculations across methods
                foreach (TargetFactors factor in listOfFactors)
                {
                    switch (factor)
                    {
                        case TargetFactors.TargetIntel:
                            //good -> info
                            if (target.intel > 0)
                            { tempList.Add(string.Format("{0}<size=95%>Intel {1}{2}</size>{3}", colourGood, target.intel > 0 ? "+" : "", target.intel, colourEnd)); }
                            else { tempList.Add(string.Format("{0}<size=95%>Intel {1}</size>{2}", colourGrey, target.intel, colourEnd)); }
                            break;
                        case TargetFactors.NodeSupport:
                            //good -> support
                            if (node.Support > 0)
                            { tempList.Add(string.Format("{0}<size=95%>Support +{1}</size>{2}", colourGood, node.Support, colourEnd)); }
                            break;
                        case TargetFactors.ActorAndGear:
                            //player or Active Actor?
                            if (GameManager.instance.nodeScript.nodePlayer == node.nodeID)
                            {
                                //Player at node -> active actor not applicable
                                if (target.actorArc != null)
                                { tempList.Add(string.Format("{0}<size=95%>{1}</size>{2}", colourGrey, target.actorArc.name, colourEnd)); }
                                //player has special gear?
                                if (target.gear != null)
                                {
                                    string gearName = GameManager.instance.playerScript.CheckGearTypePresent(target.gear);
                                    if (string.IsNullOrEmpty(gearName) == false)
                                    {
                                        Gear gear = GameManager.instance.dataScript.GetGear(gearName);
                                        if (gear != null)
                                        { tempList.Add(string.Format("{0}<size=95%>{1} +{2}</size>{3}", colourGood, gear.tag, gearEffect * (gear.rarity.level + 1), colourEnd)); }
                                        else { Debug.LogWarning(string.Format("Invalid Target gear (Null) for gear {0}", gearName)); }
                                    }
                                    else
                                    { tempList.Add(string.Format("{0}<size=95%>{1} gear</size>{2}", colourGrey, target.gear.name, colourEnd)); }
                                    //infiltration gear works on any target in addition to special gear
                                    if (target.gear.name.Equals(infiltrationGear.name, StringComparison.Ordinal) == false)
                                    {
                                        //check player has infiltration gear
                                        gearName = GameManager.instance.playerScript.CheckGearTypePresent(infiltrationGear);
                                        if (string.IsNullOrEmpty(gearName) == false)
                                        {
                                            Gear gear = GameManager.instance.dataScript.GetGear(gearName);
                                            if (gear != null)
                                            { tempList.Add(string.Format("{0}<size=95%>{1} +{2}</size>{3}", colourGood, gear.tag, gearEffect * (gear.rarity.level + 1), colourEnd)); }
                                            else { Debug.LogWarning(string.Format("Invalid Infiltration gear (Null) for gear {0}", gearName)); }
                                        }
                                        else
                                        { tempList.Add(string.Format("{0}<size=95%>{1} gear</size>{2}", colourGrey, infiltrationGear.name, colourEnd)); }
                                    }
                                }
                            }
                            else
                            {
                                Actor actor = null;
                                //player not at node ->  check if node active for the correct actor
                                if (target.actorArc != null)
                                {
                                    //check if actor present OnMap and Active (that actor is assumed to be tackling target, the target doesn't have to be part of a contact network)
                                    int slotID = GameManager.instance.dataScript.CheckActorPresent(target.actorArc.name, globalResistance);
                                    if (slotID > -1)
                                    {
                                        //actor present and available
                                        tempList.Add(string.Format("{0}<size=95%>{1} +{2}</size>{3}", colourGood, target.actorArc.name, actorEffect, colourEnd));
                                        actor = GameManager.instance.dataScript.GetCurrentActor(slotID, globalResistance);
                                    }
                                    else
                                    {
                                        //actor either not present or unavailable
                                        tempList.Add(string.Format("{0}<size=95%>{1}</size>{2}", colourGrey, target.actorArc.name, colourEnd));
                                    }
                                }
                                //gear not applicable (only when player at node)
                                if (target.gear != null)
                                {
                                    //only if correct actor arc is present & live
                                    if (actor != null)
                                    {
                                        //actor with actor arc required can use applicable gear just like a player
                                        string gearName = actor.GetGearName();
                                        if (string.IsNullOrEmpty(gearName) == false)
                                        {
                                            Gear gear = GameManager.instance.dataScript.GetGear(gearName);
                                            if (gear != null)
                                            {
                                                //correct type of gear
                                                if (gear.type.name.Equals(target.gear.name, StringComparison.Ordinal) == true)
                                                { tempList.Add(string.Format("{0}<size=95%>{1} +{2}</size>{3}", colourGood, gear.tag, gearEffect * (gear.rarity.level + 1), colourEnd)); }
                                                else
                                                {
                                                    tempList.Add(string.Format("{0}<size=95%>{1} gear</size>{2}", colourGrey, target.gear.name, colourEnd));
                                                    
                                                    /*Debug.LogFormat("[Tst]: TargetManager.cs -> GetTargetFactors: {0}, {1}, gear present but not applicable to target{2}", gear.tag, gear.type.name, "\n");*/

                                                    //infiltration gear can be used
                                                    if (gear.type.name.Equals(infiltrationGear.name, StringComparison.Ordinal) == true)
                                                    { tempList.Add(string.Format("{0}<size=95%>{1} +{2}</size>{3}", colourGood, gear.tag, gearEffect * (gear.rarity.level + 1), colourEnd)); }
                                                    else
                                                    { tempList.Add(string.Format("{0}<size=95%>{1} gear</size>{2}", colourGrey, infiltrationGear.name, colourEnd)); }
                                                }
                                            }
                                            else
                                            { Debug.LogWarning(string.Format("Invalid Target gear (Null) for gear {0}", gearName)); }
                                        }
                                        else
                                        {
                                            tempList.Add(string.Format("{0}<size=95%>{1} gear</size>{2}", colourGrey, target.gear.name, colourEnd));
                                            tempList.Add(string.Format("{0}<size=95%>{1} gear</size>{2}", colourGrey, infiltrationGear.name, colourEnd));
                                        }
                                    }
                                }
                            }
                            break;
                        case TargetFactors.NodeSecurity:
                            //bad -> security
                            tempList.Add(string.Format("{0}<size=95%>District Security {1}{2}</size>{3}", colourBad, node.Security > 0 ? "-" : "", node.Security, colourEnd));
                            break;
                        case TargetFactors.TargetLevel:
                            //bad -> target level
                            tempList.Add(string.Format("{0}<size=95%>Target Level {1}{2}</size>{3}", colourBad, target.targetLevel > 0 ? "-" : "", target.targetLevel, colourEnd));
                            break;
                        case TargetFactors.Teams:
                            if (node.isSecurityTeam == true)
                            { tempList.Add(string.Format("{0}<size=95%>Control Team -{1}</size>{2}", colourBad, GameManager.instance.teamScript.controlNodeEffect, colourEnd)); }
                            break;
                        default:
                            Debug.LogError(string.Format("Unknown TargetFactor \"{0}\"{1}", factor, "\n"));
                            break;
                    }
                }
                //
                // - - - Total - - -
                //
                int tally = GetTargetTally(targetName);
                int chance = GetTargetChance(tally);
                //add tally and chance to string
                tempList.Add(string.Format("{0}<size=95%>Total {1}</size>{2}", colourNeutral, tally, colourEnd));
                tempList.Add(string.Format("{0}{1}SUCCESS {2}%{3}{4}", colourDefault, "<mark=#FFFFFF4D>", chance, "</mark>", colourEnd));
            }
            else
            {
                Debug.LogError(string.Format("Invalid node (null), ID \"{0}\"{1}",target.nodeID, "\n"));
                tempList.Add(string.Format("{0}{1}{2}", colourBad, "Target Data inaccessible", colourEnd));
            }
        }
        else
        {
            Debug.LogError(string.Format("Invalid Target (null), \"{0}\"{1}", targetName, "\n"));
            tempList.Add(string.Format("{0}{1}{2}", colourBad, "Target Data inaccessible", colourEnd));
        }
        //convert list to string and return
        StringBuilder builder = new StringBuilder();
        foreach(string text in tempList)
        {
            if (builder.Length > 0)
            { builder.AppendLine(); }
            builder.Append(text);
        }
        return builder.ToString();
    }

    /// <summary>
    /// returns tally of all factors in target success, eg. -2, +1, etc. SetGearUsed as true only when target actually attempted.
    /// NOTE: Tweak listOfFactors in Initialise() if you want to change any factors in the calculations
    /// </summary>
    /// <param name="targetID"></param>
    /// <returns></returns>
    public int GetTargetTally(string targetName, bool setGearAsUsed = false)
    {
        int tally = 0;
        //base chance
        tally += (int)(baseTargetChance * 0.1);
        //get target
        Target target = GameManager.instance.dataScript.GetTarget(targetName);
        if (target != null)
        {
            //get node
            Node node = GameManager.instance.dataScript.GetNode(target.nodeID);
            if (node != null)
            {
            //Loop listOfFactors to ensure consistency of calculations across methods
            foreach(TargetFactors factor in listOfFactors)
                {
                    switch (factor)
                    {
                        case TargetFactors.TargetIntel:
                            //good -> info
                            tally += target.intel;
                            break;
                        case TargetFactors.NodeSupport:
                            //good -> support
                            if (node.Support > 0)
                            { tally += node.Support; }
                            break;
                        case TargetFactors.ActorAndGear:
                            //player or Active Actor?
                            if (GameManager.instance.nodeScript.nodePlayer == node.nodeID)
                            {
                                //player has special gear?
                                if (target.gear != null)
                                {
                                    string gearName = GameManager.instance.playerScript.CheckGearTypePresent(target.gear);
                                    if (string.IsNullOrEmpty(gearName) == false)
                                    {
                                        Gear gear = GameManager.instance.dataScript.GetGear(gearName);
                                        if (gear != null)
                                        {
                                            tally += gearEffect * (gear.rarity.level + 1);
                                            //gear used?
                                            if (setGearAsUsed == true)
                                            { GameManager.instance.gearScript.SetGearUsed(gear, "attempt Target"); }
                                        }
                                        else { Debug.LogWarningFormat("Invalid Target gear (Null) for gear {0}", gearName); }
                                    }
                                    //infiltration gear works on any target in addition to special gear
                                    if (target.gear.name.Equals(infiltrationGear.name, StringComparison.Ordinal) == false)
                                    {
                                        //check player has infiltration gear
                                        gearName = GameManager.instance.playerScript.CheckGearTypePresent(infiltrationGear);
                                        if (string.IsNullOrEmpty(gearName) == false)
                                        {
                                            Gear gear = GameManager.instance.dataScript.GetGear(gearName);
                                            if (gear != null)
                                            {
                                                tally += gearEffect * (gear.rarity.level + 1);
                                                //gear used?
                                                if (setGearAsUsed == true)
                                                { GameManager.instance.gearScript.SetGearUsed(gear, "attempt Target"); }
                                            }
                                            else { Debug.LogWarningFormat("Invalid Infiltration gear (Null) for gear {0}", gearName); }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //player NOT at node ->  check if actor is present in OnMap line-up
                                if (target.actorArc != null)
                                {
                                    Actor actor = null;
                                    int slotID = GameManager.instance.dataScript.CheckActorPresent(target.actorArc.name, globalResistance);
                                    if (slotID > -1)
                                    {
                                        //actor of the required actor Arc is present
                                        tally += actorEffect;
                                        actor = GameManager.instance.dataScript.GetCurrentActor(slotID, globalResistance);
                                        if (actor != null)
                                        {
                                            if (target.gear != null)
                                            {
                                                //actor, of the required actor arc type, present and available. Can use gear same as a Player
                                                string gearName = actor.GetGearName();
                                                if (string.IsNullOrEmpty(gearName) == false)
                                                {
                                                    Gear gear = GameManager.instance.dataScript.GetGear(gearName);
                                                    if (gear != null)
                                                    {
                                                        //correct type of gear
                                                        if (gear.type.name.Equals(target.gear.name, StringComparison.Ordinal) == true)
                                                        {
                                                            tally += gearEffect * (gear.rarity.level + 1);
                                                            if (setGearAsUsed == true)
                                                            { GameManager.instance.gearScript.SetGearUsed(gear, "attempt Target"); }
                                                        }
                                                        else
                                                        {
                                                            //infiltration gear can be used
                                                            if (gear.type.name.Equals(infiltrationGear.name, StringComparison.Ordinal) == true)
                                                            {
                                                                tally += gearEffect * (gear.rarity.level + 1);
                                                                if (setGearAsUsed == true)
                                                                { GameManager.instance.gearScript.SetGearUsed(gear, "attempt Target"); }
                                                            }
                                                        }
                                                    }
                                                    else { Debug.LogErrorFormat("Invalid gear (Null) for gear {0}", gearName); }
                                                }
                                            }
                                        }
                                        else { Debug.LogErrorFormat("Invalid actor (Null) for slotID {0}", slotID); }
                                    }
                                }
                            }
                            break;
                        case TargetFactors.NodeSecurity:
                            //bad -> security
                            tally -= node.Security;
                            break;
                        case TargetFactors.TargetLevel:
                            //bad -> target level
                            tally -= target.targetLevel;
                            break;
                        case TargetFactors.Teams:
                            //Teams
                            if (node.isSecurityTeam == true)
                            { tally -= GameManager.instance.teamScript.controlNodeEffect; }
                            break;
                        default:
                            Debug.LogError(string.Format("Unknown TargetFactor \"{0}\"{1}", factor, "\n"));
                            break;
                    }
                }
            }
            else
            { Debug.LogError(string.Format("Invalid node (null), ID \"{0}\"{1}", target.nodeID, "\n")); }
        }
        else
        { Debug.LogError(string.Format("Invalid Target (null), \"{0}\"{1}", targetName, "\n")); }
        return tally;
    }

    /// <summary>
    /// returns tally of all factors in target success, eg. -2, +1, etc. AI version. Ignores intel and gear (done by calling method in AIRebelManager.cs -> ProcessTargetTask)
    /// </summary>
    /// <param name="targetID"></param>
    /// <returns></returns>
    public int GetTargetTallyAI(string targetName)
    {
        int tally = 0;
        //base chance
        tally += (int)(baseTargetChance * 0.1);
        //get target
        Target target = GameManager.instance.dataScript.GetTarget(targetName);
        if (target != null)
        {
            //get node
            Node node = GameManager.instance.dataScript.GetNode(target.nodeID);
            if (node != null)
            {
                //Loop listOfFactors to ensure consistency of calculations across methods
                foreach (TargetFactors factor in listOfFactors)
                {
                    switch (factor)
                    {
                        case TargetFactors.TargetIntel:
                            break;
                        case TargetFactors.NodeSupport:
                            //good -> support
                            if (node.Support > 0)
                            { tally += node.Support; }
                            break;
                        case TargetFactors.ActorAndGear:
                            break;
                        case TargetFactors.NodeSecurity:
                            //bad -> security
                            tally -= node.Security;
                            break;
                        case TargetFactors.TargetLevel:
                            //bad -> target level
                            tally -= target.targetLevel;
                            break;
                        case TargetFactors.Teams:
                            //Teams
                            if (node.isSecurityTeam == true)
                            { tally -= GameManager.instance.teamScript.controlNodeEffect; }
                            break;
                        default:
                            Debug.LogError(string.Format("Unknown TargetFactor \"{0}\"{1}", factor, "\n"));
                            break;
                    }
                }
            }
            else
            { Debug.LogError(string.Format("Invalid node (null), ID \"{0}\"{1}", target.nodeID, "\n")); }
        }
        else
        { Debug.LogError(string.Format("Invalid Target (null), \"{0}\"{1}", targetName, "\n")); }
        return tally;
    }

    /// <summary>
    /// returns % chance (whole numbers) of target resolution being a success
    /// Formula -> baseTargetChance + tally * 10
    /// </summary>
    /// <param name="tally"></param>
    /// <returns></returns>
    public int GetTargetChance(int tally)
    {
        int chance = tally * 10;
        chance = Mathf.Clamp(chance, 0, 100);
        return chance;
    }


    /// <summary>
    /// Contains a completed (Oustanding) target as a result of Damage team intervention. Handles all related matters.
    /// Note: target is checked for Null by the calling method
    /// </summary>
    /// <param name="target"></param>
    public void ContainTarget(Target target)
    {
        Node node = GameManager.instance.dataScript.GetNode(target.nodeID);
        if (node != null)
        {
            GameManager.instance.connScript.RemoveOngoingEffect(target.ongoingID);
            GameManager.instance.nodeScript.RemoveOngoingEffect(target.ongoingID);
            //admin
            SetTargetDone(target, node);
        }
        else { Debug.LogError(string.Format("Invalid node (Null) for target.nodeID {0}", target.nodeID)); }
    }

    /// <summary>
    /// Called whenever a target is done (finished OnMap, eg. Contained or Completed with no ongoing effects or timed out (window). Handles all admin. Returns true if all O.K
    /// </summary>
    /// <param name="target"></param>
    /// <param name="node"></param>
    public bool SetTargetDone(Target target, Node node)
    {
        bool isSuccess = true;
        bool isDone = true;
        if (node != null)
        {
            if (target != null)
            {
                //remove from current target pool
                switch (target.targetStatus)
                {
                    case Status.Live:
                        GameManager.instance.dataScript.RemoveTargetFromPool(target, Status.Live);
                        break;
                    case Status.Active:
                        GameManager.instance.dataScript.RemoveTargetFromPool(target, Status.Active);
                        break;
                    case Status.Outstanding:
                        GameManager.instance.dataScript.RemoveTargetFromPool(target, Status.Outstanding);
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid targetStatus \"{0}\". Lists not updated", target.targetStatus);
                        isSuccess = false;
                        break;
                }
                //continue?
                if (isSuccess == true)
                {
                    //remove from list
                    if (GameManager.instance.dataScript.RemoveNodeFromTargetList(node.nodeID) == false)
                    { Debug.LogWarningFormat("Node id {0} NOT removed from listOfNodesWithTargets", node.nodeID); }
                    //id's back to default
                    node.targetName = null;
                    target.nodeID = -1;
                    //
                    // - - - Follow On target
                    //
                    if (target.followOnTarget != null)
                    {
                        //Success required to generate a followOn target?
                        if (target.isSuccessNeeded == true && target.turnSuccess == -1)
                        { Debug.LogFormat("[Tar] TargetManager.cs -> SetTargetDone: FollowOn for target \"{0}\" Invalid as target not successfully completed", target.targetName); }
                        else
                        {
                            switch (target.targetType.name)
                            {
                                case "Generic":
                                    //Generic can't repeat and must follow On to the same node
                                    Mission mission = GameManager.instance.missionScript.mission;
                                    //use override profile if one available (ignored if not)
                                    if (mission != null)
                                    {
                                        if (mission.profileGenericFollowOn != null)
                                        {
                                            if (SetTargetDetails(target.followOnTarget, node, mission.profileGenericFollowOn) == true)
                                            {
                                                Debug.LogFormat("[Tar] TargetManager.cs -> SetTargetDone: Node (same) \"{0}\", {1}, id {2}, assigned follow On GENERIC target \"{3}\"", node.nodeName,
                                                    node.Arc.name, node.nodeID, target.followOnTarget.targetName);
                                            }
                                        }
                                        else
                                        {
                                            if (SetTargetDetails(target.followOnTarget, node) == true)
                                            {
                                                Debug.LogFormat("[Tar] TargetManager.cs -> SetTargetDone: Node (same) \"{0}\", {1}, id {2}, assigned follow On GENERIC target \"{3}\"", node.nodeName,
                                                    node.Arc.name, node.nodeID, target.followOnTarget.targetName);
                                            }
                                        }
                                    }
                                    else { Debug.LogError("Invalid mission (Null)"); }
                                    break;
                                case "City":
                                    //City must follow On to the same node
                                    if (SetTargetDetails(target.followOnTarget, node) == true)
                                    {
                                        Debug.LogFormat("[Tar] TargetManager.cs -> SetTargetDone: Node (same) \"{0}\", {1}, id {2}, assigned follow On City target \"{3}\"", node.nodeName, node.Arc.name,
                                          node.nodeID, target.followOnTarget.targetName);
                                    }
                                    break;
                                default:
                                    //Story / VIP / Goal Follow On targets -> random node
                                    Node nodeRandom = GameManager.instance.dataScript.GetRandomTargetNode();
                                    if (nodeRandom != null)
                                    {
                                        if (SetTargetDetails(target.followOnTarget, nodeRandom) == true)
                                        {
                                            Debug.LogFormat("[Tar] TargetManager.cs -> SetTargetDone: Node (random) \"{0}\", {1}, id {2}, assigned follow On target \"{3}\"", nodeRandom.nodeName,
                                                nodeRandom.Arc.name, nodeRandom.nodeID, target.followOnTarget.targetName);
                                        }
                                    }
                                    else { Debug.LogError("Invalid nodeRandom (Null), Target not assigned"); }
                                    break;
                            }
                        }
                    }
                    //
                    // - - - Repeat target - - -
                    //
                    else if (target.profile.isRepeat == true)
                    {
                        //target can't have been done to repeat.
                        if (target.turnSuccess == -1)
                        {
                            //Generic targets can't repeat
                            if (target.targetType.name.Equals("Generic", StringComparison.Ordinal) == false)
                            {
                                isDone = false;
                                //assign repeat profile if present
                                if (target.profile.repeatProfile != null)
                                { target.profile = target.profile.repeatProfile; }
                                else { Debug.LogWarningFormat("TargetManager.cs -> SetTargetDone: target {0}, repeatProfile Invalid (Null)", target.targetName); }
                                //same node
                                if (target.profile.isRepeatSameNode == true)
                                {
                                    if (SetTargetDetails(target, node) == true)
                                    {
                                        Debug.LogFormat("[Tar] TargetManager.cs -> SetTargetDone: Node (same) \"{0}\", {1}, id {2}, assigned REPEAT target \"{3}\"", node.nodeName, node.Arc.name,
                                          node.nodeID, target.targetName);
                                    }
                                }
                                //random node
                                else
                                {
                                    //City targets can't have random repeats
                                    if (target.targetType.name.Equals("City", StringComparison.Ordinal) == false)
                                    {
                                        Node nodeRandom = GameManager.instance.dataScript.GetRandomTargetNode();
                                        if (nodeRandom != null)
                                        {
                                            if (SetTargetDetails(target, nodeRandom) == true)
                                            {
                                                Debug.LogFormat("[Tar] TargetManager.cs -> SetTargetDone: Node (random) \"{0}\", {1}, id {2}, assigned REPEAT target \"{3}\"", nodeRandom.nodeName,
                                                    nodeRandom.Arc.name, nodeRandom.nodeID, target.targetName);
                                            }
                                        }
                                        else { Debug.LogError("Invalid nodeRandom (Null), Target not assigned"); }
                                    }
                                    else
                                    {
                                        Debug.LogWarningFormat("CITY target {0}, can't have a Repeating RANDOM node (must be Same)", target.targetName);
                                        //set target as done, put in pool, etc. (below)
                                        isDone = true;
                                    }
                                }
                            }
                            else { Debug.LogWarningFormat("TargetManager.cs -> SetTargetDone: target {0}, can't REPEAT as GENERIC", target.targetName); }
                        }
                        else { Debug.LogFormat("[Tar] TargetManager.cs -> SetTargetDone: target {0}, can't REPEAT as successfully attempted", target.targetName); }
                    }
                    else
                    {
                        //no follow on, no repeat, successfully handled done admin
                        Debug.LogFormat("[Tar] TargetManager.cs -> SetTargetDone: Target \"{0}\", DONE admin completed", target.targetName);
                    }
                    //
                    // - - - Target Done (no repeat, no follow On) - - -
                    //
                    if (isSuccess == true && isDone == true)
                    {
                        target.targetStatus = Status.Done;
                        target.turnDone = GameManager.instance.turnScript.Turn;
                        //Add to pool 
                        GameManager.instance.dataScript.AddTargetToPool(target, Status.Done);
                    }
                }
            }
            else { Debug.LogError("Invalid target (Null)"); isSuccess = false; }
        }
        else { Debug.LogError("Invalid node (Null)"); isSuccess = false; }
        if (isSuccess == false)
        { Debug.LogWarningFormat("TargetManager.cs -> SetTargetDone: Target \"{0}\", DONE admin FAILED", target.targetName); }
        return isSuccess;
    }


    /// <summary>
    /// Choose Target for gaining Target Info (Resistance only): sets up ModalGenericPicker class and triggers event: ModalGenericEvent.cs -> SetGenericPicker()
    /// </summary>
    /// <param name="details"></param>
    private void InitialiseGenericPickerTargetInfo(ModalActionDetails details)
    {
        //first Gear Pick this action
        bool errorFlag = false;
        int count;
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        Node node = GameManager.instance.dataScript.GetNode(details.nodeID);
        if (node != null)
        {

            #region CaptureCheck
            //check for player/actor being captured
            int actorID = GameManager.instance.playerScript.actorID;
            if (node.nodeID != GameManager.instance.nodeScript.nodePlayer)
            {
                Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorDataID, globalResistance);
                if (actor != null)
                { actorID = actor.actorID; }
                else { Debug.LogError(string.Format("Invalid actor (Null) from details.ActorSlotID {0}", details.actorDataID)); errorFlag = true; }
            }
            //check capture provided no errors
            if (errorFlag == false)
            {
                CaptureDetails captureDetails = GameManager.instance.captureScript.CheckCaptured(node.nodeID, actorID);
                if (captureDetails != null)
                {
                    //capture happened, abort recruitment
                    captureDetails.effects = string.Format("{0}The contact wasn't there. It was a wash.{1}", colourNeutral, colourEnd);
                    EventManager.instance.PostNotification(EventType.Capture, this, captureDetails, "TargetManager.cs -> InitialiseGenericPickerTargetInfo");
                    return;
                }
            }
            else
            {
                //reset flag to the default state prior to recruitments
                errorFlag = false;
            }
            #endregion


            //proceed with a new target Selection

                #region TargetSelection
                //Obtain Info
                genericDetails.returnEvent = EventType.GenericTargetInfo;
                genericDetails.textHeader = "Select Target";
                genericDetails.side = globalResistance;
                genericDetails.nodeID = details.nodeID;
                genericDetails.actorSlotID = details.actorDataID;
                //picker text
                genericDetails.textTop = string.Format("{0}Targets{1} {2}available{3}", colourNeutral, colourEnd, colourNormal, colourEnd);
                genericDetails.textMiddle = string.Format("{0}Closer the Target the better the Intel{1}",
                    colourNormal, colourEnd);
                genericDetails.textBottom = "Click on a Target to Select. Press CONFIRM. Mouseover target for more information.";
                //generate temp list of gear to choose from

            List<Target> listOfLiveTargets = GameManager.instance.dataScript.GetTargetPool(Status.Live);
            if (listOfLiveTargets != null)
            {
                //
                //select three Targets for the picker 
                //
                count = listOfLiveTargets.Count;
                if (count < 1)
                {
                    //OUTCOME -> No targets available
                    Debug.LogWarning("TargetManager: No Targets available in InitaliseGenericPickerTargetInfo");
                    errorFlag = true;
                }
                else
                {
                    //dict to hold key -> distance to target from current node, value -> target (live and space for more intel)
                    Dictionary<Target, int> dictOfTargetDistances = new Dictionary<Target, int>();
                    int distance;
                    //loop live Targets
                    for (int i = 0; i < count; i++)
                    {
                        Target target = listOfLiveTargets[i];
                        if (target != null)
                        {
                            //has space for more intel
                            if (target.intel < maxTargetInfo)
                            {
                                distance = -1;
                                //get distance
                                if (target.nodeID == node.nodeID)
                                { dictOfTargetDistances.Add(target, 0); distance = 0; }
                                else
                                {
                                    distance = GameManager.instance.dijkstraScript.GetDistanceUnweighted(node.nodeID, target.nodeID);
                                    if (distance > -1)
                                    { dictOfTargetDistances.Add(target, distance); }
                                    else { Debug.LogErrorFormat("Invalid dijkstra distance between nodeID {0} and target nodeID {1}", node.nodeID, target.nodeID); }
                                }
                                target.distance = distance;
                                target.newIntel = GetTargetInfoNew(distance, target.intel);
                                target.intelGain = GetTargetInfoGain(distance);
                            }
                            else
                            {
                                //zero out dynamic fields
                                target.distance = 0;
                                target.newIntel = 0;
                                target.intelGain = 0;
                            }
                        }
                        else { Debug.LogErrorFormat("Invalid target (Null) for listOfLiveTargets[{0}]", i); }
                    }
                    //any records in dictionary
                    count = dictOfTargetDistances.Count;
                    if (count > 0)
                    {
                        //sort by distance
                        var sortedDistance = from pair in dictOfTargetDistances orderby pair.Value ascending select pair.Key;
                        List<Target> tempListSorted = sortedDistance.ToList();
                        //Put generic picker data package together
                        count = Mathf.Min(maxGenericOptions, count);
                        Target[] arrayOfTargets = new Target[count];
                        
                        //transfer list targets to array targets
                        for (int i = 0; i < count; i++)
                        { arrayOfTargets[i] = tempListSorted[i]; }
                        //
                        //loop targetID's that have been selected and package up ready for ModalGenericPicker
                        //
                        for (int i = 0; i < count; i++)
                        {
                            Target target = arrayOfTargets[i];
                            if (target != null)
                            {
                                //tooltip 
                                GenericTooltipDetails tooltipDetails = GetTargetTooltipGeneric(target);
                                if (tooltipDetails != null)
                                {
                                    //option details
                                    GenericOptionDetails optionDetails = new GenericOptionDetails();
                                    /*optionDetails.optionID = target.targetID;*/
                                    optionDetails.optionName = target.name;
                                    optionDetails.text = target.targetName.ToUpper();
                                    optionDetails.sprite = GameManager.instance.guiScript.targetInfoSprite;
                                    //add to master arrays
                                    genericDetails.arrayOfOptions[i] = optionDetails;
                                    genericDetails.arrayOfTooltips[i] = tooltipDetails;
                                }
                                else { Debug.LogErrorFormat("Invalid tooltip Details (Null) for target {0}", target.targetName); }
                            }
                            else { Debug.LogErrorFormat("Invalid target (Null) for target {0}", target.targetName); }
                        }
                    }
                    else { Debug.LogWarning("NOTIFICATION Only: No records in dictOfTargetDistances"); }
                }
                #endregion
            }
            else { Debug.LogError("Invalid listOfLiveTargets (Null)"); }
        }
        else
        {
            Debug.LogError(string.Format("Invalid Node (null) for nodeID {0}", details.nodeID));
            errorFlag = true;
        }
        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = globalResistance;
            outcomeDetails.textTop = "There has been an error in communication and no Targets can be located.";
            outcomeDetails.textBottom = "Backsides will be kicked!";
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "TargetManager.cs -> InitialiseGenericPickerTargetInfo");
        }
        else
        {
            //deactivate back button
            GameManager.instance.genericPickerScript.SetBackButton(EventType.None);
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails, "TargetManager.cs -> InitialiseGenericPickerTargetInfo");
        }
    }

    /// <summary>
    /// returns a data package of 3 formatted strings ready to slot into a gear tooltip. Null if a problem.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private GenericTooltipDetails GetTargetTooltipGeneric(Target target)
    {
        GenericTooltipDetails details = null;
        if (target != null)
        {
            details = new GenericTooltipDetails();
            StringBuilder builderHeader = new StringBuilder();
            StringBuilder builderMain = new StringBuilder();
            StringBuilder builderDetails = new StringBuilder();
            string colourIntel = GameManager.instance.colourScript.GetValueColour(target.intel);
            builderHeader.AppendFormat("{0}{1}{2}", colourGear, target.targetName, colourEnd);
            builderMain.AppendFormat("<b>Existing Intel {0}{1}{2}</b>{3}", colourIntel, target.intel, colourEnd, "\n");
            builderMain.AppendFormat("{0}<size=130%>+{1}{2} Intel{3}{4}</size>MAX Intel allowed is {5}{6}", colourNeutral, target.intelGain, colourEnd, "\n", colourTarget, maxTargetInfo, colourEnd);
            builderDetails.AppendFormat("Distance {0}{1}{2}", colourNeutral, target.distance, colourEnd);
            
            details.textHeader = builderHeader.ToString();
            details.textMain = builderMain.ToString();
            details.textDetails = builderDetails.ToString();
        }
        else { Debug.LogError("Invalid target (Null)"); }
        return details;
    }

    /// <summary>
    /// Process Planner gain target info for selected target (from generic picker)
    /// </summary>
    /// <param name="details"></param>
    private void ProcessTargetInfo(GenericReturnData detailsGeneric)
    {
        Target target = GameManager.instance.dataScript.GetTarget(detailsGeneric.optionName);
        if (target != null)
        {
            //Get actor
            Actor actor = GameManager.instance.dataScript.GetCurrentActor(detailsGeneric.actorSlotID, globalResistance);
            if (actor != null)
            {
                Sprite sprite = actor.sprite;
                Node node = GameManager.instance.dataScript.GetNode(detailsGeneric.nodeID);
                if (node != null)
                {
                    StringBuilder builderTop = new StringBuilder();
                    StringBuilder builderBottom = new StringBuilder();
                    //update target info level
                    target.intel = target.newIntel;
                    if (target.intel > maxTargetInfo)
                    { target.intel = maxTargetInfo; }
                    builderTop.AppendFormat("{0}{1} target{2}{3}{4}", colourGear, target.targetName, colourEnd, "\n", "\n");
                    builderTop.AppendFormat("{0}+{1}{2} INTEL gained, now {3}{4}{5}", colourNeutral, target.intelGain, colourEnd, GameManager.instance.colourScript.GetValueColour(target.intel), 
                        target.intel, colourEnd);
                    //message
                    Debug.LogFormat("[Tar] TargetManager.cs -> ProcessTargetInfo: {0} at {1}, {2}, id {3}, Intel +{4}, now {5} (PLANNER action){6}", target.targetName,
                        node.nodeName, node.Arc.name, node.nodeID, target.intelGain, target.intel, "\n");
                    //Process any other effects, if acquisition was successfull, ignore otherwise
                    Action action = actor.arc.nodeAction;
                    EffectDataInput dataInput = new EffectDataInput();
                    dataInput.originText = "Target Intel";
                    List<Effect> listOfEffects = action.GetEffects();
                    if (listOfEffects.Count > 0)
                    {
                        foreach (Effect effect in listOfEffects)
                        {
                            if (effect.ignoreEffect == false)
                            {
                                //process effect normally
                                EffectDataReturn effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput, actor);
                                if (effectReturn != null)
                                {
                                    builderTop.AppendLine();
                                    builderTop.Append(effectReturn.topText);
                                    if (builderBottom.Length > 0)
                                    { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                    builderBottom.Append(effectReturn.bottomText);
                                }
                                else { Debug.LogError("Invalid effectReturn (Null)"); }
                            }
                        }
                    }
                    //OUTCOME Window
                    ModalOutcomeDetails detailsModal = new ModalOutcomeDetails();
                    detailsModal.textTop = builderTop.ToString();
                    detailsModal.textBottom = builderBottom.ToString();
                    detailsModal.sprite = sprite;
                    detailsModal.side = GameManager.instance.globalScript.sideResistance;
                    detailsModal.isAction = true;
                    detailsModal.reason = "Gain Target Intel";
                    EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, detailsModal, "TargetManager.cs -> ProcessTargetInfo");
                }
                else { Debug.LogError(string.Format("Invalid node (Null) for NodeID {0}", detailsGeneric.nodeID)); }
            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for slotID {0}", detailsGeneric.actorSlotID); }
        }
        else { Debug.LogErrorFormat("Invalid target (Null) for target {0}", detailsGeneric.optionName); }
    }

    /// <summary>
    /// algorithim to give the amount of intel gained (shows new level) based on distance between node and target. Closer the better. 
    /// Note that this takes into account the MAX cap of targetManager.cs -> maxTargetInfo
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    private int GetTargetInfoNew(int distance, int existingInfo)
    {
        int intel = Mathf.Max(1, 3 - distance);
        intel += existingInfo;
        return Mathf.Min(maxTargetInfo, intel);
    }

    /// <summary>
    /// gives the amount of intel to be gained, solely based around distance, ignoring max cap
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    private int GetTargetInfoGain(int distance)
    { return Mathf.Max(1, 3 - distance); }

    //place methods above here
}
