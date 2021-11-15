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

    [Header("Limit")]
    [Tooltip("Use ONE of these only (if multiple only first from top to bottom will be used. Defines limit of condition. Eg. if == 2 then triggered when condition.value (code) is 2. Default -1")]
    public int isEquals = -1;
    [Tooltip("Use ONE of these only (if multiple only first from top to bottom will be used. Defines limit of condition. Eg. if > 2 then triggered when condition.value (code) > value + 2. Default -1")]
    public int isGreaterThan = -1;
    [Tooltip("Use ONE of these only (if multiple only first from top to bottom will be used. Defines limit of condition. Eg. if < 2 then triggered when condition.value (code) is < value - 2. Default -1")]
    public int isLessThan = -1;
}
