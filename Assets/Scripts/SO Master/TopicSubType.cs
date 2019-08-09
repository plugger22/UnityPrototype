using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SubCategory for human decisions (Topics) Static data only, dynamic data is kept in the dictOfTopicSubTypes using a TopictypeData package
/// </summary>
[CreateAssetMenu(menuName = "Topic / TopicSubType")]
public class TopicSubType : ScriptableObject
{
    [Header("General")]
    [Tooltip("In game descriptor")]
    public string tag;
    [Tooltip("Which TopicType does this subType belong to?")]
    public TopicType type;

    [Header("Interval")]
    [Tooltip("Multiplier to global Minimum interval, eg. 2x, 3x, etc. for number of turns that must elapse before it can be chosen again, default 0")]
    [Range(0, 20)] public int minIntervalFactor = 0;

    [Header("Vitals")]
    [Tooltip("Which side (or 'Both') does this apply to?")]
    public GlobalSide side;
    [Tooltip("Overall priority for this sub topic when being placed in the selection pool")]
    public GlobalChance priority;
    [Tooltip("Scope of Topics within - Level (topic status reset each level) / Campaign (topic status carries over)")]
    public TopicScope scope;

    [Header("SubSubTypes")]
    [Tooltip("List of SubSubTypes that are linked to this SubType. Ignore if none")]
    public List<TopicSubSubType> listOfSubSubType;

    [Header("Criteria")]
    [Tooltip("In order for the topic type to be valid for a level all Criteria must be TRUE")]
    public List<Criteria> listOfCriteria;

    [HideInInspector] public int minInterval;               //calculated value = minIntervalFactor x TopicManager.cs -> minIntervalGlobal (calculated at TopicManager.cs -> subInitialiseStartUp)

    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(priority != null, "Invalid priority (Null) for {0}", name);
        Debug.AssertFormat(type != null, "Invalid Type (Null) for {0}", name);
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
        Debug.AssertFormat(scope != null, "Invalid scope (Null) for {0}", name);
    }
}
