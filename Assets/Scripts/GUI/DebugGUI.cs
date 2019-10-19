using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using packageAPI;
using System;

/// <summary>
/// debug GUI, on demand (HotKey 'D' -> On, HotKey 'H' -> Off)
/// </summary>
public class DebugGUI : MonoBehaviour
{
    //for whenever interaction is needed
    private enum GUIStatus { None, GiveGear, GiveCondition, RemoveCondition, GiveActorTrait, SetState, AddContact, RemoveContact, isKnownContact, ShowPath, ShowPathOff, NemesisControl, ContactToggle}

    public GUIStyle customBackground;

    private GUIStatus status;
    private MessageCategory msgStatus;
    private AIDebugData aiStatus;
    private TeamDebug teamStatus;
    private DebugRegister registerStatus;
    public  bool showGUI = false;
    private bool doOnceFlag = false;                            //used to prevent continual repeats of input / output sequences
    private int debugDisplay = 0;

    public int button_height;
    [Tooltip("Space between left edge of box and button")]
    public int offset_x;  
    [Tooltip("Space between buttons")]
    public int offset_y;      
    public int box_x;
    public int box_y;
    public int box_width;
    public int box_height;
    [Tooltip("gap at start between header and first button")]
    public int gap_y;

    private int box_info;
    private int box_option;
    private int box_action;
    private int box_level; 
    private int button_width;
    private int targetToggle = 0;
    private int personalityToggle = 0;
    private int playerToggle = 0;
    private int contactToggle = 0;
    private int trackerToggle = 0;
    private int newsToggle = 0;
    private int actorToggle = 0;
    private int topicToggle = 0;
    private int factionToggle = 0;
    private int analyseToggle = 0;
    private int statisticsToggle = 0;
    private string textInput_0 = "what";
    private string textInput_1 = "who";
    private string analysis = "Unknown";
    private string textOutput;
    public string optionAutoGear;
    public string optionFogOfWar;
    public string optionConnectorTooltips;
    public string optionDebugData;
    private string optionNoAI;
    private string optionAIOffline;
    private string optionAITraceback;
    private string optionAIScreamer;
    public string optionRenownUI;
    private string optionPath;
    public string optionContacts;
    private string optionMoodInfo;

    private void Awake()
    {
        button_width = box_width - (2 * offset_x);
        box_option = box_x;
        box_info = box_x * 2 + box_width;
        box_action = box_x * 3 + box_width * 2;
        box_level = box_x * 4 + box_width * 3;
        //option strings
        optionAutoGear = "Auto Gear ON";
        optionFogOfWar = "Fog Of War ON";
        optionConnectorTooltips = "Conn tooltips ON";
        optionDebugData = "Debug Data ON";
        optionNoAI = "NO AI ON";
        optionAIOffline = "AIOffline ON";
        optionAITraceback = "AITraceback ON";
        optionAIScreamer = "AIScreamer ON";
        optionRenownUI = "Renown UI OFF";
        optionPath = "Input Path";
        optionContacts = "Contacts ON";
        optionMoodInfo = "Mood Info ON";
    }

    // Update is called once per frame
    private void Update()
    {
        //Toggle debug view on/off with 'D'
        if (Input.GetKeyDown("d") == true)
        {
            if (showGUI == false)
            { showGUI = true; }
            else { showGUI = false; }
        }
        else if (Input.GetKeyDown(KeyCode.Escape) == true && showGUI == true)
        {
            showGUI = false;
            doOnceFlag = false;
        }

    }

