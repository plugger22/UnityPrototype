﻿using gameAPI;
using modalAPI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// handles all faction related matters for both sides
/// </summary>
public class FactionManager : MonoBehaviour
{
    [Tooltip("Approval for both sides factions range from 0 to this amount")]
    [Range(0, 10)] public int maxFactionApproval = 10;
    [Tooltip("How much Renown will the faction give per turn if they decide to support the Player")]
    [Range(1, 3)] public int renownPerTurn = 1;

    [Header("Actor Influence")]
    [Tooltip("Amount Faction Approval drops by whenever an Actor resigns for whatever reason")]
    [Range(0, 3)] public int factionApprovalActorResigns = 1;

    [Header("Faction Matters")]
    [Tooltip("Timer set when faction approval is first zero. Decrements each turn and when zero the Player is fired. Reset if approval rises above zero")]
    [Range(1, 10)] public int factionFirePlayerTimer = 3;

    [HideInInspector] public Faction factionAuthority;
    [HideInInspector] public Faction factionResistance;

    private int approvalZeroTimer;                           //countdown timer once approval at zero. Player fired when timer reaches zero.
    private bool isZeroTimerThisTurn;                       //only the first zero timer event per turn is processed
    private int _approvalAuthority;                          //level of faction approval (out of 10) enjoyed by authority side (Player/AI)
    private int _approvalResistance;                         //level of faction approval (out of 10) enjoyed by resistance side (Player/AI)


    //fast access
    private GlobalSide globalAuthority;
    private GlobalSide globalResistance;

    private string colourRebel;
    private string colourAuthority;
    private string colourNeutral;
    private string colourNormal;
    private string colourGood;
    private string colourBad;
    private string colourGrey;
    //private string colourAlert;
    private string colourSide;
    private string colourEnd;


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

