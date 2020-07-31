using System;
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

    /// <summary>
    /// returns Topic Pool from listOfCampaignStories based on input topicPool.name. Returns null if not found
    /// </summary>
    /// <param name="poolName"></param>
    /// <returns></returns>
    public TopicPool GetCampaignTopicPool(string poolName)
    {
        if (string.IsNullOrEmpty(poolName) == false)
        { return listOfCampaignStories.Find(x => x.name.Equals(poolName, StringComparison.Ordinal) == true); }
        else { Debug.LogWarning("Invalid poolName (Null or Empty)"); }
        return null;
    }

    /// <summary>
    /// returns Topic Pool from listOfFamilyStories based on input topicPool.name. Returns null if not found
    /// </summary>
    /// <param name="poolName"></param>
    /// <returns></returns>
    public TopicPool GetFamilyTopicPool(string poolName)
    {
        if (string.IsNullOrEmpty(poolName) == false)
        { return listOfFamilyStories.Find(x => x.name.Equals(poolName, StringComparison.Ordinal) == true); }
        else { Debug.LogWarning("Invalid poolName (Null or Empty)"); }
        return null;
    }

    /// <summary>
    /// returns Topic Pool from listOfHqStories based on input topicPool.name. Returns null if not found
    /// </summary>
    /// <param name="poolName"></param>
    /// <returns></returns>
    public TopicPool GetHqTopicPool(string poolName)
    {
        if (string.IsNullOrEmpty(poolName) == false)
        { return listOfHqStories.Find(x => x.name.Equals(poolName, StringComparison.Ordinal) == true); }
        else { Debug.LogWarning("Invalid poolName (Null or Empty)"); }
        return null;
    }

}
