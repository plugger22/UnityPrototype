using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using modalAPI;
using gameAPI;
using System;

public class Node : MonoBehaviour
{
    //NOTE -> LevelManager.arrayOfActiveNodes stores access data, eg. which nodes are active for which actor?

    [HideInInspector] public int nodeID;                       //unique ID, sequentially derived from GameManager nodeCounter, don't skip numbers, keep it sequential, 0+
    
    [HideInInspector] public string nodeName;                  //name of node, eg. "Downtown Bronx"
    [HideInInspector] public NodeArc Arc;                      //archetype type

    [HideInInspector] public bool isTracer;                    //has resistance tracer?
    [HideInInspector] public bool isTracerActive;              //within a tracer coverage (inclusive) of neighbouring nodes
    //[HideInInspector] public bool isTracerKnown;               //true if Authority knows of tracer coverage for this node
    [HideInInspector] public bool isSpider;                    //has authority spider?
    //[HideInInspector] public bool isSpiderKnown;               //does Resistance know of spider?
    [HideInInspector] public bool isActor;                     //true if any ActorStatus.Active actor has a connection at the node
    //[HideInInspector] public bool isActorKnown;                //true if Authority knows of Actor connection
    //[HideInInspector] public bool isTeamKnown;                 //true if Resistance knows of teams (additional means other than tracer coverage or connections)
    [HideInInspector] public int targetID;                     //unique ID, 0+, -1 indicates no target

    public Material _Material { get; private set; }     //material renderer uses to draw node

    private List<Vector3> listOfNeighbourPositions;     //list of neighbouring nodes that this node is connected to
    private List<Node> listOfNeighbourNodes;            //list of neighbouring nodes that this node is connected to 
    //private List<Node> listOfMoves;                     //list of neighouring nodes but stored as nodes for move calcs
    private List<Connection> listOfConnections;                //list of neighbouring connections
    private List<Team> listOfTeams;                     //Authority teams present at the node
    private List <EffectDataOngoing> listOfAdjustments;    //list of temporary (ongoing) effects impacting on the node

    private bool onMouseFlag;                           //flag indicates that onMouseOver is true (used for tooltip coroutine)
    private float mouseOverDelay;                       //tooltip
    private float fadeInTime;                           //tooltip

    //private backing fields
    private int _stability;
    private int _support;
    private int _security;
    private bool _isTracerKnown;
    private bool _isSpiderKnown;
    private bool _isActorKnown;
    private bool _isTeamKnown;

    //Properties for backing fields
    public int Security
    {
        get { return Mathf.Clamp(_security + GetNodeAdjustment(EffectOutcome.Security), 0, 3); }
        set { _security = value; Mathf.Clamp(_security, 0, 3); }
    }

    public int Stability
    {
        get { return Mathf.Clamp(_stability + GetNodeAdjustment(EffectOutcome.Stability), 0, 3); }
        set { _stability = value; Mathf.Clamp(_stability, 0, 3); }
    }

    public int Support
    {
        get { return Mathf.Clamp(_support + GetNodeAdjustment(EffectOutcome.Support), 0, 3); }
        set { _support = value; Mathf.Clamp(_support, 0, 3); }
    }

    public bool isTracerKnown
    {
        get
        {
            //any Ongoing effect overides current setting
            int value = GetNodeAdjustment(EffectOutcome.RevealTracers);
            if (value < 0) { return false; }
            else if (value > 0) { return true; }
            else { return _isTracerKnown; }
        }
        set { _isTracerKnown = value; }
    }

    public bool isSpiderKnown
    {
        get
        {
            //any Ongoing effect overides current setting
            int value = GetNodeAdjustment(EffectOutcome.RevealSpiders);
            if (value < 0) { return false; }
            else if (value > 0) { return true; }
            else { return _isSpiderKnown; }
        }
        set { _isSpiderKnown = value; }
    }

    public bool isActorKnown
    {
        get
        {
            //any Ongoing effect overides current setting
            int value = GetNodeAdjustment(EffectOutcome.RevealActors);
            if (value < 0) { return false; }
            else if (value > 0) { return true; }
            else { return _isActorKnown; }
        }
        set { _isActorKnown = value; }
    }

    public bool isTeamKnown
    {
        get
        {
            //any Ongoing effect overides current setting
            int value = GetNodeAdjustment(EffectOutcome.RevealTeams);
            if (value < 0) { return false; }
            else if (value > 0) { return true; }
            else { return _isTeamKnown; }
        }
        set { _isTeamKnown = value; }
    }

