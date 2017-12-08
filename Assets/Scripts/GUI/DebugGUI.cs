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
            GUI.Box(new Rect(10, 10, 90, 220), "Debug Menu", customBackground);

            //first button
            if (GUI.Button(new Rect(15, 40, 80, 20), "A_Recruit"))
            {
                Debug.Log("Button -> Authority Recruit Actor");
                ModalActionDetails details = new ModalActionDetails()
                {
                    side = Side.Authority,
                    NodeID = -1,
                    ActorSlotID = -1,
                    EventType = EventType.GenericRecruitActorAuthority
                };
                EventManager.instance.PostNotification(EventType.RecruitAction, this, details);
            }

            //second button
            if (GUI.Button(new Rect(15, 60, 80, 20), "Reserves"))
            {
                Debug.Log("Button -> Toggle Reserve Lists");
                if (debugDisplay != 7)
                { debugDisplay = 7; }
                else { debugDisplay = 0; }
            }

            //third button
            if (GUI.Button(new Rect(15, 80, 80, 20), "Gear"))
            {
                Debug.Log("Button -> Toggle Gear");
                if (debugDisplay != 5)
                { debugDisplay = 5; }
                else { debugDisplay = 0; }
                
            }

            //fourth button
            if (GUI.Button(new Rect(15, 100, 80, 20), "Colour Blind"))
            {
                Debug.Log("Button -> Toggle Colour Schemes");
                if (GameManager.instance.optionScript.ColourOption == ColourScheme.Normal)
                { GameManager.instance.optionScript.ColourOption = ColourScheme.ColourBlind; }
                else { GameManager.instance.optionScript.ColourOption = ColourScheme.Normal; }
            }

            //fifth button
            if (GUI.Button(new Rect(15, 120, 80, 20), "Side"))
            {
                Debug.Log("Button -> Swap sides");
                if (GameManager.instance.optionScript.PlayerSide == Side.Resistance)
                { GameManager.instance.optionScript.PlayerSide = Side.Authority; }
                else { GameManager.instance.optionScript.PlayerSide = Side.Resistance; }
            }


            //sixth button
            if (GUI.Button(new Rect(15, 140, 80, 20), "Node/Actrs"))
            {
                Debug.Log("Button -> Toggle Node/Actors Analysis");
                if (debugDisplay != 1)
                { debugDisplay = 1; }
                else { debugDisplay = 0; }
            }

            //seventh button
            if (GUI.Button(new Rect(15, 160, 80, 20), "Team Pools"))
            {
                Debug.Log("Button -> Toggle Team Pool Analysis");
                if (debugDisplay != 2)
                { debugDisplay = 2; }
                else { debugDisplay = 0; }
            }

            //eigth button
            if (GUI.Button(new Rect(15, 180, 80, 20), "Teams"))
            {
                Debug.Log("Button -> Toggle Teams");
                if (debugDisplay != 3)
                { debugDisplay = 3; }
                else { debugDisplay = 0; }
            }

            //ninth button
            if (GUI.Button(new Rect(15, 200, 80, 20), "Actr Teams"))
            {
                Debug.Log("Button -> Toggle Actor Teams");
                if (debugDisplay != 4)
                { debugDisplay = 4; }
                else { debugDisplay = 0; }
            }

            //tenth button
            if (GUI.Button(new Rect(15, 220, 80, 20), "Actor Pools"))
            {
                Debug.Log("Button -> Toggle Actor Pools");
                if (debugDisplay != 6)
                { debugDisplay = 6; }
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
                }
            }
            

        }
    }
}
