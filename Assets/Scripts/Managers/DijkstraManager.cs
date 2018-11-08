using System;
using System.Collections;
using System.Collections.Generic;
using dijkstraAPI;
using gameAPI;
using packageAPI;
using UnityEngine;

/// <summary>
/// local class used to pass results of dijkstra algo to the calling method (InitialiseNodeData)
/// </summary>
public class DijkstraData
{
    public int[] arrayOfPaths;                     //corresponds to dijkstra pi[] array
    public List<int> listOfOrder;                  //corresponds to dijkstra List <int> S, list of all other nodes from closest to furtherst
    public int[] arrayOfDistances;                 //list of all NodeD's with distances for the recent ca

    /*/// <summary>
    /// Constructor that auto sizes array to numOfNodes => Not necessary as collections are initialised in code by referencing another collection
    /// </summary>
    /// <param name="numOfNodes"></param>
    public DijkstraData(int numOfNodes)
    {
        listOfOrder = new List<int>();
        arrayOfPaths = new int[numOfNodes];
    }*/
}


/// <summary>
/// Handles all Dijkstra algorithm functionality
/// </summary>
public class DijkstraManager : MonoBehaviour
{

    [Header("Connection Weights")]
    [Tooltip("Weight assigned to a connection with NO security level by the Dijkstra Alogrithm")]
    [Range(1, 6)] public int weightNONE = 1;
    [Tooltip("Weight assigned to a connection with LOW security level by the Dijkstra Alogrithm")]
    [Range(1, 6)] public int weightLOW = 3;
    [Tooltip("Weight assigned to a connection with MEDIUM security level by the Dijkstra Alogrithm")]
    [Range(1, 6)] public int weightMEDIUM = 4;
    [Tooltip("Weight assigned to a connection with HIGH security level by the Dijkstra Alogrithm")]
    [Range(1, 6)] public int weightHIGH = 5;


    private Algorithm algorithm;
    private int numOfNodes;             //used for sizing dijkstra dict arrays



    public void Awake()
    {
        algorithm = new Algorithm();
        //logic checks
        Debug.Assert(weightNONE < weightLOW, "Invalid weightNONE (must be < weightLOW)");
        Debug.Assert(weightLOW < weightMEDIUM, "Invalid weightLOW (must be < weightMEDIUM)");
        Debug.Assert(weightMEDIUM < weightHIGH, "Invalid weightMEDIUM (must be < weightHIGH)");
    }

    /// <summary>
    /// Start sequence
    /// </summary>
    public void Initialise()
    {
        numOfNodes = GameManager.instance.dataScript.CheckNumOfNodes();
        //Unweighted
        InitialiseDictDataUnweighted();
        InitialiseNodeDataUnweighted();
        //Weighted
        InitialiseDictDataWeighted();
        InitialiseNodeDataWeighted();
    }


