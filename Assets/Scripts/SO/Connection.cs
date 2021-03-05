using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using packageAPI;
using System.Text;

public class Connection : MonoBehaviour
{

    [Tooltip("Internal ball (sphere) used to represent movement within connection")]
    public GameObject ball;

    //private ConnectionType securityLevel;

    private int v1;                                         //vertice nodeID's for either end of the connection
    private int v2;

    private int _securityLevel;                             //private backing field (int vs. enum)
    private int securityLevelSave;               //stores existing security level prior to temporary changes

    private bool onMouseFlag;                           //flag indicates that onMouseOver is true (used for tooltip coroutine)
    private bool isMoving;                              //true if ball moving coroutine running, false if not
    private float mouseOverDelay;                       //tooltip
    private float mouseOverFade;                        //tooltip
    private Coroutine myCoroutine;
    private Coroutine ballCoroutine;
    /*private Renderer renderer;*/


    private List<EffectDataOngoing> listOfOngoingEffects = new List<EffectDataOngoing>();   //list of temporary (ongoing) effects impacting on the node

    [HideInInspector] public int connID;                                      //unique connectionID 
    [HideInInspector] public bool isDone;                                     //flag used to prevent connection being changed more than once for an effect

    [HideInInspector] public int VerticeOne { get { return v1; } }
    [HideInInspector] public int VerticeTwo { get { return v2; } }
    [HideInInspector] public Node node1;                                        //Nodes at either end of connection
    [HideInInspector] public Node node2;

    [HideInInspector] public int activityCount = -1;       //# times rebel activity occurred (invis-1, player movement)
    [HideInInspector] public int activityTime = -1;        //most recent turn when rebel activity occurred

    //fast access
    private int connectionRepeat = -1;



    //Security property -> a bit tricky but needed to handle the difference between the enum (None/High/Med/Low) and the int backing field.
    public ConnectionType SecurityLevel
    {
        get
        {
            //ongoing effects are +ve to Raise the security level and -ve to lower it, per point
            int tempValue = GetOngoingEffect();
            if (_securityLevel == 0)
            {
                //raise level
                if (tempValue > 0)
                {
                    tempValue = Mathf.Clamp(tempValue, 1, 3);
                    tempValue = 4 - tempValue;
                }
                //lower but can't go any lower than None
                else { tempValue = _securityLevel; }
            }
            else
            {
                //security level any value other than 'none'
                if (tempValue > 0)
                {
                    //raise security level
                    tempValue = _securityLevel - tempValue;
                    if (tempValue <= 0) { tempValue = 1; }
                }
                else if (tempValue < 0)
                {
                    //lower security level (double minus as tempValue is also negative)
                    tempValue = _securityLevel - tempValue;
                    if (tempValue > 3) { tempValue = 0; }
                }
                else
                {
                    //no change
                    tempValue = _securityLevel;
                }
            }
            return (ConnectionType)tempValue;
        }
        //NOTE: Internal use only -> use ChangeSecurityLevel() to change Security Level as these will swap material if needed
        private set
        {
            switch (value)
            {
                case ConnectionType.HIGH:
                    _securityLevel = 1;
                    break;
                case ConnectionType.MEDIUM:
                    _securityLevel = 2;
                    break;
                case ConnectionType.LOW:
                    _securityLevel = 3;
                    break;
                case ConnectionType.None:
                    _securityLevel = 0;
                    break;
            }
        }
    }

    public void Awake()
    {
        if (ball != null)
        {
            //deactivate ball
            ball.gameObject.SetActive(false);
        }
        else { Debug.LogError("Invalid ball (Null)"); }
    }

    public void Start()
    {
        mouseOverDelay = GameManager.i.guiScript.tooltipDelay;
        mouseOverFade = GameManager.i.guiScript.tooltipFade;
        connectionRepeat = GameManager.i.animateScript.connectionRepeat;
        Debug.Assert(connectionRepeat > -1, "Invalid connectionRepeat (-1)");
    }

    public void InitialiseConnection(int v1, int v2)
    {
        this.v1 = v1;
        this.v2 = v2;
    }


    /// <summary>
    /// adjust security level to a specified value and change connection to the appropriate color
    /// </summary>
    /// <param name="secLvl"></param>
    public void ChangeSecurityLevel(ConnectionType secLvl)
    {
        /*Debug.LogFormat("[Tst] Connection.cs -> ChangeSecurityLevel: to secLvel {0} for connID {1}{2}", secLvl, connID, "\n");*/

        if (secLvl != SecurityLevel)
        {
            SecurityLevel = secLvl;
            SetMaterial(secLvl);
        }
    }

