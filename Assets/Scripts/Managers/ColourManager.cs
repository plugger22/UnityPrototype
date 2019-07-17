using gameAPI;
using modalAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

//Colour Manager -?NOTE: DO NOT change the order of either colour enum!
public enum ColourScheme { Normal, ColourBlind, Count }
public enum ColourType
{
    dataGood, dataNeutral, dataBad, dataTerrible,
    normalText, goodText, neutralText, badText, blueText, salmonText, moccasinText, greyText, whiteText, blackText,
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
    //Data
    public Colour[] dataGood;                       //number in good range (value 3)
    public Colour[] dataNeutral;                    //neutral (value 2)
    public Colour[] dataBad;                        //bad (value 1)
    public Colour[] dataTerrible;                   //terrible (value 0)
    //Text
    public Colour[] normalText;                     //normal non-coloured text, eg. white equivalent
    public Colour[] goodText;                       //mild green text
    public Colour[] neutralText;                    //neutral yellow text
    public Colour[] badText;                        //mild red text
    public Colour[] salmonText;                     //text you want highlighted (Salmon), eg. Alert text
    public Colour[] blueText;                       //resistance side
    public Colour[] moccasinText;                   //cancel text
    public Colour[] greyText;                       //greyed out text
    public Colour[] whiteText;                    //default White text if no colour provided
    public Colour[] blackText;


    private Colour[,] arrayOfColours;               //repositry of colourTypes
    private List<Colour[]> listOfColourTypes;       //facilitates automatic population of array


    public void Initialise(GameState state)
    {
        arrayOfColours = new Colour[(int)ColourType.Count, (int)ColourScheme.Count];
        //add each array of Colours into the List -> NOTE: change this whenever you add, or modify, a colour. 
        //Note: KEEP IN SAME ORDER AS ColourType enum
        listOfColourTypes = new List<Colour[]>()
        {
            dataGood,
            dataNeutral,
            dataBad,
            dataTerrible,
            normalText,
            goodText,
            neutralText,
            badText,
            blueText,
            salmonText,
            moccasinText,
            greyText,
            whiteText,
            blackText
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



    /// <summary>
    /// subMethod  to provide correct colour for various good/neutral/bad stats and trait. Same for both sides cause High is good, low is bad throughout (3/2/1/0)
    /// </summary>
    /// <param name="datapoint"></param>
    /// <returns></returns>
    public string GetValueColour(int value)
    {
        string colourReturn;
        switch (value)
        {
            case 0: colourReturn = dataTerrible[(int)GameManager.instance.optionScript.ColourOption].hexCode; break;
            case 1: colourReturn = dataBad[(int)GameManager.instance.optionScript.ColourOption].hexCode; break;
            case 2: colourReturn = dataNeutral[(int)GameManager.instance.optionScript.ColourOption].hexCode; break;
            case 3: colourReturn = dataGood[(int)GameManager.instance.optionScript.ColourOption].hexCode; break;
            default: colourReturn = dataGood[(int)GameManager.instance.optionScript.ColourOption].hexCode; break; //default is > 3 (all good)
        }
        return string.Format("{0}{1}{2}", "<color=", colourReturn, ">");
    }

    /// <summary>
    /// returns inputted string formatted in the specified colour, ready for display. Returns original, unformatted text if a problem
    /// </summary>
    /// <param name="text"></param>
    /// <param name="colorType"></param>
    /// <returns></returns>
    public string GetFormattedString(string text, ColourType colourType)
    {
        string formattedText = text;
        if (string.IsNullOrEmpty(text) == false)
        { formattedText = string.Format("{0}{1}{2}", GetColour(colourType), text, GetEndTag()); }
        else { Debug.LogError("Invalid text (Null)"); }
        return formattedText;
    }


    /// <summary>
    /// Debug display of an outcome message with all colours LoadManager.cs -> arrayOfColours (Colour.SO) named and appropriately coloure for purposes of colour comparison
    /// </summary>
    public void DebugDisplayColourPalette()
    {
        //create an outcome window to notify player
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        outcomeDetails.side = GameManager.instance.globalScript.sideBoth;
        outcomeDetails.textTop = "Colour Palette";
        StringBuilder builder = new StringBuilder();
        Colour[] arrayOfColour = GameManager.instance.loadScript.arrayOfColours;
        if (arrayOfColour != null)
        {
            foreach(Colour colour in arrayOfColours)
            {
                if (colour != null)
                { builder.AppendFormat("<color={0}>{1}</color>{2}", colour.hexCode, colour.name, "\n"); }
                else { Debug.LogWarning("Invalid colour (Null) in arrayOfColours"); }
            }
        }
        else { Debug.LogError("Invalid arrayOfColour (Null)"); }
        outcomeDetails.textBottom = builder.ToString();
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "TargetManager.cs -> InitialiseGenericPickerTargetInfo");
    }

}
