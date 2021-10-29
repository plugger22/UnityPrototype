using System.Collections;
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
    [Tooltip("Maximum number of capture tools that the player can have at a time")]
    public int maxCaptureTools = 4;
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
            case GameState.TutorialOptions:
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
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitailiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //fast access
        teamErasureID = GameManager.i.dataScript.GetTeamArcID("ERASURE");
        conditionQuestionable = GameManager.i.dataScript.GetCondition("QUESTIONABLE");
        Debug.Assert(teamErasureID > -1, "Invalid teamErasureID");
        Debug.Assert(conditionQuestionable != null, "Invalid conditionQuestionable (Null)");
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.i.AddListener(EventType.ChangeColour, OnEvent, "CaptureManager");
        EventManager.i.AddListener(EventType.Capture, OnEvent, "CaptureManager");
        /*EventManager.instance.AddListener(EventType.ReleasePlayer, OnEvent, "CaptureManager");*/
        EventManager.i.AddListener(EventType.ReleaseActor, OnEvent, "CaptureManager");
        EventManager.i.AddListener(EventType.StartTurnEarly, OnEvent, "CaptureManager");
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
        colourGood = GameManager.i.colourScript.GetColour(ColourType.goodText);
        colourNeutral = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourBad = GameManager.i.colourScript.GetColour(ColourType.badText);
        colourAlert = GameManager.i.colourScript.GetColour(ColourType.salmonText);
        colourEnd = GameManager.i.colourScript.GetEndTag();
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
        if (GameManager.i.turnScript.CheckIsAutoRun() == true)
        {
            string textAutoRun = string.Format("{0} {1}Captured{2}", GameManager.i.playerScript.GetPlayerNameResistance(), colourBad, colourEnd);
            GameManager.i.dataScript.AddHistoryAutoRun(textAutoRun);
        }
        //detention period -> Note: Player only ever incarcerated for one turn (needs to be '2' for sequencing issues)
        GameManager.i.actorScript.captureTimerPlayer = 2;
        //effects builder
        StringBuilder builder = new StringBuilder();
        //any carry over text?
        if (string.IsNullOrEmpty(details.effects) == false)
        { builder.Append(string.Format("{0}{1}{2}", details.effects, "\n", "\n")); }
        builder.Append(string.Format("{0}Player has been Captured{1}{2}{3}", colourBad, colourEnd, "\n", "\n"));
        //message
        GameManager.i.messageScript.ActorCapture(text, details.node, details.team);
        //update node trackers
        GameManager.i.nodeScript.nodePlayer = -1;
        GameManager.i.nodeScript.nodeCaptured = details.node.nodeID;
        //Raise city loyalty
        int cause = GameManager.i.cityScript.CityLoyalty;
        cause += actorCaptured;
        cause = Mathf.Min(GameManager.i.cityScript.maxCityLoyalty, cause);
        GameManager.i.cityScript.CityLoyalty = cause;
        //invisibility set to zero (most likely already is)
        GameManager.i.playerScript.Invisibility = 0;
        //statistics
        GameManager.i.dataScript.StatisticIncrement(StatType.PlayerCaptured);
        //update map
        GameManager.i.nodeScript.NodeRedraw = true;
        //set security state back to normal
        GameManager.i.authorityScript.SetAuthoritySecurityState("Security measures have been cancelled", string.Format("{0}, Player, has been CAPTURED", GameManager.i.playerScript.PlayerName));
        //change player state
        if (GameManager.i.sideScript.resistanceOverall == SideState.Human)
        {
            //Human resistance player
            GameManager.i.playerScript.status = ActorStatus.Captured;
            GameManager.i.playerScript.tooltipStatus = ActorTooltip.Captured;
            GameManager.i.playerScript.inactiveStatus = ActorInactive.None;
            //AI side tab
            GameManager.i.aiScript.UpdateSideTabData();
            //add power to authority actor who owns the team (only if they are still OnMap
            if (GameManager.i.sideScript.authorityOverall == SideState.Human)
            {
                if (GameManager.i.dataScript.CheckActorSlotStatus(details.team.actorSlotID, GameManager.i.globalScript.sideAuthority) == true)
                {
                    Actor actor = GameManager.i.dataScript.GetCurrentActor(details.team.actorSlotID, GameManager.i.globalScript.sideAuthority);
                    if (actor != null)
                    { actor.Power++; }
                    else { Debug.LogError(string.Format("Invalid actor (null) from team.ActorSlotID {0}", details.team.actorSlotID)); }
                }
            }
            builder.AppendFormat("{0}City Loyalty +{1}{2}{3}{4}", colourBad, actorCaptured, colourEnd, "\n", "\n");
            //Gear confiscated
            int numOfGear = GameManager.i.playerScript.CheckNumOfGear();
            if (numOfGear > 0)
            {
                List<string> listOfGear = GameManager.i.playerScript.GetListOfGear();
                if (listOfGear != null)
                {
                    //reverse loop through list of gear and remove all
                    for (int i = listOfGear.Count - 1; i >= 0; i--)
                    { GameManager.i.playerScript.RemoveGear(listOfGear[i], true); }
                    builder.Append(string.Format("{0}Gear confiscated ({1} item{2}){3}", colourBad, numOfGear, numOfGear != 1 ? "s" : "", colourEnd));
                }
                else { Debug.LogError("Invalid listOfGear (Null)"); }
            }
            //switch off flashing red indicator on top widget UI
            EventManager.i.PostNotification(EventType.StopSecurityFlash, this, null, "CaptureManager.cs -> CapturePlayer");
            //reduce player alpha to show inactive (sprite and text)
            GameManager.i.actorPanelScript.UpdatePlayerAlpha(GameManager.i.guiScript.alphaInactive);
            //player captured outcome window
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
            {
                textTop = text,
                textBottom = builder.ToString(),
                sprite = GameManager.i.spriteScript.capturedSprite,
                isAction = false,
                side = GameManager.i.globalScript.sideResistance,
                type = MsgPipelineType.CapturePlayer,
                help0 = "capture_0",
                help1 = "capture_1",
                help2 = "capture_2",
                help3 = "capture_3"
            };
            if (GameManager.i.guiScript.InfoPipelineAdd(outcomeDetails) == false)
            { Debug.LogWarningFormat("Player Captured infoPipeline message FAILED to be added to dictOfPipeline"); }
            //Sandbox tutorial
            if (GameManager.i.inputScript.GameState == GameState.Tutorial && GameManager.i.tutorialScript.CheckIfSandbox() == true)
            { GameManager.i.tutorialScript.FailSandboxOutcome("They got you. That's a worry"); }
        }
        else
        {
            //AI Resistance Player
            GameManager.i.aiRebelScript.status = ActorStatus.Captured;
            //Remove Gear
            GameManager.i.aiRebelScript.GearPoolEmpty("being CAPTURED");
        }
        //invisible node
        if (GameManager.i.missionScript.mission.npc != null)
        { GameManager.i.missionScript.mission.npc.AddInvisibleNode(details.node.nodeID); }
        //popUpFixed -> don't wait for an outcome Msg, display straight away
        GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, "CAPTURED!");
        GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, "Gear Lost");
        GameManager.i.popUpFixedScript.SetData(PopUpPosition.TopCentre, $"City Loyalty +{actorCaptured}");
        GameManager.i.popUpFixedScript.ExecuteFixed();
        //Human player captured and not autorun
        if (GameManager.i.sideScript.resistanceOverall == SideState.Human && GameManager.i.turnScript.CheckIsAutoRun() == false)
        {
            //end turn
            GameManager.i.turnScript.SetActionsToZero();
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
        GameManager.i.messageScript.ActorCapture(text, details.node, details.team, details.actor.actorID);
        //AutoRun (both sides)
        if (GameManager.i.turnScript.CheckIsAutoRun() == true)
        {
            string textAutoRun = string.Format("{0}{1}{2}, {3}Captured{4}", colourAlert, details.actor.arc.name, colourEnd, colourBad, colourEnd);
            GameManager.i.dataScript.AddHistoryAutoRun(textAutoRun);
        }
        //detention period
        details.actor.captureTimer = captureTimerValue;
        //raise city loyalty
        int cause = GameManager.i.cityScript.CityLoyalty;
        cause += actorCaptured;
        cause = Mathf.Min(GameManager.i.cityScript.maxCityLoyalty, cause);
        GameManager.i.cityScript.CityLoyalty = cause;
        //invisibility set to zero (most likely already is)
        details.actor.SetDatapoint(ActorDatapoint.Invisibility2, 0);
        //history
        details.actor.AddHistory(new HistoryActor() { text = string.Format("Captured at {0}", details.node.nodeName) });
        //update map
        GameManager.i.nodeScript.NodeRedraw = true;
        //update contacts
        GameManager.i.contactScript.UpdateNodeContacts();
        //admin
        GameManager.i.actorScript.numOfActiveActors--;
        details.actor.Status = ActorStatus.Captured;
        details.actor.inactiveStatus = ActorInactive.None;
        details.actor.tooltipStatus = ActorTooltip.Captured;
        details.actor.nodeCaptured = details.node.nodeID;
        details.actor.numOfTimesCaptured++;
        if (GameManager.i.sideScript.resistanceOverall == SideState.Human)
        {
            //effects builder
            StringBuilder builder = new StringBuilder();
            //any carry over text?
            if (string.IsNullOrEmpty(details.effects) == false)
            { builder.Append(string.Format("{0}{1}{2}", details.effects, "\n", "\n")); }
            builder.Append(string.Format("{0}{1} has been Captured{2}{3}{4}", colourBad, details.actor.arc.name, colourEnd, "\n", "\n"));
            builder.AppendFormat("{0}City Loyalty +{1}{2}{3}{4}", colourBad, actorCaptured, colourEnd, "\n", "\n");
            //reduce actor alpha to show inactive (sprite and text)
            GameManager.i.actorPanelScript.UpdateActorAlpha(details.actor.slotID, GameManager.i.guiScript.alphaInactive);
            //popUpFixed -> don't wait for an outcome Msg, display straight away
            GameManager.i.popUpFixedScript.SetData(details.actor.slotID, "CAPTURED!");
            //actor captured outcome window
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
            {
                textTop = text,
                textBottom = builder.ToString(),
                sprite = GameManager.i.spriteScript.errorSprite,
                isAction = false,
                side = GameManager.i.globalScript.sideResistance
            };
            //edge case of Inactive player and Authority AI capture an actor which may (?) generate a message that'll upset the info pipeline. If player active, display at time of action, otherwise put in pipeline
            if (GameManager.i.playerScript.status != ActorStatus.Active)
            { outcomeDetails.type = MsgPipelineType.CaptureActor; }
            EventManager.i.PostNotification(EventType.OutcomeOpen, this, outcomeDetails, "CaptureManager.cs -> CaptureActor");
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
        int nodeID = GameManager.i.nodeScript.nodeCaptured;
        //message
        Node node = GameManager.i.dataScript.GetNode(nodeID);
        if (node != null)
        {
            if (isReleased == true)
            {
                //released
                text = string.Format("{0}, Player, released at \"{1}\", {2}", GameManager.i.playerScript.GetPlayerNameResistance(), node.nodeName, node.Arc.name);
                GameManager.i.messageScript.ActorRelease(text, node, GameManager.i.playerScript.actorID);
            }
            else
            {
                //escapes (can only do so with help of OrgEmergency)
                text = string.Format("{0}, Player, escapes from captivity at \"{1}\", {2}", GameManager.i.playerScript.GetPlayerNameResistance(), node.nodeName, node.Arc.name);
                GameManager.i.messageScript.PlayerEscapes(text, node);
                //history
                GameManager.i.dataScript.AddHistoryPlayer(new HistoryActor() { text = "You ESCAPE from Captivity", district = node.nodeName });
            }
            Debug.LogFormat("[Ply] CaptureManager.cs -> ReleasePlayer: {0}{1}", text, "\n");
            //history
            GameManager.i.dataScript.AddHistoryPlayer(new HistoryActor() { text = "You are RELEASED from Captivity", district = node.nodeName });
            //actor gains condition questionable (do BEFORE updating nodeID's)
            GameManager.i.playerScript.AddCondition(conditionQuestionable, GameManager.i.globalScript.sideResistance, "Has been interrogated by Authority");
            //update nodeID's
            GameManager.i.nodeScript.nodePlayer = nodeID;
            GameManager.i.nodeScript.nodeCaptured = -1;
            //decrease city loyalty
            int cause = GameManager.i.cityScript.CityLoyalty;
            cause -= actorReleased;
            cause = Mathf.Max(0, cause);
            GameManager.i.cityScript.CityLoyalty = cause;
            //zero out capture timer
            GameManager.i.actorScript.captureTimerPlayer = 0;
            //invisibility
            int invisibilityNew = releaseInvisibility;
            GameManager.i.playerScript.Invisibility = invisibilityNew;
            //autorun
            if (GameManager.i.turnScript.CheckIsAutoRun() == true)
            {
                text = string.Format("{0} {1}Released{2}", GameManager.i.playerScript.GetPlayerNameResistance(), colourGood, colourEnd);
                GameManager.i.dataScript.AddHistoryAutoRun(text);
            }
            //human resistance player
            if (GameManager.i.sideScript.resistanceOverall == SideState.Human)
            {
                //reset state
                GameManager.i.playerScript.status = ActorStatus.Active;
                GameManager.i.playerScript.tooltipStatus = ActorTooltip.None;
                builder.AppendFormat("{0}City Loyalty -{1}{2}{3}{4}", colourGood, actorReleased, colourEnd, "\n", "\n");
                builder.AppendFormat("{0}Player's Invisibility +{1}{2}{3}{4}", colourGood, invisibilityNew, colourEnd, "\n", "\n");
                builder.AppendFormat("{0}QUESTIONABLE condition gained{1}", colourBad, colourEnd);
                //AI side tab (otherwise 'player indisposed' message when accessing tab)
                GameManager.i.aiScript.UpdateSideTabData();
                //update Player alpha
                GameManager.i.actorPanelScript.UpdatePlayerAlpha(GameManager.i.guiScript.alphaActive);
                //update map
                GameManager.i.nodeScript.NodeRedraw = true;
                if (isMessage == true)
                {
                    //player released outcome window 
                    ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
                    {
                        textTop = text,
                        textBottom = builder.ToString(),
                        sprite = GameManager.i.spriteScript.errorSprite,
                        isAction = false,
                        side = GameManager.i.globalScript.sideResistance,
                        type = MsgPipelineType.ReleasePlayer
                    };
                    if (GameManager.i.guiScript.InfoPipelineAdd(outcomeDetails) == false)
                    { Debug.LogWarningFormat("Player Released from Captivity infoPipeline message FAILED to be added to dictOfPipeline"); }
                }
                //generate Action Points for the turn (Turn sequence has already reset to Zero due to player being captured)
                GameManager.i.turnScript.SetActionsForNewTurn();
            }
            else
            {
                //AI resistance player
                GameManager.i.aiRebelScript.status = ActorStatus.Active;

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
                Node node = GameManager.i.dataScript.GetNode(actor.nodeCaptured);
                if (node != null)
                {
                    actor.nodeCaptured = -1;
                    //reset actor state
                    actor.Status = ActorStatus.Active;
                    actor.tooltipStatus = ActorTooltip.None;
                    //actor gains condition questionable
                    actor.AddCondition(conditionQuestionable, "has been interrogated by Authority");
                    //decrease City loyalty
                    int cause = GameManager.i.cityScript.CityLoyalty;
                    cause -= actorReleased;
                    cause = Mathf.Max(0, cause);
                    GameManager.i.cityScript.CityLoyalty = cause;
                    builder.AppendFormat("{0}City Loyalty -{1}{2}{3}{4}", colourGood, actorReleased, colourEnd, "\n", "\n");
                    //history
                    actor.AddHistory(new HistoryActor() { text = string.Format("Released at {0}", node.nodeName) });
                    //invisibility
                    int invisibilityNew = releaseInvisibility;
                    actor.SetDatapoint(ActorDatapoint.Invisibility2, invisibilityNew);
                    builder.AppendFormat("{0}{1} Invisibility +{2}{3}", colourGood, actor.actorName, invisibilityNew, colourEnd);
                    //update contacts
                    GameManager.i.contactScript.UpdateNodeContacts();
                    //admin
                    GameManager.i.actorScript.numOfActiveActors++;
                    //traitor
                    int rndNum = Random.Range(0, 100);
                    int chance = actor.numOfTimesCaptured * GameManager.i.actorScript.actorTraitorChance;
                    if (rndNum < chance)
                    {
                        actor.isTraitor = true;
                        GameManager.i.dataScript.StatisticIncrement(StatType.ActorResistanceTraitors);
                        Debug.LogFormat("[Rnd] CaptureManager.cs -> ReleaseActor: {0}, {1}, becomes a TRAITOR (need {2}, rolled {3}){4}", actor.actorName, actor.arc.name, chance, rndNum, "\n");
                    }
                    else { Debug.LogFormat("[Rnd] CaptureManager.cs -> ReleaseActor: {0}, {1}, does NOT become a Traitor (need {2}, rolled {3}){4}", actor.actorName, actor.arc.name, chance, rndNum, "\n"); }
                    //message
                    text = string.Format("{0}, {1}, released from captivity", actor.actorName, actor.arc.name);
                    GameManager.i.messageScript.ActorRelease(text, node, actor.actorID);
                    Debug.LogFormat("[Ply] CaptureManager.cs -> ReleaseActor: {0}{1}", text, "\n");
                    //autorun
                    if (GameManager.i.turnScript.CheckIsAutoRun() == true)
                    {
                        text = string.Format("{0}{1}{2}, {3}Released{4}", colourAlert, actor.arc.name, colourEnd, colourGood, colourEnd);
                        GameManager.i.dataScript.AddHistoryAutoRun(text);
                    }
                    //Human resistance player
                    if (GameManager.i.sideScript.resistanceOverall == SideState.Human)
                    {
                        //update actor alpha
                        GameManager.i.actorPanelScript.UpdateActorAlpha(actor.slotID, GameManager.i.guiScript.alphaActive);

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
        Node node = GameManager.i.dataScript.GetNode(nodeID);
        Team team = null;
        if (node != null)
        {
            //get correct player status depending on who is in charge of Resistance
            ActorStatus status = ActorStatus.Active;
            if (GameManager.i.sideScript.resistanceOverall == SideState.Human)
            { status = GameManager.i.playerScript.status; }
            else { status = GameManager.i.aiRebelScript.status; }
            //correct state
            if (status == ActorStatus.Active)
            {
                //Player
                if (actorID == GameManager.i.playerScript.actorID)
                {
                    //check player at node
                    if (nodeID == GameManager.i.nodeScript.GetPlayerNodeID())
                    {
                        //Erasure team picks up player/actor immediately if invisibility low enough
                        if (CheckCaptureVisibility(GameManager.i.playerScript.Invisibility) == true)
                        {
                            int teamID = node.CheckTeamPresent(teamErasureID);
                            if (teamID > -1)
                            {
                                team = GameManager.i.dataScript.GetTeam(teamID);
                                if (team != null)
                                {
                                    //Player Captured
                                    details = new CaptureDetails { node = node, team = team, actor = null };
                                    Debug.LogFormat("[Ply] CaptureManager.cs -> CheckCaptured: Resistance Player is captured by an Erasure team at {0}, {1}, id {2}{3}",
                                        node.nodeName, node.Arc.name, node.nodeID, "\n");
                                    //history
                                    GameManager.i.dataScript.AddHistoryPlayer(new HistoryActor() { text = "CAPTURED by an Erasure Team", district = node.nodeName });
                                }
                                else { Debug.LogError(string.Format("Invalid team (Null) for teamID {0}", teamID)); }
                            }
                            //Security Alert -> check if an Erasure team is in a neighbouring node
                            if (GameManager.i.turnScript.authoritySecurityState == AuthoritySecurityState.SecurityAlert)
                            {
                                team = CheckCaptureAlert(node);
                                if (team != null)
                                {
                                    //Player Captured
                                    details = new CaptureDetails { node = node, team = team, actor = null };
                                    Debug.LogFormat("[Ply] CaptureManager.cs -> CheckCaptured: Resistance Player is captured by an Erasure team at {0}, {1}, id {2}{3}",
                                        node.nodeName, node.Arc.name, node.nodeID, "\n");
                                    //history
                                    GameManager.i.dataScript.AddHistoryPlayer(new HistoryActor() { text = "CAPTURED by an Erasure Team", district = node.nodeName });
                                }
                            }
                        }
                    }
                    else { Debug.LogError(string.Format("Player not at the nodeID {0}", nodeID)); }
                }
                else
                {
                    //Actor
                    Actor actor = GameManager.i.dataScript.GetActor(actorID);
                    if (actor != null)
                    {
                        //Erasure team picks up player/actor immediately if invisibility 0
                        if (CheckCaptureVisibility(actor.GetDatapoint(ActorDatapoint.Invisibility2)) == true)
                        {
                            int teamID = node.CheckTeamPresent(teamErasureID);
                            if (teamID > -1)
                            {
                                team = GameManager.i.dataScript.GetTeam(teamID);
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
                            if (GameManager.i.turnScript.authoritySecurityState == AuthoritySecurityState.SecurityAlert)
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
        switch(GameManager.i.turnScript.authoritySecurityState)
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
                    team = GameManager.i.dataScript.GetTeam(teamID);
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
        if (GameManager.i.sideScript.resistanceOverall == SideState.Human)
        { status = GameManager.i.playerScript.status; }
        else { status = GameManager.i.aiRebelScript.status; }
        //only check if player active
        if ( status == ActorStatus.Active)
        {
            CaptureDetails details = CheckCaptured(GameManager.i.nodeScript.GetPlayerNodeID());
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
        if (GameManager.i.playerScript.status != ActorStatus.Captured)
        {
            //get player node
            Node node = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.GetPlayerNodeID());
            if (node != null)
            {
                //get default ID 0 team (could be anything)
                Team team = GameManager.i.dataScript.GetTeam(0);
                if (team != null)
                { CapturePlayer(new CaptureDetails() { node = node, team = team }); }
                else { Debug.LogError("Invalid team ID 0 (Null)"); }
            }
            else { Debug.LogErrorFormat("Invalid player node (Null), ID {0}", GameManager.i.nodeScript.GetPlayerNodeID()); }
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
    /// Returns captureTool for the specified ActorHQ, null if not present
    /// </summary>
    /// <param name="actorHQ"></param>
    /// <returns></returns>
    public CaptureTool GetCaptureTool(ActorHQ actorHQ)
    {
        CaptureTool tool = null;
        switch (actorHQ)
        {
            case ActorHQ.Boss: tool = innocence_0; break;
            case ActorHQ.SubBoss1: tool = innocence_1; break;
            case ActorHQ.SubBoss2: tool = innocence_2; break;
            case ActorHQ.SubBoss3: tool = innocence_3; break;
            default: Debug.LogWarningFormat("Unrecognised actorHQ \"{0}\"", actorHQ); break;
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
