using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all trait related matters
/// </summary>
public class TraitManager : MonoBehaviour
{
    //colours
    private string colourNeutral;
    private string colourGood;
    private string colourBad;
    private string colourEnd;


    /// <summary>
    /// Not for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                break;
            case GameState.LoadAtStart:
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }

    #region Initialise SubMethods

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        SetColours();
        //initialise trait formatted field
        Dictionary<string, Trait> dictOfTraits = GameManager.i.dataScript.GetDictOfTraits();
        if (dictOfTraits != null)
        {
            foreach (var trait in dictOfTraits)
            {
                if (trait.Value != null)
                {
                    GlobalType traitType = trait.Value.typeOfTrait;
                    if (traitType != null)
                    {
                        //traitType is as is, NOT from the POV of anybody. If it says bad, then it's bad regardless
                        switch (traitType.level)
                        {
                            case 2:
                                //Good
                                trait.Value.tagFormatted = string.Format("{0}{1}{2}", colourGood, trait.Value.tag, colourEnd);
                                break;
                            case 1:
                                //Neutral
                                trait.Value.tagFormatted = string.Format("{0}{1}{2}", colourNeutral, trait.Value.tag, colourEnd);
                                break;
                            case 0:
                                //Bad
                                trait.Value.tagFormatted = string.Format("{0}{1}{2}", colourBad, trait.Value.tag, colourEnd);
                                break;
                            default:
                                Debug.LogErrorFormat("Invalid trait.traitType \"{0}\" for trait \"{1}\"", traitType.name, trait.Value.name);
                                break;
                        }

                    }
                    else { Debug.LogErrorFormat("Invalid typeOfTrait for trait \"{0}\"", trait.Value.name); }
                }
                else { Debug.LogError("Invalid trait (Null)"); }
            }
        
}
        else { Debug.LogError("Invalid dictOfTraits (Null)"); }
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "TraitManager");
    }
    #endregion

    #endregion

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
        colourNeutral = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourGood = GameManager.i.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.i.colourScript.GetColour(ColourType.dataBad);
        colourEnd = GameManager.i.colourScript.GetEndTag();
    }

    //new methods above here
}
