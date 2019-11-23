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

    #region SetColours
    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        colourTip = GameManager.instance.colourScript.GetColour(ColourType.goodText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.salmonText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }
    #endregion

    #region GetHelpData
    /// <summary>
    /// Get help data (generic). Returns listOfHelp, empty list if a problem. Max four topics, Min ONE topic required
    /// </summary>
    /// <param name="tag0"></param>
    /// <param name="tag1"></param>
    /// <param name="tag2"></param>
    /// <param name="tag3"></param>
    /// <returns></returns>
    public List<HelpData> GetHelpData(string tag0, string tag1 = null, string tag2 = null, string tag3 = null)
    {
        List<HelpData> listOfHelp = new List<HelpData>();
        //first topic, skip if null
        if (string.IsNullOrEmpty(tag0) == false)
        {
            HelpData help0 = GameManager.instance.dataScript.GetHelpData(tag0);
            if (help0 != null)
            { listOfHelp.Add(help0); }
            else { Debug.LogWarningFormat("Invalid HelpData (Null) for tag0 \"{0}\"", tag0); }
        }
        //second topic, skip if null
        if (string.IsNullOrEmpty(tag1) == false)
        {
            HelpData help1 = GameManager.instance.dataScript.GetHelpData(tag1);
            if (help1 != null)
            { listOfHelp.Add(help1); }
            else { Debug.LogWarningFormat("Invalid HelpData (Null) for tag1 \"{0}\"", tag1); }
        }
        //third topic, skip if null
        if (string.IsNullOrEmpty(tag2) == false)
        {
            HelpData help2 = GameManager.instance.dataScript.GetHelpData(tag2);
            if (help2 != null)
            { listOfHelp.Add(help2); }
            else { Debug.LogWarningFormat("Invalid HelpData (Null) for tag2 \"{0}\"", tag2); }
        }
        //fourth topic, skip if null
        if (string.IsNullOrEmpty(tag3) == false)
        {
            HelpData help3 = GameManager.instance.dataScript.GetHelpData(tag3);
            if (help3 != null)
            { listOfHelp.Add(help3); }
            else { Debug.LogWarningFormat("Invalid HelpData (Null) for tag3 \"{0}\"", tag3); }
        }
        return listOfHelp;
    }
    #endregion


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

        #region RandomTab items Help
        //
        // - - - Random Tab additional (single topic, green header) item help (explains meaning of particular item) -> Number sequentially 'rand_x'
        //
        //Compatibility check
        data = new HelpData();
        data.tag = "rand_0";
        data.header = string.Format("{0}Compatibility Check{1}", colourTip, colourEnd);
        builder = new StringBuilder();
        builder.AppendFormat("The actor's {0}Compatibility{1} with you can {2}prevent{3} any change in motivation occurring. ", colourAlert, colourEnd, colourAlert, colourEnd);
        builder.AppendFormat("If they have a good opinion of you {0}they may ignore{1} a drop in motivation and vice versa. ", colourAlert, colourEnd);
        builder.AppendFormat("The {0}more{1} Compatible or Incompatibile they are, the {2}higher the chance{3} of them ignoring the change in motivation", colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Faction support
        data = new HelpData();
        data.tag = "rand_1";
        int approval = 5;
        data.header = string.Format("{0}Faction Support{1}", colourTip, colourEnd);
        builder = new StringBuilder();
        builder.AppendFormat("HQ make a decision, each turn, whether to offer you support ({0}+1 Renown{1}). ", colourAlert, colourEnd);
        builder.AppendFormat("Your {0}chance{1} of receiving support is equal to your level of {2}HQ Approval x 10{3}, eg. Approval {4} so {5}% chance", colourAlert, colourEnd, 
            colourAlert, colourEnd, approval, approval * 10);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Addiction Feed the need check
        data = new HelpData();
        data.tag = "rand_2";
        data.header = string.Format("{0}Addiction Need Check{1}", colourTip, colourEnd);
        builder = new StringBuilder();
        builder.AppendFormat("You're ADDICTED. At random intervals ({0}{1}% chance per turn{2}) you will need to {3}feed your addiction{4}", colourAlert, 
            GameManager.instance.actorScript.playerAddictedChance, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Take Drugs check
        data = new HelpData();
        data.tag = "rand_3";
        data.header = string.Format("{0}Addicted Check{1}", colourTip, colourEnd);
        builder = new StringBuilder();
        builder.AppendFormat("Whenever you take {0}illegal drugs{1} there is a chance that you become {2}ADDICTED{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        #endregion

        #region Faction Support
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
        #endregion

        #region Main Info App
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
        builder.AppendFormat("You can adjust the {0}text scroll speed{1} using the '+' and '-' keys. ", colourAlert, colourEnd);
        builder.AppendFormat("{0}Mouse Over{1} for a news summary", colourAlert, colourEnd);
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
        #endregion

        #region Topic UI
        //
        // - - - Topic UI
        //
        //ignoring decision
        data = new HelpData();
        data.tag = "topicUI_0";
        data.header = "Ignore the Decision";
        builder = new StringBuilder();
        builder.AppendFormat("Decisions can be Ignored ({0}IGNORE Button or ESC{1}) if you don't want to deal with them. ", colourAlert, colourEnd);
        builder.AppendFormat("The Ignore button {0}tooltip{1} shows any {2}Consequences{3} if you do so", colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //options
        data = new HelpData();
        data.tag = "topicUI_1";
        data.header = "Greyed out Options";
        builder = new StringBuilder();
        builder.AppendFormat("Options may be greyed out indicating that they {0}aren't available{1}. Their {2}tooltip{3} will show why", colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //more details
        data = new HelpData();
        data.tag = "topicUI_2";
        data.header = "More Information";
        builder = new StringBuilder();
        builder.AppendFormat("{0}Tooltip, image, top left{1} for more details", colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //HQ Boss
        data = new HelpData();
        data.tag = "topicUI_3";
        data.header = "HQ Boss";
        builder = new StringBuilder();
        builder.AppendFormat("Your Boss has an opinion ({0}tooltip, image, top right{1}) on each option. It is your decision what to do but your {2}Boss keeps track{3} of your choices. ", 
            colourAlert, colourEnd, colourAlert, colourEnd);
        builder.AppendFormat("{0}Ignoring{1} a decision will always earn your Boss's {2}disapproval{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        #endregion

        #region TopicMessage
        //
        // - - - Topic Message
        //
        //overview
        data = new HelpData();
        data.tag = "topicMess_0";
        data.header = "Decision";
        builder = new StringBuilder();
        builder.AppendFormat("This item records the outcome of the decision that you have decided upon at the {0}start of this turn{1}", colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        #endregion

        #region Secrets
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
        #endregion

        #region Random Rolls
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
        data.text = string.Format("If a successful roll indicates a {0}GOOD{1} outcome from the Player's point of view, the header is shown in {2}GREEN{3}.", colourAlert, colourEnd, colourAlert, colourEnd); ;
        listOfHelp.Add(data);
        //bad rolls
        data = new HelpData();
        data.tag = "roll_2";
        data.header = "Bad Rolls";
        data.text = string.Format("If a successful roll indicates a {0}BAD{1} outcome from the Player's point of view, the header is shown in {2}RED{3}.", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Actor Compatibility
        //
        // - - - Actor Compatibility
        //
        //Motivation
        data = new HelpData();
        data.tag = "compatibility_0";
        data.header = "Motivation";
        data.text = new StringBuilder()
            .AppendFormat("Indicates how {0}Enthusiastic{1} a person is. It ranges from {2}0 to 3{3} stars, the more the better. ", colourAlert, colourEnd, colourAlert, colourEnd)
            .AppendFormat("A person's Motivation effects many game mechanics and is something to {0}keep an eye on{1}. As in real life, a person who has lost all enthusiasm will be a problem",
                colourAlert, colourEnd)
            .ToString();
        listOfHelp.Add(data);
        //Compatibiity
        data = new HelpData();
        data.tag = "compatibility_1";
        data.header = "Compatibility";
        data.text = new StringBuilder()
            .AppendFormat("Is a measure of the {0}Relationship{1} between the individual and yourself. It can range from {2}0 to 3{3} stars. ", colourAlert, colourEnd,
                colourAlert, colourEnd)
            .AppendFormat("{0}Green{1} stars indicate a {2}Positive{3} relationship, {4}Red{5} stars a {6}Negative{7} relationship. ", colourAlert, colourEnd, colourAlert, colourEnd,
                colourAlert, colourEnd, colourAlert, colourEnd)
            .AppendFormat("The number of stars indicate the {0}Intensity{1} of your relationship (if they are Green then more the better, if Red then the more, the worse your relationship).",
                colourAlert, colourEnd)
            .ToString();
        listOfHelp.Add(data);
        //Positive Compatibility
        data = new HelpData();
        data.tag = "compatibility_2";
        data.header = "Positive Compatibility";
        data.text = new StringBuilder()
            .AppendFormat("An individual who has a positive opinion of you has a {0}chance of ignoring{1} any {2}BAD{3} Motivational outcomes. ", colourAlert, colourEnd, colourAlert, colourEnd)
            .AppendFormat("The more {0}Green stars{1} (better your relationship), the more likely this is to happen", colourAlert, colourEnd)
            .ToString();
        listOfHelp.Add(data);
        //Negative Compatibility
        data = new HelpData();
        data.tag = "compatibility_3";
        data.header = "Negative Compatibility";
        data.text = new StringBuilder()
            .AppendFormat("An individual who has a negative opinion of you has a {0}chance of ignoring{1} any {2}GOOD{3} Motivational outcomes. ", colourAlert, colourEnd, colourAlert, colourEnd)
            .AppendFormat("The more {0}Red stars{1} (worse your relationship), the more likely this is to happen", colourAlert, colourEnd)
            .ToString();
        listOfHelp.Add(data);
        #endregion

        #region Actor Conflic
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
        #endregion

        #region Player Betrayed
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
        #endregion

        #region Traitor
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
        #endregion

        #region Questionable Condition
        //
        // - - - Questionable Condition
        //
        //Overview
        data = new HelpData();
        data.tag = "questionable_0";
        data.header = "Questionable Condition";
        data.text = string.Format("Whenever you, or one of your subordinates, are {0}Captured{1}, you become QUESTIONABLE. People wonder if you've secretly become an {2}informant for the Authority{3}. Your loyalty is in question",
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
        #endregion

        #region Stressed Condition
        //
        // - - - Stressed Condition
        //
        //Overview
        data = new HelpData();
        data.tag = "stress_0";
        data.header = "Stressed Condition";
        data.text = string.Format("Gained when your {0}Mood drops below 0{1} or from other causes. For every turn that you are Stressed there is a {2}chance of a Breakdown{3}",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Breakdowns
        data = new HelpData();
        data.tag = "stress_1";
        data.header = "Breakdowns";
        data.text = string.Format("A stress induced Nervous Breakdown {0}prevents you from doing anything that turn{1}. You are {2}vulnerable{3} and can be captured or worse",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Chance of Breakdowns
        data = new HelpData();
        data.tag = "stress_2";
        data.header = "Chance of Breakdowns";
        data.text = string.Format("There is {0}{1} %{2} chance of a Breakdown each turn. Subordinates can have {3}traits{4} that modify this. If you become Stressed when you are already Stressed, the {5}odds increase{6}",
            colourAlert, GameManager.instance.actorScript.breakdownChance, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Recovering from Stress
        data = new HelpData();
        data.tag = "stress_3";
        data.header = "Removing Stress";
        data.text = string.Format("{0}Lying Low and Stress Leave{1} both remove the Stressed Condition. Certain {2}stimulants{3} (Persuasion Gear) can also do so (Personal Use)",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Addicted Condition
        //
        // - - - Addicted Condition
        //
        //Overview
        data = new HelpData();
        data.tag = "addict_0";
        data.header = "Addicted Condition";
        data.text = string.Format("You have become addicted to the drug {0}{1}{2} and at periodic intervals will need to {3}feed your addiction{4}",
            colourAlert, GameManager.instance.globalScript.tagGlobalDrug, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Breakdowns
        data = new HelpData();
        data.tag = "addict_1";
        data.header = "Feeding your Habit";
        data.text = string.Format("If you fail your Addiction Check you will need to spend {0}{1} Renown{2} to obtain enough {3}{4}{5} to feed your Addiction. ",
            colourAlert, GameManager.instance.actorScript.playerAddictedRenownCost, colourEnd, colourAlert, GameManager.instance.globalScript.tagGlobalDrug, colourEnd);
        listOfHelp.Add(data);
        //Chance of Breakdowns
        data = new HelpData();
        data.tag = "addict_2";
        data.header = "Insufficient Renown";
        data.text = string.Format("If you don't have enough Renown on hand you are assumed to suffer {0}withdrawal symptoms{1}. HQ will notice and express their disapproval ({2}Approval -1{3})",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Recovering from Addiction
        data = new HelpData();
        data.tag = "addict_3";
        data.header = "Overcoming your Addiction";
        data.text = string.Format("You'll need to find somewhere, or somebody, that offers a {0}cure{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Immunity to Stress
        //
        // - - - Immune to Stress
        //
        //Overview
        data = new HelpData();
        data.tag = "immune_0";
        data.header = "Stress Immunity";
        data.text = string.Format("You've taken a dose of {0}{1}{2} and have developed an immunity from stress for a set number of days",
            colourAlert, GameManager.instance.globalScript.tagGlobalDrug, colourEnd);
        listOfHelp.Add(data);
        //Decreasing effect
        data = new HelpData();
        data.tag = "immune_1";
        data.header = "Diminishing Returns";
        int immuneMin = GameManager.instance.actorScript.playerAddictedImmuneMin;
        data.text = string.Format("Every time you take a dose of {0}{1}{2} your period of immunity {3}decreases by one day{4} down to a minimum of {5}{6}{7} day{8}", colourAlert,
            GameManager.instance.globalScript.tagGlobalDrug, colourEnd, colourAlert, colourEnd, colourAlert, immuneMin, colourEnd, immuneMin != 1 ? "s" : "");           
        listOfHelp.Add(data);
        #endregion

        #region Stress Leave
        //
        // - - - Stress Leave
        //
        //Overview
        data = new HelpData();
        data.tag = "stressLeave_0";
        data.header = "Stress Leave";
        data.text = string.Format("Is only available when your, or your subordinate, (Resistance only) has {0}Invisibility{1} at Max ({2} stars), and are {3}STRESSED{4}. You also need permission from HQ ({5}{6}{7})",
            colourAlert, colourEnd, GameManager.instance.actorScript.maxStatValue, colourAlert, colourEnd, colourAlert, 
            GameManager.instance.actorScript.stressLeaveHQApproval == true ? "Given" : "Denied", colourEnd);
        listOfHelp.Add(data);
        //Breakdowns
        data = new HelpData();
        data.tag = "stressLeave_1";
        data.header = "Taking Stress Leave";
        data.text = string.Format("It's {0}quicker{1} (one turn) than Lying Low but costs {2}{3}{4} Renown. You are {5}Safe{6} while taking Stress Leave",
            colourAlert, colourEnd, colourAlert,
            GameManager.instance.sideScript.PlayerSide.level == 1 ? GameManager.instance.actorScript.stressLeaveRenownCostAuthority : GameManager.instance.actorScript.stressLeaveRenownCostResistance, 
            colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Chance of Breakdowns
        data = new HelpData();
        data.tag = "stressLeave_2";
        data.header = "Surveillance Crackdown";
        data.text = string.Format("If this Security Measure is in place the Resistance {0}can't take Stress Leave{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Recovering from Stress
        data = new HelpData();
        data.tag = "stressLeave_3";
        data.header = "Stress Leave Effects";
        data.text = "Removes Stress and improves your mood";
        listOfHelp.Add(data);
        #endregion

        #region Cure
        //
        // - - - Cure (condition)
        //
        //Overview
        data = new HelpData();
        data.tag = "cure_0";
        data.header = "Cure";
        data.text = string.Format("Certain conditions have cures available that allow the {0}condition to be removed{1}. Cures apply {2}only{3} to the Player", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //How to
        data = new HelpData();
        data.tag = "cure_1";
        data.header = "How To";
        data.text = string.Format("To use a cure {0}move{1} to the specified district, {2}Right Click{3} for the Action Menu, Select {4}Cure{5}", colourAlert, colourEnd, colourAlert, colourEnd, 
            colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Details
        data = new HelpData();
        data.tag = "cure_2";
        data.header = "Details";
        data.text = string.Format("There is {0}no Renown or Invisibilty penalty{1} for a cure but an {2}Action{3} will be used. Cures, once available, do not {4}time out{5}", colourAlert, colourEnd, 
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Tip
        data = new HelpData();
        data.tag = "cure_3";
        data.header = string.Format("{0}Game Tip{1}", colourTip, colourEnd);
        data.text = "If a cure is available you generally don't want to waste time making use of it. Who knows if it'll still be there tomorrow?";
        listOfHelp.Add(data);
        #endregion

        #region Lying Low
        //
        // - - - Lying Low
        //
        //Overview
        data = new HelpData();
        data.tag = "lielow_0";
        data.header = "Lying Low";
        data.text = string.Format("Resistance only. You, or your subordinate, must have Invisibility {0}less than{1} the Max ({2}{3} stars{4}) and the Lie Low {5}Timer{6} must be {7}Zero{8}",
            colourAlert, colourEnd, colourAlert, GameManager.instance.actorScript.maxStatValue, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Breakdowns
        data = new HelpData();
        data.tag = "lielow_1";
        data.header = "Lie Low Timer";
        data.text = string.Format("Lying Low involves Resistance HQ sourcing a suitable, safe, location. This takes time and effort. The {0}Effects Tab{1} in the {2}App{3} lets you know when Lying Low will be next available",
            colourAlert, colourEnd, colourAlert,  colourEnd);
        listOfHelp.Add(data);
        //Chance of Breakdowns
        data = new HelpData();
        data.tag = "lielow_2";
        data.header = "Surveillance Crackdown";
        data.text = string.Format("If this Security Measure is in place, safe locations aren't available and Lying Low {0}isn't possible{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Recovering from Stress
        data = new HelpData();
        data.tag = "lielow_3";
        data.header = "Lying Low Effects";
        data.text = string.Format("Removes Stress, increases your Invisibility (to {0}{1} stars{2}) and improves your Mood (to {3}{4} stars{5}, Player only)", colourAlert, 
            GameManager.instance.actorScript.maxStatValue, colourEnd, colourAlert, GameManager.instance.playerScript.moodReset, colourEnd); ;
        listOfHelp.Add(data);
        #endregion

        #region Mood
        //
        // - - - Mood
        //
        //Overview
        data = new HelpData();
        data.tag = "mood_0";
        data.header = "Mood";
        data.text = string.Format("Only you, the Player, have a mood. Doing {0}actions in line with your beliefs{1} (defined by your personality) improves your mood, the opposite worsens it.", 
            colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Stressed
        data = new HelpData();
        data.tag = "mood_1";
        data.header = "Mood and Stress";
        data.text = string.Format("Your mood moves between 0 and {0}. If it drops {1}below Zero{2} you become {3}STRESSED{4}", GameManager.instance.playerScript.moodMax, colourAlert, colourEnd,
            colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Improving mood
        data = new HelpData();
        data.tag = "mood_2";
        data.header = "Improving your Mood";
        data.text = string.Format("{0}Lying Low, Stress Leave and Doing Nothing{1} (Unused actions at the end of your turn) all improve your mood as does {2}any action that aligns with your beliefs{3}", 
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Tip
        data = new HelpData();
        data.tag = "mood_3";
        data.header = string.Format("{0}Game Tip{1}", colourTip, colourEnd);
        data.text = string.Format("As in real life doing things that you feel strongly about will change your mood. {0}Avoid STRESSING{1} yourself out if you can",
            colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Nemesis
        //
        // - - - Nemesis
        //
        //Stealth
        data = new HelpData();
        data.tag = "nemesis_0";
        data.header = "Stealth Rating";
        builder = new StringBuilder();
        builder.AppendFormat("Nemesis have a Stealth Rating ranging from {0}0 (low) to 3 (high).{1}", colourAlert, colourEnd, colourAlert, colourEnd);
        builder.AppendFormat("The rating changes depending on their mode{0}  * {1}Hunt{2} mode {3}-1{4} Stealth{5}  * {6}Ambush{7} mode {8}INVISIBLE{9}",
            "\n", colourAlert, colourEnd, colourAlert, colourEnd, "\n", colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        #endregion

        #region Npc
        //
        // - - - Npc
        //
        //Overview
        data = new HelpData();
        data.tag = "npc_0";
        data.header = "Overview";
        builder = new StringBuilder();
        builder.AppendFormat("Special Characters arrive in the city and conduct their business before departing. They {0}aren't visible{1} but can be spotted by {2}Contacts or Tracers{3}.", 
            colourAlert, colourEnd, colourAlert, colourEnd);        
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Interaction
        data = new HelpData();
        data.tag = "npc_1";
        data.header = "Interacting";
        builder = new StringBuilder();
        builder.AppendFormat("You {0}automatically{1} find and interact with a Special Character if you {2}end your turn{3} in the {4}same district{5}", colourAlert, colourEnd, colourAlert, colourEnd,
            colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Movement
        data = new HelpData();
        data.tag = "npc_2";
        data.header = "Movement";
        builder = new StringBuilder();
        builder.AppendFormat("Special Characters can move {0}ONE{1} district a turn, at most", colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Stealth
        data = new HelpData();
        data.tag = "npc_3";
        data.header = "Stealth";
        builder = new StringBuilder();
        builder.AppendFormat("Special Characters (and NEMESIS) have a Stealth Rating ranging from {0}0 (low) to 3 (high){1}. ", colourAlert, colourEnd, colourAlert, colourEnd);
        builder.AppendFormat("A Contact will {0}spot them{1} if their Effectiveness is {2}Equal to or Greater{3} than the Character's Stealth Rating", colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Effects
        data = new HelpData();
        data.tag = "npc_4";
        data.header = "Benefits and Penalties";
        builder = new StringBuilder();
        builder.AppendFormat("HQ will task you with interacting with Special Characters. Doing so {0}before they depart{1} generally grants a benefit and failing to do so incurs a penalty", 
            colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Tracers
        data = new HelpData();
        data.tag = "npc_5";
        data.header = "Tracers";
        builder = new StringBuilder();
        builder.AppendFormat("Tracers will {0}automatically spot{1} a Special Character in the {2}same district{3} regardless of their Stealth Rating",  colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        #endregion

        #region Contact
        //
        // - - - Contact (Resistance)
        //
        //resistance overview
        data = new HelpData();
        data.tag = "contact_0";
        data.header = "Overview";
        builder = new StringBuilder();
        builder.AppendFormat("Contacts work for your subordinates and are your {0}eyes and ears{1} on the ground.", colourAlert, colourEnd);
        builder.AppendFormat("Contacts enable your subordinates to carry out {0}Actions{1} and can report on all manner of Authority activity", colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Effectiveness
        data = new HelpData();
        data.tag = "contact_1";
        data.header = "Effectiveness";
        builder = new StringBuilder();
        builder.AppendFormat("Effectiveness is a measure of the Contact's {0}ability{1} to source useful information.", colourAlert, colourEnd);
        builder.AppendFormat("It ranges from {0}{1}  1{2} '...knows stuff' (worst){3}{4}  2{5} '...is networked'{6}{7}  3{8} '...is Wired-in' (best)", 
            "\n", colourAlert, colourEnd, "\n", colourAlert, colourEnd, "\n", colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Contacts and the Authority
        data = new HelpData();
        data.tag = "contact_2";
        data.header = "Contacts and the Authority";
        builder = new StringBuilder();
        builder.AppendFormat("Contacts can become {0}known{1} to the Authority (if a {2}Probe Team{3} is in the same district) and can, on occasion, be {4}Erased{5} (Erasure Team in same district)", 
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Target Rumours
        data = new HelpData();
        data.tag = "contact_3";
        data.header = "Target Rumours";
        builder = new StringBuilder();
        builder.AppendFormat("Contacts can learn rumours of new targets that will appear in the future. Their {0}information{1} can be relied upon as {2}accurate{3}.", 
            colourAlert, colourEnd, colourAlert, colourEnd);
        builder.AppendFormat("Contacts with {0}High{1} Effectiveness are {2}more likely{3} to hear rumours than those with low Effectiveness",colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //New Contact
        data = new HelpData();
        data.tag = "contact_4";
        data.header = "New Contact";
        builder = new StringBuilder();
        builder.AppendFormat("Your subordinate will now be able to {0}carry out actions{1} in the new contact's {2}District{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Lost Contact
        data = new HelpData();
        data.tag = "contact_5";
        data.header = "Lost Contact";
        builder = new StringBuilder();
        builder.AppendFormat("Your subordinate will {0}No Longer{1} be able to carry out actions in their ex-contact's {2}District{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Nemesis
        data = new HelpData();
        data.tag = "contact_6";
        data.header = "Spotting Nemesis";
        builder = new StringBuilder();
        builder.AppendFormat("If a Nemesis is in the {0}same district{1} as the Contact they will be spotted if the {2}Contact's Effectiveness is >= Nemesis's Stealth rating{3}", 
            colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Teams
        data = new HelpData();
        data.tag = "contact_7";
        data.header = "Spotting Teams";
        builder = new StringBuilder();
        builder.AppendFormat("Any teams in the {0}same district{1} as the Contact will be {2}spotted automatically{3} (it's hard to hide a team)", colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Inactive
        data = new HelpData();
        data.tag = "contact_8";
        data.header = "Gone Silent";
        builder = new StringBuilder();
        builder.AppendFormat("A Contact who has gone silent will {0}no longer{1} provide {2}information{3} or enable your subordinate to carry out {4}actions{5} in the Contact's district", 
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //How long Inactive
        data = new HelpData();
        data.tag = "contact_9";
        data.header = "For How Long?";
        builder = new StringBuilder();
        builder.AppendFormat("You can check the {0}Effects Tab{1} in the InfoApp {2}next turn{3} to see when your Contact will {4}return{5}", 
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Return to Active
        data = new HelpData();
        data.tag = "contact_10";
        data.header = "Back on the Grid";
        builder = new StringBuilder();
        builder.AppendFormat("A Contact who has returned will have their ears to the ground for {0}information{1} and will allow your subordinate to carry out {2}actions{3} in the Contact's district",
            colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        #endregion


        //
        // - - - Return
        //
        //return help
        return listOfHelp;


    }

    /// <summary>
    /// allows a help window to pop-up showing the topics specified below. Used for debugging help topics
    /// </summary>
    public void DebugShowHelp()
    {
        string tag0 = "npc_0";
        string tag1 = "npc_1";
        string tag2 = "npc_2";
        string tag3 = "npc_3";
        List<HelpData> listOfHelp = GetHelpData(tag0, tag1, tag2, tag3);
        Vector3 screenPos = new Vector3(Screen.width / 2, Screen.height / 2);
        GameManager.instance.tooltipHelpScript.SetTooltip(listOfHelp, screenPos);
    }

    /// <summary>
    /// debug method to display all keyboard commands
    /// </summary>
    public string DisplayHelp()
    {
        return new StringBuilder()
            .AppendFormat(" Keyboard Commands{0},{1}", "\n", "\n")
            .AppendFormat(" End Turn -> Enter{0}", "\n")
            .AppendFormat(" End Level -> X{0}{1}", "\n", "\n")
            .AppendFormat(" Reserves -> R{0}", "\n")
            .AppendFormat(" Gear -> G{0}{1}", "\n", "\n")
            .AppendFormat(" Targets -> T{0}", "\n")
            .AppendFormat(" Spiders -> S{0}", "\n")
            .AppendFormat(" Tracers -> C{0}", "\n")
            .AppendFormat(" AutoRun -> A{0}", "\n")
            .AppendFormat(" Teams -> M{0}{1}", "\n", "\n")
            .AppendFormat(" Actions -> Left Click{0}", "\n")
            .AppendFormat(" Move -> Right Click{0}{1}", "\n", "\n")
            .AppendFormat(" Corporate Nodes -> F1{0}", "\n")
            .AppendFormat(" Gated Nodes -> F2{0}", "\n")
            .AppendFormat(" Government Nodes -> F3{0}", "\n")
            .AppendFormat(" Industrial Nodes -> F4{0}", "\n")
            .AppendFormat(" Research Nodes -> F5{0}", "\n")
            .AppendFormat(" Sprawl Nodes -> F6{0}", "\n")
            .AppendFormat(" Utility Nodes -> F7{0}{1}", "\n", "\n")
            .AppendFormat(" Debug Show -> D{0}{1}", "\n", "\n")
            .AppendFormat(" Activity Time -> F9{0}", "\n")
            .AppendFormat(" Activity Count -> F10{0}{1}", "\n", "\n")
            .AppendFormat(" MainInfoApp display -> I{0}", "\n")
            .AppendFormat(" MainInfoApp ShowMe -> Space{0}", "\n")
            .AppendFormat(" MainInfoApp Home -> Home{0}", "\n")
            .AppendFormat(" MainInfoApp End -> End{0}", "\n")
            .AppendFormat(" MainInfoApp Back -> PageDn{0}", "\n")
            .AppendFormat(" MainInfoApp Forward -> PageUp{0}", "\n")
            .ToString();
    }

}
