using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Meta Level. Name of SO indicates type, eg. "Good"
/// </summary>
[CreateAssetMenu(menuName = "Global / Global Type")]
public class GlobalType : ScriptableObject
{
    public string descriptor;

    [HideInInspector] public int level;         //assigned dynamically during MetaManager.initialise (0/1/2)
}
