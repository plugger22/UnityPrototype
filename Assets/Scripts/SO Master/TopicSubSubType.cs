using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SubSubCategory for human decisions (Related a TopicSubType, only applies to some, eg. ActorDistrict)
/// </summary>
[CreateAssetMenu(menuName = "Topic / TopicSubSubtType")]
public class TopicSubSubType : ScriptableObject
{
    [Header("General")]
    [Tooltip("In game descriptor")]
    public string tag;

    [Header("TopicSubType")]
    [Tooltip("Which TopicSubType does this subSubType belong to?")]
    public TopicSubType subType;

    [Header("Side")]
    [Tooltip("Which side (or 'Both') does this apply to?")]
    public GlobalSide side;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null) for {0}", name);
        Debug.AssertFormat(subType != null, "Invalid subType (Null) for {0}", name);
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
    }

}
