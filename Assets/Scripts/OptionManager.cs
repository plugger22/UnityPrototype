using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all game option matters
/// </summary>
public class OptionManager : MonoBehaviour
{
    public ColourScheme colourOption { get; set; }              //ColourManager.cs ColourScheme enum (eg. 0 -> normal, 1 -> colourblind)


    /// <summary>
    /// Change Colour Scheme
    /// </summary>
    /// <param name="colourScheme"></param>
    public void SetColourScheme(ColourScheme colourScheme)
    {
        colourOption = colourScheme;
        GameManager.instance.colourScript.ChangeColourPalettes();
        Debug.Log("OptionManager -> Colour Scheme now " + colourScheme + "\n");
    }

}
