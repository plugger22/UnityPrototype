using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Effect Type. Name of SO is the type, eg. "Good"
/// </summary>
[CreateAssetMenu(menuName = "Effect / EffectType")]
public class EffectType : ScriptableObject
{
    public string descriptor;
}
