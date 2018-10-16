using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Cities, determines frequency of node Connections
/// </summary>
[CreateAssetMenu(menuName = "Game / City / CityConnection")]
public class CityConnections : ScriptableObject
{
    [Tooltip("Frequency of node connections")]
    public int frequency;
}
