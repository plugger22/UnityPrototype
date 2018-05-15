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
    /*[Tooltip("The increase to city loyalty due to the Player being Captured")]
    [Range(0, 2)] public int playerCaptured = 1;*/
    [Tooltip("The increase to city loyalty due to a Resistance Actor, or Player, being Captured")]
    [Range(0, 2)] public int actorCaptured = 1;
    [Tooltip("The decrease to city loyalty due to an Actor being Released")]
    [Range(0, 2)] public int actorReleased = 1;

    [Tooltip("The value of the Actor's invisibility upon Release")]
    [Range(0, 3)] public int releaseInvisibility = 2;

    private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourEnd;


    public void Initialise()
    {
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "CaptureManager");
        EventManager.instance.AddListener(EventType.Capture, OnEvent, "CaptureManager");
        EventManager.instance.AddListener(EventType.ReleasePlayer, OnEvent, "CaptureManager");
        EventManager.instance.AddListener(EventType.ReleaseActor, OnEvent, "CaptureManager");
        EventManager.instance.AddListener(EventType.StartTurnEarly, OnEvent, "CaptureManager");
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
            case EventType.Capture:
                CaptureDetails details = Param as CaptureDetails;
                CaptureSomebody(details);
                break;
            case EventType.ReleasePlayer:
                ReleasePlayer();
                break;
            case EventType.ReleaseActor:
                CaptureDetails detailsRelease = Param as CaptureDetails;
                ReleaseActor(detailsRelease);
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
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
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
    private void CapturePlayer(CaptureDetails details)
    {
        //PLAYER CAPTURED
        string text = string.Format("Player Captured at \"{0}\", {1}", details.node.nodeName, details.node.Arc.name);
        //effects builder
        StringBuilder builder = new StringBuilder();
        //any carry over text?
        if (string.IsNullOrEmpty(details.effects) == false)
        { builder.Append(string.Format("{0}{1}{2}", details.effects, "\n", "\n")); }
        builder.Append(string.Format("{0}Player has been Captured{1}{2}{3}", colourBad, colourEnd, "\n", "\n"));
        //message
        Message message = GameManager.instance.messageScript.AICapture(text, details.node.nodeID, details.team.teamID);
        GameManager.instance.dataScript.AddMessage(message);
        //update node trackers
        GameManager.instance.nodeScript.nodePlayer = -1;
        GameManager.instance.nodeScript.nodeCaptured = details.node.nodeID;
        //change player state
        /*GameManager.instance.turnScript.resistanceState = ResistanceState.Captured;*/
        GameManager.instance.playerScript.status = ActorStatus.Captured;
        //add renown to authority actor who owns the team (only if they are still OnMap
        if (GameManager.instance.sideScript.authorityOverall == SideState.Player)
        {
            if (GameManager.instance.dataScript.CheckActorSlotStatus(details.team.actorSlotID, GameManager.instance.globalScript.sideAuthority) == true)
            {
                Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.team.actorSlotID, GameManager.instance.globalScript.sideAuthority);
                if (actor != null)
                { actor.renown++; }
                else { Debug.LogError(string.Format("Invalid actor (null) from team.ActorSlotID {0}", details.team.actorSlotID)); }
            }
        }
        //Raise city loyalty
        int cause = GameManager.instance.cityScript.CityLoyalty;
        cause += actorCaptured;
        cause = Mathf.Min(GameManager.instance.cityScript.maxCityLoyalty, cause);
        GameManager.instance.cityScript.CityLoyalty = cause;

        builder.AppendFormat("{0}City Loyalty +{1}{2}{3}{4}", colourBad, actorCaptured, colourEnd, "\n", "\n");
        //Gear confiscated
        int numOfGear = GameManager.instance.playerScript.CheckNumOfGear();
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
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
        {
            textTop = text,
            textBottom = builder.ToString(),
            sprite = GameManager.instance.guiScript.capturedSprite,
            isAction = false,
            side = GameManager.instance.globalScript.sideResistance
        };
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
    }

    /// <summary>
    /// Resistance Actor captured
    /// NOTE: node, team and actor checked for null by parent method
    /// </summary>
    /// <param name="node"></param>
    /// <param name="team"></param>
    /// <param name="actor"></param>
    private void CaptureActor(CaptureDetails details)
    {
        //effects builder
        StringBuilder builder = new StringBuilder();
        //any carry over text?
        if (string.IsNullOrEmpty(details.effects) == false)
        { builder.Append(string.Format("{0}{1}{2}", details.effects, "\n", "\n")); }
        string text = string.Format("Rebel {0} Captured at \"{1}\", {2}", details.actor.actorName, details.node.nodeName, details.node.Arc.name);
        builder.Append(string.Format("{0}{1} has been Captured{2}{3}{4}", colourBad, details.actor.arc.name, colourEnd, "\n", "\n"));
        //message
        Message message = GameManager.instance.messageScript.AICapture(text, details.node.nodeID, details.team.teamID, details.actor.actorID);
        GameManager.instance.dataScript.AddMessage(message);

        //raise city loyalty
        int cause = GameManager.instance.cityScript.CityLoyalty;
        cause += actorCaptured;
        cause = Mathf.Min(GameManager.instance.cityScript.maxCityLoyalty, cause);
        GameManager.instance.cityScript.CityLoyalty = cause;

        builder.AppendFormat("{0}City Loyalty +{1}{2}{3}{4}", colourBad, actorCaptured, colourEnd, "\n", "\n");
        //invisibility set to zero (most likely already is)
        details.actor.datapoint2 = 0;
        //update map
        GameManager.instance.nodeScript.NodeRedraw = true;
        //reduce actor alpha to show inactive (sprite and text)
        GameManager.instance.guiScript.UpdateActorAlpha(details.actor.actorSlotID, GameManager.instance.guiScript.alphaInactive);
        //admin
        GameManager.instance.actorScript.numOfActiveActors--;
        details.actor.Status = ActorStatus.Captured;
        details.actor.nodeCaptured = details.node.nodeID;
        //actor captured outcome window
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
        {
            textTop = text,
            textBottom = builder.ToString(),
            sprite = GameManager.instance.guiScript.errorSprite,
            isAction = false,
            side = GameManager.instance.globalScript.sideResistance
        };
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
    }

    /// <summary>
    /// Release player from captitivity
    /// </summary>
    public void ReleasePlayer()
    {
        StringBuilder builder = new StringBuilder();
        //update nodes
        int nodeID = GameManager.instance.nodeScript.nodeCaptured;
        GameManager.instance.nodeScript.nodePlayer = nodeID;
        GameManager.instance.nodeScript.nodeCaptured = -1;
        //reset state
        /*GameManager.instance.turnScript.resistanceState = ResistanceState.Normal;*/
        GameManager.instance.playerScript.status = ActorStatus.Active;

        //decrease city loyalty
        int cause = GameManager.instance.cityScript.CityLoyalty;
        cause -= actorReleased;
        cause = Mathf.Max(0, cause);
        GameManager.instance.cityScript.CityLoyalty = cause;

        builder.AppendFormat("{0}City Loyalty -{1}{2}{3}{4}", colourGood, actorReleased, colourEnd, "\n", "\n");
        //invisibility
        int invisibilityNew = releaseInvisibility;
        GameManager.instance.playerScript.invisibility = invisibilityNew;
        builder.AppendFormat("{0}Player's Invisibility +{1}{2}", colourGood, invisibilityNew, colourEnd);
        //update map
        GameManager.instance.nodeScript.NodeRedraw = true;
        //message
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        if (node != null)
        {
            string text = string.Format("Player released at \"{0}\", {1}", node.nodeName, node.Arc.name);
            Message message = GameManager.instance.messageScript.AIRelease(text, nodeID, 999);
            GameManager.instance.dataScript.AddMessage(message);
            //player released outcome window
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
            {
                textTop = text,
                textBottom = builder.ToString(),
                sprite = GameManager.instance.guiScript.errorSprite,
                isAction = false,
                side = GameManager.instance.globalScript.sideResistance
            };
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
        }
        else { Debug.LogError(string.Format("Invalid node (Null) for nodeId {0}", nodeID)); }
    }

    /// <summary>
    /// Release actor from captitivty, only Actor is needed from CaptureDetails
    /// </summary>
    /// <param name="actorID"></param>
    public void ReleaseActor(CaptureDetails details)
    {
        if (details.actor != null)
        {
            if (details.actor.Status == ActorStatus.Captured)
            {
                StringBuilder builder = new StringBuilder();
                //node (needed only for record keeping / messaging purposes
                int nodeID = details.actor.nodeCaptured;
                details.actor.nodeCaptured = -1;
                //reset actor state
                details.actor.Status = ActorStatus.Active;
                //decrease City loyalty
                int cause = GameManager.instance.cityScript.CityLoyalty;
                cause -= actorReleased;
                cause = Mathf.Max(0, cause);
                GameManager.instance.cityScript.CityLoyalty = cause;

                builder.AppendFormat("{0}City Loyalty -{1}{2}{3}{4}", colourGood, actorReleased, colourEnd, "\n", "\n");
                //invisibility
                int invisibilityNew = releaseInvisibility;
                details.actor.datapoint2 = invisibilityNew;
                builder.AppendFormat("{0}{1} Invisibility +{2}{3}", colourGood, details.actor.actorName, invisibilityNew, colourEnd);
                //update actor alpha
                GameManager.instance.guiScript.UpdateActorAlpha(details.actor.actorSlotID, GameManager.instance.guiScript.alphaActive);
                /*GameManager.instance.nodeScript.NodeRedraw = true;*/
                //admin
                GameManager.instance.actorScript.numOfActiveActors++;
                //message
                string text = string.Format("{0} released from captivity", details.actor.actorName);
                Message message = GameManager.instance.messageScript.AIRelease(text, nodeID, details.actor.actorID);
                GameManager.instance.dataScript.AddMessage(message);
                //player released outcome window
                ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
                {
                    textTop = text,
                    textBottom = builder.ToString(),
                    sprite = GameManager.instance.guiScript.errorSprite,
                    isAction = false,
                    side = GameManager.instance.globalScript.sideResistance
                };
                EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
            }
            else { Debug.LogWarningFormat("{0}, {1} can't be released as not presently captured", details.actor.arc.name, details.actor.actorName); }
        }
        else { Debug.LogError("Invalid details.actor (Null)"); }
    }


    /// <summary>
    /// Checks if Resistance player/actor captured by an Erasure team at the node (must have invisibility '0'). Returns null if not.
    /// parameters vary if an 'APB' or a 'SecurityAlert' in play
    /// ActorID is default '999' for player
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public CaptureDetails CheckCaptured(int nodeID, int actorID = 999)
    {
        CaptureDetails details = null;
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        if (node != null)
        {
            //correct state
            if (GameManager.instance.playerScript.status == ActorStatus.Active)
            {
                //Player
                if (actorID == 999)
                {
                    //check player at node
                    if (nodeID == GameManager.instance.nodeScript.nodePlayer)
                    {
                        //Erasure team picks up player/actor immediately if invisibility low enough
                        if (CheckCaptureVisibility(GameManager.instance.playerScript.invisibility) == true)
                        /*if (GameManager.instance.playerScript.invisibility == 0)*/
                        {
                            int teamArcID = GameManager.instance.dataScript.GetTeamArcID("ERASURE");
                            if (teamArcID > -1)
                            {
                                int teamID = node.CheckTeamPresent(teamArcID);
                                if (teamID > -1)
                                {
                                    Team team = GameManager.instance.dataScript.GetTeam(teamID);
                                    if (team != null)
                                    {
                                        //Player Captured
                                        details = new CaptureDetails
                                        {
                                            node = node,
                                            team = team,
                                            actor = null
                                        };
                                    }
                                    else { Debug.LogError(string.Format("Invalid team (Null) for teamID {0}", teamID)); }
                                }
                            }
                            /*else { Debug.LogError("Invalid teamArcID (-1) for ERASURE team"); }*/
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
                        if (CheckCaptureVisibility(actor.datapoint2) == true)
                        /*if (actor.datapoint2 == 0)*/
                        {
                            int teamArcID = GameManager.instance.dataScript.GetTeamArcID("ERASURE");
                            if (teamArcID > -1)
                            {
                                int teamID = node.CheckTeamPresent(teamArcID);
                                if (teamID > -1)
                                {
                                    Team team = GameManager.instance.dataScript.GetTeam(teamID);
                                    if (team != null)
                                    {
                                        //Actor Captured
                                        details = new CaptureDetails
                                        {
                                            node = node,
                                            team = team,
                                            actor = actor
                                        };
                                    }
                                    else { Debug.LogError(string.Format("Invalid team (Null) for teamID {0}", teamID)); }
                                }
                            }
                            /*else { Debug.LogError("Invalid teamArcID (-1) for ERASURE team"); }*/
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
    /// subMethod for check capture that takes into account any APB's etc. and returns true if invisibility is low enough for a capture attempt
    /// </summary>
    /// <param name="actorInvisibility"></param>
    /// <returns></returns>
    private bool CheckCaptureVisibility(int actorInvisibility)
    {
        bool isAtRisk = false;
        switch(GameManager.instance.turnScript.authorityState)
        {
            case AuthorityState.APB:
                if (actorInvisibility <= 1) { isAtRisk = true; }
                break;
            default:
                if (actorInvisibility == 0) { isAtRisk = true; }
                break;
        }
        return isAtRisk;
    }

    /// <summary>
    /// Checks player at start of turn (early) to see if invisibility is zero and Erasure team present
    /// </summary>
    private void CheckStartTurnCapture()
    {
        CaptureDetails details = CheckCaptured(GameManager.instance.nodeScript.nodePlayer);
        if (details != null)
        {
            //Player captured
            details.effects = string.Format("{0}They kicked in the door before you could get out of bed{1}", colourNeutral, colourEnd);
            CapturePlayer(details);
        }
    }


}
