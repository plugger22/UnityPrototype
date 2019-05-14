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

    public void Initialise()
    {
        //session specific (once only)
        if (GameManager.instance.inputScript.GameState == GameState.NewInitialisation)
        {
            //instantiate statistic trackers if a new session, reset if not (eg. new game started from within an existing game session)
            if (GameManager.instance.isSession == false)
            {
                //instantiate statistic trackers if a new sessession
                foreach(var stat in Enum.GetValues(typeof(StatType)))
                { GameManager.instance.dataScript.StatisticAddNew((StatType) stat); }
            }
            else { GameManager.instance.dataScript.StatisticReset(); }
        }
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
        { builder.AppendFormat("{0}: {1}{2}", (StatType)stat, GameManager.instance.dataScript.StatisticGet((StatType)stat), "\n"); }
        return builder.ToString();
    }


    //new methods above here
}
