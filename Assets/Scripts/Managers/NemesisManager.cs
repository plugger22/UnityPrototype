using gameAPI;
using modalAPI;
using packageAPI;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Handles all nemesis related matters
/// </summary>
public class NemesisManager : MonoBehaviour
{
    [Header("Loiter Nodes")]
    [Tooltip("How many nodes in listOfMostConnectedNodes to consider as potential loiter nodes.")]
    [Range(1, 10)] public int loiterNodePoolSize = 5;
    [Tooltip("If a possible loiter node is within this distance (Dijkstra unweighted), or less, of another loiter node, it is eliminated")]
    [Range(0, 3)] public int loiterDistanceCheck = 2;

    [Header("Logic")]
    [Tooltip("Chance of a nemesis switching from an Idle to a Loiter goal (provided not already present at LoiterNode)")]
    [Range(0, 100)] public int chanceIdleToLoiter = 50;
    [Tooltip("Chance of a nemesis switching from an Loiter to an Idle goal (provided not already present at LoiterNode)")]
    [Range(0, 100)] public int chanceLoiterToIdle = 15;
    [Tooltip("Chance of a nemesis switching from an Loiter to an Ambush goal (provided not already present at LoiterNode)")]
    [Range(0, 100)] public int chanceLoiterToAmbush = 20;
    [Tooltip("Duration of an Ambush")]
    [Range(1, 5)] public int durationAmbush = 2;

    [Tooltip("Number of turns Nemesis goes Offline after damaging the player (allows the player to clear the datum)")]
    [Range(0, 10)] public int durationDamageOffLine = 3;

    [Header("Spotted by Tracer")]
    [Tooltip("Chance of a nemesis with a HIGH adjusted stealth rating ('3+'), being spotted in any node with Tracer coverage")]
    [Range(0, 100)] public int chanceTracerSpotHigh = 10;
    [Tooltip("Chance of a nemesis with a MED adjusted stealth rating ('2'), being spotted in any node with Tracer coverage")]
    [Range(0, 100)] public int chanceTracerSpotMed = 25;
    [Tooltip("Chance of a nemesis with a LOW adjusted stealth rating ('1'), being spotted in any node with Tracer coverage")]
    [Range(0, 100)] public int chanceTracerSpotLow = 50;
    [Tooltip("Chance of a nemesis with a ZERO adjusted stealth rating ('0'), being spotted in any node with Tracer coverage")]
    [Range(0, 100)] public int chanceTracerSpotZero = 100;

    [HideInInspector] public Nemesis nemesis;

    private bool hasMoved;                  //flag set true if Nemesis has moved during AI phase, reset at start of next AI phase
    private bool hasActed;                  //flag set true if Nemesis has spotted player and caused Damage during the AI phase
    private bool hasWarning;                //flag set true if Nemesis at same node, hasn't spotted player, and a warning issued ("You sense a dark shadow..."). Stops a double warning

    //Nemesis AI
    private NemesisMode mode;
    private NemesisGoal goal;
    private int duration;                   //if goal is fixed for a set time then no new goal can be assigned until the duration countdown has expired
    private int targetNodeID;               //if goal is 'MoveToNode' this is the node being moved towards
    private Node nemesisNode;               //current node where nemesis is, updated by ProcessNemesisActivity

    //colour palette 
    private string colourGood;
    private string colourNeutral;
    private string colourGrey;
    private string colourBad;
    private string colourNormal;
    private string colourAlert;
    private string colourEnd;


    /// <summary>
    /// Initialise data ready for Nemesis
    /// </summary>
    public void Initialise()
    {
        //assign nemesis to a starting node
        int nemesisNodeID = -1;
        //Nemesis always starts at city Centre
        Node node = GameManager.instance.dataScript.GetNode(GameManager.instance.cityScript.cityHallDistrictID);
        if (node != null)
        {
            nemesisNodeID = node.nodeID;
            nemesisNode = node;
            Debug.LogFormat("[Nem] NemesisManager.cs -> Initialise: Nemesis starts at node {0}, {1}, id {2}{3}", node.nodeName, node.Arc.name, node.nodeID, "\n");
            //assign node
            GameManager.instance.nodeScript.nodeNemesis = nemesisNodeID;
        }
        else { Debug.LogError("Invalid node (Null)"); }
        //Nemesis AI
        int gracePeriod = GameManager.instance.scenarioScript.scenario.challenge.gracePeriod;
        if (gracePeriod > 0)
        {
            //grace period, start inactive
            mode = NemesisMode.Inactive;
            goal = NemesisGoal.Idle;
        }
        else
        {
            //NO grace period, start in normal mode, waiting for signs of player
            mode = NemesisMode.Normal;
            goal = NemesisGoal.Loiter;
        }
        duration = gracePeriod; //nemesis does nothing for 'x' turns at game start
        //Set up datafor Nemesis
        SetLoiterNodes();
        SetLoiterData();
        //event listeners
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "NemesisManager");
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
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// called by AIManager.cs -> AISideAuthority, handles all nemesis turn processing methods
    /// </summary>
    public void ProcessNemesis(int playerTargetNodeID, bool immediateFlagResistance)
    {
        ProcessNemesisAdminStart();
        CheckNemesisAtPlayerNode();
        CheckNemesisTracerSighting();
        ProcessNemesisActivity(playerTargetNodeID, immediateFlagResistance);
        ProcessNemesisAdminEnd();
    }

