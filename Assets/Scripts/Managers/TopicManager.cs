using gameAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Human decision system
/// </summary>
public class TopicManager : MonoBehaviour
{
    [Tooltip("Minimum number of turns before topicType/SubTypes can be chosen again")]
    [Range(0, 10)] public int minIntervalGlobal = 2;
    [Tooltip("Maximum number of options in a topic")]
    [Range(2, 5)] public int maxOptions = 4;

    [Header("TopicSubSubTypes")]
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType BlowStuffUp;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType CreateRiots;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType DeployTeam;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType GainTargetInfo;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType HackSecurity;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType InsertTracer;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType NeutraliseTeam;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType ObtainGear;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType RecallTeam;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType RecruitActor;
    [Tooltip("Used to avoid having to hard code the TopicSubSubType.SO names")]
    public TopicSubSubType SpreadFakeNews;

    //info tags (topic specific info) -> reset to defaults each turn in ResetTopicAdmin prior to use
    private int tagActorID;

    //collections (local)
    private List<TopicType> listOfTopicTypesTurn = new List<TopicType>();                               //level topics that passed their turn checks
    private List<TopicType> listOfTypePool = new List<TopicType>();                                     //turn selection pool for topicTypes (priority based)
    private List<TopicSubType> listOfSubTypePool = new List<TopicSubType>();                            //turn selection pool for topicSubTypes (priority based)
    private List<Topic> listOfTopicPool = new List<Topic>();                                            //turn selection pool for topics (priority based)

    //Turn selection
    private TopicType turnTopicType;
    private TopicSubType turnTopicSubType;
    private TopicSubSubType turnTopicSubSubType;
    private Topic turnTopic;
    private TopicOption turnOption;                                                                     //option selected

    private int minIntervalGlobalActual;                                                                //number used in codes. Can be less than the minIntervalGlobal

