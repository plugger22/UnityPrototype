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
    //for whenever interaction is needed
    private enum GUIStatus { None, GiveGear, GiveCondition, GiveActorTrait, SetState, AddContact, RemoveContact, isKnownContact}

    public GUIStyle customBackground;

    private GUIStatus status;
    private MessageCategory msgStatus;
    private AIDebugData aiStatus;
    private TeamDebug teamStatus;
    private bool showGUI = false;
    private int debugDisplay = 0;

    public int button_height;
    [Tooltip("Space between left edge of box and button")]
    public int offset_x;  
    [Tooltip("Space between buttons")]
    public int offset_y;      
    public int box_x;
    [Tooltip("x coord for the options box")]
    public int box_option;
    [Tooltip("x coord for the actions box")]
    public int box_action;
    [Tooltip("x coord for the levels box")]
    public int box_level; 
    public int box_y;
    public int box_width;
    public int box_height;
    [Tooltip("gap at start between header and first button")]
    public int gap_y; 

    private int button_width;
    private int targetToggle = 0;
    private string textInput_0 = "what";
    private string textInput_1 = "who";
    private string textOutput;
    private string optionAutoGear;
    private string optionFogOfWar;
    private string optionConnectorTooltips;
    private string optionDebugData;
    private string optionNoAI;
    private string optionAIOffline;
    private string optionAITraceback;
    private string optionAIScreamer;
    private string optionRenownUI;

    private void Awake()
    {
        button_width = box_width - (2 * offset_x);
        box_option = box_x * 2 + box_width;
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
        { showGUI = false; }

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
            GUI.Box(new Rect(box_x, box_y, box_width, box_height + 100), "Info Menu", customBackground);
            //background box (Options)
            GUI.Box(new Rect(box_option, box_y, box_width, box_height), "Option Menu", customBackground);
            //background box (Actions)
            GUI.Box(new Rect(box_action, box_y, box_width, box_height + 100), "Action Menu", customBackground);
            //background box (Level)
            GUI.Box(new Rect(box_level, box_y, box_width, box_height / 2), "Level Menu", customBackground);

            //
            // - - - Data (first box)
            //

            //first button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 0 + button_height * 0, button_width, button_height), "Help"))
            {
                Debug.Log("[Dbg] Button -> Toggle Help");
                if (debugDisplay != 13)
                { debugDisplay = 13; }
                else { debugDisplay = 0; }
            }

            //second button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 1 + button_height * 1, button_width, button_height), "Actor Lists"))
            {
                Debug.Log("[Dbg] Button -> Toggle Actor Lists");
                if (debugDisplay != 7)
                { debugDisplay = 7; }
                else { debugDisplay = 0; }
            }

            //third button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 2 + button_height * 2, button_width, button_height), "Actor Pools"))
            {
                Debug.Log("[Dbg] Button -> Toggle Actor Pools");
                if (debugDisplay != 6)
                { debugDisplay = 6; }
                else { debugDisplay = 0; }
            }

            //fourth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 3 + button_height * 3, button_width, button_height), "Player Stats"))
            {
                Debug.Log("[Dbg] Button -> Toggle Player Stats");
                if (debugDisplay != 8)
                { debugDisplay = 8; }
                else { debugDisplay = 0; }
            }

            //fifth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 4 + button_height * 4, button_width, button_height), "Game State"))
            {
                Debug.Log("[Dbg] Button -> Toggle Game State");
                if (debugDisplay != 5)
                { debugDisplay = 5; }
                else { debugDisplay = 0; }
            }


            //sixth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 5 + button_height * 5, button_width, button_height), "City Analysis"))
            {
                Debug.Log("[Dbg] Button -> Toggle City Analysis");
                if (debugDisplay != 1)
                { debugDisplay = 1; }
                else { debugDisplay = 0; }
            }

            //seventh button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 6 + button_height * 6, button_width, button_height), "Actions Register"))
            {
                Debug.Log("[Dbg] Button -> Toggle Actions Register");
                if (debugDisplay != 15)
                { debugDisplay = 15; }
                else { debugDisplay = 0; }
            }

            //eigth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 7 + button_height * 7, button_width, button_height), "Ongoing Register"))
            {
                Debug.Log("[Dbg] Button -> Toggle OngoingID Register");
                if (debugDisplay != 14)
                { debugDisplay = 14; }
                else { debugDisplay = 0; }
            }

            //ninth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 8 + button_height * 8, button_width, button_height), "Factions"))
            {
                Debug.Log("[Dbg] Button -> Toggle Factions");
                if (debugDisplay != 11)
                { debugDisplay = 11; }
                else { debugDisplay = 0; }
            }

            //tenth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 9 + button_height * 9, button_width, button_height), "AI Reboot"))
            {
                Debug.Log("[Dbg] Button -> Force AI Reboot");
                GameManager.instance.aiScript.RebootCommence();
            }

            //eleventh button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 10 + button_height * 10, button_width, button_height), "Toggle Teams"))
            {
                Debug.Log("[Dbg] Button -> Toggle Team Data");
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
                Debug.Log("[Dbg] Button -> Toggle Messages");
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
                Debug.Log("[Dbg] Button -> AI Data");
                switch(aiStatus)
                {
                    case AIDebugData.None: debugDisplay = 10; aiStatus = AIDebugData.Task; break;
                    case AIDebugData.Task: debugDisplay = 10; aiStatus = AIDebugData.Node; break;
                    case AIDebugData.Node: debugDisplay = 10; aiStatus = AIDebugData.Spider; break;
                    case AIDebugData.Spider: debugDisplay = 10; aiStatus = AIDebugData.Erasure; break;
                    case AIDebugData.Erasure: debugDisplay = 10; aiStatus = AIDebugData.Decision; break;
                    case AIDebugData.Decision: debugDisplay = 0; aiStatus = AIDebugData.None; break;
                }
            }

            //fourteenth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 13 + button_height * 13, button_width, button_height), "Gear Data"))
            {
                Debug.Log("[Dbg] Button -> Gear Data");
                if (debugDisplay != 24)
                { debugDisplay = 24; }
                else { debugDisplay = 0; }
            }

            //fifteenth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 14 + button_height * 14, button_width, button_height), "Secret Data"))
            {
                Debug.Log("[Dbg] Button -> Secret Data");
                if (debugDisplay != 25)
                { debugDisplay = 25; }
                else { debugDisplay = 0; }
            }

            //sixteenth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 15 + button_height * 15, button_width, button_height), "Crisis Data"))
            {
                Debug.Log("[Dbg] Button -> Crisis Data");
                if (debugDisplay != 26)
                { debugDisplay = 26; }
                else { debugDisplay = 0; }
            }

            //sevenTeenth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 16 + button_height * 16, button_width, button_height), "Contacts"))
            {
                //resistance only
                if (GameManager.instance.turnScript.currentSide.level == GameManager.instance.globalScript.sideResistance.level)
                {
                    Debug.Log("[Dbg] Button -> Toggle Contacts");
                    if (debugDisplay != 33)
                    { debugDisplay = 33; }
                    else { debugDisplay = 0; }
                }
            }

            //eightennth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 17 + button_height * 17, button_width, button_height), "Toggle Targets"))
            {
                Debug.Log("[Dbg] Button -> Toggle Targets");
                //toggles sequentially through target info displays and then switches off
                switch (targetToggle)
                {
                    case 0: debugDisplay = 34; targetToggle = 1; break;
                    case 1: debugDisplay = 35; targetToggle = 2; break;
                    case 2: debugDisplay = 36; targetToggle = 3; break;
                    case 3: debugDisplay = 0; targetToggle = 0; break;
                }
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
                                    GameManager.instance.sideScript.authorityCurrent = SideState.Player;
                                    GameManager.instance.sideScript.resistanceCurrent = SideState.Player;
                                    GameManager.instance.sideScript.authorityOverall = SideState.Player;
                                    GameManager.instance.sideScript.resistanceOverall = SideState.Player;
                                    //notification
                                    GameManager.instance.guiScript.SetAlertMessage(AlertType.DebugAI);
                                }
                                //reverts back to Authority Player, Resistance AI
                                else if (GameManager.instance.sideScript.resistanceOverall == SideState.Player)
                                {
                                    optionNoAI = "NO AI ON";
                                    GameManager.instance.optionScript.noAI = false;
                                    GameManager.instance.sideScript.authorityCurrent = SideState.Player;
                                    GameManager.instance.sideScript.resistanceCurrent = SideState.AI;
                                    GameManager.instance.sideScript.authorityOverall = SideState.Player;
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
                                break;
                        }
                    }
                }
            }

            //tenth button
            if (GUI.Button(new Rect(box_option + offset_x, box_y + gap_y + offset_y * 6 + button_height * 6, button_width, button_height), optionRenownUI))
            {
                Debug.Log("[Dbg] Button -> Toggle Renown Display");
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

            //
            // - - - Actions (third box)
            //

            //first button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 0 + button_height * 0, button_width, button_height), "Swap Sides"))
            {
                Debug.Log("[Dbg] Button -> Swap sides");
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
                Debug.Log("[Dbg] Button -> Authority Recruit Actor");
                if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
                { GameManager.instance.actorScript.RecruitActor(2); }
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
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 4 + button_height * 4, button_width, button_height), "Release Actor"))
            {
                //will release a captured actor each time pressed, nothing happens if no captured actors are present
                Debug.Log("[Dbg] Button -> Release Actor");
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
                Debug.Log("[Dbg] Button -> Remove Ongoing");
                GameManager.instance.dataScript.RemoveOngoingEffects();
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
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 15 + button_height * 15, button_width, button_height), "Give Secret"))
            {
                Debug.Log("[Dbg] Button -> Give Player a Random Secret");
                GameManager.instance.playerScript.DebugAddRandomSecret();
            }

            //fourteenth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 16 + button_height * 16, button_width, button_height), "Add Contact"))
            {
                Debug.Log("[Dbg] Button -> Add Contact");
                if (debugDisplay != 27)
                { debugDisplay = 27; }
                else { debugDisplay = 0; }
            }

            //fifteenth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 17 + button_height * 17, button_width, button_height), "Remove Contact"))
            {
                Debug.Log("[Dbg] Button -> Remove Contact");
                if (debugDisplay != 29)
                { debugDisplay = 29; }
                else { debugDisplay = 0; }
            }


            //sixteenth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 18 + button_height * 18, button_width, button_height), "isContactKnown"))
            {
                Debug.Log("[Dbg] Button -> Set isContactKnown True");
                if (debugDisplay != 31)
                { debugDisplay = 31; }
                else { debugDisplay = 0; }
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

            //
            // - - - Analysis at Right Hand side of Screen - - -
            //

            if (debugDisplay > 0)
            {
                string analysis;
                switch (debugDisplay)
                {
                    //City / Level analysis
                    case 1:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.levelScript.GetLevelAnalysis();
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
                    //AI Display ON
                    case 3:
                        break;
                    //AI Display OFF
                    case 4:
                        break;
                    //Game state
                    case 5:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.inputScript.DisplayGameState();
                        GUI.Box(new Rect(Screen.width - 255, 10, 250, 400), analysis, customBackground);
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
                            case AIDebugData.Decision:
                                analysis = GameManager.instance.aiScript.DisplayDecisionData();
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
                        GUI.Box(new Rect(Screen.width - 305, 10, 300, 800), analysis, customBackground);
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
                        analysis = GameManager.instance.dataScript.DisplayGearData();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 750), analysis, customBackground);
                        break;
                    //Secret Info Display
                    case 25:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.secretScript.DisplaySecretData();
                        GUI.Box(new Rect(Screen.width - 410, 10, 400, 750), analysis, customBackground);
                        break;
                    //Node Crisis Data Display
                    case 26:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DisplayCrisisNodes();
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
                        analysis = GameManager.instance.contactScript.DisplayContacts();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 600), analysis, customBackground);
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
