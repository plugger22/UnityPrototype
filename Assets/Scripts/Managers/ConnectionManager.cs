using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;


/// <summary>
/// Handles all connection related matters
/// </summary>
public class ConnectionManager : MonoBehaviour
{
    [HideInInspector] public bool resetNeeded;                      //used for temp changes in connection states, if true run RestoreConnections
    public Material[] arrayOfConnectionTypes;

    /// <summary>
    /// returns a connection material type based on security level
    /// </summary>
    /// <param name="securityLevel"></param>
    /// <returns></returns>
    public Material GetConnectionMaterial(ConnectionType securityLevel)
    { return arrayOfConnectionTypes[(int)securityLevel]; }

    /// <summary>
    /// sets all connection.isDone flags to false to prevent double dipping of certain effects.
    /// </summary>
    public void SetAllFlagsToFalse()
    {
        Dictionary<int, Connection> dictOfConnections = GameManager.instance.dataScript.GetAllConnections();
        if (dictOfConnections != null)
        {
            foreach(var connection in dictOfConnections)
            { connection.Value.isDone = false; }
        }
        else { Debug.LogError("Invalid dictOfConnections (Null)"); }
    }

    /// <summary>
    /// Checks all connections and removes any ongoing effects of the specified ID and, if so, updates the material (colour) of the connections
    /// </summary>
    /// <param name="ongoingID"></param>
    public void RemoveOngoingEffect(int ongoingID)
    {
        Dictionary<int, Connection> dictOfConnections = GameManager.instance.dataScript.GetAllConnections();
        if (dictOfConnections != null)
        {
            foreach (var connection in dictOfConnections)
            { connection.Value.RemoveOngoingEffect(ongoingID); }
        }
        else { Debug.LogError("Invalid dictOfConnections (Null)"); }
    }

    /// <summary>
    /// Colour codes connections according to activity 
    /// Time: if within 0 or 1 turns ago -> red, 2 turns ago -> yellow, 3+ turns ago -> green, None -> grey
    /// Count: it's 3+ red, 2 yellow, 1 green, 0 grey. Saves original connection state and sets resetNeeded to true
    /// </summary>
    public void ShowConnectionActivity(ActivityUI activityUI)
    {
        int activityData = -1;
        int currentTurn = GameManager.instance.turnScript.Turn;
        Dictionary<int, Connection> dictOfConnections = GameManager.instance.dataScript.GetAllConnections();
        if (dictOfConnections != null)
        {
            resetNeeded = true;
            //loop all connections regardless
            foreach (var conn in dictOfConnections)
            {
                //save existing security level
                conn.Value.SaveSecurityLevel();
                //action depends on type of activity display required
                switch (activityUI)
                {
                    //Time (# of turns ago)
                    case ActivityUI.Time:
                        int limit = GameManager.instance.aiScript.activityTimeLimit;
                        activityData = conn.Value.activityTime;
                        if (activityData > -1)
                        {
                            int timeElapsed = currentTurn - activityData;
                            if (timeElapsed > -1)
                            {
                                switch (timeElapsed)
                                {
                                    case 0:
                                    case 1:
                                        conn.Value.ChangeSecurityLevel(ConnectionType.HIGH);
                                        break;
                                    case 2:
                                        conn.Value.ChangeSecurityLevel(ConnectionType.MEDIUM);
                                        break;
                                    case 3:
                                    default:
                                        if (timeElapsed > limit)
                                        { conn.Value.ChangeSecurityLevel(ConnectionType.None); }
                                        else
                                        {
                                            //within time elapsed allowance
                                            conn.Value.ChangeSecurityLevel(ConnectionType.LOW);
                                        }
                                        break;
                                }
                            }
                            else
                            { conn.Value.ChangeSecurityLevel(ConnectionType.None); }
                        }
                        else
                        { conn.Value.ChangeSecurityLevel(ConnectionType.None); }
                        break;
                    //Count
                    case ActivityUI.Count:
                        activityData = conn.Value.activityCount;
                        if (activityData > -1)
                        {
                            switch (activityData)
                            {
                                case 3:
                                    conn.Value.ChangeSecurityLevel(ConnectionType.HIGH);
                                    break;
                                case 2:
                                    conn.Value.ChangeSecurityLevel(ConnectionType.MEDIUM);
                                    break;
                                case 1:
                                    conn.Value.ChangeSecurityLevel(ConnectionType.LOW);
                                    break;
                                case 0:
                                    conn.Value.ChangeSecurityLevel(ConnectionType.None);
                                    break;
                                default:
                                    //more than 3
                                    conn.Value.ChangeSecurityLevel(ConnectionType.HIGH);
                                    break;
                            }
                        }
                        else
                        { conn.Value.ChangeSecurityLevel(ConnectionType.None); }
                        break;
                }
            }
        }
        else { Debug.LogError("Invalid dictOfConnections (Null)"); }
    }

    /// <summary>
    /// restores all connections back to their previously saved security levels (materials auto updated).
    /// </summary>
    public void RestoreConnections()
    {
        Dictionary<int, Connection> dictOfConnections = GameManager.instance.dataScript.GetAllConnections();
        if (dictOfConnections != null)
        {
            foreach (var conn in dictOfConnections)
            {
                //save existing security level
                conn.Value.RestoreSecurityLevel();
            }
        }
        else { Debug.LogError("Invalid dictOfConnections (Null)"); }
    }


