using gameAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// handles all Help related matters
/// </summary>
public class HelpManager : MonoBehaviour
{
    //bullet character for help topics
    private char bullet;
    private string arrowLeft;
    private string arrowRight;

    //fast access
    private ActorArc arcFixer;
    private ActorArc arcPlanner;
    private ActorArc arcHacker;

    private GlobalSide sideResistance;

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
        //fast access -> characters
        bullet = GameManager.i.guiScript.bulletChar;
        arrowLeft = GameManager.i.guiScript.arrowIconLeft;
        arrowRight = GameManager.i.guiScript.arrowIconRight;
        Debug.Assert(string.IsNullOrEmpty(arrowLeft) == false, "Invalid arrowLeft (Null or Empty)");
        Debug.Assert(string.IsNullOrEmpty(arrowRight) == false, "Invalid arrowRight (Null or Empty)");
        //fast access -> actorArcs
        arcFixer = GameManager.i.dataScript.GetActorArc("FIXER");
        arcPlanner = GameManager.i.dataScript.GetActorArc("PLANNER");
        arcHacker = GameManager.i.dataScript.GetActorArc("HACKER");
        Debug.Assert(arcFixer != null, "Invalid arcFixer (Null)");
        Debug.Assert(arcPlanner != null, "Invalid arcPlanner (Null)");
        Debug.Assert(arcHacker != null, "Invalid arcHacker (Null)");
        //fast access -> other
        sideResistance = GameManager.i.globalScript.sideResistance;
        Debug.Assert(sideResistance != null, "Invalid sideResistance (Null)");
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