    /// <summary>
    /// Initialise SO's for Nodes
    /// </summary>
    /// <param name="archetype"></param>
    public void Initialise(NodeArc archetype)
    {
        Arc = archetype;
    }
	
	private void Awake ()
    {
        listOfNeighbourPositions = new List<Vector3>();
        listOfNeighbourNodes = new List<Node>();
        //listOfMoves = new List<Node>();
        listOfTeams = new List<Team>();
        listOfConnections = new List<Connection>();
        listOfAdjustments = new List<EffectDataOngoing>();
        _Material = GameManager.instance.nodeScript.GetNodeMaterial(NodeType.Normal);
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
        fadeInTime = GameManager.instance.tooltipScript.tooltipFade;
	}



    /// <summary>
    /// Left Mouse click
    /// </summary>
    private void OnMouseDown()
    {
        bool proceedFlag = true;
        if (GameManager.instance.CheckIsBlocked() == false)
        {
            //highlight current node
            GameManager.instance.nodeScript.ToggleNodeHighlight(nodeID);
            //exit any tooltip
            if (onMouseFlag == true)
            {
                onMouseFlag = false;
                StopCoroutine("ShowTooltip");
                GameManager.instance.tooltipNodeScript.CloseTooltip();
            }
            //Action Menu -> not valid if Resistance Plyr and Captured, etc.
            if (GameManager.instance.sideScript.PlayerSide == Side.Resistance)
            {
                if (GameManager.instance.turnScript.resistanceState != ResistanceState.Normal)
                { proceedFlag = false; }
            }
            if (proceedFlag == true)
            {
                ModalPanelDetails details = new ModalPanelDetails()
                {
                    nodeID = nodeID,
                    nodeName = this.nodeName,
                    nodeDetails = string.Format("{0} ID {1}", Arc.name, nodeID),
                    nodePos = transform.position,
                    listOfButtonDetails = GameManager.instance.actorScript.GetActorActions(nodeID)
                };
                //activate menu
                GameManager.instance.actionMenuScript.SetActionMenu(details);
            }
        }
    }

    /// <summary>
    /// mouse over exit, shut down tooltip
    /// </summary>
    private void OnMouseExit()
    {
        if (GameManager.instance.CheckIsBlocked() == false)
        {
            onMouseFlag = false;
            StopCoroutine("ShowTooltip");
            GameManager.instance.tooltipNodeScript.CloseTooltip();
        }
    }

