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
        Debug.LogFormat("[Cam] CampaignManager.cs -> Initialise: There are {0} scenarios in the \"{1}\" campaign, ID {2}{3}", campaign.listOfScenarios.Count, campaign.tag, campaign.campaignID, "\n");
        
        //event Listeners
        EventManager.instance.AddListener(EventType.NewGameOptions, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.CreateNewGame, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.CreateOptions, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.CloseNewGame, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.CloseOptions, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.CreateMetaGame, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.CloseMetaGame, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.ExitLevel, OnEvent, "CampaignManager");
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
            case EventType.NewGameOptions:
                ProcessNewGameOptions();
                break;
            case EventType.CreateNewGame:
                ProcessNewGame();
                break;
            case EventType.CloseNewGame:
                CloseNewGameOptions();
                break;
            case EventType.CreateOptions:
                ProcessOptions((GameState)Param);
                break;
            case EventType.CloseOptions:
                CloseOptions();
                break;
            case EventType.ExitLevel:
                ProcessEndLevel();
                break;
            case EventType.CreateMetaGame:
                ProcessMetaGame();
                break;
            case EventType.CloseMetaGame:
                CloseMetaGame();
                break;
            default:
                Debug.LogErrorFormat("Invalid eventType {0}{1}", eventType, "\n");
                break;
        }
    }

    //
    // - - - Event methods - - -
    //

    /// <summary>
    /// New Game
    /// </summary>
    private void ProcessNewGame()
    {
        //set background
        GameManager.instance.modalGUIScript.SetBackground(Background.NewGame);
        //close MainMenu
        EventManager.instance.PostNotification(EventType.CloseMainMenu, this, null, "CampaignManager.cs -> ProcessNewGame");
        //change game state (allows inputManager.cs to handle relevant input)
        GameManager.instance.inputScript.GameState = GameState.NewGame;
    }

    /// <summary>
    /// New game options
    /// </summary>
    private void ProcessNewGameOptions()
    {
        //modal block
        GameManager.instance.guiScript.SetIsBlocked(true);
        //open NewGame background
        GameManager.instance.modalGUIScript.SetBackground(Background.NewGameOptions);
        //close previous background
        GameManager.instance.modalGUIScript.DisableBackground(Background.NewGame);
        //change game state (allows inputManager.cs to handle relevant input)
        GameManager.instance.inputScript.GameState = GameState.NewGameOptions;
    }

    /// <summary>
    /// exit New Game screen
    /// </summary>
    private void CloseNewGameOptions()
    {
        //create new game
        campaign.Reset();
        InitialiseScenario();
        GameManager.instance.InitialiseNewLevel();
        //revert to playGame state by default
        GameManager.instance.inputScript.GameState = GameState.PlayGame;
        //close background
        GameManager.instance.modalGUIScript.DisableBackground(Background.NewGameOptions);
        //toggle of modal block
        GameManager.instance.guiScript.SetIsBlocked(false);
    }

    /// <summary>
    /// Options
    /// </summary>
    private void ProcessOptions(GameState state)
    {
        //modal block
        GameManager.instance.guiScript.SetIsBlocked(true);
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
        //toggle of modal block
        GameManager.instance.guiScript.SetIsBlocked(false);
    }

    /// <summary>
    /// Exit level and display summary background
    /// </summary>
    private void ProcessEndLevel()
    {
        //modal block
        GameManager.instance.guiScript.SetIsBlocked(true);
        //Open end level background
        GameManager.instance.modalGUIScript.SetBackground(Background.EndLevel);
        //change game state
        GameManager.instance.inputScript.GameState = GameState.ExitLevel;
    }

    /// <summary>
    /// Start Meta Game
    /// </summary>
    private void ProcessMetaGame()
    {
        //modal block
        GameManager.instance.guiScript.SetIsBlocked(true);
        //Open end level background
        GameManager.instance.modalGUIScript.SetBackground(Background.MetaGame);
        //disable end level background
        GameManager.instance.modalGUIScript.DisableBackground(Background.EndLevel);
        //change game state
        GameManager.instance.inputScript.GameState = GameState.MetaGame;
    }

    /// <summary>
    /// Close Meta Game and start up new level (debug)
    /// </summary>
    private void CloseMetaGame()
    {
        //go to next scenario
        if (campaign.IncrementScenarioIndex() == true)
        {
            //get current scenario data
            InitialiseScenario();
            //create new game
            GameManager.instance.InitialiseNewLevel();
            //revert to playGame state by default
            GameManager.instance.inputScript.GameState = GameState.PlayGame;
            //close background
            GameManager.instance.modalGUIScript.DisableBackground(Background.MetaGame);
            //toggle of modal block
            GameManager.instance.guiScript.SetIsBlocked(false);
        }
        else
        {
            //End of Campaign -> TO DO
            Debug.LogError("END OF CAMPAIGN (placeholder)");
        }
    }

    //
    // - - - Initialisation - - -
    //

    /// <summary>
    /// Get current scenario and pass to ScenarioManager.cs
    /// </summary>
    private void InitialiseScenario()
    {
        //Assign a scenario
        Scenario scenario = campaign.GetCurrentScenario();
        if (scenario != null)
        {
            GameManager.instance.scenarioScript.scenario = scenario;
            Debug.LogFormat("[Cam] CampaignManager.cs -> Initialise: Current scenario \"{0}\", ID {1}{2}", scenario.tag, scenario.scenarioID, "\n");
        }
        else { Debug.LogError("Invalid scenario (Null)"); }
    }

    //new methods above here
}
