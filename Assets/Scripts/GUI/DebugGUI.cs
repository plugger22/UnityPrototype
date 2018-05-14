using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using packageAPI;

/// <summary>
/// debug GUI, on demand (HotKey 'D' -> On, HotKey 'H' -> Off)
/// </summary>
public class DebugGUI : MonoBehaviour
{
    private enum GUIStatus { None, GiveGear, GiveCondition, GiveActorTrait}

    public GUIStyle customBackground;

    private GUIStatus status;
    private MessageCategory msgStatus;
    private AIDebugData aiStatus;
    private TeamDebug teamStatus;
    private bool showGUI = false;
    private int debugDisplay = 0;

    public int button_height;
    public int offset_x;        //space between left edge of box and button
    public int offset_y;        //space between buttons
    public int box_x;
    public int box_option;      //x coord for the options box
    public int box_action;      //x coord for the actions box
    public int box_y;
    public int box_width;
    public int box_height;
    public int gap_y;           //gap at start between header and first button

    private int button_width;
    private string textInput_0 = "what";
    private string textInput_1 = "who";
    private string textOutput;
    private string optionAutoGear;
    private string optionFogOfWar;
    private string optionConnectorTooltips;
    private string optionDebugData;
    private string optionNoAI;

    private void Awake()
    {
        button_width = box_width - (2 * offset_x);
        box_option = box_x * 2 + box_width;
        box_action = box_x * 3 + box_width * 2;
        //option strings
        optionAutoGear = "Auto Gear ON";
        optionFogOfWar = "Fog Of War ON";
        optionConnectorTooltips = "Conn tooltips ON";
        optionDebugData = "Debug Data ON";
        optionNoAI = "NO AI ON";
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

    }

