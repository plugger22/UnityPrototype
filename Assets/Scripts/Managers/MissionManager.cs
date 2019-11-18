using gameAPI;
using packageAPI;
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
            { endNode = GameManager.instance.dataScript.GetRandomNodeExclude(startNode); }
            else
            {
                //assign end node
                endNode = GetVipNode(mission.vip.nodeEnd, startNode);
                //catch all
                if (endNode == null)
                { endNode = GameManager.instance.dataScript.GetRandomNodeExclude(startNode); }
            }
            //assign
            mission.vip.currentStartNode = startNode;
            mission.vip.currentEndNode = endNode;
        }
    }

    /// <summary>
    /// Gets a start or end VipNode Node. Returns null if a problem. If random then will use source node as starting point for separation calc, if source node null then will choose any random node
    /// sourceNode is only used for randomClose/Med/Long calc's, ignored for all other
    /// </summary>
    /// <param name="vipNode"></param>
    /// <returns></returns>
    private Node GetVipNode(VipNode vipNode, Node sourceNode = null)
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
            case "RandomClose":
            case "RandomMedium":
            case "RandomLong":
                if (sourceNode == null)
                { sourceNode = GameManager.instance.dataScript.GetRandomNode(); }
                else
                {
                    int distance = 0;
                    switch (vipNode.name)
                    {
                        case "RandomClose": distance = 2; break;
                        case "RandomMedium": distance = 4; break;
                        case "RandomLong": distance = 6; break;
                    }
                    //valid source node, get path data
                    node = GameManager.instance.dijkstraScript.GetRandomNodeAtDistance(sourceNode, distance);
                    if (node == null) { Debug.LogWarningFormat("Invalid Random node (Null) for SourceNodeID {0}, distance {1}", sourceNode.nodeID, distance); }
                }
                break;
            default:
                Debug.LogWarningFormat("Unrecognised VipNode \"{0}\"", vipNode.name);
                break;
        }
        return node;
    }



}
