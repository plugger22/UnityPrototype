using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using System.Text;

/// <summary>
/// AI data package used to collate all node info where a node has degraded in some form
/// </summary>
public class AINodeData
{
    public int nodeID;
    public NodeData type;
    public NodeArc arc;
    public int difference;                  //shows difference between current and start values
    public int current;                     //shows current value
}

/// <summary>
/// AI data package detailing an Authority task that is ready to be executed next turn
/// </summary>
public class AITask
{
    public int nodeID;
    public NodeArc nodeArc;
    public string teamArc;                  //team arc name, eg. 'CIVIL'
    public Priority priority;
    public int chance;                      //dynamically added by ProcessTasksFinal (for display to player of % chance of this task being chosen)
}

/// <summary>
/// Handles AI management of both sides
/// </summary>
public class AIManager : MonoBehaviour
{
    [Tooltip("How many turns, after the event, that the AI will track Connection & Node activity before ignoring it")]
    [Range(5, 15)] public int activityTimeLimit = 10;
    [Tooltip("How much renown it will cost to access the AI's decision making process for Level 1 (potential tasks & % chances). Double this for level 2 (final tasks)")]
    [Range(0, 10)] public int playerAIRenownCost = 1;

    private Faction factionAuthority;
    private Faction factionResistance;
    private string authorityPreferredArc;                               //string name of preferred node Arc for faction (if none then null)
    private string resistancePreferredArc;
    private int authorityMaxTasksPerTurn;                               //how many tasks the AI can undertake in a turns
    private int resistanceMaxTasksPerTurn;

    //info gathering lists (collated every turn)
    List<AINodeData> listNodeMaster = new List<AINodeData>();
    List<AINodeData> listStabilityCritical = new List<AINodeData>();
    List<AINodeData> listStabilityNonCritical = new List<AINodeData>();
    List<AINodeData> listSecurityCritical = new List<AINodeData>();
    List<AINodeData> listSecurityNonCritical = new List<AINodeData>();
    List<AINodeData> listSupportCritical = new List<AINodeData>();
    List<AINodeData> listSupportNonCritical = new List<AINodeData>();
    //tasks
    List<AITask> listOfTasksPotential = new List<AITask>();
    List<AITask> listOfTasksFinal = new List<AITask>();


    public void Initialise()
    {
        factionAuthority = GameManager.instance.factionScript.factionAuthority;
        factionResistance = GameManager.instance.factionScript.factionResistance;
        Debug.Assert(factionAuthority != null, "Invalid factionAuthority (Null)");
        Debug.Assert(factionResistance != null, "Invalid factionResistance (Null)");
        //get names of node arcs (name or null, if none)
        if (factionAuthority.preferredArc != null) { authorityPreferredArc = factionAuthority.preferredArc.name; }
        if (factionResistance.preferredArc != null) { resistancePreferredArc = factionResistance.preferredArc.name; }
        authorityMaxTasksPerTurn = factionAuthority.maxTaskPerTurn;
        resistanceMaxTasksPerTurn = factionResistance.maxTaskPerTurn;
    }

    /// <summary>
    /// Runs Resistance turn on behalf of AI
    /// </summary>
    public void ProcessAISideResistance()
    {
        Debug.Log(string.Format("AIManager: ProcessAISideResistance{0}", "\n"));
        ExecuteTasks();
        ClearAICollections();
    }

    /// <summary>
    /// Runs Authority turn on behalf of AI
    /// </summary>
    public void ProcessAISideAuthority()
    {
        Debug.Log(string.Format("AIManager: ProcessAISideAuthority{0}", "\n"));
        ExecuteTasks();
        ClearAICollections();
        //Nodes        
        GetAINodeData();
        ProcessAINodeData();
        ProcessNodeTasks();
        //choose tasks for the turn
        ProcessTasksFinal(authorityMaxTasksPerTurn);
    }

    /// <summary>
    /// run prior to any info gathering each turn to empty out all data collections
    /// </summary>
    private void ClearAICollections()
    {
        listOfTasksFinal.Clear();
        listOfTasksPotential.Clear();
        listNodeMaster.Clear();
        listStabilityCritical.Clear();
        listStabilityNonCritical.Clear();
        listSecurityCritical.Clear();
        listSecurityNonCritical.Clear();
        listSupportCritical.Clear();
        listSupportNonCritical.Clear();
    }

