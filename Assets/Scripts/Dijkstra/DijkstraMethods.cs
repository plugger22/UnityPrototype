using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dijkstraAPI
{
    /// <summary>
    /// Operational Methods that bridge between graphAPI and DisjktraAPI
    /// </summary>
    public class DijkstraMethods
    {

        /// <summary>
        /// Use standard graphAPI data to set up Dijkstra Graph ready for algorithm
        /// </summary>
        public void Initialise(List<Node> listOfNodes)
        {
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
                                List<int> weights = new List<int>();
                                List<NodeD> edges = new List<NodeD>();
                                List<Node> listOfNeighbours = node.GetNeighbouringNodes();
                                if (listOfNeighbours != null)
                                {
                                    //loop node neighbours and add edges and weights (assume all are value 1)
                                    if (listOfNeighbours.Count > 0)
                                    {
                                        foreach (Node neighbour in listOfNeighbours)
                                        {
                                            //create entry for each neighbour
                                            weights.Add(1);
                                            //find nodeD in dict
                                            NodeD nodeDNeighbour = null;
                                            if (dictOfNodeD.ContainsKey(neighbour.nodeID) == true)
                                            { nodeDNeighbour = dictOfNodeD[neighbour.nodeID]; }
                                            //add to NodeD edge list (neighbouring nodes)
                                            if (nodeDNeighbour != null)
                                            { edges.Add(nodeDNeighbour); }
                                            else { Debug.LogWarningFormat("Invalid nodeDNeighbour (Null) for neighbour {0}, {1}, id {2}", neighbour.nodeName, neighbour.Arc.name, neighbour.nodeID); }
                                        }
                                    }
                                    else
                                    {
                                        //no neighbours
                                        nodeD.Adjacency = edges;
                                        nodeD.Weights = weights;
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
        /// Shortest path from source node to all other nodes
        /// </summary>
        public void GetShortestPath(int nodeID)
        {
            Debug.Assert(nodeID > -1, "Invalid nodeID (Must be > Zero)");
            Dictionary<int, NodeD> dictOfNodeD = GameManager.instance.dataScript.GetDictOfNodeD();
            if (dictOfNodeD != null)
            {
                List<NodeD> nodeList = new List<NodeD>(dictOfNodeD.Values);
                Algorithm algo = new Algorithm();
                int[] pi = null;
                List<int> S = algo.Dijkstra(ref pi, ref nodeList, nodeID);
                Debug.LogFormat("[Tst] DijkstraMethods -> GetShortestPath: S has {0} records", S.Count);
            }
            else { Debug.LogError("Invalid dictOfNodeD (Null)"); }
        }


        //new methods above here
    }
}
