using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// starting or finishing node type for a VIP movement path between two points on map
/// </summary>
[CreateAssetMenu(menuName = "Game / VIP / VipNode")]
public class VipNode : ScriptableObject
{

    [Tooltip("dev descriptor only")]
    [TextArea] public string devDescriptor;

    [Tooltip("If a specific node Arc specify here, ignore otherwise")]
    public NodeArc nodeArc;

}
