﻿using gameAPI;
using modalAPI;
using packageAPI;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles mission and level set-up
/// </summary>
public class MissionManager : MonoBehaviour
{

    [Header("VIP Globals")]
    [Tooltip("Desired node separation for RandomClose VipNode option")]
    [Range(0, 10)] public int randomClose = 2;
    [Tooltip("Desired node separation for RandomMedium VipNode option")]
    [Range(0, 10)] public int randomMedium = 4;
    [Tooltip("Desired node separation for RandomLong VipNode option")]
    [Range(0, 10)] public int randomLong = 6;

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
        if (GameManager.instance.campaignScript.campaign.side.level == 2)
        { InitialiseNpc(); }
        //register listener
        EventManager.instance.AddListener(EventType.StartTurnLate, OnEvent, "MissionManager");
    }
    #endregion

    #endregion


    /// <summary>
    /// event handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //detect event type
        switch (eventType)
        {
            case EventType.StartTurnLate:
                if (mission.npc != null)
                {
                    ProcessNpc(mission.npc);
                    if (mission.npc.status == NpcStatus.Active)
                    { ProcessContactInteraction(mission.npc); }
                }
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// Initialise Npc (if present)
    /// </summary>
    private void InitialiseNpc()
    {
        if (mission.npc != null)
        {
            Node startNode = null;
            Node endNode = null;
            //if either start or end NpcNodes are null assign random values
            if (mission.npc.nodeStart == null)
            {
                startNode = GameManager.instance.dataScript.GetRandomNode();
                if (startNode == null) { Debug.LogError("Invalid random start node (Null)"); }
            }
            else
            {
                //assign start node
                startNode = GetNpcNode(mission.npc.nodeStart);
                //catch all
                if (startNode == null)
                { startNode = GameManager.instance.dataScript.GetRandomNode(); }
            }
            //End node
            endNode = GetEndNode(startNode);
            //assign
            mission.npc.currentStartNode = startNode;
            mission.npc.currentEndNode = endNode;
            //status
            mission.npc.status = NpcStatus.Standby;
        }
    }

    /// <summary>
    /// subMethod to get an end node given a start node. Returns null if a problem
    /// NOTE: start node checked for null by parent method
    /// </summary>
    /// <param name="startNode"></param>
    /// <returns></returns>
    private Node GetEndNode(Node startNode)
    {
        Node endNode = null;
        if (mission.npc.nodeEnd == null)
        { endNode = GameManager.instance.dataScript.GetRandomNodeExclude(startNode); }
        else
        {
            //assign end node
            endNode = GetNpcNode(mission.npc.nodeEnd, startNode);
            //catch all
            if (endNode == null)
            { endNode = GameManager.instance.dataScript.GetRandomNodeExclude(startNode); }
        }
        //fail safe
        if (endNode.nodeID == startNode.nodeID)
        {
            Debug.LogWarningFormat("Invalid Matching Npc nodes (start ID {0}, end ID {1}) for GetEndNode", endNode.nodeID, startNode.nodeID);
            endNode = GameManager.instance.dataScript.GetRandomNodeExclude(startNode);
        }
        return endNode;
    }

    /// <summary>
    /// Gets a start or end NpcNode Node. Returns null if a problem. If random then will use source node as starting point for separation calc, if source node null then will choose any random node
    /// sourceNode is only used for randomClose/Med/Long calc's, ignored for all other
    /// </summary>
    /// <param name="npcNode"></param>
    /// <returns></returns>
    private Node GetNpcNode(NpcNode npcNode, Node sourceNode = null)
    {
        Node node = null;
        int nodeID = -1;
        switch (npcNode.name)
        {
            case "Airport":
                nodeID = GameManager.instance.cityScript.airportDistrictID;
                node = GameManager.instance.dataScript.GetNode(nodeID);
                if (node == null) { Debug.LogWarningFormat("Invalid Airport node (Null) for nodeID {0}", nodeID); }
                break;
            case "Harbour":
                nodeID = GameManager.instance.cityScript.harbourDistrictID;
                if (nodeID > -1)
                { node = GameManager.instance.dataScript.GetNode(nodeID); }
                else
                {
                    Debug.LogWarning("No Harbour district present (nodeID -1)");
                    if (sourceNode != null)
                    { node = GameManager.instance.dataScript.GetRandomNodeExclude(sourceNode); }
                    else { node = GameManager.instance.dataScript.GetRandomNode(); }
                }
                if (node == null) { Debug.LogWarningFormat("Invalid Harbour node (Null) for nodeID {0}", nodeID); }
                break;
            case "CityHall":
                nodeID = GameManager.instance.cityScript.cityHallDistrictID;
                node = GameManager.instance.dataScript.GetNode(nodeID);
                if (node == null) { Debug.LogWarningFormat("Invalid City Hall node (Null) for nodeID {0}", nodeID); }
                break;
            case "Icon":
                nodeID = GameManager.instance.cityScript.iconDistrictID;
                if (nodeID > -1)
                { node = GameManager.instance.dataScript.GetNode(nodeID); }
                else
                {
                    Debug.LogWarning("No Icon district present (nodeID -1)");
                    if (sourceNode != null)
                    { node = GameManager.instance.dataScript.GetRandomNodeExclude(sourceNode); }
                    else { node = GameManager.instance.dataScript.GetRandomNode(); }
                }
                if (node == null) { Debug.LogWarningFormat("Invalid Icon node (Null) for nodeID {0}", nodeID); }
                break;
            case "arcCORPORATE":
            case "arcGATED":
            case "arcGOVERNMENT":
            case "arcINDUSTRIAL":
            case "arcRESEARCH":
            case "arcSPRAWL":
            case "arcUTILITY":
                NodeArc arc = npcNode.nodeArc;
                if (arc != null)
                {
                    node = GameManager.instance.dataScript.GetRandomNode(arc.nodeArcID);
                    if (node == null) { Debug.LogWarningFormat("Invalid node (Null) for \"{0}\"", npcNode.name); }
                }
                else { Debug.LogWarningFormat("Invalid nodeArc (Null) for \"{0}\"", npcNode.name); }
                break;
            case "RandomClose":
            case "RandomMedium":
            case "RandomLong":
                if (sourceNode == null)
                { sourceNode = GameManager.instance.dataScript.GetRandomNode(); }
                else
                {
                    int distance = 0;
                    switch (npcNode.name)
                    {
                        case "RandomClose": distance = randomClose; break;
                        case "RandomMedium": distance = randomMedium; break;
                        case "RandomLong": distance = randomLong; break;
                    }
                    //valid source node, get path data
                    node = GameManager.instance.dijkstraScript.GetRandomNodeAtDistance(sourceNode, distance);
                    if (node == null) { Debug.LogWarningFormat("Invalid Random node (Null) for SourceNodeID {0}, distance {1}", sourceNode.nodeID, distance); }
                }
                break;
            default:
                Debug.LogWarningFormat("Unrecognised NpcNode \"{0}\"", npcNode.name);
                break;
        }
        return node;
    }

    /// <summary>
    /// master method to handle all Npc matters. 
    /// NOTE: parent method checks for presence of Npc
    /// </summary>
    private void ProcessNpc(Npc npc)
    {
        switch (mission.npc.status)
        {
            case NpcStatus.Standby:
                CheckNpcActive(npc);
                break;
            case NpcStatus.Active:
                UpdateActiveNpc(npc);
                break;
            case NpcStatus.Departed:
                break;
            default:
                Debug.LogWarningFormat("Unrecognised Npc status \"{0}\"", mission.npc.status);
                break;
        }
    }

    /// <summary>
    /// checks for Npc activating and being placed onMap
    /// </summary>
    private void CheckNpcActive(Npc npc)
    {
        //check start turn
        if (GameManager.instance.turnScript.Turn >= npc.startTurn)
        {
            int rnd = Random.Range(0, 100);
            if (rnd < npc.startChance)
            {
                //Start
                if (npc.currentStartNode != null)
                {
                    npc.status = NpcStatus.Active;
                    npc.currentNode = npc.currentStartNode;
                    npc.timerTurns = npc.maxTurns;
                    GameManager.instance.nodeScript.nodeNpc = npc.currentStartNode.nodeID;
                    Debug.LogFormat("[Npc] MissionManager.cs -> CheckNpcActive: Npc \"{0}\" OnMap (rnd {1}, needed < {2}) at {3}, {4}, ID {5}, timer {6}{7}", npc.tag, rnd, npc.startChance,
                        npc.currentNode.nodeName, npc.currentNode.Arc.name, npc.currentNode.nodeID, npc.timerTurns, "\n");
                    //tracker
                    AddTrackerRecord(npc);
                }
                else { Debug.LogWarning("Invalid Npc currentStartNode (Null)"); }
            }
            else { Debug.LogFormat("[Npc] MissionManager.cs -> CheckNpcActive: Npc \"{0}\" failed Activation roll (rnd {1}, needed < {2}){3}", npc.tag, rnd, npc.startChance, "\n"); }
        }
    }

    /// <summary>
    /// Updates Active onMap Npc
    /// </summary>
    /// <param name="npc"></param>
    private void UpdateActiveNpc(Npc npc)
    {
        //check if in same district as Player
        if (npc.currentNode.nodeID == GameManager.instance.nodeScript.nodePlayer)
        {
            //Player Interacts with Npc
            if (ProcessNpcInteract(npc) == true)
            { return; }
        }
        else
        {
            //Proceed with Npc activity -> decrement timer
            if (npc.timerTurns > 0) { npc.timerTurns--; }
            //check if moves
            int rnd = Random.Range(0, 100);
            if (rnd < npc.moveChance)
            {
                Debug.LogFormat("[Npc] MissionManager.cs -> UpdateActiveNpc: Npc \"{0}\" MOVED (rnd {1}, needed < {2}), timer {3}{4}", npc.tag, rnd, npc.moveChance, npc.timerTurns, "\n");
                //Npc at destination
                if (npc.currentEndNode.nodeID == npc.currentNode.nodeID)
                {
                    //no repeat
                    if (npc.isRepeat == false)
                    { ProcessNpcDepart(npc); }
                    //repeat but timer has expired
                    else
                    {
                        if (npc.timerTurns <= 0)
                        { ProcessNpcDepart(npc); }
                        else
                        {
                            //still going, create a new path (uses a move action to do so)
                            npc.currentStartNode = npc.currentNode;
                            npc.currentEndNode = GetEndNode(npc.currentNode);
                            if (npc.currentEndNode != null)
                            {
                                Debug.LogFormat("[Npc] MissionManager.cs -> UpdateActiveNpc: New path from {0}, {1}, ID {2} to {3}, {4}, ID {5}{6}", npc.currentStartNode.nodeName, npc.currentStartNode.Arc.name,
                                    npc.currentStartNode.nodeID, npc.currentEndNode.nodeName, npc.currentEndNode.Arc.name, npc.currentEndNode.nodeID, "\n");
                            }
                            else
                            {
                                Debug.LogWarning("Invalid currentEndNode (Null) for Npc when calculating new path");
                                ProcessNpcDepart(npc);
                            }
                        }
                    }
                }
                else
                {
                    //Npc moves towards destination -> Get Path
                    List<Connection> listOfConnections = GameManager.instance.dijkstraScript.GetPath(npc.currentNode.nodeID, npc.currentEndNode.nodeID, false);
                    if (listOfConnections != null)
                    {
                        int numOfLinks = listOfConnections.Count;
                        int nextNodeID;
                        if (numOfLinks > 0)
                        {
                            //move nemesis multiple links if allowed, stop moving immediately if nemesis spots Player at same node
                            Connection connection = listOfConnections[0];
                            if (connection != null)
                            {
                                //get the node to move to for this link
                                nextNodeID = connection.GetNode1();
                                if (nextNodeID == npc.currentNode.nodeID)
                                { nextNodeID = connection.GetNode2(); }
                                //move forward one link
                                Node node = GameManager.instance.dataScript.GetNode(nextNodeID);
                                if (node != null)
                                {
                                    npc.currentNode = node;
                                    GameManager.instance.nodeScript.nodeNpc = nextNodeID;
                                    Debug.LogFormat("[Npc] MissionManager.cs -> ProcessNpcDepart: Npc \"{0}\" moved to {1}, {2}, nodeID {3}, timer {4}{5}",
                                        npc.tag, node.nodeName, node.Arc.name, node.nodeID, npc.timerTurns, "\n");
                                }
                                else { Debug.LogWarningFormat("Invalid move node (Null) for nextNodeID {0}", nextNodeID); }
                            }
                            else { Debug.LogWarning("Invalid connection (Null) in listOfConnections[0]"); }
                        }
                        else { Debug.LogWarningFormat("Invalid listOfConnections (Empty) between Npc currentNodeID {0} and currentEndNodeID {1}", npc.currentNode.nodeID, npc.currentEndNode.nodeID); }
                    }
                    else { Debug.LogWarning("Invalid listOfConnections (Null)"); }
                }
            }
            else { Debug.LogFormat("[Npc] MissionManager.cs -> CheckNpcActive: Npc \"{0}\" didn't move (rnd {1}, needed < {2}), timer {3}{4}", npc.tag, rnd, npc.moveChance, npc.timerTurns, "\n"); }
            //tracker
            AddTrackerRecord(npc);
        }
    }

    /// <summary>
    /// handles all admin for Npc departing map (reached destination and no repeat or repeat but timer has run out)
    /// </summary>
    private void ProcessNpcDepart(Npc npc)
    {
        npc.status = NpcStatus.Departed;
        npc.currentNode = null;
        GameManager.instance.nodeScript.nodeNpc = -1;
        Debug.LogFormat("[Npc] MissionManager.cs -> ProcessNpcDepart: Npc \"{0}\" Departed{1}", npc.tag, "\n");
    }

    /// <summary>
    /// Active Player at same node as Npc ->  interacts
    /// </summary>
    /// <param name="npc"></param>
    private bool ProcessNpcInteract(Npc npc)
    {
        bool isSuccess = false;
        //Player interacts with Npc
        if (GameManager.instance.playerScript.status == ActorStatus.Active)
        {
            //pipeline msg
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
            {
                textTop = GameManager.instance.colourScript.GetFormattedString(string.Format("You {0} the {1}", npc.action.tag, npc.tag), ColourType.moccasinText),
                textBottom = string.Format("The {0} {1}", npc.tag, npc.action.outcome),
                sprite = npc.sprite,
                isAction = false,
                side = GameManager.instance.globalScript.sideResistance,
                type = MsgPipelineType.Npc
            };
            if (GameManager.instance.guiScript.InfoPipelineAdd(outcomeDetails) == false)
            { Debug.LogWarningFormat("Npc interacts with Player InfoPipeline message FAILED to be added to dictOfPipeline"); }
            //messages (need to be BEFORE depart)
            Debug.LogFormat("[Npc] MissionManager.cs -> UpdateActiveNpc: Player INTERACTS with Npc \"{0}\" at {1}, {2}, ID {3}{4}", npc.tag, npc.currentNode.nodeName, npc.currentNode.Arc.name,
                                npc.currentNode.nodeID, "\n");
            //Npc departs map
            ProcessNpcDepart(npc);
            isSuccess = true;
        }
        return isSuccess;
    }


    /// <summary>
    /// Add tracker data every turn
    /// </summary>
    /// <param name="npc"></param>
    private void AddTrackerRecord(Npc npc)
    {
        HistoryNpcMove data = new HistoryNpcMove()
        {
            turn = GameManager.instance.turnScript.Turn,
            currentNodeID = npc.currentNode.nodeID,
            endNodeID = npc.currentEndNode.nodeID,
            timer = npc.timerTurns,
        };
        GameManager.instance.dataScript.AddHistoryNpcMove(data);
    }

    /// <summary>
    /// Checks to see if Npc spotted by contact
    /// </summary>
    private void ProcessContactInteraction(Npc npc)
    {
        Actor actor;
        Contact contact;
        List<int> listOfActorsWithContactsAtNode = GameManager.instance.dataScript.CheckContactResistanceAtNode(npc.currentNode.nodeID);
        if (listOfActorsWithContactsAtNode != null)
        {
            int numOfActors = listOfActorsWithContactsAtNode.Count;
            if (numOfActors > 0)
            {
                //Npc stealthRating
                int stealthRating = npc.stealthRating;
                //loop actors with contacts
                for (int i = 0; i < numOfActors; i++)
                {
                    actor = GameManager.instance.dataScript.GetActor(listOfActorsWithContactsAtNode[i]);
                    if (actor != null)
                    {
                        //only active actors can work their contact network
                        if (actor.Status == ActorStatus.Active)
                        {
                            contact = actor.GetContact(npc.currentNode.nodeID);
                            if (contact != null)
                            {
                                //contact active
                                if (contact.status == ContactStatus.Active)
                                {
                                    //check Npc stealth rating vs. contact effectiveness
                                    if (contact.effectiveness >= stealthRating)
                                    {
                                        Node node = npc.currentNode;
                                        //contact spots Npc
                                        string text = string.Format("Npc {0} has been spotted by Contact {1} {2}, {3}, at node {4}, id {5}", npc.tag, contact.nameFirst, contact.nameLast,
                                            contact.job, node.nodeName, node.nodeID);
                                        Debug.LogFormat("[Cnt] MissionManager.cs -> ProcessContactInteraction: Contact {0}, effectiveness {1}, SPOTS Npc {2}, StealthRating {3} at node {4}, id {5}{6}",
                                            contact.nameFirst, contact.effectiveness, npc.tag, stealthRating, node.nodeName, node.nodeID, "\n");
                                        GameManager.instance.messageScript.ContactNpcSpotted(text, actor, node, contact, npc);
                                        //contact stats
                                        contact.statsNpc++;
                                        //no need to check anymore as one sighting is enough
                                        break;
                                    }
                                    else
                                    {
                                        //contact Fails to spot Npc
                                        Debug.LogFormat("[Cnt] MissionManager.cs -> ProcessContactInteraction: Contact {0}, effectiveness {1}, FAILS to spot Npc {2}, adj StealthRating {3} at nodeID {4}{5}",
                                            contact.nameFirst, contact.effectiveness, npc.tag, stealthRating, npc.currentNode.nodeID, "\n");
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogFormat("Invalid contact (Null) for actor {0}, id {1} at node {2}, {3}, id {4}", actor.actorName, actor.actorID, npc.currentNode.nodeName,
                                      npc.currentNode.Arc.name, npc.currentNode.nodeID);
                            }
                        }
                        else
                        {
                            Debug.LogFormat("[Cnt] MissionManager.cs -> ProcessContactInteraction: Actor {0}, {1}, id {2}, is INACTIVE and can't access their contacts{3}", actor.actorName,
                                 actor.arc.name, actor.actorID, "\n");
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", listOfActorsWithContactsAtNode[i]); }
                }
            }
            else { Debug.LogWarning("Invalid listOfActorsWithContactsAtNode (Empty)"); }
        }
        /*else { Debug.LogWarning("Invalid listOfActorsWithContactsAtNode (Null)"); }  Edit -> if no contacts at node this will trigger. No need for warning */
    }

    //new methods above here
}
