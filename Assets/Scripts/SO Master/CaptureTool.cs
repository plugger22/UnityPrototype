using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tools available during HQ metagame to help Player avoid revealing information while captured. This is a special type of gear that doesn't count against gear limits and has a very specific use
/// </summary>
[CreateAssetMenu(menuName = "Gear / CaptureTool")]
public class CaptureTool : ScriptableObject
{
    [Header("General")]
    [Tooltip("In Game name of item")]
    public string tag;
    [Tooltip("In Game description")]
    [TextArea] public string descriptor;
    [Tooltip("Sprite image of device")]
    public Sprite sprite;

    [Header("Innocence")]
    [Tooltip("Innocence level (incarceration type) that this tool applies to, eg. Innocence level 3 which is countering an InterroBot")]
    [Range(0, 3)] public int innocenceLevel;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty) for {0}", name);
        Debug.AssertFormat(sprite != null, "Invalid sprite (Null) for {0}", name);
    }

}
