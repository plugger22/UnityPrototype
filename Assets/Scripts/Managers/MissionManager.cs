using gameAPI;
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
        { InitialiseVIP(); }
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
                if (mission.vip != null)
                {
                    ProcessVIP(mission.vip);
                    if (mission.vip.status == VipStatus.Active)
                    { ProcessContactInteraction(mission.vip); }
                }
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


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
            endNode = GetEndNode(startNode);
            //assign
            mission.vip.currentStartNode = startNode;
            mission.vip.currentEndNode = endNode;
            //status
            mission.vip.status = VipStatus.Standby;
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
        //fail safe
        if (endNode.nodeID == startNode.nodeID)
        {
            Debug.LogWarningFormat("Invalid Matching VIPrepe nodes (start ID {0}, end ID {1}) for GetEndNode", endNode.nodeID, startNode.nodeID);
            endNode = GameManager.instance.dataScript.GetRandomNodeExclude(startNode);
        }
        return endNode;
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
                Debug.LogWarningFormat("Unrecognised VipNode \"{0}\"", vipNode.name);
                break;
        }
        return node;
    }

    /// <summary>
    /// master method to handle all VIP matters. 
    /// NOTE: parent method checks for presence of VIP
    /// </summary>
    private void ProcessVIP(Vip vip)
    {
        switch (mission.vip.status)
        {
            case VipStatus.Standby:
                CheckVipActive(vip);
                break;
            case VipStatus.Active:
                UpdateActiveVip(vip);
                break;
            case VipStatus.Departed:
                break;
            default:
                Debug.LogWarningFormat("Unrecognised VIP status \"{0}\"", mission.vip.status);
                break;
        }
    }

    /// <summary>
    /// checks for VIP activating and being placed onMap
    /// </summary>
    private void CheckVipActive(Vip vip)
    {
        //check start turn
        if (GameManager.instance.turnScript.Turn >= vip.startTurn)
        {
            int rnd = Random.Range(0, 100);
            if (rnd < vip.startChance)
            {
                //Start
                if (vip.currentStartNode != null)
                {
                    vip.status = VipStatus.Active;
                    vip.currentNode = vip.currentStartNode;
                    vip.timerTurns = vip.maxTurns;
                    GameManager.instance.nodeScript.nodeVip = vip.currentStartNode.nodeID;
                    Debug.LogFormat("[Vip] MissionManager.cs -> CheckVipActive: VIP \"{0}\" OnMap (rnd {1}, needed < {2}) at {3}, {4}, ID {5}, timer {6}{7}", vip.tag, rnd, vip.startChance,
                        vip.currentNode.nodeName, vip.currentNode.Arc.name, vip.currentNode.nodeID, vip.timerTurns, "\n");
                    //tracker
                    AddTrackerRecord(vip);
                }
                else { Debug.LogWarning("Invalid VIP currentStartNode (Null)"); }
            }
            else { Debug.LogFormat("[Vip] MissionManager.cs -> CheckVipActive: VIP \"{0}\" failed Activation roll (rnd {1}, needed < {2}){3}", vip.tag, rnd, vip.startChance, "\n"); }
        }
    }

    /// <summary>
    /// Updates Active onMap V.I.P
    /// </summary>
    /// <param name="vip"></param>
    private void UpdateActiveVip(Vip vip)
    {
        //check if in same district as Player
        if (vip.currentNode.nodeID == GameManager.instance.nodeScript.nodePlayer)
        {
            //Player Interacts with V.I.P
            if (ProcessVipInteract(vip) == true)
            { return; }
        }
        else
        {
            //Proceed with V.I.P activity -> decrement timer
            if (vip.timerTurns > 0) { vip.timerTurns--; }
            //check if moves
            int rnd = Random.Range(0, 100);
            if (rnd < vip.moveChance)
            {
                Debug.LogFormat("[Vip] MissionManager.cs -> UpdateActiveVip: VIP \"{0}\" MOVED (rnd {1}, needed < {2}), timer {3}{4}", vip.tag, rnd, vip.moveChance, vip.timerTurns, "\n");
                //VIP at destination
                if (vip.currentEndNode.nodeID == vip.currentNode.nodeID)
                {
                    //no repeat
                    if (vip.isRepeat == false)
                    { ProcessVipDepart(vip); }
                    //repeat but timer has expired
                    else
                    {
                        if (vip.timerTurns <= 0)
                        { ProcessVipDepart(vip); }
                        else
                        {
                            //still going, create a new path (uses a move action to do so)
                            vip.currentStartNode = vip.currentNode;
                            vip.currentEndNode = GetEndNode(vip.currentNode);
                            if (vip.currentEndNode != null)
                            {
                                Debug.LogFormat("[Vip] MissionManager.cs -> UpdateActiveVip: New path from {0}, {1}, ID {2} to {3}, {4}, ID {5}{6}", vip.currentStartNode.nodeName, vip.currentStartNode.Arc.name,
                                    vip.currentStartNode.nodeID, vip.currentEndNode.nodeName, vip.currentEndNode.Arc.name, vip.currentEndNode.nodeID, "\n");
                            }
                            else
                            {
                                Debug.LogWarning("Invalid currentEndNode (Null) for VIP when calculating new path");
                                ProcessVipDepart(vip);
                            }
                        }
                    }
                }
                else
                {
                    //Vip moves towards destination -> Get Path
                    List<Connection> listOfConnections = GameManager.instance.dijkstraScript.GetPath(vip.currentNode.nodeID, vip.currentEndNode.nodeID, false);
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
                                if (nextNodeID == vip.currentNode.nodeID)
                                { nextNodeID = connection.GetNode2(); }
                                //move forward one link
                                Node node = GameManager.instance.dataScript.GetNode(nextNodeID);
                                if (node != null)
                                {
                                    vip.currentNode = node;
                                    vip.isKnown = false;
                                    GameManager.instance.nodeScript.nodeVip = nextNodeID;
                                    Debug.LogFormat("[Vip] MissionManager.cs -> ProcessVipDepart: VIP \"{0}\" moved to {1}, {2}, nodeID {3}, timer {4}{5}",
                                        vip.tag, node.nodeName, node.Arc.name, node.nodeID, vip.timerTurns, "\n");
                                }
                                else { Debug.LogWarningFormat("Invalid move node (Null) for nextNodeID {0}", nextNodeID); }
                            }
                            else { Debug.LogWarning("Invalid connection (Null) in listOfConnections[0]"); }
                        }
                        else { Debug.LogWarningFormat("Invalid listOfConnections (Empty) between VIP currentNodeID {0} and currentEndNodeID {1}", vip.currentNode.nodeID, vip.currentEndNode.nodeID); }
                    }
                    else { Debug.LogWarning("Invalid listOfConnections (Null)"); }
                }
            }
            else { Debug.LogFormat("[Vip] MissionManager.cs -> CheckVipActive: VIP \"{0}\" didn't move (rnd {1}, needed < {2}), timer {3}{4}", vip.tag, rnd, vip.moveChance, vip.timerTurns, "\n"); }
            //tracker
            AddTrackerRecord(vip);
        }
    }

    /// <summary>
    /// handles all admin for VIP departing map (reached destination and no repeat or repeat but timer has run out)
    /// </summary>
    private void ProcessVipDepart(Vip vip)
    {
        vip.status = VipStatus.Departed;
        vip.currentNode = null;
        GameManager.instance.nodeScript.nodeVip = -1;
        Debug.LogFormat("[Vip] MissionManager.cs -> ProcessVipDepart: VIP \"{0}\" Departed{1}", vip.tag, "\n");
    }

    /// <summary>
    /// Active Player at same node as V.I.P ->  interacts
    /// </summary>
    /// <param name="vip"></param>
    private bool ProcessVipInteract(Vip vip)
    {
        bool isSuccess = false;
        //Player interacts with V.I.P
        if (GameManager.instance.playerScript.status == ActorStatus.Active)
        {
            //pipeline msg
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
            {
                textTop = string.Format("You {0} {1}", vip.action.tag, vip.tag),
                textBottom = string.Format("The {0} {1}", vip.tag, vip.action.outcome),
                sprite = vip.sprite,
                isAction = false,
                side = GameManager.instance.globalScript.sideResistance,
                type = MsgPipelineType.VIP
            };
            if (GameManager.instance.guiScript.InfoPipelineAdd(outcomeDetails) == false)
            { Debug.LogWarningFormat("V.I.P interacts with Player InfoPipeline message FAILED to be added to dictOfPipeline"); }
            //messages (need to be BEFORE depart)
            Debug.LogFormat("[Vip] MissionManager.cs -> UpdateActiveVip: Player INTERACTS with V.I.P \"{0}\" at {1}, {2}, ID {3}{4}", vip.tag, vip.currentNode.nodeName, vip.currentNode.Arc.name,
                                vip.currentNode.nodeID, "\n");
            //VIP departs map
            ProcessVipDepart(vip);
            isSuccess = true;
        }
        return isSuccess;
    }


    /// <summary>
    /// Add tracker data every turn
    /// </summary>
    /// <param name="vip"></param>
    private void AddTrackerRecord(Vip vip)
    {
        HistoryVipMove data = new HistoryVipMove()
        {
            turn = GameManager.instance.turnScript.Turn,
            currentNodeID = vip.currentNode.nodeID,
            endNodeID = vip.currentEndNode.nodeID,
            timer = vip.timerTurns,
            isKnown = vip.isKnown,
        };
        GameManager.instance.dataScript.AddHistoryVipMove(data);
    }

    /// <summary>
    /// Checks to see if V.I.P spotted by contact
    /// </summary>
    private void ProcessContactInteraction(Vip vip)
    {
        Actor actor;
        Contact contact;
        List<int> listOfActorsWithContactsAtNode = GameManager.instance.dataScript.CheckContactResistanceAtNode(vip.currentNode.nodeID);
        if (listOfActorsWithContactsAtNode != null)
        {
            int numOfActors = listOfActorsWithContactsAtNode.Count;
            if (numOfActors > 0)
            {
                //V.I.P stealthRating
                int stealthRating = vip.stealthRating;
                //loop actors with contacts
                for (int i = 0; i < numOfActors; i++)
                {
                    actor = GameManager.instance.dataScript.GetActor(listOfActorsWithContactsAtNode[i]);
                    if (actor != null)
                    {
                        //only active actors can work their contact network
                        if (actor.Status == ActorStatus.Active)
                        {
                            contact = actor.GetContact(vip.currentNode.nodeID);
                            if (contact != null)
                            {
                                //contact active
                                if (contact.status == ContactStatus.Active)
                                {
                                    //check V.I.P stealth rating vs. contact effectiveness
                                    if (contact.effectiveness >= stealthRating)
                                    {
                                        Node node = vip.currentNode;
                                        //contact spots V.I.P
                                        vip.isKnown = true;
                                        string text = string.Format("V.I.P {0} has been spotted by Contact {1} {2}, {3}, at node {4}, id {5}", vip.tag, contact.nameFirst, contact.nameLast,
                                            contact.job, node.nodeName, node.nodeID);
                                        Debug.LogFormat("[Cnt] MissionManager.cs -> ProcessContactInteraction: Contact {0}, effectiveness {1}, SPOTS V.I.P {2}, StealthRating {3} at node {4}, id {5}{6}",
                                            contact.nameFirst, contact.effectiveness, vip.tag, stealthRating, node.nodeName, node.nodeID, "\n");
                                        GameManager.instance.messageScript.ContactVipSpotted(text, actor, node, contact, vip);
                                        //contact stats
                                        contact.statsVip++;
                                        //no need to check anymore as one sighting is enough
                                        break;
                                    }
                                    else
                                    {
                                        //contact Fails to spot V.I.P
                                        Debug.LogFormat("[Cnt] MissionManager.cs -> ProcessContactInteraction: Contact {0}, effectiveness {1}, FAILS to spot V.I.P {2}, adj StealthRating {3} at nodeID {4}{5}",
                                            contact.nameFirst, contact.effectiveness, vip.tag, stealthRating, vip.currentNode.nodeID, "\n");
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogFormat("Invalid contact (Null) for actor {0}, id {1} at node {2}, {3}, id {4}", actor.actorName, actor.actorID, vip.currentNode.nodeName,
                                      vip.currentNode.Arc.name, vip.currentNode.nodeID);
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
