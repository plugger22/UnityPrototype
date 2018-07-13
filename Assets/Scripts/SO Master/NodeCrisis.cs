using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Node Crisis
/// </summary>
[CreateAssetMenu(menuName = "Node / NodeCrisis")]
public class NodeCrisis : ScriptableObject
{
    [HideInInspector] public int nodeCrisisID;               //unique #, zero based, assigned automatically by DataManager.Initialise

    public NodeDatapoint datapoint;
    [Tooltip("Used for shorter description")]
    public string tag;
    [Tooltip("Used for fuller description")]
    [TextArea] public string description;
    
}
