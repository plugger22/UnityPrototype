using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Effect Outcome. Name of SO is the type of outcome, eg. "Stability"
/// </summary>
[CreateAssetMenu(menuName = "Effect / EffectOutcome")]
public class EffectOutcome : ScriptableObject
{
    public string descriptor;
}
