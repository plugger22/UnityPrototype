using gameAPI;
using modalAPI;
using packageAPI;
using UnityEngine;

/// <summary>
/// handles all Game Control and Main Menu matters
/// </summary>
public class ControlManager : MonoBehaviour
{

    private GameState gameState;                    //stores current game state prior to change to enable a revert after change
    private RestorePoint restorePoint;              //stores point of game to restore to if user changes their mind (eg. 'Save And Exit')

    public void Initialise(GameState state)
    {
        //event Listeners
        EventManager.i.AddListener(EventType.NewGameOptions, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.CreateNewGame, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.CreateOptions, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.CloseNewGame, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.CloseOptions, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.CreateMetaOverall, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.CloseMetaOverall, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.ExitLevel, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.ExitGame, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.ExitCampaign, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.ResumeGame, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.ResumeMetaGame, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.LoadGame, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.SaveGame, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.SaveAndExit, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.CloseLoadGame, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.CloseSaveGame, OnEvent, "CampaignManager");
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
            case EventType.ResumeMetaGame:
                ProcessResumeMetaGame();
                break;
            case EventType.LoadGame:
                ProcessLoadGame((GameState)Param);
                break;
            case EventType.SaveGame:
                ProcessSaveGame((GameState)Param);
                break;
            case EventType.SaveAndExit:
                ProcessSaveAndExit((RestorePoint)Param);
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
            case EventType.CreateMetaOverall:
                ProcessMetaGame();
                break;
            case EventType.CloseMetaOverall:
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
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessNewGame: ProcessNewGame selected{0}", "\n");
        //save existing game state
        gameState = GameManager.i.inputScript.GameState;
        //set background
        GameManager.i.modalGUIScript.SetBackground(Background.NewGame);
        //close MainMenu
        EventManager.i.PostNotification(EventType.CloseMainMenu, this, null, "CampaignManager.cs -> ProcessNewGame");
        //change game state (allows inputManager.cs to handle relevant input)
        GameManager.i.inputScript.GameState = GameState.NewGame;
    }

