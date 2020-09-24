using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Text data for Comms Decision Topics
/// </summary>
[CreateAssetMenu(menuName = "Story / Story Comms")]
public class StoryComms : ScriptableObject
{
    [Tooltip("Who is the message from")]
    public string textFrom;
    [Tooltip("Where is the message from")]
    public string textWhere;

    [Tooltip("First paragraph, max of 2 and a half lines of text. Can use textTags")]
    [TextArea] public string textTop;
    [Tooltip("Second paragraph, max of 2 and a half lines of text. Can use textTags")]
    [TextArea] public string textBottom;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(textFrom) == false, "Invalid textFrom (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(textWhere) == false, "Invalid textWhere (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(textTop) == false, "Invalid textTop (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(textBottom) == false, "Invalid textBottom (Null or Empty) for {0}", name);
    }

}