    /// <summary>
    /// Mouse Over tool tip & Right Click
    /// </summary>
    private void OnMouseOver()
    {
        if (GameManager.instance.CheckIsBlocked() == false)
        {
            //Right click node -> Show either move options (node highlights) or Move Menu
            if (Input.GetMouseButtonDown(1) == true)
            {
                //exit any tooltip
                if (onMouseFlag == true)
                {
                    onMouseFlag = false;
                    StopCoroutine("ShowTooltip");
                    GameManager.instance.tooltipNodeScript.CloseTooltip();
                }
                //move action invalid if resistance player is captured, etc.
                if (GameManager.instance.sideScript.PlayerSide == Side.Resistance)
                {
                    if (GameManager.instance.turnScript.resistanceState == ResistanceState.Normal)
                    {
                        //Create a Move Menu at the node
                        if (GameManager.instance.dataScript.CheckValidMoveNode(nodeID) == true)
                        { EventManager.instance.PostNotification(EventType.CreateMoveMenu, this, nodeID); }
                        //highlight all possible move options
                        else
                        { EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.Move); }
                    }
                }
            }
            //Tool tip
            else
            {
                onMouseFlag = true;
                StartCoroutine(ShowTooltip());
            }
        }
    }


    /// <summary>
    /// tooltip coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowTooltip()
    {
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over node
        if (onMouseFlag == true)
        {
            //do once
            while (GameManager.instance.tooltipNodeScript.CheckTooltipActive() == false)
            {
                List<string> activeList = GetNodeActors();
                List<string> effectsList = GetOngoingEffects();
                List<string> teamList = new List<string>();
                if (listOfTeams.Count > 0)
                {
                    foreach (Team team in listOfTeams)
                    { teamList.Add(team.Arc.name.ToUpper()); }
                }
                List<string> targetList = new List<string>();
                if (targetID > -1)
                { targetList = GameManager.instance.targetScript.GetTargetTooltip(targetID); }
                //Transform transform = GetComponent<Transform>();
                NodeTooltipData dataTooltip = new NodeTooltipData()
                {
                    nodeName = nodeName,
                    type = string.Format("{0} ID {1}", Arc.name, nodeID),
                    isTracerActive = isTracerActive,
                    isActor = isActor,
                    isTeamKnown = isTeamKnown,
                    arrayOfStats = new int[] { Stability, Support, Security },
                    listOfActive = activeList,
                    listOfEffects = effectsList,
                    listOfTeams = teamList,
                    listOfTargets = targetList,
                    tooltipPos = transform.position
                    };
                GameManager.instance.tooltipNodeScript.SetTooltip(dataTooltip);
                yield return null;
            }
            //fade in
            float alphaCurrent;

            while (GameManager.instance.tooltipNodeScript.GetOpacity()< 1.0)
            {
                alphaCurrent = GameManager.instance.tooltipNodeScript.GetOpacity();
                alphaCurrent += Time.deltaTime / fadeInTime;
                GameManager.instance.tooltipNodeScript.SetOpacity(alphaCurrent);
                yield return null;
            }
        }
    }

    //
    // - - - Neighbours
    //

    public int GetNumOfNeighbours()
    { return listOfNeighbourPositions.Count; }

    /// <summary>
    /// add neighbouring vector3 to list
    /// </summary>
    /// <param name="pos"></param>
    public void AddNeighbourPosition(Vector3 pos)
    {
        if (pos != null)
        { listOfNeighbourPositions.Add(pos); }
        //Debug.Log("Neighbour added: " + pos);
        else { Debug.LogError("Invalid pos (Null)"); }
    }


    /// <summary>
    /// add neighbouring node to list of possible move locations
    /// </summary>
    /// <param name="node"></param>
    public void AddNeighbourNode(Node node)
    {
        Debug.Assert(node != null, "Invalid Node (Null)");
        listOfNeighbourNodes.Add(node);
    }

    /// <summary>
    /// Checks if a Vector3 node position is already present in the list of neighbours, e.g returns true if a connection already present
    /// </summary>
    /// <param name="newPos"></param>
    /// <returns></returns>
    public bool CheckNeighbourPosition(Vector3 newPos)
    {
        if (listOfNeighbourPositions.Count == 0)
        { return false; }
        else
        {
            if (listOfNeighbourPositions.Exists(pos => pos == newPos))
            { return true; }
            //default condition -> no match found
            return false;
        }
    }

    /// <summary>
    /// Get list of Neighbouring Nodes
    /// </summary>
    /// <returns></returns>
    public List<Node> GetNeighbouringNodes()
    { return listOfNeighbourNodes; }

    //
    // --- Other ---
    //

    /// <summary>
    /// Everytime player moves to a new node you have to call this to update master list of NodeID's that contain all valid move locations for the player's next move
    /// </summary>
    public void SetMoveNodes()
    {
        List<int> listOfNodeID = new List<int>();
        foreach (Node node in listOfNeighbourNodes)
        { listOfNodeID.Add(node.nodeID); }
        if (listOfNodeID.Count > 0)
        { GameManager.instance.dataScript.UpdateMoveNodes(listOfNodeID); }
        else { Debug.LogError("listOfNeighbourNodes has no records, listOfNodeID has no records -> MoveNodes not updated"); }
    }

    /// <summary>
    /// Add a connection to the list of neighbouring connections
    /// </summary>
    /// <param name="connection"></param>
    public void AddConnection(Connection connection)
    {
        if (connection != null)
        { listOfConnections.Add(connection); }
        else { Debug.LogError("Invalid Connection (Null)"); }
    }

    /// <summary>
    /// returns a neighbouring connection between the current node and the specified nodeId. 'Null' if none found
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public Connection GetConnection(int nodeID)
    {
        Connection connection = null;
        int node1, node2;
        if (GameManager.instance.dataScript.GetNode(nodeID) != null)
        {
            //loop list and find matching connection
            foreach (Connection connTemp in listOfConnections)
            {
                node1 = connTemp.GetNode1();
                node2 = connTemp.GetNode2();
                if (this.nodeID == node1)
                {
                    if (nodeID == node2) { connection = connTemp; }
                }
                else if (this.nodeID == node2)
                {
                    if (nodeID == node1) { connection = connTemp; }
                }
                //break loop if connection found
                if (connection != null) { break; }
            }
        }
        return connection;
    }

    /// <summary>
    /// returns a list of actors for whom this node is active
    /// </summary>
    /// <returns></returns>
    public List<string> GetNodeActors()
    {
        List<string> tempList = new List<string>();
        int limit = GameManager.instance.actorScript.numOfOnMapActors;
        Side side = GameManager.instance.sideScript.PlayerSide;
        for (int i = 0; i < limit; i++)
        {
            if (GameManager.instance.levelScript.CheckNodeActive(nodeID, side, i) == true)
            {
                tempList.Add(GameManager.instance.dataScript.GetCurrentActorType(i, side));
            }
        }
        return tempList;
    }

    /// <summary>
    /// returns a list of ongoing effects currently impacting the node, returns empty list if none
    /// </summary>
    /// <returns></returns>
    public List<string> GetOngoingEffects()
    {
        List<string> tempList = new List<string>();
        if (listOfAdjustments.Count > 0)
        {
            foreach (var ongoingEffect in listOfAdjustments)
            { tempList.Add(ongoingEffect.text); }
        }
        return tempList;
    }


    public void SetMaterial(Material newMaterial)
    { _Material = newMaterial; }


    public Material GetMaterial()
    { return _Material; }

    /// <summary>
    /// remove Tracer from node and tidy up bool fields
    /// </summary>
    public void RemoveTracer()
    {
        bool isNeighbourTracer, isAdjacentNeighbourTracer;
        if (isTracer == true)
        {
            isTracer = false;
            isNeighbourTracer = false;
            Debug.Log(string.Format("Tracer removed at nodeID {0}, \"{1}\"{2}", nodeID, nodeName, "\n"));
            //check neighbours
            foreach(Node node in listOfNeighbourNodes)
            {
                if (node.isTracer)
                { isNeighbourTracer = true; }
                else
                {
                    //check neighbours of the neighbour for a tracer to see if it is still active
                    List<Node> listOfAdjacentNeighbours = node.GetNeighbouringNodes();
                    if (listOfAdjacentNeighbours != null)
                    {
                        isAdjacentNeighbourTracer = false;
                        foreach(Node nodeNeighbour in listOfAdjacentNeighbours)
                        {
                            if (nodeNeighbour.isTracer == true)
                            { isAdjacentNeighbourTracer = true; }
                        }
                        //current adjacent node is still active if a neighbour has a tracer
                        if (isAdjacentNeighbourTracer == false)
                        { node.isTracerActive = false; }
                    }
                    else { Debug.LogError("Invalid listOfAdjacentNeighbours (Null)"); }
                }
            }
            //current node is still active if a neighbour has a tracer
            if (isNeighbourTracer == false)
            { isTracerActive = false; }
        }
    }

    /// <summary>
    /// Remove spider from node
    /// </summary>
    public void RemoveSpider()
    {
        if (isSpider == true)
        {
            isSpider = false;
            isSpiderKnown = false;
            Debug.Log(string.Format("Spider Removed at nodeID {0}, \"{1}\"{2}", nodeID, nodeName, "\n"));
        }
    }

    //
    // - - - Teams - - -
    //

    /// <summary>
    /// add an authority team to the node. Returns true if placement successful.
    /// Max one instance of each type of team at node
    /// Max cap on number of teams at node
    /// </summary>
    /// <param name="team"></param>
    public bool AddTeam(Team team, int actorID)
    {
        if (team != null)
        {
            //check there is room for another team
            if (listOfTeams.Count < GameManager.instance.teamScript.maxTeamsAtNode)
            {
                //check a similar type of team not already present
                int nodeArcID = team.Arc.TeamArcID;
                if (listOfTeams.Count > 0)
                {
                    foreach(Team teamExisting in listOfTeams)
                    {
                        if (teamExisting.Arc.TeamArcID == nodeArcID)
                        {
                            //already a similar team present -> no go
                            Debug.LogWarning(string.Format("{0} Team NOT added to node {1}, ID {2} as already a similar team present{3}", 
                                team.Arc.name, nodeName, nodeID, "\n"));
                            return false;
                        }
                    }
                }
                //Add team
                listOfTeams.Add(team);
                //initialise Team data
                team.NodeID = nodeID;
                team.ActorSlotID = actorID;
                team.Pool = TeamPool.OnMap;
                team.Timer = GameManager.instance.teamScript.deployTime;
                Debug.Log(string.Format("{0} Team added to node {1}, ID {2}{3}", team.Arc.name, nodeName, nodeID, "\n"));
                return true;
            }
            else { Debug.LogWarning(string.Format("Maximum number of teams already present at Node {0}, ID {1}{2}", nodeName, nodeID, "\n")); }
        }
        else { Debug.LogError(string.Format("Invalid team (null) for Node {0}, ID {1}{2}", nodeName, nodeID, "\n")); }
        return false;
    }

    /// <summary>
    /// Remove a team with the matching teamID from the listOfTeams and adjust team status. Returns true if successful, false otherwise.
    /// </summary>
    /// <param name="teamID"></param>
    public bool RemoveTeam(int teamID)
    {
        for(int i = 0; i < listOfTeams.Count; i++)
        {
            if (listOfTeams[i].TeamID == teamID)
            {
                listOfTeams.RemoveAt(i);
                Debug.Log(string.Format("TeamID {0} removed from Node ID {1}{2}", teamID, nodeID, "\n"));
                return true;
            }
        }
        //failed to find team
        Debug.LogError(string.Format("TeamID {0} not found in listOfTeams. Failed to remove team", teamID));
        return false;
    }


    /// <summary>
    /// Returns number of teams present at node, '0' if none
    /// </summary>
    /// <returns></returns>
    public int CheckNumOfTeams()
    { return listOfTeams.Count; }


    /// <summary>
    /// returns teamID if a team of that type is present at the node, -1 otherwise
    /// </summary>
    /// <param name="teamArcID"></param>
    /// <returns></returns>
    public int CheckTeamPresent(int teamArcID)
    {
        if (listOfTeams.Count > 0 && teamArcID > -1)
        {
            foreach (Team team in listOfTeams)
            {
                if (team.Arc.TeamArcID == teamArcID)
                { return team.TeamID; }
            }
        }
        return -1;
    }

    /// <summary>
    /// returns empty list if none
    /// </summary>
    /// <returns></returns>
    public List<Team> GetTeams()
    { return listOfTeams; }


    /// <summary>
    /// Add temporary effect to the listOfAdjustments
    /// </summary>
    /// <param name="ongoing"></param>
    /// <returns></returns>
    public void AddOngoingEffect(EffectDataOngoing ongoing)
    {
        if (ongoing != null)
        { listOfAdjustments.Add(ongoing); }
        else { Debug.LogError("Invalid EffectDataOngoing (Null)"); }
    }

    /// <summary>
    /// checks listOfAdjustments for any matching ongoingID and deletes them
    /// </summary>
    /// <param name="uniqueID"></param>
    public void RemoveOngoingEffect(int uniqueID)
    {
        if (listOfAdjustments.Count > 0)
        {
            //reverse loop, deleting as you go
            for (int i = listOfAdjustments.Count - 1; i >= 0; i--)
            {
                EffectDataOngoing ongoing = listOfAdjustments[i];
                if (ongoing.ongoingID == uniqueID)
                {
                    Debug.Log(string.Format("Node Effect: {0}, ID {1}, \"{2}\", ID {3}{4}", Arc.name.ToUpper(), nodeID, ongoing.text, ongoing.ongoingID, "\n"));
                    listOfAdjustments.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    /// changes fields and handles ongoing effects. Main method of changing node fields.
    /// Note: Ongoing effect doesn't affect field, just updates dictOfAdjustments ready for the following turns
    /// </summary>
    /// <param name="process"></param>
    public void ProcessNodeEffect(EffectDataProcess process)
    {
        if (process != null)
        {
            //Ongoing effect
            if (process.effectOngoing != null)
            {
                //create an entry in the dictOfAdjustments
                AddOngoingEffect(process.effectOngoing);
            }
            else
            {
                //immediate effect
                switch (process.outcome)
                {
                    case EffectOutcome.Security:
                        Security += process.value;
                        break;
                    case EffectOutcome.Stability:
                        Stability += process.value;
                        break;
                    case EffectOutcome.Support:
                        Support += process.value;
                        break;
                    case EffectOutcome.RevealTracers:
                        if (process.value <= 0) { isTracerKnown = false; }
                        else { isTracerKnown = true; }
                        break;
                    case EffectOutcome.RevealSpiders:
                        if (process.value <= 0) { isSpiderKnown = false; }
                        else { isSpiderKnown = true; }
                        break;
                    case EffectOutcome.RevealTeams:
                        if (process.value <= 0) { isTeamKnown = false; }
                        else { isTeamKnown = true; }
                        break;
                    case EffectOutcome.RevealActors:
                        if (process.value <= 0) { isActorKnown = false; }
                        else { isActorKnown = true; }
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid process.outcome \"{0}\"", process.outcome));
                        break;
                }
            }
        }
        else { Debug.LogError("Invalid effectProcess (Null)"); }
    }

    /// <summary>
    /// Returns tally of adjustments for the specified field, '0' if none
    /// </summary>
    /// <param name="outcome"></param>
    /// <returns></returns>
    private int GetNodeAdjustment(EffectOutcome outcome)
    {
        int value = 0;
        if (listOfAdjustments.Count > 0)
        {
            foreach(var adjust in listOfAdjustments)
            {
                if (adjust.outcome == outcome)
                { value += adjust.value; }
            }
        }
        return value;
    }

    //place methods above here
}
