using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Gear Rarity. Name of SO indicates level of rarity, eg. "Rare"
/// </summary>
[CreateAssetMenu(menuName = "Gear / Gear Rarity")]
public class GearRarity : ScriptableObject
{
    public string descriptor;
    [Tooltip("The level of gear -> '0' for Low, '1' for Mid, '2' for High")]
    [Range(0, 2)] public int level;           //0 to 2
}
