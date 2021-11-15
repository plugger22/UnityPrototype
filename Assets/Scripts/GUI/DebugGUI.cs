using gameAPI;
using System;
using UnityEngine;

/// <summary>
/// debug GUI, on demand (HotKey 'D' -> On, HotKey 'H' -> Off)
/// </summary>
public class DebugGUI : MonoBehaviour
{
    //for whenever interaction is needed
    private enum GUIStatus
    {
        None, GiveGear, GiveCondition, RemoveCondition, GiveActorTrait, SetState, AddContact, RemoveContact, isKnownContact, ShowPath, ShowPathOff, NemesisControl, ContactToggle, Conflict,
        GiveCaptureTool, SetInnocence, SetMood, SetFriend, SetEnemy, SetLoyalty, SetTraitor
    }

    public GUIStyle customBackground;

    private GUIStatus status;
    private MessageCategory msgStatus;
    private AIDebugData aiStatus;
    private TeamDebug teamStatus;
    private DebugRegister registerStatus;
    public bool showGUI = false;
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
    private int box_file;
    private int button_width;
    private int offset;
    private int helpToggle = 0;
    private int targetToggle = 0;
    private int campaignToggle = 0;
    private int personalityToggle = 0;
    private int playerToggle = 0;
    private int contactToggle = 0;
    private int trackerToggle = 0;
    private int messageToggle = 0;
    private int newsToggle = 0;
    private int gearToggle = 0;
    private int actorToggle = 0;
    private int topicToggle = 0;
    private int secretToggle = 0;
    private int showContactToggle = 0;
    private int storyToggle = 0;
    private int orgToggle = 0;
    private int hqToggle = 0;
    private int showNodesToggle = 0;
    private int nodeDisplayToggle = 0;
    private int tileDisplayToggle = 0;
    private int analyseToggle = 0;
    private int statisticsToggle = 0;
    private string textInput_0 = "what";
    private string textInput_1 = "who";
    private string analysis = "Unknown";
    private string textOutput;

    public string optionShowContacts;
    public string optionConnectorTooltips;
    public string optionDebugData;
    public string optionNodes;
    public string optionNodeDisplay;
    public string optionTileDisplay;
    public string optionPowerUI;
    public string optionPath;
    public string optionContacts;
    public string optionMoodInfo;
    public string optionBillboardInfo;
    public string optionAutoGear;
    //Strings that can be changed by GameManager.cs -> InitialiseFeatures
    public string optionFogOfWar;
    public string optionNoAI;
    public string optionSubordinates;
    public string optionDecisions;
    public string optionMainInfoApp;
    public string optionReviews;
    public string optionNemesis;
    public string optionNPC;
    public string optionObjectives;
    public string optionOrganisations;
    public string optionTargets;
    public string optionGear;
    public string optionRecruit;
    public string optionMoveSecurity;
    public string optionActions;

