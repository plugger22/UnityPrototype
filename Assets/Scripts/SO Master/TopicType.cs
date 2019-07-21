using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Topic category (Human decision system) Static data only, dynamic data is kept in the dictOfTopicTypes using a TopicData package
/// </summary>
[CreateAssetMenu(menuName = "Topic / TopicType")]
public class TopicType : ScriptableObject
{
    [Tooltip("In game descriptor")]
    public string tag;
    [Tooltip("Overall priority for this sub topic when being placed in the selection pool")]
    public GlobalChance priority;
    [Tooltip("Number of turns that must elapse before it can be chosen again, default 0")]
    [Range(0, 20)] public int minimumInterval = 0;

    [Header("SubTopics")]
    [Tooltip("All SubTopics for this Topic should be in this list. Should be at least one subTopic in list")]
    public List<TopicSubType> listOfSubTypes;

    [Header("Criteria")]
    [Tooltip("In order for the topic type to be valid for a level all Criteria must be TRUE")]
    public List<Criteria> listOfCriteria;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(priority != null, "Invalid priority (Null) for {0}", name);
        Debug.AssertFormat(listOfSubTypes != null, "Invalid listOfSubTypes (Null, needs at least one subType) for {0}", name);
        Debug.AssertFormat(listOfCriteria != null, "Invalid listOfCriteria (Null) for {0}", name);
    }

}
