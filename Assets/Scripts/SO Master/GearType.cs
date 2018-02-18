using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Gear Type. Name of SO indicates type, eg. "Infiltration"
/// </summary>
[CreateAssetMenu(menuName = "Gear / Gear Type")]
public class GearType : ScriptableObject
{
    public string descriptor;
}