using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO to handle all theme related matters for a story
/// </summary>
[CreateAssetMenu(menuName = "Tools / Theme")]
public class Theme : ScriptableObject
{

    public ThemeType[] arrayOfThemes = new ThemeType[11];          //NOTE: size 11 due to extra 50/50 chance of index 10
}
