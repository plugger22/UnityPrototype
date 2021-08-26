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

    [Header("Placement")]
    [Tooltip("Minimum distance in nodes for random target placement from Player's starting location. Default of 0 is ignore and place anywhere except player's starting node")]
    [Range(1, 4)] public int minDistance = 1;

    [Header("Targets")]
    [Tooltip("All of the listed targets will start the set Live and visibile")]
    public List<Target> listOfTargets;
}
