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
    public GearLevel rarity;
    public MetaLevel metaLevel;
      

}
