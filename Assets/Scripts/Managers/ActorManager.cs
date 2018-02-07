using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using modalAPI;
using packageAPI;
using System;
using Random = UnityEngine.Random;
using System.Text;
using UnityEngine.Events;

/// <summary>
/// handles Actor related data and methods
/// </summary>
public class ActorManager : MonoBehaviour
{

    [HideInInspector] public int numOfActiveActors;    //Actors who are OnMap and active, eg. not asleep or captured
    
    [Range(1, 4)] public int numOfOnMapActors = 4;      //if you increase this then GUI elements and GUIManager will need to be changed to accomodate it, default value 4
                                                        //is the total for one side (duplicated by the other side)
    [Range(1, 6)] public int numOfReserveActors = 4;    //total number of actors that can be in the replacement pool (both sides)

    public int numOfQualities = 3;        //number of qualities actors have (different for each side), eg. "Connections, Invisibility" etc. Map to DataPoint0 -> DataPoint'x'

    private static int actorIDCounter = 0;              //used to sequentially number actorID's

    //colour palette for Generic tool tip
    private string colourResistance;
    private string colourAuthority;
    private string colourCancel;
    private string colourInvalid;
    private string colourGoodEffect;
    private string colourNeutralEffect;
    private string colourBadEffect;
    private string colourDefault;
    private string colourNormal;
    private string colourRecruit;
    private string colourArc;
    private string colourEnd;


    public void PreInitialiseActors()
    {
        //number of actors, default 4
        numOfOnMapActors = numOfOnMapActors == 4 ? numOfOnMapActors : 4;
        numOfActiveActors = numOfOnMapActors;
    }


