using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Node Crisis
/// </summary>
[CreateAssetMenu(menuName = "Node / NodeCrisis")]
public class NodeCrisis : ScriptableObject
{

    [Tooltip("Node datapoint that the crisis is associated with")]
    public NodeDatapoint datapoint;
    [Tooltip("Used for shorter description")]
    public string tag;
    [Tooltip("Used for fuller description")]
    [TextArea] public string description;
    [Tooltip("Textlist for newsItems when a crisis erupts")]
    public TextList textList;

}
