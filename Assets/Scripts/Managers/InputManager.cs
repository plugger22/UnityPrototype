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
    public ModalState mainState;
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

    private GameState _gameState;                   //main game state
    private ModalState _modalState;                 //sub state for when game state is 'ModalUI'
    private ModalInfoSubState _modalInfoState;              //sub sub state of ModalState.InfoDisplay -> what type of info?
    private ModalGenericPickerSubState _modalGenericPickerState; // sub state of ModalState.GenericPicker -> what type of picker?

    public void Initialise()
    {
        GameState = GameState.Normal;
    }

    //needs to be updated whenever changed
    public GameState GameState
    {
        get { return _gameState; }
        private set
        {
            _gameState = value;
            Debug.Log(string.Format("[Inp] InputManager: GameState now {0}{1}", _gameState, "\n"));
        }
    }

    public ModalState ModalState
    {
        get { return _modalState; }
        private set
        {
            _modalState = value;
            Debug.Log(string.Format("[Inp] InputManager: ModalState now {0}{1}", _modalState, "\n"));
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

    /// <summary>
    /// Quick way of setting Modal state and Game State (to 'ModalUI'). If Modal state is 'InfoDisplay' provide a setting for ModalInfoState (type of info). Ignore otherwise
    /// </summary>
    /// <param name="modalState"></param>
    public void SetModalState(ModalStateData data)
    {
        if (data != null)
        {
            if (data.mainState != ModalState.None)
            { GameState = GameState.ModalUI; }
            ModalState = data.mainState;
            if (ModalState == ModalState.InfoDisplay)
            { ModalInfoState = data.infoState; }
            if (ModalState == ModalState.GenericPicker)
            { ModalGenericPickerState = data.pickerState; }
        }
        else { Debug.LogWarning("Invalid ModalStateData (Null)"); }
    }


    /// <summary>
    /// Quick way of reseting game and modal states back to defaults. If you input a modalState it will reset ModalState to this value, rather than the default 'None'
    /// Will only reset gamestate back to Normal if modalLevel is '0' (internal check)
    /// </summary>
    /// <param name="modal"></param>
    public void ResetStates(ModalState modal = ModalState.None)
    {
        ModalState = modal;
        //reset gamestate only if modalLevel is 0
        if (GameManager.instance.modalGUIScropt.CheckModalLevel() == 0)
        {
            //only reset back to normal if there is no longer a modal state
            if (modal == ModalState.None)
            {
                GameState = GameState.Normal;
                ModalInfoState = ModalInfoSubState.None;
                ModalGenericPickerState = ModalGenericPickerSubState.None;
            }

        }
    }

    /// <summary>
    /// takes care of all input
    /// </summary>
    public void ProcessInput()
    {
        float x_axis, y_axis;
        //Global input
        if (Input.GetButton("ExitGame") == true)
        {
            EventManager.instance.PostNotification(EventType.ExitGame, this, null, "InputManager.cs -> ProcessInput");
            //GameManager.instance.Quit();
            return;
        }

        //Game State dependant input
        switch (_gameState)
        {
            case GameState.Normal:
                if (Input.GetButtonDown("NewTurn") == true)
                {
                    //Force a new turn (perhaps didn't want to take any actions), otherwise TurnManager.cs handles this once action quota used up
                    EventManager.instance.PostNotification(EventType.NewTurn, this, null, "InputManager.cs -> ProcessInput");
                    return;
                }
                if (Input.GetButtonDown("ShowReserves") == true)
                {
                    EventManager.instance.PostNotification(EventType.InventorySetReserve, this, "Reserve Pool", "InputManager.cs -> ProcessInput");
                    return;
                }
                if (Input.GetButtonDown("ShowGear") == true)
                {
                    EventManager.instance.PostNotification(EventType.InventorySetGear, this, "Gear Inventory", "InputManager.cs -> ProcessInput");
                    return;
                }
                if (Input.GetButtonDown("ShowTargets") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.ShowTargets, "InputManager.cs -> ProcessInput");
                    return;
                }
                if (Input.GetButtonDown("ShowSpiders") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.ShowSpiders, "InputManager.cs -> ProcessInput");
                    return;
                }
                if (Input.GetButtonDown("ShowTracers") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.ShowTracers, "InputManager.cs -> ProcessInput");
                    return;
                }
                if (Input.GetButtonDown("ShowTeams") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.ShowTeams, "InputManager.cs -> ProcessInput");
                    return;
                }
                //Generic NodeArc's 0 to 9 correspond to function keys F1 -> F10 and map directly to ArcTypeID's 0 to 9
                if (Input.GetButtonDown("NodeArc0") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc0, "InputManager.cs -> ProcessInput");
                    return;
                }
                if (Input.GetButtonDown("NodeArc1") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc1, "InputManager.cs -> ProcessInput");
                    return;
                }
                if (Input.GetButtonDown("NodeArc2") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc2, "InputManager.cs -> ProcessInput");
                    return;
                }
                if (Input.GetButtonDown("NodeArc3") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc3, "InputManager.cs -> ProcessInput");
                    return;
                }
                if (Input.GetButtonDown("NodeArc4") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc4, "InputManager.cs -> ProcessInput");
                    return;
                }
                if (Input.GetButtonDown("NodeArc5") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc5, "InputManager.cs -> ProcessInput");
                    return;
                }
                if (Input.GetButtonDown("NodeArc6") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc6, "InputManager.cs -> ProcessInput");
                    return;
                }
                if (Input.GetButtonDown("NodeArc7") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc7, "InputManager.cs -> ProcessInput");
                    return;
                }
                if (Input.GetButtonDown("ActivityTime") == true)
                {
                    EventManager.instance.PostNotification(EventType.ActivityDisplay, this, ActivityUI.Time, "InputManager.cs -> ProcessInput");
                    return;
                }
                if (Input.GetButtonDown("ActivityCount") == true)
                {
                    EventManager.instance.PostNotification(EventType.ActivityDisplay, this, ActivityUI.Count, "InputManager.cs -> ProcessInput");
                    return;
                }
                break;

            case GameState.ModalUI:
                //Hotkeys for Modal UI windows
                switch (_modalState)
                {
                    case ModalState.Outcome:
                        if (Input.GetButtonDown("Cancel") == true)
                        {
                            EventManager.instance.PostNotification(EventType.CloseOutcomeWindow, this, null, "InputManager.cs -> ProcessInput");
                            return;
                        }
                        if (Input.GetButtonDown("Multipurpose") == true)
                        {
                            EventManager.instance.PostNotification(EventType.CloseOutcomeWindow, this, null, "InputManager.cs -> ProcessInput");
                            return;
                        }
                        break;
                    case ModalState.Inventory:
                        if (Input.GetButtonDown("Cancel") == true)
                        {
                            EventManager.instance.PostNotification(EventType.InventoryCloseUI, this, null, "InputManager.cs -> ProcessInput");
                            return;
                        }
                        if (Input.GetButtonDown("Multipurpose") == true)
                        {
                            EventManager.instance.PostNotification(EventType.InventoryCloseUI, this, null, "InputManager.cs -> ProcessInput");
                            return;
                        }
                        break;
                    case ModalState.TeamPicker:
                        if (Input.GetButtonDown("Cancel") == true)
                        {
                            EventManager.instance.PostNotification(EventType.CloseTeamPicker, this, null, "InputManager.cs -> ProcessInput");
                            return;
                        }
                        if (Input.GetButtonDown("Multipurpose") == true)
                        {
                            EventManager.instance.PostNotification(EventType.CloseTeamPicker, this, null, "InputManager.cs -> ProcessInput");
                            return;
                        }
                        break;
                    case ModalState.GenericPicker:
                        switch (_modalGenericPickerState)
                        {
                            case ModalGenericPickerSubState.Normal:
                                if (Input.GetButtonDown("Cancel") == true)
                                {
                                    EventManager.instance.PostNotification(EventType.CloseGenericPicker, this, null, "InputManager.cs -> ProcessInput");
                                    return;
                                }
                                if (Input.GetButtonDown("Multipurpose") == true)
                                {
                                    EventManager.instance.PostNotification(EventType.CloseGenericPicker, this, null, "InputManager.cs -> ProcessInput");
                                    return;
                                }
                                break;
                            case ModalGenericPickerSubState.CompromisedGear:
                                if (Input.GetButtonDown("Cancel") == true)
                                {
                                    EventManager.instance.PostNotification(EventType.CloseGenericPicker, this, null, "InputManager.cs -> ProcessInput");
                                    EventManager.instance.PostNotification(EventType.GenericCompromisedGear, this, null, "InputManager.cs -> ProcessInput");
                                    return;
                                }
                                if (Input.GetButtonDown("Multipurpose") == true)
                                {
                                    EventManager.instance.PostNotification(EventType.CloseGenericPicker, this, null, "InputManager.cs -> ProcessInput");
                                    EventManager.instance.PostNotification(EventType.GenericCompromisedGear, this, null, "InputManager.cs -> ProcessInput");
                                    return;
                                }
                                break;
                        }
                        break;
                        
                    case ModalState.ActionMenu:
                        if (Input.GetButtonDown("Cancel") == true)
                        {
                            EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "InputManager.cs -> ProcessInput");
                            return;
                        }
                        break;
                    case ModalState.InfoDisplay:
                        //what type of info display?
                        switch (_modalInfoState)
                        {
                            case ModalInfoSubState.CityInfo:
                                if (Input.GetButtonDown("Cancel") == true)
                                {
                                    EventManager.instance.PostNotification(EventType.CityInfoClose, this, null, "InputManager.cs -> ProcessInput");
                                    return;
                                }
                                break;
                            case ModalInfoSubState.AIInfo:
                                if (Input.GetButtonDown("Cancel") == true)
                                {
                                    EventManager.instance.PostNotification(EventType.AIDisplayClose, this, null, "InputManager.cs -> ProcessInput");
                                    return;
                                }
                                break;
                            case ModalInfoSubState.MainInfo:
                                if (Input.GetButtonDown("Cancel") == true)
                                {
                                    EventManager.instance.PostNotification(EventType.MainInfoClose, this, null, "InputManager.cs -> ProcessInput");
                                    return;
                                }
                                if (Input.GetButtonDown("Vertical"))
                                {
                                    y_axis = Input.GetAxisRaw("Vertical");
                                    if (y_axis > 0)
                                    { EventManager.instance.PostNotification(EventType.MainInfoUpArrow, this, null, "InputManager.cs -> ProcessInput"); }
                                    else if (y_axis < 0)
                                    { EventManager.instance.PostNotification(EventType.MainInfoDownArrow, this, null, "InputManager.cs -> ProcessInput"); }
                                }
                                if (Input.GetButtonDown("Horizontal"))
                                {
                                    x_axis = Input.GetAxisRaw("Horizontal");
                                    if (x_axis > 0)
                                    { EventManager.instance.PostNotification(EventType.MainInfoRightArrow, this, null, "InputManager.cs -> ProcessInput"); }
                                    else if (x_axis < 0)
                                    { EventManager.instance.PostNotification(EventType.MainInfoLeftArrow, this, null, "InputManager.cs -> ProcessInput"); }
                                }
                                break;
                            default:
                                Debug.LogWarningFormat("Invalid _modalInfoState \"{0}\"", _modalInfoState);
                                break;
                        }
                        break;
                }
                break;
        }
    }

    /// <summary>
    /// Debug method to show game states
    /// </summary>
    /// <returns></returns>
    public string DisplayGameState()
    {
        StringBuilder builder = new StringBuilder();
        int modalLevel = GameManager.instance.modalGUIScropt.CheckModalLevel();
        builder.Append(" Game States");
        builder.AppendLine(); builder.AppendLine();
        builder.AppendFormat(" GameState -> {0}{1}", GameState, "\n");
        builder.AppendFormat(" ModalState -> {0}{1}", ModalState, "\n");
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
        return builder.ToString();
    }
    
    //new methods above here
}
