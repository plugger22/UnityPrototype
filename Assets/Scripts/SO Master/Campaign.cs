using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all campaign related data
/// </summary>
[CreateAssetMenu(menuName = "Game / Campaign")]
public class Campaign : ScriptableObject
{

    [Tooltip("A list of all scenarios that make up the campaign. NOTE: Scenarios are played in order from top (index 0) to bottom")]
    public List<Scenario> listOfScenarios = new List<Scenario>();


    [HideInInspector] public int campaignID;
    [HideInInspector] public int currentScenario;                   //list index of current scenario, eg. '0' for first in the list at start of the campaign
    //master variables used to progress Story elements
    [HideInInspector] public int master0;
    [HideInInspector] public int master1;
    [HideInInspector] public int master2;
    [HideInInspector] public int master3;
    [HideInInspector] public int master4;
    [HideInInspector] public int master5;
    [HideInInspector] public int master6;
    [HideInInspector] public int master7;
    [HideInInspector] public int master8;
    [HideInInspector] public int master9;

    /// <summary>
    /// Reset all relevant data for a new campaign
    /// </summary>
    public void Reset()
    {
        currentScenario = 0;
        master0 = 0;
        master1 = 0;
        master2 = 0;
        master3 = 0;
        master4 = 0;
        master5 = 0;
        master6 = 0;
        master7 = 0;
        master8 = 0;
        master9 = 0;
    }
}
