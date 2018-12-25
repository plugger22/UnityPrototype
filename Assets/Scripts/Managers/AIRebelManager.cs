using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using System;
using System.Linq;
using System.Text;
using modalAPI;
using Random = UnityEngine.Random;

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

    //tasks
    List<AITask> listOfTasksPotential = new List<AITask>();

    //targets
    private Dictionary<Target, int> dictOfSortedTargets = new Dictionary<Target, int>();


    public void Initialise()
    {
        //set initial move node to start position (will trigger a new targetNodeID)
        targetNodeID = GameManager.instance.nodeScript.nodePlayer;
    }

    /// <summary>
    /// main controlling method to run Resistance AI each turn, called from AIManager.cs -> ProcessAISideResistance
    /// </summary>
    public void ProcessAI()
    {
        ClearAICollections();
        //update node data
        UpdateNodeData();
        UpdateActions();
        //Info gathering
        ProcessTargetData();
        do
        {
            //task creation
            ProcessMoveTask();
            //task Execution
            ExecuteTask();
        }
        while (actionsUsed < actionAllowance);
    }

    /// <summary>
    /// reset all data prior to AI turn processing
    /// </summary>
    private void ClearAICollections()
    {
        dictOfSortedTargets.Clear();
        listOfTasksPotential.Clear();
    }


    private void UpdateActions()
    {
        actionAllowance = actionsBase + actionsExtra;
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
        if (nodePlayer != null)
        {
            //Debug -> Player moves around map to a target then selects a new target to move to

            //at target node?
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
                    do
                    { randomNode = GameManager.instance.dataScript.GetRandomNode(); }
                    while (randomNode.nodeID != nodePlayer.nodeID);
                    //new target
                    targetNodeID = randomNode.nodeID;
                    nodeMoveTo = randomNode;
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessMoveTask: AI Player at Target Node, Random Node chosen{0}", "\n");
                }
            }
            //double check not at current node
            Connection connection = null;
            if (nodePlayer.nodeID != targetNodeID)
            {
                //Path to existing targetNodeID
                List<Connection> pathList = GameManager.instance.dijkstraScript.GetPath(nodePlayer.nodeID, targetNodeID, true);
                //get next node in sequence
                if (pathList != null)
                {
                    connection = pathList[0];
                    if (connection.node1.nodeID != nodePlayer.nodeID)
                    { nodeMoveTo = connection.node1; }
                    else { nodeMoveTo = connection.node2; }
                    Debug.LogFormat("[Rim] AIRebelManager.cs -> ProcessMoveTask: AI Player continues to move towards target{0}","\n");
                }
                else { Debug.LogError("Invalid pathList (Null)"); }
            }
            else { Debug.LogError("Duplicate target and current player nodes"); }
            if (nodeMoveTo != null)
            {
                //create a task
                AITask task = new AITask();
                task.data0 = nodeMoveTo.nodeID;
                task.data1 = connection.connID;
                task.type = AITaskType.Move;
                task.priority = Priority.Medium;
                //add task to list of potential tasks
                listOfTasksPotential.Add(task);
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

        //gear
    }

    //
    // - - -  Debug - - -
    //

    /// <summary>
    /// Show Resistance AI status
    /// </summary>
    /// <returns></returns>
    public string DebugShowRebelAIStatus()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat(" Resistance AI Status{0}{1}", "\n", "\n");
        builder.AppendFormat(" -ProcessTargetData (weighted distances){0}", "\n");
        int count = dictOfSortedTargets.Count;
        if (count > 0)
        {
            foreach(var target in dictOfSortedTargets)
            { builder.AppendFormat("Target: {0}, at node id {1}, distance {2}{3}", target.Key.name, target.Key.nodeID, target.Value, "\n"); }
        }
        else { builder.Append(" No records present"); }
        return builder.ToString();
    }

    //new methods above here
}
