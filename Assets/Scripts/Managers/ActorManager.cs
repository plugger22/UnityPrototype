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

    private Actor[] arrayOfActors;

    //colour palette for Generic tool tip
    private string colourBlue;
    private string colourRed;
    private string colourCancel;
    private string colourInvalid;
    private string colourEffect;
    private string colourEnd;

    public void Initialise()
    {
        //event listener is registered in InitialiseActors() due to GameManager sequence.
        GameManager.instance.eventScript.AddListener(EventType.ChangeColour, this.OnEvent);
        InitialiseActors(numOfActorsTotal);
    }

    //Called when events happen
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
    /// set colour palette for Generic Tool tip
    /// </summary>
    public void SetColours()
    {
        colourBlue = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourRed = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        colourCancel = GameManager.instance.colourScript.GetColour(ColourType.cancelNormal);
        colourInvalid = GameManager.instance.colourScript.GetColour(ColourType.cancelHighlight);
        colourEffect = GameManager.instance.colourScript.GetColour(ColourType.actionEffect);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// Set up number of required actors (minions supporting play)
    /// </summary>
    /// <param name="num"></param>
    public void InitialiseActors(int num)
    {
        //number of actors, default 4
        numOfActorsTotal = numOfActorsTotal == 4 ? numOfActorsTotal : 4;
        numOfActorsCurrent = numOfActorsCurrent < 1 ? 1 : numOfActorsCurrent;
        numOfActorsCurrent = numOfActorsCurrent > numOfActorsTotal ? numOfActorsTotal : numOfActorsCurrent;

        arrayOfActors = new Actor[numOfActorsTotal];
        if (num > 0)
        {
            //get a list of random actorArcs
            List<ActorArc> tempActorArcs = GameManager.instance.dataScript.GetRandomActorArcs(num);
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
                    isActive = true
                };
                arrayOfActors[i] = actor;

                Debug.Log("Actor added -> " + actor.arc.actorName + ", Ability " + actor.Connections + "\n");
            }                
            //add actions to dictionary
            GameManager.instance.dataScript.AddActions(arrayOfActors);
        }
        else { Debug.LogWarning("Invalid number of Actors (Zero, or less)"); }

    }

    /// <summary>
    /// Get array of actors
    /// </summary>
    /// <returns></returns>
    public Actor[] GetActors()
    { return arrayOfActors; }

    /// <summary>
    /// returns type of Actor, eg. 'Fixer', based on slotID (0 to 3)
    /// </summary>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public string GetActorType(int slotID)
    {
        Debug.Assert(slotID > -1 && slotID < numOfActorsTotal, "Invalid slotID input");
        return arrayOfActors[slotID].arc.name;
    }

    /// <summary>
    /// returns array of Stats -> [0] Connections, [1] Motivation, [2] Invisibility
    /// </summary>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public int[] GetActorStats(int slotID)
    {
        Debug.Assert(slotID > -1 && slotID < numOfActorsTotal, "Invalid slotID input");
        int[] arrayOfStats = new int[]{ arrayOfActors[slotID].Connections, arrayOfActors[slotID].Motivation, arrayOfActors[slotID].Invisibility};
        return arrayOfStats;
    }

    /// <summary>
    /// return a specific actor's name
    /// </summary>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public string GetActorName(int slotID)
    {
        Debug.Assert(slotID > -1 && slotID < numOfActorsTotal, "Invalid slotID input");
        return arrayOfActors[slotID].arc.actorName;
    }

    /// <summary>
    /// return a specific actor's trait
    /// </summary>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public string GetActorTrait(int slotID)
    {
        Debug.Assert(slotID > -1 && slotID < numOfActorsTotal, "Invalid slotID input");
        return arrayOfActors[slotID].trait.name;
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
        GameObject nodeObject = GameManager.instance.dataScript.GetNodeObject(nodeID);
        if (nodeObject != null)
        {
            Node node = nodeObject.GetComponent<Node>();
            List<ActionEffect> listOfEffects = new List<ActionEffect>();
            Action tempAction;
            EventButtonDetails details;
            //player present ('Cancel' tooltip)
            if (nodeID == playerID)
            { playerPresent = "You are present at the node"; }
            else { playerPresent = "You are NOT present at the node"; }
            //loop actors currently in game
            foreach (Actor actor in arrayOfActors)
            {
                proceedFlag = true;
                details = null;
                //actor active?
                if (actor.isActive == true)
                {
                    if (GameManager.instance.levelScript.CheckNodeActive(node.nodeID, actor.SlotID) == true || nodeID == playerID )
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
                                    ActionEffect effect = listOfEffects[i];
                                    //check effect criteria is valid
                                    effectCriteria = GameManager.instance.actionScript.CheckEffectCriteria(effect, nodeID);
                                    if (effectCriteria == null)
                                    {
                                        //Effect criteria O.K -> tool tip text
                                        if (builder.Length > 0)  { builder.AppendLine(); }
                                        builder.Append(string.Format("{0}{1}{2}", colourEffect, effect.description, colourEnd));
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
                                //renown to player if player at node, otherwise rebel
                                builder.AppendLine();
                                if (nodeID == playerID)
                                { builder.Append(string.Format("{0}Player Renown +1{1}", colourBlue, colourEnd)); }
                                else { builder.Append(string.Format("{0}{1} Renown +1{2}", colourRed, actor.arc.name, colourEnd)); }

                                //Outcome window details -> Placeholder test data
                                ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails() {
                                    textTop = string.Format("{0} has worked their magic on the {1} node", actor.arc.name, node.arc.name),
                                    textBottom = string.Format("Security -1{0}{1} Renown +1", "\n", actor.arc.name),
                                    sprite = null
                                };
                                
                                //pass all relevant details to ModalActionMenu via Node.OnClick()
                                details = new EventButtonDetails()
                                {
                                    buttonTitle = tempAction.name,
                                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name.ToUpper(), colourEnd),
                                    buttonTooltipMain = tempAction.tooltipText,
                                    buttonTooltipDetail = builder.ToString(),
                                    //use a Lambda to pass arguments to the action
                                    action = () => { GameManager.instance.eventScript.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails); }
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
                    //action = GameManager.instance.actionMenuScript.CloseActionMenu
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
                    //action = GameManager.instance.actionMenuScript.CloseActionMenu
                };
            }
            tempList.Add(cancelDetails);
        }
        return tempList;
    }

}
