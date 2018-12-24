using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using System;
using System.Linq;
using System.Text;

/// <summary>
/// handles all Resistance AI
/// </summary>
public class AIRebelManager : MonoBehaviour
{

    //AI Player
    private ActorStatus status;
    private int aiPlayerInvisibility;                   //invisibility of AI player
    private int aiPlayerRenown;                         //current renown tally
    private int actionAllowance;                        //number of actions per turn
    private int actionsExtra;                           //bonus actions for this turn
    private int actionsUsed;                            //tally of actions used this turn

    private int targetNodeID;                           //goal to move towards

    private Dictionary<Target, int> dictOfSortedTargets = new Dictionary<Target, int>();



    /// <summary>
    /// main controlling method to run Resistance AI each turn, called from AIManager.cs -> ProcessAISideResistance
    /// </summary>
    public void ProcessAI()
    {
        ClearAICollections();
        //update node data
        ProcessNodeData();
        //targets
        ProcessTargetData();
    }

    /// <summary>
    /// reset all data prior to AI turn processing
    /// </summary>
    private void ClearAICollections()
    {
        dictOfSortedTargets.Clear();
    }

    /// <summary>
    /// update node data for known erasure teams and nemesis locations prior to doing any Dijkstra pathing
    /// </summary>
    private void ProcessNodeData()
    {

    }

    /// <summary>
    /// Select a target nodeID as a goal to move towards
    /// </summary>
    private void ProcessTargetData()
    {
        List<Target> listOfTargets = GameManager.instance.dataScript.GetTargetPool(Status.Live);
        if (listOfTargets != null)
        {
            //temp dict with key -> Target, value -> distance (weighted)
            Dictionary<Target, int> dictOfTargets = new Dictionary<Target, int>();
            int distance;
            int count = listOfTargets.Count;
            if (count > 0)
            {
                int playerNodeID = GameManager.instance.nodeScript.nodePlayer;
                //loop targets and get weighted distance to each
                for (int i = 0; i < count; i++)
                {
                    Target target = listOfTargets[i];
                    if (target != null)
                    {
                        distance = GameManager.instance.dijkstraScript.GetDistanceWeighted(playerNodeID, target.nodeID);
                        if (distance > -1)
                        {
                            //add entry to dictionary
                            try
                            { dictOfTargets.Add(target, distance); }
                            catch (ArgumentException)
                            { Debug.LogErrorFormat("Duplicate target entry for target {0}, id {1}", target.name, target.targetID); }
                        }
                        else { Debug.LogWarningFormat("Invalid dijkstra weighted distance (-1) between id {0} and id {1}", playerNodeID, listOfTargets[i].nodeID); }
                    }
                    else { Debug.LogWarningFormat("Invalid target (Null) for listOfTargets[{0}]", i); }
                }
                //sort dict if it has > 1 entry
                if (dictOfTargets.Count > 1)
                {
                    var sortedTargets = from pair in dictOfTargets
                                        orderby pair.Value ascending
                                        select pair;
                    //populate sorted targets dict
                    foreach (var target in sortedTargets)
                    { dictOfSortedTargets.Add(target.Key, target.Value); }
                }
            }
        }
        else { Debug.LogError("Invalid listOfTargets (Null)"); }
    }

    //
    // - - -  Debug - - -
    //

    /// <summary>
    /// Show Resistance AI status
    /// </summary>
    /// <returns></returns>
    public string DebugShowRebelAIStatus()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat(" Resistance AI Status{0}{1}", "\n", "\n");
        builder.AppendFormat(" -ProcessTargetData (weighted distances){0}", "\n");
        int count = dictOfSortedTargets.Count;
        if (count > 0)
        {
            foreach(var target in dictOfSortedTargets)
            { builder.AppendFormat("Target: {0}, at node id {1}, distance {2}{3}", target.Key.name, target.Key.nodeID, target.Value, "\n"); }
        }
        else { builder.Append(" No records present"); }
        return builder.ToString();
    }

    //new methods above here
}
