using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Scenario setup (defines a level). Name of SO is name of the scenario
/// </summary>
[CreateAssetMenu(menuName = "Game / Scenario")]
public class Scenario : ScriptableObject
{
    [Header("In-game descriptors")]
    [TextArea] public string descriptorResistance;
    [TextArea] public string descriptorAuthority;

    [Header("Side")]
    [Tooltip("Which side can it be played from? Curently indicative only (not used in-game). Authority / Resistance / Both")]
    public GlobalSide side;

    [Header("City")]
    public City city;

    [Header("AI Opponents")]
    [Tooltip("RebelLeader SO")]
    public RebelLeader leaderResistance;
    [Tooltip("Mayor SO")]
    public Mayor leaderAuthority;

    [Header("Mission")]
    [Tooltip("Resistance Target and objectives for the scenario")]
    public Mission missionResistance;
    public Mission missionAuthority;

    [Header("Challenge")]
    [Tooltip("Challenge (difficulty) of the scenario")]
    public Challenge challengeResistance;
    public Challenge challengeAuthority;

    [Header("Number of Turns")]
    [Range(20, 100)] public int timer = 100;



    public void OnEnable()
    {
        Debug.Assert(city != null, "Invalid city (Null) for Scenario");
        Debug.Assert(missionResistance != null, "Invalid mission (Null) for Scenario");
        Debug.Assert(challengeResistance != null, "Invalid challenge (Null) for Scenario");
        Debug.Assert(leaderResistance != null, "Invalid leaderResistance (Null) for Scenario");
        Debug.Assert(leaderAuthority != null, "Invalid leaderAuthority (Null) for Scenario");
        Debug.Assert(side != null, "Invalid side (Null) for Scenario");
    }




}
