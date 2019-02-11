using gameAPI;
using System.Collections;
using System.Collections.Generic;
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
}
