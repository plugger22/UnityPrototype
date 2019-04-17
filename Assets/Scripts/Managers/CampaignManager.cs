using System.Collections;
using System.Collections.Generic;
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
    }
}
