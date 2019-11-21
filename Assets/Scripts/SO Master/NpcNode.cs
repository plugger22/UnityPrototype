using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// starting or finishing node type for a Npc movement path between two points on map
/// </summary>
[CreateAssetMenu(menuName = "Game / Npc / NpcNode")]
public class NpcNode : ScriptableObject
{

    [Tooltip("If a specific node Arc specify here, ignore otherwise")]
    public NodeArc nodeArc;

}
