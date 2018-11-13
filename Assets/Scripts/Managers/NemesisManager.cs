using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all nemesis related matters
/// </summary>
public class NemesisManager : MonoBehaviour
{

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
                            break;
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid node (Null) for listOfMostConnected[{0}]", index); }
                }
                //Take the top 3 most connected nodes (excluding centreNode, if any) and add to loiterList
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
                        if (counter == 3)
                        { break; }
                    }
                }
                //Check all nodes in list (reverse loop) to see if they have any neighbours within a set distance. Remove from list if so.
                for (int index = listOfLoiterNodes.Count -1; index >= 0; index--)
                {
                    Node node = listOfLoiterNodes[index];
                    //check against centre node, if any
                    if (centreNode != null)
                    {
                        distance = GameManager.instance.dijkstraScript.GetDistanceUnweighted(centreNode.nodeID, node.nodeID);
                        if (distance <= 2)
                        {
                            //too close, exclude node
                            listOfLoiterNodes.RemoveAt(index);
                        }
                    }
                }
            }
            else { Debug.LogError("Invalid listOfLoiterNodes (Null)"); }
        }
        else { Debug.LogError("Invalid listOfMostConnectedNodes (Null)"); }
    }

}
