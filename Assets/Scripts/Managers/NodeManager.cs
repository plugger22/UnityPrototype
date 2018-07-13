using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using gameAPI;
using modalAPI;
using packageAPI;
using System.Text;

/// <summary>
/// Handles all node related matters
/// </summary>
public class NodeManager : MonoBehaviour
{
    [Tooltip("Node Colour Types")]
    public Material[] arrayOfNodeMaterials;

    [Tooltip("% chance times actor.Ability of a primary node being active for an Actor, halved for secondary node")]
    [Range(10,40)] public int nodePrimaryChance = 20;                                
    [Tooltip("Minimum number of active nodes on a map for any actor type")]
    [Range(0,4)] public int nodeActiveMinimum = 3;
    [Tooltip("The base factor used for calculating ('factor - (gear - seclvl [High is 3, Low is 1])') the delay in notifying the Authority player that move activity has occurred ")]
    [Range(0,10)]public int moveInvisibilityDelay = 4;

    [Header("Spiders and Tracers")]
    [Tooltip("The standard time delay, in turns, before Authority notification for any node activity that results in a loss of invisibility with no spider present")]
    [Range(0, 10)] public int nodeNoSpiderDelay = 2;
    [Tooltip("The standard time delay before Authority notification for any node activity that results in a loss of invisibility with a spider present. Make sure that this is less than the NoSpider delay")]
    [Range(0, 10)] public int nodeYesSpiderDelay = 1;
    [Tooltip("The amount of turns that a Spider or Tracer stay onMap for, once placed, before being automatically removed")]
    [Range(0, 10)] public int observerTimer = 3;

    [Header("Datapoints")]
    [Tooltip("Maximum value of a node datapoint")]
    [Range(2,4)] public int maxNodeValue = 3;
    [Tooltip("Minimum value of a node datapoint")]
    [Range(2, 4)] public int minNodeValue = 0;

    [Header("Crisis")]
    [Tooltip("Base % chance of a node with any datapoint in the danger zone turning into a crisis")]
    [Range(0, 100)] public int crisisBaseChance = 50;
    [Tooltip("Number of turns to set Node.cs -> crisisTimer to (How long the crisis lasts before going critical)")]
    [Range(1, 10)] public int crisisNodeTimer = 3;
    [Tooltip("Number of turns (minimum) between crisis at a particular node. Crisis can't happen at a node if this is > 0")]
    [Range(1, 10)] public int crisisWaitTimer = 3;
    [Tooltip("City Loyalty is lowered by this amount if a node crisis goes critical (Node.cs -> crisisTimer reaches zero)")]
    [Range(1, 3)] public int crisisCityLoyalty = 1;
    [Tooltip("Level, at or below, where Node Security reaches the danger point")]
    [Range(0, 3)] public int crisisSecurity = 0;
    [Tooltip("Level, at or below, where Node Stability reaches the danger point")]
    [Range(0, 3)] public int crisisStability = 0;
    [Tooltip("Level, at or Above, where Node support (for resistance) reaches the danger point")]
    [Range(0, 3)] public int crisisSupport = 3;

    [HideInInspector] public int nodeCounter = 0;                   //sequentially numbers nodes
    [HideInInspector] public int connCounter = 0;                   //sequentially numbers connections
    [HideInInspector] public int nodeHighlight = -1;                //nodeID of currently highlighted node, if any, otherwise -1
    [HideInInspector] public int nodePlayer = -1;                   //nodeID of player
    [HideInInspector] public int nodeCaptured = -1;                 //nodeID where player has been captured, -1 if not

    private bool isFlashOn = false;                                 //used for flashing Node coroutine
    private bool showPlayerNode = true;         
    private Coroutine myCoroutine;

    //fast access
    [HideInInspector] public EffectOutcome outcomeNodeSecurity;
    [HideInInspector] public EffectOutcome outcomeNodeStability;
    [HideInInspector] public EffectOutcome outcomeNodeSupport;
    [HideInInspector] public EffectOutcome outcomeStatusSpiders;
    [HideInInspector] public EffectOutcome outcomeStatusTracers;
    [HideInInspector] public EffectOutcome outcomeStatusContacts;
    [HideInInspector] public EffectOutcome outcomeStatusTeams;
    //gear node actions
    private Action actionKinetic;
    private Action actionHacking;
    private Action actionPersuasion;
    //activity
    [HideInInspector] public ActivityUI activityState;
    //materials
    private Material materialNormal;
    private Material materialHighlight;
    private Material materialActive;
    private Material materialPlayer;
    //flash
    private float flashNodeTime;

    //colours
    string colourDefault;
    string colourNormal;
    string colourAlert;
    string colourHighlight;
    string colourResistance;
    string colourBad;
    string colourNeutral;
    string colourGood;
    string colourError;
    string colourInvalid;
    string colourCancel;
    string colourEnd;

    [SerializeField, HideInInspector]
    private int _nodeShowFlag = 0;                                   //true if a ShowNodes() is active, false otherwise
    [SerializeField, HideInInspector]
    private bool _nodeRedraw = false;                                //if true a node redraw is triggered in GameManager.Update

