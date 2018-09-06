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
    [Tooltip("Used for InfoApp itemText in format '[Actor] ... [Condition]', eg. 'is', 'is a', etc.")]
    public string itemTag;
    [Tooltip("Used for InfoApp details top Text in format '[Actor] ...")]
    [TextArea] public string topText;
    [Tooltip("Used for InfoApp details bottom Text (self Contained)")]
    [TextArea] public string bottomText;
    [Tooltip("From Point of View of Actor (should be the same for both sides")]
    public GlobalType type;
}
