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

    [HideInInspector] public Faction factionAuthority;
    [HideInInspector] public Faction factionResistance;

    private int _supportAuthority;                          //level of faction support (out of 10) enjoyed by authority side (Player/AI)
    private int _supportResistance;                         //level of faction support (out of 10) enjoyed by resistance side (Player/AI)

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


    public int SupportAuthority
    {
        get { return _supportAuthority; }
        set
        {
            _supportAuthority = value;
            //update top widget bar if current side is authority
            if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
            { EventManager.instance.PostNotification(EventType.ChangeFactionBar, this, _supportAuthority); }
        }
    }

    public int SupportResistance
    {
        get { return _supportResistance; }
        set
        {
            _supportResistance = value;
            //update top widget bar if current side is resistance
            if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
            { EventManager.instance.PostNotification(EventType.ChangeFactionBar, this, _supportResistance); }
        }
    }

    public void Initialise()
    {
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
        SupportAuthority = 10;
        SupportResistance = 10;
        Debug.Log(string.Format("FactionManager: currentResistanceFaction \"{0}\", currentAuthorityFaction \"{1}\"{2}", 
            factionResistance, factionAuthority, "\n"));
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "FactionManager");
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
    /// returns current faction for player side in a colour formatted string
    /// </summary>
    /// <returns></returns>
    public string GetFactionName()
    {
        string description = "Unknown";
        switch (GameManager.instance.sideScript.PlayerSide.level)
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
        return description;
    }

    /// <summary>
    /// returns current faction description for player side in colour formatted string
    /// </summary>
    /// <returns></returns>
    public string GetFactionDescription()
    {
        string description = "Unknown";
        switch (GameManager.instance.sideScript.PlayerSide.level)
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
        return description;
    }

    /// <summary>
    /// returns current faction support level for player side in colour formatted string
    /// </summary>
    /// <returns></returns>
    public string GetFactionSupportLevel()
    {
        string description = "Unknown";
        switch (GameManager.instance.sideScript.PlayerSide.level)
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
        return description;
    }

    /// <summary>
    /// returns current faction details for player side in colour formatted string. 'Unknown' if an problem
    /// </summary>
    /// <returns></returns>
    public string GetFactionDetails()
    {
        string colourNode = colourGrey;
        StringBuilder builder = new StringBuilder();
        NodeArc arc;
        switch (GameManager.instance.sideScript.PlayerSide.level)
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
        if (builder.Length == 0)
        { builder.Append("Unknown"); }
        return builder.ToString();
    }




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
