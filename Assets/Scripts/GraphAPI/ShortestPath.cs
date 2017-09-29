using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

namespace GraphAPI
{
    /// <summary>
    /// Calculates shortest path between two nodes using Dijkstra's algorithm. 
    /// Note: Edge Weights can't be negative.
    /// </summary>
    public class ShortestPath
    {

        private DirectedEdge[] arrayOfEdgesTo;
        private float[] arrayOfDistTo;
        //private SimplePriorityQueue<float> priorityQueue;
        private EdgeWeightedDigraph graph;

        /// <summary>
        /// constructor, takes graph and source vertex
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="source"></param>
        public ShortestPath(EdgeWeightedDigraph G, int source)
        {
            graph = G;
            arrayOfEdgesTo = new DirectedEdge[graph.Vertices];
            arrayOfDistTo = new float[graph.Vertices];

            //priorityQueue = new SimplePriorityQueue<float>(graph.Vertices); -> no implementation of an indexed priority queue available yet
        }
    }

}
