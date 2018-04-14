using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using packageAPI;
using System.Text;
using System;

public class Connection : MonoBehaviour
{

    //private ConnectionType securityLevel;

    private int v1;                                         //vertice nodeID's for either end of the connection
    private int v2;

    private int _securityLevel;                             //private backing field (int vs. enum)
    private int securityLevelSave;                          //stores existing security level prior to temporary changes

    private bool onMouseFlag;                           //flag indicates that onMouseOver is true (used for tooltip coroutine)
    private float mouseOverDelay;                       //tooltip
    private float mouseOverFade;                        //tooltip
    private float fadeInTime;                           //tooltip
    private GlobalSide side;                            //tooltip

    private List<EffectDataOngoing> listOfOngoingEffects;   //list of temporary (ongoing) effects impacting on the node

    [HideInInspector] public int connID;                                      //unique connectionID 
    [HideInInspector] public bool isDone;                                     //flag used to prevent connection being changed more than once for an effect

    [HideInInspector] public int VerticeOne { get { return v1; } }
    [HideInInspector] public int VerticeTwo { get { return v2; } }

    [HideInInspector] public int activityCountKnown = -1;       //# times known rebel activity occurred (invis-1, player movement)
    [HideInInspector] public int activityCountPossible = -1;    //# times suspected rebel activity occured (negative drop in conn security level for unexplained reasons)
    [HideInInspector] public int activityTurnKnown = -1;        //most recent turn when known rebel activity occurred
    [HideInInspector] public int activityTurnPossible = -1;     //most recent turn when suspected rebel activity occurred

    private string colourSide;
    private string colourRebel;
    private string colourAuthority;
    private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourNormal;
    private string colourEnd;

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
                    Mathf.Clamp(tempValue, 1, 3);
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
        //NOTE: Internal use only -> use AdjustSecurityLevel() to change Security Level as these will swap material if needed
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

    /// <summary>
    /// initialise
    /// </summary>
    public void Awake()
    {
        listOfOngoingEffects = new List<EffectDataOngoing>();
    }

