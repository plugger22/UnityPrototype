﻿using gameAPI;
using System;
using System.Text;
using UnityEngine;

/// <summary>
/// handles all Campaign related matters
/// </summary>
public class CampaignManager : MonoBehaviour
{

    [Tooltip("Number of story status flags used to track stuff story developments across a multi-scenario campaign (size of the array)")]
    public int numOfFlags = 10;



    #region Save Data Compatible
    //master flags used to progress Story elements
    [HideInInspector] public int[] arrayOfStoryStatus;
    private int scenarioIndex;                   //list index of current scenario, eg. '0' for first in the list at start of the campaign

    [HideInInspector] public Campaign campaign;
    [HideInInspector] public Scenario scenario;
    private int commendations;                 //gain from doing good things. Campaign status. NOTE: use method to change -> ChangeCommendations
    private int blackMarks;                    //gain from doing bad things. Campaign status. NOTE: use method to change -> ChangeBlackMarks
    private int investigationBlackMarks = 1;   //number of black marks gained from a guilty investigation, goes up +1 for each guilty verdict
    #endregion

    public void InitialiseGame(GameState state)
    {
        Debug.Assert(campaign != null, "Invalid campaign (Null)");
        //collections
        arrayOfStoryStatus = new int[numOfFlags];
        Debug.LogFormat("[Cam] CampaignManager.cs -> Initialise: There are {0} scenarios in the \"{1}\" campaign{2}", campaign.listOfScenarios.Count, campaign.tag, "\n");
    }

    /// <summary>
    /// run BEFORE LevelManager.cs
    /// </summary>
    public void InitialiseEarly(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
            case GameState.FollowOnInitialisation:
            case GameState.LoadAtStart:
            case GameState.LoadGame:
                SubInitialiseAllEarly();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }

    /// <summary>
    /// run AFTER LevelManager.cs
    /// NOTE: Initialises CityManager (Late), MissionManager (in turn initialises TargetManager) and NemesisManager
    /// </summary>
    public void InitialiseLate(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
            case GameState.FollowOnInitialisation:
            case GameState.LoadAtStart:
            case GameState.LoadGame:
                SubInitialiseAllLate();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }

    #region Initialise SubMethods

    #region SubInitialiseAllEarly
    private void SubInitialiseAllEarly()
    {
        //Assign a scenario
        scenario = GetCurrentScenario();
        if (scenario != null)
        { Debug.LogFormat("[Cam] CampaignManager.cs -> InitialiseEarly: Current scenario \"{0}\", {1}{2}", scenario.tag, scenario.descriptor, "\n"); }
        else { Debug.LogError("Invalid scenario (Null)"); }
        // City (Early)
        if (scenario.city != null)
        {
            GameManager.instance.cityScript.SetCity(scenario.city);
            //NOTE: currently chooses a random city (overrides scenario.city). Need to sort out. DEBUG
            GameManager.instance.cityScript.InitialiseEarly(scenario.leaderAuthority);
        }
        else { Debug.LogError("Invalid City (Null) for scenario"); }
    }
    #endregion

    #region SubInitialiseAllLate
    private void SubInitialiseAllLate()
    {
        // City (Late)
        GameManager.instance.cityScript.InitialiseLate();
        if (scenario.challengeResistance != null)
        {
            // Mission
            if (scenario.missionResistance != null)
            {
                GameManager.instance.missionScript.mission = scenario.missionResistance;
                GameManager.instance.objectiveScript.mission = scenario.missionResistance;
                if (GameManager.instance.inputScript.GameState != GameState.LoadGame)
                { GameManager.instance.missionScript.Initialise(); }
            }
            else { Debug.LogError("Invalid mission (Null) for scenario"); }
            // Nemesis -> may or may not be present
            if (scenario.challengeResistance.nemesisFirst != null)
            {
                GameManager.instance.nemesisScript.nemesis = scenario.challengeResistance.nemesisFirst;
                GameManager.instance.nemesisScript.Initialise();
            }
            else { Debug.LogFormat("[Nem] CampaignManager.cs -> InitialiseLate: No Nemesis present in Scenario{0}", "\n"); }
        }
        else { Debug.LogError("Invalid scenario Challenge (Null)"); }
    }
    #endregion

    #endregion



