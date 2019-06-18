using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Objectives. Name of SO is the name of the Objective, eg. "Don't Fire Anyone"
/// </summary>
[CreateAssetMenu(menuName = "Game / Objective")]
public class Objective : ScriptableObject
{
    [Tooltip("In game name")]
    public string tag;
    [Tooltip("Short text summary")]
    public string descriptor;
    
    [HideInInspector] public int progress = 0;        //progress of objective, when it gets to 100 objective is complete



    //new methods above here
}
