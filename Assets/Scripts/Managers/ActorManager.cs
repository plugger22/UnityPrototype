using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using modalAPI;
using System;
using Random = UnityEngine.Random;
using System.Text;
using UnityEngine.Events;

/// <summary>
/// handles Actor related data and methods
/// </summary>
public class ActorManager : MonoBehaviour
{

    [HideInInspector] public int numOfActorsCurrent;    //NOTE -> Not hooked up yet (need to show blank actors for any that aren't currently in use)
    public int numOfActorsTotal = 4;      //if you increase this then GUI elements and GUIManager will need to be changed to accomodate it, default value 4
                                          //is the total for one side (duplicated by the other side)

    public int numOfQualities = 3;        //number of qualities actors have (different for each side), eg. "Connections, Invisibility" etc. Map to DataPoint0 -> DataPoint'x'

    //private Actor[,] arrayOfActors;       //indexes [Side, numOfActors], two sets are created, one for each side

    //colour palette for Generic tool tip
    private string colourBlue;
    private string colourRed;
    private string colourCancel;
    private string colourInvalid;
    private string colourEffect;
    private string colourDefault;
    private string colourEnd;


    public void PreInitialiseActors()
    {
        //number of actors, default 4
        numOfActorsTotal = numOfActorsTotal == 4 ? numOfActorsTotal : 4;
        numOfActorsCurrent = numOfActorsCurrent < 1 ? 1 : numOfActorsCurrent;
        numOfActorsCurrent = numOfActorsCurrent > numOfActorsTotal ? numOfActorsTotal : numOfActorsCurrent;
    }


    public void Initialise()
    {
        //event listener is registered in InitialiseActors() due to GameManager sequence.
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
        
        InitialiseActors(numOfActorsTotal, Side.Resistance);
        InitialiseActors(numOfActorsTotal, Side.Authority);
    }

    /// <summary>
    /// handles events
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
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
    /// deregisters events
    /// </summary>
    public void OnDisable()
    {
        EventManager.instance.RemoveEvent(EventType.NodeAction);
        EventManager.instance.RemoveEvent(EventType.TargetAction);
    }

    /// <summary>
    /// set colour palette for Generic Tool tip
    /// </summary>
    public void SetColours()
    {
        colourBlue = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourRed = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        colourCancel = GameManager.instance.colourScript.GetColour(ColourType.cancelNormal);
        colourInvalid = GameManager.instance.colourScript.GetColour(ColourType.cancelHighlight);
        colourEffect = GameManager.instance.colourScript.GetColour(ColourType.actionEffect);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }



    /// <summary>
    /// Set up number of required actors (minions supporting play)
    /// </summary>
    /// <param name="num"></param>
    public void InitialiseActors(int num, Side side)
    {
        if (num > 0)
        {
            //get a list of random actorArcs
            List<ActorArc> tempActorArcs = GameManager.instance.dataScript.GetRandomActorArcs(num, side);
            //Create actors
            for (int i = 0; i < num; i++)
            {
                Actor actor = new Actor()
                {
                    Arc = tempActorArcs[i],
                    Name = tempActorArcs[i].actorName,
                    SlotID = i,
                    Datapoint0 = Random.Range(1, 4),
                    Datapoint1 = Random.Range(1, 4),
                    Datapoint2 = Random.Range(1, 4),
                    ActorSide = side,
                    trait = GameManager.instance.dataScript.GetRandomTrait(),
                    isLive = true
                };

                //add actor to array
                GameManager.instance.dataScript.AddActor(side, actor, i);

                Debug.Log(string.Format("Actor added -> {0}, {1} {2}{3}", actor.Arc.actorName,
                    GameManager.instance.dataScript.GetQuality(GameManager.instance.optionScript.PlayerSide, 0), actor.Datapoint0, "\n"));
            }
        }
        else { Debug.LogWarning("Invalid number of Actors (Zero, or less)"); }

    }


