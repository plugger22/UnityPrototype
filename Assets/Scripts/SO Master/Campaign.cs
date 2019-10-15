using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all campaign related data
/// NOTE: retain all dynamic data in CampaignManager.cs and leave static data here in the SO
/// </summary>
[CreateAssetMenu(menuName = "Game / Campaign")]
public class Campaign : ScriptableObject
{

    [Tooltip("Name of campaign used in-game")]
    public string tag;
    [Tooltip("Description of campaign used in-game")]
    public string descriptor;

    [Header("Side")]
    [Tooltip("Player side for this Campaign")]
    public GlobalSide side;

    [Header("Scenarios")]
    [Tooltip("A list of all scenarios that make up the campaign. NOTE: Scenarios are played in order from top (index 0) to bottom")]
    public List<Scenario> listOfScenarios = new List<Scenario>();

    [Header("Campaign Topic Pools")]
    [Tooltip("Topic pool of decisions for campaign story line Alpha")]
    public TopicPool campaignAlphaPool;
    [Tooltip("Topic pool of decisions for campaign story line Bravo")]
    public TopicPool campaignBravoPool;
    [Tooltip("Topic pool of decisions for campaign story line Charlie")]
    public TopicPool campaignCharliePool;

    [Header("Family Topic Pools")]
    [Tooltip("Topic pool of decisions for family story line Alpha")]
    public TopicPool familyAlphaPool;
    [Tooltip("Topic pool of decisions for family story line Bravo")]
    public TopicPool familyBravoPool;
    [Tooltip("Topic pool of decisions for family story line Charlie")]
    public TopicPool familyCharliePool;

    [Header("Authority Topic Pools")]
    [Tooltip("Topic pool of campaign specific Authority decisions")]
    public TopicPool authorityCampaignPool;
    [Tooltip("Topic pool of general Authority decisions")]
    public TopicPool authorityGeneralPool;
    [Tooltip("Topic pool for Authority Team decisions")]
    public TopicPool teamPool;

    [Header("Resistance General Topic Pools")]
    [Tooltip("Topic pool of campaign specific Resistance decisions")]
    public TopicPool resistanceCampaignPool;
    [Tooltip("Topic pool of general Resistance decisions")]
    public TopicPool resistanceGeneralPool;

    [Header("Actor Topic Pools")]
    [Tooltip("Topic pool for Actor Contact decisions")]
    public TopicPool actorContactPool;
    [Tooltip("Topic pool for Actor District decisions")]
    public TopicPool actorDistrictPool;
    [Tooltip("Topic pool for Actor Gear decisions")]
    public TopicPool actorGearPool;
    [Tooltip("Topic pool for Actor Match (Player Compatibility) decisions")]
    public TopicPool actorMatchPool;
    [Tooltip("Topic pool for Actor Politic (Actors interacting with other Actors) decisions")]
    public TopicPool actorPoliticPool;

    [Header("Player Topic Pools")]
    [Tooltip("Topic pool for Player District decisions")]
    public TopicPool playerDistrictPool;
    [Tooltip("Topic pool for Player General decisions")]
    public TopicPool playerGeneralPool;
    [Tooltip("Topic pool for Player Stats decisions")]
    public TopicPool playerStatsPool;
    [Tooltip("Topic pool for Player Conditions decisions")]
    public TopicPool playerConditionsPool;

    [Header("HQ Topic Pools")]
    [Tooltip("Topic pool for HQ decisions")]
    public TopicPool hqPool;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty) for {0}", name);
        Debug.AssertFormat(listOfScenarios.Count > 0, "Invalid listOfScenarios (Empty) for {0}", name);
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
    }


    //new methods above here
}
