using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Gear SO. Name of SO is the name of the Gear, eg. "Safe House"
/// </summary>
[CreateAssetMenu(menuName = "Gear")]
public class Gear : ScriptableObject
{
    [HideInInspector] public int gearID;

    public string description;
    public GearLevel rarity;        // 1 -> common, 2 -> rare, 3 -> unique (one off)
    public GearType type;
    public MetaLevel metaLevel;       //eg. city / state / country

    public Sprite sprite;
      

}
