using gameAPI;
using packageAPI;
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

    [HideInInspector] public Tutorial tutorial;
    [HideInInspector] public TutorialSet set;
    [HideInInspector] public int index;                         //index that tracks player's progress (set #) through current tutorial


    
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
        tutorial = resistanceTutorial;
        //set scenario
        if (tutorial.scenario != null)
        {
            GameManager.i.scenarioScript.scenario = tutorial.scenario;
            if (tutorial.scenario.city != null)
            {
                //set city
                GameManager.i.cityScript.SetCity(tutorial.scenario.city);
                //get index
                index = GameManager.i.dataScript.GetTutorialIndex(tutorial.name);
                if (index > -1)
                {
                    //get set
                    if (tutorial.listOfSets.Count > index)
                    {
                        set = tutorial.listOfSets[index];
                        if (set != null)
                        {
                            // Features toggle on/off -> To Do
                        }
                        else { Debug.LogErrorFormat("Invalid tutorialSet (Null) for index {0}", index); }
                    }
                    else { Debug.LogErrorFormat("Invalid tutorialIndex (index {0}, there are {1} sets in tutorial.listOfSets)", index, tutorial.listOfSets.Count); }
                }
                else { Debug.LogError("Invalid tutorial index (-1)"); }
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
