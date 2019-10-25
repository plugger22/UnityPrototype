using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// handles all 3rd Pary Organisation matters
/// </summary>
public class OrganisationManager : MonoBehaviour
{
    [Tooltip("Base % chance of an organisation being present in a city")]
    [Range(0, 100)] public int baseCityChance = 20;
    [Tooltip("Chance of org in city -> baseCityChance + perNodeChance x number of preferred nodes in city")]
    [Range(0, 10)] public int perNodeChance = 5;


    /// <summary>
    /// Not for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                //SubInitialiseSessionStart();
                SubInitialiseLevelStart();
                //SubInitialiseFastAccess();
                //SubInitialiseLevelAll();
                break;
            case GameState.FollowOnInitialisation:
                //SubInitialiseFollowOn();
                break;
            case GameState.LoadAtStart:
                //SubInitialiseLevelStart();
                //SubInitialiseFastAccess();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }

    #region Initialise SubMethods

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {

    }
    #endregion

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
        //get list of all Orgs involved in campaign
        List<Organisation> listOfOrgs = GameManager.instance.dataScript.GetListOfCurrentOrganisations();
        //empty list just to be sure
        listOfOrgs.Clear();
        //load all campaign organisations into list
        if (listOfOrgs != null)
        {
            //cure
            Organisation org = GameManager.instance.campaignScript.campaign.orgCure;
            if (org != null)
            { listOfOrgs.Add(org); }
            else { Debug.LogWarningFormat("Invalid campaign.orgCure (Null)"); }
            //contract
            org = GameManager.instance.campaignScript.campaign.orgContract;
            if (org != null)
            { listOfOrgs.Add(org); }
            else { Debug.LogWarningFormat("Invalid campaign.orgContract (Null)"); }
            //HQ
            org = GameManager.instance.campaignScript.campaign.orgHQ;
            if (org != null)
            { listOfOrgs.Add(org); }
            else { Debug.LogWarningFormat("Invalid campaign.orgHQ (Null)"); }
            //Emergency
            org = GameManager.instance.campaignScript.campaign.orgEmergency;
            if (org != null)
            { listOfOrgs.Add(org); }
            else { Debug.LogWarningFormat("Invalid campaign.orgEmergency (Null)"); }
            //Info
            org = GameManager.instance.campaignScript.campaign.orgInfo;
            if (org != null)
            { listOfOrgs.Add(org); }
            else { Debug.LogWarningFormat("Invalid campaign.orgInfo (Null)"); }
        }
        else { Debug.LogError("Invalid listOfCurrentOrganisations (Null)"); }
        //initialise orgs in list
        foreach(Organisation org in listOfOrgs)
        {
            org.maxStat = GameManager.instance.actorScript.maxStatValue;
            org.SetReputation(0);
            org.SetFreedom(2);
            org.isContact = false;
            Debug.LogFormat("[Org] OrganisationManager.cs -> SubInitaliseLevelStart: Org \"{0}\", reputation {1}, freedom {2}, isContact {3}{4}", 
                org.tag, org.GetReputation(), org.GetFreedom(), org.isContact, "\n");
        }
    }
    #endregion

    #region SubInitialiseLevelAll
    private void SubInitialiseLevelAll()
    {

    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {


    }
    #endregion

    #region SubInitialiseFollowOn
    private void SubInitialiseFollowOn()
    {

    }
    #endregion

    #endregion


    /// <summary>
    /// Initialises organisations in a city. 
    /// </summary>
    /// <param name="city"></param>
    public void SetOrganisationsInCity(City city)
    {
        if (city != null)
        {
            //empty out first as SO's are persistent
            city.ClearOrganisations();
            List<int> listOfDistrictTotals = city.GetListOfDistrictTotals();
            if (listOfDistrictTotals != null)
            {
                Dictionary<string, Organisation> dictOfOrganisations = GameManager.instance.dataScript.GetDictOfOrganisations();
                if (dictOfOrganisations != null)
                {
                    int index, chance;
                    //loop organisations
                    foreach(var org in dictOfOrganisations)
                    {
                        //get list index from organisation
                        index = org.Value.nodeArc.nodeArcID;
                        if (index >= 0 && index < listOfDistrictTotals.Count)
                        {
                            //chance is base chance + amount for each organisation's preferred node that's present in city
                            chance = baseCityChance + perNodeChance * listOfDistrictTotals[index];
                            if (Random.Range(0, 100) <= chance)
                            { city.AddOrganisation(org.Value); }
                        }
                    }
                }
                else { Debug.LogError("Invalid dictOfOrganisations (Null)"); }
            }
            else { Debug.LogError("Invalid city.listOfDistrictTotals (Null)"); }
        }
        else { Debug.LogError("Invalid city (Null)"); }
    }

    /// <summary>
    /// Debug method to toggle (eg. isContact = true/false) all current organisations
    /// </summary>
    public void DebugToggleAllOrganisations()
    {
        List<Organisation> listOfOrgs = GameManager.instance.dataScript.GetListOfCurrentOrganisations();
        if (listOfOrgs != null)
        {
            foreach (Organisation org in listOfOrgs)
            {
                if (org != null)
                {
                    if (org.isContact == false)
                    { org.isContact = true; }
                    else { org.isContact = false; }
                }
            }
        }
        else { Debug.LogError("Invalid listOfCurrentOrganisations (Null)"); }
    }

}
