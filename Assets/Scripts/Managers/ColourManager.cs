using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Colour Manager -?NOTE: DO NOT change the order of either colour enum!
public enum ColourScheme { Normal, ColourBlind, Count }
public enum ColourType
{
    sideRebel, sideAuthority,
    cancelHighlight, cancelNormal,
    actionEffect,
    dataGood, dataNeutral, dataBad,
    normalText, defaultText, greyText,                           //normal is slight off yellow text, default is white, grey is greyed out
    //outcome window effects
    goodEffect, neutralEffect, badEffect,                              
    nodeActive,
    error,
    black,
    //other
    actorArc,
    actorAction,
    Count
}

/// <summary>
/// Handles all colour related matters
/// Each SO has to have a hexcolor code for each ColourScheme (same order as enum)
/// Adding additional colours requires a manual entry in Awake to add new colour to the list
/// Each Colour[] entry corresponds to the ColourScheme, eg. sideRebel[0] = sideRebel[ColourScheme.Normal]
/// </summary>
public class ColourManager : MonoBehaviour
{
    //tool tip Generic
    public Colour[] sideRebel;                      //Blue for rebel text
    public Colour[] sideAuthority;                  //Red for authority text
    public Colour[] cancelHighlight;                //Cancel button generic tool tip
    public Colour[] cancelNormal;                   //Cancel button generic tool tip
    public Colour[] actionEffect;                   //effects for action menu buttons
    //stats
    public Colour[] dataGood;                       //number in good range
    public Colour[] dataNeutral;                    //neutral
    public Colour[] dataBad;                        //bad
    //default Text
    public Colour[] normalText;                     //normal non-coloured text, eg. white equivalent
    public Colour[] defaultText;                    //default White text if no colour provided
    public Colour[] greyText;                       //greyed out text
    //outcome Effects
    public Colour[] goodEffect;
    public Colour[] neutralEffect;
    public Colour[] badEffect;

    //tool tip Node
    public Colour[] nodeActive;                     //active actors in Node
    //error (global)
    public Colour[] error;                          //error text

    public Colour[] black;
    public Colour[] actorArc;                       //violet colour for "WORKS"
    public Colour[] actorAction;                    //salmon colour for "Blow Stuff Up"

    private Colour[,] arrayOfColours;               //repositry of colourTypes
    private List<Colour[]> listOfColourTypes;       //facilitates automatic population of array


    private void Awake()
    {
        arrayOfColours = new Colour[(int)ColourType.Count, (int)ColourScheme.Count];
        //add each array of Colours into the List -> NOTE: change this whenever you add, or modify, a colour. 
        //Note: KEEP IN SAME ORDER AS ColourType enum
        listOfColourTypes = new List<Colour[]>()
        {
            sideRebel,
            sideAuthority,
            cancelHighlight,
            cancelNormal,
            actionEffect,
            dataGood,
            dataNeutral,
            dataBad,
            normalText,
            defaultText,
            greyText,
            goodEffect,
            neutralEffect,
            badEffect,
            nodeActive,
            error,
            black,
            actorArc,
            actorAction,
        };

        //loop thorugh list and auto populate array
        int limit;
        for (int i = 0; i < listOfColourTypes.Count; i++)
        {
            Colour[] tempArray = listOfColourTypes[i];
            //loop limit is the smallest of tempArray or # of ColourSchemes
            limit = Mathf.Min((int)ColourScheme.Count, tempArray.Length);
            for (int j = 0; j < limit; j++)
            {
                arrayOfColours[i, j] = tempArray[j];
            }
        }

    }


    /// <summary>
    /// returns a colour TextMeshPro Tag, eg. '<color = #FFE4B5', default White if colour isn't present
    /// </summary>
    /// <param name="colour"></param>
    /// <returns></returns>
    public string GetColour(ColourType colour)
    {
        //default white colour
        string colourTag = "#FFFFFF";
        //automatically handle an access call to an arrayIndex that hasn't been populated
        try
        {
            colourTag = arrayOfColours[(int)colour, (int)GameManager.instance.optionScript.ColourOption].hexCode;
        }
        catch (IndexOutOfRangeException)
        {
            //return default white colour if no entry present
            Debug.LogError(string.Format("No colour present for {0} with ColourScheme {1}. Return default WHITE", colour, GameManager.instance.optionScript.ColourOption));
        }
        catch (NullReferenceException)
        {
            //return default white colour if no entry present
            Debug.LogError(string.Format("No colour present for {0} with ColourScheme {1} (Null). Return default WHITE", colour, GameManager.instance.optionScript.ColourOption));
        }
        return string.Format("{0}{1}{2}", "<color=", colourTag, ">");
    }

    public string GetEndTag()
    { return "</color>"; }

}
