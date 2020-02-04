﻿using gameAPI;
using packageAPI;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// handles all faction related matters for both sides
/// </summary>
public class FactionManager : MonoBehaviour
{
    [Header("Faction Data")]
    [Tooltip("Approval for both sides factions range from 0 to this amount")]
    [Range(0, 10)] public int maxFactionApproval = 10;
    [Tooltip("How much Renown will the faction give per turn if they decide to support the Player")]
    [Range(1, 3)] public int renownPerTurn = 1;
    [Tooltip("How many actors in the HQ line up. Needs to correspond with enum.ActorHQ. Determines size of DataManager.cs -> arrayOfActorsHQ")]
    [Range(1, 6)] public int numOfActorsHQ = 4;
    [Tooltip("Multiplier for HQ hierarchy Actors renown (Boss gets highest renown, subBoss's progressively less) forumla renown = (numOfActorsHQ + 2 - enum.StatusHQ) * renownFactor")]
    [Range(1, 20)] public int renownFactor = 10;

    [Header("Actor Influence")]
    [Tooltip("Amount Faction Approval drops by whenever an Actor resigns for whatever reason")]
    [Range(0, 3)] public int factionApprovalActorResigns = 1;

    [Header("Faction Matters")]
    [Tooltip("Timer set when faction approval is first zero. Decrements each turn and when zero the Player is fired. Reset if approval rises above zero")]
    [Range(1, 10)] public int factionFirePlayerTimer = 3;

    [Header("HQ Relocation")]
    [Tooltip("Number of turns needed for HQ to successfully relocate (HQ services unavialable during relocation")]
    [Range(1, 10)] public int timerRelocationBase = 5;

    [HideInInspector] public Faction factionAuthority;
    [HideInInspector] public Faction factionResistance;

    #region SaveDataCompatible
    private int bossOpinion;                                //opinion of HQ boss (changes depending on topic decisions made, if in alignment with Boss's view or not)
    private int approvalZeroTimer;                          //countdown timer once approval at zero. Player fired when timer reaches zero.
    private int timerHqRelocating;                          //activated when relocation occurs, counts down to zero when relocation is considered done
    [HideInInspector] public bool isHqRelocating;           //true if HQ relocating. Disallows HQ support + Gear/Recruit/Lie Low actions
    #endregion

    private bool isZeroTimerThisTurn;                       //only the first zero timer event per turn is processed
    private int _approvalAuthority;                         //level of faction approval (out of 10) enjoyed by authority side (Player/AI)
    private int _approvalResistance;                        //level of faction approval (out of 10) enjoyed by resistance side (Player/AI)

    //fast access
    private GlobalSide globalAuthority;
    private GlobalSide globalResistance;

    /*private string colourRebel;
    private string colourAuthority;*/
    private string colourNeutral;
    private string colourNormal;
    private string colourGood;
    private string colourBad;
    private string colourGrey;
    /*private string colourAlert;*/
    /*private string colourSide;*/
    private string colourEnd;

    /// <summary>
    /// use ChangeFactionApproval to set
    /// </summary>
    public int ApprovalAuthority
    {
        get { return _approvalAuthority; }
        private set
        {
            _approvalAuthority = value;
            _approvalAuthority = Mathf.Clamp(_approvalAuthority, 0, maxFactionApproval);
            //update top widget bar if current side is authority
            if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
            { EventManager.instance.PostNotification(EventType.ChangeFactionBar, this, _approvalAuthority, "FactionManager.cs -> ApprovalAuthority"); }
        }
    }

    /// <summary>
    /// use ChangeFactionApproval to set
    /// </summary>
    public int ApprovalResistance
    {
        get { return _approvalResistance; }
        private set
        {
            _approvalResistance = value;
            _approvalResistance = Mathf.Clamp(_approvalResistance, 0, maxFactionApproval);
            //update top widget bar if current side is resistance
            if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
            { EventManager.instance.PostNotification(EventType.ChangeFactionBar, this, _approvalResistance, "FactionManager.cs -> ApprovalResistance"); }
        }
    }

    /// <summary>
    /// Not for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseNewGame();
                SubInitialiseFastAccess();
                SubInitialiseColours();
                SubInitialiseEvents();
                SubInitialiseAll();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseAll();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseColours();
                SubInitialiseEvents();
                SubInitialiseAll();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseNewGame
    private void SubInitialiseNewGame()
    {
        //initialise HQ actors starting lineUp
        GameManager.instance.actorScript.InitialiseHQActors();
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //fast access
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
    }
    #endregion

