using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all campaign related data
/// NOTE: retain all dynamic data in CampaignManager.cs and leave static data here in the SO
/// </summary>
[CreateAssetMenu(menuName = "Game / Campaign / Campaign")]
public class Campaign : ScriptableObject
{

    [Tooltip("Name of campaign used in-game")]
    public string tag;
    [Tooltip("Description of campaign used in-game")]
    public string descriptor;

    [Header("Validation")]
    [Tooltip("If checked this will exclude the campaign from all validation checks")]
    public bool isIgnoreValidation;

    [Header("Side")]
    [Tooltip("Player side for this Campaign")]
    public GlobalSide side;

    [Header("Scenarios")]
    [Tooltip("A list of all scenarios that make up the campaign. NOTE: Scenarios are played in order from top (index 0) to bottom")]
    public List<Scenario> listOfScenarios = new List<Scenario>();

    [Header("Actor Pool")]
    [Tooltip("Pool of actors to be used for the duration of the campaign")]
    public ActorPoolFinal actorPool;

    [Header("Story Module")]
    [Tooltip("Module that holds all possible stories for the campaign and all related data")]
    public StoryModule story;

    [Header("Global Newspool")]
    [Tooltip("Add all relevant news TextLists (from zero to any number) that will be used to generate news across the campaign")]
    public List<TextList> newsPool;

    [Header("Topic Pools")]
    [Tooltip("Master collection of all relevant topic pools for this campaign")]
    public CampaignPool campaignPool;

    [Header("Orgs/MegaCorps/Gear")]
    public CampaignDetails details;


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
            Debug.Assert(actorPool != null, "Invalid actorPool (Null)");
            Debug.Assert(campaignPool != null, "Invalid CampaignPool (Null)");
        }
    }


    //new methods above here
}
