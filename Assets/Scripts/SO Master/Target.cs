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
    [Tooltip("Keep short, eg. 'Rolling Blackouts', keep short")]
    public string description;
    [Tooltip("In format '[due to]...', keep short")]
    public string reason;
    [Tooltip("Only select an option here if the Target is restricted to a particular metaLevel, otherwise leave as None (null)")]
    public GlobalMeta metaLevel;
    [Tooltip("From 1 to 3, lowest to highest. Level 1 can trigger a Level 2 which can trigger a level 3")]
    [Range(1, 3)] public int targetLevel = 1;

    [Header("Target Resolution")]
    [Tooltip("Actor arc with special bonus for target resolution (max. 1)")]
    public ActorArc actorArc;
    [Tooltip("Gear arc with special bonus for target resolution (max. 1)")]
    public GearType gear;
    [Tooltip("Target pic")]
    public Sprite sprite;

    [Header("Category")]
    public TargetType type;
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

    [Header("Linked Sequence")]
    [Tooltip("GENERIC Targets Only (set via Mission SO for others) -> Generic mission that follows this one. Can have a sequence level 1 - 2 - 3, etc. Optional")]
    public Target followOnTarget;
    

    [HideInInspector] public Status targetStatus;      //default status of Dormant
    [HideInInspector] public GlobalChance activation;               //chance of activating each turn, once live
    [HideInInspector] public int infoLevel;                        //from 1 to 3 but can be zero in some cases
    [HideInInspector] public int targetID;
    [HideInInspector] public int ongoingID;         //unique ID used to link to ongoing effects, default '0', only valid if > -1
    [HideInInspector] public bool isKnownByAI;               //is known by the AI?
    [HideInInspector] public int nodeID;                            //assigned once target is live, -1 otherwise
    [HideInInspector] public int turnWindow;                        //number of turns target is live and visibile before disappearing
    [HideInInspector] public int turnSuccess;                       //turn # when target successfully attempted
    [HideInInspector] public int turnDone;                          //turn # when target done
    [HideInInspector] public int nextTargetID;                      //if target completed, this is the next target to process, only valid if > -1
    [HideInInspector] public bool isRepeat;                         //if true target will repeat at same node using the same profile until target is completed or level times out
    [HideInInspector] public bool isSameNode;                       //Only applies if a Repeating target -> if true then target repeats at same node, otherwise at a random node

    //Timers
    [HideInInspector] public int timerDelay;                        //delay in turns before target tests for activation
    [HideInInspector] public int timerCountdown;                    //back stop timer, triggered once activations commence. If target hasn't activated randomly by the time timer reaches zero then does so
    [HideInInspector] public int timerWindow;                       //number of turns target, once live, stays that way before disappearing, set to turnWindow on activation

}
