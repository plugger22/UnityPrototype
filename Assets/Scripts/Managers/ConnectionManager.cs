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
        List<Node> listOfConnectedNodes = GameManager.instance.dataScript.GetListOfMostConnectedNodes();
        
        List<Node> tempList = new List<Node>();
        if (listOfConnectedNodes != null)
        {
            //create a temporary list (by value) that can be deleted from without upsetting the main list
            List<Node> listOfCopiedNodes = new List<Node>(listOfConnectedNodes);
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
                        { tempList.Add(listOfCopiedNodes[i]); }
                    }
                    //found any suitable nodes and do they have suitable connections?
                    if (tempList.Count > 0)
                    { isDone = ProcessNodeConnection(node); }
                }
                else { Debug.LogWarning("Invalid preferredNodeArc (Null)"); }
                //keep looking if not yet successful
                if (isDone == false)
                {
                    //randomly choose nodes looking for suitable connections. Delete as you go to prevent future selections.
                }
            }
            else { Debug.LogWarning("Invalid factionAuthority (Null)"); }
        }
        else { Debug.LogWarning("Invalid listOfMostConnectedNodes (Null)"); }
        return isDone;
    }

    //new methods above here
}
