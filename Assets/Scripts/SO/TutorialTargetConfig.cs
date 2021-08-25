using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Target configuration setup for a particular tutorial Set
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialTargetConfig")]
public class TutorialTargetConfig : ScriptableObject
{
    [Header("Side")]
    [Tooltip("Which side")]
    public GlobalSide side;

    [Header("Targets")]
    [Tooltip("All of the listed targets will start the set Live and visibile")]
    public List<Target> listOfTargets;
}
