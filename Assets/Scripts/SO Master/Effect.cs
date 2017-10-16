using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Multippurpose Effects -> Actions, Targets
/// </summary>
[CreateAssetMenu(menuName = "Effect")]
public class Effect : ScriptableObject
{
    public int EffectID { get; set; }                           //dynamically set {Edit -> I don't think this is hooked up yet, need to check]

    public string description;                                  //tooltip, eg. "Stability +1"
    //Criteria that needs to be met for the Effect to apply
    public EffectCriteria criteriaEffect;
    public Comparison criteriaCompare;
    public int criteriaValue;
    //result of effect
    public EffectOutcome effectOutcome;                         //standard effect
    public Result effectResult;                                 //leave as 'None' if there is no specific stat adjustment
    public int effectValue;                                     //leave as '0' if there is no specific stat adjustment
}
