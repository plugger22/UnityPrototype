using System.Collections;
using System.Collections.Generic;
using System.Text;
using packageAPI;
using UnityEngine;

/// <summary>
/// Human decision system
/// </summary>
public class TopicManager : MonoBehaviour
{

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

    /// <summary>
    /// display of dictOfTopicTypes/SubTypes
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayTopicTypeData()
    {
        StringBuilder builder = new StringBuilder();
        //Topic Types
        builder.AppendFormat("- DictOfTopicTypes{0}", "\n");
        builder.Append(DebugDisplayTopicDict(GameManager.instance.dataScript.GetDictOfTopicTypes()));
        //Topic Types
        builder.AppendFormat("{0}- DictOfTopicSubTypes{1}", "\n", "\n");
        builder.Append(DebugDisplayTopicDict(GameManager.instance.dataScript.GetDictOfTopicSubTypes()));
        return builder.ToString();
    }

    /// <summary>
    /// SubMethod to display dict data for Topic Types/SubTypes
    /// </summary>
    /// <param name="dictOfTopicData"></param>
    /// <returns></returns>
    private string DebugDisplayTopicDict(Dictionary<string, TopicData> dictOfTopicData)
    {
        StringBuilder builder = new StringBuilder();
        if (dictOfTopicData != null)
        {
            foreach (var record in dictOfTopicData)
            {
                builder.AppendFormat(" {0} Av {1}, Last {2}, MinInt {3}, #Lvl {4}, #Cmp {5}{6}", record.Value.type, record.Value.isAvailable, record.Value.turnLastUsed, record.Value.minInterval,
                  record.Value.timesUsedLevel, record.Value.timesUsedCampaign, "\n");
            }
        }
        else { Debug.LogError("Invalid dictOfTopicData (Null)"); }
        return builder.ToString();
    }

    //new methods above here
}
