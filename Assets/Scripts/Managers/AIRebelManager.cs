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
    [Range(1, 3)] public int actionsBase = 1;

    [Header("Sightings")]
    [Tooltip("Delete sighting reports (Nemesis, Erasure teams, etc) older than ('>') this number of turns ago")]
    [Range(1, 5)] public int deleteOlderThan = 3;

    //AI Player
    [HideInInspector] public ActorStatus status;
    [HideInInspector] public ActorInactive inactiveStatus;

    private int actionAllowance;                        //number of actions per turn (normal allowance + extras)
    private int actionsExtra;                           //bonus actions for this turn
    private int actionsUsed;                            //tally of actions used this turn
    

    private int targetNodeID;                           //goal to move towards
    private int aiPlayerStartNodeID;                    //reference only, node AI Player commences at

    private bool isConnectionsChanged;                  //if true connections have been changed due to sighting data and need to be restore once all calculations are done

    //fast access
    private GlobalSide globalResistance;
    private int numOfNodes = -1;

    //Resistance activity
    List<AITracker> listOfNemesisReports = new List<AITracker>();
    List<AITracker> listOfErasureReports = new List<AITracker>();
    List<SightingData> listOfNemesisSightData = new List<SightingData>();
    List<SightingData> listOfErasureSightData = new List<SightingData>();
    List<int> listOfBadNodes = new List<int>();                             //list of nodes to avoid for this turn, eg. nemesis or erasure team present (based on known info)

    //tasks
    List<AITask> listOfTasksPotential = new List<AITask>();

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
        globalResistance = GameManager.instance.globalScript.sideResistance;
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(numOfNodes > -1, "Invalid numOfNodes (-1)");
    }

    /// <summary>
    /// main controlling method to run Resistance AI each turn, called from AIManager.cs -> ProcessAISideResistance
    /// </summary>
    public void ProcessAI()
    {
        isConnectionsChanged = false;
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
                ClearAICollectionsLate();
                //task creation
                ProcessSurvivalTask();
                ProcessMoveTask();
                //task Execution
                ExecuteTask();
                counter++;
                if (counter > 3)
                {
                    Debug.LogWarning("Break triggered on counter value, shouldn't have happened");
                    break;
                }
            }
            while (actionsUsed < actionAllowance);
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
    }

    /// <summary>
    /// start of AI Player turn admin
    /// </summary>
    private void UpdateAdmin()
    {
        //actions
        actionAllowance = actionsBase + actionsExtra;
        actionsUsed = 0;
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
                listOfBadNodes.Add(sightingNemesis.nodeID);
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
                    listOfBadNodes.Add(sightingErasure.nodeID);
                }
            }
            //recalculate weighted data
            GameManager.instance.dijkstraScript.RecalculateWeightedData();
        }
    }

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
        List<Condition> listOfConditions = GameManager.instance.playerScript.GetListOfConditions(globalResistance);
        if (listOfConditions != null)
        {
            int count = listOfConditions.Count;
            if (count > 0)
            {
                foreach(Condition condition in listOfConditions)
                {
                    if (condition != null)
                    {
                        switch (condition.name)
                        {
                            case "BLACKMAILER":

                                break;
                            case "CORRUPT":

                                break;
                            case "DOOMED":

                                break;
                            case "IMAGED":

                                break;
                            case "INCOMPETENT":

                                break;
                                case "QUESTIONABLE":

                                break;
                            case "STAR":

                                break;
                            case "STRESSED":

                                break;
                            case "TAGGED":

                                break;
                            case "UNHAPPY":

                                break;
                            case "WOUNDED":

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
        int playerNodeID = GameManager.instance.nodeScript.nodePlayer;
        //Nemesis nearby

    }

    /// <summary>
    /// Select a suitable node to move to (single node move)
    /// </summary>
    private void ProcessMoveTask()
    {
        Node nodePlayer = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
        Node nodeMoveTo = null;
        Connection connection = null;
        if (nodePlayer != null)
        {
            //Debug -> Player moves around map to a target then selects a new target to move to

            //AT TARGET NODE
            if (nodePlayer.nodeID == targetNodeID)
            {
                //select a new target goal
                if (dictOfSortedTargets.Count > 0)
                {
                    //select the nearest target goal that is > 0 distance
                    foreach(var record in dictOfSortedTargets)
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
                AITask task = new AITask();
                task.data0 = nodeMoveTo.nodeID;
                task.data1 = connection.connID;
                task.type = AITaskType.Move;
                task.priority = Priority.Medium;
                //add task to list of potential tasks
                listOfTasksPotential.Add(task);
                Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessMoveTask: targetNodeID {0}{1}", targetNodeID, "\n");
            }
            else { Debug.LogError("Invalid nodeMoveTo (Null)"); }
        }
        else { Debug.LogErrorFormat("Invalid player node (Null) for nodeID {0}", GameManager.instance.nodeScript.nodePlayer); }
    }


    //
    // - - - Execute Tasks - - -
    //

    /// <summary>
    /// carry out task in listOfTasksFinal (should only be one)
    /// </summary>
    private void ExecuteTask()
    {
        int count = listOfTasksPotential.Count;
        Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteTask: {0} potential Task{1} available{2}", count, count != 1 ? "s" : "", "\n");
        if (count > 0)
        {
            //select a task from listOfPotential Tasks
            AITask task = listOfTasksPotential[Random.Range(0, count)];
            if (task != null)
            {
                //update actions
                actionsUsed++;
                //execute taks
                switch(task.type)
                {
                    case AITaskType.Move:
                        ExecuteMoveTask(task);
                        break;
                    default:
                        Debug.LogErrorFormat("Invalid task (Unrecognised) \"{0}\"", task.type);
                        break;
                }
            }
            else { Debug.LogWarning("Invalid task (Null)"); }
        }
        else { Debug.LogWarning("There are no tasks to execute in listOfTaskPotential"); }
    }

    /// <summary>
    /// AI Player moves
    /// NOTE: Task checked for Null by parent method
    /// </summary>
    private void ExecuteMoveTask(AITask task)
    {
        //data0 is nodeID, data1 is connectionID
        Node node = GameManager.instance.dataScript.GetNode(task.data0);
        if (node != null)
        {
            //update player node
            GameManager.instance.nodeScript.nodePlayer = node.nodeID;
            Debug.LogFormat("[Rim] AIRebelManager.cs -> ExecuteMoveTask: AI Player moves to {0}, {1}, id {2}{3}", node.nodeName, node.Arc.name, node.nodeID, "\n");

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
            CaptureDetails captureDetails = GameManager.instance.captureScript.CheckCaptured(node.nodeID, GameManager.instance.playerScript.actorID);
            if (captureDetails != null)
            {
                //Player captured!
                captureDetails.effects = "The move went bad{1}";
                EventManager.instance.PostNotification(EventType.Capture, this, captureDetails, "NodeManager.cs -> ProcessMoveOutcome");
            }
            else
            {
                //Nemesis, if at same node, can interact and damage player
                GameManager.instance.nemesisScript.CheckNemesisAtPlayerNode(true);
            }
        }
        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", task.data0); }
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

    //new methods above here
}