    private void OnGUI()
    {
        if (showGUI == true)
        {
            //
            // - - - Left Hand side of Screen - - -
            //


            customBackground.alignment = TextAnchor.UpperCenter;
            //background box (Data)
            GUI.Box(new Rect(box_x, box_y, box_width, box_height), string.Format("Debug Menu{0}Turn {1}", "\n", GameManager.instance.turnScript.Turn), customBackground);
            //background box (Options)
            GUI.Box(new Rect(box_option, box_y, box_width, box_height), string.Format("Option Menu{0}Action {1} of {2}", "\n",
                GameManager.instance.turnScript.GetActionsCurrent(), GameManager.instance.turnScript.GetActionsTotal()), customBackground);
            //background box (Actions)
            GUI.Box(new Rect(box_action, box_y, box_width, box_height), "Action Menu", customBackground);

            //
            // - - - Data (first box)
            //

            //first button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 0 + button_height * 0, button_width, button_height), "Help"))
            {
                Debug.Log("Button -> Toggle Help");
                if (debugDisplay != 13)
                { debugDisplay = 13; }
                else { debugDisplay = 0; }
            }

            //second button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 1 + button_height * 1, button_width, button_height), "Actor Lists"))
            {
                Debug.Log("Button -> Toggle Actor Lists");
                if (debugDisplay != 7)
                { debugDisplay = 7; }
                else { debugDisplay = 0; }
            }

            //third button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 2 + button_height * 2, button_width, button_height), "Actor Pools"))
            {
                Debug.Log("Button -> Toggle Actor Pools");
                if (debugDisplay != 6)
                { debugDisplay = 6; }
                else { debugDisplay = 0; }

            }

            //fourth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 3 + button_height * 3, button_width, button_height), "Player Stats"))
            {
                Debug.Log("Button -> Toggle Player Stats");
                if (debugDisplay != 8)
                { debugDisplay = 8; }
                else { debugDisplay = 0; }
            }

            //fifth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 4 + button_height * 4, button_width, button_height), "Game State"))
            {
                Debug.Log("Button -> Toggle Game State");
                if (debugDisplay != 5)
                { debugDisplay = 5; }
                else { debugDisplay = 0; }
            }


            //sixth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 5 + button_height * 5, button_width, button_height), "Node Analysis"))
            {
                Debug.Log("Button -> Toggle Node Analysis");
                if (debugDisplay != 1)
                { debugDisplay = 1; }
                else { debugDisplay = 0; }
            }

            //seventh button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 6 + button_height * 6, button_width, button_height), "Actions Register"))
            {
                Debug.Log("Button -> Toggle Actions Register");
                if (debugDisplay != 15)
                { debugDisplay = 15; }
                else { debugDisplay = 0; }
            }

            //eigth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 7 + button_height * 7, button_width, button_height), "Ongoing Register"))
            {
                Debug.Log("Button -> Toggle OngoingID Register");
                if (debugDisplay != 14)
                { debugDisplay = 14; }
                else { debugDisplay = 0; }
            }

            //ninth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 8 + button_height * 8, button_width, button_height), "Factions"))
            {
                Debug.Log("Button -> Toggle Factions");
                if (debugDisplay != 11)
                { debugDisplay = 11; }
                else { debugDisplay = 0; }
            }

            //tenth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 9 + button_height * 9, button_width, button_height), ""))
            {

            }

            //eleventh button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 10 + button_height * 10, button_width, button_height), "Toggle Teams"))
            {
                Debug.Log("Button -> Toggle Team Data");
                //toggles sequentially through team data displays and then switches off
                switch (teamStatus)
                {
                    case TeamDebug.None: debugDisplay = 2; teamStatus = TeamDebug.Pools; break;
                    case TeamDebug.Pools: debugDisplay = 2; teamStatus = TeamDebug.Roster; break;
                    case TeamDebug.Roster:
                        if (GameManager.instance.sideScript.authorityOverall == SideState.Player)
                        { debugDisplay = 2; teamStatus = TeamDebug.Actors; }
                        else { debugDisplay = 0; teamStatus = TeamDebug.None; }
                        break;
                    case TeamDebug.Actors: debugDisplay = 0; teamStatus = TeamDebug.None; break;
                }
            }

            //twelth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 11 + button_height * 11, button_width, button_height), "Toggle Messages"))
            {
                Debug.Log("Button -> Toggle Messages");
                //toggles sequentially through message dictionaries and then switches off
                switch(msgStatus)
                {
                    case MessageCategory.None: debugDisplay = 9; msgStatus = MessageCategory.Pending;  break;
                    case MessageCategory.Pending: debugDisplay = 9; msgStatus = MessageCategory.Current; break;
                    case MessageCategory.Current: debugDisplay = 9; msgStatus = MessageCategory.Archive; break;
                    case MessageCategory.Archive: debugDisplay = 9; msgStatus = MessageCategory.AI; break;
                    case MessageCategory.AI: debugDisplay = 0; msgStatus = MessageCategory.None; break;
                }
            }
        

            //thirteenth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 12 + button_height * 12, button_width, button_height), "Toggle AI Data"))
            {
                Debug.Log("Button -> AI Data");
                /*if (debugDisplay != 10)
                { debugDisplay = 10; }
                else { debugDisplay = 0; }*/
                switch(aiStatus)
                {
                    case AIDebugData.None: debugDisplay = 10; aiStatus = AIDebugData.Task; break;
                    case AIDebugData.Task: debugDisplay = 10; aiStatus = AIDebugData.Node; break;
                    case AIDebugData.Node: debugDisplay = 10; aiStatus = AIDebugData.Spider; break;
                    case AIDebugData.Spider: debugDisplay = 10; aiStatus = AIDebugData.Erasure; break;
                    case AIDebugData.Erasure: debugDisplay = 0; aiStatus = AIDebugData.None; break;
                }
            }

            
            //fourteenth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 13 + button_height * 13, button_width, button_height), ""))
            {

            }


            //
            // - - - Options (second box)
            //

            //first button
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 0 + button_height * 0, button_width, button_height), "Show Options"))
            {
                Debug.Log("Button -> Show Options");
                if (debugDisplay != 12)
                { debugDisplay = 12; }
                else { debugDisplay = 0; }
            }

            //second button
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 1 + button_height * 1, button_width, button_height), optionAutoGear))
            {
                Debug.Log("Button -> Toggle OptionAutoGear");
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
                Debug.Log("Button -> Toggle Fog Of War");
                if (GameManager.instance.optionScript.fogOfWar == true)
                {
                    GameManager.instance.optionScript.fogOfWar = false;
                    optionFogOfWar = "Fog Of War ON";
                }
                else
                {
                    GameManager.instance.optionScript.fogOfWar = true;
                    optionFogOfWar = "Fog Of War OFF";
                }
            }

            //fourth button
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 3 + button_height * 3, button_width, button_height), optionConnectorTooltips))
            {
                Debug.Log("Button -> Toggle Connection Tooltips");
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
                Debug.Log("Button -> Toggle Debug Data");
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
                    Debug.Log("Button -> Toggle NO AI");
                    //option only available on first turn
                    if (GameManager.instance.turnScript.Turn == 0)
                    {
                        //switch AI Off -> Manual player control for both sides
                        if (GameManager.instance.sideScript.authorityOverall == SideState.AI)
                        {
                            optionNoAI = "NO AI OFF";
                            GameManager.instance.optionScript.noAI = true;
                            GameManager.instance.sideScript.authorityCurrent = SideState.Player;
                            GameManager.instance.sideScript.resistanceCurrent = SideState.Player;
                            GameManager.instance.sideScript.authorityOverall = SideState.Player;
                            GameManager.instance.sideScript.resistanceOverall = SideState.Player;
                            //notification
                            GameManager.instance.guiScript.SetAlertMessage(AlertType.DebugAI);
                        }
                        //reverts back to Resistance Player, Authority AI
                        else if (GameManager.instance.sideScript.authorityOverall == SideState.Player)
                        {
                            optionNoAI = "NO AI ON";
                            GameManager.instance.optionScript.noAI = false;
                            GameManager.instance.sideScript.authorityCurrent = SideState.AI;
                            GameManager.instance.sideScript.resistanceCurrent = SideState.Player;
                            GameManager.instance.sideScript.authorityOverall = SideState.AI;
                            GameManager.instance.sideScript.resistanceOverall = SideState.Player;
                            //notification
                            GameManager.instance.guiScript.SetAlertMessage(AlertType.DebugPlayer);
                        }
                    }
                }
            }

            //
            // - - - Actions (third box)
            //

            //first button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 0 + button_height * 0, button_width, button_height), "Swap Sides"))
            {
                Debug.Log("Button -> Swap sides");
                if (GameManager.instance.inputScript.GameState == GameState.Normal)
                {
                    if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
                    { GameManager.instance.sideScript.PlayerSide = GameManager.instance.globalScript.sideAuthority; }
                    else { GameManager.instance.sideScript.PlayerSide = GameManager.instance.globalScript.sideResistance; }
                    //redraw nodes (player node shown/not shown depending on side)
                    GameManager.instance.nodeScript.RedrawNodes();
                }
            }

            //second button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 1 + button_height * 1, button_width, button_height), "Recruit Authority"))
            {
                Debug.Log("Button -> Authority Recruit Actor");
                if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
                { GameManager.instance.actorScript.RecruitActor(2); }
            }

            //third button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 2 + button_height * 2, button_width, button_height), "Remove Tracer"))
            {
                Debug.Log(string.Format("Button -> Toggle Remove Tracer at nodeID {0}{1}", GameManager.instance.nodeScript.nodePlayer, "\n"));
                if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
                {
                    Node node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
                    if (node != null) { node.RemoveTracer(); } else { Debug.LogError("Invalid current Player node (Null)"); }
                }
            }

            //fourth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 3 + button_height * 3, button_width, button_height), "Release Player"))
            {
                Debug.Log("Button -> Release Player");
                if (GameManager.instance.playerScript.status == ActorStatus.Captured)
                { GameManager.instance.captureScript.ReleasePlayer(); }
            }

            //fifth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 4 + button_height * 4, button_width, button_height), "Release Actor"))
            {
                //will release a captured actor each time pressed, nothing happens if no captured actors are present
                Debug.Log("Button -> Release Actor");
                int numOfActors = GameManager.instance.actorScript.maxNumOfOnMapActors;
                if (GameManager.instance.actorScript.numOfActiveActors < numOfActors)
                {
                    for (int i = 0; i < numOfActors; i++)
                    {
                        Actor actor = GameManager.instance.dataScript.GetCurrentActor(i, GameManager.instance.globalScript.sideResistance);
                        if (actor.Status == ActorStatus.Captured)
                        {
                            CaptureDetails details = new CaptureDetails();
                            details.actor = actor;
                            GameManager.instance.captureScript.ReleaseActor(details);
                            break;
                        }
                    }
                }
            }

            //sixth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 5 + button_height * 5, button_width, button_height), "Remove Ongoing"))
            {
                //removes connection Security ongoing effects (first entry in register dict)
                Debug.Log("Button -> Remove Ongoing");
                GameManager.instance.dataScript.RemoveOngoingEffects();
            }

            //seventh button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 6 + button_height * 6, button_width, button_height), "Most Connected"))
            {
                Debug.Log("Button -> Show Most Connected Nodes");
                EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.MostConnected);
            }

            //eighth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 7 + button_height * 7, button_width, button_height), "Centre Nodes"))
            {
                Debug.Log("Button -> Centre Nodes");
                GameManager.instance.debugGraphicsScript.SetCentrePane(true);
                EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.Centre);
            }

            //ninth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 8 + button_height * 8, button_width, button_height), "Near Neighbours"))
            {
                Debug.Log("Button -> NearNeighbours");
                EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NearNeighbours);
            }

            //tenth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 9 + button_height * 9, button_width, button_height), "Give Gear"))
            {
                //Resistance player only
                if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
                {
                    Debug.Log("Button -> Give Gear");
                    if (debugDisplay != 16)
                    { debugDisplay = 16; }
                    else { debugDisplay = 0; }
                }
            }
            //eleventh button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 10 + button_height * 10, button_width, button_height), "Give Condition"))
            {
                Debug.Log("Button -> Give Condition");
                if (debugDisplay != 18)
                { debugDisplay = 18; }
                else { debugDisplay = 0; }
            }
            //twelfth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 11 + button_height * 11, button_width, button_height), "Give Trait"))
            {
                Debug.Log("Button -> Give Actor Trait");
                if (debugDisplay != 20)
                { debugDisplay = 20; }
                else { debugDisplay = 0; }
            }


            //
            // - - - Analysis at Right Hand side of Screen - - -
            //

            if (debugDisplay > 0)
            {
                string analysis;
                switch (debugDisplay)
                {
                    //general analysis of nodes and actors
                    case 1:
                        //graph data, far right
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.levelScript.GetGraphAnalysis();
                        GUI.Box(new Rect(Screen.width - 155, 10, 150, 200), analysis, customBackground);
                        // Node Type data, near centre right
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.levelScript.GetNodeAnalysis();
                        GUI.Box(new Rect(Screen.width - 305, 10, 150, 200), analysis, customBackground);
                        break;
                    //toggle team data
                    case 2:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        /*analysis = GameManager.instance.teamScript.DisplayTeamAnalysis();
                        GUI.Box(new Rect(Screen.width - 205, 10, 200, 240), analysis, customBackground);*/
                        switch (teamStatus)
                        {
                            case TeamDebug.Pools:
                                analysis = GameManager.instance.teamScript.DisplayTeamAnalysis();
                                GUI.Box(new Rect(Screen.width - 205, 10, 200, 240), analysis, customBackground);
                                break;
                            case TeamDebug.Roster:
                                analysis = GameManager.instance.teamScript.DisplayIndividualTeams();
                                GUI.Box(new Rect(Screen.width - 405, 10, 400, 320), analysis, customBackground);
                                break;
                            case TeamDebug.Actors:
                                analysis = GameManager.instance.teamScript.DisplayTeamActorAnalysis();
                                GUI.Box(new Rect(Screen.width - 205, 10, 200, 280), analysis, customBackground);
                                break;
                        }
                        break;
                    //teams
                    case 3:
                        /*customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.teamScript.DisplayIndividualTeams();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 320), analysis, customBackground);*/
                        break;
                    //actor Teams
                    case 4:
                        /*customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.teamScript.DisplayTeamActorAnalysis();
                        GUI.Box(new Rect(Screen.width - 205, 10, 200, 280), analysis, customBackground);*/
                        break;
                    //Game state
                    case 5:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.inputScript.DisplayGameState();
                        GUI.Box(new Rect(Screen.width - 205, 10, 200, 400), analysis, customBackground);
                        break;
                    //actor Pools
                    case 6:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.actorScript.DisplayPools();
                        GUI.Box(new Rect(Screen.width - 205, 10, 200, 900), analysis, customBackground);
                        break;
                    //actor Lists
                    case 7:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DisplayActorLists();
                        GUI.Box(new Rect(Screen.width - 305, 10, 300, 450), analysis, customBackground);
                        break;
                    //Player stats
                    case 8:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.playerScript.DisplayPlayerStats();
                        GUI.Box(new Rect(Screen.width - 205, 10, 200, 400), analysis, customBackground);
                        break;
                    //Toggle Messages
                    case 9:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = "Unknown";
                        switch(msgStatus)
                        {
                            case MessageCategory.Pending:
                                analysis = GameManager.instance.dataScript.DisplayMessages(MessageCategory.Pending);
                                break;
                            case MessageCategory.Current:
                                analysis = GameManager.instance.dataScript.DisplayMessages(MessageCategory.Current);
                                break;
                            case MessageCategory.Archive:
                                analysis = GameManager.instance.dataScript.DisplayMessages(MessageCategory.Archive);
                                break;
                            case MessageCategory.AI:
                                analysis = GameManager.instance.dataScript.DisplayMessages(MessageCategory.AI);
                                break;
                        }
                        GUI.Box(new Rect(Screen.width - 460, 10, 450, 1000), analysis, customBackground);
                        break;
                    //Toggle AI Data
                    case 10:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = "Unknown";
                        switch(aiStatus)
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
                        }
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 500), analysis, customBackground);
                        break;
                    //Factions
                    case 11:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.factionScript.DisplayFactions();
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
                        GUI.Box(new Rect(Screen.width - 205, 10, 200, 500), analysis, customBackground);
                        break;
                    //Ongoing Register
                    case 14:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DisplayOngoingRegister();
                        GUI.Box(new Rect(Screen.width - 460, 10, 450, 350), analysis, customBackground);
                        break;
                    //Actions Adjustment Register
                    case 15:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DisplayActionsRegister();
                        GUI.Box(new Rect(Screen.width - 460, 10, 450, 350), analysis, customBackground);
                        break;
                    //Give Gear Input
                    case 16:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 100, 50, 200, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 75, 55, 150, 20), "Input Gear name (exact)");
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
                        GUI.Box(new Rect(Screen.width / 2 - 400, 50, 200, 100), "", customBackground);
                        GUI.Label(new Rect(Screen.width / 2 - 395, 55, 190, 20), "Input Trait name (case sensistive)");
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
                    case GUIStatus.GiveActorTrait:
                        debugDisplay = 21;
                        break;
                }
                break;
            case KeyCode.Escape:
                debugDisplay = 0;
                break;
        }
        e.keyCode = KeyCode.None;
    }


}
