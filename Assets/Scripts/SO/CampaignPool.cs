using UnityEngine;

/// <summary>
/// Handles all topic pools for a campaign.SO
/// </summary>
[CreateAssetMenu(menuName = "Game / Campaign / CampaignPool")]
public class CampaignPool : ScriptableObject
{
    [Header("Campaign")]
    public Campaign campaign;

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


    public void OnEnable()
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
    }

}
