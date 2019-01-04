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

    //fast access
    private GlobalSide globalResistance;
    private int numOfNodes = -1;

    //Resistance activity
    List<AITracker> listOfNemesisSightings = new List<AITracker>();

    //tasks
    List<AITask> listOfTasksPotential = new List<AITask>();

    //targets
    private Dictionary<Target, int> dictOfSortedTargets = new Dictionary<Target, int>();


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
        //AI player ACTIVE
        if (status == ActorStatus.Active)
        {
            ClearAICollectionsEarly();
            //update node data
            UpdateNodeData();
            UpdateAdmin();
            //Info gathering
            ProcessTargetData();
            //task loop (once per available action)
            int counter = 0;
            do
            {
                ClearAICollectionsLate();
                //task creation
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
    }

    /// <summary>
    /// reset all data prior to AI turn processing
    /// </summary>
    private void ClearAICollectionsEarly()
    {
        dictOfSortedTargets.Clear();
        listOfTasksPotential.Clear();

        //clear out list of nemesis sightings entries that are older than 'x' turns ago
        int count = listOfNemesisSightings.Count;
        if (count > 0)
        {
            int threshold = GameManager.instance.turnScript.Turn - deleteOlderThan;
            //only bother if threshold has kicked in
            if (threshold > 0)
            {
                //reverse loop as deleting
                for (int i = count - 1; i >= 0; i--)
                {
                    AITracker tracker = listOfNemesisSightings[i];
                    if (tracker != null)
                    {
                        //delete older entries
                        if (tracker.turn < threshold)
                        { listOfNemesisSightings.RemoveAt(i); }
                    }
                    else { Debug.LogErrorFormat("Invalid tracker (Null) for listOfNemesisSightings[{0}]", i); }
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

        //conditions
        ProcessConditions();
    }




    //
    // - - - Gather Data - - -
    //

    /// <summary>
    /// Extracts all relevant AI data from an AI related message
    /// </summary>
    /// <param name="message"></param>
    public void GetNemesisMessageData(Message message)
    {
        if (message != null)
        {
            if (message.type == MessageType.CONTACT)
            {
                switch (message.subType)
                {
                    case MessageSubType.Contact_Nemesis_Spotted:
                        //Nemesis detected by Resistance Contact, reliability of sighting dependant on contact effectiveness
                        Debug.Assert(message.data0 > -1 && message.data0 < numOfNodes, string.Format("Invalid nodeID {0} (less than Zero or >= numOfNodes)", message.data0));
                        //add to Nemesis list
                        AITracker trackerContact = new AITracker(message.data1, message.turnCreated);
                        Contact contact = GameManager.instance.dataScript.GetContact(message.data2);
                        if (contact != null)
                        { trackerContact.data1 = contact.effectiveness; }
                        else { Debug.LogErrorFormat("Invalid contact (Null) for contactID {0}", message.data2); }
                        //get contact effectivenss
                        listOfNemesisSightings.Add(trackerContact);
                        break;
                    case MessageSubType.Tracer_Nemesis_Spotted:
                        //Nemesis detected by Tracer
                        Debug.Assert(message.data0 > -1 && message.data0 < numOfNodes, string.Format("Invalid nodeID {0} (less than Zero or >= numOfNodes)", message.data0));
                        //add to Nemesis list
                        AITracker trackerTracer = new AITracker(message.data0, message.turnCreated);
                        //tracer effectiveness automatically 100 %
                        trackerTracer.data1 = 3;
                        listOfNemesisSightings.Add(trackerTracer);
                        break;
                }
            }
            else { Debug.LogWarning(string.Format("Invalid (not Nemesis) message type \"{0}\" for \"{1}\"", message.type, message.text)); }
        }
        else { Debug.LogWarning("Invalid message (Null)"); }
    }

    /// <summary>
    /// update node data for known erasure teams and nemesis locations prior to doing any Dijkstra pathing
    /// </summary>
    private void UpdateNodeData()
    {

    }




    /// <summary>
    /// Select a target nodeID as a goal to move towards
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
                //sort dict if it has > 1 entry
                if (dictOfTargets.Count > 1)
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

    //
    // - - - Create Task - - -
    //

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
                if (dictOfSortedTargets.Count > 1)
                {
                    //randomly select a new target goal
                    List<Target> tempList = dictOfSortedTargets.Keys.ToList();
                    //ignore first record as it must be distance 0
                    Target newTarget = tempList[Random.Range(1, tempList.Count)];
                    if (newTarget != null)
                    {
                        //assign new target
                        targetNodeID = newTarget.nodeID;
                        nodeMoveTo = GameManager.instance.dataScript.GetNode(targetNodeID);
                        Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessMoveTask: AI Player at Target Node, new target chosen{0}", "\n");
                    }
                    else { Debug.LogError("Invalid newTarget (Null)"); }
                }
                else
                {
                    //generate a random node as a new target
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
                    //new target
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

        //sorted target list
        builder.AppendFormat("{0}- ProcessTargetData ({1} records){2}", "\n", dictOfSortedTargets.Count, "\n");
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
        else { builder.Append(" No records present"); }

        //Nemesis Sightings
        count = listOfNemesisSightings.Count;
        builder.AppendFormat("{0}- Nemesis Sightings ({1} records){2}", "\n", count, "\n");
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                AITracker tracker = listOfNemesisSightings[i];
                if (tracker != null)
                { builder.AppendFormat(" node: id {0}, eff:{1}, turn:{2}{3}", tracker.data0, tracker.data1, tracker.turn, "\n"); }
                else { builder.AppendFormat(" Invalid sighting (Null){0}", "\n"); }
            }
        }
        else { builder.Append(" No records present"); }

        return builder.ToString();
    }

    //new methods above here
}