    /// <summary>
    /// Permanently raises the security level (+1) of a randomly (some logic here) chosen connection due to an Authority decision. Returns true if successful
    /// </summary>
    public bool ProcessConnectionSecurityDecision()
    {
        bool isDone = false;
        int index;
        List<Node> listOfConnectedNodes = GameManager.instance.dataScript.GetListOfMostConnectedNodes();
        
        List<Node> tempList = new List<Node>();
        if (listOfConnectedNodes != null)
        {
            //create a temporary list (by value) that can be deleted from without upsetting the main list
            List<Node> listOfCopiedNodes = new List<Node>(listOfConnectedNodes);
            Debug.LogFormat("ListOfCopiedNodes -> Start -> {0}  Turn {1}", listOfCopiedNodes.Count, GameManager.instance.turnScript.Turn);
            Faction factionAuthority = GameManager.instance.factionScript.factionAuthority;
            if (factionAuthority != null)
            {
                NodeArc preferredNodeArc = factionAuthority.preferredArc;
                if (preferredNodeArc != null)
                {
                    //reverse loop list of most connected nodes and find any that match the preferred node type (delete entries from list to prevent future selection)
                    for (int i = listOfCopiedNodes.Count - 1; i >= 0; i--)
                    {
                        if (listOfCopiedNodes[i].Arc.name.Equals(preferredNodeArc.name) == true)
                        {
                            //add to tempList and remove from Copiedlist
                            tempList.Add(listOfCopiedNodes[i]);
                            listOfCopiedNodes.RemoveAt(i);
                        }
                    }
                    //found any suitable nodes and do they have suitable connections?
                    if (tempList.Count > 0)
                    {
                        Debug.LogFormat("ListOfCopiedNodes -> TempList.Count {0}", tempList.Count);
                        do
                        {
                            index = Random.Range(0, tempList.Count);
                            isDone = ProcessNodeConnection(tempList[index]);
                            if (isDone == false)
                            { tempList.RemoveAt(index); }
                            else { break; }
                        }
                        while (tempList.Count > 0 );
                    }
                }
                else { Debug.LogWarning("Invalid preferredNodeArc (Null)"); }
                Debug.LogFormat("ListOfCopiedNodes -> Preferred Nodes Done -> {0}", listOfCopiedNodes.Count);
                //keep looking if not yet successful. List should have all preferred nodes stripped out.
                if (isDone == false)
                {
                    Debug.Log("ListOfCopiedNodes -> Look for a Random Node");
                    //randomly choose nodes looking for suitable connections. Delete as you go to prevent future selections.
                    if (listOfCopiedNodes.Count > 0)
                    {
                        do
                        {
                            index = Random.Range(0, listOfCopiedNodes.Count);
                            Node nodeTemp = listOfCopiedNodes[index];
                            isDone = ProcessNodeConnection(nodeTemp);
                            if (isDone == false)
                            {
                                Debug.LogFormat("ListOfCopiedNodes -> Before -> {0}", listOfCopiedNodes.Count);
                                listOfCopiedNodes.RemoveAt(index);
                                Debug.LogFormat("ListOfCopiedNodes -> AFTER -> {0}", listOfCopiedNodes.Count);
                            }
                            else { break; }
                        }
                        while (listOfCopiedNodes.Count > 0);
                    }
                }
            }
            else { Debug.LogWarning("Invalid factionAuthority (Null)"); }
        }
        else { Debug.LogWarning("Invalid listOfMostConnectedNodes (Null)"); }
        if (isDone != true)
        { Debug.LogFormat("ConnectionManager.cs -> ProcessConnectionSecurityDecision: FAILED TO FIND suitable connection for nodeID {0}", "\n"); }
        return isDone;
    }

    /// <summary>
    /// sub-Method for ProcessConnectionSecurityDecision that takes a node, checks for a Securitylevel.None connection, raises it up +1 level and returns true if successful.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private bool ProcessNodeConnection(Node node)
    {
        bool isSuccessful = false;
        if (node != null)
        {
            List<Connection> listOfConnections = node.GetListOfConnections();
            if (listOfConnections != null)
            {
                Node nodeFar = null;
                //loop connections and take first one with no security
                foreach (Connection connection in listOfConnections)
                {
                    if (connection.SecurityLevel == ConnectionType.None)
                    {
                        nodeFar = connection.node1;
                        //check that we've got the correct connection end
                        if (nodeFar.nodeID == node.nodeID)
                        { nodeFar = connection.node2; }
                        //check that the far node has at least 2 connections (ignore single dead end connections)
                        if (nodeFar.CheckNumOfConnections() > 1)
                        {
                            //raise security level + 1 permanently
                            connection.ChangeSecurityLevel(ConnectionType.LOW);
                            Debug.LogFormat("ConnectionManager.cs -> ProcessNodeConnection: Connection ID {0} Security Level now LOW (nodeID's {1} -> {2}){3}", 
                                connection.connID, connection.node1.nodeID, connection.node2.nodeID, "\n");
                            isSuccessful = true;
                            break;
                        }
                    }
                }
            }
            else { Debug.LogWarningFormat("Invalid listOfConnections (Null) for nodeID {0}", node.nodeID); }
        }
        else { Debug.LogWarning("Invalid node (Null)"); }
        return isSuccessful;
    }

    //new methods above here
}
