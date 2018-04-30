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
    [Range(1, 10)] public int priorityHighWeight = 3;
    [Tooltip("When selecting Non-Critical tasks where there are an excess to available choices how much relative weight do I assign to Medium Priority tasks")]
    [Range(1, 10)] public int priorityMediumWeight = 2;
    [Tooltip("When selecting Non-Critical tasks where there are an excess to available choices how much relative weight do I assign to Low Priority tasks")]
    [Range(1, 10)] public int priorityLowWeight = 1;
    [Tooltip("The listOfMostConnectedNodes includes all nodes with this number, or greater, connections. If < 3 nodes then the next set down are added. Max capped at total nodes/2")]
    [Range(0, 5)] public int nodeConnectionFactor = 3;
    [Tooltip("Spider Team node score -> Each resistance activity point is multiplied by this factor")]
    [Range(0, 5)] public int nodeActivityFactor = 1;
    [Tooltip("Spider Team node score -> If node is preferred type of current level authority faction this is added to the score")]
    [Range(0, 5)] public int nodePreferredFactor = 3;
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

    //tasks
    List<AITask> listOfTasksPotential = new List<AITask>();
    List<AITask> listOfTasksFinal = new List<AITask>();


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
        Debug.Assert(teamArcCivil > -1, "Invalid teamArcCivil");
        Debug.Assert(teamArcControl > -1, "Invalid teamArcControl");
        Debug.Assert(teamArcMedia > -1, "Invalid teamArcMedia");
        Debug.Assert(teamArcProbe > -1, "Invalid teamArcProbe");
        Debug.Assert(teamArcSpider > -1, "Invalid teamArcSpider");
        maxTeamsAtNode = GameManager.instance.teamScript.maxTeamsAtNode;
        Debug.Assert(maxTeamsAtNode > -1, "Invalid maxTeamsAtNode");
        //set up list of most connected Nodes
        SetConnectedNodes();
        SetPreferredNodes();
        SetCentreNodes();
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
        //AI Rulesets
        ProcessNodeTasks();
        ProcessProbeTask();
        ProcessSpiderTask();
        //choose tasks for the turn
        ProcessTasksFinal(authorityMaxTasksPerTurn);
    }

    /// <summary>
    /// run prior to any info gathering each turn to empty out all data collections
    /// </summary>
    private void ClearAICollections()
    {
        listOfTasksFinal.Clear();
        listOfTasksPotential.Clear();
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
    }

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
                    if (numOfConnections >= nodeConnectionFactor)
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
                int numSpecific = nodeConnectionFactor - 1;
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
                        { connection.AddActivityData(message.turnCreated); }
                        else { Debug.LogWarning(string.Format("Invalid connection (Null) for connID {0} -> AI data NOT extracted", message.data1)); }
                        break;
                    case MessageSubType.AI_Node:
                        //Get Node and add Activity data
                        Node node = GameManager.instance.dataScript.GetNode(message.data0);
                        if (node != null)
                        { node.AddActivityData(message.turnCreated); }
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
            foreach(var node in dictOfNodes)
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
                                    //Damage team node data package (good ambush situation as targets will lure in resistance player)
                                    dataPackage = new AINodeData();
                                    dataPackage.nodeID = node.Value.nodeID;
                                    dataPackage.type = NodeData.Target;
                                    dataPackage.arc = node.Value.Arc;
                                    listOfTargetsDamaged.Add(dataPackage);
                                    dataPackage.isPreferred = node.Value.isPreferredAuthority;
                                    break;
                            }
                        }
                        else { Debug.LogWarning(string.Format("Invalid target (Null) for targetID {0}", node.Value.targetID)); }
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
                int score;
                foreach (Node node in listOfMostConnected)
                {
                    //ignore nodes with existing spider teams
                    if (node.isSpiderTeam == false)
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
                        { score += node.activityCount * nodeActivityFactor; }
                        //connections
                        List<Connection> listOfConnections = node.GetAllConnections();
                        if (listOfConnections != null)
                        {
                            foreach(Connection conn in listOfConnections)
                            {
                                switch(conn.SecurityLevel)
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
            else { Debug.LogError("Invalid listOfMostConnected (Null)"); }
        }
        else { Debug.Log(string.Format("AIManager.cs -> ProcessSpiderData: No Spider teams available in reserves{0}", "\n")); }
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
        else { Debug.Log(string.Format("AIManager.cs -> ProcessNodeTasks: No Reserve teams available in reserves{0}", "\n")); }

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
            else { Debug.LogWarning("No nodes in listOfProbeNodes (Count 0)"); }
        }
        else { Debug.Log(string.Format("AIManager.cs -> ProcessProbeTasks: No Probe teams available in reserves{0}", "\n")); }
    }

    /// <summary>
    /// selects a spider team if a one is available
    /// </summary>
    private void ProcessSpiderTask()
    {
        List<AITask> poolOfTasks = new List<AITask>();
        if (listOfSpiderNodes.Count > 0)
        {
            int counter = 0;
            //sort list by score (highest at top)
            var sorted = from record in listOfSpiderNodes orderby record.score descending select record;
            //take top three records and create a task
            foreach(var record in listOfSpiderNodes)
            {
                counter++;
                AITask task = new AITask()
                {
                    data0 = record.nodeID,
                    data1 = teamArcSpider,
                    name0 = record.arc.name,
                    name1 = "SPIDER",
                    type = AIType.Team,
                    priority = Priority.Low
                };
                switch(counter)
                {
                    case 1:
                        //highest ranked score -> add three entries to pool
                        poolOfTasks.Add(task); poolOfTasks.Add(task); poolOfTasks.Add(task);
                        break;
                    case 2:
                        //second highest ranked score -> add two entries to pool
                        poolOfTasks.Add(task); poolOfTasks.Add(task);
                        break;
                    case 3:
                        //third highest ranked score -> add one entry to pool
                        poolOfTasks.Add(task);
                        break;
                }
                if (counter == 3) { break; }
            }
        }

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
        else { Debug.LogWarning("AIManager.cs -> ProcessTasksFinal: No tasks this turn"); }
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
                    Node node = GameManager.instance.dataScript.GetNode(task.data0);
                    if (node != null)
                    { GameManager.instance.teamScript.MoveTeamAI(TeamPool.OnMap, dataID, node); }
                    else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", task.data0)); }
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
    /// Debug display method for AI relevant data
    /// </summary>
    /// <returns></returns>
    public string DisplayNodeData()
    {
        StringBuilder builder = new StringBuilder();
        //Task lists
        builder.AppendFormat("- listOfTasksFinal{0}", "\n");
        builder.Append(DebugTaskList(listOfTasksFinal));
        builder.AppendFormat("{0}- listOfTasksPotential{1}", "\n", "\n");
        builder.Append(DebugTaskList(listOfTasksPotential));
        //Master Node list
        builder.AppendFormat("{0}- listNodeMaster{1}", "\n", "\n");
        builder.Append(DebugDisplayList(listNodeMaster));
        //Sub Node lists
        builder.AppendFormat("{0}- listStabilityCritical{1}", "\n", "\n");
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
                { builderList.AppendFormat("ID {0}, {1}, {2}, difference: {3}, current: {4}{5}", data.nodeID, data.arc.name, data.type, data.difference, data.current, "\n"); }
            }
            else { builderList.AppendFormat("No records{0}", "\n"); }
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
                            builderList.AppendFormat("ID {0} {1}, {2} team, {3} priority, Prob {4} %{5}", task.data0, task.name0, task.name1, task.priority,
                            task.chance, "\n");
                            break;
                        default:
                            builderList.AppendFormat("Unknown task type \"{0}\"{1}", task.type, "\n");
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
