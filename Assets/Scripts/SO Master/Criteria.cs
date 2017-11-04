using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Criteria that must be true for an Effect to occur
/// </summary>
[CreateAssetMenu(menuName = "Effect / EffectCriteria")]
public class Criteria : ScriptableObject
{

    public EffectCriteria criteriaEffect;
    public Comparison criteriaCompare;
    public int criteriaValue;
}
