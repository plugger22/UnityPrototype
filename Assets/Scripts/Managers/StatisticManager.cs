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
    [HideInInspector] public float ratioPlayerAddictedDays;
    #endregion

    public void InitialiseEarly(GameState state)
    {
        switch (state)
        {
            case GameState.StartUp:
                SubInitialiseSessionStart();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\" (InitialiseEarly)", GameManager.i.inputScript.GameState);
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
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\" (InitialiseLate)", GameManager.i.inputScript.GameState);
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
        { GameManager.i.dataScript.StatisticAddNew((StatType)stat); }
    }
    #endregion

    #region SubInitialiseSessionInProgress
    private void SubInitialiseSessionInProgress()
    {
        //session underway -> reset trackers
        GameManager.i.dataScript.StatisticReset();
    }
    #endregion

    #endregion

    #region ProcessMetaStatics
    /// <summary>
    /// Update campaign statistics with previous level stats and clear out level stats ready for new level
    /// </summary>
    public void ProcessMetaStatistics()
    {
        Dictionary<StatType, int> dictOfStatsLevel = GameManager.i.dataScript.GetDictOfStatisticsLevel();
        if (dictOfStatsLevel != null)
        {
            Dictionary<StatType, int> dictOfStatsCampaign = GameManager.i.dataScript.GetDictOfStatisticsCampaign();
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
        GameManager.i.dataScript.StatisticReset();
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
        bool isAutoRun = GameManager.i.turnScript.CheckIsAutoRun();
        int turn = GameManager.i.turnScript.Turn;
        //adjust turn to take into account lie low, captured and breakdown days for the Player
        turn -= GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerLieLowDays);
        turn -= GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerBreakdown);
        turn -= GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerCapturedDays);
        turn = Mathf.Max(1, turn);
        //PlayerNodeActions
        if (isAutoRun == false && GameManager.i.testScript.testRatioPlayNodeAct == 0)
        {
            dataTop = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerNodeActions);
            dataBottom = GameManager.i.dataScript.StatisticGetLevel(StatType.NodeActionsResistance);
            if (dataTop != 0 && dataBottom != 0)
            { ratioPlayerNodeActions = dataTop / dataBottom; }
            else { ratioPlayerNodeActions = 0; }
        }
        else { ratioPlayerNodeActions = GameManager.i.testScript.testRatioPlayNodeAct; }
        //PlayerTargetAttempts
        if (isAutoRun == false && GameManager.i.testScript.testRatioPlayTargetAtt == 0)
        {
            dataTop = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerTargetAttempts);
            dataBottom = GameManager.i.dataScript.StatisticGetLevel(StatType.TargetAttempts);
            if (dataTop != 0 && dataBottom != 0)
            { ratioPlayerTargetAttempts = dataTop / dataBottom; }
            else { ratioPlayerTargetAttempts = 0; }
        }
        else { ratioPlayerTargetAttempts = GameManager.i.testScript.testRatioPlayTargetAtt; }
        //PlayerMoveActions
        if (isAutoRun == false && GameManager.i.testScript.testRatioPlayMoveAct == 0)
        {
            dataTop = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerMoveActions);
            dataBottom = turn;
            if (dataTop != 0 && dataBottom != 0)
            { ratioPlayerMoveActions = dataTop / dataBottom; }
            else { ratioPlayerMoveActions = 0; }
        }
        else { ratioPlayerMoveActions = GameManager.i.testScript.testRatioPlayMoveAct; }
        //PlayerLieLowDays
        if (isAutoRun == false && GameManager.i.testScript.testRatioPlayLieLow == 0)
        {
            dataTop = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerLieLowDays);
            dataBottom = GameManager.i.dataScript.StatisticGetLevel(StatType.LieLowDaysTotal);
            if (dataTop != 0 && dataBottom != 0)
            { ratioPlayerLieLowDays = dataTop / dataBottom; }
            else { ratioPlayerLieLowDays = 0; }
        }
        else { ratioPlayerLieLowDays = GameManager.i.testScript.testRatioPlayLieLow; }
        //PlayerGiveGear
        if (isAutoRun == false && GameManager.i.testScript.testRatioPlayGiveGear == 0)
        {
            dataTop = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerGiveGear);
            dataBottom = GameManager.i.dataScript.StatisticGetLevel(StatType.GearTotal);
            if (dataTop != 0 && dataBottom != 0)
            { ratioPlayerGiveGear = dataTop / dataBottom; }
            else { ratioPlayerGiveGear = 0; }
        }
        else { ratioPlayerGiveGear = GameManager.i.testScript.testRatioPlayGiveGear; }
        //PlayerManageActions
        if (isAutoRun == false && GameManager.i.testScript.testRatioPlayManageAct == 0)
        {
            dataTop = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerManageActions);
            dataBottom = turn;
            if (dataTop != 0 && dataBottom != 0)
            { ratioPlayerManageActions = dataTop / dataBottom; }
            else { ratioPlayerManageActions = 0; }
        }
        else { ratioPlayerManageActions = GameManager.i.testScript.testRatioPlayManageAct; }
        //PlayerDoNothing
        if (isAutoRun == false && GameManager.i.testScript.testRatioDoNothing == 0)
        {
            dataTop = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerDoNothing);
            dataBottom = turn;
            if (dataTop != 0 && dataBottom != 0)
            { ratioPlayerDoNothing = dataTop / dataBottom; }
            else { ratioPlayerDoNothing = 0; }
        }
        else { ratioPlayerDoNothing = GameManager.i.testScript.testRatioDoNothing; }
        //PlayerAddictedDays
        if (isAutoRun == false && GameManager.i.testScript.testRatioAddictedDays == 0)
        {
            dataTop = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerAddictedDays);
            dataBottom = turn;
            if (dataTop != 0 && dataBottom != 0)
            { ratioPlayerAddictedDays = dataTop / dataBottom; }
            else { ratioPlayerAddictedDays = 0; }
        }
        else { ratioPlayerAddictedDays = GameManager.i.testScript.testRatioAddictedDays; }
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
            builder.AppendFormat("{0}: {1} (total {2}){3}", (StatType)stat, GameManager.i.dataScript.StatisticGetLevel((StatType)stat),
              GameManager.i.dataScript.StatisticGetCampaign((StatType)stat), "\n");
        }
        return builder.ToString();
    }

    /// <summary>
    /// Display statistic ratios
    /// </summary>
    /// <returns></returns>
    public string DebugShowRatios()
    {
        int turn = GameManager.i.turnScript.Turn;
        //adjust turn to take into account lie low and breakdown days for the Player
        turn -= GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerLieLowDays);
        turn -= GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerBreakdown);
        turn = Mathf.Max(1, turn);
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- Ratios (turns {0}){1}{2}", turn, "\n", "\n");
        builder.AppendFormat(" ratioPlayerNodeActions: {0}{1}", ratioPlayerNodeActions, "\n");
        builder.AppendFormat(" ratioPlayerTargetAttempts: {0}{1}", ratioPlayerTargetAttempts, "\n");
        builder.AppendFormat(" ratioPlayerMoveActions: {0}{1}", ratioPlayerMoveActions, "\n");
        builder.AppendFormat(" ratioPlayerLieLowDays: {0}{1}", ratioPlayerLieLowDays, "\n");
        builder.AppendFormat(" ratioPlayerGiveGear: {0}{1}", ratioPlayerGiveGear, "\n");
        builder.AppendFormat(" ratioPlayerManageActions: {0}{1}", ratioPlayerManageActions, "\n");
        builder.AppendFormat(" ratioPlayerDoNothing: {0}{1}", ratioPlayerDoNothing, "\n");
        builder.AppendFormat(" ratioPlayerAddictedDays: {0}{1}", ratioPlayerAddictedDays, "\n");
        return builder.ToString();
    }


    #endregion

    //new methods above here
}
