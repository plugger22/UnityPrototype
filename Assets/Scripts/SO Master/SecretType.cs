using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for SecretType (enum equivalent). Name of SO is the type of the secret, eg. "Organisation"
/// </summary>
[CreateAssetMenu(menuName = "Actor / Secret / SecretType")]
public class SecretType : ScriptableObject
{

    public string descriptor;

    [HideInInspector] public int level;            //Player -> 0, Desperate -> 1
}
