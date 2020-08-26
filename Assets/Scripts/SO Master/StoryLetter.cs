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

    [Tooltip("First paragraph, max of 2 and a bit lines of text. Can use textTags")]
    [TextArea] public string textTop;
    [Tooltip("Second paragraph, max of 2 and a bit lines of text. Can use textTags")]
    [TextArea] public string textBottom;

    [Tooltip("Love '...'")]
    public string textLove;
}
