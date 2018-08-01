using gameAPI;
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
    [Tooltip("Support for both sides factions range from 0 to this amount")]
    [Range(0, 10)] public int maxFactionSupport = 10;

    [Header("Actor Influence")]
    [Tooltip("Amount Faction Support drops by whenever an Actor resigns for whatever reason")]
    [Range(0, 3)] public int factionSupportActorResigns = 1;

    [Header("Faction Matters")]
    [Tooltip("Timer set when faction support is first zero. Decrements each turn and when zero the Player is fired. Reset if support rises above zero")]
    [Range(1, 10)] public int factionFirePlayerTimer = 3;

    [HideInInspector] public Faction factionAuthority;
    [HideInInspector] public Faction factionResistance;

    private int supportZeroTimer;                           //countdown timer once support at zero. Player fired when timer reaches zero.
    private bool isZeroTimerThisTurn;                       //only the first zero timer event per turn is processed
    private int _supportAuthority;                          //level of faction support (out of 10) enjoyed by authority side (Player/AI)
    private int _supportResistance;                         //level of faction support (out of 10) enjoyed by resistance side (Player/AI)


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


    public int SupportAuthority
    {
        get { return _supportAuthority; }
        private set
        {
            _supportAuthority = value;
            _supportAuthority = Mathf.Clamp(_supportAuthority, 0, maxFactionSupport);
            //update top widget bar if current side is authority
            if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
            { EventManager.instance.PostNotification(EventType.ChangeFactionBar, this, _supportAuthority, "FactionManager.cs -> SupportAuthority"); }
        }
    }

    public int SupportResistance
    {
        get { return _supportResistance; }
        private set
        {
            _supportResistance = value;
            _supportResistance = Mathf.Clamp(_supportResistance, 0, maxFactionSupport);
            //update top widget bar if current side is resistance
            if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
            { EventManager.instance.PostNotification(EventType.ChangeFactionBar, this, _supportResistance, "FactionManager.cs -> SupportResistance"); }
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
        //support levels
        SupportAuthority = Random.Range(1, 10);
        SupportResistance = Random.Range(1, 10);
        Debug.Log(string.Format("FactionManager: currentResistanceFaction \"{0}\", currentAuthorityFaction \"{1}\"{2}",
            factionResistance, factionAuthority, "\n"));
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
    /// checks if player given support (+1 renown) from faction based on a random roll vs. level of faction support
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
                    threshold = _supportAuthority * 10;
                    if (rnd < threshold)
                    {
                        //Support Provided
                        Debug.LogFormat("[Rnd] FactionManager.cs -> CheckFactionSupport: GIVEN need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                        string msgText = string.Format("{0} faction provides SUPPORT (+1 Renown)", factionAuthority.name);
                        GameManager.instance.messageScript.FactionSupport(msgText, factionAuthority, _supportAuthority, GameManager.instance.playerScript.Renown, 1);
                        //random
                        string text = string.Format("Faction support GIVEN, need < {0}, rolled {1}",  threshold, rnd);
                        GameManager.instance.messageScript.GeneralRandom(text, threshold, rnd);
                        //Support given
                        GameManager.instance.playerScript.Renown++;
                    }
                    else
                    {
                        //Support declined
                        Debug.LogFormat("[Rnd] FactionManager.cs -> CheckFactionSupport: DECLINED need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                        string msgText = string.Format("{0} faction declines support ({1} % chance of support)", factionAuthority.name, threshold);
                        GameManager.instance.messageScript.FactionSupport(msgText, factionAuthority, _supportAuthority, GameManager.instance.playerScript.Renown);
                        //random
                        string text = string.Format("Faction support DECLINED, need < {0}, rolled {1}", threshold, rnd);
                        GameManager.instance.messageScript.GeneralRandom(text, threshold, rnd);
                    }
                    break;
                case 2:
                    //Resistance
                    threshold = _supportResistance * 10;
                    if (rnd < threshold)
                    {
                        //Support Provided
                        Debug.LogFormat("[Rnd] FactionManager.cs -> CheckFactionSupport: GIVEN need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                        string msgText = string.Format("{0} faction provides SUPPORT (+1 Renown)", factionResistance.name);
                        GameManager.instance.messageScript.FactionSupport(msgText, factionResistance, _supportResistance, GameManager.instance.playerScript.Renown, 1);
                        //random
                        string text = string.Format("Faction support GIVEN, need < {0}, rolled {1}", threshold, rnd);
                        GameManager.instance.messageScript.GeneralRandom(text, threshold, rnd);
                        //Support given
                        GameManager.instance.playerScript.Renown++;
                    }
                    else
                    {
                        //Support declined
                        Debug.LogFormat("[Rnd] FactionManager.cs -> CheckFactionSupport: DECLINED need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                        string msgText = string.Format("{0} faction declines support ({1} % chance of support)", factionResistance.name, threshold);
                        GameManager.instance.messageScript.FactionSupport(msgText,factionResistance, _supportResistance, GameManager.instance.playerScript.Renown);
                        //random
                        string text = string.Format("Faction support DECLINED, need < {0}, rolled {1}", threshold, rnd);
                        GameManager.instance.messageScript.GeneralRandom(text, threshold, rnd);
                    }
                    break;
                default:
                    Debug.LogWarningFormat("Invalid side \"{0}\"", side);
                    break;
            }
        }
    }

    /// <summary>
    /// Checks if support zero, sets timer, counts down each turn and fires player when timer expired. Timer cancelled if support rises
    /// </summary>
    public void CheckFactionFirePlayer()
    {
        //get support
        int support = -1;
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        switch(side.level)
        {
            case 1:
                //Authority
                support = _supportAuthority;
                break;
            case 2:
                //Resistance
                support = _supportResistance;
                break;
            default:
                Debug.LogWarningFormat("Invalid side \"{0}\"", side);
                break;
        }
        //only check once per turn
        if (support == 0 && isZeroTimerThisTurn == false)
        {
            isZeroTimerThisTurn = true;
            if (supportZeroTimer == 0)
            {
                //set timer
                supportZeroTimer = factionFirePlayerTimer;
                //message
                string msgText = string.Format("Faction support Zero. Faction will FIRE you in {0} turn{1}", supportZeroTimer, supportZeroTimer != 1 ? "s" : "");
                GameManager.instance.messageScript.GeneralWarning(msgText);
            }
            else
            {
                //decrement timer
                supportZeroTimer--;
                //fire player at zero
                if (supportZeroTimer == 0)
                {
                    GameManager.instance.win = WinState.Authority;
                    GameManager.instance.messageScript.GeneralWarning("Faction support Zero. Player Fired. Authority wins");
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
                    string msgText = string.Format("Faction support Zero. Faction will FIRE you in {0} turn{1}", supportZeroTimer, supportZeroTimer != 1 ? "s" : "");
                    GameManager.instance.messageScript.GeneralWarning(msgText);
                }
            }
        }
        else
        {
            //timer set to default (support > 0)
            supportZeroTimer = 0;
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
    /// returns current faction support level for specified side in colour formatted string
    /// </summary>
    /// <returns></returns>
    public string GetFactionSupportLevel(GlobalSide side)
    {
        string description = "Unknown";
        if (side != null)
        {
            switch (side.level)
            {
                case 1:
                    description = string.Format("{0}{1}{2} out of {3}", colourNeutral, _supportAuthority, colourEnd, maxFactionSupport);
                    break;
                case 2:
                    description = string.Format("{0}{1}{2} out of {3}", colourNeutral, _supportResistance, colourEnd, maxFactionSupport);
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
    /// use this to adjust faction support level (auto checks for various faction mechanics & generates a message)
    /// </summary>
    /// <param name="amount"></param>
    public void ChangeFactionSupport(int amountToChange, string reason)
    {
        if (string.IsNullOrEmpty(reason) == true) { reason = "Unknown"; }
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        switch(side.level)
        {
            case 1:
                //Authority
                SupportAuthority += amountToChange;
                Debug.LogFormat("[Fac] Authority Faction Support: change {0}{1} now {2} ({3}){4}", amountToChange > 0 ? "+" : "", amountToChange, SupportAuthority, reason, "\n");
                break;
            case 2:
                //Resistance
                SupportResistance += amountToChange;
                Debug.LogFormat("[Fac] Resistance Faction Support: change {0}{1} now {2} ({3}){4}", amountToChange > 0 ? "+" : "", amountToChange, SupportResistance, reason, "\n");
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
