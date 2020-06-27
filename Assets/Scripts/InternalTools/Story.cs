using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Story master SO for internal tools
/// </summary>
[CreateAssetMenu(menuName = "Tools / Story")]
public class Story : ScriptableObject
{
    public string tag;
    public string notes;
    public string date;

    public bool isNew;
    public bool isContinuation;
    public bool isEnding;

    public Theme theme;

}
