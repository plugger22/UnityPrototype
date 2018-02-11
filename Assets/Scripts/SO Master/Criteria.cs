using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Criteria that must be true for an Effect to occur
/// </summary>
[CreateAssetMenu(menuName = "Effect / Criteria")]
public class Criteria : ScriptableObject
{

    //public EffectCriteriaEnum criteriaEffect;
    public Comparison criteriaCompare;
    public int criteriaValue;
    public EffectCriteria effectCriteria;
}