    private void OnGUI()
    {
        if (showGUI == true)
        {
            //
            // - - - Left Hand side of Screen - - -
            //


            customBackground.alignment = TextAnchor.UpperCenter;
            //background box (Options)
            GUI.Box(new Rect(box_option, box_y, box_width, box_height / 2 + 40), "Option Menu", customBackground);
            //background box (Info)
            GUI.Box(new Rect(box_info, box_y, box_width, box_height + 240), "Info Menu", customBackground);
            //background box (Actions)
            GUI.Box(new Rect(box_action, box_y, box_width, box_height + 240), "Action Menu", customBackground);
            //background box (Level)
            GUI.Box(new Rect(box_level, box_y, box_width, box_height / 2 + 60), "Level Menu", customBackground);

            //
            // - - - Info (first box)
            //

            //first button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 0 + button_height * 0, button_width, button_height), "Help"))
            {
                Debug.Log("[Dbg] Button -> Toggle Help");
                if (debugDisplay != 13)
                { debugDisplay = 13; }
                else { debugDisplay = 0; }
            }

            //second button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 1 + button_height * 1, button_width, button_height), "Game Data"))
            {
                Debug.Log("[Dbg] Button -> Toggle Game Data");
                if (debugDisplay != 5)
                { debugDisplay = 5; }
                else { debugDisplay = 0; }
            }

            //third button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 2 + button_height * 2, button_width, button_height), "Campaign Data"))
            {
                Debug.Log("[Dbg] Button -> Toggle Campaign Data");
                if (debugDisplay != 55)
                { debugDisplay = 55; }
                else { debugDisplay = 0; }
            }

            // fourth button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 3 + button_height * 3, button_width, button_height), "Scenario Data"))
            {
                Debug.Log("[Dbg] Button -> Toggle Scenario Data");
                if (debugDisplay != 56)
                { debugDisplay = 56; }
                else { debugDisplay = 0; }
            }

            //fifth button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 4 + button_height * 4, button_width, button_height), "Register Data"))
            {
                Debug.Log("[Dbg] Button -> Toggle Register Data");
                //toggles sequentially through register data displays and then switches off
                switch (registerStatus)
                {
                    case DebugRegister.None: debugDisplay = 14; registerStatus = DebugRegister.Ongoing; break;
                    case DebugRegister.Ongoing: debugDisplay = 15; registerStatus = DebugRegister.Actions; break;
                    case DebugRegister.Actions: debugDisplay = 0; registerStatus = DebugRegister.None; break;
                }
            }

            //sixth button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 5 + button_height * 5, button_width, button_height), "Actor Data"))
            {
                Debug.Log("[Dbg] Button -> Toggle Actor Data");
                //toggles sequentially through actor displays and then switches off
                switch (actorToggle)
                {
                    case 0: debugDisplay = 7; actorToggle = 1; break;
                    case 1: debugDisplay = 6; actorToggle = 2; break;
                    case 2: debugDisplay = 54; actorToggle = 3; break;
                    case 3: debugDisplay = 74; actorToggle = 4; break;
                    case 4: debugDisplay = 68; actorToggle = 5; break;
                    case 5: debugDisplay = 69; actorToggle = 6; break;
                    case 6: debugDisplay = 0; actorToggle = 0; break;
                }
            }

            //seventh button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 6 + button_height * 6, button_width, button_height), "City Data"))
            {
                Debug.Log("[Dbg] Button -> Toggle City Data");
                if (debugDisplay != 1)
                { debugDisplay = 1; }
                else { debugDisplay = 0; }
            }

            //eigth button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 7 + button_height * 7, button_width, button_height), "Faction Data"))
            {
                Debug.Log("[Dbg] Button -> Toggle Faction Data");
                if (debugDisplay != 11)
                { debugDisplay = 11; }
                else { debugDisplay = 0; }

                switch (factionToggle)
                {
                    case 0: debugDisplay = 11; factionToggle = 1; break;
                    case 1: debugDisplay = 71; factionToggle = 2; break;
                    case 2: debugDisplay = 0; factionToggle = 0; break;
                }
            }

            //ninth button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 8 + button_height * 8, button_width, button_height), "Player Data"))
            {
                Debug.Log("[Dbg] Button -> Toggle Player Data");
                switch (playerToggle)
                {
                    case 0: debugDisplay = 8; playerToggle = 1; break;
                    case 1: debugDisplay = 60; playerToggle = 2; break;
                    case 2: debugDisplay = 0; playerToggle = 0; break;
                }
            }

            //tenth button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 9 + button_height * 9, button_width, button_height), "Rebel AI Data"))
            {
                Debug.Log("[Dbg] Button -> Rebel AI Data");
                if (debugDisplay != 43)
                { debugDisplay = 43; }
                else { debugDisplay = 0; }
            }

            //eleventh
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 10 + button_height * 10, button_width, button_height), "Authority AI"))
            {
                Debug.Log("[Dbg] Button -> Authority AI Data");
                switch (aiStatus)
                {
                    case AIDebugData.None: debugDisplay = 10; aiStatus = AIDebugData.Task; break;
                    case AIDebugData.Task: debugDisplay = 10; aiStatus = AIDebugData.Node; break;
                    case AIDebugData.Node: debugDisplay = 10; aiStatus = AIDebugData.Spider; break;
                    case AIDebugData.Spider: debugDisplay = 10; aiStatus = AIDebugData.Erasure; break;
                    case AIDebugData.Erasure: debugDisplay = 10; aiStatus = AIDebugData.Decision; break;
                    case AIDebugData.Decision: debugDisplay = 0; aiStatus = AIDebugData.None; break;
                }
            }

            //twelth button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 11 + button_height * 11, button_width, button_height), "Team Data"))
            {
                Debug.Log("[Dbg] Button -> Team Data");
                //toggles sequentially through team data displays and then switches off
                switch (teamStatus)
                {
                    case TeamDebug.None: debugDisplay = 2; teamStatus = TeamDebug.Pools; break;
                    case TeamDebug.Pools: debugDisplay = 2; teamStatus = TeamDebug.Roster; break;
                    case TeamDebug.Roster:
                        if (GameManager.instance.sideScript.authorityOverall == SideState.Human)
                        { debugDisplay = 2; teamStatus = TeamDebug.Actors; }
                        else { debugDisplay = 0; teamStatus = TeamDebug.None; }
                        break;
                    case TeamDebug.Actors: debugDisplay = 0; teamStatus = TeamDebug.None; break;
                }
            }

            //thirteenth button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 12 + button_height * 12, button_width, button_height), "Message Data"))
            {
                Debug.Log("[Dbg] Button -> Message Data");
                //toggles sequentially through message dictionaries and then switches off
                switch (msgStatus)
                {
                    case MessageCategory.None: debugDisplay = 9; msgStatus = MessageCategory.Pending; break;
                    case MessageCategory.Pending: debugDisplay = 9; msgStatus = MessageCategory.Current; break;
                    case MessageCategory.Current: debugDisplay = 9; msgStatus = MessageCategory.Archive; break;
                    case MessageCategory.Archive: debugDisplay = 9; msgStatus = MessageCategory.AI; break;
                    case MessageCategory.AI: debugDisplay = 0; msgStatus = MessageCategory.None; break;
                }
            }

            //fourteenth button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 13 + button_height * 13, button_width, button_height), "Gear Data"))
            {
                Debug.Log("[Dbg] Button -> Gear Data");
                if (debugDisplay != 24)
                { debugDisplay = 24; }
                else { debugDisplay = 0; }
            }

            //fifteenth button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 14 + button_height * 14, button_width, button_height), "Secret Data"))
            {
                Debug.Log("[Dbg] Button -> Secret Data");
                if (debugDisplay != 25)
                { debugDisplay = 25; }
                else { debugDisplay = 0; }
            }

            //sixteenth button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 15 + button_height * 15, button_width, button_height), "Node Crisis Data"))
            {
                Debug.Log("[Dbg] Button -> Node Crisis Data");
                if (debugDisplay != 26)
                { debugDisplay = 26; }
                else { debugDisplay = 0; }
            }

            //sevenTeenth button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 16 + button_height * 16, button_width, button_height), "Contact Data"))
            {
                Debug.Log("[Dbg] Button -> Contact Data");
                switch (contactToggle)
                {
                    case 0: debugDisplay = 33; contactToggle = 1; break;
                    case 1: debugDisplay = 46; contactToggle = 2; break;
                    case 2: debugDisplay = 49; contactToggle = 3; break;
                    case 3: debugDisplay = 0; contactToggle = 0; break;
                }
            }

            //eightennth button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 17 + button_height * 17, button_width, button_height), "Target Data"))
            {
                Debug.Log("[Dbg] Button -> Target Data");
                //toggles sequentially through target info displays and then switches off
                switch (targetToggle)
                {
                    case 0: debugDisplay = 34; targetToggle = 1; break;
                    case 1: debugDisplay = 35; targetToggle = 2; break;
                    case 2: debugDisplay = 36; targetToggle = 3; break;
                    case 3: debugDisplay = 0; targetToggle = 0; break;
                }
            }

            //nineteenth button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 18 + button_height * 18, button_width, button_height), "Nemesis Data"))
            {
                Debug.Log("[Dbg] Button -> Nemesis Data");
                if (debugDisplay != 40)
                { debugDisplay = 40; }
                else { debugDisplay = 0; }
            }

            //twentieth button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 19 + button_height * 19, button_width, button_height), "Statistics Data"))
            {
                Debug.Log("[Dbg] Button -> Statistics Data");
                //toggles sequentially through statistic info displays and then switches off
                switch (statisticsToggle)
                {
                    case 0: debugDisplay = 50; statisticsToggle = 1; break;
                    case 1: debugDisplay = 77; statisticsToggle = 2; break;
                    case 2: debugDisplay = 0; statisticsToggle = 0; break;
                }
            }

            //twentyfirst button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 20 + button_height * 20, button_width, button_height), "Analysis Data"))
            {
                Debug.Log("[Dbg] Button -> AI Analysis Data");
                //toggles sequentially through analysis info displays and then switches off
                switch (analyseToggle)
                {
                    case 0: debugDisplay = 51; analyseToggle = 1; break;
                    case 1: debugDisplay = 75; analyseToggle = 2; break;
                    case 2: debugDisplay = 76; analyseToggle = 3; break;
                    case 3: debugDisplay = 0; analyseToggle = 0; break;
                }
            }


            //twentysecond button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 21 + button_height * 21, button_width, button_height), "Personality Data"))
            {
                Debug.Log("[Dbg] Button -> Personality Data");
                //toggles sequentially through personality info displays and then switches off
                switch (personalityToggle)
                {
                    case 0: debugDisplay = 57; personalityToggle = 1; break;
                    case 1: debugDisplay = 70; personalityToggle = 2; break;
                    case 2: debugDisplay = 58; personalityToggle = 3; break;
                    case 3: debugDisplay = 59; personalityToggle = 4; break;
                    case 4: debugDisplay = 61; personalityToggle = 5; break;
                    case 5: debugDisplay = 0; personalityToggle = 0; break;
                }
            }

            //twentysecond button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 22 + button_height * 22, button_width, button_height), "Topic Data"))
            {
                Debug.Log("[Dbg] Button -> Topic Data");
                if (debugDisplay != 62)
                { debugDisplay = 62; }
                else { debugDisplay = 0; }
                switch (topicToggle)
                {
                    case 0: debugDisplay = 62; topicToggle = 1; break;
                    case 1: debugDisplay = 63; topicToggle = 2; break;
                    case 2: debugDisplay = 64; topicToggle = 3; break;
                    case 3: debugDisplay = 65; topicToggle = 4; break;
                    case 4: debugDisplay = 66; topicToggle = 5; break;
                    case 5: debugDisplay = 67; topicToggle = 6; break;
                    case 6: debugDisplay = 0; topicToggle = 0; break;
                }
            }

            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 23 + button_height * 23, button_width, button_height), "Tracker Data"))
            {
                Debug.Log("[Dbg] Button -> Tracker Data");
                switch (trackerToggle)
                {
                    case 0: debugDisplay = 44; trackerToggle = 1; break;
                    case 1: debugDisplay = 45; trackerToggle = 2; break;
                    case 2: debugDisplay = 0; trackerToggle = 0; break;
                }
            }

            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 24 + button_height * 24, button_width, button_height), "News Data"))
            {
                Debug.Log("[Dbg] Button -> News Data");
                switch (newsToggle)
                {
                    case 0: debugDisplay = 72; newsToggle = 1; break;
                    case 1: debugDisplay = 73; newsToggle = 2; break;
                    case 2: debugDisplay = 0; newsToggle = 0; break;
                }
            }


            //twentySeventh button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 25 + button_height * 25, button_width, button_height), "Organisation Data"))
            {
                Debug.Log("[Dbg] Button -> Organisation Data");
                if (debugDisplay != 78)
                { debugDisplay = 78; }
                else { debugDisplay = 0; }
            }

            //
            // - - - Options (second box)
            //

            //first button
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 0 + button_height * 0, button_width, button_height), "Show Options"))
            {
                Debug.Log("[Dbg] Button -> Show Options");
                if (debugDisplay != 12)
                { debugDisplay = 12; }
                else { debugDisplay = 0; }
            }

            //second button
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 1 + button_height * 1, button_width, button_height), optionAutoGear))
            {
                Debug.Log("[Dbg] Button -> Toggle OptionAutoGear");
                if (GameManager.instance.optionScript.autoGearResolution == true)
                {
                    GameManager.instance.optionScript.autoGearResolution = false;
                    optionAutoGear = "Auto Gear ON";
                }
                else
                {
                    GameManager.instance.optionScript.autoGearResolution = true;
                    optionAutoGear = "Auto Gear OFF";
                }
            }

            //third button
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 2 + button_height * 2, button_width, button_height), optionFogOfWar))
            {
                Debug.Log("[Dbg] Button -> Toggle Fog Of War");
                if (GameManager.instance.optionScript.fogOfWar == true)
                {
                    //sets both FOW and Nemesis settings in tandem
                    GameManager.instance.optionScript.fogOfWar = false;
                    GameManager.instance.nemesisScript.isShown = true;
                    optionFogOfWar = "Fog Of War ON";
                }
                else
                {
                    GameManager.instance.optionScript.fogOfWar = true;
                    GameManager.instance.nemesisScript.isShown = false;
                    optionFogOfWar = "Fog Of War OFF";
                }
                //redraw and update nodes
                GameManager.instance.nodeScript.NodeRedraw = true;
            }

            //fourth button
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 3 + button_height * 3, button_width, button_height), optionConnectorTooltips))
            {
                Debug.Log("[Dbg] Button -> Toggle Connection Tooltips");
                if (GameManager.instance.optionScript.connectorTooltips == true)
                {
                    GameManager.instance.optionScript.connectorTooltips = false;
                    optionConnectorTooltips = "Conn Tooltips ON";
                }
                else
                {
                    GameManager.instance.optionScript.connectorTooltips = true;
                    optionConnectorTooltips = "Conn Tooltips OFF";
                }
            }

            //fifth button
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 4 + button_height * 4, button_width, button_height), optionDebugData))
            {
                Debug.Log("[Dbg] Button -> Toggle Debug Data");
                if (GameManager.instance.optionScript.debugData == true)
                {
                    GameManager.instance.optionScript.debugData = false;
                    optionDebugData = "Debug Data ON";
                }
                else
                {
                    GameManager.instance.optionScript.debugData = true;
                    optionDebugData = "Debug Data OFF";
                }
            }

            //sixth button
            if (GameManager.instance.turnScript.Turn > 0) { optionNoAI = "Unavailable"; }
            {
                if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 5 + button_height * 5, button_width, button_height), optionNoAI))
                {
                    Debug.Log("[Dbg] Button -> Toggle NO AI");
                    //option only available on first turn
                    if (GameManager.instance.turnScript.Turn == 0)
                    {

                        switch (GameManager.instance.sideScript.PlayerSide.level)
                        {
                            //authority player
                            case 1:
                                //switch AI Off -> Manual player control for both sides
                                if (GameManager.instance.sideScript.resistanceOverall == SideState.AI)
                                {
                                    optionNoAI = "NO AI OFF";
                                    GameManager.instance.optionScript.noAI = true;
                                    GameManager.instance.sideScript.authorityCurrent = SideState.Human;
                                    GameManager.instance.sideScript.resistanceCurrent = SideState.Human;
                                    GameManager.instance.sideScript.authorityOverall = SideState.Human;
                                    GameManager.instance.sideScript.resistanceOverall = SideState.Human;
                                    //notification
                                    GameManager.instance.guiScript.SetAlertMessage(AlertType.DebugAI);
                                }
                                //reverts back to Authority Player, Resistance AI
                                else if (GameManager.instance.sideScript.resistanceOverall == SideState.Human)
                                {
                                    optionNoAI = "NO AI ON";
                                    GameManager.instance.optionScript.noAI = false;
                                    GameManager.instance.sideScript.authorityCurrent = SideState.Human;
                                    GameManager.instance.sideScript.resistanceCurrent = SideState.AI;
                                    GameManager.instance.sideScript.authorityOverall = SideState.Human;
                                    GameManager.instance.sideScript.resistanceOverall = SideState.AI;
                                    //notification
                                    GameManager.instance.guiScript.SetAlertMessage(AlertType.DebugPlayer);
                                }
                                break;
                            //resistance player
                            case 2:
                                //switch AI Off -> Manual player control for both sides
                                if (GameManager.instance.sideScript.authorityOverall == SideState.AI)
                                {
                                    optionNoAI = "NO AI OFF";
                                    GameManager.instance.optionScript.noAI = true;
                                    GameManager.instance.sideScript.authorityCurrent = SideState.Human;
                                    GameManager.instance.sideScript.resistanceCurrent = SideState.Human;
                                    GameManager.instance.sideScript.authorityOverall = SideState.Human;
                                    GameManager.instance.sideScript.resistanceOverall = SideState.Human;
                                    //notification
                                    GameManager.instance.guiScript.SetAlertMessage(AlertType.DebugAI);
                                }
                                //reverts back to Resistance Player, Authority AI
                                else if (GameManager.instance.sideScript.authorityOverall == SideState.Human)
                                {
                                    optionNoAI = "NO AI ON";
                                    GameManager.instance.optionScript.noAI = false;
                                    GameManager.instance.sideScript.authorityCurrent = SideState.AI;
                                    GameManager.instance.sideScript.resistanceCurrent = SideState.Human;
                                    GameManager.instance.sideScript.authorityOverall = SideState.AI;
                                    GameManager.instance.sideScript.resistanceOverall = SideState.Human;
                                    //notification
                                    GameManager.instance.guiScript.SetAlertMessage(AlertType.DebugPlayer);
                                }
                                break;
                        }
                    }
                }
            }

            //tenth button
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 6 + button_height * 6, button_width, button_height), optionRenownUI))
            {
                Debug.Log("[Dbg] Button -> Toggle Renown UI Display");
                if (GameManager.instance.actorPanelScript.CheckRenownUIStatus() == true)
                {
                    GameManager.instance.actorPanelScript.SetActorRenownUI(false);
                    optionRenownUI = "Renown UI ON";
                }
                else
                {
                    GameManager.instance.actorPanelScript.SetActorRenownUI(true);
                    optionRenownUI = "Renown UI OFF";
                }
            }

            //eleventh button
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 7 + button_height * 7, button_width, button_height), optionContacts))
            {
                Debug.Log("[Dbg] Button -> Toggle Show Contacts in Node tooltips");
                if (GameManager.instance.optionScript.showContacts == true)
                {
                    GameManager.instance.optionScript.showContacts = false;
                    optionContacts = "Contacts ON";
                }
                else
                {
                    GameManager.instance.optionScript.showContacts = true;
                    optionContacts = "Contacts OFF";
                }
            }

            //twelfth button
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 8 + button_height * 8, button_width, button_height), optionMoodInfo))
            {
                Debug.Log("[Dbg] Button -> Toggle Full Mood Info option");
                if (GameManager.instance.optionScript.fullMoodInfo == true)
                {
                    GameManager.instance.optionScript.fullMoodInfo = false;
                    optionMoodInfo = "Mood Info ON";
                }
                else
                {
                    GameManager.instance.optionScript.fullMoodInfo = true;
                    optionMoodInfo = "MoodInfo OFF";
                }
            }


            //
            // - - - Actions (third box)
            //

            //first button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 0 + button_height * 0, button_width, button_height), "Colour Palette"))
            {
                /*Debug.Log("[Dbg] Button -> Swap sides");
                if (GameManager.instance.inputScript.ModalState == ModalState.Normal)
                {
                    if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
                    { GameManager.instance.sideScript.PlayerSide = GameManager.instance.globalScript.sideAuthority; }
                    else { GameManager.instance.sideScript.PlayerSide = GameManager.instance.globalScript.sideResistance; }
                    //redraw nodes (player node shown/not shown depending on side)
                    GameManager.instance.nodeScript.RedrawNodes();
                }*/
                Debug.Log("[Dbg] Button -> Colour Palette");
                if (GameManager.instance.inputScript.ModalState == ModalState.Normal)
                {
                    //display outcome dialogue showing colour Palette
                    GameManager.instance.colourScript.DebugDisplayColourPalette();
                }
                
            }

            //second button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 1 + button_height * 1, button_width, button_height), "Toggle Orgs"))
            {
                Debug.Log("[Dbg] Button -> Toggle All Organisations (isContact)");
                GameManager.instance.orgScript.DebugToggleAllOrganisations();
            }

            /*//second button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 1 + button_height * 1, button_width, button_height), optionPlayerSide))
            {
                Debug.Log("[Dbg] Button -> Toggle Player Side");
                //resistance -> change to authority
                if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
                { GameManager.instance.sideScript.PlayerSide = GameManager.instance.globalScript.sideAuthority; optionPlayerSide = "AUTHORITY"; }
                //both -> change to resistance
                else if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
                { GameManager.instance.sideScript.PlayerSide = GameManager.instance.globalScript.sideResistance; optionPlayerSide = "RESISTANCE"; }
            }*/

            //third button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 2 + button_height * 2, button_width, button_height), "Remove Tracer"))
            {
                Debug.LogFormat("[Dbg] Button -> Toggle Remove Tracer at nodeID {0}{1}", GameManager.instance.nodeScript.nodePlayer, "\n");
                if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
                {
                    Node node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
                    if (node != null) { node.RemoveTracer(); } else { Debug.LogError("Invalid current Player node (Null)"); }
                }
            }

            //fourth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 3 + button_height * 3, button_width, button_height), "Release Player"))
            {
                Debug.Log("[Dbg] Button -> Release Player");
                if (GameManager.instance.playerScript.status == ActorStatus.Captured)
                { GameManager.instance.captureScript.ReleasePlayer(); }
            }

            //fifth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 4 + button_height * 4, button_width, button_height), "Remove Condition"))
            {
                //removes a condition from Player or an actor
                Debug.Log("[Dbg] Button -> Remove Condition");
                if (debugDisplay != 52)
                { debugDisplay = 52; }
                else { debugDisplay = 0; }
            }

            //sixth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 5 + button_height * 5, button_width, button_height), "Remove Ongoing"))
            {
                //removes connection Security ongoing effects (first entry in register dict)
                Debug.Log("[Dbg] Button -> Remove Ongoing");
                GameManager.instance.dataScript.DebugRemoveOngoingEffects();
            }



            //tenth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 6 + button_height * 6, button_width, button_height), "Give Gear"))
            {
                //Resistance player only
                if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
                {
                    Debug.Log("[Dbg] Button -> Give Gear");
                    if (debugDisplay != 16)
                    { debugDisplay = 16; }
                    else { debugDisplay = 0; }
                }
            }
            //eleventh button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 7 + button_height * 7, button_width, button_height), "Give Condition"))
            {
                Debug.Log("[Dbg] Button -> Give Condition");
                if (debugDisplay != 18)
                { debugDisplay = 18; }
                else { debugDisplay = 0; }
            }
            //twelfth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 8 + button_height * 8, button_width, button_height), "Give Trait"))
            {
                Debug.Log("[Dbg] Button -> Give Actor Trait");
                if (debugDisplay != 20)
                { debugDisplay = 20; }
                else { debugDisplay = 0; }
            }
            //thirteenth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 9 + button_height * 9, button_width, button_height), "Give Renown"))
            {
                Debug.Log("[Dbg] Button -> Give Player Renown");
                int renown = GameManager.instance.playerScript.Renown;
                renown += 10;
                GameManager.instance.playerScript.Renown = renown;
            }

            //thirteenth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 10 + button_height * 10, button_width, button_height), "Set Sec State"))
            {
                Debug.Log("[Dbg] Button -> Set Security State");
                if (debugDisplay != 22)
                { debugDisplay = 22; }
                else { debugDisplay = 0; }
            }

            //fourteenth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 11 + button_height * 11, button_width, button_height), "AISecProtocol +"))
            {
                Debug.Log("[Dbg] Button -> Increase AI Security Protocol");
                GameManager.instance.aiScript.IncreaseAISecurityProtocolLevel();
            }

            //seventh button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 12 + button_height * 12, button_width, button_height), optionAIOffline))
            {
                Debug.Log("[Dbg] Button -> Toggle AI Offline");
                if (GameManager.instance.aiScript.CheckAIOffLineStatus() == true)
                {
                    GameManager.instance.aiScript.SetAIOffline(false);
                    optionAIOffline = "AIOffline ON";
                }
                else
                {
                    GameManager.instance.aiScript.SetAIOffline(true);
                    optionAIOffline = "AIOffline OFF";
                }
            }

            //eigth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 13 + button_height * 13, button_width, button_height), optionAITraceback))
            {
                Debug.Log("[Dbg] Button -> Toggle AI TraceBack");
                if (GameManager.instance.aiScript.CheckAITraceBackStatus() == true)
                {
                    GameManager.instance.aiScript.SetAITraceBack(false);
                    optionAITraceback = "AITraceBack ON";
                }
                else
                {
                    GameManager.instance.aiScript.SetAITraceBack(true);
                    optionAITraceback = "AITraceback OFF";
                }
            }

            //ninth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 14 + button_height * 14, button_width, button_height), optionAIScreamer))
            {
                Debug.Log("[Dbg] Button -> Toggle AI Screamer");
                if (GameManager.instance.aiScript.CheckAIScreamerStatus() == true)
                {
                    GameManager.instance.aiScript.SetAIScreamer(false);
                    optionAIScreamer = "AIScreamer ON";
                }
                else
                {
                    GameManager.instance.aiScript.SetAIScreamer(true);
                    optionAIScreamer = "AIScreamer OFF";
                }
            }


            //thirteenth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 15 + button_height * 15, button_width, button_height), "AI Reboot"))
            {
                Debug.Log("[Dbg] Button -> Force AI Reboot");
                GameManager.instance.aiScript.RebootCommence();
            }

            //thirteenth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 16 + button_height * 16, button_width, button_height), "Give Secret"))
            {
                Debug.Log("[Dbg] Button -> Give Player a Random Secret");
                GameManager.instance.playerScript.DebugAddRandomSecret();
            }

            //fourteenth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 17 + button_height * 17, button_width, button_height), "Add Contact"))
            {
                Debug.Log("[Dbg] Button -> Add Contact");
                if (debugDisplay != 27)
                { debugDisplay = 27; }
                else { debugDisplay = 0; }
            }

            //fifteenth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 18 + button_height * 18, button_width, button_height), "Remove Contact"))
            {
                Debug.Log("[Dbg] Button -> Remove Contact");
                if (debugDisplay != 29)
                { debugDisplay = 29; }
                else { debugDisplay = 0; }
            }


            //sixteenth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 19 + button_height * 19, button_width, button_height), "isContactKnown"))
            {
                Debug.Log("[Dbg] Button -> Set isContactKnown True");
                if (debugDisplay != 31)
                { debugDisplay = 31; }
                else { debugDisplay = 0; }
            }

            //seventeenth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 20 + button_height * 20, button_width, button_height), "Control Nemesis"))
            {
                Debug.Log("[Dbg] Button -> Control Nemesis");
                if (debugDisplay != 41)
                { debugDisplay = 41; }
                else { debugDisplay = 0; }
            }

            //eightteenth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 21 + button_height * 21, button_width, button_height), "Help Debug"))
            {
                Debug.Log("[Dbg] Button -> Help Debug");
                //toggle help on/off
                if (GameManager.instance.tooltipHelpScript.CheckTooltipActive() == false)
                { GameManager.instance.helpScript.DebugShowHelp(); }
                else { GameManager.instance.tooltipHelpScript.CloseTooltip(); }
            }

            //nineteenth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 22 + button_height * 22, button_width, button_height), "Toggle Contact"))
            {
                Debug.Log("[Dbg] Button -> Toggle Contact Active/Inactive");
                if (debugDisplay != 47)
                { debugDisplay = 47; }
                else { debugDisplay = 0; }
            }

            //twentyfourth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 23 + button_height * 23, button_width, button_height), "Activate Cures"))
            {
                Debug.Log("[Dbg] Button -> Activate Cures");
                GameManager.instance.dataScript.DebugActivateAllCures();
            }

            //twentyFifth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 24 + button_height * 24, button_width, button_height), "Export Data"))
            {
                Debug.Log("[Dbg] Button -> Export Data");
                GameManager.instance.textScript.Export();
            }

            //twentySixth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 25 + button_height * 25, button_width, button_height), "Import Data"))
            {
                Debug.Log("[Dbg] Button -> Import Data");
                GameManager.instance.textScript.Import();
            }

            //
            // - - - Level Menu - - -
            //

            //first button
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * 0 + button_height * 0, button_width, button_height), "Most Connected"))
            {
                Debug.Log("[Dbg] Button -> Show Most Connected Nodes");
                EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.MostConnected, "DebugGUI.cs -> OnGUI");
            }

            //second button
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * 1 + button_height * 1, button_width, button_height), "Centre Nodes"))
            {
                Debug.Log("[Dbg] Button -> Centre Nodes");
                GameManager.instance.debugGraphicsScript.SetCentrePane(true);
                EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.Centre, "DebugGUI.cs -> OnGUI");
            }

            //third button
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * 2 + button_height * 2, button_width, button_height), "Near Neighbours"))
            {
                Debug.Log("[Dbg] Button -> NearNeighbours");
                EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NearNeighbours, "DebugGUI.cs -> OnGUI");
            }

            //fourth button
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * 3 + button_height * 3, button_width, button_height), "Decision Nodes"))
            {
                Debug.Log("[Dbg] Button -> Show Decision Nodes");
                EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.DecisionNodes, "DebugGUI.cs -> OnGUI");
            }

            //fifth button
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * 4 + button_height * 4, button_width, button_height), "Crisis Nodes"))
            {
                Debug.Log("[Dbg] Button -> Show Crisis Nodes");
                EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.CrisisNodes, "DebugGUI.cs -> OnGUI");
            }

            //sixth button
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * 5 + button_height * 5, button_width, button_height), "Show NodeID"))
            {
                Debug.Log("[Dbg] Button -> Show All NodeID's");
                GameManager.instance.nodeScript.ShowAllNodeID();
            }

            //seventh button
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * 6 + button_height * 6, button_width, button_height), optionPath))
            {
                Debug.LogFormat("[Dbg] Button -> {0}", optionPath);
                switch (status)
                {
                    case GUIStatus.None:
                        debugDisplay = 37;
                        break;
                    case GUIStatus.ShowPathOff:
                        debugDisplay = 39;
                        break;
                }
            }

            //eigth button
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * 7 + button_height * 7, button_width, button_height), "Recalc Weighted"))
            {
                Debug.Log("[Dbg] Button -> Recaculate Weighted Dijkstra data");
                GameManager.instance.dijkstraScript.RecalculateWeightedData();
            }

            //ninth button
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * 8 + button_height * 8, button_width, button_height), "Loiter Nodes"))
            {
                Debug.Log("[Dbg] Button -> Show Loiter Nodes");
                EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.LoiterNodes, "DebugGUI.cs -> OnGUI");
            }

            //tenth button
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * 9 + button_height * 9, button_width, button_height), "Cure Nodes"))
            {
                Debug.Log("[Dbg] Button -> Show Cure Nodes");
                EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.CureNodes, "DebugGUI.cs -> OnGUI");
            }

            //
            // - - - Analysis at Right Hand side of Screen - - -
            //

            if (debugDisplay > 0)
            {

                switch (debugDisplay)
                {
                    //City / Level analysis
                    case 1:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DebugLevelAnalysis();
                        GUI.Box(new Rect(Screen.width - 305, 10, 300, 500), analysis, customBackground);
                        break;
                    //toggle team data
                    case 2:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        /*analysis = GameManager.instance.teamScript.DisplayTeamAnalysis();
                        GUI.Box(new Rect(Screen.width - 205, 10, 200, 240), analysis, customBackground);*/
                        switch (teamStatus)
                        {
                            case TeamDebug.Pools:
                                analysis = GameManager.instance.teamScript.DebugDisplayTeamAnalysis();
                                GUI.Box(new Rect(Screen.width - 205, 10, 200, 240), analysis, customBackground);
                                break;
                            case TeamDebug.Roster:
                                analysis = GameManager.instance.teamScript.DebugDisplayIndividualTeams();
                                GUI.Box(new Rect(Screen.width - 405, 10, 400, 320), analysis, customBackground);
                                break;
                            case TeamDebug.Actors:
                                analysis = GameManager.instance.teamScript.DebugDisplayTeamActorAnalysis();
                                GUI.Box(new Rect(Screen.width - 205, 10, 200, 280), analysis, customBackground);
                                break;
                        }
                        break;
                    //AI Display ON
                    case 3:
                        break;
                    //AI Display OFF
                    case 4:
                        break;
                    //Game state
                    case 5:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.inputScript.DebugDisplayGameState();
                        GUI.Box(new Rect(Screen.width - 255, 10, 250, 650), analysis, customBackground);
                        break;
                    //actor Pools
                    case 6:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.actorScript.DebugDisplayPools();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 900), analysis, customBackground);
                        break;
                    //actor Lists
                    case 7:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DebugDisplayActorLists();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 500), analysis, customBackground);
                        break;
                    //Player stats
                    case 8:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.playerScript.DebugDisplayPlayerStats();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 600), analysis, customBackground);
                        break;
                    //Toggle Messages
                    case 9:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = "Unknown";
                        switch (msgStatus)
                        {
                            case MessageCategory.Pending:
                                analysis = GameManager.instance.dataScript.DebugDisplayMessages(MessageCategory.Pending);
                                break;
                            case MessageCategory.Current:
                                analysis = GameManager.instance.dataScript.DebugDisplayMessages(MessageCategory.Current);
                                break;
                            case MessageCategory.Archive:
                                analysis = GameManager.instance.dataScript.DebugDisplayMessages(MessageCategory.Archive);
                                break;
                            case MessageCategory.AI:
                                analysis = GameManager.instance.dataScript.DebugDisplayMessages(MessageCategory.AI);
                                break;
                        }
                        GUI.Box(new Rect(Screen.width - 460, 10, 450, 1000), analysis, customBackground);
                        break;
                    //Toggle Authority AI Data
                    case 10:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = "Unknown";
                        switch (aiStatus)
                        {
                            case AIDebugData.Task:
                                analysis = GameManager.instance.aiScript.DisplayTaskData();
                                break;
                            case AIDebugData.Node:
                                analysis = GameManager.instance.aiScript.DisplayNodeData();
                                break;
                            case AIDebugData.Spider:
                                analysis = GameManager.instance.aiScript.DisplaySpiderData();
                                break;
                            case AIDebugData.Erasure:
                                analysis = GameManager.instance.aiScript.DisplayErasureData();
                                break;
                            case AIDebugData.Decision:
                                analysis = GameManager.instance.aiScript.DisplayDecisionData();
                                break;
                        }
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 600), analysis, customBackground);
                        break;
                    //Factions
                    case 11:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.factionScript.DebugDisplayFactions();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 500), analysis, customBackground);
                        break;
                    //Show Options
                    case 12:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.optionScript.DisplayOptions();
                        GUI.Box(new Rect(Screen.width - 460, 10, 450, 300), analysis, customBackground);
                        break;
                    //Help
                    case 13:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.helpScript.DisplayHelp();
                        GUI.Box(new Rect(Screen.width - 305, 10, 300, 800), analysis, customBackground);
                        break;
                    //Ongoing Register
                    case 14:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DebugDisplayOngoingRegister();
                        GUI.Box(new Rect(Screen.width - 460, 10, 450, 350), analysis, customBackground);
                        break;
                    //Actions Adjustment Register
                    case 15:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DebugDisplayActionsRegister();
                        GUI.Box(new Rect(Screen.width - 460, 10, 450, 350), analysis, customBackground);
                        break;
                    //Give Gear Input
                    case 16:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 100, 50, 200, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 75, 55, 150, 20), "Input Gear.name (exact)");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 50, 90, 100, 20), textInput_0);
                        status = GUIStatus.GiveGear;
                        textOutput = null;
                        break;
                    //Give Gear processing & Output
                    case 17:
                        if (textOutput == null)
                        { textOutput = GameManager.instance.playerScript.DebugAddGear(textInput_0); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 175, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //Give Condition
                    case 18:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 400, 50, 200, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 395, 55, 190, 20), "Input Condition name (lwr case)");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 350, 75, 100, 20), textInput_0);
                        GUI.Label(new Rect(Screen.width / 2 - 375, 100, 150, 20), "Input Actor (0 - 3 or p)");
                        textInput_1 = GUI.TextField(new Rect(Screen.width / 2 - 350, 120, 100, 20), textInput_1);
                        status = GUIStatus.GiveCondition;
                        textOutput = null;
                        break;
                    //Give Condition processing and Output
                    case 19:
                        if (textOutput == null)
                        { textOutput = GameManager.instance.actorScript.DebugAddCondition(textInput_0, textInput_1); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 475, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //Give Trait to Actor
                    case 20:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 400, 50, 250, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 395, 55, 240, 20), "Input Trait name (Case sensistive)");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 350, 75, 100, 20), textInput_0);
                        GUI.Label(new Rect(Screen.width / 2 - 375, 100, 150, 20), "Input Actor (0 - 3)");
                        textInput_1 = GUI.TextField(new Rect(Screen.width / 2 - 350, 120, 100, 20), textInput_1);
                        status = GUIStatus.GiveActorTrait;
                        textOutput = null;
                        break;
                    //Give Trait to Actor processing and Output
                    case 21:
                        if (textOutput == null)
                        { textOutput = GameManager.instance.actorScript.DebugAddTrait(textInput_0, textInput_1); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 475, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //Set State
                    case 22:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 400, 50, 200, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 395, 55, 190, 20), "category (a / r)");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 350, 75, 100, 20), textInput_0);
                        GUI.Label(new Rect(Screen.width / 2 - 375, 100, 170, 20), "State (a -> apb/sec/sur/nor)");
                        textInput_1 = GUI.TextField(new Rect(Screen.width / 2 - 350, 120, 100, 20), textInput_1);
                        status = GUIStatus.SetState;
                        textOutput = null;
                        break;
                    //Assign state
                    case 23:
                        if (textOutput == null)
                        { textOutput = GameManager.instance.turnScript.DebugSetState(textInput_0, textInput_1); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 475, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //Gear Info Display
                    case 24:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DebugDisplayGearData();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 750), analysis, customBackground);
                        break;
                    //Secret Info Display
                    case 25:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.secretScript.DebugDisplaySecretData();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 750), analysis, customBackground);
                        break;
                    //Node Crisis Data Display
                    case 26:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DebugDisplayCrisisNodes();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 400), analysis, customBackground);
                        break;
                    //Add Contact to Actor
                    case 27:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 400, 50, 250, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 395, 55, 240, 20), "Input Contact NodeID");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 350, 75, 100, 20), textInput_0);
                        GUI.Label(new Rect(Screen.width / 2 - 375, 100, 150, 20), "Input Actor (0 - 3)");
                        textInput_1 = GUI.TextField(new Rect(Screen.width / 2 - 350, 120, 100, 20), textInput_1);
                        status = GUIStatus.AddContact;
                        textOutput = null;
                        break;
                    //Add Contact to Actor processing and Output
                    case 28:
                        if (textOutput == null)
                        { textOutput = GameManager.instance.dataScript.DebugAddContact(textInput_0, textInput_1); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 475, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //Remove Actor Contact
                    case 29:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 400, 50, 250, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 395, 55, 240, 20), "Input Contact NodeID");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 350, 75, 100, 20), textInput_0);
                        GUI.Label(new Rect(Screen.width / 2 - 375, 100, 150, 20), "Input Actor (0 - 3)");
                        textInput_1 = GUI.TextField(new Rect(Screen.width / 2 - 350, 120, 100, 20), textInput_1);
                        status = GUIStatus.RemoveContact;
                        textOutput = null;
                        break;
                    //Remove Actor Contact processing and Output
                    case 30:
                        if (textOutput == null)
                        { textOutput = GameManager.instance.dataScript.DebugRemoveContact(textInput_0, textInput_1); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 475, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //Toggle isContactKnown for a node
                    case 31:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 400, 50, 250, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 395, 55, 240, 20), "Input Contact NodeID");
                        GUI.Label(new Rect(Screen.width / 2 - 395, 100, 240, 35), "node.isContactKnown will be toggled On/Off");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 350, 75, 100, 20), textInput_0);
                        status = GUIStatus.isKnownContact;
                        textOutput = null;
                        break;
                    //toggle isContactKnown processing
                    case 32:
                        if (textOutput == null)
                        { textOutput = GameManager.instance.contactScript.DebugToggleIsContactKnown(textInput_0); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 475, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //Contacts -> Resistance
                    case 33:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.contactScript.DebugDisplayContacts();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 800), analysis, customBackground);
                        break;
                    //Targets Generic
                    case 34:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DebugShowGenericTargets();
                        GUI.Box(new Rect(Screen.width - 555, 10, 550, 600), analysis, customBackground);
                        break;
                    //Target Pools
                    case 35:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        /*analysis = GameManager.instance.dataScript.DebugShowGenericTargets();*/
                        analysis = GameManager.instance.dataScript.DebugShowTargetPools();
                        GUI.Box(new Rect(Screen.width - 555, 10, 550, 600), analysis, customBackground);
                        break;
                    //Target Dictionary
                    case 36:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        /*analysis = GameManager.instance.dataScript.DebugShowGenericTargets();*/
                        analysis = GameManager.instance.dataScript.DebugShowTargetDict();
                        GUI.Box(new Rect(Screen.width - 555, 10, 550, 800), analysis, customBackground);
                        break;
                    //Show / Toggle Path (between two nodes)
                    case 37:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 400, 50, 250, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 395, 55, 240, 20), "Input Source nodeID");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 350, 75, 100, 20), textInput_0);
                        GUI.Label(new Rect(Screen.width / 2 - 375, 100, 150, 20), "Input Destination nodeID");
                        textInput_1 = GUI.TextField(new Rect(Screen.width / 2 - 350, 120, 100, 20), textInput_1);
                        textOutput = null;
                        optionPath = "Show Path";
                        status = GUIStatus.ShowPath;
                        break;
                    //Show Path processing and Output
                    case 38:
                        if (textOutput == null)
                        {
                            optionPath = "Path OFF";
                            status = GUIStatus.ShowPathOff;
                            textOutput = GameManager.instance.dijkstraScript.DebugShowPath(Convert.ToInt32(textInput_0), Convert.ToInt32(textInput_1), true);
                        }
                        break;
                    //Swith Path OFF
                    case 39:
                        EventManager.instance.PostNotification(EventType.FlashMultipleConnectionsStop, this, "DebugGUI.cs -> OnGUI");
                        status = GUIStatus.None;
                        optionPath = "Input Path";
                        break;
                    //Nemesis Data
                    case 40:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.nemesisScript.DebugShowNemesisStatus();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 600), analysis, customBackground);
                        break;
                    //Control Nemesis -> Input
                    case 41:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 400, 50, 250, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 395, 55, 240, 20), "Nemesis Target nodeID");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 350, 75, 100, 20), textInput_0);
                        GUI.Label(new Rect(Screen.width / 2 - 375, 100, 150, 20), "Input Goal (ambush/search)");
                        textInput_1 = GUI.TextField(new Rect(Screen.width / 2 - 350, 120, 100, 20), textInput_1);
                        status = GUIStatus.NemesisControl;
                        textOutput = null;
                        break;
                    //Control Nemesis -> Confirmation
                    case 42:
                        if (doOnceFlag == false)
                        {
                            doOnceFlag = true;
                            int nodeID = -1;
                            try
                            { nodeID = Convert.ToInt32(textInput_0); }
                            catch (FormatException) { Debug.LogWarningFormat("Invalid nodeID {0} input (Not a Number)", textInput_0); }
                            NemesisGoal goal = NemesisGoal.SEARCH;
                            switch (textInput_1)
                            {
                                case "Ambush":
                                case "ambush":
                                case "AMBUSH":
                                    goal = NemesisGoal.AMBUSH;
                                    break;
                            }
                            if (nodeID > -1)
                            {  GameManager.instance.nemesisScript.SetPlayerControlStart(nodeID, goal); }
                            status = GUIStatus.None;
                        }
                        if (textOutput == null)
                        { textOutput = string.Format("Nemesis Player Control, nodeID {0}, goal {1}{2}Press ESC to Exit", textInput_0, textInput_1, "\n"); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 475, 100, 350, 40), textOutput, customBackground);
                        break;
                    //Rebel AI data
                    case 43:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.aiRebelScript.DebugShowRebelAIStatus();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 750), analysis, customBackground);
                        break;
                    //Rebel Tracker data
                    case 44:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DebugShowRebelMoves();
                        GUI.Box(new Rect(Screen.width - 305, 10, 300, 600), analysis, customBackground);
                        break;
                    //Nemesis Tracker data
                    case 45:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DebugShowNemesisMoves();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 600), analysis, customBackground);
                        break;
                    //Contacts by Node data
                    case 46:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.contactScript.DebugDisplayContactsByNode();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 600), analysis, customBackground);
                        break;
                    //Toggle Contact Active / Inactive
                    case 47:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 400, 50, 250, 50), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 395, 55, 240, 20), "Input contactID");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 350, 75, 100, 20), textInput_0);
                        status = GUIStatus.ContactToggle;
                        textOutput = null;
                        break;
                    case 48:
                        if (doOnceFlag == false)
                        {
                            doOnceFlag = true;
                            int contactID = -1;
                            try
                            { contactID = Convert.ToInt32(textInput_0); }
                            catch (FormatException) { Debug.LogWarningFormat("Invalid nodeID {0} input (Not a Number)", textInput_0); }
                            if (contactID > -1)
                            { analysis = GameManager.instance.dataScript.ContactToggleActive(contactID); }
                            status = GUIStatus.None;
                        }
                        if (textOutput == null)
                        { textOutput = string.Format("Toggle Contact, ID {0}, status {1}{2}Press ESC to Exit", textInput_0, analysis, "\n"); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 475, 100, 350, 40), textOutput, customBackground);
                        break;
                    //Contact dict
                    case 49:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.contactScript.DebugDisplayContactsDict();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 800), analysis, customBackground);
                        break;
                    //Statistics Data
                    case 50:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.statScript.DebugShowStatistics();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 600), analysis, customBackground);
                        break;
                    //AI Analysis Data
                    case 51:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = string.Format("{0}{1}", GameManager.instance.aiScript.DebugShowTaskAnalysis(), GameManager.instance.aiRebelScript.DebugShowTaskAnalysis());
                        GUI.Box(new Rect(Screen.width - 355, 10, 500, 350), analysis, customBackground);
                        break;
                    //Remove Condition
                    case 52:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 400, 50, 200, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 395, 55, 190, 20), "Input Condition name (lwr case)");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 350, 75, 100, 20), textInput_0);
                        GUI.Label(new Rect(Screen.width / 2 - 375, 100, 150, 20), "Input Actor (0 - 3 or p)");
                        textInput_1 = GUI.TextField(new Rect(Screen.width / 2 - 350, 120, 100, 20), textInput_1);
                        status = GUIStatus.RemoveCondition;
                        textOutput = null;
                        break;
                    //Remove Condition processing and Output
                    case 53:
                        if (textOutput == null)
                        { textOutput = GameManager.instance.actorScript.DebugRemoveCondition(textInput_0, textInput_1); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 475, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //actor dictionary
                    case 54:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DebugDisplayActorDict();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 850), analysis, customBackground);
                        break;
                    //campaign data
                    case 55:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.campaignScript.DebugDisplayCampaignData();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 600), analysis, customBackground);
                        break;
                    //scenario data
                    case 56:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.campaignScript.DebugDisplayScenarioData();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 600), analysis, customBackground);
                        break;
                    //Personality data (OnMap actors)
                    case 57:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.personScript.DebugDisplayAllPersonalities();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 800), analysis, customBackground);
                        break;
                    //Actor Compatibility Range
                    case 58:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.personScript.DebugCheckActorCompatibilityRange();
                        GUI.Box(new Rect(Screen.width - 405, 10, 500, 800), analysis, customBackground);
                        break;
                    //Actor Emotional History
                    case 59:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.personScript.DebugDisplayActorMotivationHistory();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 800), analysis, customBackground);
                        break;
                    //player stats -> mood history
                    case 60:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.playerScript.DebugDisplayMoodHistory();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 600), analysis, customBackground);
                        break;
                    //Personality -> Player Preferences
                    case 61:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.personScript.DebugDisplayPlayerLikes();
                        GUI.Box(new Rect(Screen.width - 455, 10, 455, 800), analysis, customBackground);
                        break;
                    //Topic Data
                    case 62:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.topicScript.DebugDisplayTopicTypes();
                        GUI.Box(new Rect(Screen.width - 455, 10, 450, 600), analysis, customBackground);
                        break;
                    //Topic Type lists
                    case 63:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.topicScript.DebugDisplayTopicTypeLists();
                        GUI.Box(new Rect(Screen.width - 455, 10, 450, 850), analysis, customBackground);
                        break;
                    //Topic Pools
                    case 64:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.topicScript.DebugDisplayCurrentTopicPool();
                        GUI.Box(new Rect(Screen.width - 455, 10, 450, 850), analysis, customBackground);
                        break;
                    //Topic Selection data
                    case 65:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.topicScript.DebugDisplayTopicSelectionData();
                        GUI.Box(new Rect(Screen.width - 455, 10, 450, 600), analysis, customBackground);
                        break;
                    //Topic Profile data
                    case 66:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.topicScript.DebugDisplayTopicProfileData();
                        GUI.Box(new Rect(Screen.width - 455, 10, 450, 850), analysis, customBackground);
                        break;
                    //Topic Criteria data
                    case 67:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.topicScript.DebugDisplayCriteria();
                        GUI.Box(new Rect(Screen.width - 455, 10, 450, 850), analysis, customBackground);
                        break;
                    //Actor NodeActionData
                    case 68:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DebugDisplayActorNodeActionData();
                        GUI.Box(new Rect(Screen.width - 455, 10, 450, 800), analysis, customBackground);
                        break;
                    //Actor TeamActionData
                    case 69:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DebugDisplayActorTeamActionData();
                        GUI.Box(new Rect(Screen.width - 455, 10, 450, 800), analysis, customBackground);
                        break;
                    //Personality data (HQ actors)
                    case 70:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.personScript.DebugDisplayHQPersonalities();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 800), analysis, customBackground);
                        break;
                    //Factions
                    case 71:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.factionScript.DebugDisplayHQActors();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 700), analysis, customBackground);
                        break;
                    //News Items
                    case 72:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.newsScript.DebugDisplayNewsItems();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 800), analysis, customBackground);
                        break;
                    //Adverts
                    case 73:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.newsScript.DebugDisplayAdverts();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 800), analysis, customBackground);
                        break;
                    //HQ dictionary
                    case 74:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DebugDisplayHQDict();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 850), analysis, customBackground);
                        break;
                    //belief frequency analysis
                    case 75:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DebugDisplayBeliefCount();
                        GUI.Box(new Rect(Screen.width - 355, 10, 500, 300), analysis, customBackground);
                        break;
                    //topic and topicOption text tag frequency analysis
                    case 76:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DebugDisplayTextTagCount();
                        GUI.Box(new Rect(Screen.width - 355, 10, 500, 800), analysis, customBackground);
                        break;
                    //Statistics Data
                    case 77:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.statScript.DebugShowRatios();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 600), analysis, customBackground);
                        break;
                    //Organisation Data
                    case 78:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DebugDisplayCurrentOrganisations();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 600), analysis, customBackground);
                        break;
                }
            }
            else { status = GUIStatus.None; }
        }

        //User input
        Event e = Event.current;
        switch (e.keyCode)
        {
            case KeyCode.Return:
                switch (status)
                {
                    case GUIStatus.GiveGear:
                        debugDisplay = 17;
                        break;
                    case GUIStatus.GiveCondition:
                        debugDisplay = 19;
                        break;
                    case GUIStatus.RemoveCondition:
                        debugDisplay = 53;
                        break;
                    case GUIStatus.GiveActorTrait:
                        debugDisplay = 21;
                        break;
                    case GUIStatus.SetState:
                        debugDisplay = 23;
                        break;
                    case GUIStatus.AddContact:
                        debugDisplay = 28;
                        break;
                    case GUIStatus.RemoveContact:
                        debugDisplay = 30;
                        break;
                    case GUIStatus.isKnownContact:
                        debugDisplay = 32;
                        break;
                    case GUIStatus.ShowPath:
                        debugDisplay = 38;
                        break;
                    case GUIStatus.NemesisControl:
                        debugDisplay = 42;
                        break;
                    case GUIStatus.ContactToggle:
                        debugDisplay = 48;
                        break;
                }
                break;
            case KeyCode.Delete:
                //used to provide input control on text fields
                textInput_0 = "";
                break;
            case KeyCode.Escape:
                debugDisplay = 0;
                break;
        }
        e.keyCode = KeyCode.None;
    }


}
