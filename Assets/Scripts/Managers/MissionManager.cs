﻿using gameAPI;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles mission and level set-up
/// </summary>
public class MissionManager : MonoBehaviour
{
    [HideInInspector] public Mission mission;

    /// <summary>
    /// Initialisation called from CampaignManager.cs -> Initialise
    /// NOTE: Initialises TargetManager.cs
    /// </summary>
    public void Initialise()
    {
        switch (GameManager.instance.inputScript.GameState)
        {
            case GameState.NewInitialisation:
            case GameState.FollowOnInitialisation:
            case GameState.LoadAtStart:
            case GameState.LoadGame:
                SubInitialiseAll();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseAll
    private void SubInitialiseAll()
    {
        Debug.Assert(mission != null, "Invalid Mission (Null)");
        //Assign Objectives if present, if not use random objectives
        List<Objective> listOfObjectives = new List<Objective>();
        if (mission.listOfObjectives.Count > 0)
        { listOfObjectives.AddRange(mission.listOfObjectives); }
        else { listOfObjectives.AddRange(GameManager.instance.dataScript.GetRandomObjectives(GameManager.instance.objectiveScript.maxNumOfObjectives)); }
        GameManager.instance.objectiveScript.SetObjectives(listOfObjectives);
        //initialise and assign targets
        GameManager.instance.targetScript.Initialise();
        GameManager.instance.targetScript.AssignTargets(mission);
        //Human Resistance Player
        if (GameManager.instance.sideScript.resistanceOverall == SideState.Human)
        {
            InitialiseVIP();
        }
    }
    #endregion

    #endregion


    /// <summary>
    /// Initialise VIP (if present)
    /// </summary>
    private void InitialiseVIP()
    {
        if (mission.vip != null)
        {
            Node startNode = null;
            Node endNode = null;
            //if either start or end VipNodes are null assign random values
            if (mission.vip.nodeStart == null)
            {
                startNode = GameManager.instance.dataScript.GetRandomNode();
                if (startNode == null) { Debug.LogError("Invalid random start node (Null)"); }
            }
            else
            {
                //assign start node
                startNode = GetVipNode(mission.vip.nodeStart);
                //catch all
                if (startNode == null)
                { startNode = GameManager.instance.dataScript.GetRandomNode(); }
            }
            //End node
            if (mission.vip.nodeEnd == null)
            { endNode = GetRandomNode(startNode); }
            else
            {
                //assign end node
                endNode = GetVipNode(mission.vip.nodeEnd);
                //catch all
                if (endNode == null)
                { endNode = GetRandomNode(startNode); }
            }
        }
    }

    /// <summary>
    /// Gets a start or end VipNode Node. Returns null if a problem
    /// </summary>
    /// <param name="vipNode"></param>
    /// <returns></returns>
    private Node GetVipNode(VipNode vipNode)
    {
        Node node = null;
        int nodeID = -1;
        switch (vipNode.name)
        {
            case "Airport":
                nodeID = GameManager.instance.cityScript.airportDistrictID;
                node = GameManager.instance.dataScript.GetNode(nodeID);
                if (node == null) { Debug.LogWarningFormat("Invalid Airport node (Null) for nodeID {0}", nodeID); }
                break;
            case "Harbour":
                nodeID = GameManager.instance.cityScript.harbourDistrictID;
                node = GameManager.instance.dataScript.GetNode(nodeID);
                if (node == null) { Debug.LogWarningFormat("Invalid Harbour node (Null) for nodeID {0}", nodeID); }
                break;
            case "City Hall":
                nodeID = GameManager.instance.cityScript.cityHallDistrictID;
                node = GameManager.instance.dataScript.GetNode(nodeID);
                if (node == null) { Debug.LogWarningFormat("Invalid City Hall node (Null) for nodeID {0}", nodeID); }
                break;
            case "Icon":
                nodeID = GameManager.instance.cityScript.iconDistrictID;
                node = GameManager.instance.dataScript.GetNode(nodeID);
                if (node == null) { Debug.LogWarningFormat("Invalid Icon node (Null) for nodeID {0}", nodeID); }
                break;
            case "arcCORPORATE":
            case "arcGATED":
            case "arcGOVERNMENT":
            case "arcINDUSTRIAL":
            case "arcRESEARCH":
            case "arcSPRAWL":
            case "arcUTILITY":
                NodeArc arc = vipNode.nodeArc;
                if (arc != null)
                {
                    node = GameManager.instance.dataScript.GetRandomNode(arc.nodeArcID);
                    if (node == null) { Debug.LogWarningFormat("Invalid node (Null) for \"{0}\"", vipNode.name); }
                }
                else { Debug.LogWarningFormat("Invalid nodeArc (Null) for \"{0}\"", vipNode.name); }
                break;
            default:
                Debug.LogWarningFormat("Unrecognised VipNode \"{0}\"", vipNode.name);
                break;
        }

        return node;
    }

    /// <summary>
    /// Gets a random node that isn't 'notThisNode'. Returns null
    /// </summary>
    /// <param name="notThisNode"></param>
    /// <returns></returns>
    private Node GetRandomNode(Node notThisNode)
    {
        Node node = null;
        if (notThisNode != null)
        {
            //get a random end node, any will do provided it is different to the start node
            int counter = 0;
            do
            {
                node = GameManager.instance.dataScript.GetRandomNode();
                if (node == null) { Debug.LogError("Invalid random end node (Null)"); }
                counter++;
                if (counter > 10)
                {
                    Debug.LogWarningFormat("Counter has timed out (now {0})", counter);
                    break;
                }
            }
            while (node.nodeID == notThisNode.nodeID);
        }
        else { Debug.LogError("Invalid notThisNode (Null)"); }
        return node;
    }


}
