using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Effect Criteria. Name of SO is the type of criteria, eg. "NodeStability"
/// </summary>
[CreateAssetMenu(menuName = "Effect / EffectCriteria")]
public class EffectCriteria : ScriptableObject
{
    public string descriptor;
}
