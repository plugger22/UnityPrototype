﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Node Datapoint. Name of SO is the type of Node datapoint, eg. "Security"
/// </summary>
[CreateAssetMenu(menuName = "Node / NodeDatapoint")]
public class NodeDatapoint : ScriptableObject
{
    [HideInInspector] public int nodeDataID;               //unique #, zero based, assigned automatically by DataManager.Initialise

    [HideInInspector] public int level;                    //dynamically assigned by GlobalManager.cs -> Stability 0, Support 1, Security 2
}
