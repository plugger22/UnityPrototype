﻿using System.Collections;
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
    [Tooltip("Cost in resources/power for the player or actor to take Stress Leave")]
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
    [Tooltip("The % chance of Power/Resource point being used to retain gear (random roll every time gear is used. Cannot send resources below zero")]
    [Range(0, 100)] public int gearPowerChance = 20;

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
    [Tooltip("Maximum number of target attempt tasks that can be generated in a single turn")]
    [Range(0, 5)] public int targetAttemptsMax = 2;

    [Header("Recruiting")]
    [Tooltip("If number of OnMap actors (status ignored) falls BELOW this number then a survival task is generated to recruit an actor (Critical task)")]
    [Range(0, 4)] public int actorNumThreshold = 3;

    [Header("Faction")]
    [Tooltip("Below this level of faction support a faction task can be generated to raise support")]
    [Range(0, 5)] public int factionSupportThreshold = 4;

    [Header("Power/Resources")]
    [Tooltip("The number of AI Resources granted per turn (Resistance side only) provided Faction decides to provide support (dependant on HQ support level as per normal)")]
    [Range(1, 3)] public int resourcesAllowance = 1;
    [Tooltip("At the end of an AutoRun the amount of resources is divided by this in order to give the amount of Power to the Resistance player")]
    [Range(1, 3)] public int powerFactor = 2;

    //AI Resistance Player
    [HideInInspector] public ActorStatus status;
    [HideInInspector] public ActorInactive inactiveStatus;
    [HideInInspector] public bool isBreakdown;          //true if suffering from nervous, stress induced, breakdown

    #region Save Data Compatible
    private int actionAllowance;                        //number of actions per turn (normal allowance + extras)
    private int actionsUsed;                            //tally of actions used this turn
    private int gearPool;                               //number of gear points in pool
    private int gearPointsUsed;                         //number of gear points expended by AI
    private int targetIntel;                            //number of target intel points (gained by Planner and used for target attempts)
    private int targetIntelUsed;                        //number of intel points expended by AI
    private int targetNodeID;                           //goal to move towards (Target)
    private int cureNodeID;                             //goal to move towards (Cure)
    private int aiPlayerStartNodeID;                    //reference only, node AI Player commences at
    private bool isConnectionsChanged;                  //if true connections have been changed due to sighting data and need to be restore once all calculations are done
    private bool isPlayer;                              //if true the Resistance side is also the human player side (it's AI due to an autorun)
    private bool isCureNeeded;                           //true if Player possesses a condition that could benefit from a cure
    private bool isCureCritical;                          //true if Player has a condition needing a cure that is on a timer, eg. Doomed condition
    private bool isPlayerStressed;                        //true only if player is stressed
    private int stressedActorID;                          //player or actorID that is chosen to have go on stress leave (player has priority)
    private int questionableID;                           //randomly chosen actor who has the questionable trait
    #endregion

    //rebel Player profile
    private int survivalMove;                           //The % chance of AI player moving away when they are at a Bad Node
    private int playerAction;                           //The % chance of the player doing the ActorArc action, rather than an Actor
    private int targetAttemptMinOdds;                   //The minimum odds that are required for the Player/Actor to attempt a target (Note: RebelLeader % converted to 0 to 10 range for ease of target tally calc)
    private int targetAttemptPlayerChance;              //The % chance of a player attempting a target if one is available
    private int targetAttemptActorChance;               //The % chance of an actor attempting a target if one is availables
    private int dismissChance;                          //The % chance of a questionable actor being dismissed per turn (chance if low priority, x 2 if Med priority, x 3 if High)
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
    private Priority priorityFactionApproval;
    private Priority priorityReserveActors;
    private Priority priorityQuestionableActor;

    //autoRun testing
    private bool isAutoRunTest;
    private int turnForCondition;
    private Condition conditionAutoRunTest;

    //fast access
    private string playerName;
    /*private string playerTag;                           //nickname
    private string playerBackground;*/
    private GlobalSide globalResistance;
    private int numOfNodes = -1;
    private int playerID = -1;
    private int priorityHigh = -1;
    private int priorityMedium = -1;
    private int priorityLow = -1;
    private int maxStatValue = -1;
    private int maxNumOfOnMapActors = -1;
    private int failedTargetChance = -1;
    private AuthoritySecurityState security;            //updated each turn in UpdateAdmin
    //traits
    private string actorRemoveActionDoubled;
    private string actorRemoveActionHalved;
    //conditions
    private Condition conditionStressed;
    private Condition conditionQuestionable;
    private Condition conditionWounded;
    private Condition conditionDoomed;
    //Activity notifications
    private int delayNoSpider = -1;
    private int delayYesSpider = -1;
    //actor arcs
    private ActorArc arcFixer;
    //resistance faction (dynamically updated in ProcessResources)
    Hq factionResistance;

    //Authority activity
    private List<AITracker> listOfNemesisReports = new List<AITracker>();
    private List<AITracker> listOfErasureReports = new List<AITracker>();
    private List<SightingData> listOfNemesisSightData = new List<SightingData>();
    private List<SightingData> listOfErasureSightData = new List<SightingData>();
    private List<int> listOfBadNodes = new List<int>();                             //list of nodes to avoid for this turn, eg. nemesis or erasure team present (based on known info)
    private List<int> listOfSpiderNodes = new List<int>();                          //list of all nodes where spiders are known to be (node.isSpiderKnown true)

    //Node and other Action data gathering
    private List<ActorArc> listOfArcs = new List<ActorArc>();                       //current actor arcs valid for this turn
    private List<Node>[] arrayOfActorActions;                                       //list of nodes suitable for listOfArc[index] action
    private List<Actor> listOfCurrentActors = new List<Actor>();                    //list of current onMap, Active, actors at start of each action
    private List<int> listOfQuestionableActors = new List<int>();                   //any actors with the questionable condition

    //stats
    private int[] arrayOfAITaskTypes;                                                       //used for analysis of which tasks the AI generates (not executes but tracks the ones placed into the pool)

    //tasks
    private List<AITask> listOfTasksPotential = new List<AITask>();
    private List<AITask> listOfTasksCritical = new List<AITask>();

    //targets
    private Dictionary<Target, int> dictOfSortedTargets = new Dictionary<Target, int>();   //key -> target, Value -> Distance (weighted and adjusted for threats)

    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.TutorialOptions:
            case GameState.NewInitialisation:
                SubInitialiseAllEarly();
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseLevelStart();
                SubInitialiseAllLate();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseAllEarly();
                SubInitialiseLevelStart();
                SubInitialiseFollowOn();
                SubInitialiseAllLate();               
                break;
            case GameState.LoadAtStart:
                SubInitialiseAllEarly();
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseLevelStart();
                SubInitialiseAllLate();
                break;
            case GameState.LoadGame:
                SubInitialiseAllEarly();
                SubInitialiseLoadGameData();
                SubInitialiseAllLate();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseAllEarly
    private void SubInitialiseAllEarly()
    {
        //always (data could change in both situations)
        numOfNodes = GameManager.i.dataScript.CheckNumOfNodes();
        Debug.Assert(numOfNodes > -1, "Invalid numOfNodes (-1)");
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //fast access     
        playerID = GameManager.i.playerScript.actorID;
        globalResistance = GameManager.i.globalScript.sideResistance;
        failedTargetChance = GameManager.i.aiScript.targetAttemptChance;
        conditionStressed = GameManager.i.dataScript.GetCondition("STRESSED");
        conditionQuestionable = GameManager.i.dataScript.GetCondition("QUESTIONABLE");
        conditionWounded = GameManager.i.dataScript.GetCondition("WOUNDED");
        conditionDoomed = GameManager.i.dataScript.GetCondition("DOOMED");
        conditionAutoRunTest = GameManager.i.testScript.conditionResistance;
        priorityHigh = GameManager.i.aiScript.priorityHighWeight;
        priorityMedium = GameManager.i.aiScript.priorityMediumWeight;
        priorityLow = GameManager.i.aiScript.priorityLowWeight;
        turnForCondition = GameManager.i.testScript.conditionTurnResistance;
        maxStatValue = GameManager.i.actorScript.maxStatValue;
        maxNumOfOnMapActors = GameManager.i.actorScript.maxNumOfOnMapActors;
        delayNoSpider = GameManager.i.nodeScript.nodeNoSpiderDelay;
        delayYesSpider = GameManager.i.nodeScript.nodeYesSpiderDelay;
        arcFixer = GameManager.i.dataScript.GetActorArc("FIXER");
        actorRemoveActionDoubled = "ActorRemoveActionDoubled";
        actorRemoveActionHalved = "ActorRemoveActionHalved";
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(playerID > -1, "Invalid playerId (-1)");
        Debug.Assert(failedTargetChance > -1, "Invalid failedTargetChance (-1)");
        Debug.Assert(priorityHigh > -1, "Invalid priorityHigh (-1)");
        Debug.Assert(priorityMedium > -1, "Invalid priorityMedium (-1)");
        Debug.Assert(priorityLow > -1, "Invalid priorityLow (-1)");
        Debug.Assert(conditionStressed != null, "Invalid conditionStressed (Null)");
        Debug.Assert(conditionQuestionable != null, "Invalid conditionQuestionable (Null)");
        Debug.Assert(conditionWounded != null, "Invalid conditionWounded (Null)");
        Debug.Assert(conditionDoomed != null, "Invalid conditionDoomed (Null)");
        Debug.Assert(maxStatValue > -1, "Invalid maxStatValue (-1)");
        Debug.Assert(maxNumOfOnMapActors > -1, "Invalid maxNumOfOnMapActors (-1)");
        Debug.Assert(delayNoSpider > -1, "Invalid delayNoSpider (-1)");
        Debug.Assert(delayYesSpider > -1, "Invalid delayYesSpider (-1)");
        Debug.Assert(arcFixer != null, "Invalid arcFixer (Null)");
    }
    #endregion

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        //collections (AFTER: fast access)
        arrayOfAITaskTypes = new int[(int)AITaskType.Count];
        arrayOfActorActions = new List<Node>[maxNumOfOnMapActors];
        for (int i = 0; i < arrayOfActorActions.Length; i++)
        { arrayOfActorActions[i] = new List<Node>(); }
    }
    #endregion

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
        //player (human / AI revert to human)
        playerName = GameManager.i.playerScript.GetPlayerNameResistance();
        /*playerTag = GameManager.i.scenarioScript.scenario.leaderResistance.tag;
        playerBackground = GameManager.i.scenarioScript.scenario.descriptorResistance;*/
        if (GameManager.i.sideScript.PlayerSide.level != globalResistance.level) { isPlayer = false; }
        else
        { isPlayer = true; }
        //gear pool
        gearPool = GameManager.i.scenarioScript.scenario.leaderResistance.gearPoints;
        gearPool = Mathf.Clamp(gearPool, 0, gearPoolMaxSize);
        //Rebel leader
        InitialiseRebelLeader();
    }

    #endregion

    #region SubIntialiseLoadGameData
    private void SubInitialiseLoadGameData()
    {
        InitialiseRebelLeader();
    }
    #endregion

    #region SubInitialiseAllLate
    private void SubInitialiseAllLate()
    {
        //AFTER session specific initialisation

        //set initial move node to start position (will trigger a new targetNodeID)
        targetNodeID = GameManager.i.nodeScript.GetPlayerNodeID();
        aiPlayerStartNodeID = GameManager.i.nodeScript.GetPlayerNodeID();
        status = ActorStatus.Active;
        inactiveStatus = ActorInactive.None;
        if (GameManager.i.inputScript.GameState != GameState.TutorialOptions)
        { GameManager.i.playerScript.Invisibility = GameManager.i.actorScript.maxStatValue; }
        //set AI resource levels
        GameManager.i.dataScript.SetAIResources(globalResistance, GameManager.i.scenarioScript.scenario.leaderResistance.resourcesStarting);
        //autoRun test
        if (GameManager.i.testScript.conditionResistance != null && GameManager.i.testScript.conditionTurnResistance > -1)
        {
            isAutoRunTest = true;
            turnForCondition = GameManager.i.testScript.conditionTurnResistance;
            conditionAutoRunTest = GameManager.i.testScript.conditionResistance;
        }
        else { isAutoRunTest = false; }
    }
    #endregion

    #region SubInitialiseFollowOn
    private void SubInitialiseFollowOn()
    {
        //clear out collections
        listOfNemesisReports.Clear();
        listOfErasureReports.Clear();
        listOfNemesisSightData.Clear();
        listOfErasureSightData.Clear();
        listOfBadNodes.Clear();
        listOfSpiderNodes.Clear();
        //reset relevant fields
        actionsUsed = 0;
        gearPointsUsed = 0;
        targetIntel = 0;
        targetIntelUsed = 0;
        targetNodeID = -1;
        cureNodeID = -1;
        isConnectionsChanged = false;
        isCureNeeded = false;
        isCureCritical = false;
        isPlayerStressed = false;
        stressedActorID = -1;
        questionableID = -1;

    }
    #endregion

    #endregion


    /// <summary>
    /// subMethod to initlalise Rebel Leader specific datas
    /// </summary>
    private void InitialiseRebelLeader()
    {
        survivalMove = GameManager.i.scenarioScript.scenario.leaderResistance.moveChance;
        playerAction = GameManager.i.scenarioScript.scenario.leaderResistance.playerChance;
        targetAttemptMinOdds = GameManager.i.scenarioScript.scenario.leaderResistance.targetAttemptMinOdds / 10;
        targetAttemptPlayerChance = GameManager.i.scenarioScript.scenario.leaderResistance.targetAttemptPlayerChance;
        targetAttemptActorChance = GameManager.i.scenarioScript.scenario.leaderResistance.targetAttemptActorChance;
        dismissChance = GameManager.i.actorScript.dismissQuestionableChance;
        priorityStressLeavePlayer = GetPriority(GameManager.i.scenarioScript.scenario.leaderResistance.stressLeavePlayer);
        priorityStressLeaveActor = GetPriority(GameManager.i.scenarioScript.scenario.leaderResistance.stressLeaveActor);
        priorityMovePlayer = GetPriority(GameManager.i.scenarioScript.scenario.leaderResistance.movePriority);
        priorityIdlePlayer = GetPriority(GameManager.i.scenarioScript.scenario.leaderResistance.idlePriority);
        priorityAnarchistTask = GetPriority(GameManager.i.scenarioScript.scenario.leaderResistance.taskAnarchist);
        priorityBloggerTask = GetPriority(GameManager.i.scenarioScript.scenario.leaderResistance.taskBlogger);
        priorityFixerTask = GetPriority(GameManager.i.scenarioScript.scenario.leaderResistance.taskFixer);
        priorityHackerTask = GetPriority(GameManager.i.scenarioScript.scenario.leaderResistance.taskHacker);
        priorityHeavyTask = GetPriority(GameManager.i.scenarioScript.scenario.leaderResistance.taskHeavy);
        priorityObserverTask = GetPriority(GameManager.i.scenarioScript.scenario.leaderResistance.taskObserver);
        priorityOperatorTask = GetPriority(GameManager.i.scenarioScript.scenario.leaderResistance.taskOperator);
        priorityPlannerTask = GetPriority(GameManager.i.scenarioScript.scenario.leaderResistance.taskPlanner);
        priorityRecruiterTask = GetPriority(GameManager.i.scenarioScript.scenario.leaderResistance.taskRecruiter);
        priorityTargetPlayer = GetPriority(GameManager.i.scenarioScript.scenario.leaderResistance.targetPlayer);
        priorityTargetActor = GetPriority(GameManager.i.scenarioScript.scenario.leaderResistance.targetActor);
        priorityFactionApproval = GetPriority(GameManager.i.scenarioScript.scenario.leaderResistance.approvalPriority);
        priorityReserveActors = GetPriority(GameManager.i.scenarioScript.scenario.leaderResistance.manageReserve);
        priorityQuestionableActor = GetPriority(GameManager.i.scenarioScript.scenario.leaderResistance.manageQuestionable);
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
        Debug.Assert(targetAttemptPlayerChance > 0, "Invalid targetAttemptPlayerChance (Zero or less)");
        Debug.Assert(targetAttemptActorChance > 0, "Invalid targetAttemptActorChance (Zero or less)");
        Debug.Assert(dismissChance > 0, "Invalid dismissChance (Zero or less)");
        Debug.Assert(priorityFactionApproval != Priority.None, "Invalid priorityFactionApproval (None)");
        Debug.Assert(priorityReserveActors != Priority.None, "Invalid priorityReserveActors (None)");
        Debug.Assert(priorityQuestionableActor != Priority.None, "invalid priorityQuestionableActor (None)");
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
        int numOfAutoRunTurns = GameManager.i.autoRunTurns - 1;
        isConnectionsChanged = false;
        //debugging
        if (isAutoRunTest == true)
        { DebugTest(); }
        //AI player ACTIVE
        if (status == ActorStatus.Active)
        {
            ClearAICollectionsEarly();
            UpdateAdmin();
            //Info gathering 
            ProcessSightingData();
            ProcessSpiderData();
            ProcessActorArcData();
            ProcessPeopleData();
            ProcessTargetData();
            ProcessCureData();
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
                if (actionsUsed > 0)
                { FilterActorArcList(listOfArcs); }

                //task creation
                if (actionsUsed == 0)
                { ProcessSurvivalTask(); }
                ProcessCureTask();
                //only one task possible and if survival task has been generated no point in going further
                if (listOfTasksCritical.Count == 0)
                {
                    ProcessAdminTask(); //first in list
                    ProcessMoveToTargetTask();
                    ProcessPeopleTask();
                    ProcessFactionTask();
                    if (listOfArcs.Count > 0)
                    {
                        //can only process if actor Arcs present)
                        ProcessActorArcTask();
                        ProcessTargetTask();
                    }
                    else
                    {
                        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessAI: Invalid listOfArcs (Empty). Emergency Recruit Task generated");
                        ProcessRecruiterTask("RECRUITER", true);
                    }
                    //do only as last action as could remove an actor and invalidate previously collated data
                    if ((actionsUsed + 1) == actionAllowance)
                    { ProcessManageTask(); }
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
        //top up reserve actors if last turn of AutoRun
        if (numOfAutoRunTurns > 0 && isPlayer == true)
        {
            if (numOfAutoRunTurns == GameManager.i.turnScript.Turn)
            { ExecuteReserveTask(); }
        }
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
        listOfSpiderNodes.Clear();
        listOfCurrentActors.Clear();
        listOfQuestionableActors.Clear();
        listOfArcs.Clear();
        //
        // - - - Sighting Reports
        //
        int threshold = GameManager.i.turnScript.Turn - 1;
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
        threshold = GameManager.i.turnScript.Turn;
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
        
    }

    /// <summary>
    /// start of AI Player turn admin
    /// </summary>
    private void UpdateAdmin()
    {
        //actions
        actionAllowance = actionsBase;
        actionsUsed = 0;
        //security state
        security = GameManager.i.turnScript.authoritySecurityState;
        //Power (equivalent to resources for AI Rebel player)
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
                    Contact contact = GameManager.i.dataScript.GetContact(message.data2);
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
                    Contact contactTeam = GameManager.i.dataScript.GetContact(message.data2);
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
        /*isWounded = false;*/
        stressedActorID = -1;
        questionableID = -1;
        //check for conditions
        List<Condition> listOfConditions = GameManager.i.playerScript.GetListOfConditions(globalResistance);
        if (listOfConditions != null)
        {
            int count = listOfConditions.Count;
            if (count > 0)
            {
                foreach (Condition condition in listOfConditions)
                {
                    if (condition != null)
                    {
                        switch (condition.tag)
                        {
                            case "STRESSED":
                                //Player has priority for stress leave
                                isPlayerStressed = true;
                                break;
                            case "WOUNDED":
                                /*isWounded = true;*/
                                if (actionAllowance > 1)
                                {
                                    //Restricts actions
                                    if (GameManager.i.playerScript.CheckConditionPresent(conditionWounded, globalResistance) == true)
                                    {
                                        actionAllowance = 1;
                                        Debug.LogFormat("[Rim] AIRebelManager.cs -> UpdateAdmin: Rebel AI Player WOUNDED. Maximum one action{0}", "\n");
                                    }
                                }
                                break;
                            case "BLACKMAILER":
                            case "CORRUPT":
                            case "INCOMPETENT":
                            case "QUESTIONABLE":
                            case "STAR":
                            case "UNHAPPY":
                            case "TAGGED":
                            case "IMAGED":
                            case "DOOMED":
                                //All of the above dealt with (if appropriate) in ActorManager.cs -> CheckPlayerResistanceAI
                                break;
                            default:
                                Debug.LogWarningFormat("Unrecognised Condition \"{0}\"", condition.tag);
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
    /// sets up listOfSpiderNodes which includes all nodes where spiders are known to be (have to be known to Resistance)
    /// </summary>
    private void ProcessSpiderData()
    {
        List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
        if (listOfNodes != null)
        {
            //loop nodes looking for ones where the Spider is known to be
            for (int i = 0; i < listOfNodes.Count; i++)
            {
                if (listOfNodes[i].isSpiderKnown == true)
                { listOfSpiderNodes.Add(listOfNodes[i].nodeID); }
            }

            /*Debug.LogFormat("[Tst] AIRebelManager.cs -> ProcessSpiderData: {0} records in listOfSpiders{1}", listOfSpiderNodes.Count, "\n");*/

        }
        else { Debug.LogError("Invalid listOfNodes (Null)"); }
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
                //check for situation where two equal priority sightings at top of list, take the highest turn number first then highest moveNumber (nemesis may have moved twice in the same turn)
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
            GameManager.i.connScript.SaveConnections();
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
                        Node node = GameManager.i.dataScript.GetNode(sightingErasure.nodeID);
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
            GameManager.i.dijkstraScript.RecalculateWeightedData();
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
    /// returns true if nodeID is on listOfBadNodes, false if not. If isMoveOnly true (default false) considers listOfBadNodes only. If true checks also listOfSpiderNodes (considered bad nodes if doing an action)
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    private bool CheckForBadNode(int nodeID, bool isMoveOnly = false)
    {
        bool isBad = false;
        //Move only, ignore spider nodes
        if (isMoveOnly == true)
        { isBad = listOfBadNodes.Exists(x => nodeID == x); }
        else
        {
            //Action (doing something that involves a potential loss of invisibility) -> take into account spider and bad nodes
            isBad = listOfBadNodes.Exists(x => nodeID == x);
            //only bother to check further if drew a blank with bad nodes
            if (isBad == false)
            {
                if (listOfSpiderNodes.Exists(x => nodeID == x) == true)
                { isBad = true; }
            }
        }
        return isBad;
    }

    /// <summary>
    /// updates all of a node's connections to the specified security level (if lower, ignore otherwise)
    /// </summary>
    /// <param name="sight"></param>
    private void UpdateNodeConnectionSecurity(SightingData sight)
    {
        ConnectionType sightLevel = ConnectionType.None;
        Node node = GameManager.i.dataScript.GetNode(sight.nodeID);
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
        int turn = GameManager.i.turnScript.Turn;
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
        switch (GameManager.i.playerScript.Invisibility)
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
            //moveNumber & turn of sighting
            sighting.turn = tracker.turn;
            sighting.moveNumber = tracker.data2;
        }
        return sighting;
    }


    /// <summary>
    /// Populate a sorted dictionary of all available targets
    /// </summary>
    private void ProcessTargetData()
    {
        List<Target> listOfTargets = GameManager.i.dataScript.GetTargetPool(GlobalStatus.Live);
        if (listOfTargets != null)
        {
            //temp dict with key -> Target, value -> distance (weighted)
            Dictionary<Target, int> dictOfTargets = new Dictionary<Target, int>();
            int distance;
            int count = listOfTargets.Count;
            if (count > 0)
            {
                int playerNodeID = GameManager.i.nodeScript.GetPlayerNodeID();
                //loop targets and get weighted distance to each
                for (int i = 0; i < count; i++)
                {
                    Target target = listOfTargets[i];
                    if (target != null)
                    {
                        distance = GameManager.i.dijkstraScript.GetDistanceWeighted(playerNodeID, target.nodeID);
                        /*Debug.LogFormat("[Tst] AIRebelManager.cs -> ProcessTargetData: playerNodeID {0}, targetNodeID {1} DISTANCE {2}{3}", playerNodeID, target.nodeID, distance, "\n");*/
                        if (distance > -1)
                        {
                            //add entry to dictionary
                            try
                            { dictOfTargets.Add(target, distance); }
                            catch (ArgumentException)
                            { Debug.LogErrorFormat("Duplicate target entry for target {0}", target.targetName); }
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
    /// Assesses player and actors for any relevant personal situations, eg. need to take stress leave, Questionable condition present, etc.
    /// </summary>
    private void ProcessPeopleData()
    {
        int resources = GameManager.i.dataScript.CheckAIResourcePool(globalResistance);
        int numOfActors = GameManager.i.dataScript.CheckNumOfOnMapActors(globalResistance);
        //Stress Leave
        if (resources >= stressLeaveCost)
        {
            if (isPlayerStressed)
            {
                //must have max invis
                if (GameManager.i.playerScript.Invisibility == maxStatValue)
                { stressedActorID = playerID; }
            }
        }
        //check  Resistance actors
        Actor[] arrayOfActors = GameManager.i.dataScript.GetCurrentActorsFixed(globalResistance);
        if (arrayOfActors != null)
        {
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                //check actor is present in slot (not vacant)
                if (GameManager.i.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
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
                                    if (actor.GetDatapoint(ActorDatapoint.Invisibility2) == maxStatValue)
                                    {
                                        //take first instance found
                                        stressedActorID = actor.actorID;
                                        break;
                                    }
                                }
                            }
                        }
                        //
                        // - - - Questionable actors (only if min. # of actors currently OnMap)
                        //
                        if (numOfActors >= actorNumThreshold)
                        {
                            if (actor.CheckConditionPresent(conditionQuestionable) == true)
                            {
                                //add to list (cleared each turn in Admin early)
                                listOfQuestionableActors.Add(actor.actorID);
                            }
                        }
                    }
                }
            }
            //check for any questionable actors
            int count = listOfQuestionableActors.Count;
            if ( count > 0)
            {
                //select one actor (if > 1) randomly (used as a candidate to be fired by ProcessManageTask)
                questionableID = listOfQuestionableActors[Random.Range(0, count)];
                Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessPeopleData: {0} Questionable actor{1}, {2} ID selected{3}", count, count != 1 ? "s" : "", questionableID, "\n");
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
        List<ActorArc> tempList = new List<ActorArc>(GameManager.i.dataScript.GetListOfResistanceActorArcs());
        if (tempList != null)
        {
            //Filter out any arcs that match OnMap actors and who are currently not Active -> reverse loop tempList
            tempList = FilterActorArcList(tempList);
            //Randomly select from filtered pool of actorArcs
            int limit = GameManager.i.dataScript.CheckNumOfActiveActors(globalResistance);
            if (limit > tempList.Count)
            {
                Debug.LogWarningFormat("There are not enough actorArcs in the pool (need {0}, have {1})", limit, tempList.Count);
                limit = tempList.Count;
            }
            for (int i = 0; i < limit; i++)
            {
                index = Random.Range(0, tempList.Count);
                listOfArcs.Add(tempList[index]);

                /*Debug.LogFormat("[Tst] AIRebelManager.cs -> ProcessActorData: {0} actorArc added to list{1}", tempList[index].name, "\n");*/
                
                //delete to prevent duplicates
                tempList.RemoveAt(index);
            }
        }
        else { Debug.LogError("Invalid tempList ActorArcs (Null)"); }
    }

    /// <summary>
    /// subMethod to filter out of a list of ActorArcs any that match OnMap actors and who aren't currently Active
    /// </summary>
    /// <param name="tempList"></param>
    /// <returns></returns>
    private List<ActorArc> FilterActorArcList(List<ActorArc> tempList)
    {
        if (tempList != null)
        {
            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                ActorArc arc = tempList[i];
                if (arc != null)
                {
                    //is ActorArc present in the current OnMap lineup?
                    if (GameManager.i.dataScript.CheckActorArcPresent(arc, globalResistance, true) == true)
                    {
                        //check actor is Active
                        if (GameManager.i.dataScript.CheckActorPresent(arc.name, globalResistance) < 0)
                        {
                            //remove that arc from the list as actors who aren't currently active can't perform actions
                            Debug.LogFormat("[Rim] AIRebelManager.cs -> FilterActorArcList: {0} removed from pool as actor OnMap and NOT ACTIVE{1}", arc.name, "\n");
                            tempList.RemoveAt(i);
                        }
                    }
                }
                else { Debug.LogErrorFormat("Invalid arc (Null) in tempList[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid tempList (Null)"); }
        return tempList;
    }


    /// <summary>
    /// provides resources (equivalent to Power for resistance AI side) depending on level of faction support (as per normal)
    /// </summary>
    private void ProcessResources()
    {
        int resources;
        int rnd = Random.Range(0, 100);
        int approvalResistance = GameManager.i.hqScript.ApprovalResistance;
        int powerPerTurn = GameManager.i.hqScript.powerPerTurn;
        int threshold = approvalResistance * 10;
        factionResistance = GameManager.i.hqScript.hQResistance;
        if (factionResistance != null)
        {
            resources = GameManager.i.dataScript.CheckAIResourcePool(globalResistance);
            if (rnd < threshold)
            {
                //Support given
                resources += resourcesAllowance;
                GameManager.i.dataScript.SetAIResources(globalResistance, resources);
                //Support Provided
                Debug.LogFormat("[Rnd] FactionManager.cs -> CheckFactionSupport: GIVEN need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                if (isPlayer == true)
                {
                    string msgText = string.Format("{0} provides +{1} SUPPORT (now {2} Resource{3})", factionResistance.name, resourcesAllowance, resources, resources != 1 ? "s" : "");
                    GameManager.i.messageScript.HqSupport(msgText, factionResistance, approvalResistance, GameManager.i.playerScript.Power, powerPerTurn);
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessResources: {0}", msgText);
                    //random
                    GameManager.i.messageScript.GeneralRandom("Faction support GIVEN", "Faction Support", threshold, rnd);
                }
            }
            else
            {
                //Support declined
                Debug.LogFormat("[Rnd] FactionManager.cs -> CheckFactionSupport: DECLINED need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                if (isPlayer == true)
                {
                    string msgText = string.Format("{0} faction declines support ({1} % chance of support)", factionResistance.name, threshold);
                    GameManager.i.messageScript.HqSupport(msgText, factionResistance, approvalResistance, GameManager.i.playerScript.Power);
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessResources: No Support provided (now {0} Resource{1}){2}", resources, resources != 1 ? "s" : "", "\n");
                    //random
                    GameManager.i.messageScript.GeneralRandom("Faction support DECLINED", "Faction Support", threshold, rnd);
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
        int invisibility = GameManager.i.playerScript.Invisibility;
        int lieLowTimer = GameManager.i.actorScript.lieLowTimer;
        int playerNodeID = GameManager.i.nodeScript.GetPlayerNodeID();
        int numOfActors = GameManager.i.dataScript.CheckNumOfOnMapActors(globalResistance);
        bool isSuccess = false;
        //can only take action if Active (Captured/Stressed/Lie Low etc. can't do anything)
        if (status == ActorStatus.Active)
        {
            if (isCureCritical == false)
            {
                //
                // - - - Nemesis or Erasure team in current node (sighting report)
                //
                if (CheckForBadNode(playerNodeID, true) == true)
                {
                    //first response -> Move away
                    Node nodePlayer = GameManager.i.dataScript.GetNode(playerNodeID);
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
                //
                // - - - Running low on Actors - - -
                //
                else if (numOfActors < actorNumThreshold)
                {
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessSurvivalTask: {0} actors currently OnMap. RECRUIT THRESHOLD triggered{1}", numOfActors, "\n");
                    ProcessRecruiterTask("RECRUITER", true);
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
                            Actor[] arrayOfActors = GameManager.i.dataScript.GetCurrentActorsFixed(globalResistance);
                            if (arrayOfActors != null)
                            {
                                for (int i = 0; i < arrayOfActors.Length; i++)
                                {
                                    //check actor is present in slot (not vacant)
                                    if (GameManager.i.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
                                    {
                                        Actor actor = arrayOfActors[i];
                                        if (actor != null)
                                        {
                                            //below threshold
                                            if (actor.GetDatapoint(ActorDatapoint.Invisibility2) < lieLowThresholdActor)
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
            else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessSurvivalTask: Survival Task assessmenet CANCELLED due to need for a Critical Cure{0}", "\n"); }
        }

    }

    /// <summary>
    /// determine whether player has a cure related condition and, if so, set a node to move to
    /// </summary>
    private void ProcessCureData()
    {
        //does player have a condition that comes with a cure?
        List<Condition> listOfConditions = GameManager.i.playerScript.GetListOfConditions(globalResistance);
        
        //reset flags each turn
        isCureNeeded = false;
        isCureCritical = false;
        cureNodeID = -1;
        Node node = null;
        //check Player's conditions and available OnMap cures
        int count = listOfConditions.Count;
        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessCureData: {0} conditions present{1}", count, "\n");
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                if (listOfConditions[i].cure != null)
                {
                    isCureNeeded = true;
                    Condition condition = listOfConditions[i];
                    if (condition != null)
                    {
                        //Critical condition (Doomed), eg. timer
                        if (listOfConditions[i].name.Equals(conditionDoomed.name, StringComparison.Ordinal) == true)
                        {
                            isCureCritical = true;
                            //is there a cure available -> if so assign node to move towards (overrides all other cure nodes)
                            node = GameManager.i.dataScript.GetCureNode(conditionDoomed.cure);
                            if (node != null && node.cure.isActive == true)
                            { cureNodeID = node.nodeID; }
                            break;
                        }
                        else
                        {
                            //non-critical condition -> take last one found with a cure as a node to move towards
                            node = GameManager.i.dataScript.GetCureNode(listOfConditions[i].cure);
                            //can't override a critical condition cure node
                            if (node != null && isCureCritical == false && node.cure.isActive == true)
                            { cureNodeID = node.nodeID; }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid condition (Null) in listOfConditions[{0}]", i); }
                }
            }
        }
        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessCureData: isCureNeeded: {0}, isCureCritical: {1}, cureNodeID {2}{3}", isCureNeeded, isCureCritical, cureNodeID, "\n");
    }

    /// <summary>
    /// Select a suitable node to move to (single node move) in the direction of a chosen cure. All cure tasks are critical
    /// </summary>
    private void ProcessCureTask()
    {
        //is there a valid cure node destination?
        if (cureNodeID > -1)
        {
            Node nodePlayer = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.GetPlayerNodeID());
            Node nodeMoveTo = null;
            Connection connection = null;
            bool isProceed = true;
            if (nodePlayer != null)
            {
                //AT CURE NODE
                if (nodePlayer.nodeID == cureNodeID)
                {
                    if (nodePlayer.cure != null && nodePlayer.cure.isActive == true)
                    {
                        //generate task
                        AITask task = new AITask();
                        task.data0 = cureNodeID;
                        task.type = AITaskType.Cure;
                        //if a critical  cure situation, then critical priority
                        if (isCureCritical == true)
                        { task.priority = Priority.Critical; }
                        else { task.priority = Priority.High; }
                        //add task to list of potential tasks
                        AddWeightedTask(task);
                        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessCureTask: AT cureNodeID {0}, Cure ACTION{1}", cureNodeID, "\n");
                    }
                }
                else if (nodePlayer.cure != null && nodePlayer.cure.isActive == true)
                {
                    //At another cure node -> standard High priority task as it won't be a critical one
                    AITask task = new AITask();
                    task.data0 = cureNodeID;
                    task.type = AITaskType.Cure;
                    task.priority = Priority.High;
                    //add task to list of potential tasks
                    AddWeightedTask(task);
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessCureTask: AT cureNodeID {0}, Cure ACTION{1}", cureNodeID, "\n");
                }
                //NOT AT Cure Node, or any other node that happens to have a cure available
                else
                {
                    //check there is still a valid cure to move towards
                    Node nodeCure = GameManager.i.dataScript.GetNode(cureNodeID);
                    if (nodeCure != null)
                    {
                        if (nodeCure.cure != null)
                        {
                            Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessCureTask: AI Player continues to move towards Cure at nodeID {0}{1}", cureNodeID, "\n");
                            //path to cure 
                            List<Connection> pathList = GameManager.i.dijkstraScript.GetPath(nodePlayer.nodeID, cureNodeID, true);
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
                                if (CheckForBadNode(nodeMoveTo.nodeID, true) == true)
                                {
                                    Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessCureTask: Selected node id {0} is on listOfBadNodes{1}", nodeMoveTo.nodeID, "\n");
                                    //get a random neighbouring node which is good
                                    nodeMoveTo = GetRandomGoodNode(nodePlayer);
                                    if (nodeMoveTo == null)
                                    {
                                        //no viable node that isn't bad amongst neighbours. Cancel move task
                                        isProceed = false;
                                        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessCureTask: Selected node is BAD, no viable alternative. Move task CANCELLED{0}", "\n");
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
                                        task.type = AITaskType.Cure;
                                        //critical priority if a critical cure situation, high otherwise
                                        if (isCureCritical == true)
                                        { task.priority = Priority.Critical; }
                                        else { task.priority = Priority.High; }
                                        //add task to list of potential tasks
                                        AddWeightedTask(task);
                                        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessCureTask: cureNodeID {0}, move to {1}{2}", cureNodeID, nodeMoveTo.nodeID, "\n");
                                    }
                                    else { Debug.LogErrorFormat("Invalid connection (Null) for nodeID {0}", nodeMoveTo.nodeID); }
                                }
                            }
                            else { Debug.LogError("Invalid nodeMoveTo (Null)"); }
                        }
                        else
                        {
                            Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessCureTask: No valid cure at destination, {0}, {1}, ID {2} -> Move Cancelled{3}",
                                nodeCure.nodeName, nodeCure.Arc.name, nodeCure.nodeID, "\n");
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid nodeCure (Null) for nodeID {0}", cureNodeID); }
                }
            }
            else { Debug.LogErrorFormat("Invalid player node (Null) for nodeID {0}", GameManager.i.nodeScript.GetPlayerNodeID()); }
        }
    }


    /// <summary>
    /// Select a suitable node to move to (single node move) in the direction of a chosen target
    /// </summary>
    private void ProcessMoveToTargetTask()
    {
        Node nodePlayer = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.GetPlayerNodeID());
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
                            nodeMoveTo = GameManager.i.dataScript.GetNode(targetNodeID);
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
                        randomNode = GameManager.i.dataScript.GetRandomNode();
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
            List<Connection> pathList = GameManager.i.dijkstraScript.GetPath(nodePlayer.nodeID, targetNodeID, true);
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
                if (CheckForBadNode(nodeMoveTo.nodeID, true) == true)
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
        else { Debug.LogErrorFormat("Invalid player node (Null) for nodeID {0}", GameManager.i.nodeScript.GetPlayerNodeID()); }
    }


    /// <summary>
    /// Player or Actors attempt target/s. One task generated for player (if criteria O.K). If not player task then one target task per actor (possibly if enough targets and criteria O.K)
    /// </summary>
    private void ProcessTargetTask()
    {
        int targetTally, rnd;
        string targetName;
        bool isSuccess = false;
        Node nodePlayer = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.GetPlayerNodeID());
        //intel assumed to be used at max for any target attempt
        int intelTemp;
        if (targetIntel >= targetIntelAttempt) { intelTemp = targetIntelAttempt; }
        else { intelTemp = targetIntel; }
        //
        // - - - Player - - -
        //
        targetName = nodePlayer.targetName;
        //target present
        if (string.IsNullOrEmpty(targetName) == false)
        {
            Target target = GameManager.i.dataScript.GetTarget(targetName);
            if (target != null)
            {
                //Live target
                if (target.targetStatus == GlobalStatus.Live)
                {
                    //Player at target node
                    rnd = Random.Range(0, 100);
                    if (rnd < targetAttemptPlayerChance)
                    {
                        //node not bad
                        if (CheckForBadNode(nodePlayer.nodeID) == false)
                        {
                            targetTally = GameManager.i.targetScript.GetTargetTallyAI(targetName);
                            targetTally += intelTemp;
                            if (gearPool > 0 && CheckGearAvailable(false) == true)
                            { targetTally++; }
                            //must be above minimum odds threshold to proceed
                            if (targetTally >= targetAttemptMinOdds)
                            {
                                //generate task for Player Attempt
                                Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: Player target attempt passed Odds check (threshold {0}, tally {1}){2}", targetAttemptMinOdds, targetTally, "\n");
                                isSuccess = true;
                                //generate task
                                AITask task = new AITask();
                                task.data0 = playerID;
                                task.data1 = nodePlayer.nodeID;
                                task.name1 = targetName;
                                task.type = AITaskType.Target;
                                task.priority = priorityTargetPlayer;
                                //add task to list of potential tasks
                                AddWeightedTask(task);
                                Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: Player Attempt Target task, targetID {0} at {1}, {2}, id {3}{4}", targetName, nodePlayer.nodeName, nodePlayer.Arc.name,
                                    nodePlayer.nodeID, "\n");
                            }
                            else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: Player target NOT attempted due to low odds (threshold {0}, tally {1}){2}", targetAttemptMinOdds, targetTally, "\n"); }
                        }
                        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: Player target NOT attempted due to BAD NODE (nodeID {0}){1}", nodePlayer.nodeID, "\n"); }
                    }
                    else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: Player declines to attempt target (needed {0}, rolled {1}){2}", targetAttemptPlayerChance, rnd, "\n"); }
                }
            }
            else { Debug.LogErrorFormat("Invalid target (Null) for targetID {0}", targetName); }
        }
        //
        // - - - Actors - - -
        //
        if (isSuccess == false)
        {
            //at least one actor present
            if (listOfCurrentActors.Count > 0)
            {
                int numOfTasks = 0;
                //is there at least on Live target available
                List<Target> listOfLiveTargets = GameManager.i.dataScript.GetTargetPool(GlobalStatus.Live);
                if (listOfLiveTargets != null)
                {
                    int actorID;
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: Consider possible Actor attempts, {0} targets available{1}", listOfLiveTargets.Count, "\n");
                    for (int i = 0; i < listOfLiveTargets.Count; i++)
                    {
                        Target target = listOfLiveTargets[i];
                        if (target != null)
                        {
                            //LIVE target
                            if (target.targetStatus == GlobalStatus.Live)
                            {
                                if (CheckActorArcPresent(target.actorArc.name) == true)
                                {
                                    Node node = GameManager.i.dataScript.GetNode(target.nodeID);
                                    if (node != null)
                                    {
                                        //not a bad node
                                        if (CheckForBadNode(node.nodeID) == false)
                                        {
                                            //actor of that type present 
                                            if (CheckActorArcPresent(target.actorArc.name) == true)
                                            {
                                                rnd = Random.Range(0, 100);
                                                if (rnd < targetAttemptActorChance)
                                                {
                                                    //Actor present, check target odds
                                                    targetTally = GameManager.i.targetScript.GetTargetTallyAI(target.name);
                                                    targetTally += intelTemp;
                                                    if (gearPool > 0 && CheckGearAvailable(false) == true)
                                                    { targetTally++; }
                                                    //must be above minimum odds threshold to proceed
                                                    if (targetTally >= targetAttemptMinOdds)
                                                    {
                                                        //which OnMap actor is used
                                                        actorID = GetOnMapActor(target.actorArc.name);
                                                        //generate task for Actor Attempt
                                                        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: {0} target attempt passed Odds check (threshold {1}, tally {2}){3}", target.actorArc.name,
                                                            targetAttemptMinOdds, targetTally, "\n");
                                                        if (actorID > -1)
                                                        {
                                                            //generate task
                                                            numOfTasks++;
                                                            AITask task = new AITask();
                                                            task.data0 = actorID;
                                                            task.data1 = target.nodeID;
                                                            task.name1 = target.name;
                                                            task.type = AITaskType.Target;
                                                            task.priority = priorityTargetActor;
                                                            //add task to list of potential tasks
                                                            AddWeightedTask(task);
                                                            Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: {0} attempt Target task, {1} at {2}, {3}, id {4}{5}", target.actorArc.name,
                                                                target.name, node.nodeName, node.Arc.name, node.nodeID, "\n");
                                                        }
                                                        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: target task aborted as no SUITABLE Active, OnMap, actor found"); }
                                                    }
                                                    else
                                                    {
                                                        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: {0} target NOT attempted due to low odds (threshold {1}, tally {2}){3}", target.actorArc.name,
                                                     targetAttemptMinOdds, targetTally, "\n");
                                                    }
                                                }
                                                else
                                                {
                                                    Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: {0} declines to attempt target (needed {1}, rolled {2}){3}", target.actorArc.name,
                                                 targetAttemptActorChance, rnd, "\n");
                                                }
                                            }
                                        }
                                        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: {0} target NOT attempted due to BAD NODE (nodeID {1}){2}", target.actorArc.name, nodePlayer.nodeID, "\n"); }
                                    }
                                    else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", target.nodeID); }
                                }
                                else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: {0} target IGNORED as actor arc type not present{1}", target.actorArc.name, "\n"); }
                            }
                        }
                        else { Debug.LogErrorFormat("Invalid target (Null) for listOfLiveTargets[{0}]", i); }
                        //limit the number of target tasks that can be generated
                        if (numOfTasks >= targetAttemptsMax)
                        {
                            Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTargetTask: MAX number of target attempt tasks reached (limit {0}){1}", targetAttemptsMax, "\n");
                            break;
                        }
                    }
                }
                else { Debug.LogError("Invalid listOfLiveTargets (Null)"); }
            }
            else
            {
                /*if (GameManager.instance.turnScript.Turn > 0)
                { Debug.LogWarning("There are no OnMap actors present"); }*/
            }
        }
    }


    /// <summary>
    /// sets up listOfCurrentActors used by subsequent Process 'x' Tasks (actors who are OnMap and Active)
    /// </summary>
    private void ProcessAdminTask()
    {
        //create a list of all active, current, onMap actors (used for AI decision logic)
        Actor[] arrayOfActors = GameManager.i.dataScript.GetCurrentActorsFixed(globalResistance);
        if (arrayOfActors != null)
        {
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                //check actor is present in slot & Active
                if (GameManager.i.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
                {
                    Actor actor = arrayOfActors[i];
                    if (actor != null && actor.Status == ActorStatus.Active)
                    { listOfCurrentActors.Add(actor); }
                }
            }
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessActorArcTask: listOfCurrentActors has {0} records{1}", listOfCurrentActors.Count, "\n");
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }
    }


    /// <summary>
    /// Actor and Player actor action tasks. One task is generated for each ActorArc in listOfActorArcs
    /// </summary>
    private void ProcessActorArcTask()
    {
        int limit;
        string actorArcName;
        if (listOfCurrentActors.Count > 0)
        {
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
                        ProcessRecruiterTask(actorArcName);
                        break;
                    default:
                        Debug.LogErrorFormat("Unrecognised Actor Arc \"{0}\"", listOfArcs[index].name);
                        break;
                }
            }
        }
        else { Debug.LogWarning("[Rim] AIRebelManager.cs -> ProcessActorArcTask: actorArc tasks aborted as NO available actors present for ActorArcTasks{0}"); }
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
    /// deals with Management tasks, eg. fire a questionable subordinate
    /// </summary>
    private void ProcessManageTask()
    {
        //actor with questionable condition present
        if (questionableID > -1)
        {
            //chance of a task
            int chance = dismissChance;
            switch (priorityQuestionableActor)
            {
                case Priority.High: chance *= 3; break;
                case Priority.Medium: chance *= 2; break;
                case Priority.Low: chance *= 1; break;
                default: Debug.LogErrorFormat("Unrecognised priorityQuestionableActor \"{0}\"", priorityQuestionableActor); break;
            }
            int rndNum = Random.Range(0, 100);
            if (rndNum < chance)
            {
                //generate task -> player or actor
                AITask task = new AITask();
                task.data0 = questionableID;
                task.type = AITaskType.Dismiss;
                task.priority = priorityQuestionableActor;
                //add task to list of potential tasks
                AddWeightedTask(task);
                Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessManageTask: actorID {0}, Questionable, Dismiss{1}", questionableID, "\n");
            }
            else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessManageTask: Failed Roll to Dismiss questionable actor (needed {0}, rolled {1}){2}", chance, rndNum, "\n"); }
        }
    }

    /// <summary>
    /// deals with all faction specific tasks, eg. approval level
    /// </summary>
    private void ProcessFactionTask()
    {
        //
        // - - - Approval Level - - -
        //
        int approvalLevel = GameManager.i.hqScript.ApprovalResistance;
        if (approvalLevel < factionSupportThreshold)
        {
            //check only if Player Active (node irrelevant in this situation as invisibility not an issue)
            if (GameManager.i.playerScript.Status == ActorStatus.Active)
            {
                //generate task
                AITask task = new AITask();
                task.type = AITaskType.HQ;
                //normal priority as per RebelLeader but Critical if approval level zero
                if (approvalLevel == 0)
                { task.priority = Priority.Critical; }
                else { task.priority = priorityFactionApproval; }
                //add task to list of potential tasks
                AddWeightedTask(task);
                Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessFactionTask: approval Level {0}, task Priority {1}", approvalLevel, task.priority, "\n");
            }
            else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessFactionTask: Player Inactive. NO TASK Generated for Faction Approval (currently {0}){1}", approvalLevel, "\n"); }
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
        task.data0 = GameManager.i.nodeScript.GetPlayerNodeID();
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
                    nodeID = GameManager.i.nodeScript.GetPlayerNodeID();
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
            nodeID = GameManager.i.nodeScript.GetPlayerNodeID();
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
            nodeID = GameManager.i.nodeScript.GetPlayerNodeID();
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
            nodeID = GameManager.i.nodeScript.GetPlayerNodeID();
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
            nodeID = GameManager.i.nodeScript.GetPlayerNodeID();
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
            nodeID = GameManager.i.nodeScript.GetPlayerNodeID();
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
            nodeID = GameManager.i.nodeScript.GetPlayerNodeID();
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
    /// Planner Actor Arc task. There is no limit on the amount of intel that can be acquired
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
                nodeID = GameManager.i.nodeScript.GetPlayerNodeID();
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
    /// Recruiter Actor Arc task. If 'isAutoPlayer' true then player automatically does the task if there is no other viable alternative (ignores bad node). Used for ProcessSurvivalTask)
    /// </summary>
    /// <param name="actorArcName"></param>
    private void ProcessRecruiterTask(string actorArcName, bool isAutoPlayer = false)
    {
        int actorID = -1;
        int nodeID = -1;
        //check if vacancy in current line-up
        int slotID = GameManager.i.dataScript.CheckForSpareActorSlot(globalResistance);
        if (slotID > -1)
        {
            //Player does action?
            if (CheckPlayerAction(actorArcName) == true)
            {
                actorID = playerID;
                nodeID = GameManager.i.nodeScript.GetPlayerNodeID();
            }
            else
            {
                //get node and actorID
                Tuple<int, int> results = FindNodeActorRandom(actorArcName);
                nodeID = results.Item1;
                actorID = results.Item2;
            }
            //no viable option found
            if (nodeID < 0 || actorID < 0)
            {
                //player does action regardless of status, bad nodes (used by ProcessSurvivalTask to prevent running out of OnMap actors)
                if (isAutoPlayer == true)
                {
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessRecruiterTask: No viable option available, AUTOMATIC Player Recruit action invoked{0}", "\n");
                    nodeID = GameManager.i.nodeScript.GetPlayerNodeID();
                    Node node = GameManager.i.dataScript.GetNode(nodeID);
                    if (node != null)
                    {
                        if (CheckNodeCriteria("RECRUITER", node) == true)
                        { actorID = playerID; }
                        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessRecruiterTask: Automatic Player Recruit action but FAILED node Criteria check{0}", "\n"); }
                    }
                    else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", nodeID); }
                }
            }
            //generate task if valid data is present, if none, ignore task
            if (nodeID > -1 && actorID > -1)
            {
                AITask task = new AITask();
                task.type = AITaskType.ActorArc;
                task.data0 = actorID;
                task.data1 = nodeID;
                task.data2 = slotID;
                task.name0 = actorArcName;
                //priority can be automatically overidden depending on number of OnMap actors present
                int numOfActors = GameManager.i.dataScript.CheckNumOfOnMapActors(globalResistance);
                if (numOfActors < actorNumThreshold)
                {
                    if (isAutoPlayer == true)
                    {
                        task.priority = Priority.Critical;
                        //add task to list of critical tasks
                        AddWeightedTask(task);
                    }
                    else
                    {
                        if (numOfActors > 0)
                        {
                            task.priority = priorityRecruiterTask;
                            //add task to list of potential tasks
                            AddWeightedTask(task);
                        }
                        else { Debug.LogWarning("No actors present OnMap (yet indicated previously that there were"); }
                    }
                }
                else
                {
                    task.priority = priorityRecruiterTask;
                    //add task to list of potential tasks
                    AddWeightedTask(task);
                }
            }
            else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessRecruiterTask: Invalid RECRUITER task (nodeID {0}, actorID {1}). No task generated{2}", nodeID, actorID, "\n"); }
        }
        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessRecruiterTask: RECRUITER task NOT generated (no vacancy in line-up){0}", "\n"); }
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
                            if (CheckForBadNode(tempNode.nodeID, true) == true)
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
        //stats
        UpdateTaskTypeStats(listOfTasksCritical);
        UpdateTaskTypeStats(listOfTasksPotential);
        //check for Critical tasks for
        int count = listOfTasksCritical.Count;
        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessTaskFinal: {0} Critical Task{1} available{2}", count, count != 1 ? "s" : "", "\n");
        if (count > 0)
        {
            if (count > 1)
            { task = listOfTasksCritical[Random.Range(0, count)]; }
            else { task = listOfTasksCritical[0]; }

            //debug
            for (int i = 0; i < count; i++)
            {
                AITask tempTask = listOfTasksCritical[i];
                /*if (string.IsNullOrEmpty(tempTask.name0) == false)
                {
                    Debug.LogFormat("[Tst] AIRebelManager.cs -> ProcessTaskFinal: type {0}, {1} priority, d0: {2}, d1: {3}, d2: {4}, name0: {5}{6}", tempTask.type, tempTask.priority,
                        tempTask.data0, tempTask.data1, tempTask.data2, tempTask.name0, "\n");
                }
                else
                {
                    Debug.LogFormat("[Tst] AIRebelManager.cs -> ProcessTaskFinal: type {0}, {1} priority, d0: {2}, d1: {3}, d2: {4}{5}", tempTask.type, tempTask.priority,
                        tempTask.data0, tempTask.data1, tempTask.data2, "\n");
                }*/
            }

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

                    /*if (string.IsNullOrEmpty(tempTask.name0) == false)
                    { Debug.LogFormat("[Tst] AIRebelManager.cs -> ProcessTaskFinal: type {0}, {1} priority, d0: {2}, d1: {3}, d2: {4}, name0: {5}{6}", tempTask.type, tempTask.priority,
                          tempTask.data0, tempTask.data1, tempTask.data2, tempTask.name0, "\n"); }
                    else
                    { Debug.LogFormat("[Tst] AIRebelManager.cs -> ProcessTaskFinal: type {0}, {1} priority, d0: {2}, d1: {3}, d2: {4}{5}", tempTask.type, tempTask.priority,
                          tempTask.data0, tempTask.data1, tempTask.data2, "\n"); }*/
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
            case AITaskType.Cure:
                ExecuteCureTask(task);
                break;
            case AITaskType.HQ:
                ExecuteFactionTask(task);
                break;
            case AITaskType.Dismiss:
                ExecuteDismissTask(task);
                break;
            default:
                Debug.LogErrorFormat("Invalid task (Unrecognised) \"{0}\"", task.type);
                break;
        }

    }

    /// <summary>
    /// AI Player moves, task.data0 is nodeID, task.data1 is connectionID. Returns true if successful. isAction parameter (default true) as can be called by another ExecuteTask method, eg. ExecuteCureTask
    /// NOTE: Task checked for Null by parent method
    /// </summary>
    private bool ExecuteMoveTask(AITask task, bool isAction = true)
    {
        bool isSuccess = false;
        Node node = GameManager.i.dataScript.GetNode(task.data0);
        if (node != null)
        {
            //update player node
            GameManager.i.nodeScript.nodePlayer = node.nodeID;
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteMoveTask: AI Player moves to {0}, {1}, id {2}{3}", node.nodeName, node.Arc.name, node.nodeID, "\n");
            //expend action
            if (isAction == true)
            { UseAction("Move"); }
            //move list (for when autorun ends)
            node.SetPlayerMoveNodes();
            isSuccess = true;
            //invisibility
            Connection connection = GameManager.i.dataScript.GetConnection(task.data1);
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
            history.turn = GameManager.i.turnScript.Turn;
            history.playerNodeID = node.nodeID;
            history.invisibility = GameManager.i.playerScript.Invisibility;
            history.nemesisNodeID = GameManager.i.nodeScript.nodeNemesis;
            history.nodeName = node.nodeName;
            GameManager.i.dataScript.AddHistoryRebelMove(history);

            //Erasure team may pick up player  if invisibility 1 or less
            if (GameManager.i.playerScript.Invisibility <= 1)
            {
                CaptureDetails captureDetails = GameManager.i.captureScript.CheckCaptured(node.nodeID, playerID);
                if (captureDetails != null)
                {
                    //Player captured!
                    captureDetails.effects = "The move went bad";
                    EventManager.i.PostNotification(EventType.Capture, this, captureDetails, "AIRebelManager.cs -> ExecuteMoveTask");
                }
            }
            if (GameManager.i.playerScript.Status != ActorStatus.Captured)
            {
                //Nemesis, if at same node, can interact and damage player
                GameManager.i.nemesisScript.CheckNemesisAtPlayerNode(true);
            }
        }
        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", task.data0);}
        return isSuccess;
    }


    /// <summary>
    /// Player or Actor commences Lying Low. task.data0 -> nodeID, task.data1 -> actorID (999 if player)
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteLieLowTask(AITask task)
    {
        Debug.Assert(GameManager.i.turnScript.authoritySecurityState != AuthoritySecurityState.SurveillanceCrackdown, string.Format("Invalid authoritySecurityState {0}",
            GameManager.i.turnScript.authoritySecurityState));
        string aiName = "Unknown";
        bool isSuccess = true;
        if (task.data1 == playerID)
        {
            //Player Lie low
            aiName = playerName;
            //default data 
            status = ActorStatus.Inactive;
            inactiveStatus = ActorInactive.LieLow;
            GameManager.i.playerScript.isLieLowFirstturn = true;
            //message (only if human player after an autorun)
            if (isPlayer == true)
            {
                GameManager.i.dataScript.StatisticIncrement(StatType.PlayerLieLowTimes);
                Debug.LogFormat("[Ply] AIRebelManager.cs -> ExecuteLieLowTask: Player commences LYING LOW at node ID {0}{1}", task.data0, "\n");
                //history
                Node node = GameManager.i.dataScript.GetNode(task.data0);
                if (node != null)
                { GameManager.i.dataScript.AddHistoryPlayer(new HistoryActor() { text = "Commence Lying Low", district = node.nodeName }); }
                else { Debug.LogWarningFormat("Invalid node (Null) for nodeID \"{0}\"", task.data0); }
                //message
                string text = string.Format("{0} is lying Low. Status: {1}", playerName, status);
                string reason = string.Format("is currently Lying Low and is{0}{1}<b>cut off from all communications</b>", "\n", "\n");
                GameManager.i.messageScript.ActorStatus(text, "is LYING LOW", reason, playerID, globalResistance);
            }
            //expend action
            UseAction("Lie Low (Player)");
        }
        else
        {
            //Actor Lie Low
            Actor actor = GameManager.i.dataScript.GetActor(task.data1);
            if (actor != null)
            {
                aiName = string.Format("{0}, {1}", actor.actorName, actor.arc.name);
                actor.Status = ActorStatus.Inactive;
                actor.inactiveStatus = ActorInactive.LieLow;
                actor.isLieLowFirstturn = true;
                //message (only if human player after an autorun)
                if (isPlayer == true)
                {
                    Debug.LogFormat("[Tor] AIRebelManager.cs -> ExecuteLieLowTask: Actor {0}, commences LYING LOW{1}", aiName, "\n");
                    //history
                    actor.AddHistory(new HistoryActor() { text = "Goes into hiding (Lies Low)" });
                    //message
                    string text = string.Format("{0} is lying Low. Status: {1}", aiName, actor.Status);
                    string reason = string.Format("is currently Lying Low and is{0}{1}<b>out of communication</b>", "\n", "\n");
                    GameManager.i.messageScript.ActorStatus(text, "is LYING LOW", reason, actor.actorID, globalResistance);
                }
                //expend action
                UseAction("Lie Low (Actor)");
            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data1); isSuccess = false; }
        }
        if (isSuccess == true)
        {
            //set lie low timer
            GameManager.i.actorScript.SetLieLowTimer();
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteLieLowTask: \"{0}\", id {1} is LYING LOW at node ID {2}{3}", aiName, task.data1, task.data0, "\n");
        }
    }

    /// <summary>
    /// actor / player takes stress leave where invis max and stressed (NOT a survival task). Power cost & Opportunity cost
    /// </summary>
    private void ExecuteStressLeaveTask(AITask task)
    {
        if (task != null)
        {
            bool isSuccess = false;
            string text = "Unknown";
            string actorName = "Unknown";
            string actorType = "Unknown";
            int resources = GameManager.i.dataScript.CheckAIResourcePool(globalResistance);
            if (resources >= stressLeaveCost)
            {
                //remove condition
                if (task.data0 == playerID)
                {
                    actorName = GameManager.i.playerScript.GetPlayerNameResistance();
                    actorType = "Player";
                    isSuccess = GameManager.i.playerScript.RemoveCondition(conditionStressed, globalResistance, "Stress Leave");
                    text = string.Format("{0}, Player, takes Stress Leave", actorName);
                }
                else
                {
                    Actor actor = GameManager.i.dataScript.GetActor(task.data0);
                    if (actor != null)
                    {
                        actorName = actor.actorName;
                        actorType = actor.arc.name;
                        isSuccess = actor.RemoveCondition(conditionStressed, "due to Stress Leave");
                        text = string.Format("{0}, {1}, takes Stress Leave", actorName, actor.arc.name);
                        //history
                        actor.AddHistory(new HistoryActor() { text = "Takes Stress Leave" });
                    }
                    else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data0); }
                }
                if (isSuccess == true)
                {
                    stressedActorID = -1;
                    GameManager.i.messageScript.ActorStressLeave(text, task.data0, globalResistance);
                    //expend resources
                    resources -= stressLeaveCost;
                    if (resources < 0)
                    {
                        Debug.LogWarning("Not enough resources for Stress Leave");
                        resources = 0;
                    }
                    GameManager.i.dataScript.SetAIResources(globalResistance, resources);
                    //statistic
                    GameManager.i.dataScript.StatisticIncrement(StatType.StressLeaveResistance);
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
                    ExecuteRecruiterTask(task);
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
        Node node = GameManager.i.dataScript.GetNode(task.data1);
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
                UpdatePower();
                //debug
                DebugCreateNodeActionPlayerRecord(node.nodeID, NodeAction.PlayerObtainGear, "AI_Gear");
            }
            else
            {
                actor = GameManager.i.dataScript.GetActor(task.data0);
                if (actor != null)
                {
                    actorName = actor.actorName;
                    UpdatePower(actor);
                    //debug
                    DebugCreateNodeActionActorRecord(node.nodeID, actor, NodeAction.ActorObtainGear, "AI_Gear");
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
        Node node = GameManager.i.dataScript.GetNode(task.data1);
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
                UpdatePower();
                //debug
                DebugCreateNodeActionPlayerRecord(node.nodeID, NodeAction.PlayerGainTargetInfo, "AI_Target");
            }
            else
            {
                actor = GameManager.i.dataScript.GetActor(task.data0);
                if (actor != null)
                {
                    actorName = actor.actorName;
                    UpdatePower(actor);
                    //debug
                    DebugCreateNodeActionActorRecord(node.nodeID, actor, NodeAction.ActorGainTargetInfo, "AI_Target");
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
    /// Recruiter actorArc task execution
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteRecruiterTask(AITask task)
    {
        string actorName = "Unknown";
        string nodeName = "Unknown";
        string nodeArc = "Unknown";
        bool isPlayerAction = false;
        int nodeID = -1;
        Actor actor = null;
        //get node name
        Node node = GameManager.i.dataScript.GetNode(task.data1);
        if (node != null)
        {
            nodeName = node.nodeName; nodeArc = node.Arc.name; nodeID = node.nodeID;
            //effect
            GameManager.i.actorScript.AddNewActorOnMapAI(globalResistance, node, task.data2);
            //expend an action -> get actor Name
            if (task.data0 == playerID)
            {
                actorName = "Player";
                isPlayerAction = true;
                UpdatePower();
                //debug
                DebugCreateNodeActionPlayerRecord(node.nodeID, NodeAction.PlayerRecruitActor, "AI_Actor");
            }
            else
            {
                actor = GameManager.i.dataScript.GetActor(task.data0);
                if (actor != null)
                {
                    actorName = actor.actorName;
                    UpdatePower(actor);
                    //debug
                    DebugCreateNodeActionActorRecord(node.nodeID, actor, NodeAction.ActorRecruitActor, "AI_Actor");
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data0); }
            }
            UseAction(string.Format("Recruit new Actor ({0}, Recruiter at {1}, {2}, id {3})", actorName, nodeName, nodeArc, nodeID));
            //gear used to stay invisible
            if (CheckGearAvailable(false) == false)
            {
                //invisibility drops
                if (isPlayerAction == true)
                { UpdateInvisibilityNode(node); }
                else { UpdateInvisibilityNode(node, actor); }
            }
            else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteRecruiterTask: GEAR USED to remain undetected while recruiting (gearPoint NOT lost)"); }
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
        bool isPlayerAction = false;
        int nodeID = -1;
        Actor actor = null;
        //get node
        Node node = GameManager.i.dataScript.GetNode(task.data1);
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
                isPlayerAction = true;
                UpdatePower();
                //debug
                DebugCreateNodeActionPlayerRecord(node.nodeID, NodeAction.PlayerSpreadFakeNews, "Unknown");
            }
            else
            {
                actor = GameManager.i.dataScript.GetActor(task.data0);
                if (actor != null)
                {
                    actorName = actor.actorName;
                    actorArcName = actor.arc.name;
                    UpdatePower(actor);
                    //debug
                    DebugCreateNodeActionActorRecord(node.nodeID, actor, NodeAction.ActorSpreadFakeNews, "Unknown");
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data0); }
            }
            //expend action
            UseAction(string.Format("increase Support ({0}, {1} at {2}, {3}, id {4})", actorName, actorArcName, nodeName, nodeArc, nodeID));
            //gear used to stay invisible 
            if (CheckGearAvailable() == false)
            {
                //invisibility drops
                if (isPlayerAction == true)
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
        bool isPlayerAction = false;
        int nodeID = -1;
        Actor actor = null;
        //get node
        Node node = GameManager.i.dataScript.GetNode(task.data1);
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
                isPlayerAction = true;
                UpdatePower();
                //debug
                DebugCreateNodeActionPlayerRecord(node.nodeID, NodeAction.PlayerBlowStuffUp, "Unknown");
            }
            else
            {
                actor = GameManager.i.dataScript.GetActor(task.data0);
                if (actor != null)
                {
                    actorName = actor.actorName;
                    actorArcName = actor.arc.name;
                    UpdatePower(actor);
                    //debug
                    DebugCreateNodeActionActorRecord(node.nodeID, actor, NodeAction.ActorBlowStuffUp, "Unknown");
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data0); }
            }
            //expend action
            UseAction(string.Format("decrease Stability ({0}, {1} at {2}, {3}, id {4})", actorName, actorArcName, nodeName, nodeArc, nodeID));
            //gear used to stay invisible 
            if (CheckGearAvailable() == false)
            {
                //invisibility drops
                if (isPlayerAction == true)
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
        bool isPlayerAction = false;
        int nodeID = -1;
        Actor actor = null;
        //get node
        Node node = GameManager.i.dataScript.GetNode(task.data1);
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
                isPlayerAction = true;
                UpdatePower();
                //debug
                DebugCreateNodeActionPlayerRecord(node.nodeID, NodeAction.PlayerInsertTracer, "Unknown");
            }
            else
            {
                actor = GameManager.i.dataScript.GetActor(task.data0);
                if (actor != null)
                {
                    actorName = actor.actorName;
                    actorArcName = actor.arc.name;
                    UpdatePower(actor);
                    //debug
                    DebugCreateNodeActionActorRecord(node.nodeID, actor, NodeAction.ActorInsertTracer, "Unknown");
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data0); }
            }
            //expend action
            UseAction(string.Format("insert Tracer ({0}, {1} at {2}, {3}, id {4})", actorName, actorArcName, nodeName, nodeArc, nodeID));
            //gear used to stay invisible 
            if (CheckGearAvailable() == false)
            {
                //invisibility drops
                if (isPlayerAction == true)
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
        bool isPlayerAction = false;
        int nodeID = -1;
        Actor actor = null;
        //get node
        Node node = GameManager.i.dataScript.GetNode(task.data1);
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
                isPlayerAction = true;
                UpdatePower();
                //debug
                DebugCreateNodeActionPlayerRecord(node.nodeID, NodeAction.PlayerNeutraliseTeam, "Unknown");
            }
            else
            {
                actor = GameManager.i.dataScript.GetActor(task.data0);
                if (actor != null)
                {
                    actorName = actor.actorName;
                    actorArcName = actor.arc.name;
                    UpdatePower(actor);
                    //debug
                    DebugCreateNodeActionActorRecord(node.nodeID, actor, NodeAction.ActorNeutraliseTeam, "Unknown");
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data0); }
            }
            //expend action
            UseAction(string.Format("neutralise Team ({0}, {1} at {2}, {3}, id {4})", actorName, actorArcName, nodeName, nodeArc, nodeID));
            //gear used to stay invisible 
            if (CheckGearAvailable() == false)
            {
                //invisibility drops
                if (isPlayerAction == true)
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
        bool isPlayerAction = false;
        int nodeID = -1;
        Actor actor = null;
        //get node
        Node node = GameManager.i.dataScript.GetNode(task.data1);
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
                isPlayerAction = true;
                UpdatePower();
                //debug
                DebugCreateNodeActionPlayerRecord(node.nodeID, NodeAction.PlayerHackSecurity, "Unknown");
            }
            else
            {
                actor = GameManager.i.dataScript.GetActor(task.data0);
                if (actor != null)
                {
                    actorName = actor.actorName;
                    actorArcName = actor.arc.name;
                    UpdatePower(actor);
                    //debug
                    DebugCreateNodeActionActorRecord(node.nodeID, actor, NodeAction.ActorHackSecurity, "Unknown");
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data0); }
            }
            //expend action
            UseAction(string.Format("decrease Security ({0}, {1} at {2}, {3}, id {4})", actorName, actorArcName, nodeName, nodeArc, nodeID));
            //gear used to stay invisible 
            if (CheckGearAvailable() == false)
            {
                //invisibility drops
                if (isPlayerAction == true)
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
        bool isPlayerAction = false;
        int nodeID = -1;
        int counter = 0;
        Actor actor = null;
        //get node
        Node node = GameManager.i.dataScript.GetNode(task.data1);
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
                    isPlayerAction = true;
                    UpdatePower();
                    //debug
                    DebugCreateNodeActionPlayerRecord(node.nodeID, NodeAction.PlayerCreateRiots, "Unknown");
                }
                else
                {
                    actor = GameManager.i.dataScript.GetActor(task.data0);
                    if (actor != null)
                    {
                        actorName = actor.actorName;
                        actorArcName = actor.arc.name;
                        UpdatePower(actor);
                        //debug
                        DebugCreateNodeActionActorRecord(node.nodeID, actor, NodeAction.ActorCreateRiots, "Unknown");
                    }
                    else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data0); }
                }
                //expend action
                UseAction(string.Format("decrease Stability in neighbouring Nodes ({0}, {1} at {2}, {3}, id {4})", actorName, actorArcName, nodeName, nodeArc, nodeID));
                //gear used to stay invisible 
                if (CheckGearAvailable() == false)
                {
                    //invisibility drops
                    if (isPlayerAction == true)
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
    /// Player either moves towards a cure node or actions a cure at their current node
    /// NOTE: Task checked for Null by calling method (it's parent actually)
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteCureTask(AITask task)
    {
        bool isSuccess = false;
        string reason = "Unknown";
        Node nodePlayer = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.GetPlayerNodeID());
        //check situation
        if (task.data0 == nodePlayer.nodeID)
        {
            //cure in current node
            if (nodePlayer.cure != null)
            {
                if (nodePlayer.cure.isActive == true)
                {
                    Condition condition = nodePlayer.cure.condition;
                    if (condition != null)
                    {
                        reason = string.Format("cure {0} condition", condition.tag);
                        if (GameManager.i.playerScript.RemoveCondition(condition, globalResistance, reason) == true)
                        {
                            isSuccess = true;
                            Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteCureTask: Player {0} condition CURED by {1} at {2}, {3}, ID {4}{5}", condition.tag, condition.cure.cureName,
                                nodePlayer.nodeName, nodePlayer.Arc.name, nodePlayer.nodeID, "\n");
                            //at designated cure node. Reset flags to prevent a possible wasted second cure action. If still remaining conditions to cure then they will be picked up next turn.
                            if (cureNodeID == nodePlayer.nodeID)
                            {
                                isCureNeeded = false;
                                isCureCritical = false;
                            }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid condition (Null) for nodeID {0}", nodePlayer.nodeID); }
                }
                else { Debug.LogWarningFormat("Invalid cure (isActive FALSE) in nodeID {0}", nodePlayer.nodeID); }
            }
            else { Debug.LogWarningFormat("No cure present (Null) in nodeID {0}", nodePlayer.nodeID); }
        }
        else
        {
            //move to a node towards a cure
            if (ExecuteMoveTask(task, false) == true)
            {
                isSuccess = true;
                reason = string.Format("move to CURE at nodeID {0}", task.data0);
                Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteCureTask: {0}, Player moving towards CURE{1}", playerName, "\n");
            }
        }
        //expend action
        if (isSuccess == true)
        {
            //expend action
            UseAction(reason);
        }
    }

    /// <summary>
    /// Player or Actor attempts a target
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteTargetTask(AITask task)
    {
        string actorName = "Unknown";
        string actorArcName = "Unknown";
        string nodeName = "Unknown";
        string nodeArc = "Unknown";
        int nodeID = -1;
        Actor actor = null;
        //get node
        Node node = GameManager.i.dataScript.GetNode(task.data1);
        if (node != null)
        {
            nodeName = node.nodeName;
            nodeArc = node.Arc.name;
            nodeID = node.nodeID;
            //effect
            ExecuteTargetAttempt(node, task.data0);
            //expend an action -> get actor Name
            if (task.data0 == playerID)
            {
                actorName = playerName;
                actorArcName = "Player";
                /*isPlayerAction = true;*/
                UpdatePower();
            }
            else
            {
                actor = GameManager.i.dataScript.GetActor(task.data0);
                if (actor != null)
                {
                    actorName = actor.actorName;
                    actorArcName = actor.arc.name;
                    UpdatePower(actor);
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", task.data0); }
            }
            //expend action
            UseAction(string.Format("attempt Target ({0}, {1} at {2}, {3}, id {4})", actorName, actorArcName, nodeName, nodeArc, nodeID));

            
        }
        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", task.data1); }
    }


    /// <summary>
    /// subMethod to handle target attempt, called by ExecuteTargetTask
    /// NOTE: Node checked for null by calling method
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteTargetAttempt(Node node, int actorID)
    {
        string text;
        string actorArc = "Unknown";
        bool isSuccessful = false;
        bool isZeroInvisibility = false;
        bool isPlayerAction = false;
        if (actorID == playerID) { isPlayerAction = true; actorArc = "Player"; }
        Actor actor = null;
        if (isPlayerAction == false)
        {
            actor = GameManager.i.dataScript.GetActor(actorID);
            if (actor != null)
            { actorArc = actor.arc.name; }
            else
            { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", actorID); }
        }
        //target
        Target target = GameManager.i.dataScript.GetTarget(node.targetName);
        if (target != null)
        {
            //
            // - - - Invisibility and Capture - - -
            //
            if (CheckGearAvailable() == false)
            {
                //invisibility drops & Capture check
                if (UpdateInvisibilityNode(node, actor) == true)
                {
                    //Player/actor captured -> Target aborted, deal with Capture
                    return;
                }
                else
                {
                    //set flags for immediate notification of rebel activity
                    if (isPlayerAction == true)
                    {
                        if (GameManager.i.playerScript.Invisibility == 0)
                        { isZeroInvisibility = true; }
                    }
                    else
                    {
                        if (actor.GetDatapoint(ActorDatapoint.Invisibility2) == 0)
                        {
                            isZeroInvisibility = true;
                            //history
                            actor.AddHistory(new HistoryActor() { text = string.Format("Attempts target ({0})", target.descriptorResistance), district = node.nodeName });
                        }
                    }
                }
            }
            else { Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteTargetAttempt: GEAR USED to remain undetected while attempting target"); }
            //
            // - - - Attempt Target - - -  
            //
            int tally = GameManager.i.targetScript.GetTargetTallyAI(target.name);
            //gear used for target attempt
            if (CheckGearAvailable() == true)
            {
                Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteTargetTask: GEAR USED during target attempt (gearPool now {0})", gearPool);
                tally += 1;
            }
            //target intel
            int usedIntel = targetIntel;
            if (targetIntel >= targetIntelAttempt)
            { targetIntel -= targetIntelAttempt; usedIntel = targetIntelAttempt; }
            else { usedIntel = targetIntel; targetIntel = 0; }
            targetIntelUsed += usedIntel;
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteTargetTask: {0} Intel used during target attempt (remaining Intel {1}){2}", usedIntel, targetIntel, "\n");
            tally += usedIntel;
            //convert to % chance
            int chance = GameManager.i.targetScript.GetTargetChance(tally);
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteTargetAttempt: Target {0}{1}", target.targetName, "\n");
            target.numOfAttempts++;
            GameManager.i.dataScript.StatisticIncrement(StatType.TargetAttempts);
            int roll = Random.Range(0, 100);
            if (roll < chance)
            {
                //Success
                isSuccessful = true;
                GameManager.i.dataScript.StatisticIncrement(StatType.TargetSuccesses);
                target.turnSuccess = GameManager.i.turnScript.Turn;
                //Ongoing effects then target moved to completed pool
                if (target.ongoingEffect != null)
                {
                    GameManager.i.dataScript.RemoveTargetFromPool(target, GlobalStatus.Live);
                    GameManager.i.dataScript.AddTargetToPool(target, GlobalStatus.Outstanding);
                    target.targetStatus = GlobalStatus.Outstanding;
                }
                //NO ongoing effects -> target  done with. 
                else
                { GameManager.i.targetScript.SetTargetDone(target, node); }
                text = string.Format("Target \"{0}\" successfully attempted", target.targetName, "\n");
                GameManager.i.messageScript.TargetAttempt(text, node, actorID, target);
                //random roll
                Debug.LogFormat("[Rnd] AIRebelManager.cs -> ExecuteTargetAttempt: Target attempt SUCCESS need < {0}, rolled {1}{2}", chance, roll, "\n");
                Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteTargetAttempt: Target {0}, attempted SUCCESSFULLY by {1}, ID {2}, at nodeID {3}{4}", target.targetName, actorArc,
                    actorID, node.nodeID, "\n");
                text = string.Format("Target {0} attempt SUCCESS", target.targetName);
                GameManager.i.messageScript.GeneralRandom(text, "Target Attempt", chance, roll);
            }
            else
            {
                Debug.LogFormat("[Rnd] AIRebelManager.cs -> ExecuteTargetAttempt: Target attempt FAILED need < {0}, rolled {1}{2}", chance, roll, "\n");
                Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteTargetAttempt: Target {0}, attempt FAILED by {1}, ID {2}, at nodeID {3}{4}", target.targetName, actorArc,
                    actorID, node.nodeID, "\n");
                text = string.Format("Target {0} attempt FAILED", target.targetName);
                GameManager.i.messageScript.GeneralRandom(text, "Target Attempt", chance, roll);
            }
            //set isTargetKnown -> auto if success, % chance otherwise
            if (isSuccessful == true) { node.isTargetKnown = true; }
            else
            {
                //chance of being known
                if (isZeroInvisibility == false)
                {
                    roll = Random.Range(0, 100);
                    if (roll < failedTargetChance)
                    {
                        node.isTargetKnown = true;
                        Debug.LogFormat("[Rnd] AIRebelManager.cs -> ExecuteTargetAttempt: Target attempt KNOWN need < {0}, rolled {1}{2}", failedTargetChance, roll, "\n");
                    }
                    else
                    { Debug.LogFormat("[Rnd] AIRebelManager.cs -> ExecuteTargetAttempt: Target attempt UNDETECTED need < {0}, rolled {1}{2}", failedTargetChance, roll, "\n"); }
                }
                //if zero invisibility then target auto known to authorities
                else { node.isTargetKnown = true; }
            }
            Debug.LogFormat("[Tar] AIRebelManager.cs -> ExecuteTargetAttempt: Authority aware of target: {0}", node.isTargetKnown);
            //
            // - - - Effects - - - 
            //
            List<Effect> listOfEffects = new List<Effect>();
            //target SUCCESSFUL
            if (isSuccessful == true)
            {
                //combine all effects into one list for processing
                listOfEffects.AddRange(target.listOfGoodEffects);
                listOfEffects.AddRange(target.listOfBadEffects);
                if (target.ongoingEffect != null)
                { listOfEffects.Add(target.ongoingEffect); }
            }
            else
            {
                //FAILED target attempt
                listOfEffects.AddRange(target.listOfFailEffects);
                text = string.Format("Target \"{0}\" unsuccessfully attempted", target.targetName, "\n");
                GameManager.i.messageScript.TargetAttempt(text, node, actorID, target);
            }
            //Process effects
            EffectDataReturn effectReturn = new EffectDataReturn();
            //pass through data package
            EffectDataInput dataInput = new EffectDataInput();
            dataInput.originText = "Target Attempt";
            dataInput.source = EffectSource.TargetAI;
            //org name, if present (only applies to ContactOrg effect for Organisation targets but send regardless)
            dataInput.dataName = GameManager.i.targetScript.targetOrgName;
            //handle any Ongoing effects of target completed -> only if target Successful
            if (isSuccessful == true && target.ongoingEffect != null)
            {
                dataInput.ongoingID = GameManager.i.effectScript.GetOngoingEffectID();
                dataInput.ongoingText = target.reasonText;
                //add to target so it can link to effects
                target.ongoingID = dataInput.ongoingID;
            }
            //any effects to process?
            if (listOfEffects.Count > 0)
            {
                foreach (Effect effect in listOfEffects)
                {
                    if (effect != null)
                    {
                        effectReturn = GameManager.i.effectScript.ProcessEffect(effect, node, dataInput, actor);
                        if (effectReturn != null)
                        {
                            //exit effect loop on error
                            if (effectReturn.errorFlag == true)
                            { break; }
                        }
                        else
                        {
                            effectReturn.errorFlag = true;
                            break;
                        }
                    }
                    else { Debug.LogWarning("Invalid effect (Null)"); }
                }
            }
        }
        else { Debug.LogErrorFormat("Invalid Target (Null) for node.targetID {0}", node.targetName); }
    }
        
    /// <summary>
    /// Raise level of faction support by one
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteFactionTask(AITask task)
    {
        GameManager.i.hqScript.ChangeHqApproval(1, globalResistance, "Rebel Leader lobbies HQ");
        //action
        UseAction("raise Faction Approval Level");
    }

    /// <summary>
    /// Fire a questionable actor (auto done by player, uses an action but no invisibility impact. Considered a management task)
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteDismissTask(AITask task)
    {
        Actor actor = GameManager.i.dataScript.GetActor(task.data0);
        if (actor != null)
        {
            //calc Power/resources cost (need to do prior to DismissActorAI as secrets will be removed
            int numOfSecrets = actor.CheckNumOfSecrets();
            int cost = numOfSecrets * GameManager.i.actorScript.manageSecretPower + GameManager.i.actorScript.manageDismissPower;
            cost *= 2;
            //check trait -> Connected/Unconnected
            if (actor.CheckTraitEffect(actorRemoveActionDoubled) == true)
            {
                cost *= 2;
                Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteDismissTask: {0} Dismissed, CONNECTED, cost Doubled{0}", "\n");
            }
            if (actor.CheckTraitEffect(actorRemoveActionHalved) == true)
            {
                cost /= 2;
                Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteDismissTask: {0} Dismissed, UNCONNECTED, cost Halved{0}", "\n");
            }
            //Dismiss actor
            if (GameManager.i.actorScript.DismissActorAI(globalResistance, actor) == true)
            {
                //history
                actor.AddHistory(new HistoryActor() { text = "Fired from active service" });
                //deduct resources
                int resources = GameManager.i.dataScript.CheckAIResourcePool(globalResistance);
                resources = Mathf.Max(0, resources - cost);
                GameManager.i.dataScript.SetAIResources(globalResistance, resources);
                //admin
                Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteDismissTask: {0} Dismissed, cost {1} Power ({2} secrets){3}", actor.arc.name, cost, numOfSecrets, "\n");
            }
        }
        else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", questionableID); }
        //action
        UseAction("fire Questionable subordinate");
    }


    /// <summary>
    /// tops up Reserve actor pool at completion of autorun depending on RebelLeader priority (manageReserve)
    /// </summary>
    private void ExecuteReserveTask()
    {
        int numOfActors;
        
        if (priorityReserveActors != Priority.None)
        {
            //get the required number of actors
            switch (priorityReserveActors)
            {
                case Priority.High: numOfActors = 2; break;
                case Priority.Medium: numOfActors = 1; break;
                case Priority.Low: numOfActors = 0; break;
                default: Debug.LogWarningFormat("Unrecognised priority \"{0}\"", priorityReserveActors); numOfActors = 0; break;
            }
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteReserveTask: Populate RESERVE POOL, priority {0}, {1} actor{2} to add{3}", priorityReserveActors, numOfActors, numOfActors != 1 ? "s" : "", "\n");
            if (numOfActors > 0)
            {
                //add the required number of actors to the reserve
                for (int i = 0; i < numOfActors; i++)
                { GameManager.i.actorScript.AddNewActorReserveAI(globalResistance); }
            }
        }
        else { Debug.LogError("Invalid RebelLeader.SO manageReserve priority (None)"); }
    }

    /// <summary>
    /// do nothing task
    /// </summary>
    private void ExecuteIdleTask(AITask task)
    {
        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessIdleTask: \"{0}\", id {1} is IDLING at node ID {2}{3}", playerName, task.data1, task.data0, "\n");

        /*Debug.LogFormat("[Tst] AIRebelManager.cs -> ProcessIdleTask: IDLING{0}", "\n");*/

        //action
        UseAction("Idle (Player)");
    }


    //
    // - - - Utility - - -
    //

    /// <summary>
    /// increment Power (actors) and resources (player) as a result of taking an action. Leave actor as Null for Player.
    /// </summary>
    /// <param name="actor"></param>
    private void UpdatePower(Actor actor = null)
    {
        if (actor == null)
        {
            //Player -> Resources
            int resources = GameManager.i.dataScript.CheckAIResourcePool(globalResistance);
            resources++;
            GameManager.i.dataScript.SetAIResources(globalResistance, resources);
        }
        else
        {
            //Actor
            actor.Power++;
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
        int aiPlayerInvisibility = GameManager.i.playerScript.Invisibility;
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
            GameManager.i.playerScript.Invisibility = aiPlayerInvisibility;
            //message
            string text = string.Format("AI Resistance Player has moved to {0}, {1}, id {2}, Invisibility now {3}", node.nodeName, node.Arc.name, node.nodeID, aiPlayerInvisibility);
            GameManager.i.messageScript.PlayerMove(text, node, changeInvisibility, aiDelay);
            //AI message
            string textAI = string.Format("Player spotted moving to \"{0}\", {1}, ID {2}", node.nodeName, node.Arc.name, node.nodeID);
            if (aiDelay == 0)
            {
                //moving while invisibility already 0 triggers immediate alert flag
                GameManager.i.aiScript.immediateFlagResistance = true;
                //AI Immediate notification
                GameManager.i.messageScript.AIImmediateActivity("Immediate Activity \"Move\" (AI Player)", "Moving", node.nodeID, connection.connID);
            }
            else
            {
                //AI delayed notification
                GameManager.i.messageScript.AIConnectionActivity(textAI, node, connection, aiDelay);
            }
        }
    }

    /// <summary>
    /// Updates player or actor invisibility for a node action. Actor is null for Player. Handles AI activity notifications. Capture checks. Returns true if palyer/actor captured, false otherwise.
    /// NOTE: both node and actor are checked for null by the calling method, actor is deliberately null to indicate the Player
    /// </summary>
    /// <param name="nodeID"></param>
    private bool UpdateInvisibilityNode(Node node, Actor actor = null)
    {
        int delay = -1;
        int actorID = -1; //player/actorID, used for AI activity notification
        int aiInvisibility;
        bool isCaptured = false;
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
            aiInvisibility = GameManager.i.playerScript.Invisibility;
            //player did an action with invisibility zero
            if (aiInvisibility == 0)
            {
                //delay of Zero
                delay = 0;
                //immediate flag true
                GameManager.i.aiScript.immediateFlagResistance = true;
            }
            aiInvisibility -= 1;
            //double loss of invisibility if spider present
            if (node.isSpider == true)
            { aiInvisibility -= 1; }
            //min cap zero
            aiInvisibility = Mathf.Max(0, aiInvisibility);
            //update player Invisibility
            GameManager.i.playerScript.Invisibility = aiInvisibility;
            //capture check
            if (aiInvisibility <= 1)
            {
                //Erasure team picks up player immediately if invisibility 0
                CaptureDetails captureDetails = GameManager.i.captureScript.CheckCaptured(node.nodeID, playerID);
                if (captureDetails != null)
                {
                    //Player captured!
                    isCaptured = true;
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> UpdateInvisibilityNode: {0}, Player, CAPTURED by Erasure Team at {1}, {2}, ID {3}{4}", playerName, node.nodeName, node.Arc.name, node.nodeID, "\n");
                    captureDetails.effects = "They kicked in the door before you could run";
                    EventManager.i.PostNotification(EventType.Capture, this, captureDetails, "AIRebelManager.cs -> UpdateInvisibilityNode");
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
            aiInvisibility = actor.GetDatapoint(ActorDatapoint.Invisibility2);
            //actor did an action with invisibility zero
            if (aiInvisibility == 0)
            {
                //delay of Zero
                delay = 0;
                //immediate flag true
                GameManager.i.aiScript.immediateFlagResistance = true;
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
            actor.SetDatapoint(ActorDatapoint.Invisibility2, aiInvisibility);
            //capture check
            if (aiInvisibility <= 1)
            {
                //Erasure team picks up actor immediately if invisibility 0
                CaptureDetails captureDetails = GameManager.i.captureScript.CheckCaptured(node.nodeID, actorID);
                if (captureDetails != null)
                {
                    //actor captured!
                    isCaptured = true;
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> UpdateInvisibilityNode: {0}, {1}, ID {2}, CAPTURED by Erasure Team at {3}, {4}, ID {5}{6}", actorName, actorArc, actorID,
                        node.nodeName, node.Arc.name, node.nodeID, "\n");
                    captureDetails.effects = "They got you";
                    EventManager.i.PostNotification(EventType.Capture, this, captureDetails, "AIRebelManager.cs -> UpdateInvisibility");
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
        GameManager.i.messageScript.AINodeActivity(text, node, actorID, delay);
        //AI Immediate message
        if (GameManager.i.aiScript.immediateFlagResistance == true)
        {
            text = string.Format("Immediate Activity \"{0}\" ({1})", actorArc, actorName);
            string reason = "district action";
            GameManager.i.messageScript.AIImmediateActivity(text, reason, node.nodeID, -1, actorID);
        }
        return isCaptured;
    }


    /// <summary>
    /// Action used, message to that effect. Reason in format 'Action used to ... [reason]'
    /// </summary>
    /// <param name="reason"></param>
    private void UseAction(string reason)
    {
        actionsUsed++;
        if (string.IsNullOrEmpty(reason) == false)
        { Debug.LogFormat("[Rim] AIRebelManager.cs -> UseAction: ACTION {0} used to {1}{2}", actionsUsed, reason, "\n"); }
        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> UseAction: ACTION {0} used for Unknown reason{0}", actionsUsed, "\n"); }
    }


    /// <summary>
    /// determines whether it is the Player who will carry out the ActorArc action. Player's node needs to be not bad and needs to pass a test. 
    /// will check if the player's current node will allow this actorArcName action based on the actorArc criteria, eg. Blogger needs node support < max
    /// </summary>
    /// <returns></returns>
    private bool CheckPlayerAction(string actorArcName)
    {
        int rnd = -1;
        int playerNodeID = GameManager.i.nodeScript.GetPlayerNodeID();
        bool isPlayerAction = false;
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
                    Node node = GameManager.i.dataScript.GetNode(playerNodeID);
                    if (node != null)
                    {
                        if (CheckNodeCriteria(actorArcName, node) == true)
                        { isPlayerAction = true; }
                        else { reasonNot = "does not meet CRITERIA"; }
                    }
                    else { Debug.LogErrorFormat("Invalid nodeID (Null) for playerNodeID {0}", playerNodeID); }

                }
                else { reasonNot = "Failed Roll"; }
            }
            else { reasonNot = "Bad Node"; }
        }
        else { Debug.LogError("Invalid actorArcName (Null)"); reasonNot = "Error"; }
        if (isPlayerAction == true)
        { Debug.LogFormat("[Rim] AIRebelManager.cs -> CheckPlayerAction: Player will do {0} Action (needed {1}, rolled {2}){3}", actorArcName, playerAction, rnd, "\n"); }
        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> CheckPlayerAction: Player will not do {0} Action because {1}{2}", actorArcName, reasonNot, "\n"); }
        return isPlayerAction;
    }

    /// <summary>
    /// Checks onMap actors to see if that actorArc is present, if so returns actorID, if not randomly selects one onMap actor and returns their actorID. Returns -1 if a problem
    /// </summary>
    /// <param name="arc"></param>
    /// <returns></returns>
    private int FindActor(ActorArc arc)
    {
        //is there an actor of this type present and Active
        int actorID = GameManager.i.dataScript.CheckActorPresent(arc.name, globalResistance);
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
    /// Finds a suitable node (randomly selected) that is part of the specified actor's contact network, returns -1 if a problem (may not be any due to bad nodes, no contacts, etc.)
    /// </summary>
    /// <param name="actorID"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    private int FindNodeRandom(int actorID)
    {
        int count;
        int nodeID = -1;
        //get list of all nodes where actor has an active contact. 
        List<Node> listOfNodes = GameManager.i.dataScript.GetListOfActorContacts(actorID);
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
        else { Debug.LogFormat("[Rim] AIRebelManager.cs -> FindNodeRandom: actorID {0} unable to find any good contact nodes{1}", actorID, "\n"); }
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
            List<Node> tempList = GameManager.i.dataScript.GetListOfActorContacts(listOfCurrentActors[i].actorID);
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
                listOfActorsAtNode = GameManager.i.dataScript.CheckContactResistanceAtNode(tempNodeID);
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
        /*for (int i = 0; i < tempList.Count; i++)
        { Debug.LogFormat("[Tst] tempList[{0}] -> {1}{2}", i, tempList[i], "\n"); }*/
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
            case "RECRUITER":
                //No probe team present
                if (node.isProbeTeam == false)
                { isValid = true; }
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
            //Power/resource point expended (random chance regardless of whether gear is available or not)
            rnd = Random.Range(0, 100);
            if (rnd < gearPowerChance)
            {
                //deduct one resource but cannot go below zero
                int resources = GameManager.i.dataScript.CheckAIResourcePool(globalResistance);
                if (resources > 0)
                {
                    resources--;
                    GameManager.i.dataScript.SetAIResources(globalResistance, resources);
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> CheckGearAvailable: 1 resource expended on Gear retention (need {0}, rolled {1}){2}", gearPowerChance, rnd, "\n");
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

    /// <summary>
    /// Sets gear pool to Zero, used when AI resistance player caught by Bounty Hunter nemesis
    /// </summary>
    public void ResetGearPool()
    {
        gearPool = 0;
        Debug.LogFormat("[Rim] AIRebelManager.cs -> ResetGearPool: Gear Pool reset, now ZERO{0}", "\n");
    }

    //
    // - - - Tidy up - - -
    //


    /// <summary>
    /// restore connections back to original state (NOTE: recalculate dijkstra data done at end of all AIRebel processing) prior to leaving AIRebelManager.cs
    /// </summary>
    private void RestoreConnections()
    {
        GameManager.i.connScript.RestoreConnections();
        Debug.LogFormat("[Rim] AIRebelManager.cs -> RestoreConnections: Connections Restored to original state prior to AIRebelManager.cs calc's{0}", "\n");
    }

    /// <summary>
    /// resets dijktra weighted calculations back to normal ready for AIManager.cs
    /// </summary>
    private void RestoreDijkstraCalculations()
    {
        //recalculate weighted data
        GameManager.i.dijkstraScript.RecalculateWeightedData();
        isConnectionsChanged = false;
    }

    /// <summary>
    /// returns true if specific actor arc (uppercase, eg. 'FIXER') present in listOfArcs (the actor arcs chosen for this turns actions)
    /// </summary>
    /// <param name="actorArcID"></param>
    /// <returns></returns>
    private bool CheckActorArcPresent(string arcName)
    {
        if (string.IsNullOrEmpty(arcName) == false)
        { return listOfArcs.Exists(x => x.name.Equals(arcName, StringComparison.Ordinal)); }
        return false;
    }

    /// <summary>
    /// returns actorID of any OnMap, active, actor of the same type (uppercase, eg. 'FIXER'). If none, returns a random onMap, active, actor. Returns -1 if none that meet criteria
    /// </summary>
    /// <param name="actorArcID"></param>
    /// <returns></returns>
    private int GetOnMapActor(string arcName)
    {
        int actorID = -1;
        Actor[] arrayOfActors = GameManager.i.dataScript.GetCurrentActorsFixed(globalResistance);
        List<int> listOfActiveActorID = new List<int>();
        if (arrayOfActors != null)
        {
            //loop actors
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                //check actor is present in slot (not vacant)
                if (GameManager.i.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
                {
                    Actor actor = arrayOfActors[i];
                    if (actor != null)
                    {
                        //Active actor
                        if (actor.Status == ActorStatus.Active)
                        {
                            if (actor.arc.name.Equals(arcName) == true)
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
                /*else { Debug.LogError("Invalid listOfActiveActorID (no records)"); } Edit: 3Mar19 -> not needed as quite possible for there to be no actor which meets criteria*/
            }
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }
        return actorID;
    }

    /// <summary>
    /// Gear pool zeroed out for a reason in format 'due to ... [reason]'
    /// </summary>
    /// <param name="reason"></param>
    public void GearPoolEmpty(string reason)
    {
        if (string.IsNullOrEmpty(reason) == false)
        {
            Debug.LogFormat("[Rim] AIRebelManager.cs -> GearPoolEmpty: Gear Pool emptied (was {0} points) due to {1}{2}", gearPool, reason, "\n");
            gearPool = 0;
        }
        else { Debug.LogError("Invalid reason (Null or Empty)"); }
    }

    /// <summary>
    /// subMethod to keep tally of which AITaskTypes are being generated and placed into the pools (critical and potential), NOT a tally of which are executed
    /// </summary>
    /// <param name="listOfTasks"></param>
    private void UpdateTaskTypeStats(List<AITask> listOfTasks)
    {
        if (listOfTasks != null)
        {
            for (int i = 0; i < listOfTasks.Count; i++)
            {
                AITask task = listOfTasks[i];
                if (task != null)
                {
                    //add to tally
                    arrayOfAITaskTypes[(int)task.type]++;
                }
                else { Debug.LogErrorFormat("Invalid task (Null) for listOfTasks[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid listOfTasks (Null)"); }
    }

    //
    // - - - Serialization - - -
    //

    /// <summary>
    /// write private fields to serialization class
    /// </summary>
    /// <returns></returns>
    public SaveAIRebelClass WriteSaveData()
    {
        SaveAIRebelClass saveData = new SaveAIRebelClass();
        saveData.actionAllowance = actionAllowance;
        saveData.actionsUsed = actionsUsed;
        saveData.gearPool = gearPool;
        saveData.gearPointsUsed = gearPointsUsed;
        saveData.targetIntel = targetIntel;
        saveData.targetIntelUsed = targetIntelUsed;
        saveData.targetNodeID = targetNodeID;
        saveData.cureNodeID = cureNodeID;
        saveData.aiPlayerStartNodeID = aiPlayerStartNodeID;
        saveData.isConnectionsChanged = isConnectionsChanged;
        saveData.isPlayer = isPlayer;
        saveData.isCureNeeded = isCureNeeded;
        saveData.isCureCritical = isCureCritical;
        saveData.isPlayerStressed = isPlayerStressed;
        saveData.stressedActorID = stressedActorID;
        saveData.questionableID = questionableID;
        //return class to fileManager.cs for serialization
        return saveData;
    }

    /// <summary>
    /// copy data over from serialization class to private fields
    /// </summary>
    /// <param name="readData"></param>
    public void ReadSaveData(SaveAIRebelClass readData)
    {
        if (readData != null)
        {
            actionAllowance = readData.actionAllowance;
            actionsUsed = readData.actionsUsed;
            gearPool = readData.gearPool;
            gearPointsUsed = readData.gearPointsUsed;
            targetIntel = readData.targetIntel;
            targetIntelUsed = readData.targetIntelUsed;
            targetNodeID = readData.targetNodeID;
            cureNodeID = readData.cureNodeID;
            aiPlayerStartNodeID = readData.aiPlayerStartNodeID;
            isConnectionsChanged = readData.isConnectionsChanged;
            isPlayer = readData.isPlayer;
            isCureNeeded = readData.isCureNeeded;
            isCureCritical = readData.isCureCritical;
            isPlayerStressed = readData.isPlayerStressed;
            stressedActorID = readData.stressedActorID;
            questionableID = readData.questionableID;
        }
        else { Debug.LogError("Invalid SaveAIRebelClass readData (Null)"); }
    }


    public List<AITracker> GetListOfNemesisReports()
    { return listOfNemesisReports; }

    public List<AITracker> GetListOfErasureReports()
    { return listOfErasureReports; }

    /// <summary>
    /// clears and copies across save game data to listOfNemesisReports
    /// </summary>
    /// <param name="listOfReports"></param>
    public void SetListOfNemesisReports(List<AITracker> listOfReports)
    {
        if (listOfReports != null)
        {
            listOfNemesisReports.Clear();
            listOfNemesisReports.AddRange(listOfReports);
        }
        else { Debug.LogError("Invalid listOfReports (Null)"); }
    }

    /// <summary>
    /// clears and copies across save game data to listOfErasureReports
    /// </summary>
    /// <param name="listOfReports"></param>
    public void SetListOfErasureReports(List<AITracker> listOfReports)
    {
        if (listOfReports != null)
        {
            listOfErasureReports.Clear();
            listOfErasureReports.AddRange(listOfReports);
        }
        else { Debug.LogError("Invalid listOfReports (Null)"); }
    }


    public int[] GetArrayOfAITaskTypes()
    { return arrayOfAITaskTypes; }

    /// <summary>
    /// Clear array and copy across loaded save game data
    /// </summary>
    /// <param name="tempList"></param>
    public void SetArrayOfAITaskTypes(List<int> tempList)
    {
        if (tempList != null)
        {
            Array.Clear(arrayOfAITaskTypes, 0, arrayOfAITaskTypes.Length);
            arrayOfAITaskTypes = tempList.ToArray();
        }
        else { Debug.LogError("Invalid tempList (Null)"); }
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
        int turn = GameManager.i.turnScript.Turn;
        builder.AppendFormat(" Resistance AI Status {0}{1}", "\n", "\n");
        //player stats
        builder.AppendFormat("- AI Player \"{0}\"{1}", playerName, "\n");
        builder.AppendFormat(" status: {0} | {1}{2}", status, inactiveStatus, "\n");
        builder.AppendFormat(" isBreakdown: {0}{1}", isBreakdown, "\n");
        builder.AppendFormat(" Invisbility: {0}{1}", GameManager.i.playerScript.Invisibility, "\n");
        builder.AppendFormat(" Power: {0}{1}", GameManager.i.dataScript.CheckAIResourcePool(globalResistance), "\n");
        builder.AppendFormat(" Doom Timer: {0}{1}", GameManager.i.actorScript.doomTimer, "\n");
        builder.AppendFormat(" Capture Timer: {0}{1}", GameManager.i.actorScript.captureTimerPlayer, "\n");
        builder.AppendFormat(" Gear Pool: {0}{1}", gearPool, "\n");
        builder.AppendFormat(" Gear Used: {0}{1}", gearPointsUsed, "\n");
        builder.AppendFormat(" Target Intel: {0}{1}", targetIntel, "\n");
        builder.AppendFormat(" Target Intel Used: {0}{1}", targetIntelUsed, "\n");

        List<Condition> listOfConditions = GameManager.i.playerScript.GetListOfConditions(globalResistance);
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
            //
            // - - - Spider Nodes
            //
            count = listOfSpiderNodes.Count;
            builder.AppendFormat("{0}- ListOfSpiderNodes (current turn {1}){2}", "\n", turn - 1, "\n");
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                { builder.AppendFormat(" nodeID {0}{1}", listOfSpiderNodes[i], "\n"); }
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
        //Add condition to a set actor/player at a set turn (condition assumed to not be Null due to code at initialisation)
        int turn = GameManager.i.turnScript.Turn;
        //Add Condition
        if (turn == turnForCondition)
        {
            int slotID = GameManager.i.testScript.conditionWhoResistance;
            //Resistance player stressed
            if (slotID == 999)
            { GameManager.i.playerScript.AddCondition(conditionAutoRunTest, globalResistance, "for Debugging"); }
            else if (slotID > -1 && slotID < 4)
            {
                //Resistance actor given condition -> check present
                if (GameManager.i.dataScript.CheckActorSlotStatus(slotID, globalResistance) == true)
                {
                    Actor actor = GameManager.i.dataScript.GetCurrentActor(slotID, globalResistance);
                    if (actor != null)
                    { actor.AddCondition(conditionAutoRunTest, "Debug Test Action"); }
                    else { Debug.LogErrorFormat("Invalid actor (Null) for slotID {0}", slotID); }
                }
            }
        }
    }

    /// <summary>
    /// returns a breakdown of tasks by type from the start of the level to the current turn
    /// </summary>
    /// <returns></returns>
    public string DebugShowTaskAnalysis()
    {
        int total = 0;
        float typeShare = 0f;
        StringBuilder builder = new StringBuilder();
        Dictionary<AITaskType, float> dictTemp = new Dictionary<AITaskType, float>();
        
        //get a total of all tasks
        for (int i = 0; i < arrayOfAITaskTypes.Length; i++)
        { total += arrayOfAITaskTypes[i]; }
        //make calcs of % share of task
        for (int i = 0; i < arrayOfAITaskTypes.Length; i++)
        {
            if (arrayOfAITaskTypes[i] > 0)
            {
                typeShare = (float)arrayOfAITaskTypes[i] / (float)total * 100;
                //add to dictionary
                dictTemp.Add((AITaskType)i, typeShare);
            }
        }
        builder.AppendFormat("{0}{1}{2} Resistance ({3} tasks){4}", "\n", "\n", "\n", total, "\n");
        //sort dict descending order
        var result = from pair in dictTemp
                     orderby pair.Value descending
                     select pair;
        foreach(var item in result)
        { 
            //generate raw stats display
            builder.AppendFormat(" {0}: {1} %{2}", item.Key, (int)item.Value, "\n");
        }
        return builder.ToString();
    }

    /// <summary>
    /// Debug method to create a NodeActionData ACTOR record for testing topics during AutoRuns
    /// </summary>
    /// <param name="nodeID"></param>
    /// <param name="actorID"></param>
    /// <param name="nodeAction"></param>
    /// <param name="dataName"></param>
    private void DebugCreateNodeActionActorRecord(int nodeID, Actor actor, NodeAction nodeAction, string dataName)
    {
        if (GameManager.i.sideScript.PlayerSide.level == globalResistance.level)
        {
            if (actor != null)
            {
                //actor action
                NodeActionData nodeActionData = new NodeActionData()
                {
                    turn = GameManager.i.turnScript.Turn,
                    actorID = actor.actorID,
                    nodeID = nodeID,
                    dataName = dataName,
                    nodeAction = nodeAction
                };
                //add to actor's personal list
                actor.AddNodeAction(nodeActionData);
                /*Debug.LogFormat("[Tst] AIRebelManager.cs -> DebugCreateNodeActionActorRecord: nodeActionData added to {0}, {1}{2}", actor.actorName, actor.arc.name, "\n");*/
            }
            else { Debug.LogError("Invalid actor (Null) for DebugCreateAction ACTOR"); }
        }
    }


    /// <summary>
    /// Debug method to create a NodeActionData PLAYER record for testing topics during AutoRuns
    /// </summary>
    /// <param name="nodeID"></param>
    /// <param name="actorID"></param>
    /// <param name="nodeAction"></param>
    /// <param name="dataName"></param>
    private void DebugCreateNodeActionPlayerRecord(int nodeID, NodeAction nodeAction, string dataName)
    {
        if (GameManager.i.sideScript.PlayerSide.level == globalResistance.level)
        {
                //actor action
                NodeActionData nodeActionData = new NodeActionData()
                {
                    turn = GameManager.i.turnScript.Turn,
                    actorID = 999,
                    nodeID = nodeID,
                    dataName = dataName,
                    nodeAction = nodeAction
                };
                //add to player's personal list
                GameManager.i.playerScript.AddNodeAction(nodeActionData);
                /*Debug.LogFormat("[Tst] AIRebelManager.cs -> DebugCreateNodeActionPlayerRecord: nodeActionData added to {0}, {1}{2}", GameManager.instance.playerScript.PlayerName, "Player", "\n");*/
        }
    }


    //new methods above here
}
