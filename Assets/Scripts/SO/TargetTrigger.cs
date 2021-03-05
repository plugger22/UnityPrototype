using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TargetTrigger SO. Name of SO is the name of the TargetTrigger, eg. "Default"
/// </summary>
[CreateAssetMenu(menuName = "Target / TargetTrigger")]
public class TargetTrigger : ScriptableObject
{

    [Tooltip("Descriptor only, not used in game")]
    public string descriptor;
}