    /// <summary>
    /// Reset all relevant data for a new campaign
    /// </summary>
    public void Reset()
    {
        scenarioIndex = 0;
        Array.Clear(arrayOfStoryStatus, 0, arrayOfStoryStatus.Length);
        commendations = 0;
        blackMarks = 0;
        investigationBlackMarks = 1;
    }


    /// <summary>
    /// adds +1 to scenario index. Returns true if a valid scenario available, false if end of campaign
    /// </summary>
    /// <returns></returns>
    public bool IncrementScenarioIndex()
    {
        scenarioIndex++;
        int count = campaign.listOfScenarios.Count;
        Debug.LogFormat("[Cam] CampaignManager.cs -> IncrementScenarioIndex: scenario Index now {0} out of {1}{2}", scenarioIndex, count, "\n");
        if (scenarioIndex < count)
        { return true; }
        return false;
    }


    /// <summary>
    /// returns true if it's the first scenario in a campaign
    /// </summary>
    /// <returns></returns>
    public bool CheckIsFirstScenario()
    {
        if (scenarioIndex == 0)
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
        if (scenarioIndex < campaign.listOfScenarios.Count)
        { scenario = campaign.listOfScenarios[scenarioIndex]; }
        else { Debug.LogWarningFormat("Invalid scenarioIndex {0} (out of bounds)", scenarioIndex); }
        return scenario;
    }

    /// <summary>
    /// returns a scenario based on the supplied scenarioIndex
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Scenario GetScenario(int index)
    {
        Scenario scenario = null;
        if (scenarioIndex < campaign.listOfScenarios.Count)
        { scenario = campaign.listOfScenarios[scenarioIndex]; }
        else { Debug.LogWarningFormat("Invalid scenarioIndex {0} (out of bounds)", index); }
        return scenario;
    }

    public int GetScenarioIndex()
    { return scenarioIndex; }

    public int[] GetArrayOfStoryStatus()
    { return arrayOfStoryStatus; }

    /// <summary>
    /// Sets the current scenario and the scenario index according to the supplied parameter
    /// </summary>
    /// <param name="scenarioIndex"></param>
    public void SetScenario(int index)
    {
        scenarioIndex = index;
        scenario = GetScenario(index);
        if (scenario == null)
        { Debug.LogErrorFormat("Invalid scenario (Null) for scenarioIndex {0}", index); }
    }

    /// <summary>
    /// assign data from a save game storyArray. Overwrites existing data.
    /// </summary>
    /// <param name="storyArray"></param>
    public void SetArrayOfStoryStatus(int[] storyArray)
    {
        if (storyArray != null)
        {
            //clear existing array
            Array.Clear(arrayOfStoryStatus, 0, arrayOfStoryStatus.Length);
            //check both are same length
            Debug.AssertFormat(storyArray.Length == arrayOfStoryStatus.Length, "Mismatch on size of storyArray {0} and arrayOfStoryStatus {1} (should be the same)", storyArray.Length, arrayOfStoryStatus.Length);
            //copy data across
            Array.Copy(storyArray, arrayOfStoryStatus, storyArray.Length);
        }
        else { Debug.LogError("Invalid storyArray parameter (Null)"); }
    }

    /// <summary>
    /// sets mission from loaded save data
    /// </summary>
    public void SetMission()
    {
        switch (campaign.side.level)
        {
            case 1:
                //Authority mission
                if (scenario.missionAuthority != null)
                { GameManager.instance.missionScript.mission = scenario.missionAuthority; }
                break;
            case 2:
                //Resistance mission
                if (scenario.missionResistance != null)
                { GameManager.instance.missionScript.mission = scenario.missionResistance; }
                else { Debug.LogError("Invalid Resistance mission (Null) for scenario"); }
                break;
            default:
                Debug.LogErrorFormat("Unrecognised campaign side {0}", campaign.side.name);
                break;
        }
    }

    //
    // - - - Campaign Status
    //

    public int GetBlackMarks()
    { return blackMarks; }

    public int GetCommendations()
    { return commendations; }

    public int GetInvestigationBlackMarks()
    { return investigationBlackMarks; }

    /// <summary>
    /// used for Save/Load data
    /// </summary>
    /// <param name="value"></param>
    public void SetBlackMarks(int value)
    { blackMarks = value; }

    /// <summary>
    /// used for Save/Load data
    /// </summary>
    /// <param name="value"></param>
    public void SetCommendations(int value)
    { commendations = value; }

    /// <summary>
    /// used for Save/Load data
    /// </summary>
    /// <param name="value"></param>
    public void SetInvestigationBlackMarks(int value)
    { investigationBlackMarks = value; }