    /// <summary>
    /// New game options
    /// </summary>
    private void ProcessNewGameOptions()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessNewGameOptions: ProcessNewGameOptions selected{0}", "\n");
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
        Debug.LogFormat("[Ctrl] ControlManager.cs -> CloseNewGameOptions: CloseNewGameOptions selected{0}", "\n");
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
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessOptions: game OPTIONS selected{0}", "\n");
        //modal block
        GameManager.i.guiScript.SetIsBlocked(true);
        //open Options background
        GameManager.i.modalGUIScript.SetBackground(Background.Options);
        //close MainMenu
        EventManager.i.PostNotification(EventType.CloseMainMenu, this, null, "CampaignManager.cs -> ProcessOptions");
        gameState = state;
        //change game state (allows inputManager.cs to handle relevant input)
        GameManager.i.inputScript.GameState = GameState.Options;
    }

    /// <summary>
    /// exit options screen
    /// </summary>
    private void CloseOptions()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> CloseOptions: CloseOptions selected{0}", "\n");
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
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessEndLevel: ProcessEndLevel selected{0}", "\n");
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
        //start metaGame
        EventManager.i.PostNotification(EventType.CreateMetaOverall, this, null, "ControlManager.cs -> ProcessEndLevel");
    }

    /// <summary>
    /// Start Meta Game
    /// </summary>
    private void ProcessMetaGame()
    {
        //save existing game state
        gameState = GameManager.i.inputScript.GameState;
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessMetaGame: ProcessMetaGame selected{0}", "\n");
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
        Debug.LogFormat("[Ctrl] ControlManager.cs -> CloseMetaGame: CloseMetaGame selected{0}", "\n");
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
            //close background
            GameManager.i.modalGUIScript.CloseBackgrounds();
            //toggle of modal block
            GameManager.i.guiScript.SetIsBlocked(false);
            //show top bar UI at completion of meta game
            EventManager.i.PostNotification(EventType.TopBarShow, this, null, "MetaGameUI.cs -> Show TopBarUI");
            //revert to playGame state by default
            GameManager.i.inputScript.GameState = GameState.PlayGame;
            //reset modal states back to normal
            GameManager.i.inputScript.ResetStates();
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
    /// Return to the MetaGame (different points) after SaveAndExit but opting to resume game once having Saved
    /// </summary>
    private void ProcessResumeMetaGame()
    {
        //modal block
        GameManager.i.guiScript.SetIsBlocked(true);
        //Open end level background
        GameManager.i.modalGUIScript.SetBackground(Background.MetaGame);
        //disable end level background
        GameManager.i.modalGUIScript.CloseBackgrounds(Background.MetaGame);
        //change game state
        GameManager.i.inputScript.GameState = GameState.MetaGame;
        switch (restorePoint)
        {
            case RestorePoint.MetaTransition:
                //open TransitionUI
                EventManager.i.PostNotification(EventType.TransitionOpen, this, null, "ControlManager.cs -> ProcessResumMetaGame");
                break;
            case RestorePoint.MetaOptions:
                //open MetaGameUI
                EventManager.i.PostNotification(EventType.MetaGameOpen, this, null, "ControlManager.cs -> ProcessResumMetaGame");
                break;
            case RestorePoint.MetaEnd:
                //open new Level
                EventManager.i.PostNotification(EventType.CloseMetaOverall, this, null, "ControlManager.cs -> ProcessResumMetaGame");
                break;
            default:
                //default exit game
                Debug.LogWarningFormat("Unrecognised restorePoint \"[0}\"", restorePoint);
                EventManager.i.PostNotification(EventType.ExitGame, this, null, "ControlManager.cs -> ProcessResumMetaGame");
                break;
        }
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
            LoadGameState loadGameState = GameManager.i.fileScript.LoadSaveData();
            if (loadGameState != null)
            {
                //standard load game drops you straight into gameState.PlayGame
                if (loadGameState.gameState != GameState.PlayGame)
                {
                    //special cases
                    switch (loadGameState.gameState)
                    {
                        case GameState.MetaGame:
                            //MetaGame requires special handling
                            restorePoint = loadGameState.restorePoint;
                            ProcessResumeMetaGame();
                            break;
                        default: Debug.LogWarningFormat("Unrecognised gameState \"{0}\"", loadGameState.gameState); break;
                    }
                }
            }
            else { Debug.LogError("Invalid loadGameState (Null)"); }
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
        Debug.LogFormat("[Ctrl] ControlManager.cs -> CloseLoadGame: CloseLoadGame selected{0}", "\n");
        //LoadAtStart
        if (GameManager.i.inputScript.GameState == GameState.LoadAtStart)
        {
            //update colours (not done with FileManager.cs -> ReadOptionData due to sequencing issues (colour changes to objects that aren't yet initialised)
            EventManager.i.PostNotification(EventType.ChangeColour, this, null, "OptionManager.cs -> ColourOption");
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
    /// Save the current game (Normal save operation during gameState.PlayGame)
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
        GameManager.i.fileScript.WriteSaveData(new LoadGameState() { gameState = GameState.PlayGame, restorePoint = RestorePoint.None });
        GameManager.i.fileScript.SaveGame();
        //how long did it take?
        long timeElapsed = GameManager.i.testScript.StopTimer();
        Debug.LogFormat("[Per] ControlManager.cs -> ProcessSaveGame: SAVE GAME took {0} ms", timeElapsed);
    }

    /// <summary>
    /// Save the current game and Exit. Special Save operation during gameState.MetaGame)
    /// </summary>
    private void ProcessSaveAndExit(RestorePoint restorePointInput)
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessSaveAndExit: Save and Exit selected{0}", "\n");
        //save restore point in case user changes their mind (update InputManager.cs as save/load is keyed off this value)
        restorePoint = restorePointInput;
        GameManager.i.inputScript.RestorePoint = restorePointInput;
        //toggle on modal block
        GameManager.i.guiScript.SetIsBlocked(true);
        //Save Game -> open background
        GameManager.i.modalGUIScript.SetBackground(Background.SaveGame);
        //Close any open background
        GameManager.i.modalGUIScript.CloseBackgrounds(Background.SaveGame);
        //change game state
        GameManager.i.inputScript.GameState = GameState.SaveAndExit;
        //Debug -> time load game process
        GameManager.i.testScript.StartTimer();
        GameManager.i.fileScript.WriteSaveData(new LoadGameState() { gameState = GameState.MetaGame, restorePoint = restorePointInput });
        GameManager.i.fileScript.SaveGame();
        //how long did it take?
        long timeElapsed = GameManager.i.testScript.StopTimer();
        Debug.LogFormat("[Per] ControlManager.cs -> ProcessSaveGame: SAVE GAME took {0} ms", timeElapsed);
        //Game saved, ask user if they want to exit
        ModalConfirmDetails details = new ModalConfirmDetails();
        details.topText = string.Format("Your game has been {0}. You are about to Exit", GameManager.Formatt("SAVED", ColourType.neutralText));
        details.bottomText = "Are you sure?";
        details.buttonFalse = "EXIT";
        details.buttonTrue = "RESUME";
        details.eventFalse = EventType.ExitGame;
        details.eventTrue = EventType.ResumeMetaGame;
        details.modalState = ModalSubState.MetaGame;
        //open confirm
        EventManager.i.PostNotification(EventType.ConfirmOpen, this, details, "MetaManager.cs -> ProcessMetaGame");
    }

    /// <summary>
    /// win/Loss state achieved for end of campaign -> summaries, etc before exiting
    /// </summary>
    private void ProcessEndCampaign()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessEndCampaign: ProcessEndCampaign selected{0}", "\n");
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
        Debug.LogFormat("[Ctrl] ControlManager.cs -> CloseSaveGame: CloseSaveGame selected{0}", "\n");
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
