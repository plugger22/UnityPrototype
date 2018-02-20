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

    [HideInInspector] public int level;         //assigned dynamically during MetaManager.initialise (0/1/2)
}
