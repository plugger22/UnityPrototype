using dijkstraAPI;
using gameAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// handles data validation at game start. Optional toggle in GameManager to run, or not
/// Generates '[Val]' log tag entries for any failed validation checks (Not errors as you want it to complete the run and check all)
/// </summary>
public class ValidationManager : MonoBehaviour
{
    [Header("Campaign.SO Pool Criteria")]
    [Tooltip("TopicType for Campaign.SO pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicType campaignType;
    [Tooltip("TopicSubType for Campaign.SO pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType campaignAlphaSubType;
    [Tooltip("TopicSubType for Campaign.SO pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType campaignBravoSubType;
    [Tooltip("TopicSubType for Campaign.SO pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType campaignCharlieSubType;
    [Tooltip("TopicType for Campaign.SO pool (used to run validation checks to ensure the correct pool is used)")]

    public TopicType familyType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType familyAlphaSubType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType familyBravoSubType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType familyCharlieSubType;

    [Tooltip("TopicType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicType authorityType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType authorityCampaignSubType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType authorityGeneralSubType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType authorityTeamSubType;

    [Tooltip("TopicType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicType resistanceType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType resistanceCampaignSubType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType resistanceGeneralSubType;

    [Tooltip("TopicType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicType hqType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType hqSubType;

    [Tooltip("TopicType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicType captureType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType captureSubType;

    [Tooltip("TopicType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicType actorType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType actorContactSubType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType actorDistrictSubType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType actorGearSubType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType actorMatchSubType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType actorPoliticSubType;

    [Tooltip("TopicType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicType playerType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType playerDistrictSubType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType playerGeneralSubType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType playerStatsSubType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType playerConditionsSubType;

    [Tooltip("TopicType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicType organisationType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType orgCureSubType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType orgContractSubType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType orgHQSubType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType orgEmergencySubType;
    [Tooltip("TopicSubType for Campaign.SO  pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType orgInfoSubType;

    [Header("City.SO Pool Criteria")]
    [Tooltip("TopicType for City.SO pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicType cityType;
    [Tooltip("TopicSubType for City.SO pool (used to run validation checks to ensure the correct pool is used)")]
    public TopicSubType citySubType;

    [Header("Topic Profile")]
    [Tooltip("The 'Normal' topic profile with no delays. Needed to prevent hardcoding name, will auto adjust if TopicProfile.SO renamed")]
    public TopicProfile normalProfile;

    [Header("Topic Scope")]
    [Tooltip("TopicScope for TopicSubType (used to run validation checks to ensure the correct scope is used)")]
    public TopicScope levelScope;
    [Tooltip("TopicScope for TopicSubType (used to run validation checks to ensure correct scope is used")]
    public TopicScope campaignScope;

    [Header("Topic Data")]
    [Tooltip("Max length of a topic (excludes tagged data) in chars (default is a twitter's worth of 140 chars")]
    [Range(100, 200)] public int maxTopicTextLength = 150;
    [Tooltip("Max length of a topic Option text (excludes tagged data) in chars")]
    [Range(10, 50)] public int maxOptionTextLength = 40;

    //fast access
    private GlobalSide globalAuthority;
    private GlobalSide globalResistance;
    private string levelScopeName;
    private string campaignScopeName;


    /// <summary>
    /// Not called for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseTopicTypes();
                ValidateTargets();
                ValidateGear();
                ValidateMissions();
                ValidateTextLists();
                ValidateCities();
                ValidateTopics();
                ValidateScenarios();
                ValidateCampaigns();
                ValidateCapture();
                break;
            case GameState.FollowOnInitialisation:
            //do nothing
            //break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }

    #region Initialise SubMethods

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        if (levelScope != null) { levelScopeName = levelScope.name; }
        if (campaignScope != null) { campaignScopeName = campaignScope.name; }
        Debug.Assert(levelScope != null, "Invalid levelScope (Null)");
        Debug.Assert(string.IsNullOrEmpty(levelScopeName) == false, "Invalid levelScopeName (Null or Empty)");
        Debug.Assert(campaignScope != null, "Invalid campaignScope (Null)");
        Debug.Assert(string.IsNullOrEmpty(campaignScopeName) == false, "Invalid campaignScopeName (Null or Empty)");
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(normalProfile != null, "Invalid normalProfile (Null)");


    }
    #endregion

    #region SubInitialiseTopicTypes
    /// <summary>
    /// check subTypes are valid for their matching types for Topic Pool Criteria (public fields for class)
    /// </summary>
    private void SubInitialiseTopicTypes()
    {
        //subType checks -> Campaign
        if (campaignType != null)
        {
            //alpha
            if (campaignAlphaSubType != null)
            {
                if (campaignType.listOfSubTypes.Exists(x => x.name.Equals(campaignAlphaSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: campaignSubType \"{0}\" Does Not Match campaignType \"{1}\"{2}", campaignAlphaSubType.name, campaignType.name, "\n"); }
            }
            else { Debug.LogError("Invalid campaignAlphaSubType (Null)"); }
            //bravo
            if (campaignBravoSubType != null)
            {
                if (campaignType.listOfSubTypes.Exists(x => x.name.Equals(campaignBravoSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: campaignSubType \"{0}\" Does Not Match campaignType \"{1}\"{2}", campaignBravoSubType.name, campaignType.name, "\n"); }
            }
            else { Debug.LogError("Invalid campaignBravoSubType (Null)"); }
            //charlie
            if (campaignCharlieSubType != null)
            {
                if (campaignType.listOfSubTypes.Exists(x => x.name.Equals(campaignCharlieSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: campaignSubType \"{0}\" Does Not Match campaignType \"{1}\"{2}", campaignCharlieSubType.name, campaignType.name, "\n"); }
            }
            else { Debug.LogError("Invalid campaignCharlieSubType (Null)"); }
        }
        else { Debug.LogError("Invalid campaignType (Null)"); }
        //subType checks -> Family
        if (familyType != null)
        {
            //alpha
            if (familyAlphaSubType != null)
            {
                if (familyType.listOfSubTypes.Exists(x => x.name.Equals(familyAlphaSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: familyAlphaSubType \"{0}\" Does Not Match familyType \"{1}\"{2}", familyAlphaSubType.name, familyType.name, "\n"); }
            }
            else { Debug.LogError("Invalid familyAlphaSubType (Null)"); }
            //bravo
            if (familyBravoSubType != null)
            {
                if (familyType.listOfSubTypes.Exists(x => x.name.Equals(familyBravoSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: familyBravoSubType \"{0}\" Does Not Match familyType \"{1}\"{2}", familyBravoSubType.name, familyType.name, "\n"); }
            }
            else { Debug.LogError("Invalid familyBravoSubType (Null)"); }
            //charlie
            if (familyCharlieSubType != null)
            {
                if (familyType.listOfSubTypes.Exists(x => x.name.Equals(familyCharlieSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: familyCharlieSubType \"{0}\" Does Not Match familyType \"{1}\"{2}", familyCharlieSubType.name, familyType.name, "\n"); }
            }
            else { Debug.LogError("Invalid familyCharlieSubType (Null)"); }
        }
        else { Debug.LogError("Invalid familyType (Null)"); }
        //subType checks -> authority
        if (authorityType != null)
        {
            //authorityCampaign
            if (authorityCampaignSubType != null)
            {
                if (authorityType.listOfSubTypes.Exists(x => x.name.Equals(authorityCampaignSubType.name, StringComparison.Ordinal)) == false)
                {
                    Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: authorityCampaignSubType \"{0}\" Does Not Match authorityType \"{1}\"{2}",
                      authorityCampaignSubType.name, authorityType.name, "\n");
                }
            }
            else { Debug.LogError("Invalid authorityCampaignSubType (Null)"); }
            //authorityGeneral
            if (authorityGeneralSubType != null)
            {
                if (authorityType.listOfSubTypes.Exists(x => x.name.Equals(authorityGeneralSubType.name, StringComparison.Ordinal)) == false)
                {
                    Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: authorityGeneralSubType \"{0}\" Does Not Match authorityType \"{1}\"{2}",
                      authorityGeneralSubType.name, authorityType.name, "\n");
                }
            }
            else { Debug.LogError("Invalid authorityGeneralSubType (Null)"); }
            //authorityTeam
            if (authorityTeamSubType != null)
            {
                if (authorityType.listOfSubTypes.Exists(x => x.name.Equals(authorityTeamSubType.name, StringComparison.Ordinal)) == false)
                {
                    Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: authorityTeamSubType \"{0}\" Does Not Match authorityType \"{1}\"{2}",
                      authorityTeamSubType.name, authorityType.name, "\n");
                }
            }
            else { Debug.LogError("Invalid authorityTeamSubType (Null)"); }
        }
        else { Debug.LogError("Invalid authorityType (Null)"); }
        //subType checks -> Resistance
        if (resistanceType != null)
        {
            //resistanceCampaign
            if (resistanceCampaignSubType != null)
            {
                if (resistanceType.listOfSubTypes.Exists(x => x.name.Equals(resistanceCampaignSubType.name, StringComparison.Ordinal)) == false)
                {
                    Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: resistanceCampaignSubType \"{0}\" Does Not Match resistanceType \"{1}\"{2}",
                      resistanceCampaignSubType.name, resistanceType.name, "\n");
                }
            }
            else { Debug.LogError("Invalid resistanceCampaignType (Null)"); }
            //resistanceGeneral
            if (resistanceGeneralSubType != null)
            {
                if (resistanceType.listOfSubTypes.Exists(x => x.name.Equals(resistanceGeneralSubType.name, StringComparison.Ordinal)) == false)
                {
                    Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: resistanceGeneralSubType \"{0}\" Does Not Match resistanceType \"{1}\"{2}",
                      resistanceGeneralSubType.name, resistanceType.name, "\n");
                }
            }
            else { Debug.LogError("Invalid resistanceCampaignType (Null)"); }
        }
        else { Debug.LogError("Invalid resistanceType (Null)"); }
        //subType checks -> City
        if (cityType != null)
        {
            if (citySubType != null)
            {
                if (cityType.listOfSubTypes.Exists(x => x.name.Equals(citySubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: citySubType \"{0}\" Does Not Match cityType \"{1}\"{2}", citySubType.name, cityType.name, "\n"); }
            }
            else { Debug.LogError("Invalid citySubType (Null)"); }
        }
        else
        { Debug.LogError("Invalid cityType (Null)"); }
        //subType checks -> HQ
        if (hqType != null)
        {
            if (hqSubType != null)
            {
                if (hqType.listOfSubTypes.Exists(x => x.name.Equals(hqSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: hqSubType \"{0}\" Does Not Match hqType \"{1}\"{2}", hqSubType.name, hqType.name, "\n"); }
            }
            else { Debug.LogError("Invalid hqSubType (Null)"); }
        }
        else
        { Debug.LogError("Invalid hqType (Null)"); }
        //subType checks -> Capture
        if (captureType != null)
        {
            if (captureSubType != null)
            {
                if (captureType.listOfSubTypes.Exists(x => x.name.Equals(captureSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: captureSubType \"{0}\" Does Not Match captureType \"{1}\"{2}", captureSubType.name, captureType.name, "\n"); }
            }
            else { Debug.LogError("Invalid captureSubType (Null)"); }
        }
        else
        { Debug.LogError("Invalid captureType (Null)"); }
        //subType checks -> Player
        if (playerType != null)
        {
            //player District
            if (playerDistrictSubType != null)
            {
                if (playerType.listOfSubTypes.Exists(x => x.name.Equals(playerDistrictSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: playerDistrictSubType \"{0}\" Does Not Match playerType \"{1}\"{2}", playerDistrictSubType.name, playerType.name, "\n"); }
            }
            else { Debug.LogError("Invalid playerDistrictSubType (Null)"); }
            //player General
            if (playerGeneralSubType != null)
            {
                if (playerType.listOfSubTypes.Exists(x => x.name.Equals(playerGeneralSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: playerGeneralSubType \"{0}\" Does Not Match playerType \"{1}\"{2}", playerGeneralSubType.name, playerType.name, "\n"); }
            }
            else { Debug.LogError("Invalid playerGeneralSubType (Null)"); }
            //player Stats
            if (playerStatsSubType != null)
            {
                if (playerType.listOfSubTypes.Exists(x => x.name.Equals(playerStatsSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: playerStatsSubType \"{0}\" Does Not Match playerType \"{1}\"{2}", playerStatsSubType.name, playerType.name, "\n"); }
            }
            else { Debug.LogError("Invalid playerStatsSubType (Null)"); }
            //player Conditions
            if (playerConditionsSubType != null)
            {
                if (playerType.listOfSubTypes.Exists(x => x.name.Equals(playerConditionsSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: playerConditionsSubType \"{0}\" Does Not Match playerType \"{1}\"{2}", playerConditionsSubType.name, playerType.name, "\n"); }
            }
            else { Debug.LogError("Invalid playerConditionsSubType (Null)"); }
        }
        else
        { Debug.LogError("Invalid playerType (Null)"); }
        //subType checks -> Actor
        if (actorType != null)
        {
            //actorContact
            if (actorContactSubType != null)
            {
                if (actorType.listOfSubTypes.Exists(x => x.name.Equals(actorContactSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: actorContactSubType \"{0}\" Does Not Match actorType \"{1}\"{2}", actorContactSubType.name, actorType.name, "\n"); }
            }
            else { Debug.LogError("Invalid actorContactSubType (Null)"); }
            //actorDistrict
            if (actorDistrictSubType != null)
            {
                if (actorType.listOfSubTypes.Exists(x => x.name.Equals(actorDistrictSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: actorDistrictSubType \"{0}\" Does Not Match actorType \"{1}\"{2}", actorDistrictSubType.name, actorType.name, "\n"); }
            }
            else { Debug.LogError("Invalid actorDistrictSubType (Null)"); }
            //actorGear
            if (actorGearSubType != null)
            {
                if (actorType.listOfSubTypes.Exists(x => x.name.Equals(actorGearSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: actorGearSubType \"{0}\" Does Not Match actorType \"{1}\"{2}", actorGearSubType.name, actorType.name, "\n"); }
            }
            else { Debug.LogError("Invalid actorGearSubType (Null)"); }
            //actorMatch
            if (actorMatchSubType != null)
            {
                if (actorType.listOfSubTypes.Exists(x => x.name.Equals(actorMatchSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: actorMatchSubType \"{0}\" Does Not Match actorType \"{1}\"{2}", actorMatchSubType.name, actorType.name, "\n"); }
            }
            else { Debug.LogError("Invalid actorMatchSubType (Null)"); }
            //actorPolitic
            if (actorPoliticSubType != null)
            {
                if (actorType.listOfSubTypes.Exists(x => x.name.Equals(actorPoliticSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: actorPoliticSubType \"{0}\" Does Not Match actorType \"{1}\"{2}", actorPoliticSubType.name, actorType.name, "\n"); }
            }
            else { Debug.LogError("Invalid actorPoliticSubType (Null)"); }
        }
        else { Debug.LogError("Invalid actorType (Null)"); }
        //subType checks -> Organisation
        if (organisationType != null)
        {
            //orgCure
            if (orgCureSubType != null)
            {
                if (organisationType.listOfSubTypes.Exists(x => x.name.Equals(orgCureSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: orgCureSubType \"{0}\" Does Not Match organisationType \"{1}\"{2}", orgCureSubType.name, organisationType.name, "\n"); }
            }
            else { Debug.LogError("Invalid orgCureSubType (Null)"); }
            //orgContract
            if (orgContractSubType != null)
            {
                if (organisationType.listOfSubTypes.Exists(x => x.name.Equals(orgContractSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: orgContractSubType \"{0}\" Does Not Match organisationType \"{1}\"{2}", orgContractSubType.name, organisationType.name, "\n"); }
            }
            else { Debug.LogError("Invalid orgContractSubType (Null)"); }
            //orgHQ
            if (orgHQSubType != null)
            {
                if (organisationType.listOfSubTypes.Exists(x => x.name.Equals(orgHQSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: orgHQSubType \"{0}\" Does Not Match organisationType \"{1}\"{2}", orgHQSubType.name, organisationType.name, "\n"); }
            }
            else { Debug.LogError("Invalid orgHQSubType (Null)"); }
            //orgEmergency
            if (orgEmergencySubType != null)
            {
                if (organisationType.listOfSubTypes.Exists(x => x.name.Equals(orgEmergencySubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: orgEmergencySubType \"{0}\" Does Not Match organisationType \"{1}\"{2}", orgEmergencySubType.name, organisationType.name, "\n"); }
            }
            else { Debug.LogError("Invalid orgEmergencySubType (Null)"); }
            //orgInfo
            if (orgInfoSubType != null)
            {
                if (organisationType.listOfSubTypes.Exists(x => x.name.Equals(orgInfoSubType.name, StringComparison.Ordinal)) == false)
                { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: orgInfoSubType \"{0}\" Does Not Match organisationType \"{1}\"{2}", orgInfoSubType.name, organisationType.name, "\n"); }
            }
            else { Debug.LogError("Invalid orgInfoSubType (Null)"); }
        }
    }
    #endregion

    #endregion

    //
    // - - - Validation checks (Session start)
    //

    #region ValidateTargets
    /// <summary>
    /// Checks targets
    /// </summary>
    private void ValidateTargets()
    {
        Dictionary<string, Target> dictOfTargets = GameManager.instance.dataScript.GetDictOfTargets();
        /*for (int index = 0; index < dictOfTargets.Count; index++)*/
        foreach (var target in dictOfTargets)
        {
            if (target.Value != null)
            {
                //Good effects
                if (target.Value.listOfGoodEffects.Count > 0)
                {
                    foreach (Effect effect in target.Value.listOfGoodEffects)
                    {
                        if (effect != null)
                        {
                            //should be duration 'Single'
                            if (effect.duration.name.Equals("Single", StringComparison.Ordinal) == false)
                            { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Good effect \"{1}\" NOT Single", target.Value.targetName, effect.name); }
                        }
                        else { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\"  invalid Good effect (Null)", target.Value.targetName); }
                    }
                }
                //Bad effects
                if (target.Value.listOfBadEffects.Count > 0)
                {
                    foreach (Effect effect in target.Value.listOfBadEffects)
                    {
                        if (effect != null)
                        {
                            //should be duration 'Single'
                            if (effect.duration.name.Equals("Single", StringComparison.Ordinal) == false)
                            { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Bad effect \"{1}\" NOT Single", target.Value.targetName, effect.name); }
                        }
                        else { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\"  invalid Bad effect (Null)", target.Value.targetName); }
                    }
                }
                //Fail effects
                if (target.Value.listOfFailEffects.Count > 0)
                {
                    foreach (Effect effect in target.Value.listOfFailEffects)
                    {
                        if (effect != null)
                        {
                            //should be duration 'Single'
                            if (effect.duration.name.Equals("Single", StringComparison.Ordinal) == false)
                            { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Fail effect \"{1}\" NOT Single", target.Value.targetName, effect.name); }
                        }
                        else { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\"  invalid Fail effect (Null)", target.Value.targetName); }
                    }
                }
                //Ongoing effects
                if (target.Value.ongoingEffect != null)
                {
                    //should be duration 'Ongoing'
                    if (target.Value.ongoingEffect.duration.name.Equals("Ongoing", StringComparison.Ordinal) == false)
                    { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" <b>ongoing effect \"{1}\" NOT Ongoing</b>", target.Value.targetName, target.Value.ongoingEffect.name); }
                }
                //Target gear present and shouldn't be infiltration
                if (target.Value.gear != null)
                {
                    if (target.Value.gear.name.Equals("Infiltration", StringComparison.Ordinal) == true)
                    { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" <b>is using INFILTRATION gear</b> (doubled up as can be used automatically on any target)", target.Value.targetName); }
                }
                else { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\"  invalid Gear (Null)", target.Value.targetName); }
            }
            else { Debug.LogErrorFormat("Invalid target (Null) for target {0}", target.Key); }
        }
    }
    #endregion

    #region ValidateGear
    /// <summary>
    /// Checks Gear
    /// </summary>
    private void ValidateGear()
    {
        Dictionary<string, Gear> dictOfGear = GameManager.instance.dataScript.GetDictOfGear();
        if (dictOfGear != null)
        {
            foreach (var gear in dictOfGear)
            {
                if (gear.Value != null)
                {
                    //check any effect
                    if (gear.Value.listOfPersonalEffects.Count > 0)
                    {
                        //personal effects
                        foreach (var effect in gear.Value.listOfPersonalEffects)
                        {
                            if (effect != null)
                            {
                                //Can't be ONGOING
                                if (effect.duration.name.Equals("Ongoing", StringComparison.Ordinal) == true)
                                { Debug.LogFormat("[Val] ValidateGear: Gear \"{0}\"  invalid Personal effect (ONGOING)", gear.Value.name); }
                            }
                            else { Debug.LogFormat("[Val] ValidateGear: Gear \"{0}\"  invalid effect (Null)", gear.Value.name); }
                        }
                        //AI hacking effects
                        if (gear.Value.aiHackingEffect != null)
                        {
                            //Can't be ONGOING
                            if (gear.Value.aiHackingEffect.duration.name.Equals("Ongoing", StringComparison.Ordinal) == true)
                            { Debug.LogFormat("[Val] ValidateGear: Gear \"{0}\"  invalid aiHacking effect (ONGOING)", gear.Value.name); }
                        }
                    }
                }
                else { Debug.LogFormat("[Val] ValidateGar: Invalid Gear (Null) for gearID {0}", gear.Key); }
            }
        }
    }
    #endregion

    #region ValidateMissions
    /// <summary>
    /// check for unique targets and objectives and check that all ObjectveTargets have matching targets/objectives present Check objective sides match that of mission
    /// </summary>
    private void ValidateMissions()
    {
        Mission[] arrayOfMissions = GameManager.instance.loadScript.arrayOfMissions;
        if (arrayOfMissions != null)
        {
            List<string> listOfNames = new List<string>();
            List<Target> listOfTargets = new List<Target>();
            //loop missions
            foreach (Mission mission in arrayOfMissions)
            {
                if (mission != null)
                {
                    //
                    // - - - Targets
                    //
                    listOfTargets.Clear();
                    //populate list with targets
                    if (mission.targetBaseCityHall != null)
                    { listOfTargets.Add(mission.targetBaseCityHall); }
                    else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateMissions: {0} mission has no targetBaseCityHall{1}", mission.name, "\n"); }
                    if (mission.targetBaseIcon != null)
                    { listOfTargets.Add(mission.targetBaseIcon); }
                    else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateMissions: {0} mission has no targetBaseIcon{1}", mission.name, "\n"); }
                    if (mission.targetBaseAirport != null)
                    { listOfTargets.Add(mission.targetBaseAirport); }
                    else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateMissions: {0} mission has no targetBaseAirport{1}", mission.name, "\n"); }
                    if (mission.targetBaseHarbour != null)
                    { listOfTargets.Add(mission.targetBaseHarbour); }
                    else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateMissions: {0} mission has no targetBaseHarbour{1}", mission.name, "\n"); }
                    if (mission.targetBaseVIP != null)
                    { listOfTargets.Add(mission.targetBaseVIP); }
                    else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateMissions: {0} mission has no targetBaseVIP{1}", mission.name, "\n"); }
                    if (mission.targetBaseStory != null)
                    { listOfTargets.Add(mission.targetBaseStory); }
                    else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateMissions: {0} mission has no targetBaseStory{1}", mission.name, "\n"); }
                    if (mission.targetBaseGoal != null)
                    { listOfTargets.Add(mission.targetBaseGoal); }
                    else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateMissions: {0} mission has no targetBaseGoal{1}", mission.name, "\n"); }
                    //create a list of target.name
                    listOfNames.Clear();
                    for (int i = 0; i < listOfTargets.Count; i++)
                    { listOfNames.Add(listOfTargets[i].name); }
                    //check for duplicates
                    CheckListForDuplicates(listOfNames, "Mission", mission.name, "Targets");
                    //
                    // - - - Objectives
                    //
                    if (mission.listOfObjectives.Count > 0)
                    {
                        //check for duplicates
                        listOfNames.Clear();
                        for (int i = 0; i < mission.listOfObjectives.Count; i++)
                        {
                            Objective objective = mission.listOfObjectives[i];
                            if (objective != null)
                            {
                                listOfNames.Add(objective.name);
                                //check side matches that of mission
                                if (objective.side.level != mission.side.level)
                                {
                                    Debug.LogFormat("[Val] ValidationManager.cs -> ValidateMissions: mission {0}, obj \"{1}\", Invalid Side (is {2}, should be {3}){4}", mission.name, objective.name,
                                      objective.side.name, mission.side.name, "\n");
                                }
                            }
                            else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateMissions: mission {0}, Invalid Objective for listOfObjectives[{1}]{2}", mission.name, i, "\n"); }
                        }
                        //check for duplicates
                        CheckListForDuplicates(listOfNames, "Mission", mission.name, "Objectives");
                    }
                    //
                    // - - - ObjectiveTargets
                    //
                    if (mission.listOfObjectiveTargets.Count > 0)
                    {
                        //check for duplicates
                        listOfNames.Clear();
                        for (int i = 0; i < mission.listOfObjectiveTargets.Count; i++)
                        { listOfNames.Add(mission.listOfObjectiveTargets[i].name); }
                        //check for duplicates
                        CheckListForDuplicates(listOfNames, "Mission", mission.name, "ObjectiveTargets");
                        //check no more than there are objectives
                        if (mission.listOfObjectiveTargets.Count > mission.listOfObjectives.Count)
                        {
                            Debug.LogFormat("[Val] ValidationManager.cs -> ValidateMssion: Mission \"{0}\", there are more ObjectiveTargets than Objectives ({1} vs. {2}){3}", mission.name,
                                mission.listOfObjectiveTargets.Count, mission.listOfObjectives.Count, "\n");
                        }
                        //check for presence of related data
                        foreach (ObjectiveTarget objTarget in mission.listOfObjectiveTargets)
                        {
                            if (objTarget != null)
                            {
                                //corresponding target present
                                if (listOfTargets.Exists(x => x.name.Equals(objTarget.target.name, StringComparison.Ordinal)) == false)
                                {
                                    Debug.LogFormat("[Val] ValidationMission: Mission \"{0}\", objectiveTarget \"{1}\", target \"{2}\", has no corresponding target{3}", mission.name, objTarget.name,
                                        objTarget.target.name, "\n");
                                }
                                //corresponding objective present
                                if (mission.listOfObjectives.Exists(x => x.name.Equals(objTarget.objective.name, StringComparison.Ordinal)) == false)
                                {
                                    Debug.LogFormat("[Val] ValidationMission: Mission \"{0}\", objectiveTarget \"{1}\", objective \"{2}\", has no corresponding objective{3}", mission.name, objTarget.name,
                                        objTarget.objective.name, "\n");
                                }
                            }
                            else { Debug.LogWarningFormat("Invalid objectiveTarget (Null) for mission {0}", mission.name); }
                        }

                    }
                    //
                    // - - - Npc's
                    //
                    if (mission.npc != null)
                    {
                        //must have at least one good/bad effects
                        List<Effect> listOfEffects = new List<Effect>();
                        listOfEffects.AddRange(mission.npc.listOfGoodEffects);
                        listOfEffects.AddRange(mission.npc.listOfBadEffects);
                        if (listOfEffects.Count == 0)
                        { Debug.LogFormat("[Val] ValidationMission: Mission \"{0}\", npc \"[1}\", has no effects (must have at least one){2}", mission.name, mission.npc.name, "\n"); }
                    }
                }
                else { Debug.LogError("Invalid mission (Null) in arrayOfMissions"); }
            }
        }
        else { Debug.LogError("Invalid arrayOfMissions (Null)"); }
    }
    #endregion

    #region ValidateTextLists
    /// <summary>
    /// checks all textlists for duplicates
    /// </summary>
    private void ValidateTextLists()
    {
        TextList[] arrayOfTextLists = GameManager.instance.loadScript.arrayOfTextLists;
        if (arrayOfTextLists != null)
        {
            //loop textlists
            List<string> findList = new List<string>();
            int count;
            for (int i = 0; i < arrayOfTextLists.Length; i++)
            {
                TextList textList = arrayOfTextLists[i];
                if (textList != null)
                {
                    //check for duplicates
                    if (textList.isTestForDuplicates == true)
                    {
                        List<string> tempList = new List<string>(textList.randomList);
                        if (tempList != null)
                        {
                            //loop temp list and check against master text list
                            foreach (String item in tempList)
                            {
                                findList.Clear();
                                findList = textList.randomList.FindAll(x => x == item);
                                count = findList.Count;
                                //always one copy present, shouldn't be any more
                                if (count > 1)
                                {
                                    //ignore first, legit, copy
                                    count--;
                                    Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTextList: {0} Duplicate{1} exist for \"{2}\" in textList {3}{4}", count, count != 1 ? "s" : "", item, textList.name, "\n");
                                }
                            }
                        }
                        else { Debug.LogErrorFormat("Invalid randomList (Null) for textList {0}", textList.descriptor); }
                    }
                    //check for valid Text tags
                    if (textList.isTestForTextTags == true)
                    {
                        CheckTextData data = new CheckTextData() { isValidate = true };
                        for (int j = 0; j < textList.randomList.Count; j++)
                        {
                            data.text = textList.randomList[j];
                            data.objectName = string.Format("{0}, line {1}", textList.name, j);
                            GameManager.instance.newsScript.CheckNewsText(data);
                        }
                    }
                }
                else { Debug.LogErrorFormat("Invalid textList (Null) for arrayOfTextLists[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid arrayOfTextLists (Null)"); }
    }

    #endregion

    #region ValidateCities
    //checks that city speciality district names (icon, airport, harbour) aren't present in the cities list of DistrictNames
    private void ValidateCities()
    {
        City[] arrayOfCities = GameManager.instance.loadScript.arrayOfCities;
        if (arrayOfCities != null)
        {
            for (int i = 0; i < arrayOfCities.Length; i++)
            {
                City city = arrayOfCities[i];
                if (city != null)
                {
                    if (city.iconDistrict != null)
                    {
                        if (city.districtNames.CheckItemPresent(city.iconDistrict) == true)
                        { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateCities: {0} iconDistrict \"{1}\", present in textList {2}{3}", city.tag, city.iconDistrict, city.districtNames.name, "\n"); }
                    }
                    if (city.airportDistrict != null)
                    {
                        if (city.districtNames.CheckItemPresent(city.airportDistrict) == true)
                        { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateCities: {0} airportDistrict \"{1}\", present in textList {2}{3}", city.tag, city.airportDistrict, city.districtNames.name, "\n"); }
                    }
                    if (city.harbourDistrict != null)
                    {
                        if (city.districtNames.CheckItemPresent(city.harbourDistrict) == true)
                        { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateCities: {0} harbourDistrict \"{1}\", present in textList {2}{3}", city.tag, city.harbourDistrict, city.districtNames.name, "\n"); }
                    }
                }
                else { Debug.LogErrorFormat("Invalid city (Null) in arrayOfCities[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid arrayOfCities (Null)"); }
    }
    #endregion

    #region ValidateTopics
    /// <summary>
    /// checks topicTypes and topicSubTypes are correctly set up
    /// </summary>
    private void ValidateTopics()
    {
        int count, countSubType, textLength, num;
        bool isPassedCheck;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        if (playerSide == null) { Debug.LogError("Invalid playerSide (Null)"); }

        #region TopicTypes and TopicSubTypes
        //
        // - - - TopicTypes and TopicSubTypes
        //
        TopicType[] arrayOfTopicTypes = GameManager.instance.loadScript.arrayOfTopicTypes;
        if (arrayOfTopicTypes != null)
        {
            TopicSubType[] arrayOfTopicSubTypes = GameManager.instance.loadScript.arrayOfTopicSubTypes;
            if (arrayOfTopicSubTypes != null)
            {
                List<TopicSubType> listOfSubTypes = new List<TopicSubType>();
                count = arrayOfTopicTypes.Length;
                //loop topicTypes
                for (int i = 0; i < count; i++)
                {
                    TopicType type = arrayOfTopicTypes[i];
                    if (type != null)
                    {
                        listOfSubTypes.Clear();
                        listOfSubTypes.AddRange(type.listOfSubTypes);
                        countSubType = arrayOfTopicSubTypes.Length;
                        //get all subTypes in arrayOfSubTypes that match the topicType and check that they present in their parent TopicType's listOfSubTypes
                        for (int j = 0; j < countSubType; j++)
                        {
                            TopicSubType subType = arrayOfTopicSubTypes[j];
                            if (subType != null)
                            {
                                if (subType.type.name.Equals(type.name, StringComparison.Ordinal) == true)
                                {
                                    if (listOfSubTypes.Remove(subType) == false)
                                    { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: topicSubType \"{0}\" not in topicType \"{1}\" listOfSubTopics{2}", subType.tag, type.tag, "\n"); }
                                    //check subType min interval is equal to or greater than parent type interval
                                    if (subType.minIntervalFactor < type.minIntervalFactor)
                                    {
                                        Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: topicSubType \"{0}\" has min interval ({1}) LESS THAN parent type interval \"{2}\" ({3}){4}", subType.name,
                                          subType.minIntervalFactor, type.name, type.minIntervalFactor, "\n");
                                    }
                                }
                            }
                            else { Debug.LogWarningFormat("Invalid topicSubType (Null) for topic \"{0}\"", type.name); }
                        }
                        if (listOfSubTypes.Count > 0)
                        {
                            //any remaining SubTypes in list must be duplicates
                            foreach (TopicSubType topicSubType in listOfSubTypes)
                            {
                                if (topicSubType != null)
                                {
                                    Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: topicSubType \"{0}\" in list but not present in arrayOfTopicSubTypes (Duplicate or Mismatch)",
                                      topicSubType.tag, "\n");
                                }
                                else { Debug.LogWarningFormat("Invalid topicSubType (Null) for Topic \"{0}\"", type.name); }
                            }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid TopicType (Null) in listOfTopicTypes[{0}]", i); }
                }
            }
            else { Debug.LogError("Invalid arrayOfTopicSubTypes (Null)"); }
        }
        else { Debug.LogError("Invalid arrayOfTopicTypes (Null)"); }
        #endregion

        #region Topics and Topic Options
        //
        // - - - Topics and TopicOptions
        //
        Topic[] arrayOfTopics = GameManager.instance.loadScript.arrayOfTopics;
        TopicOption[] arrayOfTopicOptions = GameManager.instance.loadScript.arrayOfTopicOptions;
        List<string> listOfMoodEffects = new List<string>();
        Dictionary<string, int> dictOfBeliefs = GameManager.instance.dataScript.GetDictOfBeliefs();
        int maxOptions = GameManager.instance.topicScript.maxOptions;
        if (arrayOfTopics != null)
        {
            if (arrayOfTopicOptions != null)
            {
                if (dictOfBeliefs != null)
                {
                    //create a list of option names that you can progressively delete from to check if any are left over at the end
                    List<string> listOfOptionNames = arrayOfTopicOptions.Select(x => x.name).ToList();
                    string topicName;
                    //loop topics
                    for (int i = 0; i < arrayOfTopics.Length; i++)
                    {
                        Topic topic = arrayOfTopics[i];
                        if (topic != null)
                        {
                            topicName = topic.name;
                            //check topic text chars limit
                            textLength = topic.text.Length;
                            if (textLength > maxTopicTextLength)
                            {
                                Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: Text is overlength (is {0} chars, should be <= {1}) for topic \"{2}\"{3}",
                                  textLength, maxTopicTextLength, topicName, "\n");
                            }
                            //check topic text tags
                            GameManager.instance.topicScript.CheckTopicText(topic.text, false, true, topicName);
                            //check topic disabled
                            if (topic.isDisabled == true)
                            { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: topic \"{0}\" is Disabled{1}", topic.name, "\n"); }
                            //listOfOptions
                            if (topic.listOfOptions != null)
                            {
                                //clear out temp list prior to new set of options
                                listOfMoodEffects.Clear();
                                count = topic.listOfOptions.Count();
                                if (count > 0)
                                {
                                    //max number of options not exceeded
                                    if (count > maxOptions)
                                    { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: topic \"{0}\" has more options that allowed (has {1}, max is {2}){3}", topicName, count, maxOptions, "\n"); }
                                    isPassedCheck = false;
                                    //loop options and check that they aren't null and have the correct topic name
                                    foreach (TopicOption option in topic.listOfOptions)
                                    {
                                        //Check parent topic matches
                                        if (option.topic.name.Equals(topicName, StringComparison.Ordinal) == false)
                                        {
                                            Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: option \"{0}\" for topic \"{1}\" has a mismatching topic (\"{2}\"){3}",
                                              option.name, topicName, option.topic.name, "\n");
                                        }
                                        //
                                        // - - - Checks for completed TopicOptions (text field has data) -> DEBUG (shouldn't be any of these on completion)
                                        //
                                        //check text tags
                                        if (string.IsNullOrEmpty(option.text) == false)
                                        { GameManager.instance.topicScript.CheckTopicText(option.text, false, true, option.name); }
                                        //text
                                        if (option.text != null && option.text.Length > 0)
                                        {
                                            textLength = option.text.Length;
                                            if (textLength > 0)
                                            {
                                                //check text length limit
                                                if (textLength > maxOptionTextLength)
                                                {
                                                    Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: Text is overlength (is {0} chars, limit {1} chars) for option \"{2}\", topic {3}{4}",
                                                        textLength, maxOptionTextLength, option.name, topic.name, "\n");
                                                }
                                                //check there is at least one good or bad effect present
                                                if (option.listOfGoodEffects.Count == 0 && option.listOfBadEffects.Count == 0)
                                                { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: No effects present, Good or Bad, for option \"{0}\", topic {1}{2}", option.name, topicName, "\n"); }
                                                else
                                                {
                                                    //effects -> Good
                                                    if (option.listOfGoodEffects.Count > 0)
                                                    {
                                                        for (int j = 0; j < option.listOfGoodEffects.Count; j++)
                                                        {
                                                            Effect effect = option.listOfGoodEffects[j];
                                                            if (effect != null)
                                                            {
                                                                //check it's a good effect on the good list
                                                                if (effect.typeOfEffect.name.Equals("Good", StringComparison.Ordinal) == false)
                                                                {
                                                                    Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: Invalid Effect type (is {0} should be Good), effect \"{1}\", option {2}, topic {3}{4}",
                                                                        effect.typeOfEffect.name, effect.name, option.name, topicName, "\n");
                                                                }
                                                            }
                                                            else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: Invalid effect (Null) in listOfGoodEffects for option \"{0}\", topic {1}{2}", option.name, topicName, "\n"); }
                                                        }
                                                    }
                                                    //effects -> Bad
                                                    if (option.listOfBadEffects.Count > 0)
                                                    {
                                                        for (int j = 0; j < option.listOfBadEffects.Count; j++)
                                                        {
                                                            Effect effect = option.listOfBadEffects[j];
                                                            if (effect != null)
                                                            {
                                                                //check it's a bad effect on the bad list
                                                                if (effect.typeOfEffect.name.Equals("Bad", StringComparison.Ordinal) == false)
                                                                {
                                                                    Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: Invalid Effect type (is {0} should be Bad), effect \"{1}\", option {2}, topic {3}{4}",
                                                                        effect.typeOfEffect.name, effect.name, option.name, topicName, "\n");
                                                                }
                                                            }
                                                            else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: Invalid effect (Null) in listOfBadEffects for option \"{0}\", topic {1}{2}", option.name, topicName, "\n"); }
                                                        }
                                                    }
                                                }
                                            }
                                            //Mood effect (must be a personality effect, if present)
                                            if (option.moodEffect != null)
                                            {
                                                if (option.moodEffect.isMoodEffect == false)
                                                {
                                                    Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: Invalid Mood Effect (NOT isMoodEffect True), effect \"{1}\", option {2}, topic {3}{4}",
                                                        option.moodEffect.typeOfEffect.name, option.moodEffect.name, option.name, topicName, "\n");
                                                }

                                                listOfMoodEffects.Add(option.moodEffect.name);
                                                //tally beliefs (only if mood is valid for option)
                                                if (dictOfBeliefs.ContainsKey(option.moodEffect.name) == true)
                                                {
                                                    if (option.isIgnoreMood == false)
                                                    {
                                                        num = dictOfBeliefs[option.moodEffect.name] + 1;
                                                        dictOfBeliefs[option.moodEffect.name] = num;
                                                    }
                                                }
                                                else { Debug.LogWarningFormat("Invalid moodEffect \"{0}\", (Not found in dictOfBeliefs)", option.moodEffect.name); }

                                            }
                                            else
                                            {
                                                if (option.isIgnoreMood == false)
                                                { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: option \"{0}\" for topic \"{1}\" has missing Mood effect{2}", option.name, topicName, "\n"); }
                                            }
                                            //News snippet (check text tags are valid), only if a news snippet is present
                                            if (string.IsNullOrEmpty(option.news) == false)
                                            {
                                                GameManager.instance.topicScript.CheckTopicText(option.news, false, true, option.name);
                                                //check max length
                                                if (option.news.Length > maxTopicTextLength)
                                                {
                                                    Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: Option NEWS text is overlength (is {0} chars, should be <= {1}) for option \"{2}\", topic {3}{4}",
                                                      option.news.Length, maxTopicTextLength, option.name, topicName, "\n");
                                                }
                                            }
                                        }
                                        //delete option from list
                                        if (listOfOptionNames.Remove(option.name) == false)
                                        { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: option \"{0}\" for topic \"{1}\" not found in arrayOfTopicOptions{2}", option.name, topicName, "\n"); }
                                        //check at least one option has NO criteria (so topic always has at least one option available)
                                        if (option.listOfCriteria.Count == 0)
                                        { isPassedCheck = true; }
                                    }
                                    //at least one option with No criteria present
                                    if (isPassedCheck == false && topic.subType.name.Equals("PlayerGeneral", StringComparison.Ordinal) == false)
                                    { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: topic \"{0}\" has NO options without CRITERIA (should be at least one){1}", topicName, "\n"); }

                                    //check option moodEffects
                                    if (listOfMoodEffects.Count > 0)
                                    {
                                        if (topic.subType.name.Equals("PlayerGeneral", StringComparison.Ordinal) == false)
                                        {
                                            //no duplicates present
                                            CheckListForDuplicates(listOfMoodEffects, topic.name, "moodEffect", "AllMoodEffects");
                                        }
                                        else
                                        {
                                            //all should be the same -> Player General topic
                                            CheckListForSame(listOfMoodEffects, topic.name, "moodEffect", "AllMoodEffects");
                                        }
                                    }

                                }
                                else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: topic \"{0}\" has an Empty listOfOptions{1}", topicName, "\n"); }
                            }
                            else { Debug.LogWarningFormat("Invalid listOfOptions (Null) for topic \"{0}\"", topic.name); }

                            //subType matches type
                            TopicType topicType = GameManager.instance.dataScript.GetTopicType(topic.type.name);
                            if (topicType != null)
                            {
                                //check topic subType is on the listOfSubTypes
                                if (topicType.listOfSubTypes.Find(x => x.name.Equals(topic.subType.name, StringComparison.Ordinal)) == false)
                                { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: topic \"{0}\" has a mismatch with it's subType \"{1}\"{2}", topicName, topic.subType.name, "\n"); }
                            }
                            else { Debug.LogErrorFormat("Invalid topicType (Null) for topic.type.name \"{0}\"", topic.type.name); }

                        }
                        else { Debug.LogWarningFormat("Invalid topic (Null) for arrayOfTopics[{0}]", i); }
                    }
                    //unused topicOptions
                    if (listOfOptionNames.Count > 0)
                    {
                        foreach (string optionName in listOfOptionNames)
                        { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: option \"{0}\" NOT part of any Topic's listOfOptions{1}", optionName, "\n"); }
                    }
                }
                else { Debug.LogError("Invalid dictOfBeliefs (Null)"); }
            }
            else { Debug.LogError("Invalid arrayOfTopicOptions (Null)"); }
        }
        else { Debug.LogError("Invalid arrayOfTopics (Null)"); }
        #endregion

        #region Topic Profiles
        /*TopicProfile[] arrayOfProfiles = GameManager.instance.loadScript.arrayOfTopicProfiles; [EDIT: Redundant code as all timers/delays are now factors of the GlobalMinInterval
        if (arrayOfProfiles != null)
        {
            //get TopicManager.cs global minimum Interval
            int minIntervalGlobal = GameManager.instance.topicScript.minIntervalGlobal;
            Debug.Assert(minIntervalGlobal > 0, "Invalid minIntervalGlobal (Zero, should be at least One)");
            //loop profiles
            for (int i = 0; i < arrayOfProfiles.Length; i++)
            {
                TopicProfile profile = arrayOfProfiles[i];
                if (profile != null)
                {
                    //delay Repeat (if non Zero)
                    if (profile.delayRepeat > 0)
                    {
                        //repeat delay should be > global
                        if (profile.delayRepeat <= minIntervalGlobal)
                        {
                            Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: Invalid profile \"{0}\" delayRepeat (is {1}, should be > {2}, the minIntervalGlobal){3}",
                              profile.name, profile.delayRepeat, minIntervalGlobal, "\n");
                        }
                    }
                    //delay Start (if non Zero)
                    if (profile.delayStart > 0)
                    {
                        //start delay should be > global
                        if (profile.delayStart <= minIntervalGlobal)
                        {
                            Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: Invalid profile \"{0}\" delayStart (is {1}, should be > {2}, the minIntervalGlobal){3}",
                              profile.name, profile.delayStart, minIntervalGlobal, "\n");
                        }
                    }
                    //Timer Window (if non Zero)
                    if (profile.timerWindow > 0)
                    {
                        //timer Window should be > global
                        if (profile.timerWindow <= minIntervalGlobal)
                        {
                            Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: Invalid profile \"{0}\" timerWindow (is {1}, should be > {2}, the minIntervalGlobal){3}",
                              profile.name, profile.timerWindow, minIntervalGlobal, "\n");
                        }
                    }
                }
                else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: Invalid topicProfile (Null) for arrayOfTopicProfiles[{0}]{1}", i, "\n"); }
            }
        }
        else { Debug.LogError("Invalid arrayOfTopicProfiles (Null)"); }*/
        #endregion

        #region Topic Pools
        //
        // - - - Topic Pools
        //
        TopicPool[] arrayOfTopicPools = GameManager.instance.loadScript.arrayOfTopicPools;
        if (arrayOfTopicPools != null)
        {
            string topicName, topicSubName;
            count = arrayOfTopicPools.Length;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    TopicPool pool = arrayOfTopicPools[i];
                    topicName = pool.type.name;
                    topicSubName = pool.subType.name;
                    if (pool.listOfTopics.Count > 0)
                    {
                        foreach (Topic topic in pool.listOfTopics)
                        {
                            //Check each topic has the correct TopicType/SubType to match the TopicPool
                            if (topic.type.name.Equals(topicName, StringComparison.Ordinal) == false)
                            {
                                Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: topic \"{0}\", has incorrect Type (is {1} , should be {2}) for pool \"{3}\"{4}",
                                  topic.name, topic.type.name, topicName, pool.name, "\n");
                            }
                            if (topic.subType.name.Equals(topicSubName, StringComparison.Ordinal) == false)
                            {
                                Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: topic \"{0}\", has incorrect SubType (is {1} , should be {2}) for pool \"{3}\"{4}",
                                  topic.name, topicSubName, topic.subType.name, pool.name, "\n");
                            }
                            //check each topic has the correct side for the pool
                            switch (pool.subType.side.level)
                            {
                                case 1:
                                    //Authority
                                    if (topic.side.level != 1)
                                    {
                                        Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: topic \"{0}\", has incorrect Side (is {1} , should be {2}) for pool \"{3}\"{4}",
                                            topic.name, topic.side.name, pool.subType.side.name, pool.name, "\n");
                                    }
                                    break;
                                case 2:
                                    //Resistance
                                    if (topic.side.level != 2)
                                    {
                                        Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: topic \"{0}\", has incorrect Side (is {1} , should be {2}) for pool \"{3}\"{4}",
                                            topic.name, topic.side.name, pool.subType.side.name, pool.name, "\n");
                                    }
                                    break;
                                case 3:
                                    //Both (can be either resistance or authority)
                                    if (topic.side.level != 1 && topic.side.level != 2)
                                    {
                                        Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: topic \"{0}\", has incorrect Side (is {1} , should be Resistance or Authority) for pool \"{2}\"{3}",
                                            topic.name, topic.side.name, pool.name, "\n");
                                    }
                                    break;
                                default:
                                    Debug.LogWarningFormat("Unrecognised subType.side \"{0}\" for topicPool {1}", pool.subType.side.name, pool.name);
                                    break;

                            }
                        }
                        //check for duplicates
                        List<string> tempList = pool.listOfTopics.Select(x => x.name).ToList();
                        CheckListForDuplicates(tempList, pool.name, "listOfTopics", "topic.name");
                    }
                    else
                    {
                        //O.K to have empty listOfTopics if a subType placeholder topic that will be filled with SubSubType pool topics during UpdateTopicManager.cs
                        if (pool.subType.listOfSubSubType.Count == 0)
                        { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: topicPool \"{0}\", has no listOfTopics{1}", pool.name, "\n"); }
                    }
                }
            }
            else { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: arrayOfTopicPools  has No Pools (Empty){1}", "\n"); }
        }
        else { Debug.LogError("Invalid arrayOfTopicPools (Null)"); }
        #endregion

        #region Campaign Topics and Others
        //
        // - - - Campaigns (Topic Pools and Scenario sides
        //
        Campaign[] arrayOfCampaigns = GameManager.instance.loadScript.arrayOfCampaigns;
        if (arrayOfCampaigns != null)
        {
            count = arrayOfCampaigns.Length;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    Campaign campaign = arrayOfCampaigns[i];
                    if (campaign != null)
                    {
                        //campaign Alpha Pool
                        if (campaign.campaignAlphaPool != null)
                        {
                            //check pool (correct topicType / topicSubType / topics correct side)
                            CheckCampaignPool(campaign, campaign.campaignAlphaPool, campaignAlphaSubType);
                        }
                        //campaign Bravo Pool
                        if (campaign.campaignBravoPool != null)
                        { CheckCampaignPool(campaign, campaign.campaignBravoPool, campaignBravoSubType); }
                        //campaign Charlie Pool
                        if (campaign.campaignCharliePool != null)
                        { CheckCampaignPool(campaign, campaign.campaignCharliePool, campaignCharlieSubType); }
                        //family Alpha Pool
                        if (campaign.familyAlphaPool != null)
                        { CheckCampaignPool(campaign, campaign.familyAlphaPool, familyAlphaSubType); }
                        //family Bravo Pool
                        if (campaign.familyBravoPool != null)
                        { CheckCampaignPool(campaign, campaign.familyBravoPool, familyBravoSubType); }
                        //family Charlie Pool
                        if (campaign.familyCharliePool != null)
                        { CheckCampaignPool(campaign, campaign.familyCharliePool, familyCharlieSubType); }
                        //Authority Campaign Pool
                        if (campaign.authorityCampaignPool != null)
                        { CheckCampaignPool(campaign, campaign.authorityCampaignPool, authorityCampaignSubType); }
                        //Resistance Campaign Pool
                        if (campaign.resistanceCampaignPool != null)
                        { CheckCampaignPool(campaign, campaign.resistanceCampaignPool, resistanceCampaignSubType); }
                        //Authority General Pool
                        if (campaign.authorityGeneralPool != null)
                        { CheckCampaignPool(campaign, campaign.authorityGeneralPool, authorityGeneralSubType); }
                        //Authority Team Pool
                        if (campaign.teamPool != null)
                        { CheckCampaignPool(campaign, campaign.teamPool, authorityTeamSubType); }
                        //Resistance General Pool
                        if (campaign.resistanceGeneralPool != null)
                        { CheckCampaignPool(campaign, campaign.resistanceGeneralPool, resistanceGeneralSubType); }
                        //HQ Pool
                        if (campaign.hqPool != null)
                        { CheckCampaignPool(campaign, campaign.hqPool, hqSubType); }
                        //Capture Pool
                        if (campaign.capturePool != null)
                        { CheckCampaignPool(campaign, campaign.capturePool, captureSubType); }
                        //Actor Contact Pool
                        if (campaign.actorContactPool != null)
                        { CheckCampaignPool(campaign, campaign.actorContactPool, actorContactSubType); }
                        //Actor District Pool
                        if (campaign.actorDistrictPool != null)
                        { CheckCampaignPool(campaign, campaign.actorDistrictPool, actorDistrictSubType); }
                        //Actor Gear Pool
                        if (campaign.actorGearPool != null)
                        { CheckCampaignPool(campaign, campaign.actorGearPool, actorGearSubType); }
                        //Actor Match Pool
                        if (campaign.actorMatchPool != null)
                        { CheckCampaignPool(campaign, campaign.actorMatchPool, actorMatchSubType); }
                        //Actor Politic Pool
                        if (campaign.actorPoliticPool != null)
                        { CheckCampaignPool(campaign, campaign.actorPoliticPool, actorPoliticSubType); }
                        //Player District Pool
                        if (campaign.playerDistrictPool != null)
                        { CheckCampaignPool(campaign, campaign.playerDistrictPool, playerDistrictSubType); }
                        //Player General Pool
                        if (campaign.playerGeneralPool != null)
                        { CheckCampaignPool(campaign, campaign.playerGeneralPool, playerGeneralSubType); }
                        //Player Stats Pool
                        if (campaign.playerStatsPool != null)
                        { CheckCampaignPool(campaign, campaign.playerStatsPool, playerStatsSubType); }
                        //Player Conditions Pool
                        if (campaign.playerConditionsPool != null)
                        { CheckCampaignPool(campaign, campaign.playerConditionsPool, playerConditionsSubType); }
                        //OrgCure Pool
                        if (campaign.orgCurePool != null)
                        { CheckCampaignPool(campaign, campaign.orgCurePool, orgCureSubType); }
                        //OrgContract Pool
                        if (campaign.orgContractPool != null)
                        { CheckCampaignPool(campaign, campaign.orgContractPool, orgContractSubType); }
                        //OrgHQ Pool
                        if (campaign.orgHQPool != null)
                        { CheckCampaignPool(campaign, campaign.orgHQPool, orgHQSubType); }
                        //OrgEmergency Pool
                        if (campaign.orgEmergencyPool != null)
                        { CheckCampaignPool(campaign, campaign.orgEmergencyPool, orgEmergencySubType); }
                        //OrgInfo Pool
                        if (campaign.orgInfoPool != null)
                        { CheckCampaignPool(campaign, campaign.orgInfoPool, orgInfoSubType); }
                        //
                        // - - - Scenario side and City pools (topics in pool correct side check only)
                        //
                        //need to do here as only way to get access, via scenario, as cities can be in multiple campaigns)
                        if (campaign.listOfScenarios != null)
                        {
                            int countScenarios = campaign.listOfScenarios.Count;
                            if (countScenarios > 0)
                            {
                                //loop scenarios
                                for (int j = 0; j < countScenarios; j++)
                                {
                                    Scenario scenario = campaign.listOfScenarios[j];
                                    if (scenario != null)
                                    {
                                        //check scenario the same side as campaign
                                        if (scenario.side.level != campaign.side.level)
                                        {
                                            Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: campaign \"{0}\", scenario \"{1}\" has invalid SIDE (is {2}, should be {3}){4}",
                                                campaign.name, scenario.name, scenario.side.name, campaign.side.name, "\n");
                                        }
                                        //get city
                                        if (scenario.city != null)
                                        {
                                            TopicPool pool = null;
                                            //get city topic pool
                                            switch (campaign.side.level)
                                            {
                                                case 1: pool = scenario.city.cityPoolAuthority; break;
                                                case 2: pool = scenario.city.cityPoolResistance; break;
                                                default: Debug.LogWarningFormat("Unrecognised campaign.side \"{0}\"", campaign.side.name); break;
                                            }
                                            if (pool != null)
                                            {
                                                if (pool.listOfTopics != null)
                                                {
                                                    if (pool.listOfTopics.Count > 0)
                                                    {
                                                        //loop topics in pool
                                                        for (int k = 0; k < pool.listOfTopics.Count; k++)
                                                        {
                                                            Topic topic = pool.listOfTopics[k];
                                                            if (topic != null)
                                                            {
                                                                //check topic side same as campaign side
                                                                if (topic.side.level != campaign.side.level)
                                                                {
                                                                    Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: campaign \"{0}\", scenario \"{1}\", city \"{2}\", pool \"{3}\", topic \"{4}\", Invalid side (is {5}, should be {6}){7}",
                                                                        campaign.name, scenario.name, scenario.city.name, pool.name, topic.name, topic.side.name, campaign.side.name, "\n");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: campaign \"{0}\", scenario \"{1}\", city \"{2}\", pool \"{3}\", listOfTopics[{4}], Invalid (Null){5}",
                                                             campaign.name, scenario.name, scenario.city.name, pool.name, k, "\n");
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: campaign \"{0}\", scenario \"{1}\", Invalid city Pool (Empty){2}",
                                                     campaign.name, scenario.name, "\n");
                                                    }
                                                }
                                                else
                                                {
                                                    Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: campaign \"{0}\", scenario \"{1}\", city \"{2}\", pool \"{3}\", Invalid (Null){4}",
                                                 campaign.name, scenario.name, scenario.city.name, pool.name, "\n");
                                                }
                                            }
                                            else
                                            {
                                                Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: campaign \"{0}\", scenario \"{1}\", city \"{2}\", Invalid pool (Null){3}",
                                             campaign.name, scenario.name, scenario.city.name, "\n");
                                            }
                                        }
                                        else { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: campaign \"{0}\", scenario \"{1}\", Invalid city (Null){2}", campaign.name, scenario.name, "\n"); }
                                    }
                                    else { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: campaign \"{0}\" listOfScenarios[{1}] Invalid (Null){2}", campaign.name, j, "\n"); }
                                }
                            }
                            else { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: campaign \"{0}\" Invalid listOfScenarios (Empty){1}", campaign.name, "\n"); }
                        }
                        else { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: campaign \"{0}\" listOfScenarios Invalid (Null){1}", campaign.name, "\n"); }
                    }
                    else { Debug.LogErrorFormat("Invalid campaign in arrayOfCampaigns[{0}]", i); }
                }
            }
            else { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: arrayOfCampaigns is EMPTY{0}", "\n"); }
        }
        else { Debug.LogError("Invalid arrayOfCampaigns (Null)"); }
        #endregion

        #region City Topic Pools
        City[] arrayOfCities = GameManager.instance.loadScript.arrayOfCities;
        if (arrayOfCities != null)
        {
            count = arrayOfCities.Length;
            if (count > 0)
            {
                //loop cities
                for (int i = 0; i < count; i++)
                {
                    City city = arrayOfCities[i];
                    if (city != null)
                    {
                        //City Pool Authority
                        if (city.cityPoolAuthority != null)
                        {
                            //check type for a match
                            if (city.cityPoolAuthority.type.name.Equals(cityType.name, StringComparison.Ordinal) == false)
                            {
                                Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: City \"{0}\", cityPool \"{1}\", has incorrect type ({2} should be {3}){4}", city.name,
                                    city.cityPoolAuthority.name, city.cityPoolAuthority.type.name, cityType.name, "\n");
                            }
                            //check subType for a match
                            if (city.cityPoolAuthority.subType.name.Equals(citySubType.name, StringComparison.Ordinal) == false)
                            {
                                Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: city \"{0}\", cityPool \"{1}\", has incorrect subType ({2} should be {3}){4}", city.name,
                                    city.cityPoolAuthority.name, city.cityPoolAuthority.subType.name, citySubType.name, "\n");
                            }
                        }
                        //City Pool Resistance
                        if (city.cityPoolResistance != null)
                        {
                            //check type for a match
                            if (city.cityPoolResistance.type.name.Equals(cityType.name, StringComparison.Ordinal) == false)
                            {
                                Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: City \"{0}\", cityPool \"{1}\", has incorrect type ({2} should be {3}){4}", city.name,
                                    city.cityPoolResistance.name, city.cityPoolResistance.type.name, cityType.name, "\n");
                            }
                            //check subType for a match
                            if (city.cityPoolResistance.subType.name.Equals(citySubType.name, StringComparison.Ordinal) == false)
                            {
                                Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: city \"{0}\", cityPool \"{1}\", has incorrect subType ({2} should be {3}){4}", city.name,
                                    city.cityPoolResistance.name, city.cityPoolResistance.subType.name, citySubType.name, "\n");
                            }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid city (Null) in arrayOfCities[{0}]"); }
                }
            }
            else { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: arrayOfCities is EMPTY{0}", "\n"); }
        }
        else { Debug.LogError("Invalid arrayOfCities (Null)"); }
        #endregion
    }
    #endregion

    #region ValidateScenarios
    /// <summary>
    /// runs checks on scenarios, mainly side specific data that can't be done in Scenario.SO OnEnable
    /// </summary>
    private void ValidateScenarios()
    {
        Scenario[] arrayOfScenarios = GameManager.instance.loadScript.arrayOfScenarios;
        if (arrayOfScenarios != null)
        {
            for (int i = 0; i < arrayOfScenarios.Length; i++)
            {
                Scenario scenario = arrayOfScenarios[i];
                if (scenario != null)
                {
                    //side specific data
                    switch (scenario.side.level)
                    {
                        case 1:
                            //Authority
                            if (scenario.missionAuthority == null)
                            { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateScenarios: scenario \"{0}\" has Invalid missionAuthority (Null){1}", scenario.name, "\n"); }
                            else
                            {
                                //check mission side same as scenario
                                if (scenario.missionAuthority.side.level != scenario.side.level)
                                {
                                    Debug.LogFormat("[Val] ValidationManager.cs -> ValidateScenarios: scenario \"{0}\", Authority mission \"{1}\", Invalid Side (is {2}, should be {3}){4}",
                                        scenario.name, scenario.missionAuthority.name, scenario.missionAuthority.side.name, scenario.side.name, "\n");
                                }
                            }
                            /*if (scenario.challengeAuthority == null)
                            { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateScenarios: scenario \"{0}\" has Invalid challengeAuthority (Null){1}", scenario.name, "\n"); }*/
                            if (string.IsNullOrEmpty(scenario.descriptorAuthority) == true)
                            { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateScenarios: scenario \"{0}\" has Invalid descriptorAuthority (Null or Empty){1}", scenario.name, "\n"); }
                            break;
                        case 2:
                            //Resistance
                            if (scenario.missionResistance == null)
                            { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateScenarios: scenario \"{0}\" has Invalid missionResistance (Null){1}", scenario.name, "\n"); }
                            else
                            {
                                //check mission side same as scenario
                                if (scenario.missionResistance.side.level != scenario.side.level)
                                {
                                    Debug.LogFormat("[Val] ValidationManager.cs -> ValidateScenarios: scenario \"{0}\", Resistance mission \"{1}\", Invalid Side (is {2}, should be {3}){4}",
                                        scenario.name, scenario.missionResistance.name, scenario.missionResistance.side.name, scenario.side.name, "\n");
                                }
                            }
                            if (scenario.challengeResistance == null)
                            { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateScenarios: scenario \"{0}\" has Invalid challengeResistance (Null){1}", scenario.name, "\n"); }
                            if (string.IsNullOrEmpty(scenario.descriptorResistance) == true)
                            { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateScenarios: scenario \"{0}\" has Invalid descriptorResistance (Null or Empty){1}", scenario.name, "\n"); }
                            break;
                        default:
                            Debug.LogWarningFormat("Unrecognised playerSide \"{0}\" for scenario \"{1}\"", scenario.side.name, scenario.name);
                            break;
                    }
                }
                else { Debug.LogWarningFormat("Invalid scenario (Null) in arrayOfScenarios[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid arrayOfScenarios (Null)"); }
    }

    #endregion

    #region ValidateCampaigns
    //runs checks on campaigns
    private void ValidateCampaigns()
    {
        Campaign[] arrayOfCampaigns = GameManager.instance.loadScript.arrayOfCampaigns;
        if (arrayOfCampaigns != null)
        {
            //Check organisations are the correct type
            foreach(Campaign campaign in arrayOfCampaigns)
            {
                //cure
                if (campaign.orgCure != null)
                {
                    if (campaign.orgCure.orgType.name.Equals("Cure", StringComparison.Ordinal) == false)
                    { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateCampaigns: campaign \"{0}\", orgCure is the wrong type (\"{1}\"){2}", campaign.name, campaign.orgCure.orgType.name, "\n"); }
                }
                //contract
                if (campaign.orgContract != null)
                {
                    if (campaign.orgContract.orgType.name.Equals("Contract", StringComparison.Ordinal) == false)
                    { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateCampaigns: campaign \"{0}\", orgContract is the wrong type (\"{1}\"){2}", campaign.name, campaign.orgContract.orgType.name, "\n"); }
                }
                //Emergency
                if (campaign.orgEmergency != null)
                {
                    if (campaign.orgEmergency.orgType.name.Equals("Emergency", StringComparison.Ordinal) == false)
                    { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateCampaigns: campaign \"{0}\", orgEmergency is the wrong type (\"{1}\"){2}", campaign.name, campaign.orgEmergency.orgType.name, "\n"); }
                }
                //HQ
                if (campaign.orgHQ != null)
                {
                    if (campaign.orgHQ.orgType.name.Equals("HQ", StringComparison.Ordinal) == false)
                    { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateCampaigns: campaign \"{0}\", orgHQ is the wrong type (\"{1}\"){2}", campaign.name, campaign.orgHQ.orgType.name, "\n"); }
                }
                //Info
                if (campaign.orgInfo != null)
                {
                    if (campaign.orgInfo.orgType.name.Equals("Info", StringComparison.Ordinal) == false)
                    { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateCampaigns: campaign \"{0}\", orgInfo is the wrong type (\"{1}\"){2}", campaign.name, campaign.orgInfo.orgType.name, "\n"); }
                }
            }
        }
        else { Debug.LogError("Invalid arrayOfCampaigns (Null)"); }
    }
    #endregion

    #region ValidateCapture
    /// <summary>
    /// runs checks on CaptureManager.cs
    /// </summary>
    private void ValidateCapture()
    {
        //Capture Tools -> tool innocence level should match captureManager field level
        if (GameManager.instance.captureScript.innocence_0 != null)
        {
            if (GameManager.instance.captureScript.innocence_0.innocenceLevel != 0)
            {
                Debug.LogFormat("[Val] ValidationManager.cs -> ValidateCapture: captureTool innocence_0 \"{0}\" has incorrent innocence level (is {1}, should be 0){2}",
                    GameManager.instance.captureScript.innocence_0.name, GameManager.instance.captureScript.innocence_0.innocenceLevel, "\n");
            }
        }
        //Innocence level 1
        if (GameManager.instance.captureScript.innocence_1 != null)
        {
            if (GameManager.instance.captureScript.innocence_1.innocenceLevel != 1)
            {
                Debug.LogFormat("[Val] ValidationManager.cs -> ValidateCapture: captureTool innocence_1 \"{0}\" has incorrent innocence level (is {1}, should be 1){2}",
                    GameManager.instance.captureScript.innocence_1.name, GameManager.instance.captureScript.innocence_1.innocenceLevel, "\n");
            }
        }
        //Innocence level 2
        if (GameManager.instance.captureScript.innocence_2 != null)
        {
            if (GameManager.instance.captureScript.innocence_2.innocenceLevel != 2)
            {
                Debug.LogFormat("[Val] ValidationManager.cs -> ValidateCapture: captureTool innocence_2 \"{0}\" has incorrent innocence level (is {1}, should be 2){2}",
                    GameManager.instance.captureScript.innocence_2.name, GameManager.instance.captureScript.innocence_2.innocenceLevel, "\n");
            }
        }
        //Innocence level 3
        if (GameManager.instance.captureScript.innocence_3 != null)
        {
            if (GameManager.instance.captureScript.innocence_3.innocenceLevel != 3)
            {
                Debug.LogFormat("[Val] ValidationManager.cs -> ValidateCapture: captureTool innocence_3 \"{0}\" has incorrent innocence level (is {1}, should be 3){2}",
                    GameManager.instance.captureScript.innocence_3.name, GameManager.instance.captureScript.innocence_3.innocenceLevel, "\n");
            }
        }
    }
    #endregion


