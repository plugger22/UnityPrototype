using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ColourScheme { Normal, ColourBlind }
public enum ColourType { sideRebel, sideAuthority, cancelHighlight, cancelNormal, Count}

/// <summary>
/// Handles all colour related matters
/// </summary>
public class ColourManager : MonoBehaviour
{
    public string sideBlue = "#87CEFA";
    public string sideRed = "#FF0000";
    public string Salmon = "#FFA07A";
    public string Moccassin = "#FFE4B5";

    public Colour sideRebel;                      //Blue for rebel text
    public Colour sideAuthority;                  //Red for authority text
    public Colour cancelHighlight;                //Cancel button generic tool tip
    public Colour cancelNormal;                   //Cancel button generic tool tip


    private Colour[] colourArray;


    private void Awake()
    {
        colourArray = new Colour[(int)ColourType.Count];
        colourArray[0] = sideRebel;
        colourArray[1] = sideAuthority;
        colourArray[2] = cancelHighlight;
        colourArray[3] = cancelNormal;

    }


    /// <summary>
    /// returns a colour TextMeshPro Tag, eg. '<color = #FFE4B5'
    /// </summary>
    /// <param name="colour"></param>
    /// <returns></returns>
    public string GetColour(ColourType colour)
    {
        string colourTag = "<color=" + colourArray[(int)colour].normal + ">";
        return colourTag;
    }

    public string GetEndTag()
    { return "</color>"; }

}
