using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using System.Text;
using System;
using System.Linq;
using Random = UnityEngine.Random;

/// <summary>
/// AI data package used to collate all node info where a node has degraded in some form
/// </summary>
public class AINodeData
{
    public int nodeID;
    public int score;                       //used by spider & others to give the node a rating. Higher the score, the more likely it is to be chosen
    public NodeData type;
    public NodeArc arc;
    public int difference;                  //shows difference between current and start values
    public int current;                     //shows current value
    public bool isPreferred;                //true if preferred type of node for that side's faction
}

/// <summary>
/// AI data package detailing an Authority task that is ready to be executed next turn
/// </summary>
public class AITask
{
    public int data0;                      //could be node or connection ID
    public int data1;                      //teamArcID
    public string name0;                   //node arc name
    public string name1;                   //could be team arc name, eg. 'CIVIL'
    public Priority priority;
    public AIType type;                     //what type of task
    public int chance;                      //dynamically added by ProcessTasksFinal (for display to player of % chance of this task being chosen)
}

/// <summary>
/// extracted data from AI messages (at time of AI becoming aware of them)
/// </summary>
public class AITracker
{
    public int data0;                       //node or connectionID
    public int turn;                        //turn occurred

    public AITracker(int data, int turn)
    { data0 = data; this.turn = turn; }
}


/// <summary>
/// Handles AI management of both sides
/// </summary>
public class AIManager : MonoBehaviour
{
    [Tooltip("The % of the total map, from the centre outwards, that encompasses the geographic centre where any node in the area is node.isCentreNode true")]
    [Range(0, 100)] public float nodeGeographicCentre = 30f;
    [Tooltip("How many turns, after the event, that the AI will track Connection & Node activity before ignoring it")]
    [Range(5, 15)] public int activityTimeLimit = 10;
    [Tooltip("How much renown it will cost to access the AI's decision making process for Level 1 (potential tasks & % chances). Double this for level 2 (final tasks)")]
    [Range(0, 10)] public int playerAIRenownCost = 1;
    [Tooltip("When selecting Non-Critical tasks where there are an excess to available choices how much relative weight do I assign to High Priority tasks")]
    [Range(0, 10)] public int priorityHighWeight = 3;
    [Tooltip("When selecting Non-Critical tasks where there are an excess to available choices how much relative weight do I assign to Medium Priority tasks")]
    [Range(0, 10)] public int priorityMediumWeight = 2;
    [Tooltip("When selecting Non-Critical tasks where there are an excess to available choices how much relative weight do I assign to Low Priority tasks")]
    [Range(0, 10)] public int priorityLowWeight = 1;
    [Tooltip("How many of the most recent AI activities are tracked (keeps this number of most recent in a priorityQueue)")]
    [Range(0, 10)] public int numOfActivitiesTracked = 5;
    [Tooltip("How many turns ago (activity wise) will the AI use to select a target node for Erasure team AI processing")]
    [Range(0, 5)] public int trackerNumOfTurnsAgo = 2;
    [Tooltip("The listOfMostConnectedNodes includes all nodes with this number, or greater, connections. If < 3 nodes then the next set down are added. Max capped at total nodes/2")]
    [Range(0, 5)] public int nodeConnectionThreshold = 3;
    [Tooltip("Spider/Erasure Team node score -> Each resistance activity point is multiplied by this factor")]
    [Range(0, 5)] public int nodeActivityCountFactor = 1;
    [Tooltip("Spider/Erasure Team node score -> Factor - (Current turn - Activity last Turn)")]
    [Range(0, 5)] public int nodeActivityTimeFactor = 4;
    [Tooltip("Spider/Erasure Team node score -> added to score if node is a chokepoint ('isChokepointNode' true)")]
    [Range(0, 5)] public int nodeChokepointFactor = 5;
    [Tooltip("Erasure Team node score -> added if an uncompleted target, subtracted if a completed target")]
    [Range(0, 5)] public int nodeTargetFactor = 3;
    [Tooltip("Erasure Team node score -> Added to score if a spider is present and Not know. If known then effect is halved")]
    [Range(0, 5)] public int nodeSpiderFactor = 5;
    [Tooltip("Spider/Erasure Team node score -> If node is preferred type of current level authority faction this is added to the score")]
    [Range(0, 5)] public int nodePreferredFactor = 3;
    [Tooltip("Erasure Team node score -> added to score if node is in list of most connected nodes")]
    [Range(0, 5)] public int nodeConnectedFactor = 3;
    [Tooltip("Spider Team node score -> If node is in the designated central region of the map (see 'nodeGeographicCentre') this is added to the score")]
    [Range(0, 5)] public int nodeCentreFactor = 3;
    [Tooltip("Spider Team node score -> This is added to the score for ever 'None' security connection the node has")]
    [Range(0, 5)] public int securityNoneFactor = 3;
    [Tooltip("Spider Team node score -> This is added to the score for ever Low security connection the node has")]
    [Range(0, 5)] public int securityLowFactor = 2;
    [Tooltip("Spider Team node score -> This is added to the score for ever Medium security connection the node has")]
    [Range(0, 5)] public int securityMediumFactor = 1;
    [Tooltip("Spider Team node score -> This is added to the score for ever High security connection the node has")]
    [Range(0, 5)] public int securityHighFactor = 0;
    [Tooltip("Pool of tasks for Spider team -> number of entries for a Known target")]
    [Range(0, 5)] public int teamPoolTargetFactor = 3;
    [Tooltip("Pool of tasks for Spider/Erasure team -> number of entries for the top scored node")]
    [Range(0, 5)] public int teamPoolFirstFactor = 3;
    [Tooltip("Pool of tasks for Spider/Erasure team -> number of entries for the second top scored node")]
    [Range(0, 5)] public int teamPoolSecondFactor = 2;
    [Tooltip("Pool of tasks for Spider/Erasure team -> number of entries for the third top scored node")]
    [Range(0, 5)] public int teamPoolThirdFactor = 1;



    private Faction factionAuthority;
    private Faction factionResistance;
    private string authorityPreferredArc;                               //string name of preferred node Arc for faction (if none then null)
    private string resistancePreferredArc;
    private int authorityMaxTasksPerTurn;                               //how many tasks the AI can undertake in a turns
    private int resistanceMaxTasksPerTurn;

    //fast access
    private int teamArcCivil = -1;
    private int teamArcControl = -1;
    private int teamArcMedia = -1;
    private int teamArcProbe = -1;
    private int teamArcSpider = -1;
    private int teamArcDamage = -1;
    private int teamArcErasure = -1;
    private int maxTeamsAtNode = -1;

    //info gathering lists (collated every turn)
    List<AINodeData> listNodeMaster = new List<AINodeData>();
    List<AINodeData> listStabilityCritical = new List<AINodeData>();
    List<AINodeData> listStabilityNonCritical = new List<AINodeData>();
    List<AINodeData> listSecurityCritical = new List<AINodeData>();
    List<AINodeData> listSecurityNonCritical = new List<AINodeData>();
    List<AINodeData> listSupportCritical = new List<AINodeData>();
    List<AINodeData> listSupportNonCritical = new List<AINodeData>();
    List<AINodeData> listOfTargetsKnown = new List<AINodeData>();
    List<AINodeData> listOfTargetsDamaged = new List<AINodeData>();
    List<AINodeData> listOfProbeNodes = new List<AINodeData>();
    List<AINodeData> listOfSpiderNodes = new List<AINodeData>();
    List<AINodeData> listOfErasureNodes = new List<AINodeData>();
    //other
    List<string> listOfErasureAILog = new List<string>();
    //tasks
    List<AITask> listOfTasksPotential = new List<AITask>();
    List<AITask> listOfTasksFinal = new List<AITask>();
    List<AITask> listOfSpiderTasks = new List<AITask>();
    List<AITask> listOfErasureTasks = new List<AITask>();


