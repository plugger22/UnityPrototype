using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Target SO. Name of SO is the name of the Target, eg. "Power Grid"
/// </summary>
[CreateAssetMenu(menuName = "Target")]
public class Target : ScriptableObject
{
    
    [Tooltip("Keep short, eg. 'Rolling Blackouts'")]
    public string description;                       //eg. "Rolling Blackouts" (keep short)
    public MetaLevel metaLevel;                   //which meta level?
    [Range(1, 3)] public int targetLevel = 1;                         //from 1 to 3, lowest to highest. Level 1 can trigger a Level 2 which can trigger a level 3
    public NodeArc nodeArc;                       //which Node Arc it applies to, eg. "Government"
    
    public Activation activation = Activation.Medium;                        //chance of going live each turn, if active
    public ActorArc actorArc;                       //actor arc with special bonus for target resolution (max. 1)
    public GearType gear;

    [HideInInspector] public Status targetStatus;      //default status of Dormant
    [HideInInspector] public int infoLevel;                        //from 1 to 3 but can be zero in some cases
    [HideInInspector] public int targetID;
    [HideInInspector] public int ongoingID;         //unique ID used to link to ongoing effects, default '0', only valid if > -1
    [HideInInspector] public bool isKnownByAI;               //is known by the AI?
    [HideInInspector] public int timer;                      //countdown timer, default '-1' for ignore
    [HideInInspector] public int nodeID;                            //assigned once target is live, -1 otherwise

    //effects
    public List<Effect> listOfGoodEffects;        //all effects (SO's) that happen as a result of target successfully being resolved
    public List<Effect> listOfBadEffects;         //all effects (SO's) that happen as a result of target successfully being resolved
    public List<Effect> listOfOngoingEffects;     //all effects (SO's) that happen each turn once target resolved, status.Completed, until target is status.Contained.

    

}
