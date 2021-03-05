using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// enum SO for Countries (actually PowerBlocs), name of SO is name of Country (Cities are in countries)
/// </summary>
[CreateAssetMenu(menuName = "Game / Country")]
public class Country : ScriptableObject
{
    [Tooltip("Descriptor only")]
    [TextArea] public string notes;

    public string tag;


}
