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
    [Tooltip("In-game name of the Scenario")]
    public string tag;
    [Tooltip("In-game descriptor")]
    public string descriptor;
    [Tooltip("Side specific descriptor")]
    [TextArea] public string descriptorResistance;
    [Tooltip("Side specific descriptor")]
    [TextArea] public string descriptorAuthority;

    [Header("Side")]
    [Tooltip("Which side can it be played from? Has to match that of the campaign (can't be 'Both')")]
    public GlobalSide side;

    [Header("City")]
    [Tooltip("Which city?")]
    public City city;
    [Tooltip("Seed from which city is generated from (Zero is considered a seed). Can be a whole number between - 2147483648 and 2147483647")]
    public int seedCity;

    [Header("AI Opponents (need both)")]
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
    //public Challenge challengeAuthority;

    [Header("Number of Turns")]
    [Range(20, 100)] public int timer = 100;

    [Header("Newspool")]
    [Tooltip("Add all relevant news TextLists (from zero to any number) that will be used to generate news within the scenario")]
    public List<TextList> newsPool;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty) for {0}", name);
        Debug.AssertFormat(city != null, "Invalid city (Null) for Scenario for {0}", name);
        Debug.AssertFormat(leaderResistance != null, "Invalid leaderResistance (Null) for Scenario for {0}", name);
        Debug.AssertFormat(leaderAuthority != null, "Invalid leaderAuthority (Null) for Scenario for {0}", name);
        Debug.AssertFormat(side != null, "Invalid side (Null) for Scenario for {0}", name);
        Debug.AssertFormat(approvalStartRebelHQ > 0, "Invalid approvalStartRebelHQ (Zero) for {0}", name);
        Debug.AssertFormat(approvalStartAuthorityHQ > 0, "Invalid approvalStartAuthorityHQ (Zero) for {0}", name);
        Debug.AssertFormat(cityStartLoyalty > 0, "Invalid cityStartLoyalty (Zero) for {0}", name);
        Debug.AssertFormat(timer > 0, "Invalid timer (Zero) for {0}", name);
    }




}
