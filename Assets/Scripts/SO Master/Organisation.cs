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
    [Tooltip("Preferred node type for this Organisation. The more of this type of node the greater the chance of an organisation being present in a city")]
    public NodeArc nodeArc;

    [HideInInspector] public int orgID;
}
