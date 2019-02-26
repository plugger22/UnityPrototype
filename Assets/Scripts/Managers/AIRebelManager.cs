using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using System;
using System.Linq;
using System.Text;
using modalAPI;
using Random = UnityEngine.Random;
using packageAPI;

/// <summary>
/// All sighting reports are converted to Sighting data objects which have an adjusted priority base on their relevance (time of sighting / effectiveness of contact)
/// </summary>
public class SightingData
{
    public int nodeID;
    public int turn;                //used for tie breakers where priority is equal, most recent is taken
    public int moveNumber;          //can be ignored, default 0, case of nemesis making multiple moves within a single turn, used for tiebreakers when both priority and turn are equal
    public Priority priority;
}


/// <summary>
/// handles all Resistance AI
/// </summary>
public class AIRebelManager : MonoBehaviour
{
    [Header("Actions")]
    [Tooltip("Base amount of actions per turn for the AI Resistance Player")]
    [Range(1, 3)] public int actionsBase = 2;

    [Header("Sightings")]
    [Tooltip("Delete sighting reports (Nemesis, Erasure teams, etc) older than ('>') this number of turns ago")]
    [Range(1, 5)] public int deleteOlderThan = 3;

    [Header("Lying Low")]
    [Tooltip("The threshold of invisibility below which (less than) there is a chance of the AI player selecting a Lie Low task")]
    [Range(1,3)] public int lieLowThresholdPlayer = 2;
    [Tooltip("The threshold of invisibility below which (less than) there is a chance of the AI actor (minion) selecting a Lie Low task. Note chance is lieLowChanceActor and only if no Player Survival task")]
    [Range(1, 3)] public int lieLowThresholdActor = 1;
    [Tooltip("The % chance of AI player/actor selecting a Lie Low task in an Emergency situation")]
    [Range(0, 100)] public int lieLowEmergency = 75;
    [Tooltip("The modifier (increases) to chance of Player lying low if already stressed")]
    [Range(0, 100)] public int lieLowStressed = 25;
    [Tooltip("The % chance of AI player/actor selecting a Lie Low task when their invisibility is at 2")]
    [Range(0, 100)] public int lieLowTwo = 10;
    [Tooltip("The % chance of AI player/actor selecting a Lie Low task when their invisibility is at 1")]
    [Range(0, 100)] public int lieLowOne = 20;
    [Tooltip("The % chance of AI player/actor selecting a Lie Low task when their invisibility is at 0")]
    [Range(0, 100)] public int lieLowZero = 40;
    [Tooltip("All purpose % chance of an AI actor lying low provided it meets the criteria. Checked in ProcessSurvivalTask and only if nothing applies to the AI Player, eg. actors checked last")]
    [Range(0, 100)] public int lieLowChanceActor = 60;

    [Header("Stress Leave")]
    [Tooltip("Cost in resources/renown for the player or actor to take Stress Leave")]
    [Range(0, 10)] public int stressLeaveCost = 2;

    [Header("Gear Use")]
    [Tooltip("Chance of a suitable piece of gear being available for a task when there are SIX gear points in the gear pool")]
    [Range(0, 100)] public int gearAvailableSix = 70;
    [Tooltip("Chance of a suitable piece of gear being available for a task when there are FIVE gear points in the gear pool")]
    [Range(0, 100)] public int gearAvailableFive = 60;
    [Tooltip("Chance of a suitable piece of gear being available for a task when there are FOUR gear points in the gear pool")]
    [Range(0, 100)] public int gearAvailableFour = 50;
    [Tooltip("Chance of a suitable piece of gear being available for a task when there are THREE gear points in the gear pool")]
    [Range(0, 100)] public int gearAvailableThree = 40;
    [Tooltip("Chance of a suitable piece of gear being available for a task when there are TWO gear points in the gear pool")]
    [Range(0, 100)] public int gearAvailableTwo = 30;
    [Tooltip("Chance of a suitable piece of gear being available for a task when there is ONE gear points in the gear pool")]
    [Range(0, 100)] public int gearAvailableOne = 20;
    [Tooltip("Maxium number of gear points that can be in the gear pool. Change this and you need to refactor the code")]
    [Range(6, 6)] public int gearPoolMaxSize = 6;
    [Tooltip("Threshold at which below the Fixer action will trigger")]
    [Range(0, 10)] public int gearPoolThreshold = 3;
    [Tooltip("If a Fixer action occurs this is how many extra gear Points will be generated")]
    [Range(0, 10)] public int gearPoolTopUp = 2;
    [Tooltip("The % chance of Renown/Resource point being used to retain gear (random roll every time gear is used. Cannot send resources below zero")]
    [Range(0, 100)] public int gearRenownChance = 20;

    [Header("Target Intel")]
    [Tooltip("The maximum amount of target Intel that can be held. You can go beyond this but no Planner action will be generated to gain more, will only do so if targetIntel < targetIntelMax")]
    [Range(0, 10)] public int targetIntelMax = 6;
    [Tooltip("Amount of targetIntel gained from a single Planner Action (chance of an extra intel point beyond this)")]
    [Range(0, 5)] public int targetIntelTopUp = 1;
    [Tooltip("Chance of gaining an extra point of target intel whenever a Planner action is used (normally you gain +1 intel per action but chance of +2)")]
    [Range(0, 100)] public int targetIntelExtra = 40;
    [Tooltip("Max amount of intel to be used on any given Target attempt (if it's present)")]
    [Range(0, 5)] public int targetIntelAttempt = 3;

    [Header("Target Attempts")]


    //AI Resistance Player
    [HideInInspector] public ActorStatus status;
    [HideInInspector] public ActorInactive inactiveStatus;
    [HideInInspector] public bool isBreakdown;          //true if suffering from nervous, stress induced, breakdown

   
    private int actionAllowance;                        //number of actions per turn (normal allowance + extras)
    private int actionsExtra;                           //bonus actions for this turn
    private int actionsUsed;                            //tally of actions used this turn

    private int gearPool;                               //number of gear points in pool
    private int gearPointsUsed;                         //number of gear points expended by AI

    private int targetIntel;                            //number of target intel points (gained by Planner and used for target attempts)

    private int targetNodeID;                           //goal to move towards
    private int aiPlayerStartNodeID;                    //reference only, node AI Player commences at

    private bool isConnectionsChanged;                  //if true connections have been changed due to sighting data and need to be restore once all calculations are done
    private bool isPlayer;                              //if true the Resistance side is also the human player side (it's AI due to an autorun)
    private bool isWounded;
    private bool isPlayerStressed;                        //true only if player is stressed
    private int stressedActorID;                          //player or actorID that is chosen to have go on stress leave (player has priority)

    //rebel Player profile
    private int survivalMove;                           //The % chance of AI player moving away when they are at a Bad Node
    private int playerAction;                           //The % chance of the player doing the ActorArc action, rather than an Actor
    private int targetAttemptMinOdds;                   //The minimum odds that are required for the Player/Actor to attempt a target (Note: RebelLeader % converted to 0 to 10 range for ease of target tally calc)
    private Priority priorityStressLeavePlayer;
    private Priority priorityStressLeaveActor;
    private Priority priorityMovePlayer;
    private Priority priorityIdlePlayer;
    private Priority priorityFixerTask;
    private Priority priorityAnarchistTask;
    private Priority priorityBloggerTask;
    private Priority priorityHackerTask;
    private Priority priorityHeavyTask;
    private Priority priorityObserverTask;
    private Priority priorityOperatorTask;
    private Priority priorityPlannerTask;
    private Priority priorityRecruiterTask;
    private Priority priorityTargetPlayer;
    private Priority priorityTargetActor;


    //fast access
    private string playerName;
    private string playerTag;                           //nickname
    private string playerBackground;
    private GlobalSide globalResistance;
    private int numOfNodes = -1;
    private int playerID = -1;
    private int priorityHigh = -1;
    private int priorityMedium = -1;
    private int priorityLow = -1;
    private int maxStatValue = -1;
    private int maxNumOfOnMapActors = -1;
    private AuthoritySecurityState security;            //updated each turn in UpdateAdmin
    //conditions
    private Condition conditionStressed;
    private Condition conditionWounded;
    //tests
    private int turnForStress;
    //Activity notifications
    private int delayNoSpider = -1;
    private int delayYesSpider = -1;
    //actor arcs
    private ActorArc arcFixer;

    //Authority activity
    private List<AITracker> listOfNemesisReports = new List<AITracker>();
    private List<AITracker> listOfErasureReports = new List<AITracker>();
    private List<SightingData> listOfNemesisSightData = new List<SightingData>();
    private List<SightingData> listOfErasureSightData = new List<SightingData>();
    private List<int> listOfBadNodes = new List<int>();                             //list of nodes to avoid for this turn, eg. nemesis or erasure team present (based on known info)

    //Node and other Action data gathering
    private List<ActorArc> listOfArcs = new List<ActorArc>();                       //current actor arcs valid for this turn
    private List<Node>[] arrayOfActorActions;                                       //list of nodes suitable for listOfArc[index] action
    private List<Actor> listOfCurrentActors = new List<Actor>();                    //list of current onMap, Active, actors at start of each action
    

    //tasks
    List<AITask> listOfTasksPotential = new List<AITask>();
    List<AITask> listOfTasksCritical = new List<AITask>();

    //targets
    private Dictionary<Target, int> dictOfSortedTargets = new Dictionary<Target, int>();   //key -> target, Value -> Distance (weighted and adjusted for threats)


    public void Initialise()
    {
        //set initial move node to start position (will trigger a new targetNodeID)
        targetNodeID = GameManager.instance.nodeScript.nodePlayer;
        aiPlayerStartNodeID = GameManager.instance.nodeScript.nodePlayer;
        status = ActorStatus.Active;
        inactiveStatus = ActorInactive.None;
        GameManager.instance.playerScript.Invisibility = 3;
        //fast access
        numOfNodes = GameManager.instance.dataScript.CheckNumOfNodes();
        playerID = GameManager.instance.playerScript.actorID;
        globalResistance = GameManager.instance.globalScript.sideResistance;
        conditionStressed = GameManager.instance.dataScript.GetCondition("STRESSED");
        conditionWounded = GameManager.instance.dataScript.GetCondition("WOUNDED");
        priorityHigh = GameManager.instance.aiScript.priorityHighWeight;
        priorityMedium = GameManager.instance.aiScript.priorityMediumWeight;
        priorityLow = GameManager.instance.aiScript.priorityLowWeight;
        turnForStress = GameManager.instance.testScript.stressTurnResistance;
        maxStatValue = GameManager.instance.actorScript.maxStatValue;
        maxNumOfOnMapActors = GameManager.instance.actorScript.maxNumOfOnMapActors;
        delayNoSpider = GameManager.instance.nodeScript.nodeNoSpiderDelay;
        delayYesSpider = GameManager.instance.nodeScript.nodeYesSpiderDelay;
        arcFixer = GameManager.instance.dataScript.GetActorArc("FIXER");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(numOfNodes > -1, "Invalid numOfNodes (-1)");
        Debug.Assert(playerID > -1, "Invalid playerId (-1)");
        Debug.Assert(priorityHigh > -1, "Invalid priorityHigh (-1)");
        Debug.Assert(priorityMedium > -1, "Invalid priorityMedium (-1)");
        Debug.Assert(priorityLow > -1, "Invalid priorityLow (-1)");
        Debug.Assert(conditionStressed != null, "Invalid conditionStressed (Null)");
        Debug.Assert(conditionWounded != null, "Invalid conditionWounded (Null)");
        Debug.Assert(maxStatValue > -1, "Invalid maxStatValue (-1)");
        Debug.Assert(maxNumOfOnMapActors > -1, "Invalid maxNumOfOnMapActors (-1)");
        Debug.Assert(delayNoSpider > -1, "Invalid delayNoSpider (-1)");
        Debug.Assert(delayYesSpider > -1, "Invalid delayYesSpider (-1)");
        Debug.Assert(arcFixer != null, "Invalid arcFixer (Null)");

        //collections
        arrayOfActorActions = new List<Node>[maxNumOfOnMapActors];
        for (int i = 0; i < arrayOfActorActions.Length; i++)
        { arrayOfActorActions[i] = new List<Node>(); }
        //player (human / AI revert to human)
        playerName = GameManager.instance.playerScript.GetPlayerNameResistance();
        playerTag = GameManager.instance.scenarioScript.scenario.leaderResistance.tag;
        playerBackground = GameManager.instance.scenarioScript.scenario.descriptorResistance;
        if (GameManager.instance.sideScript.PlayerSide.level != globalResistance.level) { isPlayer = false; }
        else
        { isPlayer = true; }
        //Rebel leader
        survivalMove = GameManager.instance.scenarioScript.scenario.leaderResistance.moveChance;
        playerAction = GameManager.instance.scenarioScript.scenario.leaderResistance.playerChance;
        targetAttemptMinOdds = GameManager.instance.scenarioScript.scenario.leaderResistance.targetAttemptMinOdds / 10;
        gearPool = GameManager.instance.scenarioScript.scenario.leaderResistance.gearPoints;
        gearPool = Mathf.Clamp(gearPool, 0, gearPoolMaxSize);
        priorityStressLeavePlayer = GetPriority(GameManager.instance.scenarioScript.scenario.leaderResistance.stressLeavePlayer);
        priorityStressLeaveActor = GetPriority(GameManager.instance.scenarioScript.scenario.leaderResistance.stressLeaveActor);
        priorityMovePlayer = GetPriority(GameManager.instance.scenarioScript.scenario.leaderResistance.movePriority);
        priorityIdlePlayer = GetPriority(GameManager.instance.scenarioScript.scenario.leaderResistance.idlePriority);
        priorityAnarchistTask = GetPriority(GameManager.instance.scenarioScript.scenario.leaderResistance.taskAnarchist);
        priorityBloggerTask = GetPriority(GameManager.instance.scenarioScript.scenario.leaderResistance.taskBlogger);
        priorityFixerTask = GetPriority(GameManager.instance.scenarioScript.scenario.leaderResistance.taskFixer);
        priorityHackerTask = GetPriority(GameManager.instance.scenarioScript.scenario.leaderResistance.taskHacker);
        priorityHeavyTask = GetPriority(GameManager.instance.scenarioScript.scenario.leaderResistance.taskHeavy);
        priorityObserverTask = GetPriority(GameManager.instance.scenarioScript.scenario.leaderResistance.taskObserver);
        priorityOperatorTask = GetPriority(GameManager.instance.scenarioScript.scenario.leaderResistance.taskOperator);
        priorityPlannerTask = GetPriority(GameManager.instance.scenarioScript.scenario.leaderResistance.taskPlanner);
        priorityRecruiterTask = GetPriority(GameManager.instance.scenarioScript.scenario.leaderResistance.taskRecruiter);
        priorityTargetPlayer = GetPriority(GameManager.instance.scenarioScript.scenario.leaderResistance.targetPlayer);
        priorityTargetActor = GetPriority(GameManager.instance.scenarioScript.scenario.leaderResistance.targetActor);
        Debug.Assert(priorityStressLeavePlayer != Priority.None, "Invalid priorityStressLeavePlayer (None)");
        Debug.Assert(priorityStressLeaveActor != Priority.None, "Invalid priorityStressLeaveActor (None)");
        Debug.Assert(priorityMovePlayer != Priority.None, "Invalid priorityMovePlayer (None)");
        Debug.Assert(priorityIdlePlayer != Priority.None, "Invalid priorityIdlePlayer (None)");
        Debug.Assert(priorityAnarchistTask != Priority.None, "Invalid priorityAnarchistTask (None)");
        Debug.Assert(priorityBloggerTask != Priority.None, "Invalid priorityBloggerTask (None)");
        Debug.Assert(priorityFixerTask != Priority.None, "Invalid priorityFixerTask (None)");
        Debug.Assert(priorityHackerTask != Priority.None, "Invalid priorityHackerTask (None)");
        Debug.Assert(priorityHeavyTask != Priority.None, "Invalid priorityHeavyTask (None)");
        Debug.Assert(priorityObserverTask != Priority.None, "Invalid priorityObserverTask (None)");
        Debug.Assert(priorityOperatorTask != Priority.None, "Invalid priorityOperatorTask (None)");
        Debug.Assert(priorityPlannerTask != Priority.None, "Invalid priorityPlannerTask (None)");
        Debug.Assert(priorityRecruiterTask != Priority.None, "Invalid priorityRecruiterTask (None)");
        Debug.Assert(targetAttemptMinOdds > 0, "Invalid targetAttemptMinOdds (Zero or less)");
    }

    /// <summary>
    /// returns a Priority enum from a GlobalChance SO enum. Returns priority.None if a problem
    /// NOTE: both GlobalChance High and Critical return Priorty.High
    /// </summary>
    /// <param name="chance"></param>
    /// <returns></returns>
    private Priority GetPriority(GlobalChance chance)
    {
        Priority priority = Priority.None;
        if (chance != null)
        {
            switch(chance.level)
            {
                case 3:
                case 2:
                    priority = Priority.High;
                    break;
                case 1:
                    priority = Priority.Medium;
                    break;
                case 0:
                    priority = Priority.Low;
                    break;
                default:
                    Debug.LogWarningFormat("Unrecognised chance.level {0}", chance.level);
                    break;
            }
        }
        else { Debug.LogError("Invalid globalChance (Null)"); }
        return priority;
    }

