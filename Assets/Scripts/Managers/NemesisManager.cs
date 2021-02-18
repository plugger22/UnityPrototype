using gameAPI;
using modalAPI;
using packageAPI;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;



/// <summary>
/// Handles all nemesis related matters
/// </summary>
public class NemesisManager : MonoBehaviour
{
    [Header("Loiter Nodes")]
    [Tooltip("How many nodes in listOfMostConnectedNodes to consider as potential loiter nodes.")]
    [Range(1, 10)] public int loiterNodePoolSize = 5;
    [Tooltip("If a possible loiter node is within this distance (Dijkstra unweighted), or less, of another loiter node, it is eliminated")]
    [Range(0, 3)] public int loiterDistanceCheck = 2;

    [Header("LOITER Logic")]
    [Tooltip("Chance of a nemesis switching from an Idle to a Loiter goal (provided not already present at LoiterNode)")]
    [Range(0, 100)] public int chanceIdleToLoiter = 50;
    [Tooltip("Chance of a nemesis switching from an Loiter to an Idle goal (provided not already present at LoiterNode)")]
    [Range(0, 100)] public int chanceLoiterToIdle = 10;
    [Tooltip("Chance of a nemesis switching from an Loiter to an Ambush goal (provided not already present at LoiterNode)")]
    [Range(0, 100)] public int chanceLoiterToAmbush = 20;
    [Tooltip("While at Loiter node, chance nemesis will move to a random neighbouring node (will move back next turn)")]
    [Range(0, 100)] public int chanceLoiterToNeighbour = 50;

    [Header("AMBUSH Logic")]
    [Tooltip("Duration of an Ambush Goal")]
    [Range(1, 5)] public int durationAmbush = 2;

    [Header("SEARCH Logic")]
    [Tooltip("Chance of searching a random neighbouring node")]
    [Range(0, 100)] public int chanceSearchNeighbour = 75;
    [Tooltip("Chance of ceasing MoveToNode and switching to Search mode when ONE or TWO links from target Node")]
    [Range(0, 100)] public int chanceStartEarlySearch = 15;

    [Header("HUNT mode Logic")]
    [Tooltip("When Nemesis switches to Hunt mode it will be for this number of turns plus 1d10 random turns in total")]
    [Range(1, 5)] public int durationHuntMode = 3;

    [Tooltip("Number of turns Nemesis goes Offline after damaging the player (allows the player to clear the datum)")]
    [Range(0, 10)] public int durationDamageOffLine = 3;

    [Header("Spotted by Tracer")]
    [Tooltip("Chance of a nemesis with a HIGH adjusted stealth rating ('3+'), being spotted in any node with Tracer coverage")]
    [Range(0, 100)] public int chanceTracerSpotHigh = 10;
    [Tooltip("Chance of a nemesis with a MED adjusted stealth rating ('2'), being spotted in any node with Tracer coverage")]
    [Range(0, 100)] public int chanceTracerSpotMed = 25;
    [Tooltip("Chance of a nemesis with a LOW adjusted stealth rating ('1'), being spotted in any node with Tracer coverage")]
    [Range(0, 100)] public int chanceTracerSpotLow = 50;
    [Tooltip("Chance of a nemesis with a ZERO adjusted stealth rating ('0'), being spotted in any node with Tracer coverage")]
    [Range(0, 100)] public int chanceTracerSpotZero = 100;

    [Header("Authority Player Control")]
    [Tooltip("How many turns the Player can gain control of the Nemesis for")]
    [Range(1, 10)] public int durationControlPeriod = 5;
    [Tooltip("How mnay turns cooldown does the Player have to wait before they can excercise control again")]
    [Range(1, 10)] public int durationControlCoolDown = 3;
    [Tooltip("How much Power it costs for the player to exercise control (per instance)")]
    [Range(0, 3)] public int controlPowerCost = 1;


    [HideInInspector] public Nemesis nemesis;
    [HideInInspector] public bool isShown;        //Fog of War setting for Nemesis

    //flags
    private bool hasMoved;                  //flag set true if Nemesis has moved during AI phase, reset at start of next AI phase
    private bool hasActed;                  //flag set true if Nemesis has spotted player and caused Damage during the AI phase
    private bool hasWarning;                //flag set true if Nemesis at same node, hasn't spotted player, and a warning issued ("You sense a dark shadow..."). Stops a double warning
    private bool isFirstNemesis;            //flag set true if first Nemesis, false for arrival of second Nemesis

    //Resistance Player
    private SideState resistancePlayer;     //who is resistance player, AI or human?

    //Nemesis AI
    private NemesisMode mode;
    private NemesisGoal goal;
    private int durationGoal;               //if goal is fixed for a set time then no new goal can be assigned until the duration countdown has expired
    private int durationDelay;               //used in Hunt & Inactive modes as a countdown timer, which drops nemesis back into Normal mode once zero
    private Node nemesisNode;               //current node where nemesis is, updated by ProcessNemesisActivity
    private AITracker trackerDebug;         //tracker data passed to NemesisActivity, stored here each turn for Debugging purposes only

    //player tracking info
    private int targetNodeID;               //new targetNodeID (checked each turn) -> sets moveToNodeID
    private int moveToNodeID;               //node that the nemesis is moving towards
    private int targetDistance;             //distance to target, -1 if no target
    private bool isImmediate;               //true if immediate flag set true by

    //Authority player control
    private bool isPlayerControl;
    private int controlNodeID = -1;         //player designated target node
    private int controlTimer;               //player control is for a finite time
    private int controlCooldownTimer;       //player control has a cooldown interim period
    private NemesisGoal controlGoal;        //what the player asks the Nemesis to do once it reaches the specified node (controlledNodeID)

    //fast access
    private GlobalSide globalAuthority;
    private GlobalSide globalResistance;

    //colour palette 
    private string colourNeutral;
    private string colourAlert;
    private string colourBad;
    private string colourEnd;
    /*private string colourGood;
    private string colourNormal;
    private string colourGrey;*/



    /// <summary>
    /// Initialise data ready for Nemesis -> Called from CampaignManager.cs -> InitialiseLate
    /// </summary>
    public void Initialise()
    {
        switch (GameManager.i.inputScript.GameState)
        {
            case GameState.NewInitialisation:
                SubInitialisePlayerData();
                SubInitialiseFastAccess();
                SubInitialiseLevelStart();
                SubInitialiseEvents();
                SubInitialiseAll();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialisePlayerData();
                SubInitialiseLevelStart();
                SubInitialiseAll();
                break;
            case GameState.LoadAtStart:
                SubInitialisePlayerData();
                SubInitialiseFastAccess();
                SubInitialiseLevelStart();
                SubInitialiseEvents();
                SubInitialiseAll();
                break;
            case GameState.LoadGame:
                SubInitialisePlayerData();
                SubInitialiseAll();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
        //Debug (FOW OFF)
        isShown = true;
        isFirstNemesis = true;
        //assign nemesis to a starting node
        int nemesisNodeID = -1;
        //Nemesis always starts at city Centre
        Node node = GameManager.i.dataScript.GetNode(GameManager.i.cityScript.cityHallDistrictID);
        if (node != null)
        {
            nemesisNodeID = node.nodeID;
            nemesisNode = node;
            Debug.LogFormat("[Nem] NemesisManager.cs -> Initialise: Nemesis starts at node {0}, {1}, id {2}{3}", node.nodeName, node.Arc.name, node.nodeID, "\n");
            //assign node
            GameManager.i.nodeScript.nodeNemesis = nemesisNodeID;
        }
        else { Debug.LogError("Invalid node (Null)"); }
        //Nemesis AI -> nemesis does nothing for 'x' turns at game start
        durationDelay = GameManager.i.scenarioScript.scenario.challengeResistance.gracePeriodFirst;
        if (durationDelay > 0)
        {
            //grace period, start inactive
            SetNemesisMode(NemesisMode.Inactive);
        }
        else
        {
            //NO grace period, start in normal mode, waiting for signs of player
            SetNemesisMode(NemesisMode.NORMAL);
        }
    }
    #endregion

    #region SubInitialisePlayerData
    private void SubInitialisePlayerData()
    { resistancePlayer = GameManager.i.sideScript.resistanceOverall; }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //fast access
        globalAuthority = GameManager.i.globalScript.sideAuthority;
        globalResistance = GameManager.i.globalScript.sideResistance;
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event listeners
        EventManager.i.AddListener(EventType.ChangeColour, OnEvent, "NemesisManager");
    }
    #endregion

    #region SubInitialiseAll
    private void SubInitialiseAll()
    {
        //Set up datafor Nemesis
        SetLoiterNodes();
        SetLoiterData();
    }
    #endregion

    #endregion

    /// <summary>
    /// handles events
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// set colour palette for Generic Tool tip
    /// </summary>
    public void SetColours()
    {
        colourNeutral = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourBad = GameManager.i.colourScript.GetColour(ColourType.badText);
        colourAlert = GameManager.i.colourScript.GetColour(ColourType.salmonText);
        colourEnd = GameManager.i.colourScript.GetEndTag();
        /*colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodText);*/
    }


    public void SetResistancePlayer(SideState state)
    { resistancePlayer = state; }

