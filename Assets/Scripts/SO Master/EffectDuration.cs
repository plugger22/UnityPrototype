using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Node type. Name of SO is the type of Node Arc, eg. "Corporate"
/// </summary>
[CreateAssetMenu(menuName = "Effect / EffectDuration")]
public class EffectDuration : ScriptableObject
{
    public string descriptor;
}
