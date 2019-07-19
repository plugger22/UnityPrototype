using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SubCategory for human decisions (Topics) Static data only, dynamic data is kept in the dictOfTopicSubTypes using a TopicData package
/// </summary>
[CreateAssetMenu(menuName = "Topic / TopicSubType")]
public class TopicSubType : ScriptableObject
{
    [Tooltip("In game descriptor")]
    public string tag;
    [Tooltip("Which TopicType does this subType belong to?")]
    public TopicType type;
    [Tooltip("Overall priority for this sub topic when being placed in the selection pool")]
    public GlobalChance priority;
    [Tooltip("Number of turns that must elapse before it can be chosen again, default 0")]
    [Range(0, 20)] public int minimumInterval = 0;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(priority != null, "Invalid priority (Null) for {0}", name);
        Debug.AssertFormat(type != null, "Invalid Type (Null) for {0}", name);
    }
}
