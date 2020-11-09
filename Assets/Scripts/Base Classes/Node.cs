using gameAPI;
using modalAPI;
using packageAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Node : MonoBehaviour
{
    //child objects of node
    public GameObject parentObject;                     //parent object
    public GameObject faceObject;                       //child object that has the textmesh component for writing text on top of the node (linked in Editor)
    public GameObject baseObject;                       //child object -> base of buildings

    //tower objects
    private GameObject rearObject;                       //child object -> rear building
    private GameObject rightObject;                      //child object -> right building
    private GameObject leftObject;                       //child object -> left building

    //neon sign objects
    private GameObject sign0;                            //child object of rear Object (indivdual)
    private GameObject sign1;                            //child object of rear Object (grouped with sign2)
    private GameObject sign2;                            //child object of rear Object (grouped with sign1)

    [HideInInspector] public NodeColour colourNode;     //used to assign a new colour to node cylinder
    [HideInInspector] public NodeColour colourBase;     //used to assign a new colour to district base
    [HideInInspector] public NodeColour colourRear;     //used to assign a new colour to district rear
    [HideInInspector] public NodeColour colourRight;     //used to assign a new colour to district right
    [HideInInspector] public NodeColour colourLeft;     //used to assign a new colour to district left


    [HideInInspector] public int nodeID;                //unique ID, sequentially derived from NodeManager nodeCounter, don't skip numbers, keep it sequential, 0+
    [HideInInspector] public Vector3 nodePosition;      //position
    [HideInInspector] public string nodeName;           //name of node, eg. "Downtown Bronx"
    [HideInInspector] public string specialName;        //eg. name of icon, airport, harbour, town hall, etc. if node is special, ignore otherwise
    [HideInInspector] public NodeArc Arc;               //archetype type
    [HideInInspector] public ParticleLauncher launcher; //attached script component that controls the smoke particle system
    [HideInInspector] public Renderer nodeRenderer;   //renders node cylinder
    [HideInInspector] public Renderer baseRenderer;   //renders base object
    [HideInInspector] public Renderer rearRenderer;   //renders rear object
    [HideInInspector] public Renderer rightRenderer;   //renders right object
    [HideInInspector] public Renderer leftRenderer;   //renders left object

    #region Save Data Compatible
    [HideInInspector] public bool isTracer;             //has resistance tracer?
    [HideInInspector] public bool isSpider;             //has authority spider?
    [HideInInspector] public bool isContactResistance;  //true if any Resistance Actor has a connection at the node (ignores contact status)
    [HideInInspector] public bool isContactAuthority;   //true if any Authority Actor has a connection at the nodes (ignores contact status)
    [HideInInspector] public bool isPreferredAuthority;      //true if node is off the preferred authority faction node arc type
    [HideInInspector] public bool isPreferredResistance;     //true if node is off the preferred resistance faction node arc type 
    [HideInInspector] public bool isCentreNode;              //true if node is in the geographic centre region of the map (used by AI)
    [HideInInspector] public bool isLoiterNode;              //true if a loiter node for Nemesis
    [HideInInspector] public bool isConnectedNode;           //true if node is in the listMostConnectedNodes
    [HideInInspector] public bool isChokepointNode;          //true if node is a chokepoint (one connection between it and another subgraph)

    [HideInInspector] public string targetName;              //unique name, null indicates no target

    [HideInInspector] public int spiderTimer;           //countdown timer before removed
    [HideInInspector] public int tracerTimer;           //countdown timer before removed
    [HideInInspector] public int activityCount = -1;    //# times rebel activity occurred (invis-1)
    [HideInInspector] public int activityTime = -1;     //most recent turn when rebel activity occurred

    [HideInInspector] public bool isStabilityTeam;     //Civil team present at node
    [HideInInspector] public bool isSecurityTeam;      //Control team present at node
    [HideInInspector] public bool isSupportTeam;       //Media team present at node
    [HideInInspector] public bool isProbeTeam;         //Probe team present at node
    [HideInInspector] public bool isSpiderTeam;        //Spider team present at node
    [HideInInspector] public bool isDamageTeam;        //Damage team present at node
    [HideInInspector] public bool isErasureTeam;       //Erasure team present at node

    [HideInInspector] public int crisisTimer;           //counts down a node crisis
    [HideInInspector] public int waitTimer;             //counts down interval between possible crisis
    [HideInInspector] public NodeCrisis crisis = null;             //type of Nodecrisis, eg. "Riot"

    [HideInInspector] public LoiterData loiter;         //pre-configured data at game start to aid nemesis moving to the nearest loiter node
    [HideInInspector] public Cure cure = null;          //cure node (condition). Null if none.

    [HideInInspector] public char defaultChar;          //default node face text

    //save collections
    private List<Team> listOfTeams;                     //Authority teams present at the node
    private List<EffectDataOngoing> listOfOngoingEffects; //list of temporary (ongoing) effects impacting on the node

    //private backing fields
    private int _stability;
    private int _support;                               //support for resistance
    private int _security;
    private int _stabilityStart;                        //values at game start (base line)
    private int _supportStart;
    private int _securityStart;
    private bool _isTracerKnown;                        //true if Authority knows of tracer coverage for this node
    private bool _isSpiderKnown;                        //does Resistance know of spider?
    private bool _isContactKnown;                       //true if Authority knows of Actor contacts
    private bool _isTeamKnown;                          //true if Resistance knows of teams (additional means other than tracer coverage or connections)
    private bool _isTargetKnown;                        //true if Authority knows of Active or Live target (if present)
    #endregion

    private Coroutine myCoroutine;
    private Transform nodeTransform;
    private GameObject parent;
    private bool isSignOn;

    //generated collections
    private List<Vector3> listOfNeighbourPositions;     //list of neighbouring nodes that this node is connected to
    private List<Node> listOfNeighbourNodes;            //list of neighbouring nodes that this node is connected to 
    private List<Node> listOfNearNeighbours;            //list of all nodes within a 2 connection radius (includes immediate neighbours) -> Initialised by AIManager.cs -> Initialise -> SetNearNeighbours
    private List<Connection> listOfConnections;         //list of neighbouring connections

    [HideInInspector] public TextMeshPro faceText;         //textmesh component of faceObject (cached in Awake)



    private bool onMouseFlag;                           //flag indicates that onMouseOver is true (used for tooltip coroutine)
    private float mouseOverDelay;                       //tooltip
    /*private float fadeInTime;                           //tooltip*/

    //fast access fields
    private int maxValue;                               //max and min node datapoint values (derive from NodeManager.cs)
    private int minValue;
    //signage
    private float signDelay = -1;
    private int signMinimum = -1;
    private int signRandom = -1;
    private int signRepeat = -1;

    private int stabilityTeamEffect = 999;
    private int securityTeamEffect = 999;
    private int supportTeamEffect = 999;
    private int crisisCityLoyalty = 999;


    //Properties for backing fields
    public int Security
    {
        get { return Mathf.Clamp(_security + GetOngoingEffect(GameManager.i.nodeScript.outcomeNodeSecurity) + GetTeamEffect(NodeData.Security), minValue, maxValue); }
        set { _security = value; _security = Mathf.Clamp(_security, 0, 3); }
    }

    public int Stability
    {
        get
        { return Mathf.Clamp(_stability + GetOngoingEffect(GameManager.i.nodeScript.outcomeNodeStability) + GetTeamEffect(NodeData.Stability), minValue, maxValue); }
        set { _stability = value; _stability = Mathf.Clamp(_stability, 0, 3); }
    }

    public int Support
    {
        get { return Mathf.Clamp(_support + GetOngoingEffect(GameManager.i.nodeScript.outcomeNodeSupport) + GetTeamEffect(NodeData.Support), minValue, maxValue); }
        set { _support = value; _support = Mathf.Clamp(_support, 0, 3); }
    }

    public bool isTracerKnown
    {
        get
        {
            //any Ongoing effect overides current setting (if no ongoing effect then current setting stands)
            int value = GetOngoingEffect(GameManager.i.nodeScript.outcomeStatusTracers);
            if (value < 0 && isProbeTeam == false) { return false; }
            else if (value > 0 || isProbeTeam == true) { return true; }
            else { return _isTracerKnown; }
        }
        set { _isTracerKnown = value; }
    }

    public bool isSpiderKnown
    {
        get
        {
            //any Ongoing effect overides current setting (if no ongoing effect then current setting stands)
            int value = GetOngoingEffect(GameManager.i.nodeScript.outcomeStatusSpiders);
            if (value < 0) { return false; }
            else if (value > 0) { return true; }
            else { return _isSpiderKnown; }
        }
        set { _isSpiderKnown = value; }
    }

    /// <summary>
    /// is Resistance contacts at node known by Authority?
    /// </summary>
    public bool isContactKnown
    {
        get
        {
            //any Ongoing effect & presence of a Probe team overides current setting (if no ongoing effect then current setting stands)
            int value = GetOngoingEffect(GameManager.i.nodeScript.outcomeStatusContacts);
            if (value < 0 && isProbeTeam == false) { return false; }
            else if (value > 0 || isProbeTeam == true) { return true; }
            else { return _isContactKnown; }
        }
        set { _isContactKnown = value; }
    }

    public bool isTeamKnown
    {
        get
        {
            //any Ongoing effect overides current setting (if no ongoing effect then current setting stands)
            int value = GetOngoingEffect(GameManager.i.nodeScript.outcomeStatusTeams);
            if (value < 0) { return false; }
            else if (value > 0) { return true; }
            else { return _isTeamKnown; }
        }
        set { _isTeamKnown = value; }
    }

    public bool isTargetKnown
    {
        get { return _isTargetKnown; }
        set { _isTargetKnown = value; }
    }

    /// <summary>
    /// Initialise SO's for Nodes
    /// </summary>
    /// <param name="archetype"></param>
    public void Initialise(NodeArc archetype)
    {
        Arc = archetype;
    }

    private void Awake()
    {
        /*Debug.Assert(faceObject != null, "Invalid faceObject (Null)");*/
        nodeTransform = GetComponent<Transform>();
        Debug.Assert(parentObject != null, "Invalid parentObject (Null)");
        Debug.Assert(nodeTransform != null, "Invalid nodeTransform (Null)");
        //collections
        listOfNeighbourPositions = new List<Vector3>();
        listOfNeighbourNodes = new List<Node>();
        listOfNearNeighbours = new List<Node>();
        listOfTeams = new List<Team>();
        listOfConnections = new List<Connection>();
        listOfOngoingEffects = new List<EffectDataOngoing>();

        //vars

        /*mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
        maxValue = GameManager.instance.nodeScript.maxNodeValue;
        minValue = GameManager.instance.nodeScript.minNodeValue;*/

        //particle launcher
        launcher = GetComponent<ParticleLauncher>();
        //face Text
        if (faceObject != null)
        {
            //node face text
            faceText = faceObject.gameObject.GetComponent<TextMeshPro>();
            faceText.text = "";
        }
        else { Debug.LogError("Invalid faceObject (Null)"); }
        Debug.Assert(faceText != null, "Invalid faceText (Null)");

        //district components
        if (GameManager.i.optionScript.noNodes == true)
        { if (baseObject == null) { Debug.LogError("Invalid baseObject (Null)"); }  }
        //renderers
        nodeRenderer = GetComponent<Renderer>();
        baseRenderer = baseObject.GetComponent<Renderer>();
        Debug.Assert(nodeRenderer != null, "Invalid Cylinder renderer (Null)");
        Debug.Assert(baseRenderer != null, "Invalid Base renderer (Null)");
        //launcher
        Debug.Assert(launcher != null, "Invalid Launcher (Null)");


    }

    private void OnEnable()
    {
        //Materials and child objects
        if (GameManager.i.optionScript.noNodes == true)
        {
            //Districts in use, not cylindrical nodes
            Debug.Assert(baseObject != null, "Invalid baseObject (Null)");
            colourNode = NodeColour.Invisible;
            colourBase = NodeColour.TowerDark;
        }
        else
        {
            //Cylindrical Nodes in use, not districts
            colourNode = NodeColour.Normal;
        }

        //fast access
        mouseOverDelay = GameManager.i.guiScript.tooltipDelay;
        //signage
        signDelay = GameManager.i.guiScript.signageDelay;
        signMinimum = GameManager.i.guiScript.signageMinimum;
        signRandom = GameManager.i.guiScript.signageRandom;
        signRepeat = GameManager.i.guiScript.signageRepeat;
        Debug.Assert(signDelay > -1, "Invalid signDelay (-1)");
        /*fadeInTime = GameManager.instance.tooltipScript.tooltipFade;*/
        maxValue = GameManager.i.nodeScript.maxNodeValue;
        minValue = GameManager.i.nodeScript.minNodeValue;
    }

    private void Start()
    {
        //base line values
        _stabilityStart = _stability;
        _supportStart = _support;
        _securityStart = _security;
        //fast access
        stabilityTeamEffect = GameManager.i.teamScript.civilNodeEffect;
        securityTeamEffect = GameManager.i.teamScript.controlNodeEffect;
        supportTeamEffect = GameManager.i.teamScript.mediaNodeEffect * -1;
        crisisCityLoyalty = GameManager.i.nodeScript.crisisCityLoyalty;
        Debug.Assert(stabilityTeamEffect != 999, "Invalid stabilityTeamEffect (999)");
        Debug.Assert(securityTeamEffect != 999, "Invalid securityTeamEffect (999)");
        Debug.Assert(supportTeamEffect != 999, "Invalid supportTeamEffect (999)");
        Debug.Assert(crisisCityLoyalty != 999, "Invalid crisisCityLoyalty (999)");
    }


    /// <summary>
    /// Left Mouse click
    /// </summary>
    private void OnMouseDown()
    {
        //only do so if new turn processing hasn't commenced
        if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
        {
            bool proceedFlag = true;
            AlertType alertType = AlertType.None;
            if (GameManager.i.guiScript.CheckIsBlocked() == false)
            {
                //exit any tooltip
                if (onMouseFlag == true)
                {
                    StopMyCoroutine();
                    GameManager.i.tooltipNodeScript.CloseTooltip("Node.cs -> OnMouseDown");
                }
                //Action Menu -> not valid if AI is active for side
                if (GameManager.i.sideScript.CheckInteraction() == false)
                { proceedFlag = false; alertType = AlertType.SideStatus; }
                //Action Menu -> not valid if  Player inactive
                else if (GameManager.i.playerScript.status != ActorStatus.Active)
                { proceedFlag = false; alertType = AlertType.PlayerStatus; }
                //Proceed
                if (proceedFlag == true)
                {
                    //highlight current node
                    GameManager.i.nodeScript.ToggleNodeHighlight(nodeID);
                    //Action menu data package
                    ModalGenericMenuDetails details = new ModalGenericMenuDetails()
                    {
                        itemID = nodeID,
                        itemName = nodeName,
                        itemDetails = string.Format("{0} ID {1}", Arc.name, nodeID),
                        menuPos = transform.position,
                        listOfButtonDetails = GameManager.i.actorScript.GetNodeActions(nodeID),
                        menuType = ActionMenuType.Node
                    };
                    //activate menu
                    GameManager.i.actionMenuScript.SetActionMenu(details);
                }
                else
                {
                    //explanatory message
                    if (alertType != AlertType.None)
                    { GameManager.i.guiScript.SetAlertMessageModalOne(alertType); }
                }
            }
        }
    }

    /// <summary>
    /// mouse over exit, shut down tooltip
    /// </summary>
    private void OnMouseExit()
    {
        if (GameManager.i.guiScript.CheckIsBlocked() == false)
        {
            StopMyCoroutine();
            GameManager.i.tooltipNodeScript.CloseTooltip("Node.cs -> OnMouseExit");
        }
    }

    /// <summary>
    /// Mouse Over tool tip and Right Click
    /// </summary>
    private void OnMouseOver()
    {
        bool proceedFlag = true;
        AlertType alertType = AlertType.None;
        //check modal block isn't in place
        if (GameManager.i.guiScript.CheckIsBlocked() == false)
        {
            //
            // - - - Right click node -> Show either move options (node highlights) or Move Menu
            //
            if (Input.GetMouseButtonDown(1) == true)
            {
                //only do so if new turn processing hasn't commenced
                if (GameManager.i.turnScript.CheckNewTurnBlocked() == false)
                {
                    //exit any tooltip
                    if (onMouseFlag == true)
                    {
                        StopMyCoroutine();
                        GameManager.i.tooltipNodeScript.CloseTooltip("Node.cs -> OnMouseOver");
                    }
                    //move action invalid if resistance player is captured, etc.
                    if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideResistance.level)
                    {
                        //Action Menu -> not valid if AI is active for side
                        if (GameManager.i.sideScript.CheckInteraction() == false)
                        { proceedFlag = false; alertType = AlertType.SideStatus; }
                        //Action Menu -> not valid if  Player inactive
                        else if (GameManager.i.playerScript.status != ActorStatus.Active)
                        { proceedFlag = false; alertType = AlertType.PlayerStatus; }
                        //proceed
                        if (proceedFlag == true)
                        {
                            /*//exit any tooltip
                            GameManager.i.tooltipNodeScript.CloseTooltip("Node.cs -> OnMouseOver");*/

                            //Create a Move Menu at the node
                            if (GameManager.i.dataScript.CheckValidMoveNode(nodeID) == true)
                            { EventManager.i.PostNotification(EventType.CreateMoveMenu, this, nodeID, "Node.cs -> OnMouseOver"); }
                            //highlight all possible move options
                            else
                            {
                                EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.Move, "Node.cs -> OnMouseOver");
                                //if at Player's current node then Gear Node menu
                                if (nodeID == GameManager.i.nodeScript.GetPlayerNodeID())
                                { EventManager.i.PostNotification(EventType.CreateGearNodeMenu, this, nodeID, "Node.cs -> OnMouseOver"); }
                            }
                        }
                        else
                        {
                            //explanatory message
                            if (alertType != AlertType.None)
                            { GameManager.i.guiScript.SetAlertMessageModalOne(alertType); }
                        }
                    }
                }
            }
            // - - - Tool tip
            else
            {
                onMouseFlag = true;
                myCoroutine = StartCoroutine("ShowTooltip");
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
        if (onMouseFlag == true && GameManager.i.inputScript.ModalState == ModalState.Normal)
        {
            //check modal block isn't in place
            if (GameManager.i.guiScript.CheckIsBlocked() == false)
            {
                //do once
                while (GameManager.i.tooltipNodeScript.CheckTooltipActive() == false)
                {
                    List<string> contactListCurrent = new List<string>();
                    List<string> contactListOther = new List<string>();
                    //
                    // - - - CONTACTS vary depending on whether viewing player side or debug viewing other side
                    //
                    switch (GameManager.i.turnScript.currentSide.level)
                    {
                        case 1:
                            //authority
                            contactListCurrent = GameManager.i.dataScript.GetActiveContactsAtNodeAuthority(nodeID);
                            contactListOther = GameManager.i.dataScript.GetActiveContactsAtNodeResistance(nodeID);
                            break;
                        case 2:
                            //resistance
                            contactListCurrent = GameManager.i.dataScript.GetActiveContactsAtNodeResistance(nodeID);
                            contactListOther = GameManager.i.dataScript.GetActiveContactsAtNodeAuthority(nodeID);
                            break;
                    }
                    List<EffectDataTooltip> effectsList = GetListOfOngoingEffectTooltips();
                    List<string> teamList = new List<string>();
                    if (listOfTeams.Count > 0)
                    {
                        foreach (Team team in listOfTeams)
                        { teamList.Add(team.arc.name); }
                    }
                    //
                    // - - - TARGET info (TargetManager method handles FOW, isTargetKnown and sides logic)
                    //
                    List<string> targetList = new List<string>();
                    if (targetName != null)
                    { targetList = GameManager.i.targetScript.GetTargetTooltip(targetName, isTargetKnown); }
                    //crisis info
                    List<string> crisisList = new List<string>();
                    if (crisis != null)
                    {
                        crisisList.Add(string.Format("{0} CRISIS", crisis.tag));
                        crisisList.Add(string.Format("{0} turn{1} left", crisisTimer, crisisTimer != 1 ? "s" : ""));
                        crisisList.Add(string.Format("City Loyalty -{0}", crisisCityLoyalty));
                        crisisList.Add("if crisis not Resolved");
                    }
                    //
                    // - - - ACTIVITY info
                    //
                    List<string> activityList = null;
                    if (GameManager.i.nodeScript.activityState != ActivityUI.None)
                    { activityList = GetActivityInfo(); }
                    //
                    // - - - DEBUG Data
                    //
                    string textType, textName;
                    if (GameManager.i.optionScript.debugData == true)
                    {
                        textType = string.Format("{0}<font=\"LiberationSans SDF\"> ID {1}</font>", Arc.name, nodeID);
                        /*textName = string.Format("PrfA {0} Conn {1} Chk {2}", Convert.ToInt32(isPreferredAuthority), Convert.ToInt32(isConnectedNode),
                            Convert.ToInt32(isChokepointNode));*/
                        /*if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
                        { textName = string.Format("isCon <b>{0}</b> isK <b>{1}</b>", isContactResistance, isContactKnown); }
                        else { textName = string.Format("isCon <b>{0}</b> isK <b>{1}</b>", isContactAuthority, isContactKnown); }*/

                        /*textName = string.Format("L id {0}, dist {1}, N id {2}", loiter.nodeID, loiter.distance, loiter.neighbourID);*/

                        textName = string.Format("pos x: {0}, y: {1}, z: {2}", nodePosition.x, nodePosition.y, nodePosition.z);
                    }
                    else
                    {
                        textType = string.Format("{0}", Arc.name);
                        textName = nodeName;
                    }
                    //
                    // - - - Spider
                    //
                    bool showSpider = false;
                    if (isSpider == true)
                    {
                        if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideResistance.level
                            || GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideBoth.level)
                        {
                            if (GameManager.i.optionScript.fogOfWar == true)
                            {
                                if (isSpiderKnown == true) { showSpider = true; }
                            }
                            else { showSpider = true; }
                        }
                        else { showSpider = true; }
                    }
                    //
                    // - - - Cure
                    //
                    string specialText;
                    //combined with special Text
                    if (cure != null && cure.isActive == true)
                    {
                        if (string.IsNullOrEmpty(specialName) == false)
                        { specialText = string.Format("{0}{1}{2}", specialName, "\n", cure.cureName); }
                        else { specialText = cure.cureName; }
                    }
                    else { specialText = specialName; }
                    //
                    // - - Data package
                    //
                    NodeTooltipData dataTooltip = new NodeTooltipData()
                    {
                        nodeName = textName,
                        specialName = specialText,
                        type = textType,
                        isTargetKnown = isTargetKnown,
                        isTracer = isTracer,
                        /*isTracerActive = isTracerActive,*/
                        isTracerKnown = isTracerKnown,
                        isContactKnown = isContactKnown,
                        isTeamKnown = isTeamKnown,
                        isSpiderKnown = showSpider,
                        spiderTimer = spiderTimer,
                        tracerTimer = tracerTimer,
                        arrayOfStats = GetStats(),
                        listOfCrisis = crisisList,
                        listOfContactsCurrent = contactListCurrent,
                        listOfContactsOther = contactListOther,
                        listOfEffects = effectsList,
                        listOfTeams = teamList,
                        listOfTargets = targetList,
                        listOfActivity = activityList,
                        tooltipPos = transform.position
                    };
                    //isContact side dependant
                    GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
                    if (playerSide.level == GameManager.i.globalScript.sideResistance.level)
                    {
                        //broad check first for contacts then narrow check for active contact with an active parent actor
                        dataTooltip.isActiveContact = isContactResistance;
                        if (isContactResistance == true)
                        { dataTooltip.isActiveContact = GameManager.i.dataScript.CheckActiveContactAtNode(nodeID, playerSide); }
                    }
                    //if Authority contacts present then automatically active
                    else { dataTooltip.isActiveContact = isContactAuthority; }
                    GameManager.i.tooltipNodeScript.SetTooltip(dataTooltip);
                    yield return null;
                }
                /*//fade in -> only if normal gamestate (eg. not modal)
                float alphaCurrent;
                while (GameManager.instance.tooltipNodeScript.GetOpacity() < 1.0 && GameManager.instance.inputScript.GameState == GameState.Normal)
                {
                    alphaCurrent = GameManager.instance.tooltipNodeScript.GetOpacity();
                    alphaCurrent += Time.deltaTime / fadeInTime;
                    GameManager.instance.tooltipNodeScript.SetOpacity(alphaCurrent);
                    yield return null;
                }*/
            }
        }
    }

    /// <summary>
    /// Controlled shut down of Coroutine
    /// </summary>
    private void StopMyCoroutine()
    {
        if (myCoroutine != null)
        {
            StopCoroutine(myCoroutine);
            myCoroutine = null;
            onMouseFlag = false;
        }
    }

    //
    // - - - Neighbours
    //

    public List<Vector3> GetListOfNeighbourPositions()
    { return listOfNeighbourPositions; }

    public int GetNumOfNeighbourPositions()
    { return listOfNeighbourPositions.Count; }

    public int GetNumOfNeighbouringNodes()
    { return listOfNeighbourNodes.Count; }

    public int GetNumOfNearNeighbouringNodes()
    { return listOfNearNeighbours.Count; }

    /// <summary>
    /// Used for updating list in LevelManager.cs -> InitialiseDistricts
    /// </summary>
    /// <param name="listOfConns"></param>
    public void SetListOfConnections(List<Connection> listOfConns)
    {
        if (listOfConns != null)
        {
            listOfConnections.Clear();
            listOfConnections.AddRange(listOfConnections);
        }
        else { Debug.LogError("Invalid listOfConns (Null)"); }
    }

    public void ClearListOfNeighbourPositions()
    { listOfNeighbourPositions.Clear(); }

    public void ClearListOfNeighbouringNodes()
    { listOfNeighbourNodes.Clear(); }

    public void ClearListOfNearNeighbours()
    { listOfNearNeighbours.Clear(); }

    /// <summary>
    /// add neighbouring vector3 to list (can't null test a vector3)
    /// </summary>
    /// <param name="pos"></param>
    public void AddNeighbourPosition(Vector3 pos)
    { listOfNeighbourPositions.Add(pos); }


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

    /// <summary>
    /// Returns a random neighbouring node, null if a problem
    /// </summary>
    /// <returns></returns>
    public Node GetRandomNeighbour()
    { return listOfNeighbourNodes[Random.Range(0, listOfNeighbourNodes.Count)]; }

    /// <summary>
    /// returns true if supplied nodeID corresponds with a node in the listOfNeighbourNodes
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public bool CheckNeighbourNodeID(int nodeID)
    { return listOfNeighbourNodes.Exists(x => x.nodeID == nodeID); }

    /// <summary>
    /// Get list of all nodes within a 2 connection radius
    /// </summary>
    /// <returns></returns>
    public List<Node> GetNearNeighbours()
    { return listOfNearNeighbours; }


    /// <summary>
    /// pass a list of near neighbouring nodes
    /// </summary>
    /// <param name="listOfNodes"></param>
    public void SetNearNeighbours(List<Node> listOfNodes)
    {
        if (listOfNodes != null)
        {
            listOfNearNeighbours.Clear();
            listOfNearNeighbours.AddRange(listOfNodes);
        }
        else { Debug.LogError("Invalid listOfNodes (Null)"); }
    }

    //
    // --- Other ---
    //

    /// <summary>
    /// Everytime player moves to a new node you have to call this to update master list of NodeID's that contain all valid move locations for the player's next move
    /// </summary>
    public void SetPlayerMoveNodes()
    {
        List<Node> listOfNodes = new List<Node>();

        //special move gear, eg. SewerMap?
        if (GameManager.i.playerScript.isSpecialMoveGear == true)
        {
            //need to add all nodes 2 distance away
            foreach (Node node in listOfNearNeighbours)
            {
                //exclude current node (player node)
                if (node.nodeID != nodeID)
                { listOfNodes.Add(node); }
            }
        }
        else
        {
            //no special move gear, immediate neighbours only
            foreach (Node node in listOfNeighbourNodes)
            { listOfNodes.Add(node); }
        }
        if (listOfNodes.Count > 0)
        { GameManager.i.dataScript.UpdateMoveNodes(listOfNodes); }
        else { Debug.LogError("listOfNeighbourNodes has no records, listOfNodeID has no records -> MoveNodes not updated"); }
    }

    public int GetNumOfConnections()
    { return listOfConnections.Count; }

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
        if (GameManager.i.dataScript.GetNode(nodeID) != null)
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
    /// returns a list of ongoing effects currently impacting the node, returns empty list if none
    /// </summary>
    /// <returns></returns>
    public List<EffectDataTooltip> GetListOfOngoingEffectTooltips()
    {
        List<EffectDataTooltip> tempList = new List<EffectDataTooltip>();
        if (listOfOngoingEffects.Count > 0)
        {
            foreach (var ongoingEffect in listOfOngoingEffects)
            {
                EffectDataTooltip data = new EffectDataTooltip();
                data.text = string.Format("{0}{1}<size=90%><b>{2}</b> turn{3} remaining</size>", ongoingEffect.nodeTooltip, "\n", ongoingEffect.timer, ongoingEffect.timer != 1 ? "s" : "");
                data.typeLevel = ongoingEffect.typeLevel;
                tempList.Add(data);
            }
        }
        return tempList;
    }


    public List<EffectDataOngoing> GetListOfOngoingEffects()
    { return listOfOngoingEffects; }

    //
    // - - - Node Display
    //

    /// <summary>
    /// WARNING! DO NOT CALL this directly, instead use NodeManager.cs -> SetNodeMaterial
    /// Allows selective changing of node/district components (Cylinder assumed to be a normal node cylinder, Base and Towers being part of a district)
    /// </summary>
    /// <param name="newMaterial"></param>
    public void SetMaterial(Material newMaterial, NodeComponent component)
    {
        if (newMaterial != null)
        {
            switch (component)
            {
                case NodeComponent.Cylinder: nodeRenderer.material = newMaterial; break;
                case NodeComponent.Base: baseRenderer.material = newMaterial; break;
                case NodeComponent.Towers:
                    rearRenderer.material = newMaterial;
                    rightRenderer.material = newMaterial;
                    leftRenderer.material = newMaterial;
                    break;
                case NodeComponent.TowerRear: rearRenderer.material = newMaterial; break;
                case NodeComponent.TowerLeftRight:
                    rightRenderer.material = newMaterial;
                    leftRenderer.material = newMaterial;
                    break;
                case NodeComponent.BaseAndTowers:
                    baseRenderer.material = newMaterial;
                    rearRenderer.material = newMaterial;
                    rightRenderer.material = newMaterial;
                    leftRenderer.material = newMaterial;
                    break;
                default: Debug.LogWarningFormat("Unrecognised NodeComponent \"{0}\"", component); break;
            }
        }
        else { Debug.LogError("Invalid newMaterial (Null)"); }
    }

    /// <summary>
    /// returns material of relevant node component, null if a problem. DO NOT use returned material for anything other than Comparisons (using renderer.sharedMaterial)
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public Material GetMaterial(NodeComponent component)
    {
        Material material = null;
        switch (component)
        {
            case NodeComponent.Cylinder: material = nodeRenderer.sharedMaterial; break;
            case NodeComponent.Base: material = baseRenderer.sharedMaterial; break;
            case NodeComponent.Towers: material = rearRenderer.sharedMaterial; break;
            case NodeComponent.BaseAndTowers: material = baseRenderer.sharedMaterial; break;
            default: Debug.LogWarningFormat("Unrecognised NodeComponent \"{0}\"", component); break;
        }
        return material;
    }

    /// <summary>
    /// Sets node to active material with black, full opacity, face icon
    /// </summary>
    /// <param name="newMaterial"></param>
    public void SetActive()
    {
        if (GameManager.i.optionScript.noNodes == false)
        {
            faceText.color = new Color32(0, 0, 0, 255);
            colourNode = NodeColour.Active;
        }
        else
        {
            colourBase = NodeColour.Active;
            colourRear = NodeColour.TowerActive;
            
        }
    }

    /// <summary>
    /// Sets node to Player material with black, full opacity, face text
    /// </summary>
    public void SetPlayerFlash()
    {
        if (GameManager.i.optionScript.noNodes == false)
        {
            faceText.color = new Color32(0, 0, 0, 255);
            colourNode = NodeColour.Player;

        }
        else
        {
            colourBase = NodeColour.Player;
            colourRear = NodeColour.Active;
        }
    }

    /// <summary>
    /// Sets node to Player material with default, three quarter opacity, face icon
    /// </summary>
    /// <param name="newMaterial"></param>
    public void SetPlayerNormal()
    {
        if (GameManager.i.optionScript.noNodes == false)
        {
            faceText.color = new Color32(255, 255, 224, 202);
            colourNode = NodeColour.Player;
        }
        else
        {
            colourBase = NodeColour.Player;
            colourRear = NodeColour.TowerDark;
            colourRight = NodeColour.TowerLight;
            colourLeft = NodeColour.TowerLight;
        }
    }

    /// <summary>
    /// Sets node to Highlight material and black, full opacity, face icon
    /// </summary>
    public void SetHighlight()
    {
        if (GameManager.i.optionScript.noNodes == false)
        {
            faceText.color = new Color32(0, 0, 0, 255);
            colourNode = NodeColour.Highlight;
        }
        else
        { colourBase = NodeColour.Highlight; }
    }

    /// <summary>
    /// Sets node to Nemesis material and default icon
    /// </summary>
    public void SetNemesis()
    {
        if (GameManager.i.optionScript.noNodes == false)
        {
            faceText.color = new Color32(255, 255, 224, 202);
            colourNode = NodeColour.Nemesis;
        }
        else
        {
            colourBase = NodeColour.Nemesis;
            colourRear = NodeColour.TowerDark;
        }
    }

    /// <summary>
    /// Sets node to normal material with default, three quarter opacity, face icon
    /// </summary>
    /// <param name="newMaterial"></param>
    public void SetNormal()
    {
        if (GameManager.i.optionScript.noNodes == false)
        {
            faceText.color = new Color32(255, 255, 224, 202);
            colourNode = NodeColour.Normal;
        }
        else
        {
            colourBase = NodeColour.Normal;
            colourRear = NodeColour.TowerDark;
        }
    }

    //
    // - - - Hide and Seek
    //

    /// <summary>
    /// Add Tracer
    /// </summary>
    public void AddTracer()
    {
        isTracer = true;
        //reveals any spiders
        if (isSpider == true)
        { isSpiderKnown = true; }
        //set timer
        tracerTimer = GameManager.i.nodeScript.observerTimer;
        //teams
        if (listOfTeams.Count > 0)
        { isTeamKnown = true; }
        Debug.LogFormat("[Nod] Node.cs -> AddTracer: Tracer added at {0}, {1}, nodeID {2}{3}", nodeName, Arc.name, nodeID, "\n");
    }

    /// <summary>
    /// remove Tracer from node and tidy up bool fields for tracer coverage
    /// </summary>
    public void RemoveTracer()
    {
        if (isTracer == true)
        {
            isTracer = false;
            isTracerKnown = false;
            isTeamKnown = false;
            tracerTimer = 0;
            /*sSpiderKnown = false;*/
            Debug.LogFormat("[Nod] Node.cs -> RemoveTracer: Tracer removed at {0}, {1}, nodeID {2}{3}", nodeName, Arc.name, nodeID, "\n");

            /*//check neighbours
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
            { isTracerActive = false; }*/

        }
    }

    /// <summary>
    /// Add spider to node and handle all admin
    /// </summary>
    public void AddSpider()
    {
        if (isSpider == false)
        {
            //add spider
            isSpider = true;
            spiderTimer = GameManager.i.nodeScript.observerTimer;
            //check if same node as a Tracer
            if (isTracer == true)
            { isSpiderKnown = true; }
            else { isSpiderKnown = false; }
            Debug.LogFormat("[Nod] Node.cs -> AddSpider: Spider added at {0}, {1}, nodeID {2}{3}", nodeName, Arc.name, nodeID, "\n");
        }
        else
        {
            //spider already present -> reset timer
            spiderTimer = GameManager.i.nodeScript.observerTimer;
            Debug.LogFormat("[Nod] Node.cs -> AddSpider: Spider already present at {0}, {1}, nodeID {2}. Timer extended{3}", nodeName, Arc.name, nodeID, "\n");
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
            spiderTimer = 0;
            Debug.LogFormat("[Nod] Node.cs -> RemoveSpider: Spider removed at {0}, {1}, nodeID {2}{3}", nodeName, Arc.name, nodeID, "\n");
        }
    }

    //
    // - - - Teams - - -
    //

    /// <summary>
    /// add an authority team to the node. Returns true if placement successful.
    /// Max one instance of each type of team at node
    /// Max cap on number of teams at node.
    /// </summary>
    /// <param name="team"></param>
    public bool AddTeam(Team team, int actorSlotID)
    {
        if (team != null)
        {
            //check there is room for another team
            if (listOfTeams.Count < GameManager.i.teamScript.maxTeamsAtNode)
            {
                //check a similar type of team not already present
                int nodeArcID = team.arc.TeamArcID;
                if (listOfTeams.Count > 0)
                {
                    foreach (Team teamExisting in listOfTeams)
                    {
                        if (teamExisting.arc.TeamArcID == nodeArcID)
                        {
                            //already a similar team present -> no go

                            /*Debug.LogWarning(string.Format("{0} Team NOT added to node {1}, ID {2} as already a similar team present{3}", 
                                team.arc.name, nodeName, nodeID, "\n"));*/

                            return false;
                        }
                    }
                }
                //Add team
                listOfTeams.Add(team);
                //set appropriate team flags
                switch (team.arc.name)
                {
                    case "CIVIL": isStabilityTeam = true; break;
                    case "CONTROL": isSecurityTeam = true; break;
                    case "MEDIA": isSupportTeam = true; break;
                    case "PROBE": isProbeTeam = true; break;
                    case "SPIDER": isSpiderTeam = true; break;
                    case "DAMAGE": isDamageTeam = true; break;
                    case "ERASURE": isErasureTeam = true; break;
                }
                //initialise Team data
                team.nodeID = nodeID;
                team.actorSlotID = actorSlotID;
                team.pool = TeamPool.OnMap;
                team.timer = GameManager.i.teamScript.deployTime;
                /*Debug.Log(string.Format("{0} Team added to node {1}, ID {2}{3}", team.arc.name, nodeName, nodeID, "\n"));*/
                return true;
            }
            else { Debug.LogWarningFormat("Maximum number of teams already present at Node {0}, ID {1} (Info Only){2}", nodeName, nodeID, "\n"); }
        }
        else { Debug.LogErrorFormat("Invalid team (null) for Node {0}, ID {1}{2}", nodeName, nodeID, "\n"); }
        return false;
    }

    /// <summary>
    /// Load saved file method to drop a team straight into the listOfTeams. 
    /// NOTE: Team checked for Null by the calling method
    /// </summary>
    /// <param name="team"></param>
    public void LoadAddTeam(Team team)
    { listOfTeams.Add(team); }

    /// <summary>
    /// Remove a team with the matching teamID from the listOfTeams and adjust team status. Returns true if successful, false otherwise.
    /// </summary>
    /// <param name="teamID"></param>
    public bool RemoveTeam(int teamID)
    {
        for (int i = 0; i < listOfTeams.Count; i++)
        {
            if (listOfTeams[i].teamID == teamID)
            {
                //set appropriate team flags
                switch (listOfTeams[i].arc.name)
                {
                    case "CIVIL": isStabilityTeam = false; break;
                    case "CONTROL": isSecurityTeam = false; break;
                    case "MEDIA": isSupportTeam = false; break;
                    case "PROBE": isProbeTeam = false; break;
                    case "SPIDER": isSpiderTeam = false; break;
                    case "DAMAGE": isDamageTeam = false; break;
                    case "ERASURE": isErasureTeam = false; break;
                }
                //remove team
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
    /// AIRebelManager.cs specific implementation for Operator action. Will remove an Erasure team if present, otherwise a random team. Handles all Admin. Returns team Arc name of team removed, "Unknown" if none
    /// </summary>
    /// <returns></returns>
    public string RemoveTeamAI()
    {
        string teamArcName = "Unknown";
        int count = listOfTeams.Count;
        int index;
        Team team = null;
        if (count > 0)
        {
            //Erasure team present
            if (isErasureTeam == true)
            {
                //loop list looking for an erasure team
                for (int i = 0; i < count; i++)
                {
                    if (listOfTeams[i].arc.name.Equals("ERASURE", StringComparison.Ordinal) == true)
                    {
                        //remove Erasure team
                        team = listOfTeams[i];
                        if (GameManager.i.teamScript.MoveTeamAI(TeamPool.InTransit, team.teamID, this) == true)
                        {
                            teamArcName = team.arc.name;
                            break;
                        }
                        else
                        {
                            Debug.LogWarning("Erasure team not found even though isErasureTeam is set true");
                            //remove a random team
                            index = Random.Range(0, count);
                            team = listOfTeams[index];
                            if (GameManager.i.teamScript.MoveTeamAI(TeamPool.InTransit, team.teamID, this) == true)
                            {
                                teamArcName = team.arc.name;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                //remove a random team.
                index = Random.Range(0, count);
                team = listOfTeams[index];
                //Move Team handles all admin including removing from team node list
                if (GameManager.i.teamScript.MoveTeamAI(TeamPool.InTransit, team.teamID, this) == true)
                { teamArcName = team.arc.name; }
            }
        }
        else { Debug.LogWarning("There are no teams present at the node"); }
        //set appropriate team flags
        if (team != null)
        {
            switch (team.arc.name)
            {
                case "CIVIL": isStabilityTeam = false; break;
                case "CONTROL": isSecurityTeam = false; break;
                case "MEDIA": isSupportTeam = false; break;
                case "PROBE": isProbeTeam = false; break;
                case "SPIDER": isSpiderTeam = false; break;
                case "DAMAGE": isDamageTeam = false; break;
                case "ERASURE": isErasureTeam = false; break;
            }
        }
        return teamArcName;
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
                if (team.arc.TeamArcID == teamArcID)
                { return team.teamID; }
            }
        }
        return -1;
    }

    /// <summary>
    /// returns empty list if none
    /// </summary>
    /// <returns></returns>
    public List<Team> GetListOfTeams()
    { return listOfTeams; }

    /// <summary>
    /// returns list of TeamID's only from listOfTeams
    /// </summary>
    /// <returns></returns>
    public List<int> GetListOfTeamID()
    {
        List<int> listOfID = listOfTeams.Select(id => id.teamID).ToList();
        return listOfID;
    }

    /// <summary>
    /// returns the effect of the relevant team (Civil/Control/Media) for the node Datapoints (Stability/Security/Support)
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private int GetTeamEffect(NodeData type)
    {
        int teamEffect = 0;
        switch (type)
        {
            case NodeData.Stability: if (isStabilityTeam == true) { teamEffect = stabilityTeamEffect; } break;
            case NodeData.Security: if (isSecurityTeam == true) { teamEffect = securityTeamEffect; } break;
            case NodeData.Support: if (isSupportTeam == true) { teamEffect = supportTeamEffect; } break;
        }
        return teamEffect;
    }


    /// <summary>
    /// returns list of TeamID's only from listOfTeams
    /// </summary>
    /// <returns></returns>
    public List<int> GetListOfOngoingID()
    {
        List<int> listOfID = listOfOngoingEffects.Select(id => id.ongoingID).ToList();
        return listOfID;
    }

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
            effect.description = ongoing.description;
            effect.reason = ongoing.reason;
            effect.nodeTooltip = ongoing.nodeTooltip;
            effect.value = ongoing.value;
            effect.timer = ongoing.timer;
            effect.effectOutcome = ongoing.effectOutcome;
            effect.typeLevel = ongoing.typeLevel;
            effect.effectApply = ongoing.effectApply;
            effect.sideLevel = ongoing.sideLevel;
            effect.nodeID = ongoing.nodeID;
            //add new ongoing effect
            listOfOngoingEffects.Add(effect);
            //add to register & create message
            GameManager.i.dataScript.AddOngoingEffectToDict(effect, nodeID);
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
                    GameManager.i.dataScript.RemoveOngoingEffectFromDict(ongoing);
                    listOfOngoingEffects.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    /// Load saved file to game specific method to add an ongoing effect straight to list 
    /// NOTE: ongoing effect is checked for Null by the calling method
    /// </summary>
    /// <param name="ongoing"></param>
    public void LoadAddOngoingEffect(EffectDataOngoing ongoing)
    { listOfOngoingEffects.Add(ongoing); }

    /// <summary>
    /// Returns tally of ongoing effects for the specified node datapoint, '0' if none
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
                if (adjust.effectOutcome.Equals(outcome.name, StringComparison.Ordinal) == true)
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
            for (int i = listOfOngoingEffects.Count - 1; i >= 0; i--)
            {
                //decrement timer
                EffectDataOngoing ongoing = listOfOngoingEffects[i];
                Debug.LogFormat("[Nod] Node.cs -> ProcessOngoingEffect: Node ID {0}, \"{1}\", TIMER before {2}{3}", nodeID, ongoing.description, ongoing.timer, "\n");
                ongoing.timer--;
                if (ongoing.timer <= 0)
                {
                    //node and any connections
                    GameManager.i.connScript.RemoveOngoingEffect(ongoing.ongoingID);
                    RemoveOngoingEffect(ongoing.ongoingID);
                    //message
                    Debug.LogFormat("[Nod] Node.cs -> ProcessOngoingEffect: REMOVE Ongoing effect ID {0}, \"{1}\" from node ID {2}{3}", ongoing.ongoingID, ongoing.description, nodeID, "\n");

                    /*//delete effect -> EDIT: RemoveOngoingEffect handles this
                    GameManager.instance.dataScript.RemoveOngoingEffectFromDict(ongoing);*/
                }
            }
        }
    }

    /// <summary>
    /// decrements spider and tracer (observers, if present) timers and auto removes them at zero
    /// </summary>
    public void ProcessObserverTimers()
    {
        //spider
        if (spiderTimer > 0)
        {
            if (isSpider == true)
            {
                spiderTimer--;
                if (spiderTimer == 0)
                { RemoveSpider(); }
            }
            else { spiderTimer = 0; }
        }
        //tracer
        if (tracerTimer > 0)
        {
            if (isTracer == true)
            {
                tracerTimer--;
                if (tracerTimer == 0)
                { RemoveTracer(); }
            }
            else { tracerTimer = 0; }
        }
    }


    /// <summary>
    /// changes fields and handles ongoing effects. Main method of changing node fields. Adds Value so for subtract you need to provide a negative number
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
                        Debug.LogFormat("[Nod] -> ProcessNodeEffect: {0} {1}, ID {2}, Security now {3} (changed by {4}{5}){6}", nodeName, Arc.name, nodeID, Security,
                            process.value > 0 ? "+" : "", process.value, "\n");
                        break;
                    case "NodeStability":
                        Stability += process.value;
                        Debug.LogFormat("[Nod] -> ProcessNodeEffect: {0} {1}, ID {2}, Stability now {3} (changed by {4}{5}){6}", nodeName, Arc.name, nodeID, Stability,
                            process.value > 0 ? "+" : "", process.value, "\n");
                        break;
                    case "NodeSupport":
                        Support += process.value;
                        Debug.LogFormat("[Nod] -> ProcessNodeEffect: {0} {1}, ID {2}, Support now {3} (changed by {4}{5}){6}", nodeName, Arc.name, nodeID, Support,
                            process.value > 0 ? "+" : "", process.value, "\n");
                        break;
                    case "StatusTracers":
                        if (process.value <= 0)
                        {
                            isTracerKnown = false;
                            Debug.LogFormat("[Nod] -> ProcessNodeEffect: {0} {1}, ID {2}, TRACER removed{3}", nodeName, Arc.name, nodeID, "\n");
                        }
                        else
                        {
                            //reveal tracer only if tracer present
                            if (isTracer == true)
                            {
                                isTracerKnown = true;
                                Debug.LogFormat("[Nod] -> ProcessNodeEffect: {0} {1}, ID {2}, TRACER inserted{3}", nodeName, Arc.name, nodeID, "\n");
                            }
                        }
                        break;
                    case "StatusSpiders":
                        if (process.value <= 0)
                        {
                            isSpiderKnown = false;
                            Debug.LogFormat("[Nod] -> ProcessNodeEffect: {0} {1}, ID {2}, SPIDER removed{3}", nodeName, Arc.name, nodeID, "\n");
                        }
                        else
                        {
                            //reveal spider only if a spider is present
                            if (isSpider == true)
                            {
                                isSpiderKnown = true;
                                Debug.LogFormat("[Nod] -> ProcessNodeEffect: {0} {1}, ID {2}, SPIDER inserted{3}", nodeName, Arc.name, nodeID, "\n");
                            }
                        }
                        break;
                    case "StatusTeams":
                        if (process.value <= 0)
                        {
                            isTeamKnown = false;
                            Debug.LogFormat("[Nod] -> ProcessNodeEffect: {0} {1}, ID {2}, TEAM info NO longer available{3}", nodeName, Arc.name, nodeID, "\n");
                        }
                        else
                        {
                            //reveal teams only if a team is present
                            if (listOfTeams.Count > 0)
                            {
                                isTeamKnown = true;
                                Debug.LogFormat("[Nod] -> ProcessNodeEffect: {0} {1}, ID {2}, TEAM info is available{3}", nodeName, Arc.name, nodeID, "\n");
                            }
                        }
                        break;
                    case "StatusContacts":
                        if (process.value <= 0)
                        {
                            isContactKnown = false;
                            Debug.LogFormat("[Nod] -> ProcessNodeEffect: {0} {1}, ID {2}, CONTACT info no longer available{3}", nodeName, Arc.name, nodeID, "\n");
                        }
                        else
                        {
                            //reveal contacts only if a contact is present. NOTE: currently RESISTANCE contacts only
                            if (isContactResistance == true)
                            {
                                isContactKnown = true;
                                Debug.LogFormat("[Nod] -> ProcessNodeEffect: {0} {1}, ID {2}, CONTACT info is available{3}", nodeName, Arc.name, nodeID, "\n");
                            }
                        }
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
                        connection.SetMaterial(connection.SecurityLevel);
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


    public List<Connection> GetListOfConnections()
    { return listOfConnections; }

    public int CheckNumOfConnections()
    { return listOfConnections.Count; }

    /// <summary>
    /// //stats are the same for both sides, colours change though in the tooltips
    /// </summary>
    /// <returns></returns>
    private int[] GetStats()
    {
        int[] arrayOfStats;
        switch (GameManager.i.sideScript.PlayerSide.name)
        {
            case "Resistance":
            case "Authority":
                arrayOfStats = new int[] { Stability, Support, Security };
                break;
            /*case "Authority":
                arrayOfStats = new int[] { 3 - Stability, 3 - Support, 3 - Security };
                break;*/
            default:
                arrayOfStats = new int[] { Stability, Support, Security };
                Debug.LogError(string.Format("Invalid side \"{0}\"", GameManager.i.sideScript.PlayerSide.name));
                break;
        }

        return arrayOfStats;
    }

    /// <summary>
    /// returns true if there are ongoing target effects, false otherwise
    /// </summary>
    /// <returns></returns>
    public bool CheckForOngoingEffects()
    {
        if (listOfOngoingEffects.Count > 0) { return true; }
        return false;
    }

    //
    // - - - AI - - -
    //

    /// <summary>
    /// add a new set of activity data (time and count)
    /// </summary>
    /// <param name="turn"></param>
    public void AddActivityData(int turn)
    {
        activityCount++;
        //default value -1 so first data point needs to be '1's
        if (activityCount == 0) { activityCount++; }
        //update for the most recent activity (highest turn #)
        if (activityTime < turn)
        { activityTime = turn; }
    }

    /// <summary>
    /// returns activity level (count or time elapsed). Returns -1 if none or a problem
    /// </summary>
    /// <param name="activityUI"></param>
    /// <returns></returns>
    public int GetNodeActivity(ActivityUI activityUI)
    {
        int activityLevel = -1;
        switch (activityUI)
        {
            case ActivityUI.Count:
                activityLevel = activityCount;
                break;
            case ActivityUI.Time:
                if (activityTime > -1)
                { activityLevel = GameManager.i.turnScript.Turn - activityTime; }
                break;
            default:
                Debug.LogWarning(string.Format("Invalid activityUI \"{0}\"", activityUI));
                break;
        }

        return activityLevel;
    }

    /// <summary>
    /// returns activity info for node tooltip in cases where NodeManager.cs -> activityState > 'None'
    /// </summary>
    /// <returns></returns>
    private List<string> GetActivityInfo()
    {
        List<string> listOfActivity = new List<string>();
        //debug activity data
        if (GameManager.i.optionScript.debugData == true)
        {
            listOfActivity.Add(string.Format("activityTime     {0}{1}", activityTime > 0 ? "T" : "", activityTime));
            listOfActivity.Add(string.Format("activityCount    {0}{1}", activityCount > 0 ? "+" : "", activityCount));
        }
        else
        {
            //Activity info details
            switch (GameManager.i.nodeScript.activityState)
            {
                case ActivityUI.Time:
                    int limit = GameManager.i.aiScript.activityTimeLimit;
                    int turnCurrent = GameManager.i.turnScript.Turn;
                    int elapsedTime = -1;

                    if (activityTime > -1)
                    { elapsedTime = turnCurrent - activityTime; }
                    if (elapsedTime > -1)
                    {
                        listOfActivity.Add(string.Format("Last known activity{0}<font=\"Roboto-Bold SDF\">{1} turn{2} ago</font>{3}(ignored after {4} turns)", "\n",
                            elapsedTime, elapsedTime != 1 ? "s" : "", "\n", limit));
                    }
                    else
                    { listOfActivity.Add(string.Format("There has been{0}<font=\"Roboto-Bold SDF\">No Known Activity</font>", "\n")); }
                    break;

                case ActivityUI.Count:
                    if (activityCount > 0)
                    {
                        listOfActivity.Add(string.Format("There {0} been{1}<font=\"Roboto-Bold SDF\">{2} Known</font>{3}incident{4} (in total)",
                          activityCount != 1 ? "have" : "has", "\n", activityCount, "\n", activityCount != 1 ? "s" : ""));
                    }
                    else { listOfActivity.Add(string.Format("There have been{0}<font=\"Roboto-Bold SDF\">No Known</font>{1}incidents", "\n", "\n")); }
                    break;
            }
        }
        return listOfActivity;
    }

    /// <summary>
    /// returns difference (+/-) between current value of a node datapoint and it's original, starting value. '0' for no change, or for default if a problem.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public int GetNodeChange(NodeData type)
    {
        int difference = 0;
        switch (type)
        {
            case NodeData.Support:
                difference = Support - _supportStart;
                break;
            case NodeData.Stability:
                difference = Stability - _stabilityStart;
                break;
            case NodeData.Security:
                difference = Security - _securityStart;
                break;
            default:
                Debug.LogError(string.Format("Invalid NodeData type \"{0}\"", type));
                break;
        }
        return difference;
    }

    /// <summary>
    /// Reconfigure tower objects to be of the required Arc type
    /// </summary>
    public void SetArcType()
    {
        GameObject rear = null;
        GameObject right = null;
        GameObject left = null;

        //activate new arc specific tower objects
        switch (Arc.name)
        {
            case "CORPORATE":
                //towers
                rear = parentObject.transform.Find("Corporate/RearCorp").gameObject;
                right = parentObject.transform.Find("Corporate/RightCorp").gameObject;
                left = parentObject.transform.Find("Corporate/LeftCorp").gameObject;
                //signage
                sign0 = parentObject.transform.Find("Corporate/RearCorp/sign0Corp").gameObject;
                sign1 = parentObject.transform.Find("Corporate/RearCorp/sign1Corp").gameObject;
                sign2 = parentObject.transform.Find("Corporate/RearCorp/sign2Corp").gameObject;
                break;
            case "GOVERNMENT":
                //towers
                rear = parentObject.transform.Find("Government/RearGovt").gameObject;
                right = parentObject.transform.Find("Government/RightGovt").gameObject;
                left = parentObject.transform.Find("Government/LeftGovt").gameObject;
                //signage
                sign0 = parentObject.transform.Find("Government/RearGovt/sign0Govt").gameObject;
                sign1 = parentObject.transform.Find("Government/RearGovt/sign1Govt").gameObject;
                sign2 = parentObject.transform.Find("Government/RearGovt/sign2Govt").gameObject;
                break;
            case "UTILITY":
                //towers
                rear = parentObject.transform.Find("Utility/RearUtil").gameObject;
                right = parentObject.transform.Find("Utility/RightUtil").gameObject;
                left = parentObject.transform.Find("Utility/LeftUtil").gameObject;
                //signage
                sign0 = parentObject.transform.Find("Utility/RearUtil/sign0Util").gameObject;
                sign1 = parentObject.transform.Find("Utility/RearUtil/sign1Util").gameObject;
                sign2 = parentObject.transform.Find("Utility/RearUtil/sign2Util").gameObject;
                break;
            case "INDUSTRIAL":
                //towers
                rear = parentObject.transform.Find("Industrial/RearInd").gameObject;
                right = parentObject.transform.Find("Industrial/RightInd").gameObject;
                left = parentObject.transform.Find("Industrial/LeftInd").gameObject;
                //signage
                sign0 = parentObject.transform.Find("Industrial/RearInd/sign0Ind").gameObject;
                sign1 = parentObject.transform.Find("Industrial/RearInd/sign1Ind").gameObject;
                sign2 = parentObject.transform.Find("Industrial/RearInd/sign2Ind").gameObject;
                break;
            case "RESEARCH":
                //towers
                rear = parentObject.transform.Find("Research/RearRes").gameObject;
                right = parentObject.transform.Find("Research/RightRes").gameObject;
                left = parentObject.transform.Find("Research/LeftRes").gameObject;
                //signage
                sign0 = parentObject.transform.Find("Research/RearRes/sign0Res").gameObject;
                sign1 = parentObject.transform.Find("Research/RearRes/sign1Res").gameObject;
                sign2 = parentObject.transform.Find("Research/RearRes/sign2Res").gameObject;
                break;
            case "GATED":
                //towers
                rear = parentObject.transform.Find("Gated/RearGate").gameObject;
                right = parentObject.transform.Find("Gated/RightGate").gameObject;
                left = parentObject.transform.Find("Gated/LeftGate").gameObject;
                //signage
                sign0 = parentObject.transform.Find("Gated/RearGate/sign0Gate").gameObject;
                sign1 = parentObject.transform.Find("Gated/RearGate/sign1Gate").gameObject;
                sign2 = parentObject.transform.Find("Gated/RearGate/sign2Gate").gameObject;
                break;
            case "SPRAWL":
                //towers
                rear = parentObject.transform.Find("Sprawl/RearSprawl").gameObject;
                right = parentObject.transform.Find("Sprawl/RightSprawl").gameObject;
                left = parentObject.transform.Find("Sprawl/LeftSprawl").gameObject;
                //signage
                sign0 = parentObject.transform.Find("Sprawl/RearSprawl/sign0Sprawl").gameObject;
                sign1 = parentObject.transform.Find("Sprawl/RearSprawl/sign1Sprawl").gameObject;
                sign2 = parentObject.transform.Find("Sprawl/RearSprawl/sign2Sprawl").gameObject;
                break;
            default: Debug.LogWarningFormat("Unrecognised Arc \"{0}\"", Arc.name); break;
        }
        //apply new components
        if (rear != null)
        {
            rearObject = rear;
            rearObject.SetActive(true);
        }
        if (right != null)
        {
            rightObject = right;
            rightObject.SetActive(true);
        }
        if (left != null)
        {
            leftObject = left;
            leftObject.SetActive(true);
        }
        //update renderers 
        rearRenderer = rearObject.GetComponent<Renderer>();
        rightRenderer = rightObject.GetComponent<Renderer>();
        leftRenderer = leftObject.GetComponent<Renderer>();
        Debug.AssertFormat(rearRenderer != null, "Invalid rearRenderer (Null) for Node.Arc \"{0}\"", Arc.name);
        Debug.AssertFormat(rightRenderer != null, "Invalid rightRenderer (Null) for Node.Arc \"{0}\"", Arc.name);
        Debug.AssertFormat(leftRenderer != null, "Invalid leftRenderer (Null) for Node.Arc \"{0}\"", Arc.name);
        //towers
        colourRear = NodeColour.TowerDark;
        colourRight = NodeColour.TowerLight;
        colourLeft = NodeColour.TowerLight;
        //signage
        Debug.Assert(sign0 != null, "Invalid sign0 (Null)");
        Debug.Assert(sign1 != null, "Invalid sign1 (Null)");
        Debug.Assert(sign2 != null, "Invalid sign2 (Null)");
    }

    /// <summary>
    /// Toggles signage (sign0 component only)
    /// </summary>
    public void ToggleSign()
    {
        if (isSignOn == true)
        {
            sign0.SetActive(false);
            sign1.SetActive(false);
            sign2.SetActive(false);
            isSignOn = false;
        }
        else
        {
            sign0.SetActive(true);
            sign1.SetActive(true);
            sign2.SetActive(true);
            isSignOn = true;
        }

    }

    /// <summary>
    /// Flash signage (toggles sign0 element on/off with a pause inbetween for a random number of times)
    /// </summary>
    /// <returns></returns>
    public IEnumerator FlashSignage()
    {
        bool isFlashOn = true;
        int numOfTimes = signMinimum + Random.Range(0, signRandom);
        int counter = 0;
        while (counter < numOfTimes)
        {
            if (isFlashOn == true)
            {
                sign0.SetActive(false);
                isFlashOn = false;
            }
            else
            {
                sign0.SetActive(true);
                isFlashOn = true;
            }
            counter++;
            yield return new WaitForSeconds(signDelay);
        }
        sign0.SetActive(true);
        //chance to repeat sequence except with entire sign
        if (Random.Range(0, 100) < signRepeat)
        {
            //toggle entire sign off/on
            isSignOn = true;
            counter = 0;
            while (counter < numOfTimes)
            {
                ToggleSign();
                counter++;
                yield return new WaitForSeconds(signDelay);
            }
            //leave sign On
            if (isSignOn == false)
            { ToggleSign(); }
        }
    }

    //place methods above here
}
