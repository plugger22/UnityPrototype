using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Scenario setup (defines a level). Name of SO is name of the scenario
/// </summary>
[CreateAssetMenu(menuName = "Game / Scenario")]
public class Scenario : ScriptableObject
{
    [Header("Descriptors")]
    [Tooltip("Thematic name of the Scenario")]
    public string tag;
    [TextArea] public string descriptorResistance;
    [TextArea] public string descriptorAuthority;

    [Header("Basics")]
    [Tooltip("Which side can it be played from? Curently indicative only (not used in-game). Authority / Resistance / Both")]
    public GlobalSide side;
    [Tooltip("Which city?")]
    public City city;
    [Tooltip("Seed from which city is generated from (Zero is considered a seed). Can be a whole number between - 2147483648 and 2147483647")]
    public int seedCity;


    [Header("AI Opponents")]
    [Tooltip("RebelLeader SO")]
    public RebelLeader leaderResistance;
    [Tooltip("Mayor SO")]
    public Mayor leaderAuthority;

    [Header("Starting Approval")]
    [Tooltip("Rebel HQ approval (leave at Zero for a random value between 2 & 8)")]
    [Range(0, 10)] public int approvalStartRebelHQ;
    [Tooltip("Authority HQ approval (leave at Zero for a random value between 2 & 8)")]
    [Range(0, 10)] public int approvalStartAuthorityHQ;
    [Tooltip("City Loyalty (leave at Zero for a random value between 2 & 8")]
    [Range(0, 10)] public int cityStartLoyalty;

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

    [HideInInspector] public int scenarioID;

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