    //properties
    public int NodeShowFlag
    {
        get { return _nodeShowFlag; }
        set { _nodeShowFlag = value; /*Debug.Log(string.Format("NodeShowFlag -> {0}{1}", value, "\n"));*/ }
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
        //find specific SO's and assign to outcome fields
        string path;
        var outcomeGUID = AssetDatabase.FindAssets("t:EffectOutcome");
        foreach (var guid in outcomeGUID)
        {
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            EffectOutcome outcomeObject = (EffectOutcome)AssetDatabase.LoadAssetAtPath(path, typeof(EffectOutcome));
            if (outcomeObject != null)
            {
                //pick out and assign the ones required for node fast acess, ignore the rest
                switch (outcomeObject.name)
                {
                    case "NodeSecurity":
                        outcomeNodeSecurity = outcomeObject;
                        break;
                    case "NodeStability":
                        outcomeNodeStability = outcomeObject;
                        break;
                    case "NodeSupport":
                        outcomeNodeSupport = outcomeObject;
                        break;
                    case "StatusSpiders":
                        outcomeStatusSpiders = outcomeObject;
                        break;
                    case "StatusTracers":
                        outcomeStatusTracers = outcomeObject;
                        break;
                    case "StatusContacts":
                        outcomeStatusContacts = outcomeObject;
                        break;
                    case "StatusTeams":
                        outcomeStatusTeams = outcomeObject;
                        break;
                }
            }
        }
        //check all found and assigned
        if (outcomeNodeSecurity == null) { Debug.LogError("Invalid outcomeNodeSecurity (Null)"); }
        if (outcomeNodeStability == null) { Debug.LogError("Invalid outcomeNodeStability (Null)"); }
        if (outcomeNodeSupport == null) { Debug.LogError("Invalid outcomeNodeSupport (Null)"); }
        if (outcomeStatusSpiders == null) { Debug.LogError("Invalid outcomeStatusSpiders (Null)"); }
        if (outcomeStatusTracers == null) { Debug.LogError("Invalid outcomeStatusTracers (Null)"); }
        if (outcomeStatusContacts == null) { Debug.LogError("Invalid outcomeStatusContacts (Null)"); }
        if (outcomeStatusTeams == null) { Debug.LogError("Invalid outcomeStatusTeams (Null)"); }
        //gear node Action Quick Reference -> Kinetic
        int actionID = GameManager.instance.dataScript.GetActionID("gearKinetic");
        if (actionID > -1)
        {
            actionKinetic = GameManager.instance.dataScript.GetAction(actionID);
            if (actionKinetic == null) { Debug.LogError("Invalid actionKinetic (Null)"); }
        }
        else { Debug.LogError("Invalid gearKinetic actionID (not found)"); }
        //gear node Action Quick Reference -> Hacking
        actionID = GameManager.instance.dataScript.GetActionID("gearHacking");
        if (actionID > -1)
        {
            actionHacking = GameManager.instance.dataScript.GetAction(actionID);
            if (actionHacking == null) { Debug.LogError("Invalid actionHacking (Null)"); }
        }
        else { Debug.LogError("Invalid gearHacking actionID (not found)"); }
        //gear node Action Quick Reference -> Persuasion
        actionID = GameManager.instance.dataScript.GetActionID("gearPersuasion");
        if (actionID > -1)
        {
            actionPersuasion = GameManager.instance.dataScript.GetAction(actionID);
            if (actionPersuasion == null) { Debug.LogError("Invalid actionPersuasion (Null)"); }
        }
        else { Debug.LogError("Invalid gearPersuasion actionID (not found)"); }
        //DEBUG
        /*DebugRandomActivityValues();*/
        //fast access -> Materials
        materialNormal = GetNodeMaterial(NodeType.Normal);
        materialHighlight = GetNodeMaterial(NodeType.Highlight);
        materialActive = GetNodeMaterial(NodeType.Active);
        materialPlayer = GetNodeMaterial(NodeType.Player);
        Debug.Assert(materialNormal != null, "Invalid materialNormal (Null)");
        Debug.Assert(materialHighlight != null, "Invalid materialHighlight (Null)");
        Debug.Assert(materialActive != null, "Invalid materialActive (Null)");
        Debug.Assert(materialPlayer != null, "Invalid materialPlayer (Null)");
        //flash
        flashNodeTime = GameManager.instance.guiScript.flashNodeTime;
        Debug.Assert(flashNodeTime > 0, "Invalid flashNodeTime (zero)");
        //register listener
        EventManager.instance.AddListener(EventType.NodeDisplay, OnEvent, "NodeManager");
        EventManager.instance.AddListener(EventType.ActivityDisplay, OnEvent, "NodeManager");
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "NodeManager");
        EventManager.instance.AddListener(EventType.CreateMoveMenu, OnEvent, "NodeManager");
        EventManager.instance.AddListener(EventType.CreateGearNodeMenu, OnEvent, "NodeManager");
        EventManager.instance.AddListener(EventType.MoveAction, OnEvent, "NodeManager");
        /*EventManager.instance.AddListener(EventType.DiceReturnMove, OnEvent, "NodeManager");*/
        /*EventManager.instance.AddListener(EventType.StartTurnEarly, OnEvent, "NodeManager");*/
        EventManager.instance.AddListener(EventType.StartTurnLate, OnEvent, "NodeManager");
        EventManager.instance.AddListener(EventType.FlashNodeStart, OnEvent, "NodeManager");
        EventManager.instance.AddListener(EventType.FlashNodeStop, OnEvent, "NodeManager");
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
                switch (nodeUI)
                {
                    case NodeUI.Reset:
                        ResetAll();
                        break;
                    case NodeUI.Redraw:
                        RedrawNodes();
                        break;
                    case NodeUI.Move:
                    case NodeUI.ShowTargets:
                    case NodeUI.ShowSpiders:
                    case NodeUI.ShowTracers:
                    case NodeUI.ShowTeams:
                    case NodeUI.MostConnected:
                    case NodeUI.Centre:
                    case NodeUI.NearNeighbours:
                    case NodeUI.DecisionNodes:
                    case NodeUI.CrisisNodes:
                    case NodeUI.NodeArc0:
                    case NodeUI.NodeArc1:
                    case NodeUI.NodeArc2:
                    case NodeUI.NodeArc3:
                    case NodeUI.NodeArc4:
                    case NodeUI.NodeArc5:
                    case NodeUI.NodeArc6:
                    case NodeUI.NodeArc7:
                        if (NodeShowFlag > 0)
                        { ResetAll(); }
                        else { ShowNodes(nodeUI); }
                        break;

                    default:
                        /*Debug.LogError(string.Format("Invalid NodeUI param \"{0}\"{1}", Param.ToString(), "\n"));*/
                        Debug.LogError(string.Format("Invalid NodeUI param \"{0}\"{1}", nodeUI, "\n"));
                        break;
                }
                break;
            case EventType.FlashNodeStart:
                StartFlashingNode((int)Param);
                break;
            case EventType.FlashNodeStop:
                StopFlashingNode();
                break;
            case EventType.ActivityDisplay:
                ActivityUI activityUI = (ActivityUI)Param;
                switch (activityUI)
                {
                    case ActivityUI.Time:
                    case ActivityUI.Count:
                        if (NodeShowFlag > 0)
                        { ResetAll(); }
                        else { ShowActivity(activityUI); }
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid activityUI \"{0}\"", activityUI));
                        break;
                }
                break;
            case EventType.CreateMoveMenu:
                CreateMoveMenu((int)Param);
                break;
            case EventType.CreateGearNodeMenu:
                CreateGearNodeMenu((int)Param);
                break;
            case EventType.MoveAction:
                ModalMoveDetails details = Param as ModalMoveDetails;
                ProcessPlayerMove(details);
                break;
            /*case EventType.DiceReturnMove:
                MoveReturnData data = Param as MoveReturnData;
                ProcessMoveOutcome(data);
                break;*/
            /*case EventType.StartTurnEarly:
                break;*/
            case EventType.StartTurnLate:
                ProcessNodeTimers();
                ProcessNodeCrisis();
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
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourHighlight = GameManager.instance.colourScript.GetColour(ColourType.nodeActive);
        colourResistance = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourError = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourInvalid = GameManager.instance.colourScript.GetColour(ColourType.cancelHighlight);
        colourCancel = GameManager.instance.colourScript.GetColour(ColourType.cancelNormal);
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
    { return arrayOfNodeMaterials[(int)nodeType]; }


    /// <summary>
    /// highlights all nodes depening on the enum NodeUI criteria
    /// </summary>
    public void ShowNodes(NodeUI nodeUI)
    {
        int data = -1;
        bool successFlag = true;
        bool nodeTypeFlag = false;
        bool proceedFlag = false;
        string displayText = null;
        //set all nodes to default colour first
        ResetNodes();
        if (GameManager.instance.connScript.resetConnections == true)
        {
            //return to previously saved state prior to any changes
            GameManager.instance.connScript.RestoreConnections();
            GameManager.instance.connScript.resetConnections = false;
        }
        //set nodes depending on critera
        switch (nodeUI)
        {
            //show all nodes with Targets
            case NodeUI.ShowTargets:
                //change material for selected nodes (Live and Completed targets)
                List<Target> tempList = new List<Target>();
                if (GameManager.instance.optionScript.fogOfWar == false)
                {
                    //FOW Off
                    tempList.AddRange(GameManager.instance.dataScript.GetTargetPool(Status.Active));
                    tempList.AddRange(GameManager.instance.dataScript.GetTargetPool(Status.Live));
                    tempList.AddRange(GameManager.instance.dataScript.GetTargetPool(Status.Completed));
                }
                else
                {
                    //FOW On
                    tempList.AddRange(GameManager.instance.dataScript.GetTargetPool(Status.Live));
                    tempList.AddRange(GameManager.instance.dataScript.GetTargetPool(Status.Completed));
                }
                if (tempList.Count > 0)
                {
                    foreach (Target target in tempList)
                    {
                        Node nodeTemp = GameManager.instance.dataScript.GetNode(target.nodeID);
                        if (nodeTemp != null)
                        { nodeTemp.SetMaterial(materialActive); }
                        else { Debug.LogWarning(string.Format("Invalid node (Null) for target.nodeID {0}", target.nodeID)); }
                    }
                    displayText = string.Format("{0}{1}{2}{3} Target{4}{5} district{6}{7}", colourDefault, tempList.Count, colourEnd, colourHighlight, colourEnd,
                        colourDefault, tempList.Count != 1 ? "s" : "", colourEnd);
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
                        if (nodeList.Count > 0)
                        {
                            foreach (Node node in nodeList)
                            {
                                if (node != null)
                                { node.SetMaterial(materialActive); }
                            }
                            displayText = string.Format("{0}{1}{2} {3}valid Move district{4}{5}", colourDefault, nodeList.Count, colourEnd,
                                colourHighlight, nodeList.Count != 1 ? "s" : "", colourEnd);
                        }
                        else
                        {
                            displayText = string.Format("{0}There are no districts you can Move to{1}", colourError, colourEnd);
                            Debug.LogWarning("No records in list of Neighbouring Nodes");
                        }
                    }
                    else { Debug.LogError("Invalid nodeList (Null) for GetNeighbouring nodes"); }
                }
                else { Debug.LogError(string.Format("Invalid node (Null) for NodeID {0}", nodePlayer)); }
                break;

            //show all nodes containng a spider
            case NodeUI.ShowSpiders:
                Dictionary<int, Node> dictOfSpiderNodes = GameManager.instance.dataScript.GetDictOfNodes();
                if (dictOfSpiderNodes != null)
                {
                    int count = 0;
                    //determine level of visibility
                    switch (GameManager.instance.sideScript.PlayerSide.name)
                    {
                        case "Authority":
                            proceedFlag = true;
                            break;
                        case "Resistance":
                            //resistance -> if not FOW then auto show
                            if (GameManager.instance.optionScript.fogOfWar == false)
                            { proceedFlag = true; }
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                            break;
                    }
                    foreach (var node in dictOfSpiderNodes)
                    {
                        if (node.Value.isSpider == true)
                        {
                            //show all
                            if (proceedFlag == true)
                            {
                                node.Value.SetMaterial(materialActive);
                                count++;
                            }
                            //conditional -> only show if spider is known
                            else
                            {
                                if (node.Value.isSpiderKnown == true)
                                {
                                    node.Value.SetMaterial(materialActive);
                                    count++;
                                }
                            }
                        }
                    }
                    if (count > 0)
                    {
                        displayText = string.Format("{0}{1}{2} {3}{4}{5} {6}district{7}{8}", colourDefault, count, colourEnd,
                            colourHighlight, "Spider", colourEnd, colourDefault, count != 1 ? "s" : "", colourEnd);
                    }
                    else
                    { displayText = string.Format("{0}There are no Spider districts{1}", colourError, colourEnd); }
                }
                else { Debug.LogError("Invalid dictOfSpiderNodes (Null)"); }
                break;

            //show all nodes with a tracer or within one node radius of a tracer
            case NodeUI.ShowTracers:
                Dictionary<int, Node> dictOfTracerNodes = GameManager.instance.dataScript.GetDictOfNodes();
                if (dictOfTracerNodes != null)
                {
                    int count = 0;
                    //determine level of visibility
                    switch (GameManager.instance.sideScript.PlayerSide.name)
                    {
                        case "Resistance":
                            proceedFlag = true;
                            break;
                        case "Authority":
                            //resistance -> if not FOW then auto show
                            if (GameManager.instance.optionScript.fogOfWar == false)
                            { proceedFlag = true; }
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                            break;
                    }
                    foreach (var node in dictOfTracerNodes)
                    {
                        if (node.Value.isTracerActive == true)
                        {
                            /*node.Value.SetMaterial(nodeMaterial);
                            //count number of tracers, not active tracer nodes
                            if (node.Value.isTracer == true)
                            { count++; }*/

                            //show all
                            if (proceedFlag == true)
                            {
                                node.Value.SetMaterial(materialActive);
                                count++;
                            }
                            //conditional -> only show if tracer is known
                            else
                            {
                                if (node.Value.isTracerKnown == true)
                                {
                                    node.Value.SetMaterial(materialActive);
                                    count++;
                                }
                            }
                        }
                    }
                    if (count > 0)
                    {
                        displayText = string.Format("{0}{1}{2} {3}{4}{5} {6}district{7}{8}", colourDefault, count, colourEnd,
                            colourHighlight, "Tracer", colourEnd, colourDefault, count != 1 ? "s" : "", colourEnd);
                    }
                    else
                    { displayText = string.Format("{0}There are no Tracer districts{1}", colourError, colourEnd); }
                }
                else { Debug.LogError("Invalid dictOfTracerNodes (Null)"); }
                break;

            //show all nodes containng a Team
            case NodeUI.ShowTeams:
                List<int> listOfTeams = GameManager.instance.dataScript.GetTeamPool(TeamPool.OnMap);
                if (listOfTeams != null)
                {
                    int count = 0;
                    //determine level of visibility
                    switch (GameManager.instance.sideScript.PlayerSide.name)
                    {
                        case "Authority":
                            proceedFlag = true;
                            break;
                        case "Resistance":
                            //resistance -> if not FOW then auto show
                            if (GameManager.instance.optionScript.fogOfWar == false)
                            { proceedFlag = true; }
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid side \"{0}\"", GameManager.instance.sideScript.PlayerSide));
                            break;
                    }
                    Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetDictOfNodes();
                    if (dictOfNodes != null)
                    {
                        foreach (var node in dictOfNodes)
                        {
                            if (node.Value.CheckNumOfTeams() > 0)
                            {
                                //show all
                                if (proceedFlag == true)
                                {
                                    node.Value.SetMaterial(materialActive);
                                    count++;
                                }
                                //conditional -> only show if team is known, actor has contacts or node within tracer coverage
                                else
                                {
                                    if (node.Value.isTeamKnown || node.Value.isTracerActive || node.Value.isContactKnown)
                                    {
                                        node.Value.SetMaterial(materialActive);
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                    else { Debug.LogError("Invalid dictOfNodes (Null)"); }
                    if (count > 0)
                    {
                        displayText = string.Format("{0}{1}{2} {3}{4}{5} {6}district{7}{8}", colourDefault, count, colourEnd,
                            colourHighlight, "Team", colourEnd, colourDefault, count != 1 ? "s" : "", colourEnd);
                    }
                    else
                    { displayText = string.Format("{0}There are no Teams present{1}", colourError, colourEnd); }
                }
                break;
            case NodeUI.MostConnected:
                List<Node> connectedList = GameManager.instance.dataScript.GetListOfMostConnectedNodes();
                if (connectedList != null)
                {
                    if (connectedList.Count > 0)
                    {
                        foreach (Node node in connectedList)
                        {
                            if (node != null)
                            { node.SetMaterial(materialActive); }
                            else { Debug.LogWarning("Invalid node (Null)"); }
                        }
                        displayText = string.Format("{0}{1}{2}{3} Most Connected district{4}{5}", colourDefault, connectedList.Count, colourEnd, colourHighlight,
                            connectedList.Count != 1 ? "s" : "", colourEnd);
                    }
                    else { displayText = string.Format("{0}{1}{2}", colourError, "No Records present", colourEnd); }
                }
                else
                {
                    Debug.LogWarning("Invalid connectedList (Null)");
                    displayText = string.Format("{0}{1}{2}", colourError, "ERROR: Can't find Connected districts", colourEnd);
                }
                break;
            case NodeUI.DecisionNodes:
                //highlight nodes in DataManager.cs -> ListOfDecisionNodes (ones used for AI 'connection Security' decisions)
                List<Node> decisionList = GameManager.instance.dataScript.GetListOfDecisionNodes();
                if (decisionList != null)
                {
                    if (decisionList.Count > 0)
                    {
                        foreach (Node node in decisionList)
                        {
                            if (node != null)
                            { node.SetMaterial(materialActive); }
                            else { Debug.LogWarning("Invalid node (Null)"); }
                        }
                        displayText = string.Format("{0}{1}{2}{3} Decision district{4}{5}", colourDefault, decisionList.Count, colourEnd, colourHighlight,
                            decisionList.Count != 1 ? "s" : "", colourEnd);
                    }
                    else { displayText = string.Format("{0}{1}{2}", colourError, "No Records present", colourEnd); }
                }
                else
                {
                    Debug.LogWarning("Invalid decisionList (Null)");
                    displayText = string.Format("{0}{1}{2}", colourError, "ERROR: Can't find Decision districts", colourEnd);
                }
                break;
            case NodeUI.CrisisNodes:
                //highlight nodes in DataManager.cs -> ListOfCrisisNodes (ones a node crisis)
                List<Node> crisisList = GameManager.instance.dataScript.GetListOfCrisisNodes();
                if (crisisList != null)
                {
                    if (crisisList.Count > 0)
                    {
                        foreach (Node node in crisisList)
                        {
                            if (node != null)
                            { node.SetMaterial(materialActive); }
                            else { Debug.LogWarning("Invalid node (Null)"); }
                        }
                        displayText = string.Format("{0}{1}{2}{3} Crisis district{4}{5}", colourDefault, crisisList.Count, colourEnd, colourHighlight,
                            crisisList.Count != 1 ? "s" : "", colourEnd);
                    }
                    else { displayText = string.Format("{0}{1}{2}", colourError, "No Records present", colourEnd); }
                }
                else
                {
                    Debug.LogWarning("Invalid crisisList (Null)");
                    displayText = string.Format("{0}{1}{2}", colourError, "ERROR: Can't find Crisis districts", colourEnd);
                }
                break;
            case NodeUI.NearNeighbours:
                Node nodeNear = GameManager.instance.dataScript.GetNode(nodePlayer);
                if (nodeNear != null)
                {
                    List<Node> listOfNearNeighbours = nodeNear.GetNearNeighbours();
                    if (listOfNearNeighbours != null)
                    {
                        if (listOfNearNeighbours.Count > 0)
                        {
                            foreach (Node nodeTemp in listOfNearNeighbours)
                            {
                                if (nodeTemp != null)
                                {
                                    nodeTemp.SetMaterial(materialActive);
                                }
                                else { Debug.LogWarning("Invalid nodeTemp (Null)"); }
                            }
                            displayText = string.Format("{0}{1}{2}{3} Near Neighbouring district{4}{5}", colourDefault, listOfNearNeighbours.Count, colourEnd, colourHighlight,
                                listOfNearNeighbours.Count != 1 ? "s" : "", colourEnd);
                        }
                        else { displayText = string.Format("{0}{1}{2}", colourError, "No Records present", colourEnd); }
                    }
                    else
                    {
                        Debug.LogWarning("Invalid listOfNearNeighbours (Null)");
                        displayText = string.Format("{0}ERROR: Can't find Neighbouring districts{1}", colourError, colourEnd);
                    }
                }
                else
                {
                    Debug.LogWarning("Invalid playerNode (Null)");
                    displayText = string.Format("{0}ERROR: Player district not found{1}", colourError, colourEnd);
                }
                break;
            case NodeUI.Centre:
                //display all nodes with AI designated central area (node.isCentreNode true)
                Dictionary<int, Node> dictOfCentreNodes = GameManager.instance.dataScript.GetDictOfNodes();
                if (dictOfCentreNodes != null)
                {
                    if (dictOfCentreNodes.Count > 0)
                    {
                        int counter = 0;
                        foreach (var node in dictOfCentreNodes)
                        {
                            if (node.Value != null)
                            {
                                if (node.Value.isCentreNode == true)
                                {
                                    node.Value.SetMaterial(materialActive);
                                    counter++;
                                }
                            }
                            else { Debug.LogWarning("Invalid node (Null)"); }
                        }
                        displayText = string.Format("{0}{1}{2}{3} Centred district{4}{5}", colourDefault, counter, colourEnd, colourHighlight,
                            dictOfCentreNodes.Count != 1 ? "s" : "", colourEnd);
                    }
                    else { displayText = string.Format("{0}{1}{2}", colourError, "No Records present", colourEnd); }
                }
                else
                {
                    Debug.LogWarning("Invalid dictOfCentreNodes (Null)");
                    displayText = string.Format("{0}{1}{2}", colourError, "ERROR: Null dictOfCentreNodes", colourEnd);
                }
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
                        { node.SetMaterial(materialActive); }
                    }
                    displayText = string.Format("{0}{1}{2} {3}{4}{5} {6}district{7}{8}", colourDefault, nodeList.Count, colourEnd,
                    colourHighlight, nodeArc.name, colourEnd, colourDefault, nodeList.Count != 1 ? "s" : "", colourEnd);
                }
                else
                {
                    if (nodeArc != null)
                    { displayText = string.Format("{0}There are no {1} districts{2}", colourError, nodeArc.name, colourEnd); }
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
            if (string.IsNullOrEmpty(displayText) == false)
            { GameManager.instance.alertScript.SetAlertUI(displayText); }
        }
    }

    /// <summary>
    /// Show all active nodes for a particular actor. Use actor.slotID (0 to numOfActors)
    /// </summary>
    /// <param name="slotID"></param>
    public void ShowActiveNodes(int slotID)
    {
        Debug.Assert(slotID > -1 && slotID < GameManager.instance.actorScript.maxNumOfOnMapActors, "Invalid slotID");
        //set all nodes to default colour first
        ResetNodes();
        //change material for selected nodes
        List<GameObject> tempList = GameManager.instance.dataScript.GetListOfActorNodes(slotID);
        foreach (GameObject obj in tempList)
        {
            Node nodeTemp = obj.GetComponent<Node>();
            nodeTemp.SetMaterial(materialActive);
        }
        //Get Actor
        Actor actor = GameManager.instance.dataScript.GetCurrentActor(slotID, GameManager.instance.sideScript.PlayerSide);
        string displayText;
        string minionTitle;
        //work out minion's appropriate title
        if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { minionTitle = string.Format("{0} of ", GameManager.instance.metaScript.GetAuthorityTitle()); }
        else { minionTitle = "Rebel "; }
        if (actor != null)
        {
            displayText = string.Format("{0}\"{1}\"{2} {3}{4}{5}{6}{7}{8} {9}{10} district{11}{12}", colourHighlight, actor.actorName, colourEnd,
                colourDefault, minionTitle, colourEnd,
                colourHighlight, actor.arc.name, colourEnd,
                colourDefault, tempList.Count, tempList.Count != 1 ? "s" : "", colourEnd);
            GameManager.instance.alertScript.SetAlertUI(displayText);
            NodeShowFlag = 1;
        }
    }

    /// <summary>
    /// Sets flag to show player node whenever a node redraw. Normally true but switch off if you want to flash the player node instead
    /// </summary>
    /// <param name="isShown"></param>
    public void SetShowPlayerNode(bool isShown = true)
    { showPlayerNode = isShown; }

    /// <summary>
    /// Redraw any nodes. Show highlighted node, unless it's a non-normal node for the current redraw
    /// </summary>
    public void RedrawNodes()
    {
        Renderer nodeRenderer;
        bool proceedFlag = true;
        Dictionary<int, Node> tempDict = GameManager.instance.dataScript.GetDictOfNodes();
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
                    if (node.GetMaterial() == materialNormal)
                    {
                        nodeRenderer = node.GetComponent<Renderer>();
                        nodeRenderer.material = materialHighlight;
                    }
                }
                else { Debug.LogError("Invalid Node (null) returned from dictOfNodes"); }
            }
            //show player node if flag is true (default condition -> would be false only in case of flashing player Node)
            if (showPlayerNode == true)
            {
                //player's current node (Resistance side only if FOW ON)
                if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
                {
                    if (GameManager.instance.optionScript.fogOfWar == true)
                    { proceedFlag = false; }
                }
                if (proceedFlag == true)
                {
                    if (nodePlayer > -1)
                    {
                        Node node = GameManager.instance.dataScript.GetNode(nodePlayer);
                        if (node != null)
                        {
                            //only do so if it's a normal node, otherwise ignore
                            if (node.GetMaterial() == materialNormal)
                            {
                                nodeRenderer = node.GetComponent<Renderer>();
                                nodeRenderer.material = materialPlayer;
                            }
                        }
                    }
                }
            }
            //reset flag to prevent constant redraws
            NodeRedraw = false;
        }
        else { Debug.LogError("Invalid dictOfNodes (Null) returned from dataManager in RedrawNodes"); }
    }

    /// <summary>
    /// Sets the material colour of all nodes to the default (doesn't change on screen, just sets them up). Call before making any changes to node colours
    /// </summary>
    public void ResetNodes()
    {
        //loop and assign
        Dictionary<int, Node> tempDict = GameManager.instance.dataScript.GetDictOfNodes();
        if (tempDict != null)
        {
            foreach (var node in tempDict)
            {
                node.Value.SetMaterial(materialNormal);
                node.Value.faceText.text = "";
            }
            //trigger an automatic redraw
            NodeRedraw = true;
        }
        else { Debug.LogError("Invalid dictOfNodes (Null) returned from dataManager in ResetNodes"); }

    }

    /// <summary>
    /// shows resistance activity in various forms (nodes and connections)
    /// </summary>
    /// <param name="activtyUI"></param>
    private void ShowActivity(ActivityUI activityUI)
    {
        //set state
        activityState = activityUI;
        //show activity
        switch (activityUI)
        {
            case ActivityUI.Count:
            case ActivityUI.Time:
                //change connections to reflect activity (also sets resetNeeded to True)
                GameManager.instance.connScript.ShowConnectionActivity(activityUI);
                //activate face text on nodes to reflect activity levels
                ShowNodeActivity(activityUI);
                break;
            default:
                Debug.LogError(string.Format("Invalid activityUI \"{0}\"", activityUI));
                break;
        }
        //set show node flag on success
        NodeShowFlag = 1;
        string displayText = "Unknown";
        switch (activityUI)
        {
            case ActivityUI.Count:
                displayText = string.Format("{0}Resistance Activity by Count{1}", colourNeutral, colourEnd);
                break;
            case ActivityUI.Time:
                displayText = string.Format("{0}Resistance Activity by Time{1}", colourNeutral, colourEnd);
                break;
        }
        //active AlertUI
        if (string.IsNullOrEmpty(displayText) == false)
        { GameManager.instance.alertScript.SetAlertUI(displayText); }
    }

    /// <summary>
    /// Highlight activity levels on node faces
    /// </summary>
    /// <param name="activityUI"></param>
    private void ShowNodeActivity(ActivityUI activityUI)
    {
        if (activityUI != ActivityUI.None)
        {
            int data;
            Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetDictOfNodes();
            if (dictOfNodes != null)
            {
                //loop nodes
                bool errorFlag = false;
                foreach(var node in dictOfNodes)
                {
                    if (node.Value != null)
                    {
                        if (node.Value.faceObject != null)
                        {
                            switch(activityUI)
                            {
                                case ActivityUI.Count:
                                case ActivityUI.Time:
                                    data = node.Value.GetNodeActivity(activityUI);
                                    if (data > -1)
                                    {
                                        node.Value.faceText.text = data.ToString();
                                        node.Value.faceText.color = GetActivityColour(activityUI, data);
                                    }
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid activityUI \"{0}\"", activityUI));
                                    errorFlag = true;
                                    break;
                            }
                        }
                        else { Debug.LogWarning(string.Format("Invalid node faceObject (Null) for nodeID {0}", node.Key)); }

                        if (errorFlag == true)
                        { break; }
                    }
                    else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", node.Key)); }
                }
            }
            else { Debug.LogError("Invalid dictOfNodes (Null)"); }
        }
    }

    /// <summary>
    /// returns a colour to display face Text on a node depending on value (which varies depending on activityUI)
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private Color GetActivityColour(ActivityUI activityUI, int data)
    {
        Color color = Color.clear;
        switch (activityUI)
        {
            case ActivityUI.Count:
                //0 -> clear, 1 -> Green, 2 -> Yellow, 3+ -> Red
                switch (data)
                {
                    case 0: color = Color.clear; break;
                    case 1: color = Color.green; break;
                    case 2: color = Color.yellow; break;
                    default:
                    case 3: color = Color.red; break;
                }
                break;
            case ActivityUI.Time:
                //0 -> clear, 1 -> Red, 2 -> Yellow, 3+ -> Green (if past limit then Grey)
                switch (data)
                {
                    case 0: color = Color.red; break;
                    case 1: color = Color.red; break;
                    case 2: color = Color.yellow; break;
                    case 3: color = Color.green; break;
                    default:
                        //only display elapsed time data if it's not too old to be of use
                        if (data > GameManager.instance.aiScript.activityTimeLimit)
                        { color = Color.clear; }
                        else { color = Color.black; }
                        break;
                }
                break;
        }
        return color;
    }

    /// <summary>
    /// resets all nodes and connections back to their default (nodes) or previously saved (connections) states
    /// </summary>
    public void ResetAll()
    {
        //set all nodes to default colour first
        ResetNodes();
        //Connections
        if (NodeShowFlag > 0)
        {
            GameManager.instance.alertScript.CloseAlertUI();
            if (GameManager.instance.connScript.resetConnections == true)
            {
                //return Connections to previously saved state prior to any changes
                GameManager.instance.connScript.RestoreConnections();
                GameManager.instance.connScript.resetConnections = false;
            }
            NodeShowFlag = 0;
            //reset state
            activityState = ActivityUI.None;
            //DEBUG
            GameManager.instance.debugGraphicsScript.SetCentrePane(false);
        }
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
                colourDefault, node.Arc.name, node.nodeID, colourEnd);
            string moveMain = "UNKNOWN";
            int adjustInvisibility = 0;
            //Get Connection (between new node and Player's current location)
            Connection connection = node.GetConnection(nodePlayer);
            if (connection != null)
            {
                ConnectionType connSecType = connection.SecurityLevel;
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
                        string movement = GameManager.instance.gearScript.typeMovement.name;
                        for (int i = 0; i < listOfGear.Count; i++)
                        {
                            StringBuilder builderDetail = new StringBuilder();
                            Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
                            if (gear != null)
                            {
                                /*if (gear.type == GearTypeEnum.Movement)*/
                                if (gear.type.name.Equals(movement) == true)
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
                                        builderDetail.Append(string.Format("{0}No risk of being spotted{1}", colourGood, colourEnd));
                                    }
                                    else
                                    {
                                        //gear falls short of security level
                                        adjustInvisibility = -1;
                                        if (GameManager.instance.playerScript.Invisibility <= 1)
                                        {
                                            //invisibility will be zero, or less, if move. Immediate notification
                                            builderDetail.Append(string.Format("{0}Invisibility -1{1}<size=110%>Authority will know Immediately</size>{2}", colourBad, "\n",
                                              colourEnd));
                                            moveGearDetails.ai_Delay = 0;
                                        }
                                        else
                                        {
                                            //invisibility reduces, still above zero
                                            int delay = moveInvisibilityDelay - Mathf.Abs(gear.data - secLevel);
                                            delay = Mathf.Max(0, delay);
                                            moveGearDetails.ai_Delay = delay; 
                                            builderDetail.AppendFormat("{0}Invisibility -1{1}Authority will know in {2}{3}{4}{5}{6} turn{7}{8}", colourBad, "\n", colourEnd,
                                                colourNeutral, moveGearDetails.ai_Delay, colourEnd, colourBad, moveGearDetails.ai_Delay != 1 ? "s" : "", colourEnd);
                                        }
                                    }
                                    //add gear chance of compromise
                                    builderDetail.Append(string.Format("{0}{1}Gear has a {2}{3}{4} %{5}{6} chance of being compromised{7}", "\n", colourAlert, colourEnd, colourNeutral,
                                        GameManager.instance.gearScript.GetChanceOfCompromise(gear.gearID), colourEnd, colourAlert, colourEnd));

                                    //Move details
                                    moveGearDetails.nodeID = nodeID;
                                    moveGearDetails.connectionID = connection.connID;
                                    moveGearDetails.changeInvisibility = adjustInvisibility;
                                    moveGearDetails.gearID = gear.gearID;
                                    //button target details (red for High security to match red connection security colour on map)
                                    string colourGearLevel = colourNeutral;
                                    if (gear.data == 3) { colourGearLevel = colourGood; }
                                    else if (gear.data == 1) { colourGearLevel = colourBad; }
                                    EventButtonDetails eventMoveDetails = new EventButtonDetails()
                                    {
                                        buttonTitle = string.Format("{0} Move", gear.name),
                                        buttonTooltipHeader = string.Format("Move using{0}{1}{2}{3}{4}{5}{6}{7}", "\n", colourNeutral, gear.name, colourEnd,
                                        "\n", colourGearLevel, (ConnectionType)gear.data, colourEnd),
                                        buttonTooltipMain = moveMain,
                                        buttonTooltipDetail = builderDetail.ToString(),
                                        //use a Lambda to pass arguments to the action
                                        action = () => { EventManager.instance.PostNotification(EventType.MoveAction, this, moveGearDetails, "NodeManager.cs -> CreateMoveMenu"); }
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
                    if (GameManager.instance.playerScript.Invisibility <= 1)
                    {
                        //invisibility will be zero, or less, if move. Immediate notification
                        moveDetail = string.Format("{0}Invisibility -1{1}<size=110%>Authority will know Immediately</size>{2}", colourBad, "\n",
                          colourEnd);
                        moveDetails.ai_Delay = 0;
                    }
                    else
                    {
                        //invisibility reduces, still above zero
                        moveDetails.ai_Delay = secLevel;
                        moveDetail = string.Format("{0}Invisibility -1{1}Authority will know in {2}{3}{4}{5}{6} turn{7}{8}", colourBad, "\n", colourEnd, colourNeutral,
                          moveDetails.ai_Delay, colourEnd, colourBad, moveDetails.ai_Delay != 1 ? "s" : "", colourEnd);
                    }
                }
                else { moveDetail = string.Format("{0}No risk of being spotted{1}", colourGood, colourEnd); }

                //Move details
                moveDetails.nodeID = nodeID;
                moveDetails.connectionID = connection.connID;
                moveDetails.changeInvisibility = adjustInvisibility;
                moveDetails.gearID = -1;
                //button target details
                EventButtonDetails eventDetails = new EventButtonDetails()
                {
                    buttonTitle = "Move",
                    buttonTooltipHeader = string.Format("{0}Move (no gear){1}", colourNeutral, colourEnd),
                    buttonTooltipMain = moveMain,
                    buttonTooltipDetail = moveDetail,
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.MoveAction, this, moveDetails, "NodeManager.cs -> CreateMoveMenu"); }
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
                    action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "NodeManager.cs -> CreateMoveMenu"); }
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
            itemID = nodeID,
            itemName = node.nodeName,
            itemDetails = string.Format("{0} ID {1}", node.Arc.name, node.nodeID),
            itemPos = node.transform.position,
            listOfButtonDetails = tempList,
            menuType = ActionMenuType.Move
        };
        //activate menu
        GameManager.instance.actionMenuScript.SetActionMenu(details);
    }

    /// <summary>
    /// Right Click the Resistance Player's current node -> gear actions
    /// </summary>
    /// <param name="nodeID"></param>
    private void CreateGearNodeMenu(int nodeID)
    {
        Debug.LogFormat("[UI] NodeManager.cs -> CreateGearNodeMenu{0}", "\n");
        int counter = 0;                    //num of gear that can be used
        List<EventButtonDetails> tempList = new List<EventButtonDetails>();

        //Get Node
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        if (node != null)
        {
            //
            // - - - Gear Node Effects - - -
            //
            // only applies if R-clicked on Player's current node
            if (nodeID == nodePlayer)
            {
                List<Effect> listOfEffects;
                Action tempAction;
                bool proceedFlag;
                string effectCriteria;
                //string builder for cancel button (handles all no go cases
                StringBuilder infoBuilder = new StringBuilder();
                //
                // - - - Kinetic Gear - - -
                //
                List<Gear> listOfKineticGear = GameManager.instance.playerScript.GetListOfGearType(GameManager.instance.gearScript.typeKinetic);
                {
                    if (listOfKineticGear != null)
                    {
                        //loop list of gear and create a button for each item
                        for (int index = 0; index < listOfKineticGear.Count; index++)
                        {
                            proceedFlag = true;
                            Gear kineticGear = listOfKineticGear[index];
                            if (kineticGear != null)
                            {
                                tempAction = actionKinetic;
                                if (tempAction != null)
                                {
                                    counter++;
                                    //effects
                                    StringBuilder builder = new StringBuilder();
                                    listOfEffects = tempAction.listOfEffects;
                                    if (listOfEffects.Count > 0)
                                    {
                                        string colourEffect = colourDefault;
                                        for (int i = 0; i < listOfEffects.Count; i++)
                                        {
                                            Effect effect = listOfEffects[i];
                                            //colour code effects according to type
                                            if (effect.typeOfEffect != null)
                                            {
                                                switch (effect.typeOfEffect.name)
                                                {
                                                    case "Good":
                                                        colourEffect = colourGood;
                                                        break;
                                                    case "Neutral":
                                                        colourEffect = colourNeutral;
                                                        break;
                                                    case "Bad":
                                                        colourEffect = colourBad;
                                                        break;
                                                }
                                            }
                                            //check effect criteria is valid
                                            CriteriaDataInput criteriaInput = new CriteriaDataInput()
                                            {
                                                nodeID = nodeID,
                                                listOfCriteria = effect.listOfCriteria
                                            };
                                            effectCriteria = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
                                            if (effectCriteria == null)
                                            {
                                                //Effect criteria O.K -> tool tip text
                                                if (builder.Length > 0) { builder.AppendLine(); }
                                                if (effect.outcome.name.Equals("Renown") == false && effect.outcome.name.Equals("Invisibility") == false)
                                                { builder.Append(string.Format("{0}{1}{2}", colourEffect, effect.textTag, colourEnd)); }
                                                else
                                                {
                                                    //Invisibility and Renown -> player affected (good for renown, bad for invisibility)
                                                    if (effect.outcome.name.Equals("Renown"))
                                                    { builder.Append(string.Format("{0}Player {1}{2}", colourGood, effect.textTag, colourEnd)); }
                                                    else
                                                    { builder.Append(string.Format("{0}Player {1}{2}", colourBad, effect.textTag, colourEnd)); }
                                                }
                                            }
                                            else
                                            {
                                                //invalid effect criteria -> Action cancelled
                                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                                infoBuilder.Append(string.Format("{0}{1} action invalid{2}{3}{4}({5}){6}",
                                                    colourInvalid, kineticGear.name, "\n", colourEnd,
                                                    colourResistance, effectCriteria, colourEnd));
                                                proceedFlag = false;
                                            }
                                        }
                                    }
                                    else
                                    { Debug.LogWarning(string.Format("Action \"{0}\" has no effects", tempAction)); }
                                    if (proceedFlag == true)
                                    {
                                        //Details to pass on for processing via button click
                                        ModalActionDetails actionDetails = new ModalActionDetails() { };

                                        actionDetails.side = GameManager.instance.globalScript.sideResistance;
                                        actionDetails.nodeID = nodeID;
                                        actionDetails.gearAction = actionKinetic;
                                        actionDetails.gearID = kineticGear.gearID;
                                        //pass all relevant details to ModalActionMenu via Node.OnClick()
                                        EventButtonDetails kineticDetails = new EventButtonDetails()
                                        {
                                            buttonTitle = string.Format("Use {0}", kineticGear.name),
                                            buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, kineticGear.name, colourEnd),
                                            buttonTooltipMain = tempAction.tooltipText,
                                            buttonTooltipDetail = builder.ToString(),
                                            //use a Lambda to pass arguments to the action
                                            action = () => { EventManager.instance.PostNotification(EventType.NodeGearAction, this, actionDetails, "NodeManager.cs -> CreateGearNodeMenu"); }
                                        };
                                        tempList.Add(kineticDetails);
                                    }
                                }
                                else { Debug.LogError("Invalid actionKinetic (Null)"); }
                            }
                        }
                    }
                }
                //
                // - - - Hacking Gear - - -
                //
                List<Gear> listOfHackingGear = GameManager.instance.playerScript.GetListOfGearType(GameManager.instance.gearScript.typeHacking);
                {
                    if (listOfHackingGear != null)
                    {
                        //loop list of gear and create a button for each item
                        for (int index = 0; index < listOfHackingGear.Count; index++)
                        {
                            proceedFlag = true;
                            Gear hackingGear = listOfHackingGear[index];
                            if (hackingGear != null)
                            {
                                tempAction = actionHacking;
                                if (tempAction != null)
                                {
                                    counter++;
                                    //effects
                                    StringBuilder builder = new StringBuilder();
                                    listOfEffects = tempAction.listOfEffects;
                                    if (listOfEffects.Count > 0)
                                    {
                                        string colourEffect = colourDefault;
                                        for (int i = 0; i < listOfEffects.Count; i++)
                                        {
                                            Effect effect = listOfEffects[i];
                                            //colour code effects according to type
                                            if (effect.typeOfEffect != null)
                                            {
                                                switch (effect.typeOfEffect.name)
                                                {
                                                    case "Good":
                                                        colourEffect = colourGood;
                                                        break;
                                                    case "Neutral":
                                                        colourEffect = colourNeutral;
                                                        break;
                                                    case "Bad":
                                                        colourEffect = colourBad;
                                                        break;
                                                }
                                            }
                                            //check effect criteria is valid
                                            CriteriaDataInput criteriaInput = new CriteriaDataInput()
                                            {
                                                nodeID = nodeID,
                                                listOfCriteria = effect.listOfCriteria
                                            };
                                            effectCriteria = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
                                            if (effectCriteria == null)
                                            {
                                                //Effect criteria O.K -> tool tip text
                                                if (builder.Length > 0) { builder.AppendLine(); }
                                                if (effect.outcome.name.Equals("Renown") == false && effect.outcome.name.Equals("Invisibility") == false)
                                                { builder.Append(string.Format("{0}{1}{2}", colourEffect, effect.textTag, colourEnd)); }
                                                else
                                                {
                                                    //Invisibility and Renown -> player affected (good for renown, bad for invisibility)
                                                    if (effect.outcome.name.Equals("Renown"))
                                                    { builder.Append(string.Format("{0}Player {1}{2}", colourGood, effect.textTag, colourEnd)); }
                                                    else
                                                    { builder.Append(string.Format("{0}Player {1}{2}", colourBad, effect.textTag, colourEnd)); }
                                                }
                                            }
                                            else
                                            {
                                                //invalid effect criteria -> Action cancelled
                                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                                infoBuilder.Append(string.Format("{0}{1} action invalid{2}{3}{4}({5}){6}",
                                                    colourInvalid, hackingGear.name, "\n", colourEnd,
                                                    colourResistance, effectCriteria, colourEnd));
                                                proceedFlag = false;
                                            }
                                        }
                                    }
                                    else
                                    { Debug.LogWarning(string.Format("Action \"{0}\" has no effects", tempAction)); }
                                    if (proceedFlag == true)
                                    {
                                        //Details to pass on for processing via button click
                                        ModalActionDetails actionDetails = new ModalActionDetails() { };

                                        actionDetails.side = GameManager.instance.globalScript.sideResistance;
                                        actionDetails.nodeID = nodeID;
                                        actionDetails.gearAction = actionHacking;
                                        actionDetails.gearID = hackingGear.gearID;
                                        //pass all relevant details to ModalActionMenu via Node.OnClick()
                                        EventButtonDetails hackingDetails = new EventButtonDetails()
                                        {
                                            buttonTitle = string.Format("Use {0}", hackingGear.name),
                                            buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, hackingGear.name, colourEnd),
                                            buttonTooltipMain = tempAction.tooltipText,
                                            buttonTooltipDetail = builder.ToString(),
                                            //use a Lambda to pass arguments to the action
                                            action = () => { EventManager.instance.PostNotification(EventType.NodeGearAction, this, actionDetails, "NodeManager.cs -> CreateGearNodeMenu"); }
                                        };
                                        tempList.Add(hackingDetails);
                                    }
                                }
                                else { Debug.LogError("Invalid actionHacking (Null)"); }
                            }
                        }
                    }
                }
                //
                // - - - Persuasion Gear - - -
                //
                List<Gear> listOfPersuasionGear = GameManager.instance.playerScript.GetListOfGearType(GameManager.instance.gearScript.typePersuasion);
                {
                    if (listOfPersuasionGear != null)
                    {
                        //loop list of gear and create a button for each item
                        for (int index = 0; index < listOfPersuasionGear.Count; index++)
                        {
                            proceedFlag = true;
                            Gear persuasionGear = listOfPersuasionGear[index];
                            if (persuasionGear != null)
                            {
                                tempAction = actionPersuasion;
                                if (tempAction != null)
                                {
                                    counter++;
                                    //effects
                                    StringBuilder builder = new StringBuilder();
                                    listOfEffects = tempAction.listOfEffects;
                                    if (listOfEffects.Count > 0)
                                    {
                                        string colourEffect = colourDefault;
                                        for (int i = 0; i < listOfEffects.Count; i++)
                                        {
                                            Effect effect = listOfEffects[i];
                                            //colour code effects according to type
                                            if (effect.typeOfEffect != null)
                                            {
                                                switch (effect.typeOfEffect.name)
                                                {
                                                    case "Good":
                                                        colourEffect = colourGood;
                                                        break;
                                                    case "Neutral":
                                                        colourEffect = colourNeutral;
                                                        break;
                                                    case "Bad":
                                                        colourEffect = colourBad;
                                                        break;
                                                }
                                            }
                                            //check effect criteria is valid
                                            CriteriaDataInput criteriaInput = new CriteriaDataInput()
                                            {
                                                nodeID = nodeID,
                                                listOfCriteria = effect.listOfCriteria
                                            };
                                            effectCriteria = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
                                            if (effectCriteria == null)
                                            {
                                                //Effect criteria O.K -> tool tip text
                                                if (builder.Length > 0) { builder.AppendLine(); }
                                                if (effect.outcome.name.Equals("Renown") == false && effect.outcome.name.Equals("Invisibility") == false)
                                                { builder.Append(string.Format("{0}{1}{2}", colourEffect, effect.textTag, colourEnd)); }
                                                else
                                                {
                                                    //Invisibility and Renown -> player affected (good for renown, bad for invisibility)
                                                    if (effect.outcome.name.Equals("Renown"))
                                                    { builder.Append(string.Format("{0}Player {1}{2}", colourGood, effect.textTag, colourEnd)); }
                                                    else
                                                    { builder.Append(string.Format("{0}Player {1}{2}", colourBad, effect.textTag, colourEnd)); }
                                                }
                                            }
                                            else
                                            {
                                                //invalid effect criteria -> Action cancelled
                                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                                infoBuilder.Append(string.Format("{0}{1} action invalid{2}{3}{4}({5}){6}",
                                                    colourInvalid, persuasionGear.name, "\n", colourEnd,
                                                    colourResistance, effectCriteria, colourEnd));
                                                proceedFlag = false;
                                            }
                                        }
                                    }
                                    else
                                    { Debug.LogWarning(string.Format("Action \"{0}\" has no effects", tempAction)); }
                                    if (proceedFlag == true)
                                    {
                                        //Details to pass on for processing via button click
                                        ModalActionDetails actionDetails = new ModalActionDetails() { };

                                        actionDetails.side = GameManager.instance.globalScript.sideResistance;
                                        actionDetails.nodeID = nodeID;
                                        actionDetails.gearAction = actionPersuasion;
                                        actionDetails.gearID = persuasionGear.gearID;
                                        //pass all relevant details to ModalActionMenu via Node.OnClick()
                                        EventButtonDetails persuasionDetails = new EventButtonDetails()
                                        {
                                            buttonTitle = string.Format("Use {0}", persuasionGear.name),
                                            buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, persuasionGear.name, colourEnd),
                                            buttonTooltipMain = tempAction.tooltipText,
                                            buttonTooltipDetail = builder.ToString(),
                                            //use a Lambda to pass arguments to the action
                                            action = () => { EventManager.instance.PostNotification(EventType.NodeGearAction, this, actionDetails, "NodeManager.cs -> CreateGearNodeMenu"); }
                                        };
                                        tempList.Add(persuasionDetails);
                                    }
                                }
                                else { Debug.LogError("Invalid actionPersuasion (Null)"); }
                            }
                        }
                    }
                }
                //
                // - - - Cancel
                //
                //Cancel button is added last
                EventButtonDetails cancelDetails = null;
                //does player have gear?
                if (GameManager.instance.playerScript.CheckNumOfGear() > 0)
                {
                    //is there any relevant, node-use, gear?
                    if (counter > 0)
                    {
                        //are there any restrictions on the use of the node-type gear?
                        if (infoBuilder.Length > 0)
                        {
                            //node-type gear present but there are restrictions
                            cancelDetails = new EventButtonDetails()
                            {
                                buttonTitle = "CANCEL",
                                buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, "INFO", colourEnd),
                                buttonTooltipMain = "There are some limitations preventing gear use",
                                buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, infoBuilder.ToString(), colourEnd),
                                //use a Lambda to pass arguments to the action
                                action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "NodeManager.cs -> CreateGearNodeMenu"); }
                            };
                        }
                        else
                        {
                            //node-type gear present but no restrictions
                            cancelDetails = new EventButtonDetails()
                            {
                                buttonTitle = "CANCEL",
                                buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, "INFO", colourEnd),
                                buttonTooltipMain = "You decide not to use your gear to carry out a district action",
                                //use a Lambda to pass arguments to the action
                                action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "NodeManager.cs -> CreateGearNodeMenu"); }
                            };
                        }
                    }
                    else
                        //Gear present but none of it can be used at Nodes
                        cancelDetails = new EventButtonDetails()
                        {
                            buttonTitle = "CANCEL",
                            buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, "INFO", colourEnd),
                            buttonTooltipMain = "You can only take Gear actions at a district if you have gear with that capability",
                            //use a Lambda to pass arguments to the action
                            action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "NodeManager.cs -> CreateGearNodeMenu"); }
                        };
                }
                else
                {
                    //NO GEAR
                    cancelDetails = new EventButtonDetails()
                    {
                        buttonTitle = "CANCEL",
                        buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, "INFO", colourEnd),
                        buttonTooltipMain = string.Format("{0}You do not have any Gear capable of being used in districts{1}", colourAlert, colourEnd),
                        //use a Lambda to pass arguments to the action
                        action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this, null, "NodeManager.cs -> CreateGearNodeMenu"); }
                    };
                }
                //add Cancel button to list
                tempList.Add(cancelDetails);
            }
        }
        else { Debug.LogError(string.Format("Invalid Node (null), ID {0}", nodeID)); }
        //
        // - - - Action Menu
        //
        ModalPanelDetails details = new ModalPanelDetails()
        {
            itemID = nodeID,
            itemName = node.nodeName,
            itemDetails = string.Format("{0}District Gear Actions{1}", colourResistance, colourEnd),
            itemPos = node.transform.position,
            listOfButtonDetails = tempList,
            menuType = ActionMenuType.NodeGear
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
            if (node != null)
            {
                //update Player node
                nodePlayer = moveDetails.nodeID;
                //update move list
                node.SetMoveNodes();
                string destination = string.Format("\"{0}\", {1}, ID {2}", node.nodeName, node.Arc.name, node.nodeID);
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
                    //display
                    builder.AppendLine();
                    builder.AppendFormat("{0}Invisibility {1}{2}{3}", colourBad, moveDetails.changeInvisibility > 0 ? "+" : "",
                        moveDetails.changeInvisibility, colourEnd);
                    //player invisibility
                    int invisibility = GameManager.instance.playerScript.Invisibility;
                    if (invisibility == 0)
                    {
                        //moving while invisibility already 0 triggers immediate alert flag
                        GameManager.instance.aiScript.immediateFlagResistance = true;
                        builder.AppendFormat("{0}{1}{2}Authority will know immediately{3}", "\n", "\n", colourBad, colourEnd);
                    }
                    else if (invisibility > 1)
                    {
                        builder.AppendFormat("{0}{1}{2}Authority will know in {3}{4}{5}{6}{7} turn{8}{9}", "\n", "\n", colourAlert, colourEnd, colourNeutral,
                            moveDetails.ai_Delay, colourEnd, colourAlert, moveDetails.ai_Delay != 1 ? "s" : "", colourEnd);
                    }
                    else
                    { builder.AppendFormat("{0}{1}{2}Authority will know next turn{3}", "\n", "\n", colourBad, colourEnd); }
                    //update invisibility
                    invisibility += moveDetails.changeInvisibility;
                    invisibility = Mathf.Max(0, invisibility);
                    GameManager.instance.playerScript.Invisibility = invisibility;
                    //AI message
                    Connection connection = GameManager.instance.dataScript.GetConnection(moveDetails.connectionID);
                    if (connection != null)
                    {
                        string textAI = string.Format("Player spotted moving to \"{0}\", {1}, ID {2}",
                            node.nodeName, node.Arc.name, moveDetails.nodeID);
                        Message messageAI = GameManager.instance.messageScript.AIConnectionActivity(textAI, moveDetails.nodeID, moveDetails.connectionID, moveDetails.ai_Delay);
                        GameManager.instance.dataScript.AddMessage(messageAI);
                        //AI Immediate message
                        if (GameManager.instance.aiScript.immediateFlagResistance == true)
                        {
                            Message messageImmediate = GameManager.instance.messageScript.AIImmediateActivity("Immediate Activity \"Move\" (Player)",
                                GameManager.instance.globalScript.sideAuthority, moveDetails.nodeID, moveDetails.connectionID);
                            GameManager.instance.dataScript.AddMessage(messageImmediate);
                        }
                    }
                    else { Debug.LogError(string.Format("Invalid connection (Null) for connectionID {0}", moveDetails.connectionID)); }
                }
                else
                {
                    builder.AppendLine();
                    builder.Append(string.Format("{0}Player not spotted{1}", colourGood, colourEnd));
                }
                //
                // - - - Gear - - -
                //
                if (moveDetails.gearID > -1)
                {
                    Gear gear = GameManager.instance.dataScript.GetGear(moveDetails.gearID);
                    if (gear != null)
                    {
                        builder.AppendFormat("{0}{1}{2}{3}{4}{5} used to minimise recognition{6}", "\n", "\n", colourNeutral, gear.name, colourEnd, colourNormal, colourEnd);
                        GameManager.instance.gearScript.SetGearUsed(gear, "move with as little recognition as possible");
                        MoveReturnData moveData = new MoveReturnData();
                        moveData.node = node;
                        moveData.text = builder.ToString();
                        ProcessMoveOutcome(moveData);
                    }
                    else { Debug.LogError(string.Format("Invalid Gear (Null) for gearID {0}", moveDetails.gearID)); }
                }
                else
                {
                    //No gear involved, move straight to outcome, otherwise skip outcome if connection has no security as unnecessary
                    if (moveDetails.changeInvisibility != 0)
                    {
                        MoveReturnData moveData = new MoveReturnData();
                        moveData.node = node;
                        moveData.text = builder.ToString();
                        ProcessMoveOutcome(moveData);
                    }
                    else { EventManager.instance.PostNotification(EventType.UseAction, this, "Player Move", "NodeManager.cs -> ProcessPlayerMove"); }
                }
            }
            else
            { Debug.LogError(string.Format("Invalid node (Null) for nodeID {0}", moveDetails.nodeID)); }
        }
        else
        { Debug.LogError("Invalid ModalMoveDetails (Null)"); }
    }


    /// <summary>
    /// ProcessPlayerMove -> ProcessMoveOutcome. Node checked for Null in calling procedure
    /// </summary>
    private void ProcessMoveOutcome(MoveReturnData data)
    {
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        //Erasure team picks up player immediately if invisibility 0
        CaptureDetails captureDetails = GameManager.instance.captureScript.CheckCaptured(data.node.nodeID, GameManager.instance.playerScript.actorID);
        if (captureDetails != null)
        {
            //Player captured!
            captureDetails.effects = string.Format("{0}The move went bad{1}", colourNeutral, colourEnd);
            EventManager.instance.PostNotification(EventType.Capture, this, captureDetails, "NodeManager.cs -> ProcessMoveOutcome");
        }
        //Normal Move  Outcome
        else
        {
            outcomeDetails.textTop = "Player has moved";
            outcomeDetails.textBottom = data.text;
            outcomeDetails.sprite = GameManager.instance.guiScript.alarmSprite;
            outcomeDetails.isAction = true;
            outcomeDetails.side = GameManager.instance.globalScript.sideResistance;
            outcomeDetails.reason = "Player Move";
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "NodeManager.cs -> ProcessMoveOutcome");
        }
    }


    /// <summary>
    /// Sets the node.isContact flag (true if any Resistance actor has a connection at node). Run everytime an actor changes status to keep flags up to date.
    /// </summary>
    public void SetNodeActorFlags()
    {
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetDictOfNodes();
        GlobalSide side = GameManager.instance.globalScript.sideResistance;
        if (dictOfNodes != null)
        {
            //set all to false
            foreach (var node in dictOfNodes)
            { node.Value.isContact = false; }
            //loop Resistance actors
            for (int slotID = 0; slotID < GameManager.instance.actorScript.maxNumOfOnMapActors; slotID++)
            {
                //check there an actor present in the slot
                if (GameManager.instance.dataScript.CheckActorSlotStatus(slotID, side) == true)
                {
                    Actor actor = GameManager.instance.dataScript.GetCurrentActor(slotID, side);
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
                                    { node.isContact = true; }
                                    else { Debug.LogError(string.Format("Invalid node (Null) for slotID {0}", slotID)); }
                                }

                            }
                            else { Debug.LogError(string.Format("Invalid listOfNodes (Null) for slotID {0}", slotID)); }
                        }
                    }
                    else { Debug.LogError(string.Format("Invalid Actor (null) for slotID {0}", slotID)); }
                }
            }
        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
    }

    /// <summary>
    /// loops all nodes and removes any ongoing effects that match the specified ID
    /// </summary>
    /// <param name="ongoingID"></param>
    public void RemoveOngoingEffect(int ongoingID)
    {
        if (ongoingID > -1)
        {
            Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetDictOfNodes();
            if (dictOfNodes != null)
            {
                foreach (var node in dictOfNodes)
                { node.Value.RemoveOngoingEffect(ongoingID); }
            }
            else { Debug.LogError("Invalid dictOfNodes (Null)"); }
        }
        else { Debug.LogError(string.Format("Invalid ongoingID {0} (must be zero or above)", ongoingID)); }
    }


    /// <summary>
    /// Decrement all ongoing Effect timers in nodes and delete any that have expired
    /// </summary>
    private void ProcessNodeTimers()
    {
        //Debug.LogWarning(string.Format("PROCESSNODETIMER: turn {0}{1}", GameManager.instance.turnScript.Turn, "\n"));
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetDictOfNodes();
        if (dictOfNodes != null)
        {
            foreach (var node in dictOfNodes)
            {
                node.Value.ProcessOngoingEffectTimers();
                node.Value.ProcessObserverTimers();
            }
        }
    }

    /// <summary>
    /// Debug method that randomly assigns activity data to nodes and connection for development purposes
    /// </summary>
    private void DebugRandomActivityValues()
    {
        int baseChance = 20;
        int counter = 0;
        //Nodes
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetDictOfNodes();
        if (dictOfNodes != null)
        {
            foreach (var node in dictOfNodes)
            {
                if (Random.Range(0, 100) < baseChance)
                {
                    node.Value.activityCount = Random.Range(1, 5);
                    node.Value.activityTime = Random.Range(0, 3);
                    counter++;
                }
            }
            Debug.Log(string.Format("DebugRandomActivity: {0} Nodes initiated{1}", counter, "\n"));
        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
        //Connections
        Dictionary<int, Connection> dictOfConnections = GameManager.instance.dataScript.GetAllConnections();
        if (dictOfConnections != null)
        {
            counter = 0;
            foreach (var conn in dictOfConnections)
            {
                if (Random.Range(0, 100) < baseChance)
                {
                    conn.Value.activityCount = Random.Range(1, 5);
                    conn.Value.activityTime = Random.Range(0, 3);
                    counter++;
                }
            }
            Debug.Log(string.Format("DebugRandomActivity: {0} Connections initiated{1}", counter, "\n"));
        }
        else { Debug.LogError("Invalid dictOfConnections (Null)"); }
    }

    /// <summary>
    /// returns AI delay, in turns, for being notified of rebel player moving through a connection where they lost invisibility doing so.
    /// </summary>
    /// <param name="secLvl"></param>
    /// <returns></returns>
    public int GetAIDelayForMove(ConnectionType secLvl)
    {
        int securityLevel;
        switch(secLvl)
        {
            case ConnectionType.HIGH: securityLevel = 3; break;
            case ConnectionType.MEDIUM: securityLevel = 2; break;
            case ConnectionType.LOW: securityLevel = 1; break;
            case ConnectionType.None:
            default:
                securityLevel = moveInvisibilityDelay; break;
        }
        int delay = moveInvisibilityDelay - securityLevel;
        delay = Mathf.Max(0, delay);
        return delay;
    }

    /// <summary>
    /// event driven -> start coroutine
    /// </summary>
    /// <param name="nodeID"></param>
    private void StartFlashingNode(int nodeID)
    {
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        if (node != null)
        {
            isFlashOn = false;
            myCoroutine = StartCoroutine("FlashingNode", node);
        }
        else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", nodeID); }
    }

    /// <summary>
    /// event driven -> stop coroutine
    /// </summary>
    private void StopFlashingNode()
    {
        if (myCoroutine != null)
        { StopCoroutine(myCoroutine); }
        ResetNodes();
    }

    /// <summary>
    /// coroutine to flash a node
    /// NOTE: Node checked for null by calling procedure
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    IEnumerator FlashingNode(Node node)
    {
        //forever loop
        for (; ;)
        {
            if (isFlashOn == false)
            {
                node.SetMaterial(materialActive);
                NodeRedraw = true;
                isFlashOn = true;
                yield return new WaitForSecondsRealtime(flashNodeTime);
            }
            else
            {
                node.SetMaterial(materialNormal);
                NodeRedraw = true;
                isFlashOn = false;
                yield return new WaitForSecondsRealtime(flashNodeTime);
            }
        }
    }

    /// <summary>
    /// Checks all nodes, checks for datapoints in dangerzone, updates crisis timers and runs crisis checks
    /// </summary>
    private void ProcessNodeCrisis()
    {
        
        Dictionary<int, Node> tempDict = GameManager.instance.dataScript.GetDictOfNodes();
        List<Node> listOfCrisisNodes = GameManager.instance.dataScript.GetListOfCrisisNodes();
        List<Node> tempList = new List<Node>(); //add all current crisis nodes in list then, once done, overwrite listOfCrisisNodes
        List<NodeDatapoint> listOfDatapoints = new List<NodeDatapoint>();
        if (tempDict != null)
        {
            if (listOfCrisisNodes != null)
            {
                int numOfDangerSigns;
                int chance;
                int rnd;
                string msgText;
                //loop all nodes
                foreach (var node in tempDict)
                {
                    if (node.Value != null)
                    {
                        numOfDangerSigns = 0;
                        if (node.Value.crisisTimer > 0)
                        {
                            //currently undergoing a CRISIS
                            if (node.Value.Security <= crisisSecurity) { numOfDangerSigns++; }
                            if (node.Value.Stability <= crisisStability) { numOfDangerSigns++; }
                            if (node.Value.Support >= crisisSupport) { numOfDangerSigns++; }
                            //check if crisis averted
                            if (numOfDangerSigns == 0)
                            {
                                //crisis AVERTED
                                node.Value.crisisTimer = 0;
                                node.Value.waitTimer = crisisWaitTimer;
                                node.Value.crisis = null;
                                //admin
                                msgText = string.Format("{0}, {1}, ID {2} crisis AVERTED", node.Value.nodeName, node.Value.Arc.name, node.Value.nodeID);
                                Message message = GameManager.instance.messageScript.NodeCrisis(msgText, node.Value.nodeID);
                                GameManager.instance.dataScript.AddMessage(message);
                            }
                            else
                            {
                                //Crisis CONTINUES
                                node.Value.crisisTimer--;
                                if (node.Value.crisisTimer > 0)
                                {
                                    //add to list of crisis nodes
                                    tempList.Add(node.Value);
                                    //warning message
                                    msgText = string.Format("{0} crisis in {1}, {2}, ({3} turn{4} left to Resolve)", node.Value.crisis.tag, node.Value.nodeName, node.Value.Arc.name,
                                        node.Value.crisisTimer, node.Value.crisisTimer != 1 ? "s" : "");
                                    Message message = GameManager.instance.messageScript.GeneralWarning(msgText);
                                    GameManager.instance.dataScript.AddMessage(message);
                                }
                                else
                                {
                                    //Crisis COMPLETED (goes Critical) -> lower city support
                                    int loyalty = GameManager.instance.cityScript.CityLoyalty;
                                    loyalty -= crisisCityLoyalty;
                                    loyalty = Mathf.Max(0, loyalty);
                                    GameManager.instance.cityScript.CityLoyalty = loyalty;
                                    //admin
                                    City city = GameManager.instance.cityScript.GetCity();
                                    string cityName = "Unknown";
                                    if (city != null) { cityName = city.name; }
                                    Debug.LogFormat("[Cit] NodeManager.cs -> ProcessNodeCrisis: {0} Loyalty -{1}, now {2}{3}", cityName, crisisCityLoyalty, loyalty, "\n");
                                    msgText = string.Format("{0}, {1} crisis ({2}), has EXPLODED", node.Value.nodeName, node.Value.Arc.name, node.Value.crisis.tag);
                                    Message message = GameManager.instance.messageScript.NodeCrisis(msgText, node.Value.nodeID, crisisCityLoyalty);
                                    GameManager.instance.dataScript.AddMessage(message);
                                    //set variables
                                    node.Value.waitTimer = crisisWaitTimer;
                                    node.Value.crisis = null;
                                }
                            }
                        }
                        else if (node.Value.waitTimer > 0)
                        {
                            //WAITING between potential crisis
                            node.Value.waitTimer--;
                            Debug.LogFormat("[Tst] NodeManager.cs -> ProcessNodeCrisis: {0}, ID {1}, waitTimer now {2}{3}", node.Value.Arc.name, node.Value.nodeID, node.Value.waitTimer, "\n");
                        }
                        else
                        {
                            //check for datapoints in the DANGER ZONE
                            if (node.Value.Security <= crisisSecurity) { numOfDangerSigns++; listOfDatapoints.Add(GameManager.instance.globalScript.nodeSecurity); }
                            if (node.Value.Stability <= crisisStability) { numOfDangerSigns++; listOfDatapoints.Add(GameManager.instance.globalScript.nodeStability); }
                            if (node.Value.Support >= crisisSupport) { numOfDangerSigns++; listOfDatapoints.Add(GameManager.instance.globalScript.nodeSupport); }
                            //Danger signs present
                            if (numOfDangerSigns > 0)
                            {
                                Debug.LogFormat("[Tst] NodeManager.cs -> ProcessNode: {0}, ID {1}, danger signs {2}{3}", node.Value.Arc.name, node.Value.nodeID, numOfDangerSigns, "\n");
                                chance = crisisBaseChance * numOfDangerSigns;
                                rnd = Random.Range(0, 100);
                                if (rnd < chance)
                                {
                                    //CRISIS COMMENCES
                                    node.Value.crisisTimer = crisisNodeTimer;
                                    node.Value.waitTimer = 0;
                                    //add to list of crisis nodes
                                    tempList.Add(node.Value);

                                    //Get crisis type
                                    NodeDatapoint datapoint;
                                    int numOfDatapoints = listOfDatapoints.Count;
                                    if (numOfDatapoints > 0)
                                    {
                                        //get random datapoint
                                        datapoint = listOfDatapoints[Random.Range(0, numOfDatapoints)];

                                    }
                                    else
                                    {
                                        //safety backup
                                        Debug.LogWarning("Invalid listOfDatapoints (Empty) -> default datapoint Stability provided");
                                        datapoint = GameManager.instance.globalScript.nodeStability;
                                    }
                                    node.Value.crisis = GameManager.instance.dataScript.GetRandomNodeCrisis(datapoint);
                                    if (node.Value.crisis == null)
                                    {
                                        //safety backup
                                        Debug.LogWarning("Invalid node.Value.crisis (Null) -> default value Crisis provided (nodeCrisisID 0)");
                                        node.Value.crisis = GameManager.instance.dataScript.GetNodeCrisisByID(0);
                                        //if backup failed, generate an error
                                        if (node.Value.crisis == null)
                                        { Debug.LogError("Invalid nodeCrisis default backup nodeCrisisID 0 (Null)"); }
                                    }
                                    //admin
                                    Debug.LogFormat("[Rnd] NodeManager.cs -> ProcessNodeCrisis: {0} ID {1}, CRISIS need < {2}, rolled {3}", node.Value.Arc.name, node.Value.nodeID, 
                                        chance, rnd);
                                    msgText = string.Format("{0}, {1}, ID {2} crisis COMMENCES ({3})", node.Value.nodeName, node.Value.Arc.name, node.Value.nodeID, node.Value.crisis.tag);
                                    Message message = GameManager.instance.messageScript.NodeCrisis(msgText, node.Value.nodeID);
                                    GameManager.instance.dataScript.AddMessage(message);
                                }
                                else
                                {
                                    //failed roll, nothing happens
                                    Debug.LogFormat("[Tst] NodeManager.cs -> ProcessNodeCrisis: {0} ID {1}, Failed need < {2}, rolled {3}", node.Value.Arc.name, node.Value.nodeID,
                                        chance, rnd);
                                }
                            }
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", node.Key); }
                }
                //update listOfCrisisNodes
                listOfCrisisNodes.Clear();
                listOfCrisisNodes.AddRange(tempList);
                Debug.LogFormat("[Tst] NodeManager.cs -> ProcessNodeCrisis: {0} records in listOfCrisisNodes{1}", listOfCrisisNodes.Count, "\n");
            }
            else { Debug.LogWarning("Invalid listOfCrisisNodes (Null)"); }
        }
        else { Debug.LogWarning("Invalid dictOfNodes (tempDict) (Null)"); }
    }

    //new methods above here
}