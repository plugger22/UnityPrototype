using System.Collections;
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

    [Header("Options")]
    [Tooltip("Options for the decision. Max 4")]
    public List<TopicOption> listOfOptions;

    [HideInInspector] public Status status = Status.Dormant;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(type != null, "Invalid type (Null) for {0}", name);
        Debug.AssertFormat(subType != null, "Invalid subType (Null) for {0}", name);
        Debug.AssertFormat(listOfOptions != null, "Invalid listOfOptions (Null) for {0}", name);
        Debug.AssertFormat(listOfOptions?.Count <= 4, "To many options (Max 4) for {0}", name);
    }


}
