using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using gameAPI;
using packageAPI;

/// <summary>
/// holds all methods required for generating ItemData.bottomText's (called by MessageManager.cs methods)
/// </summary>
public class ItemDataManager : MonoBehaviour
{


    //private string colourRebel;
    //private string colourAuthority;
    private string colourNeutral;
    //private string colourNormal;
    private string colourGood;
    private string colourBad;
    //private string colourGrey;
    private string colourAlert;
    //private string colourSide;
    private string colourEnd;

    public void Initialise()
    {
        //update colours for AI Display tooltip data
        /*SetColours();*/
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
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        //colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        //colourRebel = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        //colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        //colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
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
        builder.AppendFormat("{0}{1}{2} now at{3}{4}", colourNeutral, GameManager.instance.playerScript.PlayerName, colourEnd, "\n", "\n");
        builder.AppendFormat("{0}, a {1}{2}{3} district", node.nodeName, colourAlert, node.Arc.name, colourEnd);
        if (GameManager.instance.optionScript.debugData == true)
        { builder.AppendFormat(", ID {0}", node.nodeID); }
        if (changeInvisibility != 0)
        {
            builder.AppendFormat("{0}{1}{2}Player SPOTTED{3}<b>Invisibility {4}</b>{5}", "\n", "\n", colourBad, "\n", changeInvisibility, colourEnd);
            if (aiDelay > 0)
            { builder.AppendFormat("{0}{1}Authority will know in {2}<b>{3}</b>{4}{5} turn{6}{7}", "\n", colourAlert, colourNeutral, aiDelay, colourEnd, colourAlert, aiDelay != 1 ? "s" : "", colourEnd); }
        }
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
            //Player
            if (string.IsNullOrEmpty(reason) == false)
            { builder.AppendFormat("{0}{1}, PLAYER{2}{3}{4}{5}{6}", GameManager.instance.playerScript.PlayerName, colourAlert, colourEnd, "\n", reason, "\n", "\n"); }
            builder.AppendFormat("{0}PLAYER{1} status now {2}<b>{3}</b>{4}", colourAlert, colourEnd, colourNeutral, GameManager.instance.playerScript.status, colourEnd);
        }
        else
        {
            //Actor
            if (string.IsNullOrEmpty(reason) == false)
            { builder.AppendFormat("{0}, {1}{2}{3}{4}{5}{6}{7}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", reason, "\n", "\n"); }
            builder.AppendFormat("{0}{1}{2} status now {3}<b>{4}</b>{5}", colourAlert, actor.arc.name, colourEnd, colourNeutral, actor.Status, colourEnd);
        }
        if (string.IsNullOrEmpty(details) == false)
        { builder.AppendFormat("{0}{1}{2}", "\n", "\n", details); }
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
    /// <param name="secretID"></param>
    /// <param name="isThreatDropped"></param>
    /// <returns></returns>
    public string GetActorBlackmailDetails(Actor actor, int secretID, bool isThreatDropped, string reason)
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
            if (secretID > -1)
            {
                Secret secret = GameManager.instance.dataScript.GetSecret(secretID);
                if (secret != null)
                {
                    builder.AppendFormat("{0}, {1}{2}{3}{4}{5}has carried out their threat{6}{7}{8}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", colourBad, colourEnd, "\n", "\n");
                    builder.AppendFormat("<b>\"{0}\" is Revealed</b>", secret.tag);
                    /*GetSecretEffects(builder, secret);*/
                }
                else { Debug.LogWarningFormat("Invalid secret (Null) for secretID {0}", secretID); }
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
    public string GetActorConflictDetails(Actor actor, int conflictID, string reasonNoConflict)
    {
        StringBuilder builder = new StringBuilder();
        if (conflictID > -1)
        {
            //CONFLICT
            builder.AppendFormat("{0}, {1}{2}{3} has a{4}Relationship Conflict with you{5}{6}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", "\n", "\n");
            ActorConflict conflict = GameManager.instance.dataScript.GetActorConflict(conflictID);
            if (conflict != null)
            {  builder.AppendFormat("{0} threatens to{1}{2}<b>{3}</b>{4}", actor.actorName, "\n", colourBad, conflict.threatText, colourEnd); }
            else { Debug.LogWarningFormat("Invalid actorConflict (Null) for conflictID {0}", conflictID); }
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
        else { Debug.LogFormat("Invalid condition.type (Null) for condition \"{0}\"", condition.name); }
        if (isGained == true)
        { builder.AppendFormat("{0}, {1}{2}{3}{4}gains condition {5}<b>{6}</b>{7}{8}{9}", genericActorName, colourAlert, genericActorArc, colourEnd, "\n", colourCondition, condition.name, colourEnd, "\n", "\n"); }
        else
        { builder.AppendFormat("{0}, {1}{2}{3}{4}loses condition {5}<b>{6}</b>{7}{8}{9}", genericActorName, colourAlert, genericActorArc, colourEnd, "\n", colourCondition, condition.name, colourEnd, "\n", "\n"); }
        if (string.IsNullOrEmpty(reason) == false)
        { builder.AppendFormat("{0}{1}{2}", colourNeutral, reason, colourEnd); }
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
        builder.AppendFormat("{0}{1}{2} gear{3}", colourNeutral, gear.name, colourEnd, "\n");
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
        builder.AppendFormat("{0}{1}{2} gear{3}", colourNeutral, gear.name, colourEnd, "\n");
        if (renownUsed > 0)
        {
            builder.AppendFormat("has been {0}<b>SAVED</b>{1}{2}{3}", colourGood, colourEnd, "\n", "\n");
            builder.AppendFormat("{0}<b>{1}</b>{2} Renown used", colourNeutral, renownUsed, colourEnd);
        }
        else
        {
            builder.AppendFormat("has been {0}<b>COMPROMISED</b>{1}{2}{3}", colourBad, colourEnd, "\n", "\n");
            builder.AppendFormat("{0}Gear has been lost{1}", colourBad, colourEnd);
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
        builder.AppendFormat("{0}{1}{2} gear{3}", colourNeutral, gear.name, colourEnd, "\n");
        if (isGivenToHQ == true)
        {
            //gear lost 'cause you overloaded the actor with gear
            builder.AppendFormat("{0}Donated to Faction HQ{1}{2}{3}", colourBad, colourEnd, "\n", "\n");
            builder.AppendFormat("by {0}, {1}{2}{3}{4}{5}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", "\n");
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
        builder.AppendFormat("{0}{1}{2} gear{3}", colourNeutral, gear.name, colourEnd, "\n");
        builder.AppendFormat("{0}Can be acquired{1}{2}", colourGood, colourEnd, "\n");
        builder.AppendFormat("from {0}, {1}{2}{3}{4}{5}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", "\n");
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
        builder.AppendFormat("{0}{1}{2} gear{3}", colourNeutral, gear.name, colourEnd, "\n");
        if (isGiven == true)
        {
            //gear given
            builder.AppendFormat("has been given{0}", "\n");
            builder.AppendFormat("to {0}, {1}{2}{3}{4}{5}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", "\n");
            builder.AppendFormat("{0}{1} Motivation +{2}{3}", actor.actorName, colourGood, motivation, colourEnd);
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
        builder.AppendFormat("{0}{1}{2} gear{3}", colourNeutral, gear.name, colourEnd, "\n");
        if (actorID != 999)
        {
            //actor got gear
            Actor actor = GameManager.instance.dataScript.GetActor(actorID);
            if (actor != null)
            {
                builder.AppendFormat("Sourced by {0}, {1}{2}{3}{4}{5}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", "\n");
                builder.AppendFormat("at {0}, a {1}{2}{3} district", node.nodeName, colourAlert, node.Arc.name, colourEnd);
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
            builder.AppendFormat("Sourced by {0}, {1}PLAYER{2}{3}{4}", GameManager.instance.playerScript.PlayerName, colourAlert, colourEnd, "\n", "\n");
            builder.AppendFormat("at {0}, a {1}{2}{3} district", node.nodeName, colourAlert, node.Arc.name, colourEnd);
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
            builder.AppendFormat("{0} HQ have agreed to your request for support{1}{2}", faction.name, "\n", "\n");
            builder.AppendFormat("{0}Renown +{1}{2}{3}{4}", colourGood, supportGiven, colourEnd, "\n", "\n");
            builder.AppendFormat("{0}<b>{1}% chance of Approval</b>{2}{3}Faction Approval {4} out of {5}", colourNeutral, factionApproval * 10, colourEnd, "\n",
                factionApproval, GameManager.instance.factionScript.maxFactionApproval);
        }
        else
        {
            //support declined
            builder.AppendFormat("{0} HQ couldn't agree{1}{2}{3}<b>No Support provided</b>{4}{5}{6}", faction.name, "\n", "\n", colourBad, colourEnd, "\n", "\n");
            builder.AppendFormat("{0}<b>{1}% chance of Approval</b>{2}{3}Faction Approval {4} out of {5}", colourNeutral, factionApproval * 10, colourEnd, "\n",
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
        { builder.AppendFormat("{0} faction{1}Approval {2}<b>+{3}</b>{4}, now {5}<b>{6}</b>{7}{8}{9}", faction.name, "\n", colourGood, change, colourEnd, colourNeutral, newLevel, colourEnd, "\n", "\n"); }
        else { builder.AppendFormat("{0} faction{1}Approval {2}<b>{3}</b>{4}, now {5}<b>{6}</b>{7}{8}{9}", faction.name, "\n", colourBad, change, colourEnd, colourNeutral, newLevel, colourEnd, "\n", "\n"); }
        if (string.IsNullOrEmpty(reason) == false)
        {
            if (change > 0)
            { builder.AppendFormat("{0}{1}{2}", colourGood, reason, colourEnd); }
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
        builder.AppendFormat("{0}{1}{2}", secret.descriptor, "\n", "\n");
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
        builder.AppendFormat("{0}{1}{2}", secret.descriptor, "\n", "\n");
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
        builder.AppendFormat("{0}, {1}{2}{3}", actor.actorName, colourAlert, actor.arc.name, colourEnd);
        if (string.IsNullOrEmpty(reason) == false)
        { builder.AppendFormat("{0}{1}{2}{3}", "\n", colourBad, reason, colourEnd); }
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
        else { Debug.LogWarningFormat("Invalid listOfEffects (Null) for secretID {0}", secret.secretID); }
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
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        if (node != null)
        {
            string playerName = GameManager.instance.playerScript.PlayerName;
            builder.AppendFormat("AI Traceback countermeasures{0}has {1}<b>DETECTED</b>{2} {3}{4}{5}", "\n", colourBad, colourEnd, playerName, "\n", "\n");
            if (delay > 0)
            { builder.AppendFormat("{0}'s location known in{1}{2}<b>{3}</b>{4} turn{5}{6}{7}", playerName, "\n", colourNeutral, delay, colourEnd, delay != 1 ? "s" : "", "\n", "\n"); }
            builder.AppendFormat("{0}{1}'s Invisibility -1{2}", colourBad, playerName, colourEnd);
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
        builder.AppendFormat("{0}, {1}Resistance Leader{2}{3}", GameManager.instance.playerScript.PlayerName, colourAlert, colourEnd, "\n");
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
        builder.AppendFormat("{0}, {1}{2}{3}{4}", actorName, colourAlert, actorArcName, colourEnd, "\n");
        builder.AppendFormat("{0}<b>CURRENT LOCATION KNOWN</b>{1}{2}", colourBad, colourEnd, "\n");
        if (string.IsNullOrEmpty(reason) == false)
        { builder.AppendFormat("due to {0}{1}{2}{3}", colourNeutral, reason, colourEnd, "\n"); }
        if (nodeID > 0)
        {
            Node node = GameManager.instance.dataScript.GetNode(nodeID);
            if (node != null)
            {
                builder.AppendFormat("{0}Spotted at {1}, {2}{3}{4}", "\n", node.nodeName, node.Arc.name, "\n", "\n");
                builder.AppendFormat("{0}{1}'s Invisibility dropped below Zero{2}", colourNeutral, actorName, colourEnd);
            }
            else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", nodeID); }
        }
        else if (connID > 0)
        {
            Connection conn = GameManager.instance.dataScript.GetConnection(connID);
            if (conn != null)
            {
                builder.AppendFormat("{0}Spotted moving between {1}, {2} and {3}, {4}{5}{6}", "\n", conn.node1.nodeName, conn.node1.Arc.name, conn.node2.nodeName, conn.node2.Arc.name, "\n", "\n");
                builder.AppendFormat("{0}{1}'s Invisibility dropped below Zero{2}", colourNeutral, actorName, colourEnd);
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
    public string GetAICaptureDetails(string actorName, string actorArcName, Node node, Team team)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}, {1}{2}{3}{4}", actorName, colourAlert, actorArcName, colourEnd, "\n");
        builder.AppendFormat("{0}<b>has been CAPTURED</b>{1}{2}{3}", colourBad, colourEnd, "\n", "\n");
        builder.AppendFormat("at {0}, {1}{2}{3}by {4} {5}{6}", node.nodeName, node.Arc.name, "\n", colourNeutral, team.arc.name, team.teamName, colourEnd);
        return builder.ToString();
    }



    /// <summary>
    /// actor or player have been released from captivity
    /// </summary>
    /// <param name="actorName"></param>
    /// <param name="actorArcName"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public string GetAIReleaseDetails(string actorName, string actorArcName, Node node)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}, {1}{2}{3}{4}", actorName, colourAlert, actorArcName, colourEnd, "\n");
        builder.AppendFormat("{0}<b>has been RELEASED</b>{1}{2}{3}", colourGood, colourEnd, "\n", "\n");
        builder.AppendFormat("at {0}, {1}{2}{3}", node.nodeName, node.Arc.name, "\n", "\n");
        builder.AppendFormat("{0}{1} is under a cloud of suspicion as a result of their time with the Authority{2}", colourBad, actorName, colourEnd);
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
            if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
            { colourSide = colourGood; }
            builder.AppendFormat("{0}<b>{1}</b>{2}", colourSide, warning, colourEnd);
        }
        if (duration > 0)
        { builder.AppendFormat("{0}{1}{2}<b>Duration {3} turn{4}</b>{5}", "\n", "\n", colourNeutral, duration, duration != 1 ? "s" : "", colourEnd); }
        if (protocolLevelNew > 0)
        { builder.AppendFormat("{0}{1}{2} AI Security Protocol now <b>Level {3}</b>{4}", "\n", "\n", colourNeutral, protocolLevelNew, colourEnd);  }
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
        if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourStart = colourBad; colourFinish = colourGood; colourCrisis = colourGood; }
        //text
        if (string.IsNullOrEmpty(description) == false)
        { builder.AppendFormat("{0}", description); }
        if (duration > 0)
        { builder.AppendFormat("{0}{1}{2}<b>Duration {3} turn{4}</b>{5}", "\n", "\n", colourNeutral, duration, duration != 1 ? "s" : "", colourEnd); }
        if (loyaltyAdjust != 0)
        { builder.AppendFormat("{0}{1}{2}<b>City Loyalty {3}{4}</b>{5}", "\n", "\n", duration > 0 ? colourStart : colourFinish, loyaltyAdjust > 0 ? "+" : "", loyaltyAdjust, colourEnd); }
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
        builder.AppendFormat("Connection between {0}{1}{2} and {3}{4}{5} districts", colourNeutral, connection.node1.nodeName, colourEnd, colourNeutral, connection.node2.nodeName, colourEnd);
        builder.AppendLine(); builder.AppendLine();
        string colourSecurity = colourBad;
        if (secLevel == ConnectionType.MEDIUM) { colourSecurity = colourNeutral; }
        else if (secLevel == ConnectionType.LOW) { colourSecurity = colourGood; }
        builder.AppendFormat("Security level now {0}<b>{1}</b>{2}", colourSecurity, secLevel, colourEnd);
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
        if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
        { colourSideGood = colourBad; colourSideBad = colourGood; }
        if (changeInLoyalty > 0)
        {
            //loyalty increases
            builder.AppendFormat("{0}{1}{2}<b>Loyalty now {3}</b>{4}{5}{6}", GameManager.instance.cityScript.GetCityName(), "\n", colourSideGood, newCityLoyalty, colourEnd, "\n", "\n");
            //change
            builder.AppendFormat("Loyalty increased by {0}<b>+{1}</b>{2}", colourSideGood, changeInLoyalty, colourEnd);
        }
        else
        {
            //loyalty decresed
            builder.AppendFormat("{0}{1}{2}<b>Loyalty now {3}</b>{4}{5}{6}", GameManager.instance.cityScript.GetCityName(), "\n", colourSideBad, newCityLoyalty, colourEnd, "\n", "\n");
            //change
            builder.AppendFormat("Loyalty decreased by {0}<b>{1}</b>{2}", colourSideBad, changeInLoyalty, colourEnd);
        }
        //reason
        if (string.IsNullOrEmpty(reason) == false)
        { builder.AppendFormat("{0}{1}due to {2}{3}{4}", "\n", "\n", colourNeutral, reason, colourEnd); }
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
        if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSideGood = colourBad; colourSideBad = colourGood; }
        builder.AppendFormat("{0}, {1}{2}", node.nodeName, node.Arc.name, "\n");
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
                builder.AppendFormat("{0}{1} crisis has spun out of control{2}{3}{4}", colourAlert, node.crisis.tag, colourEnd, "\n", "\n");
                builder.AppendFormat("{0}<b>City Loyalty -{1}</b>{2}", colourSideGood, reductionInCityLoyalty, colourEnd);
            }
        }
        return builder.ToString();
    }

    //
    // - - - Targets - - -
    //

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
        builder.AppendFormat("{0}{1}{2}{3}", colourNeutral, target.name, colourEnd, "\n");
        builder.AppendFormat("{0}, {1}{2}", node.nodeName, node.Arc.name, "\n");
        //who did it?
        if (actorID == 999)
        { actorName = "Player"; }
        else
        {
            Actor actor = GameManager.instance.dataScript.GetActor(actorID);
            if (actor != null)
            { actorName = actor.actorName; }
            else { Debug.LogWarningFormat("Invalid Actor (Null) for actorID {0}", actorID); }
        }
        if (target.targetStatus == Status.Live)
        {
            //failed
            builder.AppendFormat("{0}{1}{2}<b>{3} attempt FAILED</b>{4}", "\n", "\n", colourBad, actorName, colourEnd);
        }
        else
        {
            //success
            builder.AppendFormat("<b>{0} attempt SUCCEEDED</b>{1}{2}", actorName, "\n", "\n");
            //effects
            builder.AppendFormat(GameManager.instance.targetScript.GetTargetEffects(target.targetID));
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
        builder.AppendFormat("{0}<b>{1}</b>{2}{3}", colourNeutral, target.name, colourEnd, "\n");
        builder.AppendFormat("{0}, {1}{2}{3}", node.nodeName, node.Arc.name, "\n", "\n");
        if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
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
                builder.AppendFormat("{0}, PLAYER{1}{2}", GameManager.instance.playerScript.PlayerName, "\n", "\n");
            }
            else
            {
                Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    //actor
                    builder.AppendFormat("{0}, {1}{2}{3}", actor.actorName, actor.arc.name, "\n", "\n");
                }
                else { Debug.LogWarningFormat("Invalid Actor (Null) for actorID {0}", actorID); }
            }
        }
        else if (node != null)
        {
            //node
            builder.AppendFormat("{0}, {1}, district{2}{3}", node.nodeName, node.Arc.name, "\n", "\n");
        }
        //details
        if (string.IsNullOrEmpty(detailsTop) == false)
        { builder.Append(detailsTop); }
        if (string.IsNullOrEmpty(detailsBottom) == false)
        { builder.AppendFormat("{0}{1}{2}", "\n", "\n", detailsBottom); }
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
        if (ongoing.node != null)
        { builder.AppendFormat("{0}, {1}{2}{3}", ongoing.node.nodeName, colourAlert, ongoing.node.Arc.name, colourEnd); }
        else { builder.AppendFormat("{0}, {1}PLAYER{2}, {3}", GameManager.instance.playerScript.PlayerName, colourAlert, colourEnd, "\n"); }
        if (string.IsNullOrEmpty(ongoing.reason) == false)
        { builder.AppendFormat("{0}due to {1}", "\n", ongoing.reason); }
        if (string.IsNullOrEmpty(ongoing.description) == false)
        {
            string colourEffect;
            switch(ongoing.type.level)
            {
                case 0: colourEffect = colourBad; break;
                case 1: colourEffect = colourNeutral; break;
                case 2: colourEffect = colourGood; break;
                default: colourEffect = colourNeutral; Debug.LogWarningFormat("Invalid ongoing.type.level \"{0}\" for \"{1}\"", ongoing.type.level, ongoing.text); break;
            }
            builder.AppendFormat("{0}{1}{2}<b>{3}</b>{4}", "\n", "\n", colourEffect, ongoing.description, colourEnd);
            if (ongoing.timer > 0)
            { builder.AppendFormat("{0}{1}{2}<b>{3} turn{4}</b>{5} remaining", "\n", "\n", colourNeutral, ongoing.timer, ongoing.timer != 1 ? "s" : "", colourEnd); }
            else { Debug.LogWarningFormat("Invalid ongoing.timer (less than 1) for \"{0}\"", ongoing.text); }
        }
        else { Debug.LogWarningFormat("Invalid ongoing.description (Null or Empty) for \"{0}\"", ongoing.text); }
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

}