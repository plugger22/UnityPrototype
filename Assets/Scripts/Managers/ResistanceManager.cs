﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using modalAPI;
using gameAPI;

/// <summary>
/// Handles all Resitance related matters
/// </summary>
public class ResistanceManager : MonoBehaviour
{
    [Tooltip("The reduction to the ResistanceCause due to the Player being Captured")]
    [Range(1, 4)] public int playerCaptured = 2;
    [Tooltip("The reduction to the ResistanceCause due to a Resistance Actor being Captured")]
    [Range(1, 4)] public int actorCaptured = 1;
    [Tooltip("The increase to the ResistanceCause due to an Actor being Released")]
    [Range(1, 4)] public int actorReleased = 1;
    [Tooltip("The value of the Actor's invisibility upon Release")]
    [Range(0, 3)] public int releaseInvisibility = 2;

    [HideInInspector] public int resistanceCauseMax;                        //level of Rebel Support. Max out to Win the level. Max level is a big part of difficulty.
    [HideInInspector] public int resistanceCause;                           //current level of Rebel Support

    private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourEnd;


    public void Initialise()
    {
        resistanceCauseMax = 10;
        resistanceCause = 0;
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
        EventManager.instance.AddListener(EventType.CaptureActor, OnEvent);
        EventManager.instance.AddListener(EventType.CapturePlayer, OnEvent);
    }

    /// <summary>
    /// event handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //detect event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.CapturePlayer:
                AIDetails detailsPlayer = Param as AIDetails;
                CapturePlayer(detailsPlayer);
                break;
            case EventType.CaptureActor:
                AIDetails detailsActor = Param as AIDetails;
                CaptureActor(detailsActor);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// Set colour palette for tooltip
    /// </summary>
    public void SetColours()
    {
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }


