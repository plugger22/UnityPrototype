using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Cities. Name of SO is the name of the city, eg. "Gotham City"
/// </summary>
[CreateAssetMenu(menuName = "Game / City")]
public class City : ScriptableObject
{
    [Tooltip("Short text summary that appears in city tooltip")]
    public string descriptor;
    [Tooltip("Starting loyalty to the Authorities of the City (10 is total loyalty)")]
    [Range(0, 10)] public int baseLoyalty = 10;
    [Tooltip("Chance of a connection having a high security level (more than 'None')")]
    [Range(0, 100)] public int connectionSecurityChance = 25;

    [Tooltip("City Arc determines the size, layout and node type frequencies of the city")]
    public CityArc Arc;

    [HideInInspector] public int cityID;         //dynamically assigned by ImportManager.cs

}
