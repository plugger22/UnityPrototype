using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Authority Nemesis damage type. Name of SO is the name of the damage, eg. "Wound"
/// </summary>
[CreateAssetMenu(menuName = "Authority / Damage")]
public class Damage : ScriptableObject
{
    [Tooltip("In game tooltiop")]
    public string descriptor;

    [Tooltip("Used in Messages, format '[You have been] ...")]
    public string tag;

    [Tooltip("Used in Messages, keep short, self contained explanation of damage effects")]
    [TextArea] public string effect;
}
