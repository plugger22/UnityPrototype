using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attached to GameManager Prefab. Provides global settings for tooltips
/// </summary>
public class TooltipManager : MonoBehaviour
{
    [Tooltip("Time in seconds before tooltip activated (commences fade in)")]
    public float tooltipDelay;
    [Tooltip("Time in seconds for tooltip to fade in")]
    public float tooltipFade;
    [Tooltip("How many pixels above object that tooltip will be offset by")]
    public int tooltipOffset; 
}
