using gameAPI;
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
        //player
        GameManager.instance.dataScript.StatisticAddNew(StatType.PlayerBreakdown);
        GameManager.instance.dataScript.StatisticAddNew(StatType.PlayerLieLow);
        //stress leave
        GameManager.instance.dataScript.StatisticAddNew(StatType.StressLeaveAuthority);
        GameManager.instance.dataScript.StatisticAddNew(StatType.StressLeaveResistance);
    }


    /// <summary>
    /// Display statistics
    /// </summary>
    /// <returns></returns>
    public string DebugShowStatistics()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("-Statistics{0}{1}", "\n", "\n");
        builder.AppendFormat(" Player Breakdowns: {0}{1}", GameManager.instance.dataScript.StatisticGet(StatType.PlayerBreakdown), "\n");
        builder.AppendFormat(" Player Lie Low: {0}{1}{2}", GameManager.instance.dataScript.StatisticGet(StatType.PlayerLieLow), "\n", "\n");
        builder.AppendFormat(" Stress Leave Authority (all): {0}{1}", GameManager.instance.dataScript.StatisticGet(StatType.StressLeaveAuthority), "\n");
        builder.AppendFormat(" Stress Leave Resistance (all): {0}{1}{2}", GameManager.instance.dataScript.StatisticGet(StatType.StressLeaveResistance), "\n", "\n");
        return builder.ToString();
    }


    //new methods above here
}
