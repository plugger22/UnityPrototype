using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sub SO for tutorial Items that provides data for a dialogue Item
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialDialogue")]
public class TutorialDialogue : ScriptableObject
{

    [Tooltip("Top text in dialogue (larger size")]
    [TextArea] public string topText;
    [Tooltip("Bottom text")]
    [TextArea] public string bottomText;

    [Header("Parent")]
    public TutorialItem item;

    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(topText) == false, "Invalid topText (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(bottomText) == false, "Invalid bottomText (Null or Empty) for {0}", name);
        Debug.AssertFormat(item != null, "Invalid item (Null) for {0}", name);
    }
}
