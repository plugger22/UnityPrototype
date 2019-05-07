﻿using System.Collections;
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

    //master flags used to progress Story elements
    [HideInInspector] public int[] arrayOfFlags = new int[10];

    private int scenarioIndex;                   //list index of current scenario, eg. '0' for first in the list at start of the campaign

    private Scenario scenario;


    public void Initialise()
    {
        Debug.Assert(campaign != null, "Invalid campaign (Null)");
        Debug.LogFormat("[Cam] CampaignManager.cs -> Initialise: There are {0} scenarios in the \"{1}\" campaign, ID {2}{3}", campaign.listOfScenarios.Count, campaign.tag, campaign.campaignID, "\n");
    }

    /// <summary>
    /// Get current scenario and pass to ScenarioManager.cs
    /// </summary>
    public void InitialiseScenario()
    {
        //Assign a scenario
        Scenario scenario = GetCurrentScenario();
        if (scenario != null)
        {
            GameManager.instance.scenarioScript.scenario = scenario;
            Debug.LogFormat("[Cam] CampaignManager.cs -> Initialise: Current scenario \"{0}\", ID {1}{2}", scenario.tag, scenario.scenarioID, "\n");
        }
        else { Debug.LogError("Invalid scenario (Null)"); }
    }


    /// <summary>
    /// Reset all relevant data for a new campaign
    /// </summary>
    public void Reset()
    {
        scenarioIndex = 0;
        for (int i = 0; i < arrayOfFlags.Length; i++)
        { arrayOfFlags[i] = 0; }
    }


    /// <summary>
    /// adds +1 to scenario index. Returns true if a valid scenario available, false if end of campaign
    /// </summary>
    /// <returns></returns>
    public bool IncrementScenarioIndex()
    {
        scenarioIndex++;
        int count = campaign.listOfScenarios.Count;
        Debug.LogFormat("[Cam] CampaignManager.cs -> IncrementScenarioIndex: scenario Index now {0} out of {1}{2}", scenarioIndex, count, "\n");
        if (scenarioIndex < count)
        { return true; }
        return false;
    }


    /// <summary>
    /// returns true if it's the first scenario in a campaign
    /// </summary>
    /// <returns></returns>
    public bool CheckIsFirstScenario()
    {
        if (scenarioIndex == 0)
        { return true; }
        return false;
    }


    /// <summary>
    /// returns current scenario, null if not found
    /// </summary>
    /// <returns></returns>
    public Scenario GetCurrentScenario()
    {
        Scenario scenario = null;
        if (scenarioIndex < campaign.listOfScenarios.Count)
        { scenario = campaign.listOfScenarios[scenarioIndex]; }
        return scenario;
    }

    //new methods above here
}
