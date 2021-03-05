using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Story SO that handles all story module related matters and is attached to a Campaign.SO
/// </summary>
[CreateAssetMenu(menuName = "Story / StoryModule")]
public class StoryModule : ScriptableObject
{
    [Header("Core Data")]
    [Tooltip("All story modules within this Story.SO are for the following side")]
    public GlobalSide side;

    [Header("Story Data")]
    [Tooltip("Place all campaign related story Modules here -> TopicSubType.StoryAlpha")]
    public List<StoryData> listOfCampaignStories;
    [Tooltip("Place all family related story Modules here -> TopicSubType.StoryBravo")]
    public List<StoryData> listOfFamilyStories;
    [Tooltip("Place all Resistance/Authority related story Modules here -> TopicSubType.StoryCharlie")]
    public List<StoryData> listOfHqStories;


    public void OnEnable()
    {
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
    }

    /// <summary>
    /// returns T from listOfCampaignStories based on input topicPool.name. Returns null if not found
    /// </summary>
    /// <param name="poolName"></param>
    /// <returns></returns>
    public TopicPool GetCampaignTopicPool(string poolName)
    {
        if (string.IsNullOrEmpty(poolName) == false)
        {
            StoryData storyData = listOfCampaignStories.Find(x => x.pool.name.Equals(poolName, StringComparison.Ordinal) == true);
            if (storyData != null)
            { return storyData.pool; }
        }
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
        {
            StoryData storyData = listOfFamilyStories.Find(x => x.pool.name.Equals(poolName, StringComparison.Ordinal) == true);
            if (storyData != null)
            { return storyData.pool; }
        }
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
        {
            StoryData storyData = listOfHqStories.Find(x => x.pool.name.Equals(poolName, StringComparison.Ordinal) == true);
            if (storyData != null)
            { return storyData.pool; }
        }
        else { Debug.LogWarning("Invalid poolName (Null or Empty)"); }
        return null;
    }

    /// <summary>
    /// Returns a random story Data selected from Campaign List. Returns null if none found or a problem
    /// </summary>
    /// <returns></returns>
    public StoryData GetRandomCampaignStoryData()
    {
        int count = listOfCampaignStories.Count;
        if (count > 0)
        { return listOfCampaignStories[UnityEngine.Random.Range(0, count)];  }
        return null;
    }

    /// <summary>
    /// Returns a random story Data selected from Family List. Returns null if none found or a problem
    /// </summary>
    /// <returns></returns>
    public StoryData GetRandomFamilyStoryData()
    {
        int count = listOfFamilyStories.Count;
        if (count > 0)
        { return listOfFamilyStories[UnityEngine.Random.Range(0, count)]; }
        return null;
    }

    /// <summary>
    /// Returns a random story Data selected from Hq List. Returns null if none found or a problem
    /// </summary>
    /// <returns></returns>
    public StoryData GetRandomHqStoryData()
    {
        int count = listOfHqStories.Count;
        if (count > 0)
        { return listOfHqStories[UnityEngine.Random.Range(0, count)]; }
        return null;
    }    
}