    /// <summary>
    /// Initialisation
    /// </summary>
    /// <param name="state"></param>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseStartUp();
                SubInitialiseLevelStart();
                SubInitialiseFastAccess();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseLevelStart();
                break;
            case GameState.LoadAtStart:
                SubInitialiseStartUp();
                SubInitialiseFastAccess();
                break;
            case GameState.LoadGame:
                //do nothing
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\" (InitialiseLate)", GameManager.instance.inputScript.GameState);
                break;
        }
    }


    #region Initialisation SubMethods

    #region SubInitialiseStartUp
    private void SubInitialiseStartUp()
    {
        //calculates topicType/SubType minimum Intervals based on global setting (minTopicTypeTurns)
        List<TopicType> listOfTopicTypes = GameManager.instance.dataScript.GetListOfTopicTypes();
        if (listOfTopicTypes != null)
        {
            //loop topicTypes and update minIntervals
            for (int i = 0; i < listOfTopicTypes.Count; i++)
            {
                TopicType topicType = listOfTopicTypes[i];
                if (topicType != null)
                {
                    topicType.minInterval = topicType.minIntervalFactor * minIntervalGlobal;
                    //update topicTypeData minInteval
                    TopicTypeData dataType = GameManager.instance.dataScript.GetTopicTypeData(topicType.name);
                    if (dataType != null)
                    { dataType.minInterval = topicType.minInterval; }
                    else { Debug.LogWarningFormat("Invaid topicTypeData (Null) for topicType \"{0}\"", topicType.name); }
                    //get listOfSubTypes
                    if (topicType.listOfSubTypes != null)
                    {
                        //loop listOfSubTypes
                        for (int j = 0; j < topicType.listOfSubTypes.Count; j++)
                        {
                            TopicSubType subType = topicType.listOfSubTypes[j];
                            if (subType != null)
                            {
                                subType.minInterval = subType.minIntervalFactor * minIntervalGlobal;
                                //update topicTypeData minInteval
                                TopicTypeData dataSub = GameManager.instance.dataScript.GetTopicSubTypeData(subType.name);
                                if (dataSub != null)
                                { dataSub.minInterval = subType.minInterval; }
                                else { Debug.LogWarningFormat("Invaid topicTypeData (Null) for topicSubType \"{0}\"", subType.name); }
                            }
                            else { Debug.LogWarningFormat("Invalid topicSubType (Null) in listOfTopicSubType[{0}] for topicType \"{1}\"", j, topicType.name); }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid topicType.listOfSubTypes (Null) for \"{0}\"", topicType.name); }
                }
                else { Debug.LogWarningFormat("Invalid topicType (Null) in listOfTopicTypes[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid listOfTopicTypes (Null)"); }
    }
    #endregion

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
        //reset all topics to isCurrent False prior to changes
        GameManager.instance.dataScript.ResetTopics();
        //establish which TopicTypes are valid for the level. Initialise profile and status data.
        UpdateTopicPools();
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        Debug.Assert(BlowStuffUp != null, "Invalid BlowStuffUp (Null)");
        Debug.Assert(CreateRiots != null, "Invalid CreateRiots (Null)");
        Debug.Assert(DeployTeam != null, "Invalid DeployTeam (Null)");
        Debug.Assert(GainTargetInfo != null, "Invalid GainTargetInfo (Null)");
        Debug.Assert(HackSecurity != null, "Invalid HackSecurity (Null)");
        Debug.Assert(InsertTracer != null, "Invalid InsertTracer (Null)");
        Debug.Assert(NeutraliseTeam != null, "Invalid NeutraliseTeam (Null)");
        Debug.Assert(ObtainGear != null, "Invalid ObtainGear (Null)");
        Debug.Assert(RecallTeam != null, "Invalid RecallTeam (Null)");
        Debug.Assert(RecruitActor != null, "Invalid RecruitActor (Null)");
        Debug.Assert(SpreadFakeNews != null, "Invalid SpreadFakeNews (Null)");
    }
    #endregion

    #endregion

    #region Session Start
    //
    // - - - Session Start - - -
    //

    #region UpdateTopicPools
    /// <summary>
    /// Populates DataManager.cs -> dictOfTopicPools (at start of a level) with all valid topics and sets up listOfTopicTypesLevel for all topics that are valid for the current level
    /// </summary>
    public void UpdateTopicPools()
    {
        List<TopicType> listOfTopicTypesLevel = GameManager.instance.dataScript.GetListOfTopicTypesLevel();
        if (listOfTopicTypesLevel != null)
        {
            //get current topicTypes
            List<TopicType> listOfTopicTypes = GameManager.instance.dataScript.GetListOfTopicTypes();
            if (listOfTopicTypes != null)
            {
                //get current campaign
                Campaign campaign = GameManager.instance.campaignScript.campaign;
                if (campaign != null)
                {
                    //get current city
                    City city = GameManager.instance.campaignScript.scenario.city;
                    if (city != null)
                    {
                        string subTypeName;
                        //loop by subTypes
                        for (int i = 0; i < listOfTopicTypes.Count; i++)
                        {
                            TopicType topicType = listOfTopicTypes[i];
                            if (topicType != null)
                            {
                                GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
                                //only proceed with topicType if either the same as Player side or 'Both'
                                if (topicType.side.level == playerSide.level || topicType.side.level == 3)
                                {
                                    for (int j = 0; j < topicType.listOfSubTypes.Count; j++)
                                    {
                                        TopicSubType topicSubType = topicType.listOfSubTypes[j];
                                        if (topicSubType != null)
                                        {
                                            //subType must be playerSide of 'both'
                                            if (topicSubType.side.level == playerSide.level || topicSubType.side.level == 3)
                                            {
                                                subTypeName = topicSubType.name;
                                                bool isValid = false;

                                                switch (topicSubType.name)
                                                {
                                                    case "ActorPolitic":
                                                        if (campaign.actorPoliticPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.actorPoliticPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.actorPoliticPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorPoliticPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.actorPoliticPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ActorContact":
                                                        if (campaign.actorContactPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.actorContactPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.actorContactPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorContactPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.actorContactPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ActorDistrict":
                                                        if (campaign.actorDistrictPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.actorDistrictPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.actorDistrictPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorDistrictPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.actorDistrictPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ActorGear":
                                                        if (campaign.actorGearPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.actorGearPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.actorGearPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorGearPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.actorGearPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ActorMatch":
                                                        if (campaign.actorMatchPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.actorMatchPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.actorMatchPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorMatchPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.actorMatchPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "AuthorityCampaign":
                                                        if (campaign.authorityCampaignPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.authorityCampaignPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.authorityCampaignPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.authorityCampaignPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.authorityCampaignPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "AuthorityGeneral":
                                                        if (campaign.authorityGeneralPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.authorityGeneralPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.authorityGeneralPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.authorityGeneralPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.authorityGeneralPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "AuthorityTeam":
                                                        if (campaign.teamPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.teamPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.teamPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.teamPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.teamPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ResistanceCampaign":
                                                        if (campaign.resistanceCampaignPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.resistanceCampaignPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.resistanceCampaignPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.resistanceCampaignPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.resistanceCampaignPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ResistanceGeneral":
                                                        if (campaign.resistanceGeneralPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.resistanceGeneralPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.resistanceGeneralPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.resistanceGeneralPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.resistanceGeneralPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "CampaignAlpha":
                                                        if (campaign.campaignAlphaPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignAlphaPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignAlphaPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignAlphaPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignAlphaPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "CampaignBravo":
                                                        if (campaign.campaignBravoPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignBravoPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignBravoPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignBravoPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignBravoPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "CampaignCharlie":
                                                        if (campaign.campaignCharliePool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.campaignCharliePool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.campaignCharliePool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignCharliePool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignCharliePool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "CitySub":
                                                        switch (campaign.side.level)
                                                        {
                                                            case 1:
                                                                //Authority
                                                                if (city.cityPoolAuthority != null)
                                                                {
                                                                    //any subSubTypes present?
                                                                    if (city.cityPoolAuthority.listOfSubSubTypePools.Count > 0)
                                                                    { LoadSubSubTypePools(city.cityPoolAuthority, campaign.side); }
                                                                    //populate dictionary
                                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, city.cityPoolAuthority.listOfTopics);
                                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                                    SetTopicDynamicData(city.cityPoolAuthority.listOfTopics);
                                                                    isValid = true;
                                                                }
                                                                break;
                                                            case 2:
                                                                //Resistance
                                                                if (city.cityPoolResistance != null)
                                                                {
                                                                    //any subSubTypes present?
                                                                    if (city.cityPoolResistance.listOfSubSubTypePools.Count > 0)
                                                                    { LoadSubSubTypePools(city.cityPoolResistance, campaign.side); }
                                                                    //populate dictionary   
                                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, city.cityPoolResistance.listOfTopics);
                                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                                    SetTopicDynamicData(city.cityPoolResistance.listOfTopics);
                                                                    isValid = true;
                                                                }
                                                                break;
                                                            default:
                                                                Debug.LogWarningFormat("Unrecognised campaign side \"{0}\" for CitySub", campaign.side.name);
                                                                break;
                                                        }
                                                        break;
                                                    case "FamilyAlpha":
                                                        if (campaign.familyAlphaPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.familyAlphaPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.familyAlphaPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.familyAlphaPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.familyAlphaPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "FamilyBravo":
                                                        if (campaign.familyBravoPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.familyBravoPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.familyBravoPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.familyBravoPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.familyBravoPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "FamilyCharlie":
                                                        if (campaign.familyCharliePool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.familyCharliePool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.familyCharliePool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.familyCharliePool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.familyCharliePool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "HQSub":
                                                        if (campaign.hqPool != null)
                                                        {
                                                            //any subSubTypes present?
                                                            if (campaign.hqPool.listOfSubSubTypePools.Count > 0)
                                                            { LoadSubSubTypePools(campaign.hqPool, campaign.side); }
                                                            //populate dictionary
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.hqPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.hqPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    default:
                                                        Debug.LogWarningFormat("Unrecognised topicSubType \"{0}\" for topicType \"{1}\"", topicSubType.name, topicType.name);
                                                        break;
                                                }
                                                //set subType topicTypeData 'isAvailable' to appropriate value
                                                TopicTypeData data = GameManager.instance.dataScript.GetTopicSubTypeData(topicSubType.name);
                                                if (data != null)
                                                { data.isAvailable = isValid; }
                                                else
                                                { Debug.LogErrorFormat("Invalid topicTypeData (Null) for topicSubType \"{0}\"", topicSubType.name); }
                                            }
                                        }

                                        else { Debug.LogWarningFormat("Invalid topicSubType (Null) for topicType \"{0}\" in listOFSubTypes[{1}]", topicType.name, j); }
                                    }
                                }
                            }
                            else { Debug.LogWarningFormat("Invalid topicType (Null) for listOfTopicTypes[{0}]", i); }
                        }
                    }
                    else { Debug.LogError("Invalid City (Null)"); }
                }
                else { Debug.LogError("Invalid campaign (Null)"); }
            }
            else { Debug.LogError("Invalid listOfTopicTypes (Null)"); }
        }
        else { Debug.LogError("Invalid listOfTopicTypesLevel (Null)"); }
    }
    #endregion

    #region SetTopicDynamicData
    /// <summary>
    /// Initialises all dynamic profile data, sets 'isCurrent' to true, zero's stats  and sets status at session start for all topics in topic pool list
    /// </summary>
    /// <param name="listOfTopics"></param>
    private void SetTopicDynamicData(List<Topic> listOfTopics)
    {
        if (listOfTopics != null)
        {
            for (int i = 0; i < listOfTopics.Count; i++)
            {
                Topic topic = listOfTopics[i];
                if (topic != null)
                {
                    TopicProfile profile = topic.profile;
                    if (profile != null)
                    {
                        //set timers
                        topic.timerRepeat = profile.delayRepeat;
                        topic.timerStart = profile.delayStart;
                        topic.timerWindow = profile.timerWindow;
                        //set status
                        if (topic.timerStart == 0)
                        { topic.status = Status.Active; }
                        else { topic.status = Status.Dormant; }
                        //isCurrent (all topics set to false prior to changes by SubInitialiseLevelStart
                        topic.isCurrent = true;
                        //zero status
                        topic.turnsDormant = 0;
                        topic.turnsActive = 0;
                        topic.turnsLive = 0;
                        topic.turnsDone = 0;
                    }
                    else { Debug.LogWarningFormat("Invalid profile (Null) for topic \"{0}\"", topic.name); }
                }
                else { Debug.LogWarningFormat("Invalid topic (Null) for listOfTopics[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid listOfTopics (Null)"); }
    }
    #endregion

    #endregion

    #region Select Topic
    //
    //  - - - Select Topic - - -
    //   

    /// <summary>
    /// Selects a topic for the turn (there will always be one)
    /// </summary>
    public void SelectTopic(GlobalSide playerSide)
    {
        if (playerSide != null)
        {
            //Player must be Active
            if (CheckPlayerStatus(playerSide) == true)
            {
                ResetTopicAdmin();
                //select a topic, if none found then drop the global interval by 1 and try again
                do
                {
                    CheckTopics();
                    CheckForValidTopicTypes();
                    GetTopicType();
                    GetTopicSubType(playerSide);
                    GetTopic(playerSide);
                    //repeat process with a reduced minInterval
                    if (turnTopic == null)
                    {
                        minIntervalGlobalActual--;
                        Debug.LogFormat("[Tst] TopicManager.cs -> SelectTopic: REPEAT LOOP, minIntervalGlobalActual now {0}{1}", minIntervalGlobalActual, "\n");
                    }
                    else { break; }
                }
                while (turnTopic == null && minIntervalGlobalActual > 0);
                //debug purposes only -> BEFORE UpdateTopicTypeData
                UnitTestTopic(playerSide);
                //debug -> should be in ProcessTopic but here for autorun debugging purposes
                UpdateTopicTypeData();
            }
        }
        else { Debug.LogError("Invalid playerSide (Null)"); }
    }

    #region CheckTopics
    /// <summary>
    /// checks all current topic timers and criteria and adjusts status if required
    /// </summary>
    private void CheckTopics()
    {
        Dictionary<string, Topic> dictOfTopics = GameManager.instance.dataScript.GetDictOfTopics();
        if (dictOfTopics != null)
        {
            foreach (var topic in dictOfTopics)
            {
                //current topics for level only
                if (topic.Value != null)
                {
                    if (topic.Value.isCurrent == true)
                    {
                        //
                        // - - - Criteria checks (done BEFORE timer checks for sequence issues, otherwise a start timer delay of 5 turns works out to be only 4)
                        //
                        switch (topic.Value.status)
                        {
                            case Status.Dormant:
                                //do nothing
                                break;
                            case Status.Active:
                                //check criteria (even they're aren't any)
                                if (CheckTopicCriteria(topic.Value) == true)
                                {
                                    //Criteria O.K, status Live
                                    topic.Value.status = Status.Live;
                                }
                                break;
                            case Status.Live:
                                if (CheckTopicCriteria(topic.Value) == false)
                                {
                                    //Criteria FAILED, status Active
                                    topic.Value.status = Status.Active;
                                }
                                break;
                            case Status.Done:
                                //do nothing
                                break;
                            default:
                                Debug.LogWarningFormat("Unrecognised status \"{0}\" for topic \"{1}\"", topic.Value.status, topic.Value.name);
                                break;
                        }
                        //
                        // - - - Timer checks
                        //
                        switch (topic.Value.status)
                        {
                            case Status.Dormant:
                                //Start timer (turns before activating at level start)
                                if (topic.Value.timerStart > 0)
                                {
                                    //decrement timer
                                    topic.Value.timerStart--;
                                    //if Zero, status Active (no need to reset timer as it's a once only use)
                                    if (topic.Value.timerStart == 0)
                                    { topic.Value.status = Status.Active; }
                                }
                                else if (topic.Value.turnsLive > 0 && topic.Value.timerRepeat > 0)
                                {
                                    //Repeat topic (has already been Live at least once)
                                    topic.Value.timerRepeat--;
                                    //if Zero, status Active
                                    if (topic.Value.timerRepeat == 0)
                                    {
                                        topic.Value.status = Status.Active;
                                        //reset repeat timer ready for next time (profile assumed to be not Null due to SO OnEnable checks)
                                        topic.Value.timerRepeat = topic.Value.profile.delayRepeat;
                                    }
                                }
                                else
                                {
                                    //no timers -> status Done
                                    topic.Value.status = Status.Done;
                                }
                                break;
                            case Status.Active:
                                //do nothing
                                break;
                            case Status.Live:
                                //Window timer
                                if (topic.Value.timerWindow > 0)
                                {
                                    //decrement timer
                                    topic.Value.timerWindow--;
                                    //if Zero, status Dormant
                                    if (topic.Value.timerWindow == 0)
                                    {
                                        topic.Value.status = Status.Dormant;
                                        //reset timer ready for next time
                                        topic.Value.timerWindow = topic.Value.profile.timerWindow;
                                    }
                                }
                                break;
                            case Status.Done:
                                //do nothing
                                break;
                            default:
                                Debug.LogWarningFormat("Unrecognised status \"{0}\" for topic \"{1}\"", topic.Value.status, topic.Value.name);
                                break;
                        }
                        //
                        // - - - Stat's update
                        //
                        switch (topic.Value.status)
                        {
                            case Status.Dormant:
                                topic.Value.turnsDormant++;
                                break;
                            case Status.Active:
                                topic.Value.turnsActive++;
                                break;
                            case Status.Live:
                                topic.Value.turnsLive++;
                                break;
                            case Status.Done:
                                topic.Value.turnsDone++;
                                break;
                            default:
                                Debug.LogWarningFormat("Unrecognised status \"{0}\" for topic \"{1}\"", topic.Value.status, topic.Value.name);
                                break;
                        }
                    }
                }
                else { Debug.LogWarningFormat("Invalid topic \"{0}\" (Null) in dictOfTopics", topic.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfTopics (Null)"); }
    }
    #endregion

    #region CheckTopicCriteria
    /// <summary>
    /// Check a topic's criteria, returns true if all criteria O.K (or none if first instance), false if any failed
    /// NOTE: topic checked for Null by calling method
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    private bool CheckTopicCriteria(Topic topic)
    {
        bool isCheck = false;
        //topic criteria must pass checks
        if (topic.listOfCriteria != null && topic.listOfCriteria.Count > 0)
        {
            CriteriaDataInput criteriaInput = new CriteriaDataInput()
            { listOfCriteria = topic.listOfCriteria };
            string criteriaCheck = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
            if (criteriaCheck == null)
            {
                //criteria check passed O.K
                isCheck = true;
            }
            else
            {
                //criteria check FAILED
                isCheck = false;

                /*//generate message explaining why criteria failed -> debug only, spam otherwise
                Debug.LogFormat("[Tst] TopicManager.cs -> GetTopic: topic \"{0}\", Criteria FAILED \"{1}\"{2}", topic.name, criteriaCheck, "\n");*/
            }
        }
        else
        {
            //no criteria present
            isCheck = true;
        }
        return isCheck;
    }
    #endregion

    #region CheckForValidTopicType
    /// <summary>
    /// Check all level topic types to see if they are valid for the Turn. Updates listOfTopicTypesTurn with valid topicTypes
    /// TopicType.side already taken into account with listOfTopicTypesLevel
    /// </summary>
    private void CheckForValidTopicTypes()
    {
        List<TopicType> listOfTopicTypesLevel = GameManager.instance.dataScript.GetListOfTopicTypesLevel();
        if (listOfTopicTypesLevel != null)
        {
            string criteriaCheck;
            int turn = GameManager.instance.turnScript.Turn;
            //loop list of Topic Types
            foreach (TopicType topicType in listOfTopicTypesLevel)
            {
                TopicTypeData topicTypeData = GameManager.instance.dataScript.GetTopicTypeData(topicType.name);
                if (topicTypeData != null)
                {
                    //check topicTypeData
                    if (CheckTopicTypeData(topicTypeData, turn) == true)
                    {
                        //check individual topicType criteria
                        CriteriaDataInput criteriaInput = new CriteriaDataInput()
                        { listOfCriteria = topicType.listOfCriteria };
                        criteriaCheck = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
                        if (criteriaCheck == null)
                        {
                            //criteria check passed O.K
                            listOfTopicTypesTurn.Add(topicType);
                            //add to local list of valid TopicTypes for the Turn
                            AddTopicTypeToList(listOfTopicTypesTurn, topicType);
                        }
                        else
                        {
                            //criteria check FAILED

                            /*//generate message explaining why criteria failed -> debug only, spam otherwise
                            Debug.LogFormat("[Top] TopicManager.cs -> CheckForValidTopics: topicType \"{0}\" {1} Criteria check{2}", topicType.tag, criteriaCheck, "\n");*/
                        }
                    }
                    /*else { Debug.LogFormat("[Top] TopicManager.cs -> CheckForValidTopics: topicType \"{0}\" Failed TopicTypeData check{1}", topicType.tag, "\n"); }*/
                }
                else { Debug.LogError("Invalid topicTypeData (Null)"); }
            }
        }
        else { Debug.LogError("Invalid listOfTopicTypesLevel (Null)"); }
    }
    #endregion

    #region ResetTopicAdmin
    /// <summary>
    /// Resets all relevant turn topic data prior to processing for the current turn
    /// </summary>
    private void ResetTopicAdmin()
    {
        //minInterval
        minIntervalGlobalActual = minIntervalGlobal;
        //selected topic data
        turnTopicType = null;
        turnTopicSubType = null;
        turnTopicSubSubType = null;
        turnTopic = null;
        //info tags
        tagActorID = -1;
        //empty collections
        listOfTopicTypesTurn.Clear();
        listOfTypePool.Clear();
        listOfSubTypePool.Clear();
        listOfTopicPool.Clear();
    }
    #endregion

    #region GetTopicType
    /// <summary>
    /// Get topicType for turn Decision
    /// </summary>
    private void GetTopicType()
    {
        int numOfEntries;
        int count = listOfTopicTypesTurn.Count;
        if (count > 0)
        {
            //build up a pool
            foreach (TopicType topicType in listOfTopicTypesTurn)
            {
                //populate pool based on priority
                numOfEntries = GetNumOfEntries(topicType.priority);
                if (numOfEntries > 0)
                {
                    for (int i = 0; i < numOfEntries; i++)
                    { listOfTypePool.Add(topicType); }
                }
            }
            if (listOfTypePool.Count > 0)
            {
                //random draw of pool
                turnTopicType = listOfTypePool[Random.Range(0, listOfTypePool.Count)];
            }
            else { Debug.LogError("Invalid listOfTypePool (Empty) for selecting topicType"); }
        }
    }
    #endregion

    #region GetTopicSubType
    /// <summary>
    /// Get topicSubType for turn Decision
    /// Note: playerSide checked for null by parent method
    /// </summary>
    private void GetTopicSubType(GlobalSide playerSide)
    {
        int numOfEntries;
        bool isProceed;
        if (turnTopicType != null)
        {
            //loop all subTypes for topicType
            for (int i = 0; i < turnTopicType.listOfSubTypes.Count; i++)
            {
                TopicSubType subType = turnTopicType.listOfSubTypes[i];
                if (subType != null)
                {
                    isProceed = false;
                    //check for same side as Player or 'both', ignore otherwise
                    if (subType.side.level == playerSide.level || subType.side.level == 3)
                    {
                        //check TopicSubTypes isValid (topicType may have been approved with some subTypes O.K and others not)
                        TopicTypeData dataSub = GameManager.instance.dataScript.GetTopicSubTypeData(subType.name);
                        if (dataSub != null)
                        {
                            if (CheckTopicTypeData(dataSub, GameManager.instance.turnScript.Turn, true) == true)
                            {
                                //check that there are Live topics available for subType
                                List<Topic> listOfTopics = GameManager.instance.dataScript.GetListOfTopics(subType);
                                if (listOfTopics != null)
                                {
                                    //loop topics looking for the first valid topic (only need one) in order to validate subType
                                    for (int k = 0; k < listOfTopics.Count; k++)
                                    {
                                        Topic topic = listOfTopics[k];
                                        if (topic != null)
                                        {
                                            if (topic.status == Status.Live)
                                            {
                                                isProceed = true;
                                                break;
                                            }
                                        }
                                        else { Debug.LogWarningFormat("Invalid topic (Null) for subType \"{0}\"", subType.name); }
                                    }
                                    if (isProceed == true)
                                    {
                                        //populate pool based on priorities
                                        numOfEntries = GetNumOfEntries(subType.priority);
                                        if (numOfEntries > 0)
                                        {
                                            for (int j = 0; j < numOfEntries; j++)
                                            { listOfSubTypePool.Add(subType); }
                                        }
                                    }
                                }
                                else { Debug.LogErrorFormat("Invalid listOfTopics (Null) for topicSubType \"{0}\"", subType.name); }
                            }
                        }
                        else { Debug.LogErrorFormat("Invalid dataTopic (Null) for topicSubType \"{0}\"", subType.name); }
                    }
                }
                else { Debug.LogWarningFormat("Invalid subType (Null) for topicType \"{0}\"", turnTopicType.name); }
            }
            if (listOfSubTypePool.Count > 0)
            {
                //random draw of pool
                turnTopicSubType = listOfSubTypePool[Random.Range(0, listOfSubTypePool.Count)];
            }
            else { Debug.LogError("Invalid listOfSubTypePool (Empty) for topicSubType selection"); }
        }
        //O.K for there to be no valid topic
    }
    #endregion

    #region GetTopic
    /// <summary>
    /// Get individual topic for turn Decision
    /// </summary>
    private void GetTopic(GlobalSide playerSide)
    {
        int numOfEntries;
        if (turnTopicSubType != null)
        {
            List<Topic> listOfPotentialTopics = new List<Topic>();
            List<Topic> listOfSubTypeTopics = GameManager.instance.dataScript.GetListOfTopics(turnTopicSubType);
            if (listOfSubTypeTopics != null)
            {
                switch (turnTopicSubType.name)
                {
                    //Standard topics
                    case "CampaignAlpha":
                    case "CampaignBravo":
                    case "CampaignCharlie":
                    case "ResistanceCampaign":
                    case "ResistanceGeneral":
                    case "AuthorityCampaign":
                    case "AuthorityGeneral":
                    case "FamilyAlpha":
                    case "FamilyBravo":
                    case "FamilyCharlie":
                        listOfPotentialTopics = listOfSubTypeTopics;
                        break;
                    //Dynamic topic
                    case "AuthorityTeam":
                    case "CitySub":
                    case "HQSub":
                        listOfPotentialTopics = listOfSubTypeTopics;
                        break;
                    case "ActorGear":
                        listOfPotentialTopics = GetActorGearTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "ActorPolitic":
                        listOfPotentialTopics = GetActorPoliticTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "ActorContact":
                        listOfPotentialTopics = GetActorContactTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "ActorMatch":
                        listOfPotentialTopics = GetActorMatchTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    case "ActorDistrict":
                        listOfPotentialTopics = GetActorDistrictTopics(listOfSubTypeTopics, playerSide, turnTopicSubType.name);
                        break;
                    default:
                        Debug.LogWarningFormat("Unrecognised topicSubType \"{0}\" for topic \"{1}\"", turnTopicSubType.name, turnTopic.name);
                        break;
                }
                //SubType should have provided a list of topics ready for a random draw
                if (listOfPotentialTopics != null)
                {
                    int count = listOfPotentialTopics.Count;
                    //shouldn't be empty
                    if (count > 0)
                    {
                        //loop topics
                        for (int i = 0; i < count; i++)
                        {
                            Topic topic = listOfPotentialTopics[i];
                            if (topic != null)
                            {
                                //check topic Live (timer and status checks O.K)
                                if (topic.status == Status.Live)
                                {
                                    //populate pool based on priorities
                                    numOfEntries = GetNumOfEntries(topic.priority);
                                    if (numOfEntries > 0)
                                    {
                                        for (int j = 0; j < numOfEntries; j++)
                                        { listOfTopicPool.Add(topic); }
                                    }
                                }
                            }
                            else { Debug.LogWarningFormat("Invalid topic (Null) for {0}.listOfTopics[{1}]", turnTopicSubType.name, i); }
                        }
                        //Select topic
                        count = listOfTopicPool.Count;
                        if (count > 0)
                        {
                            turnTopic = listOfTopicPool[Random.Range(0, count)];
                            Debug.LogFormat("[Top] TopicManager.cs -> GetTopic: t {0}, {1} -> {2} -> {3}{4}", GameManager.instance.turnScript.Turn, turnTopicType.name, turnTopicSubType.name, turnTopic.name, "\n");
                        }
                        else { Debug.LogWarningFormat("Invalid listOfTopicPool (EMPTY) for topicSubType \"{0}\"", turnTopicSubType.name); }
                    }
                    else { Debug.LogWarningFormat("Invalid listOfTopics (EMPTY) for topicSubType \"{0}\"", turnTopicSubType.name); }
                }
                else { Debug.LogErrorFormat("Invalid listOfTopics (Null) for topicSubType \"{0}\"", turnTopicSubType.name); }
            }
            else { Debug.LogErrorFormat("Invalid listOfSubTypeTopics (Null) for topicSubType \"{0}\"", turnTopicSubType.name); }
        }
        else
        {
            //No topic selected
            Debug.LogFormat("[Top] TopicManager.cs -> GetTopic: t {0}, No Topic Selected{1}", GameManager.instance.turnScript.Turn, "\n");
        }
    }
    #endregion

    #endregion

    #region Dynamic Topics
    //
    // - - - Select Dynamic Topics
    //

    #region GetActorContactTopics
    /// <summary>
    /// subType ActorContact template topics selected by actor / motivation (good/bad group). Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <returns></returns>
    private List<Topic> GetActorContactTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //get all active, onMap actors
        List<Actor> listOfActors = GameManager.instance.dataScript.GetActiveActors(playerSide);
        if (listOfActors != null)
        {
            if (listOfActors.Count > 0)
            {
                //select an actor
                Actor actor = listOfActors[Random.Range(0, listOfActors.Count)];
                if (actor != null)
                {
                    //get actor motivation
                    int motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
                    switch (motivation)
                    {
                        case 3: group = GroupType.Good; break;
                        case 2: group = GroupType.Neutral; break;
                        case 1: group = GroupType.Bad; break;
                        case 0: group = GroupType.VeryBad; break;
                        default: Debug.LogWarningFormat("Unrecognised motivation \"[0}\" for actor {1}, {2}", motivation, actor.actorName, actor.arc.name); break;
                    }
                    //if no entries use entire list by default
                    listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
                    //Info tags
                    tagActorID = actor.actorID;
                }
                else { Debug.LogWarning("Invalid actor (Null) randomly selected from listOfActors for actorContact subType"); }
            }
        }
        else { Debug.LogWarning("Invalid listOfActors (Null) for ActorContact subType"); }
        return listOfTopics;
    }
    #endregion

    #region GetActorMatchTopics
    /// <summary>
    /// subType ActorMatch template topics selected by actor compatibility (good/bad group). Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <returns></returns>
    private List<Topic> GetActorMatchTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //get all active, onMap actors
        List<Actor> listOfActors = GameManager.instance.dataScript.GetActiveActors(playerSide);
        if (listOfActors != null)
        {
            List<Actor> selectionList = new List<Actor>();
            int compatibility;
            foreach (Actor actor in listOfActors)
            {
                if (actor != null)
                {
                    //filter actors for compatibility != 0
                    compatibility = actor.GetPersonality().GetCompatibilityWithPlayer();
                    if (compatibility != 0)
                    {
                        //seed actor selection pool based on ABS(compatility), eg. 2 entries for compatibility +/-2 and 1 for compability +/-1
                        for (int i = 0; i < Mathf.Abs(compatibility); i++)
                        { selectionList.Add(actor); }
                    }
                }
                else { Debug.LogWarning("Invalid Actor (Null) in listOfActors"); }
            }
            //check at least one entry in selectionList
            if (selectionList.Count > 0)
            {
                //randomly select an actor from compatibility weighted list
                Actor actor = selectionList[Random.Range(0, selectionList.Count)];
                compatibility = actor.GetPersonality().GetCompatibilityWithPlayer();
                switch (compatibility)
                {
                    case 3: group = GroupType.Good; break;
                    case 2: group = GroupType.Good; break;
                    case 1: group = GroupType.Good; break;
                    case -1: group = GroupType.Bad; break;
                    case -2: group = GroupType.VeryBad; break;
                    case -3: group = GroupType.VeryBad; break;
                    default: Debug.LogWarningFormat("Unrecognised compatibility \"{0}\" for {1}, {2}", compatibility, actor.actorName, actor.arc.name); break;
                }
                //if no entries use entire list by default
                listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
                //Debug
                if (listOfTopics.Count == 0)
                {
                    Debug.LogWarning("ActorMatch count 0");
                    Debug.LogFormat("[Tst] TopicManager.cs -> GetActorMatchTopic: listOfSubTopics.Count {0}, group {1}{2}", listOfSubTypeTopics.Count, group, "\n");
                    foreach (Topic topic in listOfSubTypeTopics)
                    { Debug.LogFormat("[Tst] TopicManager.cs -> GetActorMatchTopic: topic \"{0}\", status {1}{2}", topic.name, topic.status, "\n"); }
                }
                //Info tags
                tagActorID = actor.actorID;
            }
        }
        else { Debug.LogWarning("Invalid listOfActors (Null) for ActorMatch subType"); }
        return listOfTopics;
    }
    #endregion

    #region GetActorPoliticTopics
    /// <summary>
    /// subType ActorPolitic template topics selected by random actor based on motivation (good/bad group). Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <returns></returns>
    private List<Topic> GetActorPoliticTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //get all active, onMap actors
        List<Actor> listOfActors = GameManager.instance.dataScript.GetActiveActors(playerSide);
        if (listOfActors != null)
        {
            List<Actor> selectionList = new List<Actor>();
            int motivation;
            int numOfEntries = 0;
            int numOfActors = 0;
            foreach (Actor actor in listOfActors)
            {
                if (actor != null)
                {
                    numOfActors++;
                    //seed selection pool by motivation (the further off neutral their motivation, the more entries they get)
                    motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
                    switch (motivation)
                    {
                        case 3: numOfEntries = 2; break;
                        case 2: numOfEntries = 1; break;
                        case 1: numOfEntries = 2; break;
                        case 0: numOfEntries = 3; break;
                    }
                    //populate selection pool
                    for (int i = 0; i < numOfEntries; i++)
                    { selectionList.Add(actor); }

                }
                else { Debug.LogWarning("Invalid Actor (Null) in listOfActors"); }
            }
            //check at least two actors present (need one actor interacting with another)
            if (selectionList.Count > 0 && numOfActors > 1)
            {
                //randomly select an actor from unweighted list
                Actor actor = selectionList[Random.Range(0, selectionList.Count)];
                motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
                switch (motivation)
                {
                    case 3: group = GroupType.Good; break;
                    case 2: group = GroupType.Neutral; break;
                    case 1: group = GroupType.Bad; break;
                    case 0: group = GroupType.VeryBad; break;
                    default: Debug.LogWarningFormat("Unrecognised motivation \"{0}\" for {1}, {2}", motivation, actor.actorName, actor.arc.name); break;
                }
                //if no entries use entire list by default
                listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
                //Info tags
                tagActorID = actor.actorID;
            }
        }
        else { Debug.LogWarning("Invalid listOfActors (Null) for ActorPolitic subType"); }
        return listOfTopics;
    }
    #endregion

    #region GetActorGearTopics
    /// <summary>
    /// subType ActorGear template topics selected by random actor based on motivation (good/bad group). Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <returns></returns>
    private List<Topic> GetActorGearTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //get all active, onMap actors
        List<Actor> listOfActors = GameManager.instance.dataScript.GetActiveActors(playerSide);
        if (listOfActors != null)
        {
            List<Actor> selectionList = new List<Actor>();
            int motivation;
            int numOfEntries = 0;
            int numOfActors = 0;
            foreach (Actor actor in listOfActors)
            {
                if (actor != null)
                {
                    numOfActors++;
                    //seed selection pool by motivation (the further off neutral their motivation, the more entries they get)
                    motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
                    switch (motivation)
                    {
                        case 3: numOfEntries = 2; break;
                        case 2: numOfEntries = 1; break;
                        case 1: numOfEntries = 2; break;
                        case 0: numOfEntries = 3; break;
                    }
                    //populate selection pool
                    for (int i = 0; i < numOfEntries; i++)
                    { selectionList.Add(actor); }
                }
                else { Debug.LogWarning("Invalid Actor (Null) in listOfActors"); }
            }
            //check at least two actors present (need one actor interacting with another)
            if (selectionList.Count > 0 && numOfActors > 1)
            {
                //randomly select an actor from unweighted list
                Actor actor = selectionList[Random.Range(0, selectionList.Count)];
                motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
                switch (motivation)
                {
                    case 3: group = GroupType.Good; break;
                    case 2: group = GroupType.Neutral; break;
                    case 1: group = GroupType.Bad; break;
                    case 0: group = GroupType.VeryBad; break;
                    default: Debug.LogWarningFormat("Unrecognised motivation \"{0}\" for {1}, {2}", motivation, actor.actorName, actor.arc.name); break;
                }
                //if no entries use entire list by default
                listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName);
                //Info tags
                tagActorID = actor.actorID;
            }
        }
        else { Debug.LogWarning("Invalid listOfActors (Null) for ActorGear subType"); }
        return listOfTopics;
    }
    #endregion

    #region GetActorDistrictTopics
    /// <summary>
    /// subType ActorDistrict template topics selected by random actor based on motivation (good/bad group). Returns a list of suitable Live topics. Returns EMPTY if none found.
    /// NOTE: listOfSubTypeTopics and playerSide checked for Null by the parent method
    /// </summary>
    /// <param name="listOfSubTypeTopics"></param>
    /// <param name="playerSide"></param>
    /// <param name="subTypeName"></param>
    /// <returns></returns>
    private List<Topic> GetActorDistrictTopics(List<Topic> listOfSubTypeTopics, GlobalSide playerSide, string subTypeName = "Unknown")
    {
        int count, motivation;
        GroupType group = GroupType.Neutral;
        List<Topic> listOfTopics = new List<Topic>();
        //Get all actors with at least one district action available
        List<Actor> listOfActors = GameManager.instance.dataScript.GetActiveActorsSpecial(ActorCheck.NodeActionsNOTZero, playerSide);
        count = listOfActors.Count;
        if (count > 0)
        {
            //loop list and put any actor with a viable subSubType topic pool (consider recent NodeAction only) up for random selection
            List<Actor> listOfSelection = new List<Actor>();
            for (int i = 0; i < count; i++)
            {
                Actor actorTemp = listOfActors[i];
                if (actorTemp != null)
                {
                    NodeActionData dataTemp = actorTemp.GetMostRecentNodeAction();
                    turnTopicSubSubType = GetTopicSubSubType(dataTemp.nodeAction);
                    if (turnTopicSubSubType != null)
                    {

                        //check topics of this subSubType present
                        if (CheckSubSubTypeTopicsPresent(listOfSubTypeTopics, turnTopicSubSubType.name) == true)
                        {
                            //add to selection pool
                            listOfSelection.Add(actorTemp);
                            Debug.LogFormat("[Tst] TopicManager.cs -> GetActorDistrictTopics: {0}, {1}, actorID {2} has a valid ActionTopic \"{3}\"{4}", actorTemp.actorName, actorTemp.arc.name,
                                actorTemp.actorID, turnTopicSubSubType.name, "\n");
                        }
                        else
                        {
                            Debug.LogFormat("[Tst] TopicManager.cs -> GetActorDistrictTopics: {0}, {1}, actorID {2} has an INVALID ActionTopic \"{3}\"{4}", actorTemp.actorName, actorTemp.arc.name,
                                actorTemp.actorID, turnTopicSubSubType.name, "\n");
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid TopicSubSubType (Null) for nodeAction \"{0}\"", dataTemp.nodeAction); }
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for listOfActors[{0}]", i); }
            }
            //select a random actor (all in pool have at least one matching topics present)
            count = listOfSelection.Count;
            if (count > 0)
            {
                Actor actor = listOfSelection[Random.Range(0, count)];
                //get the most recent actor node action
                NodeActionData data = actor.GetMostRecentNodeAction();
                if (data != null)
                {
                    //group depends on actor motivation
                    motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
                    switch (motivation)
                    {
                        case 3: group = GroupType.Good; break;
                        case 2: group = GroupType.Neutral; break;
                        case 1: group = GroupType.Bad; break;
                        case 0: group = GroupType.VeryBad; break;
                        default: Debug.LogWarningFormat("Unrecognised motivation \"{0}\" for {1}, {2}", motivation, actor.actorName, actor.arc.name); break;
                    }
                    //if no entries use entire list by default
                    listOfTopics = GetTopicGroup(listOfSubTypeTopics, group, subTypeName, turnTopicSubSubType.name);
                    //debug
                    foreach (Topic topic in listOfTopics)
                    { Debug.LogFormat("[Tst] TopicManager.cs -> GetActorDistrictTopic: listOfTopics -> {0}, turn {1}{2}", topic.name, GameManager.instance.turnScript.Turn, "\n"); }
                }
                else { Debug.LogErrorFormat("Invalid nodeActionData (Null) for {0}, {1}, actorID {2}", actor.actorName, actor.arc.name, actor.actorID); }
                //Info tags
                tagActorID = actor.actorID;
            }
            else { Debug.LogFormat("[Tst] TopicManager.cs -> GetActorDistrictTopics: No topics found for ActorDistrict actions for turn {0}{1}", GameManager.instance.turnScript.Turn, "\n"); }
        }
        else { Debug.LogWarning("No active, onMap actors present with at least one NodeAction"); }
        return listOfTopics;
    }

    #endregion


    #endregion

    #region ProcessTopic
    //
    // - - - Process Topic - - - 
    //

    /// <summary>
    /// Process selected topic (decision or Information)
    /// </summary>
    public void ProcessTopic(GlobalSide playerSide)
    {
        if (playerSide != null)
        {
            //only if Player Active
            if (CheckPlayerStatus(playerSide) == true)
            {
                ExecuteTopic();
                UpdateTopicStatus();
            }
        }
        else { Debug.LogError("Invalid playerSide (Null)"); }
    }

    /// <summary>
    /// Takes selected topic and executes according to topic subType
    /// </summary>
    private void ExecuteTopic()
    {
        if (turnTopic != null)
        {
            //TO DO -> execution of topics (branches to a method that returns a topic data package ready to send to UI)

            //Processing of topic depends on whether it's a standard subType or a dynamic, template, subType
            switch (turnTopicSubType.name)
            {
                //Standard topics
                case "CampaignAlpha":
                case "CampaignBravo":
                case "CampaignCharlie":
                case "ResistanceCampaign":
                case "ResistanceGeneral":
                case "AuthorityCampaign":
                case "AuthorityGeneral":
                case "FamilyAlpha":
                case "FamilyBravo":
                case "FamilyCharlie":

                    break;
                //Dynamic topic
                case "ActorPolitic":

                    break;
                case "ActorContact":

                    break;
                case "ActorDistrict":

                    break;
                case "ActorGear":

                    break;
                case "ActorMatch":

                    break;

                case "AuthorityTeam":

                    break;

                case "CitySub":

                    break;

                case "HQSub":

                    break;
                default:
                    Debug.LogWarningFormat("Unrecognised topicSubType \"{0}\" for topic \"{1}\"", turnTopicSubType.name, turnTopic.name);
                    break;
            }

            // TO DO -> before updating stats check that topic data package from above is O.K



            // TO DO -> send topic data package to UI for display and user interaction
        }
        else { Debug.LogError("Invalid turnTopic (Null) -> No decision generated this turn"); }
    }

    /// <summary>
    /// handles all admin once a topic has been displayed and the user has chosen an option (or not, then default option selected)
    /// </summary>
    private void UpdateTopicTypeData()
    {
        if (turnTopic != null)
        {
            int turn = GameManager.instance.turnScript.Turn;
            //update TopicType topicTypeData
            TopicTypeData typeData = GameManager.instance.dataScript.GetTopicTypeData(turnTopicType.name);
            if (typeData != null)
            {
                typeData.timesUsedLevel++;
                typeData.turnLastUsed = turn;
            }
            else { Debug.LogErrorFormat("Invalid topicTypeData (Null) for turnTopicType \"{0}\"", turnTopicType.name); }
            //update TopicSubType topicTypeData
            TopicTypeData typeSubData = GameManager.instance.dataScript.GetTopicSubTypeData(turnTopicSubType.name);
            if (typeSubData != null)
            {
                typeSubData.timesUsedLevel++;
                typeSubData.turnLastUsed = turn;
            }
            else { Debug.LogErrorFormat("Invalid topicTypeData (Null) for turnTopicType \"{0}\"", turnTopicType.name); }
            //NodeAction
            if (turnTopicSubSubType != null)
            {
                //actor subSubTopic?
                if (turnTopicSubType.name.Equals("Actor", StringComparison.Ordinal) == true)
                {
                    //get actor and delete most recent NodeAction record
                    Actor actor = GameManager.instance.dataScript.GetActor(tagActorID);
                    if (actor != null)
                    { actor.RemoveMostRecentNodeAction(); }
                    else { Debug.LogErrorFormat("Invalid actor (Null) for tagActorID {0}", tagActorID); }
                }
            }
            //topicHistory
            HistoryTopic history = new HistoryTopic()
            {
                turn = turn,
                numSelect = listOfTopicTypesTurn.Count,
                topicType = turnTopicType.name,
                topicSubType = turnTopicSubType.name,
                topic = turnTopic.name,
                option = "?"

                //TO DO -> option selected

            };
            GameManager.instance.dataScript.AddTopicHistory(history);
        }
        //no need to generate warning message as covered elsewhere
    }

    /// <summary>
    /// Update status for selected topic once interaction complete
    /// </summary>
    private void UpdateTopicStatus()
    {
        if (turnTopic != null)
        {
            //Back to Dormant if repeat, Done otherwise
            if (turnTopic.timerRepeat > 0)
            { turnTopic.status = Status.Dormant; }
            else { turnTopic.status = Status.Done; }
        }
        else { Debug.LogError("Invalid turnTopic (Null)"); }
    }

    #endregion

    #region TopicTypeData
    //
    // - - - TopicTypeData - - -
    //

    /// <summary>
    /// returns true if Topic Type Data check passes, false otherwise. 
    /// TopicTypes test for global and topicTypeData.minIntervals
    /// TopicSubTypes test for topicTypeData.isAvailable and topicTypeData.minIntervals
    /// </summary>
    /// <param name="data"></param>
    /// <param name="turn"></param>
    /// <param name="isTopicSubType"></param>
    /// <returns></returns>
    private bool CheckTopicTypeData(TopicTypeData data, int turn, bool isTopicSubType = false)
    {
        int interval;
        bool isValid = false;
        bool isProceed = true;
        //accomodate the first few turns that may be less than the minTopicTypeTurns value
        int minOverallInterval = Mathf.Min(turn, minIntervalGlobalActual);
        if (data != null)
        {
            switch (isTopicSubType)
            {
                case false:
                    //TopicTypes -> check global interval & data.minInterval -> check for global interval
                    if ((turn - data.turnLastUsed) >= minOverallInterval)
                    { isValid = true; }
                    else
                    {
                        isProceed = false;
                        isValid = false;
                    }
                    if (isProceed == true)
                    {
                        //edge case, level start, hasn't been used yet
                        if (data.timesUsedLevel == 0)
                        { interval = Mathf.Min(data.minInterval, turn); }
                        else { interval = data.minInterval; }
                        //check for minimum Interval
                        if ((turn - data.turnLastUsed) >= interval)
                        { isValid = true; }
                        else { isValid = false; }
                    }
                    break;
                case true:
                    //TopicSubTypes -> check isAvailable & data.minInterval
                    if (data.isAvailable == true)
                    {
                        isProceed = true;
                        isValid = true;
                    }
                    else { isProceed = false; }
                    if (isProceed == true)
                    {
                        //edge case, level start, hasn't been used yet
                        if (data.timesUsedLevel == 0)
                        { interval = Mathf.Min(data.minInterval, turn); }
                        else { interval = data.minInterval; }
                        //check for minimum Interval
                        if ((turn - data.turnLastUsed) >= interval)
                        { isValid = true; }
                        else { isValid = false; }
                    }
                    break;
            }
        }
        else { Debug.LogError("Invalid TopicTypeData (Null)"); }
        return isValid;
    }
    #endregion

    #region Topic Type Criteria
    //
    // - - - TopicType Criteria - - -
    //

    /// <summary>
    /// Checks any topicType for availability this Turn using a cascading series of checks that exits on the first positive outcome
    /// </summary>
    /// <param name="topicType"></param>
    /// <returns></returns>
    public bool CheckTopicsAvailable(TopicType topicType, int turn)
    {
        bool isValid = false;
        if (topicType != null)
        {
            //loop subTypes
            for (int i = 0; i < topicType.listOfSubTypes.Count; i++)
            {
                TopicSubType subType = topicType.listOfSubTypes[i];
                if (subType != null)
                {
                    //check subType pool present
                    List<Topic> listOfTopics = GameManager.instance.dataScript.GetListOfTopics(subType);
                    if (listOfTopics != null)
                    {
                        //check subType topicTypeData
                        TopicTypeData dataSub = GameManager.instance.dataScript.GetTopicSubTypeData(subType.name);
                        if (dataSub != null)
                        {
                            if (CheckTopicTypeData(dataSub, turn, true) == true)
                            {
                                /*if (dataSub.minInterval > 0)
                                {
                                    Debug.LogFormat("[Tst] TopicManager.cs -> CheckTopicAvailable: \"{0}\", t {1}, l {2}, m {3}, isValid {4}{5}", subType.name,
                                        turn, dataSub.turnLastUsed, dataSub.minInterval, "True", "\n");
                                }*/

                                //TO DO check subType criteria (bypass the topic checks if fail) 

                                //loop pool of topics looking for any that are Live. Break on the first active topic (only needs to be one)
                                for (int j = 0; j < listOfTopics.Count; j++)
                                {
                                    Topic topic = listOfTopics[j];
                                    if (topic != null)
                                    {
                                        //check status
                                        if (topic.status == Status.Live)
                                        {
                                            //at least one valid topic present, exit
                                            isValid = true;

                                            //TO DO set subTopic availability (Edit: do I still need to do this? I doubt it but need to check)

                                            break;
                                        }
                                    }
                                    else { Debug.LogErrorFormat("Invalid topic (Null) for subTopicType \"{0}\" listOfTopics[{1}]", subType.name, j); }
                                }
                                //check successful if any one subType of the topicType is valid
                                if (isValid == true)
                                { break; }
                            }
                            else
                            {
                                /*if (dataSub.minInterval > 0)
                                {
                                    Debug.LogFormat("[Tst] TopicManager.cs -> CheckTopicAvailable: \"{0}\", t {1}, l {2}, m {3}, isValid {4}{5}", subType.name,
                                        turn, dataSub.turnLastUsed, dataSub.minInterval, "False", "\n");
                                }*/
                            }
                        }
                        else { Debug.LogErrorFormat("Invalid topicTypeData (Null) for \"{0} / {1}\"", topicType.name, subType.name); }
                    }
                }
                else { Debug.LogErrorFormat("Invalid subType (Null) for topic \"{0}\" listOfSubTypes[{1}]", topicType.name, i); }
            }
        }
        return isValid;
    }
    #endregion

    #region Unit Tests
    //
    // - - - Unit Tests - - -
    //

    /// <summary>
    /// runs a turn based unit test on selected topictype/subtype/topic looking for anything out of place
    /// NOTE: checks a lot of stuff that should have already been checked but acts as a final check and picks up stuff in the case of code changes that might have thrown something out of alignment
    /// NOTE: playerSide checked for Null by parent method
    /// </summary>
    private void UnitTestTopic(GlobalSide playerSide)
    {
        //check if at least one entry in listOfTopicTypesTurn
        if (listOfTopicTypesTurn.Count > 0)
        {
            int turn = GameManager.instance.turnScript.Turn;
            int interval;
            //
            // - - - topicType
            //
            if (turnTopic != null)
            {
                TopicTypeData dataType = GameManager.instance.dataScript.GetTopicTypeData(turnTopicType.name);
                if (dataType != null)
                {
                    //check global interval
                    int minOverallInterval = Mathf.Min(turn, minIntervalGlobalActual);
                    if ((turn - dataType.turnLastUsed) < minOverallInterval)
                    { Debug.LogWarningFormat("Invalid topicType \"{0}\" Global Interval (turn {1}, last used t {2}, minGlobaIInterval {3})", turnTopic.name, turn, dataType.turnLastUsed, minOverallInterval); }
                    //check topicType minInterval
                    if (dataType.minInterval > 0)
                    {
                        //edge case, level start, hasn't been used yet
                        if (dataType.timesUsedLevel == 0)
                        { interval = Mathf.Min(dataType.minInterval, turn); }
                        else { interval = dataType.minInterval; }
                        //check
                        if ((turn - dataType.turnLastUsed) < interval)
                        { Debug.LogWarningFormat("Invalid topicType \"{0}\" Min Interval (turn {1}, last used t {2}, minInterval {3})", turnTopic.name, turn, dataType.turnLastUsed, dataType.minInterval); }
                    }
                    //check topicType on list
                    if (listOfTopicTypesTurn.Exists(x => x.name.Equals(turnTopicType.name, StringComparison.Ordinal)) == false)
                    { Debug.LogWarningFormat("Invalid topicType \"{0}\" NOT found in listOfTopicTypeTurn", turnTopicType.name); }
                }
                else { Debug.LogWarningFormat("Invalid topicTypeData (Null) for topicType \"{0}\"", turnTopic.name); }
            }
            else { Debug.LogWarning("Invalid topicType (Null)"); }
            //
            // - - - topicSubType
            //
            if (turnTopicSubType != null)
            {
                TopicTypeData dataSubType = GameManager.instance.dataScript.GetTopicSubTypeData(turnTopicSubType.name);
                if (dataSubType != null)
                {
                    //check isAvailable 
                    if (dataSubType.isAvailable == false)
                    { Debug.LogWarningFormat("Invalid topicSubType \"{0}\" dataTopic.isAvailable \"{1}\" (should be True)", turnTopicSubType.name, dataSubType.isAvailable); }
                    //check topicSubType minInterval
                    if (dataSubType.minInterval > 0)
                    {
                        //edge case, level start, hasn't been used yet
                        if (dataSubType.timesUsedLevel == 0)
                        { interval = Mathf.Min(dataSubType.minInterval, turn); }
                        else { interval = dataSubType.minInterval; }
                        //check
                        if ((turn - dataSubType.turnLastUsed) < interval)
                        {
                            Debug.LogWarningFormat("Invalid topicSubType \"{0}\" minInterval (turn {1}, last used t {2}, minInterval {3})",
                                turnTopicSubType.name, turn, dataSubType.turnLastUsed, dataSubType.minInterval);
                        }
                    }
                    //check subType child of TopicType parent
                    if (turnTopicSubType.type.name.Equals(turnTopicType.name, StringComparison.Ordinal) == false)
                    { Debug.LogWarningFormat("Invalid topicSubType \"{0}\" has MISMATCH with topicType parent (is {1}, should be {2})", turnTopicSubType.name, turnTopicSubType.type.name, turnTopicType.name); }
                }
                else { Debug.LogWarningFormat("Invalid topicTypeData for topicSubType \"{0}\"", turnTopicSubType.name); }
            }
            else { Debug.LogWarning("Invalid topicSubType (Null)"); }
            //
            // - - - Topic
            //
            if (turnTopic != null)
            {
                //status active
                if (turnTopic.status != Status.Live)
                { Debug.LogWarningFormat("Invalid topic \"{0}\" status \"{1}\" (should be {2})", turnTopic.name, turnTopic.status, Status.Live); }
                //check correct topicSubType,
                if (turnTopic.subType.name.Equals(turnTopicSubType.name, StringComparison.Ordinal) == false)
                { Debug.LogWarningFormat("Invalid topic \"{0}\" has MISMATCH with topicSubType parent (is {1}, should be {2})", turnTopic.name, turnTopic.subType.name, turnTopicSubType.name); }
                //check correct topicType
                if (turnTopic.type.name.Equals(turnTopicType.name, StringComparison.Ordinal) == false)
                { Debug.LogWarningFormat("Invalid topic \"{0}\" has MISMATCH with topicType parent (is {1}, should be {2})", turnTopic.name, turnTopic.type.name, turnTopicType.name); }
                //check correct side
                if (turnTopic.side.level != playerSide.level)
                { Debug.LogWarningFormat("Invalid topic \"{0}\" with INCORRECT SIDE (is {1}, should be {2})", turnTopic.name, turnTopic.side.name, playerSide.name); }
            }
            else { Debug.LogWarning("Invalid topic (Null)"); }
        }
        else
        { /*Debug.LogWarning("Invalid listOfTopicTypesTurn (Empty)");*/ }
    }
    #endregion

    #region Utilities
    //
    // - - - Utilities - - -
    //

    #region AddTopicTypeToList
    /// <summary>
    /// add a topicType item to a list, only if not present already (uses TopicType.name for dupe checks)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="item"></param>
    private void AddTopicTypeToList(List<TopicType> list, TopicType item)
    {
        if (list != null)
        {
            if (item != null)
            {
                //autoAdd first item
                if (list.Count > 0)
                {
                    //add only if not already in list
                    if (list.Exists(x => x.name.Equals(item.name, StringComparison.Ordinal)) == false)
                    { list.Add(item); }
                }
                else { list.Add(item); }
            }
            else { Debug.LogError("Invalid TopicType item (Null)"); }
        }
        else { Debug.LogError("Invalid TopicType list (Null)"); }
    }
    #endregion

    #region GetNumOfEntries
    /// <summary>
    /// Returns the number of entries of a TopicType/SubType to place into a selection pool based on the items priority. Returns 0 if a problem.
    /// </summary>
    /// <param name="priority"></param>
    /// <returns></returns>
    private int GetNumOfEntries(GlobalChance priority)
    {
        int numOfEntries = 0;
        if (priority != null)
        {
            switch (priority.name)
            {
                case "Low":
                    numOfEntries = 1;
                    break;
                case "Medium":
                    numOfEntries = 2;
                    break;
                case "High":
                    numOfEntries = 3;
                    break;
                case "Extreme":
                    numOfEntries = 5;
                    break;
                default:
                    Debug.LogWarningFormat("Unrecognised priority.name \"{0}\"", priority.name);
                    break;
            }
        }
        else { Debug.LogError("Invalid priority (Null)"); }
        return numOfEntries;
    }
    #endregion

    #region GetTopicGroup
    /// <summary>
    /// Returns a list Of Live topics filtered out of the inputList by group type, eg. Good / Bad, etc. If Neutral then all of inputList is used, if no topic found of the correct group then all used again.
    /// Returns EMPTY list if a problem
    /// </summary>
    /// <param name="inputList"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    private List<Topic> GetTopicGroup(List<Topic> inputList, GroupType group, string subTypeName = "Unknown", string subSubTypeName = "")
    {
        List<Topic> listOfTopics = new List<Topic>();
        if (inputList != null)
        {
            //normal
            if (string.IsNullOrEmpty(subSubTypeName) == true)
            {
                switch (group)
                {
                    case GroupType.Good:
                        //high motivation, good group
                        listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live &&
                        t.group.name.Equals("Good", StringComparison.Ordinal)).ToList());
                        break;
                    case GroupType.Neutral:
                        //neutral motivation, use all Active topics
                        listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live).ToList());
                        break;
                    case GroupType.Bad:
                    case GroupType.VeryBad:
                        //low motivation, bad group
                        listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live &&
                        t.group.name.Equals("Bad", StringComparison.Ordinal)).ToList());
                        break;
                    default:
                        Debug.LogWarningFormat("Unrecognised GroupType \"[0}\"", group);
                        break;
                }
                //if no entries use entire list by default
                if (listOfTopics.Count == 0)
                {
                    Debug.LogFormat("[Tst] TopicManager.cs -> GetActorContactTopics: No topics found for \"{0}\", group \"{1}\", All topics used{2}", subTypeName, group, "\n");
                    listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live).ToList());
                }
                else { Debug.LogFormat("[Tst] TopicManager.cs -> GetTopicGroup: {0} topics found for \"{1}\" group {2}{3}", listOfTopics.Count, subTypeName, group, "\n"); }
            }
            else
            {
                //filter topics by subSubTypeName
                switch (group)
                {
                    case GroupType.Good:
                        //high motivation, good group
                        listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live &&
                        t.subSubType.name.Equals(subSubTypeName, StringComparison.Ordinal) &&
                        t.group.name.Equals("Good", StringComparison.Ordinal)).ToList());
                        break;
                    case GroupType.Neutral:
                        //neutral motivation, use all Active topics
                        listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live &&
                        t.subSubType.name.Equals(subSubTypeName, StringComparison.Ordinal)).ToList());
                        break;
                    case GroupType.Bad:
                    case GroupType.VeryBad:
                        //low motivation, bad group
                        listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live &&
                        t.subSubType.name.Equals(subSubTypeName, StringComparison.Ordinal) &&
                        t.group.name.Equals("Bad", StringComparison.Ordinal)).ToList());
                        break;
                    default:
                        Debug.LogWarningFormat("Unrecognised GroupType \"[0}\"", group);
                        break;
                }
                //if no entries use full sub sub type list by default
                if (listOfTopics.Count == 0)
                {
                    Debug.LogFormat("[Tst] TopicManager.cs -> GetActorContactTopics: No topics found for \"{0}\", \"{1}\", group \"{2}\", All relevant topics used{2}", subTypeName, subSubTypeName, group, "\n");
                    listOfTopics.AddRange(inputList.Where(t => t.status == Status.Live &&
                    t.subSubType.name.Equals(subSubTypeName, StringComparison.Ordinal)).ToList());
                }
                else { Debug.LogFormat("[Tst] TopicManager.cs -> GetTopicGroup: {0} topics found for \"{1}\", \"{2}\", group {3}{4}", listOfTopics.Count, subTypeName, subSubTypeName, group, "\n"); }
            }

        }
        else { Debug.LogError("Invalid inputList (Null)"); }
        return listOfTopics;
    }
    #endregion

    #region GetTopicSubSubType
    /// <summary>
    /// Get's NodeAction TopicSubSubType.SO.name given a nodeAction enum. Returns Null if a problem
    /// </summary>
    /// <param name="nodeAction"></param>
    /// <returns></returns>
    public TopicSubSubType GetTopicSubSubType(NodeAction nodeAction)
    {
        TopicSubSubType subSubType = null;
        //get specific subSubType topic pool
        switch (nodeAction)
        {
            case NodeAction.BlowStuffUp: subSubType = BlowStuffUp; break;
            case NodeAction.CreateRiots: subSubType = CreateRiots; break;
            case NodeAction.DeployTeam: subSubType = DeployTeam; break;
            case NodeAction.GainTargetInfo: subSubType = GainTargetInfo; break;
            case NodeAction.HackSecurity: subSubType = HackSecurity; break;
            case NodeAction.InsertTracer: subSubType = InsertTracer; break;
            case NodeAction.NeutraliseTeam: subSubType = NeutraliseTeam; break;
            case NodeAction.ObtainGear: subSubType = ObtainGear; break;
            case NodeAction.RecallTeam: subSubType = RecallTeam; break;
            case NodeAction.RecruitActor: subSubType = RecruitActor; break;
            case NodeAction.SpreadFakeNews: subSubType = SpreadFakeNews; break;
            default: Debug.LogWarningFormat("Unrecognised data.nodeAction \"{0}\"", nodeAction); break;
        }
        return subSubType;
    }
    #endregion

    #region CheckSubSubTypeTopicsPresent
    /// <summary>
    /// Returns true if any topic in given list has the matching TopicSubSubType.name, false if not or if a problem
    /// </summary>
    /// <param name="listOfTopics"></param>
    /// <param name="subSubTypeName"></param>
    /// <returns></returns>
    public bool CheckSubSubTypeTopicsPresent(List<Topic> listOfTopics, string subSubTypeName)
    {
        if (listOfTopics != null)
        {
            if (string.IsNullOrEmpty(subSubTypeName) == false)
            {
                //loop list and break out and return true on first match
                for (int i = 0; i < listOfTopics.Count; i++)
                {
                    if (listOfTopics[i].subSubType.name.Equals(subSubTypeName, StringComparison.Ordinal) == true)
                    { return true; }
                }
            }
            else { Debug.LogError("Invalid subSubTypeName (Null)"); }
        }
        else { Debug.LogError("Invalid listOfTopics (Null)"); }
        return false;
    }
    #endregion

    #region CheckPlayerStatus
    /// <summary>
    /// Checks Player (AI/Human) status
    /// NOTE: playerSide checked for Null by parent method
    /// </summary>
    /// <returns></returns>
    private bool CheckPlayerStatus(GlobalSide playerSide)
    {
        bool isValid = false;
        switch (playerSide.level)
        {
            case 1:
                //Authority
                switch (GameManager.instance.sideScript.authorityOverall)
                {
                    case SideState.Human:
                        if (GameManager.instance.playerScript.status == ActorStatus.Active)
                        { isValid = true; }
                        break;
                    case SideState.AI:
                        if (GameManager.instance.aiScript.status == ActorStatus.Active)
                        { isValid = true; }
                        break;
                    default:
                        Debug.LogWarningFormat("Unrecognised authorityOverall \"{0}\"", GameManager.instance.sideScript.authorityOverall);
                        break;
                }
                break;
            case 2:
                //Resistance
                switch (GameManager.instance.sideScript.resistanceOverall)
                {
                    case SideState.Human:
                        if (GameManager.instance.playerScript.status == ActorStatus.Active)
                        { isValid = true; }
                        break;
                    case SideState.AI:
                        if (GameManager.instance.aiRebelScript.status == ActorStatus.Active)
                        { isValid = true; }
                        break;
                    default:
                        Debug.LogWarningFormat("Unrecognised resistanceOverall \"{0}\"", GameManager.instance.sideScript.authorityOverall);
                        break;
                }
                break;
            default:
                Debug.LogWarningFormat("Unrecognised playerSide \"{0}\"", playerSide.name);
                break;
        }
        return isValid;
    }
    #endregion

    #region LoadSubSubTypePools
    /// <summary>
    /// Load up a subType topic pool with any associated subSubType pools
    /// NOTE: Assumes that the parent method has checked that the subType has listOfSubSubTypes.Count > 0
    /// </summary>
    private void LoadSubSubTypePools(TopicPool subPool, GlobalSide side)
    {
        if (subPool != null)
        {
            //empty out any residual topics prior to repopulating
            subPool.listOfTopics.Clear();
            int count = 0;
            //populate subType listOfTopics from all subSubType pool's listOfTopics
            foreach (TopicPool subSubPool in subPool.listOfSubSubTypePools)
            {
                //correct side or 'Both'
                if (subSubPool.subSubType.side.level == side.level || subSubPool.subSubType.side.level == 3)
                {
                    count += subSubPool.listOfTopics.Count;
                    subPool.listOfTopics.AddRange(subSubPool.listOfTopics);
                }
            }
            Debug.LogFormat("[Top] TopicManager.cs -> LoadSubSubTypePools: {0} topics added to {1} from subSubTopicPools{2}",
                count, subPool.name, "\n");
        }
        else { Debug.LogError("Invalid subPool (Null)"); }
    }
    #endregion

    #endregion

    #region Meta Methods
    //
    // - - - MetaManager - - -
    //

    /// <summary>
    /// Runs at end of level, MetaManager.cs -> ProcessMetaGame, to clear out relevant topic data and update others (eg. timesUsedCampaign += timesUsedLevel)
    /// </summary>
    /// <param name="dictOfTopicTypeData"></param>
    public void ProcessMetaTopics()
    {
        //dictOfType/SubType
        UpdateTopicTypes(GameManager.instance.dataScript.GetDictOfTopicTypeData());
        UpdateTopicTypes(GameManager.instance.dataScript.GetDictOfTopicSubTypeData());
    }

    /// <summary>
    /// subMethod for ProcessMetaTopics to handle dictOfTopicType/SubType
    /// </summary>
    /// <param name="dictOfTopicTypeData"></param>
    private void UpdateTopicTypes(Dictionary<string, TopicTypeData> dictOfTopicTypeData)
    {
        foreach (var record in dictOfTopicTypeData)
        {
            record.Value.isAvailable = true;
            record.Value.turnLastUsed = 0;
            record.Value.timesUsedCampaign += record.Value.timesUsedLevel;
            record.Value.timesUsedLevel = 0;
        }
    }
    #endregion

    #region Debug Methods
    //
    // - - - Debug - - -
    //

    /// <summary>
    /// debug method to randomise topic status (50/50 Active/Dormant) at start of each level
    /// </summary>
    private void DebugRandomiseTopicStatus()
    {
        Dictionary<string, Topic> dictOfTopics = GameManager.instance.dataScript.GetDictOfTopics();
        if (dictOfTopics != null)
        {
            foreach (var topic in dictOfTopics)
            {
                if (Random.Range(0, 100) < 20)
                { topic.Value.status = Status.Dormant; }
                else { topic.Value.status = Status.Active; }
            }
        }
        else { Debug.LogError("Invalid dictOfTopics (Null)"); }
    }

    /// <summary>
    /// Display's topic type data in a more user friendly manner (subTypes grouped by Types)
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayTopicTypes()
    {
        StringBuilder builder = new StringBuilder();
        Dictionary<string, TopicTypeData> dictOfTopicTypes = GameManager.instance.dataScript.GetDictOfTopicTypeData();
        if (dictOfTopicTypes != null)
        {
            Dictionary<string, TopicTypeData> dictOfTopicSubTypes = GameManager.instance.dataScript.GetDictOfTopicSubTypeData();
            if (dictOfTopicSubTypes != null)
            {
                GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
                builder.AppendFormat("- TopicTypeData for TopicTypes{0}{1}", "\n", "\n");
                //used to print a '*' to indicate topic / subType is valid and ready to go
                bool isValidType, isValidSubType;
                //loop topic Types
                foreach (var topicTypeData in dictOfTopicTypes)
                {
                    TopicType topicType = GameManager.instance.dataScript.GetTopicType(topicTypeData.Key);
                    if (topicType != null)
                    {
                        if (topicType.side.level == playerSide.level || topicType.side.level == 3)
                        {
                            isValidType = DebugCheckValidType(topicTypeData.Value);
                            builder.AppendFormat(" {0}{1}{2}", DebugDisplayTypeRecord(topicTypeData.Value), isValidType == true ? " *" : "", "\n");
                            //loop subTypes
                            foreach (TopicSubType subType in topicType.listOfSubTypes)
                            {
                                //needs to be the correct side
                                if (subType.side.level == playerSide.level || subType.side.level == 3)
                                {
                                    TopicTypeData subData = GameManager.instance.dataScript.GetTopicSubTypeData(subType.name);
                                    if (subData != null)
                                    {
                                        isValidSubType = false;
                                        if (isValidType == true)
                                        { isValidSubType = DebugCheckValidType(subData); }
                                        builder.AppendFormat("  {0}{1}{2}", DebugDisplayTypeRecord(subData), isValidSubType == true ? " *" : "", "\n");
                                    }
                                    else { Debug.LogWarningFormat("Invalid subData (Null) for topicSubType \"{0}\"", subType.name); }
                                }
                                else { builder.AppendFormat("  {0} -> Not Valid for this Side{1}", subType.name, "\n"); }
                            }
                            builder.AppendLine();
                        }
                        else { builder.AppendFormat("  {0} -> Not valid for this side{1}{2}", topicType.name, "\n", "\n"); }
                    }
                    else { Debug.LogError("Invalid topicType (Null)"); }
                }
            }
            else { Debug.LogError("Invalid dictOfTopicSubTypes (Null)"); }
        }
        else { Debug.LogError("Invalid dictOfTopicTypes (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// Sub method for DebugDisplayTopicTypes to display a TopicTypeData record (topicType / SubType)
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private string DebugDisplayTypeRecord(TopicTypeData data)
    {
        return string.Format(" {0} Av {1}, Lst {2}, Min {3}, #Lv {4}, #Ca {5}", data.type, data.isAvailable, data.turnLastUsed, data.minInterval,
                         data.timesUsedLevel, data.timesUsedCampaign);
    }

    /// <summary>
    /// display two lists of topic Types
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayTopicTypeLists()
    {
        StringBuilder builder = new StringBuilder();
        //listOfTopicTypesLevel
        builder.AppendFormat("- listOfTopicTypesLevel{0}", "\n");
        List<TopicType> listOfTopicTypeLevel = GameManager.instance.dataScript.GetListOfTopicTypesLevel();
        if (listOfTopicTypeLevel != null)
        {
            if (listOfTopicTypeLevel.Count > 0)
            {
                foreach (TopicType typeLevel in listOfTopicTypeLevel)
                { builder.AppendFormat(" {0}{1}", typeLevel.tag, "\n"); }
            }
            else { builder.AppendFormat(" No records{0}", "\n"); }
        }
        else { Debug.LogError("Invalid listOfTopicTypesLevel (Null)"); }
        //listOfTopicTypesTurn
        builder.AppendFormat("{0}- listOfTopicTypesTurn{1}", "\n", "\n");
        if (listOfTopicTypesTurn != null)
        {
            if (listOfTopicTypesTurn.Count > 0)
            {
                foreach (TopicType typeTurn in listOfTopicTypesTurn)
                { builder.AppendFormat(" {0}{1}", typeTurn.tag, "\n"); }
            }
            else { builder.AppendFormat(" No records{0}", "\n"); }
        }
        else { Debug.LogError("Invalid listOfTopicTypesTurn (Null)"); }
        //topicHistory
        Dictionary<int, HistoryTopic> dictOfTopicHistory = GameManager.instance.dataScript.GetDictOfTopicHistory();
        if (dictOfTopicHistory != null)
        {
            builder.AppendFormat("{0} - dictOfTopicHistory ({1} records){2}", "\n", dictOfTopicHistory.Count, "\n");
            if (dictOfTopicHistory.Count > 0)
            {
                foreach (var history in dictOfTopicHistory)
                {
                    builder.AppendFormat("  t {0} -> # {1} -> {2} -> {3} -> {4} -> {5}{6}", history.Value.turn, history.Value.numSelect,
                      history.Value.topicType, history.Value.topicSubType, history.Value.topic, history.Value.option, "\n");
                }
            }
            else { builder.AppendFormat("  No records{0}", "\n"); }
        }
        else { Debug.LogError("Invalid dictOfTopicHistory (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// Debug display of topic Type criteria
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayCriteria()
    {
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- listOfTopicTypes -> Criteria{0}", "\n");
        //listOfTopicTypes
        List<TopicType> listOfTopicTypes = GameManager.instance.dataScript.GetListOfTopicTypes();
        if (listOfTopicTypes != null)
        {
            foreach (TopicType topicType in listOfTopicTypes)
            {
                builder.AppendFormat("{0} {1}, pr: {2}, minInt {3}{4}", "\n", topicType.tag, topicType.priority.name, topicType.minInterval, "\n");
                if (topicType.listOfCriteria.Count > 0)
                {
                    foreach (Criteria criteria in topicType.listOfCriteria)
                    { builder.AppendFormat("    \"{0}\", {1}{2}", criteria.name, criteria.description, "\n"); }
                }
                else { builder.AppendFormat("No criteria{0}", "\n"); }
                //loop subTypes
                foreach (TopicSubType subType in topicType.listOfSubTypes)
                {
                    //needs to be the correct side
                    if (subType.side.level == playerSide.level || subType.side.level == 3)
                    {
                        builder.AppendFormat("  -{0}, pr: {1}, minInt {2}{3}", subType.name, subType.priority.name, subType.minInterval, "\n");
                        if (subType.listOfCriteria.Count > 0)
                        {
                            foreach (Criteria criteria in subType.listOfCriteria)
                            { builder.AppendFormat("    \"{0}\", {1}{2}", criteria.name, criteria.description, "\n"); }
                        }
                    }
                    else { builder.AppendFormat("  -{0} -> Not Valid for this Side{1}", subType.name, "\n"); }
                }
            }
        }
        else { Debug.LogError("Invalid listOfTopicTypes (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// Debug display of all topic pools (topics valid for the current level)
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayTopicPools()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- dictOfTopicPools{0}", "\n");
        List<TopicType> listOfTopicTypes = GameManager.instance.dataScript.GetListOfTopicTypesLevel();
        if (listOfTopicTypes != null)
        {
            //loop topic types by level
            foreach (TopicType topicType in listOfTopicTypes)
            {
                /*builder.AppendFormat("{0} {1}{2}", "\n", topicType.tag, "\n");*/
                builder.AppendLine();
                //loop topicSubTypes
                if (topicType.listOfSubTypes != null)
                {
                    foreach (TopicSubType subType in topicType.listOfSubTypes)
                    {
                        builder.AppendFormat("   {0}{1}", subType.tag, "\n");
                        //find entry in dictOfTopicPools and display all topics for that subType
                        List<Topic> listOfTopics = GameManager.instance.dataScript.GetListOfTopics(subType);
                        {
                            if (listOfTopics != null)
                            {
                                if (listOfTopics.Count > 0)
                                {
                                    foreach (Topic topic in listOfTopics)
                                    {
                                        builder.AppendFormat("     {0}, {1} x Op, St: {2}, Pr: {3}, Gr: {4}{5}", topic.name, topic.listOfOptions?.Count, topic.status,
                                          topic.priority.name, topic.group.name, "\n");
                                    }
                                }
                                else { builder.AppendFormat("    None found{0}", "\n"); }
                            }
                            else
                            {
                                /*Debug.LogWarningFormat("Invalid listOfTopics (Null) for topicSubType {0}", subType.name);*/
                                builder.AppendFormat("    Missing list (Error){0}", "\n");
                            }
                        }
                    }
                }
                else
                {
                    /*Debug.LogWarningFormat("Invalid listOfSubTypes (Null) for topicType \"{0}\"{1}", topicType, "\n");*/
                    builder.AppendFormat("  None found (Error){0}", "\n");
                }
            }
        }
        else { Debug.LogError("Invalid listOfTopicTypesLevel (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// Debug display of selection pools and data
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayTopicSelectionData()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- Topic Selection{0}{1}", "\n", "\n");
        builder.AppendFormat(" minIntervalGlobal: {0}{1}", minIntervalGlobal, "\n");
        builder.AppendFormat(" minIntervalGlobalActual: {0}{1}{2}", minIntervalGlobalActual, "\n", "\n");
        if (turnTopicType != null)
        { builder.AppendFormat(" topicType: {0}{1}", turnTopicType.name, "\n"); }
        else { builder.AppendFormat(" topicType: NULL{0}", "\n"); }
        if (turnTopicSubType != null)
        { builder.AppendFormat(" topicSubType: {0}{1}", turnTopicSubType.name, "\n"); }
        if (turnTopicSubSubType != null)
        { builder.AppendFormat(" topicSubSubType: {0}{1}", turnTopicSubSubType.name, "\n"); }
        else { builder.AppendFormat(" topicSubType: NULL{0}", "\n"); }
        if (turnTopic != null)
        { builder.AppendFormat(" topic: {0}{1}", turnTopic.name, "\n"); }
        else { builder.AppendFormat(" topic: NULL{0}", "\n"); }
        //topic type pool
        builder.AppendFormat("{0}- listOfTopicTypePool{1}", "\n", "\n");
        if (listOfTypePool.Count > 0)
        {
            foreach (TopicType topicType in listOfTypePool)
            { builder.AppendFormat(" {0}, priority: {1}{2}", topicType.name, topicType.priority.name, "\n"); }
        }
        else { builder.AppendFormat(" No records{0}", "\n"); }
        //topic sub type pool
        builder.AppendFormat("{0}- listOfTopicSubTypePool{1}", "\n", "\n");
        if (listOfSubTypePool.Count > 0)
        {
            foreach (TopicSubType subType in listOfSubTypePool)
            { builder.AppendFormat(" {0}, proirity: {1}{2}", subType.name, subType.priority.name, "\n"); }
        }
        else { builder.AppendFormat("No records{0}", "\n"); }
        //topic pool
        builder.AppendFormat("{0}- listOfTopicPool{1}", "\n", "\n");
        if (listOfTopicPool.Count > 0)
        {
            foreach (Topic topic in listOfTopicPool)
            { builder.AppendFormat(" {0}, priority: {1}{2}", topic.name, topic.priority.name, "\n"); }
        }
        else { builder.AppendFormat("No records{0}", "\n"); }
        return builder.ToString();
    }

    /// <summary>
    /// Debug display of dynamic profile data, eg. timers etc.
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayTopicProfileData()
    {
        StringBuilder builder = new StringBuilder();
        Dictionary<string, Topic> dictOfTopics = GameManager.instance.dataScript.GetDictOfTopics();
        if (dictOfTopics != null)
        {
            builder.AppendFormat("- Topic Profile Data{0}{1}", "\n", "\n");
            foreach (var topic in dictOfTopics)
            {
                if (topic.Value.profile != null)
                {
                    builder.AppendFormat(" {0} -> {1} -> ts {2}, tr {3}, tw {4} -> D {5}, A {6}, L {7}, D {8} -> {9}{10}", topic.Value.name, topic.Value.profile.name, topic.Value.timerStart,
                        topic.Value.timerRepeat, topic.Value.timerWindow, topic.Value.turnsDormant, topic.Value.turnsActive, topic.Value.turnsLive,
                        topic.Value.turnsDone, topic.Value.isCurrent ? "true" : "FALSE", "\n");
                }
                else { builder.AppendFormat(" {0} -> Invalid Profile (Null){1}", topic.Value.name, "\n"); }
            }
        }
        else { Debug.LogError("Invalid dictOfTopics (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// returns true if a valid topicType (must pass minInterval and global minInterval checks) -> Used for DebugDisplayTopicTypes to show a '*' or not
    /// </summary>
    /// <param name="topicType"></param>
    /// <returns></returns>
    private bool DebugCheckValidType(TopicTypeData data)
    {
        int turn = GameManager.instance.turnScript.Turn;
        if (data != null)
        {
            if (data.minInterval == 0)
            {
                //global minInterval applies
                if (turn - data.turnLastUsed >= minIntervalGlobal)
                { return true; }
            }
            else
            {
                //local minInterval applies
                if (turn - data.turnLastUsed >= data.minInterval)
                { return true; }
            }
        }
        else { Debug.LogError("Invalid topictypeData (Null)"); }
        return false;
    }

    #endregion

    //new methods above here
}
