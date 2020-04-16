﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using gameAPI;
using modalAPI;
using packageAPI;

/// <summary>
/// handles all resistance capture and release matters
/// </summary>
public class CaptureManager : MonoBehaviour
{
    [Header("City Loyalty")]
    /*[Tooltip("The increase to city loyalty due to the Player being Captured")]
    [Range(0, 2)] public int playerCaptured = 1;*/
    [Tooltip("The increase to city loyalty due to a Resistance Actor, or Player, being Captured")]
    [Range(0, 2)] public int actorCaptured = 1;
    [Tooltip("The decrease to city loyalty due to an Actor being Released")]
    [Range(0, 2)] public int actorReleased = 1;

    [Header("Invisibility")]
    [Tooltip("The value of the Actor's invisibility upon Release")]
    [Range(0, 3)] public int releaseInvisibility = 3;

    [Header("Detention Period")]
    [Tooltip("How many turns will actor be held for when Captured (Player hard coded)")]
    [Range(1, 10)] public int captureTimerValue = 3;

    [Header("Capture Tools")]
    [Tooltip("CaptureTool for dealing with Innocence level 3 InterroBots. Optional")]
    public CaptureTool innocence_3;
    [Tooltip("CaptureTool for dealing with Innocence level 2 Inquisitors. Optional")]
    public CaptureTool innocence_2;
    [Tooltip("CaptureTool for dealing with Innocence level 1 TortureBots. Optional")]
    public CaptureTool innocence_1;
    [Tooltip("CaptureTool for dealing with Innocence level 0 Mayor. Optional")]
    public CaptureTool innocence_0;

    //fast access
    private int teamErasureID;
    private Condition conditionQuestionable;

    private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourAlert;
    private string colourEnd;

