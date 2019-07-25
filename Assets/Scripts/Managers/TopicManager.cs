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
    [Tooltip("Minimum number of turns before topicType can be chosen again")]
    [Range(0, 10)] public int minTopicTypeTurns = 4;

    private List<TopicType> listOfTopicTypesTurn = new List<TopicType>();                               //level topics that passed their turn checks

    /// <summary>
    /// Initialisation
    /// </summary>
    /// <param name="state"></param>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
            case GameState.FollowOnInitialisation:
            case GameState.LoadAtStart:
                SubInitialiseLevelStart();
                break;
            case GameState.LoadGame:
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\" (InitialiseLate)", GameManager.instance.inputScript.GameState);
                break;
        }
    }


    #region Initialisation SubMethods

    #region SubInitialiseStartUp
    private void SubInitialiseStartUp()
    {

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
        DebugRandomiseTopicStatus();
        //establish which TopicTypes are valid for the level
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
                                for (int j = 0; j < topicType.listOfSubTypes.Count; j++)
                                {
                                    TopicSubType topicSubType = topicType.listOfSubTypes[j];
                                    if (topicSubType != null)
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
                                                    isValid = true;
                                                }
                                                break;
                                            case "ActorContact":
                                                if (campaign.actorContactPool != null)
                                                {
                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorContactPool.listOfTopics);
                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                    isValid = true;
                                                }
                                                break;
                                            case "ActorDistrict":
                                                if (campaign.actorDistrictPool != null)
                                                {
                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorDistrictPool.listOfTopics);
                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                    isValid = true;
                                                }
                                                break;
                                            case "ActorGear":
                                                if (campaign.actorGearPool != null)
                                                {
                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorGearPool.listOfTopics);
                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                    isValid = true;
                                                }
                                                break;
                                            case "ActorMatch":
                                                if (campaign.actorMatchPool != null)
                                                {
                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorMatchPool.listOfTopics);
                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                    isValid = true;
                                                }
                                                break;
                                            case "AuthorityCampaign":
                                                if (campaign.authorityCampaignPool != null)
                                                {
                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.authorityCampaignPool.listOfTopics);
                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                    isValid = true;
                                                }
                                                break;
                                            case "AuthorityGeneral":
                                                if (campaign.authorityGeneralPool != null)
                                                {
                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.authorityGeneralPool.listOfTopics);
                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                    isValid = true;
                                                }
                                                break;
                                            case "AuthorityTeam":
                                                if (campaign.teamPool != null)
                                                {
                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.teamPool.listOfTopics);
                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                    isValid = true;
                                                }
                                                break;
                                            case "ResistanceCampaign":
                                                if (campaign.resistanceCampaignPool != null)
                                                {
                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.resistanceCampaignPool.listOfTopics);
                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                    isValid = true;
                                                }
                                                break;
                                            case "ResistanceGeneral":
                                                if (campaign.resistanceGeneralPool != null)
                                                {
                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.resistanceGeneralPool.listOfTopics);
                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                    isValid = true;
                                                }
                                                break;
                                            case "CampaignAlpha":
                                                if (campaign.campaignAlphaPool != null)
                                                {
                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignAlphaPool.listOfTopics);
                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                    isValid = true;
                                                }
                                                break;
                                            case "CampaignBravo":
                                                if (campaign.campaignBravoPool != null)
                                                {
                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignBravoPool.listOfTopics);
                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                    isValid = true;
                                                }
                                                break;
                                            case "CampaignCharlie":
                                                if (campaign.campaignCharliePool != null)
                                                {
                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignCharliePool.listOfTopics);
                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                    isValid = true;
                                                }
                                                break;
                                            case "CitySub":
                                                if (city.cityPool != null)
                                                {
                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, city.cityPool.listOfTopics);
                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                    isValid = true;
                                                }
                                                break;
                                            case "FamilyAlpha":
                                                if (campaign.familyAlphaPool != null)
                                                {
                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.familyAlphaPool.listOfTopics);
                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                    isValid = true;
                                                }
                                                break;
                                            case "FamilyBravo":
                                                if (campaign.familyBravoPool != null)
                                                {
                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.familyBravoPool.listOfTopics);
                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                    isValid = true;
                                                }
                                                break;
                                            case "FamilyCharlie":
                                                if (campaign.familyCharliePool != null)
                                                {
                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.familyCharliePool.listOfTopics);
                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                    isValid = true;
                                                }
                                                break;
                                            case "HQSub":
                                                if (campaign.hqPool != null)
                                                {
                                                    GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.hqPool.listOfTopics);
                                                    AddTopicTypeToList(listOfTopicTypesLevel, topicType);
                                                    isValid = true;
                                                }
                                                break;
                                            default:
                                                Debug.LogWarningFormat("Unrecognised topicSubType \"{0}\" for topicType \"{1}\"", topicSubType.name, topicType.name);
                                                break;
                                        }
                                        //set subType topicData 'isAvailable' to appropriate value
                                        TopicData data = GameManager.instance.dataScript.GetTopicSubTypeData(topicSubType.name);
                                        if (data != null)
                                        { data.isAvailable = isValid; }
                                        else
                                        { Debug.LogErrorFormat("Invalid topicData (Null) for topicSubType \"{0}\"", topicSubType.name); }
                                    }
                                    else { Debug.LogWarningFormat("Invalid topicSubType (Null) for topicType \"{0}\" in listOFSubTypes[{1}]", topicType.name, j); }
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

    //
    //  - - - Turn Topic - - -
    //

    

    /// <summary>
    /// Generates a decision or information topic for the turn (there will always be one)
    /// </summary>
    public void ProcessTopic()
    {
        CheckForValidTopicTypes();
    }

    /// <summary>
    /// Check all level topic types to see if they are valid for the Turn. Updates listOfTopicTypesTurn with valid topicTypes
    /// </summary>
    private void CheckForValidTopicTypes()
    {
        List<TopicType> listOfTopicTypesLevel = GameManager.instance.dataScript.GetListOfTopicTypesLevel();
        if (listOfTopicTypesLevel != null)
        {
            string criteriaCheck;
            int turn = GameManager.instance.turnScript.Turn;
            //clear out turn list prior to updating
            listOfTopicTypesTurn.Clear();
            //loop list of Topic Types
            foreach (TopicType topicType in listOfTopicTypesLevel)
            {
                TopicData topicData = GameManager.instance.dataScript.GetTopicTypeData(topicType.name);
                if (topicData != null)
                {
                    //check topicData
                    if (CheckTopicData(topicData, turn) == true)
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
                    else { Debug.LogFormat("[Top] TopicManager.cs -> CheckForValidTopics: topicType \"{0}\" Failed TopicData check{1}", topicType.tag, "\n"); }
                }
                else { Debug.LogError("Invalid topicData (Null)"); }
            }
        }
        else { Debug.LogError("Invalid listOfTopicTypesLevel (Null)"); }
    }

    //
    // - - - TopicData - - -
    //

    /*/// <summary>
    /// returns true if Topic Data check passes, false otherwise. Default NO check for isAvailable
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private bool CheckTopicData(TopicData data, int turn, bool isAvailableCheck = false)
    {
        bool isValid = false;
        bool isProceed = true;
        if (data != null)
        {
            //check isAvaiable
            if (isAvailableCheck == true)
            {
                if (data.isAvailable == true)
                { isValid = true; }
                else { isProceed = false; }
            }
            if (isProceed == true)
            {
                //check for global interval
                if ((turn - data.turnLastUsed) >= minTopicTypeTurns)
                { isValid = true; }
                else
                {
                    isProceed = false;
                    isValid = false;
                }
                if (isProceed == true)
                {
                    //check for minimum Interval
                    if ((turn - data.turnLastUsed) >= data.minInterval)
                    { isValid = true; }
                }
            }
        }
        else { Debug.LogError("Invalid TopicData (Null)"); }
        return isValid;
    }*/

    /// <summary>
    /// returns true if Topic Data check passes, false otherwise. 
    /// TopicTypes test for global and topicData.minIntervals
    /// TopicSubTypes test for topicData.isAvailable and topicData.minIntervals
    /// </summary>
    /// <param name="data"></param>
    /// <param name="turn"></param>
    /// <param name="isTopicSubType"></param>
    /// <returns></returns>
    private bool CheckTopicData(TopicData data, int turn, bool isTopicSubType = false)
    {
        bool isValid = false;
        bool isProceed = true;
        if (data != null)
        {
            switch(isTopicSubType)
            {
                case false:
                    //TopicTypes -> check global interval & data.minInterval
                    //check for global interval
                    if ((turn - data.turnLastUsed) >= minTopicTypeTurns)
                    { isValid = true; }
                    else
                    {
                        isProceed = false;
                        isValid = false;
                    }
                    if (isProceed == true)
                    {
                        //check for minimum Interval
                        if ((turn - data.turnLastUsed) >= data.minInterval)
                        { isValid = true; }
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
                        //check for minimum Interval
                        if ((turn - data.turnLastUsed) >= data.minInterval)
                        { isValid = true; }
                    }
                    break;
            }
        }
        else { Debug.LogError("Invalid TopicData (Null)"); }
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
                        //check subType topicData
                        TopicData data = GameManager.instance.dataScript.GetTopicSubTypeData(subType.name);
                        if (data != null)
                        {
                            if (CheckTopicData(data, turn, true) == true)
                            {
                                //TO DO check subType criteria (bypass the topic checks if fail) 

                                //loop pool of topics looking for any that are active. Break on the first active topic (only needs to be one)
                                for (int j = 0; j < listOfTopics.Count; j++)
                                {
                                    Topic topic = listOfTopics[j];
                                    if (topic != null)
                                    {
                                        //check status
                                        if (topic.status == Status.Active)
                                        {
                                            //TO DO check topic criteria

                                            //at least one valid topic present, exit
                                            isValid = true;

                                            //TO DO set subTopic availability

                                            break;
                                        }
                                    }
                                    else { Debug.LogErrorFormat("Invalid topic (Null) for subTopicType \"{0}\" listOfTopics[{1}]", subType.name, j); }
                                }
                                //check successful if any one subType of the topicType is valid
                                if (isValid == true)
                                { break; }
                            }
                        }
                        else { Debug.LogErrorFormat("Invalid topicData (Null) for \"{0} / {1}\"", topicType.name, subType.name); }
                    }
                }
                else { Debug.LogErrorFormat("Invalid subType (Null) for topic \"{0}\" listOfSubTypes[{1}]", topicType.name, i); }
            }
        }
        return isValid;
    }


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
    #endregion

    #region Meta Methods
    //
    // - - - MetaManager - - -
    //

    /// <summary>
    /// Runs at end of level, MetaManager.cs -> ProcessMetaGame, to clear out relevant topic data and update others (eg. timesUsedCampaign += timesUsedLevel)
    /// </summary>
    /// <param name="dictOfTopicData"></param>
    public void ProcessMetaTopics()
    {
        //dictOfType/SubType
        UpdateTopicTypes(GameManager.instance.dataScript.GetDictOfTopicTypeData());
        UpdateTopicTypes(GameManager.instance.dataScript.GetDictOfTopicSubTypeData());
    }

    /// <summary>
    /// subMethod for ProcessMetaTopics to handle dictOfTopicType/SubType
    /// </summary>
    /// <param name="dictOfTopicData"></param>
    private void UpdateTopicTypes(Dictionary<string, TopicData> dictOfTopicData)
    {
        foreach (var record in dictOfTopicData)
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
            foreach(var topic in dictOfTopics)
            {
                if (Random.Range(0, 100) < 50)
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
        Dictionary<string, TopicData> dictOfTopicTypes = GameManager.instance.dataScript.GetDictOfTopicTypeData();
        if (dictOfTopicTypes != null)
        {
            Dictionary<string, TopicData> dictOfTopicSubTypes = GameManager.instance.dataScript.GetDictOfTopicSubTypeData();
            if (dictOfTopicSubTypes != null)
            {
                builder.AppendFormat("- TopicData for TopicTypes{0}{1}", "\n", "\n");
                //loop topic Types
                foreach (var topicType in dictOfTopicTypes)
                {
                    builder.AppendFormat(" {0}", DebugDisplayTypeRecord(topicType.Value));
                    //look for any matching SubTypes
                    foreach (var topicSubType in dictOfTopicSubTypes)
                    {
                        if (topicSubType.Value.parent.Equals(topicType.Key, StringComparison.Ordinal) == true)
                        { builder.AppendFormat("  {0}", DebugDisplayTypeRecord(topicSubType.Value)); }
                    }
                    builder.AppendLine();
                }
            }
            else { Debug.LogError("Invalid dictOfTopicSubTypes (Null)"); }
        }
        else { Debug.LogError("Invalid dictOfTopicTypes (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// Sub method for DebugDisplayTopicTypes to display a TopicData record
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private string DebugDisplayTypeRecord(TopicData data)
    {
        return string.Format(" {0} Av {1}, Last {2}, MinInt {3}, #Lvl {4}, #Cmp {5}{6}", data.type, data.isAvailable, data.turnLastUsed, data.minInterval,
                        data.timesUsedLevel, data.timesUsedCampaign, "\n");
    }

    /// <summary>
    /// display two lists of topic Types
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayTopicTypeLists()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- listOfTopicTypes{0}", "\n");
        //listOfTopicTypes
        List<TopicType> listOfTopicTypes = GameManager.instance.dataScript.GetListOfTopicTypes();
        if (listOfTopicTypes != null)
        {
            foreach (TopicType topicType in listOfTopicTypes)
            {
                builder.AppendFormat("{0} {1}, priority: {2}, minInt {3}{4}", "\n", topicType.tag, topicType.priority.name, topicType.minimumInterval, "\n");
                foreach (Criteria criteria in topicType.listOfCriteria)
                { builder.AppendFormat("  criteria: \"{0}\", {1}{2}", criteria.name, criteria.description, "\n"); }
            }
        }
        else { Debug.LogError("Invalid listOfTopicTypes (Null)"); }
        //listOfTopicTypesLevel
        builder.AppendFormat("{0}{1}- listOfTopicTypesLevel{2}", "\n", "\n", "\n");
        List<TopicType> listOfTopicTypeLevel = GameManager.instance.dataScript.GetListOfTopicTypesLevel();
        if (listOfTopicTypeLevel != null)
        {
            foreach (TopicType typeLevel in listOfTopicTypeLevel)
            { builder.AppendFormat(" {0}{1}", typeLevel.tag, "\n"); }
        }
        else { Debug.LogError("Invalid listOfTopicTypesLevel (Null)"); }
        //listOfTopicTypesTurn
        builder.AppendFormat("{0}- listOfTopicTypesTurn{1}", "\n", "\n");
        if (listOfTopicTypesTurn != null)
        {
            foreach (TopicType typeTurn in listOfTopicTypesTurn)
            { builder.AppendFormat(" {0}{1}", typeTurn.tag, "\n"); }
        }
        else { Debug.LogError("Invalid listOfTopicTypesTurn (Null)"); }
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
                                    { builder.AppendFormat("     {0}, {1}, {2} x Op, St: {3}{4}", topic.name, topic.tag, topic.listOfOptions?.Count, topic.status, "\n"); }
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
    #endregion

    //new methods above here
}
