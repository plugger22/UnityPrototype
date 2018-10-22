using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Handles mission and level set-up
/// </summary>
public class MissionManager : MonoBehaviour
{

    public Mission mission;

    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        Debug.Assert(mission != null, "Invalid mission (Null)");
        //Set up mission
        AssignCityTargets();
    }

    /// <summary>
    /// Assign city targets to appropriate nodes
    /// </summary>
    private void AssignCityTargets()
    {
        int nodeID;
        Target target;
        //icon
        if (mission.iconTarget != null)
        {
            target = mission.iconTarget;
            nodeID = GameManager.instance.cityScript.iconDistrictID;
            Node node = GameManager.instance.dataScript.GetNode(nodeID);
            if (node != null)
            {
                node.targetID = target.targetID;
                //set status (debug)
                target.targetStatus = Status.Live;
                Debug.LogFormat("[Tar] MissionManager.cs -> AssignCityTarget: Icon node \"{0}\", {1}, id {2}, assigned target \"{3}\", id {4}", node.nodeName, node.Arc.name, nodeID, 
                    target.name, target.targetID);
            }
            else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0} for iconTarget", nodeID); }
        }
    }
}
