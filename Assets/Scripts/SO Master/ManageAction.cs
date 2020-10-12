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
    //public string tooltipHeader;
    [Tooltip("Word as a complete sentence but DO NOT refer to the actor. Keep it general, eg. 'They have to go, there is no way around it'")]
    public string tooltipMain;
    [Tooltip("Word it in the format of <Actor name + space> 'xxxxxxxx', eg. 'is Fired!'")]
    public string tooltipDetails;
    [Tooltip("The order (low to high) that the options appear (left to right) in the Generic Picker. Aim to give everyone in a set a unique number")]
    [Range(1,3)] public int order;
    [Tooltip("Any criteria that must be valid (ALL of them) in order for the option to be active (greyed out otherwise)")]
    public List<Criteria> listOfCriteria;
    [Tooltip("Effects that occur when this Manage Action is selected")]
    public List<Effect> listOfEffects;
    [Tooltip("If the action requires a renown cost to use then True, otherwise default false. Cost is added to the tooltip dynamically")]
    public bool isPowerCost;

}
