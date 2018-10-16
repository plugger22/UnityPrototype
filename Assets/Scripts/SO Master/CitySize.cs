using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Cities, determines size, eg. number of nodes
/// </summary>
[CreateAssetMenu(menuName = "Game / City / CitySize")]
public class CitySize : ScriptableObject
{
    [Tooltip("Total nodes in city")]
    public int numOfNodes;

    [Tooltip("Minimum number of nodes of each NodeArc type")]
    public int minNum;

}
