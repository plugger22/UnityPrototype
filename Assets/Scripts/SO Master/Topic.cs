﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Topic Decision 
/// </summary>
[CreateAssetMenu(menuName = "Topic / Topic")]
public class Topic : ScriptableObject
{

    [Tooltip("Short descriptor")]
    public string tag;

    [Header("Categories")]
    [Tooltip("Topic Type that decision is associated with")]
    public TopicType type;
    [Tooltip("Topic SubType that the decision is associated with")]
    public TopicSubType subType;
    [Tooltip("Topic SubSubType that the decision is associated with. Ignore if not applicable")]
    public TopicSubSubType subSubType;

    [Header("Priority")]
    [Tooltip("Overall priority for this topic when being placed in the selection pool")]
    public GlobalChance priority;

    [Header("Group")]
    [Tooltip("Is it in the Good, Nuetral or Bad group of topics for that SubType? If not relevant use Neutral")]
    public GlobalType group;

    [Header("Criteria")]
    [Tooltip("Which side (or 'Both') does this apply to?")]
    public GlobalSide side;

    [Header("Profile")]
    [Tooltip("Profile with timer and repeat data")]
    public TopicProfile profile;

    [Header("Criteria")]
    [Tooltip("In order for the topic to be valid all Criteria must be TRUE")]
    public List<Criteria> listOfCriteria;

    [Header("Content")]
    [Tooltip("Designer notes, not used inGame")]
    [TextArea] public string notes;

    [Header("Options")]
    [Tooltip("Options for the decision. Max as per TopicManager.cs -> maxOptions")]
    public List<TopicOption> listOfOptions;

    #region Save Data Compatible
    [HideInInspector] public Status status = Status.Dormant;
    [HideInInspector] public bool isCurrent;                                                //true if topic valid for current level (in a Campaign/city topicPool), false otherwise
    //timers
    [HideInInspector] public int timerStart;
    [HideInInspector] public int timerRepeat;
    [HideInInspector] public int timerWindow;
    //stats
    [HideInInspector] public int turnsDormant;
    [HideInInspector] public int turnsActive;
    [HideInInspector] public int turnsLive;
    [HideInInspector] public int turnsDone;
    #endregion


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(type != null, "Invalid type (Null) for {0}", name);
        Debug.AssertFormat(subType != null, "Invalid subType (Null) for {0}", name);
        Debug.AssertFormat(priority != null, "Invalid priority (Null) for {0}", name);
        Debug.AssertFormat(listOfOptions != null, "Invalid listOfOptions (Null) for {0}", name);
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
        Debug.AssertFormat(profile != null, "Invalid profile (Null) for {0}", name);
        Debug.AssertFormat(group != null, "Invalid group (Null) for {0}", name);
    }


}
