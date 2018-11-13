using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for City Arcs. Name of SO is the name of the city arc, eg. "Small Research City"
/// </summary>
[CreateAssetMenu(menuName = "Game / City / CityArc")]
public class CityArc : ScriptableObject
{
    public string descriptor;

    [HideInInspector] public int cityArcID;         //dynamically assigned by ImportManager.cs

    [Header("City Images")]
    /*[Tooltip("Number of nodes on the map (may be less if procgen can't fit them in). Aim for 15 / 20 / 25 / 30 if possible")]
    [Range(15, 30)] public int numOfNodes = 20;
    [Tooltip("Minimum spacing between nodes (don't exceed 1 if numOfNodes > 25) Note: going to high here with a lot of nodes will result in nodes not being able to fit")]
    [Range(1f, 3f)] public float minNodeSpacing = 1.5f;
    [Tooltip("% chance of each node having an additional connection")]
    [Range(0, 100)] public int connectionFrequency = 50;
    [Tooltip("512 x 150 Png sprite of city Arc")]*/
    public Sprite sprite;

    //Lists that control node type distribution within a city
    /*//allow individual city set-ups (randomly chosen still). Leave any empty if you are happy with DataManager.cs default versions.
    [Header("Preferred Node Mix")]
    [Tooltip("Node Arc types that can be found in nodes with ONE connection (type is randomly chosen so multiple instances of the same NodeArc are O.K")]
    public List<NodeArc> listOfOneConnArcs;
    [Tooltip("Node Arc types that can be found in nodes with TWO connections (type is randomly chosen so multiple instances of the same NodeArc are O.K")]
    public List<NodeArc> listOfTwoConnArcs;
    [Tooltip("Node Arc types that can be found in nodes with THREE connections (type is randomly chosen so multiple instances of the same NodeArc are O.K")]
    public List<NodeArc> listOfThreeConnArcs;
    [Tooltip("Node Arc types that can be found in nodes with FOUR connections (type is randomly chosen so multiple instances of the same NodeArc are O.K")]
    public List<NodeArc> listOfFourConnArcs;
    [Tooltip("Node Arc types that can be found in nodes with FIVE connections (type is randomly chosen so multiple instances of the same NodeArc are O.K")]
    public List<NodeArc> listOfFiveConnArcs;*/

    [Header("City Stats")]
    [Tooltip("Size of City")]
    public CitySize size;
    [Tooltip("Spacing of Nodes")]
    public CitySpacing spacing;
    [Tooltip("Node Connections frequency")]
    public CityConnections connections;
    [Tooltip("Connection Security (chance of a connection having a higher than default level")]
    public CitySecurity security;

    [Tooltip("Any NodeArc placed here will take up HALF of the districts left-over after assigning the minimum requirements. If left blank then random from default selection")]
    public NodeArc priority;


    public void OnEnable()
    {
        /*Debug.Assert(size != null, "Invalid CitySize (Null)");
        Debug.Assert(spacing != null, "Invalid CitySpacing (Null)");
        Debug.Assert(connections != null, "Invalid CityConnections (Null)");
        Debug.Assert(security != null, "Invalid CitySecurity (Null)");*/
    }

}
