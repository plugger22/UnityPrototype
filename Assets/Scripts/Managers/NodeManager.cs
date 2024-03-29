﻿using gameAPI;
using modalAPI;
using packageAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Handles all node related matters
/// </summary>
public class NodeManager : MonoBehaviour
{
    [Header("Materials")]
    [Tooltip("Node Colour Types. Indexes correspond to enum.NodeColour")]
    public Material[] arrayOfNodeMaterials;

    [Header("Active Nodes")]
    [Tooltip("% chance times actor.Ability of a primary node being active for an Actor, halved for secondary node")]
    [Range(10, 40)] public int nodePrimaryChance = 20;
    [Tooltip("Minimum number of active nodes on a map for any actor type")]
    [Range(0, 4)] public int nodeActiveMinimum = 3;
    [Tooltip("The base factor used for calculating ('factor - (gear - seclvl [High is 3, Low is 1])') the delay in notifying the Authority player that move activity has occurred ")]
    [Range(0, 10)] public int moveInvisibilityDelay = 4;

    [Header("Spiders and Tracers")]
    [Tooltip("The standard time delay, in turns, before Authority notification for any node activity that results in a loss of invisibility with no spider present")]
    [Range(0, 10)] public int nodeNoSpiderDelay = 2;
    [Tooltip("The standard time delay before Authority notification for any node activity that results in a loss of invisibility with a spider present. Make sure that this is less than the NoSpider delay")]
    [Range(0, 10)] public int nodeYesSpiderDelay = 1;
    [Tooltip("The amount of turns that a Spider or Tracer stay onMap for, once placed, before being automatically removed")]
    [Range(0, 10)] public int observerTimer = 3;

    [Header("Datapoints")]
    [Tooltip("Maximum value of a node datapoint")]
    [Range(0, 4)] public int maxNodeValue = 3;
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
    [HideInInspector] public int nodeNpc = -1;                      //nodeID of Npc
    [HideInInspector] public int nodeCaptured = -1;                 //nodeID where player has been captured, -1 if not
    #endregion

    private bool isFlashOn = false;                                 //used for flashing Node coroutine
    private bool showPlayerNode = true;                             //switched off if player node needs to be flashed


    private Coroutine myFlashCoroutine;

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
    private Material materialTowerDark;
    private Material materialTowerLight;
    private Material materialNormal;
    private Material materialHighlight;
    private Material materialActive;
    private Material materialTowerActive;
    private Material materialPlayer;
    private Material materialNemesis;
    private Material materialBackground;            //grey
    private Material materialInvisible;
    private Material materialTransparent;
    //flash
    private float flashNodeTime;

    //fast access
    private GlobalSide globalResistance;
    private GlobalSide globalAuthority;

    //colours
    string colourDefault;
    string colourNormal;
    string colourCyber;
    string colourAlert;
    /*string colourHighlight;*/
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

    #region Properties...
    //
    // - - - Properties
    //
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
    #endregion

    #region Awake
    /// <summary>
    /// Self initialisation
    /// </summary>
    private void Awake()
    {
        //bounds checking
        nodePrimaryChance = nodePrimaryChance > 0 ? nodePrimaryChance : 10;
        nodeActiveMinimum = nodeActiveMinimum > 2 ? nodeActiveMinimum : 3;
    }
    #endregion

