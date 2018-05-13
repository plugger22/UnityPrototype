﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using modalAPI;
using gameAPI;
using System.Text;

/// <summary>
/// Handles Modal Team Picker UI
/// </summary>
public class ModalTeamPicker : MonoBehaviour
{
    public GameObject modalTeamObject;
    public GameObject modalPanelObject;
    public Image modalPanel;

    public TextMeshProUGUI topText;
    public TextMeshProUGUI middleText;
    public TextMeshProUGUI bottomText;

    public Button buttonCancel;
    public Button buttonConfirm;

    public GameObject[] arrayOfTeamOptions;                //place team image UI elements here (should be seven)


    private CanvasGroup canvasGroup;
    private static ModalTeamPicker modalTeamPicker;

    private int teamIDSelected;                        //teamID of currently selected team, default '-1' value
    private int teamActorSlotID;                        //actorSlotID of actor who initiated the 'ANY TEAM' option
    private Node teamNode;                              //Node where the team is to be inserted


    private string colourEffect;
    private string colourSide;
    private string colourTeam;
    private string colourDefault;
    private string colourNormal;
    private string colourGood;
    private string colourBad;
    private string colourActor;
    private string colourEnd;

    private void Awake()
    {
        canvasGroup = modalPanel.GetComponent<CanvasGroup>();
        //confirm button event
        ButtonInteraction buttonInteraction = buttonConfirm.GetComponent<ButtonInteraction>();
        if (buttonInteraction != null)
        { buttonInteraction.SetEvent(EventType.ConfirmTeamChoice); }
        else { Debug.LogError("Invalid buttonInteraction Confirm (Null)"); }
        //cancel button event
        buttonInteraction = buttonCancel.GetComponent<ButtonInteraction>();
        if (buttonInteraction != null)
        { buttonInteraction.SetEvent(EventType.CloseTeamPicker); }
        else { Debug.LogError("Invalid buttonInteraction Cancel (Null)"); }
    }


