using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Global chance. Name of SO indicates chance, eg. "Medium"
/// </summary>
[CreateAssetMenu(menuName = "Global / Global Chance")]
public class GlobalChance : ScriptableObject
{
    public string descriptor;

    [HideInInspector] public int level;         //assigned dynamically during DataManager.initialise (0/1/2)
}
