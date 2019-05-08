﻿using System;
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
    [Tooltip("Number of story status flags used to track stuff story developments across a multi-scenario campaign (size of the array)")]
    public int numOfFlags = 10;

    //master flags used to progress Story elements
    [HideInInspector] public int[] arrayOfStoryStatus;

    private int scenarioIndex;                   //list index of current scenario, eg. '0' for first in the list at start of the campaign

    [HideInInspector] public Scenario scenario;


    public void InitialiseGame()
    {
        Debug.Assert(campaign != null, "Invalid campaign (Null)");
        //collections
        arrayOfStoryStatus = new int[numOfFlags];
        Debug.LogFormat("[Cam] CampaignManager.cs -> Initialise: There are {0} scenarios in the \"{1}\" campaign, ID {2}{3}", campaign.listOfScenarios.Count, campaign.tag, campaign.campaignID, "\n");
    }

    /// <summary>
    /// run BEFORE LevelManager.cs
    /// </summary>
    public void InitialiseEarly()
    {
        //Assign a scenario
        scenario = GetCurrentScenario();
        if (scenario != null)
        { Debug.LogFormat("[Cam] CampaignManager.cs -> Initialise: Current scenario \"{0}\", ID {1}{2}", scenario.tag, scenario.scenarioID, "\n"); }
        else { Debug.LogError("Invalid scenario (Null)"); }
        // City (Early)
        if (scenario.city != null)
        {
            GameManager.instance.cityScript.SetCity(scenario.city);
            //NOTE: currently chooses a random city (overrides scenario.city). Need to sort out. DEBUG
            GameManager.instance.cityScript.InitialiseEarly(scenario.leaderAuthority);
        }
        else { Debug.LogError("Invalid City (Null) for scenario"); }
    }

    /// <summary>
    /// run AFTER LevelManager.cs
    /// </summary>
    public void InitialiseLate()
    {
        // City (Late)
        GameManager.instance.cityScript.InitialiseLate();
        if (scenario.challengeResistance != null)
        {
            // Mission
            if (scenario.missionResistance != null)
            {
                GameManager.instance.missionScript.mission = scenario.missionResistance;
                GameManager.instance.missionScript.Initialise();
            }
            else { Debug.LogError("Invalid mission (Null) for scenario"); }
            // Nemesis -> may or may not be present
            if (scenario.challengeResistance.nemesisFirst != null)
            {
                GameManager.instance.nemesisScript.nemesis = scenario.challengeResistance.nemesisFirst;
                GameManager.instance.nemesisScript.Initialise();
            }
            else { Debug.LogFormat("[Nem] CampaignManager.cs -> InitialiseLate: No Nemesis present in Scenario{0}", "\n"); }
        }
        else { Debug.LogError("Invalid scenario Challenge (Null)"); }
    }



    /// <summary>
    /// Reset all relevant data for a new campaign
    /// </summary>
    public void Reset()
    {
        scenarioIndex = 0;
        Array.Clear(arrayOfStoryStatus, 0, arrayOfStoryStatus.Length);
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
        else { Debug.LogWarningFormat("Invalid scenarioIndex {0} (out of bounds)", scenarioIndex); }
        return scenario;
    }

    /// <summary>
    /// returns a scenario based on the supplied scenarioIndex
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Scenario GetScenario(int index)
    {
        Scenario scenario = null;
        if (scenarioIndex < campaign.listOfScenarios.Count)
        { scenario = campaign.listOfScenarios[scenarioIndex]; }
        else { Debug.LogWarningFormat("Invalid scenarioIndex {0} (out of bounds)", index); }
        return scenario;
    }

    public int GetScenarioIndex()
    { return scenarioIndex; }

    public int[] GetArrayOfStoryStatus()
    { return arrayOfStoryStatus; }

    /// <summary>
    /// Sets the current scenario and the scenario index according to the supplied parameter
    /// </summary>
    /// <param name="scenarioIndex"></param>
    public void SetScenario(int index)
    {
        scenarioIndex = index;
        scenario = GetScenario(index);
        if (scenario == null)
        { Debug.LogErrorFormat("Invalid scenario (Null) for scenarioIndex {0}", index); }
    }


    public void SetArrayOfStoryStatus(int[] storyArray)
    {
        if (storyArray != null)
        {

        }
    }

    //new methods above here
}
