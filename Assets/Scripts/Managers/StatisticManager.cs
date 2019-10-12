using gameAPI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// handles all in-game statistics with the exception of SO specific statistics
/// </summary>
public class StatisticManager : MonoBehaviour
{
    #region Save Compatible Data
    [HideInInspector] public float ratioPlayerNodeActions;
    [HideInInspector] public float ratioPlayerTargetAttempts;
    [HideInInspector] public float ratioPlayerMoveActions;
    [HideInInspector] public float ratioPlayerLieLowDays;
    [HideInInspector] public float ratioPlayerGiveGear;
    [HideInInspector] public float ratioPlayerManageActions;
    [HideInInspector] public float ratioPlayerDoNothing;
    #endregion

    public void InitialiseEarly(GameState state)
    {
        switch (state)
        {
            case GameState.StartUp:
                SubInitialiseSessionStart();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\" (InitialiseEarly)", GameManager.instance.inputScript.GameState);
                break;
        }
    }

    /// <summary>
    /// Not for GameState.LoadGame
    /// </summary>
    public void InitialiseLate(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
            case GameState.FollowOnInitialisation:
            case GameState.LoadAtStart:
                SubInitialiseSessionInProgress();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\" (InitialiseLate)", GameManager.instance.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseSessionStart
    /// <summary>
    /// loops statType enum and initialises all statistics
    /// </summary>
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

    #region ProcessMetaStatics
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
    #endregion

    #region UpdateRatios
    /// <summary>
    /// Method to update all ratios at start of each turn. 
    /// NOTE: Calculated values will be overriden by relevant TestManager.cs ratio if non-zero (doesn't apply during an autoRun)
    /// </summary>
    public void UpdateRatios()
    {
        float dataTop, dataBottom;
        bool isAutoRun = GameManager.instance.turnScript.CheckIsAutoRun();
        int turn = GameManager.instance.turnScript.Turn;
        //PlayerNodeActions
        if (isAutoRun == false && GameManager.instance.testScript.testRatioPlayNodeAct == 0)
        {
            dataTop = GameManager.instance.dataScript.StatisticGetLevel(StatType.PlayerNodeActions);
            dataBottom = GameManager.instance.dataScript.StatisticGetLevel(StatType.NodeActionsResistance);
            if (dataTop != 0 && dataBottom != 0)
            { ratioPlayerNodeActions = dataTop / dataBottom; }
            else { ratioPlayerNodeActions = 0; }
        }
        else { ratioPlayerNodeActions = GameManager.instance.testScript.testRatioPlayNodeAct; }
        //PlayerTargetAttempts
        if (isAutoRun == false && GameManager.instance.testScript.testRatioPlayTargetAtt == 0)
        {
            dataTop = GameManager.instance.dataScript.StatisticGetLevel(StatType.PlayerTargetAttempts);
            dataBottom = GameManager.instance.dataScript.StatisticGetLevel(StatType.TargetAttempts);
            if (dataTop != 0 && dataBottom != 0)
            { ratioPlayerTargetAttempts = dataTop / dataBottom; }
            else { ratioPlayerTargetAttempts = 0; }
        }
        else { ratioPlayerTargetAttempts = GameManager.instance.testScript.testRatioPlayTargetAtt; }
        //PlayerMoveActions
        if (isAutoRun == false && GameManager.instance.testScript.testRatioPlayMoveAct == 0)
        {
            dataTop = GameManager.instance.dataScript.StatisticGetLevel(StatType.PlayerMoveActions);
            dataBottom = turn;
            if (dataTop != 0 && dataBottom != 0)
            { ratioPlayerMoveActions = dataTop / dataBottom; }
            else { ratioPlayerMoveActions = 0; }
        }
        else { ratioPlayerMoveActions = GameManager.instance.testScript.testRatioPlayMoveAct; }
        //PlayerLieLowDays
        if (isAutoRun == false && GameManager.instance.testScript.testRatioPlayLieLow == 0)
        {
            dataTop = GameManager.instance.dataScript.StatisticGetLevel(StatType.PlayerLieLowDays);
            dataBottom = GameManager.instance.dataScript.StatisticGetLevel(StatType.LieLowDaysTotal);
            if (dataTop != 0 && dataBottom != 0)
            { ratioPlayerLieLowDays = dataTop / dataBottom; }
            else { ratioPlayerLieLowDays = 0; }
        }
        else { ratioPlayerLieLowDays = GameManager.instance.testScript.testRatioPlayLieLow; }
        //PlayerGiveGear
        if (isAutoRun == false && GameManager.instance.testScript.testRatioPlayGiveGear == 0)
        {
            dataTop = GameManager.instance.dataScript.StatisticGetLevel(StatType.PlayerGiveGear);
            dataBottom = GameManager.instance.dataScript.StatisticGetLevel(StatType.GearTotal);
            if (dataTop != 0 && dataBottom != 0)
            { ratioPlayerGiveGear = dataTop / dataBottom; }
            else { ratioPlayerGiveGear = 0; }
        }
        else { ratioPlayerGiveGear = GameManager.instance.testScript.testRatioPlayGiveGear; }
        //PlayerManageActions
        if (isAutoRun == false && GameManager.instance.testScript.testRatioPlayManageAct == 0)
        {
            dataTop = GameManager.instance.dataScript.StatisticGetLevel(StatType.PlayerManageActions);
            dataBottom = turn;
            if (dataTop != 0 && dataBottom != 0)
            { ratioPlayerManageActions = dataTop / dataBottom; }
            else { ratioPlayerManageActions = 0; }
        }
        else { ratioPlayerManageActions = GameManager.instance.testScript.testRatioPlayManageAct; }
        //PlayerDoNothing
        if (isAutoRun == false && GameManager.instance.testScript.testRatioDoNothing == 0)
        {
            dataTop = GameManager.instance.dataScript.StatisticGetLevel(StatType.PlayerDoNothing);
            dataBottom = turn;
            if (dataTop != 0 && dataBottom != 0)
            { ratioPlayerDoNothing = dataTop / dataBottom; }
            else { ratioPlayerDoNothing = 0; }
        }
        else { ratioPlayerManageActions = GameManager.instance.testScript.testRatioDoNothing; }
    }
    #endregion

    #region Debug Methods
    //
    // - - - Debug Methods
    //

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
        {
            builder.AppendFormat("{0}: {1} (total {2}){3}", (StatType)stat, GameManager.instance.dataScript.StatisticGetLevel((StatType)stat),
              GameManager.instance.dataScript.StatisticGetCampaign((StatType)stat), "\n");
        }
        return builder.ToString();
    }

    /// <summary>
    /// Display statistic ratios
    /// </summary>
    /// <returns></returns>
    public string DebugShowRatios()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- Ratios{0}{1}", "\n", "\n");
        builder.AppendFormat(" ratioPlayerNodeActions: {0}{1}", ratioPlayerNodeActions, "\n");
        builder.AppendFormat(" ratioPlayerTargetAttempts: {0}{1}", ratioPlayerTargetAttempts, "\n");
        builder.AppendFormat(" ratioPlayerMoveActions: {0}{1}", ratioPlayerMoveActions, "\n");
        builder.AppendFormat(" ratioPlayerLieLowDays: {0}{1}", ratioPlayerLieLowDays, "\n");
        builder.AppendFormat(" ratioPlayerGiveGear: {0}{1}", ratioPlayerGiveGear, "\n");
        builder.AppendFormat(" ratioPlayerManageActions: {0}{1}", ratioPlayerManageActions, "\n");
        builder.AppendFormat(" ratioPlayerDoNothing: {0}{1}", ratioPlayerDoNothing, "\n");

        return builder.ToString();
    }


    #endregion

    //new methods above here
}
