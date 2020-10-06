using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles texts that display on the end of turn billboard. Could be various types, eg. Advertisements, Headlines, etc.
/// </summary>
[CreateAssetMenu(menuName = "Billboard / Billboard")]
public class Billboard : ScriptableObject
{
    [Header("Main")]
    [Tooltip("Category of Billboard")]
    public BillboardType category;

    [Tooltip("Main text, can use '[' & ']' tags for larger size and different colours")]
    [TextArea] public string textTop;
    [Tooltip("Secondary text, shown slanted in a smaller size down the bottom (can't use tags)")]
    [TextArea] public string textBottom;
}
