using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Meta Level. Name of SO indicates level, eg. "City"
/// </summary>
[CreateAssetMenu(menuName = "Global / Global Meta")]
public class GlobalMeta : ScriptableObject
{
    public string descriptor;

    [HideInInspector] public int level;         //assigned dynamically during MetaManager.initialise (0/1/2)
}