    public void Initialise()
    {
        factionAuthority = GameManager.instance.factionScript.factionAuthority;
        factionResistance = GameManager.instance.factionScript.factionResistance;
        Debug.Assert(factionAuthority != null, "Invalid factionAuthority (Null)");
        Debug.Assert(factionResistance != null, "Invalid factionResistance (Null)");
        //get names of node arcs (name or null, if none)
        if (factionAuthority.preferredArc != null) { authorityPreferredArc = factionAuthority.preferredArc.name; }
        if (factionResistance.preferredArc != null) { resistancePreferredArc = factionResistance.preferredArc.name; }
        authorityMaxTasksPerTurn = factionAuthority.maxTaskPerTurn;
        resistanceMaxTasksPerTurn = factionResistance.maxTaskPerTurn;
        //fast access
        teamArcCivil = GameManager.instance.dataScript.GetTeamArcID("CIVIL");
        teamArcControl = GameManager.instance.dataScript.GetTeamArcID("CONTROL");
        teamArcMedia = GameManager.instance.dataScript.GetTeamArcID("MEDIA");
        teamArcProbe = GameManager.instance.dataScript.GetTeamArcID("PROBE");
        teamArcSpider = GameManager.instance.dataScript.GetTeamArcID("SPIDER");
        teamArcDamage = GameManager.instance.dataScript.GetTeamArcID("DAMAGE");
        teamArcErasure = GameManager.instance.dataScript.GetTeamArcID("ERASURE");
        Debug.Assert(teamArcCivil > -1, "Invalid teamArcCivil");
        Debug.Assert(teamArcControl > -1, "Invalid teamArcControl");
        Debug.Assert(teamArcMedia > -1, "Invalid teamArcMedia");
        Debug.Assert(teamArcProbe > -1, "Invalid teamArcProbe");
        Debug.Assert(teamArcSpider > -1, "Invalid teamArcSpider");
        Debug.Assert(teamArcDamage > -1, "Invalid teamArcDamage");
        Debug.Assert(teamArcErasure > -1, "Invalid teamArcErasure");
        maxTeamsAtNode = GameManager.instance.teamScript.maxTeamsAtNode;
        Debug.Assert(maxTeamsAtNode > -1, "Invalid maxTeamsAtNode");
        //set up list of most connected Nodes
        SetConnectedNodes();
        SetPreferredNodes();
        SetCentreNodes();
        SetNearNeighbours();
    }

    /// <summary>
    /// Runs Resistance turn on behalf of AI
    /// TurnManager.cs -> ProcessNewTurn -> EndTurnFinalAI
    /// </summary>
    public void ProcessAISideResistance()
    {
        Debug.Log(string.Format("AIManager: ProcessAISideResistance{0}", "\n"));
        ExecuteTasks();
        ClearAICollections();
    }

    /// <summary>
    /// Runs Authority turn on behalf of AI
    /// TurnManager.cs -> ProcessNewTurn -> EndTurnFinalAI
    /// </summary>
    public void ProcessAISideAuthority()
    {
        Debug.Log(string.Format("AIManager: ProcessAISideAuthority{0}", "\n"));
        ExecuteTasks();
        ClearAICollections();
        //Info Gathering      
        GetAINodeData();
        ProcessNodeData();
        ProcessSpiderData();
        ProcessErasureData();
        //AI Rulesets
        ProcessNodeTasks();
        ProcessProbeTask();
        ProcessSpiderTask();
        ProcessDamageTask();
        ProcessErasureTask();
        //choose tasks for the turn
        ProcessTasksFinal(authorityMaxTasksPerTurn);
    }

    /// <summary>
    /// run prior to any info gathering each turn to empty out all data collections 
    /// All AIManager collections are for turn based data. Longer term data kept at DataManager.cs
    /// </summary>
    private void ClearAICollections()
    {
        //AINodeData
        listNodeMaster.Clear();
        listStabilityCritical.Clear();
        listStabilityNonCritical.Clear();
        listSecurityCritical.Clear();
        listSecurityNonCritical.Clear();
        listSupportCritical.Clear();
        listSupportNonCritical.Clear();
        listOfTargetsKnown.Clear();
        listOfTargetsDamaged.Clear();
        listOfProbeNodes.Clear();
        listOfSpiderNodes.Clear();
        listOfErasureNodes.Clear();
        //other
        listOfErasureAILog.Clear();
        //AITasks
        listOfTasksFinal.Clear();
        listOfTasksPotential.Clear();
        listOfSpiderTasks.Clear();
        listOfErasureTasks.Clear();
    }

    //
    // - - - Game Start Setup - - -
    //

