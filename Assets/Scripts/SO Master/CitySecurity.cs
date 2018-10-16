using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Cities, determines chance of a connection having higher than default security
/// </summary>
[CreateAssetMenu(menuName = "Game / City / CitySecurity")]
public class CitySecurity : ScriptableObject
{

    [Tooltip("Chance of a connection having a higher, no default, security level")]
    public int chance;
}
