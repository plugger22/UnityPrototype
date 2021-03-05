using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Belief.SO that is attached to decision and game mechanics to determine changes in player mood
/// </summary>
[CreateAssetMenu(menuName = "Actor / Belief")]
public class Belief : ScriptableObject
{
    [Tooltip("Ingame name")]
    public string tag;
    [Tooltip("Explanation only, not used Ingame")]
    public string descriptor;
    [Tooltip("Personality Factor that applies")]
    public Factor factor;
    [Tooltip("Positive Player mood effect if factor is this type, negative if the opposite")]
    public GlobalType type;
}
