using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using System.Text;
using System;
using System.Linq;
using Random = UnityEngine.Random;

/// <summary>
/// AI data package used to collate all node info where a node has degraded in some form
/// </summary>
public class AINodeData
{
    public int nodeID;
    public int score;                       //used by spider & others to give the node a rating. Higher the score, the more likely it is to be chosen
    public NodeData type;
    public NodeArc arc;
    public int difference;                  //shows difference between current and start values
    public int current;                     //shows current value
    public bool isPreferred;                //true if preferred type of node for that side's faction
}

/// <summary>
/// AI data package detailing an Authority task that is ready to be executed next turn
/// </summary>
public class AITask
{
    public int taskID;                     //automatically assigned
    public int data0;                      //could be node, connection ID or teamID
    public int data1;                      //teamArcID, decision cost in resources
    public int data2;                      //aiDeciID if a decision, otherwise ignored
    public string name0;                   //node arc name, decision name
    public string name1;                   //could be team arc name, eg. 'CIVIL'
    public Priority priority;
    public AIType type;                     //what type of task
    public int chance;                      //dynamically added by ProcessTasksFinal (for display to player of % chance of this task being chosen)

    public AITask()
    { taskID = GameManager.instance.aiScript.aiTaskCounter++; }
}

/// <summary>
/// extracted data from AI messages (at time of AI becoming aware of them)
/// </summary>
public class AITracker
{
    public int data0;                       //node or connectionID
    public int turn;                        //turn occurred

    public AITracker(int data, int turn)
    { data0 = data; this.turn = turn; }
}

/// <summary>
/// data package to populate AIDisplayUI
/// </summary>
public class AIDisplayData
{
    public int rebootTimer;                 //AIDisplayUI will only open (allow hacking attempts) if timer = 0 (which infers that isRebooting = false)
    public string task_1_textUpper;
    public string task_1_textLower;
    public string task_1_chance;
    public string task_1_tooltipMain;
    public string task_1_tooltipDetails;
    public string task_2_textUpper;
    public string task_2_textLower;
    public string task_2_chance;
    public string task_2_tooltipMain;
    public string task_2_tooltipDetails;
    public string task_3_textUpper;
    public string task_3_textLower;
    public string task_3_chance;
    public string task_3_tooltipMain;
    public string task_3_tooltipDetails;
    public string factionDetails;
    public string renownDecision;
    public int nodeID_1;                   //used for highlighting node or connection referred to by task
    public int connID_1;
    public int nodeID_2;
    public int connID_2;
    public int nodeID_3;
    public int connID_3;

    public AIDisplayData()
        {
        task_1_textUpper = ""; task_1_textLower = ""; task_1_chance = ""; task_1_tooltipMain = ""; task_1_tooltipDetails = "";
        task_2_textUpper = ""; task_2_textLower = ""; task_2_chance = ""; task_2_tooltipMain = ""; task_2_tooltipDetails = "";
        task_3_textUpper = ""; task_3_textLower = ""; task_3_chance = ""; task_3_tooltipMain = ""; task_3_tooltipDetails = "";
        nodeID_1 = -1; nodeID_2 = -1; nodeID_3 = -1;
        connID_1 = -1; connID_2 = -1; connID_3 = -1;
        }

}

/// <summary>
/// data package to populate AIDisplayUI (needs to be separate because it's dynamic data whereas AIDisplayData is sent once during AIEndOfTurn)
/// </summary>
public class AIHackingData
{
    public string hackingStatus;            //combined string of AI Alert Status and number of hacking attempts
    public string tooltipHeader;
    public string tooltipMain;              //combined string for tooltip
    public string tooltipDetails;
}

/// <summary>
/// data package to populate AISideTabUI
/// </summary>
public class AISideTabData
{
    public string topText;              //eg. 'A.I' but colour formatted
    public string bottomText;           //eg. Renown cost to hack or 'X' if not possible, greyed out if not enough renown
    public string tooltipMain;
    public string tooltipDetails;
    public HackingStatus status;        //used to determine what happens when player clicks AI Side Tab UI
}

/// <summary>
/// Handles AI management of both sides
/// </summary>
public class AIManager : MonoBehaviour
{
    [Tooltip("The % of the total map, from the centre outwards, that encompasses the geographic centre where any node in the area is node.isCentreNode true")]
    [Range(0, 100)] public float nodeGeographicCentre = 30f;
    [Tooltip("When a target is attempted and the attempt fails this is the % chance of Authority becoming aware of the target (node.isTargetKnown true)")]
    [Range(0, 100)] public int targetAttemptChance = 50;
    [Tooltip("How many turns, after the event, that the AI will track Connection & Node activity before ignoring it")]
    [Range(5, 15)] public int activityTimeLimit = 10;

    [Header("Priorities")]
    [Tooltip("When selecting Non-Critical tasks where there are an excess to available choices how much relative weight do I assign to High Priority tasks")]
    [Range(0, 10)] public int priorityHighWeight = 3;
    [Tooltip("When selecting Non-Critical tasks where there are an excess to available choices how much relative weight do I assign to Medium Priority tasks")]
    [Range(0, 10)] public int priorityMediumWeight = 2;
    [Tooltip("When selecting Non-Critical tasks where there are an excess to available choices how much relative weight do I assign to Low Priority tasks")]
    [Range(0, 10)] public int priorityLowWeight = 1;

    [Header("Tracking")]
    [Tooltip("How many of the most recent AI activities are tracked (keeps this number of most recent in a priorityQueue)")]
    [Range(0, 10)] public int numOfActivitiesTracked = 5;
    [Tooltip("How many turns ago (activity wise) will the AI use to select a target node for Erasure team AI processing")]
    [Range(0, 5)] public int trackerNumOfTurnsAgoErasure = 2;
    [Tooltip("How many turns ago (activity wise) will the AI use to select a target node for Nemesis AI processing")]
    [Range(0, 5)] public int trackerNumOfTurnsAgoNemesis = 2;

    [Header("Nodes")]
    [Tooltip("The listOfMostConnectedNodes includes all nodes with this number, or greater, connections. If < 3 nodes then the next set down are added. Max capped at total nodes/2")]
    [Range(0, 5)] public int nodeConnectionThreshold = 3;
    [Tooltip("Spider/Erasure Team node score -> Each resistance activity point is multiplied by this factor")]
    [Range(0, 5)] public int nodeActivityCountFactor = 1;
    [Tooltip("Spider/Erasure Team node score -> Factor - (Current turn - Activity last Turn)")]
    [Range(0, 5)] public int nodeActivityTimeFactor = 4;
    [Tooltip("Spider/Erasure Team node score -> added to score if node is a chokepoint ('isChokepointNode' true)")]
    [Range(0, 5)] public int nodeChokepointFactor = 5;
    [Tooltip("Erasure Team node score -> added if an uncompleted target, subtracted if a completed target")]
    [Range(0, 5)] public int nodeTargetFactor = 3;
    [Tooltip("Erasure Team node score -> Added to score if a spider is present and Not know. If known then effect is halved")]
    [Range(0, 5)] public int nodeSpiderFactor = 5;
    [Tooltip("Spider/Erasure Team node score -> If node is preferred type of current level authority faction this is added to the score")]
    [Range(0, 5)] public int nodePreferredFactor = 3;
    [Tooltip("Erasure Team node score -> added to score if node is in list of most connected nodes")]
    [Range(0, 5)] public int nodeConnectedFactor = 3;
    [Tooltip("Spider Team node score -> If node is in the designated central region of the map (see 'nodeGeographicCentre') this is added to the score")]
    [Range(0, 5)] public int nodeCentreFactor = 3;

    [Header("Connections")]
    [Tooltip("Spider Team node score -> This is added to the score for ever 'None' security connection the node has")]
    [Range(0, 5)] public int securityNoneFactor = 3;
    [Tooltip("Spider Team node score -> This is added to the score for ever Low security connection the node has")]
    [Range(0, 5)] public int securityLowFactor = 2;
    [Tooltip("Spider Team node score -> This is added to the score for ever Medium security connection the node has")]
    [Range(0, 5)] public int securityMediumFactor = 1;
    [Tooltip("Spider Team node score -> This is added to the score for ever High security connection the node has")]
    [Range(0, 5)] public int securityHighFactor = 0;

    [Header("AI Tasks")]
    [Tooltip("The aiming point for the number of final tasks (if available) generated each turn. The faction chooses their set num of tasks out of this pool")]
    [Range(1, 3)] public int numOfFinalTasks = 3;

    [Header("Teams")]
    [Tooltip("Pool of tasks for Spider team -> number of entries for a Known target")]
    [Range(0, 5)] public int teamPoolTargetFactor = 3;
    [Tooltip("Pool of tasks for Spider/Erasure team -> number of entries for the top scored node")]
    [Range(0, 5)] public int teamPoolFirstFactor = 3;
    [Tooltip("Pool of tasks for Spider/Erasure team -> number of entries for the second top scored node")]
    [Range(0, 5)] public int teamPoolSecondFactor = 2;
    [Tooltip("Pool of tasks for Spider/Erasure team -> number of entries for the third top scored node")]
    [Range(0, 5)] public int teamPoolThirdFactor = 1;

    [Header("Decisions")]
    [Tooltip("The ratio level above which no more Connection Security decisions will be made")]
    [Range(0f, 1f)] public float connectionRatioThreshold = 0.75f;
    [Tooltip("The ratio level above which no more Request Team decisions will be made")]
    [Range(0f, 1f)] public float teamRatioThreshold = 0.75f;
    [Tooltip("The point below which a low Resource Pool situation (isLowResources true) is declared and a request for more resources can be made")]
    [Range(0, 10)] public int lowResourcesThreshold = 2;
    [Tooltip("The base % chance of a Request Resources decision being approved")]
    [Range(0, 100)] public int resourcesChance = 50;
    [Tooltip("This amount will be added to the chance of a resource request being approved once per unsuccessful attempt")]
    [Range(0, 100)] public int resourcesBoost = 15;

    [Header("Policy Decisions")]
    [Tooltip("The level of City Loyalty, or above, at which policies countering crisis will be considered")]
    [Range(0, 10)] public int policyCrisisLoyaltyCriteria = 4;
    [Tooltip("The number, or above, of confirmed District Crisis before Low impact Crisis policies will be considered")]
    [Range(0, 10)] public int policyCrisisCriteriaLow = 1;
    [Tooltip("The number, or above, of confirmed District Crisis before Medium impact Crisis policies will be considered")]
    [Range(0, 10)] public int policyCrisisCriteriaMed = 3;
    [Tooltip("The number, or above, of confirmed District Crisis before High impact Crisis policies will be considered")]
    [Range(0, 10)] public int policyCrisisCriteriaHigh = 5;
    [Tooltip("The amount City Loyalty changes by (down when policy implemented, up when policy cancelled) LOW impact")]
    [Range(1, 3)] public int policyCityLoyaltyLow = 1;
    [Tooltip("The amount City Loyalty changes by (down when policy implemented, up when policy cancelled) MED impact")]
    [Range(1, 3)] public int policyCityLoyaltyMed = 2;
    [Tooltip("The amount City Loyalty changes by (down when policy implemented, up when policy cancelled) HIGH impact")]
    [Range(1, 3)] public int policyCityLoyaltyHigh = 3;
    [Tooltip("The amount that the base chance of a node crisis occuring lessens (makes it harder for a crisis) while policy in force. LOW impact")]
    [Range(10, 50)] public int policyNodeCrisisLow = 15;
    [Tooltip("The amount that the base chance of a node crisis occuring lessens (makes it harder for a crisis) while policy in force. MED impact")]
    [Range(10, 50)] public int policyNodeCrisisMed = 30;
    [Tooltip("The amount that the base chance of a node crisis occuring lessens (makes it harder for a crisis) while policy in force. HIGH impact")]
    [Range(10, 50)] public int policyNodeCrisisHigh = 45;

    [Header("Handouts")]
    [Tooltip("City Loyalty level, at or below, where Low Impact handouts will be considered")]
    [Range(1, 10)] public int handoutLoyaltyCriteriaLow = 7;
    [Tooltip("City Loyalty level, at or below, where Medium Impact handouts will be considered")]
    [Range(1, 10)] public int handoutLoyaltyCriteriaMed = 5;
    [Tooltip("City Loyalty level, at or below, where High Impact handouts will be considered")]
    [Range(1, 10)] public int handoutLoyaltyCriteriaHigh = 3;
    [Tooltip("This sets the number of turns for the handout cooldown timer")]
    [Range(0, 10)] public int handoutCooldownTimer = 4;
    [Tooltip("The amount City Loyalty increases by for a Low Impact handout")]
    [Range(1, 5)] public int handoutCityLoyaltyLow = 1;
    [Tooltip("The amount City Loyalty increases by for a Medium Impact handout")]
    [Range(1, 5)] public int handoutCityLoyaltyMed = 2;
    [Tooltip("The amount City Loyalty increases by for a High Impact handout")]
    [Range(1, 5)] public int handoutCityLoyaltyHigh = 3;

    [Header("Hacking AI")]
    [Tooltip("Base cost, in renown, to hack AI at start of a level")]
    [Range(0, 5)] public int hackingBaseCost = 2;
    [Tooltip("Amount that the hackingBaseCost increases everytime AI Reboots")]
    [Range(0, 3)] public int hackingIncrement = 1;
    [Tooltip("% base chance that each hacking attempt will lead to an increase in AI Alert Level")]
    [Range(1, 100)] public int hackingDetectBaseChance = 50;
    [Tooltip("How many turns (inclusive of current) does it take to reboot the AI' Security Systems (hacking isn't possible during a reboot")]
    [Range(2, 10)] public int hackingRebootTimer = 3;
    [Tooltip("How much of a modifier is a 'Lower Detection' gear effect have on your chances of being detected while hacking AI")]
    [Range(0, 50)] public int hackingLowDetectionEffect = 20;
    [Tooltip("How much of a modifier does the Player being STRESSED have on their chances of being detected while hacking AI")]
    [Range(0, 50)] public int hackingStressedDetectionEffect = 25;
    [Tooltip("Each level of AI Security Protocol increases the chance of detecting a hacking attempt by this much")]
    [Range(0, 50)] public int hackingSecurityProtocolFactor = 10;
    [Tooltip("Mayoral Traits that increase / decrease the chance of detecting an AI hacking attempt are adjusted by this amount")]
    [Range(0, 50)] public int hackingTraitDetectionFactor = 20;
    

    [Header("AI Countermeasures")]
    [Tooltip("The highest level that the AI can raise it's Security Protocols to in order to increase the chances of detecting Hacking")]
    [Range(1, 5)] public int aiSecurityProtocolMaxLevel = 3;

    [Header("Timers")]
    [Tooltip("When the AI instigates any Network counter measures they will stay in place for this number of turns")]
    [Range(1, 10)] public int aiCounterMeasureTimer = 5;
    [Tooltip("When the AI instigates any Policies they will stay in place for this number of turns")]
    [Range(1, 10)] public int aiPolicyTimer= 5;



    [HideInInspector] public bool immediateFlagAuthority;               //true if any authority activity that flags immediate notification
    [HideInInspector] public bool immediateFlagResistance;              //true if any resistance activity that flags immediate notification, eg. activity while invis 0
    [HideInInspector] public int resourcesGainAuthority;                //resources added to pool (DataManager.cs -> arrayOfAIResources every turn
    [HideInInspector] public int resourcesGainResistance;
    [HideInInspector] public int aiTaskCounter;                         //AITask ID counter (reset every turn)
    //hacking
    [HideInInspector] public int hackingAttemptsTotal;                  //number of hacking attempts overall
    [HideInInspector] public int hackingAttemptsReboot;                 //number of hacking attempts since last reboot
    [HideInInspector] public int hackingAttemptsDetected;               //number of times hacking detected by AI since last reboot
    [HideInInspector] public int hackingCurrentCost;                    //base current cost to hack
    [HideInInspector] public int hackingModifiedCost;                   //base cost to hack modified by gear, etc.
    [HideInInspector] public bool isHacked;                             //true if player has already hacked AI this turn, reset each turn
    [HideInInspector] public Priority aiAlertStatus;
    [HideInInspector] public bool isRebooting;                          //true if AI Security System is rebooting and hacking not possible (external access cut)
    [HideInInspector] public int rebootTimer;                           //how many times does it take to reboot, (rebooted when timer reaches zero)
    [HideInInspector] public int numOfCrisis;                           //tally of number of node crisis that have occured so far
    
    //ai countermeasure flags
    private bool isOffline;                            //if true AI DisplayUI is offline and can't be hacked by the player
    private bool isTraceBack;                          //if true AI has ability to trace back whenever AI hacking detected and find player and drop their invisibility
    private bool isScreamer;                           //if true AI has ability to give Player STRESSED condition whenever they detect a hacking attempt
    private bool isPolicy;                             //true if a  policy is in currently in force (blocks other policies from occurring -> one at a time)
    //ai countermeasures
    private int timerTraceBack;
    private int aiSecurityProtocolLevel;                //each level of security provides a 'HackingSecurityProtocolFactor' * level increased risk of hacking attempt detection
    private int timerScreamer;
    private int timerOffline;
    private int timerHandout;                           //cooldown timer before another handout is possible
    private int timerPolicy;
    private string policyName;                          //name of current policy in play, null if none
    private int policyEffectCrisis;                     //int for use with a tag detailing effect of policy on node crisis chance, eg. "District Crisis base chance -15%"
    private int policyEffectLoyalty;                    //int for use with a tag detailing effect of Policy on City Loyalty, eg. "City Loyalty -2 (while policy in force)"
    //hacking
    private int detectModifierMayor;                    //modifiers to base chance of AI detecting an hacking attempt (HackingDetectBaseChance)
    private int detectModifierFaction;
    private int detectModifierGear;
    //factions
    private Faction factionAuthority;
    private Faction factionResistance;
    private string authorityPreferredArc;                               //string name of preferred node Arc for faction (if none then null)
    private string resistancePreferredArc;
    private int authorityMaxTasksPerTurn;                               //how many tasks the AI can undertake in a turns
    private int resistanceMaxTasksPerTurn;
    //player target (Nemesis / Erasure teams)
    private int playerTargetNodeID;                                     //most likely node where player is, -1 if no viable recent information available

    //decision data
    private float connSecRatio;
    private float teamRatio;
    private int erasureTeamsOnMap;
    private bool isInsufficientResources;                               //true whenever not enough resources to implement a decision (triggers 'Request Resources')
    private int numOfUnsuccessfulResourceRequests;                      //running tally, reset back to zero once a request is APPROVED
    private int numOfSuccessfulResourceRequests;

    //fast access -> teams
    private int teamArcCivil = -1;
    private int teamArcControl = -1;
    private int teamArcMedia = -1;
    private int teamArcProbe = -1;
    private int teamArcSpider = -1;
    private int teamArcDamage = -1;
    private int teamArcErasure = -1;
    private int maxTeamsAtNode = -1;
    //traits
    private int aiDetectionChanceHigher;
    private int aiDetectionChanceLower;
    private int aiCounterMeasurePriorityRaise;
    private int aiCounterMeasureTimerDoubled;
    private int aiPolicyTimerDoubled;
    private int aiPolicyCostLower;
    private int aiPolicyCostHigher;
    private int aiHandoutCostLower;
    private int aiHandoutCostHigher;
    //conditions
    private Condition stressedCondition;
    //sides
    private GlobalSide globalAuthority;
    private GlobalSide globalResistance;
    private City city;
    private int totalNodes;
    private int totalConnections;
    private Condition conditionStressed;
    //decisions Authority
    private DecisionAI decisionAPB;
    private DecisionAI decisionConnSec;
    private DecisionAI decisionRequestTeam;
    private DecisionAI decisionSecAlert;
    private DecisionAI decisionCrackdown;
    private DecisionAI decisionResources;
    private DecisionAI decisionTraceBack;
    private DecisionAI decisionScreamer;
    private DecisionAI decisionProtocol;
    private DecisionAI decisionOffline;
    private DecisionAI decisionCensorship;
    private DecisionAI decisionCurfew;
    private DecisionAI decisionBanProtests;
    private DecisionAI decisionMartialLaw;
    private DecisionAI decisionRoboCop;
    private DecisionAI decisionDrones;
    private DecisionAI decisionHamper;
    private DecisionAI decisionAusterity;
    private DecisionAI decisionMedical;
    private DecisionAI decisionBlindEye;
    private DecisionAI decisionHoliday;
    //text strings
    private string traceBackFormattedText;                                   //specially formatted string (uncoloured) for tooltips
    private string screamerFormattedText;
    //countermeasure effects
    private string traceBackEffectText;                                      //effect name, eg. "TraceBack Mask"
    private string screamerEffectText;
    //gear effects
    private string cheapHackingEffectText;                                   //effect of resistance Hacking gear
    private string freeHackingEffectText;
    private string invisibileHackingEffectText;
    private string lowerDetectionEffectText;
    //colour palette 
    private string colourGood;
    private string colourNeutral;
    private string colourGrey;
    private string colourBad;
    private string colourNormal;
    private string colourAlert;
    private string colourEnd;

    //info gathering lists (collated every turn)
    List<AINodeData> listNodeMaster = new List<AINodeData>();
    List<AINodeData> listStabilityCritical = new List<AINodeData>();
    List<AINodeData> listStabilityNonCritical = new List<AINodeData>();
    List<AINodeData> listSecurityCritical = new List<AINodeData>();
    List<AINodeData> listSecurityNonCritical = new List<AINodeData>();
    List<AINodeData> listSupportCritical = new List<AINodeData>();
    List<AINodeData> listSupportNonCritical = new List<AINodeData>();
    List<AINodeData> listOfTargetsKnown = new List<AINodeData>();
    List<AINodeData> listOfTargetsDamaged = new List<AINodeData>();
    List<AINodeData> listOfProbeNodes = new List<AINodeData>();
    List<AINodeData> listOfSpiderNodes = new List<AINodeData>();
    List<AINodeData> listOfErasureNodes = new List<AINodeData>();
    //other
    List<string> listOfErasureAILog = new List<string>();
    List<string> listOfPlayerEffects = new List<string>();
    List<string> listOfPlayerEffectDescriptors = new List<string>();
    //nemesis
    List<AITracker> listOfPlayerActivity = new List<AITracker>();
    //tasks
    List<AITask> listOfTasksPotential = new List<AITask>();
    List<AITask> listOfTasksFinal = new List<AITask>();
    List<AITask> listOfSpiderTasks = new List<AITask>();
    List<AITask> listOfErasureTasks = new List<AITask>();
    List<AITask> listOfDecisionTasksNonCritical = new List<AITask>();
    List<AITask> listOfDecisionTasksCritical = new List<AITask>();


