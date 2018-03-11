using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Factions. Name of SO is the name of the faction, eg. "Nihilists"
/// </summary>
[CreateAssetMenu(menuName = "Faction / Faction")]
public class Faction : ScriptableObject
{
    public string descriptor;
    public GlobalSide side;

    [HideInInspector] public int factionID;         //dynamically assigned by DataManager.cs on import
}
