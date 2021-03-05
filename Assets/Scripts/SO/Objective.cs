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

    [Header("Side")]
    [Tooltip("Player side for this Objective")]
    public GlobalSide side;

    [HideInInspector] public int progress = 0;        //progress of objective, when it gets to 100 objective is complete

    public void OnEnable()
    {
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty) for {0}", name);   
    }

    //new methods above here
}
