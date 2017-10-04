using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// provide two hex color code options for both normal and colorBlind schemes. Name SO as the normal color
/// </summary>
[CreateAssetMenu(menuName = "Colour")]
public class Colour : ScriptableObject
{
    public string normal;
    public string colorBlind;
    public string colorBlindColorName;                  //html color name, eg. "Light Salmon"

}
