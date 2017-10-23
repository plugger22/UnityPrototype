using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;


/// <summary>
/// Handles all node related matters
/// </summary>
public class NodeManager : MonoBehaviour
{
    [Tooltip("% chance times actor.Ability of a primary node being active for an Actor")]
    public int nodePrimaryChance;                                   //% chance (times actor.Ability) of a primary node being active for an actor, halved for secondary, default 10%
    public int nodeActiveMinimum;                                   //minimum number of active nodes on a map for any actor type, default 3

    public Material[] arrayOfNodeTypes;

    [HideInInspector] public int nodeCounter = 0;                   //sequentially numbers nodes
    [HideInInspector] public int connCounter = 0;                   //sequentially numbers connections
    [HideInInspector] public int nodeHighlight = -1;                //nodeID of currently highlighted node, if any, otherwise -1
    [HideInInspector] public int nodePlayer = -1;                   //nodeID of player
    //public int NumOfNodeArcs { get; set; }                        //total number of possible nodeArcs
    private bool nodeShowFlag = false;                          //true if a ShowNodes() is active, false otherwise
    private bool nodeRedraw = false;                                //if true a node redraw is triggered in GameManager.Update

    public bool NodeRedraw
    {
        get
        { return nodeRedraw; }
        set
        { nodeRedraw = value; }
    }

    public bool NodeShowFlag
    {
        get
        { return nodeShowFlag; }

        set
        { nodeShowFlag = value; }
    }

    private void Awake()
    {
        //bounds checking
        nodePrimaryChance = nodePrimaryChance > 0 ? nodePrimaryChance : 10;
        nodeActiveMinimum = nodeActiveMinimum > 2 ? nodeActiveMinimum : 3;

    }

    private void Start()
    {
                //register listener
        EventManager.instance.AddListener(EventType.NodeDisplay, OnEvent);
    }


    /// <summary>
    /// event handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object param = null)
    {
        //detect event type
        switch (eventType)
        {
            case EventType.NodeDisplay:
                NodeUI nodeUI = (NodeUI)param;
                switch(nodeUI)
                {
                    case NodeUI.Reset:
                        ResetNodes();
                        break;
                    case NodeUI.Redraw:
                        RedrawNodes();
                        break;
                    case NodeUI.ShowTargets:
                        ShowNodes(nodeUI);
                        break;
                        
                    default:
                        Debug.LogError(string.Format("Invalid NodeUI param \"{0}\"{1}", param.ToString(), "\n"));
                        break;
                }
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    public void OnDisable()
    {
        EventManager.instance.RemoveEvent(EventType.ShowTargetNodes);
    }

    /// <summary>
    /// toggles a node on/off as Highlighted, default OFF 
    /// </summary>
    /// <param name="nodeID">leave blank to switch off currently highlighted node</param>
    public void ToggleNodeHighlight(int highlightID = -1)
    {
        if (nodeHighlight != highlightID)
        {
            ResetNodes();
            nodeHighlight = highlightID;
            Debug.Log("Highlighted node " + highlightID);
        }
        else
        {
            ResetNodes();
            nodeHighlight = -1;
        }
    }

    /// <summary>
    /// Return a node Material
    /// </summary>
    /// <param name="nodeType"></param>
    /// <returns></returns>
    public Material GetNodeMaterial(NodeType nodeType)
    { return arrayOfNodeTypes[(int)nodeType]; }


    /// <summary>
    /// highlights all nodes depening on the enum NodeUI criteria
    /// </summary>
    public void ShowNodes(NodeUI nodeUI)
    {
        //set all nodes to default colour first
        ResetNodes();
        bool successFlag = true;
        //set nodes depending on critera
        switch (nodeUI)
        {
            case NodeUI.ShowTargets:
                //change material for selected nodes
                List<Target> tempList = GameManager.instance.dataScript.GetLiveTargets();
                GameObject nodeObject = null;
                foreach (Target target in tempList)
                {
                    nodeObject = GameManager.instance.dataScript.GetNodeObject(target.NodeID);
                    if (nodeObject != null)
                    {
                        Node nodeTemp = nodeObject.GetComponent<Node>();
                        Material nodeMaterial = GameManager.instance.nodeScript.GetNodeMaterial(NodeType.Active);
                        nodeTemp.SetMaterial(nodeMaterial);
                    }
                }
                break;
            default:
                Debug.LogError(string.Format("Invalid NodeUI parameter \"{0}\"{1}", nodeUI, "\n"));
                successFlag = false;
                break;
        }
        //set show node flag on success
        if (successFlag == true)
        { NodeShowFlag = true; }
    }


    /// <summary>
    /// Redraw any nodes. Show highlighted node, unless it's a non-normal node for the current redraw
    /// </summary>
    public void RedrawNodes()
    {
        Renderer nodeRenderer;
        Dictionary<int, Node> tempDict = GameManager.instance.dataScript.GetAllNodes();
        if (tempDict != null)
        {
            //loop all nodes & assign current materials to their renderers (changes colour on screen)
            foreach (var node in tempDict)
            {
                nodeRenderer = node.Value.GetComponent<Renderer>();
                nodeRenderer.material = node.Value._Material;
            }
            //highlighted node
            if (nodeHighlight > -1)
            {
                Node node = GameManager.instance.dataScript.GetNode(nodeHighlight);
                if (node != null)
                {
                    //only do so if it's a normal node, otherwise ignore
                    if (node.GetMaterial() == GetNodeMaterial(NodeType.Normal))
                    {
                        nodeRenderer = node.GetComponent<Renderer>();
                        nodeRenderer.material = GetNodeMaterial(NodeType.Highlight);
                    }
                }
                else { Debug.LogError("Invalid Node (null) returned from dictOfNodes"); }
            }
            //player's current node
            if (nodePlayer > -1)
            {
                Node node = GameManager.instance.dataScript.GetNode(nodePlayer);
                if (node != null)
                {
                    //only do so if it's a normal node, otherwise ignore
                    if (node.GetMaterial() == GetNodeMaterial(NodeType.Normal))
                    {
                        nodeRenderer = node.GetComponent<Renderer>();
                        nodeRenderer.material = GetNodeMaterial(NodeType.Player);
                    }
                }
            }
            //Alert UI text (ShowNodes)
            if (NodeShowFlag == true)
            { }
            //reset flag to prevent constant redraws
            NodeRedraw = false;
            //switch off flag to turn off Alert UI text next time around
            NodeShowFlag = false;
        }
        else { Debug.LogError("Invalid dictOfNodes (Null) returned from dataManager in RedrawNodes"); }
    }

    /// <summary>
    /// Sets the material colour of all nodes to the default (doesn't change on screen, just sets them up). Call before making any changes to node colours
    /// </summary>
    public void ResetNodes()
    {
        //get default material
        Material nodeMaterial = GetNodeMaterial(NodeType.Normal);
        //loop and assign
        Dictionary<int, Node> tempDict = GameManager.instance.dataScript.GetAllNodes();
        if (tempDict != null)
        {
            foreach (var node in tempDict)
            { node.Value.SetMaterial(nodeMaterial); }
            //trigger an automatic redraw
            NodeRedraw = true;
        }
        else { Debug.LogError("Invalid dictOfNodes (Null) returned from dataManager in ResetNodes"); }
    }


}
