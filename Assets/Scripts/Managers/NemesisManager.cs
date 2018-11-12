using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all nemesis related matters
/// </summary>
public class NemesisManager : MonoBehaviour
{


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
    }


}
