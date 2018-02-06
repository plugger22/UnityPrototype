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



}
