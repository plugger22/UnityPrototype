using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// handles all Resistance AI
/// </summary>
public class AIRebelManager : MonoBehaviour
{

    private int targetNodeID;                           //goal to move towards

    /// <summary>
    /// main controlling method to run Resistance AI each turn, called from AIManager.cs -> ProcessAISideResistance
    /// </summary>
    public void ProcessAI()
    {

    }

    /// <summary>
    /// Select a target nodeID as a goal to move towards
    /// </summary>
    private void GetTarget()
    {
        List<Target> listOfTargets = GameManager.instance.dataScript.GetTargetPool(Status.Live);
    }
}
