using gameAPI;
using packageAPI;
using System;
using System.Collections.Generic;
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

    private List<TopicType> listOfTopicTypesTurn = new List<TopicType>();                               //level topics that passed their turn checks
    private List<TopicType> listOfTypePool = new List<TopicType>();                                     //turn selection pool for topicTypes (priority based)
    private List<TopicSubType> listOfSubTypePool = new List<TopicSubType>();                            //turn selection pool for topicSubTypes (priority based)
    private List<Topic> listOfTopicPool = new List<Topic>();                                            //turn selection pool for topics (priority based)

    //Turn selection
    private TopicType turnTopicType;
    private TopicSubType turnTopicSubType;
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
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseLevelStart();
                break;
            case GameState.LoadAtStart:
                SubInitialiseStartUp();
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

    #region SubInitialiseAll
    private void SubInitialiseAll()
    {

    }
    #endregion

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
        /*DebugRandomiseTopicStatus();*/

        //reset all topics to isCurrent False prior to changes
        GameManager.instance.dataScript.ResetTopics();
        //establish which TopicTypes are valid for the level. Initialise profile and status data.
        UpdateTopicPools();
    }
    #endregion

    #endregion

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
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorPoliticPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.actorPoliticPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ActorContact":
                                                        if (campaign.actorContactPool != null)
                                                        {
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorContactPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.actorContactPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ActorDistrict":
                                                        if (campaign.actorDistrictPool != null)
                                                        {
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorDistrictPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.actorDistrictPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ActorGear":
                                                        if (campaign.actorGearPool != null)
                                                        {
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorGearPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.actorGearPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ActorMatch":
                                                        if (campaign.actorMatchPool != null)
                                                        {
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorMatchPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.actorMatchPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "AuthorityCampaign":
                                                        if (campaign.authorityCampaignPool != null)
                                                        {
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.authorityCampaignPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.authorityCampaignPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "AuthorityGeneral":
                                                        if (campaign.authorityGeneralPool != null)
                                                        {
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.authorityGeneralPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.authorityGeneralPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "AuthorityTeam":
                                                        if (campaign.teamPool != null)
                                                        {
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.teamPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.teamPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ResistanceCampaign":
                                                        if (campaign.resistanceCampaignPool != null)
                                                        {
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.resistanceCampaignPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.resistanceCampaignPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "ResistanceGeneral":
                                                        if (campaign.resistanceGeneralPool != null)
                                                        {
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.resistanceGeneralPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.resistanceGeneralPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "CampaignAlpha":
                                                        if (campaign.campaignAlphaPool != null)
                                                        {
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignAlphaPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignAlphaPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "CampaignBravo":
                                                        if (campaign.campaignBravoPool != null)
                                                        {
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignBravoPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.campaignBravoPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "CampaignCharlie":
                                                        if (campaign.campaignCharliePool != null)
                                                        {
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
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.familyAlphaPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.familyAlphaPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "FamilyBravo":
                                                        if (campaign.familyBravoPool != null)
                                                        {
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.familyBravoPool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.familyBravoPool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "FamilyCharlie":
                                                        if (campaign.familyCharliePool != null)
                                                        {
                                                            GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.familyCharliePool.listOfTopics);
                                                            AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                            SetTopicDynamicData(campaign.familyCharliePool.listOfTopics);
                                                            isValid = true;
                                                        }
                                                        break;
                                                    case "HQSub":
                                                        if (campaign.hqPool != null)
                                                        {
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
                        if (profile.isRepeat == true)
                        { topic.timerRepeat = profile.delayRepeat; }
                        else { topic.timerRepeat = 0; }
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

    //
    //  - - - Select Topic - - -
    //   

    /// <summary>
    /// Selects a topic for the turn (there will always be one)
    /// </summary>
    public void SelectTopic()
    {
        ResetTopicAdmin();
        //select a topic, if none found then drop the global interval by 1 and try again
        do
        {
            CheckTopics();
            CheckForValidTopicTypes();
            GetTopicType();
            GetTopicSubType();
            GetTopic();
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
        UnitTestTopic();
        //debug -> should be in ProcessTopic but here for autorun debugging purposes
        UpdateTopicTypeData();
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
            foreach(var topic in dictOfTopics)
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
                //generate message explaining why criteria failed -> debug only, spam otherwise
                Debug.LogFormat("[Top] TopicManager.cs -> GetTopic: topic \"{0}\" {1} Criteria check failed{2}", topic.name, criteriaCheck, "\n");
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
        turnTopic = null;
        //empty collections
        listOfTopicTypesTurn.Clear();
        listOfTypePool.Clear();
        listOfSubTypePool.Clear();
        listOfTopicPool.Clear();
    }

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

    /// <summary>
    /// Get topicSubType for turn Decision
    /// </summary>
    private void GetTopicSubType()
    {
        int numOfEntries;
        bool isProceed;
        if (turnTopicType != null)
        {
            GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
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


    /// <summary>
    /// Get individual topic for turn Decision
    /// </summary>
    private void GetTopic()
    {
        int numOfEntries;
        if (turnTopicSubType != null)
        {
            List<Topic> listOfTopics = GameManager.instance.dataScript.GetListOfTopics(turnTopicSubType);
            if (listOfTopics != null)
            {
                int count = listOfTopics.Count;
                //shouldn't be empty
                if (count > 0)
                {
                    //loop topics
                    for (int i = 0; i < count; i++)
                    {
                        Topic topic = listOfTopics[i];
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
        else
        {
            //No topic selected
            Debug.LogFormat("[Top] TopicManager.cs -> GetTopic: t {0}, No Topic Selected{1}", GameManager.instance.turnScript.Turn, "\n");
        }
    }


    /*/// <summary>
    /// Checks individual topic for Active status and any Criteria checks being O.K. Returns true if good on all counts, false otherwise
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    private bool CheckIndividualTopic(Topic topic)
    {
        bool isProceed = false;
        if (topic != null)
        {
            isProceed = false;
            //topic must be live or active
            if (topic.status == Status.Live)
            {
                isProceed = true;
                //topic criteria must pass checks
                if (topic.listOfCriteria != null && topic.listOfCriteria.Count > 0)
                {
                    CriteriaDataInput criteriaInput = new CriteriaDataInput()
                    { listOfCriteria = topic.listOfCriteria };
                    string criteriaCheck = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
                    if (criteriaCheck == null)
                    {
                        //criteria check passed O.K
                        isProceed = true;
                    }
                    else
                    {
                        //criteria check FAILED
                        isProceed = false;
                        //generate message explaining why criteria failed -> debug only, spam otherwise
                        Debug.LogFormat("[Top] TopicManager.cs -> GetTopic: topic \"{0}\" {1} Criteria check failed{2}", topic.name, criteriaCheck, "\n");
                    }
                }
            }
        }
        else { Debug.LogError("Invalid topic (Null)"); }
        return isProceed;
    }*/



    //
    // - - - Process Topic - - - 
    //

    /// <summary>
    /// Process selected topic (decision or Information)
    /// </summary>
    public void ProcessTopic()
    {
        ExecuteTopic();
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

    #region Analytics
    //
    // - - - Analytics - - -
    //

    /// <summary>
    /// runs a turn based unit test on selected topictype/subtype/topic looking for anything out of place
    /// NOTE: checks a lot of stuff that should have already been checked but acts as a final check and picks up stuff in the case of code changes that might have thrown something out of alignment
    /// </summary>
    private void UnitTestTopic()
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
                if (turnTopic.side.level != GameManager.instance.sideScript.PlayerSide.level)
                { Debug.LogWarningFormat("Invalid topic \"{0}\" with INCORRECT SIDE (is {1}, should be {2})", turnTopic.name, turnTopic.side.name, GameManager.instance.sideScript.PlayerSide.name); }
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
                            
                            /*//look for any matching SubTypes
                            foreach (var topicSubType in dictOfTopicSubTypes)
                            {
                                isValidSubType = false;
                                if (isValidType == true)
                                { isValidSubType = DebugCheckValidType(topicSubType.Value); }
                                if (topicSubType.Value.parent.Equals(topicTypeData.Key, StringComparison.Ordinal) == true)
                                { builder.AppendFormat("  {0}{1}{2}", DebugDisplayTypeRecord(topicSubType.Value), isValidSubType == true ? " *" : "", "\n"); }
                            }*/

                            //loop subTypes
                            foreach(TopicSubType subType in topicType.listOfSubTypes)
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
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- listOfTopicTypes -> Criteria{0}", "\n");
        //listOfTopicTypes
        List<TopicType> listOfTopicTypes = GameManager.instance.dataScript.GetListOfTopicTypes();
        if (listOfTopicTypes != null)
        {
            foreach (TopicType topicType in listOfTopicTypes)
            {
                builder.AppendFormat("{0} {1}, priority: {2}, minInt {3}{4}", "\n", topicType.tag, topicType.priority.name, topicType.minInterval, "\n");
                if (topicType.listOfCriteria.Count > 0)
                {
                    foreach (Criteria criteria in topicType.listOfCriteria)
                    { builder.AppendFormat("  criteria: \"{0}\", {1}{2}", criteria.name, criteria.description, "\n"); }
                }
                else { builder.AppendFormat("No criteria{0}", "\n"); }
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
                                    { builder.AppendFormat("     {0}, {1} x Op, St: {2}, Pr: {3}{4}", topic.name, topic.listOfOptions?.Count, topic.status, topic.priority.name, "\n"); }
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

    #endregion

    //new methods above here
}