    #region Initialise
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.TutorialOptions:
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseAll();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseAll();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseAll();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadGame:
                SubInitialiseAll();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }
    #endregion

    #region Initialise SubMethods

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //fast access
        globalResistance = GameManager.i.globalScript.sideResistance;
        globalAuthority = GameManager.i.globalScript.sideAuthority;
        materialTowerDark = GetNodeMaterial(NodeColour.TowerDark);
        materialTowerLight = GetNodeMaterial(NodeColour.TowerLight);
        materialNormal = GetNodeMaterial(NodeColour.Normal);
        materialHighlight = GetNodeMaterial(NodeColour.Highlight);
        materialActive = GetNodeMaterial(NodeColour.Active);
        materialTowerActive = GetNodeMaterial(NodeColour.TowerActive);
        materialPlayer = GetNodeMaterial(NodeColour.Player);
        materialNemesis = GetNodeMaterial(NodeColour.Nemesis);
        materialBackground = GetNodeMaterial(NodeColour.Background);
        materialInvisible = GetNodeMaterial(NodeColour.Invisible);
        materialTransparent = GetNodeMaterial(NodeColour.Transparent);
        crisisBaseChanceDoubled = "NodeCrisisBaseChanceDoubled";
        crisisBaseChanceHalved = "NodeCrisisBaseChanceHalved";
        crisisTimerHigh = "NodeCrisisTimerHigh";
        crisisTimerLow = "NodeCrisisTimerLow";
        crisisWaitTimerDoubled = "NodeCrisisWaitTimerDoubled";
        crisisWaitTimerHalved = "NodeCrisisWaitTimerHalved";
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(materialTowerDark != null, "Invalid materialTowerDark (Null)");
        Debug.Assert(materialTowerLight != null, "Invalid materialTowerLight (Null)");
        Debug.Assert(materialNormal != null, "Invalid materialNormal (Null)");
        Debug.Assert(materialHighlight != null, "Invalid materialHighlight (Null)");
        Debug.Assert(materialActive != null, "Invalid materialActive (Null)");
        Debug.Assert(materialPlayer != null, "Invalid materialPlayer (Null)");
        Debug.Assert(materialNemesis != null, "Invalid materialNemesis (Null)");
        Debug.Assert(materialInvisible != null, "Invalid materialInvisibile (Null)");
        Debug.Assert(materialBackground != null, "Invalid materialBackground (Null)");
        Debug.Assert(materialTransparent != null, "Invalid materialTransparent (Null)");
        //gear node Action Fast access
        actionKinetic = GameManager.i.dataScript.GetAction("gearKinetic");
        actionHacking = GameManager.i.dataScript.GetAction("gearHacking");
        actionPersuasion = GameManager.i.dataScript.GetAction("gearPersuasion");
        Debug.Assert(actionKinetic != null, "Invalid actionKinetic (Null)");
        Debug.Assert(actionHacking != null, "Invalid actionHacking (Null)");
        Debug.Assert(actionPersuasion != null, "Invalid actionPersuasion (Null)");
        //flash
        flashNodeTime = GameManager.i.guiScript.flashNodeTime;
        Debug.Assert(flashNodeTime > 0, "Invalid flashNodeTime (zero)");
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.i.AddListener(EventType.NodeDisplay, OnEvent, "NodeManager");
        EventManager.i.AddListener(EventType.ActivityDisplay, OnEvent, "NodeManager");
        EventManager.i.AddListener(EventType.ChangeColour, OnEvent, "NodeManager");
        EventManager.i.AddListener(EventType.CreateMoveMenu, OnEvent, "NodeManager");
        EventManager.i.AddListener(EventType.CreateGearNodeMenu, OnEvent, "NodeManager");
        EventManager.i.AddListener(EventType.MoveAction, OnEvent, "NodeManager");
        EventManager.i.AddListener(EventType.StartTurnLate, OnEvent, "NodeManager");
        EventManager.i.AddListener(EventType.FlashNodesStart, OnEvent, "NodeManager");
        EventManager.i.AddListener(EventType.FlashNodesStop, OnEvent, "NodeManager");
        EventManager.i.AddListener(EventType.ShowNode, OnEvent, "NodeManager");
    }
    #endregion

    #region SubInitialiseAll
    private void SubInitialiseAll()
    {
        //Set node contact flags (player side & non-player side)
        GameManager.i.contactScript.UpdateNodeContacts();
        GameManager.i.contactScript.UpdateNodeContacts(false);
        //check arrayOfNodeMaterials fully stocked
        Debug.AssertFormat(arrayOfNodeMaterials.Length == (int)NodeColour.Count, "Invalid arrayOfNodeMaterials (is {0} materials, should be {1})", arrayOfNodeMaterials.Length, NodeColour.Count);
        //set default face text for nodes
        ResetNodes();
    }
    #endregion

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        //find specific EffectOutcome SO's and assign to outcome fields
        EffectOutcome[] arrayOfEffectOutcome = GameManager.i.loadScript.arrayOfEffectOutcome;
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
        //tutorial hide (spider/tracer items)
        if (GameManager.i.inputScript.GameState == GameState.TutorialOptions)
        {
            TutorialHideConfig config = GameManager.i.tutorialScript.set.hideConfig;
            if (config != null)
            { ConfigureTutorialHideItems(config); }
        }
        //check all found and assigned
        if (outcomeNodeSecurity == null) { Debug.LogError("Invalid outcomeNodeSecurity (Null)"); }
        if (outcomeNodeStability == null) { Debug.LogError("Invalid outcomeNodeStability (Null)"); }
        if (outcomeNodeSupport == null) { Debug.LogError("Invalid outcomeNodeSupport (Null)"); }
        if (outcomeStatusSpiders == null) { Debug.LogError("Invalid outcomeStatusSpiders (Null)"); }
        if (outcomeStatusTracers == null) { Debug.LogError("Invalid outcomeStatusTracers (Null)"); }
        if (outcomeStatusContacts == null) { Debug.LogError("Invalid outcomeStatusContacts (Null)"); }
        if (outcomeStatusTeams == null) { Debug.LogError("Invalid outcomeStatusTeams (Null)"); }
    }
    #endregion

    #endregion

    #region OnEvent
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
                    case NodeUI.ShowContacts:
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
                    case NodeUI.PlayerKnown:
                        if (NodeShowFlag > 0)
                        { ResetAll(); }
                        else
                        {
                            /*ShowNodes(nodeUI);*/
                            FlashHighlightNodes(nodeUI);
                        }
                        break;
                    default:
                        /*Debug.LogError(string.Format("Invalid NodeUI param \"{0}\"{1}", Param.ToString(), "\n"));*/
                        Debug.LogError(string.Format("Invalid NodeUI param \"{0}\"{1}", nodeUI, "\n"));
                        break;
                }
                break;
            case EventType.ShowNode:
                if (NodeShowFlag > 0)
                { ResetAll(); }
                else
                {
                    //flash a particular node (FinderUI.cs)
                    FlashNode((int)Param);
                }
                break;
            case EventType.FlashNodesStart:
                StartFlashingNodes((List<Node>)Param);
                break;
            case EventType.FlashNodesStop:
                StopFlashingNodes();
                break;
            case EventType.ActivityDisplay:
                ActivityUI activityUI = (ActivityUI)Param;
                switch (activityUI)
                {
                    case ActivityUI.Time:
                    case ActivityUI.Count:
                        if (NodeShowFlag > 0)
                        {
                            if (GameManager.i.optionScript.noNodes == false)
                            { ResetAll(); }
                            else
                            {
                                SetDistrictFaceText(NodeText.None, false);
                                GameManager.i.connScript.RestoreConnections();
                                GameManager.i.alertScript.CloseAlertUI(true);
                            }
                        }
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
    #endregion

    #region SetColours
    /// <summary>
    /// Set colour palette for tooltip
    /// </summary>
    public void SetColours()
    {
        colourDefault = GameManager.i.colourScript.GetColour(ColourType.whiteText);
        colourNormal = GameManager.i.colourScript.GetColour(ColourType.normalText);
        colourCyber = GameManager.i.colourScript.GetColour(ColourType.cyberText);
        colourAlert = GameManager.i.colourScript.GetColour(ColourType.salmonText);
        /*colourHighlight = GameManager.i.colourScript.GetColour(ColourType.neutralText);*/
        colourResistance = GameManager.i.colourScript.GetColour(ColourType.blueText);
        colourBad = GameManager.i.colourScript.GetColour(ColourType.badText);
        colourNeutral = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourGood = GameManager.i.colourScript.GetColour(ColourType.goodText);
        colourError = GameManager.i.colourScript.GetColour(ColourType.dataBad);
        colourInvalid = GameManager.i.colourScript.GetColour(ColourType.salmonText);
        colourCancel = GameManager.i.colourScript.GetColour(ColourType.moccasinText);
        colourEnd = GameManager.i.colourScript.GetEndTag();
    }
    #endregion


    #region ResetCounters
    /// <summary>
    /// reset data ready for a new level
    /// </summary>
    public void ResetCounters()
    {
        //reset node & connection ID counters
        nodeIDCounter = 0;
        connIDCounter = 0;
    }
    #endregion

    #region ToggleNodeHighlight
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
    #endregion

    #region GetNodeMaterial
    /// <summary>
    /// Return a node Material
    /// </summary>
    /// <param name="nodeType"></param>
    /// <returns></returns>
    public Material GetNodeMaterial(NodeColour nodeType)
    { return arrayOfNodeMaterials[(int)nodeType]; }
    #endregion

    #region GetPlayerNodeID
    /// <summary>
    /// Use this for all calls for player nodeID. Handles edge cases and player/AI automatically. Returns -1 if a problem (unlikely)
    /// </summary>
    /// <returns></returns>
    public int GetPlayerNodeID()
    {
        int nodeID = -1;
        switch (GameManager.i.sideScript.PlayerSide.level)
        {
            case 1:
                //Authority
                switch (GameManager.i.sideScript.authorityCurrent)
                {
                    case SideState.Human:
                        {
                            switch (GameManager.i.playerScript.Status)
                            {
                                case ActorStatus.Active:
                                case ActorStatus.Inactive:
                                    nodeID = nodePlayer; break;
                                default: Debug.LogWarningFormat("Unrecognised Player Status \"{0}\"", GameManager.i.playerScript.Status); nodeID = -1; break;
                            }
                        }
                        break;
                    case SideState.AI:
                        {
                            switch (GameManager.i.aiScript.status)
                            {
                                case ActorStatus.Active:
                                case ActorStatus.Inactive:
                                    nodeID = nodePlayer; break;
                                default: Debug.LogWarningFormat("Unrecognised aiRebel Status \"{0}\"", GameManager.i.playerScript.Status); nodeID = -1; break;
                            }
                        }
                        break;
                }
                break;
            case 2:
                //Resistance
                switch (GameManager.i.sideScript.resistanceCurrent)
                {
                    case SideState.Human:
                        {
                            switch (GameManager.i.playerScript.Status)
                            {
                                case ActorStatus.Active:
                                case ActorStatus.Inactive:
                                    nodeID = nodePlayer; break;
                                case ActorStatus.Captured: nodeID = nodeCaptured; break;
                                default: Debug.LogWarningFormat("Unrecognised Player Status \"{0}\"", GameManager.i.playerScript.Status); nodeID = -1; break;
                            }
                        }
                        break;
                    case SideState.AI:
                        {
                            switch (GameManager.i.aiRebelScript.status)
                            {
                                case ActorStatus.Active:
                                case ActorStatus.Inactive:
                                    nodeID = nodePlayer; break;
                                case ActorStatus.Captured: nodeID = nodeCaptured; break;
                                default: Debug.LogWarningFormat("Unrecognised aiRebel Status \"{0}\"", GameManager.i.playerScript.Status); nodeID = -1; break;
                            }
                        }
                        break;
                }
                break;
            default: Debug.LogWarningFormat("Unrecognised Player Side \"{0}\"", GameManager.i.sideScript.PlayerSide.name); break;
        }
        //debug problem tracking
        if (nodeID < 0)
        { Debug.LogWarningFormat("Invalid nodeID {0} for Player status \"{1}\", inactive status \"{2}\"", nodeID, GameManager.i.playerScript.Status, GameManager.i.playerScript.InactiveStatus); }
        return nodeID;
    }
    #endregion

    #region ShowNodes
    /// <summary>
    /// highlights all nodes depening on the enum NodeUI criteria
    /// </summary>
    public List<Node> ShowNodes(NodeUI nodeUI)
    {
        int data = -1;
        int count;
        bool successFlag = true;
        bool nodeTypeFlag = false;
        bool proceedFlag = false;
        string displayText = null;
        bool isFogOfWar = GameManager.i.optionScript.isFogOfWar;
        List<Node> listOfHighlighted = new List<Node>();
        //set all nodes to default colour first
        ResetNodes();
        if (GameManager.i.connScript.resetConnections == true)
        {
            //return to previously saved state prior to any changes
            GameManager.i.connScript.RestoreConnections();
        }
        //set nodes depending on critera
        switch (nodeUI)
        {
            //show all nodes with Targets
            case NodeUI.ShowTargets:
                if (GameManager.i.optionScript.isTargets == true)
                {
                    List<Target> tempList = new List<Target>();
                    if (isFogOfWar == false)
                    {
                        //FOW Off
                        /*tempList.AddRange(GameManager.i.dataScript.GetTargetPool(Status.Active));*/
                        tempList.AddRange(GameManager.i.dataScript.GetTargetPool(GlobalStatus.Live));
                        tempList.AddRange(GameManager.i.dataScript.GetTargetPool(GlobalStatus.Outstanding));
                        if (tempList.Count > 0)
                        {
                            foreach (Target target in tempList)
                            {
                                Node nodeTemp = GameManager.i.dataScript.GetNode(target.nodeID);
                                if (nodeTemp != null)
                                {
                                    nodeTemp.SetActive();
                                    listOfHighlighted.Add(nodeTemp);
                                }
                                else { Debug.LogWarning(string.Format("Invalid node (Null) for target.nodeID {0}", target.nodeID)); }
                            }
                            displayText = string.Format("{0}{1}{2}{3} Target{4}{5} district{6}{7}", colourNormal, tempList.Count, colourEnd, colourCyber, colourEnd,
                                colourDefault, tempList.Count != 1 ? "s" : "", colourEnd);
                        }
                        else { displayText = string.Format("{0}{1}{2}", colourError, "No Targets present", colourEnd); }
                    }
                    else
                    {
                        //FOW ON
                        tempList.AddRange(GameManager.i.dataScript.GetTargetPool(GlobalStatus.Live));
                        tempList.AddRange(GameManager.i.dataScript.GetTargetPool(GlobalStatus.Outstanding));
                        if (tempList.Count > 0)
                        {
                            count = 0;
                            foreach (Target target in tempList)
                            {
                                Node nodeTemp = GameManager.i.dataScript.GetNode(target.nodeID);
                                if (nodeTemp != null)
                                {
                                    //only show if target isKnown (authority side
                                    if (GameManager.i.sideScript.PlayerSide.level == 1)
                                    {
                                        if (nodeTemp.isTargetKnown == true)
                                        {
                                            nodeTemp.SetActive();
                                            listOfHighlighted.Add(nodeTemp);
                                            count++;
                                        }
                                    }
                                    else
                                    {
                                        //resistance player
                                        nodeTemp.SetActive();
                                        listOfHighlighted.Add(nodeTemp);
                                        count++;
                                    }
                                }
                                else { Debug.LogWarningFormat("Invalid node (Null) for target.nodeID {0}", target.nodeID); }
                            }
                            displayText = string.Format("{0}{1}{2}{3} Target{4}{5} district{6}{7}", colourNormal, count, colourEnd, colourCyber, colourEnd,
                                colourDefault, tempList.Count != 1 ? "s" : "", colourEnd);
                        }
                        else { displayText = string.Format("{0}{1}{2}", colourError, "No Targets present", colourEnd); }
                    }
                }
                //Targets off
                else { displayText = string.Format("{0}Targets have been Disabled{1}", colourError, colourEnd); }
                break;

            //show all viable move locations for player (nodes adjacent to current location)
            case NodeUI.Move:
                Node nodeRef = GameManager.i.dataScript.GetNode(GetPlayerNodeID());
                if (nodeRef != null)
                {
                    /*List<Node> nodeList = null;
                    //get list of move nodes (depends on whether player has special move gear in their inventory)
                    if (GameManager.i.playerScript.isSpecialMoveGear == true)
                    { nodeList = nodeRef.GetNearNeighbours(); }
                    else { nodeList = nodeRef.GetNeighbouringNodes(); }*/

                    List<Node> nodeList = GameManager.i.dataScript.GetListOfMoveNodes();
                    if (nodeList != null)
                    {
                        if (nodeList.Count > 0)
                        {
                            foreach (Node node in nodeList)
                            {
                                if (node != null && node.nodeID != nodeRef.nodeID)
                                {
                                    node.SetActive();
                                    listOfHighlighted.Add(node);
                                }
                            }
                            if (GameManager.i.playerScript.isSpecialMoveGear == false)
                            {
                                //normal move
                                displayText = string.Format("{0}{1}{2} {3}valid Move district{4}{5}", colourNormal, nodeList.Count, colourEnd,
                                colourCyber, nodeList.Count != 1 ? "s" : "", colourEnd);
                            }
                            else
                            {
                                //has special move gear
                                displayText = string.Format("{0}{1}{2} {3}valid Move district{4} (using {5}){6}", colourNormal, nodeList.Count, colourEnd,
                                colourCyber, nodeList.Count != 1 ? "s" : "", GameManager.i.gearScript.gearSpecialMove.tag, colourEnd);
                            }
                        }
                        else
                        {
                            displayText = string.Format("{0}There are no districts you can Move to{1}", colourError, colourEnd);
                            Debug.LogWarning("No records in list of Neighbouring Nodes");
                        }
                    }
                    else { Debug.LogError("Invalid nodeList (Null) for GetNeighbouring nodes"); }
                }
                else { Debug.LogError(string.Format("Invalid node (Null) for NodeID {0}", GetPlayerNodeID())); }
                break;
            //show all districts where at least one contact is present
            case NodeUI.ShowContacts:

                /*ShowAllContacts(); -> Displays node face data*/

                List<Node> listOfContactNodes = GameManager.i.dataScript.GetListOfContactNodes();
                count = 0;
                if ( listOfContactNodes.Count > 0)
                {
                    foreach(Node node in listOfContactNodes)
                    {
                        node.SetActive();
                        listOfHighlighted.Add(node);
                        count++;
                    }
                }
                if (count > 0)
                {
                    displayText = string.Format("{0}{1}{2} {3}{4}{5} with {6}Contacts{7}{8}", colourNormal, count, colourEnd,
                        colourDefault, "district", count != 1 ? "s" : "", colourEnd, 
                        colourCyber,  colourEnd);
                }
                else
                { displayText = string.Format("{0}There are no districts with Contacts{1}", colourError, colourEnd); }
                break;
            //show all nodes containng a spider
            case NodeUI.ShowSpiders:
                List<Node> listOfSpiderNodes = GameManager.i.dataScript.GetListOfAllNodes();
                if (listOfSpiderNodes != null)
                {
                    count = 0;
                    //determine level of visibility
                    switch (GameManager.i.sideScript.PlayerSide.name)
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
                            Debug.LogError(string.Format("Invalid side \"{0}\"", GameManager.i.sideScript.PlayerSide.name));
                            break;
                    }
                    foreach (Node node in listOfSpiderNodes)
                    {
                        if (node.isSpider == true)
                        {
                            //show all
                            if (proceedFlag == true)
                            {
                                node.SetActive();
                                listOfHighlighted.Add(node);
                                count++;
                            }
                            //conditional -> only show if spider is known
                            else
                            {
                                if (node.isSpiderKnown == true)
                                {
                                    node.SetActive();
                                    listOfHighlighted.Add(node);
                                    count++;
                                }
                            }
                        }
                    }
                    if (count > 0)
                    {
                        displayText = string.Format("{0}{1}{2} {3}{4}{5} {6}district{7}{8}", colourNormal, count, colourEnd,
                            colourCyber, "Spider", colourEnd, colourDefault, count != 1 ? "s" : "", colourEnd);
                    }
                    else
                    { displayText = string.Format("{0}There are no Spider districts{1}", colourError, colourEnd); }
                }
                else { Debug.LogError("Invalid listOfSpiderNodes (Null)"); }
                break;

            //show all nodes with a tracer or within one node radius of a tracer
            case NodeUI.ShowTracers:
                List<Node> listOfTracerNodes = GameManager.i.dataScript.GetListOfAllNodes();
                if (listOfTracerNodes != null)
                {
                    count = 0;
                    //determine level of visibility
                    switch (GameManager.i.sideScript.PlayerSide.level)
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
                            Debug.LogError(string.Format("Invalid side \"{0}\"", GameManager.i.sideScript.PlayerSide.name));
                            break;
                    }
                    foreach (Node node in listOfTracerNodes)
                    {
                        if (node.isTracer == true)
                        {
                            //show all
                            if (proceedFlag == true)
                            {
                                node.SetActive();
                                listOfHighlighted.Add(node);
                                count++;
                            }
                            //conditional -> only show if tracer is known
                            else
                            {
                                if (node.isTracerKnown == true)
                                {
                                    node.SetActive();
                                    listOfHighlighted.Add(node);
                                    count++;
                                }
                            }
                        }
                    }
                    if (count > 0)
                    {
                        displayText = string.Format("{0}{1}{2} {3}{4}{5} {6}district{7}{8}", colourNormal, count, colourEnd,
                            colourCyber, "Tracer", colourEnd, colourDefault, count != 1 ? "s" : "", colourEnd);
                    }
                    else
                    { displayText = string.Format("{0}There are no Tracer districts{1}", colourError, colourEnd); }
                }
                else { Debug.LogError("Invalid listOfTracerNodes (Null)"); }
                break;

            //show all nodes containng a Team
            case NodeUI.ShowTeams:
                List<int> listOfTeams = GameManager.i.dataScript.GetTeamPool(TeamPool.OnMap);
                if (listOfTeams != null)
                {
                    count = 0;
                    //determine level of visibility
                    switch (GameManager.i.sideScript.PlayerSide.level)
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
                            Debug.LogError(string.Format("Invalid side \"{0}\"", GameManager.i.sideScript.PlayerSide));
                            break;
                    }
                    List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
                    if (listOfNodes != null)
                    {
                        foreach (Node node in listOfNodes)
                        {
                            if (node.CheckNumOfTeams() > 0)
                            {
                                //show all
                                if (proceedFlag == true)
                                {
                                    node.SetActive();
                                    listOfHighlighted.Add(node);
                                    count++;
                                }
                                //conditional -> only show if team is known, actor has contacts or node within tracer coverage
                                else
                                {
                                    if (node.isTeamKnown || node.isTracer || node.isContactResistance)
                                    {
                                        node.SetActive();
                                        listOfHighlighted.Add(node);
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                    else { Debug.LogError("Invalid listOfNodes (Null)"); }
                    if (count > 0)
                    { displayText = string.Format("{0}{1}{2} {3} Team district{4}{5}", colourNormal, count, colourEnd, colourCyber, count != 1 ? "s" : "", colourEnd); }
                    else
                    { displayText = string.Format("{0}There are no Teams present{1}", colourError, colourEnd); }
                }
                break;
            case NodeUI.MostConnected:
                List<Node> connectedList = GameManager.i.dataScript.GetListOfMostConnectedNodes();
                if (connectedList != null)
                {
                    if (connectedList.Count > 0)
                    {
                        foreach (Node node in connectedList)
                        {
                            if (node != null)
                            {
                                node.SetActive();
                                listOfHighlighted.Add(node);
                            }
                            else { Debug.LogWarning("Invalid node (Null)"); }
                        }
                        displayText = string.Format("{0}{1}{2}{3} Most Connected district{4}{5}", colourNormal, connectedList.Count, colourEnd, colourCyber,
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
                List<Node> loiterList = GameManager.i.dataScript.GetListOfLoiterNodes();
                if (loiterList != null)
                {
                    if (loiterList.Count > 0)
                    {
                        foreach (Node node in loiterList)
                        {
                            if (node != null)
                            {
                                node.SetActive();
                                listOfHighlighted.Add(node);
                            }
                            else { Debug.LogWarning("Invalid node (Null)"); }
                        }
                        displayText = string.Format("{0}{1}{2}{3} Loiter district{4}{5}", colourNormal, loiterList.Count, colourEnd, colourCyber,
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
                count = 0;
                List<Node> cureList = GameManager.i.dataScript.GetListOfCureNodes();
                if (cureList != null)
                {
                    if (cureList.Count > 0)
                    {
                        foreach (Node node in cureList)
                        {
                            if (node != null)
                            {
                                if (node.cure.isActive == true)
                                {
                                    node.SetActive();
                                    listOfHighlighted.Add(node);
                                    count++;
                                }
                            }
                            else { Debug.LogWarning("Invalid node (Null)"); }
                        }
                        displayText = string.Format("{0}{1}{2}{3} Cure district{4}{5}", colourNormal, count, colourEnd, colourCyber, count != 1 ? "s" : "", colourEnd);
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
                List<Node> decisionList = GameManager.i.dataScript.GetListOfDecisionNodes();
                if (decisionList != null)
                {
                    if (decisionList.Count > 0)
                    {
                        foreach (Node node in decisionList)
                        {
                            if (node != null)
                            {
                                node.SetActive();
                                listOfHighlighted.Add(node);
                            }
                            else { Debug.LogWarning("Invalid node (Null)"); }
                        }
                        displayText = string.Format("{0}{1}{2}{3} Decision district{4}{5}", colourNormal, decisionList.Count, colourEnd, colourCyber,
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
                List<Node> crisisList = GameManager.i.dataScript.GetListOfCrisisNodes();
                if (crisisList != null)
                {
                    if (crisisList.Count > 0)
                    {
                        foreach (Node node in crisisList)
                        {
                            if (node != null)
                            {
                                node.SetActive();
                                listOfHighlighted.Add(node);
                            }
                            else { Debug.LogWarning("Invalid node (Null)"); }
                        }
                        displayText = string.Format("{0}{1}{2}{3} Crisis district{4}{5}", colourNormal, crisisList.Count, colourEnd, colourCyber,
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
                Node nodeNear = GameManager.i.dataScript.GetNode(GetPlayerNodeID());
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
                                    nodeTemp.SetActive();
                                    listOfHighlighted.Add(nodeTemp);
                                }
                                else { Debug.LogWarning("Invalid nodeTemp (Null)"); }
                            }
                            displayText = string.Format("{0}{1}{2}{3} Near Neighbouring district{4}{5}", colourNormal, listOfNearNeighbours.Count, colourEnd, colourCyber,
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
                List<Node> listOfCentreNodes = GameManager.i.dataScript.GetListOfAllNodes();
                if (listOfCentreNodes != null)
                {
                    if (listOfCentreNodes.Count > 0)
                    {
                        count = 0;
                        foreach (Node node in listOfCentreNodes)
                        {
                            if (node != null)
                            {
                                if (node.isCentreNode == true)
                                {
                                    node.SetActive();
                                    listOfHighlighted.Add(node);
                                    count++;
                                }
                            }
                            else { Debug.LogWarning("Invalid node (Null)"); }
                        }
                        displayText = string.Format("{0}{1}{2}{3} Centred district{4}{5}", colourNormal, count, colourEnd, colourCyber,
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
            case NodeUI.PlayerKnown:
                List<int> listOfInvisibleNodes = GameManager.i.missionScript.mission.npc.listOfInvisibleNodes;
                if (listOfInvisibleNodes != null)
                {
                    int counter = 0;
                    count = listOfInvisibleNodes.Count;
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            Node node = GameManager.i.dataScript.GetNode(listOfInvisibleNodes[i]);
                            if (node != null)
                            {
                                node.SetActive();
                                listOfHighlighted.Add(node);
                                counter++;
                            }
                            else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}, listOfInvisibleNodes[{1}]", listOfInvisibleNodes[i], i); }
                        }
                        displayText = string.Format("There {0} {1} District{2} where the Player is Known", counter != 1 ? "are" : "is", counter, counter != 1 ? "s" : "");
                    }
                    else { displayText = "There are NO Districts where the Player is Known"; }
                }
                else
                {
                    Debug.LogWarning("Invalid listOfInvisibleNodes (Null)");
                    displayText = string.Format("{0}{1}{2}", colourError, "ERROR: Null listOfInvisibileNodes", colourEnd);
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
            List<Node> nodeList = GameManager.i.dataScript.GetListOfNodesByType(data);
            if (nodeList != null)
            {
                NodeArc nodeArc = GameManager.i.dataScript.GetNodeArc(data);
                if (nodeList.Count > 0)
                {
                    foreach (Node node in nodeList)
                    {
                        if (node != null)
                        {
                            node.SetActive();
                            listOfHighlighted.Add(node);
                        }
                    }
                    displayText = string.Format("{0}{1}{2} {3}{4}{5} {6}district{7}{8}", colourNormal, nodeList.Count, colourEnd,
                    colourCyber, nodeArc.name, colourEnd, colourNormal, nodeList.Count != 1 ? "s" : "", colourEnd);
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
            { GameManager.i.alertScript.SetAlertUI(displayText); }
        }
        return listOfHighlighted;
    }
    #endregion

    #region ShowActiveNodes
    /// <summary>
    /// Show all active nodes (Contacts) for a particular actor. Use actor.slotID (0 to numOfActors)
    /// </summary>
    /// <param name="slotID"></param>
    public List<Node> ShowActiveNodes(int slotID)
    {
        Debug.Assert(slotID > -1 && slotID < GameManager.i.actorScript.maxNumOfOnMapActors, "Invalid slotID");
        //set all nodes to default colour first
        ResetNodes();
        GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
        List<Node> tempNodeList = GameManager.i.dataScript.GetListOfActorContactNodes(slotID);
        int count = tempNodeList.Count;
        List<Node> activeNodes = new List<Node>();
        //Get Actor
        Actor actor = GameManager.i.dataScript.GetCurrentActor(slotID, GameManager.i.sideScript.PlayerSide);
        foreach (Node node in tempNodeList)
        {
            switch (playerSide.level)
            {
                case 1:
                    //authority - contact status always Active
                    if (GameManager.i.dataScript.CheckActiveContactAtNode(node.nodeID, playerSide) == true)
                    { activeNodes.Add(node); }
                    break;
                case 2:
                    //resistance - contact status can change
                    if (GameManager.i.dataScript.CheckForActorContactActive(actor, node.nodeID) == true)
                    { activeNodes.Add(node); }
                    break;
            }
        }
        if (actor != null)
        {
            string displayText = string.Format("{0}{1}{2} {3}{4} has{5} {6}Contacts{7} {8}in{9} {10}{11}{12} {13}district{14}{15} ", colourNormal, actor.arc.name, colourEnd, 
                colourCyber, actor.actorName, colourEnd, 
                colourNormal, colourEnd,
                colourCyber, colourEnd,
                colourNormal, count, colourEnd,
                colourCyber, count != 1 ? "s" : "", colourEnd);
            GameManager.i.alertScript.SetAlertUI(displayText, 999);
            NodeShowFlag = 1;
        }
        return activeNodes;
    }
    #endregion

    #region SetShowPlayerNode
    /// <summary>
    /// Sets flag to show player node whenever a node redraw. Normally true but switch off if you want to flash the player node instead
    /// </summary>
    /// <param name="isShown"></param>
    public void SetShowPlayerNode(bool isShown = true)
    { showPlayerNode = isShown; }
    #endregion

    #region GetShowPlayerNoe
    public bool GetShowPlayerNode()
    { return showPlayerNode; }
    #endregion

    #region RedrawNodes
    /// <summary>
    /// Redraw any nodes. Show highlighted node, unless it's a non-normal node for the current redraw
    /// </summary>
    public void RedrawNodes()
    {
        bool proceedFlag = true;
        List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
        bool noNodes = GameManager.i.optionScript.noNodes;
        if (listOfNodes != null)
        {
            //loop all nodes & assign current materials to their renderers (changes colour on screen)
            foreach (Node node in listOfNodes)
            {
                if (noNodes == false)
                { SetNodeMaterial(node, NodeComponent.Cylinder, node.colourNode); }
                else
                {
                    SetNodeMaterial(node, NodeComponent.Base, node.colourBase);
                    SetNodeMaterial(node, NodeComponent.Towers, node.colourRear);
                }
            }
            //highlighted node
            if (nodeHighlight > -1)
            {
                Node node = GameManager.i.dataScript.GetNode(nodeHighlight);
                if (node != null)
                {
                    if (noNodes == false)
                    {
                        //only do so if it's a normal node, otherwise ignore
                        if (node.GetMaterial(NodeComponent.Cylinder) == materialNormal)
                        { SetNodeMaterial(node, NodeComponent.Cylinder, NodeColour.Highlight); }
                    }
                    else
                    {
                        if (node.GetMaterial(NodeComponent.Base) == materialNormal)
                        {
                            SetNodeMaterial(node, NodeComponent.Base, NodeColour.Highlight);
                            SetNodeMaterial(node, NodeComponent.Towers, NodeColour.Highlight);
                        }
                    }
                }
                else { Debug.LogError("Invalid Node (null) returned from listOfNodes"); }
            }
            //show player node if flag is true (default condition -> would be false only in case of flashing player Node)
            if (showPlayerNode == true)
            {
                //player's current node (Resistance side only if FOW ON)
                if (GameManager.i.sideScript.PlayerSide.level == globalAuthority.level)
                {
                    if (GameManager.i.optionScript.isFogOfWar == true)
                    { proceedFlag = false; }
                }
                if (proceedFlag == true)
                {
                    //Player node
                    if (nodePlayer > -1)
                    {
                        Node node = GameManager.i.dataScript.GetNode(nodePlayer);
                        if (node != null)
                        {
                            //only do so if it's a normal node, otherwise ignore
                            if (noNodes == false)
                            {
                                if (node.GetMaterial(NodeComponent.Cylinder) == materialNormal)
                                { SetNodeMaterial(node, NodeComponent.Cylinder, NodeColour.Player); }
                            }
                            else
                            {
                                if (node.GetMaterial(NodeComponent.Base) == materialNormal)
                                {
                                    SetNodeMaterial(node, NodeComponent.Base, NodeColour.Player);
                                    SetNodeMaterial(node, NodeComponent.Towers, NodeColour.TowerDark);
                                }
                            }
                        }
                    }
                }
                //Nemesis current node (Resistance side only if FOW ON & Nemesis present)

                /*
                proceedFlag = true;
                if (GameManager.i.sideScript.PlayerSide.level == globalResistance.level)
                {
                    //Nemesis has a separate FOW setting
                    if (GameManager.i.nemesisScript.isShown == false)
                    { proceedFlag = false; }
                    else if (GameManager.i.nemesisScript.nemesis == null)
                    { proceedFlag = false; }
                }
                */

                if (GameManager.i.nemesisScript.ShowNemesis() == true)
                {
                    //Nemesis node
                    if (nodeNemesis > -1)
                    {
                        Node node = GameManager.i.dataScript.GetNode(nodeNemesis);
                        if (node != null)
                        {
                            //only do so if it's a normal node, otherwise ignore
                            if (noNodes == false)
                            {
                                if (node.GetMaterial(NodeComponent.Cylinder) == materialNormal)
                                { SetNodeMaterial(node, NodeComponent.Cylinder, NodeColour.Nemesis); }
                            }
                            else
                            {
                                if (node.GetMaterial(NodeComponent.Base) == materialNormal)
                                { SetNodeMaterial(node, NodeComponent.Base, NodeColour.Nemesis); }
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
    #endregion

    #region RedrawNodesArchive
    /*/// <summary>
    /// Redraw any nodes. Show highlighted node, unless it's a non-normal node for the current redraw
    /// </summary>
    public void RedrawNodesArchive()
    {
        bool proceedFlag = true;
        List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
        bool noNodes = GameManager.i.optionScript.noNodes;
        if (listOfNodes != null)
        {
            //loop all nodes & assign current materials to their renderers (changes colour on screen)
            foreach (Node node in listOfNodes)
            {
                if (noNodes == false)
                {
                    node.nodeRenderer.material = node._MaterialNode;
                    SetNodeMaterial(node, NodeColour.Normal, NodeComponent.Cylinder);
                }
                else { node.nodeRenderer.material = node._MaterialBase; }
            }
            //highlighted node
            if (nodeHighlight > -1)
            {
                Node node = GameManager.i.dataScript.GetNode(nodeHighlight);
                if (node != null)
                {
                    if (noNodes == false)
                    {
                        //only do so if it's a normal node, otherwise ignore
                        if (node.GetMaterial(NodeComponent.Cylinder) == materialNormal)
                        { node.nodeRenderer.material = materialHighlight; }
                    }
                    else
                    {

                    }
                }
                else { Debug.LogError("Invalid Node (null) returned from listOfNodes"); }
            }
            //show player node if flag is true (default condition -> would be false only in case of flashing player Node)
            if (showPlayerNode == true)
            {
                //player's current node (Resistance side only if FOW ON)
                if (GameManager.i.sideScript.PlayerSide.level == globalAuthority.level)
                {
                    if (GameManager.i.optionScript.fogOfWar == true)
                    { proceedFlag = false; }
                }
                if (proceedFlag == true)
                {
                    //Player node
                    if (nodePlayer > -1)
                    {
                        Node node = GameManager.i.dataScript.GetNode(nodePlayer);
                        if (node != null)
                        {
                            //only do so if it's a normal node, otherwise ignore
                            if (node.GetMaterial(NodeComponent.Cylinder) == materialNormal)
                            { node.nodeRenderer.material = materialPlayer; }
                        }
                    }
                }
                //Nemesis current node (Resistance side only if FOW ON & Nemesis present)
                proceedFlag = true;
                if (GameManager.i.sideScript.PlayerSide.level == globalResistance.level)
                {
                    //Nemesis has a separate FOW setting
                    if (GameManager.i.nemesisScript.isShown == false)
                    { proceedFlag = false; }
                    else if (GameManager.i.nemesisScript.nemesis == null)
                    { proceedFlag = false; }
                }
                if (proceedFlag == true)
                {
                    //Nemesis node
                    if (nodeNemesis > -1)
                    {
                        Node node = GameManager.i.dataScript.GetNode(nodeNemesis);
                        if (node != null)
                        {
                            //only do so if it's a normal node, otherwise ignore
                            if (node.GetMaterial(NodeComponent.Cylinder) == materialNormal)
                            { node.nodeRenderer.material = materialNemesis; }
                        }
                    }
                }
            }
            //reset flag to prevent constant redraws
            NodeRedraw = false;
        }
        else { Debug.LogError("Invalid listOfNodes (Null) returned from dataManager in RedrawNodes"); }
    }*/
    #endregion

    #region ResetNodes
    /// <summary>
    /// Sets the material colour of all nodes to the default (doesn't change on screen, just sets them up). Call before making any changes to node colours
    /// </summary>
    public void ResetNodes()
    {
        //stop flashing node coroutine if running
        if (myFlashCoroutine != null)
        {
            StopCoroutine(myFlashCoroutine);
            myFlashCoroutine = null;
        }
        //loop and assign
        List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
        bool noNodes = GameManager.i.optionScript.noNodes;
        if (listOfNodes != null)
        {
            foreach (Node node in listOfNodes)
            {

                if (noNodes == false)
                {
                    node.colourNode = NodeColour.Normal;
                    if (node.defaultChar != '\0')
                    { node.faceText.text = string.Format("{0}", node.defaultChar); }
                    else { node.faceText.text = ""; }
                    //colourNormal
                    node.faceText.color = new Color32(255, 255, 224, 202);
                }
                else
                {
                    //normal cylindrical nodes
                    node.colourBase = NodeColour.Normal;
                    node.colourRear = NodeColour.TowerDark;
                    node.colourRight = NodeColour.TowerLight;
                    node.colourLeft = NodeColour.TowerLight;
                }
            }
            //trigger an automatic redraw
            NodeRedraw = true;
        }
        else { Debug.LogError("Invalid listOfNodes (Null) returned from dataManager in ResetNodes"); }
    }
    #endregion

    #region SetNodeType
    /// <summary>
    /// Sets type of node display (node or district styles) and updates board
    /// </summary>
    /// <param name="nodeType"></param>
    public void SetNodeType(NodeType nodeType)
    {
        List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
        if (listOfNodes != null)
        {
            switch (nodeType)
            {
                case NodeType.Node:
                    GameManager.i.optionScript.noNodes = false;
                    //set all nodes to normal
                    foreach (Node node in listOfNodes)
                    {
                        if (node != null)
                        {
                            SetNodeMaterial(node, NodeComponent.Cylinder, NodeColour.Normal);
                        }
                        else { Debug.LogError("Invalid node (Null) in listOfNodes"); }
                    }
                    //set player node
                    if (nodePlayer > -1)
                    {
                        Node playerNode = GameManager.i.dataScript.GetNode(nodePlayer);
                        if (playerNode != null)
                        { SetNodeMaterial(playerNode, NodeComponent.Cylinder, NodeColour.Player); }
                        else { Debug.LogErrorFormat("Invalid playerNode (Null) for nodeID {0}", nodePlayer); }
                    }
                    //set face text
                    SetDistrictFaceText(NodeText.Icon, false);
                    break;
                case NodeType.District:
                    GameManager.i.optionScript.noNodes = true;
                    //set all district bases to normal
                    foreach (Node node in listOfNodes)
                    {
                        if (node != null)
                        {
                            SetNodeMaterial(node, NodeComponent.Base, NodeColour.Normal);
                            SetNodeMaterial(node, NodeComponent.Cylinder, NodeColour.Invisible);
                        }
                        else { Debug.LogError("Invalid node (Null) in listOfNodes"); }
                        //set player node
                        if (nodePlayer > -1)
                        {
                            Node playerNode = GameManager.i.dataScript.GetNode(nodePlayer);
                            if (playerNode != null)
                            { SetNodeMaterial(playerNode, NodeComponent.Base, NodeColour.Player); }
                            else { Debug.LogErrorFormat("Invalid playerNode (Null) for nodeID {0}", nodePlayer); }
                        }
                        //clear any face text
                        SetDistrictFaceText(NodeText.None, false);
                    }
                    break;
            }
        }
        else { Debug.LogError("Invalid listOfNodes (Null)"); }
    }
    #endregion

    #region SetDistrictFaceText
    /// <summary>
    /// Set's district face text. 'isTransparent' shows a transparent cylinder (default) to enhance readability
    /// </summary>
    /// <param name="nodeText"></param>
    public void SetDistrictFaceText(NodeText nodeText, bool isTransparent = true)
    {
        int data;
        List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
        if (listOfNodes != null)
        {
            switch (nodeText)
            {
                case NodeText.None:
                    foreach (Node node in listOfNodes)
                    {
                        node.faceText.text = "";
                        SetNodeMaterial(node, NodeComponent.Cylinder, NodeColour.Invisible);
                    }
                    break;
                case NodeText.ID:
                    foreach (Node node in listOfNodes)
                    {
                        if (node != null)
                        {
                            data = node.nodeID;
                            node.faceText.text = data.ToString();
                            node.faceText.color = Color.yellow;
                            //Set transparent
                            if (isTransparent == true)
                            { SetNodeMaterial(node, NodeComponent.Cylinder, NodeColour.Transparent); }
                        }
                        else { Debug.LogError("Invalid node (Null) in listOfNodes"); }
                    }
                    break;
                case NodeText.Icon:
                    foreach (Node node in listOfNodes)
                    {
                        if (node != null)
                        {
                            data = node.nodeID;
                            node.colourNode = NodeColour.Normal;
                            if (node.defaultChar != '\0')
                            { node.faceText.text = string.Format("{0}", node.defaultChar); }
                            else { node.faceText.text = ""; }
                            //colourNormal
                            node.faceText.color = new Color32(255, 255, 224, 202);
                            if (isTransparent == true)
                            { SetNodeMaterial(node, NodeComponent.Cylinder, NodeColour.Transparent); }
                        }
                        else { Debug.LogError("Invalid node (Null) in listOfNodes"); }
                    }
                    break;
                case NodeText.Contact:
                    List<Contact> listOfContacts = new List<Contact>();
                    foreach (Node node in listOfNodes)
                    {
                        if (node != null)
                        {
                            //contact at node
                            if (GameManager.i.dataScript.CheckActiveContactAtNode(node.nodeID, globalResistance) == true)
                            {
                                listOfContacts = GameManager.i.dataScript.GetListOfNodeContacts(node.nodeID);
                                data = 0;
                                //loop contacts (could be more than one at the same node), get highest effectiveness
                                for (int i = 0; i < listOfContacts.Count; i++)
                                {
                                    Contact contact = listOfContacts[i];
                                    if (contact != null)
                                    {
                                        if (contact.effectiveness > data) { data = contact.effectiveness; }
                                    }
                                    else { Debug.LogErrorFormat("Invalid contact (Null) for listOfContacts[{0}], nodeID {1}", i, node.nodeID); }
                                }
                                node.faceText.text = data.ToString();
                                node.faceText.color = Color.yellow;
                                if (isTransparent == true)
                                { SetNodeMaterial(node, NodeComponent.Cylinder, NodeColour.Transparent); }
                            }
                        }
                        else { Debug.LogError("Invalid node (Null) in listOfNodes"); }
                    }
                    break;
                case NodeText.ActivityCount:
                    //Resistance AI activity
                    foreach (Node node in listOfNodes)
                    {
                        if (node != null)
                        {
                            if (node.faceObject != null)
                            {
                                data = node.GetNodeActivity(ActivityUI.Count);
                                if (data > -1)
                                {
                                    //face text
                                    node.faceText.text = string.Format("<size=120%>{0}</size>", data.ToString());
                                    node.faceText.color = GetActivityColour(ActivityUI.Count, data);
                                    if (isTransparent == true)
                                    { SetNodeMaterial(node, NodeComponent.Cylinder, NodeColour.Transparent); }
                                }
                            }
                            else { Debug.LogWarning(string.Format("Invalid node faceObject (Null) for nodeID {0}", node.nodeID)); }
                        }
                        else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", node.nodeID)); }
                    }
                    break;
                case NodeText.ActivityTime:
                    //Resistance AI activity
                    foreach (Node node in listOfNodes)
                    {
                        if (node != null)
                        {
                            if (node.faceObject != null)
                            {
                                data = node.GetNodeActivity(ActivityUI.Time);
                                if (data > -1)
                                {
                                    //face text
                                    node.faceText.text = string.Format("<size=120%>{0}</size>", data.ToString());
                                    node.faceText.color = GetActivityColour(ActivityUI.Time, data);
                                    if (isTransparent == true)
                                    { SetNodeMaterial(node, NodeComponent.Cylinder, NodeColour.Transparent); }
                                }
                            }
                            else { Debug.LogWarning(string.Format("Invalid node faceObject (Null) for nodeID {0}", node.nodeID)); }
                        }
                        else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", node.nodeID)); }
                    }
                    break;
                default: Debug.LogWarningFormat("Unrecogonised nodeText \"{0}\"", nodeText); break;
            }
        }
        else { Debug.LogError("Invalid listOfNodes (Null) returned from dataManager in SetDistrictFaceText"); }
    }
    #endregion

    #region SetNodeMaterial
    /// <summary>
    /// Use this to change a node/districts material settings
    /// </summary>
    /// <param name="node"></param>
    /// <param name="nodeType"></param>
    private void SetNodeMaterial(Node node, NodeComponent nodeComponent, NodeColour nodeType)
    {
        Material material = materialTowerDark;
        Debug.Assert(node != null, "Invalid node (Null)");
        {
            switch (nodeType)
            {
                case NodeColour.TowerDark: material = materialTowerDark; break;
                case NodeColour.TowerLight: material = materialTowerLight; break;
                case NodeColour.TowerActive: material = materialTowerActive; break;
                case NodeColour.Normal: material = materialNormal; break;
                case NodeColour.Highlight: material = materialHighlight; break;
                case NodeColour.Active: material = materialActive; break;
                case NodeColour.Player: material = materialPlayer; break;
                case NodeColour.Background: material = materialBackground; break;
                case NodeColour.Nemesis: material = materialNemesis; break;
                case NodeColour.Invisible: material = materialInvisible; break;
                case NodeColour.Transparent: material = materialTransparent; break;
                default: Debug.LogWarningFormat("Unrecognised NodeType \"{0}\"", nodeType); break;
            }
            //special case of setting towers back to normal (requires a binary colour scheme)
            if (nodeComponent == NodeComponent.Towers && nodeType == NodeColour.TowerDark)
            {
                node.SetMaterial(materialTowerDark, NodeComponent.TowerRear);
                node.SetMaterial(materialTowerLight, NodeComponent.TowerLeftRight);
            }
            else { node.SetMaterial(material, nodeComponent); }
        }
    }
    #endregion

    #region ShowActivity
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
                if (GameManager.i.optionScript.noNodes == false)
                { ShowNodeActivity(activityUI); }
                else { SetDistrictFaceText(NodeText.ActivityCount); }
                break;
            case ActivityUI.Time:
                //change connections to reflect activity (also sets resetNeeded to True)
                GameManager.i.connScript.ShowConnectionActivity(activityUI);
                //activate face text on nodes to reflect activity levels
                if (GameManager.i.optionScript.noNodes == false)
                { ShowNodeActivity(activityUI); }
                else { SetDistrictFaceText(NodeText.ActivityTime); }
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
                displayText = "Resistance Activity by Count";
                break;
            case ActivityUI.Time:
                displayText = "Resistance Activity by Time";
                break;
        }
        //active AlertUI
        if (string.IsNullOrEmpty(displayText) == false)
        { GameManager.i.alertScript.SetAlertUI(displayText); }
        //redraw only if standard node representation
        if (GameManager.i.optionScript.noNodes == false)
        { NodeRedraw = true; }
    }

    /// <summary>
    /// Highlight activity levels on node faces. Assumes Node representation (OptionManager.cs -> noNodes FALSE)
    /// </summary>
    /// <param name="activityUI"></param>
    private void ShowNodeActivity(ActivityUI activityUI)
    {
        if (activityUI != ActivityUI.None)
        {
            int data;
            List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
            if (listOfNodes != null)
            {
                //loop nodes
                bool errorFlag = false;
                if (GameManager.i.optionScript.noNodes == false)
                {
                    foreach (Node node in listOfNodes)
                    {
                        if (node != null)
                        {
                            if (node.faceObject != null)
                            {
                                switch (activityUI)
                                {
                                    case ActivityUI.Count:
                                    case ActivityUI.Time:
                                        data = node.GetNodeActivity(activityUI);
                                        if (data > -1)
                                        {
                                            //face text
                                            node.faceText.text = string.Format("<size=120%>{0}</size>", data.ToString());
                                            node.faceText.color = GetActivityColour(activityUI, data);
                                        }
                                        else
                                        {
                                            node.faceText.text = "";
                                            SetNodeMaterial(node, NodeComponent.Cylinder, NodeColour.Background);
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
                else { Debug.LogWarningFormat("Can't show activity as OptionManager.cs -> noNodes is {0}", GameManager.i.optionScript.noNodes); }
            }
            else { Debug.LogError("Invalid listOfNodes (Null)"); }
        }
    }
    #endregion

    #region ShowAllNodes
    /// <summary>
    /// Displays nodeID's on node faces
    /// </summary>
    /// <param name="highlightID"></param>
    public void ShowAllNodeID()
    {
        int data;
        List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
        if (listOfNodes != null)
        {
            bool noNodes = GameManager.i.optionScript.noNodes;
            //loop nodes
            foreach (Node node in listOfNodes)
            {
                if (node != null)
                {
                    if (node.faceObject != null)
                    {
                        data = node.nodeID;
                        if (noNodes == false)
                        {
                            node.faceText.text = data.ToString();
                            node.faceText.color = Color.yellow;
                        }
                    }
                    else { Debug.LogWarning(string.Format("Invalid node faceObject (Null) for nodeID {0}", node.nodeID)); }
                }
                else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", node.nodeID)); }
            }
        }
        else { Debug.LogError("Invalid listOfNodes (Null)"); }
    }
    #endregion

    #region ShowAllContacts
    /// <summary>
    /// displays all contacts with the highest contact at the node displaying their effectiveness on the node face. Called by event via ShowNodes (for option.noNodes false, SetDistrictText otherwise)
    /// </summary>
    private void ShowAllContacts()
    {
        int data;
        List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
        List<Contact> listOfContacts = new List<Contact>();
        //loop nodes
        foreach (Node node in listOfNodes)
        {
            if (node != null)
            {
                //contact at node
                if (GameManager.i.dataScript.CheckActiveContactAtNode(node.nodeID, globalResistance) == true)
                {
                    listOfContacts = GameManager.i.dataScript.GetListOfNodeContacts(node.nodeID);
                    data = 0;
                    //loop contacts (could be more than one at the same node), get highest effectiveness
                    for (int i = 0; i < listOfContacts.Count; i++)
                    {
                        Contact contact = listOfContacts[i];
                        if (contact != null)
                        {
                            if (contact.effectiveness > data) { data = contact.effectiveness; }
                        }
                        else { Debug.LogErrorFormat("Invalid contact (Null) for listOfContacts[{0}], nodeID {1}", i, node.nodeID); }
                    }
                    node.faceText.text = data.ToString();
                    node.faceText.color = Color.yellow;
                }
            }
            else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", node.nodeID)); }
        }
    }
    #endregion

    #region GetActivityColour
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
                        if (data > GameManager.i.aiScript.activityTimeLimit)
                        { color = Color.clear; }
                        else { color = Color.black; }
                        break;
                }
                break;
        }
        return color;
    }
    #endregion

    #region ResetAll
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
            GameManager.i.alertScript.CloseAlertUI();
            if (GameManager.i.connScript.resetConnections == true)
            {
                //return Connections to previously saved state prior to any changes
                GameManager.i.connScript.RestoreConnections();
            }
            NodeShowFlag = 0;
            //reset state
            activityState = ActivityUI.None;
            //DEBUG
            GameManager.i.debugGraphicsScript.SetCentrePane(false);
        }
    }
    #endregion

    #region CreateSpecialNodeMenu
    /// <summary>
    /// Right Click the Resistance Player's current node -> special actions (Gear and Cure)
    /// </summary>
    /// <param name="nodeID"></param>
    private void CreateSpecialNodeMenu(int nodeID)
    {
        Debug.LogFormat("[UI] NodeManager.cs -> CreateGearNodeMenu{0}", "\n");
        int counter = 0;                    //num of gear that can be used
        List<EventButtonDetails> tempList = new List<EventButtonDetails>();
        string gearCompromise;
        //Get Node
        Node node = GameManager.i.dataScript.GetNode(nodeID);
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
                List<Gear> listOfKineticGear = GameManager.i.playerScript.GetListOfGearType(GameManager.i.gearScript.typeKinetic);
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
                                            effectCriteria = GameManager.i.effectScript.CheckCriteria(criteriaInput);
                                            if (effectCriteria == null)
                                            {
                                                //Effect criteria O.K -> tool tip text
                                                if (builder.Length > 0) { builder.AppendLine(); }
                                                if (effect.outcome.name.Equals("Power", StringComparison.Ordinal) == false && effect.outcome.name.Equals("Invisibility", StringComparison.Ordinal) == false)
                                                { builder.Append(string.Format("{0}{1}{2}", colourEffect, effect.description, colourEnd)); }
                                                else
                                                {
                                                    //Invisibility and Power -> player affected (good for Power, bad for invisibility)
                                                    if (effect.outcome.name.Equals("Power", StringComparison.Ordinal) == true)
                                                    { builder.Append(string.Format("{0}Player {1}{2}", colourGood, effect.description, colourEnd)); }
                                                    else
                                                    { builder.Append(string.Format("{0}Player {1}{2}", colourBad, effect.description, colourEnd)); }
                                                }
                                                //Mood (special case)
                                                if (effect.isMoodEffect == true)
                                                {
                                                    string moodText = GameManager.i.personScript.GetMoodTooltip(effect.belief, "Player");
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

                                        actionDetails.side = GameManager.i.globalScript.sideResistance;
                                        actionDetails.nodeID = nodeID;
                                        actionDetails.gearAction = actionKinetic;
                                        actionDetails.gearName = kineticGear.name;
                                        //gear compromise tooltip
                                        gearCompromise = string.Format("{0}Chance of Gear being Compromised {1}{2}{3}%{4}", colourAlert, colourEnd,
                                            colourNeutral, GameManager.i.gearScript.GetChanceOfCompromise(kineticGear.name), colourEnd);
                                        //pass all relevant details to ModalActionMenu via Node.OnClick()
                                        EventButtonDetails kineticDetails = new EventButtonDetails()
                                        {
                                            buttonTitle = string.Format("Use {0}", kineticGear.tag),
                                            buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, kineticGear.tag, colourEnd),
                                            buttonTooltipMain = tempAction.tooltipText,
                                            buttonTooltipDetail = string.Format("{0}{1}{2}", gearCompromise, "\n", builder.ToString()),
                                            //use a Lambda to pass arguments to the action
                                            action = () => { EventManager.i.PostNotification(EventType.NodeGearAction, this, actionDetails, "NodeManager.cs -> CreateGearNodeMenu"); }
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
                List<Gear> listOfHackingGear = GameManager.i.playerScript.GetListOfGearType(GameManager.i.gearScript.typeHacking);
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
                                            effectCriteria = GameManager.i.effectScript.CheckCriteria(criteriaInput);
                                            if (effectCriteria == null)
                                            {
                                                //Effect criteria O.K -> tool tip text
                                                if (builder.Length > 0) { builder.AppendLine(); }
                                                if (effect.outcome.name.Equals("Power", StringComparison.Ordinal) == false && effect.outcome.name.Equals("Invisibility", StringComparison.Ordinal) == false)
                                                { builder.Append(string.Format("{0}{1}{2}", colourEffect, effect.description, colourEnd)); }
                                                else
                                                {
                                                    //Invisibility and Power -> player affected (good for Power, bad for invisibility)
                                                    if (effect.outcome.name.Equals("Power", StringComparison.Ordinal) == true)
                                                    { builder.Append(string.Format("{0}Player {1}{2}", colourGood, effect.description, colourEnd)); }
                                                    else
                                                    { builder.Append(string.Format("{0}Player {1}{2}", colourBad, effect.description, colourEnd)); }
                                                }
                                                //Mood (special case)
                                                if (effect.isMoodEffect == true)
                                                {
                                                    string moodText = GameManager.i.personScript.GetMoodTooltip(effect.belief, "Player");
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

                                        actionDetails.side = GameManager.i.globalScript.sideResistance;
                                        actionDetails.nodeID = nodeID;
                                        actionDetails.gearAction = actionHacking;
                                        actionDetails.gearName = hackingGear.name;
                                        //gear compromise tooltip
                                        gearCompromise = string.Format("{0}Chance of Gear being Compromised {1}{2}{3}%{4}", colourAlert, colourEnd,
                                            colourNeutral, GameManager.i.gearScript.GetChanceOfCompromise(hackingGear.name), colourEnd);
                                        //pass all relevant details to ModalActionMenu via Node.OnClick()
                                        EventButtonDetails hackingDetails = new EventButtonDetails()
                                        {
                                            buttonTitle = string.Format("Use {0}", hackingGear.tag),
                                            buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, hackingGear.tag, colourEnd),
                                            buttonTooltipMain = tempAction.tooltipText,
                                            buttonTooltipDetail = string.Format("{0}{1}{2}", gearCompromise, "\n", builder.ToString()),
                                            //use a Lambda to pass arguments to the action
                                            action = () => { EventManager.i.PostNotification(EventType.NodeGearAction, this, actionDetails, "NodeManager.cs -> CreateGearNodeMenu"); }
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
                List<Gear> listOfPersuasionGear = GameManager.i.playerScript.GetListOfGearType(GameManager.i.gearScript.typePersuasion);
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
                                            effectCriteria = GameManager.i.effectScript.CheckCriteria(criteriaInput);
                                            if (effectCriteria == null)
                                            {
                                                //Effect criteria O.K -> tool tip text
                                                if (builder.Length > 0) { builder.AppendLine(); }
                                                if (effect.outcome.name.Equals("Power", StringComparison.Ordinal) == false && effect.outcome.name.Equals("Invisibility", StringComparison.Ordinal) == false)
                                                { builder.Append(string.Format("{0}{1}{2}", colourEffect, effect.description, colourEnd)); }
                                                else
                                                {
                                                    //Invisibility and Power -> player affected (good for Power, bad for invisibility)
                                                    if (effect.outcome.name.Equals("Power", StringComparison.Ordinal))
                                                    { builder.Append(string.Format("{0}Player {1}{2}", colourGood, effect.description, colourEnd)); }
                                                    else
                                                    { builder.Append(string.Format("{0}Player {1}{2}", colourBad, effect.description, colourEnd)); }
                                                }
                                                //Mood (special case)
                                                if (effect.isMoodEffect == true)
                                                {
                                                    string moodText = GameManager.i.personScript.GetMoodTooltip(effect.belief, "Player");
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

                                        actionDetails.side = GameManager.i.globalScript.sideResistance;
                                        actionDetails.nodeID = nodeID;
                                        actionDetails.gearAction = actionPersuasion;
                                        actionDetails.gearName = persuasionGear.name;
                                        //gear compromise tooltip
                                        gearCompromise = string.Format("{0}Chance of Gear being Compromised {1}{2}{3}%{4}", colourAlert, colourEnd,
                                            colourNeutral, GameManager.i.gearScript.GetChanceOfCompromise(persuasionGear.name), colourEnd);
                                        //pass all relevant details to ModalActionMenu via Node.OnClick()
                                        EventButtonDetails persuasionDetails = new EventButtonDetails()
                                        {
                                            buttonTitle = string.Format("Use {0}", persuasionGear.tag),
                                            buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, persuasionGear.tag, colourEnd),
                                            buttonTooltipMain = tempAction.tooltipText,
                                            buttonTooltipDetail = string.Format("{0}{1}{2}", gearCompromise, "\n", builder.ToString()),
                                            //use a Lambda to pass arguments to the action
                                            action = () => { EventManager.i.PostNotification(EventType.NodeGearAction, this, actionDetails, "NodeManager.cs -> CreateGearNodeMenu"); }
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
                    cureActionDetails.actorDataID = GameManager.i.playerScript.actorID;
                    cureActionDetails.powerCost = 0;
                    EventButtonDetails cureDetails = new EventButtonDetails()
                    {
                        buttonTitle = node.cure.cureName,
                        buttonTooltipHeader = string.Format("{0}{1}{2}", colourResistance, "Cure Condition", colourEnd),
                        buttonTooltipMain = string.Format("Remove {0}{1}{2} condition", colourBad, node.cure.condition.tag, colourEnd),
                        buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, node.cure.tooltipText, colourEnd),
                        //use a Lambda to pass arguments to the action
                        action = () => { EventManager.i.PostNotification(EventType.CurePlayerAction, this, cureActionDetails, "NodeManager.cs -> CreateGearMenu"); }
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
                if (GameManager.i.playerScript.CheckNumOfGear() > 0)
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
                                action = () => { EventManager.i.PostNotification(EventType.CloseActionMenu, this, null, "NodeManager.cs -> CreateGearNodeMenu"); }
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
                                action = () => { EventManager.i.PostNotification(EventType.CloseActionMenu, this, null, "NodeManager.cs -> CreateGearNodeMenu"); }
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
                            action = () => { EventManager.i.PostNotification(EventType.CloseActionMenu, this, null, "NodeManager.cs -> CreateGearNodeMenu"); }
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
                        action = () => { EventManager.i.PostNotification(EventType.CloseActionMenu, this, null, "NodeManager.cs -> CreateGearNodeMenu"); }
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
        GameManager.i.actionMenuScript.SetActionMenu(details);
    }
    #endregion

    #region Player Movement...

    #region CreateMoveMenu
    /// <summary>
    /// Put an action Menu together at the selected node that has 'Move' and 'Cancel' options
    /// </summary>
    /// <param name="nodeID"></param>
    private void CreateMoveMenu(int nodeID)
    {
        Debug.LogFormat("[UI] NodeManager.cs -> CreateMoveMenu: CreateMoveMenu (right click on node)");

        List<EventButtonDetails> tempList = new List<EventButtonDetails>();
        int playerNodeID = GetPlayerNodeID();
        //Get Node
        Node node = GameManager.i.dataScript.GetNode(nodeID);
        if (node != null)
        {
            string moveHeader = string.Format("{0}\"{1}\", {2}{3}{4}{5}, ID {6}{7}", colourResistance, node.nodeName, colourEnd, "\n",
                colourDefault, node.Arc.name, node.nodeID, colourEnd);
            string moveMain = "UNKNOWN";
            int adjustInvisibility = 0;
            //check player has special move gear and node to move to is 2 nodes distance away
            if (GameManager.i.playerScript.isSpecialMoveGear == true && GameManager.i.dijkstraScript.GetDistanceUnweighted(playerNodeID, nodeID) == 2)
            {
                //
                // - - - Special gear Move 2 nodes
                //
                Gear gear = GameManager.i.gearScript.gearSpecialMove;
                if (gear != null)
                {
                    //tooltip details
                    StringBuilder builderDetail = new StringBuilder();

                    builderDetail.Append(string.Format("{0}No risk of being spotted{1}", colourGood, colourEnd));
                    //add gear chance of compromise
                    builderDetail.Append(string.Format("{0}{1}Gear has a {2}{3}{4} %{5}{6} chance of being compromised{7}", "\n", colourAlert, colourEnd, colourNeutral,
                        GameManager.i.gearScript.GetChanceOfCompromise(gear.name), colourEnd, colourAlert, colourEnd));
                    //gear handles security level
                    adjustInvisibility = 0;
                    //Move details
                    ModalMoveDetails moveGearDetails = new ModalMoveDetails();
                    moveGearDetails.nodeID = nodeID;
                    moveGearDetails.connectionID = 0;  //can set as a random value as it's ignored by ProcessPlayerMove if changeInvisibility is Zero
                    moveGearDetails.changeInvisibility = adjustInvisibility;
                    moveGearDetails.gearName = gear.name;
                    //button target details (red for High security to match red connection security colour on map)
                    string colourGearLevel = colourBad;
                    EventButtonDetails eventMoveDetails = new EventButtonDetails()
                    {
                        buttonTitle = string.Format("{0} Move", gear.tag),
                        buttonTooltipHeader = string.Format("Move using{0}{1}{2}{3}{4}{5}<size=90%>defeats</size> {6}{7}{8}{9} <size=90%>Security</size>{10}", "\n", colourNeutral, gear.tag, colourEnd,
                        "\n", colourNormal, colourEnd, colourGearLevel, (ConnectionType)gear.data, colourEnd, colourNormal, colourEnd),
                        buttonTooltipMain = string.Format("{0}Underground Movement{1}", colourGood, colourEnd),
                        buttonTooltipDetail = builderDetail.ToString(),
                        //use a Lambda to pass arguments to the action
                        action = () => { EventManager.i.PostNotification(EventType.MoveAction, this, moveGearDetails, "NodeManager.cs -> CreateMoveMenu"); }
                    };
                    tempList.Add(eventMoveDetails);
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
                        action = () => { EventManager.i.PostNotification(EventType.CloseActionMenu, this, null, "NodeManager.cs -> CreateMoveMenu"); }
                    };
                    //add Cancel button to list
                    tempList.Add(cancelDetails);
                }
                else { Debug.LogError("Invalid gearManager.cs -> gearSpecialMove (Null)"); }
            }
            else
            {
                //
                // - - - Normal Move (could include special gear but only moving one node away)
                //
                //Get Connection (between new node and Player's current location)
                Connection connection = node.GetConnection(playerNodeID);
                if (connection != null)
                {
                    ConnectionType connSecType = connection.SecurityLevel;
                    int secLevel = (int)connSecType;
                    if (secLevel == 0) { secLevel = 4; } //need to do this to get the default colour as 0 is regarded as terrible normally
                    moveMain = string.Format("Connection Security{0}{1}{2}{3}", "\n", GameManager.i.colourScript.GetValueColour(secLevel), connSecType, colourEnd);
                    //
                    // - - - Gear (Movement) only if connection has a security level
                    //
                    if (GameManager.i.optionScript.isMoveSecurity == true)
                    {
                        if (secLevel < 4)
                        {
                            List<string> listOfGear = GameManager.i.playerScript.GetListOfGear();
                            if (listOfGear.Count > 0)
                            {
                                string movement = GameManager.i.gearScript.typeMovement.name;
                                for (int i = 0; i < listOfGear.Count; i++)
                                {
                                    StringBuilder builderDetail = new StringBuilder();
                                    Gear gear = GameManager.i.dataScript.GetGear(listOfGear[i]);
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
                                                if (GameManager.i.playerScript.Invisibility <= 0)
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
                                                GameManager.i.gearScript.GetChanceOfCompromise(gear.name), colourEnd, colourAlert, colourEnd));

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
                                                buttonTooltipHeader = string.Format("Move using{0}{1}{2}{3}{4}{5}<size=90%>defeats</size> {6}{7}{8}{9} <size=90%>Security</size>{10}", "\n",
                                                colourNeutral, gear.tag, colourEnd, "\n", colourNormal, colourEnd, colourGearLevel, (ConnectionType)gear.data, colourEnd, colourNormal, colourEnd),
                                                buttonTooltipMain = moveMain,
                                                buttonTooltipDetail = builderDetail.ToString(),
                                                //use a Lambda to pass arguments to the action
                                                action = () => { EventManager.i.PostNotification(EventType.MoveAction, this, moveGearDetails, "NodeManager.cs -> CreateMoveMenu"); }
                                            };
                                            tempList.Add(eventMoveDetails);
                                        }
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
                    if (GameManager.i.optionScript.isMoveSecurity == true)
                    {
                        if (secLevel < 4)
                        {
                            //player loses one level of invisibility each time they traverse a security rated connection
                            adjustInvisibility = -1;
                            if (GameManager.i.playerScript.Invisibility <= 0)
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
                        buttonTooltipHeader = string.Format("{0}Move{1}", colourNeutral, colourEnd),
                        buttonTooltipMain = moveMain,
                        buttonTooltipDetail = moveDetail,
                        //use a Lambda to pass arguments to the action
                        action = () => { EventManager.i.PostNotification(EventType.MoveAction, this, moveDetails, "NodeManager.cs -> CreateMoveMenu"); }
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
                        action = () => { EventManager.i.PostNotification(EventType.CloseActionMenu, this, null, "NodeManager.cs -> CreateMoveMenu"); }
                    };
                    //add Cancel button to list
                    tempList.Add(cancelDetails);
                }
                else { Debug.LogError("Invalid Connection (Null)"); }
            }
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
        GameManager.i.actionMenuScript.SetActionMenu(details);
    }
    #endregion

    #region ProcessPlayerMove
    /// <summary>
    /// Move player to the specified node
    /// </summary>
    /// <param name="nodeID"></param>
    private void ProcessPlayerMove(ModalMoveDetails moveDetails)
    {
        if (moveDetails != null)
        {

            /*Debug.LogFormat("[Tst] NodeManager.cs -> ProcessPlayerMove: ModalMoveDetails nodeID {0}, change {1}, delay {2}{3}", moveDetails.nodeID, moveDetails.changeInvisibility, moveDetails.ai_Delay, "\n");*/

            Node node = GameManager.i.dataScript.GetNode(moveDetails.nodeID);
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
                string destination = string.Format("Moved to {0}{1}{2}, {3}{4}{5} district", colourNeutral, node.nodeName, colourEnd, colourAlert, node.Arc.name, colourEnd);
                StringBuilder builder = new StringBuilder();
                builder.Append(string.Format("{0}{1}", destination, "\n"));
                //message
                string text = string.Format("You've moved to {0}", destination);
                GameManager.i.messageScript.PlayerMove(text, node, moveDetails.changeInvisibility, moveDetails.ai_Delay);
                //Statistics (needs to be before ProcessMoveOutcome calls for tutorial goals)
                GameManager.i.dataScript.StatisticIncrement(StatType.PlayerMoveActions);

                if (GameManager.i.optionScript.isMoveSecurity == true)
                {
                    //
                    // - - - Invisibility - - -
                    //
                    if (moveDetails.changeInvisibility != 0)
                    {
                        Connection connection = GameManager.i.dataScript.GetConnection(moveDetails.connectionID);
                        if (connection != null)
                        {
                            //statistics
                            if (connection.SecurityLevel != ConnectionType.None)
                            { GameManager.i.dataScript.StatisticIncrement(StatType.PlayerMoveSecureConnections); }
                            //AI message
                            string textAI = string.Format("Player spotted moving to \"{0}\", {1}, ID {2}", node.nodeName, node.Arc.name, moveDetails.nodeID);
                            //display
                            builder.AppendLine();
                            builder.AppendFormat("{0}Invisibility {1}{2}{3}", colourBad, moveDetails.changeInvisibility > 0 ? "+" : "",
                                moveDetails.changeInvisibility, colourEnd);
                            //player invisibility
                            int invisibility = GameManager.i.playerScript.Invisibility;
                            if (invisibility <= 0)
                            {
                                //moving while invisibility already 0 triggers immediate alert flag
                                GameManager.i.aiScript.immediateFlagResistance = true;
                                builder.AppendFormat("{0}{1}{2}Authority will know immediately{3}", "\n", "\n", colourBad, colourEnd);
                                //AI Immediate notification
                                GameManager.i.messageScript.AIImmediateActivity("Immediate Activity \"Move\" (Player)",
                                    "Moving", moveDetails.nodeID, moveDetails.connectionID);
                                //Npc invisible node
                                if (GameManager.i.missionScript.mission.npc != null)
                                { GameManager.i.missionScript.mission.npc.AddInvisibleNode(moveDetails.nodeID); }
                            }
                            else
                            {
                                //normal adjusted security level delay, eg. low gives a 3 turn delay
                                builder.AppendFormat("{0}{1}{2}Authority will know in {3}{4}{5}{6}{7} turn{8}{9}", "\n", "\n", colourAlert, colourEnd, colourNeutral,
                                    moveDetails.ai_Delay, colourEnd, colourAlert, moveDetails.ai_Delay != 1 ? "s" : "", colourEnd);
                                //AI delayed notification
                                GameManager.i.messageScript.AIConnectionActivity(textAI, node, connection, moveDetails.ai_Delay);
                            }
                            //update invisibility
                            invisibility += moveDetails.changeInvisibility;
                            invisibility = Mathf.Max(0, invisibility);
                            GameManager.i.playerScript.Invisibility = invisibility;
                            //popUp
                            GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, $"Invisibility {moveDetails.changeInvisibility}");
                        }
                        else { Debug.LogErrorFormat("Invalid connection (Null) for connectionID {0}", moveDetails.connectionID); }
                    }
                    /*else
                    {
                        builder.AppendLine();
                        builder.Append(string.Format("{0}Player not spotted{1}", colourGood, colourEnd));
                    }*/
                    //
                    // - - - Gear - - -
                    //
                    if (string.IsNullOrEmpty(moveDetails.gearName) == false)
                    {
                        Gear gear = GameManager.i.dataScript.GetGear(moveDetails.gearName);
                        if (gear != null)
                        {
                            //popUp
                            GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, $"{gear.tag} used");
                            //process move
                            MoveReturnData moveData = new MoveReturnData();
                            if (moveDetails.changeInvisibility != 0)
                            {
                                moveData.isChangeInvisibility = true;
                                builder.AppendFormat("{0}{1}{2}{3}{4}{5} used to minimise recognition{6}", "\n", "\n", colourNeutral, gear.tag, colourEnd, colourNormal, colourEnd);
                                GameManager.i.gearScript.SetGearUsed(gear, "move with as little recognition as possible");
                            }
                            else
                            {
                                moveData.isChangeInvisibility = false;
                                builder.AppendFormat("{0}{1}{2}{3}{4} used to avoid detection{5}", "\n", colourNeutral, gear.tag, colourEnd, colourNormal, colourEnd);
                                GameManager.i.gearScript.SetGearUsed(gear, "avoid detection while moving");
                            }
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
                            moveData.isChangeInvisibility = true;
                            ProcessMoveOutcome(moveData);
                        }
                        else
                        {
                            //Unsecured connection, no invisibility loss involved

                            /*EventManager.i.PostNotification(EventType.UseAction, this, "Player Move", "NodeManager.cs -> ProcessPlayerMove");*/

                            GameManager.i.turnScript.UseAction("PlayerMove");
                            //Nemesis, if at same node, can interact and damage player
                            GameManager.i.nemesisScript.CheckNemesisAtPlayerNode(true);
                        }
                    }
                }
                else
                {
                    //connection security ignored

                    /*EventManager.i.PostNotification(EventType.UseAction, this, "Player Move", "NodeManager.cs -> ProcessPlayerMove");*/

                    GameManager.i.turnScript.UseAction("PlayerMove");
                    //Nemesis, if at same node, can interact and damage player
                    GameManager.i.nemesisScript.CheckNemesisAtPlayerNode(true);
                }
                //Tracker Data
                HistoryRebelMove history = new HistoryRebelMove();
                history.turn = GameManager.i.turnScript.Turn;
                history.playerNodeID = moveDetails.nodeID;
                history.invisibility = GameManager.i.playerScript.Invisibility;
                history.nemesisNodeID = GameManager.i.nodeScript.nodeNemesis;
                history.nodeName = node.nodeName;
                GameManager.i.dataScript.AddHistoryRebelMove(history);
            }
            else
            { Debug.LogError(string.Format("Invalid node (Null) for nodeID {0}", moveDetails.nodeID)); }
        }
        else
        { Debug.LogError("Invalid ModalMoveDetails (Null)"); }
    }
    #endregion

    #region ProcessMoveOutcome
    /// <summary>
    /// ProcessPlayerMove -> ProcessMoveOutcome. Node checked for Null in calling procedure. Checks for presence of Erasure team and Nemesis in destination node
    /// </summary>
    private void ProcessMoveOutcome(MoveReturnData data)
    {
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        outcomeDetails.isSpecial = true;
        //Erasure team picks up player immediately if invisibility 0
        CaptureDetails captureDetails = GameManager.i.captureScript.CheckCaptured(data.node.nodeID, GameManager.i.playerScript.actorID);
        if (captureDetails != null)
        {
            //Player captured!
            captureDetails.effects = string.Format("{0}The move went bad, you were spotted on Arrival{1}", colourNeutral, colourEnd);
            EventManager.i.PostNotification(EventType.Capture, this, captureDetails, "NodeManager.cs -> ProcessMoveOutcome");
        }
        else
        {
            //Normal Move  Outcome
            if (data.isChangeInvisibility == true)
            {
                //Detected
                outcomeDetails.textTop = string.Format("You have been {0}DETECTED{1}", colourBad, colourEnd);
                outcomeDetails.textBottom = data.text;
                outcomeDetails.sprite = GameManager.i.spriteScript.alarmSprite;
                outcomeDetails.isAction = true;
                outcomeDetails.side = globalResistance;
                outcomeDetails.reason = "Player Move";
                outcomeDetails.isSpecialGood = false;
            }
            else
            {
                //Not detected
                outcomeDetails.textTop = string.Format("You Move {0}without being Detected{1}", colourGood, colourEnd);
                outcomeDetails.textBottom = data.text;
                outcomeDetails.sprite = GameManager.i.spriteScript.undetectedSprite;
                outcomeDetails.isAction = true;
                outcomeDetails.side = globalResistance;
                outcomeDetails.reason = "Player Move";
                outcomeDetails.isSpecialGood = true;
            }
            EventManager.i.PostNotification(EventType.OutcomeOpen, this, outcomeDetails, "NodeManager.cs -> ProcessMoveOutcome");
            //Nemesis, if at same node, can interact and damage player
            GameManager.i.nemesisScript.CheckNemesisAtPlayerNode(true);
        }
    }
    #endregion

    #endregion

    #region RemoveOngoingEffect
    /// <summary>
    /// loops all nodes and removes any ongoing effects that match the specified ID
    /// </summary>
    /// <param name="ongoingID"></param>
    public void RemoveOngoingEffect(int ongoingID)
    {
        if (ongoingID > -1)
        {
            List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
            if (listOfNodes != null)
            {
                foreach (Node node in listOfNodes)
                { node.RemoveOngoingEffect(ongoingID); }
            }
            else { Debug.LogError("Invalid listOfNodes (Null)"); }
        }
        else { Debug.LogError(string.Format("Invalid ongoingID {0} (must be zero or above)", ongoingID)); }
    }
    #endregion

    #region ProcessNodeTimers
    /// <summary>
    /// Decrement all ongoing Effect timers in nodes and delete any that have expired
    /// </summary>
    private void ProcessNodeTimers()
    {
        //Debug.LogWarning(string.Format("PROCESSNODETIMER: turn {0}{1}", GameManager.instance.turnScript.Turn, "\n"));
        List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
        if (listOfNodes != null)
        {
            foreach (Node node in listOfNodes)
            {
                node.ProcessOngoingEffectTimers();
                node.ProcessObserverTimers();
            }
        }
    }
    #endregion

    #region ProcessOngoingEffects
    /// <summary>
    /// Generates messages for 'Effect' tab in InfoApp
    /// </summary>
    private void ProcessOngoingEffects()
    {
        string text;
        //Ongoing node effects
        Dictionary<int, EffectDataOngoing> dictOfOngoingEffects = GameManager.i.dataScript.GetDictOfOngoingEffects();
        if (dictOfOngoingEffects != null)
        {
            int count = dictOfOngoingEffects.Count;
            if (count > 0)
            {
                foreach (var ongoing in dictOfOngoingEffects)
                {
                    //message
                    if (ongoing.Value.nodeID > -1)
                    { GameManager.i.messageScript.MessageOngoingEffectCurrentNode(ongoing.Value); }
                    else { Debug.LogWarningFormat("Invalid ongoingEffect for {0}, ID {1}", ongoing.Value.text, ongoing.Key); }
                }
            }
        }
        else { Debug.LogWarning("Invalid dictOfOngoingEffects (Null)"); }
        //Nodes with a valid Crisis wait period
        Dictionary<int, Node> tempDict = GameManager.i.dataScript.GetDictOfNodes();
        if (tempDict != null)
        {
            foreach (var node in tempDict)
            {
                if (node.Value.waitTimer > 0)
                {
                    //Info App ongoing effect message
                    text = string.Format("{0}, {1}, id {2} district cannot have a crisis for another {3} turn{4}", node.Value.nodeName, node.Value.Arc.name, node.Value.nodeID, node.Value.waitTimer,
                        node.Value.waitTimer != 1 ? "s" : "");
                    GameManager.i.messageScript.NodeOngoingEffect(text, node.Value);
                }
            }
        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
    }
    #endregion

    #region DebugRandomActivityValues
    /// <summary>
    /// Debug method that randomly assigns activity data to nodes and connection for development purposes
    /// </summary>
    private void DebugRandomActivityValues()
    {
        int baseChance = 20;
        int counter = 0;
        //Nodes
        List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
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
        List<Connection> listOfConnections = GameManager.i.dataScript.GetListOfConnections();
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
    #endregion

    #region GetAIDelayForMove
    /// <summary>
    /// returns AI delay, in turns, for being notified of rebel player moving through a connection where they lost invisibility doing so.
    /// </summary>
    /// <param name="secLvl"></param>
    /// <returns></returns>
    public int GetAIDelayForMove(ConnectionType secLvl)
    {
        int securityLevel;
        switch (secLvl)
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
    #endregion

    #region Flashing Nodes...
    //
    // - - - Flashing Nodes
    //

    #region FlashHighlightNodes
    /// <summary>
    /// Master method to flash highlighted nodes
    /// </summary>
    /// <param name="nodeUI"></param>
    public void FlashHighlightNodes(NodeUI nodeUI)
    {
        List<Node> listOfNodes = ShowNodes(nodeUI);
        if (listOfNodes.Count > 0)
        { myFlashCoroutine = StartCoroutine("FlashingNodes", listOfNodes); }
    }

    /// <summary>
    /// For showing a single node (FinderUI.cs)
    /// </summary>
    /// <param name="nodeID"></param>
    public void FlashNode(int nodeID)
    {
        List<Node> listOfNodes = new List<Node>();
        Node node = GameManager.i.dataScript.GetNode(nodeID);
        if (node != null)
        {
            listOfNodes.Add(node);
            ResetNodes();
            NodeShowFlag = 1;
            GameManager.i.alertScript.SetAlertUI(node.nodeName);
            myFlashCoroutine = StartCoroutine("FlashingNodes", listOfNodes);
        }
    }
    #endregion

    #region StartFlashingNodes
    /// <summary>
    /// NODES plural -> event driven -> start coroutine
    /// </summary>
    /// <param name="nodeID"></param>
    private void StartFlashingNodes(List<Node> listOfNodes)
    {
        if (listOfNodes != null)
        {
            if (listOfNodes.Count > 0)
            {
                isFlashOn = false;
                myFlashCoroutine = StartCoroutine("FlashingNodes", listOfNodes);
            }
            else { Debug.LogWarning("Invalid listOfNodes (Empty)"); }
        }
        else { Debug.LogWarning("Invalid listOfNodes (Null)"); }
    }
    #endregion

    #region StopFlashingNodes
    /// <summary>
    /// NODES plural -> event driven -> stop coroutine
    /// </summary>
    private void StopFlashingNodes()
    {
        if (myFlashCoroutine != null)
        {
            StopCoroutine(myFlashCoroutine);
            myFlashCoroutine = null;
        }
        //alert UI -> turn off if active
        if (GameManager.i.alertScript.CheckAlertUI() == true)
        { GameManager.i.alertScript.CloseAlertUI(); }
        //reset nodes
        ResetNodes();
    }
    #endregion

    #region FlashingNodes
    /// <summary>
    /// coroutine to flash NODES plural
    /// NOTE: listOfNodes checked for null and Empty by calling procedure
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    IEnumerator FlashingNodes(List<Node> listOfNodes)
    {
        int count = listOfNodes.Count;
        //forever loop
        for (; ; )
        {
            if (isFlashOn == false)
            {
                for (int i = 0; i < count; i++)
                { listOfNodes[i].SetActive(); }
                NodeRedraw = true;
                isFlashOn = true;
                yield return new WaitForSecondsRealtime(flashNodeTime);
            }
            else
            {
                for (int i = 0; i < count; i++)
                { listOfNodes[i].SetNormal(); }
                NodeRedraw = true;
                isFlashOn = false;
                yield return new WaitForSecondsRealtime(flashNodeTime);
            }
        }
    }
    #endregion

    #region CheckFlashingNodes
    /// <summary>
    /// returns true if flashing nodes are active, false otherwise
    /// </summary>
    /// <returns></returns>
    public bool CheckFlashingNodes()
    { return myFlashCoroutine == null ? false : true; }
    #endregion

    #endregion

    #region Node Crisis...
    //
    // - - - Node Crisis
    //

    #region ProcessNodeCrisis
    /// <summary>
    /// Checks all nodes, checks for datapoints in dangerzone, updates crisis timers and runs crisis checks
    /// </summary>
    private void ProcessNodeCrisis()
    {
        List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
        List<Node> listOfCrisisNodes = GameManager.i.dataScript.GetListOfCrisisNodes();
        List<Node> tempList = new List<Node>(); //add all current crisis nodes in list then, once done, overwrite listOfCrisisNodes
        List<NodeDatapoint> listOfDatapoints = new List<NodeDatapoint>();
        if (listOfNodes != null)
        {
            if (listOfCrisisNodes != null)
            {
                //city
                City city = GameManager.i.cityScript.GetCity();
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
                                    GameManager.i.messageScript.NodeCrisis(msgText, itemText, node);
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
                                        string reason = string.Format("<b>{0}{1}{2} crisis will shortly go CRITICAL</b>", colourAlert, node.crisis.tag, colourEnd);
                                        string warning = string.Format("{0} turn{1} left for Authority to resolve Crisis", node.crisisTimer, node.crisisTimer != 1 ? "s" : "");
                                        //good for Resistance, bad for Authority
                                        bool isBad = false;
                                        if (GameManager.i.sideScript.PlayerSide.level == globalAuthority.level) { isBad = true; }
                                        GameManager.i.messageScript.GeneralWarning(msgText, itemText, "District Crisis", reason, warning, true, isBad);
                                        //news snippet
                                        if (GameManager.i.turnScript.CheckIsAutoRun() == false)
                                        {
                                            if (node.crisis.textList != null)
                                            {
                                                CheckTextData data = new CheckTextData();
                                                data.node = node;
                                                data.text = node.crisis.textList.GetIndexedRecord();
                                                string newsSnippet = GameManager.i.newsScript.CheckNewsText(data);
                                                NewsItem item = new NewsItem() { text = newsSnippet, timer = 1 };
                                                GameManager.i.dataScript.AddNewsItem(item);
                                            }
                                            else { Debug.LogWarningFormat("Invalid textList for crisis \"{0}\" at node {1}, id {2}", node.crisis.tag, node.nodeName, node.nodeID); }
                                        }
                                    }
                                    else
                                    {
                                        //Crisis COMPLETED (goes Critical) -> lower city support
                                        int loyalty = GameManager.i.cityScript.CityLoyalty;
                                        loyalty -= crisisCityLoyalty;
                                        loyalty = Mathf.Max(0, loyalty);
                                        GameManager.i.cityScript.CityLoyalty = loyalty;
                                        //statistics
                                        GameManager.i.dataScript.StatisticIncrement(StatType.NodeCrisisExplodes);
                                        //admin                                
                                        msgText = string.Format("{0}, {1}, crisis ({2}) has EXPLODED", node.nodeName, node.Arc.name, node.crisis.tag);
                                        itemText = string.Format("{0}, {1}, crisis has EXPLODED", node.nodeName, node.Arc.name);
                                        GameManager.i.messageScript.NodeCrisis(msgText, itemText, node, crisisCityLoyalty);
                                        msgText = string.Format("{0} Loyalty falls by -{1} to {2} ({3} crisis)", city.tag, crisisCityLoyalty, loyalty, node.crisis.tag);
                                        string reasonText = string.Format("{0} district crisis", node.crisis.tag);
                                        GameManager.i.messageScript.CityLoyalty(msgText, reasonText, loyalty, crisisCityLoyalty * -1);
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
                                if (node.Security <= crisisSecurity) { numOfDangerSigns++; listOfDatapoints.Add(GameManager.i.globalScript.nodeSecurity); }
                                if (node.Stability <= crisisStability) { numOfDangerSigns++; listOfDatapoints.Add(GameManager.i.globalScript.nodeStability); }
                                if (node.Support >= crisisSupport) { numOfDangerSigns++; listOfDatapoints.Add(GameManager.i.globalScript.nodeSupport); }
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
                                        GameManager.i.aiScript.numOfCrisis++;
                                        GameManager.i.dataScript.StatisticIncrement(StatType.NodeCrisis);
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
                                            datapoint = GameManager.i.globalScript.nodeStability;
                                        }
                                        node.crisis = GameManager.i.dataScript.GetRandomNodeCrisis(datapoint);
                                        if (node.crisis == null)
                                        { Debug.LogErrorFormat("Invalid nodeCrisis default random crisis (Null) for datapoint {0}", datapoint); }
                                        //admin
                                        Debug.LogFormat("[Rnd] NodeManager.cs -> ProcessNodeCrisis: {0} ID {1}, CRISIS need < {2}, rolled {3}", node.Arc.name, node.nodeID,
                                            chance, rnd);
                                        msgText = string.Format("{0}, {1}, ID {2} crisis COMMENCES ({3})", node.nodeName, node.Arc.name, node.nodeID, node.crisis.tag);
                                        itemText = string.Format("{0}, {1}, district crisis COMMENCES", node.nodeName, node.Arc.name);
                                        GameManager.i.messageScript.NodeCrisis(msgText, itemText, node);
                                        //random
                                        msgText = string.Format("{0}, {1}, district CRISIS succeeds", node.nodeName, node.Arc.name);
                                        GameManager.i.messageScript.GeneralRandom(msgText, "District Crisis", chance, rnd, true);
                                        //news snippet
                                        if (GameManager.i.turnScript.CheckIsAutoRun() == false)
                                        {
                                            if (node.crisis.textList != null)
                                            {
                                                CheckTextData data = new CheckTextData();
                                                data.node = node;
                                                data.text = node.crisis.textList.GetIndexedRecord();
                                                string newsSnippet = GameManager.i.newsScript.CheckNewsText(data);
                                                NewsItem item = new NewsItem() { text = newsSnippet, timer = 1 };
                                                GameManager.i.dataScript.AddNewsItem(item);
                                            }
                                            else { Debug.LogWarningFormat("Invalid textList for crisis \"{0}\" at node {1}, id {2}", node.crisis.tag, node.nodeName, node.nodeID); }
                                        }
                                    }
                                    else
                                    {
                                        //failed roll, nothing happens
                                        node.launcher.StopSmoke();
                                        /*Debug.LogFormat("[Tst] NodeManager.cs -> ProcessNodeCrisis: {0} ID {1}, Failed need < {2}, rolled {3}", node.Arc.name, node.nodeID,
                                            chance, rnd);*/
                                        //random
                                        msgText = string.Format("{0}, {1}, district CRISIS fails", node.nodeName, node.Arc.name);
                                        GameManager.i.messageScript.GeneralRandom(msgText, "District Crisis", chance, rnd, true);
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
    #endregion

    #region ProcessLoadNodeCrisis
    /// <summary>
    /// start up smoke emitters on crisis nodes when Loading saved game data
    /// </summary>
    public void ProcessLoadNodeCrisis()
    {
        List<Node> listOfCrisisNodes = GameManager.i.dataScript.GetListOfCrisisNodes();
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
    #endregion

    #endregion

    #region CureNodes...
    //
    // - - - Cure Nodes
    //

    #region AddCureNode
    /// <summary>
    /// Add a cure to a node location and handles all admin. isActive sets initial state of Cure (default false). Overriden to true if Resistance is controlled by AI.
    /// </summary>
    /// <param name="cure"></param>
    /// <returns></returns>
    public void AddCureNode(Cure cure, bool isActive = false)
    {
        if (cure != null)
        {
            int nodeID = GetCureNodeRandom(cure);
            if (nodeID > -1)
            {
                //set
                Node node = GameManager.i.dataScript.GetNode(nodeID);
                {
                    if (node != null)
                    {
                        node.cure = cure;
                        cure.isActive = isActive;
                        //automatically make active if Resistance is AI
                        if (isActive == false && GameManager.i.sideScript.resistanceOverall == SideState.AI)
                        { cure.isActive = true; }
                        GameManager.i.dataScript.AddCureNode(node);
                        Debug.LogFormat("[Nod] NodeManager.cs -> AddCureNode: {0} cure ADDED at {1}, {2}, ID {3}{4}", cure.cureName, node.nodeName, node.Arc.name, node.nodeID, "\n");
                    }
                    else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", nodeID); }
                }
            }
            else { Debug.LogErrorFormat("Invalid Cure nodeID (-1) for \"{0}\", distance {1}", cure.cureName, cure.distance); }
        }
        else { Debug.LogError("Invalid cure (Null)"); }
    }
    #endregion

    #region RemoveCureNode
    /// <summary>
    /// Removes a specified cure from it's node location and handles all admin
    /// </summary>
    /// <param name="cure"></param>
    public void RemoveCureNode(Cure cure)
    {
        if (cure != null)
        {
            Node node = GameManager.i.dataScript.GetCureNode(cure);
            if (node != null)
            {
                if (GameManager.i.dataScript.RemoveCureNode(node) == true)
                {
                    cure.isActive = false;
                    node.cure = null;
                    Debug.LogFormat("[Nod] NodeManager.cs -> RemoveCureNode: {0} cure REMOVED at {1}, {2}, ID {3}{4}", cure.cureName, node.nodeName, node.Arc.name, node.nodeID, "\n");
                }
                else { Debug.LogErrorFormat("{0}, {1}, ID {2}, NOT REMOVED from listOfCureNodes", node.nodeName, node.Arc.name, node.nodeID); }
            }
            else { Debug.LogErrorFormat("Invalid node (Null) for \"{0}\" cure", cure.cureName); }
        }
        else { Debug.LogError("Invalid cure (Null)"); }
    }
    #endregion

    #region GetCureNodeRandom
    /// <summary>
    /// finds a random node, 'x' distance links away from the Resistance Player's current location (may end up being less), one that doesn't already have a cure. Returns -1 if a problem
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    private int GetCureNodeRandom(Cure cure)
    {
        int cureNodeID = -1;
        int requiredDistance = cure.distance;
        Debug.Assert(requiredDistance > 0, "Invalid cure.requiredDistance (must be > 0)");
        if (cure != null)
        {
            //get exclusion list of nodes currently with a cure
            List<int> listOfExclusion = new List<int>();
            List<Node> listOfCureNodes = GameManager.i.dataScript.GetListOfCureNodes();
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
            //get cure node
            Node node = GameManager.i.dataScript.GetNode(GetPlayerNodeID());
            Node nodeCure = null;
            if (listOfExclusion != null && listOfExclusion.Count > 0)
            { nodeCure = GameManager.i.dijkstraScript.GetRandomNodeAtDistance(node, requiredDistance, listOfExclusion); }
            else { nodeCure = GameManager.i.dijkstraScript.GetRandomNodeAtDistance(node, requiredDistance); }
            if (nodeCure != null)
            { cureNodeID = nodeCure.nodeID; }
        }
        else { Debug.LogError("Invalid cure (Null)"); }
        //return
        return cureNodeID;
    }
    #endregion

    #endregion

    #region ConfigureTutorialHideItems
    /// <summary>
    /// places spiders and tracers onMap a start of a tutorial set
    /// </summary>
    public void ConfigureTutorialHideItems(TutorialHideConfig config)
    {
        if (config != null)
        {
            int index;
            Node node;
            List<Node> listOfAllNodes = GameManager.i.dataScript.GetListOfAllNodes();
            List<Node> listOfConnected = GameManager.i.dataScript.GetListOfMostConnectedNodes();
            List<Node> listOfTempNodes;
            if (listOfAllNodes != null)
            {
                if (listOfConnected != null)
                {
                    //
                    // - - - Spiders
                    //
                    if (config.numOfSpiders > 0)
                    {
                        listOfTempNodes = null;
                        //placement
                        if (config.isRandomSpiderNodes == true)
                        { listOfTempNodes = new List<Node>(listOfAllNodes); }
                        else
                        { listOfTempNodes = new List<Node>(listOfConnected); }
                        if (listOfTempNodes != null)
                        {
                            for (int i = 0; i < config.numOfSpiders; i++)
                            {
                                index = Random.Range(0, listOfTempNodes.Count);
                                node = listOfTempNodes[index];
                                if (node != null)
                                {
                                    node.AddSpider();
                                    //remove from list to prevent dupes
                                    listOfTempNodes.RemoveAt(index);
                                    //check list isn't empty -> abort remaining spider placement
                                    if (listOfTempNodes.Count == 0)
                                    {
                                        Debug.LogWarningFormat("No more nodes in listOfTempNodes at loop {0}. Spider placement aborted", i);
                                        break;
                                    }
                                }
                                else { Debug.LogWarningFormat("Invalid node (Null) for loop {0}", i); }
                            }
                        }
                    }
                    //
                    // - - - Tracers
                    //
                    if (config.numOfTracers > 0)
                    {
                        listOfTempNodes = null;
                        //placement
                        if (config.isRandomTracerNodes == true)
                        { listOfTempNodes = new List<Node>(listOfAllNodes); }
                        else
                        { listOfTempNodes = new List<Node>(listOfConnected); }
                        if (listOfTempNodes != null)
                        {
                            for (int i = 0; i < config.numOfTracers; i++)
                            {
                                index = Random.Range(0, listOfTempNodes.Count);
                                node = listOfTempNodes[index];
                                if (node != null)
                                {
                                    node.AddTracer();
                                    //remove from list to prevent dupes
                                    listOfTempNodes.RemoveAt(index);
                                    //check list isn't empty -> abort remaining tracer placement
                                    if (listOfTempNodes.Count == 0)
                                    {
                                        Debug.LogWarningFormat("No more nodes in listOfTempNodes at loop {0}. Tracer placement aborted", i);
                                        break;
                                    }
                                }
                                else { Debug.LogWarningFormat("Invalid node (Null) for loop {0}", i); }
                            }
                        }
                    }
                }
                else { Debug.LogError("Invalid listOfConnected (Null)"); }
            }
            else { Debug.LogError("Invalid listOfAllNodes (Null)"); }
        }
        else { Debug.LogError("Invalid TutorialHideConfig (Null)"); }
    }
    #endregion

    //new methods above here
}