using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using System;

/// <summary>
/// Target SO. Name of SO is the name of the Target, eg. "Power Grid"
/// </summary>
[CreateAssetMenu(menuName = "Target / Target")]
public class Target : ScriptableObject
{
    [Header("Base data")]
    [Tooltip("Name of target, used in-game")]
    public string targetName;
    [Tooltip("Keep short, indicates Opportunity eg. 'Grind traffic to a half', Resistance POV")]
    public string descriptorResistance;
    [Tooltip("Keep short, indicates suspected vulnerability eg. 'Possible Security Break', Authority POV")]
    public string descriptorAuthority;
    [Tooltip("Onging effects and Objectives. In format '[due to]...', keep short.")]
    public string reasonText;
    [Tooltip("Rumour text in format '[they'll be a chance soon to]....")]
    public string rumourText;
    [Tooltip("Base targets are level 1, follow-on targets in a sequence are numbered consecutively higher")]
    [Range(1, 5)] public int targetLevel = 1;

    [Header("Target Resolution")]
    [Tooltip("Actor arc with special bonus for target resolution (max. 1)")]
    public ActorArc actorArc;
    [Tooltip("Gear arc with special bonus for target resolution (max. 1)")]
    public GearType gear;
    [Tooltip("Target pic")]
    public Sprite sprite;

    [Header("Category")]
    public TargetType targetType;
    [Tooltip("Which Node Arc it applies to, eg. 'Government' -> Only applies if a Generic type target, ignore otherwise")]
    public NodeArc nodeArc;

    [Header("Effects")]
    [Tooltip("All GOOD effects (SO's) that happen as a result of target successfully being resolved")]
    public List<Effect> listOfGoodEffects;
    [Tooltip("All BAD effects (SO's) that happen as a result of target successfully being resolved")]
    public List<Effect> listOfBadEffects;
    [Tooltip("All effects (SO's) that happen when a target is attempted and the attempt Fails")]
    public List<Effect> listOfFailEffects;

    [Header("Mood Effect only")]
    [Tooltip("Effect that may change Player's mood IF Player successfully attempts the target")]
    public Effect moodEffect;

    [Header("Ongoing Effect only")]
    [Tooltip("ONGOING effect (SO's) that happen each turn once target resolved, status.Completed, until target is status.Contained. Currently MAX of ONE effect allowed. Must be 'Ongoing' effect")]
    public Effect ongoingEffect;

    [Header("Profile")]
    [Tooltip("Base Profile that each target starts a level with")]
    public TargetProfile profileBase;
    

    [Header("Linked Sequence")]
    [Tooltip("If true then a FOLLOW ON target will only be executed if the target has been successfully completed. If false then executed regardless. Ignored for REPEAT targets")]
    public bool isSuccessNeeded = true;
    [Tooltip("Target that follows this one, next in a possible sequence. Can be Ignored if you want a 'Oncer'")]
    public Target followOnTarget;
    
    [HideInInspector] public TargetProfile profile;                 //Profile that used in code

    #region Save Compatible Data
    [HideInInspector] public Status targetStatus;                   //default status of Dormant
    [HideInInspector] public int intel;                             //from 1 to 3, default 0. Gained by Planner action
    [HideInInspector] public int ongoingID;                         //unique ID used to link to ongoing effects, default '0', only valid if > -1
    [HideInInspector] public bool isKnownByAI;                      //is known by the AI?
    [HideInInspector] public int nodeID;                            //assigned once target is live, -1 otherwise
    [HideInInspector] public int distance;                          //distance to current node (used for Planner). Dynamic (targetManager.cs -> InitialiseGenericPickerTargetInfo. Can be ignored.
    [HideInInspector] public int newIntel;                          //new level of intel after gain, takes into account max cap and existing intel level. Dynamic. Can be ignored. 
    [HideInInspector] public int intelGain;                         //amount of intel gained (info) if selected in generic picker. Dynamic. Can be ignored 
    //Tracking data
    [HideInInspector] public int turnSuccess;                       //turn # when target successfully attempted, -1 default
    [HideInInspector] public int turnDone;                          //turn # when target done, -1 default
    [HideInInspector] public int numOfAttempts;                     //how many attempts made on target, 0 default
    [HideInInspector] public int turnsWindow;                       //number of turns of Live window (info purposes only)
    //Timers
    [HideInInspector] public int timerDelay;                        //delay in turns before target tests for activation
    [HideInInspector] public int timerHardLimit;                    //back stop timer, triggered once activations commence. If target hasn't activated randomly by the time timer reaches zero then does so
    [HideInInspector] public int timerWindow;                       //number of turns target, once live, stays that way before disappearing, set to turnWindow on activation
    //Contact Rumours
    [HideInInspector] public List<int> listOfRumourContacts = new List<int>();  //list of all contactID's who have heard a rumour about this target (Active targets only)
    #endregion

