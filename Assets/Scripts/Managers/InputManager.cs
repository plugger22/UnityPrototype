using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using System.Text;

/// <summary>
/// class used for SetModalState, only main state needs to be specified as constructor has default options for subStates
/// </summary>
public class ModalStateData
{
    public ModalSubState mainState;
    public ModalInfoSubState infoState;
    public ModalGenericPickerSubState pickerState;

    public ModalStateData()
    {
        infoState = ModalInfoSubState.None;
        pickerState = ModalGenericPickerSubState.None;
    }
}

/// <summary>
/// handles all input & game states
/// </summary>
public class InputManager : MonoBehaviour
{
    private GameState _gameState;                                   //overall big picture game state
    private ModalState _modalState;                                 //main modal state status
    private ModalSubState _modalSubState;                           //sub state for when game state is 'ModalUI'
    private ModalInfoSubState _modalInfoState;                      //sub sub state of ModalState.InfoDisplay -> what type of info?
    private ModalGenericPickerSubState _modalGenericPickerState;    // sub state of ModalState.GenericPicker -> what type of picker?

    public void Initialise()
    {
        GameState = GameState.StartUp;
        ModalState = ModalState.Normal;
    }

    #region properties
    public GameState GameState
    {
        get { return _gameState; }
        set
        {
            _gameState = value;
            Debug.Log(string.Format("[Inp] InputManager: GameState now {0}{1}", _gameState, "\n"));
        }
    }

    //needs to be updated whenever changed
    public ModalState ModalState
    {
        get { return _modalState; }
        private set
        {
            _modalState = value;
            Debug.Log(string.Format("[Inp] InputManager: ModalState now {0}{1}", _modalState, "\n"));
        }
    }

    public ModalSubState ModalSubState
    {
        get { return _modalSubState; }
        private set
        {
            _modalSubState = value;
            Debug.Log(string.Format("[Inp] InputManager: ModalSubState now {0}{1}", _modalSubState, "\n"));
        }
    }

    public ModalInfoSubState ModalInfoState
    {
        get { return _modalInfoState; }
        private set
        {
            _modalInfoState = value;
            Debug.Log(string.Format("[Inp] InputManager.cs: ModalInfoState now {0}{1}", _modalInfoState, "\n"));
        }
    }

    public ModalGenericPickerSubState ModalGenericPickerState
    {
        get { return _modalGenericPickerState; }
        private set
        {
            _modalGenericPickerState = value;
            Debug.Log(string.Format("[Inp] InputManager.cs: ModalGenericPickerState now {0}{1}", _modalGenericPickerState, "\n"));
        }
    }
    #endregion

    #region Set and Reset Modal State
    /// <summary>
    /// Quick way of setting Modal state and Game State (to 'ModalUI'). If Modal state is 'InfoDisplay' provide a setting for ModalInfoState (type of info). Ignore otherwise
    /// </summary>
    /// <param name="modalState"></param>
    public void SetModalState(ModalStateData data)
    {
        if (data != null)
        {
            if (data.mainState != ModalSubState.None)
            { ModalState = gameAPI.ModalState.ModalUI; }
            ModalSubState = data.mainState;
            if (ModalSubState == ModalSubState.InfoDisplay)
            { ModalInfoState = data.infoState; }
            if (ModalSubState == ModalSubState.GenericPicker)
            { ModalGenericPickerState = data.pickerState; }
        }
        else { Debug.LogWarning("Invalid ModalStateData (Null)"); }
    }


    /// <summary>
    /// Quick way of reseting game and modal states back to defaults. If you input a modalState it will reset ModalState to this value, rather than the default 'None'
    /// Will only reset gamestate back to Normal if modalLevel is '0' (internal check)
    /// </summary>
    /// <param name="modal"></param>
    public void ResetStates(ModalSubState modal = ModalSubState.None)
    {
        ModalSubState = modal;
        //reset gamestate only if modalLevel is 0
        if (GameManager.instance.modalGUIScript.CheckModalLevel() == 0)
        {
            //only reset back to normal if there is no longer a modal state
            if (modal == ModalSubState.None)
            {
                ModalState = gameAPI.ModalState.Normal;
                ModalInfoState = ModalInfoSubState.None;
                ModalGenericPickerState = ModalGenericPickerSubState.None;
            }
        }
    }
    #endregion

