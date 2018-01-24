using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using modalAPI;
using gameAPI;

/// <summary>
/// Handles all Resitance related matters
/// </summary>
public class ResistanceManager : MonoBehaviour
{


    [HideInInspector] public int resistanceCauseMax;                        //level of Rebel Support. Max out to Win the level. Max level is a big part of difficulty.
    [HideInInspector] public int resistanceCause;                           //current level of Rebel Support

    private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourEnd;


    public void Initialise()
    {
        resistanceCauseMax = 10;
        resistanceCause = 0;
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
    }

    /// <summary>
    /// event handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //detect event type
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
    /// Set colour palette for tooltip
    /// </summary>
    public void SetColours()
    {
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

   

    //new methods above here
}