#if (UNITY_EDITOR)

    #region ValidateSO
    /// <summary>
    /// optional (GameManager.cs toggle) program to run to check SO's loaded in LoadManager.cs arrays vs. those found by an Asset search (editor only)
    /// Designed to pick up SO's that might have been added in the editor but not added to the arrays (ignored by game if this is the case).
    /// </summary>
    public void ValidateSO(GameState state)
    {
        //GlobalMeta
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfGlobalMeta);
        //GlobalChance
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfGlobalChance);
        //GlobalType
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfGlobalType);
        //GlobalSide
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfGlobalSide);
        //GlobalWho
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfGlobalWho);
        //Criteria
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfCriteria);
        //EffectApply
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfEffectApply);
        //EffectCriteria
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfEffectCriteria);
        //EffectDuration
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfEffectDuration);
        //EffectOperator
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfEffectOperator);
        //EffectOutcome
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfEffectOutcome);
        //ContactType
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfContactTypes);
        //TargetType
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTargetTypes);
        //TargetTrigger
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTargetTriggers);
        //TargetProfile
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTargetProfiles);
        //Quality
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfQualities);
        //Condition
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfConditions);
        //Cure
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfCures);
        //TraitCategory
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTraitCategories);
        //TraitEffect
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTraitEffects);
        //SecretType
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfSecretTypes);
        //CityArc
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfCityArcs);
        //City
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfCities);
        //CitySize
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfCitySize);
        //CitySpacing
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfCitySpacing);
        //CityConnections
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfCityConnections);
        //CitySecurity
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfCitySecurity);
        //Damage
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfDamages);
        //Challenge
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfChallenges);
        //Nemesis
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfNemesis);
        //Rebel Leaders
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfRebelLeaders);
        //Scenario
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfScenarios);
        //Campaign
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfCampaigns);
        //NameSet
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfNameSets);
        //NodeDatapoint
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfNodeDatapoints);
        //NodeArc
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfNodeArcs);
        //NodeCrisis
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfNodeCrisis);
        //Trait
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTraits);
        //ActorArc
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfActorArcs);
        //Effect
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfEffects);
        //Action
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfActions);
        //TeamArc
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTeamArcs);
        //Gear
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfGear);
        //GearRarity
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfGearRarity);
        //GearType
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfGearType);
        //TopicType
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTopicTypes);
        //TopicSubType
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTopicSubTypes);
        //TopicSubSubType
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTopicSubSubTypes);
        //TopicOption
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTopicOptions);
        //TopicPool
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTopicPools);
        //TopicScope
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTopicScopes);
        //TopicGroupType
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTopicGroupTypes);
        //Topics
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTopics);
        //TopicProfiles
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTopicProfiles);
        //ManageActor
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfManageActors);
        //ManageAction
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfManageActions);
        //ActorConflict
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfActorConflicts);
        //Secret
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfSecrets);
        //Faction
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfFactions);
        //Objective
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfObjectives);
        //Organisation
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfOrganisations);
        //OrgTypes
        ValidateSOGeneric<OrgType>(GameManager.instance.loadScript.arrayOfOrgTypes);
        //Mayor
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfMayors);
        //DecisionAI
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfDecisionAI);
        //Mission
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfMissions);
        //TextList
        ValidateSOGeneric<TextList>(GameManager.instance.loadScript.arrayOfTextLists);
        //Target
        ValidateSOGeneric<Target>(GameManager.instance.loadScript.arrayOfTargets);
        //VIP
        ValidateSOGeneric<Npc>(GameManager.instance.loadScript.arrayOfNpcs);
        //VIP Nodes
        ValidateSOGeneric<NpcNode>(GameManager.instance.loadScript.arrayOfNpcNodes);
        //VIP Actions
        ValidateSOGeneric<NpcAction>(GameManager.instance.loadScript.arrayOfNpcActions);
        //HqPositions
        ValidateSOGeneric<HqPosition>(GameManager.instance.loadScript.arrayOfHqPositions);
        //CaptureTools
        ValidateSOGeneric<CaptureTool>(GameManager.instance.loadScript.arrayOfCaptureTools);

    }
    #endregion

    #region ValidateSOGeneric
    /// <summary>
    /// Generic method to validate an SO type via an Asset Search on hard drive vs. the equivalent array in LoadManager.cs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arraySO"></param>
    private void ValidateSOGeneric<T>(T[] arraySO) where T : ScriptableObject
    {
        if (arraySO != null)
        {
            int numArray, numAssets;
            // 'isVerbal' is true for displaying ALL messages, false for problem messages only
            bool isVerbal = false;
            string path;
            // Validate SO
            var metaGUID = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T).Name), new[] { "Assets/SO" });
            numArray = arraySO.Length;
            numAssets = metaGUID.Length;
            if (numAssets != numArray)
            {
                Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on {0}, array {1}, assets {2} records", typeof(T).Name, numArray, numAssets);
                T[] arrayTemp = arraySO;
                foreach (var guid in metaGUID)
                {
                    //get path
                    path = AssetDatabase.GUIDToAssetPath(guid);
                    //get SO
                    UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(T));
                    //get object
                    T meta = metaObject as T;
                    if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name, StringComparison.Ordinal)) == false)
                    { Debug.LogFormat("[Val] ValidateSO: array MISSING {0} \"{1}\"", typeof(T).Name, meta.name); }
                }
            }
            else
            {
                if (isVerbal == true)
                { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on {0} SO, array {1}, assets {2} records", typeof(T).Name, numArray, numAssets); }
            }
        }
        else { Debug.LogWarningFormat("Invalid arraySO for {0}", typeof(T).Name); }
    }
    #endregion

