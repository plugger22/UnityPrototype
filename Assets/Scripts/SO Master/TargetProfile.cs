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
    [Tooltip("Number of turns before activation rolls begin -> Ignored for LIVE")]
    public int delay;
    [Tooltip("Chance of activating per turn (after delay countdown). Ignored for LIVE")]
    public GlobalChance activation;
    [Tooltip("The number of turns the target is active for before disappearing. Leave at default 999 if you want target to remain on map permanently")]
    public int window = 999;

    [Tooltip("Applies to All -> If true the target repeats using the same profile as now")]
    public bool isRepeat;
    [Tooltip("Only applicable for REPEAT profiles -> if true the target repeats at same node, otherwise random node")]
    public bool isRepeatSameNode;
    [Tooltip("If a repeat target you need to specify a Repeat Profile, ignore otherwise")]
    public TargetProfile repeatProfile;


    /// <summary>
    /// Data Validation
    /// </summary>
    public void OnEnable()
    {
        Debug.AssertFormat(activation != null, "Invalid activation (Null) for TargetProfile \"{0}\"", name);
        Debug.AssertFormat(trigger != null, "Invalid trigger (Null) for TargetProfile \"{0}\"", name);
        Debug.AssertFormat(window > 0, "Invalid turnWindow (Zero) for TargetProfile \"{0}\"", name);
        if (isRepeat == true)
        { Debug.Assert(repeatProfile != null, "Invalid repeatProfile (Null)"); }
    }

}
