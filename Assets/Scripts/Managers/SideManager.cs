using gameAPI;
using modalAPI;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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
                    if (GameManager.i.optionScript.noAI == false)
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
                    if (GameManager.i.optionScript.noAI == false)
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
            EventManager.i.PostNotification(EventType.ChangeSide, this, _playerSide, "SideManager.cs -> PlayerSide");
            EventManager.i.PostNotification(EventType.ChangeColour, this, null, "SideManager.cs -> PlayerSide");
            Debug.Log(string.Format("OptionManager -> Player Side now {0}{1}", _playerSide, "\n"));
        }
    }

    /// <summary>
    /// Note called for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                break;
            case GameState.FollowOnInitialisation:
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }

    #region InitialiseSubMethods

    #region SubInitialiseFastAccess
    /// <summary>
    /// subMethod to for fast access cached fields
    /// </summary>
    private void SubInitialiseFastAccess()
    {
        globalAuthority = GameManager.i.globalScript.sideAuthority;
        globalResistance = GameManager.i.globalScript.sideResistance;
        Debug.Assert(globalAuthority != null, "Invalid GlobalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid GlobalResistance (Null)");
    }
    #endregion

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        Campaign campaign = GameManager.i.campaignScript.campaign;
        if (campaign != null)
        {
            //AUTORUN (first scenario in a campaign only)
            if (GameManager.i.autoRunTurns > 0 && GameManager.i.campaignScript.CheckIsFirstScenario() == true)
            {
                //Campaign determines what side will be Player side (for GUI and Messages once auto run finished)
                switch (campaign.side.level)
                {
                    case 1:
                        //Authority
                        PlayerSide = globalAuthority;
                        //reverts to Human authority player
                        GameManager.i.playerScript.SetPlayerNameAuthority(GameManager.i.preloadScript.nameAuthority);
                        GameManager.i.playerScript.SetPlayerNameResistance(GameManager.i.campaignScript.scenario.leaderResistance.leaderName);
                        break;
                    case 2:
                        //Resistance
                        PlayerSide = globalResistance;
                        //reverts to Human resistance player
                        GameManager.i.playerScript.SetPlayerNameAuthority(GameManager.i.campaignScript.scenario.leaderAuthority.name);
                        GameManager.i.playerScript.SetPlayerNameResistance(GameManager.i.preloadScript.nameResistance);
                        break;
                    default:
                        Debug.LogWarningFormat("Unrecognised campaign side \"{0}\"", campaign.side.name);
                        break;
                }
                Debug.Log("[Start] Player set to AI for both sides");
                resistanceOverall = SideState.AI;
                authorityOverall = SideState.AI;
                resistanceCurrent = SideState.AI;
                authorityCurrent = SideState.AI;
            }
            else
            {
                //HUMAN Player
                switch (campaign.side.level)
                {
                    case 1:
                        //Authority player
                        PlayerSide = globalAuthority;
                        Debug.Log("[Start] Player set to AUTHORITY side");
                        resistanceOverall = SideState.AI;
                        authorityOverall = SideState.Human;
                        resistanceCurrent = SideState.AI;
                        authorityCurrent = SideState.Human;
                        //names
                        GameManager.i.playerScript.SetPlayerNameResistance(GameManager.i.campaignScript.scenario.leaderResistance.leaderName);
                        GameManager.i.playerScript.SetPlayerNameAuthority(GameManager.i.preloadScript.nameAuthority);
                        break;
                    case 2:
                        //Resistance player
                        PlayerSide = GameManager.i.globalScript.sideResistance;
                        Debug.Log("[Start] Player set to RESISTANCE side");
                        resistanceOverall = SideState.Human;
                        authorityOverall = SideState.AI;
                        resistanceCurrent = SideState.Human;
                        authorityCurrent = SideState.AI;
                        //names
                        GameManager.i.playerScript.SetPlayerNameResistance(GameManager.i.preloadScript.nameResistance);
                        GameManager.i.playerScript.SetPlayerNameAuthority(GameManager.i.campaignScript.scenario.leaderAuthority.mayorName);
                        break;
                    default:
                        Debug.LogWarningFormat("Unrecognised campaign side \"{0}\"", campaign.side.name);
                        break;
                }
            }
            //set first name
            GameManager.i.playerScript.SetPlayerFirstName(GameManager.i.preloadScript.nameFirst);
        }
        else { Debug.LogError("Invalid campaign (Null)"); }
    }
    #endregion

    #endregion

    /// <summary>
    /// Returns true if interaction is possible for the current Player side (human) given the overall & noAI settings.
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public bool CheckInteraction()
    {
        bool isPossible = true;
        if (GameManager.i.optionScript.noAI == false)
        {
            switch (_playerSide.level)
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
        int power;
        ActorStatus status;
        ActorInactive inactiveStatus;
        float inactiveAlpha = GameManager.i.guiScript.alphaInactive;
        float activeAlpha = GameManager.i.guiScript.alphaActive;
        //flashing red alert at top UI for Security Status -> switch on/off
        if (GameManager.i.turnScript.authoritySecurityState != AuthoritySecurityState.Normal)
        { EventManager.i.PostNotification(EventType.StartSecurityFlash, this, null, "SideManager.cs -> RevertToHumanPlayer"); }
        else
        { EventManager.i.PostNotification(EventType.StopSecurityFlash, this, null, "SideManager.cs -> RevertToHumanPlayer"); }
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
                //convert resources to power
                power = GameManager.i.dataScript.CheckAIResourcePool(globalAuthority);
                Debug.LogFormat("[Aim] SideManager.cs -> RevertToHumanPlayer: Authority has {0} Resources{1}", power, "\n");
                power /= GameManager.i.aiScript.powerFactor;
                GameManager.i.playerScript.Power = power;
                //update states
                status = GameManager.i.aiScript.status;
                inactiveStatus = GameManager.i.aiScript.inactiveStatus;
                GameManager.i.playerScript.status = status;
                GameManager.i.playerScript.inactiveStatus = inactiveStatus;
                GameManager.i.playerScript.isBreakdown = GameManager.i.aiScript.isBreakdown;
                //player
                switch (status)
                {
                    case ActorStatus.Inactive:
                        switch (inactiveStatus)
                        {
                            case ActorInactive.Breakdown:
                                //reduce player alpha to show inactive (sprite and text)
                                GameManager.i.actorPanelScript.UpdatePlayerAlpha(inactiveAlpha);
                                GameManager.i.playerScript.tooltipStatus = ActorTooltip.Breakdown;
                                break;
                            case ActorInactive.None:
                                //change actor alpha to show active (sprite and text)
                                GameManager.i.actorPanelScript.UpdatePlayerAlpha(activeAlpha);
                                GameManager.i.playerScript.tooltipStatus = ActorTooltip.None;
                                break;
                        }
                        break;
                }
                //clear out debug NodeActionData records for Player
                GameManager.i.playerScript.ClearAllNodeActions();
                //loop actors and check for status
                Actor[] arrayOfActors = GameManager.i.dataScript.GetCurrentActors(globalAuthority);
                if (arrayOfActors != null)
                {
                    for (int i = 0; i < arrayOfActors.Length; i++)
                    {
                        //check actor is present in slot (not vacant)
                        if (GameManager.i.dataScript.CheckActorSlotStatus(i, globalAuthority) == true)
                        {
                            Actor actor = arrayOfActors[i];
                            if (actor != null)
                            {
                                switch (actor.inactiveStatus)
                                {
                                    case ActorInactive.Breakdown:
                                        //change actor alpha to show inactive (sprite and text)
                                        GameManager.i.actorPanelScript.UpdateActorAlpha(actor.slotID, inactiveAlpha);
                                        actor.tooltipStatus = ActorTooltip.Breakdown;
                                        break;
                                    case ActorInactive.None:
                                        //change actor alpha to show active (sprite and text)
                                        GameManager.i.actorPanelScript.UpdateActorAlpha(actor.slotID, activeAlpha);
                                        actor.tooltipStatus = ActorTooltip.None;
                                        break;
                                }
                            }
                        }
                    }
                }
                else { Debug.LogError("Invalid arrayOfActors (Null)"); }
                //teams need actors assigned
                GameManager.i.teamScript.AutoRunAssignActors();
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
                GameManager.i.nemesisScript.SetResistancePlayer(SideState.Human);
                //convert resources to power
                power = GameManager.i.dataScript.CheckAIResourcePool(globalResistance);
                Debug.LogFormat("[Rim] SideManager.cs -> RevertToHumanPlayer: Resistance has {0} Resources{1}", power, "\n");
                power /= GameManager.i.aiRebelScript.powerFactor;
                GameManager.i.playerScript.Power = power;
                //update states
                status = GameManager.i.aiRebelScript.status;
                inactiveStatus = GameManager.i.aiRebelScript.inactiveStatus;
                GameManager.i.playerScript.status = status;
                GameManager.i.playerScript.inactiveStatus = inactiveStatus;
                GameManager.i.playerScript.isBreakdown = GameManager.i.aiScript.isBreakdown;
                //testManager.cs Invisibility
                if (GameManager.i.testScript.playerInvisibility > -1)
                { GameManager.i.playerScript.Invisibility = GameManager.i.testScript.playerInvisibility; }
                //player
                switch (status)
                {
                    case ActorStatus.Captured:
                        GameManager.i.playerScript.tooltipStatus = ActorTooltip.Captured;
                        //reduce player alpha to show inactive (sprite and text)
                        GameManager.i.actorPanelScript.UpdatePlayerAlpha(inactiveAlpha);
                        break;
                    case ActorStatus.Inactive:
                        //reduce player alpha to show inactive (sprite and text)
                        GameManager.i.actorPanelScript.UpdatePlayerAlpha(inactiveAlpha);
                        switch (inactiveStatus)
                        {
                            case ActorInactive.Breakdown:
                                GameManager.i.playerScript.tooltipStatus = ActorTooltip.Breakdown;
                                break;
                            case ActorInactive.LieLow:
                                GameManager.i.playerScript.tooltipStatus = ActorTooltip.LieLow;
                                break;
                            case ActorInactive.None:
                                //change actor alpha to show active (sprite and text)
                                GameManager.i.actorPanelScript.UpdatePlayerAlpha(activeAlpha);
                                GameManager.i.playerScript.tooltipStatus = ActorTooltip.None;
                                break;
                        }
                        break;
                }
                //clear out debug NodeActionData records for Player
                GameManager.i.playerScript.ClearAllNodeActions();
                //loop actors and check for status
                arrayOfActors = GameManager.i.dataScript.GetCurrentActors(globalResistance);
                if (arrayOfActors != null)
                {
                    for (int i = 0; i < arrayOfActors.Length; i++)
                    {
                        //check actor is present in slot (not vacant)
                        if (GameManager.i.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
                        {
                            Actor actor = arrayOfActors[i];
                            if (actor != null)
                            {
                                //clear out debug NodeActionData records
                                actor.RemoveAllNodeActions();
                                //update
                                switch (actor.Status)
                                {
                                    case ActorStatus.Active:

                                        // - - - Compatibility (Edit: cause duplicate msg)
                                        // - - - Invisibility Zero warning (Edit: causes duplicate msg)

                                        /*//
                                        // - - - Motivation Warning - - -  EDIT -> Duplicate message
                                        //
                                        if (actor.GetDatapoint(ActorDatapoint.Motivation1) == 0)
                                        { GameManager.instance.actorScript.ProcessMotivationWarning(actor); }*/

                                        break;
                                    case ActorStatus.Captured:
                                        //change actor alpha to show inactive (sprite and text)
                                        GameManager.i.actorPanelScript.UpdateActorAlpha(actor.slotID, inactiveAlpha);
                                        actor.tooltipStatus = ActorTooltip.Captured;
                                        break;
                                    case ActorStatus.Inactive:
                                        switch (actor.inactiveStatus)
                                        {
                                            case ActorInactive.Breakdown:
                                                //change actor alpha to show inactive (sprite and text)
                                                GameManager.i.actorPanelScript.UpdateActorAlpha(actor.slotID, inactiveAlpha);
                                                actor.tooltipStatus = ActorTooltip.Breakdown;
                                                break;
                                            case ActorInactive.LieLow:
                                                GameManager.i.actorPanelScript.UpdateActorAlpha(actor.slotID, inactiveAlpha);
                                                actor.tooltipStatus = ActorTooltip.LieLow;
                                                break;
                                            case ActorInactive.None:
                                                //change actor alpha to show active (sprite and text)
                                                GameManager.i.actorPanelScript.UpdateActorAlpha(actor.slotID, activeAlpha);
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
                
                //DEBUG -> add false (but more accurate) NodeAction records for testing purposes
                GameManager.i.actorScript.DebugCreateNodeActionResistanceData();

                //Gear
                int gearUsed = GameManager.i.aiRebelScript.GetGearUsedAdjusted();
                int gearPoints = 0;
                if (GameManager.i.testScript.numOfGearItems == -1)
                {
                    //normal gear point allocation according to AI gear pool
                    gearPoints = GameManager.i.aiRebelScript.GetGearPoints();
                    /*Debug.LogFormat("[Tst] SideManager.cs -> RevertToHumanPlayer: Rebel AIManager Gear points set to {0}{1}", gearPoints, "\n");*/
                }
                else
                {
                    //Test Manager specified gear points
                    gearPoints = GameManager.i.testScript.numOfGearItems;
                    /*Debug.LogFormat("[Tst] SideManager.cs -> RevertToHumanPlayer: TestManager Gear points set to {0}{1}", gearPoints, "\n");*/
                }
                if (gearUsed > 0)
                {
                    //delete gear from common and rare pools to reflect gear that's been used
                    GameManager.i.dataScript.UpdateGearLostOnRevert(gearUsed);
                }
                if (gearPoints > 0)
                {
                    //add gear to reflect gear that is currently in use
                    GameManager.i.dataScript.UpdateGearCurrentOnRevert(gearPoints);
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
        ShowAutoRunMessage();
    }

    /// <summary>
    /// display autoRun history message in outcome window
    /// </summary>
    public void ShowAutoRunMessage()
    {
        //only if nobody has yet won
        if (GameManager.i.turnScript.winStateLevel == WinStateLevel.None)
        {
            List<string> listOfEvents = GameManager.i.dataScript.GetListOfHistoryAutoRun();
            if (listOfEvents != null)
            {
                StringBuilder builder = new StringBuilder();
                if (listOfEvents.Count > 0)
                {
                    for (int i = 0; i < listOfEvents.Count; i++)
                    { builder.AppendLine(listOfEvents[i]); }
                }
                else { builder.AppendLine("No Events"); }
                //create an outcome window to notify player
                ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
                outcomeDetails.side = _playerSide;
                outcomeDetails.textTop = "AutoRun complete";
                outcomeDetails.textBottom = builder.ToString();
                EventManager.i.PostNotification(EventType.OutcomeOpen, this, outcomeDetails, "SideManager.cs -> RevertToHumanPlayer");
            }
            else { Debug.LogError("Invalid listOfHistoryAutoRun (Null)"); }
        }
    }

    //add new methods above here
}
