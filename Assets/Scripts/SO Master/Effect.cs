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
    [Tooltip("Keep short as used for the tooltip. Status effects: keep to text only as '+1' added dynamically during import, all other Effects should be complete")]
    public string description;                                  //tooltip, eg. "Stability"
    [HideInInspector] public string textTag;                    //text that is used in game. Dynamically assigned at time of import (DataManager.cs)
    //criteria 
    [Tooltip("Any criterial that must be satisfied before the effect can be applied")]
    public List<Criteria> listOfCriteria;                       //list of effect criteria that must be true for an effect to occur
    //result of effect
    [Tooltip("Applies to the current node, the current plus all neighbouring nodes, all nodes or nodes of the same type as the current one")]
    public EffectApply apply;                                   //who does the effect apply to?
    [Tooltip("What the effect applies to")]
    public EffectOutcome outcome;
    [Tooltip("Use Add or Subtract if appropriate, otherwise ignore")]
    public EffectOperator operand;                              //leave as 'None' if there is no specific stat adjustment
    [Tooltip("The effect from the point of view of the Resistance")]
    public EffectType type;                                     //used so that ActorManager.cs -> GetActorActions can colour the effects correctly for the tooltips
    [Tooltip("Single, one shot, effect or Ongoing?")]
    public EffectDuration duration;                             //once off effect or ongoing?
    [Tooltip("Value is always POSITIVE. Use operand 'Subtract' to handle negative numbers")]
    public int value;                                           //leave as '0' if there is no specific stat adjustment

    [Tooltip("True only for cases where a ModalGenericPicker is needed rather than a straight ProcessEffect. False by default. Negates normal effect processing")]
    public bool ignoreEffect;                           


}