    /// <summary>
    /// Player captured.
    /// Note: Node and Team already checked for null by parent method
    /// </summary>
    /// <param name="node"></param>
    /// <param name="team"></param>
    public void CapturePlayer(AIDetails details)
    {
        //PLAYER CAPTURED
        string text = string.Format("Player Captured at \"{0}\", {1}", details.node.nodeName, details.node.Arc.name.ToUpper());
        //effects builder
        StringBuilder builder = new StringBuilder();
        //any carry over text?
        if (string.IsNullOrEmpty(details.effects) == false)
        { builder.Append(string.Format("{0}{1}{2}", details.effects, "\n", "\n")); }
        builder.Append(string.Format("{0}Player has been Captured{1}{2}{3}", colourBad, colourEnd, "\n", "\n"));
        //message
        Message message = GameManager.instance.messageScript.AICapture(text, details.node.nodeID, details.team.TeamID);
        GameManager.instance.dataScript.AddMessage(message);
        //update node trackers
        GameManager.instance.nodeScript.nodePlayer = -1;
        GameManager.instance.nodeScript.nodeCaptured = details.node.nodeID;
        //change player state
        GameManager.instance.turnScript.resistanceState = ResistanceState.Captured;
        //add renown to authority actor who owns the team
        Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.team.ActorSlotID, Side.Authority);
        if (actor != null)
        { actor.renown++; }
        else { Debug.LogError(string.Format("Invalid actor (null) from team.ActorSlotID {0}", details.team.ActorSlotID)); }
        //lower resistance Cause
        int cause = resistanceCause;
        cause -= playerCaptured;
        cause = Mathf.Max(0, cause);
        resistanceCause = cause;
        builder.Append(string.Format("{0}Resistance Cause -{1} (Now {2}){3}{4}{5}", colourBad, playerCaptured,
            cause, colourEnd, "\n", "\n"));
        //Gear confiscated
        int numOfGear = GameManager.instance.playerScript.GetNumOfGear();
        if (numOfGear > 0)
        {
            List<int> listOfGear = GameManager.instance.playerScript.GetListOfGear();
            if (listOfGear != null)
            {
                //reverse loop through list of gear and remove all
                for (int i = listOfGear.Count - 1; i >= 0; i--)
                { GameManager.instance.playerScript.RemoveGear(listOfGear[i]); }
                builder.Append(string.Format("{0}Gear confiscated ({1} item{2}){3}", colourBad, numOfGear, numOfGear != 1 ? "s" : "", colourEnd));
            }
            else { Debug.LogError("Invalid listOfGear (Null)"); }
        }
        //invisibility set to zero (most likely already is)
        GameManager.instance.playerScript.invisibility = 0;
        //update map
        GameManager.instance.nodeScript.NodeRedraw = true;
        //player captured outcome window
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        outcomeDetails.textTop = text;
        outcomeDetails.textBottom = builder.ToString();
        outcomeDetails.sprite = GameManager.instance.outcomeScript.errorSprite;
        outcomeDetails.isAction = false;
        outcomeDetails.side = Side.Resistance;
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
    }

    /// <summary>
    /// Resistance Actor captured
    /// NOTE: node, team and actor checked for null by parent method
    /// </summary>
    /// <param name="node"></param>
    /// <param name="team"></param>
    /// <param name="actor"></param>
    public void CaptureActor(AIDetails details)
    {        
        //effects builder
        StringBuilder builder = new StringBuilder();
        //any carry over text?
        if (string.IsNullOrEmpty(details.effects) == false)
        { builder.Append(string.Format("{0}{1}{2}", details.effects, "\n", "\n")); }
        string text = string.Format("Rebel {0} Captured at \"{1}\", {2}", details.actor.actorName, details.node.nodeName, details.node.Arc.name.ToUpper());
        builder.Append(string.Format("{0}{1} has been Captured{2}{3}{4}", colourBad, details.actor.arc.name.ToUpper(), colourEnd, "\n", "\n"));
        details.actor.status = ActorStatus.Captured;
        //message
        Message message = GameManager.instance.messageScript.AICapture(text, details.node.nodeID, details.team.TeamID, details.actor.actorID);
        GameManager.instance.dataScript.AddMessage(message);
        //lower resistance Cause
        int cause = resistanceCause;
        cause -= playerCaptured;
        cause = Mathf.Max(0, cause);
        resistanceCause = cause;
        builder.Append(string.Format("{0}Resistance Cause -{1} (Now {2}){3}{4}{5}", colourBad, playerCaptured,
            cause, colourEnd, "\n", "\n"));
        //invisibility set to zero (most likely already is)
        details.actor.datapoint2 = 0;
        //update map
        GameManager.instance.nodeScript.NodeRedraw = true;
        //actor captured outcome window
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        outcomeDetails.textTop = text;
        outcomeDetails.textBottom = builder.ToString();
        outcomeDetails.sprite = GameManager.instance.outcomeScript.errorSprite;
        outcomeDetails.isAction = false;
        outcomeDetails.side = Side.Resistance;
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
    }

    /// <summary>
    /// Release player from captitivity
    /// </summary>
    public void ReleasePlayer()
    {
        string colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        string colourEnd = GameManager.instance.colourScript.GetEndTag();
        StringBuilder builder = new StringBuilder();
        //update nodes
        int nodeID = GameManager.instance.nodeScript.nodeCaptured;
        GameManager.instance.nodeScript.nodePlayer = nodeID;
        GameManager.instance.nodeScript.nodeCaptured = -1;
        //reset state
        GameManager.instance.turnScript.resistanceState = ResistanceState.Normal;
        //increase resistance Cause
        int cause = resistanceCause;
        cause += actorReleased;
        cause = Mathf.Min(cause, resistanceCauseMax);
        resistanceCause = cause;
        builder.Append(string.Format("{0}Resistance Cause +{1} (Now {2}){3}{4}{5}", colourGood, actorReleased,
            cause, colourEnd, "\n", "\n"));
        //invisibility
        int invisibilityNew = releaseInvisibility;
        GameManager.instance.playerScript.invisibility = invisibilityNew;
        builder.Append(string.Format("{0}Player's Invisibility +{1} (Now {2}){3}", colourGood, invisibilityNew, invisibilityNew, colourEnd));
        //update map
        GameManager.instance.nodeScript.NodeRedraw = true;
        //message
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        if (node != null)
        {
            string text = string.Format("Player released at \"{0}\", {1}", node.nodeName, node.Arc.name.ToUpper());
            Message message = GameManager.instance.messageScript.AIRelease(text, nodeID, 999);
            GameManager.instance.dataScript.AddMessage(message);
            //player released outcome window
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.textTop = text;
            outcomeDetails.textBottom = builder.ToString();
            outcomeDetails.sprite = GameManager.instance.outcomeScript.errorSprite;
            outcomeDetails.isAction = false;
            outcomeDetails.side = Side.Resistance;
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
        }
        else { Debug.LogError(string.Format("Invalid node (Null) for nodeId {0}", nodeID)); }
    }




    //new methods above here
}
