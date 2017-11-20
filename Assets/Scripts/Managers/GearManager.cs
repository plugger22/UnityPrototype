using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using modalAPI;
using gameAPI;

/// <summary>
/// handles all gear related matters (Only the Resistance has gear)
/// </summary>
public class GearManager : MonoBehaviour
{

    


    private string colourEffect;
    private string colourSide;
    private string colourGear;
    private string colourDefault;
    private string colourNormal;
    private string colourGood;
    private string colourActor;
    private string colourBad;
    private string colourEnd;



    // internal initialization
    void Start ()
    {
        //event Listeners
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
        EventManager.instance.AddListener(EventType.GearAction, OnEvent);
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

            case EventType.GearAction:
                ModalActionDetails details = Param as ModalActionDetails;
                InitialiseGenericPickerGear(details);
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
        colourGear = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourActor = GameManager.instance.colourScript.GetColour(ColourType.actorArc);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }


    public void Initialise()
    {
        Dictionary<int, Gear> dictOfGear = GameManager.instance.dataScript.GetAllGear();
        if (dictOfGear != null)
        {
            int gameLevel = GameManager.instance.GetMetaLevel();
            //set up an array of Lists, with index corresponding to GearLevel enum, eg. Common / Rare / Unique
            List<int>[] arrayOfGearLists = new List<int>[(int)GearLevel.Count];
            for(int i = 0; i < arrayOfGearLists.Length; i++)
            { arrayOfGearLists[i] = new List<int>(); }
            //loop dict and allocate gear to various lists
            foreach(var gearEntry in dictOfGear)
            {
                //check appropraite for metaLevel (same level or 'none', both are acceptable)
                if (gearEntry.Value.metaLevel == MetaLevel.None || (int)gearEntry.Value.metaLevel == gameLevel)
                {
                    //assign to a list based on rarity
                    arrayOfGearLists[(int)gearEntry.Value.rarity].Add(gearEntry.Key);
                }
            }
            //initialise dataManager lists with local lists
            for(int i = 0; i < arrayOfGearLists.Length; i++)
            { GameManager.instance.dataScript.SetGearList(arrayOfGearLists[i], (GearLevel)i); }
        }
        else { Debug.LogError("Invalid dictOfGear (Null) -> Gear not initialised"); }
    }


    /// <summary>
    /// Choose Gear (Resistance): sets up ModalGenericPicker class and triggers event: ModalGenericEvent.cs -> SetGenericPicker()
    /// </summary>
    /// <param name="details"></param>
    private void InitialiseGenericPickerGear(ModalActionDetails details)
    {
        bool errorFlag = false;
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        //does the node have any teams that can be neutralised?
        Node node = GameManager.instance.dataScript.GetNode(details.NodeID);
        if (node != null)
        {
            //double check to see if there are teams present at the node
            List<Team> listOfTeams = node.GetTeams();
            if (listOfTeams != null && listOfTeams.Count > 0)
            {
                genericDetails.returnEvent = EventType.GenericNeutraliseTeam;
                genericDetails.side = Side.Resistance;
                genericDetails.nodeID = details.NodeID;
                genericDetails.actorSlotID = details.ActorSlotID;
                //picker text
                genericDetails.textTop = string.Format("{0}Neutralise{1} {2}team{3}", colourEffect, colourEnd, colourNormal, colourEnd);
                genericDetails.textMiddle = string.Format("{0}Operatives are in place to Neutralise a team. The team will be forced to retire immediately{1}",
                    colourNormal, colourEnd);
                genericDetails.textBottom = "Click on a Team to Select. Press CONFIRM to Neutralise team. Mouseover teams for more information.";
                //loop teams present at node
                int turnsAgo;
                string dataColour;
                for (int i = 0; i < listOfTeams.Count; i++)
                {
                    Team team = listOfTeams[i];
                    if (team != null)
                    {
                        //option details
                        GenericOptionDetails optionDetails = new GenericOptionDetails();
                        optionDetails.optionID = team.TeamID;
                        optionDetails.text = team.Arc.name.ToUpper();
                        optionDetails.sprite = team.Arc.sprite;
                        //tooltip -> TO DO
                        GenericTooltipDetails tooltipDetails = new GenericTooltipDetails();
                        tooltipDetails.textHeader = string.Format("{0}{1}{2} {3}{4}{5}", colourGear, team.Arc.name.ToUpper(), colourEnd, colourNormal, team.Name, colourEnd);
                        turnsAgo = GameManager.instance.turnScript.Turn - team.TurnDeployed;
                        if (team.Timer > 0) { dataColour = colourGood; } else { dataColour = colourBad; }
                        tooltipDetails.textMain = string.Format("Will be immediately removed from the location.");
                        tooltipDetails.textDetails = string.Format("{0}Automatic success{1}", colourEffect, colourEnd);
                        //add to master arrays
                        genericDetails.arrayOfOptions[i] = optionDetails;
                        genericDetails.arrayOfTooltips[i] = tooltipDetails;
                    }
                    else { Debug.LogError(string.Format("Invalid team (Null) for listOfTeams[{0}]", i)); }
                    //check that limit hasn't been exceeded (max 3 options)
                    if (i > 2)
                    {
                        Debug.LogError(string.Format("Invalid number of Teams (more than 3) at NodeId {0}", details.NodeID));
                        break;
                    }
                }
            }
            else
            {
                Debug.LogError(string.Format("Invalid listOfTeams (Empty or Null) for NodeID {0}", details.NodeID));
                errorFlag = true;
            }
        }
        else
        {
            Debug.LogError(string.Format("Invalid Node (null) for nodeID {0}", details.NodeID));
            errorFlag = true;
        }
        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = Side.Resistance;
            outcomeDetails.textTop = "There has been an error in communication and No teams can be Neutralised.";
            outcomeDetails.textBottom = "Heads will roll!";
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
        }
        else
        {
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails);
        }
    }



    //new methods above here
}