    /// <summary>
    /// Use standard graphAPI data to set up Dijkstra Graph ready for algorithm. Unweighted
    /// </summary>
    private void InitialiseDictDataUnweighted()
    {
        //existing nodes
        List<Node> listOfNodes = new List<Node>(GameManager.instance.dataScript.GetDictOfNodes().Values);
        Debug.Assert(listOfNodes.Count == numOfNodes, string.Format("Mismatch on Node Count, listOfNodes {0} vs. numOfNodes {1}", listOfNodes.Count, numOfNodes));
        //set up mirror dijkstra friendly nodes (refered to as 'nodeD')
        List<NodeD> listOfNodeD = new List<NodeD>();
        Dictionary<int, NodeD> dictOfNodeD = GameManager.instance.dataScript.GetDictOfNodeDUnweighted();
        if (listOfNodes != null)
        {
            if (dictOfNodeD != null)
            {
                //loop nodes and populate listOfNodeD
                foreach (Node node in listOfNodes)
                {
                    if (node != null)
                    {
                        NodeD nodeD = new NodeD(int.MaxValue, node.nodeID, node.nodeName);
                        //add nodeD to collections
                        if (nodeD != null)
                        {
                            //list
                            listOfNodeD.Add(nodeD);
                            //add to dict
                            try
                            { dictOfNodeD.Add(nodeD.ID, nodeD); }
                            catch (ArgumentNullException)
                            { Debug.LogError("Invalid NodeD (Null)"); }
                            catch (ArgumentException)
                            { Debug.LogError(string.Format("Invalid (duplicate) nodeD.ID \"{0}\" for NodeD \"{1}\"", nodeD.ID, nodeD.Name)); }
                        }
                        else { Debug.LogWarning("Invalid nodeD (Null) -> Failed initialisaction"); }
                    }
                    else { Debug.LogWarning("Invalid node (Null) in listOfNodes"); }
                }
                //loop listOfNodes again  and add Neighbour data to NodeD's
                foreach (Node node in listOfNodes)
                {
                    if (node != null)
                    {
                        //get mirror nodeD
                        NodeD nodeD = null;
                        if (dictOfNodeD.ContainsKey(node.nodeID) == true)
                        { nodeD = dictOfNodeD[node.nodeID]; }
                        if (nodeD != null)
                        {
                            List<int> listOfWeights = new List<int>();
                            List<NodeD> listOfEdges = new List<NodeD>();
                            List<Node> listOfNeighbours = node.GetNeighbouringNodes();
                            if (listOfNeighbours != null)
                            {
                                //loop node neighbours and add edges and weights (assume all are value 1)
                                if (listOfNeighbours.Count > 0)
                                {
                                    foreach (Node neighbour in listOfNeighbours)
                                    {
                                        //create entry for each neighbour, default weight of 1 for all
                                        listOfWeights.Add(1);
                                        //find nodeD in dict
                                        NodeD nodeDNeighbour = null;
                                        if (dictOfNodeD.ContainsKey(neighbour.nodeID) == true)
                                        { nodeDNeighbour = dictOfNodeD[neighbour.nodeID]; }
                                        //add to NodeD edge list (neighbouring nodes)
                                        if (nodeDNeighbour != null)
                                        { listOfEdges.Add(nodeDNeighbour); }
                                        else { Debug.LogWarningFormat("Invalid nodeDNeighbour (Null) for neighbour {0}, {1}, id {2}", neighbour.nodeName, neighbour.Arc.name, neighbour.nodeID); }
                                    }
                                    //add to nodeD
                                    nodeD.Adjacency = listOfEdges;
                                    nodeD.Weights = listOfWeights;
                                }
                                else
                                {
                                    //no neighbours
                                    nodeD.Adjacency = listOfEdges;
                                    nodeD.Weights = listOfWeights;
                                }
                            }
                            else { Debug.LogWarningFormat("Invalid listOfNeighbours (Null) for node {0}, {1}, id {2}", node.nodeName, node.Arc.name, node.nodeID); }
                        }
                        else { Debug.LogWarningFormat("Invalid nodeD (Null) for node {0}, {1}, id {2}", node.nodeName, node.Arc.name, node.nodeID); }
                    }
                    else { Debug.LogWarning("Invalid node (Null) in listOfNodes"); }
                }
                /*Debug.LogFormat("[Tst] DijkstraMethods.cs -> Initialise: dictOfNodeD's has {0} records", dictOfNodeD.Count);*/
            }
            else { Debug.LogError("Invalid dictOfNodeD (Null)"); }
        }
        else { Debug.LogError("Invalid listOfNodes (Null)"); }
    }


