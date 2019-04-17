using System.Collections;
using System.Collections.Generic;
using gameAPI;
using UnityEngine;

/// <summary>
/// handles all Campaign related matters
/// </summary>
public class CampaignManager : MonoBehaviour
{

    [Tooltip("Current Campaign (this is the default campaign at game start)")]
    public Campaign campaign;

    public void Initialise()
    {
        Debug.Assert(campaign != null, "Invalid campaign (Null)");
        //Assign a scenario
        Scenario scenario = campaign.GetCurrentScenario();
        if (scenario != null)
        {
            GameManager.instance.scenarioScript.scenario = scenario;
            Debug.LogFormat("[Cam] CampaignManager.cs -> Initialise: Current scenario \"{0}\"{1}", scenario.tag, "\n");
        }
        else { Debug.LogError("Invalid scenario (Null)"); }
        //event Listeners
        EventManager.instance.AddListener(EventType.CreateNewGame, OnEvent, "CampaignManager");
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
        switch (eventType)
        {
            case EventType.CreateNewGame:
                ProcessNewGame();
                break;

            default:
                Debug.LogErrorFormat("Invalid eventType {0}{1}", eventType, "\n");
                break;
        }
    }


    private void ProcessNewGame()
    {
        //open NewGame background
        GameManager.instance.modalGUIScript.SetBackground(Background.NewGame);
        //close MainMenu
        EventManager.instance.PostNotification(EventType.CloseMainMenu, this, null, "CampaignManager.cs -> ProcessNewGame");

    }

    //new methods above here
}