    /// <summary>
    /// takes care of all input
    /// </summary>
    public void ProcessInput()
    {
        float x_axis, y_axis;

        //Modal and Game State dependant input -> NOTE: use Input.inputString only for normal key presses (won't pick up non-standard keypresses)
        switch (_modalState)
        {
            //
            // - - - Normal Modal state - - -
            //
            case ModalState.Normal:
                if (Input.GetButton("ExitLevel") == true)
                {
                    //can only exit while in Normal mode
                    switch (_gameState)
                    {
                        case GameState.PlayGame:
                            EventManager.instance.PostNotification(EventType.ExitLevel, this, null, string.Format("InputManager.cs -> ProcessInput ExitLevel \"{0}\"", Input.inputString.ToUpper()));
                            break;
                    }
                }
                else if (Input.GetButton("Cancel") == true)
                {
                    //can only exit while in Normal mode -> NOTE: use Input.inputString only for normal key presses (won't pick up non-standard keypresses)
                    switch (_gameState)
                    {
                        case GameState.MainMenu:
                            //close main menu
                            EventManager.instance.PostNotification(EventType.CloseMainMenu, this, null, "InputManager.cs -> ProcessInput Exit \"Cancel (ESC)\"");
                            break;
                        case GameState.ExitGame:
                            //do nothing, already exiting game
                            break;
                        default:
                            //all other options revert to main menu (default option of displaying over the top of whatever is present with no background initiated)
                            EventManager.instance.PostNotification(EventType.OpenMainMenu, this, null, "InputManager.cs -> ProcessInput Exit \"Cancel (ESC)\"");
                            break;
                    }
                }
                else if (Input.GetButton("Multipurpose") == true)
                {
                    switch (_gameState)
                    {
                        case GameState.NewGame:
                            EventManager.instance.PostNotification(EventType.NewGameOptions, this, null, string.Format("InputManager.cs -> ProcessInput \"Multipurpose (SPACE)\"", Input.inputString.ToUpper()));
                            break;
                        case GameState.NewGameOptions:
                            EventManager.instance.PostNotification(EventType.CloseNewGame, this, null, string.Format("InputManager.cs -> ProcessInput \"Multipurpose (SPACE)\"", Input.inputString.ToUpper()));
                            break;
                        case GameState.Options:
                            //close Options background -> Debug: need to set new Game State
                            EventManager.instance.PostNotification(EventType.CloseOptions, this, null, "InputManager.cs -> ProcessInput \"Multipurpose (SPACE)\"");
                            break;
                        case GameState.ExitLevel:
                            EventManager.instance.PostNotification(EventType.CreateMetaGame, this, null, string.Format("InputManager.cs -> ProcessInput \"Multipurpose (SPACE)\"", Input.inputString.ToUpper()));
                            break;
                        case GameState.MetaGame:
                            EventManager.instance.PostNotification(EventType.CloseMetaGame, this, null, string.Format("InputManager.cs -> ProcessInput \"Multipurpose (SPACE)\"", Input.inputString.ToUpper()));
                            break;
                        default:
                            //ignore all the rest
                            break;
                    }
                }
                else if (Input.GetButtonDown("NewTurn") == true)
                {
                    //Force a new turn (perhaps didn't want to take any actions), otherwise TurnManager.cs handles this once action quota used up
                    EventManager.instance.PostNotification(EventType.NewTurn, this, null, "InputManager.cs -> ProcessInput NewTurn");
                }
                else if (Input.GetButtonDown("ShowReserves") == true)
                {
                    EventManager.instance.PostNotification(EventType.InventorySetReserve, this, "Reserve Pool", string.Format("InputManager.cs -> ProcessInput ShowReserves \"{0}\"", Input.inputString.ToUpper()));
                }
                else if (Input.GetButtonDown("ShowGear") == true)
                {
                    EventManager.instance.PostNotification(EventType.InventorySetGear, this, "Gear Inventory", string.Format("InputManager.cs -> ProcessInput ShowGear \"{0}\"", Input.inputString.ToUpper()));
                }
                else if (Input.GetButtonDown("ShowTargets") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.ShowTargets, string.Format("InputManager.cs -> ProcessInput ShowTargets \"{0}\"", Input.inputString.ToUpper()));
                }
                else if (Input.GetButtonDown("ShowSpiders") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.ShowSpiders, string.Format("InputManager.cs -> ProcessInput ShowSpiders \"{0}\"", Input.inputString.ToUpper()));
                }
                else if (Input.GetButtonDown("ShowTracers") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.ShowTracers, string.Format("InputManager.cs -> ProcessInput ShowTracers \"{0}\"", Input.inputString.ToUpper()));
                }
                else if (Input.GetButtonDown("ShowTeams") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.ShowTeams, string.Format("InputManager.cs -> ProcessInput ShowTeams \"{0}\"", Input.inputString.ToUpper()));
                }
                //Generic NodeArc's 0 to 9 correspond to function keys F1 -> F10 and map directly to ArcTypeID's 0 to 9
                else if (Input.GetButtonDown("NodeArc0") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc0, "InputManager.cs -> ProcessInput NodeArc0");
                }
                else if (Input.GetButtonDown("NodeArc1") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc1, "InputManager.cs -> ProcessInput NodeArc1");
                }
                else if (Input.GetButtonDown("NodeArc2") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc2, "InputManager.cs -> ProcessInput NodeArc2");
                }
                else if (Input.GetButtonDown("NodeArc3") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc3, "InputManager.cs -> ProcessInput NodeArc3");
                }
                else if (Input.GetButtonDown("NodeArc4") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc4, "InputManager.cs -> ProcessInput NodeArc4");
                }
                else if (Input.GetButtonDown("NodeArc5") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc5, "InputManager.cs -> ProcessInput NodeArc5");
                }
                else if (Input.GetButtonDown("NodeArc6") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc6, "InputManager.cs -> ProcessInput NodeArc6");
                }
                else if (Input.GetButtonDown("NodeArc7") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc7, "InputManager.cs -> ProcessInput NodeArc7");
                }
                else if (Input.GetButtonDown("ActivityTime") == true)
                {
                    EventManager.instance.PostNotification(EventType.ActivityDisplay, this, ActivityUI.Time, "InputManager.cs -> ProcessInput ActivityTime");
                }
                else if (Input.GetButtonDown("ActivityCount") == true)
                {
                    EventManager.instance.PostNotification(EventType.ActivityDisplay, this, ActivityUI.Count, "InputManager.cs -> ProcessInput ActivityCount");
                }
                else if (Input.GetButtonDown("OpenMainInfo") == true)
                {
                    //Keyboard shortcut to open MainInfoApp between turns -> 'I'
                    EventManager.instance.PostNotification(EventType.MainInfoOpenInterim, this, null, string.Format("InputManager.cs -> ProcessInput OpenMainInfo \"{0}\"", Input.inputString.ToUpper()));
                }
                break;

            case gameAPI.ModalState.ModalUI:
                //Hotkeys for Modal UI windows
                switch (_modalSubState)
                {
                    case ModalSubState.Outcome:
                        if (Input.GetButtonDown("Cancel") == true)
                        {
                            EventManager.instance.PostNotification(EventType.CloseOutcomeWindow, this, null, "InputManager.cs -> ProcessInput Cancel");
                        }
                        else if (Input.GetButtonDown("Multipurpose") == true)
                        {
                            EventManager.instance.PostNotification(EventType.CloseOutcomeWindow, this, null, "InputManager.cs -> ProcessInput Multipurpose");
                        }
                        break;
                    case ModalSubState.Inventory:
                        if (Input.GetButtonDown("Cancel") == true)
                        {
                            EventManager.instance.PostNotification(EventType.InventoryCloseUI, this, null, "InputManager.cs -> ProcessInput Cancel");
                        }
                        else if (Input.GetButtonDown("Multipurpose") == true)
                        {
                            EventManager.instance.PostNotification(EventType.InventoryCloseUI, this, null, "InputManager.cs -> ProcessInput Multipurpose");
                        }
                        break;
                    case ModalSubState.TeamPicker:
                        if (Input.GetButtonDown("Cancel") == true)
                        {
                            EventManager.instance.PostNotification(EventType.CloseTeamPicker, this, null, "InputManager.cs -> ProcessInput Cancel");
                        }
                        else if (Input.GetButtonDown("Multipurpose") == true)
                        {
                            EventManager.instance.PostNotification(EventType.CloseTeamPicker, this, null, "InputManager.cs -> ProcessInput Multipurpose");
                        }
                        break;
                    case ModalSubState.GenericPicker:
                        switch (_modalGenericPickerState)
                        {
                            case ModalGenericPickerSubState.Normal:
                                if (Input.GetButtonDown("Cancel") == true)
                                {
                                    EventManager.instance.PostNotification(EventType.CloseGenericPicker, this, null, "InputManager.cs -> ProcessInput Cancel");
                                }
                                else if (Input.GetButtonDown("Multipurpose") == true)
                                {
                                    EventManager.instance.PostNotification(EventType.CloseGenericPicker, this, null, "InputManager.cs -> ProcessInput Multipurpose");
                                }
                                break;
                            case ModalGenericPickerSubState.CompromisedGear:
                                if (Input.GetButtonDown("Cancel") == true)
                                {
                                    EventManager.instance.PostNotification(EventType.CloseGenericPicker, this, null, "InputManager.cs -> ProcessInput Cancel");
                                    EventManager.instance.PostNotification(EventType.GenericCompromisedGear, this, null, "InputManager.cs -> ProcessInput Cancel");
                                }
                                else if (Input.GetButtonDown("Multipurpose") == true)
                                {
                                    EventManager.instance.PostNotification(EventType.CloseGenericPicker, this, null, "InputManager.cs -> ProcessInput Multipurpose");
                                    EventManager.instance.PostNotification(EventType.GenericCompromisedGear, this, null, "InputManager.cs -> ProcessInput Multipurpose");
                                }
                                break;
                        }
                        break;

                    case ModalSubState.ActionMenu:
                        if (Input.GetButtonDown("Cancel") == true)
                        {
                            EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "InputManager.cs -> ProcessInput Cancel");
                        }
                        break;
                    case ModalSubState.InfoDisplay:
                        //info displays are all at ModalLevel 1. Ignore commands if level > 1, eg. outcome window open on top of an info display.
                        if (GameManager.instance.modalGUIScript.CheckModalLevel() == 1)
                        {
                            //what type of info display?
                            switch (_modalInfoState)
                            {
                                case ModalInfoSubState.CityInfo:
                                    if (Input.GetButtonDown("Cancel") == true)
                                    {
                                        EventManager.instance.PostNotification(EventType.CityInfoClose, this, null, "InputManager.cs -> ProcessInput Cancel");
                                    }
                                    break;
                                case ModalInfoSubState.AIInfo:
                                    if (Input.GetButtonDown("Cancel") == true)
                                    {
                                        EventManager.instance.PostNotification(EventType.AIDisplayClose, this, null, "InputManager.cs -> ProcessInput Cancel");
                                    }
                                    break;
                                case ModalInfoSubState.MainInfo:
                                    if (Input.GetButtonDown("Cancel") == true)
                                    {
                                        EventManager.instance.PostNotification(EventType.MainInfoClose, this, null, "InputManager.cs -> ProcessInput Cancel");
                                    }
                                    else if (Input.GetButtonDown("Vertical"))
                                    {
                                        y_axis = Input.GetAxisRaw("Vertical");
                                        if (y_axis > 0)
                                        { EventManager.instance.PostNotification(EventType.MainInfoUpArrow, this, null, "InputManager.cs -> ProcessInput Vertical"); }
                                        else if (y_axis < 0)
                                        { EventManager.instance.PostNotification(EventType.MainInfoDownArrow, this, null, "InputManager.cs -> ProcessInput Vertical"); }
                                    }
                                    else if (Input.GetButtonDown("Horizontal"))
                                    {
                                        x_axis = Input.GetAxisRaw("Horizontal");
                                        if (x_axis > 0)
                                        { EventManager.instance.PostNotification(EventType.MainInfoRightArrow, this, null, "InputManager.cs -> ProcessInput Horizontal"); }
                                        else if (x_axis < 0)
                                        { EventManager.instance.PostNotification(EventType.MainInfoLeftArrow, this, null, "InputManager.cs -> ProcessInput Horizontal"); }
                                    }
                                    else if (Input.GetButtonDown("Multipurpose") == true)
                                    {
                                        //Space bar is keyboard shortcut for 'Show Me' button
                                        EventManager.instance.PostNotification(EventType.MainInfoShowMe, this, null, "InputManager.cs -> ProcessInput Multipurpose");
                                        Input.ResetInputAxes();
                                    }
                                    else if (Input.GetButtonDown("DayAhead") == true)
                                    {
                                        //Keyboard shortcut for forward a day -> PgUp
                                        EventManager.instance.PostNotification(EventType.MainInfoForward, this, null, string.Format("InputManager.cs -> ProcessInput DayAhead \"{0}\"", Input.inputString.ToUpper()));
                                    }
                                    else if (Input.GetButtonDown("DayBehind") == true)
                                    {
                                        //Keyboard shortcut for back a day -> PgUp
                                        EventManager.instance.PostNotification(EventType.MainInfoBack, this, null, string.Format("InputManager.cs -> ProcessInput DayBehind \"{0}\"", Input.inputString.ToUpper()));
                                    }
                                    else if (Input.GetButtonDown("CurrentDay") == true)
                                    {
                                        //Keyboard shortcut for go to current day (Home) day -> Home
                                        EventManager.instance.PostNotification(EventType.MainInfoHome, this, null, string.Format("InputManager.cs -> ProcessInput CurrentDay \"{0}\"", Input.inputString.ToUpper()));
                                    }
                                    else if (Input.GetButtonDown("StartDay") == true)
                                    {
                                        //Keyboard shortcut for go to start (day 1) -> End
                                        EventManager.instance.PostNotification(EventType.MainInfoEnd, this, null, string.Format("InputManager.cs -> ProcessInput StartDay \"{0}\"", Input.inputString.ToUpper()));
                                    }
                                    else if (Input.GetButtonDown("Plus") == true)
                                    {
                                        //Keyboard shortcut to increase speed of ticker tape
                                        EventManager.instance.PostNotification(EventType.MainInfoTickerFaster, this, null, string.Format("InputManager.cs -> ProcessInput Plus \"{0}\"", Input.inputString.ToUpper()));
                                    }
                                    else if (Input.GetButtonDown("Minus") == true)
                                    {
                                        //Keyboard shortcut to decrease speed of ticker tape
                                        EventManager.instance.PostNotification(EventType.MainInfoTickerSlower, this, null, string.Format("InputManager.cs -> ProcessInput Minus \"{0}\"", Input.inputString.ToUpper()));
                                    }
                                    break;
                            }
                        }
                        break;
                    case ModalSubState.ShowMe:
                        //'Show Me' -> retore infoApp
                        if (Input.anyKeyDown == true)
                        { EventManager.instance.PostNotification(EventType.ShowMeRestore, this, null, "InputManager.cs -> ProcessInput ShowMe"); }
                        break;
                }
                break;
        }
    }


    #region DisplayGameState
    /// <summary>
    /// Debug method to show game states
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayGameState()
    {
        StringBuilder builder = new StringBuilder();
        int modalLevel = GameManager.instance.modalGUIScript.CheckModalLevel();
        builder.Append(" Game States");
        builder.AppendLine(); builder.AppendLine();
        builder.AppendFormat(" GameState -> {0}{1}", GameState, "\n");
        builder.AppendFormat(" ModalState -> {0}{1}", ModalState, "\n");
        builder.AppendFormat(" ModalSubState -> {0}{1}", ModalSubState, "\n");
        builder.AppendFormat(" ModalLevel -> {0}{1}", modalLevel, "\n");
        builder.AppendFormat(" ModalInfo -> {0}{1}", ModalInfoState, "\n");
        builder.AppendFormat(" isBlocked -> {0}{1}", GameManager.instance.guiScript.CheckIsBlocked(modalLevel), "\n");
        builder.AppendFormat(" {0} PlayerSide -> {1}{2}{3}", "\n", GameManager.instance.sideScript.PlayerSide.name, "\n", "\n");
        builder.AppendFormat(" currentSide -> {0}{1}", GameManager.instance.turnScript.currentSide.name, "\n");
        builder.AppendFormat(" AuthorityCurrent -> {0}{1}", GameManager.instance.sideScript.authorityCurrent, "\n");
        builder.AppendFormat(" ResistanceCurrent -> {0}{1}{2}", GameManager.instance.sideScript.resistanceCurrent, "\n", "\n");
        builder.AppendFormat(" AuthorityOverall -> {0}{1}", GameManager.instance.sideScript.authorityOverall, "\n");
        builder.AppendFormat(" ResistanceOverall -> {0}{1}{2}", GameManager.instance.sideScript.resistanceOverall, "\n", "\n");
        builder.AppendFormat(" AuthorityState -> {0}{1}", GameManager.instance.turnScript.authoritySecurityState, "\n");
        builder.AppendFormat(" ResistanceState -> {0}{1}{2}", GameManager.instance.turnScript.resistanceState, "\n", "\n");
        builder.AppendFormat(" immediateAuthorityFlag -> {0}{1}", GameManager.instance.aiScript.immediateFlagAuthority, "\n");
        builder.AppendFormat(" immediateResistanceFlag -> {0}{1}{2}", GameManager.instance.aiScript.immediateFlagResistance, "\n", "\n");
        builder.AppendFormat(" Authority Player -> {0}{1}", GameManager.instance.playerScript.GetPlayerNameAuthority(), "\n");
        builder.AppendFormat(" Resistance Player -> {0}{1}{2}", GameManager.instance.playerScript.GetPlayerNameResistance(), "\n", "\n");
        builder.AppendFormat(" HQ Approval Authority -> {0}{1}", GameManager.instance.factionScript.ApprovalAuthority, "\n");
        builder.AppendFormat(" HQ Approval Resistance -> {0}{1}", GameManager.instance.factionScript.ApprovalResistance, "\n");
        return builder.ToString();
    }
    #endregion

    //new methods above here
}