    /// <summary>
    /// change value of black marks. Keep reason short
    /// </summary>
    /// <param name="value"></param>
    /// <param name="reason"></param>
    public void ChangeBlackMarks(int value, string reason = "Unknown")
    {
        int previous = blackMarks;
        blackMarks += value;
        Debug.LogFormat("[Cam] CampaignManager.cs -> ChangeBlackMarks: Black Marks now {0}, was {1} (due to {2}){3}", blackMarks, previous, reason, "\n");
    }

    /// <summary>
    /// change value of Commendiations. Keep reason short
    /// </summary>
    /// <param name="value"></param>
    /// <param name="reason"></param>
    public void ChangeCommendations(int value, string reason = "Unknown")
    {
        int previous = commendations;
        commendations += value;
        Debug.LogFormat("[Cam] CampaignManager.cs -> ChangeCommendations: Commendations now {0}, was {1} (due to {2}){3}", commendations, previous, reason, "\n");
    }

    /// <summary>
    /// increment black marks given per investigation by +1 (happens each time a guilty verdict is reached)
    /// </summary>
    public void IncrementInvestigationBlackMarks()
    { investigationBlackMarks++; }

    

    //
    // - - - Debug - - -
    //

    /// <summary>
    /// Debug display
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayCampaignData()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- CampaignData{0}{1}", "\n", "\n");
        builder.AppendFormat(" {0} campaign{1}", campaign.side.name, "\n");
        builder.AppendFormat(" campaign: \"{0}\", {1}{2}", campaign.tag, campaign.descriptor, "\n");
        builder.AppendFormat(" current scenario: index {0}, \"{1}\", {2}{3}", scenarioIndex, scenario.tag, scenario.descriptor, "\n");
        //campaign scenario list in order
        builder.AppendFormat("{0} ListOfScenarios{1}", "\n", "\n");
        int count = campaign.listOfScenarios.Count;
        if (count > 0)
        {
            int index = 0;
            campaign.listOfScenarios.ForEach(scenario => builder.AppendFormat(" {0}: \"{1}\", {2}, seed {3}{4}", index++, scenario.tag, scenario.city.tag,
                scenario.seedCity, "\n"));
        }
        else
        { builder.AppendFormat(" No scenarios found{0}", "\n"); }
        //status
        builder.AppendFormat("{0} Campaign Status{1}", "\n", "\n");
        builder.AppendFormat(" Commendations: {0}{1}", commendations, "\n");
        builder.AppendFormat(" Black Marks: {0}{1}", blackMarks, "\n");    
        builder.AppendFormat(" Investigation Black Marks (if Guilty): {0}{1}", investigationBlackMarks, "\n");
        //win/loss
        builder.AppendFormat("{0} Win/Loss Status{1}", "\n", "\n");
        builder.AppendFormat(" Level WinState: {0}{1}", GameManager.instance.turnScript.winStateLevel, "\n");
        builder.AppendFormat(" Level WinReason: {0}{1}", GameManager.instance.turnScript.winReasonLevel, "\n");
        builder.AppendFormat(" Campaign WinState: {0}{1}", GameManager.instance.turnScript.winStateCampaign, "\n");
        builder.AppendFormat(" Campaign WinReason: {0}{1}", GameManager.instance.turnScript.winReasonCampaign, "\n");
        //story Status array
        builder.AppendFormat("{0} ArrayOfStoryStatus{1}", "\n", "\n");
        /*for (int i = 0; i < arrayOfStoryStatus.Length; i++)
        { builder.AppendFormat(" {0} status: {1}{2}", i, arrayOfStoryStatus[i], "\n"); }*/
        count = 0;
        Array.ForEach(arrayOfStoryStatus, status => builder.AppendFormat(" {0} status: {1}{2}", count++, status, "\n"));
        //organisations
        builder.AppendFormat("{0} Organisations{1}", "\n", "\n");
        Organisation org = campaign.orgCure;
        if (org != null)
        { builder.AppendFormat(" orgCure: {0}, rep {1}, free {2}, isContact {3}{4}", org.tag, org.GetReputation(), org.GetFreedom(), org.isContact, "\n"); }
        org = campaign.orgContract;
        if (org != null)
        { builder.AppendFormat(" orgContract: {0}, rep {1}, free {2}, isContact {3}{4}", org.tag, org.GetReputation(), org.GetFreedom(), org.isContact, "\n"); }
        org = campaign.orgHQ;
        if (org != null)
        { builder.AppendFormat(" orgHQ: {0}, rep {1}, free {2}, isContact {3}{4}", org.tag, org.GetReputation(), org.GetFreedom(), org.isContact, "\n"); }
        org = campaign.orgEmergency;
        if (org != null)
        { builder.AppendFormat(" orgEmergency: {0}, rep {1}, free {2}, isContact {3}{4}", org.tag, org.GetReputation(), org.GetFreedom(), org.isContact, "\n"); }
        org = campaign.orgInfo;
        if (org != null)
        { builder.AppendFormat(" orgInfo: {0}, rep {1}, free {2}, isContact {3}{4}", org.tag, org.GetReputation(), org.GetFreedom(), org.isContact, "\n"); }

