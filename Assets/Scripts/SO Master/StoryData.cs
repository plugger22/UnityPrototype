using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains everything required for a self-contained Story Module
/// </summary>
[CreateAssetMenu(menuName = "Story / Story Data")]
public class StoryData : ScriptableObject
{
    [Header("Core Data")]
    [Tooltip("Design Notes - not used in game")]
    public string designNotes;
    [Tooltip("Topic Pool containing all topics for this story")]
    public TopicPool pool;

    [Header("TopicItems")]
    [Tooltip("List of all Topic Items (sprites and sprite tooltips) that are referenced by various topics within the Story")]
    public List<TopicItem> listOfTopicItems;

    [Header("Targets")]
    [Tooltip("List of all Targets for relevant Scenario 0 story options")]
    public List<Target> listOfTargetsLevel0;
    [Tooltip("List of all Targets for relevant Scenario 1 story options")]
    public List<Target> listOfTargetsLevel1;
    [Tooltip("List of all Targets for relevant Scenario 2 story options")]
    public List<Target> listOfTargetsLevel2;
    [Tooltip("List of all Targets for relevant Scenario 3 story options")]
    public List<Target> listOfTargetsLevel3;
    [Tooltip("List of all Targets for relevant Scenario 4 story options")]
    public List<Target> listOfTargetsLevel4;

    [Header("StoryHelp")]
    [Tooltip("List of all StoryHelp that are referenced by various topics within the Story")]
    public List<StoryHelp> listOfStoryHelp;


    public void OnEnable()
    {
        Debug.AssertFormat(pool != null, "Invalid topicPool (Null) for {0}", pool);
    }

}
