using gameAPI;
using UnityEngine;

/// <summary>
/// handles all Game Control and Main Menu matters
/// </summary>
public class ControlManager : MonoBehaviour
{

    private GameState gameState;                    //stores current game state prior to change to enable a revert after change


    public void Initialise(GameState state)
    {
        //event Listeners
        EventManager.instance.AddListener(EventType.NewGameOptions, OnEvent, "ControlManager");
        EventManager.instance.AddListener(EventType.CreateNewGame, OnEvent, "ControlManager");
        EventManager.instance.AddListener(EventType.CreateOptions, OnEvent, "ControlManager");
        EventManager.instance.AddListener(EventType.CloseNewGame, OnEvent, "ControlManager");
        EventManager.instance.AddListener(EventType.CloseOptions, OnEvent, "ControlManager");
        EventManager.instance.AddListener(EventType.CreateMetaGame, OnEvent, "ControlManager");
        EventManager.instance.AddListener(EventType.CloseMetaGame, OnEvent, "ControlManager");
        EventManager.instance.AddListener(EventType.ExitLevel, OnEvent, "ControlManager");
        EventManager.instance.AddListener(EventType.ExitGame, OnEvent, "ControlManager");
        EventManager.instance.AddListener(EventType.ExitCampaign, OnEvent, "ControlManager");
        EventManager.instance.AddListener(EventType.ResumeGame, OnEvent, "ControlManager");
        EventManager.instance.AddListener(EventType.LoadGame, OnEvent, "ControlManager");
        EventManager.instance.AddListener(EventType.SaveGame, OnEvent, "ControlManager");
        EventManager.instance.AddListener(EventType.CloseLoadGame, OnEvent, "ControlManager");
        EventManager.instance.AddListener(EventType.CloseSaveGame, OnEvent, "CampaignManager");
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
                ProcessLoadGame((GameState)Param);
                break;
            case EventType.SaveGame:
                ProcessSaveGame((GameState)Param);
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
            case EventType.CloseSaveGame:
                CloseSaveGame();
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
            case EventType.ExitCampaign:
                ProcessEndCampaign();
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
        //save existing game state
        gameState = GameManager.i.inputScript.GameState;
        //set background
        GameManager.i.modalGUIScript.SetBackground(Background.NewGame);
        //close MainMenu
        EventManager.instance.PostNotification(EventType.CloseMainMenu, this, null, "CampaignManager.cs -> ProcessNewGame");
        //change game state (allows inputManager.cs to handle relevant input)
        GameManager.i.inputScript.GameState = GameState.NewGame;
    }

    /// <summary>
    /// New game options
    /// </summary>
    private void ProcessNewGameOptions()
    {
        //save existing game state
        gameState = GameManager.i.inputScript.GameState;
        //modal block
        GameManager.i.guiScript.SetIsBlocked(true);
        //open NewGame background
        GameManager.i.modalGUIScript.SetBackground(Background.NewGameOptions);
        //close previous background
        GameManager.i.modalGUIScript.CloseBackgrounds(Background.NewGameOptions);
        //change game state (allows inputManager.cs to handle relevant input)
        GameManager.i.inputScript.GameState = GameState.NewGameOptions;
    }

    /// <summary>
    /// exit New Game screen
    /// </summary>
    private void CloseNewGameOptions()
    {
        //save existing game state
        gameState = GameManager.i.inputScript.GameState;
        //revert to playGame state by default
        GameManager.i.inputScript.GameState = GameState.NewInitialisation;
        //create new game -> DEBUG: resets campaign so assumes brand new campaign
        GameManager.i.campaignScript.Reset();
        //set up first level in campaign
        GameManager.i.InitialiseNewSession();
        //campaign history
        GameManager.i.dataScript.SetCampaignHistoryStart();
        //revert to playGame state by default
        GameManager.i.inputScript.GameState = GameState.PlayGame;
        //close background
        GameManager.i.modalGUIScript.CloseBackgrounds();
        //toggle of modal block
        GameManager.i.guiScript.SetIsBlocked(false);
    }



