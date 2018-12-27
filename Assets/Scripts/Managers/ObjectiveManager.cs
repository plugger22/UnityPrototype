using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using gameAPI;

/// <summary>
/// handles all Objective related matters (both sides)
/// NOTE: Only one side has objectives (Human side)
/// </summary>
public class ObjectiveManager : MonoBehaviour
{
    [Tooltip("The maximum number of objectives in a level (determined by top widget UI)")]
    [Range(0, 3)] public int maxNumOfObjectives = 3;

    [HideInInspector] public int objectivesTotal;                               //how many objectives the human side has this level in total
    [HideInInspector] public int objectivesCurrent;                             //how many objectives the human side currently has remaining to be completed   

    private List<Objective> listOfObjectives = new List<Objective>();           //Objectives for the playable side (only the human side has objectives)

    private string colourRebel;
    private string colourAuthority;
    private string colourNeutral;
    private string colourNormal;
    private string colourGood;
    private string colourBad;
    //private string colourGrey;
    private string colourAlert;
    private string colourSide;
    private string colourEnd;

    public void Initialise()
    {
        //Get objectives -> Placeholder
        listOfObjectives.AddRange(GameManager.instance.dataScript.GetRandomObjectives(maxNumOfObjectives));
        objectivesTotal = maxNumOfObjectives;
        //event listeners
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "ObjectiveManager");
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
        //colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
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
                if (GameManager.instance.sideScript.authorityOverall == SideState.Human)
                {
                    description = string.Format("{0}{1}{2}{3} of <b>{4}{5}</b> {6}Objectives have been completed{7}", colourNeutral, GetCompletedObjectives(),
                        colourEnd, colourNormal, colourEnd, objectivesTotal, colourNormal, colourEnd);
                }
                else { description = string.Format("{0}No Objectives for AI{1}", colourAlert, colourEnd); }
                break;
            case 2:
                if (GameManager.instance.sideScript.resistanceOverall == SideState.Human)
                {
                    description = string.Format("{0}{1}{2}{3} of <b>{4}{5}</b> {6}Objectives have been completed{7}", colourNeutral, GetCompletedObjectives(),
                        colourEnd, colourNormal, colourEnd, objectivesTotal, colourNormal, colourEnd);
                }
                else { description = string.Format("{0}No Objectives for AI{1}", colourAlert, colourEnd); }
                break;
        }
        return description;
    }

    /// <summary>
    /// returns colour formatted string based on Player side
    /// </summary>
    /// <returns></returns>
    public string GetObjectiveDetails()
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < listOfObjectives.Count; i++)
        {
            Objective objective = listOfObjectives[i];
            if (objective != null)
            {
                if (builder.Length > 0) { builder.AppendLine(); }
                builder.AppendFormat("{0}{1}{2}  ", colourNormal, objective.name, colourEnd);
                //objective complete if progress 100% or above
                if (objective.progress >= 100)
                { builder.AppendFormat("{0}Done{1}", colourGood, colourEnd); }
                else if (objective.progress > 0)
                { builder.AppendFormat("{0}{1} %{2}", colourNeutral, objective.progress, colourEnd); }
                else { builder.AppendFormat("{0}{1} %{2}", colourBad, objective.progress, colourEnd); }
            }
            else { Debug.LogWarningFormat("Invalid objective (Null) for listOfObjectives[{0}]", i); }
        }

        return builder.ToString();
    }

    /// <summary>
    /// returns the number of objectives that have been completed (for the human side)
    /// </summary>
    /// <returns></returns>
    public int GetCompletedObjectives()
    {
        int numCompleted = 0;
        for(int i = 0; i < listOfObjectives.Count; i++)
        {
            Objective objective = listOfObjectives[i];
            if (objective != null)
            {
                //objective complete if progress 100% or above
                if (objective.progress >= 100)
                { numCompleted++; }
            }
            else { Debug.LogWarningFormat("Invalid objective (Null) for listOfObjectives[{0}]", i); }
        }
        return numCompleted;
    }

    //new methods above here
}
