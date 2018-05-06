﻿using System.Collections;
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
    [Tooltip("Base number of actions that the Resistance side can carry out each turn")]
    [Range(1, 4)] public int actionsResistance = 2;
    [Tooltip("Base number of actions that the Authority side can carry out each turn")]
    [Range(1, 4)] public int actionsAuthority = 2;

    public float showSplashTimeout = 2.0f;

    
    [HideInInspector] public ResistanceState resistanceState;
    [HideInInspector] public AuthorityState authorityState;
    [HideInInspector] public GlobalSide currentSide;         //which side is it who is currently taking their turn (Resistance or Authority regardless of Player / AI)

    [SerializeField, HideInInspector]
    private int _turn;
    private int _actionsCurrent;                                                //cumulative actions taken by the player for the current turn
    private int _actionsLimit;                                                  //set to the actions cap for the player's current side
    private int _actionsAdjust;                                                 //any temporary adjustments that apply to the actions Limit
    private int _actionsTotal;                                                  //total number of actions available to the player this turn (adjustments + limit)

    private bool allowQuitting = false;

    /*private string colourRebel;
    private string colourAuthority;*/
    private string colourNeutral;
    private string colourNormal;
    /*private string colourGood;
    private string colourBad;
    private string colourGrey;
    private string colourSide;*/
    private string colourAlert;
    private string colourEnd;


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
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
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
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        /*colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        colourRebel = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);*/
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
        //current Player side colour
        /*if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; }
        else { colourSide = colourRebel; }*/
    }

    /// <summary>
    /// master method that handles sequence for ending a turn and commencing a new one
    /// </summary>
    private void ProcessNewTurn()
    {
        Debug.Log(string.Format("TurnManager -> ProcessNewTurn -> Player side: {0}, Current side: {1}{2}", 
            GameManager.instance.sideScript.PlayerSide.name, currentSide.name, "\n"));
        bool finishedProcessing = false;
        int safetyCircuit = 0;
        //only process a new turn if game state is normal (eg. not in the middle of a modal window operation
        if (GameManager.instance.inputScript.GameState == GameState.Normal)
        {
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
                    if (safetyCircuit > 10)
                    {
                        finishedProcessing = true;
                        Debug.LogError("TurnManagers.cs -> ProcessNewTurn -> SafetyCircuit triggered");
                        Quit(); }
                }
                else
                { finishedProcessing = true; }
            }
            while (finishedProcessing == false);
            //switch off any node Alerts
            GameManager.instance.alertScript.CloseAlertUI(true);
        }
    }


    /// <summary>
    /// all pre-start matters are handled here
    /// </summary>
    private void StartTurnEarly()
    {
        //increment turn counter
        _turn++;
        Debug.Log(string.Format("TurnManager: - - - StartTurnEarly - - - turn {0}", "\n"));
        EventManager.instance.PostNotification(EventType.StartTurnEarly, this);
        //reset nodes and connections if not in normal state
        if (GameManager.instance.nodeScript.activityState != ActivityUI.None)
        { GameManager.instance.nodeScript.ResetAll(); }
        //update turn in top widget UI
        EventManager.instance.PostNotification(EventType.ChangeTurn, this, _turn);
    }

    /// <summary>
    /// all general post-start matters are handled here
    /// </summary>
    private void StartTurnLate()
    {
        Debug.Log(string.Format("TurnManager: - - - StartTurnLate - - - turn {0}", "\n"));
        EventManager.instance.PostNotification(EventType.StartTurnLate, this);
    }

    /// <summary>
    /// Special event for admin control (doesn't generate an event for other methods). Returns true if a player interaction phase to come, otherwise false (AI vs AI)
    /// </summary>
    private bool StartTurnFinal()
    {
        bool playerInteraction = true;
        Debug.Log(string.Format("TurnManager: - - - StartTurnFinal - - - turn {0}", "\n"));
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
        Debug.Log(string.Format("TurnManager: - - - EndTurnAI - - - turn {0}", "\n"));
        switch (GameManager.instance.sideScript.PlayerSide.name)
        {
            case "Resistance":
                //process Authority AI
                currentSide = GameManager.instance.globalScript.sideAuthority;
                if (GameManager.instance.sideScript.authorityOverall == SideState.AI)
                { GameManager.instance.aiScript.ProcessAISideAuthority(); }
                if (GameManager.instance.sideScript.resistanceOverall == SideState.AI)
                { GameManager.instance.aiScript.ProcessAISideResistance(); }
                break;
            case "Authority":
                //process Resistance AI
                currentSide = GameManager.instance.globalScript.sideResistance;
                if (GameManager.instance.sideScript.authorityOverall == SideState.AI)
                { GameManager.instance.aiScript.ProcessAISideAuthority(); }
                if (GameManager.instance.sideScript.resistanceOverall == SideState.AI)
                { GameManager.instance.aiScript.ProcessAISideResistance(); }
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
        //decrement any action adjustments
        GameManager.instance.dataScript.UpdateActionAdjustments();
        //actions
        _actionsCurrent = 0;
        _actionsTotal = _actionsLimit + GameManager.instance.dataScript.GetActionAdjustment(GameManager.instance.sideScript.PlayerSide);
        _actionsTotal = Mathf.Max(0, _actionsTotal);
        Debug.Log(string.Format("TurnManager: - - - EndTurnFinal - - - turn {0}", "\n"));
        EventManager.instance.PostNotification(EventType.ChangeActionPoints, this, _actionsTotal);
        EventManager.instance.PostNotification(EventType.EndTurnFinal, this);
    }


    public void ChangeSide(GlobalSide side)
    {
        UpdateActionsLimit(side);
    }

    /// <summary>
    /// sub-method to set up action limit based on player's current side. Run at game start and whenever change sides or action effect applied.
    /// To force and update and retain current actions value, input the current action value, otherwise leave as default zero
    /// </summary>
    /// <param name="side"></param>
    public void UpdateActionsLimit(GlobalSide side, int currentActions = 0)
    {
        _actionsCurrent = currentActions;
        switch (side.name)
        {
            case "Authority": _actionsLimit = actionsAuthority; break;
            case "Resistance": _actionsLimit = actionsResistance; break;
            default: Debug.LogError(string.Format("Invalid side {0}", side.name)); break;
        }
        //calculate total actions available
        _actionsAdjust = GameManager.instance.dataScript.GetActionAdjustment(side);
        _actionsTotal = _actionsLimit + _actionsAdjust;
    }

    /// <summary>
    /// call this method (via event) everytime an action is expended by the Player. Triggers new turn if action limit reached, error if exceeded
    /// </summary>
    private void UseAction()
    {
        int remainder;
        _actionsCurrent++;
        Debug.Log(string.Format("TurnManager: Action used, {0} current actions of {1}{2}", _actionsCurrent, _actionsTotal, "\n"));
        //exceed action limit? (total includes any temporary adjustments)
        remainder = _actionsTotal - _actionsCurrent;
        /*if (_actionsCurrent > _actionsTotal)*/
        if (remainder < 0)
        { Debug.LogError("_actionsTotal exceeded by _actionsCurrent"); }
        else
        { EventManager.instance.PostNotification(EventType.ChangeActionPoints, this, remainder); }
    }

    /// <summary>
    /// returns the number of actions the player has currently used
    /// </summary>
    /// <returns></returns>
    public int GetActionsCurrent()
    { return _actionsCurrent; }

    /// <summary>
    /// returns the number of remaining actions currently available to the player for this turn
    /// </summary>
    /// <returns></returns>
    public int GetActionsAvailable()
    { return _actionsTotal - _actionsCurrent; }

    public int GetActionsTotal()
    { return _actionsTotal; }

    /// <summary>
    /// returns a  colour string in format "1 or 2 Actions available"
    /// </summary>
    /// <returns></returns>
    public string GetActionsTooltip()
    { return string.Format("{0}{1}{2}{3} of {4}<b>{5}</b>{6} Actions available{7}", colourNeutral, GetActionsAvailable(), colourEnd, colourNormal, colourEnd,
        _actionsTotal, colourNormal, colourEnd); }

    /// <summary>
    /// Returns true if the player has at least one remaining action, otherwise false
    /// </summary>
    /// <returns></returns>
    public bool CheckRemainingActions()
    {
        if (_actionsCurrent < _actionsTotal)
        { return true; }
        return false;
    }

    /// <summary>
    /// returns a colour string in format "Turn 2"
    /// </summary>
    /// <returns></returns>
    public string GetTurnTooltip()
    { return string.Format("Turn {0}{1}{2}", colourNeutral, _turn, colourEnd); }

    /// <summary>
    /// returns a colour string for the turn tooltip details info tip
    /// </summary>
    /// <returns></returns>
    public string GetTurnInfoTip()
    { return string.Format("{0}Press Enter for a New Turn{1}", colourAlert, colourEnd); }

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
