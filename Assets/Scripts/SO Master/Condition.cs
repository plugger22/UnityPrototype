using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Actor Conditions that can be gained during the course of the game. Name of SO is the type of the condition, eg. "Stressed"
/// </summary>
[CreateAssetMenu(menuName = "Actor / Condition")]
public class Condition : ScriptableObject
{
    public string descriptor;
    [Tooltip("Used for when actor resigns over Player having a bad condition. Format -> [actor] Resigns (due to Player's '....'). Ignore if not applicable")]
    public string resignTag;
    [Tooltip("From Point of View of Actor (should be the same for both sides")]
    public GlobalType type;
}
