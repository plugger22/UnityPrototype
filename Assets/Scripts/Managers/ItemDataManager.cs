using gameAPI;
using packageAPI;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// holds all methods required for generating ItemData.bottomText's (called by MessageManager.cs methods)
/// </summary>
public class ItemDataManager : MonoBehaviour
{

    [Header("TextList Shorts")]
    [Tooltip("Random pick list used for Actor Rumour messages, format '[Contact] has heard that'")]
    public TextList shortRumourAware;
    [Tooltip("Random pick list used for Actor Rumour message, format '[Contact][shortRumourHeard] there'll be a chance soon to'")]
    public TextList shortRumourAction;
    [Tooltip("Random pick list used for Nemesis & Team Spotted message, format '[Contact] shortContactAware [that they] shortContactAction [a] [Nemesis / Team]")]
    public TextList shortContactAware;
    [Tooltip("Random pick list used for Nemesis & Team Spotted message, format '[Contact] shortContactAware [that they] shortContactAction [a] [Nemesis / Team]")]
    public TextList shortContactAction;

    //fast access
    private GlobalSide globalResistance;
    private GlobalSide globalAuthority;
    private string colourRebel;
    //private string colourAuthority;
    private string colourNeutral;
    private string colourNormal;
    private string colourGood;
    private string colourBad;
    //private string colourGrey;
    private string colourAlert;
    //private string colourSide;
    private string colourEnd;



