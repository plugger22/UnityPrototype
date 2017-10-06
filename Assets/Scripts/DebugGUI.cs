using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using modalAPI;

/// <summary>
/// debug GUI, on demand (HotKey 'D' -> On, HotKey 'H' -> Off)
/// </summary>
public class DebugGUI : MonoBehaviour
{
    public GUIStyle customBackground;

    private bool showGUI = false;
    private bool showAnalysis = false;

    

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

                /*Debug.Log("Button -> Toggle Action Menu");
                if (GameManager.instance.isBlocked == false)
                { GameManager.instance.actionMenuScript.SetActionMenu(new ModalPanelDetails()); }
                else
                { GameManager.instance.actionMenuScript.CloseActionMenu(); }*/

                /*Debug.Log("Button -> Toggle News Ticker");
                GameManager.instance.tickerScript.ToggleTicker();*/

                /*int nodeID = GameManager.instance.nodeActive;
                GameManager.instance.levelScript.CheckConnected(nodeID);
                Debug.Log("Connection Check to Node " + nodeID);*/
            }

            //fifth button
            if (GUI.Button(new Rect(15, 120, 80, 20), "Analysis"))
            {
                //path too
                /*int nodeID = GameManager.instance.nodeActive;
                Debug.Log("Button -> Check path from node 0 to " + nodeID);
                GameManager.instance.levelScript.CheckPath(nodeID);*/
                Debug.Log("Button -> Toggle Analysis");
                if (showAnalysis == false)
                { showAnalysis = true; }
                else { showAnalysis = false; }
            }


            //sixth button
            if (GUI.Button(new Rect(15, 140, 80, 20), "Side"))
            {
                Debug.Log("Button -> Swap sides");
                if (GameManager.instance.optionScript.PlayerSide == Side.Rebel)
                { GameManager.instance.optionScript.PlayerSide = Side.Authority; }
                else { GameManager.instance.optionScript.PlayerSide = Side.Rebel; }
            }

            //
            // - - - Analysis at Right Hand side of Screen - - -
            //

            if (showAnalysis == true)
            {
                //graph data, far right
                customBackground.alignment = TextAnchor.UpperLeft;
                string analysis = GameManager.instance.levelScript.GetGraphAnalysis();
                GUI.Box(new Rect(Screen.width - 115, 10, 110, 200), analysis, customBackground);

                //Actor data, middle right
                customBackground.alignment = TextAnchor.UpperLeft;
                analysis = GameManager.instance.levelScript.GetActorAnalysis();
                GUI.Box(new Rect(Screen.width - 335, 10, 220, 200), analysis, customBackground);

                // Node Type data, near centre right
                customBackground.alignment = TextAnchor.UpperLeft;
                analysis = GameManager.instance.levelScript.GetNodeAnalysis();
                GUI.Box(new Rect(Screen.width - 485, 10, 150, 200), analysis, customBackground);
            }
        }
    }
}