    /// <summary>
    /// initialises list of most connected nodes (Game start)
    /// </summary>
    private void SetConnectedNodes()
    {
        int numOfConnections, numNodesHalf, counter, limit;
        //temp dictionary, key -> nodeID, value -> # of connections
        Dictionary<int, int> dictOfConnected = new Dictionary<int, int>();
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetAllNodes();
        List<Node> listOfMostConnectedNodes = new List<Node>();
        if (dictOfNodes != null)
        {
            numNodesHalf = dictOfNodes.Count / 2;
            //loop nodes and set up dictionary of nodes and their # of connections (Neighbours are used but same thing)
            foreach(var node in dictOfNodes)
            {
                if (node.Value != null)
                {
                    numOfConnections = node.Value.GetNumOfNeighbours();
                    //only select nodes that have 'x' number of connections
                    if (numOfConnections >= nodeConnectionThreshold)
                    {
                        try
                        { dictOfConnected.Add(node.Value.nodeID, numOfConnections); }
                        catch (ArgumentException)
                        { Debug.LogWarning(string.Format("Invalid connection entry (duplicate) for nodeID {0}", node.Value.nodeID)); }
                    }

                }
                else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", node.Key)); }
            }
            //check that there are at least 3 nodes in dict
            if (dictOfConnected.Count < 3)
            {
                //Not enough nodes -> loop again and add nodes with 1 - factor connections
                int numSpecific = nodeConnectionThreshold - 1;
                if (numSpecific > 0)
                {
                    foreach(var node in dictOfNodes)
                    {
                        if (node.Value != null)
                        {
                            numOfConnections = node.Value.GetNumOfNeighbours();
                            //only select nodes that have 'x' number of connections
                            if (numOfConnections == numSpecific)
                            {
                                try
                                { dictOfConnected.Add(node.Value.nodeID, numOfConnections); }
                                catch (ArgumentException)
                                { Debug.LogWarning(string.Format("Invalid connection entry (duplicate) for nodeID {0}", node.Value.nodeID)); }
                            }

                        }
                        else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", node.Key)); }
                    }
                    Debug.Log(string.Format("A!Manager.cs -> SetConnectedNodes: Extra nodes ({0} connections) have been added to the listOfMostConnectedNodes{1}", 
                        numSpecific, "\n"));
                }
                else { Debug.LogWarning("Insufficient records for SetConnectedNodes"); }
            }
            //ensure that their is a maximum of total Nodes on Map / 2 in the list -> take the top half
            limit = 999;
            if (dictOfConnected.Count > numNodesHalf == true)
            { limit = numNodesHalf; }
            //Final check for a viable number of records
            if (dictOfConnected.Count > 0)
            {
                if (dictOfConnected.Count >= 3)
                {
                    //Sort by # of connections
                    var sorted = from record in dictOfConnected orderby record.Value descending select record.Key;
                    counter = 0;
                    //select all nodes with 'x' connections
                    foreach (var record in sorted)
                    {
                        Node nodeConnected = GameManager.instance.dataScript.GetNode(record);
                        if (nodeConnected != null)
                        {
                            listOfMostConnectedNodes.Add(nodeConnected);
                            //set node field
                            nodeConnected.isConnectedNode = true;
                            counter++;
                            //ensure that a max of half the total nodes on the map are in the list (top half)
                            if (counter == limit)
                            { break; }
                        }
                        else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", record)); }
                    }
                    //Pass data to the main reference list
                    GameManager.instance.dataScript.SetConnectedNodes(listOfMostConnectedNodes);
                    Debug.Log(string.Format("A!Manager.cs -> SetConnectedNodes: {0} nodes have been added to the listOfMostConnectedNodes{1}", counter, "\n"));
                }
                else { Debug.LogWarning("Insufficient number of records ( < 3) for SetConnectedNodes"); }
            }
            else { Debug.LogError("Insufficient records (None) for SetConnectedNodes"); }
        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
    }

    /// <summary>
    /// loops nodes and sets '.isPreferredNode'flag (Game start)
    /// </summary>
    private void SetPreferredNodes()
    {
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetAllNodes();
        if (dictOfNodes != null)
        {
            //isPreferred
            if (authorityPreferredArc != null)
            {
                foreach (var node in dictOfNodes)
                {
                    if (node.Value.Arc.name.Equals(authorityPreferredArc) == true)
                    { node.Value.isPreferredAuthority = true; }
                    else { node.Value.isPreferredAuthority = false; }
                }
            }
            else { Debug.LogWarning("Invalid authorityPreferredArc (Null)"); }

            //Resistance preferred -> TO DO

        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
    }

    /// <summary>
    /// loops nodes and sets '.isCentreNode' flag (Game start)
    /// </summary>
    private void SetCentreNodes()
    {
        //calculate limits of central area of map
        float centreNum = 10 * nodeGeographicCentre / 100;
        float limit = centreNum / 2f;
        float upper = limit;
        float lower = limit * -1;
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetAllNodes();
        if (dictOfNodes != null)
        {
            foreach (var node in dictOfNodes)
            {
                //check each node to determine if it's inside or outside central area
                if (node.Value.nodePosition.x <= upper && node.Value.nodePosition.x >= lower)
                {
                    if (node.Value.nodePosition.z <= upper && node.Value.nodePosition.z >= lower)
                    { node.Value.isCentreNode = true; }
                    else
                    { node.Value.isCentreNode = false; }
                }
                else { node.Value.isCentreNode = false; }
            }
        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
    }

    /// <summary>
    /// For each node finds all nodes within 2 connections radius
    /// </summary>
    private void SetNearNeighbours()
    {
        List<Node> listOfNearNeighbours = new List<Node>();
        List<int> listLookup = new List<int>();
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetAllNodes();
        if (dictOfNodes != null)
        {
            foreach(var node in dictOfNodes)
            {
                List<Node> listOfImmediateNeighbours = node.Value.GetNeighbouringNodes();
                if (listOfImmediateNeighbours != null)
                {
                    //immediate neighbours
                    foreach(Node nodeimmediate in listOfImmediateNeighbours)
                    {
                        //use lookup list to check that node isn't already in system
                        if (listLookup.Exists(id => id == nodeimmediate.nodeID) == false)
                        {
                            //new node -> add to lookup and main lists
                            listLookup.Add(nodeimmediate.nodeID);
                            listOfNearNeighbours.Add(nodeimmediate);
                        }
                        //Neighbours of immediate neighbour (may not have any)
                        List<Node> listOfNeighbours = nodeimmediate.GetNeighbouringNodes();
                        if (listOfNeighbours != null)
                        {
                            if (listOfNeighbours.Count > 0)
                            {
                                foreach(Node nodeNear in listOfNeighbours)
                                {
                                    //use lookup list to check that node isn't already in system
                                    if (listLookup.Exists(id => id == nodeNear.nodeID) == false)
                                    {
                                        //new node -> add to lookup and main lists
                                        listLookup.Add(nodeNear.nodeID);
                                        listOfNearNeighbours.Add(nodeNear);
                                    }
                                }
                            }
                        }
                        else { Debug.LogErrorFormat("Invalid listOfNeighbours for nodeID {0}", nodeimmediate.nodeID); }
                    }
                }
                //Pass data to Node
                if (listOfNearNeighbours.Count > 0)
                { node.Value.SetNearNeighbours(listOfNearNeighbours); }
                //clear lists ready for next pass
                listOfNearNeighbours.Clear();
                listLookup.Clear();
            }
        }
    }


    //
    // - - - Miscellaneous - - -
    //

    /// <summary>
    /// Extracts all relevant AI data from an AI related message
    /// </summary>
    /// <param name="message"></param>
    public void GetAIMessageData(Message message)
    {
        if (message != null)
        {
            if (message.type == MessageType.AI)
            {
                switch (message.subType)
                {
                    case MessageSubType.AI_Connection:
                        //Get Connection and add Activity data
                        Connection connection = GameManager.instance.dataScript.GetConnection(message.data1);
                        if (connection != null)
                        {
                            connection.AddActivityData(message.turnCreated);
                            //add to queue of most recent activity -> destination nodeID and turn created
                            GameManager.instance.dataScript.AddToRecentConnectionQueue(new AITracker(message.data0, message.turnCreated));
                        }
                        else { Debug.LogWarning(string.Format("Invalid connection (Null) for connID {0} -> AI data NOT extracted", message.data1)); }
                        break;
                    case MessageSubType.AI_Node:
                        //Get Node and add Activity data
                        Node node = GameManager.instance.dataScript.GetNode(message.data0);
                        if (node != null)
                        {
                            node.AddActivityData(message.turnCreated);
                            //add to queue of most recent activity -> nodeID and turn created
                            GameManager.instance.dataScript.AddToRecentNodeQueue(new AITracker(message.data0, message.turnCreated));
                        }
                        else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0} -> AI data NOT extracted", message.data0)); }
                        break;
                    case MessageSubType.AI_Capture:
                    case MessageSubType.AI_Release:

                        //- - - TO DO - - - 

                        break;
                    default:
                        Debug.LogWarning(string.Format("Invalid message AI subType \"{0}\" for \"{1}\"", message.subType, message.text));
                        break;

                }
            }
            else { Debug.LogWarning(string.Format("Invalid (not AI) message type \"{0}\" for \"{1}\"", message.type, message.text)); }
        }
        else { Debug.LogWarning("Invalid message (Null)"); }
    }

    //
    // - - - Turn based AI - - -
    //

    /// <summary>
    /// gathers data on all nodes that have degraded in some from (from their starting values) and adds to listNodeMaster (from scratch each turn)
    /// also gathers data on known, but uncompleted, targets as well as Damaged targets that need to be contained
    /// </summary>
    private void GetAINodeData()
    {
        int data;
        AINodeData dataPackage;
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetAllNodes();
        if (dictOfNodes != null)
        {
            foreach (var node in dictOfNodes)
            {
                if (node.Value != null)
                {
                    //
                    // - - - Node datapoints - - -
                    //
                    //check that node isn't already maxxed out on teams
                    if (node.Value.CheckNumOfTeams() < maxTeamsAtNode)
                    {
                        //Stability
                        data = node.Value.GetNodeChange(NodeData.Stability);
                        if (data < 0)
                        {
                            //ignore if civil team already present
                            if (node.Value.isStabilityTeam == false)
                            {
                                //node stability has degraded
                                dataPackage = new AINodeData();
                                dataPackage.nodeID = node.Value.nodeID;
                                dataPackage.type = NodeData.Stability;
                                dataPackage.arc = node.Value.Arc;
                                dataPackage.difference = Mathf.Abs(data);
                                dataPackage.current = node.Value.Stability;
                                dataPackage.isPreferred = node.Value.isPreferredAuthority;
                                listNodeMaster.Add(dataPackage);
                            }
                        }
                        //Security
                        data = node.Value.GetNodeChange(NodeData.Security);
                        if (data < 0)
                        {
                            //ignore if control team already present
                            if (node.Value.isSecurityTeam == false)
                            {
                                //node stability has degraded
                                dataPackage = new AINodeData();
                                dataPackage.nodeID = node.Value.nodeID;
                                dataPackage.type = NodeData.Security;
                                dataPackage.arc = node.Value.Arc;
                                dataPackage.difference = Mathf.Abs(data);
                                dataPackage.current = node.Value.Security;
                                dataPackage.isPreferred = node.Value.isPreferredAuthority;
                                listNodeMaster.Add(dataPackage);
                            }
                        }
                        //Support (positive value indicates a problem, eg. growing support for resistance)
                        data = node.Value.GetNodeChange(NodeData.Support);
                        if (data > 0)
                        {
                            //ignore if media team already present
                            if (node.Value.isSupportTeam == false)
                            {
                                //node stability has degraded
                                dataPackage = new AINodeData();
                                dataPackage.nodeID = node.Value.nodeID;
                                dataPackage.type = NodeData.Support;
                                dataPackage.arc = node.Value.Arc;
                                dataPackage.difference = data;
                                dataPackage.current = node.Value.Support;
                                dataPackage.isPreferred = node.Value.isPreferredAuthority;
                                listNodeMaster.Add(dataPackage);
                            }
                        }
                        //
                        // - - - Probe nodes - - -
                        //
                        if (node.Value.isProbeTeam == false)
                        {
                            if (node.Value.isTargetKnown == false)
                            {
                                //probe team suitable node data package
                                dataPackage = new AINodeData();
                                dataPackage.nodeID = node.Value.nodeID;
                                dataPackage.type = NodeData.Probe;
                                dataPackage.arc = node.Value.Arc;
                                dataPackage.isPreferred = node.Value.isPreferredAuthority;
                                listOfProbeNodes.Add(dataPackage);
                            }
                        }
                    }
                    //
                    // - - - Targets (known and uncompleted) - - -
                    //
                    if (node.Value.isTargetKnown == true)
                    {
                        Target target = GameManager.instance.dataScript.GetTarget(node.Value.targetID);
                        if (target != null)
                        {
                            switch (target.targetStatus)
                            {
                                case Status.Active:
                                case Status.Live:
                                    //spider/erasure team node data package (good ambush situation as targets will lure in resistance player)
                                    dataPackage = new AINodeData();
                                    dataPackage.nodeID = node.Value.nodeID;
                                    dataPackage.type = NodeData.Target;
                                    dataPackage.arc = node.Value.Arc;
                                    dataPackage.isPreferred = node.Value.isPreferredAuthority;
                                    listOfTargetsKnown.Add(dataPackage);
                                    break;
                                case Status.Completed:
                                    //Damage team node data package (only for completed targets with ongoing effects that require containing)
                                    if (node.Value.CheckForOngoingEffects() == true)
                                    {
                                        dataPackage = new AINodeData();
                                        dataPackage.nodeID = node.Value.nodeID;
                                        dataPackage.type = NodeData.Target;
                                        dataPackage.arc = node.Value.Arc;
                                        dataPackage.isPreferred = node.Value.isPreferredAuthority;
                                        listOfTargetsDamaged.Add(dataPackage);
                                    }
                                    break;
                            }
                        }
                        else { Debug.LogWarning(string.Format("Invalid target (Null) for targetID {0}", node.Value.targetID)); }
                    }
                }
                else { Debug.LogWarning(string.Format("Invalid node (Null) in dictOfNodes for nodeID {0}", node.Key)); }
            }
        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
    }

    /// <summary>
    /// Processes raw node data into useable data
    /// </summary>
    private void ProcessNodeData()
    {
        //loop master list and populate sub lists
        if (listNodeMaster.Count > 0)
        {
            foreach (AINodeData data in listNodeMaster)
            {
                //critical is when datapoint has reached max bad condition
                switch(data.type)
                {
                    case NodeData.Stability:
                        if (data.current == 0)
                        { listStabilityCritical.Add(data); }
                        else { listStabilityNonCritical.Add(data); }
                        break;
                    case NodeData.Security:
                        if (data.current == 0)
                        { listSecurityCritical.Add(data); }
                        else { listSecurityNonCritical.Add(data); }
                        break;
                    case NodeData.Support:
                        if (data.current == 3)
                        { listSupportCritical.Add(data); }
                        else { listSupportNonCritical.Add(data); }
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid AINodeData.type \"{0}\"", data.type));
                        break;
                }
            }
        }
    }

    /// <summary>
    /// processes raw most connected node data by giving all a score (higher the score, more likely node is to be selected)
    /// </summary>
    private void ProcessSpiderData()
    {
        //only bother proceeding if there are spider teams available to deploy
        if (GameManager.instance.dataScript.CheckTeamInfo(teamArcSpider, TeamInfo.Reserve) > 0)
        {
            List<Node> listOfMostConnected = GameManager.instance.dataScript.GetMostConnectedNodes();
            if (listOfMostConnected != null)
            {
                int score, tally;
                int currentTurn = GameManager.instance.turnScript.Turn;
                foreach (Node node in listOfMostConnected)
                {
                    //ignore nodes with existing spider teams & spiders
                    if (node.isSpiderTeam == false && node.isSpider == false)
                    {
                        //check that node isn't already maxxed out on teams
                        if (node.CheckNumOfTeams() < maxTeamsAtNode)
                        {
                            AINodeData dataPackage = new AINodeData();
                            dataPackage.nodeID = node.nodeID;
                            dataPackage.type = NodeData.Spider;
                            dataPackage.arc = node.Arc;
                            //calculate score 
                            score = 0;
                            //preferred type
                            if (node.isPreferredAuthority == true)
                            { score += nodePreferredFactor; }
                            //central region
                            if (node.isCentreNode == true)
                            { score += nodeCentreFactor; }
                            //resistance activity at node
                            if (node.activityCount > 0)
                            { score += node.activityCount * nodeActivityCountFactor; }
                            //resistance activity time, eg. how recent? (min capped at 0)
                            if (node.activityTime > -1)
                            {
                                tally = nodeActivityTimeFactor - (currentTurn - node.activityTime);
                                if (tally > 0)
                                { score += tally; }
                            }
                            //connections
                            List<Connection> listOfConnections = node.GetAllConnections();
                            if (listOfConnections != null)
                            {
                                foreach (Connection conn in listOfConnections)
                                {
                                    switch (conn.SecurityLevel)
                                    {
                                        case ConnectionType.HIGH:
                                            score += securityHighFactor;
                                            break;
                                        case ConnectionType.MEDIUM:
                                            score += securityMediumFactor;
                                            break;
                                        case ConnectionType.LOW:
                                            score += securityLowFactor;
                                            break;
                                        case ConnectionType.None:
                                            score += securityNoneFactor;
                                            break;
                                        default:
                                            Debug.LogWarning(string.Format("Invalid connecion Security Level \"{0}\"", conn.SecurityLevel));
                                            break;
                                    }
                                }
                            }
                            else { Debug.LogWarning(string.Format("Invalid listOfConnections (Null) for nodeID {0}", node.nodeID)); }
                            dataPackage.score = score;
                            //add data to list
                            listOfSpiderNodes.Add(dataPackage);
                        }
                    }
                }
            }
            else { Debug.LogError("Invalid listOfMostConnected (Null)"); }
        }
        else { Debug.Log(string.Format("AIManager.cs -> ProcessSpiderData: No Spider teams available in reserves{0}", "\n")); }
    }

    /// <summary>
    /// Determines the target node to be used as a focal point for Erasure AI calculations. Returns -1 if none found. Called by ProcessErasureData
    /// </summary>
    private int ProcessErasureTarget()
    {
        int nodeReturnID = -1;
        Queue<AITracker> queueRecentConnections = GameManager.instance.dataScript.GetRecentConnectionsQueue();
        Queue<AITracker> queueRecentNodes = GameManager.instance.dataScript.GetRecentNodesQueue();
        if (queueRecentConnections != null && queueRecentNodes != null)
        {
            int currentTurn = GameManager.instance.turnScript.Turn;
            int connID = -1;
            int turnConn = 0;
            //if there isn't any confirmed player activity within the specified time frame then there is no target node
            if (queueRecentConnections.Count > 0)
            {
                //examine connection activity queue and find the most recent
                foreach (AITracker data in queueRecentConnections)
                {
                    if (data.turn > turnConn)
                    {
                        turnConn = data.turn;
                        connID = data.data0;
                    }
                }
                //if was within previous 2 turns then this is the target node (definite player activity)
                if (currentTurn - turnConn <= trackerNumOfTurnsAgo)
                {
                    nodeReturnID = connID;
                    if (connID > -1)
                    { listOfErasureAILog.Add(string.Format("Target: Recent Connection nodeID {0}, turn {1}", nodeReturnID, turnConn)); }
                }
                else
                {
                    int nodeID = -1;
                    int turnNode = -1;
                    //check for most recent turn activity (could be player or actor)
                    foreach (AITracker data in queueRecentNodes)
                    {
                        if (data.turn > turnNode)
                        {
                            turnNode = data.turn;
                            nodeID = data.data0;
                        }
                    }
                    if (currentTurn - turnNode <= trackerNumOfTurnsAgo)
                    {
                        listOfErasureAILog.Add(string.Format("Target: Recent Node nodeID {0}, turn {1}", nodeID, turnNode));
                        //check if matches any recent connection activity
                        foreach (AITracker data in queueRecentConnections)
                        {
                            if (data.data0 == nodeID)
                            {
                                //a match -> Player was confirmed as being at this node
                                nodeReturnID = nodeID;
                                listOfErasureAILog.Add(string.Format("Target: Confirmed Conn Activity for ID {0}, turn {1}", nodeID, turnNode));
                                break;
                            }
                        }
                    }
                }
            }
            if (nodeReturnID > 0)
            { listOfErasureAILog.Add(string.Format("Target: nodeReturnID {0}", nodeReturnID)); }
            else { listOfErasureAILog.Add("No viable target node found"); }
            Debug.LogFormat("AIManager.cs -> ProcessErasureTarget: target nodeID {0}{1}", nodeReturnID);
        }
        else { Debug.LogWarning("Invalid queue (Null)"); }
        return nodeReturnID;
    }

    /// <summary>
    /// Processes the potential nodes based on a specified target node -> Note: there may be no target node as nothing notable has happened
    /// </summary>
    private void ProcessErasureData()
    {
        int score, tally;
        int numOfTeams = GameManager.instance.dataScript.CheckTeamInfo(teamArcErasure, TeamInfo.Reserve);
        int currentTurn = GameManager.instance.turnScript.Turn;
        //only bother proceeding if there are spider teams available to deploy
        if (numOfTeams > 0)
        {
            //get target node
            int targetNodeID = ProcessErasureTarget();
            if (targetNodeID > -1)
            {
                //get near neighbours as potential node targets
                Node node = GameManager.instance.dataScript.GetNode(targetNodeID);
                if (node != null)
                {
                    List<Node> listOfNearNeighbours = node.GetNearNeighbours();
                    if (listOfNearNeighbours != null)
                    {
                        foreach (Node nodeNear in listOfNearNeighbours)
                        {
                            if (nodeNear.CheckNumOfTeams() < maxTeamsAtNode)
                            {
                                //Erasure team not already present
                                if (nodeNear.isErasureTeam == false)
                                {
                                    score = 0;
                                    //activity count
                                    if (nodeNear.activityCount > -1)
                                    { score += nodeNear.activityCount * nodeActivityCountFactor; }
                                    //activity time (min capped at 0)
                                    if (nodeNear.activityTime > -1)
                                    {
                                        tally = nodeActivityTimeFactor - (currentTurn - nodeNear.activityTime);
                                        if (tally > 0)
                                        { score += tally; }
                                    }
                                    //spider (not known)
                                    if (nodeNear.isSpider == true)
                                    {
                                        //full effect if spider not known, halved otherwise
                                        if (nodeNear.isSpiderKnown == false)
                                        { score += nodeSpiderFactor; }
                                        else { score += nodeSpiderFactor / 2; }
                                    }
                                    //in list of most connected
                                    if (nodeNear.isConnectedNode == true)
                                    { score += nodeConnectedFactor; }
                                    //chokepoint node
                                    if (nodeNear.isChokepointNode == true)
                                    { score += nodeChokepointFactor; }
                                    //preferred target
                                    if (nodeNear.isPreferredAuthority == true)
                                    { score += nodePreferredFactor; }
                                    //target
                                    if (nodeNear.targetID > -1)
                                    {
                                        Target target = GameManager.instance.dataScript.GetTarget(nodeNear.targetID);
                                        {
                                            //positive effect if target known by AI and uncompleted, reversed negative effect otherwise
                                            if (target != null)
                                            {
                                                switch (target.targetStatus)
                                                {
                                                    case Status.Active:
                                                    case Status.Live:
                                                        if (target.isKnownByAI)
                                                        { score += nodeTargetFactor; }
                                                        break;
                                                    case Status.Completed:
                                                    case Status.Contained:
                                                        score -= nodeTargetFactor;
                                                        break;
                                                }
                                            }
                                            else { Debug.LogWarningFormat("Invalid target (Null) for targetID {0}", nodeNear.targetID); }
                                        }
                                    }
                                    //add to list of possible target nodes
                                    AINodeData data = new AINodeData()
                                    {
                                        nodeID = nodeNear.nodeID,
                                        arc = nodeNear.Arc,
                                        type = NodeData.Erasure,
                                        isPreferred = nodeNear.isPreferredAuthority,
                                        score = score
                                    };
                                    listOfErasureNodes.Add(data);
                                }
                            }
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid listOfNearNeighbours for nodeID {0}", node.nodeID); }
                }
                else { Debug.LogWarningFormat("Invalid target node (Null) for nodeID {0}", targetNodeID); }
            }
            else
            {
                Debug.Log("AIManager.cs -> ProcessErasureData: No suitable target node found");

                //if a minimum number of erasure teams in reserve then place one at a likely place
                
                if (numOfTeams > 1)
                {
                    List<Node> listOfMostConnected = GameManager.instance.dataScript.GetMostConnectedNodes();
                    if (listOfMostConnected != null)
                    {
                        foreach (Node nodeConnected in listOfMostConnected)
                        {
                            //most connected -> Unknown Spider (incorporates activity time and count)
                            if (nodeConnected.isSpider == true && nodeConnected.isSpiderKnown == false)
                            {
                                //add to list of possible target nodes -> all are scored equally (spiderAI ensures high scoring node)
                                AINodeData data = new AINodeData()
                                {
                                    nodeID = nodeConnected.nodeID,
                                    arc = nodeConnected.Arc,
                                    type = NodeData.Erasure,
                                    isPreferred = nodeConnected.isPreferredAuthority,
                                    score = 0
                                };
                                listOfErasureNodes.Add(data);
                            }
                        }
                    }
                    else { Debug.LogWarning("Invalid lisOfMostConnected (Null)"); }
                }
            }
        }
        else { Debug.LogFormat("AIManager.cs -> ProcessSpiderData: No Erasure teams available in reserves{0}", "\n"); }
    }


    /// <summary>
    /// master method that determines up to 3 separate tasks, one for each node datapoint and the relevant team (Control/Civil/Media)
    /// </summary>
    private void ProcessNodeTasks()
    {
        int numOfTeams;
        //Stability
        numOfTeams = GameManager.instance.dataScript.CheckTeamInfo(teamArcCivil, TeamInfo.Reserve);
        if (numOfTeams > 0)
        {
            AITask taskStability = SelectNodeTask(listStabilityCritical, listStabilityNonCritical, "CIVIL", teamArcCivil);
            if (taskStability != null) { listOfTasksPotential.Add(taskStability); }
            else { Debug.Log(string.Format("AIManager.cs -> ProcessNodeTasks: No available Stability Node tasks{0}", "\n")); }
        }
        else { Debug.Log(string.Format("AIManager.cs -> ProcessNodeTasks: No Civil teams available in reserves{0}", "\n")); }
        //Security
        numOfTeams = GameManager.instance.dataScript.CheckTeamInfo(teamArcControl, TeamInfo.Reserve);
        if (numOfTeams > 0)
        {
            AITask taskSecurity = SelectNodeTask(listSecurityCritical, listSecurityNonCritical, "CONTROL", teamArcControl);
            if (taskSecurity != null) { listOfTasksPotential.Add(taskSecurity); }
            else { Debug.Log(string.Format("AIManager.cs -> ProcessNodeTasks: No available Security Node tasks{0}", "\n")); }
        }
        else { Debug.Log(string.Format("AIManager.cs -> ProcessNodeTasks: No Security teams available in reserves{0}", "\n")); }
        //Support
        numOfTeams = GameManager.instance.dataScript.CheckTeamInfo(teamArcMedia, TeamInfo.Reserve);
        if (numOfTeams > 0)
        {
            AITask taskSupport = SelectNodeTask(listSupportCritical, listSupportNonCritical, "MEDIA", teamArcMedia);
            if (taskSupport != null) { listOfTasksPotential.Add(taskSupport); }
            else { Debug.Log(string.Format("AIManager.cs -> ProcessNodeTasks: No available Support Node tasks{0}", "\n")); }
        }
        else { Debug.Log(string.Format("AIManager.cs -> ProcessNodeTasks: No Media teams available in reserves{0}", "\n")); }

    }

    /// <summary>
    /// selects a probe team task if one is available
    /// </summary>
    private void ProcessProbeTask()
    {
        if (GameManager.instance.dataScript.CheckTeamInfo(teamArcProbe, TeamInfo.Reserve) > 0)
        {
            int numOfRecords = listOfProbeNodes.Count;
            if (numOfRecords > 0)
            {
                AINodeData data = listOfProbeNodes[Random.Range(0, numOfRecords)];
                if (data != null)
                {
                    //create a task
                    AITask taskProbe = new AITask()
                    {
                        data0 = data.nodeID,
                        data1 = teamArcProbe,
                        name0 = data.arc.name,
                        name1 = "PROBE",
                        type = AIType.Team,
                        priority = Priority.Low
                    };
                    //add to list of potentials
                    listOfTasksPotential.Add(taskProbe);
                }
                else { Debug.LogWarning("Invalid record from listOfProbeNodes (Null)"); }
            }
            else { Debug.Log("No available nodes for a Probe Team"); }
        }
        else { Debug.Log(string.Format("AIManager.cs -> ProcessProbeTasks: No Probe teams available in reserves{0}", "\n")); }
    }

    /// <summary>
    /// selects a spider team if a one is available
    /// </summary>
    private void ProcessSpiderTask()
    {
        //only bother proceeding if there are spider teams available to deploy
        if (GameManager.instance.dataScript.CheckTeamInfo(teamArcSpider, TeamInfo.Reserve) > 0)
        {
            if (listOfSpiderNodes.Count > 0)
            {
                int counter = 0;
                //sort Node list by score (highest at top)
                var sorted = from record in listOfSpiderNodes orderby record.score descending select record;
                //take top three records and create a task
                foreach (var record in sorted)
                {
                    if (record != null)
                    {
                        counter++;
                        AITask task = new AITask()
                        {
                            data0 = record.nodeID,
                            data1 = teamArcSpider,
                            name0 = record.arc.name,
                            name1 = "SPIDER",
                            type = AIType.Team,
                        };
                        switch (counter)
                        {
                            case 1:
                                //highest ranked score -> add three entries to pool
                                task.priority = Priority.High;
                                for (int i = 0; i < teamPoolFirstFactor; i++)
                                { listOfSpiderTasks.Add(task); }
                                break;
                            case 2:
                                //second highest ranked score -> add two entries to pool
                                task.priority = Priority.Medium;
                                for (int i = 0; i < teamPoolSecondFactor; i++)
                                { listOfSpiderTasks.Add(task); }
                                break;
                            case 3:
                                //third highest ranked score -> add one entry to pool
                                task.priority = Priority.Low;
                                for (int i = 0; i < teamPoolThirdFactor; i++)
                                { listOfSpiderTasks.Add(task); }
                                break;
                        }
                        if (counter == 3) { break; }
                    }
                    else { Debug.LogWarning("Invalid record (Null) in listOfSpiderNodes"); }
                }
            }
            //Add Known, but uncompleted, targets to the pool
            if (listOfTargetsKnown.Count > 0)
            {
                foreach (AINodeData record in listOfTargetsKnown)
                {
                    if (record != null)
                    {
                        AITask task = new AITask()
                        {
                            data0 = record.nodeID,
                            data1 = teamArcSpider,
                            name0 = record.arc.name,
                            name1 = "SPIDER",
                            type = AIType.Team,
                            priority = Priority.High
                        };
                        //Preferred node type -> Priority Critical and one extra task added to pool
                        if (record.isPreferred == true)
                        {
                            listOfSpiderTasks.Add(task);
                            task.priority = Priority.Critical;
                        }
                        //add target task to pool
                        for (int i = 0; i < teamPoolTargetFactor; i++)
                        { listOfSpiderTasks.Add(task); }
                    }
                    else { Debug.LogWarning("Invalid record (Null) in listOfTargetsKnown"); }
                }
            }
            //Select random task from pool
            if (listOfSpiderTasks.Count > 0)
            {
                AITask taskSpider = new AITask();
                taskSpider = listOfSpiderTasks[Random.Range(0, listOfSpiderTasks.Count)];
                //add to list of potentials
                listOfTasksPotential.Add(taskSpider);
            }
            else { Debug.Log(string.Format("AIManager.cs -> ProcessSpiderTask: No available Spider Team tasks{0}", "\n")); }
        }
    }


    /// <summary>
    /// Determines the Erasure task 
    /// </summary>
    private void ProcessErasureTask()
    {
        //only bother proceeding if there are erasure teams available to deploy
        if (GameManager.instance.dataScript.CheckTeamInfo(teamArcErasure, TeamInfo.Reserve) > 0)
        {
            if (listOfErasureNodes.Count > 0)
            {
                int counter = 0;
                //sort Node list by score (highest at top)
                var sorted = from record in listOfErasureNodes orderby record.score descending select record;
                //take top three records and create a task
                foreach (var record in sorted)
                {
                    if (record != null)
                    {
                        counter++;
                        AITask task = new AITask()
                        {
                            data0 = record.nodeID,
                            data1 = teamArcErasure,
                            name0 = record.arc.name,
                            name1 = "ERASURE",
                            type = AIType.Team,
                            priority = Priority.Critical
                        };
                        switch (counter)
                        {
                            case 1:
                                //highest ranked score -> add three entries to pool
                                for (int i = 0; i < teamPoolFirstFactor; i++)
                                { listOfErasureTasks.Add(task); }
                                break;
                            case 2:
                                //second highest ranked score -> add two entries to pool
                                for (int i = 0; i < teamPoolSecondFactor; i++)
                                { listOfErasureTasks.Add(task); }
                                break;
                            case 3:
                                //third highest ranked score -> add one entry to pool
                                for (int i = 0; i < teamPoolThirdFactor; i++)
                                { listOfErasureTasks.Add(task); }
                                break;
                        }
                        if (counter == 3) { break; }
                    }
                    else { Debug.LogWarning("Invalid record (Null) in listOfErasureNodes"); }
                }
                //Select random task from pool
                if (listOfErasureTasks.Count > 0)
                {
                    AITask taskErasure = new AITask();
                    taskErasure = listOfErasureTasks[Random.Range(0, listOfErasureTasks.Count)];
                    //add to list of potentials
                    listOfTasksPotential.Add(taskErasure);
                }
                else { Debug.Log(string.Format("AIManager.cs -> ProcessErasureTask: No available Erasure Team tasks{0}", "\n")); }
            }
        }
    }

    /// <summary>
    /// selects a damage team if one available
    /// </summary>
    private void ProcessDamageTask()
    {
        //Damage teams available to deploy?
        if (GameManager.instance.dataScript.CheckTeamInfo(teamArcDamage, TeamInfo.Reserve) > 0)
        {
            if (listOfTargetsDamaged.Count > 0)
            {
                foreach(AINodeData data in listOfTargetsDamaged)
                {
                    if (data != null)
                    {
                        Node node = GameManager.instance.dataScript.GetNode(data.nodeID);
                        if (node != null)
                        {
                            //room for another team?
                            if (node.CheckNumOfTeams() < maxTeamsAtNode)
                            {
                                //damage team not already present
                                if (node.isDamageTeam == false)
                                {
                                    AITask taskDamage = new AITask()
                                    {
                                        data0 = data.nodeID,
                                        data1 = teamArcDamage,
                                        name0 = data.arc.name,
                                        name1 = "DAMAGE",
                                        type = AIType.Team,
                                        priority = Priority.High
                                    };
                                    //preferred node type -> Critical
                                    if (data.isPreferred == true)
                                    { taskDamage.priority = Priority.Critical; }
                                    //add to list of potentials
                                    listOfTasksPotential.Add(taskDamage);
                                }
                            }
                        }
                        else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", data.nodeID)); }
                    }
                    else { Debug.LogWarning("Invalid record (Null) in listOfTargetsDamaged"); }
                }
            }
            else { Debug.Log(string.Format("AIManager.cs -> ProcessDamageTask: No available Damaged Targets{0}", "\n")); }
        }
        else { Debug.Log(string.Format("AIManager.cs -> ProcessDamageTask: No Damage teams available in reserves{0}", "\n")); }
    }

    /// <summary>
    /// Selects tasks from pool of potential tasks for this turn
    /// </summary>
    private void ProcessTasksFinal(int maxTasksPerTurn)
    {
        int numTasks = listOfTasksPotential.Count;
        int index, odds;
        float baseOdds = 100f;
        int numTasksSelected = 0;
        if (numTasks > 0)
        {
            List<AITask> listOfTasksCritical = new List<AITask>();
            List<AITask> listOfTasksNonCritical = new List<AITask>();
            //loop listOfTasksPotential and split tasks into Critical and non critical
            foreach (AITask task in listOfTasksPotential)
            {
                switch (task.priority)
                {
                    case Priority.Critical:
                        listOfTasksCritical.Add(task);
                        break;
                    default:
                        listOfTasksNonCritical.Add(task);
                        break;
                }
            }
            //Critical tasks first
            numTasks = listOfTasksCritical.Count;
            if (numTasks > 0)
            {
                if (numTasks <= maxTasksPerTurn)
                {
                    //enough available tasks to do all
                    foreach (AITask task in listOfTasksCritical)
                    {
                        listOfTasksFinal.Add(task);
                        task.chance = (int)baseOdds;
                        numTasksSelected++;
                    }
                }
                else
                {
                    //insufficient tasks -> work out odds first
                    odds = (int)(baseOdds / numTasks);
                    foreach(AITask task in listOfTasksCritical)
                    { task.chance = odds; }
                    //randomly choose
                    do
                    {
                        index = Random.Range(0, numTasks);
                        listOfTasksFinal.Add(listOfTasksCritical[index]);
                        numTasksSelected++;
                        //remove entry from list to prevent future selection
                        listOfTasksCritical.RemoveAt(index);
                    }
                    while (numTasksSelected < maxTasksPerTurn);
                }
            }
            //still room 
            if (numTasksSelected < maxTasksPerTurn)
            {
                numTasks = listOfTasksNonCritical.Count;
                int remainingChoices = maxTasksPerTurn - numTasksSelected;
                //Non-Critical tasks next
                if (remainingChoices >= numTasks)
                {
                    //do all -> assign odds first

                    /*if (remainingChoices == numTasks) { odds = (int)baseOdds; }
                    else { odds = (int)((baseOdds / (float)numTasks) * remainingChoices); }*/

                    foreach (AITask task in listOfTasksNonCritical)
                    { task.chance = (int)baseOdds; }
                    //select tasks
                    foreach (AITask task in listOfTasksNonCritical)
                    {
                        listOfTasksFinal.Add(task);
                        numTasksSelected++;
                        if (numTasksSelected >= maxTasksPerTurn)
                        { break; }
                    }
                }
                else
                {
                    //need to randomly select from pool of tasks based on priority probabilities
                    List<AITask> tempList = new List<AITask>();
                    //populate tempList with copies of NonCritical tasks depending on priority (low -> 1 copy, medium -> 2 copies, high -> 3 copies)
                    foreach (AITask task in listOfTasksNonCritical)
                    {
                        switch(task.priority)
                        {
                            case Priority.High:
                                for (int i = 0; i < priorityHighWeight; i++)
                                { tempList.Add(task); }
                                break;
                            case Priority.Medium:
                                for (int i = 0; i < priorityMediumWeight; i++)
                                { tempList.Add(task); }
                                break;
                            case Priority.Low:
                                for (int i = 0; i < priorityLowWeight; i++)
                                { tempList.Add(task); }
                                break;
                            default:
                                Debug.LogWarning(string.Format("Invalid task.priority \"{0}\" for nodeID {1}, team {2}", task.priority, task.data0, task.name1));
                                break;
                        }
                    }
                    //work out and assign odds first
                    numTasks = tempList.Count;
                    foreach(AITask task in listOfTasksNonCritical)
                    {
                        switch(task.priority)
                        {
                            case Priority.High:
                                task.chance = (int)(((float)priorityHighWeight / (float)numTasks) * 100 * remainingChoices);
                                break;
                            case Priority.Medium:
                                task.chance = (int)(((float)priorityMediumWeight / (float)numTasks) * 100 * remainingChoices);
                                break;
                            case Priority.Low:
                                task.chance = (int)(((float)priorityLowWeight / (float)numTasks) * 100 * remainingChoices);
                                break;
                        }
                    }
                    //randomly draw from pool
                    string selectedTeamArc;
                    do
                    {
                        numTasks = tempList.Count;
                        if (numTasks > 0)
                        {
                            index = Random.Range(0, numTasks);
                            listOfTasksFinal.Add(tempList[index]);
                            selectedTeamArc = tempList[index].name1;
                            numTasksSelected++;
                            //don't bother unless further selections are needed
                            if (numTasksSelected < maxTasksPerTurn)
                            {
                                //reverse loop and remove all instances of task from tempList to prevent duplicate selections
                                for (int i = numTasks - 1; i >= 0; i--)
                                {
                                    if (tempList[i].name1.Equals(selectedTeamArc) == true)
                                    { tempList.RemoveAt(i); }
                                }
                            }
                        }
                        else { numTasksSelected++; }
                    }
                    while (numTasksSelected < maxTasksPerTurn);
                }
            }

        }
        else { Debug.Log(string.Format("AIManager.cs -> ProcessTasksFinal: No tasks this turn{0}", "\n")); }
    }


    /// <summary>
    /// sub method (ProcessNodeTasks) that takes two node datapoint lists (must be the same datapoint, eg. security) and determines a task (Null if none) based on AI rules
    /// </summary>
    /// <param name="listCritical"></param>
    /// <param name="listNonCritical"></param>
    /// <returns></returns>
    private AITask SelectNodeTask(List<AINodeData> listCritical, List<AINodeData> listNonCritical, string name, int teamArcID)
    {
        AITask task = null;
        int index;
        List<AINodeData> tempList = new List<AINodeData>();
        //check for Critical tasks first
        int listCount = listCritical.Count;
        if (listCount > 0)
        {
            index = 0;
            if (listCount > 1)
            {
                //scan for any nodes of the preferred faction type
                foreach (AINodeData data in listCritical)
                {
                    if (data.isPreferred == true)
                    { tempList.Add(data); }
                }
                if (tempList.Count > 0)
                {
                    //randomly select a preferred faction option
                    index = Random.Range(0, tempList.Count);
                    //generate task
                    task = new AITask() { data0 = tempList[index].nodeID, name0 = tempList[index].arc.name, name1 = name, priority = Priority.Critical };
                }
                else
                {
                    //otherwise randomly select any option
                    index = Random.Range(0, listCount);
                    //generate task
                    task = new AITask() { data0 = listCritical[index].nodeID, name0 = listCritical[index].arc.name, name1 = name, priority = Priority.Critical };
                }
            }
            else
            {
                //single record only
                task = new AITask() { data0 = listCritical[0].nodeID, name0 = listCritical[0].arc.name, name1 = name, priority = Priority.Critical };
            }
        }
        else
        {
            //otherwise Non Critical
            listCount = listNonCritical.Count;
            if (listCount > 0)
            {
                index = 0;
                Priority priority = Priority.Low;
                tempList.Clear();
                //scan for any nodes of the preferred faction type
                foreach (AINodeData data in listNonCritical)
                {
                    if (data.isPreferred == true)
                    { tempList.Add(data); }
                }
                if (tempList.Count > 0)
                {
                    //randomly select a preferred faction option
                    index = Random.Range(0, tempList.Count);
                    //determine priority (one notch higher than normal due to being a preferred faction node arc)
                    switch (tempList[index].difference)
                    {
                        case 1: priority = Priority.Medium; break;
                        case 2: priority = Priority.High; break;
                        default:
                            Debug.LogWarning(string.Format("Invalid difference \"{0}\" for nodeID {1}", tempList[0].difference,
                       tempList[0].nodeID)); break;
                    }
                    //generate task
                    task = new AITask() { data0 = tempList[index].nodeID, name0 = tempList[index].arc.name, name1 = name, priority = priority };
                }
                else
                {
                    //otherwise randomly select any option
                    index = Random.Range(0, listCount);
                    //determine priority
                    switch (listNonCritical[index].difference)
                    {
                        case 1: priority = Priority.Low; break;
                        case 2: priority = Priority.Medium; break;
                        default:
                            Debug.LogWarning(string.Format("Invalid difference \"{0}\" for nodeID {1}", listNonCritical[0].difference,
                       listNonCritical[0].nodeID)); break;
                    }
                    //generate task
                    task = new AITask() { data0 = listNonCritical[index].nodeID, name0 = listNonCritical[index].arc.name, name1 = name, priority = priority };
                }
            }
        }
        //add final data
        if (task != null)
        {
            task.type = AIType.Team;
            task.data1 = teamArcID;
        }
        return task;
    }


    /// <summary>
    /// carry out all tasks in listOfTasksFinal 
    /// </summary>
    private void ExecuteTasks()
    {
        int dataID;
        foreach(AITask task in listOfTasksFinal)
        {
            switch (task.type)
            {
                case AIType.Team:
                    dataID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, task.data1);
                    if (dataID > -1)
                    {
                        Node node = GameManager.instance.dataScript.GetNode(task.data0);
                        if (node != null)
                        { GameManager.instance.teamScript.MoveTeamAI(TeamPool.OnMap, dataID, node); }
                        else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", task.data0)); }
                    }
                    else { Debug.LogWarning(string.Format("Invalid teamID (-1) for teamArcID {0}", task.data1)); }
                    break;
                default:
                    Debug.LogError(string.Format("Invalid task.type \"{0}\"", task.type));
                    break;
            }
        }
    }

    //
    // - - - Debug - - -
    //

    /// <summary>
    /// Debug display of all Task related data
    /// </summary>
    /// <returns></returns>
    public string DisplayTaskData()
    {
        StringBuilder builder = new StringBuilder();
        //Task lists
        builder.AppendFormat("- listOfTasksFinal{0}", "\n");
        builder.Append(DebugTaskList(listOfTasksFinal));
        builder.AppendFormat("{0}{1}- listOfTasksPotential{2}", "\n", "\n", "\n");
        builder.Append(DebugTaskList(listOfTasksPotential));
        return builder.ToString();
    }

    /// <summary>
    /// Debug display method for AI Node data
    /// </summary>
    /// <returns></returns>
    public string DisplayNodeData()
    {
        StringBuilder builder = new StringBuilder();
        //Master Node list
        builder.AppendFormat("- listNodeMaster{0}", "\n");
        builder.Append(DebugDisplayList(listNodeMaster));
        //Sub Node lists
        builder.AppendFormat("{0}{1}- listStabilityCritical{2}", "\n", "\n", "\n");
        builder.Append(DebugDisplayList(listStabilityCritical));
        builder.AppendFormat("{0}- listStabilityNonCritical{1}", "\n", "\n");
        builder.Append(DebugDisplayList(listStabilityNonCritical));
        builder.AppendFormat("{0}- listSecurityCritical{1}", "\n", "\n");
        builder.Append(DebugDisplayList(listSecurityCritical));
        builder.AppendFormat("{0}- listSecurityNonCritical{1}", "\n", "\n");
        builder.Append(DebugDisplayList(listSecurityNonCritical));
        builder.AppendFormat("{0}- listSupportCritical{1}", "\n", "\n"); ;
        builder.Append(DebugDisplayList(listSupportCritical));
        builder.AppendFormat("{0}- listSupportNonCritical{1}", "\n", "\n");
        builder.Append(DebugDisplayList(listSupportNonCritical));
        return builder.ToString();
    }

    /// <summary>
    /// Debug display of all relevant spider AI related data
    /// </summary>
    /// <returns></returns>
    public string DisplaySpiderData()
    {
        StringBuilder builder = new StringBuilder();
        //Task lists
        builder.AppendFormat("- listOfSpiderTask{0}", "\n");
        builder.Append(DebugTaskList(listOfSpiderTasks));
        builder.AppendFormat("{0}- listOfSpiderNodes{1}", "\n", "\n");
        if (listOfSpiderNodes.Count > 0)
        {
            foreach (AINodeData data in listOfSpiderNodes)
            { builder.AppendFormat(" ID {0}, {1}, isPreferred: {2}, Score: {3}{4}", data.nodeID, data.arc.name, data.isPreferred, data.score, "\n"); }
        }
        else { builder.AppendFormat(" No records{0}", "\n"); }
        //Known Targets
        builder.AppendFormat("{0}- listOfTargetsKnown{1}", "\n", "\n");
        if (listOfTargetsKnown.Count > 0)
        {
            foreach (AINodeData data in listOfTargetsKnown)
            { builder.AppendFormat(" ID {0}, {1}, isPreferred: {2}{3}", data.nodeID, data.arc.name, data.isPreferred, "\n"); }
        }
        else { builder.AppendFormat(" No records{0}", "\n"); }
        //Damaged Targets
        builder.AppendFormat("{0}- listOfTargetsDamaged{1}", "\n", "\n");
        if (listOfTargetsDamaged.Count > 0)
        {
            foreach (AINodeData data in listOfTargetsDamaged)
            { builder.AppendFormat(" ID {0}, {1}, isPreferred: {2}{3}", data.nodeID, data.arc.name, data.isPreferred, "\n"); }
        }
        else { builder.AppendFormat(" No records{0}", "\n"); }
        return builder.ToString();
    }

    /// <summary>
    /// Debug display of all relevant erasure AI related data
    /// </summary>
    /// <returns></returns>
    public String DisplayErasureData()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- listOfErasureTasks{0}", "\n");
        builder.Append(DebugTaskList(listOfErasureTasks));
        builder.AppendFormat("{0}- listOfErasureNodes{1}", "\n", "\n");
        if (listOfErasureNodes.Count > 0)
        {
            foreach (AINodeData data in listOfErasureNodes)
            { builder.AppendFormat(" ID {0}, {1}, isPreferred: {2}, Score: {3}{4}", data.nodeID, data.arc.name, data.isPreferred, data.score, "\n"); }
        }
        else { builder.AppendFormat(" No records{0}", "\n"); }
        Queue<AITracker> queueRecentNodes = GameManager.instance.dataScript.GetRecentNodesQueue();
        if (queueRecentNodes != null)
        {
            builder.AppendFormat("{0}- queueRecentNodes{1}", "\n", "\n");
            if (queueRecentNodes.Count > 0)
            {
                //you can enumerate the queue without distrubing it's contents
                foreach (AITracker data in queueRecentNodes)
                { builder.AppendFormat("nodeID {0}, turn {1}{2}", data.data0, data.turn, "\n"); }
            }
            else { builder.AppendFormat(" No records{0}", "\n"); }
        }
        else { Debug.LogWarning("Invalid queueRecentNodes (Null)"); }
        Queue<AITracker> queueRecentConnections = GameManager.instance.dataScript.GetRecentConnectionsQueue();
        if (queueRecentConnections != null)
        {
            builder.AppendFormat("{0}- queueRecentConnections{1}", "\n", "\n");
            if (queueRecentConnections.Count > 0)
            {
                //you can enumerate the queue without distrubing it's contents
                foreach (AITracker data in queueRecentConnections)
                { builder.AppendFormat("connID {0}, turn {1}{2}", data.data0, data.turn, "\n"); }
            }
            else { builder.AppendFormat(" No records{0}", "\n"); }
        }
        else { Debug.LogWarning("Invalid queueRecentConnections (Null)"); }
        builder.AppendFormat("{0}- listOfErasureAILog{1}", "\n", "\n");
        if (listOfErasureAILog.Count > 0)
        {
            foreach (string data in listOfErasureAILog)
            { builder.AppendFormat("{0}{1}", data, "\n"); }
        }
        else { builder.AppendFormat(" No records{0}", "\n"); }
        return builder.ToString();
    }

    /// <summary>
    /// debug submethod to display AI Node list data, used by AIManager.cs -> DisplayNodeData
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private string DebugDisplayList(List<AINodeData> list)
    {
        StringBuilder builderList = new StringBuilder();
        if (list != null)
        {
            if (list.Count > 0)
            {
                foreach (AINodeData data in list)
                { builderList.AppendFormat(" ID {0}, {1}, {2}, difference: {3}, current: {4}{5}", data.nodeID, data.arc.name, data.type, data.difference, data.current, "\n"); }
            }
            else { builderList.AppendFormat(" No records{0}", "\n"); }
        }
        else { Debug.LogWarning(string.Format("Invalid list \"{0}\" (Null)", list)); }
        return builderList.ToString();
    }

    /// <summary>
    /// debug submethod to display AI Tasks, used by AIManager.cs -> DisplayNodeData
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private string DebugTaskList(List<AITask> listOfTasks)
    {
        StringBuilder builderList = new StringBuilder();
        if (listOfTasks != null)
        {
            if (listOfTasks.Count > 0)
            {
                foreach (AITask task in listOfTasks)
                {
                    switch (task.type)
                    {
                        case AIType.Team:
                            builderList.AppendFormat(" ID {0} {1}, {2} team, {3} priority, Prob {4} %{5}", task.data0, task.name0, task.name1, task.priority,
                            task.chance, "\n");
                            break;
                        default:
                            builderList.AppendFormat(" Unknown task type \"{0}\"{1}", task.type, "\n");
                            break;
                    }
                }
            }
            else { builderList.AppendFormat("No records{0}", "\n"); }
        }
        else { builderList.AppendFormat("No records{0}", "\n"); }
        return builderList.ToString();
    }



    //new methods above here
}
