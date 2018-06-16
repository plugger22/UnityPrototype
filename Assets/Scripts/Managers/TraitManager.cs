﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all trait related matters
/// </summary>
public class TraitManager : MonoBehaviour
{


    private string colourNeutral;
    private string colourGood;
    private string colourBad;
    private string colourEnd;


    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //select event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }



    public void Initialise()
    {
        SetColours();
        //initialise trait formatted field
        Dictionary<int, Trait> dictOfTraits = GameManager.instance.dataScript.GetDictOfTraits();
        if (dictOfTraits != null)
        {
            foreach(var trait in dictOfTraits)
            {
                if (trait.Value != null)
                {
                    GlobalType traitType = trait.Value.typeOfTrait;
                    if ( traitType != null)
                    {
                        //traitType is from POV of Resistance
                        if (GameManager.instance.sideScript.PlayerSide.level == 2)
                        {
                            //Resistance Side
                            switch (traitType.name)
                            {
                                case "Good":
                                    trait.Value.tagFormatted = string.Format("{0}{1}{2}", colourGood, trait.Value.tag, colourEnd);
                                    break;
                                case "Neutral":
                                    trait.Value.tagFormatted = string.Format("{0}{1}{2}", colourNeutral, trait.Value.tag, colourEnd);
                                    break;
                                case "Bad":
                                    trait.Value.tagFormatted = string.Format("{0}{1}{2}", colourBad, trait.Value.tag, colourEnd);
                                    break;
                                default:
                                    Debug.LogErrorFormat("Invalid trait.traitType \"{0}\" for trait \"{1}\"", traitType.name, trait.Value.name);
                                    break;
                            }
                        }
                        else if (GameManager.instance.sideScript.PlayerSide.level == 1)
                        {
                            //Authority Side
                            switch (traitType.name)
                            {
                                case "Good":
                                    trait.Value.tagFormatted = string.Format("{0}{1}{2}", colourBad, trait.Value.tag, colourEnd);
                                    break;
                                case "Neutral":
                                    trait.Value.tagFormatted = string.Format("{0}{1}{2}", colourNeutral, trait.Value.tag, colourEnd);
                                    break;
                                case "Bad":
                                    trait.Value.tagFormatted = string.Format("{0}{1}{2}", colourGood, trait.Value.tag, colourEnd);
                                    break;
                                default:
                                    Debug.LogErrorFormat("Invalid trait.traitType \"{0}\" for trait \"{1}\"", traitType.name, trait.Value.name);
                                    break;
                            }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid typeOfTrait for trait \"{0}\"", trait.Value.name); }
                }
                else { Debug.LogError("Invalid trait (Null)"); }
            }
        }
        else { Debug.LogError("Invalid dictOfTraits (Null)"); }
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "TraitManager");
    }
}
