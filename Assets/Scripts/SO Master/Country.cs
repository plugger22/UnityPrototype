using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Countries, name of SO is name of Country (Cities are in countries)
/// </summary>
[CreateAssetMenu(menuName = "Game / Country")]
public class Country : ScriptableObject
{
    [Tooltip("Descriptor only")]
    public string colourName;
    [Tooltip("RGB colour that the city name and country are displayed as")]
    [Range(0, 255)] public byte colour_red;
    [Tooltip("RGB colour that the city name and country are displayed as")]
    [Range(0, 255)] public byte colour_green;
    [Tooltip("RGB colour that the city name and country are displayed as")]
    [Range(0, 255)] public byte colour_blue;

    [Tooltip("Names of all cities in this country are derived from this name set")]
    public NameSet nameSet;
}
