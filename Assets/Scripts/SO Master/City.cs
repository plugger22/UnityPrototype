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

    //
    //- - - City Set Up - - -
    //
    [Tooltip("Number of nodes on the map (may be less if procgen can't fit them in")]
    [Range(10, 40)] public int numOfNodes = 20;
    [Tooltip("Minimum spacing between nodes (don't exceed 1 if numOfNodes > 25) Note: going to high here with a lot of nodes will result in nodes not being able to fit")]
    [Range(1, 3)] public int maxNodeSpacing = 1;
    [Tooltip("% chance of each node having an additional connection")]
    [Range(0, 100)] public int connectionFrequency = 50;
    //Lists that control node type distribution within a city
    //allow individual city set-ups (randomly chosen still). Leave any empty if you are happy with DataManager.cs default versions.
    [Tooltip("Node Arc types that can be found in nodes with ONE connection (type is randomly chosen so multiple instances of the same NodeArc are O.K")]
    public List<NodeArc> listOfOneConnArcs;
    [Tooltip("Node Arc types that can be found in nodes with TWO connections (type is randomly chosen so multiple instances of the same NodeArc are O.K")]
    public List<NodeArc> listOfTwoConnArcs;
    [Tooltip("Node Arc types that can be found in nodes with THREE connections (type is randomly chosen so multiple instances of the same NodeArc are O.K")]
    public List<NodeArc> listOfThreeConnArcs;
    [Tooltip("Node Arc types that can be found in nodes with FOUR connections (type is randomly chosen so multiple instances of the same NodeArc are O.K")]
    public List<NodeArc> listOfFourConnArcs;
    [Tooltip("Node Arc types that can be found in nodes with FIVE connections (type is randomly chosen so multiple instances of the same NodeArc are O.K")]
    public List<NodeArc> listOfFiveConnArcs;

    [HideInInspector] public int cityID;         //dynamically assigned by DataManager.cs on import

}
