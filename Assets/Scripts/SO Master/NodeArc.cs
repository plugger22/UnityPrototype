using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Node type. Name of SO is the type of Node Arc, eg. "Corporate"
/// </summary>
[CreateAssetMenu(menuName = "Node / Archetype")]
public class NodeArc : ScriptableObject
{
    [HideInInspector] public int nodeArcID;               //unique #, zero based, assigned automatically by DataManager.Initialise

    //node stats -> base level stats (0 to 3)
    [Tooltip("Values between 0 and 3 only")] [Range(0,3)] public int Stability;
    [Tooltip("Values between 0 and 3 only")] [Range(0, 3)] public int Support;
    [Tooltip("Values between 0 and 3 only")] [Range(0, 3)] public int Security;

    public Sprite sprite;
}
