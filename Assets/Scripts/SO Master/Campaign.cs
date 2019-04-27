﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all campaign related data
/// </summary>
[CreateAssetMenu(menuName = "Game / Campaign")]
public class Campaign : ScriptableObject
{

    [Tooltip("Name of campaign used in-game")]
    public string tag;

    [Tooltip("A list of all scenarios that make up the campaign. NOTE: Scenarios are played in order from top (index 0) to bottom")]
    public List<Scenario> listOfScenarios = new List<Scenario>();


    [HideInInspector] public int campaignID;
    
    //master flags used to progress Story elements
    [HideInInspector] public int[] arrayOfFlags = new int[10];



    private int scenarioIndex;                   //list index of current scenario, eg. '0' for first in the list at start of the campaign

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
        int count = listOfScenarios.Count;
        Debug.LogFormat("[Cam] Campaign.SO -> IncrementScenarioIndex: scenario Index now {0} out of {1}{2}", scenarioIndex, count, "\n");
        if (scenarioIndex < count)
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
        if (scenarioIndex < listOfScenarios.Count)
        { scenario = listOfScenarios[scenarioIndex]; }
        return scenario;
    }


    //new methods above here
}