    /// <summary>
    /// Initial set up
    /// </summary>
    public void Initialise()
    {
        //assign sprites to team Images
        Dictionary<int, TeamArc> dictOfTeamArcs = GameManager.instance.dataScript.GetDictOfTeamArcs();
        if (dictOfTeamArcs != null)
        {
            if (dictOfTeamArcs.Count != arrayOfTeamOptions.Length)
            { Debug.LogWarning(string.Format("dictOfTeamArcs.Count {0} != arrayOfTeamImages.Length {1}", dictOfTeamArcs.Count, arrayOfTeamOptions.Length)); }
            else
            {
                int limit = Mathf.Min(dictOfTeamArcs.Count, arrayOfTeamOptions.Length);
                //limit = Mathf.Min(dictOfTeamArcs.Count, arrayOfTeamTexts.Length);
                for (int index = 0; index < limit; index++)
                {
                    //get TeamArc from dict based on index
                    TeamArc arc = null;
                    if (dictOfTeamArcs.ContainsKey(index) == true)
                    {
                        arc = dictOfTeamArcs[index];
                        TeamInteraction teamUI = arrayOfTeamOptions[index].GetComponent<TeamInteraction>();
                        if (teamUI != null)
                        {
                            //assign to sprite 
                            teamUI.teamImage.sprite = arc.sprite;
                            //assign to text (name of teamArc)
                            teamUI.teamText.text = arc.name;
                        }
                        else { Debug.LogError("Invalid TeamChoicUI component (Null)"); }


                    }
                    else { Debug.LogWarning(string.Format("Invalid arc index \"{0}\" for \"{1}\" -> No Sprite assigned", index, arc.name)); }
                }
            }
        }
        else { Debug.LogError("Invalid dictOfTeamArcs (null) -> Sprites not assigned to ModalTeamPicker"); }
        //register listener
        EventManager.instance.AddListener(EventType.OpenTeamPicker, OnEvent, "ModalTeamPicker");
        EventManager.instance.AddListener(EventType.CloseTeamPicker, OnEvent, "ModalTeamPicker");
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "ModalTeamPicker");
        EventManager.instance.AddListener(EventType.ConfirmTeamActivate, OnEvent, "ModalTeamPicker");
        EventManager.instance.AddListener(EventType.ConfirmTeamChoice, OnEvent, "ModalTeamPicker");
        EventManager.instance.AddListener(EventType.ConfirmTeamDeactivate, OnEvent, "ModalTeamPicker");
    }



    /// <summary>
    /// Static instance so the Modal Team Picker can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalTeamPicker Instance()
    {
        if (!modalTeamPicker)
        {
            modalTeamPicker = FindObjectOfType(typeof(ModalTeamPicker)) as ModalTeamPicker;
            if (!modalTeamPicker)
            { Debug.LogError("There needs to be one active ModalTeamPicker script on a GameObject in your scene"); }
        }
        return modalTeamPicker;
    }

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
            case EventType.OpenTeamPicker:
                ModalActionDetails details = Param as ModalActionDetails;
                SetTeamPicker(details);
                break;
            case EventType.CloseTeamPicker:
                CloseTeamPicker();
                break;
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.ConfirmTeamActivate:
                SetConfirmButton(true, (int)Param);
                break;
            case EventType.ConfirmTeamDeactivate:
                SetConfirmButton(false);
                break;
            case EventType.ConfirmTeamChoice:
                ProcessTeamChoice();
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
        colourEffect = GameManager.instance.colourScript.GetColour(ColourType.actionEffect);
        colourSide = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourTeam = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourActor = GameManager.instance.colourScript.GetColour(ColourType.actorArc);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }


    /// <summary>
    /// Initialise and Activate Team Picker (insert ANY TEAM)
    /// </summary>
    public void SetTeamPicker(ModalActionDetails details)
    {
        StringBuilder builder = new StringBuilder();
        CanvasGroup teamCanvasGroup;
        TeamInteraction teamInteract;
        string textTooltip;
        GameManager.instance.guiScript.SetIsBlocked(true);
        modalTeamObject.SetActive(true);
        modalPanelObject.SetActive(true);
        //confirm button should be switched off at the start
        buttonConfirm.gameObject.SetActive(false);
        canvasGroup.alpha = 100;
        //Set up texts
        topText.text = string.Format("{0}Select {1}{2}ANY{3}{4} Team{5}", colourDefault, colourEnd, colourEffect, colourEnd, colourDefault, colourEnd);
        Node node = GameManager.instance.dataScript.GetNode(details.nodeID);
        if (node != null)
        {
            //track core data needed to resolve Insert team action
            teamNode = node;
            teamActorSlotID = details.actorDataID;
            Actor actor = GameManager.instance.dataScript.GetCurrentActor(teamActorSlotID, GameManager.instance.globalScript.sideAuthority);
            int numTeams = node.CheckNumOfTeams();
            builder.AppendFormat("{0}{1} \"{2}\", {3} Team{4} present{5}", colourNormal, node.Arc.name, node.nodeName, numTeams, 
                numTeams != 1 ? "s" : "", colourEnd);
            //teams at node
            if (numTeams > 0)
            {
                List<Team> listOfTeams = node.GetTeams();
                if (listOfTeams != null)
                {
                    if (listOfTeams.Count > 0)
                    {
                        builder.AppendFormat("{0} ({1}{2}", colourNormal, colourEnd, colourTeam);
                        int counter = 0;
                        foreach (Team team in listOfTeams)
                        {
                            builder.Append(team.arc.name);
                            counter++;
                            if (counter < listOfTeams.Count)
                            { builder.Append(", "); }
                        }
                        builder.AppendFormat("{0}{1}){2}", colourEnd, colourNormal, colourEnd);
                    }
                }
                else { Debug.LogError("Invalid listOfTeams (Null)"); }
            }
            //Actor
            if (actor != null)
            {
                builder.AppendLine();
                string colourNumbers = colourGood;
                if (actor.CheckNumOfTeams() == actor.datapoint2)
                { colourNumbers = colourBad; }
                builder.AppendFormat("{0}, {1} of {2}{3}{4} has deployed {5}{6}{7} of {8}{9}{10} teams",
                    actor.actorName, GameManager.instance.metaScript.GetAuthorityTitle(), colourActor, actor.arc.name, colourEnd,
                    colourNumbers, actor.CheckNumOfTeams(), colourEnd, colourNumbers, actor.datapoint2, colourEnd);
            }
            else { Debug.LogError(string.Format("Invalid actor (Null) from ActorSlotID {0}", teamActorSlotID)); }
        }
        else { Debug.LogError(string.Format("Invalid node (Null) for details.NodeID {0}", details.nodeID)); }
        middleText.text = builder.ToString();
        //
        // - - - Teams - - -
        //
        //Get list of team Arcs
        int teamID, numOfTeams;
        string teamType = "Unknown";
        List<int> listOfTeamArcIDs = GameManager.instance.dataScript.GetTeamArcIDs();       //all lists are keyed off this one, index-wise
        List<int> listOfTeamIDs = new List<int>();                                          //place teamID of first available team in reserve pool of that type
        List<string> listOfTeamTooltipsMain = new List<string>();                           //holds tooltip for team options, one for each team Arc, main text
        List<string> listOfTeamTooltipsHeader = new List<string>();                         //tooltip header ("CORPORATE")
        List<string> listOfTeamTooltipsDetails = new List<string>();                        //breakdown of team type details
        {
            if (listOfTeamArcIDs != null || listOfTeamArcIDs.Count > 0)
            {
                //loop team Arcs
                for (int arcIndex = 0; arcIndex < listOfTeamArcIDs.Count; arcIndex++)
                {
                    textTooltip = "Unknown";
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, arcIndex);
                    if (teamID == -1)
                    { textTooltip = "No teams of this type are currently in the Reserve Pool"; }
                    //if a team of that type is available (teamID > -1) check if a duplicate team already exists at node
                    else
                    {
                        if (node.CheckTeamPresent(arcIndex) > -1)
                        {
                            //change teamID to -1 (invalid team as you can't insert a team of a type already present at the node)
                            teamID = -1;
                            textTooltip = "A team of this type is already present at the Node";
                        }
                    }
                    //add to list
                    listOfTeamIDs.Add(teamID);
                    //tooltip data
                    if (teamID > -1)
                    {
                        //get team
                        Team team = GameManager.instance.dataScript.GetTeam(teamID);
                        if (team != null)
                        {
                            textTooltip = string.Format("{0} {1} is available and awaiting deployment", team.arc.name, team.teamName);
                            //default team tooltip header
                            teamType = team.arc.name;
                        }
                        else { Debug.LogError(string.Format("Invalid Team (Null) for teamID {0}", teamID)); }
                    }
                    else
                    {
                        teamType = GameManager.instance.dataScript.GetTeamArc(arcIndex).name;
                    }
                    //header tooltip text
                    listOfTeamTooltipsHeader.Add(string.Format("{0}{1}{2}", colourSide, teamType, colourEnd));
                    //main tooltip text
                    listOfTeamTooltipsMain.Add(textTooltip);
                    //details tooltip text
                    numOfTeams = GameManager.instance.dataScript.CheckTeamInfo(arcIndex, TeamInfo.Total);
                    StringBuilder builderDetails = new StringBuilder();
                    builderDetails.AppendFormat("{0}{1} {2} team{3}{4}", colourEffect, numOfTeams, teamType,
                        numOfTeams != 1 ? "s" : "", colourEnd);
                    builderDetails.AppendLine();
                    numOfTeams = GameManager.instance.dataScript.CheckTeamInfo(arcIndex, TeamInfo.Reserve);
                    builderDetails.AppendFormat("{0}{1} in Reserve{2}", colourEffect, numOfTeams, colourEnd);
                    builderDetails.AppendLine();
                    numOfTeams = GameManager.instance.dataScript.CheckTeamInfo(arcIndex, TeamInfo.OnMap);
                    builderDetails.AppendFormat("{0}{1} Deployed{2}", colourEffect, numOfTeams, colourEnd);
                    builderDetails.AppendLine();
                    numOfTeams = GameManager.instance.dataScript.CheckTeamInfo(arcIndex, TeamInfo.InTransit);
                    builderDetails.AppendFormat("{0}{1} in Transit{2}", colourEffect, numOfTeams, colourEnd);
                    listOfTeamTooltipsDetails.Add(builderDetails.ToString());
                }

            }
            else { Debug.LogError("Invalid listOfTeamArcIDs (Null or Empty)"); }
        }

        //loop list of Teams and deactivate those that aren't valid picks
        int limit = arrayOfTeamOptions.Length;
        for (int teamIndex = 0; teamIndex < listOfTeamIDs.Count; teamIndex++)
        {
            //get option canvas
            teamCanvasGroup = arrayOfTeamOptions[teamIndex].GetComponent<CanvasGroup>();
            //get TeamInteraction component
            teamInteract = arrayOfTeamOptions[teamIndex].GetComponent<TeamInteraction>();
            if (teamIndex < limit)
            {
                if (listOfTeamIDs[teamIndex] == -1)
                {
                    if (teamCanvasGroup != null)
                    {
                        //deactivate option
                        teamCanvasGroup.alpha = 0.25f;
                        teamCanvasGroup.interactable = false;
                    }
                    else { Debug.LogError(string.Format("Invalid teamCanvasGroup (Null) for listOfTeamIDs[\"{0}\"]", teamIndex)); }
                    if (teamInteract != null)
                    {
                        //deactivate team selection
                        teamInteract.isActive = false;
                        teamInteract.teamID = -1;
                    }
                    else { Debug.LogError(string.Format("Invalid teamInteract (Null) for listOfTeamIDs[\"{0}\"]", teamIndex)); }
                }
                else
                {
                    if (teamCanvasGroup != null)
                    {
                        //activate option
                        teamCanvasGroup.alpha = 1.0f;
                        teamCanvasGroup.interactable = true;
                    }
                    else { Debug.LogError(string.Format("Invalid teamCanvasGroup (Null) for listOfTeamIDs[\"{0}\"]", teamIndex)); }
                    if (teamInteract != null)
                    {
                        //Activate team selection
                        teamInteract.isActive = true;
                        teamInteract.teamID = listOfTeamIDs[teamIndex];
                    }
                    else { Debug.LogError(string.Format("Invalid teamInteract (Null) for listOfTeamIDs[\"{0}\"]", teamIndex)); }
                }
                //add tooltip
                GenericTooltipUI optionTooltip = arrayOfTeamOptions[teamIndex].GetComponent<GenericTooltipUI>();
                optionTooltip.tooltipHeader = listOfTeamTooltipsHeader[teamIndex];
                optionTooltip.tooltipMain = listOfTeamTooltipsMain[teamIndex];
                optionTooltip.tooltipEffect = listOfTeamTooltipsDetails[teamIndex];
            }
            else { Debug.LogWarning(string.Format("teamIndex \"{0}\" has exceeded limit \"{1}\"", teamIndex, limit)); }
        }

        //set game state
        GameManager.instance.inputScript.SetModalState(ModalState.TeamPicker);
        Debug.Log("UI: Open -> ModalTeamPicker" + "\n");
    }





    /// <summary>
    /// close Action Menu
    /// </summary>
    private void CloseTeamPicker()
    {
        modalTeamObject.SetActive(false);
        GameManager.instance.guiScript.SetIsBlocked(false);
        //deselect all teams to prevent picker opening next time with a preselected team
        EventManager.instance.PostNotification(EventType.DeselectOtherTeams, this);
        SetConfirmButton(false);
        //set game state
        GameManager.instance.inputScript.ResetStates();
        Debug.Log("UI: Close -> ModalTeamPicker" + "\n");
    }


    /// <summary>
    /// Confirm button switched on/off. Only ON and visible if a team has been selected
    /// </summary>
    /// <param name="activate"></param>
    public void SetConfirmButton(bool isActive, int teamID = -1)
    {
        string text = "Unknown";;
        if (isActive == true)
        {
            buttonConfirm.gameObject.SetActive(true);
            if (teamID > -1)
            {
                //change Top text to show which team is selected
                Team team = GameManager.instance.dataScript.GetTeam(teamID);
                if (team != null)
                {
                    text = string.Format("{0}{1} Team {2}{3}selected{4}", colourEffect, team.arc.name, colourEnd, colourDefault, colourEnd);
                    //record most recently chosen selection
                    teamIDSelected = teamID;
                    Debug.Log(string.Format("TeamPicker: teamArcID {0} selected{1}", teamID, "\n"));
                }
                else { Debug.LogError(string.Format("Invalid team (Null) for teamID {0}", teamID)); }
            }
        }
        else
        {
            buttonConfirm.gameObject.SetActive(false);
            text = string.Format("{0}Select {1}{2}ANY{3}{4} Team{5}", colourDefault, colourEnd, colourEffect, colourEnd, colourDefault, colourEnd);
        }
        //update top text
        topText.text = text;
    }


    /// <summary>
    /// Click confirm, carry out Team insert and exit picker
    /// </summary>
    private void ProcessTeamChoice()
    {
        modalTeamObject.SetActive(false);
        GameManager.instance.guiScript.SetIsBlocked(false);
        //deselect all teams to prevent picker opening next time with a preselected team
        EventManager.instance.PostNotification(EventType.DeselectOtherTeams, this);
        //set game state
        GameManager.instance.inputScript.GameState = GameState.Normal;
        Debug.Log(string.Format("UI: Close -> ModalTeamPicker" + "\n"));
        Debug.Log(string.Format("TeamPicker: Confirm teamID {0}{1}", teamIDSelected, "\n"));
        //insert team
        if (teamIDSelected > -1)
        {
            GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamIDSelected, teamActorSlotID, teamNode);
        }
        else { Debug.LogError(string.Format("Invalid teamIDSelected \"{0}\" -> insert team operation cancelled", teamIDSelected)); }
        //outcome dialogue windows
        ModalOutcomeDetails details = new ModalOutcomeDetails();
        details.side = GameManager.instance.globalScript.sideAuthority;
        bool successFlag = true;
        if (teamIDSelected > -1)
        {
            details.textTop = GameManager.instance.effectScript.SetTopTeamText(teamIDSelected);
            Actor actor = GameManager.instance.dataScript.GetCurrentActor(teamActorSlotID, GameManager.instance.globalScript.sideAuthority);
            if (actor != null)
            { details.textBottom = GameManager.instance.effectScript.SetBottomTeamText(actor); }
            else { successFlag = false; }
            Team team = GameManager.instance.dataScript.GetTeam(teamIDSelected);
            if (team != null)
            {
                TeamInteraction teamInteract = arrayOfTeamOptions[team.arc.TeamArcID].GetComponent<TeamInteraction>();
                if (teamInteract != null)
                { details.sprite = teamInteract.teamImage.sprite; }
                else { successFlag = false; }
            }
            else { successFlag = false; }
        }
        //something went wrong, default message
        if (successFlag == false)
        {
            details.textTop = "There have been unexplained delays and no team has been inserted";
            details.textBottom = "As soon as you've identified who is at fault heads will roll";
            details.sprite = GameManager.instance.guiScript.errorSprite;
        }
        //action expended if successful
        if (successFlag == true)
        { details.isAction = true; }
        //fire up Outcome dialogue
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details);
    }
}
