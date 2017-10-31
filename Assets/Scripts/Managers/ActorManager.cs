using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using modalAPI;
using System;
using Random = UnityEngine.Random;
using System.Text;

/// <summary>
/// handles Actor related data and methods
/// </summary>
public class ActorManager : MonoBehaviour
{

    [HideInInspector] public int numOfActorsCurrent;    //NOTE -> Not hooked up yet (need to show blank actors for any that aren't currently in use)
    public int numOfActorsTotal = 4;      //if you increase this then GUI elements and GUIManager will need to be changed to accomodate it, default value 4
                                          //is the total for one side (duplicated by the other side)

    private Actor[,] arrayOfActors;       //indexes [Side, numOfActors], two sets are created, one for each side

    //colour palette for Generic tool tip
    private string colourBlue;
    private string colourRed;
    private string colourCancel;
    private string colourInvalid;
    private string colourEffect;
    private string colourDefault;
    private string colourEnd;

    public void Initialise()
    {
        //event listener is registered in InitialiseActors() due to GameManager sequence.
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
        PreInitialiseActors();
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

    private void PreInitialiseActors()
    {
        //number of actors, default 4
        numOfActorsTotal = numOfActorsTotal == 4 ? numOfActorsTotal : 4;
        numOfActorsCurrent = numOfActorsCurrent < 1 ? 1 : numOfActorsCurrent;
        numOfActorsCurrent = numOfActorsCurrent > numOfActorsTotal ? numOfActorsTotal : numOfActorsCurrent;
        //array
        arrayOfActors = new Actor[(int)Side.Count, numOfActorsTotal];
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
                    arc = tempActorArcs[i],
                    Name = tempActorArcs[i].actorName,
                    SlotID = i,
                    Connections = Random.Range(1, 4),
                    Motivation = Random.Range(1, 4),
                    Invisibility = Random.Range(1, 4),
                    trait = GameManager.instance.dataScript.GetRandomTrait(),
                    isLive = true
                };
                arrayOfActors[(int)side, i] = actor;

                Debug.Log("Actor added -> " + actor.arc.actorName + ", Ability " + actor.Connections + "\n");
            }
        }
        else { Debug.LogWarning("Invalid number of Actors (Zero, or less)"); }

    }

    /// <summary>
    /// Get array of actors for a specified side
    /// </summary>
    /// <returns></returns>
    public Actor[] GetActors(Side side)
    {
        Actor[] tempArray = new Actor[numOfActorsTotal];
        for (int i = 0; i < numOfActorsTotal; i++)
        { tempArray[i] = arrayOfActors[(int)side, i]; }
        return tempArray;
    }

    /// <summary>
    /// Get specific actor
    /// </summary>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public Actor GetActor(int slotID, Side side)
    {
        Debug.Assert(slotID > -1 && slotID < numOfActorsTotal, "Invalid slotID input");
        return arrayOfActors[(int)side, slotID];
    }

    /// <summary>
    /// returns type of Actor, eg. 'Fixer', based on slotID (0 to 3)
    /// </summary>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public string GetActorType(int slotID, Side side)
    {
        Debug.Assert(slotID > -1 && slotID < numOfActorsTotal, "Invalid slotID input");
        return arrayOfActors[(int)side, slotID].arc.name;
    }

    /// <summary>
    /// returns array of Stats -> [0] dataPoint0, [1] dataPoint1 , [2] dataPoint3
    /// </summary>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public int[] GetActorStats(int slotID, Side side)
    {
        Debug.Assert(slotID > -1 && slotID < numOfActorsTotal, "Invalid slotID input");
        int[] arrayOfStats = new int[]{ arrayOfActors[(int)side, slotID].Connections, arrayOfActors[(int)side, slotID].Motivation, arrayOfActors[(int)side, slotID].Invisibility};
        return arrayOfStats;
    }

    /// <summary>
    /// return a specific actor's name
    /// </summary>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public string GetActorName(int slotID, Side side)
    {
        Debug.Assert(slotID > -1 && slotID < numOfActorsTotal, "Invalid slotID input");
        return arrayOfActors[(int)side, slotID].arc.actorName;
    }

    /// <summary>
    /// return a specific actor's trait
    /// </summary>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public string GetActorTrait(int slotID, Side side)
    {
        Debug.Assert(slotID > -1 && slotID < numOfActorsTotal, "Invalid slotID input");
        return arrayOfActors[(int)side, slotID].trait.name;
    }

    /// <summary>
    /// Returns a list of all relevant Actor Actions for the  node to enable a ModalActionMenu to be put together (one button per action). 
    /// Max 4 Actor + 1 Target actions wiht an additional 'Cancel' buttnn added last automatically
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public List<EventButtonDetails> GetActorActions(int nodeID)
    {
        string sideColour;
        string playerPresent = null;
        string effectCriteria;
        bool proceedFlag;
        //color code for button tooltip header text, eg. "Operator"ss
        if (GameManager.instance.optionScript.PlayerSide == Side.Authority)
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
            //player present ('Cancel' tooltip)
            if (nodeID == playerID)
            { playerPresent = "You are present at the node"; }
            else { playerPresent = "You are NOT present at the node"; }
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
            foreach (Actor actor in arrayOfActors)
            {
                proceedFlag = true;
                details = null;

                //actualRenownEffect = null;

                //actor active?
                if (actor.isLive == true)
                {
                    if (GameManager.instance.levelScript.CheckNodeActive(node.NodeID, GameManager.instance.optionScript.PlayerSide, actor.SlotID) == true ||
                        nodeID == playerID )
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
                                for(int i = 0; i < listOfEffects.Count; i++)
                                {
                                    Effect effect = listOfEffects[i];
                                    //check effect criteria is valid
                                    effectCriteria = GameManager.instance.effectScript.CheckEffectCriteria(effect, nodeID);
                                    if (effectCriteria == null)
                                    {
                                        //Effect criteria O.K -> tool tip text
                                        if (builder.Length > 0)  { builder.AppendLine(); }
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
                                            colourInvalid, actor.arc.name.ToUpper(), "\n", colourEnd,
                                            colourRed, effectCriteria, colourEnd));
                                        proceedFlag = false;
                                    }
                                }
                            }
                            if (proceedFlag == true)
                            {
                                //Details to pass on for processing via button click
                                ModalActionDetails actionDetails = new ModalActionDetails() { };
                                actionDetails.NodeID = nodeID;
                                actionDetails.ActorSlotID = actor.SlotID;
                                //pass all relevant details to ModalActionMenu via Node.OnClick()
                                details = new EventButtonDetails()
                                {
                                    buttonTitle = tempAction.name,
                                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name.ToUpper(), colourEnd),
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
                        infoBuilder.Append(string.Format("{0} has no connections", actor.arc.name.ToUpper()));
                    }
                }
                else
                {
                    //actor gone silent
                    if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                    infoBuilder.Append(string.Format("{0} is lying low and unavailale", actor.arc.name.ToUpper()));
                }

                //add to list
                if (details != null)
                { tempList.Add(details); }
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
                    buttonTooltipMain = playerPresent,
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
                    buttonTooltipMain = playerPresent,
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this); }
                };
            }
            tempList.Add(cancelDetails);
        }
        else { Debug.LogError(string.Format("Invalid Node (null), ID {0}{1}", nodeID, "\n")); }
        return tempList;
    }

    /// <summary>
    /// returns slotID of actor if present and available (live), '-1' if not
    /// </summary>
    /// <param name="actorArcID"></param>
    /// <returns></returns>
    public int CheckActorPresent(int actorArcID)
    {
        int slotID = -1;
        foreach(Actor actor in arrayOfActors)
        {
            if (actor.arc.ActorArcID == actorArcID && actor.isLive == true)
            { return actor.SlotID; }
        }
        return slotID;
    }

}