    /// <summary>
    /// Returns a list of all relevant Actor Actions for the  node to enable a ModalActionMenu to be put together (one button per action). 
    /// Max 4 Actor + 1 Target actions with an additional 'Cancel' buttnn added last automatically
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public List<EventButtonDetails> GetActorActions(int nodeID)
    {
        string sideColour;
        string cancelText = null;
        string effectCriteria;
        bool proceedFlag;
        int actionID;
        Actor[] arrayOfActors;
        Side side = GameManager.instance.optionScript.PlayerSide;
        //color code for button tooltip header text, eg. "Operator"ss
        if ( side == Side.Authority)
        { sideColour = colourRed; }
        else { sideColour = colourBlue; }
        List<EventButtonDetails> tempList = new List<EventButtonDetails>();
        //string builder for info button (handles all no go cases
        StringBuilder infoBuilder = new StringBuilder();
        //player's current node
        int playerID = GameManager.instance.nodeScript.nodePlayer;
        //Get Node
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        if (node != null)
        {
            List<Effect> listOfEffects = new List<Effect>();
            Action tempAction;
            EventButtonDetails details;
            //
            // - - - Resistance - - -
            //
            if (side == Side.Resistance)
            {
                //'Cancel' button tooltip)
                if (nodeID == playerID)
                { cancelText = "You are present at the node"; }
                else { cancelText = "You are NOT present at the node"; }
                //
                // - - -  Target - - -
                //
                //Does the Node have a target attached? -> added first
                if (node.TargetID >= 0)
                {
                    Target target = GameManager.instance.dataScript.GetTarget(node.TargetID);
                    if (target != null)
                    {
                        string targetHeader = string.Format("{0}{1}{2}{3}{4}{5}{6}", sideColour, target.name, colourEnd, "\n", colourDefault, target.description, colourEnd);
                        //button target details
                        EventButtonDetails targetDetails = new EventButtonDetails()
                        {
                            buttonTitle = "Target",
                            buttonTooltipHeader = targetHeader,
                            buttonTooltipMain = GameManager.instance.targetScript.GetTargetFactors(node.TargetID),
                            buttonTooltipDetail = GameManager.instance.targetScript.GetTargetGoodEffects(node.TargetID),
                            //use a Lambda to pass arguments to the action
                            action = () => { EventManager.instance.PostNotification(EventType.TargetAction, this, nodeID); }
                        };
                        tempList.Add(targetDetails);
                    }
                    else { Debug.LogError(string.Format("Invalid TargetID \"{0}\" (Null){1}", node.TargetID, "\n")); }
                }
                //
                // - - - Actors - - - 
                //
                //loop actors currently in game -> get Node actions (1 per Actor, if valid criteria)
                arrayOfActors = GameManager.instance.dataScript.GetActors(Side.Resistance);
                foreach (Actor actor in arrayOfActors)
                {
                    proceedFlag = true;
                    details = null;

                    //actualRenownEffect = null;

                    //correct side?
                    if (actor.ActorSide == side)
                    {
                        //actor active?
                        if (actor.isLive == true)
                        {
                            //active node for actor or player at node
                            if (GameManager.instance.levelScript.CheckNodeActive(node.NodeID, GameManager.instance.optionScript.PlayerSide, actor.SlotID) == true ||
                                nodeID == playerID)
                            {
                                //get node action
                                tempAction = actor.Arc.nodeAction;

                                if (tempAction != null)
                                {
                                    //effects
                                    StringBuilder builder = new StringBuilder();
                                    listOfEffects = tempAction.listOfEffects;
                                    if (listOfEffects.Count > 0)
                                    {
                                        for (int i = 0; i < listOfEffects.Count; i++)
                                        {
                                            Effect effect = listOfEffects[i];
                                            //check effect criteria is valid
                                            effectCriteria = GameManager.instance.effectScript.CheckEffectCriteria(effect, nodeID);
                                            if (effectCriteria == null)
                                            {
                                                //Effect criteria O.K -> tool tip text
                                                if (builder.Length > 0) { builder.AppendLine(); }
                                                if (effect.effectOutcome != EffectOutcome.Renown)
                                                { builder.Append(string.Format("{0}{1}{2}", colourEffect, effect.description, colourEnd)); }
                                                else
                                                {
                                                    //handle renown situation - players or actors?
                                                    if (nodeID == playerID)
                                                    {
                                                        //actualRenownEffect = playerRenownEffect;
                                                        builder.Append(string.Format("{0}Player {1}{2}", colourBlue, effect.description, colourEnd));
                                                    }
                                                    else
                                                    {
                                                        //actualRenownEffect = actorRenownEffect;
                                                        builder.Append(string.Format("{0}{1} {2}{3}", colourRed, actor.Arc.name, effect.description, colourEnd));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //invalid effect criteria -> Action cancelled
                                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                                infoBuilder.Append(string.Format("{0}{1} action invalid{2}{3}{4}({5}){6}",
                                                    colourInvalid, actor.Arc.name, "\n", colourEnd,
                                                    colourRed, effectCriteria, colourEnd));
                                                proceedFlag = false;
                                            }
                                        }
                                    }
                                    else
                                    { Debug.LogWarning(string.Format("Action \"{0}\" has no effects", tempAction)); }
                                    if (proceedFlag == true)
                                    {
                                        //Details to pass on for processing via button click
                                        ModalActionDetails actionDetails = new ModalActionDetails() { };
                                        actionDetails.side = Side.Resistance;
                                        actionDetails.NodeID = nodeID;
                                        actionDetails.ActorSlotID = actor.SlotID;
                                        //pass all relevant details to ModalActionMenu via Node.OnClick()
                                        details = new EventButtonDetails()
                                        {
                                            buttonTitle = tempAction.name,
                                            buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.Arc.name, colourEnd),
                                            buttonTooltipMain = tempAction.tooltipText,
                                            buttonTooltipDetail = builder.ToString(),
                                            //use a Lambda to pass arguments to the action
                                            action = () => { EventManager.instance.PostNotification(EventType.NodeAction, this, actionDetails); }
                                        };
                                    }
                                }
                            }
                            else
                            {
                                //actor not live at node
                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                infoBuilder.Append(string.Format("{0} has no connections", actor.Arc.name));
                            }
                        }
                        else
                        {
                            //actor gone silent
                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                            infoBuilder.Append(string.Format("{0} is lying low and unavailale", actor.Arc.name));
                        }

                        //add to list
                        if (details != null)
                        { tempList.Add(details); }
                    }
                }
            }
            //
            // - - - Authority - - -
            //
            else if (side == Side.Authority)
            {
                int teamID;
                bool isAnyTeam;
                string tooltipMain;
                //'Cancel' button tooltip)
                int numOfTeams = node.CheckNumOfTeams();
                cancelText = string.Format("There {0} {1} team{2} present at the node", numOfTeams == 1 ? "is" : "are", numOfTeams,
                    numOfTeams != 1 ? "s" : "");
                //
                // - - -  Recall Team - - -
                //
                //Does the Node have any teams present? -> added first
                if (node.CheckNumOfTeams() > 0)
                {
                    //get list of teams
                    List<Team> listOfTeams = node.GetTeams();
                    if (listOfTeams != null)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach(Team team in listOfTeams)
                        {
                            if (builder.Length > 0) { builder.AppendLine(); }
                            builder.Append(string.Format("{0}{1} {2}{3}", colourEffect, team.Arc.name.ToUpper(), team.Name, colourEnd));
                        }
                        //button target details
                        EventButtonDetails targetDetails = new EventButtonDetails()
                        {
                            buttonTitle = "Recall Team",
                            buttonTooltipHeader = string.Format("{0}Recall Team{1}", sideColour, colourEnd),
                            buttonTooltipMain = "The following teams can be withdrawn early",
                            buttonTooltipDetail = builder.ToString(),
                            //use a Lambda to pass arguments to the action
                            action = () => { EventManager.instance.PostNotification(EventType.RecallTeamAction, this, node.NodeID); }
                        };
                        tempList.Add(targetDetails);
                    }
                    else { Debug.LogError(string.Format("Invalid listOfTeams (Null) for Node {0} \"{1}\", ID {2}", node.arc.name, node.NodeName, node.NodeID)); }
                }

                //get a list pre-emptively as it's computationally expensive to do so on demand
                List<string> tempTeamList = GameManager.instance.dataScript.GetAvailableReserveTeams(node);
                arrayOfActors = GameManager.instance.dataScript.GetActors(Side.Authority);
                //loop actors currently in game -> get Node actions (1 per Actor, if valid criteria)
                foreach (Actor actor in arrayOfActors)
                {
                    proceedFlag = true;
                    details = null;
                    isAnyTeam = false;
                    //correct side?
                    if (actor.ActorSide == side)
                    {
                        //actor active?
                        if (actor.isLive == true)
                        {
                            //assign preferred team as default (doesn't matter if actor has ANY Team action)
                            teamID = actor.Arc.preferredTeam.TeamArcID;
                            tempAction = null;
                            //active node for actor
                            if (GameManager.instance.levelScript.CheckNodeActive(node.NodeID, GameManager.instance.optionScript.PlayerSide, actor.SlotID) == true)
                            {
                                //get ANY TEAM node action
                                actionID = GameManager.instance.dataScript.GetActionID("Any Team");
                                if (actionID > -1)
                                {
                                    tempAction = GameManager.instance.dataScript.GetAction(actionID);
                                    isAnyTeam = true;
                                }
                            }
                            //actor not live at node -> Preferred team
                            else
                            { tempAction = actor.Arc.nodeAction; }
                            //default main tooltip text body
                            tooltipMain = tempAction.tooltipText;
                            //tweak if ANY TEAM
                            if (isAnyTeam == true)
                            { tooltipMain = string.Format("{0} as the {1} has Influence here", tooltipMain, (AuthorityActor)GameManager.instance.GetMetaLevel()); }
                            //valid action?
                            if (tempAction != null)
                            {
                                //effects
                                StringBuilder builder = new StringBuilder();
                                listOfEffects = tempAction.listOfEffects;
                                if (listOfEffects.Count > 0)
                                {
                                    for (int i = 0; i < listOfEffects.Count; i++)
                                    {
                                        Effect effect = listOfEffects[i];
                                        //check effect criteria is valid
                                        effectCriteria = GameManager.instance.effectScript.CheckEffectCriteria(effect, nodeID, actor.SlotID, teamID);
                                        if (effectCriteria == null)
                                        {
                                            //Effect criteria O.K -> tool tip text
                                            if (builder.Length > 0) { builder.AppendLine(); }
                                            if (effect.effectOutcome != EffectOutcome.Renown)
                                            {
                                                builder.Append(string.Format("{0}{1}{2}", colourEffect, effect.description, colourEnd));
                                                //if an ANY TEAM action then display available teams
                                                if (isAnyTeam == true)
                                                {
                                                    foreach (string teamName in tempTeamList)
                                                    {
                                                        builder.AppendLine();
                                                        builder.Append(string.Format("{0}{1}{2}", colourEffect, teamName, colourEnd));
                                                    }
                                                }
                                            }
                                            //actor automatically accumulates renown for their faction
                                            else
                                            { builder.Append(string.Format("{0}{1} {2}{3}", colourRed, actor.Arc.name, effect.description, colourEnd)); }

                                        }
                                        else
                                        {
                                            //invalid effect criteria -> Action cancelled
                                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                            infoBuilder.Append(string.Format("{0}{1} action invalid{2}{3}{4}({5}){6}",
                                                colourInvalid, actor.Arc.name, "\n", colourEnd,
                                                colourRed, effectCriteria, colourEnd));
                                            proceedFlag = false;
                                        }
                                    }
                                }
                                else
                                { Debug.LogWarning(string.Format("Action \"{0}\" has no effects", tempAction)); }
                                if (proceedFlag == true)
                                {
                                    //Details to pass on for processing via button click
                                    ModalActionDetails actionDetails = new ModalActionDetails() { };
                                    actionDetails.side = Side.Authority;
                                    actionDetails.NodeID = nodeID;
                                    actionDetails.ActorSlotID = actor.SlotID;
                                    //Node action is standard but other actions are possible
                                    UnityAction clickAction = null;
                                    //Team action
                                    if (isAnyTeam)
                                    { clickAction = () => { EventManager.instance.PostNotification(EventType.InsertTeamAction, this, actionDetails); };  }
                                    //Node action
                                    else
                                    { clickAction = () => { EventManager.instance.PostNotification(EventType.NodeAction, this, actionDetails); };  }
                                    //pass all relevant details to ModalActionMenu via Node.OnClick()
                                    details = new EventButtonDetails()
                                    {
                                        buttonTitle = tempAction.name,
                                        buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.Arc.name, colourEnd),
                                        buttonTooltipMain = tooltipMain,
                                        buttonTooltipDetail = builder.ToString(),
                                        //use a Lambda to pass arguments to the action
                                        action = clickAction
                                    };
                                }
                            }
                            else
                            {
                                Debug.LogError(string.Format("{0}, slotID {1} has no valid action", actor.Arc.name, actor.SlotID));
                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                infoBuilder.Append(string.Format("{0} is having a bad day", actor.Arc.name));
                            }
                        }
                        else
                        {
                            //actor gone silent
                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                            infoBuilder.Append(string.Format("{0} is lying low and unavailale", actor.Arc.name));
                        }
                        //add to list
                        if (details != null)
                        { tempList.Add(details); }
                    }
                }
            }
            //
            // - - - Cancel
            //
            //Cancel button is added last
            EventButtonDetails cancelDetails = null;
            if (infoBuilder.Length > 0)
            {
                cancelDetails = new EventButtonDetails()
                {
                    buttonTitle = "CANCEL",
                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                    buttonTooltipMain = cancelText,
                    buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, infoBuilder.ToString(), colourEnd),
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this); }
                };
            }
            else
            {
                //necessary to prevent color tags triggering the bottom divider in TooltipGeneric
                cancelDetails = new EventButtonDetails()
                {
                    buttonTitle = "CANCEL",
                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                    buttonTooltipMain = cancelText,
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this); }
                };
            }
            //add Cancel button to list
            tempList.Add(cancelDetails);
        }
        else { Debug.LogError(string.Format("Invalid Node (null), ID {0}{1}", nodeID, "\n")); }
        return tempList;
    }

}
