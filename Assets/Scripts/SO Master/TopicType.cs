using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Topic category (Human decision system)
/// </summary>
[CreateAssetMenu(menuName = "Topic / TopicType")]
public class TopicType : ScriptableObject
{
    [Tooltip("In game descriptor")]
    public string tag;
    [Tooltip("Overall priority for this sub topic when being placed in the selection pool")]
    public GlobalChance priority;

    [Header("SubTopics")]
    [Tooltip("All SubTopics for this Topic should be in this list. Should be at least one subTopic in list")]
    public List<TopicSubType> listOfSubTypes;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(priority != null, "Invalid priority (Null) for {0}", name);
        Debug.AssertFormat(listOfSubTypes != null, "Invalid listOfSubTypes (Null, needs at least one subType) for {0}", name);
    }

}
