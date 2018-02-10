using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Effect Apply. Name of SO is who the effect applies to, eg. "NodeCurrent"
/// </summary>
[CreateAssetMenu(menuName = "Effect / EffectApply")]
public class EffectApply : ScriptableObject
{
    public string descriptor;
}
