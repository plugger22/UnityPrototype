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

    [Header("City Images")]
    public Sprite sprite;

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
        Debug.Assert(size != null, "Invalid CitySize (Null)");
        Debug.Assert(spacing != null, "Invalid CitySpacing (Null)");
        Debug.Assert(connections != null, "Invalid CityConnections (Null)");
        Debug.Assert(security != null, "Invalid CitySecurity (Null)");
    }

}
