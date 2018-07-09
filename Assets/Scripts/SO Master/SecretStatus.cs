using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for SecretStatus (enum equivalent). Name of SO is the type of the status, eg. "Active"
/// </summary>
[CreateAssetMenu(menuName = "Actor / Secret / SecretStatus")]
public class SecretStatus : ScriptableObject
{

    public string descriptor;

    [HideInInspector] public int level;            //Inactive -> 0, Active -> 1, Revealed -> 2, Deleted -> 3
}
