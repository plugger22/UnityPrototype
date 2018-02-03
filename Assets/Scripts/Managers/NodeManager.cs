﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using modalAPI;
using System.Text;

/// <summary>
/// Handles all node related matters
/// </summary>
public class NodeManager : MonoBehaviour
{
    [Tooltip("% chance times actor.Ability of a primary node being active for an Actor")]
    public int nodePrimaryChance;                                   //% chance (times actor.Ability) of a primary node being active for an actor, halved for secondary, default 10%
    public int nodeActiveMinimum;                                   //minimum number of active nodes on a map for any actor type, default 3
    public Material[] arrayOfNodeTypes;

    [HideInInspector] public int nodeCounter = 0;                   //sequentially numbers nodes
    [HideInInspector] public int connCounter = 0;                   //sequentially numbers connections
    [HideInInspector] public int nodeHighlight = -1;                //nodeID of currently highlighted node, if any, otherwise -1
    [HideInInspector] public int nodePlayer = -1;                   //nodeID of player
    [HideInInspector] public int nodeCaptured = -1;                 //nodeID where player has been captured, -1 if not

    string colourDefault;
    string colourAlert;
    string colourHighlight;
    string colourResistance;
    string colourEffectBad;
    string colourEffectNeutral;
    string colourEffectGood;
    string colourError;
    string colourEnd;

    [SerializeField, HideInInspector]
    private int _nodeShowFlag = 0;                                   //true if a ShowNodes() is active, false otherwise
    [SerializeField, HideInInspector]
    private bool _nodeRedraw = false;                                //if true a node redraw is triggered in GameManager.Update

    //properties
    public int NodeShowFlag
    {
        get { return _nodeShowFlag; }
        set { _nodeShowFlag = value; Debug.Log(string.Format("NodeShowFlag -> {0}{1}", value, "\n"));}
    }

    public bool NodeRedraw
    {
        get { return _nodeRedraw; }
        set { _nodeRedraw = value; }
    }


    /// <summary>
    /// Self initialisation
    /// </summary>
    private void Awake()
    {
        //bounds checking
        nodePrimaryChance = nodePrimaryChance > 0 ? nodePrimaryChance : 10;
        nodeActiveMinimum = nodeActiveMinimum > 2 ? nodeActiveMinimum : 3;

    }

