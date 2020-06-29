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

    //
    // - - - Stories
    //

    public Dictionary<string, Story> GetDictOfStories()
    { return dictOfStories; }

    public void ClearDictOfStories()
    { dictOfStories.Clear(); }

    /// <summary>
    /// Add story to dict
    /// </summary>
    /// <param name="story"></param>
    public void AddStory(Story story)
    {
        //create a new story to add to dict (stops issues with reference being deleted in UI)
        Story storyAdd = new Story(story);
        try { dictOfStories.Add(storyAdd.tag, storyAdd); }
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
    /// Sets stories in dict for a load/save operation. Existing data deleted prior to new data input
    /// </summary>
    /// <param name="listOfStories"></param>
    public void SetStories(List<Story> listOfStories)
    {
        int counter = 0;
        if (listOfStories != null)
        {
            dictOfStories.Clear();
            for (int i = 0; i < listOfStories.Count; i++)
            {
                Story story = listOfStories[i];
                if (story != null)
                {
                    if (string.IsNullOrEmpty(story.tag) == false)
                    { AddStory(story); counter++; }
                    else { Debug.LogWarningFormat("Invalid story.tag (Null or Empty) for listOfStories[{0}]", i); }
                }
                else { Debug.LogWarningFormat("Invalid story (Null) for listOfStories[{0}]", i); }
            }
            Debug.LogFormat("[Tst] ToolDataManager.cs -> SetStories: listOfStories has {0} records, {1} have been loaded into Dict{2}", listOfStories.Count, counter, "\n");
        }
        else { Debug.LogError("Invalid listOfStories (Null)"); }

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
