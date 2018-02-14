using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Special Resistance Actions. Name of SO is the type of special action, eg. "GetGear"
/// </summary>
[CreateAssetMenu(menuName = "Action / Special Resistance Action")]
public class ActionSpecial : ScriptableObject
{
    public string descriptor;
}
