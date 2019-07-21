using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Topic Decision 
/// </summary>
[CreateAssetMenu(menuName = "Topic / TopicDecision")]
public class Topic : ScriptableObject
{

    [Tooltip("Short descriptor")]
    public string tag;

    [Tooltip("Topic Type that decision is associated with")]
    public TopicType type;
    [Tooltip("Topic SubType that the decision is associated with")]
    public TopicSubType subType;

    [Header("Options")]
    [Tooltip("Options for the decision")]
    public List<TopicOption> listOfOptions;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(type != null, "Invalid type (Null) for {0}", name);
        Debug.AssertFormat(subType != null, "Invalid subType (Null) for {0}", name);
        Debug.AssertFormat(listOfOptions != null, "Invalid listOfOptions (Null) for {0}", name);
    }


}
