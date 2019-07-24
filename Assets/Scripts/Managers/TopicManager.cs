﻿using gameAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Human decision system
/// </summary>
public class TopicManager : MonoBehaviour
{
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
        //establish which TopicTypes are valid for the level
        CheckForValidTopics();
        UpdateTopicPools();
    }
    #endregion

    #endregion

    //
    // - - - Session Start - - -
    //

    /// <summary>
    /// Check all topic types to see if they are valid for the level (if so copied to DataManager.cs listOfTopicTypesLevel)
    /// </summary>
    private void CheckForValidTopics()
    {
        List<TopicType> listOfTopicTypes = GameManager.instance.dataScript.GetListOfTopicTypes();
        List<TopicType> listOfTopicTypesLevel = GameManager.instance.dataScript.GetListOfTopicTypesLevel();
        if (listOfTopicTypes != null)
        {
            if (listOfTopicTypesLevel != null)
            {
                string criteriaCheck;
                //loop list
                foreach (TopicType topicType in listOfTopicTypes)
                {
                    TopicData topicData = GameManager.instance.dataScript.GetTopicTypeData(topicType.name);
                    if (topicData != null)
                    {
                        CriteriaDataInput criteriaInput = new CriteriaDataInput()
                        { listOfCriteria = topicType.listOfCriteria };
                        criteriaCheck = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
                        if (criteriaCheck == null)
                        {
                            //criteria check passed O.K
                            topicData.isAvailable = true;
                            listOfTopicTypesLevel.Add(topicType);
                        }
                        else
                        {
                            //criteria check FAILED
                            topicData.isAvailable = false;
                            //generate message explaining why criteria failed
                            Debug.LogFormat("[Top] TopicManager.cs -> CheckForValidTopics: topicType \"{0}\" FAILED Criteria check due to {1}{2}", topicType.tag, criteriaCheck, "\n");
                        }
                    }
                    else { Debug.LogError("Invalid topicData (Null)"); }
                }
            }
            else { Debug.LogError("Invalid listOfTopicTypesLevel (Null)"); }
        }
        else { Debug.LogError("Invalid listOfTopicTypes (Null)"); }
    }

    /// <summary>
    /// Populates DataManager.cs -> dictOfTopicPools (at start of a level) with all valid topics for the current level
    /// </summary>
    public void UpdateTopicPools()
    {
        /*//Debug loads in all topics in dictOfTopics. Need to replace.
        Dictionary<string, Topic> dictOfTopics = GameManager.instance.dataScript.GetDictOfTopics();
        if (dictOfTopics != null)
        {
            //get all valid topic types for level
            List<TopicType> listOfTopicTypes = GameManager.instance.dataScript.GetListOfTopicTypesLevel();
            if (listOfTopicTypes != null)
            {
                foreach (TopicType topicType in listOfTopicTypes)
                {
                    if (topicType != null)
                    {
                        //Loop list Of topic subTypes
                        foreach (TopicSubType subTopicType in topicType.listOfSubTypes)
                        {
                            if (subTopicType != null)
                            {
                                IEnumerable<Topic> topicData =
                                    from item in dictOfTopics
                                    where item.Value.subType == subTopicType
                                    select item.Value;
                                List<Topic> listOfTopics = topicData.ToList();
                                if (listOfTopics.Count > 0)
                                { GameManager.instance.dataScript.AddListOfTopicsToPool(subTopicType.name, listOfTopics); }
                            }
                            else { Debug.LogErrorFormat("Invalid TopicSubType (Null) for Topic \"{0}\"", topicType.name); }
                        }
                    }
                    else { Debug.LogError("Invalid topicType (Null) in listOfTopicTypesLevel"); }
                }
            }
            else { Debug.LogError("Invalid listOfTopicTypes (Null)"); }
        }
        else { Debug.LogError("Invalid dictOfTopics (Null)"); }
        */

        //get current topicTypes for the level
        List<TopicType> listOfTopicTypes = GameManager.instance.dataScript.GetListOfTopicTypesLevel();
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
                                    switch (topicSubType.name)
                                    {
                                        case "ActorPolitic":
                                            if (campaign.actorPoliticPool != null)
                                            { GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorPoliticPool.listOfTopics); }
                                            break;
                                        case "ActorContact":
                                            if (campaign.actorContactPool != null)
                                            { GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorContactPool.listOfTopics); }
                                            break;
                                        case "ActorDistrict":
                                            if (campaign.actorDistrictPool != null)
                                            { GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorDistrictPool.listOfTopics); }
                                            break;
                                        case "ActorGear":
                                            if (campaign.actorGearPool != null)
                                            { GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorGearPool.listOfTopics); }
                                            break;
                                        case "ActorMatch":
                                            if (campaign.actorMatchPool != null)
                                            { GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.actorMatchPool.listOfTopics); }
                                            break;
                                        case "AuthorityCampaign":
                                            if (campaign.authorityCampaignPool != null)
                                            { GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.authorityCampaignPool.listOfTopics); }
                                            break;
                                        case "AuthorityGeneral":
                                            if (campaign.authorityGeneralPool != null)
                                            { GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.authorityGeneralPool.listOfTopics); }
                                            break;
                                        case "AuthorityTeam":
                                            if (campaign.teamPool != null)
                                            { GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.teamPool.listOfTopics); }
                                            break;
                                        case "ResistanceCampaign":
                                            if (campaign.resistanceCampaignPool != null)
                                            { GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.resistanceCampaignPool.listOfTopics); }
                                            break;
                                        case "ResistanceGeneral":
                                            if (campaign.resistanceGeneralPool != null)
                                            { GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.resistanceGeneralPool.listOfTopics); }
                                            break;
                                        case "CampaignAlpha":
                                            if (campaign.campaignAlphaPool != null)
                                            { GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignAlphaPool.listOfTopics); }
                                            break;
                                        case "CampaignBravo":
                                            if (campaign.campaignBravoPool != null)
                                            { GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignBravoPool.listOfTopics); }
                                            break;
                                        case "CampaignCharlie":
                                            if (campaign.campaignCharliePool != null)
                                            { GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.campaignCharliePool.listOfTopics); }
                                            break;
                                        case "CitySub":
                                            if (city.cityPool != null)
                                            { GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, city.cityPool.listOfTopics); }
                                            break;
                                        case "FamilyAlpha":
                                            if (campaign.familyAlphaPool != null)
                                            { GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.familyAlphaPool.listOfTopics); }
                                            break;
                                        case "FamilyBravo":
                                            if (campaign.familyBravoPool != null)
                                            { GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.familyBravoPool.listOfTopics); }
                                            break;
                                        case "FamilyCharlie":
                                            if (campaign.familyCharliePool != null)
                                            { GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.familyCharliePool.listOfTopics); }
                                            break;
                                        case "HQSub":
                                            if (campaign.hqPool != null)
                                            { GameManager.instance.dataScript.AddListOfTopicsToPool(subTypeName, campaign.hqPool.listOfTopics); }
                                            break;
                                        default:
                                            Debug.LogWarningFormat("Unrecognised topicSubType \"{0}\" for topicType \"{1}\"", topicSubType.name, topicType.name);
                                            break;
                                    }
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
        else { Debug.LogError("Invalid listOfTopicTypes by Level (Null)"); }
    }

    //
    // - - - Topic Availability - - -
    //

    /// <summary>
    /// returns true if Actor topics are available. Run at level Start. Called by EffectManager.cs -> CheckCriteria
    /// </summary>
    /// <returns></returns>
    public bool CheckTopicActor()
    {
        return true;
    }

    /// <summary>
    /// returns true if Campaign topics are available. Run at level Start. Called by EffectManager.cs -> CheckCriteria
    /// </summary>
    /// <returns></returns>
    public bool CheckTopicCampaign()
    {
        return true;
    }

    /// <summary>
    /// returns true if City topics are available. Run at level Start. Called by EffectManager.cs -> CheckCriteria
    /// </summary>
    /// <returns></returns>
    public bool CheckTopicCity()
    {
        return true;
    }

    /// <summary>
    /// returns true if Family topics are available. Run at level Start. Called by EffectManager.cs -> CheckCriteria
    /// </summary>
    /// <returns></returns>
    public bool CheckTopicFamily()
    {
        return true;
    }

    /// <summary>
    /// returns true if Authority topics are available. Run at level Start. Called by EffectManager.cs -> CheckCriteria
    /// </summary>
    /// <returns></returns>
    public bool CheckTopicAuthority()
    {
        return true;
    }

    /// <summary>
    /// returns true if Resistance topics are available. Run at level Start. Called by EffectManager.cs -> CheckCriteria
    /// </summary>
    /// <returns></returns>
    public bool CheckTopicRebel()
    {
        return true;
    }

    /// <summary>
    /// returns true if HQ topics are available. Run at level Start. Called by EffectManager.cs -> CheckCriteria
    /// </summary>
    /// <returns></returns>
    public bool CheckTopicHQ()
    {
        return true;
    }

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

    //
    // - - - Debug - - -
    //

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
                                    { builder.AppendFormat("     {0}, {1}, {2} options{3}", topic.name, topic.tag, topic.listOfOptions?.Count, "\n"); }
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


    //new methods above here
}
