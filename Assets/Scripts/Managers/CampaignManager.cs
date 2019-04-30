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
        Debug.LogFormat("[Cam] CampaignManager.cs -> Initialise: There are {0} scenarios in the \"{1}\" campaign, ID {2}{3}", campaign.listOfScenarios.Count, campaign.tag, campaign.campaignID, "\n");

    }



   



    //new methods above here
}