    /// <summary>
    /// Extracts all relevant AI data from an AI related message
    /// </summary>
    /// <param name="message"></param>
    public void GetAIData(Message message)
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
                        { connection.AddActivityData(message.turnCreated); }
                        else { Debug.LogWarning(string.Format("Invalid connection (Null) for connID {0} -> AI data NOT extracted", message.data1)); }
                        break;
                    case MessageSubType.AI_Node:
                        //Get Node and add Activity data
                        Node node = GameManager.instance.dataScript.GetNode(message.data0);
                        if (node != null)
                        { node.AddActivityData(message.turnCreated); }
                        else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0} -> AI data NOT extracted", message.data0)); }
                        break;
                    case MessageSubType.AI_Capture:
                    case MessageSubType.AI_Release:
                        
                        //- - - TO DO - - - 

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

    /// <summary>
    /// gathers data on all nodes that have degraded in some from (from their starting values) and adds to listNodeMaster (from scratch each turn)
    /// </summary>
    private void GetAINodeData()
    {
        int data;
        AINodeData dataPackage;
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetAllNodes();
        if (dictOfNodes != null)
        {
            foreach(var node in dictOfNodes)
            {
                if (node.Value != null)
                {
                    //Stability
                    data = node.Value.GetNodeChange(NodeData.Stability);
                    if (data < 0)
                    {
                        //ignore if civil team already present
                        if (node.Value.isStabilityTeam == false)
                        {
                            //node stability has degraded
                            dataPackage = new AINodeData();
                            dataPackage.nodeID = node.Value.nodeID;
                            dataPackage.type = NodeData.Stability;
                            dataPackage.arc = node.Value.Arc;
                            dataPackage.difference = Mathf.Abs(data);
                            dataPackage.current = node.Value.Stability;
                            listNodeMaster.Add(dataPackage);
                        }
                    }
                    //Security
                    data = node.Value.GetNodeChange(NodeData.Security);
                    if (data < 0)
                    {
                        //ignore if control team already present
                        if (node.Value.isSecurityTeam == false)
                        {
                            //node stability has degraded
                            dataPackage = new AINodeData();
                            dataPackage.nodeID = node.Value.nodeID;
                            dataPackage.type = NodeData.Security;
                            dataPackage.arc = node.Value.Arc;
                            dataPackage.difference = Mathf.Abs(data);
                            dataPackage.current = node.Value.Security;
                            listNodeMaster.Add(dataPackage);
                        }
                    }
                    //Support (positive value indicates a problem, eg. growing support for resistance)
                    data = node.Value.GetNodeChange(NodeData.Support);
                    if (data > 0)
                    {
                        //ignore if media team already present
                        if (node.Value.isSupportTeam == false)
                        {
                            //node stability has degraded
                            dataPackage = new AINodeData();
                            dataPackage.nodeID = node.Value.nodeID;
                            dataPackage.type = NodeData.Support;
                            dataPackage.arc = node.Value.Arc;
                            dataPackage.difference = data;
                            dataPackage.current = node.Value.Support;
                            listNodeMaster.Add(dataPackage);
                        }
                    }
                }
                else { Debug.LogWarning(string.Format("Invalid node (Null) in dictOfNodes for nodeID {0}", node.Key)); }
            }
        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
    }

    /// <summary>
    /// Processes raw node data into useable data
    /// </summary>
    private void ProcessAINodeData()
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
    /// master method that determines up to 3 separate tasks, one for each node datapoint and the relevant team (Control/Civil/Media)
    /// </summary>
    private void ProcessNodeTasks()
    {
        //Stability
        AITask taskStability = SelectNodeTask(listStabilityCritical, listStabilityNonCritical, "CIVIL");
        if (taskStability != null) { listOfTasksPotential.Add(taskStability); }
        //Security
        AITask taskSecurity = SelectNodeTask(listSecurityCritical, listSecurityNonCritical, "CONTROL");
        if (taskSecurity != null) { listOfTasksPotential.Add(taskSecurity); }
        //Support
        AITask taskSupport = SelectNodeTask(listSupportCritical, listSupportNonCritical, "MEDIA");
        if (taskSupport != null) { listOfTasksPotential.Add(taskSupport); }

    }


    /// <summary>
    /// Selects tasks from pool of potential tasks for this turn
    /// </summary>
    private void ProcessTasksFinal(int maxTasksPerTurn)
    {
        int numTasks = listOfTasksPotential.Count;
        int index, odds;
        float baseOdds = 100f;
        int numTasksSelected = 0;
        if (numTasks > 0)
        {
            List<AITask> listOfTasksCritical = new List<AITask>();
            List<AITask> listOfTasksNonCritical = new List<AITask>();
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
                if (numTasks <= maxTasksPerTurn)
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
                    //insufficient tasks -> work out odds first
                    odds = (int)(baseOdds / numTasks);
                    foreach(AITask task in listOfTasksCritical)
                    { task.chance = odds; }
                    //randomly choose
                    do
                    {
                        index = Random.Range(0, numTasks);
                        listOfTasksFinal.Add(listOfTasksCritical[index]);
                        numTasksSelected++;
                        //remove entry from list to prevent future selection
                        listOfTasksCritical.RemoveAt(index);
                    }
                    while (numTasksSelected < maxTasksPerTurn);
                }
            }
            //still room 
            if (numTasksSelected < maxTasksPerTurn)
            {
                numTasks = listOfTasksNonCritical.Count;
                int remainingTasks = maxTasksPerTurn - numTasksSelected;
                baseOdds = ((float)remainingTasks / (float)maxTasksPerTurn) * 100f;
                //Non-Critical tasks next
                if (remainingTasks <= numTasks)
                {
                    //do all -> assign odds first
                    odds = (int)(baseOdds / (float)numTasks);
                    foreach (AITask task in listOfTasksNonCritical)
                    { task.chance = odds; }
                    //select tasks
                    foreach (AITask task in listOfTasksNonCritical)
                    {
                        listOfTasksFinal.Add(task);
                        numTasksSelected++;
                        if (numTasksSelected >= maxTasksPerTurn)
                        { break; }
                    }
                }
                else
                {
                    //need to randomly select from pool of tasks based on priority probabilities
                    List<AITask> tempList = new List<AITask>();
                    //populate tempList with copies of NonCritical tasks depending on priority (low -> 1 copy, medium -> 2 copies, high -> 3 copies)
                    foreach (AITask task in listOfTasksNonCritical)
                    {
                        switch(task.priority)
                        {
                            case Priority.High:
                                tempList.Add(task); tempList.Add(task); tempList.Add(task);
                                break;
                            case Priority.Medium:
                                tempList.Add(task); tempList.Add(task);
                                break;
                            case Priority.Low:
                                tempList.Add(task);
                                break;
                            default:
                                Debug.LogWarning(string.Format("Invalid task.priority \"{0}\" for nodeID {1}, team {2}", task.priority, task.nodeID, task.teamArc));
                                break;
                        }
                    }
                    //work out and assign odds first
                    numTasks = tempList.Count;
                    foreach(AITask task in listOfTasksNonCritical)
                    {
                        switch(task.priority)
                        {
                            case Priority.High:
                                task.chance = (int)((3 / (float)numTasks) * 100);
                                break;
                            case Priority.Medium:
                                task.chance = (int)((2 / (float)numTasks) * 100);
                                break;
                            case Priority.Low:
                                task.chance = (int)((1 / (float)numTasks) * 100);
                                break;
                        }
                    }
                    //randomly draw from pool
                    string selectedTeamArc;
                    do
                    {
                        numTasks = tempList.Count;
                        if (numTasks > 0)
                        {
                            index = Random.Range(0, numTasks);
                            listOfTasksFinal.Add(tempList[index]);
                            selectedTeamArc = tempList[index].teamArc;
                            numTasksSelected++;
                            //don't bother unless further selections are needed
                            if (numTasksSelected < maxTasksPerTurn)
                            {
                                //reverse loop and remove all instances of task from tempList to prevent duplicate selections
                                for (int i = numTasks - 1; i >= 0; i--)
                                {
                                    if (tempList[i].teamArc.Equals(selectedTeamArc) == true)
                                    { tempList.RemoveAt(i); }
                                }
                            }
                        }
                        else { numTasksSelected++; }
                    }
                    while (numTasksSelected < maxTasksPerTurn);
                }
            }

        }
        else { Debug.LogWarning("AIManager.cs -> ProcessTasksFinal: No tasks this turn"); }
    }


    /// <summary>
    /// sub method (ProcessNodeTasks) that taks two node datapoint lists (must be the same datapoint, eg. security) and determines a task (Null if none) based on AI rules
    /// </summary>
    /// <param name="listCritical"></param>
    /// <param name="listNonCritical"></param>
    /// <returns></returns>
    private AITask SelectNodeTask(List<AINodeData> listCritical, List<AINodeData> listNonCritical, string name)
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
                if (authorityPreferredArc != null)
                {
                    foreach (AINodeData data in listCritical)
                    {
                        if (data.arc.name.Equals(authorityPreferredArc) == true)
                        { tempList.Add(data); }
                    }
                }
                if (tempList.Count > 0)
                {
                    //randomly select a preferred faction option
                    index = Random.Range(0, tempList.Count);
                    //generate task
                    task = new AITask() { nodeID = tempList[index].nodeID, nodeArc = tempList[index].arc, teamArc = name, priority = Priority.Critical };
                }
                else
                {
                    //otherwise randomly select any option
                    index = Random.Range(0, listCount);
                    //generate task
                    task = new AITask() { nodeID = listCritical[index].nodeID, nodeArc = listCritical[index].arc, teamArc = name, priority = Priority.Critical };
                }
            }
            else
            {
                //single record only
                task = new AITask() { nodeID = listCritical[0].nodeID, nodeArc = listCritical[0].arc, teamArc = name, priority = Priority.Critical };
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
                if (authorityPreferredArc != null)
                {
                    foreach (AINodeData data in listNonCritical)
                    {
                        if (data.arc.name.Equals(authorityPreferredArc) == true)
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
                        task = new AITask() { nodeID = tempList[index].nodeID, nodeArc = tempList[index].arc, teamArc = name, priority = priority };
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
                        task = new AITask() { nodeID = listNonCritical[index].nodeID, nodeArc = listNonCritical[index].arc, teamArc = name, priority = priority };
                    }
                }
            }
        }
        return task;
    }


    /// <summary>
    /// carry out all tasks in listOfTasksFinal 
    /// </summary>
    private void ExecuteTasks()
    {

    }

    //
    // - - - Debug - - -
    //

    /// <summary>
    /// Debug display method for AI relevant data
    /// </summary>
    /// <returns></returns>
    public string DisplayNodeData()
    {
        StringBuilder builder = new StringBuilder();
        //Task lists
        builder.AppendFormat("- listOfTasksFinal{0}", "\n");
        builder.Append(DebugTaskList(listOfTasksFinal));
        builder.AppendFormat("{0}- listOfTasksPotential{1}", "\n", "\n");
        builder.Append(DebugTaskList(listOfTasksPotential));
        //Master Node list
        builder.AppendFormat("{0}- listNodeMaster{1}", "\n", "\n");
        builder.Append(DebugDisplayList(listNodeMaster));
        //Sub Node lists
        builder.AppendFormat("{0}- listStabilityCritical{1}", "\n", "\n");
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
                { builderList.AppendFormat("ID {0}, {1}, {2}, difference: {3}, current: {4}{5}", data.nodeID, data.arc.name, data.type, data.difference, data.current, "\n"); }
            }
            else { builderList.AppendFormat("No records{0}", "\n"); }
        }
        else { Debug.LogWarning(string.Format("Invalid list \"{0}\" (Null)", list)); }
        return builderList.ToString();
    }

    /// <summary>
    /// debug submethod to display AI Tasks, used by AIManager.cs -> DisplayNodeData
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private string DebugTaskList(List<AITask> listOfTasks)
    {
        StringBuilder builderList = new StringBuilder();
        if (listOfTasks != null)
        {
            if (listOfTasks.Count > 0)
            {
                foreach (AITask task in listOfTasks)
                { builderList.AppendFormat("ID {0} {1}, {2} team, {3} priority, Prob {4} %{5}", task.nodeID, task.nodeArc.name, task.teamArc, task.priority,
                    task.chance, "\n"); }
            }
            else { builderList.AppendFormat("No records{0}", "\n"); }
        }
        else { builderList.AppendFormat("No records{0}", "\n"); }
        return builderList.ToString();
    }

    //new methods above here
}