    /// <summary>
    /// main controlling method to run Resistance AI each turn, called from AIManager.cs -> ProcessAISideResistance
    /// </summary>
    public void ProcessAI()
    {
        isConnectionsChanged = false;
        //debugging
        DebugTest();
        //AI player ACTIVE
        if (status == ActorStatus.Active)
        {
            ClearAICollectionsEarly();
            UpdateAdmin();
            //Info gathering
            ProcessSightingData();
            ProcessTargetData();
            ProcessPeopleData();
            ProcessActorArcData();
            //restore back to original state after any changes & prior to any moves, tasks, etc. Calcs will still use updated sighting data dijkstra (weighted)
            if (isConnectionsChanged == true)
            { RestoreConnections(); }
            //
            // - - - task loop (once per available action)
            //
            int counter = 0;
            do
            {
                Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessAI: commence action {0} of {1} - - -{2}", actionsUsed + 1, actionAllowance, "\n");
                ClearAICollectionsLate();
                //task creation
                if (actionsUsed == 0)
                { ProcessSurvivalTask(); }
                //only one task possible and if survival task has been generated no point in going further
                if (listOfTasksCritical.Count == 0)
                {
                    /*ProcessAdminTask();*/
                    ProcessTargetTask();
                    ProcessMoveTask();
                    ProcessPeopleTask();
                    ProcessActorArcTask();
                    ProcessIdleTask();
                }
                //task Execution
                ProcessTaskFinal();
                counter++;
                if (counter > 3)
                {
                    Debug.LogWarning("Break triggered on counter value, shouldn't have happened");
                    break;
                }
            }
            while (actionsUsed < actionAllowance && status == ActorStatus.Active);
        }
        else
        {
            //AI Player NOT Active
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessAI: Rebel AI Suspended as AI Player not active{0}", "\n");
            switch (status)
            {
                case ActorStatus.Inactive:
                    switch (inactiveStatus)
                    {
                        case ActorInactive.LieLow:
                            Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessAI: AI Player Lying Low{0}", "\n");
                            break;
                        case ActorInactive.Breakdown:
                            Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessAI: AI Player suffering a stress related Breakdown{0}", "\n");
                            break;
                    }
                    break;
                case ActorStatus.Captured:
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessAI: AI Player Captured{0}", "\n");
                    break;
            }
        }
        //recalculate Dijkstra weighted data once done (set back to original state)
        if (isConnectionsChanged == true)
        { RestoreDijkstraCalculations(); }
    }


    /// <summary>
    /// reset all data prior to AI turn processing
    /// </summary>
    private void ClearAICollectionsEarly()
    {
        dictOfSortedTargets.Clear();
        listOfTasksPotential.Clear();
        listOfTasksCritical.Clear();
        listOfNemesisSightData.Clear();
        listOfErasureSightData.Clear();
        listOfBadNodes.Clear();
        listOfArcs.Clear();
        //
        // - - - Sighting Reports
        //
        int threshold = GameManager.instance.turnScript.Turn - 1;
        //NEMESIS reports entries that are older than 'x' turns ago
        int count = listOfNemesisReports.Count;
        if (count > 0)
        {
            //only bother if threshold has kicked in
            if (threshold > 0)
            {
                //reverse loop as deleting
                for (int i = count - 1; i >= 0; i--)
                {
                    AITracker tracker = listOfNemesisReports[i];
                    if (tracker != null)
                    {
                        //delete older entries
                        if (tracker.turn < threshold)
                        {
                            /*Debug.LogFormat("[Tst] AIRebelManager.cs -> ClearAICollectionsEarly: DELETED Nemesis tracker t: {0} < threshold {1}, nodeID {2}, effect {3}{4}", 
                                tracker.turn, threshold, tracker.data0, tracker.data1, "\n");*/
                            listOfNemesisReports.RemoveAt(i);
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid tracker (Null) for listOfNemesisReports[{0}]", i); }
                }
            }
        }
        //ERASURE reports entries that are older than last turn
        threshold = GameManager.instance.turnScript.Turn;
        count = listOfErasureReports.Count;
        if (count > 0)
        {
            //only bother if threshold has kicked in
            if (threshold > 0)
            {
                //reverse loop as deleting
                for (int i = count - 1; i >= 0; i--)
                {
                    AITracker tracker = listOfErasureReports[i];
                    if (tracker != null)
                    {
                        //delete older entries
                        if (tracker.turn < threshold)
                        {
                            /*Debug.LogFormat("[Tst] AIRebelManager.cs -> ClearAICollectionsEarly: DELETED Erasure tracker t: {0} < threshold {1}, nodeID {2}, effect {3}{4}",
                                tracker.turn, threshold, tracker.data0, tracker.data1, "\n");*/
                            listOfErasureReports.RemoveAt(i);
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid tracker (Null) for listOfErasureReports[{0}]", i); }
                }
            }
        }
    }

    /// <summary>
    /// resets collections between actions (within a turn)
    /// </summary>
    private void ClearAICollectionsLate()
    {
        listOfTasksPotential.Clear();
        listOfTasksCritical.Clear();
        //actor actions
        for (int i = 0; i < arrayOfActorActions.Length; i++)
        {
            List<Node> tempList = arrayOfActorActions[i];
            if (tempList != null)
            { tempList.Clear(); }
            else { Debug.LogErrorFormat("Invalid tempList <Node> (Null) in arrayOfActorActions[\"{0}\"]", i); }
        }
        listOfCurrentActors.Clear();
    }

    /// <summary>
    /// start of AI Player turn admin
    /// </summary>
    private void UpdateAdmin()
    {
        //actions
        actionAllowance = actionsBase + actionsExtra;
        actionsUsed = 0;
        //security state
        security = GameManager.instance.turnScript.authoritySecurityState;
        //renown (equivalent to resources for AI Rebel player)
        ProcessResources();
        //conditions
        ProcessConditions();
    }


    //
    // - - - Gather Data - - -
    //

    /// <summary>
    /// Extracts all relevant AI data from an Contact message
    /// NOTE: message checked for Null by calling method
    /// </summary>
    /// <param name="message"></param>
    public void GetAIRebelMessageData(Message message)
    {
        if (message.type == MessageType.CONTACT)
        {
            switch (message.subType)
            {
                case MessageSubType.Contact_Nemesis_Spotted:
                    //Nemesis detected by Resistance Contact, reliability of sighting dependant on contact effectiveness
                    Debug.Assert(message.data1 > -1 && message.data1 < numOfNodes, string.Format("Invalid nodeID {0} (less than Zero or >= numOfNodes)", message.data1));
                    //add to Nemesis list
                    AITracker trackerContact = new AITracker(message.data1, message.turnCreated);
                    Contact contact = GameManager.instance.dataScript.GetContact(message.data2);
                    if (contact != null)
                    { trackerContact.data1 = contact.effectiveness; }
                    else { Debug.LogErrorFormat("Invalid contact (Null) for contactID {0}", message.data2); }
                    //moveNumber
                    trackerContact.data2 = message.data3;
                    //get contact effectivenss
                    listOfNemesisReports.Add(trackerContact);
                    break;
                case MessageSubType.Tracer_Nemesis_Spotted:
                    //Nemesis detected by Tracer
                    Debug.Assert(message.data0 > -1 && message.data0 < numOfNodes, string.Format("Invalid nodeID {0} (less than Zero or >= numOfNodes)", message.data0));
                    //add to Nemesis list
                    AITracker trackerTracer = new AITracker(message.data0, message.turnCreated);
                    //tracer effectiveness automatically 100 %
                    trackerTracer.data1 = 3;
                    listOfNemesisReports.Add(trackerTracer);
                    break;
                case MessageSubType.Contact_Team_Spotted:
                    //Contact detects an Erasure team
                    Debug.Assert(message.data1 > -1 && message.data1 < numOfNodes, string.Format("Invalid nodeID {0} (less than Zero or >= numOfNodes)", message.data1));
                    //add to Erasure Team list
                    AITracker trackerContactTeam = new AITracker(message.data1, message.turnCreated);
                    Contact contactTeam = GameManager.instance.dataScript.GetContact(message.data2);
                    if (contactTeam != null)
                    { trackerContactTeam.data1 = contactTeam.effectiveness; }
                    else { Debug.LogErrorFormat("Invalid contact (Null) for contactID {0}", message.data2); }
                    //get contact effectivenss
                    listOfErasureReports.Add(trackerContactTeam);
                    break;
                case MessageSubType.Tracer_Team_Spotted:
                    //Contact detacts an erasure team
                    Debug.Assert(message.data0 > -1 && message.data0 < numOfNodes, string.Format("Invalid nodeID {0} (less than Zero or >= numOfNodes)", message.data0));
                    //add to Erasure Team list
                    AITracker trackerTracerTeam = new AITracker(message.data0, message.turnCreated);
                    trackerTracerTeam.data1 = 3;
                    listOfErasureReports.Add(trackerTracerTeam);
                    break;
            }
        }
        else { Debug.LogWarning(string.Format("Invalid (not Nemesis) message type \"{0}\" for \"{1}\"", message.type, message.text)); }
    }


    /// <summary>
    /// deals with any conditions that AI player/actors may have -> done right at start prior to assessing and deciding on a course of action
    /// </summary>
    private void ProcessConditions()
    {
        //reset all condition flags
        isPlayerStressed = false;
        isWounded = false;
        stressedActorID = -1;
        //check for conditions
        List<Condition> listOfConditions = GameManager.instance.playerScript.GetListOfConditions(globalResistance);
        if (listOfConditions != null)
        {
            int count = listOfConditions.Count;
            if (count > 0)
            {
                foreach (Condition condition in listOfConditions)
                {
                    if (condition != null)
                    {
                        switch (condition.name)
                        {
                            case "BLACKMAILER":
                            case "CORRUPT":
                            case "INCOMPETENT":
                            case "QUESTIONABLE":
                            case "STAR":
                            case "UNHAPPY":
                            case "TAGGED":
                            case "IMAGED":
                                break;
                            case "DOOMED":

                                break;
                            case "STRESSED":
                                //Player has priority for stress leave
                                isPlayerStressed = true;
                                break;
                            case "WOUNDED":
                                isWounded = true;
                                if (actionAllowance > 1)
                                {
                                    //Restricts actions
                                    if (GameManager.instance.playerScript.CheckConditionPresent(conditionWounded, globalResistance) == true)
                                    {
                                        actionAllowance = 1;
                                        Debug.LogFormat("[Rim] AIRebelManager.cs -> UpdateAdmin: Rebel AI Player WOUNDED. Maximum one action{0}", "\n");
                                    }
                                }
                                break;
                            default:
                                Debug.LogWarningFormat("Unrecognised Condition \"{0}\"", condition.name);
                                break;
                        }
                    }
                    else { Debug.LogError("Invalid condition (Null) in listOfConditions"); }
                }
            }
        }
        else { Debug.LogError("Invalid list of Conditions (Null)"); }
    }

    /// <summary>
    /// Take all sighting reports and convert to sorted, prioritised, Sighting Data ready for analysis. Update connections
    /// </summary>
    private void ProcessSightingData()
    {
        int count;
        //
        // - - - Nemesis Reports - - - 
        //
        count = listOfNemesisReports.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                AITracker tracker = listOfNemesisReports[i];
                if (tracker != null)
                {
                    //convert tracker data to SightingData
                    SightingData sighting = ConvertTrackerToSighting(tracker);
                    //add to list
                    if (sighting != null)
                    {
                        listOfNemesisSightData.Add(sighting);
                        /*Debug.LogFormat("[Tst] AIRebelManager.cs -> ProcessSightingData: listOfNemesisSightData.Add( nodeID {0} priority \"{1}\"){2}", sighting.nodeID, sighting.priority, "\n");*/
                    }
                    else { Debug.LogWarningFormat("Invalid sightingData (Null) for tracker d0: {0}, d1: {1}, turn {2}", tracker.data0, tracker.data1, tracker.turn); }
                }
                else { Debug.LogErrorFormat("Invalid tracker (Null) for listOfNemesisReports[{0}]", i); }
            }
            //sort list
            var sortedList = listOfNemesisSightData.OrderByDescending(obj => obj.priority);
            listOfNemesisSightData = sortedList.ToList();
        }
        //
        // - - - Erasure team Reports - - - 
        //
        count = listOfErasureReports.Count;
        if (count > 0)
        {
            //NOTE: no need to sort list at end as all entries are used (list is cleared each turn so only the most current entries are used)
            for (int i = 0; i < count; i++)
            {
                AITracker tracker = listOfErasureReports[i];
                if (tracker != null)
                {
                    //convert tracker data to SightingData
                    SightingData sighting = ConvertTrackerToSighting(tracker);
                    //add to list
                    if (sighting != null)
                    {
                        listOfErasureSightData.Add(sighting);
                        /*Debug.LogFormat("[Tst] AIRebelManager.cs -> ProcessSightingData: listOfErasureSightData.Add( nodeID {0} priority \"{1}\"){2}", sighting.nodeID, sighting.priority, "\n");*/
                    }
                    else { Debug.LogWarningFormat("Invalid sightingData (Null) for tracker d0: {0},  d1 {1}, turn {2}", tracker.data0, tracker.data1, tracker.turn); }
                }
                else { Debug.LogErrorFormat("Invalid tracker (Null) for listOfErasureReports[{0}]", i); }
            }
        }
        //
        // - - - Update Connections based on highest priority Nemesis Sighting
        //
        SightingData sightingNemesis = null;
        //change connections to reflect sighting data (set 'isConnectionsChanged' to 'true')
        count = listOfNemesisSightData.Count;
        if (count > 0)
        {
            isConnectionsChanged = true;
            //get nodeID of highest priority sighting (top of sorted list)
            if (count > 1)
            {
                //check for situation where two equal priority sightings at top of list, take the highest turn number first then highest moveNumber (nemeis may have moved twice in the same turn)
                if (listOfNemesisSightData[0].priority == listOfNemesisSightData[1].priority)
                {
                    if (listOfNemesisSightData[0].turn > listOfNemesisSightData[1].turn)
                    { sightingNemesis = listOfNemesisSightData[0]; }
                    else if (listOfNemesisSightData[0].turn < listOfNemesisSightData[1].turn)
                    { sightingNemesis = listOfNemesisSightData[1]; }
                    else
                    {
                        //Tiebreaker -> take the highest move Number as the latest sighting report
                        if (listOfNemesisSightData[0].moveNumber < listOfNemesisSightData[1].moveNumber)
                        { sightingNemesis = listOfNemesisSightData[1]; }
                        else { sightingNemesis = listOfNemesisSightData[0]; }
                    }
                }
                else { sightingNemesis = listOfNemesisSightData[0]; }
            }
            else { sightingNemesis = listOfNemesisSightData[0]; }
        }
        //
        // - - - Update Connections based on highest priority Erasure Team Sighting
        //
        //change connections to reflect sighting data (set 'isConnectionsChanged' to 'true')
        if (listOfErasureSightData.Count > 0)
        { isConnectionsChanged = true; }
        //
        // - - - Recalculate Dijkstra weights
        //
        if (isConnectionsChanged == true)
        {
            //save connection state prior to changing
            GameManager.instance.connScript.SaveConnections();
            //change connections based on selected NEMESIS sighting report
            if (sightingNemesis != null)
            {
                Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessSightingData: sightingNemesis nodeID {0}, priority {1}, moveNumber {2}{3}", sightingNemesis.nodeID,
                    sightingNemesis.priority, sightingNemesis.moveNumber, "\n");
                UpdateNodeConnectionSecurity(sightingNemesis);
                //add to listOfBadNodes
                AddNodeToListOfBadNodes(sightingNemesis.nodeID);
            }
            //change connections based on selected ERASURE TEAM sighting report
            for (int i = 0; i < listOfErasureSightData.Count; i++)
            {
                SightingData sightingErasure = listOfErasureSightData[i];
                if (sightingErasure != null)
                {
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessSightingData: sightingErasure nodeID {0}, priority {1}, moveNumber {2}{3}", sightingErasure.nodeID,
                        sightingErasure.priority, sightingErasure.moveNumber, "\n");
                    UpdateNodeConnectionSecurity(sightingErasure);
                    //add to listOfBadNodes
                    AddNodeToListOfBadNodes(sightingErasure.nodeID);
                    //if Security Alert add all neighbouring nodes
                    if (security == AuthoritySecurityState.SecurityAlert)
                    {
                        //all neighbouring nodes of erasure team sighting are added to the listOfBadNodes
                        Node node = GameManager.instance.dataScript.GetNode(sightingErasure.nodeID);
                        if (node != null)
                        {
                            List<Node> listOfNeighbours = node.GetNeighbouringNodes();
                            if (listOfNeighbours != null)
                            {
                                foreach(Node tempNode in listOfNeighbours)
                                { AddNodeToListOfBadNodes(node.nodeID); }
                            }
                            else { Debug.LogError("Invalid listOfNeighbours (Null)"); }
                        }
                        else { Debug.LogErrorFormat("Invalid node (Null) for sightingErasure.nodeID {0}", sightingErasure.nodeID); }
                    }
                }
            }
            //recalculate weighted data
            GameManager.instance.dijkstraScript.RecalculateWeightedData();
        }
    }

