using gameAPI;
using System.Text;
using UnityEngine;

/// <summary>
/// class used for SetModalState, only main state needs to be specified as constructor has default options for subStates
/// </summary>
public class ModalStateData
{
    public ModalSubState mainState;
    public ModalInfoSubState infoState;
    public ModalGenericPickerSubState pickerState;
    public ModalInventorySubState inventoryState;
    public ModalMetaSubState metaState;

    public ModalStateData()
    {
        infoState = ModalInfoSubState.None;
        pickerState = ModalGenericPickerSubState.None;
        inventoryState = ModalInventorySubState.None;
        metaState = ModalMetaSubState.None;
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
    private ModalInfoSubState _modalInfoState;                      //sub sub state of ModalSubState.InfoDisplay -> what type of info?
    private ModalGenericPickerSubState _modalGenericPickerState;    //sub state of ModalSubState.GenericPicker -> what type of picker?
    private ModalMetaSubState _modalMetaState;                      //sub state of ModalSubState.MetaGame -> what type of MetaUI?
    private ModalInventorySubState _modalInventoryState;                //sub state of ModalSubState.Inventory
    private ModalReviewSubState _modalReviewState;                  //sub state for ModalReviewUI

    

    public void Initialise(GameState state)
    {
        /*GameState = GameState.StartUp;*/
        ModalState = ModalState.Normal;
    }

    #region properties
    public GameState GameState
    {
        get { return _gameState; }
        set
        {
            _gameState = value;
            Debug.LogFormat("[Inp] InputManager: GameState now {0}{1}", _gameState, "\n");
        }
    }

    //needs to be updated whenever changed
    public ModalState ModalState
    {
        get { return _modalState; }
        private set
        {
            _modalState = value;
            Debug.LogFormat("[Inp] InputManager: ModalState now {0}{1}", _modalState, "\n");
        }
    }

    public ModalSubState ModalSubState
    {
        get { return _modalSubState; }
        private set
        {
            _modalSubState = value;
            Debug.LogFormat("[Inp] InputManager: ModalSubState now {0}{1}", _modalSubState, "\n");
        }
    }

    public ModalInfoSubState ModalInfoState
    {
        get { return _modalInfoState; }
        private set
        {
            _modalInfoState = value;
            Debug.LogFormat("[Inp] InputManager.cs: ModalInfoState now {0}{1}", _modalInfoState, "\n");
        }
    }

    public ModalGenericPickerSubState ModalGenericPickerState
    {
        get { return _modalGenericPickerState; }
        private set
        {
            _modalGenericPickerState = value;
            Debug.LogFormat("[Inp] InputManager.cs: ModalGenericPickerState now {0}{1}", _modalGenericPickerState, "\n");
        }
    }

    public ModalMetaSubState ModalMetaState
    {
        get { return _modalMetaState; }
        private set
        {
            _modalMetaState = value;
            Debug.LogFormat("[Inp] InputManager.cs: ModalMetaState now {0}{1}", _modalMetaState, "\n");
        }
    }

    public ModalInventorySubState ModalInventoryState
    {
        get { return _modalInventoryState; }
        private set
        {
            _modalInventoryState = value;
            Debug.LogFormat("[Inp] InputManager.cs: ModalInventoryState now {0}{1}", _modalInventoryState, "\n");
        }
    }

    public ModalReviewSubState ModalReviewState
    {
        get { return _modalReviewState; }
        set
        {
            _modalReviewState = value;
            Debug.LogFormat("[Inp] InputManager.cs: ModalReviewSubState now {0}{1}", _modalReviewState, "\n");
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
            ModalSubState = data.mainState;
            //main
            if (data.mainState != ModalSubState.None)
            { ModalState = ModalState.ModalUI; }
            //main Info App
            if (ModalSubState == ModalSubState.InfoDisplay)
            { ModalInfoState = data.infoState; }
            //picker
            if (ModalSubState == ModalSubState.GenericPicker)
            { ModalGenericPickerState = data.pickerState; }
            //metaGame
            if (ModalSubState == ModalSubState.MetaGame)
            { ModalMetaState = data.metaState; }
            //inventory
            if (ModalSubState == ModalSubState.Inventory)
            { ModalInventoryState = data.inventoryState; }
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
        if (GameManager.i.modalGUIScript.CheckModalLevel() == 0)
        {
            //only reset back to normal if there is no longer a modal state
            if (modal == ModalSubState.None)
            {
                ModalState = ModalState.Normal;
                ModalInfoState = ModalInfoSubState.None;
                ModalMetaState = ModalMetaSubState.None;
                ModalGenericPickerState = ModalGenericPickerSubState.None;
                ModalInventoryState = ModalInventorySubState.None;
            }
        }
    }
    #endregion

    /// <summary>
    /// takes care of all key and mouse press input (excludes mouse wheel movement -> see ProcessMouseWheelInput)
    /// </summary>
    public void ProcessKeyInput()
    {
        float x_axis, y_axis;
        //
        // - - - Global options -> Apply modal or non-modal
        //
        if (Input.GetButtonDown("ActorInfo") == true)
        {
            //Toggle Actor Info display between Renown and Compatibility
            EventManager.i.PostNotification(EventType.ActorInfo, this, "ActorInfo Toggle", string.Format("InputManager.cs -> ProcessKeyInput ActorInfo"));
        }
        else if (Input.GetButtonDown("Test") == true)
        {
            //runs a test condition (whatever you want)
            GameManager.i.playerScript.DebugGiveRenown();
        }
        else
        {
            //
            // - - - Modal and Game State dependant input -> NOTE: use Input.inputString only for normal key presses (won't pick up non-standard keypresses)
            //
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
                                EventManager.i.PostNotification(EventType.ExitLevel, this, null, string.Format("InputManager.cs -> ProcessKeyInput ExitLevel \"{0}\"", Input.inputString.ToUpper()));
                                break;
                        }
                    }
                    else if (Input.GetButton("Cancel") == true)     //ESC
                    {
                        //can only exit while in Normal mode -> NOTE: use Input.inputString only for normal key presses (won't pick up non-standard keypresses)
                        switch (_gameState)
                        {
                            case GameState.MainMenu:
                                //close main menu
                                EventManager.i.PostNotification(EventType.CloseMainMenu, this, null, "InputManager.cs -> ProcessKeyInput Exit \"Cancel (ESC)\"");
                                break;
                            case GameState.ExitGame:
                                //do nothing, already exiting game
                                break;
                            default:
                                //if debug overlay on then switch this off first before any game UI element
                                if (GameManager.i.debugScript.showGUI == false)
                                {
                                    //all other options revert to main menu (default option of displaying over the top of whatever is present with no background initiated)
                                    EventManager.i.PostNotification(EventType.OpenMainMenu, this, null, "InputManager.cs -> ProcessKeyInput Exit \"Cancel (ESC)\"");
                                }
                                else { GameManager.i.debugScript.showGUI = false; }
                                break;
                        }
                    }
                    else if (Input.GetButton("Multipurpose") == true)  //SPACEBAR
                    {
                        switch (_gameState)
                        {
                            case GameState.NewGame:
                                EventManager.i.PostNotification(EventType.NewGameOptions, this, null, "InputManager.cs -> ProcessKeyInput \"Multipurpose (SPACE)\"");
                                break;
                            case GameState.NewGameOptions:
                                EventManager.i.PostNotification(EventType.CloseNewGame, this, null, "InputManager.cs -> ProcessKeyInput \"Multipurpose (SPACE)\"");
                                break;
                            case GameState.LoadGame:
                            case GameState.LoadAtStart:
                                EventManager.i.PostNotification(EventType.CloseLoadGame, this, null, "InputManager.cs -> ProcessKeyInput \"Multipurpose (SPACE)\"");
                                break;
                            case GameState.SaveGame:
                                EventManager.i.PostNotification(EventType.CloseSaveGame, this, null, "InputManager.cs -> ProcessKeyInput \"Multipurpose (SPACE)\"");
                                break;
                            case GameState.Options:
                                //close Options background -> Debug: need to set new Game State
                                EventManager.i.PostNotification(EventType.CloseOptions, this, null, "InputManager.cs -> ProcessKeyInput \"Multipurpose (SPACE)\"");
                                break;
                            case GameState.ExitLevel:
                                EventManager.i.PostNotification(EventType.CreateMetaGame, this, null, "InputManager.cs -> ProcessKeyInput \"Multipurpose (SPACE)\"");
                                break;
                            case GameState.MetaGame:
                                EventManager.i.PostNotification(EventType.CloseMetaGame, this, null, "InputManager.cs -> ProcessKeyInput \"Multipurpose (SPACE)\"");
                                break;
                            case GameState.ExitCampaign:
                                EventManager.i.PostNotification(EventType.ExitGame, this, null, "InputManager.cs -> ProcessKeyInput \"Multipurpose (SPACE)\"");
                                break;
                            default:
                                //ignore all the rest
                                break;
                        }
                    }
                    else if (Input.GetButtonDown("NewTurn") == true)    //ENTER
                    {
                        //new turn only if in normal play game state
                        if (GameState == GameState.PlayGame)
                        {
                            //Force a new turn (perhaps didn't want to take any actions), otherwise TurnManager.cs handles this once action quota used up
                            EventManager.i.PostNotification(EventType.NewTurn, this, null, "InputManager.cs -> ProcessKeyInput NewTurn");
                        }
                    }
                    else if (Input.GetButtonDown("ShowReserves") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.InventorySetReserve, this, "Reserve Pool", string.Format("InputManager.cs -> ProcessKeyInput ShowReserves \"{0}\"", Input.inputString.ToUpper())); }
                    }
                    else if (Input.GetButtonDown("ShowGear") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.InventorySetGear, this, "Gear Inventory", string.Format("InputManager.cs -> ProcessKeyInput ShowGear \"{0}\"", Input.inputString.ToUpper())); }
                    }
                    else if (Input.GetButtonDown("ShowHQ") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.InventorySetHQ, this, "HQ Inventory", string.Format("InputManager.cs -> ProcessKeyInput ShowHQ \"{0}\"", Input.inputString.ToUpper())); }
                    }
                    else if (Input.GetButtonDown("ShowDevices") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.InventorySetDevice, this, "Device Inventory", string.Format("InputManager.cs -> ProcessKeyInput ShowDevices \"{0}\"", Input.inputString.ToUpper())); }
                    }
                    else if (Input.GetButtonDown("ShowTargets") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.ShowTargets, string.Format("InputManager.cs -> ProcessKeyInput ShowTargets \"{0}\"", Input.inputString.ToUpper())); }
                    }
                    else if (Input.GetButtonDown("ShowSpiders") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.ShowSpiders, string.Format("InputManager.cs -> ProcessKeyInput ShowSpiders \"{0}\"", Input.inputString.ToUpper())); }
                    }
                    else if (Input.GetButtonDown("ShowTracers") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.ShowTracers, string.Format("InputManager.cs -> ProcessKeyInput ShowTracers \"{0}\"", Input.inputString.ToUpper())); }
                    }
                    else if (Input.GetButtonDown("ShowTeams") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.ShowTeams, string.Format("InputManager.cs -> ProcessKeyInput ShowTeams \"{0}\"", Input.inputString.ToUpper())); }
                    }
                    else if (Input.GetButtonDown("ShowAutoRun") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { GameManager.i.sideScript.ShowAutoRunMessage(); }
                    }
                    //Generic NodeArc's 0 to 9 correspond to function keys F1 -> F10 and map directly to ArcTypeID's 0 to 9
                    else if (Input.GetButtonDown("NodeArc0") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc0, "InputManager.cs -> ProcessKeyInput NodeArc0"); }
                    }
                    else if (Input.GetButtonDown("NodeArc1") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc1, "InputManager.cs -> ProcessKeyInput NodeArc1"); }
                    }
                    else if (Input.GetButtonDown("NodeArc2") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc2, "InputManager.cs -> ProcessKeyInput NodeArc2"); }
                    }
                    else if (Input.GetButtonDown("NodeArc3") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc3, "InputManager.cs -> ProcessKeyInput NodeArc3"); }
                    }
                    else if (Input.GetButtonDown("NodeArc4") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc4, "InputManager.cs -> ProcessKeyInput NodeArc4"); }
                    }
                    else if (Input.GetButtonDown("NodeArc5") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc5, "InputManager.cs -> ProcessKeyInput NodeArc5"); }
                    }
                    else if (Input.GetButtonDown("NodeArc6") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc6, "InputManager.cs -> ProcessKeyInput NodeArc6"); }
                    }
                    else if (Input.GetButtonDown("PlayerKnown") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.PlayerKnown, "InputManager.cs -> ProcessKeyInput PlayerKnown"); }
                    }
                    else if (Input.GetButtonDown("ActivityTime") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.ActivityDisplay, this, ActivityUI.Time, "InputManager.cs -> ProcessKeyInput ActivityTime"); }
                    }
                    else if (Input.GetButtonDown("ActivityCount") == true)
                    {
                        //only do so if new turn processing hasn't commenced
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.ActivityDisplay, this, ActivityUI.Count, "InputManager.cs -> ProcessKeyInput ActivityCount"); }
                    }
                    else if (Input.GetButtonDown("OpenMainInfo") == true)
                    {
                        //only do so if new turn processing hasn't commenced -> Keyboard shortcut to open MainInfoApp between turns -> 'I'
                        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                        { EventManager.i.PostNotification(EventType.MainInfoOpenInterim, this, null, string.Format("InputManager.cs -> ProcessKeyInput OpenMainInfo \"{0}\"", Input.inputString.ToUpper())); }
                    }
                    break;
                case ModalState.ModalUI:
                    //Hotkeys for Modal UI windows
                    switch (_modalSubState)
                    {
                        case ModalSubState.Outcome:
                            if (Input.GetButtonDown("Cancel") == true)
                            {
                                EventManager.i.PostNotification(EventType.OutcomeClose, this, null, "InputManager.cs -> ProcessKeyInput Cancel");
                            }
                            else if (Input.GetButtonDown("Multipurpose") == true)
                            {
                                EventManager.i.PostNotification(EventType.OutcomeClose, this, null, "InputManager.cs -> ProcessKeyInput Multipurpose");
                            }
                            break;
                        case ModalSubState.Inventory:
                            if (Input.GetButtonDown("Cancel") == true)
                            {
                                EventManager.i.PostNotification(EventType.InventoryCloseUI, this, null, "InputManager.cs -> ProcessKeyInput Cancel");
                            }
                            else if (Input.GetButtonDown("Multipurpose") == true)
                            {
                                EventManager.i.PostNotification(EventType.InventoryCloseUI, this, null, "InputManager.cs -> ProcessKeyInput Multipurpose");
                            }
                            else if (Input.GetButtonDown("ShowReserves") == true)
                            {
                                if (ModalInventoryState == ModalInventorySubState.ReservePool)
                                { EventManager.i.PostNotification(EventType.InventoryCloseUI, this, null, string.Format("InputManager.cs -> ProcessKeyInput ShowReserves \"{0}\"", Input.inputString.ToUpper())); }
                            }
                            else if (Input.GetButtonDown("ShowGear") == true)
                            {
                                if (ModalInventoryState == ModalInventorySubState.Gear)
                                { EventManager.i.PostNotification(EventType.InventoryCloseUI, this, null, string.Format("InputManager.cs -> ProcessKeyInput ShowGear \"{0}\"", Input.inputString.ToUpper())); }
                            }
                            else if (Input.GetButtonDown("ShowHQ") == true)
                            {
                                if (ModalInventoryState == ModalInventorySubState.HQ)
                                { EventManager.i.PostNotification(EventType.InventoryCloseUI, this, null, string.Format("InputManager.cs -> ProcessKeyInput ShowHQ \"{0}\"", Input.inputString.ToUpper())); }
                            }
                            else if (Input.GetButtonDown("ShowDevices") == true)
                            {
                                if (ModalInventoryState == ModalInventorySubState.CaptureTool)
                                { EventManager.i.PostNotification(EventType.InventoryCloseUI, this, null, string.Format("InputManager.cs -> ProcessKeyInput ShowDevices \"{0}\"", Input.inputString.ToUpper())); }
                            }
                            break;
                        case ModalSubState.TeamPicker:
                            if (Input.GetButtonDown("Cancel") == true)
                            {
                                EventManager.i.PostNotification(EventType.CloseTeamPicker, this, null, "InputManager.cs -> ProcessKeyInput Cancel");
                            }
                            else if (Input.GetButtonDown("Multipurpose") == true)
                            {
                                EventManager.i.PostNotification(EventType.CloseTeamPicker, this, null, "InputManager.cs -> ProcessKeyInput Multipurpose");
                            }
                            break;
                        case ModalSubState.GenericPicker:
                            switch (_modalGenericPickerState)
                            {
                                case ModalGenericPickerSubState.Normal:
                                    if (Input.GetButtonDown("Cancel") == true)
                                    {
                                        EventManager.i.PostNotification(EventType.CloseGenericPicker, this, null, "InputManager.cs -> ProcessKeyInput Cancel");
                                    }
                                    else if (Input.GetButtonDown("Multipurpose") == true)
                                    {
                                        EventManager.i.PostNotification(EventType.CloseGenericPicker, this, null, "InputManager.cs -> ProcessKeyInput Multipurpose");
                                    }
                                    break;
                                case ModalGenericPickerSubState.CompromisedGear:
                                    if (Input.GetButtonDown("Cancel") == true)
                                    {
                                        EventManager.i.PostNotification(EventType.CloseGenericPicker, this, null, "InputManager.cs -> ProcessKeyInput Cancel");
                                        EventManager.i.PostNotification(EventType.GenericCompromisedGear, this, null, "InputManager.cs -> ProcessKeyInput Cancel");
                                    }
                                    else if (Input.GetButtonDown("Multipurpose") == true)
                                    {
                                        EventManager.i.PostNotification(EventType.CloseGenericPicker, this, null, "InputManager.cs -> ProcessKeyInput Multipurpose");
                                        EventManager.i.PostNotification(EventType.GenericCompromisedGear, this, null, "InputManager.cs -> ProcessKeyInput Multipurpose");
                                    }
                                    break;
                            }
                            break;
                        case ModalSubState.Review:
                            switch (_modalReviewState)
                            {
                                case ModalReviewSubState.Open:
                                    if (Input.GetButtonDown("Cancel") == true)
                                    { EventManager.i.PostNotification(EventType.ReviewStart, this, null, "InputManager.cs -> ProcessKeyInput Cancel"); }
                                    else if (Input.GetButtonDown("Multipurpose") == true)
                                    { EventManager.i.PostNotification(EventType.ReviewStart, this, null, "InputManager.cs -> ProcessKeyInput Multipurpose"); }
                                    break;
                                case ModalReviewSubState.Review:
                                    if (Input.GetButtonDown("Multipurpose") == true)
                                    {
                                        Debug.LogFormat("[Inp] InputManager.cs -> ProcessKeyInput: SKIP REVIEW{0}", "\n");
                                        GameManager.i.reviewScript.reviewWaitTime = 0.0f;
                                    }
                                    break;
                                case ModalReviewSubState.Close:
                                    if (Input.GetButtonDown("Cancel") == true)
                                    { EventManager.i.PostNotification(EventType.ReviewCloseUI, this, null, "InputManager.cs -> ProcessKeyInput Cancel"); }
                                    break;
                            }
                            break;
                        case ModalSubState.ActionMenu:
                            if (Input.GetButtonDown("Cancel") == true)
                            {
                                EventManager.i.PostNotification(EventType.CloseActionMenu, this, null, "InputManager.cs -> ProcessKeyInput Cancel");
                            }
                            break;
                        case ModalSubState.Topic:
                            if (Input.GetButtonDown("Cancel") == true)
                            { EventManager.i.PostNotification(EventType.TopicDisplayIgnore, this, null, "InputManager.cs -> ProcessKeyInput Cancel"); }
                            else if (Input.GetButtonDown("Multipurpose") == true)
                            {
                                //Space bar is keyboard shortcut for 'Show Me' button
                                EventManager.i.PostNotification(EventType.TopicDisplayShowMe, this, null, "InputManager.cs -> ProcessKeyInput Multipurpose");
                                Input.ResetInputAxes();
                            }
                            break;
                        case ModalSubState.InfoDisplay:
                            //info displays are all at ModalLevel 1. Ignore commands if level > 1, eg. outcome window open on top of an info display.
                            if (GameManager.i.modalGUIScript.CheckModalLevel() == 1)
                            {
                                //what type of info display?
                                switch (_modalInfoState)
                                {
                                    case ModalInfoSubState.CityInfo:
                                        if (Input.GetButtonDown("Cancel") == true)
                                        {
                                            EventManager.i.PostNotification(EventType.CityInfoClose, this, null, "InputManager.cs -> ProcessKeyInput Cancel");
                                        }
                                        break;
                                    case ModalInfoSubState.AIInfo:
                                        if (Input.GetButtonDown("Cancel") == true)
                                        {
                                            EventManager.i.PostNotification(EventType.AIDisplayClose, this, null, "InputManager.cs -> ProcessKeyInput Cancel");
                                        }
                                        break;
                                    case ModalInfoSubState.MainInfo:
                                        if (Input.GetButtonDown("Cancel") == true)
                                        {
                                            EventManager.i.PostNotification(EventType.MainInfoClose, this, null, "InputManager.cs -> ProcessKeyInput Cancel");
                                        }
                                        else if (Input.GetButtonDown("Vertical"))
                                        {
                                            y_axis = Input.GetAxisRaw("Vertical");
                                            if (y_axis > 0)
                                            { EventManager.i.PostNotification(EventType.MainInfoUpArrow, this, null, "InputManager.cs -> ProcessKeyInput Vertical"); }
                                            else if (y_axis < 0)
                                            { EventManager.i.PostNotification(EventType.MainInfoDownArrow, this, null, "InputManager.cs -> ProcessKeyInput Vertical"); }
                                        }
                                        else if (Input.GetButtonDown("Horizontal"))
                                        {
                                            x_axis = Input.GetAxisRaw("Horizontal");
                                            if (x_axis > 0)
                                            { EventManager.i.PostNotification(EventType.MainInfoRightArrow, this, null, "InputManager.cs -> ProcessKeyInput Horizontal"); }
                                            else if (x_axis < 0)
                                            { EventManager.i.PostNotification(EventType.MainInfoLeftArrow, this, null, "InputManager.cs -> ProcessKeyInput Horizontal"); }
                                        }
                                        else if (Input.GetButtonDown("Multipurpose") == true)
                                        {
                                            //Space bar is keyboard shortcut for 'Show Me' button
                                            EventManager.i.PostNotification(EventType.MainInfoShowMe, this, null, "InputManager.cs -> ProcessKeyInput Multipurpose");
                                            Input.ResetInputAxes();
                                        }
                                        else if (Input.GetButtonDown("PageUp") == true)
                                        {
                                            //Keyboard shortcut for forward a day -> PgUp
                                            EventManager.i.PostNotification(EventType.MainInfoForward, this, null, string.Format("InputManager.cs -> ProcessKeyInput DayAhead (PageUp) \"{0}\"", Input.inputString.ToUpper()));
                                        }
                                        else if (Input.GetButtonDown("PageDown") == true)
                                        {
                                            //Keyboard shortcut for back a day -> PgDown
                                            EventManager.i.PostNotification(EventType.MainInfoBack, this, null, string.Format("InputManager.cs -> ProcessKeyInput DayBehind (PageDown) \"{0}\"", Input.inputString.ToUpper()));
                                        }
                                        else if (Input.GetButtonDown("CurrentDay") == true)
                                        {
                                            //Keyboard shortcut for go to current day (Home) day -> Home
                                            EventManager.i.PostNotification(EventType.MainInfoHome, this, null, string.Format("InputManager.cs -> ProcessKeyInput CurrentDay \"{0}\"", Input.inputString.ToUpper()));
                                        }
                                        else if (Input.GetButtonDown("StartDay") == true)
                                        {
                                            //Keyboard shortcut for go to start (day 1) -> End
                                            EventManager.i.PostNotification(EventType.MainInfoEnd, this, null, string.Format("InputManager.cs -> ProcessKeyInput StartDay \"{0}\"", Input.inputString.ToUpper()));
                                        }
                                        else if (Input.GetButtonDown("Plus") == true)
                                        {
                                            //Keyboard shortcut to increase speed of ticker tape
                                            EventManager.i.PostNotification(EventType.MainInfoTickerFaster, this, null, string.Format("InputManager.cs -> ProcessKeyInput Plus \"{0}\"", Input.inputString.ToUpper()));
                                        }
                                        else if (Input.GetButtonDown("Minus") == true)
                                        {
                                            //Keyboard shortcut to decrease speed of ticker tape
                                            EventManager.i.PostNotification(EventType.MainInfoTickerSlower, this, null, string.Format("InputManager.cs -> ProcessKeyInput Minus \"{0}\"", Input.inputString.ToUpper()));
                                        }
                                        break;
                                }
                            }
                            break;
                        case ModalSubState.MetaGame:
                            //MetaGame displays are all at ModalLevel 1. Ignore commands if level > 1, eg. outcome window open on top of an info display.
                            if (GameManager.i.modalGUIScript.CheckModalLevel() == 1)
                            {
                                if (Input.GetButtonDown("Cancel") == true)
                                {
                                    EventManager.i.PostNotification(EventType.MetaGameClose, this, null, "InputManager.cs -> ProcessKeyInput Cancel");
                                }
                                else if (Input.GetButtonDown("Vertical"))
                                {
                                    y_axis = Input.GetAxisRaw("Vertical");
                                    if (y_axis > 0)
                                    { EventManager.i.PostNotification(EventType.MetaGameUpArrow, this, null, "InputManager.cs -> ProcessKeyInput Vertical"); }
                                    else if (y_axis < 0)
                                    { EventManager.i.PostNotification(EventType.MetaGameDownArrow, this, null, "InputManager.cs -> ProcessKeyInput Vertical"); }
                                }
                                else if (Input.GetButtonDown("Horizontal"))
                                {
                                    x_axis = Input.GetAxisRaw("Horizontal");
                                    if (x_axis > 0)
                                    { EventManager.i.PostNotification(EventType.MetaGameRightArrow, this, null, "InputManager.cs -> ProcessKeyInput Horizontal"); }
                                    else if (x_axis < 0)
                                    { EventManager.i.PostNotification(EventType.MetaGameLeftArrow, this, null, "InputManager.cs -> ProcessKeyInput Horizontal"); }
                                }
                                else if (Input.GetButtonDown("PageUp") == true)
                                {
                                    //Keyboard shortcut for forward a day -> PgUp
                                    EventManager.i.PostNotification(EventType.MetaGamePageUp, this, null, string.Format("InputManager.cs -> ProcessKeyInput PageUp \"{0}\"", Input.inputString.ToUpper()));
                                }
                                else if (Input.GetButtonDown("PageDown") == true)
                                {
                                    //Keyboard shortcut for back a day -> PgDown
                                    EventManager.i.PostNotification(EventType.MetaGamePageDown, this, null, string.Format("InputManager.cs -> ProcessKeyInput PageDown \"{0}\"", Input.inputString.ToUpper()));
                                }
                                else if (Input.GetButtonDown("Multipurpose") == true)
                                {
                                    //Space bar is keyboard shortcut for 'Select/Deselect' buttons (parameter needs to be '-1' for MetaGameUI.cs -> ExecuteButton to deal with correctly
                                    EventManager.i.PostNotification(EventType.MetaGameButton, this, -1, "InputManager.cs -> ProcessKeyInput Multipurpose");
                                    Input.ResetInputAxes();
                                }
                            }
                            break;
                        case ModalSubState.ShowMe:
                            //'Show Me' -> retore infoApp
                            if (Input.anyKeyDown == true)
                            { EventManager.i.PostNotification(EventType.ShowMeRestore, this, null, "InputManager.cs -> ProcessKeyInput ShowMe"); }
                            break;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Handles mouse wheel input exclusively. Change is +ve if UP (+0.1), -ve if DOWN (-0.1)
    /// </summary>
    /// <param name="change"></param>
    public void ProcessMouseWheelInput(float change)
    {
        /*Debug.LogFormat("[Inp] InputManager.cs -> ProcessMouseWheelInput: change {0}{1}{2}", change > 0 ? "+" : "", change, "\n");*/

        switch (_modalState)
        {
            case ModalState.Normal:
                break;
            case ModalState.ModalUI:
                //Hotkeys for Modal UI windows
                switch (_modalSubState)
                {
                    case ModalSubState.Outcome:
                        break;
                    case ModalSubState.Inventory:
                        break;
                    case ModalSubState.TeamPicker:
                        break;
                    case ModalSubState.GenericPicker:
                        switch (_modalGenericPickerState)
                        {
                            case ModalGenericPickerSubState.Normal:
                                break;
                            case ModalGenericPickerSubState.CompromisedGear:
                                break;
                        }
                        break;
                    case ModalSubState.Review:
                        switch (_modalReviewState)
                        {
                            case ModalReviewSubState.Open:
                                break;
                            case ModalReviewSubState.Review:
                                break;
                            case ModalReviewSubState.Close:
                                break;
                        }
                        break;
                    case ModalSubState.ActionMenu:
                        break;
                    case ModalSubState.Topic:
                        break;
                    case ModalSubState.InfoDisplay:
                        //info displays are all at ModalLevel 1. Ignore commands if level > 1, eg. outcome window open on top of an info display.
                        if (GameManager.i.modalGUIScript.CheckModalLevel() == 1)
                        {
                            //what type of info display?
                            switch (_modalInfoState)
                            {
                                case ModalInfoSubState.CityInfo:
                                    break;
                                case ModalInfoSubState.AIInfo:
                                    break;
                                case ModalInfoSubState.MainInfo:
                                    if (change > 0)
                                    { EventManager.i.PostNotification(EventType.MainInfoUpArrow, this, null, "InputManager.cs -> ProcessMouseWheelInput"); }
                                    else if (change < 0)
                                    { EventManager.i.PostNotification(EventType.MainInfoDownArrow, this, null, "InputManager.cs -> ProcessMouseWheelInput"); }
                                    break;
                            }
                        }
                        break;
                    case ModalSubState.MetaGame:
                        //info displays are all at ModalLevel 1. Ignore commands if level > 1, eg. outcome window open on top of an info display.
                        if (GameManager.i.modalGUIScript.CheckModalLevel() == 1)
                        {
                            switch (_modalMetaState)
                            {
                                case ModalMetaSubState.PlayerOptions:
                                    if (change > 0)
                                    { EventManager.i.PostNotification(EventType.MetaGameUpArrow, this, null, "InputManager.cs -> ProcessMouseWheelInput"); }
                                    else if (change < 0)
                                    { EventManager.i.PostNotification(EventType.MetaGameDownArrow, this, null, "InputManager.cs -> ProcessMouseWheelInput"); }
                                    break;
                            }
                        }
                            break;
                    case ModalSubState.ShowMe:
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
        int modalLevel = GameManager.i.modalGUIScript.CheckModalLevel();
        builder.Append(" Game States");
        builder.AppendLine(); builder.AppendLine();
        builder.AppendFormat(" GameState -> {0}{1}", GameState, "\n");
        builder.AppendFormat(" ModalState -> {0}{1}", ModalState, "\n");
        builder.AppendFormat(" ModalSubState -> {0}{1}", ModalSubState, "\n");
        builder.AppendFormat(" ModalLevel -> {0}{1}", modalLevel, "\n");
        builder.AppendFormat(" ModalInfo -> {0}{1}", ModalInfoState, "\n");
        builder.AppendFormat(" isBlocked -> {0}{1}", GameManager.i.guiScript.CheckIsBlocked(modalLevel), "\n");
        builder.AppendFormat(" NodeShowFlag -> {0}{1}", GameManager.i.nodeScript.NodeShowFlag, "\n");
        builder.AppendFormat(" isHaltExecution -> {0}{1}", GameManager.i.turnScript.haltExecution, "\n");
        builder.AppendFormat(" {0} PlayerSide -> {1}{2}{3}", "\n", GameManager.i.sideScript.PlayerSide.name, "\n", "\n");
        builder.AppendFormat(" currentSide -> {0}{1}", GameManager.i.turnScript.currentSide.name, "\n");
        builder.AppendFormat(" AuthorityCurrent -> {0}{1}", GameManager.i.sideScript.authorityCurrent, "\n");
        builder.AppendFormat(" ResistanceCurrent -> {0}{1}{2}", GameManager.i.sideScript.resistanceCurrent, "\n", "\n");
        builder.AppendFormat(" AuthorityOverall -> {0}{1}", GameManager.i.sideScript.authorityOverall, "\n");
        builder.AppendFormat(" ResistanceOverall -> {0}{1}{2}", GameManager.i.sideScript.resistanceOverall, "\n", "\n");
        builder.AppendFormat(" AuthoritySecurityState -> {0}{1}", GameManager.i.turnScript.authoritySecurityState, "\n");
        /*builder.AppendFormat(" ResistanceState -> {0}{1}{2}", GameManager.instance.turnScript.resistanceState, "\n", "\n");*/
        builder.AppendFormat(" immediateAuthorityFlag -> {0}{1}", GameManager.i.aiScript.immediateFlagAuthority, "\n");
        builder.AppendFormat(" immediateResistanceFlag -> {0}{1}{2}", GameManager.i.aiScript.immediateFlagResistance, "\n", "\n");
        builder.AppendFormat(" Authority Player -> {0}{1}", GameManager.i.playerScript.GetPlayerNameAuthority(), "\n");
        builder.AppendFormat(" Resistance Player -> {0}{1}{2}", GameManager.i.playerScript.GetPlayerNameResistance(), "\n", "\n");
        builder.AppendFormat(" HQ Approval Authority -> {0}{1}", GameManager.i.hqScript.ApprovalAuthority, "\n");
        builder.AppendFormat(" HQ Approval Resistance -> {0}{1}", GameManager.i.hqScript.ApprovalResistance, "\n");
        //backgrounds
        builder.AppendLine();
        builder.AppendLine();
        builder.Append(GameManager.i.modalGUIScript.DebugDisplayBackgrounds());
        return builder.ToString();
    }
    #endregion

    //new methods above here
}
