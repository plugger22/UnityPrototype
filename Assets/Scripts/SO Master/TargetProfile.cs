﻿using System.Collections;
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
    [Tooltip("Required for ALL")]
    public GlobalChance activation;
    [Tooltip("Applies to ALL -> The number of turns the target is active for before disappearing. Leave at default 999 if you want target to remain on map permanently")]
    public int turnWindow = 999;

    [Tooltip("Applies to All -> If true the target repeats using the same profile as now")]
    public bool isRepeat;
    [Tooltip("Only applicable for REPEAT profiles -> if true the target repeats at same node, otherwise random node")]
    public bool isSameNode;


    /// <summary>
    /// Data Validation
    /// </summary>
    public void OnEnable()
    {
        Debug.Assert(activation != null, string.Format("Invalid activation (Null) for TargetProfile \"{0}\"", this.name));
        Debug.Assert(trigger != null, string.Format("Invalid trigger (Null) for TargetProfile \"{0}\"", this.name));
        Debug.Assert(turnWindow > 0, string.Format("Invalid turnWindow (Zero) for TargetProfile \"{0}\"", this.name));
    }

}
