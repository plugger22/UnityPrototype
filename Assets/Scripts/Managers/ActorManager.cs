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

    [HideInInspector] public int numOfActiveActors;    //NOTE -> Not hooked up yet (need to show blank actors for any that aren't currently in use)
    
    [Range(1, 4)] public int numOfOnMapActors = 4;      //if you increase this then GUI elements and GUIManager will need to be changed to accomodate it, default value 4
                                                        //is the total for one side (duplicated by the other side)
    [Range(1, 6)] public int numOfReserveActors = 4;    //total number of actors that can be in the replacement pool (both sides)

    public int numOfQualities = 3;        //number of qualities actors have (different for each side), eg. "Connections, Invisibility" etc. Map to DataPoint0 -> DataPoint'x'

    private static int actorIDCounter = 0;              //used to sequentially number actorID's

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
        numOfOnMapActors = numOfOnMapActors == 4 ? numOfOnMapActors : 4;
        numOfActiveActors = numOfActiveActors < 1 ? 1 : numOfActiveActors;
        numOfActiveActors = numOfActiveActors > numOfOnMapActors ? numOfOnMapActors : numOfActiveActors;
    }


    public void Initialise()
    {
        //event listener is registered in InitialiseActors() due to GameManager sequence.
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
        EventManager.instance.AddListener(EventType.RecruitAction, OnEvent);
        EventManager.instance.AddListener(EventType.GenericRecruitActor, OnEvent);
        //create active, OnMap actors
        InitialiseActors(numOfOnMapActors, Side.Resistance);
        InitialiseActors(numOfOnMapActors, Side.Authority);
        //create pool actors
        InitialisePoolActors();

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
            case EventType.RecruitAction:
                ModalActionDetails details = Param as ModalActionDetails;
                InitialiseGenericPickerRecruit(details);
                break;
            case EventType.GenericRecruitActor:
                GenericReturnData returnDataRecruit = Param as GenericReturnData;
                ProcessRecruitChoice(returnDataRecruit);
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
                Actor actor = CreateActor(side, tempActorArcs[i].ActorArcID, 1, ActorStatus.Active, i);
                if (actor != null)
                {
                    Debug.Log(string.Format("Actor added -> {0}, {1} {2}, {3} {4}, {5} {6}, level {7}{8}", actor.arc.actorName,
                        GameManager.instance.dataScript.GetQuality(side, 0), actor.datapoint0, 
                        GameManager.instance.dataScript.GetQuality(side, 1), actor.datapoint1, 
                        GameManager.instance.dataScript.GetQuality(side, 2), actor.datapoint2, actor.level, "\n"));
                }
                else { Debug.LogWarning("Actor not created"); }
            }
        }
        else { Debug.LogWarning("Invalid number of Actors (Zero, or less)"); }
    }

    /// <summary>
    /// populate the pools to recruit from (one full set of actor arcs for each side and each level)
    /// </summary>
    public void InitialisePoolActors()
    {
        int numOfArcs;
        //Authority
        List<ActorArc> listOfArcs = GameManager.instance.dataScript.GetActorArcs(Side.Authority);
        if (listOfArcs != null)
        {
            numOfArcs = listOfArcs.Count;
            for (int i = 0; i < numOfArcs; i++)
            {
                //level one actor
                Actor actorOne = CreateActor(Side.Authority, listOfArcs[i].ActorArcID, 1, ActorStatus.Pool);
                if (actorOne != null)
                {
                    //NOTE: need to add to Dictionary BEFORE adding to Pool (Debug.Assert checks dictOfActors.Count in AddActorToPool)
                    GameManager.instance.dataScript.AddActorToDict(actorOne);
                    GameManager.instance.dataScript.AddActorToPool(actorOne.actorID, 1, Side.Authority);
                }
                else { Debug.LogWarning(string.Format("Invalid Authority actorOne (Null) for actorArcID \"{0}\"", listOfArcs[i].ActorArcID)); }
                //level two actor
                Actor actorTwo = CreateActor(Side.Authority, listOfArcs[i].ActorArcID, 2, ActorStatus.Pool);
                if (actorTwo != null)
                {
                    GameManager.instance.dataScript.AddActorToDict(actorTwo);
                    GameManager.instance.dataScript.AddActorToPool(actorTwo.actorID, 2, Side.Authority);
                }
                else { Debug.LogWarning(string.Format("Invalid Authority actorTwo (Null) for actorArcID \"{0}\"", listOfArcs[i].ActorArcID)); }
                //level three actor
                Actor actorThree = CreateActor(Side.Authority, listOfArcs[i].ActorArcID, 3, ActorStatus.Pool);
                if (actorThree != null)
                {
                    GameManager.instance.dataScript.AddActorToDict(actorThree);
                    GameManager.instance.dataScript.AddActorToPool(actorThree.actorID, 3, Side.Authority);
                }
                else { Debug.LogWarning(string.Format("Invalid Authority actorThree (Null) for actorArcID \"{0}\"", listOfArcs[i].ActorArcID)); }
            }
        }
        else { Debug.LogError("Invalid list of Authority Actor Arcs (Null)"); }
        //Resistance
        listOfArcs = GameManager.instance.dataScript.GetActorArcs(Side.Resistance);
        if (listOfArcs != null)
        {
            numOfArcs = listOfArcs.Count;
            for (int i = 0; i < numOfArcs; i++)
            {
                //level one actor
                Actor actorOne = CreateActor(Side.Resistance, listOfArcs[i].ActorArcID, 1, ActorStatus.Pool);
                if (actorOne != null)
                {
                    //NOTE: need to add to Dictionary BEFORE adding to Pool (Debug.Assert checks dictOfActors.Count in AddActorToPool)
                    GameManager.instance.dataScript.AddActorToDict(actorOne);
                    GameManager.instance.dataScript.AddActorToPool(actorOne.actorID, 1, Side.Resistance);
                }
                else { Debug.LogWarning(string.Format("Invalid Resistance actorOne (Null) for actorArcID \"{0}\"", listOfArcs[i].ActorArcID)); }
                //level two actor
                Actor actorTwo = CreateActor(Side.Resistance, listOfArcs[i].ActorArcID, 2, ActorStatus.Pool);
                if (actorTwo != null)
                {
                    GameManager.instance.dataScript.AddActorToDict(actorTwo);
                    GameManager.instance.dataScript.AddActorToPool(actorTwo.actorID, 2, Side.Resistance);
                }
                else { Debug.LogWarning(string.Format("Invalid Resistance actorTwo (Null) for actorArcID \"{0}\"", listOfArcs[i].ActorArcID)); }
                //level three actor
                Actor actorThree = CreateActor(Side.Resistance, listOfArcs[i].ActorArcID, 3, ActorStatus.Pool);
                if (actorThree != null)
                {
                    GameManager.instance.dataScript.AddActorToDict(actorThree);
                    GameManager.instance.dataScript.AddActorToPool(actorThree.actorID, 3, Side.Resistance);
                }
                else { Debug.LogWarning(string.Format("Invalid Resistance actorThree (Null) for actorArcID \"{0}\"", listOfArcs[i].ActorArcID)); }
            }
        }
        else { Debug.LogError("Invalid list of Resistance Actor Arcs (Null)"); }
    }

    /// <summary>
    /// creates a new actor of the specified actor arc type, side and level (1 worst -> 3 best). SlotID (0 to 3) for current actor, default '-1' for reserve pool actor.
    /// adds actor to dataManager.cs ArrayOfActors if OnMap (slot ID > -1). NOT added to dictOfActors if slotID -1 (do so after actor is chosen in generic picker)
    /// returns null if a problem
    /// </summary>
    /// <param name="arc"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public Actor CreateActor(Side side, int actorArcID, int level, ActorStatus status, int slotID = -1)
    {
        Debug.Assert(level > 0 && level < 4, "Invalid level (must be between 1 and 3)");
        Debug.Assert(slotID >= -1 && slotID <= numOfOnMapActors, "Invalid slotID (must be -1 (default) or between 1 and 3");
        //check a valid actor Arc parameter
        ActorArc arc = GameManager.instance.dataScript.GetActorArc(actorArcID);
        if (arc != null)
        {
            //check actor arc is the correct side
            if (arc.side == side)
            {
                //create new actor
                Actor actor = new Actor();
                //data
                actor.actorID = actorIDCounter++;
                actor.slotID = slotID;
                actor.level = level;
                actor.actorSide = side;
                actor.arc = arc;
                actor.actorName = arc.actorName;
                actor.trait = GameManager.instance.dataScript.GetRandomTrait();
                actor.status = status;
                //level -> range limits
                int limitLower = 1;
                if (level == 3) { limitLower = 2; }
                int limitUpper = Math.Min(4, level + 2);
                //level -> assign
                actor.datapoint0 = Random.Range(limitLower, limitUpper); //connections and influence
                actor.datapoint1 = Random.Range(limitLower, limitUpper); //motivation and support
                if (side == Side.Resistance)
                { actor.datapoint2 = 0; /*invisibility (always starts at zero*/}
                else if (side == Side.Authority)
                { actor.datapoint2 = Random.Range(limitLower, limitUpper); /*Ability*/}
                //OnMap actor
                if (slotID > -1)
                {
                    actor.isLive = true;
                    //add to data collections
                    GameManager.instance.dataScript.AddCurrentActor(side, actor, slotID);
                    GameManager.instance.dataScript.AddActorToDict(actor);
                }
                //Reserve pool actor or perhaps and actor for a generic pick list
                else
                { actor.isLive = false; }
                //return actor
                return actor;
            }
            else { Debug.LogError(string.Format("ActorArc (\"{0}\") is the wrong side for actorArcID {1}", arc.name, actorArcID)); }
        }
        else { Debug.LogError(string.Format("Invalid ActorArc (Null) for actorArcID {0}", actorArcID)); }
        return null;
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
                if (node.targetID >= 0)
                {
                    Target target = GameManager.instance.dataScript.GetTarget(node.targetID);
                    if (target != null)
                    {
                        string targetHeader = string.Format("{0}{1}{2}{3}{4}{5}{6}", sideColour, target.name, colourEnd, "\n", colourDefault, target.description, colourEnd);
                        //button target details
                        EventButtonDetails targetDetails = new EventButtonDetails()
                        {
                            buttonTitle = "Target",
                            buttonTooltipHeader = targetHeader,
                            buttonTooltipMain = GameManager.instance.targetScript.GetTargetFactors(node.targetID),
                            buttonTooltipDetail = GameManager.instance.targetScript.GetTargetGoodEffects(node.targetID),
                            //use a Lambda to pass arguments to the action
                            action = () => { EventManager.instance.PostNotification(EventType.TargetAction, this, nodeID); }
                        };
                        tempList.Add(targetDetails);
                    }
                    else { Debug.LogError(string.Format("Invalid TargetID \"{0}\" (Null){1}", node.targetID, "\n")); }
                }
                //
                // - - - Actors - - - 
                //
                //loop actors currently in game -> get Node actions (1 per Actor, if valid criteria)
                arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(Side.Resistance);
                foreach (Actor actor in arrayOfActors)
                {
                    proceedFlag = true;
                    details = null;

                    //actualRenownEffect = null;

                    //correct side?
                    if (actor.actorSide == side)
                    {
                        //actor active?
                        if (actor.isLive == true)
                        {
                            //active node for actor or player at node
                            if (GameManager.instance.levelScript.CheckNodeActive(node.NodeID, GameManager.instance.optionScript.PlayerSide, actor.slotID) == true ||
                                nodeID == playerID)
                            {
                                //get node action
                                tempAction = actor.arc.nodeAction;

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
                                                        builder.Append(string.Format("{0}{1} {2}{3}", colourRed, actor.arc.name, effect.description, colourEnd));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //invalid effect criteria -> Action cancelled
                                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                                infoBuilder.Append(string.Format("{0}{1} action invalid{2}{3}{4}({5}){6}",
                                                    colourInvalid, actor.arc.name, "\n", colourEnd,
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
                                        actionDetails.ActorSlotID = actor.slotID;
                                        //pass all relevant details to ModalActionMenu via Node.OnClick()

                                        switch (actor.arc.nodeAction.type)
                                        {
                                            case ActionType.Node:
                                            case ActionType.None:
                                                details = new EventButtonDetails()
                                                {
                                                    buttonTitle = tempAction.name,
                                                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                                    buttonTooltipMain = tempAction.tooltipText,
                                                    buttonTooltipDetail = builder.ToString(),
                                                    //use a Lambda to pass arguments to the action
                                                    action = () => { EventManager.instance.PostNotification(EventType.NodeAction, this, actionDetails); }
                                                };
                                                break;
                                            case ActionType.NeutraliseTeam:
                                                details = new EventButtonDetails()
                                                {
                                                    buttonTitle = tempAction.name,
                                                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                                    buttonTooltipMain = tempAction.tooltipText,
                                                    buttonTooltipDetail = builder.ToString(),
                                                    action = () => { EventManager.instance.PostNotification(EventType.NeutraliseTeamAction, this, actionDetails); }
                                                };
                                                break;
                                            case ActionType.Gear:
                                                details = new EventButtonDetails()
                                                {
                                                    buttonTitle = tempAction.name,
                                                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                                    buttonTooltipMain = tempAction.tooltipText,
                                                    buttonTooltipDetail = builder.ToString(),
                                                    action = () => { EventManager.instance.PostNotification(EventType.GearAction, this, actionDetails); }
                                                };
                                                break;
                                            case ActionType.Recruit:
                                                details = new EventButtonDetails()
                                                {
                                                    buttonTitle = tempAction.name,
                                                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                                    buttonTooltipMain = tempAction.tooltipText,
                                                    buttonTooltipDetail = builder.ToString(),
                                                    action = () => { EventManager.instance.PostNotification(EventType.RecruitAction, this, actionDetails); }
                                                };
                                                break;
                                            default:
                                                Debug.LogError(string.Format("Invalid actor.Arc.nodeAction.type \"{0}\"", actor.arc.nodeAction.type));
                                                break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //actor not live at node
                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                infoBuilder.Append(string.Format("{0} has no connections", actor.arc.name));
                            }
                        }
                        else
                        {
                            //actor gone silent
                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                            infoBuilder.Append(string.Format("{0} is lying low and unavailale", actor.arc.name));
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
                        //button details
                        EventButtonDetails recallDetails = new EventButtonDetails()
                        {
                            buttonTitle = "Recall Team",
                            buttonTooltipHeader = string.Format("{0}Recall Team{1}", sideColour, colourEnd),
                            buttonTooltipMain = "The following teams can be withdrawn early",
                            buttonTooltipDetail = builder.ToString(),
                            //use a Lambda to pass arguments to the action
                            action = () => { EventManager.instance.PostNotification(EventType.RecallTeamAction, this, node.NodeID); }
                        };
                        tempList.Add(recallDetails);
                    }
                    else { Debug.LogError(string.Format("Invalid listOfTeams (Null) for Node {0} \"{1}\", ID {2}", node.arc.name, node.NodeName, node.NodeID)); }
                }

                //get a list pre-emptively as it's computationally expensive to do so on demand
                List<string> tempTeamList = GameManager.instance.dataScript.GetAvailableReserveTeams(node);
                arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(Side.Authority);
                //loop actors currently in game -> get Node actions (1 per Actor, if valid criteria)
                foreach (Actor actor in arrayOfActors)
                {
                    proceedFlag = true;
                    details = null;
                    isAnyTeam = false;
                    //correct side?
                    if (actor.actorSide == side)
                    {
                        //actor active?
                        if (actor.isLive == true)
                        {
                            //assign preferred team as default (doesn't matter if actor has ANY Team action)
                            teamID = actor.arc.preferredTeam.TeamArcID;
                            tempAction = null;
                            //active node for actor
                            if (GameManager.instance.levelScript.CheckNodeActive(node.NodeID, GameManager.instance.optionScript.PlayerSide, actor.slotID) == true)
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
                            { tempAction = actor.arc.nodeAction; }
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
                                        effectCriteria = GameManager.instance.effectScript.CheckEffectCriteria(effect, nodeID, actor.slotID, teamID);
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
                                            { builder.Append(string.Format("{0}{1} {2}{3}", colourRed, actor.arc.name, effect.description, colourEnd)); }

                                        }
                                        else
                                        {
                                            //invalid effect criteria -> Action cancelled
                                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                            infoBuilder.Append(string.Format("{0}{1} action invalid{2}{3}{4}({5}){6}",
                                                colourInvalid, actor.arc.name, "\n", colourEnd,
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
                                    actionDetails.ActorSlotID = actor.slotID;
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
                                        buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                        buttonTooltipMain = tooltipMain,
                                        buttonTooltipDetail = builder.ToString(),
                                        //use a Lambda to pass arguments to the action
                                        action = clickAction
                                    };
                                }
                            }
                            else
                            {
                                Debug.LogError(string.Format("{0}, slotID {1} has no valid action", actor.arc.name, actor.slotID));
                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                infoBuilder.Append(string.Format("{0} is having a bad day", actor.arc.name));
                            }
                        }
                        else
                        {
                            //actor gone silent
                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                            infoBuilder.Append(string.Format("{0} is lying low and unavailale", actor.arc.name));
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

    /// <summary>
    /// Choose Recruit (Both sides): sets up ModalGenericPicker class and triggers event: ModalGenericEvent.cs -> SetGenericPicker()
    /// </summary>
    /// <param name="details"></param>
    private void InitialiseGenericPickerRecruit(ModalActionDetails details)
    {
        /*
        bool errorFlag = false;
        int recruitID, index;
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        Node node = GameManager.instance.dataScript.GetNode(details.NodeID);
        if (node != null)
        {
            genericDetails.returnEvent = EventType.GenericGearChoice;
            genericDetails.side = Side.Resistance;
            genericDetails.nodeID = details.NodeID;
            genericDetails.actorSlotID = details.ActorSlotID;
            //picker text
            genericDetails.textTop = string.Format("{0}Gear{1} {2}available{3}", colourEffect, colourEnd, colourNormal, colourEnd);
            genericDetails.textMiddle = string.Format("{0}Gear will be placed in your inventory{1}",
                colourNormal, colourEnd);
            genericDetails.textBottom = "Click on an item to Select. Press CONFIRM to obtain gear. Mouseover gear for more information.";
            //
            //generate temp list of gear to choose from
            //
            List<int> tempCommonGear = new List<int>(GameManager.instance.dataScript.GetListOfGear(GearLevel.Common));
            List<int> tempRareGear = new List<int>(GameManager.instance.dataScript.GetListOfGear(GearLevel.Rare));
            //remove from lists any gear that the player currently has
            List<int> tempPlayerGear = new List<int>(GameManager.instance.playerScript.GetListOfGear());
            if (tempPlayerGear.Count > 0)
            {

                for (int i = 0; i < tempPlayerGear.Count; i++)
                {
                    gearID = tempPlayerGear[i];
                    if (tempCommonGear.Exists(id => id == gearID) == true)
                    { tempCommonGear.Remove(gearID); }
                    else if (tempRareGear.Exists(id => id == gearID) == true)
                    { tempRareGear.Remove(gearID); }
                }
            }
            //
            //select two items of gear for the picker
            //
            int[] arrayOfGear = new int[2];
            int countOfGear = 0;
            for (int i = 0; i < arrayOfGear.Length; i++)
            {
                gearID = -1;
                //any rare gear available?
                if (tempRareGear.Count > 0)
                {
                    //chance of rare gear -> base chance * actor ability (or 1 if player)
                    int chance = chanceOfRareGear;
                    Actor actor = GameManager.instance.dataScript.GetActor(details.ActorSlotID, Side.Resistance);
                    if (actor != null)
                    {
                        //if Player doing it then assumed to have an ability of 1, actor (Fixer) may have a higher ability.
                        if (node.NodeID != GameManager.instance.nodeScript.nodePlayer)
                        { chance *= actor.datapoint2; }
                    }
                    else
                    {
                        chance = 0;
                        Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", details.ActorSlotID));
                    }
                    if (Random.Range(0, 100) < chance)
                    {
                        index = Random.Range(0, tempRareGear.Count);
                        gearID = tempRareGear[index];
                        tempRareGear.RemoveAt(index);
                    }
                    //if failed chance for rare gear then need to get common
                    else if (tempCommonGear.Count > 0)
                    {
                        index = Random.Range(0, tempCommonGear.Count);
                        gearID = tempCommonGear[index];
                        tempCommonGear.RemoveAt(index);
                    }
                }
                //common gear
                else
                {
                    if (tempCommonGear.Count > 0)
                    {
                        index = Random.Range(0, tempCommonGear.Count);
                        gearID = tempCommonGear[index];
                        tempCommonGear.RemoveAt(index);
                    }
                }
                //found some gear?
                if (gearID > -1)
                { arrayOfGear[i] = gearID; countOfGear++; }
            }
            //check there is at least one item of gear available
            if (countOfGear < 1)
            {
                //OUTCOME -> No gear available
                Debug.LogWarning("GearManager: No gear available in InitaliseGenericPickerGear");
                errorFlag = true;
            }
            else
            {
                //
                //loop gearID's that have been selected and package up ready for ModalGenericPicker
                //
                for (int i = 0; i < countOfGear; i++)
                {
                    Gear gear = GameManager.instance.dataScript.GetGear(arrayOfGear[i]);
                    if (gear != null)
                    {
                        //option details
                        GenericOptionDetails optionDetails = new GenericOptionDetails();
                        optionDetails.optionID = gear.gearID;
                        optionDetails.text = gear.name.ToUpper();
                        optionDetails.sprite = gear.sprite;
                        //tooltip -> TO DO
                        GenericTooltipDetails tooltipDetails = new GenericTooltipDetails();
                        tooltipDetails.textHeader = string.Format("{0}{1}{2}", colourGear, gear.name.ToUpper(), colourEnd);
                        tooltipDetails.textMain = string.Format("{0}{1}{2}", colourNormal, gear.description, colourEnd);
                        tooltipDetails.textDetails = string.Format("{0}{1}{2}{3}{4}{5} gear{6}", colourEffect, gear.rarity, colourEnd,
                            "\n", colourSide, gear.type, colourEnd);
                        //add to master arrays
                        genericDetails.arrayOfOptions[i] = optionDetails;
                        genericDetails.arrayOfTooltips[i] = tooltipDetails;
                    }
                    else { Debug.LogError(string.Format("Invalid gear (Null) for gearID {0}", arrayOfGear[i])); }
                }
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
            outcomeDetails.textTop = "There has been an error in communication and no gear can be sourced.";
            outcomeDetails.textBottom = "Heads will roll!";
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
        }
        else
        {
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails);
        }
        */
    }

    /// <summary>
    /// Processes choice of Recruit
    /// </summary>
    /// <param name="returnDetails"></param>
    private void ProcessRecruitChoice(GenericReturnData data)
    {

    }

    //new methods above here
}
