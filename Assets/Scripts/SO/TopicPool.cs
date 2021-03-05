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
    [Tooltip("Topic SubSubType that the decisions are associated with (ignore if not applicable)")]
    public TopicSubSubType subSubType;

    [Header("Topics")]
    [Tooltip("All topics must be of the same type/subType. Any linked topics must all be in the list. Ignore if there are SubSubType Pools present as their topics will be loaded in at level start")]
    public List<Topic> listOfTopics;

    [Header("SubSubType Pools")]
    [Tooltip("Place all relevant SubSubType pools here")]
    public List<TopicPool> listOfSubSubTypePools;

    [Header("Debugging")]
    [Tooltip("Default false for the pool/topics to undergo enhanced Validation testing. Specific to STORY topic pools (enforces structure -> flag allows development of new stories). Optional")]
    public bool isIgnoreExtraValidation;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(type != null, "Invalid type (Null) for {0}", name);
        Debug.AssertFormat(subType != null, "Invalid subType (Null) for {0}", name);
        Debug.AssertFormat(listOfTopics != null, "Invalid listOfTopics (Null) for {0}", name);
        //if there subSubTypes then they will be consolidated into this subType pool at level start with TopicManager.cs -> UpdateTopicPools but at present they will be empty topic Pools so ignore count check
        if (subSubType == null && listOfSubSubTypePools.Count == 0)
        { Debug.AssertFormat(listOfTopics.Count > 0, "Invalid listOfTopics (Empty) for {0}", name); }
        //if a subSubType pool then can't have anything in listOfSubSubTypePools
        if (subSubType != null)
        { Debug.AssertFormat(listOfSubSubTypePools.Count == 0, "If a subSubType present ({0}), can't have any listOfSubSubTypePools, for {1}", subSubType.name, name); }
    }
}