    public void Initialise(GameState state)
    {
        //textlists
        Debug.Assert(shortRumourAware != null, "Invalid shortRumourHeard (Null)");
        Debug.Assert(shortRumourAction != null, "Invalid shortRumourAction (Null)");
        Debug.Assert(shortContactAware != null, "Invalid shortContactAware (Null)");
        Debug.Assert(shortContactAction != null, "Invalid shortContactAction (Null)");
        Debug.Assert(shortRumourAware.category.name.Equals("Shorts", System.StringComparison.Ordinal) == true, "Invalid shortRumourHeard (wrong Category)");
        Debug.Assert(shortRumourAction.category.name.Equals("Shorts", System.StringComparison.Ordinal) == true, "Invalid shortRumourAction (wrong Category)");
        Debug.Assert(shortContactAware.category.name.Equals("Shorts", System.StringComparison.Ordinal) == true, "Invalid shortContactAware (wrong Category)");
        Debug.Assert(shortContactAction.category.name.Equals("Shorts", System.StringComparison.Ordinal) == true, "Invalid shortContactAction (wrong Category)");
        //fast access
        globalResistance = GameManager.instance.globalScript.sideResistance;
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        /*playerName = GameManager.instance.playerScript.PlayerName;
        playerNameResistance = GameManager.instance.playerScript.GetPlayerNameResistance();*/
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "ItemDataManager");
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
            case EventType.ChangeColour:
                SetColours();
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
        //colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.badText);
        colourRebel = GameManager.instance.colourScript.GetColour(ColourType.blueText);
        //colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodText);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.salmonText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
        /*if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; }
        else { colourSide = colourRebel; }*/
    }


    //
    // - - - General - - -
    //

    /// <summary>
    /// returns a colour formatted string for ItemData string message (Random roll). Shows red for a bad outcome and green for a good (to do this you need to use the isReversed setting depending on roll type)
    /// </summary>
    /// <param name="numNeeded"></param>
    /// <param name="numRolled"></param>
    /// <returns></returns>
    public string GetRandomDetails(int numNeeded, int numRolled, bool isReversed)
    {
        StringBuilder builder = new StringBuilder();
        //reverse colours in case of a success indicating a bad outcome, eg. gear is compromised (so shows SUCCESS but in red, not green)
        string colourSuccess = colourGood;
        string colourFail = colourBad;
        if (isReversed == true)
        {
            colourSuccess = colourBad;
            colourFail = colourGood;
        }
        builder.AppendFormat("Need less than {0}<b>{1}</b>{2}", colourNeutral, numNeeded, colourEnd);
        //success or failure
        if (numRolled < numNeeded)
        {
            builder.AppendFormat("{0}Rolled {1}<b>{2}</b>{3}{4}{5}", "\n", colourSuccess, numRolled, colourEnd, "\n", "\n");
            builder.AppendFormat("{0}<b>SUCCESS</b>{1}{2}{3}", colourSuccess, colourEnd, "\n", "\n");
        }
        else
        {
            builder.AppendFormat("{0}Rolled {1}<b>{2}</b>{3}{4}{5}", "\n", colourFail, numRolled, colourEnd, "\n", "\n");
            builder.AppendFormat("{0}<b>FAILED</b>{1}{2}{3}", colourFail, colourEnd, "\n", "\n");
        }
        builder.AppendFormat("A {0}Percentage die{1} (1d00) is used", colourNeutral, colourEnd);
        return builder.ToString();
    }

    /// <summary>
    /// returns colour formatted string in style '[typeOfCheck] Check' with typeOfCheck being colour coded where Green indicates a success on the check is a good thing for the player
    /// </summary>
    /// <param name="typeOfCheck"></param>
    /// <param name="isReversed"></param>
    /// <returns></returns>
    public string GetRandomTopText(string typeOfCheck, bool isReversed)
    {
        if (isReversed == false)
        { return string.Format("{0}{1}{2} check", colourGood, typeOfCheck, colourEnd); }
        else { return string.Format("{0}{1}{2} check", colourBad, typeOfCheck, colourEnd); }
    }


    /// <summary>
    /// General Info (could be anything). 'explanation' shown in Red if 'isBad' true, otherwise in Green
    /// </summary>
    /// <param name="reason"></param>
    /// <param name="warning"></param>
    /// <returns></returns>
    public string GetGeneralInfoDetails(string reason, string explanation, bool isBad)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<b>Info Alert!</b>{1}{2}{3}{4}{5}", colourNeutral, colourEnd, "\n", reason, "\n", "\n");
        if (isBad == true)
        { builder.AppendFormat("{0}{1}{2}", colourBad, explanation, colourEnd); }
        else { builder.AppendFormat("{0}{1}{2}", colourGood, explanation, colourEnd); }
        return builder.ToString();
    }

    /// <summary>
    /// General Warning (could be anything)
    /// </summary>
    /// <param name="reason"></param>
    /// <param name="warning"></param>
    /// <returns></returns>
    public string GetGeneralWarningDetails(string reason, string warning, bool isBad)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<b>Alert!</b>{1}{2}{3}{4}{5}", colourNeutral, colourEnd, "\n", reason, "\n", "\n");
        if (isBad == true)
        { builder.AppendFormat("{0}<b>{1}</b>{2}", colourBad, warning, colourEnd); }
        else { builder.AppendFormat("{0}<b>{1}</b>{2}", colourGood, warning, colourEnd); }
        return builder.ToString();
    }


    //
    // - - - Player - - -
    //

    /// <summary>
    /// Returns a colour formatted string for ItemData string message
    /// NOTE: Node & playerName have been checked for null by the calling method -> MessageManager.cs -> PlayerMove
    /// </summary>
    /// <returns></returns>
    public string GetPlayerMoveDetails(Node node, int changeInvisibility, int aiDelay)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<b>{1}</b>{2} now at{3}{4}", colourNeutral, GameManager.instance.playerScript.GetPlayerNameResistance(), colourEnd, "\n", "\n");
        builder.AppendFormat("{0}, a {1}<b>{2}</b>{3} district", node.nodeName, colourAlert, node.Arc.name, colourEnd);
        if (GameManager.instance.optionScript.debugData == true)
        { builder.AppendFormat(", ID {0}", node.nodeID); }
        if (changeInvisibility != 0)
        {
            builder.AppendFormat("{0}{1}{2}<b>Player SPOTTED{3}Invisibility {4}</b>{5}", "\n", "\n", colourBad, "\n", changeInvisibility, colourEnd);
            if (aiDelay > 0)
            { builder.AppendFormat("{0}{1}{2}<b>Authority will know in {3}{4}{5}{6} turn{7}</b>{8}", "\n", "\n", colourAlert, colourNeutral, aiDelay, colourEnd, colourAlert, aiDelay != 1 ? "s" : "", colourEnd); }
        }
        return builder.ToString();
    }

    /// <summary>
    /// Returns a colour formatted string for ItemData string message
    /// Player damaged by Nemesis
    /// </summary>
    /// <param name="damageInfo"></param>
    /// <param name="damageEffect"></param>
    /// <returns></returns>
    public string GetPlayerDamageDetails(string damageInfo, string damageEffect, bool isResistance)
    {
        StringBuilder builder = new StringBuilder();
        if (string.IsNullOrEmpty(damageInfo) == false)
        {
            if (isResistance == true)
            { builder.AppendFormat("You have been <b>{0}</b>{1}{2}", damageInfo, "\n", "\n"); }
            else { builder.AppendFormat("{0} has been <b>{1}</b>{2}{3}", GameManager.instance.playerScript.GetPlayerNameResistance(), damageInfo, "\n", "\n"); }
        }
        if (string.IsNullOrEmpty(damageEffect) == false)
        { builder.AppendFormat("{0}<b>{1}</b>{2}", colourAlert, damageEffect, colourEnd); }
        return builder.ToString();
    }

    /// <summary>
    /// Player spotted (IMAGED, TAGGED)
    /// </summary>
    /// <param name="detailsTop"></param>
    /// <param name="detailsBottom"></param>
    /// <param name="actorID"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public string GetPlayerSpottedDetails(string detailsTop, string detailsBottom, Node node)
    {
        StringBuilder builder = new StringBuilder();
        //player
        builder.AppendFormat("<b>{0}, {1}PLAYER</b>{2}{3}{4}", GameManager.instance.playerScript.GetPlayerNameResistance(), colourAlert, colourEnd, "\n", "\n");
        builder.AppendFormat("<b>{0}, {1}{2}</b>{3}, district{4}{5}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n", "\n");
        //details
        if (string.IsNullOrEmpty(detailsTop) == false)
        { builder.Append(detailsTop); }
        if (string.IsNullOrEmpty(detailsBottom) == false)
        { builder.AppendFormat("{0}{1}{2}", "\n", "\n", detailsBottom); }
        return builder.ToString();
    }

    /// <summary>
    /// Player betrayed
    /// </summary>
    /// <returns></returns>
    public string GetPlayerBetrayedDetails(bool isImmediate)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>A {0}TRAITOR{1} in {2}Rebel HQ{3}{4}or perhaps one of your{5}{6}Subordinates{7}?</b>", colourBad, colourEnd, colourAlert, colourEnd, "\n", "\n", colourAlert, colourEnd);
        builder.AppendFormat("<b>{0}{1}{2}Invisibility -1{3}</b>", "\n", "\n", colourBad, colourEnd);
        if (isImmediate == true)
        { builder.AppendFormat("{0}{1}{2}<size=110%><b>Authority will know immediately</b></size>{3}", "\n", "\n", colourBad, colourEnd); }
        return builder.ToString();
    }

    /// <summary>
    /// Player's mood changed
    /// </summary>
    /// <param name="details"></param>
    /// <param name="change"></param>
    /// <param name="isStressed"></param>
    /// <returns></returns>
    public string GetPlayerMoodChangeDetails(string details, int change, int current, bool isStressed)
    {
        StringBuilder builder = new StringBuilder();
        string colourMood = colourGood;
        if (change < 0) { colourMood = colourBad; }
        builder.AppendFormat("<b>{0}{1}{2}</b>{3}{4}", colourAlert, details, colourEnd, "\n", "\n");
        builder.AppendFormat("{0}<b>Mood {1}{2}</b>{3}", colourMood, change > 0 ? "+" : "", change, colourEnd);
        if (isStressed == true)
        { builder.AppendFormat("{0}{1}{2}Gains gains STRESSED Condition{1}", "\n", "\n", colourBad, colourEnd); }
        builder.AppendFormat("{0}{1}<b>Mood is now {2}</b>", "\n", "\n", current);
        return builder.ToString();
    }

    /// <summary>
    /// Player immune to stress (start of immune period) due to taking illegal drugs. May or may not be addicted
    /// </summary>
    /// <param name="currentPeriod"></param>
    /// <param name="isAddicted"></param>
    /// <returns></returns>
    public string GetPlayerImmuneStartDetails(int currentPeriod, bool isAddicted)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>Taking a dose of {0}{1}makes you {2}Immune to Stress{3}</b>{4}{5}", GameManager.instance.globalScript.tagGlobalDrug, "\n", colourGood, colourEnd, "\n", "\n");
        builder.AppendFormat("<b>Immunity lasts for{0}{1}{2} day{3}</b>{4}", "\n", colourNeutral, currentPeriod, currentPeriod != 1 ? "s" : "", colourEnd);
        if (isAddicted == true)
        { builder.AppendFormat("{0}{1}<b>{2}You are ADDICTED{3}</b>", "\n", "\n", colourBad, colourEnd); }
        return builder.ToString();
    }

    /// <summary>
    /// Player did NOT get stressed due to taking illegal drugs. May or may not be addicted
    /// </summary>
    /// <param name="currentPeriod"></param>
    /// <param name="isAddicted"></param>
    /// <returns></returns>
    public string GetPlayerImmuneStressDetails(int currentPeriod, bool isAddicted)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>{0}You did NOT get STRESSED{1}{2}due to taking {3}{4}{5}</b>{6}{7}", colourGood, colourEnd, "\n", colourAlert, GameManager.instance.globalScript.tagGlobalDrug, colourEnd, "\n", "\n");
        builder.AppendFormat("<b>Your Immunity lasts for{0}{1}{2} day{3}</b>{4}", "\n", colourNeutral, currentPeriod, currentPeriod != 1 ? "s" : "", colourEnd);
        if (isAddicted == true)
        { builder.AppendFormat("{0}{1}<b>{2}You are ADDICTED{3}</b>", "\n", "\n", colourBad, colourEnd); }
        return builder.ToString();
    }

    /// <summary>
    /// Player immune to stress (start of immune period) due to taking illegal drugs. May or may not be addicted
    /// </summary>
    /// <param name="currentPeriod"></param>
    /// <param name="isAddicted"></param>
    /// <returns></returns>
    public string GetPlayerImmuneEffectDetails(int currentPeriod, bool isAddicted)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>Taking a dose of {0}{1}{2}{3}makes you {4}Immune to Stress{5}</b>{6}{7}", colourAlert, GameManager.instance.globalScript.tagGlobalDrug, colourEnd, "\n",
            colourGood, colourEnd, "\n", "\n");
        builder.AppendFormat("<b>Immunity lasts for{0}{1}{2} day{3}</b>{4}", "\n", colourNeutral, currentPeriod, currentPeriod != 1 ? "s" : "", colourEnd);
        if (isAddicted == true)
        { builder.AppendFormat("{0}{1}<b>{2}You are ADDICTED{3}</b>", "\n", "\n", colourBad, colourEnd); }
        return builder.ToString();
    }

    /// <summary>
    /// Player is addicted and has had to feed their need
    /// </summary>
    /// <param name="renownCost"></param>
    /// <param name="approvalCost"></param>
    /// <param name="currentDays"></param>
    /// <returns></returns>
    public string GetPlayerAddictedDetails(int renownCost, int approvalCost, int currentDays)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>Your addiction to {0}{1}{2}{3} rages unabated</b>{4}{5}", colourNeutral, GameManager.instance.globalScript.tagGlobalDrug, colourEnd, "\n", "\n", "\n");
        if (renownCost > 0)
        { builder.AppendFormat("<b>{0}You spend {1} Renown{2}{3}to buy more drugs</b>{4}{5}", colourBad, renownCost, colourEnd, "\n", "\n", "\n"); }
        else
        {
            builder.AppendFormat("<b>{0}You did not have enough{1}Renown ({2} needed){3}{4}to buy more drugs{5}{6}HQ Approval -1</b>{7}{8}{9}", colourAlert, "\n",
         GameManager.instance.actorScript.playerAddictedRenownCost, colourEnd, "\n", "\n", colourBad, colourEnd, "\n", "\n");
        }
        builder.AppendFormat("<b>{0}You have Immunity from Stress{1}{2}for {3}{4} day{5}{6}", colourGood, colourEnd, "\n", colourNeutral, currentDays, currentDays != 1 ? "s" : "", colourEnd);
        return builder.ToString();
    }


    /// <summary>
    /// A cure has arisen for the Player's condition
    /// </summary>
    /// <param name="node"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    public string GetPlayerCureDetails(Node node, Condition condition, bool isActivated)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<b>{1}</b>{2}{3}", colourNeutral, condition.cure.name, colourEnd, "\n");
        builder.AppendFormat("<b>{0}, {1}{2}</b>{3}{4}{5}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n", "\n");
        if (isActivated == true)
        { builder.AppendFormat("{0}<b>{1}</b>{2}", colourAlert, condition.cure.tooltipText, colourEnd); }
        else { builder.AppendFormat("{0}NO LONGER AVAILABLE{1}", colourBad, colourEnd); }
        return builder.ToString();
    }


    //
    // - - - Actor - - -
    //

    /// <summary>
    /// Actor / Player status change
    /// </summary>
    /// <param name="reason"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    public string GetActorStatusDetails(string reason, string details, Actor actor)
    {
        StringBuilder builder = new StringBuilder();
        if (actor == null)
        {
            //
            // - - - Player
            //
            //get correct player status depending on who is in charge of Resistance
            ActorStatus status = ActorStatus.Active;
            if (GameManager.instance.sideScript.resistanceOverall == SideState.Human)
            { status = GameManager.instance.playerScript.status; }
            else { status = GameManager.instance.aiRebelScript.status; }
            //message
            if (string.IsNullOrEmpty(reason) == false)
            { builder.AppendFormat("<b>{0}{1}, PLAYER</b>{2}{3}<b>{4}</b>{5}{6}", GameManager.instance.playerScript.PlayerName, colourAlert, colourEnd, "\n", reason, "\n", "\n"); }
            builder.AppendFormat("{0}<b>PLAYER</b>{1} status now {2}<b>{3}</b>{4}", colourAlert, colourEnd, colourNeutral, status, colourEnd);
        }
        else
        {
            //Actor
            if (string.IsNullOrEmpty(reason) == false)
            { builder.AppendFormat("{0}, {1}<b>{2}</b>{3}{4}{5}{6}{7}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", reason, "\n", "\n"); }
            builder.AppendFormat("{0}<b>{1}</b>{2} status now {3}<b>{4}</b>{5}", colourAlert, actor.arc.name, colourEnd, colourNeutral, actor.Status, colourEnd);
        }
        if (string.IsNullOrEmpty(details) == false)
        { builder.AppendFormat("{0}{1}<b>{2}</b>", "\n", "\n", details); }
        return builder.ToString();
    }

    /// <summary>
    /// Actor / Player (both sides) takes stress leave (actor Null for Player)
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>
    public string GetActorStressLeaveDetails(GlobalSide side, Actor actor = null)
    {
        StringBuilder builder = new StringBuilder();
        if (actor == null)
        {
            //Player
            if (side.level == globalAuthority.level)
            { builder.AppendFormat("The {0}<b>Mayor{1}, {2},</b> has left the office for a short while", colourAlert, colourEnd, GameManager.instance.playerScript.GetPlayerNameAuthority()); }
            else { builder.AppendFormat("The {0}<b>Player{1}, {2},</b> has left for a short while", colourAlert, colourEnd, GameManager.instance.playerScript.GetPlayerNameResistance()); }
            builder.AppendFormat("{0}{1}When they return they will be {2}<b>Stress Free</b>{3}", "\n", "\n", colourGood, colourEnd);
        }
        else
        {
            //Actor
            builder.AppendFormat("<b>{0}, {1}{2}{3},</b> has taken a short break", actor.actorName, colourAlert, actor.arc.name, colourEnd);
            builder.AppendFormat("{0}{1}When they return they will be {2}<b>Stress Free</b>{3}", "\n", "\n", colourGood, colourEnd);
        }
        return builder.ToString();
    }

    /// <summary>
    /// Actor in Reserves has been spoken to
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public string GetActorSpokenTooDetails(Actor actor, string reason, int benefit)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}, {1}{2}{3}, currently in the {4}Reserves{5}{6}{7}", actor.actorName, colourAlert, actor.arc.name, colourEnd, colourNeutral, colourEnd, "\n", "\n");
        if (string.IsNullOrEmpty(reason) == false)
        { builder.AppendFormat("Has been spoken to and {0}<b>{1}</b>{2}{3}{4}", colourNeutral, reason, colourEnd, "\n", "\n"); }
        builder.AppendFormat("{0}{1}{2}<b>Unhappy Timer +{3}</b>{4}", actor.actorName, "\n", colourGood, benefit, colourEnd);
        return builder.ToString();
    }

    /// <summary>
    /// Actor resolves blackmail -> either carries out threat or drops threat (isThreatDropped 'true' and 'reason' why) due to max. motivation
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="secretName"></param>
    /// <param name="isThreatDropped"></param>
    /// <returns></returns>
    public string GetActorBlackmailDetails(Actor actor, string secretName, bool isThreatDropped, string reason)
    {
        StringBuilder builder = new StringBuilder();
        if (isThreatDropped == true)
        {
            //blackmail threat dropped
            builder.AppendFormat("{0}, {1}{2}{3} has{4}{5}Dropped their Threat of Blackmail{6}{7}{8}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", colourGood, colourEnd, "\n", "\n");
            if (string.IsNullOrEmpty(reason) == false)
            { builder.AppendFormat("The threat has been dropped because{0}{1}{2}{3}", "\n", colourNeutral, reason, colourEnd); }
        }
        else
        {
            //blackmail threat carried out -> Secret Revealed
            if (string.IsNullOrEmpty(secretName) == false)
            {
                Secret secret = GameManager.instance.dataScript.GetSecret(secretName);
                if (secret != null)
                {
                    builder.AppendFormat("<b>{0}, {1}{2}{3}</b>{4}{5}has carried out their threat{6}{7}{8}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", colourBad, colourEnd, "\n", "\n");
                    builder.AppendFormat("{0}<b>\"{1}\" secret is Revealed</b>{2}", colourBad, secret.tag, colourEnd);
                    /*GetSecretEffects(builder, secret);*/
                }
                else { Debug.LogWarningFormat("Invalid secret (Null) for secret {0}", secretName); }
            }
        }
        return builder.ToString();
    }

    /// <summary>
    /// Actor confict with Player
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public string GetActorConflictDetails(Actor actor, ActorConflict conflict, string reasonNoConflict)
    {
        StringBuilder builder = new StringBuilder();
        if (conflict != null)
        {
            //CONFLICT
            builder.AppendFormat("{0}, {1}<b>{2}</b>{3} has a{4}Relationship Conflict with you{5}{6}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", "\n", "\n");
            builder.AppendFormat("<b>{0}{1}{2}{3}</b>{4}", actor.actorName, "\n", colourBad, conflict.outcomeText, colourEnd);
        }
        else
        {
            //Conflict didn't happen
            builder.AppendFormat("{0}, {1}{2}{3}{4}{5}Chose not to start a Conflict{6}{7}{8}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", colourGood, colourEnd, "\n", "\n");
            if (string.IsNullOrEmpty(reasonNoConflict) == false)
            { builder.AppendFormat("because{0}{1}{2}{3}", "\n", colourNeutral, reasonNoConflict, colourEnd); }
        }
        return builder.ToString();
    }

    /// <summary>
    /// Player / Actor gained or lost a Condition
    /// </summary>
    /// <param name="genericActorName"></param>
    /// <param name="genericActorArc"></param>
    /// <param name="condition"></param>
    /// <param name="isGained"></param>
    /// <returns></returns>
    public string GetActorConditionDetails(string genericActorName, string genericActorArc, Condition condition, bool isGained, string reason)
    {
        StringBuilder builder = new StringBuilder();
        string colourCondition = colourNeutral;
        if (condition.type != null)
        {
            switch (condition.type.level)
            {
                case 0: colourCondition = colourBad; break;
                case 1: colourCondition = colourNeutral; break;
                case 2: colourCondition = colourGood; break;
                default: colourCondition = colourNeutral; break;
            }
        }
        else { Debug.LogFormat("Invalid condition.type (Null) for condition \"{0}\"", condition.tag); }
        if (isGained == true)
        { builder.AppendFormat("<b>{0}, {1}{2}</b>{3}{4}gains condition {5}<b>{6}</b>{7}{8}{9}", genericActorName, colourAlert, genericActorArc, colourEnd, "\n", colourCondition, condition.tag, colourEnd, "\n", "\n"); }
        else
        { builder.AppendFormat("<b>{0}, {1}{2}</b>{3}{4}loses condition {5}<b>{6}</b>{7}{8}{9}", genericActorName, colourAlert, genericActorArc, colourEnd, "\n", colourCondition, condition.tag, colourEnd, "\n", "\n"); }
        if (string.IsNullOrEmpty(reason) == false)
        { builder.AppendFormat("{0}<b>{1}</b>{2}", colourNeutral, reason, colourEnd); }
        return builder.ToString();
    }

    /// <summary>
    /// Actor uses a trait
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="trait"></param>
    /// <param name="forText"></param>
    /// <param name="toText"></param>
    /// <returns></returns>
    public string GetActorTraitDetails(Actor actor, Trait trait, string forText, string toText)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}, {1}{2}{3} uses{4}{5}<b>{6}</b>{7} trait{8}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", colourNeutral, trait.tag, colourEnd, "\n");
        if (string.IsNullOrEmpty(forText) == false)
        { builder.AppendFormat("{0}{1}{2}", forText, "\n", "\n"); }
        else { Debug.LogWarning("Invalid forText (Null or Empty)"); }
        if (string.IsNullOrEmpty(toText) == false)
        {
            string colourTrait = colourGood;
            switch (trait.typeOfTrait.level)
            {
                case 0: colourTrait = colourBad; break;
                case 1: colourTrait = colourNeutral; break;
                case 2: colourTrait = colourGood; break;
                default: colourTrait = colourNeutral; break;
            }
            builder.AppendFormat("{0}<b>{1}</b>{2}", colourTrait, toText, colourEnd);
        }
        else { Debug.LogWarning("Invalid toText (Null or Empty)"); }
        return builder.ToString();
    }

    /// <summary>
    /// Status of Lie Low timer in InfoApp 'Effects' tab
    /// </summary>
    /// <param name="timer"></param>
    /// <returns></returns>
    public string GetActorLieLowOngoingDetails(int timer)
    {
        StringBuilder builder = new StringBuilder();
        if (timer == 0)
        {
            //available
            builder.AppendFormat("Arrangements have been made by {0}<b>RebelHQ</b>{1}{2}{3}", colourAlert, colourEnd, "\n", "\n");
            builder.AppendFormat("<b>Lie Low</b> action is {0}<b>AVAILABLE</b>{1}", colourNeutral, colourEnd);
        }
        else
        {
            //unavailable
            builder.AppendFormat("<b>RebelHQ</b> are working on it as fast as they can{0}{1}", "\n", "\n");
            builder.AppendFormat("<b>Lie Low</b> action {0}<b>UNAVAILABLE</b>{1}{2}{3}", colourBad, colourEnd, "\n", "\n");
            builder.AppendFormat("<b>Duration {0}{1} turn{2}{3}</b>", colourNeutral, timer, timer != 1 ? "s" : "", colourEnd);
        }
        return builder.ToString();
    }

    /// <summary>
    /// Actor complains
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="reason"></param>
    /// <param name="warning"></param>
    /// <returns></returns>
    public string GetActorComplainsDetails(Actor actor, string reason, string warning)
    {
        StringBuilder builder = new StringBuilder();
        if (string.IsNullOrEmpty(reason) == false)
        { builder.AppendFormat("{0}, {1}{2}{3}", actor.actorName, colourAlert, actor.arc.name, colourEnd); }
        if (string.IsNullOrEmpty(reason) == false)
        { builder.AppendFormat("{0}{1}{2}{3}", "\n", colourNeutral, reason, colourEnd); }
        if (string.IsNullOrEmpty(warning) == false)
        { builder.AppendFormat("{0}{1}{2}<b>{3}</b>{4}", "\n", "\n", colourBad, warning, colourEnd); }
        return builder.ToString();
    }

    /// <summary>
    /// Actor recruited
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="unhappyTimer"></param>
    /// <returns></returns>
    public string GetActorRecruitedDetails(Actor actor, int unhappyTimer)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}, {1}{2}{3}{4}has been Recruited{5}{6}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", "\n", "\n");
        builder.AppendFormat("{0} awaits your command in the Reserves{1}{2}{3}{4} Unhappy in {5} turn{6}{7}", actor.actorName, "\n", "\n", colourBad, actor.arc.name,
            unhappyTimer, unhappyTimer != 1 ? "s" : "", colourEnd);
        return builder.ToString();
    }

    /// <summary>
    /// Warning, eg. 'Actor can be captured 'cause invis is Zero', InfoApp 'Effects' tab
    /// </summary>
    /// <param name="detailsTop"></param>
    /// <param name="detailsBottom"></param>
    /// <param name="actorID"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public string GetActorWarningOngoingDetails(string detailsTop, string detailsBottom)
    {
        StringBuilder builder = new StringBuilder();
        if (string.IsNullOrEmpty(detailsTop) == false)
        { builder.Append(detailsTop); }
        if (string.IsNullOrEmpty(detailsBottom) == false)
        {
            if (builder.Length > 0)
            { builder.AppendLine(); builder.AppendLine(); }
            builder.Append(detailsBottom);
        }
        return builder.ToString();
    }

    /// <summary>
    /// Actor negates a motivational shift due to their relationship with the player
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="difference"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public string GetActorCompatibilityDetails(Actor actor, int difference, string reason)
    {
        int comp = actor.GetPersonality().GetCompatibilityWithPlayer();
        return new StringBuilder()
            .AppendFormat("<b>{0}, {1}{2}{3}</b>{4}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n")
            .AppendFormat("has ignored a{0}", "\n")
            .AppendFormat("{0}<b>{1}{2}{3} change in {4}Motivation{5}</b>{6}", colourNeutral, difference > 0 ? "+" : "", difference, colourEnd, colourAlert, colourEnd, "\n")
            .AppendFormat("due to <b>{0}</b>{1}{2}", reason, "\n", "\n")
            .AppendFormat("As a result of their{0}", "\n")
            .AppendFormat("{0}<b>{1} opinion of you</b>{2}", comp > 0 ? colourGood : colourBad, comp > 0 ? "Positive" : "Negative", colourEnd)
            .ToString();
    }

    /// <summary>
    /// Actor gains or loses a Contact
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="node"></param>
    /// <param name="contact"></param>
    /// <param name="isGained"></param>
    /// <returns></returns>
    public string GetContactDetails(string reason, Actor actor, Node node, Contact contact, bool isGained)
    {
        StringBuilder builder = new StringBuilder();
        string verb, colourVerb, effectiveVerb, colourEffective;
        verb = "Gained"; effectiveVerb = "Unknown";
        colourVerb = colourGood; colourEffective = colourBad;
        if (isGained == false) { verb = "Lost"; colourVerb = colourBad; }
        builder.AppendFormat("{0}, {1}<b>{2}</b>{3}{4}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n");
        builder.AppendFormat("{0}<b>has {1} a Contact</b>{2}{3}", colourVerb, verb, colourEnd, "\n");
        if (string.IsNullOrEmpty(reason) == false)
        { builder.AppendFormat("{0}<b>{1}</b>{2}{3}", colourNeutral, reason, colourEnd, "\n"); }
        builder.AppendFormat("{0}{1}<b>{2}</b>{3} contact{4}", "\n", colourNeutral, contact.typeName, colourEnd, "\n");
        builder.AppendFormat("{0} {1}, {2}<b>{3}</b>{4}{5}", contact.nameFirst, contact.nameLast, colourAlert, contact.job, colourEnd, "\n");
        builder.AppendFormat("{0}, {1}<b>{2}</b>{3}{4}{5}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n", "\n");
        //effectiveness (convert to text string, put into current or past tense and colour code)
        if (isGained == true)
        {
            switch (contact.effectiveness)
            {
                case 3: effectiveVerb = "is Wired-In"; colourEffective = colourGood; break;
                case 2: effectiveVerb = "is Networked"; colourEffective = colourNeutral; break;
                case 1: effectiveVerb = "knows stuff"; colourEffective = colourBad; break;
            }
        }
        else
        {
            switch (contact.effectiveness)
            {
                case 3: effectiveVerb = "was Wired-In"; colourEffective = colourGood; break;
                case 2: effectiveVerb = "was Networked"; colourEffective = colourNeutral; break;
                case 1: effectiveVerb = "knew stuff"; colourEffective = colourBad; break;
            }
        }
        builder.AppendFormat("{0} {1}<b>{2}</b>{3}", contact.nameFirst, colourEffective, effectiveVerb, colourEnd);
        return builder.ToString();
    }

    /// <summary>
    /// One of actor's network of contacts learns of a rumour about a forthcoming target
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="node"></param>
    /// <param name="contact"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public string GetContactTargetRumourDetails(Actor actor, Node node, Contact contact, Target target)
    {
        StringBuilder builder = new StringBuilder();
        string textAware = shortRumourAware.GetRandomRecord(false);
        string textAction = shortRumourAction.GetRandomRecord(false);
        builder.AppendFormat("<b>{0} {1}, {2}{3}</b>{4} {5} {6}{7}{8}", contact.nameFirst, contact.nameLast, colourAlert, contact.job, colourEnd, textAware, textAction, "\n", "\n");
        builder.AppendFormat("{0}<b>{1}</b>{2}{3}{4}", colourNeutral, target.rumourText, colourEnd, "\n", "\n");
        builder.AppendFormat("At <b>{0}, {1}{2}{3}</b> district{4}{5}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n", "\n");
        /*builder.AppendFormat("<b>{0}</b>", GetConfidenceLevel(contact.effectiveness));*/
        return builder.ToString();
    }

    /// <summary>
    /// One of actor's network of contacts spots the Nemesis
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="node"></param>
    /// <param name="contact"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public string GetContactNemesisSpottedDetails(Actor actor, Node node, Contact contact, Nemesis nemesis, int moveNumber)
    {
        StringBuilder builder = new StringBuilder();
        string textAware = shortContactAware.GetRandomRecord(false);
        string textAction = shortContactAction.GetRandomRecord(false);
        builder.AppendFormat("<b>{0} {1}, {2}{3}{4},</b> {5} that they {6} a{7}{8}", contact.nameFirst, contact.nameLast, colourAlert, contact.job, colourEnd, textAware, textAction, "\n", "\n");
        builder.AppendFormat("{0}<b>{1}</b>{2}{3}{4}", colourNeutral, nemesis.name, colourEnd, "\n", "\n");
        builder.AppendFormat("At <b>{0}, {1}{2}{3}</b> district", node.nodeName, colourAlert, node.Arc.name, colourEnd);
        //only add a time stamp if nemesis is capable of multiple moves within a single turn
        if (nemesis.movement > 1)
        { builder.AppendFormat(" at <b>{0}</b> hrs", GetContactTime(moveNumber)); }
        /*builder.AppendFormat("<b>{0}</b>", GetConfidenceLevel(contact.effectiveness));*/
        return builder.ToString();
    }

    /// <summary>
    /// One of actor's network of contacts spots the Npc
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="node"></param>
    /// <param name="contact"></param>
    /// <param name="npc"></param>
    /// <returns></returns>
    public string GetContactNpcSpottedDetails(Actor actor, Node node, Contact contact, Npc npc)
    {
        StringBuilder builder = new StringBuilder();
        string textAware = shortContactAware.GetRandomRecord(false);
        string textAction = shortContactAction.GetRandomRecord(false);
        builder.AppendFormat("<b>{0} {1}, {2}{3}{4}</b>, {5} that they {6} the {7}<b>{8}</b>{9}{10}{11}", contact.nameFirst, contact.nameLast, colourAlert, contact.job, colourEnd, textAware,
            textAction, colourNeutral, npc.tag, colourEnd, "\n", "\n");
        builder.AppendFormat("At <b>{0}, {1}{2}{3}</b> district{4}{5}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n", "\n");
        builder.AppendFormat("<b>{0} {1}, {2}</b>", contact.nameFirst, contact.nameLast, GameManager.instance.contactScript.GetEffectivenessFormatted(contact.effectiveness));
        return builder.ToString();
    }

    /// <summary>
    /// subMethod to determine a time of day (in 24HR notation) of a contact sighting where moveNumber '0' is a.m and '1' is p.m (to aid player in parsing multiple sightings in a single day)
    /// </summary>
    /// <param name="moveNumber"></param>
    /// <returns></returns>
    private string GetContactTime(int moveNumber)
    {
        string contactTime = "Unknown";
        switch (moveNumber)
        {
            case 0:
                //a.m
                int rndNum = 4 + Random.Range(0, 7);
                contactTime = string.Format("{0}{1}00", rndNum > 9 ? "" : "0", rndNum);
                break;
            default:
                //all others in p.m
                contactTime = string.Format("{0}00", 13 + Random.Range(0, 10));
                break;
        }
        return contactTime;
    }

    /// <summary>
    /// One of actor's network of contacts spots an Authority Team
    /// </summary>
    /// <param name=""></param>
    /// <param name=""></param>
    /// <param name=""></param>
    /// <param name=""></param>
    /// <returns></returns>
    public string GetContactTeamSpottedDetails(Actor actor, Node node, Contact contact, Team team)
    {
        StringBuilder builder = new StringBuilder();
        string textAware = shortContactAware.GetRandomRecord(false);
        string textAction = shortContactAction.GetRandomRecord(false);
        builder.AppendFormat("<b>{0} {1}, {2}{3}</b>{4} {5} that they {6} a{7}{8}", contact.nameFirst, contact.nameLast, colourAlert, contact.job, colourEnd, textAware, textAction, "\n", "\n");
        builder.AppendFormat("{0}<b>{1}</b>{2} team{3}{4}", colourNeutral, team.arc.name, colourEnd, "\n", "\n");
        builder.AppendFormat("At <b>{0}, {1}{2}{3}</b> district{4}{5}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n", "\n");
        /*builder.AppendFormat("<b>{0}</b>", GetConfidenceLevel(contact.effectiveness));*/
        return builder.ToString();
    }

    /// <summary>
    /// One of actor's contacts changes to Inactive Status
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="node"></param>
    /// <param name="contact"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public string GetContactInactiveDetails(Node node, Contact contact, string reason)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>{0} {1}, {2}{3}</b>{4}{5}{6}", contact.nameFirst, contact.nameLast, colourAlert, contact.job, colourEnd, "\n", "\n");
        builder.AppendFormat("{0}<b>Has gone Silent</b>{1}{2}Due to a{3}<b>{4}</b>{5}{6}", colourBad, colourEnd, "\n", "\n", reason, "\n", "\n");
        builder.AppendFormat("At <b>{0}, {1}{2}{3}</b> district{4}{5}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n", "\n");
        return builder.ToString();
    }

    /// <summary>
    /// Inactive contact becomes active
    /// </summary>
    /// <param name="node"></param>
    /// <param name="contact"></param>
    /// <returns></returns>
    public string GetContactActiveDetails(Node node, Contact contact)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>{0} {1}, {2}{3}</b>{4}{5}{6}", contact.nameFirst, contact.nameLast, colourAlert, contact.job, colourEnd, "\n", "\n");
        builder.AppendFormat("{0}<b>Is back on the Grid</b>{1}<b>{2}{3}", colourGood, colourEnd, "\n", "\n");
        builder.AppendFormat("At <b>{0}, {1}{2}{3}</b> district{4}{5}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n", "\n");
        return builder.ToString();
    }

    /// <summary>
    /// Inactive contact, effects tab, how much longer before active
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="node"></param>
    /// <param name="contact"></param>
    /// <returns></returns>
    public string GetContactTimerDetails(Node node, Contact contact)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>{0} {1}, {2}{3}</b>{4}{5}{6}", contact.nameFirst, contact.nameLast, colourAlert, contact.job, colourEnd, "\n", "\n");
        builder.AppendFormat("Will remain Silent for{0}<b>another {1}{2}{3} turn{4}</b>{5}{6}", "\n", colourNeutral, contact.timerInactive, colourEnd, contact.timerInactive != 1 ? "s" : "", "\n", "\n");
        builder.AppendFormat("At <b>{0}, {1}{2}{3}</b> district{4}{5}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n", "\n");
        return builder.ToString();
    }


    /// <summary>
    /// A resistance tracer picks up signs of the Nemesis
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public string GetTracerNemesisSpottedDetails(Node node, Nemesis nemesis, int moveNumber)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>Hostile body</b> detected by {0}<b>Tracer</b>{1}{2}{3}", colourNeutral, colourEnd, "\n", "\n");
        builder.AppendFormat("<b>Rebel HQ</b> suspect a {0}<b>serious threat</b>{1} to your person{2}{3}", colourBad, colourEnd, "\n", "\n");
        builder.AppendFormat("At <b>{0}, {1}{2}{3}</b> district", node.nodeName, colourAlert, node.Arc.name, colourEnd);
        //only add a time stamp if nemesis is capable of multiple moves within a single turn
        if (nemesis.movement > 1)
        { builder.AppendFormat(" at <b>{0}</b> hrs", GetContactTime(moveNumber)); }
        return builder.ToString();
    }

    /// <summary>
    /// A resistance tracer detects the presence of a team (Erasure)
    /// </summary>
    /// <param name="node"></param>
    /// <param name="team"></param>
    /// <returns></returns>
    public string GetTracerTeamSpottedDetails(Node node, Team team)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>{0} Team</b> has been detected by one of your {1}<b>Tracers</b>{2}{3}{4}", team.arc.name, colourNeutral, colourEnd, "\n", "\n");
        builder.AppendFormat("<b>Rebel HQ</b> warn of {0}<b>danger to yourself</b>{1} and advise that you avoid the district{2}{3}", colourBad, colourEnd, "\n", "\n");
        builder.AppendFormat("At <b>{0}, {1}{2}{3}</b> district", node.nodeName, colourAlert, node.Arc.name, colourEnd);
        return builder.ToString();
    }

    /// <summary>
    /// A resistance tracer detectst the presence of an Npc
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public string GetTracerNpcSpottedDetails(Npc npc)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>The {0}{1}{2}</b> has been detected by one of your {3}<b>Tracers</b>{4}{5}{6}", colourAlert, npc.tag, colourEnd, colourNeutral, colourEnd, "\n", "\n");
        builder.AppendFormat("<b>Rebel HQ</b> remind you of the need to {0}<b>{1}</b>{2}{3}{4}", colourNeutral, npc.action.activity, colourEnd, "\n", "\n");
        builder.AppendFormat("At <b>{0}, {1}{2}{3}</b> district", npc.currentNode.nodeName, colourAlert, npc.currentNode.Arc.name, colourEnd);
        return builder.ToString();
    }

    /// <summary>
    /// Nemesis Ongoing effect status (displayed every turn Nemesis is onMap)
    /// </summary>
    /// <param name="nemesis"></param>
    /// <returns></returns>
    public string GetNemesisOngoingEffectDetails(Nemesis nemesis, int search)
    {
        StringBuilder builder = new StringBuilder();
        //colour code invis detection threshold based on search rating
        string colourSearch = colourNeutral;
        switch (search)
        {
            case 3: colourSearch = colourBad; break;
            case 2: colourSearch = colourNeutral; break;
            case 1: colourSearch = colourGood; break;
            default: Debug.LogWarningFormat("Invalid nemesis Search rating \"{0}\"", search); break;
        }
        builder.AppendFormat("<b>{0}</b> Nemesis is{1}", nemesis.name, "\n");
        NemesisMode mode = GameManager.instance.nemesisScript.GetMode();
        switch (mode)
        {
            case NemesisMode.Inactive:
                builder.AppendFormat("{0}<b>INACTIVE</b>{1}{2}", colourAlert, colourEnd, "\n");
                builder.AppendFormat("{0}{1}<b>You are currently in no danger</b>{2}", "\n", colourGood, colourEnd);
                break;
            case NemesisMode.NORMAL:
                builder.AppendFormat("in {0}<b>NORMAL</b>{1} mode{2}", colourNeutral, colourEnd, "\n");
                if (GameManager.instance.nemesisScript.CheckNemesisAmbush() == true)
                { builder.AppendFormat("lurking in {0}<b>AMBUSH</b>{1}{2}", colourBad, colourEnd, "\n"); }
                builder.AppendFormat("{0}Nemesis will find you if in the {1}{2}<b>SAME District</b>{3}{4}and your <b>Invisibility</b> is {5}{6}<b>{7}, or less<b>{8}{9}", "\n", "\n", colourAlert, colourEnd, "\n",
                     "\n", colourSearch, search, colourEnd, "\n");
                builder.AppendFormat("{0}Can move {1}<b>{2}</b>{3} District{4} a day", "\n", colourNeutral, nemesis.movement, colourEnd, nemesis.movement != 1 ? "s" : "", "\n");
                break;
            case NemesisMode.HUNT:
                builder.AppendFormat("in {0}<b>HUNT</b>{1} mode{2}", colourBad, colourEnd, "\n");
                if (GameManager.instance.nemesisScript.CheckNemesisSearch() == true)
                { builder.AppendFormat("actively {0}<b>SEARCHING</b>{1}{2}", colourBad, colourEnd, "\n"); }
                builder.AppendFormat("{0}Nemesis will find you if in the {1}{2}<b>SAME District</b>{3}{4}and your <b>Invisibility</b> is {5}{6}<b>{7}, or less<b>{8}{9}", "\n", "\n", colourAlert, colourEnd, "\n",
                     "\n", colourSearch, search, colourEnd, "\n");
                builder.AppendFormat("{0}Can move {1}<b>{2}</b>{3} District{4} a day", "\n", colourNeutral, nemesis.movement, colourEnd, nemesis.movement != 1 ? "s" : "", "\n");
                break;
            default:
                Debug.LogWarningFormat("Invalid Nemesis mode \"{0}\"", mode);
                break;
        }
        return builder.ToString();
    }

    /// <summary>
    /// Generated whenever nemesis changes between Normal and Hunt modes
    /// </summary>
    /// <param name="nemesis"></param>
    /// <param name="search"></param>
    /// <returns></returns>
    public string GetNemesisNewModeDetails(Nemesis nemesis, int search)
    {
        StringBuilder builder = new StringBuilder();
        //find on invis 1 or 2, colourNeutral, if 3 or less, colourBad
        string colourSearch = colourNeutral;
        if (search == 3) { colourSearch = colourBad; }
        builder.AppendFormat("<b>{0}</b> Nemesis changes{1}", nemesis.name, "\n");
        NemesisMode mode = GameManager.instance.nemesisScript.GetMode();
        switch (mode)
        {
            case NemesisMode.NORMAL:
                builder.AppendFormat("to {0}<b>NORMAL</b>{1} mode{2}", colourNeutral, colourEnd, "\n");
                builder.AppendFormat("{0}Nemesis will find you if in the {1}{2}<b>SAME District</b>{3}{4}and your <b>Invisibility</b> is {5}{6}<b>{7}, or less<b>{8}{9}", "\n", "\n", colourAlert, colourEnd, "\n",
                       "\n", colourSearch, search, colourEnd, "\n");
                break;
            case NemesisMode.HUNT:
                builder.AppendFormat("to {0}<b>HUNT</b>{1} mode{2}", colourBad, colourEnd, "\n");
                builder.AppendFormat("{0}Nemesis will find you if in the {1}{2}<b>SAME District</b>{3}{4}and your <b>Invisibility</b> is {5}{6}<b>{7}, or less<b>{8}{9}", "\n", "\n", colourAlert, colourEnd, "\n",
                     "\n", colourSearch, search, colourEnd, "\n");
                break;
            /*case NemesisMode.Inactive:
                builder.AppendFormat("to {0}<b>INACTIVE</b>{1} mode{2}", colourNeutral, colourEnd, "\n");
                builder.AppendFormat("{0}<b>Nemesis is currently not a threat</b>{1}", "\n", "\n");
                break;*/
            default:
                Debug.LogWarningFormat("Invalid Nemesis mode \"{0}\"", mode);
                break;
        }
        return builder.ToString();
    }

    /// <summary>
    /// Status of Player control of Nemesis -> can be in charge, can be waiting for cooldown period, can be ready to go
    /// </summary>
    /// <param name="isPlayerControl"></param>
    /// <param name="coolDownTimer"></param>
    /// <param name="controlTimer"></param>
    /// <returns></returns>
    public string GetNemesisPlayerOngoingDetails(Nemesis nemesis, bool isPlayerControl, int coolDownTimer, int controlTimer, Node controlNode, Node currentNode)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<b>{1}{2} nemesis</b>{3}{4}", colourNeutral, nemesis.name, colourEnd, "\n", "\n");
        if (GameManager.instance.nemesisScript.GetMode() != NemesisMode.Inactive)
        {
            if (isPlayerControl == true)
            {
                builder.AppendFormat("<b>CONTROL</b> remaining for {0}<b>{1} turn{2}</b>{3}", colourNeutral, controlTimer, controlTimer != 1 ? "s" : "", colourEnd);
                if (controlNode != null)
                {
                    builder.AppendFormat("{0}{1}Nemesis ordered to{2}<b>{3}, {4}{5}</b>{6} district{7}{8}<b>{9}</b>{10} on arrival <b>(Show)</b>", "\n", "\n", "\n", controlNode.nodeName, colourAlert, controlNode.Arc.name,
                        colourEnd, "\n", colourNeutral, GameManager.instance.nemesisScript.GetGoal(true), colourEnd);
                }
                if (currentNode != null)
                { builder.AppendFormat("{0}{1}Currently at{2}<b>{3}, {4}{5}</b>{6}, district", "\n", "\n", "\n", currentNode.nodeName, colourAlert, currentNode.Arc.name, colourEnd); }
                else { Debug.LogWarning("Invalid node (Null) for Player Controlled Nemesis"); }
            }
            else
            {
                if (coolDownTimer == 0)
                {
                    builder.AppendFormat("You can <b>TAKE CONTROL</b> at any time (<b>cost {0}{1} Renown</b>{2})", colourBad, GameManager.instance.nemesisScript.controlRenownCost, colourEnd);
                    builder.AppendFormat("{0}{1}The <b>AI</b> will {2}<b>automatically manage</b>{3} the nemesis otherwise", "\n", "\n", colourAlert, colourEnd);
                }
                else
                {
                    builder.AppendFormat("<b>Nemesis under {0}AI control<b>{1}{2}{3}", colourAlert, colourEnd, "\n", "\n");
                    builder.AppendFormat("You can take back control in {0}{1}<b>{2} turn{3}</b>{4}", "\n", colourNeutral, coolDownTimer, coolDownTimer != 1 ? "s" : "", colourEnd);
                }
            }
        }
        else
        {
            int duration = GameManager.instance.nemesisScript.GetDurationMode();
            builder.AppendFormat("Will <b>POWER UP<b> in{0}{1}<b>{2} turn{3}</b>{4}", "\n", colourNeutral, duration, duration != 1 ? "s" : "", colourEnd);
        }
        return builder.ToString();
    }

    /*/// <summary>
    /// subMethod to return a colour formatted string based on contact's effectiveness
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    private string GetConfidenceLevel(int level)
    {
        string confidenceLevel = "Unknown";
        switch(level)
        {
            case 3: confidenceLevel = string.Format("{0}Confidence Level High{1}", colourGood, colourEnd); break;
            case 2: confidenceLevel = string.Format("{0}Confidence Level Medium{1}", colourNeutral, colourEnd); break;
            case 1: confidenceLevel = string.Format("{0}Confidence Level Low{1}", colourBad, colourEnd); break;
        }
        return confidenceLevel;
    }*/

    //
    // - - - Gear - - -
    //

    /// <summary>
    /// Gear Used. 
    /// Note: Gear checked for null by calling method, MessageManager.cs -> GearUsed
    /// </summary>
    /// <param name="gear"></param>
    /// <returns></returns>
    public string GetGearUsedDetails(Gear gear)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<b>{1}</b>{2} gear{3}", colourNeutral, gear.tag, colourEnd, "\n");
        builder.AppendFormat("{0}{1}{2}", gear.reasonUsed, "\n", "\n");
        int turnsOwned = GameManager.instance.turnScript.Turn - gear.statTurnObtained;
        builder.AppendFormat("Owned for {0}{1}{2} turn{3}{4}", colourNeutral, turnsOwned, colourEnd, turnsOwned != 1 ? "s" : "", "\n");
        builder.AppendFormat("Used {0}{1}{2} time{3}", colourNeutral, gear.statTimesUsed, colourEnd, gear.statTimesUsed != 1 ? "s" : "");
        return builder.ToString();
    }

    /// <summary>
    /// Gear Compromised -> Saved or Compromised depending on renownUsed (saved if > 0)
    /// </summary>
    /// <param name="gear"></param>
    /// <param name="renownUsed"></param>
    /// <returns></returns>
    public string GetGearCompromisedDetails(Gear gear, int renownUsed)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<b>{1}</b>{2} gear{3}", colourNeutral, gear.tag, colourEnd, "\n");
        if (renownUsed > 0)
        {
            builder.AppendFormat("has been {0}<b>SAVED</b>{1}{2}{3}", colourGood, colourEnd, "\n", "\n");
            builder.AppendFormat("{0}<b>{1}</b>{2} Renown used", colourNeutral, renownUsed, colourEnd);
        }
        else
        {
            builder.AppendFormat("has been {0}<b>COMPROMISED</b>{1}{2}{3}", colourAlert, colourEnd, "\n", "\n");
            builder.AppendFormat("{0}<b>Gear has been LOST</b>{1}", colourBad, colourEnd);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Gear Lost -> either by actor losing it or by you giving gear to an actor who already has gear (isGivenToHQ 'true')
    /// </summary>
    /// <param name="gear"></param>
    /// <param name="actor"></param>
    /// <param name="isGivenToHQ"></param>
    /// <returns></returns>
    public string GetGearLostDetails(Gear gear, Actor actor, bool isGivenToHQ)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<b>{1}</b>{2} gear{3}", colourNeutral, gear.tag, colourEnd, "\n");
        if (isGivenToHQ == true)
        {
            //gear lost 'cause you overloaded the actor with gear
            builder.AppendFormat("{0}Donated to Faction HQ{1}{2}{3}", colourBad, colourEnd, "\n", "\n");
            builder.AppendFormat("by <b>{0}, {1}{2}</b>{3}{4}{5}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", "\n");
            builder.AppendFormat("Subordinates can only have{0}{1}one item of Gear{2}", "\n", colourAlert, colourEnd);
        }
        else
        {
            //gear lost through actor doing so
            builder.AppendFormat("{0}Lost, sold or Used{1}{2}{3}", colourBad, colourEnd, "\n", "\n");
            builder.AppendFormat("by {0}, {1}{2}{3}{4}{5}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", "\n");
            builder.AppendFormat("Subordinates won't hold gear {0}forever{1}", colourAlert, colourEnd);
        }
        return builder.ToString();
    }

    /// <summary>
    /// Gear Available -> you can recover gear from an actor if need be
    /// </summary>
    /// <param name="gear"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    public string GetGearAvailableDetails(Gear gear, Actor actor)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<b>{1}</b>{2} gear{3}", colourNeutral, gear.tag, colourEnd, "\n");
        builder.AppendFormat("{0}Can be acquired{1}{2}", colourGood, colourEnd, "\n");
        builder.AppendFormat("from <b>{0}, {1}{2}</b>{3}{4}{5}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", "\n");
        builder.AppendFormat("Asking for your gear back {0}may{1} upset {2}", colourBad, colourEnd, actor.actorName);
        return builder.ToString();
    }


    /// <summary>
    /// Gear Taken or Given
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="gear"></param>
    /// <param name="motivation"></param>
    /// <param name="isGiven"></param>
    /// <returns></returns>
    public string GetGearTakeOrGiveDetails(Actor actor, Gear gear, int motivation, bool isGiven)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<b>{1}</b>{2} gear{3}", colourNeutral, gear.tag, colourEnd, "\n");
        if (isGiven == true)
        {
            //gear given
            builder.AppendFormat("has been given{0}", "\n");
            builder.AppendFormat("to <b>{0}, {1}{2}</b>{3}{4}{5}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", "\n");
            builder.AppendFormat("{0}{1} <b>Motivation +{2}</b>{3}", actor.actorName, colourGood, motivation, colourEnd);
        }
        else
        {
            //gear Taken back from actor
            builder.AppendFormat("has been taken back{0}", "\n");
            builder.AppendFormat("from {0}, {1}{2}{3}{4}{5}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", "\n");
            builder.AppendFormat("{0} {1}<b>Motivation -{2}</b>{3}", actor.actorName, colourBad, motivation, colourEnd);
        }
        return builder.ToString();
    }

    /// <summary>
    /// Gear Obtained by player or actor
    /// </summary>
    /// <param name="gear"></param>
    /// <param name="node"></param>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public string GetGearObtainedDetails(Gear gear, Node node, int actorID)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<b>{1}</b>{2} gear{3}", colourNeutral, gear.tag, colourEnd, "\n");
        if (actorID != 999)
        {
            //actor got gear
            Actor actor = GameManager.instance.dataScript.GetActor(actorID);
            if (actor != null)
            {
                builder.AppendFormat("Sourced by <b>{0}, {1}{2}</b>{3}{4}{5}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", "\n");
                builder.AppendFormat("at <b>{0}, {1}{2}</b>{3}, district", node.nodeName, colourAlert, node.Arc.name, colourEnd);
            }
            else
            {
                Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actorID);
                builder.Append("Unknown");
            }
        }
        else
        {
            //player got gear
            builder.AppendFormat("Sourced by <b>{0}, {1}PLAYER</b>{2}{3}{4}", GameManager.instance.playerScript.PlayerName, colourAlert, colourEnd, "\n", "\n");
            builder.AppendFormat("at <b>{0}, {1}{2}</b>{3} district", node.nodeName, colourAlert, node.Arc.name, colourEnd);
        }
        return builder.ToString();
    }

    //
    // - - - Faction - - -
    //

    /// <summary>
    /// returns a colour formatted string for ItemData string Message
    /// NOTE: faction has been checked for null by the calling method: MessageManager.cs -> FactionSupport
    /// </summary>
    /// <param name="supportGiven"></param>
    /// <returns></returns>
    public string GetFactionSupportDetails(Faction faction, int factionApproval, int supportGiven)
    {
        StringBuilder builder = new StringBuilder();
        if (supportGiven > 0)
        {
            //support approved
            builder.AppendFormat("{0} HQ have agreed to your request for support{1}{2}", faction.tag, "\n", "\n");
            builder.AppendFormat("{0}<b>Renown +{1}</b>{2}{3}{4}", colourGood, supportGiven, colourEnd, "\n", "\n");
            builder.AppendFormat("{0}<b>{1}% chance of Approval</b>{2}{3}Faction Approval <b>{4}</b> out of <b>{5}</b>", colourNeutral, factionApproval * 10, colourEnd, "\n",
                factionApproval, GameManager.instance.factionScript.maxFactionApproval);
        }
        else
        {
            //support declined
            builder.AppendFormat("{0} HQ couldn't agree{1}{2}{3}<b>No Support provided</b>{4}{5}{6}", faction.tag, "\n", "\n", colourBad, colourEnd, "\n", "\n");
            builder.AppendFormat("{0}<b>{1}% chance of Approval</b>{2}{3}Faction Approval <b>{4} out of <b>{5}</b>", colourNeutral, factionApproval * 10, colourEnd, "\n",
                factionApproval, GameManager.instance.factionScript.maxFactionApproval);
        }
        return builder.ToString();
    }

    /// <summary>
    /// Faction Approval level changes
    /// </summary>
    /// <param name="faction"></param>
    /// <param name="reason"></param>
    /// <param name="oldLevel"></param>
    /// <param name="change"></param>
    /// <param name="newLevel"></param>
    /// <returns></returns>
    public string GetFactionApprovalDetails(Faction faction, string reason, int change, int newLevel)
    {
        StringBuilder builder = new StringBuilder();
        if (change > 0)
        { builder.AppendFormat("{0} {1}Approval {2}<b>+{3}</b>{4}, now {5}<b>{6}</b>{7}{8}{9}", faction.tag, "\n", colourGood, change, colourEnd, colourNeutral, newLevel, colourEnd, "\n", "\n"); }
        else { builder.AppendFormat("{0} faction{1}Approval {2}<b>{3}</b>{4}, now {5}<b>{6}</b>{7}{8}{9}", faction.tag, "\n", colourBad, change, colourEnd, colourNeutral, newLevel, colourEnd, "\n", "\n"); }
        if (string.IsNullOrEmpty(reason) == false)
        {
            builder.AppendFormat("<b>Because of</b>{0}", "\n");
            if (change > 0)
            { builder.AppendFormat("{0}<b>{1}</b>{2}", colourGood, reason, colourEnd); }
            else { builder.AppendFormat("{0}<b>{1}</b>{2}", colourBad, reason, colourEnd); }
        }
        return builder.ToString();
    }


    //
    // - - - Secrets - - -
    //

    /// <summary>
    /// returns a colour formatted string for ItemData string message (Player Secret)
    /// NOTE: Secret checked for null by calling method: MessageManager.cs -> PlayerSecret
    /// </summary>
    /// <param name="secret"></param>
    /// <returns></returns>
    public string GetPlayerSecretDetails(Secret secret, bool isGained)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<b>{1}</b>{2}{3}{4}", colourAlert, secret.descriptor, colourEnd, "\n", "\n");
        if (isGained == true)
        {
            //secret gained
            builder.Append("<b>Effects if Secret revealed</b>");
            GetSecretEffects(builder, secret);
            builder.AppendFormat("{0}{1}{2}<b>Nobody</b>{3} currently knows this secret", "\n", "\n", colourNeutral, colourEnd);
        }
        else
        {
            //secret lost
            builder.AppendFormat("Your secret is gone and all those who know it have {0}forgotten{1} about it", colourNeutral, colourEnd);
        }
        return builder.ToString();
    }

    /// <summary>
    /// returns a colour formatted string for ItemData string message (Actor Secret)
    /// NOTE: Actor and secret checked for Null by calling method: MessageManager.cs -> ActorSecret
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="secret"></param>
    /// <param name="isGained"></param>
    /// <returns></returns>
    public string GetActorSecretDetails(Actor actor, Secret secret, bool isGained)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<b>{1}</b>{2}{3}{4}", colourAlert, secret.descriptor, colourEnd, "\n", "\n");
        if (isGained == true)
        {
            //secret gained
            builder.Append("<b>Effects if Secret revealed</b>");
            GetSecretEffects(builder, secret);
            builder.AppendFormat("{0}{1}Unless {2}<b>provoked</b>{3}, {4} will keep your secret", "\n", "\n", colourNeutral, colourEnd, actor.actorName);
        }
        else
        {
            //secret lost
            builder.AppendFormat("{0} can {1}no longer remember{2} the details of your secret", actor.actorName, colourNeutral, colourEnd);
        }
        return builder.ToString();
    }

    /// <summary>
    /// Whenever secret revealed, shows reason why and specific effects
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="secret"></param>
    /// <param name="secretEffect"></param>
    /// <returns></returns>
    public string GetActorRevealSecretDetails(Actor actor, Secret secret, string reason)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>{0}, {1}{2}</b>{3}", actor.actorName, colourAlert, actor.arc.name, colourEnd);
        if (string.IsNullOrEmpty(reason) == false)
        { builder.AppendFormat("{0}{1}<b>{2}</b>{3}", "\n", colourBad, reason, colourEnd); }
        builder.AppendFormat("{0}{1}<b>Secret Revealed</b>", "\n", "\n");
        GetSecretEffects(builder, secret);
        return builder.ToString();
    }

    /// <summary>
    /// subMethod to add Secret effects to builder string (colour Formatted) for GetPlayerSecretDetails and GetActorSecretDetails
    /// NOTE: secret checked for null by calling method
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="secret"></param>
    /// <returns></returns>
    private StringBuilder GetSecretEffects(StringBuilder builder, Secret secret)
    {
        List<Effect> listOfEffects = secret.GetListOfEffects();
        if (listOfEffects != null)
        {
            if (listOfEffects.Count > 0)
            {
                foreach (Effect effect in listOfEffects)
                { builder.AppendFormat("{0}{1}<b>{2}</b>{3}", "\n", colourBad, effect.description, colourEnd); }
            }
            else { builder.AppendFormat("{0}No effect", "\n"); }
        }
        else { Debug.LogWarningFormat("Invalid listOfEffects (Null) for secret {0}", secret.name); }
        return builder;
    }

    //
    // - - - AI - - -
    //

    /// <summary>
    /// AI traceback
    /// </summary>
    /// <param name="nodeID"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    public string GetAIDetectedDetails(int nodeID, int delay)
    {
        StringBuilder builder = new StringBuilder();
        string playerNameResistance = GameManager.instance.playerScript.GetPlayerNameResistance();
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        if (node != null)
        {
            builder.AppendFormat("AI Traceback countermeasures{0}has {1}<b>DETECTED</b>{2} {3}{4}{5}", "\n", colourBad, colourEnd, playerNameResistance, "\n", "\n");
            if (delay > 0)
            { builder.AppendFormat("{0}'s location known in{1}{2}<b>{3}</b>{4} turn{5}{6}{7}", playerNameResistance, "\n", colourNeutral, delay, colourEnd, delay != 1 ? "s" : "", "\n", "\n"); }
            builder.AppendFormat("{0}{1}'s Invisibility -1{2}", colourBad, playerNameResistance, colourEnd);
        }
        else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", nodeID); builder.Append("Unknown"); }
        return builder.ToString();
    }

    /// <summary>
    /// AI detects resistance activity on a Connection
    /// </summary>
    /// <param name="destinationNode"></param>
    /// <param name="connection"></param>
    /// <returns></returns>
    public string GetAIConnectionActivityDetails(Node destinationNode, int delay)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}, {1}Resistance Leader{2}{3}", GameManager.instance.playerScript.GetPlayerNameResistance(), colourAlert, colourEnd, "\n");
        builder.AppendFormat("{0}<b>DETECTED</b>{1}{2}{3}", colourGood, colourEnd, "\n", "\n");
        builder.AppendFormat("On Connection travelling to{0}{1}{2}, {3}{4}{5}{6}", "\n", colourNeutral, destinationNode.nodeName, destinationNode.Arc.name, colourEnd, "\n", "\n");
        builder.AppendFormat("{0}Detected {1}{2}<b>{3}{4}</b>{5} turns ago{6}", colourAlert, colourEnd, colourNeutral, delay, colourEnd, colourAlert, colourEnd);
        return builder.ToString();
    }

    /// <summary>
    /// AI detects resistance activity at a district
    /// </summary>
    /// <param name="node"></param>
    /// <param name="actorID"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    public string GetAINodeActivityDetails(Node node, int delay)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("Resistance activity {0}", "\n");
        builder.AppendFormat("{0}<b>DETECTED</b>{1} at{2}", colourGood, colourEnd, "\n");
        builder.AppendFormat("{0}{1}, {2}{3}{4}{5}", colourNeutral, node.nodeName, node.Arc.name, colourEnd, "\n", "\n");
        builder.AppendFormat("{0}Detected {1}{2}<b>{3}{4}</b>{5} turns ago{6}", colourAlert, colourEnd, colourNeutral, delay, colourEnd, colourAlert, colourEnd);
        return builder.ToString();
    }

    /// <summary>
    /// Resistance leader invis < zero with immediate detection
    /// </summary>
    /// <param name="reason"></param>
    /// <param name="nodeID"></param>
    /// <param name="connID"></param>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public string GetAIImmediateActivityDetails(string reason, int nodeID, int connID, string actorName, string actorArcName)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>{0}, {1}{2}</b>{3}{4}", actorName, colourAlert, actorArcName, colourEnd, "\n");
        builder.AppendFormat("{0}<b>CURRENT LOCATION KNOWN</b>{1}{2}", colourBad, colourEnd, "\n");
        if (string.IsNullOrEmpty(reason) == false)
        { builder.AppendFormat("due to {0}{1}{2}{3}", colourNeutral, reason, colourEnd, "\n"); }
        if (nodeID > 0)
        {
            Node node = GameManager.instance.dataScript.GetNode(nodeID);
            if (node != null)
            {
                builder.AppendFormat("{0}Spotted at <b>{1}, {2}{3}</b>{4}{5}{6}", "\n", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n", "\n");
                builder.AppendFormat("{0}{1}'s Invisibility dropped <b>below Zero</b>{2}", colourNeutral, actorName, colourEnd);
            }
            else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", nodeID); }
        }
        else if (connID > 0)
        {
            Connection conn = GameManager.instance.dataScript.GetConnection(connID);
            if (conn != null)
            {
                builder.AppendFormat("{0}Spotted moving between <b>{1}, {2}{3}</b>{4} and <b>{5}, {6}{7}</b>{8}{9}{10}", "\n", conn.node1.nodeName, colourAlert, conn.node1.Arc.name, colourEnd,
                    conn.node2.nodeName, colourAlert, conn.node2.Arc.name, colourEnd, "\n", "\n");
                builder.AppendFormat("{0}{1}'s Invisibility dropped <b>below Zero</b>{2}", colourNeutral, actorName, colourEnd);
            }
            else { Debug.LogWarningFormat("Invalid connection (Null) for connID {0}", connID); }
        }
        return builder.ToString();
    }

    /// <summary>
    /// actor or player were captured
    /// </summary>
    /// <param name="actorName"></param>
    /// <param name="actorArcName"></param>
    /// <param name="node"></param>
    /// <param name="team"></param>
    /// <returns></returns>
    public string GetActorCaptureDetails(string actorName, string actorArcName, Node node, Team team)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}, {1}<b>{2}</b>{3}{4}", actorName, colourAlert, actorArcName, colourEnd, "\n");
        builder.AppendFormat("{0}<b>has been CAPTURED</b>{1}{2}{3}", colourBad, colourEnd, "\n", "\n");
        builder.AppendFormat("at <b>{0}, {1}</b> district{2}by {3}<b>{4} {5}</b>{6}", node.nodeName, node.Arc.name, "\n", colourNeutral, team.arc.name, team.teamName, colourEnd);
        return builder.ToString();
    }



    /// <summary>
    /// actor or player have been released from captivity
    /// </summary>
    /// <param name="actorName"></param>
    /// <param name="actorArcName"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public string GetActorReleaseDetails(string actorName, string actorArcName, Node node)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}, {1}<b>{2}</b>{3}{4}", actorName, colourAlert, actorArcName, colourEnd, "\n");
        builder.AppendFormat("{0}<b>has been RELEASED</b>{1}{2}{3}", colourGood, colourEnd, "\n", "\n");
        builder.AppendFormat("at <b>{0}, {1}{2}</b>{3}{4}{5}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n", "\n");
        builder.AppendFormat("{0}<b>{1} is under a cloud of suspicion as a result of their time with the Authority</b>{2}", colourBad, actorName, colourEnd);
        return builder.ToString();
    }

    /// <summary>
    /// AI hacked, detected or not
    /// </summary>
    /// <param name="text"></param>
    /// <param name="currentRenownCost"></param>
    /// <param name="isDetected"></param>
    /// <returns></returns>
    public string GetAIHackedDetails(bool isDetected, int attemptsDetected, int attemptsTotal)
    {
        StringBuilder builder = new StringBuilder();
        if (isDetected == true)
        {
            builder.Append("AI hacked by Resistance");
            builder.AppendFormat("{0}{1}<b>DETECTED</b>{2}{3}{4}", "\n", colourBad, colourEnd, "\n", "\n");
            string colourAlertStatus = colourGood;
            //alert status coloured good/neutral/bad depending on low / med / high priority
            switch (GameManager.instance.aiScript.aiAlertStatus)
            {
                case Priority.High: colourAlertStatus = colourBad; break;
                case Priority.Medium: colourAlertStatus = colourNeutral; break;
            }
            builder.AppendFormat("AI Alert status now {0}<b>{1}</b>{2}", colourAlertStatus, GameManager.instance.aiScript.aiAlertStatus, colourEnd);
        }
        else
        {
            builder.AppendFormat("AI hacked by Resistance{0}", "\n");
            builder.AppendFormat("{0}Undetected{1}{2}{3}", colourGood, colourEnd, "\n", "\n");
            builder.AppendFormat("{0}No Change to AI Alert Status{1}", colourNeutral, colourEnd);
        }
        //attempts (same for both)
        builder.AppendFormat("{0}{1}AI has detected {2}<b>{3}</b>{4} of {5}<b>{6}</b>{7} attempts", "\n", "\n", colourNeutral, attemptsDetected, colourEnd, colourNeutral, attemptsTotal, colourEnd);
        return builder.ToString();
    }

    /// <summary>
    /// AI reboots, if rebootTimer > 0 then commences, otherwise completes Reboot
    /// </summary>
    /// <param name="rebootTimer"></param>
    /// <returns></returns>
    public string GetAIRebootDetails(int rebootTimer, int currentRenownCost)
    {
        StringBuilder builder = new StringBuilder();
        if (rebootTimer > 0)
        {
            //Commenced
            builder.AppendFormat("AI Reboot {0}<b>COMMENCED</b>{1}{2}{3}AI is Offline{4}{5}{6}", colourNeutral, colourEnd, "\n", colourBad, colourEnd, "\n", "\n");
            builder.AppendFormat("AI Returns in {0}<b>{1}</b>{2} turn{3}{4}{5}", colourNeutral, rebootTimer, colourEnd, rebootTimer != 1 ? "s" : "", "\n", "\n");
            builder.Append("Any existing AI Countermeasures are cancelled");
        }
        else
        {
            //Completed
            builder.AppendFormat("AI Reboot {0}<b>COMPLETED</b>{1}{2}{3}AI is Online{4}{5}{6}", colourNeutral, colourEnd, "\n", colourGood, colourEnd, "\n", "\n");
            builder.AppendFormat("Cost to hack AI now{0}{1}{2} Renown{3}{4}{5}", "\n", colourNeutral, currentRenownCost, colourEnd, "\n", "\n");
            builder.AppendFormat("AI Alert Status {0}<b>Low</b>{1}", colourGood, colourEnd);
        }
        return builder.ToString();
    }

    /// <summary>
    /// AI implements countermeasures to counter hacking
    /// </summary>
    /// <param name="warning"></param>
    /// <param name="protocolLevelNew"></param>
    /// <returns></returns>
    public string GetAICounterMeasureDetails(string warning, int duration, int protocolLevelNew)
    {
        StringBuilder builder = new StringBuilder();
        if (string.IsNullOrEmpty(warning) == false)
        {
            string colourSide = colourBad;
            if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level)
            { colourSide = colourGood; }
            builder.AppendFormat("{0}<b>{1}</b>{2}", colourSide, warning, colourEnd);
        }
        if (duration > 0)
        { builder.AppendFormat("{0}{1}<b>Duration {2}{3} turn{4}</b>{5}", "\n", "\n", colourNeutral, duration, duration != 1 ? "s" : "", colourEnd); }
        if (protocolLevelNew > 0)
        { builder.AppendFormat("{0}{1}{2} AI Security Protocol now <b>Level {3}</b>{4}", "\n", "\n", colourNeutral, protocolLevelNew, colourEnd); }
        return builder.ToString();
    }

    //
    // - - - Decisions - - -
    //

    /// <summary>
    /// city wide decision, eg. Surveillane crackdown
    /// </summary>
    /// <param name="descriptor"></param>
    /// <param name="warning"></param>
    /// <returns></returns>
    public string GetDecisionGlobalDetails(string description, int duration, int loyaltyAdjust, int crisisAdjust)
    {
        StringBuilder builder = new StringBuilder();
        //side specific colours, default POV Resistance
        string colourStart = colourBad;
        string colourFinish = colourGood;
        string colourCrisis = colourBad;
        if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level)
        { colourStart = colourBad; colourFinish = colourGood; colourCrisis = colourGood; }
        //text
        if (string.IsNullOrEmpty(description) == false)
        { builder.AppendFormat("{0}", description); }
        if (duration > 0)
        { builder.AppendFormat("{0}{1}<b>Duration {2}{3} turn{4}</b>{5}", "\n", "\n", colourNeutral, duration, duration != 1 ? "s" : "", colourEnd); }
        if (loyaltyAdjust != 0)
        { builder.AppendFormat("{0}{1}{2}<b>City Loyalty {3}{4}</b>{5}", "\n", "\n", loyaltyAdjust > 0 ? colourStart : colourFinish, loyaltyAdjust > 0 ? "+" : "", loyaltyAdjust, colourEnd); }
        if (crisisAdjust != 0)
        { builder.AppendFormat("{0}{1}{2}<b>District Crisis {3}% less likely</b>{4}", "\n", "\n", colourCrisis, crisisAdjust, colourEnd); }
        return builder.ToString();
    }


    /// <summary>
    /// connection security changes
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="secLevel"></param>
    /// <returns></returns>
    public string GetDecisionConnectionDetails(Connection connection, ConnectionType secLevel)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("Connection between {0}<b>{1}</b>{2} and {3}<b>{4}</b>{5} districts", colourNeutral, connection.node1.nodeName, colourEnd, colourNeutral, connection.node2.nodeName, colourEnd);
        builder.AppendLine(); builder.AppendLine();
        string colourSecurity = colourBad;
        if (secLevel == ConnectionType.MEDIUM) { colourSecurity = colourNeutral; }
        else if (secLevel == ConnectionType.LOW) { colourSecurity = colourGood; }
        builder.AppendFormat("Security level now {0}<b>{1}</b>{2}", colourSecurity, secLevel, colourEnd);
        return builder.ToString();
    }

    /// <summary>
    /// Ongoing decision effect
    /// </summary>
    /// <param name=""></param>
    /// <param name=""></param>
    /// <returns></returns>
    public string GetDecisionOngoingEffectDetails(string topText, string middleText, string bottomText)
    {
        StringBuilder builder = new StringBuilder();
        if (string.IsNullOrEmpty(topText) == false)
        { builder.AppendFormat("<b>{0}</b>", topText, "\n", "\n"); }
        if (string.IsNullOrEmpty(middleText) == false)
        {
            if (builder.Length > 0)
            { builder.AppendLine(); builder.AppendLine(); }
            builder.Append(middleText);
        }
        if (string.IsNullOrEmpty(bottomText) == false)
        {
            if (builder.Length > 0)
            { builder.AppendLine(); builder.AppendLine(); }
            builder.AppendFormat("<b>{0}</b>", bottomText);
        }
        return builder.ToString();
    }

    //
    // - - - City - - -
    //

    /// <summary>
    /// city loyalty changes
    /// </summary>
    /// <param name="newCityLoyalty"></param>
    /// <param name="changeInLoyalty"></param>
    /// <returns></returns>
    public string GetCityLoyaltyDetails(string reason, int newCityLoyalty, int changeInLoyalty)
    {
        StringBuilder builder = new StringBuilder();
        string colourSideGood = colourGood;
        string colourSideBad = colourBad;
        //default colours from POV of authority, reverse for resistance
        if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level)
        { colourSideGood = colourBad; colourSideBad = colourGood; }
        if (changeInLoyalty > 0)
        {
            //loyalty increases
            builder.AppendFormat("{0}{1}<b>Loyalty now {2}{3}</b>{4}{5}{6}", GameManager.instance.cityScript.GetCityNameFormatted(), "\n", colourSideGood, newCityLoyalty, colourEnd, "\n", "\n");
            //change
            builder.AppendFormat("<b>Loyalty increased by {0}+{1}</b>{2}", colourSideGood, changeInLoyalty, colourEnd);
        }
        else
        {
            //loyalty decresed
            builder.AppendFormat("{0}{1}<b>Loyalty now {2}{3}</b>{4}{5}{6}", GameManager.instance.cityScript.GetCityNameFormatted(), "\n", colourSideBad, newCityLoyalty, colourEnd, "\n", "\n");
            //change
            builder.AppendFormat("<b>Loyalty decreased by {0}{1}</b>{2}", colourSideBad, changeInLoyalty, colourEnd);
        }
        //reason
        if (string.IsNullOrEmpty(reason) == false)
        { builder.AppendFormat("{0}{1}due to {2}<b>{3}</b>{4}", "\n", "\n", colourNeutral, reason, colourEnd); }
        return builder.ToString();
    }

    //
    // - - - Districts - - -
    //

    /// <summary>
    /// Node crisis starts, explodes, averted
    /// </summary>
    /// <param name="node"></param>
    /// <param name="reductionInCityLoyalty"></param>
    /// <returns></returns>
    public string GetNodeCrisisDetails(Node node, int reductionInCityLoyalty)
    {
        StringBuilder builder = new StringBuilder();
        //default POV Resistance side
        string colourSideGood = colourGood;
        string colourSideBad = colourBad;
        if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level)
        { colourSideGood = colourBad; colourSideBad = colourGood; }
        builder.AppendFormat("{0}, {1}<b>{2}</b>{3}{4}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n");
        if (node.crisisTimer > 0)
        {
            //crisis commences
            builder.AppendFormat("{0}<b>Crisis BEGINS</b>{1}{2}{3}", colourSideGood, colourEnd, "\n", "\n");
            builder.AppendFormat("{0}{1}{2}{3}{4}", colourNeutral, node.crisis.description, colourEnd, "\n", "\n");
            builder.AppendFormat("Crisis Explodes in {0}<b>{1} turn{2}</b>{3}{4}", colourNeutral, node.crisisTimer, node.crisisTimer != 1 ? "s" : "", colourEnd, "\n");
            builder.AppendFormat("{0}<b>City Loyalty -{1} if explodes</b>{2}", colourSideGood, GameManager.instance.nodeScript.crisisCityLoyalty, colourEnd);
        }
        else
        {
            if (node.crisis == null)
            {
                //crisis averted
                builder.AppendFormat("{0}Authority has contained the situation{1}{2}", colourAlert, colourEnd, "\n");
                builder.AppendFormat("{0}<b>Crisis AVERTED</b>{1}{2}{3}", colourSideBad, colourEnd, "\n", "\n");
                builder.AppendFormat("District immune to crisis for {0}<b>{1} turn{2}</b>{3}", colourNeutral, node.waitTimer, node.waitTimer != 1 ? "s" : "", colourEnd);
            }
            else
            {
                //crisis explodes
                builder.AppendFormat("{0}<b>Crisis EXPLODES</b>{1}{2}{3}", colourSideGood, colourEnd, "\n", "\n");
                builder.AppendFormat("{0}<b>{1}</b> crisis has spun out of control{2}{3}{4}", colourAlert, node.crisis.tag, colourEnd, "\n", "\n");
                builder.AppendFormat("{0}<b>City Loyalty -{1}</b>{2}", colourSideGood, reductionInCityLoyalty, colourEnd);
            }
        }
        return builder.ToString();
    }

    /// <summary>
    /// Node that is immune to a crisis for a period due to recent upheavals
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public string GetNodeOngoingEffectDetails(Node node)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}, {1}<b>{2}</b>{3}{4}{5}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n", "\n");
        builder.AppendFormat("{0}Is <b>Immune to Crisis</b> due to recent upheavals{1}{2}{3}", colourAlert, colourEnd, "\n", "\n");
        builder.AppendFormat("<b>Duration {0}{1} turn{2}</b>{3}", colourNeutral, node.waitTimer, node.waitTimer != 1 ? "s" : "", colourEnd);
        return builder.ToString();
    }

    //
    // - - - Targets - - -
    //


    /// <summary>
    /// New LIVE target popped up
    /// </summary>
    /// <param name="node"></param>
    /// <param name="target"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public string GetTargetNewDetails(Node node, Target target, string sideText, int sideLevel)
    {
        StringBuilder builder = new StringBuilder();
        if (string.IsNullOrEmpty(sideText) == true) { sideText = "Unknown"; }
        if (sideLevel == globalResistance.level)
        { builder.AppendFormat("An {0}<b>{1}</b>{2} exists at{3}", colourGood, sideText, colourEnd, "\n"); }
        else { builder.AppendFormat("{0}<b>{1}</b>{2} identified at{3}", colourBad, sideText, colourEnd, "\n"); }
        builder.AppendFormat("{0}, {1}<b>{2}</b>{3}{4}{5}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n", "\n");
        builder.AppendFormat("{0}<size=110%><b>{1}</b></size>{2}{3}", colourAlert, target.targetName, colourEnd, "\n");
        if (sideLevel == globalResistance.level)
        {
            builder.AppendFormat("{0}<b>{1}</b>{2}{3}", colourNeutral, target.descriptorResistance, colourEnd, "\n");
            builder.AppendFormat("{0}<b>{1} gear</b>{2} bonus{3}", colourRebel, target.gear.name, colourEnd, "\n");
            builder.AppendFormat("{0}<b>{1}</b>{2} bonus{3}{4}", colourRebel, target.actorArc.name, colourEnd, "\n", "\n");
        }
        else
        { builder.AppendFormat("{0}<b>{1}</b>{2}{3}{4}", colourNeutral, target.descriptorAuthority, colourEnd, "\n", "\n"); }
        builder.AppendFormat("{0} exists for {1}<b>{2}</b>{3} day{4}", sideText, colourNeutral, target.turnsWindow, colourEnd, target.turnsWindow != 1 ? "s" : "");
        return builder.ToString();
    }

    /// <summary>
    /// A target that has not been successfully attempt has expired (timed out)
    /// </summary>
    /// <param name="node"></param>
    /// <param name="target"></param>
    /// <param name="sideText"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public string GetTargetExpiredDetails(Node node, Target target, string sideText, int sideLevel)
    {
        StringBuilder builder = new StringBuilder();
        if (string.IsNullOrEmpty(sideText) == true) { sideText = "Unknown"; }
        if (sideLevel == globalResistance.level)
        { builder.AppendFormat("Expired {0}{1}{2} at{3}", colourGood, sideText, colourEnd, "\n"); }
        else { builder.AppendFormat("{0}<b>{1}</b>{2} has been resolved at{3}", colourBad, sideText, colourEnd, "\n"); }
        builder.AppendFormat("{0}, {1}<b>{2}</b>{3}{4}{5}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n", "\n");
        builder.AppendFormat("{0}<size=110%><b>{1}</b></size>{2}{3}{4}", colourAlert, target.targetName, colourEnd, "\n", "\n");
        builder.AppendFormat("{0}<b>{1}</b>{2} attempt{3} were made on Target{4}", colourNeutral, target.numOfAttempts, colourEnd, target.numOfAttempts != 1 ? "s" : "", "\n");
        builder.AppendFormat("Target was Live for {0}<b>{1}</b>{2} day{3}", colourNeutral, target.turnsWindow, colourEnd, target.turnsWindow != 1 ? "s" : "");
        return builder.ToString();
    }

    /// <summary>
    /// A target that has not been successfully attempted is about to expire
    /// </summary>
    /// <param name="node"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public string GetTargetExpiredWarningDetails(Node node, Target target)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<b>Opportunity</b>{1} at{2}", colourGood, colourEnd, "\n");
        builder.AppendFormat("{0}, {1}<b>{2}</b>{3}{4}{5}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n", "\n");
        builder.AppendFormat("{0}<size=110%><b>{1}</b></size>{2}{3}{4}", colourAlert, target.targetName, colourEnd, "\n", "\n");
        builder.AppendFormat("<b>Will be gone in</b>{0}", "\n");
        builder.AppendFormat("{0}<b>{1}{2} days</b>", colourNeutral, target.timerWindow, colourEnd);
        return builder.ToString();
    }

    /// <summary>
    /// Target attempt, successful or not, player or actor, Resistance message only
    /// </summary>
    /// <param name="node"></param>
    /// <param name="actorID"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public string GetTargetAttemptDetails(Node node, int actorID, Target target)
    {
        StringBuilder builder = new StringBuilder();
        string actorName = "Unknown";
        string actorArc = "Unknown";
        string targetEffects;
        builder.AppendFormat("{0}<b>{1}</b>{2}{3}", colourNeutral, target.targetName, colourEnd, "\n");
        builder.AppendFormat("<b>{0}, {1}{2}{3}</b>{4}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n");
        //who did it?
        if (actorID == 999)
        { actorName = GameManager.instance.playerScript.GetPlayerNameResistance(); actorArc = "Player"; }
        else
        {
            Actor actor = GameManager.instance.dataScript.GetActor(actorID);
            if (actor != null)
            { actorName = actor.actorName; actorArc = actor.arc.name; }
            else { Debug.LogWarningFormat("Invalid Actor (Null) for actorID {0}", actorID); }
        }
        if (target.targetStatus == Status.Live)
        {
            //failed
            builder.AppendFormat("{0}{1}<b>{2}, {3}{4}{5}</b>{6}attempt {7}<b>FAILED</b>{8}", "\n", "\n", actorName, colourAlert, actorArc, colourEnd, "\n", colourBad, colourEnd);
        }
        else
        {
            //success
            builder.AppendFormat("<b>{0}, {1}{2}{3}</b>{4}attempt {5}<b>SUCCEEDED</b>{6}{7}{8}", actorName, colourAlert, actorArc, colourEnd, "\n", colourGood, colourEnd, "\n", "\n");
            //effects
            targetEffects = GameManager.instance.targetScript.GetTargetEffects(target.name);
            builder.AppendFormat("<b>{0}</b>", targetEffects);
        }
        return builder.ToString();
    }


    /// <summary>
    /// Authority contains a target and cancels any ongoing effects
    /// </summary>
    /// <param name="node"></param>
    /// <param name="team"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public string GetTargetContainedDetails(Node node, Team team, Target target)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<b>{1}</b>{2}{3}", colourNeutral, target.targetName, colourEnd, "\n");
        builder.AppendFormat("{0}, {1}{2}{3}", node.nodeName, node.Arc.name, "\n", "\n");
        if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level)
        { builder.AppendFormat("<b>Authority has sealed off the situation{0}{1}Ongoing effects cancelled</b>{2}", "\n", colourBad, colourEnd); }
        else
        { builder.AppendFormat("{0}<b>{1}, {2}{3}, has sealed off the situation{4}{5}Ongoing effects cancelled</b>{6}", colourNeutral, team.arc.name, team.teamName, colourEnd, "\n", colourBad, colourEnd); }
        return builder.ToString();
    }


    //
    // - - - Effects (Active & Ongoing) - - -
    //

    /// <summary>
    /// Active effects for the InfoTab 'Effect' tab. Actor or Node or neither
    /// </summary>
    /// <param name="detailsTop"></param>
    /// <param name="detailsBottom"></param>
    /// <param name="actorID"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public string GetActiveEffectDetails(string detailsTop, string detailsBottom, int actorID, Node node)
    {
        StringBuilder builder = new StringBuilder();
        if (actorID > -1)
        {
            //actor
            if (actorID == 999)
            {
                //player
                builder.AppendFormat("<b>{0}, {1}PLAYER</b>{2}{3}{4}", GameManager.instance.playerScript.PlayerName, colourAlert, colourEnd, "\n", "\n");
            }
            else
            {
                Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    //actor
                    builder.AppendFormat("<b>{0}, {1}{2}</b>{3}{4}{5}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", "\n");
                }
                else { Debug.LogWarningFormat("Invalid Actor (Null) for actorID {0}", actorID); }
            }
        }
        else if (node != null)
        {
            //node
            builder.AppendFormat("<b>{0}, {1}{2}</b>{3}{4}{5}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n", "\n");
        }
        //details
        if (string.IsNullOrEmpty(detailsTop) == false)
        { builder.AppendFormat("<b>{0}</b>", detailsTop); }
        if (string.IsNullOrEmpty(detailsBottom) == false)
        { builder.AppendFormat("{0}{1}<b>{2}</b>", "\n", "\n", detailsBottom); }
        return builder.ToString();
    }


    /// <summary>
    /// Current Ongoing effect (summary of all effects originating from a specific node)
    /// </summary>
    /// <param name="ongoing"></param>
    /// <returns></returns>
    public string GetOngoingEffectDetails(EffectDataOngoing ongoing)
    {
        StringBuilder builder = new StringBuilder();
        if (ongoing.nodeID > -1)
        {
            Node node = GameManager.instance.dataScript.GetNode(ongoing.nodeID);
            if (node != null)
            { builder.AppendFormat("{0}, {1}{2}{3}", node.nodeName, colourAlert, node.Arc.name, colourEnd); }
            else
            {
                Debug.LogWarningFormat("Invalid node (Null) for ongoing.nodeID {0}", ongoing.nodeID);
                builder.Append("Unknown Node");
            }
        }
        else { builder.AppendFormat("{0}, {1}PLAYER{2}, {3}", GameManager.instance.playerScript.PlayerName, colourAlert, colourEnd, "\n"); }
        if (string.IsNullOrEmpty(ongoing.reason) == false)
        { builder.AppendFormat("{0}due to {1}", "\n", ongoing.reason); }
        if (string.IsNullOrEmpty(ongoing.description) == false)
        {
            string colourEffect;
            switch (ongoing.typeLevel)
            {
                case 0: colourEffect = colourBad; break;
                case 1: colourEffect = colourNeutral; break;
                case 2: colourEffect = colourGood; break;
                default: colourEffect = colourNeutral; Debug.LogWarningFormat("Invalid ongoing.type.level \"{0}\" for \"{1}\"", ongoing.typeLevel, ongoing.description); break;
            }
            builder.AppendFormat("{0}{1}{2}<b>{3}</b>{4}", "\n", "\n", colourEffect, ongoing.description, colourEnd);
            if (ongoing.timer > 0)
            { builder.AppendFormat("{0}{1}{2}<b>{3} turn{4}</b>{5} remaining", "\n", "\n", colourNeutral, ongoing.timer, ongoing.timer != 1 ? "s" : "", colourEnd); }
            else { Debug.LogWarningFormat("Invalid ongoing.timer (less than 1) for \"{0}\"", ongoing.description); }
        }
        else { Debug.LogWarningFormat("Invalid ongoing.description (Null or Empty) for \"{0}\"", ongoing.description); }
        return builder.ToString();
    }

    //
    // - - - Teams - - -
    //

    /// <summary>
    /// Authority actor recalled from reserves for active duty and brings a team with him
    /// </summary>
    /// <param name="team"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public string GetAddTeamDetails(Team team, string reason)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}{1}{2} {3} added to Reserves", colourAlert, team.arc.name, colourEnd, team.teamName);
        if (string.IsNullOrEmpty(reason) == false)
        { builder.AppendFormat("{0}{1}{2}", "\n", "\n", reason); }
        return builder.ToString();
    }

    /// <summary>
    /// Team deployed OnMap
    /// </summary>
    /// <param name="team"></param>
    /// <param name="node"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    public string GetTeamDeployDetails(Team team, Node node, Actor actor)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}, {1}{2}{3}{4}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n");
        builder.AppendFormat("{0}{1} {2}{3} Deployed{4}{5}", colourNeutral, team.arc.name, team.teamName, colourEnd, "\n", "\n");
        builder.AppendFormat("by {0} {1}, {2}{3}{4}", GameManager.instance.metaScript.GetAuthorityTitle(), actor.actorName, colourAlert, actor.arc.name, colourEnd);
        return builder.ToString();
    }

    /// <summary>
    /// Team has been autorecalled
    /// </summary>
    /// <param name="node"></param>
    /// <param name="team"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    public string GetTeamAutoRecallDetails(Node node, Team team, Actor actor)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}, {1}{2}{3}{4}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n");
        builder.AppendFormat("{0}{1} {2}{3} AutoRecalled{4}{5}", colourNeutral, team.arc.name, team.teamName, colourEnd, "\n", "\n");
        builder.AppendFormat("{0} {1}, {2}{3}{4} has recalled the team, which has completed their task, back to the Reserves", GameManager.instance.metaScript.GetAuthorityTitle(),
            actor.actorName, colourAlert, actor.arc.name, colourEnd);
        return builder.ToString();
    }

    /// <summary>
    /// Team has been withdrawn early
    /// </summary>
    /// <param name="reason"></param>
    /// <param name="team"></param>
    /// <param name="node"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    public string GetTeamWithdrawDetails(string reason, Team team, Node node, Actor actor)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}, {1}{2}{3}{4}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n");
        builder.AppendFormat("{0}{1} {2}{3} Withdrawn{4}{5}", colourNeutral, team.arc.name, team.teamName, colourEnd, "\n", "\n");
        builder.AppendFormat("{0} {1}, {2}{3}{4} has recalled the team EARLY", GameManager.instance.metaScript.GetAuthorityTitle(), actor.actorName, colourAlert, actor.arc.name, colourEnd);
        if (string.IsNullOrEmpty(reason) == false)
        { builder.AppendFormat("{0}{1}{2}{3}{4}", "\n", "\n", colourNeutral, reason, colourEnd); }
        return builder.ToString();
    }

    /// <summary>
    /// Team carries out effect OnMap
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="node"></param>
    /// <param name="team"></param>
    /// <returns></returns>
    public string GetTeamEffectDetails(string effectText, Node node, Team team)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}, {1}{2}{3}{4}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n");
        builder.AppendFormat("{0}{1} {2}{3}", colourNeutral, team.arc.name, team.teamName, colourEnd);
        if (string.IsNullOrEmpty(effectText) == false)
        { builder.AppendFormat("{0}{1}{2}", "\n", "\n", effectText); }
        return builder.ToString();
    }

    /// <summary>
    /// Team neutralised by Resistance
    /// </summary>
    /// <param name=""></param>
    /// <param name=""></param>
    /// <param name=""></param>
    /// <returns></returns>
    public string GetTeamNeutraliseDetails(Node node, Team team)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}, {1}{2}{3}{4}", node.nodeName, colourAlert, node.Arc.name, colourEnd, "\n");
        builder.AppendFormat("{0}{1} {2}{3}", colourNeutral, team.arc.name, team.teamName, colourEnd);
        builder.AppendFormat("{0}{1}{2}<b>Neutralised by Resistance and withdrawn</b>{3}", "\n", "\n", colourBad, colourEnd);
        return builder.ToString();
    }

    //
    // - - - Objectives
    //

    /// <summary>
    /// Progress made towards an Objective (or Objective completed if progress 100%)
    /// </summary>
    /// <param name="reason"></param>
    /// <param name="adjustment"></param>
    /// <param name="objective"></param>
    /// <returns></returns>
    public string GetObjectiveProgressDetails(string reason, int adjustment, Objective objective)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>Objective {0}\'{1}\'</b>{2}{3}", colourAlert, objective.tag, colourEnd, "\n");
        if (objective.progress < 100)
        {
            //progress made
            builder.AppendFormat("<b>Progress {0}+{1}%{2}</b>{3}", colourNeutral, adjustment, colourEnd, "\n");
            builder.AppendFormat("<b>Now {0}{1}%{2} Complete{3}", colourNeutral, objective.progress, colourEnd, "\n");
        }
        else
        {
            //objective complete
            builder.AppendFormat("{0}<b>COMPLETED</b>{1}{2}", colourNeutral, colourEnd, "\n");
        }
        //reason
        builder.AppendFormat("{0}<b>Due to {1}{2}</b>{3}", "\n", colourAlert, reason, colourEnd);
        return builder.ToString();
    }

    //
    // - - - Organisations
    //

    /// <summary>
    /// Whenever Org secret revealed, shows reason why and specific effects
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="secret"></param>
    /// <param name="secretEffect"></param>
    /// <returns></returns>
    public string GetOrgRevealSecretDetails(string orgName, Secret secret, string reason)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>{0}{1}{2}</b>", colourNormal, orgName, colourEnd);
        if (string.IsNullOrEmpty(reason) == false)
        { builder.AppendFormat("{0}{1}<b>{2}</b>{3}", "\n", colourBad, reason, colourEnd); }
        builder.AppendFormat("{0}{1}<b>Secret Revealed</b>", "\n", "\n");
        GetSecretEffects(builder, secret);
        return builder.ToString();
    }

    /// <summary>
    /// Player reputation with Org at Zero, no more help until it improves
    /// </summary>
    /// <param name="orgName"></param>
    /// <returns></returns>
    public string GetOrgReputationDetails(string orgName)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>{0}{1}{2}</b>", colourNormal, orgName, colourEnd);
        builder.AppendFormat("{0}{1}<b>Reputation at Zero</b>{2}", "\n", colourBad, colourEnd);
        builder.AppendFormat("{0}{1}<b>Organisation will do {2}{3}NO MORE FAVOURS{4}{5}until your reputation with them improves</b>", "\n", "\n", "\n", colourBad, colourEnd, "\n");
        return builder.ToString();
    }

    /// <summary>
    /// InfoOrg tracks Nemesis on behalf of player
    /// </summary>
    /// <param name="node"></param>
    /// <param name="nemesis"></param>
    /// <param name="org"></param>
    /// <param name="moveNumber"></param>
    /// <returns></returns>
    public string GetOrgNemesisDetails(Node node, Nemesis nemesis, Organisation org, int moveNumber)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>{0}{1}{2}</b> info feed{3}has your{4}{5}", colourAlert, org.tag, colourEnd, "\n", "\n", "\n");
        builder.AppendFormat("<b>{0}{1}{2} Nemesis</b>{3}{4}", colourNeutral, nemesis.name, colourEnd, "\n", "\n");
        builder.AppendFormat("At <b>{0}, {1}{2}{3}</b> district", node.nodeName, colourAlert, node.Arc.name, colourEnd);
        //only add a time stamp if nemesis is capable of multiple moves within a single turn
        if (nemesis.movement > 1)
        { builder.AppendFormat(" at <b>{0}</b> hrs", GetContactTime(moveNumber)); }
        return builder.ToString();
    }

    /// <summary>
    /// InfoOrg tracks Erasure Team on behalf of Player
    /// </summary>
    /// <param name="node"></param>
    /// <param name="team"></param>
    /// <param name="org"></param>
    /// <returns></returns>
    public string GetOrgErasureTeamDetails(Node node, Team team, Organisation org)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>{0}{1}{2}</b> info feed{3}has{4}{5}", colourAlert, org.tag, colourEnd, "\n", "\n", "\n");
        builder.AppendFormat("<b>{0}{1} {2}{3}</b>{4}{5}", colourNeutral, team.arc.name, team.teamName, colourEnd, "\n", "\n");
        builder.AppendFormat("At <b>{0}, {1}{2}{3}</b> district", node.nodeName, colourAlert, node.Arc.name, colourEnd);
        return builder.ToString();
    }

    /// <summary>
    /// InfoOrg tracks Npc on behalf of Player
    /// </summary>
    /// <param name="node"></param>
    /// <param name="npc"></param>
    /// <param name="org"></param>
    /// <returns></returns>
    public string GetOrgNpcDetails(Node node, Npc npc, Organisation org)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>{0}{1}{2}</b> info feed{3}has the{4}{5}", colourAlert, org.tag, colourEnd, "\n", "\n", "\n");
        builder.AppendFormat("<b>{0}{1}{2}</b>{3}{4}", colourNeutral, npc.tag, colourEnd, "\n", "\n");
        builder.AppendFormat("At <b>{0}, {1}{2}{3}</b> district", node.nodeName, colourAlert, node.Arc.name, colourEnd);
        return builder.ToString();
    }


    //
    // - - - Npc
    //

    /// <summary>
    /// Npc arrives in city
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public string GetNpcArrivalDetails(Npc npc)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>The {0}{1}{2} will be in {3} for at least {4}{5} days{6}</b>{7}{8}", colourAlert, npc.tag, colourEnd, GameManager.instance.cityScript.GetCity().tag,
            colourNeutral, npc.maxTurns, colourEnd, "\n", "\n");
        if (string.IsNullOrEmpty(npc.nodeStart.arriving) == false)
        { builder.AppendFormat("<b>They will be arriving at {0}{1}{2}</b>{3}", colourNeutral, npc.nodeStart.arriving, colourEnd, "\n"); }
        else { builder.AppendFormat("{0}<b>We don't know where they are arriving</b>{1}{2}", colourBad, colourEnd, "\n"); }
        if (string.IsNullOrEmpty(npc.nodeEnd.visiting) == false)
        { builder.AppendFormat("<b>We expect them to visit {0}{1}{2}</b>{3}{4}", colourNeutral, npc.nodeEnd.visiting, colourEnd, "\n", "\n"); }
        else { builder.AppendFormat("{0}<b>It's unclear where they will visit</b>{1}{2}{3}", colourBad, colourEnd, "\n", "\n"); }
        builder.Append(GameManager.instance.missionScript.GetFormattedNpcEffectsAll(npc));
        return builder.ToString();
    }

    /// <summary>
    /// Npc departs city without the Player interacting with them (timed out)
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public string GetNpcDepartDetails(Npc npc)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>After {0}{1}{2} days, the {3}{4}{5} has caught a shuttle out of the city from {6}{7}{8}</b>{9}{10}", colourNeutral, npc.daysActive, colourEnd, colourAlert, npc.tag, colourEnd,
            colourAlert, npc.currentNode.nodeName, colourEnd, "\n", "\n");
        builder.AppendFormat("<b>You failed to {0}</b>{1}", npc.action.activity, "\n");
        builder.Append(GameManager.instance.missionScript.GetFormattedNpcEffectsBad(npc));
        return builder.ToString();
    }

    /// <summary>
    /// Npc interacts with Player then departs
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public string GetNpcInteractDetails(Npc npc)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>After {0}{1}{2} days, you found the {3}{4}{5} at {6}</b>{7}{8}", colourNeutral, npc.daysActive, colourEnd, colourAlert, npc.tag, colourEnd, npc.currentNode.nodeName, "\n", "\n");
        builder.AppendFormat("<b>You managed to {0}{1}{2}</b>{3}{4}", colourAlert, npc.action.activity, colourEnd, "\n", "\n");
        builder.Append(GameManager.instance.missionScript.GetFormattedNpcEffectsGood(npc));
        return builder.ToString();
    }

    /// <summary>
    /// Npc Ongoing effect status (displayed every turn Npc is onMap)
    /// </summary>
    /// <param name="nemesis"></param>
    /// <returns></returns>
    public string GetNpcOngoingEffectDetails(Npc npc)
    {
        StringBuilder builder = new StringBuilder();
        if (npc.timerTurns == 0)
        { builder.AppendFormat("<b>The {0}{1}{2} is expected to leave the city {3}any day{4} now</b>{5}", colourAlert, npc.tag, colourEnd, colourBad, colourEnd, "\n"); }
        else
        {
            builder.AppendFormat("<b>The {0}{1}{2} should remain in the city for at least another {3}{4} day{5}{6}{7}", colourAlert, npc.tag, colourEnd, colourNeutral, npc.timerTurns,
                npc.timerTurns != 1 ? "s" : "", colourEnd, "\n");
        }
        builder.AppendFormat("{0}<b>Your subordinates Contacts will spot the {1}{2}{3} if their Effectiveness is {4}{5}, or higher{6}</b>{7}{8}", "\n", colourAlert, npc.tag, colourEnd, colourNeutral,
            npc.stealthRating, colourEnd, "\n", "\n");
        builder.AppendFormat("<b>You need to end your turn in the {0}same district{1} as the {2} to {3}{4}{5}</b>", colourNeutral, colourEnd, npc.tag, colourAlert, npc.action.activity, colourEnd);
        return builder.ToString();
    }

    //
    // - - - Investigations
    //

    /// <summary>
    /// new Investigation launched
    /// </summary>
    /// <param name="invest"></param>
    /// <returns></returns>
    public string GetInvestNewDetails(Investigation invest)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("<b>An Investigation has been launched into your{0}{1}{2}{3}</b>{4}{5}{6}", "\n", "\n", colourNeutral, invest.tag, colourEnd, "\n", "\n");
        Actor actor = GameManager.instance.dataScript.GetHQHierarchyActor(invest.lead);
        if (actor != null)
        { builder.AppendFormat("<b>{0}, {1}{2}{3}</b>{4}will be leading the Investigation", actor.actorName, colourAlert, GameManager.instance.dataScript.GetHQActorPosition(actor.statusHQ), colourEnd, "\n"); }
        else
        {
            Debug.LogWarningFormat("Invalid actor (Null) for investigation lead {0}", invest.lead);
            builder.AppendFormat("{0}It is not known who is leading the Investigation{1}", colourAlert, colourEnd);
        }
        return builder.ToString();
    }

    /// <summary>
    /// ongoing investigation, Effects tab
    /// </summary>
    /// <param name="invest"></param>
    /// <returns></returns>
    public string GetInvestOngoingDetails(Investigation invest)
    {
        StringBuilder builder = new StringBuilder();
        string outcome = "Unknown";
        Actor actor = GameManager.instance.dataScript.GetHQHierarchyActor(invest.lead);
        if (actor != null)
        {
            int motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
            switch (motivation)
            {
                case 0: outcome = string.Format("{0}Very Bad{1}", colourBad, colourEnd); break;
                case 1: outcome = string.Format("{0}Bad{1}", colourBad, colourEnd); break;
                case 2: outcome = string.Format("{0}Neutral{1}", colourNeutral, colourEnd); break;
                case 3: outcome = string.Format("{0}Good{1}", colourGood, colourEnd); break;
                default: Debug.LogWarningFormat("Unrecognised motivation \"{0}\"", motivation); outcome = "Unclear"; break;
            }
            builder.AppendFormat("Lead Investigator{0}<b>{1}, {2}{3}{4}</b>{5}{6}", "\n", actor.actorName, colourAlert, GameManager.instance.dataScript.GetHQActorPosition(invest.lead), colourEnd, "\n", "\n");
            builder.AppendFormat("<b>Lead Motivation {0}{1}{2}, {3}</b>{4}{5}", colourNeutral, motivation, colourEnd, outcome, "\n", "\n");
        }
        else
        {
            Debug.LogWarningFormat("Invalid actor (Null) for investigation lead {0}", invest.lead);
            builder.AppendFormat("{0}It is not known who is leading the Investigation{1}{2}{3}", colourAlert, colourEnd, "\n", "\n");
        }
        switch (invest.evidence)
        {
            case 0: outcome = string.Format("{0}Very Bad{1}", colourBad, colourEnd); break;
            case 1: outcome = string.Format("{0}Bad{1}", colourBad, colourEnd); break;
            case 2: outcome = string.Format("{0}Neutral{1}", colourNeutral, colourEnd); break;
            case 3: outcome = string.Format("{0}Good{1}", colourGood, colourEnd); break;
            default: Debug.LogWarningFormat("Unrecognised invest.evidence \"{0}\"", invest.evidence); outcome = "Unclear"; break;
        }
        builder.AppendFormat("<b>Evidence level {0}{1}{2}, {3}</b>{4}{5}", colourNeutral, invest.evidence, colourEnd, outcome, "\n", "\n");
        switch (invest.evidence)
        {
            case 0: outcome = string.Format("{0}Guilty{1}", colourBad, colourEnd); break;
            case 1:
            case 2: outcome = string.Format("{0}Uncertain{1}", colourNeutral, colourEnd); break;
            case 3: outcome = string.Format("{0}Innocent{1}", colourGood, colourEnd); break;
            default: Debug.LogWarningFormat("Unrecognised evidence \"{0}\"", invest.evidence); outcome = "Unclear"; break;
        }
        builder.AppendFormat("<b>Likely Outcome {0}</b>", outcome);
        return builder.ToString();
    }

    /// <summary>
    /// New evidence has come forward
    /// </summary>
    /// <param name="invest"></param>
    /// <returns></returns>
    public string GetInvestEvidenceDetails(Investigation invest, string source)
    {
        if (string.IsNullOrEmpty(source) == true)
        { Debug.LogWarning("Invalid source (Null or Empty)"); source = "Unknown"; }
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<b>{1}{2} Investigation</b>{3}{4}", colourNeutral, invest.tag, colourEnd, "\n", "\n");
        if (invest.evidence != invest.previousEvidence)
        {
            builder.AppendFormat("<b>New Evidence has come to light{0}{1}Evidence level now {2}{3}{4} (was {5}{6}{7})</b>{8}", "\n", "\n",
              colourAlert, invest.evidence, colourEnd, colourAlert, invest.previousEvidence, colourEnd, "\n");
            if (invest.evidence > invest.previousEvidence)
            { builder.AppendFormat("<b>This {0}Helps{1} your case</b>{2}{3}", colourGood, colourEnd, "\n", "\n"); }
            else { builder.AppendFormat("This {0}<b>Harms</b>{1} your case{2}{3}", colourBad, colourEnd, "\n", "\n"); }
        }
        else
        {
            switch (invest.evidence)
            {
                case 3:
                    builder.AppendFormat("<b>Evidence is Conclusive{0}{1}Innocent verdict imminent</b>{2}{3}", "\n", colourGood, colourEnd, "\n");
                    builder.AppendFormat("<b>in {0}{1}{2} more turns</b>{3}{4}", colourNeutral, GameManager.instance.playerScript.timerInvestigationBase, colourEnd, "\n", "\n");
                    break;
                case 0:
                    builder.AppendFormat("<b>Evidence is Conclusive{0}{1}Guilty verdict imminent</b>{2}{3}{4}", "\n", colourBad, colourEnd, "\n", "\n");
                    builder.AppendFormat("<b>in {0}{1}{2} more turns</b>{3}{4}", colourNeutral, GameManager.instance.playerScript.timerInvestigationBase, colourEnd, "\n", "\n");
                    break;
                case 2:
                case 1:
                default: Debug.LogWarningFormat("Invalid invest.evidence \"{0}\"", invest.evidence); break;
            }
        }
        builder.AppendFormat("Evidence Uncovered by{0}{1}<b>{2}{3}</b>", "\n", colourNeutral, source, colourEnd);
        Actor actor = GameManager.instance.dataScript.GetHQHierarchyActor(invest.lead);
        if (actor != null)
        { builder.AppendFormat("<b>{0}{1}, {2}{3}{4}</b>", "\n", actor.actorName, colourAlert, GameManager.instance.dataScript.GetHQActorPosition(actor.statusHQ), colourEnd); }
        else
        { Debug.LogWarningFormat("Invalid actor (Null) for investigation lead {0}", invest.lead); }
        return builder.ToString();
    }

    /// <summary>
    /// Investigation conclusion reached and counting down to a resolution
    /// </summary>
    /// <param name="invest"></param>
    /// <returns></returns>
    public string GetInvestResolutionDetails(Investigation invest)
    {
        StringBuilder builder = new StringBuilder();
        switch (invest.evidence)
        {
            case 0:
                builder.AppendFormat("{0}<b>{1}{2} Investigation is counting down to a{3}{4}{5}GUILTY{6} verdict</b>{7}{8}", colourAlert, invest.tag, colourEnd, "\n", "\n", colourBad, colourEnd, "\n", "\n");
                break;
            case 3:
                builder.AppendFormat("{0}<b>{1}{2} Investigation is counting down to an{3}{4}{5}INNOCENT{6} verdict</b>{7}{8}", colourAlert, invest.tag, colourEnd, "\n", "\n", colourGood, colourEnd, "\n", "\n");
                break;
            default:
                Debug.LogWarningFormat("Unrecognised invest.evidence \"{0}\" (should be 0 or 3)", invest.evidence);
                builder.AppendFormat("{0}<b>{1}{2} Investigation is counting down to an{3}{4}{5}UNCERTAIN{6} verdict</b>{7}{8}", colourAlert, invest.tag, colourEnd, "\n", "\n", colourAlert, colourEnd, "\n", "\n");
                break;
        }
        builder.AppendFormat("<b>A resolution will be reached{0}in {1}{2}{3} turn{4}</b>", "\n", colourNeutral, invest.timer, colourEnd, invest.timer != 1 ? "s" : "");
        return builder.ToString();
    }

    /// <summary>
    /// Investigation completed. Verdict enforced
    /// </summary>
    /// <param name="invest"></param>
    /// <returns></returns>
    public string GetInvestCompletedDetails(Investigation invest)
    {
        StringBuilder builder = new StringBuilder();
        switch (invest.outcome)
        {
            case InvestOutcome.Guilty:
                builder.AppendFormat("{0}<b>{1}{2} Investigation completed{3}{4}{5}GUILTY Verdict{6}{7}{8}", colourAlert, invest.tag, colourEnd, "\n", "\n", colourBad, colourEnd, "\n", "\n");
                builder.AppendFormat("{0}<b>You have been Fired</b>{1}Black Marks +{2}{3}", colourBad, "\n", GameManager.instance.campaignScript.GetInvestigationBlackMarks() - 1, colourEnd);
                break;
            case InvestOutcome.Innocent:
                builder.AppendFormat("{0}<b>{1}{2} Investigation completed{3}{4}{5}INNOCENT{6} verdict</b>{7}{8}", colourAlert, invest.tag, colourEnd, "\n", "\n", colourGood, colourEnd, "\n", "\n");
                builder.AppendFormat("<b>You have been exonerated{0}{1}{2}+{3} HQ Approval</b>{4}", "\n", "\n", colourGood, GameManager.instance.playerScript.investHQApproval, colourEnd);
                break;
            default:
                Debug.LogWarningFormat("Unrecognised invest.outcome \"{0}\" (should be 0 or 3)", invest.outcome);
                builder.AppendFormat("{0}<b>{1}{2} Investigation completed{3}{4}{5}UNCERTAIN{6} verdict</b>{7}{8}", colourAlert, invest.tag, colourEnd, "\n", "\n", colourAlert, colourEnd, "\n", "\n");
                break;
        }
        return builder.ToString();
    }

    /// <summary>
    /// Investigation Dropped
    /// </summary>
    /// <param name="invest"></param>
    /// <returns></returns>
    public string GetInvestDroppedDetails(Investigation invest)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}<b>{1}{2} Investigation dropped{3}{4}For reasons unknown{5}{6}", colourAlert, invest.tag, colourEnd, "\n", "\n", "\n", "\n");
        builder.AppendFormat("{0}No further action will be taken{1}", colourGood, colourEnd);
        return builder.ToString();
    }


    //new methods above here
}