using gameAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using UnityEngine;

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
                SubInitialiseLevelStart();
                SubInitialiseSessionStart();
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
        //get list of all Orgs involved in campaign
        List<Organisation> listOfOrgs = GameManager.instance.dataScript.GetListOfCurrentOrganisations();
        if (listOfOrgs != null)
        {
            //reset isCutOff in case of a new Game
            foreach (Organisation org in listOfOrgs)
            { org.isCutOff = false; }
        }
        else { Debug.LogError("Invalid listOfCurrentOrganisations (Null)"); }
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
        foreach (Organisation org in listOfOrgs)
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
            foreach (Organisation org in listOfOrgs)
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
                    {
                        org.timer--;
                        //timer hits zero
                        ProcessOrgTimerAdmin(org);
                    }
                }
            }
        }
        else { Debug.LogError("Invalid listOfOrgs (Null)"); }
    }

    /// <summary>
    /// Org's timer has decremented. Messaging and admin
    /// </summary>
    /// <param name="org"></param>
    private void ProcessOrgTimerAdmin(Organisation org)
    {
        switch (org.orgType.name)
        {
            case "Contract":
            case "Cure":
            case "Emergency":
            case "HQ":
                //none of the above currently have timers
                break;
            case "Info":
                if (org.timer <= 0)
                {
                    //reset array to false (Not tracking anything)
                    GameManager.instance.dataScript.ResetOrgInfoArray(false);
                }
                else
                { ProcessOrgInfoEffectMessage(org); }
                break;
            default: Debug.LogWarningFormat("Unrecognised orgType \"{0}\"", org.orgType.name); break;
        }
    }

    /// <summary>
    /// Generates an OrgInfo effects tab message
    /// </summary>
    /// <param name="org"></param>
    public void ProcessOrgInfoEffectMessage(Organisation org, OrgInfoType orgType = OrgInfoType.Count)
    {
        //if default orginfoType.Count need to get correct type
        orgType = GameManager.instance.dataScript.GetOrgInfoType();
        string orgText = "";
        switch (orgType)
        {
            case OrgInfoType.Nemesis:
                orgText = string.Format("Reporting the position of your {0}", GameManager.instance.colourScript.GetFormattedString("Nemesis", ColourType.salmonText));
                break;
            case OrgInfoType.ErasureTeams:
                orgText = string.Format("Reporting the position of any {0}", GameManager.instance.colourScript.GetFormattedString("Erasure Teams", ColourType.salmonText));
                break;
            case OrgInfoType.Npc:
                if (GameManager.instance.missionScript.mission.npc != null)
                { orgText = string.Format("Reporting the position of the {0}", GameManager.instance.colourScript.GetFormattedString(GameManager.instance.missionScript.mission.npc.tag, ColourType.salmonText)); }
                break;
            default: Debug.LogWarningFormat("Unrecognised orgType \"{0}\"", orgType); break;
        }
        //effects tab message
        ActiveEffectData data = new ActiveEffectData()
        {
            text = string.Format("{0} active", org.tag.ToUpper()),
            topText = string.Format("{0} Direct Feed", org.tag),
            detailsTop = string.Format("{0} will continue to provide a direct feed for", GameManager.instance.colourScript.GetFormattedString(org.tag, ColourType.neutralText)),
            detailsBottom = string.Format("{0} days{1}", GameManager.instance.colourScript.GetFormattedString(Convert.ToString(org.timer), ColourType.neutralText),
                string.IsNullOrEmpty(orgText) == false ? string.Format("{0}{1}{2}", "\n", "\n", orgText) : ""),
            sprite = org.sprite,
            help0 = "orgInfo_0",
            help1 = "orgInfo_1",
            help2 = "orgInfo_2"
        };
        GameManager.instance.messageScript.ActiveEffect(data);
    }

    /// <summary>
    /// Cancels a current tracking service due to target being no longer available
    /// </summary>
    /// <param name="infoType"></param>
    public void CancelOrgInfoTracking(OrgInfoType orgInfoType)
    {
        //reset array to false (Not tracking anything)
        GameManager.instance.dataScript.SetOrgInfoType(orgInfoType, false);
        //reset timer
        Organisation org = GameManager.instance.campaignScript.campaign.orgInfo;
        if (org != null)
        { org.timer = 0; }
        else { Debug.LogError("Invalid orgInfo (Null)"); }
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
