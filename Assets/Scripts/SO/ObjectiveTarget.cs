using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used in Mission.SO for connecting targets to objectives
/// </summary>
[CreateAssetMenu(menuName = "Game / ObjectiveTarget")]
public class ObjectiveTarget : ScriptableObject
{
    [Tooltip("When this target is complete (eg. status.OUTSTANDING, doesn't matter if it has ongoing effects) the objective has it's progress adjusted")]
    public Target target;
    public Objective objective;
    [Tooltip("Adjustment to objective progress, could be +/-, with progress on a % scale, 0 to 100")]
    [Range(1,100)] public int adjustment;



    public void OnEnable()
    {
        Debug.AssertFormat(target != null, "Invalid target (Null) for ObjectiveTarget {0}", name);
        Debug.AssertFormat(objective != null, "Invalid objective (Null) for ObjectiveTarget {0}", name);
        Debug.AssertFormat(adjustment != 0, "Invalid adjustment (Zero) for ObjectiveTarget {0}", name);
    }
}
