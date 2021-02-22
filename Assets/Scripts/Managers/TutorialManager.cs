using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles Tutorial
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("Tutorials")]
    public Tutorial resistanceTutorial;
    public Tutorial authorityTutorial;

    [HideInInspector] int index;                                  //index for tutorial set (which one is currently in use)


    [HideInInspector] public Tutorial currentTutorial;

    
    #region Initialisation...
    /// <summary>
    /// Master Initialisation
    /// </summary>
    /// <param name="state"></param>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.TutorialOptions:
                SubInitialiseFastAccess();
                SubInitialiseTutorial();
                break;
            case GameState.LoadGame:
            case GameState.NewInitialisation:
            case GameState.LoadAtStart:
            case GameState.StartUp:
            case GameState.FollowOnInitialisation:
                //do nothing
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        Debug.Assert(resistanceTutorial != null, "Invalid resistanceTutorial (Null)");
        /*Debug.Assert(authorityTutorial != null, "Invalid authorityTutorial (Null)");*/  //EDIT -> TO DO -> switch back on for Authority side
    }
    #endregion


    #region SubInitialiseTutorial
    private void SubInitialiseTutorial()
    {
        //Debug
        currentTutorial = resistanceTutorial;
        //set scenario
        if (currentTutorial.scenario != null)
        {
            GameManager.i.scenarioScript.scenario = currentTutorial.scenario;
            if (currentTutorial.scenario.city != null)
            {
                //set city
                GameManager.i.cityScript.SetCity(currentTutorial.scenario.city);
            }
            else { Debug.LogError("Invalid tutorial city (Null)"); }
        }
        else { Debug.LogError("Invalid tutorial Scenario (Null)"); }
    }
    #endregion



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
