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
    /// Display statistics
    /// </summary>
    /// <returns></returns>
    public string DebugShowStatistics()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("-Statistics{0}{1}", "\n", "\n");
        //loop each stat type
        foreach (var stat in Enum.GetValues(typeof(StatType)))
        { builder.AppendFormat("{0}: {1}{2}", (StatType)stat, GameManager.instance.dataScript.StatisticGet((StatType)stat), "\n"); }
        return builder.ToString();
    }


    //new methods above here
}
