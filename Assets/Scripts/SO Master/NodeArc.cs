using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Node type -> To Add a new arc use GameManager / DataManager and drag arc SO into listOfNodeArcs and appropriate node Arc lists (# of connections)
/// </summary>
[CreateAssetMenu(menuName = "Node / Archetype")]
public class NodeArc : ScriptableObject
{
    public string NodeTag { get; set; }              //4 letter tag, UPPERCASE eg. 'GOV', 'COR' -> must be unique (use first three letters of name)
    public int NodeArcID { get; set; }               //unique #, zero based, assigned automatically by DataManager.Initialise
    public string NodeName { get; set; }

    //node stats -> base level stats (0 to 3)
    [Tooltip("Values between 0 and 3 only")] public int Stability;
    [Tooltip("Values between 0 and 3 only")] public int Support;
    [Tooltip("Values between 0 and 3 only")] public int Security;
}
