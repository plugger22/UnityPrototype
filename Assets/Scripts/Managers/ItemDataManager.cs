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


    private string colourRebel;
    private string colourAuthority;
    private string colourNeutral;
    private string colourNormal;
    private string colourGood;
    private string colourBad;
    private string colourGrey;
    private string colourAlert;
    private string colourSide;
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
        colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        colourRebel = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
        if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; }
        else { colourSide = colourRebel; }
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
        builder.AppendFormat("{0}{1}{2} now at{3}", colourAlert, GameManager.instance.playerScript.PlayerName, colourEnd, "\n");
        builder.AppendFormat("{0},{1}", node.nodeName, "\n");
        builder.AppendFormat("{0}{1}{2} district", colourNeutral, node.Arc.name, colourEnd);
        if (GameManager.instance.optionScript.debugData == true)
        { builder.AppendFormat(", ID {0}", node.nodeID); }
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
            builder.AppendFormat("{0}Donated to Faction HQ{1}{2}{3}", colourBad, colourEnd, "\n");
            builder.AppendFormat("by {0}, {1}{2}{3}{4}{5}", actor.actorName, colourNeutral, actor.arc.name, colourEnd, "\n", "\n");
            builder.AppendFormat("{0}Minions can only have one item of Gear{1}", colourAlert, colourEnd);
        }
        else
        {
            //gear lost through actor doing so
            builder.AppendFormat("{0}Lost, sold or Used{1}{2}{3}", colourBad, colourEnd, "\n");
            builder.AppendFormat("by {0}, {1}{2}{3}{4}{5}", actor.actorName, colourNeutral, actor.arc.name, colourEnd, "\n", "\n");
            builder.AppendFormat("{0}Minions won't hold gear forever{1}", colourAlert, colourEnd);
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
        builder.AppendFormat("{0}Can be acquired{1}{2}{3}", colourGood, colourEnd, "\n");
        builder.AppendFormat("from {0}, {1}{2}{3}{4}{5}", actor.actorName, colourNeutral, actor.arc.name, colourEnd, "\n", "\n");
        builder.AppendFormat("{0}Asking for your gear back <i>may</i> upset your Minion{1}", colourAlert, colourEnd);
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
            builder.AppendFormat("to {0}, {1}{2}{3}{4}{5}", actor.actorName, colourNeutral, actor.arc.name, colourEnd, "\n", "\n");
            builder.AppendFormat("{0} {1}Motivation +{2}{3}", actor.arc.name, colourGood, colourEnd);
        }
        else
        {
            //gear Taken back from actor
            builder.AppendFormat("has been taken back{0}", "\n");
            builder.AppendFormat("from {0}, {1}{2}{3}{4}{5}", actor.actorName, colourNeutral, actor.arc.name, colourEnd, "\n", "\n");
            builder.AppendFormat("{0} {1}Motivation {2}{3}", actor.arc.name, colourBad, colourEnd);
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
    /// subMethod to add Secret effects to builder string for GetPlayerSecretDetails and GetActorSecretDetails
    /// NOTE: secret checked for null by calling method
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="secret"></param>
    /// <returns></returns>
    private StringBuilder GetSecretEffects(StringBuilder builder, Secret secret)
    {
        builder.AppendFormat("<b>Effects if Secret revealed</b>");
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
}