    /// <summary>
    /// called by AIManager.cs -> AISideAuthority, handles all nemesis turn processing methods. 
    /// NOTE: Nemesis checked for null by calling method
    /// </summary>
    public void ProcessNemesis(AITracker tracker, bool immediateFlagResistance)
    {
        ProcessNemesisAdminStart();
        CheckNemesisAtPlayerNode();
        //NOTE: Need to as Nemesis can find player and switch to a null nemesis in CheckNemesisAtPlayerNode
        if (nemesis != null)
        {
            CheckNemesisTracerSighting();

            //Player controlled?
            if (isPlayerControl == true)
            { ProcessNemesisPlayerControlled(tracker, immediateFlagResistance); }
            //AI controlled
            else
            {
                //authority player control
                if (GameManager.i.sideScript.PlayerSide.level == globalAuthority.level)
                {
                    //Authority controlled cooldown period
                    if (controlCooldownTimer > 0)
                    {
                        controlCooldownTimer--;
                        Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesis: Nemesis cooldown period now {0}{1}", controlCooldownTimer, "\n");
                    }
                    else { Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesis: Nemesis AVAILABLE for Player Control{0}", "\n"); }
                }
                //standard AI control
                ProcessNemesisActivity(tracker, immediateFlagResistance);
            }
            //tidy up admin
            ProcessNemesisAdminEnd();
        }
    }

    /// <summary>
    /// Nemesis related matters that need to be reset, or taken care off, prior to nemesis turn processing
    /// </summary>
    private void ProcessNemesisAdminStart()
    {
        hasMoved = false;
        hasActed = false;
    }

    /// <summary>
    /// Nemesis related matters that need to be reset, or taken care off, at end of nemesis turn processing
    /// </summary>
    private void ProcessNemesisAdminEnd()
    {
        string text = "Unknown";
        hasWarning = false;
        //Ongoing effect messages
        if (nemesis != null)
        {
            Node currentNode = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.nodeNemesis);
            if (currentNode != null)
            {
                //resistance player
                text = string.Format("{0} at {1}, {2}, district in {3} mode", nemesis.name, currentNode.nodeName, currentNode.Arc.name, mode);
                GameManager.i.messageScript.NemesisOngoingEffect(text, currentNode.nodeID, nemesis);
            }
            else { Debug.LogError("Invalid node (Null) for Nemesis"); }
            //authority player
            if (globalAuthority.level == GameManager.i.sideScript.PlayerSide.level)
            {
                if (mode != NemesisMode.Inactive)
                {
                    if (isPlayerControl)
                    { text = string.Format("{0} nemesis under PLAYER CONTROL", nemesis.name); }
                    else
                    {
                        if (controlCooldownTimer == 0) { text = string.Format("{0} nemesis AVAILABLE for Player Control", nemesis.name); }
                        else { string.Format("{0} nemesis available for Player Control in {1} turn{2}", nemesis.name, controlCooldownTimer, controlCooldownTimer != 1 ? "s" : ""); }
                    }
                }
                else
                { text = string.Format("{0} nemesis will be ACTIVE in {0} turn{1}", nemesis.name, durationDelay, durationDelay != 1 ? "s" : ""); }
                //destination node for Player control, if applicable (passed Null if not)
                Node controlNode = null;
                if (isPlayerControl == true)
                { controlNode = GameManager.i.dataScript.GetNode(controlNodeID); }
                GameManager.i.messageScript.NemesisPlayerOngoing(text, nemesis, isPlayerControl, controlCooldownTimer, controlTimer, controlNode, currentNode);
            }
        }
    }