    private void Awake()
    {
        button_width = box_width - (2 * offset_x);
        box_option = box_x;
        box_info = box_x * 2 + box_width;
        box_action = box_x * 3 + box_width * 2;
        box_level = box_x * 4 + box_width * 3;
        box_file = box_x * 5 + box_width * 4;
        //option strings
        optionAutoGear = "Auto Gear ON";
        optionConnectorTooltips = "Conn tooltips ON";
        optionDebugData = "Debug Data ON";
        optionNodes = "Show IDs";
        /*optionAIOffline = "AIOffline ON";*/
        /*optionAITraceback = "AITraceback ON";*/
        /*optionAIScreamer = "AIScreamer ON";*/
        optionPowerUI = "Compatibility ON";
        optionPath = "Input Path";
        optionContacts = "Contacts ON";
        optionShowContacts = "Show Contacts";
        optionMoodInfo = "Mood Info ON";
        optionBillboardInfo = "Billboard OFF";
        optionNodeDisplay = "Show NODES";
        optionTileDisplay = "Show SURV Tiles";
        optionSubordinates = "Subordinates OFF";
        optionFogOfWar = "Fog of War ON";
        optionNoAI = "AI OFF";
        optionDecisions = "Decisions OFF";
        optionMainInfoApp = "InfoApp OFF";
        optionReviews = "Reviews OFF";
        optionNemesis = "Nemesis OFF";
        optionNPC = "NPC OFF";
        optionObjectives = "Objectives OFF";
        optionOrganisations = "Organisations OFF";
        optionTargets = "Targets OFF";
        optionGear = "Gear OFF";
        optionRecruit = "Recruiting OFF";
        optionMoveSecurity = "Move Sec OFF";
        optionActions = "Actions OFF";
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
            GUI.Box(new Rect(box_option, box_y, box_width, box_height + 340), "Option Menu", customBackground);
            //background box (Info)
            GUI.Box(new Rect(box_info, box_y, box_width, box_height + 340), "Info Menu", customBackground);
            //background box (Actions)
            GUI.Box(new Rect(box_action, box_y, box_width, box_height + 340), "Action Menu", customBackground);
            //background box (Map)
            GUI.Box(new Rect(box_level, box_y, box_width, box_height / 2 + 130), "Map Menu", customBackground);
            //background box (File Ops)
            GUI.Box(new Rect(box_file, box_y, box_width, box_height / 2 + 60), "File Menu", customBackground);
            //
            // - - - Info (first box)
            //

            //first button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 0 + button_height * 0, button_width, button_height), "Help Data"))
            {
                Debug.Log("[Dbg] Button -> Toggle Help Data");
                switch (helpToggle)
                {
                    case 0: debugDisplay = 13; helpToggle = 1; break;
                    case 1: debugDisplay = 120; helpToggle = 2; break;
                    case 2: debugDisplay = 0; helpToggle = 0; break;
                }
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
                switch (campaignToggle)
                {
                    case 0: debugDisplay = 55; campaignToggle = 1; break;
                    case 1: debugDisplay = 107; campaignToggle = 2; break;
                    case 2: debugDisplay = 0; campaignToggle = 0; break;
                }
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
                    case 6: debugDisplay = 80; actorToggle = 7; break;
                    case 7: debugDisplay = 91; actorToggle = 8; break;
                    case 8: debugDisplay = 100; actorToggle = 9; break;
                    case 9: debugDisplay = 0; actorToggle = 0; break;
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
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 7 + button_height * 7, button_width, button_height), "HQ Data"))
            {
                Debug.Log("[Dbg] Button -> Toggle HQ Data");
                if (debugDisplay != 11)
                { debugDisplay = 11; }
                else { debugDisplay = 0; }

                switch (hqToggle)
                {
                    case 0: debugDisplay = 11; hqToggle = 1; break;
                    case 1: debugDisplay = 71; hqToggle = 2; break;
                    case 2: debugDisplay = 99; hqToggle = 3; break;
                    case 3: debugDisplay = 102; hqToggle = 4; break;
                    case 4: debugDisplay = 0; hqToggle = 0; break;
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
                    case 2: debugDisplay = 83; playerToggle = 3; break;
                    case 3: debugDisplay = 96; playerToggle = 4; break;
                    case 4: debugDisplay = 101; playerToggle = 5; break;
                    case 5: debugDisplay = 0; playerToggle = 0; break;
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
                        if (GameManager.i.sideScript.authorityOverall == SideState.Human)
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
                switch (messageToggle)
                {
                    case 0: debugDisplay = 9; messageToggle = 1; break;
                    case 1: debugDisplay = 9; messageToggle = 2; break;
                    case 2: debugDisplay = 9; messageToggle = 3; break;
                    case 3: debugDisplay = 9; messageToggle = 4; break;
                    case 4: debugDisplay = 9; messageToggle = 5; break;
                    case 5: debugDisplay = 0; messageToggle = 0; break;
                }
            }

            //fourteenth button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 13 + button_height * 13, button_width, button_height), "Gear Data"))
            {
                Debug.Log("[Dbg] Button -> Gear Data");
                switch (gearToggle)
                {
                    case 0: debugDisplay = 24; gearToggle = 1; break;
                    case 1: debugDisplay = 108; gearToggle = 2; break;
                    case 2: debugDisplay = 0; gearToggle = 0; break;
                }
            }

            //fifteenth button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 14 + button_height * 14, button_width, button_height), "Secret Data"))
            {
                Debug.Log("[Dbg] Button -> Secret Data");
                switch (secretToggle)
                {
                    case 0: debugDisplay = 25; secretToggle = 1; break;
                    case 1: debugDisplay = 106; secretToggle = 2; break;
                    case 2: debugDisplay = 0; secretToggle = 0; break;
                }
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
                    case 2: debugDisplay = 116; contactToggle = 3; break;
                    case 3: debugDisplay = 49; contactToggle = 4; break;
                    case 4: debugDisplay = 0; contactToggle = 0; break;
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
                    case 3: debugDisplay = 119; targetToggle = 4; break;
                    case 4: debugDisplay = 0; targetToggle = 0; break;
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
                    case 5: debugDisplay = 90; personalityToggle = 6; break;
                    case 6: debugDisplay = 0; personalityToggle = 0; break;
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
                    case 2: debugDisplay = 79; trackerToggle = 3; break;
                    case 3: debugDisplay = 0; trackerToggle = 0; break;
                }
            }

            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 24 + button_height * 24, button_width, button_height), "News Data"))
            {
                Debug.Log("[Dbg] Button -> News Data");
                switch (newsToggle)
                {
                    case 0: debugDisplay = 72; newsToggle = 1; break;
                    case 1: debugDisplay = 73; newsToggle = 2; break;
                    case 2: debugDisplay = 114; newsToggle = 3; break;
                    case 3: debugDisplay = 117; newsToggle = 4; break;
                    case 4: debugDisplay = 118; newsToggle = 5; break;
                    case 5: debugDisplay = 0; newsToggle = 0; break;
                        
                }
            }


            //twentySeventh button
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * 25 + button_height * 25, button_width, button_height), "Org & Corp Data"))
            {
                Debug.Log("[Dbg] Button -> Organisation and MegaCorp Data");
                switch (orgToggle)
                {
                    case 0: debugDisplay = 78; orgToggle = 1; break;
                    case 1: debugDisplay = 111; orgToggle = 2; break;
                    case 2: debugDisplay = 0; orgToggle = 0; break;
                }
            }

            //twentyEigth button
            offset = 26;
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "MetaGame Data"))
            {
                Debug.Log("[Dbg] Button -> MetaGame Data");
                if (debugDisplay != 105)
                { debugDisplay = 105; }
                else { debugDisplay = 0; }
            }

            //twentyNinth button
            offset = 27;
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Story Data"))
            {
                Debug.Log("[Dbg] Button -> Story Data");
                switch (storyToggle)
                {
                    case 0: debugDisplay = 109; storyToggle = 1; break;
                    case 1: debugDisplay = 110; storyToggle = 2; break;
                    case 2: debugDisplay = 112; storyToggle = 3; break;
                    case 3: debugDisplay = 113; storyToggle = 4; break;
                    case 4: debugDisplay = 0; storyToggle = 0; break;
                }
            }

            //thirtieth button
            offset = 28;
            if (GUI.Button(new Rect(box_info + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Tutorial Data"))
            {
                Debug.Log("[Dbg] Button -> Tutorial Data");
                if (debugDisplay != 115)
                { debugDisplay = 115; }
                else { debugDisplay = 0; }
            }

            //
            // - - - Options Menu
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
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 1 + button_height * 1, button_width, button_height), "Colour Palette"))
            {
                Debug.Log("[Dbg] Button -> Colour Palette");
                if (GameManager.i.inputScript.ModalState == ModalState.Normal)
                {
                    //display outcome dialogue showing colour Palette
                    GameManager.i.colourScript.DebugDisplayColourPalette();
                }
            }

            //second button
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 2 + button_height * 2, button_width, button_height), optionAutoGear))
            {
                Debug.Log("[Dbg] Button -> Toggle OptionAutoGear");
                if (GameManager.i.optionScript.autoGearResolution == true)
                {
                    GameManager.i.optionScript.autoGearResolution = false;
                    optionAutoGear = "Auto Gear ON";
                }
                else
                {
                    GameManager.i.optionScript.autoGearResolution = true;
                    optionAutoGear = "Auto Gear OFF";
                }
            }

            //third button
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 3 + button_height * 3, button_width, button_height), optionFogOfWar))
            {
                Debug.Log("[Dbg] Button -> Toggle Fog Of War");
                if (GameManager.i.optionScript.isFogOfWar == true)
                {
                    //sets both FOW and Nemesis settings in tandem
                    GameManager.i.optionScript.isFogOfWar = false;
                    GameManager.i.nemesisScript.isShown = true;
                    optionFogOfWar = "Fog Of War ON";
                }
                else
                {
                    GameManager.i.optionScript.isFogOfWar = true;
                    GameManager.i.nemesisScript.isShown = false;
                    optionFogOfWar = "Fog Of War OFF";
                }
                //redraw and update nodes
                GameManager.i.nodeScript.NodeRedraw = true;
            }

            //fourth button
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 4 + button_height * 4, button_width, button_height), optionConnectorTooltips))
            {
                Debug.Log("[Dbg] Button -> Toggle Connection Tooltips");
                if (GameManager.i.optionScript.connectorTooltips == true)
                {
                    GameManager.i.optionScript.connectorTooltips = false;
                    optionConnectorTooltips = "Conn Tooltips ON";
                }
                else
                {
                    GameManager.i.optionScript.connectorTooltips = true;
                    optionConnectorTooltips = "Conn Tooltips OFF";
                }
            }

            //fifth button
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 5 + button_height * 5, button_width, button_height), optionDebugData))
            {
                Debug.Log("[Dbg] Button -> Toggle Debug Data");
                if (GameManager.i.optionScript.debugData == true)
                {
                    GameManager.i.optionScript.debugData = false;
                    optionDebugData = "Debug Data ON";
                }
                else
                {
                    GameManager.i.optionScript.debugData = true;
                    optionDebugData = "Debug Data OFF";
                }
            }

            //sixth button
            if (GameManager.i.turnScript.Turn > 0) { optionNoAI = "Unavailable"; }
            {
                if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 6 + button_height * 6, button_width, button_height), optionNoAI))
                {
                    Debug.Log("[Dbg] Button -> Toggle NO AI");
                    //option only available on first turn
                    if (GameManager.i.turnScript.Turn == 0)
                    {

                        switch (GameManager.i.sideScript.PlayerSide.level)
                        {
                            //authority player
                            case 1:
                                //switch AI Off -> Manual player control for both sides
                                if (GameManager.i.sideScript.resistanceOverall == SideState.AI)
                                {
                                    optionNoAI = "AI OFF";
                                    GameManager.i.optionScript.isAI = false;
                                    GameManager.i.sideScript.authorityCurrent = SideState.Human;
                                    GameManager.i.sideScript.resistanceCurrent = SideState.Human;
                                    GameManager.i.sideScript.authorityOverall = SideState.Human;
                                    GameManager.i.sideScript.resistanceOverall = SideState.Human;
                                    //notification
                                    GameManager.i.guiScript.SetAlertMessageModalOne(AlertType.DebugAI);
                                }
                                //reverts back to Authority Player, Resistance AI
                                else if (GameManager.i.sideScript.resistanceOverall == SideState.Human)
                                {
                                    optionNoAI = "AI ON";
                                    GameManager.i.optionScript.isAI = true;
                                    GameManager.i.sideScript.authorityCurrent = SideState.Human;
                                    GameManager.i.sideScript.resistanceCurrent = SideState.AI;
                                    GameManager.i.sideScript.authorityOverall = SideState.Human;
                                    GameManager.i.sideScript.resistanceOverall = SideState.AI;
                                    //notification
                                    GameManager.i.guiScript.SetAlertMessageModalOne(AlertType.DebugPlayer);
                                }
                                break;
                            //resistance player
                            case 2:
                                //switch AI Off -> Manual player control for both sides
                                if (GameManager.i.sideScript.authorityOverall == SideState.AI)
                                {
                                    optionNoAI = "AI OFF";
                                    GameManager.i.optionScript.isAI = false;
                                    GameManager.i.sideScript.authorityCurrent = SideState.Human;
                                    GameManager.i.sideScript.resistanceCurrent = SideState.Human;
                                    GameManager.i.sideScript.authorityOverall = SideState.Human;
                                    GameManager.i.sideScript.resistanceOverall = SideState.Human;
                                    //notification
                                    GameManager.i.guiScript.SetAlertMessageModalOne(AlertType.DebugAI);
                                }
                                //reverts back to Resistance Player, Authority AI
                                else if (GameManager.i.sideScript.authorityOverall == SideState.Human)
                                {
                                    optionNoAI = "AI ON";
                                    GameManager.i.optionScript.isAI = true;
                                    GameManager.i.sideScript.authorityCurrent = SideState.AI;
                                    GameManager.i.sideScript.resistanceCurrent = SideState.Human;
                                    GameManager.i.sideScript.authorityOverall = SideState.AI;
                                    GameManager.i.sideScript.resistanceOverall = SideState.Human;
                                    //notification
                                    GameManager.i.guiScript.SetAlertMessageModalOne(AlertType.DebugPlayer);
                                }
                                break;
                        }
                    }
                }
            }

            //tenth button
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 7 + button_height * 7, button_width, button_height), optionPowerUI))
            {
                Debug.Log("[Dbg] Button -> Toggle Info UI Display");
                if (GameManager.i.actorPanelScript.CheckInfoUIStatus() == true)
                {
                    GameManager.i.actorPanelScript.SetActorInfoUI(false);
                    optionPowerUI = "Power ON";
                }
                else
                {
                    GameManager.i.actorPanelScript.SetActorInfoUI(true);
                    optionPowerUI = "Compatibility ON";
                }
            }

            //eleventh button
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 8 + button_height * 8, button_width, button_height), optionContacts))
            {
                Debug.Log("[Dbg] Button -> Toggle Show Contacts in Node tooltips");
                if (GameManager.i.optionScript.showContacts == true)
                {
                    GameManager.i.optionScript.showContacts = false;
                    optionContacts = "Contacts ON";
                }
                else
                {
                    GameManager.i.optionScript.showContacts = true;
                    optionContacts = "Contacts OFF";
                }
            }

            //twelfth button
            offset = 9;
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionMoodInfo))
            {
                Debug.Log("[Dbg] Button -> Toggle Full Mood Info option");
                if (GameManager.i.optionScript.fullMoodInfo == true)
                {
                    GameManager.i.optionScript.fullMoodInfo = false;
                    optionMoodInfo = "Mood Info ON";
                }
                else
                {
                    GameManager.i.optionScript.fullMoodInfo = true;
                    optionMoodInfo = "MoodInfo OFF";
                }
            }

            //thirteenth button
            offset = 10;
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionBillboardInfo))
            {
                Debug.Log("[Dbg] Button -> Toggle Billboard option");
                if (GameManager.i.optionScript.billboard == true)
                {
                    GameManager.i.optionScript.billboard = false;
                    optionBillboardInfo = "Billboard ON";
                }
                else
                {
                    GameManager.i.optionScript.billboard = true;
                    optionBillboardInfo = "Billboard OFF";
                }
            }

            //Subordinates button
            offset = 11;
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionSubordinates))
            {
                Debug.Log("[Dbg] Button -> Toggle Subordinates option");
                if (GameManager.i.optionScript.isSubordinates == true)
                {
                    //subordinates off
                    optionSubordinates = "Subordinates ON";
                    GameManager.i.featureScript.ToggleOnMapActors(false);
                }
                else
                {
                    //subordinates on
                    optionSubordinates = "Subordinates OFF";
                    GameManager.i.featureScript.ToggleOnMapActors(true);
                }
            }

            //Decisions button
            offset = 12;
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionDecisions))
            {
                Debug.Log("[Dbg] Button -> Toggle Decisions option");
                if (GameManager.i.optionScript.isDecisions == true)
                {
                    GameManager.i.optionScript.isDecisions = false;
                    optionDecisions = "Decisions ON";
                }
                else
                {
                    GameManager.i.optionScript.isDecisions = true;
                    optionDecisions = "Decisions OFF";
                }
            }

            //MainInfoApp button
            offset = 13;
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionMainInfoApp))
            {
                Debug.Log("[Dbg] Button -> Toggle MainInfoApp option");
                if (GameManager.i.optionScript.isMainInfoApp == true)
                {
                    GameManager.i.optionScript.isMainInfoApp = false;
                    optionMainInfoApp = "InfoApp ON";
                }
                else
                {
                    GameManager.i.optionScript.isMainInfoApp = true;
                    optionMainInfoApp = "InfoApp OFF";
                }
            }

            //Review button
            offset = 14;
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionReviews))
            {
                Debug.Log("[Dbg] Button -> Toggle Review option");
                if (GameManager.i.optionScript.isReviews == true)
                {
                    GameManager.i.optionScript.isReviews = false;
                    optionReviews = "Reviews ON";
                }
                else
                {
                    GameManager.i.optionScript.isReviews = true;
                    optionReviews = "Reviews OFF";
                }
            }

            //Nemesis button
            offset = 15;
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionNemesis))
            {
                Debug.Log("[Dbg] Button -> Toggle Nemesis option");
                if (GameManager.i.optionScript.isNemesis == true)
                {
                    GameManager.i.optionScript.isNemesis = false;
                    optionNemesis = "Nemesis ON";
                }
                else
                {
                    GameManager.i.optionScript.isNemesis = true;
                    optionNemesis = "Nemesis OFF";
                }
            }

            //NPC button
            offset = 16;
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionNPC))
            {
                Debug.Log("[Dbg] Button -> Toggle NPC option");
                if (GameManager.i.optionScript.isNPC == true)
                {
                    GameManager.i.optionScript.isNPC = false;
                    optionNPC = "NPC ON";
                }
                else
                {
                    GameManager.i.optionScript.isNPC = true;
                    optionNPC = "NPC OFF";
                }
            }

            //Objectives button
            offset = 17;
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionObjectives))
            {
                Debug.Log("[Dbg] Button -> Toggle Objectives option");
                if (GameManager.i.optionScript.isObjectives == true)
                {
                    GameManager.i.optionScript.isObjectives = false;
                    optionObjectives = "Objectives ON";
                }
                else
                {
                    GameManager.i.optionScript.isObjectives = true;
                    optionObjectives = "Objectives OFF";
                }
            }

            //Organisations button
            offset = 18;
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionOrganisations))
            {
                Debug.Log("[Dbg] Button -> Toggle Organisations option");
                if (GameManager.i.optionScript.isOrganisations == true)
                {
                    GameManager.i.optionScript.isOrganisations = false;
                    optionOrganisations = "Organisations ON";
                }
                else
                {
                    GameManager.i.optionScript.isOrganisations = true;
                    optionOrganisations = "Organisations OFF";
                }
            }

            //Targets button
            offset = 19;
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionTargets))
            {
                Debug.Log("[Dbg] Button -> Toggle Targets option");
                if (GameManager.i.optionScript.isTargets == true)
                {
                    GameManager.i.optionScript.isTargets = false;
                    optionTargets = "Targets ON";
                }
                else
                {
                    GameManager.i.optionScript.isTargets = true;
                    optionTargets = "Targets OFF";
                }
            }

            //Gear button
            offset = 20;
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionGear))
            {
                Debug.Log("[Dbg] Button -> Toggle Gear option");
                if (GameManager.i.optionScript.isGear == true)
                {
                    GameManager.i.optionScript.isGear = false;
                    optionGear = "Gear ON";
                }
                else
                {
                    GameManager.i.optionScript.isGear = true;
                    optionGear = "Gear OFF";
                }
            }

            //Recruit actors button
            offset = 21;
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionRecruit))
            {
                Debug.Log("[Dbg] Button -> Toggle Recruiting option");
                if (GameManager.i.optionScript.isRecruit == true)
                {
                    GameManager.i.optionScript.isRecruit = false;
                    optionRecruit = "Recruiting ON";
                }
                else
                {
                    GameManager.i.optionScript.isRecruit = true;
                    optionRecruit = "Recruiting OFF";
                }
            }

            //Move Security button
            offset = 22;
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionMoveSecurity))
            {
                Debug.Log("[Dbg] Button -> Toggle Move Security option");
                if (GameManager.i.optionScript.isMoveSecurity == true)
                {
                    GameManager.i.optionScript.isMoveSecurity = false;
                    optionMoveSecurity = "Move Sec ON";
                }
                else
                {
                    GameManager.i.optionScript.isMoveSecurity = true;
                    optionMoveSecurity = "Move Sec OFF";
                }
            }

            //Actions button
            offset = 23;
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionActions))
            {
                Debug.Log("[Dbg] Button -> Toggle Actions option");
                if (GameManager.i.optionScript.isActions == true)
                {
                    GameManager.i.optionScript.isActions = false;
                    optionActions = "Actions ON";
                }
                else
                {
                    GameManager.i.optionScript.isActions = true;
                    optionActions = "Actions OFF";
                }
            }

            //
            // - - - Actions (third box)
            //

            //first button
            offset = 0;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Remove Secret"))
            {
                Debug.Log("[Dbg] Button -> Remove Random Secret");
                if (GameManager.i.inputScript.ModalState == ModalState.Normal)
                {
                    //Remove a random secret from Player and anyone else
                    GameManager.i.playerScript.DebugRemoveRandomSecret();
                }
            }

            //second button
            offset = 1;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Toggle Orgs"))
            {
                Debug.Log("[Dbg] Button -> Toggle All Organisations (isContact)");
                GameManager.i.orgScript.DebugToggleAllOrganisations();
            }

            //third button
            offset = 2;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Remove Tracer"))
            {
                Debug.LogFormat("[Dbg] Button -> Toggle Remove Tracer at nodeID {0}{1}", GameManager.i.nodeScript.GetPlayerNodeID(), "\n");
                if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideResistance.level)
                {
                    Node node = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.GetPlayerNodeID());
                    if (node != null) { node.RemoveTracer(); } else { Debug.LogError("Invalid current Player node (Null)"); }
                }
            }

            //fourth button
            offset = 3;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Activate Cures"))
            {
                Debug.Log("[Dbg] Button -> Activate Cures");
                GameManager.i.dataScript.DebugActivateAllCures();
            }

            //fifth button
            offset = 4;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Remove Condition"))
            {
                //removes a condition from Player or an actor
                Debug.Log("[Dbg] Button -> Remove Condition");
                if (debugDisplay != 52)
                { debugDisplay = 52; }
                else { debugDisplay = 0; }
            }

            //sixth button
            offset = 5;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Actor Conflict"))
            {
                //initiate an actor conflict
                Debug.Log("[Dbg] Button -> Actor Conflict");
                if (debugDisplay != 81)
                { debugDisplay = 81; }
                else { debugDisplay = 0; }
            }



            //tenth button
            offset = 6;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Give Gear"))
            {
                //Resistance player only
                if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideResistance.level)
                {
                    Debug.Log("[Dbg] Button -> Give Gear");
                    if (debugDisplay != 16)
                    { debugDisplay = 16; }
                    else { debugDisplay = 0; }
                }
            }
            //eleventh button
            offset = 7;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Give Condition"))
            {
                Debug.Log("[Dbg] Button -> Give Condition");
                if (debugDisplay != 18)
                { debugDisplay = 18; }
                else { debugDisplay = 0; }
            }
            //twelfth button
            offset = 8;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Give Trait"))
            {
                Debug.Log("[Dbg] Button -> Give Actor Trait");
                if (debugDisplay != 20)
                { debugDisplay = 20; }
                else { debugDisplay = 0; }
            }
            //thirteenth button
            offset = 9;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Give Power"))
            {
                Debug.Log("[Dbg] Button -> Give Player Power");
                GameManager.i.playerScript.DebugGivePower();
            }

            offset = 10;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Give CaptureTool" /*optionAITraceback*/))
            {
                //Resistance player only
                if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideResistance.level)
                {
                    Debug.Log("[Dbg] Button -> Give CaptureTool");
                    if (debugDisplay != 84)
                    { debugDisplay = 84; }
                    else { debugDisplay = 0; }
                }
            }

            //fourteenth button
            offset = 11;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Set Mood" /*"AISecProtocol +"*/))
            {
                /*Debug.Log("[Dbg] Button -> Increase AI Security Protocol");
                GameManager.instance.aiScript.IncreaseAISecurityProtocolLevel();*/

                Debug.Log("[Dbg] Button -> Set Player Mood");
                if (debugDisplay != 88)
                { debugDisplay = 88; }
                else { debugDisplay = 0; }
            }

            //seventh button
            offset = 12;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Set Innocence" /*optionAIOffline*/))
            {
                /*Debug.Log("[Dbg] Button -> Toggle AI Offline");
                if (GameManager.instance.aiScript.CheckAIOffLineStatus() == true)
                {
                    GameManager.instance.aiScript.SetAIOffline(false);
                    optionAIOffline = "AIOffline ON";
                }
                else
                {
                    GameManager.instance.aiScript.SetAIOffline(true);
                    optionAIOffline = "AIOffline OFF";
                }*/

                Debug.Log("[Dbg] Button -> Set Player Innocence");
                if (debugDisplay != 86)
                { debugDisplay = 86; }
                else { debugDisplay = 0; }
            }

            //thirteenth button
            offset = 13;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Set Sec State"))
            {
                Debug.Log("[Dbg] Button -> Set Security State");
                if (debugDisplay != 22)
                { debugDisplay = 22; }
                else { debugDisplay = 0; }
            }

            /*//eigth button
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
            }*/

            //ninth button
            offset = 14;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Capture Player" /*optionAIScreamer*/))
            {
                /*Debug.Log("[Dbg] Button -> Toggle AI Screamer");
                if (GameManager.instance.aiScript.CheckAIScreamerStatus() == true)
                {
                    GameManager.instance.aiScript.SetAIScreamer(false);
                    optionAIScreamer = "AIScreamer ON";
                }
                else
                {
                    GameManager.instance.aiScript.SetAIScreamer(true);
                    optionAIScreamer = "AIScreamer OFF";
                }*/
                Debug.Log("[Dbg] Button -> Capture Player");
                GameManager.i.captureScript.DebugCapturePlayer();
            }


            //thirteenth button
            offset = 15;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "City Loyalty"))
            {
                Debug.Log("[Dbg] Button -> Change City Loyalty");
                debugDisplay = 97;
            }

            //thirteenth button
            offset = 16;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Give Secret"))
            {
                Debug.Log("[Dbg] Button -> Give Player a Random Secret");
                GameManager.i.playerScript.DebugAddRandomSecret();
            }

            //fourteenth button
            offset = 17;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Add Contact"))
            {
                Debug.Log("[Dbg] Button -> Add Contact");
                if (debugDisplay != 27)
                { debugDisplay = 27; }
                else { debugDisplay = 0; }
            }

            //fifteenth button
            offset = 18;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Remove Contact"))
            {
                Debug.Log("[Dbg] Button -> Remove Contact");
                if (debugDisplay != 29)
                { debugDisplay = 29; }
                else { debugDisplay = 0; }
            }


            //sixteenth button
            offset = 19;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Set Traitor"))
            {
                Debug.Log("[Dbg] Button -> Set Traitor");
                if (debugDisplay != 103)
                { debugDisplay = 103; }
                else { debugDisplay = 0; }
            }

            //seventeenth button
            offset = 20;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Control Nemesis"))
            {
                Debug.Log("[Dbg] Button -> Control Nemesis");
                if (debugDisplay != 41)
                { debugDisplay = 41; }
                else { debugDisplay = 0; }
            }

            //eightteenth button
            offset = 21;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Help Debug"))
            {
                Debug.Log("[Dbg] Button -> Help Debug");
                //toggle help on/off
                if (GameManager.i.tooltipHelpScript.CheckTooltipActive() == false)
                { GameManager.i.helpScript.DebugShowHelp(); }
                else { GameManager.i.tooltipHelpScript.CloseTooltip(); }
            }

            //nineteenth button
            offset = 22;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Toggle Contact"))
            {
                Debug.Log("[Dbg] Button -> Toggle Contact Active/Inactive");
                if (debugDisplay != 47)
                { debugDisplay = 47; }
                else { debugDisplay = 0; }
            }

            //twentyfourth button
            offset = 23;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "News Tester"))
            {
                Debug.Log("[Dbg] Button -> Topic News Tester");
                GameManager.i.topicScript.DebugTestNews();
            }

            //twentyFifth button
            offset = 24;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "StoryHelp Tester"))
            {
                Debug.Log("[Dbg] Button -> StoryHelp Tester");
                GameManager.i.topicScript.DebugTestStoryHelp();
            }

            //twentySixth button
            offset = 25;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Option Tester"))
            {
                Debug.Log("[Dbg] Button -> TopicOption Tester");
                GameManager.i.topicScript.DebugTestTopicOptions();
            }

            //twentySeventh button
            offset = 26;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Set Friend"))
            {
                Debug.Log("[Dbg] Button -> Set Friend");
                if (debugDisplay != 92)
                { debugDisplay = 92; }
                else { debugDisplay = 0; }
            }

            //twentyEight button
            offset = 27;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Set Enemy"))
            {
                Debug.Log("[Dbg] Button -> Set Enemy");
                if (debugDisplay != 94)
                { debugDisplay = 94; }
                else { debugDisplay = 0; }
            }

            //twentyNinth button
            offset = 28;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Add Investigation"))
            {
                Debug.Log("[Dbg] Button -> Add Investigation");
                GameManager.i.playerScript.DebugAddInvestigation();
            }

            //thirtieth button
            offset = 29;
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Rnd Obj Prog"))
            {
                Debug.Log("[Dbg] Button -> Random Objective Progress");
                //test condition -> randomly give progress to objectives
                GameManager.i.objectiveScript.DebugTestObjectives();
            }

            //
            // - - - Level Menu - - -
            //

            //first button
            offset = 0;
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Most Connected"))
            {
                Debug.Log("[Dbg] Button -> Show Most Connected Nodes");
                EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.MostConnected, "DebugGUI.cs -> OnGUI");
            }

            //second button
            offset = 1;
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Centre Nodes"))
            {
                Debug.Log("[Dbg] Button -> Centre Nodes");
                GameManager.i.debugGraphicsScript.SetCentrePane(true);
                EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.Centre, "DebugGUI.cs -> OnGUI");
            }

            //third button
            offset = 2;
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Near Neighbours"))
            {
                Debug.Log("[Dbg] Button -> NearNeighbours");
                EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.NearNeighbours, "DebugGUI.cs -> OnGUI");
            }

            //fourth button
            offset = 3;
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Decision Nodes"))
            {
                Debug.Log("[Dbg] Button -> Show Decision Nodes");
                EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.DecisionNodes, "DebugGUI.cs -> OnGUI");
            }

            //fifth button
            offset = 4;
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Crisis Nodes"))
            {
                Debug.Log("[Dbg] Button -> Show Crisis Nodes");
                EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.CrisisNodes, "DebugGUI.cs -> OnGUI");
            }

            //sixth button
            offset = 5;
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionNodes))
            {
                Debug.Log("[Dbg] Button -> Show All NodeID's");
                switch (showNodesToggle)
                {
                    case 0:
                        if (GameManager.i.optionScript.noNodes == false)
                        { GameManager.i.nodeScript.ShowAllNodeID(); }
                        else
                        {
                            GameManager.i.nodeScript.SetDistrictFaceText(NodeText.ID);
                            optionNodes = "Show ICONS";
                            showNodesToggle = 1;
                        }
                        break;
                    case 1:
                        if (GameManager.i.optionScript.noNodes == true)
                        {
                            GameManager.i.nodeScript.SetDistrictFaceText(NodeText.Icon);
                            optionNodes = "No Node Text";
                            showNodesToggle = 2;
                        }
                        break;
                    case 2:
                        if (GameManager.i.optionScript.noNodes == true)
                        {
                            GameManager.i.nodeScript.SetDistrictFaceText(NodeText.None);
                            optionNodes = "Show IDs";
                            showNodesToggle = 0;
                        }
                        break;
                }

            }

            //seventh button
            offset = 6;
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionPath))
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
            offset = 7;
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Recalc Weighted"))
            {
                Debug.Log("[Dbg] Button -> Recaculate Weighted Dijkstra data");
                GameManager.i.dijkstraScript.RecalculateWeightedData();
            }

            //ninth button
            offset = 8;
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Loiter Nodes"))
            {
                Debug.Log("[Dbg] Button -> Show Loiter Nodes");
                EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.LoiterNodes, "DebugGUI.cs -> OnGUI");
            }

            //tenth button
            offset = 9;
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Cure Nodes"))
            {
                Debug.Log("[Dbg] Button -> Show Cure Nodes");
                EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.CureNodes, "DebugGUI.cs -> OnGUI");
            }

            //eleventh button
            offset = 10;
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionShowContacts))
            {
                Debug.Log("[Dbg] Button -> Show Contact Nodes");
                switch (showContactToggle)
                {
                    case 0:
                        if (GameManager.i.optionScript.noNodes == false)
                        { EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.ShowContacts, "DebugGUI.cs -> OnGUI"); }
                        else
                        {
                            GameManager.i.nodeScript.SetDistrictFaceText(NodeText.Contact);
                            optionShowContacts = "Contacts OFF";
                            showContactToggle = 1;
                        }
                        break;
                    case 1:
                        if (GameManager.i.optionScript.noNodes == true)
                        {
                            GameManager.i.nodeScript.SetDistrictFaceText(NodeText.None);
                            optionShowContacts = "Contacts ON";
                            showContactToggle = 0;
                        }
                        break;
                }
            }

            //twelth button
            offset = 11;
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionNodeDisplay))
            {
                Debug.Log("[Dbg] Button -> Toggle Node Display (Node/District)");
                switch (nodeDisplayToggle)
                {
                    case 0:
                        GameManager.i.nodeScript.SetNodeType(NodeType.Node);
                        optionNodeDisplay = "Show DISTRICTS";
                        nodeDisplayToggle = 1;
                        break;
                    case 1:
                        GameManager.i.nodeScript.SetNodeType(NodeType.District);
                        optionNodeDisplay = "Show NODES";
                        nodeDisplayToggle = 0;
                        break;
                }
            }

            //thirteenth button
            offset = 12;
            if (GUI.Button(new Rect(box_level + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), optionTileDisplay))
            {
                Debug.Log("[Dbg] Button -> Toggle Surveillance Nodes On/Off");
                switch (tileDisplayToggle)
                {
                    case 0:
                        GameManager.i.levelScript.DebugDisplaySurveillanceTiles(true);
                        optionTileDisplay = "Show Surv Tiles";
                        tileDisplayToggle = 1;
                        break;
                    case 1:
                        GameManager.i.levelScript.DebugDisplaySurveillanceTiles(false);
                        optionTileDisplay = "Restore Tiles";
                        tileDisplayToggle = 0;
                        break;
                }
            }

            //
            // - - - File Menu - - -
            //

            //first button
            offset = 0;
            if (GUI.Button(new Rect(box_file + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Export TextLists"))
            {
                Debug.Log("[Dbg] Button -> Export TextLists");
                GameManager.i.textScript.ExportTextLists();
            }

            //second button
            offset = 1;
            if (GUI.Button(new Rect(box_file + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Import TextLists"))
            {
                Debug.Log("[Dbg] Button -> Import Data");
                GameManager.i.textScript.ImportTextLists();
            }

            //third button
            offset = 2;
            if (GUI.Button(new Rect(box_file + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Exp Topic Options"))
            {
                Debug.Log("[Dbg] Button -> Export Topic Options");
                GameManager.i.textScript.ExportTopicOptions();
            }

            //fourth button
            offset = 3;
            if (GUI.Button(new Rect(box_file + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Exp Story Data"))
            {
                Debug.Log("[Dbg] Button -> Export Story Data");
                GameManager.i.fileScript.ExportStoryData();

            }

            //fifth button
            offset = 4;
            if (GUI.Button(new Rect(box_file + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Exp Story Help"))
            {
                Debug.Log("[Dbg] Button -> Export Story Help");
                GameManager.i.fileScript.ExportStoryHelp();
            }

            //six button
            offset = 5;
            if (GUI.Button(new Rect(box_file + offset_x, box_y + gap_y + offset_y * offset + button_height * offset, button_width, button_height), "Exp Actor Pool"))
            {
                Debug.Log("[Dbg] Button -> Export Actor Pool");
                GameManager.i.fileScript.ExportActorPoolData();
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
                        analysis = GameManager.i.dataScript.DebugLevelAnalysis();
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
                                analysis = GameManager.i.teamScript.DebugDisplayTeamAnalysis();
                                GUI.Box(new Rect(Screen.width - 205, 10, 200, 240), analysis, customBackground);
                                break;
                            case TeamDebug.Roster:
                                analysis = GameManager.i.teamScript.DebugDisplayIndividualTeams();
                                GUI.Box(new Rect(Screen.width - 405, 10, 400, 320), analysis, customBackground);
                                break;
                            case TeamDebug.Actors:
                                analysis = GameManager.i.teamScript.DebugDisplayTeamActorAnalysis();
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
                        analysis = GameManager.i.inputScript.DebugDisplayGameState();
                        GUI.Box(new Rect(Screen.width - 305, 10, 300, 800), analysis, customBackground);
                        break;
                    //actor Pools
                    case 6:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.actorScript.DebugDisplayPools();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 900), analysis, customBackground);
                        break;
                    //actor Lists
                    case 7:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayActorLists();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 700), analysis, customBackground);
                        break;
                    //Player data
                    case 8:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.playerScript.DebugDisplayPlayerStats();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 900), analysis, customBackground);
                        break;
                    //Toggle Messages
                    case 9:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = "Unknown";
                        switch (messageToggle)
                        {
                            case 1:
                                analysis = GameManager.i.dataScript.DebugDisplayMessages(MessageCategory.Pending); break;
                            case 2:
                                analysis = GameManager.i.dataScript.DebugDisplayMessages(MessageCategory.Current); break;
                            case 3:
                                analysis = GameManager.i.dataScript.DebugDisplayMessages(MessageCategory.Archive); break;
                            case 4:
                                analysis = GameManager.i.dataScript.DebugDisplayMessages(MessageCategory.AI); break;
                            case 5:
                                analysis = GameManager.i.guiScript.DebugDisplayInfoPipeLine(); break;
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
                                analysis = GameManager.i.aiScript.DisplayTaskData();
                                break;
                            case AIDebugData.Node:
                                analysis = GameManager.i.aiScript.DisplayNodeData();
                                break;
                            case AIDebugData.Spider:
                                analysis = GameManager.i.aiScript.DisplaySpiderData();
                                break;
                            case AIDebugData.Erasure:
                                analysis = GameManager.i.aiScript.DisplayErasureData();
                                break;
                            case AIDebugData.Decision:
                                analysis = GameManager.i.aiScript.DisplayDecisionData();
                                break;
                        }
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 600), analysis, customBackground);
                        break;
                    //HQs
                    case 11:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.hqScript.DebugDisplayHq();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 500), analysis, customBackground);
                        break;
                    //Show Options
                    case 12:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.optionScript.DisplayOptions();
                        GUI.Box(new Rect(Screen.width - 460, 10, 450, 750), analysis, customBackground);
                        break;
                    //Help
                    case 13:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.helpScript.DisplayHelp();
                        GUI.Box(new Rect(Screen.width - 305, 10, 300, 800), analysis, customBackground);
                        break;
                    //Ongoing Register
                    case 14:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayOngoingRegister();
                        GUI.Box(new Rect(Screen.width - 460, 10, 450, 350), analysis, customBackground);
                        break;
                    //Actions Adjustment Register
                    case 15:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayActionsRegister();
                        GUI.Box(new Rect(Screen.width - 460, 10, 450, 350), analysis, customBackground);
                        break;
                    //Give Gear Input
                    case 16:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 100, 50, 200, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 75, 55, 150, 20), "Gear.name (no spaces)");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 50, 90, 100, 20), textInput_0);
                        status = GUIStatus.GiveGear;
                        textOutput = null;
                        break;
                    //Give Gear processing & Output
                    case 17:
                        if (textOutput == null)
                        { textOutput = GameManager.i.playerScript.DebugAddGear(textInput_0); }
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
                        { textOutput = GameManager.i.actorScript.DebugAddCondition(textInput_0, textInput_1); }
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
                        { textOutput = GameManager.i.actorScript.DebugAddTrait(textInput_0, textInput_1); }
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
                        { textOutput = GameManager.i.turnScript.DebugSetState(textInput_0, textInput_1); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 475, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //Gear Info Display
                    case 24:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayGearData();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 750), analysis, customBackground);
                        break;
                    //Secret Info Display
                    case 25:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.secretScript.DebugDisplaySecretData();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 800), analysis, customBackground);
                        break;
                    //Node Crisis Data Display
                    case 26:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayCrisisNodes();
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
                        { textOutput = GameManager.i.dataScript.DebugAddContact(textInput_0, textInput_1); }
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
                        { textOutput = GameManager.i.dataScript.DebugRemoveContact(textInput_0, textInput_1); }
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
                        { textOutput = GameManager.i.contactScript.DebugToggleIsContactKnown(textInput_0); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 475, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //Contacts -> Resistance
                    case 33:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.contactScript.DebugDisplayContacts();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 800), analysis, customBackground);
                        break;
                    //Targets Generic
                    case 34:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayGenericTargets();
                        GUI.Box(new Rect(Screen.width - 555, 10, 550, 600), analysis, customBackground);
                        break;
                    //Target Pools
                    case 35:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        /*analysis = GameManager.instance.dataScript.DebugShowGenericTargets();*/
                        analysis = GameManager.i.dataScript.DebugDisplayTargetPools();
                        GUI.Box(new Rect(Screen.width - 555, 10, 550, 600), analysis, customBackground);
                        break;
                    //Target Dictionary
                    case 36:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        /*analysis = GameManager.instance.dataScript.DebugShowGenericTargets();*/
                        analysis = GameManager.i.dataScript.DebugDisplayTargetDict();
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
                            textOutput = GameManager.i.dijkstraScript.DebugShowPath(Convert.ToInt32(textInput_0), Convert.ToInt32(textInput_1), true);
                        }
                        break;
                    //Swith Path OFF
                    case 39:
                        EventManager.i.PostNotification(EventType.FlashMultipleConnectionsStop, this, "DebugGUI.cs -> OnGUI");
                        status = GUIStatus.None;
                        optionPath = "Input Path";
                        break;
                    //Nemesis Data
                    case 40:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.nemesisScript.DebugShowNemesisStatus();
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
                            { GameManager.i.nemesisScript.SetPlayerControlStart(nodeID, goal); }
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
                        analysis = GameManager.i.aiRebelScript.DebugShowRebelAIStatus();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 750), analysis, customBackground);
                        break;
                    //Rebel Tracker data
                    case 44:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugShowRebelMoves();
                        GUI.Box(new Rect(Screen.width - 305, 10, 300, 600), analysis, customBackground);
                        break;
                    //Nemesis Tracker data
                    case 45:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugShowNemesisMoves();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 600), analysis, customBackground);
                        break;
                    //Contacts by Node data
                    case 46:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.contactScript.DebugDisplayContactsByNode();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 800), analysis, customBackground);
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
                            { analysis = GameManager.i.dataScript.ContactToggleActive(contactID); }
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
                        analysis = GameManager.i.contactScript.DebugDisplayContactsDict();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 800), analysis, customBackground);
                        break;
                    //Statistics Data
                    case 50:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.statScript.DebugShowStatistics();
                        GUI.Box(new Rect(Screen.width - 355, 10, 350, 1000), analysis, customBackground);
                        break;
                    //AI Analysis Data
                    case 51:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = string.Format("{0}{1}", GameManager.i.aiScript.DebugShowTaskAnalysis(), GameManager.i.aiRebelScript.DebugShowTaskAnalysis());
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
                        { textOutput = GameManager.i.actorScript.DebugRemoveCondition(textInput_0, textInput_1); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 475, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //actor dictionary
                    case 54:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayActorDict();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 850), analysis, customBackground);
                        break;
                    //campaign data
                    case 55:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.campaignScript.DebugDisplayCampaignData();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 700), analysis, customBackground);
                        break;
                    //scenario data
                    case 56:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.scenarioScript.DebugDisplayScenarioData();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 900), analysis, customBackground);
                        break;
                    //Personality data (OnMap actors)
                    case 57:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.personScript.DebugDisplayAllPersonalities();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 800), analysis, customBackground);
                        break;
                    //Actor Compatibility Range
                    case 58:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.personScript.DebugCheckActorCompatibilityRange();
                        GUI.Box(new Rect(Screen.width - 405, 10, 500, 800), analysis, customBackground);
                        break;
                    //Actor Emotional History
                    case 59:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.personScript.DebugDisplayActorMotivationHistory();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 800), analysis, customBackground);
                        break;
                    //player stats -> mood history
                    case 60:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.playerScript.DebugDisplayMoodHistory();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 600), analysis, customBackground);
                        break;
                    //Personality -> Player Preferences
                    case 61:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.personScript.DebugDisplayPlayerLikes();
                        GUI.Box(new Rect(Screen.width - 455, 10, 455, 800), analysis, customBackground);
                        break;
                    //Topic Data
                    case 62:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.topicScript.DebugDisplayTopicTypes();
                        GUI.Box(new Rect(Screen.width - 505, 10, 500, 750), analysis, customBackground);
                        break;
                    //Topic Type lists
                    case 63:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.topicScript.DebugDisplayTopicTypeLists();
                        GUI.Box(new Rect(Screen.width - 505, 10, 500, 850), analysis, customBackground);
                        break;
                    //Topic Pools
                    case 64:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.topicScript.DebugDisplayCurrentTopicPool();
                        GUI.Box(new Rect(Screen.width - 505, 10, 500, 850), analysis, customBackground);
                        break;
                    //Topic Selection data
                    case 65:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.topicScript.DebugDisplayTopicSelectionData();
                        GUI.Box(new Rect(Screen.width - 505, 10, 500, 700), analysis, customBackground);
                        break;
                    //Topic Profile data
                    case 66:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.topicScript.DebugDisplayTopicProfileData();
                        GUI.Box(new Rect(Screen.width - 505, 10, 500, 850), analysis, customBackground);
                        break;
                    //Topic Criteria data
                    case 67:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.topicScript.DebugDisplayCriteria();
                        GUI.Box(new Rect(Screen.width - 505, 10, 500, 850), analysis, customBackground);
                        break;
                    //Actor NodeActionData
                    case 68:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayActorNodeActionData();
                        GUI.Box(new Rect(Screen.width - 455, 10, 450, 800), analysis, customBackground);
                        break;
                    //Actor TeamActionData
                    case 69:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayActorTeamActionData();
                        GUI.Box(new Rect(Screen.width - 455, 10, 450, 800), analysis, customBackground);
                        break;
                    //Personality data (HQ actors)
                    case 70:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.personScript.DebugDisplayHQPersonalities();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 800), analysis, customBackground);
                        break;
                    //HQs
                    case 71:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.hqScript.DebugDisplayHqActors();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 700), analysis, customBackground);
                        break;
                    //News Items
                    case 72:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.newsScript.DebugDisplayNewsItems();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 800), analysis, customBackground);
                        break;
                    //Adverts
                    case 73:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.newsScript.DebugDisplayAdverts();
                        GUI.Box(new Rect(Screen.width - 1010, 10, 1000, 800), analysis, customBackground);
                        break;
                    //HQ dictionary
                    case 74:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayHQDict();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 850), analysis, customBackground);
                        break;
                    //belief frequency analysis
                    case 75:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayBeliefCount();
                        GUI.Box(new Rect(Screen.width - 355, 10, 500, 300), analysis, customBackground);
                        break;
                    //topic and topicOption text tag frequency analysis
                    case 76:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayTextTagCount();
                        GUI.Box(new Rect(Screen.width - 355, 10, 500, 800), analysis, customBackground);
                        break;
                    //Statistics Data
                    case 77:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.statScript.DebugShowRatios();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 600), analysis, customBackground);
                        break;
                    //Organisation Data
                    case 78:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayCurrentOrganisations();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 600), analysis, customBackground);
                        break;
                    //Npc Tracker data
                    case 79:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugShowNpcMoves();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 600), analysis, customBackground);
                        break;
                    //Actor (Current) Details
                    case 80:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayActorDetails();
                        GUI.Box(new Rect(Screen.width - 455, 10, 450, 800), analysis, customBackground);
                        break;
                    //Initiate a Relationship conflict
                    case 81:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 385, 50, 200, 90), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 375, 60, 150, 20), "Create a Conflict");
                        GUI.Label(new Rect(Screen.width / 2 - 375, 110, 150, 20), "Input Actor (0 - 3)");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 375, 85, 100, 20), textInput_0);
                        status = GUIStatus.Conflict;
                        textOutput = null;
                        break;
                    //Start relationship conflict
                    case 82:
                        if (textOutput == null)
                        { textOutput = GameManager.i.actorScript.DebugCreateConflict(textInput_0); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 375, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //player stats -> investigations
                    case 83:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.playerScript.DebugDisplayInvestigations();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 600), analysis, customBackground);
                        break;
                    //Give CaptureTool input
                    case 84:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 100, 50, 200, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 75, 55, 150, 20), "Innocence Lvl (0 to 3)");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 50, 90, 100, 20), textInput_0);
                        status = GUIStatus.GiveCaptureTool;
                        textOutput = null;
                        break;
                    //Give CaptureTool processing & Output
                    case 85:
                        if (textOutput == null)
                        { textOutput = GameManager.i.playerScript.DebugAddCaptureTool(textInput_0); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 175, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //Set Player Innnocence Input
                    case 86:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 100, 50, 200, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 75, 55, 150, 20), "Innocence Lvl (0 to 3)");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 50, 90, 100, 20), textInput_0);
                        status = GUIStatus.SetInnocence;
                        textOutput = null;
                        break;
                    //Set Innocence processing & Output
                    case 87:
                        if (textOutput == null)
                        { textOutput = GameManager.i.playerScript.DebugSetInnocence(textInput_0); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 175, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //Set Player Mood Input
                    case 88:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 100, 50, 200, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 75, 55, 150, 20), "Mood Lvl (0 to 3)");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 50, 90, 100, 20), textInput_0);
                        status = GUIStatus.SetMood;
                        textOutput = null;
                        break;
                    //Set Mood processing & Output
                    case 89:
                        if (textOutput == null)
                        { textOutput = GameManager.i.playerScript.DebugSetMood(textInput_0); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 175, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //Actor Compatibility with other Actors
                    case 90:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.personScript.DebugDisplayActorCompatibility();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 800), analysis, customBackground);
                        break;
                    //Actor Relationship with other Actors
                    case 91:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayActorRelations();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 800), analysis, customBackground);
                        break;
                    //Set Friend Input
                    case 92:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 100, 30, 200, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 75, 35, 150, 20), "First Actor slotID (0 to 3)");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 50, 55, 100, 20), textInput_0);
                        GUI.Label(new Rect(Screen.width / 2 - 75, 85, 150, 20), "Second Actor (0 - 3)");
                        textInput_1 = GUI.TextField(new Rect(Screen.width / 2 - 50, 105, 100, 20), textInput_1);
                        status = GUIStatus.SetFriend;
                        textOutput = null;
                        break;
                    //Set Friend processing & Output
                    case 93:
                        if (textOutput == null)
                        { textOutput = GameManager.i.dataScript.DebugSetFriend(textInput_0, textInput_1); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 175, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //Set Enemy Input
                    case 94:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 100, 30, 200, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 75, 35, 150, 20), "First Actor slotID (0 to 3)");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 50, 55, 100, 20), textInput_0);
                        GUI.Label(new Rect(Screen.width / 2 - 75, 85, 150, 20), "Second Actor (0 - 3)");
                        textInput_1 = GUI.TextField(new Rect(Screen.width / 2 - 50, 105, 100, 20), textInput_1);
                        status = GUIStatus.SetEnemy;
                        textOutput = null;
                        break;
                    //Set Enemy processing & Output
                    case 95:
                        if (textOutput == null)
                        { textOutput = GameManager.i.dataScript.DebugSetEnemy(textInput_0, textInput_1); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 175, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //Actor Relationship with other Actors
                    case 96:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.playerScript.DebugDisplayPlayerNodeActions();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 800), analysis, customBackground);
                        break;
                    //Set City Loyalty Input
                    case 97:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 100, 30, 200, 60), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 75, 35, 150, 20), "City Loyalty (0 to 10)");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 50, 60, 100, 20), textInput_0);
                        status = GUIStatus.SetLoyalty;
                        textOutput = null;
                        break;
                    //City Loyalty process and output
                    case 98:
                        if (textOutput == null)
                        { textOutput = GameManager.i.cityScript.DebugSetLoyalty(textInput_0); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 175, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //HQ actors Power history
                    case 99:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.hqScript.DebugDisplayHqActorPower();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 500), analysis, customBackground);
                        break;
                    //OnMap Actors History
                    case 100:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayActorsHistory();
                        GUI.Box(new Rect(Screen.width - 455, 10, 450, 900), analysis, customBackground);
                        break;
                    //Player History
                    case 101:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayPlayerHistory();
                        GUI.Box(new Rect(Screen.width - 455, 10, 450, 900), analysis, customBackground);
                        break;
                    //HQ Hierarchy History
                    case 102:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayHqHierarchyHistory();
                        GUI.Box(new Rect(Screen.width - 455, 10, 450, 900), analysis, customBackground);
                        break;
                    //Set Traitor
                    case 103:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 100, 50, 200, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 75, 55, 150, 20), "Actor (0 to 3)");
                        textInput_0 = GUI.TextField(new Rect(Screen.width / 2 - 50, 90, 100, 20), textInput_0);
                        status = GUIStatus.SetTraitor;
                        textOutput = null;
                        break;
                    //Set Mood processing & Output
                    case 104:
                        if (textOutput == null)
                        { textOutput = GameManager.i.actorScript.DebugSetTraitor(textInput_0); }
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 175, 100, 350, 40), textOutput, customBackground);
                        status = GUIStatus.None;
                        break;
                    //MetaGame data
                    case 105:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.metaUIScript.DebugDisplaySelected();
                        GUI.Box(new Rect(Screen.width - 455, 10, 450, 500), analysis, customBackground);
                        break;
                    //Secret Details Display
                    case 106:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplaySecretDetails();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 800), analysis, customBackground);
                        break;
                    //Campaign history
                    case 107:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayCampaignHistory();
                        GUI.Box(new Rect(Screen.width - 455, 10, 450, 900), analysis, customBackground);
                        break;
                    //Gear Dict Display
                    case 108:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayGearDict();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 750), analysis, customBackground);
                        break;
                    //Story Data Display
                    case 109:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.topicScript.DebugDisplayStoryData();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 750), analysis, customBackground);
                        break;
                    //Story Topic Display -> Alpha
                    case 110:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.topicScript.DebugDisplayStoryTopics(StoryType.Alpha);
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 800), analysis, customBackground);
                        break;
                    //MegaCorp Relations Data
                    case 111:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayMegaCorpRelations();
                        GUI.Box(new Rect(Screen.width - 455, 10, 450, 600), analysis, customBackground);
                        break;
                    //Story Topic Display -> Bravo
                    case 112:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.topicScript.DebugDisplayStoryTopics(StoryType.Bravo);
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 800), analysis, customBackground);
                        break;
                    //Story Topic Display -> Charlie
                    case 113:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.topicScript.DebugDisplayStoryTopics(StoryType.Charlie);
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 800), analysis, customBackground);
                        break;
                    //Billboards
                    case 114:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.newsScript.DebugDisplayBillboards();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 800), analysis, customBackground);
                        break;
                    //Tutorial Data
                    case 115:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.tutorialScript.DebugDisplayTutorialData();
                        GUI.Box(new Rect(Screen.width - 460, 10, 450, 700), analysis, customBackground);
                        break;
                    //Actors with contacts at Nodes data
                    case 116:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.contactScript.DebugDisplayNodeContactsActors();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 800), analysis, customBackground);
                        break;
                    //Campaign news
                    case 117:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayCampaignNews();
                        GUI.Box(new Rect(Screen.width - 1210, 10, 1200, 900), analysis, customBackground);
                        break;
                    //Scenario news
                    case 118:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayScenarioNews();
                        GUI.Box(new Rect(Screen.width - 1210, 10, 1200, 900), analysis, customBackground);
                        break;
                    //Nodes with Targets
                    case 119:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.dataScript.DebugDisplayListOfNodesWithTargets();
                        GUI.Box(new Rect(Screen.width - 555, 10, 550, 600), analysis, customBackground);
                        break;
                    //Help Messages
                    case 120:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.i.helpScript.DebugDisplayHelpMessages();
                        GUI.Box(new Rect(Screen.width - 555, 10, 550, 900), analysis, customBackground);
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
                    case GUIStatus.GiveCaptureTool:
                        debugDisplay = 85;
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
                    case GUIStatus.Conflict:
                        debugDisplay = 82;
                        break;
                    case GUIStatus.SetInnocence:
                        debugDisplay = 87;
                        break;
                    case GUIStatus.SetMood:
                        debugDisplay = 89;
                        break;
                    case GUIStatus.SetFriend:
                        debugDisplay = 93;
                        break;
                    case GUIStatus.SetEnemy:
                        debugDisplay = 95;
                        break;
                    case GUIStatus.SetLoyalty:
                        debugDisplay = 98;
                        break;
                    case GUIStatus.SetTraitor:
                        debugDisplay = 104;
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
