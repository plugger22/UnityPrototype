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
                SubInitialiseStartSession();
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

    #region SubInitialiseStartSession
    private void SubInitialiseStartSession()
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
    /// Check all topic types to see if they are valid for the level
    /// </summary>
    private void CheckForValidTopics()
    {
        Dictionary<string, TopicData> dictOfTopicTypes = GameManager.instance.dataScript.GetDictOfTopicTypes();
        if (dictOfTopicTypes != null)
        {
            foreach(var topicType in dictOfTopicTypes)
            {

            }
        }
        else { Debug.LogError("Invalid dictOfTopicTypes (Null)"); }
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
        UpdateTopicTypes(GameManager.instance.dataScript.GetDictOfTopicTypes());
        UpdateTopicTypes(GameManager.instance.dataScript.GetDictOfTopicSubTypes());
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
        Dictionary<string, TopicData> dictOfTopicTypes = GameManager.instance.dataScript.GetDictOfTopicTypes();
        if (dictOfTopicTypes != null)
        {
            Dictionary<string, TopicData> dictOfTopicSubTypes = GameManager.instance.dataScript.GetDictOfTopicSubTypes();
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
