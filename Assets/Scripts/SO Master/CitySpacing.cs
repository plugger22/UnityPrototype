using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Cities, determines spacing of nodes, eg. minimum distance apart
/// </summary>
[CreateAssetMenu(menuName = "Game / City / CitySpacing")]
public class CitySpacing : ScriptableObject
{
    public float minDistance;

}
