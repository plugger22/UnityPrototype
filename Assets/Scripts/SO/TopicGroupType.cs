using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// enum SO to indicate which Group type method is used for a TopicSubType or SubSubType
/// </summary>
[CreateAssetMenu(menuName = "Topic / TopicGroupType")]
public class TopicGroupType : ScriptableObject
{
    [Tooltip("In game tooltip info")]
    public string tag;

}
