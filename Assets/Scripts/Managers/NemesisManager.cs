using packageAPI;
using System.Collections.Generic;
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
        { Debug.LogFormat("[Nem] NemesisManager.cs -> Initialise: Nemesis starts at node {0}, {1}, id {2}{3}", nodeTemp.nodeName, nodeTemp.Arc.name, nodeTemp.nodeID, "\n"); }
        else { Debug.LogErrorFormat("Invalid nodeNemesis (Null) nodeID {0}", nemesisNodeID); }
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
    public void ProcessNemesisActivity(int playerTargetNodeID)
    {
        if (playerTargetNodeID > -1)
        {
            //recent player activity
        }
        else
        {
            //no recent player activity
        }
    }

    //new methods above here
}
