using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using packageAPI;

public class Connection : MonoBehaviour {

    //private ConnectionType securityLevel;

    private int v1;                                         //vertice nodeID's for either end of the connection
    private int v2;

    private int _securityLevel;                             //private backing field (int vs. enum)

    private List<EffectDataOngoing> listOfOngoingEffects;   //list of temporary (ongoing) effects impacting on the node

    public int connID;                                      //unique connectionID 
    public bool isDone;                                     //flag used to prevent connection being changed more than once for an effect

    public int VerticeOne { get { return v1; } }
    public int VerticeTwo { get { return v2; } }

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
                for(int i = 0; i < listOfOngoingEffects.Count; i++)
                {
                    if (listOfOngoingEffects[i].ongoingID == ongoing.ongoingID)
                    { isNotDuplicate = false; break; }
                }
                if (isNotDuplicate == true)
                { listOfOngoingEffects.Add(ongoing); }
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
                    Debug.Log(string.Format("Connection, ID {0}, Ongoing Effect ID {1}, \"{2}\", REMOVED{3}", connID,  ongoing.ongoingID, ongoing.text,"\n"));
                    listOfOngoingEffects.RemoveAt(i);
                    isRemoved = true;
                }
            }
            //reset colour to take into account the new security level
            if (isRemoved == true)
            { SetConnectionMaterial(SecurityLevel); }
        }
    }

}
