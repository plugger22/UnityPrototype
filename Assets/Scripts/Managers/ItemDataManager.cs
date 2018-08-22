using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using gameAPI;

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
    public string GetPlayerMoveDetails(Node node)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}{1}{2} now at{3}{4}", colourNeutral, GameManager.instance.playerScript.PlayerName, colourEnd, "\n", "\n");
        builder.AppendFormat("{0}, a {1}{2}{3} district", node.nodeName, colourAlert, node.Arc.name, colourEnd);
        if (GameManager.instance.optionScript.debugData == true)
        { builder.AppendFormat(", ID {0}", node.nodeID); }
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
    public string GetActorStatusDetails(string reason, Actor actor)
    {
        StringBuilder builder = new StringBuilder();
        if (actor == null)
        {
            //Player
            builder.AppendFormat("{0}Player{1}, {2}, status now {3}<b>{4}</b>{5}{6}{7}", colourAlert, colourEnd, GameManager.instance.playerScript.PlayerName,
                colourNeutral, GameManager.instance.playerScript.status, colourEnd, "\n", "\n");
            if (string.IsNullOrEmpty(reason) == false)
            { builder.AppendFormat("{0}Player{1}{2}{3}", colourAlert, colourEnd, "\n", reason); }
        }
        else
        {
            //Actor
            builder.AppendFormat("{0}, {1}{2}{3}, status now {4}<b>{5}</b>{6}{7}{8}", actor.actorName, colourAlert, actor.arc.name, colourEnd, colourNeutral, actor.Status, colourEnd, "\n", "\n");
            if (string.IsNullOrEmpty(reason) == false)
            { builder.AppendFormat("{0}, {1}{2}{3}{4}{5}", actor.actorName, colourAlert, actor.arc.name, colourEnd, "\n", reason); }
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
            builder.AppendFormat("Sourced by {0}, {1}Player{2}{3}{4}", GameManager.instance.playerScript.PlayerName, colourAlert, colourEnd, "\n", "\n");
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
            else { builder.AppendFormat("{0}{1}{2}", colourBad, reason, colourEnd); }
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
                { builder.AppendFormat("{0}{1}<b>{2}</b>{3}", "\n", colourBad, effect.textTag, colourEnd); }
            }
            else { builder.AppendFormat("{0}No effect", "\n"); }
        }
        else { Debug.LogWarningFormat("Invalid listOfEffects (Null) for secretID {0}", secret.secretID); }
        return builder;
    }

    //
    // - - - AI - - -
    //

    public string GetAIDetectedDetails(int nodeID, int delay)
    {
        StringBuilder builder = new StringBuilder();
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        if (node != null)
        {
            string playerName = GameManager.instance.playerScript.PlayerName;
            builder.AppendFormat("AI Traceback countermeasures{0}has {1}<b>DETECTED</b>{2} {3}{4}{5}", "\n", colourBad, colourEnd, playerName, "\n", "\n");
            builder.AppendFormat("{0}'s location known in{1}{2}<b>{3}</b>{4} turn{5}", playerName, "\n", colourNeutral, delay, colourEnd, delay != 1 ? "s" : "");
            builder.AppendFormat("{0}{1}{2}{3}'s Invisibility -1{4}", "\n", "\n", colourBad, playerName, colourEnd);
        }
        else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", nodeID); builder.Append("Unknown"); }
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

    //
    // - - - Decisions - - -
    //

    /// <summary>
    /// city wide decision, eg. Surveillane crackdown
    /// </summary>
    /// <param name="descriptor"></param>
    /// <param name="warning"></param>
    /// <returns></returns>
    public string GetDecisionGlobalDetails(string descriptor, string warning)
    {
        StringBuilder builder = new StringBuilder();
        if (string.IsNullOrEmpty(descriptor) == false)
        { builder.AppendFormat("<b>{0}</b>{1}{2}{3}{4}", GameManager.instance.cityScript.GetCityName(), "\n", descriptor, "\n", "\n"); }
        if (string.IsNullOrEmpty(warning) == false)
        { builder.AppendFormat("{0}<b>{1}</b>{2}", colourBad, warning, colourEnd); }
        return builder.ToString();
    }

}