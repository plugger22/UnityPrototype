﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for City Arcs. Name of SO is the name of the city arc, eg. "Small Research City"
/// </summary>
[CreateAssetMenu(menuName = "Game / CityArc")]
public class CityArc : ScriptableObject
{
    public string descriptor;

    [HideInInspector] public int cityArcID;         //dynamically assigned by ImportManager.cs

    [Header("City Layout")]
    [Tooltip("Number of nodes on the map (may be less if procgen can't fit them in")]
    [Range(10, 40)] public int numOfNodes = 20;
    [Tooltip("Minimum spacing between nodes (don't exceed 1 if numOfNodes > 25) Note: going to high here with a lot of nodes will result in nodes not being able to fit")]
    [Range(1f, 3f)] public float minNodeSpacing = 1.5f;
    [Tooltip("% chance of each node having an additional connection")]
    [Range(0, 100)] public int connectionFrequency = 50;

    //Lists that control node type distribution within a city
    //allow individual city set-ups (randomly chosen still). Leave any empty if you are happy with DataManager.cs default versions.
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
    public List<NodeArc> listOfFiveConnArcs;
}
