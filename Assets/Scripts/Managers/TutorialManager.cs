using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles Tutorial
/// </summary>
public class TutorialManager : MonoBehaviour
{

    /*
    #region Initialisation...
    /// <summary>
    /// Master Initialisation
    /// </summary>
    /// <param name="state"></param>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.LoadGame:
            case GameState.NewInitialisation:
            case GameState.LoadAtStart:
            case GameState.StartUp:
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                //do nothing
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }


    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {

    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register a listener
        EventManager.i.AddListener(EventType.Tutorial, OnEvent, "TutorialManager.cs");
    }
    #endregion

    #endregion
    */

    #region OnEvent
    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //detect event type
        switch (eventType)
        {
            case EventType.Tutorial:
                InitialiseTutorial();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion

    /// <summary>
    /// Set up tutorial prior to running
    /// </summary>
    public void InitialiseTutorial()
    {
        Debug.LogFormat("[Tut] TutorialManager.cs -> InitialiseTutorial: Commence Initialisation{0}", "\n");
    }


    //new methods above here
}
