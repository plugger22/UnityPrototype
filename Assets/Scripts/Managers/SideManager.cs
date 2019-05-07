using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using gameAPI;
using System.Text;
using modalAPI;

/// <summary>
/// Handles change of Sides, eg. Rebel / Authority, admin
/// </summary>
public class SideManager : MonoBehaviour
{

    //node tooltip
    public Sprite toolTip_backgroundAuthority;
    public Sprite toolTip_backgroundRebel;
    public Sprite toolTip_dividerAuthority;
    public Sprite toolTip_dividerRebel;
    public Sprite outcome_backgroundAuthority;
    public Sprite outcome_backgroundRebel;
    public Sprite picker_background_Authority;
    public Sprite picker_background_Rebel;
    public Sprite inventory_background_Authority;
    public Sprite inventory_background_Resistance;
    public Sprite info_background_Authority;
    public Sprite info_background_Resistance;
    public Sprite header_background_Authority;
    public Sprite header_background_Resistance;
    public Sprite button_Authority;
    public Sprite button_highlight_Authority;
    public Sprite button_Resistance;
    public Sprite button_highlight_Resistance;
    public Sprite button_Click;

    #region save data compatible
    [HideInInspector] public SideState resistanceCurrent;               //who's currently in charge, AI or player?
    [HideInInspector] public SideState authorityCurrent;
    [HideInInspector] public SideState resistanceOverall;                 //who's in charge overall (flows through throughout, constant)
    [HideInInspector] public SideState authorityOverall;
    #endregion

    //fast access
    GlobalSide globalAuthority;
    GlobalSide globalResistance;

    //backing field
    private GlobalSide _playerSide;

    //what side is the player (Human, even if temporarily under AI control for an autorun)
    public GlobalSide PlayerSide
    {
        get { return _playerSide; }
        set
        {
            _playerSide = value;
            switch (value.level)
            {
                case 1:
                    //Authority
                    if (GameManager.instance.optionScript.noAI == false)
                    {
                        resistanceCurrent = SideState.AI;
                        authorityCurrent = SideState.Human;
                    }
                    else
                    {
                        //no AI debug mode
                        resistanceCurrent = SideState.Human;
                        authorityCurrent = SideState.Human;
                    }
                    break;
                case 2:
                    //Resistance
                    if (GameManager.instance.optionScript.noAI == false)
                    {
                        resistanceCurrent = SideState.Human;
                        authorityCurrent = SideState.AI;
                    }
                    else
                    {
                        //no AI debug mode
                        resistanceCurrent = SideState.Human;
                        authorityCurrent = SideState.Human;
                    }
                    break;
                default:
                    Debug.LogError(string.Format("Invalid side.level \"{0}\"", value.level));
                    break;
            }
            //Post notification - Player side has been changed, update colours as well
            EventManager.instance.PostNotification(EventType.ChangeSide, this, _playerSide, "SideManager.cs -> PlayerSide");
            EventManager.instance.PostNotification(EventType.ChangeColour, this, null, "SideManager.cs -> PlayerSide");
            Debug.Log(string.Format("OptionManager -> Player Side now {0}{1}", _playerSide, "\n"));
        }
    }

