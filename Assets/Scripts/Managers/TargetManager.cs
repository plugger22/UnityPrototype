using gameAPI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public enum TargetFactors { TargetInfo, NodeSupport, ActorAndGear, NodeSecurity, TargetLevel, Teams} //Sequence is order of factor display

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

    [HideInInspector] public int StartTargets;
    [HideInInspector] public int ActiveTargets;
    [HideInInspector] public int LiveTargets;
    [HideInInspector] public int MaxTargets;

    private List<TargetFactors> listOfFactors = new List<TargetFactors>();              //used to ensure target calculations are consistent across methods

    //fast access
    private GearType infiltrationGear;
    private GlobalSide globalResistance;
    private GlobalSide globalAuthority;

    //colour Palette
    private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourGear;
    private string colourNormal;
    private string colourDefault;
    private string colourGrey;
    //private string colourRebel;
    private string colourTarget;
    private string colourEnd;

    /// <summary>
    /// Initial setup
    /// </summary>
    public void Initialise()
    {
        Debug.Assert(activateLowChance < activateMedChance, "invalid activateLowChance (should be less than MED)");
        Debug.Assert(activateMedChance < activateHighChance, "invalid activateMedChance (should be less than HIGH)");
        Debug.Assert(activateHighChance < activateExtremeChance, "invalid activateHighChance (should be less than EXTREME)");
        Debug.Assert(activateLowLimit > activateMedLimit, "invalid activateLowLimit (should be more than MED)");
        Debug.Assert(activateMedLimit > activateHighLimit, "invalid activateMedLimit (should be more than HIGH)");
        Debug.Assert(activateHighLimit > activateExtremeLimit, "invalid activateHighLimit (should be more than EXTREME)");

        /*//calculate limits
        int numOfNodes = GameManager.instance.dataScript.CheckNumOfNodes();
        StartTargets = numOfNodes * startPercentTargets / 100;
        MaxTargets = numOfNodes * maxPercentTargets / 100;
        ActiveTargets = MaxTargets - StartTargets;
        ActiveTargets = Mathf.Max(0, ActiveTargets);*/
        /*Debug.LogFormat("TargetManager.cs -> Initialise: MaxTargets {0}, StartTargets {1}, ActiveTargets {2}, LiveTargets {3}{4}", MaxTargets, StartTargets, ActiveTargets, LiveTargets, "\n");
        //Set initialise targets on map
        SetRandomTargets(1, Status.Active);
        SetRandomTargets(StartTargets, Status.Live);*/

        //set up listOfTargetFactors. Note -> Sequence matters and is the order that the factors will be displayed
        foreach (var factor in Enum.GetValues(typeof(TargetFactors)))
        { listOfFactors.Add((TargetFactors)factor); }
        //fast access
        infiltrationGear = GameManager.instance.dataScript.GetGearType("Infiltration");
        globalResistance = GameManager.instance.globalScript.sideResistance;
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        Debug.Assert(infiltrationGear != null, "Invalid infiltrationGear (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        //event listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "TargetManager");
        EventManager.instance.AddListener(EventType.StartTurnLate, OnEvent, "TargetManager");
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
            case EventType.StartTurnLate:
                StartTurnLate();
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
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourGear = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        //colourRebel = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourTarget = GameManager.instance.colourScript.GetColour(ColourType.actorArc);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// loops nodes, checks all targets and updates isTargetKnown status
    /// </summary>
    private void StartTurnLate()
    {
        CheckTargets();
    }

    /// <summary>
    /// checks all targets on map and handles admin and status changes
    /// </summary>
    private void CheckTargets()
    {
        int targetID, rndNum;
        bool isLive;
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        if (listOfNodes != null)
        {
            //loop nodes
            foreach (Node node in listOfNodes)
            {
                targetID = node.targetID;
                //Target present
                if (targetID > -1)
                {
                    Target target = GameManager.instance.dataScript.GetTarget(targetID);
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
                            case Status.Active:
                                if (target.timerDelay == 0)
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
                                            if (rndNum < activateMedChance) { isLive = true; /*Debug.LogFormat("[Tst] Med Roll {0} < {1}", rndNum, activateMedChance);*/}
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
                                        string text = string.Format("New target {0}, id {1} at {2}, {3}, id {4}", target.targetName, target.targetID, node.nodeName, node.Arc.name, node.nodeID);
                                        GameManager.instance.messageScript.TargetNew(text, node, target);
                                        Debug.LogFormat("[Tar] TargetManager.cs -> CheckTargets: Target {0}, id {1} goes LIVE", target.targetName, target.targetID);
                                    }
                                }
                                else
                                { target.timerDelay--; }
                                break;
                            case Status.Live:
                                if (target.timerWindow == 0)
                                {
                                    Debug.LogFormat("[Tar] TargetManager.cs -> CheckTargets: Target {0}, id {1} Expired", target.targetName, target.targetID);
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
                        }
                    }
                    else { Debug.LogWarning(string.Format("Invalid target (Null) for targetID {0}", targetID)); }
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
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignCityTarget: CityHall node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}", node.nodeName, node.Arc.name, nodeID,
                    target.targetName, target.targetID);
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
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignCityTarget: Icon node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}", node.nodeName, node.Arc.name, nodeID,
                    target.targetName, target.targetID);
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
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignCityTarget: Airport node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}", node.nodeName, node.Arc.name, nodeID,
                    target.targetName, target.targetID);
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
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignCityTarget: Harbour node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}", node.nodeName, node.Arc.name, nodeID,
                    target.targetName, target.targetID);
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
                                if (node.targetID == -1)
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
                                Debug.LogFormat("[Tar] MissionManager.cs -> AssignGenericTarget LIVE: node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}", node.nodeName, node.Arc.name, node.nodeID,
                                    target.targetName, target.targetID);
                                counter++;
                                //delete target to prevent dupes
                                if (GameManager.instance.dataScript.RemoveTargetFromGenericList(target.targetID, nodeArc.nodeArcID) == false)
                                { Debug.LogErrorFormat("Target not removed from GenericList, target {0}, id {1}, nodeArc {2}", target.targetName, target.targetID, nodeArc.nodeArcID); }
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
                                if (node.targetID == -1)
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
                                Debug.LogFormat("[Tar] MissionManager.cs -> AssignGenericTarget ACTIVE: node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}", node.nodeName, node.Arc.name, node.nodeID,
                                    target.targetName, target.targetID);
                                counter++;
                                //delete target to prevent dupes
                                if (GameManager.instance.dataScript.RemoveTargetFromGenericList(target.targetID, nodeArc.nodeArcID) == false)
                                { Debug.LogErrorFormat("Target not removed from GenericList, target {0}, id {1}, nodeArc {2}", target.targetName, target.targetID, nodeArc.nodeArcID); }
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
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignVIPTarget: VIP node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}", node.nodeName, node.Arc.name, node.nodeID,
                    target.targetName, target.targetID);
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
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignStoryTarget: Story node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}", node.nodeName, node.Arc.name, node.nodeID,
                    target.targetName, target.targetID);
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
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignGoalTarget: Goal node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}", node.nodeName, node.Arc.name, node.nodeID,
                    target.targetName, target.targetID);
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
        if (node.targetID == -1)
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
                    else if (target.targetType.name.Equals("Generic") == true)
                    { target.profile = defaultProfile; }
                    //profile must be valid
                    if (target.profile != null)
                    {
                        //ID's
                        node.targetID = target.targetID;
                        target.nodeID = node.nodeID;
                        //timers
                        target.timerDelay = target.profile.delay;
                        target.timerHardLimit = 0;
                        target.timerWindow = target.profile.window;
                        target.turnsWindow = target.profile.window;
                        //defaults (need to set as Target SO could be carrying over data from a previous level)
                        target.isKnownByAI = false;
                        target.turnSuccess = -1;
                        target.turnDone = -1;
                        target.numOfAttempts = 0;
                        target.ongoingID = -1;
                        target.infoLevel = 0;
                        //status and message
                        switch (target.profile.trigger.name)
                        {
                            case "Live":
                                target.targetStatus = Status.Live;
                                string text = string.Format("New target {0}, id {1} at {2}, {3}, id {4}", target.targetName, target.targetID, node.nodeName, node.Arc.name, node.nodeID);
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
                    else { Debug.LogWarningFormat("Invalid profile (Null) for target {0}, targetID {1}", target.targetName, target.targetID); isSuccess = false; }
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
    public List<string> GetTargetTooltip(int targetID, bool isTargetKnown)
    {
        List<string> tempList = new List<string>();
        if (targetID > -1)
        {
            //find target
            Target target = GameManager.instance.dataScript.GetTarget(targetID);
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
            { Debug.LogError(string.Format("Invalid Target (null) for ID {0}{1}", targetID, "\n")); }
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
                    tempList.Add(string.Format("{0}Info level{1}  {2}<b>{3}</b>{4}", colourDefault, colourEnd,
                        GameManager.instance.colourScript.GetValueColour(target.infoLevel), target.infoLevel, colourEnd));
                    tempList.Add(string.Format("{0}<b>{1} gear</b>{2}", colourGear, target.gear.name, colourEnd));
                    tempList.Add(string.Format("{0}<b>{1}</b>{2}", colourGear, target.actorArc.name, colourEnd));
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
                if (target.OngoingEffect != null)
                {
                    tempList.Add(string.Format("{0}Ongoing effects until contained{1}", colourDefault, colourEnd));
                    tempList.Add(string.Format("{0}{1}{2}", colourGood, target.OngoingEffect.description, colourEnd));
                }
                break;
        }
        return tempList;
    }

    /// <summary>
    /// returns formatted string of all good and bad target effects. Returns null if none.
    /// </summary>
    /// <param name="targetID"></param>
    /// <returns></returns>
    public string GetTargetEffects(int targetID)
    {
        List<string> tempList = new List<string>();
        //find target
        Target target = GameManager.instance.dataScript.GetTarget(targetID);
        if (target != null)
        {
            //good effects
            Effect effect = null;
            tempList.Add("Target Effects");
            if (target.listOfGoodEffects.Count > 0)
            {
                //add header
                for (int i = 0; i < target.listOfGoodEffects.Count; i++)
                {
                    effect = target.listOfGoodEffects[i];
                    if (effect != null)
                    { tempList.Add(string.Format("{0}{1}{2}", colourGood, effect.description, colourEnd)); }
                    else { Debug.LogError(string.Format("Invalid Good effect (null) for \"{0}\", ID {1}{2}", target.targetName, target.targetID, "\n")); }
                }
            }
            //bad effects
            if (target.listOfBadEffects.Count > 0)
            {
                //add header
                for (int i = 0; i < target.listOfBadEffects.Count; i++)
                {
                    effect = target.listOfBadEffects[i];
                    if (effect != null)
                    { tempList.Add(string.Format("{0}{1}{2}", colourBad, effect.description, colourEnd)); }
                    else { Debug.LogError(string.Format("Invalid Bad effect (null) for \"{0}\", ID {1}{2}", target.targetName, target.targetID, "\n")); }
                }
            }
            //Ongoing effects -> add header
            if (target.OngoingEffect != null)
            { tempList.Add(string.Format("{0}{1} (Ongoing){2}", colourGood, effect.description, colourEnd)); }
        }
        else
        {
            Debug.LogError(string.Format("Invalid Target (null) for ID {0}{1}", targetID, "\n"));
            return null;
        }
        //convert to a string
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
    /// Returns all factors involved in a particular targets resolution (eg. effects chance of success). 
    /// Used by ActorManager.cs -> GetNodeActions for action button tooltip
    /// NOTE: Tweak listOfFactors in Initialise() if you want to change any factors in the calculations
    /// </summary>
    /// <param name="targetID"></param>
    /// <returns></returns>
    public string GetTargetFactors(int targetID)
    {
        List<string> tempList = new List<string>();
        //get target
        Target target = GameManager.instance.dataScript.GetTarget(targetID);
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
                tempList.Add(string.Format("{0}<size=95%> Base Chance +{1}</size>{2}", colourNeutral, baseTargetChance * 0.1, colourEnd));
                //Loop listOfFactors to ensure consistency of calculations across methods
                foreach (TargetFactors factor in listOfFactors)
                {
                    switch (factor)
                    {
                        case TargetFactors.TargetInfo:
                            //good -> info
                            tempList.Add(string.Format("{0}<size=95%>Info {1}{2}</size>{3}", colourGood, target.infoLevel > 0 ? "+" : "", target.infoLevel, colourEnd));
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
                                    int gearID = GameManager.instance.playerScript.CheckGearTypePresent(target.gear);
                                    if (gearID > -1)
                                    {
                                        Gear gear = GameManager.instance.dataScript.GetGear(gearID);
                                        if (gear != null)
                                        { tempList.Add(string.Format("{0}<size=95%>{1} +{2}</size>{3}", colourGood, gear.name, gearEffect * (gear.rarity.level + 1), colourEnd)); }
                                        else { Debug.LogWarning(string.Format("Invalid Target gear (Null) for gearID {0}", gearID)); }
                                    }
                                    else
                                    { tempList.Add(string.Format("{0}<size=95%>{1} gear</size>{2}", colourGrey, target.gear.name, colourEnd)); }
                                    //infiltration gear works on any target in addition to special gear
                                    if (target.gear.name.Equals(infiltrationGear.name) == false)
                                    {
                                        //check player has infiltration gear
                                        gearID = GameManager.instance.playerScript.CheckGearTypePresent(infiltrationGear);
                                        if (gearID > -1)
                                        {
                                            Gear gear = GameManager.instance.dataScript.GetGear(gearID);
                                            if (gear != null)
                                            { tempList.Add(string.Format("{0}<size=95%>{1} +{2}</size>{3}", colourGood, gear.name, gearEffect * (gear.rarity.level + 1), colourEnd)); }
                                            else { Debug.LogWarning(string.Format("Invalid Infiltration gear (Null) for gearID {0}", gearID)); }
                                        }
                                        else
                                        { tempList.Add(string.Format("{0}<size=95%>{1} gear</size>{2}", colourGrey, infiltrationGear.name, colourEnd)); }
                                    }
                                }
                            }
                            else
                            {
                                //player not at node ->  check if node active for the correct actor
                                if (target.actorArc != null)
                                {
                                    //check if actor present in team
                                    int slotID = GameManager.instance.dataScript.CheckActorPresent(target.actorArc.ActorArcID, globalResistance);
                                    if (slotID > -1)
                                    {
                                        //actor present and available
                                        tempList.Add(string.Format("{0}<size=95%>{1} +{2}</size>{3}", colourGood, target.actorArc.name, actorEffect, colourEnd));
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
                                    tempList.Add(string.Format("{0}<size=95%>{1} gear</size>{2}", colourGrey, target.gear.name, colourEnd));
                                    tempList.Add(string.Format("{0}<size=95%>{1} gear</size>{2}", colourGrey, infiltrationGear.name, colourEnd));
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
                int tally = GetTargetTally(targetID);
                int chance = GetTargetChance(tally);
                //add tally and chance to string
                tempList.Add(string.Format("{0}<size=95%>Total {1}{2}</size>{3}", colourNeutral, tally > 0 ? "+" : "", tally, colourEnd));
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
            Debug.LogError(string.Format("Invalid Target (null), ID \"{0}\"{1}", targetID, "\n"));
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
    public int GetTargetTally(int targetID, bool setGearAsUsed = false)
    {
        int tally = 0;
        //base chance
        tally += (int)(baseTargetChance * 0.1);
        //get target
        Target target = GameManager.instance.dataScript.GetTarget(targetID);
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
                        case TargetFactors.TargetInfo:
                            //good -> info
                            tally += target.infoLevel;
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
                                    int gearID = GameManager.instance.playerScript.CheckGearTypePresent(target.gear);
                                    if (gearID > -1)
                                    {
                                        Gear gear = GameManager.instance.dataScript.GetGear(gearID);
                                        if (gear != null)
                                        {
                                            tally += gearEffect * (gear.rarity.level + 1);
                                            //gear used?
                                            if (setGearAsUsed == true)
                                            { GameManager.instance.gearScript.SetGearUsed(gear, "attempt Target"); }
                                        }
                                        else { Debug.LogWarningFormat("Invalid Target gear (Null) for gearID {0}", gearID); }
                                    }
                                    //infiltration gear works on any target in addition to special gear
                                    if (target.gear.name.Equals(infiltrationGear.name) == false)
                                    {
                                        //check player has infiltration gear
                                        gearID = GameManager.instance.playerScript.CheckGearTypePresent(infiltrationGear);
                                        if (gearID > -1)
                                        {
                                            Gear gear = GameManager.instance.dataScript.GetGear(gearID);
                                            if (gear != null)
                                            {
                                                tally += gearEffect * (gear.rarity.level + 1);
                                                //gear used?
                                                if (setGearAsUsed == true)
                                                { GameManager.instance.gearScript.SetGearUsed(gear, "attempt Target"); }
                                            }
                                            else { Debug.LogWarningFormat("Invalid Infiltration gear (Null) for gearID {0}", gearID); }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //player NOT at node ->  check if actor is present in OnMap line-up
                                if (target.actorArc != null)
                                {
                                    int slotID = GameManager.instance.dataScript.CheckActorPresent(target.actorArc.ActorArcID, globalResistance);
                                    if (slotID > -1)
                                    {
                                        //actor present and available
                                        tally += actorEffect;
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
        { Debug.LogError(string.Format("Invalid Target (null), ID \"{0}\"{1}", targetID, "\n")); }
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
    /// Called whenever a target is done (finished OnMap, eg. Contained or Completed with no ongoing effects or timed out (window). Returns true if all O.K
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
                    node.targetID = -1;
                    target.nodeID = -1;
                    //
                    // - - - Follow On target
                    //
                    if (target.followOnTarget != null)
                    {
                        //Success required to generate a followOn target?
                        if (target.isSuccessNeeded == true && target.turnSuccess == -1)
                        { Debug.LogFormat("[Tst] TargetManager.cs -> SetTargetDone: FollowOn for target \"{0}\" Invalid as target not successfully completed", target.targetName); }
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
                                                Debug.LogFormat("[Tar] TargetManager.cs -> SetTargetDone: Node (same) \"{0}\", {1}, id {2}, assigned follow On GENERIC target \"{3}\", id {4}", node.nodeName,
                                                    node.Arc.name, node.nodeID, target.followOnTarget.targetName, target.followOnTarget.targetID);
                                            }
                                        }
                                        else
                                        {
                                            if (SetTargetDetails(target.followOnTarget, node) == true)
                                            {
                                                Debug.LogFormat("[Tar] TargetManager.cs -> SetTargetDone: Node (same) \"{0}\", {1}, id {2}, assigned follow On GENERIC target \"{3}\", id {4}", node.nodeName,
                                                    node.Arc.name, node.nodeID, target.followOnTarget.targetName, target.followOnTarget.targetID);
                                            }
                                        }
                                    }
                                    else { Debug.LogError("Invalid mission (Null)"); }
                                    break;
                                case "City":
                                    //City must follow On to the same node
                                    if (SetTargetDetails(target.followOnTarget, node) == true)
                                    {
                                        Debug.LogFormat("[Tar] TargetManager.cs -> SetTargetDone: Node (same) \"{0}\", {1}, id {2}, assigned follow On City target \"{3}\", id {4}", node.nodeName, node.Arc.name,
                                          node.nodeID, target.followOnTarget.targetName, target.followOnTarget.targetID);
                                    }
                                    break;
                                default:
                                    //Story / VIP / Goal Follow On targets -> random node
                                    Node nodeRandom = GameManager.instance.dataScript.GetRandomTargetNode();
                                    if (nodeRandom != null)
                                    {
                                        if (SetTargetDetails(target.followOnTarget, nodeRandom) == true)
                                        {
                                            Debug.LogFormat("[Tar] TargetManager.cs -> SetTargetDone: Node (random) \"{0}\", {1}, id {2}, assigned follow On target \"{3}\", id {4}", nodeRandom.nodeName,
                                                nodeRandom.Arc.name, nodeRandom.nodeID, target.followOnTarget.targetName, target.followOnTarget.targetID);
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
                            if (target.targetType.name.Equals("Generic") == false)
                            {
                                isDone = false;
                                //assign repeat profile if present
                                if (target.profile.repeatProfile != null)
                                { target.profile = target.profile.repeatProfile; }
                                else { Debug.LogWarningFormat("TargetManager.cs -> SetTargetDone: target {0}, id {1} repeatProfile Invalid (Null)", target.targetName, target.targetID); }
                                //same node
                                if (target.profile.isRepeatSameNode == true)
                                {
                                    if (SetTargetDetails(target, node) == true)
                                    {
                                        Debug.LogFormat("[Tar] TargetManager.cs -> SetTargetDone: Node (same) \"{0}\", {1}, id {2}, assigned REPEAT target \"{3}\", id {4}", node.nodeName, node.Arc.name,
                                          node.nodeID, target.targetName, target.targetID);
                                    }
                                }
                                //random node
                                else
                                {
                                    //City targets can't have random repeats
                                    if (target.targetType.name.Equals("City") == false)
                                    {
                                        Node nodeRandom = GameManager.instance.dataScript.GetRandomTargetNode();
                                        if (nodeRandom != null)
                                        {
                                            if (SetTargetDetails(target, nodeRandom) == true)
                                            {
                                                Debug.LogFormat("[Tar] TargetManager.cs -> SetTargetDone: Node (random) \"{0}\", {1}, id {2}, assigned REPEAT target \"{3}\", id {4}", nodeRandom.nodeName,
                                                    nodeRandom.Arc.name, nodeRandom.nodeID, target.targetName, target.targetID);
                                            }
                                        }
                                        else { Debug.LogError("Invalid nodeRandom (Null), Target not assigned"); }
                                    }
                                    else
                                    {
                                        Debug.LogWarningFormat("CITY target {0}, id {1}, can't have a Repeating RANDOM node (must be Same)", target.targetName, target.targetID);
                                        //set target as done, put in pool, etc. (below)
                                        isDone = true;
                                    }
                                }
                            }
                            else { Debug.LogWarningFormat("TargetManager.cs -> SetTargetDone: target {0}, id {1} can't REPEAT as GENERIC", target.targetName, target.targetID); }
                        }
                        else { Debug.LogFormat("[Tar] TargetManager.cs -> SetTargetDone: target {0}, id {1} can't REPEAT as successfully attempted", target.targetName, target.targetID); }
                    }
                    else
                    {
                        //no follow on, no repeat, successfully handled done admin
                        Debug.LogFormat("[Tar] TargetManager.cs -> SetTargetDone: Target \"{0}\", id {1}, DONE admin completed", target.targetName, target.targetID);
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
        { Debug.LogWarningFormat("TargetManager.cs -> SetTargetDone: Target \"{0}\", id {1}, DONE admin FAILED", target.targetName, target.targetID); }
        return isSuccess;
    }


    //place methods above here
}
