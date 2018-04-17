using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;


/// <summary>
/// Handles all connection related matters
/// </summary>
public class ConnectionManager : MonoBehaviour
{
    public int connectionSecurityChance = 25;                       //chance of connection having a higher security level at game start (than default)
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
}