    public void Initialise()
    {
        //session specific (once only)
        if (GameManager.instance.inputScript.GameState == GameState.NewInitialisation)
        {
            globalAuthority = GameManager.instance.globalScript.sideAuthority;
            globalResistance = GameManager.instance.globalScript.sideResistance;
            Debug.Assert(globalAuthority != null, "Invalid GlobalAuthority (Null)");
            Debug.Assert(globalResistance != null, "Invalid GlobalResistance (Null)");
        }
        /*//if autoRun then bothSidesAI automatically true
        if (GameManager.instance.autoRunTurns > 0) { GameManager.instance.isBothAI = true; } else { GameManager.instance.isBothAI = false; }*/

        //AUTORUN (first scenario in a campaign only)
        if (GameManager.instance.autoRunTurns > 0 && GameManager.instance.campaignScript.CheckIsFirstScenario() == true)
        {
            //isAuthority determines what side will be Player side (for GUI and Messages once auto run finished)
            if (GameManager.instance.isAuthority == true)
            {
                PlayerSide = globalAuthority;
                //reverts to Human authority player
                GameManager.instance.playerScript.SetPlayerNameAuthority(GameManager.instance.preloadScript.nameAuthority);
                GameManager.instance.playerScript.SetPlayerNameResistance(GameManager.instance.scenarioScript.scenario.leaderResistance.leaderName);
            }
            else
            {
                PlayerSide = globalResistance;
                //reverts to Human resistance player
                GameManager.instance.playerScript.SetPlayerNameAuthority(GameManager.instance.scenarioScript.scenario.leaderAuthority.name);
                GameManager.instance.playerScript.SetPlayerNameResistance(GameManager.instance.preloadScript.nameResistance);
            }
            Debug.Log("[Start] Player set to AI for both sides");
            resistanceOverall = SideState.AI;
            authorityOverall = SideState.AI;
            resistanceCurrent = SideState.AI;
            authorityCurrent = SideState.AI;
        }
        else
        {
            // HUMAN Player
            if (GameManager.instance.isAuthority == false)
            {
                //Resistance player
                PlayerSide = GameManager.instance.globalScript.sideResistance;
                Debug.Log("[Start] Player set to RESISTANCE side");
                resistanceOverall = SideState.Human;
                authorityOverall = SideState.AI;
                resistanceCurrent = SideState.Human;
                authorityCurrent = SideState.AI;
                //names
                GameManager.instance.playerScript.SetPlayerNameResistance(GameManager.instance.preloadScript.nameResistance);
                GameManager.instance.playerScript.SetPlayerNameAuthority(GameManager.instance.scenarioScript.scenario.leaderAuthority.name);
            }
            else
            {
                //Authority player
                PlayerSide = globalAuthority;
                Debug.Log("[Start] Player set to AUTHORITY side");
                resistanceOverall = SideState.AI;
                authorityOverall = SideState.Human;
                resistanceCurrent = SideState.AI;
                authorityCurrent = SideState.Human;
                //names
                GameManager.instance.playerScript.SetPlayerNameResistance(GameManager.instance.scenarioScript.scenario.leaderResistance.leaderName);
                GameManager.instance.playerScript.SetPlayerNameAuthority(GameManager.instance.preloadScript.nameAuthority);
            }
        }
    }

    /// <summary>
    /// Returns true if interaction is possible for the current Player side given the overall & noAI settings.
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public bool CheckInteraction()
    {
        bool isPossible = true;
        if (GameManager.instance.optionScript.noAI == false)
        {
            switch(_playerSide.level)
            {
                case 0:
                    //AI both
                    isPossible = false;
                    break;
                case 1:
                    //Authority
                    if (authorityOverall == SideState.AI)
                    { isPossible = false; }
                    break;
                case 2:
                    //Resistance
                    if (resistanceOverall == SideState.AI)
                    { isPossible = false; }
                    break;
                default:
                    Debug.LogError(string.Format("Invalid _playerSide.level \"{0}\"", _playerSide.level));
                    break;
            }
        }
        return isPossible;
    }

    /// <summary>
    /// returns side controlled by AI (opposite to that of the player). If both sides are AI then returns null. Returns null if a problem
    /// </summary>
    /// <returns></returns>
    public GlobalSide GetAISide()
    {
        GlobalSide aiSide = null;
        switch (_playerSide.level)
        {
            case 0:
                //AI
                break;
            case 1:
                //Authority
                aiSide = globalResistance;
                break;
            case 2:
                //Resistance
                aiSide = globalAuthority;
                break;
            default:
                Debug.LogError(string.Format("Invalid _playerSide.level \"{0}\"", _playerSide.level));
                break;
        }
        return aiSide;
    }

