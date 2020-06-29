using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toolsAPI;
using System;

/// <summary>
/// Data Manager class
/// </summary>
public class ToolDataManager : MonoBehaviour
{

    public Dictionary<string, Story> dictOfStories = new Dictionary<string, Story>();

    /// <summary>
    /// return story dictionary
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, Story> GetDictOfStories()
    { return dictOfStories; }

    /// <summary>
    /// Add story to dict
    /// </summary>
    /// <param name="story"></param>
    public void AddStory(Story story)
    {
        try { dictOfStories.Add(story.tag, story); }
        catch(ArgumentNullException)
        { Debug.LogError("Invalid story (Null)"); }
        catch(ArgumentException)
        { Debug.LogErrorFormat("Duplicate entry exists for story \"{0}\"", story.tag); }
    }

    /// <summary>
    /// returns true if story present in dictOfStories, false otherwise or a problem
    /// </summary>
    /// <param name="story"></param>
    /// <returns></returns>
    public bool CheckStoryExists(Story story)
    {
        if (story != null)
        { return dictOfStories.ContainsKey(story.tag); }
        else
        { Debug.LogError("Invalid story (Null)"); }
        return false;
    }

    //new methods above here
}
