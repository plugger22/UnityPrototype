using gameAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// handles all in-game statistics with the exception of SO specific statistics
/// </summary>
public class StatisticManager : MonoBehaviour
{

    /// <summary>
    /// Not for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
            case GameState.FollowOnInitialisation:
            case GameState.LoadAtStart:
                //instantiate statistic trackers if a new session, reset if not (eg. new game started from within an existing game session)
                if (GameManager.instance.isSession == false)
                { SubInitialiseSessionStart(); }
                else
                { SubInitialiseSessionInProgress(); }
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        //instantiate statistic trackers if a new sessession
        foreach (var stat in Enum.GetValues(typeof(StatType)))
        { GameManager.instance.dataScript.StatisticAddNew((StatType)stat); }
    }
    #endregion

    #region SubInitialiseSessionInProgress
    private void SubInitialiseSessionInProgress()
    {
        //session underway -> reset trackers
        GameManager.instance.dataScript.StatisticReset();
    }
    #endregion

    #endregion

    /// <summary>
    /// Update campaign statistics with previous level stats and clear out level stats ready for new level
    /// </summary>
    public void ProcessMetaStatistics()
    {
        Dictionary<StatType, int> dictOfStatsLevel = GameManager.instance.dataScript.GetDictOfStatisticsLevel();
        if (dictOfStatsLevel != null)
        {
            Dictionary<StatType, int> dictOfStatsCampaign = GameManager.instance.dataScript.GetDictOfStatisticsCampaign();
            if (dictOfStatsCampaign != null)
            {
                int statValue;
                //loop enums and update mirrored stats in campaign dict
                foreach (StatType statType in Enum.GetValues(typeof(StatType)))
                {
                    if (dictOfStatsLevel.ContainsKey(statType) == true)
                    {
                        statValue = dictOfStatsLevel[statType];
                        if (dictOfStatsCampaign.ContainsKey(statType) == true)
                        {
                            statValue += dictOfStatsCampaign[statType];
                            dictOfStatsCampaign[statType] = statValue;
                        }
                        else { Debug.LogWarningFormat("StatType \"{0}\" not found in dictOfStatisticsCampaign", statType); }
                    }
                    else { Debug.LogWarningFormat("StatType \"{0}\" not found in dictOfStatisticsLevel", statType); }
                }
            }
            else { Debug.LogError("Invalid dictOfStatisticsCampaign (Null)"); }
        }
        else { Debug.LogError("Invalid dictOfStatisticsLevel (Null)"); }
        //reset all level based statistics back to zero
        GameManager.instance.dataScript.StatisticReset();
    }

    /// <summary>
    /// Display statistics
    /// </summary>
    /// <returns></returns>
    public string DebugShowStatistics()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("-Statistics{0}{1}", "\n", "\n");
        //loop each stat type
        foreach (var stat in Enum.GetValues(typeof(StatType)))
        { builder.AppendFormat("{0}: {1} (total {2}){3}", (StatType)stat, GameManager.instance.dataScript.StatisticGetLevel((StatType)stat), 
            GameManager.instance.dataScript.StatisticGetCampaign((StatType)stat), "\n"); }
        return builder.ToString();
    }


    //new methods above here
}
