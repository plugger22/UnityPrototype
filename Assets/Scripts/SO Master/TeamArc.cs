using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Team SO. Name of SO is the name of the Team, eg. "Security"
/// </summary>
[CreateAssetMenu(menuName = "TeamArc")]
public class TeamArc : ScriptableObject
{
    public int TeamArcID { get; set; }
    public string description;

}
