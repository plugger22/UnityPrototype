using gameAPI;
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

    [HideInInspector] public Nemesis nemesis;

    //Nemesis AI
    private NemesisMode mode;
    private NemesisGoal goal;
    private int duration;                   //if goal is fixed for a set time then no new goal can be assigned until the duration countdown has expired
    private int targetNodeID;               //if goal is 'MoveToNode' this is the node being moved towards
    private Node nemesisNode;               //current node where nemesis is, updated by ProcessNemesisActivity

    /// <summary>
    /// Initialise data ready for Nemesis
    /// </summary>
    public void Initialise()
    {
        //assign nemesis to a starting node
        int nemesisNodeID = -1;
        Node node = GameManager.instance.dataScript.GetRandomNode();
        if (node != null)
        { nemesisNodeID = node.nodeID; }
        else
        {
            //invalid node, switch to default nodeID '0'
            nemesisNodeID = 0;
            Debug.LogWarning("Invalid nemesis starting Node (Null), nemesis given default node '0'");
        }
        if (nemesisNodeID == GameManager.instance.nodeScript.nodePlayer)
        {
            //same node as Player, switch to default nodeID '1'
            nemesisNodeID = 1;
            Debug.LogWarning("Invalid nemesis starting Node (same as Player), nemesis given default node '1'");
        }
        //assign node
        GameManager.instance.nodeScript.nodeNemesis = nemesisNodeID;
        Node nodeTemp = GameManager.instance.dataScript.GetNode(nemesisNodeID);
        if (nodeTemp != null)
        {
            nemesisNode = nodeTemp;
            Debug.LogFormat("[Nem] NemesisManager.cs -> Initialise: Nemesis starts at node {0}, {1}, id {2}{3}", nodeTemp.nodeName, nodeTemp.Arc.name, nodeTemp.nodeID, "\n");
        }
        else { Debug.LogErrorFormat("Invalid nodeNemesis (Null) nodeID {0}", nemesisNodeID); }
        //Nemesis AI
        mode = NemesisMode.Normal;
        goal = NemesisGoal.Loiter;
        duration = 3; //nemesis does nothing for 'x' turns at game start
        //Set up datafor Nemesis
        SetLoiterNodes();
        SetLoiterData();
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
                            Debug.LogFormat("[Nem] NemesisManager.cs -> SetLoiterNodes: CENTRE -> there is a CENTRE nodeID {0}", centreNode.nodeID);
                            break;
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid node (Null) for listOfMostConnected[{0}]", index); }
                }
                if (centreNode == null)
                    { Debug.Log("[Nem] NemesisManager.cs -> SetLoiterNodes: CENTRE -> there is NO Centre node"); }
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
                Debug.LogFormat("[Nem] NemesisManager.cs -> SetLoiterNodes: TOP X -> there are {0} loiter nodes", listOfLoiterNodes.Count);

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
                            Debug.LogFormat("[Nem] NemesisManager.cs -> SetLoiterNodes: nodeID {0} removed (too close to Centre nodeID {1}) distance {2}", node.nodeID, centreNode.nodeID, distance);
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
                                    Debug.LogFormat("[Nem] NemesisManager.cs -> SetLoiterNodes: nodeID {0} removed (too close to nodeID {1}), distance {2}", node.nodeID, nodeTemp.nodeID, distance);
                                    listOfLoiterNodes.RemoveAt(index);
                                    break;
                                }
                                else
                                {
                                    Debug.Log("[Nem] NemesisManager.cs -> SetLoiterNodes: Last Node NOT Removed");
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

                    ProcessContinueWithGoal();
                }
            }
            else
            {
                // Continue with existing goal
                ProcessContinueWithGoal();
            }
        }
        else { Debug.LogErrorFormat("Invalid nemesisNode (Null) for nodeID {0}", nodeID); }
    }

    /// <summary>
    /// Nemesis proceeds with curent goal
    /// </summary>
    private void ProcessContinueWithGoal()
    {
        switch (goal)
        {
            case NemesisGoal.Ambush:

                break;
            case NemesisGoal.Search:

                break;
            case NemesisGoal.MoveToNode:

                break;
            case NemesisGoal.Loiter:
                //nemesis already at loiter node? -> if so IDLE, if not move towards loiter node at a speed of 1
                if (nemesisNode.isLoiterNode == true)
                { Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisActivity: goal {0}, At Loiter Node {1}, id {2}, IDLE{3}", goal, nemesisNode.nodeName, nemesisNode.nodeID, "\n"); }
                else
                { ProcessMoveNemesis(nemesisNode.loiter.neighbourID); }
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
                //update nodeManage
                GameManager.instance.nodeScript.nodeNemesis = nodeID;
                GameManager.instance.nodeScript.NodeRedraw = true;
                Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessMoveNemesis: Nemesis moves to node {0}, {1}, id {2}{3}", nemesisNode.nodeName, nemesisNode.Arc.name, nemesisNode.nodeID, "\n");
                //check for player at same node
                if (nemesisNode.nodeID == GameManager.instance.nodeScript.nodePlayer)
                { ProcessPlayerInteraction(); }
                //check for Resistance contact at same node
                List<int> tempList = GameManager.instance.dataScript.CheckContactResistanceAtNode(nodeID);
                if (tempList != null)
                { ProcessContactInteraction(tempList); }
            }
            else { Debug.LogWarningFormat("Invalid move node {Null) for nodeID {0}", nodeID); }
        }
        else {
            Debug.LogWarningFormat("Invalid move nodeId (Doesn't match any of neighbours) for nodeID {0} and nemesisNode {1}, {2}, id {3}{4}", nodeID, nemesisNode.nodeName, nemesisNode.Arc.name,
         nemesisNode.nodeID, "\n");
        }
    }

    /// <summary>
    /// nemesis and player at same node
    /// </summary>
    private void ProcessPlayerInteraction()
    {
        //player spotted if nemesis search rating >= player invisibility
        int searchRating = nemesis.searchRating;
        //adjust for mode
        if (mode == NemesisMode.Normal)
        { searchRating--; }
        if (searchRating >= GameManager.instance.playerScript.Invisibility)
        {
            //player SPOTTED
            Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessMoveNemesis: PLAYER SPOTTED at node {0}, {1}, id {2}{3}", nemesisNode.nodeName, nemesisNode.Arc.name, nemesisNode.nodeID, "\n");
        }
        else
        {
            //player NOT spotted
            Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessMoveNemesis: Player NOT Spotted at node {0}, {1}, id {2}{3}", nemesisNode.nodeName, nemesisNode.Arc.name, nemesisNode.nodeID, "\n");
        }
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
                //nemesis searchRating
                int stealthRating = nemesis.stealthRating;
                //adjust for mode
                if (mode == NemesisMode.Hunt)
                { stealthRating--; }
                //loop actors with contacts
                for (int i = 0; i < numOfActors; i++)
                {
                    actor = GameManager.instance.dataScript.GetActor(listOfActorsWithContactsAtNode[i]);
                    if (actor != null)
                    {
                        contact = actor.GetContact(nemesisNode.nodeID);
                        if (contact != null)
                        {
                            //check nemesis stealth rating vs. contact effectiveness
                            if (contact.effectiveness >= stealthRating)
                            {
                                //contact spots Nemesis
                                string text = string.Format("Nemesis {0} has been spotted by Contact {1} {2}, {3}, at node {4}, id {5}", nemesis.name, contact.nameFirst, contact.nameLast,
                                    contact.job, nemesisNode.nodeName, nemesisNode.nodeID);
                                Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessMoveNemesis: Contact {0}, effectiveness {1}, SPOTS Nemesis {2}, adj StealthRating {3} at nodeID {4}{5}",
                                    contact.nameFirst, contact.effectiveness, nemesis.name, stealthRating, nemesisNode.nodeID, "\n");
                                GameManager.instance.messageScript.ContactNemesisSpotted(text, actor, nemesisNode, contact, nemesis);
                                //no need to check anymore as one sighting is enough
                                break;
                            }
                            else
                            {
                                //contact Fails to spot Nemesis
                                Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessMoveNemesis: Contact {0}, effectiveness {1}, FAILS to spot Nemesis {2}, adj StealthRating {3} at nodeID {4}{5}", 
                                    contact.nameFirst, contact.effectiveness, nemesis.name, stealthRating, nemesisNode.nodeID, "\n");
                            }
                        }
                        else { Debug.LogFormat("Invalid contact (Null) for actor {0}, id {1} at node {2}, {3}, id {4}", actor.actorName, actor.actorID, nemesisNode.nodeName,
                         nemesisNode.Arc.name, nemesisNode.nodeID); }
                    }
                    else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", listOfActorsWithContactsAtNode[i]); }
                }
            }
            else { Debug.LogWarning("Invalid listOfActorsWithContactsAtNode (Empty)"); }
        }
        else { Debug.LogWarning("Invalid listOfActorsWithContactsAtNode (Null)"); }
    }

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
        //nemesis stats
        builder.AppendFormat(" {0}{1}{2}",  "\n", nemesis.name, "\n");
        builder.AppendFormat(" movement: {0}{1}", nemesis.movement, "\n");
        builder.AppendFormat(" search: {0}{1}", nemesis.searchRating, "\n");
        builder.AppendFormat(" stealth: {0}{1}", nemesis.stealthRating, "\n");
        builder.AppendFormat(" damage: {0}{1}", nemesis.damage.name, "\n");
        return builder.ToString();
    }

    //new methods above here
}