    /// <summary>
    /// adds node to list of bad nodes. Checks for duplicates before doing so.
    /// </summary>
    /// <param name="nodeID"></param>
    private void AddNodeToListOfBadNodes(int nodeID)
    {
        Debug.Assert(nodeID > -1, "Invalid nodeID {0} (less than Zero)");
        if (listOfBadNodes.Exists(x => x == nodeID) == false)
        { listOfBadNodes.Add(nodeID); }
    }

    /// <summary>
    /// returns true if nodeID is on listOfBadNodes, false if not
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    private bool CheckForBadNode(int nodeID)
    { return listOfBadNodes.Exists(x => nodeID == x); }

    /// <summary>
    /// updates all of a node's connections to the specified security level (if lower, ignore otherwise)
    /// </summary>
    /// <param name="sight"></param>
    private void UpdateNodeConnectionSecurity(SightingData sight)
    {
        ConnectionType sightLevel = ConnectionType.None;
        Node node = GameManager.instance.dataScript.GetNode(sight.nodeID);
        if (node != null)
        {
            List<Connection> listOfConnections = node.GetListOfConnections();
            if (listOfConnections != null)
            {
                //convert priority to Connection Security level
                switch (sight.priority)
                {
                    case Priority.Critical:
                    case Priority.High:
                        sightLevel = ConnectionType.HIGH;
                        break;
                    case Priority.Medium:
                        sightLevel = ConnectionType.MEDIUM;
                        break;
                    case Priority.Low:
                        sightLevel = ConnectionType.LOW;
                        break;
                    default:
                        Debug.LogWarningFormat("Unrecognised sight.priority \"{0}\"", sight.priority);
                        break;
                }
                //loop all connections
                for (int i = 0; i < listOfConnections.Count; i++)
                {
                    Connection connection = listOfConnections[i];
                    if (connection != null)
                    {
                        if (connection.SecurityLevel == ConnectionType.None)
                        {
                            connection.ChangeSecurityLevel(sightLevel);
                            /*Debug.LogFormat("[Tst] AIRebelManager.cs -> UpdateNodeConnectionSecurity: Connection from id {0} to id {1} UPGRADED to {2}{3}", connection.GetNode1(), connection.GetNode2(),
                                sightLevel, "\n");*/
                        }
                        //only upgrade connection security level if the sighting data indicates a higher level than is already there. NOTE '>' than 'cause enums for ConnectionType is reversed
                        else if (connection.SecurityLevel > sightLevel)
                        {
                            connection.ChangeSecurityLevel(sightLevel);
                            /*Debug.LogFormat("[Tst] AIRebelManager.cs -> UpdateNodeConnectionSecurity: Connection from id {0} to id {1} UPGRADED to {2}{3}", connection.GetNode1(), connection.GetNode2(),
                                sightLevel, "\n");*/
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid connection (Null) for listOFConnections[{0}]", i); }
                }
            }
            else { Debug.LogError("Invalid listOfConnections (Null)"); }

        }
        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", sight.nodeID); }
            
    }

    /// <summary>
    /// AITracker data is converted to priority based Sighting Data based on time of sighting and AI Rebel player Invisibility. Returns null if a problem
    /// NOTE: calling methods have checked tracker for Null
    /// </summary>
    /// <param name="tracker"></param>
    /// <returns></returns>
    private SightingData ConvertTrackerToSighting(AITracker tracker)
    {
        //keep prioritylevel as an int ( 0 / 1 / 2 / 3  corresponds to Low / Medium / High / Critical )
        int priorityLevel = 0;
        SightingData sighting = null;
        //set a base priority
        int turn = GameManager.instance.turnScript.Turn;
        int turnsAgo = turn - tracker.turn;
        /*Debug.LogFormat("[Tst] AIRebelManager.cs -> ConvertTrackerToSighting: t:{0}, turnsAgo: {1}, nodeID {2}, contact eff {3}{4}", tracker.turn, turnsAgo, tracker.data0, tracker.data1, "\n");*/
        switch (turnsAgo)
        {
            case 0:
            case 1:
                //critical
                priorityLevel = 3;
                break;
            case 2:
                //high
                priorityLevel = 2;
                break;
            case 3:
                //medium
                priorityLevel = 1;
                break;
            case 4:
                //low
                priorityLevel = 0;
                break;
        }
        //adjust for AI rebel player invisibility
        switch (GameManager.instance.playerScript.Invisibility)
        {
            case 3:
                //max level
                priorityLevel--;
                break;
            case 0:
                //min level
                priorityLevel++;
                break;
        }
        //ignore if priority zero or less
        if (priorityLevel > -1)
        {
            sighting = new SightingData();
            sighting.nodeID = tracker.data0;
            //assign priority
            switch (priorityLevel)
            {
                case 4:
                case 3:
                    sighting.priority = Priority.Critical;
                    break;
                case 2:
                    sighting.priority = Priority.High;
                    break;
                case 1:
                    sighting.priority = Priority.Medium;
                    break;
                case 0:
                    sighting.priority = Priority.Low;
                    break;
                default:
                    Debug.LogWarningFormat("Invalid priorityLevel \"{0}\"", priorityLevel);
                    sighting.priority = Priority.Low;
                    break;
            }
        }
        //moveNumber & turn of sighting
        sighting.turn = tracker.turn;
        sighting.moveNumber = tracker.data2;
        return sighting;
    }


    /// <summary>
    /// Populate a sorted dictionary of all available targets
    /// </summary>
    private void ProcessTargetData()
    {
        List<Target> listOfTargets = GameManager.instance.dataScript.GetTargetPool(Status.Live);
        if (listOfTargets != null)
        {
            //temp dict with key -> Target, value -> distance (weighted)
            Dictionary<Target, int> dictOfTargets = new Dictionary<Target, int>();
            int distance;
            int count = listOfTargets.Count;
            if (count > 0)
            {
                int playerNodeID = GameManager.instance.nodeScript.nodePlayer;
                //loop targets and get weighted distance to each
                for (int i = 0; i < count; i++)
                {
                    Target target = listOfTargets[i];
                    if (target != null)
                    {
                        distance = GameManager.instance.dijkstraScript.GetDistanceWeighted(playerNodeID, target.nodeID);
                        /*Debug.LogFormat("[Tst] AIRebelManager.cs -> ProcessTargetData: playerNodeID {0}, targetNodeID {1} DISTANCE {2}{3}", playerNodeID, target.nodeID, distance, "\n");*/
                        if (distance > -1)
                        {
                            //add entry to dictionary
                            try
                            { dictOfTargets.Add(target, distance); }
                            catch (ArgumentException)
                            { Debug.LogErrorFormat("Duplicate target entry for target {0}, id {1}", target.name, target.targetID); }
                        }
                        else { Debug.LogWarningFormat("Invalid dijkstra weighted distance (-1) between id {0} and id {1}", playerNodeID, listOfTargets[i].nodeID); }
                    }
                    else { Debug.LogWarningFormat("Invalid target (Null) for listOfTargets[{0}]", i); }
                }
                //sort dict if it has > 1 entry, nearest target at top of list
                if (dictOfTargets.Count > 0)
                {
                    var sortedTargets = from pair in dictOfTargets
                                        orderby pair.Value ascending
                                        select pair;
                    //populate sorted targets dict
                    foreach (var target in sortedTargets)
                    { dictOfSortedTargets.Add(target.Key, target.Value); }
                }
            }
            /*Debug.LogFormat("[Tst] AIRebelManager.cs -> ProcessTargetData: dictOfSortedTargets has {0} records{1}", dictOfSortedTargets.Count, "\n");*/
        }
        else { Debug.LogError("Invalid listOfTargets (Null)"); }
    }

    /// <summary>
    /// Assesses player and actors for any relevant personal situations, eg. need to take stress leave
    /// </summary>
    private void ProcessPeopleData()
    {
        int resources = GameManager.instance.dataScript.CheckAIResourcePool(globalResistance);

        //Stress Leave
        if (resources >= stressLeaveCost)
        {
            if (isPlayerStressed)
            {
                //must have max invis
                if (GameManager.instance.playerScript.Invisibility == maxStatValue)
                { stressedActorID = playerID; }
            }
        }
        //check  Resistance actors
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
        if (arrayOfActors != null)
        {
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                //check actor is present in slot (not vacant)
                if (GameManager.instance.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
                {
                    Actor actor = arrayOfActors[i];
                    if (actor != null)
                    {
                        //
                        // - - - Stress (only if player not already earmarked for stress leave)
                        //
                        if (stressedActorID != playerID)
                        {
                            //stressed (only if player not already stressed)
                            if (actor.CheckConditionPresent(conditionStressed) == true)
                            {
                                if (resources >= stressLeaveCost)
                                {
                                    //must have max invis for stress leave
                                    if (actor.datapoint2 == maxStatValue)
                                    {
                                        //take first instance found
                                        stressedActorID = actor.actorID;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }
    }

    /// <summary>
    /// determines which set of actor arcs are valid for use this turn. One actor arc is randomly chosen for each Active, onMap, actor present
    /// </summary>
    private void ProcessActorArcData()
    {
        int index;
        //get by value as we'll be deleting
        List<ActorArc> tempList = new List<ActorArc>(GameManager.instance.dataScript.GetListOfResistanceActorArcs());
        if (tempList != null)
        {
            int limit = GameManager.instance.dataScript.CheckNumOfActiveActors(globalResistance);
            for (int i = 0; i < limit; i++)
            {
                index = Random.Range(0, tempList.Count);
                listOfArcs.Add(tempList[index]);
                Debug.LogFormat("[Tst] AIRebelManager.cs -> ProcessActorData: {0} actorArc added to list{1}", tempList[index].name, "\n");
                //delete to prevent duplicates
                tempList.RemoveAt(index);
            }
        }
        else { Debug.LogError("Invalid tempList ActorArcs (Null)"); }
    }



    /// <summary>
    /// provides resources (equivalent to renown for resistance AI side) depending on level of faction support (as per normal)
    /// </summary>
    private void ProcessResources()
    {
        int resources;
        int rnd = Random.Range(0, 100);
        int approvalResistance = GameManager.instance.factionScript.ApprovalResistance;
        int renownPerTurn = GameManager.instance.factionScript.renownPerTurn;
        int threshold = approvalResistance * 10;
        Faction factionResistance = GameManager.instance.factionScript.factionResistance;
        if (factionResistance != null)
        {
            if (rnd < threshold)
            {
                //Support given
                resources = GameManager.instance.dataScript.CheckAIResourcePool(globalResistance) + factionResistance.resourcesAllowance;
                GameManager.instance.dataScript.SetAIResources(globalResistance, resources);
                //Support Provided
                Debug.LogFormat("[Rnd] FactionManager.cs -> CheckFactionSupport: GIVEN need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                if (isPlayer == true)
                {
                    string msgText = string.Format("{0} faction provides SUPPORT (+{1} Resource{2})", factionResistance.name, resources, resources != 1 ? "s" : "");
                    GameManager.instance.messageScript.FactionSupport(msgText, factionResistance, approvalResistance, GameManager.instance.playerScript.Renown, renownPerTurn);
                    //random
                    GameManager.instance.messageScript.GeneralRandom("Faction support GIVEN", "Faction Support", threshold, rnd);
                }
            }
            else
            {
                //Support declined
                Debug.LogFormat("[Rnd] FactionManager.cs -> CheckFactionSupport: DECLINED need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                if (isPlayer == true)
                {
                    string msgText = string.Format("{0} faction declines support ({1} % chance of support)", factionResistance.name, threshold);
                    GameManager.instance.messageScript.FactionSupport(msgText, factionResistance, approvalResistance, GameManager.instance.playerScript.Renown);
                    //random
                    GameManager.instance.messageScript.GeneralRandom("Faction support DECLINED", "Faction Support", threshold, rnd);
                }
            }
        }
        else { Debug.LogError("Invalid faction (Null) for Resistance"); }
    }

    //
    // - - - Create Task - - -
    //

    /// <summary>
    /// checks for any critical, survival related, priority tasks
    /// </summary>
    private void ProcessSurvivalTask()
    {
        int rnd, threshold, count;
        int invisibility = GameManager.instance.playerScript.Invisibility;
        int lieLowTimer = GameManager.instance.actorScript.lieLowTimer;
        int playerNodeID = GameManager.instance.nodeScript.nodePlayer;
        bool isSuccess = false;
        //
        // - - - Nemesis or Erasure team in current node (sighting report)
        //
        if (CheckForBadNode(playerNodeID) == true)
        {
            //first response -> Move away
            Node nodePlayer = GameManager.instance.dataScript.GetNode(playerNodeID);
            if (nodePlayer != null)
            {
                Node nodeMoveTo = GetRandomGoodNode(nodePlayer);
                if (nodeMoveTo != null)
                {
                    Connection connection = nodePlayer.GetConnection(nodeMoveTo.nodeID);
                    if (connection != null)
                    {
                        //random chance of moving (not a given as threat could be nearby and staying still, or lying low, might be a better option)
                        rnd = Random.Range(0, 100);
                        if (rnd < survivalMove)
                        {
                            isSuccess = true;
                            //generate task
                            AITask task = new AITask();
                            task.data0 = nodeMoveTo.nodeID;
                            task.data1 = connection.connID;
                            task.type = AITaskType.Move;
                            task.priority = Priority.Critical;
                            //add task to list of potential tasks
                            AddWeightedTask(task);
                            Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessSurvivalTask: Move Away to node ID {0}{1}", nodeMoveTo.nodeID, "\n");
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid connection (Null) for nodeID {0}", nodeMoveTo.nodeID); }
                }
            }
            else { Debug.LogErrorFormat("Invalid node (Null) for playerNodeID {0}", playerNodeID); }
            //nothing decided so far -> player lie low
            if (isSuccess == false)
            {
                // Emergency Lie Low 
                if (invisibility < 3)
                {
                    //not a Surveillance crackdown
                    if (security != AuthoritySecurityState.SurveillanceCrackdown)
                    {
                        if (lieLowTimer == 0)
                        {
                            //random chance of lying low
                            rnd = Random.Range(0, 100);
                            int lieLowChance = lieLowEmergency;
                            if (isPlayerStressed == true)
                            { lieLowChance += lieLowStressed; }
                            if (rnd < lieLowChance)
                            {
                                isSuccess = true;
                                //generate task
                                AITask task = new AITask();
                                task.data0 = playerNodeID;
                                task.data1 = playerID;
                                task.type = AITaskType.LieLow;
                                task.priority = Priority.Critical;
                                //add task to list of potential tasks
                                AddWeightedTask(task);
                                Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessSurvivalTask: PLAYER Emergency Lie Low at node ID {0}{1}", playerNodeID, "\n");
                            }
                        }
                    }
                }
            }
        }
        //
        // - - - Low Player invisibility (not at a Bad node) - - - 
        //
        else if (invisibility < lieLowThresholdPlayer)
        {
            //not a Surveillance crackdown
            if (security != AuthoritySecurityState.SurveillanceCrackdown)
            {
                if (lieLowTimer == 0)
                {
                    rnd = Random.Range(0, 100);
                    threshold = 0;
                    switch (invisibility)
                    {
                        case 2: threshold = lieLowTwo; break;
                        case 1: threshold = lieLowOne; break;
                        case 0: threshold = lieLowZero; break;
                        default: Debug.LogErrorFormat("Invalid invisibility \"[0}\"", invisibility); break;
                    }
                    if (isPlayerStressed == true)
                    { threshold += lieLowStressed; }
                    if (rnd < threshold)
                    {
                        isSuccess = true;
                        //generate task
                        AITask task = new AITask();
                        task.data0 = playerNodeID;
                        task.data1 = playerID;
                        task.type = AITaskType.LieLow;
                        task.priority = Priority.Critical;
                        //add task to list of potential tasks
                        AddWeightedTask(task);
                        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessSurvivalTask: PLAYER Generic Lie Low at node ID {0}{1}", playerNodeID, "\n");
                    }
                }
            }
        }
        //actor lie low
        else
        {
            //criteria -> actor invis <= 1, lie low timer = 0, player invis > 1, actor stressed (increased chanced of lying low)
            //not a Surveillance crackdown
            if (security != AuthoritySecurityState.SurveillanceCrackdown)
            {
                if (lieLowTimer == 0)
                {
                    List<Actor> listOfActors = new List<Actor>();
                    //loop actors and check for any < lieLowThreshold
                    Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
                    if (arrayOfActors != null)
                    {
                        for (int i = 0; i < arrayOfActors.Length; i++)
                        {
                            //check actor is present in slot (not vacant)
                            if (GameManager.instance.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
                            {
                                Actor actor = arrayOfActors[i];
                                if (actor != null)
                                {
                                    //below threshold
                                    if (actor.datapoint2 < lieLowThresholdActor)
                                    {
                                        if (actor.CheckConditionPresent(conditionStressed) == true)
                                        {
                                            //add three entries (high priority)
                                            listOfActors.Add(actor); listOfActors.Add(actor); listOfActors.Add(actor);
                                        }
                                        else
                                        {
                                            //add one entry
                                            listOfActors.Add(actor);
                                        }
                                    }
                                }
                            }
                        }
                        //any candidates for lying low
                        count = listOfActors.Count();
                        if (count > 0)
                        {
                            //randomly select from pool of actors 
                            int index = Random.Range(0, count);
                            Actor actorLieLow = listOfActors[index];
                            if (Random.Range(0, 100) < lieLowChanceActor)
                            {

                                isSuccess = true;
                                //generate task
                                AITask task = new AITask();
                                task.data0 = playerNodeID;
                                task.data1 = listOfActors[index].actorID;
                                task.type = AITaskType.LieLow;
                                task.priority = Priority.Critical;
                                //add task to list of potential tasks
                                AddWeightedTask(task);
                                Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessSurvivalTask: ACTOR {0}, id {1}, Emergency Lie Low{2}", actorLieLow.arc.name, actorLieLow.actorID, "\n");
                            }
                        }
                    }
                    else { Debug.LogError("Invalid arrayOfActors (Null)"); }
                }
            }
        }
    }

    /// <summary>
    /// Select a suitable node to move to (single node move)
    /// </summary>
    private void ProcessMoveTask()
    {
        Node nodePlayer = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
        Node nodeMoveTo = null;
        Connection connection = null;
        bool isProceed = true;
        if (nodePlayer != null)
        {
            //Debug -> Player moves around map to nearest target then selects a new target to move to

            //AT TARGET NODE
            if (nodePlayer.nodeID == targetNodeID)
            {
                //select a new target goal
                if (dictOfSortedTargets.Count > 0)
                {
                    //select the nearest target goal that is > 0 distance
                    foreach (var record in dictOfSortedTargets)
                    {
                        if (record.Value > 0)
                        {
                            //assign new target
                            targetNodeID = record.Key.nodeID;
                            nodeMoveTo = GameManager.instance.dataScript.GetNode(targetNodeID);
                            Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessMoveTask: AI Player at Target Node, new target chosen{0}", "\n");
                            break;
                        }
                    }
                }
                //No suitable target available -> generate a random node as a new target
                if (targetNodeID == nodePlayer.nodeID)
                {
                    Node randomNode;
                    int counter = 0;
                    do
                    {
                        randomNode = GameManager.instance.dataScript.GetRandomNode();
                        counter++;
                        if (counter > 10)
                        {
                            Debug.LogError("Counter timed out");
                            break;
                        }
                    }
                    while (randomNode.nodeID == nodePlayer.nodeID);
                    //assign new target
                    targetNodeID = randomNode.nodeID;
                    nodeMoveTo = randomNode;
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessMoveTask: AI Player at Target Node, Random Node chosen, id {0}{1}", targetNodeID, "\n");
                }
            }
            //NOT AT Target Node
            else
            { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessMoveTask: AI Player continues to move towards target{0}", "\n"); }
            //path to target   
            List<Connection> pathList = GameManager.instance.dijkstraScript.GetPath(nodePlayer.nodeID, targetNodeID, true);
            //get next node in sequence
            if (pathList != null)
            {
                connection = pathList[0];
                if (connection.node1.nodeID != nodePlayer.nodeID)
                { nodeMoveTo = connection.node1; }
                else { nodeMoveTo = connection.node2; }
            }
            else { Debug.LogError("Invalid pathList (Null)"); }
            //GENERATE TASK
            if (nodeMoveTo != null)
            {
                //check if moveTo node on listOfBadNodes
                if (CheckForBadNode(nodeMoveTo.nodeID) == true)
                {
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessMoveTask: Selected node id {0} is on listOfBadNodes{1}", nodeMoveTo.nodeID, "\n");
                    //get a random neighbouring node which is good
                    nodeMoveTo = GetRandomGoodNode(nodePlayer);
                    if (nodeMoveTo == null)
                    {
                        //no viable node that isn't bad amongst neighbours. Cancel move task
                        isProceed = false;
                        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessMoveTask: Selected node is BAD, no viable alternative. Move task CANCELLED{0}", "\n");
                    }
                }
                if (isProceed == true)
                {
                    //get connectionID (may have changed)
                    Connection conn = nodePlayer.GetConnection(nodeMoveTo.nodeID);
                    if (conn != null)
                    {
                        //generate task
                        AITask task = new AITask();
                        task.data0 = nodeMoveTo.nodeID;
                        task.data1 = conn.connID;
                        task.type = AITaskType.Move;
                        task.priority = priorityMovePlayer;
                        //add task to list of potential tasks
                        AddWeightedTask(task);
                        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessMoveTask: targetNodeID {0}, move to {1}{2}", targetNodeID, nodeMoveTo.nodeID, "\n");
                    }
                    else { Debug.LogErrorFormat("Invalid connection (Null) for nodeID {0}", nodeMoveTo.nodeID); }
                }
            }
            else { Debug.LogError("Invalid nodeMoveTo (Null)"); }
        }
        else { Debug.LogErrorFormat("Invalid player node (Null) for nodeID {0}", GameManager.instance.nodeScript.nodePlayer); }
    }

    /*/// <summary>
    /// assorted administrative tasks
    /// </summary>
    private void ProcessAdminTask()
    {

    }*/

    /// <summary>
    /// Player or Actors attempt target/s. One task generated for player (if criteria O.K). If not player task then one target task per actor (possibly if enough targets and criteria O.K)
    /// </summary>
    private void ProcessTargetTask()
    {
        int targetTally, targetID;
        bool isSuccess = false;
        Node nodePlayer = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
        //intel assumed to be used at max for any target attempt
        int intelTemp;
        if (targetIntel >= targetIntelAttempt) { intelTemp = targetIntelAttempt; }
        else { intelTemp = targetIntel; }
        //
        // - - - Player - - -
        //
        if (nodePlayer.nodeID == targetNodeID)
        {
            targetID = nodePlayer.targetID;
            //Player at target node
            targetTally = GameManager.instance.targetScript.GetTargetTallyAI(targetID);
            targetTally += intelTemp;
            if (gearPool > 0 && CheckGearAvailable(false) == true) 
            { targetTally++; }
            //must be above minimum odds threshold to proceed
            if (targetTally >= targetAttemptMinOdds)
            {
                //generate task for Player Attempt
                Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: <b>Player target attempt passed Odds check (threshold {0}, tally {1}){2}", targetAttemptMinOdds, targetTally, "\n");
                isSuccess = true;
                //generate task
                AITask task = new AITask();
                task.data0 = playerID;
                task.data1 = nodePlayer.nodeID;
                task.data2 = targetID;
                task.type = AITaskType.Target;
                task.priority = priorityTargetPlayer;
                //add task to list of potential tasks
                AddWeightedTask(task);
                Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: Player Attempt Target task, targetID {0} at {1}, {2}, id {3}{4}", targetID, nodePlayer.nodeName, nodePlayer.Arc.name, 
                    nodePlayer.nodeID, "\n");
            }
            else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: Player target NOT attempted due to low odds (threshold {0}, tally {1}){2}", targetAttemptMinOdds, targetTally, "\n"); }
        }
        //
        // - - - Actors - - -
        //
        if (isSuccess == false)
        {
            //is there at least on Live target available
            List<Target> listOfLiveTargets = GameManager.instance.dataScript.GetTargetPool(Status.Live);
            if (listOfLiveTargets != null)
            {
                int actorID;
                Target target;
                for (int i = 0; i < listOfLiveTargets.Count; i++)
                {
                    target = listOfLiveTargets[i];
                    if (target != null)
                    {
                        if (GameManager.instance.dataScript.CheckActorArcPresent(target.actorArc, globalResistance) == true)
                        {
                            //actor of that type present 
                            if (CheckActorArcPresent(target.actorArc.ActorArcID) == true)
                            {
                                //Actor present, check target odds
                                targetTally = GameManager.instance.targetScript.GetTargetTallyAI(target.targetID);
                                targetTally += intelTemp;
                                if (gearPool > 0 && CheckGearAvailable(false) == true)
                                { targetTally++; }
                                //must be above minimum odds threshold to proceed
                                if (targetTally >= targetAttemptMinOdds)
                                {
                                    //which OnMap actor is used
                                    actorID = GetOnMapActor(target.actorArc.ActorArcID);
                                    //generate task for Actor Attempt
                                    Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: <b>{0} target attempt passed Odds check (threshold {1}, tally {2}){3}", target.actorArc.name, 
                                        targetAttemptMinOdds, targetTally, "\n");
                                    if (actorID > -1)
                                    {
                                        //generate task
                                        AITask task = new AITask();
                                        task.data0 = actorID;
                                        task.data1 = target.nodeID;
                                        task.data2 = target.targetID;
                                        task.type = AITaskType.Target;
                                        task.priority = priorityTargetActor;
                                        //add task to list of potential tasks
                                        AddWeightedTask(task);
                                        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: {0} attempt Target task, targetID {1} at {2}, {3}, id {4}{5}", target.actorArc.name,
                                            target.targetID, nodePlayer.nodeName, nodePlayer.Arc.name, nodePlayer.nodeID, "\n");
                                    }
                                    else { Debug.LogError("Invalid actorID {-1)"); }
                                }
                                else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: {0} target NOT attempted due to low odds (threshold {1}, tally {2}){3}", target.actorArc.name,
                                    targetAttemptMinOdds, targetTally, "\n"); }
                            }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid target (Null) for listOfLiveTargets[{0}]", i); }
                }
            }
            else { Debug.LogError("Invalid listOfLiveTargets (Null)"); }
        }
    }


    /// <summary>
    /// Actor and Player actor action tasks. One task is generated for each ActorArc in listOfActorArcs
    /// </summary>
    private void ProcessActorArcTask()
    {
        int limit;
        string actorArcName;
        //create a list of all active, current, onMap actors (used for AI decision logic)
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
        if (arrayOfActors != null)
        {
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                //check actor is present in slot (not vacant)
                if (GameManager.instance.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
                {
                    Actor actor = arrayOfActors[i];
                    if (actor != null)
                    { listOfCurrentActors.Add(actor); }
                }
            }
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessActorArcTask: listOfCurrentActors has {0} records{1}", listOfCurrentActors.Count, "\n");
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }

        //create a task depending on the number of actorArcs but capped by number of onMap active actors
        limit = Mathf.Min(listOfArcs.Count, listOfCurrentActors.Count);
        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessActorArcTask: Creating {0} Tasks (listOfArcs {1} records, listOfCurrentActors {2} records){3}", limit, listOfArcs.Count, listOfCurrentActors.Count, "\n");
        //loop arcs and create a task for each
        for (int index = 0; index < limit; index++)
        {
            actorArcName = listOfArcs[index].name;
            //each branches to an sub method that generates a task
            switch (actorArcName)
            {
                case "ANARCHIST":
                    ProcessAnarchistTask(actorArcName);
                    break;
                case "BLOGGER":
                    ProcessBloggerTask(actorArcName);
                    break;
                case "FIXER":
                    ProcessFixerTask(actorArcName);
                    break;
                case "HACKER":
                    ProcessHackerTask(actorArcName);
                    break;
                case "HEAVY":
                    ProcessHeavyTask(actorArcName);
                    break;
                case "OBSERVER":
                    ProcessObserverTask(actorArcName);
                    break;
                case "OPERATOR":
                    ProcessOperatorTask(actorArcName);
                    break;
                case "PLANNER":
                    ProcessPlannerTask(actorArcName);
                    break;
                case "RECRUITER":
                    break;
                default:
                    Debug.LogErrorFormat("Unrecognised Actor Arc \"{0}\"", listOfArcs[index].name);
                    break;
            }
        }
    }

    /// <summary>
    /// deals with people specific tasks, eg. Stress Leave
    /// </summary>
    private void ProcessPeopleTask()
    {
        //
        // - - - Stress Leave - - -
        //
        if (stressedActorID > -1)
        {
            //generate task -> player or actor
            AITask task = new AITask();
            task.data0 = stressedActorID;
            task.type = AITaskType.StressLeave;
            task.priority = priorityStressLeavePlayer;
            //medium priority for player, low for actor
            if (stressedActorID != playerID)
            { task.priority = priorityStressLeaveActor; }
            //add task to list of potential tasks
            AddWeightedTask(task);
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessPeopleTask: stressedActorID {0}, Stress Leave{1}", stressedActorID, "\n");
        }
    }


    /// <summary>
    /// Do nothing default task
    /// </summary>
    private void ProcessIdleTask()
    {
        //generate task
        AITask task = new AITask();
        task.type = AITaskType.Idle;
        task.data0 = GameManager.instance.nodeScript.nodePlayer;
        task.data1 = playerID;
        task.priority = priorityIdlePlayer;        
        //add task to list of potential tasks
        AddWeightedTask(task);
    }

    /// <summary>
    /// Fixer Actor Arc task to get extra gear points. Data2 is the amount to top up the gear pool, name0 is the actor arc name
    /// </summary>
    private void ProcessFixerTask(string actorArcName)
    {
        int actorID, nodeID = -1;
        //is the gear pool below the threshold
        if (gearPool < gearPoolThreshold)
        {
            if (gearPool < gearPoolMaxSize)
            {
                //Player does action?
                if (CheckPlayerAction(actorArcName) == true)
                {
                    actorID = playerID;
                    nodeID = GameManager.instance.nodeScript.nodePlayer;
                }
                else
                {
                    actorID = FindActor(arcFixer);
                    nodeID = FindNodeRandom(actorID);
                }
                if (actorID > -1 && nodeID > -1)
                {
                        //generate task
                        AITask task = new AITask();
                        task.type = AITaskType.ActorArc;
                        task.data0 = actorID;
                        task.data1 = nodeID;
                        task.data2 = gearPoolTopUp;
                        task.name0 = actorArcName;
                        task.priority = priorityFixerTask;
                        //add task to list of potential tasks
                        AddWeightedTask(task);
                }
                else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessFixerTask: Invalid FIXER task (nodeID {0}, actorID {1}). No task generated{2}", nodeID, actorID, "\n"); }
            }
          else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessFixerTask: No FIXER task as gearPool {0} Maxxed out (limit {1}){2}", gearPool, gearPoolMaxSize, "\n"); }
        }
        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessFixerTask: No FIXER task as gearPool {0} >= gearPoolThreshold {1}{2}", gearPool, gearPoolThreshold, "\n"); }
    }


    /// <summary>
    /// Blogger Actor Arc task.
    /// NOTE: actorArcName checked for null by calling method
    /// </summary>
    private void ProcessBloggerTask(string actorArcName)
    {
        int actorID = -1;
        int nodeID = -1;
        //Player does action?
        if (CheckPlayerAction(actorArcName) == true)
        {
            actorID = playerID;
            nodeID = GameManager.instance.nodeScript.nodePlayer;
        }
        else
        {
            //get node and actorID
            Tuple<int, int> results = FindNodeActorRandom(actorArcName);
            nodeID = results.Item1;
            actorID = results.Item2;
        }
        //generate task if valid data is present, if none, ignore task
        if (nodeID > -1 && actorID > -1)
        {
            //generate task
            AITask task = new AITask();
            task.type = AITaskType.ActorArc;
            task.data0 = actorID;
            task.data1 = nodeID;
            task.name0 = actorArcName;
            task.priority = priorityBloggerTask;
            //add task to list of potential tasks
            AddWeightedTask(task);
        }
        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessBloggerTask: Invalid BLOGGER task (nodeID {0}, actorID {1}). No task generated{2}", nodeID, actorID, "\n"); }
    }

    /// <summary>
    /// Hacker Actor Arc task.
    /// NOTE: actorArcName checked for null by calling method
    /// </summary>
    private void ProcessHackerTask(string actorArcName)
    {
        int actorID = -1;
        int nodeID = -1;
        //Player does action?
        if (CheckPlayerAction(actorArcName) == true)
        {
            actorID = playerID;
            nodeID = GameManager.instance.nodeScript.nodePlayer;
        }
        else
        {
            //get node and actorID
            Tuple<int, int> results = FindNodeActorRandom(actorArcName);
            nodeID = results.Item1;
            actorID = results.Item2;
        }
        //generate task if valid data is present, if none, ignore task
        if (nodeID > -1 && actorID > -1)
        {
            //generate task
            AITask task = new AITask();
            task.type = AITaskType.ActorArc;
            task.data0 = actorID;
            task.data1 = nodeID;
            task.name0 = actorArcName;
            task.priority = priorityHackerTask;
            //add task to list of potential tasks
            AddWeightedTask(task);
        }
        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessHackerTask: Invalid HACKER task (nodeID {0}, actorID {1}). No task generated{2}", nodeID, actorID, "\n"); }
    }

    /// <summary>
    /// Anarchist Actor Arc task.
    /// NOTE: actorArcName checked for null by calling method
    /// </summary>
    private void ProcessAnarchistTask(string actorArcName)
    {
        int actorID = -1;
        int nodeID = -1;
        //Player does action?
        if (CheckPlayerAction(actorArcName) == true)
        {
            actorID = playerID;
            nodeID = GameManager.instance.nodeScript.nodePlayer;
        }
        else
        {
            //get node and actorID
            Tuple<int, int> results = FindNodeActorRandom(actorArcName);
            nodeID = results.Item1;
            actorID = results.Item2;
        }
        //generate task if valid data is present, if none, ignore task
        if (nodeID > -1 && actorID > -1)
        {
            //generate task
            AITask task = new AITask();
            task.type = AITaskType.ActorArc;
            task.data0 = actorID;
            task.data1 = nodeID;
            task.name0 = actorArcName;
            task.priority = priorityAnarchistTask;
            //add task to list of potential tasks
            AddWeightedTask(task);
        }
        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessAnarchistTask: Invalid ANARCHIST task (nodeID {0}, actorID {1}). No task generated{2}", nodeID, actorID, "\n"); }
    }

    /// <summary>
    /// Observer Actor Arc task
    /// NOTE: actorArcName checked for null by calling method
    /// </summary>
    /// <param name="actorArcName"></param>
    private void ProcessObserverTask(string actorArcName)
    {
        int actorID = -1;
        int nodeID = -1;
        //Player does action?
        if (CheckPlayerAction(actorArcName) == true)
        {
            actorID = playerID;
            nodeID = GameManager.instance.nodeScript.nodePlayer;
        }
        else
        {
            //get node and actorID
            Tuple<int, int> results = FindNodeActorRandom(actorArcName);
            nodeID = results.Item1;
            actorID = results.Item2;
        }
        //generate task if valid data is present, if none, ignore task
        if (nodeID > -1 && actorID > -1)
        {
            //generate task
            AITask task = new AITask();
            task.type = AITaskType.ActorArc;
            task.data0 = actorID;
            task.data1 = nodeID;
            task.name0 = actorArcName;
            task.priority = priorityObserverTask;
            //add task to list of potential tasks
            AddWeightedTask(task);
        }
        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessObserverTask: Invalid OBSERVER task (nodeID {0}, actorID {1}). No task generated{2}", nodeID, actorID, "\n"); }
    }

    /// <summary>
    /// Operator Actor Arc task
    /// NOTE: actorArcName checked for null by calling method
    /// </summary>
    /// <param name="actorArcName"></param>
    private void ProcessOperatorTask(string actorArcName)
    {
        int actorID = -1;
        int nodeID = -1;
        //Player does action?
        if (CheckPlayerAction(actorArcName) == true)
        {
            actorID = playerID;
            nodeID = GameManager.instance.nodeScript.nodePlayer;
        }
        else
        {
            //get node and actorID
            Tuple<int, int> results = FindNodeActorRandom(actorArcName);
            nodeID = results.Item1;
            actorID = results.Item2;
        }
        //generate task if valid data is present, if none, ignore task
        if (nodeID > -1 && actorID > -1)
        {
            //generate task
            AITask task = new AITask();
            task.type = AITaskType.ActorArc;
            task.data0 = actorID;
            task.data1 = nodeID;
            task.name0 = actorArcName;
            task.priority = priorityOperatorTask;
            //add task to list of potential tasks
            AddWeightedTask(task);
        }
        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessOperatorTask: Invalid OPERATOR task (nodeID {0}, actorID {1}). No task generated{2}", nodeID, actorID, "\n"); }
    }

    /// <summary>
    /// Heavy Actor Arc task.
    /// NOTE: actorArcName checked for null by calling method
    /// </summary>
    private void ProcessHeavyTask(string actorArcName)
    {
        int actorID = -1;
        int nodeID = -1;
        //Player does action?
        if (CheckPlayerAction(actorArcName) == true)
        {
            actorID = playerID;
            nodeID = GameManager.instance.nodeScript.nodePlayer;
        }
        else
        {
            //get node and actorID
            Tuple<int, int> results = FindNodeActorRandom(actorArcName);
            nodeID = results.Item1;
            actorID = results.Item2;
        }
        //generate task if valid data is present, if none, ignore task
        if (nodeID > -1 && actorID > -1)
        {
            //generate task
            AITask task = new AITask();
            task.type = AITaskType.ActorArc;
            task.data0 = actorID;
            task.data1 = nodeID;
            task.name0 = actorArcName;
            task.priority = priorityHeavyTask;
            //add task to list of potential tasks
            AddWeightedTask(task);
        }
        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessHeavyTask: Invalid HEAVY task (nodeID {0}, actorID {1}). No task generated{2}", nodeID, actorID, "\n"); }
    }

    /// <summary>
    /// Planner Actor Arc task. Ther eis no limit on the amount of intel that can be acquired
    /// NOTE: actorArcName checked for null by calling method
    /// </summary>
    private void ProcessPlannerTask(string actorArcName)
    {
        int actorID = -1;
        int nodeID = -1;
        if (targetIntel < targetIntelMax)
        {
            //Player does action?
            if (CheckPlayerAction(actorArcName) == true)
            {
                actorID = playerID;
                nodeID = GameManager.instance.nodeScript.nodePlayer;
            }
            else
            {
                //get node and actorID
                Tuple<int, int> results = FindNodeActorRandom(actorArcName);
                nodeID = results.Item1;
                actorID = results.Item2;
            }
            //generate task if valid data is present, if none, ignore task
            if (nodeID > -1 && actorID > -1)
            {
                //generate task
                AITask task = new AITask();
                task.type = AITaskType.ActorArc;
                task.data0 = actorID;
                task.data1 = nodeID;
                task.data2 = targetIntelTopUp;
                task.name0 = actorArcName;
                task.priority = priorityPlannerTask;
                //add task to list of potential tasks
                AddWeightedTask(task);
            }
            else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessPlannerTask: Invalid PLANNER task (nodeID {0}, actorID {1}). No task generated{2}", nodeID, actorID, "\n"); }
        }
        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessPlannerTask: PLANNER task NOT generated as already at Max targetIntel (have {0}, max is {1}){2}", targetIntel, targetIntelMax, "\n"); }
    }

    /// <summary>
    /// randomly selects a good node (not on listOfBadNodes) from the specified node's immediate neighbours. Returns Null if none found.
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    private Node GetRandomGoodNode(Node node)
    {
        Node goodNode = null;
        if (node != null)
        {
            //get list of current node neighbours (by value, not reference otherwise you'll delete from the nodes list of Neighbours)
            List<Node> listOfNeighbours = new List<Node>(node.GetNeighbouringNodes());
            if (listOfNeighbours != null)
            {
                //remove any bad nodes, reverse loop
                if (listOfNeighbours.Count > 0)
                {
                    for (int i = listOfNeighbours.Count - 1; i >= 0; i--)
                    {
                        Node tempNode = listOfNeighbours[i];
                        if (tempNode != null)
                        {
                            if (CheckForBadNode(tempNode.nodeID) == true)
                            {
                                //remove bad node
                                listOfNeighbours.RemoveAt(i);
                            }
                        }
                        else { Debug.LogErrorFormat("Invalid node (Null) for listOfNeighbours[{0}]", i); }
                    }
                    //any remaining nodes
                    if (listOfNeighbours.Count > 0)
                    {
                        //randomly choose one as a moveTo node
                        goodNode = listOfNeighbours[Random.Range(0, listOfNeighbours.Count)];
                        Debug.LogFormat("[Rim] AIRebelManager.cs -> GetRandomGoodNode: Randomly choose GOOD node id {0}{1}", goodNode.nodeID, "\n");
                    }
                }
            }
            else { Debug.LogError("Invalid listOfNeighbours (Null)"); }
        }
        else { Debug.LogError("Invalid node (Null)"); }
        return goodNode;
    }

    /// <summary>
    /// sub method to add a task to listOfTasksPotential/Critical in a weighted manner
    /// </summary>
    /// <param name="task"></param>
    private void AddWeightedTask(AITask task)
    {
        if (task != null)
        {
            switch (task.priority)
            {
                case Priority.Critical:
                    listOfTasksCritical.Add(task);
                    break;
                case Priority.High:
                    for (int i = 0; i < priorityHigh; i++)
                    { listOfTasksPotential.Add(task); }
                    break;
                case Priority.Medium:
                    for (int i = 0; i < priorityMedium; i++)
                    { listOfTasksPotential.Add(task); }
                    break;
                case Priority.Low:
                    for (int i = 0; i < priorityLow; i++)
                    { listOfTasksPotential.Add(task); }
                    break;
                default:
                    Debug.LogErrorFormat("Unrecognised task priority \"{0}\"", task.priority);
                    break;
            }
        }
        else { Debug.LogError("Invalid task (Null)"); }
    }

    //
    // - - - Execute Tasks - - -
    //

    /// <summary>
    /// Chose task to implement. If any critical tasks are present one is randomly chosen, if not one is randomly chosen from listofTasksPotential.
    /// </summary>
    private void ProcessTaskFinal()
    {
        AITask task = null;
        //check for Critical tasks for
        int count = listOfTasksCritical.Count;
        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTaskFinal: {0} Critical Task{1} available{2}", count, count != 1 ? "s" : "", "\n");
        if (count > 0)
        {
            if (count > 1)
            { task = listOfTasksCritical[Random.Range(0, count)]; }
            else { task = listOfTasksCritical[0]; }
        }
        else
        {
            //no critical, check for lower priority tasks
            count = listOfTasksPotential.Count;
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTaskFinal: {0} Potential Task{1} available{2}", count, count != 1 ? "s" : "", "\n");
            if (count > 0)
            {

                //debug
                for (int i = 0; i < count; i++)
                {
                    AITask tempTask = listOfTasksPotential[i];
                    if (string.IsNullOrEmpty(tempTask.name0) == false)
                    { Debug.LogFormat("[Tst] AIRebelManager.cs -> ProcessTaskFinal: type {0}, {1} priority, d0: {2}, d1: {3}, d2: {4}, name0: {5}{6}", tempTask.type, tempTask.priority,
                          tempTask.data0, tempTask.data1, tempTask.data2, tempTask.name0, "\n"); }
                    else
                    { Debug.LogFormat("[Tst] AIRebelManager.cs -> ProcessTaskFinal: type {0}, {1} priority, d0: {2}, d1: {3}, d2: {4}{5}", tempTask.type, tempTask.priority,
                          tempTask.data0, tempTask.data1, tempTask.data2, "\n"); }
                }

                //select a task from listOfPotential Tasks
                if (count > 1)
                { task = listOfTasksPotential[Random.Range(0, count)]; }
                else { task = listOfTasksPotential[0]; }
            }
            else { Debug.LogError("There are no tasks to execute in listOfTaskPotential or listOfTasksCritical"); }
        }
        //Execute task
        if (task != null)
        { ExecuteTask(task); }
        else { Debug.LogWarning("Invalid task (Null)"); }
    }


    /// <summary>
    /// Implement task. One task is implemented. Called by ProcessTaskFinal
    /// NOTE: Task Checked for Null by calling method
    /// </summary>
    private void ExecuteTask(AITask task)
    {
        if (string.IsNullOrEmpty(task.name0) == false)
        { Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteTask: task {0}, {1} priority, d0: {2}, d1: {3}, d2: {4}, name0: {5}{6}", task.type, task.priority, task.data0, task.data1,
              task.data2, task.name0, "\n"); }
        else
        { Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteTask: task {0}, {1} priority, d0: {2}, d1: {3}, d2: {4}{5}", task.type, task.priority, task.data0, task.data1, task.data2, "\n"); }
        //execute taks
        switch (task.type)
        {
            case AITaskType.Move:
                ExecuteMoveTask(task);
                break;
            case AITaskType.LieLow:
                ExecuteLieLowTask(task);
                break;
            case AITaskType.StressLeave:
                ExecuteStressLeaveTask(task);
                break;
            case AITaskType.ActorArc:
                ExecuteActorArcTask(task);
                break;
            case AITaskType.Idle:
                ExecuteIdleTask(task);
                break;
            case AITaskType.Target:
                ExecuteTargetTask(task);
                break;
            default:
                Debug.LogErrorFormat("Invalid task (Unrecognised) \"{0}\"", task.type);
                break;
        }

    }

    /// <summary>
    /// AI Player moves, task.data0 is nodeID, task.data1 is connectionID
    /// NOTE: Task checked for Null by parent method
    /// </summary>
    private void ExecuteMoveTask(AITask task)
    {
        Node node = GameManager.instance.dataScript.GetNode(task.data0);
        if (node != null)
        {
            //update player node
            GameManager.instance.nodeScript.nodePlayer = node.nodeID;
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteMoveTask: AI Player moves to {0}, {1}, id {2}{3}", node.nodeName, node.Arc.name, node.nodeID, "\n");
            //expend action
            UseAction("Move");
            //move list (for when autorun ends)
            node.SetPlayerMoveNodes();
            //gear

            //invisibility
            Connection connection = GameManager.instance.dataScript.GetConnection(task.data1);
            if (connection != null)
            {
                if (connection.SecurityLevel != ConnectionType.None)
                {
                    //is their gear available?
                    if (CheckGearAvailable() == true)
                    { Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteMoveTask: GEAR USED to remain unnoticed while moving{0}", "\n"); }
                    else
                    { UpdateInvisibilityMove(connection, node); }
                }
            }
            else { Debug.LogErrorFormat("Invalid connection (Null) for connID {0}", task.data1); }

            //Tracker data
            HistoryRebelMove history = new HistoryRebelMove();
            history.turn = GameManager.instance.turnScript.Turn;
            history.playerNodeID = node.nodeID;
            history.invisibility = GameManager.instance.playerScript.Invisibility;
            history.nemesisNodeID = GameManager.instance.nodeScript.nodeNemesis;
            GameManager.instance.dataScript.AddHistoryRebelMove(history);

            //Erasure team picks up player immediately if invisibility 0
            CaptureDetails captureDetails = GameManager.instance.captureScript.CheckCaptured(node.nodeID, playerID);
            if (captureDetails != null)
            {
                //Player captured!
                captureDetails.effects = "The move went bad";
                EventManager.instance.PostNotification(EventType.Capture, this, captureDetails, "NodeManager.cs -> ProcessMoveOutcome");
            }
            else
            {
                //Nemesis, if at same node, can interact and damage player
                GameManager.instance.nemesisScript.CheckNemesisAtPlayerNode(true);
            }
        }
        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", task.data0);}
    }


    /// <summary>
    /// Player or Actor commences Lying Low. task.data0 -> nodeID, task.data1 -> actorID (999 if player)
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteLieLowTask(AITask task)
    {
        Debug.Assert(GameManager.instance.turnScript.authoritySecurityState != AuthoritySecurityState.SurveillanceCrackdown, string.Format("Invalid authoritySecurityState {0}",
            GameManager.instance.turnScript.authoritySecurityState));
        string aiName = "Unknown";
        bool isSuccess = true;
        if (task.data1 == playerID)
        {
            //Player Lie low
            aiName = playerName;
            //default data 
            status = ActorStatus.Inactive;
            inactiveStatus = ActorInactive.LieLow;
            GameManager.instance.playerScript.isLieLowFirstturn = true;
            //message (only if human player after an autorun)
            if (isPlayer == true)
            {
                GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerLieLow);
                Debug.LogFormat("[Ply] AIRebelManager.cs -> ExecuteLieLowTask: Player commences LYING LOW at node ID {0}{1}", task.data0, "\n");
                //message
                string text = string.Format("{0} is lying Low. Status: {1}", playerName, status);
                string reason = string.Format("is currently Lying Low and is{0}{1}<b>cut off from all communications</b>", "\n", "\n");
                GameManager.instance.messageScript.ActorStatus(text, "is LYING LOW", reason, playerID, globalResistance);
            }
            //expend action
            UseAction("Lie Low (Player)");
        }
        else
        {
            //Actor Lie Low
            Actor actor = GameManager.instance.dataScript.GetActor(task.data1);
            if (actor != null)
            {
                aiName = string.Format("{0}, {1}", actor.actorName, actor.arc.name);
                actor.Status = ActorStatus.Inactive;
                actor.inactiveStatus = ActorInactive.LieLow;
                actor.isLieLowFirstturn = true;
                //message (only if human player after an autorun)
                if (isPlayer == true)
                {
                    Debug.LogFormat("[Ply] AIRebelManager.cs -> ExecuteLieLowTask: Actor {0}, commences LYING LOW{1}", aiName, "\n");
                    //message
                    string text = string.Format("{0} is lying Low. Status: {1}", aiName, actor.Status);
                    string reason = string.Format("is currently Lying Low and is{0}{1}<b>out of communication</b>", "\n", "\n");
                    GameManager.instance.messageScript.ActorStatus(text, "is LYING LOW", reason, actor.actorID, globalResistance);
                }
                //expend action
                UseAction("Lie Low (Actor)");
            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data1); isSuccess = false; }
        }
        if (isSuccess == true)
        {
            //set lie low timer
            GameManager.instance.actorScript.SetLieLowTimer();
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteLieLowTask: \"{0}\", id {1} is LYING LOW at node ID {2}{3}", aiName, task.data1, task.data0, "\n");
        }
    }

    /// <summary>
    /// actor / player takes stress leave where invis max and stressed (NOT a survival task). Renown cost & Opportunity cost
    /// </summary>
    private void ExecuteStressLeaveTask(AITask task)
    {
        if (task != null)
        {
            bool isSuccess = false;
            string text = "Unknown";
            string actorName = "Unknown";
            string actorType = "Unknown";
            int resources = GameManager.instance.dataScript.CheckAIResourcePool(globalResistance);
            if (resources >= stressLeaveCost)
            {
                //remove condition
                if (task.data0 == playerID)
                {
                    actorName = GameManager.instance.playerScript.GetPlayerNameResistance();
                    actorType = "Player";
                    isSuccess = GameManager.instance.playerScript.RemoveCondition(conditionStressed, globalResistance, "Stress Leave");
                    text = string.Format("{0}, Player, takes Stress Leave", actorName);
                }
                else
                {
                    Actor actor = GameManager.instance.dataScript.GetActor(task.data0);
                    if (actor != null)
                    {
                        actorName = actor.actorName;
                        actorType = actor.arc.name;
                        isSuccess = actor.RemoveCondition(conditionStressed, "due to Stress Leave");
                        text = string.Format("{0}, {1}, takes Stress Leave", actorName, actor.arc.name);
                    }
                    else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data0); }
                }
                if (isSuccess == true)
                {
                    stressedActorID = -1;
                    GameManager.instance.messageScript.ActorStressLeave(text, task.data0, globalResistance);
                    //expend resources
                    resources -= stressLeaveCost;
                    if (resources < 0)
                    {
                        Debug.LogWarning("Not enough resources for Stress Leave");
                        resources = 0;
                    }
                    GameManager.instance.dataScript.SetAIResources(globalResistance, resources);
                    //statistic
                    GameManager.instance.dataScript.StatisticIncrement(StatType.StressLeaveResistance);
                    //expend action
                    UseAction(string.Format("give {0} Stress Leave", actorName));
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteStressLeaveTask: {0}, {1}, takes STRESS LEAVE, cost {2} resources{3}", actorName, actorType, stressLeaveCost, "\n");
                }
            }
            else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteStressLeaveTask: WARNING Insufficient resources {0} (need {1}) for Stress Leave{2}", resources, stressLeaveCost, "\n"); }
        }
        else { Debug.LogError("Invalid Task (null)"); }
    }

    /// <summary>
    /// actor/player executes an actor arc task. task.name0 is the actorArc name
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteActorArcTask(AITask task)
    {
        if (task != null)
        {
            switch (task.name0)
            {
                case "ANARCHIST":
                    ExecuteAnarchistTask(task);
                    break;
                case "BLOGGER":
                    ExecuteBloggerTask(task);
                    break;
                case "FIXER":
                    ExecuteFixerTask(task);
                    break;
                case "HACKER":
                    ExecuteHackerTask(task);
                    break;
                case "HEAVY":
                    ExecuteHeavyTask(task);
                    break;
                case "OBSERVER":
                    ExecuteObserverTask(task);
                    break;
                case "OPERATOR":
                    ExecuteOperatorTask(task);
                    break;
                case "PLANNER":
                    ExecutePlannerTask(task);
                    break;
                case "RECRUITER":
                    break;
                default:
                    Debug.LogErrorFormat("Unrecognised task.name0 \"{0}\"", task.name0);
                    break;
            }
        }
        else { Debug.LogError("Invalid Task (null)"); }
    }

    /// <summary>
    /// Fixer actorArc task execution
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteFixerTask(AITask task)
    {
        string actorName = "Unknown";
        string nodeName = "Unknown";
        string nodeArc = "Unknown";
        bool isPlayer = false;
        int nodeID = -1;
        Actor actor = null;
        //get node name
        Node node = GameManager.instance.dataScript.GetNode(task.data1);
        if (node != null)
        {
            nodeName = node.nodeName; nodeArc = node.Arc.name; nodeID = node.nodeID;
            //effect
            gearPool += task.data2;
            gearPool = Mathf.Min(gearPoolMaxSize, gearPool);
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteFixerTask: Fixer action, +{0} gearPoints, gearPool now {1} (max size {2}){3}", task.data2, gearPool, gearPoolMaxSize, "\n");
            //expend an action -> get actor Name
            if (task.data0 == playerID)
            {
                actorName = "Player";
                isPlayer = true;
                UpdateRenown();
            }
            else
            {
                actor = GameManager.instance.dataScript.GetActor(task.data0);
                if (actor != null)
                {
                    actorName = actor.actorName;
                    UpdateRenown(actor);
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data0); }
            }
            UseAction(string.Format("get Gear ({0}, FIXER at {1}, {2}, id {3})", actorName, nodeName, nodeArc, nodeID));
            //gear used to stay invisible (gear can be used but there is no deduction of gear points for the AI Fixer action)
            if (CheckGearAvailable(false) == false)
            {
                //invisibility drops
                if (isPlayer == true)
                { UpdateInvisibilityNode(node); }
                else { UpdateInvisibilityNode(node, actor); }
            }
            else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteFixerTask: GEAR USED to remain undetected while getting gear (gearPoint NOT lost)"); }
        }
        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", task.data1); }
    }

    /// <summary>
    /// Planner actorArc task execution
    /// </summary>
    /// <param name="task"></param>
    private void ExecutePlannerTask(AITask task)
    {
        string actorName = "Unknown";
        string nodeName = "Unknown";
        string nodeArc = "Unknown";
        bool isPlayer = false;
        int nodeID = -1;
        Actor actor = null;
        //get node name
        Node node = GameManager.instance.dataScript.GetNode(task.data1);
        if (node != null)
        {
            nodeName = node.nodeName; nodeArc = node.Arc.name; nodeID = node.nodeID;
            //effect
            int newIntel = task.data2;
            if (Random.Range(0, 100) < targetIntelExtra) { newIntel++; }
            targetIntel += newIntel;
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecutePlannerTask: Planner action, +{0} targetIntel, targetIntel now {1}{2}", newIntel, targetIntel, "\n");
            //expend an action -> get actor Name
            if (task.data0 == playerID)
            {
                actorName = "Player";
                isPlayer = true;
                UpdateRenown();
            }
            else
            {
                actor = GameManager.instance.dataScript.GetActor(task.data0);
                if (actor != null)
                {
                    actorName = actor.actorName;
                    UpdateRenown(actor);
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data0); }
            }
            UseAction(string.Format("get target Intel ({0}, PLANNER at {1}, {2}, id {3})", actorName, nodeName, nodeArc, nodeID));
            //gear used to stay invisible
            if (CheckGearAvailable(false) == false)
            {
                //invisibility drops
                if (isPlayer == true)
                { UpdateInvisibilityNode(node); }
                else { UpdateInvisibilityNode(node, actor); }
            }
            else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecutePlannerTask: GEAR USED to remain undetected while acquiring targetIntel (gearPoint NOT lost)"); }
        }
        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", task.data1); }
    }

    /// <summary>
    /// Blogger actorArc task execution
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteBloggerTask(AITask task)
    {
        string actorName = "Unknown";
        string actorArcName = "Unknown";
        string nodeName = "Unknown";
        string nodeArc = "Unknown";
        bool isPlayer = false;
        int nodeID = -1;
        Actor actor = null;
        //get node
        Node node = GameManager.instance.dataScript.GetNode(task.data1);
        if (node != null)
        {
            nodeName = node.nodeName;
            nodeArc = node.Arc.name;
            nodeID = node.nodeID;
            //effect
            node.Support++;
            Debug.LogFormat("[Nod] AIRebelManager.cs -> ExecuteBloggerTask: {0}, {1}, ID {2}, Support now {3} (changed by +1){4}", nodeName, nodeArc, nodeID, node.Support, "\n");
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteBloggerTask: Blogger action, nodeID {0} Support {1}{2}", node.nodeID, node.Support, "\n");

            //expend an action -> get actor Name
            if (task.data0 == playerID)
            {
                actorName = playerName;
                actorArcName = "Player";
                isPlayer = true;
                UpdateRenown();
            }
            else
            {
                actor = GameManager.instance.dataScript.GetActor(task.data0);
                if (actor != null)
                {
                    actorName = actor.actorName;
                    actorArcName = actor.arc.name;
                    UpdateRenown(actor);
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data0); }
            }
            //expend action
            UseAction(string.Format("increase Support ({0}, {1} at {2}, {3}, id {4})", actorName, actorArcName, nodeName, nodeArc, nodeID));
            //gear used to stay invisible 
            if (CheckGearAvailable() == false)
            {
                //invisibility drops
                if (isPlayer == true)
                { UpdateInvisibilityNode(node); }
                else { UpdateInvisibilityNode(node, actor); }
            }
            else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteBloggerTask: GEAR USED to remain undetected while increasing district Support"); }
        }
        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", task.data1); }
    }

    /// <summary>
    /// Anarchist actorArc task execution
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteAnarchistTask(AITask task)
    {
        string actorName = "Unknown";
        string actorArcName = "Unknown";
        string nodeName = "Unknown";
        string nodeArc = "Unknown";
        bool isPlayer = false;
        int nodeID = -1;
        Actor actor = null;
        //get node
        Node node = GameManager.instance.dataScript.GetNode(task.data1);
        if (node != null)
        {
            nodeName = node.nodeName;
            nodeArc = node.Arc.name;
            nodeID = node.nodeID;
            //effect
            node.Stability--;
            Debug.LogFormat("[Nod] AIRebelManager.cs -> ExecuteAnarchistTask: {0}, {1}, ID {2}, Stability now {3} (changed by -1){4}", nodeName, nodeArc, nodeID, node.Stability, "\n");
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteAnarchistTask: Anarchist action, nodeID {0} Stability {1}{2}", node.nodeID, node.Stability, "\n");
            //expend an action -> get actor Name
            if (task.data0 == playerID)
            {
                actorName = playerName;
                actorArcName = "Player";
                isPlayer = true;
                UpdateRenown();
            }
            else
            {
                actor = GameManager.instance.dataScript.GetActor(task.data0);
                if (actor != null)
                {
                    actorName = actor.actorName;
                    actorArcName = actor.arc.name;
                    UpdateRenown(actor);
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data0); }
            }
            //expend action
            UseAction(string.Format("decrease Stability ({0}, {1} at {2}, {3}, id {4})", actorName, actorArcName, nodeName, nodeArc, nodeID));
            //gear used to stay invisible 
            if (CheckGearAvailable() == false)
            {
                //invisibility drops
                if (isPlayer == true)
                { UpdateInvisibilityNode(node); }
                else { UpdateInvisibilityNode(node, actor); }
            }
            else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteAnarchistTask: GEAR USED to remain undetected while decreasing district Stability"); }
        }
        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", task.data1); }
    }

    /// <summary>
    /// Observer actorArc task execution
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteObserverTask(AITask task)
    {
        string actorName = "Unknown";
        string actorArcName = "Unknown";
        string nodeName = "Unknown";
        string nodeArc = "Unknown";
        bool isPlayer = false;
        int nodeID = -1;
        Actor actor = null;
        //get node
        Node node = GameManager.instance.dataScript.GetNode(task.data1);
        if (node != null)
        {
            nodeName = node.nodeName;
            nodeArc = node.Arc.name;
            nodeID = node.nodeID;
            //effect
            node.AddTracer();
            if (node.isSpider == true) { node.isSpiderKnown = true; }
            Debug.LogFormat("[Nod] AIRebelManager.cs -> ExecuteObserverTask: {0}, {1}, ID {2}, TRACER inserted{3}", nodeName, nodeArc, nodeID, "\n");
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteObserverTask: Observer action, nodeID {0} TRACER inserted{1}", node.nodeID, "\n");
            //expend an action -> get actor Name
            if (task.data0 == playerID)
            {
                actorName = playerName;
                actorArcName = "Player";
                isPlayer = true;
                UpdateRenown();
            }
            else
            {
                actor = GameManager.instance.dataScript.GetActor(task.data0);
                if (actor != null)
                {
                    actorName = actor.actorName;
                    actorArcName = actor.arc.name;
                    UpdateRenown(actor);
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data0); }
            }
            //expend action
            UseAction(string.Format("insert Tracer ({0}, {1} at {2}, {3}, id {4})", actorName, actorArcName, nodeName, nodeArc, nodeID));
            //gear used to stay invisible 
            if (CheckGearAvailable() == false)
            {
                //invisibility drops
                if (isPlayer == true)
                { UpdateInvisibilityNode(node); }
                else { UpdateInvisibilityNode(node, actor); }
            }
            else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteObserverTask: GEAR USED to remain undetected while decreasing district Stability"); }
        }
        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", task.data1); }
    }

    /// <summary>
    /// Operator actorArc task execution
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteOperatorTask(AITask task)
    {
        string actorName = "Unknown";
        string actorArcName = "Unknown";
        string nodeName = "Unknown";
        string nodeArc = "Unknown";
        string teamArcName = "Unknown";
        bool isPlayer = false;
        int nodeID = -1;
        Actor actor = null;
        //get node
        Node node = GameManager.instance.dataScript.GetNode(task.data1);
        if (node != null)
        {
            nodeName = node.nodeName;
            nodeArc = node.Arc.name;
            nodeID = node.nodeID;
            //effect
            teamArcName = node.RemoveTeamAI();
            Debug.LogFormat("[Tea] AIRebelManager.cs -> ExecuteOperatorTask: {0}, {1}, ID {2}, {3} team Removed{4}", nodeName, nodeArc, nodeID, teamArcName, "\n");
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteOperatorTask: Observer action, nodeID {0}, {1} team Removed{2}", node.nodeID, teamArcName, "\n");
            //expend an action -> get actor Name
            if (task.data0 == playerID)
            {
                actorName = playerName;
                actorArcName = "Player";
                isPlayer = true;
                UpdateRenown();
            }
            else
            {
                actor = GameManager.instance.dataScript.GetActor(task.data0);
                if (actor != null)
                {
                    actorName = actor.actorName;
                    actorArcName = actor.arc.name;
                    UpdateRenown(actor);
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data0); }
            }
            //expend action
            UseAction(string.Format("neutralise Team ({0}, {1} at {2}, {3}, id {4})", actorName, actorArcName, nodeName, nodeArc, nodeID));
            //gear used to stay invisible 
            if (CheckGearAvailable() == false)
            {
                //invisibility drops
                if (isPlayer == true)
                { UpdateInvisibilityNode(node); }
                else { UpdateInvisibilityNode(node, actor); }
            }
            else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteOperatorTask:  OPERATOR unable to remain undetected while neutralising team"); }
        }
        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", task.data1); }
    }

    /// <summary>
    /// Hacker actorArc task execution
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteHackerTask(AITask task)
    {
        string actorName = "Unknown";
        string actorArcName = "Unknown";
        string nodeName = "Unknown";
        string nodeArc = "Unknown";
        bool isPlayer = false;
        int nodeID = -1;
        Actor actor = null;
        //get node
        Node node = GameManager.instance.dataScript.GetNode(task.data1);
        if (node != null)
        {
            nodeName = node.nodeName;
            nodeArc = node.Arc.name;
            nodeID = node.nodeID;
            node.Security--;
            Debug.LogFormat("[Nod] AIRebelManager.cs -> ExecuteHackerTask: {0}, {1}, ID {2}, Security now {3} (changed by -1){4}", nodeName, nodeArc, nodeID, node.Security, "\n");
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteHackerTask: Hacker action, nodeID {0} Security {1}{2}", node.nodeID, node.Security, "\n");

            //expend an action -> get actor Name
            if (task.data0 == playerID)
            {
                actorName = playerName;
                actorArcName = "Player";
                isPlayer = true;
                UpdateRenown();
            }
            else
            {
                actor = GameManager.instance.dataScript.GetActor(task.data0);
                if (actor != null)
                {
                    actorName = actor.actorName;
                    actorArcName = actor.arc.name;
                    UpdateRenown(actor);
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data0); }
            }
            //expend action
            UseAction(string.Format("decrease Security ({0}, {1} at {2}, {3}, id {4})", actorName, actorArcName, nodeName, nodeArc, nodeID));
            //gear used to stay invisible 
            if (CheckGearAvailable() == false)
            {
                //invisibility drops
                if (isPlayer == true)
                { UpdateInvisibilityNode(node); }
                else { UpdateInvisibilityNode(node, actor); }
            }
            else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteHackerTask: GEAR USED to remain undetected while increasing district Support"); }
        }
        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", task.data1); }
    }

    /// <summary>
    /// Heavy actorArc task execution
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteHeavyTask(AITask task)
    {
        string actorName = "Unknown";
        string actorArcName = "Unknown";
        string nodeName = "Unknown";
        string nodeArc = "Unknown";
        bool isPlayer = false;
        int nodeID = -1;
        int counter = 0;
        Actor actor = null;
        //get node
        Node node = GameManager.instance.dataScript.GetNode(task.data1);
        if (node != null)
        {
            nodeName = node.nodeName;
            nodeArc = node.Arc.name;
            nodeID = node.nodeID;
            //effect -> Current and all neighbouring nodes have their Stability reduced by 1
            List<Node> listOfNeighbouringNodes = node.GetNeighbouringNodes();
            if (listOfNeighbouringNodes != null)
            {
                //neighbouring nodes 
                foreach (Node nearNode in listOfNeighbouringNodes)
                {
                    if (nearNode.Stability > 0)
                    {
                        nearNode.Stability--;
                        counter++;
                        Debug.LogFormat("[Nod] AIRebelManager.cs -> ExecuteHeavyTask: {0}, {1}, ID {2}, Stability now {3} (changed by -1){4}", nearNode.nodeName, nearNode.Arc.name, nearNode.nodeID, 
                            nearNode.Stability, "\n");
                    }
                }
                //current node
                if (node.Stability > 0)
                {
                    node.Stability--;
                    counter++;
                    Debug.LogFormat("[Nod] AIRebelManager.cs -> ExecuteHeavyTask: {0}, {1}, ID {2}, Stability now {3} (changed by -1){4}", nodeName, nodeArc, nodeID, node.Stability, "\n");
                }
                Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteHeavyTask: Heavy action, {0} districts have had their Stability reduced by 1{1}", counter, "\n");
                //expend an action -> get actor Name
                if (task.data0 == playerID)
                {
                    actorName = playerName;
                    actorArcName = "Player";
                    isPlayer = true;
                    UpdateRenown();
                }
                else
                {
                    actor = GameManager.instance.dataScript.GetActor(task.data0);
                    if (actor != null)
                    {
                        actorName = actor.actorName;
                        actorArcName = actor.arc.name;
                        UpdateRenown(actor);
                    }
                    else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data0); }
                }
                //expend action
                UseAction(string.Format("decrease Stability in neighbouring Nodes ({0}, {1} at {2}, {3}, id {4})", actorName, actorArcName, nodeName, nodeArc, nodeID));
                //gear used to stay invisible 
                if (CheckGearAvailable() == false)
                {
                    //invisibility drops
                    if (isPlayer == true)
                    { UpdateInvisibilityNode(node); }
                    else { UpdateInvisibilityNode(node, actor); }
                }
                else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteHeavyTask: GEAR USED to remain undetected while decreasing neighbouring district Stability"); }
            }
            else { Debug.LogErrorFormat("Invalid listOfNeighbouring Nodes (Null) for nodeID {0}", node.nodeID); }
        }
        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", task.data1); }
    }

    /// <summary>
    /// Player or Actor attempts a target
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteTargetTask(AITask task)
    {

    }

    /// <summary>
    /// do nothing task
    /// </summary>
    private void ExecuteIdleTask(AITask task)
    {
        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessIdleTask: \"{0}\", id {1} is IDLING at node ID {2}{3}", playerName, task.data1, task.data0, "\n");
        Debug.LogFormat("[Tst] AIRebelManager.cs -> ProcessIdleTask: IDLING{0}", "\n");
        //action
        UseAction("Idle (Player)");
    }


    //
    // - - - Utility - - -
    //

    /// <summary>
    /// increment renown (actors) and resources (player) as a result of taking an action. Leave actor as Null for Player.
    /// </summary>
    /// <param name="actor"></param>
    private void UpdateRenown(Actor actor = null)
    {
        if (actor == null)
        {
            //Player -> Resources
            int resources = GameManager.instance.dataScript.CheckAIResourcePool(globalResistance);
            resources++;
            GameManager.instance.dataScript.SetAIResources(globalResistance, resources);
        }
        else
        {
            //Actor
            actor.Renown++;
        }
    }

    /// <summary>
    /// submethod that handles Player invisibility loss for ExecuteMoveTask
    /// NOTE: connection and node checked for null by parent method. Also it's assumed that security is > 'None'
    /// </summary>
    /// <param name="secLevel"></param>
    private void UpdateInvisibilityMove(Connection connection, Node node)
    {
        int changeInvisibility = 0;
        int aiPlayerInvisibility = GameManager.instance.playerScript.Invisibility;
        int aiDelay = 0;
        switch (connection.SecurityLevel)
        {
            case ConnectionType.HIGH:
                changeInvisibility = 1;
                aiDelay = 1;
                break;
            case ConnectionType.MEDIUM:
                changeInvisibility = 1;
                aiDelay = 2;
                break;
            case ConnectionType.LOW:
                changeInvisibility = 1;
                aiDelay = 3;
                break;
            default:
                Debug.LogWarningFormat("Invalid secLevel (Unrecognised) \"{0}\"", connection.SecurityLevel);
                break;
        }
        //update invisibility
        aiPlayerInvisibility -= changeInvisibility;
        //calculate AI delay
        if (changeInvisibility != 0)
        {
            //min cap 0
            aiPlayerInvisibility = Mathf.Max(0, aiPlayerInvisibility);
            Debug.LogFormat("[Rim] AIRebelManager.cs -> UpdateInvisibility: Invisibility -1, now {0}{1}", aiPlayerInvisibility, "\n");
            //update player invisibility
            GameManager.instance.playerScript.Invisibility = aiPlayerInvisibility;
            //message
            string text = string.Format("AI Resistance Player has moved to {0}, {1}, id {2}, Invisibility now {3}", node.nodeName, node.Arc.name, node.nodeID, aiPlayerInvisibility);
            GameManager.instance.messageScript.PlayerMove(text, node, changeInvisibility, aiDelay);
            //AI message
            string textAI = string.Format("Player spotted moving to \"{0}\", {1}, ID {2}", node.nodeName, node.Arc.name, node.nodeID);
            if (aiDelay == 0)
            {
                //moving while invisibility already 0 triggers immediate alert flag
                GameManager.instance.aiScript.immediateFlagResistance = true;
                //AI Immediate notification
                GameManager.instance.messageScript.AIImmediateActivity("Immediate Activity \"Move\" (AI Player)", "Moving", node.nodeID, connection.connID);
            }
            else
            {
                //AI delayed notification
                GameManager.instance.messageScript.AIConnectionActivity(textAI, node, connection, aiDelay);
            }
        }
    }

    /// <summary>
    /// Updates player or actor invisibility for a node action. Actor is null for Player. Handles AI activity notifications
    /// NOTE: both node and actor are checked for null by the calling method, actor is deliberately null to indicae the Player
    /// </summary>
    /// <param name="nodeID"></param>
    private void UpdateInvisibilityNode(Node node, Actor actor = null)
    {
        int delay = -1;
        int actorID = -1; //player/actorID, used for AI activity notification
        int aiInvisibility;
        string actorName = "Unknown";
        string actorArc = "Unknown";
        //
        // - - - Player - - -
        //
        if (actor == null)
        {
            actorID = playerID;
            actorName = playerName;
            actorArc = "Player";
            aiInvisibility = GameManager.instance.playerScript.Invisibility;
            //player did an action with invisibility zero
            if (aiInvisibility == 0)
            {
                //delay of Zero
                delay = 0;
                //immediate flag true
                GameManager.instance.aiScript.immediateFlagResistance = true;
            }
            aiInvisibility -= 1;
            //double loss of invisibility if spider present
            if (node.isSpider == true)
            { aiInvisibility -= 1; }
            //min cap zero
            aiInvisibility = Mathf.Max(0, aiInvisibility);
            //update player Invisibility
            GameManager.instance.playerScript.Invisibility = aiInvisibility;
            //capture check
            if (aiInvisibility == 0)
            {
                //Erasure team picks up player immediately if invisibility 0
                CaptureDetails captureDetails = GameManager.instance.captureScript.CheckCaptured(node.nodeID, playerID);
                if (captureDetails != null)
                {
                    //Player captured!
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> UpdateInvisibilityNode: {0}, Player, CAPTURED by Erasure Team at {1}, {2}, ID {3}{4}", playerName, node.nodeName, node.Arc.name, node.nodeID, "\n");
                    captureDetails.effects = "They kicked in the door before you could run";
                    EventManager.instance.PostNotification(EventType.Capture, this, captureDetails, "NodeManager.cs -> ProcessMoveOutcome");
                }
            }
        }
        //
        // - - - Actor - - -
        //
        else
        {
            actorID = actor.actorID;
            actorName = actor.actorName;
            actorArc = actor.arc.name;
            aiInvisibility = actor.datapoint2;
            //actor did an action with invisibility zero
            if (aiInvisibility == 0)
            {
                //delay of Zero
                delay = 0;
                //immediate flag true
                GameManager.instance.aiScript.immediateFlagResistance = true;
            }
            aiInvisibility -= 1;
            Debug.LogFormat("[Rim] AIRebelManager.cs -> UpdateInvisibilityNode: {0}, {1}, ID {2}, invisibility -1, now {3}{4}", actorName, actorArc, actorID, aiInvisibility, "\n");
            //double loss of invisibility if spider present
            if (node.isSpider == true)
            {
                aiInvisibility -= 1;
                Debug.LogFormat("[Rim] AIRebelManager.cs -> UpdateInvisibilityNode: {0}, {1}, ID {2}, invisibility -1 (SPIDER), now {3}{4}", actorName, actorArc, actorID, aiInvisibility, "\n");
            }
            //min cap zero
            aiInvisibility = Mathf.Max(0, aiInvisibility);
            //update actor Invisibility
            actor.datapoint2 = aiInvisibility;
            //capture check
            if (aiInvisibility == 0)
            {
                //Erasure team picks up actor immediately if invisibility 0
                CaptureDetails captureDetails = GameManager.instance.captureScript.CheckCaptured(node.nodeID, actorID);
                if (captureDetails != null)
                {
                    //actor captured!
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> UpdateInvisibilityNode: {0}, {1}, ID {2}, CAPTURED by Erasure Team at {3}, {4}, ID {5}{6}", actorName, actorArc, actorID,
                        node.nodeName, node.Arc.name, node.nodeID, "\n");
                    captureDetails.effects = "They got you";
                    EventManager.instance.PostNotification(EventType.Capture, this, captureDetails, "NodeManager.cs -> ProcessMoveOutcome");
                }
            }
        }
        //
        // - - - Notification of Resistance activity - - -
        //
        if (delay < 0)
        {
            if (node.isSpider == true) { delay = delayYesSpider; }
            else { delay = delayNoSpider; }
        }
        string text = string.Format("Resistance Activity \"{0}\" ({1})", actorArc, actorName);
        GameManager.instance.messageScript.AINodeActivity(text, node, actorID, delay);
        //AI Immediate message
        if (GameManager.instance.aiScript.immediateFlagResistance == true)
        {
            text = string.Format("Immediate Activity \"{0}\" ({1})", actorArc, actorName);
            string reason = "district action";
            GameManager.instance.messageScript.AIImmediateActivity(text, reason, node.nodeID, -1, actorID);
        }
    }

    /// <summary>
    /// Action used, message to that effect. Reason in format 'Action used to ... [reason]'
    /// </summary>
    /// <param name="reason"></param>
    private void UseAction(string reason)
    {
        actionsUsed++;
        if (string.IsNullOrEmpty(reason) == false)
        { Debug.LogFormat("[Rim] AIRebelManager.cs -> UseAction: ACTION used to {0}{1}", reason, "\n"); }
        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> UseAction: ACTION for Unknown reason{0}", "\n"); }
    }


    /// <summary>
    /// determines whether it is the Player who will carry out the ActorArc action. Player's node needs to be not bad and needs to pass a test. 
    /// will check if the player's current node will allow this actorArcName action based on the actorArc criteria, eg. Blogger needs node support < max
    /// </summary>
    /// <returns></returns>
    private bool CheckPlayerAction(string actorArcName)
    {
        int rnd = -1;
        int playerNodeID = GameManager.instance.nodeScript.nodePlayer;
        bool isPlayer = false;
        string reasonNot = "Unknown";
        if (string.IsNullOrEmpty(actorArcName) == false)
        {
            //not a bad node
            if (CheckForBadNode(playerNodeID) == false)
            {
                rnd = Random.Range(0, 100);
                //pass a random check
                if (rnd < playerAction)
                {
                    Node node = GameManager.instance.dataScript.GetNode(playerNodeID);
                    if (node != null)
                    {
                        if (CheckNodeCriteria(actorArcName, node) == true)
                        { isPlayer = true; }
                        else { reasonNot = "does not meet CRITERIA"; }
                    }
                    else { Debug.LogErrorFormat("Invalid nodeID (Null) for playerNodeID {0}", playerNodeID); }

                }
                else { reasonNot = "Failed Roll"; }
            }
            else { reasonNot = "Bad Node"; }
        }
        else { Debug.LogError("Invalid actorArcName (Null)"); reasonNot = "Error"; }
        if (isPlayer == true)
        { Debug.LogFormat("[Rim] AIRebelManager.cs -> CheckPlayerAction: Player will do {0} Action (needed {1}, rolled {2}){3}", actorArcName, playerAction, rnd, "\n"); }
        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> CheckPlayerAction: Player will not do {0} Action because {1}{2}", actorArcName, reasonNot, "\n"); }
        return isPlayer;
    }

    /// <summary>
    /// Checks onMap actors to see if that actorArc is present, if so returns actorID, if not randomly selects one onMap actor and returns their actorID. Returns -1 if a problem
    /// </summary>
    /// <param name="arc"></param>
    /// <returns></returns>
    private int FindActor(ActorArc arc)
    {
        //is there an actor of this type present and Active
        int actorID = GameManager.instance.dataScript.CheckActorPresent(arc.ActorArcID, globalResistance);
        if (actorID < 0)
        {
            //select a random onMap, Active, actor
            Actor actor = listOfCurrentActors[Random.Range(0, listOfCurrentActors.Count)];
            if (actor != null)
            { actorID = actor.actorID; }
            else { Debug.LogError("Invalid randomly selected Actor (Null)"); }
        }
        return actorID;
    }

    /// <summary>
    /// Finds a suitable node (randomly selected) that is part of the specified actor's contact network, returns -1 if a problem
    /// </summary>
    /// <param name="actorID"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    private int FindNodeRandom(int actorID)
    {
        int count;
        int nodeID = -1;
        //get list of all nodes where actor has an active contact. 
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfActorContacts(actorID);
        List<int> listOfNodeID = new List<int>();
        count = listOfNodes.Count();
        if (count > 0)
        {
            //weed out any bad nodes
            int tempNodeID;
            for (int i = 0; i < count; i++)
            {
                tempNodeID = listOfNodes[i].nodeID;
                if (CheckForBadNode(tempNodeID) == false)
                { listOfNodeID.Add(tempNodeID); }
            }
            //any suitable nodes
            count = listOfNodeID.Count;
            if (count > 0)
            {
                //select random node
                nodeID = listOfNodeID[Random.Range(0, count)];
            }
        }
        else { Debug.LogWarningFormat("Invalid listOfNodes (Empty) for actorID {0}", actorID); }
        return nodeID;
    }


    /// <summary>
    /// Finds a suitable node (randomly selected) that meets the actor Arc criteria and attempts to match with correct actor (if not then random) and returns a tuple of nodeID (item1) and actorID (item2)
    /// if a problem returns -1 for both
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>
    private Tuple<int, int>  FindNodeActorRandom(string actorArcName)
    {
        int nodeID = -1;
        int actorID = -1;
        int tempNodeID;
        int count;
        List<Node> listOfContactNodes = new List<Node>();
        List<int> listOfSuitableNodes = new List<int>();
        List<int> listOfActorID = new List<int>();
        //get a list of all contacts nodeID's from active, onMap, actors 
        for (int i = 0; i < listOfCurrentActors.Count; i++)
        {
            //get actor network contact nodes
            List<Node> tempList = GameManager.instance.dataScript.GetListOfActorContacts(listOfCurrentActors[i].actorID);
            if (tempList != null)
            {
                //add to list (by Value)
                listOfContactNodes.AddRange(tempList);
                //add actorID to list
                listOfActorID.Add(listOfCurrentActors[i].actorID);
            }
            else { Debug.LogErrorFormat("Invalid listOfContactNodes (Null) for actorID {0}", listOfCurrentActors[i].actorID); }
        }
        //do we have a viable list
        count = listOfContactNodes.Count();
        Debug.LogFormat("[Rim] AIRebelManager.cs -> FindNodeActorRandom: listOfContactNodes has {0} records{1}", count, "\n");
        if (count > 0)
        {
            /*DebugShowList(listOfActorID);*/

            //take listOfActorID's and randomly shuffle them
            ShuffleList(listOfActorID);

            /*Debug.LogFormat("[Tst] AFTER SHUFFLE{0}", "\n");
            DebugShowList(listOfActorID);*/

            //loop nodes and find those that match the actorArc criteria
            for (int i = 0; i < count; i++)
            {
                Node node = listOfContactNodes[i];
                if (node != null)
                {
                    if (CheckNodeCriteria(actorArcName, node) == true)
                    {
                        //if not already on list, add to list of nodes that meet the criteria
                        if (listOfSuitableNodes.Exists(x => x == node.nodeID) == false)
                        { listOfSuitableNodes.Add(node.nodeID); }
                    }
                }
                else { Debug.LogErrorFormat("Invalid node (Null) for listOfContactNodes[{0}]", i); }
            }
        }
        //are there any suitable nodes -> if not return default values of -1 and ignore this actorArc task
        count = listOfSuitableNodes.Count();
        List<int> listOfActorsAtNode = new List<int>();
        if (count > 0)
        {
            ShuffleList(listOfSuitableNodes);

            /*Debug.LogFormat("[Tst] AFTER SHUFFLE{0}", "\n");
            DebugShowList(listOfSuitableNodes);*/

            //see if you can match a node to an onMap actor
            for (int i = 0; i < count; i++)
            {
                tempNodeID = listOfSuitableNodes[i];
                int actorToFind;
                //already null checked, get list of actorID's who have contacts at node
                listOfActorsAtNode = GameManager.instance.dataScript.CheckContactResistanceAtNode(tempNodeID);
                if (listOfActorsAtNode != null)
                {
                    //look for a match with any OnMap, active, Actor. Take the first you get (listOfActorID has been randomly shuffled so no actor is favoured)
                    for (int index = 0; index < listOfActorsAtNode.Count; index++)
                    {
                        actorToFind = listOfActorsAtNode[index];
                        if (listOfActorID.Exists(x => x == actorToFind) == true)
                        {
                            actorID = actorToFind;
                            nodeID = tempNodeID;
                            break;
                        }
                    }
                }
                //break out of loop if you found a match already
                if (actorID > -1 && nodeID > -1)
                {
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> FindNodeActorRandom: Match found, actorID {0}, nodeID {1}{2}", actorID, nodeID, "\n");
                    break;
                }
            }
        }
        return new Tuple<int, int>(nodeID, actorID);
    }

    /// <summary>
    /// Debug method to display contents of a list
    /// </summary>
    /// <param name="tempList"></param>
    private void DebugShowList(List<int> tempList)
    {
        for (int i = 0; i < tempList.Count; i++)
        { Debug.LogFormat("[Tst] tempList[{0}] -> {1}{2}", i, tempList[i], "\n"); }
    }

    /// <summary>
    /// returns true if node meets actor arc criteria, false otherwise. Called by FindNodeActorRandom and CheckPlayerAction
    /// NOTE: both paramaters are checked for null by the calling methods
    /// </summary>
    /// <param name="actorArcName"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    private bool CheckNodeCriteria(string actorArcName, Node node)
    {
        bool isValid = false;
        switch (actorArcName)
        {
            case "BLOGGER":
                //Node support must be < max & no Media team present
                if (node.Support < maxStatValue && node.isSupportTeam == false)
                { isValid = true; }
                break;
            case "ANARCHIST":
                //Node stability must be > 0 & no Civil team present
                if (node.Stability > 0 && node.isStabilityTeam == false)
                { isValid = true; }
                break;
            case "HACKER":
                //Node security must be > 0 & no Control team present
                if (node.Security > 0 && node.isSecurityTeam == false)
                { isValid = true; }
                break;
            case "FIXER":
                //can be any node
                isValid = true;
                break;
            case "PLANNER":
                //can be any node
                isValid = true;
                break;
            case "OBSERVER":
                //no tracer present and no Spider TEAM present 
                if (node.isTracer == false && node.isSpiderTeam == false)
                { isValid = true; }
                break;
            case "OPERATOR":
                if (node.CheckNumOfTeams() > 0)
                { isValid = true; }
                break;
            case "HEAVY":
                //Node stability must be One or less and no Civil team present
                if (node.Stability <= 1 && node.isStabilityTeam == false)
                {
                    //a least one neighbouring node must have stability > 0
                    List<Node> listOfNeighbouringNodes = node.GetNeighbouringNodes();
                    if (listOfNeighbouringNodes != null)
                    {
                        foreach (Node nearNode in listOfNeighbouringNodes)
                        {
                            if (nearNode.Stability > 0)
                            { isValid = true; break; }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid listOfNeighbouring nodes (Null) for nodeID {0)", node.nodeID); }
                }
                break;
            default:
                Debug.LogErrorFormat("Unrecognised actorArcName \"{0}\"", actorArcName);
                break;
        }
        return isValid;
    }

    /// <summary>
    /// method to randomly shuffle a list of int's. Uses the Fisher-Yates algorithm
    /// </summary>
    /// <param name="tempList"></param>
    private void ShuffleList(List<int> tempList)
    {
        if (tempList != null)
        {
            int rnd;
            int mem;
            int count = tempList.Count;
            while (count > 1)
            {
                count--;
                rnd = Random.Range(0, count + 1);
                mem = tempList[rnd];
                tempList[rnd] = tempList[count];
                tempList[count] = mem;
            }
        }
        else { Debug.LogError("Invalid tempList (Null)"); }
    }

    //
    // - - - Gear - - -
    //

    /// <summary>
    /// returns true if a suitable piece of gear is available for the specified task. Chance is proportional to the number of gear points in the pool, more the points, greater the chance
    /// If availabe will deduct a gear point from the gear pool as it's assumed that the gear will be used (unless isDeductGearPoint set false)
    /// </summary>
    /// <returns></returns>
    private bool CheckGearAvailable(bool isDeductGearPoint = true)
    {
        bool isAvailable = false;
        int rnd = Random.Range(0, 100);
        int threshold = -1;
        switch (gearPool)
        {
            case 6: threshold = gearAvailableSix; break;
            case 5: threshold = gearAvailableFive; break;
            case 4: threshold = gearAvailableFour; break;
            case 3: threshold = gearAvailableThree; break;
            case 2: threshold = gearAvailableTwo; break;
            case 1: threshold = gearAvailableOne; break;
            case 0: threshold = -1; break;
            default:
                Debug.LogErrorFormat("Inrecognised gearPool \"{0}\"", gearPool);
                break;
        }
        if (rnd < threshold)
        {
            isAvailable = true;
            if (isDeductGearPoint == true)
            {
                //deduct one gear point from gear pool (automatically used, no option to recover or retain gear)
                gearPool--;
                gearPointsUsed++;
                Debug.LogFormat("[Rim] AIRebelManager.cs -> CheckGearAvailable: Gear is AVAILABLE, need < {0}, rolled {1} (Gear Pool now {2}){3}", threshold, rnd, gearPool, "\n");
            }
        }
        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> CheckGearAvailable: Gear is NOT Available, need < {0}, rolled {1} (Gear Pool {2}){3}", threshold, rnd, gearPool, "\n"); }
        
        if (threshold > 0)
        {
            //renown/resource point expended (random chance regardless of whether gear is available or not)
            rnd = Random.Range(0, 100);
            if (rnd < gearRenownChance)
            {
                //deduct one resource but cannot go below zero
                int resources = GameManager.instance.dataScript.CheckAIResourcePool(globalResistance);
                if (resources > 0)
                {
                    resources--;
                    GameManager.instance.dataScript.SetAIResources(globalResistance, resources);
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> CheckGearAvailable: 1 resource expended on Gear retention (need {0}, rolled {1}){2}", gearRenownChance, rnd, "\n");
                }
            }
        }
        return isAvailable;
    }

    /// <summary>
    /// returns the number of gear items to delete from the pools upon reverting to a human resistance player. Calculated at half gear points used, rounded down, plus one
    /// </summary>
    /// <returns></returns>
    public int GetGearUsedAdjusted()
    { return gearPointsUsed / 2 + 1; }

    /// <summary>
    /// returns the number of gear points within the gear Pool
    /// </summary>
    /// <returns></returns>
    public int GetGearPoints()
    { return gearPool; }

    //
    // - - - Tidy up - - -
    //


    /// <summary>
    /// restore connections back to original state (NOTE: recalculate dijkstra data done at end of all AIRebel processing) prior to leaving AIRebelManager.cs
    /// </summary>
    private void RestoreConnections()
    {
        GameManager.instance.connScript.RestoreConnections();
        Debug.LogFormat("[Rim] AIRebelManager.cs -> RestoreConnections: Connections Restored to original state prior to AIRebelManager.cs calc's{0}", "\n");
    }

    /// <summary>
    /// resets dijktra weighted calculations back to normal ready for AIManager.cs
    /// </summary>
    private void RestoreDijkstraCalculations()
    {
        //recalculate weighted data
        GameManager.instance.dijkstraScript.RecalculateWeightedData();
        isConnectionsChanged = false;
    }

    /// <summary>
    /// returns true if specific actor arc ID present in listOfArcs (the actor arcs chosen for this turns actions)
    /// </summary>
    /// <param name="actorArcID"></param>
    /// <returns></returns>
    private bool CheckActorArcPresent(int actorArcID)
    { return listOfArcs.Exists(x => x.ActorArcID == actorArcID); }

    /// <summary>
    /// returns actorID of any OnMap, active, actor of the same type. If none, returns a random onMap, active, actor. Returns -1 if a problem
    /// </summary>
    /// <param name="actorArcID"></param>
    /// <returns></returns>
    private int GetOnMapActor(int actorArcID)
    {
        int actorID = -1;
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
        List<int> listOfActiveActorID = new List<int>();
        if (arrayOfActors != null)
        {
            //loop actors
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                //check actor is present in slot (not vacant)
                if (GameManager.instance.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
                {
                    Actor actor = arrayOfActors[i];
                    if (actor != null)
                    {
                        //Active actor
                        if (actor.Status == ActorStatus.Active)
                        {
                            if (actor.arc.ActorArcID == actorArcID)
                            { actorID = actor.actorID; break; }
                            else
                            {
                                //add live actor 
                                listOfActiveActorID.Add(actor.actorID);
                            }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid actor (Null) for arrayOfActors[{0}]", i); }
                }
            }
            //no actor of required type chosen
            if (actorID == -1)
            {
                //select a random actor from list of OnMap, Active, actors
                if (listOfActiveActorID.Count > 0)
                { actorID = listOfActiveActorID[Random.Range(0, listOfActiveActorID.Count)]; }
                else { Debug.LogError("Invalid listOfActiveActorID (no records)"); }
            }
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }
        return actorID;
    }

    //
    // - - -  Debug - - -
    //


    public int GetStartPlayerNode()
    { return aiPlayerStartNodeID; }

    /// <summary>
    /// Show Resistance AI status
    /// </summary>
    /// <returns></returns>
    public string DebugShowRebelAIStatus()
    {
        StringBuilder builder = new StringBuilder();
        int turn = GameManager.instance.turnScript.Turn;
        builder.AppendFormat(" Resistance AI Status {0}{1}", "\n", "\n");
        //player stats
        builder.AppendFormat("- AI Player{0}", "\n");
        builder.AppendFormat(" status: {0} | {1}{2}", status, inactiveStatus, "\n");
        builder.AppendFormat(" isBreakdown: {0}{1}", isBreakdown, "\n");
        builder.AppendFormat(" Invisbility: {0}{1}", GameManager.instance.playerScript.Invisibility, "\n");
        builder.AppendFormat(" Renown: {0}{1}", GameManager.instance.dataScript.CheckAIResourcePool(globalResistance), "\n");
        builder.AppendFormat(" Gear Pool: {0}{1}", gearPool, "\n");
        builder.AppendFormat(" Gear Used: {0}{1}", gearPointsUsed, "\n");
        builder.AppendFormat(" Target Intel: {0}{1}", targetIntel, "\n");

        List<Condition> listOfConditions = GameManager.instance.playerScript.GetListOfConditions(globalResistance);
        if (listOfConditions != null)
        {
            builder.Append(string.Format("{0}- Conditions{1}", "\n", "\n"));
            if (listOfConditions.Count > 0)
            {
                for (int i = 0; i < listOfConditions.Count; i++)
                { builder.Append(string.Format(" {0}{1}", listOfConditions[i].name, "\n")); }
            }
            else { builder.AppendFormat(" None{0}", "\n"); }
        }
        else { Debug.LogError(" Invalid listOfConditions (Null)"); }
        //
        // - - - Sorted target list
        //
        builder.AppendFormat("{0}- ProcessTargetData (current turn {1}){2}", "\n", turn - 1, "\n");
        int count = dictOfSortedTargets.Count;
        if (count > 0)
        {
            foreach (var target in dictOfSortedTargets)
            {
                if (target.Key != null)
                { builder.AppendFormat(" Target: {0}, at node id {1}, distance {2}{3}", target.Key.name, target.Key.nodeID, target.Value, "\n"); }
                else { builder.AppendFormat(" Invalid target (Null){0}", "\n"); }
            }
        }
        else { builder.AppendFormat(" No records present{0}", "\n"); }
        //actor not captured (if so don't display data as it's old data that hasn't been updated)
        if (status != ActorStatus.Captured)
        {
            //
            // - - - Nemesis Reports
            //
            count = listOfNemesisReports.Count;
            builder.AppendFormat("{0}- Nemesis Reports (current turn {1}) {2}", "\n", turn, "\n");
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    AITracker tracker = listOfNemesisReports[i];
                    if (tracker != null)
                    { builder.AppendFormat(" t: {0}, nodeID {1}, contact eff: {2}{3}", tracker.turn, tracker.data0, tracker.data1, "\n"); }
                    else { builder.AppendFormat(" Invalid sighting (Null){0}", "\n"); }
                }
            }
            else { builder.AppendFormat(" No records present{0}", "\n"); }
            //
            // - - - Nemesis Sighting Data
            //
            count = listOfNemesisSightData.Count;
            builder.AppendFormat("{0}- Nemesis Sight Data (current turn {1}){2}", "\n", turn - 1, "\n");
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    SightingData sight = listOfNemesisSightData[i];
                    if (sight != null)
                    { builder.AppendFormat(" nodeID {0}, priority {1}, moveNumber {2}{3}", sight.nodeID, sight.priority, sight.moveNumber, "\n"); }
                    else { builder.AppendFormat(" Invalid Nemesis Sight Data (Null){0}", "\n"); }
                }
            }
            else { builder.AppendFormat(" No records present{0}", "\n"); }
            //
            // - - - Erasure Reports
            //
            count = listOfErasureReports.Count;
            builder.AppendFormat("{0}- Erasure Reports (current turn {1}) {2}", "\n", turn, "\n");
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    AITracker tracker = listOfErasureReports[i];
                    if (tracker != null)
                    { builder.AppendFormat(" t: {0}, nodeID {1}, contact eff: {2}{3}", tracker.turn, tracker.data0, tracker.data1, "\n"); }
                    else { builder.AppendFormat(" Invalid sighting (Null){0}", "\n"); }
                }
            }
            else { builder.AppendFormat(" No records present{0}", "\n"); }
            //
            // - - - Erasure Sighting Data
            //
            count = listOfErasureSightData.Count;
            builder.AppendFormat("{0}- Erasure Sight Data (current turn {1}){2}", "\n", turn - 1, "\n");
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    SightingData sight = listOfErasureSightData[i];
                    if (sight != null)
                    { builder.AppendFormat(" nodeID {0}, priority {1}, moveNumber {2}{3}", sight.nodeID, sight.priority, sight.moveNumber, "\n"); }
                    else { builder.AppendFormat(" Invalid Erasure Sight Data (Null){0}", "\n"); }
                }
            }
            else { builder.AppendFormat(" No records present{0}", "\n"); }
            //
            // - - - Bad Nodes
            //
            count = listOfBadNodes.Count;
            builder.AppendFormat("{0}- ListOfBadNodes (current turn {1}){2}", "\n", turn - 1, "\n");
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                { builder.AppendFormat(" nodeID {0}{1}", listOfBadNodes[i], "\n"); }
            }
            else { builder.AppendFormat(" No records present{0}", "\n"); }
        }
        else { builder.AppendFormat("{0}{1} - CAPTURED (no report or sighting data)", "\n", "\n"); }

        //return
        return builder.ToString();
    }


    /// <summary>
    /// Runs specific turn based test conditions for debugging purposes
    /// </summary>
    private void DebugTest()
    {
        int turn = GameManager.instance.turnScript.Turn;
        int slotID = GameManager.instance.testScript.stressWhoResistance;
        //Add STRESSED condition
        if (turn == turnForStress)
        {
            //resistance player stressed
            if (slotID == 999)
            { GameManager.instance.playerScript.AddCondition(conditionStressed, globalResistance, "for Debugging"); }
            else if (slotID > -1 && slotID < 4)
            {
                //Resistance actor stressed -> check present
                if (GameManager.instance.dataScript.CheckActorSlotStatus(slotID, globalResistance) == true)
                {
                    Actor actor = GameManager.instance.dataScript.GetCurrentActor(slotID, globalResistance);
                    if (actor != null)
                    { actor.AddCondition(conditionStressed, "Debug Test Action"); }
                    else { Debug.LogErrorFormat("Invalid actor (Null) for slotID {0}", slotID); }
                }
            }
        }
    }

    //new methods above here
}
