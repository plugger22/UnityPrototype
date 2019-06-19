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

    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseEvents();
                SubInitialiseObjectives();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseObjectives();
                break;
            case GameState.LoadAtStart:
                SubInitialiseEvents();
                SubInitialiseObjectives();
                break;
            case GameState.LoadGame:
                SubInitialiseObjectives();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }

    #region Initialisation SubMethods

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event listeners
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "ObjectiveManager");
    }
    #endregion

    #region SubInitialiseObjectives
    private void SubInitialiseObjectives()
    {
        /*//Get objectives -> Placeholder
        listOfObjectives.Clear();
        listOfObjectives.AddRange(GameManager.instance.dataScript.GetRandomObjectives(maxNumOfObjectives));*/
        objectivesTotal = maxNumOfObjectives;
    }
    #endregion

    #endregion

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
    /// clears list before adding new objectives. Only adds up to the maxNumOfObjectives, any extras are ignored. Sets progress of all nominated objectives to zero
    /// Will reset objective progress meters back to zero by default
    /// </summary>
    /// <param name="listOfSetObjectives"></param>
    public void SetObjectives(List<Objective> listOfSetObjectives, bool isZeroProgress = true)
    {
        if (listOfSetObjectives != null)
        {
            int counter = 0;
            listOfObjectives.Clear();
            foreach(Objective objective in listOfSetObjectives)
            {
                if (objective != null)
                {
                    //reset progress to zero
                    if (isZeroProgress == true)
                    { objective.progress = 0; }
                    //add to list
                    listOfObjectives.Add(objective);
                    Debug.LogFormat("[Obj] ObjectiveManager.cs -> SetObjectives: {0} progress {1}{2}", objective.tag, objective.progress, "\n");
                    //objectives correspond to widget stars (top UI) depending on their index position in the list ([0] is first, [1] is second, etc.)
                    SetObjectiveStar(counter, objective.progress);
                    counter++;
                    //can't have more than max objectives
                    if (counter >= maxNumOfObjectives)
                    { break; }
                }
                else { Debug.LogWarning("Invalid objective (Null) in listOfSetObjectives"); }
            }
            objectivesCurrent = listOfObjectives.Count;
        }
        else { Debug.LogError("Invalid listOfSetObjectives (Null)"); }
    }


    public List<Objective> GetListOfObjectives()
    { return listOfObjectives; }


    /// <summary>
    /// Checks if objective of name present (ObjectiveSO.name) and, if so, adjusts progress by specified amount with min/max cap 0 and 100). Adjusts relevant Objective star in top widget UI as well.
    /// </summary>
    /// <param name="objectiveName"></param>
    /// <param name="progressAdjust"></param>
    public void UpdateObjectiveProgress(string objectiveName, int progressAdjust)
    {
        if (string.IsNullOrEmpty(objectiveName) == false)
        {
            //present?
            Objective objective = listOfObjectives.Find(x => x.name == objectiveName);
            if (objective != null)
            {
                objective.progress += progressAdjust;
                Mathf.Clamp(objective.progress, 0, 100);
                Debug.LogFormat("[Obj] ObjectiveManager.cs -> SetObjectiveProgress: {0} now at progress {1} (adjust {2}){3}", objective.tag, objective.progress, progressAdjust, "\n");
                //set appropriate star opacity in top widget UI
                int index = listOfObjectives.FindIndex(x => x.name == objectiveName);
                //objectives correspond to widget stars depending on their index position in the list ([0] is first, [1] is second, etc.)
                SetObjectiveStar(index, objective.progress);
            }
            else { Debug.LogWarningFormat("Objective \"{0}\" not found in listOfObjectives", objectiveName); }
        }
        else { Debug.LogError("Invaiid objectiveName (Null)"); }
    }

    /// <summary>
    /// SubMethod to set objective stars to an opacity level corresponding to objective progress (% scale) with a default 10% value for 0 progress (so star can be seen as empty)
    /// </summary>
    /// <param name="index"></param>
    /// <param name="opacity"></param>
    private void SetObjectiveStar(int index, int progress)
    {
        float opacity = progress;
        //min cap 10% in order to keep star faintly visibile in background)
        Mathf.Clamp(opacity, 10.0f, 100.0f);
        //objectives correspond to widget stars depending on their index position in the list ([0] is first, [1] is second, etc.)
        switch (index)
        {
            case 0:
                //left hand, first star/objective
                EventManager.instance.PostNotification(EventType.ChangeStarLeft, this, opacity, "ObjectiveManager.cs -> SetObjectiveProgress");
                break;
            case 1:
                //middle, second star/objective
                EventManager.instance.PostNotification(EventType.ChangeStarMiddle, this, opacity, "ObjectiveManager.cs -> SetObjectiveProgress");
                break;
            case 2:
                //right hand, third star/objective
                EventManager.instance.PostNotification(EventType.ChangeStarRight, this, opacity, "ObjectiveManager.cs -> SetObjectiveProgress");
                break;
            default:
                Debug.LogWarningFormat("Index not valid, {0}", index);
                break;
        }
    }

    /// <summary>
    /// checks whether a successfully completed target has a progress effect on a current objective
    /// called by TargetManager.cs -> SetTargetDone automatically whenever a target is 'done' (doesn't matter if there are ongoing effects and target is outstanding)
    /// </summary>
    /// <param name="targetName"></param>
    public void CheckObjectiveTarget(string targetName)
    {
        if (string.IsNullOrEmpty(targetName) == false)
        {
            List<ObjectiveTarget> listOfObjectiveTargets = GameManager.instance.missionScript.mission.listOfObjectiveTargets;
            //no need for error as list may well be null (no targets in this mission relate to objectives)
            if (listOfObjectiveTargets != null)
            {
                //target present
                ObjectiveTarget objectiveTarget = listOfObjectiveTargets.Find(x => x.target.name == targetName);
                //no need for error check as a match may not be present
                if (objectiveTarget != null)
                {
                    //update progress of objective
                    UpdateObjectiveProgress(objectiveTarget.objective.name, objectiveTarget.adjustment);
                }
            }
        }
        else { Debug.LogError("Invalid targetName (Null or Empty)"); }
    }


    //
    // - - - UI request methods for top Widget UI tooltip
    //

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
                    description = string.Format("{0}{1}{2}{3} of <b>{4}{5}{6}{7}</b> Objectives have been completed{8}", colourNeutral, GetCompletedObjectives(),
                        colourEnd, colourNormal, colourEnd, colourNeutral, objectivesCurrent, colourNormal, colourEnd);
                }
                else { description = string.Format("{0}No Objectives for AI{1}", colourAlert, colourEnd); }
                break;
            case 2:
                if (GameManager.instance.sideScript.resistanceOverall == SideState.Human)
                {
                    description = string.Format("{0}{1}{2}{3} of <b>{4}{5}{6}{7}</b> Objectives have been completed{8}", colourNeutral, GetCompletedObjectives(),
                        colourEnd, colourNormal, colourEnd, colourNeutral, objectivesCurrent, colourNormal, colourEnd);
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
                builder.AppendFormat("{0}{1}{2}  ", colourNormal, objective.tag, colourEnd);
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