    /// <summary>
    /// At the completion of an AI vs AI autorun the specified Player side reverts back to Human Control
    /// </summary>
    public void RevertToHumanPlayer()
    {
        int renown;
        ActorStatus status;
        ActorInactive inactiveStatus;
        float inactiveAlpha = GameManager.instance.guiScript.alphaInactive;
        float activeAlpha = GameManager.instance.guiScript.alphaActive;
        //flashing red alert at top UI for Security Status -> switch on/off
        if (GameManager.instance.turnScript.authoritySecurityState != AuthoritySecurityState.Normal)
        { EventManager.instance.PostNotification(EventType.StartSecurityFlash, this, null, "SideManager.cs -> RevertToHumanPlayer");  }
        else
        { EventManager.instance.PostNotification(EventType.StopSecurityFlash, this, null, "SideManager.cs -> RevertToHumanPlayer");  }
        switch (_playerSide.level)
        {
            case 1:
                //
                // - - - Authority - - -
                //
                authorityOverall = SideState.Human;
                authorityCurrent = SideState.Human;
                resistanceOverall = SideState.AI;
                resistanceCurrent = SideState.AI;
                //convert resources to renown
                renown = GameManager.instance.dataScript.CheckAIResourcePool(globalAuthority);
                Debug.LogFormat("[Aim] SideManager.cs -> RevertToHumanPlayer: Authority has {0} Resources{1}", renown, "\n");
                renown /= GameManager.instance.aiScript.renownFactor;
                GameManager.instance.playerScript.Renown = renown;
                //update states
                status = GameManager.instance.aiScript.status;
                inactiveStatus = GameManager.instance.aiScript.inactiveStatus;
                GameManager.instance.playerScript.status = status;
                GameManager.instance.playerScript.inactiveStatus = inactiveStatus;
                GameManager.instance.playerScript.isBreakdown = GameManager.instance.aiScript.isBreakdown;
                //player
                switch (status)
                {
                    case ActorStatus.Inactive:
                        switch (inactiveStatus)
                        {
                            case ActorInactive.Breakdown:
                                //reduce player alpha to show inactive (sprite and text)
                                GameManager.instance.actorPanelScript.UpdatePlayerAlpha(inactiveAlpha);
                                GameManager.instance.playerScript.tooltipStatus = ActorTooltip.Breakdown;
                                break;
                            case ActorInactive.None:
                                //change actor alpha to show active (sprite and text)
                                GameManager.instance.actorPanelScript.UpdatePlayerAlpha(activeAlpha);
                                GameManager.instance.playerScript.tooltipStatus = ActorTooltip.None;
                                break;
                        }
                        break;
                }
                //loop actors and check for status
                Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalAuthority);
                if (arrayOfActors != null)
                {
                    for (int i = 0; i < arrayOfActors.Length; i++)
                    {
                        //check actor is present in slot (not vacant)
                        if (GameManager.instance.dataScript.CheckActorSlotStatus(i, globalAuthority) == true)
                        {
                            Actor actor = arrayOfActors[i];
                            if (actor != null)
                            {
                                switch (actor.inactiveStatus)
                                {
                                    case ActorInactive.Breakdown:
                                        //change actor alpha to show inactive (sprite and text)
                                        GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.slotID, inactiveAlpha);
                                        actor.tooltipStatus = ActorTooltip.Breakdown;
                                        break;
                                    case ActorInactive.None:
                                        //change actor alpha to show active (sprite and text)
                                        GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.slotID, activeAlpha);
                                        actor.tooltipStatus = ActorTooltip.None;
                                        break;
                                }
                            }
                        }
                    }
                }
                else { Debug.LogError("Invalid arrayOfActors (Null)"); }
                //teams need actors assigned
                GameManager.instance.teamScript.DebugAssignActors();
                Debug.LogFormat("[Ply] SideManager.cs -> RevertToHumanPlayer: Authority side now under HUMAN control{0}", "\n");
                break;
            case 2:
                //
                // - - - Resistance - - -
                //
                resistanceOverall = SideState.Human;
                resistanceCurrent = SideState.Human;
                authorityOverall = SideState.AI;
                authorityCurrent = SideState.AI;
                //convert resources to renown
                renown = GameManager.instance.dataScript.CheckAIResourcePool(globalResistance);
                Debug.LogFormat("[Rim] SideManager.cs -> RevertToHumanPlayer: Resistance has {0} Resources{1}", renown, "\n");
                renown /= GameManager.instance.aiRebelScript.renownFactor;
                GameManager.instance.playerScript.Renown = renown;
                //update states
                status = GameManager.instance.aiRebelScript.status;
                inactiveStatus = GameManager.instance.aiRebelScript.inactiveStatus;
                GameManager.instance.playerScript.status = status;
                GameManager.instance.playerScript.inactiveStatus = inactiveStatus;
                GameManager.instance.playerScript.isBreakdown = GameManager.instance.aiScript.isBreakdown;
                switch (status)
                {
                    case ActorStatus.Captured:
                        GameManager.instance.playerScript.tooltipStatus = ActorTooltip.Captured;
                        //reduce player alpha to show inactive (sprite and text)
                        GameManager.instance.actorPanelScript.UpdatePlayerAlpha(inactiveAlpha);
                        break;
                    case ActorStatus.Inactive:
                        //reduce player alpha to show inactive (sprite and text)
                        GameManager.instance.actorPanelScript.UpdatePlayerAlpha(inactiveAlpha);
                        switch (inactiveStatus)
                        {
                            case ActorInactive.Breakdown:
                                GameManager.instance.playerScript.tooltipStatus = ActorTooltip.Breakdown;
                                break;
                            case ActorInactive.LieLow:
                                GameManager.instance.playerScript.tooltipStatus = ActorTooltip.LieLow;
                                break;
                            case ActorInactive.None:
                                //change actor alpha to show active (sprite and text)
                                GameManager.instance.actorPanelScript.UpdatePlayerAlpha(activeAlpha);
                                GameManager.instance.playerScript.tooltipStatus = ActorTooltip.None;
                                break;
                        }
                        break;
                }
                //loop actors and check for status
                arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
                if (arrayOfActors != null)
                {
                    for (int i = 0; i < arrayOfActors.Length; i++)
                    {
                        //check actor is present in slot (not vacant)
                        if (GameManager.instance.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
                        {
                            Actor actor = arrayOfActors[i];
                            if (actor != null)
                            {
                                //update
                                switch (actor.Status)
                                {
                                    case ActorStatus.Active:

                                        // - - - Compatibility (Edit: cause duplicate msg)
                                        // - - - Invisibility Zero warning (Edit: causes duplicate msg)

                                        //
                                        // - - - Motivation Warning - - -
                                        //
                                        if (actor.datapoint1 == 0)
                                        { GameManager.instance.actorScript.ProcessMotivationWarning(actor); }
                                        break;
                                    case ActorStatus.Captured:
                                        //change actor alpha to show inactive (sprite and text)
                                        GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.slotID, inactiveAlpha);
                                        actor.tooltipStatus = ActorTooltip.Captured;
                                        break;
                                    case ActorStatus.Inactive:
                                        switch (actor.inactiveStatus)
                                        {
                                            case ActorInactive.Breakdown:
                                                //change actor alpha to show inactive (sprite and text)
                                                GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.slotID, inactiveAlpha);
                                                actor.tooltipStatus = ActorTooltip.Breakdown;
                                                break;
                                            case ActorInactive.LieLow:
                                                GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.slotID, inactiveAlpha);
                                                actor.tooltipStatus = ActorTooltip.LieLow;
                                                break;
                                            case ActorInactive.None:
                                                //change actor alpha to show active (sprite and text)
                                                GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.slotID, activeAlpha);
                                                actor.tooltipStatus = ActorTooltip.None;
                                                break;
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
                else { Debug.LogError("Invalid arrayOfActors (Null)"); }
                //Gear
                int gearUsed = GameManager.instance.aiRebelScript.GetGearUsedAdjusted();
                int gearPoints = GameManager.instance.aiRebelScript.GetGearPoints();
                if (gearUsed > 0)
                {
                    //delete gear from common and rare pools to reflect gear that's been used
                    GameManager.instance.dataScript.UpdateGearLostOnRevert(gearUsed);
                }
                if (gearPoints > 0)
                {
                    //add gear to reflect gear that is currently in use
                    GameManager.instance.dataScript.UpdateGearCurrentOnRevert(gearPoints);
                }
                Debug.LogFormat("[Ply] SideManager.cs -> RevertToHumanPlayer: Resistance side now under HUMAN control{0}", "\n");
                break;
            default:
                Debug.LogError(string.Format("Invalid _playerSide.level \"{0}\"", _playerSide.level));
                break;
        }
        //
        // - - - Events of note occur during the AutoRun
        //
        //only if nobody has yet won
        if (GameManager.instance.turnScript.winStateLevel == WinState.None)
        {
            List<string> listOfEvents = GameManager.instance.dataScript.GetListOfHistoryAutoRun();
            if (listOfEvents != null)
            {
                if (listOfEvents.Count > 0)
                {
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < listOfEvents.Count; i++)
                    { builder.AppendLine(listOfEvents[i]); }
                    //create an outcome window to notify player
                    ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
                    outcomeDetails.side = _playerSide;
                    outcomeDetails.textTop = "AutoRun complete";
                    outcomeDetails.textBottom = builder.ToString();
                    EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "SideManager.cs -> RevertToHumanPlayer");
                }
            }
            else { Debug.LogError("Invalid listOfHistoryAutoRun (Null)"); }
        }
    }

    //add new methods above here
}
