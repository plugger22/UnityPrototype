﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System.Linq;
using gameAPI;

namespace GraphAPI
{
    /// <summary>
    /// Minimum Spanning Tree operational class
    /// </summary>
    public class LazyPrimMST
    {
        private EdgeWeightedGraph graph;                            //graph
        //Collections
        private bool[] arrayOfMarked;                               //MST vertices
        private Queue<Edge> queueMST;                           //queue of All edges
        private SimplePriorityQueue<Edge> priorityQueue;                //crossing and other ineligible edges

        public float Weight { get; private set; }                   //weight of MST 

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="graph"></param>
        public LazyPrimMST(EdgeWeightedGraph graph)
        {
            int v, w;

            /*//reset collections (in case of followOn level)
            arrayOfMarked = null;
            queueMST = null;
            priorityQueue = null;*/

            this.graph = graph;
            if (graph != null)
            {
                priorityQueue = new SimplePriorityQueue<Edge>();
                arrayOfMarked = new bool[graph.V_total];
                queueMST = new Queue<Edge>();
                //initiate algorithim with arbitary start node '0'
                Visit(0);
                //recursive code
                while( priorityQueue.Count > 0)
                {
                    //get the lowest weight (pq is in ascending order)
                    Edge edge = priorityQueue.Dequeue();
                    //edge from pq
                    v = edge.GetEither();
                    w = edge.GetOther(v);

                    /*if (v == 17 || w == 17)
                    {
                        if (GameManager.instance.inputScript.GameState == GameState.MetaGame)
                        { Debug.LogFormat("[Tst] LazyPrimMST.cs -> Constructor: nodeID {0} to nodeID {1} Edge present{2}", v, w, "\n"); }
                    }*/

                    //skip if ineligible
                    if (arrayOfMarked[v] && arrayOfMarked[w])
                    {
                        /*if (v == 17 || w == 17)
                        {
                            if (GameManager.instance.inputScript.GameState == GameState.MetaGame)
                            { Debug.LogFormat("[Tst] LazyPrimMST.cs -> Constructor: nodeID {0} to nodeID {1} SKIPPED (arrayOfmarked){2}", v, w, "\n"); }
                        }*/
                        continue;
                    }

                    /*if (v == 17 || w == 17)
                    {
                        if (GameManager.instance.inputScript.GameState == GameState.MetaGame)
                        { Debug.LogFormat("[Tst] LazyPrimMST.cs -> Constructor: nodeID {0} to nodeID {1} Edge added to TREE{2}", v, w, "\n"); }
                    }*/

                    //add edge to tree
                    queueMST.Enqueue(edge);
                    //add vertex to tree, either v or w
                    if (arrayOfMarked[v] == false)  { Visit(v); }
                    if (arrayOfMarked[w] == false)  { Visit(w); }
                }
                
            }
            else { Debug.LogError("Invalid graph input (Null)"); }
        }

        /// <summary>
        /// Part of the recursive algorithm, returns appropriate adjacency list
        /// </summary>
        /// <param name="v"></param>
        private void Visit(int v)
        {
            //Mark v and add to pq all edges from v to unmarked vertices
            arrayOfMarked[v] = true;
            foreach (Edge edge in graph.GetAdjList(v))
            {

                /*if (v == 17)
                {
                    if (GameManager.instance.inputScript.GameState == GameState.MetaGame)
                    { Debug.LogFormat("[Tst] LazyPrimMST.cs -> Visit: node1 {0} Edge present in ListOfAdj{1}", v, "\n"); }
                }*/

                if (arrayOfMarked[edge.GetOther(v)] == false)
                {
                    priorityQueue.Enqueue(edge, edge.GetWeight());
                    //Debug.Log(string.Format("Enqueue -> node {0} edge weight {1:F2}", v, edge.GetWeight()));
                }
            }
        }

        /// <summary>
        /// returns a list of all edges in MST
        /// </summary>
        /// <returns></returns>
        public List<Edge> GetEdges()
        { return queueMST.ToList(); }

    }

}
