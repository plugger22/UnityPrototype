﻿using gameAPI;
using packageAPI;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// handles all Help related matters
/// </summary>
public class HelpManager : MonoBehaviour
{
    //bullet character for help topics
    char bullet;
    string arrowLeft;
    string arrowRight;

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
        //fast access
        bullet = GameManager.i.guiScript.bulletChar;
        arrowLeft = GameManager.i.guiScript.arrowIconLeft;
        arrowRight = GameManager.i.guiScript.arrowIconRight;
        Debug.Assert(string.IsNullOrEmpty(arrowLeft) == false, "Invalid arrowLeft (Null or Empty)");
        Debug.Assert(string.IsNullOrEmpty(arrowRight) == false, "Invalid arrowRight (Null or Empty)");
        //register listener
        EventManager.i.AddListener(EventType.ChangeColour, OnEvent, "ItemDataManager");
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
        colourTip = GameManager.i.colourScript.GetColour(ColourType.goodText);
        colourAlert = GameManager.i.colourScript.GetColour(ColourType.salmonText);
        colourEnd = GameManager.i.colourScript.GetEndTag();
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
            HelpData help0 = GameManager.i.dataScript.GetHelpData(tag0);
            if (help0 != null)
            { listOfHelp.Add(help0); }
            else { Debug.LogWarningFormat("Invalid HelpData (Null) for tag0 \"{0}\"", tag0); }
        }
        //second topic, skip if null
        if (string.IsNullOrEmpty(tag1) == false)
        {
            HelpData help1 = GameManager.i.dataScript.GetHelpData(tag1);
            if (help1 != null)
            { listOfHelp.Add(help1); }
            else { Debug.LogWarningFormat("Invalid HelpData (Null) for tag1 \"{0}\"", tag1); }
        }
        //third topic, skip if null
        if (string.IsNullOrEmpty(tag2) == false)
        {
            HelpData help2 = GameManager.i.dataScript.GetHelpData(tag2);
            if (help2 != null)
            { listOfHelp.Add(help2); }
            else { Debug.LogWarningFormat("Invalid HelpData (Null) for tag2 \"{0}\"", tag2); }
        }
        //fourth topic, skip if null
        if (string.IsNullOrEmpty(tag3) == false)
        {
            HelpData help3 = GameManager.i.dataScript.GetHelpData(tag3);
            if (help3 != null)
            { listOfHelp.Add(help3); }
            else { Debug.LogWarningFormat("Invalid HelpData (Null) for tag3 \"{0}\"", tag3); }
        }
        return listOfHelp;
    }

    /// <summary>
    /// Returns a colour formatted string from a specific help tag. Returns null if a problem
    /// NOTE: For use with Generic Tooltips.
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public string GetHelpText(string tag)
    {
        if (string.IsNullOrEmpty(tag) == false)
        {
            HelpData help = GameManager.i.dataScript.GetHelpData(tag);
            if (help != null)
            { return help.text; }
            else { Debug.LogWarningFormat("Invalid HelpData (Null) for tag \"{0}\"", tag); }
        }
        return null;
    }

    /// <summary>
    /// Get help data generic (overloaded for a list input, only the first four entries are taken into account)
    /// </summary>
    /// <param name="listOfHelpStrings"></param>
    /// <returns></returns>
    public List<HelpData> GetHelpData(List<string> listOfHelpStrings)
    {
        List<HelpData> listOfHelp = new List<HelpData>();
        if (listOfHelpStrings != null)
        {
            string tag;
            //max four entries
            int count = Mathf.Min(listOfHelpStrings.Count, 4);
            for (int i = 0; i < count; i++)
            {
                tag = listOfHelpStrings[i];
                if (string.IsNullOrEmpty(tag) == false)
                {
                    HelpData help = GameManager.i.dataScript.GetHelpData(tag);
                    if (help != null)
                    { listOfHelp.Add(help); }
                    else { Debug.LogWarningFormat("Invalid HelpData (Null) for tag \"{0}\"", tag); }
                }
            }
        }
        else { Debug.LogWarning("Invalid listOfHelpStrings (Null)"); }
        return listOfHelp;
    }

    #endregion


    /// <summary>
    /// creates item Help data. Returns an empty list if none
    /// NOTE: Use stringbuilder for help topics with multiple lines only, too much overhead for single lines
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



        #region ...GUI

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

        #region TransitionUI
        //
        // - - - Transition UI
        //
        #region Main
        //
        // - - - Main
        //
        //Keyboard
        data = new HelpData();
        data.tag = "transitionMain_0";
        data.header = "Keyboard Shortcuts";
        data.text = string.Format("{0} {1}Left{2} and {3} {4}Right Arrow{5} keys to cycle through pages", arrowLeft, colourAlert, colourEnd, arrowRight, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region End Level
        //
        // - - - End Level
        //
        //overview
        data = new HelpData();
        data.tag = "transitionEnd_0";
        data.header = "Overview";
        data.text = string.Format("Each HQ member will grant you {0}Renown{1} according to their {2}assessment{3}. Each assesses you on {4}three criteria{5}", 
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //renown
        data = new HelpData();
        data.tag = "transitionEnd_1";
        data.header = "Renown";
        data.text = string.Format("Their {0}Overall{1} assessment ({2}weighted{3} average of all three criteria) determines how much renown you are given. {4}Senior{5} HQ members will can grant" +
            ", potentially, {6}more renown{7} than junior members", colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //factors
        data = new HelpData();
        data.tag = "transitionEnd_2";
        data.header = "Criteria";
        data.text = new StringBuilder()
            .AppendFormat("{0}Objectives{1}{2}", colourAlert, colourEnd, "\n")
            .AppendFormat("3 stars if you have completed all objectives. The most important criteria{0}", "\n")
            .AppendFormat("{0}City Support{1}{2}", colourAlert, colourEnd, "\n")
            .AppendFormat("The level of City Support for the Authority at the end of the level (lower the better){0}", "\n")
            .AppendFormat("{0}Opinion{1}{2}", colourAlert, colourEnd, "\n")
            .AppendFormat("HQ Members personal opinion of you (<b>Motivation</b>){0}", "\n")
            .AppendFormat("{0}Targets{1}{2}", colourAlert, colourEnd, "\n")
            .AppendFormat("One star for each successfully completed Target{0}", "\n")
            .AppendFormat("{0}Exposure{1}{2}", colourAlert, colourEnd, "\n")
            .AppendFormat("Your Innocence level at the end of the level{0}", "\n")
            .AppendFormat("{0}District Crisis{1}{2}", colourAlert, colourEnd, "\n")
            .AppendFormat("One star for each crisis that explodes{0}", "\n")
            .AppendFormat("{0}HQ Approval{1}{2}", colourAlert, colourEnd, "\n")
            .AppendFormat("Your HQ Approval at the end of the level (higher the better){0}", "\n")
            .AppendFormat("{0}Reviews{1}{2}", colourAlert, colourEnd, "\n")
            .AppendFormat("Commendations less Blackmarks for this level{0}", "\n")
            .AppendFormat("{0}Investigations{1}{2}", colourAlert, colourEnd, "\n")
            .AppendFormat("3 stars if no investigations launched, 0 stars otherwise{0}", "\n")
            .ToString();
        listOfHelp.Add(data);
        //Tip
        data = new HelpData();
        data.tag = "transitionEnd_3";
        data.header = string.Format("{0}Game Tip{1}", colourTip, colourEnd);
        data.text = string.Format("Pay attention to the {0}criteria{1}. The first criteria is {2}3 times more important{3} (weighting) than the last. HQ {4}seniority{5} counts. " +
            "Each {6}Overall{7} star from the most senior member is worth {8}4x more renown{9} than a star from the most junior member", 
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Objective Overview
        data = new HelpData();
        data.tag = "transitionEnd_4";
        data.header = "Overview";
        data.text = string.Format("{0}Completing{1} Objectives is how you gain the {2}greatest renown{3}. They are the first criteria of the most senior HQ member and are, as a consequence, worth the most",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Objective pop-up
        data = new HelpData();
        data.tag = "transitionEnd_5";
        data.header = "Assessment";
        data.text = string.Format("All Objectives are {0}weighted the same{1}. Having close to {2}100% completion{3} of all objectives is the equivalent of a {4}3 star{5} assessment",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Hq Status
        //
        // - - - HQ status
        //
        //HQ events
        data = new HelpData();
        data.tag = "transitionHq_0";
        data.header = "HQ Events";
        data.text = string.Format("Things happen and change people's {0}renown{1} which, in turn, {2}determines{3}, who has what {4}position{5} in the hierarchy ({6}highest to lowest{7})",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Type of Event
        data = new HelpData();
        data.tag = "transitionHq_1";
        data.header = "Traits";
        data.text = string.Format("A person's {0}Trait{1} strongly {2}influences{3} what happens to them while at HQ", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Tip
        data = new HelpData();
        data.tag = "transitionHq_2";
        data.header = string.Format("{0}Game Tip{1}", colourTip, colourEnd);
        data.text = string.Format("These happen in the background and are {0}outside of your control{1} however you can {2}influence{3} who gains or loses renown through {4}HQ events{5}" +
            " which can shake up the hierarchy", colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Hq main overview
        data = new HelpData();
        data.tag = "transitionHq_3";
        data.header = "HQ Events";
        data.text = string.Format("Your HQ consists of a {0}{1}{2} person {3}hierarchy{4} (the {5}important{6} people that you interact with) and a group of workers who {7}aspire{8} to be promoted",
            colourAlert, GameManager.i.hqScript.numOfActorsHQ, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //promotion
        data = new HelpData();
        data.tag = "transitionHq_4";
        data.header = "Promotion";
        data.text = string.Format("Promotion is based around {0}renown{1}. The person with the most renown gets the job. The {2}hierarchy{3} are those with the highest renown, in " +
            "{4}descending order of importance{5}", colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //workers
        data = new HelpData();
        data.tag = "transitionHq_5";
        data.header = "Workers";
        data.text = string.Format("At the {0}end{1} of every level, there is a chance that some of your {2}subordinates{3} (current, or previous) may get {4}moved{5} to HQ as a Worker. " +
            "From here they can, with the right circumstances, climb there way up the ladder to a hierarchy position of power", colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //game tip
        data = new HelpData();
        data.tag = "transitionHq_6";
        data.header = string.Format("{0}Game Tip{1}", colourTip, colourEnd);
        data.text = string.Format("The HQ heirarchy are the people who {0}may help you{1}. Opinions matter. Aim to {2}Promote{3} subordinates ({4}MANAGE{5} action)" +
            " who have a favourable view of you ({6}Compatibility{7})", colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Player Status
        //
        // - - - Player Status
        //
        //Overview
        data = new HelpData();
        data.tag = "transitionPlayer_0";
        data.header = "Current Status";
        data.text = string.Format("Shows all the things that {0}currently affect you{1} and which will {2}carry over{3} (unless dealt with) to the {4}next City{5}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Deal with
        data = new HelpData();
        data.tag = "transitionPlayer_1";
        data.header = "How do Get Rid of Them?";
        data.text = string.Format("You will be given the {0}opportunity{1} by various HQ Hierarchy members to take care of your problems (it'll {2}cost you{3} Renown) {4}before{5} your next mission",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Briefing One
        //
        // - - - Briefing One
        //
        //Overview
        data = new HelpData();
        data.tag = "transitionOne_0";
        data.header = "Briefing";
        data.text = string.Format("HQ won't send you in blind. They have prepared a short dossier of {0}key information{1} to enable you to hit the ground running", colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Briefing Two
        //
        // - - - Briefing Two
        //
        //Overview
        data = new HelpData();
        data.tag = "transitionTwo_0";
        data.header = "Objectives";
        data.text = string.Format("HQ have specific {0}requirements{1} for what they would like you to {2}achieve{3}. Wild men or women who do their own thing {4}aren't welcome{5} in the Resistance", 
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #endregion

        #region MetaGame UI
        //
        // - - - Meta Game UI
        //
        //overview
        data = new HelpData();
        data.tag = "metaGameUI_0";
        data.header = "HQ Assistance";
        data.text = string.Format("Your HQ is standing by to offer assistance {0}prior to your next assignment{1}. Each member of your HQ has a {2}unique range of services{3} that you can access. ",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //controls
        data = new HelpData();
        data.tag = "metaGameUI_1";
        data.header = "Controls";
        data.text = new StringBuilder()
            .AppendFormat("The following controls are available{0}", "\n")
            .AppendFormat(" {0} {1}PgUp & PgDown{2}, cycles through the {3}Side Tabs{4}{5}", bullet, colourAlert, colourEnd, colourAlert, colourEnd, "\n")
            .AppendFormat(" {0} {1} {2}Left{3} & {4} {5}Right Arrows{6}, cycles {7}Top Tabs{8}{9}", bullet, arrowLeft, colourAlert, colourEnd, arrowRight, 
            colourAlert, colourEnd, colourAlert, colourEnd, "\n")
            .AppendFormat(" {0} {1}Right Click{2} an Option to {3}Select/Deselect{4}{5}", bullet, colourAlert, colourEnd, colourAlert, colourEnd, "\n")
            .AppendFormat(" {0} {1}SPACE{2} to {3}Select/Deselect{4} highlighted Option{5}", bullet, colourAlert, colourEnd, colourAlert, colourEnd, "\n")
            .AppendFormat(" {0} {1}Mouse Wheel{2} to {3}scroll{4} through options{5}", bullet, colourAlert, colourEnd, colourAlert, colourEnd, "\n")
            .AppendFormat(" {0} {1}Up & Down Arrows{2} to {3}scroll{4} through options{5}", bullet, colourAlert, colourEnd, colourAlert, colourEnd, "\n")
            .ToString();
        listOfHelp.Add(data);
        //Max limit
        data = new HelpData();
        data.tag = "metaGameUI_2";
        data.header = "How many can I select?";
        data.text = string.Format("You can select up to {0}{1} options{2} of any type, provided you have {3}enough renown{4} to pay for them",
            colourAlert, GameManager.i.metaScript.numOfChoices, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Carry Over Renown
        data = new HelpData();
        data.tag = "metaGameUI_3";
        data.header = string.Format("{0}Do I have to?{1}", colourTip, colourEnd);
        data.text = string.Format("You don't have to select any options. Any {0}renown{1} you have will {2}carry over{3} to your {4}next assignment{5}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Reset button
        data = new HelpData();
        data.tag = "metaGameUI_4";
        data.header = "Reset Button";
        data.text = string.Format("Any {0}previously selected{1} Options will be {2}deselected{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Reset button tip
        data = new HelpData();
        data.tag = "metaGameUI_5";
        data.header = string.Format("{0}Why would I Reset?{1}", colourTip, colourEnd);
        data.text = string.Format("It will give you a {0}blank slate{1} from which to choose options again, or allow you to exit (press CONFIRM) and {2}save your renown{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Confirm button
        data = new HelpData();
        data.tag = "metaGameUI_6";
        data.header = "Confirm Button";
        data.text = string.Format("{0}Selected{1} Options will be {2}locked in{3} and any {4}remaining renown{5} will carry over to your {6}next assignment{7}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Confirm button tip
        data = new HelpData();
        data.tag = "metaGameUI_7";
        data.header = string.Format("{0}Can I press this if I haven't selected anything?{1}", colourTip, colourEnd);
        data.text = string.Format("Yes you can. You'll exit the HQ Options and your {0}renown{1} will carry over to your {2}next assignment{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Recommended button
        data = new HelpData();
        data.tag = "metaGameUI_8";
        data.header = "Recommended Button";
        data.text = string.Format("Recommended options will be selected, within your Renown allowance, and {0}all other options deselected{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Recommended button tip
        data = new HelpData();
        data.tag = "metaGameUI_9";
        data.header = string.Format("{0}What's Recommended?{1}", colourTip, colourEnd);
        data.text = string.Format("It's recommended that you deal with any outstanding {0}liabilities{1} such as {2}Secrets{3}, {4}Investigations{5}, adverse {6}Conditions{7} and that you maintain contact with {8}Organisations{9}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Status tab
        data = new HelpData();
        data.tag = "metaGameUI_10";
        data.header = "Status Tab";
        data.text = string.Format("Shows all options that will {0}carry over{1} to your next assignment if not dealt with", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Status tab -> organisations
        data = new HelpData();
        data.tag = "metaGameUI_11";
        data.header = "Organisation";
        data.text = string.Format("Any Organisations that you have {0}been in contact{1} with are shown and, if their option is {2}NOT{3} selected, contact with them will be {4}lost{5}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Status tab -> can I select here?
        data = new HelpData();
        data.tag = "metaGameUI_12";
        data.header = string.Format("{0}Can I Select here?{1}", colourTip, colourEnd);
        data.text = string.Format("Yes you can. You can {0}do anything here{1} that you would normally do in any other tab", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Selected tab
        data = new HelpData();
        data.tag = "metaGameUI_13";
        data.header = "Selected Tab";
        data.text = string.Format("Shows all options that you have {0}currently selected{1} (the {2}checkmark icon{3} indicates \'selected\')", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Selected tab -> What can I do?
        data = new HelpData();
        data.tag = "metaGameUI_14";
        data.header = string.Format("{0}What can I do here?{1}", colourTip, colourEnd);
        data.text = string.Format("You can {0}do anything here{1} that you would normally do in any other tab", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Renown
        data = new HelpData();
        data.tag = "metaGameUI_15";
        data.header = "Renown";
        data.text = string.Format("Shows the amount of Renown you have {0}available to spend{1} on options. It goes {2}up and down{3} as you make select and deselect options", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Remaining Renown
        data = new HelpData();
        data.tag = "metaGameUI_16";
        data.header = string.Format("{0}Do I have to spend it all?{1}", colourTip, colourEnd);
        data.text = string.Format("No. You can spend {0}nothing{1} (press CONFIRM to exit) if you wish. Any renown you have will {2}carry over{3} to your next assignment", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Review UI
        //
        // - - - Review UI
        //
        //Open
        data = new HelpData();
        data.tag = "review_0";
        data.header = "Overview";
        data.text = string.Format("At regular intervals your {0}performance will be assessed{1} by your peers. You may, as a result, gain a {2}Commendation{3} or a {4}Black Mark{5}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Votes
        data = new HelpData();
        data.tag = "review_1";
        data.header = "Votes";
        data.text = string.Format("Your Subordinates each gets a {0}single vote{1} as does all members of HQ with the exception of the {2}Head of HQ{3} who gets {4}two{5}. ",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Director
        data = new HelpData();
        data.tag = "review_2";
        data.header = "Head of HQ";
        data.text = string.Format("Your Head of HQ votes once for his {0}personal opinion{1} of you (Motivation) and once for his view of the {2}decisions you have taken{3}",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Close
        data = new HelpData();
        data.tag = "review_3";
        data.header = "Outcome";
        data.text = string.Format("If there is a {0}majority of votes{1} FOR (Green Tick) or AGAINST (Red Cross) and there is enough to meet the {2}minimum requirement{3} ({4} votes) you gain a Commendation or a Black Star",
            colourAlert, colourEnd, colourAlert, colourEnd, GameManager.i.campaignScript.reviewMinVotes);
        listOfHelp.Add(data);
        //Commendations
        data = new HelpData();
        data.tag = "review_4";
        data.header = "Commendations";
        data.text = string.Format("You have done good things and your efforts have been {0}rewarded{1}. Gain {2} Commendations and you will {3}Win the Campaign{4}",
            colourAlert, colourEnd, GameManager.i.campaignScript.awardsWinLose, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Black Marks
        data = new HelpData();
        data.tag = "review_5";
        data.header = "Black Marks";
        data.text = string.Format("Your performance is {0}below an acceptable standard{1} and it has been noted on your record. Gain {2} Black Marks and you will {3}Lose the Campaign{4}",
            colourAlert, colourEnd, GameManager.i.campaignScript.awardsWinLose, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Inventory UI

        #region Reserves Inventory
        //
        // - - - Reserves UI
        //
        //Open
        data = new HelpData();
        data.tag = "reserveInv_0";
        data.header = "Overview";
        data.text = string.Format("Subordinates are willing to sit in the Reserves for a while, {0}but not forever{1}. At some point they'll become {2}Unhappy{3} and will eventually take decisive action",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Options
        data = new HelpData();
        data.tag = "reserveInv_1";
        data.header = "Options";
        data.text = string.Format("You can {0}delay{1} a subordinate becoming unhappy by Reassuring or Threatening them, change your mind and {2}get rid of them{3}, or select them for {4}Active Duty{5}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Quality
        data = new HelpData();
        data.tag = "reserveInv_2";
        data.header = string.Format("{0}Recruiting{1}", colourTip, colourEnd);
        data.text = string.Format("Your {0}RECRUITER{1} will always source {2}better candidates{3} than you can. Aim to source new subordinates with your RECRUITER if possible", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Gear Inventory
        //
        // - - - Gear UI
        //
        //Capabilities
        data = new HelpData();
        data.tag = "gearInv_0";
        data.header = "Gear Capabilities";
        data.text = new StringBuilder()
                    .AppendFormat("{0}District use{1} indicates gear can be used in a district. Right Click on a district{2}", colourAlert, colourEnd, "\n")
                    .AppendLine()
                    .AppendFormat("{0}Can be Gifted{1} can be given to one of your subordinates. Right Click subordinate's portrait{2}", colourAlert, colourEnd, "\n")
                    .AppendLine()
                    .AppendFormat("{0}Personal use{1} indicates gear with a personal dimension. Right Click on your portrait{2}", colourAlert, colourEnd, "\n")
                    .AppendLine()
                    .AppendFormat("{0}AI use{1} provides a benefit when hacking the AI. Use the Left Hand side tab{2}", colourAlert, colourEnd, "\n")
                    .AppendLine()
                    .AppendFormat("{0}Movement{1} allows you to negate different levels of Connection security when moving between districts{2}", colourAlert, colourEnd, "\n")
                    .AppendLine()
                    .AppendFormat("{0}Invisibility{1} prevents detection whenever you do a district action{2}", colourAlert, colourEnd, "\n")
                    .AppendLine()
                    .AppendFormat("{0}Target Use{1} can be used on ANY target. Other gear may be usable on specific targets{2}", colourAlert, colourEnd, "\n")
                    .ToString();
        listOfHelp.Add(data);
        //Rarity
        data = new HelpData();
        data.tag = "gearInv_1";
        data.header = "Gear Rarity";
        data.text = new StringBuilder()
            .AppendFormat("Gear comes in {0}four levels{1} of rarity, in order of frequency and value{2}", colourAlert, colourEnd, "\n")
            .AppendFormat("  {0} {1}Common{2}{3}", bullet, colourAlert, colourEnd, "\n")
            .AppendFormat("  {0} {1}Rare{2}{3}", bullet, colourAlert, colourEnd, "\n")
            .AppendFormat("  {0} {1}Unique{2}{3}", bullet, colourAlert, colourEnd, "\n")
            .AppendFormat("  {0} {1}Special{2} Available only from HQ between missions{3}", bullet, colourAlert, colourEnd, "\n")
            .ToString();
        listOfHelp.Add(data);
        //Gifting
        data = new HelpData();
        data.tag = "gearInv_2";
        data.header = "Gifting Gear to Subordinates";
        data.text = string.Format("Will give your subordinate a {0}Motivation{1} boost. You can {2}ask for the gear back{3} after {4}{5} turns{6}. Would you be happy about returning a gift?",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, GameManager.i.gearScript.actorGearGracePeriod, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Capture Tool Inventory
        //
        // - - - Capture Tools (Devices) Ui
        //
        //Overview
        data = new HelpData();
        data.tag = "deviceInv_0";
        data.header = "Overview";
        data.text = string.Format("Devices used to counter {0}Interrogations{1} when {2}captured{3}. Each can be used for a particular {4}Innocence{5} Level and are {6}separate{7} to any {8}Gear{9}" +
            " that you have", colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Details
        data = new HelpData();
        data.tag = "deviceInv_1";
        data.header = "Details";
        data.text = string.Format("Devices are {0}single use{1} items and {2}don't carry over{3} between cities. " +
            "They are only available from your {4}HQ superiors{5} during a transition and are dependant on their having a good opinion of you ({6}Motivation){7}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Limits
        data = new HelpData();
        data.tag = "deviceInv_2";
        data.header = "Limits";
        data.text = string.Format("There are {0}four{1} possible devices (one for each Innocence Level) and you {2}can have all four{3} at once in your possession. " +
            "Devices have {4}no uses other than{5} helping you when you've been captured",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Tip
        data = new HelpData();
        data.tag = "deviceInv_3";
        data.header = string.Format("{0}Game Tip{1}", colourTip, colourEnd);
        data.text = string.Format("These are {0}'Get Out Of Jail for Free'{1} devices. Don't leave home without them", colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #endregion

        #endregion

        #region ...Topics

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
        data.text = string.Format("Options may be greyed out indicating that they {0}aren't available{1}. Their {2}tooltip{3} will show why", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //more details
        data = new HelpData();
        data.tag = "topicUI_2";
        data.header = "More Information";
        data.text = string.Format("{0}Tooltip, image, top left{1} for more details", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //HQ Boss
        data = new HelpData();
        data.tag = "topicUI_3";
        data.header = "HQ Boss (if present)";
        builder = new StringBuilder();
        builder.AppendFormat("Your Boss has an opinion ({0}tooltip, image, top right{1}) on each option. It is your decision what to do but your {2}Boss remembers{3} your choices. ",
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
        data.text = string.Format("This item records the outcome of the decision that you have decided upon at the {0}start of this turn{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region TopicSubType...
        //
        // - - - TopicSubType help (for optional 2nd help icon on topicUI that covers subType details for the topic)
        //

        //ActorPolitic -> Overview
        data = new HelpData();
        data.tag = "topicSub_0";
        data.header = string.Format("{0}Subordinate Politics{1}", colourTip, colourEnd);
        data.text = string.Format("\'Pull the string, and it will follow wherever you wish. Push it, and it will go nowhere at all\' - {0}Dwight D. Eisenhower{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //ActorPolitic -> Good/Bad
        data = new HelpData();
        data.tag = "topicSub_1";
        data.header = "Good and Bad";
        data.text = string.Format("The good and bad versions of each topic depend on the {0}Player's Mood{1}. You're the leader and you set the tone", colourAlert, colourEnd);
        listOfHelp.Add(data);

        //ActorMatch -> Overview
        data = new HelpData();
        data.tag = "topicSub_2";
        data.header = string.Format("{0}Subordinate Interactions{1}", colourTip, colourEnd);
        data.text = string.Format("\'If you judge people you have no time to love them\' - {0}Mother Teresa{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //ActorMatch -> Good/Bad
        data = new HelpData();
        data.tag = "topicSub_3";
        data.header = "Good and Bad";
        data.text = string.Format("The good and bad versions of each topic depend on the {0}Player's Mood{1}. You're in charge and you set the example", colourAlert, colourEnd);
        listOfHelp.Add(data);

        //ActorGear -> Overview
        data = new HelpData();
        data.tag = "topicSub_24";
        data.header = string.Format("{0}Subordinate Gear{1}", colourTip, colourEnd);
        data.text = string.Format("\'The things you own end up owning you\' - {0}Chuck Palahnuik, <i>Fight Club</i>{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //ActorGear -> Good/Bad
        data = new HelpData();
        data.tag = "topicSub_25";
        data.header = "Good and Bad";
        data.text = string.Format("The good and bad versions of each topic depend on your {0}Subordinates Motivation{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);

        //ActorDistrict -> Overview
        data = new HelpData();
        data.tag = "topicSub_4";
        data.header = string.Format("{0}Subordinate Actions{1}", colourTip, colourEnd);
        data.text = string.Format("\'Every actionhas an equal and opposite reaction\' - {0}Sir Isaac Newton{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //ActorDistrict -> Good/Bad
        data = new HelpData();
        data.tag = "topicSub_5";
        data.header = "Good and Bad";
        data.text = string.Format("The good and bad versions of each topic depend on your {0}Subordinates Motivation{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //ActorDistrict -> Tracking
        data = new HelpData();
        data.tag = "topicSub_6";
        data.header = "Tracking";
        data.text = string.Format("All district actions taken by your Subordinates are tracked. Their {0}most recent actions{1} are the ones most likely to cause a ripple (or splash)", colourAlert, colourEnd);
        listOfHelp.Add(data);

        //ActorContact -> Overview
        data = new HelpData();
        data.tag = "topicSub_7";
        data.header = string.Format("{0}Subordinate Contacts{1}", colourTip, colourEnd);
        data.text = string.Format("\'It occurs to me that our survival may depend upon our talking to one another\' - {0}Dan Simmons, Hyperion{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //ActorContact -> Good/Bad
        data = new HelpData();
        data.tag = "topicSub_8";
        data.header = "Good and Bad";
        data.text = string.Format("The good and bad versions of each topic depend on the {0}Subordinates Motivation{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //ActorContact -> Networks
        data = new HelpData();
        data.tag = "topicSub_9";
        data.header = "Networks";
        data.text = string.Format("Each of your Subordinates has their own {0}personal{1} network of Contacts within the city that {2}answer to them alone{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);

        //PlayerConditions -> Overview
        data = new HelpData();
        data.tag = "topicSub_10";
        data.header = string.Format("{0}Player Conditions{1}", colourTip, colourEnd);
        data.text = string.Format("\'You are never strong enough that you don't need help\' - {0}Cesar Chavez, Civil Rights Activist{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //PlayerConditions -> Good/Bad
        data = new HelpData();
        data.tag = "topicSub_11";
        data.header = "Good and Bad";
        data.text = string.Format("The good and bad versions of each topic depend on your {0}Mood{1}. How people relate to you has a lot to do with how you project yourself", colourAlert, colourEnd);
        listOfHelp.Add(data);

        //PlayerStats -> Overview
        data = new HelpData();
        data.tag = "topicSub_12";
        data.header = string.Format("{0}Player Activity{1}", colourTip, colourEnd);
        data.text = string.Format("\'Measure a man's worth by his actions alone. For the devil also promises the moon!\' - {0}Avijeet Das, Poet{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //PlayerStats -> Good/Bad
        data = new HelpData();
        data.tag = "topicSub_13";
        data.header = "Good and Bad";
        data.text = string.Format("The good and bad versions of each topic depend on your {0}Mood{1}. How people view you has a lot to do with how you project yourself", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //PlayerStats -> Tracking
        data = new HelpData();
        data.tag = "topicSub_14";
        data.header = "Your Activity";
        data.text = string.Format("Your activity is tracked in some detail. Your subordinates will {0}judge you on what you've done{1}, not what you say you've done", colourAlert, colourEnd);
        listOfHelp.Add(data);

        //PlayerGeneral -> Overview
        data = new HelpData();
        data.tag = "topicSub_15";
        data.header = string.Format("{0}Unique Actions{1}", colourTip, colourEnd);
        data.text = string.Format("\'No man will make a great leader who wants to do it all himself or get all the credit for doing it.\' - {0}Andrew Carnegie, Industrialist{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //PlayerGeneral -> Good/Bad
        data = new HelpData();
        data.tag = "topicSub_16";
        data.header = "Leadership";
        data.text = string.Format("These represent unique opportunities to exercise various degrees of leadership depending on your own {0}Mood{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);

        //PlayerDistrict -> Overview
        data = new HelpData();
        data.tag = "topicSub_17";
        data.header = string.Format("{0}Player Actions{1}", colourTip, colourEnd);
        data.text = string.Format("\'This is a world of action, and not for moping and droning in.\' - {0}Charles Dickens, Author{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //PlayerDistrict -> Tracking
        data = new HelpData();
        data.tag = "topicSub_18";
        data.header = "Your Actions";
        data.text = string.Format("Every action you take has an impact. Actions can have {0}unforseen consequences{1}, both good or bad", colourAlert, colourEnd);
        listOfHelp.Add(data);

        //HQ -> Overview
        data = new HelpData();
        data.tag = "topicSub_19";
        data.header = string.Format("{0}HQ Actions{1}", colourTip, colourEnd);
        data.text = string.Format("\'Without pride, man becomes a parasite – and there are already too many parasites.\' - {0}Carla H Krueger, Author{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //HQ -> Tracking
        data = new HelpData();
        data.tag = "topicSub_20";
        data.header = "Good and Bad";
        data.text = string.Format("Internal politics are a sad fact of life for any organisation. Your {0}HQ Approval Level{1} determines whether they are good or bad", colourAlert, colourEnd);
        listOfHelp.Add(data);

        //Organisations -> Overview
        data = new HelpData();
        data.tag = "topicSub_21";
        data.header = string.Format("{0}Underground Organisations{1}", colourTip, colourEnd);
        data.text = string.Format("Underground Organisations can, once contact has been made, provide {0}special services{1}. Note that these are {2}illegal{3} and {4}frowned up by HQ{5}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Organisations -> Reputation
        data = new HelpData();
        data.tag = "topicSub_22";
        data.header = "Reputation";
        data.text = string.Format("You have a Reputation ({0}0 to 3 stars{1}, higher the better) with an Organisation which represents their {2}willingness to help{3} you",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Organisations -> Freedom
        data = new HelpData();
        data.tag = "topicSub_23";
        data.header = "Freedom";
        data.text = string.Format("Once you use an Organisation's services, you start incurring a debt with them. Freedom ({0}0 to 3 stars{1}, higher the better) indicates have much {2}obligation (debt){3} you have",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);

        //Story Alpha -> Overview
        data = new HelpData();
        data.tag = "topicSub_26";
        data.header = string.Format("{0}Story Alpha{1}", colourTip, colourEnd);
        data.text = string.Format("\'You’re never going to kill storytelling because it’s built into the human plan. We come with it\' - {0}Margaret Atwood, Author{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Story -> Good and Bad
        data = new HelpData();
        data.tag = "topicSub_27";
        data.header = "Good and Bad";
        data.text = string.Format("This is one of {0}three{1} stories, drawn from a {2}pool{3}, that weave throughout the {4}campaign{5}. Each story has a good or bad path that is {6}randomly{7}" +
            " determined at the start", colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);

        //Story Bravo -> Overview
        data = new HelpData();
        data.tag = "topicSub_28";
        data.header = string.Format("{0}Story Bravo{1}", colourTip, colourEnd);
        data.text = string.Format("\'The future belongs to a different kind of person with a different kind of mind: artists, inventors, storytellers-creative and holistic ‘right-brain’ thinkers\' - " +
            "{0}Daniel Pink, Author{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);

        //Story Charlie -> Overview
        data = new HelpData();
        data.tag = "topicSub_29";
        data.header = string.Format("{0}Story Charlie{1}", colourTip, colourEnd);
        data.text = string.Format("\'Sometimes reality is too complex. Stories give it form\' - {0}Jean Luc Goodard, Film Director{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);

        //Story -> story Stars
        data = new HelpData();
        data.tag = "topicSub_30";
        data.header = "Story Stars";
        data.text = string.Format("Each Story gives you the opportunity to earn a {0}Story Star{1} by completing a Story related {2}Target{3} in every {4}City{5} you visit. " +
            "There is a total of {6}5 Stars{7} that can be earned" +
            " per story over the duration of the {8}campaign{9}", colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #endregion

        #region ...Actors

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

        #region Actor Conflicts
        //
        // - - - Actor Conflict
        //
        //Overview
        data = new HelpData();
        data.tag = "conflict_0";
        data.header = "Conflict";
        builder = new StringBuilder();
        builder.AppendFormat("Anytime one of your subordinates {0}Motivation{1} falls {2}below Zero{3}, for any reason, they instigate a relationship conflict with you. ", colourAlert, colourEnd,
            colourAlert, colourEnd);
        builder.AppendFormat("Conflicts can have a {0}range of outcomes{1} such as your subordinate resigning, blackmailing you, becoming stressed, leaking against you or simply simmering away and doing nothing",
            colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Traits
        data = new HelpData();
        data.tag = "conflict_1";
        data.header = "Traits";
        data.text = string.Format("Certain character traits can {0}add extra possibilities{1} to the pool of potential actions that a subordinate may take in a relationship conflict", colourAlert, colourEnd,
            colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Tip
        data = new HelpData();
        data.tag = "conflict_2";
        data.header = string.Format("{0}Game Tip{1}", colourTip, colourEnd);
        data.text = string.Format("Relationship conflicts can be {0}unpredictable{1} and are {2}best avoided{3} wherever possible.", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #endregion

        #region ...Conditions

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

        #region Tagged Condition
        //
        // - - - Tagged Condition
        //
        //Overview
        data = new HelpData();
        data.tag = "tagged_0";
        data.header = "Tagged Condition";
        data.text = string.Format("You have been microchipped subdermally with a powerful electronic {0}TRACKER{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Effect
        data = new HelpData();
        data.tag = "tagged_1";
        data.header = "Effect on Player";
        data.text = string.Format("Every time you {0}Personally take an Action{1} your {2}Invisibility{3} will drop to {4}Zero{5} and the Authority will know your location", colourAlert, colourEnd,
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Tip
        data = new HelpData();
        data.tag = "tagged_2";
        data.header = string.Format("{0}Game Tip{1}", colourTip, colourEnd);
        data.text = string.Format("Being Tagged is no small matter. To avoid capture {0}keep a very low profile{1}, make use of your {2}Subordinates{3}, and jump at any chance of a {4}Cure{5}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
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
            colourAlert, GameManager.i.actorScript.breakdownChance, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
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
            colourAlert, GameManager.i.globalScript.tagGlobalDrug, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Breakdowns
        data = new HelpData();
        data.tag = "addict_1";
        data.header = "Feeding your Habit";
        data.text = string.Format("If you fail your Addiction Check you will need to spend {0}{1} Renown{2} to obtain enough {3}{4}{5} to feed your Addiction. ",
            colourAlert, GameManager.i.actorScript.playerAddictedRenownCost, colourEnd, colourAlert, GameManager.i.globalScript.tagGlobalDrug, colourEnd);
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

        #region Bad Condition
        //
        // - - - Bad Condition
        //
        //Overview
        data = new HelpData();
        data.tag = "bad_0";
        data.header = "Poor Show";
        data.text = string.Format("There are {0}certain conditions{1} you can have that your {2}Subordinates{3} may struggle with and they {4}may resign{5} in protest at the poor example you are setting",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Overview
        data = new HelpData();
        data.tag = "bad_1";
        data.header = "Bad Conditions";
        data.text = string.Format("Conditions that will upset your Subordinates are {0}CORRUPT{1}, {2}QUESTIONABLE{3} and {4}INCOMPETENT{5}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #endregion

        #region ...Stress

        #region Immunity to Stress
        //
        // - - - Immune to Stress
        //
        //Overview
        data = new HelpData();
        data.tag = "immune_0";
        data.header = "Stress Immunity";
        data.text = string.Format("You've taken a dose of {0}{1}{2} and have developed an immunity from stress for a set number of days",
            colourAlert, GameManager.i.globalScript.tagGlobalDrug, colourEnd);
        listOfHelp.Add(data);
        //Decreasing effect
        data = new HelpData();
        data.tag = "immune_1";
        data.header = "Diminishing Returns";
        int immuneMin = GameManager.i.actorScript.playerAddictedImmuneMin;
        data.text = string.Format("Every time you take a dose of {0}{1}{2} your period of immunity {3}decreases by one day{4} down to a minimum of {5}{6}{7} day{8}", colourAlert,
            GameManager.i.globalScript.tagGlobalDrug, colourEnd, colourAlert, colourEnd, colourAlert, immuneMin, colourEnd, immuneMin != 1 ? "s" : "");
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
            colourAlert, colourEnd, GameManager.i.actorScript.maxStatValue, colourAlert, colourEnd, colourAlert,
            GameManager.i.actorScript.stressLeaveHQApproval == true ? "Given" : "Denied", colourEnd);
        listOfHelp.Add(data);
        //Breakdowns
        data = new HelpData();
        data.tag = "stressLeave_1";
        data.header = "Taking Stress Leave";
        data.text = string.Format("It's {0}quicker{1} (one turn) than Lying Low but costs {2}{3}{4} Renown. You are {5}Safe{6} while taking Stress Leave",
            colourAlert, colourEnd, colourAlert,
            GameManager.i.sideScript.PlayerSide.level == 1 ? GameManager.i.actorScript.stressLeaveRenownCostAuthority : GameManager.i.actorScript.stressLeaveRenownCostResistance,
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

        #endregion

        #region ...Organisations

        #region OrgInfo
        //
        // - - - OrgInfo
        //
        //Overview
        data = new HelpData();
        data.tag = "orgInfo_0";
        Organisation org = GameManager.i.campaignScript.campaign.orgInfo;
        if (org != null)
        {
            data.header = org.tag;
            data.text = string.Format("The {0}{1}{2} will track your chosen target for {3}{4} days{5}. They will automatically find the target wherever they are {6}regardless of their stealth{7}", colourAlert,
                org.tag, colourEnd, colourAlert, GameManager.i.orgScript.timerOrgInfoMax, colourEnd, colourAlert, colourEnd);
        }
        else
        {
            data.header = "Overview";
            data.text = string.Format("The Organisation will track your chosen target for {0}{1} days{2}. They will automatically find the target wherever they are {3}(ignores Stealth Rating){4}", colourAlert,
                GameManager.i.orgScript.timerOrgInfoMax, colourEnd, colourAlert, colourEnd);
        }
        listOfHelp.Add(data);
        //Limitations
        data = new HelpData();
        data.tag = "orgInfo_1";
        data.header = "Limitations";
        data.text = string.Format("Only {0}one target type{1} can be tracked at a time", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Other Sighting Reports
        data = new HelpData();
        data.tag = "orgInfo_2";
        data.header = "Other Sighting Reports";
        data.text = string.Format("Any {0}Contact or Tracer{1} sighting reports of the same target type are {2}suppressed{3} (they're redundant) while you are receiving a {4}Direct Feed{5} from the Organisation",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Limitations
        data = new HelpData();
        data.tag = "orgInfo_3";
        data.header = "Invisibile";
        data.text = string.Format("The target Npc is in a district where the {0}Player previously made themselves known{1}. They {2}can't be interdicted{3} here", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Organisations
        //
        // - - - Organisations
        //
        //Overview
        data = new HelpData();
        data.tag = "org_0";
        data.header = "Underground Organisations";
        data.text = string.Format("Underground Organisations can, once contact has been made, provide {0}special services{1}. Note that these are {2}illegal{3} and {4}frowned up by HQ{5}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Duration
        data = new HelpData();
        data.tag = "org_1";
        data.header = "Organisation Contact Duration";
        data.text = string.Format("Once contact has been made a representative of the Organisation will be on hand to provide their specific services {0}in any subsequent city{1} that you visit",
            colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Break Off Contact
        data = new HelpData();
        data.tag = "org_2";
        data.header = "Break Off Contact";
        data.text = string.Format("Once an Organisation breaks off contact with you this is {0}permanent for the remainder of the campaign{1}. They will no longer have anything to do with you",
            colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #endregion

        #region ...HQ

        #region HQ Overview
        //
        // - - - HQ Overview
        //
        //Overview
        data = new HelpData();
        data.tag = "hq_over_0";
        data.header = "Overview";
        data.text = string.Format("You report to your HQ. They provide you with support and services. If you {0}displease{1} your HQ you may get {2}FIRED{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Opinion
        data = new HelpData();
        data.tag = "hq_over_1";
        data.header = "HQ's Opinion of You";
        data.text = string.Format("The level of {0}HQ Approval{1} (top centre) reflects your standing with your HQ as a whole. Each HQ member has a personal opinion of you according to their {2}Motivation{3}",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Hierarchy
        data = new HelpData();
        data.tag = "hq_over_2";
        data.header = "HQ Hierarcy";
        data.text = string.Format("HQ has a {0}four member{1} hierarchy with the {2}seniority{3} running from {4}LEFT{5} (most senior) to {6}RIGHT{7} (least senior)",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Boss
        data = new HelpData();
        data.tag = "hq_over_3";
        data.header = "HQ Boss";
        data.text = new StringBuilder()
            .AppendFormat("The Boss (Director) has a {0}personal opinion{1} (Motivation) but they also have a view on the {2}Decisions{3} you have taken. ", colourAlert, colourEnd, colourAlert, colourEnd)
            .AppendFormat("When it comes time for a {0}REVIEW{1} the Boss gets {2}TWO{3} votes, one for each.", colourAlert, colourEnd, colourAlert, colourEnd)
            .ToString();
        listOfHelp.Add(data);
        #endregion

        #region HQ Services
        //
        // - - - HQ Services (Relocating)
        //
        //Overview
        data = new HelpData();
        data.tag = "hq_0";
        data.header = "HQ Relocation";
        data.text = string.Format("Relocation is {0}extremely disruptive{1} and involved. Nobody has time to provide the services that you expect {2}until things settle down{3}",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Services Unavailable
        data = new HelpData();
        data.tag = "hq_1";
        data.header = "HQ Services";
        data.text = new StringBuilder()
            .AppendFormat("The following services will be {0}unavailable{1} during Relocation{2}", colourAlert, colourEnd, "\n")
            .AppendFormat("  {0} {1}HQ Support{2}{3}", bullet, colourAlert, colourEnd, "\n")
            .AppendFormat("  {0} {1}Lying Low{2}{3}", bullet, colourAlert, colourEnd, "\n")
            .AppendFormat("  {0} {1}Stress Leave{2}{3}", bullet, colourAlert, colourEnd, "\n")
            .AppendFormat("  {0} {1}Obtaining Gear{2}{3}", bullet, colourAlert, colourEnd, "\n")
            .AppendFormat("  {0} {1}Obtaining Recruits{2}{3}", bullet, colourAlert, colourEnd, "\n")
            .ToString();
        listOfHelp.Add(data);
        //Time to Relocate
        data = new HelpData();
        data.tag = "hq_2";
        data.header = "Time to Relocate";
        data.text = string.Format("The number of days (turns) required to relocate {0}increases with each successive relocation{1}. This carries over between Cities", colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region HQ Support
        //
        // - - - HQ Support
        //
        //overview
        data = new HelpData();
        data.tag = "hq_supp_0";
        data.header = "HQ Support";
        builder = new StringBuilder();
        builder.AppendFormat("At the start of each turn your HQ decides whether to provide you with assistance ({0}+1 Renown{1}) or not. ", colourAlert, colourEnd);
        builder.AppendFormat("The chance of it doing so depends on your level of {0}HQ Approval{1}. ", colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //chance
        data = new HelpData();
        data.tag = "hq_supp_1";
        data.header = "Chance of Support";
        builder = new StringBuilder();
        builder.AppendFormat("The better your {0}relationship{1} with your HQ, the greater the chance of support. ", colourAlert, colourEnd);
        builder.AppendFormat("Support is given if a ten sided die (1d10) is {0}less than{1} your level of HQ Approval (top centre)", colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //tip
        data = new HelpData();
        data.tag = "hq_supp_2";
        data.header = string.Format("{0}Game Tip{1}", colourTip, colourEnd);
        builder = new StringBuilder();
        builder.AppendFormat("Renown is the {0}currency{1} of the game. The more you have the more you can do. ", colourAlert, colourEnd);
        builder.AppendFormat("Your main source of Renown is from the support of your HQ. Aim to keep a {0}positive relationship{1} with them where ever possible.", colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        #endregion

        #endregion

        #region Random...

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
        //HQ support
        data = new HelpData();
        data.tag = "rand_1";
        int approval = 5;
        data.header = string.Format("{0}HQ Support{1}", colourTip, colourEnd);
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
        data.text = string.Format("You're ADDICTED. At random intervals ({0}{1}% chance per turn{2}) you will need to {3}feed your addiction{4}", colourAlert,
            GameManager.i.actorScript.playerAddictedChance, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Take Drugs check
        data = new HelpData();
        data.tag = "rand_3";
        data.header = string.Format("{0}Addicted Check{1}", colourTip, colourEnd);
        data.text = string.Format("Whenever you take {0}illegal drugs{1} there is a chance that you become {2}ADDICTED{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Investigation check
        data = new HelpData();
        data.tag = "rand_4";
        data.header = string.Format("{0}Investigation Check{1}", colourTip, colourEnd);
        data.text = string.Format("Whenever a {0}Secret{1} is revealed there is a {2}{3} %{4} chance of an Investigation being launched into your conduct", colourAlert, colourEnd,
            colourAlert, GameManager.i.playerScript.chanceInvestigation, colourEnd);
        listOfHelp.Add(data);
        //New Evidence
        data = new HelpData();
        data.tag = "rand_5";
        data.header = string.Format("{0}Evidence Type{1}", colourTip, colourEnd);
        data.text = string.Format("Each turn there is a chance that the Lead Investigator will {0}uncover new evidence{1}. The type of evidence will have a {2}higher chance{3} of being in your {4}favour{5} if the Lead has a high {6}Motivation{7}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Relations Motivational shifts
        data = new HelpData();
        data.tag = "rand_6";
        data.header = string.Format("{0}Relationships{1}", colourTip, colourEnd);
        data.text = string.Format("Whenever a {0}Friend or Enemy{1} relationship exists, any {2}change in Motivation{3} in one subordinate can affect the motivation of the {4}other subordinate{5}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //MegaCorp Spotting
        data = new HelpData();
        data.tag = "rand_7";
        data.header = string.Format("{0}MegaCorp Targets{1}", colourTip, colourEnd);
        data.text = string.Format("Whenever you {0}attempt{1} a MegaCorp target and {2}Fail{3} there is a {4}{5} %{6} chance that you'll be {7}spotted{8} and that your " +
            "{9}relations{10} with the MegaCorp will {11}deteroriate{12}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, GameManager.i.effectScript.chanceOfSpottedByMegaCorp, 
            colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #endregion

        #region MetaOptions...

        #region Special Gear
        //
        // - - - Special Gear - - -
        //
        //overview
        data = new HelpData();
        data.tag = "metaGear_0";
        data.header = "Special Gear";
        data.text = string.Format("Each HQ member has access to a particular piece of special gear {0}unavailable anywhere else{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //motivation
        data = new HelpData();
        data.tag = "metaGear_1";
        data.header = "Good Opinion";
        data.text = string.Format("They will only {0}offer the gear{1} to you if their opinion of you is positive ({2}Motivation 2+ stars{3}). They don't offer their gear to just anyone. " +
            "The {4}cost depends{5} on how good their {6}opinion{7} of you is", colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Max options
        data = new HelpData();
        data.tag = "metaGear_2";
        data.header = "Max Options";
        data.text = string.Format("There is an {0}limit{1} to how many gear options will be presented to you ({2}{3} options{4}). Eligible HQ personnel (Motivation 2+) present options in {5}order of seniority{6}" +
            " (eg. Director first) until the limit is reached", colourAlert, colourEnd, colourAlert, GameManager.i.metaScript.maxNumOfGear, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //tip
        data = new HelpData();
        data.tag = "metaGear_3";
        data.header = string.Format("{0}Game Tip{1}", colourTip, colourEnd);
        data.text = string.Format("Relationships matter. Special Gear, if it's on offer, is {0}gear that you want{1}. Think twice before passing on the opportunity.", colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Capture Tools
        //
        // - - - Capture Tools (Devices)
        //
        //Relationship affected option
        data = new HelpData();
        data.tag = "metaDevice_0";
        data.header = "Good Relationship";
        data.text = string.Format("This device is {0}only available{1} because your {2}HQ Superior{3} (current tab) has a good opinion of you ({4}Movtivation 2+ stars{5})",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Cost of Option (relationship dependant)
        data = new HelpData();
        data.tag = "metaDevice_1";
        data.header = "Variable Cost";
        data.text = string.Format("The {0}cost{1} of this option {2}varies{3} dependant on your superior's current {4}opinion{5} of you. The better their opinion ({6}Motivation{7}) " +
            "the less Renown it will cost", colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Poor Relationship
        data = new HelpData();
        data.tag = "metaDevice_2";
        data.header = "Poor Relationship";
        data.text = string.Format("If your HQ Superior's opinion of you is poor ({0}Motivation 1, or less, stars{1}), they won't bother helping you and this {2}option won't be shown{3}",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Tip
        data = new HelpData();
        data.tag = "metaDevice_3";
        data.header = string.Format("{0}Game Tip{1}", colourTip, colourEnd);
        data.text = string.Format("{0}Relationships matter{1}. If your HQ superior views you in a positive light they will actively try to {2}help you{3} by offering {4}special{5} devices",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Active option
        //
        // - - - Active Option - - -
        //
        //overview
        data = new HelpData();
        data.tag = "metaActive_0";
        data.header = "Active Option";
        data.text = string.Format("This is a live option that you can {0}select{1} if you have {2}enough renown{3} available", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //limit
        data = new HelpData();
        data.tag = "metaActive_1";
        data.header = "How many can I select?";
        data.text = string.Format("Up to {0}{1} options{2}, in total, from any tab", colourAlert, GameManager.i.metaScript.numOfChoices, colourEnd);
        listOfHelp.Add(data);
        //Select tab
        data = new HelpData();
        data.tag = "metaActive_2";
        data.header = "How can I tell if I've selected an Option?";
        data.text = string.Format("A selected option has a {0}tickmark icon{1} and is displayed in the top {2}Selected tab{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Inactive option
        //
        // - - - InActive Option - - -
        //
        //overview
        data = new HelpData();
        data.tag = "metaInactive_0";
        data.header = "inActive Option";
        data.text = string.Format("For a variety of reasons this option {0}isn't available{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //why?
        data = new HelpData();
        data.tag = "metaInactive_1";
        data.header = "Why?";
        data.text = string.Format("Options can have {0}criteria{1} that need to be met or it could be that you {2}don't have enough Renown{3} to pay for it", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Are all inActive options shown?
        data = new HelpData();
        data.tag = "metaInactive_2";
        data.header = "Are all inActive options shown?";
        data.text = string.Format("{0}No{1}, only certain ones. If your HQ member doesn't like you, for instance, their special gear option won't be shown", colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

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

        #region Captured...

        #region Player Captured
        //
        // - - - Player Captured
        //
        //Overview
        data = new HelpData();
        data.tag = "capture_0";
        data.header = "Overview";
        data.text = string.Format("You can be captured by an {0}Erasure Team{1} that is in your {2}district{3} whenever you have {4}Zero Invisibility{5}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //All Points Bulletin
        data = new HelpData();
        data.tag = "capture_1";
        data.header = "All Points Bulletin";
        data.text = string.Format("If this Authority {0}security measure{1} is in force (see top Centre, if flashing red, check tooltip), you can be {2}captured{3} with {4}1 Star Invisibility{5}. " +
            "They are actively looking for you, beware! ",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Moving
        data = new HelpData();
        data.tag = "capture_2";
        data.header = string.Format("{0}Can I be Captured while Moving?{1}", colourTip, colourEnd);
        data.text = string.Format("No, you can only be detected while moving, but not Captured. However, when you {0}arrive{1} at your {2}destination{3} district and there is an Erasure Team waiting, " +
            "you may be captured if your {4}Invisibility is Zero{5}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Invisibility Gear
        data = new HelpData();
        data.tag = "capture_3";
        data.header = string.Format("{0}Will Invisibility Gear save me from Capture?{1}", colourTip, colourEnd);
        data.text = string.Format("It's purpose is to {0}prevent you from being detected{1} and having your Invisibility fall to Zero. Once your Invisibility reaches {2}Zero{3} you are {4}vulnerable{5} to " +
            "capture and the {6}gear can't help you{7}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

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
        //Revealed
        data = new HelpData();
        data.tag = "secret_4";
        data.header = "Secret Revealed";
        data.text = string.Format("Every time one of your secrets is revealed there is a {0}{1} %{2} chance that your HQ will launch an {3}Investigation{4} into your conduct",
            colourAlert, GameManager.i.playerScript.chanceInvestigation, colourEnd, colourAlert, colourEnd);
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
        data.text = string.Format("Resistance only. You, or your subordinate, must have Invisibility {0}less than{1} the Max ({2}{3} stars{4}) and HQ must have sourced a {5}Safe House{6}",
            colourAlert, colourEnd, colourAlert, GameManager.i.actorScript.maxStatValue, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Breakdowns
        data = new HelpData();
        data.tag = "lielow_1";
        data.header = "Lie Low Timer";
        data.text = string.Format("Lying Low involves Resistance HQ sourcing a suitable, safe, location. This takes time and effort. The {0}Effects Tab{1} in the {2}App{3} lets you know when Lying Low will be next available",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Surveillance Crackdown
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
            GameManager.i.actorScript.maxStatValue, colourEnd, colourAlert, GameManager.i.playerScript.moodReset, colourEnd); ;
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
        data.text = string.Format("Your mood moves between 0 and {0}. If it drops {1}below Zero{2} you become {3}STRESSED{4}", GameManager.i.playerScript.moodMax, colourAlert, colourEnd,
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
        data.text = string.Format("Special Characters arrive in the city and conduct their business before departing. They {0}aren't visible{1} but can be spotted by {2}Contacts or Tracers{3}.",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Interaction
        data = new HelpData();
        data.tag = "npc_1";
        data.header = "Interacting";
        data.text = string.Format("You {0}automatically{1} find and interact with a Special Character if you {2}end your turn{3} in the {4}same district{5}", colourAlert, colourEnd, colourAlert, colourEnd,
            colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Movement
        data = new HelpData();
        data.tag = "npc_2";
        data.header = "Movement";
        data.text = string.Format("Special Characters can move {0}ONE{1} district a turn, at most", colourAlert, colourEnd);
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
        data.text = string.Format("HQ will task you with interacting with Special Characters. Doing so {0}before they depart{1} generally grants a benefit and failing to do so incurs a penalty",
            colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Tracers
        data = new HelpData();
        data.tag = "npc_5";
        data.header = "Tracers";
        data.text = string.Format("Tracers will {0}automatically spot{1} a Special Character in the {2}same district{3} regardless of their Stealth Rating", colourAlert, colourEnd, colourAlert, colourEnd);
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
        data.text = string.Format("Contacts can become {0}known{1} to the Authority (if a {2}Probe Team{3} is in the same district) and can, on occasion, be {4}Erased{5} (Erasure Team in same district)",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Target Rumours
        data = new HelpData();
        data.tag = "contact_3";
        data.header = "Target Rumours";
        builder = new StringBuilder();
        builder.AppendFormat("Contacts can learn rumours of new targets that will appear in the future. Their {0}information{1} can be relied upon as {2}accurate{3}.",
            colourAlert, colourEnd, colourAlert, colourEnd);
        builder.AppendFormat("Contacts with {0}High{1} Effectiveness are {2}more likely{3} to hear rumours than those with low Effectiveness", colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //New Contact
        data = new HelpData();
        data.tag = "contact_4";
        data.header = "New Contact";
        data.text = string.Format("Your subordinate will now be able to {0}carry out actions{1} in the new contact's {2}District{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Lost Contact
        data = new HelpData();
        data.tag = "contact_5";
        data.header = "Lost Contact";
        data.text = string.Format("Your subordinate will {0}No Longer{1} be able to carry out actions in their ex-contact's {2}District{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Nemesis
        data = new HelpData();
        data.tag = "contact_6";
        data.header = "Spotting Nemesis";
        data.text = string.Format("If a Nemesis is in the {0}same district{1} as the Contact they will be spotted if the {2}Contact's Effectiveness is >= Nemesis's Stealth rating{3}",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Teams
        data = new HelpData();
        data.tag = "contact_7";
        data.header = "Spotting Teams";
        data.text = string.Format("Any teams in the {0}same district{1} as the Contact will be {2}spotted automatically{3} (it's hard to hide a team)", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Inactive
        data = new HelpData();
        data.tag = "contact_8";
        data.header = "Gone Silent";
        data.text = string.Format("A Contact who has gone silent will {0}no longer{1} provide {2}information{3} or enable your subordinate to carry out {4}actions{5} in the Contact's district",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //How long Inactive
        data = new HelpData();
        data.tag = "contact_9";
        data.header = "For How Long?";
        data.text = string.Format("You can check the {0}Effects Tab{1} in the InfoApp {2}next turn{3} to see when your Contact will {4}return{5}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Return to Active
        data = new HelpData();
        data.tag = "contact_10";
        data.header = "Back on the Grid";
        data.text = string.Format("A Contact who has returned will have their ears to the ground for {0}information{1} and will allow your subordinate to carry out {2}actions{3} in the Contact's district",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Investigations
        //
        // - - - Investigations
        //
        //Launched
        data = new HelpData();
        data.tag = "invest_0";
        data.header = "Launched";
        data.text = string.Format("Whenever a {0}Secret{1} is revealed there is a {2}{3} %{4} chance of an Investigation being launched into your conduct", colourAlert, colourEnd,
            colourAlert, GameManager.i.playerScript.chanceInvestigation, colourEnd);
        listOfHelp.Add(data);
        //Process
        data = new HelpData();
        data.tag = "invest_1";
        data.header = "Process";
        data.text = string.Format("Investigations continue until their is sufficient evidence to Incriminate you ({0}< 0 stars, found Guilty{1}) or be Exonerated ({2}> 3 stars, found Innocent{3})",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Evidence
        data = new HelpData();
        data.tag = "invest_2";
        data.header = "Evidence";
        data.text = string.Format("New evidence will come to light as time progresses through {0}events{1} or as a result of the work of the {2}Lead Investigator{3} (Good evidence more likely if they have a high {4}Motivation{5}, and vice versa)",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Outcome
        data = new HelpData();
        data.tag = "invest_3";
        data.header = "Resolution";
        data.text = string.Format("If an investigation concludes that you are {0}Guilty{1} you will be immediately fired once the investigation is Resolved. {2}LEVEL OVER{3} and you gain {4}Black Marks{5}. ",
            colourAlert, colourEnd, colourTip, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Lead Investigator
        data = new HelpData();
        data.tag = "invest_4";
        data.header = "Lead Investigator";
        data.text = new StringBuilder()
        .AppendFormat("There is a {0}{1} %{2} chance of them finding new evidence {3}each turn{4}. The type of evidence depends on the Motivation of the Lead{5}",
            colourAlert, GameManager.i.playerScript.chanceEvidence, colourEnd, colourAlert, colourEnd, "\n")
        .AppendFormat("  {0} Motivation 3, {1}Good 80%{2}, Bad 20%{3}", bullet, colourAlert, colourEnd, "\n")
        .AppendFormat("  {0} Motivation 2, {1}Good 60%{2}, Bad 40%{3}", bullet, colourAlert, colourEnd, "\n")
        .AppendFormat("  {0} Motivation 1, Good 40%, {1}Bad 60%{2}{3}", bullet, colourAlert, colourEnd, "\n")
        .AppendFormat("  {0} Motivation 0, Good 20%, {1}Bad 80%{2}", bullet, colourAlert, colourEnd)
        .ToString();
        listOfHelp.Add(data);
        //Evidence Details
        data = new HelpData();
        data.tag = "invest_5";
        data.header = "Evidence";
        data.text = string.Format("Evidence is assessed at between {0}0{1} (Bad, {2}Guilty{3}) and {4}3{5} (Good, {6}Innocent{7}) stars",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Verdict
        data = new HelpData();
        data.tag = "invest_6";
        data.header = "Verdict";
        data.text = string.Format("Once a verdict has been reached the investigation will {0}cease{1} taking into account any {2}new evidence{3} and in {4}{5}{6} turns there will be a formal resolution",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, GameManager.i.playerScript.timerInvestigationBase, colourEnd);
        listOfHelp.Add(data);
        //Politics
        data = new HelpData();
        data.tag = "invest_7";
        data.header = "Politics";
        data.text = string.Format("During the countdown to the resolution evidence can't change the verdict but it's {0}possible{1} that Politics may", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Guilty
        data = new HelpData();
        data.tag = "invest_8";
        data.header = "Guilty";
        data.text = string.Format("If the investigation reaches a resolution with a {0}Guilty verdict{1} you are fired immediately and it's {2}LEVEL OVER{3}. You also gain a {4}Black Mark{5}",
            colourAlert, colourEnd, colourTip, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Innocent
        data = new HelpData();
        data.tag = "invest_9";
        data.header = "Innocent";
        data.text = string.Format("If the investigation reaches a resolution with an {0}Innocent verdict{1} you will be exonerated and gain {2}+{3} HQ Approval{4}", colourAlert, colourEnd,
            colourTip, GameManager.i.playerScript.investHQApproval, colourEnd);
        listOfHelp.Add(data);
        //Tip
        data = new HelpData();
        data.tag = "invest_10";
        data.header = string.Format("{0}Investigations{1})", colourTip, colourEnd);
        data.text = string.Format("Investigations are serious matters with {0}significant consequences{1}. Avoid a Guilty verdict by any means possible", colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Relations
        //
        // - - - Relations
        //
        //Overview
        data = new HelpData();
        data.tag = "relation_0";
        data.header = "Relations Overview";
        data.text = new StringBuilder()
            .AppendFormat("Friend or Enemy relationships can exist between your {0}subordinates{1}. ", colourAlert, colourEnd)
            .AppendFormat("A subordinate can only have {0}one{1} relationship {2}at a time{3} and relationships {4}end{5} once either party {6}leaves{7}", colourAlert, colourEnd,
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd)
            .ToString();
        listOfHelp.Add(data);
        //Movitation
        data = new HelpData();
        data.tag = "relation_1";
        data.header = "Motivation";
        data.text = string.Format("If either party in a relationship experiences a {0}change in Motivation{1} then there is a {2}{3} % chance{4} of the other subordinate experiencing a change at the {5}same time{6}",
            colourAlert, colourEnd, colourAlert, GameManager.i.actorScript.chanceRelationShift, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Friends
        data = new HelpData();
        data.tag = "relation_2";
        data.header = "Friends";
        data.text = string.Format("In a Friends relationship the change in Motivation to one party is {0}identical{1} to the change in the other (they are happy when their friend is happy and vice versa)",
            colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Friends
        data = new HelpData();
        data.tag = "relation_3";
        data.header = "Enemies";
        data.text = string.Format("In an Enemies relationship the change in Motivation to one party is the {0}opposite{1} to the change in the other (they are happy when their enemy is upset and vice versa)",
            colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Targets
        //
        // - - - Targets
        //
        //Overvew
        data = new HelpData();
        data.tag = "target_0";
        data.header = "Overview";
        data.text = string.Format("Targets can be {0}attempted{1} by either the {2}Player{3} (you need to move to the {4}District{5} to do so) or by the {6}subordinate{7} mentioned in the target description, eg. Blogger",
            colourAlert, colourEnd, colourAlert,  colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Subordinate
        data = new HelpData();
        data.tag = "target_1";
        data.header = "Subordinate";
        data.text = string.Format("If you have the {0}correct subordinate{1} present in your line-up they can attempt the target at {2}any time{3}. There is no need for you to move to the District",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Target Attempts
        data = new HelpData();
        data.tag = "target_2";
        data.header = "Attempts";
        data.text = string.Format("{0}Left Click{1} on the {2}District{3} if you have the correct subordinate, or you are in the District, and mouseover {4}Attempt Target{5} to see your chances of success",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
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
        string tag0 = "hq_0";
        string tag1 = "hq_1";
        string tag2 = "relation_2";
        string tag3 = "relation_3";
        List<HelpData> listOfHelp = GetHelpData(tag0, tag1, tag2, tag3);
        Vector3 screenPos = new Vector3(Screen.width / 2, Screen.height / 2);
        GameManager.i.tooltipHelpScript.SetTooltip(listOfHelp, screenPos);
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
            .AppendFormat(" HQ Hierarchy -> H{0}", "\n")
            .AppendFormat(" Interrogation Devices -> V{0}", "\n")
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
            .AppendFormat(" Utility Nodes -> F7{0}", "\n")
            .AppendFormat(" Player Known Nodes -> F8{0}{1}", "\n", "\n")
            .AppendFormat(" Debug Show -> D{0}{1}", "\n", "\n")
            .AppendFormat(" Activity Time -> F9{0}", "\n")
            .AppendFormat(" Activity Count -> F10{0}{1}", "\n", "\n")
            .AppendFormat(" MainInfoApp display -> I{0}", "\n")
            .AppendFormat(" MainInfoApp ShowMe -> Space{0}", "\n")
            .AppendFormat(" MainInfoApp Home -> Home{0}", "\n")
            .AppendFormat(" MainInfoApp End -> End{0}", "\n")
            .AppendFormat(" MainInfoApp Back -> PageDn{0}", "\n")
            .AppendFormat(" MainInfoApp Forward -> PageUp{0}{1}", "\n", "\n")
            .AppendFormat("-Global Options (Modal/non-Modal){0}", "\n")
            .AppendFormat(" Actor Info toggle -> 1{0}", "\n")
            .AppendFormat(" Test Condition -> Z{0}", "\n")
            .ToString();
    }

}
