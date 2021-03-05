using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for HQ's. Name of SO is the name of the HQ, eg. "Nihilists"
/// </summary>
[CreateAssetMenu(menuName = "Game / Hq")]
public class Hq : ScriptableObject
{
    [Tooltip("In game descriptor")]
    public string tag;
    [Tooltip("Tooltip descriptor")]
    public string descriptor;
    [Tooltip("There is only a single HQ for each side, specify side here")]
    public GlobalSide side;
    [Tooltip("Pictorial representation of HQ (152 x 160 png)")]
    public Sprite sprite;

    


    private void OnEnable()
    {
        Debug.AssertFormat(sprite != null, "Invalid sprite (Null) for {0}", name);
        Debug.AssertFormat(tag != null, "Invalid tag (Null) for {0}", name);
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
        Debug.AssertFormat(descriptor != null, "Invalid descriptor (Null) for {0}", name);
    }


}
