using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Gear SO. Name of SO is the name of the Gear, eg. "Safe House"
/// </summary>
[CreateAssetMenu(menuName = "Gear / Gear")]
public class Gear : ScriptableObject
{
    [HideInInspector] public int gearID;

    public string description;
    public MetaLevel metaLevel;       //eg. city / state / country

    public GearRarity rarity;       //common / rare / unique
    public GearType type;

    public int data;               //multipurpose datapoint that depends on gear category

    public Sprite sprite;
      

}
