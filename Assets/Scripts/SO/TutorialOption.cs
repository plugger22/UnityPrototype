using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides strings to populate topicOption.SO's (templates) with data for TutorialItem.SO Query options
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialOption")]
public class TutorialOption : ScriptableObject
{
    [Tooltip("Header in option text -> maps to TopicOption.tag")]
    public string tag;
    [Tooltip("Detail in option text -> maps to TopicOption.text")]
    public string text;

    [Tooltip("Query that the options are associated with")]
    public TutorialItem query;


    public void OnEnable()
    {
        Debug.AssertFormat(tag != null, "Invalid tag (Null) for {0}", name);
        Debug.AssertFormat(text != null, "Invalid text (Null) for {0}", name);
        Debug.AssertFormat(query != null, "Invalid query (Null) for {0}", name);
    }
}
