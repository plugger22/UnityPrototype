using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all kinds of text data in the form of a list. Used for creating drop-in random pick lists for other SO's
/// </summary>
[CreateAssetMenu(menuName = "TextList")]
public class TextList : ScriptableObject
{
    [Tooltip("Descriptor only, not used in-game")]
    public string descriptor;
    [Tooltip("A list of strings (related) used to create a random pick list")]
    public List<string> randomList;
}
