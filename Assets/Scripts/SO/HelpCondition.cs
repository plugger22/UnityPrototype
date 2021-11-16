using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Conditions for HelpMessages.SO in order for them to be context sensitive, if required (NOTE: HelpConditions are a separate mechanic to Effect Conditions)
/// </summary>
[CreateAssetMenu(menuName = "Help / HelpCondition")]
public class HelpCondition : ScriptableObject
{
    [Tooltip("Descriptor only. Not used in-game")]
    [TextArea] public string descriptor;

    [Header("Comparison")]
    [Tooltip("Use ONE of these only (if multiple only first from top to bottom will be used. Optional")]
    public bool isEquals;
    [Tooltip("Use ONE of these only (if multiple only first from top to bottom will be used. Optional")]
    public bool isGreaterThan;
    [Tooltip("Use ONE of these only (if multiple only first from top to bottom will be used. Optional")]
    public bool isLessThan;

    [Header("Availability")]
    [Tooltip("Item or person is present in the correct context, eg. OnMap, inventory, reserves, etc. Can be EITHER present or notPresent. Optional")]
    public bool isPresent;
    [Tooltip("Item or person is NOT present in the correct context (OnMap / Inventory / Reserves etc). Can be EITHER present or notPresent. Optional")]
    public bool isNotPresent;

    [Header("Limits")]
    [Tooltip("Lower limit, inclusive, where this + value allows the condition to kick in. Optional")]
    public int lowerLimit = -1;
    [Tooltip("Upper limit - inclusive, where, past this + value, the condition becomes invalid. Optional")]
    public int upperLimit = -1;

    [Header("Set Value")]
    [Tooltip("A set number that applies. Eg. equals 2. You have have limits or a Set Value but not both. Optional")]
    public int setNumber = -1;
}