    /// <summary>
    /// Use standard graphAPI data to set up Dijkstra Graph ready for algorithm. Weighted
    /// </summary>
    private void InitialiseDictDataWeighted()
    {
        //existing nodes
        List<Node> listOfNodes = new List<Node>(GameManager.instance.dataScript.GetDictOfNodes().Values);
        Debug.Assert(listOfNodes.Count == numOfNodes, string.Format("Mismatch on Node Count, listOfNodes {0} vs. numOfNodes {1}", listOfNodes.Count, numOfNodes));
        //set up mirror dijkstra friendly nodes (refered to as 'nodeD')
        List<NodeD> listOfNodeD = new List<NodeD>();
        Dictionary<int, NodeD> dictOfNodeD = GameManager.instance.dataScript.GetDictOfNodeDWeighted();
        if (listOfNodes != null)
        {
            if (dictOfNodeD != null)
            {
                //loop nodes and populate listOfNodeD
                foreach (Node node in listOfNodes)
                {
                    if (node != null)
                    {
                        NodeD nodeD = new NodeD(int.MaxValue, node.nodeID, node.nodeName);
                        //add nodeD to collections
                        if (nodeD != null)
                        {
                            //list
                            listOfNodeD.Add(nodeD);
                            //add to dict
                            try
                            { dictOfNodeD.Add(nodeD.ID, nodeD); }
                            catch (ArgumentNullException)
                            { Debug.LogError("Invalid NodeD (Null)"); }
                            catch (ArgumentException)
                            { Debug.LogError(string.Format("Invalid (duplicate) nodeD.ID \"{0}\" for NodeD \"{1}\"", nodeD.ID, nodeD.Name)); }
                        }
                        else { Debug.LogWarning("Invalid nodeD (Null) -> Failed initialisaction"); }
                    }
                    else { Debug.LogWarning("Invalid node (Null) in listOfNodes"); }
                }
                //loop listOfNodes again  and add Neighbour data to NodeD's
                foreach (Node node in listOfNodes)
                {
                    if (node != null)
                    {
                        //get mirror nodeD
                        NodeD nodeD = null;
                        if (dictOfNodeD.ContainsKey(node.nodeID) == true)
                        { nodeD = dictOfNodeD[node.nodeID]; }
                        if (nodeD != null)
                        {
                            List<int> listOfWeights = new List<int>();
                            List<NodeD> listOfEdges = new List<NodeD>();
                            List<Node> listOfNeighbours = node.GetNeighbouringNodes();
                            if (listOfNeighbours != null)
                            {
                                //loop node neighbours and add edges and weights (assume all are value 1)
                                if (listOfNeighbours.Count > 0)
                                {
                                    foreach (Node neighbour in listOfNeighbours)
                                    {
                                        //create entry for each neighbour
                                        Connection connection = node.GetConnection(neighbour.nodeID);
                                        if (connection != null)
                                        {
                                            //get weight of connection based on Security level
                                            switch(connection.SecurityLevel)
                                            {
                                                case ConnectionType.None:
                                                    listOfWeights.Add(weightNONE);
                                                    break;
                                                case ConnectionType.LOW:
                                                    listOfWeights.Add(weightLOW);
                                                    break;
                                                case ConnectionType.MEDIUM:
                                                    listOfWeights.Add(weightMEDIUM);
                                                    break;
                                                case ConnectionType.HIGH:
                                                    listOfWeights.Add(weightHIGH);
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            //assign a default weight of '1' if a problem
                                            listOfWeights.Add(1);
                                            Debug.LogWarningFormat("Invalid connection (Null) between node {0}, id {1} and neighbour {2}, id {3}", node.nodeName, node.nodeID, neighbour.nodeName, neighbour.nodeID);
                                        }

                                        //find nodeD in dict
                                        NodeD nodeDNeighbour = null;
                                        if (dictOfNodeD.ContainsKey(neighbour.nodeID) == true)
                                        { nodeDNeighbour = dictOfNodeD[neighbour.nodeID]; }
                                        //add to NodeD edge list (neighbouring nodes)
                                        if (nodeDNeighbour != null)
                                        { listOfEdges.Add(nodeDNeighbour); }
                                        else { Debug.LogWarningFormat("Invalid nodeDNeighbour (Null) for neighbour {0}, {1}, id {2}", neighbour.nodeName, neighbour.Arc.name, neighbour.nodeID); }
                                    }
                                    //add to nodeD
                                    nodeD.Adjacency = listOfEdges;
                                    nodeD.Weights = listOfWeights;
                                }
                                else
                                {
                                    //no neighbours
                                    nodeD.Adjacency = listOfEdges;
                                    nodeD.Weights = listOfWeights;
                                }
                            }
                            else { Debug.LogWarningFormat("Invalid listOfNeighbours (Null) for node {0}, {1}, id {2}", node.nodeName, node.Arc.name, node.nodeID); }
                        }
                        else { Debug.LogWarningFormat("Invalid nodeD (Null) for node {0}, {1}, id {2}", node.nodeName, node.Arc.name, node.nodeID); }
                    }
                    else { Debug.LogWarning("Invalid node (Null) in listOfNodes"); }
                }
                /*Debug.LogFormat("[Tst] DijkstraMethods.cs -> Initialise: dictOfNodeD's has {0} records", dictOfNodeD.Count);*/
            }
            else { Debug.LogError("Invalid dictOfNodeD (Null)"); }
        }
        else { Debug.LogError("Invalid listOfNodes (Null)"); }
    }

    /// <summary>
    /// Runs dijkstra algo on all nodes and initialises up main dijkstra collection (dictOfDijkstra). Unweighted
    /// </summary>
    private void InitialiseNodeDataUnweighted()
    {
        int nodeID;
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetDictOfNodes();
        Dictionary<int, NodeD> dictOfNodeD = GameManager.instance.dataScript.GetDictOfNodeDUnweighted();
        Dictionary<int, PathData> dictOfDijkstra = GameManager.instance.dataScript.GetDictOfDijkstraUnweighted();
        if (dictOfNodes != null)
        {
            if (dictOfDijkstra != null)
            {
                if (dictOfNodeD != null)
                {
                    //list Of NodeD's to pass to algo
                    List<NodeD> nodeList = new List<NodeD>(dictOfNodeD.Values);
                    //loop all nodes
                    foreach (var node in dictOfNodes)
                    {
                        if (node.Value != null)
                        {
                            nodeID = node.Value.nodeID;
                            //reset distances in nodeList (otherwise algo gives incorrect data)
                            for (int i = 0; i < nodeList.Count; i++)
                            { nodeList[i].Distance = int.MaxValue; }
                            DijkstraData data = GetShortestPath(nodeID, nodeList);
                            PathData path = new PathData(numOfNodes);
                            if (data != null)
                            {
                                //populate array
                                for (int indexNode = 0; indexNode < numOfNodes; indexNode++)
                                {
                                    path.pathArray[indexNode] = data.arrayOfPaths[indexNode];
                                    path.distanceArray[indexNode] = data.arrayOfDistances[indexNode];
                                }
                                //add entry to dictionary
                                try
                                { dictOfDijkstra.Add(node.Value.nodeID, path); }
                                catch (ArgumentNullException)
                                { Debug.LogError("Invalid NodeD (Null)"); }
                                catch (ArgumentException)
                                { Debug.LogError(string.Format("Invalid (duplicate) node {0}, {1}, id {2}", node.Value.nodeName, node.Value.Arc.name, node.Value.nodeID)); }
                            }
                            else { Debug.LogWarningFormat("Invalid DijkstraData package (Null) for node {0}, {1} ID {2}", node.Value.nodeName, node.Value.Arc.name, nodeID); }
                        }
                        else { Debug.LogWarning("Invalid node (Null)"); }
                    }
                }
                else { Debug.LogError("Invalid dictOfNodeD (null) Unweighted"); }
            }
            else { Debug.LogError("Invalid dictOfDijkstra (Null) Unweighted"); }
        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
    }


    /// <summary>
    /// Runs dijkstra algo on all nodes and initialises up main dijkstra collection (dictOfDijkstra). Weighted
    /// </summary>
    private void InitialiseNodeDataWeighted()
    {
        int nodeID;
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetDictOfNodes();
        Dictionary<int, NodeD> dictOfNodeD = GameManager.instance.dataScript.GetDictOfNodeDWeighted();
        Dictionary<int, PathData> dictOfDijkstra = GameManager.instance.dataScript.GetDictOfDijkstraWeighted();
        if (dictOfNodes != null)
        {
            if (dictOfDijkstra != null)
            {
                if (dictOfNodeD != null)
                {
                    //list Of NodeD's to pass to algo
                    List<NodeD> nodeList = new List<NodeD>(dictOfNodeD.Values);
                    //loop all nodes
                    foreach (var node in dictOfNodes)
                    {
                        if (node.Value != null)
                        {
                            nodeID = node.Value.nodeID;
                            //reset distances in nodeList (otherwise algo gives incorrect data)
                            for (int i = 0; i < nodeList.Count; i++)
                            { nodeList[i].Distance = int.MaxValue; }
                            DijkstraData data = GetShortestPath(nodeID, nodeList);
                            PathData path = new PathData(numOfNodes);
                            if (data != null)
                            {
                                //populate array
                                for (int indexNode = 0; indexNode < numOfNodes; indexNode++)
                                {
                                    path.pathArray[indexNode] = data.arrayOfPaths[indexNode];
                                    path.distanceArray[indexNode] = data.arrayOfDistances[indexNode];
                                }
                                //add entry to dictionary
                                try
                                { dictOfDijkstra.Add(node.Value.nodeID, path); }
                                catch (ArgumentNullException)
                                { Debug.LogError("Invalid NodeD (Null)"); }
                                catch (ArgumentException)
                                { Debug.LogError(string.Format("Invalid (duplicate) node {0}, {1}, id {2}", node.Value.nodeName, node.Value.Arc.name, node.Value.nodeID)); }
                            }
                            else { Debug.LogWarningFormat("Invalid DijkstraData package (Null) for node {0}, {1} ID {2}", node.Value.nodeName, node.Value.Arc.name, nodeID); }
                        }
                        else { Debug.LogWarning("Invalid node (Null)"); }
                    }
                }
                else { Debug.LogError("Invalid dictOfNodeD (null) Weighted"); }
            }
            else { Debug.LogError("Invalid dictOfDijkstra (Null) Weighted"); }
        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
    }

    /// <summary>
    /// Shortest path from source node to all other nodes. Returns Dijkstra data package, null if a problem
    /// </summary>
    public DijkstraData GetShortestPath(int nodeID, List<NodeD> nodeList)
    {
        Debug.Assert(nodeID > -1, "Invalid nodeID (Must be > Zero)");
        DijkstraData data = new DijkstraData();

        if (nodeList != null)
        {
            int[] pi = new int[numOfNodes];
            List<int> S = algorithm.Dijkstra(ref pi, ref nodeList, nodeID);
            data.listOfOrder = S;
            data.arrayOfPaths = pi;
            //create distance array
            int[] arrayOfDistances = new int[numOfNodes];
            for (int i = 0; i < nodeList.Count; i++)
            { arrayOfDistances[nodeList[i].ID] = nodeList[i].Distance; }
            data.arrayOfDistances = arrayOfDistances;
            /*Debug.LogFormat("[Tst] DijkstraMethods -> GetShortestPath: S has {0} records", S.Count);*/
        }
        else { Debug.LogError("Invalid nodeList (Null)"); data = null; }
        return data;
    }

    /// <summary>
    /// given two nodes method returns a list of sequential cnnnections between the two. If 'isReverseOrder' is true then returns connections in order destination to source, otherwise opposite (default)
    /// 'isWeighted' true uses data from dictOfDijkstraWeighted (last calculated), if false then used dictOfDijkstraUnweighted data.
    /// </summary>
    /// <param name="nodeSourceID"></param>
    /// <param name="nodeDestinationID"></param>
    /// <returns></returns>
    public List<Connection> GetPath(int nodeSourceID, int nodeDestinationID, bool isWeighted, bool isReverseOrder = false)
    {
        int nodeCurrentID, nodeNextID;
        bool isError = false;
        List<Connection> listOfConnections = new List<Connection>();
        //get path array (weighted / unweighted)
        PathData data;
        if (isWeighted == false)
        { data = GameManager.instance.dataScript.GetDijkstraPathUnweighted(nodeSourceID); }
        else { data = GameManager.instance.dataScript.GetDijkstraPathWeighted(nodeSourceID); }
        if (data != null)
        {
            //find destinationID in pathArray
            nodeCurrentID = nodeDestinationID;
            do
            {
                nodeNextID = data.pathArray[nodeCurrentID];
                Node nodeCurrent = GameManager.instance.dataScript.GetNode(nodeCurrentID);
                if (nodeCurrent != null)
                {
                    Connection connection = nodeCurrent.GetConnection(nodeNextID);
                    if (connection != null)
                    {
                        //add to list
                        listOfConnections.Add(connection);
                        //swap nodes ready for next iteration
                        nodeCurrentID = nodeNextID;
                    }
                    else
                    {
                        Debug.LogWarningFormat("Invalid connection (Null) between current node {0}, {1}, id {2} and nodeNextID {3}", nodeCurrent.nodeName, nodeCurrent.Arc.name, nodeCurrent, nodeDestinationID);
                        isError = true;
                    }
                }
                else
                {
                    Debug.LogWarningFormat("Invalid nodeSource (Null) for id {0}", nodeCurrentID);
                    isError = true;
                }
            }
            while (isError == false && nodeCurrentID != nodeSourceID);
        }
        else { Debug.LogWarningFormat("Invalid PathData (Null) for nodeSourceID {0}", nodeSourceID); }
        //do I need to reverse the list?
        if (isReverseOrder == false)
        { listOfConnections.Reverse(); }
        return listOfConnections;
    }

    /// <summary>
    /// run whenever connection security levels change. Do so before calculating shortest path.
    /// </summary>
    public void RecalculateWeightedData()
    {
        //empty out collections
        GameManager.instance.dataScript.SetWeightedDijkstraDataClear();
        //recalculate
        InitialiseDictDataWeighted();
        InitialiseNodeDataWeighted();
        //log
        Debug.Log("DijkstraManager.cs -> RecalculateWeightedData: Data successfully recalculated");
    }

    /// <summary>
    /// Gets distance of shortest path between two nodes. Weighted. Returns 0 if a problem
    /// </summary>
    /// <param name="nodeSourceID"></param>
    /// <param name="nodeDestinationID"></param>
    /// <returns></returns>
    public int GetDistanceWeighted(int nodeSourceID, int nodeDestinationID)
    {
        int distance = 0;
        Debug.Assert(nodeSourceID > -1 && nodeSourceID < numOfNodes, "Invalid sourceID (must be between Zero and numOfNodes)");
        Debug.Assert(nodeDestinationID > -1 && nodeDestinationID < numOfNodes, "Invalid destinationID (must be between Zero and numOfNodes)");
        if (nodeSourceID != nodeDestinationID)
        {
            PathData data = GameManager.instance.dataScript.GetDijkstraPathWeighted(nodeSourceID);
            if (data != null)
            { distance = data.distanceArray[nodeDestinationID]; }
            else { Debug.LogErrorFormat("Invalid PathData (Null) for sourceNodeID {0}", nodeSourceID); }
        }
        else { Debug.LogWarningFormat("Invalid nodeID's (sourceID {0} must be Different from destinationID {1})", nodeSourceID, nodeDestinationID); }
        /*Debug.LogFormat("[Tst] DijkstraManager.cs -> GetDistanceWeighted: Distance between nodeID {0} and nodeID {1} is {2}", sourceID, destinationID, distance);*/
        return distance;
    }

    /// <summary>
    /// Gets distance of shortest path between two nodes. Unweighted. Returns 0 if a problem
    /// </summary>
    /// <param name="nodeSourceID"></param>
    /// <param name="nodeDestinationID"></param>
    /// <returns></returns>
    public int GetDistanceUnweighted(int nodeSourceID, int nodeDestinationID)
    {
        int distance = 0;
        Debug.Assert(nodeSourceID > -1 && nodeSourceID < numOfNodes, "Invalid sourceID (must be between Zero and numOfNodes)");
        Debug.Assert(nodeDestinationID > -1 && nodeDestinationID < numOfNodes, "Invalid destinationID (must be between Zero and numOfNodes)");
        if (nodeSourceID != nodeDestinationID)
        {
            PathData data = GameManager.instance.dataScript.GetDijkstraPathUnweighted(nodeSourceID);
            if (data != null)
            { distance = data.distanceArray[nodeDestinationID]; }
            else { Debug.LogErrorFormat("Invalid PathData (Null) for sourceNodeID {0}", nodeSourceID); }
        }
        else { Debug.LogWarningFormat("Invalid nodeID's (sourceID {0} must be Different from destinationID {1})", nodeSourceID, nodeDestinationID); }
        /*Debug.LogFormat("[Tst] DijkstraManager.cs -> GetDistanceUnweighted: Distance between nodeID {0} and nodeID {1} is {2}", sourceID, destinationID, distance);*/
        return distance;
    }

    

    /// <summary>
    /// Debug method that takes two nodeID's and shows a flashing connection path between the two (Unweighted pathing by default, set 'isWeighted' to true for weighted pathing based on last set of calcs)
    /// </summary>
    /// <param name="nodeSourceID"></param>
    /// <param name="nodeDestinationID"></param>
    public string DebugShowPath(int nodeSourceID, int nodeDestinationID, bool isWeighted = false)
    {
        Debug.Assert(nodeSourceID < numOfNodes && nodeSourceID > -1, "Invalid nodeSourceID (must be btwn Zero and max. nodeID #)");
        Debug.Assert(nodeDestinationID < numOfNodes && nodeDestinationID > -1, "Invalid nodeSourceID (must be btwn Zero and max. nodeID #)");
        if (nodeSourceID != nodeDestinationID)
        {
            List<Connection> listOfConnections = GetPath(nodeSourceID, nodeDestinationID, isWeighted);
            if (listOfConnections != null)
            {
                /*//debug print out connections list
                for (int index = 0; index < listOfConnections.Count; index++)
                { Debug.LogFormat("[Tst] DijkstraManager.cs -> DebugShowPath: listOfConnections[{0}] from {1} to {2}", index, listOfConnections[index].GetNode1(), listOfConnections[index].GetNode2()); }*/

                //flash connections
                EventManager.instance.PostNotification(EventType.FlashMultipleConnectionsStart, this, listOfConnections, "DijkstraManager.cs -> DebugShowPath");
            }
            else { Debug.LogWarningFormat("Invalid listOfConnections (Null) for sourceID {0} and destinationID {1}", nodeSourceID, nodeDestinationID); }
        }
        else { Debug.LogWarningFormat("Invalid input nodes (Matching), nodeSourceID {0}, nodeDestinationID {1}", nodeSourceID, nodeDestinationID); }
        return string.Format("Distance {0}", GetDistanceWeighted(nodeSourceID, nodeDestinationID));   /* here for dEbugGUI purposes only*/
    }

    //new methods above here
}