    public void Start()
    {
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
        mouseOverFade = GameManager.instance.tooltipScript.tooltipFade;
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
    }

    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //select event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        colourRebel = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
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
        if (secLvl != SecurityLevel)
        {
            SecurityLevel = secLvl;
            SetConnectionMaterial(secLvl);
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
            { SetConnectionMaterial(SecurityLevel); }
        }
    }

    /// <summary>
    /// saves level to a field so it can be restored back to the same state later
    /// </summary>
    public void SaveSecurityLevel()
    { securityLevelSave = _securityLevel; }

    /// <summary>
    /// restores previously saved security level and updates connection material
    /// </summary>
    public void RestoreSecurityLevel()
    {
        _securityLevel = securityLevelSave;
        ConnectionType connType;
        switch (_securityLevel)
        {
            case 3:
                connType = ConnectionType.HIGH;
                break;
            case 2:
                connType = ConnectionType.MEDIUM;
                break;
            case 1:
                connType = ConnectionType.LOW;
                break;
            case 0:
            default:
                connType = ConnectionType.None;
                break;
        }
        SetConnectionMaterial(connType);
    }

    /// <summary>
    /// sub method to change connections material (colour)
    /// </summary>
    /// <param name="secLvl"></param>
    public void SetConnectionMaterial(ConnectionType secLvl)
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material = GameManager.instance.connScript.GetConnectionMaterial(secLvl);
    }

    public int GetNode1()
    { return v1; }

    public int GetNode2()
    { return v2; }


    //
    // - - - Ongoing Effects - - -
    //

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
                    GameManager.instance.dataScript.AddOngoingEffectToDict(ongoing, connID);
                }
            }
        }
        else { Debug.LogError("Invalid EffectDataOngoing (Null)"); }
        return isNotDuplicate;
    }

    /// <summary>
    /// checks listOfOngoingEffects for any matching ongoingID and deletes them
    /// </summary>
    /// <param name="uniqueID"></param>
    public void RemoveOngoingEffect(int uniqueID)
    {
        if (listOfOngoingEffects.Count > 0)
        {
            bool isRemoved = false;
            //reverse loop, deleting as you go
            for (int i = listOfOngoingEffects.Count - 1; i >= 0; i--)
            {
                EffectDataOngoing ongoing = listOfOngoingEffects[i];
                if (ongoing.ongoingID == uniqueID)
                {
                    Debug.Log(string.Format("Connection, ID {0}, Ongoing Effect ID {1}, \"{2}\", REMOVED{3}", connID, ongoing.ongoingID, ongoing.text, "\n"));
                    //add to register & create message
                    GameManager.instance.dataScript.RemoveOngoingEffect(ongoing, connID);
                    //remove from list
                    listOfOngoingEffects.RemoveAt(i);
                    isRemoved = true;
                }
            }
            //reset colour to take into account the new security level
            if (isRemoved == true)
            { SetConnectionMaterial(SecurityLevel); }
        }
    }


    /// <summary>
    /// Mouse Over tool tip & Right Click
    /// </summary>
    private void OnMouseOver()
    {
        //check modal block isn't in place
        if (GameManager.instance.guiScript.CheckIsBlocked() == false)
        {
            //Right click node -> Show either move options (node highlights) or Move Menu
            if (Input.GetMouseButtonDown(1) == true)
            {
            }
            //Tool tip
            else
            {
                onMouseFlag = true;
                if (GameManager.instance.optionScript.connectorTooltips == true)
                {
                    side = GameManager.instance.sideScript.PlayerSide;
                    StartCoroutine(ShowTooltip());
                }
            }
        }
    }

    /// <summary>
    /// mouse over exit, shut down tooltip
    /// </summary>
    private void OnMouseExit()
    {
        if (GameManager.instance.guiScript.CheckIsBlocked() == false)
        {
            onMouseFlag = false;
            StopCoroutine("ShowTooltip");
            GameManager.instance.tooltipGenericScript.CloseTooltip();
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
            while (GameManager.instance.tooltipGenericScript.CheckTooltipActive() == false)
            {
                colourSide = colourRebel;
                if (side.level == GameManager.instance.globalScript.sideAuthority.level)
                { colourSide = colourAuthority; }
                //security level
                string connText;
                switch (SecurityLevel)
                {
                    case ConnectionType.HIGH:
                        connText = string.Format("{0}<b>HIGH</b>{1}", colourBad, colourEnd);
                        break;
                    case ConnectionType.MEDIUM:
                        connText = string.Format("{0}<b>MEDIUM</b>{1}", colourNeutral, colourEnd);
                        break;
                    case ConnectionType.LOW:
                        connText = string.Format("{0}<b>LOW</b>{1}", colourGood, colourEnd);
                        break;
                    case ConnectionType.None:
                        connText = string.Format("<b>NONE</b>");
                        break;
                    default:
                        connText = string.Format("<b>Unknown</b>");
                        break;
                }
                //debug data
                StringBuilder builderData = new StringBuilder();
                builderData.AppendFormat("{0}activityTimeKnown      {1}{2}{3}", colourNormal, activityTurnKnown, colourEnd, "\n");
                builderData.AppendFormat("{0}activityTimePossible   {1}{2}{3}", colourNormal, activityTurnPossible, colourEnd, "\n");
                builderData.AppendFormat("{0}activityCountKnown     {1}{2}{3}", colourNormal, activityCountKnown, colourEnd, "\n");
                builderData.AppendFormat("{0}activityCountPossible  {1}{2}", colourNormal, activityCountPossible, colourEnd);
                //ongoing effects
                StringBuilder builderOngoing = new StringBuilder();
                if (listOfOngoingEffects != null & listOfOngoingEffects.Count > 0)
                {
                    foreach (EffectDataOngoing effect in listOfOngoingEffects)
                    {
                        if (builderOngoing.Length > 0) { builderOngoing.AppendLine(); }
                        if (String.IsNullOrEmpty(effect.text) == false)
                        { builderOngoing.AppendFormat("{0}effect.text{1}{2}", colourNeutral, colourEnd, "\n"); }
                        builderOngoing.AppendFormat("Security {0}{1}{2}{3}{4}", effect.value > 0 ? colourGood : colourBad, effect.value > 0 ? "+" : "", effect.value,
                            colourEnd, "\n");
                        builderOngoing.AppendFormat("For {0}{1}{2} more turns", colourNeutral, effect.timer, colourEnd);
                    }
                }
                else { builderOngoing.Append("No Ongoing effects present"); }
                //position
                Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
                //tooltip data
                string tooltipHeader = string.Format("{0}Connection{1}{2}Security Level {3}", colourSide, colourEnd, "\n", connText);
                GameManager.instance.tooltipGenericScript.SetTooltip(builderData.ToString(), screenPos, tooltipHeader, builderOngoing.ToString());
                yield return null;
            }
            //fade in
            float alphaCurrent;
            while (GameManager.instance.tooltipGenericScript.GetOpacity() < 1.0)
            {
                alphaCurrent = GameManager.instance.tooltipGenericScript.GetOpacity();
                alphaCurrent += Time.deltaTime / mouseOverFade;
                GameManager.instance.tooltipGenericScript.SetOpacity(alphaCurrent);
                yield return null;
            }
        }
    }
}