    /// <summary>
    /// Nemesis related matters that need to be reset, or taken care off, prior to nemesis turn processing
    /// </summary>
    private void ProcessNemesisAdminStart()
    {
        hasMoved = false;
        hasActed = false;
    }

    /// <summary>
    /// Nemesis related matters that need to be reset, or taken care off, at end of nemesis turn processing
    /// </summary>
    private void ProcessNemesisAdminEnd()
    {
        hasWarning = false;
    }

    /// <summary>
    /// Sets up a list (max 3) of nodes which are well-connected and, hopefully, centred, where the nemesis can sit and wait for developments
    /// </summary>
    private void SetLoiterNodes()
    {
        int numOfNodes, counter, distance;
        List<Node> listOfLoiterNodes = GameManager.instance.dataScript.GetListOfLoiterNodes();
        List<Node> listOfMostConnected = GameManager.instance.dataScript.GetListOfMostConnectedNodes();
        Node centreNode = null;
        if (listOfMostConnected != null)
        {
            if (listOfLoiterNodes != null)
            {
                numOfNodes = listOfMostConnected.Count;
                //loop through most Connected looking for the first instance of a centre, connected node (the most connected are checked first, least connected last)
                for (int index = 0; index < numOfNodes; index++)
                {
                    Node node = listOfMostConnected[index];
                    if (node != null)
                    {
                        if (node.isCentreNode == true)
                        {
                            //found the ideal node, job done
                            centreNode = node;
                            /*Debug.LogFormat("[Nem] NemesisManager.cs -> SetLoiterNodes: CENTRE -> there is a CENTRE nodeID {0}", centreNode.nodeID);*/
                            break;
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid node (Null) for listOfMostConnected[{0}]", index); }
                }
                /*if (centreNode == null)
                    { Debug.Log("[Nem] NemesisManager.cs -> SetLoiterNodes: CENTRE -> there is NO Centre node"); }*/

                //Take the top 'x' most connected nodes (excluding centreNode, if any) and add to loiterList
                counter = 0;
                for (int index = 0; index < numOfNodes; index++)
                {
                    Node node = listOfMostConnected[index];
                    //check not the centreNode
                    if (centreNode != null)
                    {
                        if (node.nodeID != centreNode.nodeID)
                        {
                            listOfLoiterNodes.Add(node);
                            counter++;
                        }
                    }
                    else
                    {
                        listOfLoiterNodes.Add(node);
                        counter++;
                    }
                    //check limit isn't exceeded
                    if (counter == loiterNodePoolSize)
                    { break; }
                }
               
                /*Debug.LogFormat("[Nem] NemesisManager.cs -> SetLoiterNodes: TOP X -> there are {0} loiter nodes", listOfLoiterNodes.Count);*/

                //Check all nodes in list (reverse loop list) to see if they have any neighbours within a set distance. Remove from list if so. Should be at least one node remaining.
                for (int index = listOfLoiterNodes.Count - 1; index >= 0; index--)
                {
                    Node node = listOfLoiterNodes[index];
                    //check against centre node, if any
                    if (centreNode != null)
                    {
                        distance = GameManager.instance.dijkstraScript.GetDistanceUnweighted(centreNode.nodeID, node.nodeID);
                        if (distance <= loiterDistanceCheck)
                        {
                            //too close, exclude node
                            
                            /*Debug.LogFormat("[Nem] NemesisManager.cs -> SetLoiterNodes: nodeID {0} removed (too close to Centre nodeID {1}) distance {2}", node.nodeID, centreNode.nodeID, distance);*/

                            listOfLoiterNodes.RemoveAt(index);
                            continue;
                        }
                    }
                }
                //Check remaining that loiter nodes aren't too close to each other (reverse loop list) Least connected nodes are checked and deleted before most connected (at list[0])
                for (int index = listOfLoiterNodes.Count - 1; index >= 0; index--)
                {
                    Node node = listOfLoiterNodes[index];
                    //check against all other nodes in list
                    for (int i = 0; i < listOfLoiterNodes.Count; i++)
                    {
                        Node nodeTemp = listOfLoiterNodes[i];
                        //not the same node?
                        if (nodeTemp.nodeID != node.nodeID)
                        {
                            //check distance
                            distance = GameManager.instance.dijkstraScript.GetDistanceUnweighted(nodeTemp.nodeID, node.nodeID);
                            if (distance <= loiterDistanceCheck)
                            {
                                //too close, remove current node from list (make sure at least one node is remaining)
                                counter = listOfLoiterNodes.Count;
                                if (centreNode != null)
                                { counter++; }
                                //only delete if more than one node remaining
                                if (counter > 1)
                                {
                                    /*Debug.LogFormat("[Nem] NemesisManager.cs -> SetLoiterNodes: nodeID {0} removed (too close to nodeID {1}), distance {2}", node.nodeID, nodeTemp.nodeID, distance);*/
                                    listOfLoiterNodes.RemoveAt(index);
                                    break;
                                }
                                else
                                {
                                    /*Debug.Log("[Nem] NemesisManager.cs -> SetLoiterNodes: Last Node NOT Removed");*/
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else { Debug.LogError("Invalid listOfLoiterNodes (Null)"); }
            //add centre node to list, if present
            if (centreNode != null)
            { listOfLoiterNodes.Add(centreNode); }
            //how many remaining
            Debug.LogFormat("[Nem] NemesisManager.cs -> SetLoiterNodes: FINAL -> there are {0} loiter nodes", listOfLoiterNodes.Count);
        }
        else { Debug.LogError("Invalid listOfMostConnectedNodes (Null)"); }
    }

    /// <summary>
    /// Takes loiter nodes and configueres a LoiterData package for each individual node for quick reference by nemesis
    /// </summary>
    private void SetLoiterData()
    {
        int tempNodeID, shortestNodeID, tempDistance, shortestDistance, numOfLoiterNodes, v1, v2;
        int counter = 0;
        List<Node> listOfLoiterNodes = GameManager.instance.dataScript.GetListOfLoiterNodes();
        List<Node> listOfAllNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        List<Connection> listOfConnections = new List<Connection>();
        Connection connection;
        if (listOfLoiterNodes != null)
        {
            numOfLoiterNodes = listOfLoiterNodes.Count;
            if (numOfLoiterNodes > 0)
            {
                if (listOfAllNodes != null)
                {
                    //loop all nodes
                    for (int index = 0; index < listOfAllNodes.Count; index++)
                    {
                        LoiterData data = new LoiterData();
                        Node node = listOfAllNodes[index];
                        if (node != null)
                        {
                            //check if node is a loiter node
                            if (listOfLoiterNodes.Exists(x => x.nodeID == node.nodeID) == true)
                            {
                                //Loiter node
                                data.nodeID = node.nodeID;
                                data.distance = 0;
                                data.neighbourID = node.nodeID;
                            }
                            else
                            {
                                //NOT a loiter node
                                shortestNodeID = -1;
                                shortestDistance = 999;
                                //check distance to all loiter nodes and get closest
                                for (int i = 0; i < numOfLoiterNodes; i++)
                                {
                                    tempNodeID = listOfLoiterNodes[i].nodeID;
                                    tempDistance = GameManager.instance.dijkstraScript.GetDistanceUnweighted(node.nodeID, tempNodeID);
                                    if (tempDistance < shortestDistance)
                                    {
                                        shortestDistance = tempDistance;
                                        shortestNodeID = tempNodeID;
                                    }
                                }
                                data.nodeID = shortestNodeID;
                                data.distance = shortestDistance;
                                //get path to nearest loiter node
                                if (shortestNodeID > -1)
                                {
                                    listOfConnections = GameManager.instance.dijkstraScript.GetPath(node.nodeID, shortestNodeID, false);
                                    if (listOfConnections != null)
                                    {
                                        //get first connection (from source node)
                                        connection = listOfConnections[0];
                                        if (connection != null)
                                        {
                                            v1 = connection.GetNode1();
                                            v2 = connection.GetNode2();
                                            if (v1 == node.nodeID || v2 == node.nodeID)
                                            {
                                                if (v1 == node.nodeID)
                                                { data.neighbourID = v2; }
                                                else { data.neighbourID = v1; }
                                                //check neighbourID is valid for node
                                                if (node.CheckNeighbourNodeID(data.neighbourID) == false)
                                                {
                                                    Debug.LogWarningFormat("Invalid data.neighbourID (doesn't correspond with node neighbours) for node {0}, {1}, id {2}", node.nodeName, node.Arc.name, node.nodeID);
                                                    data.neighbourID = -1;
                                                }
                                            }
                                            else { Debug.LogWarningFormat("Invalid connection (endpoints don't match nodeID) for node {0}, {1}, id {2}", node.nodeName, node.Arc.name, node.nodeID); }
                                        }
                                        else { Debug.LogWarningFormat("Invalid listOfConnections[0] (Null) for node {0}, {1}, id {2}", node.nodeName, node.Arc.name, node.nodeID); }
                                    }
                                    else { Debug.LogWarningFormat("Invalid listOfConnections (Null) for source nodeID {0} and destination nodeID {1}", node.nodeID, shortestNodeID); }
                                }
                                else { Debug.LogWarningFormat("Invalid shortestNodeID (-1) for node {0}, {1}, id {2}", node.nodeName, node.Arc.name, node.nodeID); }
                            }
                        }
                        else { Debug.LogErrorFormat("Invalid node (Null) in listOfAllNodes[{0}]", index); }
                        //store loiter data
                        if (data.distance > -1 && data.nodeID > -1 && data.neighbourID > -1)
                        {
                            node.loiter = data;
                            counter++;
                            if (data.distance == 0)
                            { node.isLoiterNode = true; }
                            else { node.isLoiterNode = false; }
                        }
                        else { Debug.LogWarningFormat("Invalid loiterData (missing data) for node {0}, {1}, id {2}", node.nodeName, node.Arc.name, node.nodeID); }
                    }
                    Debug.LogFormat("[Nem] NemesisManager.cs -> SetLoiterData: LoiterData successfully initialised in {0} nodes", counter);
                }
                else { Debug.LogError("Invalid listOfAllNodes (Null)"); }
            }
            else { Debug.LogError("Invalid listOfLoiterNodes (Empty)"); }
        }
        else { Debug.LogError("Invalid listOfLoiterNodes (Null)"); }
    }

    /// <summary>
    /// master AI for nemesis. AIManager.cs -> ProcessErasureTarget provides an update on recent player activity and gives a playerTargetNodeID, if worth doing so and -1 if nothing to report)
    /// </summary>
    /// <param name="playerTargetNodeID"></param>
    public void ProcessNemesisActivity(int playerTargetNodeID, bool immediateFlag)
    {
        int nodeID = GameManager.instance.nodeScript.nodeNemesis;
        nemesisNode = GameManager.instance.dataScript.GetNode(nodeID);
        if (nemesisNode != null)
        {
            bool isProceed = true;
            //player carrying out a set goal for a set period of time, keep doing so unless immediate flag set
            if (duration > 0)
            {
                if (immediateFlag == true) { isProceed = true; }
                else { isProceed = false; }
                //decrement duration
                duration--;
                Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisActivity: TargetNodeID {0}, ImmediateFlag {1}, duration {2}, isProceed {3}{4}", targetNodeID, immediateFlag, duration, isProceed, "\n");
            }
            //do nothing if inactive & duration > 0, swap to normal mode and proceed otherwise
            if (mode == NemesisMode.Inactive)
            {
                //message kicks in one turn early
                if (duration == 1)
                {
                    //message
                    string text = string.Format("{0} Nemesis comes online", nemesis.name);
                    string itemText = "Your NEMESIS comes Online";
                    string topText = "Nemesis goes ACTIVE";
                    string reason = string.Format("{0}<b>{1} Nemesis</b>", "\n", nemesis.name);
                    string warning = string.Format("{0}", nemesis.descriptor);
                    GameManager.instance.messageScript.GeneralWarning(text, itemText, topText, reason, warning, false);
                }
                else if (duration == 0)
                {
                    //change mode to Normal, goal to Loiter
                    mode = NemesisMode.Normal;
                    goal = NemesisGoal.Loiter;
                    isProceed = true;
                    Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisActivity: Nemesis Mode changed from INACTIVE to NORMAL{0}", "\n");
                }
                else
                {
                    isProceed = false;
                    goal = NemesisGoal.Idle;
                }
            }
            //
            // - - - Proceed - - -
            //
            if (isProceed == true)
            {
                //
                // - - - Possible new goal
                //
                if (playerTargetNodeID > -1)
                {
                    //recent player activity
                }
                else
                {
                    //no recent player activity

                    //DEBUG -> Chance to change goals

                    ProcessNemesisGoal();
                }
            }
            else
            {
                if (mode != NemesisMode.Inactive)
                {
                    // Continue with existing goal
                    ProcessNemesisGoal();
                }
            }
        }
        else { Debug.LogErrorFormat("Invalid nemesisNode (Null) for nodeID {0}", nodeID); }
    }

    /// <summary>
    /// Nemesis proceeds with curent goal
    /// </summary>
    private void ProcessNemesisGoal()
    {
        switch (goal)
        {
            case NemesisGoal.Ambush:
                if (duration > 0)
                { Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisContinueWithGoal: Nemesis continues with AMBUSH, duration {0},  nodeID {1}{2}", duration, nemesisNode.nodeID, "\n"); }
                else
                {
                    //change goal
                    if (mode == NemesisMode.Normal)
                    {
                        goal = NemesisGoal.Loiter;
                        Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisContinueWithGoal: Nemesis changes goal from AMBUSH to LOITER, current nodeID {0}{1}", nemesisNode.nodeID, "\n");
                    }
                    else if (mode == NemesisMode.Hunt)
                    {
                        goal = NemesisGoal.Search;
                        Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisContinueWithGoal: Nemesis changes goal from AMBUSH to SEARCH, current nodeID {0}{1}", nemesisNode.nodeID, "\n");
                    }
                }
                break;
            case NemesisGoal.Search:

                break;
            case NemesisGoal.MoveToNode:

                break;
            case NemesisGoal.Idle:
                    //chance to switch to Loiter goal
                    if (Random.Range(0, 100) < chanceIdleToLoiter)
                    {
                        goal = NemesisGoal.Loiter;
                        Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisContinueWithGoal: Nemesis changes goal from IDLE to LOITER, current nodeID {0}{1}", nemesisNode.nodeID, "\n");
                    }
                    else { Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisContinueWithGoal: Nemesis retains IDLE goal, current nodeID {0}{1}", nemesisNode.nodeID, "\n"); }
                break;
            case NemesisGoal.Loiter:
                //nemesis already at loiter node?
                if (nemesisNode.isLoiterNode == true)
                {
                    //at loiter node, chance to switch to Ambush
                    if (Random.Range(0, 100) < chanceLoiterToAmbush)
                    {
                        goal = NemesisGoal.Ambush;
                        duration = durationAmbush;
                        Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisContinueWithGoal: Nemesis changes goal from LOITER to AMBUSH, current nodeID {0}{1}", nemesisNode.nodeID, "\n");
                    }
                    else
                    {
                        //at LoiterNode
                        Debug.LogFormat("[Nem] NemesisManager.cs -> ContinueWithGoal: At Loiter Node {1}, id {2}, do NOTHING{3}", goal, nemesisNode.nodeName, nemesisNode.nodeID, "\n");
                    }
                }
                //not yet at LoiterNode, move towards nearest at speed 1
                else
                {
                    //chance to switch to IDLE
                    if (Random.Range(0, 100) < chanceLoiterToIdle)
                    {
                        goal = NemesisGoal.Idle;
                        Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisContinueWithGoal: Nemesis changes goal from LOITER to IDLE, current nodeID {0}{1}", nemesisNode.nodeID, "\n");
                    }
                    //chance to switch to AMBUSH
                    else if (Random.Range(0, 100) < chanceLoiterToAmbush)
                    {
                        goal = NemesisGoal.Ambush;
                        duration = durationAmbush;
                        Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisContinueWithGoal: Nemesis changes goal from LOITER to AMBUSH, current nodeID {0}{1}", nemesisNode.nodeID, "\n");
                    }
                    //Continue with Loiter mode
                    else
                    {
                        ProcessMoveNemesis(nemesisNode.loiter.neighbourID);
                        Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisContinueWithGoal: Nemesis continues with LOITER, current nodeID {0}{1}", nemesisNode.nodeID, "\n");
                    }
                }
                break;
            default:
                Debug.LogWarningFormat("Invalid NemesisGoal \"{0}\"", goal);
                break;
        }
    }

    /// <summary>
    /// method to move nemesis. Handles admin and player and contact interaction checks. 
    /// NOTE: Assumed to be a single link move.
    /// </summary>
    /// <param name="nodeID"></param>
    private void ProcessMoveNemesis(int nodeID)
    {
        //check if node is a neighbour of current nemesis node (assumed to be a single link move)
        if (nemesisNode.CheckNeighbourNodeID(nodeID) == true)
        {
            //update nemesisManager
            nemesisNode = GameManager.instance.dataScript.GetNode(nodeID);
            if (nemesisNode != null)
            {
                //update nodeManager
                GameManager.instance.nodeScript.nodeNemesis = nodeID;
                GameManager.instance.nodeScript.NodeRedraw = true;
                hasMoved = true;
                Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessMoveNemesis: Nemesis moves to node {0}, {1}, id {2}{3}", nemesisNode.nodeName, nemesisNode.Arc.name, nemesisNode.nodeID, "\n");
                //check for player at same node
                if (nemesisNode.nodeID == GameManager.instance.nodeScript.nodePlayer)
                { ProcessPlayerInteraction(false); }
                //check for Resistance contact at same node
                List<int> tempList = GameManager.instance.dataScript.CheckContactResistanceAtNode(nodeID);
                if (tempList != null)
                { ProcessContactInteraction(tempList); }
                //check for Tracer Sighting
                CheckNemesisTracerSighting();
            }
            else { Debug.LogWarningFormat("Invalid move node {Null) for nodeID {0}", nodeID); }
        }
        else {
            Debug.LogWarningFormat("Invalid move nodeId (Doesn't match any of neighbours) for nodeID {0} and nemesisNode {1}, {2}, id {3}{4}", nodeID, nemesisNode.nodeName, nemesisNode.Arc.name,
         nemesisNode.nodeID, "\n");
        }
    }

    /// <summary>
    /// nemesis and player at same node. For end of turn checks set 'isPlayerMove' to false as this tweaks modal setting of outcome window to handle MainInfoApp, leave as default true otherwise
    /// </summary>
    private void ProcessPlayerInteraction(bool isPlayerMove = true)
    {
        //player spotted if nemesis search rating >= player invisibility
        int searchRating = GetSearchRatingAdjusted();
        if (searchRating >= GameManager.instance.playerScript.Invisibility)
        {
            //player SPOTTED
            Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessPlayerInteraction: PLAYER SPOTTED at node {0}, {1}, id {2}{3}", nemesisNode.nodeName, nemesisNode.Arc.name, nemesisNode.nodeID, "\n");
            //cause damage / message
            ProcessPlayerDamage(isPlayerMove);
            //place Nemesis OFFLINE for a period
            mode = NemesisMode.Inactive;
            goal = NemesisGoal.Idle;
            duration = durationDamageOffLine;
            if (duration > 0)
            {
                string text = string.Format("Nemesis goes Offline for {0} turns after Damaging player{1}", duration, "\n");
                string itemText = "NEMESIS goes OFFLINE for a short while";
                string topText = "Nemesis OFFLINE";
                string reason = string.Format("<b>{0} Nemesis</b>", nemesis.name);
                string warning = "Rebel HQ STRONGLY ADVISE that you get the h*ll out of there!";
                GameManager.instance.messageScript.GeneralWarning(text, itemText, topText, reason, warning, false);
            }
            //set flag to prevent nemesis acting immediately again at start of player's turn (gives them one turn's grace to get out of dodge)
            hasActed = true;
        }
        else
        {
            //player NOT spotted
            Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessPlayerInteraction: Player NOT Spotted at node {0}, {1}, id {2}{3}", nemesisNode.nodeName, nemesisNode.Arc.name, nemesisNode.nodeID, "\n");
            //warn player (only if resistance side)
            if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
            {
                //prevent a double warning if player moves into a node with nemesis and nemesis is stationary
                if (hasWarning == false)
                {
                    Node node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
                    if (node != null)
                    {
                        hasWarning = true;
                        string text = string.Format("You feel the presence of a <b>DARK SHADOW</b> at {0}, {1} district", node.nodeName, node.Arc.name);
                        string itemText = "You feel the presence of a DARK SHADOW";
                        string topText = "Something is WRONG";
                        string reason = "Could it be that your NEMESIS is nearby?";
                        string warning = "Your instincts urge you to move, NOW";
                        GameManager.instance.messageScript.GeneralWarning(text, itemText, topText, reason, warning, true, true);
                    }
                    else { Debug.LogWarning("Invalid nodePlayer (Null)"); }
                }
            }
        }
    }

    /// <summary>
    /// returns Nemesis Search rating after adjusting for mode and activiy
    /// </summary>
    /// <returns></returns>
    private int GetSearchRatingAdjusted()
    {
        int searchRating = nemesis.searchRating;
        //adjust for mode
        if (goal == NemesisGoal.Search)
        { searchRating++; }
        return searchRating;
    }

    /// <summary>
    /// returns Nemesis Stealth rating after adjusting for mode and activiy
    /// </summary>
    /// <returns></returns>
    private int GetStealthRatingAdjusted()
    {
        int stealthRating = nemesis.stealthRating;
        //adjust for mode
        if (mode == NemesisMode.Hunt)
        { stealthRating--; }
        //adjust for goal
        if (goal == NemesisGoal.Ambush)
        { stealthRating += 999; }
        return stealthRating;
    }

    /// <summary>
    /// nemesis at same node as one or more resistance contacts
    /// </summary>
    /// <param name="listOfActorsWithContactsAtNode"></param>
    private void ProcessContactInteraction(List<int> listOfActorsWithContactsAtNode)
    {
        Actor actor;
        Contact contact;
        if (listOfActorsWithContactsAtNode != null)
        {
            int numOfActors = listOfActorsWithContactsAtNode.Count;
            if (numOfActors > 0)
            {
                //nemesis stealthRating
                int stealthRating = GetStealthRatingAdjusted();
                //loop actors with contacts
                for (int i = 0; i < numOfActors; i++)
                {
                    actor = GameManager.instance.dataScript.GetActor(listOfActorsWithContactsAtNode[i]);
                    if (actor != null)
                    {
                        //only active actors can work their contact network
                        if (actor.Status == ActorStatus.Active)
                        {
                            contact = actor.GetContact(nemesisNode.nodeID);
                            if (contact != null)
                            {
                                //check nemesis stealth rating vs. contact effectiveness
                                if (contact.effectiveness >= stealthRating)
                                {
                                    //check contact reliabiity -> if not use a random neighbouring node
                                    Node node = nemesisNode;
                                    if (GameManager.instance.contactScript.CheckContactIsReliable(contact) == false)
                                    { node = nemesisNode.GetRandomNeighbour(); }
                                    //contact spots Nemesis
                                    string text = string.Format("Nemesis {0} has been spotted by Contact {1} {2}, {3}, at node {4}, id {5}", nemesis.name, contact.nameFirst, contact.nameLast,
                                        contact.job, node.nodeName, node.nodeID);
                                    Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessContactInteraction: Contact {0}, effectiveness {1}, SPOTS Nemesis {2}, adj StealthRating {3} at node {4}, id {5}{6}",
                                        contact.nameFirst, contact.effectiveness, nemesis.name, stealthRating, node.nodeName, node.nodeID, "\n");
                                    GameManager.instance.messageScript.ContactNemesisSpotted(text, actor, node, contact, nemesis);
                                    //no need to check anymore as one sighting is enough
                                    break;
                                }
                                else
                                {
                                    //contact Fails to spot Nemesis
                                    Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessContactInteraction: Contact {0}, effectiveness {1}, FAILS to spot Nemesis {2}, adj StealthRating {3} at nodeID {4}{5}",
                                        contact.nameFirst, contact.effectiveness, nemesis.name, stealthRating, nemesisNode.nodeID, "\n");
                                }
                            }
                            else
                            { Debug.LogFormat("Invalid contact (Null) for actor {0}, id {1} at node {2}, {3}, id {4}", actor.actorName, actor.actorID, nemesisNode.nodeName,
                                    nemesisNode.Arc.name, nemesisNode.nodeID); }
                        }
                        else
                        {  Debug.LogFormat("[Con] NemesisManager.cs -> ProcessContactInteraction: Actor {0}, {1}, id {2}, is INACTIVE and can't access their contacts{3}", actor.actorName,
                                actor.arc.name, actor.actorID, "\n"); }
                    }
                    else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", listOfActorsWithContactsAtNode[i]); }
                }
            }
            else { Debug.LogWarning("Invalid listOfActorsWithContactsAtNode (Empty)"); }
        }
        else { Debug.LogWarning("Invalid listOfActorsWithContactsAtNode (Null)"); }
    }

    /// <summary>
    /// Check if nemesis spotted by Contacts at start of a turn where the nemesis hasn't moved
    /// </summary>
    public void CheckNemesisContactSighting()
    {
        //ignored if nemesis inactive
        if (mode != NemesisMode.Inactive)
        {
            if (hasMoved == false)
            {
                int nodeID = GameManager.instance.nodeScript.nodeNemesis;
                if (nodeID > -1)
                {
                    //check for Resistance contact at same node
                    List<int> tempList = GameManager.instance.dataScript.CheckContactResistanceAtNode(nodeID);
                    if (tempList != null)
                    {ProcessContactInteraction(tempList); }
                }
                else { Debug.LogWarning("Invalid nodeNemesis (-1)"); }
            }
        }
    }

    /// <summary>
    /// check if nemesis spotted by Tracers that are covering the node they are currently in. Will run regardless of 'hasWarning' (additional info from a secondary source)
    /// </summary>
    public void CheckNemesisTracerSighting()
    {
        if (nemesisNode.isTracerActive == true)
        {
            bool isSpotted = false;
            //nemesis stealthRating
            int stealthRating = GetStealthRatingAdjusted();
            stealthRating = Mathf.Max(stealthRating, 0);
            int rndNum = Random.Range(0, 100);
            int needNum = -1;
            switch (stealthRating)
            {
                case 3: needNum = chanceTracerSpotHigh; break;
                case 2: needNum = chanceTracerSpotMed;  break;
                case 1: needNum = chanceTracerSpotLow;  break;
                case 0: needNum = chanceTracerSpotZero; break;
                default: needNum = 999; break;
            }
            //random check
            if (rndNum <= needNum)
            { isSpotted = true; }
            //Spotted
            if (isSpotted == true)
            {
                Debug.LogFormat("[Rnd] NemesisManager.cs -> CheckNemesisTracerSighting: Tracer SUCCEEDS, need < {0} rolled {1}{2}", needNum, rndNum, "\n");
                Debug.LogFormat("[Nem] NemesisManager.cs -> CheckNemesisTracerSighting: Tracer spots Nemesis at {0}, {1}, id {2}{3}", nemesisNode.nodeName, nemesisNode.Arc.name, nemesisNode.nodeID, "\n");
                GameManager.instance.messageScript.GeneralRandom("Tracer spots Nemesis", "Tracer Sighting", needNum, rndNum);
                //SPOTTED -> node is always correct
                string text = string.Format("Tracer picks up an Anomalous reading at {0}, {1} district", nemesisNode.nodeName, nemesisNode.Arc.name);
                GameManager.instance.messageScript.TracerNemesisSpotted(text, nemesisNode);
            }
            else { Debug.LogFormat("[Rnd] NemesisManager.cs -> CheckNemesisTracerSighting: Tracer FAILED to spot, need < {0} rolled {1}{2}", needNum, rndNum, "\n"); }
        }
    }

    /// <summary>
    /// check if nemesis is at the same node as the player. Used when nemesis hasn't moved (start of turn, 'isPlayerMove' false) and when player moving to a new node ('isPlayerMove' true)
    /// if nemesis stationary at start of turn there is a further check to prevent nemesis damaging the player twice in a row if they have already done so during the AI turn
    /// </summary>
    public void CheckNemesisAtPlayerNode(bool isPlayerMove = false)
    {
        //ignored if nemesis inactive
        if (mode != NemesisMode.Inactive)
        {
            if (GameManager.instance.playerScript.status == ActorStatus.Active)
            {
                bool isCheckNeeded = false;
                if (isPlayerMove == false)
                {
                    //start of turn (player hasn't moved) and nemesis hasn't moved
                    if (hasMoved == false)
                    {
                        //can only check if nemesis didn't damage the player during the AI turn (gives the player a chance to leave)
                        if (hasActed == false)
                        { isCheckNeeded = true; }
                    }
                }
                //player moving to a new node, automatic check
                else { isCheckNeeded = true; }
                //proceed with a check
                if (isCheckNeeded == true)
                {
                    //both at same node
                    if (nemesisNode.nodeID == GameManager.instance.nodeScript.nodePlayer)
                    { ProcessPlayerInteraction(isPlayerMove); }
                }
            }
        }
    }

    /// <summary>
    /// called whenever Nemesis spots and catches player. Both assumed to be at the same node. 'isModalOutcomeNormal' set false (auto) only if end of turn check and tweaks outcome window modal setting
    /// </summary>
    private void ProcessPlayerDamage(bool isOutcomeModalNormal)
    {
        StringBuilder builder = new StringBuilder();
        Damage damage = nemesis.damage;
        if (damage != null)
        {
            switch (damage.name)
            {
                case "Capture":

                    break;
                case "Discredit":

                    break;
                case "Image":

                    break;
                case "Kill":

                    break;
                case "Tag":

                    break;
                case "Wound":

                    break;
                default:
                    builder.AppendFormat("Damage is of an unknown kind");
                    Debug.LogWarningFormat("Invalid damage \"{0}\"", damage.name);
                    break;
            }
            string text = string.Format("Player has been found and targeted by their{0}{1}<b>{2} Nemesis</b>{3}", "\n", colourNeutral, nemesis.name, colourEnd);
            builder.AppendFormat("at {0}, {1} district{2}{3}", nemesisNode.nodeName, nemesisNode.Arc.name, "\n", "\n");
            builder.AppendFormat("{0}<b>Player {1}</b>{2}", colourBad, nemesis.damage.tag, colourEnd);
            builder.AppendFormat("{0}{1}{2}<b>{3}</b>{4}", "\n", "\n", colourAlert, nemesis.damage.effect, colourEnd);
            //Message
            string msgText = string.Format("Player has been {0} by their {1} Nemesis", nemesis.damage.tag, nemesis.name);
            GameManager.instance.messageScript.PlayerDamage(msgText, nemesis.damage.tag, nemesis.damage.effect, nemesisNode.nodeID);
            //player damaged outcome window
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
            {
                textTop = text,
                textBottom = builder.ToString(),
                sprite = GameManager.instance.guiScript.aiAlertSprite,
                isAction = false,
                side = GameManager.instance.globalScript.sideResistance
            };
            //end of turn outcome window which needs to overlay ontop of InfoAPP and requires a different than normal modal setting
            if (isOutcomeModalNormal == false)
            {
                outcomeDetails.modalLevel = 2;
                outcomeDetails.modalState = ModalState.InfoDisplay;
            }
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "NemesisManager.cs -> ProcessPlayerDamage");
        }
        else { Debug.LogWarning("Invalid damage (Null)"); }
    }


    //
    // - - - Debug - - -
    //

    /// <summary>
    /// Debug method to display nemesis status
    /// </summary>
    /// <returns></returns>
    public string DebugShowNemesisStatus()
    {
        StringBuilder builder = new StringBuilder();
        //current status
        builder.AppendFormat(" Nemesis Status{0}{1}", "\n", "\n");
        builder.AppendFormat(" mode: {0}{1}", mode, "\n");
        builder.AppendFormat(" goal: {0}{1}", goal, "\n");
        builder.AppendFormat(" duration: {0}{1}", duration, "\n");
        builder.AppendFormat(" targetNodeID: {0}{1}", targetNodeID, "\n");
        builder.AppendFormat(" nemesis node: {0}, {1}, id {2}{3}", nemesisNode.nodeName, nemesisNode.Arc.name, nemesisNode.nodeID, "\n");
        builder.AppendFormat(" hasMoved: {0}{1}", hasMoved, "\n");
        builder.AppendFormat(" hasActed: {0}{1}", hasActed, "\n");
        builder.AppendFormat(" hasWarning: {0}{1}", hasWarning, "\n");
        //nemesis stats
        builder.AppendFormat("{0} {1}{2}",  "\n", nemesis.name, "\n");
        builder.AppendFormat(" movement: {0}{1}", nemesis.movement, "\n");
        builder.AppendFormat(" search: {0}, adjusted: {1}{2}", nemesis.searchRating, GetSearchRatingAdjusted(), "\n");
        builder.AppendFormat(" stealth: {0}, adjusted: {1}{2}", nemesis.stealthRating, GetStealthRatingAdjusted(), "\n");
        builder.AppendFormat(" damage: {0}{1}", nemesis.damage.name, "\n");
        return builder.ToString();
    }

    //new methods above here
}