    /// <summary>
    /// Not for GameState.LoadGame
    /// </summary>
    /// <param name="state"></param>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseEvents();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitailiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //fast access
        teamErasureID = GameManager.instance.dataScript.GetTeamArcID("ERASURE");
        conditionQuestionable = GameManager.instance.dataScript.GetCondition("QUESTIONABLE");
        Debug.Assert(teamErasureID > -1, "Invalid teamErasureID");
        Debug.Assert(conditionQuestionable != null, "Invalid conditionQuestionable (Null)");
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "CaptureManager");
        EventManager.instance.AddListener(EventType.Capture, OnEvent, "CaptureManager");
        /*EventManager.instance.AddListener(EventType.ReleasePlayer, OnEvent, "CaptureManager");*/
        EventManager.instance.AddListener(EventType.ReleaseActor, OnEvent, "CaptureManager");
        EventManager.instance.AddListener(EventType.StartTurnEarly, OnEvent, "CaptureManager");
    }
    #endregion

    #endregion

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
            case EventType.Capture:
                CaptureDetails details = Param as CaptureDetails;
                CaptureSomebody(details);
                break;
            /*case EventType.ReleasePlayer:
                ReleasePlayer();
                break;*/
            case EventType.ReleaseActor:
                Actor actor = Param as Actor;
                ReleaseActor(actor);
                break;
            case EventType.StartTurnEarly:
                CheckStartTurnCapture();
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
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodText);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralText);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.salmonText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    //
    // - - - Capture and Release
    //

    public void CaptureSomebody(CaptureDetails details)
    {
        if (details != null)
        {
            if (details.actor == null)
            { CapturePlayer(details); }
            else
            { CaptureActor(details); }
        }
        else { Debug.LogError("Invalid CaptureDetails (Null)"); }
    }


    /// <summary>
    /// Player captured.
    /// Note: Node and Team already checked for null by parent method
    /// </summary>
    /// <param name="node"></param>
    /// <param name="team"></param>
    private void CapturePlayer(CaptureDetails details, bool isStartOfTurn = false)
    {
        //PLAYER CAPTURED
        string text = string.Format("Player Captured at {0}{1}{2}, {3}{4}{5} district by {6}{7} {8}{9}", colourAlert, details.node.nodeName, colourEnd, 
            colourAlert, details.node.Arc.name, colourEnd, colourBad, details.team.arc.name, details.team.teamName, colourEnd);
        //AutoRun (both sides)
        if (GameManager.instance.turnScript.CheckIsAutoRun() == true)
        {
            string textAutoRun = string.Format("{0} {1}Captured{2}", GameManager.instance.playerScript.GetPlayerNameResistance(), colourBad, colourEnd);
            GameManager.instance.dataScript.AddHistoryAutoRun(textAutoRun);
        }
        //detention period -> Note: Player only ever incarcerated for one turn (needs to be '2' for sequencing issues)
        GameManager.instance.actorScript.captureTimerPlayer = 2;
        //effects builder
        StringBuilder builder = new StringBuilder();
        //any carry over text?
        if (string.IsNullOrEmpty(details.effects) == false)
        { builder.Append(string.Format("{0}{1}{2}", details.effects, "\n", "\n")); }
        builder.Append(string.Format("{0}Player has been Captured{1}{2}{3}", colourBad, colourEnd, "\n", "\n"));
        //message
        GameManager.instance.messageScript.ActorCapture(text, details.node, details.team);
        //update node trackers
        GameManager.instance.nodeScript.nodePlayer = -1;
        GameManager.instance.nodeScript.nodeCaptured = details.node.nodeID;
        //Raise city loyalty
        int cause = GameManager.instance.cityScript.CityLoyalty;
        cause += actorCaptured;
        cause = Mathf.Min(GameManager.instance.cityScript.maxCityLoyalty, cause);
        GameManager.instance.cityScript.CityLoyalty = cause;
        //invisibility set to zero (most likely already is)
        GameManager.instance.playerScript.Invisibility = 0;
        //statistics
        GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerCaptured);
        //update map
        GameManager.instance.nodeScript.NodeRedraw = true;
        //set security state back to normal
        GameManager.instance.authorityScript.SetAuthoritySecurityState("Security measures have been cancelled", string.Format("{0}, Player, has been CAPTURED", GameManager.instance.playerScript.PlayerName));
        //change player state
        if (GameManager.instance.sideScript.resistanceOverall == SideState.Human)
        {
            //Human resistance player
            GameManager.instance.playerScript.status = ActorStatus.Captured;
            GameManager.instance.playerScript.tooltipStatus = ActorTooltip.Captured;
            GameManager.instance.playerScript.inactiveStatus = ActorInactive.None;
            //AI side tab
            GameManager.instance.aiScript.UpdateSideTabData();
            //add renown to authority actor who owns the team (only if they are still OnMap
            if (GameManager.instance.sideScript.authorityOverall == SideState.Human)
            {
                if (GameManager.instance.dataScript.CheckActorSlotStatus(details.team.actorSlotID, GameManager.instance.globalScript.sideAuthority) == true)
                {
                    Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.team.actorSlotID, GameManager.instance.globalScript.sideAuthority);
                    if (actor != null)
                    { actor.Renown++; }
                    else { Debug.LogError(string.Format("Invalid actor (null) from team.ActorSlotID {0}", details.team.actorSlotID)); }
                }
            }
            builder.AppendFormat("{0}City Loyalty +{1}{2}{3}{4}", colourBad, actorCaptured, colourEnd, "\n", "\n");
            //Gear confiscated
            int numOfGear = GameManager.instance.playerScript.CheckNumOfGear();
            if (numOfGear > 0)
            {
                List<string> listOfGear = GameManager.instance.playerScript.GetListOfGear();
                if (listOfGear != null)
                {
                    //reverse loop through list of gear and remove all
                    for (int i = listOfGear.Count - 1; i >= 0; i--)
                    { GameManager.instance.playerScript.RemoveGear(listOfGear[i], true); }
                    builder.Append(string.Format("{0}Gear confiscated ({1} item{2}){3}", colourBad, numOfGear, numOfGear != 1 ? "s" : "", colourEnd));
                }
                else { Debug.LogError("Invalid listOfGear (Null)"); }
            }
            //switch off flashing red indicator on top widget UI
            EventManager.instance.PostNotification(EventType.StopSecurityFlash, this, null, "CaptureManager.cs -> CapturePlayer");
            //reduce player alpha to show inactive (sprite and text)
            GameManager.instance.actorPanelScript.UpdatePlayerAlpha(GameManager.instance.guiScript.alphaInactive);
            //player captured outcome window
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
            {
                textTop = text,
                textBottom = builder.ToString(),
                sprite = GameManager.instance.guiScript.capturedSprite,
                isAction = false,
                side = GameManager.instance.globalScript.sideResistance,
                type = MsgPipelineType.CapturePlayer
            };
            if (GameManager.instance.guiScript.InfoPipelineAdd(outcomeDetails) == false)
            { Debug.LogWarningFormat("Player Captured infoPipeline message FAILED to be added to dictOfPipeline"); }

            /*EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "CaptureManager.cs -> CapturePlayer"); NO NEED AS IN INFOPIPELINE ALREADY */
        }
        else
        {
            //AI Resistance Player
            GameManager.instance.aiRebelScript.status = ActorStatus.Captured;
            //Remove Gear
            GameManager.instance.aiRebelScript.GearPoolEmpty("being CAPTURED");
        }
    }

    /// <summary>
    /// Resistance Actor (Human/AI) captured
    /// NOTE: node, team and actor checked for null by parent method
    /// </summary>
    /// <param name="node"></param>
    /// <param name="team"></param>
    /// <param name="actor"></param>
    private void CaptureActor(CaptureDetails details)
    {
        string text = string.Format("{0}, {1}, Captured at \"{2}\", {3}", details.actor.actorName, details.actor.arc.name, details.node.nodeName, details.node.Arc.name);
        //message
        GameManager.instance.messageScript.ActorCapture(text, details.node, details.team, details.actor.actorID);
        //AutoRun (both sides)
        if (GameManager.instance.turnScript.CheckIsAutoRun() == true)
        {
            string textAutoRun = string.Format("{0}{1}{2}, {3}Captured{4}", colourAlert, details.actor.arc.name, colourEnd, colourBad, colourEnd);
            GameManager.instance.dataScript.AddHistoryAutoRun(textAutoRun);
        }
        //detention period
        details.actor.captureTimer = captureTimerValue;
        //raise city loyalty
        int cause = GameManager.instance.cityScript.CityLoyalty;
        cause += actorCaptured;
        cause = Mathf.Min(GameManager.instance.cityScript.maxCityLoyalty, cause);
        GameManager.instance.cityScript.CityLoyalty = cause;
        //invisibility set to zero (most likely already is)
        details.actor.SetDatapoint(ActorDatapoint.Invisibility2, 0);
        //update map
        GameManager.instance.nodeScript.NodeRedraw = true;
        //update contacts
        GameManager.instance.contactScript.UpdateNodeContacts();
        //admin
        GameManager.instance.actorScript.numOfActiveActors--;
        details.actor.Status = ActorStatus.Captured;
        details.actor.inactiveStatus = ActorInactive.None;
        details.actor.tooltipStatus = ActorTooltip.Captured;
        details.actor.nodeCaptured = details.node.nodeID;
        details.actor.numOfTimesCaptured++;
        if (GameManager.instance.sideScript.resistanceOverall == SideState.Human)
        {
            //effects builder
            StringBuilder builder = new StringBuilder();
            //any carry over text?
            if (string.IsNullOrEmpty(details.effects) == false)
            { builder.Append(string.Format("{0}{1}{2}", details.effects, "\n", "\n")); }
            builder.Append(string.Format("{0}{1} has been Captured{2}{3}{4}", colourBad, details.actor.arc.name, colourEnd, "\n", "\n"));
            builder.AppendFormat("{0}City Loyalty +{1}{2}{3}{4}", colourBad, actorCaptured, colourEnd, "\n", "\n");
            //reduce actor alpha to show inactive (sprite and text)
            GameManager.instance.actorPanelScript.UpdateActorAlpha(details.actor.slotID, GameManager.instance.guiScript.alphaInactive);
            //actor captured outcome window
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
            {
                textTop = text,
                textBottom = builder.ToString(),
                sprite = GameManager.instance.guiScript.errorSprite,
                isAction = false,
                side = GameManager.instance.globalScript.sideResistance
            };
            //edge case of Inactive player and Authority AI capture an actor which may (?) generate a message that'll upset the info pipeline. If player active, display at time of action, otherwise put in pipeline
            if (GameManager.instance.playerScript.status != ActorStatus.Active)
            { outcomeDetails.type = MsgPipelineType.CaptureActor; }
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "CaptureManager.cs -> CaptureActor");
        }
    }


    /// <summary>
    /// Release, or Escapes,  Human/AI Resitance player from captitivity. 'isMessage' true for an outcome message (go false if called via EffectManager.cs). 'isReleased' false then assumed to have escaped
    /// </summary>
    public void ReleasePlayer(bool isMessage = false, bool isReleased = true)
    {
        string text;
        StringBuilder builder = new StringBuilder();
        //update nodes
        int nodeID = GameManager.instance.nodeScript.nodeCaptured;
        //message
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        if (node != null)
        {
            if (isReleased == true)
            {
                //released
                text = string.Format("{0}, Player, released at \"{1}\", {2}", GameManager.instance.playerScript.GetPlayerNameResistance(), node.nodeName, node.Arc.name);
                GameManager.instance.messageScript.ActorRelease(text, node, GameManager.instance.playerScript.actorID);
            }
            else
            {
                //escapes (can only do so with help of OrgEmergency)
                text = string.Format("{0}, Player, escapes from captivity at \"{1}\", {2}", GameManager.instance.playerScript.GetPlayerNameResistance(), node.nodeName, node.Arc.name);
                GameManager.instance.messageScript.PlayerEscapes(text, node);
            }
            Debug.LogFormat("[Ply] CaptureManager.cs -> ReleasePlayer: {0}{1}", text, "\n");
            //actor gains condition questionable (do BEFORE updating nodeID's)
            GameManager.instance.playerScript.AddCondition(conditionQuestionable, GameManager.instance.globalScript.sideResistance, "Has been interrogated by Authority");
            //update nodeID's
            GameManager.instance.nodeScript.nodePlayer = nodeID;
            GameManager.instance.nodeScript.nodeCaptured = -1;
            //decrease city loyalty
            int cause = GameManager.instance.cityScript.CityLoyalty;
            cause -= actorReleased;
            cause = Mathf.Max(0, cause);
            GameManager.instance.cityScript.CityLoyalty = cause;
            //zero out capture timer
            GameManager.instance.actorScript.captureTimerPlayer = 0;
            //invisibility
            int invisibilityNew = releaseInvisibility;
            GameManager.instance.playerScript.Invisibility = invisibilityNew;
            //autorun
            if (GameManager.instance.turnScript.CheckIsAutoRun() == true)
            {
                text = string.Format("{0} {1}Released{2}", GameManager.instance.playerScript.GetPlayerNameResistance(), colourGood, colourEnd);
                GameManager.instance.dataScript.AddHistoryAutoRun(text);
            }
            //human resistance player
            if (GameManager.instance.sideScript.resistanceOverall == SideState.Human)
            {
                //reset state
                GameManager.instance.playerScript.status = ActorStatus.Active;
                GameManager.instance.playerScript.tooltipStatus = ActorTooltip.None;
                builder.AppendFormat("{0}City Loyalty -{1}{2}{3}{4}", colourGood, actorReleased, colourEnd, "\n", "\n");
                builder.AppendFormat("{0}Player's Invisibility +{1}{2}{3}{4}", colourGood, invisibilityNew, colourEnd, "\n", "\n");
                builder.AppendFormat("{0}QUESTIONABLE condition gained{1}", colourBad, colourEnd);
                //AI side tab (otherwise 'player indisposed' message when accessing tab)
                GameManager.instance.aiScript.UpdateSideTabData();
                //update Player alpha
                GameManager.instance.actorPanelScript.UpdatePlayerAlpha(GameManager.instance.guiScript.alphaActive);
                //update map
                GameManager.instance.nodeScript.NodeRedraw = true;
                if (isMessage == true)
                {
                    //player released outcome window 
                    ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
                    {
                        textTop = text,
                        textBottom = builder.ToString(),
                        sprite = GameManager.instance.guiScript.errorSprite,
                        isAction = false,
                        side = GameManager.instance.globalScript.sideResistance,
                        type = MsgPipelineType.ReleasePlayer
                    };
                    if (GameManager.instance.guiScript.InfoPipelineAdd(outcomeDetails) == false)
                    { Debug.LogWarningFormat("Player Released from Captivity infoPipeline message FAILED to be added to dictOfPipeline"); }
                }
            }
            else
            {
                //AI resistance player
                GameManager.instance.aiRebelScript.status = ActorStatus.Active;

            }
        }
        else { Debug.LogError(string.Format("Invalid node (Null) for nodeId {0}", nodeID)); }
    }

    /// <summary>
    /// Release actor from captitivty, only Actor is needed from CaptureDetails
    /// </summary>
    /// <param name="actorID"></param>
    public void ReleaseActor(Actor actor)
    {
        string text = "unknown";
        if (actor != null)
        {
            if (actor.Status == ActorStatus.Captured)
            {
                StringBuilder builder = new StringBuilder();
                //node (needed only for record keeping / messaging purposes NOTE: needs to be done here as it's reset to -1 thereafter
                Node node = GameManager.instance.dataScript.GetNode(actor.nodeCaptured);
                if (node != null)
                {
                    actor.nodeCaptured = -1;
                    //reset actor state
                    actor.Status = ActorStatus.Active;
                    actor.tooltipStatus = ActorTooltip.None;
                    //actor gains condition questionable
                    actor.AddCondition(conditionQuestionable, "has been interrogated by Authority");
                    //decrease City loyalty
                    int cause = GameManager.instance.cityScript.CityLoyalty;
                    cause -= actorReleased;
                    cause = Mathf.Max(0, cause);
                    GameManager.instance.cityScript.CityLoyalty = cause;
                    builder.AppendFormat("{0}City Loyalty -{1}{2}{3}{4}", colourGood, actorReleased, colourEnd, "\n", "\n");
                    //invisibility
                    int invisibilityNew = releaseInvisibility;
                    actor.SetDatapoint(ActorDatapoint.Invisibility2, invisibilityNew);
                    builder.AppendFormat("{0}{1} Invisibility +{2}{3}", colourGood, actor.actorName, invisibilityNew, colourEnd);
                    //update contacts
                    GameManager.instance.contactScript.UpdateNodeContacts();
                    //admin
                    GameManager.instance.actorScript.numOfActiveActors++;
                    //traitor
                    int rndNum = Random.Range(0, 100);
                    int chance = actor.numOfTimesCaptured * GameManager.instance.actorScript.actorTraitorChance;
                    if (rndNum < chance)
                    {
                        actor.isTraitor = true;
                        GameManager.instance.dataScript.StatisticIncrement(StatType.ActorResistanceTraitors);
                        Debug.LogFormat("[Rnd] CaptureManager.cs -> ReleaseActor: {0}, {1}, becomes a TRAITOR (need {2}, rolled {3}){4}", actor.actorName, actor.arc.name, chance, rndNum, "\n");
                    }
                    else { Debug.LogFormat("[Rnd] CaptureManager.cs -> ReleaseActor: {0}, {1}, does NOT become a Traitor (need {2}, rolled {3}){4}", actor.actorName, actor.arc.name, chance, rndNum, "\n"); }
                    //message
                    text = string.Format("{0}, {1}, released from captivity", actor.actorName, actor.arc.name);
                    GameManager.instance.messageScript.ActorRelease(text, node, actor.actorID);
                    Debug.LogFormat("[Ply] CaptureManager.cs -> ReleaseActor: {0}{1}", text, "\n");
                    //autorun
                    if (GameManager.instance.turnScript.CheckIsAutoRun() == true)
                    {
                        text = string.Format("{0}{1}{2}, {3}Released{4}", colourAlert, actor.arc.name, colourEnd, colourGood, colourEnd);
                        GameManager.instance.dataScript.AddHistoryAutoRun(text);
                    }
                    //Human resistance player
                    if (GameManager.instance.sideScript.resistanceOverall == SideState.Human)
                    {
                        //update actor alpha
                        GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.slotID, GameManager.instance.guiScript.alphaActive);

                        /* NO outcome message for release actor, infoApp and actor alpha change only (messes with end of turn messages otherwise)
                         //actor released outcome window
                        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
                        {
                            textTop = text,
                            textBottom = builder.ToString(),
                            sprite = GameManager.instance.guiScript.errorSprite,
                            isAction = false,
                            side = GameManager.instance.globalScript.sideResistance
                        };
                        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "CaptureManager.cs -> ReleaseActor");*/
                    }
                    else
                    {
                        //AI Resistance player
                    }
                }
                else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", actor.nodeCaptured); }
            }
            else { Debug.LogWarningFormat("{0}, {1} can't be released as not presently captured", actor.arc.name, actor.actorName); }
        }
        else { Debug.LogError("Invalid details.actor (Null)"); }
    }


    /// <summary>
    /// Checks if AI/Human Resistance player/actor captured by an Erasure team at the node (must have invisibility 1 or less). Returns null if not.
    /// parameters vary if an 'APB' or a 'SecurityAlert' in play
    /// ActorID is default '999' for player
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public CaptureDetails CheckCaptured(int nodeID, int actorID = 999)
    {
        CaptureDetails details = null;
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        Team team = null;
        if (node != null)
        {
            //get correct player status depending on who is in charge of Resistance
            ActorStatus status = ActorStatus.Active;
            if (GameManager.instance.sideScript.resistanceOverall == SideState.Human)
            { status = GameManager.instance.playerScript.status; }
            else { status = GameManager.instance.aiRebelScript.status; }
            //correct state
            if (status == ActorStatus.Active)
            {
                //Player
                if (actorID == GameManager.instance.playerScript.actorID)
                {
                    //check player at node
                    if (nodeID == GameManager.instance.nodeScript.nodePlayer)
                    {
                        //Erasure team picks up player/actor immediately if invisibility low enough
                        if (CheckCaptureVisibility(GameManager.instance.playerScript.Invisibility) == true)
                        {
                            int teamID = node.CheckTeamPresent(teamErasureID);
                            if (teamID > -1)
                            {
                                team = GameManager.instance.dataScript.GetTeam(teamID);
                                if (team != null)
                                {
                                    //Player Captured
                                    details = new CaptureDetails { node = node, team = team, actor = null };
                                    Debug.LogFormat("[Ply] CaptureManager.cs -> CheckCaptured: Resistance Player is captured by an Erasure team at {0}, {1}, id {2}{3}",
                                        node.nodeName, node.Arc.name, node.nodeID, "\n");
                                }
                                else { Debug.LogError(string.Format("Invalid team (Null) for teamID {0}", teamID)); }
                            }
                            //Security Alert -> check if an Erasure team is in a neighbouring node
                            if (GameManager.instance.turnScript.authoritySecurityState == AuthoritySecurityState.SecurityAlert)
                            {
                                team = CheckCaptureAlert(node);
                                if (team != null)
                                {
                                    //Player Captured
                                    details = new CaptureDetails { node = node, team = team, actor = null };
                                    Debug.LogFormat("[Ply] CaptureManager.cs -> CheckCaptured: Resistance Player is captured by an Erasure team at {0}, {1}, id {2}{3}",
                                        node.nodeName, node.Arc.name, node.nodeID, "\n");
                                }
                            }
                        }
                    }
                    else { Debug.LogError(string.Format("Player not at the nodeID {0}", nodeID)); }
                }
                else
                {
                    //Actor
                    Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                    if (actor != null)
                    {
                        //Erasure team picks up player/actor immediately if invisibility 0
                        if (CheckCaptureVisibility(actor.GetDatapoint(ActorDatapoint.Invisibility2)) == true)
                        {
                            int teamID = node.CheckTeamPresent(teamErasureID);
                            if (teamID > -1)
                            {
                                team = GameManager.instance.dataScript.GetTeam(teamID);
                                if (team != null)
                                {
                                    //Actor Captured
                                    details = new CaptureDetails { node = node, team = team, actor = actor };
                                    Debug.LogFormat("[Ply] CaptureManager.cs -> CheckCaptured: {0}, {1},  is captured by an Erasure team at {2}, {3}, id {4}{5}", actor.actorName, actor.arc.name,
                                        node.nodeName, node.Arc.name, node.nodeID, "\n");
                                }
                                else { Debug.LogError(string.Format("Invalid team (Null) for teamID {0}", teamID)); }
                            }
                            //Security Alert -> Check if an Erasure team is in a neighbouring node
                            if (GameManager.instance.turnScript.authoritySecurityState == AuthoritySecurityState.SecurityAlert)
                            {
                                team = CheckCaptureAlert(node);
                                if (team != null)
                                {
                                    //Actor Captured
                                    details = new CaptureDetails { node = node, team = team, actor = actor };
                                    Debug.LogFormat("[Ply] CaptureManager.cs -> CheckCaptured: {0}, {1},  is captured by an Erasure team at {2}, {3}, id {4}{5}", actor.actorName, actor.arc.name,
                                        node.nodeName, node.Arc.name, node.nodeID, "\n");
                                }
                            }
                        }
                    }
                    else { Debug.LogError(string.Format("Invalid actor (Null) for actorID {0}", actorID)); }
                }
            }
        }
        else { Debug.LogError(string.Format("Invalid node (Null) for nodeID {0}", nodeID)); }
        //return CaptureDetails
        return details;
    }

    /// <summary>
    /// subMethod for checkCapture that assesses whether the actor is within the correct visibility level for a possible capture candidates
    /// </summary>
    /// <param name="actorInvisibility"></param>
    /// <returns></returns>
    private bool CheckCaptureVisibility(int actorInvisibility)
    {
        bool isAtRisk = false;
        switch(GameManager.instance.turnScript.authoritySecurityState)
        {
            case AuthoritySecurityState.APB:
                if (actorInvisibility <= 1) { isAtRisk = true; }
                break;
            default:
                if (actorInvisibility == 0) { isAtRisk = true; }
                break;
        }
        return isAtRisk;
    }

    /// <summary>
    /// subMethod for CheckCapture that handles Security Alert -> checks neighbouring nodes for presence of Erasure team. Returns team if found, null otherwise
    /// NOTE: node checked for Null by parent method
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private Team CheckCaptureAlert(Node node)
    {
        Team team = null;
        List<Node> listOfNeighbours = node.GetNeighbouringNodes();
        if (listOfNeighbours != null)
        {
            //loop each node and check for a match
            foreach (Node nodeCheck in listOfNeighbours)
            {
                //does any have an Erasure team present?
                int teamID = nodeCheck.CheckTeamPresent(teamErasureID);
                if (teamID > -1)
                {
                    team = GameManager.instance.dataScript.GetTeam(teamID);
                    break;
                }
            }
        }
        return team;
    }

    /// <summary>
    /// Checks player at start of turn (early) to see if invisibility is one or less and Erasure team present
    /// </summary>
    private void CheckStartTurnCapture()
    {
        //get correct player status depending on who is in charge of Resistance
        ActorStatus status = ActorStatus.Active;
        if (GameManager.instance.sideScript.resistanceOverall == SideState.Human)
        { status = GameManager.instance.playerScript.status; }
        else { status = GameManager.instance.aiRebelScript.status; }
        //only check if player active
        if ( status == ActorStatus.Active)
        {
            CaptureDetails details = CheckCaptured(GameManager.instance.nodeScript.nodePlayer);
            if (details != null)
            {
                //Player captured
                details.effects = string.Format("{0}They kicked in the door before you could get out of bed{1}", colourNeutral, colourEnd);
                CapturePlayer(details, true);
            }
        }
    }

    //
    // - - - Debug
    //

    /// <summary>
    /// Capture player with random team
    /// </summary>
    public void DebugCapturePlayer()
    {
        //player not already captured
        if (GameManager.instance.playerScript.status != ActorStatus.Captured)
        {
            //get player node
            Node node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
            if (node != null)
            {
                //get default ID 0 team (could be anything)
                Team team = GameManager.instance.dataScript.GetTeam(0);
                if (team != null)
                { CapturePlayer(new CaptureDetails() { node = node, team = team }); }
                else { Debug.LogError("Invalid team ID 0 (Null)"); }
            }
            else { Debug.LogErrorFormat("Invalid player node (Null), ID {0}", GameManager.instance.nodeScript.nodePlayer); }
        }
        else { Debug.LogWarning("Player can't be Debug Captured as they are already Captured"); }
    }

    //
    // - - - Capture Tools
    //

    /// <summary>
    /// Returns captureTool for the specific innocence level, null if not present
    /// </summary>
    /// <param name="innocenceLevel"></param>
    /// <returns></returns>
    public CaptureTool GetCaptureTool(int innocenceLevel)
    {
        CaptureTool tool = null;
        switch(innocenceLevel)
        {
            case 0: tool = innocence_0; break;
            case 1: tool = innocence_1; break;
            case 2: tool = innocence_2; break;
            case 3: tool = innocence_3; break;
            default: Debug.LogWarningFormat("Unrecognised innocenceLevel \"{0}\"", innocenceLevel); break;
        }
        return tool;
    }

    /// <summary>
    /// Returns true if specified innocence level has an associated CaptureTool present, false otherwise
    /// </summary>
    /// <param name="innocenceLevel"></param>
    /// <returns></returns>
    public bool CheckIfCaptureToolPresent(int innocenceLevel)
    {
        bool isPresent = false;
        switch (innocenceLevel)
        {
            case 0: if (innocence_0 != null) { isPresent = true; } break;
            case 1: if (innocence_1 != null) { isPresent = true; } break;
            case 2: if (innocence_2 != null) { isPresent = true; } break;
            case 3: if (innocence_3 != null) { isPresent = true; } break;
            default: Debug.LogWarningFormat("Unrecognised innocenceLevel \"{0}\"", innocenceLevel); break;
        }
        return isPresent;
    }

    //new methods above here
}
