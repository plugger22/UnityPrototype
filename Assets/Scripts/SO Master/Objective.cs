using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Objectives. Name of SO is the name of the Objective, eg. "Don't Fire Anyone"
/// </summary>
[CreateAssetMenu(menuName = "Game / Objective")]
public class Objective : ScriptableObject
{
    [Tooltip("Short text summary")]
    public string descriptor;
    
    [HideInInspector] public int progress = 0;        //progress of objective, when it gets to 100 objective is complete
    [HideInInspector] public int objectiveID;         //dynamically assigned by DataManager.cs on import



    //new methods above here
}