    /// <summary>
    /// Data Validation
    /// </summary>
    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(targetName) == false, "Target {0} Invalid targetName (Null or Empty)", name);
        Debug.AssertFormat(string.IsNullOrEmpty(descriptorResistance) == false, "Target {0} Invalid description (Null or Empty)", name);
        Debug.AssertFormat(string.IsNullOrEmpty(descriptorAuthority) == false, "Target {0} Invalid descriptorAuthority (Null or Empty)", name);
        Debug.AssertFormat(string.IsNullOrEmpty(reasonText) == false, "Target {0} Invalid reasonText (Null or Empty)", name);
        Debug.AssertFormat(string.IsNullOrEmpty(rumourText) == false, "Target {0} Invalid rumourText (Null or Empty)", name);
        Debug.AssertFormat(actorArc != null, "Target {0} Invalid actorArc (Null)", name);
        Debug.AssertFormat(gear != null, "Target {0} Invalid gear (Null)", name);
        Debug.AssertFormat(sprite != null, "Target {0} Invalid sprite (Null)", name);
        Debug.AssertFormat(targetType != null, "Target {0} Invalid targetType (Null)", name);
        //nodeArc needed only if targetType Generic
        if (targetType.name.Equals("Generic", StringComparison.Ordinal) == true)
        { Debug.AssertFormat(nodeArc != null, "Target {0} Invalid nodeArc (Null)", name); }

        /*//list of effects
        Debug.AssertFormat(listOfGoodEffects.Count > 0, "Target {0} Invalid listOfGoodEffects (Empty)", name);
        Debug.AssertFormat(listOfBadEffects.Count > 0, "Target {0} Invalid listOfBadEffects (Empty)", name);
        Debug.AssertFormat(listOfFailEffects.Count > 0, "Target {0} Invalid listOfFailEffects (Empty)", name);*/

        Debug.AssertFormat(profileBase != null, "Target {0} has no profileBase (Null)", name);
        //NOTE: No need to check profile for Null as handled in TargetManager.cs -> SetTargetDetails (assigns defaultProfile if null)
    }

    /// <summary>
    /// Add a contact who has learned of a rumour about the target
    /// </summary>
    /// <param name="contactID"></param>
    public void AddContactRumour(int contactID)
    {
        Debug.Assert(contactID > -1, "Invalid contactID (less than zero)");
        listOfRumourContacts.Add(contactID);
    }

    /// <summary>
    /// Checks listOfRmourContacts and returns true if the contact has already learnt a rumour about target
    /// </summary>
    /// <param name="contactID"></param>
    /// <returns></returns>
    public bool CheckRumourContact(int contactID)
    { return listOfRumourContacts.Exists(x => x == contactID); }

    /// <summary>
    /// Resets basic fields, statistics and flags
    /// </summary>
    public void Reset()
    {
        isKnownByAI = false;
        turnSuccess = -1;
        turnDone = -1;
        numOfAttempts = 0;
        ongoingID = -1;
        intel = 0;
    }

}
