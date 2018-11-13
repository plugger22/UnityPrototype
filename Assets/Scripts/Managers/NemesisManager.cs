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

    //new methods above here
}
