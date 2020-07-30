using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Story SO that handles all story module related matters and is attached to a Campaign.SO
/// </summary>
[CreateAssetMenu(menuName = "StoryModule / StoryModule")]
public class StoryModule : ScriptableObject
{
    [Header("Core Data")]
    [Tooltip("All story modules within this Story.SO are for the following side")]
    public GlobalSide side;

    [Header("Story Modules")]
    [Tooltip("Place all campaign related story Modules here -> TopicSubType.StoryAlpha")]
    public List<TopicPool> listOfCampaignStories;
    [Tooltip("Place all family related story Modules here -> TopicSubType.StoryBravo")]
    public List<TopicPool> listOfFamilyStories;
    [Tooltip("Place all Resistance/Authority related story Modules here -> TopicSubType.StoryCharlie")]
    public List<TopicPool> listOfHqStories;


    public void OnEnable()
    {
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
    }

    
}
