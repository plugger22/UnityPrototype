using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// AI data package used to collate all node info where a node has degraded in some form
/// </summary>
public class AINodeData
{
    public int nodeID;
    public NodeData type;
    public int difference;                  //shows difference between current and start values
    public int current;                     //shows current value
}

/// <summary>
/// Handles AI management of both sides
/// </summary>
public class AIManager : MonoBehaviour
{
    [Tooltip("How many turns, after the event, that the AI will track Connection & Node activity before ignoring it")]
    [Range(5, 15)] public int activityTimeLimit = 10;

    //info gathering lists (collated every turn)
    List<AINodeData> listNodeMaster = new List<AINodeData>();

    /// <summary>
    /// Runs Resistance turn on behalf of AI
    /// </summary>
    public void ProcessAISideResistance()
    {
        Debug.Log(string.Format("AIManager: ProcessAISideResistance{0}", "\n"));
        ClearAICollections();
    }

    /// <summary>
    /// Runs Authority turn on behalf of AI
    /// </summary>
    public void ProcessAISideAuthority()
    {
        Debug.Log(string.Format("AIManager: ProcessAISideAuthority{0}", "\n"));
        ClearAICollections();
        //Nodes        
        GetAINodeData();
        ProcessAINodeData();
    }

    /// <summary>
    /// run prior to any info gathering each turn to empty out all data collections
    /// </summary>
    private void ClearAICollections()
    {
        listNodeMaster.Clear();
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
    /// gathers data on all nodes that have degraded in some from (from their starting values)
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
                        //node stability has degraded
                        dataPackage = new AINodeData();
                        dataPackage.nodeID = node.Value.nodeID;
                        dataPackage.type = NodeData.Stability;
                        dataPackage.difference = Mathf.Abs(data);
                        dataPackage.current = node.Value.Stability;
                        listNodeMaster.Add(dataPackage);
                    }
                    //Security
                    data = node.Value.GetNodeChange(NodeData.Security);
                    if (data < 0)
                    {
                        //node stability has degraded
                        dataPackage = new AINodeData();
                        dataPackage.nodeID = node.Value.nodeID;
                        dataPackage.type = NodeData.Security;
                        dataPackage.difference = Mathf.Abs(data);
                        dataPackage.current = node.Value.Security;
                        listNodeMaster.Add(dataPackage);
                    }
                    //Support (positive value indicates a problem, eg. growing support for resistance)
                    data = node.Value.GetNodeChange(NodeData.Support);
                    if (data > 0)
                    {
                        //node stability has degraded
                        dataPackage = new AINodeData();
                        dataPackage.nodeID = node.Value.nodeID;
                        dataPackage.type = NodeData.Support;
                        dataPackage.difference = data;
                        dataPackage.current = node.Value.Support;
                        listNodeMaster.Add(dataPackage);
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

    }
    //new methods above here
}
