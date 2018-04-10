using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using modalAPI;

/// <summary>
/// debug GUI, on demand (HotKey 'D' -> On, HotKey 'H' -> Off)
/// </summary>
public class DebugGUI : MonoBehaviour
{
    private enum GUIStatus { None, GearInput, Output}

    public GUIStyle customBackground;

    private GUIStatus status = GUIStatus.None;
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
    private string textFieldString = "input";
    private string textOutput;
    private string optionAutoGear;
    private string optionFogOfWar;

    private void Awake()
    {
        button_width = box_width - (2 * offset_x);
        box_option = box_x * 2 + box_width;
        box_action = box_x * 3 + box_width * 2;
        //option strings
        optionAutoGear = "Auto Gear ON";
        optionFogOfWar = "Fog Of War ON";
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
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 5 + button_height * 5, button_width, button_height), "Actor Analysis"))
            {
                Debug.Log("Button -> Toggle Node/Actors Analysis");
                if (debugDisplay != 1)
                { debugDisplay = 1; }
                else { debugDisplay = 0; }
            }

            //seventh button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 6 + button_height * 6, button_width, button_height), "Team Pools"))
            {
                Debug.Log("Button -> Toggle Team Pool Analysis");
                if (debugDisplay != 2)
                { debugDisplay = 2; }
                else { debugDisplay = 0; }
            }

            //eigth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 7 + button_height * 7, button_width, button_height), "Teams by Type"))
            {
                Debug.Log("Button -> Toggle Teams");
                if (debugDisplay != 3)
                { debugDisplay = 3; }
                else { debugDisplay = 0; }
            }

            //ninth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 8 + button_height * 8, button_width, button_height), "Teams by Actor"))
            {
                Debug.Log("Button -> Toggle Actor Teams");
                if (debugDisplay != 4)
                { debugDisplay = 4; }
                else { debugDisplay = 0; }
            }

            //tenth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 9 + button_height * 9, button_width, button_height), "Ongoing Register"))
            {
                Debug.Log("Button -> Toggle OngoingID Register");
                if (debugDisplay != 14)
                { debugDisplay = 14; }
                else { debugDisplay = 0; }
            }

            //eleventh button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 10 + button_height * 10, button_width, button_height), "Actions Register"))
            {
                Debug.Log("Button -> Toggle Actions Register");
                if (debugDisplay != 15)
                { debugDisplay = 15; }
                else { debugDisplay = 0; }
            }

            //twelth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 11 + button_height * 11, button_width, button_height), "Pending Messages"))
            {
                Debug.Log("Button -> Toggle Pending Messages");
                if (debugDisplay != 9)
                { debugDisplay = 9; }
                else { debugDisplay = 0; }
            }

            //thirteenth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 12 + button_height * 12, button_width, button_height), "Archive Messages"))
            {
                Debug.Log("Button -> Toggle Archive Messages");
                if (debugDisplay != 10)
                { debugDisplay = 10; }
                else { debugDisplay = 0; }
            }

            //fourteenth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 13 + button_height * 13, button_width, button_height), "Current Messages"))
            {
                Debug.Log("Button -> Toggle Current Messages");
                if (debugDisplay != 11)
                { debugDisplay = 11; }
                else { debugDisplay = 0; }
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
                            AIDetails details = new AIDetails();
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
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 6 + button_height * 6, button_width, button_height), "Connections LOW"))
            {
                Debug.Log("Button -> Change Connections");
                GameManager.instance.levelScript.ChangeAllConnections(ConnectionType.LOW);
            }

            //eighth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 7 + button_height * 7, button_width, button_height), "Connections MED"))
            {
                Debug.Log("Button -> Change Connections");
                GameManager.instance.levelScript.ChangeAllConnections(ConnectionType.MEDIUM);
            }

            //ninth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 8 + button_height * 8, button_width, button_height), "Connections HIGH"))
            {
                Debug.Log("Button -> Change Connections");
                GameManager.instance.levelScript.ChangeAllConnections(ConnectionType.HIGH);
            }

            //tenth button
            if (GUI.Button(new Rect(box_action + offset_x, box_y + gap_y + offset_y * 9 + button_height * 9, button_width, button_height), "Give Gear"))
            {
                Debug.Log("Button -> Give Gear");
                if (debugDisplay != 16)
                { debugDisplay = 16; }
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
                        GUI.Box(new Rect(Screen.width - 115, 10, 110, 200), analysis, customBackground);

                        //Actor data, middle right
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.levelScript.GetActorAnalysis(GameManager.instance.sideScript.PlayerSide);
                        GUI.Box(new Rect(Screen.width - 335, 10, 220, 200), analysis, customBackground);

                        // Node Type data, near centre right
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.levelScript.GetNodeAnalysis();
                        GUI.Box(new Rect(Screen.width - 485, 10, 150, 200), analysis, customBackground);
                        break;
                    //team pool analysis
                    case 2:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.teamScript.DisplayTeamAnalysis();
                        GUI.Box(new Rect(Screen.width - 205, 10, 200, 240), analysis, customBackground);
                        break;
                    //teams
                    case 3:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.teamScript.DisplayIndividualTeams();
                        GUI.Box(new Rect(Screen.width - 405, 10, 400, 320), analysis, customBackground);
                        break;
                    //actor Teams
                    case 4:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.teamScript.DisplayTeamActorAnalysis();
                        GUI.Box(new Rect(Screen.width - 205, 10, 200, 280), analysis, customBackground);
                        break;
                    //player's gear
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
                    //Pending Messages
                    case 9:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DisplayMessages(MessageCategory.Pending);
                        GUI.Box(new Rect(Screen.width - 460, 10, 450, 1000), analysis, customBackground);
                        break;
                    //Archive Messages
                    case 10:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DisplayMessages(MessageCategory.Archive);
                        GUI.Box(new Rect(Screen.width - 460, 10, 450, 1000), analysis, customBackground);
                        break;
                    //Current Messages
                    case 11:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysis = GameManager.instance.dataScript.DisplayMessages(MessageCategory.Current);
                        GUI.Box(new Rect(Screen.width - 460, 10, 450, 500), analysis, customBackground);
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
                        GUI.Box(new Rect(Screen.width - 205, 10, 200, 400), analysis, customBackground);
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
                    //Give Gear
                    case 16:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        textFieldString = GUI.TextField(new Rect(Screen.width / 2 - 50, 100, 100, 30), textFieldString);
                        status = GUIStatus.GearInput;
                        break;
                    //Give Gear processing
                    case 17:
                        textOutput = GameManager.instance.playerScript.DebugAddGear(textFieldString);
                        debugDisplay = 0;
                        status = GUIStatus.Output;
                        break;
                        //Give Gear Ouput
                    case 18:
                        customBackground.alignment = TextAnchor.UpperLeft;
                        GUI.Box(new Rect(Screen.width / 2 - 175, 100, 350, 40), textOutput, customBackground);
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
                        case GUIStatus.Output:
                            debugDisplay = 18;
                            break;
                        case GUIStatus.GearInput:
                            debugDisplay = 17;
                            break;
                    }
                    break;
                case KeyCode.Escape:
                    debugDisplay = 0;
                    break;
        }
    }


}
