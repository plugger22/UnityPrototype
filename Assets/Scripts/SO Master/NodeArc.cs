using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Node type. Name of SO is the type of Node Arc, eg. "Corporate"
/// </summary>
[CreateAssetMenu(menuName = "Node / Archetype")]
public class NodeArc : ScriptableObject
{
    public string NodeArcTag { get; set; }           //4 letter tag, UPPERCASE eg. 'GOV', 'COR' -> must be unique (use first three letters of name)
    public int NodeArcID { get; set; }               //unique #, zero based, assigned automatically by DataManager.Initialise
    public string NodeName { get; set; }             //specific local name, eg. "Brooklyn"

    //node stats -> base level stats (0 to 3)
    [Tooltip("Values between 0 and 3 only")] public int Stability;
    [Tooltip("Values between 0 and 3 only")] public int Support;
    [Tooltip("Values between 0 and 3 only")] public int Security;
}
