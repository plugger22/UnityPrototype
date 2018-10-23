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
    [Range(0, 20)]
    [Tooltip("The % of the total Nodes on the level which commence with a Live target")]
    public int startPercentTargets = 10;
    [Range(20, 50)]
    [Tooltip("The % of the total Nodes on the level that can have a Live target at any one time")]
    public int maxPercentTargets = 25;
    [Range(0, 100)]
    [Tooltip("The base chance of a target attempt being successful with no other factors in play")]
    public int baseTargetChance = 50;
    [Range(1, 3)]
    [Tooltip("How much effect having the right Gear for a target will have on the chance of success")]
    public int gearEffect = 2;
    [Range(1, 3)]
    [Tooltip("How much effect having the right Actor for a target will have on the chance of success")]
    public int actorEffect = 2;
    [Range(1, 3)]
    [Tooltip("Maximum amount of target info that can be acquired on a specific target")]
    public int maxTargetInfo = 3;

    [HideInInspector] public int StartTargets;
    [HideInInspector] public int ActiveTargets;
    [HideInInspector] public int LiveTargets;
    [HideInInspector] public int MaxTargets;

    private List<TargetFactors> listOfFactors = new List<TargetFactors>();              //used to ensure target calculations are consistent across methods

    //fast access
    private GearType infiltrationGear;

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
        //calculate limits
        int numOfNodes = GameManager.instance.dataScript.CheckNumOfNodes();
        StartTargets = numOfNodes * startPercentTargets / 100;
        MaxTargets = numOfNodes * maxPercentTargets / 100;
        ActiveTargets = MaxTargets - StartTargets;
        ActiveTargets = Mathf.Max(0, ActiveTargets);

        /*Debug.LogFormat("TargetManager.cs -> Initialise: MaxTargets {0}, StartTargets {1}, ActiveTargets {2}, LiveTargets {3}{4}", MaxTargets, StartTargets, ActiveTargets, LiveTargets, "\n");
        //Set initialise targets on map
        SetRandomTargets(1, Status.Active);
        SetRandomTargets(StartTargets, Status.Live);*/

        //set up listOfTargetFactors. Note -> Sequence matters and is the order that the factors will be displayed
        foreach(var factor in Enum.GetValues(typeof(TargetFactors)))
        { listOfFactors.Add((TargetFactors)factor); }
        //fast access
        infiltrationGear = GameManager.instance.dataScript.GetGearType("Infiltration");
        Debug.Assert(infiltrationGear != null, "Invalid infiltrationGear (Null)");
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
        int targetID;
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        if (listOfNodes != null)
        {
            foreach(Node node in listOfNodes)
            {
                targetID = node.targetID;
                if (targetID > -1)
                {
                    //target present
                    if (node.isTargetKnown == false)
                    {
                        //probe team present -> target known
                        if (node.isProbeTeam == true)
                        { node.isTargetKnown = true; }
                        else
                        {
                            //get target status
                            Target target = GameManager.instance.dataScript.GetTarget(targetID);
                            if (target != null)
                            {
                                //target automatically known regardless if completed or contained
                                switch(target.targetStatus)
                                {
                                    case Status.Completed:
                                    case Status.Contained:
                                        node.isTargetKnown = true;
                                        break;
                                }
                            }
                            else { Debug.LogWarning(string.Format("Invalid target (Null) for targetID {0}", targetID)); }
                        }
                    }
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
        if (mission.cityHallTarget != null)
        {
            target = mission.cityHallTarget;
            nodeID = GameManager.instance.cityScript.cityHallDistrictID;
            Node node = GameManager.instance.dataScript.GetNode(nodeID);
            if (node != null)
            {
                SetTargetDetails(target, node, mission.cityHallProfile, mission.cityHallFollowOnTarget);
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignCityTarget: CityHall node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}, followON id {5}", node.nodeName, node.Arc.name, nodeID,
                    target.name, target.targetID, target.nextTargetID);
            }
            else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0} for iconTarget", nodeID); }
        }
        //icon
        if (mission.iconTarget != null)
        {
            target = mission.iconTarget;
            nodeID = GameManager.instance.cityScript.iconDistrictID;
            Node node = GameManager.instance.dataScript.GetNode(nodeID);
            if (node != null)
            {
                SetTargetDetails(target, node, mission.iconProfile, mission.iconFollowOnTarget);
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignCityTarget: Icon node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}, followON id {5}", node.nodeName, node.Arc.name, nodeID,
                    target.name, target.targetID, target.nextTargetID);
            }
            else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0} for iconTarget", nodeID); }
        }
        //airport
        if (mission.airportTarget != null)
        {
            target = mission.airportTarget;
            nodeID = GameManager.instance.cityScript.airportDistrictID;
            Node node = GameManager.instance.dataScript.GetNode(nodeID);
            if (node != null)
            {
                SetTargetDetails(target, node, mission.airportProfile, mission.airportFollowOnTarget);
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignCityTarget: Airport node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}, followON id {5}", node.nodeName, node.Arc.name, nodeID,
                    target.name, target.targetID, target.nextTargetID);
            }
            else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0} for airportTarget", nodeID); }
        }
        //harbour
        if (mission.harbourTarget != null)
        {
            target = mission.harbourTarget;
            nodeID = GameManager.instance.cityScript.harbourDistrictID;
            Node node = GameManager.instance.dataScript.GetNode(nodeID);
            if (node != null)
            {
                SetTargetDetails(target, node, mission.harbourProfile, mission.harbourFollowOnTarget);
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignCityTarget: Harbour node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}, followON id {5}", node.nodeName, node.Arc.name, nodeID,
                    target.name, target.targetID, target.nextTargetID);
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
                                //valid target
                                if (mission.genericLiveProfile != null)
                                {
                                    //assign target to node
                                    SetTargetDetails(target, node, mission.genericLiveProfile);
                                    Debug.LogFormat("[Tar] MissionManager.cs -> AssignGenericTarget LIVE: node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}", node.nodeName, node.Arc.name, node.nodeID,
                                        target.name, target.targetID);
                                    counter++;
                                    //delete target to prevent dupes
                                    if (GameManager.instance.dataScript.RemoveTargetFromGenericList(target.targetID, nodeArc.nodeArcID) == false)
                                    { Debug.LogErrorFormat("Target not removed from GenericList, target {0}, id {1}, nodeArc {2}", target.name, target.targetID, nodeArc.nodeArcID); }
                                }
                                else { Debug.LogError("Invalid mission.genericLiveProfile (Null)"); }
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
                                //valid target
                                if (mission.genericActiveProfile != null)
                                {
                                    //assign target to node
                                    SetTargetDetails(target, node, mission.genericActiveProfile);
                                    Debug.LogFormat("[Tar] MissionManager.cs -> AssignGenericTarget ACTIVE: node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}", node.nodeName, node.Arc.name, node.nodeID,
                                        target.name, target.targetID);
                                    counter++;
                                    //delete target to prevent dupes
                                    if (GameManager.instance.dataScript.RemoveTargetFromGenericList(target.targetID, nodeArc.nodeArcID) == false)
                                    { Debug.LogErrorFormat("Target not removed from GenericList, target {0}, id {1}, nodeArc {2}", target.name, target.targetID, nodeArc.nodeArcID); }
                                }
                                else { Debug.LogError("Invalid mission.genericActiveProfile (Null)"); }
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
            if (mission.vipTarget != null)
            {
                Target target = mission.vipTarget;
                //VIP targets don't have follow-on targets (use repeating random node targets instead)
                SetTargetDetails(target, node, mission.vipProfile);
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignVIPTarget: VIP node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}", node.nodeName, node.Arc.name, node.nodeID,
                    target.name, target.targetID);
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
            if (mission.harbourTarget != null)
            {
                Target target = mission.storyTarget;
                SetTargetDetails(target, node, mission.storyProfile);
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignStoryTarget: Story node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}", node.nodeName, node.Arc.name, node.nodeID,
                    target.name, target.targetID);
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
            if (mission.harbourTarget != null)
            {
                Target target = mission.goalTarget;
                SetTargetDetails(target, node, mission.goalProfile);
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignGoalTarget: Goal node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}", node.nodeName, node.Arc.name, node.nodeID,
                    target.name, target.targetID);
            }
        }
        else { Debug.LogWarning("Invalid node (Null) for GoalTarget"); }
    }

    /// <summary>
    /// Sets target activation, status and adds to relevant pools
    /// NOTE: Target and Node checked for null by calling methods
    /// </summary>
    /// <param name="target"></param>
    private void SetTargetDetails(Target target, Node node, TargetProfile profile, Target followOnTarget = null)
    {
        //only proceed to assign target if successfully added to list
        if (GameManager.instance.dataScript.AddNodeToTargetList(node.nodeID) == true)
        {
            //profile must be valid
            if (profile != null)
            {
                node.targetID = target.targetID;
                target.nodeID = node.nodeID;

                //activation
                switch (profile.trigger.name)
                {
                    case "Live":
                        //target commences live, ignore activation field
                        target.targetStatus = Status.Live;
                        target.isRepeat = false;
                        break;
                    case "Custom":
                        target.targetStatus = Status.Active;
                        target.isRepeat = false;
                        if (profile.activation != null)
                        { target.activation = profile.activation; }
                        else { Debug.LogErrorFormat("Invalid profile.activation (Null) for target {0}", target.name); }
                        break;
                    default:
                        Debug.LogErrorFormat("Invalid profile.Trigger \"{0}\" for target {1}", profile.trigger.name, target.name);
                        break;
                }
                //delay
                target.timerDelay = profile.delay;
                //repeat
                target.isRepeat = profile.isRepeat;
                target.isSameNode = profile.isSameNode;
                //window
                target.turnWindow = profile.turnWindow;
                //follow On target
                if (followOnTarget != null)
                { target.nextTargetID = followOnTarget.targetID; }
                else { target.nextTargetID = -1; }
                //add to pool
                GameManager.instance.dataScript.AddTargetToPool(target, target.targetStatus);

            }
            else { Debug.LogWarningFormat("Invalid profile (Null) for target {0}, targetID {1}", target.name, target.targetID); }
        }
        else { Debug.LogWarningFormat("Node {0}, {1}, id {2} NOT assigned target {3}", node.nodeName, node.Arc.name, node.nodeID, target.name); }
    }






    /*/// <summary>
    /// Populates a set number of randomly chosen level 1 targets into the level with the indicated status
    /// </summary>
    /// <param name="numOfTargets"></param>
    /// <param name="status"></param>
    public void SetRandomTargets(int numOfTargetsInput, Status status = Status.Active)
    {
        int index, nodeArcID, totalOfType, totalOfTypewithTargets, totalActive, totalLive, totalTargets, endlessCounter, numOfTargets;
        int counter = 0;
        bool doneFlag;
        //dictionary to hold list of already assigned nodeID's to prevent duplicate assignments
        Dictionary<int, List<int>> dictOfExisting = new Dictionary<int, List<int>>();
        //if not enough viable targets drop number down to what's doable
        int numPossibleTargets = GameManager.instance.dataScript.CheckNumOfPossibleTargets();
        if (numOfTargetsInput > numPossibleTargets) { numOfTargets = numPossibleTargets; }
        else { numOfTargets = numOfTargetsInput; }
        //loop and populate
        List<Target> listOfPossibleTargets = GameManager.instance.dataScript.GetPossibleTargets();
        for(int i = 0; i < numOfTargets; i++)
        {
            //successflag breaks out of while loop if a suitable node is found
            doneFlag = false;
            endlessCounter = 0;
            //keep chosing a node until you find a suitable one (max 5 goes)
            while (doneFlag == false)
            {
                //get a random target
                index = Random.Range(0, listOfPossibleTargets.Count);
                Target target = listOfPossibleTargets[index];
                //get target nodeArcId
                nodeArcID = target.nodeArc.nodeArcID;
                //check that there is a suitable spare node available
                totalOfType = GameManager.instance.dataScript.CheckNodeInfo(nodeArcID, NodeInfo.Number);
                totalOfTypewithTargets = GameManager.instance.dataScript.CheckNodeInfo(nodeArcID, NodeInfo.TargetsAll);
                if ((totalOfType - totalOfTypewithTargets) > 0)
                {
                    //get a random node
                    List<Node> tempNodes = new List<Node>(GameManager.instance.dataScript.GetListOfNodesByType(nodeArcID));
                    //clean out any nodes with prior targets prior to pruning
                    if (totalOfTypewithTargets > 0)
                    {
                        for(int j = tempNodes.Count - 1; j >= 0; j--)
                        {
                            if (tempNodes[j].targetID > -1)
                            { tempNodes.RemoveAt(j); }
                        }
                    }
                    if (tempNodes.Count > 0)
                    {
                        //remove all duplicate nodes from list that already have targets assigned to them
                        List<Node> prunedListOfNodes = PruneListOfNodes(nodeArcID, dictOfExisting, tempNodes);
                        if (prunedListOfNodes != null)
                        {
                            if (prunedListOfNodes.Count > 0)
                            {
                                Node node = prunedListOfNodes[Random.Range(0, prunedListOfNodes.Count)];
                                //assign targetID to node
                                node.targetID = target.targetID;
                                counter++;
                                Debug.Log(string.Format("TargetManager: Node ID {0}, type \"{1}\", assigned Target ID {2}, \"{3}\"{4}",
                                    node.nodeID, node.Arc.name, target.targetID, target.name, "\n"));
                                //reset target status
                                Target dictTarget = GameManager.instance.dataScript.GetTarget(target.targetID);
                                dictTarget.targetStatus = status;
                                //Remove from listOfPossibleTargets
                                listOfPossibleTargets.RemoveAt(index);

                                //add to dictionary (used to prevent targets being assigned to duplicate nodes)
                                List<int> listOfNodeID = new List<int>();
                                if (dictOfExisting.ContainsKey(nodeArcID) == true)
                                {
                                    listOfNodeID = dictOfExisting[nodeArcID];
                                    //add new entry
                                    listOfNodeID.Add(node.nodeID);
                                }
                                else
                                {
                                    //new entry in dictionary
                                    listOfNodeID.Add(node.nodeID);
                                    try
                                    { dictOfExisting.Add(nodeArcID, listOfNodeID); }
                                    catch (ArgumentException)
                                    { Debug.LogError(string.Format("Invalid nodeArcID {0} (duplicate entry) in dictOfExisting", nodeArcID)); }
                                }

                                //Update node Array info stats
                                totalTargets = GameManager.instance.dataScript.CheckNodeInfo(nodeArcID, NodeInfo.TargetsAll) + 1;
                                GameManager.instance.dataScript.SetNodeInfo(nodeArcID, NodeInfo.TargetsAll, totalTargets);
                                switch (status)
                                {
                                    case Status.Active:
                                        totalActive = GameManager.instance.dataScript.CheckNodeInfo(nodeArcID, NodeInfo.TargetsActive) + 1;
                                        GameManager.instance.dataScript.SetNodeInfo(nodeArcID, NodeInfo.TargetsActive, totalActive);
                                        GameManager.instance.dataScript.AddTargetToPool(target, Status.Active);
                                        //assign nodeID to target
                                        target.nodeID = node.nodeID;
                                        break;
                                    case Status.Live:
                                        totalLive = GameManager.instance.dataScript.CheckNodeInfo(nodeArcID, NodeInfo.TargetsLive) + 1;
                                        GameManager.instance.dataScript.SetNodeInfo(nodeArcID, NodeInfo.TargetsLive, totalLive);
                                        GameManager.instance.dataScript.AddTargetToPool(target, Status.Live);
                                        target.nodeID = node.nodeID;
                                        break;
                                    default:
                                        Debug.LogError(string.Format("Invalid status \"{0}\"{1}", status, "\n"));
                                        break;
                                }
                                doneFlag = true;
                            }
                            else
                            {
                                Debug.LogWarning(string.Format("No available nodes left after pruning for nodeArcID {0}{1}", nodeArcID, "\n"));
                                doneFlag = true;
                            }
                        }
                        else
                        {
                            endlessCounter++;
                            if (endlessCounter > 5)
                            {
                                Debug.LogWarning(string.Format("TargetManager: Breaking out of loop after 5 iterations, for type \"{0}\"{1}", target.nodeArc.name, "\n"));
                                doneFlag = true;
                            }
                            Debug.LogError(string.Format("No nodes available of type {0}. Unable to assign target{1}", target.nodeArc.name, "\n"));
                        }
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("TargetManager: Insufficient nodes of type \"{0}\" (zero or less){1}", target.nodeArc.name, "\n"));
                        doneFlag = true;
                    }
                }
                else
                {
                    Debug.LogWarning(string.Format("TargetManager: There are no \"{0}\" nodes without targets remaining{1}", target.nodeArc.name,"\n"));
                    doneFlag = true;
                }
            }
        }
        //check tally
        if (counter == 0)
        { Debug.LogWarning(string.Format("No nodes were assigned Targets{0}", "\n")); }
        else if (counter < StartTargets)
        { Debug.LogWarning(string.Format("TargetManager: Less than the required number of starting nodes assigned targets{0}", "\n")); }
    }


    /// <summary>
    /// private sub method to take list of Nodes and remove any that match entries in the dictOfExisting to prevent SetRandomTargets assigning targets to duplicate nodes
    /// </summary>
    /// <param name="dictOfExisting"></param>
    /// <param name="tempList"></param>
    /// <returns></returns>
    private List<Node> PruneListOfNodes(int nodeArcID, Dictionary<int, List<int>> dictOfExisting, List<Node> tempList)
    {
        List<Node> listOfNodes = null;
        if (tempList != null && dictOfExisting != null)
        {
            listOfNodes = new List<Node>(tempList);
            
            //lookup dictionary to see if any entries for identical nodeArcID
            if (dictOfExisting.ContainsKey(nodeArcID))
            {
                List<int> listOfExistingNodeID = new List<int>(dictOfExisting[nodeArcID]);
                //reverse loop parameter node List and delete any that match entries in listOfNodeIDs
                if (listOfExistingNodeID != null && listOfExistingNodeID.Count > 0)
                {
                    int nodeID;
                    bool isMatch;
                    for(int i = listOfNodes.Count - 1; i >= 0; i--)
                    {
                        isMatch = false;
                        nodeID = listOfNodes[i].nodeID;
                        //check for a match with existing
                        for(int j = 0; j < listOfExistingNodeID.Count; j++)
                        {
                            if (nodeID == listOfExistingNodeID[j])
                            {
                                isMatch = true;
                                break;
                            }
                        }
                        if (isMatch == true)
                        {
                            //delete entry
                            listOfNodes.RemoveAt(i);
                        }
                    }
                }
            }
            return listOfNodes;
        }
        else { Debug.LogError("Invalid dictOfExisting or tempList (either are Null)"); }
        return listOfNodes;
    }*/


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
                                        tempList.Add(string.Format("{0}<size=110%>{1}</size>{2}", colourTarget, target.name, colourEnd));
                                        tempList.Add(string.Format("Level {0}", target.targetLevel));
                                        break;
                                }
                            }
                            else
                            {
                                switch (target.targetStatus)
                                {
                                    case Status.Completed:
                                    case Status.Contained:
                                        tempList.Add(string.Format("<b>{0} Target</b>", target.targetStatus));
                                        tempList.Add(string.Format("{0}<size=110%>{1}</size>{2}", colourTarget, target.name, colourEnd));
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
                                case Status.Completed:
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
                                case Status.Completed:
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
                tempList.Add(string.Format("{0}<size=110%><b>{1}</b></size>{2}", colourTarget, target.name, colourEnd));
                tempList.Add(string.Format("{0}<b>Level {1}</b>{2}", colourDefault, target.targetLevel, colourEnd));
                break;
            case Status.Live:
                tempList.Add(string.Format("{0}<size=110%><b>{1}</b></size>{2}", colourTarget, target.name, colourEnd));
                tempList.Add(string.Format("{0}{1}{2}", colourDefault, target.description, colourEnd));
                /*//good effects
                Effect effect = null;
                for (int i = 0; i < target.listOfGoodEffects.Count; i++)
                {
                    effect = target.listOfGoodEffects[i];
                    if (effect != null)
                    { tempList.Add(string.Format("{0}{1}{2}", colourGood, effect.description, colourEnd)); }
                    else { Debug.LogError(string.Format("Invalid effect (null) for \"{0}\", ID {1}{2}", target.name, target.targetID, "\n")); }
                }
                //bad effects
                for (int i = 0; i < target.listOfBadEffects.Count; i++)
                {
                    effect = target.listOfBadEffects[i];
                    if (effect != null)
                    { tempList.Add(string.Format("{0}{1}{2}", colourBad, effect.description, colourEnd)); }
                    else { Debug.LogError(string.Format("Invalid effect (null) for \"{0}\", ID {1}{2}", target.name, target.targetID, "\n")); }
                }
                //ongoing effects
                if (target.OngoingEffect != null)
                { tempList.Add(string.Format("{0}{1}{2}", colourGood, target.OngoingEffect.description, colourEnd)); }*/
                //info level data colour graded
                tempList.Add(string.Format("{0}Info level{1}  {2}{3}{4}", colourDefault, colourEnd,
                    GameManager.instance.colourScript.GetValueColour(target.infoLevel), target.infoLevel, colourEnd));
                tempList.Add(string.Format("{0}<b>{1} gear</b>{2}", colourGear, target.gear.name, colourEnd));
                tempList.Add(string.Format("{0}<b>{1}</b>{2}", colourGear, target.actorArc.name, colourEnd));
                break;
            case Status.Completed:
                //put tooltip together
                tempList.Add(string.Format("{0}Target \"{1}\" has been Completed{2}", colourTarget, target.name, colourEnd));
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
                    else { Debug.LogError(string.Format("Invalid Good effect (null) for \"{0}\", ID {1}{2}", target.name, target.targetID, "\n")); }
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
                    else { Debug.LogError(string.Format("Invalid Bad effect (null) for \"{0}\", ID {1}{2}", target.name, target.targetID, "\n")); }
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
                                    int slotID = GameManager.instance.dataScript.CheckActorPresent(target.actorArc.ActorArcID, GameManager.instance.globalScript.sideResistance);
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
                                    int slotID = GameManager.instance.dataScript.CheckActorPresent(target.actorArc.ActorArcID, GameManager.instance.globalScript.sideResistance);
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
    /// Contains a completed target as a result of Damage team intervention. Handles all related matters.
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
            target.targetStatus = Status.Contained;
            GameManager.instance.dataScript.AddTargetToPool(target, Status.Contained);
            node.targetID = -1;
        }
        else { Debug.LogError(string.Format("Invalid node (Null) for target.nodeID {0}", target.nodeID)); }
    }


    //place methods above here
}
