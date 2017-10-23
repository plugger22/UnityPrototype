using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// handles all input
/// </summary>
public class InputManager : MonoBehaviour
{

    private GameState _gameState;

    //needs to be updated whenever changed
    public GameState GameState
    {
        get { return _gameState; }
        set
        {
            _gameState = value;
            Debug.Log("InputManager -> GameState now " + _gameState + "\n");
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
                if (Input.GetButton("ShowTargets") == true)
                {
                    EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.ShowTargets);
                    return;
                }
                break;
            case GameState.ModalOutcome:
                if (Input.GetButton("Cancel") == true)
                {
                    EventManager.instance.PostNotification(EventType.CloseOutcomeWindow, this);
                    return;
                }
                if (Input.GetButton("Multipurpose") == true)
                {
                    EventManager.instance.PostNotification(EventType.CloseOutcomeWindow, this);
                    return;
                }
                break;
            case GameState.ModalActionMenu:
                if (Input.GetButton("Cancel") == true)
                {
                    EventManager.instance.PostNotification(EventType.CloseActionMenu, this);
                    return;
                }
                break;
        }
    }

}
