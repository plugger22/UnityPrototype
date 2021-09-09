using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// specifies Authority team onMap starting state for a tutorial state. NOTE: AI Authority on/off doesn't matter. If actions are on teams will be recalled but if not they will stay on map indefinitely
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialTeamConfig")]
public class TutorialTeamConfig : ScriptableObject
{
    [Header("Side")]
    [Tooltip("Which side")]
    public GlobalSide side;

    [Header("OnMap Teams")]
    [Tooltip("Teams (no dupes allowed) onMap at start of Tutorial Set. THREE teams max, must be at least one.")]
    public List<TeamArc> listOfTeams;

    public void OnEnable()
    {
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
    }

}
