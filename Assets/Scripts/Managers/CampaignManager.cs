using System.Collections;
using System.Collections.Generic;
using gameAPI;
using UnityEngine;

/// <summary>
/// handles all Campaign related matters
/// </summary>
public class CampaignManager : MonoBehaviour
{

    [Tooltip("Current Campaign (this is the default campaign at game start)")]
    public Campaign campaign;


    private GameState gameState;                    //stores current game state prior to change to enable a revert after change

    public void Initialise()
    {
        Debug.Assert(campaign != null, "Invalid campaign (Null)");
        //Assign a scenario
        Scenario scenario = campaign.GetCurrentScenario();
        if (scenario != null)
        {
            GameManager.instance.scenarioScript.scenario = scenario;
            Debug.LogFormat("[Cam] CampaignManager.cs -> Initialise: Current scenario \"{0}\"{1}", scenario.tag, "\n");
        }
        else { Debug.LogError("Invalid scenario (Null)"); }
        //event Listeners
        EventManager.instance.AddListener(EventType.CreateNewGame, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.CreateOptions, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.CloseNewGame, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.CloseOptions, OnEvent, "CampaignManager");
    }

    /// <summary>
    /// Called when an event happens
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect Event type
        switch (eventType)
        {
            case EventType.CreateNewGame:
                ProcessNewGame();
                break;
            case EventType.CloseNewGame:
                CloseNewGame();
                break;
            case EventType.CreateOptions:
                ProcessOptions((GameState)Param);
                break;
            case EventType.CloseOptions:
                CloseOptions();
                break;
            default:
                Debug.LogErrorFormat("Invalid eventType {0}{1}", eventType, "\n");
                break;
        }
    }

    /// <summary>
    /// New Game
    /// </summary>
    private void ProcessNewGame()
    {
        //open NewGame background
        GameManager.instance.modalGUIScript.SetBackground(Background.NewGame);
        //close MainMenu
        EventManager.instance.PostNotification(EventType.CloseMainMenu, this, null, "CampaignManager.cs -> ProcessNewGame");
        //change game state (allows inputManager.cs to handle relevant input)
        GameManager.instance.inputScript.GameState = GameState.NewGame;
    }

    /// <summary>
    /// exit New Game screen
    /// </summary>
    private void CloseNewGame()
    {
        //revert to playGame state by default
        GameManager.instance.inputScript.GameState = GameState.PlayGame;
        //close background
        GameManager.instance.modalGUIScript.DisableBackground(Background.NewGame);
    }

    /// <summary>
    /// Options
    /// </summary>
    private void ProcessOptions(GameState state)
    {
        //open Options background
        GameManager.instance.modalGUIScript.SetBackground(Background.Options);
        //close MainMenu
        EventManager.instance.PostNotification(EventType.CloseMainMenu, this, null, "CampaignManager.cs -> ProcessOptions");
        gameState = state;
        //change game state (allows inputManager.cs to handle relevant input)
        GameManager.instance.inputScript.GameState = GameState.Options;
    }

    /// <summary>
    /// exit options screen
    /// </summary>
    private void CloseOptions()
    {
        //revert to original game screen prior to Options being chosen
        GameManager.instance.inputScript.GameState = gameState;
        //close background
        GameManager.instance.modalGUIScript.DisableBackground(Background.Options);
    }

    //new methods above here
}
