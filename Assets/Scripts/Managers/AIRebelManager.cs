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

    //AI Player
    private ActorStatus status;
    private int aiPlayerInvisibility;                   //invisibility of AI player
    private int aiPlayerRenown;                         //current renown tally

    private int actionAllowance;                        //number of actions per turn (normal allowance + extras)
    private int actionsExtra;                           //bonus actions for this turn
    private int actionsUsed;                            //tally of actions used this turn
    

    private int targetNodeID;                           //goal to move towards
    private int aiPlayerStartNodeID;                    //reference only, node AI Player commences at

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
        aiPlayerInvisibility = 3;
        aiPlayerRenown = 0;
    }

    /// <summary>
    /// main controlling method to run Resistance AI each turn, called from AIManager.cs -> ProcessAISideResistance
    /// </summary>
    public void ProcessAI()
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

    /// <summary>
    /// reset all data prior to AI turn processing
    /// </summary>
    private void ClearAICollectionsEarly()
    {
        dictOfSortedTargets.Clear();
        listOfTasksPotential.Clear();
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
        //renown
        int approval = GameManager.instance.factionScript.ApprovalResistance;
        int threshold = approval * 10;
        if(Random.Range(0, 100) < threshold)
        {
            aiPlayerRenown++;
            Debug.LogFormat("[Rim] AIRebelManager.cs -> UpdateAdmin: AI Player gains +1 Renown from HQ (approval {0}), total now {1}{2}", approval, aiPlayerRenown, "\n");
        }
    }



    //
    // - - - Gather Data - - -
    //


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
        }
        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", task.data0); }

        //invisibility
        Connection connection = GameManager.instance.dataScript.GetConnection(task.data1);
        if (connection != null)
        { UpdateInvisibility(connection.SecurityLevel); }
        else { Debug.LogErrorFormat("Invalid connection (Null) for connID {0}", task.data1); }

        //gear

        //Tracker data
        TrackerRebelMove tracker = new TrackerRebelMove();
        tracker.turn = GameManager.instance.turnScript.Turn;
        tracker.playerNodeID = task.data0;
        tracker.invisibility = aiPlayerInvisibility;
        tracker.nemesisNodeID = GameManager.instance.nodeScript.nodeNemesis;
        GameManager.instance.dataScript.AddTrackerRebelMove(tracker);
    }

    /// <summary>
    /// submethod that handles invisibility loss for ExecuteMoveTask
    /// </summary>
    /// <param name="security"></param>
    private void UpdateInvisibility(ConnectionType security)
    {
        switch (security)
        {
            case ConnectionType.HIGH:
                aiPlayerInvisibility--;
                break;
            case ConnectionType.MEDIUM:
                aiPlayerInvisibility--;
                break;
            case ConnectionType.LOW:
                aiPlayerInvisibility--;
                break;
        }
        //min cap 0
        aiPlayerInvisibility = Mathf.Max(0, aiPlayerInvisibility);
        if (security != ConnectionType.None)
        { Debug.LogFormat("[Rim] AIRebelManager.cs -> UpdateInvisibility: Invisibility -1, now {0}{1}", aiPlayerInvisibility, "\n"); }
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
        builder.AppendFormat("-AI Player{0}", "\n");
        builder.AppendFormat(" status: {0}{1}", status, "\n");
        builder.AppendFormat(" Invisbility: {0}{1}", aiPlayerInvisibility, "\n");
        builder.AppendFormat(" Renown: {0}{1}", aiPlayerRenown, "\n");
        //sorted target list
        builder.AppendFormat("{0}-ProcessTargetData ({1} records){2}", "\n", dictOfSortedTargets.Count, "\n");
        int count = dictOfSortedTargets.Count;
        if (count > 0)
        {
            foreach(var target in dictOfSortedTargets)
            {
                if (target.Key != null)
                { builder.AppendFormat("Target: {0}, at node id {1}, distance {2}{3}", target.Key.name, target.Key.nodeID, target.Value, "\n"); }
                else { builder.AppendFormat("Invalid target (Null){0}", "\n"); }
            }
        }
        else { builder.Append(" No records present"); }
        return builder.ToString();
    }

    //new methods above here
}
