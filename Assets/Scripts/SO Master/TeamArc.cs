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
    public Sprite sprite;
    public List<Effect> listOfEffects;

}