    public void Initialise()
    {
        //fast access
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        //Authority faction -> cityManager decides current authority faction as it depends on mayor's faction
        Debug.Assert(factionAuthority != null, "Invalid factionAuthority (Null)");
        Trait trait = GameManager.instance.dataScript.GetRandomTrait(GameManager.instance.globalScript.categoryFaction);
        Debug.Assert(trait != null, "Invalid authority trait (Null)");
        factionAuthority.AddTrait(trait);
        //set AI resource levels
        GameManager.instance.aiScript.resourcesGainAuthority = factionAuthority.resourcesAllowance;
        GameManager.instance.dataScript.SetAIResources(GameManager.instance.globalScript.sideAuthority, factionAuthority.resourcesStarting);
        //Resistance faction
        factionResistance = GameManager.instance.dataScript.GetRandomFaction(GameManager.instance.globalScript.sideResistance);
        Debug.Assert(factionResistance != null, "Invalid factionResistance (Null)");
        trait = GameManager.instance.dataScript.GetRandomTrait(GameManager.instance.globalScript.categoryFaction);
        Debug.Assert(trait != null, "Invalid resistance trait (Null)");
        factionResistance.AddTrait(trait);
        //set AI resource levels
        GameManager.instance.aiScript.resourcesGainResistance = factionResistance.resourcesAllowance;
        GameManager.instance.dataScript.SetAIResources(GameManager.instance.globalScript.sideResistance, factionResistance.resourcesStarting);
        //approval levels
        ApprovalAuthority = Random.Range(1, 10);
        ApprovalResistance = Random.Range(1, 10);
        Debug.LogFormat("FactionManager: currentResistanceFaction \"{0}\", currentAuthorityFaction \"{1}\"{2}",
            factionResistance, factionAuthority, "\n");
        //update colours for AI Display tooltip data
        SetColours();
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "FactionManager");
        EventManager.instance.AddListener(EventType.StartTurnEarly, OnEvent, "FactionManager");
        EventManager.instance.AddListener(EventType.EndTurnLate, OnEvent, "FactionManager");
    }

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
                CheckFactionRenownSupport();
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
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        colourRebel = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        //colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
        if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; }
        else { colourSide = colourRebel; }
    }

    /// <summary>
    /// End turn late event
    /// </summary>
    private void EndTurnLate()
    {
        //reset flag ready for next turn
        isZeroTimerThisTurn = false;
    }

    /// <summary>
    /// checks if player given support (+1 renown) from faction based on a random roll vs. level of faction approval
    /// </summary>
    private void CheckFactionRenownSupport()
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
                        Debug.LogFormat("[Rnd] FactionManager.cs -> CheckFactionSupport: GIVEN need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                        string msgText = string.Format("{0} faction provides SUPPORT (+1 Renown)", factionAuthority.name);
                        GameManager.instance.messageScript.FactionSupport(msgText, factionAuthority, _approvalAuthority, GameManager.instance.playerScript.Renown, renownPerTurn);
                        //random
                        GameManager.instance.messageScript.GeneralRandom("Faction support GIVEN", "Faction Support", threshold, rnd);
                        //Support given
                        GameManager.instance.playerScript.Renown += renownPerTurn;
                    }
                    else
                    {
                        //Support declined
                        Debug.LogFormat("[Rnd] FactionManager.cs -> CheckFactionSupport: DECLINED need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                        string msgText = string.Format("{0} faction declines support ({1} % chance of support)", factionAuthority.name, threshold);
                        GameManager.instance.messageScript.FactionSupport(msgText, factionAuthority, _approvalAuthority, GameManager.instance.playerScript.Renown);
                        //random
                        GameManager.instance.messageScript.GeneralRandom("Faction support DECLINED", "Faction Support", threshold, rnd);
                    }
                    break;
                case 2:
                    //Resistance
                    threshold = _approvalResistance * 10;
                    if (rnd < threshold)
                    {
                        //Support Provided
                        Debug.LogFormat("[Rnd] FactionManager.cs -> CheckFactionSupport: GIVEN need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                        string msgText = string.Format("{0} faction provides SUPPORT (+1 Renown)", factionResistance.name);
                        GameManager.instance.messageScript.FactionSupport(msgText, factionResistance, _approvalResistance, GameManager.instance.playerScript.Renown, renownPerTurn);
                        //random
                        GameManager.instance.messageScript.GeneralRandom("Faction support GIVEN", "Faction Support", threshold, rnd);
                        //Support given
                        GameManager.instance.playerScript.Renown += renownPerTurn;
                    }
                    else
                    {
                        //Support declined
                        Debug.LogFormat("[Rnd] FactionManager.cs -> CheckFactionSupport: DECLINED need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                        string msgText = string.Format("{0} faction declines support ({1} % chance of support)", factionResistance.name, threshold);
                        GameManager.instance.messageScript.FactionSupport(msgText,factionResistance, _approvalResistance, GameManager.instance.playerScript.Renown);
                        //random
                        GameManager.instance.messageScript.GeneralRandom("Faction support DECLINED", "Faction Support", threshold, rnd);
                    }
                    break;
                default:
                    Debug.LogWarningFormat("Invalid side \"{0}\"", side);
                    break;
            }
        }
    }

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
        switch(side.level)
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
                    //you lost, opposite side won
                    GameManager.instance.win = WinState.Authority;
                    if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
                    {
                        //Resistance side wins
                        GameManager.instance.win = WinState.Resistance;
                    }
                    msgText = string.Format("{0} faction approval Zero. Player Fired. Authority wins", playerFaction.name);
                    itemText = string.Format("{0} faction has LOST PATIENCE", playerFaction.name);
                    reason = string.Format("{0} faction approval at ZERO for an extended period", playerFaction.name);
                    warning = "You've been FIRED, game over";
                    GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "You're FIRED", reason, warning);
                    //Player fired -> outcome
                    ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
                    outcomeDetails.side = side;
                    outcomeDetails.textTop = string.Format("{0}The {1} faction has lost faith in your abilities{2}", colourNormal, GetFactionName(side), colourEnd);
                    outcomeDetails.textBottom = string.Format("{0}You've been FIRED{1}", colourBad, colourEnd);
                    outcomeDetails.sprite = GameManager.instance.guiScript.firedSprite;
                    EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "FactionManager.cs -> CheckFactionFirePlayer");

                }
                else
                {
                    //message
                    msgText = string.Format("Faction approval Zero. Faction will FIRE you in {0} turn{1}", approvalZeroTimer, approvalZeroTimer != 1 ? "s" : "");
                    itemText = string.Format("{0} faction about to FIRE you", playerFaction.name);
                    reason = string.Format("{0} faction are displeased with your performance", playerFaction.name);
                    warning = string.Format("You will be FIRED in {0} turn{1}", approvalZeroTimer, approvalZeroTimer != 1 ? "s" : "");
                    GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "Faction Unhappy", reason, warning);
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
        switch(GameManager.instance.sideScript.PlayerSide.name)
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
    /// returns current faction for the specified side in a colour formatted string
    /// </summary>
    /// <returns></returns>
    public string GetFactionName(GlobalSide side)
    {
        string description = "Unknown";
        if (side != null)
        {
            switch (side.level)
            {
                case 1:
                    description = string.Format("<b>{0}{1}{2}</b>", colourSide, factionAuthority.name, colourEnd);
                    break;
                case 2:
                    description = string.Format("<b>{0}{1}{2}</b>", colourSide, factionResistance.name, colourEnd);
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
                    description = string.Format("{0}{1}{2}", colourNormal, factionAuthority.descriptor, colourEnd);
                    break;
                case 2:
                    description = string.Format("{0}{1}{2}", colourNormal, factionResistance.descriptor, colourEnd);
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
            NodeArc arc;
            switch (side.level)
            {
                case 1:
                    arc = factionAuthority.preferredArc;
                    if (arc != null) { colourNode = colourGood; }
                    else { colourNode = colourGrey; }
                    builder.AppendFormat("Preferred Nodes {0}{1}{2}{3}", colourNode, arc != null ? arc.name : "None", colourEnd, "\n");
                    arc = factionAuthority.hostileArc;
                    if (arc != null) { colourNode = colourBad; }
                    else { colourNode = colourGrey; }
                    builder.AppendFormat("Hostile Nodes {0}{1}{2}{3}", colourNode, arc != null ? arc.name : "None", colourEnd, "\n");
                    builder.AppendFormat("{0}{1}{2}{3} Actions per turn{4}", colourNeutral, factionAuthority.maxTaskPerTurn, colourEnd, colourNormal, colourEnd);
                    break;
                case 2:
                    arc = factionResistance.preferredArc;
                    if (arc != null) { colourNode = colourGood; }
                    else { colourNode = colourGrey; }
                    builder.AppendFormat("Preferred Nodes {0}{1}{2}{3}", colourNode, arc != null ? arc.name : "None", colourEnd, "\n");
                    arc = factionResistance.hostileArc;
                    if (arc != null) { colourNode = colourBad; }
                    else { colourNode = colourGrey; }
                    builder.AppendFormat("Hostile Nodes {0}{1}{2}{3}", colourNode, arc != null ? arc.name : "None", colourEnd, "\n");
                    builder.AppendFormat("{0}{1}{2}{3} Actions per turn{4}", colourNeutral, factionResistance.maxTaskPerTurn, colourEnd, colourNormal, colourEnd);
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
    /// use this to adjust faction approval level (auto checks for various faction mechanics & generates a message)
    /// </summary>
    /// <param name="amount"></param>
    public void ChangeFactionApproval(int amountToChange, string reason)
    {
        if (string.IsNullOrEmpty(reason) == true) { reason = "Unknown"; }
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        switch(side.level)
        {
            case 1:
                //Authority
                ApprovalAuthority += amountToChange;
                Debug.LogFormat("[Fac] Authority Faction Approval: change {0}{1} now {2} ({3}){4}", amountToChange > 0 ? "+" : "", amountToChange, ApprovalAuthority, reason, "\n");
                break;
            case 2:
                //Resistance
                ApprovalResistance += amountToChange;
                Debug.LogFormat("[Fac] Resistance Faction Approval: change {0}{1} now {2} ({3}){4}", amountToChange > 0 ? "+" : "", amountToChange, ApprovalResistance, reason, "\n");
                break;
            default:
                Debug.LogWarningFormat("Invalid PlayerSide \"{0}\"", side);
                break;
        }
    }



    //
    // - - - Debug - - -
    //

    /// <summary>
    /// Debug method to display faction info
    /// </summary>
    /// <returns></returns>
    public string DisplayFactions()
    {
        StringBuilder builder = new StringBuilder();
        //authority
        builder.AppendFormat(" AUTHORITY{0}{1}", "\n", "\n");
        builder.AppendFormat(" {0} faction{1}", factionAuthority.name, "\n");
        builder.AppendFormat(" {0}{1}{2}", factionAuthority.descriptor, "\n", "\n");
        builder.AppendFormat(" Preferred Nodes: {0}{1}", factionAuthority.preferredArc != null ? factionAuthority.preferredArc.name : "None", "\n");
        builder.AppendFormat(" Hostile Nodes: {0}{1}", factionAuthority.hostileArc != null ? factionAuthority.hostileArc.name : "None", "\n", "\n");
        builder.AppendFormat(" Max Number of Tasks per Turn: {0}{1}{2}", factionAuthority.maxTaskPerTurn, "\n", "\n");
        builder.AppendFormat(" AI Resource Pool: {0}{1}", GameManager.instance.dataScript.CheckAIResourcePool(GameManager.instance.globalScript.sideAuthority), "\n");
        builder.AppendFormat(" AI Resource Allowance: {0}{1}{2}", GameManager.instance.aiScript.resourcesGainAuthority, "\n", "\n");
        //resistance
        builder.AppendFormat("{0} RESISTANCE{1}{2}", "\n", "\n", "\n");
        builder.AppendFormat(" {0} faction{1}", factionResistance.name, "\n");
        builder.AppendFormat(" {0}{1}{2}", factionResistance.descriptor, "\n", "\n");
        builder.AppendFormat(" Preferred Nodes: {0}{1}", factionResistance.preferredArc != null ? factionResistance.preferredArc.name : "None", "\n");
        builder.AppendFormat(" Hostile Nodes: {0}{1}", factionResistance.hostileArc != null ? factionResistance.hostileArc.name : "None", "\n", "\n");
        builder.AppendFormat(" Max Number of Tasks per Turn: {0}{1}{2}", factionResistance.maxTaskPerTurn, "\n", "\n");
        builder.AppendFormat(" AI Resource Pool: {0}{1}", GameManager.instance.dataScript.CheckAIResourcePool(GameManager.instance.globalScript.sideResistance), "\n");
        builder.AppendFormat(" AI Resource Allowance: {0}{1}{2}", GameManager.instance.aiScript.resourcesGainResistance, "\n", "\n");
        return builder.ToString();
    }
}
