using System.Collections;
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
    public Image modalPanel;

    public TextMeshProUGUI topText;
    public TextMeshProUGUI middleText;
    public TextMeshProUGUI bottomText;

    public Button buttonCancel;
    public Button buttonConfirm;
    
    public GameObject[] arrayOfTeamOptions;                //place team image UI elements here (should be seven)
    public TextMeshProUGUI[] arrayOfTeamTexts;       //place team texts UI elements here (should be seven)


    private CanvasGroup canvasGroup;
    private static ModalTeamPicker modalTeamPicker;

    private string colourEffect;
    private string colourSide;
    private string colourDefault;
    private string colourNormal;
    private string colourEnd;

    private void Start()
    {
        canvasGroup = modalPanel.GetComponent<CanvasGroup>();
    }

    public void Initialise()
    {
        //assign sprites to team Images
        Dictionary<int, TeamArc> dictOfTeamArcs = GameManager.instance.dataScript.GetTeamArcs();
        if (dictOfTeamArcs != null)
        {
            if (dictOfTeamArcs.Count != arrayOfTeamOptions.Length)
            { Debug.LogWarning(string.Format("dictOfTeamArcs.Count {0} != arrayOfTeamImages.Length {1}", dictOfTeamArcs.Count, arrayOfTeamOptions.Length)); }
            else
            {
                if (dictOfTeamArcs.Count != arrayOfTeamTexts.Length)
                { Debug.LogWarning(string.Format("dictOfTeamArcs.Count {0} != arrayOfTeamTexts.Length {1}", dictOfTeamArcs.Count, arrayOfTeamTexts.Length)); }
                else
                {
                    int limit = Mathf.Min(dictOfTeamArcs.Count, arrayOfTeamOptions.Length);
                    limit = Mathf.Min(dictOfTeamArcs.Count, arrayOfTeamTexts.Length);
                    for (int index = 0; index < limit; index++)
                    {
                        //get TeamArc from dict based on index
                        TeamArc arc = null;
                        if (dictOfTeamArcs.ContainsKey(index) == true)
                        {
                            arc = dictOfTeamArcs[index];
                            TeamChoiceUI teamUI = arrayOfTeamOptions[index].GetComponent<TeamChoiceUI>();
                            if (teamUI != null)
                            {
                                //assign to sprite 
                                teamUI.teamImage.sprite = arc.sprite;
                                //assign to text (name of teamArc)
                                teamUI.name.text = arc.name;
                            }
                            else { Debug.LogError("Invalid TeamChoicUI component (Null)"); }


                        }
                        else { Debug.LogWarning(string.Format("Invalid arc index \"{0}\" for \"{1}\" -> No Sprite assigned", index, arc.name)); }
                    }
                }
            }
        }
        else { Debug.LogError("Invalid dictOfTeamArcs (null) -> Sprites not assigned to ModalTeamPicker"); }
        //register listener
        EventManager.instance.AddListener(EventType.OpenTeamPicker, OnEvent);
        EventManager.instance.AddListener(EventType.CloseTeamPicker, OnEvent);
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
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
                ModalActionDetails detailsTeam = Param as ModalActionDetails;
                CloseTeamPicker();
                break;
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Initialise and Activate Team Picker (insert ANY TEAM)
    /// </summary>
    public void SetTeamPicker(ModalActionDetails details)
    {
        StringBuilder builder = new StringBuilder();
        CanvasGroup teamCanvasGroup;
        string textTooltip;
        GameManager.instance.Blocked(true);
        modalTeamObject.SetActive(true);
        canvasGroup.alpha = 100;
        //Set up texts
        topText.text = string.Format("{0}Select {1}{2}ANY{3}{4} Team{5}", colourDefault, colourEnd, colourEffect, colourEnd, colourDefault, colourEnd);
        //node details
        
        Node node = GameManager.instance.dataScript.GetNode(details.NodeID);
        if (node != null)
        {
            int numTeams = node.CheckNumOfTeams();
            builder.Append(string.Format("{0}{1} \"{2}\"{3}", colourNormal, node.arc.name.ToUpper(), node.NodeName, colourEnd));
            builder.AppendLine();
            builder.Append(string.Format("{0}Currently {1} Team{2} present{3}", colourNormal, numTeams, numTeams != 1 ? "s" : "", colourEnd));
        }
        else { Debug.LogError(string.Format("Invalid node (Null) for details.NodeID {0}", details.NodeID)); }
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
                        if (node.CheckTeamPresent(arcIndex) == true)
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
                            textTooltip = string.Format("{0} {1} is available and awaiting deployment", team.Arc.name, team.Name);
                            //default team tooltip header
                            teamType = string.Format("{0}{1}{2}", colourSide, team.Arc.name.ToUpper(), colourEnd);
                        }
                        else { Debug.LogError(string.Format("Invalid Team (Null) for teamID {0}", teamID)); }
                    }
                    else
                    {
                        teamType = string.Format("{0}{1}{2}", colourSide, GameManager.instance.dataScript.GetTeamArc(arcIndex).name.ToUpper(), 
                            colourEnd);
                    }
                    //header tooltip text
                    listOfTeamTooltipsHeader.Add(teamType);
                    //main tooltip text
                    listOfTeamTooltipsMain.Add(textTooltip);
                    //details tooltip text
                    numOfTeams = GameManager.instance.dataScript.CheckTeamInfo(arcIndex, TeamInfo.Total);
                    StringBuilder builderDetails = new StringBuilder();
                    builderDetails.Append(string.Format("{0}{1} {2} team{3}{4}", colourEffect, numOfTeams, teamType, 
                        numOfTeams != 1 ? "s" : "", colourEnd));
                    builderDetails.AppendLine();
                    numOfTeams = GameManager.instance.dataScript.CheckTeamInfo(arcIndex, TeamInfo.Reserve);
                    builderDetails.Append(string.Format("{0}{1} in Reserve{2}", colourEffect, numOfTeams, colourEnd));
                    builderDetails.AppendLine();
                    numOfTeams = GameManager.instance.dataScript.CheckTeamInfo(arcIndex, TeamInfo.OnMap);
                    builderDetails.Append(string.Format("{0}{1} Deployed{2}", colourEffect, numOfTeams, colourEnd));
                    builderDetails.AppendLine();
                    numOfTeams = GameManager.instance.dataScript.CheckTeamInfo(arcIndex, TeamInfo.InTransit);
                    builderDetails.Append(string.Format("{0}{1} in Transit{2}", colourEffect, numOfTeams, colourEnd));
                    listOfTeamTooltipsDetails.Add(builderDetails.ToString());
                }

            }
            else { Debug.LogError("Invalid listOfTeamArcIDs (Null or Empty)"); }
        }

        //loop list of Teams and deactivate those that are valid picks
        int limit = arrayOfTeamOptions.Length;
        for (int teamIndex = 0; teamIndex < listOfTeamIDs.Count; teamIndex++)
        {                
            //get option canvas
            teamCanvasGroup = arrayOfTeamOptions[teamIndex].GetComponent<CanvasGroup>();
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
                }
                else
                {
                    //activate option
                    teamCanvasGroup.alpha = 1.0f;
                    teamCanvasGroup.interactable = true;
                }
                //add tooltip
                GenericTooltipUI optionTooltip = arrayOfTeamOptions[teamIndex].GetComponent<GenericTooltipUI>();
                optionTooltip.ToolTipHeader = listOfTeamTooltipsHeader[teamIndex];
                optionTooltip.ToolTipMain = listOfTeamTooltipsMain[teamIndex];
                optionTooltip.ToolTipEffect = listOfTeamTooltipsDetails[teamIndex];
            }
            else { Debug.LogWarning(string.Format("teamIndex \"{0}\" has exceeded limit \"{1}\"", teamIndex, limit)); }
        }

        //are their teams available in the reserve pool?
        //can't have identical teams to what already exists on node

        //set Cancel Button
        buttonCancel.onClick.RemoveAllListeners();
        buttonCancel.onClick.AddListener(CloseTeamPicker);
        //set Confirm Button
        buttonConfirm.onClick.RemoveAllListeners();
        buttonConfirm.onClick.AddListener(CloseTeamPicker);
        //buttonConfirm.onClick.AddListener(buttonDetails.action);

        //set game state
        GameManager.instance.inputScript.GameState = GameState.ModalTeamPicker;
        Debug.Log("UI: Open -> ModalTeamPicker" + "\n");
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
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// close Action Menu
    /// </summary>
    public void CloseTeamPicker()
    {
        modalTeamObject.SetActive(false);
        GameManager.instance.Blocked(false);

        //set game state
        GameManager.instance.inputScript.GameState = GameState.Normal;
        Debug.Log("UI: Close -> ModalTeamPicker" + "\n");
    }
}