    #region CreateItemDataHelp...
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
        builder.AppendFormat("{0}MOUSE OVER{1} for a News Summary", colourAlert, colourEnd);
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
        data.text = string.Format("Each HQ member will grant you {0}Power{1} according to their {2}assessment{3}. Each assesses you on {4}three criteria{5}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //power
        data = new HelpData();
        data.tag = "transitionEnd_1";
        data.header = "Power";
        data.text = string.Format("Their {0}Overall{1} assessment ({2}weighted{3} average of all three criteria) determines how much Power you are given. {4}Senior{5} HQ members will can grant" +
            ", potentially, {6}more Power{7} than junior members", colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
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
            .AppendFormat("HQ Members personal view of you (<b>Opinion</b>){0}", "\n")
            .AppendFormat("{0}Targets{1}{2}", colourAlert, colourEnd, "\n")
            .AppendFormat("One star for each successfully completed Target{0}", "\n")
            .AppendFormat("{0}Exposure{1}{2}", colourAlert, colourEnd, "\n")
            .AppendFormat("Your Innocence level {0} at the end of the level{1}", GameManager.i.guiScript.innocenceIcon, "\n")
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
            "Each {6}Overall{7} star from the most senior member is worth {8}4x more Power{9} than a star from the most junior member",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Objective Overview
        data = new HelpData();
        data.tag = "transitionEnd_4";
        data.header = "Overview";
        data.text = string.Format("{0}Completing{1} Objectives is how you gain the {2}greatest Power{3}. They are the first criteria of the most senior HQ member and are, as a consequence, worth the most",
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
        data.text = string.Format("Things happen and change people's {0}Power{1} which, in turn, {2}determines{3}, who has what {4}position{5} in the hierarchy ({6}highest to lowest{7})",
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
        data.text = string.Format("These happen in the background and are {0}outside of your control{1} however you can {2}influence{3} who gains or loses Power through {4}HQ events{5}" +
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
        data.text = string.Format("Promotion is based around {0}Power{1}. The person with the most Power gets the job. The {2}hierarchy{3} are those with the highest Power, in " +
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
        data.text = string.Format("You will be given the {0}opportunity{1} by various HQ Hierarchy members to take care of your problems (it'll {2}cost you{3} Power) {4}before{5} your next mission",
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
        data.text = string.Format("You can select up to {0}{1} options{2} of any type, provided you have {3}enough Power{4} to pay for them",
            colourAlert, GameManager.i.metaScript.numOfChoices, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Carry Over Power
        data = new HelpData();
        data.tag = "metaGameUI_3";
        data.header = string.Format("{0}Do I have to?{1}", colourTip, colourEnd);
        data.text = string.Format("You don't have to select any options. Any {0}Power{1} you have will {2}carry over{3} to your {4}next assignment{5}",
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
        data.text = string.Format("It will give you a {0}blank slate{1} from which to choose options again, or allow you to exit (press CONFIRM) and {2}save your Power{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Confirm button
        data = new HelpData();
        data.tag = "metaGameUI_6";
        data.header = "Confirm Button";
        data.text = string.Format("{0}Selected{1} Options will be {2}locked in{3} and any {4}remaining Power{5} will carry over to your {6}next assignment{7}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Confirm button tip
        data = new HelpData();
        data.tag = "metaGameUI_7";
        data.header = string.Format("{0}Can I press this if I haven't selected anything?{1}", colourTip, colourEnd);
        data.text = string.Format("Yes you can. You'll exit the HQ Options and your {0}Power{1} will carry over to your {2}next assignment{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Recommended button
        data = new HelpData();
        data.tag = "metaGameUI_8";
        data.header = "Recommended Button";
        data.text = string.Format("Recommended options will be selected, within your Power allowance, and {0}all other options deselected{1}", colourAlert, colourEnd);
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
        //Power
        data = new HelpData();
        data.tag = "metaGameUI_15";
        data.header = "Power";
        data.text = string.Format("Shows the amount of Power you have {0}available to spend{1} on options. It goes {2}up and down{3} as you make select and deselect options", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Remaining Power
        data = new HelpData();
        data.tag = "metaGameUI_16";
        data.header = string.Format("{0}Do I have to spend it all?{1}", colourTip, colourEnd);
        data.text = string.Format("No. You can spend {0}nothing{1} (press CONFIRM to exit) if you wish. Any Power you have will {2}carry over{3} to your next assignment", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Tabbed UI
        //
        // - - - Tabbed UI
        //
        //overview
        data = new HelpData();
        data.tag = "tabbedUI_0";
        data.header = "Character Dossier";
        data.text = string.Format("A summary of all known {0}information{1} about important characters, including yourself. This is {2}confidential{3} and for your eyes only",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //controls
        data = new HelpData();
        data.tag = "tabbedUI_1";
        data.header = "Controls";
        data.text = new StringBuilder()
            .AppendFormat("The following controls are available{0}", "\n")
            .AppendFormat(" {0} {1}PgUp & PgDown{2}, cycles through the {3}Side Tabs{4}{5}", bullet, colourAlert, colourEnd, colourAlert, colourEnd, "\n")
            .AppendFormat(" {0} {1} {2}Left{3} & {4} {5}Right Arrows{6}, cycles {7}Top Tabs{8}{9}", bullet, arrowLeft, colourAlert, colourEnd, arrowRight,
            colourAlert, colourEnd, colourAlert, colourEnd, "\n")
            .AppendFormat(" {0} {1}Up & Down Arrows{2} to switch character {3}Groups{4}", bullet, colourAlert, colourEnd, colourAlert, colourEnd, "\n")
            .ToString();
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
        data.text = string.Format("Your Subordinates each gets a {0}single vote{1} as does all members of HQ with the exception of the {2}HQ Director{3} who gets {4}two{5}. ",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Director
        data = new HelpData();
        data.tag = "review_2";
        data.header = "HQ Director";
        data.text = string.Format("Your HQ Director votes once for his {0}Opinion{1} of you and once for his judgment of the {2}decisions you have taken{3}",
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

        #region OutcomeUI
        //
        // - - - Outcome UI
        //
        //Open
        data = new HelpData();
        data.tag = "outcome_0";
        data.header = "District Actions";
        data.text = string.Format("Press {0}any key{1} to close", colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Inventory UI

        #region Reserves Inventory
        //
        // - - - Reserves UI
        //
        //Overview
        data = new HelpData();
        data.tag = "reserveInv_0";
        data.header = "Overview";
        builder = new StringBuilder();
        builder.AppendFormat("Newly recruited subordinates are placed in the Reserves. To have them {0}join your team{1} you need to select them for Active Duty ({2}Right Click portrait{3}). ",
            colourAlert, colourEnd, colourAlert, colourEnd);
        builder.AppendFormat("Existing team members can also be sent to the Reserves to {0}make room{1} for new recruits. The Reserves act as a {2}holding pool{3}",
            colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Subordinates
        data = new HelpData();
        data.tag = "reserveInv_1";
        data.header = "Subordinates";
        data.text = string.Format("Subordinates are willing to sit in the Reserves for a while, {0}but not forever{1}. At some point they'll become {2}Unhappy{3} and will eventually take decisive action",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Options
        data = new HelpData();
        data.tag = "reserveInv_2";
        data.header = "Options";
        data.text = string.Format("You can {0}delay{1} a subordinate becoming unhappy by Reassuring or Threatening them, change your mind and {2}get rid of them{3}, or select them for {4}Active Duty{5}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Quality
        data = new HelpData();
        data.tag = "reserveInv_3";
        data.header = string.Format("{0}Recruiting{1}", colourTip, colourEnd);
        data.text = string.Format("Your {0}RECRUITER{1} will always source {2}better candidates{3} than you can. Aim to source new subordinates with your RECRUITER if possible", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Reserves
        data = new HelpData();
        data.tag = "reserveInv_4";
        data.header = string.Format("{0}Reserves{1}", colourTip, colourEnd);
        data.text = string.Format("To {0}view{1} your Reserve Pool and to make any changes, {2}press R{3} (after everything else is closed)", colourAlert, colourEnd, colourAlert, colourEnd);
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
            "They are only available from your {4}HQ superiors{5} during a transition and are dependant on their having a good {6}Opinion{7} of you",
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

        #region NewTurn UI
        //
        // - - - New Turn Ui
        //
        //Overview
        data = new HelpData();
        data.tag = "newTurn_0";
        data.header = "New Turn";
        data.text = string.Format("When you are ready, press {0}ENTER{1} for a new turn or {2}Click{3} the button", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Tutorial UI
        //
        // - - - Tutorial UI
        //
        //Overview
        data = new HelpData();
        data.tag = "tutorial_0";
        data.header = "Tutorial";
        data.text = string.Format("This control allows you to {0}move{1} between different parts of the tutorial and access the {2}Master Help{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Controls
        data = new HelpData();
        data.tag = "tutorial_1";
        data.header = "Controls";
        data.text = string.Format("Click on the {0}RIGHT{1} arrow to move to the {2}Next{3} stage of the tutorial, the {4}LEFT{5} for the {6}Previous{7}. Click on the {8}QUESTION{9} for {10}Help{11}", 
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Master Help
        data = new HelpData();
        data.tag = "tutorial_2";
        data.header = string.Format("{0}Hint{1}", colourTip, colourEnd);
        data.text = string.Format("The tutorial is flexible and allows you to bounce around if you wish but proceeding through in logical manner is recommended", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
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
        data.header = "Good or Bad Event";
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
        data.header = "Good or Bad Event";
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
        data.header = "Good or Bad Event";
        data.text = string.Format("The good and bad versions of each topic depend on your {0}Subordinates Opinion{1}", colourAlert, colourEnd);
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
        data.header = "Good or Bad Event";
        data.text = string.Format("The good and bad versions of each topic depend on your {0}Subordinates Opinion{1}", colourAlert, colourEnd);
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
        data.header = "Good or Bad Event";
        data.text = string.Format("The good and bad versions of each topic depend on the {0}Subordinates Opinion{1}", colourAlert, colourEnd);
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
        data.header = "Good or Bad Event";
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
        data.header = "Good or Bad Event";
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
        data.header = "Good or Bad Event";
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
        data.header = "Good or Bad Event";
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
        //Opinion
        data = new HelpData();
        data.tag = "compatibility_0";
        data.header = "Opinion";
        data.text = new StringBuilder()
            .AppendFormat("Indicates how {0}Enthusiastic{1} a person is. It ranges from {2}0 to 3{3} stars, the more the better. ", colourAlert, colourEnd, colourAlert, colourEnd)
            .AppendFormat("A person's Opinion effects many game mechanics and is something to {0}keep an eye on{1}. As in real life, a person who has lost all enthusiasm will be a problem",
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
            .AppendFormat("An individual who has a positive feeling towards you has a {0}chance of ignoring{1} any {2}BAD{3} Opinion outcomes. ", colourAlert, colourEnd, colourAlert, colourEnd)
            .AppendFormat("The more {0}Green stars{1} (better your relationship), the more likely this is to happen", colourAlert, colourEnd)
            .ToString();
        listOfHelp.Add(data);
        //Negative Compatibility
        data = new HelpData();
        data.tag = "compatibility_3";
        data.header = "Negative Compatibility";
        data.text = new StringBuilder()
            .AppendFormat("An individual who has a negative feeling towards you has a {0}chance of ignoring{1} any {2}GOOD{3} Opinion outcomes. ", colourAlert, colourEnd, colourAlert, colourEnd)
            .AppendFormat("The more {0}Red stars{1} (worse your relationship), the more likely this is to happen", colourAlert, colourEnd)
            .ToString();
        listOfHelp.Add(data);
        #endregion

        #region Actor Conflicts
        //
        // - - - Relationship Conflicts
        //
        //Overview
        data = new HelpData();
        data.tag = "conflict_0";
        data.header = "Relationship Conflicts";
        builder = new StringBuilder();
        builder.AppendFormat("Anytime one of your subordinates {0}Opinion{1} falls {2}below Zero{3}, for any reason, they instigate a relationship conflict with you. ", colourAlert, colourEnd,
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
        data.text = string.Format("You can ask Rebel HQ to quietly {0}Dispose Of{1} a QUESTIONABLE subordinate at {2}no cost{3} in Power if you suspect that they might be a traitor",
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

        #region Imaged Condition
        //
        // - - - Imaged Condition
        //
        //Overview
        data = new HelpData();
        data.tag = "imaged_0";
        data.header = "Imaged Condition";
        data.text = string.Format("The Authority has a copy of your facial likeness", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Effect
        data = new HelpData();
        data.tag = "imaged_1";
        data.header = "Effect";
        data.text = string.Format("There is a {0}random chance, each turn{1}, that you'll be picked up by Facial Recognition Software and suffer a {2}1 star drop in Invisibility{3}",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Stressed Condition
        //
        // - - - Stressed Condition
        //
        //Overview -> Player
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
        //Subordinate Stress
        data = new HelpData();
        data.tag = "stress_4";
        data.header = "Stressed Condition";
        data.text = string.Format("Subordinates can become stressed from a {0}variety of reasons{1}. For every turn that they are Stressed there is a {2}chance of a Breakdown{3}",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Breakdowns -> Subordinates
        data = new HelpData();
        data.tag = "stress_5";
        data.header = "Breakdowns";
        data.text = string.Format("A stress induced Nervous Breakdown {0}prevents your subordinate from doing anything that turn{1}. They are {2}vulnerable{3} and can be captured or worse",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Chance of Breakdowns -> Subordinates
        data = new HelpData();
        data.tag = "stress_6";
        data.header = "Chance of Breakdowns";
        data.text = string.Format("There is {0}{1} %{2} chance of a Breakdown each turn. Subordinates can have {3}traits{4} that modify this. If they become Stressed when they are already Stressed, the {5}odds increase{6}",
            colourAlert, GameManager.i.actorScript.breakdownChance, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Recovering from Stress -> Subordinates
        data = new HelpData();
        data.tag = "stress_7";
        data.header = "Removing Stress";
        data.text = string.Format("{0}Lying Low{1} and sending your subordinate on {2}Stress Leave{3} will remove their Stressed Condition",
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
        data.text = string.Format("If you fail your Addiction Check you will need to spend {0}{1} Power{2} to obtain enough {3}{4}{5} to feed your Addiction. ",
            colourAlert, GameManager.i.actorScript.playerAddictedPowerCost, colourEnd, colourAlert, GameManager.i.globalScript.tagGlobalDrug, colourEnd);
        listOfHelp.Add(data);
        //Chance of Breakdowns
        data = new HelpData();
        data.tag = "addict_2";
        data.header = "Insufficient Power";
        data.text = string.Format("If you don't have enough Power on hand you are assumed to suffer {0}withdrawal symptoms{1}. HQ will notice and express their disapproval ({2}Approval -1{3})",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Recovering from Addiction
        data = new HelpData();
        data.tag = "addict_3";
        data.header = "Overcoming your Addiction";
        data.text = string.Format("You'll need to find somewhere, or somebody, that offers a {0}cure{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Star Condition
        //
        // - - - Star Condition
        //
        //Overview
        data = new HelpData();
        data.tag = "star_0";
        data.header = "Star Condition";
        data.text = string.Format("Your subordinate has performed {0}above and beyond expectations{1}. They are a STAR", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Effect
        data = new HelpData();
        data.tag = "star_1";
        data.header = "Effect";
        data.text = string.Format("Your subordinate can be {0}Promoted{1} (MANAGE Menu) at {2}no cost{3}. If so they will {4}join HQ{5} (at end of the Level) as a worker and will view you in a {6}positive light{7}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Tip
        data = new HelpData();
        data.tag = "star_2";
        data.header = "Why would I want to Promote them?";
        data.text = string.Format("The more friends you have at HQ the {0}more assistance{1} they will give you over" +
            " the course of the Campaign", colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Wounded Condition
        //
        // - - - Wounded condition
        //
        //Overview
        data = new HelpData();
        data.tag = "wounded_0";
        data.header = "Wounded Condition";
        data.text = string.Format("Applies {0}only to yourself{1}, the Player. You're badly hurt", colourAlert, colourEnd);
        listOfHelp.Add(data);
        data = new HelpData();
        data.tag = "wounded_1";
        data.header = "Effect";
        data.text = string.Format("While ever you're Wounded you are {0}restricted to ONE Action per turn{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Incompetent Condition
        //
        // - - - Incompetent condition
        //
        //Overview
        data = new HelpData();
        data.tag = "incompetent_0";
        data.header = "Incompetent Condition";
        data.text = string.Format("Either yourself, or your subordinate, {0}didn't do their job{1} and have been shown to be {2}inept{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        data = new HelpData();
        //Incompetent -> Player
        data = new HelpData();
        data.tag = "incompetent_1";
        data.header = "Incompetent Player";
        data.text = string.Format("If you're Incompetent your subordinates may {0}resign in disgust{1} at your ineptitude", colourAlert, colourEnd);
        listOfHelp.Add(data);
        data = new HelpData();
        //Incompetent -> Subordinate
        data = new HelpData();
        data.tag = "incompetent_2";
        data.header = "Incompetent Subordinate";
        data.text = string.Format("Your subordinate can be {0}Disposed off{1} (MANAGE menu) at {2}no cost{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        data = new HelpData();
        #endregion

        #region Corrupt Condition
        //
        // - - - Corrupt condition
        //
        //Overview
        data = new HelpData();
        data.tag = "corrupt_0";
        data.header = "Corrupt Condition";
        data.text = string.Format("Either yourself, or your subordinate, {0}took money on the side{1} and have been shown to be {2}tainted{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        data = new HelpData();
        //Corrupt -> Player
        data = new HelpData();
        data.tag = "corrupt_1";
        data.header = "Corrupt Player";
        data.text = string.Format("If you're Corrupt your subordinates may {0}resign in disgust{1} at your crooked ways", colourAlert, colourEnd);
        listOfHelp.Add(data);
        data = new HelpData();
        //Corrupt -> Subordinate
        data = new HelpData();
        data.tag = "corrupt_2";
        data.header = "Corrupt Subordinate";
        data.text = string.Format("Your subordinate can be {0}Disposed off{1} (MANAGE menu) at {2}no cost{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        data = new HelpData();
        #endregion

        #region Doomed Condition
        //
        // - - - Doomed condition
        //
        //Overview
        data = new HelpData();
        data.tag = "doom_0";
        data.header = "Doomed Condition";
        data.text = string.Format("The news isn't good. You're {0}destined to die. Soon. The {2}virus{3} is doing it's job", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        data = new HelpData();
        //Effect
        data = new HelpData();
        data.tag = "doom_1";
        data.header = "Effect";
        data.text = string.Format("You'll die. {0}Level OVER{1} (but not your Campaign). Worm food. That's your future", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        data = new HelpData();
        //Cure
        data = new HelpData();
        data.tag = "doom_2";
        data.header = "Cure";
        data.text = string.Format("If you don't access a {0}cure{1} to the Virus before the {2}countdown expires{3}, you are toast", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        data = new HelpData();
        //Tip
        data = new HelpData();
        data.tag = "doom_3";
        data.header = string.Format("{0}Advice{1}", colourTip, colourEnd);
        data.text = string.Format("You are about to die. Find an {0}Antidote{1}. Nothing else matters", colourAlert, colourEnd);
        listOfHelp.Add(data);
        data = new HelpData();
        #endregion

        #region Blackmailer Condition
        //
        // - - - Blackmailer Condition
        //
        //Overview
        data = new HelpData();
        data.tag = "blackmail_0";
        data.header = "Blackmailer Condition";
        data.text = string.Format("Your subordinate is Blackmailing you as a result of a {0}Relationship Conflict{1}. They are threatening to {2}reveal{3} one of your {4}secrets{5}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        data = new HelpData();
        //Effect
        data = new HelpData();
        data.tag = "blackmail_1";
        data.header = "Effect";
        data.text = string.Format("There is a Blackmail {0}countdown{1}, which, if it ever gets to {2}Zero{3}, results in your subordinate carrying out their threat to {4}reveal your secret{5}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        data = new HelpData();
        //Removing Subordinate
        data = new HelpData();
        data.tag = "blackmail_2";
        data.header = "Removing your Subordinate";
        data.text = string.Format("It's possible to remove your subordinate (MANAGE menu) and {0}neutralise{1} their threat but the {2}cost{3} (Power) of doing so is {4}higher{5} than normal",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        data = new HelpData();
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
        //stress leave
        data = new HelpData();
        data.tag = "stressLeave_1";
        data.header = "Taking Stress Leave";
        data.text = string.Format("It's {0}quicker{1} (one turn) than Lying Low but costs {2}{3}{4} Power. You are {5}Safe{6} while taking Stress Leave",
            colourAlert, colourEnd, colourAlert,
            GameManager.i.sideScript.PlayerSide.level == 1 ? GameManager.i.actorScript.stressLeavePowerCostAuthority : GameManager.i.actorScript.stressLeavePowerCostResistance,
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

        #region Nervous Breakdowns
        //
        // - - - Nervous Beakdown
        //
        //Overview
        data = new HelpData();
        data.tag = "stressBreak_0";
        data.header = "Nervous Breakdowns";
        data.text = string.Format("Whenever a character, or you the Player, is stressed, there is a {0}random chance, each turn{1}, that they may suffer a Nervous Breakdown", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Effects
        data = new HelpData();
        data.tag = "stressBreak_1";
        data.header = "Effect of a Breakdown";
        data.text = string.Format("The character, or you the Player, are fully occupied dealing with their issues and are {0}unable to do anything else{1}. The breakdown lasts for {2}one turn{3}",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Stress Removal
        data = new HelpData();
        data.tag = "stressBreak_2";
        data.header = string.Format("{0}How do I get rid of Stress?{1}", colourTip, colourEnd);
        data.text = string.Format("Have yourself, or the character, either {0}Lie Low{1} or take {2}Stress Leave{3} to remove the STRESSED condition", colourAlert, colourEnd, colourAlert, colourEnd);
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
        Organisation org = GameManager.i.campaignScript.campaign.details.orgInfo;
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
        //Reputation
        data = new HelpData();
        data.tag = "org_3";
        data.header = "Reputation";
        data.text = string.Format("A measure of your {0}reputation{1} with the Organisation (more stars the better). Organisations will help you regardless of your reputation but their {2}price{3} will {4}increase{5}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Freedom
        data = new HelpData();
        data.tag = "org_4";
        data.header = "Freedom";
        data.text = string.Format("How much of an {0}obligation{1} you {2}owe{3} to the Organisation (the {4}more stars{5}, the less your debt, {6}the better{7}). " +
            "The more you ask of an Organisation the greater your debt. At {8}zero{9} Freedom {10}no more{11} assistance will be offered",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region MegaCorps
        //Overview
        data = new HelpData();
        data.tag = "megaCorp_0";
        data.header = "MegaCorporations";
        data.text = string.Format("There are a handful of {0}globe spanning{1} corporations that have risen to become {2}more powerful{3} than even the ruling Power Blocs. " +
            "Their reach extends {4}beyond Earth{5} throughout known space",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Resistance
        data = new HelpData();
        data.tag = "megaCorp_1";
        data.header = "Resistance";
        data.text = string.Format("While the MegaCorps are {0}officially{1} on the side of the {2}Authority{3} and have an intense desire to maintain the, very profitable, " +
            "status quo they are not averse to using the Resistance to handle {4}delicate{5} matters",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Reputation
        data = new HelpData();
        data.tag = "megaCorp_2";
        data.header = "Reputation";
        data.text = string.Format("Most of the time you are {0}irrelevant{1} to the MegaCorps, except when you're not. Then they take an interest. " +
            "This is reflected by your {2}Reputation{3} (0 to 3 stars, {4}higher the better{5})",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Low Reputation
        data = new HelpData();
        data.tag = "megaCorp_3";
        data.header = string.Format("{0}Tips{1}", colourTip, colourEnd);
        data.text = string.Format("MegaCorps are {0}extremely powerful{1} organisations who don't take kindly to insignificant {2}insects{3} who annoy them. It doesn't generally {4}end{5} well for the insects",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
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
        data.text = string.Format("The level of {0}HQ Approval{1} (top centre) reflects your standing with your HQ as a whole. Each HQ member has a personal {2}Opinion{3} of you",
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
            .AppendFormat("The Boss (Director) has an {0}Opinion{1} but they also have a view on the {2}Decisions{3} you have taken. ", colourAlert, colourEnd, colourAlert, colourEnd)
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
        builder.AppendFormat("At the start of each turn your HQ decides whether to provide you with assistance ({0}+1 Power{1}) or not. ", colourAlert, colourEnd);
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
        builder.AppendFormat("Power is the {0}currency{1} of the game. The more you have the more you can do. ", colourAlert, colourEnd);
        builder.AppendFormat("Your main source of Power is from the support of your HQ. Aim to keep a {0}positive relationship{1} with them where ever possible.", colourAlert, colourEnd);
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
        builder.AppendFormat("The actor's {0}Compatibility{1} with you can {2}prevent{3} any change in Opinion occurring. ", colourAlert, colourEnd, colourAlert, colourEnd);
        builder.AppendFormat("If they have a good feeling about you {0}they may ignore{1} a drop in Opinion and vice versa. ", colourAlert, colourEnd);
        builder.AppendFormat("The {0}more{1} Compatible or Incompatibile they are, the {2}higher the chance{3} of them ignoring the change in opinion", colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //HQ support
        data = new HelpData();
        data.tag = "rand_1";
        int approval = 5;
        data.header = string.Format("{0}HQ Support{1}", colourTip, colourEnd);
        builder = new StringBuilder();
        builder.AppendFormat("HQ make a decision, each turn, whether to offer you support ({0}+1 Power{1}). ", colourAlert, colourEnd);
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
        data.text = string.Format("Each turn there is a chance that the Lead Investigator will {0}uncover new evidence{1}. The type of evidence will have a {2}higher chance{3} of being in your {4}favour{5} if the Lead has a high {6}Opinion{7}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Relations Opinion shifts
        data = new HelpData();
        data.tag = "rand_6";
        data.header = string.Format("{0}Relationships{1}", colourTip, colourEnd);
        data.text = string.Format("Whenever a {0}Friend or Enemy{1} relationship exists, any {2}change in Opinion{3} in one subordinate can affect the opinion of the {4}other subordinate{5}",
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
        //opinion
        data = new HelpData();
        data.tag = "metaGear_1";
        data.header = "Good Opinion";
        data.text = string.Format("They will only {0}offer the gear{1} to you if their opinion of you is positive ({2}Opinion 2+ stars{3}). They don't offer their gear to just anyone. " +
            "The {4}cost depends{5} on how good their {6}opinion{7} of you is", colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Max options
        data = new HelpData();
        data.tag = "metaGear_2";
        data.header = "Max Options";
        data.text = string.Format("There is an {0}limit{1} to how many gear options will be presented to you ({2}{3} options{4}). Eligible HQ personnel (Opinion 2+) present options in {5}order of seniority{6}" +
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
        data.text = string.Format("The {0}cost{1} of this option {2}varies{3} dependant on your superior's current {4}opinion{5} of you. The better their opinion ({6}Opinion{7}) " +
            "the less Power it will cost", colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Poor Relationship
        data = new HelpData();
        data.tag = "metaDevice_2";
        data.header = "Poor Relationship";
        data.text = string.Format("If your HQ Superior's opinion of you is poor ({0}Opinion 1, or less, stars{1}), they won't bother helping you and this {2}option won't be shown{3}",
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
        data.text = string.Format("This is a live option that you can {0}select{1} if you have {2}enough Power{3} available", colourAlert, colourEnd, colourAlert, colourEnd);
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
        data.text = string.Format("Options can have {0}criteria{1} that need to be met or it could be that you {2}don't have enough Power{3} to pay for it", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Are all inActive options shown?
        data = new HelpData();
        data.tag = "metaInactive_2";
        data.header = "Are all inActive options shown?";
        data.text = string.Format("{0}No{1}, only certain ones. If your HQ member doesn't like you, for instance, their special gear option won't be shown", colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #endregion

        #region Personality
        //
        // - - - Personality Factors
        //
        //Openness
        data = new HelpData();
        data.tag = "person_0";
        data.header = "Openness";
        data.text = string.Format("Reflects an individuals {0}imagination, feelings, actions{1} and {2}ideas{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //High Openness
        data = new HelpData();
        data.tag = "person_1";
        data.header = "High Score";
        data.text = string.Format("{0}Curious{1}, wide range of interests, independent", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Low Openness
        data = new HelpData();
        data.tag = "person_2";
        data.header = "Low Score";
        data.text = string.Format("{0}Practical{1}, conventional, prefers routine", colourAlert, colourEnd);
        listOfHelp.Add(data);
        data = new HelpData();

        //Conscientiousness
        data = new HelpData();
        data.tag = "person_3";
        data.header = "Conscientiousness";
        data.text = string.Format("Reflects an individuals {0}competence, self-discipline, thoughtfulness{1} and how {2}goal-driven{3} they are", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //High Openness
        data = new HelpData();
        data.tag = "person_4";
        data.header = "High Score";
        data.text = string.Format("{0}Organised{1}, hardworking, dependable", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Low Openness
        data = new HelpData();
        data.tag = "person_5";
        data.header = "Low Score";
        data.text = string.Format("{0}Impulsive{1}, careless, disorganised", colourAlert, colourEnd);
        listOfHelp.Add(data);

        //Extroversion
        data = new HelpData();
        data.tag = "person_6";
        data.header = "Extroversion";
        data.text = string.Format("Reflects an individuals {0}sociability, assertiveness{1} and {2}emotional expression{3}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //High Openness
        data = new HelpData();
        data.tag = "person_7";
        data.header = "High Score";
        data.text = string.Format("{0}Outgoing{1}, warm, seeks adventure", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Low Openness
        data = new HelpData();
        data.tag = "person_8";
        data.header = "Low Score";
        data.text = string.Format("{0}Quiet{1}, reserved, withdrawn", colourAlert, colourEnd);
        listOfHelp.Add(data);

        //Agreeableness
        data = new HelpData();
        data.tag = "person_9";
        data.header = "Agreeableness";
        data.text = string.Format("Reflects an individuals levels of {0}cooperation, trustworthiness{1},  and how {2}good natured{3} they are", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //High Openness
        data = new HelpData();
        data.tag = "person_10";
        data.header = "High Score";
        data.text = string.Format("{0}Helpful{1}, trusting, empathetic", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Low Openness
        data = new HelpData();
        data.tag = "person_11";
        data.header = "Low Score";
        data.text = string.Format("{0}Critical{1}, uncooperative, suspicious", colourAlert, colourEnd);
        listOfHelp.Add(data);

        //Neuroticism
        data = new HelpData();
        data.tag = "person_12";
        data.header = "Neuroticism";
        data.text = string.Format("Reflects an individuals tendency towards {0}unstable emotions{1}", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //High Openness
        data = new HelpData();
        data.tag = "person_13";
        data.header = "High Score";
        data.text = string.Format("{0}Anxious{1}, unhappy, prone to negative emotions", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Low Openness
        data = new HelpData();
        data.tag = "person_14";
        data.header = "Low Score";
        data.text = string.Format("{0}Calm{1}, even-tempered, secure", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //
        // - - - Personality overview
        //
        //personality -> overview
        data = new HelpData();
        data.tag = "person_15";
        data.header = "Personality";
        data.text = string.Format("Every character has a {0}unique{1} personality that defines their behaviour and {2}compatibility{3} {4} towards yourself. Your own personality determines your beliefs.",
            colourAlert, colourEnd, colourAlert, colourEnd, GameManager.i.guiScript.compatibilityIcon);
        listOfHelp.Add(data);
        //personality -> compatibility
        data = new HelpData();
        data.tag = "person_16";
        data.header = string.Format("Compatibility {0}", GameManager.i.guiScript.compatibilityIcon);
        data.text = string.Format("A character's compatibility with yourself (one to three stars, red or green) is calculated based on the {0}interaction of your personalities{1}. The greater the similarities, " +
            "the {2}more they like you{3} and vice versa",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //personality -> Character Personalities
        data = new HelpData();
        data.tag = "person_17";
        data.header = "Opinion and Mood";
        data.text = string.Format("A character's personality and compatibility, with yourself, are {0}constant{1}. They don't change, unlike their {2}Opinion{3} {4} of you which can {5}vary{6} due to circumstances," +
            " as can your {7}mood{8} {9}",
            colourAlert, colourEnd, colourAlert, colourEnd, GameManager.i.guiScript.opinionIcon, colourAlert, colourEnd, colourAlert, colourEnd, GameManager.i.guiScript.opinionIcon);
        listOfHelp.Add(data);
        //personality -> Tip
        data = new HelpData();
        data.tag = "person_18";
        data.header = string.Format("{0}Personality Tips{1}", colourTip, colourEnd);
        data.text = string.Format("The more {0}compatible{1} {2} a character is with you (green stars) the more they will {3}forgive{4} whereas characters who are {5}incompatible{6} (red stars) " +
            "{7}don't readily{8} change their opinion of you for the better",
            colourAlert, colourEnd, GameManager.i.guiScript.compatibilityIcon, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //
        // - - - Beliefs
        //
        //personality -> beliefs overview
        data = new HelpData();
        data.tag = "person_19";
        data.header = "Beliefs";
        data.text = string.Format("Your personality may {0}predispose{1} you to agreeing to, or opposing, certain actions based on your beliefs. " +
            "Listed below are various actions and what {2}effect{3} they will have on your {4}mood{5} {6}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, GameManager.i.guiScript.opinionIcon);
        listOfHelp.Add(data);
        //personality -> tip
        data = new HelpData();
        data.tag = "person_20";
        data.header = string.Format("{0}Tip{1}", colourTip, colourEnd);
        data.text = string.Format("Your personality {0}doesn't{1} prevent you from taking specific actions. You can {2}do what you like{3} but be aware that if you do something that is in {4}opposition{5} to your " +
            "beliefs your {6}mood{7} will suffer and vice versa. We all have a {8}natural tendency{9} to do what we prefer",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
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

        #region Anyone Captured
        //
        // - - - Anyone Captured
        //
        //overview
        data = new HelpData();
        data.tag = "capture_4";
        data.header = "Overview";
        data.text = string.Format("Whenever a character, or you the Player, are released from capture you {0}automatically{1} gain the {2}QUESTIONABLE{3} loyalty condition", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //traitor
        data = new HelpData();
        data.tag = "capture_5";
        data.header = "Subordinates";
        data.text = string.Format("Whenever a character has been captured there is a chance that the Authority has {0}done a deal{1} with them and they turn into a {2}TRAITOR{3}. This is a {3}hidden{4} condition",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //traitor
        data = new HelpData();
        data.tag = "capture_6";
        data.header = "Player";
        data.text = string.Format("Whenever you are captured you receive a {0}special event{1}. Depending on what option you choose your {2}Innocence Level{3} may drop. If it ever falls to {4}ZERO{5} stars, " +
            "you {6}Lose the Campaign{7}", colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
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
        builder.AppendFormat("If you are thinking of removing them, try and do so {0}before{1} they know your secrets as it costs less Power", colourAlert, colourEnd);
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
        data.text = string.Format("There is {0}no Power or Invisibilty penalty{1} for a cure but an {2}Action{3} will be used. Cures, once available, do not {4}time out{5}", colourAlert, colourEnd,
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
        //Spotting
        data = new HelpData();
        data.tag = "nemesis_1";
        data.header = "Spotting";
        builder = new StringBuilder();
        builder.AppendFormat("Tracers will {0}automatically{1} spot your Nemesis. Contacts will spot them only if their effetiveness is {2}equal to, or higher{3}, than the stealth rating of the Nemesis", 
            colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Hiding
        data = new HelpData();
        data.tag = "nemesis_2";
        data.header = "Hiding";
        builder = new StringBuilder();
        builder.AppendFormat("If your Nemesis is in the {0}same district{1} as yourself it will find you if your Invisibility is {2}less then, or equal to{3}, it's search rating", 
            colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Safe
        data = new HelpData();
        data.tag = "nemesis_3";
        data.header = "Safe Refuge";
        builder = new StringBuilder();
        builder.AppendFormat("If you are {0}Lying Low{1} or on {2}Stress Leave{3} your Nemesis can't find you. This {4}doesn't{5} apply if you are suffering a Stress induced {6}Breakdown{7}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        data.text = builder.ToString();
        listOfHelp.Add(data);
        //Modes
        data = new HelpData();
        data.tag = "nemesis_4";
        data.header = "Modes";
        builder = new StringBuilder();
        builder.AppendFormat("A Nemesis will wait at a central location until it gets a confirmed sighting whereupon it changes to {0}HUNT{1} mode and begins actively look for you. At times it may switch to {2}AMBUSH{3} mode" +
            " where it lurks in a likely district, waiting...",
            colourAlert, colourEnd, colourAlert, colourEnd);
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

        #region Contacts
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
        builder.AppendFormat("It ranges from...{0}{1}  1 star{2}<pos=45%>...knows stuff (worst){3}{4}  2 stars{5}<pos=45%>...is networked{6}{7}  3 stars{8}<pos=45%>...is Wired-in (best)",
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
        //Network Rating
        data = new HelpData();
        data.tag = "contact_11";
        data.header = "Network Rating";
        data.text = string.Format("Is the {0}sum{1} of each individual contact's {2}effectiveness{3} (1 to 3 stars) and provides a big picture view of the {4}value{5} of a subordinates contact network",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
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
        data.text = string.Format("New evidence will come to light as time progresses through {0}events{1} or as a result of the work of the {2}Lead Investigator{3} (Good evidence is more likely if they have a high {4}Opinion{5}, and vice versa)",
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
        .AppendFormat("There is a {0}{1} %{2} chance of them finding new evidence {3}each turn{4}. The type of evidence depends on the Opinion of the Lead{5}",
            colourAlert, GameManager.i.playerScript.chanceEvidence, colourEnd, colourAlert, colourEnd, "\n")
        .AppendFormat("  {0} Opinion 3, {1}Good 80%{2}, Bad 20%{3}", bullet, colourAlert, colourEnd, "\n")
        .AppendFormat("  {0} Opinion 2, {1}Good 60%{2}, Bad 40%{3}", bullet, colourAlert, colourEnd, "\n")
        .AppendFormat("  {0} Opinion 1, Good 40%, {1}Bad 60%{2}{3}", bullet, colourAlert, colourEnd, "\n")
        .AppendFormat("  {0} Opinion 0, Good 20%, {1}Bad 80%{2}", bullet, colourAlert, colourEnd)
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

        #region Friends and Enemies
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
        //Opinion
        data = new HelpData();
        data.tag = "relation_1";
        data.header = "Opinion";
        data.text = string.Format("If either party in a relationship experiences a {0}change in Opinion{1} then there is a {2}{3} % chance{4} of the other subordinate experiencing a change at the {5}same time{6}",
            colourAlert, colourEnd, colourAlert, GameManager.i.actorScript.chanceRelationShift, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Friends
        data = new HelpData();
        data.tag = "relation_2";
        data.header = "Friends";
        data.text = string.Format("In a Friends relationship the change in Opinion to one party is {0}identical{1} to the change in the other (they are happy when their friend is happy and vice versa)",
            colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Friends
        data = new HelpData();
        data.tag = "relation_3";
        data.header = "Enemies";
        data.text = string.Format("In an Enemies relationship the change in Opinion to one party is the {0}opposite{1} to the change in the other (they are happy when their enemy is upset and vice versa)",
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
        data.text = string.Format("Targets can be {0}attempted{1} by either the {2}Player{3} (you need to move to the {4}District{5} to do so) or by the {6}Subordinate{7} mentioned in the target description, eg. Blogger",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Subordinate
        data = new HelpData();
        data.tag = "target_1";
        data.header = "Subordinate";
        data.text = string.Format("If you have the {0}correct Subordinate{1} present in your line-up they can attempt the target at {2}any time{3}. There is no need for you to move to the District",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Target Attempts
        data = new HelpData();
        data.tag = "target_2";
        data.header = "Attempts";
        data.text = string.Format("{0}Left Click{1} on the {2}District{3} if you have the correct subordinate, or you are in the District, and mouseover {4}Attempt Target{5} to see your chances of success",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Power
        data = new HelpData();
        data.tag = "target_3";
        data.header = "Power";
        data.text = string.Format("If there is Power to be gained from {0}successfully{1} attempting the target, whoever attempts the target gets the Power ({1}Player or Subordinate{2})",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Power
        //
        // - - - Power
        //
        //Overvew
        data = new HelpData();
        data.tag = "power_0";
        data.header = "Overview";
        data.text = string.Format("Is a measure of how {0}important{1} a person is within the Organisation", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Player Power
        data = new HelpData();
        data.tag = "power_1";
        data.header = "Player Power";
        data.text = string.Format("Power is the currency you {0}use to do things{1}. It represents reputation, money, goodwill and accrued favours", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Subordinate Power
        data = new HelpData();
        data.tag = "power_2";
        data.header = "Subordinate Power";
        data.text = string.Format("A subordinate with more Power than yourself might get {0}ideas above their station{1}. They also have a higher chance of being {2}promoted to HQ{3} at the end of the level",
            colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //HQ Power
        data = new HelpData();
        data.tag = "power_3";
        data.header = "HQ Power";
        data.text = string.Format("Determines promotion. The {0}HQ hierarchy{1} are those with the highest Power, in {2}descending order of importance{3} ", colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Active Status
        //
        // - - - Active Status Player
        //
        //Overvew -> Player
        data = new HelpData();
        data.tag = "active_0";
        data.header = "Player Active";
        data.text = string.Format("What are doing here {0}wasting time?{1} Get to work, HQ are depending on you", colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Overvew -> Player
        data = new HelpData();
        data.tag = "active_1";
        data.header = "Subordinate Active";
        data.text = string.Format("All systems are go. Your subordinate and their {0}network of contacts{1} are at your disposal", colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region History
        //
        // - - - History
        //
        //Overvew 
        data = new HelpData();
        data.tag = "history_0";
        data.header = "History";
        data.text = string.Format("Shown is a history of all important events that have affected the character, or yourself. Any changes to your {0}Mood{1} {2} or a character's {3}Opinion{4} {5} of you are tracked as well",
            colourAlert, colourEnd, GameManager.i.guiScript.opinionIcon, colourAlert, colourEnd, GameManager.i.guiScript.opinionIcon);
        listOfHelp.Add(data);
        #endregion

        #region Stats
        //
        // - - - Stats
        //
        //Overvew 
        data = new HelpData();
        data.tag = "stat_0";
        data.header = "Statistics";
        data.text = string.Format("Shown are a collection of statistics relevant to the character, or yourself. We know {0}EVERYTHING{1}", colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        #region Gear
        //
        // - - - Gear
        //
        //Overvew 
        data = new HelpData();
        data.tag = "gear_0";
        data.header = "Gear";
        data.text = string.Format("Gear are {0}special{1} items that can confer many different {2}benefits{3}. You own the gear but you may {4}gift{5} an item to a subordinate and, perhaps, {6}recover{7} it from them later",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Capabilities
        data = new HelpData();
        data.tag = "gear_1";
        data.header = "Gear Capabilities";
        data.text = new StringBuilder()
                    .AppendFormat("{0}District use{1} indicates gear can be used in a district. Right Click on a district{2}", colourAlert, colourEnd, "\n")
                    .AppendLine()
                    .AppendFormat("{0}Personal use{1} indicates gear with a personal dimension. Right Click on your portrait{2}", colourAlert, colourEnd, "\n")
                    .AppendLine()
                    .AppendFormat("{0}AI use{1} provides a benefit when hacking the AI. Automatic{2}", colourAlert, colourEnd, "\n")
                    .AppendLine()
                    .AppendFormat("{0}Movement{1} allows you to negate different levels of Connection security when moving between districts{2}", colourAlert, colourEnd, "\n")
                    .AppendLine()
                    .AppendFormat("{0}Invisibility{1} prevents detection whenever you do a district action. Automatic{2}", colourAlert, colourEnd, "\n")
                    .AppendLine()
                    .AppendFormat("{0}Target Use{1} can be used on ANY target. Other gear may be usable on specific targets. Automatic{2}", colourAlert, colourEnd, "\n")
                    .AppendLine()
                    .AppendFormat("{0}Can be Gifted{1} ALL gear can be gifted to a subordinate. Right Click subordinate's portrait{2}{3}", colourAlert, colourEnd, "\n", "\n")
                    .ToString();
        listOfHelp.Add(data);
        //Rarity
        data = new HelpData();
        data.tag = "gear_2";
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
        data.tag = "gear_3";
        data.header = "Gifting Gear to Subordinates";
        data.text = string.Format("Will give your subordinate a {0}Opinion{1} boost. You can {2}ask for the gear back{3} after {4}{5} turns{6}. Would you be happy about returning a gift?",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, GameManager.i.gearScript.actorGearGracePeriod, colourEnd);
        listOfHelp.Add(data);
        //Gear Limits
        data = new HelpData();
        data.tag = "gear_4";
        data.header = "Limits";
        data.text = string.Format("You can have up to {0}{1} items{2} of {3}gear{4}. Any gear that you have {5}gifted{6} ('loaned') to Subordinates {7}doesn't count{8} towards your limit",
            colourAlert, GameManager.i.gearScript.maxNumOfGear, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Devices
        data = new HelpData();
        data.tag = "gear_5";
        data.header = "Devices";
        data.text = string.Format("These special items can {0}only{1} be obtained from {2}HQ characters{3} between levels. They can help you resist {4}interrogation{5} when you've been {6}captured{7}",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        //Devices Use
        data = new HelpData();
        data.tag = "gear_6";
        data.header = "Use";
        data.text = string.Format("Devices are used to {0}resist interrogation{1} techniques by the Authority which will {2}vary{3} depending on how big a threat ({4}Innocence{5} {6}) they percieve you as. " +
            "Devices are {7}specific{8} to a particular Innocence {9} level",
            colourAlert, colourEnd, colourAlert, colourEnd, colourAlert, colourEnd, GameManager.i.guiScript.innocenceIcon, colourAlert, colourEnd, GameManager.i.guiScript.innocenceIcon);
        listOfHelp.Add(data);
        //Gifting
        data = new HelpData();
        data.tag = "gear_7";
        data.header = "Devices";
        data.text = string.Format("Devices {0}don't{1} count towards your gear limit ({2}{3}{4} items) and {5}cannot be gifted{6} to your Subordinates",
            colourAlert, colourEnd, colourAlert, GameManager.i.gearScript.maxNumOfGear, colourEnd, colourAlert, colourEnd);
        listOfHelp.Add(data);
        #endregion

        //
        // - - - Return
        //
        //return help
        return listOfHelp;
    }
    #endregion

    #region DebugShowHelp
    /// <summary>
    /// allows a help window to pop-up showing the topics specified below. Used for debugging help topics
    /// </summary>
    public void DebugShowHelp()
    {
        string tag0 = "nemesis_2";
        string tag1 = "nemesis_3";
        string tag2 = "nemesis_1";
        string tag3 = "nemesis_4";
        List<HelpData> listOfHelp = GetHelpData(tag0, tag1, tag2, tag3);
        Vector3 screenPos = new Vector3(Screen.width / 2, Screen.height / 2);
        GameManager.i.tooltipHelpScript.SetTooltip(listOfHelp, screenPos);
    }
    #endregion

    #region DisplayHelp
    /// <summary>
    /// debug method to display all keyboard commands
    /// </summary>
    public string DisplayHelp()
    {
        return new StringBuilder()
            .AppendFormat(" Keyboard Commands{0},{1}", "\n", "\n")
            .AppendFormat(" Help -> F1{0}", "\n")
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
            .AppendFormat(" Corporate Nodes -> F2{0}", "\n")
            .AppendFormat(" Gated Nodes -> F3{0}", "\n")
            .AppendFormat(" Government Nodes -> F4{0}", "\n")
            .AppendFormat(" Industrial Nodes -> F5{0}", "\n")
            .AppendFormat(" Research Nodes -> F6{0}", "\n")
            .AppendFormat(" Sprawl Nodes -> F7{0}", "\n")
            .AppendFormat(" Utility Nodes -> F8{0}", "\n")
            .AppendFormat(" Debug Show -> D{0}{1}", "\n", "\n")
            .AppendFormat(" Player Known Nodes -> F10{0}{1}", "\n", "\n")
            .AppendFormat(" Activity Time -> F11{0}", "\n")
            .AppendFormat(" Activity Count -> F12{0}{1}", "\n", "\n")
            .AppendFormat(" MainInfoApp display -> I{0}", "\n")
            .AppendFormat(" MainInfoApp ShowMe -> Space{0}", "\n")
            .AppendFormat(" MainInfoApp Home -> Home{0}", "\n")
            .AppendFormat(" MainInfoApp End -> End{0}", "\n")
            .AppendFormat(" MainInfoApp Back -> PageDn{0}", "\n")
            .AppendFormat(" MainInfoApp Forward -> PageUp{0}{1}", "\n", "\n")
            .AppendFormat("-Global Options (Modal/non-Modal){0}", "\n")
            .AppendFormat(" Actor Info toggle -> 1{0}", "\n")
            .AppendFormat(" Actor Relations toggle -> 2{0}", "\n")
            .AppendFormat(" Test Condition -> Z{0}", "\n")
            .ToString();
    }
    #endregion

    #region HelpMessages...
    //
    // - - - Help Messages
    //

    #region InitialiseHelpMessages
    /// <summary>
    /// Converts HelpMessage.SO's to data packages and populates DM -> dictOfHelpMessages
    /// </summary>
    /// <param name="listOfMessages"></param>
    public void InitialiseHelpMessages(List<HelpMessage> listOfMessages)
    {
        if (listOfMessages != null)
        {
            int countConditions;
            int countMessages = listOfMessages.Count;
            if (countMessages > 0)
            {
                Dictionary<string, HelpMessageData> dictOfHelpMessages = GameManager.i.dataScript.GetDictOfHelpMessages();
                if (dictOfHelpMessages != null)
                {
                    //check empty - info message if not
                    if (dictOfHelpMessages.Count > 0)
                    { Debug.LogWarningFormat("Invalid dictOfHelpMessages (has {0} records, should be Empty)", dictOfHelpMessages.Count); }
                    //clear any residual data from dictionary
                    dictOfHelpMessages.Clear();
                    //Populate dictionary
                    for (int i = 0; i < countMessages; i++)
                    {
                        HelpMessage message = listOfMessages[i];
                        if (message != null)
                        {
                            //convert to data packages
                            HelpMessageData dataMsg = new HelpMessageData();
                            dataMsg.name = message.name;
                            dataMsg.textTop = message.textTop;
                            dataMsg.textBottom = message.textBottom;
                            //convert any conditions to data packages
                            countConditions = message.listOfConditions.Count;
                            if (countConditions > 0)
                            {
                                for (int k = 0; k < countConditions; k++)
                                {
                                    HelpCondition condition = message.listOfConditions[k];
                                    if (condition != null)
                                    {
                                        HelpConditionData dataCon = new HelpConditionData();
                                        dataCon.name = condition.name;
                                        dataCon.isEquals = condition.isEquals;
                                        dataCon.isGreaterThan = condition.isGreaterThan;
                                        dataCon.isLessThan = condition.isLessThan;
                                        dataCon.isPresent = condition.isPresent;
                                        dataCon.isNotPresent = condition.isNotPresent;
                                        dataCon.lowerLimit = condition.lowerLimit;
                                        dataCon.upperLimit = condition.upperLimit;
                                        dataCon.setNumber = condition.setNumber;
                                        dataCon.conType = GetConditionType(condition.name);
                                        dataCon.conValue = GetConditionValue(dataCon.conType);
                                        //add to list
                                        dataMsg.listOfConditions.Add(dataCon);
                                    }
                                    else { Debug.LogErrorFormat("Invalid HelpCondition (Null) in {0}.listOfConditions[{1}]", message.name, k); }
                                }
                            }
                            //add to dict
                            dictOfHelpMessages.Add(dataMsg.name, dataMsg);
                        }
                        else { Debug.LogErrorFormat("Invalid HelpMessage (Null) in listOfMessages[{0}]", i); }
                    }
                    //log
                    Debug.LogFormat("[Loa] HelpManager.cs -> InitialiseHelpManager.cs: dictOfHelpMessages loaded up with {0} records{1}", dictOfHelpMessages.Count, "\n");
                }
                else { Debug.LogError("Invalid dictOfHelpMessages (Null)"); }
            }
            else { Debug.LogError("Invalid listOfMessages (Empty)"); }
        }
        else { Debug.LogError("Invalid listOfMessages (Null)"); }
    }
    #endregion


    #region GetHelpMessageType
    /// <summary>
    /// Converts HelpMessage.SO name into an enum for faster access. Returns HelpConditionType.None if a problem
    /// </summary>
    /// <param name="messageName"></param>
    /// <returns></returns>
    private HelpConditionType GetConditionType(string conditionName)
    {
        HelpConditionType conditionType = HelpConditionType.None;
        if (string.IsNullOrEmpty(conditionName) == false)
        {
            switch(conditionName)
            {
                case "GearGet": conditionType = HelpConditionType.GearGet; break;
                case "FixerPresent": conditionType = HelpConditionType.FixerOnMap; break;
                case "Hacking": conditionType = HelpConditionType.DistrictHack; break;
                case "HackerPresent": conditionType = HelpConditionType.HackerOnMap; break;
                case "PlayerLieLow": conditionType = HelpConditionType.PlayerLieLow; break;
                case "Diversion": conditionType = HelpConditionType.ActionDiversion; break;
                case "TargetIntelGet": conditionType = HelpConditionType.TargetIntelGet; break;
                case "PlannerPresent": conditionType = HelpConditionType.PlannerOnMap; break;
                default: Debug.LogWarningFormat("Unrecognised conditionName \"{0}\"", conditionName); break;
            }
        }
        else { Debug.LogError("Invalid conditionName (Null or Empty)"); }
        return conditionType;
    }
    #endregion

    #region GetConditionValue
    /// <summary>
    /// Gets current value of a condition type. If a bool value is expected will return 1 for true, 0 for false. Returns -1 if a problem
    /// </summary>
    /// <param name="conType"></param>
    /// <returns></returns>
    private int GetConditionValue(HelpConditionType conType)
    {
        int value = -1;
        switch (conType)
        {
            case HelpConditionType.GearGet:
                value = GameManager.i.dataScript.StatisticGetLevel(StatType.GearTotal);
                break;
            case HelpConditionType.FixerOnMap:
                value = GameManager.i.dataScript.CheckActorArcPresent(arcFixer, sideResistance, true) == true ? 1 : 0;
                break;
            case HelpConditionType.PlannerOnMap:
                value = GameManager.i.dataScript.CheckActorArcPresent(arcPlanner, sideResistance, true) == true ? 1 : 0;
                break;
            case HelpConditionType.HackerOnMap:
                value = GameManager.i.dataScript.CheckActorArcPresent(arcHacker, sideResistance, true) == true ? 1 : 0;
                break;
            case HelpConditionType.PlayerLieLow:
                value = GameManager.i.playerScript.Invisibility;
                break;
            case HelpConditionType.ActionDiversion:
                value = GameManager.i.dataScript.StatisticGetLevel(StatType.SubordinateNodeActions);
                break;
            case HelpConditionType.TargetIntelGet:
                value = GameManager.i.dataScript.StatisticGetLevel(StatType.TargetInfo);
                break;
            case HelpConditionType.DistrictHack:
                
                break;
            default: Debug.LogWarningFormat("Unrecognised conType \"{0}\"", conType); break;
        }
        return value;
    }
    #endregion

    #region DebugDisplayHelpMessages
    /// <summary>
    /// Displays contents of DM -> dictOfHelpMessages
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayHelpMessages()
    {
        StringBuilder builder = new StringBuilder();
        Dictionary<string, HelpMessageData> dictOfHelpMessages = GameManager.i.dataScript.GetDictOfHelpMessages();
        if (dictOfHelpMessages != null)
        {
            int count = dictOfHelpMessages.Count;
            int numOfConditions;
            HelpConditionData condition;
            builder.AppendFormat("-dictOfHelpMessages  {0} record{1}{2}{3}", count, count != 1 ? "s" : "", "\n", "\n");
            if (count == 0)
            { builder.Append("No records"); }
            else
            {
                foreach (var data in dictOfHelpMessages)
                {
                    numOfConditions = data.Value.listOfConditions.Count;
                    builder.AppendFormat(" {0} -> texts {1}, {2} condition{3}, isDone {4}{5}", 
                        data.Key, 
                        data.Value.textTop.Length > 0 || data.Value.textBottom.Length > 0 ? "ok" : "M.I.A",
                        numOfConditions > 0 ? Convert.ToString(numOfConditions) : "No", 
                        numOfConditions != 1 ? "s" : "", 
                        data.Value.isDone, 
                        "\n");
                    if (numOfConditions > 0)
                    {
                        for (int i = 0; i < numOfConditions; i++)
                        {
                            condition = data.Value.listOfConditions[i];
                            if (condition != null)
                            {
                                builder.AppendFormat("    {0} -> {1}, val {2}{3}{4}{5}{6}{7}{8}{9}{10}", 
                                    condition.name, 
                                    condition.conType,
                                    condition.conValue, 
                                    condition.isEquals == true ? ", isEqual " : "",
                                    condition.isGreaterThan == true ? ", isGreaterThan " : "",
                                    condition.isLessThan == true ? ", isLessThan " : "",
                                    condition.isPresent == true ? ", Present" : "",
                                    condition.isNotPresent == true ? ", Not Present" : "",
                                    condition.lowerLimit > -1 || condition.upperLimit > -1 ? " limits " + condition.lowerLimit + "/" + condition.upperLimit : "",
                                    condition.setNumber > -1 ? " set # " + condition.setNumber : "", 
                                    "\n");
                            }
                            else { Debug.LogErrorFormat("Invalid HelpConditionData (Null) in {0}.listOfConditions[{1}]", data.Key, i); }
                        }
                    }
                }
            }
        }
        else { Debug.LogError("Invalid dictOfHelpMessages (Null)"); }
        return builder.ToString();
    }
    #endregion

    #endregion


}
