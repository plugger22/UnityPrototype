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

    [Header("Story Module")]
    [Tooltip("Module that holds all possible stories for the campaign and all related data")]
    public StoryModule story;

    [Header("Authority Topic Pools")]
    [Tooltip("Topic pool for Authority Team decisions")]
    public TopicPool teamPool;

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
    [Tooltip("Topic pool for Player Campaign decisions")]
    public TopicPool playerGearPool;
    [Tooltip("Topic pool for Player Conditions decisions")]
    public TopicPool playerConditionsPool;

    [Header("HQ Topic Pools")]
    [Tooltip("Topic pool for HQ decisions")]
    public TopicPool hqPool;

    [Header("Capture Topic Pools")]
    [Tooltip("Topic pool for Capture decisions")]
    public TopicPool capturePool;

    [Header("Organisation Topic Pools")]
    [Tooltip("Topic pool for Org Cure decisions")]
    public TopicPool orgCurePool;
    [Tooltip("Topic pool for Org Contract decisions")]
    public TopicPool orgContractPool;
    [Tooltip("Topic pool for Org HQ decisions")]
    public TopicPool orgHQPool;
    [Tooltip("Topic pool for Org Emergency decisions")]
    public TopicPool orgEmergencyPool;
    [Tooltip("Topic pool for Org Info decisions")]
    public TopicPool orgInfoPool;

    [Header("Organisations")]
    [Tooltip("Org that provides Cures for any condition that needs them")]
    public Organisation orgCure;
    [Tooltip("Org that provides Elimination Contracts to permanently remove your subordinates")]
    public Organisation orgContract;
    [Tooltip("Org that provides special HQ services such as shutting down investigations")]
    public Organisation orgHQ;
    [Tooltip("Org that provides Emergency services such as get out of jail and safe houses during a crackdown")]
    public Organisation orgEmergency;
    [Tooltip("Org that provides Information services such as location of Nemesis and the cleansing of secret evidence")]
    public Organisation orgInfo;

    [Header("Special Gear")]
    [Tooltip("Special gear that is available during MetaGame if Player's relationship with HQ Boss is 2+")]
    public Gear specialBossGear;
    [Tooltip("Special gear that is available during MetaGame if Player's relationship with HQ SubBoss1 is 2+")]
    public Gear specialSubBoss1Gear;
    [Tooltip("Special gear that is available during MetaGame if Player's relationship with HQ SubBoss2 is 2+")]
    public Gear specialSubBoss2Gear;
    [Tooltip("Special gear that is available during MetaGame if Player's relationship with HQ SubBoss3 is 2+")]
    public Gear specialSubBoss3Gear;

    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty) for {0}", name);
        Debug.AssertFormat(listOfScenarios.Count > 0, "Invalid listOfScenarios (Empty) for {0}", name);
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
        Debug.AssertFormat(story != null, "Invalid storyModule (Null) for {0}, name");

        //Asserts for Resistance side only (Debug measure -> remove once Authority is active)
        if (side.level == 2)
        {

            Debug.Assert(actorContactPool != null, "Invalid actorContactPool (Null)");
            Debug.Assert(actorDistrictPool != null, "Invalid actorDistrictPool (Null)");
            Debug.Assert(actorGearPool != null, "Invalid actorGearPool (Null)");
            Debug.Assert(actorMatchPool != null, "Invalid actorMatchPool (Null)");
            Debug.Assert(actorPoliticPool != null, "Invalid actorPoliticPool (Null)");

            Debug.Assert(playerDistrictPool != null, "Invalid PlayerDistrictPool (Null)");
            Debug.Assert(playerGeneralPool != null, "Invalid PlayerGeneralPool (Null)");
            Debug.Assert(playerStatsPool != null, "Invalid PlayerStatsPool (Null)");
            Debug.Assert(playerGearPool != null, "Invalid PlayerGearPool (Null)");
            Debug.Assert(playerConditionsPool != null, "Invalid PlayerConditionsPool (Null)");

            Debug.Assert(hqPool != null, "Invalid hqPool (Null)");
            Debug.Assert(capturePool != null, "Invalid capturePool (Null)");

            Debug.Assert(orgCure != null, "Invalid orgCure (Null)");
            Debug.Assert(orgContract != null, "Invalid orgContract (Null)");
            Debug.Assert(orgHQ != null, "Invalid orgHQ (Null)");
            Debug.Assert(orgEmergency != null, "Invalid orgEmergency (Null)");
            Debug.Assert(orgInfo != null, "Invalid orgInfo (Null)");

            Debug.Assert(specialBossGear != null, "Invalid specialGearBoss (Null)");
            Debug.Assert(specialSubBoss1Gear != null, "Invalid specialSubBoss1Gear (Null)");
            Debug.Assert(specialSubBoss2Gear != null, "Invalid specialSubBoss2Gear (Null)");
            Debug.Assert(specialSubBoss3Gear != null, "Invalid specialSubBoss3Gear (Null)");
        }
    }


    //new methods above here
}
