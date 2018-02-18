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
    public GearLevel rarity;        // 1 -> common, 2 -> rare, 3 -> unique (one off)
    public GearTypeEnum type;
    public MetaLevel metaLevel;       //eg. city / state / country

    public GearRarity gearRarity;
    public GearType gearType;

    public int data;               //multipurpose datapoint that depends on gear category

    public Sprite sprite;
      

}
