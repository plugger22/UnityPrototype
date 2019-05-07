using gameAPI;
using UnityEngine;

/// <summary>
/// handles all Game Control and Main Menu matters
/// </summary>
public class ControlManager : MonoBehaviour
{

    private GameState gameState;                    //stores current game state prior to change to enable a revert after change


    public void Initialise()
    {
        //event Listeners
        EventManager.instance.AddListener(EventType.NewGameOptions, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.CreateNewGame, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.CreateOptions, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.CloseNewGame, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.CloseOptions, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.CreateMetaGame, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.CloseMetaGame, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.ExitLevel, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.ExitGame, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.ResumeGame, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.LoadGame, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.SaveGame, OnEvent, "CampaignManager");
        EventManager.instance.AddListener(EventType.CloseLoadGame, OnEvent, "CampaignManager");
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
            case EventType.ResumeGame:
                ProcessResumeGame();
                break;
            case EventType.LoadGame:
                ProcessLoadGame();
                break;
            case EventType.SaveGame:
                ProcessSaveGame();
                break;
            case EventType.NewGameOptions:
                ProcessNewGameOptions();
                break;
            case EventType.CreateNewGame:
                ProcessNewGame();
                break;
            case EventType.CloseNewGame:
                CloseNewGameOptions();
                break;
            case EventType.CloseLoadGame:
                CloseLoadGame();
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
            case EventType.ExitGame:
                CloseGame();
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
        GameManager.instance.modalGUIScript.CloseBackgrounds(Background.NewGameOptions);
        //change game state (allows inputManager.cs to handle relevant input)
        GameManager.instance.inputScript.GameState = GameState.NewGameOptions;
    }

    /// <summary>
    /// exit New Game screen
    /// </summary>
    private void CloseNewGameOptions()
    {
        //revert to playGame state by default
        GameManager.instance.inputScript.GameState = GameState.NewInitialisation;
        //create new game -> DEBUG: resets campaign so assumes brand new campaign
        GameManager.instance.campaignScript.Reset();
        GameManager.instance.campaignScript.InitialiseScenario();
        //set up new level
        GameManager.instance.InitialiseNewLevel();
        //revert to playGame state by default
        GameManager.instance.inputScript.GameState = GameState.PlayGame;
        //close background
        GameManager.instance.modalGUIScript.CloseBackgrounds();
        //toggle of modal block
        GameManager.instance.guiScript.SetIsBlocked(false);
    }



    /// <summary>
    /// Options
    /// </summary>
    private void ProcessOptions(GameState state)
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessResumeGame: game OPTIONS selected{0}", "\n");
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
        GameManager.instance.modalGUIScript.CloseBackgrounds();
        //toggle of modal block
        GameManager.instance.guiScript.SetIsBlocked(false);
    }

    /// <summary>
    /// Exit level and display summary background
    /// </summary>
    private void ProcessEndLevel()
    {      
        //close any Node tooltip
        GameManager.instance.tooltipNodeScript.CloseTooltip("CityInfoUI.cs -> SetCityInfo");
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
        GameManager.instance.modalGUIScript.CloseBackgrounds(Background.MetaGame);
        //change game state
        GameManager.instance.inputScript.GameState = GameState.MetaGame;
        //run metaGame
        GameManager.instance.metaScript.ProcessMetaGame();
    }

    /// <summary>
    /// Close Meta Game and start up new level (debug)
    /// </summary>
    private void CloseMetaGame()
    {
        //go to next scenario
        if (GameManager.instance.campaignScript.IncrementScenarioIndex() == true)
        {
            //change game state
            GameManager.instance.inputScript.GameState = GameState.FollowOnInitialisation;
            //get current scenario data
            GameManager.instance.campaignScript.InitialiseScenario();
            //create new level
            GameManager.instance.InitialiseNewLevel();
            //revert to playGame state by default
            GameManager.instance.inputScript.GameState = GameState.PlayGame;
            //close background
            GameManager.instance.modalGUIScript.CloseBackgrounds();
            //toggle of modal block
            GameManager.instance.guiScript.SetIsBlocked(false);
        }
        else
        {
            //End of Campaign -> open background
            GameManager.instance.modalGUIScript.SetBackground(Background.EndCampaign);
            //disable end level background
            GameManager.instance.modalGUIScript.CloseBackgrounds(Background.EndCampaign);
            //change game state
            GameManager.instance.inputScript.GameState = GameState.ExitCampaign;
        }
    }

    /// <summary>
    /// Return to the game
    /// </summary>
    private void ProcessResumeGame()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessResumeGame: RESUME game option selected{0}", "\n");
        //if anything other than play Game then set a modal block (it cancels otherwise due to Main Menu closing)
        if (GameManager.instance.inputScript.GameState != GameState.PlayGame)
        { GameManager.instance.guiScript.SetIsBlocked(true); }
        
    }

    /// <summary>
    /// Load a saved game
    /// </summary>
    private void ProcessLoadGame()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessLoadGame: LOAD game option selected{0}", "\n");
        //toggle on modal block
        GameManager.instance.guiScript.SetIsBlocked(true);
        //Load Game -> open background
        GameManager.instance.modalGUIScript.SetBackground(Background.LoadGame);
        //Close any open background
        GameManager.instance.modalGUIScript.CloseBackgrounds(Background.LoadGame);
        //change game state
        GameManager.instance.inputScript.GameState = GameState.LoadGame;
        //Debug -> time load game process
        GameManager.instance.testScript.StartTimer();
        //read data from file
        if (GameManager.instance.fileScript.ReadGameData() == true)
        {
            //load data into game
            GameManager.instance.fileScript.LoadSaveData();
        }
        //how long did it take?
        long timeElapsed = GameManager.instance.testScript.StopTimer();
        Debug.LogFormat("[Per] ControlManager.cs -> ProcessLoadGame: LOAD GAME took {0} ms", timeElapsed);
    }

    /// <summary>
    /// Close load game screen
    /// </summary>
    private void CloseLoadGame()
    {
        //Close any open background
        GameManager.instance.modalGUIScript.CloseBackgrounds();
        //toggle of modal block
        GameManager.instance.guiScript.SetIsBlocked(false);
        //change game state
        GameManager.instance.inputScript.GameState = GameState.PlayGame;

    }

    /// <summary>
    /// Save the current game
    /// </summary>
    private void ProcessSaveGame()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessResumeGame: SAVE game option selected{0}", "\n");
        //Debug -> time load game process
        GameManager.instance.testScript.StartTimer();
        GameManager.instance.fileScript.WriteGameData();
        GameManager.instance.fileScript.SaveGame();
        //how long did it take?
        long timeElapsed = GameManager.instance.testScript.StopTimer();
        Debug.LogFormat("[Per] ControlManager.cs -> ProcessSaveGame: SAVE GAME took {0} ms", timeElapsed);
    }


    private void CloseGame()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessResumeGame: QUIT game option selected{0}", "\n");
        GameManager.instance.turnScript.Quit();
    }




    //new methods above here
}
