using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using gameAPI;


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

    [HideInInspector] public SideState resistanceCurrent;               //who's currently in charge, AI or player?
    [HideInInspector] public SideState authorityCurrent;
    [HideInInspector] public SideState resistanceOverall;                 //who's in charge overall (flows through throughout, constant)
    [HideInInspector] public SideState authorityOverall;

    //fast access
    GlobalSide globalAuthority;
    GlobalSide globalResistance;

    //backing field
    private GlobalSide _playerSide;

    //what side is the player
    public GlobalSide PlayerSide
    {
        get { return _playerSide; }
        set
        {
            _playerSide = value;
            switch (value.level)
            {
                case 0:
                    //AI both
                    resistanceCurrent = SideState.AI;
                    authorityCurrent = SideState.AI;
                    break;
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
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
        Debug.Assert(globalAuthority != null, "Invalid GlobalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid GlobalResistance (Null)");
        //both sides AI
        if (GameManager.instance.isBothAI == true)
        {
            if (GameManager.instance.autoRunTurns > 0)
            {
                //isAuthority determines what side will be Player side (for GUI and Messages once auto run finished)
                if (GameManager.instance.isAuthority == true)
                { PlayerSide = globalAuthority; }
                else { PlayerSide = globalResistance; }
                Debug.Log("[Start] Player set to AI for both sides");
                resistanceOverall = SideState.AI;
                authorityOverall = SideState.AI;
                resistanceCurrent = SideState.AI;
                authorityCurrent = SideState.AI;
            }
            else
            { Debug.LogError("AutoRunTurns must be > Zero for isBothAI to be true"); }
        }
        else
        {
            //One side is a HUMAN Player
            if (GameManager.instance.isAuthority == false)
            {
                //Resistance player
                PlayerSide = GameManager.instance.globalScript.sideResistance;
                Debug.Log("[Start] Player set to RESISTANCE side");
                resistanceOverall = SideState.Human;
                authorityOverall = SideState.AI;
                resistanceCurrent = SideState.Human;
                authorityCurrent = SideState.AI;
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
        switch (_playerSide.level)
        {
            case 1:
                //Authority
                authorityOverall = SideState.Human;
                authorityCurrent = SideState.Human;
                resistanceOverall = SideState.AI;
                resistanceCurrent = SideState.AI;
                //convert resources to renown
                GameManager.instance.playerScript.Renown = GameManager.instance.dataScript.CheckAIResourcePool(globalAuthority);
                //teams need actors assigned
                GameManager.instance.teamScript.DebugAssignActors();
                Debug.LogFormat("[Ply] SideManager.cs -> RevertToHumanPlayer: Authority side now under HUMAN control{0}", "\n");
                break;
            case 2:
                //Resistance
                resistanceOverall = SideState.Human;
                resistanceCurrent = SideState.Human;
                authorityOverall = SideState.AI;
                authorityCurrent = SideState.AI;
                //convert resources to renown
                GameManager.instance.playerScript.Renown = GameManager.instance.dataScript.CheckAIResourcePool(globalResistance);
                //update states
                ActorStatus aiRebelStatus = GameManager.instance.aiRebelScript.status;
                ActorInactive aiRebelInactive = GameManager.instance.aiRebelScript.inactiveStatus;
                GameManager.instance.playerScript.status = aiRebelStatus;
                switch(aiRebelStatus)
                {
                    case ActorStatus.Captured:
                        GameManager.instance.playerScript.tooltipStatus = ActorTooltip.Captured;
                        ///switch off flashing red indicator on top widget UI
                        EventManager.instance.PostNotification(EventType.StopSecurityFlash, this, null, "CaptureManager.cs -> CapturePlayer");
                        //reduce player alpha to show inactive (sprite and text)
                        GameManager.instance.actorPanelScript.UpdatePlayerAlpha(GameManager.instance.guiScript.alphaInactive);
                        break;
                    case ActorStatus.Inactive:
                        switch (aiRebelInactive)
                        {
                            case ActorInactive.Breakdown:
                                GameManager.instance.playerScript.tooltipStatus = ActorTooltip.Breakdown;
                                break;
                            case ActorInactive.LieLow:
                                GameManager.instance.playerScript.tooltipStatus = ActorTooltip.LieLow;
                                break;
                        }
                        break;
                }
                Debug.LogFormat("[Ply] SideManager.cs -> RevertToHumanPlayer: Resistance side now under HUMAN control{0}", "\n");
                break;
            default:
                Debug.LogError(string.Format("Invalid _playerSide.level \"{0}\"", _playerSide.level));
                break;
        }
    }

}
