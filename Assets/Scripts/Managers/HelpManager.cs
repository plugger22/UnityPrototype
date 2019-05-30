using gameAPI;
using packageAPI;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// handles all Help related matters
/// </summary>
public class HelpManager : MonoBehaviour
{

    //
    // - - - Colour Scheme - - -
    //
    // Headers are default colourNeutral (automatic)
    // Tips are colourTip (do yourself)
    // Text is default slightly off-yellow (automatic). Add colourAlert highlights yourself
    // No other colours are used

    //colours
    private string colourTip;
    private string colourAlert;
    private string colourEnd;

    public void Initialise(GameState state)
    {
        SetColours();
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
        colourTip = GameManager.instance.colourScript.GetColour(ColourType.goodText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }



    /// <summary>
    /// creates item Help data. Returns an empty list if none
    /// </summary>
    /// <returns></returns>
    public List<HelpData> CreateItemDataHelp()
    {
        List<HelpData> listOfHelp = new List<HelpData>();
        HelpData data;
        StringBuilder builder;
        //
        // - - - Debug - - -
        //
        //test0
        data = new HelpData();
        data.tag = "test0";
        data.header = "Missing help";
        data.text = string.Format("No help has been provided here. You need to go into {0}HelpManager.cs{1} and create a series of topics for the this item", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //
        // - - - Faction Support
        //
        //overview
        data = new HelpData();
        data.tag = "fact_supp_0";
        data.header = "Faction Support";
        builder = new StringBuilder();
        builder.AppendFormat("At the start of each turn your Faction decides whether to provide you with assistance ({0}+1 Renown{1}) or not. ", colourAlert, colourEnd);
        builder.AppendFormat("The chance of it doing so depends on your level of {0}Faction Support{1}. ", colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //chance
        data = new HelpData();
        data.tag = "fact_supp_1";
        data.header = "Chance of Support";
        builder = new StringBuilder();
        builder.AppendFormat("The better your {0}relationship{1} with your faction, the greater the chance of support. ", colourAlert, colourEnd);
        builder.AppendFormat("Support is given if a ten sided die (1d10) is {0}less than{1} your level of Faction Approval (top centre)", colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //tip
        data = new HelpData();
        data.tag = "fact_supp_2";
        data.header = string.Format("{0}Game Tip{1}", colourTip, colourEnd);
        builder = new StringBuilder();
        builder.AppendFormat("Renown is the {0}currency{1} of the game. The more you have the more you can do. ", colourAlert, colourEnd);
        builder.AppendFormat("Your main source of Renown is from the support of your Faction. Aim to keep a {0}positive relationship{1} with them where ever possible.", colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //
        // - - - Main Info App 
        //
        //overview
        data = new HelpData();
        data.tag = "info_app_0";
        data.header = "Info App";
        builder = new StringBuilder();
        builder.AppendFormat("The Info App is your main source of information. <sprite index=0> It refreshes at the {0}beginning{1} of each day. ", colourAlert, colourEnd);
        builder.AppendFormat("It is available while ever you aren't {0}indisposed{1} (Captured, Stressed, etc.)", colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Flashing Tab
        data = new HelpData();
        data.tag = "info_app_1";
        data.header = "Flashing Tabs";
        data.text = string.Format("A flashing white circle over a tab indicates that there is something within the tab that {0}requires your attention{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //ticker tap
        data = new HelpData();
        data.tag = "info_app_2";
        data.header = "News Ticker";
        builder = new StringBuilder(); 
        builder.AppendFormat("The news ticker at the bottom of the App gives daily highlights of events in the city. ", colourAlert, colourEnd);
        builder.AppendFormat("You can adjust the {0}text scroll speed{1} using the '+' and '-' keys", colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Keys
        data = new HelpData();
        data.tag = "info_app_3";
        data.header = "Key Shortcuts";
        builder = new StringBuilder();
        builder.AppendFormat("Use the arrow keys to navigate {0}within{1} the App{2}", colourAlert, colourEnd, "\n");
        builder.AppendFormat("To {0}move between days{1} use 'PGUP' and 'PGDWN'{2}", colourAlert, colourEnd, "\n");
        builder.AppendFormat("To {0}open{1} the App at any time press 'I'", colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //
        // - - - Secrets
        //
        //Overview
        data = new HelpData();
        data.tag = "secret_0";
        data.header = "Secrets";
        builder = new StringBuilder();
        builder.AppendFormat("You start the game with a secret and can gain more due to your {0}actions{1}. ", colourAlert, colourEnd);
        builder.AppendFormat("Only {0}you{1} have secrets but your subordinates can learn them", colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Learning
        data = new HelpData();
        data.tag = "secret_1";
        data.header = "Learning Secrets";
        data.text = string.Format("Each turn there is a {0}small chance{1} that your subordinates may learn one of your secrets. The walls have ears", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //They Know Your Secrets
        data = new HelpData();
        data.tag = "secret_2";
        data.header = "Revealing Secrets";
        builder = new StringBuilder();
        builder.AppendFormat("Secrets can have {0}serious consequences{1} if revealed. ", colourAlert, colourEnd);
        builder.AppendFormat("Any time you have a {0}Conflict{1} with one of your Subordinates there is a chance they may blackmail you and threaten to reveal your secret", colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //tip
        data = new HelpData();
        data.tag = "secret_3";
        data.header = string.Format("{0}Game Tip{1}", colourTip, colourEnd);
        builder = new StringBuilder();
        builder.AppendFormat("Avoid {0}conflicts{1} with any of your subordinates that know of your secrets. It's too risky. ", colourAlert, colourEnd);
        builder.AppendFormat("If you are thinking of removing them, try and do so {0}before{1} they know your secrets as it costs less Renown", colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //
        // - - - Random Rolls - - -
        //
        //overview
        data = new HelpData();
        data.tag = "roll_0";
        data.header = "Random Rolls";
        data.text = string.Format("Various events within the game require random rolls. Important ones are shown here in the {0}Random{1} tab. ", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //good rolls
        data = new HelpData();
        data.tag = "roll_1";
        data.header = "Good Rolls";
        data.text = string.Format("If a successful roll indicates a {0}GOOD{1} outcome from the Player's point of view, the header is shown in {2}GREEN{3}.", colourAlert, colourEnd, colourAlert, colourEnd);;
        listOfHelp.Add(data);
        //bad rolls
        data = new HelpData();
        data.tag = "roll_2";
        data.header = "Bad Rolls";
        data.text = string.Format("If a successful roll indicates a {0}BAD{1} outcome from the Player's point of view, the header is shown in {2}RED{3}.", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //
        // - - - Actor Conflict
        //
        //Overview
        data = new HelpData();
        data.tag = "conflict_0";
        data.header = "Conflict";
        builder = new StringBuilder();
        builder.AppendFormat("Anytime one of your subordinates Motivation falls below Zero, for any reason, they instigate a relationship conflict with you. ", colourAlert, colourEnd);
        builder.AppendFormat("Conflicts can have a {0}range of outcomes{1} such as your subordinate resigning, blackmailing you, becoming stressed, leaking against you or simply simmering away and doing nothing",
            colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //
        // - - - Player Betrayed
        //
        //overview
        data = new HelpData();
        data.tag = "betrayal_0";
        data.header = "Betrayed";
        data.text = string.Format("At any point there is a chance of you being Betrayed, {0}losing Invisibility{1} and possibly having the Authority immediately know your position (Invisibility {2}Zero{3})", 
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Who
        data = new HelpData();
        data.tag = "betrayal_1";
        data.header = "Traitors";
        data.text = string.Format("Traitors within Rebel HQ are outside of your control but any subordinate with the {0}QUESTIONABLE{1} condition has a chance of being a traitor (increases everytime they are {2}Captured{3})",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Odds of Betrayal
        data = new HelpData();
        data.tag = "betrayal_2";
        data.header = "Chance of Betrayal";
        data.text = string.Format("A check is made every turn. There is a base chance representing Rebel HQ and this {0}increases with traitorous subordinates{1} (doubles if one, triples if two, etc.)", 
            colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Tip
        data = new HelpData();
        data.tag = "betrayal_3";
        data.header = string.Format("{0}Game Tip{1}", colourTip, colourEnd);
        data.text = string.Format("A {0}QUESTIONABLE subordinate{1} may NOT be a traitor. On the other hand if you're being betrayed {2}often{3}, don't take any chances", 
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //
        // - - - Traitor
        //
        //overview
        data = new HelpData();
        data.tag = "traitor_0";
        data.header = "Traitors";
        data.text = string.Format("Whenever a subordinate has been in Captivity there is a chance that they have been persuaded to turn {0}TRAITOR{1} and will inform the Authority of your location from time to time",
            colourAlert, colourEnd);
        listOfHelp.Add(data);
        //chance of Traitor
        data = new HelpData();
        data.tag = "traitor_1";
        data.header = "Chance of Becoming a Traitor";
        data.text = string.Format("There is a base chance that {0}increases{1} with each {2}period of Captivity{3} (doubles if Captured twice, triples if Captured three times, etc.)",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Tip
        data = new HelpData();
        data.tag = "traitor_2";
        data.header = string.Format("{0}Game Tip{1}", colourTip, colourEnd);
        data.text = string.Format("How can you tell if a subordinate is a {0}TRAITOR{1}? You can't. Any leaks could be coming from Rebel HQ but if they become more frequent, take a hard look at any {2}QUESTIONABLE{3} subordinates",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //
        // - - - Questionable Condition
        //
        //Over
        data = new HelpData();
        data.tag = "questionable_0";
        data.header = "Questionable Condition";
        data.text = string.Format("Whenever you, or one of your subordinates, are {0}Captured{1}, you become QUESTIONABLE. People wonder if you've secretly become an {2}informant for the Authority{3}. Your loyalty is in quesiton",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //player
        data = new HelpData();
        data.tag = "questionable_1";
        data.header = "Questionable Player";
        data.text = string.Format("Rebel HQ and your subordinates will {0}doubt your Loyalty{1} to the cause. Your approval with Rebel HQ may suffer and your subordinates may {2}Resign{3}",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //actor
        data = new HelpData();
        data.tag = "questionable_2";
        data.header = "Questionable Subordinate";
        data.text = string.Format("There is a chance that the subordinate secretly becomes a {0}Traitor{1} and will, from time to time, {2}inform the Authority{3} of the Player's whereabouts.",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Tip
        data = new HelpData();
        data.tag = "questionable_3";
        data.header = string.Format("{0}Game Tip{1}", colourTip, colourEnd);
        data.text = string.Format("You can ask Rebel HQ to quietly {0}Dispose Of{1} a QUESTIONABLE subordinate at {2}no cost{3} in Renown if you suspect that they might be a traitor",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //return help
        return listOfHelp;

    
    }

    /// <summary>
    /// allows a help window to pop-up showing the topics specified below. Used for debugging help topics
    /// </summary>
    public void DebugShowHelp()
    {
        string tag0 = "info_app_0";
        string tag1 = "info_app_1";
        string tag2 = "info_app_2";
        string tag3 = "";
        List<HelpData> listOfHelp = GameManager.instance.mainInfoScript.GetHelpData(tag0, tag1, tag2, tag3);
        Vector3 screenPos = new Vector3(Screen.width / 2, Screen.height / 2);
        GameManager.instance.tooltipHelpScript.SetTooltip(listOfHelp, screenPos);
    }

    /// <summary>
    /// debug method to display all keyboard commands
    /// </summary>
    public string DisplayHelp()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat(" Keyboard Commands{0},{1}", "\n", "\n");
        builder.AppendFormat(" End Turn -> Enter{0}", "\n");
        builder.AppendFormat(" End Level -> X{0}{1}", "\n", "\n");
        builder.AppendFormat(" Reserves -> R{0}", "\n");
        builder.AppendFormat(" Gear -> G{0}{1}", "\n", "\n");
        builder.AppendFormat(" Targets -> T{0}", "\n");
        builder.AppendFormat(" Spiders -> S{0}", "\n");
        builder.AppendFormat(" Tracers -> C{0}", "\n");
        builder.AppendFormat(" Teams -> M{0}{1}", "\n", "\n");
        builder.AppendFormat(" Actions -> Left Click{0}", "\n");
        builder.AppendFormat(" Move -> Right Click{0}{1}", "\n", "\n");
        builder.AppendFormat(" Corporate Nodes -> F1{0}", "\n");
        builder.AppendFormat(" Gated Nodes -> F2{0}", "\n");
        builder.AppendFormat(" Government Nodes -> F3{0}", "\n");
        builder.AppendFormat(" Industrial Nodes -> F4{0}", "\n");
        builder.AppendFormat(" Research Nodes -> F5{0}", "\n");
        builder.AppendFormat(" Sprawl Nodes -> F6{0}", "\n");
        builder.AppendFormat(" Utility Nodes -> F7{0}{1}", "\n", "\n");
        builder.AppendFormat(" Debug Show -> D{0}{1}", "\n", "\n");
        builder.AppendFormat(" Activity Time -> F9{0}", "\n");
        builder.AppendFormat(" Activity Count -> F10{0}{1}", "\n", "\n");
        builder.AppendFormat(" MainInfoApp display -> I{0}", "\n");
        builder.AppendFormat(" MainInfoApp ShowMe -> Space{0}", "\n");
        builder.AppendFormat(" MainInfoApp Home -> Home{0}", "\n");
        builder.AppendFormat(" MainInfoApp End -> End{0}", "\n");
        builder.AppendFormat(" MainInfoApp Back -> PageDn{0}", "\n");
        builder.AppendFormat(" MainInfoApp Forward -> PageUp{0}", "\n");
        return builder.ToString();
    }

}