    /// <summary>
    /// master AI for nemesis. AIManager.cs -> ProcessErasureTarget provides an update on recent player activity and gives AITracker data (null if nothing to reports)
    /// </summary>
    /// <param name="playerTargetNodeID"></param>
    public void ProcessNemesisActivity(AITracker tracker, bool immediateFlag)
    {
        int playerTargetNodeID = -1;
        int turnDifference = 0;
        int nodeID = GameManager.i.nodeScript.nodeNemesis;
        nemesisNode = GameManager.i.dataScript.GetNode(nodeID);
        isImmediate = immediateFlag;
        trackerDebug = tracker;
        bool isPossibleNewGoal = false;
        //convert tracker data to useable format (No need for null check as it's a 'do nothing' option in this case)
        if (tracker != null)
        {
            playerTargetNodeID = tracker.data0;
            //acts as a DM for hunt duration, the older the information is, the bigger the modifier
            turnDifference = GameManager.i.turnScript.Turn - tracker.turn;
            if (mode != NemesisMode.Inactive)
            { Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisActivity: AITracker Data, turn {0}, nodeID {1}, turnDifference {2}{3}", tracker.turn, tracker.data0, turnDifference, "\n"); }
        }
        //only use playerTargetNodeID if different from previous turns value (the AIManager.cs -> ProcessErasureTarget method kicks out a dirty data stream with lots of repeats)
        if (playerTargetNodeID == targetNodeID || playerTargetNodeID == moveToNodeID)
        {
            if (immediateFlag == false)
            {
                //repeat target, ignore
                if (mode != NemesisMode.Inactive)
                { Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisActivity: REPEAT target (playerTargetNodeID {0}) Immediate FALSE -> IGNORED{1}", playerTargetNodeID, "\n"); }
                targetNodeID = -1;
            }
            else
            {
                //immediate flag true & nemesis already at target node
                if (playerTargetNodeID == nemesisNode.nodeID)
                {
                    //repeat target, ignore
                    if (mode != NemesisMode.Inactive)
                    { Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisActivity: REPEAT target (playerTargetNodeID {0}) Immediate TRUE -> IGNORED{1}", playerTargetNodeID, "\n"); }
                    targetNodeID = -1;
                }
            }
        }
        else { targetNodeID = playerTargetNodeID; }
        //new target
        if (targetNodeID > -1)
        { isPossibleNewGoal = true; }
        //mode counter
        if (durationDelay > 0)
        { durationDelay--; }
        //big picture logic
        if (nemesisNode != null)
        {
            //player carrying out a set goal for a set period of time, keep doing so unless immediate flag set
            if (durationGoal > 0)
            {
                //immediate flag breaks duration cycle
                if (immediateFlag == true) { isPossibleNewGoal = true; }
                else { isPossibleNewGoal = false; }
                //decrement duration
                durationGoal--;
            }
            //do nothing if inactive & duration > 0, swap to normal mode and proceed otherwise
            switch (mode)
            {
                case NemesisMode.Inactive:
                    //message kicks in one turn early
                    if (durationDelay == 1)
                    {
                        isPossibleNewGoal = false;
                        //message - warning, Resistance player only
                        if (GameManager.i.sideScript.PlayerSide.level == globalResistance.level)
                        {
                            string text = string.Format("Reports of a {0} about to come online", nemesis.name);
                            string itemText = "Reports of forthcoming NEMESIS Activity";
                            string topText = "Nemesis Heads Up";
                            string reason = string.Format("{0}<b>Rebel HQ indicate there are signs of your Nemesis stirring</b>", "\n");
                            string warning = "Nemesis activity can be expected shortly";
                            GameManager.i.messageScript.GeneralWarning(text, itemText, topText, reason, warning, false);
                        }
                    }
                    else if (durationDelay == 0)
                    {
                        //message
                        string text = string.Format("{0} Nemesis comes online", nemesis.name);
                        string itemText = "NEMESIS comes Online";
                        string topText = "Nemesis goes ACTIVE";
                        string reason = string.Format("{0}{1}<b>{2} Nemesis</b>{3}", "\n", colourAlert, nemesis.name, colourEnd);
                        string warning = string.Format("{0}", nemesis.descriptor);
                        GameManager.i.messageScript.GeneralWarning(text, itemText, topText, reason, warning, false);
                        //No action, change mode to Normal, goal to Loiter
                        if (targetNodeID < 0)
                        {
                            SetNemesisMode(NemesisMode.NORMAL);
                            isPossibleNewGoal = false;
                        }
                        else
                        { isPossibleNewGoal = true; }
                    }
                    else
                    { isPossibleNewGoal = false; }
                    break;
                case NemesisMode.HUNT:
                    if (durationDelay == 0)
                    {
                        //timer run out and no viable target present
                        if (targetNodeID == -1)
                        {
                            //swap back to normal mode
                            Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisActivity: HUNT mode, TIMER Run out, switch to NORMAL{0}", "\n");
                            SetNemesisMode(NemesisMode.NORMAL);
                            string text = string.Format("{0} changes to {1} mode at {2}, {3} district", nemesis.name, mode, nemesisNode.name, nemesisNode.Arc.name);
                            GameManager.i.messageScript.NemesisNewMode(text, nodeID, nemesis);
                            isPossibleNewGoal = false;
                        }
                        else
                        {
                            //new player target info is available
                            Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisActivity: HUNT mode, TIMER Run out, NEW Target info available{0}", "\n");
                            isPossibleNewGoal = true;
                        }
                    }
                    break;
            }
            //status
            if (mode != NemesisMode.Inactive)
            {
                Debug.LogFormat("[Nem] Status Start: playerTargetNodeID {0}, targetNodeID {1}, Immediate {2}, duration Mode {3} Goal {4}, isNewGoal {5}, ({6} {7}), {8}",
                    playerTargetNodeID, targetNodeID, immediateFlag, durationDelay, durationGoal, isPossibleNewGoal, mode, goal, "\n");
                Debug.LogFormat("[Nem] Status Start: Nemesis at node {0}, {1}, id {2}{3}", nemesisNode.nodeName, nemesisNode.Arc.name, nemesisNode.nodeID, "\n");
            }
            //
            // - - - Proceed - - -
            //
            if (isPossibleNewGoal == true)
            {
                //
                // - - - Possible new goal
                //
                if (targetNodeID > -1)
                {
                    //recent player activity
                    if (mode == NemesisMode.HUNT)
                    {
                        //already in Hunt mode, do we switch to a new target or continue with existing? (algorithm means the older the information the less likely Nemesis is to abandon current target)
                        int threshold = 100 - (turnDifference * 20);
                        int rndNum = Random.Range(0, 100);
                        Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisActivity: Recent ACTIVITY, roll {0} vs {1} (turnDifference {2}){3}", rndNum, threshold, turnDifference, "\n");
                        if (rndNum < threshold == true)
                        {
                            //reset hunt mode and chase new target
                            Debug.LogFormat("[Nem] NemesisManager.cs -> isNewGoal True -> ProcessNemesisActivity: Recent ACTIVITY -> Chase New Target{0}", "\n");
                            SetNemesisMode(NemesisMode.HUNT, turnDifference);
                            ProcessNemesisHunt();
                            /*string text = string.Format("{0} changes to {1} mode at {2}, {3} district", nemesis.name, mode, nemesisNode.name, nemesisNode.Arc.name);
                            GameManager.instance.messageScript.NemesisNewMode(text, nodeID, nemesis); EDIT > Not needed as already in HUNT mode*/
                        }
                        else
                        {
                            //continue with existing hunt mode but bump up the timers a little
                            Debug.LogFormat("[Nem] NemesisManager.cs -> isNewGoal True -> ProcessNemesisActivity: Recent ACTIVITY -> Continue with Exisitng (timers +2){0}", "\n");
                            durationDelay += 2;
                            durationGoal += 2;
                        }
                    }
                    else
                    {
                        //in Normal mode, switch to Hunt
                        Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisActivity: isNewGoal True -> Recent ACTIVITY -> Switch Mode to HUNT{0}", "\n");
                        SetNemesisMode(NemesisMode.HUNT, turnDifference);
                        ProcessNemesisHunt();
                        //Chance nemesis could catch player in ProcessNemesisHunt -> MoveTo and revert to Null if no follow-on nemesis
                        if (nemesis != null)
                        {
                            if (mode != NemesisMode.Inactive)
                            {
                                string text = string.Format("{0} changes to {1} mode at {2}, {3} district", nemesis.name, mode, nemesisNode.name, nemesisNode.Arc.name);
                                GameManager.i.messageScript.NemesisNewMode(text, nodeID, nemesis);
                            }
                        }

                    }
                }
                else
                {
                    //no recent player activity
                    Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisActivity: isNewGoal True -> NO Activity -> Continue ({0}, {1}){2}", mode, goal, "\n");
                    ProcessNemesisGoal();
                }
            }
            else
            {
                targetDistance = -1;
                // Continue with existing goal
                Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisActivity: isNewGoal FALSE -> NO Activity -> Continue ({0}, {1}){2}", mode, goal, "\n");
                ProcessNemesisGoal();
            }
        }
        else { Debug.LogErrorFormat("Invalid nemesisNode (Null) for nodeID {0}", nodeID); }
    }

    /// <summary>
    /// Alternative to ProcessNemesisActivity when nemesis is under direct control of Authority player. Parameters are there solely to pass on if AI takes back control. No effect if Player controlled.
    /// </summary>
    private void ProcessNemesisPlayerControlled(AITracker tracker, bool immediateFlagResistance)
    {
        //decrement timer
        if (controlTimer > 0)
        {
            //Player control
            controlTimer--;
            //is Nemesis at required Node?
            int nemesisNodeID = GameManager.i.nodeScript.nodeNemesis;
            if (nemesisNodeID == controlNodeID)
            {
                //switch to goal
                switch (controlGoal)
                {
                    case NemesisGoal.AMBUSH:
                        //set manually as you don't want to trigger other actions with SetNemesisMode... etc.
                        mode = NemesisMode.NORMAL;
                        goal = NemesisGoal.AMBUSH;
                        Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisGoal: PLAYER Control, AMBUSH, controlTimer {0}{1}", controlTimer, "\n");
                        break;
                    case NemesisGoal.SEARCH:
                        //set manually as you don't want to trigger other actions with SetNemesisMode... etc.
                        mode = NemesisMode.HUNT;
                        goal = NemesisGoal.SEARCH;
                        Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisGoal: PLAYER Control, SEARCH, controlTimer {0}{1}", controlTimer, "\n");
                        //search routine
                        if (Random.Range(0, 100) < chanceSearchNeighbour)
                        {
                            //move to a random neighbouring node
                            Node node = nemesisNode.GetRandomNeighbour();
                            if (node != null)
                            {
                                Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisGoal: SEARCH goal, move to NEIGHBOUR{0}", "\n");
                                ProcessNemesisMove(node.nodeID);
                            }
                            else { Debug.LogWarning("Invalid neighbouring node (Null)"); }
                        }
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid controlGoal \"{0}\"", controlGoal);
                        break;
                }
            }
            else
            {
                //move to required Node
                Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisGoal: PLAYER Control, MoveTo, controlTimer {0}{1}", controlTimer, "\n");
                moveToNodeID = controlNodeID;
                ProcessNemesisMoveTo();
            }
        }
        else
        {
            //timer expired
            SetPlayerControlEnd();
            //revert to AI control
            ProcessNemesisActivity(tracker, immediateFlagResistance);
        }

    }

    /// <summary>
    /// Player has opted to directly control nemesis
    /// </summary>
    /// <param name="nodeID"></param>
    /// <param name="goal"></param>
    public void SetPlayerControlStart(int nodeID, NemesisGoal goal)
    {
        //cooldown period has ended
        if (controlCooldownTimer == 0)
        {
            Node node = GameManager.i.dataScript.GetNode(nodeID);
            if (node != null)
            {
                //deduct Power cost for control
                int power = GameManager.i.playerScript.Power;
                power -= controlPowerCost;
                if (power > -1)
                {
                    GameManager.i.playerScript.Power = power;
                    //change relevant fields
                    isPlayerControl = true;
                    controlNodeID = nodeID;
                    controlTimer = durationControlPeriod;
                    //default search goal if not a viable goal format
                    if (goal != NemesisGoal.AMBUSH && goal != NemesisGoal.SEARCH)
                    { controlGoal = NemesisGoal.SEARCH; }
                    else { controlGoal = goal; }
                    //set normal target node to invalid number (otherwise hunt mode will automatically act)
                    targetNodeID = -1;
                    //log
                    Debug.LogFormat("[Nem] NemesisManager.cs -> SetPlayerControlStart: Authority Player now has control of Nemesis, {0}, {1}, nodeID {2}, cost {3} Power{4}", node.nodeName, node.Arc.name,
                        node.nodeID, controlPowerCost, "\n");
                }
                else { Debug.LogWarning("Insufficient Power available to take control of Nemesis"); }
            }
            else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", nodeID); }
        }
        else { Debug.LogWarningFormat("Can't take control of Nemesis because cooldown Timer still active ({0} turns)", controlCooldownTimer); }
    }

    /// <summary>
    /// Player control of Nemesis has ended. Nemesis reverts back to Normal / Loiter, cooldown period activated (interval before player can resume control again)
    /// </summary>
    private void SetPlayerControlEnd()
    {
        //revert back to Normal / Loiter
        SetNemesisMode(NemesisMode.NORMAL);
        //set cooldown timer
        isPlayerControl = false;
        controlNodeID = -1;
        controlTimer = 0;
        controlCooldownTimer = durationControlCoolDown;
        //log
        Debug.LogFormat("[Nem] NemesisManager.cs -> SetPlayerControlStart: Authority Player NO LONGER has control of Nemesis, cooldown period {0}{1}", durationControlCoolDown, "\n");
    }

    /// <summary>
    /// sets mode and all associated variables in a consistent manner
    /// </summary>
    /// <param name="mode"></param>
    private void SetNemesisMode(NemesisMode requiredMode, int modifier = 0)
    {
        NemesisMode previousMode = mode;
        switch (requiredMode)
        {
            case NemesisMode.Inactive:
                //NOTE: durationMode for Inactive set locally by the calling metod
                mode = NemesisMode.Inactive;
                SetNemesisGoal(NemesisGoal.IDLE);
                targetNodeID = -1;
                moveToNodeID = -1;
                targetDistance = -1;
                Debug.LogFormat("[Nem] NemesisManager.cs -> SetNemesisMode: Nemesis Mode set to INACTIVE (previously {0}), duration {1}{2}", previousMode, durationDelay, "\n");
                break;
            case NemesisMode.NORMAL:
                mode = NemesisMode.NORMAL;
                SetNemesisGoal(NemesisGoal.LOITER);
                durationDelay = 0;
                targetNodeID = -1;
                moveToNodeID = -1;
                targetDistance = -1;
                Debug.LogFormat("[Nem] NemesisManager.cs -> SetNemesisMode: Nemesis Mode set to NORMAL (previously {0}){1}", previousMode, "\n");
                break;
            case NemesisMode.HUNT:
                Debug.LogFormat("[Nem] NemesisManager.cs -> SetNemesisMode: Nemesis Mode set to HUNT (previously {0}), duration {1}{2}", previousMode, durationDelay, "\n");
                mode = NemesisMode.HUNT;
                durationDelay = durationHuntMode + Random.Range(1, 10) - modifier;
                durationDelay = Mathf.Max(2, durationDelay);
                if (targetNodeID > -1)
                {
                    //target available
                    moveToNodeID = targetNodeID;
                    SetNemesisGoal(NemesisGoal.MOVE);
                }
                else { SetNemesisGoal(NemesisGoal.SEARCH); }
                break;
            default:
                Debug.LogWarningFormat("Invalid mode \"{0}\"", requiredMode);
                break;

        }
    }

    /// <summary>
    /// sets goal and all associated variables in a consistent manner.
    /// </summary>
    /// <param name="requiredGoal"></param>
    /// <param name="data0"></param>
    private void SetNemesisGoal(NemesisGoal requiredGoal)
    {
        NemesisGoal previousGoal = goal;
        switch (requiredGoal)
        {
            case NemesisGoal.MOVE:
                goal = NemesisGoal.MOVE;
                durationGoal = durationDelay;
                Debug.LogFormat("[Nem] NemesisManager.cs -> SetNemesisGoal: Nemesis Goal set to MoveToNode (previously {0}) moveToNodeID {1}{2}", previousGoal, moveToNodeID, "\n");
                break;
            case NemesisGoal.IDLE:
                goal = NemesisGoal.IDLE;
                durationGoal = 0;
                targetNodeID = -1;
                Debug.LogFormat("[Nem] NemesisManager.cs -> SetNemesisGoal: Nemesis Goal set to IDLE (previously {0}){1}", previousGoal, "\n");
                break;
            case NemesisGoal.LOITER:
                goal = NemesisGoal.LOITER;
                durationGoal = 0;
                targetNodeID = -1;
                Debug.LogFormat("[Nem] NemesisManager.cs -> SetNemesisGoal: Nemesis Goal set to LOITER (previously {0}){1}", previousGoal, "\n");
                break;
            case NemesisGoal.SEARCH:
                goal = NemesisGoal.SEARCH;
                targetNodeID = -1;
                moveToNodeID = -1;
                //searches for remainder of Hunt mode timer
                durationGoal = durationDelay;
                Debug.LogFormat("[Nem] NemesisManager.cs -> SetNemesisGoal: Nemesis Goal set to SEARCH (previously {0}){1}", previousGoal, "\n");
                break;
            case NemesisGoal.AMBUSH:
                goal = NemesisGoal.AMBUSH;
                targetNodeID = -1;
                durationGoal = durationAmbush;
                Debug.LogFormat("[Nem] NemesisManager.cs -> SetNemesisGoal: Nemesis Goal set to AMBUSH (previously {0}){1}", previousGoal, "\n");
                break;
            default:
                Debug.LogWarningFormat("Invalid requiredGoal \"{0}\"", requiredGoal);
                break;
        }
    }

    /// <summary>
    /// Player activity detected this turn (HUNT mode / MOVETO goal)
    /// </summary>
    /// <param name="isImmediate"></param>
    private void ProcessNemesisHunt()
    {
        if (targetNodeID > -1)
        {
            //get distance between nemesis and target (player activity) node
            targetDistance = GameManager.i.dijkstraScript.GetDistanceUnweighted(nemesisNode.nodeID, targetNodeID);

            //immediate flag (confirmed Player activity) -> overrides current goal
            if (isImmediate == true)
            {
                switch (targetDistance)
                {
                    case -1:
                        //problem with Dijkstra distance -> default Search
                        Debug.LogWarningFormat("[Nem] NemesisManager.cs -> ProcessNemesisHunt: targetDistance {0} -> SWITCH to SEARCH{1}", targetDistance, "\n");
                        SetNemesisGoal(NemesisGoal.SEARCH);
                        break;
                    case 0:
                        //switch to search mode
                        if (goal != NemesisGoal.SEARCH)
                        {
                            Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisHunt: targetDistance {0} -> SWITCH to SEARCH{1}", targetDistance, "\n");
                            SetNemesisGoal(NemesisGoal.SEARCH);
                        }
                        break;
                    case 1:
                        //chance to switch to search mode
                        if (Random.Range(0, 100) < chanceStartEarlySearch)
                        {
                            Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisHunt: targetDistance {0} -> SWITCH to SEARCH{1}", targetDistance, "\n");
                            SetNemesisGoal(NemesisGoal.SEARCH);
                        }
                        //move towards player at full speed
                        else
                        {
                            if (goal != NemesisGoal.MOVE)
                            {
                                Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisHunt: targetDistance {0} -> SWITCH to MoveTo{1}", targetDistance, "\n");
                                SetNemesisGoal(NemesisGoal.MOVE);
                            }
                            else { Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisHunt: targetDistance {0} -> CONTINUE MoveTo{1}", targetDistance, "\n"); }
                            ProcessNemesisMoveTo();
                        }
                        break;
                    case 2:
                        //chance to switch to search mode
                        if (Random.Range(0, 100) < chanceStartEarlySearch)
                        {
                            Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisHunt: targetDistance {0} -> SWITCH to SEARCH{1}", targetDistance, "\n");
                            SetNemesisGoal(NemesisGoal.SEARCH);
                        }
                        //move towards player at full speed
                        else
                        {
                            if (goal != NemesisGoal.MOVE)
                            {
                                Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisHunt: targetDistance {0} -> SWITCH to MoveTo{1}", targetDistance, "\n");
                                SetNemesisGoal(NemesisGoal.MOVE);
                            }
                            else { Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisHunt: targetDistance {0} -> CONTINUE MoveTo{1}", targetDistance, "\n"); }
                            ProcessNemesisMoveTo();
                        }
                        break;
                    default:
                        //more than 2 away
                        //move towards player at full speed
                        if (goal != NemesisGoal.MOVE)
                        {
                            Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisHunt: targetDistance {0} -> CONTINUE MoveTo{1}", targetDistance, "\n");
                            SetNemesisGoal(NemesisGoal.MOVE);
                        }
                        ProcessNemesisMoveTo();
                        break;
                }
            }
            else
            {
                //continue with existing hunt goal
                ProcessNemesisGoal();
            }
        }
        else { Debug.LogWarning("Invalid targetNodeID (-1)"); }
    }

    /// <summary>
    /// Nemesis proceeds with curent goal
    /// </summary>
    private void ProcessNemesisGoal()
    {
        if (mode != NemesisMode.Inactive)
        {
            switch (goal)
            {
                case NemesisGoal.AMBUSH:
                    if (durationGoal > 0)
                    { Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisGoal: Nemesis continues with AMBUSH, duration {0},  nodeID {1}{2}", durationGoal, nemesisNode.nodeID, "\n"); }
                    else
                    {
                        //change goal
                        if (mode == NemesisMode.NORMAL)
                        { SetNemesisGoal(NemesisGoal.LOITER); }
                        else if (mode == NemesisMode.HUNT)
                        { SetNemesisGoal(NemesisGoal.SEARCH); }
                    }
                    break;
                case NemesisGoal.SEARCH:
                    //HUNT mode only -> chance to move to a random node or stay searching in current node
                    if (Random.Range(0, 100) < chanceSearchNeighbour)
                    {
                        //move to a random neighbouring node
                        Node node = nemesisNode.GetRandomNeighbour();
                        if (node != null)
                        {
                            Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisGoal: SEARCH goal, move to NEIGHBOUR{0}", "\n");
                            ProcessNemesisMove(node.nodeID);
                        }
                        else { Debug.LogWarning("Invalid neighbouring node (Null)"); }
                    }
                    break;
                case NemesisGoal.MOVE:
                    //HUNT mode only
                    ProcessNemesisMoveTo();
                    break;
                case NemesisGoal.IDLE:
                    //NORMAL mode only -> chance to switch to Loiter goal
                    if (Random.Range(0, 100) < chanceIdleToLoiter)
                    { SetNemesisGoal(NemesisGoal.LOITER); }
                    else { Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisGoal: Nemesis retains IDLE goal, current nodeID {0}{1}", nemesisNode.nodeID, "\n"); }
                    break;
                case NemesisGoal.LOITER:
                    //NORMAL mode only -> nemesis already at loiter node?
                    if (nemesisNode.isLoiterNode == true)
                    {
                        //at loiter node, chance to move to a random neighbour (one turn only, will revert back to loiter node)
                        if (Random.Range(0, 100) < chanceLoiterToNeighbour)
                        {
                            //move to a random neighbouring node
                            Node node = nemesisNode.GetRandomNeighbour();
                            if (node != null)
                            {
                                Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisGoal: LOITER goal, move to NEIGHBOUR{0}", "\n");
                                ProcessNemesisMove(node.nodeID);
                            }
                            else { Debug.LogWarning("Invalid neighbouring node (Null)"); }
                        }
                        else
                        {
                            //at LoiterNode
                            Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisGoal: At Loiter Node {1}, id {2}, do NOTHING{3}", goal, nemesisNode.nodeName, nemesisNode.nodeID, "\n");
                        }
                    }
                    //not yet at LoiterNode, move towards nearest at speed 1
                    else
                    {
                        //chance to switch to IDLE
                        if (Random.Range(0, 100) < chanceLoiterToIdle)
                        { SetNemesisGoal(NemesisGoal.IDLE); }
                        //chance to switch to AMBUSH
                        else if (Random.Range(0, 100) < chanceLoiterToAmbush)
                        { SetNemesisGoal(NemesisGoal.AMBUSH); }
                        //Continue with Loiter mode
                        else
                        {
                            ProcessNemesisMove(nemesisNode.loiter.neighbourID);
                            Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisGoal: Nemesis continues with LOITER, current nodeID {0}{1}", nemesisNode.nodeID, "\n");
                        }
                    }
                    break;
                default:
                    Debug.LogWarningFormat("Invalid NemesisGoal \"{0}\"", goal);
                    break;
            }
        }
    }

    /// <summary>
    /// specific MoveTo goal where Nemesis heads to a target nodeID at their maximum move allowance
    /// </summary>
    /// <param name="nodeID"></param>
    private void ProcessNemesisMoveTo()
    {
        //check not already at destination
        if (nemesisNode.nodeID != moveToNodeID)
        {
            if (moveToNodeID > -1)
            {
                //Get path to 
                List<Connection> listOfConnections = GameManager.i.dijkstraScript.GetPath(nemesisNode.nodeID, moveToNodeID, false);
                if (listOfConnections != null)
                {
                    int numOfLinks = listOfConnections.Count;
                    int nextNodeID;
                    if (numOfLinks > 0)
                    {
                        //get nemesis move allowance
                        int moveAllowance = nemesis.movement;
                        int index = 0;
                        bool isSpotted;
                        if (moveAllowance > 0)
                        {
                            //move nemesis multiple links if allowed, stop moving immediately if nemesis spots Player at same node
                            do
                            {
                                isSpotted = false;
                                moveAllowance--;
                                Connection connection = listOfConnections[index];
                                if (connection != null)
                                {
                                    //get the node to move to for this link
                                    nextNodeID = connection.GetNode1();
                                    if (nextNodeID == nemesisNode.nodeID)
                                    { nextNodeID = connection.GetNode2(); }
                                    //move forward one link
                                    isSpotted = ProcessNemesisMove(nextNodeID, index);
                                }
                                else { Debug.LogWarningFormat("Invalid connection (Null) in listOfConnections[{0}]", index); }
                                index++;

                            }
                            while (moveAllowance > 0 && index < numOfLinks && isSpotted == false);
                            //check if at target node
                            if (nemesisNode.nodeID == moveToNodeID)
                            {
                                //switch to search mode (provided not player controlled)
                                if (isPlayerControl == false)
                                { SetNemesisGoal(NemesisGoal.SEARCH); }
                                else { goal = controlGoal; }
                            }
                        }
                        else { Debug.LogWarning("Nemesis moveAllowance invalid (Zero)"); }
                    }
                    else { Debug.LogWarningFormat("Invalid listOfConnections (Empty) between nemesis nodeID {0} and moveTo node ID {1}", nemesisNode.nodeID, moveToNodeID); }
                }
                else { Debug.LogWarning("Invalid listOfConnections (Null)"); }
            }
            else { Debug.LogWarning("Invalid moveToNodeID (-1)"); }
        }
        else
        {
            Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessNemesisMoveTo: Nemesis node id {0} and moveTOnode id {1} are identical. Switch to Search Mode", nemesisNode.nodeID, moveToNodeID);
            //already at target destination, switch to search mode
            SetNemesisGoal(NemesisGoal.SEARCH);
        }
    }

    /// <summary>
    /// method to move nemesis one link. Handles admin and player and contact interaction checks. moveNumber is number of move in sequential order for this turn. '0' is for first move.
    /// NOTE: Assumed to be a single link move.
    /// Returns true if player spotted at same node as Nemesis
    /// </summary>
    /// <param name="nodeID"></param>
    private bool ProcessNemesisMove(int nodeID, int moveNumber = 0)
    {
        bool isSpotted = false;
        //check if node is a neighbour of current nemesis node (assumed to be a single link move)
        if (nemesisNode.CheckNeighbourNodeID(nodeID) == true)
        {
            //update nemesisManager
            nemesisNode = GameManager.i.dataScript.GetNode(nodeID);
            if (nemesisNode != null)
            {
                //update nodeManager
                GameManager.i.nodeScript.nodeNemesis = nodeID;
                GameManager.i.nodeScript.NodeRedraw = true;
                hasMoved = true;
                Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessMoveNemesis: Nemesis MOVES to node {0}, {1}, id {2}{3}", nemesisNode.nodeName, nemesisNode.Arc.name, nemesisNode.nodeID, "\n");
                int playerNodeID = GameManager.i.nodeScript.GetPlayerNodeID();
                //tracker
                HistoryNemesisMove history = new HistoryNemesisMove();
                history.turn = GameManager.i.turnScript.Turn;
                history.nemesisNodeID = nodeID;
                history.mode = mode;
                history.goal = goal;
                history.targetNodeID = targetNodeID;
                history.searchRating = GetSearchRatingAdjusted();
                history.playerNodeID = playerNodeID;
                GameManager.i.dataScript.AddHistoryNemesisMove(history);
                //check for player at same node
                if (nemesisNode.nodeID == playerNodeID)
                { isSpotted = ProcessPlayerInteraction(false); }
                //Player interaction can result in nemesis damaging player and leaving with no follow-on nemesis
                if (nemesis != null)
                {
                    if (mode != NemesisMode.Inactive)
                    {
                        //OrgInfo NOT Involved
                        if (GameManager.i.dataScript.CheckOrgInfoType(OrgInfoType.Nemesis) == false)
                        {
                            //check for Resistance contact at same node
                            List<int> tempList = GameManager.i.dataScript.CheckContactResistanceAtNode(nodeID);
                            if (tempList != null)
                            { ProcessContactInteraction(tempList, moveNumber); }
                            //check for Tracer Sighting
                            CheckNemesisTracerSighting(moveNumber);
                        }
                        else
                        {
                            //OrgInfo provides a direct feed of Nemesis position to Player
                            Node node = GameManager.i.dataScript.GetNode(nodeID);
                            if (node != null)
                            {
                                string text = string.Format("OrgInfo tracks {0} Nemesis at {1}, {2}, ID {3}", nemesis.name, node.nodeName, node.Arc.name, node.nodeID);
                                GameManager.i.messageScript.OrganisationNemesis(text, node, nemesis);
                            }
                            else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", nodeID); }
                        }
                    }
                }
            }
            else { Debug.LogWarningFormat("Invalid move node {Null) for nodeID {0}", nodeID); }
        }
        else
        {
            Debug.LogWarningFormat("Invalid move nodeId (Doesn't match any of neighbours) for nodeID {0} and nemesisNode {1}, {2}, id {3}{4}", nodeID, nemesisNode.nodeName, nemesisNode.Arc.name,
         nemesisNode.nodeID, "\n");
        }
        return isSpotted;
    }


    /// <summary>
    /// nemesis and player at same node. For end of turn checks set 'isPlayerMove' to false as this tweaks modal setting of outcome window to handle MainInfoApp, leave as default true otherwise
    /// returns true if player spotted by nemesis. 
    /// </summary>
    private bool ProcessPlayerInteraction(bool isPlayerMove = true)
    {
        bool isSpotted = false;
        bool isResistancePlayer = true;
        if (GameManager.i.sideScript.PlayerSide.level == 1) { isResistancePlayer = false; }
        //player spotted if nemesis search rating >= player invisibility
        int searchRating = GetSearchRatingAdjusted();
        if (searchRating >= GameManager.i.playerScript.Invisibility)
        {

            bool isValidPlayer = true;
            //can't be spotted if Lying Low or on Stress Leave (but can for breakdown) 
            switch (resistancePlayer)
            {
                case SideState.AI:
                    if (GameManager.i.aiRebelScript.status == ActorStatus.Inactive && GameManager.i.aiRebelScript.inactiveStatus != ActorInactive.Breakdown)
                    { isValidPlayer = false; }
                    break;
                case SideState.Human:
                    if (GameManager.i.playerScript.status == ActorStatus.Inactive && GameManager.i.playerScript.inactiveStatus != ActorInactive.Breakdown)
                    { isValidPlayer = false; }
                    break;
                default:
                    Debug.LogErrorFormat("Unrecognised resistancePlayer \"{0}\"", resistancePlayer);
                    break;
            }
            //proceed
            if (isValidPlayer == true)
            {
                //player SPOTTED
                isSpotted = true;
                Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessPlayerInteraction: PLAYER SPOTTED at node {0}, {1}, id {2}{3}", nemesisNode.nodeName, nemesisNode.Arc.name, nemesisNode.nodeID, "\n");

                /*Debug.LogFormat("[Tst] NemesisManager.cs -> ProcessPlayerInteraction: Human status {0} inactive {1}, AI status {2} inactive {3}{4}",
                    GameManager.instance.playerScript.status, GameManager.instance.playerScript.inactiveStatus,
                    GameManager.instance.aiRebelScript.status, GameManager.instance.aiRebelScript.inactiveStatus, "\n");*/

                //cause damage / message
                ProcessPlayerDamage();
                hasActed = true;
                //orgInfo reset?
                if (GameManager.i.campaignScript.campaign.orgInfo != null)
                { GameManager.i.orgScript.CancelOrgInfoTracking(OrgInfoType.Nemesis); }
                //Nemesis has done their job, new nemesis arrives?
                if (isFirstNemesis == true)
                {
                    //get second nemesis
                    isFirstNemesis = false;
                    nemesis = GameManager.i.scenarioScript.scenario.challengeResistance.nemesisSecond;
                    if (nemesis != null)
                    {
                        //place Nemesis OFFLINE for a period (standard damage wait plus any new nemesis grace period)
                        SetNemesisMode(NemesisMode.Inactive);
                        durationDelay = durationDamageOffLine + GameManager.i.scenarioScript.scenario.challengeResistance.gracePeriodSecond;
                        string.Format("[Nem] NemesisManager.cs -> ProcessPlayerInteraction: NEW Nemesis arrives, {0}, offline for {1} turns{2}", nemesis.name, durationDelay, "\n");
                        if (durationDelay > 0)
                        {
                            if (isResistancePlayer == true)
                            {
                                string text = string.Format("New Nemesis in {0} turns after player damaged", durationDelay);
                                string itemText = "Rumours of a new NEMESIS";
                                string topText = "Nemesis OFFLINE";
                                string reason = string.Format("{0}{1}<b>{2} Nemesis</b>{3}", "\n", colourAlert, nemesis.name, colourEnd);
                                string warning = string.Format("It's a new Nemesis!{0}Rebel HQ STRONGLY ADVISE that you get the heck out of there", "\n");
                                GameManager.i.messageScript.GeneralWarning(text, itemText, topText, reason, warning, false);
                            }
                            else
                            {
                                //authority player
                                string text = string.Format("New Nemesis online in {0} turns", durationDelay);
                                string itemText = "New NEMESIS on their way";
                                string topText = "Nemesis OFFLINE";
                                string reason = string.Format("{0}{1}<b>{2} Nemesis</b>{3}", "\n", colourAlert, nemesis.name, colourEnd);
                                string warning = string.Format("There will be a delay ({0} turn{1}) before the Nemesis arrives", durationDelay, durationDelay != 1 ? "s" : "");
                                GameManager.i.messageScript.GeneralWarning(text, itemText, topText, reason, warning, false);
                            }
                        }
                    }
                    //no more nemesis after first
                    else
                    {
                        if (isResistancePlayer == true)
                        {
                            string text = "NO new Nemesis after player damaged";
                            string itemText = "NEMESIS threat eases";
                            string topText = "Nemesis M.I.A";
                            string reason = string.Format("{0}<b>It appears that there is no longer a Nemesis in the City</b>", "\n");
                            string explanation = "<b>Rebel HQ can provide no further information on the situation</b>";
                            GameManager.i.messageScript.GeneralInfo(text, itemText, topText, reason, explanation);
                        }
                        else
                        {
                            //authority player
                            string text = "First Nemesis has done their job and will depart the city";
                            string itemText = "First NEMESIS gone";
                            string topText = "Nemesis Recalled";
                            string reason = string.Format("{0}<b>HQ have decided to recall the Nemesis from the City as it has done it's job</b>", "\n");
                            string explanation = "<b>There will be no further Nemesis</b>";
                            GameManager.i.messageScript.GeneralInfo(text, itemText, topText, reason, explanation);
                        }
                    }
                }
                else
                {
                    //2nd Nemesis has done it's job -> no more nemesis
                    nemesis = null;
                    SetNemesisMode(NemesisMode.Inactive);
                    //message
                    if (isResistancePlayer == true)
                    {
                        string text = "NO new Nemesis after player damaged{0}";
                        string itemText = "NEMESIS threat eases";
                        string topText = "Nemesis M.I.A";
                        string reason = string.Format("{0}<b>It appears that there is no longer a Nemesis in the City</b>", "\n");
                        string explanation = "<b>Rebel HQ can provide no further information on the situation</b>";
                        GameManager.i.messageScript.GeneralInfo(text, itemText, topText, reason, explanation);
                    }
                    else
                    {
                        //authority player
                        string text = "Second Nemesis has done their job and will depart the city";
                        string itemText = "Second NEMESIS gone";
                        string topText = "Nemesis Recalled";
                        string reason = string.Format("{0}<b>HQ have decided to recall the second Nemesis from the City as it has done it's job</b>", "\n");
                        string explanation = "<b>This decision is final and cannot be appealed</b>";
                        GameManager.i.messageScript.GeneralInfo(text, itemText, topText, reason, explanation);
                    }
                }
            }
            else
            {
                Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessPlayerInteraction: Player NOT Spotted at node {0}, {1}, id {2}, due to LYING LOW{3}",
             nemesisNode.nodeName, nemesisNode.Arc.name, nemesisNode.nodeID, "\n");
            }
        }
        else
        {
            //player NOT spotted
            Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessPlayerInteraction: Player NOT Spotted at node {0}, {1}, id {2}{3}", nemesisNode.nodeName, nemesisNode.Arc.name, nemesisNode.nodeID, "\n");
            //warn player (only if resistance side)
            if (isResistancePlayer == true)
            {
                //prevent a double warning if player moves into a node with nemesis and nemesis is stationary
                if (hasWarning == false)
                {
                    Node node = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.GetPlayerNodeID());
                    if (node != null)
                    {
                        hasWarning = true;
                        string text = string.Format("You feel the presence of a <b>DARK SHADOW</b> at {0}, {1} district", node.nodeName, node.Arc.name);
                        string itemText = "You feel the presence of a DARK SHADOW";
                        string topText = "Something is WRONG";
                        string reason = string.Format("{0}Could it be that your <b>NEMESIS</b> is nearby?", "\n");
                        string warning = "Your instincts urge you to move, NOW";
                        GameManager.i.messageScript.GeneralWarning(text, itemText, topText, reason, warning, true, true);
                    }
                    else { Debug.LogWarning("Invalid nodePlayer (Null)"); }
                }
            }
        }
        return isSpotted;
    }

    /// <summary>
    /// returns Nemesis Search rating after adjusting for mode and activiy
    /// </summary>
    /// <returns></returns>
    public int GetSearchRatingAdjusted()
    {
        int searchRating = nemesis.searchRating;
        //adjust for mode
        if (goal == NemesisGoal.SEARCH)
        { searchRating++; }
        searchRating = Mathf.Clamp(searchRating, 1, 3);
        return searchRating;
    }

    /// <summary>
    /// returns Nemesis Stealth rating after adjusting for mode and activiy
    /// </summary>
    /// <returns></returns>
    public int GetStealthRatingAdjusted()
    {
        int stealthRating = nemesis.stealthRating;
        //adjust for mode
        if (mode == NemesisMode.HUNT)
        { stealthRating--; }
        //adjust for goal
        if (goal == NemesisGoal.AMBUSH)
        { stealthRating += 999; }
        return stealthRating;
    }

    /// <summary>
    /// nemesis at same node as one or more resistance contacts. moveNumber is the sequential move in the sequence of moves for this turn, '0' being the first
    /// </summary>
    /// <param name="listOfActorsWithContactsAtNode"></param>
    private void ProcessContactInteraction(List<int> listOfActorsWithContactsAtNode, int moveNumber = 0)
    {
        Actor actor;
        Contact contact;
        if (listOfActorsWithContactsAtNode != null)
        {
            int numOfActors = listOfActorsWithContactsAtNode.Count;
            if (numOfActors > 0)
            {
                //nemesis stealthRating
                int stealthRating = GetStealthRatingAdjusted();
                //loop actors with contacts
                for (int i = 0; i < numOfActors; i++)
                {
                    actor = GameManager.i.dataScript.GetActor(listOfActorsWithContactsAtNode[i]);
                    if (actor != null)
                    {
                        //only active actors can work their contact network
                        if (actor.Status == ActorStatus.Active)
                        {
                            contact = actor.GetContact(nemesisNode.nodeID);
                            if (contact != null)
                            {
                                //contact active
                                if (contact.status == ContactStatus.Active)
                                {
                                    //check nemesis stealth rating vs. contact effectiveness
                                    if (contact.effectiveness >= stealthRating)
                                    {
                                        Node node = nemesisNode;
                                        //contact spots Nemesis
                                        string text = string.Format("Nemesis {0} has been spotted by Contact {1} {2}, {3}, at node {4}, id {5}", nemesis.name, contact.nameFirst, contact.nameLast,
                                            contact.job, node.nodeName, node.nodeID);
                                        Debug.LogFormat("[Cnt] NemesisManager.cs -> ProcessContactInteraction: Contact {0}, effectiveness {1}, SPOTS Nemesis {2}, adj StealthRating {3} at node {4}, id {5}{6}",
                                            contact.nameFirst, contact.effectiveness, nemesis.name, stealthRating, node.nodeName, node.nodeID, "\n");
                                        GameManager.i.messageScript.ContactNemesisSpotted(text, actor, node, contact, nemesis, moveNumber);
                                        //contact stats
                                        contact.statsNemesis++;
                                        //no need to check anymore as one sighting is enough
                                        break;
                                    }
                                    else
                                    {
                                        //contact Fails to spot Nemesis
                                        Debug.LogFormat("[Cnt] NemesisManager.cs -> ProcessContactInteraction: Contact {0}, effectiveness {1}, FAILS to spot Nemesis {2}, adj StealthRating {3} at nodeID {4}{5}",
                                            contact.nameFirst, contact.effectiveness, nemesis.name, stealthRating, nemesisNode.nodeID, "\n");
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogFormat("Invalid contact (Null) for actor {0}, id {1} at node {2}, {3}, id {4}", actor.actorName, actor.actorID, nemesisNode.nodeName,
                                      nemesisNode.Arc.name, nemesisNode.nodeID);
                            }
                        }
                        else
                        {
                            Debug.LogFormat("[Cnt] NemesisManager.cs -> ProcessContactInteraction: Actor {0}, {1}, id {2}, is INACTIVE and can't access their contacts{3}", actor.actorName,
                                 actor.arc.name, actor.actorID, "\n");
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", listOfActorsWithContactsAtNode[i]); }
                }
            }
            else { Debug.LogWarning("Invalid listOfActorsWithContactsAtNode (Empty)"); }
        }
        else { Debug.LogWarning("Invalid listOfActorsWithContactsAtNode (Null)"); }
    }

    /// <summary>
    /// Check if nemesis spotted by Contacts at start of a turn where the nemesis hasn't moved
    /// </summary>
    public void CheckNemesisContactSighting()
    {
        if (nemesis != null)
        {
            //ignored if nemesis inactive
            if (mode != NemesisMode.Inactive)
            {
                if (hasMoved == false)
                {
                    int nodeID = GameManager.i.nodeScript.nodeNemesis;
                    if (nodeID > -1)
                    {
                        //OrgInfo not involved
                        if (GameManager.i.dataScript.CheckOrgInfoType(OrgInfoType.Nemesis) == false)
                        {
                            //check for Resistance contact at same node
                            List<int> tempList = GameManager.i.dataScript.CheckContactResistanceAtNode(nodeID);
                            if (tempList != null)
                            { ProcessContactInteraction(tempList); }
                        }
                        else
                        {
                            //OrgInfo IS involved and provides info directly to player in lieu of contact report
                            Node node = GameManager.i.dataScript.GetNode(nodeID);
                            if (node != null)
                            {
                                string text = string.Format("OrgInfo tracks {0} Nemesis at {1}, {2}, ID {3}", nemesis.name, node.nodeName, node.Arc.name, node.nodeID);
                                GameManager.i.messageScript.OrganisationNemesis(text, node, nemesis);
                            }
                            else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", nodeID); }
                        }
                    }
                    else { Debug.LogWarning("Invalid nodeNemesis (-1)"); }
                }
            }
        }
    }

    /// <summary>
    /// check if nemesis automatically spotted by a Tracer that is inserted in the node they are currently in. Will run regardless of 'hasWarning' (additional info from a secondary source)
    /// </summary>
    public void CheckNemesisTracerSighting(int moveNumber = 0)
    {
        if (nemesisNode.isTracer == true)
        {
            //automatically SPOTTED -> node is always correct
            string text = string.Format("Tracer picks up an Anomalous reading at {0}, {1} district", nemesisNode.nodeName, nemesisNode.Arc.name);
            GameManager.i.messageScript.TracerNemesisSpotted(text, nemesisNode, nemesis, moveNumber);
        }
    }

    /// <summary>
    /// check if nemesis is at the same node as the player. Used when nemesis hasn't moved (start of turn, 'isPlayerMove' false) and when player moving to a new node ('isPlayerMove' true)
    /// if nemesis stationary at start of turn there is a further check to prevent nemesis damaging the player twice in a row if they have already done so during the AI turn
    /// </summary>
    public void CheckNemesisAtPlayerNode(bool isPlayerMove = false)
    {
        //nemesis must be present OnMap
        if (nemesis != null)
        {
            //ignored if nemesis inactive
            if (mode != NemesisMode.Inactive)
            {
                //Resistance player must be active (or suffering a stress induced breakdown) in order to be found
                bool isValidPlayer = true;
                switch (resistancePlayer)
                {
                    case SideState.AI:
                        if (GameManager.i.aiRebelScript.status == ActorStatus.Inactive && GameManager.i.aiRebelScript.inactiveStatus != ActorInactive.Breakdown)
                        { isValidPlayer = false; }
                        break;
                    case SideState.Human:
                        if (GameManager.i.playerScript.status == ActorStatus.Inactive && GameManager.i.playerScript.inactiveStatus != ActorInactive.Breakdown)
                        { isValidPlayer = false; }
                        break;
                    default:
                        Debug.LogErrorFormat("Unrecognised resistancePlayer \"{0}\"", resistancePlayer);
                        break;
                }
                //proceed?
                if (isValidPlayer == true)
                {
                    bool isCheckNeeded = false;
                    if (isPlayerMove == false)
                    {
                        //start of turn (player hasn't moved) and nemesis hasn't moved
                        if (hasMoved == false)
                        {
                            //can only check if nemesis didn't damage the player during the AI turn (gives the player a chance to leave)
                            if (hasActed == false)
                            { isCheckNeeded = true; }
                        }
                    }
                    //player moving to a new node, automatic check
                    else { isCheckNeeded = true; }
                    //proceed with a check
                    if (isCheckNeeded == true)
                    {
                        //both at same node
                        if (nemesisNode.nodeID == GameManager.i.nodeScript.GetPlayerNodeID())
                        { ProcessPlayerInteraction(isPlayerMove); }
                    }
                }
            }
        }
    }

    /// <summary>
    /// called whenever Nemesis spots and catches player. Both assumed to be at the same node.
    /// </summary>
    private void ProcessPlayerDamage()
    {
        StringBuilder builder = new StringBuilder();
        Damage damage = nemesis.damage;
        if (damage != null)
        {
            Condition condition;
            switch (damage.name)
            {
                case "Ransom":
                    GameManager.i.playerScript.RansomPlayer();
                    break;
                case "Discredit":
                    condition = GameManager.i.dataScript.GetCondition("CORRUPT");
                    if (condition != null)
                    { GameManager.i.playerScript.AddCondition(condition, globalResistance, "due to ScumBot Nemesis"); }
                    else { Debug.LogWarningFormat("Invalid condition CORRUPT (Null)"); }
                    break;
                case "Image":
                    condition = GameManager.i.dataScript.GetCondition("IMAGED");
                    if (condition != null)
                    { GameManager.i.playerScript.AddCondition(condition, globalResistance, "due to Paparrazi Nemesis"); }
                    else { Debug.LogWarningFormat("Invalid condition IMAGED (Null)"); }
                    break;
                case "Inject":
                    condition = GameManager.i.dataScript.GetCondition("DOOMED");
                    if (condition != null)
                    { GameManager.i.playerScript.AddCondition(condition, globalResistance, "due to Assassin Droid"); }
                    else { Debug.LogWarningFormat("Invalid condition DOOMED (Null)"); }
                    break;
                case "Tag":
                    condition = GameManager.i.dataScript.GetCondition("TAGGED");
                    if (condition != null)
                    { GameManager.i.playerScript.AddCondition(condition, globalResistance, "due to Cyber Hound"); }
                    else { Debug.LogWarningFormat("Invalid condition TAGGED (Null)"); }
                    break;
                case "Wound":
                    condition = GameManager.i.dataScript.GetCondition("WOUNDED");
                    if (condition != null)
                    { GameManager.i.playerScript.AddCondition(condition, globalResistance, "due to Security Droid"); }
                    else { Debug.LogWarningFormat("Invalid condition WOUNDED (Null)"); }
                    break;
                default:
                    builder.AppendFormat("Damage is of an unknown kind");
                    Debug.LogWarningFormat("Invalid damage \"{0}\"", damage.name);
                    break;
            }
            Debug.LogFormat("[Nem] NemesisManager.cs -> ProcessPlayerDamage: Nemesis DAMAGES (\"{0}\") Player{1}", damage.name, "\n");
            Debug.LogFormat("[Ply] NemesisManager.cs -> ProcessPlayerDamage: Nemesis DAMAGES (\"{0}\") Player{1}", damage.name, "\n");
            string text, textAutoRun;
            GlobalSide sideWho;
            //Message
            string msgText = string.Format("{0} has been {1} by their {2} Nemesis", GameManager.i.playerScript.GetPlayerNameResistance(), nemesis.damage.tag, nemesis.name);
            if (GameManager.i.sideScript.PlayerSide.level == globalResistance.level)
            {
                //resistance human player
                text = string.Format("Player has been found and targeted by their{0}{1}<b>{2} Nemesis</b>{3}", "\n", colourNeutral, nemesis.name, colourEnd);
                textAutoRun = string.Format("You have been {0}{1}{2}", colourBad, nemesis.damage.tag, colourEnd);
                builder.AppendFormat("{0}, {1} district{2}{3}", nemesisNode.nodeName, nemesisNode.Arc.name, "\n", "\n");
                builder.AppendFormat("{0}<b>Resistance Player {1}</b>{2}", colourBad, nemesis.damage.tag, colourEnd);
                builder.AppendFormat("{0}{1}{2}<b>{3}</b>{4}", "\n", "\n", colourAlert, nemesis.damage.effectResistance, colourEnd);
                GameManager.i.messageScript.PlayerDamage(msgText, nemesis.damage.tag, nemesis.damage.effectResistance, nemesisNode.nodeID);
                sideWho = GameManager.i.globalScript.sideResistance;
                //history
                GameManager.i.dataScript.AddHistoryPlayer(new HistoryActor() { text = "You've been found and Targeted by your NEMESIS", district = nemesisNode.nodeName });
            }
            else
            {
                //authority human player
                text = string.Format("Resistance Player has been found and targeted by their{0}{1}<b>{2} Nemesis</b>{3}", "\n", colourNeutral, nemesis.name, colourEnd);
                textAutoRun = string.Format("{0} {1}{2}{3}", GameManager.i.playerScript.GetPlayerNameResistance(), colourBad, nemesis.damage.tag, colourEnd);
                builder.AppendFormat("{0}, {1} district{2}{3}", nemesisNode.nodeName, nemesisNode.Arc.name, "\n", "\n");
                builder.AppendFormat("{0}<b>{1} {2}</b>{3}", colourBad, GameManager.i.playerScript.GetPlayerNameResistance(), nemesis.damage.tag, colourEnd);
                builder.AppendFormat("{0}{1}{2}<b>{3}</b>{4}", "\n", "\n", colourAlert, nemesis.damage.effectAuthority, colourEnd);
                GameManager.i.messageScript.PlayerDamage(msgText, nemesis.damage.tag, nemesis.damage.effectAuthority, nemesisNode.nodeID);
                sideWho = GameManager.i.globalScript.sideAuthority;
                //history
                GameManager.i.dataScript.AddHistoryPlayer(new HistoryActor() { text = "Resistance Leader has been found and Targeted by your NEMESIS", district = nemesisNode.nodeName });
            }
            //AutoRun Event
            if (GameManager.i.turnScript.CheckIsAutoRun() == true)
            { GameManager.i.dataScript.AddHistoryAutoRun(textAutoRun); }
            else
            {
                //player damaged outcome window
                ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
                {
                    textTop = text,
                    textBottom = builder.ToString(),
                    sprite = GameManager.i.spriteScript.aiAlertSprite,
                    isAction = false,
                    side = sideWho,
                    type = MsgPipelineType.Nemesis
                };
                //end of turn outcome window which needs to overlay ontop of InfoAPP and requires a different than normal modal setting
                if (GameManager.i.guiScript.InfoPipelineAdd(outcomeDetails) == false)
                { Debug.LogWarningFormat("Nemesis Damage infoPipeline message FAILED to be added to dictOfPipeline"); }
            }
        }
        else { Debug.LogWarning("Invalid damage (Null)"); }
    }

    //
    // - - - Loiter - - -
    //

    /// <summary>
    /// Sets up a list (max 3) of nodes which are well-connected and, hopefully, centred, where the nemesis can sit and wait for developments
    /// </summary>
    private void SetLoiterNodes()
    {
        int numOfNodes, counter, distance;
        List<Node> listOfLoiterNodes = GameManager.i.dataScript.GetListOfLoiterNodes();
        List<Node> listOfMostConnected = GameManager.i.dataScript.GetListOfMostConnectedNodes();
        Node centreNode = null;
        if (listOfMostConnected != null)
        {
            if (listOfLoiterNodes != null)
            {
                numOfNodes = listOfMostConnected.Count;
                //loop through most Connected looking for the first instance of a centre, connected node (the most connected are checked first, least connected last)
                for (int index = 0; index < numOfNodes; index++)
                {
                    Node node = listOfMostConnected[index];
                    if (node != null)
                    {
                        if (node.isCentreNode == true)
                        {
                            //found the ideal node, job done
                            centreNode = node;
                            /*Debug.LogFormat("[Nem] NemesisManager.cs -> SetLoiterNodes: CENTRE -> there is a CENTRE nodeID {0}", centreNode.nodeID);*/
                            break;
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid node (Null) for listOfMostConnected[{0}]", index); }
                }
                /*if (centreNode == null)
                    { Debug.Log("[Nem] NemesisManager.cs -> SetLoiterNodes: CENTRE -> there is NO Centre node"); }*/

                //Take the top 'x' most connected nodes (excluding centreNode, if any) and add to loiterList
                counter = 0;
                for (int index = 0; index < numOfNodes; index++)
                {
                    Node node = listOfMostConnected[index];
                    //check not the centreNode
                    if (centreNode != null)
                    {
                        if (node.nodeID != centreNode.nodeID)
                        {
                            listOfLoiterNodes.Add(node);
                            counter++;
                        }
                    }
                    else
                    {
                        listOfLoiterNodes.Add(node);
                        counter++;
                    }
                    //check limit isn't exceeded
                    if (counter == loiterNodePoolSize)
                    { break; }
                }

                /*Debug.LogFormat("[Nem] NemesisManager.cs -> SetLoiterNodes: TOP X -> there are {0} loiter nodes", listOfLoiterNodes.Count);*/

                //Check all nodes in list (reverse loop list) to see if they have any neighbours within a set distance. Remove from list if so. Should be at least one node remaining.
                for (int index = listOfLoiterNodes.Count - 1; index >= 0; index--)
                {
                    Node node = listOfLoiterNodes[index];
                    //check against centre node, if any
                    if (centreNode != null)
                    {
                        distance = GameManager.i.dijkstraScript.GetDistanceUnweighted(centreNode.nodeID, node.nodeID);
                        if (distance > -1)
                        {
                            if (distance <= loiterDistanceCheck)
                            {
                                //too close, exclude node

                                /*Debug.LogFormat("[Nem] NemesisManager.cs -> SetLoiterNodes: nodeID {0} removed (too close to Centre nodeID {1}) distance {2}", node.nodeID, centreNode.nodeID, distance);*/

                                listOfLoiterNodes.RemoveAt(index);
                                continue;
                            }
                        }
                        else { Debug.LogWarning("Invalid GetDistanceUnweighted (-1)"); }
                    }
                }
                //Check remaining that loiter nodes aren't too close to each other (reverse loop list) Least connected nodes are checked and deleted before most connected (at list[0])
                for (int index = listOfLoiterNodes.Count - 1; index >= 0; index--)
                {
                    Node node = listOfLoiterNodes[index];
                    //check against all other nodes in list
                    for (int i = 0; i < listOfLoiterNodes.Count; i++)
                    {
                        Node nodeTemp = listOfLoiterNodes[i];
                        //not the same node?
                        if (nodeTemp.nodeID != node.nodeID)
                        {
                            //check distance
                            distance = GameManager.i.dijkstraScript.GetDistanceUnweighted(nodeTemp.nodeID, node.nodeID);
                            if (distance > -1)
                            {
                                if (distance <= loiterDistanceCheck)
                                {
                                    //too close, remove current node from list (make sure at least one node is remaining)
                                    counter = listOfLoiterNodes.Count;
                                    if (centreNode != null)
                                    { counter++; }
                                    //only delete if more than one node remaining
                                    if (counter > 1)
                                    {
                                        /*Debug.LogFormat("[Nem] NemesisManager.cs -> SetLoiterNodes: nodeID {0} removed (too close to nodeID {1}), distance {2}", node.nodeID, nodeTemp.nodeID, distance);*/
                                        listOfLoiterNodes.RemoveAt(index);
                                        break;
                                    }
                                    else
                                    {
                                        /*Debug.Log("[Nem] NemesisManager.cs -> SetLoiterNodes: Last Node NOT Removed");*/
                                        break;
                                    }
                                }
                            }
                            else { Debug.LogWarning("Invalid dijkstra GetDistanceUnweighted (-1)"); }
                        }
                    }
                }
            }
            else { Debug.LogError("Invalid listOfLoiterNodes (Null)"); }
            //add centre node to list, if present
            if (centreNode != null)
            { listOfLoiterNodes.Add(centreNode); }
            //how many remaining
            Debug.LogFormat("[Nem] NemesisManager.cs -> SetLoiterNodes: FINAL -> there are {0} loiter nodes", listOfLoiterNodes.Count);
        }
        else { Debug.LogError("Invalid listOfMostConnectedNodes (Null)"); }
    }

    /// <summary>
    /// Takes loiter nodes and configueres a LoiterData package for each individual node for quick reference by nemesis
    /// </summary>
    private void SetLoiterData()
    {
        int tempNodeID, shortestNodeID, tempDistance, shortestDistance, numOfLoiterNodes, v1, v2;
        int counter = 0;
        List<Node> listOfLoiterNodes = GameManager.i.dataScript.GetListOfLoiterNodes();
        List<Node> listOfAllNodes = GameManager.i.dataScript.GetListOfAllNodes();
        List<Connection> listOfConnections = new List<Connection>();
        Connection connection;
        if (listOfLoiterNodes != null)
        {
            numOfLoiterNodes = listOfLoiterNodes.Count;
            if (numOfLoiterNodes > 0)
            {
                if (listOfAllNodes != null)
                {
                    //loop all nodes
                    for (int index = 0; index < listOfAllNodes.Count; index++)
                    {
                        LoiterData data = new LoiterData();
                        Node node = listOfAllNodes[index];
                        if (node != null)
                        {
                            //check if node is a loiter node
                            if (listOfLoiterNodes.Exists(x => x.nodeID == node.nodeID) == true)
                            {
                                //Loiter node
                                data.nodeID = node.nodeID;
                                data.distance = 0;
                                data.neighbourID = node.nodeID;
                            }
                            else
                            {
                                //NOT a loiter node
                                shortestNodeID = -1;
                                shortestDistance = 999;
                                //check distance to all loiter nodes and get closest
                                for (int i = 0; i < numOfLoiterNodes; i++)
                                {
                                    tempNodeID = listOfLoiterNodes[i].nodeID;
                                    tempDistance = GameManager.i.dijkstraScript.GetDistanceUnweighted(node.nodeID, tempNodeID);
                                    if (tempDistance > -1)
                                    {
                                        if (tempDistance < shortestDistance)
                                        {
                                            shortestDistance = tempDistance;
                                            shortestNodeID = tempNodeID;
                                        }
                                    }
                                    else { Debug.LogWarning("Invalid dijkstra GetDistanceUnweighted (-1)"); }
                                }
                                data.nodeID = shortestNodeID;
                                data.distance = shortestDistance;
                                //get path to nearest loiter node
                                if (shortestNodeID > -1)
                                {
                                    listOfConnections = GameManager.i.dijkstraScript.GetPath(node.nodeID, shortestNodeID, false);
                                    if (listOfConnections != null)
                                    {
                                        //get first connection (from source node)
                                        connection = listOfConnections[0];
                                        if (connection != null)
                                        {
                                            v1 = connection.GetNode1();
                                            v2 = connection.GetNode2();
                                            if (v1 == node.nodeID || v2 == node.nodeID)
                                            {
                                                if (v1 == node.nodeID)
                                                { data.neighbourID = v2; }
                                                else { data.neighbourID = v1; }
                                                //check neighbourID is valid for node
                                                if (node.CheckNeighbourNodeID(data.neighbourID) == false)
                                                {
                                                    Debug.LogWarningFormat("Invalid data.neighbourID (doesn't correspond with node neighbours) for node {0}, {1}, id {2}", node.nodeName, node.Arc.name, node.nodeID);
                                                    data.neighbourID = -1;
                                                }
                                            }
                                            else { Debug.LogWarningFormat("Invalid connection (endpoints don't match nodeID) for node {0}, {1}, id {2}", node.nodeName, node.Arc.name, node.nodeID); }
                                        }
                                        else { Debug.LogWarningFormat("Invalid listOfConnections[0] (Null) for node {0}, {1}, id {2}", node.nodeName, node.Arc.name, node.nodeID); }
                                    }
                                    else { Debug.LogWarningFormat("Invalid listOfConnections (Null) for source nodeID {0} and destination nodeID {1}", node.nodeID, shortestNodeID); }
                                }
                                else { Debug.LogWarningFormat("Invalid shortestNodeID (-1) for node {0}, {1}, id {2}", node.nodeName, node.Arc.name, node.nodeID); }
                            }
                        }
                        else { Debug.LogErrorFormat("Invalid node (Null) in listOfAllNodes[{0}]", index); }
                        //store loiter data
                        if (data.distance > -1 && data.nodeID > -1 && data.neighbourID > -1)
                        {
                            node.loiter = data;
                            counter++;
                            if (data.distance == 0)
                            { node.isLoiterNode = true; }
                            else { node.isLoiterNode = false; }
                        }
                        else { Debug.LogWarningFormat("Invalid loiterData (missing data) for node {0}, {1}, id {2}", node.nodeName, node.Arc.name, node.nodeID); }
                    }
                    Debug.LogFormat("[Nem] NemesisManager.cs -> SetLoiterData: LoiterData successfully initialised in {0} nodes", counter);
                }
                else { Debug.LogError("Invalid listOfAllNodes (Null)"); }
            }
            else { Debug.LogError("Invalid listOfLoiterNodes (Empty)"); }
        }
        else { Debug.LogError("Invalid listOfLoiterNodes (Null)"); }
    }

    //
    // - - - Debug - - -
    //

    /// <summary>
    /// Debug method to display nemesis status
    /// </summary>
    /// <returns></returns>
    public string DebugShowNemesisStatus()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat(" Nemesis{0}{1}", "\n", "\n");
        //tracking info
        builder.AppendFormat(" -Tracking Info{0}", "\n");
        builder.AppendFormat(" targetNodeID: {0}{1}", targetNodeID, "\n");
        if (targetNodeID > -1)
        {
            Node node = GameManager.i.dataScript.GetNode(targetNodeID);
            if (node != null)
            { builder.AppendFormat(" target node: {0}, {1}, id {2}{3}", node.nodeName, node.Arc.name, node.nodeID, "\n"); }
            else { Debug.LogWarningFormat("Invalid target node (Null) for targetNodeID {0}", targetNodeID); }
        }
        builder.AppendFormat(" moveToNodeID: {0}{1}", moveToNodeID, "\n");
        if (moveToNodeID > -1)
        {
            Node node = GameManager.i.dataScript.GetNode(moveToNodeID);
            if (node != null)
            { builder.AppendFormat(" moveTo node: {0}, {1}, id {2}{3}", node.nodeName, node.Arc.name, node.nodeID, "\n"); }
            else { Debug.LogWarningFormat("Invalid moveTo node (Null) for moveToNodeID {0}", moveToNodeID); }
        }
        builder.AppendFormat(" targetDistance: {0}{1}", targetDistance, "\n");
        builder.AppendFormat(" isImmediate: {0}{1}", isImmediate, "\n");
        //current status
        builder.AppendFormat("{0} -Status{1}", "\n", "\n");
        builder.AppendFormat(" mode: {0}{1}", mode, "\n");
        builder.AppendFormat(" goal: {0}{1}", goal, "\n");
        builder.AppendFormat(" durationMode: {0}{1}", durationDelay, "\n");
        builder.AppendFormat(" durationGoal: {0}{1}", durationGoal, "\n");
        if (nemesis != null)
        { builder.AppendFormat(" nemesis node: {0}, {1}, id {2}{3}", nemesisNode.nodeName, nemesisNode.Arc.name, nemesisNode.nodeID, "\n"); }
        //flags
        builder.AppendFormat("{0} -Flags{1}", "\n", "\n");
        builder.AppendFormat(" hasMoved: {0}{1}", hasMoved, "\n");
        builder.AppendFormat(" hasActed: {0}{1}", hasActed, "\n");
        builder.AppendFormat(" hasWarning: {0}{1}", hasWarning, "\n");
        //nemesis stats
        if (nemesis != null)
        {
            builder.AppendFormat("{0} -{1}{2}", "\n", nemesis.name, "\n");
            builder.AppendFormat(" movement: {0}{1}", nemesis.movement, "\n");
            builder.AppendFormat(" search: {0}, adjusted: {1}{2}", nemesis.searchRating, GetSearchRatingAdjusted(), "\n");
            builder.AppendFormat(" stealth: {0}, adjusted: {1}{2}", nemesis.stealthRating, GetStealthRatingAdjusted(), "\n");
            builder.AppendFormat(" damage: {0}{1}", nemesis.damage.name, "\n");
        }
        else { builder.AppendFormat("{0} -No Nemesis present{1}", "\n", "\n"); }
        //AITracker (passed from AIManager)
        builder.AppendFormat("{0} -Tracker Data (AIManager.cs){1}", "\n", "\n");
        if (trackerDebug != null)
        {
            builder.AppendFormat(" activity on Turn {0}{1}", trackerDebug.turn, "\n");
            builder.AppendFormat(" at nodeID {0}{1}", trackerDebug.data0, "\n");
        }
        else
        { builder.AppendFormat(" Nothing to report{0}", "\n"); }
        //Authority Player control
        builder.AppendFormat("{0} -Player Control{1}", "\n", "\n");
        builder.AppendFormat(" isPlayerControl: {0}{1}", isPlayerControl, "\n");
        builder.AppendFormat(" controlNodeID: {0}{1}", controlNodeID, "\n");
        Node nodeControl = GameManager.i.dataScript.GetNode(controlNodeID);
        if (nodeControl != null) { builder.AppendFormat(" controlNode: {0}, {1}, id {2}{3}", nodeControl.nodeName, nodeControl.Arc.name, nodeControl.nodeID, "\n"); }
        builder.AppendFormat(" controlTimer: {0}{1}", controlTimer, "\n");
        builder.AppendFormat(" coolDownTimer: {0}{1}", controlCooldownTimer, "\n");
        builder.AppendFormat(" controlGoal: {0}{1}", controlGoal, "\n");
        return builder.ToString();
    }

    /// <summary>
    /// returns true if Nemesis is active (Normal or Hunt modes), false if Inactive. NOTE: Assumes Nemesis is present (use CheckNemesisPresent to determine this)
    /// </summary>
    /// <returns></returns>
    public bool CheckNemesisActive()
    {
        if (mode != NemesisMode.Inactive) { return true; }
        return false;
    }

    /// <summary>
    /// returns true if a Nemesis is present, false if not
    /// </summary>
    /// <returns></returns>
    public bool CheckNemesisPresent()
    { return nemesis != null ? true : false; }

    /// <summary>
    /// returns true if Nemesis currently has an Ambush goal, false otherwise
    /// </summary>
    /// <returns></returns>
    public bool CheckNemesisAmbush()
    {
        if (goal == NemesisGoal.AMBUSH) { return true; }
        return false;
    }

    /// <summary>
    /// returns true if Nemesis currently has a Search goal, false otherwise
    /// </summary>
    /// <returns></returns>
    public bool CheckNemesisSearch()
    {
        if (goal == NemesisGoal.SEARCH) { return true; }
        return false;
    }

    public NemesisMode GetMode()
    { return mode; }

    /// <summary>
    /// returns normal nemesis goal or Player controlled goal
    /// </summary>
    /// <param name="isControl"></param>
    /// <returns></returns>
    public NemesisGoal GetGoal(bool isControl = false)
    {
        if (isControl == false) { return goal; }
        return controlGoal;
    }

    /// <summary>
    /// returns duration, in turns, of nemesis being inactive or in hunt mode
    /// </summary>
    /// <returns></returns>
    public int GetDurationMode()
    { return durationDelay; }

    //
    // - - - Save / Load
    //

    /// <summary>
    /// populates Save data class and returns the same
    /// </summary>
    /// <returns></returns>
    public NemesisSaveClass WriteSaveData()
    {
        NemesisSaveClass writeData = new NemesisSaveClass();
        //copy data over
        writeData.hasMoved = hasMoved;
        writeData.hasActed = hasActed;
        writeData.hasWarning = hasWarning;
        writeData.isFirstNemesis = isFirstNemesis;
        writeData.mode = mode;
        writeData.goal = goal;
        writeData.durationGoal = durationGoal;
        writeData.durationDelay = durationDelay;
        writeData.nemesisNodeID = nemesisNode.nodeID;
        writeData.trackerDebug = trackerDebug;
        writeData.targetNodeID = targetNodeID;
        writeData.moveToNodeID = moveToNodeID;
        writeData.targetDistance = targetDistance;
        writeData.isImmediate = isImmediate;
        writeData.isPlayerControl = isPlayerControl;
        writeData.controlNodeID = controlNodeID;
        writeData.controlTimer = controlTimer;
        writeData.controlCooldownTimer = controlCooldownTimer;
        writeData.controlGoal = controlGoal;
        writeData.resistancePlayer = resistancePlayer;
        return writeData;
    }

    /// <summary>
    /// update nemesisManager.cs with loaded save file data
    /// </summary>
    /// <param name="readData"></param>
    public void ReadSaveData(NemesisSaveClass readData)
    {
        if (readData != null)
        {
            //copy data into NemesisManager.cs
            hasMoved = readData.hasMoved;
            hasActed = readData.hasActed;
            hasWarning = readData.hasWarning;
            isFirstNemesis = readData.isFirstNemesis;
            mode = readData.mode;
            goal = readData.goal;
            durationGoal = readData.durationGoal;
            durationDelay = readData.durationDelay;
            nemesisNode = GameManager.i.dataScript.GetNode(readData.nemesisNodeID);
            Debug.Assert(nemesisNode != null, "Invalid nemesisNode (Null)");
            trackerDebug = readData.trackerDebug;
            targetNodeID = readData.targetNodeID;
            moveToNodeID = readData.moveToNodeID;
            targetDistance = readData.targetDistance;
            isImmediate = readData.isImmediate;
            isPlayerControl = readData.isPlayerControl;
            controlNodeID = readData.controlNodeID;
            controlTimer = readData.controlTimer;
            controlCooldownTimer = readData.controlCooldownTimer;
            controlGoal = readData.controlGoal;
            resistancePlayer = readData.resistancePlayer;
        }
        else { Debug.LogError("Invalid NemesisSaveClass (Null)"); }
    }



    //new methods above here
}
