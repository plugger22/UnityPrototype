using gameAPI;
using System;
using System.Text;
using UnityEngine;

/// <summary>
/// handles all Campaign related matters
/// </summary>
public class CampaignManager : MonoBehaviour
{

    [Tooltip("Current Campaign (this is the default campaign at game start)")]
    public Campaign campaign;
    [Tooltip("Number of story status flags used to track stuff story developments across a multi-scenario campaign (size of the array)")]
    public int numOfFlags = 10;



    #region Save Data Compatible
    //master flags used to progress Story elements
    [HideInInspector] public int[] arrayOfStoryStatus;

    private int scenarioIndex;                   //list index of current scenario, eg. '0' for first in the list at start of the campaign
    #endregion

    [HideInInspector] public Scenario scenario;


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
    /// Debug display
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayCampaignData()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- CampaignData{0}{1}", "\n", "\n");
        builder.AppendFormat(" campaign: \"{0}\", {1}{2}", campaign.tag, campaign.descriptor, "\n");
        builder.AppendFormat(" current scenario: index {0}, \"{1}\", {2}{3}", scenarioIndex, scenario.tag, scenario.descriptor, "\n");
        //campaign scenario list in order
        builder.AppendFormat("{0} ListOfScenarios{1}", "\n", "\n");
        int count = campaign.listOfScenarios.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            { builder.AppendFormat(" {0}: \"{1}\", {2}, seed {3}{4}", i, campaign.listOfScenarios[i].tag, campaign.listOfScenarios[i].city.tag, 
                campaign.listOfScenarios[i].seedCity, "\n"); }
        }
        else
        { builder.AppendFormat(" No scenarios found{0}", "\n"); }
        //story Status array
        builder.AppendFormat("{0} ArrayOfStoryStatus{1}", "\n", "\n");
        for (int i = 0; i < arrayOfStoryStatus.Length; i++)
        { builder.AppendFormat(" {0} status: {1}{2}", i, arrayOfStoryStatus[i], "\n"); }
        return builder.ToString();
    }

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
        foreach(Objective objective in mission.listOfObjectives)
        { builder.AppendFormat(" Objective: {0}{1}", objective.tag, "\n"); }
        builder.AppendFormat(" Generic Targets: Live {0}, Active {1}{2}", mission.targetsGenericLive, mission.targetsGenericActive, "\n");
        builder.AppendFormat(" Target City Hall: {0}{1}", mission.targetBaseCityHall.targetName, "\n");
        builder.AppendFormat(" Target Icon: {0}{1}", mission.targetBaseIcon.targetName, "\n");
        builder.AppendFormat(" Target Airport: {0}{1}", mission.targetBaseAirport.targetName, "\n");
        builder.AppendFormat(" Target Harbour: {0}{1}", mission.targetBaseHarbour.targetName, "\n");
        builder.AppendFormat(" Target VIP: {0}{1}", mission.targetBaseVIP.targetName, "\n");
        builder.AppendFormat(" Target Story: {0}{1}", mission.targetBaseStory.targetName, "\n");
        builder.AppendFormat(" Target Goal: {0}{1}", mission.targetBaseGoal.targetName, "\n");

        return builder.ToString();
    }

    //new methods above here
}
