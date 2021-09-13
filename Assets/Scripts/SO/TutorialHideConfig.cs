using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Specifies Hide and Seek items (Spiders and Tracers) for the map for a tutorial set
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialHideConfig")]
public class TutorialHideConfig : ScriptableObject
{
    [Header("Number")]
    [Tooltip("Number of Spiders onMap at start of a tutorial Set")]
    [Range(0, 6)] public int numOfSpiders;
    [Tooltip("Number of Tracers onMap at start of a tutorial Set")]
    [Range(0, 6)] public int numOfTracers;

    [Header("Placement")]
    [Tooltip("If true then random Spider node placement, if false then most connected nodes placement")]
    public bool isRandomSpiderNodes;
    [Tooltip("If true then random Tracer node placement, if false then most connected nodes placement")]
    public bool isRandomTracerNodes;

}
