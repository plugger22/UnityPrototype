using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gameAPI;
using packageAPI;
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
        //Debug loads in all topics in dictOfTopics. Need to replace.
        Dictionary<string, Topic> dictOfTopics = GameManager.instance.dataScript.GetDictOfTopics();
        List<Topic> listOfTopics = new List<Topic>();
        if (dictOfTopics != null)
        {
            //get all valid topic types for level
            List<TopicType> listOfTopicTypes = GameManager.instance.dataScript.GetListOfTopicTypesLevel();
            if (listOfTopicTypes != null)
            {
                foreach(TopicType topicType in listOfTopicTypes)
                {
                    if (topicType != null)
                    {
                        //Loop list Of topic subTypes
                        foreach(TopicSubType subTopicType in topicType.listOfSubTypes)
                        {
                            if (subTopicType != null)
                            {
                                listOfTopics.Clear();
                                /*listOfTopics = dictOfTopics.Select(t => t.Value.subType.name.Equals(subTopicType.name, StringComparison.Ordinal)).ToList();*/
                                IEnumerable<Topic> topicData =
                                    from item in dictOfTopics
                                    where item.Value.subType == subTopicType
                                    select item.Value;
                                listOfTopics = topicData.ToList();
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
        return false;
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
                foreach(var topicType in dictOfTopicTypes)
                {
                    builder.AppendFormat(" {0}", DisplayTypeRecord(topicType.Value));
                    //look for any matching SubTypes
                    foreach(var topicSubType in dictOfTopicSubTypes)
                    {
                        if (topicSubType.Value.parent.Equals(topicType.Key, StringComparison.Ordinal) == true)
                        { builder.AppendFormat("  {0}", DisplayTypeRecord(topicSubType.Value)); }
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
    private string DisplayTypeRecord(TopicData data)
    {
        return string.Format(" {0} Av {1}, Last {2}, MinInt {3}, #Lvl {4}, #Cmp {5}{6}", data.type, data.isAvailable, data.turnLastUsed, data.minInterval,
                        data.timesUsedLevel, data.timesUsedCampaign, "\n");
    }

    /// <summary>
    /// display two lists of topic Types
    /// </summary>
    /// <returns></returns>
    public string DisplayTopicTypeLists()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- listOfTopicTypes{0}", "\n");
        //listOfTopicTypes
        List<TopicType> listOfTopicTypes = GameManager.instance.dataScript.GetListOfTopicTypes();
        if (listOfTopicTypes != null)
        {
            foreach(TopicType topicType in listOfTopicTypes)
            {
                builder.AppendFormat("{0} {1}, priority: {2}, minInt {3}{4}", "\n", topicType.tag, topicType.priority.name, topicType.minimumInterval, "\n");
                foreach(Criteria criteria in topicType.listOfCriteria)
                { builder.AppendFormat("  criteria: \"{0}\", {1}{2}", criteria.name, criteria.description, "\n"); }
            }
        }
        else { Debug.LogError("Invalid listOfTopicTypes (Null)"); }
        //listOfTopicTypesLevel
        builder.AppendFormat("{0}{1}- listOfTopicTypesLevel{2}", "\n", "\n", "\n");
        List<TopicType> listOfTopicTypeLevel = GameManager.instance.dataScript.GetListOfTopicTypesLevel();
        if (listOfTopicTypeLevel != null)
        {
            foreach(TopicType typeLevel in listOfTopicTypeLevel)
            { builder.AppendFormat(" {0}{1}", typeLevel.tag, "\n"); }
        }
        else { Debug.LogError("Invalid listOfTopicTypesLevel (Null)"); }
        return builder.ToString();
    }


    //new methods above here
}