    /// <summary>
    /// Options
    /// </summary>
    private void ProcessOptions(GameState state)
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessResumeGame: game OPTIONS selected{0}", "\n");
        //modal block
        GameManager.i.guiScript.SetIsBlocked(true);
        //open Options background
        GameManager.i.modalGUIScript.SetBackground(Background.Options);
        //close MainMenu
        EventManager.instance.PostNotification(EventType.CloseMainMenu, this, null, "CampaignManager.cs -> ProcessOptions");
        gameState = state;
        //change game state (allows inputManager.cs to handle relevant input)
        GameManager.i.inputScript.GameState = GameState.Options;
    }

    /// <summary>
    /// exit options screen
    /// </summary>
    private void CloseOptions()
    {
        //revert to original game screen prior to Options being chosen
        GameManager.i.inputScript.GameState = gameState;
        //close background
        GameManager.i.modalGUIScript.CloseBackgrounds();
        //toggle of modal block
        GameManager.i.guiScript.SetIsBlocked(false);
    }

    /// <summary>
    /// Exit level and display summary background
    /// </summary>
    private void ProcessEndLevel()
    {
        //save existing game state
        gameState = GameManager.i.inputScript.GameState;
        //close all tooltips
        GameManager.i.guiScript.SetTooltipsOff();
        //modal block
        GameManager.i.guiScript.SetIsBlocked(true);
        //Open end level background
        GameManager.i.modalGUIScript.SetBackground(Background.EndLevel);
        //change game state
        GameManager.i.inputScript.GameState = GameState.ExitLevel;
        //campaign history
        GameManager.i.dataScript.SetCampaignHistoryEnd();
    }

    /// <summary>
    /// Start Meta Game
    /// </summary>
    private void ProcessMetaGame()
    {
        //save existing game state
        gameState = GameManager.i.inputScript.GameState;
        //modal block
        GameManager.i.guiScript.SetIsBlocked(true);
        //Open end level background
        GameManager.i.modalGUIScript.SetBackground(Background.MetaGame);
        //disable end level background
        GameManager.i.modalGUIScript.CloseBackgrounds(Background.MetaGame);
        //change game state
        GameManager.i.inputScript.GameState = GameState.MetaGame;
        //run metaGame
        GameManager.i.metaScript.ProcessMetaGame();
    }

    /// <summary>
    /// Close Meta Game and start up new level (debug)
    /// </summary>
    private void CloseMetaGame()
    { 
        //save existing game state
        gameState = GameManager.i.inputScript.GameState;
        //go to next scenario
        if (GameManager.i.campaignScript.IncrementScenarioIndex() == true)
        {
            //change game state
            GameManager.i.inputScript.GameState = GameState.FollowOnInitialisation;
            //create new followOn level
            GameManager.i.InitialiseNewLevel();
            //campaign data
            GameManager.i.dataScript.SetCampaignHistoryStart();
            //data Integrity check
            if (GameManager.i.isIntegrityCheck == true)
            { GameManager.i.validateScript.ExecuteIntegrityCheck(); }
            //revert to playGame state by default
            GameManager.i.inputScript.GameState = GameState.PlayGame;
            //close background
            GameManager.i.modalGUIScript.CloseBackgrounds();
            //toggle of modal block
            GameManager.i.guiScript.SetIsBlocked(false);
        }
        else
        {
            //End of Campaign -> open background
            GameManager.i.modalGUIScript.SetBackground(Background.EndCampaign);
            //disable end level background
            GameManager.i.modalGUIScript.CloseBackgrounds(Background.EndCampaign);
            //change game state
            GameManager.i.inputScript.GameState = GameState.ExitCampaign;
        }
    }

    /// <summary>
    /// Return to the game
    /// </summary>
    private void ProcessResumeGame()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessResumeGame: RESUME game option selected{0}", "\n");
        //save existing game state
        gameState = GameManager.i.inputScript.GameState;
        //if anything other than play Game then set a modal block (it cancels otherwise due to Main Menu closing)
        if (GameManager.i.inputScript.GameState != GameState.PlayGame)
        { GameManager.i.guiScript.SetIsBlocked(true); }
        
    }

    /// <summary>
    /// Load a saved game
    /// </summary>
    private void ProcessLoadGame(GameState state)
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessLoadGame: LOAD game option selected{0}", "\n");
        //save existing game state
        gameState = state;
        //toggle on modal block
        GameManager.i.guiScript.SetIsBlocked(true);
        //Load Game -> open background
        GameManager.i.modalGUIScript.SetBackground(Background.LoadGame);
        //Close any open background
        GameManager.i.modalGUIScript.CloseBackgrounds(Background.LoadGame);
        //change game state (mid game load or start of session load?)
        if (GameManager.i.isSession == true)
        {
            GameManager.i.inputScript.GameState = GameState.LoadGame;
            //check node display and reset back to normal if not prior to save
            if (GameManager.i.nodeScript.NodeShowFlag > 0)
            { GameManager.i.nodeScript.ResetAll(); }
            //check connection display and reset back to normal if not prior to save
            if (GameManager.i.connScript.resetConnections == true)
            { GameManager.i.connScript.RestoreConnections(); }
        }
        else
        {
            //load game at start
            GameManager.i.inputScript.GameState = GameState.LoadAtStart;
        }
        //Debug -> time load game process
        GameManager.i.testScript.StartTimer();
        //read data from file
        if (GameManager.i.fileScript.ReadSaveData() == true)
        {
            //load data into game
            GameManager.i.fileScript.LoadSaveData();
        }
        //how long did it take?
        long timeElapsed = GameManager.i.testScript.StopTimer();
        Debug.LogFormat("[Per] ControlManager.cs -> ProcessLoadGame: LOAD GAME took {0} ms", timeElapsed);
    }

    /// <summary>
    /// Close load game screen
    /// </summary>
    private void CloseLoadGame()
    {
        //LoadAtStart
        if (GameManager.i.inputScript.GameState == GameState.LoadAtStart)
        {
            //update colours (not done with FileManager.cs -> ReadOptionData due to sequencing issues (colour changes to objects that aren't yet initialised)
            EventManager.instance.PostNotification(EventType.ChangeColour, this, null, "OptionManager.cs -> ColourOption");
        }
        //Close any open background
        GameManager.i.modalGUIScript.CloseBackgrounds();
        //toggle of modal block
        GameManager.i.guiScript.SetIsBlocked(false);
        //Integrity check 
        if (GameManager.i.isIntegrityCheck == true)
        { GameManager.i.validateScript.ExecuteIntegrityCheck(); }
        //change game state
        GameManager.i.inputScript.GameState = GameState.PlayGame;
    }

    /// <summary>
    /// Save the current game
    /// </summary>
    private void ProcessSaveGame(GameState state)
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessResumeGame: SAVE game option selected{0}", "\n");
        //save existing game state
        gameState = state;
        //toggle on modal block
        GameManager.i.guiScript.SetIsBlocked(true);
        //Save Game -> open background
        GameManager.i.modalGUIScript.SetBackground(Background.SaveGame);
        //Close any open background
        GameManager.i.modalGUIScript.CloseBackgrounds(Background.SaveGame);
        //change game state
        GameManager.i.inputScript.GameState = GameState.SaveGame;
        //check node display and reset back to normal if not prior to save
        if (GameManager.i.nodeScript.NodeShowFlag > 0)
        { GameManager.i.nodeScript.ResetAll(); }
        //check connection display and reset back to normal if not prior to save
        if (GameManager.i.connScript.resetConnections == true)
        { GameManager.i.connScript.RestoreConnections(); }
        //Debug -> time load game process
        GameManager.i.testScript.StartTimer();
        GameManager.i.fileScript.WriteSaveData();
        GameManager.i.fileScript.SaveGame();
        //how long did it take?
        long timeElapsed = GameManager.i.testScript.StopTimer();
        Debug.LogFormat("[Per] ControlManager.cs -> ProcessSaveGame: SAVE GAME took {0} ms", timeElapsed);
    }

    /// <summary>
    /// win/Loss state achieved for end of campaign -> summaries, etc before exiting
    /// </summary>
    private void ProcessEndCampaign()
    {
        //close any Node tooltip
        GameManager.i.tooltipNodeScript.CloseTooltip("CityInfoUI.cs -> SetCityInfo");
        //modal block
        GameManager.i.guiScript.SetIsBlocked(true);
        //Open end level background
        GameManager.i.modalGUIScript.SetBackground(Background.EndCampaign);
        //change game state
        GameManager.i.inputScript.GameState = GameState.ExitCampaign;
        //end level campaign data
        GameManager.i.dataScript.SetCampaignHistoryEnd();
    }

    /// <summary>
    /// Close save game screen
    /// </summary>
    private void CloseSaveGame()
    {
        //Close any open background
        GameManager.i.modalGUIScript.CloseBackgrounds();
        //toggle of modal block
        GameManager.i.guiScript.SetIsBlocked(false);
        //change game state
        GameManager.i.inputScript.GameState = GameState.PlayGame;
    }


    private void CloseGame()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessResumeGame: QUIT game option selected{0}", "\n");
        //save existing game state
        gameState = GameManager.i.inputScript.GameState;
        //quit game
        GameManager.i.turnScript.Quit();
    }



    /// <summary>
    /// returns game state at time player opted for the selected option. Use during non-normal gameState checks, eg. Menu ops, Save/Load, otherwise inputScript.GameState
    /// </summary>
    /// <returns></returns>
    public GameState GetExistingGameState()
    { return gameState; }


    //new methods above here
}
