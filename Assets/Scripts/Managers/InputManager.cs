using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using System.Text;

/// <summary>
/// handles all input & game states
/// </summary>
public class InputManager : MonoBehaviour
{

    private GameState _gameState;                   //main game state
    private ModalState _modalState;                 //sub state for when game state is 'ModalUI'
    private ModalInfo _modalInfoState;              //sub sub state of ModalState.InfoDisplay -> what type of info?

    public void Initialise()
    {
        GameState = GameState.Normal;
    }

    //needs to be updated whenever changed
    public GameState GameState
    {
        get { return _gameState; }
        set
        {
            _gameState = value;
            Debug.Log(string.Format("[Inp] InputManager: GameState now {0}{1}", _gameState, "\n"));
        }
    }

    public ModalState ModalState
    {
        get { return _modalState; }
        set
        {
            _modalState = value;
            Debug.Log(string.Format("[Inp] InputManager: ModalState now {0}{1}", _modalState, "\n"));
        }
    }

    public ModalInfo ModalInfoState
    {
        get { return _modalInfoState; }
        set
        {
            _modalInfoState = value;
            Debug.Log(string.Format("[Inp] InputManager.cs: ModalInfo now {0}{1}", _modalInfoState, "\n"));
        }
    }

    /// <summary>
    /// Quick way of setting Modal state and Game State (to 'ModalUI'). If Modal state is 'InfoDisplay' provide a setting for ModalInfoState (type of info). Ignore otherwise
    /// </summary>
    /// <param name="modalState"></param>
    public void SetModalState(ModalState modalState, ModalInfo modalInfoState = ModalInfo.None)
    {
        if (modalState != ModalState.None)
        { GameState = GameState.ModalUI; }
        ModalState = modalState;
        if (ModalState == ModalState.InfoDisplay)
        { ModalInfoState = modalInfoState; }
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
                ModalInfoState = ModalInfo.None;
            }

        }
    }

    /// <summary>
    /// takes care of all input
    /// </summary>
    public void ProcessInput()
    {
        //Global input
        if (Input.GetButton("ExitGame") == true)
        {
            EventManager.instance.PostNotification(EventType.ExitGame, this);
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
                    EventManager.instance.PostNotification(EventType.NewTurn, this);
                    return;
                }
                if (Input.GetButtonDown("ShowReserves") == true)
                {
                    EventManager.instance.PostNotification(EventType.InventorySetReserve, this, "Reserve Pool");
                    return;
                }
                if (Input.GetButtonDown("ShowGear") == true)
                {
                    EventManager.instance.PostNotification(EventType.InventorySetGear, this, "Gear Inventory");
                    return;
                }
                if (Input.GetButtonDown("ShowTargets") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.ShowTargets);
                    return;
                }
                if (Input.GetButtonDown("ShowSpiders") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.ShowSpiders);
                    return;
                }
                if (Input.GetButtonDown("ShowTracers") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.ShowTracers);
                    return;
                }
                if (Input.GetButtonDown("ShowTeams") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.ShowTeams);
                    return;
                }
                //Generic NodeArc's 0 to 9 correspond to function keys F1 -> F10 and map directly to ArcTypeID's 0 to 9
                if (Input.GetButtonDown("NodeArc0") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc0);
                    return;
                }
                if (Input.GetButtonDown("NodeArc1") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc1);
                    return;
                }
                if (Input.GetButtonDown("NodeArc2") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc2);
                    return;
                }
                if (Input.GetButtonDown("NodeArc3") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc3);
                    return;
                }
                if (Input.GetButtonDown("NodeArc4") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc4);
                    return;
                }
                if (Input.GetButtonDown("NodeArc5") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc5);
                    return;
                }
                if (Input.GetButtonDown("NodeArc6") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc6);
                    return;
                }
                if (Input.GetButtonDown("NodeArc7") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc7);
                    return;
                }
                if (Input.GetButtonDown("ActivityTime") == true)
                {
                    EventManager.instance.PostNotification(EventType.ActivityDisplay, this, ActivityUI.Time);
                    return;
                }
                if (Input.GetButtonDown("ActivityCount") == true)
                {
                    EventManager.instance.PostNotification(EventType.ActivityDisplay, this, ActivityUI.Count);
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
                            EventManager.instance.PostNotification(EventType.CloseOutcomeWindow, this);
                            return;
                        }
                        if (Input.GetButtonDown("Multipurpose") == true)
                        {
                            EventManager.instance.PostNotification(EventType.CloseOutcomeWindow, this);
                            return;
                        }
                        break;
                    case ModalState.Inventory:
                        if (Input.GetButtonDown("Cancel") == true)
                        {
                            EventManager.instance.PostNotification(EventType.InventoryCloseUI, this);
                            return;
                        }
                        if (Input.GetButtonDown("Multipurpose") == true)
                        {
                            EventManager.instance.PostNotification(EventType.InventoryCloseUI, this);
                            return;
                        }
                        break;
                    case ModalState.TeamPicker:
                        if (Input.GetButtonDown("Cancel") == true)
                        {
                            EventManager.instance.PostNotification(EventType.CloseTeamPicker, this);
                            return;
                        }
                        if (Input.GetButtonDown("Multipurpose") == true)
                        {
                            EventManager.instance.PostNotification(EventType.CloseTeamPicker, this);
                            return;
                        }
                        break;
                    case ModalState.GenericPicker:
                        if (Input.GetButtonDown("Cancel") == true)
                        {
                            EventManager.instance.PostNotification(EventType.CloseGenericPicker, this);
                            return;
                        }
                        if (Input.GetButtonDown("Multipurpose") == true)
                        {
                            EventManager.instance.PostNotification(EventType.CloseGenericPicker, this);
                            return;
                        }
                        break;
                    case ModalState.ActionMenu:
                        if (Input.GetButtonDown("Cancel") == true)
                        {
                            EventManager.instance.PostNotification(EventType.CloseActionMenu, this);
                            return;
                        }
                        break;
                    case ModalState.InfoDisplay:
                        //what type of info display?
                        switch (_modalInfoState)
                        {
                            case ModalInfo.CityInfo:
                                EventManager.instance.PostNotification(EventType.CityInfoClose, this);
                                break;
                            case ModalInfo.AIInfo:
                                EventManager.instance.PostNotification(EventType.AIDisplayClose, this);
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
