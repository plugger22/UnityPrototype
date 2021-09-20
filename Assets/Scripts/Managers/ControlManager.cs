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

    #region Initialise
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
        EventManager.i.AddListener(EventType.ExitGameTutorial, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.ExitCampaign, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.ResumeGame, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.ResumeMetaGame, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.LoadGame, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.SaveGame, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.SaveGameAndReturn, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.SaveAndExit, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.SaveToMain, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.CloseLoadGame, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.CloseSaveGame, OnEvent, "ControlManager");
        EventManager.i.AddListener(EventType.TutorialOptions, OnEvent, "ControlManager.cs");
        EventManager.i.AddListener(EventType.CloseTutorialOptions, OnEvent, "ControlManager.cs");
        EventManager.i.AddListener(EventType.TutorialReturn, OnEvent, "ControlManager.cs");
        EventManager.i.AddListener(EventType.GameReturn, OnEvent, "ControlManager.cs");
    }
    #endregion

    #region OnEvent
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
            case EventType.SaveGameAndReturn:
                ProcessSaveGameAndReturn();
                break;
            case EventType.SaveToMain:
                ProcessCloseSaveGameAndReturnToMainMenu();
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
                ProcessCloseNewGameOptions();
                break;
            case EventType.CloseLoadGame:
                CloseLoadGame();
                break;
            case EventType.CloseSaveGame:
                ProcessCloseSaveGame();
                break;
            case EventType.TutorialOptions:
                ProcessTutorialOptions();
                break;
            case EventType.CloseTutorialOptions:
                ProcessCloseTutorialOptions();
                break;
            case EventType.TutorialReturn:
                ProcessTutorialReturn();
                break;
            case EventType.GameReturn:
                ProcessGameReturn();
                break;
            case EventType.CreateOptions:
                ProcessOptions((GameState)Param);
                break;
            case EventType.CloseOptions:
                ProcessCloseOptions();
                break;
            case EventType.ExitLevel:
                ProcessEndLevel();
                break;
            case EventType.CreateMetaOverall:
                ProcessMetaGame();
                break;
            case EventType.CloseMetaOverall:
                ProcessCloseMetaGame();
                break;
            case EventType.ExitCampaign:
                ProcessEndCampaign();
                break;
            case EventType.ExitGame:
                ProcessCloseGame();
                break;
            case EventType.ExitGameTutorial:
                ProcessCloseGameTutorial();
                break;
            default:
                Debug.LogErrorFormat("Invalid eventType {0}{1}", eventType, "\n");
                break;
        }
    }
    #endregion

    //
    // - - - Event methods - - -
    //

    #region Process...

    #region ProcessNewGame
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
        EventManager.i.PostNotification(EventType.CloseMainMenu, this, null, "ControlManager.cs -> ProcessNewGame");
        //modal block -> after closeMainMenu
        GameManager.i.guiScript.SetIsBlocked(true);
        //change game state (allows inputManager.cs to handle relevant input)
        GameManager.i.inputScript.GameState = GameState.NewGame;
    }
    #endregion

    #region ProcessNewGameOptions
    /// <summary>
    /// New game options
    /// </summary>
    private void ProcessNewGameOptions()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessNewGameOptions: ProcessNewGameOptions selected{0}", "\n");
        //save existing game state
        gameState = GameManager.i.inputScript.GameState;
        //open NewGame background
        GameManager.i.modalGUIScript.SetBackground(Background.NewGameOptions);
        //close previous background
        GameManager.i.modalGUIScript.CloseBackgrounds(Background.NewGameOptions);
        //change game state (allows inputManager.cs to handle relevant input)
        GameManager.i.inputScript.GameState = GameState.NewGameOptions;
    }
    #endregion

    #region ProcessCloseNewGameOptions
    /// <summary>
    /// exit New Game screen
    /// </summary>
    private void ProcessCloseNewGameOptions()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> CloseNewGameOptions: CloseNewGameOptions selected{0}", "\n");
        //save existing game state
        gameState = GameManager.i.inputScript.GameState;
        //revert to playGame state by default
        GameManager.i.inputScript.GameState = GameState.NewInitialisation;
        //create new game -> DEBUG: resets campaign so assumes brand new campaign
        GameManager.i.campaignScript.Reset();
        //set up a level in campaign
        GameManager.i.InitialiseNewSession();
        //campaign history
        GameManager.i.dataScript.SetCampaignHistoryStart();
        //revert to playGame state by default
        GameManager.i.inputScript.GameState = GameState.PlayGame;
        //close background
        GameManager.i.modalGUIScript.CloseBackgrounds();
        //toggle of modal block
        GameManager.i.guiScript.SetIsBlocked(false);
        //start animations
        GameManager.i.animateScript.StartAnimations();
    }
    #endregion

    #region ProcessOptions
    /// <summary>
    /// Options
    /// </summary>
    private void ProcessOptions(GameState state)
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessOptions: game OPTIONS selected{0}", "\n");
        //open Options background
        GameManager.i.modalGUIScript.SetBackground(Background.Options);
        //close MainMenu
        EventManager.i.PostNotification(EventType.CloseMainMenu, this, null, "ControlManager.cs -> ProcessOptions");
        //modal block -> after CloseMainMenu (it resets block)
        GameManager.i.guiScript.SetIsBlocked(true);
        gameState = state;
        //change game state (allows inputManager.cs to handle relevant input)
        GameManager.i.inputScript.GameState = GameState.Options;
    }
    #endregion

    #region ProcessCloseOptions
    /// <summary>
    /// exit options screen
    /// </summary>
    private void ProcessCloseOptions()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> CloseOptions: CloseOptions selected{0}", "\n");
        //revert to original game screen prior to Options being chosen
        GameManager.i.inputScript.GameState = gameState;
        //close background
        GameManager.i.modalGUIScript.CloseBackgrounds();
        //toggle of modal block
        GameManager.i.guiScript.SetIsBlocked(false);
    }
    #endregion

    #region ProcessEndLevel
    /// <summary>
    /// Exit level and display summary background
    /// </summary>
    private void ProcessEndLevel()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessEndLevel: ProcessEndLevel selected{0}", "\n");
        //save existing game state
        gameState = GameManager.i.inputScript.GameState;
        //stop animations
        GameManager.i.animateScript.StopAnimations();
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
    #endregion

    #region ProcessMetaGame
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
    #endregion

    #region ProcessCloseMetaGame
    /// <summary>
    /// Close Meta Game and start up new level (debug)
    /// </summary>
    private void ProcessCloseMetaGame()
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
            //start animations
            GameManager.i.animateScript.StartAnimations();
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
    #endregion

    #region ProcessResumeGame
    /// <summary>
    /// Return to the game
    /// </summary>
    private void ProcessResumeGame()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessResumeGame: RESUME game option selected{0}", "\n");
        //save existing game state
        gameState = GameManager.i.inputScript.GameState;
        //if anything other than play Game /Tutorial then set a modal block (it cancels otherwise due to Main Menu closing)
        if (GameManager.i.inputScript.CheckNormalMode() == false)
        { GameManager.i.guiScript.SetIsBlocked(true); }

    }
    #endregion

    #region ProcessResumeMetaGame
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
            case RestorePoint.MetaComplete:
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
    #endregion

    #region ProcessLoadGame
    /// <summary>
    /// Load a saved game
    /// </summary>
    private void ProcessLoadGame(GameState state)
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessLoadGame: LOAD game option selected{0}", "\n");
        //shutdown animations
        GameManager.i.animateScript.StopAnimations();
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
        if (GameManager.i.fileScript.ReadSaveData(SaveType.PlayerSave) == true)
        {
            //load data into game
            LoadGameState loadGameState = GameManager.i.fileScript.LoadSaveData(SaveType.PlayerSave);
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
    #endregion

    #region CloseLoadGame
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
        //start animations
        GameManager.i.animateScript.StartAnimations();
    }
    #endregion

    #region ProcessSaveGame
    /// <summary>
    /// Save the current game (Normal save operation during gameState.PlayGame)
    /// </summary>
    private void ProcessSaveGame(GameState state)
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessSaveGame: SAVE game option selected{0}", "\n");
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
        GameManager.i.fileScript.WriteSaveData(new LoadGameState() { gameState = GameState.PlayGame, restorePoint = RestorePoint.None }, SaveType.PlayerSave);
        GameManager.i.fileScript.SaveGame(SaveType.PlayerSave);
        //how long did it take?
        long timeElapsed = GameManager.i.testScript.StopTimer();
        Debug.LogFormat("[Per] ControlManager.cs -> ProcessSaveGame: SAVE GAME took {0} ms", timeElapsed);
    }
    #endregion

    #region ProcessSaveAndExit
    /// <summary>
    /// Save the current game and Exit. Special Save operation during gameState.MetaGame)
    /// </summary>
    private void ProcessSaveAndExit(RestorePoint restorePointInput)
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessSaveAndExit: Save and Exit selected{0}", "\n");
        //save restore point in case user changes their mind (update InputManager.cs as save/load is keyed off this value)
        restorePoint = restorePointInput;

        /*GameManager.i.inputScript.RestorePoint = restorePointInput;*/

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
        GameManager.i.fileScript.WriteSaveData(new LoadGameState() { gameState = GameState.MetaGame, restorePoint = restorePointInput }, SaveType.PlayerSave);
        GameManager.i.fileScript.SaveGame(SaveType.PlayerSave);
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
    #endregion

    #region ProcessAutoSave
    /// <summary>
    /// AutoSave file (TurnManager.cs -> ProcessNewTurn)
    /// </summary>
    public void ProcessAutoSave(GameState state, RestorePoint restore = RestorePoint.None)
    {
        //Debug -> time load game process
        GameManager.i.testScript.StartTimer();
        GameManager.i.fileScript.WriteSaveData(new LoadGameState() { gameState = state, restorePoint = restore }, SaveType.AutoSave);
        GameManager.i.fileScript.SaveGame(SaveType.AutoSave, true);
        //how long did it take?
        long timeElapsed = GameManager.i.testScript.StopTimer();
        Debug.LogFormat("[Per] ControlManager.cs -> ProcessSaveGame: SAVE GAME took {0} ms", timeElapsed);
    }
    #endregion

    #region ProcessEndCampaign
    /// <summary>
    /// win/Loss state achieved for end of campaign -> summaries, etc before exiting
    /// </summary>
    private void ProcessEndCampaign()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessEndCampaign: ProcessEndCampaign selected{0}", "\n");
        //close any Node tooltip
        GameManager.i.tooltipNodeScript.CloseTooltip("CityInfoUI.cs -> SetCityInfo");
        //stop animations
        GameManager.i.animateScript.StopAnimations();
        //modal block
        GameManager.i.guiScript.SetIsBlocked(true);
        //Open end level background
        GameManager.i.modalGUIScript.SetBackground(Background.EndCampaign);
        //change game state
        GameManager.i.inputScript.GameState = GameState.ExitCampaign;
        //end level campaign data
        GameManager.i.dataScript.SetCampaignHistoryEnd();
    }
    #endregion

    #region ProcessTutorialOptions
    /// <summary>
    /// Initialises and runs tutorial
    /// </summary>
    private void ProcessTutorialOptions()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessTutorialOptions: ProcessTutorialOptions selected{0}", "\n");
        //save existing game state
        gameState = GameManager.i.inputScript.GameState;
        //close MainMenu
        EventManager.i.PostNotification(EventType.CloseMainMenu, this, null, "ControlManager.cs -> ProcessTutorialOptions");
        //toggle on modal block -> after closeMainmenu (it resets block)
        GameManager.i.guiScript.SetIsBlocked(true);
        //set background
        GameManager.i.modalGUIScript.SetBackground(Background.TutorialOptions);
        //admin for new game
        GameManager.i.inputScript.GameState = GameState.NewInitialisation;
        //create new game -> DEBUG: resets campaign so assumes brand new campaign
        GameManager.i.campaignScript.Reset();
        //change game state
        GameManager.i.inputScript.GameState = GameState.TutorialOptions;
        //autoload last saved tutorial (if not already loaded) -> TO DO -> give options for all tutorials including their current states
        if (GameManager.i.fileScript.CheckTutorialSaveLoaded() == false)
        {
            GameManager.i.testScript.StartTimer();
            if (GameManager.i.fileScript.ReadSaveData(SaveType.Tutorial) == true)
            {
                GameManager.i.fileScript.LoadSaveData(SaveType.Tutorial);
            }
            long timeElapsed = GameManager.i.testScript.StopTimer();
            Debug.LogFormat("[Per] ControlManager.cs -> ProcessTutorialOptions: LOAD TUTORIAL took {0} ms", timeElapsed);
        }
        //set up tutorial
        GameManager.i.InitialiseTutorial();
        //toggle off finder UI
        EventManager.i.PostNotification(EventType.FinderSideTabClose, this, null, "ControlManager.cs -> ProcessTutorialOptions");
        //debug
        /*Debug.LogFormat("[Tst] ControlManager.cs -> ProcessTutorialOptions:  number of HQ Workers {0}", GameManager.i.dataScript.CheckHqWorkers());*/
    }
    #endregion

    #region ProcessCloseTutorialOptions
    //
    /// <summary>
    /// Closes tutorial screen and drops player into tutorial city
    /// </summary>
    private void ProcessCloseTutorialOptions()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessCloseTutorialOptions: ProcessCloseTutorialOptions selected{0}", "\n");
        //save existing game state
        gameState = GameManager.i.inputScript.GameState;
        //change game state
        GameManager.i.inputScript.GameState = GameState.Tutorial;
        //close background
        GameManager.i.modalGUIScript.CloseBackgrounds();
        //toggle off modal block
        GameManager.i.guiScript.SetIsBlocked(false);
        //start animations
        GameManager.i.animateScript.StartAnimations();
        //activate tutorialUI
        EventManager.i.PostNotification(EventType.TutorialOpenUI, this, GameManager.i.tutorialScript.set, "ControlManager.cs -> ProcessCloseTutorialOptions");
    }
    #endregion

    #region ProcessTutorialReturn
    /// <summary>
    /// Closes tutorial and returns to main Menu
    /// </summary>
    private void ProcessTutorialReturn()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessTutorialReturn: ProcessTutorialReturn to MainMenu selected{0}", "\n");

        //stop animations
        GameManager.i.animateScript.StopAnimations();
        EventManager.i.PostNotification(EventType.TutorialCloseUI, this, null, "ControlManager.cs -> ProcessTutorialReturn");
        //update dictionary
        GameManager.i.dataScript.UpdateTutorialIndex(GameManager.i.tutorialScript.tutorial.name, GameManager.i.tutorialScript.index);
        //Debug -> time save game process
        GameManager.i.testScript.StartTimer();
        GameManager.i.fileScript.WriteSaveData(new LoadGameState() { gameState = GameState.Tutorial, restorePoint = RestorePoint.None }, SaveType.Tutorial);
        GameManager.i.fileScript.SaveGame(SaveType.Tutorial);
        //how long did it take?
        long timeElapsed = GameManager.i.testScript.StopTimer();
        Debug.LogFormat("[Per] ControlManager.cs -> ProcessTutorialReturn: SAVE GAME took {0} ms", timeElapsed);
        //reset all GUI options (may have come from a tutorial)
        GameManager.i.optionScript.SetAllGUIOptionsToDefault();
        //game features (set according to FeatureManager in GameManager.cs prefab)
        GameManager.i.featureScript.InitialiseFeatures();
        //activate menu
        EventManager.i.PostNotification(EventType.OpenMainMenu, this, MainMenuType.Main, "GameManager.cs -> ProcessTutorialReturn");
    }
    #endregion

    #region ProcessGameReturn
    /// <summary>
    /// Closes game (playing) and returns to main Menu -> asks if you want to save first
    /// </summary>
    private void ProcessGameReturn()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessGameReturn: ProcessGameReturn to MainMenu selected{0}", "\n");
        //stop animations
        GameManager.i.animateScript.StopAnimations();
        //save dialogue if there is unsaved progress
        if (GameManager.i.fileScript.CheckSaveRequired() == true)
        {
            ModalConfirmDetails details = new ModalConfirmDetails()
            {
                topText = "Any recent progress will be lost unless Saved",
                bottomText = "Save progress?",
                buttonFalse = "Don't Save",
                buttonTrue = "SAVE",
                eventFalse = EventType.OpenMainMenu,
                eventTrue = EventType.SaveGameAndReturn
            };
            //open confirmation dialogue
            EventManager.i.PostNotification(EventType.ConfirmOpen, this, details, "ControlManager.cs -> ProcessGameReturn");
        }
        else
        {
            //returns to main menu (nothing to save, hence no save dialogue
            EventManager.i.PostNotification(EventType.OpenMainMenu, this, MainMenuType.Main, "ControlManager.cs -> ProcessGameReturn");
        }
    }
    #endregion

    #region ProcessSaveGameAndReturn
    /// <summary>
    /// Returning to main menu from game, saving progress on the way
    /// </summary>
    private void ProcessSaveGameAndReturn()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessSaveGameAndReturn: SAVE game option selected{0}", "\n");

        //toggle on modal block
        GameManager.i.guiScript.SetIsBlocked(true);
        //Save Game -> open background
        GameManager.i.modalGUIScript.SetBackground(Background.SaveGame);
        //Close any open background
        GameManager.i.modalGUIScript.CloseBackgrounds(Background.SaveGame);

        /*//check node display and reset back to normal if not prior to save
        if (GameManager.i.nodeScript.NodeShowFlag > 0)
        { GameManager.i.nodeScript.ResetAll(); }
        //check connection display and reset back to normal if not prior to save
        if (GameManager.i.connScript.resetConnections == true)
        { GameManager.i.connScript.RestoreConnections(); }*/

        //Debug -> time load game process
        GameManager.i.testScript.StartTimer();
        GameManager.i.fileScript.WriteSaveData(new LoadGameState() { gameState = GameState.PlayGame, restorePoint = RestorePoint.None }, SaveType.PlayerSave);
        GameManager.i.fileScript.SaveGame(SaveType.PlayerSave);
        //how long did it take?
        long timeElapsed = GameManager.i.testScript.StopTimer();
        Debug.LogFormat("[Per] ControlManager.cs -> ProcessSaveGameAndReturn: SAVE GAME took {0} ms", timeElapsed);
        //change game state
        GameManager.i.inputScript.GameState = GameState.SaveAndMain;

    }
    #endregion

    #region ProcessCloseSaveGameAndReturn
    /// <summary>
    /// Follow on from ProcessSaveGameAndReturn. Closes save background and opens main menu
    /// </summary>
    private void ProcessCloseSaveGameAndReturnToMainMenu()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> CloseSaveGameAndReturnToMainMenu: CloseSaveGameAndReturnToMainMenu selected{0}", "\n");
        //Close any open background
        GameManager.i.modalGUIScript.CloseBackgrounds();
        //toggle of modal block
        GameManager.i.guiScript.SetIsBlocked(false);
        //change game state
        GameManager.i.inputScript.GameState = GameState.MainMenu;
        //activate menu
        EventManager.i.PostNotification(EventType.OpenMainMenu, this, MainMenuType.Main, "GameManager.cs -> ProcessTutorialReturn");
    }
    #endregion

    #region ProcessCloseSaveGame
    /// <summary>
    /// Close save game screen
    /// </summary>
    private void ProcessCloseSaveGame()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> CloseSaveGame: CloseSaveGame selected{0}", "\n");
        //Close any open background
        GameManager.i.modalGUIScript.CloseBackgrounds();
        //toggle of modal block
        GameManager.i.guiScript.SetIsBlocked(false);
        //change game state
        GameManager.i.inputScript.GameState = GameState.PlayGame;
    }
    #endregion

    #region ProcessCloseGame
    /// <summary>
    /// Exit to desktop
    /// </summary>
    private void ProcessCloseGame()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessResumeGame: QUIT game option selected{0}", "\n");
        //shutdown animations
        GameManager.i.animateScript.StopAnimations();
        //save existing game state
        gameState = GameManager.i.inputScript.GameState;
        //quit game
        GameManager.i.turnScript.Quit();
    }
    #endregion

    #region ProcessCloseGameTutorial
    /// <summary>
    /// Exit to desktop from within Tutorial
    /// </summary>
    private void ProcessCloseGameTutorial()
    {
        Debug.LogFormat("[Ctrl] ControlManager.cs -> ProcessCloseGameTutorial: QUIT game option selected{0}", "\n");
        //shutdown animations
        GameManager.i.animateScript.StopAnimations();
        //save existing game state
        gameState = GameManager.i.inputScript.GameState;
        //update dictionary
        GameManager.i.dataScript.UpdateTutorialIndex(GameManager.i.tutorialScript.tutorial.name, GameManager.i.tutorialScript.index);
        //Debug -> time save game process
        GameManager.i.testScript.StartTimer();
        GameManager.i.fileScript.WriteSaveData(new LoadGameState() { gameState = GameState.Tutorial, restorePoint = RestorePoint.None }, SaveType.Tutorial);
        GameManager.i.fileScript.SaveGame(SaveType.Tutorial);
        //how long did it take?
        long timeElapsed = GameManager.i.testScript.StopTimer();
        Debug.LogFormat("[Per] ControlManager.cs -> ProcessCloseGameTutorial: SAVE GAME took {0} ms", timeElapsed);
        //quit game
        GameManager.i.turnScript.Quit();
    }
    #endregion

    #endregion

    #region Utilities...
    //
    // - - - Utilities
    //

    /// <summary>
    /// returns game state at time player opted for the selected option. Use during non-normal gameState checks, eg. Menu ops, Save/Load, otherwise inputScript.GameState
    /// </summary>
    /// <returns></returns>
    public GameState GetExistingGameState()
    { return gameState; }



    #endregion



    //new methods above here
}
