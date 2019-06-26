using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Type of personality factor, eg. Five Factor Model or Dark Triad. Acts as an enum
/// </summary>
[CreateAssetMenu(menuName = "Actor / Factor Type")]
public class FactorType : ScriptableObject
{
    [Tooltip("In game name")]
    public string tag;
}
