using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Topic Options (component of TopicDecisions)
/// </summary>
[CreateAssetMenu(menuName = "Topic / TopicOption")]
public class TopicOption : ScriptableObject
{
    [Tooltip("Short descriptor")]
    public string tag;

    [Tooltip("Topic that the option is connected with")]
    public Topic topic;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null) for {0}", name);
        Debug.AssertFormat(topic != null, "Invalid topic (Null)");
    }
}
