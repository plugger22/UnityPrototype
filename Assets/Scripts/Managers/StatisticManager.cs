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
        GameManager.instance.dataScript.StatisticAddNew(StatType.PlayerCaptured);
        //actors
        GameManager.instance.dataScript.StatisticAddNew(StatType.actorsResignedAuthority);
        GameManager.instance.dataScript.StatisticAddNew(StatType.actorsResignedResistance);
        //stress leave
        GameManager.instance.dataScript.StatisticAddNew(StatType.StressLeaveAuthority);
        GameManager.instance.dataScript.StatisticAddNew(StatType.StressLeaveResistance);
        //targets
        GameManager.instance.dataScript.StatisticAddNew(StatType.TargetAttempts);
        GameManager.instance.dataScript.StatisticAddNew(StatType.TargetSuccesses);

    }


    /// <summary>
    /// Display statistics
    /// </summary>
    /// <returns></returns>
    public string DebugShowStatistics()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("-Statistics{0}{1}", "\n", "\n");
        //player
        builder.AppendFormat(" Player Breakdowns: {0}{1}", GameManager.instance.dataScript.StatisticGet(StatType.PlayerBreakdown), "\n");
        builder.AppendFormat(" Player Captured: {0}{1}", GameManager.instance.dataScript.StatisticGet(StatType.PlayerCaptured), "\n");
        builder.AppendFormat(" Player Lie Low: {0}{1}{2}", GameManager.instance.dataScript.StatisticGet(StatType.PlayerLieLow), "\n", "\n");
        //actors
        builder.AppendFormat(" Actors Resigned (Authority): {0}{1}", GameManager.instance.dataScript.StatisticGet(StatType.actorsResignedAuthority), "\n");
        builder.AppendFormat(" Actors Resigned (Resistance): {0}{1}", GameManager.instance.dataScript.StatisticGet(StatType.actorsResignedResistance), "\n");
        //stress leave
        builder.AppendFormat(" Stress Leave Authority (all): {0}{1}", GameManager.instance.dataScript.StatisticGet(StatType.StressLeaveAuthority), "\n");
        builder.AppendFormat(" Stress Leave Resistance (all): {0}{1}{2}", GameManager.instance.dataScript.StatisticGet(StatType.StressLeaveResistance), "\n", "\n");
        //targets
        builder.AppendFormat(" Target Attempts (Resistance): {0}{1}", GameManager.instance.dataScript.StatisticGet(StatType.TargetAttempts), "\n");
        builder.AppendFormat(" Target Successes (Resistance): {0}{1}", GameManager.instance.dataScript.StatisticGet(StatType.TargetSuccesses), "\n", "\n");
        return builder.ToString();
        
    }


    //new methods above here
}
