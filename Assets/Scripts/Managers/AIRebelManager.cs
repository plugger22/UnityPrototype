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

    [Header("Survival Situation")]
    [Tooltip("The % chance of AI player moving away when they are at a Bad Node")]
    [Range(0, 100)] public int survivalMove = 50;

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

    //AI Resistance Player
    [HideInInspector] public ActorStatus status;
    [HideInInspector] public ActorInactive inactiveStatus;
    [HideInInspector] public bool isBreakdown;          //true if suffering from nervous, stress induced, breakdown
    [HideInInspector] public string playerName;         //name of Rebel leader if a non-player (eg. human controls authority side)

    private int actionAllowance;                        //number of actions per turn (normal allowance + extras)
    private int actionsExtra;                           //bonus actions for this turn
    private int actionsUsed;                            //tally of actions used this turn
    

    private int targetNodeID;                           //goal to move towards
    private int aiPlayerStartNodeID;                    //reference only, node AI Player commences at

    private bool isConnectionsChanged;                  //if true connections have been changed due to sighting data and need to be restore once all calculations are done
    private bool isPlayer;                              //if true the Resistance side is also the human player side (it's AI due to an autorun)
    private bool isWounded;
    private bool isStressed;

    //fast access
    private GlobalSide globalResistance;
    private int numOfNodes = -1;
    private int playerID = -1;
    private int priorityHigh = -1;
    private int priorityMedium = -1;
    private int priorityLow = -1;
    private AuthoritySecurityState security;            //updated each turn in UpdateAdmin
    //conditions
    private Condition conditionStressed;
    private Condition conditionWounded;
    //tests
    private int turnForStress;

    //Resistance activity
    List<AITracker> listOfNemesisReports = new List<AITracker>();
    List<AITracker> listOfErasureReports = new List<AITracker>();
    List<SightingData> listOfNemesisSightData = new List<SightingData>();
    List<SightingData> listOfErasureSightData = new List<SightingData>();
    List<int> listOfBadNodes = new List<int>();                             //list of nodes to avoid for this turn, eg. nemesis or erasure team present (based on known info)

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
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(numOfNodes > -1, "Invalid numOfNodes (-1)");
        Debug.Assert(playerID > -1, "Invalid playerId (-1)");
        Debug.Assert(priorityHigh > -1, "Invalid priorityHigh (-1)");
        Debug.Assert(priorityMedium > -1, "Invalid priorityMedium (-1)");
        Debug.Assert(priorityLow > -1, "Invalid priorityLow (-1)");
        Debug.Assert(conditionStressed != null, "Invalid conditionStressed (Null)");
        Debug.Assert(conditionWounded != null, "Invalid conditionWounded (Null)");
        //player (human / AI)
        playerName = "The Phantom";
        if (GameManager.instance.sideScript.PlayerSide.level != globalResistance.level) { isPlayer = false; }
        else
        {
            isPlayer = true;
            playerName = GameManager.instance.playerScript.PlayerName;
        }
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
            //restore back to original state after any changes & prior to any moves, tasks, etc. Calcs will still use updated sighting data dijkstra (weighted)
            if (isConnectionsChanged == true)
            { RestoreConnections(); }
            //
            // - - - task loop (once per available action)
            //
            int counter = 0;
            do
            {
                Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessAI: commence action {0} of {1}{2}", actionsUsed + 1, actionAllowance, "\n");
                ClearAICollectionsLate();
                //task creation
                if (actionsUsed == 0)
                { ProcessSurvivalTask(); }
                ProcessMoveTask();
                ProcessIdleTask();
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
                    else { Debug.LogWarningFormat("Invalid sightingData (Null) for tracker data0 {0} data1 {1} turn {2}", tracker.data0, tracker.data1, tracker.turn); }
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
                    else { Debug.LogWarningFormat("Invalid sightingData (Null) for tracker data0 {0} data1 {1} turn {2}", tracker.data0, tracker.data1, tracker.turn); }
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
    private bool CheckIfNodeBad(int nodeID)
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
    /// deals with any conditions that AI player may have
    /// </summary>
    private void ProcessConditions()
    {
        //reset all condition flags
        isStressed = false;
        isWounded = false;
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
                                isStressed = true;
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
                string msgText = string.Format("{0} faction provides SUPPORT (+{1} Resource{2})", factionResistance.name, resources, resources != 1 ? "s" : "");
                GameManager.instance.messageScript.FactionSupport(msgText, factionResistance, approvalResistance, GameManager.instance.playerScript.Renown, renownPerTurn);
                //random
                GameManager.instance.messageScript.GeneralRandom("Faction support GIVEN", "Faction Support", threshold, rnd);
            }
            else
            {
                //Support declined
                Debug.LogFormat("[Rnd] FactionManager.cs -> CheckFactionSupport: DECLINED need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                string msgText = string.Format("{0} faction declines support ({1} % chance of support)", factionResistance.name, threshold);
                GameManager.instance.messageScript.FactionSupport(msgText, factionResistance, approvalResistance, GameManager.instance.playerScript.Renown);
                //random
                GameManager.instance.messageScript.GeneralRandom("Faction support DECLINED", "Faction Support", threshold, rnd);
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
        if (CheckIfNodeBad(playerNodeID) == true)
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
                            if (isStressed == true)
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
                    if (isStressed == true)
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
                if (CheckIfNodeBad(nodeMoveTo.nodeID) == true)
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
                        task.priority = Priority.Medium;
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
        task.priority = Priority.Low;
        //add task to list of potential tasks
        AddWeightedTask(task);
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
            int currentNodeID = node.nodeID;
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
                            if (CheckIfNodeBad(tempNode.nodeID) == true)
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
        Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteTask: task {0}, {1} priority, data0 {2}, data1 {3}{4}", task.type, task.priority, task.data0, task.data1, "\n");
        //execute taks
        switch (task.type)
        {
            case AITaskType.Move:
                ExecuteMoveTask(task);
                break;
            case AITaskType.LieLow:
                ExecuteLieLowTask(task);
                break;
            case AITaskType.Idle:
                ExecuteIdleTask(task);
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
            //action
            UseAction("Move");
            //move list (for when autorun ends)
            node.SetPlayerMoveNodes();
            //gear

            //invisibility
            Connection connection = GameManager.instance.dataScript.GetConnection(task.data1);
            if (connection != null)
            {
                if (connection.SecurityLevel != ConnectionType.None)
                { UpdateInvisibility(connection, node); }
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
                Debug.LogFormat("[Ply] AIRebelManager.cs -> ExecuteLieLowTask: Player commences LYING LOW at node ID {0}{1}", task.data0, "\n");
                //message
                string text = string.Format("{0} is lying Low. Status: {1}", playerName, status);
                string reason = string.Format("is currently Lying Low and is{0}{1}<b>cut off from all communications</b>", "\n", "\n");
                GameManager.instance.messageScript.ActorStatus(text, "is LYING LOW", reason, playerID, globalResistance);
            }
            //action
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
                //action
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
    /// do nothing task
    /// </summary>
    private void ExecuteIdleTask(AITask task)
    {
        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessIdleTask: \"{0}\", id {1} is IDLING at node ID {2}{3}", playerName, task.data1, task.data0, "\n");
        Debug.LogFormat("[Tst] AIRebelManager.cs -> ProcessIdleTask: IDLING{0}", "\n");
        //action
        UseAction("Lie Low (Player)");
    }

    /// <summary>
    /// submethod that handles invisibility loss for ExecuteMoveTask
    /// NOTE: connection and node checked for null by parent method. Also it's assumed that security is > 'None'
    /// </summary>
    /// <param name="secLevel"></param>
    private void UpdateInvisibility(Connection connection, Node node)
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
        //Add STRESSED condition
        if (turn == turnForStress)
        { GameManager.instance.playerScript.AddCondition(conditionStressed, globalResistance, "for Debugging"); }
    }

    //new methods above here
}
