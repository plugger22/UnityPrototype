using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// starting or finishing node type for a Npc movement path between two points on map
/// </summary>
[CreateAssetMenu(menuName = "Game / Npc / NpcNode")]
public class NpcNode : ScriptableObject
{
    [Tooltip("Used in format '[They will arriving at] ...', e.g 'the Airport', 'a Corporate district', for Random[x] leave as null so code can pick this up and deal with it")]
    public string arriving;
    [Tooltip("Used in format '[We expect them to visit] ...', e.g 'Corporate districts', for Random[x] leave as null so code can pick this up and deal with it ")]
    public string visiting;
    [Tooltip("If a specific node Arc specify here, ignore otherwise")]
    public NodeArc nodeArc;

}
