using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SubCategory for human decisions (Topics)
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


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(priority != null, "Invalid priority (Null) for {0}", name);
        Debug.AssertFormat(type != null, "Invalid Type (Null) for {0}", name);
    }
}
