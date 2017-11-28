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
    public int TargetID { get; set; }
    [Tooltip("Keep short, eg. 'Rolling Blackouts'")]
    public string description;                       //eg. "Rolling Blackouts" (keep short)
    public MetaLevel metaLevel;                   //which meta level?
    [Range(1, 3)]
    public int targetLevel = 1;                         //from 1 to 3, lowest to highest. Level 1 can trigger a Level 2 which can trigger a level 3
    public int InfoLevel { get; set; }                        //from 1 to 3 but can be zero in some cases
    public NodeArc nodeArc;                       //which Node Arc it applies to, eg. "Government"
    public int NodeID;                            //assigned once target is live, -1 otherwise
    public Activation activation = Activation.Medium;                        //chance of going live each turn, if active
    public Status TargetStatus { get; set; }      //default status of Dormant
    public bool IsKnownByAI { get; set; }               //is known by the AI?
    public int Timer { get; set; }                      //countdown timer, default '-1' for ignore
    public ActorArc actorArc;                       //actor arc with special bonus for target resolution (max. 1)
    public GearType gearType;
    public List<Effect> listOfGoodEffects;        //all effects (SO's) that happen as a result of target successfully being resolved
    public List<Effect> listOfBadEffects;         //all effects (SO's) that happen as a result of target failing to be resolved

}
