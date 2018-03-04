using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Side. Name of SO indicates Side, eg. "Resistance"
/// </summary>
[CreateAssetMenu(menuName = "Global / Global Side")]
public class GlobalSide : ScriptableObject
{
    public string descriptor;

    [HideInInspector] public int level;         //assigned dynamically during GlobaManager.initialise (0 - AI / 1 - Authority / 2 - Resistance)
}
