using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for type of MegaCorporations
/// </summary>
[CreateAssetMenu(menuName = "Game / MegaCorp")]
public class MegaCorp : ScriptableObject
{
    [Tooltip("First name only, eg. ignore 'Corp' as it's added in via text tags if required")]
    public string tag;
    [Tooltip("What are they into?")]
    public string field; 
    [Tooltip("Corp backstory")]
    [TextArea(2, 3)] public string descriptor;
    public Sprite sprite;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(field) == false, "Invalid field (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty) for {0}", name);
        Debug.AssertFormat(sprite != null, "Invalid sprite (Null) for {0}", name);
    }
}
