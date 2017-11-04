using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

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
            if (GUI.Button(new Rect(15, 40, 80, 20), "Sec Low"))
            {
                Debug.Log("Button -> Switch Connections to Low Security");
                GameManager.instance.levelScript.ChangeAllConnections(ConnectionType.LowSec);
            }

            //second button
            if (GUI.Button(new Rect(15, 60, 80, 20), "Sec Med"))
            {
                Debug.Log("Button -> Switch Connection to Medium Security");
                GameManager.instance.levelScript.ChangeAllConnections(ConnectionType.MedSec);
            }

            //third button
            if (GUI.Button(new Rect(15, 80, 80, 20), "Sec High"))
            {
                Debug.Log("Button -> Switch Connection to High Security");
                GameManager.instance.levelScript.ChangeAllConnections(ConnectionType.HighSec);
            }

            //fourth button
            if (GUI.Button(new Rect(15, 100, 80, 20), "Colours"))
            {
                Debug.Log("Button -> Toggle Colour Scheme");
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
            if (GUI.Button(new Rect(15, 140, 80, 20), "Node/Actors"))
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
                }
            }
            

        }
    }
}
