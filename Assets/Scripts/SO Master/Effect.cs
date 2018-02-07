using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Multippurpose Effects -> Actions, Targets
/// </summary>
[CreateAssetMenu(menuName = "Effect / Effect")]
public class Effect : ScriptableObject
{
    public int EffectID { get; set; }                           //dynamically set {Edit -> I don't think this is hooked up yet, need to check]

    public string description;                                  //tooltip, eg. "Stability +1"
    public Side side;                                           //which side does the effect apply to?

    public List<Criteria> listOfCriteria;                       //list of effect criteria that must be true for an effect to occur

    //result of effect
    public EffectCategory category;                       //which group does it belong to? For Data Validation purposes
    public EffectApply apply;                             //to whom does the effect apply? Only for Target effects, ignore for the rest
    public EffectOutcome outcome;                         //standard effect
    public Result result;                                 //leave as 'None' if there is no specific stat adjustment
    public EffectType type;                               //used so that ActorManager.cs -> GetActorActions can colour the effects correctly for the tooltips
    //public EffectDurationEnum durationEnum;                       //once off effect or ongoing?
    public EffectDuration duration;

    public int value;                                     //leave as '0' if there is no specific stat adjustment
    [Tooltip("True only for cases where a ModalGenericPicker is needed rather than a straight ProcessEffect. False by default. Negates normal effect processing")]
    public bool ignoreEffect;                           


}