    /// <summary>
    /// adjust Security level up (+ve) or down (-ve) and change connection to the appropriate colour
    /// </summary>
    /// <param name="adjust"></param>
    public void ChangeSecurityLevel(int adjust)
    {
        //ignore if connection has already been changed for the current effect
        if (isDone == false)
        {
            /*Debug.LogFormat("[Tst] Connection.cs -> ChangeSecurityLevel: adjust {0} for connID {1}{2}", adjust, connID, "\n");*/

            //set flag to prevent being changed twice
            isDone = true;
            ConnectionType originalLevel = SecurityLevel;
            int currentLevel = (int)SecurityLevel;
            //current SecurityLevel 'None'
            if (currentLevel == 0)
            {
                if (adjust > 0)
                {
                    //increase security level (can't go any further below where it already is)
                    currentLevel = 4 - adjust;
                    switch (currentLevel)
                    {
                        case 3:
                            SecurityLevel = ConnectionType.LOW;
                            break;
                        case 2:
                            SecurityLevel = ConnectionType.MEDIUM;
                            break;
                        case 1:
                        case 0:
                        default:
                            //overshot -> max security level ceiling
                            SecurityLevel = ConnectionType.HIGH;
                            break;
                    }
                }
            }
            //current SecurityLevel at a value higher than 'None'
            else
            {
                if (adjust > 0)
                {
                    //raise security level
                    currentLevel -= adjust;
                    switch (currentLevel)
                    {
                        case 3:
                            SecurityLevel = ConnectionType.LOW;
                            break;
                        case 2:
                            SecurityLevel = ConnectionType.MEDIUM;
                            break;
                        case 1:
                        case 0:
                        default:
                            //overshot -> max security level ceiling
                            SecurityLevel = ConnectionType.HIGH;
                            break;
                    }
                }
                else if (adjust < 0)
                {
                    //lower security level (double minus as adjust is also negative)
                    currentLevel -= adjust;
                    switch (currentLevel)
                    {
                        case 3:
                            SecurityLevel = ConnectionType.LOW;
                            break;
                        case 2:
                            SecurityLevel = ConnectionType.MEDIUM;
                            break;
                        case 1:
                            SecurityLevel = ConnectionType.HIGH;
                            break;
                        case 0:
                        default:
                            //undershot -> min security level floor
                            SecurityLevel = ConnectionType.None;
                            break;
                    }
                }
            }
            //change material if different
            if (originalLevel != SecurityLevel)
            { SetMaterial(SecurityLevel); }
        }
    }

    /// <summary>
    /// saves level to a field so it can be restored back to the same state later. Uses private backing field in order to exclude any Ongoing effects.
    /// </summary>
    public void SaveSecurityLevel()
    {
        securityLevelSave = _securityLevel;
    }

    /// <summary>
    /// restores previously saved security level and updates connection material
    /// </summary>
    public void RestoreSecurityLevel()
    {
        /*SecurityLevel = securityLevelSave;*/
        switch (securityLevelSave)
        {
            case 1: SecurityLevel = ConnectionType.HIGH; break;
            case 2: SecurityLevel = ConnectionType.MEDIUM; break;
            case 3: SecurityLevel = ConnectionType.LOW; break;
            case 0: SecurityLevel = ConnectionType.None; break;
            default: Debug.LogWarningFormat("Unrecognised _securityLevel \"{0}\"", securityLevelSave); break;
        }
        SetMaterial(SecurityLevel);
    }

    /// <summary>
    /// sub method to change connections material (colour)
    /// </summary>
    /// <param name="secLvl"></param>
    public void SetMaterial(ConnectionType secLvl)
    {
        GetComponent<Renderer>().material = GameManager.i.connScript.GetConnectionMaterial(secLvl);
    }

    public int GetNode1()
    { return v1; }

    public int GetNode2()
    { return v2; }

    //
    // - - - AI - - -
    //

    /// <summary>
    /// add a new set of activity data (time and count)
    /// </summary>
    /// <param name="turn"></param>
    public void AddActivityData(int turn)
    {
        activityCount++;
        //default value -1 so first data point needs to be '1's
        if (activityCount == 0) { activityCount++; }
        //update for the most recent activity (highest turn #)
        if (activityTime < turn)
        { activityTime = turn; }
    }


    //
    // - - - Ongoing Effects - - -
    //

    public List<EffectDataOngoing> GetListOfOngoingEffects()
    { return listOfOngoingEffects; }

    /// <summary>
    /// clear list and copy across loaded save game data
    /// </summary>
    /// <param name="listOfOngoing"></param>
    public void SetListOfOngoingEffects(List<EffectDataOngoing> listOfOngoing)
    {
        if (listOfOngoing != null)
        {
            listOfOngoingEffects.Clear();
            listOfOngoingEffects.AddRange(listOfOngoing);
        }
        else { Debug.LogError("Invalid listOfOngoing (Null)"); }
    }

