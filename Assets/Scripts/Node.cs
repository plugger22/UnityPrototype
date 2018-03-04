using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using modalAPI;
using gameAPI;
using packageAPI;

public class Node : MonoBehaviour
{
    //NOTE -> LevelManager.arrayOfActiveNodes stores access data, eg. which nodes are active for which actor?

    [HideInInspector] public int nodeID;                //unique ID, sequentially derived from GameManager nodeCounter, don't skip numbers, keep it sequential, 0+
    
    [HideInInspector] public string nodeName;           //name of node, eg. "Downtown Bronx"
    [HideInInspector] public NodeArc Arc;               //archetype type

    [HideInInspector] public bool isTracer;             //has resistance tracer?
    [HideInInspector] public bool isTracerActive;       //within a tracer coverage (inclusive) of neighbouring nodes
    [HideInInspector] public bool isSpider;             //has authority spider?
    [HideInInspector] public bool isContact;              //true if any ActorStatus.Active actor has a connection at the node
    [HideInInspector] public int targetID;              //unique ID, 0+, -1 indicates no target

    public Material _Material { get; private set; }     //material renderer uses to draw node

    private List<Vector3> listOfNeighbourPositions;     //list of neighbouring nodes that this node is connected to
    private List<Node> listOfNeighbourNodes;            //list of neighbouring nodes that this node is connected to 
    private List<Connection> listOfConnections;         //list of neighbouring connections
    private List<Team> listOfTeams;                     //Authority teams present at the node
    private List <EffectDataOngoing> listOfOngoingEffects; //list of temporary (ongoing) effects impacting on the node

    private bool onMouseFlag;                           //flag indicates that onMouseOver is true (used for tooltip coroutine)
    private float mouseOverDelay;                       //tooltip
    private float fadeInTime;                           //tooltip

    private int maxValue;                               //max and min node datapoint values (derive from NodeManager.cs)
    private int minValue;

    //private backing fields
    private int _stability;
    private int _support;
    private int _security;
    private bool _isTracerKnown;                        //true if Authority knows of tracer coverage for this node
    private bool _isSpiderKnown;                        //does Resistance know of spider?
    private bool _isContactKnown;                         //true if Authority knows of Actor contacts
    private bool _isTeamKnown;                          //true if Resistance knows of teams (additional means other than tracer coverage or connections)

    //Properties for backing fields
    public int Security
    {
        get { return Mathf.Clamp(_security + GetOngoingEffect(GameManager.instance.nodeScript.outcomeNodeSecurity), minValue, maxValue); }
        set { _security = value; Mathf.Clamp(_security, 0, 3); }
    }

    public int Stability
    {
        get { return Mathf.Clamp(_stability + GetOngoingEffect(GameManager.instance.nodeScript.outcomeNodeStability), minValue, maxValue); }
        set { _stability = value; Mathf.Clamp(_stability, 0, 3); }
    }

    public int Support
    {
        get { return Mathf.Clamp(_support + GetOngoingEffect(GameManager.instance.nodeScript.outcomeNodeSupport), minValue, maxValue); }
        set { _support = value; Mathf.Clamp(_support, 0, 3); }
    }

    public bool isTracerKnown
    {
        get
        {
            //any Ongoing effect overides current setting
            int value = GetOngoingEffect(GameManager.instance.nodeScript.outcomeStatusTracers);
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
            int value = GetOngoingEffect(GameManager.instance.nodeScript.outcomeStatusSpiders);
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
            int value = GetOngoingEffect(GameManager.instance.nodeScript.outcomeStatusContacts);
            if (value < 0) { return false; }
            else if (value > 0) { return true; }
            else { return _isContactKnown; }
        }
        set { _isContactKnown = value; }
    }

