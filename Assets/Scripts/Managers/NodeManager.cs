using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using modalAPI;
using packageAPI;
using System.Text;
using System;
using Random = UnityEngine.Random;
using System.Linq;

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
    [Range(0,4)] public int maxNodeValue = 3;
    [Tooltip("Minimum value of a node datapoint")]
    [Range(0, 4)] public int minNodeValue = 0;
    [Tooltip("Low value of a node datapoint")]
    [Range(0, 4)] public int lowNodeValue = 1;
    [Tooltip("Medium value of a node datapoint")]
    [Range(0, 4)] public int medNodeValue = 2;

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
    [Tooltip("Number of smoke particles emitted per frame")]
    [Range(1, 100)] public int numOfSmokeParticles = 30;

    #region Save Data Compatible
    [HideInInspector] public int crisisPolicyModifier = 0;          //modifier to  crisisBaseChance due to Authority Policies, eg. "Curfew" 
    [HideInInspector] public int nodeIDCounter = 0;                   //sequentially numbers nodes (reset for each new level)
    [HideInInspector] public int connIDCounter = 0;                   //sequentially numbers connections (reset for each new level)
    [HideInInspector] public int nodeHighlight = -1;                //nodeID of currently highlighted node, if any, otherwise -1
    [HideInInspector] public int nodePlayer = -1;                   //nodeID of human Resistance/Authority player
    [HideInInspector] public int nodeNemesis = -1;                  //nodeID of nemesis
    [HideInInspector] public int nodeCaptured = -1;                 //nodeID where player has been captured, -1 if not
    #endregion

    private bool isFlashOn = false;                                 //used for flashing Node coroutine
    private bool showPlayerNode = true;                             //switched off if player node needs to be flashed


    private Coroutine myCoroutine;

    //fast access -> outcomes
    [HideInInspector] public EffectOutcome outcomeNodeSecurity;
    [HideInInspector] public EffectOutcome outcomeNodeStability;
    [HideInInspector] public EffectOutcome outcomeNodeSupport;
    [HideInInspector] public EffectOutcome outcomeStatusSpiders;
    [HideInInspector] public EffectOutcome outcomeStatusTracers;
    [HideInInspector] public EffectOutcome outcomeStatusContacts;
    [HideInInspector] public EffectOutcome outcomeStatusTeams;
    //traits
    private string crisisBaseChanceDoubled;
    private string crisisBaseChanceHalved;
    private string crisisTimerHigh;
    private string crisisTimerLow;
    private string crisisWaitTimerDoubled;
    private string crisisWaitTimerHalved;
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
    private Material materialNemesis;
    //flash
    private float flashNodeTime;

    //fast access
    private GlobalSide globalResistance;
    private GlobalSide globalAuthority;

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

    private int _nodeShowFlag = 0;                                   //true if a ShowNodes() is active, false otherwise
    private bool _nodeRedraw = false;                                //if true a node redraw is triggered in GameManager.Update

    //properties
    public int NodeShowFlag
    {
        get { return _nodeShowFlag; }
        set
        {
            _nodeShowFlag = value;
            /*Debug.LogFormat("[Tst] NodeManager.cs -> NodeShowFlag: {0}{1}", value, "\n");*/
        }
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

    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseAll();
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseAll();
                break;
            case GameState.LoadAtStart:
                SubInitialiseAll();
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadGame:
                SubInitialiseAll();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {            //fast access
        globalResistance = GameManager.instance.globalScript.sideResistance;
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        materialNormal = GetNodeMaterial(NodeType.Normal);
        materialHighlight = GetNodeMaterial(NodeType.Highlight);
        materialActive = GetNodeMaterial(NodeType.Active);
        materialPlayer = GetNodeMaterial(NodeType.Player);
        materialNemesis = GetNodeMaterial(NodeType.Nemesis);
        crisisBaseChanceDoubled = "NodeCrisisBaseChanceDoubled";
        crisisBaseChanceHalved = "NodeCrisisBaseChanceHalved";
        crisisTimerHigh = "NodeCrisisTimerHigh";
        crisisTimerLow = "NodeCrisisTimerLow";
        crisisWaitTimerDoubled = "NodeCrisisWaitTimerDoubled";
        crisisWaitTimerHalved = "NodeCrisisWaitTimerHalved";
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(materialNormal != null, "Invalid materialNormal (Null)");
        Debug.Assert(materialHighlight != null, "Invalid materialHighlight (Null)");
        Debug.Assert(materialActive != null, "Invalid materialActive (Null)");
        Debug.Assert(materialPlayer != null, "Invalid materialPlayer (Null)");
        Debug.Assert(materialNemesis != null, "Invalid materialNemesis (Null)");
        //flash
        flashNodeTime = GameManager.instance.guiScript.flashNodeTime;
        Debug.Assert(flashNodeTime > 0, "Invalid flashNodeTime (zero)");
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
            //register listener
            EventManager.instance.AddListener(EventType.NodeDisplay, OnEvent, "NodeManager");
            EventManager.instance.AddListener(EventType.ActivityDisplay, OnEvent, "NodeManager");
            EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "NodeManager");
            EventManager.instance.AddListener(EventType.CreateMoveMenu, OnEvent, "NodeManager");
            EventManager.instance.AddListener(EventType.CreateGearNodeMenu, OnEvent, "NodeManager");
            EventManager.instance.AddListener(EventType.MoveAction, OnEvent, "NodeManager");
            EventManager.instance.AddListener(EventType.StartTurnLate, OnEvent, "NodeManager");
            EventManager.instance.AddListener(EventType.FlashNodeStart, OnEvent, "NodeManager");
            EventManager.instance.AddListener(EventType.FlashNodeStop, OnEvent, "NodeManager");
    }
    #endregion

    #region SubInitialiseAll
    private void SubInitialiseAll()
    {
        //Set node contact flags (player side & non-player side)
        GameManager.instance.contactScript.UpdateNodeContacts();
        GameManager.instance.contactScript.UpdateNodeContacts(false);
    }
    #endregion

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        //find specific EffectOutcome SO's and assign to outcome fields
        EffectOutcome[] arrayOfEffectOutcome = GameManager.instance.loadScript.arrayOfEffectOutcome;
        if (arrayOfEffectOutcome != null)
        {
            for (int i = 0; i < arrayOfEffectOutcome.Length; i++)
            {
                //get SO
                EffectOutcome outcomeObject = arrayOfEffectOutcome[i];
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
        }
        else { Debug.LogWarning("Invalid arrayOfEffectOutcome (Null)"); }
        //check all found and assigned
        if (outcomeNodeSecurity == null) { Debug.LogError("Invalid outcomeNodeSecurity (Null)"); }
        if (outcomeNodeStability == null) { Debug.LogError("Invalid outcomeNodeStability (Null)"); }
        if (outcomeNodeSupport == null) { Debug.LogError("Invalid outcomeNodeSupport (Null)"); }
        if (outcomeStatusSpiders == null) { Debug.LogError("Invalid outcomeStatusSpiders (Null)"); }
        if (outcomeStatusTracers == null) { Debug.LogError("Invalid outcomeStatusTracers (Null)"); }
        if (outcomeStatusContacts == null) { Debug.LogError("Invalid outcomeStatusContacts (Null)"); }
        if (outcomeStatusTeams == null) { Debug.LogError("Invalid outcomeStatusTeams (Null)"); }
        //gear node Action Fast access
        actionKinetic = GameManager.instance.dataScript.GetAction("gearKinetic");
        actionHacking = GameManager.instance.dataScript.GetAction("gearHacking");
        actionPersuasion = GameManager.instance.dataScript.GetAction("gearPersuasion");
        Debug.Assert(actionKinetic != null, "Invalid actionKinetic (Null)");
        Debug.Assert(actionHacking != null, "Invalid actionHacking (Null)");
        Debug.Assert(actionPersuasion != null, "Invalid actionPersuasion (Null)");
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
                    case NodeUI.LoiterNodes:
                    case NodeUI.CureNodes:
                    case NodeUI.CrisisNodes:
                    case NodeUI.NodeArc0:
                    case NodeUI.NodeArc1:
                    case NodeUI.NodeArc2:
                    case NodeUI.NodeArc3:
                    case NodeUI.NodeArc4:
                    case NodeUI.NodeArc5:
                    case NodeUI.NodeArc6:
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
                CreateSpecialNodeMenu((int)Param);
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
                //Must be AFTER ProcessNodeTimers
                ProcessOngoingEffects();
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
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.whiteText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourHighlight = GameManager.instance.colourScript.GetColour(ColourType.neutralText);
        colourResistance = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badText);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralText);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodText);
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
    /// reset data ready for a new level
    /// </summary>
    public void ResetCounters()
    {
        //reset node & connection ID counters
        nodeIDCounter = 0;
        connIDCounter = 0;
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
        int counter;
        bool successFlag = true;
        bool nodeTypeFlag = false;
        bool proceedFlag = false;
        string displayText = null;
        bool isFogOfWar = GameManager.instance.optionScript.fogOfWar;
        //set all nodes to default colour first
        ResetNodes();
        if (GameManager.instance.connScript.resetConnections == true)
        {
            //return to previously saved state prior to any changes
            GameManager.instance.connScript.RestoreConnections();
        }
        //set nodes depending on critera
        switch (nodeUI)
        {
            //show all nodes with Targets
            case NodeUI.ShowTargets:
                List<Target> tempList = new List<Target>();
                if (isFogOfWar == false)
                {
                    //FOW Off
                    tempList.AddRange(GameManager.instance.dataScript.GetTargetPool(Status.Active));
                    tempList.AddRange(GameManager.instance.dataScript.GetTargetPool(Status.Live));
                    tempList.AddRange(GameManager.instance.dataScript.GetTargetPool(Status.Outstanding));
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
                }
                else
                {
                    //FOW ON
                    tempList.AddRange(GameManager.instance.dataScript.GetTargetPool(Status.Live));
                    tempList.AddRange(GameManager.instance.dataScript.GetTargetPool(Status.Outstanding));
                    if (tempList.Count > 0)
                    {
                        counter = 0;
                        foreach (Target target in tempList)
                        {
                            Node nodeTemp = GameManager.instance.dataScript.GetNode(target.nodeID);
                            //only show if target isKnown
                            if (nodeTemp != null && nodeTemp.isTargetKnown == true)
                            { nodeTemp.SetMaterial(materialActive); counter++; }
                            else { Debug.LogWarning(string.Format("Invalid node (Null) for target.nodeID {0}", target.nodeID)); }
                        }
                        displayText = string.Format("{0}{1}{2}{3} Target{4}{5} district{6}{7}", colourDefault, counter, colourEnd, colourHighlight, colourEnd,
                            colourDefault, tempList.Count != 1 ? "s" : "", colourEnd);
                    }
                    else { displayText = string.Format("{0}{1}{2}", colourError, "No Targets present", colourEnd); }
                }
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
                List<Node> listOfSpiderNodes = GameManager.instance.dataScript.GetListOfAllNodes();
                if (listOfSpiderNodes != null)
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
                            if (isFogOfWar == false)
                            { proceedFlag = true; }
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                            break;
                    }
                    foreach (Node node in listOfSpiderNodes)
                    {
                        if (node.isSpider == true)
                        {
                            //show all
                            if (proceedFlag == true)
                            {
                                node.SetMaterial(materialActive);
                                count++;
                            }
                            //conditional -> only show if spider is known
                            else
                            {
                                if (node.isSpiderKnown == true)
                                {
                                    node.SetMaterial(materialActive);
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
                else { Debug.LogError("Invalid listOfSpiderNodes (Null)"); }
                break;

            //show all nodes with a tracer or within one node radius of a tracer
            case NodeUI.ShowTracers:
                List<Node> listOfTracerNodes = GameManager.instance.dataScript.GetListOfAllNodes();
                if (listOfTracerNodes != null)
                {
                    int count = 0;
                    //determine level of visibility
                    switch (GameManager.instance.sideScript.PlayerSide.level)
                    {
                        case 1:
                            //Authority -> if not FOW then auto show
                            if (isFogOfWar == false)
                            { proceedFlag = true; }
                            break;
                        case 2:
                            //Resistance
                            proceedFlag = true;
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                            break;
                    }
                    foreach (Node node in listOfTracerNodes)
                    {
                        if (node.isTracer == true)
                        {
                            //show all
                            if (proceedFlag == true)
                            {
                                node.SetMaterial(materialActive);
                                count++;
                            }
                            //conditional -> only show if tracer is known
                            else
                            {
                                if (node.isTracerKnown == true)
                                {
                                    node.SetMaterial(materialActive);
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
                else { Debug.LogError("Invalid listOfTracerNodes (Null)"); }
                break;

            //show all nodes containng a Team
            case NodeUI.ShowTeams:
                List<int> listOfTeams = GameManager.instance.dataScript.GetTeamPool(TeamPool.OnMap);
                if (listOfTeams != null)
                {
                    int count = 0;
                    //determine level of visibility
                    switch (GameManager.instance.sideScript.PlayerSide.level)
                    {
                        case 1:
                            //Authority
                            proceedFlag = true;
                            break;
                        case 2:
                            //resistance -> if not FOW then auto show
                            if (isFogOfWar == false)
                            { proceedFlag = true; }
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid side \"{0}\"", GameManager.instance.sideScript.PlayerSide));
                            break;
                    }
                    List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
                    if (listOfNodes != null)
                    {
                        foreach (Node node in listOfNodes)
                        {
                            if (node.CheckNumOfTeams() > 0)
                            {
                                //show all
                                if (proceedFlag == true)
                                {
                                    node.SetMaterial(materialActive);
                                    count++;
                                }
                                //conditional -> only show if team is known, actor has contacts or node within tracer coverage
                                else
                                {
                                    if (node.isTeamKnown || node.isTracer || node.isContactResistance)
                                    {
                                        node.SetMaterial(materialActive);
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                    else { Debug.LogError("Invalid listOfNodes (Null)"); }
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
                    else { displayText = string.Format("{0}{1}{2}", colourError, "0 Most Connected Districts present", colourEnd); }
                }
                else
                {
                    Debug.LogWarning("Invalid connectedList (Null)");
                    displayText = string.Format("{0}{1}{2}", colourError, "ERROR: Can't find Connected districts", colourEnd);
                }
                break;
            case NodeUI.LoiterNodes:
                List<Node> loiterList = GameManager.instance.dataScript.GetListOfLoiterNodes();
                if (loiterList != null)
                {
                    if (loiterList.Count > 0)
                    {
                        foreach (Node node in loiterList)
                        {
                            if (node != null)
                            { node.SetMaterial(materialActive); }
                            else { Debug.LogWarning("Invalid node (Null)"); }
                        }
                        displayText = string.Format("{0}{1}{2}{3} Loiter district{4}{5}", colourDefault, loiterList.Count, colourEnd, colourHighlight,
                            loiterList.Count != 1 ? "s" : "", colourEnd);
                    }
                    else { displayText = string.Format("{0}{1}{2}", colourError, "0 Loiter Districts present", colourEnd); }
                }
                else
                {
                    Debug.LogWarning("Invalid loiterList (Null)");
                    displayText = string.Format("{0}{1}{2}", colourError, "ERROR: Can't find Loiter districts", colourEnd);
                }
                break;
            case NodeUI.CureNodes:
                //NOTE: will only show nodes if a cure is present AND is Active
                counter = 0;
                List<Node> cureList = GameManager.instance.dataScript.GetListOfCureNodes();
                if (cureList != null)
                {
                    if (cureList.Count > 0)
                    {
                        foreach (Node node in cureList)
                        {
                            if (node != null)
                            {
                                if (node.cure.isActive == true)
                                { node.SetMaterial(materialActive); counter++; }
                            }
                            else { Debug.LogWarning("Invalid node (Null)"); }
                        }
                        displayText = string.Format("{0}{1}{2}{3} Cure district{4}{5}", colourDefault, counter, colourEnd, colourHighlight, counter != 1 ? "s" : "", colourEnd);
                    }
                    else { displayText = string.Format("{0}{1}{2}", colourError, "0 Cure Districts present", colourEnd); }
                }
                else
                {
                    Debug.LogWarning("Invalid cureList (Null)");
                    displayText = string.Format("{0}{1}{2}", colourError, "ERROR: Can't find Cure districts", colourEnd);
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
                    else { displayText = string.Format("{0}{1}{2}", colourError, "0 Decision Districts present", colourEnd); }
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
                            {
                                node.SetMaterial(materialActive);
                            }
                            else { Debug.LogWarning("Invalid node (Null)"); }
                        }
                        displayText = string.Format("{0}{1}{2}{3} Crisis district{4}{5}", colourDefault, crisisList.Count, colourEnd, colourHighlight,
                            crisisList.Count != 1 ? "s" : "", colourEnd);
                    }
                    else { displayText = string.Format("{0}{1}{2}", colourError, "0 Crisis Districts present", colourEnd); }
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
                        else { displayText = string.Format("{0}{1}{2}", colourError, "0 Near Neighbours present", colourEnd); }
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
                List<Node> listOfCentreNodes = GameManager.instance.dataScript.GetListOfAllNodes();
                if (listOfCentreNodes != null)
                {
                    if (listOfCentreNodes.Count > 0)
                    {
                        counter = 0;
                        foreach (Node node in listOfCentreNodes)
                        {
                            if (node != null)
                            {
                                if (node.isCentreNode == true)
                                {
                                    node.SetMaterial(materialActive);
                                    counter++;
                                }
                            }
                            else { Debug.LogWarning("Invalid node (Null)"); }
                        }
                        displayText = string.Format("{0}{1}{2}{3} Centred district{4}{5}", colourDefault, counter, colourEnd, colourHighlight,
                            listOfCentreNodes.Count != 1 ? "s" : "", colourEnd);
                    }
                    else { displayText = string.Format("{0}{1}{2}", colourError, "0 Centre Districts present", colourEnd); }
                }
                else
                {
                    Debug.LogWarning("Invalid listOfCentreNodes (Null)");
                    displayText = string.Format("{0}{1}{2}", colourError, "ERROR: Null listOfCentreNodes", colourEnd);
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
            { GameManager.instance.alertScript.SetAlertUI(displayText, 999); }
        }
    }

    /// <summary>
    /// Show all active nodes (Contacts) for a particular actor. Use actor.slotID (0 to numOfActors)
    /// </summary>
    /// <param name="slotID"></param>
    public void ShowActiveNodes(int slotID)
    {
        Debug.Assert(slotID > -1 && slotID < GameManager.instance.actorScript.maxNumOfOnMapActors, "Invalid slotID");
        //set all nodes to default colour first
        ResetNodes();

        /*List<GameObject> tempList = GameManager.instance.dataScript.GetListOfActorNodes(slotID);
        foreach (GameObject obj in tempList)
        {
            Node nodeTemp = obj.GetComponent<Node>();
            nodeTemp.SetMaterial(materialActive);
        }*/

        List<Node> tempNodeList = GameManager.instance.dataScript.GetListOfActorContactNodes(slotID);
        foreach(Node node in tempNodeList)
        {
            //change material for selected nodes
            node.SetMaterial(materialActive);
        }

        //Get Actor
        Actor actor = GameManager.instance.dataScript.GetCurrentActor(slotID, GameManager.instance.sideScript.PlayerSide);
        string displayText;
        string minionTitle;
        //work out minion's appropriate title
        if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level)
        { minionTitle = string.Format("{0} of ", GameManager.instance.metaScript.GetAuthorityTitle()); }
        else { minionTitle = "Rebel "; }
        if (actor != null)
        {
            displayText = string.Format("{0}\"{1}\"{2} {3}{4}{5}{6}{7}{8} {9}{10} district{11}{12}", colourHighlight, actor.actorName, colourEnd,
                colourDefault, minionTitle, colourEnd,
                colourHighlight, actor.arc.name, colourEnd,
                colourDefault, tempNodeList.Count, tempNodeList.Count != 1 ? "s" : "", colourEnd);
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
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        if (listOfNodes != null)
        {
            //loop all nodes & assign current materials to their renderers (changes colour on screen)
            foreach (Node node in listOfNodes)
            {
                nodeRenderer = node.GetComponent<Renderer>();
                nodeRenderer.material = node._Material;
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
                else { Debug.LogError("Invalid Node (null) returned from listOfNodes"); }
            }
            //show player node if flag is true (default condition -> would be false only in case of flashing player Node)
            if (showPlayerNode == true)
            {
                //player's current node (Resistance side only if FOW ON)
                if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level)
                {
                    if (GameManager.instance.optionScript.fogOfWar == true)
                    { proceedFlag = false; }
                }
                if (proceedFlag == true)
                {
                    //Player node
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
                //Nemesis current node (Resistance side only if FOW ON & Nemesis present)
                proceedFlag = true;
                if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level)
                {
                    //Nemesis has a separate FOW setting
                    if (GameManager.instance.nemesisScript.isShown == false)
                    { proceedFlag = false; }
                    else if (GameManager.instance.nemesisScript.nemesis == null)
                    { proceedFlag = false; }
                }
                if (proceedFlag == true)
                {
                    //Nemesis ndoe
                    if (nodeNemesis > -1)
                    {
                        Node node = GameManager.instance.dataScript.GetNode(nodeNemesis);
                        if (node != null)
                        {
                            //only do so if it's a normal node, otherwise ignore
                            if (node.GetMaterial() == materialNormal)
                            {
                                nodeRenderer = node.GetComponent<Renderer>();
                                nodeRenderer.material = materialNemesis;
                            }
                        }
                    }
                }
            }
            //reset flag to prevent constant redraws
            NodeRedraw = false;
        }
        else { Debug.LogError("Invalid listOfNodes (Null) returned from dataManager in RedrawNodes"); }
    }

    /// <summary>
    /// Sets the material colour of all nodes to the default (doesn't change on screen, just sets them up). Call before making any changes to node colours
    /// </summary>
    public void ResetNodes()
    {
        //loop and assign
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        if (listOfNodes != null)
        {
            foreach (Node node in listOfNodes)
            {
                node.SetMaterial(materialNormal);
                node.faceText.text = "";
            }
            //trigger an automatic redraw
            NodeRedraw = true;
        }
        else { Debug.LogError("Invalid listOfNodes (Null) returned from dataManager in ResetNodes"); }

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
            List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
            if (listOfNodes != null)
            {
                //loop nodes
                bool errorFlag = false;
                foreach(Node node in listOfNodes)
                {
                    if (node != null)
                    {
                        if (node.faceObject != null)
                        {
                            switch(activityUI)
                            {
                                case ActivityUI.Count:
                                case ActivityUI.Time:
                                    data = node.GetNodeActivity(activityUI);
                                    if (data > -1)
                                    {
                                        node.faceText.text = data.ToString();
                                        node.faceText.color = GetActivityColour(activityUI, data);
                                    }
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid activityUI \"{0}\"", activityUI));
                                    errorFlag = true;
                                    break;
                            }
                        }
                        else { Debug.LogWarning(string.Format("Invalid node faceObject (Null) for nodeID {0}", node.nodeID)); }
                        if (errorFlag == true)
                        { break; }
                    }
                    else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", node.nodeID)); }
                }
            }
            else { Debug.LogError("Invalid listOfNodes (Null)"); }
        }
    }

    /// <summary>
    /// Displays nodeID's on node faces
    /// </summary>
    /// <param name="highlightID"></param>
    public void ShowAllNodeID()
    {
        int data;
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        if (listOfNodes != null)
        {
            //loop nodes
            foreach (Node node in listOfNodes)
            {
                if (node != null)
                {
                    if (node.faceObject != null)
                    {
                        data = node.nodeID;
                        node.faceText.text = data.ToString();
                        node.faceText.color = Color.yellow;
                    }
                    else { Debug.LogWarning(string.Format("Invalid node faceObject (Null) for nodeID {0}", node.nodeID)); }
                }
                else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", node.nodeID)); }
            }
        }
        else { Debug.LogError("Invalid listOfNodes (Null)"); }
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
                    List<string> listOfGear = GameManager.instance.playerScript.GetListOfGear();
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
                                if (gear.type.name.Equals(movement, StringComparison.Ordinal) == true)
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
                                        if (GameManager.instance.playerScript.Invisibility <= 0)
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
                                        GameManager.instance.gearScript.GetChanceOfCompromise(gear.name), colourEnd, colourAlert, colourEnd));

                                    //Move details
                                    moveGearDetails.nodeID = nodeID;
                                    moveGearDetails.connectionID = connection.connID;
                                    moveGearDetails.changeInvisibility = adjustInvisibility;
                                    moveGearDetails.gearName = gear.name;
                                    //button target details (red for High security to match red connection security colour on map)
                                    string colourGearLevel = colourNeutral;
                                    if (gear.data == 3) { colourGearLevel = colourGood; }
                                    else if (gear.data == 1) { colourGearLevel = colourBad; }
                                    EventButtonDetails eventMoveDetails = new EventButtonDetails()
                                    {
                                        buttonTitle = string.Format("{0} Move", gear.tag),
                                        buttonTooltipHeader = string.Format("Move using{0}{1}{2}{3}{4}{5}{6}{7}", "\n", colourNeutral, gear.tag, colourEnd,
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
                    if (GameManager.instance.playerScript.Invisibility <= 0)
                    {
                        //invisibility <= zero, if move. Immediate notification
                        moveDetail = string.Format("{0}Invisibility -1{1}<size=110%>Authority will know Immediately</size>{2}", colourBad, "\n",
                          colourEnd);
                        moveDetails.ai_Delay = 0;
                    }
                    else
                    {
                        //invisibility reduces, zero or above
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
                moveDetails.gearName = null;
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
        ModalGenericMenuDetails details = new ModalGenericMenuDetails()
        {
            itemID = nodeID,
            itemName = node.nodeName,
            itemDetails = string.Format("{0} ID {1}", node.Arc.name, node.nodeID),
            menuPos = node.transform.position,
            listOfButtonDetails = tempList,
            menuType = ActionMenuType.Move
        };
        //activate menu
        GameManager.instance.actionMenuScript.SetActionMenu(details);
    }

    /// <summary>
    /// Right Click the Resistance Player's current node -> special actions (Gear & Cure)
    /// </summary>
    /// <param name="nodeID"></param>
    private void CreateSpecialNodeMenu(int nodeID)
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
                                                if (effect.outcome.name.Equals("Renown", StringComparison.Ordinal) == false && effect.outcome.name.Equals("Invisibility", StringComparison.Ordinal) == false)
                                                { builder.Append(string.Format("{0}{1}{2}", colourEffect, effect.description, colourEnd)); }
                                                else
                                                {
                                                    //Invisibility and Renown -> player affected (good for renown, bad for invisibility)
                                                    if (effect.outcome.name.Equals("Renown", StringComparison.Ordinal) == true)
                                                    { builder.Append(string.Format("{0}Player {1}{2}", colourGood, effect.description, colourEnd)); }
                                                    else
                                                    { builder.Append(string.Format("{0}Player {1}{2}", colourBad, effect.description, colourEnd)); }
                                                }
                                                //Mood (special case)
                                                if (effect.isMoodEffect == true)
                                                {
                                                    string moodText = GameManager.instance.personScript.GetMoodTooltip(effect.belief, "Player");
                                                    builder.Append(moodText);
                                                }
                                            }
                                            else
                                            {
                                                //invalid effect criteria -> Action cancelled
                                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                                infoBuilder.Append(string.Format("{0}{1} action invalid{2}{3}{4}({5}){6}",
                                                    colourInvalid, kineticGear.tag, "\n", colourEnd,
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
                                        actionDetails.gearName = kineticGear.name;
                                        //pass all relevant details to ModalActionMenu via Node.OnClick()
                                        EventButtonDetails kineticDetails = new EventButtonDetails()
                                        {
                                            buttonTitle = string.Format("Use {0}", kineticGear.tag),
                                            buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, kineticGear.tag, colourEnd),
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
                                                if (effect.outcome.name.Equals("Renown", StringComparison.Ordinal) == false && effect.outcome.name.Equals("Invisibility", StringComparison.Ordinal) == false)
                                                { builder.Append(string.Format("{0}{1}{2}", colourEffect, effect.description, colourEnd)); }
                                                else
                                                {
                                                    //Invisibility and Renown -> player affected (good for renown, bad for invisibility)
                                                    if (effect.outcome.name.Equals("Renown", StringComparison.Ordinal) == true)
                                                    { builder.Append(string.Format("{0}Player {1}{2}", colourGood, effect.description, colourEnd)); }
                                                    else
                                                    { builder.Append(string.Format("{0}Player {1}{2}", colourBad, effect.description, colourEnd)); }
                                                }
                                                //Mood (special case)
                                                if (effect.isMoodEffect == true)
                                                {
                                                    string moodText = GameManager.instance.personScript.GetMoodTooltip(effect.belief, "Player");
                                                    builder.Append(moodText);
                                                }
                                            }
                                            else
                                            {
                                                //invalid effect criteria -> Action cancelled
                                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                                infoBuilder.Append(string.Format("{0}{1} action invalid{2}{3}{4}({5}){6}",
                                                    colourInvalid, hackingGear.tag, "\n", colourEnd,
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
                                        actionDetails.gearName = hackingGear.name;
                                        //pass all relevant details to ModalActionMenu via Node.OnClick()
                                        EventButtonDetails hackingDetails = new EventButtonDetails()
                                        {
                                            buttonTitle = string.Format("Use {0}", hackingGear.tag),
                                            buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, hackingGear.tag, colourEnd),
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
                                                if (effect.outcome.name.Equals("Renown", StringComparison.Ordinal) == false && effect.outcome.name.Equals("Invisibility", StringComparison.Ordinal) == false)
                                                { builder.Append(string.Format("{0}{1}{2}", colourEffect, effect.description, colourEnd)); }
                                                else
                                                {
                                                    //Invisibility and Renown -> player affected (good for renown, bad for invisibility)
                                                    if (effect.outcome.name.Equals("Renown", StringComparison.Ordinal))
                                                    { builder.Append(string.Format("{0}Player {1}{2}", colourGood, effect.description, colourEnd)); }
                                                    else
                                                    { builder.Append(string.Format("{0}Player {1}{2}", colourBad, effect.description, colourEnd)); }
                                                }
                                                //Mood (special case)
                                                if (effect.isMoodEffect == true)
                                                {
                                                    string moodText = GameManager.instance.personScript.GetMoodTooltip(effect.belief, "Player");
                                                    builder.Append(moodText);
                                                }
                                            }
                                            else
                                            {
                                                //invalid effect criteria -> Action cancelled
                                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                                infoBuilder.Append(string.Format("{0}{1} action invalid{2}{3}{4}({5}){6}",
                                                    colourInvalid, persuasionGear.tag, "\n", colourEnd,
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
                                        actionDetails.gearName = persuasionGear.name;
                                        //pass all relevant details to ModalActionMenu via Node.OnClick()
                                        EventButtonDetails persuasionDetails = new EventButtonDetails()
                                        {
                                            buttonTitle = string.Format("Use {0}", persuasionGear.tag),
                                            buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, persuasionGear.tag, colourEnd),
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
                // - - - Cure - - -
                //
                //if node has an active cure then it must be required for one of the Player's conditions
                if (node.cure != null && node.cure.isActive == true)
                {
                    ModalActionDetails cureActionDetails = new ModalActionDetails();
                    cureActionDetails.side = globalResistance;
                    cureActionDetails.nodeID = nodeID;
                    cureActionDetails.actorDataID = GameManager.instance.playerScript.actorID;
                    cureActionDetails.renownCost = 0;
                    EventButtonDetails cureDetails = new EventButtonDetails()
                    {
                        buttonTitle = node.cure.cureName,
                        buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, "Cure Condition", colourEnd),
                        buttonTooltipMain = string.Format("Remove {0}{1}{2} condition", colourBad, node.cure.condition.tag, colourEnd),
                        buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, node.cure.tooltipText, colourEnd),
                        //use a Lambda to pass arguments to the action
                        action = () => { EventManager.instance.PostNotification(EventType.CurePlayerAction, this, cureActionDetails, "NodeManager.cs -> CreateGearMenu"); }
                    };
                    //add Activate button to list
                    tempList.Add(cureDetails);
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
        ModalGenericMenuDetails details = new ModalGenericMenuDetails()
        {
            itemID = nodeID,
            itemName = string.Format("{0}", node.nodeName),
            itemDetails = string.Format("{0}District Special Actions{1}", colourResistance, colourEnd),
            menuPos = node.transform.position,
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

            /*Debug.LogFormat("[Tst] NodeManager.cs -> ProcessPlayerMove: ModalMoveDetails nodeID {0}, change {1}, delay {2}{3}", moveDetails.nodeID, moveDetails.changeInvisibility, moveDetails.ai_Delay, "\n");*/

            Node node = GameManager.instance.dataScript.GetNode(moveDetails.nodeID);
            if (node != null)
            {
                //update Player node
                nodePlayer = moveDetails.nodeID;
                //update move list
                node.SetPlayerMoveNodes();
                //message
                if (moveDetails.changeInvisibility != 0)
                {
                    Debug.LogFormat("[Ply] NodeManager.cs -> ProcessPlayerMove: Player moves to node {0}, {1}, nodeID {2}, SPOTTED, AI knows in {3} turn{4}{5}", node.nodeName, node.Arc.name, node.nodeID,
                      moveDetails.ai_Delay, moveDetails.ai_Delay != 1 ? "s" : "", "\n");
                }
                else { Debug.LogFormat("[Ply] NodeManager.cs -> ProcessPlayerMove: Player moves to node {0}, {1}, nodeID {2}{3}", node.nodeName, node.Arc.name, node.nodeID, "\n"); }
                string destination = string.Format("\"{0}\", {1} district", node.nodeName, node.Arc.name);
                StringBuilder builder = new StringBuilder();
                builder.Append(string.Format("{0}{1}", destination, "\n"));
                //message
                string text = string.Format("Player has moved to {0}", destination);
                GameManager.instance.messageScript.PlayerMove(text, node, moveDetails.changeInvisibility, moveDetails.ai_Delay);
                //
                // - - - Invisibility - - -
                //
                if (moveDetails.changeInvisibility != 0)
                {
                    Connection connection = GameManager.instance.dataScript.GetConnection(moveDetails.connectionID);
                    if (connection != null)
                    {
                        //AI message
                        string textAI = string.Format("Player spotted moving to \"{0}\", {1}, ID {2}", node.nodeName, node.Arc.name, moveDetails.nodeID);
                        //display
                        builder.AppendLine();
                        builder.AppendFormat("{0}Invisibility {1}{2}{3}", colourBad, moveDetails.changeInvisibility > 0 ? "+" : "",
                            moveDetails.changeInvisibility, colourEnd);
                        //player invisibility
                        int invisibility = GameManager.instance.playerScript.Invisibility;
                        if (invisibility <= 0)
                        {
                            //moving while invisibility already 0 triggers immediate alert flag
                            GameManager.instance.aiScript.immediateFlagResistance = true;
                            builder.AppendFormat("{0}{1}{2}Authority will know immediately{3}", "\n", "\n", colourBad, colourEnd);
                            //AI Immediate notification
                            GameManager.instance.messageScript.AIImmediateActivity("Immediate Activity \"Move\" (Player)",
                                "Moving", moveDetails.nodeID, moveDetails.connectionID);
                        }
                        else
                        {
                            //normal adjusted security level delay, eg. low gives a 3 turn delay
                            builder.AppendFormat("{0}{1}{2}Authority will know in {3}{4}{5}{6}{7} turn{8}{9}", "\n", "\n", colourAlert, colourEnd, colourNeutral,
                                moveDetails.ai_Delay, colourEnd, colourAlert, moveDetails.ai_Delay != 1 ? "s" : "", colourEnd);
                            //AI delayed notification
                            GameManager.instance.messageScript.AIConnectionActivity(textAI, node, connection, moveDetails.ai_Delay);
                        }
                        //update invisibility
                        invisibility += moveDetails.changeInvisibility;
                        invisibility = Mathf.Max(0, invisibility);
                        GameManager.instance.playerScript.Invisibility = invisibility;
                    }
                    else { Debug.LogErrorFormat("Invalid connection (Null) for connectionID {0}", moveDetails.connectionID); }
                }
                else
                {
                    builder.AppendLine();
                    builder.Append(string.Format("{0}Player not spotted{1}", colourGood, colourEnd));
                }
                //
                // - - - Gear - - -
                //
                if (string.IsNullOrEmpty(moveDetails.gearName) == false)
                {
                    Gear gear = GameManager.instance.dataScript.GetGear(moveDetails.gearName);
                    if (gear != null)
                    {
                        builder.AppendFormat("{0}{1}{2}{3}{4}{5} used to minimise recognition{6}", "\n", "\n", colourNeutral, gear.tag, colourEnd, colourNormal, colourEnd);
                        GameManager.instance.gearScript.SetGearUsed(gear, "move with as little recognition as possible");
                        MoveReturnData moveData = new MoveReturnData();
                        moveData.node = node;
                        moveData.text = builder.ToString();
                        ProcessMoveOutcome(moveData);
                    }
                    else { Debug.LogError(string.Format("Invalid Gear (Null) for gear {0}", moveDetails.gearName)); }
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
                    else
                    {
                        //Unsecured connection, no invisibility loss involved
                        EventManager.instance.PostNotification(EventType.UseAction, this, "Player Move", "NodeManager.cs -> ProcessPlayerMove");
                        //Nemesis, if at same node, can interact and damage player
                        GameManager.instance.nemesisScript.CheckNemesisAtPlayerNode(true);
                    }
                }
                //Tracker Data
                HistoryRebelMove history = new HistoryRebelMove();
                history.turn = GameManager.instance.turnScript.Turn;
                history.playerNodeID = moveDetails.nodeID;
                history.invisibility = GameManager.instance.playerScript.Invisibility;
                history.nemesisNodeID = GameManager.instance.nodeScript.nodeNemesis;
                GameManager.instance.dataScript.AddHistoryRebelMove(history);
            }
            else
            { Debug.LogError(string.Format("Invalid node (Null) for nodeID {0}", moveDetails.nodeID)); }
        }
        else
        { Debug.LogError("Invalid ModalMoveDetails (Null)"); }
    }


    /// <summary>
    /// ProcessPlayerMove -> ProcessMoveOutcome. Node checked for Null in calling procedure. Checks for presence of Erasure team and Nemesis in destination node
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
        else
        {
            //Normal Move  Outcome
            outcomeDetails.textTop = string.Format("You have been {0}DETECTED{1} moving to", colourBad, colourEnd);
            outcomeDetails.textBottom = data.text;
            outcomeDetails.sprite = GameManager.instance.guiScript.alarmSprite;
            outcomeDetails.isAction = true;
            outcomeDetails.side = globalResistance;
            outcomeDetails.reason = "Player Move";
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "NodeManager.cs -> ProcessMoveOutcome");
            //Nemesis, if at same node, can interact and damage player
            GameManager.instance.nemesisScript.CheckNemesisAtPlayerNode(true);
        }
    }

    /// <summary>
    /// loops all nodes and removes any ongoing effects that match the specified ID
    /// </summary>
    /// <param name="ongoingID"></param>
    public void RemoveOngoingEffect(int ongoingID)
    {
        if (ongoingID > -1)
        {
            List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
            if (listOfNodes != null)
            {
                foreach (Node node in listOfNodes)
                { node.RemoveOngoingEffect(ongoingID); }
            }
            else { Debug.LogError("Invalid listOfNodes (Null)"); }
        }
        else { Debug.LogError(string.Format("Invalid ongoingID {0} (must be zero or above)", ongoingID)); }
    }


    /// <summary>
    /// Decrement all ongoing Effect timers in nodes and delete any that have expired
    /// </summary>
    private void ProcessNodeTimers()
    {
        //Debug.LogWarning(string.Format("PROCESSNODETIMER: turn {0}{1}", GameManager.instance.turnScript.Turn, "\n"));
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        if (listOfNodes != null)
        {
            foreach (Node node in listOfNodes)
            {
                node.ProcessOngoingEffectTimers();
                node.ProcessObserverTimers();
            }
        }
    }

    /// <summary>
    /// Generates messages for 'Effect' tab in InfoApp
    /// </summary>
    private void ProcessOngoingEffects()
    {
        string text;
        //Ongoing node effects
        Dictionary<int, EffectDataOngoing> dictOfOngoingEffects = GameManager.instance.dataScript.GetDictOfOngoingEffects();
        if (dictOfOngoingEffects != null)
        {
            int count = dictOfOngoingEffects.Count;
            if (count > 0)
            {
                foreach(var ongoing in dictOfOngoingEffects)
                {
                    //message
                    if (ongoing.Value.nodeID > -1)
                    { GameManager.instance.messageScript.MessageOngoingEffectCurrentNode(ongoing.Value); }
                    else { Debug.LogWarningFormat("Invalid ongoingEffect for {0}, ID {1}", ongoing.Value.text, ongoing.Key); }
                }
            }
        }
        else { Debug.LogWarning("Invalid dictOfOngoingEffects (Null)"); }
        //Nodes with a valid Crisis wait period
        Dictionary<int, Node> tempDict = GameManager.instance.dataScript.GetDictOfNodes();
        if (tempDict != null)
        {
            foreach (var node in tempDict)
            {
                if (node.Value.waitTimer > 0)
                {
                    //Info App ongoing effect message
                    text = string.Format("{0}, {1}, id {2} district cannot have a crisis for another {3} turn{4}", node.Value.nodeName, node.Value.Arc.name, node.Value.nodeID, node.Value.waitTimer,
                        node.Value.waitTimer != 1 ? "s" : "");
                    GameManager.instance.messageScript.NodeOngoingEffect(text, node.Value);
                }
            }
        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
    }

    /// <summary>
    /// Debug method that randomly assigns activity data to nodes and connection for development purposes
    /// </summary>
    private void DebugRandomActivityValues()
    {
        int baseChance = 20;
        int counter = 0;
        //Nodes
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        if (listOfNodes != null)
        {
            foreach (Node node in listOfNodes)
            {
                if (Random.Range(0, 100) < baseChance)
                {
                    node.activityCount = Random.Range(1, 5);
                    node.activityTime = Random.Range(0, 3);
                    counter++;
                }
            }
            Debug.Log(string.Format("DebugRandomActivity: {0} Nodes initiated{1}", counter, "\n"));
        }
        else { Debug.LogError("Invalid listOfNodes (Null)"); }
        //Connections
        List<Connection> listOfConnections = GameManager.instance.dataScript.GetListOfConnections();
        if (listOfConnections != null)
        {
            counter = 0;
            foreach (Connection conn in listOfConnections)
            {
                if (Random.Range(0, 100) < baseChance)
                {
                    conn.activityCount = Random.Range(1, 5);
                    conn.activityTime = Random.Range(0, 3);
                    counter++;
                }
            }
            Debug.Log(string.Format("DebugRandomActivity: {0} Connections initiated{1}", counter, "\n"));
        }
        else { Debug.LogError("Invalid listOfConnections (Null)"); }
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
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        List<Node> listOfCrisisNodes = GameManager.instance.dataScript.GetListOfCrisisNodes();
        List<Node> tempList = new List<Node>(); //add all current crisis nodes in list then, once done, overwrite listOfCrisisNodes
        List<NodeDatapoint> listOfDatapoints = new List<NodeDatapoint>();
        if (listOfNodes != null)
        {
            if (listOfCrisisNodes != null)
            {
                //city
                City city = GameManager.instance.cityScript.GetCity();
                if (city != null)
                {
                    int numOfDangerSigns;
                    int chance;
                    int rnd;
                    string msgText, itemText;
                    //wait timer
                    int nodeWaitTimer = crisisWaitTimer;
                    //wait timer -> traits
                    if (city.mayor.CheckTraitEffect(crisisWaitTimerDoubled) == true) { nodeWaitTimer *= 2; }
                    else if (city.mayor.CheckTraitEffect(crisisWaitTimerHalved) == true) { nodeWaitTimer /= 2; }
                    //base crisis chance
                    int nodeBaseChance = crisisBaseChance;
                    //base crisis chance -> traits
                    if (city.mayor.CheckTraitEffect(crisisBaseChanceDoubled) == true) { nodeBaseChance *= 2; }
                    else if (city.mayor.CheckTraitEffect(crisisBaseChanceHalved) == true) { nodeBaseChance /= 2; }
                    //crisis timer
                    int nodeTimer = crisisNodeTimer;
                    //crisis timer -> traits
                    if (city.mayor.CheckTraitEffect(crisisTimerHigh) == true) { nodeTimer += 1; }
                    else if (city.mayor.CheckTraitEffect(crisisTimerLow) == true) { nodeTimer -= 1; }
                    //loop all nodes
                    foreach (Node node in listOfNodes)
                    {
                        if (node != null)
                        {
                            numOfDangerSigns = 0;
                            if (node.crisisTimer > 0)
                            {
                                //currently undergoing a CRISIS
                                if (node.Security <= crisisSecurity) { numOfDangerSigns++; }
                                if (node.Stability <= crisisStability) { numOfDangerSigns++; }
                                if (node.Support >= crisisSupport) { numOfDangerSigns++; }
                                //check if crisis averted
                                if (numOfDangerSigns == 0)
                                {
                                    //crisis AVERTED
                                    node.crisisTimer = 0;
                                    node.waitTimer = nodeWaitTimer;
                                    node.crisis = null;
                                    node.launcher.StopSmoke();
                                    //admin
                                    msgText = string.Format("{0}, {1}, ID {2} crisis AVERTED", node.nodeName, node.Arc.name, node.nodeID);
                                    itemText = string.Format("{0}, {1}, district crisis AVERTED", node.nodeName, node.Arc.name);
                                    GameManager.instance.messageScript.NodeCrisis(msgText, itemText, node);
                                }
                                else
                                {
                                    //Crisis CONTINUES
                                    node.crisisTimer--;
                                    if (node.crisisTimer > 0)
                                    {
                                        node.launcher.StartSmoke(numOfSmokeParticles);
                                        //add to list of crisis nodes
                                        tempList.Add(node);
                                        //warning message
                                        msgText = string.Format("{0} crisis in {1}, {2}, ({3} turn{4} left to Resolve)", node.crisis.tag, node.nodeName, node.Arc.name,
                                            node.crisisTimer, node.crisisTimer != 1 ? "s" : "");
                                        itemText = string.Format("Crisis continues in {0}, {1}", node.nodeName, node.Arc.name);
                                        string reason = string.Format("{0} crisis will shortly go CRITICAL", node.crisis.tag);
                                        string warning = string.Format("{0} turn{1} left for Authority to resolve Crisis", node.crisisTimer, node.crisisTimer != 1 ? "s" : "");
                                        //good for Resistance, bad for Authority
                                        bool isBad = false;
                                        if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level) { isBad = true; }
                                        GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "District Crisis", reason, warning, true, isBad);
                                    }
                                    else
                                    {
                                        //Crisis COMPLETED (goes Critical) -> lower city support
                                        int loyalty = GameManager.instance.cityScript.CityLoyalty;
                                        loyalty -= crisisCityLoyalty;
                                        loyalty = Mathf.Max(0, loyalty);
                                        GameManager.instance.cityScript.CityLoyalty = loyalty;
                                        //statistics
                                        GameManager.instance.dataScript.StatisticIncrement(StatType.NodeCrisisExplodes);
                                        //admin                                
                                        msgText = string.Format("{0}, {1}, crisis ({2}) has EXPLODED", node.nodeName, node.Arc.name, node.crisis.tag);
                                        itemText = string.Format("{0}, {1}, crisis has EXPLODED", node.nodeName, node.Arc.name);
                                        GameManager.instance.messageScript.NodeCrisis(msgText, itemText, node, crisisCityLoyalty);
                                        msgText = string.Format("{0} Loyalty falls by -{1} to {2} ({3} crisis)", city.tag, crisisCityLoyalty, loyalty, node.crisis.tag);
                                        string reasonText = string.Format("{0} district crisis", node.crisis.tag);
                                        GameManager.instance.messageScript.CityLoyalty(msgText, reasonText, loyalty, crisisCityLoyalty * -1);
                                        //set variables
                                        node.waitTimer = nodeWaitTimer;
                                        node.crisis = null;
                                        node.launcher.StopSmoke();
                                    }
                                }
                            }
                            else if (node.waitTimer > 0)
                            {
                                //WAITING between potential crisis
                                node.waitTimer--;
                                node.launcher.StopSmoke();
                                /*Debug.LogFormat("[Tst] NodeManager.cs -> ProcessNodeCrisis: {0}, ID {1}, waitTimer now {2}{3}", node.Value.Arc.name, node.Value.nodeID, node.Value.waitTimer, "\n");*/
                            }
                            else
                            {
                                //check for datapoints in the DANGER ZONE
                                if (node.Security <= crisisSecurity) { numOfDangerSigns++; listOfDatapoints.Add(GameManager.instance.globalScript.nodeSecurity); }
                                if (node.Stability <= crisisStability) { numOfDangerSigns++; listOfDatapoints.Add(GameManager.instance.globalScript.nodeStability); }
                                if (node.Support >= crisisSupport) { numOfDangerSigns++; listOfDatapoints.Add(GameManager.instance.globalScript.nodeSupport); }
                                //Danger signs present
                                if (numOfDangerSigns > 0)
                                {
                                    chance = (nodeBaseChance - crisisPolicyModifier) * numOfDangerSigns;
                                    chance = Mathf.Clamp(chance, 0, 100);
                                    /*Debug.LogFormat("[Tst] NodeManager.cs -> ProcessNode: {0}, ID {1}, base chance {2}{3}", node.Arc.name, node.nodeID, nodeBaseChance, "\n");
                                    Debug.LogFormat("[Tst] NodeManager.cs -> ProcessNode: {0}, ID {1}, danger signs {2}{3}", node.Arc.name, node.nodeID, numOfDangerSigns, "\n");
                                    Debug.LogFormat("[Tst] NodeManager.cs -> ProcessNode: {0}, ID {1}, policy modifier {2}{3}", node.Arc.name, node.nodeID, crisisPolicyModifier, "\n");*/
                                    //random roll
                                    rnd = Random.Range(0, 100);
                                    if (rnd < chance)
                                    {
                                        //CRISIS COMMENCES -> set crisisTimer
                                        node.crisisTimer = nodeTimer;
                                        node.waitTimer = 0;
                                        node.launcher.StartSmoke(numOfSmokeParticles);
                                        //add to list of crisis nodes
                                        tempList.Add(node);
                                        //track number of crisis for AI decision making
                                        GameManager.instance.aiScript.numOfCrisis++;
                                        GameManager.instance.dataScript.StatisticIncrement(StatType.NodeCrisis);
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
                                        node.crisis = GameManager.instance.dataScript.GetRandomNodeCrisis(datapoint);
                                        if (node.crisis == null)
                                        { Debug.LogErrorFormat("Invalid nodeCrisis default random crisis (Null) for datapoint {0}", datapoint); }
                                        //admin
                                        Debug.LogFormat("[Rnd] NodeManager.cs -> ProcessNodeCrisis: {0} ID {1}, CRISIS need < {2}, rolled {3}", node.Arc.name, node.nodeID,
                                            chance, rnd);
                                        msgText = string.Format("{0}, {1}, ID {2} crisis COMMENCES ({3})", node.nodeName, node.Arc.name, node.nodeID, node.crisis.tag);
                                        itemText = string.Format("{0}, {1}, district crisis COMMENCES", node.nodeName, node.Arc.name);
                                        GameManager.instance.messageScript.NodeCrisis(msgText, itemText, node);
                                        //random
                                        msgText = string.Format("{0}, {1}, district CRISIS succeeds", node.nodeName, node.Arc.name);
                                        GameManager.instance.messageScript.GeneralRandom(msgText, "District Crisis", chance, rnd, true);
                                    }
                                    else
                                    {
                                        //failed roll, nothing happens
                                        node.launcher.StopSmoke();
                                        /*Debug.LogFormat("[Tst] NodeManager.cs -> ProcessNodeCrisis: {0} ID {1}, Failed need < {2}, rolled {3}", node.Arc.name, node.nodeID,
                                            chance, rnd);*/
                                        //random
                                        msgText = string.Format("{0}, {1}, district CRISIS fails", node.nodeName, node.Arc.name);
                                        GameManager.instance.messageScript.GeneralRandom(msgText, "District Crisis", chance, rnd, true);
                                    }
                                }
                            }
                        }
                        else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", node.nodeID); }
                    }
                    //update listOfCrisisNodes
                    listOfCrisisNodes.Clear();
                    listOfCrisisNodes.AddRange(tempList);
                    /*Debug.LogFormat("[Tst] NodeManager.cs -> ProcessNodeCrisis: {0} records in listOfCrisisNodes{1}", listOfCrisisNodes.Count, "\n");*/
                }
                else { Debug.LogWarning("Invalid city (Null) -> Crisis checks cancelled"); }
            }
            else { Debug.LogWarning("Invalid listOfCrisisNodes (Null) -> Crisis checks cancelled"); }
        }
        else { Debug.LogWarning("Invalid listOfNodes (Null) -> Crisis checks cancelled"); }
    }

    /// <summary>
    /// start up smoke emitters on crisis nodes when Loading saved game data
    /// </summary>
    public void ProcessLoadNodeCrisis()
    {
        List<Node> listOfCrisisNodes = GameManager.instance.dataScript.GetListOfCrisisNodes();
        if (listOfCrisisNodes != null)
        {
            for (int i = 0; i < listOfCrisisNodes.Count; i++)
            {
                Node node = listOfCrisisNodes[i];
                if (node != null)
                {
                    if (node.crisisTimer > 0)
                    { node.launcher.StartSmoke(numOfSmokeParticles); }
                    else { Debug.LogWarningFormat("Invalid nodeCrisisTimer {0} for nodeID {1} (should be > 0)", node.crisisTimer, node.nodeID); }
                }
                else { Debug.LogWarningFormat("Invalid node (Null) for listOfCrisisNodes[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid listOfCrisisNodes (Null)"); }
    }

    /// <summary>
    /// Add a cure to a node location and handles all admin. isActive sets initial state of Cure (default false). Overriden to true if Resistance is controlled by AI.
    /// </summary>
    /// <param name="cure"></param>
    /// <returns></returns>
    public void AddCureNode(Cure cure, bool isActive = false)
    {
        if (cure != null)
        {
            int nodeID = GetCureNode(cure);
            if (nodeID > -1)
            {
                //set
                Node node = GameManager.instance.dataScript.GetNode(nodeID);
                {
                    if (node != null)
                    {
                        node.cure = cure;
                        cure.isActive = isActive;
                        //automatically make active if Resistance is AI
                        if (isActive == false && GameManager.instance.sideScript.resistanceOverall == SideState.AI)
                        { cure.isActive = true; }
                        GameManager.instance.dataScript.AddCureNode(node);
                        Debug.LogFormat("[Nod] NodeManager.cs -> AddCureNode: {0} cure ADDED at {1}, {2}, ID {3}{4}", cure.cureName, node.nodeName, node.Arc.name, node.nodeID, "\n");
                    }
                    else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", nodeID); }
                }
            }
            else { Debug.LogErrorFormat("Invalid Cure nodeID (-1) for \"{0}\", distance {1}", cure.cureName, cure.distance); }
        }
        else { Debug.LogError("Invalid cure (Null)"); }
    }

    /// <summary>
    /// Removes a specified cure from it's node location and handles all admin
    /// </summary>
    /// <param name="cure"></param>
    public void RemoveCureNode(Cure cure)
    {
        if (cure != null)
        {
            Node node = GameManager.instance.dataScript.GetCureNode(cure);
            if (node != null)
            {
                if (GameManager.instance.dataScript.RemoveCureNode(node) == true)
                {
                    cure.timesCured++;
                    cure.isActive = false;
                    node.cure = null;
                    Debug.LogFormat("[Nod] NodeManager.cs -> RemoveCureNode: {0} cure REMOVED at {1}, {2}, ID {3}{4}", cure.cureName, node.nodeName, node.Arc.name, node.nodeID, "\n");
                }
                else { Debug.LogErrorFormat("{0}, {1}, ID {2}, NOT REMOVED from listOfCureNodes", node.nodeName, node.Arc.name, node.nodeID); }
            }
            else { Debug.LogErrorFormat("Invalid node (Null) for \"{0}\", ID {1} cure", cure.cureName, cure.cureID); }
        }
        else { Debug.LogError("Invalid cure (Null)"); }
    }

    /// <summary>
    /// finds a random node, 'x' distance links away from the Resistance Player's current location (may end up being less), one that doesn't already have a cure. Returns -1 if a problem
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    private int GetCureNode(Cure cure)
    {
        int cureNodeID = -1;
        int requiredDistance = cure.distance;
        int furthestDistance = 0;
        int actualDistance = 0;
        Debug.Assert(requiredDistance > 0, "Invalid cure.requiredDistance (must be > 0)");
        if (cure != null)
        {
            PathData data = GameManager.instance.dataScript.GetDijkstraPathUnweighted(nodePlayer);
            if (data != null)
            {
                if (data.distanceArray != null)
                {
                    int[] arrayOfDistances = data.distanceArray;
                    //get exclusion list of nodes currently with a cure
                    List<int> listOfExclusion = new List<int>();
                    List<Node> listOfCureNodes = GameManager.instance.dataScript.GetListOfCureNodes();
                    if (listOfCureNodes != null)
                    {
                        for (int index = 0; index < listOfCureNodes.Count; index++)
                        {
                            if (listOfCureNodes[index] != null)
                            { listOfExclusion.Add(listOfCureNodes[index].nodeID); }
                            else { Debug.LogErrorFormat("Invalid node (Null) for listOfCureNodes[{0}]", index); }
                        }
                    }
                    else { Debug.LogError("Invalid listOfCureNodes (Null)"); }
                    //get max distance possible from Resistance Player's current node
                    furthestDistance = arrayOfDistances.Max();
                    //adjust distance required to furthest available (if required in case map size, or player position, doesn't accomodate the requested distance)
                    actualDistance = Mathf.Min(furthestDistance, requiredDistance);
                    //loop distance Array and find first node that is that distance, or greater, and one that doesn't currently have a cure location
                    for (int index = 0; index < arrayOfDistances.Length; index++)
                    {
                        if (arrayOfDistances[index] == actualDistance)
                        {
                            //not on exclusion list
                            if (listOfExclusion.Exists(x => x == index) == false)
                            {
                                Debug.LogFormat("[Tst] NodeManager.cs -> GetCureNode: Straight Match for {0} cure, nodeID {1}, distance {2} (actual {3}){4}", 
                                    cure.cureName, index, cure.distance, actualDistance, "\n");
                                cureNodeID = index;
                                break;
                            }
                        }
                    }
                    //if not successful scale up distance until you get a hit. If you max out, scale down distance until you get a hit.
                    if (cureNodeID < 0)
                    {
                        int tempDistance = actualDistance;
                        if (actualDistance < furthestDistance)
                        {
                            //gradually increase distance until you find a suitable node
                            do
                            {
                                tempDistance++;
                                //search on new distance criteria
                                for (int index = 0; index < arrayOfDistances.Length; index++)
                                {
                                    if (arrayOfDistances[index] == tempDistance)
                                    {
                                        //not on exclusion list
                                        if (listOfExclusion.Exists(x => x == index) == false)
                                        {
                                            Debug.LogFormat("[Tst] NodeManager.cs -> GetCureNode: SCALE UP for {0} cure, nodeID {1}, distance {2} (actual {3}){4}",
                                                cure.cureName, index, cure.distance, tempDistance, "\n");
                                            cureNodeID = index;
                                            break;
                                        }
                                    }
                                }
                                if (cureNodeID > -1) { break; }
                            }
                            while (tempDistance < furthestDistance);
                        }
                        //if unsuccessful (or already maxxed out on distance) decrease distance until a suitable node is found
                        if (cureNodeID < 0)
                        {
                            tempDistance = actualDistance;
                            do
                            {
                                tempDistance--;
                                //search on new distance criteria
                                for (int index = 0; index < arrayOfDistances.Length; index++)
                                {
                                    if (arrayOfDistances[index] == tempDistance)
                                    {
                                        //not on exclusion list
                                        if (listOfExclusion.Exists(x => x == index) == false)
                                        {
                                            Debug.LogFormat("[Tst] NodeManager.cs -> GetCureNode: SCALE DOWN for {0} cure, nodeID {1}, distance {2} (actual {3}){4}",
                                                cure.cureName, index, cure.distance, tempDistance, "\n");
                                            cureNodeID = index;
                                            break;
                                        }
                                    }
                                }
                                if (cureNodeID > -1) { break; }
                            }
                            while (tempDistance > 0);
                        }
                    }
                }
                else { Debug.LogError("Invalid distanceArray (Null)"); }
            }
            else { Debug.LogError("Invalid PathData (Null)"); }
        }
        else { Debug.LogError("Invalid cure (Null)"); }
        return cureNodeID;
    }


    //new methods above here
}