using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Team SO. Name of SO is the name of the Team, eg. "Control"
/// </summary>
[CreateAssetMenu(menuName = "Authority / TeamArc")]
public class TeamArc : ScriptableObject
{
    public int TeamArcID { get; set; }
    public string description;
    public Sprite sprite;
    [Tooltip("Active effect that team has on District at completion of their time OnMap. Can be ignored")]
    public Effect activeEffect;

}
