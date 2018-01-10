using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Team SO. Name of SO is the name of the Team, eg. "Control"
/// </summary>
[CreateAssetMenu(menuName = "TeamArc")]
public class TeamArc : ScriptableObject
{
    public int TeamArcID { get; set; }
    public string description;
    public TeamType type;
    public Sprite sprite;
    //[Tooltip("True only if the team's effect applies while it is present at the node, eg. a TEMPORARY effect")]
    //public bool isTemporaryEffect;                  //true for any team arc whose effect only applies while team present at the node
}
