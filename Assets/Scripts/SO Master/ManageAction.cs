using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Actor Managment Actions (Dismiss/Reserve/Dispose options). Name of SO is the type of  action, eg. "DismissIncompetent"
/// </summary>
[CreateAssetMenu(menuName = "Manage / Action")]
public class ManageAction : ScriptableObject
{
    public string descriptor;
    public ManageActor manage;
    public Sprite sprite;
    public string optionTitle;
    public string tooltipHeader;
    public string tooltipMain;
    public string tooltipDetails;
    [Tooltip("The order (low to high) that the options appear (left to right) in the Generic Picker. Aim to give everyone in a set a unique number")]
    [Range(1,3)] public int order;
}
