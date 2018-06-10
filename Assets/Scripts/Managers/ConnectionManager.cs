using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;


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
            resetConnections = true;
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


    /*/// <summary>
    /// Permanently raises the security level (+1) of a specific (some logic here) chosen connection due to an Authority decision. Returns true if successful
    /// </summary>
    public bool ProcessConnectionSecurityDecisionKKK()
    {
        bool isDone = false;
        int index;
        List<Node> listOfDecisionNodes = GameManager.instance.dataScript.GetListOfDecisionNodes();
        List<Node> tempList = new List<Node>();
        if (listOfDecisionNodes != null)
        {
            //Debug.LogFormat("ListOfDecisionNodes -> Start -> {0}  Turn {1}", listOfDecisionNodes.Count, GameManager.instance.turnScript.Turn);
            Faction factionAuthority = GameManager.instance.factionScript.factionAuthority;
            if (factionAuthority != null)
            {
                NodeArc preferredNodeArc = factionAuthority.preferredArc;
                if (preferredNodeArc != null)
                {
                    //reverse loop list of most connected nodes and find any that match the preferred node type (delete entries from list to prevent future selection)
                    for (int i = listOfDecisionNodes.Count - 1; i >= 0; i--)
                    {
                        if (listOfDecisionNodes[i].Arc.name.Equals(preferredNodeArc.name) == true)
                        {
                            //add to tempList and remove from decision List
                            tempList.Add(listOfDecisionNodes[i]);
                            listOfDecisionNodes.RemoveAt(i);
                        }
                    }
                    //found any suitable nodes and do they have suitable connections?
                    if (tempList.Count > 0)
                    {
                        //Debug.LogFormat("ListOfDecisionNodes -> TempList.Count {0}", tempList.Count);
                        do
                        {
                            index = Random.Range(0, tempList.Count);
                            isDone = ProcessNodeConnection(tempList[index]);
                            if (isDone == false)
                            { tempList.RemoveAt(index); }
                            else { break; }
                        }
                        while (tempList.Count > 0);
                    }
                }
                else { Debug.LogWarning("Invalid preferredNodeArc (Null)"); }
                //Debug.LogFormat("ListOfDecisionNodes -> Preferred Nodes Done -> {0}", listOfDecisionNodes.Count);
                //keep looking if not yet successful. List should have all preferred nodes stripped out.
                if (isDone == false)
                {
                    //Debug.Log("ListOfDecisionNodes -> Look for a Random Node");
                    //randomly choose nodes looking for suitable connections. Delete as you go to prevent future selections.
                    if (listOfDecisionNodes.Count > 0)
                    {
                        do
                        {
                            index = Random.Range(0, listOfDecisionNodes.Count);
                            Node nodeTemp = listOfDecisionNodes[index];
                            isDone = ProcessNodeConnection(nodeTemp);
                            if (isDone == false)
                            { listOfDecisionNodes.RemoveAt(index); } //not needed with refactored code but left in anyway
                            else { break; }
                        }
                        while (listOfDecisionNodes.Count > 0);
                    }
                }
            }
            else { Debug.LogWarning("Invalid factionAuthority (Null)"); }
        }
        else { Debug.LogWarning("Invalid listOfMostConnectedNodes (Null)"); }
        if (isDone != true)
        { Debug.LogWarningFormat("ConnectionManager.cs -> ProcessConnectionSecurityDecision: FAILED TO FIND suitable connection for nodeID {0}", "\n"); }
        else
        {
            //update listOfDecisionNodes
            GameManager.instance.aiScript.SetDecisionNodes();
        }
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
                            //message
                            string descriptor = string.Format("ConnID {0}, Security Level now LOW (btwn nodeID's {1} & {2})", 
                                connection.connID, connection.node1.nodeID, connection.node2.nodeID);
                            Message message = GameManager.instance.messageScript.DecisionConnection(descriptor, connection.connID, (int)ConnectionType.LOW);
                            GameManager.instance.dataScript.AddMessage(message);
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
    }*/

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
