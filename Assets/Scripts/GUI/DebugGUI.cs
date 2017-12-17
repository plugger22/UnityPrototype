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
    public GUIStyle customBackground;

    private bool showGUI = false;
    private int debugDisplay = 0;

    public int button_height;
    public int offset_x;        //space between left edge of box and button
    public int offset_y;        //space between buttons
    public int box_x;
    public int box_y;
    public int box_width;
    public int box_height;
    public int gap_y;           //gap at start between header and first button

    private int button_width;

    private void Awake()
    {
        button_width = box_width - (2 * offset_x);
    }

    // Update is called once per frame
    private void Update ()
    {
		if (Input.GetKeyDown("d") == true)
        { showGUI = true; }
        if (Input.GetKeyDown("h") == true)
        { showGUI = false; }
	}

    private void OnGUI()
    {
        if (showGUI == true)
        {
            //
            // - - - Left Hand side of Screen - - -
            //

            //background box
            customBackground.alignment = TextAnchor.UpperCenter;
            GUI.Box(new Rect(box_x, box_y, box_width, box_height), string.Format("Debug Menu{0}Turn {1}", "\n", GameManager.instance.turnScript.Turn), customBackground);

            //first button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 0 + button_height * 0, button_width, button_height), "Authority Recruit"))
            {
                Debug.Log("Button -> Authority Recruit Actor");
                if (GameManager.instance.optionScript.PlayerSide == Side.Authority)
                { GameManager.instance.actorScript.RecruitActor(2); }
            }

            //second button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 1 + button_height * 1, button_width, button_height), "Reserve Lists"))
            {
                Debug.Log("Button -> Toggle Reserve Lists");
                if (debugDisplay != 7)
                { debugDisplay = 7; }
                else { debugDisplay = 0; }
            }

            //third button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 2 + button_height * 2, button_width, button_height), "Gear Display"))
            {
                Debug.Log("Button -> Toggle Gear");
                if (debugDisplay != 5)
                { debugDisplay = 5; }
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
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 4 + button_height * 4, button_width, button_height), "Change Side"))
            {
                Debug.Log("Button -> Swap sides");
                if (GameManager.instance.optionScript.PlayerSide == Side.Resistance)
                { GameManager.instance.optionScript.PlayerSide = Side.Authority; }
                else { GameManager.instance.optionScript.PlayerSide = Side.Resistance; }
            }


            //sixth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 5 + button_height * 5, button_width, button_height), "Analysis"))
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
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 7+ button_height * 7, button_width, button_height), "Teams by Type"))
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
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 9 + button_height * 9, button_width, button_height), "Actor Pools"))
            {
                Debug.Log("Button -> Toggle Actor Pools");
                if (debugDisplay != 6)
                { debugDisplay = 6; }
                else { debugDisplay = 0; }
            }

            //eleventh button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 10 + button_height * 10, button_width, button_height), "Pending Messages"))
            {
                Debug.Log("Button -> Toggle Pending Messages");
                if (debugDisplay != 9)
                { debugDisplay = 9; }
                else { debugDisplay = 0; }
            }

            //twelth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 11 + button_height * 11, button_width, button_height), "Archive Messages"))
            {
                Debug.Log("Button -> Toggle Archive Messages");
                if (debugDisplay != 10)
                { debugDisplay = 10; }
                else { debugDisplay = 0; }
            }

            //thirteenth button
            if (GUI.Button(new Rect(box_x + offset_x, box_y + gap_y + offset_y * 12 + button_height * 12, button_width, button_height), "Current Messages"))
            {
                Debug.Log("Button -> Toggle Current Messages");
                if (debugDisplay != 11)
                { debugDisplay = 11; }
                else { debugDisplay = 0; }
            }

            //
            // - - - Analysis at Right Hand side of Screen - - -
            //
            if (debugDisplay > 0)
            {
                switch (debugDisplay)
                {
                    //general analysis of nodes and actors
                    case 1:
                        //graph data, far right
                        customBackground.alignment = TextAnchor.UpperLeft;
                        string analysisNodes = GameManager.instance.levelScript.GetGraphAnalysis();
                        GUI.Box(new Rect(Screen.width - 115, 10, 110, 200), analysisNodes, customBackground);

                        //Actor data, middle right
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysisNodes = GameManager.instance.levelScript.GetActorAnalysis(GameManager.instance.optionScript.PlayerSide);
                        GUI.Box(new Rect(Screen.width - 335, 10, 220, 200), analysisNodes, customBackground);

                        // Node Type data, near centre right
                        customBackground.alignment = TextAnchor.UpperLeft;
                        analysisNodes = GameManager.instance.levelScript.GetNodeAnalysis();
                        GUI.Box(new Rect(Screen.width - 485, 10, 150, 200), analysisNodes, customBackground);
                        break;
                    //team pool analysis
                    case 2:
                        {
                            customBackground.alignment = TextAnchor.UpperLeft;
                            string analysisPools = GameManager.instance.teamScript.GetTeamAnalysis();
                            GUI.Box(new Rect(Screen.width - 205, 10, 200, 240), analysisPools, customBackground);
                        }
                        break;
                    //teams
                    case 3:
                        {
                            customBackground.alignment = TextAnchor.UpperLeft;
                            string analysisTeams = GameManager.instance.teamScript.GetIndividualTeams();
                            GUI.Box(new Rect(Screen.width - 405, 10, 400, 320), analysisTeams, customBackground);
                        }
                        break;
                    //actor Teams
                    case 4:
                        {
                            customBackground.alignment = TextAnchor.UpperLeft;
                            string analysisActors = GameManager.instance.teamScript.GetTeamActorAnalysis();
                            GUI.Box(new Rect(Screen.width - 205, 10, 200, 280), analysisActors, customBackground);
                        }
                        break;
                    //player's gear
                    case 5:
                        {
                            customBackground.alignment = TextAnchor.UpperLeft;
                            string analysisActors = GameManager.instance.playerScript.DisplayGear();
                            GUI.Box(new Rect(Screen.width - 205, 10, 200, 280), analysisActors, customBackground);
                        }
                        break;
                    //actor Pools
                    case 6:
                        {
                            customBackground.alignment = TextAnchor.UpperLeft;
                            string analysisPools = GameManager.instance.actorScript.DisplayPools();
                            GUI.Box(new Rect(Screen.width - 205, 10, 200, 900), analysisPools, customBackground);
                        }
                        break;
                    //Reserve Lists
                    case 7:
                        {
                            customBackground.alignment = TextAnchor.UpperLeft;
                            string analysisPools = GameManager.instance.dataScript.DisplayReserveLists();
                            GUI.Box(new Rect(Screen.width - 205, 10, 200, 240), analysisPools, customBackground);
                        }
                        break;
                    //Player stats
                    case 8:
                        {
                            customBackground.alignment = TextAnchor.UpperLeft;
                            string analysisPools = GameManager.instance.playerScript.DisplayPlayerStats();
                            GUI.Box(new Rect(Screen.width - 205, 10, 200, 240), analysisPools, customBackground);
                        }
                        break;
                    //Pending Messages
                    case 9:
                        {
                            customBackground.alignment = TextAnchor.UpperLeft;
                            string analysisPools = GameManager.instance.dataScript.DisplayMessages(MessageCategory.Pending);
                            GUI.Box(new Rect(Screen.width - 460, 10, 450, 1000), analysisPools, customBackground);
                        }
                        break;
                    //Archive Messages
                    case 10:
                        {
                            customBackground.alignment = TextAnchor.UpperLeft;
                            string analysisPools = GameManager.instance.dataScript.DisplayMessages(MessageCategory.Archive);
                            GUI.Box(new Rect(Screen.width - 460, 10, 450, 1000), analysisPools, customBackground);
                        }
                        break;
                    //Current Messages
                    case 11:
                        {
                            customBackground.alignment = TextAnchor.UpperLeft;
                            string analysisPools = GameManager.instance.dataScript.DisplayMessages(MessageCategory.Current);
                            GUI.Box(new Rect(Screen.width - 460, 10, 450, 500), analysisPools, customBackground);
                        }
                        break;

                }
            }
            

        }
    }
}