    /// <summary>
    /// Returns tally of ongoing effects for the SecurityLevel, '0' if none, every +1 is increase a level of security, every -1 is decrease
    /// </summary>
    /// <param name="outcome"></param>
    /// <returns></returns>
    private int GetOngoingEffect()
    {
        int value = 0;
        if (listOfOngoingEffects.Count > 0)
        {
            foreach (var adjust in listOfOngoingEffects)
            { value += adjust.value; }
        }
        return value;
    }


    /// <summary>
    /// Add temporary effect to the listOfOngoingEffects. Returns true if successful (eg. not a duplicate)
    /// </summary>
    /// <param name="ongoing"></param>
    /// <returns></returns>
    public bool AddOngoingEffect(EffectDataOngoing ongoing)
    {
        bool isNotDuplicate = true;
        if (ongoing != null)
        {
            //check to see if an identical ongoingID not already present
            if (listOfOngoingEffects.Count == 0)
            { listOfOngoingEffects.Add(ongoing); }
            else
            {
                //check list for dupes
                for (int i = 0; i < listOfOngoingEffects.Count; i++)
                {
                    if (listOfOngoingEffects[i].ongoingID == ongoing.ongoingID)
                    { isNotDuplicate = false; break; }
                }
                if (isNotDuplicate == true)
                {
                    listOfOngoingEffects.Add(ongoing);
                    //add to register & create message
                    GameManager.i.dataScript.AddOngoingEffectToDict(ongoing, connID);
                }
            }
        }
        else { Debug.LogError("Invalid EffectDataOngoing (Null)"); }
        return isNotDuplicate;
    }

    /// <summary>
    /// checks listOfOngoingEffects for any matching ongoingID and deletes them
    /// NOTE: there are no checks for not finding the uniqueID or having no entries in list as ConnectionManager.cs loops through all connections looking for the right ones.
    /// </summary>
    /// <param name="uniqueID"></param>
    public void RemoveOngoingEffect(int uniqueID)
    {
        if (listOfOngoingEffects.Count > 0)
        {
            //reverse loop, deleting as you go
            for (int i = listOfOngoingEffects.Count - 1; i >= 0; i--)
            {
                EffectDataOngoing ongoing = listOfOngoingEffects[i];
                if (ongoing.ongoingID == uniqueID)
                {
                    //amount to reverse security level
                    Debug.LogFormat("Connection, ID {0}, Ongoing Effect ID {1}, \"{2}\", REMOVED{3}", connID, ongoing.ongoingID, ongoing.description, "\n");
                    //remove from register & create message
                    GameManager.i.dataScript.RemoveOngoingEffectFromDict(ongoing);
                    //remove from list
                    listOfOngoingEffects.RemoveAt(i);
                    //reset material of connection. Note that you are simply redoing the same security level without the additional ongoing effect (which will be at a lower level)
                    SetMaterial(SecurityLevel);
                }
            }
        }
    }


    /// <summary>
    /// Mouse Over tool tip & Right Click
    /// </summary>
    private void OnMouseOver()
    {
        //check modal block isn't in place
        if (GameManager.i.guiScript.CheckIsBlocked() == false)
        {
            //Right click connection
            if (Input.GetMouseButtonDown(1) == true)
            {
            }
            //Tool tip
            else
            {
                onMouseFlag = true;
                if (GameManager.i.optionScript.connectorTooltips == true)
                {
                    //exit any node tooltip that might be open
                    GameManager.i.tooltipNodeScript.CloseTooltip("Connection.cs -> OnMouseOver");
                    //start tooltip routine
                    myCoroutine = StartCoroutine("ShowTooltip");
                }
            }
        }
    }

    /// <summary>
    /// mouse over exit, shut down tooltip
    /// </summary>
    private void OnMouseExit()
    {
        if (GameManager.i.guiScript.CheckIsBlocked() == false)
        {
            onMouseFlag = false;
            //tooltips for Connections may not be switched on
            if (myCoroutine != null)
            { StopCoroutine(myCoroutine); }
            GameManager.i.tooltipConnScript.CloseTooltip("Connection.cs -> OnMouseExit");
        }
    }