    #region SubInitialiseColours
    private void SubInitialiseColours()
    { SetColours(); }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "FactionManager");
        EventManager.instance.AddListener(EventType.StartTurnEarly, OnEvent, "FactionManager");
        EventManager.instance.AddListener(EventType.EndTurnLate, OnEvent, "FactionManager");
    }
    #endregion

    #region SubInitialiseAll
    private void SubInitialiseAll()
    {
        //Authority faction 
        factionAuthority = GameManager.instance.dataScript.GetFaction(GameManager.instance.globalScript.sideAuthority);
        if (factionAuthority == null)
        { Debug.LogError("Invalid factionAuthority (Null)"); }
        //Resistance faction
        factionResistance = GameManager.instance.dataScript.GetFaction(GameManager.instance.globalScript.sideResistance);
        if (factionResistance == null)
        { Debug.LogError("Invalid factionResistance (Null)"); }
        //approval levels (if input approval is Zero then generate a random value between 2 & 8)
        int approval = GameManager.instance.campaignScript.scenario.approvalStartAuthorityHQ;
        if (approval == 0) { approval = Random.Range(2, 9); }
        ApprovalAuthority = approval;
        approval = GameManager.instance.campaignScript.scenario.approvalStartRebelHQ;
        if (approval == 0)
        { approval = Random.Range(2, 9); }
        ApprovalResistance = approval;
        Debug.LogFormat("[Fac] FactionManager -> Initialise: {0}, approval {1}, {2}, approval {3}{4}",
            factionResistance, ApprovalResistance, factionAuthority, ApprovalAuthority, "\n");

    }
    #endregion

    #endregion

    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //select event type
        switch (eventType)
        {
            case EventType.EndTurnLate:
                EndTurnLate();
                break;
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.StartTurnEarly:
                CheckHQRenownSupport();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralText);
        /*colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.badText);
        colourRebel = GameManager.instance.colourScript.GetColour(ColourType.blueText);*/
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        //colourAlert = GameManager.instance.colourScript.GetColour(ColourType.salmonText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
        /*if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; }
        else { colourSide = colourRebel; }*/
    }

    /// <summary>
    /// End turn late event
    /// </summary>
    private void EndTurnLate()
    {
        //reset flag ready for next turn
        isZeroTimerThisTurn = false;
        //HQ relocating
        if (timerHqRelocating > 0)
        {
            string text;
            timerHqRelocating--;
            if (timerHqRelocating <= 0)
            {
                //relocation complete
                isHqRelocating = false;
                timerHqRelocating = 0;
                text = "HQ has successfully Relocated";
                GameManager.instance.messageScript.HqRelocates(text, "Relocation Successful");
                Debug.LogFormat("[Fac] FactionManager.cs -> EndTurnLate: HQ has successfully Relocated{0}", "\n");
            }
            else
            {
                //relocation ongoing - effects tab
                text = string.Format("<b>HQ is relocating ({0}{1}{2} turn{3} to go</b>)", colourNeutral, timerHqRelocating, colourEnd, timerHqRelocating != 1 ? "s" : "");
                ActiveEffectData dataEffect = new ActiveEffectData()
                {
                    text = "HQ is Relocating",
                    topText = "Relocation Underway",
                    detailsTop = text,
                    detailsBottom = string.Format("{0}<b>HQ Services are temporarily Unavailable</b>{1}", colourBad, colourEnd),
                    sprite = GetCurrentFaction().sprite,
                    help0 = "hq_0",
                    help1 = "hq_1",
                    help2 = "hq_2"
                };
                GameManager.instance.messageScript.ActiveEffect(dataEffect);
                Debug.LogFormat("[Fac] FactionManager.cs -> EndTurnLate: {0}{1}", text, "\n");
            }
        }
    }

    /// <summary>
    /// checks if player given support (+1 renown) from their HQ based on a random roll vs. level of HQ approval. No support if player inactive.
    /// </summary>
    private void CheckHQRenownSupport()
    {
        bool isProceed = true;
        //ignore if autorun with both sides AI
        if (GameManager.instance.turnScript.CheckIsAutoRun() == true && GameManager.instance.autoRunTurns > 0)
        { isProceed = false; }
        //ignore if player is inactive
        if (GameManager.instance.playerScript.status == ActorStatus.Inactive)
        {
            isProceed = false;
            Debug.LogFormat("[Fac] FactionManager.cs -> CheckHQRenownSupport: NO support as Player is Inactive ({0}){1}", GameManager.instance.playerScript.inactiveStatus, "\n");
        }
        //HQ relocating
        if (isHqRelocating == true)
        {
            isProceed = false;
            Faction faction = GetCurrentFaction();
            if (faction != null)
            {
                Debug.LogFormat("Fac] FactionManager.cs -> CheckHQRenownSupport: NO support as HQ Relocating{0}", "\n");
                string text = string.Format("HQ support unavailable as HQ is currently Relocating{0}", "\n");
                string reason = GameManager.instance.colourScript.GetFormattedString(string.Format("<b>{0} is currently Relocating</b>", faction.tag), ColourType.salmonText);
                GameManager.instance.messageScript.HqSupportUnavailable(text, reason, faction);
            }
            else { Debug.LogWarning("Invalid current faction (Null)"); }
        }
        if (isProceed == true)
        {
            int side = GameManager.instance.sideScript.PlayerSide.level;
            if (side > 0)
            {
                int rnd = Random.Range(0, 100);
                int threshold;
                switch (side)
                {
                    case 1:
                        //Authority
                        threshold = _approvalAuthority * 10;
                        if (rnd < threshold)
                        {
                            //Support Provided
                            Debug.LogFormat("[Rnd] FactionManager.cs -> CheckHQRenownSupport: GIVEN need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                            string msgText = string.Format("{0} provides SUPPORT (+1 Renown)", factionAuthority.tag);
                            GameManager.instance.messageScript.HqSupport(msgText, factionAuthority, _approvalAuthority, GameManager.instance.playerScript.Renown, renownPerTurn);
                            //random
                            GameManager.instance.messageScript.GeneralRandom("HQ support GIVEN", "HQ Support", threshold, rnd);
                            //Support given
                            GameManager.instance.playerScript.Renown += renownPerTurn;
                        }
                        else
                        {
                            //Support declined
                            Debug.LogFormat("[Rnd] FactionManager.cs -> CheckHQRenownSupport: DECLINED need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                            string msgText = string.Format("{0} declines support ({1} % chance of support)", factionAuthority.tag, threshold);
                            GameManager.instance.messageScript.HqSupport(msgText, factionAuthority, _approvalAuthority, GameManager.instance.playerScript.Renown);
                            //random
                            GameManager.instance.messageScript.GeneralRandom("HQ support DECLINED", "HQ Support", threshold, rnd);
                        }
                        break;
                    case 2:
                        //Resistance
                        threshold = _approvalResistance * 10;
                        if (rnd < threshold)
                        {
                            //Support Provided
                            Debug.LogFormat("[Rnd] FactionManager.cs -> CheckHQRenownSupport: GIVEN need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                            string msgText = string.Format("{0} provides SUPPORT (+1 Renown)", factionResistance.tag);
                            GameManager.instance.messageScript.HqSupport(msgText, factionResistance, _approvalResistance, GameManager.instance.playerScript.Renown, renownPerTurn);
                            //random
                            GameManager.instance.messageScript.GeneralRandom("HQ support GIVEN", "HQ Support", threshold, rnd, false, "rand_1");
                            //Support given
                            GameManager.instance.playerScript.Renown += renownPerTurn;
                        }
                        else
                        {
                            //Support declined
                            Debug.LogFormat("[Rnd] FactionManager.cs -> CheckHQRenownSupport: DECLINED need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                            string msgText = string.Format("{0} declines support ({1} % chance of support)", factionResistance.tag, threshold);
                            GameManager.instance.messageScript.HqSupport(msgText, factionResistance, _approvalResistance, GameManager.instance.playerScript.Renown);
                            //random
                            GameManager.instance.messageScript.GeneralRandom("HQ support DECLINED", "HQ Support", threshold, rnd, false, "rand_1");
                        }
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid side \"{0}\"", side);
                        break;
                }
            }
        }
    }

    public int GetApprovalZeroTimer()
    { return approvalZeroTimer; }

    /// <summary>
    /// Used by Save/Load to set data
    /// </summary>
    /// <param name="timer"></param>
    public void LoadApprovalZeroTimer(int timer)
    { approvalZeroTimer = timer; }

    /// <summary>
    /// Checks if approval zero, sets timer, counts down each turn and fires player when timer expired. Timer cancelled if approval rises
    /// </summary>
    public void CheckFactionFirePlayer()
    {
        string msgText, itemText, reason, warning;
        //get approval
        int approval = -1;
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        Faction playerFaction = null;
        switch (side.level)
        {
            case 1:
                //Authority
                approval = _approvalAuthority;
                playerFaction = factionAuthority;
                break;
            case 2:
                //Resistance
                approval = _approvalResistance;
                playerFaction = factionResistance;
                break;
            default:
                Debug.LogWarningFormat("Invalid side \"{0}\"", side);
                break;
        }
        Debug.Assert(playerFaction != null, "Invalid playerFaction (Null)");
        //only check once per turn
        if (approval == 0 && isZeroTimerThisTurn == false)
        {
            isZeroTimerThisTurn = true;
            if (approvalZeroTimer == 0)
            {
                //set timer
                approvalZeroTimer = factionFirePlayerTimer;
                //message
                msgText = string.Format("Faction approval Zero. Faction will FIRE you in {0} turn{1}", approvalZeroTimer, approvalZeroTimer != 1 ? "s" : "");
                itemText = string.Format("{0} faction approval at ZERO", playerFaction.name);
                reason = string.Format("{0} faction have lost faith in you", playerFaction.name);
                warning = string.Format("You will be FIRED in {0} turn{1}", approvalZeroTimer, approvalZeroTimer != 1 ? "s" : "");
                GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "Faction Approval", reason, warning);
            }
            else
            {
                //decrement timer
                approvalZeroTimer--;
                //fire player at zero
                if (approvalZeroTimer == 0)
                {
                    WinStateLevel winState = WinStateLevel.Authority;
                    //you lost, opposite side won
                    if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
                    {
                        //Resistance side wins
                        winState = WinStateLevel.Resistance;
                    }
                    msgText = string.Format("{0} approval Zero. Player Fired. Authority wins", playerFaction.tag);
                    itemText = string.Format("{0} has LOST PATIENCE", playerFaction.tag);
                    reason = string.Format("{0} approval at ZERO for an extended period", playerFaction.tag);
                    warning = "You've been FIRED, game over";
                    GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "You're FIRED", reason, warning);
                    //Player fired -> outcome
                    string textTop = string.Format("{0}The {1} has lost faith in your abilities{2}", colourNormal, GetFactionName(side), colourEnd);
                    string textBottom = string.Format("{0}You've been FIRED{1}", colourBad, colourEnd);
                    GameManager.instance.turnScript.SetWinStateLevel(winState, WinReasonLevel.FactionSupportMin, textTop, textBottom);

                }
                else
                {
                    //message
                    msgText = string.Format("HQ approval Zero. HQ will FIRE you in {0} turn{1}", approvalZeroTimer, approvalZeroTimer != 1 ? "s" : "");
                    itemText = string.Format("{0} about to FIRE you", playerFaction.tag);
                    reason = string.Format("{0} are displeased with your performance", playerFaction.tag);
                    warning = string.Format("You will be FIRED in {0} turn{1}", approvalZeroTimer, approvalZeroTimer != 1 ? "s" : "");
                    GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "HQ Unhappy", reason, warning);
                }
            }
        }
        else
        {
            //timer set to default (approval > 0)
            approvalZeroTimer = 0;
        }
    }

    /// <summary>
    /// returns current faction for player side, Null if not found
    /// </summary>
    /// <returns></returns>
    public Faction GetCurrentFaction()
    {
        Faction faction = null;
        switch (GameManager.instance.sideScript.PlayerSide.name)
        {
            case "Authority":
                faction = factionAuthority;
                break;
            case "Resistance":
                faction = factionResistance;
                break;
            default:
                Debug.LogError(string.Format("Invalid player side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                break;
        }
        return faction;
    }

    /// <summary>
    /// returns current faction for the specified side in a colour formatted string. 'isOversized' gives name at 115% (default), false for normal.
    /// </summary>
    /// <returns></returns>
    public string GetFactionName(GlobalSide side, bool isOversized = true)
    {
        string description = "Unknown";
        if (side != null)
        {
            if (isOversized == true)
            {
                //oversized text
                switch (side.level)
                {
                    case 1:
                        description = string.Format("<b><size=115%>{0}</size></b>", factionAuthority.tag);
                        break;
                    case 2:
                        description = string.Format("<b><size=115%>{0}</size></b>", factionResistance.tag);
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid player side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                        break;
                }
            }
            else
            {
                //normal sized text
                switch (side.level)
                {
                    case 1:
                        description = string.Format("<b>{0}</b>", factionAuthority.name);
                        break;
                    case 2:
                        description = string.Format("<b>{0}</b>", factionResistance.name);
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid player side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                        break;
                }
            }
        }
        else { Debug.LogWarning("Invalid side (Null)"); }
        return description;
    }

    /// <summary>
    /// returns current faction description for specified side in colour formatted string
    /// </summary>
    /// <returns></returns>
    public string GetFactionDescription(GlobalSide side)
    {
        string description = "Unknown";
        if (side != null)
        {
            switch (side.level)
            {
                case 1:
                    description = string.Format("{0}{1}{2}", colourNeutral, factionAuthority.descriptor, colourEnd);
                    break;
                case 2:
                    description = string.Format("{0}{1}{2}", colourNeutral, factionResistance.descriptor, colourEnd);
                    break;
                default:
                    Debug.LogError(string.Format("Invalid player side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                    break;
            }
        }
        else { Debug.LogWarning("Invalid side (Null)"); }
        return description;
    }

    /// <summary>
    /// returns faction approval level for current side, returns -1 if a problem
    /// </summary>
    /// <returns></returns>
    public int GetFactionApproval()
    {
        int approval = -1;
        switch (GameManager.instance.sideScript.PlayerSide.level)
        {
            case 1: approval = _approvalAuthority; break;
            case 2: approval = _approvalResistance; break;
            default: Debug.LogWarningFormat("Unrecognised playerSide.level \"{0}\"", GameManager.instance.sideScript.PlayerSide.level); break;
        }
        return approval;
    }

    /// <summary>
    /// returns current faction approval level for specified side in colour formatted string
    /// </summary>
    /// <returns></returns>
    public string GetFactionApprovalLevel(GlobalSide side)
    {
        string description = "Unknown";
        if (side != null)
        {
            switch (side.level)
            {
                case 1:
                    description = string.Format("{0}{1}{2} out of {3}", colourNeutral, _approvalAuthority, colourEnd, maxFactionApproval);
                    break;
                case 2:
                    description = string.Format("{0}{1}{2} out of {3}", colourNeutral, _approvalResistance, colourEnd, maxFactionApproval);
                    break;
                default:
                    Debug.LogError(string.Format("Invalid player side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                    break;
            }
        }
        else { Debug.LogWarning("Invalid side (Null)"); }
        return description;
    }

    /// <summary>
    /// returns current faction details for specified side in colour formatted string. 'Unknown' if an problem
    /// </summary>
    /// <returns></returns>
    public string GetFactionDetails(GlobalSide side)
    {
        string colourNode = colourGrey;
        StringBuilder builder = new StringBuilder();
        if (side != null)
        {
            switch (side.level)
            {
                case 1:
                    builder.AppendFormat("Faction details: {0}To be Done{1}", colourNode, colourEnd);
                    /*arc = factionAuthority.preferredArc;
                    if (arc != null) { colourNode = colourGood; }
                    else { colourNode = colourGrey; }
                    builder.AppendFormat("Preferred Nodes {0}{1}{2}{3}", colourNode, arc != null ? arc.name : "None", colourEnd, "\n");
                    arc = factionAuthority.hostileArc;
                    if (arc != null) { colourNode = colourBad; }
                    else { colourNode = colourGrey; }
                    builder.AppendFormat("Hostile Nodes {0}{1}{2}{3}", colourNode, arc != null ? arc.name : "None", colourEnd, "\n");
                    builder.AppendFormat("{0}{1}{2}{3} Actions per turn{4}", colourNeutral, factionAuthority.actionsTaskPerTurn, colourEnd, colourNormal, colourEnd);*/
                    break;
                case 2:
                    builder.AppendFormat("Faction details: {0}To be Done{1}", colourNode, colourEnd);
                    /*arc = factionResistance.preferredArc;
                    if (arc != null) { colourNode = colourGood; }
                    else { colourNode = colourGrey; }
                    builder.AppendFormat("Preferred Nodes {0}{1}{2}{3}", colourNode, arc != null ? arc.name : "None", colourEnd, "\n");
                    arc = factionResistance.hostileArc;
                    if (arc != null) { colourNode = colourBad; }
                    else { colourNode = colourGrey; }
                    builder.AppendFormat("Hostile Nodes {0}{1}{2}{3}", colourNode, arc != null ? arc.name : "None", colourEnd, "\n");
                    builder.AppendFormat("{0}{1}{2}{3} Actions per turn{4}", colourNeutral, factionResistance.actionsTaskPerTurn, colourEnd, colourNormal, colourEnd);*/
                    break;
                default:
                    Debug.LogError(string.Format("Invalid player side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                    break;
            }
        }
        else { Debug.LogWarning("Invalid side (Null)"); }
        if (builder.Length == 0)
        { builder.Append("Unknown"); }
        return builder.ToString();
    }



    /// <summary>
    /// use this to adjust faction approval level (auto checks for various faction mechanics and generates a message) 'Reason' is self contained. Amount to change should be negative to lower Approval
    /// </summary>
    /// <param name="amount"></param>
    public void ChangeFactionApproval(int amountToChange, GlobalSide side, string reason)
    {
        if (side != null)
        {
            int oldApproval;
            string msgText;
            if (string.IsNullOrEmpty(reason) == true) { reason = "Unknown"; }
            switch (side.level)
            {
                case 1:
                    //Authority
                    oldApproval = ApprovalAuthority;
                    ApprovalAuthority += amountToChange;
                    Debug.LogFormat("[Fac] <b>Authority</b> Faction Approval: change {0}{1} now {2} ({3}){4}", amountToChange > 0 ? "+" : "", amountToChange, ApprovalAuthority, reason, "\n");
                    if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level)
                    {
                        msgText = string.Format("{0} faction approval {1}{2}", factionAuthority.name, amountToChange > 0 ? "+" : "", amountToChange);
                        GameManager.instance.messageScript.HqApproval(msgText, reason, factionAuthority, oldApproval, amountToChange, ApprovalAuthority);
                    }
                    break;
                case 2:
                    //Resistance
                    oldApproval = ApprovalResistance;
                    ApprovalResistance += amountToChange;
                    Debug.LogFormat("[Fac] Resistance Faction Approval: change {0}{1} now {2} ({3}){4}", amountToChange > 0 ? "+" : "", amountToChange, ApprovalResistance, reason, "\n");
                    if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level)
                    {
                        msgText = string.Format("{0} faction approval {1}{2}", factionResistance.name, amountToChange > 0 ? "+" : "", amountToChange);
                        GameManager.instance.messageScript.HqApproval(msgText, reason, factionResistance, oldApproval, amountToChange, ApprovalResistance);
                    }
                    break;
                default:
                    Debug.LogWarningFormat("Invalid PlayerSide \"{0}\"", side);
                    break;
            }
        }
        else { Debug.LogError("Invalid side (Null)"); }
    }

    /// <summary>
    /// Set faction approval levels directly when loading saved game data (use ChangeFactionApproval otherwise)
    /// </summary>
    /// <param name="side"></param>
    /// <param name="approval"></param>
    public void LoadSetFactionApproval(GlobalSide side, int approval)
    {
        if (side != null)
        {
            Debug.AssertFormat(approval > -1 && approval <= 10, "Invalid approval \"{0}\"");
            switch (side.level)
            {
                case 1: ApprovalAuthority = approval; break;
                case 2: ApprovalResistance = approval; break;
                default: Debug.LogWarningFormat("Unrecognised {0}", side); break;
            }
        }
        else { Debug.LogError("Invalid side (Null)"); }
    }


    //
    // - - - Boss Opinion
    //

    public int GetBossOpinion()
    { return bossOpinion; }

    /// <summary>
    /// change boss opinion to new value, generate message, reason is '[due to] [reason]'
    /// </summary>
    /// <param name="newValue"></param>
    /// <param name="reason"></param>
    public void SetBossOpinion(int newValue, string reason)
    {
        int oldValue = bossOpinion;
        bossOpinion = newValue;
        Debug.LogFormat("[Fac] FactionManager.cs -> SetBossOpinion: Opinion now {0} (was {1}), due to {2}{3}", newValue, oldValue, reason, "\n");
    }

    /// <summary>
    /// returns a colour formatted string showing boss's current opinion represented as a text, eg. "Poor" (red)
    /// </summary>
    /// <returns></returns>
    public string GetBossOpinionFormatted()
    {
        string opinion = "Unknown";
        if (bossOpinion >= 4) { opinion = string.Format("{0}Very Good{1}", colourGood, colourEnd); }
        else if (bossOpinion <= -4) { opinion = string.Format("{0}Very Poor{1}", colourBad, colourEnd); }
        else
        {
            switch (bossOpinion)
            {
                case 3:
                case 2: opinion = string.Format("{0}Good{1}", colourGood, colourEnd); break;
                case 1:
                case 0:
                case -1: opinion = string.Format("{0}Neutral{1}", colourNeutral, colourEnd); break;
                case -2:
                case -3: opinion = string.Format("{0}Poor{1}", colourBad, colourEnd); break;
                default: Debug.LogWarningFormat("Unrecognised bossOpinion \"{0}\"", bossOpinion); break;
            }
        }
        return opinion;
    }

    /// <summary>
    /// returns a random ranking HQ position
    /// </summary>
    /// <returns></returns>
    public ActorHQ GetRandomHQPosition()
    {
        List<ActorHQ> listOfHQPositions = new List<ActorHQ>() { ActorHQ.Boss, ActorHQ.SubBoss1, ActorHQ.SubBoss2, ActorHQ.SubBoss3 };
        return listOfHQPositions[Random.Range(0, listOfHQPositions.Count)];
    }

    /// <summary>
    /// HQ relocating. Takes a number of turns. Services unavailable while relocating. Handles all admin. Time to relocate increases with every instance
    /// </summary>
    /// <param name="reason"></param>
    public void RelocateHQ(string reason = "Unknown")
    {
        //stats (do first as needed for calcs below) 
        GameManager.instance.dataScript.StatisticIncrement(StatType.HQRelocations);
        //use max value of level or campaign (as campaign data isn't updated until metagame)
        int timesRelocated = Mathf.Max(GameManager.instance.dataScript.StatisticGetLevel(StatType.HQRelocations), GameManager.instance.dataScript.StatisticGetCampaign(StatType.HQRelocations));
        isHqRelocating = true;
        timerHqRelocating = timerRelocationBase * timesRelocated;
        //messages
        Debug.LogFormat("[Fac] FactionManager.cs -> RelocateHQ: HQ relocates due to {0}{1}", reason, "\n");
        string text = string.Format("HQ is relocating due to {0} (will take {1} turns)", reason, timerHqRelocating);
        GameManager.instance.messageScript.HqRelocates(text, reason);
    }


    public int GetHqRelocationTimer()
    { return timerHqRelocating; }

    public void SetHqRelocationTimer(int timer)
    { timerHqRelocating = timer; }

    /// <summary>
    /// Metagame resetting of HQ data
    /// </summary>
    public void ProcessMetaHQ()
    {
        isHqRelocating = false;
        timerHqRelocating = 0;       
    }

    //
    // - - - Debug - - -
    //

    /// <summary>
    /// Debug method to display faction info
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayFactions()
    {
        StringBuilder builder = new StringBuilder();
        //authority
        builder.AppendFormat(" AUTHORITY{0}", "\n");
        builder.AppendFormat(" {0} faction{1}", factionAuthority.name, "\n");
        builder.AppendFormat(" {0}{1}{2}", factionAuthority.descriptor, "\n", "\n");
        /*builder.AppendFormat(" Preferred Nodes: {0}{1}", factionAuthority.preferredArc != null ? factionAuthority.preferredArc.name : "None", "\n");
        builder.AppendFormat(" Hostile Nodes: {0}{1}", factionAuthority.hostileArc != null ? factionAuthority.hostileArc.name : "None", "\n", "\n");*/
        builder.AppendFormat(" AI Resource Pool: {0}{1}", GameManager.instance.dataScript.CheckAIResourcePool(GameManager.instance.globalScript.sideAuthority), "\n");
        builder.AppendFormat(" AI Resource Allowance: {0}{1}{2}", GameManager.instance.aiScript.resourcesGainAuthority, "\n", "\n");
        //resistance
        builder.AppendFormat("{0} RESISTANCE{1}", "\n", "\n");
        builder.AppendFormat(" {0} faction{1}", factionResistance.name, "\n");
        builder.AppendFormat(" {0}{1}{2}", factionResistance.descriptor, "\n", "\n");
        /*builder.AppendFormat(" Preferred Nodes: {0}{1}", factionResistance.preferredArc != null ? factionResistance.preferredArc.name : "None", "\n");
        builder.AppendFormat(" Hostile Nodes: {0}{1}", factionResistance.hostileArc != null ? factionResistance.hostileArc.name : "None", "\n", "\n");*/
        builder.AppendFormat(" AI Resource Pool: {0}{1}", GameManager.instance.dataScript.CheckAIResourcePool(GameManager.instance.globalScript.sideResistance), "\n");
        builder.AppendFormat(" AI Resource Allowance: {0}{1}{2}", GameManager.instance.aiScript.resourcesGainResistance, "\n", "\n");
        //HQ data
        builder.AppendFormat("{0}- HQ Hierarchy{1}", "\n", "\n");
        builder.AppendFormat(" bossOpinion {0}{1}, {2}{3}", bossOpinion > 0 ? "+" : "", bossOpinion, GetBossOpinionFormatted(), "\n");
        //Relocation
        builder.AppendFormat("{0}- HQ Relocation{1}", "\n", "\n");
        builder.AppendFormat(" isHqRelocating: {0}{1}", isHqRelocating, "\n");
        builder.AppendFormat(" timerHqRelocating: {0}", timerHqRelocating);
        return builder.ToString();
    }

    /// <summary>
    /// Debug method to display HQ Actors
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayHQActors()
    {
        StringBuilder builder = new StringBuilder();
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetArrayOfActorsHQ();
        if (arrayOfActors != null)
        {
            builder.AppendFormat("-HQ Hierarchy{0}", "\n");
            //first and last indexes are blanks ('None', 'Workers', 'LeftHQ')
            for (int i = 1; i < (int)ActorHQ.Count - 2; i++)
            {
                Actor actor = arrayOfActors[i];
                if (actor != null)
                {
                    builder.AppendFormat("{0}- {1}{2}", "\n", actor.statusHQ, "\n");
                    builder.AppendFormat(" {0}, {1}, ID {2}, Mot {3}, Comp {4}, R {5}{6}", actor.actorName, actor.arc.name, actor.actorID, actor.GetDatapoint(ActorDatapoint.Motivation1),
                        actor.GetPersonality().GetCompatibilityWithPlayer(), actor.Renown, "\n");
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for arrayOfActorsHQ[{0}]", i); }
            }
            //loop dictOfHQ and get Workers and LeftHQ
            List<Actor> listOfWorkers = new List<Actor>();
            List<Actor> listOfLeftHQ = new List<Actor>();
            Dictionary<int, Actor> dictOfHQ = GameManager.instance.dataScript.GetDictOfHQ();
            if (dictOfHQ != null)
            {
                foreach (var actor in dictOfHQ)
                {
                    if (actor.Value != null)
                    {
                        switch (actor.Value.statusHQ)
                        {
                            case ActorHQ.Worker: listOfWorkers.Add(actor.Value); break;
                            case ActorHQ.LeftHQ: listOfLeftHQ.Add(actor.Value); break;
                        }
                    }
                    else { Debug.LogWarning("Invalid actor (Null) in dictOfHQ"); }
                }
                //display Workers
                builder.AppendFormat("{0}- Workers{1}", "\n", "\n");
                if (listOfWorkers.Count > 0)
                {
                    foreach (Actor actor in listOfWorkers)
                    {
                        builder.AppendFormat(" {0}, {1}, ID {2}, Mot {3}, Comp {4}, R {5}{6}", actor.actorName, actor.arc.name, actor.actorID, actor.GetDatapoint(ActorDatapoint.Motivation1),
                            actor.GetPersonality().GetCompatibilityWithPlayer(), actor.Renown, "\n");
                    }
                }
                else { builder.AppendFormat(" No records{0}", "\n"); }
                //display LeftHQ
                builder.AppendFormat("{0}- LeftHQ{1}", "\n", "\n");
                if (listOfLeftHQ.Count > 0)
                {
                    foreach (Actor actor in listOfLeftHQ)
                    {
                        builder.AppendFormat(" {0}, {1}, ID {2}, Mot {3}, Comp {4}, R {5}{6}", actor.actorName, actor.arc.name, actor.actorID, actor.GetDatapoint(ActorDatapoint.Motivation1),
                            actor.GetPersonality().GetCompatibilityWithPlayer(), actor.Renown, "\n");
                    }
                }
                else { builder.AppendFormat(" No records{0}", "\n"); }
            }
            else { Debug.LogError("Invalid dictOfHQ (Null)"); }
        }
        else { Debug.LogError("Invalid arrayOfActorsHQ (Null)"); }
        return builder.ToString();
    }
}