    public void Initialise()
    {
        //event listener is registered in InitialiseActors() due to GameManager sequence.
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
        EventManager.instance.AddListener(EventType.RecruitAction, OnEvent);
        EventManager.instance.AddListener(EventType.RecruitDecision, OnEvent);
        EventManager.instance.AddListener(EventType.GenericRecruitActorResistance, OnEvent);
        EventManager.instance.AddListener(EventType.GenericRecruitActorAuthority, OnEvent);
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
            case EventType.RecruitDecision:
                RecruitActor((int)Param);
                break;
            case EventType.GenericRecruitActorResistance:
                GenericReturnData returnDataRecruitResistance = Param as GenericReturnData;
                ProcessRecruitChoiceResistance(returnDataRecruitResistance);
                break;
            case EventType.GenericRecruitActorAuthority:
                GenericReturnData returnDataRecruitAuthority = Param as GenericReturnData;
                ProcessRecruitChoiceAuthority(returnDataRecruitAuthority);
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
        colourResistance = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        colourCancel = GameManager.instance.colourScript.GetColour(ColourType.cancelNormal);
        colourInvalid = GameManager.instance.colourScript.GetColour(ColourType.cancelHighlight);
        colourGoodEffect = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourNeutralEffect = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourBadEffect = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourRecruit = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourArc = GameManager.instance.colourScript.GetColour(ColourType.actorArc);
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
                actor.Status = status;
                //level -> range limits
                int limitLower = 1;
                if (level == 3) { limitLower = 2; }
                int limitUpper = Math.Min(4, level + 2);
                //level -> assign
                actor.datapoint0 = Random.Range(limitLower, limitUpper); //connections and influence
                actor.datapoint1 = Random.Range(limitLower, limitUpper); //motivation and support
                if (side == Side.Resistance)
                {
                    //invisibility -> Level 3 100% Invis 3, level 2 25% Invis 2, 75% Invis 3, level 1 50% Invis 2, 50% Invis 3
                    switch(actor.level)
                    {
                        case 3: actor.datapoint2 = 3; break;
                        case 2: 
                            if (Random.Range(0,100) <= 25) { actor.datapoint2 = 2; }
                            else { actor.datapoint2 = 3; }
                            break;
                        case 1:
                            if (Random.Range(0, 100) <= 50) { actor.datapoint2 = 2; }
                            else { actor.datapoint2 = 3; }
                            break;
                    }
                    
                }
                else if (side == Side.Authority)
                { actor.datapoint2 = Random.Range(limitLower, limitUpper); /*Ability*/}
                //OnMap actor
                if (slotID > -1)
                {
                    /*actor.isLive = true;*/

                    //add to data collections
                    GameManager.instance.dataScript.AddCurrentActor(side, actor, slotID);
                    GameManager.instance.dataScript.AddActorToDict(actor);
                }
                
                /*//Reserve pool actor or perhaps an actor for a generic pick list
                else
                { actor.isLive = false; }*/

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
    /// Max 4 Actor + 1 Target actions with an additional 'Cancel' buttnn added last automatically -> total six buttons (hardwired into GUI design)
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
        Side side = GameManager.instance.sideScript.PlayerSide;
        //color code for button tooltip header text, eg. "Operator"ss
        if ( side == Side.Authority)
        { sideColour = colourAuthority; }
        else { sideColour = colourResistance; }
        List<EventButtonDetails> tempList = new List<EventButtonDetails>();
        //string builder for cancel button (handles all no go cases
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
                        bool targetProceed = false;
                        //can tackle target if Player at node or target specified actor is present in line-up
                        if (nodeID == playerID) { targetProceed = true; }
                        else if (GameManager.instance.dataScript.CheckActorArcPresent(target.actorArc, Side.Resistance) == true) { targetProceed = true; }
                        //target live and dancing
                        if (targetProceed == true)
                        {
                            string targetHeader = string.Format("{0}{1}{2}{3}{4}{5}{6}", sideColour, target.name, colourEnd, "\n", colourDefault, target.description, colourEnd);
                            //button target details
                            EventButtonDetails targetDetails = new EventButtonDetails()
                            {
                                buttonTitle = "Attempt Target",
                                buttonTooltipHeader = targetHeader,
                                buttonTooltipMain = GameManager.instance.targetScript.GetTargetFactors(node.targetID),
                                buttonTooltipDetail = GameManager.instance.targetScript.GetTargetEffects(node.targetID),
                                //use a Lambda to pass arguments to the action
                                action = () => { EventManager.instance.PostNotification(EventType.TargetAction, this, nodeID); }
                            };
                            tempList.Add(targetDetails);
                        }
                        else
                        {
                            //invalid target (Player not present, specified actor Arc not in line up)
                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                            infoBuilder.Append(string.Format("{0}Target invalid{1}{2}{3}(No Player, No {4}){5}",
                                colourInvalid, "\n", colourEnd, colourBadEffect, target.actorArc.name, colourEnd));
                        }
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
                    //correct side?
                    if (actor.actorSide == side)
                    {
                        //actor active?
                        if (actor.Status == ActorStatus.Active)
                        {
                            //active node for actor or player at node
                            if (GameManager.instance.levelScript.CheckNodeActive(node.nodeID, GameManager.instance.sideScript.PlayerSide, actor.slotID) == true ||
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
                                        string colourEffect = colourNeutralEffect;
                                        for (int i = 0; i < listOfEffects.Count; i++)
                                        {
                                            Effect effect = listOfEffects[i];
                                            //colour code effects according to type
                                            if (effect.type != null)
                                            {
                                                switch (effect.type.name)
                                                {
                                                    case "Good":
                                                        colourEffect = colourGoodEffect;
                                                        break;
                                                    case "Neutral":
                                                        colourEffect = colourNeutralEffect;
                                                        break;
                                                    case "Bad":
                                                        colourEffect = colourBadEffect;
                                                        break;
                                                }
                                            }
                                            //check effect criteria is valid
                                            effectCriteria = GameManager.instance.effectScript.CheckEffectCriteria(effect, nodeID);
                                            if (effectCriteria == null)
                                            {
                                                //Effect criteria O.K -> tool tip text
                                                if (builder.Length > 0) { builder.AppendLine(); }
                                                if (effect.outcome != EffectOutcome.Renown && effect.outcome != EffectOutcome.Invisibility)
                                                { builder.Append(string.Format("{0}{1}{2}", colourEffect, effect.description, colourEnd)); }
                                                else
                                                {
                                                    //handle renown & invisibility situatiosn - players or actors?
                                                    if (nodeID == playerID)
                                                    {
                                                        //player affected (good for renown, bad for invisibility)
                                                        if (effect.outcome == EffectOutcome.Renown)
                                                        { builder.Append(string.Format("{0}Player {1}{2}", colourGoodEffect, effect.description, colourEnd)); }
                                                        else
                                                        { builder.Append(string.Format("{0}Player {1}{2}", colourBadEffect, effect.description, colourEnd)); }
                                                    }
                                                    else
                                                    {
                                                        //actor affected
                                                        builder.Append(string.Format("{0}{1} {2}{3}", colourBadEffect, actor.arc.name, effect.description, colourEnd));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //invalid effect criteria -> Action cancelled
                                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                                infoBuilder.Append(string.Format("{0}{1} action invalid{2}{3}{4}({5}){6}",
                                                    colourInvalid, actor.arc.name, "\n", colourEnd,
                                                    colourAuthority, effectCriteria, colourEnd));
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
                                //actor has no connections at node
                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                infoBuilder.Append(string.Format("{0} has no connections", actor.arc.name));
                            }
                        }
                        else
                        {
                            //actor isn't Active
                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                            switch(actor.Status)
                            {
                                case ActorStatus.Inactive:
                                    infoBuilder.Append(string.Format("{0} is lying low and unavailable", actor.arc.name));
                                    break;
                                case ActorStatus.Captured:
                                    infoBuilder.Append(string.Format("{0} has been captured", actor.arc.name));
                                    break;
                                default:
                                    infoBuilder.Append(string.Format("{0} is unavailable", actor.arc.name));
                                    break;
                            }
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
                            builder.Append(string.Format("{0}{1} {2}{3}", colourNeutralEffect, team.Arc.name.ToUpper(), team.Name, colourEnd));
                        }
                        //button details
                        EventButtonDetails recallDetails = new EventButtonDetails()
                        {
                            buttonTitle = "Recall Team",
                            buttonTooltipHeader = string.Format("{0}Recall Team{1}", sideColour, colourEnd),
                            buttonTooltipMain = "The following teams can be withdrawn early",
                            buttonTooltipDetail = builder.ToString(),
                            //use a Lambda to pass arguments to the action
                            action = () => { EventManager.instance.PostNotification(EventType.RecallTeamAction, this, node.nodeID); }
                        };
                        tempList.Add(recallDetails);
                    }
                    else { Debug.LogError(string.Format("Invalid listOfTeams (Null) for Node {0} \"{1}\", ID {2}", node.Arc.name, node.nodeName, node.nodeID)); }
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
                        if (actor.Status == ActorStatus.Active)
                        {
                            //assign preferred team as default (doesn't matter if actor has ANY Team action)
                            teamID = actor.arc.preferredTeam.TeamArcID;
                            tempAction = null;
                            //active node for actor
                            if (GameManager.instance.levelScript.CheckNodeActive(node.nodeID, GameManager.instance.sideScript.PlayerSide, actor.slotID) == true)
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
                            { tooltipMain = string.Format("{0} as the {1} has Influence here", tooltipMain, (AuthorityActor)GameManager.instance.turnScript.metaLevel); }
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
                                            if (effect.outcome != EffectOutcome.Renown)
                                            {
                                                builder.Append(string.Format("{0}{1}{2}", colourNeutralEffect, effect.description, colourEnd));
                                                //if an ANY TEAM action then display available teams
                                                if (isAnyTeam == true)
                                                {
                                                    foreach (string teamName in tempTeamList)
                                                    {
                                                        builder.AppendLine();
                                                        builder.Append(string.Format("{0}{1}{2}", colourNeutralEffect, teamName, colourEnd));
                                                    }
                                                }
                                            }
                                            //actor automatically accumulates renown for their faction
                                            else
                                            { builder.Append(string.Format("{0}{1} {2}{3}", colourAuthority, actor.arc.name, effect.description, colourEnd)); }

                                        }
                                        else
                                        {
                                            //invalid effect criteria -> Action cancelled
                                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                            infoBuilder.Append(string.Format("{0}{1} action invalid{2}{3}{4}({5}){6}",
                                                colourInvalid, actor.arc.name, "\n", colourEnd,
                                                colourAuthority, effectCriteria, colourEnd));
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
    /// call this to recruit an actor of a specified level (1 to 3) from a Decision
    /// </summary>
    /// <param name="level"></param>
    public void RecruitActor(int level = 0)
    {
        Debug.Assert(level > 0 && level <= 3, string.Format("Invalid level {0}", level));
        ModalActionDetails details = new ModalActionDetails();
        Side side = GameManager.instance.sideScript.PlayerSide;
        //ignore node and actorSlotID
        details.side = side;
        details.Level = level;
        details.NodeID = -1;
        details.ActorSlotID = -1;
        //get event
        switch (side)
        {
            case Side.Authority:
                details.EventType = EventType.GenericRecruitActorAuthority;
                break;
            case Side.Resistance:
                details.EventType = EventType.GenericRecruitActorResistance;
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", side));
                break;
        }
        InitialiseGenericPickerRecruit(details);
    }

    /// <summary>
    /// Choose Recruit (Both sides): sets up ModalGenericPicker class and triggers event: ModalGenericEvent.cs -> SetGenericPicker()
    /// </summary>
    /// <param name="details"></param>
    private void InitialiseGenericPickerRecruit(ModalActionDetails details)
    {
        bool errorFlag = false;
        int index, countOfRecruits;
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        Node node = null;
        if (details.side == Side.Resistance)
        {
            node = GameManager.instance.dataScript.GetNode(details.NodeID);
            if (node != null)
            {
                //check for player/actor being captured
                int actorID = 999;
                if (node.nodeID != GameManager.instance.nodeScript.nodePlayer)
                {
                    Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.ActorSlotID, Side.Resistance);
                    if (actor != null)
                    { actorID = actor.actorID; }
                    else { Debug.LogError(string.Format("Invalid actor (Null) fro details.ActorSlotID {0}", details.ActorSlotID)); errorFlag = true; }
                }
                //check capture provided no errors
                if (errorFlag == false)
                {
                    AIDetails aiDetails = GameManager.instance.captureScript.CheckCaptured(node.nodeID, actorID);
                    if (aiDetails != null)
                    {
                        //capture happened, abort recruitment
                        aiDetails.effects = string.Format("{0}The Recruiting mission was a wipe{1}", colourNeutralEffect, colourEnd);
                        EventManager.instance.PostNotification(EventType.Capture, this, aiDetails);
                        return;
                    }
                }
                else
                {
                    //reset flag to the default state prior to recruitments
                    errorFlag = false;
                }
            }
        }
        if (details.side == Side.Authority || node != null)
        {
            if (details.side == Side.Resistance) { genericDetails.returnEvent = EventType.GenericRecruitActorResistance; }
            else if (details.side == Side.Authority) { genericDetails.returnEvent = EventType.GenericRecruitActorAuthority; }
            else { Debug.LogError(string.Format("Invalid side \"{0}\"", details.side)); }
            genericDetails.side = details.side;
            if (details.side == Side.Resistance)
            { genericDetails.nodeID = details.NodeID; }
            else { genericDetails.nodeID = -1; }
            genericDetails.actorSlotID = details.ActorSlotID;
            //picker text
            genericDetails.textTop = string.Format("{0}Recruits{1} {2}available{3}", colourNeutralEffect, colourEnd, colourNormal, colourEnd);
            genericDetails.textMiddle = string.Format("{0}Recruit will be assigned to your reserve list{1}",
                colourNormal, colourEnd);
            genericDetails.textBottom = "Click on a Recruit to Select. Press CONFIRM to hire Recruit. Mouseover recruit for more information.";
            //
            //generate temp list of Recruits to choose from and a list of ones for the picker
            //
            int numOfOptions;
            List<int> listOfPoolActors = new List<int>();
            List<int> listOfPickerActors = new List<int>();
            List<int> listOfCurrentArcIDs = new List<int>(GameManager.instance.dataScript.GetAllCurrentActorArcIDs(details.side));
            //selection methodology varies for each side -> need to populate 'listOfPoolActors'
            if (details.side == Side.Resistance)
            {
                if (node.nodeID == GameManager.instance.nodeScript.nodePlayer)
                {
                    //player at node, select from 3 x level 1 options, different from current OnMap actor types
                    listOfPoolActors.AddRange(GameManager.instance.dataScript.GetActorPool(1, details.side));
                    //loop backwards through pool of actors and remove any that match the curent OnMap types
                    for (int i = listOfPoolActors.Count - 1; i >= 0; i--)
                    {
                        Actor actor = GameManager.instance.dataScript.GetActor(listOfPoolActors[i]);
                        if (actor != null)
                        {
                            if (listOfCurrentArcIDs.Exists(x => x == actor.arc.ActorArcID))
                            { listOfPoolActors.RemoveAt(i); }
                        }
                        else { Debug.LogWarning(string.Format("Invalid actor (Null) for actorID {0}", listOfPoolActors[i])); }
                    }
                }
                else
                {
                    //actor at node, select from 3 x level 2 options (random types, could be the same as currently OnMap)
                    listOfPoolActors.AddRange(GameManager.instance.dataScript.GetActorPool(2, details.side));
                }
            }
            //Authority
            else if (details.side == Side.Authority)
            {
                //placeholder -> select from 3 x specified level options (random types, could be the same as currently OnMap)
                listOfPoolActors.AddRange(GameManager.instance.dataScript.GetActorPool(details.Level, details.side));
            }
            else { Debug.LogError(string.Format("Invalid side \"{0}\"", details.side)); }
            //
            //select three actors for the picker
            //
            numOfOptions = Math.Min(3, listOfPoolActors.Count);
            for (int i = 0; i < numOfOptions; i++)
            {
                index = Random.Range(0, listOfPoolActors.Count);
                listOfPickerActors.Add(listOfPoolActors[index]);
                //remove actor from pool to prevent duplicate types
                listOfPoolActors.RemoveAt(index);
            }
            //check there is at least one recruit available
            countOfRecruits = listOfPickerActors.Count;
            if (countOfRecruits < 1)
            {
                //OUTCOME -> No recruits available
                Debug.LogWarning("ActorManager: No recruits available in InitaliseGenericPickerRecruits");
                errorFlag = true;
            }
            else
            {
                //
                //loop actorID's that have been selected and package up ready for ModalGenericPicker
                //
                for (int i = 0; i < countOfRecruits; i++)
                {
                    Actor actor = GameManager.instance.dataScript.GetActor(listOfPickerActors[i]);
                    if (actor != null)
                    {
                        //option details
                        GenericOptionDetails optionDetails = new GenericOptionDetails();
                        optionDetails.optionID = actor.actorID;
                        optionDetails.text = actor.arc.name;
                        optionDetails.sprite = actor.arc.baseSprite;
                        //tooltip
                        GenericTooltipDetails tooltipDetails = new GenericTooltipDetails();
                        //arc type and name
                        tooltipDetails.textHeader = string.Format("{0}{1}{2}{3}{4}{5}{6}", colourRecruit, actor.arc.name, colourEnd,
                            "\n", colourNormal, actor.actorName, colourEnd);
                        //stats
                        string[] arrayOfQualities = GameManager.instance.dataScript.GetQualities(details.side);
                        StringBuilder builder = new StringBuilder();
                        if (arrayOfQualities.Length > 0)
                        {
                            builder.Append(string.Format("{0}  {1}{2}{3}{4}", arrayOfQualities[0], GameManager.instance.colourScript.GetValueColour(actor.datapoint0), 
                                actor.datapoint0, colourEnd,"\n"));
                            builder.Append(string.Format("{0}  {1}{2}{3}{4}", arrayOfQualities[1], GameManager.instance.colourScript.GetValueColour(actor.datapoint1), 
                                actor.datapoint1, colourEnd, "\n"));
                            builder.Append(string.Format("{0}  {1}{2}{3}", arrayOfQualities[2], GameManager.instance.colourScript.GetValueColour(actor.datapoint2), 
                                actor.datapoint2, colourEnd));
                            tooltipDetails.textMain = string.Format("{0}{1}{2}", colourNormal, builder.ToString(), colourEnd);
                        }
                        //trait and action
                        tooltipDetails.textDetails = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}", "<font=\"Bangers SDF\">", 
                            GameManager.instance.colourScript.GetValueColour(3 - (int)actor.trait.type), 
                            "<cspace=0.6em>", actor.trait.name, "</cspace>", colourEnd, "</font>", "\n", colourNormal, actor.arc.nodeAction.name, colourEnd);
                        //add to master arrays
                        genericDetails.arrayOfOptions[i] = optionDetails;
                        genericDetails.arrayOfTooltips[i] = tooltipDetails;
                    }
                    else { Debug.LogError(string.Format("Invalid actor (Null) for gearID {0}", listOfPickerActors[i])); }
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
            outcomeDetails.textTop = "There has been a Snafu in communication and no recruits can be found.";
            outcomeDetails.textBottom = "Heads will roll!";
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
        }
        else
        {
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails);
        }
     
    }

    /// <summary>
    /// Processes choice of Recruit Resistance Actor
    /// </summary>
    /// <param name="returnDetails"></param>
    private void ProcessRecruitChoiceResistance(GenericReturnData data)
    {
        bool successFlag = true;
        StringBuilder builderTop = new StringBuilder();
        StringBuilder builderBottom = new StringBuilder();
        Sprite sprite = GameManager.instance.outcomeScript.errorSprite;
        Side side = GameManager.instance.sideScript.PlayerSide;
        if (data.optionID > -1)
        {
            //find actor
            Actor actorCurrent = GameManager.instance.dataScript.GetCurrentActor(data.actorSlotID, side);
            if (actorCurrent != null)
            {
                Actor actorRecruited = GameManager.instance.dataScript.GetActor(data.optionID);
                if (actorRecruited != null)
                {
                    //add actor to reserve pool
                    if (GameManager.instance.dataScript.AddActorToReserve(actorRecruited.actorID, side) == true)
                    {
                        //change actor's status
                        actorRecruited.Status = ActorStatus.Reserve;
                        //remove actor from appropriate pool list
                        GameManager.instance.dataScript.RemoveActorFromPool(actorRecruited.actorID, actorRecruited.level, side);
                        //sprite of recruited actor
                        sprite = actorRecruited.arc.baseSprite;

                        //actor successfully recruited
                        builderTop.Append(string.Format("{0}The interview went well!{1}", colourNormal, colourEnd));
                        builderBottom.Append(string.Format("{0}{1}{2}, {3}\"{4}\", has been recruited and is available in the Reserve List{5}", colourArc,
                            actorRecruited.arc.name, colourEnd, colourNormal, actorRecruited.actorName, colourEnd));
                        //message
                        string textMsg = string.Format("{0}, {1}, ID {2} has been recruited", actorRecruited.actorName, actorRecruited.arc.name.ToUpper(),
                            actorRecruited.actorID);
                        Message message = GameManager.instance.messageScript.ActorRecruited(textMsg, data.nodeID, actorRecruited.actorID, Side.Resistance);
                        if (message != null) { GameManager.instance.dataScript.AddMessage(message); }
                        //Process any other effects, if acquisition was successfull, ignore otherwise
                        Action action = actorCurrent.arc.nodeAction;
                        List<Effect> listOfEffects = action.GetEffects();
                        if (listOfEffects.Count > 0)
                        {
                            EffectDataInput dataInput = new EffectDataInput();
                            Node node = GameManager.instance.dataScript.GetNode(data.nodeID);
                            if (node != null)
                            {
                                foreach (Effect effect in listOfEffects)
                                {
                                    if (effect.ignoreEffect == false)
                                    {
                                        EffectDataReturn effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput, actorCurrent);
                                        if (effectReturn != null)
                                        {
                                            builderTop.AppendLine();
                                            builderTop.Append(effectReturn.topText);
                                            builderBottom.AppendLine();
                                            builderBottom.AppendLine();
                                            builderBottom.Append(effectReturn.bottomText);
                                            //exit effect loop on error
                                            if (effectReturn.errorFlag == true) { break; }
                                        }
                                        else { Debug.LogError("Invalid effectReturn (Null)"); }
                                    }
                                }
                            }
                            else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", data.nodeID)); }
                        }
                    }
                    else
                    {
                        //some issue prevents actor being added to reserve pool (full? -> probably not as a criteria checks this)
                        successFlag = false;
                    }
                }
                else
                {
                    Debug.LogWarning(string.Format("Invalid Recruited Actor (Null) for actorID {0}", data.optionID));
                    successFlag = false;
                }
            }
            else
            {
                Debug.LogWarning(string.Format("Invalid Current Actor (Null) for actorSlotID {0}", data.actorSlotID));
                successFlag = false;
            }
        }
        else
        { Debug.LogWarning(string.Format("Invalid optionID {0}", data.optionID)); }
        //failed outcome
        if (successFlag == false)
        {
            builderTop.Append("Something has gone wrong. Our Recruit has gone missing");
            builderBottom.Append("This is a matter of concern");
        }
        //
        // - - - Outcome - - - 
        //
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        outcomeDetails.textTop = builderTop.ToString();
        outcomeDetails.textBottom = builderBottom.ToString();
        outcomeDetails.sprite = sprite;
        outcomeDetails.side = side;
        //action expended automatically for recruit actor
        if (successFlag == true)
        { outcomeDetails.isAction = true; }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
    }

    /// <summary>
    /// Processes choice of Recruit Authority Actor
    /// </summary>
    /// <param name="returnDetails"></param>
    private void ProcessRecruitChoiceAuthority(GenericReturnData data)
    {
        bool successFlag = true;
        StringBuilder builderTop = new StringBuilder();
        StringBuilder builderBottom = new StringBuilder();
        Sprite sprite = GameManager.instance.outcomeScript.errorSprite;
        Side side = GameManager.instance.sideScript.PlayerSide;
        if (data.optionID > -1)
        {
            //find actor
            Actor actorRecruited = GameManager.instance.dataScript.GetActor(data.optionID);
            if (actorRecruited != null)
            {
                //add actor to reserve pool
                if (GameManager.instance.dataScript.AddActorToReserve(actorRecruited.actorID, side) == true)
                {
                    //change actor's status
                    actorRecruited.Status = ActorStatus.Reserve;
                    //remove actor from appropriate pool list
                    GameManager.instance.dataScript.RemoveActorFromPool(actorRecruited.actorID, actorRecruited.level, side);
                    //sprite of recruited actor
                    sprite = actorRecruited.arc.baseSprite;
                    //message
                    string textMsg = string.Format("{0}, {1}, ID {2} has been recruited", actorRecruited.actorName, actorRecruited.arc.name.ToUpper(),
                        actorRecruited.actorID);
                    Message message = GameManager.instance.messageScript.ActorRecruited(textMsg, data.nodeID, actorRecruited.actorID, Side.Authority);
                    if (message != null) { GameManager.instance.dataScript.AddMessage(message); }
                    //actor successfully recruited
                    builderTop.Append(string.Format("{0}The interview went well!{1}", colourNormal, colourEnd));
                    builderBottom.Append(string.Format("{0}{1}{2}, {3}\"{4}\", has been recruited and is available in the Reserve List{5}", colourArc,
                        actorRecruited.arc.name, colourEnd, colourNormal, actorRecruited.actorName, colourEnd));
                }
                else
                {
                    //some issue prevents actor being added to reserve pool (full? -> probably not as a criteria checks this)
                    successFlag = false;
                }
            }
            else
            {
                Debug.LogWarning(string.Format("Invalid Recruited Actor (Null) for actorID {0}", data.optionID));
                successFlag = false;
            }
        }
        else
        { Debug.LogWarning(string.Format("Invalid optionID {0}", data.optionID)); }
        //failed outcome
        if (successFlag == false)
        {
            builderTop.Append("Something has gone wrong. Our Recruit has gone missing");
            builderBottom.Append("This is a matter of concern");
        }
        //
        // - - - Outcome - - - 
        //
        ModalOutcomeDetails details = new ModalOutcomeDetails();
        details.textTop = builderTop.ToString();
        details.textBottom = builderBottom.ToString();
        details.sprite = sprite;
        details.side = side;
        if (successFlag == true)
        { details.isAction = true; }
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details);
    }

    /// <summary>
    /// Debug method to display actor pools (both sides)
    /// </summary>
    /// <returns></returns>
    public string DisplayPools()
    {
        List<int> listOfActors = new List<int>();
        StringBuilder builder = new StringBuilder();
        //Resistance
        builder.Append(DisplaySubPool(GameManager.instance.dataScript.GetActorPool(1, Side.Resistance), " ResistanceActorPoolLevelOne"));
        builder.Append(DisplaySubPool(GameManager.instance.dataScript.GetActorPool(2, Side.Resistance), " ResistanceActorPoolLevelTwo"));
        builder.Append(DisplaySubPool(GameManager.instance.dataScript.GetActorPool(3, Side.Resistance), " ResistanceActorPoolLevelThree"));
        //Authority
        builder.Append(DisplaySubPool(GameManager.instance.dataScript.GetActorPool(1, Side.Authority), " AuthorityActorPoolLevelOne"));
        builder.Append(DisplaySubPool(GameManager.instance.dataScript.GetActorPool(2, Side.Authority), " AuthorityActorPoolLevelTwo"));
        builder.Append(DisplaySubPool(GameManager.instance.dataScript.GetActorPool(3, Side.Authority), " AuthorityActorPoolLevelThree"));
        return builder.ToString();
    }

    /// <summary>
    /// submethod for DisplayPools
    /// </summary>
    /// <param name="listOfActors"></param>
    /// <param name="header"></param>
    /// <returns></returns>
    private string DisplaySubPool(List<int> listOfActors, string header)
    {
        StringBuilder subBuilder = new StringBuilder();
        subBuilder.Append(header);
        subBuilder.AppendLine();
        for (int i = 0; i < listOfActors.Count; i++)
        {
            Actor actor = GameManager.instance.dataScript.GetActor(listOfActors[i]);
            subBuilder.Append(string.Format(" actID {0}, {1}, L{2}, {3}-{4}-{5}{6}", actor.actorID, actor.arc.name.ToLower(), actor.level,
                actor.datapoint0, actor.datapoint1, actor.datapoint2, "\n"));
        }
        subBuilder.AppendLine();
        return subBuilder.ToString();
    }

    //new methods above here
}
