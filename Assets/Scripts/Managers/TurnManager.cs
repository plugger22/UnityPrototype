using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all turn to turn related matters
/// Note events don't handle this as sequence is important and sequence can't be guaranteed with events
/// </summary>
public class TurnManager : MonoBehaviour
{
    private int turn;

    public int Turn
    {
        get { return turn; }
        set
        {
            turn = value;
            Debug.Log(string.Format("TurnManager: Turn {0}{1}", turn, "\n"));
        }
    }




    public void Initialise()
    {
        //event Listeners
        EventManager.instance.AddListener(EventType.EndTurn, OnEvent);
        EventManager.instance.AddListener(EventType.StartTurnEarly, OnEvent);
        EventManager.instance.AddListener(EventType.StartTurnLate, OnEvent);
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
        Turn++;
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
        Debug.Log("TurnManager: - - - EndTurn - - - " + "\n");
    }
}