    public void Initialise()
    {
        SetNodeActorFlags();
        //register listener
        EventManager.instance.AddListener(EventType.NodeDisplay, OnEvent);
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
        EventManager.instance.AddListener(EventType.CreateMoveMenu, OnEvent);
        EventManager.instance.AddListener(EventType.MoveAction, OnEvent);
        EventManager.instance.AddListener(EventType.DiceReturn, OnEvent);
    }


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
            case EventType.NodeDisplay:
                NodeUI nodeUI = (NodeUI)Param;
                switch(nodeUI)
                {
                    case NodeUI.Reset:
                        ResetNodes();
                        break;
                    case NodeUI.Redraw:
                        RedrawNodes();
                        break;
                    case NodeUI.Move:
                    case NodeUI.ShowTargets:
                    case NodeUI.ShowSpiders:
                    case NodeUI.ShowTracers:
                    case NodeUI.NodeArc0:
                    case NodeUI.NodeArc1:
                    case NodeUI.NodeArc2:
                    case NodeUI.NodeArc3:
                    case NodeUI.NodeArc4:
                    case NodeUI.NodeArc5:
                    case NodeUI.NodeArc6:
                    case NodeUI.NodeArc7:
                    case NodeUI.NodeArc8:
                    case NodeUI.NodeArc9:
                        if (NodeShowFlag > 0)
                        { GameManager.instance.alertScript.CloseAlertUI(true); }
                        else { ShowNodes(nodeUI); }
                        break;

                    default:
                        Debug.LogError(string.Format("Invalid NodeUI param \"{0}\"{1}", Param.ToString(), "\n"));
                        break;
                }
                break;
            case EventType.CreateMoveMenu:
                CreateMoveMenu((int)Param);
                break;
            case EventType.MoveAction:
                ModalMoveDetails details = Param as ModalMoveDetails;
                ProcessPlayerMove(details);
                break;
            case EventType.DiceReturn:
                DiceReturnData data = Param as DiceReturnData;
                ProcessDiceMove(data);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Set colour palette for tooltip
    /// </summary>
    public void SetColours()
    {
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourHighlight = GameManager.instance.colourScript.GetColour(ColourType.nodeActive);
        colourResistance = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourEffectBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourEffectNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourEffectGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourError = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// deregister events
    /// </summary>
    public void OnDisable()
    {
        EventManager.instance.RemoveEvent(EventType.ShowTargetNodes);
    }

    /// <summary>
    /// toggles a node on/off as Highlighted, default OFF 
    /// </summary>
    /// <param name="nodeID">leave blank to switch off currently highlighted node</param>
    public void ToggleNodeHighlight(int highlightID = -1)
    {
        if (nodeHighlight != highlightID)
        {
            ResetNodes();
            nodeHighlight = highlightID;
            Debug.Log(string.Format("Highlighted nodeID {0}{1}", highlightID, "\n"));
        }
        else
        {
            ResetNodes();
            nodeHighlight = -1;
        }
    }

    /// <summary>
    /// Return a node Material
    /// </summary>
    /// <param name="nodeType"></param>
    /// <returns></returns>
    public Material GetNodeMaterial(NodeType nodeType)
    { return arrayOfNodeTypes[(int)nodeType]; }


    /// <summary>
    /// highlights all nodes depening on the enum NodeUI criteria
    /// </summary>
    public void ShowNodes(NodeUI nodeUI)
    {
        int data = -1;
        bool successFlag = true;
        bool nodeTypeFlag = false;
        string displayText = null;
        //set all nodes to default colour first
        ResetNodes();
        //set nodes depending on critera
        switch (nodeUI)
        {
            //show all nodes with Targets
            case NodeUI.ShowTargets:
                //change material for selected nodes (Live and Completed targets)
                List<Target> tempList = new List<Target>();
                tempList.AddRange(GameManager.instance.dataScript.GetTargetPool(Status.Live));
                tempList.AddRange(GameManager.instance.dataScript.GetTargetPool(Status.Completed));
                GameObject nodeObject = null;
                if (tempList.Count > 0)
                {
                    foreach (Target target in tempList)
                    {
                        nodeObject = GameManager.instance.dataScript.GetNodeObject(target.nodeID);
                        if (nodeObject != null)
                        {
                            Node nodeTemp = nodeObject.GetComponent<Node>();
                            Material nodeMaterial = GameManager.instance.nodeScript.GetNodeMaterial(NodeType.Active);
                            nodeTemp.SetMaterial(nodeMaterial);
                            displayText = string.Format("{0}{1}{2}{3} Target{4}{5} node{6}{7}", colourDefault, tempList.Count, colourEnd, colourHighlight, colourEnd,
                                colourDefault,  tempList.Count != 1 ? "s" : "", colourEnd);
                        }
                    }
                }
                else { displayText = string.Format("{0}{1}{2}", colourError, "No Targets present", colourEnd); }
                break;

            //show all viable move locations for player (nodes adjacent to current location)
            case NodeUI.Move:
                Node nodeRef = GameManager.instance.dataScript.GetNode(nodePlayer);
                if (nodeRef != null)
                {
                    List<Node> nodeList = nodeRef.GetNeighbouringNodes();
                    if (nodeList != null)
                    {
                        NodeArc nodeArc = GameManager.instance.dataScript.GetNodeArc(data);
                        if (nodeList.Count > 0)
                        {
                            foreach (Node node in nodeList)
                            {
                                if (node != null)
                                {
                                    Material nodeMaterial = GameManager.instance.nodeScript.GetNodeMaterial(NodeType.Active);
                                    node.SetMaterial(nodeMaterial);
                                    displayText = string.Format("{0}{1}{2} {3}valid Move Node{4}{5}", colourDefault, nodeList.Count, colourEnd,
                                        colourHighlight, nodeList.Count != 1 ? "s" : "", colourEnd);
                                }
                            }
                        }
                        else
                        {
                            if (nodeArc != null)
                            { displayText = string.Format("{0}There are no {1} nodes{2}", colourError, nodeArc.name, colourEnd); }
                        }
                    }
                    else { Debug.LogError(string.Format("Invalid nodeList (null) for NodeArcID {0}{1}", data, "\n")); }
                }
                else { Debug.LogError(string.Format("Invalid node (Null) for NodeID {0}", nodePlayer)); }
                break;

            //show all nodes containng a spider
            case NodeUI.ShowSpiders:
                Dictionary<int, Node> dictOfSpiderNodes = GameManager.instance.dataScript.GetAllNodes();
                if (dictOfSpiderNodes != null)
                {
                    int count = 0;
                    foreach(var node in dictOfSpiderNodes)
                    {
                        if (node.Value.isSpider == true)
                        {
                            Material nodeMaterial = GameManager.instance.nodeScript.GetNodeMaterial(NodeType.Active);
                            //visibility depends on side
                            switch (GameManager.instance.sideScript.PlayerSide)
                            {
                                case Side.Authority:
                                    node.Value.SetMaterial(nodeMaterial);
                                    count++;
                                    break;
                                case Side.Resistance:
                                    //resistance -> show only if known, eg. within tracer coverage
                                    if (node.Value.isSpiderKnown == true)
                                    {
                                        node.Value.SetMaterial(nodeMaterial);
                                        count++;
                                    }
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid side \"{0}\"", GameManager.instance.sideScript.PlayerSide));
                                    break;
                            }
                        }
                    }
                    if (count > 0)
                    {
                        displayText = string.Format("{0}{1}{2} {3}{4}{5} {6}node{7}{8}", colourDefault, count, colourEnd,
                            colourHighlight, "Spider", colourEnd, colourDefault, count != 1 ? "s" : "", colourEnd);
                    }
                    else
                    { displayText = string.Format("{0}There are no Spider nodes{1}", colourError, colourEnd); }
                }
                else { Debug.LogError("Invalid dictOfSpiderNodes (Null)"); }
                break;
            //show all nodes with a tracer or within one node radius of a tracer
            case NodeUI.ShowTracers:
                Dictionary<int, Node> dictOfTracerNodes = GameManager.instance.dataScript.GetAllNodes();
                if (dictOfTracerNodes != null)
                {
                    int count = 0;
                    foreach (var node in dictOfTracerNodes)
                    {
                        if (node.Value.isTracerActive == true)
                        {
                            Material nodeMaterial = GameManager.instance.nodeScript.GetNodeMaterial(NodeType.Active);
                            node.Value.SetMaterial(nodeMaterial);
                            //count number of tracers, not active tracer nodes
                            if (node.Value.isTracer == true)
                            { count++; }
                        }
                    }
                    if (count > 0)
                    {
                        displayText = string.Format("{0}{1}{2} {3}{4}{5} {6}node{7}{8}", colourDefault, count, colourEnd,
                            colourHighlight, "Tracer", colourEnd, colourDefault, count != 1 ? "s" : "", colourEnd);
                    }
                    else
                    { displayText = string.Format("{0}There are no Tracer nodes{1}", colourError, colourEnd); }
                }
                else { Debug.LogError("Invalid dictOfTracerNodes (Null)"); }
                break;

            //show specific NodeArcTypes
            case NodeUI.NodeArc0: data = 0; nodeTypeFlag = true; break;
            case NodeUI.NodeArc1: data = 1; nodeTypeFlag = true; break;
            case NodeUI.NodeArc2: data = 2; nodeTypeFlag = true; break;
            case NodeUI.NodeArc3: data = 3; nodeTypeFlag = true; break;
            case NodeUI.NodeArc4: data = 4; nodeTypeFlag = true; break;
            case NodeUI.NodeArc5: data = 5; nodeTypeFlag = true; break;
            case NodeUI.NodeArc6: data = 6; nodeTypeFlag = true; break;
            case NodeUI.NodeArc7: data = 7; nodeTypeFlag = true; break;
            case NodeUI.NodeArc8: data = 8; nodeTypeFlag = true; break;
            case NodeUI.NodeArc9: data = 9; nodeTypeFlag = true; break;
            default:
                Debug.LogError(string.Format("Invalid NodeUI parameter \"{0}\"{1}", nodeUI, "\n"));
                successFlag = false;
                break;
        }
        //display specific nodeArcType
        if (nodeTypeFlag == true)
        {
            List<Node> nodeList = GameManager.instance.dataScript.GetListOfNodesByType(data);
            if (nodeList != null)
            {
                NodeArc nodeArc = GameManager.instance.dataScript.GetNodeArc(data);
                if (nodeList.Count > 0)
                {
                    foreach (Node node in nodeList)
                    {
                        if (node != null)
                        {
                            Material nodeMaterial = GameManager.instance.nodeScript.GetNodeMaterial(NodeType.Active);
                            node.SetMaterial(nodeMaterial);
                        }
                    }
                    displayText = string.Format("{0}{1}{2} {3}{4}{5} {6}node{7}{8}", colourDefault, nodeList.Count, colourEnd, 
                    colourHighlight, nodeArc.name, colourEnd, colourDefault, nodeList.Count != 1 ? "s" : "", colourEnd);
                }
                else
                {
                    if (nodeArc != null)
                    { displayText = string.Format("{0}There are no {1} nodes{2}", colourError, nodeArc.name, colourEnd); }
                }
            }
            else { Debug.LogError(string.Format("Invalid nodeList (null) for NodeArcID {0}{1}", data, "\n")); }

        }
        //successful event housekeeping
        if (successFlag == true)
        {
            //set show node flag on success
            NodeShowFlag = 1;
            //active AlertUI
            if (string.IsNullOrEmpty(displayText) != true)
            { GameManager.instance.alertScript.SetAlertUI(displayText); }
        }
    }

    /// <summary>
    /// Show all active nodes for a particular actor. Use actor.slotID (0 to numOfActors)
    /// </summary>
    /// <param name="slotID"></param>
    public void ShowActiveNodes(int slotID)
    {
        Debug.Assert(slotID > -1 && slotID < GameManager.instance.actorScript.numOfOnMapActors, "Invalid slotID");
        //set all nodes to default colour first
        ResetNodes();
        //change material for selected nodes
        List<GameObject> tempList = GameManager.instance.dataScript.GetListOfActorNodes(slotID);
        foreach (GameObject obj in tempList)
        {
            Node nodeTemp = obj.GetComponent<Node>();
            Material nodeMaterial = GameManager.instance.nodeScript.GetNodeMaterial(NodeType.Active);
            nodeTemp.SetMaterial(nodeMaterial);
        }
        //Get Actor
        Actor actor = GameManager.instance.dataScript.GetCurrentActor(slotID, GameManager.instance.sideScript.PlayerSide);
        string displayText;
        string minionTitle;
        //work out minion's appropriate title
        if (GameManager.instance.sideScript.PlayerSide == Side.Authority)
        { minionTitle = string.Format("{0} of ", (AuthorityActor)GameManager.instance.turnScript.metaLevel); }
        else { minionTitle = "Rebel "; }
        if (actor != null)
        {
            displayText = string.Format("{0}\"{1}\"{2} {3}{4}{5}{6}{7}{8} {9}{10} node{11}{12}", colourHighlight, actor.actorName, colourEnd, 
                colourDefault, minionTitle, colourEnd,
                colourHighlight, actor.arc.name, colourEnd, 
                colourDefault, tempList.Count, tempList.Count != 1 ? "s" : "", colourEnd);
            GameManager.instance.alertScript.SetAlertUI(displayText);
            NodeShowFlag = 1;
        }
    }


    /// <summary>
    /// Redraw any nodes. Show highlighted node, unless it's a non-normal node for the current redraw
    /// </summary>
    public void RedrawNodes()
    {
        Renderer nodeRenderer;
        Dictionary<int, Node> tempDict = GameManager.instance.dataScript.GetAllNodes();
        if (tempDict != null)
        {
            //loop all nodes & assign current materials to their renderers (changes colour on screen)
            foreach (var node in tempDict)
            {
                nodeRenderer = node.Value.GetComponent<Renderer>();
                nodeRenderer.material = node.Value._Material;
            }
            //highlighted node
            if (nodeHighlight > -1)
            {
                Node node = GameManager.instance.dataScript.GetNode(nodeHighlight);
                if (node != null)
                {
                    //only do so if it's a normal node, otherwise ignore
                    if (node.GetMaterial() == GetNodeMaterial(NodeType.Normal))
                    {
                        nodeRenderer = node.GetComponent<Renderer>();
                        nodeRenderer.material = GetNodeMaterial(NodeType.Highlight);
                    }
                }
                else { Debug.LogError("Invalid Node (null) returned from dictOfNodes"); }
            }
            //player's current node
            if (nodePlayer > -1)
            {
                Node node = GameManager.instance.dataScript.GetNode(nodePlayer);
                if (node != null)
                {
                    //only do so if it's a normal node, otherwise ignore
                    if (node.GetMaterial() == GetNodeMaterial(NodeType.Normal))
                    {
                        nodeRenderer = node.GetComponent<Renderer>();
                        nodeRenderer.material = GetNodeMaterial(NodeType.Player);
                    }
                }
            }
            //reset flag to prevent constant redraws
            NodeRedraw = false;
            //switch off flag to turn off Alert UI text next time around
            if (NodeShowFlag > 1)
            { GameManager.instance.alertScript.CloseAlertUI(); }
            if (NodeShowFlag > 0) { NodeShowFlag++; }
            
        }
        else { Debug.LogError("Invalid dictOfNodes (Null) returned from dataManager in RedrawNodes"); }
    }

    /// <summary>
    /// Sets the material colour of all nodes to the default (doesn't change on screen, just sets them up). Call before making any changes to node colours
    /// </summary>
    public void ResetNodes()
    {
        //get default material
        Material nodeMaterial = GetNodeMaterial(NodeType.Normal);
        //loop and assign
        Dictionary<int, Node> tempDict = GameManager.instance.dataScript.GetAllNodes();
        if (tempDict != null)
        {
            foreach (var node in tempDict)
            { node.Value.SetMaterial(nodeMaterial); }
            //trigger an automatic redraw
            NodeRedraw = true;
        }
        else { Debug.LogError("Invalid dictOfNodes (Null) returned from dataManager in ResetNodes"); }
    }

    /// <summary>
    /// Put an action Menu together at the selected node that has 'Move' and 'Cancel' options
    /// </summary>
    /// <param name="nodeID"></param>
    private void CreateMoveMenu(int nodeID)
    {
        Debug.Log("CreateMoveMenu");

        List<EventButtonDetails> tempList = new List<EventButtonDetails>();

        //Get Node
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        if (node != null)
        {
            string moveHeader = string.Format("{0}\"{1}\", {2}{3}{4}{5}, ID {6}{7}", colourResistance, node.nodeName, colourEnd, "\n",
                colourDefault, node.Arc.name.ToUpper(), node.nodeID, colourEnd);
            string moveMain = "UNKNOWN";
            int adjustInvisibility = 0;
            //Get Connection (between new node and Player's current location)
            Connection connection = node.GetConnection(nodePlayer);
            if (connection != null)
            {
                ConnectionType connSecType = connection.GetSecurity();
                int secLevel = (int)connSecType;
                if (secLevel == 0) { secLevel = 4; } //need to do this to get the default colour as 0 is regarded as terrible normally
                moveMain = string.Format("Connection Security{0}{1}{2}{3}", "\n", GameManager.instance.colourScript.GetValueColour(secLevel), connSecType, colourEnd);
                //
                // - - - Gear (Movement) only if connection has a security level
                //
                if (secLevel < 4)
                {
                    List<int> listOfGear = GameManager.instance.playerScript.GetListOfGear();
                    if (listOfGear.Count > 0)
                    {
                        for (int i = 0; i < listOfGear.Count; i++)
                        {
                            StringBuilder builderDetail = new StringBuilder();
                            Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
                            if (gear != null)
                            {
                                if (gear.type == GearType.Movement)
                                {
                                    //
                                    // - - - Create Gear Button (one for each item of relevant gear in Player's possesion)
                                    //
                                    ModalMoveDetails moveGearDetails = new ModalMoveDetails();
                                    //gear effect on security
                                    if (gear.data <= secLevel)
                                    {
                                        //gear handles security level
                                        adjustInvisibility = 0;
                                        builderDetail.Append(string.Format("{0}No risk of being spotted{1}", colourEffectGood, colourEnd));
                                    }
                                    else
                                    {
                                        //gear falls short of security level
                                        adjustInvisibility = -1;
                                        if (GameManager.instance.playerScript.invisibility <= 1)
                                        {
                                            //invisibility will be zero, or less, if move. Immediate notification
                                            builderDetail.Append(string.Format("{0}Invisibility -1{1}Authority will know IMMEDIATELY{2}", colourEffectBad, "\n",
                                              colourEnd));
                                            moveGearDetails.ai_Delay = 0;
                                        }
                                        else
                                        {
                                            //invisibility reduces, still above zero
                                            moveGearDetails.ai_Delay = 4 - Mathf.Abs(gear.data - secLevel);
                                            builderDetail.Append(string.Format("{0}Invisibility -1{1}Authorities will know in {2} turn{3}{4}", colourEffectBad, "\n",
                                              moveGearDetails.ai_Delay, moveGearDetails.ai_Delay != 1 ? "s" : "", colourEnd));
                                            
                                        }
                                    }
                                    //add gear chance of compromise
                                    builderDetail.Append(string.Format("{0}{1}Gear has a {2}% chance of being compromised{3}", "\n", colourAlert, 
                                        GameManager.instance.gearScript.GetChanceOfCompromise(gear.gearID), colourEnd));

                                    //Move details
                                    moveGearDetails.nodeID = nodeID;
                                    moveGearDetails.connectionID = connection.connID;
                                    moveGearDetails.changeInvisibility = adjustInvisibility;
                                    moveGearDetails.gearID = gear.gearID;
                                    //button target details (red for High security to match red connection security colour on map)
                                    string colourGearLevel = colourEffectNeutral;
                                    if (gear.data == 3) { colourGearLevel = colourEffectGood; }
                                    else if (gear.data == 1) { colourGearLevel = colourEffectBad; }
                                    EventButtonDetails eventMoveDetails = new EventButtonDetails()
                                    {
                                        buttonTitle = string.Format("{0} Move", gear.name),
                                        buttonTooltipHeader = string.Format("Move using{0}{1}{2}{3}{4}{5}{6}{7}", "\n", colourEffectNeutral, gear.name, colourEnd,
                                        "\n", colourGearLevel, (ConnectionType)gear.data, colourEnd),
                                        buttonTooltipMain = moveMain,
                                        buttonTooltipDetail = builderDetail.ToString(),
                                        //use a Lambda to pass arguments to the action
                                        action = () => { EventManager.instance.PostNotification(EventType.MoveAction, this, moveGearDetails); }
                                    };
                                    tempList.Add(eventMoveDetails);
                                }
                            }
                        }
                    }
                }
                //
                // - - - Move Button (No gear used)
                //
                ModalMoveDetails moveDetails = new ModalMoveDetails();
                //security conseqences (no gear)
                string moveDetail = "UNKNOWN";
                if (secLevel < 4)
                {
                    //player loses one level of invisibility each time they traverse a security rated connection
                    adjustInvisibility = -1;
                    if (GameManager.instance.playerScript.invisibility <= 1)
                    {
                        //invisibility will be zero, or less, if move. Immediate notification
                        moveDetail = string.Format("{0}Invisibility -1{1}Authority will know IMMEDIATELY{2}", colourEffectBad, "\n",
                          colourEnd);
                        moveDetails.ai_Delay = 0;
                    }
                    else
                    {
                        //invisibility reduces, still above zero
                        moveDetails.ai_Delay = secLevel;
                        moveDetail = string.Format("{0}Invisibility -1{1}Authorities will know in {2} turn{3}{4}", colourEffectBad, "\n",
                          moveDetails.ai_Delay, moveDetails.ai_Delay != 1 ? "s" : "", colourEnd);
                    }
                }
                else { moveDetail = string.Format("{0}No risk of being spotted{1}", colourEffectGood, colourEnd); }

                //Move details
                moveDetails.nodeID = nodeID;
                moveDetails.connectionID = connection.connID;
                moveDetails.changeInvisibility = adjustInvisibility;
                moveDetails.gearID = -1;
                //button target details
                EventButtonDetails eventDetails = new EventButtonDetails()
                {
                    buttonTitle = "Move",
                    buttonTooltipHeader = string.Format("{0}Move (no gear){1}", colourEffectNeutral, colourEnd),
                    buttonTooltipMain = moveMain,
                    buttonTooltipDetail = moveDetail,
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.MoveAction, this, moveDetails); }
                };
                tempList.Add(eventDetails);
                //
                // - - - Cancel
                //
                //Cancel button is added last
                EventButtonDetails cancelDetails = null;
                //necessary to prevent color tags triggering the bottom divider in TooltipGeneric
                cancelDetails = new EventButtonDetails()
                {
                    buttonTitle = "CANCEL",
                    buttonTooltipHeader = moveHeader,
                    buttonTooltipMain = "You'd like to think about it",
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this); }
                };
                //add Cancel button to list
                tempList.Add(cancelDetails);
            }
            else { Debug.LogError("Invalid Connection (Null)"); }
        }
        else { Debug.LogError(string.Format("Invalid Node (null), ID {0}", nodeID)); }
        //
        // - - - Action Menu
        //
        ModalPanelDetails details = new ModalPanelDetails()
        {
            nodeID = nodeID,
            nodeName = node.nodeName,
            nodeDetails = string.Format("{0} ID {1}", node.Arc.name, node.nodeID),
            nodePos = node.transform.position,
            listOfButtonDetails = tempList
        };
        //activate menu
        GameManager.instance.actionMenuScript.SetActionMenu(details);
    }

    /// <summary>
    /// Move player to the specified node
    /// </summary>
    /// <param name="nodeID"></param>
    private void ProcessPlayerMove(ModalMoveDetails moveDetails)
    {
        if (moveDetails != null)
        {
            Node node = GameManager.instance.dataScript.GetNode(moveDetails.nodeID);
            int nodeOriginal = nodePlayer;
            if (node != null)
            {
                //update Player node
                nodePlayer = moveDetails.nodeID;
                //update move list
                node.SetMoveNodes();
                string destination = string.Format("\"{0}\", {1}, ID {2}", node.nodeName, node.Arc.name.ToUpper(), node.nodeID);
                StringBuilder builder = new StringBuilder();
                builder.Append(string.Format("{0}{1}", destination, "\n"));
                //message
                string text = string.Format("Player has moved to {0}", destination);
                Message message = GameManager.instance.messageScript.PlayerMove(text, node.nodeID);
                if (message != null) { GameManager.instance.dataScript.AddMessage(message); }
                //
                // - - - Invisibility - - -
                //
                if (moveDetails.changeInvisibility != 0)
                {
                    //update player invisibility
                    int invisibility = GameManager.instance.playerScript.invisibility;
                    invisibility += moveDetails.changeInvisibility;
                    invisibility = Mathf.Max(0, invisibility);
                    GameManager.instance.playerScript.invisibility = invisibility;
                    //display
                    builder.AppendLine();
                    builder.Append(string.Format("{0}Invisibility {1}{2}{3}", colourEffectBad, moveDetails.changeInvisibility > 0 ? "+" : "",
                        moveDetails.changeInvisibility, colourEnd));
                    //message
                    Connection connection = GameManager.instance.dataScript.GetConnection(moveDetails.connectionID);
                    if (connection != null)
                    {
                        string textAI = string.Format("Player spotted moving to \"{0}\", {1}, ID {2}",
                            node.nodeName, node.Arc.name.ToUpper(), moveDetails.nodeID);
                        Message messageAI = GameManager.instance.messageScript.AISpotMove(textAI, moveDetails.nodeID, moveDetails.connectionID, moveDetails.ai_Delay);
                        if (messageAI != null) { GameManager.instance.dataScript.AddMessage(messageAI); }
                    }
                    else { Debug.LogError(string.Format("Invalid connection (Null) for connectionID {0}", moveDetails.connectionID)); }
                }
                else
                {
                    builder.AppendLine();
                    builder.Append(string.Format("{0}Player not spotted{1}", colourEffectGood, colourEnd));
                }
                //
                // - - - Gear - - -
                //
                if (moveDetails.gearID > -1)
                {
                    Gear gear = GameManager.instance.dataScript.GetGear(moveDetails.gearID);
                    int renownCost = GameManager.instance.playerScript.renownCostGear;
                    if (gear != null)
                    {
                        //chance of Gear being Compromised
                        ModalDiceDetails diceDetails = new ModalDiceDetails();
                        diceDetails.chance = GameManager.instance.gearScript.GetChanceOfCompromise(gear.gearID);
                        diceDetails.renownCost = renownCost;
                        diceDetails.topText = string.Format("{0}{1}{2} used to move{3}{4}{5}% Chance{6} of it being compromised and lost", colourEffectNeutral,
                            gear.name, colourEnd, "\n", colourEffectBad, diceDetails.chance, colourEnd);
                        if (GameManager.instance.playerScript.Renown >= renownCost) { diceDetails.isEnoughRenown = true; }
                        else { diceDetails.isEnoughRenown = false; }
                        //as gear involved data will be needed to be passed through from this method to ProcessMoveOutcome via ProcessDiveMove
                        PassThroughDiceData passThroughData = new PassThroughDiceData();
                        passThroughData.nodeID = node.nodeID;
                        passThroughData.gearID = gear.gearID;
                        passThroughData.renownCost = renownCost;
                        passThroughData.text = builder.ToString();
                        diceDetails.passData = passThroughData;
                        //go straight to an outcome dialogue if not enough renown and option set to ignore dice roller
                        if (diceDetails.isEnoughRenown == false && GameManager.instance.optionScript.autoGearResolution == true)
                        { ProcessAutoDiceMove(diceDetails); }
                        //roll dice
                        else
                        { EventManager.instance.PostNotification(EventType.OpenDiceUI, this, diceDetails); }
                    }
                    else { Debug.LogError(string.Format("Invalid Gear (Null) for gearID {0}", moveDetails.gearID)); }
                }
                else
                {
                    //No gear involved, move straight to outcome, otherwise skip outcome if connection has no security as unnecessary
                    if (moveDetails.changeInvisibility != 0)
                    { ProcessMoveOutcome(node, builder.ToString()); }
                    else { EventManager.instance.PostNotification(EventType.UseAction, this); }
                }
            }
            else
            { Debug.LogError(string.Format("Invalid node (Null) for nodeID {0}", moveDetails.nodeID)); }
        }
        else
        { Debug.LogError("Invalid ModalMoveDetails (Null)"); }
    }

    /// <summary>
    /// return event from dice roller (Move -> gear compromised or not)
    /// </summary>
    /// <param name="data"></param>
    private void ProcessDiceMove(DiceReturnData data)
    {
        //no need to check for nulls for node and gear as already checked in ProcessPlayerMove (calling method)
        Node node = GameManager.instance.dataScript.GetNode(data.passData.nodeID);
        Gear gear = GameManager.instance.dataScript.GetGear(data.passData.gearID);
        StringBuilder builder = new StringBuilder();
        builder.Append(data.passData.text);
        //process gear and renown outcome
        if (data != null)
        {
            switch (data.outcome)
            {
                case DiceOutcome.Ignore:
                    //bypasses roller, accepts result, no renown intervention
                    if (data.isSuccess == true)
                    {
                        GearUsed(gear, node);
                    }
                    else
                    {
                        //bad result stands -> gear compromised
                        builder.Append(GearUsedAndCompromised(gear, node));
                    }
                    break;
                case DiceOutcome.Auto:
                    //bypass roller, auto spends renown to avert a bad result
                    if (data.isSuccess == true)
                    {
                        GearUsed(gear, node);
                    }
                    else
                    {
                        //bad result  -> gear compromised but renown auto spent to negate
                        builder.Append(RenownUsed(gear, node, data.passData.renownCost));
                        GearUsed(gear, node);
                    }
                    break;
                case DiceOutcome.Roll:
                    //rolls dice, if bad result has option to spend renown to negate
                    if (data.isSuccess == true)
                    {
                        GearUsed(gear, node);
                    }
                    //Fail result
                    else
                    {
                        if (data.isRenown == true)
                        {
                            //player spent renown to negate a bad result
                            GearUsed(gear, node);
                            builder.Append(RenownUsed(gear, node, data.passData.renownCost));
                        }
                        else
                        {
                            //bad result stands -> gear compromised
                            builder.Append(GearUsedAndCompromised(gear, node));
                        }
                    }
                    break;
                default:
                    Debug.LogError(string.Format("Invalid returnData.outcome \"{0}\"", data.outcome));
                    break;
            }
        }
        else { Debug.LogError("Invalid DiceReturnData (Null)"); }

        //all done, go to outcome
        ProcessMoveOutcome(node, builder.ToString());
        
    }

    /// <summary>
    /// ProcessPlayerMove -> used when gear involved but not enough renown to mitigate a bad result (optionAutoGear = true)
    /// </summary>
    /// <param name="details"></param>
    private void ProcessAutoDiceMove(ModalDiceDetails details)
    {
        string gearResult = "";
        //Roll
        if (Random.Range(0, 100) > details.chance)
        {
            Gear gear = GameManager.instance.dataScript.GetGear(details.passData.gearID);
            if (gear != null)
            {
                Node node = GameManager.instance.dataScript.GetNode(details.passData.nodeID);
                if (node != null)
                {
                    //remove gear and return string for outcome dialogue
                    gearResult = GearUsedAndCompromised(gear, node);
                }
                else { Debug.LogError(string.Format("Invalid node (Null) for nodeID {0}", details.passData.nodeID)); }
            }
            else { Debug.LogError(string.Format("Invalid Gear (Null) for gearID {0}", details.passData.gearID)); }
        }
        // Outcome
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        outcomeDetails.textTop = "Player has moved";
        if (gearResult.Length > 0)
        { outcomeDetails.textBottom = string.Format("{0}{1}", details.passData.text, gearResult); }
        else { outcomeDetails.textBottom = details.passData.text; }
        outcomeDetails.sprite = GameManager.instance.outcomeScript.errorSprite;
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
    }

    /// <summary>
    /// ProcessPlayerMove -> ProcessMoveOutcome. Node checked for Null in calling procedure
    /// </summary>
    private void ProcessMoveOutcome(Node node, string textBottom)
    {
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        //Erasure team picks up player immediately if invisibility 0
        AIDetails aiDetails = GameManager.instance.captureScript.CheckCaptured(node.nodeID, 999);
        if (aiDetails != null)
        {
            //Player captured!
            aiDetails.effects = string.Format("{0}The move went bad{1}", colourEffectNeutral, colourEnd);
            EventManager.instance.PostNotification(EventType.Capture, this, aiDetails);
        }
        //Normal Move  Outcome
        else
        {
            outcomeDetails.textTop = "Player has moved";
            outcomeDetails.textBottom = textBottom;
            outcomeDetails.sprite = GameManager.instance.outcomeScript.errorSprite;
            outcomeDetails.isAction = true;
            outcomeDetails.side = Side.Resistance;
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
        }
    }




    /// <summary>
    /// submethod to handle gear comprised for ProcessPlayerMove (node and Gear not tested for null as already checked in calling method)
    /// </summary>
    /// <param name="gear"></param>
    /// <returns></returns>
    private string GearUsedAndCompromised(Gear gear, Node node)
    {
        //remove gear from inventory
        GameManager.instance.playerScript.RemoveGear(gear.gearID);
        //message -> gear compromised
        string textMsg = string.Format("{0}, ID {1} has been comprised while moving", gear.name, gear.gearID);
        Message messageGear = GameManager.instance.messageScript.GearCompromised(textMsg, node.nodeID, gear.gearID);
        if (messageGear != null) { GameManager.instance.dataScript.AddMessage(messageGear); }
        //return text string for builder
        return string.Format("{0}{1}{2}{3} has been compromised!{4}", "\n", "\n", colourEffectBad, gear.name, colourEnd);
    }

    /// <summary>
    /// submethod to handle gear being used but NOT compromised (node and Gear are checked for null by the calling method)
    /// </summary>
    /// <param name="gear"></param>
    /// <param name="node"></param>
    private void GearUsed(Gear gear, Node node)
    {
        //message
        string textMsg = string.Format("{0}, ID {1} has been used while moving", gear.name, gear.gearID);
        Message messageGear = GameManager.instance.messageScript.GearUsed(textMsg, node.nodeID, gear.gearID);
        if (messageGear != null) { GameManager.instance.dataScript.AddMessage(messageGear); }
    }

    /// <summary>
    /// subMethod to handle admin for Player renown expenditure
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    private string RenownUsed(Gear gear, Node node, int amount)
    {
        //update player renown
        GameManager.instance.playerScript.Renown -= amount;
        //message
        string textMsg = string.Format("{0}, ID {1} has been compromised. Saved by using {2} Renown.", gear.name, gear.gearID, amount);
        Message messageRenown = GameManager.instance.messageScript.RenownUsedPlayer(textMsg, node.nodeID, gear.gearID);
        if (messageRenown != null) { GameManager.instance.dataScript.AddMessage(messageRenown); }
        //return text string for builder
        return string.Format("{0}{1}{2}Gear saved, Renown -{3}{4}", "\n", "\n", colourEffectBad, amount, colourEnd);
    }

    /// <summary>
    /// Sets the node.isActor flag (true if any actor has a connection at node). Run everytime an actor changes status to keep flags up to date.
    /// </summary>
    public void SetNodeActorFlags()
    {
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetAllNodes();
        if (dictOfNodes != null)
        {
            //set all to false
            foreach (var node in dictOfNodes)
            { node.Value.isActor = false; }
            //loop Resistance actors
            for (int slotID = 0; slotID < GameManager.instance.actorScript.numOfOnMapActors; slotID++)
            {
                Actor actor = GameManager.instance.dataScript.GetCurrentActor(slotID, Side.Resistance);
                if (actor != null)
                {
                    //only consider actor if Active
                    if (actor.Status == ActorStatus.Active)
                    {
                        List<GameObject> listOfNodes = GameManager.instance.dataScript.GetListOfActorNodes(slotID);
                        if (listOfNodes != null)
                        {
                            //loop nodes where actor has a connection
                            for (int i = 0; i < listOfNodes.Count; i++)
                            {
                                //set flag to true
                                Node node = listOfNodes[i].GetComponent<Node>();
                                if (node != null)
                                { node.isActor = true; }
                                else { Debug.LogError(string.Format("Invalid node (Null) for slotID {0}", slotID)); }
                            }

                        }
                        else { Debug.LogError(string.Format("Invalid listOfNodes (Null) for slotID {0}", slotID)); }
                    }
                }
                else { Debug.LogError(string.Format("Invalid Actor (null) for slotID {0}", slotID)); }
            }
        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
    }

    //place new methods above here
}
