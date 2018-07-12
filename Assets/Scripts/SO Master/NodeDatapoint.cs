using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Node Datapoint. Name of SO is the type of Node datapoint, eg. "Security"
/// </summary>
[CreateAssetMenu(menuName = "Node / NodeDatapoint")]
public class NodeDatapoint : ScriptableObject
{
    [HideInInspector] public int nodeDataID;               //unique #, zero based, assigned automatically by DataManager.Initialise
}
