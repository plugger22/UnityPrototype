using System;
using System.Collections;
using System.Collections.Generic;
using dijkstraAPI;
using gameAPI;
using UnityEngine;

/// <summary>
/// subclass used to pass results of dijkstra algo to the calling method (InitialiseNodeData)
/// </summary>
public class DijkstraData
{
    public int[] arrayOfPaths;                     //corresponds to dijkstra pi[] array
    public List<int> listOfOrder;                  //corresponds to dijkstra List <int> S, list of all other nodes from closest to furtherst
    public int[] arrayOfDistances;                 //list of all NodeD's with distances for the recent ca

    /*/// <summary>
    /// Constructor that auto sizes array to numOfNodes -> NOTE: NO need to initialise collections
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
    private Algorithm algorithm;
    private int numOfNodes;             //used for sizing dijkstra dict arrays



    public void Awake()
    {
        algorithm = new Algorithm();
    }
    /// <summary>
    /// Start sequence
    /// </summary>
    public void Initialise()
    {
        numOfNodes = GameManager.instance.dataScript.CheckNumOfNodes();
        InitialiseDictData();
        InitialiseNodeData();
    }


    /// <summary>
    /// Use standard graphAPI data to set up Dijkstra Graph ready for algorithm
    /// </summary>
    private void InitialiseDictData()
    {
        //existing nodes
        List<Node> listOfNodes = new List<Node>(GameManager.instance.dataScript.GetDictOfNodes().Values);
        Debug.Assert(listOfNodes.Count == numOfNodes, string.Format("Mismatch on Node Count, listOfNodes {0} vs. numOfNodes {1}", listOfNodes.Count, numOfNodes));
        //set up mirror dijkstra friendly nodes (refered to as 'nodeD')
        List<NodeD> listOfNodeD = new List<NodeD>();
        Dictionary<int, NodeD> dictOfNodeD = GameManager.instance.dataScript.GetDictOfNodeD();
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
                Debug.LogFormat("[Tst] DijkstraMethods.cs -> Initialise: dictOfNodeD's has {0} records", dictOfNodeD.Count);
            }
            else { Debug.LogError("Invalid dictOfNodeD (Null)"); }
        }
        else { Debug.LogError("Invalid listOfNodes (Null)"); }
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
    /// Runs dijkstra algo on all nodes and initialises up main dijkstra collection (dictOfDijkstra)
    /// </summary>
    private void InitialiseNodeData()
    {
        int nodeID, indexPath, indexWeighted, indexUnweighted, indexMain;
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetDictOfNodes();
        Dictionary<int, NodeD> dictOfNodeD = GameManager.instance.dataScript.GetDictOfNodeD();
        Dictionary<int, int[,]> dictOfDijkstra = GameManager.instance.dataScript.GetDictOfDijkstra();
        if (dictOfNodes != null)
        {
            if (dictOfDijkstra != null)
            {
                if (dictOfNodeD != null)
                {
                    //array constants
                    indexMain = (int)NodeDijkstra.Count;
                    indexPath = (int)NodeDijkstra.Path;
                    indexUnweighted = (int)NodeDijkstra.UnWeighted;
                    indexWeighted = (int)NodeDijkstra.Weighted;
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
                            if (data != null)
                            {
                                //create data array
                                int[,] arrayOfData = new int[indexMain, numOfNodes];
                                //populate array
                                for (int indexNode = 0; indexNode < numOfNodes; indexNode++)
                                {
                                    arrayOfData[indexPath, indexNode] = data.arrayOfPaths[indexNode];
                                    //unweighted distance to indexed Node (all connections assumed to have a weight of 1)
                                    arrayOfData[indexUnweighted, indexNode] = data.arrayOfDistances[indexNode];
                                    //default data for weighted (not yet calculated)
                                    arrayOfData[indexWeighted, indexNode] = -1;
                                }
                                //add entry to dictionary
                                try
                                { dictOfDijkstra.Add(node.Value.nodeID, arrayOfData); }
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
                else { Debug.LogError("Invalid dictOfNodeD (null)"); }
            }
            else { Debug.LogError("Invalid dictOfDijkstra (Null)"); }
        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
    }

    /// <summary>
    /// Debug method that takes two nodeID's and shows a flashing connection path between the two
    /// </summary>
    /// <param name="nodeSourceID"></param>
    /// <param name="nodeDestinationID"></param>
    public void DebugShowPath(int nodeSourceID, int nodeDestinationID)
    {
        Debug.Assert(nodeSourceID < numOfNodes && nodeSourceID > -1, "Invalid nodeSourceID (must be btwn Zero and max. nodeID #)");
        Debug.Assert(nodeDestinationID < numOfNodes && nodeDestinationID > -1, "Invalid nodeSourceID (must be btwn Zero and max. nodeID #)");
        List<Connection> listOfConnections = new List<Connection>();
        Node nodeSource = GameManager.instance.dataScript.GetNode(nodeSourceID);
        if (nodeSource != null)
        {
            Connection connection = nodeSource.GetConnection(nodeDestinationID);
            if (connection != null)
            {
                //add to list
                listOfConnections.Add(connection);
            }
            else { Debug.LogWarningFormat("Invalid connection (Null) between node {0}, {1}, id {2} and nodeDestinationID {3}", nodeSource.nodeName, nodeSource.Arc.name, nodeSource, nodeDestinationID); }
        }
        else { Debug.LogWarningFormat("Invalid nodeSource (Null) for id {0}", nodeSourceID); }
    }

    //new methods above here
}
