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

    [HideInInspector] public int level;         //assigned dynamically during GearManager.subInitialiseFastAccess (0/1/2/3 for common/rare/unique/metaGame)
}