    public void Initialise()
    {
        //factions
        factionAuthority = GameManager.instance.factionScript.factionAuthority;
        factionResistance = GameManager.instance.factionScript.factionResistance;
        Debug.Assert(factionAuthority != null, "Invalid factionAuthority (Null)");
        Debug.Assert(factionResistance != null, "Invalid factionResistance (Null)");
        //decision data
        totalNodes = GameManager.instance.dataScript.CheckNumOfNodes();
        totalConnections = GameManager.instance.dataScript.CheckNumOfConnections();
        numOfUnsuccessfulResourceRequests = 0;
        numOfSuccessfulResourceRequests = 0;
        numOfCrisis = 0;
        isInsufficientResources = false;
        //decision ID's
        int aiDecID = GameManager.instance.dataScript.GetAIDecisionID("APB");
        decisionAPB = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Connection Security");
        decisionConnSec = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Request Team");
        decisionRequestTeam = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Security Alert");
        decisionSecAlert = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Surveillance Crackdown");
        decisionCrackdown = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Request Resources");
        decisionResources = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("TraceBack");
        decisionTraceBack = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Screamer");
        decisionScreamer = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Offline");
        decisionOffline = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Security Protocol");
        decisionProtocol = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Censorship");
        decisionCensorship = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Ban Protests");
        decisionBanProtests = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Martial Law");
        decisionMartialLaw = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Curfew");
        decisionCurfew = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Robo Cops");
        decisionRoboCop = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Drone Warfare");
        decisionDrones = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Christmas Hampers");
        decisionHamper = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Austerity Payment");
        decisionAusterity = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Medical Care");
        decisionMedical = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Blind Eye");
        decisionBlindEye = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        aiDecID = GameManager.instance.dataScript.GetAIDecisionID("Holiday");
        decisionHoliday = GameManager.instance.dataScript.GetAIDecision(aiDecID);
        Debug.Assert(decisionAPB != null, "Invalid decisionAPB (Null)");
        Debug.Assert(decisionConnSec != null, "Invalid decisionConnSec (Null)");
        Debug.Assert(decisionRequestTeam != null, "Invalid decisionRequestTeam (Null)");
        Debug.Assert(decisionSecAlert != null, "Invalid decisionSecAlert (Null)");
        Debug.Assert(decisionCrackdown != null, "Invalid decisionCrackdown (Null)");
        Debug.Assert(decisionResources != null, "Invalid decisionResources (Null)");
        Debug.Assert(decisionTraceBack != null, "Invalid decisionTraceBack (Null)");
        Debug.Assert(decisionScreamer != null, "Invalid decisionScreamer (Null)");
        Debug.Assert(decisionOffline != null, "Invalid decisionOffline (Null)");
        Debug.Assert(decisionProtocol != null, "Invalid decisionProtocol (Null)");
        Debug.Assert(decisionCensorship != null, "Invalid decisionCensorWeb (Null)");
        Debug.Assert(decisionBanProtests != null, "Invalid decisionBanProtests (Null)");
        Debug.Assert(decisionMartialLaw != null, "Invalid decisionMartialLaw (Null)");
        Debug.Assert(decisionCurfew != null, "Invalid decisionCurfew (Null)");
        Debug.Assert(decisionRoboCop != null, "Invalid decisionRoboCops (Null)");
        Debug.Assert(decisionDrones != null, "Invalid decisionDrones (Null)");
        Debug.Assert(decisionHamper != null, "Invalid decisionHamper (Null)");
        Debug.Assert(decisionAusterity != null, "Invalid decisionAusterity (Null)");
        Debug.Assert(decisionMedical != null, "Invalid decisionMedical (Null)");
        Debug.Assert(decisionBlindEye != null, "Invalid decisionBlindEye (Null)");
        Debug.Assert(decisionHoliday != null, "Invalid decisionHoliday (Null)");
        //conditions
        stressedCondition = GameManager.instance.dataScript.GetCondition("STRESSED");
        Debug.Assert(stressedCondition != null, "Invalid stressedCondition (Null)");
        //sides
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
        conditionStressed = GameManager.instance.dataScript.GetCondition("STRESSED");
        city = GameManager.instance.cityScript.GetCity();
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(conditionStressed != null, "Invalid conditionStressed (Null)");
        Debug.Assert(city != null, "Invalid City (Null)");
        //cached TraitEffects
        aiDetectionChanceHigher = GameManager.instance.dataScript.GetTraitEffectID("AIDetectionChanceHigher");
        aiDetectionChanceLower = GameManager.instance.dataScript.GetTraitEffectID("AIDetectionChanceLower");
        aiCounterMeasurePriorityRaise = GameManager.instance.dataScript.GetTraitEffectID("AICounterMeasurePriorityRaise");
        aiCounterMeasureTimerDoubled = GameManager.instance.dataScript.GetTraitEffectID("AICounterMeasureTimerDoubled");
        aiPolicyTimerDoubled = GameManager.instance.dataScript.GetTraitEffectID("AIPolicyTimerDoubled");
        aiPolicyCostLower = GameManager.instance.dataScript.GetTraitEffectID("AIPolicyCostLower");
        aiPolicyCostHigher = GameManager.instance.dataScript.GetTraitEffectID("AIPolicyCostHigher");
        aiHandoutCostLower = GameManager.instance.dataScript.GetTraitEffectID("AIHandoutCostLower");
        aiHandoutCostHigher = GameManager.instance.dataScript.GetTraitEffectID("AIHandoutCostHigher");
        Debug.Assert(aiDetectionChanceHigher > -1, "Invalid aiDetectionChanceHigher (-1)");
        Debug.Assert(aiDetectionChanceLower > -1, "Invalid aiDetectionChanceLower (-1)");
        Debug.Assert(aiCounterMeasurePriorityRaise > -1, "Invalid aiCounterMeasurePriorityRaise (-1)");
        Debug.Assert(aiCounterMeasureTimerDoubled > -1, "Invalid aiCounterMeasuresTimerDoubled (-1)");
        //get names of node arcs (name or null, if none)
        if (factionAuthority.preferredArc != null) { authorityPreferredArc = factionAuthority.preferredArc.name; }
        if (factionResistance.preferredArc != null) { resistancePreferredArc = factionResistance.preferredArc.name; }
        authorityMaxTasksPerTurn = factionAuthority.maxTaskPerTurn;
        resistanceMaxTasksPerTurn = factionResistance.maxTaskPerTurn;
        Debug.Assert(authorityMaxTasksPerTurn > -1, "Invalid authorityMaxTasksPerTurn (-1)");
        Debug.Assert(resistanceMaxTasksPerTurn > -1, "Invalid resistanceMaxTasksPerTurn (-1)");
        //fast access
        teamArcCivil = GameManager.instance.dataScript.GetTeamArcID("CIVIL");
        teamArcControl = GameManager.instance.dataScript.GetTeamArcID("CONTROL");
        teamArcMedia = GameManager.instance.dataScript.GetTeamArcID("MEDIA");
        teamArcProbe = GameManager.instance.dataScript.GetTeamArcID("PROBE");
        teamArcSpider = GameManager.instance.dataScript.GetTeamArcID("SPIDER");
        teamArcDamage = GameManager.instance.dataScript.GetTeamArcID("DAMAGE");
        teamArcErasure = GameManager.instance.dataScript.GetTeamArcID("ERASURE");
        maxTeamsAtNode = GameManager.instance.teamScript.maxTeamsAtNode;
        Debug.Assert(teamArcCivil > -1, "Invalid teamArcCivil");
        Debug.Assert(teamArcControl > -1, "Invalid teamArcControl");
        Debug.Assert(teamArcMedia > -1, "Invalid teamArcMedia");
        Debug.Assert(teamArcProbe > -1, "Invalid teamArcProbe");
        Debug.Assert(teamArcSpider > -1, "Invalid teamArcSpider");
        Debug.Assert(teamArcDamage > -1, "Invalid teamArcDamage");
        Debug.Assert(teamArcErasure > -1, "Invalid teamArcErasure");
        Debug.Assert(maxTeamsAtNode > -1, "Invalid maxTeamsAtNode");
        //text strings (uncoloured)
        traceBackFormattedText = "<font=\"Bangers SDF\"><cspace=1em><size=120%>TRACEBACK</size></cspace></font>";
        screamerFormattedText = "<font=\"Bangers SDF\"><cspace=1em><size=120%>SCREAMER</size></cspace></font>";
        traceBackEffectText = "TraceBack Mask";
        screamerEffectText = "Screamer Mask";
        cheapHackingEffectText = "Cheap Hacking";
        freeHackingEffectText = "Free Hacking";
        invisibileHackingEffectText = "Invisible Hacking";
        lowerDetectionEffectText = "Lower Detection";
        //Hacking
        hackingAttemptsTotal = 0;
        hackingAttemptsReboot = 0;
        hackingAttemptsDetected = 0;
        hackingCurrentCost = hackingBaseCost;
        aiAlertStatus = Priority.Low;
        aiSecurityProtocolLevel = 0;
        isRebooting = false;
        rebootTimer = 0;
        isOffline = false;
        isTraceBack = false;
        isScreamer = false;
        isPolicy = false;
        timerTraceBack = -1;
        timerScreamer = -1;
        timerOffline = -1;
        timerPolicy = -1;
        timerHandout = 0;
        //set up list of most connected Nodes
        SetConnectedNodes();
        SetPreferredNodes();
        SetCentreNodes();
        SetDecisionNodes();
        SetNearNeighbours();
        SetColours();
        //event listeners
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "AIManager");
        EventManager.instance.AddListener(EventType.StartTurnEarly, OnEvent, "AIManager");
    }

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
            case EventType.StartTurnEarly:
                StartTurnEarly();
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
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// Runs Resistance turn on behalf of AI
    /// TurnManager.cs -> ProcessNewTurn -> EndTurnFinalAI
    /// </summary>
    public void ProcessAISideResistance()
    {
        Debug.Log(string.Format("[Aim] -> ProcessAISideResistance -> turn {0}{1}", GameManager.instance.turnScript.Turn, "\n"));
        ExecuteTasks(resistanceMaxTasksPerTurn);
        ClearAICollections();
        UpdateResources(globalResistance);
        //reset flags
        immediateFlagAuthority = false;
    }

    /// <summary>
    /// Runs Authority turn on behalf of AI
    /// TurnManager.cs -> ProcessNewTurn -> EndTurnFinalAI
    /// </summary>
    public void ProcessAISideAuthority()
    {
        Debug.Log(string.Format("[Aim] -> ProcessAISideAuthority -> turn {0}{1}", GameManager.instance.turnScript.Turn, "\n"));
        ExecuteTasks(authorityMaxTasksPerTurn);
        ClearAICollections();
        UpdateResources(globalAuthority);
        //AI Status checks
        UpdateRebootStatus();
        UpdateCounterMeasureTimers();
        //Info Gathering      
        GetAINodeData();
        ProcessNodeData();
        ProcessSpiderData();
        ProcessErasureData();
        ProcessDecisionData();
        //AI Rulesets
        ProcessNodeTasks();
        ProcessProbeTask();
        ProcessSpiderTask();
        ProcessDamageTask();
        ProcessErasureTask();
        ProcessDecisionTask();
        //choose tasks for the following turn
        ProcessFinalTasks(authorityMaxTasksPerTurn);
        //Nemesis
        if (GameManager.instance.nemesisScript.nemesis != null)
        {
            AITracker tracker = ProcessNemesisTarget();
            GameManager.instance.nemesisScript.ProcessNemesis(tracker, immediateFlagResistance);
        }
        //reset flags
        immediateFlagResistance = false;
        isHacked = false;
    }

    /// <summary>
    /// run prior to any info gathering each turn to empty out all data collections 
    /// All AIManager collections are for turn based data. Longer term data kept at DataManager.cs
    /// </summary>
    private void ClearAICollections()
    {
        //reset AITask ID counter
        aiTaskCounter = 0;
        //AINodeData
        listNodeMaster.Clear();
        listStabilityCritical.Clear();
        listStabilityNonCritical.Clear();
        listSecurityCritical.Clear();
        listSecurityNonCritical.Clear();
        listSupportCritical.Clear();
        listSupportNonCritical.Clear();
        listOfTargetsKnown.Clear();
        listOfTargetsDamaged.Clear();
        listOfProbeNodes.Clear();
        listOfSpiderNodes.Clear();
        listOfErasureNodes.Clear();
        //other
        listOfErasureAILog.Clear();
        //AITasks
        listOfTasksFinal.Clear();
        listOfTasksPotential.Clear();
        listOfSpiderTasks.Clear();
        listOfErasureTasks.Clear();
        listOfDecisionTasksNonCritical.Clear();
        listOfDecisionTasksCritical.Clear();
    }

    /// <summary>
    /// Start Turn Early
    /// </summary>
    private void StartTurnEarly()
    {
        if (GameManager.instance.turnScript.Turn > 0)
        {
            //update lists for gear hacking effects
            UpdatePlayerHackingLists();
            //send data to AI Display UI elements
            UpdateTaskDisplayData();
            UpdateSideTabData();
            UpdateBottomTabData();
        }
    }
    
    //
    // - - - Global Flags - - -
    //

    public void SetAIOffline(bool status)
    {
        isOffline = status;
        Debug.LogFormat("[Aim] -> SetAIOffline: isOffLine {0}{1}", isOffline, "\n");
    }

    public void SetAITraceBack(bool status)
    {
        isTraceBack = status;
        Debug.LogFormat("[Aim] -> SetAITraceBack: isTraceBack {0}{1}", isTraceBack, "\n");
    }

    public void SetAIScreamer(bool status)
    {
        isScreamer = status;
        Debug.LogFormat("[Aim] -> SetAIScreamer: isScreamer {0}{1}", isScreamer, "\n");
    }

    public bool CheckAIOffLineStatus()
    { return isOffline; }

    public bool CheckAITraceBackStatus()
    { return isTraceBack; }

    public bool CheckAIScreamerStatus()
    { return isScreamer; }

    //
    // - - - Game Start Setup - - -
    //

    /// <summary>
    /// initialises list of most connected nodes (Game start)
    /// </summary>
    private void SetConnectedNodes()
    {
        int numOfConnections, numNodesHalf, counter, limit;
        //temp dictionary, key -> nodeID, value -> # of connections
        Dictionary<int, int> dictOfConnected = new Dictionary<int, int>();
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        List<Node> listOfMostConnectedNodes = new List<Node>();
        if (listOfNodes != null)
        {
            numNodesHalf = listOfNodes.Count / 2;
            //loop nodes and set up dictionary of nodes and their # of connections (Neighbours are used but same thing)
            foreach(Node node in listOfNodes)
            {
                if (node != null)
                {
                    numOfConnections = node.GetNumOfNeighbours();
                    //only select nodes that have 'x' number of connections
                    if (numOfConnections >= nodeConnectionThreshold)
                    {
                        try
                        { dictOfConnected.Add(node.nodeID, numOfConnections); }
                        catch (ArgumentException)
                        { Debug.LogWarningFormat("Invalid connection entry (duplicate) for nodeID {0}", node.nodeID); }
                    }

                }
                else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", node.nodeID); }
            }
         
            //check that there are at least 3 nodes in dict
            if (dictOfConnected.Count < 3)
            {
                //Not enough nodes -> loop again and add nodes with 1 - factor connections
                int numSpecific = nodeConnectionThreshold - 1;
                if (numSpecific > 0)
                {
                    foreach(Node node in listOfNodes)
                    {
                        if (node != null)
                        {
                            numOfConnections = node.GetNumOfNeighbours();
                            //only select nodes that have 'x' number of connections
                            if (numOfConnections == numSpecific)
                            {
                                try
                                { dictOfConnected.Add(node.nodeID, numOfConnections); }
                                catch (ArgumentException)
                                { Debug.LogWarningFormat("Invalid connection entry (duplicate) for nodeID {0}", node.nodeID); }
                            }

                        }
                        else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", node.nodeID); }
                    }
                    Debug.Log(string.Format("[Aim] -> SetConnectedNodes: Extra nodes ({0} connections) have been added to the listOfMostConnectedNodes{1}", 
                        numSpecific, "\n"));
                }
                else { Debug.LogWarning("Insufficient records for SetConnectedNodes"); }
            }
            //ensure that their is a maximum of total Nodes on Map / 2 in the list -> take the top half
            limit = 999;
            if (dictOfConnected.Count > numNodesHalf == true)
            { limit = numNodesHalf; }
            //Final check for a viable number of records
            if (dictOfConnected.Count > 0)
            {
                if (dictOfConnected.Count >= 3)
                {
                    //Sort by # of connections
                    var sorted = from record in dictOfConnected orderby record.Value descending select record.Key;
                    counter = 0;
                    //select all nodes with 'x' connections
                    foreach (var record in sorted)
                    {
                        Node nodeConnected = GameManager.instance.dataScript.GetNode(record);
                        if (nodeConnected != null)
                        {
                            listOfMostConnectedNodes.Add(nodeConnected);
                            //set node field
                            nodeConnected.isConnectedNode = true;
                            counter++;
                            //ensure that a max of half the total nodes on the map are in the list (top half)
                            if (counter == limit)
                            { break; }
                        }
                        else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", record)); }
                    }
                    //Pass data to the main reference list
                    GameManager.instance.dataScript.SetConnectedNodes(listOfMostConnectedNodes);
                    Debug.Log(string.Format("[Aim] -> SetConnectedNodes: {0} nodes have been added to the listOfMostConnectedNodes{1}", counter, "\n"));
                }
                else { Debug.LogWarning("Insufficient number of records ( < 3) for SetConnectedNodes"); }
            }
            else { Debug.LogError("Insufficient records (None) for SetConnectedNodes"); }
        }
        else { Debug.LogError("Invalid ListOfNodes (Null)"); }
    }

    /// <summary>
    /// loops nodes and sets '.isPreferredNode'flag (Game start)
    /// </summary>
    private void SetPreferredNodes()
    {
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        if (listOfNodes != null)
        {
            //isPreferred
            if (authorityPreferredArc != null)
            {
                foreach (Node node in listOfNodes)
                {
                    if (node.Arc.name.Equals(authorityPreferredArc) == true)
                    { node.isPreferredAuthority = true; }
                    else { node.isPreferredAuthority = false; }
                }
            }
            else { Debug.LogWarning("Invalid authorityPreferredArc (Null)"); }

            //Resistance preferred -> TO DO
            if (resistancePreferredArc != null)
            {
                foreach (Node node in listOfNodes)
                {
                    if (node.Arc.name.Equals(resistancePreferredArc) == true)
                    { node.isPreferredResistance = true; }
                    else { node.isPreferredResistance = false; }
                }
            }
            else { Debug.LogWarning("Invalid resistancePreferredArc (Null)"); }
        }
        else { Debug.LogError("Invalid listOfNodes (Null)"); }
    }

    /// <summary>
    /// loops nodes and sets '.isCentreNode' flag (Game start)
    /// </summary>
    private void SetCentreNodes()
    {
        //calculate limits of central area of map
        float centreNum = 10 * nodeGeographicCentre / 100;
        float limit = centreNum / 2f;
        float upper = limit;
        float lower = limit * -1;
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        if (listOfNodes != null)
        {
            foreach (Node node in listOfNodes)
            {
                //check each node to determine if it's inside or outside central area
                if (node.nodePosition.x <= upper && node.nodePosition.x >= lower)
                {
                    if (node.nodePosition.z <= upper && node.nodePosition.z >= lower)
                    { node.isCentreNode = true; }
                    else
                    { node.isCentreNode = false; }
                }
                else { node.isCentreNode = false; }
            }
        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
    }

    /// <summary>
    /// initialises DataManager.cs -> listOfDecisionNodes which is all nodes isCentred or isConnected with at least one no security connection that isn't a dead end
    /// </summary>
    public void SetDecisionNodes()
    {
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        List<Node> listOfDecisionNodes = new List<Node>();
        Node nodeFar;
        bool isSuccessful;
        if (listOfNodes != null)
        {
            foreach(Node node in listOfNodes)
            {
                isSuccessful = false;
                if (node != null)
                {
                    //centred and/or connected
                    if (node.isCentreNode == true || node.isConnectedNode == true)
                    {
                        //check connections
                        List<Connection> listOfConnections = node.GetListOfConnections();
                        if (listOfConnections != null)
                        {
                            //avoid dead end connections
                            if (listOfConnections.Count > 1)
                            {
                                foreach (Connection connection in listOfConnections)
                                {
                                    if (connection.SecurityLevel == ConnectionType.None)
                                    {
                                        nodeFar = connection.node1;
                                        //check that we've got the correct connection end
                                        if (nodeFar.nodeID == node.nodeID)
                                        { nodeFar = connection.node2; }
                                        //check that the far node has at least 2 connections (ignore single dead end connections)
                                        if (nodeFar.CheckNumOfConnections() > 1)
                                        {
                                            isSuccessful = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        { Debug.LogWarningFormat("Invalid listOfConnections (Null) for nodeID {0}", node.nodeID); }
                        //add to list
                        if (isSuccessful == true)
                        { listOfDecisionNodes.Add(node); }
                    }
                }
                else { Debug.LogWarning("Invalid node (Null) in listOfNodes"); }
            }
        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
        //initialise list (overwrites any existing data)
        Debug.LogFormat("[Aim] -> SetDecisionNodes: {0} nodes have been added to listOfDecisionNodes{1}", listOfDecisionNodes.Count, "\n");
        GameManager.instance.dataScript.SetDecisionNodes(listOfDecisionNodes);
    }

    /// <summary>
    /// For each node finds all nodes within 2 connections radius
    /// </summary>
    private void SetNearNeighbours()
    {
        List<Node> listOfNearNeighbours = new List<Node>();
        List<int> listLookup = new List<int>();
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        if (listOfNodes != null)
        {
            foreach(Node node in listOfNodes)
            {
                List<Node> listOfImmediateNeighbours = node.GetNeighbouringNodes();
                if (listOfImmediateNeighbours != null)
                {
                    //immediate neighbours
                    foreach(Node nodeimmediate in listOfImmediateNeighbours)
                    {
                        //use lookup list to check that node isn't already in system
                        if (listLookup.Exists(id => id == nodeimmediate.nodeID) == false)
                        {
                            //new node -> add to lookup and main lists
                            listLookup.Add(nodeimmediate.nodeID);
                            listOfNearNeighbours.Add(nodeimmediate);
                        }
                        //Neighbours of immediate neighbour (may not have any)
                        List<Node> listOfNeighbours = nodeimmediate.GetNeighbouringNodes();
                        if (listOfNeighbours != null)
                        {
                            if (listOfNeighbours.Count > 0)
                            {
                                foreach(Node nodeNear in listOfNeighbours)
                                {
                                    //use lookup list to check that node isn't already in system
                                    if (listLookup.Exists(id => id == nodeNear.nodeID) == false)
                                    {
                                        //new node -> add to lookup and main lists
                                        listLookup.Add(nodeNear.nodeID);
                                        listOfNearNeighbours.Add(nodeNear);
                                    }
                                }
                            }
                        }
                        else { Debug.LogErrorFormat("Invalid listOfNeighbours for nodeID {0}", nodeimmediate.nodeID); }
                    }
                }
                //Pass data to Node
                if (listOfNearNeighbours.Count > 0)
                { node.SetNearNeighbours(listOfNearNeighbours); }
                //clear lists ready for next pass
                listOfNearNeighbours.Clear();
                listLookup.Clear();
            }
        }
        else { Debug.LogWarning("Invalid listOfNodes (NUll)"); }
    }


    //
    // - - - Miscellaneous - - -
    //

    /// <summary>
    /// add the resource allowance each turn to the relevant pool
    /// </summary>
    /// <param name="side"></param>
    private void UpdateResources(GlobalSide side)
    {
        int resources = GameManager.instance.dataScript.CheckAIResourcePool(side);
        if (side.level == 1)
        { resources += resourcesGainAuthority; }
        else if (side.level == 2)
        { resources += resourcesGainResistance; }
        GameManager.instance.dataScript.SetAIResources(side, resources);
        Debug.LogFormat("[Aim]  -> UpdateResources: {0} resources {1}{2}", side.name, resources, "\n");
    }

    /// <summary>
    /// Extracts all relevant AI data from an AI related message
    /// </summary>
    /// <param name="message"></param>
    public void GetAIMessageData(Message message)
    {
        if (message != null)
        {
            if (message.type == MessageType.AI)
            {
                switch (message.subType)
                {
                    case MessageSubType.AI_Connection:
                        //Get Connection and add Activity data
                        Connection connection = GameManager.instance.dataScript.GetConnection(message.data1);
                        if (connection != null)
                        {
                            connection.AddActivityData(message.turnCreated);
                            //add to queue of most recent activity -> destination nodeID and turn created
                            GameManager.instance.dataScript.AddToRecentConnectionQueue(new AITracker(message.data0, message.turnCreated));

                            /*//add to Nemesis list -> don't double up on AI_Immediate messages -> NOTE: Not necessary as PlayerMove msg's don't double up anymore
                            if (message.turnCreated != GameManager.instance.turnScript.Turn)
                            { listOfPlayerActivity.Add(new AITracker(message.data0, message.turnCreated)); }*/

                            //Add to Nemesis List
                            listOfPlayerActivity.Add(new AITracker(message.data0, message.turnCreated));
                        }
                        else { Debug.LogWarning(string.Format("Invalid connection (Null) for connID {0} -> AI data NOT extracted", message.data1)); }
                        break;
                    case MessageSubType.AI_Node:
                    case MessageSubType.AI_Detected:
                        //Player Detected Hacking with Traceback ON -> Get Node and add Activity data
                        Node node = GameManager.instance.dataScript.GetNode(message.data0);
                        if (node != null)
                        {
                            node.AddActivityData(message.turnCreated);
                            //add to queue of most recent activity -> nodeID and turn created
                            GameManager.instance.dataScript.AddToRecentNodeQueue(new AITracker(message.data0, message.turnCreated));
                            //add to Nemesis list
                            listOfPlayerActivity.Add(new AITracker(message.data0, message.turnCreated));
                        }
                        else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0} -> AI data NOT extracted", message.data0)); }
                        break;
                    case MessageSubType.AI_Capture:
                    case MessageSubType.AI_Release:

                        //- - - TO DO - - - 

                        break;
                    case MessageSubType.AI_Immediate:
                        //immediate flag is set by EffectManager.cs -> ProcessEffect (Invisibility) prior to this
                        if (message.data1 > -1)
                        {
                            //player move, add to connections queue -> destination nodeID and turn created
                            if (message.data0 > -1)
                            {
                                GameManager.instance.dataScript.AddToRecentConnectionQueue(new AITracker(message.data0, message.turnCreated));
                                //add to Nemesis list
                                listOfPlayerActivity.Add(new AITracker(message.data0, message.turnCreated));
                            }
                            else { Debug.LogWarning("Invalid message.data0 (-1) for 'AI_Immediate' type message"); }
                        }
                        else
                        {
                            //node activity, add to node queue -> destination nodeID and turn created
                            if (message.data0 > -1)
                            { GameManager.instance.dataScript.AddToRecentNodeQueue(new AITracker(message.data0, message.turnCreated)); }
                            else { Debug.LogWarning("Invalid message.data0 (-1) for 'AI_Immediate' type message"); }
                        }
                        break;
                    case MessageSubType.AI_Reboot:
                    case MessageSubType.AI_Alert:
                    case MessageSubType.AI_Hacked:
                    case MessageSubType.AI_Countermeasure:
                        //not applicable
                        break;
                    default:
                        Debug.LogWarning(string.Format("Invalid message AI subType \"{0}\" for \"{1}\"", message.subType, message.text));
                        break;

                }
            }
            else { Debug.LogWarning(string.Format("Invalid (not AI) message type \"{0}\" for \"{1}\"", message.type, message.text)); }
        }
        else { Debug.LogWarning("Invalid message (Null)"); }
    }

    //
    // - - - Turn based AI - - -
    //

    /// <summary>
    /// gathers data on all nodes that have degraded in some from (from their starting values) and adds to listNodeMaster (from scratch each turn)
    /// also gathers data on known, but uncompleted, targets as well as Damaged targets that need to be contained
    /// </summary>
    private void GetAINodeData()
    {
        int data;
        AINodeData dataPackage;
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        if (listOfNodes != null)
        {
            foreach (Node node in listOfNodes)
            {
                if (node != null)
                {
                    //
                    // - - - Node datapoints - - -
                    //
                    //check that node isn't already maxxed out on teams
                    if (node.CheckNumOfTeams() < maxTeamsAtNode)
                    {
                        //Stability
                        data = node.GetNodeChange(NodeData.Stability);
                        if (data < 0)
                        {
                            //ignore if civil team already present
                            if (node.isStabilityTeam == false)
                            {
                                //node stability has degraded
                                dataPackage = new AINodeData();
                                dataPackage.nodeID = node.nodeID;
                                dataPackage.type = NodeData.Stability;
                                dataPackage.arc = node.Arc;
                                dataPackage.difference = Mathf.Abs(data);
                                dataPackage.current = node.Stability;
                                dataPackage.isPreferred = node.isPreferredAuthority;
                                listNodeMaster.Add(dataPackage);
                            }
                        }
                        //Security
                        data = node.GetNodeChange(NodeData.Security);
                        if (data < 0)
                        {
                            //ignore if control team already present
                            if (node.isSecurityTeam == false)
                            {
                                //node stability has degraded
                                dataPackage = new AINodeData();
                                dataPackage.nodeID = node.nodeID;
                                dataPackage.type = NodeData.Security;
                                dataPackage.arc = node.Arc;
                                dataPackage.difference = Mathf.Abs(data);
                                dataPackage.current = node.Security;
                                dataPackage.isPreferred = node.isPreferredAuthority;
                                listNodeMaster.Add(dataPackage);
                            }
                        }
                        //Support (positive value indicates a problem, eg. growing support for resistance)
                        data = node.GetNodeChange(NodeData.Support);
                        if (data > 0)
                        {
                            //ignore if media team already present
                            if (node.isSupportTeam == false)
                            {
                                //node stability has degraded
                                dataPackage = new AINodeData();
                                dataPackage.nodeID = node.nodeID;
                                dataPackage.type = NodeData.Support;
                                dataPackage.arc = node.Arc;
                                dataPackage.difference = data;
                                dataPackage.current = node.Support;
                                dataPackage.isPreferred = node.isPreferredAuthority;
                                listNodeMaster.Add(dataPackage);
                            }
                        }
                        //
                        // - - - Probe nodes - - -
                        //
                        if (node.isProbeTeam == false)
                        {
                            if (node.isTargetKnown == false)
                            {
                                //probe team suitable node data package
                                dataPackage = new AINodeData();
                                dataPackage.nodeID = node.nodeID;
                                dataPackage.type = NodeData.Probe;
                                dataPackage.arc = node.Arc;
                                dataPackage.isPreferred = node.isPreferredAuthority;
                                listOfProbeNodes.Add(dataPackage);
                            }
                        }
                    }
                    //
                    // - - - Targets (known and uncompleted) - - -
                    //
                    if (node.isTargetKnown == true)
                    {
                        Target target = GameManager.instance.dataScript.GetTarget(node.targetID);
                        if (target != null)
                        {
                            switch (target.targetStatus)
                            {
                                case Status.Active:
                                case Status.Live:
                                    //spider/erasure team node data package (good ambush situation as targets will lure in resistance player)
                                    dataPackage = new AINodeData();
                                    dataPackage.nodeID = node.nodeID;
                                    dataPackage.type = NodeData.Target;
                                    dataPackage.arc = node.Arc;
                                    dataPackage.isPreferred = node.isPreferredAuthority;
                                    listOfTargetsKnown.Add(dataPackage);
                                    break;
                                case Status.Outstanding:
                                    //Damage team node data package (only for completed targets with ongoing effects that require containing)
                                    if (node.CheckForOngoingEffects() == true)
                                    {
                                        dataPackage = new AINodeData();
                                        dataPackage.nodeID = node.nodeID;
                                        dataPackage.type = NodeData.Target;
                                        dataPackage.arc = node.Arc;
                                        dataPackage.isPreferred = node.isPreferredAuthority;
                                        listOfTargetsDamaged.Add(dataPackage);
                                    }
                                    break;
                            }
                        }
                        //Explanation -> if target is Known and null then it's a successfully completed target with no ongoing effects. No need for a warning.
                        /*else { Debug.LogWarning(string.Format("Invalid target (Null) for targetID {0}", node.Value.targetID)); }*/
                    }
                }
                else { Debug.LogWarning(string.Format("Invalid node (Null) in listOfNodes for nodeID {0}", node.nodeID)); }
            }
        }
        else { Debug.LogError("Invalid listOfNodes (Null)"); }
    }

    /// <summary>
    /// Processes raw node data into useable data
    /// </summary>
    private void ProcessNodeData()
    {
        //loop master list and populate sub lists
        if (listNodeMaster.Count > 0)
        {
            foreach (AINodeData data in listNodeMaster)
            {
                //critical is when datapoint has reached max bad condition
                switch(data.type)
                {
                    case NodeData.Stability:
                        if (data.current == 0)
                        { listStabilityCritical.Add(data); }
                        else { listStabilityNonCritical.Add(data); }
                        break;
                    case NodeData.Security:
                        if (data.current == 0)
                        { listSecurityCritical.Add(data); }
                        else { listSecurityNonCritical.Add(data); }
                        break;
                    case NodeData.Support:
                        if (data.current == 3)
                        { listSupportCritical.Add(data); }
                        else { listSupportNonCritical.Add(data); }
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid AINodeData.type \"{0}\"", data.type));
                        break;
                }
            }
        }
    }

    /// <summary>
    /// processes raw most connected node data by giving all a score (higher the score, more likely node is to be selected)
    /// </summary>
    private void ProcessSpiderData()
    {
        //only bother proceeding if there are spider teams available to deploy
        if (GameManager.instance.dataScript.CheckTeamInfo(teamArcSpider, TeamInfo.Reserve) > 0)
        {
            List<Node> listOfMostConnected = GameManager.instance.dataScript.GetListOfMostConnectedNodes();
            if (listOfMostConnected != null)
            {
                int score, tally;
                int currentTurn = GameManager.instance.turnScript.Turn;
                foreach (Node node in listOfMostConnected)
                {
                    //ignore nodes with existing spider teams & spiders
                    if (node.isSpiderTeam == false && node.isSpider == false)
                    {
                        //check that node isn't already maxxed out on teams
                        if (node.CheckNumOfTeams() < maxTeamsAtNode)
                        {
                            AINodeData dataPackage = new AINodeData();
                            dataPackage.nodeID = node.nodeID;
                            dataPackage.type = NodeData.Spider;
                            dataPackage.arc = node.Arc;
                            //calculate score 
                            score = 0;
                            //preferred type
                            if (node.isPreferredAuthority == true)
                            { score += nodePreferredFactor; }
                            //central region
                            if (node.isCentreNode == true)
                            { score += nodeCentreFactor; }
                            //resistance activity at node
                            if (node.activityCount > 0)
                            { score += node.activityCount * nodeActivityCountFactor; }
                            //resistance activity time, eg. how recent? (min capped at 0)
                            if (node.activityTime > -1)
                            {
                                tally = nodeActivityTimeFactor - (currentTurn - node.activityTime);
                                if (tally > 0)
                                { score += tally; }
                            }
                            //connections
                            List<Connection> listOfConnections = node.GetListOfConnections();
                            if (listOfConnections != null)
                            {
                                foreach (Connection conn in listOfConnections)
                                {
                                    switch (conn.SecurityLevel)
                                    {
                                        case ConnectionType.HIGH:
                                            score += securityHighFactor;
                                            break;
                                        case ConnectionType.MEDIUM:
                                            score += securityMediumFactor;
                                            break;
                                        case ConnectionType.LOW:
                                            score += securityLowFactor;
                                            break;
                                        case ConnectionType.None:
                                            score += securityNoneFactor;
                                            break;
                                        default:
                                            Debug.LogWarning(string.Format("Invalid connecion Security Level \"{0}\"", conn.SecurityLevel));
                                            break;
                                    }
                                }
                            }
                            else { Debug.LogWarning(string.Format("Invalid listOfConnections (Null) for nodeID {0}", node.nodeID)); }
                            dataPackage.score = score;
                            //add data to list
                            listOfSpiderNodes.Add(dataPackage);
                        }
                    }
                }
                Debug.LogFormat("[Aim]  -> ProcessSpiderData: {0} possible nodes added{1}", listOfSpiderNodes.Count, "\n");
            }
            else { Debug.LogError("Invalid listOfMostConnected (Null)"); }
        }
        else { Debug.Log(string.Format("[Aim]  -> ProcessSpiderData: No Spider teams available in reserves{0}", "\n")); }
    }

    /// <summary>
    /// Determines the target node to be used as a focal point for Erasure AI calculations. Returns -1 if none found. Called by ProcessErasureData
    /// </summary>
    private int ProcessErasureTarget()
    {
        int nodeReturnID = -1;
        Queue<AITracker> queueRecentConnections = GameManager.instance.dataScript.GetRecentConnectionsQueue();
        Queue<AITracker> queueRecentNodes = GameManager.instance.dataScript.GetRecentNodesQueue();
        if (queueRecentConnections != null && queueRecentNodes != null)
        {
            int currentTurn = GameManager.instance.turnScript.Turn;
            int connID = -1;
            int turnConn = 0;
            //if there isn't any confirmed player activity within the specified time frame then there is no target node
            if (queueRecentConnections.Count > 0)
            {
                //examine connection activity queue and find the most recent
                foreach (AITracker data in queueRecentConnections)
                {
                    if (data.turn > turnConn)
                    {
                        turnConn = data.turn;
                        connID = data.data0;
                    }
                }
                //if was within previous 2 turns then this is the target node (definite player activity)
                if (currentTurn - turnConn <= trackerNumOfTurnsAgoErasure)
                {
                    nodeReturnID = connID;
                    if (connID > -1)
                    { listOfErasureAILog.Add(string.Format("Target: Recent Connection nodeID {0}, turn {1}", nodeReturnID, turnConn)); }
                }
                else
                {
                    int nodeID = -1;
                    int turnNode = -1;
                    //check for most recent turn activity (could be player or actor)
                    foreach (AITracker data in queueRecentNodes)
                    {
                        if (data.turn > turnNode)
                        {
                            turnNode = data.turn;
                            nodeID = data.data0;
                        }
                    }
                    if (currentTurn - turnNode <= trackerNumOfTurnsAgoErasure)
                    {
                        listOfErasureAILog.Add(string.Format("Target: Recent Node nodeID {0}, turn {1}", nodeID, turnNode));
                        //check if matches any recent connection activity
                        foreach (AITracker data in queueRecentConnections)
                        {
                            if (data.data0 == nodeID)
                            {
                                //a match -> Player was confirmed as being at this node
                                nodeReturnID = nodeID;
                                listOfErasureAILog.Add(string.Format("Target: Confirmed Conn Activity for ID {0}, turn {1}", nodeID, turnNode));
                                break;
                            }
                        }
                    }
                }
            }
            if (nodeReturnID > 0)
            { listOfErasureAILog.Add(string.Format("Target: nodeReturnID {0}", nodeReturnID)); }
            else { listOfErasureAILog.Add("No viable target node found"); }
            Debug.LogFormat("[Aim]  -> ProcessErasureTarget: queueRecentConnected {0} records, queueRecentNodes {1} records, target nodeID {2}{3}", queueRecentConnections.Count,  
                queueRecentNodes.Count, nodeReturnID, "\n");
        }
        else { Debug.LogWarning("Invalid queue (Null)"); }
        return nodeReturnID;
    }

    /// <summary>
    /// Determines the AITracker package (nodeID & turn) to be used as a focal point for Nemesis AI calculations. Returns null if none found. Called by AISideAuthority
    /// </summary>
    private AITracker ProcessNemesisTarget()
    {
        AITracker targetTracker = null;
        int turnNum = -1;
        int numOfRecords = listOfPlayerActivity.Count;
        if (numOfRecords > 0)
        {
            for (int i = 0; i < numOfRecords; i++)
            {
                AITracker tracker = listOfPlayerActivity[i];
                if (tracker != null)
                {
                    /*Debug.LogFormat("[Tst] AIManager.cs -> ProcessNemesisTarget: player Activity, turnNum {1}, nodeID {2}{3}", numOfRecords, tracker.turn, tracker.data0, "\n");*/

                    //examine connection activity queue and find the most recent
                    if (tracker.turn > turnNum)
                    {
                        turnNum = tracker.turn;
                        targetTracker = tracker;
                    }
                }
                else { Debug.LogWarningFormat("Invalid tracker (Null) for listOfPlayerActivity[{0}]", i); }
            }
        }
        //clear list ready for next turn
        listOfPlayerActivity.Clear();
        return targetTracker;
    }

    /// <summary>
    /// Processes the potential nodes based on a specified target node -> Note: there may be no target node as nothing notable has happened
    /// </summary>
    private void ProcessErasureData()
    {
        int score, tally;
        int numOfTeams = GameManager.instance.dataScript.CheckTeamInfo(teamArcErasure, TeamInfo.Reserve);
        int currentTurn = GameManager.instance.turnScript.Turn;
        //only bother proceeding if there are spider teams available to deploy
        if (numOfTeams > 0)
        {
            //get target node
            playerTargetNodeID = ProcessErasureTarget();
            if (playerTargetNodeID > -1)
            {
                //get near neighbours as potential node targets
                Node node = GameManager.instance.dataScript.GetNode(playerTargetNodeID);
                if (node != null)
                {
                    List<Node> listOfNearNeighbours = node.GetNearNeighbours();
                    if (listOfNearNeighbours != null)
                    {
                        foreach (Node nodeNear in listOfNearNeighbours)
                        {
                            if (nodeNear.CheckNumOfTeams() < maxTeamsAtNode)
                            {
                                //Erasure team not already present
                                if (nodeNear.isErasureTeam == false)
                                {
                                    score = 0;
                                    //activity count
                                    if (nodeNear.activityCount > -1)
                                    { score += nodeNear.activityCount * nodeActivityCountFactor; }
                                    //activity time (min capped at 0)
                                    if (nodeNear.activityTime > -1)
                                    {
                                        tally = nodeActivityTimeFactor - (currentTurn - nodeNear.activityTime);
                                        if (tally > 0)
                                        { score += tally; }
                                    }
                                    //spider (not known)
                                    if (nodeNear.isSpider == true)
                                    {
                                        //full effect if spider not known, halved otherwise
                                        if (nodeNear.isSpiderKnown == false)
                                        { score += nodeSpiderFactor; }
                                        else { score += nodeSpiderFactor / 2; }
                                    }
                                    //in list of most connected
                                    if (nodeNear.isConnectedNode == true)
                                    { score += nodeConnectedFactor; }
                                    //chokepoint node
                                    if (nodeNear.isChokepointNode == true)
                                    { score += nodeChokepointFactor; }
                                    //preferred target
                                    if (nodeNear.isPreferredAuthority == true)
                                    { score += nodePreferredFactor; }
                                    //target
                                    if (nodeNear.targetID > -1)
                                    {
                                        Target target = GameManager.instance.dataScript.GetTarget(nodeNear.targetID);
                                        {
                                            //positive effect if target known by AI and uncompleted, reversed negative effect otherwise
                                            if (target != null)
                                            {
                                                switch (target.targetStatus)
                                                {
                                                    case Status.Active:
                                                    case Status.Live:
                                                        if (target.isKnownByAI)
                                                        { score += nodeTargetFactor; }
                                                        break;
                                                    case Status.Outstanding:
                                                        score -= nodeTargetFactor;
                                                        break;
                                                }
                                            }
                                            else { Debug.LogWarningFormat("Invalid target (Null) for targetID {0}", nodeNear.targetID); }
                                        }
                                    }
                                    //add to list of possible target nodes
                                    AINodeData data = new AINodeData()
                                    {
                                        nodeID = nodeNear.nodeID,
                                        arc = nodeNear.Arc,
                                        type = NodeData.Erasure,
                                        isPreferred = nodeNear.isPreferredAuthority,
                                        score = score
                                    };
                                    listOfErasureNodes.Add(data);
                                }
                            }
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid listOfNearNeighbours for nodeID {0}", node.nodeID); }
                }
                else { Debug.LogWarningFormat("Invalid target node (Null) for nodeID {0}", playerTargetNodeID); }
            }
            else
            {
                Debug.LogFormat("[Aim]  -> ProcessErasureData: No target node identified, checking other possibilities{0}", "\n");

                //if a minimum number of erasure teams in reserve then place one at a likely place
                if (numOfTeams > 1)
                {
                    List<Node> listOfMostConnected = GameManager.instance.dataScript.GetListOfMostConnectedNodes();
                    if (listOfMostConnected != null)
                    {
                        foreach (Node nodeConnected in listOfMostConnected)
                        {
                            //most connected -> Unknown Spider (incorporates activity time and count)
                            if (nodeConnected.isSpider == true && nodeConnected.isSpiderKnown == false)
                            {
                                //add to list of possible target nodes -> all are scored equally (spiderAI ensures high scoring node)
                                AINodeData data = new AINodeData()
                                {
                                    nodeID = nodeConnected.nodeID,
                                    arc = nodeConnected.Arc,
                                    type = NodeData.Erasure,
                                    isPreferred = nodeConnected.isPreferredAuthority,
                                    score = 0
                                };
                                listOfErasureNodes.Add(data);
                            }
                        }
                        Debug.LogFormat("[Aim]  -> ProcessErasureData: {0} possible nodes added (Connected and Unknown Spiders{1}", listOfErasureNodes.Count, "\n");
                    }
                    else { Debug.LogWarning("Invalid lisOfMostConnected (Null)"); }
                }
            }
        }
        else { Debug.LogFormat("[Aim]  -> ProcessSpiderData: No Erasure teams available in reserves{0}", "\n"); }
    }

    /// <summary>
    /// Processes all data relevant for deciding on Decision tasks
    /// </summary>
    private void ProcessDecisionData()
    {
        float tally;
        //zero data points
        connSecRatio = 0;
        teamRatio = 0;
        erasureTeamsOnMap = 0;
        //work out connection security ratio (cumulate tally of connection security levels / number of connections)
        List<Connection> listOfConnections = GameManager.instance.dataScript.GetListOfConnections();
        if (listOfConnections != null)
        {
            tally = 0;
            foreach(Connection conn in listOfConnections)
            {
                switch(conn.SecurityLevel)
                {
                    case ConnectionType.HIGH:
                        tally += 3f;
                        break;
                    case ConnectionType.MEDIUM:
                        tally += 2f;
                        break;
                    case ConnectionType.LOW:
                        tally += 1f;
                        break;
                }
            }
            connSecRatio = tally / (float)totalConnections;
        }
        else { Debug.LogWarning("Invalid listOfConnections (Null)"); }
        //work out team ratio (total teams / total nodes)
        teamRatio = GameManager.instance.dataScript.CheckNumOfTeams() / (float)totalNodes;
        //number of erasure teams onMap
        erasureTeamsOnMap = GameManager.instance.dataScript.CheckTeamInfo(teamArcErasure, TeamInfo.OnMap);
        //log output
        Debug.LogFormat("[Aim] -> ProcessDecisionData: connection Security Ratio {0:f1} {1}{2}", connSecRatio, 
            connSecRatio >= connectionRatioThreshold ? "THRESHOLD EXCEEDED" : "", "\n");
        Debug.LogFormat("[Aim] -> ProcessDecisionData: teamRatio {0:f1} {1}{2}", teamRatio, teamRatio >= teamRatioThreshold ? "THRESHOLD EXCEEDED" : "", "\n");
        Debug.LogFormat("[Aim] -> ProcessDecisionData: number of Erasure teams onMap {0}", erasureTeamsOnMap);
        Debug.LogFormat("[Aim] -> ProcessDecisionData: immediateFlagResistance -> {0}{1}", immediateFlagResistance, "\n");
        if (erasureTeamsOnMap > 0 && immediateFlagResistance == true)
        { Debug.LogFormat("[Aim] -> ProcessDecisionData: SECURITY MEASURES Available{0}", "\n"); }
    }

    /// <summary>
    /// master method that determines up to 3 separate tasks, one for each node datapoint and the relevant team (Control/Civil/Media)
    /// </summary>
    private void ProcessNodeTasks()
    {
        int numOfTeams;
        //Stability
        numOfTeams = GameManager.instance.dataScript.CheckTeamInfo(teamArcCivil, TeamInfo.Reserve);
        if (numOfTeams > 0)
        {
            AITask taskStability = SelectNodeTask(listStabilityCritical, listStabilityNonCritical, "CIVIL", teamArcCivil);
            if (taskStability != null) { listOfTasksPotential.Add(taskStability); }
            else { Debug.Log(string.Format("[Aim]  -> ProcessNodeTasks: No available Stability Node tasks{0}", "\n")); }
        }
        else { Debug.Log(string.Format("[Aim]  -> ProcessNodeTasks: No Civil teams available in reserves{0}", "\n")); }
        //Security
        numOfTeams = GameManager.instance.dataScript.CheckTeamInfo(teamArcControl, TeamInfo.Reserve);
        if (numOfTeams > 0)
        {
            AITask taskSecurity = SelectNodeTask(listSecurityCritical, listSecurityNonCritical, "CONTROL", teamArcControl);
            if (taskSecurity != null) { listOfTasksPotential.Add(taskSecurity); }
            else { Debug.Log(string.Format("[Aim]  -> ProcessNodeTasks: No available Security Node tasks{0}", "\n")); }
        }
        else { Debug.Log(string.Format("[Aim]  -> ProcessNodeTasks: No Security teams available in reserves{0}", "\n")); }
        //Support
        numOfTeams = GameManager.instance.dataScript.CheckTeamInfo(teamArcMedia, TeamInfo.Reserve);
        if (numOfTeams > 0)
        {
            AITask taskSupport = SelectNodeTask(listSupportCritical, listSupportNonCritical, "MEDIA", teamArcMedia);
            if (taskSupport != null) { listOfTasksPotential.Add(taskSupport); }
            else { Debug.Log(string.Format("[Aim]  -> ProcessNodeTasks: No available Support Node tasks{0}", "\n")); }
        }
        else { Debug.Log(string.Format("[Aim]  -> ProcessNodeTasks: No Media teams available in reserves{0}", "\n")); }

    }

    /// <summary>
    /// selects a probe team task if one is available
    /// </summary>
    private void ProcessProbeTask()
    {
        if (GameManager.instance.dataScript.CheckTeamInfo(teamArcProbe, TeamInfo.Reserve) > 0)
        {
            int numOfRecords = listOfProbeNodes.Count;
            if (numOfRecords > 0)
            {
                AINodeData data = listOfProbeNodes[Random.Range(0, numOfRecords)];
                if (data != null)
                {
                    //create a task
                    AITask taskProbe = new AITask()
                    {
                        data0 = data.nodeID,
                        data1 = teamArcProbe,
                        name0 = data.arc.name,
                        name1 = "PROBE",
                        type = AIType.Team,
                        priority = Priority.Low
                    };
                    //add to list of potentials
                    listOfTasksPotential.Add(taskProbe);
                }
                else { Debug.LogWarning("Invalid record from listOfProbeNodes (Null)"); }
            }
            else { Debug.Log("No available nodes for a Probe Team"); }
        }
        else { Debug.Log(string.Format("[Aim]  -> ProcessProbeTasks: No Probe teams available in reserves{0}", "\n")); }
    }

    /// <summary>
    /// selects a spider team if a one is available
    /// </summary>
    private void ProcessSpiderTask()
    {
        //only bother proceeding if there are spider teams available to deploy
        if (GameManager.instance.dataScript.CheckTeamInfo(teamArcSpider, TeamInfo.Reserve) > 0)
        {
            if (listOfSpiderNodes.Count > 0)
            {
                int counter = 0;
                //sort Node list by score (highest at top)
                var sorted = from record in listOfSpiderNodes orderby record.score descending select record;
                //take top three records and create a task
                foreach (var record in sorted)
                {
                    if (record != null)
                    {
                        counter++;
                        AITask task = new AITask()
                        {
                            data0 = record.nodeID,
                            data1 = teamArcSpider,
                            name0 = record.arc.name,
                            name1 = "SPIDER",
                            type = AIType.Team,
                        };
                        switch (counter)
                        {
                            case 1:
                                //highest ranked score -> add three entries to pool
                                task.priority = Priority.High;
                                for (int i = 0; i < teamPoolFirstFactor; i++)
                                { listOfSpiderTasks.Add(task); }
                                break;
                            case 2:
                                //second highest ranked score -> add two entries to pool
                                task.priority = Priority.Medium;
                                for (int i = 0; i < teamPoolSecondFactor; i++)
                                { listOfSpiderTasks.Add(task); }
                                break;
                            case 3:
                                //third highest ranked score -> add one entry to pool
                                task.priority = Priority.Low;
                                for (int i = 0; i < teamPoolThirdFactor; i++)
                                { listOfSpiderTasks.Add(task); }
                                break;
                        }
                        if (counter == 3) { break; }
                    }
                    else { Debug.LogWarning("Invalid record (Null) in listOfSpiderNodes"); }
                }
            }
            //Add Known, but uncompleted, targets to the pool
            if (listOfTargetsKnown.Count > 0)
            {
                foreach (AINodeData record in listOfTargetsKnown)
                {
                    if (record != null)
                    {
                        AITask task = new AITask()
                        {
                            data0 = record.nodeID,
                            data1 = teamArcSpider,
                            name0 = record.arc.name,
                            name1 = "SPIDER",
                            type = AIType.Team,
                            priority = Priority.High
                        };
                        //Preferred node type -> Priority Critical and one extra task added to pool
                        if (record.isPreferred == true)
                        {
                            listOfSpiderTasks.Add(task);
                            task.priority = Priority.Critical;
                        }
                        //add target task to pool
                        for (int i = 0; i < teamPoolTargetFactor; i++)
                        { listOfSpiderTasks.Add(task); }
                    }
                    else { Debug.LogWarning("Invalid record (Null) in listOfTargetsKnown"); }
                }
            }
            //Select random task from pool
            if (listOfSpiderTasks.Count > 0)
            {
                AITask taskSpider = new AITask();
                taskSpider = listOfSpiderTasks[Random.Range(0, listOfSpiderTasks.Count)];
                //add to list of potentials
                listOfTasksPotential.Add(taskSpider);
            }
            else { Debug.Log(string.Format("[Aim]  -> ProcessSpiderTask: No available Spider Team tasks{0}", "\n")); }
        }
    }


    /// <summary>
    /// Determines the Erasure task 
    /// </summary>
    private void ProcessErasureTask()
    {
        //only bother proceeding if there are erasure teams available to deploy
        if (GameManager.instance.dataScript.CheckTeamInfo(teamArcErasure, TeamInfo.Reserve) > 0)
        {
            if (listOfErasureNodes.Count > 0)
            {
                int counter = 0;
                //sort Node list by score (highest at top)
                var sorted = from record in listOfErasureNodes orderby record.score descending select record;
                //take top three records and create a task
                foreach (var record in sorted)
                {
                    if (record != null)
                    {
                        counter++;
                        AITask task = new AITask()
                        {
                            data0 = record.nodeID,
                            data1 = teamArcErasure,
                            name0 = record.arc.name,
                            name1 = "ERASURE",
                            type = AIType.Team,
                            priority = Priority.Critical
                        };
                        switch (counter)
                        {
                            case 1:
                                //highest ranked score -> add three entries to pool
                                for (int i = 0; i < teamPoolFirstFactor; i++)
                                { listOfErasureTasks.Add(task); }
                                break;
                            case 2:
                                //second highest ranked score -> add two entries to pool
                                for (int i = 0; i < teamPoolSecondFactor; i++)
                                { listOfErasureTasks.Add(task); }
                                break;
                            case 3:
                                //third highest ranked score -> add one entry to pool
                                for (int i = 0; i < teamPoolThirdFactor; i++)
                                { listOfErasureTasks.Add(task); }
                                break;
                        }
                        if (counter == 3) { break; }
                    }
                    else { Debug.LogWarning("Invalid record (Null) in listOfErasureNodes"); }
                }
                //Select random task from pool
                if (listOfErasureTasks.Count > 0)
                {
                    AITask taskErasure = new AITask();
                    taskErasure = listOfErasureTasks[Random.Range(0, listOfErasureTasks.Count)];
                    //add to list of potentials
                    listOfTasksPotential.Add(taskErasure);
                }
                else { Debug.Log(string.Format("[Aim]  -> ProcessErasureTask: No available Erasure Team tasks{0}", "\n")); }
            }
        }
    }

    /// <summary>
    /// selects a damage team if one available
    /// </summary>
    private void ProcessDamageTask()
    {
        //Damage teams available to deploy?
        if (GameManager.instance.dataScript.CheckTeamInfo(teamArcDamage, TeamInfo.Reserve) > 0)
        {
            if (listOfTargetsDamaged.Count > 0)
            {
                foreach(AINodeData data in listOfTargetsDamaged)
                {
                    if (data != null)
                    {
                        Node node = GameManager.instance.dataScript.GetNode(data.nodeID);
                        if (node != null)
                        {
                            //room for another team?
                            if (node.CheckNumOfTeams() < maxTeamsAtNode)
                            {
                                //damage team not already present
                                if (node.isDamageTeam == false)
                                {
                                    AITask taskDamage = new AITask()
                                    {
                                        data0 = data.nodeID,
                                        data1 = teamArcDamage,
                                        name0 = data.arc.name,
                                        name1 = "DAMAGE",
                                        type = AIType.Team,
                                        priority = Priority.High
                                    };
                                    //preferred node type -> Critical
                                    if (data.isPreferred == true)
                                    { taskDamage.priority = Priority.Critical; }
                                    //add to list of potentials
                                    listOfTasksPotential.Add(taskDamage);
                                }
                            }
                        }
                        else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", data.nodeID)); }
                    }
                    else { Debug.LogWarning("Invalid record (Null) in listOfTargetsDamaged"); }
                }
            }
            else { Debug.Log(string.Format("[Aim]  -> ProcessDamageTask: No available Damaged Targets{0}", "\n")); }
        }
        else { Debug.Log(string.Format("[Aim]  -> ProcessDamageTask: No Damage teams available in reserves{0}", "\n")); }
    }

    /// <summary>
    /// selects a decision if situation warrants it
    /// Goes through each decision and adds 'x' entries (depending on priority) of each before randomly choosing one to add to the listOfTaskPotential
    /// </summary>
    private void ProcessDecisionTask()
    {
        Debug.Assert(listOfDecisionTasksNonCritical != null, "Invalid listOfDecisionTasksNonCritical (Null)");
        Debug.Assert(listOfDecisionTasksCritical != null, "Invalid listOfDecisionTasksCritical (Null)");
        //generate a security decision, choose which one (random choice but exclude ones where the cost can't be covered by the resource pool)
        int resources = GameManager.instance.dataScript.CheckAIResourcePool(globalAuthority);
        int cityLoyalty = GameManager.instance.cityScript.CityLoyalty;
        //
        // - - - Security -> Critical priority - - -
        //
        if (erasureTeamsOnMap > 0 && immediateFlagResistance == true)
        {
            //APB
            if (resources >= decisionAPB.cost)
            {
                AITask taskAPB = new AITask()
                {
                    data1 = decisionAPB.cost,
                    data2 = decisionAPB.aiDecID,
                    name0 = decisionAPB.name,
                    type = AIType.Decision,
                    priority = Priority.Critical
                };
                listOfDecisionTasksCritical.Add(taskAPB);
            }
            //Security Alert
            if (resources >= decisionSecAlert.cost)
            {
                AITask taskSecAlert = new AITask()
                {
                    data1 = decisionSecAlert.cost,
                    data2 = decisionSecAlert.aiDecID,
                    name0 = decisionSecAlert.name,
                    type = AIType.Decision,
                    priority = Priority.Critical
                };
                listOfDecisionTasksCritical.Add(taskSecAlert);
            }
            //Surveillance Crackdown
            if (resources >= decisionCrackdown.cost)
            {
                AITask taskCrackdown = new AITask()
                {
                    data1 = decisionCrackdown.cost,
                    data2 = decisionCrackdown.aiDecID,
                    name0 = decisionCrackdown.name,
                    type = AIType.Decision,
                    priority = Priority.Critical
                };
                listOfDecisionTasksCritical.Add(taskCrackdown);
            }
        }
        //Connections -> Medium priority
        if (connSecRatio < connectionRatioThreshold )
        {
            int connID = ProcessConnectionSelection();
            if (connID > -1)
            {
                AITask taskConnSec = new AITask()
                {
                    data0 = connID,
                    data1 = decisionConnSec.cost,
                    data2 = decisionConnSec.aiDecID,
                    name0 = decisionConnSec.name,
                    type = AIType.Decision,
                    priority = Priority.Medium
                };
                for (int i = 0; i < priorityMediumWeight; i++)
                { listOfDecisionTasksNonCritical.Add(taskConnSec); }
            }
            else { Debug.LogWarning("Invalid connID (-1). Connection Decision Deleted. No valid connection found"); }
        }
        //
        // - - - Logistics - - -
        //
        //Team request -> medium priority
        if (teamRatio < teamRatioThreshold)
        {
            AITask taskTeam = new AITask()
            {
                data1 = decisionRequestTeam.cost,
                data2 = decisionRequestTeam.aiDecID,
                name0 = decisionRequestTeam.name,
                type = AIType.Decision,
                priority = Priority.Medium
            };
            for (int i = 0; i < priorityMediumWeight; i++)
            { listOfDecisionTasksNonCritical.Add(taskTeam); }
        }
        //Resource request -> once made can be Approved or Denied by higher authority. NOTE: make sure it is '<=' to avoid getting stuck in dead end with '<'
        if (resources <= lowResourcesThreshold || isInsufficientResources == true)
        {
            AITask taskResources = new AITask()
            {
                data1 = decisionResources.cost,
                data2 = decisionResources.aiDecID,
                name0 = decisionResources.name,
                type = AIType.Decision,
                priority = Priority.Critical
            };
            listOfDecisionTasksCritical.Add(taskResources);
        }
        isInsufficientResources = false;
        //
        // - - - Policy - - -
        //
        if (isPolicy == false)
        {
            //there is no policy currently in play (one at a time allowed) -> Check City Loyalty
            
            if (cityLoyalty >= policyCrisisLoyaltyCriteria)
            {
                //LOW IMPACT
                if (numOfCrisis >= policyCrisisCriteriaLow)
                {
                    //Only one policy is (randomly) selected per impact category per turn
                    if (Random.Range(0, 100) < 50)
                    {
                        //Censorship
                        if (resources >= decisionCensorship.cost)
                        {
                            AITask taskPolicy = new AITask()
                            {
                                data1 = decisionCensorship.cost,
                                data2 = decisionCensorship.aiDecID,
                                name0 = decisionCensorship.name,
                                type = AIType.Decision,
                                priority = Priority.Low
                            };
                            listOfDecisionTasksNonCritical.Add(taskPolicy);
                        }
                    }
                    else
                    {
                        //Ban Protests
                        if (resources >= decisionBanProtests.cost)
                        {
                            AITask taskPolicy = new AITask()
                            {
                                data1 = decisionBanProtests.cost,
                                data2 = decisionBanProtests.aiDecID,
                                name0 = decisionBanProtests.name,
                                type = AIType.Decision,
                                priority = Priority.Low
                            };
                            listOfDecisionTasksNonCritical.Add(taskPolicy);
                        }
                    }
                }
                //MEDIUM IMPACT
                if (numOfCrisis >= policyCrisisCriteriaMed)
                {
                    //Only one policy is (randomly) selected per impact category per turn
                    if (Random.Range(0, 100) < 50)
                    {
                        //Censorship
                        if (resources >= decisionCurfew.cost)
                        {
                            AITask taskPolicy = new AITask()
                            {
                                data1 = decisionCurfew.cost,
                                data2 = decisionCurfew.aiDecID,
                                name0 = decisionCurfew.name,
                                type = AIType.Decision,
                                priority = Priority.Medium
                            };
                            listOfDecisionTasksNonCritical.Add(taskPolicy); listOfDecisionTasksNonCritical.Add(taskPolicy);
                        }
                    }
                    else
                    { 
                        //Robo Cops
                        if (resources >= decisionRoboCop.cost)
                        {
                            AITask taskPolicy = new AITask()
                            {
                                data1 = decisionRoboCop.cost,
                                data2 = decisionRoboCop.aiDecID,
                                name0 = decisionRoboCop.name,
                                type = AIType.Decision,
                                priority = Priority.Medium
                            };
                            listOfDecisionTasksNonCritical.Add(taskPolicy); listOfDecisionTasksNonCritical.Add(taskPolicy);
                        }
                    }
                }
                //HIGH IMPACT
                if (numOfCrisis >= policyCrisisCriteriaHigh)
                {
                    //Only one policy is (randomly) selected per impact category per turn
                    if (Random.Range(0, 100) < 50)
                    {
                        //Martial Law
                        if (resources >= decisionMartialLaw.cost)
                        {
                            AITask taskPolicy = new AITask()
                            {
                                data1 = decisionMartialLaw.cost,
                                data2 = decisionMartialLaw.aiDecID,
                                name0 = decisionMartialLaw.name,
                                type = AIType.Decision,
                                priority = Priority.High
                            };
                            listOfDecisionTasksNonCritical.Add(taskPolicy); listOfDecisionTasksNonCritical.Add(taskPolicy); listOfDecisionTasksNonCritical.Add(taskPolicy);
                        }
                    }
                    else
                    {
                        //Drone Warfare
                        if (resources >= decisionDrones.cost)
                        {
                            AITask taskPolicy = new AITask()
                            {
                                data1 = decisionDrones.cost,
                                data2 = decisionDrones.aiDecID,
                                name0 = decisionDrones.name,
                                type = AIType.Decision,
                                priority = Priority.High
                            };
                            listOfDecisionTasksNonCritical.Add(taskPolicy); listOfDecisionTasksNonCritical.Add(taskPolicy); listOfDecisionTasksNonCritical.Add(taskPolicy);
                        }
                    }
                }
            }
        }
        //
        // - - - Handouts - - -
        //
        if (timerHandout == 0)
        {
            //LOW IMPACT
            if (cityLoyalty <= handoutLoyaltyCriteriaLow)
            {
                //randomly choose one low impact handout
                if (Random.Range(0, 100) < 50)
                {
                    //Christmas Hampers
                    if (resources >= decisionHamper.cost)
                    {
                        AITask taskHandout = new AITask()
                        {
                            data1 = decisionHamper.cost,
                            data2 = decisionHamper.aiDecID,
                            name0 = decisionHamper.name,
                            type = AIType.Decision,
                            priority = Priority.Low
                        };
                        listOfDecisionTasksNonCritical.Add(taskHandout);
                    }
                }
                else
                {
                    //Blind Eye
                    if (resources >= decisionBlindEye.cost)
                    {
                        AITask taskHandout = new AITask()
                        {
                            data1 = decisionBlindEye.cost,
                            data2 = decisionBlindEye.aiDecID,
                            name0 = decisionBlindEye.name,
                            type = AIType.Decision,
                            priority = Priority.Low
                        };
                        listOfDecisionTasksNonCritical.Add(taskHandout);
                    }
                }
            }
            //MEDIUM IMPACT
            if (cityLoyalty <= handoutLoyaltyCriteriaMed)
            {
                //randomly choose one Medium impact handout
                if (Random.Range(0, 100) < 50)
                {
                    //Austerity Payments
                    if (resources >= decisionAusterity.cost)
                    {
                        AITask taskHandout = new AITask()
                        {
                            data1 = decisionAusterity.cost,
                            data2 = decisionAusterity.aiDecID,
                            name0 = decisionAusterity.name,
                            type = AIType.Decision,
                            priority = Priority.Medium
                        };
                        listOfDecisionTasksNonCritical.Add(taskHandout); listOfDecisionTasksNonCritical.Add(taskHandout);
                    }
                }
                else
                {
                    //Holiday
                    if (resources >= decisionHoliday.cost)
                    {
                        AITask taskHandout = new AITask()
                        {
                            data1 = decisionHoliday.cost,
                            data2 = decisionHoliday.aiDecID,
                            name0 = decisionHoliday.name,
                            type = AIType.Decision,
                            priority = Priority.Medium
                        };
                        listOfDecisionTasksNonCritical.Add(taskHandout); listOfDecisionTasksNonCritical.Add(taskHandout);
                    }
                }
            }
            //HIGH IMPACT
            if (cityLoyalty <= handoutLoyaltyCriteriaHigh)
            {

                //randomly choose one High impact handout

                //Medical Clinics
                if (resources >= decisionMedical.cost)
                {
                    AITask taskHandout = new AITask()
                    {
                        data1 = decisionMedical.cost,
                        data2 = decisionMedical.aiDecID,
                        name0 = decisionMedical.name,
                        type = AIType.Decision,
                        priority = Priority.High
                    };
                    listOfDecisionTasksNonCritical.Add(taskHandout); listOfDecisionTasksNonCritical.Add(taskHandout); listOfDecisionTasksNonCritical.Add(taskHandout);
                }
            }
        }
        //
        // - - - Network (AI CounterMeasures) - - - 
        //
        // No countermeasure options are valid if rebooting or offline
        if (isRebooting == false || isOffline == false)
        {
            //priority depends on how many times hacking has been detected since last reboot
            Priority priorityDetect = Priority.Low;
            int priorityWeight = 0;
            // Determine priority -> applies to all AI countermeasures (priority up a notch if Mayor has 'Tech Savvy' trait
            switch (hackingAttemptsDetected)
            {
                case 0:
                    if (city.mayor.CheckTraitEffect(aiCounterMeasurePriorityRaise) == false)
                    { priorityDetect = Priority.Low; priorityWeight = priorityLowWeight; }
                    else
                    { priorityDetect = Priority.Medium; priorityWeight = priorityMediumWeight; }
                    break;
                case 1:
                    if (city.mayor.CheckTraitEffect(aiCounterMeasurePriorityRaise) == false)
                    { priorityDetect = Priority.Medium; priorityWeight = priorityMediumWeight; }
                    else
                    { priorityDetect = Priority.High; priorityWeight = priorityHighWeight; }
                    break;
                case 2:
                    if (city.mayor.CheckTraitEffect(aiCounterMeasurePriorityRaise) == false)
                    { priorityDetect = Priority.High; priorityWeight = priorityHighWeight; }
                    else
                    { priorityDetect = Priority.Critical; }
                    break;
                default:
                    priorityDetect = Priority.Critical;
                    priorityWeight = priorityHighWeight;
                    /*Debug.LogWarningFormat("Invalid hackingAttemptsDetected {0}", hackingAttemptsDetected);*/
                    break;
            }
            if (priorityDetect != Priority.Critical)
            { Debug.Assert(priorityWeight > 0, string.Format("Invalid priorityWeight (zero) -> hackingAttemptsDetected {0}", hackingAttemptsDetected)); }
            //AI Security Protocols -> increase chance of detection
            if (aiSecurityProtocolLevel < aiSecurityProtocolMaxLevel)
            {
                AITask taskProtocol = new AITask()
                {
                    data1 = decisionProtocol.cost,
                    data2 = decisionProtocol.aiDecID,
                    name0 = decisionProtocol.name,
                    type = AIType.Decision,
                    priority = priorityDetect
                };
                if (priorityDetect == Priority.Critical)
                { listOfDecisionTasksCritical.Add(taskProtocol); }
                else
                {
                    for (int i = 0; i < priorityWeight; i++)
                    { listOfDecisionTasksNonCritical.Add(taskProtocol); }
                }
            }
            //AI TraceBack
            if (isTraceBack == false)
            {
                AITask taskTraceBack = new AITask()
                {
                    data1 = decisionTraceBack.cost,
                    data2 = decisionTraceBack.aiDecID,
                    name0 = decisionTraceBack.name,
                    type = AIType.Decision,
                    priority = priorityDetect
                };
                if (priorityDetect == Priority.Critical)
                { listOfDecisionTasksCritical.Add(taskTraceBack); }
                else
                {
                    for (int i = 0; i < priorityWeight; i++)
                    { listOfDecisionTasksNonCritical.Add(taskTraceBack); }
                }
            }
            //AI Screamer
            if (isScreamer == false)
            {
                AITask taskScreamer = new AITask()
                {
                    data1 = decisionScreamer.cost,
                    data2 = decisionScreamer.aiDecID,
                    name0 = decisionScreamer.name,
                    type = AIType.Decision,
                    priority = priorityDetect
                };
                if (priorityDetect == Priority.Critical)
                { listOfDecisionTasksCritical.Add(taskScreamer); }
                else
                {
                    for (int i = 0; i < priorityWeight; i++)
                    { listOfDecisionTasksNonCritical.Add(taskScreamer); }
                }
            }
            //AI Offline
            if (isTraceBack == false && isScreamer == false)
            {
                AITask taskOffline = new AITask()
                {
                    data1 = decisionOffline.cost,
                    data2 = decisionOffline.aiDecID,
                    name0 = decisionOffline.name,
                    type = AIType.Decision,
                    priority = priorityDetect
                };
                if (priorityDetect == Priority.Critical)
                { listOfDecisionTasksCritical.Add(taskOffline); }
                else
                {
                    for (int i = 0; i < priorityWeight; i++)
                    { listOfDecisionTasksNonCritical.Add(taskOffline); }
                }
            }
        }
        //
        // - - - Select task -> Critical have priority - - - 
        //
        Debug.LogFormat("[Aim] -> ProcessDecisionTask: {0} Critical tasks available{1}", listOfDecisionTasksCritical.Count, "\n");
        Debug.LogFormat("[Aim] -> ProcessDecisionTask: {0} Non-Critical tasks available{1}", listOfDecisionTasksNonCritical.Count, "\n");
        if (listOfDecisionTasksCritical.Count > 0)
        {
            AITask task = listOfDecisionTasksCritical[Random.Range(0, listOfDecisionTasksCritical.Count)];
            listOfTasksPotential.Add(task);
        }
        else
        {
            //choose a non-critical task from the weighted list
            if (listOfDecisionTasksNonCritical.Count > 0)
            {
                AITask task = listOfDecisionTasksNonCritical[Random.Range(0, listOfDecisionTasksNonCritical.Count)];
                listOfTasksPotential.Add(task);
            }
        }
    }

    /// <summary>
    /// Chooses a connection for a 'raise Connection Security Level' decision. Returns connID of connection, or '-1' if no suitable connection found
    /// </summary>
    public int ProcessConnectionSelection()
    {
        bool isDone = false;
        int connID = -1;
        int index;
        List<Node> listOfDecisionNodes = GameManager.instance.dataScript.GetListOfDecisionNodes();
        List<Node> tempList = new List<Node>();
        if (listOfDecisionNodes != null)
        {
            /*Debug.LogFormat("ListOfDecisionNodes -> Start -> {0}  Turn {1}", listOfDecisionNodes.Count, GameManager.instance.turnScript.Turn);*/
            Faction factionAuthority = GameManager.instance.factionScript.factionAuthority;
            if (factionAuthority != null)
            {
                NodeArc preferredNodeArc = factionAuthority.preferredArc;
                if (preferredNodeArc != null)
                {
                    //reverse loop list of most connected nodes and find any that match the preferred node type (delete entries from list to prevent future selection)
                    for (int i = listOfDecisionNodes.Count - 1; i >= 0; i--)
                    {
                        if (listOfDecisionNodes[i].Arc.name.Equals(preferredNodeArc.name) == true)
                        {
                            //add to tempList and remove from decision List
                            tempList.Add(listOfDecisionNodes[i]);
                            listOfDecisionNodes.RemoveAt(i);
                        }
                    }
                    //found any suitable nodes and do they have suitable connections?
                    if (tempList.Count > 0)
                    {
                        /*Debug.LogFormat("ListOfDecisionNodes -> TempList.Count {0}", tempList.Count);*/
                        do
                        {
                            index = Random.Range(0, tempList.Count);
                            connID = ProcessConnectionSelectionNode(tempList[index]);
                            if (connID == -1)
                            { tempList.RemoveAt(index); }
                            else { break; }
                        }
                        while (tempList.Count > 0);
                    }
                }
                else { Debug.LogWarning("Invalid preferredNodeArc (Null)"); }
                /*Debug.LogFormat("ListOfDecisionNodes -> Preferred Nodes Done -> {0}", listOfDecisionNodes.Count);*/
                //keep looking if not yet successful. List should have all preferred nodes stripped out.
                if (connID == -1)
                {
                    /*Debug.Log("ListOfDecisionNodes -> Look for a Random Node");*/
                    //randomly choose nodes looking for suitable connections. Delete as you go to prevent future selections.
                    if (listOfDecisionNodes.Count > 0)
                    {
                        do
                        {
                            index = Random.Range(0, listOfDecisionNodes.Count);
                            Node nodeTemp = listOfDecisionNodes[index];
                            connID = ProcessConnectionSelectionNode(nodeTemp);
                            if (connID == -1)
                            { listOfDecisionNodes.RemoveAt(index); } //not needed with refactored code but left in anyway
                            else { break; }
                        }
                        while (listOfDecisionNodes.Count > 0);
                    }
                }
            }
            else { Debug.LogWarning("Invalid factionAuthority (Null)"); }
        }
        else { Debug.LogWarning("Invalid listOfMostConnectedNodes (Null)"); }
        if (isDone == true)
        { 
            //update listOfDecisionNodes
            GameManager.instance.aiScript.SetDecisionNodes();
        }
        return connID;
    }

    /// <summary>
    /// sub-Method for ProcessConnectionSelection that takes a node, checks for a Securitylevel.None connection, returns connID or '-1' if not round
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private int ProcessConnectionSelectionNode(Node node)
    {
        int connID = -1;
        if (node != null)
        {
            List<Connection> listOfConnections = node.GetListOfConnections();
            if (listOfConnections != null)
            {
                Node nodeFar = null;
                //loop connections and take first one with no security
                foreach (Connection connection in listOfConnections)
                {
                    if (connection.SecurityLevel == ConnectionType.None)
                    {
                        nodeFar = connection.node1;
                        //check that we've got the correct connection end
                        if (nodeFar.nodeID == node.nodeID)
                        { nodeFar = connection.node2; }
                        //check that the far node has at least 2 connections (ignore single dead end connections)
                        if (nodeFar.CheckNumOfConnections() > 1)
                        {
                            connID = connection.connID;
                            break;
                        }
                    }
                }
            }
            else { Debug.LogWarningFormat("Invalid listOfConnections (Null) for nodeID {0}", node.nodeID); }
        }
        else { Debug.LogWarning("Invalid node (Null)"); }
        return connID;
    }

    /// <summary>
    /// Selects tasks from pool of potential tasks for this turn
    /// </summary>
    private void ProcessFinalTasks(int numOfFactionTasks)
    {
        int numTasks = listOfTasksPotential.Count;
        int index, remainingChoices;
        float baseOdds = 100f;
        int numTasksSelected = 0;
        int maxCombinedTasks = Mathf.Min(numOfFinalTasks, numOfFactionTasks);
        if (numTasks > 0)
        {
            List<AITask> listOfTasksCritical = new List<AITask>();
            List<AITask> listOfTasksNonCritical = new List<AITask>();
            List<AITask> tempList = new List<AITask>();
            //loop listOfTasksPotential and split tasks into Critical and non critical
            foreach (AITask task in listOfTasksPotential)
            {
                switch (task.priority)
                {
                    case Priority.Critical:
                        listOfTasksCritical.Add(task);
                        break;
                    default:
                        listOfTasksNonCritical.Add(task);
                        break;
                }
            }
            //Critical tasks first
            numTasks = listOfTasksCritical.Count;
            if (numTasks > 0)
            {
                //note combined tasks here which could be less than maxFinalTasks
                if (numTasks <= maxCombinedTasks)
                {
                    //enough available tasks to do all
                    foreach (AITask task in listOfTasksCritical)
                    {
                        listOfTasksFinal.Add(task);
                        task.chance = (int)baseOdds;
                        numTasksSelected++;
                    }
                }
                else
                {
                    //insufficient tasks -> set chance to 0
                    foreach (AITask task in listOfTasksCritical)
                    { task.chance = 0; }

                    //randomly choose
                    do
                    {
                        index = Random.Range(0, listOfTasksCritical.Count);
                        listOfTasksFinal.Add(listOfTasksCritical[index]);
                        numTasksSelected++;
                        //remove entry from list to prevent future selection
                        listOfTasksCritical.RemoveAt(index);
                    }
                    while (numTasksSelected < numOfFinalTasks && listOfTasksCritical.Count > 0);
                    /*while (numTasksSelected < numOfFinalTasks || listOfTasksCritical.Count == 0);*/
                }
            }
            //still room 
            if (numTasksSelected < numOfFinalTasks)
            {
                numTasks = listOfTasksNonCritical.Count;
                remainingChoices = numOfFinalTasks - numTasksSelected;
                //Non-Critical tasks next
                if (remainingChoices >= numTasks)
                {
                    /*//do all -> assign odds first
                    foreach (AITask task in listOfTasksNonCritical)
                    { task.chance = (int)baseOdds; }*/
                    //select tasks
                    foreach (AITask task in listOfTasksNonCritical)
                    {
                        task.chance = 0;
                        listOfTasksFinal.Add(task);
                        numTasksSelected++;
                        if (numTasksSelected >= numOfFinalTasks)
                        { break; }
                    }
                }
                //need to randomly select from pool of tasks based on priority probabilities
                else
                {
                    //populate tempList with copies of NonCritical tasks depending on priority (low -> 1 copy, medium -> 2 copies, high -> 3 copies)
                    foreach (AITask task in listOfTasksNonCritical)
                    {
                        task.chance = 0;
                        switch (task.priority)
                        {
                            case Priority.High:
                                for (int i = 0; i < priorityHighWeight; i++)
                                { tempList.Add(task); }
                                break;
                            case Priority.Medium:
                                for (int i = 0; i < priorityMediumWeight; i++)
                                { tempList.Add(task); }
                                break;
                            case Priority.Low:
                                for (int i = 0; i < priorityLowWeight; i++)
                                { tempList.Add(task); }
                                break;
                            default:
                                Debug.LogWarning(string.Format("Invalid task.priority \"{0}\" for nodeID {1}, team {2}", task.priority, task.data0, task.name1));
                                break;
                        }
                    }

                    /*//work out and assign odds first
                    numTasks = tempList.Count;
                    foreach (AITask task in listOfTasksNonCritical)
                    {
                        switch (task.priority)
                        {
                            case Priority.High:
                                task.chance = (int)(((float)priorityHighWeight / (float)numTasks) * 100 * remainingChoices);
                                break;
                            case Priority.Medium:
                                task.chance = (int)(((float)priorityMediumWeight / (float)numTasks) * 100 * remainingChoices);
                                break;
                            case Priority.Low:
                                task.chance = (int)(((float)priorityLowWeight / (float)numTasks) * 100 * remainingChoices);
                                break;
                        }
                    }*/

                    //randomly draw from pool
                    int taskID;
                    do
                    {
                        numTasks = tempList.Count;
                        if (numTasks > 0)
                        {
                            index = Random.Range(0, numTasks);
                            listOfTasksFinal.Add(tempList[index]);
                            taskID = tempList[index].taskID;
                            numTasksSelected++;
                            //don't bother unless further selections are needed
                            if (numTasksSelected < numOfFinalTasks)
                            {
                                //reverse loop and remove all instances of task from tempList to prevent duplicate selections
                                for (int i = numTasks - 1; i >= 0; i--)
                                {
                                    if (tempList[i].taskID == taskID)
                                    { tempList.RemoveAt(i); }
                                }
                            }
                        }
                        else { numTasksSelected++; }
                    }
                    while (numTasksSelected < numOfFinalTasks);

                }
            }
            //copy tasks with chance != 100 (non-critical tasks) to tempList for calculation of odds
            tempList.Clear();
            //new variables
            int numTasksAlreadyChosen = 0;
            foreach (AITask task in listOfTasksFinal)
            {
                if (task.chance != 100)
                { tempList.Add(task); }
                else { numTasksAlreadyChosen++; }
            }
            
            //work out and assign odds
            int numOfRemainingTasks = tempList.Count;
            int numOfOutstandingChoices = numOfFactionTasks - numTasksAlreadyChosen;
            numOfOutstandingChoices = Mathf.Max(0, numOfOutstandingChoices);
            if (numOfRemainingTasks > 0 )
            {
                //otherwise keep chance at 0 as no more choices required
                if (numOfOutstandingChoices > 0)
                {
                    if (numOfOutstandingChoices < numOfRemainingTasks)
                    {
                        foreach (AITask task in tempList)
                        { task.chance = (int)(baseOdds * (float)numOfOutstandingChoices / (float)numOfRemainingTasks); }
                    }
                    else
                    {
                        foreach (AITask task in tempList)
                        { task.chance = (int)baseOdds; }
                    }
                }
            }
        }
        else { Debug.Log(string.Format("[Aim]  -> ProcessTasksFinal: No tasks this turn{0}", "\n")); }
    }

    /// <summary>
    /// takes listOfPotentialTasks and packages up data for AIDisplayUI. Returns package even if all data is blanked
    /// </summary>
    private void UpdateTaskDisplayData()
    {
        AIDisplayData data = new AIDisplayData();
        int count = listOfTasksFinal.Count;
        Tuple<int, string> resultsCost = GetHackingCost();
        hackingModifiedCost = resultsCost.Item1;
        //pass timer
        data.rebootTimer = rebootTimer;
        //decision test
        data.renownDecision = string.Format("Hack AI for {0}{1}{2} Renown", colourNeutral, hackingModifiedCost, colourEnd);
        //if tasks are present, process into descriptor strings
        if (count > 0)
        {
            string colourChance;
            //loop final tasks
            for (int i = 0; i < count; i++)
            {
                AITask task = listOfTasksFinal[i];
                if (task != null)
                {
                    //colour code chance (green if upper third %, yellow for middle, red for lower third)
                    if (task.chance > 66) { colourChance = colourGood; }
                    else if (task.chance < 33) { colourChance = colourBad; }
                    else { colourChance = colourNeutral; }
                    //get upper and lower & tooltip strings
                    Tuple<string, string, string, string, int, int> results = GetTaskDescriptors(task);
                    //tooltip text
                    //up to 3 tasks
                    switch (i)
                    {
                        case 0:
                            data.task_1_textUpper = results.Item1;
                            data.task_1_textLower = results.Item2;
                            data.task_1_chance = string.Format("{0}{1}%{2}", colourChance, task.chance, colourEnd);
                            data.task_1_tooltipMain = results.Item3;
                            data.task_1_tooltipDetails = results.Item4;
                            data.nodeID_1 = results.Item5;
                            data.connID_1 = results.Item6;
                            break;
                        case 1:
                            data.task_2_textUpper = results.Item1;
                            data.task_2_textLower = results.Item2;
                            data.task_2_chance = string.Format("{0}{1}%{2}", colourChance, task.chance, colourEnd);
                            data.task_2_tooltipMain = results.Item3;
                            data.task_2_tooltipDetails = results.Item4;
                            data.nodeID_2 = results.Item5;
                            data.connID_2 = results.Item6;
                            break;
                        case 2:
                            data.task_3_textUpper = results.Item1;
                            data.task_3_textLower = results.Item2;
                            data.task_3_chance = string.Format("{0}{1}%{2}", colourChance, task.chance, colourEnd);
                            data.task_3_tooltipMain = results.Item3;
                            data.task_3_tooltipDetails = results.Item4;
                            data.nodeID_3 = results.Item5;
                            data.connID_3 = results.Item6;
                            break;
                        default:
                            Debug.LogWarningFormat("Invalid index {0} for listOfTasksFinal", i);
                            break;
                    }
                    //other data
                    data.factionDetails = string.Format("{0} Action{1}{2}{3}", factionAuthority.maxTaskPerTurn, 
                        factionAuthority.maxTaskPerTurn != 1 ? "s" : "", "\n", factionAuthority.name);
                }
                else { Debug.LogWarningFormat("Invalid AITask for listOfTasksFinal[{0}]", i); }
            }
        }
        EventManager.instance.PostNotification(EventType.AISendDisplayData, this, data, "AIManager.cs -> UpdateTaskDisplayData");
    }


    /// <summary>
    /// private sub-Method for ProcessTaskDescriptors that takes an AI task and returns three strings that correspond to AIDisplayUI textUpper & textLower & tooltip
    /// returns two ints that correspond to nodeID and connID if applicable (sends '-1' if not)
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    private Tuple<string, string, string, string, int, int> GetTaskDescriptors(AITask task)
    {
        string textUpper = "";
        string textLower = "";
        string tooltipMain = "";
        string tooltipDetails = "";
        int nodeID = -1;
        int connID = -1;
        if (task != null)
        {
            //tooltip Details (chance)
            switch(task.chance)
            {
                case 100:
                    tooltipDetails = string.Format("{0}Automatically{1} implemented at the {2}end of your turn{3}", colourNeutral, colourEnd, colourAlert, colourEnd);
                    break;
                case 0:
                    tooltipDetails = string.Format("{0}No chance{1} of being implemented at the {2}end of your turn{3}", colourNeutral, colourEnd, colourAlert, colourEnd);
                    break;
                default:
                    tooltipDetails = string.Format("{0}{1} %{2} chance of being implemented at the {3}end of your turn{4}", colourNeutral, task.chance, colourEnd, colourAlert, colourEnd);
                    break;
            }
            //tooltip Main
            switch(task.type)
            {
                case AIType.Team:
                    textUpper = string.Format("Deploy {0} Team", task.name1);
                    Node node = GameManager.instance.dataScript.GetNode(task.data0);
                    if (node != null)
                    {
                        textLower = string.Format("Deploy to {0}, {1} district", node.nodeName, node.Arc.name);
                        tooltipMain = string.Format("{0} district \"{1}\" is currently {2}highlighted{3} on the map", node.Arc.name, node.nodeName, colourNeutral, colourEnd);
                        nodeID = node.nodeID;
                    }
                    else
                    {
                        Debug.LogWarningFormat("Invalid node (Null) for task.data0 nodeID {0}", task.data0);
                        textLower = "Details unknown";
                    }
                    break;
                case AIType.Decision:
                    textUpper = string.Format("{0} DECISION", task.name0);
                    DecisionAI decisionAI = GameManager.instance.dataScript.GetAIDecision(task.data2);
                    if (decisionAI != null)
                    {
                        //Connection decision
                        if (decisionAI.name.Equals(decisionConnSec.name) == true)
                        {
                            Connection connection = GameManager.instance.dataScript.GetConnection(task.data0);
                            if (connection != null)
                            {
                                textLower = string.Format("Between {0} and {1}", connection.node1.nodeName, connection.node2.nodeName);
                                tooltipMain = string.Format("The connection is currently {0}highlighted{1} on the map and will have it's security {2}increased{3} by one level",
                                    colourNeutral, colourEnd, colourBad, colourEnd);
                                connID = connection.connID;
                            }
                            else
                            {
                                Debug.LogWarningFormat("Invalid connection (Null) for task.data0 connectionID {0}", task.data0);
                                textLower = "Details unknown";
                            }
                        }
                        //all other decisions
                        else
                        {
                            textLower = decisionAI.descriptor;
                            tooltipMain = decisionAI.tooltipDescriptor;
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat("Invalid decisionAI (Null) for task.data2 aiDecID {0}", task.data2);
                        textLower = "Details unknown";
                    }
                    break;
                default:
                    Debug.LogWarningFormat("Invalid task.type \"{0}\"", task.type);
                    break;
            }
        }
        else { Debug.LogWarning("Invalid AI task (Null)"); }
        return new Tuple<string, string, string, string, int, int>(textUpper, textLower, tooltipMain, tooltipDetails, nodeID, connID);
    }

    /// <summary>
    /// sub method (ProcessNodeTasks) that takes two node datapoint lists (must be the same datapoint, eg. security) and determines a task (Null if none) based on AI rules
    /// </summary>
    /// <param name="listCritical"></param>
    /// <param name="listNonCritical"></param>
    /// <returns></returns>
    private AITask SelectNodeTask(List<AINodeData> listCritical, List<AINodeData> listNonCritical, string name, int teamArcID)
    {
        AITask task = null;
        int index;
        List<AINodeData> tempList = new List<AINodeData>();
        //check for Critical tasks first
        int listCount = listCritical.Count;
        if (listCount > 0)
        {
            index = 0;
            if (listCount > 1)
            {
                //scan for any nodes of the preferred faction type
                foreach (AINodeData data in listCritical)
                {
                    if (data.isPreferred == true)
                    { tempList.Add(data); }
                }
                if (tempList.Count > 0)
                {
                    //randomly select a preferred faction option
                    index = Random.Range(0, tempList.Count);
                    //generate task
                    task = new AITask()
                    { data0 = tempList[index].nodeID, name0 = tempList[index].arc.name, name1 = name, priority = Priority.Critical };
                }
                else
                {
                    //otherwise randomly select any option
                    index = Random.Range(0, listCount);
                    //generate task
                    task = new AITask()
                    { data0 = listCritical[index].nodeID, name0 = listCritical[index].arc.name, name1 = name, priority = Priority.Critical };
                }
            }
            else
            {
                //single record only
                task = new AITask()
                { data0 = listCritical[0].nodeID, name0 = listCritical[0].arc.name, name1 = name, priority = Priority.Critical };
            }
        }
        else
        {
            //otherwise Non Critical
            listCount = listNonCritical.Count;
            if (listCount > 0)
            {
                index = 0;
                Priority priority = Priority.Low;
                tempList.Clear();
                //scan for any nodes of the preferred faction type
                foreach (AINodeData data in listNonCritical)
                {
                    if (data.isPreferred == true)
                    { tempList.Add(data); }
                }
                if (tempList.Count > 0)
                {
                    //randomly select a preferred faction option
                    index = Random.Range(0, tempList.Count);
                    //determine priority (one notch higher than normal due to being a preferred faction node arc)
                    switch (tempList[index].difference)
                    {
                        case 1: priority = Priority.Medium; break;
                        case 2: priority = Priority.High; break;
                        default:
                            Debug.LogWarning(string.Format("Invalid difference \"{0}\" for nodeID {1}", tempList[0].difference,
                       tempList[0].nodeID)); break;
                    }
                    //generate task
                    task = new AITask()
                    { data0 = tempList[index].nodeID, name0 = tempList[index].arc.name, name1 = name, priority = priority };
                }
                else
                {
                    //otherwise randomly select any option
                    index = Random.Range(0, listCount);
                    //determine priority
                    switch (listNonCritical[index].difference)
                    {
                        case 1: priority = Priority.Low; break;
                        case 2: priority = Priority.Medium; break;
                        default:
                            Debug.LogWarning(string.Format("Invalid difference \"{0}\" for nodeID {1}", listNonCritical[0].difference,
                       listNonCritical[0].nodeID)); break;
                    }
                    //generate task
                    task = new AITask()
                    { data0 = listNonCritical[index].nodeID, name0 = listNonCritical[index].arc.name, name1 = name, priority = priority };
                }
            }
        }
        //add final data
        if (task != null)
        {
            task.type = AIType.Team;
            task.data1 = teamArcID;
        }
        return task;
    }


    /// <summary>
    /// carry out all tasks in listOfTasksFinal. 
    /// </summary>
    private void ExecuteTasks(int numOfFactionTasks)
    {
        int rnd;
        int safetyCheck = 0;
        int counter = 0;
        Debug.LogFormat("[Aim] -> ExecuteTasks: {0} tasks available (faction limit {1} tasks) for AI this turn{2}", listOfTasksFinal.Count, numOfFactionTasks, "\n");
        if (listOfTasksFinal.Count == 0)
        { Debug.Log("[Aim] -> ExecuteTasks: NO TASKS AVAILABLE"); }
        //
        // - - - Execute all 100% tasks first & remove an 0 % tasks -> delete tasks as you go
        //
        for (int i = listOfTasksFinal.Count -1; i >= 0; i--)
        {
            AITask task = listOfTasksFinal[i];
            if (task != null)
            {
                Debug.LogFormat("[Aim] -> ExecuteTasks: listOfFinalTasks[{0}] {1} task, {2} priority, {3} % chance{4}", i, task.type, task.priority, task.chance, "\n");
                if (task.chance == 100)
                {
                    switch (task.type)
                    {
                        case AIType.Team:
                            ExecuteTeamTask(task);
                            break;
                        case AIType.Decision:
                            ExecuteDecisionTask(task);
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid task.type \"{0}\"", task.type));
                            break;
                    }

                    counter++;
                    //remove task once executed
                    listOfTasksFinal.RemoveAt(i);
                    //check max. tasks haven't been exceeded
                    if (counter >= numOfFactionTasks)
                    { break; }
                }
                else if (task.chance == 0)
                {
                    //remove all zero rated tasks as they'll never be implemented
                    listOfTasksFinal.RemoveAt(i);
                }
            }
            else { Debug.LogWarningFormat("Invalid task (Null) for listOfTasksFinal[{0}]", listOfTasksFinal); }
        }
        //
        //- - - Remaining Tasks
        //
        if (listOfTasksFinal.Count > 0)
        {
            //more need for faction task list
            if (counter < numOfFactionTasks)
            {
                do
                {
                    //keep checking until sufficient tasks have been executed or run out of tasks
                    for (int i = listOfTasksFinal.Count - 1; i >= 0; i--)
                    {
                        AITask task = listOfTasksFinal[i];
                        if (task != null)
                        {
                            rnd = Random.Range(0, 100);
                            //auto select if only one task remaining
                            if (rnd < task.chance || listOfTasksFinal.Count == 1)
                            {
                                switch (task.type)
                                {
                                    case AIType.Team:
                                        ExecuteTeamTask(task);
                                        break;
                                    case AIType.Decision:
                                        ExecuteDecisionTask(task);
                                        break;
                                    default:
                                        Debug.LogError(string.Format("Invalid task.type \"{0}\"", task.type));
                                        break;
                                }
                                counter++;
                                //remove task once executed
                                listOfTasksFinal.RemoveAt(i);
                                //interim check
                                if (counter >= numOfFactionTasks || listOfTasksFinal.Count == 0)
                                { break; }
                            }
                        }
                        else { Debug.LogWarningFormat("Invalid task (Null) for listOfTasksFinal[{0}]", listOfTasksFinal); }
                    }
                    safetyCheck++;
                    if (safetyCheck > 10)
                    {
                        Debug.LogWarning("SafetyCheck triggered (>10) in ExecuteTasks");
                        break;
                    }
                }
                while (counter < numOfFactionTasks && listOfTasksFinal.Count > 0);
                //
                // - - - Fallback if timed out -> low probability but need to cover it
                //
                if (counter < numOfFactionTasks)
                {
                    if (safetyCheck > 10 && listOfTasksFinal.Count > 0)
                    {
                        safetyCheck = 0;
                        //implement remaining tasks sequentially up to the faction limit required
                        Debug.LogWarning("Safety Check triggered -> Default sequential task implementation in progress");
                        do
                        {
                            //keep checking until sufficient tasks have been executed or run out of tasks
                            for (int i = listOfTasksFinal.Count - 1; i >= 0; i--)
                            {
                                AITask task = listOfTasksFinal[i];
                                if (task != null)
                                {
                                    switch (task.type)
                                    {
                                        case AIType.Team:
                                            ExecuteTeamTask(task);
                                            break;
                                        case AIType.Decision:
                                            ExecuteDecisionTask(task);
                                            break;
                                        default:
                                            Debug.LogError(string.Format("Invalid task.type \"{0}\"", task.type));
                                            break;
                                    }
                                    counter++;
                                    //remove task once executed
                                    listOfTasksFinal.RemoveAt(i);
                                    //interim check
                                    if (counter >= numOfFactionTasks || listOfTasksFinal.Count == 0)
                                    { break; }
                                }
                                else { Debug.LogWarningFormat("Invalid task (Null) for listOfTasksFinal[{0}]", listOfTasksFinal); }
                            }
                            safetyCheck++;
                            if (safetyCheck > 10)
                            {
                                Debug.LogWarning("SafetyCheck triggered SECOND TIME (>10) in fall back routine");
                                break;
                            }
                        }
                        while (counter < numOfFactionTasks && listOfTasksFinal.Count > 0);
                    }
                }
            }
        }
    }

    /// <summary>
    /// carry out a team deploy OnMap task
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteTeamTask(AITask task)
    {
        bool isSuccess = false;
        int dataID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, task.data1);
        if (dataID > -1)
        {
            Node node = GameManager.instance.dataScript.GetNode(task.data0);
            if (node != null)
            { isSuccess = GameManager.instance.teamScript.MoveTeamAI(TeamPool.OnMap, dataID, node); }
            else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", task.data0)); }
        }
        else { Debug.LogWarning(string.Format("Invalid teamID (-1) for teamArcID {0}", task.data1)); }
        //debug log
        if (isSuccess == true)
        { Debug.LogFormat("[Aim] -> ExecuteTeamTask: \"{0}\" Decision implemented{1}", task.name1, "\n"); }
        else { Debug.LogFormat("[Aim] -> ExecuteTeamTask: \"{0}\" Decision NOT implemented{1}", task.name1, "\n"); }
    }

    /// <summary>
    /// carry out a decision task. There is a cost to do so
    /// </summary>
    /// <param name="task"></param>
    private void ExecuteDecisionTask(AITask task)
    {
        //check enough resources in pool to carry out task
        int resources = GameManager.instance.dataScript.CheckAIResourcePool(globalAuthority);
        int decisionCost = task.data1;
        bool isSuccess = false;
        if (decisionCost <= resources)
        {
            //deduct cost
            resources -= decisionCost;
            GameManager.instance.dataScript.SetAIResources(globalAuthority, resources);
            Debug.LogFormat("[Aim] -> ExecuteDecisionTask: \"{0}\" decision, cost {1}, resources now {2}{3}", task.name0, decisionCost, resources, "\n");
            //Security Measures
            if (task.name0.Equals(decisionAPB.name) == true)
            {
                isSuccess = GameManager.instance.authorityScript.SetAuthoritySecurityState(decisionAPB.descriptor, decisionAPB.warning, AuthoritySecurityState.APB);
                EventManager.instance.PostNotification(EventType.StartSecurityFlash, this, null, "AIManager.cs -> ExecuteDecisionTask");
            }
            else if (task.name0.Equals(decisionSecAlert.name) == true)
            {
                isSuccess = GameManager.instance.authorityScript.SetAuthoritySecurityState(decisionSecAlert.descriptor, decisionSecAlert.warning, AuthoritySecurityState.SecurityAlert);
                EventManager.instance.PostNotification(EventType.StartSecurityFlash, this, null, "AIManager.cs -> ExecuteDecisionTask");
            }
            else if (task.name0.Equals(decisionCrackdown.name) == true)
            {
                isSuccess = GameManager.instance.authorityScript.SetAuthoritySecurityState(decisionCrackdown.descriptor, decisionCrackdown.warning, AuthoritySecurityState.SurveillanceCrackdown);
                EventManager.instance.PostNotification(EventType.StartSecurityFlash, this, null, "AIManager.cs -> ExecuteDecisionTask");
            }
            else if (task.name0.Equals(decisionConnSec.name) == true)
            { isSuccess = GameManager.instance.connScript.ProcessConnectionSecurityDecision(task.data0); }
            //logistics
            else if (task.name0.Equals(decisionRequestTeam.name) == true)
            { isSuccess = ProcessAITeamRequest(); }
            else if (task.name0.Equals(decisionResources.name) == true)
            { isSuccess = ProcessAIResourceRequest(); }
            //Network countermeasures
            else if (task.name0.Equals(decisionTraceBack.name) == true)
            { isSuccess = ProcessAITraceBack(); }
            else if (task.name0.Equals(decisionScreamer.name) == true)
            { isSuccess = ProcessAIScreamer(); }
            else if (task.name0.Equals(decisionProtocol.name) == true)
            { isSuccess = ProcessAIProtocol(); }
            else if (task.name0.Equals(decisionOffline.name) == true)
            { isSuccess = ProcessAIOffline(); }
            //policies
            else if (task.name0.Equals(decisionCensorship.name) == true)
            { isSuccess = ProcessAIPolicy(task); }
            else if (task.name0.Equals(decisionBanProtests.name) == true)
            { isSuccess = ProcessAIPolicy(task); }
            else if (task.name0.Equals(decisionCurfew.name) == true)
            { isSuccess = ProcessAIPolicy(task); }
            else if (task.name0.Equals(decisionRoboCop.name) == true)
            { isSuccess = ProcessAIPolicy(task); }
            else if (task.name0.Equals(decisionMartialLaw.name) == true)
            { isSuccess = ProcessAIPolicy(task); }
            else if (task.name0.Equals(decisionDrones.name) == true)
            { isSuccess = ProcessAIPolicy(task); }
            //handouts
            else if (task.name0.Equals(decisionHamper.name) == true)
            { isSuccess = ProcessAIHandout(task); }
            else if (task.name0.Equals(decisionBlindEye.name) == true)
            { isSuccess = ProcessAIHandout(task); }
            else if (task.name0.Equals(decisionAusterity.name) == true)
            { isSuccess = ProcessAIHandout(task); }
            else if (task.name0.Equals(decisionHoliday.name) == true)
            { isSuccess = ProcessAIHandout(task); }
            else if (task.name0.Equals(decisionMedical.name) == true)
            { isSuccess = ProcessAIHandout(task); }
            else
            { Debug.LogWarningFormat("Invalid task.name0 \"{0}\"", task.name0); }
            //debug logs
            if (isSuccess == true) { Debug.LogFormat("[Aim] -> ExecuteDecisionTask: \"{0}\" Decision implemented{1}", task.name0, "\n"); }
            else { Debug.LogFormat("[Aim] -> ExecuteDecisionTask: \"{0}\" Decision NOT implemented{1}", task.name0, "\n"); }
        }
        else
        {
            //decision cancelled due to insufficient resources
            Debug.LogFormat("[Aim] -> ExecuteDecisionTask: INSUFFICIENT RESOURCES to implement \"{0}\" decision{1}", task.name0, "\n");
            isInsufficientResources = true;
        }
    }

    /// <summary>
    /// Processes an AI policy decision from Authority. Returns true if successful
    /// NOTE: task has been checked for Null by the calling method (ExecuteDecisionTask)
    /// </summary>
    /// <returns></returns>
    private bool ProcessAIPolicy(AITask task)
    {
        //deduct city Loyalty & update node crisis modifier
        int cityLoyalty = GameManager.instance.cityScript.CityLoyalty;
        int loyaltyChange = 0;
        int nodeCrisisModifier = 0;
        //task priority determines impact category
        switch (task.priority)
        {
            case Priority.Low:
                loyaltyChange = policyCityLoyaltyLow;
                nodeCrisisModifier = policyNodeCrisisLow;
                break;
            case Priority.Medium:
                loyaltyChange = policyCityLoyaltyMed;
                nodeCrisisModifier = policyNodeCrisisMed;
                break;
            case Priority.High:
                loyaltyChange = policyCityLoyaltyHigh;
                nodeCrisisModifier = policyNodeCrisisHigh;
                break;
            default:
                Debug.LogWarningFormat("Invalid task.priority \"{0}\"", task.priority);
                break;
        }
        //update
        cityLoyalty -= loyaltyChange;
        cityLoyalty = Mathf.Max(0, cityLoyalty);
        GameManager.instance.cityScript.CityLoyalty = cityLoyalty;
        GameManager.instance.nodeScript.crisisPolicyModifier = nodeCrisisModifier;
        //set vars
        isPolicy = true;
        timerPolicy = aiPolicyTimer;
        policyName = task.name0;
        policyEffectCrisis = nodeCrisisModifier;
        policyEffectLoyalty = loyaltyChange;
        //trait -> PolicyWonk
        if (city.mayor.CheckTraitEffect(aiPolicyTimerDoubled) == true)
        {
            timerPolicy *= 2;
            Debug.LogFormat("[Trt] {0} uses {1} trait, timerPolicy now {2}{3}", city.mayor.name, city.mayor.GetTrait().tag, timerPolicy, "\n");
        }
        //trait -> Expert
        else if (city.mayor.CheckTraitEffect(aiPolicyCostLower) == true)
        {
            //add an extra resource back to the pool (policy costs less)
            int resources = GameManager.instance.dataScript.CheckAIResourcePool(globalAuthority);
            resources += 1;
            GameManager.instance.dataScript.SetAIResources(globalAuthority, resources);
            Debug.LogFormat("[Trt] {0} uses {1} trait, +1 resource now {2}{3}", city.mayor.name, city.mayor.GetTrait().tag, resources, "\n");
        }
        //trait -> Sloppy
        else if (city.mayor.CheckTraitEffect(aiPolicyCostHigher) == true)
        {
            //deduct an extra resource (policy costs more)
            int resources = GameManager.instance.dataScript.CheckAIResourcePool(globalAuthority);
            resources -= 1;
            resources = Mathf.Max(0, resources);
            GameManager.instance.dataScript.SetAIResources(globalAuthority, resources);
            Debug.LogFormat("[Trt] {0} uses {1} trait, -1 resource now {2}{3}", city.mayor.name, city.mayor.GetTrait().tag, resources, "\n");
        }
        string policyDescription = "Unknown";
        DecisionAI decision = GameManager.instance.dataScript.GetAIDecision(task.data2);
        if (decision != null)
        { policyDescription = decision.descriptor; }
        else { Debug.LogWarningFormat("Invalid decision (Null) for decID {0}", task.data2); }
        //admin
        string msgText = string.Format("Authority implements {0} policy", policyName);
        GameManager.instance.messageScript.DecisionGlobal(msgText, msgText, policyDescription, task.data2, timerPolicy - 1, loyaltyChange, nodeCrisisModifier);
        msgText = string.Format("{0} loyalty has decreased by -{1} ({2} policy)", city.name, loyaltyChange, policyName);
        string reasonText = string.Format("{0} policy", policyName);
        GameManager.instance.messageScript.CityLoyalty(msgText, reasonText, cityLoyalty, loyaltyChange);
        return true;
    }


    /// <summary>
    /// Process an AI Handout decision from Authority. Returns true if successful
    /// NOTE: task has been checked for null by the calling method (ExcecuteDecisionTask)
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    private bool ProcessAIHandout(AITask task)
    {
        //deduct city Loyalty
        int cityLoyalty = GameManager.instance.cityScript.CityLoyalty;
        int loyaltyChange = 0;
        //task priority determines impact category
        switch (task.priority)
        {
            case Priority.Low:
                loyaltyChange = handoutCityLoyaltyLow;
                break;
            case Priority.Medium:
                loyaltyChange = handoutCityLoyaltyMed;
                break;
            case Priority.High:
                loyaltyChange = handoutCityLoyaltyHigh;
                break;
            default:
                Debug.LogWarningFormat("Invalid task.priority \"{0}\"", task.priority);
                break;
        }
        //update
        cityLoyalty += loyaltyChange;
        cityLoyalty = Mathf.Max(0, cityLoyalty);
        GameManager.instance.cityScript.CityLoyalty = cityLoyalty;
        //set cooldown timer
        timerHandout = handoutCooldownTimer;
        //trait -> Liberal
        if (city.mayor.CheckTraitEffect(aiHandoutCostLower) == true)
        {
            //add an extra resource back to the pool (handout costs less)
            int resources = GameManager.instance.dataScript.CheckAIResourcePool(globalAuthority);
            resources += 1;
            GameManager.instance.dataScript.SetAIResources(globalAuthority, resources);
            Debug.LogFormat("[Trt] {0} uses {1} trait, +1 resource now {2}{3}", city.mayor.name, city.mayor.GetTrait().tag, resources, "\n");
        }
        //trait -> Penny Pincher
        else if (city.mayor.CheckTraitEffect(aiHandoutCostHigher) == true)
        {
            //deduct an extra resource (handout costs more)
            int resources = GameManager.instance.dataScript.CheckAIResourcePool(globalAuthority);
            resources -= 1;
            resources = Mathf.Max(0, resources);
            GameManager.instance.dataScript.SetAIResources(globalAuthority, resources);
            Debug.LogFormat("[Trt] {0} uses {1} trait, -1 resource now {2}{3}", city.mayor.name, city.mayor.GetTrait().tag, resources, "\n");
        }
        string handoutDescription = "Unknown";
        DecisionAI decision = GameManager.instance.dataScript.GetAIDecision(task.data2);
        if (decision != null)
        { handoutDescription = decision.descriptor; }
        else { Debug.LogWarningFormat("Invalid decision (Null) for decID {0}", task.data2); }
        //admin
        string msgText = string.Format("Authority implements {0} policy", task.name0);
        GameManager.instance.messageScript.DecisionGlobal(msgText, msgText, handoutDescription, task.data2, 0, loyaltyChange);
        msgText = string.Format("{0} loyalty has increased by +{1} ({2} policy)", city.name, loyaltyChange, task.name0);
        string reasonText = string.Format("{0} policy", task.name0);
        GameManager.instance.messageScript.CityLoyalty(msgText, reasonText, cityLoyalty, loyaltyChange);
        return true;
    }

    /// <summary>
    /// Processes a 'Request Resources' decision from Authority. Returns true if successful
    /// </summary>
    /// <returns></returns>
    private bool ProcessAIResourceRequest()
    {
        bool isSuccess = false;
        int amount = 0;
        //each faction has a % chance of the request being approved (acts as a friction limiter on faction efficiency as resources drive everything)
        int rnd = Random.Range(0, 100);
        int adjustedChance = resourcesChance + numOfUnsuccessfulResourceRequests * resourcesBoost;
        if (rnd < resourcesChance)
        {
            amount = factionAuthority.resourcesStarting + numOfSuccessfulResourceRequests;
            numOfSuccessfulResourceRequests++;
            int resourcePool = GameManager.instance.dataScript.CheckAIResourcePool(globalAuthority) + amount;
            //add faction starting resources amount to their resource pool + 1 for each successful request
            GameManager.instance.dataScript.SetAIResources(globalAuthority, resourcePool);
            isSuccess = true;
            Debug.LogFormat("[Rnd] AIManager.cs -> ProcessAIResourceRequest: APPROVED need < {0}, rolled {1}{2}", adjustedChance, rnd, "\n");
        }
        //message & request counter
        string text = "";
        if (isSuccess == true)
        {
            Debug.Assert(amount > 0, "Invalid amount (zero)");
            text = string.Format("Request for Resources APPROVED ({0} added to pool)", amount);
            //reset counter back to zero
            numOfUnsuccessfulResourceRequests = 0;
        }
        else
        {
            text = string.Format("Request for Resources DENIED ({0} % chance of being Approved)", adjustedChance);
            Debug.LogFormat("[Rnd] AIManager.cs -> ProcessAIResourceRequest: DENIED need < {0}, rolled {1}{2}", adjustedChance, rnd, "\n");
            //increment counter
            numOfUnsuccessfulResourceRequests++;
            amount = 0;
        }
        GameManager.instance.messageScript.DecisionRequestResources(text, globalAuthority, amount);
        return isSuccess;
    }


    /// <summary>
    /// handles AI team request. Returns true if request successful and adds an single Team to the Reserve pool. Handles logic for what type of team.
    /// Calling method assumed to have already checked team ratio vs. teamRatioThreshold
    /// </summary>
    /// <returns></returns>
    public bool ProcessAITeamRequest()
    {
        bool isDone = false;
        int teamArc = -1;
        int teamCap = 2;
        int teamID = -1;
        string msgText = "";
        List<TeamArc> listOfTeamPrioritiesHigh = GameManager.instance.teamScript.GetListOfTeamPrioritiesHigh();
        List<TeamArc> listOfTeamPrioritiesMedium = GameManager.instance.teamScript.GetListOfTeamPrioritiesMed();
        if (listOfTeamPrioritiesHigh != null)
        {
            if (listOfTeamPrioritiesMedium != null)
            {
                if (listOfTeamPrioritiesHigh.Count > 0 || listOfTeamPrioritiesMedium.Count > 0)
                {
                    //keeps checking for teams (high priority then medium, ignores low) that are under a teamCap (2 of each, then 3 of each), until one found
                    List<TeamArc> tempList = new List<TeamArc>();
                    do
                    {
                        //first check if there are less than 2 team of all high priority team types
                        if (listOfTeamPrioritiesHigh.Count > 0)
                        {
                            tempList.Clear();
                            foreach (TeamArc arc in listOfTeamPrioritiesHigh)
                            {
                                if (GameManager.instance.dataScript.CheckTeamInfo(arc.TeamArcID, TeamInfo.Total) < teamCap)
                                { tempList.Add(arc); }
                            }
                            if (tempList.Count > 0)
                            {
                                teamArc = tempList[Random.Range(0, tempList.Count)].TeamArcID;
                                isDone = true;
                            }
                        }
                        //second check if there are less than 2 teams of all medium priority team types
                        if (isDone == false)
                        {
                            //must be at least one record   
                            if (listOfTeamPrioritiesMedium.Count > 0)
                            {
                                tempList.Clear();
                                foreach (TeamArc arc in listOfTeamPrioritiesMedium)
                                {
                                    if (GameManager.instance.dataScript.CheckTeamInfo(arc.TeamArcID, TeamInfo.Total) < teamCap)
                                    { tempList.Add(arc); }
                                }
                                if (tempList.Count > 0)
                                {
                                    teamArc = tempList[Random.Range(0, tempList.Count)].TeamArcID;
                                    isDone = true;
                                }
                            }
                        }
                        teamCap++;
                    }
                    while (isDone == false && teamCap < 4);
                }
                else { Debug.LogError("No valid records in listOfTeamPriorities High and Medium"); }
            }
            else { Debug.LogError("Invalid listOfTeamPriortiesMedium (Null)"); }
        }
        else { Debug.LogError("Invalid listOfTeamPriortiesHigh (Null)"); }
        //Add a new team if successful
        if (isDone == true && teamArc > -1)
        {
            Team team = new Team(teamArc, teamCap - 1);
            //update team info
            GameManager.instance.dataScript.AdjustTeamInfo(teamArc, TeamInfo.Reserve, +1);
            GameManager.instance.dataScript.AdjustTeamInfo(teamArc, TeamInfo.Total, +1);
            msgText = string.Format("Request for Team: {0} {1} added to Reserves", team.arc.name, team.teamName);
            teamID = team.teamID;
        }
        else
        { msgText = "Request for Team DENIED"; }
        //message
        GameManager.instance.messageScript.DecisionRequestTeam(msgText, teamID);
        return isDone;
    }

    /// <summary>
    /// Implements TraceBack AI Countermeasure. Returns true if successful
    /// </summary>
    /// <returns></returns>
    private bool ProcessAITraceBack()
    {
        isTraceBack = true;
        //countermeasure last for a set period of time (2 x if mayor has 'Daemon' trait)
        if (city.mayor.CheckTraitEffect(aiCounterMeasureTimerDoubled) == true)
        { timerTraceBack = aiCounterMeasureTimer * 2; }
        else { timerTraceBack = aiCounterMeasureTimer; }
        //Message
        string msgText = string.Format("AI activates TRACEBACK Countermeasure ({0} turn duration)", timerTraceBack);
        string itemText = "AI TRACEBACK countermeasures implemented";
        string warning = "Hacker's Location revealed if detected";
        GameManager.instance.messageScript.AICounterMeasure(msgText, itemText, warning, timerTraceBack - 1);
        return isTraceBack;
    }

    /// <summary>
    /// Implements Screamer AI Countermeasure. Returns true if successful
    /// </summary>
    /// <returns></returns>
    private bool ProcessAIScreamer()
    {
        isScreamer = true;
        //countermeasure last for a set period of time (2 x if mayor has 'Daemon' trait)
        if (city.mayor.CheckTraitEffect(aiCounterMeasureTimerDoubled) == true)
        { timerScreamer = aiCounterMeasureTimer * 2; }
        else { timerScreamer = aiCounterMeasureTimer; }
        //Message
        string msgText = string.Format("AI activates SCREAMER Countermeasure ({0} turn duration)", timerScreamer);
        string itemText = "AI SCREAMER countermeasures implemented";
        string warning = "Hacker, if detected, becomes <b>STRESSED</b>";
        GameManager.instance.messageScript.AICounterMeasure(msgText, itemText, warning, timerScreamer - 1);
        return isScreamer;
    }

    /// <summary>
    /// Implements Offline AI Countermeasures. Returns true if successful
    /// </summary>
    /// <returns></returns>
    private bool ProcessAIOffline()
    {
        isOffline = true;
        //countermeasure last for a set period of time (2 x if mayor has 'Daemon' trait)
        if (city.mayor.CheckTraitEffect(aiCounterMeasureTimerDoubled) == true)
        { timerOffline = aiCounterMeasureTimer * 2; }
        else { timerOffline = aiCounterMeasureTimer; }
        //Message
        string msgText = string.Format("AI activates OFFLINE Countermeasure ({0} turn duration)", timerOffline - 1);
        string itemText = "AI OFFLINE countermeasures implemented";
        string warning = "AI cannot be hacked";
        GameManager.instance.messageScript.AICounterMeasure(msgText, itemText, warning, timerOffline);
        return isOffline;
    }

    /// <summary>
    /// Implements increase AI Security Protocol level countermeasure. Returns true if successful.
    /// NOTE: calling method has already checked that there is scope for an increase
    /// </summary>
    /// <returns></returns>
    private bool ProcessAIProtocol()
    {
        aiSecurityProtocolLevel++;
        //Message
        string msgText = string.Format("AI increases SECURITY PROTOCOL to level {0}", aiSecurityProtocolLevel);
        string itemText = "AI increases SECURITY PROTOCOL";
        string warning = "Increased chance of hacking being detected";
        GameManager.instance.messageScript.AICounterMeasure(msgText, itemText, warning, -1, aiSecurityProtocolLevel);
        return true;
    }

    /// <summary>
    /// returns true if there is space available for a new team (team ratio < team ratio Threshold)
    /// </summary>
    /// <returns></returns>
    public bool CheckNewTeamPossible()
    { return teamRatio < teamRatioThreshold; }


    /// <summary>
    /// Sends a colour formatted data package to AISideTabUI indicating cost and status to hack AI. Ignore renown parameter (it's used by PlayerManager.cs -> Renown Set property)
    /// NOTE: data is dynamic
    /// </summary>
    public void UpdateSideTabData(int renown = 0)
    {
        AISideTabData data = new AISideTabData();
        //ai gear effects?
        Tuple<int, string> results = GetHackingCost();
        hackingModifiedCost = results.Item1;
        string gearEffect = results.Item2;
        switch (GameManager.instance.playerScript.status)
        {
            case ActorStatus.Active:
                int playerRenown = renown;
                if (playerRenown == 0)
                { playerRenown = GameManager.instance.playerScript.Renown; }
                if (isRebooting == true)
                {
                    //AI Security System rebooting, Hacking is unavailable
                    data.topText = string.Format("{0}A.I{1}", colourBad, colourEnd);
                    data.bottomText = string.Format("{0}X{1}", colourBad, colourEnd);
                    data.status = HackingStatus.Rebooting;
                    data.tooltipMain = string.Format("The AI is {0}REBOOTING{1} it's Security systems and {2}cannot be hacked{3}",
                        colourBad, colourEnd, colourNeutral, colourEnd);
                }
                else
                { 
                    //AI online and available to be hacked
                    if (isOffline == false)
                    {
                        data.topText = "A.I";
                        data.status = HackingStatus.Possible;
                        StringBuilder builder = new StringBuilder();
                        //renown to spare -> Green
                        if (playerRenown > hackingModifiedCost)
                        {
                            data.bottomText = string.Format("{0}{1}{2}", colourGood, hackingModifiedCost, colourEnd);
                            builder.AppendFormat("You can hack the AI for {0}{1}{2}{3} Renown{4}{5}", "\n", colourGood, hackingModifiedCost, colourEnd, "\n", gearEffect);
                        }
                        //just enough renown -> Yellow
                        else if (playerRenown == hackingModifiedCost)
                        {
                            data.bottomText = string.Format("{0}{1}{2}", colourNeutral, hackingModifiedCost, colourEnd);
                            builder.AppendFormat("You can hack the AI for {0}{1}{2}{3} Renown{4}{5}", "\n", colourNeutral, hackingModifiedCost, colourEnd, "\n", gearEffect);
                        }
                        else
                        {
                            //insufficient renown -> Greyed out
                            data.topText = string.Format("{0}A.I{1}", colourGrey, colourEnd);
                            data.bottomText = string.Format("{0}{1}{2}", colourGrey, hackingModifiedCost, colourEnd);
                            data.status = HackingStatus.InsufficientRenown;
                            builder.AppendFormat("You can hack the AI for {0}{1}{2}{3} Renown{4}{5}", "\n", colourBad, hackingModifiedCost, colourEnd, "\n", gearEffect);
                        }
                        //AI countermeasures
                        if (isTraceBack == true || isScreamer == true)
                        {
                            builder.AppendFormat("{0}o o o{1}{2}{3}AI Countermeasures{4}", colourGrey, colourEnd, "\n", colourAlert, colourEnd);
                            if (isTraceBack == true)
                            {
                                if (CheckAIGearEffectPresent(traceBackEffectText) == true)
                                {
                                    //show as grey if masked and indicate why
                                    builder.AppendFormat("{0}{1}{2}{3}", "\n", colourGrey, traceBackFormattedText, colourEnd);
                                    Gear gear = GameManager.instance.playerScript.GetAIGear(traceBackEffectText);
                                    if (gear != null)
                                    { builder.AppendFormat("{0}{1}{2}{3}{4} defeats TraceBack{5}", "\n", colourNeutral, gear.name, colourEnd, colourGood, colourEnd); }
                                    else { Debug.LogWarning("Invalid gear (Null) for TraceBack"); }
                                }
                                else { builder.AppendFormat("{0}{1}{2}{3}", "\n", colourBad, traceBackFormattedText, colourEnd); }
                                builder.AppendFormat("{0}duration {1}{2}{3} turn{4}", "\n", colourNeutral, timerTraceBack, colourEnd, timerTraceBack != 1 ? "s" : "");
                            }
                            if (isScreamer == true)
                            {
                                if (CheckAIGearEffectPresent(screamerEffectText) == true)
                                {
                                    //show as grey if masked and indicate why
                                    builder.AppendFormat("{0}{1}{2}{3}", "\n", colourGrey, screamerFormattedText, colourEnd);
                                    Gear gear = GameManager.instance.playerScript.GetAIGear(screamerEffectText);
                                    if (gear != null)
                                    { builder.AppendFormat("{0}{1}{2}{3}{4} defeats Screamer{5}", "\n", colourNeutral, gear.name, colourEnd, colourGood, colourEnd); }
                                    else { Debug.LogWarning("Invalid gear (Null) for Screamer"); }
                                }
                                else { builder.AppendFormat("{0}{1}{2}{3}", "\n", colourBad, screamerFormattedText, colourEnd); }
                                builder.AppendFormat("{0}duration {1}{2}{3} turn{4}", "\n", colourNeutral, timerScreamer, colourEnd, timerScreamer != 1 ? "s" : "");
                            }
                        }
                        //finalise main tooltip text
                        data.tooltipMain = builder.ToString();
                    }
                    else
                    {
                        //AI Offline and can't be hacked
                        data.topText = string.Format("{0}A.I{1}", colourGrey, colourEnd);
                        data.bottomText = string.Format("{0}X{1}", colourGrey, colourEnd);
                        data.status = HackingStatus.Offline;
                        StringBuilder builder = new StringBuilder();
                        builder.AppendFormat("The AI is {0}ISOLATED{1} from external access{2}{3}Cannot be hacked{4}",
                            colourBad, colourEnd, "\n", colourNeutral, colourEnd);
                        builder.AppendFormat("{0}Duration {1}{2}{3} turn{4}", "\n", colourNeutral, timerOffline, colourEnd, timerOffline != 1 ? "s" : "");
                        data.tooltipMain = builder.ToString();
                    }
                }
                break;
            default:
                //player indisposed -> Greyed out
                data.topText = string.Format("{0}A.I{1}", colourGrey, colourEnd);
                data.bottomText = string.Format("{0}{1}{2}", colourGrey, hackingModifiedCost, colourEnd);
                data.status = HackingStatus.Indisposed;
                data.tooltipMain = string.Format("You are {0}not in a position to Hack the AI{1} at present due to your {2}current circumstances{3}", colourBad, colourEnd,
                    colourNeutral, colourEnd);
                break;
        }
        data.tooltipDetails = string.Format("Hacking the AI does{0}{1}NOT{2}{3}cost an Action", "\n", colourNeutral, colourEnd, "\n");
        //send data package
        EventManager.instance.PostNotification(EventType.AISendSideData, this, data, "AIManager.cs -> UpdateSideTabData");
    }

    /// <summary>
    /// reboots AI security system and prevents hacking until reboot is over
    /// </summary>
    public void RebootCommence()
    {
        isRebooting = true;
        rebootTimer = hackingRebootTimer;
        //update side tab
        UpdateSideTabData();
        //message
        Debug.LogFormat("[Aim] AIManager.cs -> RebootCommence: rebootTimer set to {0}{1}", rebootTimer, "\n");
        GameManager.instance.messageScript.AIReboot("AI commences Rebooting Security Systems", hackingCurrentCost, rebootTimer - 1);
        //reset any active ai countermeasures
        if (isTraceBack == true)
        { CancelTraceBack(); }
        if (isScreamer == true)
        { CancelScreamer(); }
        if (isOffline == true)
        { CancelOffline(); }
    }

    /// <summary>
    /// Reboot of Security system is complete, hacking is now possible
    /// </summary>
    private void RebootComplete()
    {
        isRebooting = false;
        rebootTimer = 0;
        hackingAttemptsDetected = 0;
        hackingAttemptsReboot = 0;
        //increment hacking cost
        hackingCurrentCost += hackingIncrement;
        //reset Alert status to low
        aiAlertStatus = Priority.Low;
        //message
        GameManager.instance.messageScript.AIReboot("AI completes Rebooting Security Systems", hackingCurrentCost, rebootTimer);
    }

    /// <summary>
    /// Checks reboot timer each turn, decrements if necessary and handles admin
    /// </summary>
    private void UpdateRebootStatus()
    {
        if (rebootTimer > 0)
        {
            rebootTimer--;
            if (rebootTimer <= 0)
            { RebootComplete(); }
            Debug.LogFormat("[Aim] AIManager.cs -> UpdateRebootStatus: rebootTimer now {0}{1}", rebootTimer, "\n");
        }
    }

    /// <summary>
    /// Decrement any active countermeasure timers and 
    /// </summary>
    private void UpdateCounterMeasureTimers()
    {
        string msgText;
        //TraceBack
        if (timerTraceBack > 0)
        {
            timerTraceBack--;
            if (timerTraceBack == 0)
            { CancelTraceBack(); }
            else { Debug.LogFormat("[Aim] -> UpdateCounterMeasureTimers: timerTraceBack now {0}{1}", timerTraceBack, "\n"); }
        }
        //Screamer
        if (timerScreamer > 0)
        {
            timerScreamer--;
            if (timerScreamer == 0)
            { CancelScreamer(); }
            else { Debug.LogFormat("[Aim] -> UpdateCounterMeasureTimers: timerScreamer now {0}{1}", timerScreamer, "\n"); }
        }
        //Offline
        if (timerOffline > 0)
        {
            timerOffline--;
            if (timerOffline == 0)
            { CancelOffline(); }
            else { Debug.LogFormat("[Aim] -> UpdateCounterMeasureTimers: timerOffline now {0}{1}", timerOffline, "\n"); }
        }
        //Policy
        if (timerPolicy > 0)
        {
            timerPolicy--;
            if (timerPolicy == 0)
            { CancelPolicy(); }
            else
            {
                if (string.IsNullOrEmpty(policyName) == false)
                {
                    int aiDecID = GameManager.instance.dataScript.GetAIDecisionID(policyName);
                    if (aiDecID > -1)
                    {
                        DecisionAI policy = GameManager.instance.dataScript.GetAIDecision(aiDecID);
                        if (policy != null)
                        {
                            //ongoing effect message
                            msgText = string.Format("{0} policy in force (District Crisis chance -{1} %), {2} turn{3} to go", policyName, policyEffectCrisis, timerPolicy, timerPolicy != 1 ? "s" : "");
                            string itemText = string.Format("City Wide {0} policy in force", policyName);
                            string topText = policy.descriptor;
                            string middleText = string.Format("{0}{1}{2}", colourAlert, policy.tooltipDescriptor, colourEnd);
                            string bottomText = string.Format("<b>Duration {0}{1} turn{2}</b>{3}", colourNeutral, timerPolicy, timerPolicy != 1 ? "s" : "", colourEnd);
                            GameManager.instance.messageScript.DecisionOngoingEffect(msgText, itemText, topText, middleText, bottomText, aiDecID);
                        }
                        else { Debug.LogErrorFormat("Invalid policy (Null) for aiDecID {0}", aiDecID); }
                    }
                    else { Debug.LogWarningFormat("Invalid aiDecID (-1) for policyName \"{0}\"", policyName); }
                }
                else { Debug.LogError("Invalid policyName (Null)"); }
            }
        }
        //Handout
        if (timerHandout > 0)
        { timerHandout--; }
    }

    /// <summary>
    /// called everytime Player hacks AI. Updates Alert Status and checks for reboots. Updates data and sends package to AIDisplay. Returns true if player detected
    /// called from AIDisplayUI.cs -> OpenAIDisplayPanel
    /// NOTE: data is dynamic
    /// </summary>
    public bool UpdateHackingStatus()
    {
        bool isDetected = false;
        bool isTraceBackMasker = false;
        bool isScreamerMasker = false;
        int traceBackDelay = 0;
        string text, traceBackGearName;
        string screamerGearName = "Screamer Gear";
        //ignore if Player has already hacked AI this turn
        if (isHacked == false)
        {
            string colourStatus = colourNormal;
            AIHackingData data = new AIHackingData();
            //increment number of hacking attempts
            hackingAttemptsTotal++;
            hackingAttemptsReboot++;
            isHacked = true;
            //does AI Alert Status increase?
            int rnd = Random.Range(0, 100);
            //int chance = hackingDetectBaseChance;
            Tuple<int, string> results = GetChanceOfDetection(true);
            int chance = results.Item1;
            if (CheckAIGearEffectPresent(invisibileHackingEffectText) == false)
            {
                //tooltip
                data.tooltipHeader = results.Item2;
                //
                // - - - Detection Check - - -
                //
                if (rnd < chance)
                {
                    //AI DETECTS hacking attempt
                    Debug.LogFormat("[Rnd] AIManager.cs -> UpdateHackingStatus: Hacking attempt DETECTED, need < {0}, rolled {1}{2}", chance, rnd, "\n");
                    Debug.LogFormat("[Ply] AIManager.cs -> UpdateHackingStatus: Player HACKS AI and is DETECTED{0}", "\n");
                    GameManager.instance.messageScript.GeneralRandom("Hacking attempt DETECTED", "Detection", chance, rnd, true);
                    isDetected = true;
                    hackingAttemptsDetected++;
                    //increase alert status
                    switch (aiAlertStatus)
                    {
                        case Priority.Low:
                            aiAlertStatus = Priority.Medium;
                            colourStatus = colourNeutral;
                            traceBackDelay = 2;
                            //Message
                            string textLow = string.Format("AI detects hacking activity. AlertStatus now {0}", aiAlertStatus);
                            GameManager.instance.messageScript.AIAlertStatus(textLow, chance, rnd);
                            break;
                        case Priority.Medium:
                            aiAlertStatus = Priority.High;
                            colourStatus = colourBad;
                            traceBackDelay = 1;
                            //Message
                            string textMedium = string.Format("AI detects hacking activity. AlertStatus now {0}", aiAlertStatus);
                            GameManager.instance.messageScript.AIAlertStatus(textMedium, chance, rnd);
                            break;
                        case Priority.High:
                            //stays High (auto reset to Low by RebootComplete) -> Trigger Reboot 
                            traceBackDelay = 0;
                            colourStatus = colourBad;
                            aiAlertStatus = Priority.Critical;
                            //Message
                            string textHigh = "AI detects hacking activity. AlertStatus now CRITICAL";
                            GameManager.instance.messageScript.AIAlertStatus(textHigh, chance, rnd);
                            RebootCommence();
                            break;
                        default:
                            Debug.LogWarningFormat("Invalid aiAlertStatus \"{0}\"", aiAlertStatus);
                            break;
                    }
                    Debug.LogFormat("[Aim] -> UpdateHackingStatus: AI detects Hacking. Alert Status increased to {0}{1}", aiAlertStatus, "\n");
                    //
                    // - - - Traceback - - -
                    //
                    if (isTraceBack == true)
                    {
                        //gear can negate tracebacks
                        if (CheckAIGearEffectPresent(traceBackEffectText) == false)
                        {
                            //Player loses a level of invisiblity
                            int invisibility = GameManager.instance.playerScript.Invisibility;
                            invisibility -= 1;

                            /*if (invisibility < 0)
                            {
                                //AI knows immediately
                                traceBackDelay = 0;
                                invisibility = 0;
                            }*/

                            GameManager.instance.playerScript.Invisibility = invisibility;
                            //immediate flag activity

                            /*if (traceBackDelay == 0)
                            {
                                immediateFlagResistance = true;
                                text = "AI Hacking attempt Detected (IMMEDIATE TraceBack)";
                                GameManager.instance.messageScript.AIImmediateActivity(text, "AI Traceback", GameManager.instance.nodeScript.nodePlayer, -1);
                            }
                            //AI notification
                            text = "AI Hacking attempt detected (TraceBack)";
                            GameManager.instance.messageScript.AIDetected(text, GameManager.instance.nodeScript.nodePlayer, traceBackDelay);*/

                            //Detected by AITrackback results in immediate notification regardless of circumstances (unless detection negated by Gear)
                            immediateFlagResistance = true;
                            text = "AI Hacking attempt Detected (IMMEDIATE TraceBack)";
                            GameManager.instance.messageScript.AIDetected(text, GameManager.instance.nodeScript.nodePlayer, 0);
                        }
                        else
                        {
                            // AI TraceBack defeated by Hacking Gear
                            isTraceBackMasker = true;
                            Debug.Log("[Aim] -> UpdateHackingStatus: AI TraceBack defeated by Hacking Gear (TraceBack Mask)");
                        }
                    }
                    //
                    // - - - Screamer - - -
                    //
                    if (isScreamer == true)
                    {
                        //gear can negate Screamer
                        if (CheckAIGearEffectPresent(screamerEffectText) == false)
                        {
                            GameManager.instance.playerScript.AddCondition(conditionStressed, "Acquired due to Screamer countermeasures");
                        }
                        else
                        {
                            //screamer masker present
                            isScreamerMasker = true;
                            Gear gear = GameManager.instance.playerScript.GetAIGear(screamerEffectText);
                            if (gear == null)
                            {
                                screamerGearName = "Screamer Gear";
                                Debug.LogWarning("Invalid gear (Null) for screamerEffectText");
                            }
                            else
                            {
                                screamerGearName = gear.name;
                                Debug.Log("[Aim] -> UpdateHackingStatus: AI Screamer defeated by Hacking Gear (Screamer Mask)");
                                GameManager.instance.gearScript.SetGearUsed(gear, "defeat AI Screamer hacking countermeasure");
                            }
                            
                        }
                    }
                }
                // NOT Detected
                else
                {
                    Debug.LogFormat("[Rnd] AIManager.cs -> UpdateHackingStatus: Hacking attempt Undetected, need < {0}, rolled {1}{2}", chance, rnd, "\n");
                    Debug.LogFormat("[Ply] AIManager.cs -> UpdateHackingStatus: Player HACKS AI and is NOT Detected{0}", "\n");
                    GameManager.instance.messageScript.GeneralRandom("Hacking attempt Undetected", "Detection", chance, rnd, true);
                    //no change to status
                    switch (aiAlertStatus)
                    {
                        case Priority.Low: colourStatus = colourGood; break;
                        case Priority.Medium: colourStatus = colourNeutral; break;
                        case Priority.High: colourStatus = colourBad; break;
                        default:
                            Debug.LogWarningFormat("Invalid aiAlertStatus \"{0}\"", aiAlertStatus);
                            break;
                    }
                }
            }
            else
            {
                //Invisible Hacking gear present -> Player can't be detected -> no change to status
                Gear gear = GameManager.instance.playerScript.GetAIGear(invisibileHackingEffectText);
                if (gear != null)
                {
                    data.tooltipHeader = string.Format("There is {0}NO{1} chance of being {2}Detected{3} due to {4}{5}{6} gear", colourGood, colourEnd, colourBad, colourEnd,
                        colourNeutral, gear.name, colourEnd);
                }
                else
                {
                    Debug.LogWarning("Invalid gear (Null) from invisibleHackingEffectText");
                    data.tooltipHeader = string.Format("There is {0}NO{1} chance of being {2}Detected{3} due to Gear", colourGood, colourEnd, colourBad, colourEnd);
                }
                Debug.LogFormat("[Aim] -> UpdateHackingStatus: Hacking attempt INVISIBLE due to gear{0}", "\n");
                switch (aiAlertStatus)
                {
                    case Priority.Low: colourStatus = colourGood; break;
                    case Priority.Medium: colourStatus = colourNeutral; break;
                    case Priority.High: colourStatus = colourBad; break;
                    default:
                        Debug.LogWarningFormat("Invalid aiAlertStatus \"{0}\"", aiAlertStatus);
                        break;
                }
            }
            //deduct renown cost for hacking
            UpdateHackingCost(isDetected, hackingAttemptsDetected, hackingAttemptsTotal);

            //
            // - - - Tooltip - - - 
            //
            if (isDetected == true)
            {
                //ai has detected a hacking attempt
                if (isTraceBack == true)
                {
                    if (isTraceBackMasker == false)
                    {
                        //TRACEBACK
                        if (traceBackDelay > 0)
                        {
                            StringBuilder builder = new StringBuilder();
                            builder.AppendFormat("{0}<size=110%>DETECTED</size>{1}{2}{3}{4}{5}{6}", colourBad, colourEnd, "\n", colourBad, traceBackFormattedText, colourEnd, "\n");
                            if (isScreamer == true)
                            {
                                builder.AppendFormat("{0}{1}{2}{3}", colourBad, screamerFormattedText, colourEnd, "\n");
                                if (isScreamerMasker == false)
                                { builder.AppendFormat("{0}Player gains{1}{2}{3}STRESSED{4}{5} Condition{6}{7}", colourBad, colourEnd, "\n", 
                                    colourNeutral, colourEnd, colourBad, colourEnd, "\n"); }
                                else
                                { builder.AppendFormat("{0}{1}{2}{3} defeats Screamer{4}{5}", colourNeutral, screamerGearName, colourEnd, colourGood, colourEnd, "\n"); }
                            }
                            builder.AppendFormat("{0}Authority will know your location in {1}{2}{3}{4}{5} turn{6}{7}{8}", colourAlert, colourEnd, colourNeutral, traceBackDelay,
                                colourEnd, colourAlert, traceBackDelay != 1 ? "s" : "", colourEnd, "\n");
                            builder.AppendFormat("{0}Player Invisibility -1{1}", colourBad, colourEnd);
                            data.tooltipMain = builder.ToString();
                        }
                        else
                        {
                            StringBuilder builder = new StringBuilder();
                            builder.AppendFormat("{0}<size=110%>DETECTED</size>{1}{2}{3}{4}{5}{6}", colourBad, colourEnd, "\n", colourBad, traceBackFormattedText, colourEnd, "\n");
                            if (isScreamer == true)
                            {
                                builder.AppendFormat("{0}{1}{2}{3}", colourBad, screamerFormattedText, colourEnd, "\n");
                                if (isScreamerMasker == false)
                                { builder.AppendFormat("{0}Player gains{1}{2}{3}STRESSED{4}{5} Condition{6}{7}", colourBad, colourEnd, "\n",
                                      colourNeutral, colourEnd, colourBad, colourEnd, "\n"); }
                                else
                                { builder.AppendFormat("{0}{1}{2}{3} defeats Screamer{4}{5}", colourNeutral, screamerGearName, colourEnd, colourGood, colourEnd, "\n"); }
                            }
                            builder.AppendFormat("{0}Authority will know your location {1}{2}IMMEDIATELY{3}{4}", colourAlert, colourEnd, colourBad, colourEnd, "\n");
                            builder.AppendFormat("{0}Player Invisibility -1{1}", colourBad, colourEnd);
                            data.tooltipMain = builder.ToString();
                        }
                    }
                    else
                    {
                        //TraceBack Mask
                        Gear gear = GameManager.instance.playerScript.GetAIGear(traceBackEffectText);
                        if (gear == null)
                        {
                            traceBackGearName = "Hacking Gear";
                            Debug.LogWarning("Invalid gear (Null) for traceBackEffectText");
                        }
                        else
                        {
                            //traceback masker present and used
                            traceBackGearName = gear.name;
                            GameManager.instance.gearScript.SetGearUsed(gear, "defeat AI TraceBack countermeasure");
                        }
                        StringBuilder builder = new StringBuilder();
                        builder.AppendFormat("{0}<size=110%>DETECTED</size>{1}{2}{3}{4}{5}{6}", colourBad, colourEnd, "\n", colourBad, traceBackFormattedText, colourEnd, "\n");
                        if (isScreamer == true)
                        {
                            builder.AppendFormat("{0}{1}{2}{3}", colourBad, screamerFormattedText, colourEnd, "\n");
                            if (isScreamerMasker == false)
                            { builder.AppendFormat("{0}Player gains{1}{2}{3}STRESSED{4}{5} Condition{6}{7}", colourBad, colourEnd, "\n",
                                colourNeutral, colourEnd, colourBad, colourEnd, "\n"); }
                            else
                            { builder.AppendFormat("{0}{1}{2}{3} defeats Screamer{4}{5}", colourNeutral, screamerGearName, colourEnd, colourGood, colourEnd, "\n"); }
                        }
                        builder.AppendFormat("{0}{1}{2}{3} defeats TraceBack{4}{5}", colourNeutral, traceBackGearName, colourEnd, colourGood, colourEnd, "\n");
                        builder.AppendFormat("No Loss of Invisibility and your position is not revealed");
                        data.tooltipMain = builder.ToString();
                    }
                }
                else
                {
                    //Normal detection, no traceback, possible screamer
                    StringBuilder builder = new StringBuilder();
                    builder.AppendFormat("{0}<size=110%>DETECTED</size>{1}{2}", colourBad, colourEnd, "\n");
                    if (isScreamer == true)
                    {
                        builder.AppendFormat("{0}{1}{2}{3}", colourBad, screamerFormattedText, colourEnd, "\n");
                        if (isScreamerMasker == false)
                        { builder.AppendFormat("{0}Player gains{1}{2}{3}STRESSED{4}{5} Condition{6}{7}", colourBad, colourEnd, "\n",
                            colourNeutral, colourEnd, colourBad, colourEnd, "\n"); }
                        else
                        { builder.AppendFormat("{0}{1}{2}{3} defeats Screamer{4}{5}", colourNeutral, screamerGearName, colourEnd, colourGood, colourEnd, "\n"); }
                    }
                    builder.AppendFormat("AI Alert Status has increased to {0}{1}{2}", colourStatus, aiAlertStatus, colourEnd);
                    data.tooltipMain = builder.ToString();
                }
            }
            else
            {
                //no detection but change message if just about to trigger a reboot
                if (aiAlertStatus == Priority.High)
                {
                    data.tooltipMain = string.Format("{0}The AI will {1}{2}REBOOT{3}{4} it's Security Systems {5}{6}{7}next time{8}{9}{10} it detects a hacking attempt{11}", colourAlert, colourEnd,
                        colourBad, colourEnd, colourAlert, colourEnd, "\n", colourNeutral, colourEnd, "\n", colourAlert, colourEnd);
                }
                else
                {
                    data.tooltipMain = string.Format("{0}Alert Status increases whenever hacking detected. AI will {1}{2}Reboot{3}{4} at status {5}{6}Critical{7}",
                        colourAlert, colourEnd, colourBad, colourEnd, colourAlert, colourEnd, colourBad, colourEnd);
                }
            }
            data.tooltipDetails = string.Format("{0}The AI has detected{1}{2}{3}{4} hacking attempt{5}{6}{7}{8}since its last Reboot{9}", colourNormal, colourEnd, "\n",
                colourNeutral, hackingAttemptsDetected,  hackingAttemptsDetected != 1 ? "s" : "", colourEnd, "\n", colourNormal, colourEnd);
            //
            // - - -admin - - - 
            //
            Debug.LogFormat("[Aim] -> UpdateHackingStatus: hackingAttemptsTotal now {0}{1}", hackingAttemptsTotal, "\n");
            Debug.LogFormat("[Aim] -> UpdateHackingStatus: AI Alert Status {0}{1}", aiAlertStatus, "\n");
            //data package
            data.hackingStatus = string.Format("{0} Hacking Attempt{1}{2}AI Alert Status {3}{4}{5}", hackingAttemptsReboot,
                hackingAttemptsReboot != 1 ? "s" : "", "\n", colourStatus, aiAlertStatus, colourEnd);
            //send data package to AIDisplayUI
            EventManager.instance.PostNotification(EventType.AISendHackingData, this, data, "AIManager.cs -> UpdateHackingStatus");
        }
        return isDetected;
    }

    /// <summary>
    /// method run during AIEndFinalTurn sequence to ensure bottom tab and tooltip start next turn with a clean slate
    /// </summary>
    public void UpdateBottomTabData()
    {
        AIHackingData data = new AIHackingData();
        string colourStatus = colourNormal;
        Tuple<int, string> results = GetChanceOfDetection();
        int chance = results.Item1;
        switch (aiAlertStatus)
        {
            case Priority.Low: colourStatus = colourGood; break;
            case Priority.Medium: colourStatus = colourNeutral; break;
            case Priority.High: colourStatus = colourBad; break;
            case Priority.Critical: colourStatus = colourBad; break;
            default:
                Debug.LogWarningFormat("Invalid aiAlertStatus \"{0}\"", aiAlertStatus);
                break;
        }
        //data package
        data.hackingStatus = string.Format("{0} Hacking Attempt{1}{2}AI Alert Status {3}{4}{5}", hackingAttemptsReboot,
            hackingAttemptsReboot != 1 ? "s" : "", "\n", colourStatus, aiAlertStatus, colourEnd);
        //tooltip
        data.tooltipHeader = results.Item2;
        if (aiAlertStatus == Priority.High)
        {
            data.tooltipMain = string.Format("{0}The AI will {1}{2}REBOOT{3}{4} it's Security Systems {5}{6}{7}next time{8}{9}{10} it detects a hacking attempt{11}", colourAlert, colourEnd,
                colourBad, colourEnd, colourAlert, colourEnd, "\n", colourNeutral, colourEnd, "\n", colourAlert, colourEnd);
        }
        else
        {
            //change message if just about to trigger a reboot
            data.tooltipMain = string.Format("{0}Alert Status increases whenever hacking detected. AI will {1}{2}Reboot{3}{4} at status {5}{6}Critical{7}",
                colourAlert, colourEnd, colourBad, colourEnd, colourAlert, colourEnd, colourBad, colourEnd);
        }
        data.tooltipDetails = string.Format("{0}The AI has detected{1}{2}{3}{4} hacking attempt{5}{6}{7}{8}since its last Reboot{9}", colourNormal, colourEnd, "\n",
            colourNeutral, hackingAttemptsDetected, hackingAttemptsDetected != 1 ? "s" : "", colourEnd, "\n", colourNormal, colourEnd);
        //send data package to AIDisplayUI
        EventManager.instance.PostNotification(EventType.AISendHackingData, this, data, "AIManager.cs -> UpdateHackingStatus");
    }

    /// <summary>
    /// Player is hacking AI -> pays cost in renown, called from AIDisplayUI.cs -> OpenAIDisplayPanel
    /// </summary>
    public void UpdateHackingCost(bool isDetected, int attemptsDetected, int attemptsTotal)
    {
        //deduct cost
        int renown = GameManager.instance.playerScript.Renown;
        //ai gear effects?
        Tuple <int, string> results = GetHackingCost(true);
        hackingModifiedCost = results.Item1;
        string gearEffect = results.Item2;
        //update Player renown
        renown -= hackingModifiedCost;
        Debug.Assert(renown >= 0, "Invalid Renown cost (below zero)");
        GameManager.instance.playerScript.Renown = renown;
        //message
        GameManager.instance.messageScript.AIHacked("AI has been hacked", hackingModifiedCost, isDetected, attemptsDetected, attemptsTotal);
    }



    /// <summary>
    /// run this method whenever player adds/removes any AI capable hacking gear in order to keep AI Side tab and AI Display UI in synch with actual hacking cost
    /// </summary>
    public void UpdateAIGearStatus()
    {
        //no update during first turn (player shouldn't be able to open side tab yet).
        if (GameManager.instance.turnScript.Turn > 0)
        {
            UpdatePlayerHackingLists();
            UpdateSideTabData();
            UpdateTaskDisplayData();
        }
    }


    /// <summary>
    /// gets all gear AI hacking related effects. Run every time player hacks AI for first time each turn. Run at time of hacking so is Dynamic
    /// </summary>
    public void UpdatePlayerHackingLists()
    {
        List<int> tempList = GameManager.instance.playerScript.CheckAIGearPresent();
        listOfPlayerEffects.Clear();
        listOfPlayerEffectDescriptors.Clear();
        if (tempList != null)
        {
            //AI hacking gear present
            for (int i = 0; i < tempList.Count; i++)
            {
                //gear is already checked to be hacking and have AI effects by playerManager.cs -> CheckAIGearPresent
                Gear gear = GameManager.instance.dataScript.GetGear(tempList[i]);
                if (gear != null)
                {
                    if (gear.aiHackingEffect != null)
                    {

                        listOfPlayerEffects.Add(gear.aiHackingEffect.name);
                        switch (gear.rarity.name)
                        {
                            case "Common":
                            case "Rare":
                                listOfPlayerEffectDescriptors.Add(string.Format("{0} ({1}{2}{3})", gear.aiHackingEffect.description, colourNeutral, gear.name, colourEnd));
                                break;
                            case "Unique":
                                listOfPlayerEffectDescriptors.Add(string.Format("{0} ({1}{2}{3})", gear.aiHackingEffect.description, colourGood, gear.name, colourEnd));
                                break;
                            default:
                                Debug.LogWarningFormat("Invalid gear.rarity.name \"{0}\"", gear.rarity.name);
                                break;
                        }

                    }
                }
                else { Debug.LogWarningFormat("Invalid gear (Null) for gearID {0}", tempList[i]); }
            }
        }
    }

    /// <summary>
    /// returns a colour formatted string showing any gear related effects to AI hacking
    /// </summary>
    /// <returns></returns>
    public string UpdateGearText()
    {
        StringBuilder builder = new StringBuilder();
        int numOfEffects = listOfPlayerEffectDescriptors.Count;
        if (numOfEffects == 0)
        { builder.AppendFormat("{0}<size=80%>Gear Effects{1}None</size>{2}", colourGrey, "\n", colourEnd); }
        else
        {
            builder.AppendFormat("{0}<size=90%>Gear Effects</size>{1}", colourAlert, colourEnd);
            foreach(string text in listOfPlayerEffectDescriptors)
            { builder.AppendFormat("{0}{1}", "\n", text); }
        }
        return builder.ToString();
    }

    //
    // - - - SubMethods - - -
    //

    /// <summary>
    /// returns hacking cost Tuple and a string descriptor of cost at present taking into account gear effects. String is "" and cost is currentHackingCost if there are no gear effects present
    /// isGearUsed set true only when you want to log gear use, ignore otherwise
    /// </summary>
    /// <returns></returns>
    public Tuple<int, string> GetHackingCost(bool logGearUse = false)
    {
        int tempCost = 0;
        string gearEffect = "";
        //deduct cost
        int renown = GameManager.instance.playerScript.Renown;
        string textGear;
        //ai gear effects?
        if (CheckAIGearEffectPresent(cheapHackingEffectText) == true && CheckAIGearEffectPresent(freeHackingEffectText) == false)
        {
            Gear gear = GameManager.instance.playerScript.GetAIGear(cheapHackingEffectText);
            if (gear != null)
            {
                textGear = gear.name;
                if (logGearUse == true)
                { GameManager.instance.gearScript.SetGearUsed(gear, "provide half cost AI Hacking"); }
            }
            else
            {
                textGear = "Hacking";
                Debug.LogWarningFormat("Invalid gear (Null) from {0}", cheapHackingEffectText);
            }
            //tempCost = (int)Math.Floor(hackingCurrentCost / 2.0f);
            tempCost = hackingCurrentCost / 2;
            gearEffect = string.Format("{0} (Half cost due to {1}{2}{3}{4} {5}gear){6}{7}", colourAlert, colourEnd, colourNeutral, textGear, colourEnd, colourAlert, colourEnd, "\n");
        }
        //Free Hacking will override Cheap Hacking if both are present
        if (CheckAIGearEffectPresent(freeHackingEffectText) == true)
        {
            Gear gear = GameManager.instance.playerScript.GetAIGear(freeHackingEffectText);
            if (gear != null)
            {
                textGear = gear.name;
                if (logGearUse == true)
                { GameManager.instance.gearScript.SetGearUsed(gear, "provide free AI Hacking"); }
            }
            else
            {
                textGear = "Hacking";
                Debug.LogWarningFormat("Invalid gear (Null) from {0}", freeHackingEffectText);
            }
            tempCost = 0;
            gearEffect = string.Format("{0} (No cost due to {1}{2}{3}{4} {5}gear){6}{7}", colourAlert, colourEnd, colourNeutral, textGear, colourEnd, colourAlert, colourEnd, "\n");
        }
        if (gearEffect.Length == 0)
        { tempCost = hackingCurrentCost; }
        return new Tuple<int, string>(tempCost, gearEffect);
    }

    /// <summary>
    /// subMethod to return modified chance (%) of any given hacking attempt being detected as well as a colour formatted tooltip string detailing a breakdown of chance factors
    /// isGearUsed set to true if you want to log gear effects, otherwise ignore
    /// </summary>
    /// <returns></returns>
    private Tuple<int, string> GetChanceOfDetection(bool logGearUse = false)
    {
        //base chance
        int chance = hackingDetectBaseChance;
        string detectText = "";
        string gearName;
        Gear gear = GameManager.instance.playerScript.GetAIGear(invisibileHackingEffectText);
        if (gear == null)
        {
            StringBuilder builder = new StringBuilder();
            //Mayor modifier -> traits
            string textMayor = "";
            if (city.mayor.CheckTraitEffect(aiDetectionChanceHigher) == true)
            {
                chance += hackingTraitDetectionFactor;
                textMayor = string.Format("{0}<size=95%>{1}Mayor +{2}{3}</size>", "\n", colourBad, hackingTraitDetectionFactor * 0.1, colourEnd);
            }
            else if (city.mayor.CheckTraitEffect(aiDetectionChanceLower) == true)
            {
                chance -= hackingTraitDetectionFactor;
                textMayor = string.Format("{0}<size=95%>{1}Mayor -{2}{3}</size>", "\n", colourGood, hackingTraitDetectionFactor * 0.1, colourEnd);
            }
            //AI security protocols 
            string textProtocols = "";
            if (aiSecurityProtocolLevel > 0)
            {
                int protocolEffect = aiSecurityProtocolLevel * hackingSecurityProtocolFactor;
                chance += protocolEffect;
                textProtocols = string.Format("{0}<size=95%>{1}AI Protocol +{2}{3}</size>", "\n", colourBad, protocolEffect * 0.1, colourEnd);
            }
            //Gear modifiers
            string textGear = "";
            Gear gearDetect = GameManager.instance.playerScript.GetAIGear(lowerDetectionEffectText);
            if (gearDetect != null)
            {
                chance -= hackingLowDetectionEffect;
                textGear = string.Format("{0}<size=95%>{1}Gear -{2}{3}</size>", "\n", colourGood, hackingLowDetectionEffect * 0.1, colourEnd);
                if (logGearUse == true)
                { GameManager.instance.gearScript.SetGearUsed(gearDetect, "lower the chance of being Detected while Hacking"); }
            }
            //Player Stressed
            string textStressed = "";
            stressedCondition = GameManager.instance.dataScript.GetCondition("STRESSED");
            if (GameManager.instance.playerScript.CheckConditionPresent(stressedCondition) == true)
            {
                chance += hackingStressedDetectionEffect;
                textStressed = string.Format("{0}<size=95%>{1}STRESSED +{2}{3}</size>", "\n", colourBad, hackingStressedDetectionEffect, colourEnd);
            }
            //keep within acceptable parameters
            chance = Mathf.Clamp(chance, 0, 100);
            //put together tooltip string
            builder.AppendFormat("{0}Chance of Being Detected{1}", colourAlert, colourEnd);
            builder.AppendFormat("{0}<size=95%>Base +{1} </size>", "\n", hackingDetectBaseChance * 0.1);
            if (textMayor.Length > 0) { builder.Append(textMayor); }
            if (textProtocols.Length > 0) { builder.Append(textProtocols); }
            if (textGear.Length > 0) { builder.Append(textGear); }
            if (textStressed.Length > 0) { builder.Append(textStressed); }
            builder.AppendFormat("{0}{1}<size=95%>Total +{2}{3}</size>", "\n", colourNeutral, chance * 0.1, colourEnd);
            builder.AppendFormat("{0}{1}<size=110%>DETECTION {2} %</size>{3}", "\n", "<mark=#FFFFFF4D>", chance, "</mark>");
            detectText = builder.ToString();
        }
        else
        {
            gearName = gear.name;
            if (logGearUse == true)
            { GameManager.instance.gearScript.SetGearUsed(gear, "provide Invisibile AI Hacking"); }
            //Invisible Hacking gear present -> Player can't be detected -> no change to status
            detectText = string.Format("There is {0}NO{1} chance of being {2}Detected{3} due to {4}{5}{6} gear", colourGood, colourEnd, colourBad, colourEnd, 
                colourNeutral, gearName, colourEnd);
        }
        return new Tuple<int, string>(chance, detectText);
    }

    /// <summary>
    /// returns tooltip string for AIDisplayUI close tab
    /// </summary>
    /// <returns></returns>
    public string GetCloseAITabTooltip()
    { return string.Format("You can access the AI at {0}NO COST{1} for the rest of {2}this turn{3}", colourGood, colourEnd, colourNeutral, colourEnd); }

    /// <summary>
    /// Increase level by one notch
    /// </summary>
    public void IncreaseAISecurityProtocolLevel()
    { aiSecurityProtocolLevel++; }

    /// <summary>
    /// input an AI effect name, eg. "Invisibile Hacking" and, if present (Player's gear) will return true, false otherwise
    /// </summary>
    /// <param name="effectName"></param>
    /// <returns></returns>
    public bool CheckAIGearEffectPresent(string effectName)
    {
        if (listOfPlayerEffects != null && listOfPlayerEffects.Count > 0)
        { return listOfPlayerEffects.Exists(x => x == effectName); }
        return false;
    }

    /// <summary>
    /// subMethod to cancel TraceBack AI countermeasure
    /// </summary>
    private void CancelTraceBack()
    {
        //remove TraceBack
        isTraceBack = false;
        timerTraceBack = -1;
        //message
        string msgText = "AI TRACEBACK countermeasure expired";
        GameManager.instance.messageScript.AICounterMeasure(msgText, msgText, "<b>Countermeasure expired</b>");
    }

    /// <summary>
    /// subMethod to cancel Screamer AI countermeasure
    /// </summary>
    private void CancelScreamer()
    {
        //remove Screamer
        isScreamer = false;
        timerScreamer = -1;
        //message
        string msgText = "AI SCREAMER countermeasure expired";
        GameManager.instance.messageScript.AICounterMeasure(msgText, msgText, "<b>Countermeasure expired</b>");
    }

    /// <summary>
    /// subMethod to cancel Offline countermeasure
    /// </summary>
    private void CancelOffline()
    {
        //remove Offline
        isOffline = false;
        timerOffline = -1;
        //message
        string msgText = "AI OFFLINE countermeasure expired";
        GameManager.instance.messageScript.AICounterMeasure(msgText, msgText, "<b>Countermeasure expired</b>");
    }

    /// <summary>
    /// subMethod to cancel the current Policy
    /// </summary>
    private void CancelPolicy()
    {
        //update node crisis modifer
        GameManager.instance.nodeScript.crisisPolicyModifier = 0;
        //update loyalty
        int cityLoyalty = GameManager.instance.cityScript.CityLoyalty;
        cityLoyalty += policyEffectLoyalty;
        cityLoyalty = Mathf.Min(GameManager.instance.cityScript.maxCityLoyalty, cityLoyalty);
        GameManager.instance.cityScript.CityLoyalty = cityLoyalty;
        //admin
        string msgText = string.Format("{0} policy is no longer in effect", policyName);
        string cancelText = string.Format("{0} policy has been cancelled", policyName);
        GameManager.instance.messageScript.DecisionGlobal(msgText, msgText, cancelText, -1, 0, policyEffectLoyalty);
        msgText = string.Format("{0} Loyalty has increased by +{1} ({2} policy lifted)", city.name, policyEffectLoyalty, policyName);
        string reasonText = string.Format("{0} policy being cancelled", policyName);
        GameManager.instance.messageScript.CityLoyalty(msgText, reasonText, cityLoyalty, policyEffectLoyalty);
        //reset vars
        isPolicy = false;
        timerPolicy = -1;
        policyName = "Unknown";
        policyEffectCrisis = -1;
        policyEffectLoyalty = -1;
    }

    //
    // - - - Debug - - -
    //

    /// <summary>
    /// Debug display of all Task related data
    /// </summary>
    /// <returns></returns>
    public string DisplayTaskData()
    {
        StringBuilder builder = new StringBuilder();
        //Task lists
        builder.AppendFormat("- Task Allowances{0}", "\n");
        builder.AppendFormat(" {0} Authority tasks per turn ({1}){2}", factionAuthority.maxTaskPerTurn, factionAuthority.name, "\n");
        builder.AppendFormat(" {0} Resistance tasks per turn ({1}){2}{3}", factionResistance.maxTaskPerTurn, factionResistance.name, "\n", "\n");
        builder.AppendFormat("- Resource Pools{0}", "\n");
        builder.AppendFormat(" {0} Authority resources{1}", GameManager.instance.dataScript.CheckAIResourcePool(globalAuthority), "\n");
        builder.AppendFormat(" {0} Resistance resources{1}{2}", GameManager.instance.dataScript.CheckAIResourcePool(globalResistance), "\n", "\n");
        builder.AppendFormat("- Options{0}", "\n");
        builder.AppendFormat(" AI Security Protocol level {0}{1}", aiSecurityProtocolLevel, "\n");
        builder.AppendFormat(" isPolicy -> {0}{1}", isPolicy, "\n");
        builder.AppendFormat(" isOffline -> {0}{1}", isOffline, "\n");
        builder.AppendFormat(" isScreamer -> {0}{1}", isScreamer, "\n");
        builder.AppendFormat(" isTraceBack -> {0}{1}{2}", isTraceBack, "\n", "\n");
        builder.AppendFormat("- listOfTasksFinal{0}", "\n");
        builder.Append(DebugTaskList(listOfTasksFinal));
        builder.AppendFormat("{0}{1}- listOfTasksPotential{2}", "\n", "\n", "\n");
        builder.Append(DebugTaskList(listOfTasksPotential, true));
        return builder.ToString();
    }

    /// <summary>
    /// Debug display method for AI Node data
    /// </summary>
    /// <returns></returns>
    public string DisplayNodeData()
    {
        StringBuilder builder = new StringBuilder();
        //Master Node list
        builder.AppendFormat("- listNodeMaster{0}", "\n");
        builder.Append(DebugDisplayList(listNodeMaster));
        //Sub Node lists
        builder.AppendFormat("{0}{1}- listStabilityCritical{2}", "\n", "\n", "\n");
        builder.Append(DebugDisplayList(listStabilityCritical));
        builder.AppendFormat("{0}- listStabilityNonCritical{1}", "\n", "\n");
        builder.Append(DebugDisplayList(listStabilityNonCritical));
        builder.AppendFormat("{0}- listSecurityCritical{1}", "\n", "\n");
        builder.Append(DebugDisplayList(listSecurityCritical));
        builder.AppendFormat("{0}- listSecurityNonCritical{1}", "\n", "\n");
        builder.Append(DebugDisplayList(listSecurityNonCritical));
        builder.AppendFormat("{0}- listSupportCritical{1}", "\n", "\n"); ;
        builder.Append(DebugDisplayList(listSupportCritical));
        builder.AppendFormat("{0}- listSupportNonCritical{1}", "\n", "\n");
        builder.Append(DebugDisplayList(listSupportNonCritical));
        return builder.ToString();
    }

    /// <summary>
    /// Debug display of all relevant spider AI related data
    /// </summary>
    /// <returns></returns>
    public string DisplaySpiderData()
    {
        StringBuilder builder = new StringBuilder();
        //Task lists
        builder.AppendFormat("- listOfSpiderTask{0}", "\n");
        builder.Append(DebugTaskList(listOfSpiderTasks));
        builder.AppendFormat("{0}- listOfSpiderNodes{1}", "\n", "\n");
        if (listOfSpiderNodes.Count > 0)
        {
            foreach (AINodeData data in listOfSpiderNodes)
            { builder.AppendFormat(" ID {0}, {1}, isPreferred: {2}, Score: {3}{4}", data.nodeID, data.arc.name, data.isPreferred, data.score, "\n"); }
        }
        else { builder.AppendFormat(" No records{0}", "\n"); }
        //Known Targets
        builder.AppendFormat("{0}- listOfTargetsKnown{1}", "\n", "\n");
        if (listOfTargetsKnown.Count > 0)
        {
            foreach (AINodeData data in listOfTargetsKnown)
            { builder.AppendFormat(" ID {0}, {1}, isPreferred: {2}{3}", data.nodeID, data.arc.name, data.isPreferred, "\n"); }
        }
        else { builder.AppendFormat(" No records{0}", "\n"); }
        //Damaged Targets
        builder.AppendFormat("{0}- listOfTargetsDamaged{1}", "\n", "\n");
        if (listOfTargetsDamaged.Count > 0)
        {
            foreach (AINodeData data in listOfTargetsDamaged)
            { builder.AppendFormat(" ID {0}, {1}, isPreferred: {2}{3}", data.nodeID, data.arc.name, data.isPreferred, "\n"); }
        }
        else { builder.AppendFormat(" No records{0}", "\n"); }
        return builder.ToString();
    }

    /// <summary>
    /// Debug display of all relevant erasure AI related data
    /// </summary>
    /// <returns></returns>
    public String DisplayErasureData()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- listOfErasureTasks{0}", "\n");
        builder.Append(DebugTaskList(listOfErasureTasks));
        builder.AppendFormat("{0}- listOfErasureNodes{1}", "\n", "\n");
        if (listOfErasureNodes.Count > 0)
        {
            foreach (AINodeData data in listOfErasureNodes)
            { builder.AppendFormat(" ID {0}, {1}, isPreferred: {2}, Score: {3}{4}", data.nodeID, data.arc.name, data.isPreferred, data.score, "\n"); }
        }
        else { builder.AppendFormat(" No records{0}", "\n"); }
        Queue<AITracker> queueRecentNodes = GameManager.instance.dataScript.GetRecentNodesQueue();
        if (queueRecentNodes != null)
        {
            builder.AppendFormat("{0}- queueRecentNodes{1}", "\n", "\n");
            if (queueRecentNodes.Count > 0)
            {
                //you can enumerate the queue without distrubing it's contents
                foreach (AITracker data in queueRecentNodes)
                { builder.AppendFormat("nodeID {0}, turn {1}{2}", data.data0, data.turn, "\n"); }
            }
            else { builder.AppendFormat(" No records{0}", "\n"); }
        }
        else { Debug.LogWarning("Invalid queueRecentNodes (Null)"); }
        Queue<AITracker> queueRecentConnections = GameManager.instance.dataScript.GetRecentConnectionsQueue();
        if (queueRecentConnections != null)
        {
            builder.AppendFormat("{0}- queueRecentConnections{1}", "\n", "\n");
            if (queueRecentConnections.Count > 0)
            {
                //you can enumerate the queue without distrubing it's contents
                foreach (AITracker data in queueRecentConnections)
                { builder.AppendFormat("connID {0}, turn {1}{2}", data.data0, data.turn, "\n"); }
            }
            else { builder.AppendFormat(" No records{0}", "\n"); }
        }
        else { Debug.LogWarning("Invalid queueRecentConnections (Null)"); }
        builder.AppendFormat("{0}- listOfErasureAILog{1}", "\n", "\n");
        if (listOfErasureAILog.Count > 0)
        {
            foreach (string data in listOfErasureAILog)
            { builder.AppendFormat("{0}{1}", data, "\n"); }
        }
        else { builder.AppendFormat(" No records{0}", "\n"); }
        return builder.ToString();
    }

    public String DisplayDecisionData()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- listOfDecisionTasksCritical{0}", "\n");
        builder.Append(DebugTaskList(listOfDecisionTasksCritical));
        builder.AppendFormat("{0}- listOfDecisionTasksNonCritical{1}", "\n", "\n");
        builder.Append(DebugTaskList(listOfDecisionTasksNonCritical));
        builder.AppendFormat("{0}{1}- ProcessDecisionData{2}", "\n", "\n", "\n");
        builder.AppendFormat(" connectionSecurityRatio -> {0:f2} / {1} {2}{3}", connSecRatio, connectionRatioThreshold, 
            connSecRatio >= connectionRatioThreshold ? "THRESHOLD EXCEEDED" : "", "\n");
        builder.AppendFormat(" teamRatio -> {0:f2} / {1} {2}{3}", teamRatio, teamRatioThreshold, 
            teamRatio >= teamRatioThreshold ? "THRESHOLD EXCEEDED" : "", "\n");
        builder.AppendFormat(" erasureTeamsOnMap -> {0}{1}", erasureTeamsOnMap, "\n");
        builder.AppendFormat(" immediateFlagResistance -> {0}{1}", immediateFlagResistance, "\n");
        builder.AppendFormat(" isInsufficientResources -> {0}{1}", isInsufficientResources, "\n");
        builder.AppendFormat(" numOfUnsuccessfulResourceRequests -> {0}{1}", numOfUnsuccessfulResourceRequests, "\n");
        builder.AppendFormat(" numOfCrisis -> {0}{1}", numOfCrisis, "\n");
        builder.AppendFormat(" timerHandout -> {0}{1}", timerHandout, "\n");
        if (erasureTeamsOnMap > 0 && immediateFlagResistance == true)
        { builder.AppendFormat(" SECURITY MEASURES Available{0}", "\n"); }
        return builder.ToString();
    }

    /// <summary>
    /// debug submethod to display AI Node list data, used by AIManager.cs -> DisplayNodeData
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private string DebugDisplayList(List<AINodeData> list)
    {
        StringBuilder builderList = new StringBuilder();
        if (list != null)
        {
            if (list.Count > 0)
            {
                foreach (AINodeData data in list)
                { builderList.AppendFormat(" ID {0}, {1}, {2}, difference: {3}, current: {4}{5}", data.nodeID, data.arc.name, data.type, data.difference, data.current, "\n"); }
            }
            else { builderList.AppendFormat(" No records{0}", "\n"); }
        }
        else { Debug.LogWarning(string.Format("Invalid list \"{0}\" (Null)", list)); }
        return builderList.ToString();
    }

    /// <summary>
    /// debug submethod to display AI Tasks, used by AIManager.cs -> DisplayNodeData
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private string DebugTaskList(List<AITask> listOfTasks, bool showChance = false)
    {
        StringBuilder builderList = new StringBuilder();
        if (listOfTasks != null)
        {
            if (listOfTasks.Count > 0)
            {
                foreach (AITask task in listOfTasks)
                {
                    switch (task.type)
                    {
                        case AIType.Team:
                            if (showChance == true)
                            { builderList.AppendFormat(" teamID {0} {1}, {2} team, {3} priority, Prob {4} %{5}", task.data0, task.name0, task.name1, task.priority, 
                                task.chance, "\n"); }
                            else
                            { builderList.AppendFormat(" teamID {0} {1}, {2} team, {3} priority{4}", task.data0, task.name0, task.name1, task.priority, "\n"); }
                            break;
                        case AIType.Decision:
                            if (showChance == true)
                            { builderList.AppendFormat(" taskID {0}, aiDecID {1}, Decision \"{2}\", Prob {3} %{4}", task.taskID, task.data0, task.name0, task.chance, "\n"); }
                            else
                            { builderList.AppendFormat(" taskID {0}, aiDecID {1}, Decision \"{2}\", {3}{4}", task.taskID, task.data0, task.name0, task.priority, "\n"); }
                            break;
                        default:
                            builderList.AppendFormat(" Unknown task type \"{0}\"{1}", task.type, "\n");
                            break;
                    }
                }
            }
            else { builderList.AppendFormat("No records{0}", "\n"); }
        }
        else { builderList.AppendFormat("No records{0}", "\n"); }
        return builderList.ToString();
    }

    /*/// <summary>
    /// Show list of player Activity (used for Nemesis AI)
    /// </summary>
    /// <returns></returns>
    public string DebugShowPlayerActivity()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0} -listOfPlayerActivity{1}", "\n", "\n");
        int numOfRecords = listOfPlayerActivity.Count;
        if (numOfRecords > 0)
        {
            for (int i = 0; i < numOfRecords; i++)
            {
                AITracker data = listOfPlayerActivity[i];
                if (data != null)
                { builder.AppendFormat(" turn {0}, nodeID {1}{2}", data.turn, data.data0, "\n"); }
                else { Debug.LogWarning("Invalid AITracker data (Null)"); }
            }
        }
        else { builder.Append(" No records present"); }

        return builder.ToString();
    }*/

    /// <summary>
    /// provide Ongoing effect msg's in the infoApp for any relevant active decisions
    /// NOTE: NodeCrisis policies (max one active) are done via AIManager.cs -> UpdateCounterMeasureTimers
    /// </summary>
    public void ProcessOngoingEffects()
    {
        int aiDecID;
        string text, itemText, topText, middleText, bottomText/* ,colourEffect*/;
        //
        // - - - Network decisions - - -
        //
        //Screamer
        if (isScreamer == true)
        {
            aiDecID = decisionScreamer.aiDecID;
            text = string.Format("AI Network countermeasure {0} in place, duration {1} turn{2}{3}", decisionScreamer.name.ToUpper(), timerScreamer, timerScreamer != 1 ? "s" : "", "\n");
            itemText = string.Format("AI hacking countermeasure {0} in force", decisionScreamer.name.ToUpper());
            topText = decisionScreamer.descriptor;
            middleText = string.Format("{0}{1}{2}", colourAlert, decisionScreamer.tooltipDescriptor, colourEnd);
            bottomText = string.Format("Duration {0}{1} turn{2}{3}", colourNeutral, timerScreamer, timerScreamer != 1 ? "s" : "", colourEnd);
            GameManager.instance.messageScript.DecisionOngoingEffect(text, itemText, topText, middleText, bottomText, aiDecID);
        }
        //Traceback
        if (isTraceBack == true)
        {
            aiDecID = decisionTraceBack.aiDecID;
            text = string.Format("AI Network countermeasure {0} in place, duration {1} turn{2}{3}", decisionTraceBack.name.ToUpper(), timerTraceBack, timerTraceBack != 1 ? "s" : "", "\n");
            itemText = string.Format("AI hacking countermeasure {0} in force", decisionTraceBack.name.ToUpper());
            topText = decisionTraceBack.descriptor;
            middleText = string.Format("{0}{1}{2}", colourAlert, decisionTraceBack.tooltipDescriptor, colourEnd);
            bottomText = string.Format("Duration {0}{1} turn{2}{3}", colourNeutral, timerTraceBack, timerTraceBack != 1 ? "s" : "", colourEnd);
            GameManager.instance.messageScript.DecisionOngoingEffect(text, itemText, topText, middleText, bottomText, aiDecID);
        }
        //Offline
        if (isOffline == true)
        {
            aiDecID = decisionOffline.aiDecID;
            text = string.Format("AI Network countermeasure {0} in place, duration {1} turn{2}{3}", decisionOffline.name.ToUpper(), timerOffline, timerOffline != 1 ? "s" : "", "\n");
            itemText = string.Format("AI hacking countermeasure {0} in force", decisionOffline.name.ToUpper());
            topText = decisionOffline.descriptor;
            middleText = string.Format("{0}{1}{2}", colourAlert, decisionOffline.tooltipDescriptor, colourEnd);
            bottomText = string.Format("Duration {0}{1} turn{2}{3}", colourNeutral, timerOffline, timerOffline != 1 ? "s" : "", colourEnd);
            GameManager.instance.messageScript.DecisionOngoingEffect(text, itemText, topText, middleText, bottomText, aiDecID);
        }
        //
        // - - - Security decisions - - -
        //
        switch (GameManager.instance.turnScript.authoritySecurityState)
        {
            case AuthoritySecurityState.APB:
                aiDecID = decisionAPB.aiDecID;
                text = string.Format("AI Security countermeasure {0} in place{1}", decisionAPB.name.ToUpper(), "\n");
                itemText = string.Format("AI Security countermeasure {0} in force", decisionAPB.name.ToUpper());
                topText = decisionAPB.descriptor;
                middleText = string.Format("{0}{1}{2}", colourAlert, decisionAPB.tooltipDescriptor, colourEnd);
                bottomText = string.Format("Duration {0}Unknown{1}", colourNeutral, colourEnd);
                GameManager.instance.messageScript.DecisionOngoingEffect(text, itemText, topText, middleText, bottomText, aiDecID);
                break;
            case AuthoritySecurityState.SecurityAlert:
                aiDecID = decisionSecAlert.aiDecID;
                text = string.Format("AI Security countermeasure {0} in place{1}", decisionSecAlert.name.ToUpper(), "\n");
                itemText = string.Format("AI Security countermeasure {0} in force", decisionSecAlert.name.ToUpper());
                topText = decisionSecAlert.descriptor;
                middleText = string.Format("{0}{1}{2}", colourAlert, decisionSecAlert.tooltipDescriptor, colourEnd);
                bottomText = string.Format("Duration {0}Unknown{1}", colourNeutral, colourEnd);
                GameManager.instance.messageScript.DecisionOngoingEffect(text, itemText, topText, middleText, bottomText, aiDecID);
                break;
            case AuthoritySecurityState.SurveillanceCrackdown:
                aiDecID = decisionCrackdown.aiDecID;
                text = string.Format("AI Security countermeasure {0} in place{1}", decisionCrackdown.name.ToUpper(), "\n");
                itemText = string.Format("AI Security countermeasure {0} in force", decisionCrackdown.name.ToUpper());
                topText = decisionCrackdown.descriptor;
                middleText = string.Format("{0}{1}{2}", colourAlert, decisionCrackdown.tooltipDescriptor, colourEnd);
                bottomText = string.Format("Duration {0}Unknown{1}", colourNeutral, colourEnd);
                GameManager.instance.messageScript.DecisionOngoingEffect(text, itemText, topText, middleText, bottomText, aiDecID);
                break;
            case AuthoritySecurityState.Normal:
                //Nothing happens here but needed to avoid triggering default statement
                break;
            default:
                Debug.LogWarningFormat("Invalid AuthoritySecurityState \"{0}\"", GameManager.instance.turnScript.authoritySecurityState);
                break;
            
        }
    }


    //new methods above here
}
