using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Effect Criteria. Name of SO is the type of criteria, eg. "NodeStability"
/// NOTE that 'Criteria.cs' is the actual criteria whereas EffectCriteria is the game data being checked
/// </summary>
[CreateAssetMenu(menuName = "Effect / EffectCriteria")]
public class EffectCriteria : ScriptableObject
{
    public string descriptor;
}
