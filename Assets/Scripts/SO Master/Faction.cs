using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Factions. Name of SO is the name of the faction, eg. "Nihilists"
/// </summary>
[CreateAssetMenu(menuName = "Game / Faction")]
public class Faction : ScriptableObject
{
    public string descriptor;
    public GlobalSide side;
    [Tooltip("Node type that faction wants to protect (authority) or turn a blind eye to (resistance). Leave blank if none.")]
    public NodeArc preferredArc; 
    [Tooltip("Node type that faction wants to destroy (resistance) or ignore (authority). Leave blank if none.")]
    public NodeArc hostileArc;
    [Tooltip("How many actions the AI faction can carry out per turn")]
    [Range(1,3)] public int maxTaskPerTurn = 3;
    [Tooltip("Unique trait of the Faction")]
    public Trait trait;

    [HideInInspector] public int factionID;         //dynamically assigned by DataManager.cs on import
}
