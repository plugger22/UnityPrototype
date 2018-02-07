using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Effect Duration. Name of SO is the type of duration, eg. "Ongoing"
/// </summary>
[CreateAssetMenu(menuName = "Effect / EffectDuration")]
public class EffectDuration : ScriptableObject
{
    public string descriptor;
}
