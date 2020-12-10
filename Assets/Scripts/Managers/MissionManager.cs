using gameAPI;
using modalAPI;
using packageAPI;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Handles mission and level set-up
/// </summary>
public class MissionManager : MonoBehaviour
{

    [Header("Npc Globals")]
    [Tooltip("Desired node separation for RandomClose NpcNode option")]
    [Range(0, 10)] public int randomClose = 2;
    [Tooltip("Desired node separation for RandomMedium NpcNode option")]
    [Range(0, 10)] public int randomMedium = 4;
    [Tooltip("Desired node separation for RandomLong NpcNode option")]
    [Range(0, 10)] public int randomLong = 6;

    [HideInInspector] public Mission mission;

    //colour palette
    private string colourGood;
    private string colourBad;
    private string colourNeutral;
    /*private string colourNormal;
    private string colourDefault;*/
    private string colourAlert;
    private string colourGrey;
    /*private string colourCancel;*/
    private string colourEnd;

    /// <summary>
    /// Initialisation called from CampaignManager.cs -> Initialise
    /// NOTE: Initialises TargetManager.cs
    /// </summary>
    public void Initialise()
    {
        switch (GameManager.i.inputScript.GameState)
        {
            case GameState.NewInitialisation:
                SubInitialiseAll();
                SubInitialiseEvents();
                SubInitialiseNpc();
                break;
            case GameState.LoadAtStart:
            case GameState.LoadGame:
                SubInitialiseAll();
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseAll();
                SubInitialiseNpc();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
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
        else { listOfObjectives.AddRange(GameManager.i.dataScript.GetRandomObjectives(GameManager.i.objectiveScript.maxNumOfObjectives)); }
        GameManager.i.objectiveScript.SetObjectives(listOfObjectives);
        //initialise and assign targets
        GameManager.i.targetScript.Initialise();
        GameManager.i.targetScript.AssignTargets(mission);
        //Npc -> Human Resistance Player only
        if (GameManager.i.optionScript.isNPC == true)
        {
            if (GameManager.i.campaignScript.campaign.side.level == 2)
            { InitialiseNpc(); }
        }
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.i.AddListener(EventType.ChangeColour, OnEvent, "EffectManager");
        EventManager.i.AddListener(EventType.StartTurnLate, OnEvent, "MissionManager");
    }
    #endregion

    #region SubInitialiseNpc
    /// <summary>
    /// Npc initialisation start of a new level
    /// </summary>
    private void SubInitialiseNpc()
    {
        //clear out Invisible nodes
        if (mission.npc != null)
        { mission.npc.Reset(); }
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
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.StartTurnLate:
                StartTurnLate();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    #region SetColours

    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        colourGood = GameManager.i.colourScript.GetColour(ColourType.goodText);
        colourBad = GameManager.i.colourScript.GetColour(ColourType.badText);
        colourNeutral = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        /*colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.whiteText);*/
        colourAlert = GameManager.i.colourScript.GetColour(ColourType.salmonText);
        /*colourCancel = GameManager.instance.colourScript.GetColour(ColourType.moccasinText);*/
        colourGrey = GameManager.i.colourScript.GetColour(ColourType.greyText);
        colourEnd = GameManager.i.colourScript.GetEndTag();
    }
    #endregion

    /// <summary>
    /// Start Turn Late Event
    /// </summary>
    private void StartTurnLate()
    {
        // - - - NPC
        if (GameManager.i.optionScript.isNPC == true)
        {
            if (mission.npc != null)
            {
                ProcessNpc(mission.npc);
                if (mission.npc.status == NpcStatus.Active)
                {
                    //orgInfo not involved
                    if (GameManager.i.dataScript.CheckOrgInfoType(OrgInfoType.Npc) == false)
                    { ProcessContactInteraction(mission.npc); }
                    else
                    {
                        //orgInfo automatically spots Npc
                        Node node = mission.npc.currentNode;
                        if (node != null)
                        {
                            string text = string.Format("{0} tracks {1} at {2}, {3}, ID {4}", GameManager.i.campaignScript.campaign.orgInfo.tag, mission.npc.tag, node.nodeName, node.Arc.name,
                                node.nodeID);
                            GameManager.i.messageScript.OrganisationNpc(text, node, mission.npc);
                        }
                        else { Debug.LogWarning("Invalid node (Null) for npc.CurrentNode"); }
                    }
                }
            }
        }
    }

    /// <summary>
    /// returns the next mission in the sequence for the specified side. Returns null if a problem
    /// </summary>
    /// <returns></returns>
    public Mission GetNextMission(GlobalSide side)
    {
        Mission mission = null;
        int scenarioIndex = GameManager.i.campaignScript.GetScenarioIndex();
        if (scenarioIndex < GameManager.i.campaignScript.GetMaxScenarioIndex())
        {
            scenarioIndex++;
            switch (side.level)
            {
                case 1:
                    //Authority
                    mission = GameManager.i.campaignScript.GetScenario(scenarioIndex).missionAuthority;
                    break;
                case 2:
                    //Resistance
                    mission = GameManager.i.campaignScript.GetScenario(scenarioIndex).missionResistance;
                    break;
                default: Debug.LogWarningFormat("Unrecognised side \"{0}\"", side.name); break;
            }
        }
        else { Debug.LogWarningFormat("Invalid scenarioIndex \"{0}\" (should be < Max Scenario Index {1})", scenarioIndex, GameManager.i.campaignScript.GetMaxScenarioIndex()); }
        if (mission == null)
        { Debug.LogWarningFormat("Invalid mission (Null) for side \"{0}\"", side.name); }
        return mission;
    }


    //
    // - - - Npc
    //

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
                startNode = GameManager.i.dataScript.GetRandomNode();
                if (startNode == null) { Debug.LogError("Invalid random start node (Null)"); }
            }
            else
            {
                //assign start node
                startNode = GetNpcNode(mission.npc.nodeStart);
                //catch all
                if (startNode == null)
                { startNode = GameManager.i.dataScript.GetRandomNode(); }
            }
            //End node
            endNode = GetEndNode(startNode);
            //assign
            mission.npc.currentStartNode = startNode;
            mission.npc.currentEndNode = endNode;
            //status
            mission.npc.status = NpcStatus.Standby;
            Debug.LogFormat("[Npc] MissionManager.cs -> InitialiseNpc: Npc \"{0}\" starts {1}, {2}, ID {3}, ends {4}, {5}, ID {6}{7}", mission.npc.tag, startNode.nodeName, startNode.Arc.name, startNode.nodeID,
                endNode.nodeName, endNode.Arc.name, endNode.nodeID, "\n");
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
        { endNode = GameManager.i.dataScript.GetRandomNodeExclude(startNode); }
        else
        {
            //assign end node
            endNode = GetNpcNode(mission.npc.nodeEnd, startNode);
            //catch all
            if (endNode == null)
            { endNode = GameManager.i.dataScript.GetRandomNodeExclude(startNode); }
        }
        //fail safe
        if (endNode.nodeID == startNode.nodeID)
        {
            Debug.LogWarningFormat("Invalid Matching Npc nodes (start ID {0}, end ID {1} \"{2}\") for GetEndNode", endNode.nodeID, startNode.nodeID, mission.npc.nodeEnd.name);
            endNode = GameManager.i.dataScript.GetRandomNodeExclude(startNode);
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
                nodeID = GameManager.i.cityScript.airportDistrictID;
                node = GameManager.i.dataScript.GetNode(nodeID);
                if (node == null) { Debug.LogWarningFormat("Invalid Airport node (Null) for nodeID {0}", nodeID); }
                break;
            case "Harbour":
                nodeID = GameManager.i.cityScript.harbourDistrictID;
                if (nodeID > -1)
                { node = GameManager.i.dataScript.GetNode(nodeID); }
                else
                {
                    Debug.LogWarning("No Harbour district present (nodeID -1)");
                    if (sourceNode != null)
                    { node = GameManager.i.dataScript.GetRandomNodeExclude(sourceNode); }
                    else { node = GameManager.i.dataScript.GetRandomNode(); }
                }
                if (node == null) { Debug.LogWarningFormat("Invalid Harbour node (Null) for nodeID {0}", nodeID); }
                break;
            case "CityHall":
                nodeID = GameManager.i.cityScript.cityHallDistrictID;
                node = GameManager.i.dataScript.GetNode(nodeID);
                if (node == null) { Debug.LogWarningFormat("Invalid City Hall node (Null) for nodeID {0}", nodeID); }
                break;
            case "Icon":
                nodeID = GameManager.i.cityScript.iconDistrictID;
                if (nodeID > -1)
                { node = GameManager.i.dataScript.GetNode(nodeID); }
                else
                {
                    Debug.LogWarning("No Icon district present (nodeID -1)");
                    if (sourceNode != null)
                    { node = GameManager.i.dataScript.GetRandomNodeExclude(sourceNode); }
                    else { node = GameManager.i.dataScript.GetRandomNode(); }
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
                    if (sourceNode != null)
                    { node = GameManager.i.dataScript.GetRandomNode(arc.nodeArcID, sourceNode.nodeID); }
                    else { node = GameManager.i.dataScript.GetRandomNode(arc.nodeArcID); }
                    if (node == null) { Debug.LogWarningFormat("Invalid node (Null) for \"{0}\"", npcNode.name); }
                }
                else { Debug.LogWarningFormat("Invalid nodeArc (Null) for \"{0}\"", npcNode.name); }
                break;
            case "RandomClose":
            case "RandomMedium":
            case "RandomLong":
                if (sourceNode == null)
                { sourceNode = GameManager.i.dataScript.GetRandomNode(); }
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
                    node = GameManager.i.dijkstraScript.GetRandomNodeAtDistance(sourceNode, distance);
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
        if (GameManager.i.turnScript.Turn >= npc.startTurn)
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
                    npc.daysActive = 1;
                    GameManager.i.nodeScript.nodeNpc = npc.currentStartNode.nodeID;
                    Debug.LogFormat("[Npc] MissionManager.cs -> CheckNpcActive: Npc \"{0}\" OnMap (rnd {1}, needed < {2}) at {3}, {4}, ID {5}, timer {6}{7}", npc.tag, rnd, npc.startChance,
                        npc.currentNode.nodeName, npc.currentNode.Arc.name, npc.currentNode.nodeID, npc.timerTurns, "\n");
                    string text = string.Format("{0} has arrived in City at {1}, {2}, ID {3}", npc.tag, npc.currentStartNode.nodeName, npc.currentStartNode.Arc.name, npc.currentStartNode.nodeID);
                    GameManager.i.messageScript.NpcArrival(text, npc);
                    //tracker
                    AddTrackerRecord(npc);
                    //outcome msg (infoPipeline)
                    string goodEffects = GetEffects(npc.listOfGoodEffects, colourGood);
                    string badEffects = GetEffects(npc.listOfBadEffects, colourBad);
                    string textTopString = string.Format("A {0}{1}{2} has arrived in the city. They have {3}{4}{5}", colourAlert, npc.tag, colourEnd, colourAlert, npc.item, colourEnd);
                    string textBottomString = string.Format("We want you to {0}{1}{2}{3}{4}If successful{5}{6}{7}{8}If you fail{9}{10}", colourNeutral, npc.action.activity, colourEnd, "\n", "\n",
                        "\n", goodEffects, "\n", "\n", "\n", badEffects);
                    ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
                    {
                        textTop = textTopString,
                        textBottom = textBottomString,
                        sprite = npc.sprite,
                        isAction = false,
                        side = GameManager.i.globalScript.sideResistance,
                        type = MsgPipelineType.Npc,
                        help0 = "npc_0",
                        help1 = "npc_1"
                    };
                    if (GameManager.i.guiScript.InfoPipelineAdd(outcomeDetails) == false)
                    { Debug.LogWarningFormat("Npc arrives InfoPipeline message FAILED to be added to dictOfPipeline"); }
                    //msg
                    GameManager.i.messageScript.NpcOngoing("Npc arrives onMap", npc);
                    //tracer
                    CheckIfTracerSpotsNpc(npc);
                }
                else { Debug.LogWarning("Invalid Npc currentStartNode (Null)"); }
            }
            else { Debug.LogFormat("[Npc] MissionManager.cs -> CheckNpcActive: Npc \"{0}\" failed Activation roll (rnd {1}, needed < {2}){3}", npc.tag, rnd, npc.startChance, "\n"); }
        }
    }

    /// <summary>
    /// returns true if there is an active NPC onMap (false otherwise, auto handles cases of no NPC present in level)
    /// </summary>
    /// <returns></returns>
    public bool CheckIfNpcOnMap()
    {
        if (mission.npc != null)
        {
            if (mission.npc.status == NpcStatus.Active)
            { return true; }
        }
        return false;
    }

    /// <summary>
    /// subMethod to return a colour formatted string to explain what a listOfEffects would do. ColourEffect is to differentiate between good and bad effects
    /// </summary>
    /// <param name="listOfEffects"></param>
    /// <param name="colourEffect"></param>
    /// <returns></returns>
    private string GetEffects(List<Effect> listOfEffects, string colourEffect)
    {
        StringBuilder builder = new StringBuilder();
        if (listOfEffects != null)
        {
            if (listOfEffects.Count > 0)
            {
                foreach (Effect effect in listOfEffects)
                {
                    if (builder.Length > 0) { builder.AppendLine(); }
                    builder.AppendFormat("{0}{1}{2}", colourEffect, effect.description, colourEnd);
                }
            }
            else { builder.AppendFormat("{0}No Effect{1}", colourGrey, colourEnd); }
        }
        else
        {
            Debug.LogWarning("Invalid listOfEffects (Null)");
            builder.AppendFormat("{0}No Effect{1}", colourGrey, colourEnd);
        }
        return builder.ToString();
    }

    /// <summary>
    /// returns a success/fail combined colour formatted string suitable for NpcArrival message
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public string GetFormattedNpcEffectsAll(Npc npc)
    {
        string goodEffects = GetEffects(npc.listOfGoodEffects, colourGood);
        string badEffects = GetEffects(npc.listOfBadEffects, colourBad);
        return string.Format("<b>If You Succeed{0}{1}{2}If You Fail{3}{4}</b>", "\n", goodEffects, "\n", "\n", badEffects);
    }

    /// <summary>
    /// returns a success colour formatted string suitable for Npc Interact message. Auto handles no effect present
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public string GetFormattedNpcEffectsGood(Npc npc)
    {
        string goodEffects = GetEffects(npc.listOfGoodEffects, colourGood);
        return string.Format("<b>{0}</b>", goodEffects);
    }

    /// <summary>
    /// returns a fail colour formatted string suitable for Npc Depart message. Auto handles no effect present
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public string GetFormattedNpcEffectsBad(Npc npc)
    {
        string BadEffects = GetEffects(npc.listOfBadEffects, colourGood);
        return string.Format("<b>{0}</b>", BadEffects);
    }

    /// <summary>
    /// Updates Active onMap Npc
    /// </summary>
    /// <param name="npc"></param>
    private void UpdateActiveNpc(Npc npc)
    {
        //update day tally regardless
        npc.daysActive++;
        //check if in same district as Player
        if (npc.currentNode.nodeID == GameManager.i.nodeScript.GetPlayerNodeID())
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
                    {
                        GameManager.i.messageScript.NpcDepart("Npc Departs", npc);
                        ProcessNpcDepart(npc);
                    }
                    //repeat but timer has expired
                    else
                    {
                        if (npc.timerTurns <= 0)
                        {
                            GameManager.i.messageScript.NpcDepart("Npc Departs", npc);
                            ProcessNpcDepart(npc);
                        }
                        else
                        {
                            //still going, create a new path (uses a move action to do so)
                            npc.currentStartNode = npc.currentNode;
                            npc.currentEndNode = GetEndNode(npc.currentNode);
                            if (npc.currentEndNode != null)
                            {
                                Debug.LogFormat("[Npc] MissionManager.cs -> UpdateActiveNpc: New path from {0}, {1}, ID {2} to {3}, {4}, ID {5}{6}", npc.currentStartNode.nodeName, npc.currentStartNode.Arc.name,
                                    npc.currentStartNode.nodeID, npc.currentEndNode.nodeName, npc.currentEndNode.Arc.name, npc.currentEndNode.nodeID, "\n");
                                //msg
                                GameManager.i.messageScript.NpcOngoing("Npc still onMap", npc);
                                //tracer
                                CheckIfTracerSpotsNpc(npc);
                            }
                            else
                            {
                                Debug.LogWarning("Invalid currentEndNode (Null) for Npc when calculating new path");
                                GameManager.i.messageScript.NpcDepart("Npc Departs", npc);
                                ProcessNpcDepart(npc);
                            }
                        }
                    }
                }
                else
                {
                    //Npc moves towards destination -> Get Path
                    List<Connection> listOfConnections = GameManager.i.dijkstraScript.GetPath(npc.currentNode.nodeID, npc.currentEndNode.nodeID, false);
                    if (listOfConnections != null)
                    {
                        int numOfLinks = listOfConnections.Count;
                        int nextNodeID;
                        if (numOfLinks > 0)
                        {
                            //move npc one link max
                            Connection connection = listOfConnections[0];
                            if (connection != null)
                            {
                                //get the node to move to for this link
                                nextNodeID = connection.GetNode1();
                                if (nextNodeID == npc.currentNode.nodeID)
                                { nextNodeID = connection.GetNode2(); }
                                //move forward one link
                                Node node = GameManager.i.dataScript.GetNode(nextNodeID);
                                if (node != null)
                                {
                                    npc.currentNode = node;
                                    GameManager.i.nodeScript.nodeNpc = nextNodeID;
                                    Debug.LogFormat("[Npc] MissionManager.cs -> UpdateActiveNpc: Npc \"{0}\" moved to {1}, {2}, nodeID {3}, timer {4}{5}",
                                        npc.tag, node.nodeName, node.Arc.name, node.nodeID, npc.timerTurns, "\n");
                                    //msg
                                    GameManager.i.messageScript.NpcOngoing("Npc still onMap", npc);
                                    //tracer
                                    CheckIfTracerSpotsNpc(npc);
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
            else
            {
                Debug.LogFormat("[Npc] MissionManager.cs -> CheckNpcActive: Npc \"{0}\" didn't move (rnd {1}, needed < {2}), timer {3}{4}", npc.tag, rnd, npc.moveChance, npc.timerTurns, "\n");
                //msg
                GameManager.i.messageScript.NpcOngoing("Npc still onMap", npc);
                //tracer
                CheckIfTracerSpotsNpc(npc);
            }
            //tracker
            AddTrackerRecord(npc);
        }
    }

    /// <summary>
    /// SubMethod to determine if tracer present and if so, does it spot npc? (automatic spotting but no if orgInfo active or npc in Invisible Mode). Generates message if so.
    /// </summary>
    private void CheckIfTracerSpotsNpc(Npc npc)
    {
        if (npc.currentNode.isTracer == true)
        {
            //OrgInfo not involved
            if (GameManager.i.dataScript.CheckOrgInfoType(OrgInfoType.Npc) == false)
            {
                if (npc.CheckIfInvisibleMode() == false)
                { GameManager.i.messageScript.TracerNpcSpotted("Npc Spotted", npc); }
                else
                {
                    Debug.LogFormat("[Npc] MissionManager.cs -> CheckNpcActive: Npc in Invisible Mode and MISSED by Tracer at {0}, {1}, nodeID {2}{3}",
                        npc.currentNode.nodeName, npc.currentNode.Arc.name, npc.currentNode.nodeID, "\n");
                }
            }
        }
    }

    /// <summary>
    /// handles all admin for Npc departing map (reached destination and no repeat or repeat but timer has run out). isInteract true if player has interacted with Npc prior to departure
    /// </summary>
    private void ProcessNpcDepart(Npc npc, bool isInteract = false)
    {
        //outcome message for InfoPipeline if Player hasn't interacted with Npc
        if (isInteract == false)
        {
            //bad effects
            string effectText = ProcessEffects(npc, false);
            string textTopString = string.Format("The {0}{1}{2} catches a shuttle out of {3}. You failed to {4}{5}{6}", colourNeutral, npc.tag, colourEnd,
                GameManager.i.cityScript.GetCity().tag, colourBad, npc.action.activity, colourEnd);
            string textBottomString = effectText;
            //pipeline msg
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
            {
                textTop = textTopString,
                textBottom = textBottomString,
                sprite = npc.sprite,
                isAction = false,
                side = GameManager.i.globalScript.sideResistance,
                type = MsgPipelineType.Npc
            };
            if (GameManager.i.guiScript.InfoPipelineAdd(outcomeDetails) == false)
            { Debug.LogWarningFormat("Npc departs with Player InfoPipeline message FAILED to be added to dictOfPipeline"); }
        }
        npc.status = NpcStatus.Departed;
        npc.currentNode = null;
        GameManager.i.nodeScript.nodeNpc = -1;
        Debug.LogFormat("[Npc] MissionManager.cs -> ProcessNpcDepart: Npc \"{0}\" Departed{1}", npc.tag, "\n");
        //infoOrg reset
        if (GameManager.i.campaignScript.campaign.orgInfo != null)
        { GameManager.i.orgScript.CancelOrgInfoTracking(OrgInfoType.Npc); }
    }

    /// <summary>
    /// Active Player at same node as Npc ->  interacts (can't happen if Npc in Invisible mode)
    /// </summary>
    /// <param name="npc"></param>
    private bool ProcessNpcInteract(Npc npc)
    {
        bool isSuccess = false;
        if (npc.CheckIfInvisibleMode() == false)
        {
            //Player interacts with Npc
            if (GameManager.i.playerScript.status == ActorStatus.Active)
            {
                //good effects
                string effectText = ProcessEffects(npc, true);
                string textTopString = string.Format("You {0}{1}{2} the {3}{4}{5}", colourAlert, npc.action.tag, colourEnd, colourAlert, npc.tag, colourEnd);
                string textBottomString = string.Format("The {0} {1} at {2}{3}{4}{5}{6}{7}", npc.tag, npc.action.outcome, colourAlert, npc.currentNode.nodeName, colourEnd, "\n", "\n", effectText);
                //pipeline msg
                ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
                {
                    textTop = textTopString,
                    textBottom = textBottomString,
                    sprite = npc.sprite,
                    isAction = false,
                    side = GameManager.i.globalScript.sideResistance,
                    type = MsgPipelineType.Npc
                };
                if (GameManager.i.guiScript.InfoPipelineAdd(outcomeDetails) == false)
                { Debug.LogWarningFormat("Npc interacts with Player InfoPipeline message FAILED to be added to dictOfPipeline"); }
                //messages (need to be BEFORE depart)
                Debug.LogFormat("[Npc] MissionManager.cs -> UpdateActiveNpc: Player INTERACTS with Npc \"{0}\" at {1}, {2}, ID {3}{4}", npc.tag, npc.currentNode.nodeName, npc.currentNode.Arc.name,
                                    npc.currentNode.nodeID, "\n");
                GameManager.i.messageScript.NpcInteract("Npc interacted with", npc);
                //history
                GameManager.i.dataScript.AddHistoryPlayer(new HistoryActor() { text = string.Format("You {0} the {1}", npc.action.tag, npc.tag), district = npc.currentNode.nodeName });
                //Npc departs map
                ProcessNpcDepart(npc, true);
                isSuccess = true;
            }
        }
        else { Debug.LogFormat("[Npc] MissionManager.cs -> ProcessNpcInteract: Npc in Invisible Mode, {0}, {1}, nodeID {2}{3}", npc.currentNode.nodeName, npc.currentNode.Arc.name, npc.currentNode.nodeID, "\n"); }
        return isSuccess;
    }


    /// <summary>
    /// Add tracker data every turn
    /// </summary>
    /// <param name="npc"></param>
    private void AddTrackerRecord(Npc npc)
    {
        int npcCurrentNodeID = -1;
        int npcEndNodeID = -1;
        //handles departing Npc
        if (npc.currentNode != null) { npcCurrentNodeID = npc.currentNode.nodeID; }
        if (npc.currentEndNode != null) { npcEndNodeID = npc.currentEndNode.nodeID; }
        //create record
        HistoryNpcMove data = new HistoryNpcMove()
        {
            turn = GameManager.i.turnScript.Turn,
            currentNodeID = npcCurrentNodeID,
            endNodeID = npcEndNodeID,
            timer = npc.timerTurns,
        };
        //add record
        GameManager.i.dataScript.AddHistoryNpcMove(data);
    }

    /// <summary>
    /// Checks to see if Npc spotted by contact (can't happen if Npc in Invisible mode)
    /// </summary>
    private void ProcessContactInteraction(Npc npc)
    {
        if (npc.CheckIfInvisibleMode() == false)
        {
            Actor actor;
            Contact contact;
            List<int> listOfActorsWithContactsAtNode = GameManager.i.dataScript.CheckContactResistanceAtNode(npc.currentNode.nodeID);
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
                        actor = GameManager.i.dataScript.GetActor(listOfActorsWithContactsAtNode[i]);
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
                                            GameManager.i.messageScript.ContactNpcSpotted(text, actor, node, contact, npc);
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
        else { Debug.LogFormat("[Npc] MissionManager.cs -> ProcessContactInteraction: Npc in Invisible Mode, {0}, {1}, nodeID {2}{3}", npc.currentNode.nodeName, npc.currentNode.Arc.name, npc.currentNode.nodeID, "\n"); }
    }


    /// <summary>
    /// process good effects, if any, on interaction (isGood true) and bad effects, if any, on depart (isGood false) provided there was no interaction
    /// </summary>
    /// <param name="isGood"></param>
    /// <param name="npc"></param>
    /// <returns></returns>
    private string ProcessEffects(Npc npc, bool isGood)
    {
        StringBuilder builder = new StringBuilder();
        List<Effect> listOfEffects = new List<Effect>();
        if (isGood == true) { listOfEffects = npc.listOfGoodEffects; }
        else { listOfEffects = npc.listOfBadEffects; }
        //valid effects
        if (listOfEffects != null && listOfEffects.Count > 0)
        {
            //data packages
            EffectDataReturn effectReturn = new EffectDataReturn();
            EffectDataInput effectInput = new EffectDataInput();
            effectInput.originText = npc.tag;
            effectInput.source = EffectSource.Mission;
            Node node = npc.currentNode;
            if (node != null)
            {
                //loop effects
                foreach (Effect effect in listOfEffects)
                {
                    effectReturn = GameManager.i.effectScript.ProcessEffect(effect, node, effectInput);
                    if (builder.Length > 0) { builder.AppendLine(); builder.AppendLine(); }
                    if (string.IsNullOrEmpty(effectReturn.topText) == false)
                    { builder.AppendFormat("{0}{1}{2}", effectReturn.topText, "\n", effectReturn.bottomText); }
                    else { builder.Append(effectReturn.bottomText); }
                }
            }
        }
        else { /*Debug.LogWarningFormat("No valid effects present for Npc \"{0}\", isGood {1}", npc.tag, isGood); EDIT: O.K to have no effects*/ }
        return builder.ToString();
    }

    /// <summary>
    /// Load Saved Npc data
    /// </summary>
    /// <param name="npcSave"></param>
    public void SetNpcData(SaveNpc npc = null)
    {
        if (npc != null)
        {
            if (npc != null)
            {
                mission.npc.status = npc.status;
                mission.npc.timerTurns = npc.timerTurns;
                mission.npc.daysActive = npc.daysActive;
                //Nodes -> if '-1' then values are null
                if (npc.startNodeID > -1)
                { mission.npc.currentStartNode = GameManager.i.dataScript.GetNode(npc.startNodeID); }
                else
                {
                    mission.npc.currentStartNode = null;
                    Debug.LogWarning("Invalid Npc currentStartNode (Null)");
                }
                if (npc.endNodeID > -1)
                { mission.npc.currentEndNode = GameManager.i.dataScript.GetNode(npc.endNodeID); }
                else
                {
                    mission.npc.currentEndNode = null;
                    Debug.LogWarning("Invalid Npc currentEndNode (Null)");
                }
                //O.K for currentNode to be Null
                if (npc.currentNodeID > -1)
                { mission.npc.currentNode = GameManager.i.dataScript.GetNode(npc.currentNodeID); }
                else { mission.npc.currentNode = null; }
            }
            else { Debug.LogError("Invalid npcSave (Null)"); }
        }
        else { mission.npc = null; }
    }

    /// <summary>
    /// Resets npc listOfInvisibile nodes
    /// </summary>
    public void ResetNpcData()
    {
        if (mission.npc != null)
        { mission.npc.Reset(); }
    }

    //new methods above here
}
