using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// handles all turn to turn related matters
/// Note events don't handle this as sequence is important and sequence can't be guaranteed with events
/// </summary>
public class TurnManager : MonoBehaviour
{
    [Range(1, 4)] public int actionsResistance = 2;
    [Range(1, 4)] public int actionsAuthority = 2;

    [HideInInspector] public MetaLevel metaLevel;
    [HideInInspector] public ResistanceState resistanceState;
    [HideInInspector] public AuthorityState authorityState;
    [HideInInspector] public Side turnSide;         //which side is it who is currently taking their turn (Resistance or Authority regardless of Player / AI)

    [SerializeField, HideInInspector]
    private int _turn;
    private int _actionsCurrent;                                                //cumulative actions taken by the player for the current turn
    private int _actionsLimit;                                                  //set to the actions cap for the player's current side

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
        metaLevel = MetaLevel.City;
        resistanceState = ResistanceState.Normal;
        authorityState = AuthorityState.Normal;
        //event Listeners
        EventManager.instance.AddListener(EventType.EndTurn, OnEvent);
        EventManager.instance.AddListener(EventType.StartTurnEarly, OnEvent);
        EventManager.instance.AddListener(EventType.StartTurnLate, OnEvent);
        EventManager.instance.AddListener(EventType.UseAction, OnEvent);
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent);
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
            case EventType.EndTurn:
                EndTurn();
                break;
            case EventType.StartTurnEarly:
                StartTurnEarly();
                break;
            case EventType.StartTurnLate:
                StartTurnLate();
                break;
            case EventType.UseAction:
                UseAction();
                break;
            case EventType.ChangeSide:
                ChangeSide((Side)Param);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// all pre-start matters are handled here
    /// </summary>
    public void StartTurnEarly()
    {
        Debug.Log("TurnManager: - - - StartTurnEarly - - - " + "\n");
        //increment turn counter
        _turn++;
    }

    /// <summary>
    /// all general post-start matters are handled here
    /// </summary>
    public void StartTurnLate()
    {
        Debug.Log("TurnManager: - - - StartTurnLate - - - " + "\n");
    }

    /// <summary>
    /// all general end of turn matters are handled here
    /// </summary>
    /// <returns></returns>
    public void EndTurn()
    {
        _actionsCurrent = 0;
        Debug.Log("TurnManager: - - - EndTurn - - - " + "\n");
    }


    public void ChangeSide(Side side)
    {
        UpdateActionsLimit(side);
    }

    /// <summary>
    /// sub-method to set up action limit based on player's current side
    /// </summary>
    /// <param name="side"></param>
    private void UpdateActionsLimit(Side side)
    {
        _actionsCurrent = 0;
        switch (side)
        {
            case Side.Authority: _actionsLimit = actionsAuthority; break;
            case Side.Resistance: _actionsLimit = actionsResistance; break;
            default: Debug.LogError(string.Format("Invalid side {0}", side)); break;
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
            EventManager.instance.PostNotification(EventType.EndTurn, this);
            EventManager.instance.PostNotification(EventType.StartTurnEarly, this);
            EventManager.instance.PostNotification(EventType.StartTurnLate, this);
        }
    }


    public int GetActionsCurrent()
    { return _actionsCurrent; }
}
