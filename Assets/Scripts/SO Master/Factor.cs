using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Personality Factor, either a 'Five Factor Model' or a 'Dark Traid'
/// </summary>
[CreateAssetMenu(menuName = "Actor / Personality Factor")]
public class Factor : ScriptableObject
{
    [Tooltip("In game name")]
    public string tag;
    [Tooltip("In game descriptor")]
    [TextArea]public string descriptor;
    [Tooltip("Type of Factor")]
    public FactorType type;

    [Tooltip("Text list of descriptors if +2 in factor")]
    public TextList positiveDescriptor;
    [Tooltip("Text list of descriptors if -2 in factor")]
    public TextList negativeDescriptor;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null) for {0}", name);
        Debug.AssertFormat(type != null, "Invalid type (Null) for {0}", name);
    }
}
