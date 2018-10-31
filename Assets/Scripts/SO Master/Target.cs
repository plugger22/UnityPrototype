using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

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
    [Tooltip("In format '[due to]...', keep short")]
    public string reason;
    [Tooltip("Only select an option here if the Target is restricted to a particular metaLevel, otherwise leave as None (null)")]
    public GlobalMeta metaLevel;
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
    [Tooltip("ONGOING effect (SO's) that happen each turn once target resolved, status.Completed, until target is status.Contained. Currently MAX of ONE effect allowed")]
    public Effect OngoingEffect;

    [Header("Profile")]
    [Tooltip("Base Profile that each target starts a level with")]
    public TargetProfile profileBase;
    

    [Header("Linked Sequence")]
    [Tooltip("If true then a FOLLOW ON target will only be executed if the target has been successfully completed. If false then executed regardless. Ignored for REPEAT targets")]
    public bool isSuccessNeeded = true;
    [Tooltip("Target that follows this one, next in a possible sequence. Can be Ignored if you want a 'Oncer'")]
    public Target followOnTarget;
    
    [HideInInspector] public TargetProfile profile;                 //Profile that used in code
    [HideInInspector] public Status targetStatus;                   //default status of Dormant
    /*[HideInInspector] public GlobalChance activation;               //chance of activating each turn, once live*/
    [HideInInspector] public int infoLevel;                         //from 1 to 3 but can be zero in some cases
    [HideInInspector] public int targetID;
    [HideInInspector] public int ongoingID;                         //unique ID used to link to ongoing effects, default '0', only valid if > -1
    [HideInInspector] public bool isKnownByAI;                      //is known by the AI?
    [HideInInspector] public int nodeID;                            //assigned once target is live, -1 otherwise
    /*[HideInInspector] public bool isRepeat;                         //if true target will repeat at same node using the same profile until target is completed or level times out
    [HideInInspector] public bool isSameNode;                       //Only applies if a Repeating target -> if true then target repeats at same node, otherwise at a random node*/

    //Tracking data
    [HideInInspector] public int turnSuccess;                       //turn # when target successfully attempted, -1 default
    [HideInInspector] public int turnDone;                          //turn # when target done, -1 default
    [HideInInspector] public int numOfAttempts;                     //how many attempts made on target, 0 default
    [HideInInspector] public int turnsWindow;                       //number of turns of Live window (info purposes only)
    //Timers
    [HideInInspector] public int timerDelay;                        //delay in turns before target tests for activation
    [HideInInspector] public int timerHardLimit;                    //back stop timer, triggered once activations commence. If target hasn't activated randomly by the time timer reaches zero then does so
    [HideInInspector] public int timerWindow;                       //number of turns target, once live, stays that way before disappearing, set to turnWindow on activation

    /// <summary>
    /// Data Validation
    /// </summary>
    public void OnEnable()
    {
        Debug.Assert(string.IsNullOrEmpty(descriptorResistance) == false, "Invalid description (Null or Empty)");
        Debug.Assert(string.IsNullOrEmpty(descriptorAuthority) == false, "Invalid descriptorAuthority (Null or Empty)");
        Debug.Assert(profileBase != null, string.Format("Target {0}, id {1}, has no profileBase (Null)", targetName, targetID));
        //NOTE: No need to check profile for Null as handled in TargetManager.cs -> SetTargetDetails (assigns defaultProfile if null)
    }
}
