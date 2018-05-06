using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all Objective related matters (both sides)
/// </summary>
public class ObjectiveManager : MonoBehaviour
{
    [Tooltip("The maximum number of objectives in a level (determined by top widget UI)")]
    [Range(0, 3)] public int maxNumOfObjectives = 3;

    [HideInInspector] public int objectivesAuthority;            //how many objectives the Authority side has this level in total
    [HideInInspector] public int objectivesResistance;           //how many objectives the Resistance side has this level in total

    [HideInInspector] public int objectivesCurrentAuthority;        //how many objectives the Authority side currently has remaining to be completed   
    [HideInInspector] public int objectivesCurrentResistance;        //how many objectives the Resistance side currently has remaining to be completed

    private string colourRebel;
    private string colourAuthority;
    private string colourNeutral;
    private string colourNormal;
    /*private string colourGood;
    private string colourBad;
    private string colourGrey;
    private string colourAlert;*/
    private string colourSide;
    private string colourEnd;

    public void Initialise()
    {
        //event listeners
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
    }


    /// <summary>
    /// Called when an event happens
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect Event type
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
        colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        colourRebel = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        /*colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);*/
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
        //current Player side colour
        if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; }
        else { colourSide = colourRebel; }
    }

    /// <summary>
    /// returns colour string based on Player side in format 'Resistance Objectives'
    /// </summary>
    /// <returns></returns>
    public string GetObjectivesTitle()
    {
        string description = "Unknown";
        switch (GameManager.instance.sideScript.PlayerSide.level)
        {
            case 1:
                description = string.Format("{0}Authority Objectives{1}", colourSide, colourEnd);
                break;
            case 2:
                description = string.Format("{0}Resistance Objectives{1}", colourSide, colourEnd);
                break;
        }
        return description;
    }

    /// <summary>
    /// returns colour string based on Player side that summarises objective status
    /// </summary>
    /// <returns></returns>
    public string GetObjectivesSummary()
    {
        string description = "Unknown";
        switch (GameManager.instance.sideScript.PlayerSide.level)
        {
            case 1:
                description = string.Format("{0}{1}{2}{3} of <b>{4}{5}</b> {6}Objectives have been completed{7}", colourNeutral, objectivesAuthority - objectivesCurrentAuthority, 
                    colourEnd, colourNormal, colourEnd, objectivesAuthority, colourNormal, colourEnd);
                break;
            case 2:
                description = string.Format("{0}{1}{2}{3} of <b>{4}{5}</b> {6}Objectives have been completed{7}", colourNeutral, objectivesResistance - objectivesCurrentResistance,
                    colourEnd, colourNormal, colourEnd, objectivesAuthority, colourNormal, colourEnd);
                break;
        }
        return description;
    }

    //new methods above here
}
