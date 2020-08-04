using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Topic Item typically used for Stories but can be multipurpose. Provides sprite for topic and sprite tooltips
/// </summary>
[CreateAssetMenu(menuName = "Topic / Topic Item")]
public class TopicItem : ScriptableObject
{
    [Tooltip("Sprite for Item that is displayed in relevant topics")]
    public Sprite sprite;
    [Tooltip("Name of Item that is displayed as the sprite tooltip top text")]
    public string tag;
    [Tooltip("Descriptor of Item that is displayed as the sprite tooltip bottom text")]
    [TextArea(2,3)] public string descriptor;
    [Tooltip("Topic pool this topicItem is associated with (used for Validation checks)")]
    public TopicPool pool;


    public void OnEnable()
    {
        Debug.AssertFormat(sprite != null, "Invalid sprite (Null) for {0}", name);
        Debug.AssertFormat(tag != null, "Invalid tag (Null) for {0}", name);
        Debug.AssertFormat(descriptor != null, "Invalid tag (Null) for {0}", name);
        Debug.AssertFormat(pool != null, "Invalid topicPool (Null) for {0}", name);
    }
}
