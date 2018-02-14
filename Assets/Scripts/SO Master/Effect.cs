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
    [HideInInspector] public int effectID;                      //dynamically set by DataManager.cs

    public string description;                                  //tooltip, eg. "Stability +1"
    //criteria 
    public List<Criteria> listOfCriteria;                       //list of effect criteria that must be true for an effect to occur
    //result of effect
    public EffectApply apply;                                   //who does the effect apply to?
    public EffectOutcome outcome;
    public EffectOperator operand;                              //leave as 'None' if there is no specific stat adjustment
    public EffectType type;                                     //used so that ActorManager.cs -> GetActorActions can colour the effects correctly for the tooltips
    public EffectDuration duration;                             //once off effect or ongoing?

    public int value;                                           //leave as '0' if there is no specific stat adjustment

    [Tooltip("True only for cases where a ModalGenericPicker is needed rather than a straight ProcessEffect. False by default. Negates normal effect processing")]
    public bool ignoreEffect;                           


}
