using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// defines and initialises a 'level'
/// </summary>
public class ScenarioManager : MonoBehaviour
{
    //NOTE: Manually assigned at present but should be autoassigned from CampaignManager.cs and be '[HideInInspector]'
    public Scenario scenario;

    /// <summary>
    /// run BEFORE LevelManager.cs
    /// </summary>
    public void InitialiseEarly()
    {
        Debug.Assert(scenario != null, "Invalid scenario (Null)");
        //
        // - - - City (Early) - - -
        //
        if (scenario.city != null)
        {
            GameManager.instance.cityScript.SetCity(scenario.city);
            //NOTE: currently chooses a random city (overrides scenario.city). Need to sort out. DEBUG
            GameManager.instance.cityScript.InitialiseEarly();
        }
        else { Debug.LogError("Invalid City (Null) for scenario"); }
    }

    /// <summary>
    /// run AFTER LevelManager.cs
    /// </summary>
    public void InitialiseLate()
    {
        //
        // - - - City (Late) - - -
        //
        GameManager.instance.cityScript.InitialiseLate();
        if (scenario.challenge != null)
        {
            //
            // - - - Mission - - - 
            //
            if (scenario.mission != null)
            {
                GameManager.instance.missionScript.mission = scenario.mission;
                GameManager.instance.missionScript.Initialise();
            }
            else { Debug.LogError("Invalid mission (Null) for scenario"); }
            //
            // - - - Nemesis -> may or may not be present - - - 
            //
            if (scenario.challenge.nemesisFirst != null)
            {
                GameManager.instance.nemesisScript.nemesis = scenario.challenge.nemesisFirst;
                GameManager.instance.nemesisScript.Initialise();
            }
            else { Debug.LogFormat("[Nem] ScenarioManager.cs -> InitialiseLate: No Nemesis present in Scenario{0}", "\n"); }
        }
        else { Debug.LogError("Invalid scenario Challenge (Null)"); }
    }
}