    /// <summary>
    /// Connection tooltip
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowTooltip()
    {
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over button
        if (onMouseFlag == true)
        {
            //do once
            while (GameManager.i.tooltipConnScript.CheckTooltipActive() == false)
            {
                //debug activity data
                StringBuilder builder = new StringBuilder();
                if (GameManager.i.optionScript.debugData == true)
                {
                    builder.AppendFormat("activityTimeKnown      {0}{1}{2}", activityTime > 0 ? "T" : "", activityTime, "\n");
                    builder.AppendFormat("activityCountKnown     {0}{1}{2}", activityCount > 0 ? "+" : "", activityCount, "\n");
                }
                else
                {
                    //Activity info details
                    switch(GameManager.i.nodeScript.activityState)
                    {
                        case ActivityUI.None:
                            switch(SecurityLevel)
                            {
                                case ConnectionType.HIGH:
                                case ConnectionType.MEDIUM:
                                case ConnectionType.LOW:
                                    int delay = GameManager.i.nodeScript.GetAIDelayForMove(SecurityLevel);
                                    builder.AppendFormat("If used Authority will know in{0}<font=\"Roboto-Bold SDF\">{1} turn{2}</font>", "\n", delay,
                                        delay != 1 ? "s" : "");
                                    break;
                                case ConnectionType.None:
                                    builder.AppendFormat("If used Authority will be{0}<font=\"Roboto-Bold SDF\">UNAWARE</font>", "\n");
                                    break;
                            }
                            break;
                        case ActivityUI.Time:
                            int limit = GameManager.i.aiScript.activityTimeLimit;
                            int turnCurrent = GameManager.i.turnScript.Turn;
                            int elapsedTime = -1;

                            if (activityTime > -1)
                            { elapsedTime = turnCurrent - activityTime; }
                            if (elapsedTime > -1)
                            {
                                builder.AppendFormat("Last known activity{0}<font=\"Roboto-Bold SDF\">{1} turn{2} ago</font>{3}(ignored after {4} turns)", "\n",
                                    elapsedTime, elapsedTime != 1 ? "s" : "", "\n", limit);
                            }
                            else
                            { builder.AppendFormat("There has been{0}<font=\"Roboto-Bold SDF\">No Known Activity</font>", "\n"); }
                            break;
                        case ActivityUI.Count:
                            if (activityCount > 0)
                            { builder.AppendFormat("There {0} been{1}<font=\"Roboto-Bold SDF\">{2} Known</font>{3} incident{4} (total)", 
                                activityCount != 1 ? "have" : "has", "\n", activityCount, "\n", activityCount != 1 ? "s" : ""); }
                            else { builder.AppendFormat("There have been{0}<font=\"Roboto-Bold SDF\">No Known</font>{1}incidents", "\n", "\n"); }
                            break;
                    }
                }
                GameManager.i.tooltipConnScript.SetTooltip(transform.position, connID, SecurityLevel, listOfOngoingEffects, builder.ToString());
                yield return null;
            }
            //fade in
            float alphaCurrent;
            while (GameManager.i.tooltipConnScript.GetOpacity() < 1.0)
            {
                alphaCurrent = GameManager.i.tooltipConnScript.GetOpacity();
                alphaCurrent += Time.deltaTime / mouseOverFade;
                GameManager.i.tooltipConnScript.SetOpacity(alphaCurrent);
                yield return null;
            }
        }
    }


    /// <summary>
    /// Moves ball along connection. Random direction. Has a chance to repeat (can repeat multiple times provided it succeeds in connectionRepeat roll)
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveBall(float speed)
    {
        bool isForward = false;    //determines direction
        bool isRepeat = false;
        float y_pos = -0.95f;
        float amount = 0.0f;
        if (Random.Range(0, 100) < 50)
        {
            isForward = true;
            y_pos = 0.95f;
        }
        ball.transform.localPosition = new Vector3(0, y_pos, 0);
        ball.SetActive(true);
        isMoving = true;
        do
        {
            do
            {
                //move ball
                amount += Time.deltaTime / speed;
                if (isForward == true)
                { y_pos -= amount; }
                else { y_pos += amount; }
                /*Debug.LogFormat("[Tst] Connection.SO -> MoveBall: connID {0}, adjust {1}, amount {2}{3}, y_pos now {4}{5}", connID, adjust, isForward == false ? "+" : "", amount, y_pos, "\n");*/
                ball.transform.localPosition = new Vector3(0, y_pos, 0);
                yield return null;
            }
            while (y_pos <= 0.95f && y_pos >= -0.95f);
            //repeat
            if (Random.Range(0, 100) < connectionRepeat)
            {
                isRepeat = true;
                //reset ball
                y_pos = -0.95f;
                amount = 0.0f;
                if (isForward == true)
                { y_pos = 0.95f; }
                ball.transform.localPosition = new Vector3(0, y_pos, 0);
            }
            else { isRepeat = false; }
        }
        while (isRepeat == true);
        ball.SetActive(false);
        isMoving = false;
    }

    /// <summary>
    /// returns true if ball coroutine still running
    /// </summary>
    /// <returns></returns>
    public bool CheckBallMoving()
    { return isMoving; }


    /*/// <summary>
    /// Called to destroy connection instance (clone of prefab)
    /// </summary>
    public void DestroyConnection()
    { Destroy(this.gameObject); }*/


    //new methods above here
}
