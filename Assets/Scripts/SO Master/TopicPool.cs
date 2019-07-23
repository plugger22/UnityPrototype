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

    [Header("Categories")]
    [Tooltip("Topic Type that decisions are associated with")]
    public TopicType type;
    [Tooltip("Topic SubType that the decisions are associated with. Can be left as 'none' if not relevant")]
    public TopicSubType subType;


    [Header("Topics")]
    [Tooltip("All topics must be of the same type/subType (if relevant) and any linked topics must all be in the list")]
    public List<Topic> listOfTopics;

}
