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

    [Header("Lists")]
    [Tooltip("List of all Topic Items (sprites and sprite tooltips) that are referenced by various topics within the Story")]
    public List<TopicItem> listOfTopicItems;
    [Tooltip("List of all Targets that are referenced by various options within the Story")]
    public List<Target> listOfTargets;



    public void OnEnable()
    {
        Debug.AssertFormat(pool != null, "Invalid topicPool (Null) for {0}", pool);
    }

}
