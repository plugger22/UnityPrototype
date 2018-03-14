using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// handles all input
/// </summary>
public class InputManager : MonoBehaviour
{

    private GameState _gameState;                   //main game state
    private ModalState _modalState;                 //sub state for when game state is 'ModalUI'

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
            Debug.Log(string.Format("InputManager: GameState now {0}{1}", _gameState, "\n"));
        }
    }

    public ModalState ModalState
    {
        get { return _modalState; }
        set
        {
            _modalState = value;
            Debug.Log(string.Format("InputManager: ModalState now {0}{1}", _modalState, "\n"));
        }
    }

    /// <summary>
    /// Quick way of setting Modal state and Game State (to 'ModalUI')
    /// </summary>
    /// <param name="modal"></param>
    public void SetModalState(ModalState modal)
    {
        GameState = GameState.ModalUI;
        ModalState = modal;
    }

    /// <summary>
    /// Quick way of reseting game & modal states back to defaults
    /// </summary>
    public void ResetStates()
    {
        GameState = GameState.Normal;
        ModalState = ModalState.None;
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
                if (Input.GetButtonDown("NodeArc8") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc8);
                    return;
                }
                if (Input.GetButtonDown("NodeArc9") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.NodeArc9);
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
                }
                break;
        }
    }

}
