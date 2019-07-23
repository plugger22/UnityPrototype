using System.Collections;
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

    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty) for {0}", name);
        Debug.AssertFormat(listOfScenarios.Count > 0, "Invalid listOfScenarios (Empty) for {0}", name);
    }


    //new methods above here
}
