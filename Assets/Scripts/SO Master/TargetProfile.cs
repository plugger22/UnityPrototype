using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TargetTrigger SO. Name of SO is the name of the TargetTrigger, eg. "Default"
/// </summary>
[CreateAssetMenu(menuName = "Target / TargetProfile")]
public class TargetProfile : ScriptableObject
{

    [Tooltip("Descriptor only, not used in-game")]
    public string descriptor;

    [Tooltip("Activation set-up for target")]
    public TargetTrigger trigger;
    [Tooltip("CUSTOM & REPEAT trigger only (ignore otherwise) -> Number of turns before activation rolls begin")]
    public int delay;
    [Tooltip("CUSTOM & REPEAT trigger only (Ignore otherwise) -> Activation probability")]
    public GlobalChance activation;
    [Tooltip("Applies to ALL -> The number of turns the target is active for before disappearing. Leave at default 999 if you want target to remain on map permanently")]
    public int turnWindow = 999;

    [Tooltip("Applies to All -> If true the target repeats using the same profile as now")]
    public bool isRepeat;
    [Tooltip("Only applicable for REPEAT profiles -> if true the target repeats at same node, otherwise random node")]
    public bool isSameNode;

}