    public bool isTeamKnown
    {
        get
        {
            //any Ongoing effect overides current setting
            int value = GetOngoingEffect(GameManager.instance.nodeScript.outcomeStatusTeams);
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
        listOfOngoingEffects = new List<EffectDataOngoing>();
        _Material = GameManager.instance.nodeScript.GetNodeMaterial(NodeType.Normal);
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
        fadeInTime = GameManager.instance.tooltipScript.tooltipFade;
        maxValue = GameManager.instance.nodeScript.maxNodeValue;
        minValue = GameManager.instance.nodeScript.minNodeValue;
	}



    /// <summary>
    /// Left Mouse click
    /// </summary>
    private void OnMouseDown()
    {
        bool proceedFlag = true;
        if (GameManager.instance.guiScript.CheckIsBlocked() == false)
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
            if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
            {
                if (GameManager.instance.turnScript.resistanceState != ResistanceState.Normal)
                { proceedFlag = false; }
            }
            if (proceedFlag == true)
            {
                ModalPanelDetails details = new ModalPanelDetails()
                {
                    itemID = nodeID,
                    itemName = this.nodeName,
                    itemDetails = string.Format("{0} ID {1}", Arc.name, nodeID),
                    itemPos = transform.position,
                    listOfButtonDetails = GameManager.instance.actorScript.GetNodeActions(nodeID),
                    menuType = ActionMenuType.Node
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
        if (GameManager.instance.guiScript.CheckIsBlocked() == false)
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
        if (GameManager.instance.guiScript.CheckIsBlocked() == false)
        {
            //Right click node -> Show either move options (node highlights) or Move Menu
            if (Input.GetMouseButtonDown(1) == true)
            {
                //exit any tooltip
                if (onMouseFlag == true)
                { onMouseFlag = false; }
                //move action invalid if resistance player is captured, etc.
                if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
                {
                    if (GameManager.instance.turnScript.resistanceState == ResistanceState.Normal)
                    {
                        //exit any tooltip
                        StopCoroutine("ShowTooltip");
                        GameManager.instance.tooltipNodeScript.CloseTooltip();
                        //Create a Move Menu at the node
                        if (GameManager.instance.dataScript.CheckValidMoveNode(nodeID) == true)
                        { EventManager.instance.PostNotification(EventType.CreateMoveMenu, this, nodeID); }
                        //highlight all possible move options
                        else
                        {
                            EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.Move);
                            //if at Player's current node then Gear Node menu
                            if (nodeID == GameManager.instance.nodeScript.nodePlayer)
                            { EventManager.instance.PostNotification(EventType.CreateGearNodeMenu, this, nodeID); }
                        }
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
                List<EffectDataTooltip> effectsList = GetOngoingEffects();
                List<string> teamList = new List<string>();
                if (listOfTeams.Count > 0)
                {
                    foreach (Team team in listOfTeams)
                    { teamList.Add(team.Arc.name); }
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
                    isActor = isContact,
                    isActorKnown = isActorKnown,
                    isTeamKnown = isTeamKnown,
                    arrayOfStats = GetStats(),
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
        int limit = GameManager.instance.actorScript.maxNumOfOnMapActors;
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
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
    public List<EffectDataTooltip> GetOngoingEffects()
    {
        List<EffectDataTooltip> tempList = new List<EffectDataTooltip>();
        if (listOfOngoingEffects.Count > 0)
        {
            foreach (var ongoingEffect in listOfOngoingEffects)
            {
                EffectDataTooltip data = new EffectDataTooltip();
                //data.text = string.Format("{0} {1}{2}", ongoingEffect.text, ongoingEffect.value > 0 ? "+" : "", ongoingEffect.value);
                data.text = ongoingEffect.text;
                data.type = ongoingEffect.type;
                tempList.Add(data);
            }
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
        {
            //create a new value effect as otherwise passed by reference and timers will decrement for identical ongoingID's as one.
            EffectDataOngoing effect = new EffectDataOngoing();
            effect.ongoingID = ongoing.ongoingID;
            effect.text = ongoing.text;
            effect.value = ongoing.value;
            effect.timer = ongoing.timer;
            effect.outcome = ongoing.outcome;
            effect.type = ongoing.type;
            effect.apply = ongoing.apply;
            effect.side = ongoing.side;
            //add new ongoing effect
            listOfOngoingEffects.Add(effect);
            //add to register & create message
            GameManager.instance.dataScript.AddOngoingEffectToDict(effect, nodeID);
        }
        else { Debug.LogError("Invalid EffectDataOngoing (Null)"); }
    }

    /// <summary>
    /// checks listOfAdjustments for any matching ongoingID and deletes them
    /// </summary>
    /// <param name="uniqueID"></param>
    public void RemoveOngoingEffect(int uniqueID)
    {
        if (listOfOngoingEffects.Count > 0)
        {
            //reverse loop, deleting as you go
            for (int i = listOfOngoingEffects.Count - 1; i >= 0; i--)
            {
                EffectDataOngoing ongoing = listOfOngoingEffects[i];
                if (ongoing.ongoingID == uniqueID)
                {
                    //Debug.Log(string.Format("Node Effect: {0}, ID {1}, \"{2}\", ID {3}{4}", Arc.name.ToUpper(), nodeID, ongoing.text, ongoing.ongoingID, "\n"));
                    GameManager.instance.dataScript.RemoveOngoingEffect(ongoing, nodeID);
                    listOfOngoingEffects.RemoveAt(i);
                }
            }
        }
    }


    /// <summary>
    /// Returns tally of ongoing effects for the specified field, '0' if none
    /// </summary>
    /// <param name="outcome"></param>
    /// <returns></returns>
    private int GetOngoingEffect(EffectOutcome outcome)
    {
        int value = 0;
        if (listOfOngoingEffects.Count > 0)
        {
            foreach (var adjust in listOfOngoingEffects)
            {
                if (adjust.outcome == outcome)
                { value += adjust.value; }
            }
        }
        return value;
    }

    /// <summary>
    /// Checks each effect, if any, decrements timers and deletes any that have expired
    /// </summary>
    public void ProcessOngoingEffectTimers()
    {
        if (listOfOngoingEffects.Count > 0)
        {
            for(int i = listOfOngoingEffects.Count - 1; i >= 0; i--)
            {
                //decrement timer
                EffectDataOngoing ongoing = listOfOngoingEffects[i];
                Debug.Log(string.Format("Node ID {0}, Timer before {1}{2}", nodeID, ongoing.timer, "\n"));
                ongoing.timer--;
                if (ongoing.timer <= 0)
                {
                    //message
                    Debug.Log(string.Format("REMOVE: Ongoing effect ID {0}, \"{1}\" from node ID {2}{3}", ongoing.ongoingID, ongoing.text, nodeID, "\n"));
                    //delete effect
                    GameManager.instance.dataScript.RemoveOngoingEffect(ongoing, nodeID);
                    listOfOngoingEffects.RemoveAt(i);
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
                //create an entry in listOfOngoingEffects
                AddOngoingEffect(process.effectOngoing);
            }
            else
            {
                //immediate effect
                switch (process.outcome.name)
                {
                    case "NodeSecurity":
                        Security += process.value;
                        break;
                    case "NodeStability":
                        Stability += process.value;
                        break;
                    case "NodeSupport":
                        Support += process.value;
                        break;
                    case "StatusTracers":
                        if (process.value <= 0) { isTracerKnown = false; }
                        else { isTracerKnown = true; }
                        break;
                    case "StatusSpiders":
                        if (process.value <= 0) { isSpiderKnown = false; }
                        else { isSpiderKnown = true; }
                        break;
                    case "StatusTeams":
                        if (process.value <= 0) { isTeamKnown = false; }
                        else { isTeamKnown = true; }
                        break;
                    case "StatusContacts":
                        if (process.value <= 0) { isActorKnown = false; }
                        else { isActorKnown = true; }
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid process.outcome \"{0}\"", process.outcome.name));
                        break;
                }
            }
        }
        else { Debug.LogError("Invalid effectProcess (Null)"); }
    }

    /// <summary>
    /// changes connection security level for all connections leading from the node
    /// </summary>
    /// <param name="process"></param>
    public void ProcessConnectionEffect(EffectDataProcess process)
    {
        if (process != null)
        {
            bool isOngoingAndOK = false;
            bool isAtLeastOneOngoing = false;
            //loop all the connections leading from the node
            foreach (Connection connection in listOfConnections)
            {
                if (process.effectOngoing != null)
                {
                    //process Ongoing effect provided not a duplicate ongoingID
                    isOngoingAndOK = connection.AddOngoingEffect(process.effectOngoing);
                    if (isOngoingAndOK == true)
                    {
                        //update material to reflect any change
                        connection.SetConnectionMaterial(connection.SecurityLevel);
                        //set flag to true (only has to be true once for the node to get an ongoing effect)
                        isAtLeastOneOngoing = true;
                    }
                }
                else
                {
                    //single effect
                    switch (process.outcome.name)
                    {
                        case "ConnectionSecurity":
                            //changes security level and updates material
                            connection.ChangeSecurityLevel(process.value);
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid process.outcome \"{0}\"", process.outcome.name));
                            break;
                    }
                }
            }
            //need an Ongoing entry for Node (everything apart from text is ignored by node)
            if (isAtLeastOneOngoing == true)
            {                         
                //create an entry in the nodes listOfOngoingEffects
                AddOngoingEffect(process.effectOngoing);
            }
        }
        else
        { Debug.LogError("Invalid effectProcess (Null)"); }
    }


    /// <summary>
    /// //stats are reversed for Authority FOR DISPLAY ONLY (a stat of 0 for resistance is very bad but it shows as a stat of 3, very good, for the authority side)
    /// </summary>
    /// <returns></returns>
    private int[] GetStats()
    {
        int[] arrayOfStats;
        switch (GameManager.instance.sideScript.PlayerSide.name)
        {
            case "Resistance":
                arrayOfStats = new int[] { Stability, Support, Security };
                break;
            case "Authority":
                arrayOfStats = new int[] { 3 - Stability, 3 - Support, 3 - Security };
                break;
            default:
                arrayOfStats = new int[] { Stability, Support, Security };
                Debug.LogError(string.Format("Invalid side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                break;
        }

        return arrayOfStats;
    }

    //place methods above here
}
