using gameAPI;
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
                        switch (topicSubType.name)
                        {
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
                            case "AuthorityCampaign":

                                break;
                            case "AuthorityGeneral":

                                break;
                            case "AuthorityTeam":

                                break;
                            case "ResistanceCampaign":

                                break;
                            case "ResistanceGeneral":

                                break;
                            case "CampaignAlpha":

                                break;
                            case "CampaignBravo":

                                break;
                            case "CampaignCharlie":

                                break;
                            case "CitySub":

                                break;
                            case "FamilySub":

                                break;
                            case "HQSub":

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
                                Debug.LogWarningFormat("Invalid listOfTopics (Null) for topicSubType {0}", subType.name);
                                builder.AppendFormat("    Invalid list (Error){0}", "\n");
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarningFormat("Invalid listOfSubTypes (Null) for topicType \"{0}\"{1}", topicType, "\n");
                    builder.AppendFormat("  None found (Error){0}", "\n");
                }
            }
        }
        else { Debug.LogError("Invalid listOfTopicTypesLevel (Null)"); }
        return builder.ToString();
    }


    //new methods above here
}
