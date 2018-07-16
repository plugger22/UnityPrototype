﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using System.Linq;

/// <summary>
/// Handles all connection related matters
/// </summary>
public class ConnectionManager : MonoBehaviour
{
   
    [Tooltip("In order -> None, HIGH, MEDIUM, LOW, Active")]
    public Material[] arrayOfConnectionTypes;

    [HideInInspector] public bool resetConnections;                      //used for temp changes in connection states, if true run RestoreConnections

    private bool isFlashOn = false;                                 //used for flashing Connection coroutine
    private ConnectionType secLvl;                                  //used to save existing security level of connection prior to flashing
    private Coroutine myCoroutine;

    //fast access
    private float flashConnectionTime;

    public void Initialise()
    {
        //flash
        flashConnectionTime = GameManager.instance.guiScript.flashNodeTime;
        Debug.Assert(flashConnectionTime > 0, "Invalid flashConnectionTime (zero)");
        //Data Collections
        InitialiseListOfConnections();
        //register listener
        EventManager.instance.AddListener(EventType.FlashConnectionStart, OnEvent, "ConnectionManager");
        EventManager.instance.AddListener(EventType.FlashConnectionStop, OnEvent, "ConnectionManager");
    }

    /// <summary>
    /// event handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //detect event type
        switch (eventType)
        {
            case EventType.FlashConnectionStart:
                StartFlashingConnection((int)Param);
                break;
            case EventType.FlashConnectionStop:
                StopFlashingConnection((int)Param);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Sets up a mirrored listOfConnections for iteration purposes (performant)
    /// </summary>
    private void InitialiseListOfConnections()
    {
        Dictionary<int, Connection> dictOfConnections = GameManager.instance.dataScript.GetDictOfConnections();
        if (dictOfConnections != null)
        {
            List<Connection> listOfConnections = GameManager.instance.dataScript.GetListOfConnections();
            if (listOfConnections != null)
            {
                listOfConnections.Clear();
                listOfConnections.AddRange(dictOfConnections.Values.ToList());
                Debug.LogFormat("[Imp] ConnectionManager.cs -> dictOfConnections has {0} records", dictOfConnections.Count);
                Debug.LogFormat("[Imp] ConnectionManager.cs -> listOfConnections has {0} records", listOfConnections.Count);
                Debug.Assert(listOfConnections.Count == dictOfConnections.Count, "Mismatch on record count between listOfConnections and dictOfConnections");
            }
            else { Debug.LogWarning("Invalid listOfConnections (Null)"); }
        }
        else { Debug.LogWarning("Invalid dictOfConnections (Null)"); }
    }


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
        Dictionary<int, Connection> dictOfConnections = GameManager.instance.dataScript.GetDictOfConnections();
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
        List<Connection> listOfConnections = GameManager.instance.dataScript.GetListOfConnections();
        if (listOfConnections != null)
        {
            foreach (Connection connection in listOfConnections)
            { connection.RemoveOngoingEffect(ongoingID); }
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
        List<Connection> listOfConnections = GameManager.instance.dataScript.GetListOfConnections();
        if (listOfConnections != null)
        {
            resetConnections = true;
            //loop all connections regardless
            foreach (Connection conn in listOfConnections)
            {
                //save existing security level
                conn.SaveSecurityLevel();
                //action depends on type of activity display required
                switch (activityUI)
                {
                    //Time (# of turns ago)
                    case ActivityUI.Time:
                        int limit = GameManager.instance.aiScript.activityTimeLimit;
                        activityData = conn.activityTime;
                        if (activityData > -1)
                        {
                            int timeElapsed = currentTurn - activityData;
                            if (timeElapsed > -1)
                            {
                                switch (timeElapsed)
                                {
                                    case 0:
                                    case 1:
                                        conn.ChangeSecurityLevel(ConnectionType.HIGH);
                                        break;
                                    case 2:
                                        conn.ChangeSecurityLevel(ConnectionType.MEDIUM);
                                        break;
                                    case 3:
                                    default:
                                        if (timeElapsed > limit)
                                        { conn.ChangeSecurityLevel(ConnectionType.None); }
                                        else
                                        {
                                            //within time elapsed allowance
                                            conn.ChangeSecurityLevel(ConnectionType.LOW);
                                        }
                                        break;
                                }
                            }
                            else
                            { conn.ChangeSecurityLevel(ConnectionType.None); }
                        }
                        else
                        { conn.ChangeSecurityLevel(ConnectionType.None); }
                        break;
                    //Count
                    case ActivityUI.Count:
                        activityData = conn.activityCount;
                        if (activityData > -1)
                        {
                            switch (activityData)
                            {
                                case 3:
                                    conn.ChangeSecurityLevel(ConnectionType.HIGH);
                                    break;
                                case 2:
                                    conn.ChangeSecurityLevel(ConnectionType.MEDIUM);
                                    break;
                                case 1:
                                    conn.ChangeSecurityLevel(ConnectionType.LOW);
                                    break;
                                case 0:
                                    conn.ChangeSecurityLevel(ConnectionType.None);
                                    break;
                                default:
                                    //more than 3
                                    conn.ChangeSecurityLevel(ConnectionType.HIGH);
                                    break;
                            }
                        }
                        else
                        { conn.ChangeSecurityLevel(ConnectionType.None); }
                        break;
                }
            }
        }
        else { Debug.LogError("Invalid listOfConnections (Null)"); }
    }

    /// <summary>
    /// restores all connections back to their previously saved security levels (materials auto updated).
    /// </summary>
    public void RestoreConnections()
    {
        List<Connection> listOfConnections = GameManager.instance.dataScript.GetListOfConnections();
        if (listOfConnections != null)
        {
            foreach (Connection conn in listOfConnections)
            {
                //save existing security level
                conn.RestoreSecurityLevel();
            }
        }
        else { Debug.LogError("Invalid listOfConnections (Null)"); }
    }


    /// <summary>
    /// Permanently raises the security level (+1) of a specific (some logic here) chosen connection due to an Authority decision. Returns true if successful
    /// </summary>
    public bool ProcessConnectionSecurityDecision(int connID)
    {
        bool isSuccess = false;
        Connection connection = GameManager.instance.dataScript.GetConnection(connID);
        if (connection != null)
        {
            connection.ChangeSecurityLevel(ConnectionType.LOW);
            isSuccess = true;
            //message
            string descriptor = string.Format("ConnID {0}, Security Level now LOW (btwn nodeID's {1} & {2})",
                connection.connID, connection.node1.nodeID, connection.node2.nodeID);
            Message message = GameManager.instance.messageScript.DecisionConnection(descriptor, connection.connID, (int)ConnectionType.LOW);
            GameManager.instance.dataScript.AddMessage(message);
        }
        else
        { Debug.LogWarningFormat("Invalid connection (Null) for connID {0}", connID); }
        return isSuccess;
    }

        /// <summary>
        /// event driven -> start coroutine
        /// </summary>
        /// <param name="nodeID"></param>
        private void StartFlashingConnection(int connID)
    {
        Connection connection = GameManager.instance.dataScript.GetConnection(connID);
        if (connection != null)
        {
            isFlashOn = false;
            secLvl = connection.SecurityLevel;

            myCoroutine = StartCoroutine("FlashingConnection", connection);
        }
        else { Debug.LogWarningFormat("Invalid connection (Null) for connID {0}", connID); }
    }

    /// <summary>
    /// event driven -> stop coroutine
    /// </summary>
    private void StopFlashingConnection(int connID)
    {
        if (myCoroutine != null)
        { StopCoroutine(myCoroutine); }
        //resture original security level material
        Connection connection = GameManager.instance.dataScript.GetConnection(connID);
        if (connection != null)
        {
            //connection.ChangeSecurityLevel(secLvl);
            connection.SetMaterial(secLvl);
        }
        else { Debug.LogWarningFormat("Invalid connection (Null) for connID {0}", connID); }
    }


    /// <summary>
    /// coroutine to flash a connection
    /// NOTE: Connection checked for null by calling procedure
    /// </summary>
    /// <param name="connection"></param>
    /// <returns></returns>
    IEnumerator FlashingConnection(Connection connection)
    {
        //forever loop
        for (; ; )
        {
            if (isFlashOn == false)
            {
                connection.SetMaterial(ConnectionType.Active);
                //NodeRedraw = true;
                isFlashOn = true;
                yield return new WaitForSecondsRealtime(flashConnectionTime);
            }
            else
            {
                connection.SetMaterial(secLvl);
                //NodeRedraw = true;
                isFlashOn = false;
                yield return new WaitForSecondsRealtime(flashConnectionTime);
            }
        }
    }

    //new methods above here
}
