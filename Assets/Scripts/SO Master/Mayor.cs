using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for City Mayors. Name of SO is the name of the Mayor (surname), eg. "Mayor Bean"
/// </summary>
[CreateAssetMenu(menuName = "Game / Mayor")]
public class Mayor : ScriptableObject
{
    [Tooltip("Faction that the Mayor belongs to. If Mayor in a city this is will be the faction of the city")]
    public Faction faction;
    [Tooltip("Motto of Mayor in 6 words or less")]
    public string motto;
    [Tooltip("Unique trait of the Mayor")]
    public Trait trait;

    [HideInInspector] public int mayorID;

}
