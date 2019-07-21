using System;
using System.Collections;
using System.Collections.Generic;
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

    //Sub method for DebugDisplayTopicTypes to display a TopicData record
    private string DisplayTypeRecord(TopicData data)
    {
        return string.Format(" {0} Av {1}, Last {2}, MinInt {3}, #Lvl {4}, #Cmp {5}{6}", data.type, data.isAvailable, data.turnLastUsed, data.minInterval,
                        data.timesUsedLevel, data.timesUsedCampaign, "\n");
    }
    //new methods above here
}
