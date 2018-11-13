using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Scenario setup (defines a level). Name of SO is name of the scenario
/// </summary>
[CreateAssetMenu(menuName = "Game / Scenario")]
public class Scenario : ScriptableObject
{
    [Tooltip("In-game descriptor")]
    [TextArea] public string descriptor;

    [Tooltip("City where the scenario will be")]
    public City city;

    [Tooltip("Target and objectives for the scenario")]
    public Mission mission;

    [Tooltip("Challenge (difficulty) of the scenario")]
    public Challenge challenge;


    public void OnEnable()
    {
        Debug.Assert(city != null, "Invalid city (Null) for Scenario");
        Debug.Assert(mission != null, "Invalid mission (Null) for Scenario");
        Debug.Assert(challenge != null, "Invalid challenge (Null) for Scenario");
    }




}
