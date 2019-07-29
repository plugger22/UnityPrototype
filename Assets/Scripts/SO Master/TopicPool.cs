using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Collection of related topics used to 'drop in' lists of topics to other SO's, eg. Campaign / City / Trait. 
/// </summary>
[CreateAssetMenu(menuName = "Topic / Topic Pool")]
public class TopicPool : ScriptableObject
{
    [Tooltip("Short descriptor")]
    public string tag;
    [Tooltip("Designer explanation")]
    [TextArea] public string descriptor;

    [Header("Categories")]
    [Tooltip("Topic Type that decisions are associated with")]
    public TopicType type;
    [Tooltip("Topic SubType that the decisions are associated with")]
    public TopicSubType subType;


    [Header("Topics")]
    [Tooltip("All topics must be of the same type/subType. Any linked topics must all be in the list")]
    public List<Topic> listOfTopics;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(type != null, "Invalid type (Null) for {0}", name);
        Debug.AssertFormat(subType != null, "Invalid subType (Null) for {0}", name);
        Debug.AssertFormat(listOfTopics != null, "Invalid listOfTopics (Null) for {0}", name);
        Debug.AssertFormat(listOfTopics.Count > 0, "Invalid listOfTopics (Empty) for {0}", name);
    }
}
