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
    /*[Tooltip("Base % chance of an organisation being present in a city")] //redundant
    [Range(0, 100)] public int baseCityChance = 20;
    [Tooltip("Chance of org in city -> baseCityChance + perNodeChance x number of preferred nodes in city")] //redundant
    [Range(0, 10)] public int perNodeChance = 5;*/

    [Header("OrgInfo")]
    [Tooltip("How many turns a direct feed of the position of Nemesis/Erasure Teams/Npc last for")]
    [Range(1, 10)] public int timerOrgInfoMax = 8;


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
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseFollowOn();
                break;
            case GameState.LoadAtStart:
                //SubInitialiseLevelStart();
                //SubInitialiseFastAccess();
                SubInitialiseEvents();
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
            org.SetReputation(GameManager.instance.testScript.orgReputation);
            org.SetFreedom(GameManager.instance.testScript.orgFreedom);
            org.isContact = false;
            org.isSecretKnown = false;
            org.timer = 0;
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
        //get list of all Orgs involved in campaign
        List<Organisation> listOfOrgs = GameManager.instance.dataScript.GetListOfCurrentOrganisations();
        if (listOfOrgs != null)
        {
            //reset org values where needed
            foreach (Organisation org in listOfOrgs)
            { org.timer = 0; }
        }
        else { Debug.LogError("Invalid listOfCurrentOrganisations (Null)"); }
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register event listeners
        EventManager.instance.AddListener(EventType.StartTurnLate, OnEvent, "OrganisationManager.cs");
    }
    #endregion

    #endregion



    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //select event type
        switch (eventType)
        {
            case EventType.StartTurnLate:
                StartTurnLate();
                break;
            default:
                Debug.LogErrorFormat("Invalid eventType {0}{1}", eventType, "\n");
                break;
        }
    }


    private void StartTurnLate()
    {
        //poll all active orgs
        List<Organisation> listOfOrgs = GameManager.instance.dataScript.GetListOfCurrentOrganisations();
        if (listOfOrgs != null)
        {
            string text;
            foreach(Organisation org in listOfOrgs)
            {
                if (org.isContact == true)
                {
                    if (org.GetReputation() == 0)
                    {
                        //rep 0, generate warning message
                        text = string.Format("Poor Reputation (Zero) with {0}", org.name);
                        GameManager.instance.messageScript.OrganisationReputation(text, org);
                    }
                    //decrement timers where appropriate
                    if (org.timer > 0)
                    { org.timer--; }
                }
            }
        }
        else { Debug.LogError("Invalid listOfOrgs (Null)"); }
    }


    /*/// <summary>
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
    }*/

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
