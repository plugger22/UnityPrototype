using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for 3rd party Organisations. Name of SO is the name of the organisation, eg. "Mafia"
/// </summary>
[CreateAssetMenu(menuName = "Game / Organisation")]
public class Organisation : ScriptableObject
{
    [Tooltip("Short text description (3 words max)")]
    public string descriptor;
    [Tooltip("% Chance of an organisation being present in a city")]
    [Range(0, 100)] public int chance;
}
