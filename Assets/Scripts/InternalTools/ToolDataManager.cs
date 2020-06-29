using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toolsAPI;
using System;
using System.Linq;

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

    /// <summary>
    /// returns first record in dictOfStories (not in sorted order so could be anything). Returns null if a problem or none found
    /// </summary>
    /// <returns></returns>
    public Story GetFirstStoryInDict()
    {
        Story story = null;
        if (dictOfStories.Count > 0)
        {
            story = dictOfStories.Values.First();
        }
        else { Debug.LogWarning("No records in dictOfStories -> ALERT"); }
        return story;
    }

    //new methods above here
}
