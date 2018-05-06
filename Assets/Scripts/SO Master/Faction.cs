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
    [Tooltip("How many AI Tasks can the faction undertake in any given turn")]
    [Range(1,5)] public int maxTaskPerTurn = 3;

    [HideInInspector] public int factionID;         //dynamically assigned by DataManager.cs on import
}