        return builder.ToString();
    }

    /*public string DeubgDisplayScenarioDataFunctional()
    {
        return new StringBuilder()
            .AppendFormat("- ScenarioData{0}{1}", "\n", "\n")
            .AppendFormat(" Scenario: \"{0}\", {1}{2}", scenario.tag, scenario.descriptor, "\n")
            .AppendFormat(" Side: {0}{1}", scenario.side.name, "\n")
            .AppendFormat(" City: {0}{1}", scenario.city.tag, "\n")
            .AppendFormat(" Seed: {0}{1}", scenario.seedCity, "\n")
            .AppendFormat(" Leader Resistance: {0}{1}", scenario.leaderResistance.tag, "\n")
            .AppendFormat(" Leader Authority: {0}{1}", scenario.leaderAuthority.mayorName, "\n")
            .AppendFormat(" RebelHQ Approval: {0}{1}", scenario.approvalStartRebelHQ, "\n")
            .AppendFormat(" AuthorityHQ Approval: {0}{1}", scenario.approvalStartAuthorityHQ, "\n")
            .AppendFormat(" City Start Loyalty: {0}{1}", scenario.cityStartLoyalty, "\n")
            .AppendFormat(" Mission Resistance: {0}{1}", scenario.missionResistance.name, "\n")
            .AppendFormat(" Challenge Resistance: {0}{1}", scenario.challengeResistance.name, "\n")
            .AppendFormat(" Number of Turns: {0}{1}", scenario.timer, "\n")
            .ToString();
    }*/

    /// <summary>
    /// Debug display of current scenario
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayScenarioData()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- ScenarioData{0}{1}", "\n", "\n");
        builder.AppendFormat(" Scenario: \"{0}\", {1}{2}", scenario.tag, scenario.descriptor, "\n");
        builder.AppendFormat(" Side: {0}{1}", scenario.side.name, "\n");
        builder.AppendFormat(" City: {0}{1}", scenario.city.tag, "\n");
        builder.AppendFormat(" Seed: {0}{1}", scenario.seedCity, "\n");
        builder.AppendFormat(" Leader Resistance: {0}{1}", scenario.leaderResistance.tag, "\n");
        builder.AppendFormat(" Leader Authority: {0}{1}", scenario.leaderAuthority.mayorName, "\n");
        builder.AppendFormat(" RebelHQ Approval: {0}{1}", scenario.approvalStartRebelHQ, "\n");
        builder.AppendFormat(" AuthorityHQ Approval: {0}{1}", scenario.approvalStartAuthorityHQ, "\n");
        builder.AppendFormat(" City Start Loyalty: {0}{1}", scenario.cityStartLoyalty, "\n");
        builder.AppendFormat(" Mission Resistance: {0}{1}", scenario.missionResistance.name, "\n");
        builder.AppendFormat(" Challenge Resistance: {0}{1}", scenario.challengeResistance.name, "\n");
        builder.AppendFormat(" Number of Turns: {0}{1}", scenario.timer, "\n");

        Mission mission = scenario.missionResistance;
        builder.AppendFormat("{0}-Mission Resistance \"{1}\"{2}", "\n", mission.descriptor, "\n");
        /*foreach(Objective objective in mission.listOfObjectives)
        { builder.AppendFormat(" Objective: {0}{1}", objective.tag, "\n"); }*/
        mission.listOfObjectives.ForEach(objective => builder.AppendFormat(" Objective: {0}{1}", objective.tag, "\n"));
        builder.AppendFormat(" Generic Targets: Live {0}, Active {1}{2}", mission.targetsGenericLive, mission.targetsGenericActive, "\n");
        if (mission.targetBaseCityHall != null)
        { builder.AppendFormat(" Target City Hall: {0}{1}", mission.targetBaseCityHall.targetName, "\n"); }
        if (mission.targetBaseIcon != null)
        { builder.AppendFormat(" Target Icon: {0}{1}", mission.targetBaseIcon.targetName, "\n"); }
        if (mission.targetBaseAirport != null)
        { builder.AppendFormat(" Target Airport: {0}{1}", mission.targetBaseAirport.targetName, "\n"); }
        if (mission.targetBaseHarbour != null)
        { builder.AppendFormat(" Target Harbour: {0}{1}", mission.targetBaseHarbour.targetName, "\n"); }
        if (mission.targetBaseVIP != null)
        { builder.AppendFormat(" Target VIP: {0}{1}", mission.targetBaseVIP.targetName, "\n"); }
        if (mission.targetBaseStory != null)
        { builder.AppendFormat(" Target Story: {0}{1}", mission.targetBaseStory.targetName, "\n"); }
        if (mission.targetBaseGoal != null)
        { builder.AppendFormat(" Target Goal: {0}{1}", mission.targetBaseGoal.targetName, "\n"); }
        if (mission.profileGenericLive != null)
        { builder.AppendFormat(" Profile Generic Live: {0}{1}", mission.profileGenericLive.name, "\n"); }
        if (mission.profileGenericActive != null)
        { builder.AppendFormat(" Profile Generic Active: {0}{1}", mission.profileGenericActive.name, "\n"); }
        if (mission.profileGenericFollowOn != null)
        { builder.AppendFormat(" Profile Generic FollowOn: {0}{1}", mission.profileGenericFollowOn.name, "\n"); }
        /*foreach(ObjectiveTarget objectiveTarget in mission.listOfObjectiveTargets)
        { builder.AppendFormat(" ObjectiveTarget: {0}{1}", objectiveTarget.name, "\n"); }*/
        mission.listOfObjectiveTargets.ForEach(objectiveTarget => builder.AppendFormat(" ObjectiveTarget: {0}{1}", objectiveTarget.name, "\n"));
        //Npc
        if (GameManager.instance.missionScript.mission.npc != null)
        {
            builder.AppendFormat("{0}{1}-Npc{2}", "\n", "\n", "\n");
            builder.AppendFormat(" Npc Name: {0}{1}", mission.npc.tag, "\n");
            builder.AppendFormat(" startTurn: {0}{1}", mission.npc.startTurn, "\n");
            builder.AppendFormat(" startChance: {0}{1}", mission.npc.startChance, "\n");
            builder.AppendFormat(" stealthRating: {0}{1}", mission.npc.stealthRating, "\n");
            builder.AppendFormat(" Start NpcNode: {0}{1}", mission.npc.nodeStart.name, "\n");
            Node node = mission.npc.currentStartNode;
            if (node != null)
            { builder.AppendFormat(" currentStartNode: {0}, {1}, ID {2}{3}", node.nodeName, node.Arc.name, node.nodeID, "\n"); }
            else { builder.AppendFormat(" currentStartNode: Invalid{0}", "\n"); }
            builder.AppendFormat(" End NpcNode: {0}{1}", mission.npc.nodeEnd.name, "\n");
            node = mission.npc.currentEndNode;
            if (node != null)
            { builder.AppendFormat(" currentEndNode: {0}, {1}, ID {2}{3}", node.nodeName, node.Arc.name, node.nodeID, "\n"); }
            else { builder.AppendFormat(" currentEndNode: Invalid{0}", "\n"); }
            builder.AppendFormat(" Status: {0}{1}", mission.npc.status, "\n");
            if (mission.npc.status == NpcStatus.Active)
            {
                builder.AppendFormat(" currentNode: {0}, {1}, ID {2}{3}", mission.npc.currentNode.nodeName, mission.npc.currentNode.Arc.name, mission.npc.currentNode.nodeID, "\n");
                builder.AppendFormat(" timerTurns: {0} (start {1}){2}", mission.npc.timerTurns, mission.npc.maxTurns, "\n");
                builder.AppendFormat(" moveChance: {0}{1}", mission.npc.moveChance, "\n");
                builder.AppendFormat(" isRepeat: {0}{1}", mission.npc.isRepeat, "\n");
            }
        }
        return builder.ToString();
    }

    //new methods above here
}
