using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using gameAPI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// handles all turn to turn related matters
/// Note events don't handle this as sequence is important and sequence can't be guaranteed with events
/// </summary>
public class TurnManager : MonoBehaviour
{
    [Range(1, 4)] public int actionsResistance = 2;
    [Range(1, 4)] public int actionsAuthority = 2;

    public float showSplashTimeout = 2.0f;

    
    [HideInInspector] public ResistanceState resistanceState;
    [HideInInspector] public AuthorityState authorityState;
    [HideInInspector] public GlobalSide currentSide;         //which side is it who is currently taking their turn (Resistance or Authority regardless of Player / AI)

    [SerializeField, HideInInspector]
    private int _turn;
    private int _actionsCurrent;                                                //cumulative actions taken by the player for the current turn
    private int _actionsLimit;                                                  //set to the actions cap for the player's current side

    private bool allowQuitting = false;

    public int Turn
    {
        get { return _turn; }
        set
        {
            _turn = value;
            Debug.Log(string.Format("TurnManager: Turn {0}{1}", _turn, "\n"));
        }
    }

    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        UpdateActionsLimit(GameManager.instance.sideScript.PlayerSide);
        //states
        
        resistanceState = ResistanceState.Normal;
        authorityState = AuthorityState.Normal;
        //current side
        currentSide = GameManager.instance.sideScript.PlayerSide;
        //event Listeners
        EventManager.instance.AddListener(EventType.NewTurn, OnEvent);
        EventManager.instance.AddListener(EventType.UseAction, OnEvent);
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent);
        EventManager.instance.AddListener(EventType.ExitGame, OnEvent);
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
        switch(eventType)
        {
            case EventType.NewTurn:
                ProcessNewTurn();
                break;
            case EventType.ExitGame:
                Quit();
                break;
            case EventType.UseAction:
                UseAction();
                break;
            case EventType.ChangeSide:
                ChangeSide((GlobalSide)Param);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// master method that handles sequence for ending a turn and commencing a new one
    /// </summary>
    private void ProcessNewTurn()
    {
        bool finishedProcessing = false;
        int safetyCircuit = 0;
        //continue processing turns until game over if AI vs. AI or single turn process in Player is involved
        do
        {
            //end the current turn
            EndTurnAI();
            EndTurnFinal();
            //start the new turn
            StartTurnEarly();
            StartTurnLate();
            if (StartTurnFinal() == false)
            {
                //safety switch to prevent endless loop -> debug only
                safetyCircuit++;
                if (safetyCircuit > 10) { finishedProcessing = true; Quit(); }
            }
            else
            { finishedProcessing = true; }
        }
        while (finishedProcessing == false);
    }

    /// <summary>
    /// all pre-start matters are handled here
    /// </summary>
    private void StartTurnEarly()
    {
        //increment turn counter
        _turn++;
        Debug.Log(string.Format("TurnManager: - - - StartTurnEarly - - - turn {0}{1}", GameManager.instance.turnScript.Turn, "\n"));
        EventManager.instance.PostNotification(EventType.StartTurnEarly, this);
    }

    /// <summary>
    /// all general post-start matters are handled here
    /// </summary>
    private void StartTurnLate()
    {
        Debug.Log(string.Format("TurnManager: - - - StartTurnLate - - - turn {0}{1}", GameManager.instance.turnScript.Turn, "\n"));
        EventManager.instance.PostNotification(EventType.StartTurnLate, this);
    }

    /// <summary>
    /// Special event for admin control (doesn't generate an event for other methods). Returns true if a player interaction phase to come, otherwise false (AI vs AI)
    /// </summary>
    private bool StartTurnFinal()
    {
        bool playerInteraction = true;
        Debug.Log(string.Format("TurnManager: - - - StartTurnFinal - - - turn {0}{1}", GameManager.instance.turnScript, "\n"));
        switch (GameManager.instance.sideScript.PlayerSide.name)
        {
            case "Resistance":
                currentSide = GameManager.instance.globalScript.sideResistance;
                break;
            case "Authority":
                currentSide = GameManager.instance.globalScript.sideAuthority;
                break;
            case "AI":
                //It's an AI vs. AI game so you need to go straight to EndTurnAI as there will be no player interaction
                playerInteraction = false;
                break;
            default:
                Debug.LogError(string.Format("Invalid player Side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                break;
        }
        return playerInteraction;
    }

    /// <summary>
    /// all AI end of turn matters are handled here
    /// </summary>
    /// <returns></returns>
    private void EndTurnAI()
    {
        Debug.Log(string.Format("TurnManager: - - - EndTurnAI - - - turn {0}{1}", GameManager.instance.turnScript.Turn, "\n"));
        switch (GameManager.instance.sideScript.PlayerSide.name)
        {
            case "Resistance":
                //process Authority AI
                currentSide = GameManager.instance.globalScript.sideAuthority;
                GameManager.instance.aiScript.ProcessAISideAuthority();
                break;
            case "Authority":
                //process Resistance AI
                currentSide = GameManager.instance.globalScript.sideResistance;
                GameManager.instance.aiScript.ProcessAISideResistance();
                break;
            case "AI":
                //Process both sides AI, resistance first
                currentSide = GameManager.instance.globalScript.sideResistance;
                GameManager.instance.aiScript.ProcessAISideResistance();
                currentSide = GameManager.instance.globalScript.sideAuthority;
                GameManager.instance.aiScript.ProcessAISideAuthority();
                break;
            default:
                Debug.LogError(string.Format("Invalid player Side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                break;
        }
    }

    /// <summary>
    /// all general end of turn matters are handled here
    /// </summary>
    /// <returns></returns>
    private void EndTurnFinal()
    {
        _actionsCurrent = 0;
        Debug.Log(string.Format("TurnManager: - - - EndTurnFinal - - - turn {0}{1}", GameManager.instance.turnScript.Turn, "\n"));
        EventManager.instance.PostNotification(EventType.EndTurnFinal, this);
    }


    public void ChangeSide(GlobalSide side)
    {
        UpdateActionsLimit(side);
    }

    /// <summary>
    /// sub-method to set up action limit based on player's current side
    /// </summary>
    /// <param name="side"></param>
    private void UpdateActionsLimit(GlobalSide side)
    {
        _actionsCurrent = 0;
        switch (side.name)
        {
            case "Authority": _actionsLimit = actionsAuthority; break;
            case "Resistance": _actionsLimit = actionsResistance; break;
            default: Debug.LogError(string.Format("Invalid side {0}", side.name)); break;
        }
    }

    /// <summary>
    /// call this method (via event) everytime an action is expended by the Player. Triggers new turn if action limit reached, error if exceeded
    /// </summary>
    private void UseAction()
    {
        _actionsCurrent++;
        Debug.Log(string.Format("TurnManager: Action used, {0} current actions of {1}{2}", _actionsCurrent, _actionsLimit, "\n"));
        //exceed action limit?
        if (_actionsCurrent > _actionsLimit)
        { Debug.LogError("_actionsLimit exceeded by _actionsCurrent"); }
        else if (_actionsCurrent == _actionsLimit)
        {
            //end of turn
            ProcessNewTurn();
        }
    }


    public int GetActionsCurrent()
    { return _actionsCurrent; }


    /// <summary>
    /// Quit 
    /// </summary>
    private void Quit()
    {
        //show thank you splash screen before quitting
        if (SceneManager.GetActiveScene().name != "End_Game")
        { StartCoroutine("DelayedQuit"); }
        if (allowQuitting == false)
        {
            Debug.Log(string.Format("TurnManager: Quit selected but not allowed as allowQuitting is false{0}", "\n"));
            Application.CancelQuit();
        }
    }
    

    /// <summary>
    /// display splash screen for a short while before quitting
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayedQuit()
    {
        SceneManager.LoadScene(1);
        yield return new WaitForSeconds(showSplashTimeout);
        allowQuitting = true;
        //editor quit or application quit
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void OnDisable()
    {
        EventManager.instance.RemoveEvent(EventType.ExitGame);
    }

    //new methods above here
}