#endif

    #region Data Integrity Checks
    //
    // - - - Integrity Check (Dynamic Data) - - -
    //

    #region ExecuteIntegrityCheck
    /// <summary>
    /// Master method to run all data Integrity checks
    /// </summary>
    public void ExecuteIntegrityCheck()
    {
        string prefix = "[Val] ValidationManager.cs -> ";
        //range limits
        int highestActorID = GameManager.instance.actorScript.actorIDCounter;
        int highestHQID = GameManager.instance.actorScript.hqIDCounter;
        int highestNodeID = GameManager.instance.nodeScript.nodeIDCounter;
        int highestConnID = GameManager.instance.nodeScript.connIDCounter;
        int highestSlotID = GameManager.instance.actorScript.maxNumOfOnMapActors - 1;
        int highestContactID = GameManager.instance.contactScript.contactIDCounter;
        int highestMessageID = GameManager.instance.messageScript.messageIDCounter;
        int highestTeamID = GameManager.instance.teamScript.teamIDCounter;
        int highestTurn = GameManager.instance.turnScript.Turn;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //debug checks
        Debug.Assert(highestActorID > 0, "Invalid highestActor (Zero or less)");
        Debug.Assert(highestNodeID > 0, "Invalid highestNodeID (Zero or less)");
        Debug.Assert(highestConnID > 0, "Invalid highestConnID (Zero or less)");
        Debug.Assert(highestSlotID > 0, "Invalid highestSlotID (Zero or less)");
        Debug.Assert(highestMessageID > 0, "Invalid highestMessageID (Zero or less)");
        Debug.Assert(highestContactID > 0, "Invalid highestContactID (Zero or less)");
        Debug.Assert(highestTeamID > 0, "Invalid highestTeamID (Zero or less)");
        Debug.Assert(playerSide != null, "Invalid playerSide (Null)");

        //run checks
        if (string.IsNullOrEmpty(prefix) == false)
        {
            Debug.LogFormat("{0}ExecuteIntegrityCheck: Commence checks...{1}", prefix, "\n");
            CheckNodeData(prefix, highestNodeID);
            CheckActorData(prefix, highestActorID, highestHQID, highestNodeID, highestSlotID);
            CheckTargetData(prefix, highestNodeID, highestContactID, highestTurn);
            CheckTeamData(prefix, highestNodeID, highestTeamID, highestSlotID, highestTurn, playerSide);
            CheckGearData(prefix);
            CheckConnectionData(prefix, highestNodeID, highestConnID);
            CheckMessageData(prefix, highestMessageID, highestTurn);
            CheckSecretData(prefix, highestActorID, highestTurn);
            CheckMainInfoData(prefix, highestTurn);
            CheckContactData(prefix, highestContactID, highestNodeID, highestActorID, highestTurn, playerSide);
            CheckPlayerData(prefix);
            CheckTopicData(prefix);
            CheckTextListData(prefix);
        }
    }
    #endregion

    #region CheckNodeData
    /// <summary>
    /// Integrity check on all node related data collections
    /// </summary>
    /// <param name="prefix"></param>
    private void CheckNodeData(string prefix, int highestNodeID)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckNodeData: ");

        int maxStatValue = GameManager.instance.nodeScript.maxNodeValue;
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfNodes
        //
        //check dictionaries all have the same number of entries
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetDictOfNodes();
        if (dictOfNodes != null)
        {
            //check count
            if (dictOfNodes.Count != highestNodeID)
            { Debug.LogFormat("{0}Incorrect count, dictOfNodes has {1} records, highestNode {2}{3}", tag, dictOfNodes.Count, highestNodeID, "\n"); }
            foreach (var node in dictOfNodes)
            {
                //null check record
                if (node.Value != null)
                {
                    key = node.Key.ToString();
                    //range checks
                    CheckDictRange(node.Key, 0, highestNodeID, "nodeID", tag, key);
                    CheckDictRange(node.Value.Stability, 0, maxStatValue, "Stability", tag, key);
                    CheckDictRange(node.Value.Support, 0, maxStatValue, "Support", tag, key);
                    CheckDictRange(node.Value.Security, 0, maxStatValue, "Security", tag, key);
                    //null checks
                    CheckDictString(node.Value.nodeName, "nodeName", tag, key);
                    CheckDictObject(node.Value._Material, "_Material", tag, key);
                    CheckDictObject(node.Value.gameObject, "gameObject", tag, key);
                    CheckDictObject(node.Value.Arc, "node.Arc", tag, key);
                    CheckDictObject(node.Value.launcher, "launcher", tag, key);
                    CheckDictObject(node.Value.nodePosition, "nodePosition", tag, key);
                    CheckDictObject(node.Value.loiter, "loiter", tag, key);
                    CheckDictList(node.Value.GetListOfTeams(), "dictOfNodes: listOfTeams", tag, key);
                    CheckDictList(node.Value.GetListOfOngoingEffects(), "dictOfNodes: listOfOngoingEffects", tag, key);
                    CheckDictList(node.Value.GetNeighbouringNodes(), "dictOfNodes: listOfNeighbouringNodes", tag, key);
                    CheckDictList(node.Value.GetNearNeighbours(), "dictOfNodes: listOfNearNeighbours", tag, key);
                    CheckDictList(node.Value.GetListOfConnections(), "dictOfNodes: listOfConnections", tag, key);
                    //duplicate checks
                    CheckListForDuplicates(node.Value.GetListOfTeamID(), string.Format("dictOfNodes: dictKey {0}", key), "TeamID", "listOfTeams");
                    CheckListForDuplicates(node.Value.GetListOfOngoingID(), string.Format("dictOfNodes: dictKey {0}", key), "OngoingID", "listOfOngoingEffects");
                }
                else { Debug.LogFormat("{0}Invalid entry (Null) in dictOfNodes for nodeID {1}{2}", node.Key, "\n"); }
            }
        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
        //
        // - - - dictOfNodeObjects
        //
        Dictionary<int, GameObject> dictOfNodeObjects = GameManager.instance.dataScript.GetDictOfNodeObjects();
        if (dictOfNodeObjects != null)
        {
            foreach (var node in dictOfNodeObjects)
            {
                key = node.Key.ToString();
                CheckDictRange(node.Key, 0, highestNodeID, "nodeID", tag, key);
                CheckDictObject(node.Value, "node", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfNodeObjects (Null)"); }
        //
        // - - - dictOfNodeArcs
        //
        Dictionary<int, NodeArc> dictOfNodeArcs = GameManager.instance.dataScript.GetDictOfNodeArcs();
        if (dictOfNodeArcs != null)
        {
            foreach (var arc in dictOfNodeArcs)
            {
                key = arc.Key.ToString();
                CheckDictRange(arc.Value.nodeArcID, 0, 99, "nodeArcID", tag, key);
                CheckDictObject(arc.Value.sprite, "sprite", tag, key);
                CheckDictArray(arc.Value.contactTypes, "arrayOfContactTypes", tag, key, true);
            }
        }
        else { Debug.LogError("Invalid dictOfNodeArcs (Null)"); }
        //
        // - - - dictOfNodeDUnweighted
        //
        Dictionary<int, NodeD> dictOfNodeUnweighted = GameManager.instance.dataScript.GetDictOfNodeDUnweighted();
        if (dictOfNodeUnweighted != null)
        {
            foreach (var nodeD in dictOfNodeUnweighted)
            {
                key = nodeD.Key.ToString();
                CheckDictRange(nodeD.Key, 0, highestNodeID, "nodeD.ID", tag, key);
                CheckDictObject(nodeD.Value, "nodeD", tag, key);
                CheckDictRange(nodeD.Value.Distance, 0, 99, "Distance", tag, key);
                CheckDictString(nodeD.Value.Name, "Name", tag, key);
                CheckDictList(nodeD.Value.Weights, "NodeDUnWeighted: weights", tag, key, false);
                CheckDictList(nodeD.Value.Adjacency, "NodeDUnWeighted: adjaceny", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfNodeUnweighted (Null)"); }
        //
        // - - - dictOfNodeDWeighted
        //
        Dictionary<int, NodeD> dictOfNodeWeighted = GameManager.instance.dataScript.GetDictOfNodeDWeighted();
        if (dictOfNodeWeighted != null)
        {
            foreach (var nodeD in dictOfNodeWeighted)
            {
                key = nodeD.Key.ToString();
                CheckDictRange(nodeD.Key, 0, highestNodeID, "nodeD.ID", tag, key);
                CheckDictObject(nodeD.Value, "nodeD", tag, key);
                CheckDictRange(nodeD.Value.Distance, 0, 99, "Distance", tag, key);
                CheckDictString(nodeD.Value.Name, "Name", tag, key);
                CheckDictList(nodeD.Value.Weights, "NodeDWeighted: weights", tag, key, false);
                CheckDictList(nodeD.Value.Adjacency, "NodeDWeighted: adjaceny", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfNodeWeighted (Null)"); }
        //
        // - - - Node lists
        //
        CheckList(GameManager.instance.dataScript.GetListOfAllNodes(), tag, "listOfNodes", highestNodeID);
        CheckList(GameManager.instance.dataScript.GetListOfMostConnectedNodes(), "listOfMostConnectedNodes", tag);
        CheckList(GameManager.instance.dataScript.GetListOfDecisionNodes(), "listOfDecisionNodes", tag);
        CheckList(GameManager.instance.dataScript.GetListOfLoiterNodes(), "listOfLoiterNodes", tag);
        CheckList(GameManager.instance.dataScript.GetListOfCureNodes(), "listOfCureNodes", tag);
        //duplicate checks
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfNodeID(), "Node", "nodeID", "listOfNodes");
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfMostConnectedNodeID(), "Node", "nodeID", "listOfMostConnectedNodes");
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfDecisionNodeID(), "Node", "nodeID", "listOfDecisionNodes");
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfLoiterNodeID(), "Node", "nodeID", "listOfLoiterNodes");
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfCureNodeID(), "Node", "nodeID", "listOfCureNodes");
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfCrisisNodeID(), "Node", "nodeID", "listOfCrisisNodes");
    }
    #endregion

    #region CheckActorData
    /// <summary>
    /// Integrity check on all actor related collections
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="highestActorID"></param>
    private void CheckActorData(string prefix, int highestActorID, int highestHQID, int highestNodeID, int highestSlotID)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckActorData: ");
        int maxNumOfOnMapActors = GameManager.instance.actorScript.maxNumOfOnMapActors;
        int maxStatValue = GameManager.instance.actorScript.maxStatValue;
        int maxCompatibility = GameManager.instance.personScript.maxCompatibilityWithPlayer;
        int minCompatibility = GameManager.instance.personScript.minCompatibilityWithPlayer;
        int maxFactor = GameManager.instance.personScript.maxPersonalityFactor;
        int minFactor = GameManager.instance.personScript.minPersonalityFactor;
        //
        // - - - dictOfActors
        //
        Dictionary<int, Actor> dictOfActors = GameManager.instance.dataScript.GetDictOfActors();
        if (dictOfActors != null)
        {
            foreach (var actor in dictOfActors)
            {
                key = actor.Key.ToString();
                CheckDictRange(actor.Key, 0, highestActorID, "actorID", tag, key);
                CheckDictRange(actor.Value.slotID, -1, highestSlotID, "slotID", tag, key);
                CheckDictRange(actor.Value.level, 1, 3, "level", tag, key);
                CheckDictString(actor.Value.actorName, "actorName", tag, key);
                CheckDictString(actor.Value.firstName, "firstName", tag, key);
                CheckDictString(actor.Value.spriteName, "spriteName", tag, key);
                CheckDictObject(actor.Value.side, "side", tag, key);
                CheckDictObject(actor.Value.arc, "arc", tag, key);
                CheckDictObject(actor.Value.sprite, "sprite", tag, key);
                CheckDictObject(actor.Value.GetTrait(), "trait", tag, key);
                CheckDictRange(actor.Value.GetDatapoint(ActorDatapoint.Datapoint0), 0, maxStatValue, "datapoint0", tag, key);
                CheckDictRange(actor.Value.GetDatapoint(ActorDatapoint.Datapoint1), 0, maxStatValue, "datapoint1", tag, key);
                CheckDictRange(actor.Value.GetDatapoint(ActorDatapoint.Datapoint2), 0, maxStatValue, "datapoint2", tag, key);
                CheckDictRange(actor.Value.Renown, 0, 99, "Renown", tag, key);
                CheckDictList(actor.Value.GetListOfTeams(), "listOfTeams", tag, key, false);
                CheckDictList(actor.Value.GetListOfSecrets(), "listOfSecrets", tag, key);
                CheckDictList(actor.Value.GetListOfConditions(), "listOfConditions", tag, key);
                CheckDictList(actor.Value.GetListOfTraitEffects(), "listOfTraitEffects", tag, key);
                CheckDictDict(actor.Value.GetDictOfContacts(), "dictOfContacts", tag, key);
                //sex
                if (actor.Value.sex == ActorSex.None)
                { Debug.LogFormat("{0}Invalid sex ('None') for actorID {1}{2}", tag, actor.Key, "\n"); }
                //personality
                Personality personality = actor.Value.GetPersonality();
                if (personality != null)
                {
                    CheckDictArrayBounds(personality.GetFactors(), "arrayOfFactors", tag, key, minFactor, maxFactor);
                    CheckDictRange(personality.GetCompatibilityWithPlayer(), minCompatibility, maxCompatibility, "compatibilityWithPlayer", tag, key);
                    if (personality.GetProfile() != null)
                    {
                        CheckDictString(personality.GetProfileDescriptor(), "profileDescriptor", tag, key);
                        CheckDictString(personality.GetProfileExplanation(), "profileExplanation", tag, key);
                        CheckList(personality.GetListOfDescriptors(), "personality.listOfDescriptors", tag);
                        CheckList(personality.GetListOfMotivation(), "personality.listOfMotivation", tag);
                    }
                }
                else { Debug.LogFormat("{0}Invalid personality (Null) for actorID {1}{2}", tag, actor.Key, "\n"); }
            }
        }
        else { Debug.LogError("Invalid dictOfActors (Null)"); }
        //
        // - - - dictOfHQ
        //
        Dictionary<int, Actor> dictOfHQ = GameManager.instance.dataScript.GetDictOfHQ();
        if (dictOfHQ != null)
        {
            foreach (var actor in dictOfHQ)
            {
                key = actor.Key.ToString();
                CheckDictRange(actor.Key, 0, highestHQID, "hqID", tag, key);
                CheckDictRange(actor.Value.level, 1, 3, "level", tag, key);
                CheckDictString(actor.Value.actorName, "actorName", tag, key);
                CheckDictString(actor.Value.firstName, "firstName", tag, key);
                CheckDictString(actor.Value.spriteName, "spriteName", tag, key);
                CheckDictObject(actor.Value.side, "side", tag, key);
                CheckDictObject(actor.Value.arc, "arc", tag, key);
                CheckDictObject(actor.Value.sprite, "sprite", tag, key);
                CheckDictObject(actor.Value.GetTrait(), "trait", tag, key);
                CheckDictRange(actor.Value.GetDatapoint(ActorDatapoint.Datapoint0), 0, maxStatValue, "datapoint0", tag, key);
                CheckDictRange(actor.Value.GetDatapoint(ActorDatapoint.Datapoint1), 0, maxStatValue, "datapoint1", tag, key);
                CheckDictRange(actor.Value.GetDatapoint(ActorDatapoint.Datapoint2), 0, maxStatValue, "datapoint2", tag, key);
                CheckDictRange(actor.Value.Renown, 0, 999, "Renown", tag, key);
                //personality
                Personality personality = actor.Value.GetPersonality();
                if (personality != null)
                {
                    CheckDictArrayBounds(personality.GetFactors(), "arrayOfFactors", tag, key, minFactor, maxFactor);
                    CheckDictRange(personality.GetCompatibilityWithPlayer(), minCompatibility, maxCompatibility, "compatibilityWithPlayer", tag, key);
                    if (personality.GetProfile() != null)
                    {
                        CheckDictString(personality.GetProfileDescriptor(), "profileDescriptor", tag, key);
                        CheckDictString(personality.GetProfileExplanation(), "profileExplanation", tag, key);
                        CheckList(personality.GetListOfDescriptors(), "personality.listOfDescriptors", tag);
                        CheckList(personality.GetListOfMotivation(), "personality.listOfMotivation", tag);
                    }
                }
                else { Debug.LogFormat("{0}Invalid personality (Null) for actorID {1}{2}", tag, actor.Key, "\n"); }
                //status
                if (actor.Value.Status != ActorStatus.HQ)
                {
                    Debug.LogFormat("{0}Invalid status \"{1}\" (should be HQ) for hqID {2}, actor {3}{4}",
tag, actor.Value.Status, actor.Value.hqID, actor.Value.actorName, "\n");
                }
                else if (actor.Value.statusHQ == ActorHQ.None)
                {
                    Debug.LogFormat("{0}Invalid HQ Status \"{1}\" (shouldn't be None) for hqID {2}, actor {3}{4}",
tag, actor.Value.statusHQ, actor.Value.hqID, actor.Value.actorName, "\n");
                }
            }
        }
        else { Debug.LogError("Invalid dictOfHQ (Null)"); }
        //
        // - - - Other Actor Collections
        //
        CheckList(GameManager.instance.dataScript.GetListOfActorTypes(), "listOfActorTypes", tag);
        CheckList(GameManager.instance.dataScript.GetListOfActorPortraits(), "listOfActorPortraits", tag);
        //authority
        CheckList(GameManager.instance.dataScript.GetActorRecruitPool(1, globalAuthority), "authorityActorPoolLevelOne", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorRecruitPool(2, globalAuthority), "authorityActorPoolLevelTwo", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorRecruitPool(3, globalAuthority), "authorityActorPoolLevelThree", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Reserve), "authorityActorReserve", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Dismissed), "authorityActorDismissed", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Promoted), "authorityActorPromoted", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Disposed), "authorityActorDisposedOff", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Resigned), "authorityActorResigned", tag, 0, false);
        //resistance
        CheckList(GameManager.instance.dataScript.GetActorRecruitPool(1, globalResistance), "resistanceActorPoolLevelOne", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorRecruitPool(2, globalResistance), "resistanceActorPoolLevelTwo", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorRecruitPool(3, globalResistance), "resistanceActorPoolLevelThree", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Reserve), "resistanceActorReserve", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Dismissed), "resistanceActorDismissed", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Promoted), "resistanceActorPromoted", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Disposed), "resistanceActorDisposedOff", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Resigned), "resistanceActorResigned", tag, 0, false);
        //hq
        CheckList(GameManager.instance.dataScript.GetListOfActorHQ(), "listOfActorHQ", tag, 0, false);
        //authority duplicates
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorRecruitPool(1, globalAuthority), "actorID", "actor", "authorityActorPoolLevelOne");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorRecruitPool(2, globalAuthority), "actorID", "actor", "authorityActorPoolLevelTwo");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorRecruitPool(3, globalAuthority), "actorID", "actor", "authorityActorPoolLevelThree");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Reserve), "actorID", "actor", "authorityActorReserve");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Dismissed), "actorID", "actor", "authorityActorDismissed");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Promoted), "actorID", "actor", "authorityActorPromoted");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Disposed), "actorID", "actor", "authorityActorDisposedOff");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Resigned), "actorID", "actor", "authorityActorResigned");
        //resistence duplicates
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorRecruitPool(1, globalResistance), "actorID", "actor", "resistanceActorPoolLevelOne");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorRecruitPool(2, globalResistance), "actorID", "actor", "resistanceActorPoolLevelTwo");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorRecruitPool(3, globalResistance), "actorID", "actor", "resistanceActorPoolLevelThree");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Reserve), "actorID", "actor", "resistanceActorReserve");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Dismissed), "actorID", "actor", "resistanceActorDismissed");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Promoted), "actorID", "actor", "resistanceActorPromoted");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Disposed), "actorID", "actor", "resistanceActorDisposedOff");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Resigned), "actorID", "actor", "resistanceActorResigned");
        //hq
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfActorHQ(), "actorID", "actor", "actorHQPool");
        //
        // - - - arrayOfActors
        //
        Actor[,] arrayOfActors = GameManager.instance.dataScript.GetArrayOfActors();
        if (arrayOfActors != null)
        {
            bool[,] arrayOfActorsPresent = GameManager.instance.dataScript.GetArrayOfActorsPresent();
            if (arrayOfActorsPresent != null)
            {
                //check for actors being, or not being, present (eg. cross check both arrays to see if in synch)
                for (int outer = 1; outer < 3; outer++)
                {
                    for (int inner = 0; inner < maxNumOfOnMapActors; inner++)
                    {
                        if (arrayOfActorsPresent[outer, inner] == true)
                        {
                            //actor present
                            if (arrayOfActors[outer, inner] == null)
                            { Debug.LogFormat("{0}Invalid Actor (Null, shouldn't be as TRUE in arrayOfActorsPresent, in arrayOfActors[{1}, {2}]{3}", tag, outer, inner, "\n"); }
                        }
                        else
                        {
                            //no actor
                            if (arrayOfActors[outer, inner] != null)
                            { Debug.LogFormat("{0}Invalid Actor (should be Null as FALSE in arrayOfActorsPresent, in arrayOfActors[{1}, {2}]{3}", tag, outer, inner, "\n"); }
                        }
                    }
                }
            }
            else { Debug.LogError("Invalid arrayOfActorsPresent (Null)"); }
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }
        //
        // - - - arrayOfActorsHQ
        //
        Actor[] arrayOfActorsHQ = GameManager.instance.dataScript.GetArrayOfActorsHQ();
        if (arrayOfActorsHQ != null)
        {
            //check first and last indexes are empty (due to enum.ActorHQ as indexes have to correspond, so first, 'None' and last 'Worker' should be Null)
            int stopIndex = (int)ActorHQ.Count - 2;
            if (arrayOfActorsHQ[0] != null) { Debug.LogFormat("{0} Actor present in arrayOfActorsHQ[0] (should be Null){1}", tag, "\n"); }
            if (arrayOfActorsHQ[stopIndex] != null) { Debug.LogFormat("{0} Actor present in arrayOfActorsHQ[{1}] (should be Null){2}", tag, stopIndex, "\n"); }
            for (int i = 1; i < stopIndex; i++)
            {
                if (arrayOfActorsHQ[i] == null)
                { Debug.LogFormat("{0}Invalid Actor in arrayOfActorsHQ[{1}] (Null, shouldn't be as all hierarchy positions should be filled){2}", tag, i, "\n"); }
            }
        }
        else { Debug.LogError("Invalid arrayOfActorsHQ (Null)"); }
    }
    #endregion

    #region CheckTargetData
    /// <summary>
    /// Integrity check on all target related collections
    /// </summary>
    /// <param name="prefix"></param>
    private void CheckTargetData(string prefix, int highestNodeID, int highestContactID, int highestTurn)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckTargetData: ");
        int maxTargetIntel = GameManager.instance.targetScript.maxTargetInfo;
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfTargets
        //
        Dictionary<string, Target> dictOfTargets = GameManager.instance.dataScript.GetDictOfTargets();
        if (dictOfTargets != null)
        {
            foreach (var target in dictOfTargets)
            {
                key = target.Key;
                CheckDictRange(target.Value.intel, 0, maxTargetIntel, "intel", tag, key);
                if (target.Value.targetStatus != Status.Dormant && target.Value.targetStatus != Status.Done)
                { CheckDictRange(target.Value.nodeID, 0, highestNodeID, "nodeID", tag, key); }
                if (target.Value.targetStatus == Status.Done)
                { CheckDictRange(target.Value.turnDone, 0, highestTurn, "turnDone", tag, key); }
                CheckDictRange(target.Value.distance, 0, 20, "distance", tag, key);
                CheckDictRange(target.Value.numOfAttempts, 0, 20, "numOfAtttempts", tag, key);
                CheckDictList(target.Value.listOfRumourContacts, "listOfRumourContacts", tag, key, false);
                CheckDictListBounds(target.Value.listOfRumourContacts, "listOfRumourContacts", tag, key, 0, highestContactID);
            }
        }
        else { Debug.LogError("Invalid dictOfTargets (Null)"); }
        //
        // - - - Target lists
        //
        CheckList(GameManager.instance.dataScript.GetTargetPool(Status.Active), "targetPoolActive", tag);
        CheckList(GameManager.instance.dataScript.GetTargetPool(Status.Live), "targetPoolLive", tag);
        CheckList(GameManager.instance.dataScript.GetTargetPool(Status.Outstanding), "targetPoolOutstanding", tag);
        CheckList(GameManager.instance.dataScript.GetTargetPool(Status.Done), "targetPoolDone", tag);
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfNodesWithTargets(), "nodes", "nodeID", "listOfNodesWithTargets");
    }
    #endregion

    #region CheckTeamData
    /// <summary>
    /// Integrity check all team related collections
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="highestNodeID"></param>
    /// <param name="highestTeamID"></param>
    private void CheckTeamData(string prefix, int highestNodeID, int highestTeamID, int highestSlotID, int highestTurn, GlobalSide playerSide)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckTeamData: ");
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfTeams
        //
        Dictionary<int, Team> dictOfTeams = GameManager.instance.dataScript.GetDictOfTeams();
        if (dictOfTeams != null)
        {
            foreach (var team in dictOfTeams)
            {
                key = team.Key.ToString();
                CheckDictRange(team.Value.teamID, 0, highestTeamID, "teamID", tag, key);
                CheckDictString(team.Value.teamName, "teamName", tag, key);
                CheckDictObject(team.Value.arc, "arc", tag, key);
                if (team.Value.pool == TeamPool.OnMap)
                {
                    //OnMap team should have actor assigned only if Authority player side, default '-1' if playerSide
                    if (playerSide.level == globalAuthority.level)
                    { CheckDictRange(team.Value.actorSlotID, 0, highestSlotID, "actorSlotID", tag, key); }
                    else { CheckDictRange(team.Value.actorSlotID, -1, -1, "actorSlotID", tag, key); }
                    CheckDictRange(team.Value.nodeID, 0, highestNodeID, "nodeID", tag, key);
                    CheckDictRange(team.Value.turnDeployed, 0, highestTurn, "turnDeployed", tag, key);
                }
                else
                {
                    //if Reserve or Intransit values should all be set to default
                    CheckDictRange(team.Value.actorSlotID, -1, -1, "actorSlotID", tag, key);
                    CheckDictRange(team.Value.nodeID, -1, -1, "nodeID", tag, key);
                    CheckDictRange(team.Value.turnDeployed, -1, -1, "turnDeployed", tag, key);
                }
            }
        }
        else { Debug.LogError("Invalid dictOfTeams (Null)"); }
        //
        // - - - Team pools
        //
        CheckListForDuplicates(GameManager.instance.dataScript.GetTeamPool(TeamPool.Reserve), "team", "teamID", "teamPoolReserve");
        CheckListForDuplicates(GameManager.instance.dataScript.GetTeamPool(TeamPool.OnMap), "team", "teamID", "teamPoolOnMap");
        CheckListForDuplicates(GameManager.instance.dataScript.GetTeamPool(TeamPool.InTransit), "team", "teamID", "teamPoolInTransit");
    }
    #endregion

    #region CheckGearData
    /// <summary>
    /// Integrity check for all gear related conditions
    /// </summary>
    /// <param name="prefix"></param>
    private void CheckGearData(string prefix)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckGearData: ");
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfGear
        //
        Dictionary<string, Gear> dictOfGear = GameManager.instance.dataScript.GetDictOfGear();
        if (dictOfGear != null)
        {
            foreach (var gear in dictOfGear)
            {
                key = gear.Key;
                CheckDictObject(gear.Value, "Gear.Value", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfGear (Null)"); }
        //
        // - - - Gear lists
        //
        CheckList(GameManager.instance.dataScript.GetListOfGearRarity(), "listOfGearRarity", tag);
        CheckList(GameManager.instance.dataScript.GetListOfGearType(), "listOfGearType", tag);
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfCurrentGear(), "Gear", "gearName", "listOfCurrentGear");
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfLostGear(), "Gear", "gearName", "listOfLostGear");
    }
    #endregion

    #region CheckConnectionData
    /// <summary>
    /// Integrity check for all connection related collections
    /// </summary>
    /// <param name="prefix"></param>
    private void CheckConnectionData(string prefix, int highestNodeID, int highestConnID)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckConnectionData: ");
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfConnections
        //
        Dictionary<int, Connection> dictOfConnections = GameManager.instance.dataScript.GetDictOfConnections();
        if (dictOfConnections != null)
        {
            foreach (var conn in dictOfConnections)
            {
                key = conn.Key.ToString();
                CheckDictRange(conn.Value.connID, 0, highestConnID, "connID", tag, key);
                CheckDictRange(conn.Value.node1.nodeID, 0, highestNodeID, "node1.nodeID", tag, key);
                CheckDictRange(conn.Value.node2.nodeID, 0, highestNodeID, "node2.nodeID", tag, key);
                CheckDictRange(conn.Value.VerticeOne, 0, highestNodeID, "VerticeOne", tag, key);
                CheckDictRange(conn.Value.VerticeTwo, 0, highestNodeID, "VerticeTwo", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfConnections (Null)"); }
        //
        // - - - listOfConnections
        //
        CheckList(GameManager.instance.dataScript.GetListOfConnections(), "listOfConnections", tag, highestConnID);
    }
    #endregion

    #region CheckMessageData
    /// <summary>
    /// Integrity check for all message related collections
    /// </summary>
    private void CheckMessageData(string prefix, int highestMessageID, int highestTurn)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckMessageData: ");
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfArchiveMessages
        //
        Dictionary<int, Message> dictOfArchiveMessages = GameManager.instance.dataScript.GetMessageDict(MessageCategory.Archive);
        if (dictOfArchiveMessages != null)
        {
            foreach (var message in dictOfArchiveMessages)
            {
                key = message.Key.ToString();
                CheckDictRange(message.Value.msgID, 0, highestMessageID, "msgID", tag, key);
                CheckDictRange(message.Value.turnCreated, 0, highestTurn, "turnCreated", tag, key);
                CheckDictString(message.Value.text, "text", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfArchiveMessages (Null)"); }
        //
        // - - - dictOfPendingMessages
        //
        Dictionary<int, Message> dictOfPendingMessages = GameManager.instance.dataScript.GetMessageDict(MessageCategory.Pending);
        if (dictOfPendingMessages != null)
        {
            foreach (var message in dictOfPendingMessages)
            {
                key = message.Key.ToString();
                CheckDictRange(message.Value.msgID, 0, highestMessageID, "msgID", tag, key);
                CheckDictRange(message.Value.turnCreated, 0, highestTurn, "turnCreated", tag, key);
                CheckDictString(message.Value.text, "text", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfPendingMessages (Null)"); }
        //
        // - - - dictOfCurrentMessages
        //
        Dictionary<int, Message> dictOfCurrentMessages = GameManager.instance.dataScript.GetMessageDict(MessageCategory.Current);
        if (dictOfCurrentMessages != null)
        {
            foreach (var message in dictOfCurrentMessages)
            {
                key = message.Key.ToString();
                CheckDictRange(message.Value.msgID, 0, highestMessageID, "msgID", tag, key);
                CheckDictRange(message.Value.turnCreated, 0, highestTurn, "turnCreated", tag, key);
                CheckDictString(message.Value.text, "text", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfCurrentMessages (Null)"); }
        //
        // - - - dictOfAIMessages
        //
        Dictionary<int, Message> dictOfAIMessages = GameManager.instance.dataScript.GetMessageDict(MessageCategory.AI);
        if (dictOfAIMessages != null)
        {
            foreach (var message in dictOfAIMessages)
            {
                key = message.Key.ToString();
                CheckDictRange(message.Value.msgID, 0, highestMessageID, "msgID", tag, key);
                CheckDictRange(message.Value.turnCreated, 0, highestTurn, "turnCreated", tag, key);
                CheckDictString(message.Value.text, "text", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfAIMessages (Null)"); }
        //
        // - - - Duplicate check all dictionaries
        //
        CheckListForDuplicates(GameManager.instance.dataScript.GetMessageListOfID(MessageCategory.Archive), "Message", "msgID", "dictOfArchiveMessages");
        CheckListForDuplicates(GameManager.instance.dataScript.GetMessageListOfID(MessageCategory.Pending), "Message", "msgID", "dictOfPendingMessages");
        CheckListForDuplicates(GameManager.instance.dataScript.GetMessageListOfID(MessageCategory.Current), "Message", "msgID", "dictOfCurrentMessages");
        CheckListForDuplicates(GameManager.instance.dataScript.GetMessageListOfID(MessageCategory.AI), "Message", "msgID", "dictOfAIMessages");
    }
    #endregion

    #region CheckSecretData
    /// <summary>
    /// Integrity check for all secret related collections
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="highestActorID"></param>
    /// <param name="highestTurn"></param>
    private void CheckSecretData(string prefix, int highestActorID, int highestTurn)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckSecretData: ");
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfSecrets
        //
        Dictionary<string, Secret> dictOfSecrets = GameManager.instance.dataScript.GetDictOfSecrets();
        if (dictOfSecrets != null)
        {
            foreach (var secret in dictOfSecrets)
            {
                key = secret.Key;
                switch (secret.Value.status)
                {
                    case gameAPI.SecretStatus.Inactive:

                        break;
                    case gameAPI.SecretStatus.Active:
                        CheckDictRange(secret.Value.gainedWhen, 0, highestTurn, "gainedWhen", tag, key);
                        break;
                    case gameAPI.SecretStatus.Revealed:
                        CheckDictRange(secret.Value.gainedWhen, 0, highestTurn, "gainedWhen", tag, key);
                        CheckDictRange(secret.Value.revealedWhen, 0, highestTurn, "revealedWhen (turn)", tag, key);
                        CheckDictString(secret.Value.revealedWho, "secret.revealedWho", tag, key);
                        break;
                    case gameAPI.SecretStatus.Deleted:
                        CheckDictRange(secret.Value.gainedWhen, 0, highestTurn, "gainedWhen", tag, key);
                        CheckDictRange(secret.Value.deletedWhen, 0, highestTurn, "deletedWhen", tag, key);
                        break;
                    default:
                        Debug.LogWarningFormat("Unrecognised secret.status \"{0}\" for {1}", secret.Value.status, key);
                        break;
                }
                CheckDictListBounds(secret.Value.GetListOfActors(), "listOfActors", tag, key, 0, highestActorID);
                //check Organisation secrets have a non-null Organisation field
                if (secret.Value.type.name.Equals("Organisation", StringComparison.Ordinal) == true)
                {
                    if (secret.Value.org == null)
                    { Debug.LogFormat("{0}Organisation secret \"{1}\" has an invalid org field (Null){2}", tag, secret.Value.name, "\n"); }
                }
            }
        }
        else { Debug.LogError("Invalid dictOfSecrets (Null)"); }
        //
        // - - - lists Of Secrets
        //
        CheckList(GameManager.instance.dataScript.GetListOfPlayerSecrets(), "listOfPlayerSecrets", tag);
        CheckList(GameManager.instance.dataScript.GetListOfRevealedSecrets(), "listOfRevealedSecrets", tag);
        CheckList(GameManager.instance.dataScript.GetListOfDeletedSecrets(), "listOfDeletedSecrets", tag);
    }
    #endregion

    #region CheckMainInfoData
    /// <summary>
    /// Integrity check for all MainInfoData
    /// </summary>
    private void CheckMainInfoData(string prefix, int highestTurn)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckMainInfoData: ");
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfHistory
        //
        Dictionary<int, MainInfoData> dictOfHistory = GameManager.instance.dataScript.GetDictOfHistory();
        if (dictOfHistory != null)
        {
            foreach (var info in dictOfHistory)
            {
                key = info.Key.ToString();
                CheckDictRange(info.Key, 0, highestTurn, "turn", tag, key);
                CheckDictObject(info.Value, "MainInfoData", tag, key);
                CheckDictArray(info.Value.arrayOfItemData, "arrayOfItemData", tag, key, true);
                CheckDictString(info.Value.tickerText, "tickerText", tag, key);
            }
        }
    }
    #endregion

    #region CheckContactData
    /// <summary>
    /// Integrity check for all contact related collections
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="highestContactID"></param>
    /// <param name="highestNodeID"></param>
    /// <param name="highestActorID"></param>
    /// <param name="highestTurn"></param>
    private void CheckContactData(string prefix, int highestContactID, int highestNodeID, int highestActorID, int highestTurn, GlobalSide playerSide)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckContactData: ");
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfContacts
        //
        Dictionary<int, Contact> dictOfContacts = GameManager.instance.dataScript.GetDictOfContacts();
        if (dictOfContacts != null)
        {
            foreach (var contact in dictOfContacts)
            {
                key = contact.Key.ToString();
                CheckDictRange(contact.Value.contactID, 0, highestContactID, "contactID", tag, key);
                CheckDictString(contact.Value.nameFirst, "nameFirst", tag, key);
                CheckDictString(contact.Value.nameLast, "nameLast", tag, key);
                CheckDictString(contact.Value.job, "job", tag, key);
                CheckDictRange(contact.Value.statsRumours, 0, 99, "statsRumour", tag, key);
                CheckDictRange(contact.Value.statsNemesis, 0, 99, "statsNemesis", tag, key);
                CheckDictRange(contact.Value.statsTeams, 0, 99, "statsTeams", tag, key);
                switch (contact.Value.status)
                {
                    case ContactStatus.Active:
                        CheckDictRange(contact.Value.actorID, 0, highestActorID, "actorID", tag, key);
                        CheckDictRange(contact.Value.nodeID, 0, highestNodeID, "nodeID", tag, key);
                        CheckDictRange(contact.Value.turnStart, 0, highestTurn, "turnStart", tag, key);
                        CheckDictString(contact.Value.typeName, "typeName", tag, key);
                        break;
                    case ContactStatus.Inactive:
                        CheckDictRange(contact.Value.turnFinish, 0, highestTurn, "turnFinish", tag, key);
                        CheckDictString(contact.Value.typeName, "typeName", tag, key);
                        break;
                }
                CheckDictRange(contact.Value.effectiveness, 1, 3, "effectiveness", tag, key);
                //sex
                if (contact.Value.sex == ActorSex.None)
                { Debug.LogFormat("{0}Invalid sex ('None') for contact {0} {1}, ID {2}{3}", contact.Value.nameFirst, contact.Value.nameLast, contact.Value.contactID, "\n"); }
            }
        }
        else { Debug.LogError("Invalid dictOfContacts (Null)"); }
        //
        // - - - Resistance only
        //
        if (playerSide.level == globalResistance.level)
        {
            CheckListForDuplicates(GameManager.instance.dataScript.GetContactPool(), "Contact", "contactID", "contactPool");
            //dictOfActorContacts
            Dictionary<int, List<int>> dictOfActorContacts = GameManager.instance.dataScript.GetDictOfActorContacts();
            if (dictOfActorContacts != null)
            {
                foreach (var record in dictOfActorContacts)
                {
                    key = record.Key.ToString();
                    CheckDictRange(record.Key, 0, highestActorID, "actorID", tag, key);
                    CheckDictListBounds(record.Value, "dictOfActorContacts.List<int>", tag, key, 0, highestNodeID);
                    CheckListForDuplicates(record.Value, "Node", "nodeID", string.Format("dictOfActorContacts for actorID {0}", record.Key));

                }
            }
            else { Debug.LogError("Invalid dictOfActorContacts (Null)"); }
            //dictOfNodeContactsResistance
            Dictionary<int, List<int>> dictOfNodeContactsResistance = GameManager.instance.dataScript.GetDictOfNodeContacts(globalResistance);
            if (dictOfNodeContactsResistance != null)
            {
                foreach (var record in dictOfNodeContactsResistance)
                {
                    key = record.Key.ToString();
                    CheckDictRange(record.Key, 0, highestNodeID, "nodeID", tag, key);
                    CheckDictListBounds(record.Value, "dictOfNodeContactsResistance.List<int>", tag, key, 0, highestActorID);
                    CheckListForDuplicates(record.Value, "Actor", "actorID", string.Format("dictOfNodeContactsResistance for nodeID {0}", record.Key));
                }
            }
            else { Debug.LogError("Invalid dictOfNodeContactsResistance (Null)"); }
            //dictOfContactsByNodeResistance
            Dictionary<int, List<Contact>> dictOfContactsByNodeResistance = GameManager.instance.dataScript.GetDictOfContactsByNodeResistance();
            if (dictOfContactsByNodeResistance != null)
            {
                foreach (var record in dictOfContactsByNodeResistance)
                {
                    key = record.Key.ToString();
                    CheckDictRange(record.Key, 0, highestNodeID, "nodeID", tag, key);
                    CheckDictList(record.Value, "dictOfNodeContactsResistance.List<Contact>", tag, key);
                }
            }
            else { Debug.LogError("Invalid dictOfContactsByNodeResistance (Null)"); }
        }
        //
        // - - - Both sides
        //
        //dictOfNodeContactsAuthority
        Dictionary<int, List<int>> dictOfNodeContactsAuthority = GameManager.instance.dataScript.GetDictOfNodeContacts(globalAuthority);
        if (dictOfNodeContactsAuthority != null)
        {
            foreach (var record in dictOfNodeContactsAuthority)
            {
                key = record.Key.ToString();
                List<int> tempList = record.Value;
                CheckListForDuplicates(tempList, "Actor", "actorID", string.Format("dictOfNodeContactsAuthority for nodeID {0}", record.Key));
            }
        }
        else { Debug.LogError("Invalid dictOfNodeContactsAuthority (Null)"); }
    }
    #endregion

    #region CheckPlayerData
    /// <summary>
    /// Integrity check of all dynamic Player data
    /// </summary>
    /// <param name="prefix"></param>
    private void CheckPlayerData(string prefix)
    {
        string tag = string.Format("{0}{1}", prefix, "CheckPlayerData: ");
        int maxFactor = GameManager.instance.personScript.maxPersonalityFactor;
        int minFactor = GameManager.instance.personScript.minPersonalityFactor;
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        CheckRange(GameManager.instance.playerScript.GetMood(), 0, GameManager.instance.playerScript.moodMax, "mood", tag);
        CheckRange(GameManager.instance.playerScript.actorID, 999, 999, "actorID", tag);
        //sex
        if (GameManager.instance.playerScript.sex == ActorSex.None)
        { Debug.LogFormat("{0}Invalid sex ('None') for Player{1}", tag, "\n"); }
        //personality
        Personality personality = GameManager.instance.playerScript.GetPersonality();
        if (personality != null)
        {
            CheckArrayBounds(personality.GetFactors(), "arrayOfFactors", tag, minFactor, maxFactor);
            if (personality.GetProfile() != null)
            {
                CheckString(personality.GetProfileDescriptor(), "profileDescriptor", tag);
                CheckString(personality.GetProfileExplanation(), "profileExplanation", tag);
                CheckList(personality.GetListOfDescriptors(), "personality.listOfDescriptors", tag);
                CheckList(personality.GetListOfMotivation(), "personality.listOfMotivation", tag);
            }
        }
        else { Debug.LogFormat("{0}Invalid personality (Null) for Player {1}", tag, "\n"); }
        //collections
        CheckList(GameManager.instance.playerScript.GetListOfGear(), "listOfGear", tag);
        CheckListForDuplicates(GameManager.instance.playerScript.GetListOfGear(), "Gear", "gearName", "ListOfGear");
        CheckList(GameManager.instance.playerScript.GetListOfConditions(globalAuthority), "listOfConditionsAuthority", tag);
        CheckList(GameManager.instance.playerScript.GetListOfConditions(globalResistance), "listOfConditionsResistance", tag);
        CheckList(GameManager.instance.playerScript.GetListOfSecrets(), "listOfSecrets", tag);
        CheckList(GameManager.instance.playerScript.GetListOfMoodHistory(), "listOfModdHistory", tag);

    }
    #endregion

    #region CheckTopicData
    /// <summary>
    /// Integrity check of all relevant dynamic Topic data
    /// </summary>
    /// <param name="prefix"></param>
    private void CheckTopicData(string prefix)
    {
        int index;
        string tag = string.Format("{0}{1}", prefix, "CheckTopicData: ");
        Dictionary<string, List<Topic>> dictOfTopicPools = GameManager.instance.dataScript.GetDictOfTopicPools();
        if (dictOfTopicPools != null)
        {
            //
            // - - - Topic Pools
            //
            foreach (var pool in dictOfTopicPools)
            {
                CheckDictList(pool.Value, "listOfTopics", tag, pool.Key);
                foreach (Topic topic in pool.Value)
                {
                    //Level scope
                    if (topic.subType.scope.name.Equals(levelScopeName, StringComparison.Ordinal) == true)
                    {
                        //each topic within pool should be 'isCurrent' true (dict contains all valid topics for the level) if Level Scope only
                        if (topic.isCurrent != true)
                        { Debug.LogFormat("{0}, topic \"{1}\", Invalid isCurrent (is {2}, should be {3}){4}", tag, topic.name, topic.isCurrent, "True", "\n"); }
                        //shouldn't have a linkedIndex other than the default -1 value
                        if (topic.linkedIndex > -1)
                        { Debug.LogFormat("{0}, topic \"{1}\", Invalid linkedIndex (is {2}, should be default -1){3}", tag, topic.name, topic.linkedIndex, "\n"); }
                        //both linked and buddy lists should be empty
                        if (topic.listOfLinkedTopics != null)
                        {
                            if (topic.listOfLinkedTopics.Count > 0)
                            { Debug.LogFormat("{0}, topic \"{1}\", Invalid listOfLinkedTopics Count (is {2}, should be Zero){3}", tag, topic.name, topic.listOfLinkedTopics.Count, "\n"); }
                        }
                        if (topic.listOfBuddyTopics != null)
                        {
                            if (topic.listOfBuddyTopics.Count > 0)
                            { Debug.LogFormat("{0}, topic \"{1}\", Invalid listOfBuddyTopics Count (is {2}, should be Zero){3}", tag, topic.name, topic.listOfBuddyTopics.Count, "\n"); }
                        }
                    }
                    //Campaign scope
                    else if (topic.subType.scope.name.Equals(campaignScopeName, StringComparison.Ordinal) == true)
                    {
                        index = topic.linkedIndex;
                        //linkedIndex > -1 (default)
                        if (index < 0)
                        { Debug.LogFormat("{0}, topic \"{1}\", Invalid linkedIndex (is {2}, should be > -1){3}", tag, topic.name, index, "\n"); }
                        //Profile can't have a repeat delay or a window timer
                        if (topic.profile != null)
                        {
                            if (topic.profile.delayRepeat > 0)
                            { Debug.LogFormat("{0}, topic \"{1}\", Invalid profile.delayRepeat (is {2}, should be Zero){3}", tag, topic.name, topic.profile.delayRepeat, "\n"); }
                            if (topic.profile.timerWindow > 0)
                            { Debug.LogFormat("{0}, topic \"{1}\", Invalid profile.timerWindoow (is {2}, should be Zero){3}", tag, topic.name, topic.profile.timerWindow, "\n"); }
                        }
                        else { Debug.LogWarningFormat("Invalid profile (Null) for topic \"{0}\"", topic.name); }
                        //listOfBuddyTopics
                        if (topic.listOfBuddyTopics != null)
                        {
                            //should have at least one entry
                            if (topic.listOfBuddyTopics.Count == 0)
                            { Debug.LogFormat("{0}, topic \"{1}\", Invalid listOfBuddyTopics Count (is {2}, should be > 0){3}", tag, topic.name, topic.listOfBuddyTopics.Count, "\n"); }
                            //check for nulls
                            CheckList<Topic>(topic.listOfBuddyTopics, "listOfBuddyTopics", topic.name);
                            //check for duplicates
                            List<string> tempList = topic.listOfBuddyTopics.Select(x => x.name).ToList();
                            if (tempList != null)
                            { CheckListForDuplicates<string>(tempList, "Topic", topic.name, "listOfBuddyTopics"); }
                            else { Debug.LogWarningFormat("Invalid tempList (Null) from {0}.listOfBuddyTopics", topic.name); }
                            //check topic is present in buddy list
                            if (topic.listOfBuddyTopics.Exists(x => x.name.Equals(topic.name)) == false)
                            { Debug.LogFormat("{0}, topic \"{1}\", MISSING in listOfBuddyTopics{2}", tag, topic.name, "\n"); }
                            //should all have same linkedIndex as topic
                            for (int i = 0; i < topic.listOfBuddyTopics.Count; i++)
                            {
                                Topic buddyTopic = topic.listOfBuddyTopics[i];
                                if (buddyTopic != null)
                                {
                                    if (buddyTopic.linkedIndex != index)
                                    { Debug.LogFormat("{0}, topic \"{1}\", in listOfBuddyTopics has Invalid linkedIndex (is {2}, should be > {3}){4}", tag, buddyTopic.name, buddyTopic.linkedIndex, index, "\n"); }
                                }
                                else { Debug.LogWarningFormat("Invalid topic (Null) in listOfBuddyTopics for \"{0}\"", buddyTopic.name); }
                            }
                        }
                        else { Debug.LogFormat("{0}, topic \"{1}\", Invalid listOfBuddyTopics (Null) (should have entries){2}", tag, topic.name, "\n"); }
                        //listOfLinkedTopics
                        if (topic.listOfLinkedTopics != null)
                        {
                            if (topic.listOfLinkedTopics.Count > 0)
                            {
                                //check for nulls
                                CheckList<Topic>(topic.listOfLinkedTopics, "listOfLinkedTopics", topic.name);
                                //check for duplicates
                                List<string> tempList = topic.listOfLinkedTopics.Select(x => x.name).ToList();
                                if (tempList != null)
                                { CheckListForDuplicates<string>(tempList, "Topic", topic.name, "listOfLinkedTopics"); }
                                else { Debug.LogWarningFormat("Invalid tempList (Null) from {0}.listOfLinkedTopics", topic.name); }
                                //should all have the same linkedIndex and it should be one more than the topic's linkedIndex
                                index += 1;
                                for (int i = 0; i < topic.listOfLinkedTopics.Count; i++)
                                {
                                    Topic linkedTopic = topic.listOfLinkedTopics[i];
                                    if (linkedTopic != null)
                                    {
                                        if (linkedTopic.linkedIndex != index)
                                        { Debug.LogFormat("{0}, topic \"{1}\", in listOfLinkedTopics has Invalid linkedIndex (is {2}, should be > {3}){4}", tag, linkedTopic.name, linkedTopic.linkedIndex, index, "\n"); }
                                    }
                                    else { Debug.LogWarningFormat("Invalid topic (Null) in listOfLinkedTopics for \"{0}\"", linkedTopic.name); }
                                }
                            }
                        }

                    }
                    else { Debug.LogFormat("{0}, topic \"{1}\", Invalid subType.scope.name \"{2}\"{3}", tag, topic.name, topic.subType.scope.name, "\n"); }
                }
            }
        }
        else { Debug.LogError("Invalid dictOfTopicPools (Null)"); }
    }
    #endregion

    #region CheckTextListData
    /// <summary>
    /// Integrity check to ensure textList indexes are within bounds
    /// </summary>
    /// <param name="prefix"></param>
    private void CheckTextListData(string prefix)
    {
        string tag = string.Format("{0}{1}", prefix, "CheckTextListData: ");
        TextList[] arrayOfTextLists = GameManager.instance.loadScript.arrayOfTextLists;
        if (arrayOfTextLists != null)
        {
            for (int i = 0; i < arrayOfTextLists.Length; i++)
            {
                TextList textList = arrayOfTextLists[i];
                if (textList != null)
                {
                    //check index within bounds
                    if (textList.index > -1)
                    {
                        if (textList.index >= textList.randomList.Count)
                        { Debug.LogFormat("{0}Invalid textList index \"{0}\" for textList {1}, (has only {2} records){3}", textList.index, textList.name, textList.randomList.Count, "\n"); }
                    }
                    //check randomList
                    CheckList(textList.randomList, textList.name, tag);
                }
                else { Debug.LogFormat("{0}Invalid textList (Null) for arrayOfTextLists[{1}]{2}", tag, i, "\n"); }
            }
        }
        else { Debug.LogError("Invalid arrayOfTextLists (Null)"); }
    }

    #endregion

    #endregion

    #region Utilities
    //
    // - - - Utilities - - - 
    //

    #region CheckListForDuplicates
    /// <summary>
    /// generic method that takes any list of IComparables, eg. int, string, but NOT SO's, and checks for duplicates. Debug.LogFormat("[Val]"...) messages generated for dupes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="listToCheck"></param>
    private void CheckListForDuplicates<T>(List<T> listToCheck, string typeOfObject = "Unknown", string nameOfObject = "Unknown", string nameOfList = "Unknown") where T : IComparable
    {
        if (listToCheck != null)
        {
            if (listToCheck.Count > 0)
            {
                var query = listToCheck.GroupBy(x => x)
                                .Where(g => g.Count() > 1)
                                .Select(y => new { Element = y.Key, Counter = y.Count() })
                                .ToList();
                if (query.Count > 0)
                {
                    //generate message for each set of duplicates in list
                    int numOfDuplicates = 0;
                    foreach (var item in query)
                    {
                        numOfDuplicates = item.Counter - 1;
                        Debug.LogFormat("[Val] ValidationManager.cs -> CheckListForDuplicates: {0} \"{1}\" , list \"{2}\", record \"{3}\" has {4} duplicate{5}{6}", typeOfObject, nameOfObject, nameOfList,
                            item.Element, numOfDuplicates, numOfDuplicates != 1 ? "s" : "", "\n");
                    }
                }
            }
        }
        else { Debug.LogFormat("[Val] ValidationManager.cs -> CheckListForDuplicates: Invalid listToCheck {0} (Null){1}", nameOfList, "\n"); }
    }
    #endregion

    #region CheckListForSame
    /// <summary>
    /// generic method that takes any list of IComparables, eg. int, string, but NOT SO's, and checks that all items are identical. Message generated if not.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="listToCheck"></param>
    /// <param name="typeOfObject"></param>
    /// <param name="nameOfObject"></param>
    /// <param name="nameOfList"></param>
    private void CheckListForSame<T>(List<T> listToCheck, string typeOfObject = "Unknown", string nameOfObject = "Unknown", string nameOfList = "Unknown") where T : IComparable
    {
        if (listToCheck != null)
        {
            if (listToCheck.Count > 0)
            {
                var query = listToCheck.GroupBy(x => x)
                                .Where(g => g.Count() > 1)
                                .Select(y => new { Element = y.Key, Counter = y.Count() })
                                .ToList();
                if (query.Count != 1)
                { Debug.LogFormat("[Val] ValidationManager.cs -> CheckListForSame: {0} \"{1}\" , list \"{2}\", has DIFFERENCES (should all be the same){3}", typeOfObject, nameOfObject, nameOfList, "\n"); }
            }
        }
        else { Debug.LogFormat("[Val] ValidationManager.cs -> CheckListForSame: Invalid listToCheck {0} (Null){1}", nameOfList, "\n"); }
    }
    #endregion


    /// <summary>
    /// Checks that a dictionary int value is inside specified range (inclusive)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    /// <param name="tag"></param>
    /// <param name="key"></param>
    private void CheckDictRange(int value, int lower, int upper, string varName, string tag, string key)
    {
        if (value < lower || value > upper)
        { Debug.LogFormat("{0}dictKey \"{1}\", variable \"{2}\", value {3}, outside of range ({4} to {5}){6}", tag, key, varName, value, lower, upper, "\n"); }
    }

    /// <summary>
    /// Checks dictionary string for Null and Empty
    /// </summary>
    /// <param name="text"></param>
    /// <param name="tag"></param>
    /// <param name="key"></param>
    private void CheckDictString(string text, string stringName, string tag, string key)
    {
        if (string.IsNullOrEmpty(text) == true)
        { Debug.LogFormat("{0}Invalid {1} (Null or Empty) for dictKey {2}{3}", tag, stringName, key, "\n"); }
    }

    /// <summary>
    /// Checks any dictionary object/field for null
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="thing"></param>
    /// <param name="tag"></param>
    /// <param name="key"></param>
    private void CheckDictObject<T>(T thing, string objectName, string tag, string key)
    {
        if (thing == null)
        { Debug.LogFormat("{0}Invalid {1} (Null) for dictKey {2}{3}", tag, objectName, key, "\n"); }
    }

    /// <summary>
    /// Check dictionary list for Null and check elements in list for Null (optional, default true)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="key"></param>
    /// <param name="tag"></param>
    private void CheckDictList<T>(List<T> list, string listName, string tag, string key, bool isNullCheckContents = true)
    {
        if (list == null)
        { Debug.LogFormat("{0}Invalid {1} (Null) for dictKey {2}{3}", tag, listName, key, "\n"); }
        else
        {
            if (isNullCheckContents == true)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == null)
                    { Debug.LogFormat("{0}\"{1}\"[{2] invalid (Null) for dictKey {3}{4}", tag, listName, i, key, "\n"); }
                }
            }
        }
    }

    /// <summary>
    /// Bounds checks a dict list of Int's (each element checked if within lower and upper, inclusive) 
    /// </summary>
    /// <param name="list"></param>
    /// <param name="listName"></param>
    /// <param name="tag"></param>
    /// <param name="key"></param>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    private void CheckDictListBounds(List<int> list, string listName, string tag, string key, int lower = 0, int upper = 0)
    {
        if (list != null)
        {
            if (lower != 0 || upper != 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] < lower || list[i] > upper)
                    { Debug.LogFormat("{0}\"{1}\"[{2}] failed bounds check, value {3) (should be between {4} and {5}){6}", tag, listName, i, list[i], lower, upper, "\n"); }
                }
            }
            else { Debug.LogWarningFormat("Invalid CheckDictListBounds for {0} check as both lower and upper bounds are Zero{1}", listName, "\n"); }
        }
        else { Debug.LogFormat("{0}\"{1}\" Invalid (Null) for dictKey {2}{3}", tag, listName, key, "\n"); }
    }

    /// <summary>
    /// Check an encapsulated Dictionary within a dict object for null and all elements in list for Null (optional ,default true)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="listName"></param>
    /// <param name="tag"></param>
    /// <param name="key"></param>
    /// <param name="isNullCheckContents"></param>
    private void CheckDictDict<K, T>(Dictionary<K, T> dict, string dictName, string tag, string key, bool isNullCheckContents = true)
    {
        if (dict == null)
        { Debug.LogFormat("{0}Invalid {1} (Null) for dictKey {2}{3}", tag, dictName, key, "\n"); }
        else
        {
            if (isNullCheckContents == true)
            {
                foreach (var record in dict)
                {
                    if (record.Value == null)
                    { Debug.LogFormat("{0}\"{1}\"[{2}] invalid (Null) for dictKey {3}{4}", tag, dictName, record.Key, key, "\n"); }
                }
            }
        }
    }

    /// <summary>
    /// Check a Dict array for null. Elements checked for null if 'isNullCheckContents' true
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="tag"></param>
    private void CheckDictArray<T>(T[] array, string arrayName, string tag, string key, bool isNullCheckContents = false)
    {
        if (array != null)
        {
            for (int i = 0; i < array.Length; i++)
            {
                //null checks on elements
                if (isNullCheckContents == true)
                {
                    T item = array[0];
                    if (item == null)
                    { Debug.LogFormat("{0}{1}[{2}] is invalid (Null) for dictKey {3}{4}", tag, arrayName, i, key, "\n"); }
                }
            }
        }
        else { Debug.LogFormat("{0}\"{1}\" Invalid (Null) for dictKey {2}{3}", tag, arrayName, key, "\n"); }
    }

    /// <summary>
    /// Bounds checks a dict array of Int's (each element checked if within lower and upper, inclusive) 
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayName"></param>
    /// <param name="tag"></param>
    /// <param name="key"></param>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    private void CheckDictArrayBounds(int[] array, string arrayName, string tag, string key, int lower = 0, int upper = 0)
    {
        if (array != null)
        {
            if (lower != 0 || upper != 0)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] < lower || array[i] > upper)
                    { Debug.LogFormat("{0}\"{1}\"[{2}] failed bounds check, value {3) (should be between {4} and {5}){6}", tag, arrayName, i, array[i], lower, upper, "\n"); }
                }
            }
            else { Debug.LogWarningFormat("Invalid CheckDictArrayBounds for {0} check as both lower and upper bounds are Zero{1}", arrayName, "\n"); }
        }
        else { Debug.LogFormat("{0}\"{1}\" Invalid (Null) for dictKey {2}{3}", tag, arrayName, key, "\n"); }
    }


    /// <summary>
    /// Check a standalone (not enscapsulated within a class) list for null and check all entries in list for null (optional, default true)
    /// expectedCount used if you expect the list to have 'x' amount of records. Ignore otherwise.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="tag"></param>
    private void CheckList<T>(List<T> list, string listName, string tag, int expectedCount = 0, bool isNullCheckContents = true)
    {
        if (list != null)
        {
            //check number of records in list (optional)
            if (expectedCount > 0)
            {
                if (list.Count != expectedCount)
                { Debug.LogFormat("{0}Mismatch for {1} on count (was {2}, expected {3}){4}", tag, listName, list.Count, expectedCount, "\n"); }
            }
            //check for null records in list
            if (isNullCheckContents == true)
            {
                foreach (T item in list)
                {
                    if (item == null)
                    { Debug.LogFormat("{0}Invalid {1} (Null) in {2}{3}", tag, nameof(T), listName, "\n"); }
                }
            }
        }
        else { Debug.LogFormat("{0}\"{1}\" Invalid (Null){2}", tag, listName, "\n"); }
    }

    /// <summary>
    /// Check a string for Null or Empty
    /// </summary>
    /// <param name="text"></param>
    /// <param name="stringName"></param>
    /// <param name="tagy"></param>
    private void CheckString(string text, string stringName, string tagy)
    {
        if (string.IsNullOrEmpty(text) == true)
        { Debug.LogFormat("{0}Invalid {1} (Null or Empty){2}", tag, stringName, "\n"); }
    }

    /// <summary>
    /// Check an int is within a range (inclusive)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    /// <param name="varName"></param>
    /// <param name="tag"></param>
    private void CheckRange(int value, int lower, int upper, string varName, string tag)
    {
        if (value < lower || value > upper)
        { Debug.LogFormat("{0} variable \"{1}\", value {2}, outside of range ({3} to {4}){5}", tag, varName, value, lower, upper, "\n"); }
    }

    /// <summary>
    /// Bounds checks a dict array of Int's (each element checked if within lower and upper, inclusive) 
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayName"></param>
    /// <param name="tag"></param>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    private void CheckArrayBounds(int[] array, string arrayName, string tag, int lower = 0, int upper = 0)
    {
        if (array != null)
        {
            if (lower != 0 || upper != 0)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] < lower || array[i] > upper)
                    { Debug.LogFormat("{0}\"{1}\"[{2}] failed bounds check, value {3) (should be between {4} and {5}){6}", tag, arrayName, i, array[i], lower, upper, "\n"); }
                }
            }
            else { Debug.LogWarningFormat("Invalid CheckDictArrayBounds for {0} check as both lower and upper bounds are Zero{1}", arrayName, "\n"); }
        }
        else { Debug.LogFormat("{0}\"{1}\" Invalid (Null){2}", tag, arrayName, "\n"); }
    }

    /// <summary>
    /// Checks campaign pools (various checks) (ValidateTopics -> Campaign Topic Pool)
    /// NOTE: campaign and pool have been Null checked by parent method (and initialisation method in case of subType)
    /// </summary>
    /// <param name="campaign"></param>
    /// <param name="pool"></param>
    private void CheckCampaignPool(Campaign campaign, TopicPool pool, TopicSubType subType)
    {
        bool isGoodTopicPresent, isBadTopicPresent;
        //check type for a match
        if (pool.type.name.Equals(subType.type.name, StringComparison.Ordinal) == false)
        {
            Debug.LogFormat("[Val] ValidationManager.cs-> CheckCampaignPool: campaign \"{0}\", \"{1}\", has incorrect type ({2} should be {3}){4}",
                campaign.name, pool.name, pool.type.name, subType.type.name, "\n");
        }
        //check subType for a match
        if (pool.subType.name.Equals(subType.name, StringComparison.Ordinal) == false)
        {
            Debug.LogFormat("[Val] ValidationManager.cs-> CheckCampaignPool: campaign \"{0}\", \"{1}\", has incorrect subType ({2} should be {3}){4}", campaign.name,
                pool.name, pool.subType.name, subType.name, "\n");

        }
        //SubSubTypes present?
        if (pool.listOfSubSubTypePools.Count > 0)
        {
            for (int i = 0; i < pool.listOfSubSubTypePools.Count; i++)
            {
                TopicPool subSubPool = pool.listOfSubSubTypePools[i];
                if (subSubPool != null)
                {
                    //must have a subSubType name
                    if (subSubPool.subSubType == null)
                    { Debug.LogFormat("[Val] ValidationManager.cs-> CheckCampaignPool: campaign \"{0}\", \"{1}\", {2}, has no subSubType (Null){3}", campaign.name, pool.name, subSubPool.name, "\n"); }
                    //must have at least one topic
                    if (subSubPool.listOfTopics.Count == 0)
                    { Debug.LogFormat("[Val] ValidationManager.cs-> CheckCampaignPool: campaign \"{0}\", \"{1}\", {2}, has an Invalid listOfTopics (EMPTY){3}", campaign.name, pool.name, subSubPool.name, "\n"); }
                    else
                    {
                        //check listOfTopics for duplicates
                        List<string> tempList = subSubPool.listOfTopics.Select(x => x.name).ToList();
                        CheckListForDuplicates(tempList, "Topic", subSubPool.name, "listOfTopics");
                        //only check subSubPools of the same side as the campaign
                        if (subSubPool.subSubType.side.level == campaign.side.level)
                        {
                            isGoodTopicPresent = false;
                            isBadTopicPresent = false;
                            foreach (Topic topic in subSubPool.listOfTopics)
                            {
                                if (topic != null)
                                {
                                    //check each topic for the correct side
                                    if (topic.side.level != campaign.side.level)
                                    {
                                        Debug.LogFormat("[Val] ValidationManager.cs-> CheckCampaignPool: campaign \"{0}\", \"{1}\", topic \"{2}\", has INCORRECT SIDE (is {3}, should be {4}){5}",
                                          campaign.name, subSubPool.name, topic.name, topic.side.name, campaign.side.name, "\n");
                                    }
                                    //check each topic the correct subSubType
                                    if (topic.subSubType.name.Equals(subSubPool.subSubType.name, StringComparison.Ordinal) == false)
                                    {
                                        Debug.LogFormat("[Val] ValidationManager.cs-> CheckCampaignPool: campaign \"{0}\", \"{1}\", topic \"{2}\", has INCORRECT subSubType (is {3}, should be {4}){5}",
                                            campaign.name, subSubPool.name, topic.name, topic.subSubType.name, subSubPool.subSubType.name, "\n");
                                    }
                                    //check at least one good and one bad topic with 'Normal' profile present in pool (see DecisionTopic/DesignNotes in Keep) -> needs to be one of each present at all times
                                    //NOTE: -> checks first four characters for a match to 'Norm' (could be Norm2X, for example). Sep '`9
                                    if (topic.group.name.Equals("Good", StringComparison.Ordinal) == true)
                                    {
                                        /*if (topic.profile.name.Equals(normalProfile.name, StringComparison.Ordinal) == true)*/
                                        if (topic.profile.name.Substring(0, 4).Equals("Norm", StringComparison.Ordinal) == true)
                                        { isGoodTopicPresent = true; }
                                    }
                                    if (topic.group.name.Equals("Bad", StringComparison.Ordinal) == true)
                                    {
                                        /*if (topic.profile.name.Equals(normalProfile.name, StringComparison.Ordinal) == true)*/
                                        if (topic.profile.name.Substring(0, 4).Equals("Norm", StringComparison.Ordinal) == true)
                                        { isBadTopicPresent = true; }
                                    }
                                }
                                else
                                {
                                    Debug.LogFormat("[Val] ValidationManager.cs-> CheckCampaignPool: campaign \"{0}\", \"{1}\", topic \"{2}\" Invalid (Null){3}}",
                                        campaign.name, subSubPool.name, topic.name, "\n");
                                }
                            }
                            //Has one good and one bad topic present with 'Normal' profiles?
                            if (isGoodTopicPresent == false || isBadTopicPresent == false)
                            {
                                Debug.LogFormat("[Val] ValidationManager.cs-> CheckCampaignPool: campaign \"{0}\", \"{1}\", must have at least 1 Good & 1 Bad topics with \"{2}\" profiles present (is Good {3}, Bad {4}){5}",
                                    campaign.name, subSubPool.name, normalProfile.name, isGoodTopicPresent, isBadTopicPresent, "\n");
                            }
                        }
                    }
                    //must have identical subType as parent
                    if (subSubPool.subType.name.Equals(subType.name, StringComparison.Ordinal) == false)
                    {
                        Debug.LogFormat("[Val] ValidationManager.cs-> CheckCampaignPool: campaign \"{0}\", \"{1}\", {2} has Mismatching subTypes (is {3}, should be {4}){5}", campaign.name, pool.name,
                          subSubPool.name, subSubPool.subType.name, subType.name, "\n");
                    }

                }
                else { Debug.LogFormat("[Val] ValidationManager.cs-> CheckCampaignPool: campaign \"{0}\", \"{1}\", has invalid SubSubTypePool (Null){2}", campaign.name, pool.name, "\n"); }
            }
        }
        //loop all topics to check they are the correct side (campaign side)
        int count = pool.listOfTopics.Count;
        if (count > 0)
        {
            isGoodTopicPresent = false;
            isBadTopicPresent = false;
            for (int i = 0; i < count; i++)
            {
                Topic topic = pool.listOfTopics[i];
                if (topic != null)
                {
                    if (topic.side.level != campaign.side.level)
                    {
                        Debug.LogFormat("[Val] ValidationManager.cs-> CheckCampaignPool: campaign \"{0}\", \"{1}\", topic \"{2}\", has INCORRECT SIDE (is {3}, should be {4}){5}",
                          campaign.name, pool.name, topic.name, topic.side.name, campaign.side.name, "\n");
                    }
                    //check at least one good and one bad topic  present in pool
                    if (topic.group.name.Equals("Good", StringComparison.Ordinal) == true)
                    { isGoodTopicPresent = true; }
                    if (topic.group.name.Equals("Bad", StringComparison.Ordinal) == true)
                    { isBadTopicPresent = true; }
                }
                else
                {
                    Debug.LogFormat("[Val] ValidationManager.cs-> CheckCampaignPool: campaign \"{0}\", \"{1}\", topic \"{2}\" Invalid (Null){3}}",
                        campaign.name, pool.name, topic.name, "\n");
                }
                //check topic profile interval is equal to or greater than the subType min interval -> SubSubType's only
                if (pool.subSubType != null)
                {
                    if (topic.profile.delayRepeatFactor < pool.subType.minIntervalFactor)
                    {
                        Debug.LogFormat("[Val] ValidationManager.cs -> CheckCampaignPool: topic \"{0}\" has profile interval factor ({1}) LESS THAN subType \"{2}\" ({3}){4}", topic.name,
                          topic.profile.delayRepeatFactor, pool.subType.name, pool.subType.minIntervalFactor, "\n");
                    }
                }
            }
            //Has one good and one bad topic present?
            if (isGoodTopicPresent == false || isBadTopicPresent == false)
            {
                Debug.LogFormat("[Val] ValidationManager.cs-> CheckCampaignPool: campaign \"{0}\", \"{1}\", must have at least 1 Good & 1 Bad topics present (is Good {2}, Bad {3}){4}",
                    campaign.name, pool.name, isGoodTopicPresent, isBadTopicPresent, "\n");
            }
            //check for duplicates
            List<string> tempList = pool.listOfTopics.Select(i => i.name).ToList();
            CheckListForDuplicates(tempList, "Topic", pool.name, "listOfTopics");
        }
        else
        {
            //O.K to have no topics if there are subSubType pools that will be used to fill the pool via UpdateTopicPools
            if (pool.subType.listOfSubSubType.Count == 0)
            { Debug.LogFormat("[Val] ValidationManager.cs-> CheckCampaignPool: campaign \"{0}\", \"{1}\" is EMPTY{2}", campaign.name, pool.name, "\n"); }
        }
    }


    #endregion





    //new methods above here
}
