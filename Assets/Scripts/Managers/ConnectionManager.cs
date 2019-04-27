using System.Collections;
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
    private ConnectionType secLvl;                                  //used to save existing security level of a single connection prior to flashing
    private List<ConnectionType> listOfSecLevels;                   //stores sec lvl of multiple connections to enable restoration after flashing
    private List<Connection> listOfFlashConnections;                //list of connections (copy) for multiple flashing connection event.
    private Coroutine myCoroutine;                                  //ginle handler. Can only run one ConnectionManager coroutine at a time (eg. single / multiple flashing connections)

    //fast access
    private float flashConnectionTime;

    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        InitialiseListOfConnections();

        //session specific (once only)
        if (GameManager.instance.inputScript.GameState == GameState.NewInitialisation)
        {
            //flash
            flashConnectionTime = GameManager.instance.guiScript.flashNodeTime;
            Debug.Assert(flashConnectionTime > 0, "Invalid flashConnectionTime (zero)");
            //Data Collections
            listOfSecLevels = new List<ConnectionType>();
            listOfFlashConnections = new List<Connection>();
            //register listener
            EventManager.instance.AddListener(EventType.FlashSingleConnectionStart, OnEvent, "ConnectionManager");
            EventManager.instance.AddListener(EventType.FlashSingleConnectionStop, OnEvent, "ConnectionManager");
            EventManager.instance.AddListener(EventType.FlashMultipleConnectionsStart, OnEvent, "ConnectionManager");
            EventManager.instance.AddListener(EventType.FlashMultipleConnectionsStop, OnEvent, "ConnectionManager");
        }
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
            case EventType.FlashSingleConnectionStart:
                StartFlashingSingleConnection((int)Param);
                break;
            case EventType.FlashSingleConnectionStop:
                StopFlashingSingleConnection((int)Param);
                break;
            case EventType.FlashMultipleConnectionsStart:
                StartFlashingMultipleConnections((List<Connection>)Param);
                break;
            case EventType.FlashMultipleConnectionsStop:
                StopFlashingMultipleConnections();
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
            //reset back to normal prior to any changes
            if (resetConnections == true)
            { RestoreConnections(); }
            //set flag as changes will be made
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
    /// Save all connections prior to changes in order to be able to restore back to their original state later
    /// </summary>
    public void SaveConnections()
    {
        List<Connection> listOfConnections = GameManager.instance.dataScript.GetListOfConnections();
        if (listOfConnections != null)
        {
            foreach (Connection conn in listOfConnections)
            {
                if (conn != null)
                {
                    //save existing security level
                    conn.SaveSecurityLevel();
                }
                else { Debug.LogError("Invalid connection (Null) in listOfConnections"); }
            }
            resetConnections = true;
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
                if (conn != null)
                {
                    //save existing security level
                    conn.RestoreSecurityLevel();
                }
                else { Debug.LogError("Invalid connection (Null) in listOfConnections"); }
            }
            resetConnections = false;
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
            GameManager.instance.messageScript.DecisionConnection(descriptor, connection, ConnectionType.LOW);
            //recalculate dijkstra weighting data
            GameManager.instance.dijkstraScript.RecalculateWeightedData();
        }
        else
        { Debug.LogWarningFormat("Invalid connection (Null) for connID {0}", connID); }
        return isSuccess;
    }

    /// <summary>
    /// event driven -> start coroutine
    /// </summary>
    /// <param name="nodeID"></param>
    private void StartFlashingSingleConnection(int connID)
    {
        Connection connection = GameManager.instance.dataScript.GetConnection(connID);
        if (connection != null)
        {
            if (myCoroutine == null)
            { 
            isFlashOn = false;
            secLvl = connection.SecurityLevel;
            myCoroutine = StartCoroutine("FlashSingleConnection", connection);
            }
            else { Debug.LogWarning("Invalid myCoroutine (should be Null), can't run FlashingSingleConnections as another connection coroutine already running"); }
        }
        else { Debug.LogWarningFormat("Invalid connection (Null) for connID {0}", connID); }
    }

    /// <summary>
    /// event driven -> stop coroutine
    /// </summary>
    private void StopFlashingSingleConnection(int connID)
    {
        if (myCoroutine != null)
        {
            StopCoroutine(myCoroutine);
            myCoroutine = null;
        }
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
    IEnumerator FlashSingleConnection(Connection connection)
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

    /// <summary>
    /// event driven -> start coroutine
    /// </summary>
    /// <param name="nodeID"></param>
    private void StartFlashingMultipleConnections(List<Connection> listOfConnections)
    {
        if (listOfConnections != null)
        {
            if (myCoroutine == null)
            {
                int numOfConn = listOfConnections.Count;
                if (numOfConn > 0)
                {
                    bool isError = false;
                    //do a quick run through to check all connections are valid to avoid constant checks during flashing sequence
                    for (int i = 0; i < numOfConn; i++)
                    {
                        Connection connection = listOfConnections[i];
                        if (connection == null)
                        {
                            Debug.LogWarning("Invalid connection (Null)");
                            isError = true;
                        }
                    }
                    //O.K to proceed
                    if (isError == false)
                    {
                        //clear list of any previous data
                        listOfSecLevels.Clear();
                        //security levels
                        for (int i = 0; i < numOfConn; i++)
                        { listOfSecLevels.Add(listOfConnections[i].SecurityLevel); }
                        isFlashOn = false;
                        //copy data into local list for access when stopping coroutine
                        listOfFlashConnections = listOfConnections;
                        myCoroutine = StartCoroutine("FlashMultipleConnections", listOfConnections);
                    }
                }
                else { Debug.LogWarning("Invalid listOfConnections (must be > Zero)"); }
            }
            else { Debug.LogWarning("Invalid myCoroutine (should be Null), can't run FlashingMultipleConnections as another connection coroutine already running"); }
        }
        else { Debug.LogWarning("Invalid listOfConnections (Null)"); }
    }

    /// <summary>
    /// event driven -> stop coroutine
    /// </summary>
    private void StopFlashingMultipleConnections()
    {
        if (listOfFlashConnections != null)
        {
            if (myCoroutine != null)
            {
                StopCoroutine(myCoroutine);
                myCoroutine = null;
            }
            int numOfConn = listOfFlashConnections.Count;
            //resture original security level material
            for (int i = 0; i < numOfConn; i++)
            { listOfFlashConnections[i].SetMaterial(listOfSecLevels[i]); }
        }
        else { Debug.LogWarning("Invalid listOfFlashConnections (Null)"); }
    }


    /// <summary>
    /// coroutine to flash a list of connections
    /// NOTE: listOfConnections checked for null as are individual connections within and for > zero count by calling procedure
    /// </summary>
    /// <param name="connection"></param>
    /// <returns></returns>
    IEnumerator FlashMultipleConnections(List<Connection> listOfConnections)
    {
        int numOfConn = listOfConnections.Count;
        //forever loop
        for (; ; )
        {
            if (isFlashOn == false)
            {
                for (int i = 0; i < numOfConn; i++)
                {
                    Connection connection = listOfConnections[i];
                    connection.SetMaterial(ConnectionType.Active);
                }
                isFlashOn = true;
                yield return new WaitForSecondsRealtime(flashConnectionTime);
            }
            else
            {
                for (int i = 0; i < numOfConn; i++)
                {
                    Connection connection = listOfConnections[i];
                    //resture original security level material
                    connection.SetMaterial(listOfSecLevels[i]);
                }
                isFlashOn = false;
                yield return new WaitForSecondsRealtime(flashConnectionTime);
            }
        }
    }

    //new methods above here
}
