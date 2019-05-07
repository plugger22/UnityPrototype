using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all campaign related data
/// NOTE: retain all dynamic data in CampaignManager.cs and leave static data here in the SO
/// </summary>
[CreateAssetMenu(menuName = "Game / Campaign")]
public class Campaign : ScriptableObject
{

    [Tooltip("Name of campaign used in-game")]
    public string tag;

    [Tooltip("A list of all scenarios that make up the campaign. NOTE: Scenarios are played in order from top (index 0) to bottom")]
    public List<Scenario> listOfScenarios = new List<Scenario>();


    [HideInInspector] public int campaignID;
    
    

    //new methods above here
}
