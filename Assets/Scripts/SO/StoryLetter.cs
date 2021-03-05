using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Text data for Letter Decision Topics
/// </summary>
[CreateAssetMenu(menuName = "Story / Story Letter")]
public class StoryLetter : ScriptableObject
{
    [Tooltip("Dear '...'")]
    public string textDear;

    [Tooltip("First paragraph, max of 2 and a half lines of text. Can use textTags")]
    [TextArea] public string textTop;
    [Tooltip("Second paragraph, max of 2 and a half lines of text. Can use textTags")]
    [TextArea] public string textBottom;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(textDear) == false, "Invalid textDear (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(textTop) == false, "Invalid textTop (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(textBottom) == false, "Invalid textBottom (Null or Empty) for {0}", name);
    }
}
